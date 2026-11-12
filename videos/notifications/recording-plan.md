# Recording plan: Notifications

An admin video: every clip is a browser recording of the admin UI, and the two slide chapters are drawn by `slides.py`.
Read this page top to bottom once; then record the clips in the order of section 3. Labels are quoted exactly as the
Orchard Core source spells them, with the file and line they come from (commit `92db4a83` of `OrchardCMS/OrchardCore`).
File paths are relative to `src/` of the checkout, and the folder `OrchardCore.Modules/` is left out of the paths of
modules: `OrchardCore.Notifications/Manifest.cs` is `src/OrchardCore.Modules/OrchardCore.Notifications/Manifest.cs`, while
`OrchardCore/...` and `OrchardCore.Themes/...` are under `src/` as written. A selector written in this page stands for
`scene.Page.Locator(...)`. The sign-in is user `admin`, email `admin@example.com`, and the password `<password>`
chosen outside git (`VIDEO_PASSWORD`).

## 1. The site it needs

### 1.1 Run it

- `src/OrchardCore.Cms.Web` of the checkout, on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty
  folder outside both repositories, provisioned by Auto Setup (see the `orchardcore-tester` skill): recipe **Blog**,
  site name **Orchard Core**, time zone `America/Los_Angeles`, database provider `Sqlite` (the value the `orchardcore-tester` skill
  uses for Auto Setup), user `admin`, email `admin@example.com`, password `<password>`.
  The setup form and the sign-in are not recorded.
- Run it with `ASPNETCORE_ENVIRONMENT=Development`. The **Log** SMS provider that the video relies on is only
  registered in the Development environment (`OrchardCore.Sms/Startup.cs:31-34`).
- Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so that nothing leaves the machine. Email goes to a pickup folder
  and SMS to the log, so no mail or SMS service is called.
- Leave the admin list layout at its defaults: the **List** layout, with the row actions as buttons followed by an
  **Actions** menu (`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs:30,36`). The narration describes that layout.
- Nothing needs a mask script: the site is made from scratch and only holds the fake data of section 1.4.

### 1.2 Starting state (snapshot S0), made outside the capture

Do these in the admin, in this order, then run `Recorder setup`, stop the site, and copy `App_Data` and `state.json` aside.

1. **Tools, Features** (`OrchardCore.Features/AdminMenu.cs:26-27`): enable `OrchardCore.Workflows`, `OrchardCore.Email`,
   `OrchardCore.Email.Smtp` and `OrchardCore.Sms`. Select each row by
   `input[name='featureIds'][value='<feature id>']`, then **Bulk Actions**, **Enable**. Do **not** enable any of the three
   Notifications features: clip `02-enable` does it.
2. **Settings, Communication, Email** (`OrchardCore.Email/AdminMenu.cs:39-40`):
   on the **SMTP** tab check **Enable SMTP Provider** (`OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml:10`),
   set **Sender email address** to `noreply@example.com` (line 17), set **Delivery method** to
   **Specified pickup directory** (lines 26-33) and keep the default pickup location, then save. Open the page again and,
   on the **Providers** tab, set **Default provider** (`OrchardCore.Email/Views/EmailSettings.Edit.cshtml:20`) to the SMTP
   provider, then save.
3. **Settings, Communication, SMS** (`OrchardCore.Sms/AdminMenu.cs:29`): set **Default SMS provider**
   (`OrchardCore.Sms/Views/SmsSettings.Edit.cshtml:6`) to `Log`, then save.
4. **Settings, Security, User Login** (`OrchardCore.Users/AdminMenu.cs:41-42`): check
   **Allow user to change their phone number** (`OrchardCore.Users/Views/LoginSettings.Edit.cshtml:52`), then save.
   Without it the **Phone Number** field of an existing user is read-only.
5. **Access Control, Users** (`OrchardCore.Users/AdminMenu.cs:27`): edit the user `admin` and set **Phone Number** to
   `+12015550123`, then **Save**. Then **Add User** (`OrchardCore.Users/Views/UsersAdminListCreate.cshtml:7`) with
   **User name** `jamie.chen`, **Email** `jamie.chen@example.com`, **Phone Number** `+12015550124`, a password chosen
   outside git, and the role Editor.
