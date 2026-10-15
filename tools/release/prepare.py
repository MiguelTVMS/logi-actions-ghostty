"""Prepare a release snapshot from develop without changing any remote branch."""
import argparse
import json
import re
import subprocess
from pathlib import Path
from version import update_version


def git(root, *args):
    return subprocess.check_output(["git", *args], cwd=root, text=True).strip()


def prepare(root, bump, expected_sha):
    source = git(root, "rev-parse", "HEAD")
    if source != expected_sha or git(root, "branch", "--show-current") != "develop":
        raise ValueError("Release must start at the dispatched develop commit")
    if git(root, "status", "--porcelain"):
        raise ValueError("Release checkout must be clean")
    from xml.etree import ElementTree
    current = ElementTree.parse(root / "src/GhosttyPlugin.csproj").getroot().find("PropertyGroup/Version").text
    versions = [current]
    versions.extend(tag[1:] for tag in git(root, "tag", "--list", "v*").splitlines()
                    if re.fullmatch(r"v\d+\.\d+\.\d+", tag))
    base = max(versions, key=lambda version: tuple(map(int, version.split("."))))
    version = update_version(root, bump, base)
    tag = f"v{version}"
    if tag in git(root, "tag", "--list").splitlines():
        raise ValueError(f"Release tag already exists: {tag}")
    main = subprocess.run(["git", "rev-parse", "--verify", "refs/remotes/origin/main"],
                          cwd=root, capture_output=True, text=True)
    if main.returncode not in (0, 128):
        raise ValueError("Unable to resolve production branch")
    main_sha = main.stdout.strip() if main.returncode == 0 else ""
    git(root, "add", "src/GhosttyPlugin.csproj", "src/package/metadata/LoupedeckPackage.yaml")
    tree = git(root, "write-tree")
    # The released tree is exactly develop plus the version bump. Preserve main's
    # history as first parent, so updating main is fast-forward and needs no PR.
    parents = ["-p", main_sha, "-p", source] if main_sha and main_sha != source else ["-p", source]
    commit = git(root, "commit-tree", tree, *parents, "-m", f"chore: release {tag}")
    git(root, "switch", "-c", "feature/release-build", commit)
    return {"version": version, "tag": tag, "package": f"Ghostty_{version}.lplug4",
            "commit": commit, "main_sha": main_sha, "source": source}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("bump", choices=["Major", "Minor", "Fix"])
    parser.add_argument("source_sha")
    args = parser.parse_args()
    print(json.dumps(prepare(Path(__file__).resolve().parents[2], args.bump, args.source_sha)))
