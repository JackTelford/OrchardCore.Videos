# Recording plan: HTML Sanitizer

Read this file alone to record the video. Only one chapter is a recording (`02-setting`, three steps). Every other chapter
is a slide or a card, drawn by `slides.py` and `assemble.py`, so it needs no browser.

In this file a selector stands for `scene.Page.Locator(...)`, since the recorder's methods take locators. Every label
below was read from the Orchard Core source at commit `92db4a83`; paths are relative to the root of that checkout.

## 1. The site it needs

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of the Orchard Core checkout, run on `http://localhost:5010` (`VIDEO_BASE_URL`). |
| Data | `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories. |
| Provisioning | Auto Setup (`src/docs/reference/modules/AutoSetup/README.md`), so no setup or sign-in form is recorded. Tenant `Default`, `SiteName` "Orchard Core", `SiteTimeZone` `America/Los_Angeles`, `AdminUsername` `admin`, `AdminEmail` `admin@example.com`, `DatabaseProvider` `Sqlite`, `RecipeName` `Blog`. The password is chosen outside git and passed in `VIDEO_PASSWORD`; in this file it is written `<password>`. |
| Outbound calls | `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address, as `.agents/skills/orchardcore-video-producer/references/recording.md` asks. The video needs no external service. |
| Sign-in | `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>` saves `state.json` (git-ignored). |
| Features to enable | None. The Blog recipe enables `OrchardCore.ContentTypes`, `OrchardCore.Html`, `OrchardCore.Markdown` and `OrchardCore.Menu` (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:31-82`). The sanitizer is not a feature: `AddOrchardCms` always registers it (`src/OrchardCore/OrchardCore.Application.Cms.Core.Targets/ServiceExtensions.cs:24`). |
| Settings | None. Leave every default. |
| Seed data | None. The video uses two content types that the Blog recipe already creates: `Article` (display name "Article", with an `HtmlBodyPart`, `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:130-186`) and `BlogPost` (display name "Blog Post", with a `MarkdownBodyPart`, `src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json:187-241`). Do not edit either type. |
| Mask script | Not needed: the site is new and shows only the fake data of the Blog recipe and the user `admin`. |

## 2. Order of the clips, and what changes the site

Only one clip is recorded: `02-setting`.

`02-setting` is read-only. It opens settings pages and never submits a form, so the site is the same after a take and
no snapshot has to be restored. Never click the **Sanitize Html** check box, **Save** or **Cancel** while recording.
If one is clicked by mistake, stop the site, restore the `App_Data` copy taken after Auto Setup, and start again.

## 3. Clip `02-setting`

Narration: three steps. Start the capture on the Content Types page.

### Measure the scrolls once, before recording

The scroll distances depend on the real page, so measure them instead of guessing. With the site running and the
recorder signed in (`Recorder setup`), run these three probes in Git Bash, from the folder of the built recorder
(`<bin>`):

```bash
export MSYS_NO_PATHCONV=1
<bin>/Recorder probe /Admin/ContentTypes/Edit/Article "() => Math.round(document.querySelector('#parts').getBoundingClientRect().top + window.scrollY)"
<bin>/Recorder probe /Admin/ContentTypes/Article/ContentParts/HtmlBodyPart/Edit "() => Math.round([...document.querySelectorAll('div.form-check')].find(e => e.textContent.includes('Sanitize Html')).getBoundingClientRect().top + window.scrollY)"
<bin>/Recorder probe /Admin/ContentTypes/BlogPost/ContentParts/MarkdownBodyPart/Edit "() => Math.round([...document.querySelectorAll('div.form-check')].find(e => e.textContent.includes('Sanitize Html')).getBoundingClientRect().top + window.scrollY)"
```

Each prints a number `T`: the distance in CSS pixels from the top of the page to the element. The viewport is 864 pixels
high. Call the three numbers `T1` (the Parts list), `T2` (the setting on the Article part) and `T3` (the setting on the
MarkdownBody part). In the steps below, scroll with `scene.ScrollAsync(0, T - 200)` when `T` is 600 or more, and do not
scroll when it is less. If `window.scrollY` stays 0 after a test scroll, the page scrolls inside a container: measure
that container's `scrollTop` the same way, and write that in `OPEN-QUESTIONS.md`.

### Before the capture (not recorded)

1. `scene.OpenAsync("/Admin/ContentTypes/List")`. This is the page of **Design > Content Definition > Content Types**
   (`src/OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23-25`; the route is
   `src/OrchardCore.Modules/OrchardCore.ContentTypes/Controllers/AdminController.cs:72`). The page title is "Content Types".
2. Make sure the admin menu shows **Design**, **Content Definition** and **Content Types**, with **Content Types**
   marked as the current page. If opening the page does not expand the branch, click **Design**, then **Content
   Definition**, before the capture starts.
3. Check that the list shows the types **Article** and **Blog Post**.
4. `scene.StartCaptureAsync()`.

### `setting-1`

Narration (about 11 seconds): the Content Types page lists the content types; select Edit for Article; the Parts list
includes HtmlBody; select Edit next to it.

Actions, in this order. Together they take about 7 seconds: hold each highlight for 700 ms at most, and skip action 1
if a take runs longer than the narration.

1. Optional. The Edit button of the **Article** row is `a[role='btn-edit-Article']`
   (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/ContentTypeEntry.Buttons.SummaryAdmin.cshtml:24`, label "Edit"). `scene.HighlightAsync(...)` it,
   hold 700 ms, then `scene.ClearHighlightsAsync()`.
