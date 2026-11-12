# Recording plan: site-settings-general

This plan lets a person record the video without deciding anything. Orchard Core source references are
`file:line` in `OrchardCMS/OrchardCore` (paths under `src/`). A selector in this plan stands for
`scene.Page.Locator(...)`. Confirm each selector once with `Recorder probe` before recording, and adjust scroll
amounts to the real page.

The video shows Settings, General only. It never opens the Debugging page, and it never reads or edits the value
of Page title format (see "Private data and rules" at the end).

## The site it needs

- **Orchard Core**: a checkout of `OrchardCMS/OrchardCore`, `src/OrchardCore.Cms.Web`, run on `http://localhost:5010`
  with its own `ORCHARD_APP_DATA` folder outside both repositories.
- **Auto Setup** (environment variables, no Setup screen and no sign-in form on camera): tenant `Default`, recipe
  `Blog`, database provider `Sqlite`, site name `Orchard Core`, time zone `America/Los_Angeles`, user `admin`, email
  `admin@example.com`, password `<password>` (the value of `VIDEO_PASSWORD`, chosen outside git). The variables are
  `OrchardCore__AutoSetup__Tenants__0__ShellName`, `...__SiteName`, `...__SiteTimeZone`, `...__AdminUsername`,
  `...__AdminEmail`, `...__AdminPassword`, `...__DatabaseProvider`, `...__RecipeName`.
- **Outbound calls**: set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address so the site never calls an outside service.
  The Blog theme loads Google Fonts from the page; the recording does not depend on them.
- **Features**: none to enable. The Blog recipe enables `OrchardCore.Deployment` (needed to seed the data below) and
  `OrchardCore.Contents`. `OrchardCore.Users.TimeZone` stays disabled, and no other feature is enabled. Settings,
  General is always available (`OrchardCore.Settings` cannot be disabled).
- **Sign in** once outside the capture with `Recorder setup` (user `admin`, `VIDEO_PASSWORD`), which saves `state.json`
  (git-ignored).
- **External services**: none, so no local stand-ins.

### The state the site must be in before the first take (snapshot A)

Right after Auto Setup, check on Settings, General (open it with `Recorder probe /Admin/Settings/general`):

| Field | Value expected |
|---|---|
| Site name | `Orchard Core` |
| Page title format | the template a site starts with (not edited) |
| Base url | empty |
| Default Time Zone | the entry for `America/Los_Angeles` |
| Page size | `10` |
| Page size selection | unchecked |
| Page size options (hidden until Page size selection is checked) | `10, 25, 50, 100` (a value, not a placeholder: `SiteSettings.cs:22`, `DefaultSiteSettingsDisplayDriver.cs:153`) |
| Resources: Use resources cache busting | checked |
| Resources: Use framework CDN (Content Delivery Network) | unchecked |
| Resources: Site CDN (Content Delivery Network) base url | empty |
| Resources: Resource Debug Mode | `From environment — disabled in Production, enabled otherwise` |
| Cache: Cache Mode | `From environment — enabled in Production, disabled otherwise` |

If a value differs, the narration of `site-2`, `site-3`, `size-1`, `size-4`, `res-1`, `res-2`, `res-3`, `res-4` or `res-5` no longer matches the screen:
record the difference in `OPEN-QUESTIONS.md` instead of editing the site to hide it.

### The fake seed data (create outside the capture, before snapshot A)

Content, Content Items must list more than 10 items, so that the pager has two pages at a page size of 10 and three
pages at 5. By the source, the Blog recipe leaves five listable items (the `Main Menu` menu, the `Tags` and
`Categories` taxonomies, the `Blog` item and the `About` article: the types Menu, Taxonomy, Blog and Article are
listable, the blog post and widget types are not), and the seed adds 10 published articles named
`Harbor Lane Note 1` to `Harbor Lane Note 10`: 15 items expected, 10 on the first page and 5 on the second. The
narration states no total, so a different total is fine as long as it is above 10 (with more than 15, the pager at
a page size of 5 shows more than three pages); write the real count in `OPEN-QUESTIONS.md`.

Create the 10 articles from the JSON of a recipe:

