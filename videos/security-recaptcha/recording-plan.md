# Recording plan: security-recaptcha

How to record "reCAPTCHA: Keep Bots Out of Sign-In, Registration and Forms". Read it from top to bottom once, then follow
the clips in the order given. Source references (`file:line`) are relative to the Orchard Core checkout the site runs
from (`src/OrchardCore.Modules/...` and `src/OrchardCore/...`). A selector written between backticks stands for
`scene.Page.Locator(...)`. "Checkbox" below means the reCAPTCHA checkbox, which sits in an `iframe` that Google's script
adds to the page.

## The site it needs

- **Site**: a local `OrchardCore.Cms.Web`, on `http://localhost:5010`, with its own `ORCHARD_APP_DATA` outside both
  repositories, provisioned by Auto Setup (see `references/recording.md`, "The site to record"): tenant `Default`, recipe
  `Blog`, site name **Orchard Core**, time zone `America/Los_Angeles`, database `Sqlite`.
- **Administrator**: user name `admin`, email `admin@example.com`, password `<password>` (chosen outside git, passed in
  `VIDEO_PASSWORD`; the same value is typed in the clip `04-signin`).
- **Outbound calls**: this video is the exception to the dead proxy of `references/recording.md`. Do **not** set
  `HTTP_PROXY`/`HTTPS_PROXY` for this site. The browser loads Google's reCAPTCHA script, and the site posts each answer to
  Google's verification address; both addresses are the defaults in `src/OrchardCore/OrchardCore.ReCaptcha.Core/Constants.cs`
  (lines 5 and 7), and they must be reachable from the machine. No other outbound call happens: no mail, SMS or payment
  provider is configured (the Email feature that Users Registration pulls in stays unconfigured, and the clips send no
  email).
- **External service stand-in**: Google's published reCAPTCHA test keys, from the question about automated tests in
  Google's reCAPTCHA FAQ. They always pass, and the widget shows a warning that it is for testing only. That warning is
  expected on screen. Use exactly these two values, and no other key in any clip:
  - Site Key: `6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI`
  - Secret Key: `6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe`
- **Host configuration**: leave `OrchardCore:ReCaptcha` unset in `appsettings.json`, user secrets and the environment.
  `ConfigureReCaptchaSettings()` is not called by `OrchardCore.Cms.Web`, so the values saved in the admin are the ones used
  (`Extensions/OrchardCoreBuilderExtensions.cs:10`).

### Seed, done outside the capture (snapshot A)

The Blog recipe already enables Users, Flows, Widgets and Features. Then, signed in as `admin`:

1. **Tools, Features** (`/Admin/Features`): enable **Users Registration** (`OrchardCore.Users.Registration`), **Users Reset
   Password** (`OrchardCore.Users.ResetPassword`) and **Forms** (`OrchardCore.Forms`). Users Registration and Users Reset
   Password also enable the Email feature as a dependency; do not configure it. Do **not** enable ReCaptcha or ReCaptcha
   Users: the clip `02-features` does that on camera. Leave Workflows and HTTP Workflows Activities disabled.
2. **Settings, Security, Reset Password** (`/Admin/Settings/userResetPassword`): check **Allow the users to reset their
   password** (`Views/ResetPasswordSettings.Edit.cshtml:8` in the Users module), click **Save**. Without it,
   `/ForgotPassword` answers "not found" and the Log in page has no "Forgot your password?" link
   (`Controllers/ResetPasswordController.cs:71` and `Drivers/ForgotPasswordLoginFormDisplayDriver.cs:21`).
