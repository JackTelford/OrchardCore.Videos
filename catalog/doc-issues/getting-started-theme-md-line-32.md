# Documentation issues: getting-started theme.md (line 32)

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Themes admin page name | getting-started/theme.md (line 32) | The theme should be available in the 'Active themes' admin page. | The page is titled 'Themes' and sits under 'Design'. The string 'Active themes' appears nowhere in the source. | src/OrchardCore.Modules/OrchardCore.Themes/AdminMenu.cs:18-21; src/OrchardCore.Modules/OrchardCore.Themes/Views/Admin/Index.cshtml:12 | theme-from-html-template |