1. Print the recipe (Git Bash, Python 3):

   ```bash
   python -c "import json,uuid; items=[{'ContentType':'Article','ContentItemId':uuid.uuid4().hex[:26],'ContentItemVersionId':uuid.uuid4().hex[:26],'DisplayText':f'Harbor Lane Note {n}','Latest':True,'Published':True,'Author':'admin','TitlePart':{'Title':f'Harbor Lane Note {n}'}} for n in range(1,11)]; print(json.dumps({'steps':[{'name':'content','data':items}]}))"
   ```

2. Open `/Admin/DeploymentPlan/Import/Json` (Tools, Deployments, JSON Import), click in the JSON field (a code editor),
   select all, paste the output, and click `Import`. The page answers `Recipe imported successfully!`
   (`OrchardCore.Deployment/Controllers/ImportController.cs:185`).
3. Open `/Admin/Contents/ContentItems` and check the list: 15 items expected, on two pages (10 and 5), none with a
   real name. The author of each is `admin`. If the count differs, write it in `OPEN-QUESTIONS.md`.

Then stop the site and copy its `ORCHARD_APP_DATA` folder and `state.json` aside: that copy is **snapshot A**.

## Order of the clips and what each one changes

Record the clips in the order of the video, in one take from snapshot A, so the top bar shows `Harbor Lane Journal`
from `04-zone` on, as the viewer expects. Narrate first (`narrate.py`), then record.

| Order | Clip | Starts from | Changes the site? | Leaves the site with |
|---|---|---|---|---|
| 1 | `02-open` | snapshot A | no | unchanged |
| 2 | `03-site` | snapshot A | yes | Site name `Harbor Lane Journal` |
| 3 | `04-zone` | after `03-site` | yes | Default Time Zone `Europe/London` |
| 4 | `05-size` | after `04-zone` | yes | Page size `5`, Page size selection checked, Page size options `5, 10, 25` |
| 5 | `06-resources` | after `05-size` | no | unchanged |

The slide chapter (`01-about`) and the two cards need no site.

To record one clip again, stop the site, restore snapshot A, start the site, and bring it to the state that clip
starts from (the "Leaves the site with" values of the clips before it), saving them on Settings, General outside the
capture. A clip that saves restarts the tenant, so wait until a page answers before the capture starts.

## Selectors

| Name | Selector (confirm with `Recorder probe`) | Source |
|---|---|---|
| Settings menu group | `#settings button.item-label` | `TheAdmin/Views/NavigationItem-admin.cshtml:84`, `NavigationItemLink-admin.cshtml:29` |
| General menu entry | `#general a.item-label` (else `ul[data-title="Settings"] a:has-text("General")`) | `OrchardCore.Settings/AdminMenu.cs:36-44` |
| Page heading | `h1` | `OrchardCore.Settings/Views/Admin/Index.cshtml:4` |
| Restart warning | `p.alert.alert-warning` | `OrchardCore.Settings/Views/Settings-Reload.Wrapper.cshtml:3` |
| Tab links | `a.nav-link[role="tab"]`, filtered by their text | `OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:14-61` |
| Site name field | `input[name$="SiteName"]` | `Settings-Site.Edit.cshtml:16` |
| Page title format label | `label:text-is("Page title format")` | `Settings-Site.Edit.cshtml:25` |
| Base url label, field | `label:text-is("Base url")`, `input[name$="BaseUrl"]:not([name$="CdnBaseUrl"])` (a plain `[name$="BaseUrl"]` also matches the CDN field and fails in strict mode) | `Settings-Site.Edit.cshtml:34-36` |
| Default Time Zone label, select, hint | `label:text-is("Default Time Zone")`, `select[name$="TimeZone"]`, the `.hint` in the same `div.ocat-limited-wrapper` | `Settings-Site.Edit.cshtml:43-49` |
| Page size field | `input[name$="PageSize"]` (the only input whose name ends in `PageSize`) | `Settings-Site.Edit.cshtml:54-58` |
| Page size selection checkbox | `input[name$="AllowPageSizeSelection"][type="checkbox"]` | `Settings-Site.Edit.cshtml:66-67` |
| Page size options field, wrapper | `input[name$="PageSizeOptions"]`, `#pageSizeOptionsWrapper` | `Settings-Site.Edit.cshtml:73-76` |
| Save button | `button.save` (in `div.action-bar-bottom`, fixed at the bottom of the window) | `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`, `TheAdmin/Assets/scss/components/_action-bar.scss:14-16` |
| Success message | `.toast.message-success` (shown for 5 seconds) | `Settings/Controllers/AdminController.cs:119`, `Notify/NotifierExtensions.cs:71`, `TheAdmin/Views/Message.cshtml` |
| Top bar site name | `a.ta-navbar-brand` | `OrchardCore.Admin/Views/AdminBranding.cshtml:8` |
| Visit Site icon | `a[title="Visit Site"]`, else `a[data-bs-original-title="Visit Site"]` (the admin script moves the title into this attribute when it builds the tooltip), else `a.nav-link[href="/"]` in the top bar | `OrchardCore.Admin/Views/VisitSiteNavbarItem.cshtml:2` |
| Blog theme site name | `a.navbar-brand` | `TheBlogTheme/Views/Layout.liquid:27` |
| Cache busting checkbox | `input[name$="AppendVersion"][type="checkbox"]` | `Settings-Resources.Edit.cshtml:19` |
| Framework CDN checkbox | `input[name$="UseCdn"][type="checkbox"]` | `Settings-Resources.Edit.cshtml:29` |
| CDN base url label, field | `label:text-is("Site CDN (Content Delivery Network) base url")`, `input[name$="CdnBaseUrl"]` (not `has-text("base url")`: it also matches the Base url label of the Site tab) | `Settings-Resources.Edit.cshtml:36-38` |
| Resource Debug Mode select | `select[name$="ResourceDebugMode"]` | `Settings-Resources.Edit.cshtml:45-52` |
| Cache Mode select | `select[name$="CacheMode"]` | `Settings-Cache.Edit.cshtml:10-18` |
| Items per page select | `div.pager-page-size select` | `TheAdmin/Views/Pager_PageSizeSelector.cshtml:9-12` |

