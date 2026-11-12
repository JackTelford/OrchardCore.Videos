# Recording plan: Run Code When a Tenant Starts

Audience: developer. Length: about 4 minutes of narration (`python tools/validate_content.py run-code-on-startup`
prints the exact count).

## What there is to record

**Nothing is recorded in a browser.** Every chapter of this video is a card or a set of slides: the story is a class,
one registration line, the order in which Orchard Core calls the methods, and a line of console output, and none of
them is a screen of the admin UI. The decided convention applies: terminal commands and console output of a developer
video are slides, and only browser screens are "clip" steps.

So the person who makes this video:

- writes no `recorder/Scenes.cs`, runs no `Recorder setup`, `prepare` or `record`, and needs no `state.json`;
- needs no Orchard Core site, no Auto Setup recipe, no tenant, no time zone, no feature to enable, no setting, no
  external service and no seed data;
- has no clips to order and nothing to restore from a snapshot, since no step changes a site.

The video is built with `narrate.py` and then `assemble.py`, which draws the cards and the slides from
`storyboard.json` and `slides.py`. `assemble.py` takes no recordings for this video, so it needs no `clips/` folder.

## Checking the slides before assembling

From this folder, with the skill's `scripts` folder on `PYTHONPATH`, call `slide()` for every slide step and look at
the exceptions only (the `-B` flag keeps Python from writing a `__pycache__` folder here):

```bash
cd videos/run-code-on-startup
PYTHONPATH=../../.agents/skills/orchardcore-video-producer/scripts python -B -c "
import json, slides
sb = json.load(open('storyboard.json', encoding='utf-8'))
for c in sb['chapters']:
    for cl in c['clips']:
        if cl['kind'] == 'slide':
            for s in cl['steps']:
                print(s['id'], slides.slide(s['id'], c['title'], 'CHAPTER ' + str(int(c['id']))).size)
"
```

Every line must print `(1920, 1080)`. Do not save the images: render them to a window or to a scratch folder outside
git (`videos/*/*.png` is ignored, but nothing needs to be kept).

When the slides are assembled, look at each one once: no text may touch the edge of a panel or a card, and the
highlighted lines of a code panel must be the ones the narration talks about (listed below).

## The code on the slides

All code is real, trimmed with `// ...`. The references are to the Orchard Core source at the commit of
`catalog/features.json`, and to the documentation page of the guide.

| Constant in `slides.py` | Shown on | Taken from |
|---|---|---|
| `INTERFACE` | `write-1` | `src/OrchardCore/OrchardCore.Abstractions/Modules/IModularTenantEvents.cs:1-12` |
| `HANDLER` | `write-2`, `write-3` | `src/docs/guides/run-code-on-startup/README.md:16-36` (the `using System;` line is left out, since nothing uses it) |
| `STARTUP` | `register-1` | the registration line is `src/docs/guides/run-code-on-startup/README.md:42`; the class around it has the shape of `src/OrchardCore.Modules/OrchardCore.Settings/Startup.cs:31-34` (`public sealed class Startup : StartupBase`, `ConfigureServices`) |
| `MIGRATIONS` | `order-3` | `src/OrchardCore/OrchardCore.Data/Migration/AutomaticDataMigrations.cs:11,34-48` |
| `PRELOAD` | `order-3` | `src/OrchardCore.Modules/OrchardCore.Settings/Services/PreloadSiteSettingsTenantEventHandler.cs:6,17-28` |
| `REMOVING` | `remove-1`, `remove-2` | `src/OrchardCore.Modules/OrchardCore.Media.Azure/Services/MediaBlobContainerTenantEvents.cs:88-115` |
| `CLEANER` | `care-1`, `care-2` | `src/OrchardCore.Modules/OrchardCore.Media/Services/TempDirCleanerService.cs:25-55` |
| `CONSOLE` | `run-1` | `src/docs/guides/run-code-on-startup/README.md:52-53` |

The steps below name source files without a folder; they are in these folders:

