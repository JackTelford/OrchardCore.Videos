# Documentation issues: Seo module editor views

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Hint under Open Graph Title and Twitter Title (UI text, not a docs page) | Seo module editor views | The hint reads 'The Title is used by default, then the Page Title, or can be overridden here.' This suggests the content item's title comes before the Page Title. | The order is the field itself, then the Page Title, then the content item's title. The narration follows the code. | src/OrchardCore.Modules/OrchardCore.Seo/Views/SeoMetaPartOpenGraph.Edit.cshtml:15 and SeoMetaPartTwitter.Edit.cshtml:7, against SeoMetaPartHandler.cs:103-110,141-148 and SeoMetaSettingsHandler.cs:179-182,233-236 | seo-meta |
