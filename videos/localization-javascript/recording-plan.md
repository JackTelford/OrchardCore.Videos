# Recording plan: JavaScript Localization

Everything a person needs to record this video without deciding anything. Orchard Core paths are relative to the
Orchard Core source tree (`src/OrchardCore.Modules/...`); `file:line` points at the place a label or a behavior comes from.

The video has thirteen slide steps and one recording (`05-admin`, three steps). The opening and closing cards are
narration only. The slides are drawn by `slides.py`; they need no site.

## The site it needs

This is not a video about the Setup screen: record on a site provisioned by Auto Setup, signed in with a saved
`state.json` (git-ignored).

- **Code**: `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout that contains the CORS and Localization
  modules, run on `http://localhost:5010` with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories.
  Block outbound calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Auto Setup**: recipe Blog, site name `Orchard Core`, time zone `America/Los_Angeles`, tenant `Default`, user `admin`,
  email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`).
- **Browser**: the recorder's Chromium (en-US, light color scheme). Do not change its language.
- **External services**: none.
- **Features to enable** (Tools > Features, `OrchardCore.Features/AdminMenu.cs:26`): `Localization`
  (`OrchardCore.Localization/Manifest.cs:11-12`, id `OrchardCore.Localization`) and `CORS Configuration`
  (`OrchardCore.Cors/Manifest.cs:4`, id `OrchardCore.Cors`). On the Features page, type the name in the search box, whose placeholder is
  `Search` (`OrchardCore.Features/Views/Admin/Features.cshtml:27`), and click the `Enable` link of the row named
  exactly `Localization` (other rows, such as `Content Language Header`, may match the search too), then of the row named
  `CORS Configuration` (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).

### Prepare the site (outside the capture)

1. Start the site once with an empty `ORCHARD_APP_DATA`, so Auto Setup creates the `Default` tenant. Stop the site.
2. Create the file `<ORCHARD_APP_DATA>/Sites/Default/Localization/fr.po` (UTF-8) with the content of "The French
   translation" below. The folder is the tenant's PO location (`OrchardCore.Localization/ModularPoFileLocationProvider.cs:64-65`:
   `App_Data/Sites/[Tenant]/Localization/[CultureName].po`). The file must exist before the site first serves a French
   page, because an edited PO file is only read again after a restart (`OrchardCore.Localization.Core/LocalizationManager.cs:38`).
3. Start the site again. Run `Recorder setup` to sign in and save `state.json`.
4. In a normal browser signed in as `admin`, enable the two features above.
5. Open Settings > Localization > Cultures (`OrchardCore.Localization/AdminMenu.cs:34-43`). The default culture must be
   `en-US`. If the machine's culture is another one, select `en-US | English (United States)` in the culture list, click
   `Add culture`, then `Set as default` on it, and remove the other culture with `Remove culture`
   (labels: `OrchardCore.Localization/Services/LocalizationJSLocalizer.cs:16-20`). Select `fr | French` in the culture
   list and click `Add culture`. Click `Save` (`OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`). A warning
   says the site has been restarted for the settings to take effect (`OrchardCore.Localization/Drivers/LocalizationSettingsDisplayDriver.cs:132`).
6. Open Settings > Security > Cross-Origin Resource Sharing (`OrchardCore.Cors/AdminMenu.cs:18-25`; the page is
   `/Admin/Cors`, `Controllers/AdminController.cs:44`), in English, without any culture in the address. Create two policies:
   - Click `Add a policy`. Replace the text of `Policy name` (`New policy`) with `Public site`. Check
     `Set as default policy`. Click `Save`. The page reloads with the message `The CORS settings have updated successfully.`
   - Click `Add a policy`. Replace the text of `Policy name` with `Partner portal`. Leave `Set as default policy`
     unchecked. Click `Save`.

   Leave every other field of both policies as it is. The list then shows `Public site` (with the `Default Policy`
   badge) first and `Partner portal` second, each with `Edit` and `Delete`.
7. Stop the site. Copy its `App_Data` folder aside, outside both repositories. This is the snapshot.
   Restore it before every full take; do not commit it.

### The French translation

This is the whole of `fr.po`. It translates the 40 strings of `CorsJSLocalizer`
(`OrchardCore.Cors/Services/CorsJSLocalizer.cs:16-55`) and nothing else, so the text drawn by the server stays English.
The context of every entry is the full name of the class, as in `OrchardCore.Cors/OrchardCore.Cors.pot`.

```po
msgid ""
msgstr ""
"Content-Type: text/plain; charset=UTF-8\n"
"Language: fr\n"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Search"
msgstr "Rechercher"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Add a policy"
msgstr "Ajouter une politique"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Default Policy"
msgstr "Politique par défaut"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Edit"
msgstr "Modifier"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Delete"
msgstr "Supprimer"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Add"
msgstr "Ajouter"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Save"
msgstr "Enregistrer"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Cancel"
msgstr "Annuler"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Nothing here! There are no CORS policies for the moment."
msgstr "Rien ici ! Il n'y a aucune politique CORS pour le moment."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Nothing here! Your search returned no results."
msgstr "Rien ici ! Votre recherche n'a retourné aucun résultat."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Details"
msgstr "Détails"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Provide policy details."
msgstr "Renseignez les détails de la politique."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Policy name"
msgstr "Nom de la politique"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "The name of the policy."
msgstr "Le nom de la politique."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Set as default policy"
msgstr "Définir comme politique par défaut"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Credentials"
msgstr "Informations d'identification"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Configure the credentials behavior."
msgstr "Configurez le comportement des informations d'identification."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allow credentials"
msgstr "Autoriser les informations d'identification"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allows credentials to be transported in requests. This setting is not allowed in combination with Allow Any Origin."
msgstr "Autorise le transport des informations d'identification dans les requêtes. Ce paramètre n'est pas autorisé avec Autoriser toutes les origines."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Origins"
msgstr "Origines"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Configure the origins behavior."
msgstr "Configurez le comportement des origines."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allow any origin"
msgstr "Autoriser toutes les origines"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allows requests from any origin. This will bypass any origin that is configured below."
msgstr "Autorise les requêtes de n'importe quelle origine. Les origines configurées ci-dessous sont ignorées."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Origin"
msgstr "Origine"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allowed origins"
msgstr "Origines autorisées"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Headers"
msgstr "En-têtes"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allow certain or all headers to be used by the external client."
msgstr "Autorisez certains ou tous les en-têtes pour le client externe."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allow any header"
msgstr "Autoriser tous les en-têtes"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allows requests with any header. This will bypass any headers that are configured below."
msgstr "Autorise les requêtes avec n'importe quel en-tête. Les en-têtes configurés ci-dessous sont ignorés."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Header"
msgstr "En-tête"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allowed headers"
msgstr "En-têtes autorisés"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Methods"
msgstr "Méthodes"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Configure the methods behavior (POST, PUT, DELETE, GET, etc)."
msgstr "Configurez le comportement des méthodes (POST, PUT, DELETE, GET, etc.)."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allow any method"
msgstr "Autoriser toutes les méthodes"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allows requests with any method. This will bypass any methods that are configured below."
msgstr "Autorise les requêtes avec n'importe quelle méthode. Les méthodes configurées ci-dessous sont ignorées."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Method"
msgstr "Méthode"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Allowed methods"
msgstr "Méthodes autorisées"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Exposed headers"
msgstr "En-têtes exposés"

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Configure which headers should be exposed."
msgstr "Configurez les en-têtes à exposer."