| File | Folder (in the Orchard Core source) |
|---|---|
| `ShellScope.cs` | `src/OrchardCore/OrchardCore.Abstractions/Shell/Scope/` |
| `ShellContext.cs` | `src/OrchardCore/OrchardCore.Abstractions/Shell/Builders/` |
| `ShellHost.cs` | `src/OrchardCore/OrchardCore/Shell/` |
| `ShellRemovalManager.cs` | `src/OrchardCore/OrchardCore/Shell/Removing/` |
| `ShellContainerFactory.cs` | `src/OrchardCore/OrchardCore/Shell/Builders/` |
| `ModularTenantContainerMiddleware.cs`, `ModularBackgroundService.cs` | `src/OrchardCore/OrchardCore/Modules/` |
| `AutomaticDataMigrations.cs` | `src/OrchardCore/OrchardCore.Data/Migration/` |
| `PreloadSiteSettingsTenantEventHandler.cs` | `src/OrchardCore.Modules/OrchardCore.Settings/Services/` |
| `TempDirCleanerService.cs` | `src/OrchardCore.Modules/OrchardCore.Media/Services/` |
| `MediaBlobContainerTenantEvents.cs` | `src/OrchardCore.Modules/OrchardCore.Media.Azure/Services/` |

## Chapter 00: opening card

- `intro` (clip `00-intro`, card): the opening card, with the title "Run Code When a Tenant Starts" and the subtitle
  "Tenant event handlers in your own module". Nothing to record.

## Chapter 01: When a tenant starts (clip `01-when`, slides)

- `when-1`: heading "A Startup class and a tenant event handler", two cards: "Startup class" (registers the services
  and the middleware of a module for each tenant) and "Tenant event handler" (runs your own code at key moments in the
  life of a tenant).
- `when-2`: heading "What the handlers of Orchard Core do", four cards: run data migrations, preload the site
  settings, create a storage container, empty a temporary folder. Sources: `AutomaticDataMigrations.cs:34-48`,
  `PreloadSiteSettingsTenantEventHandler.cs:17-28`, `MediaBlobContainerTenantEvents.cs:38-86`,
  `TempDirCleanerService.cs:25-55`.
- `when-3`: heading "Tenants are loaded lazily", three cards: the application starts (by default, no handler runs), a
  tenant handles its first request (the tenant activates and its handlers run), the request is processed. Sources:
  the documentation page lines 9-10; `ModularTenantContainerMiddleware.cs:58-60` (the scope activates the tenant before
  `_next` runs) and `ShellScope.cs:263-266, 303-355`.

## Chapter 02: Write a handler (clip `02-write`, slides)

- `write-1`: heading "The IModularTenantEvents interface"; the interface in a code panel, and three cards at its right:
  "Starts" (`ActivatingAsync`, `ActivatedAsync`), "Released" (`TerminatingAsync`, `TerminatedAsync`), "Removed"
  (`RemovingAsync`).
- `write-2`: heading "Inherit from ModularTenantEvents"; the handler class, with the class declaration and the
  `ActivatingAsync` override in bright lines (5 and 14 to 20), the rest dimmed.
- `write-3`: heading "Services come from the tenant"; the same class, with the logger field and the constructor in
  bright lines (5 to 12 and 20). Source of the claim: `ShellScope.cs:341` resolves the handlers from the tenant's
  service provider; `PreloadSiteSettingsTenantEventHandler.cs:11-15` and `TempDirCleanerService.cs:14-23` receive
  `ShellSettings` and other tenant services in their constructors.

## Chapter 03: Register the handler (clip `03-register`, slides)

- `register-1`: heading "Register it in the Startup class"; the `Startup` class with the
  `services.AddScoped<IModularTenantEvents, MyStartupTaskService>();` line, and two notes under it: "The service type is
  IModularTenantEvents." and "Every handler registered this way is called." (`ShellScope.cs:341-351` calls every
  registered service of the type).

## Chapter 04: The order of the events (clip `04-order`, slides)

All three slides use the same four cards; the bright ones follow the narration.

- `order-1`: heading "The order of the events"; only the first card is bright: `ActivatingAsync` in the order of
  registration, which follows the dependencies of the modules (`ShellScope.cs:341-345`; the documentation page lines
  46-47; `ShellContainerFactory.cs:118-122` orders the startups by module dependency, then by `Order`).
