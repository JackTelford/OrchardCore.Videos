# Open questions: features-dependencies

Everything in the narration was read from the source of the Features module and of the manifests of the features it
names. The points below could not be seen on a running site. Confirm each on the recording site before recording; the
step that depends on it is named.

## Things the narration states that the source supports but a person must see

1. **Badges of Users Registration** (`read-2`, `read-4`, `why-1`). From the manifests: direct dependencies **Email** and
   **Users**; indirect dependencies **Roles Core Services** and **Settings**. Confirm that the row shows exactly these
   badges, in this order, and that the indirect ones are hidden until the three-dot button is used.
2. **Email has no other enabled dependent** (`disable-2`). The dialog must list only **Users Registration**. Check this
   in snapshot S1 before recording. If it lists more (for example **SMTP Email Provider**), the narration "here, Users
   Registration" is wrong and must change. The Email migration enables SMTP Email Provider when the site settings hold a default
   sender, or the configuration holds a default sender with a host or a pickup directory
   (`OrchardCore.Modules/OrchardCore.Email/Migrations/EmailMigrations.cs:19-37`,
   `OrchardCore/OrchardCore.Email.Abstractions/SmtpOptions.cs:37-46`); a fresh site with no such setting should not.
3. **The dependents are disabled with the feature** (`disable-3`). The code disables every enabled feature that depends on
   Email, directly or not (`OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:190-208`, forced from the
   page: `OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:214`). Confirm that, after the
   redirect, the **Users Registration** row is **Disabled**.
4. **Only the selected feature is named by the message** (`enable-2`, `disable-3`). The code passes the selected features,
   not the ones enabled or disabled with them, to the notification
   (`OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:247`, `313-322`,
   `OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:78-85`); the plural helper puts the count in `{0}`
   and the names in `{1}`
   (`OrchardCore/OrchardCore.Localization.Core/PortableObject/PortableObjectStringLocalizer.cs:88-98`). Confirm the
   wording: "The feature Users Registration was enabled." and "The feature Email was disabled."
5. **Dependencies that are already enabled stay as they are** (`enable-3`). From
   `OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:163-181`. Confirm that **Users** and **Settings**
   are unchanged, and that **SMTP Email Provider** stays **Disabled** after Email is enabled.
6. **On demand features** (`demand-1`, `demand-2`, `sum-1`). The tooltips say "automatically enabled based on
   dependency" and "automatically disabled based on dependency"
   (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.Buttons.SummaryAdmin.cshtml:19`, `25`), and the shell
   descriptor adds the dependencies of the enabled features back when it loads
   (`OrchardCore/OrchardCore.Infrastructure/Shell/ShellDescriptorManager.cs:74-89`). The sentence of `sum-1` "enabled
   while another feature needs it, and disabled when none does" rests on this. To confirm it, outside the capture:
   enable **Two-Factor Authenticator App Method**, check that **Two-Factor Authentication Services** (on demand) is
   **Enabled**, disable the first one, and check that the second is **Disabled** again. Then restore the snapshot.
7. **Roles Core Services is Enabled** on the recording site (`demand-2`). It follows from the Blog recipe enabling Users
   and Roles. Confirm it, and that the row appears once **Hide on demand** is cleared.
8. **Dependency not available** (left out of `sum-1` on purpose). The view shows a warning badge with the tooltip "This
   dependency is not available." and the feature has no Enable button when it has such a dependency
   (`OrchardCore.Modules/OrchardCore.Features/Views/FeatureEntry.DefaultTags.SummaryAdmin.cshtml:42-46`,
   `OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:141-151`). What was checked: a dependency
   counts as missing only when it is a loaded feature that the page does not list. Dependencies on ids that no
   installed module declares are dropped before the page sees them
   (`OrchardCore/OrchardCore/Extensions/ExtensionManager.cs:400-417`), the validation providers refuse a feature
   together with its dependencies
   (`OrchardCore/OrchardCore/Shell/DefaultTenantOnlyFeatureValidationProvider.cs:21-32`,
   `OrchardCore/OrchardCore/Shell/FeatureProfilesValidationProvider.cs:70-85`), and no manifest of this checkout depends
   on a theme or on the application feature, the kinds the page leaves out
   (`OrchardCore/OrchardCore.Features.Core/Services/FeatureService.cs:21-24`,
   `OrchardCore/OrchardCore/Shell/ApplicationFeatureValidationProvider.cs:57-67`).
   So the case looks unreachable with the modules of Orchard Core, and the narration does not mention it. A person who
   knows a site where the badge appears may add it back; it needs one sentence in `sum-1`, a third card in `slides.py`
   and the words "not available" in the subtitle of chapter 05 and in `video.json`.

## Selectors and behaviors to check with a probe

9. The dialog of the **Disable** button: `#confirmRemoveModal`, the title **Disable Feature**, a bulleted list built from
   the message (the message is put in the dialog as HTML,
   `OrchardCore.Themes/TheAdmin/Assets/js/TheAdmin/TheAdmin.ts:15-49`), the buttons `#modalOkButton` (**Yes**) and
   `#modalCancelButton` (**No**).
10. The addresses of the row buttons: `a[href$='/Features/OrchardCore.Users.Registration/Enable']` and
    `a[href$='/Features/OrchardCore.Email/Disable']` follow from the routes
    (`AdminController.cs:186`, `226`). As a fallback, the buttons have the ids `btn-enable-<id>` and `btn-disable-<id>`,
    where the dots of the feature id are probably replaced by underscores: check the generated id.
11. The row attribute used by the selectors, `data-filter-value`, holds "name, identifier, description" in this order
    (`OrchardCore.Modules/OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`). Check that
    `[data-filter-value^='Email OrchardCore.Email ']` matches only the Email row, and that the searches `Users
    Registration`, `Roles Core Services` and `OrchardCore.Features` each leave one row.
12. The **Visibility** filter is a Bootstrap Select drop-down: confirm the selectors of its button
    (`button[data-id='visibility-filter']`) and options, that clicking an option keeps the menu open, and that Escape
    closes it.
13. Tooltips over the inactive buttons: they are on the wrapper span, not on the button (selector: span[data-bs-toggle='tooltip']:has(> button[disabled]), since the badges of a row have tooltips too). Confirm that moving the recorder's
    cursor onto the wrapper shows the tooltip in the headless capture.
14. The success message after **Enable** or **Disable**: its position, and that it stays visible for the whole
    narration of `enable-2` and until `disable-3` has typed its search.
15. The search `email` (`enable-1`, `enable-3`, `disable-1`) matches rows in several categories (Communication,
    Notifications, Security). Check how far the page must be scrolled to see the **Users Registration** row, and whether
    the **Email** and the **Users Registration** rows are visible together in 864 CSS pixels. If they are, drop the scroll
    from the plan.
16. A fresh site shows the list layout (not the grid) and the row actions as buttons. If the layout selector next to the
    filters shows **Grid** selected, select **List** before recording.
17. The first sentence of `read-1` says "under Tools, Features". Check whether the admin menu shows the current entry on
    screen; if it does, ring it for that sentence.
