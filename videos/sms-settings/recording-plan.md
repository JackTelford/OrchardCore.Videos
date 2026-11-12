# Recording plan: SMS settings

Everything a person needs to record this video without deciding anything. Source paths are relative to the root of
`OrchardCMS/OrchardCore` (commit `92db4a83`). In this file a selector stands for `scene.Page.Locator(...)`, and a
label is the text on screen, as the code spells it.

The video shows the admin UI only. The terminal and the application log are never on screen: they are used outside the
capture, to prepare the site and to check a result.

## 1. The site it needs

| | |
|---|---|
| Source | `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, with its own `App_Data` folder outside both repositories (`ORCHARD_APP_DATA`). |
| Environment | **`ASPNETCORE_ENVIRONMENT=Development`.** The Log provider is registered only in a development environment (`src/OrchardCore.Modules/OrchardCore.Sms/Startup.cs:31-34`). In any other environment the Providers list is empty and the SMS Test page shows only "No SMS providers are available. Enable at least one SMS provider feature." (`Views/Admin/Test.cshtml:8`). |
| Setup | Auto Setup through environment variables: recipe **Blog**, site name **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, password `<password>` (chosen outside git and passed in `VIDEO_PASSWORD`). No setup or sign-in form is recorded; sign in once outside the capture with `Recorder setup`, which saves `state.json`. |
| Port | `http://localhost:5010` (`VIDEO_BASE_URL`). |
| Outbound calls | Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address (for example `http://127.0.0.1:9`) **in the environment of the site's process only**, never in the recorder's. Clip `05-twilio` sends a request through the Twilio provider on purpose, and it must never reach the Internet. |
| Features before recording | None beyond what the Blog recipe enables (that recipe enables neither SMS nor Secrets). **SMS** (`OrchardCore.Sms`) and **Azure Communication Services SMS** (`OrchardCore.Sms.Azure`) are enabled on camera, in clips `02-enable` and `06-azure`. **Secrets must stay disabled**, so the Auth Token and Connection String are plain password boxes (`src/OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:96-104`). Do not enable Workflows, Notifications or Health Checks. |
| Settings | None. All SMS settings are saved on camera. |
| External services | None. The Log provider writes to the application log; Twilio and Azure are given made-up values and are never reached. |
| Seed data | None. Every value is typed on camera, from the list below. |

### The made-up values

Use exactly these. They are invented, and none is a real account, number or key.

| Where | Value |
|---|---|
| Twilio, Phone Number (sender) | `+14255550123` |
| Twilio, Account SID | `AC00000000000000000000000000000000` |
| Twilio, Auth Token | `fake-auth-token` (masked on screen) |
| Azure, Phone Number (sender) | `+12065550123` |
| Azure, Connection String | `endpoint=https://example.com/;accesskey=fake-key` (masked on screen) |
| SMS Test, Phone Number (recipient) | `+12015550123` |

The phone numbers go through a country-code validator when saved or sent (`src/OrchardCore/OrchardCore.Infrastructure/PhoneFormatValidator.cs`): a number must start with a plus sign and a country code, and be valid for that country. If a take shows a validation error on a number, stop and read `OPEN-QUESTIONS.md`.

## 2. Order, dependencies and snapshots

The clips depend on each other and are recorded in the order they play:

1. `02-enable` needs the fresh site. It enables SMS.
2. `03-default` needs SMS enabled. It saves Log as the default provider.
3. `04-test` needs `03-default` (the Log provider is listed because the site runs in development; the default does not matter to the test). It changes nothing on the site; it adds one line to the application log.
4. `05-twilio` needs SMS enabled and Log as the default. It saves the Twilio settings.
5. `06-azure` needs `05-twilio`. It enables the Azure feature and saves the Azure settings and Azure as the default provider.

Clips that **change the site** and need a restored snapshot before a new take: `02-enable`, `03-default`, `05-twilio`, `06-azure`. Take these snapshots (stop the site, copy `App_Data` and `state.json` aside, start it again):

