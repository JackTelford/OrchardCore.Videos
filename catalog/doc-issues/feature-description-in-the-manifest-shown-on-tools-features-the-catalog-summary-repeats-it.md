# Documentation issues: Feature description in the manifest (shown on Tools &gt; Features); the catalog summary repeats it

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Lists | Feature description in the manifest (shown on Tools &gt; Features); the catalog summary repeats it | Introduces a preconfigured container-enabled content type. | The module creates no content type: its migration only defines an attachable ListPart (description Add a list behavior.), and a site attaches it to a type of its own (the Blog recipe does so on Blog). No AlterTypeDefinition exists in the module. | src/OrchardCore.Modules/OrchardCore.Lists/Manifest.cs:11; Migrations.cs:20-23 | lists-overview |
