# Documentation issues: Microsoft.Authentication and guides

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Tokens required by the app registration | reference/modules/Microsoft.Authentication and guides/microsoft-entra-id-integration | The reference page says to enable both Access tokens and ID tokens. The guide says to enable at least ID tokens. | The code sets no response type, so the flow comes from ASP.NET Core defaults. | OrchardCore.Microsoft.Authentication/Configuration/OpenIdConnectOptionsConfiguration.cs:24-42 | microsoft-entra-id-login |
