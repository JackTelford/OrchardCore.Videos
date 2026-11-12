# Documentation issues: getting-started README.md and src

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Setup screen labels | getting-started/README.md and src/docs/guides/create-cms-application/README.md | 'The theme recipe to use' (getting-started) and 'SQLite' (guide). | The field is labeled Recipe, and its menu lists recipes such as Blog and Agency. The database provider is named Sqlite. | src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml:129-135 (Recipe label); src/OrchardCore/OrchardCore.Data.YesSql/OrchardCoreBuilderExtensions.cs:53 (TryAddDataProvider name: "Sqlite") | create-cms-web-application |