6. **Content, Content Items**, **New**, **Page**: **Title** `Contact us`, then **Save Draft**
   (`OrchardCore.Contents/Views/Content.SaveDraftButton.cshtml:7`). Never publish it: clip `03-workflow` publishes it.

### 1.3 Seed after clip `02-enable` (snapshot S1), made outside the capture

The notification tasks exist only once the Notifications features are enabled, so this part runs after clip `02-enable`.

1. **Tools, Deployments, JSON Import** (`OrchardCore.Deployment/AdminMenu.cs:30`): paste the recipe below in the **JSON**
   box and select **Import** (`OrchardCore.Deployment/Views/Import/Json.cshtml:21`). The `WorkflowType` step saves the two
   workflows directly (`OrchardCore.Workflows/Recipes/WorkflowTypeStep.cs:89`). The notification content is plain text on
   purpose: it uses no Liquid, so no template code is ever on screen.
2. Open **Design, Workflows**: both workflows must be listed. Open the first one, check that its two activities show no
   warning badge, and go back. If an activity is shown as missing, the Notifications features are not enabled.
3. **Content, Content Items**, **New**, **Page**, in this order, always with the **Publish** button:
   1. **Title** `About the market`, **Publish**.
   2. **Title** `Old market map`, **Publish**.
   3. In the **Content Items** list, on the row of `Old market map`, **Actions**, **Unpublish**, then **Yes**
      (`OrchardCore.Contents/Views/ContentsButtonActions_SummaryAdmin.cshtml:35`).
4. Check: the bell badge shows `3`, and `/Admin/notifications` lists, newest first, `A page was unpublished`,
   `A page was published`, `A page was published`, all unread. `Contact us` is still a draft.
5. Copy `App_Data` aside as snapshot S1.

Record clips `03-workflow` to `06-preferences` within about an hour of this seed, so that the rows read
"minutes ago" and not "days ago".

```json
{
  "steps": [
    {
      "name": "WorkflowType",
      "data": [
        {
          "WorkflowTypeId": "notifyeditorspublished",
          "Name": "Notify the editors when a page is published",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Activities": [
            {
              "ActivityId": "pagepublished",
              "Name": "ContentPublishedEvent",
              "X": 40,
              "Y": 40,
              "IsStart": true,
              "Properties": { "ContentTypeFilter": [ "Page" ] }
            },
            {
              "ActivityId": "notifyeditors",
              "Name": "NotifyUserTask",
              "X": 380,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "UserNames": { "Expression": "ADMIN, JAMIE.CHEN" },
                "Subject": { "Expression": "A page was published" },
                "Summary": { "Expression": "Check the page and share it with your readers." },
                "TextBody": { "Expression": "The page is live on the site." },
                "HtmlBody": { "Expression": "" },
                "IsHtmlPreferred": false,
                "Url": { "Expression": "" }
              }
            }
          ],
          "Transitions": [
            { "SourceActivityId": "pagepublished", "SourceOutcomeName": "Done", "DestinationActivityId": "notifyeditors" }
          ]
        },
        {
          "WorkflowTypeId": "notifyeditorsunpublished",
          "Name": "Notify the editors when a page is unpublished",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Activities": [
            {
              "ActivityId": "pageunpublished",
              "Name": "ContentUnpublishedEvent",
              "X": 40,
              "Y": 40,
              "IsStart": true,
              "Properties": { "ContentTypeFilter": [ "Page" ] }
            },
            {
              "ActivityId": "notifyeditors",
              "Name": "NotifyUserTask",
              "X": 380,
              "Y": 40,
              "IsStart": false,
              "Properties": {
                "UserNames": { "Expression": "ADMIN, JAMIE.CHEN" },
                "Subject": { "Expression": "A page was unpublished" },
                "Summary": { "Expression": "Readers no longer see the page." },
                "TextBody": { "Expression": "The page is no longer visible on the site." },
                "HtmlBody": { "Expression": "" },
                "IsHtmlPreferred": false,
                "Url": { "Expression": "" }
              }
            }
          ],
          "Transitions": [
            { "SourceActivityId": "pageunpublished", "SourceOutcomeName": "Done", "DestinationActivityId": "notifyeditors" }
          ]
        }
      ]
    }
  ]
}
```

