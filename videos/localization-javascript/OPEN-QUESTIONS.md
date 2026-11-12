# Open questions: JavaScript Localization

What was checked in the Orchard Core source, and what a person must confirm on a running site or by listening to the
voice. None of these changes a claim of the narration: the narration says only what the source shows. Paths are relative to
`src/OrchardCore.Modules` unless they start with `src/`.

## To confirm on a running site (before recording `05-admin`)

1. **The cultures editor.** `OrchardCore.Localization/Views/LocalizationSettings.Edit.cshtml` mounts a component of the shared
   `bloom` package (`bloom/components/culture-settings-editor`) that is not in the source snapshot. The labels `Culture`,
   `Default culture`, `Set as default`, `Remove culture` and `Add culture` come from `LocalizationJSLocalizer.cs:16-20`, and the
   guide `src/docs/guides/install-localization-files/README.md` says "select `fr | French` then click Add culture". Confirm the
   text of the entries of the list (`fr | French`, `en-US | English (United States)`: it depends on the culture data of the machine) and
   which button the editor shows for each culture.
2. **`?culture=fr` on an admin page.** The query string is read by the ASP.NET Core request localization providers, which are
   not in the Orchard Core source. The Orchard Core provider that could get in the way (`OrchardCore.Localization/AdminCookieCultureProvider.cs`)
   only reads a cookie named `admin_culture_<version>`, which only the Admin Culture Picker feature writes. Confirm that
   `/Admin/Cors?culture=fr` shows the French strings in a fresh browser context, and that the same page without the query
   string shows English.
3. **The PO file.** Confirm that `fr.po` loads with the header entry (`msgid ""`) and `Content-Type: text/plain; charset=UTF-8`, and
   that every string of the page is French. If a string stays English, its `msgid` differs from the source: compare with
   `OrchardCore.Cors/OrchardCore.Cors.pot`. The file is read once per culture and kept in memory
   (`OrchardCore.Localization.Core/LocalizationManager.cs:38`), so restart the site after editing it.
4. **What fits on the screen in `admin-3`.** After clicking `Modifier`, the heading `Public site`, the card `Détails`, the field and the
   top of the card `Informations d'identification` should be visible at 1536 x 864. If the page needs a scroll, scroll to the top
   (`scene.ScrollAsync(0, -2000)`) before the step ends.
5. **Highlights.** `recording.md` documents `HighlightAsync(locator, caption)` with an optional caption: confirm that a call without
   a caption draws only the ring. The selectors of `recording-plan.md` come from the template of
   `OrchardCore.Cors/Assets/ts/cors-admin-index.ts` (`#search-box`, `button.btn-secondary`, `span.badge`, `li.list-group-item`, `h5.card-title`).
6. **The Enable button of a feature.** `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` builds the id of the button
   from the feature id (`btn-enable-...`); the plan uses the button of the row, found by its text `Enable`, instead of the id.

7. **Other French translations in the checkout.** The source snapshot used for this review has no `.po` file at all, so nothing in it
   translates the server-drawn text of `/Admin/Cors` (the breadcrumb `CORS Settings` and the two alerts). If the checkout used for
   recording ships a `fr.po` (for example in a `Localization` folder of the web project or of the CORS module, downloaded from Crowdin),
   that text becomes French too and the plan's remark that it "stays English" no longer holds. Check the folders listed in
   `ModularPoFileLocationProvider.cs:49-86` for `fr.po` before the first take, and if one has a CORS entry, either remove it for the
   recording or describe the French text in the expected screen. The narration does not depend on it: it names only the strings of the
   `cors-admin` group.
8. **The Features page search.** `Features.cshtml:27` gives the search box the placeholder `Search`; whether the list is filtered by name
   only or also by description is done in a script that was not checked. Enable the rows named exactly `Localization` and
   `CORS Configuration`.

## To confirm by listening

The storyboard's `spoken` map says these identifiers; each is a new spelling, so a person must listen to it before assembling
(`pronounce.py` was not run: it makes audio files):

- `IJSLocalizer` as "I JavaScript Localizer", `GetJSLocalizations` as "Get JavaScript Localizations", `GetLocalizations` as "Get Localizations".
- `UpdateLocalizationCatalog` as "Update Localization Catalog" and `OrchardCore.Localization.Build` as "Orchard Core dot Localization dot Build".
- `setTranslations` as "set Translations", `getTranslations` as "get Translations", `AddPolicy` as "Add Policy", `AddScoped` as "Add Scoped".
- "PO file" and "PO entries" as "P.O. file" and "P.O. entries"; `C#` as "C sharp".
- `cors-admin` as "cors admin" (the hyphenated group name of `admin-1`).
- "CORS" is not mapped: confirm that the voice says it as a word ("cors") and not as letters, and add a `spoken` entry if it does.

## What the narration relies on, and where it comes from

- `IJSLocalizer`, `GetLocalizations(string group)`, and the `null` result for a group the class does not own:
  `src/OrchardCore/OrchardCore.Localization.Abstractions/IJSLocalizer.cs`.
- The merge (groups in the order given, then every registered localizer, `null` skipped, the last write of a key wins):
  `JSLocalizerExtensions.cs:23-37`; the Razor call that uses it: `LocalizationOrchardHelperExtensions.cs:32-38`.
- The culture of a lookup is the current UI culture of the request, read at each call:
  `src/OrchardCore/OrchardCore.Localization.Core/PortableObject/PortableObjectStringLocalizer.cs:48`.
- The PO context of a typed localizer is the full name of the type, and an entry without a context is used when none with
  the context exists (`PortableObjectStringLocalizer.cs`, `ExtractTranslation`). The documentation's `fr.po` example has no context; the
  video shows the context that the generated templates write (`OrchardCore.Cors/OrchardCore.Cors.pot`).
- PO locations: `OrchardCore.Localization/ModularPoFileLocationProvider.cs:49-86` and `src/docs/reference/modules/Localize/README.md`
  ("PO files locations"); "restart after an edit": the same README, and the cache above.
- The shared helper (`getTranslations`, `setTranslations` with `Object.assign`): its file, `.scripts/bloom/helpers/localizations.ts`, is
  not in the source snapshot. Its source was read from `sourcesContent` of
  `OrchardCore.Cors/wwwroot/Scripts/cors-admin-index/cors-admin-index.js.map`, and matches the documentation. Confirm it against the
  checkout before recording.

## Left out of the narration on purpose

- Whether a module outside the Orchard Core repository can import `@orchardcore/bloom/helpers/localizations`. The CORS and Forms modules do,
  and the documentation says Orchard Core ships the helper; the video says "Orchard Core's helper" and shows the import as the CORS
  module writes it. A person who knows the packaging of third-party modules should say whether this needs a sentence.
- The documentation's example puts `@Json.Serialize(localizations)` in an attribute and says Razor HTML-encodes it. Every view in the source
  calls `.ToString()` on the result first (`Cors/Views/Admin/Index.cshtml:5`), and the video shows that form. It is not checked whether the form
  without `.ToString()` encodes the attribute.
- The `api/media/localizations` endpoint (`OrchardCore.Media/Endpoints/Api/GetLocalizationsEndpoint.cs:19`) serves the `media-gallery` group
  as JSON with `GetMergedLocalizations`. It is a second way to pass a dictionary to a script and is not in the documentation.
- The Media module registers its localizer with `AddSingleton` (`OrchardCore.Media/Startup.cs:70`); the documentation, the Localization, CORS, Forms
  and other modules use `AddScoped`. The narration says `AddScoped` without giving a reason.