| Snapshot | Taken | Restore it to record again |
|---|---|---|
| A | After Auto Setup and `Recorder setup`, before any clip | `02-enable` |
| B | After `02-enable` | `03-default` |
| C | After `03-default` | `04-test`, `05-twilio` |
| D | After `05-twilio` | `06-azure` |

`04-test` can be recorded again at any time after `03-default`, from snapshot C or from a later state: the Log provider stays listed. `05-twilio` leaves no trace beyond the Twilio settings.

Every clip opens its page before the capture starts, with the left admin menu **collapsed**, unless the entry says otherwise.

Pages used: `/Admin` (home of the admin), `/Admin/Features`, `/Admin/Settings/sms` (SMS settings), `/Admin/sms/test` (SMS Test). In Git Bash on Windows, set `MSYS_NO_PATHCONV=1`, or a path such as `/Admin/Features` is turned into a Windows path.

A notification is a toast, a small box over the admin page (`src/OrchardCore.Themes/TheAdmin/Views/Message.cshtml`): green (`.toast.message-success`) for a success, red (`.toast.message-error`) for an error. It can hide itself after a while, so the step must end while it is still visible: wait for it, then do nothing else until the narration ends.

## 3. Chapter 00: opening card

`intro`: a card step, no recording. The card shows the title "Orchard Core SMS settings" and the subtitle "Choose a provider, save its settings, and send a test message".

## 4. Chapter 01: How SMS works (slides)

Slides are drawn by `slides.py`. No recording.

- `how-1`: a slide "SMS providers" with three cards: Log, Twilio, Azure Communication Services.
- `how-2`: a slide "The default provider sends the site's messages" with three cards: Workflow tasks, Notifications, Two-factor sign-in.
- `how-3`: a slide "Where to find it" with three cards: Settings, Communication, SMS; Tools, Testing, SMS Test; Manage SMS Settings (the permission both pages need; the settings page also needs the permission Manage settings, because `Settings/AdminController.cs:53` checks it and the SMS module does not register its own settings group permission).

## 5. Chapter 02: clip `02-enable`

**Starts on:** `/Admin`, signed in, menu collapsed. **Changes the site:** yes (enables SMS).

### `enable-1`

1. Click **Tools** in the left menu so that it expands (`src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26`).
2. Click **Features** under Tools, with a click that waits for the navigation. `/Admin/Features` loads (`AdminMenu.cs:27`).
3. Type `SMS` in the search box (`#search-box`, placeholder "Search", `Views/Admin/Features.cshtml:27`). Wait for the list to be filtered: the text in `#list-summary` no longer says that every feature is shown.
4. Highlight the **Enable** button of the row whose title is exactly "SMS" (category "Communication"). The button is the link `a#btn-enable-OrchardCore_Sms` (the id is the feature id with the dot replaced by an underscore: confirm it with `Recorder probe`); if the id differs, use the "Enable" link inside the list item whose title is exactly "SMS", not "SMS Notifications" or "Azure Communication Services SMS".

**On screen when the narration ends:** the Features page, "SMS" in the search box, the filtered list with the "SMS" feature in the Communication category and a green ring on its Enable button. Do not read out or rely on how many rows match.

Labels: Tools and Features `src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27`; Search `Views/Admin/Features.cshtml:27`; Enable `Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`; feature name "SMS" and category "Communication" `src/OrchardCore.Modules/OrchardCore.Sms/Manifest.cs:10-13`.

### `enable-2`

1. Clear the highlight, then click the **Enable** link of the "SMS" row, with a click that waits for the navigation (the page posts and reloads, and the capture freezes otherwise).
2. Wait for the green toast "The feature SMS was enabled." (`src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`).
3. Click **Settings** in the left menu so that it expands, then click **Communication** so that it expands (`src/OrchardCore.Modules/OrchardCore.Sms/AdminMenu.cs:27-29`).
4. Highlight the **SMS** item under Communication.

**On screen when the narration ends:** the Features page with the green toast "The feature SMS was enabled.", and the left menu open on Settings, Communication, with a green ring on SMS.