The names in **User names** are written in capitals on purpose. The task looks the names up in the normalized user name,
which Identity stores in capitals, and it does not normalize what was typed
(`OrchardCore.Notifications/Activities/NotifyUserTask.cs:53-61`; `OrchardCore.Users/Liquid/UsersByNameFilter.cs:28-38` does
normalize). On a database that compares text with case, `admin, jamie.chen` finds no user and the task ends with
**Failed: no user found**, so no notification is created. Capitals work on every database. See OPEN-QUESTIONS.md.

The **URL** is empty on purpose. A notification without a URL has no link in the bell, and its subject opens the
notification's own page in the notification center (`OrchardCore.Notifications/Views/NotificationsSubject_SummaryAdmin.cshtml:12-19`),
which is the page the video shows. An **Open** button only exists for a notification that has a URL
(`OrchardCore.Notifications/Views/NotificationsActions_DetailAdmin.cshtml:8-11`), and the video does not use one.

### 1.4 Fake data used on screen

| What | Value |
|---|---|
| Sign-in | `admin`, `admin@example.com`, `<password>` |
| Second user | `jamie.chen`, `jamie.chen@example.com`, phone `+12015550124` |
| Phone number of `admin` | `+12015550123` |
| Pages | `Contact us` (draft), `About the market` (published), `Old market map` (unpublished) |
| Workflows | `Notify the editors when a page is published`, `Notify the editors when a page is unpublished` |
| User names in the workflows | `ADMIN, JAMIE.CHEN` (capitals, see section 1.3) |
| Notifications of `admin` | `A page was published` (twice, then once more in clip 03), `A page was unpublished` |
| Sender of the email | `noreply@example.com` |

## 2. Which clips depend on which, and which change the site

| Clip | Starts from | Changes the site | Take again by |
|---|---|---|---|
| `02-enable` | S0 | Enables the three Notifications features | Restoring S0, then redoing the seed of 1.3 (new snapshot S1) |
| `03-workflow` | S1 | Publishes `Contact us`: one more unread notification, `A page was published`, so the bell reads `4` | Restoring S1 |
| `04-bell` | S2 (S1 after clip 03) | Nothing intended; see the cursor warning in section 5 | Restoring S2 if the badge changed |
| `05-center` | S2 | Marks notifications as read (one by its button, two in bulk, one by viewing it) | Restoring S2 |
| `06-preferences` | S2 | Saves the profile of `admin` with **SMS Notifications** cleared | Restoring S2 |

Record in this order: `02-enable`, the seed of 1.3, snapshot S1, `03-workflow`, snapshot S2, then `04-bell`,
`05-center` and `06-preferences`, each from S2 (restore S2 before `05-center` and before `06-preferences`).
Clips `04-bell`, `05-center` and `06-preferences` do not depend on each other. Clip `05-center` and clip `04-bell`
both expect the four unread notifications of S2.

The slide chapters (`01-why`, `07-delivery`) need no site.

## 3. The clips

Common rules: park the cursor on an empty part of the page between steps (not over the bell, a notification, or the top
bar), keep the actions of a step shorter than its narration, and look at every contact sheet.

### Clip `02-enable` (chapter 02)

Starts on `/Admin`, the dashboard, with the Notifications features disabled. Nothing is expanded in the admin menu. The
narration opens the menu, so opening it is on camera.

- `enable-1`: In the admin menu click **Tools** (`OrchardCore.Features/AdminMenu.cs:26`), then **Features** (line 27), and wait
  for the heading **Features**. Click `#search-box` (placeholder **Search**, `OrchardCore.Features/Views/Admin/Features.cshtml:27`)
  and type `notifications`. Wait until the group **Notifications** shows its three rows.
  *On screen at the end:* the search box with `notifications`, and the **Notifications** group with **Notifications**,
  **Email Notifications** and **SMS Notifications**. *Source:* `OrchardCore.Notifications/Manifest.cs:10,22` and
  `OrchardCore.Sms/Manifest.cs:17` give the names and the category **Notifications**.
