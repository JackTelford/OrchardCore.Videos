# Recording plan: Email: Configure SMTP and Send a Test Email

Everything a person needs to record this video without deciding anything. Paths such as `src/OrchardCore.Modules/...`
are in the `OrchardCMS/OrchardCore` source (commit `92db4a83`). A selector written below stands for
`scene.Page.Locator(...)`. The narration is in `storyboard.json`; narrate it first (`scripts/narrate.py`), because the
recorder holds each step for the length of its narration.

The video has six chapters between its cards: two are slide chapters (`01-concepts`, `06-fail`, nothing to record) and
four are recordings (`02-enable`, `03-configure`, `04-test`, `05-inbox`).

## The site it needs

A fresh site, provisioned by Auto Setup, with the **Blog** recipe, and a local mail server that catches the messages.

| Setting | Value |
|---|---|
| Recipe | `Blog` |
| Tenant | `Default` |
| Site name | `Orchard Core` |
| Time zone | `America/Los_Angeles` |
| Admin user | `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Database | `Sqlite`, in its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`) |
| Address | `http://localhost:5010` (`VIDEO_BASE_URL`) |
| Mail server | smtp4dev in Docker: SMTP on `localhost:2525`, web page on `http://localhost:3000` |

Auto Setup is given through environment variables, so no setup or sign-in form is recorded
(`OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`,
`...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`,
`...__AdminPassword=$VIDEO_PASSWORD`, `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog`). See `references/recording.md`
of the skill. `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address stop the site's web calls, but they do not apply to
SMTP, which is not HTTP: the only mail server the site can reach is the one the clips configure (`localhost`, port
`2525`).

**The mail server.** Start it before the first clip, outside the capture, and keep its web page open in a browser of
your own to check that it is empty:

```bash
docker run -d --name smtp4dev-video -p 3000:80 -p 2525:25 rnwood/smtp4dev
```

smtp4dev must accept a plain connection without a sign-in on port 25 inside the container (no TLS, no authentication
required): the narration says that the test server accepts plain connections and does not ask for a sign-in. Its inbox
must be empty before clip `04-test`. To empty it, remove the container and start it again:
`docker rm -f smtp4dev-video`, then the `docker run` line above. When all the takes are done, remove the container
(`docker rm -f smtp4dev-video`). The image name, the ports and these two behaviors come from smtp4dev, not from the
Orchard Core source: they are listed in `OPEN-QUESTIONS.md`.

**Features.** Enable nothing before recording. The Blog recipe does not enable Email or SMTP Email Provider, and none of
the features that pull Email in as a dependency (Users Registration, Users Reset Password, Two-Factor Email Method,
Email Notifications, Azure Communication Services Email) is in its list
(`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`; the dependencies are in the `Manifest.cs` files of
`OrchardCore.Users`, `OrchardCore.Notifications` and `OrchardCore.Email.Azure`). Before the first clip, open
`/Admin/Features`, search `email`, and check that **Email** and **SMTP Email Provider** show **Disabled**. Clip
`02-enable` enables them on camera.

**Settings.** Nothing to set up. Leave the **Settings, Admin** page at its defaults. The clip `03-configure` saves the
Email and SMTP settings on camera.

**Seed data.** None to create. The values typed on camera are all fake: `noreply@example.com`,
`jamie.chen@example.com`, the subject `Test message`, the body `Hello from Orchard Core.`, host `localhost`, port `2525`.
No password is typed in any clip.

**Browser.** Chromium at 1536x864, light color scheme, `en-US`, the admin menu expanded with every section closed.
Sign in once outside the capture (`Recorder setup`, with `VIDEO_USER_NAME=admin` and `VIDEO_PASSWORD`), which saves
`state.json` (git-ignored). No `PrepareAsync` is needed: the clips do all the setup on camera.

## Order of the clips, and what changes the site

Record in this order. Take **snapshot A** (a copy of `App_Data` and of `state.json`) after `Recorder setup`, with the
site stopped.

| Order | Clip | Starts on | Changes the site? | Depends on |
|---|---|---|---|---|
| 1 | `02-enable` | `/Admin` | **Yes**: enables Email and SMTP Email Provider | Snapshot A |
| 2 | `03-configure` | `/Admin` | **Yes**: saves the Email and SMTP settings | Snapshot B (the features enabled, no settings saved) |
| 3 | `04-test` | `/Admin` | No (sends one message to the mail server) | Snapshot C (the settings saved), an empty inbox |
| 4 | `05-inbox` | `/Admin/Email/Test` | No | The message `04-test` sent, still in the inbox |

- After clip 1, stop the site and copy `App_Data` and `state.json` aside (**snapshot B**). After clip 2, do the same
  (**snapshot C**). Start the site again each time.
- To record the whole video again: stop the site, restore snapshot A, start it, empty the mail server, and record the
  clips 1 to 4 in order.
- To record `03-configure` again: restore snapshot B. It needs SMTP **not enabled**, so that the Providers tab shows the
  warning, and the SMTP tab shows its check box unchecked.
- To record `04-test` again: restore snapshot C, and empty the mail server first, so the inbox holds one message. Record
  `05-inbox` straight afterwards, before anything else sends mail.
- Clip `02-enable` cannot be recorded again without restoring snapshot A (the features would be enabled already).

Before every clip, check that the admin menu is expanded and that no section of it is open.

## Slide steps (nothing to record)

- `intro`: the opening card, the logo over "Email: Configure SMTP and Send a Test Email" and "Connect your site to a
  mail server and check that it works".
- `why-1`: slide "Where a site sends email", three cards: User accounts, Notifications, Workflows.
- `parts-1`: slide "A feature and its providers", three cards with check marks: Email, SMTP Email Provider, Azure
  Communication Services Email.
- `parts-2`: slide "Who can use it", two cards with check marks: Manage Email Settings, Administrator role.
- `plan-1`: slide "What we will do", four numbered cards: Enable the provider, Configure SMTP, Send a test email, Check
  the message.
- `fail-1`: slide "When the test fails", three numbered cards: Read the error, Check the server, Check the addresses.
- `outro`: the closing card, "Email: Configure SMTP and Send a Test Email" and "docs.orchardcore.net".

## Menu and field selectors used by the clips

In every clip, a menu entry is `#left-nav .item-label:has(span.title:text-is("<label>"))`
(`src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml:69`, `NavigationItemText.cshtml`). Sections have
no page of their own, so clicking their label opens or closes them without leaving the page. Use `.First` if two entries
share a label.