Common habits for every clip:

- Open the first page with `scene.OpenAsync` before `StartCaptureAsync`. Navigate during the capture only with
  `ClickAndWaitForNavigationAsync`, `WaitForNavigationAsync` or `NavigateAsync`.
- **Save composition.** Saving posts the form, restarts the tenant, and loads the redirected page, which can take a few
  seconds, and it navigates, so it goes through `WaitForNavigationAsync` (otherwise the screencast stays on the old
  document). Save sits in a bar fixed at the bottom of the window (`position: fixed`), so it is visible without scrolling. Move the cursor onto it on camera, then cut the restart out of the clip:

  ```csharp
  var save = scene.Page.Locator("button.save");
  await scene.MoveToAsync(save);
  await scene.OffCameraAsync(async () =>
  {
      await scene.WaitForNavigationAsync(() => save.ClickAsync());
      await scene.Page.Locator(".toast.message-success").WaitForAsync();
  });
  ```

  The click ripple is not on camera (the click happens off camera); the cursor is already on Save. The success
  message ("Site settings updated successfully.") lasts 5 seconds from the moment the reloaded page shows it, so it is
  on camera for most of that time right after the cut. If the wait times out, the save failed (a refused value): stop
  and read the page. Every "Save composition" below means this block.
- After a Save the page shows the Site tab again, whichever tab was active.
- Clear highlights before scrolling. Never highlight or zoom on the Page title format field itself.
- Select the time zone by its value (for example `Europe/London`): the visible label holds offsets that depend on
  daylight saving time.

## Chapter 01: What the settings are (slides)

The chapter has one clip, `01-about`, made of slides drawn by `slides.py`.

- `about-1`: the slide "One set of settings for the whole site" with four cards: Site name, Default Time Zone,
  Page size, Resources and caches.
- `about-2`: the slide "Where to find them" with three cards: Settings, General; Site, Resources, Cache; Other pages
  under Settings.
- `about-3`: the slide "Who can change them" with three cards: Manage general settings; Manage settings;
  Administrator.

Labels: the menu entries come from `OrchardCore.Settings/AdminMenu.cs:36-44`, the tab names from
`Drivers/DefaultSiteSettingsDisplayDriver.cs:51,54,57`, the permissions from
`OrchardCore.Settings.Core/SettingsPermissions.cs:8,13`, and the Administrator default from
`OrchardCore.Settings/Permissions.cs:17-25`.

## Chapter 02: Open the General settings (`02-open`)

