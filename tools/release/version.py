"""Keep the plugin assembly and Logitech package versions synchronized."""
import argparse
import re
from pathlib import Path


def bump_version(version, bump):
    if not re.fullmatch(r"(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)", version):
        raise ValueError("Expected a numeric major.minor.patch version")
    major, minor, patch = map(int, version.split("."))
    if bump == "Major":
        major, minor, patch = major + 1, 0, 0
    elif bump == "Minor":
        minor, patch = minor + 1, 0
    elif bump == "Fix":
        patch += 1
    else:
        raise ValueError("Version increment must be Major, Minor, or Fix")
    return f"{major}.{minor}.{patch}"


def update_version(root, bump, base_version=None):
    project = root / "src/GhosttyPlugin.csproj"
    metadata = root / "src/package/metadata/LoupedeckPackage.yaml"
    project_text, metadata_text = project.read_text(), metadata.read_text()
    project_versions = re.findall(r"<Version>([^<]+)</Version>", project_text)
    package_versions = re.findall(r"(?m)^version: (\S+)$", metadata_text)
    if len(project_versions) != 1 or package_versions != project_versions:
        raise ValueError("Assembly and package must have one matching version")
    version = bump_version(base_version or project_versions[0], bump)
    # Validate both files before writing either one.
    project.write_text(project_text.replace(f"<Version>{project_versions[0]}</Version>", f"<Version>{version}</Version>"))
    metadata.write_text(re.sub(r"(?m)^version: \S+$", f"version: {version}", metadata_text))
    return version


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("bump", choices=["Major", "Minor", "Fix"])
    args = parser.parse_args()
    print(update_version(Path(__file__).resolve().parents[2], args.bump))
