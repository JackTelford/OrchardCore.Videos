# Open questions: Notifications

The narration states only what the source of Orchard Core (commit `92db4a83`) shows. The points below were read from the
source but never run on a site. A person must confirm each one on the recording site before the take; what to do if the
site disagrees is given with each point. Paths are relative to `src/`, with `OrchardCore.Modules/` left out of module
paths, as in recording-plan.md.

## About the seed

1. **Do the imported workflows run without a Publish in the designer?** Checked: the `WorkflowType` recipe step saves the
   workflow type itself (`OrchardCore.Workflows/Recipes/WorkflowTypeStep.cs:89`), as the sample recipe of the module does.
   Not checked: that a content event finds a workflow imported this way. Confirm: after the import of recording-plan.md
   section 1.3, publish the first page and see the bell reach `1`. If it does not, open each workflow in the designer, select
   **Publish** (`OrchardCore.Workflows/Services/WorkflowsDesignerJSLocalizer.cs:190`), and run the seed again.
2. **Are the property names of the recipe right?** Checked against the activity classes: `ContentTypeFilter`
   (`OrchardCore.Contents/Workflows/Activities/ContentEvent.cs:23`), and `UserNames`, `Subject`, `Summary`, `TextBody`, `HtmlBody`,
   `IsHtmlPreferred`, `Url` (`OrchardCore.Notifications/Activities/NotifyUserTask.cs:41`, `NotifyUserTaskActivity.cs:48-95`) as
   expression objects. Not run. Confirm in the designer that the **Notify Specific Users** settings show the seeded values.
3. **Phone numbers.** `+12015550123` and `+12015550124` are in the range that is reserved for fiction (555-0100 to 555-0199),
   with a United States area code, so they belong to nobody. They must pass the phone number validator of the Users module
   (`OrchardCore/OrchardCore.Infrastructure/PhoneFormatValidator.cs`, called from `OrchardCore.Users/Drivers/UserInformationDisplayDriver.cs:103,130`).
   Not run. If a form says "Please provide a valid phone number.", use another number of the same range that passes.
4. **SMTP pickup directory.** Checked: the label **Specified pickup directory** and that the default location is used when the
   box is left empty (`OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:74-76,186-188`). Not checked: the folder
   the default resolves to, and that each notification to `admin@example.com` and `jamie.chen@example.com` produces a message
   there. Not on camera, so only the absence of errors matters; check the log of the site after the seed.
5. **The Log SMS provider.** Checked: registered only in the Development environment
   (`OrchardCore.Sms/Startup.cs:31-34`) and listed in **Default SMS provider** by its key, `Log`
   (`OrchardCore.Sms/Drivers/SmsSettingsDisplayDriver.cs:42-48`). Confirm the value `Log` is in the list.

## About the clips

