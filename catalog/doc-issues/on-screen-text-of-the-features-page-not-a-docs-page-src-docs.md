# Documentation issues: On-screen text of the Features page (not a docs page); src docs

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Lucene (feature description shown on the Features page) | On-screen text of the Features page (not a docs page); src/docs/reference/modules/Lucene/README.md does not mention it | The manifest description reads 'Creates Lucene indexes to support search scenarios, introduces a preconfigured container-enabled content type.' | A search of the Lucene module and Lucene.Core finds no code that creates a container-enabled content type, so the second half of the description appears stale. It is shown on screen in enable-1 and enable-2, and the narration does not repeat it. | OrchardCore.Modules/OrchardCore.Lucene/Manifest.cs:13 | lucene-search |
