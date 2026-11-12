# Documentation issues: reference README.md (and mkdocs.yml nav)

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.HealthChecks | reference/README.md (and mkdocs.yml nav) | The reference index links the page as 'Health Check' (reference/README.md:124) and the mkdocs.yml nav entry is 'Health Check' (mkdocs.yml:299). | The module and its feature are named 'Health Checks' (Manifest Name = "Health Checks"). That name is shown in Tools &gt; Features, and the module's own page is titled 'Health Checks'. | src/OrchardCore.Modules/OrchardCore.HealthChecks/Manifest.cs:4 | health-checks |
