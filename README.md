# Logi Actions for Ghostty

MIT-licensed C# Logi Actions SDK plugin for Ghostty, targeting macOS and Logitech Actions Ring / compatible MX devices.

## Actions

Settings actions appear under **Settings**:

| Action | Behavior |
| --- | --- |
| Open Settings | Runs Ghostty `open_config`, opening its configuration in the default editor |
| Reload Settings | Runs Ghostty `reload_config`, applying reloadable settings from disk |

Requires Ghostty 1.3.0 or later, AppleScript enabled (`macos-applescript = true`, the default), and an existing Ghostty terminal window. macOS may request Automation permission for Logi Plugin Service / its automation process to control Ghostty. Failures are logged through the Logitech plugin log. Open Settings uses Ghostty's editor behavior; Reload Settings does not save an editor's unsaved changes. Some configuration settings require a new terminal or app restart.

The integration invokes Ghostty's native AppleScript API, so remapped keyboard shortcuts do not affect these actions. Native Logitech/device acceptance is still pending.

## Development in VS Code

Install the .NET SDK specified by `global.json` and a current Logi Options+ installation. The C# SDK is `PluginApi.dll` supplied by Logi Plugin Service, rather than a standalone NuGet SDK package. This repo was built against the locally installed service **6.4.2.3414**. Its API targets .NET 10; the official generator currently creates net8.0 projects, so this project deliberately targets net10.0.

Open the repository folder in VS Code and accept the recommended extensions: Codex, C# Dev Kit, C#, .NET Install Tool, Logi Command Manager, EditorConfig, YAML, and GitHub tooling. Cmd+Shift+B builds the solution. Use **Tasks: Run Task** for SDK checks, local linking, watching, packaging, verification, and unlinking.

```sh
dotnet tool restore
dotnet run --project tools/SdkCheck
dotnet build GhosttyPlugin.sln -c Release
./repo test
dotnet tool run logiplugintool -- pack bin/Release Ghostty_0.1.0.lplug4
dotnet tool run logiplugintool -- verify Ghostty_0.1.0.lplug4
```

Before each development session, check for Logi Options+ updates and review current SDK documentation. `SdkCheck` checks the live NuGet generator version and reports the installed Plugin API version. It cannot verify whether the installed host is the latest release. Update the generator with `dotnet tool update logiplugintool --local`, then review generated changes and rebuild. Never commit or package the host's proprietary API assembly. Override its location with the MSBuild `PluginApiDir` property; the checker accepts an environment variable with the same name.

The required .NET 10 SDK and .NET 8 tool runtime are installed in the current user’s `~/.dotnet` directory. The repo command discovers this location, VS Code tasks/terminals include it in PATH, and the login shell exposes it through `~/.zprofile`. Quit and reopen VS Code after installing .NET so C# Dev Kit does not retain a failed SDK discovery. Fresh clones still require these runtimes. No temporary SDK path is hardcoded.

## Validation commands and Git gates

Use the same entry point locally, in VS Code, in Git hooks, and in CI:

| Command | Validation |
| --- | --- |
| `./repo check` | SDK tooling freshness, translations, action icons, tests/coverage, Release build, package and verify |
| `./repo test` | Run automated tests |
| `./repo coverage` | Require at least 90% production line coverage; write Cobertura report under `artifacts/coverage/` |
| `./repo translations` | Require 100% action/group/description translations for en-US and pt-BR, checked with Logitech's parser |
| `./repo icons` | Require an outlined SVG symbol and matching blue/white button template for every action |
| `./repo profile <exported-file.lp5>` | Validate and copy an authentic Ghostty Actions Ring export into the package |
| `./repo hooks` | Enable this clone's pre-commit and pre-push hooks |

Coverage covers the production GhosttyPlugin assembly, including the SDK adapter, shared code, and platform implementations. No production source exclusions are configured. Empty or stale reports cannot satisfy the gate. Branch coverage is reported alongside the enforced line threshold.

Both Git hooks invoke the full `./repo check` gate. The commit hook also requires a Gitflow work branch. The push hook rejects direct pushes to develop/main, invalid branch names, a dirty working tree, and commits other than the validated HEAD. Release tags must point at the release branch's validated HEAD. Enable hooks once per clone; Git does not install hooks merely by cloning. GitHub protection independently requires PRs and the **Quality gate** status for develop, including administrators.

## Languages

The initial supported languages are **English (US), en-US**, and **Portuguese (Brazil), pt-BR**. Translation files live in `src/package/localization/` and are packaged automatically. The plugin follows the Logi Options+ language, with English fallback. pt-PT is not an alias for pt-BR.

| English | Portuguese (Brazil) |
| --- | --- |
| Settings | Configurações |
| Open Settings | Abrir configurações |
| Reload Settings | Recarregar configurações |