6. **Features search (`enable-1`).** The group **Notifications** has the three features (their manifests give the category). The search
   box matches the name, the id and the description of a row, as typed (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`,
   `OrchardCore.Features/Assets/ts/features-index.ts`), and no other manifest of this commit mentions notifications, so only the three rows
   should remain. Not run. If other rows remain, type `notifications` anyway and scroll so that the group **Notifications** is on screen.
7. **The designer.**
   - Node text: the header of a node shows the display text (**Content Published Event**, **Notify Specific Users Task**) and its
     body the title (**Content Published**, **Notify Specific Users**), from `canvas/ActivityNode.vue` and the `Fields.Design`
     views. The narration says "Content Published event" and "Notify Specific Users task", which fits both. Confirm on screen.
   - Opening the settings of an activity and scrolling its panel should create no draft ("Changes are saved as you make them",
     `WorkflowsDesignerJSLocalizer.cs:59`). Not run. After `workflow-5`, check that the designer shows no draft banner, and that
     leaving it in `workflow-6` asks nothing. If a draft exists, discard it before the take; do not publish a changed workflow.
   - The order of the fields in the panel (**User names** first, then **Subject**, **Summary**, **URL**, **Type**, **Text Body**,
     **HTML Body**) follows the order the two drivers add their shapes in; confirm it.
   - The toolbox search `notifications` should list only the two cards of the category **Notifications**. The core task **Notify**
     (category UI, `OrchardCore.Workflows/Activities/NotifyTask.cs:36`) must not match; confirm, since it has a similar name.
8. **The bell after the publish (`workflow-6`).** The navbar shape is cached per user (the sliding expiration defaults to 1800 s,
   `OrchardCore/OrchardCore.Notifications.Core/Models/NotificationOptions.cs:32`) and the entry is removed when a notification is
   sent (`OrchardCore/OrchardCore.Notifications.Core/Services/CacheNotificationEventsHandler.cs`). Confirm that the page loaded
   after the redirect shows `4`, not `3`.
9. **Marking as read by hovering.** `OrchardCore.Notifications/Assets/js/notification-manager.js:44` binds `click` and `mouseover`
   on every element of class `mark-notification-as-read`, acting only when the event target is that element itself. The
   bell items and the rows are such elements, so a pointer resting on their padding may mark them as read. Section 5 of
   recording-plan.md gives the cursor routes; confirm on the first take that the badge stays at `4` through `04-bell`.
10. **The drop-downs Show and Sort** are bootstrap-select controls
    (`OrchardCore.Notifications/Views/NotificationsAdminListFilters.cshtml:10,13`). Not run: the rendered button's `title`, the
    texts of the options, and the automatic submit on change (the compiled script submits the filter form on
    `changed.bs.select`, `OrchardCore.Notifications/wwwroot/Scripts/notifications-admin-list/notifications-admin-list.js`). Adjust the
    selectors of `center-3` if the markup differs.
11. **Bulk actions.** The compiled script shows the **Actions** button only when more than one row is selected, and asks for a
    confirmation whose title is **Bulk Action**, with the message "Are you sure you want to mark as read these items?" and the
    button **Ok** (`NotificationsAdminListBulkActions.cshtml:5`, `OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99`). Confirm the
    texts on screen; the narration names only the actions **Mark as read**, **Mark as unread** and **Remove**.
12. **Mark All as Read** opens a confirmation titled "Delete", with the message "Are you sure you to mark all notifications as read?"
    (`NotificationsAdminListActionBarButtons.cshtml:6-8`). It is a quirk of the code, not of the documentation; the clip does not
    click the button, so that the window is never on camera.
13. **The profile page.** Checked: **Profile** opens the editor of the current user (`OrchardCore.Users/Controllers/AdminController.cs:325-354`),
    the preferences are a section of that editor (`OrchardCore.Notifications/Drivers/UserNotificationPreferencesPartDisplayDriver.cs:71`),
    and **Save** shows "User updated successfully." (`OrchardCore.Users/Controllers/AdminController.cs:406`). Not run: that the
    section appears below the **Phone Number** field without a tab, that saving raises no validation error for the seeded
    values, and that the reloaded page keeps **SMS Notifications** cleared.
14. **Times.** Rows and bell items show a relative time through the `Timespan` shape
    (`NotificationsMeta_Header.cshtml`, `NotificationsMeta_SummaryAdmin.cshtml`). Confirm the format reads sensibly a few minutes
    after the seed.

## About what the narration says

15. **Order of the methods.** The preferences page lets a user drag the methods to sort them (hint text in
    `OrchardCore.Notifications/Views/UserNotificationPreferencesPart.Edit.cshtml:31`), but the sorting script throws on every drag end
    (`OrchardCore.Notifications/Assets/ts/user-notification-preferences-part.ts:19-23,60`, a comment in the source says so) and the service sends through
    **every** method the user keeps, in that order (`OrchardCore/OrchardCore.Notifications.Core/Services/NotificationService.cs:56`,
    the loop over the providers). The video neither narrates nor records the ordering.
16. **Types of notification.** No module of this commit registers a notification type, so the **Type** filter of the center, the type
    badges and the table **Notification Types** of the preferences never appear on a site made for this video
    (`OrchardCore.Notifications/Views/UserNotificationPreferencesPart.Edit.cshtml:36`). The narration only says that **URL** and **Type**
    are optional fields of the task.
17. **Workflow outcomes (slide `deliver-3`).** The task ends with Done when at least one send succeeded (counted over all its users), and
    with Failed otherwise, also when the user has turned every method off or no method feature is enabled, although the notification is
    stored (`OrchardCore.Notifications/Activities/NotifyUserTaskActivity.cs:121-141`). A third outcome, "Failed: no user found", is returned
    when the task finds no user to notify (line 103) and is named in the narration. None of the outcomes is on screen in the video.
18. **Who can see the bell.** The bell needs the permission **Manage notifications**
    (`OrchardCore/OrchardCore.Notifications.Core/NotificationPermissions.cs:8`,
    `OrchardCore.Notifications/Drivers/NotificationNavbarDisplayDriver.cs:46`). The video says only that users need it; the roles that
    receive it by default (`OrchardCore.Notifications/NotificationPermissionsProvider.cs`) are not named, because the role updater only
    applies them to roles that exist when the feature is installed.

## About the documentation page

19. **The Notifications page already embeds a video.** Line 7 of `docs/reference/modules/Notifications/README.md` introduces an
    existing iframe and says it shows "the notification bell, the notification center, the notification preferences, a workflow
    that sends notifications, and how a module uses the notification center as its inbox". The last part is developer material
    that this admin video does not cover (a separate video for developers does). A maintainer must decide whether this video
    replaces that iframe and its sentence, or gets its own marker beside them; the page is not changed here.

## Added by the review of the source

20. **User names and capitals (seed, `workflow-3`).** The task `NotifyUserTask` queries `UserIndex.NormalizedUserName` with the names exactly
    as they were typed (`OrchardCore.Notifications/Activities/NotifyUserTask.cs:53-61`), while the stored value is the normalized name, in capitals
    (the default Identity normalizer). The Liquid filter of the Users module normalizes first (`OrchardCore.Users/Liquid/UsersByNameFilter.cs:28-38`).
    Whether `admin` matches `ADMIN` depends on how the database compares text: a case-sensitive comparison would not match. Not run.
    The plan provisions the site with the Sqlite provider, and SQLite compares text with case unless a collation is set (the columns that
    YesSql creates were not checked here). The seed therefore writes `ADMIN, JAMIE.CHEN`, which matches on every
    database. Confirm after the first publish that the bell reaches `1`. If lowercase names are found to fail on some database, that is a
    point to report to the maintainers.
21. **Capitals on screen.** The field **User names** shows `ADMIN, JAMIE.CHEN` in `workflow-3`. The narration only says the field takes a
    comma-separated list of the users to notify, which is true for any case. Confirm the capitals read acceptably on screen.
22. **SMS provider fallback.** `deliver-2` tells the viewer to choose the **Default SMS provider**. When none is chosen, the SMS service falls
    back to the last registered provider (`OrchardCore/OrchardCore.Sms.Core/Services/DefaultSmsProviderResolver.cs:43-46`), which would be Twilio
    on a stock site if the registration order holds; the narration does not claim that a choice is required, only that the viewer should make it.
23. **Which modules send notifications.** The narration says that a module can send notifications, which the public service allows
    (`INotificationService`, documented on the Notifications page). In this commit no module outside the Notifications module itself calls it
    (searched in `src`): the only senders that ship are the two workflow tasks. The video does not name a module that sends them.