2. `scene.ClickAndWaitForNavigationAsync(a[role='btn-edit-Article'])`. The page "Edit Content Type - Article" opens
   (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:63`; route `ContentTypes/Edit/{id}`,
   `src/OrchardCore.Modules/OrchardCore.ContentTypes/Controllers/AdminController.cs:167`).
3. Wait for the heading "Parts" (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:144`) to be shown. If `T1` is 600 or more,
   `scene.ScrollAsync(0, T1 - 200)`.
4. `scene.HighlightAsync(#parts li:has(a[href$='/ContentTypes/Article/ContentParts/HtmlBodyPart/Edit']))`, hold 700 ms,
   then `scene.ClearHighlightsAsync()`. The row reads "HtmlBody" (the part name "HtmlBodyPart" without its "Part" suffix:
   `src/OrchardCore/OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-61`,
   shown by `src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:146-163`).
5. `scene.ClickAndWaitForNavigationAsync(a[href$='/ContentTypes/Article/ContentParts/HtmlBodyPart/Edit'])`. This is the
   "Edit" button of the row (`src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:163`). The page opens at the top.

On screen when the narration of the step ends: the page titled "Edit Part - HtmlBody"
(`src/OrchardCore.Modules/OrchardCore.ContentTypes/Views/Admin/EditTypePart.cshtml:22`), with the breadcrumb "Content Types" and "Edit Content Type -
Article", at the top of the page. No highlight ring is left, and no field has been changed.

### `setting-2`

Narration (about 17 seconds): Sanitize Html is checked by default; its hint says the HTML is sanitized, for example by
removing script tags; clearing the box turns sanitization off for this part of this type; leave it checked unless you
trust every author.

Actions:

1. If `T2` is 600 or more, `scene.ScrollAsync(0, T2 - 200)`, then wait one second. The container of the setting is
   `div.form-check:has-text('Sanitize Html')`; it holds the check box, the label `label.form-check-label` with the text
   "Sanitize Html", and the hint `span.hint.dashed`
   (`src/OrchardCore.Modules/OrchardCore.Html/Views/HtmlBodyPartSettings.Edit.cshtml:7-10`).
2. `scene.MoveToAsync(label.form-check-label)` on the label with the text "Sanitize Html" (do not click it).
3. `scene.HighlightAsync(div.form-check:has-text('Sanitize Html'))`, with no caption. Keep the ring until the end of the
   step.

On screen when the narration of the step ends: the check box **Sanitize Html**, checked, the hint "Whether the Html is
sanitized, e.g. by removing script tags (and more)." and the link "See documentation", all inside the green ring. The
check box is still checked.

### `setting-3`

Narration (about 10 seconds): the Blog Post type has the same setting on its MarkdownBody part; the hint says it is the
HTML output from Markdown that is sanitized.

Actions:

