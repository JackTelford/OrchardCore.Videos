# Open questions: security-recaptcha

What was checked in the Orchard Core source, and what a person must confirm on a running site before or while
recording. Paths are relative to the Orchard Core checkout (`src/OrchardCore.Modules/...`, `src/OrchardCore/...`). The
narration was kept to what the source shows; each item below says what it does not claim.

## 1. A refused sign-in may show no message

- **Checked**: the Log in page reports a failed check under the key `ReCaptcha` (`OrchardCore.ReCaptcha.Core/Services/ReCaptchaService.cs:80`,
  `OrchardCore.Users/Controllers/AccountController.cs:132`), and the model state is then invalid, so the password is never
  checked and the page is shown again (`AccountController.cs:136,221`). But `Views/Account/Login.cshtml:20` prints its error
  list with `asp-validation-summary="ModelOnly"`, which lists only errors that have no field name.
- **Confirm**: with the checkbox unticked, does the Log in page show any text such as "Failed to validate ReCaptcha"? In the
  clip `04-signin`, `signin-3` describes only what the code guarantees: the page loads again, the password field is empty and
  the user is not signed in. The Forgot password page, whose list uses `asp-validation-summary="All"`
  (`Views/ForgotPasswordForm.Edit.cshtml:8`), does show the message, and `pages-3` names it.

## 2. A failed check on the Register page does not stop the account

- **Checked**: `RegisterPOST` tests `ModelState.IsValid` before it does anything else, then calls `UserService.RegisterAsync`
  (`OrchardCore.Users/Controllers/RegistrationController.cs:64-66`). `RegisterAsync` calls the registration events, and the
  `ReCaptcha.Users` handler reports "Failed to validate ReCaptcha" through `ReCaptchaService.ValidateCaptchaAsync`
  (`OrchardCore.ReCaptcha/Users/Handlers/RegistrationFormEventHandler.cs:21-30`). It then creates the user and signs it in
  whatever the events reported (`OrchardCore.Users.Core/Services/UserService.cs:359-380`), and the controller redirects as soon
  as a user is returned (`RegistrationController.cs:69-81`). Read from the code alone, an unticked box adds an error to the model
  state after the controller has looked at it, and the account is created. The Log in, Forgot password and Reset password pages
  do not behave this way: their events run before the `ModelState.IsValid` test (`AccountController.cs:132-136`,
  `ResetPasswordController.cs:97-99,166-168`).
- **What the video says**: only that the Register page shows the checkbox. The opening card, the introduction and the problem
  slide do not say that registration is refused, and the problem slide leaves registration out. The title comes from the
  tracker and mentions registration; it cannot change in this video, and a person should decide whether it stays.
- **Confirm**: on a throwaway site, submit the Register form with the checkbox unticked (a throwaway user name, an address
  `@example.com`) and see whether the account is created. If it is, report it upstream, and review `features-3`, `pages-1` and
  `outro`, which describe the pages that get the checkbox. The feature's own description in `Manifest.cs` says it protects
  "login, register, forgot password and forms", and the documentation says only that the "login pages" are protected.

## 3. The label of the Re Captcha widget

- **Checked**: the widget is the content type `ReCaptcha` with the stereotype `Widget`
  (`OrchardCore.ReCaptcha/Forms/Migrations.cs:21-23`). No display name is set, so the type is created with its name split at
  the capital letters (`OrchardCore.ContentManagement.Abstractions/ContentDefinitionManagerExtensions.cs:12`, `Utilities/StringExtensions.cs:30`),
  which gives **Re Captcha**. The **Insert Widget** list prints that display name (`OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:68`),
  and the card header prints it after the item's own text (`:38`). The narration of `form-2`, the subtitle of chapter 06 and
  the description in `video.json` use "Re Captcha" for this reason. The other words, "ReCaptcha" and "ReCaptcha settings", are
  the labels of the features, the menu and the alert (`Manifest.cs`, `AdminMenu.cs`, `ReCaptchaPart.Fields.Edit.cshtml:7`).
- **Confirm**: that the entry in the **Insert Widget** list and the header of the new card read "Re Captcha" exactly. If a site
  shows another label (for example a translation), change the words in `form-2` and keep the word count between 8 and 45.

## 4. The Insert Widget button inside a Form widget

- **Checked**: a widget card has an **Insert Widget** button that inserts the new widget before that card
  (`OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:51`, `Assets/ts/flows.edit.ts:96`, on the `.widget-template` frame,
  `Views/ContentCard-FlowPart.Frame.cshtml:3`). It is shown unless `CanInsert` is false, and `CanInsert` defaults to true
  (`OrchardCore.ContentTypes.Abstractions/Shapes/ContentCardShape.cs:22`). The Form type's `FlowPart` has no list of contained
  types (`OrchardCore.Forms/Migrations.cs:38-40`), so every widget type is offered, `ReCaptcha` among them
  (`OrchardCore.Flows/Drivers/FlowPartDisplayDriver.cs:217-228`). The flow also has an **Add Widget** button at its bottom
  (`Views/FlowPart.Edit.cshtml:72`) that adds after the last widget.
- **Confirm**: that the plus button of the Button card is visible and clickable on screen (the source does not show whether a
  style hides it until the pointer is over the card), and that the new card lands between the Input card and the Button card.
  If only **Add Widget** works, the checkbox lands after the Button, and `form-2` and `form-4` ("just before the Button",
  "above the Button") must change.