Descriptions are translated too. English source strings are SDK translation keys, so keep both catalogs synchronized whenever UI strings change. The validation command discovers current command classes from the compiled plugin and verifies exact IDs, matching sources, nonempty targets, and SDK lookup results. Native plugin loading and visibility in Logi Options+ have been verified. The initial loading failure was caused by a missing ClientApplication subclass; the host requires exactly one even for a universal plugin. GhosttyApplication now provides that contract, with a regression test. The maintainer confirmed Settings actions work on 2026-10-15; debugger attachment acceptance remains separate.

## Gitflow

`develop` is the default/integration branch. Create `feature/*` branches from develop and merge code into develop only through a PR. The initial setup remains on `feature/initial-setup`; nothing has been pushed and no PR has been created.

Create `release/*` branches from develop for release stabilization and publication. Release fixes return to develop through PRs. Introduce `main` with the first production version and protect it before accepting changes. Reserve `hotfix/*` for production fixes and backport by PR. Releases must be created from a validated release branch. Creating PRs, pushing, and publishing remain explicit user actions.

## Local testing and debugging

Builds and packaging leave local plugin registration alone. Use **Build and link plugin**, or:

```sh
dotnet build GhosttyPlugin.sln -p:EnablePluginLink=true
```

This writes `GhosttyPlugin.link` in the user Logi Plugin Service Plugins directory and requests a reload. In Options+, assign **Open Settings** and **Reload Settings** from the **Settings** group to controls. Verify opening the configuration and reloading a saved, reversible setting. Also verify error handling with no Ghostty window or denied Automation permission.

For debugging, select **Attach to Logi Plugin Service** in VS Code and choose the `LogiPluginService` process. This DLL runs inside that host, not as a standalone application. Development builds include symbols.

Watch mode retains the development link after stopping. Remove it with the **Unlink plugin** task or `dotnet msbuild src/GhosttyPlugin.csproj -t:UnlinkPlugin`, then restart the service if needed.

## Architecture and Windows roadmap

Logitech actions call `IGhosttyController`. `GhosttyControllerCommon` handles asynchronous process execution, failures, and timeouts. `GhosttyControllerMacOs` translates operations to native Ghostty AppleScript actions. A single factory selects the platform implementation.

We will monitor official Ghostty development on Windows during project maintenance and SDK reviews, and develop Windows support when an official version becomes available. The design must always keep that future support in mind. `GhosttyControllerWindows` inherits the shared controller but explicitly reports unsupported operations today. Windows is not advertised in package metadata. Future support requires an actual Windows implementation, platform-specific profiles where needed, and native Windows acceptance before release.

## Profiles and CI

Authentic Logitech UI exports belong in `src/package/profiles/`, which is copied into the package. See `profiles/README.md` for the export process. The plugin is associated with Ghostty (`HasNoApplication = false`). Its default Actions Ring layout will be bundled after you export it from Options+ and run `./repo profile <exported-file.lp5>`.

Automatic CI runs `./repo check` on a hosted macOS runner. Without an installed host, the repo command resolves PluginApi.dll from the pinned official LogiPluginTool package in the NuGet cache. This verifies compatibility with the tool-distributed API, rather than the current installed host API. The manually triggered **Native Logitech build** runs the same command on a trusted self-hosted macOS runner labelled `logi-sdk` with current Logi Options+ installed. Such a runner has not been configured by this task. Native device acceptance is separate from both jobs.

The temporary plugin icon is original outlined ghost artwork with a transparent background. SVG sources and rendering instructions are documented in `assets/branding/README.md`. Action buttons default to blue backgrounds with white outlined SVGs and can be recolored in Logitech’s icon editor. The repository's existing MIT license applies to our source code; Logitech's SDK remains subject to its own terms.

## References

