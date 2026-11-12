# Documentation issues: releases 2.0.0.md (Users Module, about token expiration)

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Users.ChangeEmail | releases/2.0.0.md (Users Module, about token expiration) | Says the tokens for password reset, email confirmation and email change are "sent through the email service", and that the ChangeEmailTokenProviderOptions token is valid for 15 minutes. | ChangeEmailController.Index (POST) calls UserService.ChangeEmailAsync, which generates the change-email token and passes it straight to UserManager.ChangeEmailAsync in the same call. No email is sent, nothing waits for the user to confirm the address, and the 15-minute lifespan never matters in practice. The address is saved when the form is submitted. | src/OrchardCore.Modules/OrchardCore.Users/Controllers/ChangeEmailController.cs:58-76; src/OrchardCore/OrchardCore.Users.Core/Services/UserService.cs:167-172; docs line src/docs/releases/2.0.0.md:598 | users-change-email |
