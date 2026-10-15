import tempfile
import unittest
from pathlib import Path
from version import bump_version, update_version


class VersionTests(unittest.TestCase):
    def test_semantic_increments(self):
        self.assertEqual(bump_version("1.2.3", "Major"), "2.0.0")
        self.assertEqual(bump_version("1.2.3", "Minor"), "1.3.0")
        self.assertEqual(bump_version("1.2.3", "Fix"), "1.2.4")
        for version, bump in [("1.2.3-rc.1", "Fix"), ("01.2.3", "Fix"), ("1.2", "Fix"), ("1.2.3", "unknown")]:
            with self.assertRaises(ValueError):
                bump_version(version, bump)

    def test_updates_both_files_and_rejects_mismatch_without_writing(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "src/GhosttyPlugin.csproj"
            metadata = root / "src/package/metadata/LoupedeckPackage.yaml"
            metadata.parent.mkdir(parents=True)
            project.write_text("<Project><Version>0.1.0</Version></Project>")
            metadata.write_text("name: Ghostty\nversion: 0.1.0\n")
            self.assertEqual(update_version(root, "Minor"), "0.2.0")
            self.assertIn("<Version>0.2.0</Version>", project.read_text())
            self.assertIn("version: 0.2.0", metadata.read_text())
            metadata.write_text("version: 9.0.0\n")
            previous = project.read_text(), metadata.read_text()
            with self.assertRaises(ValueError):
                update_version(root, "Fix")
            self.assertEqual(previous, (project.read_text(), metadata.read_text()))


if __name__ == "__main__":
    unittest.main()