msgctxt "OrchardCore.Cors.Services.CorsJSLocalizer"
msgid "Sets response header 'Access-Control-Expose-Headers'."
msgstr "Définit l'en-tête de réponse « Access-Control-Expose-Headers »."
```

## Order of the clips

There is one recording, `05-admin`. It does not change the site: it only reads the CORS page and opens a policy
without saving, so it can be recorded again without restoring the snapshot. The slide steps need no recording.

## Clip `05-admin`: the CORS settings page in French

Starts on `/Admin/Cors` (no culture in the address), opened with `scene.OpenAsync` before the capture. Wait before
`StartCaptureAsync` until `scene.Page.Locator("#corsAdmin li.list-group-item")` is visible, so the list is already
drawn. The browser window keeps the default admin menu as it is. Keep the actions of each step shorter than its
narration (about 11, 8 and 10 seconds).

The page is a Vue app mounted on `#corsAdmin` (`OrchardCore.Cors/Views/Admin/Index.cshtml:17`). All of its text comes from
the dictionary of the group `cors-admin` (`Views/Admin/Index.cshtml:4`), through the template of
`OrchardCore.Cors/Assets/ts/cors-admin-index.ts`.

### `admin-1`

English page. Actions:

1. `scene.HighlightAsync(scene.Page.Locator("#corsAdmin #search-box"))`, then `scene.PauseAsync(1500)`, then `scene.ClearHighlightsAsync()`.
2. `scene.HighlightAsync(scene.Page.Locator("#corsAdmin button.btn-secondary"))`, `scene.PauseAsync(1500)`, `scene.ClearHighlightsAsync()`.
3. `scene.HighlightAsync(scene.Page.Locator("#corsAdmin span.badge"))` (the badge of `Public site`), `scene.PauseAsync(1500)`.

