# Ghostty default profile

Create your layout in the plugin-linked Ghostty application profile in Logi Options+. Put a **Settings** folder in the top Actions Ring slot, with **Open Settings** and **Reload Settings** inside it.

Export the profile through Options+, then run:

```sh
./repo profile /absolute/path/to/export.lp5
./repo check
```

The copy command verifies the export is an Actions Ring profile associated with Ghostty and contains both Settings actions. It copies the file unchanged to `src/package/profiles/DefaultProfile72.lp5`, replacing the previous bundled default if one exists. It rejects missing files, invalid archives, other devices/applications, and personal filesystem paths in the profile data. It does not edit your active Options+ profile or re-author the archive.

VS Code also provides **Tasks: Run Task → Copy exported default profile**, which prompts for the exported file path.

The build includes this file automatically. Logitech uses it on initial application-profile creation and as a template for new profiles. Updates do not overwrite users' existing layouts. The maintainer-tested `Default Ghostty Profile_ring.lp5` export is bundled unchanged as `src/package/profiles/DefaultProfile72.lp5`. Native actions and layout were confirmed working on 2026-10-15 with Ghostty 1.3.1 and Plugin API 6.4.2.3414.

After export, review the profile for personal information before committing. Document the Logi Options+ and Ghostty versions used alongside the export. Native installation/default-profile acceptance must be checked after the actual export is bundled.

Windows profiles will be developed when official Ghostty Windows support becomes available and native behavior has been validated.

Reference: https://logitech.github.io/actions-sdk-docs/csharp/plugin-features/default-application-profiles/

## Folder icons

Import [Window.svg](icons/Window.svg) and [Split.svg](icons/Split.svg) individually through the folder icon editor. Both have transparent backgrounds and black outline contours for recoloring. Use background `#149CFF` and icon `#FFFFFF`. These are MIT-licensed Tabler icons; upstream sources and license are under `assets/icons/tabler/`. The existing exported profile is unchanged. Export it again after assigning these icons and adding the new actions.

[Focus.svg](icons/Focus.svg) and [Tab.svg](icons/Tab.svg) provide matching icons for focus/navigation and tab folders. Import them individually and use the same blue background and white foreground. Original Tabler sources are `focus-2.svg` and `tabs.svg`.

[Clipboard.svg](icons/Clipboard.svg) is the matching clipboard folder icon, derived from the MIT-licensed Tabler `clipboard.svg`. Import it in the folder icon editor and use background `#149CFF` with icon `#FFFFFF`.

[Search.svg](icons/Search.svg) and [Terminal.svg](icons/Terminal.svg) provide matching folder icons, using the MIT-licensed Tabler `search` and `terminal-2` artwork. Import them individually and use background `#149CFF` with icon `#FFFFFF`.
