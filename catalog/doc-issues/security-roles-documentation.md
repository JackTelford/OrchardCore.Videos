# Documentation issues: Security Roles documentation

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Anonymous role scope | Security Roles documentation | The Anonymous role represents non-authenticated users only. | RolesPermissionsHandler also applies Anonymous role permissions to signed-in users. | src/OrchardCore.Modules/OrchardCore.Roles/Services/RolesPermissionsHandler.cs:36-52 | media-secure-media |
