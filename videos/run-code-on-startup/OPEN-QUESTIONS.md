# Open questions: Run Code When a Tenant Starts

The narration was written from the documentation page of the guide and from the Orchard Core source (commit
`92db4a83`); nothing was run. Every sentence below is already read from the source, so none of it is in doubt, but a
person with a running site should confirm it once before the video is published.

Source files named without a folder:

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

## How to check on a running site

Run `src/OrchardCore.Cms.Web` with a throwaway `App_Data` folder (the Blog recipe, user `admin`, password `<password>`
from outside git), with a small module whose `Startup` class registers the guide's `MyStartupTaskService`, plus a
second handler class in a second module that depends on the first. Make every method of both handlers write a log
entry that names the handler and the method (`ActivatingAsync`, `ActivatedAsync`, `TerminatingAsync`,
`TerminatedAsync`, `RemovingAsync`). Then:

1. **First request** (`run-1`, `when-3`). Start the site, wait, and check that no handler entry is logged before the
   first request. Send the first request and check that the entries appear, before the page is returned. The guide
   shows `info: MyStartupTaskService[0]` followed by `A tenant has been activated.`; confirm the exact console lines,
   and that the category is the full name of the class when the class is in a namespace (the narration says only "the
   handler's class"). Source: `ModularTenantContainerMiddleware.cs:58-60`, `ShellScope.cs:263-266, 303-355`.
2. **Second request and rebuilds** (`run-2`). Send a second request and check that nothing is logged. Enable or disable
   a feature, and reload the tenant, and check that the entries come back on the next request. Source:
   `ShellScope.cs:305-308`, `ShellHost.cs:171-183`.
3. **Order** (`order-1`, `order-2`). With the two handlers, check that all `ActivatingAsync` entries come before all
   `ActivatedAsync` entries, that the `ActivatedAsync` entries are in the reverse order of the `ActivatingAsync`
   ones, and that the handler of the module that depends on the other one is called after it in `ActivatingAsync`.
   The direction (dependencies first) is stated by the comment at `ShellRemovalManager.cs:89-93` and by
   `ShellContainerFactory.cs:120-122` ("ordered by module dependency"), but no page of the documentation says it.
4. **Terminating and removing** (`stop-1`, `remove-1`, `remove-2`). After a reload, check that `TerminatingAsync` is
   logged in the order of registration and `TerminatedAsync` in reverse, once the requests already running have
   ended. Disable a tenant on which the module's feature has been enabled, remove it, and check that `RemovingAsync` is
   logged in reverse order (the handlers come from a context built with all the installed features of the tenant, not
   only the enabled ones: `ShellContextFactoryExtensions.cs:14-27`, used at `ShellRemovalManager.cs:59`). The source calls the tenant handlers only for a tenant that is `Disabled` and when not all
   resources are local (`ShellRemovalManager.cs:52-102`), so an `Uninitialized` tenant that is removed does not call
   them; the narration says "a disabled tenant". Also check that a handler setting `context.ErrorMessage` stops the
   handlers after it (`ShellRemovalManager.cs:100-102`).
5. **A handler that throws** (`care-1`). Throw from `ActivatingAsync` and check that the first request fails, and that
   the next request calls the handler again. Source: `IsActivated` is set only after both loops complete
   (`ShellScope.cs:333-353`).
6. **An `Uninitialized` tenant** (`care-2`). Start a site that is not set up (no Auto Setup) and check that the handlers
   are called when the Setup screen is first requested, with `ShellSettings.State` equal to `Uninitialized`. The
   narration says that such a tenant has no database session; that is the comment at
   `PreloadSiteSettingsTenantEventHandler.cs:21`, and the handlers that check the state are
   `AutomaticDataMigrations.cs:36`, `TempDirCleanerService.cs:27` and `MediaBlobContainerTenantEvents.cs:54`.

## Wording to confirm

- **"By default" in `when-3`.** The page says the handlers are not called when the application starts. The
  `ShellWarmup` option of `OrchardCore:BackgroundService` defaults to `false` (`docs/reference/modules/BackgroundTasks/README.md:83`),
  which is what "by default" stands for. With the option set to `true`, `ModularBackgroundService` loads every tenant
  at start (`Modules/ModularBackgroundService.cs:59-67`) and then opens a shell scope on each running tenant
  (`:281-295`), and opening a scope activates the tenant (`ShellScope.cs:248-266`). Start a site with that option and
  check whether the entries appear with no request; the narration does not depend on the answer.
- **Order and `Order`.** The handlers are in the order in which the `Startup` classes register them, and the startups
  are sorted by their `Order` property (0 by default) after the module dependency order
  (`ShellContainerFactory.cs:118-122`). The narration says only that a module's handlers run after those of the
  modules it depends on, which holds when no `Startup` class sets a different `Order`. Check it with the two-module
  site described above.
- **"Registered as a scoped service" in `register-1`.** This is the guide's choice (`README.md:42`); most modules of
  Orchard Core do the same, a few register a singleton (`ISignal`, `IRedisService`). The activation and the termination
  run in different scopes (`ShellScope.cs:339-351`, `:532-541`), so a scoped handler is a different instance for
  `ActivatingAsync` and for `TerminatingAsync`. The video does not say anything about this; add it if a handler is
  meant to keep a field between the two.
- **Pronunciation.** `spoken` in `storyboard.json` has "I Modular Tenant Events", "Modular Tenant Events",
  "Activating Async", "Activated Async", "Terminating Async", "Terminated Async", "Removing Async",
  "Configure Services", "Shell Settings", "Shell Removing Context" and "Error Message". A person must confirm them with
  `scripts/pronounce.py` before the voice is generated. Also listen to "Uninitialized", "Startup" and "Azure".
- **`write-1`: "when it is released".** The video does not define a released tenant before `stop-1`. A tenant is
  released when its shell is reloaded, or when a feature is enabled or disabled (`ShellHost.cs:171-183`). Check that the pause between `write-1` and `stop-1` reads well, or move
  `stop-1` earlier.
