# Open questions: Azure Communication Services Email

What was checked in the Orchard Core source at commit `92db4a83` and the documentation, and what a person must confirm
on a running site before or while recording. Nothing below is stated in the narration as a fact that the source does not
show; each item is a place where the source shows the behavior but a running site has not been looked at.

## To confirm on the running site

1. **The Providers tab right after the first Save** (`configure-7`). The source sets the default provider to Azure
   when none is set (`src/OrchardCore.Modules/OrchardCore.Email.Azure/Drivers/AzureEmailSettingsDisplayDriver.cs:140-143`) and
   asks the options to refresh after the save (`:149-154`). Confirm that the redirect after Save already lists
   **Azure Communication Services** in **Default provider**, and that the warning **No email providers are currently
   enabled.** is gone. If the list only fills after a second page load, reload the page once outside the capture before the
   step and tell the reviewer.
2. **Which tab is active after Save** (`configure-6`). The settings page redirects to itself after Save; the plan clicks the
   tab it needs, which works either way, but the first frame of `configure-6` depends on it.
3. **The toast of `enable-2`.** The source passes only the selected feature to the message, so the toast should read **The feature
   Azure Communication Services Email was enabled.** (`src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:247` and
   `:317`; the Email feature is enabled with it by `ShellDescriptorFeaturesManager.cs:163-180` but is not named). Confirm the wording, that
   the Email feature shows **Enabled** afterwards, and that the toast is still visible when the narration ends (success toasts hide after
   5 seconds). The narration only says that a message confirms that the feature was enabled.
4. **The search of `enable-1`.** The text `Azure Communication Services Email` should leave one row. The search matches the
   name, the id and the description of every feature (`FeatureEntryDisplayDriver.cs:20`); if another row stays, shorten the text
   typed and keep the narration (it names the feature, not the typed text).
5. **The selectors of the left menu.** The plan uses `#left-nav`, group `button`s, and `li.email a` and `li.emailtest a`
   (the class set by `AddClass("emailtest")` in `src/OrchardCore.Modules/OrchardCore.Email/AdminMenu.cs:30` and `AddClass("email")`
   at `:41`). The classes are read from `Themes/TheAdmin/Views/NavigationItem-admin.cshtml`, not from a rendered page. Confirm them
   with `Recorder probe`, and fall back to text locators if the markup differs.
6. **The input ids.** The plan uses `#To`, `#Subject`, `#Body`, `#From`, `#Cc`, `#Bcc`, `#ReplyTo` on the test page (from
   `asp-for` on `EmailTestViewModel`) and labels on the settings page. Confirm them with `Recorder probe`.
7. **The placeholder of the saved connection string** (`configure-6`). The text **The value is securely stored. Enter a
   new value to update it, or leave it blank to keep it.** must be fully readable in the field at 1536 px; if the field
   truncates it, the narration still holds (it says the placeholder says the value is securely stored), but the highlight
   should then cover the field and the recorder should widen nothing: tell the reviewer.
8. **The Secrets sentence** (`notes-3`). The source shows a drop-down with **User-specified value** and **Use a secret**
   when the Secrets feature is enabled (`src/OrchardCore.Modules/OrchardCore.Secrets/Views/Shared/SecretInput.cshtml:26-27`). It is not
   recorded; confirm it once by enabling Secrets on a scratch site.

## Not said in the video, on purpose

- **What Azure requires of the sender address** (for example that Azure Communication Services only sends from a domain
  connected to the resource) and where the connection string is found in the Azure portal. Neither is in the Orchard Core
  source or in its documentation page, which only links to Microsoft's email overview. Decide, after reading Microsoft's
  documentation, whether the video should say it; a person must verify it first.

## Pronunciation

- `Cc` and `Bcc` (step `test-3`) are spoken through the `spoken` map of `storyboard.json` as `C.C.` and `B.C.C.`. Check both with
  `scripts/pronounce.py` before narrating; the captions keep the written form.
