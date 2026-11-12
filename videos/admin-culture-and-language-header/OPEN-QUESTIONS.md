# Open questions: Admin Culture Picker and the Content-Language Header

What was checked in the source, and what a person must confirm on a running site (features enabled, `en-US` and `fr`
supported, the seed `fr.po` of `recording-plan.md` in place) before or while recording.

## 1. What the Content-Language header carries

**Checked in the source**: `Localization/Startup.cs:75-81` sets `ApplyCurrentCultureToResponseHeaders = true` for the
feature `OrchardCore.Localization.ContentLanguageHeader`, and nothing else (no setting, no screen). The XML documentation of
ASP.NET Core says the option applies `CurrentUICulture` to the `Content-Language` response header
(`Microsoft.AspNetCore.Localization.xml`, member `RequestLocalizationOptions.ApplyCurrentCultureToResponseHeaders`). The
localizer reads `CultureInfo.CurrentUICulture` (`src/OrchardCore/OrchardCore.Localization.Core/PortableObject/PortableObjectStringLocalizer.cs:48`),
and the picker's cookie is read first, for the admin paths only (`Localization/RequestLocalizationOptionsConfigurations.cs:23`,
`Localization/AdminCookieCultureProvider.cs:23-28`). The narration of `header-1` and `header-2` follows from these four facts.

**To confirm on a running site**: with the browser's developer tools (Network tab), or `curl`:

```bash
curl -sI http://localhost:5010/ | grep -i content-language
```

- The front end, in English, answers with the culture name of the request (expected `en-US`).
- An admin page after choosing français (signed in, Network tab) answers with the culture of the cookie (expected `fr`).
- A response without the feature (before `enable-3`) has no such header.

The slide `header-2` names the cultures without printing a header value on purpose. If the values are confirmed as above,
the slide may show them (`en-US`, `fr`) on the cards.

## 2. Pronunciation (`pronounce.py`)

Confirm with `scripts/pronounce.py` before narrating, as the specification asks:

- "français" (`pick-2`, `header-2`): the multilingual voice should say the French word.
- "PO files" (`why-4`) is spoken "P.O. files" through the `spoken` map of `storyboard.json`; confirm the letters are said
  one by one at a natural pace.
- "Admin Culture Picker", "Content Language Header" and "Cultures" are read as written.

## 3. Names the screen shows

- The picker's text is `CultureInfo.NativeName`, written by .NET (`Localization/Views/AdminCulturePicker.cshtml:11-16,25`),
  not by a localizer. On the recording machine it should read "English (United States)" and "français". The narration
  says "français" (`pick-2`) and "English" (`pick-6`).
- Checked in the source: nothing caches the admin menu (`OrchardCore.Admin/AdminMenuFilter.cs` builds the `Navigation` shape on
  each request, and no `Cache` declaration touches it), so the menu of `pick-2` is built with the culture of the request.
  Confirm it on screen: the left menu must be in French after the reload.
- The option for the French culture in the Cultures page's drop-down list reads `fr (French)`: the built script draws
  `Name (DisplayName)` (`Localization/wwwroot/Scripts/localization-settings/localization-settings.js`). Confirm that it is
  listed and that its value is `fr` (`SelectAsync` uses the value).
- The narration says French for the option of `cultures-2` and the menu and labels "that have a French translation"
  for `pick-2`: confirm on screen that the seeded labels change (the left menu, the breadcrumb, the editor of the Cultures
  page) and that the yellow warning and the Visit Site tooltip stay English.

## 4. Behaviours to see once on the running site

- `cultures-1`: with one supported culture the top bar shows no culture picker. Checked in the source
  (`Localization/Drivers/AdminCulturePickerNavbarDisplayDriver.cs:36`, the picker renders only when more than one culture is
  supported); confirm on screen, and that Save restarts the tenant in a few seconds.
- `pick-4`: the front end stays in English after the admin was switched to French. Checked in the source: the provider of
  the cookie answers only for the admin paths (`Localization/AdminCookieCultureProvider.cs:23-28`); the front end then follows
  the other providers and the recorder's `en-US` locale. Confirm that the Blog front end shows no French text.
- `pick-5`: the next admin page keeps the French culture (the cookie has no expiry in `Localization/Assets/ts/admin-culture-selector.ts:11`;
  the narration claims nothing about how long it lasts).
- `enable-1` to `enable-4`: neither feature is hidden by the default filters of the Features page ("Hide on demand",
  "Hide always enabled"), and each Enable click returns to the Features page within the narration of its step. If a tenant
  restart takes longer, record the step again, or end the step on the notification.

## 5. Not shown, only stated

- `why-2` says the admin follows the text direction of a language written right to left. Checked in the source
  (`src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:19,31` set `dir` and load the right-to-left style sheet from the
  current UI culture); the video does not record it. A person can confirm by adding a right-to-left culture (for
  example `ar`) and choosing it with the picker.

## 6. Timing of the success notification

**Checked in the source**: a success notification is removed 5 seconds after the page loads, a warning stays
(`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`, `TheAdmin/Views/Message.cshtml:11-20`).

**To confirm on a running site**: in `enable-2` the notification "The feature Admin Culture Picker was enabled." is on
screen while the narrator says "A message confirms that the feature was enabled." (it depends on how long the tenant
restart takes), and the machine's installed UI culture is `en-US` (see the recording plan, "Machine").
