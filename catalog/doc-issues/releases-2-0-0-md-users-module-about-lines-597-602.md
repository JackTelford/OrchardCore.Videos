# Documentation issues: releases 2.0.0.md (Users Module, about lines 597-602)

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Users.ChangeEmail | releases/2.0.0.md (Users Module, about lines 597-602) | The tokens for password reset, email confirmation and email change 'are sent through the email service', and ChangeEmailTokenProviderOptions is valid for 15 minutes. | The email-change token is generated and redeemed in the same call. Nothing is sent by email, the user is not asked to confirm, and the address is saved as soon as the form passes its checks. The 15-minute lifespan is real (ChangeEmailTokenProviderOptions) but never waited on. | OrchardCore/OrchardCore.Users.Core/Services/UserService.cs:167-170; OrchardCore.Modules/OrchardCore.Users/Controllers/ChangeEmailController.cs:73; OrchardCore/OrchardCore.Users.Core/Models/ChangeEmailTokenProviderOptions.cs:7-11 | users-change-email |
