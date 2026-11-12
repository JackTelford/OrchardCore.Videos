# Documentation issues: Preview part description set in Migrations.cs

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Preview part description | Preview part description set in Migrations.cs | The part is for running the frontend decoupled from the admin. | The Preview part re-runs the same tenant's request pipeline for the pattern's path, so it cannot reach a separate application. | OrchardCore.ContentPreview/Migrations.cs:20; PreviewStartupFilter.cs | content-preview |
