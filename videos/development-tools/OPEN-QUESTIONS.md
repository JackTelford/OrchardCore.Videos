# Open questions: development-tools

What the narration says that rests on the documentation alone, or on a tool that is not part of the Orchard Core source
(`92db4a83`: no `.github` folder, no solution file, no NuGet.config at the root). Each item says what was checked and what
a person must confirm on a machine before the video is recorded.

## 1. The preview feed and its build

Checked: `src/docs/getting-started/preview-package-source.md:3-16` (daily build, the Preview - CI workflow, Cloudsmith,
no credentials, the address). The workflow files are not in the source that was read.

Narrated as facts: the daily build publishes preview packages to Cloudsmith when the main branch has changes; the Preview
CI workflow can be run manually; restoring packages from the feed needs no credentials; preview packages are not kept
forever.

To confirm:

- On GitHub, Actions of `OrchardCMS/OrchardCore`: a workflow named "Preview - CI", on a daily schedule and with a manual
  run.
- `curl -s -o /dev/null -w "%{http_code}" https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json` answers 200
  without a credential.
- A recent preview package exists on the feed. Its version ends with the preview suffix, as the narration of `use-1` says
  (the source sets `VersionSuffix` to `preview`, and adds `-$(BuildNumber)` when a build number is given:
  `src/OrchardCore.Build/OrchardCore.Commons.props:5-7`).

## 2. Two feeds or one

`preview-package-source.md:12` says both feeds receive the same packages and that you choose either, and `:21` says
"either NuGet v3 source URL from the table above", but the table (`:14-16`) lists one feed, Cloudsmith. The video
shows and narrates the Cloudsmith feed only. Confirm whether a second feed exists, and if so add it to the video and to
the documentation.

## 3. Visual Studio labels

Visual Studio is not part of the Orchard Core source, so these names come from the documentation only:

- Tools, NuGet Package Manager, Package Manager Settings (`preview-package-source.md:20`);
- Package Sources, Name, Source, Update, OK, and the plus button, read from the screenshot
  `src/docs/getting-started/assets/add-preview-package-source.png`, which shows the Options dialog;
- Manage NuGet packages, Include prerelease, Package Source (`src/docs/getting-started/README.md:18`, which spells
  "Manage NuGet packages..." with a lower-case "p").

Confirm each one, and its spelling, in an installed Visual Studio: the slides `vs-1` and `use-1` and the narration of both
say them aloud. Also confirm that adding a source with the plus button, then Name, Source and Update, is the order the
dialog asks for.

## 4. Packing the source code

Checked: `src/docs/topics/local-nuget-packages/README.md:9-20`, and in the source: every project gets its version from
`OrchardCore.Commons.props` (`VersionPrefix` and `VersionSuffix`), `OrchardCore.Application.Cms.Targets` and
`OrchardCore.Application.Cms.Core.Targets` turn their project references into package references when packed
(`...Targets.csproj:24-25` and `:30-31`), and the project template replaces `${TemplateOrchardVersion}` with `$(Version)`
(`OrchardCore.ProjectTemplates.csproj:90`), so a template packed from the source defaults to the version of that source.

To confirm in a clone of `OrchardCMS/OrchardCore`:

- `dotnet pack -c Release -o <folder>` run at the root of the clone succeeds (the source that was read has no solution
  file, so confirm which solution or project file sits at the root and that `dotnet pack` finds exactly one), and the
  folder holds one package per packable project, `OrchardCore.Application.Cms.Targets` among them. Note whether a second solution file or a
  `.snupkg` symbol package for each project (`IncludeSymbols` and `SymbolPackageFormat` in `OrchardCore.Commons.props:29-30`)
  changes what the next step copies.
- The version in the file names of the output is the version prefix and the version suffix, without a build number
  (at the commit that was read: `4.0.0` and `preview`). The narration of `local-5` says "each package takes the version
  of the source you packed", and the slide says the version comes from a prefix and a suffix in `OrchardCore.Commons.props`.