- `enable-2`: Highlight the three rows one after the other (about 2 s each), without a caption. The rows show
  their dependency badges: **Email Notifications** shows **Notifications** and **Email**, **SMS Notifications** shows
  **Notifications** and **SMS** (`OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml`).
  *On screen at the end:* the three rows, the last one highlighted, with their badges readable. *Source:* the
  dependencies are in `OrchardCore.Notifications/Manifest.cs:25-29` and `OrchardCore.Sms/Manifest.cs:21-25`.
- `enable-3`: Click the check boxes `input[name='featureIds'][value='OrchardCore.Notifications']`,
  `[value='OrchardCore.Notifications.Email']` and `[value='OrchardCore.Notifications.Sms']`. Click `#bulk-action-menu-button`
  (**Bulk Actions**, `OrchardCore.Features/Views/Admin/Features.cshtml:34`), then click `a[data-action='Enable']` (**Enable**, line 38), with
  `ClickAndWaitForNavigationAsync`. Wait for the message "The following features were enabled" (it lists the features,
  `OrchardCore.Features/Controllers/AdminController.cs:317`) and for the bell `.notification-navbar-container > button`.
  Highlight the bell for the last 2 s of the narration.
  *On screen at the end:* the **Features** page after the reload, the three rows with the badge **Enabled**, the message,
  and a ring around the empty bell (`fa-regular fa-bell`, no badge) in the top bar. *Source:* the bell is
  `OrchardCore.Notifications/Views/UserNotificationNavbar.cshtml:15,34`.

### Clip `03-workflow` (chapter 03)

Starts on `/Admin` from snapshot S1. The bell reads `3`, and the cursor stays away from it.

- `workflow-1`: In the admin menu click **Design** (`OrchardCore.Workflows/AdminMenu.cs:18`), then **Workflows** (line 19), and
  wait for the list. Click the link `Notify the editors when a page is published`
  (`OrchardCore.Workflows/Views/WorkflowTypeEntry.Fields.SummaryAdmin.cshtml:7`) with `ClickAndWaitForNavigationAsync`, and wait
  until `[data-cy='activity-notifyeditors']` is visible. The toolbox on the left must be expanded, not collapsed to a rail: it
  is expanded by default, and a collapsed state is kept in the local storage of the browser
  (`OrchardCore.Workflows/Assets/designer/src/App.vue:73`), so check it once before the take.
  *On screen at the end:* the designer, with the toolbox on the left and two connected activities on the canvas.
- `workflow-2`: Highlight `[data-cy='activity-pagepublished']`, hold about 3 s, clear the highlight, then highlight
  `[data-cy='activity-notifyeditors']`. The first activity shows **Content Published** and the content type **Page**
  (`OrchardCore.Contents/Views/Items/ContentPublishedEvent.Fields.Design.cshtml:4-6`); the second shows **Notify Specific Users**
  and its subject (`OrchardCore.Notifications/Views/Items/NotifyUserTask.Fields.Design.cshtml:8-10`).
  *On screen at the end:* a ring around the **Notify Specific Users** activity.
- `workflow-3`: Clear the highlight. `DoubleClickAsync` on `[data-cy='activity-notifyeditors']` (the label
  "Settings (or double-click the activity)" is in `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:153`). Wait for
  `[data-cy='activity-panel']` and its **Settings** tab (line 79). Highlight the field **User names**, then the field
  **Subject**.
  *On screen at the end:* the activity panel below the canvas, with **User names** (`ADMIN, JAMIE.CHEN`) and **Subject**
  (`A page was published`) visible. *Source:* `OrchardCore.Notifications/Views/Items/NotifyUserTask.Fields.Edit.cshtml:4,8` and
  `OrchardCore.Notifications/Views/Items/NotifyUserTaskActivity.Fields.Edit.cshtml:5`.
- `workflow-4`: Clear the highlight and scroll the panel (`[data-cy='activity-panel']`, scroll inside it, not the page) down
  by about one field every 2 s, so that the viewer sees **Summary**, **URL**, **Type**, **Text Body**, **HTML Body** and the
  check box **Use HTML Body when possible** (`OrchardCore.Notifications/Views/Items/NotifyUserTaskActivity.Fields.Edit.cshtml:14,23,32,41,50,62`), ending on the
  hints "You may use Liquid syntax." Do not click into a field or change a value.
  *On screen at the end:* **Text Body** and **HTML Body** in the panel, with **Text Body** showing
  `The page is live on the site.`