The settings editors prefix their field names with the model and the section
(`src/OrchardCore/OrchardCore.DisplayManagement/Handlers/DisplayDriver.cs:13`,
`.../Entities/SectionDisplayDriverBase.cs:87`), so a field of the SMTP tab is `[name="ISite.SmtpSettings.<Property>"]`
and the field of the Providers tab is `[name="ISite.EmailSettings.DefaultProvider"]`. Use the exact name: a suffix such
as `Host` also matches `ProxyHost`, and `Port` also matches `ProxyPort`. The tabs are `ul.nav-tabs a.nav-link` with the
tab name as their text (`src/OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:30-59`). The Email Test
form is not prefixed: `#Provider`, `#To`, `#From` (the field labeled Sender), `#Cc`, `#Bcc`, `#ReplyTo`, `#Subject`,
`#Body`. Confirm each name once on the page (`Recorder probe`) before the first take.

A success message is a toast that hides itself after 5 seconds
(`src/OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`,
`src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml`). A step that must show a toast clicks early and keeps its
other actions short.

## Clip `02-enable`

Starts on `/Admin` (opened before the capture), the menu expanded, every section closed. Features Email and SMTP Email
Provider are disabled.

- `enable-1`: click the menu entry Tools, wait until its entries show, then `ClickAndWaitForNavigationAsync` on the entry
  Features. Wait until `#search-box` is shown. `TypeAsync(#search-box, "smtp")` (it is typed in lower case; the narration
  says SMTP). Wait until the list shows one row. *Visible at the end:* the Features page, `smtp` in the Search box, the
  heading "Communication", and the row "SMTP Email Provider" with an Enable button. *Labels:*
  `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27` (Tools, Features);
  `src/OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:5` (title Features), `:27` (the Search box),
  `:88` (the category heading); `OrchardCore.Email.Smtp/Manifest.cs:4,13` (name SMTP Email Provider, category
  Communication).
- `enable-2`: highlight the row `:is(li, tr):has(span.fw-semibold:text-is("SMTP Email Provider"))` for about 2 seconds,
  clear it, then move to and highlight the dependency badge in that row,
  `:is(li, tr):has(span.fw-semibold:text-is("SMTP Email Provider")) .badge:text-is("Email")`, and leave the ring on it.
  Click nothing. *Visible at the end:* the ring on the badge Email in the row of SMTP Email Provider.
  *Labels:* `.../Views/FeatureEntry.Fields.SummaryAdmin.cshtml:11` (the name), `.../Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:13-16`
  (the dependency badges; in the Grid layout the same names are in the Dependencies column,
  `Features.cshtml:96`). The dependency is declared in `OrchardCore.Email.Smtp/Manifest.cs:9-12`.
