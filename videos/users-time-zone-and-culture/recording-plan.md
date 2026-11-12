# Recording plan: User Time Zone and Culture

Read this top to bottom, then record. Nothing here needs a decision: every action, text, wait and expected frame is
written down. A selector in backticks stands for `scene.Page.Locator(...)`; "the field labelled X" stands for
`scene.Page.GetByLabel("X")`. File references are to the Orchard Core source at the commit named in
`catalog/SOURCE.md` (`src/...`).

The video has three recordings (`02-prepare`, `03-edit`, `04-result`) and two slide clips (`01-why`, `05-order`).

## 1. The site it needs

A fresh site, never a copy of a real one, so no mask script is needed.

- **Host:** `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`
  (`VIDEO_BASE_URL`), with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories. Block outbound
  calls with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address.
- **Auto Setup:** the setup screen is not part of this video, so the site is provisioned by Auto Setup through
  environment variables (see `references/recording.md`):

  | Variable (`OrchardCore__AutoSetup__Tenants__0__...`) | Value |
  | --- | --- |
  | `ShellName` | `Default` |
  | `SiteName` | `Orchard Core` |
  | `SiteTimeZone` | `America/Los_Angeles` |
  | `AdminUsername` | `admin` |
  | `AdminEmail` | `admin@example.com` |
  | `AdminPassword` | `<password>` (chosen outside git, the same value as `VIDEO_PASSWORD`) |
  | `DatabaseProvider` | `Sqlite` |
  | `RecipeName` | `Blog` |

- **Site time zone:** `America/Los_Angeles`. It is the site's own **Default Time Zone** and the story depends on it.
  Check it at `/Admin/Settings/general`: the field labelled `Default Time Zone` shows `America/Los_Angeles`
  (`src/OrchardCore.Modules/OrchardCore.Settings/Views/Settings-Site.Edit.cshtml:43`).
