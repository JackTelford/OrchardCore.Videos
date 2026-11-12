# Documentation issues: OrchardCore.Lists Manifest (shown on Tools, Features)

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Lists feature description | OrchardCore.Lists Manifest (shown on Tools, Features) | 'Introduces a preconfigured container-enabled content type.' | The module creates no content type. Migrations only defines an attachable ListPart ('Add a list behavior.') and the contained-part index. A content type becomes a list only when an administrator adds the part to it. | OrchardCore.Modules/OrchardCore.Lists/Manifest.cs; Migrations.cs CreateAsync | lists-overview |
