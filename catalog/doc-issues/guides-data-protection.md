# Documentation issues: guides data-protection

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| guide:guides/data-protection | guides/data-protection/README.md | Application restarts: locally stored keys may be lost, invalidating existing cookies and tokens. | The default key ring is a folder under App_Data (PersistKeysToFileSystem), which survives a restart. Keys are lost only when that folder is not durable, for example on an ephemeral disk. | src/OrchardCore/OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:565-577 | data-protection |