- `enable-3`: clear the ring, then `ClickAndWaitForNavigationAsync` on `a[href*="OrchardCore.Email.Smtp/Enable"]` (the
  Enable button of that row; if the admin lists show their actions in a menu, open the row's menu first and click Enable
  in it). The link posts through the page script and returns to `/Admin/Features`. Wait for the page to load. *Visible
  at the end:* the Features page after the reload, with the green toast "The feature SMTP Email Provider was enabled."
  at the top right (it hides 5 seconds after the page loads: this step is short so the toast is still shown; check the
  contact sheet). *Labels:* `.../Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40` (Enable);
  `.../Controllers/AdminController.cs:317` (the message, here with the one feature);
  `.../Controllers/AdminController.cs:247` (the Enable button passes `force` as true, so the dependencies of the feature are enabled with it, `src/OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs`, `GetFeaturesToEnable`).
- `enable-4`: `TypeAsync(#search-box, "email")` (the box is empty after the reload). Wait until the list is filtered.
  Then highlight, one after the other for about 1.5 seconds each, the rows of **Email** and **SMTP Email Provider**,
  clearing each ring before the next. *Visible at the end:* the Features page filtered on `email`, the rows Email and
  SMTP Email Provider each with the badge "Enabled" (other features whose name holds "email" are listed too, and they
  show "Disabled"). *Labels:* `.../Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9` (Enabled);
  `src/OrchardCore.Modules/OrchardCore.Email/Manifest.cs:4` (name Email).

After this clip, stop the site and take snapshot B.

## Clip `03-configure`

Starts on `/Admin` (opened before the capture), the menu expanded, every section closed. The Email and SMTP Email
Provider features are enabled, and no email setting is saved.

- `config-1`: click the menu entry Settings, then the entry Communication, then `ClickAndWaitForNavigationAsync` on the
  entry Email. Wait until the tabs are shown (`ul.nav-tabs`). Highlight the warning `.alert.alert-warning` for about 2
  seconds, and clear it. *Visible at the end:* the Email settings page (`/Admin/Settings/email`), the tabs Providers
  (active) and SMTP, the warning "No email providers are currently enabled." and the Save button. *Labels:*
  `src/OrchardCore.Modules/OrchardCore.Email/AdminMenu.cs:38-40` (Settings, Communication, Email);
  `.../Drivers/EmailSettingsDisplayDriver.cs:59` (the tab Providers, first);
  `.../Views/EmailSettings.Edit.cshtml:11` (the warning, shown when no provider is enabled);
  `src/OrchardCore.Modules/OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:88` (the tab SMTP);
  `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2` (Save).
- `config-2`: click the tab `ul.nav-tabs a.nav-link:text-is("SMTP")`. Click the check box
  `input[type="checkbox"][name="ISite.SmtpSettings.IsEnabled"]`. Wait 0.6 seconds for the fields to open. *Visible at the
  end:* the SMTP tab, "Enable SMTP Provider" checked, and below it Sender email address, Delivery method and the
  Network delivery options. *Labels:* `OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml:10` (Enable SMTP
  Provider).
- `config-3`: `TypeAsync([name="ISite.SmtpSettings.DefaultSender"], "noreply@example.com")`. *Visible at the end:* the
  field Sender email address holding `noreply@example.com`, with its hint "The default email address to use as a
  sender, unless the email sender is set." *Labels:* `SmtpSettings.Edit.cshtml:17` (Sender email address; the label is
  marked required), `:21` (the hint).
- `config-4`: highlight the select `[name="ISite.SmtpSettings.DeliveryMethod"]` and leave the ring on it. **Do not
  select anything in it**: choosing Specified pickup directory shows a hint with template tokens that an admin video
  does not show. *Visible at the end:* the ring on Delivery method, with the value Network, and the hint "The delivery
  method used when sending email. Use Network in production. The other options can be useful when developing and
  testing." *Labels:* `SmtpSettings.Edit.cshtml:26` (Delivery method), `:30` (Network), `:33` (Specified pickup
  directory), `:37` (the hint).
