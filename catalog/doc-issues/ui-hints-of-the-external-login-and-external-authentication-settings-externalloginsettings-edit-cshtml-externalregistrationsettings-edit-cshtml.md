# Documentation issues: UI hints of the External Login and External Authentication settings (ExternalLoginSettings.Edit.cshtml, ExternalRegistrationSettings.Edit.cshtml)

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Users.ExternalAuthentication | UI hints of the External Login and External Authentication settings (ExternalLoginSettings.Edit.cshtml, ExternalRegistrationSettings.Edit.cshtml) | Hints say that when 'Use a script to ...' is selected, none of the IExternalLoginEventHandler implementations (or those defined in modules) are triggered. | The controller invokes every registered IExternalLoginEventHandler for UpdateUserAsync and GenerateUserName whatever the settings; the script handler is just one of them, so other handlers still run. | src/OrchardCore.Modules/OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:609-618, 708-725 | external-login-settings |