- `OrchardCore.Application.Cms.Targets` reaches the modules through `OrchardCore.Application.Cms.Core.Targets` and
  references the front-end themes itself, so the narration says one reference "restores them from your feed". It also
  references `OrchardCore.Translations.All` (`OrchardCore.Application.Cms.Targets.csproj:42`), a package that is not
  built from this source; its version is not set in the project file. Confirm it restores from nuget.org, which is why
  the sample `NuGet.config` keeps nuget.org next to `MyFeed`.
- `nuget init <output folder> <feed folder>` is a command of the NuGet command line tool (`nuget.exe`), not of the
  `dotnet` CLI. Confirm that it copies the packages as the narration of `local-3` implies (the first folder is the source,
  the second the feed), that a UNC path works, and that a plain local folder also works as a feed.
- The `NuGet.config` of `local-4` restores a project from the feed. Note that the documentation's own sample has
  `<clear />`, which the narration does not explain.
- Re-packing the same version a second time: confirm whether the global packages folder of NuGet keeps the first copy.
  The video does not cover it.
- `dotnet new occms -n MySite --orchard-version <version>` creates a project whose `PackageReference` of
  `OrchardCore.Application.Cms.Targets` has that version (template: `template.json:49-55`, `.csproj:31`). Confirm that the
  `occms` template is installed from the same source, or from another one, in this scenario.

## 5. smtp4dev

Checked: `src/docs/getting-started/development-tools.md:17` (a small SMTP server, installed with `dotnet`, with a web
interface for the processed emails), and, in Orchard Core, Settings, Communication, Email
(`OrchardCore.Email/AdminMenu.cs:38-40`), the SMTP tab (`SmtpSettingsDisplayDriver.cs:88`), and the labels Enable SMTP
Provider, Host name and Port number (`SmtpSettings.Edit.cshtml:10`, `:50`, `:59`).

Also checked: with Delivery method set to Network (the first value of `SmtpDeliveryMethod`, so the default), the SMTP tab
shows Host name and Port number; saving with the SMTP provider enabled requires a Sender email address
(`SmtpSettingsDisplayDriver.cs:120-122`) and a Host name (`:129-133`), and enabling the only provider makes it the
default provider (`:193-199`). The SMTP tab exists only when the feature "SMTP Email Provider"
(`OrchardCore.Email.Smtp/Manifest.cs:4`, `Startup.cs:29`) is enabled; the slide `tools-4` says so, the narration does not.

To confirm: that the feature appears as "SMTP Email Provider" on the Features page (Tools, Features,
`OrchardCore.Features/AdminMenu.cs:27`) and enabling it adds the SMTP tab under Settings, Communication, Email; the
dotnet tool command that installs smtp4dev, the SMTP port and the web interface port it listens on, and that its name
is spelled "smtp4dev" in lower case. The narration gives neither the command nor the ports.

## 6. DB Browser for SQLite and the database file name

The documentation (`development-tools.md:16`) says `yessql.db`. The code names the file `OrchardCore.db` for a site
created by setup (`SetupService.cs:178-181`), and uses `yessql.db` only for a tenant whose settings have no
`DatabaseName` (`SqliteHelper.cs:21-26`). The narration says "by default, setup names it OrchardCore.db", and the
slide `tools-3` adds the fallback. Confirm on a fresh site made with Auto Setup and SQLite:
`App_Data/Sites/Default/OrchardCore.db` exists, and DB Browser for SQLite shows a `Document` table whose `Content`
column holds JSON.

## 7. Pronunciation

The `"spoken"` map of `storyboard.json` holds respellings that were not heard (the voice could not be run while writing
the content). Check each with `scripts/pronounce.py` and have a person confirm it:

| Written | Spoken as |
|---|---|
| `NuGet.config` | NuGet dot config |
| `App_Data` | App Data |
| `OrchardCore.db` | Orchard Core dot D.B (the sentence supplies the last period) |
| `OrchardCore.Application.Cms.Targets` | Orchard Core Application CMS Targets |
| `OrchardCorePreview` | Orchard Core Preview |
| `smtp4dev` | S.M.T.P. for dev |
| `dotnet` | dot net |
| `CLI` | C.L.I. |
| `CI` | C.I. |

Also listen to words with no respelling: "Lombiq", "Cloudsmith", "JetBrains", "SQLite", "nuget", "nuget.org", "orchard-version".