- [Logitech C# development guide](https://logitech.github.io/actions-sdk-docs/csharp/plugin-development/introduction/)
- [Logitech packaging and verification](https://logitech.github.io/actions-sdk-docs/csharp/plugin-development/distributing-the-plugin/)
- [Ghostty AppleScript API](https://ghostty.org/docs/features/applescript)
- [Ghostty settings actions](https://ghostty.org/docs/config/keybind/reference)
- [Ghostty platform support](https://ghostty.org/docs/features)

Window Management includes Split Left, Split Right, Split Up, Split Down, New Tab, and New Window. Splits and New Tab target the front Ghostty window and require an existing terminal. New Window also works without an existing window. Each action has en-US and pt-BR names/descriptions and matching compact Tabler outline icons. Native execution of these actions still requires acceptance in Options+.

## Additional actions

| Group | Actions |
| --- | --- |
| Window Management | Focus Split Left, Focus Split Right, Focus Split Up, Focus Split Down, Toggle Split Zoom, Equalize Splits, Previous Tab, Next Tab, Previous Window, Next Window |
| Appearance | Increase Font Size, Decrease Font Size, Reset Font Size |
| Search | Find, Find Selection, Next Search Result, Previous Search Result |
| Terminal | Open Scrollback in Editor |

Font size changes use one-point increments. Find Selection requires selected text; result navigation requires an active search. Split navigation requires a neighboring pane. Scrollback opens a temporary text file through the default editor. All target the focused terminal in the front Ghostty window. New actions include icons and en-US/pt-BR translations; the maintainer confirmed native device acceptance on 2026-10-15.

**Toggle Secure Keyboard Entry** is in Terminal. It runs Ghostty `toggle_secure_input`, affecting the entire Ghostty application, and persists across Ghostty restarts until toggled off. Ghostty temporarily releases active secure input while it is in the background. It requires an open Ghostty terminal. This command toggles the native state; the button does not display an on/off indicator. The maintainer confirmed native acceptance on 2026-10-15.

## Clipboard actions

| Action | Behavior |
| --- | --- |
| Copy | Copy selected text in multiple clipboard formats |
| Copy as Plain Text | Copy selected text without formatting |
| Copy as HTML | Copy selected text with HTML formatting |
| Copy with Terminal Formatting | Copy selected text with terminal escape sequences |
| Copy URL | Copy the URL under the terminal pointer |
| Copy Terminal Title | Copy the title of the focused terminal |
| Paste | Paste from the system clipboard using Ghostty paste handling |
| Paste from Selection | Paste from Ghostty selection clipboard |

Copy uses Ghostty mixed formats, letting the receiving application choose a representation. Plain Text removes formatting; HTML retains rich formatting; Terminal Formatting includes VT escape sequences. Copy variants require selected text. Copy URL requires a link under the terminal pointer; Copy Terminal Title requires a nonempty terminal title. Paste from Selection uses Ghostty’s selection clipboard rather than an arbitrary system-wide text selection. Clipboard actions use native Ghostty handling, including configured paste protections, and require an open terminal. No clipboard contents are read or logged by the plugin. The maintainer confirmed native acceptance on 2026-10-15.

## Release workflow

The **Release** workflow has one parameter: **Version** (`Major`, `Minor`, `Fix`, default `Fix`). Every release is stable and marked latest. There is no prerelease option. Signing and App Store controls do not apply to this Logitech plugin.

After this workflow has entered `develop` through a PR, create a `release/*` branch from `develop`. In GitHub Actions, select **Release → Run workflow**, choose that release branch and the increment. Dispatching authorizes publication; the workflow rejects every other branch. GitHub requires the dispatch workflow to exist on the repository default branch before it can be run.

Before the first production release, create and protect `main`. Its protection must permit the release automation identity to integrate validated releases directly, without requiring a PR to main. If the default GitHub Actions identity cannot do so, configure a dedicated `RELEASE_TOKEN` secret for an authorized release identity. The workflow honors protection and never disables it. This is an explicit exception for release automation; changes into `develop` still require a PR.

The workflow synchronizes the assembly and Logitech versions, commits the bump on the release branch, merges existing `main` history into that branch, and runs `./repo check` on the resulting commit. After validation, it atomically advances the release branch and `main` to that same HEAD and pushes its `vX.Y.Z` tag. It rejects concurrent changes to either branch and never pushes to `develop`. No PR to main is created. Return release/version changes to `develop` through a PR.

The GitHub release contains `Ghostty_X.Y.Z.lplug4` and its SHA-256 checksum, with generated release notes. Build artifacts also include coverage. It does not submit to Logitech Marketplace.

For local preparation, run `./repo release-version Major`, `Minor`, or `Fix`, or use VS Code's **Bump release version** task. This modifies the two version files locally and does not commit or publish. `./repo check` derives the package filename from their matching version. Python 3 is required for the version helper and its shared gate tests.

Hosted release builds use the official pinned generator API compatibility fallback; they do not establish host freshness or replace native Logi Options+ acceptance. SDK reviews and native acceptance remain prerequisites for release. If publication fails after the atomic push, inspect the existing tag and artifact before recovery; rerunning against a changed branch is intentionally rejected.

Native acceptance: on 2026-10-15, the maintainer confirmed all current actions and the exported Actions Ring layout worked in Logi Options+, using Ghostty 1.3.1 and Plugin API 6.4.2.3414. The profile is bundled unchanged as `src/package/profiles/DefaultProfile72.lp5`. Automated compatibility builds remain separate from this manual acceptance.

Dependency policy: GitHub Actions use stable major tags; NuGet references use stable major ranges (`18.*`, `2.*`, `4.*`, `10.*`) with committed lock files for reproducible CI. Refresh packages with `dotnet restore GhosttyPlugin.sln --force-evaluate`, review the lock changes, and run `./repo check`. The .NET SDK uses stable-only roll-forward within major 10; global.json and the local tool manifest retain concrete versions because their formats require them. Current NuGet package identities remain unchanged.