At the end of the narration the screen shows: the breadcrumb `CORS Settings` and the two server-drawn alerts in English; the
search box with the placeholder `Search`; the button `Add a policy`; the list with `Public site`, its badge `Default Policy`
and `Edit` and `Delete` buttons, and `Partner portal` with `Edit` and `Delete`. A green ring surrounds the badge.
Labels: `cors-admin-index.ts:337` (`Search`), `:341` (`Add a policy`), `:351` (`Default Policy`), `:352-353` (`Edit`,
`Delete`); the strings are in `CorsJSLocalizer.cs:16-20`.

### `admin-2`

Actions: `scene.ClearHighlightsAsync()`, then `scene.NavigateAsync("/Admin/Cors?culture=fr")`, then wait until
`scene.Page.Locator("#corsAdmin #search-box[placeholder='Rechercher']")` is visible and `scene.Page.Locator("#corsAdmin li.list-group-item")`
is visible. The `culture` query string is read by the request localization middleware of ASP.NET Core; `fr` is a supported
culture of the site (step 5 of "Prepare the site"). The same query string is used by the guide in
`src/docs/guides/install-localization-files/README.md`.

At the end of the narration the screen shows the same page in French: the placeholder `Rechercher`, the button
`Ajouter une politique`, the badge `Politique par défaut` after `Public site`, and `Modifier` and `Supprimer` on both rows.
The breadcrumb and the two alerts above the list stay in English: they are drawn by the server and are not in `fr.po`.
No highlight ring is visible.

### `admin-3`

Actions: `scene.ClickAsync(scene.Page.Locator("#corsAdmin li.list-group-item button.btn-primary").First)` (the `Modifier`
button of `Public site`, `cors-admin-index.ts:352`), then wait until
`scene.Page.Locator("#corsAdmin h5.card-title", new() { HasText = "Détails" })` is visible, then
`scene.HighlightAsync(scene.Page.Locator("#corsAdmin h5.card-title").First)`. Do not click `Enregistrer` or `Annuler`: the
site must stay as it is.

At the end of the narration the screen shows the policy `Public site`: the title `Public site`, the card `Détails` with the hint
`Renseignez les détails de la politique.`, the field `Nom de la politique` holding `Public site`, the hint `Le nom de la politique.`,
the checked box `Définir comme politique par défaut`, and the top of the card `Informations d'identification`. A green ring
surrounds `Détails`. Labels: `cors-admin-index.ts:97-98`, `:101`, `:104`, `:111`, `:119`.

## Slide steps

One line each: what the slide shows. All of them are drawn by `slides.py` from the code of the CORS module of Orchard Core
(`CorsJSLocalizer.cs`, `Startup.cs:32`, `Views/Admin/Index.cshtml`, `Assets/ts/cors-admin-index.ts`) and from the shared
`localizations.ts` helper that the script imports (its source ships in the source map
`OrchardCore.Cors/wwwroot/Scripts/cors-admin-index/cors-admin-index.js.map`).

- `why-1`: three cards: C# code translates with PO files; a script runs in the browser and can't call a string localizer; its labels still need translating.
- `why-2`: three cards, the path of a string: a localizer, a view, a script.
- `localizer-1`: `CorsJSLocalizer.cs`, with the usings, the class declaration and the `GetLocalizations` signature bright.
- `localizer-2`: the same file, with the group test, the dictionary and `return null` bright.
- `localizer-3`: `Startup.cs` with the `AddScoped` line, and two cards: Scoped; One per module.
- `view-1`: `Index.cshtml` with the `GetJSLocalizations` line bright, and two cards: asks every registered localizer; translated for the request.
- `view-2`: the same view with the `Json.Serialize` line and the `data-cors-translations` element bright, and two cards: a data attribute; part of the page.
- `script-1`: `cors-admin-index.ts` with the import, the lookup of the element, the `JSON.parse` and the `setTranslations` call.
- `script-2`: two panels: the `localizations.ts` helper (`getTranslations`, `setTranslations`), and a short read with `getTranslations()`.
- `po-1`: `fr.po` with two entries (context, message ID, translation), and three cards naming those parts.
- `po-2`: four cards: the three PO locations (module, site, tenant) and the restart after an edit.
- `groups-1`: a view requesting two groups, and three cards: group by group; null is skipped; the last one wins.
- `template-1`: the `PackageReference` of `OrchardCore.Localization.Build`, the `UpdateLocalizationCatalog` command, and two cards: C# only; constant keys.

The cards (`intro`, `outro`) show the title, the subtitle and the narration only.

## Checks for private data

- The only account on screen is `admin`; the only address is `admin@example.com`, and it does not appear in `05-admin`.
- The policies are `Public site` and `Partner portal`, with no origin, header or method added. No real domain, name or
  token is on screen. The French text is the text of `fr.po` above.
- Look at the contact sheet of `05-admin` (`review.py`): the end of each step must show what its narration describes, with
  no error message and no empty list.
- Scan the finished video as `SKILL.md` describes (frames at one per second and at scene changes), and search the text for
  real names, email addresses and tokens. Never publish a video that shows real data.
