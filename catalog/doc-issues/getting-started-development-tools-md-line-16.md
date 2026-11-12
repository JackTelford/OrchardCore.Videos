# Documentation issues: getting-started development-tools.md (line 16)

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| topic:Data | getting-started/development-tools.md (line 16) | Open the yessql.db files under the tenant folders in App_Data/Sites to browse the tables and documents. | Setup names a new SQLite file OrchardCore.db (an empty DatabaseName falls back to yessql.db only for tenants set up without a name), and the Data page documents OrchardCore.db. | src/OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs:178-181; src/OrchardCore/OrchardCore.Data.YesSql/SqliteHelper.cs:21-26; src/docs/reference/modules/Data/README.md:28 | data-and-yessql |
