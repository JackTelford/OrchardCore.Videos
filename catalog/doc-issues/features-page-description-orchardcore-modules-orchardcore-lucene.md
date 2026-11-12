# Documentation issues: Features page description (OrchardCore.Modules OrchardCore.Lucene

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Lucene | Features page description (OrchardCore.Modules/OrchardCore.Lucene/Manifest.cs), not a docs file | Creates Lucene indexes to support search scenarios, introduces a preconfigured container-enabled content type. | Nothing in the Lucene module or the Lucene core library creates a container-enabled content type; the module only registers the index source, drivers, queries, recipe steps and deployment steps. | OrchardCore.Modules/OrchardCore.Lucene/Manifest.cs:13 | lucene-search |