- `config-5`: clear the ring. `FillAsync("")` on `[name="ISite.SmtpSettings.Host"]`, then
  `TypeAsync([name="ISite.SmtpSettings.Host"], "localhost")`. Then `FillAsync("")` on
  `[name="ISite.SmtpSettings.Port"]` (it holds 25) and `TypeAsync([name="ISite.SmtpSettings.Port"], "2525")`. Press Tab so
  the value is applied. *Visible at the end:* under "Network delivery options", Host name `localhost` and Port number
  `2525`; the hints under them read "The SMTP server domain, e.g." followed by an example server name in italics, and "The SMTP
  server port, usually 25." (product text, not typed by anyone). *Labels:* `SmtpSettings.Edit.cshtml:45` (Network delivery options),
  `:50` (Host name), `:54` (its hint), `:59` (Port number), `:63` (its hint).
- `config-6`: `SelectAsync([name="ISite.SmtpSettings.EncryptionMethod"], "None")`, then highlight the check box
  `input[type="checkbox"][name="ISite.SmtpSettings.AutoSelectEncryption"]` (unchecked; its label is Auto Select
  Encryption method). Leave the ring on it. On a fresh site the check box is unchecked and the select shows None
  already (`src/OrchardCore/OrchardCore.Email.Abstractions/SmtpSettings.cs:21,36`): if the page differs, make it so
  (uncheck Auto Select Encryption method first, because it disables the select), and write the difference in
  `OPEN-QUESTIONS.md`. *Visible at the end:* "Encryption method to use" showing "None - Connect to server using
  insecure connection.", and a ring on the unchecked "Auto Select Encryption method". *Labels:*
  `SmtpSettings.Edit.cshtml:90` (Auto Select Encryption method), `:91` (hint), `:97` (Encryption method to use), `:101`,
  `:104`, `:107` (the three options None, SSL/TLS, STARTTLS).
- `config-7`: clear the ring. Click the check box `input[type="checkbox"][name="ISite.SmtpSettings.RequireCredentials"]`;
  wait 0.6 seconds; the entries Use Windows authentication, User name and Password open. Hold about 5 seconds, moving
  the cursor over the three labels without clicking them or typing in them. Then click the check box again to uncheck
  it, and wait 0.6 seconds. Keep this to about 10 seconds in all. **Never type a user name or a password.** *Visible at
  the end:* "Require credentials" unchecked and the credential fields closed. *Labels:* `SmtpSettings.Edit.cshtml:120`
  (Require credentials), `:131` (Use Windows authentication), `:139` (User name), `:148` (Password).
- `config-8`: `ClickAndWaitForNavigationAsync` on `button.primaryAction.save`. Wait for the page to load: it returns to
  `/Admin/Settings/email`, on the tab Providers (the first tab is active after a reload: no script keeps the active tab,
  `GroupShapes.cs:47-49`). *Visible at the end:* the Providers tab, "Default provider" showing "Simple Mail Transfer
  Protocol (SMTP)", and the green toast "Site settings updated successfully." at the top right if the page loaded less
  than 5 seconds before. *Labels:* `src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`
  (the message); `.../OrchardCore.Email/Views/EmailSettings.Edit.cshtml:20` (Default provider);
  `OrchardCore.Email.Smtp/Services/SmtpEmailProvider.cs:20` (the name of the provider).
- `config-9`: highlight the select `[name="ISite.EmailSettings.DefaultProvider"]` and leave the ring on it. Do not
  change it. *Visible at the end:* the ring on Default provider with "Simple Mail Transfer Protocol (SMTP)". *Source of
  the behavior:* `OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:193-200` (when the SMTP provider is enabled
  and the site has no default provider, SMTP becomes the default).

After this clip, stop the site and take snapshot C.

## Clip `04-test`

Starts on `/Admin` (opened before the capture), the menu expanded, every section closed. The inbox of the mail server is
empty.

- `test-1`: click the menu entry Tools, then Testing, then `ClickAndWaitForNavigationAsync` on Email Test. Wait until
  `#Provider` is shown. Highlight `#Provider` and leave the ring on it. *Visible at the end:* the Email Test page
  (`/Admin/Email/Test`), "Provider" showing "Simple Mail Transfer Protocol (SMTP)", then To, Sender, Cc, Bcc, Reply To,
  Subject, Body and the Send button. *Labels:* `src/OrchardCore.Modules/OrchardCore.Email/AdminMenu.cs:27-31` (Tools,
  Testing, Email Test); `.../Views/Admin/Test.cshtml:20` (Provider), `:23` (Select a provider), `:29` (To), `:37`
  (Sender), `:46` (Cc), `:54` (Bcc), `:62` (Reply To), `:70` (Subject), `:78` (Body), `:86` (Send);
  `.../Controllers/AdminController.cs:56` (the provider is the default one).
