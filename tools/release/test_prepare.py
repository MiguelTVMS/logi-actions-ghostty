import subprocess
import tempfile
import unittest
from pathlib import Path
from prepare import git, prepare


class PrepareTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        subprocess.run(["git", "init", "-b", "develop", self.temp.name], check=True, capture_output=True)
        git(self.root, "config", "commit.gpgsign", "false")
        git(self.root, "config", "user.name", "Release test")
        git(self.root, "config", "user.email", "release@example.invalid")
        project = self.root / "src/GhosttyPlugin.csproj"
        metadata = self.root / "src/package/metadata/LoupedeckPackage.yaml"
        metadata.parent.mkdir(parents=True)
        project.write_text("<Project><PropertyGroup><Version>0.1.0</Version></PropertyGroup></Project>")
        metadata.write_text("version: 0.1.0\n")
        git(self.root, "add", ".")
        git(self.root, "commit", "-m", "Initial")
        self.source = git(self.root, "rev-parse", "HEAD")

    def test_first_release_keeps_develop_unchanged(self):
        result = prepare(self.root, "Fix", self.source)
        self.assertEqual(result["tag"], "v0.1.1")
        self.assertEqual(result["main_sha"], "")
        self.assertEqual(git(self.root, "rev-parse", "develop"), self.source)
        self.assertEqual(git(self.root, "status", "--porcelain"), "")
        self.assertEqual(git(self.root, "rev-parse", "HEAD"), result["commit"])

    def test_repeat_release_uses_latest_tag_and_preserves_main_history(self):
        first = prepare(self.root, "Fix", self.source)
        git(self.root, "tag", first["tag"], first["commit"])
        git(self.root, "update-ref", "refs/remotes/origin/main", first["commit"])
        git(self.root, "switch", "develop")
        git(self.root, "branch", "-D", "feature/release-build")
        second = prepare(self.root, "Fix", self.source)
        self.assertEqual(second["tag"], "v0.1.2")
        self.assertEqual(git(self.root, "rev-parse", "HEAD^1"), first["commit"])
        self.assertEqual(git(self.root, "rev-parse", "HEAD^2"), self.source)
        self.assertEqual(git(self.root, "diff", "--name-only", self.source, "HEAD").splitlines(),
                         ["src/GhosttyPlugin.csproj", "src/package/metadata/LoupedeckPackage.yaml"])

    def test_wrong_source_branch_and_dirty_tree_fail(self):
        with self.assertRaises(ValueError):
            prepare(self.root, "Fix", "0" * 40)
        (self.root / "uncommitted.txt").write_text("dirty")
        with self.assertRaises(ValueError):
            prepare(self.root, "Fix", self.source)
        git(self.root, "switch", "-c", "feature/wrong")
        with self.assertRaises(ValueError):
            prepare(self.root, "Fix", self.source)
