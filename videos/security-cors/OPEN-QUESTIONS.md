# Open questions: CORS: Allow Other Sites to Call Your APIs

What was checked in the source, and what a person must confirm on a running site before or while recording. None of
the unconfirmed items is stated as a fact in the narration.

## 1. Is a policy used when no policy is marked as the default?

- The documentation (`src/docs/reference/modules/Cors/README.md`, "Create and apply a policy") says: if no policy is marked as the
  default, the first configured policy becomes the default.
- The code (`src/OrchardCore.Modules/OrchardCore.Cors/Services/CorsOptionsConfiguration.cs:84`) ends with
  `options.DefaultPolicyName ??= corsSettings.Policies.FirstOrDefault()?.Name;`. In ASP.NET Core,
  `CorsOptions.DefaultPolicyName` starts with the value `__DefaultCorsPolicy`, not `null` (checked by creating a `CorsOptions` from the
  `Microsoft.AspNetCore.Cors` assembly of the local 10.0.15 shared framework), so `??=` never assigns. With no policy marked as
  the default, the middleware would ask for a policy named `__DefaultCorsPolicy`, which is not registered, and the site would send no CORS headers.
- Not confirmed on a running site. Confirm with two policies saved, neither marked as the default:
  `curl -i http://localhost:5010/ -H "Origin: https://portal.example.com"` and look for `Access-Control-Allow-Origin`. Then mark one
  policy as the default, save, and send the same request again.
- The narration does not describe the fallback. It says the default policy is the one the site applies (`create-2`), and
  advises marking one policy as the default (`narrow-2`, `narrow-4`, and the closing card).

## 2. What the narration states from the source or the documentation only

These are not demonstrated in the video; a person can confirm them with the commands above and the browser's network tools.

- `create-4`: an origin has no path and no trailing slash (documentation, "Policy options"). Checked with the `CorsPolicyBuilder` of the
  local 10.0.15 shared framework: `WithOrigins` keeps `https://portal.example.com/` and `https://portal.example.com/path` exactly as
  typed (it only lowercases the scheme and host), so such an entry is not the same string as the `Origin`
  header of a browser, which has no path. Not checked against a request on a running site: send
  `curl -i http://localhost:5010/ -H "Origin: https://portal.example.com"` with the origin saved with and without the trailing slash.
- `create-3`: "when the page asks for it" is the documentation ("The client must also opt in to credentialed requests"). The code only
  calls `AllowCredentials()` or `DisallowCredentials()` (`Cors/Services/CorsOptionsConfiguration.cs:63-70`); the response header it
  produces comes from ASP.NET Core. Confirm the presence of `Access-Control-Allow-Credentials` with the request above, with the box
  selected and cleared.
- `create-6`: response headers that browsers always allow are readable without being listed (documentation, "Policy options").
- `unsafe-2`: "an asterisk entry in the allowed origins is treated the same way" is the check of the admin
  (`Cors/Controllers/AdminController.cs:149-150`). At runtime, `CorsOptionsConfiguration` skips only a policy whose
  `AllowAnyOrigin` flag is set together with credentials (`Cors/Services/CorsOptionsConfiguration.cs:28-32`). An asterisk entry with
  credentials, which only a recipe or deployment import can store, reaches `options.AddPolicy`, and the local 10.0.15 framework throws an
  `InvalidOperationException` there (checked on a `CorsOptions` instance, not on a running site). The narration says nothing about
  imports.
- `unsafe-2`: the admin saves nothing when any policy has the combination. The action returns before `UpdateSettingsAsync`
  (`AdminController.cs:132`, `:140`). Confirm on the site that, after the warning, opening `/Admin/Cors` again shows the policy as it was saved.
- `narrow-3`: the step named `Cors settings` appears when the Deployment feature is enabled (`Cors/Startup.cs`,
  `DeploymentStartup`). Confirm its label on the page **Tools, Deployments, Plans** when a step is added to a plan.
- `enable-3`: the permission is shown as `Managing Cors Settings` on the role editor
  (`Cors/Permissions.cs:8`; the documentation spells it `Managing CORS Settings`).

## 3. To check while recording

- **Pronunciation.** Check how the voice says "CORS" and "Cors" with `scripts/pronounce.py` before narrating, and add a
  `spoken` entry only if a person confirms a better spelling.
- **Admin menu.** The labels Tools, Features, Settings, Security and Cross-Origin Resource Sharing come from
  `Features/AdminMenu.cs` and `Cors/AdminMenu.cs`. Confirm in the running admin that each section can be expanded by a
  click, and that `a[href$='/Admin/Features']` and `a[href$='/Admin/Cors']` match the menu links.
- **Features search.** The search matches the text `Name Id Description` of each row, ignoring case
  (`Features/Drivers/FeatureEntryDisplayDriver.cs:20`, `Features/Assets/ts/features-index.ts`). In the source, no `Manifest.cs` other than
  that of **CORS Configuration** holds "cors" in a name, id or description, so one row should remain. Confirm it on the running site: if another
  row remains, the narration of `enable-1` is still right, but the frame shows more than one row.
- **Time of the toasts.** A success toast stays 5 seconds. After `enable-2` the Features page loads afresh, with an empty search box,
  so the CORS Configuration row is out of view and only the message is shown (see the plan). Confirm that the message of `enable-2` is on screen when the
  narration of that step ends, and that the message of `create-7` appears while the narration mentions it. A click that navigates cannot be wrapped in
  `OffCameraAsync`, so a pause of a few seconds while the tenant restarts is shown as a still frame. If it lasts more than about 3 seconds,
  decide with the maintainer whether to record again, or to accept it.
- **Viewport.** Confirm that the cards of the editor and the **Save** button are reachable with the scroll call of the plan, and
  that in `unsafe-1` the Credentials and Origins cards fit together in 1536 by 864 pixels. If they do not, scroll to
  `label[for='allowed-origins']` only after the first click, and note the final scroll in the recorder's scene.
- **Page after the warning.** The page that follows the warning is drawn from the posted policies, so the list can show a
  state that is not saved. The narration of `unsafe-2` only says the admin saves nothing.

## 4. Observed, not narrated

- In the editor, **Edit** copies the policy with a shallow copy (`Cors/Assets/ts/cors-admin-index.ts:296`), so the arrays of
  origins, headers and methods are shared with the list. Adding or deleting an item and then selecting **Cancel** does not
  undo the change in the page, and the next **Save** of any policy posts it. The video never selects Cancel.
- **Delete** in the list saves at once and asks for no confirmation (`cors-admin-index.ts:298-306`). The video does not click it.

## 5. For the maintainer

- The documentation page of the feature already ends with a `## Video` section holding a YouTube iframe
  (`src/docs/reference/modules/Cors/README.md`). Decide whether this video replaces it, or goes under the introduction with
  a `video:` marker as the skill describes.
- `narrow-2` says a module can select another policy by name for an endpoint. The tenant pipeline calls `UseRouting` first
  and then the startups in `ConfigureOrder` (`src/OrchardCore/OrchardCore/Modules/Extensions/ShellPipelineExtensions.cs:80,83,94-105`), and the CORS middleware sits after it (`Cors/Startup.cs`), so endpoint metadata is available to it.
