# Open questions: secrets-manage

What the author could not confirm from the source or the documentation, and what a person must check on a running site
(the checkout at commit 92db4a83, Blog recipe, Secrets and SMTP provider enabled) before or while recording. The
narration states only what the source shows; each item below is either a spoken pronunciation or a screen detail the
source cannot prove.

## Pronunciation (check with `scripts/pronounce.py`, then add to `"spoken"` if needed)

- "X.509 Certificate" (`create-1`, `types-3`): `spoken` maps "X.509" to "X five oh nine"; confirm the result.
- "PEM-encoded certificate" (`types-3`): `spoken` maps "PEM" to "P.E.M." (the usual reading is the three letters); confirm the result.
- "Smtp.Password" is never spoken: the narration says "SMTP" only. The dotted secret names stay on screen.
- "RSA", "API", "SMTP", "SMS" are read correctly by the voice according to `references/storyboard.md`.

## Screen details to confirm

1. **The SMTP tab** (`settings-1`, `settings-2`, `settings-5`, `settings-6`). The source places the SMTP editor in a tab
   named "SMTP" (`OrchardCore.Email.Smtp/Drivers/SmtpSettingsDisplayDriver.cs:88`, `Content:5#SMTP`) beside a tab named
   "Providers" (`OrchardCore.Email/Drivers/EmailSettingsDisplayDriver.cs:59`). Confirm that the admin theme draws these
   two tabs with those names, which tab is selected on arrival and after saving, and whether the Password row is below the
   fold at 1536x864.
2. **The warning on the admin home page** (`review-5`). `SecretExpirationWarningFilter` adds the warning to the `Messages`
   zone of `Admin/Index` or of the Admin Dashboard's `Dashboard/Index` (`OrchardCore.Secrets/Filters/SecretExpirationWarningFilter.cs:87-99`).
   The plan keeps Admin Dashboard disabled (the Blog recipe does not enable it). Confirm the warning and the **Review
   secrets** link are visible on `/Admin` in that state.
3. **Highlighted badges** (`review-2`). The narration says only "highlighted". The classes are `text-bg-danger` for **Expired**
   and `text-bg-warning` for **Expires** (`OrchardCore.Secrets/Views/Admin/Index.cshtml:120-135`). Confirm that the admin
   theme makes them visibly different from the plain **Expires** and **Never expires** badges.
4. **The bootstrap-select filters** (`review-4`). The locators in the plan (`div.filter button.dropdown-toggle`,
   `.dropdown-menu.show a`) follow the usual bootstrap-select markup, not this repository's source. Confirm them with
   `Recorder probe`, and confirm that choosing an option reloads the page (`OrchardCore.Resources/Assets/js/list-management.js:497-509`).
5. **The sidebar** (`enable-1`, `enable-3`, `settings-1`). The ids of the menu entries are not in the source. Confirm that
   **Tools** opens to **Security** and that **Security** lists **Secrets**, **Migrate Secrets**, **Secret Stores** in that
   order, and that **Settings**, **Communication**, **Email** open in that order.
6. **Toasts last 5 seconds** (`enable-2`, `create-6`, `delete-2`, `settings-5`). `Notifier` success messages use
   `DismissalMilliseconds = 5000` (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`). Confirm on
   a take that each toast the narration mentions is on screen while it is mentioned; the plan says where to place the click.
7. **Enabling a feature** (`enable-2`). Enabling restarts the tenant. Confirm that the redirect to the Features page, with the
   message "The feature Secrets was enabled.", arrives within the narration of the step.
8. **Date format** (`create-6`, `review-1`, `review-2`). The badges print `secret.ExpiresUtc.Value.ToString("d")`
   (`Index.cshtml:121-133`) on the server. The Blog recipe does not enable the Localization feature, and
   `UseRequestLocalization` is called only by `OrchardCore.Localization/Startup.cs:62` and `OrchardCore.Setup/Startup.cs:87`, so
   the culture of the browser probably does not apply and the form follows the culture of the host process. Confirm the form on
   screen (the plan expects the `en-US` form, such as `11/2/2026`) and set the regional format of the account that runs the site
   if it differs.
9. **The Features page rows** (`enable-2`). The plan clicks `#btn-enable-OrchardCore_Secrets`, built from
   `id="btn-enable-@Html.GenerateIdFromName(featureId)"` (`OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
   The helper replaces the dot of the feature id by the default replacement character, which is an underscore. The default admin
   list layout is List with Buttons (`OrchardCore/OrchardCore.Admin.Abstractions/AdminListOptions.cs`). Confirm the id with `Recorder probe`.
10. **The ids of the SMTP fields** (`settings-2`). The display driver prefixes the ids (`smtp-settings.ts:1-3`), so the plan
    selects the fields by the end of their id, such as `[id$='_DefaultSender']`. Confirm them with `Recorder probe`.

## Behavior in the source that the narration avoids

- **RSA keys created in the admin.** `AdminController.CreateRsaKeySecret` (`OrchardCore.Secrets/Controllers/AdminController.cs:556-571`)
  fills `PublicKey` and `PrivateKey` but never sets `IncludesPrivateKey` (default `false`) or `KeySize` (default 2048, whatever
  size was chosen; the generated key itself has the chosen size). The OpenID code
  (`OrchardCore.OpenId/Secrets/Services/OpenIdSecretsOptionsConfiguration.cs:113`) skips the private key when the flag is
  `false`, and the decryption code (`OrchardCore/OrchardCore.Secrets.Core/Services/SecretEncryptionService.cs:145`) refuses
  such a key. The source settles this, so it is reported as a documentation issue (the documentation presents the admin page as
  a way to generate a key pair for these uses). The narration (`types-1`, `types-2`) says only that a key pair is generated, that
  the private key is never shown, and that the key pair cannot be changed, which the source confirms. It does not say what the
  key can be used for.
- **The Store row.** With one writable store, the **Store** row is a read-only text (**Database**); the documentation says the
  field appears when several writable stores are enabled. The narration of `create-4` describes the single-store case only,
  which is what the recorded site has.
