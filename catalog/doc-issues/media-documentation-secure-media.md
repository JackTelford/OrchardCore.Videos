# Documentation issues: Media documentation, Secure Media

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Root folder permission implied by folder permissions | Media documentation, Secure Media | View media content in the root folder is implied only by View media content in all folders. | Every first-level folder permission also implies the root permission. | src/OrchardCore.Modules/OrchardCore.Media/SecureMediaPermissions.cs:118-123,147 | media-secure-media |