- `test-2`: clear the ring. `TypeAsync(#To, "jamie.chen@example.com")`. Then highlight `#From` and leave the ring on
  it; its hint reads "The sender is optional, it is useful when the email author is different than the email
  submitter." Leave the Sender empty. *Visible at the end:* To holding `jamie.chen@example.com`, a ring on the empty
  Sender. *Labels:* `Test.cshtml:41` (the hint). *Source of the behavior:* `OrchardCore.Email.Smtp/Services/SmtpEmailProviderBase.cs:54-56`
  (without a sender, the message uses the Sender email address of the SMTP settings).
- `test-3`: clear the ring. Move the cursor over Cc, Bcc and Reply To in turn (0.5 second each, no click, no typing).
  `TypeAsync(#Subject, "Test message")`, then `TypeAsync(#Body, "Hello from Orchard Core.")`. Keep the actions to
  about 6 seconds. *Visible at the end:* Subject `Test message` and Body `Hello from Orchard Core.`; Cc, Bcc, Reply To
  and Sender empty. *Labels:* `Test.cshtml:46,54,62,70,78`; Provider, To, Subject and Body are marked required
  (`Test.cshtml:20,29,70,78`).
- `test-4`: `ClickAndWaitForNavigationAsync` on `button:text-is("Send")`. The page posts the message to the mail server
  and returns to `/Admin/Email/Test` with an empty form. Wait for the page to load. *Visible at the end:* the Email Test
  page with its form empty again and the green toast "Message sent successfully." at the top right (it hides 5 seconds
  after the page loads: the narration says it in its second sentence; check the contact sheet that the toast is on the
  frame at the start of that sentence). *Labels:* `.../Controllers/AdminController.cs:82` (Message sent successfully.);
  `Test.cshtml:86` (Send). If an error shows instead (a red summary above the form), the settings of clip `03-configure`
  or the mail server are wrong: fix them, restore snapshot C, empty the mail server, and take the clip again.

## Clip `05-inbox`

Starts on `/Admin/Email/Test` (`OpenAsync` before the capture). Before `StartCaptureAsync`, open the mail server in a
second tab of the same context, and bring the first tab back to the front:
`var inbox = await scene.Page.Context.NewPageAsync(); await inbox.GotoAsync("http://localhost:3000", new() { WaitUntil = WaitUntilState.NetworkIdle }); await scene.Page.BringToFrontAsync();`.
Wait until the message list of smtp4dev shows one message (the subject `Test message`). Nothing in smtp4dev's own
interface comes from the Orchard Core source: the narration describes only what the recording shows.

- `inbox-1`: first action `await scene.SwitchToAsync(inbox)` (the capture follows the second tab, which was loaded
  before). Highlight the row of the message, `inbox.GetByText("Test message").First`, and leave the ring on it.
  *Visible at the end:* the smtp4dev page, its message list with exactly one message: the sender `noreply@example.com`,
  the recipient `jamie.chen@example.com` and the subject `Test message` in its row, with a ring on it.
- `inbox-2`: clear the ring, then click the row of the message. Wait until the text `Hello from Orchard Core.` is
  shown in the message view (`inbox.GetByText("Hello from Orchard Core.")`). **Do not open the headers, the raw message
  or the sections of the message**: they can show the name of the recording computer. *Visible at the end:* the message
  opened, with the sender `noreply@example.com`, the subject `Test message` and the text `Hello from Orchard Core.`

When this clip is done, stop and remove the mail server (`docker rm -f smtp4dev-video`).

## Checks for private data

The site is fresh and every value is fake, so no mask script is needed. Still, look at every contact sheet
(`review.py`) and OCR the assembled video (one frame per second, at every scene change, and at 5 frames per second)
for:

- The name in the top bar and the user menu: only `Orchard Core` and `admin`.
- Addresses: only `noreply@example.com`, `jamie.chen@example.com` and `admin@example.com`. No other address, domain or
  host name appears in the Orchard Core pages or in the mail server's page (search the OCR text for `@`). The one
  exception is the example server name in italics in the hint under Host name (`SmtpSettings.Edit.cshtml:54`), which
  is the product's own text and stays.
- The name of the recording computer: it can appear in the headers of a message and in a mail server's status line.
  The clips never open them. Search the OCR text of the clip `05-inbox` for the computer's name and for the user name
  of the account that ran the recording.
- The password: it is never typed on camera (the session comes from `state.json`), and no clip types in the User name
  or Password fields of the SMTP tab.
- The dates in the mail server's list are the recording time and may be shown.
- Any file `state.json`, `App_Data` or mask script stays out of git.
