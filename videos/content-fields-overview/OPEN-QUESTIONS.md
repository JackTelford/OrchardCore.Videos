# Open questions: content-fields-overview

Everything the narration states was read from the Orchard Core source at commit 92db4a83 (labels, settings, defaults,
validation, templates). The points below could not be seen on a running site. Confirm each one before the first take; if
one differs, change `recording-plan.md` and, when the narration is concerned, the storyboard.

## What the recorder must check

1. **Typing in date, date-time and time inputs.** `edit-5` types digits into `type="date"`, `type="datetime-local"` and
   `type="time"` inputs (`OrchardCore.ContentFields/Views/DateField.Edit.cshtml:16`, `DateTimeField.Edit.cshtml:15`, `TimeField.Edit.cshtml:16`).
   Checked: the input types. Not checked: where a click lands in the segmented input of headless Chromium, and whether the year
   segment moves on by itself after four digits (the plan sends `ArrowRight` after the year of the date-time input).
   Confirm with `Recorder probe /Admin/Contents/ContentTypes/Workshop/Create "() => document.querySelector('input[type=datetime-local]').value"`
   after typing. The helper `EnterAsync` already falls back to `FillAsync` with the expected value when the typed value differs.
2. **Admin menu clicks.** `open-1`, `open-2` and `edit-1` open and follow entries of the admin menu through `#left-nav figcaption.nav-item-row`
   (`OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml`, `NavigationItemLink-admin.cshtml`). Checked in the templates: a group without a page is a
   `button` with `aria-expanded`; a group that links to a page has a separate arrow button with `aria-expanded`; a page is an `a.item-label`; nothing is
   expanded when the menu renders. Not checked: that the three entries are visible at 1536x864, that the menu is not collapsed to icons, and which of the
   two forms Tools, Design, Content Definition and Content use. If the helpers do not work, open the groups before `StartCaptureAsync`, or use
   `NavigateAsync("/Admin/ContentTypes/List")` and keep the narration.
3. **The failed Publish of `edit-6`.** `NumericFieldDisplayDriver` adds `The value must be less than 40.`
   (`OrchardCore.ContentFields/Drivers/NumericFieldDisplayDriver.cs:94`). The create page also prints a validation summary when the model is
   invalid (`OrchardCore.Contents/Views/Admin/Create.cshtml:32-34`), so the message probably shows twice, at the top and under the box. Confirm
   the wording, the place, and that every other value is still in its box after the page loads again (the drivers rebuild the editor from the
   posted values). The narration only says "an error message".
4. **The front end of `08-view`.** Checked: each of the six templates prints the bare value in a `div` with the classes
   `field field-type-… field-name-<part>-<field>` (`OrchardCore.ContentFields/Views/*Field.cshtml`), TheBlogTheme has no template for the Workshop
   type, and its `Content.liquid` renders the content of an item (`OrchardCore.Themes/TheBlogTheme/Views/Content.liquid`). Not checked, on a running site:
   the exact text of each value. The sources give `True`, a short date (`ToShortDateString`), the local date and time of the Date Time Field
   (`DisplayDateTimeFieldViewModel.LocalDateTime`) and a `TimeSpan` printed as hours, minutes and seconds. The formats follow the culture of the page.
   The narration of `view-3` says only "the day", "the day and the time", "hours, minutes and seconds". Confirm that this is what the page shows,
   and that the six values are in the order of the type.
5. **Page loads that wait for the network.** `OpenAsync` and the navigation helpers wait for network idle. The theme asks for fonts from
   an outside host (`OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid`, Google fonts). On a machine without internet access, `08-view`
   can time out; allow the request, or block it with a route in the scene.
6. **Features page.** `open-1` filters the list with the search box and rings the row found by the id of its button
   (`btn-disable-OrchardCore_ContentFields`, `OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:53`).
   Confirm that the row is listed with the default visibility filters (**Hide on demand**, **Hide always enabled** are on by default,
   `Views/Admin/Features.cshtml:65-66`) and that the layout is the list layout the plan assumes.
7. **Content Types list.** `open-2` and `open-3` use `a.edit-button[href$='/ContentTypes/Edit/Workshop']`. The `.edit-button` part is needed: the name of the type in the row is a plain link to the
   same address (`OrchardCore.ContentTypes/Views/ContentTypeEntry.Fields.SummaryAdmin.cshtml`), so the address alone matches two elements. Confirm that the row's buttons are shown
   (the admin can also draw them in a menu, `ContentTypeEntry.SummaryAdmin.cshtml`) and that the **Edit** button is the only match.
8. **Settings pages.** `text-3` rings the settings and scrolls to the two lists. Confirm that the **Type** box shows **Editable** and that the
   **Pattern** box is hidden, so that no pattern text is on screen (`TextFieldSettings.Edit.cshtml:72-78`, `Assets/ts/text-field-settings.ts`).
   The pattern is Liquid; it must not appear in this admin video.
9. **The Field Type list.** It holds the fields of every enabled feature, sorted by class name, and the first one is checked when the page opens
   (`OrchardCore.ContentTypes/Controllers/AdminController.cs:690`, `AddFieldTo.cshtml:61`). The narration gives no count. Confirm that the six labels are
   found by `GetByText(..., Exact = true)` and that no other element of the page has the same text.
10. **Order of the Title and the fields in the editor.** The narration says only that the fields are listed in the order they were added
    (`OrchardCore.ContentTypes/Services/ContentDefinitionService.cs:494-519`). Confirm that all six fields fit in the window at `edit-2`
    (the plan scrolls once if not).
11. **The tab opened by View.** `view-1` follows a link with `target="_blank"` (`OrchardCore.Contents/Views/ContentsButtonEdit_SummaryAdmin.cshtml:18`) with
    `RunAndWaitForPageAsync` and `SwitchToAsync`. Confirm that the capture continues in the new tab.
12. **Time zone.** `dates-2` and `view-3` say that the Date Time Field is shown in the time zone of the site. The code uses `ILocalClock`, whose
    selector returns the site's **Default Time Zone** unless the **User Time Zone** feature is on (`OrchardCore.Settings/Services/DefaultTimeZoneSelector.cs`,
    `OrchardCore.Users/TimeZone/Services/UserTimeZoneSelector.cs`). The plan keeps that feature off. Confirm it on the Features page.

13. **The Date Field settings page** (`dates-1`). The narration says "Beyond the two lists, its settings are Required and a Hint". Checked: `EditField.cshtml:76-91` and `:97-112`
    draw each list only when the field type has option shapes, and the Date Field has `DateField.Option.cshtml`, `DateField-Localized.Option.cshtml` and `DateField.DisplayOption.cshtml`.
    Not checked: that the two lists are in view below **Hint** at 1536x864 without scrolling. Confirm it; if they are not, scroll with `ScrollToAsync` before the rings.

## What a person must confirm for the voice

14. The storyboard maps `Save & Configure` to `Save and Configure` in `spoken`, so the voice does not read the ampersand. Check the sample with
    `scripts/pronounce.py`. Also listen for "Boolean", "Numeric" and "Coordinated Universal Time".