Starts on `/Admin` (the admin home), signed in as `admin`, with the left menu collapsed to its groups. The Settings
group is at the end of the menu.

- `open-1`
  - Actions: click the Settings menu group (a button that expands its entries), then click the General menu entry
    with `ClickAndWaitForNavigationAsync`.
  - Visible at the end: the General settings page at `/Admin/Settings/general`, the Settings group expanded in the
    left menu with General marked as the current entry, and the heading `Settings`.
  - Labels: `OrchardCore.Settings/AdminMenu.cs:36,39`.
- `open-2`
  - Actions: highlight the heading, hold, clear it, highlight the restart warning.
  - Visible at the end: the warning "The website might be restarted upon saving the settings, potentially leading to
    temporary unresponsiveness during the process." with a green ring.
  - Labels: `Views/Admin/Index.cshtml:4` (the heading is the breadcrumb title `Settings`),
    `Views/Settings-Reload.Wrapper.cshtml:4`.
- `open-3`
  - Actions: clear the highlight, highlight the tab links (the row with Site, Resources, Cache), hold, clear it,
    highlight the Save button (it sits in a bar fixed at the bottom of the window, so no scrolling is needed).
  - Visible at the end: the Save button with a green ring, in the bar at the bottom of the window.
  - Labels: tab names `Drivers/DefaultSiteSettingsDisplayDriver.cs:51,54,57`, rendered unchanged by
    `GroupShapes.cs:59`; `Views/SiteSettings.SaveButton.cshtml:2`.

## Chapter 03: Site name and page title (`03-site`)

Starts on `/Admin/Settings/general`, Site tab. The Site name field reads `Orchard Core`.

- `site-1`
  - Actions: click the Site name field, empty it with `FillAsync("")`, then `TypeAsync` the text
    `Harbor Lane Journal`.
  - Visible at the end: the Site name field reading `Harbor Lane Journal`, the Site tab active.
  - Labels: `Views/Settings-Site.Edit.cshtml:16`.
- `site-2`
  - Actions: highlight the label `Page title format` (the label only, not the field), hold.
  - Visible at the end: the ring around the label `Page title format`. The field next to it shows its template; it
    is neither highlighted nor edited.
  - Labels: `Views/Settings-Site.Edit.cshtml:25`.
- `site-3`
  - Actions: clear the highlight, highlight the label `Base url`, hold.
  - Visible at the end: the ring around the label `Base url`, and the empty field next to it. Do not type in it: a
    cleared Base url is not saved as empty (see `OPEN-QUESTIONS.md`).
  - Labels: `Views/Settings-Site.Edit.cshtml:34`.
- `site-4`
  - Actions: clear the highlight, Save composition, highlight the top bar site name (the top bar is fixed, so no
    scrolling is needed).
  - Visible at the end: the top bar of the admin reading `Harbor Lane Journal` with a green ring, and the Site name
    field showing the same value. The message "Site settings updated successfully." shows for 5 seconds from the
    moment the page returns; it is gone at the end of the step, which is expected.
  - Labels: `Controllers/AdminController.cs:119`, `OrchardCore.Admin/Views/AdminBranding.cshtml:8`.
- `site-5`
  - Actions: clear the highlight, highlight the Visit Site icon with the caption `Visit Site`, click it with
    `ClickAndWaitForNavigationAsync`, then highlight the site name in the navigation bar of the Blog theme.
  - Visible at the end: the front end of the site (Blog theme), with `Harbor Lane Journal` at the top left of the
    navigation bar, ringed in green.
  - Labels: `OrchardCore.Admin/Views/VisitSiteNavbarItem.cshtml:2`, `TheBlogTheme/Views/Layout.liquid:27`.

## Chapter 04: Default time zone (`04-zone`)

Starts on `/Admin/Settings/general`, Site tab, signed in as `admin`. The top bar reads `Harbor Lane Journal`.

- `zone-1`
  - Actions: highlight the label `Default Time Zone` and the hint under the drop-down list, hold.
  - Visible at the end: the ring around the hint "Determines the default time zone used when displaying and editing
    dates and times. DST (daylight saving time) will be applied automatically per time zone if available." The
    drop-down list shows the entry for `America/Los_Angeles`.
  - Labels: `Views/Settings-Site.Edit.cshtml:43,49`.
