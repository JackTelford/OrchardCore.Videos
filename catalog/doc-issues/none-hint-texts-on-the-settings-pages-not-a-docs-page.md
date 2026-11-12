# Documentation issues: none (hint texts on the settings pages, not a docs page)

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Users.ExternalAuthentication | none (hint texts on the settings pages, not a docs page) | Hint of 'Use a script to set user roles, user claims and user properties based on external provider claims': none of the implemented IExternalLoginEventHandler will be triggered. Hint of 'Use a script to generate userName based on external provider claims': handlers defined in modules are not triggered. | The controller calls every registered IExternalLoginEventHandler on each external sign-in and when generating a username, whatever the two script options say. The script handler is one of them and returns early when its option is off. | OrchardCore.Users/Views/ExternalLoginSettings.Edit.cshtml:27-29; OrchardCore.Users/Views/ExternalRegistrationSettings.Edit.cshtml:68; OrchardCore.Users/Controllers/ExternalAuthenticationsController.cs:609-618, 708-725; OrchardCore.Users/Handlers/ScriptExternalLoginEventHandler.cs:60-70 | external-login-settings |
