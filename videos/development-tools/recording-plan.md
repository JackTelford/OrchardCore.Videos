# Recording plan: development-tools

Developer video, 19 narrated steps: the opening card, 17 slides in three chapters, and the closing card.
**There is nothing to record.** Every task of this video happens in an editor, a dialog of Visual Studio, a text file
or a terminal, and the programme draws those as slides (`draw_code`); the recorder drives a browser only, and no
browser screen is part of this story. `assemble.py` draws every slide from `slides.py` and joins the cards, the chapter
cards and the slides with the narration.

## The site it needs

None. No Orchard Core site, no Auto Setup recipe, no tenant, no time zone, no features to enable, no settings, no
external service, no seed data, no `state.json`, no `VIDEO_PASSWORD`, no `recorder/` folder and no `Scenes.cs`.

## Order of the clips, and what they change

There are no `clip` steps, so no clip depends on another and none changes a site: there is no snapshot to restore
before a new take. The steps are produced in the order of the storyboard:

1. `python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/development-tools`
2. `python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/development-tools`

Before narrating, listen to the words of the `"spoken"` map of `storyboard.json` with `scripts/pronounce.py`
(see `OPEN-QUESTIONS.md`), and have a person confirm each respelling.

## Cards

- `intro`: the opening card. Title "Orchard Core Development Tools", subtitle "Editors, preview packages and local
  NuGet feeds".
- `outro`: the closing card. Same title, subtitle "docs.orchardcore.net".

## Slides

Chapter 1, "Editors and utilities" (clip `01-tools`). Each slide is three or four cards drawn by `bullets`.

- `tools-1`: "Use the tool you prefer". Three cards: Visual Studio, Visual Studio Code, JetBrains Rider, and a line
  saying that any tool that edits source files and builds a .NET application works.
  Source: `src/docs/getting-started/development-tools.md:3` and `:7-12`.
- `tools-2`: "Optional extras for Visual Studio". Three cards: the Lombiq Orchard Visual Studio Extension, the code
  snippets of the Orchard Dojo Library, the further tips of the Orchard Dojo Library.
  Source: `development-tools.md:8-10`.
- `tools-3`: "Browse the database of a tenant". Four cards: DB Browser for SQLite; the folder `App_Data`, `Sites`, the
  tenant's name; the file name `OrchardCore.db` (and `yessql.db` for a tenant with no saved name); the tables, queries
  and JSON documents (the `Document` table).
  Source: `src/OrchardCore/OrchardCore.Data.YesSql/SqliteHelper.cs:21-26` (the fallback name) and `:51-52` (the folder),
  `src/OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs:178-181` (`OrchardCore.db`),
  `src/OrchardCore/OrchardCore/Shell/ShellOptionsSetup.cs:20-31`, `src/OrchardCore/OrchardCore.Abstractions/ShellOptionConstants.cs:6-7`
  (`App_Data`, `Sites`), `src/OrchardCore/OrchardCore.Abstractions/Shell/ShellSettings.cs:19` (`Default`),
  `src/docs/topics/data/yessql.md:7` (`Document` table), `src/docs/getting-started/development-tools.md:16`.
- `tools-4`: "Test email on your machine". Four cards: smtp4dev; "Settings, Communication, Email" with the SMTP tab, which
  needs the SMTP Email Provider feature enabled; "Enable SMTP Provider", then Sender email address, Host name and Port
  number; its web interface.
  Source: `src/OrchardCore.Modules/OrchardCore.Email/AdminMenu.cs:38-40` (Settings, Communication, Email),
  `src/OrchardCore.Modules/OrchardCore.Email.Smtp/Manifest.cs:4` (the feature name "SMTP Email Provider"),
  `src/OrchardCore.Modules/OrchardCore.Email.Smtp/Startup.cs:29` (the SMTP settings exist only with that feature),
  `src/OrchardCore.Modules/OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:88` (the SMTP tab) and `:120-122`
  (the sender is required) and `:129-133` (the host name is required for the Network delivery method, the default),
  `src/OrchardCore.Modules/OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml:10` (Enable SMTP Provider), `:17` (Sender email address),
  `:50` (Host name), `:59` (Port number),
  `src/docs/getting-started/development-tools.md:17`.

Chapter 2, "Preview packages" (clip `02-preview`).

