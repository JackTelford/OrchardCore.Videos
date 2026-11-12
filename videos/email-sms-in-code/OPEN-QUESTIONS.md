# Open questions: Send Email, SMS and Notifications From Your Module

Everything in the narration was read from the Orchard Core source (the `file:line` references are in
`recording-plan.md`; paths start in `src/`, where the libraries are in `OrchardCore/` and the modules in `OrchardCore.Modules/`). The items below could not be confirmed without a running site or a person's ear.

## 1. Spoken forms to confirm with `scripts/pronounce.py`

The skill asks a person to confirm each new spelling before it is used. Nothing was synthesized for this video.

- Spelled-out identifiers in the narration: "I Email Service", "I SMS Service", "I Notification Service", "I Notifier",
  "I Email Provider", "I SMS Provider", "I Notification Manager", "I Notification Method Provider",
  "I Email Service Events", "Send Async", "Mail Message", "SMS Message", "Notification Message",
  "Notification Send Result", "Text Body", "Reference Id", "Disabled Methods", "Add Notification Type".
- New `"spoken"` entries of `storyboard.json`: `Bcc` as "B.C.C.", `Cc` as "C.C.", `Html` as "H.T.M.L.", `Url` as "U.R.L.".
  They are plain text replacements, so each must occur only where intended. They do today: `Bcc` and `Cc` only in
  "To, Cc, and Bcc" and "To, Cc, or Bcc"; `Html` only in "Html Body", "Html markup" and "Is Html Preferred"; `Url` only in
  "a Url". None is the last word of a sentence, so the voice never reads "B.C.C.." or "H.T.M.L..".
- Whether the voice reads "SMS" in "I SMS Service" as three letters, and "Twilio", "SMTP" and "Async" correctly.

## 2. Behavior to confirm on a running site

Each item is written in the narration or on a slide because the code says so. A person who runs a small module can
confirm it in a few minutes (the slide code is meant to be pasted into one).

1. **A module's features are enabled with it.** The manifest `Dependencies` list is read by
   `ShellDescriptorFeaturesManager.GetFeaturesToEnable` (`OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:163`), which
   enables the dependencies only when `force` is set; the Features page posts `Force=true`
   (`OrchardCore.Modules/OrchardCore.Features/Views/Admin/Features.cshtml:49`). Confirm that enabling the sample module under
   Tools, Features also enables Email, SMS and Notifications. The narration says "the Features page enables them together
   with your module".
2. **An unknown email provider name throws.** `DefaultEmailProviderResolver.GetAsync` throws `InvalidEmailProviderException`
   (`:39`), and `DefaultEmailService.SendAsync` does not catch it. Confirm with `SendAsync(message, "Unknown")`.
3. **Providers validate differently.** `TwilioSmsProvider.SendAsync` throws `ArgumentException` for an empty `To` or `Body`
   before its `try` block (`:55`, `:60`), and `AzureSmsProviderBase.SendAsync` returns a failed `Result` (`:59`, `:68`, `:77`).
   Confirm with an empty body for each provider. A related case, not in the narration: a notification without a
   `TextBody` sent to a user who has a phone number reaches `SmsNotificationProvider` (`:41`), and `NotificationService.SendAsync`
   has no `try` around the providers (`:56` to `:60`), so the exception would leave `SendAsync` when Twilio is the default provider.
4. **The Log provider exists only in Development.** `OrchardCore.Modules/OrchardCore.Sms/Startup.cs:31` registers it only when
   `IsDevelopment()`. Confirm that "Log" is missing from the Default SMS provider list when the environment is Production.
5. **Without a selected SMS provider.** The narration says `ISmsService` "uses the default SMS provider that the site
   settings select". When none is selected, `DefaultSmsProviderResolver.GetAsync` falls back to the last registered provider
   (`OrchardCore.Sms.Core/Services/DefaultSmsProviderResolver.cs`), whereas the documentation says a default must be configured.
   Confirm what a site that never saved the SMS settings does.
6. **A non-`User` recipient.** `CoreNotificationEventsHandler.CreatingAsync` sets `UserId` only when the recipient is a `User`
   (`:18`), and the Email and SMS methods cast the recipient to `User`. Confirm that a notification sent to another object is
   stored without a user, so no notification center lists it. The narration only says to pass a `User` object.
7. **What a registered type looks like.** The narration says the display name labels the notifications, fills the Type filter of
   the notification center and appears in the notification preferences of each user. This was read from
   `NotificationsType_Header.cshtml`, `NotificationsType_SummaryAdmin.cshtml`, `Controllers/AdminController.cs:109` and
   `UserNotificationPreferencesPart.Edit.cshtml`, and was not seen on a screen.

## 3. The slide code was not built inside a module

Every listing of `slides.py` was compiled in a scratch project outside the repository, against the real source files of
`MailMessage`, `IEmailService`, `EmailServiceExtensions`, `SmsMessage`, `ISmsService`, `INotificationService`,
`NotificationMessage`, `NotificationSendResult`, `INotificationManager`, `NotificationFilter`, `AddNotificationType`,
`INotifier` and the provider interfaces. `User`, `Entity`, `StartupBase`, `ModuleAttribute` and the two `Add...Provider`
extensions were replaced by stubs with the signatures read from the source. A person should paste the listings into a
module of a real site to confirm they build, and that `Dependencies = [ ... ]` in the manifest (a collection expression)
builds with the language version of the module project.