- `workflow-5`: Scroll the panel back up if needed. Click `[data-cy='toolbox-search']` (placeholder **Search activities**,
  `OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:46`) and type `notifications`. The toolbox then lists, in the category **Notifications**,
  the cards **Notify Specific Users** and **Notify Content Owner**
  (`OrchardCore.Notifications/Views/Items/NotifyUserTask.Fields.Thumbnail.cshtml:1`, `OrchardCore.Notifications/Views/Items/NotifyContentOwnerTask.Fields.Thumbnail.cshtml:1`). Highlight the first card,
  then the second. Do not click a card (it would add an activity).
  *On screen at the end:* the toolbox with the two cards, the second one highlighted.
- `workflow-6`: In the admin menu click **Content** (`OrchardCore.Contents/AdminMenu.cs:65`), then **Content Items**
  (line 68) with `ClickAndWaitForNavigationAsync`. If the browser asks to leave the designer, there is a draft: see
  OPEN-QUESTIONS.md. Click the search box (placeholder **Search**, `OrchardCore.Contents/Views/ContentsAdminListSearch.cshtml:28`), type `Contact us` and press
  Enter (`WaitForNavigationAsync`). On the row of `Contact us` click **Actions**, then **Publish Draft**
  (`OrchardCore.Contents/Views/ContentsButtonActions_SummaryAdmin.cshtml:19`), with `WaitForNavigationAsync`. The page publishes at once,
  without a confirmation, and the workflow sends the notifications during the request.
  *On screen at the end:* the **Content Items** list after the redirect, with `Contact us` published and the bell badge
  reading `4`. If the badge still reads `3`, reload the page before the narration ends and check OPEN-QUESTIONS.md.

### Clip `04-bell` (chapter 04)

Starts on `/Admin` from snapshot S2, the bell reading `4`, the cursor parked on the page title.

- `bell-1`: Move the cursor to the bell `.notification-navbar-container > button`
  (`aria-label` **Notifications**, `OrchardCore.Notifications/Views/UserNotificationNavbar.cshtml:15`) and highlight it. The badge is
  `#UnreadNotificationBadge` (line 30). Do not click.
  *On screen at the end:* a ring around the bell, with the badge `4`.
- `bell-2`: Clear the highlight and click the bell. The menu `.notification-navbar` opens with four items, newest first:
  `A page was published` (the one published in clip 03), `A page was unpublished`, `A page was published`,
  `A page was published`, each with its summary and a relative time. Hold the cursor on the bell.
  *On screen at the end:* the open menu with the four notifications and the line **Notification Center** at its bottom.
  *Source:* `OrchardCore.Notifications/Views/NotificationsSubject_Header.cshtml`, `OrchardCore.Notifications/Views/NotificationsMeta_Header.cshtml`.
- `bell-3`: Move to the link **Notification Center** (`OrchardCore.Notifications/Views/UserNotificationNavbar.cshtml:65`) along the right edge of the menu, as
  described in section 5, and click it with `ClickAndWaitForNavigationAsync`.
  *On screen at the end:* `/Admin/notifications`, titled **Notification Center** (`OrchardCore.Notifications/Views/Admin/List.cshtml:4`).

### Clip `05-center` (chapter 05)

Starts on `/Admin/notifications` from snapshot S2, with the four notifications unread and the cursor parked on the page
title. Open it before the capture so the rows are drawn.

- `center-1`: No action; hold on the list for the whole narration. The unread rows are highlighted in yellow and their
  subjects are bold (`OrchardCore.Notifications/Views/NotificationsAdminList.cshtml:61-66`, `OrchardCore.Notifications/Views/NotificationsSubject_SummaryAdmin.cshtml:23`). The order is the
  most recent first (`OrchardCore/OrchardCore.Notifications.Core/DefaultNotificationsAdminListFilterProvider.cs`, the `sort` term).
  *On screen at the end:* the four rows, all highlighted, with the item count above the list
  (`OrchardCore.Notifications/Views/NotificationsAdminListSummary.cshtml:9`).
