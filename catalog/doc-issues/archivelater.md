# Documentation issues: ArchiveLater

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.ArchiveLater | reference/modules/ArchiveLater/README.md | When editing a published item, set the scheduled archive date and time in the editor; use the cancel option in the editor to cancel (no button names, no mention of publishing). | The Archive Later and Cancel Archive Later buttons post submit.Publish, so selecting either also publishes the current editor content (the Publish Later buttons post submit.Save and only save a draft). | src/OrchardCore.Modules/OrchardCore.ArchiveLater/Views/ArchiveLaterPart.Edit.cshtml:12,19; src/OrchardCore.Modules/OrchardCore.Contents/Controllers/AdminController.cs:546-551,938-980 | publish-later-archive-later |
| OrchardCore.ArchiveLater | reference/modules/ArchiveLater/README.md | A background task finds published items whose scheduled archive time has passed and unpublishes them. | The schedule is indexed only while the saved version is both published and the latest version; an item whose published version has a newer unpublished draft is not archived until that draft is published. The docs do not say so. | src/OrchardCore.Modules/OrchardCore.ArchiveLater/Indexes/ArchiveLaterPartIndexProvider.cs:50; Services/ScheduledArchivingBackgroundTask.cs:32 | publish-later-archive-later |