- **Sign in once:** `Recorder setup` with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD=<password>`, which saves
  `state.json` (git-ignored). The password is never typed on camera.
- **Browser:** the recorder's default (`en-US`, light). Do not change the browser language: the video relies on the
  site's own culture being English until the user chooses French.
- **Features at the start of `02-prepare`:** everything the Blog recipe enables, plus **Localization** (seed A below).
  **User Time Zone** and **User Localization** must be disabled: the video enables them on camera.
- **Do not enable** Admin Culture Picker or Content Language Header.

## 2. The fake seed data (outside the capture)

All three seeds use **Tools, Deployments, JSON Import** (`/Admin/DeploymentPlan/Import/Json`): paste the JSON into the
editor, then select **Import** (`src/OrchardCore.Modules/OrchardCore.Deployment/Views/Import/Json.cshtml:15,21`).
The result is the message "Recipe imported successfully!"
(`src/OrchardCore.Modules/OrchardCore.Deployment/Controllers/ImportController.cs:185`). Do the seeds in this order
and wait for the site to answer between them, since a feature change restarts the tenant.

**Seed A: the Localization feature** (User Localization depends on it, and the Blog recipe does not enable it).

```json
{
  "steps": [
    { "name": "feature", "enable": [ "OrchardCore.Localization" ] }
  ]
}
```

Check: **Tools, Features**, search `localization`: the row **Localization** shows Enabled.

**Seed B: the supported cultures** (English is the default, French is also supported).

```json
{
  "steps": [
    {
      "name": "settings",
      "LocalizationSettings": {
        "DefaultCulture": "en-US",
        "SupportedCultures": [ "en-US", "fr-FR" ]
      }
    },
    { "name": "ReloadTenant" }
  ]
}
```

Check: `/Admin/Settings/localization` shows a table with exactly two rows, `en-US` (its radio button selected, as the
default) and `fr-FR`. If the table is different, restart the site and look again; if it is still different, remove the
extra rows with their trash buttons, add the missing culture with **Add culture**, select the `en-US` radio button, and
select **Save**.

**Seed C: the blog post** that shows the date. Its creation time is `2026-03-15T03:30:00Z`. That is the evening of
March 14 in Los Angeles (UTC-7 on that date) and the early morning of March 15 in Paris (UTC+1 on that date), so the
two time zones show different days, whatever the day the video is recorded. The post has no container, so it is not
in the list of the blog: it is reached by its path.

```json
{
  "steps": [
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "4qm7x2k9r5t8c1v6b3n0d4w2ya",
          "ContentType": "BlogPost",
          "DisplayText": "A walk through the market",
          "Latest": true,
          "Published": true,
          "CreatedUtc": "2026-03-15T03:30:00Z",
          "ModifiedUtc": "2026-03-15T03:30:00Z",
          "PublishedUtc": "2026-03-15T03:30:00Z",
          "Author": "admin",
          "TitlePart": { "Title": "A walk through the market" },
          "AutoroutePart": { "Path": "blog/market-walk" },
          "MarkdownBodyPart": {
            "Markdown": "Fresh bread, ripe pears and a stall of spices. Saturday morning at the market is slow, loud and full of colour."
          },
          "BlogPost": { "Subtitle": { "Text": "Saturday morning at the stalls" } }
        }
      ]
    }
  ]
}
```

Check: open `/blog/market-walk` signed in as admin. The header shows the title **A walk through the market** and the
line **Posted by admin on March 14, 2026**. If it shows March 15, the site time zone is not `America/Los_Angeles`.
If the page is a 404, find the item's URL under **Content, Content Items** (Edit, the field labelled `Permalink`, `src/OrchardCore.Modules/OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:30`) and use that URL
wherever this plan says `/blog/market-walk`.

**No other data.** The Users list holds one user: `admin` with `admin@example.com`. Do not create other users.

## 3. Snapshots, order and what each clip changes

Record in this order. The slide clips need nothing from the site.

| Clip | Starts from | Changes the site? | Snapshot to restore before a new take |
| --- | --- | --- | --- |
| `02-prepare` | S0 | Yes: enables User Time Zone and User Localization | S0 |
| `03-edit` | S1 | Yes: saves the time zone and culture of `admin` | S1 |
| `04-result` | S2 | No | S2 |

- **S0:** after the three seeds. Stop the site, copy `App_Data` and `state.json` aside as S0, start the site.
- **S1:** after `02-prepare`. Copy `App_Data` and `state.json` aside as S1.
- **S2:** after `03-edit`. **Run `Recorder setup` again before copying**, so `state.json` holds a sign-in made after
  the culture was saved. The culture is carried by a claim of the sign-in cookie
  (`src/OrchardCore.Modules/OrchardCore.Users/Localization/Providers/UserLocalizationClaimsProvider.cs:23-25`), and the
  cookie of S0 and S1 was issued before the choice. Copy `App_Data` and the new `state.json` aside as S2.
- To record one clip again, stop the site, restore the `App_Data` and `state.json` of its snapshot, and start the
  site. `03-edit` must start with both user fields on their default options, which is the state of S1.
- The fields of `admin` and the two features are the only things the three clips change.

## 4. Clips

### `01-why` (slides)

- `why-1`: the slide "One site, many places": three cards, the site's time zone, the site's culture, and people
  elsewhere.
- `why-2`: the slide "Two features, two fields": four cards, User Time Zone, User Localization, Personal, Optional.

### `02-prepare` (recording)

**Starts on:** `/blog/market-walk`, signed in as admin (S0). Load it before `StartCaptureAsync`.

- `prepare-1`
  - Actions: ring the line with `HighlightAsync(.post-heading .meta)`, no caption, then `HoldAsync` until the step ends.
  - Visible at the end: the post header with the title, the subtitle, and the ringed line
    **Posted by admin on March 14, 2026**.
  - Source: the line comes from `src/OrchardCore.Themes/TheBlogTheme/Views/Content-BlogPost.liquid:17-20`; the date
    is written with the format `MMMM dd, yyyy`.
- `prepare-2`
  - Actions: `ClearHighlightsAsync()`; `NavigateAsync("/Admin/Features")`; wait until `#search-box` is visible;
    `TypeAsync(#search-box, "user time zone")`; ring the row of User Time Zone with `HighlightAsync` on
    `[data-filter-value]` that contains the text `OrchardCore.Users.TimeZone`; then
    `ClickAndWaitForNavigationAsync` on `#btn-enable-OrchardCore_Users_TimeZone`. If that id does not exist, use the
    link named **Enable** inside the same row. Click about three seconds before the end of the narration (during
    "Enable turns it on."). Then wait, with `OffCameraAsync`, until the text
    **The feature User Time Zone was enabled.** is visible; the first answer after a feature change can take several
    seconds. The search matches the name, the id and the description of a feature, so the full name leaves this one
    row on screen; a search for `user` would list every feature that mentions users
    (`src/OrchardCore.Modules/OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`, filter in
    `.../Assets/ts/features-index.ts:97-101`).
  - Visible at the end: the Features page with the green message "The feature User Time Zone was enabled."; the
    sidebar shows Tools expanded with Features marked, if it was left open. The message hides itself about five
    seconds after the page is shown (`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`,
    `src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml:12-20`), so the click must not come earlier than the
    three seconds above.
  - Sources: Tools, Features `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`; search box
    `.../Views/Admin/Features.cshtml:27`; the Enable button `.../Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`;
    the name and id of the row `.../Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11`; the message
    `.../Controllers/AdminController.cs:317`; the feature name "User Time Zone"
    `src/OrchardCore.Modules/OrchardCore.Users/Manifest.cs:71`.
