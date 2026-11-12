# Documentation issues: getting-started development-tools

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Sqlite database file name (data-1) | getting-started/development-tools.md | Line 16 says to open the `yessql.db` files under the tenant folders in `App_Data/Sites` with DB Browser for SQLite. | Setup names the file of a new Sqlite tenant `OrchardCore.db` (DatabaseName). `yessql.db` is used only for a tenant with no DatabaseName. `reference/modules/Data/README.md:28` and the ConfigurationSchema.json description already say `OrchardCore.db`. | src/OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs:178-180; src/OrchardCore/OrchardCore.Data.YesSql/SqliteHelper.cs:23-26 | development-tools, test-drive-orchard-core |
| guide:getting-started/development-tools | getting-started/development-tools.md | Line 17 says smtp4dev only needs to be installed 'via dotnet' and Orchard configured 'to use it as an SMTP server'. | The SMTP settings exist only when the feature 'SMTP Email Provider' is enabled. Saving with Enable SMTP Provider checked requires a Sender email address and, for the Network delivery method, a Host name. Enabling the only provider makes it the default provider. | src/OrchardCore.Modules/OrchardCore.Email.Smtp/Manifest.cs:4; Startup.cs:29; Drivers/SmtpSettingsDisplayDriver.cs:120-133 and :193-199 | development-tools |
