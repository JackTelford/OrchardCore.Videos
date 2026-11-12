# Recording plan: Send Email, SMS and Notifications From Your Module

A developer video made of the opening card, slides, and the closing card. **Nothing is recorded in a browser**: the
storyboard has no `clip` steps, so there is no site to run, no Auto Setup, no tenant, no features to enable, no
settings to save, no external service, no seed data, no `state.json`, no `VIDEO_PASSWORD`, and no recorder method.
The images come from `slides.py` and `scripts/brand.py`; the voice comes from `storyboard.json`.

Source paths below start in `src/` of the Orchard Core source: libraries are in `OrchardCore/`, modules in
`OrchardCore.Modules/`.

## The site it needs

None. Do not start Orchard Core, do not run `Recorder setup`, `Recorder prepare` or `Recorder record`.

## Order, and what changes the site

There are no clips, so no clip depends on another and none changes a site (nothing to snapshot or restore). Produce
the video in this order:

1. A person confirms the spoken form of the identifiers listed in `OPEN-QUESTIONS.md` with `scripts/pronounce.py`, and
   adds the confirmed spellings to `"spoken"` in `storyboard.json`.
2. `scripts/narrate.py videos/email-sms-in-code` (voice and `durations.json`).
3. `scripts/assemble.py videos/email-sms-in-code` (cards, slides, captions).
4. Look at one frame of every slide, and at the opening and closing cards. Check that no code line is cut at the right
   edge of its panel, and that the dimmed lines of a code panel are the ones the narration does not talk about.

## Cards

- `intro`: the opening card (full logo), the title "Email, SMS and Notifications in Code", and the subtitle "Three
  services for the messages of your module".
- `outro`: the closing card, the same title, and the subtitle "docs.orchardcore.net".

## Chapter 01, Three services (`01-services`, slides)

- `services-1`: heading "Three services, one pattern". Four cards; the first three are bright: `IEmailService`,
  `ISmsService`, `INotificationService` (the services of the Email, SMS and Notifications modules).
- `services-2`: the same four cards, with the fourth bright: "Results", that is `Result` for email and SMS and
  `NotificationSendResult` for notifications.
- `services-3`: heading "Declare the features in the manifest". The module manifest with `Dependencies` listing
  `OrchardCore.Email`, `OrchardCore.Sms` and `OrchardCore.Notifications`, and three cards: Email Notifications
  (`OrchardCore.Notifications.Email`), SMS Notifications (`OrchardCore.Notifications.Sms`), and "Optional features".
  Source: `OrchardCore.Modules/OrchardCore.Notifications/Manifest.cs:21` and `OrchardCore.Modules/OrchardCore.Sms/Manifest.cs`.

## Chapter 02, Send an email (`02-email`, slides)

All four slides come from `OrchardCore/OrchardCore.Email.Abstractions` (`IEmailService.cs:14`, `MailMessage.cs`,
`MailMessageExtensions.cs:5`, `Extensions/EmailServiceExtensions.cs:21` and `:45`) and `OrchardCore.Email.Core`
(`Services/DefaultEmailService.cs`, `Services/EmailMessageValidator.cs:54`, `Services/DefaultEmailProviderResolver.cs:39`).

- `email-1`: heading "1. Inject IEmailService". The `OrderMailer` class; the field, the constructor and the closing
  brace are bright. Cards: Namespace `OrchardCore.Email`, Registered by "The Email feature."
- `email-2`: heading "2. Create a MailMessage". Same class; the `MailMessage` initializer is bright. Cards: To, Cc, Bcc;
  HtmlBody; TextBody.
- `email-3`: heading "3. Send it and check the Result". Same class; the `SendAsync` call and `Succeeded` are bright. Cards:
  Succeeded, Errors, Validation.
- `email-4`: heading "4. Choose a provider, or a shorter overload". Three calls: the default provider, the provider
  named `SMTP`, and the overload with recipient, subject and body. Cards: Technical name, Unknown name, Shorter overload.

## Chapter 03, Send an SMS (`03-sms`, slides)

Sources: `OrchardCore.Sms.Abstractions` (`SmsMessage.cs`, `ISmsService.cs:13`, `Extensions/SmsServiceExtensions.cs`),
`OrchardCore.Sms.Core` (`Services/SmsService.cs:29`, `Services/TwilioSmsProvider.cs:55` and `:60`, `Services/LogSmsProvider.cs:33`),
`OrchardCore.Sms.Azure/Services/AzureSmsProviderBase.cs:59`, `:68` and `:77`, and `OrchardCore.Sms/Startup.cs:31`.

- `sms-1`: heading "1. Create an SmsMessage". The `SendShippedAsync` method; the `SmsMessage` initializer is bright.
  Cards: To, Body, From.
- `sms-2`: heading "2. Send it to the default provider". Two calls: with the message, and with the phone number
  `+14255550142` and the text. Cards: No provider argument, Shorter overload.