- `center-2`: Highlight the subject and the summary of the first row, then the time badge of that row. Then click **Mark as read**
  (`OrchardCore.Notifications/Views/NotificationsActions_SummaryAdmin.cshtml:6`) on the **last** row, the oldest, with `WaitForNavigationAsync`; the page
  reloads and that row is no longer highlighted.
  *On screen at the end:* three highlighted rows and one plain row at the bottom.
- `center-3`: Click the search box (placeholder **Search**, `OrchardCore.Notifications/Views/NotificationsAdminListSearch.cshtml:8`), type `share` and press Enter
  (`WaitForNavigationAsync`): the summary of `A page was published` holds the word, the summary of `A page was unpublished` does
  not (the search covers the summary and the text body, `OrchardCore/OrchardCore.Notifications.Core/Indexes/NotificationIndex.cs:46-52`). Then click the **Show** drop-down: it is a
  bootstrap-select control, so click its rendered button (`.bootstrap-select button[title='Show']`, `OrchardCore.Notifications/Views/NotificationsAdminListFilters.cshtml:10`),
  then the option **Unread** (the options are **All**, **Read**, **Unread**: line 11 and `OrchardCore.Notifications/Controllers/AdminController.cs:101-102`).
  The page reloads. Move to the **Sort** button (`button[title='Sort']`, line 13) and highlight it without opening it.
  *On screen at the end:* the list filtered to the unread published notifications, with the search text and the **Show** and
  **Sort** buttons visible.
- `center-4`: On the first row click its **Actions** button (`button.dropdown-toggle.actions` inside the row,
  `OrchardCore.Admin/Views/AdminListActions.cshtml`), then **View**
  (`OrchardCore.Notifications/Views/NotificationsButtonActions_SummaryAdmin.cshtml:2`) with `ClickAndWaitForNavigationAsync`. The page of the notification opens and
  marks it as read. Highlight the buttons **Mark as unread** and **Delete** in turn, without clicking them
  (`OrchardCore.Notifications/Views/NotificationsActions_DetailAdmin.cshtml:13,15`).
  *On screen at the end:* the page titled with the subject, a card with the summary and the text body
  `The page is live on the site.`, and the buttons **Mark as unread** and **Delete** at the bottom of the card.
- `center-5`: Click the breadcrumb link **Notification Center** (`OrchardCore.Notifications/Views/Admin/Display.cshtml`) with
  `ClickAndWaitForNavigationAsync`: the list is back without a filter. Click the check boxes `input[name='itemIds']` of two
  unread rows. The **Actions** button `#bulk-action-menu-button` appears (it needs two rows selected,
  `OrchardCore.Notifications/wwwroot/Scripts/notifications-admin-list/notifications-admin-list.js`). Click it, then
  `a.dropdown-item[data-action='Read']` (**Mark as read**, `OrchardCore.Notifications/Controllers/AdminController.cs:115`); the confirmation `#confirmRemoveModal`
  appears: click `#modalOkButton` (**Ok**: the dialog is built in `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:14-30`, its default texts are in `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`).
  *On screen at the end:* the list after the reload, with the message "2 notifications read successfully."
  (`OrchardCore.Notifications/Controllers/AdminController.cs:214`) and the two rows no longer highlighted. The page
  of the previous step must be left within the first 2 s of this step, so that the whole list is on screen while the narration
  says "select their check boxes".
- `center-6`: Highlight the button **Mark All as Read** (`a.btn`, `OrchardCore.Notifications/Views/NotificationsAdminListActionBarButtons.cshtml:8`), next to the
  search box. Do **not** click it: its confirmation window is titled "Delete" (see OPEN-QUESTIONS.md).
  *On screen at the end:* a ring around **Mark All as Read**.

### Clip `06-preferences` (chapter 06)

Starts on `/Admin` from snapshot S2 (the bell reads `4`), cursor parked on the page title.