## 5. Selectors inside Google's checkbox

The checkbox is drawn by Google's script into an `iframe`. The plan uses `iframe[title='reCAPTCHA']`,
`.recaptcha-checkbox-border` and `#recaptcha-anchor` with `aria-checked`; these come from the usual markup of Google's widget,
not from the Orchard Core source. Confirm them once with `Recorder probe`. The Orchard Core side is checked: the shape
renders a `div` with the class `g-recaptcha` and a `data-sitekey` attribute and adds Google's script to the foot of the page
(`OrchardCore.ReCaptcha.Core/Services/ReCaptchaShape.cs:46-54`).

## 6. The test keys

The two keys in `recording-plan.md` were read from the question about automated tests in Google's reCAPTCHA FAQ when the
plan was written. `keys-2` says only that Google publishes test keys and that a live site needs keys of its own (the
documentation says the keys come from a Google account, `ReCaptcha/README.md`, "Configuration"). That the published keys
exist comes from Google's page, not from the Orchard Core source: check the page again before recording. The narration does
not say what the test keys accept. In the code, an unticked box is refused before Google is called, because a blank answer
fails at once (`ReCaptchaService.cs:47`), so `signin-3` and `pages-3` do not depend on the test keys. The module reads the
checkbox type that these keys belong to: a `g-recaptcha` div with a site key, and a posted `g-recaptcha-response`
(`ReCaptchaService.cs:68-73`).

## 7. Notifications and layout of the Features page

- The notification texts "The feature ReCaptcha was enabled." and "The feature ReCaptcha Users was enabled." come from
  `Controllers/AdminController.cs:317` of the Features module with the feature name filled in; confirm the rendered text.
- The plan clicks **Enable** in each row. If the admin settings render the row actions as a menu, open it and choose
  **Enable** (`Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
- Confirm that the search `recaptcha` shows only the two ReCaptcha rows. The text a search matches is the feature name, its
  id and its description (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`).

## 8. Administrators sign in on the Log in page

`signin-1` says administrators sign in on this page too. The default login path is `Login` (`OrchardCore.Users.Abstractions/UserOptions.cs:5`, a private field set to `Login`)
and it is the cookie's login path (`OrchardCore.Users/Startup.cs:129`). Confirm by opening `/Admin` signed out: the browser
should be sent to the Log in page.

## 9. Pronunciation

The narration writes the product as "reCAPTCHA" and the feature and menu labels as the code spells them ("ReCaptcha",
"ReCaptcha Users", "reCaptcha"). The `spoken` map of the storyboard is empty. The narration writes "Http Request event" and "Http Redirect task", as the activities are labeled ("Http Request Event", "Http Redirect Task": `OrchardCore.Workflows/Http/Activities/HttpRequestEvent.cs:30`, `HttpRedirectTask.cs:32`); the `spoken` map reads "Http" as letters. Listen to a sample of these spellings, and of that one, with
`scripts/pronounce.py` before narrating, and add a `spoken` entry only if the voice mispronounces one.

## 10. Behavior the video leaves out on purpose

- With no keys saved, the Validate ReCaptcha workflow task keeps its **Valid** outcome: `ValidateCaptchaAsync` returns false
  but reports no error when the settings are missing (`ReCaptchaService.cs:60-65`), and the task only switches to **Invalid**
  in the error callback (`OrchardCore.ReCaptcha/Workflows/ValidateReCaptchaTask.cs:48-54`). The video does not say what the
  task does without keys. A person should confirm this on a running site and, if it holds, report it upstream.
- External sign-in providers skip the check on the Log in and Register pages
  (`Users/Handlers/LoginFormEventEventHandler.cs:25`). Not in the video.
- The Reset password page, reached from the emailed link, carries the checkbox too (`Drivers/ReCaptchaResetPasswordFormDisplayDriver.cs`).
  It needs a reset token from an email, so it is not recorded.
- The workflow chapter is two slides: building a workflow in the designer is a long task of its own, and the designer's layout
  was not read for this video. The Forms documentation describes the steps with older names ("NoCaptcha", "Validate NoCaptcha").

## 11. The widget and the workflow task need two other features

`why-3` and `features-2` say that the ReCaptcha feature adds a widget when the Forms feature is on and a workflow task when
the Workflows feature is on. The code registers each one only with both features: `OrchardCore.ReCaptcha/Forms/Startup.cs:10`
(`OrchardCore.Forms` and `OrchardCore.ReCaptcha`) and `Workflows/Startup.cs:9` (`OrchardCore.Workflows` and
`OrchardCore.ReCaptcha`). The Validate ReCaptcha task appears in the designer only on a site with Workflows enabled, and the
HTTP Request event only with HTTP Workflows Activities (`OrchardCore.Workflows/Manifest.cs`, `Http/Startup.cs:21`). The
recording site keeps Workflows and HTTP Workflows Activities disabled (Forms is enabled for the Contact page), and the video shows the task on slides only. A person should open the workflow designer
once on a scratch site to confirm that the task is listed as "Validate ReCaptcha" and shows the outcomes Done, Valid and
Invalid (`ValidateReCaptchaTask.cs:41-42`, `Views/Items/ValidateReCaptchaTask.Fields.Thumbnail.cshtml:1`).