## 6. Chapter 03: clip `03-default`

**Starts on:** `/Admin`, signed in, menu collapsed. **Changes the site:** yes (saves Log as the default provider).

### `default-1`

1. Click **Settings**, then **Communication** (each expands), then click **SMS** with a click that waits for the navigation. `/Admin/Settings/sms` loads (`src/OrchardCore.Modules/OrchardCore.Sms/AdminMenu.cs:27-34`).
2. Highlight the drop-down labeled "Default SMS provider" on the first tab, with the caption "Default SMS provider".

**On screen when the narration ends:** the SMS settings page with two tabs, **Providers** (active) and **Twilio**, the label "Default SMS provider" and its drop-down showing "Select a provider", ringed in green. (If the Azure tab is present, the Azure feature was enabled by mistake: restore snapshot B.)

Labels: tab "Providers" `src/OrchardCore.Modules/OrchardCore.Sms/Drivers/SmsSettingsDisplayDriver.cs:48`; tab "Twilio" `Drivers/TwilioSettingsDisplayDriver.cs:67`; "Default SMS provider" and "Select a provider" `Views/SmsSettings.Edit.cshtml:6,9`.

### `default-2`

1. Clear the highlight.
2. Select the option **Log** in the "Default SMS provider" drop-down. The drop-down is a native one: a headless browser draws no open list, so the ripple shows and the value changes.

**On screen when the narration ends:** the drop-down shows "Log". The options of the list are not visible; the narration describes them, and the drop-down holds only "Select a provider" and "Log". Check this once outside the capture (`Recorder probe` on the options of the select): it lists the enabled providers by their technical names (`Drivers/SmsSettingsDisplayDriver.cs:41-44`), which on this site is only Log.

### `default-3`

1. Click **Save** at the bottom of the form (button with the text "Save", `src/OrchardCore.Modules/OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`), with a click that waits for the navigation.
2. Wait for the green toast "Site settings updated successfully." (`src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119`).

**On screen when the narration ends:** the SMS settings page reloaded, the green toast, and the Providers tab with "Log" in the drop-down.

## 7. Chapter 04: clip `04-test`

**Starts on:** `/Admin`, signed in, menu collapsed. **Changes the site:** no (it writes one line to the application log).

### `test-1`

1. Click **Tools** so that it expands, then **Testing** so that it expands, then click **SMS Test** with a click that waits for the navigation. `/Admin/sms/test` loads (`src/OrchardCore.Modules/OrchardCore.Sms/AdminMenu.cs:38-40`).
2. Highlight the label "Provider" with its drop-down, then the label "Phone Number" with its field.

**On screen when the narration ends:** the SMS Test form: "Provider" with a drop-down showing "Select a provider", "Phone Number" with an empty field and the hint "Phone number must include a country code. For example, +1 for United States.", and the Send button; the rings on Provider and Phone Number. This page has no heading of its own (`Views/Admin/Test.cshtml` has no title), so do not look for one.

Labels: "Provider", "Select a provider", "Phone Number", the hint and "Send" `src/OrchardCore.Modules/OrchardCore.Sms/Views/Admin/Test.cshtml:17,20,26,29,34`.

### `test-2`

1. Clear the highlights.
2. Select the option **Log** in the "Provider" drop-down.
3. Type `+12015550123` in the Phone Number field (`input[type=tel]`).

**On screen when the narration ends:** "Log" in Provider, `+12015550123` in Phone Number, and the hint under the field readable.

### `test-3`

1. Click **Send** with a click that waits for the navigation. The page posts, then redirects to `/Admin/sms/test`.
2. Wait for the green toast "The test SMS message has been successfully sent." (`src/OrchardCore.Modules/OrchardCore.Sms/Controllers/AdminController.cs:87`).

**On screen when the narration ends:** the SMS Test page with the green toast, and an empty form again ("Select a provider", empty Phone Number).

