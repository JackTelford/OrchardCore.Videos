# Open questions: OpenID Connect Client

Everything in the narration was read in the Orchard Core source (the labels, the show and hide rules of the page, the
controller branches of the sign-in). Nobody has run the round trip between two tenants. A person must confirm the
following on a running site, by a full dry run from snapshot S0 of recording-plan.md, before recording.

## The round trip

1. **The button appears after Save.** After clip `03-connect` saves the settings, the login page must show the section
   "Use another service to log in" with the button "Example Identity". The button comes from the authentication scheme
   that `OpenIdClientConfiguration` registers once the tenant has restarted, and only when the settings are valid
   (`OrchardCore.OpenId/Configuration/OpenIdClientConfiguration.cs:39-49,115-138`). If the section is missing, look in the
   log for "The OpenID client settings are invalid".
2. **The redirect works over http with the `/idp` authority.** The client uses `http://localhost:5010/idp` as its
   authority, with Response Mode form_post, and the provider answers with a form posted to `/signin-oidc`. Confirm that
   the sign-in completes in the recorder's Chromium. If the browser drops the correlation cookie of the sign-in
   (an error page or a redirect back to the login page), the cause is the cookie rules for http, not the settings. In
   that case run the site over https for the recording (and change the Authority, the Redirect Uris of the provider and
   `VIDEO_BASE_URL` to match), or tell the author, because the narration does not mention the address scheme.
3. **The provider accepts the scopes.** The client asks for `openid`, `profile` and `email`. The application of the
   provider allows `email` and `profile` (plan, seed step 3). Confirm that the provider does not answer with an error
   about a scope.
4. **The email address arrives, and the registration page is short.** `signin-4` and `signin-5` rely on the email claim
   reaching the site (`ExternalLoginInfoExtensions.GetEmail` reads the `email` claim; the user information endpoint
   returns it for the `email` scope). With "Do not ask email address" and "Do not create local password" checked, the page
   Create a new account. must show only Username. If it also shows Email, the claim did not arrive: the narration of
   `signin-4` ("asks only for a username") and `reg-3` ("because the provider sends the email address") would be wrong.
5. **The account is created and linked.** After Register, the Users list must show `jamie.chen` with
   `jamie.chen@example.com` and no Unconfirmed badge (`account-1`). Signing in again with the button must go straight to
   the home page, with no registration page (`account-2`).
6. **No Authorization page.** The application uses Implicit consent, so the provider must not show its Authorization page.
   If the application is changed to Explicit consent, the page Authorization (Yes, No) appears between `signin-3` and
   `signin-4`, and the narration does not cover it.

## Screens whose exact look was not seen

7. **The admin menu.** The plan clicks Settings, then OpenID Connect, then Authentication Client; Settings, Security,
   Registration; and Access Control, Users. The labels are from the menu builders, but nobody has checked how the admin
   theme expands the menu (a click on the parent, or a hover). Adjust the clicks of `connect-1`, `reg-1` and `account-1`
   to what the menu does; the labels must stay as they are.
8. **The External Authentication options.** The options are added to the group "userRegistration" with the location
   `Content:5#External Authentication;5`. The Blog recipe does not enable User Registration, so no other editor shares the
   page, and the zone renders one group without a tab strip (`OrchardCore.DisplayManagement/Zones/ZoneShapes.cs:87,113`).
   The narration of `reg-1` says "External Authentication options" and does not say tab or section. Confirm on the page
   that the checkboxes are on screen without a click on a tab, and that the page holds a fifth checkbox ("Use a script to
   generate userName based on external provider claims") that the narration does not cover.
9. **The page after Enable.** After Click on Enable (`enable-2`), the redirect target of the Features page
   (`OrchardCore.Features/Controllers/AdminController.cs:287-310`) was not followed to the end. Check whether the search
   box keeps `OpenID Connect`, and whether the row is on screen, as the plan assumes.
10. **The Client Secret placeholder after Save.** `connect-8` says the field "shows that its value is securely stored".
    The text is the placeholder of the empty password input
    (`OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:103`). Confirm it is visible after the reload,
    that the collapsed field is open for the code flow, and that Secrets is not enabled.
11. **The length of the Save.** Saving the client settings restarts the tenant. Check that the redirect after the save
    lands on the settings page within the 20 seconds that the plan allows.

## Pronunciation

12. Check with `scripts/pronounce.py` how the voice says ClientId, CallbackPath, SignedOut CallbackPath, SignedOut
    Redirect Uri (the "Uri" part), "openid" and "OpenID". These are labels of the page and must be said as they are
    written, so the `spoken` map holds no spelling for them: none was tried. Its only entry drops the question mark of the
    label Store tokens? in `connect-5`, because the label stands in the middle of a sentence and the voice would raise
    its pitch; the captions keep the written label. Listen to that sentence once.

## Wording that rests on a hint of the page

13. `connect-3` and `connect-5` describe CallbackPath, SignedOut CallbackPath and SignedOut Redirect Uri with the hints
    of the page (`Views/OpenIdClientSettings.Edit.cshtml:45,54,66`). Whether signing out of the site also signs the person
    out of the provider was not checked, and the video does not say so.