3. **Settings, Security, Registration**: leave every setting as it is.
4. **Create the Contact page**: open `/Admin/Contents/ContentTypes/Page/Create` and fill it in:
   - **Title**: `Contact`.
   - In the flow of the page, use **Add Widget** (the plus button) and choose **Form**. In the Form widget, **Title**:
     `Contact`; leave **Action**, **Method** and **EncType** as they are.
   - Inside the Form widget, use its own **Add Widget** button and choose **Input**: **HTML Name** `Name`, **Type**
     `Text`, **Placeholder Text** `Enter your name`, **Label option** `Standard`, **Label Text** `Name`.
   - Inside the Form widget, use **Add Widget** again and choose **Button**: **Text** `Send`, **Type** `Submit`. The Button
     must be the last widget of the form.
   - Click **Publish**. The page is served at `/contact` (the Blog recipe's Page pattern slugifies the title). Open
     `/Admin/Contents/ContentItems/Page`, open the Contact page, and note its content item id from the address bar: it is
     `<contact-id>` in the clip `06-form`.
   - Labels: `Flows/Views/FlowPart.Edit.cshtml:72`, `Forms/Views/Items/FormPart.Fields.Edit.cshtml:6`,
     `Forms/Views/Items/FormInputElementPart.Fields.Edit.cshtml:5`, `InputPart.Fields.Edit.cshtml:37`,
     `ButtonPart.Fields.Edit.cshtml:5`.
5. Sign the recorder in (`Recorder setup`), so `state.json` exists. Stop the site and copy its `App_Data` aside as
   **snapshot A**, together with `state.json`. Start the site again.

Fake data in this video: the user `admin` (`admin@example.com`), the address `reader@example.com` typed in the clip
`05-pages`, the page title Contact, the input `Name`, the button `Send`. Nothing else is typed.

## Order of the clips, and what changes the site

Record the clips in the order they play: `02-features`, `03-keys`, `04-signin`, `05-pages`, `06-form`. Each one starts from
what the previous one left, except that `04-signin` and `05-pages` only need the keys from `03-keys`.

**`04-signin` and `05-pages` are recorded signed out.** A refused sign-in does not remove the cookie of a session that is
already signed in, so a signed-in take would not show a refusal, and `signin-4` would prove nothing. In `Scenes.cs`, start
these two scenes with a state path that does not exist (for example `Path.Combine(Root, "signed-out.json")`): `Scene.StartAsync`
then opens the browser without a saved sign-in (`Scene.cs`, the `StorageStatePath` line). Do not sign out of the saved session
to get this: a sign-out could end the session in `state.json`. The other clips keep using `state.json`.

| Clip | Needs | Changes the site | Restore before a new take |
|---|---|---|---|
| `02-features` | snapshot A | enables ReCaptcha and ReCaptcha Users | snapshot A |
| `03-keys` | snapshot B (A plus the two features enabled) | saves the Site Key and the Secret Key | snapshot B |
| `04-signin` | snapshot C (B plus the keys saved), signed out | nothing: the sign-in lives in the scene's own browser context, and `state.json` stays as it is | none |
| `05-pages` | snapshot C, signed out | nothing | none |
| `06-form` | snapshot C, signed in with `state.json` | publishes the Contact page with the Re Captcha widget | snapshot C |

After a good take of `02-features`, stop the site and copy `App_Data` and `state.json` aside as **snapshot B**; after a good
take of `03-keys`, do the same for **snapshot C**. Restore a snapshot with the site stopped.

Check each contact sheet from `review.py` for an error message, an empty list, or a banner about the email provider on a
captured page. If such a banner shows, write it in `OPEN-QUESTIONS.md` and record the clip again after dismissing it.

## Slides and cards

- `intro` (opening card): the card with the title "reCAPTCHA in Orchard Core" and its subtitle. Nothing to record.
- `why-1`: slide "Bots send requests on their own", three cards: sign-in pages, password pages, forms.
- `why-2`: slide "How the check works", three cards: the visitor ticks the checkbox, your site asks Google, Google replies.
- `why-3`: slide "Two features in Orchard Core", two cards: ReCaptcha (keys, a widget for Forms, a task for Workflows),
  ReCaptcha Users.
- `workflow-1`: slide "From the form to a workflow", three cards, the first two bright: Form widget, HTTP Request event,
  Validate ReCaptcha task.
- `workflow-2`: slide "The outcomes of Validate ReCaptcha", three cards: Valid, Invalid, Done.
- `outro` (closing card): the card with "docs.orchardcore.net". Nothing to record.

## Clip `02-features`

**Starts on** `/Admin` (the dashboard), signed in, admin menu collapsed. Start the capture there.

- `features-1`: click **Tools** in the admin menu (the group opens), then **Features** (`ClickAndWaitForNavigationAsync`).
  Wait until `#search-box` is visible. Type `recaptcha` into `#search-box` (`TypeAsync`; the list filters while you type,
  do not press Enter). Wait until the summary line reads "Showing 2 of" followed by the total. *Visible at the end*: the
  **Features** page, the search box holding `recaptcha`, and only two rows under the **Security** heading: **ReCaptcha**
  (`OrchardCore.ReCaptcha`) and **ReCaptcha Users** (`OrchardCore.ReCaptcha.Users`), both with the **Disabled** badge and an
  **Enable** button. If another row also matches, keep going and write it in `OPEN-QUESTIONS.md`.
  Sources: `OrchardCore.Features/AdminMenu.cs:26-27`, `Views/Admin/Features.cshtml:27`,
  `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:15`, `Drivers/FeatureEntryDisplayDriver.cs:20` (the text a search matches).
- `features-2`: click **Enable** in the ReCaptcha row, `[data-filter-value^='ReCaptcha OrchardCore.ReCaptcha '] a:has-text('Enable')`
  (`ClickAndWaitForNavigationAsync`; the first request after enabling can take a few seconds, wait up to 30 seconds for the
  text below). The trailing space in the selector keeps it from matching the ReCaptcha Users row. *Visible at the end*: the
  **Features** page with the green notification "The feature ReCaptcha was enabled." Source: `Controllers/AdminController.cs:317`.
- `features-3`: the search box is empty again after the reload. Type `recaptcha` into `#search-box`, wait until two rows show,
  and move the pointer over the **ReCaptcha Users** row, whose dependencies show as the badges **ReCaptcha** and **Users**.
  Click **Enable** in that row, `[data-filter-value^='ReCaptcha Users OrchardCore.ReCaptcha.Users '] a:has-text('Enable')`
  (`ClickAndWaitForNavigationAsync`). *Visible at the end*: the notification "The feature ReCaptcha Users was enabled."
  Source: `Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:15-19`, `Controllers/AdminController.cs:317`.
- `features-4`: type `recaptcha` into `#search-box` once more and wait until two rows show. Highlight the **Enabled** badge of
  each row (`HighlightAsync` on `[data-filter-value^='ReCaptcha OrchardCore.ReCaptcha '] .badge:has-text('Enabled')` and on
  `[data-filter-value^='ReCaptcha Users OrchardCore.ReCaptcha.Users '] .badge:has-text('Enabled')`, no caption), then hold. *Visible at the end*:
  both rows with the **Enabled** badge and a **Disable** button, rings around the badges. Source:
  `Views/FeatureEntry.DefaultMeta.SummaryAdmin.cshtml:9`.

## Clip `03-keys`

**Starts on** `/Admin`, signed in, admin menu collapsed. Start the capture there.

- `keys-1`: click **Settings** in the admin menu, then **Security**, then **reCaptcha**
  (`ClickAndWaitForNavigationAsync` on the last click). Wait until the **Site Key** field is visible. Highlight the Site Key
  field and the Secret Key field (`HighlightAsync`, no caption). *Visible at the end*: the settings page (breadcrumb
  "Settings") with the fields **Site Key** and **Secret Key**, both empty, their hints ("Your Google's reCaptcha Site Key."
  and "Your Google's reCaptcha Secret Key."), and the line "Get your reCaptcha at" followed by a link. Do not click the
  link. Sources: `OrchardCore.ReCaptcha/AdminMenu.cs:26-28`, `Views/ReCaptchaSettings.Edit.cshtml:4,8,13,17,23`.
- `keys-2`: clear the highlights (`ClearHighlightsAsync`). Type the Site Key value from the list above into the field
  labelled **Site Key** (`GetByLabel("Site Key")`), then the Secret Key value into **Secret Key** (`GetByLabel("Secret Key")`).
  Press Tab after the second value. *Visible at the end*: both fields filled, the Secret Key readable as plain text (its input
  type is text, `Views/ReCaptchaSettings.Edit.cshtml:15`).
- `keys-3`: click **Save** (`button.save`, `ClickAndWaitForNavigationAsync`). Wait for the text "Site settings updated
  successfully." *Visible at the end*: the settings page again, the green notification "Site settings updated successfully.",
  and both fields still filled. Sources: `OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`,
  `Controllers/AdminController.cs:119`.
- `keys-4`: highlight both fields again (`HighlightAsync`, no caption), then hold. No other action. *Visible at the end*: the
  same page with a ring around each field.

After this clip, stop the site and copy `App_Data` and `state.json` aside as snapshot C.

## Clip `04-signin`

**Starts on** `/Login`, opened before the capture (`OpenAsync("/Login")`) in a scene that is **signed out** (see "Order of
the clips"). Wait until the checkbox is drawn:
`iframe[src*='recaptcha']` is visible and the frame contains `.recaptcha-checkbox-border`. Start the capture then.
Selectors inside Google's `iframe` do not come from the Orchard Core source: confirm them once with `Recorder probe`.

- `signin-1`: highlight the widget, `div.g-recaptcha` (`HighlightAsync`, no caption). *Visible at the end*: the **Log in** page
  with the fields **Username or email address** and **Password**, **Remember me**, the checkbox below them, the **Log in**
  button, and the links "Forgot your password?" and "Register as a new user". Sources:
  `OrchardCore.Users/Views/Account/Login.cshtml:27`, `Views/LoginFormCredentials.cshtml:7,13,26`, `Views/LoginForm.Edit.cshtml:9`,
  `Views/LoginFormForgotPassword.cshtml:2`, `Views/LoginFormRegisterUser.cshtml:2`,
  `OrchardCore.ReCaptcha/Drivers/ReCaptchaLoginFormDisplayDriver.cs:26-29` (the widget sits after the credentials).
- `signin-2`: clear the highlights. Type `admin` into **Username or email address**, type the password (`<password>`, read from
  `VIDEO_PASSWORD`; the field shows dots) into **Password**, and do **not** tick the checkbox. Click **Log in**
  (`form.auth-form button[type=submit]`, `ClickAndWaitForNavigationAsync`). *Visible at the end*: the result of the click, which
  is the page described in `signin-3`.
- `signin-3`: no action, hold. *Visible at the end*: the **Log in** page again, **Username or email address** still holding
  `admin`, **Password** empty, the checkbox unticked, and the visitor still signed out. The page is expected to show no message
  about the checkbox (see `OPEN-QUESTIONS.md`, question 1). Source: `Controllers/AccountController.cs:130-136,221`,
  `Drivers/LoginFormDisplayDriver.cs:18-28`.
- `signin-4`: type `<password>` again into **Password**. Click the checkbox: `FrameLocator("iframe[title='reCAPTCHA']")` then
  `.recaptcha-checkbox-border` (if the title differs, use `iframe[src*='recaptcha/api2/anchor']`). Wait until the frame's
  `#recaptcha-anchor` has `aria-checked="true"`. Click **Log in** (`ClickAndWaitForNavigationAsync`) and wait until the address
  is no longer `/Login`. *Visible at the end*: the site's home page at `/`, signed in as `admin` (the page opened without a
  return address, so the sign-in lands on the home page, `Controllers/AccountBaseController.cs:34-41`). The signed-in state
  is only in this scene's browser context: do not save it to `state.json`.

## Clip `05-pages`

**Starts on** `/Register`, opened before the capture in a scene that is **signed out** (`OpenAsync("/Register")`, see "Order
of the clips"). Wait until the checkbox is drawn, as in the clip `04-signin`. Start the capture then. Never submit this form:
it would create an account, and the code does not refuse an unticked box there (`OPEN-QUESTIONS.md`, question 2).

- `pages-1`: highlight the widget, `div.g-recaptcha` (`HighlightAsync`, no caption), then hold. *Visible at the end*: the page
  with the heading "Register a new account", the fields **Username**, **Email address**, **Password** and **Confirm
  password**, the checkbox, and the **Register** button. Sources: `OrchardCore.Users/Views/RegisterUserForm.Edit.cshtml:3,16`,
  `Views/RegisterUserFormIdentifier.cshtml:4,10,16,22`, `OrchardCore.ReCaptcha/Drivers/RegisterUserFormDisplayDriver.cs:26-29`.
- `pages-2`: clear the highlights. `NavigateAsync("/ForgotPassword")` and wait until the checkbox is drawn. Type
  `reader@example.com` into **Username or email address**. Do **not** tick the checkbox. Click **Submit**
  (`ClickAndWaitForNavigationAsync`). *Visible at the end*: the heading "Forgot password?" with a list of errors above the field
  (the result of the click). Sources: `Views/ForgotPasswordForm.Edit.cshtml:3,8,16`, `Views/ForgotPasswordFormIdentifier.cshtml:4`.
- `pages-3`: highlight the error list (`HighlightAsync` on `div.validation-summary-errors`, or on the list `ul` inside the
  form, no caption) and pause 3 seconds so the text "Failed to validate ReCaptcha" is readable, then clear the highlight. If
  **Username or email address** is empty, type `reader@example.com` again. Click the checkbox (as in `signin-4`) and wait for
  `aria-checked="true"`. Click **Submit** (`ClickAndWaitForNavigationAsync`). *Visible at the end*: the page with the heading
  "Forgot Password Confirmation" and the text "Please check your email to reset your password." No email is sent, because no
  user has this address. Sources: `OrchardCore.ReCaptcha.Core/Services/ReCaptchaService.cs:80`,
  `Controllers/ResetPasswordController.cs:97-104`, `Views/ResetPassword/ForgotPasswordConfirmation.cshtml:5-6`.

## Clip `06-form`

**Starts on** `/Admin/Contents/ContentItems/<contact-id>/Edit` (the edit page of the Contact page), opened before the capture
(`OpenAsync`). Scroll so that the Form widget card and the card of its Button widget are fully visible, with room above the
Button card. Start the capture then.

- `form-1`: highlight the Form widget card (`HighlightAsync` on the `.widget-editor` card whose header text contains "Contact"),
  then hold. *Visible at the end*: the edit page of the Contact page with the Form widget and, inside it, the cards of the
  Input widget (HTML Name `Name`) and the Button widget (Text `Send`). Source: `OrchardCore.Flows/Views/ContentCard-FlowPart.Edit.cshtml:29,38`.
- `form-2`: clear the highlights. In the header of the Button widget card, click the plus button titled **Insert Widget**, then
  click the entry **Re Captcha** in the list that opens (the narration says "Re Captcha": the type `ReCaptcha` has no display
  name, so its label is the name split at the capital letter; see `OPEN-QUESTIONS.md`, question 3). Wait until
  `.widget-editor[data-content-type='ReCaptcha']` is visible. *Visible at the end*: a new widget card, headed "Re Captcha",
  between the Input card and the Button card. The widget is inserted before the card whose button was clicked
  (`OrchardCore.Flows/Assets/ts/flows.edit.ts:96`, on the `.widget-template` frame of the card,
  `Views/ContentCard-FlowPart.Frame.cshtml:3`). The card shows **Insert Widget** because `CanInsert` is true unless set
  otherwise (`OrchardCore.ContentTypes.Abstractions/Shapes/ContentCardShape.cs:22`). If the list shows another label, say it
  in `OPEN-QUESTIONS.md` and tell the author, so the narration of `form-2` and the subtitle of chapter 06 follow it. If the
  card has no **Insert Widget** button, click **Add Widget** at the bottom of the Form widget instead, and tell the author:
  the widget then lands after the Button, and the narration of `form-2` and `form-4` ("just before the Button", "above the
  Button") must change.
  Sources: `Views/ContentCard-FlowPart.Edit.cshtml:51`, `Views/FlowPart.Edit.cshtml:72`,
  `OrchardCore.ContentManagement.Abstractions/ContentDefinitionManagerExtensions.cs:12`.
- `form-3`: highlight the green alert inside the new card (`HighlightAsync` on
  `.widget-editor[data-content-type='ReCaptcha'] .alert-success`, no caption). Do not click its link (it opens a new tab).
  *Visible at the end*: the alert "ReCaptcha settings are configured.", with "ReCaptcha settings" as a link, inside the new
  card. Source: `OrchardCore.ReCaptcha/Views/Items/ReCaptchaPart.Fields.Edit.cshtml:7,16`.
- `form-4`: clear the highlights. Click **Publish** (`button[name='submit.Publish']`, `ClickAndWaitForNavigationAsync`; the page
  is opened without a return address, so one button has this name, `Content.PublishButton.cshtml:5-8`). Then
  `NavigateAsync("/contact")` and wait until the checkbox is drawn. *Visible at the end*: the Contact page of the site (public
  side) with the form: the **Name** label and input, the checkbox, and the **Send** button below it. Sources:
  `OrchardCore.Contents/Views/Content.PublishButton.cshtml:7`, `OrchardCore.ReCaptcha/Views/Items/ReCaptchaPart.cshtml:3-6`.

## Checks for private data

After every take, with the contact sheets of `review.py` and then OCR at one frame per second and at every scene change:

- The only address on screen is `admin@example.com` (if the user menu shows it) and `reader@example.com`. The only user name
  is `admin`.
- The password typed in `04-signin` shows as dots; the real value must not appear on any frame. Do not tick "Show password"
  (the eye button next to the field) at any time.
- The only reCAPTCHA keys on screen are the two test keys of this plan. Never type a key of a real Google account, not even to
  try it, and never leave one in the site's `App_Data`.
- The home page shown at the end of `signin-4` is the Blog recipe's demo content: confirm that no real person's name shows.
- Delete the copies of `App_Data` and `state.json` after the video is published. They are never committed.