After the clip, outside the capture, check that the application log (`App_Data/logs/orchard-log-<date>.log`) holds a warning with the body and the number: "A message with the body 'This is a test SMS message.' was set to '+12015550123'." (`src/OrchardCore/OrchardCore.Sms.Core/Services/LogSmsProvider.cs:33`). The log is not shown in the video.

## 8. Chapter 05: clip `05-twilio`

**Starts on:** `/Admin`, signed in. Before the capture starts, expand **Settings** and **Communication** in the left menu (they are not part of the story here). **Changes the site:** yes (saves the Twilio settings).

### `twilio-1`

1. Click **SMS** under Communication, with a click that waits for the navigation. The page opens on the Providers tab.
2. Click the **Twilio** tab.
3. Click the checkbox labeled **Enable Twilio Provider** (click its label). A section with the heading **Twilio Account Info** expands.

**On screen when the narration ends:** the Twilio tab active, "Enable Twilio Provider" checked, the heading "Twilio Account Info", and three empty fields: "Phone Number" (with the hint "Phone number must include a country code. For example, +1 for United States."), "Account SID" and "Auth Token" (a password box whose placeholder is "Enter a new value").

Labels: `src/OrchardCore.Modules/OrchardCore.Sms/Views/TwilioSettings.Edit.cshtml:11` (Enable Twilio Provider), `:19` (Twilio Account Info), `:24,28` (Phone Number and its hint), `:33` (Account SID), `:41` (Auth Token); placeholder `src/OrchardCore/OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:104`.

### `twilio-2`

1. Type `+14255550123` in Phone Number.
2. Type `AC00000000000000000000000000000000` in Account SID.
3. Type `fake-auth-token` in Auth Token (the password box of the Twilio section). The characters appear as dots.

**On screen when the narration ends:** the three fields filled; Phone Number and Account SID readable, Auth Token shown as dots.

### `twilio-3`

1. Click **Save**, with a click that waits for the navigation. Wait for the green toast "Site settings updated successfully."
2. If the page opened on the Providers tab, click the **Twilio** tab again.
3. Highlight the Auth Token field.

**On screen when the narration ends:** the green toast, the Twilio tab with "Enable Twilio Provider" checked, Phone Number and Account SID filled, and the Auth Token field empty with the grey placeholder "The value is securely stored. Enter a new value to update it, or leave it blank to keep it." (`SecretInputTagHelper.cs:103`), ringed in green.

### `twilio-4`

1. Clear the highlight. Navigate to `/Admin/sms/test` (a navigation that keeps capturing).
2. Select the option **Twilio** in the "Provider" drop-down. (If Twilio is not in the list, the settings did not save: restore snapshot C and record the clip again.)
3. Type `+12015550123` in Phone Number.
4. Move the cursor onto **Send** on camera, then post the form inside the cut. Before the capture starts, raise the navigation time-out of the page (`scene.Page.SetDefaultNavigationTimeout(120000)`). Then run `scene.OffCameraAsync(...)` around a click that waits for the navigation (`scene.WaitForNavigationAsync(() => scene.ClickAsync(Send))`): the form posts, the Twilio provider retries before it gives up (`src/OrchardCore/OrchardCore.Sms.Core/ServiceCollectionExtensions.cs:48`), so the wait can last many seconds, and the page that comes back is a new document, so a click that does not wait for it freezes the capture. The ripple of that click is not recorded; the cursor rests on Send until the cut. Inside the cut, after the navigation, wait for this condition: the red toast "The test SMS message failed to send." (`src/OrchardCore.Modules/OrchardCore.Sms/Controllers/AdminController.cs:93`) is visible (`.toast.message-error`, `WaitForAsync` with a time-out of 120 seconds).

**On screen when the narration ends:** the SMS Test page with the red toast "The test SMS message failed to send.", "Twilio" in Provider and `+12015550123` in Phone Number.

The failure has the same result whether the site cannot reach Twilio or Twilio refuses the made-up values (`src/OrchardCore/OrchardCore.Sms.Core/Services/TwilioSmsProvider.cs:88-99`). Check outside the capture, with the site's output, that no request left the machine.

