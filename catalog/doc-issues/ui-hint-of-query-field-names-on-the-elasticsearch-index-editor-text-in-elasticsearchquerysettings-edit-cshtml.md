# Documentation issues: UI hint of Query field names on the Elasticsearch index editor (text in ElasticsearchQuerySettings.Edit.cshtml)

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Elasticsearch | UI hint of Query field names on the Elasticsearch index editor (text in ElasticsearchQuerySettings.Edit.cshtml) | The hint says 'Leave it blank to query all searchable fields.' | When no query field is selected, the search returns no results (and logs 'No default query settings were configured'). | src/OrchardCore.Modules/OrchardCore.Elasticsearch/Views/ElasticsearchQuerySettings.Edit.cshtml:58; src/OrchardCore/OrchardCore.Elasticsearch.Core/Services/ElasticsearchService.cs:86-91 | elasticsearch-search |