- `order-2`: same heading; the second, third and fourth cards are bright: `ActivatedAsync` in the reverse order after
  every `ActivatingAsync` (`ShellScope.cs:347-350`), `TerminatingAsync` in the order of registration
  (`ShellScope.cs:533-536`), `TerminatedAsync` and `RemovingAsync` in the reverse order (`ShellScope.cs:538-541`,
  `ShellRemovalManager.cs:94`).
- `order-3`: heading "Both are used by Orchard Core"; two code panels, `AutomaticDataMigrations.cs` (its
  `ActivatingAsync` calls `UpdateAllFeaturesAsync`) above `PreloadSiteSettingsTenantEventHandler.cs` (its
  `ActivatedAsync` calls `GetSiteSettingsAsync`).

## Chapter 05: Run it (clip `05-run`, slides)

- `run-1`: heading "The first request activates the tenant"; a "Console" panel with the two lines the documentation
  shows (`info: MyStartupTaskService[0]` and `A tenant has been activated.`), and two cards under it: "Category" (the
  class of the handler) and "Message" (the text that the handler logged). The slide stands for a terminal; no real
  console is recorded. The two lines are exactly those of the documentation page, with no path or machine name.
- `run-2`: heading "The handlers run again when the tenant is built again"; three cards: a second request (nothing is
  logged, the tenant is already activated), a feature is enabled or disabled, the tenant is reloaded (the tenant is
  built again and activates on its next request). Sources: `ShellScope.cs:305-308` (an activated shell returns
  at once), `ShellHost.cs:171-183`.

## Chapter 06: Stopping and removing (clip `06-stop`, slides)

- `stop-1`: heading "Terminating a released tenant"; three cards: the tenant is released, the work already using it
  finishes, the handlers are called (`TerminatingAsync` in the order of registration, `TerminatedAsync` in reverse).
  Source: `ShellScope.cs:520-545` and the comment of `ShellContext.TerminateShellContextAsync` ("the last scope").
- `remove-1`: heading "Removing a tenant"; the `RemovingAsync` of the Azure media handler, with the signature, the
  `blobContainer` line, the `RemoveContainer` test and the `DeleteIfExistsAsync` line bright (lines 1, 4, 5 and 9).
  Source of the claim that handlers run in reverse order when a disabled tenant is removed:
  `ShellRemovalManager.cs:52-100`.
- `remove-2`: same slide; the signature and the `catch` block that sets `context.ErrorMessage` and `context.Error`
  are bright (lines 1 and 12 to 17). Source: `ShellRemovalManager.cs:100-102` (`if (!context.Success) break;`).

## Chapter 07: Write handlers with care (clip `07-care`, slides)

- `care-1`: heading "Errors and the state of the tenant"; the `ActivatedAsync` of `TempDirCleanerService`, with the
  `try` and `catch` blocks bright (lines 5 to 12). Source of the claim that the first request waits and that an
  exception fails it: `ShellScope.cs:263-266` (the activation is awaited before the request runs) and
  `ShellScope.cs:333-353` (`IsActivated` is set only after the handlers completed).
- `care-2`: same slide; the `IsRunning` test and its braces are bright (lines 1, 3, 4 and 13). Source of the claim
  about the `Uninitialized` state: the comment at `PreloadSiteSettingsTenantEventHandler.cs:21` and the test at
  `AutomaticDataMigrations.cs:36`.

## Chapter 99: closing card

- `outro` (clip `99-outro`, card): the closing card, with the title "Run Code When a Tenant Starts" and the subtitle
  "docs.orchardcore.net".

## Checks for private data

- The slides show source code of Orchard Core and the documentation's example class `MyStartupTaskService`; there is
  no name of a person, an email address, a path of a machine, a connection string or a token. The Azure handler
  excerpt shows `_options.ConnectionString` as an identifier only, with no value.
- No recording is made, so there is no site to scan: after assembling, extract a frame of each slide and read the
  texts once to confirm that none was added by hand.
