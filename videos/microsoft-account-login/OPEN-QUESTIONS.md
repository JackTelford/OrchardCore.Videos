# Open questions: Microsoft Account sign-in

What was checked in the Orchard Core source (the commit named in `catalog/SOURCE.md`), and what a person must confirm on
a running site or at Microsoft before the video is recorded. The narration holds only what the source or the
documentation states; nothing below is claimed in it.

## To confirm on a running site (selectors and timing)

1. **Element ids.** The ids in `recording-plan.md` are derived, not seen: `ISite_MicrosoftAccountSettings_AppId`,
   `_AppSecret`, `_CallbackPath` and `_SaveTokens` follow from the display driver prefix
   (`src/OrchardCore/OrchardCore.DisplayManagement/Entities/SectionDisplayDriverBase.cs:87` gives `ISite.MicrosoftAccountSettings`)
   and from `SecretInputTagHelper.cs`. Run `Recorder probe /Admin/Settings/OrchardCore.Microsoft.Authentication.MicrosoftAccount`
   with a script that lists `input` ids, and fix the plan if they differ. The label text (`AppId`, `Secret Key`,
   `CallbackPath`, `Store tokens`) is certain; the ids are not.
2. **Admin menu.** `NavigationItem-admin.cshtml` makes a group without a page of its own a `button` and a group with
   a page a link plus a `button.nav-toggle`. Confirm that **Tools**, **Settings**, **Security** and **Authentication**
   open on one click, and whether `data-menu-behavior="focused"` closes the other groups. Fall back to `Ctrl+K`
   (`#adminQuickNavigationInput`) only if the clicks cannot be recorded.
3. **Features search.** The search text of a row is its name, its id and its description
   (`src/OrchardCore.Modules/OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`), compared as a lower-case
   substring (`Assets/ts/features-index.ts`). In the source, only Microsoft Account Authentication holds the words
   `Microsoft Account`: the Microsoft Entra ID row does not, so one row is left. Confirm on the running site that one row
   is shown, that the search works again after the page reloads in `enable-2`, and that the default visibility filters
   (Hide on demand, Hide always enabled) do not hide the row.
4. **Messages.** Confirm how the two success messages look and how long they stay on screen: **The feature Microsoft
   Account Authentication was enabled.** and **Site settings updated successfully.**
5. **Restart time.** Enabling the feature and saving the settings both restart the tenant. Measure the pause; if it is
   longer than the narration of `enable-2` or `settings-6`, use `OffCameraAsync` as the plan says.
6. **Login layout.** `Login.cshtml` places the **Log in** form and **Use another service to log in** in two `col-md-6`
   columns. Confirm that at the recording viewport (1536 px wide) they are side by side, as `login-1` says ("Beside the
   Log in form").
7. **Register button label.** `LinkExternalLogin.cshtml` and `RegisterExternalLogin.cshtml` name their submit button
   **Register**, also on the page that only links an existing account. The slides and narration do not name it.

## To confirm with the voice

8. **Pronunciation.** No `spoken` entry was added. Check with `scripts/pronounce.py` how the voice says `AppId`,
   `CallbackPath`, `/signin-microsoft` (narration of `how-2`) and `sign-in`, and add a
   `spoken` entry for any that is misread. A person must confirm each new spelling.

## To confirm at Microsoft (not verifiable from the Orchard Core source)

9. **Creating the app.** The narration says "an app for your site in the Microsoft developer portal", "Application ID",
   "secret key" and "callback path". The first three follow the hints of the settings page ("The Application ID found
   on the Microsoft developer portal parameters." and "The Application secret key found on the Microsoft developer
   portal parameters.") and the documentation. The documentation (`src/docs/reference/modules/Microsoft.Authentication/README.md`
   lines 14-16) also says to use the "Application Registration Portal" (the page links to a Microsoft address), to "add the
   web platform", and to "allow the implicit flow". The handler that does the sign-in is
   `Microsoft.AspNetCore.Authentication.MicrosoftAccount`, which is not part of the checkout, so none of these steps
   could be checked. The video leaves the portal's address, the platform and the flow out. A person should check how an
   app is registered at Microsoft today and decide whether the video needs a line about it.
10. **Which accounts sign in.** The title of the video says "Personal Microsoft Accounts" (the tracker's title). The
    narration says only "Microsoft account", and repeats the documentation's note (README lines 11-12) that Microsoft
    Entra ID authentication with a multi-tenant app is the choice for work or school accounts as well. Whether this
    feature also accepts work or school accounts depends on the app registration at Microsoft and was not verified.
11. **The email claim.** `return-2` says a local user with the same email address is offered **Link your account**.
    In the source this needs `info.GetEmail()` to return an address
    (`ExternalAuthenticationsController.cs:148-175`) and unique emails to be required, which is the default
    (`OrchardCore.Users/Models/IdentitySettings.cs:24`). That Microsoft returns the email claim to this handler was not
    verified. Test it with a Microsoft account of a person's own, outside the recording.
12. **The redirect.** `login-2` does not click the button. The source shows that the click posts to the server and
    returns a challenge to the `Microsoft` scheme (`ExternalAuthenticationsController.cs:74-83`); the page Microsoft then
    shows for a sample AppId was not seen. If the maintainer wants the click on camera, a person must first see what
    Microsoft answers to a sample AppId, check that the page holds no private data, and decide where the clip ends.

## Documentation differences

The differences between the documentation and the code are reported in the result of the work (docIssues), not here.
