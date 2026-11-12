# Documentation issues: getting-started README

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Setup screen fields | getting-started/README.md | Fields: 'The Sql provider to use. Ex: SqLite', time zone example '(+01:00) Europe/Paris', 'The name of the admin user', 'The theme recipe to use'. | The labels are 'What type of database to use?' (value 'Sqlite'), 'Default Time Zone' (option text '(GMT&lt;standard offset&gt;) &lt;zone id&gt; (&lt;offset now&gt;)'), 'User Name' under 'Super User', and 'Recipe'. | src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml:130,159,176,218-222; src/OrchardCore/OrchardCore/Modules/Model/TimeZone.cs:25 | create-cms-web-application |
| Setup screen time zone | getting-started/README.md | The timezone of the site. Ex: `(+01:00) Europe/Paris`. | The Default Time Zone option text is '(GMT+01:00) Europe/Paris (+01:00)': the standard offset with a GMT prefix, the zone id, then the current offset. | src/OrchardCore/OrchardCore/Modules/Model/TimeZone.cs:25 (ToString); src/OrchardCore.Modules/OrchardCore.Setup/Views/Setup/Index.cshtml:25-30,160 | create-cms-web-application |