- `zone-2`
  - Actions: clear highlights, highlight the drop-down list (it shows the entry for `America/Los_Angeles`, which is
    the entry the first sentence describes), `PauseAsync(8000)` while that sentence is spoken (it lasts about 9
    seconds), clear the highlight, click the drop-down list (a headless browser draws no open list: only the click
    ripple shows), then `SelectAsync` the label `Local to server`. The actions last about 11 seconds, shorter than the
    narration (about 15 seconds).
  - Visible at the end: the drop-down list closed and reading `Local to server`. Nothing is saved at this point.
  - Labels: `Views/Settings-Site.Edit.cshtml:46`; the entry text is built by `OrchardCore/Modules/Model/TimeZone.cs:25`
    (the standard offset from GMT, the name, then the wall offset of the instant at which the list was built, which
    includes daylight saving time only while it is in effect: the narration does not say "current").
- `zone-3`
  - Actions: `SelectAsync` the value `Europe/London`, Save composition.
  - Visible at the end: the drop-down list reading the entry for `Europe/London` (its name and offsets, as the label
    is built by `OrchardCore/Modules/Model/TimeZone.cs:25`). The message may already be gone.
  - Labels: `Views/Settings-Site.Edit.cshtml:43`, `Drivers/DefaultSiteSettingsDisplayDriver.cs:84`.
- `zone-4`
  - Actions: highlight the drop-down list and hold. Nothing else happens: the step is spoken over the page `zone-3`
    leaves.
  - Visible at the end: the drop-down list reading the entry for `Europe/London`, ringed in green.
  - Labels: the User Time Zone feature is `OrchardCore.Users/Manifest.cs:70-71`; it is not enabled in this recording and
    not shown (see `OPEN-QUESTIONS.md`).

## Chapter 05: Page size (`05-size`)

Starts on `/Admin/Contents/ContentItems`: the list holds 15 items by the source, ten on the first page and five on the
second. Scroll position at the top.

- `size-1`
  - Actions: hold on the top of the page for the first sentence of the narration (the heading `Manage Content` is
    visible), then scroll down until the pager under the list is visible and hold.
  - Visible at the end: the last rows of the first page and the pager with two pages, below the list.
  - Labels: the page is `OrchardCore.Contents/Controllers/AdminController.cs:67`, reached from the menu entry
    `Content`, `Content Items` (`OrchardCore.Contents/AdminMenu.cs:65,68`); its heading is the breadcrumb title
    `Manage Content` (`OrchardCore.Contents/Views/Admin/List.cshtml:17`), which is why the narration names the page
    "Manage Content" and the menu entry "Content Items", and says only "the list" afterwards; the pager is drawn by
    `OrchardCore.Navigation/PagerShapesTableProvider.cs` (`Pager_Links`).
- `size-2`
  - Actions: `NavigateAsync("/Admin/Settings/general")`, click the Page size field, empty it with `FillAsync("")`,
    `TypeAsync` the text `5`, Save composition.
  - Visible at the end: the Page size field reading `5` on the Site tab; the message "Site settings updated
    successfully." may still show.
  - Labels: `Views/Settings-Site.Edit.cshtml:54`, `Drivers/DefaultSiteSettingsDisplayDriver.cs:113,118` (the two
    refusals the narration names; they are not shown on camera).
- `size-3`
  - Actions: `NavigateAsync("/Admin/Contents/ContentItems")`; scroll down only if the pager is below the fold.
  - Visible at the end: 5 rows, and a pager with at least three pages under them (three, with 15 items).
  - Labels: `OrchardCore.Contents/Controllers/AdminController.cs:232`.
- `size-4`
  - Actions: `NavigateAsync("/Admin/Settings/general")`, `PauseAsync(3000)` (the first sentence, about 6 seconds, is
    spoken while the page shows the checkbox unchecked), click the Page size selection checkbox, highlight the field
    Page size options that appears, hold.
  - Visible at the end: the checkbox "Allow users to change the page size on listing pages" checked, the field
    `Page size options` shown under it (reading `10, 25, 50, 100`) with a green ring.
  - Labels: `Views/Settings-Site.Edit.cshtml:63,67,74`.