- `sources-1`: "Where Orchard Core packages come from". Three cards: release packages (NuGet, a release's version tag),
  preview packages (Cloudsmith, the daily build of the main branch), the Preview - CI workflow (can also be run
  manually). Source: `src/docs/getting-started/preview-package-source.md:3`.
- `sources-2`: "Use preview packages with care". Three cards: the development code (not the most stable, breaking
  changes), to try a fix, not in production (not kept forever). Source: `preview-package-source.md:5-8`.
- `feed-1`: "The preview feed". Two code panels: the Cloudsmith address
  `https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json` and the NuGet address
  `https://api.nuget.org/v3/index.json`, and two cards: hosted on Cloudsmith, no credentials.
  Source: `preview-package-source.md:12-16` and `:35`.
- `vs-1`: "Add the feed in Visual Studio". Four cards: Tools, NuGet Package Manager, Package Manager Settings;
  Package Sources; Name and Source; Update, then OK.
  Source: `preview-package-source.md:20-21` and its screenshot `src/docs/getting-started/assets/add-preview-package-source.png`
  (the Options dialog: Package Sources, Name, Source, Update, OK). Visual Studio is not part of the Orchard Core source:
  see `OPEN-QUESTIONS.md`.
- `config-1`: "Add the feed in a NuGet.config file". A code panel with the file of the documentation; the two `add`
  lines are bright. Source: `preview-package-source.md:27-38`.
- `cli-1`: "Add the feed with the .NET CLI". A code panel with the `dotnet nuget add source` command, and two cards: the
  feed address, and `--name`. Source: `preview-package-source.md:43-46`.
- `use-1`: "Install a preview package". Three cards: Manage NuGet packages, Include prerelease, Package Source.
  Source: `src/docs/getting-started/README.md:18` (labels as the documentation spells them);
  `src/OrchardCore.Build/OrchardCore.Commons.props:5-7` (the preview suffix of the version).

Chapter 3, "A local NuGet feed" (clip `03-local`).

- `local-1`: "Packages from your own source code". Three cards: Pack, Publish, Update your project.
  Source: `src/docs/topics/local-nuget-packages/README.md:3`, `:5`, `:13` and `:22`.
- `local-2`: "1. Pack the source code". A code panel with `dotnet pack -c Release -o c:\OrchardCoreNuget`, and two cards
  on `-c Release` and `-o`. Source: `local-nuget-packages/README.md:9-11`.
- `local-3`: "2. Publish to a feed". A code panel with `nuget init c:\OrchardCoreNuget \\{YourServer}\NuGetServer`, and two
  cards: the output folder, the feed folder. Source: `local-nuget-packages/README.md:15-20`.
- `local-4`: "3. Point your project at the feed". A code panel with the `NuGet.config` of the documentation (MyFeed, NuGet);
  the two `add` lines are bright. Source: `local-nuget-packages/README.md:24-37` and `:39`.
- `local-5`: "One version for every package". Three cards: every package takes the version of the source you packed; a
  version prefix and a version suffix, set in `OrchardCore.Commons.props`; `OrchardCore.Application.Cms.Targets`
  bundles the modules and themes as package references.
  Source: `src/OrchardCore.Build/OrchardCore.Commons.props:5-7`,
  `src/OrchardCore/OrchardCore.Application.Cms.Targets/OrchardCore.Application.Cms.Targets.csproj:19-34`,
  `src/OrchardCore/OrchardCore.Application.Cms.Core.Targets/OrchardCore.Application.Cms.Core.Targets.csproj:25-35`.
- `local-6`: "Reference that version". Two code panels: a package reference of `OrchardCore.Application.Cms.Targets` with
  `Version="YOUR_VERSION"`, and `dotnet new occms -n MySite --orchard-version YOUR_VERSION`.
  Source: `src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Cms.Web/OrchardCore.Templates.Cms.Web.csproj:31`,
  `.../.template.config.src/template.json:49-55` and `.../dotnetcli.host.json:12-15` (the `orchard-version` option),
  `src/docs/guides/create-cms-application/README.md:23` (`dotnet new occms -n MySite`).

## After assembling

Look at one frame of every slide with ffmpeg: no text cut off, no panel overlapping a card, every text readable at
1080p. The slides were checked in memory for overflow, not looked at.

## Checks for private data

- Every slide shows placeholders only: `\\{YourServer}\NuGetServer`, `c:\OrchardCoreNuget`, `MyFeed`, `MySite`,
  `YOUR_VERSION`. No machine name, user name, email address, token or password appears on a slide or in the narration.
- The only addresses on screen are the two feed addresses of the documentation (the preview feed and nuget.org). Both
  are public and need no credentials.
- The video has no recording, so no screen of a real site, sign-in or `App_Data` copy is ever shown.
- Look at the final frames for the same words by OCR, as the skill asks for every video.
