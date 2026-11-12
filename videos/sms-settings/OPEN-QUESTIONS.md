# Open questions: SMS settings

The narration says only what was read in the source (commit `92db4a83` of `OrchardCMS/OrchardCore`). The points below
could not be run here: someone with a running site confirms each one before or while recording. Nothing in the
narration depends on an unconfirmed behavior, except where a point says so.

## 1. The made-up phone numbers must pass the validator

- **Checked:** every phone number is validated with `IsValidNumber`, parsed without a default region, so a number must
  start with a plus sign and a country code (`src/OrchardCore/OrchardCore.Infrastructure/PhoneFormatValidator.cs`).
  The Twilio settings (`Drivers/TwilioSettingsDisplayDriver.cs:104-112`) and the Azure settings
  (`Sms.Azure/Drivers/AzureSettingsDisplayDriver.cs:104-112`) refuse an invalid sender number when they are saved.
  The test page refuses an invalid recipient with a model error that its view does not display as a message (see
  point 3).
- **Why these numbers:** the three numbers are in the range that is reserved for fiction (`555-0100` to `555-0199`), so
  none belongs to a real person. `+14255550123` is also the example in `Sms.Azure/ConfigurationSchema.json`. The
  other two are `+12065550123` (Azure sender) and `+12015550123` (test recipient).
- **Checked:** `PhoneNumberUtil.IsValidNumber` of libphonenumber-csharp 9.0.30 (the build in the local NuGet cache, not
  necessarily the version Orchard Core pins at this commit) returned true for all three, parsed without a region.
- **To confirm:** save the Twilio tab with `+14255550123`, the Azure tab with `+12065550123`, and send a test message to
  `+12015550123`. If one is refused, choose another number of the same kind (country code `+1`, a valid area code,
  the exchange `555` and a line number from `0100` to `0199`), change it in `recording-plan.md`, and change nothing
  else: the narration never says a number.

## 2. Which default provider wins when a provider is enabled on a site with no default

- **Checked:** enabling Twilio (or Azure) while no default provider is set makes that provider the default
  (`Drivers/TwilioSettingsDisplayDriver.cs:152-158`, `Sms.Azure/Drivers/AzureSettingsDisplayDriver.cs:142-148`).
  The Providers tab posts its own drop-down in the same form, and `Drivers/SmsSettingsDisplayDriver.cs:67` copies the
  posted value to the settings. Which one is saved last depends on the order the drivers run in, which was not
  verified.
- **Effect on the video:** none. Clip `03-default` saves Log explicitly before any other provider is enabled, and the
  narration never says that a provider becomes the default by itself.
- **To confirm (optional):** on a site with no default, enable only Twilio and read the Providers tab after saving. The
  documentation page says to select Twilio as the default provider afterwards.

## 3. The SMS Test page: heading and validation messages

- **Checked:** `Views/Admin/Test.cshtml` has no title or breadcrumb (the email test page has one), so the page may show
  no heading. The view also has no validation summary and no validation message next to the fields, while the controller
  adds "Please select a valid provider." and "Please provide a valid phone number." to the model state
  (`Controllers/AdminController.cs:71,75`).
- **Effect on the video:** the narration identifies the page by its menu path, never by a heading, and the video never
  submits an invalid form.
- **To confirm:** the browser tab title and any heading the page shows, and what the page shows after Send with an invalid
  number or with no provider (nothing, a message, a red field). If a heading exists, `recording-plan.md` may mention it.

## 4. Clip `twilio-4`: the failed Twilio test

- **Checked:** the Twilio provider posts to Twilio with the standard resilience handler
  (`src/OrchardCore/OrchardCore.Sms.Core/ServiceCollectionExtensions.cs:48`) and turns every exception, and every
  answer other than "sent" or "queued", into a failure (`Services/TwilioSmsProvider.cs:88-99`). The test page then
  adds the error "The test SMS message failed to send." (`Controllers/AdminController.cs:93`) and shows the form again.
- **Not verified:** how long the retries take behind the dead proxy (the plan raises the navigation time-out of the page
  to 120 seconds, because the click on Send is a navigation that waits for the answer), whether the process honors
  `HTTP_PROXY` and `HTTPS_PROXY` for this client, and that the red toast stays visible long enough after the page
  loads.
- **To confirm:** record the step once with the site's output visible, check that no connection to Twilio is made, and
  time the wait. If the failure toast does not appear within 120 seconds, or the page shows something else, the step
  needs a decision: the narration of `twilio-4` promises that failure message.

## 5. After Save, which tab is active, and does the new provider appear at once?

- **Checked:** the settings page redirects to itself after a successful save
  (`src/OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:122`), and the tabs are drawn with
  the first one active (`src/OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:47`). Whether the
  admin remembers the last tab was not verified; the plan clicks the tab again when it is not the expected one. The
  Twilio and Azure tabs are both placed at `Content:5` (`TwilioSettingsDisplayDriver.cs:67`,
  `Sms.Azure/Drivers/AzureSettingsDisplayDriver.cs:66`), so which of the two comes first was not verified; the plan
  and the narration never depend on it.
- **To confirm:** after saving the Twilio settings, the Provider list of the SMS Test page offers "Twilio" at once
  (the save asks for the provider options to be built again, `TwilioSettingsDisplayDriver.cs:162-166`), and after
  saving the Azure settings it offers "Azure". The drop-downs show the technical names "Azure", "Log" and "Twilio"
  (`SmsSettingsDisplayDriver.cs:41-44`), not the friendly name "Log - writes messages to the logs".

## 6. The Features page

- **Checked:** the Enable link of a feature has the id `btn-enable-` followed by the feature id
  (`src/OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`); the framework turns
  the dots of the feature id into underscores, which was not run here. A search for "SMS" also lists other features
  (for example "SMS Notifications"); the narration never counts them.
- **To confirm:** the ids `btn-enable-OrchardCore_Sms` and `btn-enable-OrchardCore_Sms_Azure`, and that the admin menu
  shows Settings, Communication, SMS right after the feature is enabled, without a second reload.

## 7. The Log provider writes to the log, not to the screen

- **Checked:** `LogSmsProvider` writes a warning with the body and the number, then reports success
  (`src/OrchardCore/OrchardCore.Sms.Core/Services/LogSmsProvider.cs:33`). The text of the line says "was set to", as the
  code spells it. The Log provider is registered only in a development environment
  (`src/OrchardCore.Modules/OrchardCore.Sms/Startup.cs:31-34`).
- **To confirm:** after clip `04-test`, the file `App_Data/logs/orchard-log-<date>.log` holds that warning. The narration
  of `test-3` says that the message is written to the application log and nothing is delivered.

## 8. Pronunciation

- **To confirm with `scripts/pronounce.py`:** how the voice says "Twilio", "SID" (in "Account SID") and "Auth Token". The
  storyboard has no `spoken` entry. If "SID" should be spelled out, add `"SID": "S.I.D."` to `spoken` once a person
  has heard it; the captions keep the written form.