- `prepare-3`
  - Actions: `TypeAsync(#search-box, "user localization")` (the reload emptied it; the full name leaves this one row
    on screen); ring the dependency badges of the User
    Localization row with `HighlightAsync` on `[data-filter-value]` that contains the text
    `OrchardCore.Users.Localization`, then its `.direct-dependencies`; then `ClickAndWaitForNavigationAsync` on
    `#btn-enable-OrchardCore_Users_Localization` (or the **Enable** link of that row), about three seconds before the
    end of the narration; then `OffCameraAsync` until the text **The feature User Localization was enabled.** is
    visible.
  - Visible at the end: the Features page with the green message "The feature User Localization was enabled." The
    message hides itself about five seconds after the page is shown (see `prepare-2`), so keep the click three seconds
    before the end of the narration.
  - Sources: the badges `.../Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:12-20` (the row shows the badges
    **Localization** and **Users**); the feature name "User Localization"
    `src/OrchardCore.Modules/OrchardCore.Users/Manifest.cs:79`; the dependency on Localization `Manifest.cs:81-85`.
- `prepare-4`
  - Actions: `ClearHighlightsAsync()`; `NavigateAsync("/Admin/Settings/localization")`; wait until the table is shown
    (`.localization-settings-wrapper table`); ring the table with `HighlightAsync`; about five seconds later, clear it
    and ring the button `a.btn-success` named **Add culture**. Do not select anything and do not save.
  - Visible at the end: the Cultures page with the table (`en-US` selected as the default, `fr-FR`), and the green
    **Add culture** button ringed.
  - Sources: Settings, Localization, Cultures `src/OrchardCore.Modules/OrchardCore.Localization/AdminMenu.cs:34-38`;
    the table and its columns "Culture" and "Default culture", and the button, are drawn by
    `.../wwwroot/Scripts/localization-settings/localization-settings.js`, with the words from
    `.../Services/LocalizationJSLocalizer.cs:16-20`; the sentence above the table
    `.../Views/LocalizationSettings.Edit.cshtml:31`.

### `03-edit` (recording)

**Starts on:** `/Admin/Users/Index` (S1), the list with the single user `admin` / `admin@example.com`. Load it before
`StartCaptureAsync`.

- `edit-1`
  - Actions: ring the **Edit** button of the row with `HighlightAsync` on `a.btn.btn-primary.btn-sm` named **Edit**
    (the only one on the page); about two seconds before the end of the narration, `ClearHighlightsAsync()` and
    `ClickAndWaitForNavigationAsync` on it.
  - Visible at the end: the page titled **Edit User** (breadcrumb "Users" then "Edit User") with the user name
    `admin` and the email `admin@example.com`.
  - Sources: Access Control, Users `src/OrchardCore.Modules/OrchardCore.Users/AdminMenu.cs:27-30`; **Edit**
    `.../Views/UserButtons.cshtml:11`; **Edit User** `.../Views/Admin/Edit.cshtml:4`.
- `edit-2`
  - Actions: scroll the field labelled `Default Time Zone` into view (`ScrollAsync`, or a scroll to the element, then
    `PauseAsync(600)`); ring both fields with `HighlightAsync` on the field labelled `Default Time Zone` and on the
    field labelled `Default User Culture`. Do not change them.
  - Visible at the end: both fields on screen at once, each ringed, the first showing **Use site's time zone** and the
    second **Use site's culture**. If both do not fit, scroll so that the time zone field is at the top.
  - Sources: `src/OrchardCore.Modules/OrchardCore.Users/Views/UserTimeZone.Edit.cshtml:15,18`;
    `.../Views/UserCulture.Edit.cshtml:5`; "Use site's culture"
    `.../Localization/Drivers/UserLocalizationDisplayDriver.cs:43`. The order of the two fields is not fixed (both
    are placed at `Content:2`): narrate and record whichever order the page shows.