## 9. Chapter 06: clip `06-azure`

**Starts on:** `/Admin`, signed in, menu collapsed. **Changes the site:** yes (enables Azure Communication Services SMS, saves its settings and Azure as the default provider).

### `azure-1`

1. Click **Tools**, then click **Features** with a click that waits for the navigation.
2. Type `SMS` in the search box.
3. Click the **Enable** link of the row whose title is exactly "Azure Communication Services SMS" (`a#btn-enable-OrchardCore_Sms_Azure`; confirm the id with `Recorder probe`), with a click that waits for the navigation (the page posts and reloads, and the capture freezes otherwise).
4. Wait for the green toast "The feature Azure Communication Services SMS was enabled." (`src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:317`; the name is `src/OrchardCore.Modules/OrchardCore.Sms.Azure/Manifest.cs:10`).

**On screen when the narration ends:** the Features page with the green toast.

### `azure-2`

1. Navigate to `/Admin/Settings/sms` (a navigation that keeps capturing). The SMS settings page has three tabs: Providers (the first one, active), and Twilio and **Azure Communication Services** (`src/OrchardCore.Modules/OrchardCore.Sms.Azure/Drivers/AzureSettingsDisplayDriver.cs:66`). Both of the last two are placed at the same position (`Content:5`), so their order is not known: click the tabs by their text, never by their place.
2. Click the **Azure Communication Services** tab.
3. Click the checkbox whose label is exactly **Enable** (click its label; the Twilio tab has a label "Enable Twilio Provider", so do not match the text "Enable" as a substring, and use the label inside the Azure tab). The fields expand.
4. Type `+12065550123` in Phone Number (hint: "The default phone number to use as a sender. Phone number must include a country code. For example, +1 for United States.").
5. Type `endpoint=https://example.com/;accesskey=fake-key` in Connection String (a password box with the placeholder "Enter the connection string"; the characters appear as dots).
6. Click **Save**, with a click that waits for the navigation, and wait for the green toast "Site settings updated successfully."
7. If the page opened on the Providers tab, click the **Azure Communication Services** tab again.

**On screen when the narration ends:** the green toast and the Azure Communication Services tab with "Enable" checked and the Phone Number filled; Connection String empty with the placeholder "The value is securely stored. Enter a new value to update it, or leave it blank to keep it."

Labels: `src/OrchardCore.Modules/OrchardCore.Sms.Azure/Views/AzureSmsSettings.Edit.cshtml:11` (Enable), `:18,22` (Phone Number and its hint), `:27,29` (Connection String and its placeholder).

### `azure-3`

1. Click the **Providers** tab.
2. Select the option **Azure** in the "Default SMS provider" drop-down.
3. Click **Save**, with a click that waits for the navigation, and wait for the green toast "Site settings updated successfully."

**On screen when the narration ends:** the green toast and the Providers tab with "Azure" in the "Default SMS provider" drop-down. The list of the options is not visible (a native drop-down); it holds "Select a provider", "Azure", "Log" and "Twilio" (check once outside the capture, `Drivers/SmsSettingsDisplayDriver.cs:41-44`). If a provider named "DefaultAzure" is listed, the server's configuration holds Azure values: remove them and record again.

## 10. Chapter 99: closing card

`outro`: a card step, no recording. The card shows the title "Orchard Core SMS settings" and the subtitle "docs.orchardcore.net".

## 11. Checks for private data

Before the video is assembled, look at every contact sheet and, after assembling, OCR the video at one frame per second and at every scene change. The text on screen must hold only:

- the made-up values of the table in section 1, and `admin` / `admin@example.com` if the user menu is open;
- no password: the recorder signs in outside the capture, and the Auth Token and Connection String are typed into password boxes that show dots;
- no real name, email address, phone number, domain (only `example.com`), Twilio SID or Azure key.

Check also that no toast or page shows a stack trace or an error other than "The test SMS message failed to send." in `twilio-4`, and that the application log, the site's console and `App_Data` never appear in a frame.
