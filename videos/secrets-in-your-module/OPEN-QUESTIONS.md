# Open questions: Secrets in Your Module

Everything in the narration was read from the source of Orchard Core at commit `92db4a83` (paths below are relative to
`src/`). Nothing was run: no site was started and no code was compiled while writing this video. What a person must
confirm before or while recording:

## On a running site (the two recorded clips)

1. **The tab after saving.** `recording-plan.md` (`editor-2`, `editor-5`) clicks the **SMTP** tab again after **Save**.
   The settings controller redirects to the same group
   (`OrchardCore.Modules/OrchardCore.Settings/Controllers/AdminController.cs:119-122`), but the source does not say
   whether the admin theme reopens the tab that was active (`OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs`
   marks the first tab `active`). Confirm what the page shows after the redirect.
2. **The scroll.** The row **Password** of the SMTP tab is below the fold at 1536 x 864 CSS pixels
   (`OrchardCore.Email.Smtp/Views/SmtpSettings.Edit.cshtml:147-152`, after nine other rows). The plan scrolls in steps
   of 200 pixels until `label[for$='_Password']` is inside the viewport. Confirm that at that point the whole editor
   (the list, the input and the hint, or the panel of the list of secrets) is visible too, and that the **User name**
   row is still visible above it; otherwise add one more step.
3. **The menu branch.** Confirm that opening `/Admin/Settings/email` expands **Settings**, **Communication** and
   highlights **Email** in the admin menu; otherwise click them before the capture starts.
4. **Enabling SMTP.** Confirm that **Email** and **SMTP Email Provider** both show **Disable** after the two clicks on
   **Enable**, and that the **SMTP** tab appears (`OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:88`).
5. **The placeholders.** Confirm on screen that the empty password input shows **Enter a new value** before the first
   save, and **The value is securely stored. Enter a new value to update it, or leave it blank to keep it.** after it
   (`OrchardCore.Secrets.Abstractions/TagHelpers/SecretInputTagHelper.cs:103-104`; the same strings in
   `OrchardCore.Secrets/Views/Shared/SecretInput.cshtml:31`).
6. **The Network delivery options.** The rows **Host name** to **Password** sit inside a panel that is rendered
   collapsed (`SmtpSettings.Edit.cshtml:42`). `OrchardCore.Email.Smtp/Assets/ts/smtp-settings.ts` opens the panel of the
   selected delivery method when the page loads (the compiled `smtp-settings.min.js` is loaded at
   `SmtpSettings.Edit.cshtml:187`). Confirm that the panel is open, with **Password** visible, within a second of
   clicking the **SMTP** tab, so that the capture does not show the panel opening.
7. **The secret list.** Confirm that the list in the **Use a secret** panel shows **None** and **Smtp.Password**, that
   the reload button has no text (only the title **Reload the secrets**), and that **New secret** is shown for the
   `admin` user (the button needs `ManageSecrets`, the reload button `ViewSecrets`; the Administrator role has both,
   `OrchardCore.Secrets/SecretsPermissionProvider.cs`).

## On the slides (code that was not compiled)

8. **The MyService example.** The code of `settings-1` to `driver-4`, `view-1`, `runtime-1`, `runtime-2` and `migrate-1`
   to `migrate-3` is the example of the Secrets documentation (`docs/reference/modules/Secrets/README.md`, "Adding Secret
   Support to Your Own Credentials"), trimmed with `// ...`. Each call was compared with the signatures in the source
   (`SecretInputViewModel`, `SecretInputUpdateContext`, `SecretInputExtensions.UpdateAsync`, `SecretValueExtensions`,
   `SecretMigrationExtensions.MoveToSecretAsync`, `SectionDisplayDriverBase`, `SiteDisplayDriver`) and with the SMTP
   provider, which follows the same pattern. Build the example in a throwaway module once before the slides are used.
   The `migrate-2` slide calls `CanMigrate()` and `AuthorizeAsync()` as the SMTP migration driver does
   (`OrchardCore.Email.Smtp/Drivers/SmtpSecretMigrationDisplayDriver.cs:54,102-106`); the documentation example has no
   permission check, and the two helpers are only described by a comment on the slide: `AuthorizeAsync()` wraps
   `IAuthorizationService` and the permission of the settings, `CanMigrate()` tests that a value is kept and no secret
   is referenced.
9. **The Startup slide (`driver-5`).** The documentation example does not show how the driver is registered. The slide
   uses `services.AddSiteDisplayDriver<...>()` as the SMTP provider does
   (`OrchardCore.Email.Smtp/Startup.cs`); its `using` lines were taken from the namespaces of the source
   (`OrchardCore.DisplayManagement.Handlers`, `OrchardCore.Modules`). Confirm that it compiles.
10. **The options signal (`runtime-3`).** The slide shows the code of the SMTP provider: the update request is made in
    `OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:191-208` (only when the model state is valid and a value
    changed), and the registration is `AddSignalOptionsChangeTokenSource<SmtpOptions>()` in
    `OrchardCore.Email.Smtp/Startup.cs:30`. The narration says that the registration is needed because the interface
    documents it (`OrchardCore.Abstractions/Options/IOptionsUpdateNotifier.cs:10-19`). Confirm in a throwaway module that
    an `IOptionsMonitor<T>` of the module sees the saved value after both steps.
11. **A credential outside site settings (`other-1`).** "The same steps apply to a credential kept anywhere else, such as
    a content part" comes from the documentation (the `WebhookPart` example). The abstractions do not depend on site
    settings, but the built-in modules only use the secret input in site settings. Confirm that the `WebhookPart` example
    builds and that its editor binds correctly in a content part editor before keeping that sentence.

## Narration

12. **Pronunciation.** Check each entry of `"spoken"` in `storyboard.json` with `scripts/pronounce.py` before narrating:
    "Orchard Core Secrets Abstractions", "Secret Input View Model", "Secret Migration", "Secret Migration Item Edit",
    "Get Secret Value Async", "Move To Secret Async", "Update Async", "Build Prefix", "secret input", "secret name".
    Also check "SMTP", "null" and "Data Protection".
13. **Length.** The validator counts an identifier such as `SecretInputViewModel` as one word, and the voice says four.
    The narration is 785 words (5:14 at 150 words per minute); after `narrate.py`, confirm in `durations.json` that the
    total is still close to 5 minutes (about 5:30 is expected).