1. `scene.ClearHighlightsAsync()`.
2. `scene.NavigateAsync("/Admin/ContentTypes/BlogPost/ContentParts/MarkdownBodyPart/Edit")`. This is the same page
   that **Design > Content Definition > Content Types > Blog Post > Parts > MarkdownBody > Edit** opens (route
   `ContentTypes/{id}/ContentParts/{name}/Edit`, `src/OrchardCore.Modules/OrchardCore.ContentTypes/Controllers/AdminController.cs:966`). The page is titled "Edit Part -
   MarkdownBody", and its breadcrumb reads "Edit Content Type - Blog Post".
3. If `T3` is 600 or more, `scene.ScrollAsync(0, T3 - 200)`, then wait one second.
4. `scene.HighlightAsync(div.form-check:has-text('Sanitize Html'))`
   (`src/OrchardCore.Modules/OrchardCore.Markdown/Views/MarkdownBodyPartSettings.Edit.cshtml:7-10`).

On screen when the narration of the step ends: the check box **Sanitize Html**, checked, and the hint "Whether the Html
output from Markdown is sanitized, e.g. by removing script tags (and more)." with the link "See documentation", inside
the green ring.

After the last step the capture ends. Do not leave the page by clicking anything.

## 4. Slides and cards

These steps are not recorded. `slides.py` draws them, `assemble.py` joins them.

| Step | What it shows |
|---|---|
| `intro` | The opening card: the logo, the title "The Orchard Core HTML Sanitizer" and the subtitle "Keep unsafe markup out of rendered content". |
| `idea-1` | Three cards, "Script elements", "Event handler attributes", "Script addresses": markup that runs in a browser. |
| `idea-2` | Two code panels, "Stored source" (a paragraph and a script element) and "Rendered page" (the paragraph only), and two cards: "Stored as written" and "Cleaned when displayed". |
| `idea-3` | Four cards in order: "Authored source", "Markdown to HTML", "Shortcodes", "Sanitizer". |
| `idea-4` | Four cards: the parts and fields sanitized by default, and the "Sanitize Html" setting. |
| `idea-5` | Four cards: the URL check of `LinkField`, `LinkMenuItemPart` and `HtmlMenuItemPart` when saved, how it works, its result, and the second check of link and HTML menu items when they are displayed (`src/OrchardCore.Modules/OrchardCore.Menu/MenuShapes.cs:157-193`, `:201-255`). |
| `use-1` | Three cards: the Razor, Liquid and C# ways to call the sanitizer. |
| `use-2` | A Razor line with `Orchard.SanitizeHtml`, and two cards: it takes a string, it returns `IHtmlContent`. |
| `use-3` | A Liquid template with `sanitize_html` and `raw`, and two cards: `raw` and `markdownify`. |
| `use-4` | The `HtmlDisplayService` of the Html module (`src/OrchardCore/OrchardCore.Html.Core/Services/HtmlDisplayService.cs`), trimmed: it takes an `IHtmlSanitizerService` and calls `Sanitize` when `sanitizeHtml` is true. |
| `configure-1` | The defaults Orchard Core sets (`class`, no `form`, `mailto`, `tel`) and three cards. |
| `configure-2` | `AddOrchardCms().ConfigureServices(...)` calling `ConfigureHtmlSanitizer` to allow `ftp` (the action runs for each tenant: `src/OrchardCore/OrchardCore.Abstractions/Modules/Builder/OrchardCoreBuilder.cs:26-45`), and one card. |
| `configure-3` | Two cards: the link checks use the same sanitizer and the same list of schemes. |
| `configure-4` | Three cards on the cache: results are cached, each tenant has its own, same fragment and same result. |
| `outro` | The closing card with the subtitle "docs.orchardcore.net". |

Before assembling, render every slide once in memory (see the skill, "Writing the slides") and look for text that
overflows.

## 5. Checks for private data

- The only user shown is `admin`, with the email `admin@example.com`; the user menu of the admin bar may show it.
- The content types and the settings pages show no content items, so no names, emails or addresses appear.
- The site name is "Orchard Core". No real domain appears in the address bar: it is `localhost:5010`.
- OCR the build at one frame per second and at every scene change, as the skill asks, and look for anything that is
  not the list above.
- Never commit `state.json`, `App_Data`, a mask script, or the password.