- `sms-3`: heading "3. Check the values first". A guard on the phone number and the text, then the call and
  `Succeeded`. Cards: Twilio (throws an exception), Azure (returns a failed `Result`), Your code.
- `sms-4`: heading "While you develop: the Log provider". Three cards: Log provider, Development only, Default SMS provider
  (the label of the setting is `Default SMS provider`, `OrchardCore.Modules/OrchardCore.Sms/Views/SmsSettings.Edit.cshtml:6`).

## Chapter 04, Notify a user (`04-notify`, slides)

Sources: `OrchardCore.Notifications.Abstractions` (`INotificationService.cs`, `INotificationMessage.cs`,
`NotificationSendResult.cs:47`), `OrchardCore.Notifications.Core` (`Models/NotificationMessage.cs`,
`Services/NotificationService.cs:37` to `:121`, `Services/EmailNotificationProvider.cs:34` and `:44`),
`OrchardCore.Sms.Core/Services/SmsNotificationProvider.cs:38` and `:41`,
`OrchardCore.Modules/OrchardCore.Notifications/Handlers/CoreNotificationEventsHandler.cs:18`,
`OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:70`.

- `notify-1`: heading "1. Inject INotificationService". The `OrderNotifier` class; the field, the constructor, the method
  signature with `User user` and the closing brace are bright. Cards: Recipient, Stored first, Then sent.
- `notify-2`: heading "2. Build a NotificationMessage". Same class; the `NotificationMessage` initializer is bright.
  Cards: Subject, Summary, TextBody.
- `notify-3`: heading "3. Read the NotificationSendResult". A call, then the five members of the result with a comment
  each: `Notification`, `SuccessfulCount`, `FailedCount`, `Errors`, `Status`. No cards; the panel is full width.
- `notify-4`: heading "Which methods run". Three cards: Features, Preferences, Failures.
- `notify-5`: heading "INotifier or INotificationService?". A `SuccessAsync` call and a `SendAsync` call, each with a
  comment. Cards: INotifier, INotificationService.

## Chapter 05, Notification types (`05-types`, slides)

Sources: `OrchardCore.Notifications.Abstractions/NotificationTypeServiceCollectionExtensions.cs:17`,
`NotificationTypeDescriptor.cs:46`, `INotificationManager.cs`, `NotificationFilter.cs:45`,
`OrchardCore.Notifications.Core/NotificationConstants.cs:20` and `:25`, `NotificationUrlHelper.cs:24`,
`OrchardCore.Modules/OrchardCore.Notifications/Controllers/AdminController.cs:109`,
`Drivers/UserNotificationPreferencesPartDisplayDriver.cs:57`.

- `types-1`: heading "1. Register the type". The `OrderNotificationTypes` constant and the `Startup` class; the constant and
  the first arguments of `AddNotificationType` are bright. Cards: Technical name, Display name.
- `types-2`: heading "2. Display name and default methods". Same code; the display name, the description and
  `DisabledMethods.Add("SMS")` are bright. Cards: Type filter, Preferences, DisabledMethods.
- `types-3`: heading "3. Send notifications of the type". A `NotificationMessage` with `Type`, `ReferenceId` and `Url`
  (bright), then the `SendAsync` call. Cards: Type (at most 64 characters), ReferenceId (at most 255), Url.
- `types-4`: heading "4. Keep them in sync with the record". Two calls on `INotificationManager`: `DeleteAsync` and
  `MarkAsReadAsync`, each with a `NotificationFilter`. Cards: Filter, Required, Also.

## Chapter 06, Extension points (`06-extend`, slides)

Sources: `OrchardCore.Email.Core/ServiceCollectionExtensions.cs:20`, `OrchardCore.Sms.Core/ServiceCollectionExtensions.cs:21`,
`OrchardCore.Modules/OrchardCore.Notifications/Startup.cs:111`, `OrchardCore.Email.Core/Services/EmailServiceEventsBase.cs`,
`OrchardCore.Modules/OrchardCore.Email/Drivers/EmailSettingsDisplayDriver.cs:90`.

- `extend-1`: heading "Add an email or SMS provider". `AddEmailProvider` and `AddSmsProvider` with a technical name, and a
  comment with the members of each interface. Cards: Technical name, Email only.
- `extend-2`: heading "Add a notification method or follow email events". A scoped registration of an
  `INotificationMethodProvider` and of an `IEmailServiceEvents`. Cards: Notification method, EmailServiceEventsBase.

## Checks for private data

- All names, addresses and numbers on the slides are invented: `Jamie Chen` (the author of the sample manifest),
  `jamie.chen@example.com`, `+14255550142`, "Order 1042". Search `slides.py` and `storyboard.json` for `@` and for `+1`: only
  these may appear.
- No password, key, token or connection string appears on any slide, in the narration or in `video.json`.
- Nothing from a real site is shown, since no site is recorded; the check is a look at one frame of every slide.