- `edit-3`
  - Actions: `ClearHighlightsAsync()`; ring the hint under the time zone field (`.hint` of that field); about four
    seconds later, `SelectAsync` on the field labelled `Default Time Zone` with the value `Europe/Paris`.
  - Visible at the end: the time zone field showing the option for `Europe/Paris` (its text starts with `(GMT+01)`).
  - Sources: hint `.../Views/UserTimeZone.Edit.cshtml:21`; the option text, from
    `src/OrchardCore/OrchardCore/Modules/Model/TimeZone.cs:25`, is the offset, the zone name and the current offset.
- `edit-4`
  - Actions: `ClearHighlightsAsync()`; `SelectAsync` on the field labelled `Default User Culture` with the value
    `fr-FR`; `PauseAsync(800)`; then `ClickAndWaitForNavigationAsync` on `button.save` named **Save**, about four
    seconds before the end of the narration.
  - Visible at the end: the Users list, with the green message **User updated successfully.** if it is still shown
    (it hides itself about five seconds after the page is shown, see `prepare-2`; the narration says that a message
    confirms the update, so the click comes four seconds before the end, as above, and the message is on screen
    during that sentence).
  - Sources: **Save** `.../Views/UserSaveButtons.Edit.cshtml:2`; the message `.../Controllers/AdminController.cs:406`;
    the refresh of the sign-in when a user saves their own account `.../Controllers/AdminController.cs:403`.

### `04-result` (recording)

**Before recording:** restore S2 (the `App_Data` after `03-edit` and the `state.json` made by a new `Recorder setup`).
**Starts on:** `/blog/market-walk`, signed in as admin. Load it before `StartCaptureAsync`.

- `result-1`
  - Actions: ring the line with `HighlightAsync(.post-heading .meta)`, no caption, then `HoldAsync` until the step
    ends.
  - Visible at the end: the post header with the ringed line **Posted by admin on mars 15, 2026**: the 15th instead
    of the 14th, and the month in French.
  - Sources: `Content-BlogPost.liquid:17-20`; the date is converted to the time zone of the signed-in user
    (`src/OrchardCore.Modules/OrchardCore.Users/TimeZone/Services/UserTimeZoneSelector.cs:28`, priority 100, against
    `.../OrchardCore.Settings/Services/DefaultTimeZoneSelector.cs:23`, priority 0) and written with the culture of the
    request (`src/OrchardCore/OrchardCore.DisplayManagement/Shapes/DateTimeShapes.cs:123`).

### `05-order` (slides)

- `order-1`: the slide "Which time zone applies": three cards, the user's choice, the site's setting, the server.
- `order-2`: the slide "Which culture applies": three cards, the user's choice, Use site's culture, the choices.
- `order-3`: the slide "Who sets the preferences": three cards, administrators, users themselves, visitors.
  The card of the users themselves names two permissions, **Edit own user information** and **Access admin panel**:
  the Profile link needs the first (`src/OrchardCore.Modules/OrchardCore.Users/Views/UserMenuItems-Profile.cshtml:5`,
  label `src/OrchardCore/OrchardCore.Users.Core/UsersPermissions.cs:31`), and the page it opens is in the admin area,
  which needs the second (`src/OrchardCore.Modules/OrchardCore.Admin/AdminFilter.cs:56-58`, label
  `src/OrchardCore/OrchardCore.Admin.Abstractions/AdminPermissions.cs:8`; the Users controller is marked with the
  admin attribute, `.../OrchardCore.Users/Controllers/AdminController.cs:28`).

## 5. Checks for private data

- The only account on screen is `admin` / `admin@example.com`. The content is the fake post "A walk through the
  market" and the sample content that the Blog recipe creates itself (the blog, its tags and categories, one sample
  post whose author is `admin`); the titles of the most recent posts can appear below the post
  (`Content-BlogPost.liquid:34-37`, the query `RecentBlogPosts` in `blog.recipe.json:1176-1186`). That sample content
  is invented. No other name, email address, phone number or domain than `example.com` may appear.
- The password never appears: sign-in comes from `state.json`. If a sign-in page flashes, record again.
- Look at every contact sheet (`review.py`) for error pages, the 404 page, and the setup screen.
- OCR the final video at one frame per second and at scene changes, and search for anything that is not in the list
  above.
