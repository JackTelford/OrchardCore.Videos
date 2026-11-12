# Documentation issues: src OrchardCore

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Tenant RecipeName setting | src/OrchardCore/OrchardCore/ConfigurationSchema.json (RecipeName, line 52; not a src/docs page) | 'The name of the recipe preselected when the tenant is set up.' | The recipe is preset, not preselected. When RecipeName is set, the setup screen hides the recipe picker, and the POST uses the configured recipe and ignores any posted value. | src/OrchardCore.Modules/OrchardCore.Setup/Controllers/SetupController.cs:115,236; src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml:124 | configuration-appsettings |