- `size-5`
  - Actions: clear the highlight, click the Page size options field, empty it with `FillAsync("")`, `TypeAsync` the
    text `25, 5, 10`, Save composition, highlight the Page size options field.
  - Visible at the end: the Page size options field reading `5, 10, 25` (sorted by
    `Drivers/DefaultSiteSettingsDisplayDriver.cs:192`), with a green ring, the checkbox still checked.
  - Labels: `Views/Settings-Site.Edit.cshtml:74`.
- `size-6`
  - Actions: `NavigateAsync("/Admin/Contents/ContentItems")`, scroll down to the pager, highlight the Items per page
    select (it reads `5`), `PauseAsync(2500)` (the first sentence is spoken while the select is ringed), clear the
    highlight, then `SelectAsync` the label `10` inside `WaitForNavigationAsync`, and scroll down to the pager again.
  - Visible at the end: the select labeled `Items per page` reading `10`, next to a pager with two pages, and the
    list above it with 10 rows.
  - Labels: `TheAdmin/Views/Pager_PageSizeSelector.cshtml:9`.

## Chapter 06: Resources and caches (`06-resources`)

Starts on `/Admin/Settings/general`, Site tab. This clip saves nothing.

- `res-1`
  - Actions: click the Resources tab, highlight the heading `Settings for scripts and stylesheets`, then the
    checkbox `Use resources cache busting` (checked).
  - Visible at the end: the Resources tab active, the checked box `Use resources cache busting` ringed in green.
  - Labels: `Views/Settings-Resources.Edit.cshtml:11,19`.
- `res-2`
  - Actions: clear the highlight, highlight the label `Use framework CDN (Content Delivery Network)` and its checkbox
    (unchecked), hold.
  - Visible at the end: the label and the unchecked box ringed in green, with the hint under them ("Whether a
    framework CDN is used for registered scripts and stylesheets, such as jQuery, or their local version").
  - Labels: `Views/Settings-Resources.Edit.cshtml:29,30`.
- `res-3`
  - Actions: clear the highlight, highlight the label and the empty field `Site CDN (Content Delivery Network) base url`.
  - Visible at the end: the label `Site CDN (Content Delivery Network) base url` and its empty field ringed in green,
    with the hint under the field ("A base CDN URL prefixed to the local scripts and stylesheets. It is disabled if the
    value is empty or if the Resource Debug Mode is enabled"; the hint ends with an example address of the product's own
    text). Nothing is typed in the field.
  - Labels: `Views/Settings-Resources.Edit.cshtml:36,40`; the exception the narration names is
    `OrchardCore.ResourceManagement.Abstractions/ResourceDefinition.cs:441-450` (`!settings.DebugMode`).
- `res-4`
  - Actions: clear the highlight, highlight the Resource Debug Mode select.
  - Visible at the end: the select reading `From environment — disabled in Production, enabled otherwise`, ringed in
    green, with the hint "Determines whether scripts and stylesheets load in their debuggable or minified form."
    under it.
  - Labels: `Views/Settings-Resources.Edit.cshtml:45,48,52`.
- `res-5`
  - Actions: clear the highlight, click the Cache tab, highlight the heading `Settings for dynamic caches`, then the
    Cache Mode select.
  - Visible at the end: the Cache tab active, the Cache Mode select reading `From environment — enabled in
    Production, disabled otherwise`, ringed in green.
  - Labels: `Views/Settings-Cache.Edit.cshtml:5,10,13`.

## Cards

- `intro`: the opening card (title "Orchard Core site settings").
- `outro`: the closing card.

## Private data and rules

- Everything on screen is the fake data above: the site name `Harbor Lane Journal`, the articles `Harbor Lane Note 1` to
  `Harbor Lane Note 10`, the user `admin`. No real name, email or address is typed. The password is not typed on
  camera, since `state.json` carries the session.
- Page title format holds a template by default. It is on screen in chapters 03 to 05 because it is a field of the
  page, but it is never highlighted, zoomed on, clicked or edited. If a frame holds it larger than the page shows it,
  record the step again.
- Base url and Site CDN (Content Delivery Network) base url stay empty.
- After recording, OCR the video at one frame per second and at every scene change, and search the text for names,
  email addresses other than `admin@example.com`, phone numbers and tokens.
- Do not commit the MP4, audio, `state.json`, or a copy of `ORCHARD_APP_DATA`.