- `pref-1`: Click the user menu button `#navbarDropdown` (`OrchardCore.Users/Views/UserMenu.DetailAdmin.cshtml:7`; it shows a person
  icon, with the user name as its tooltip), then the item **Profile** (`OrchardCore.Users/Views/UserMenuItems-Profile.cshtml:9`) with
  `ClickAndWaitForNavigationAsync`. The page is titled **Edit User** (`OrchardCore.Users/Views/Admin/Edit.cshtml:4`).
  *On screen at the end:* the **Edit User** page of `admin`, showing **User name**, **Email**, **Phone Number**.
- `pref-2`: Scroll the page down until the label **Notification Methods** is visible
  (`OrchardCore.Notifications/Views/UserNotificationPreferencesPart.Edit.cshtml:7`) and highlight the list
  `ul[id$='Methods']`. It holds **Email Notifications** and **SMS Notifications**, both checked, with the hint "Select all
  notification methods to receive." (line 11).
  *On screen at the end:* the list with two checked methods. There is no **Notification Types** table: no module registers a
  notification type (line 36 shows it only when one does).
- `pref-3`: Click the check box `ul[id$='Methods'] input[type='checkbox'][value='SMS']` to clear **SMS Notifications**, then scroll to the bottom and click
  **Save** (`button.save`, `OrchardCore.Users/Views/UserSaveButtons.Edit.cshtml:2`) with `WaitForNavigationAsync`. The page reloads on
  **Edit User** with the message "User updated successfully." (`OrchardCore.Users/Controllers/AdminController.cs:406`). Scroll back to
  **Notification Methods**.
  *On screen at the end:* **Notification Methods** with **Email Notifications** checked and **SMS Notifications** cleared.
- `pref-4`: Scroll to the top. Highlight the field **Email** (`input[type='email']`, `OrchardCore.Users/Views/UserEmail.Edit.cshtml:4`,
  value `admin@example.com`), then the field **Phone Number** (`input[type='tel']`, `OrchardCore.Users/Views/UserPhoneNumber.Edit.cshtml:4`, value `+12015550123`).
  *On screen at the end:* the two fields with their values, the second one highlighted.
  *Source of the narration:* `OrchardCore/OrchardCore.Notifications.Core/Services/EmailNotificationProvider.cs:38` ("No email address
  provided.") and `OrchardCore/OrchardCore.Sms.Core/Services/SmsNotificationProvider.cs:38` ("No phone number provided.").

## 4. The slides

Drawn by `slides.py`, with the brand helpers only. Each slide shows the same heading and cards, with the cards of the step bright.

- `why-1`: heading "How notifications reach users"; the card **Notification center** is bright.
- `why-2`: the same slide; the card **Notification methods** is bright.
- `why-3`: the same slide; the card **Senders** is bright.
- `deliver-1`: heading "What each method sends and needs"; the card **Email Notifications** is bright.
- `deliver-2`: the same slide; the card **SMS Notifications** is bright.
- `deliver-3`: the same slide; the cards **Notification center** and **Workflow outcome** are bright.

## 5. Cursor warning: hovering marks notifications as read

`OrchardCore.Notifications/Assets/js/notification-manager.js:44` marks a notification as read on `click` **and on `mouseover`** of its
element (class `mark-notification-as-read`) when the pointer is directly on that element. The root of a bell item is such an
element, and so is the block of a row in the notification center. Resting the cursor on the padding of an unread bell item
marks it as read and lowers the badge. Therefore:

- In `bell-2` and `bell-3`, keep the cursor on the bell, and move to **Notification Center** in two moves: first to the right edge of
  the menu (`MoveToAsync` with an x offset of half the menu width, outside the text of the items), then straight down to the link.
- In `center-1` to `center-3`, keep the cursor away from the rows, except for the clicks the step asks for.
- After `04-bell`, check the badge: it must still read `4` on the next page. If it does not, restore S2 and record the clip again.

## 6. Checks for private data

- OCR the video at one frame per second and at every scene change. The text must only hold the fake data of section 1.4.
- The video may show: `admin`, `admin@example.com`, `jamie.chen`, `jamie.chen@example.com`, `noreply@example.com`, the two phone numbers
  of section 1.4, the capitals `ADMIN, JAMIE.CHEN` of the **User names** field, and the pages and workflows listed there. Nothing else.
- Never type or show the password. The password field of the user form is never on screen in this video.
- The designer must not show an instance link or a token (this video's workflows have no HTTP Request event).
