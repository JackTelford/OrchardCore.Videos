# Documentation issues: topics media

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Media (Media Field) | topics/media/media-picker.md | A Media Field is added to a content type from Content → Content Definition in the admin. | The menu is Design, Content Definition, Content Types; there is no Content Definition entry under Content. | src/OrchardCore.Modules/OrchardCore.ContentTypes/AdminMenu.cs:23-25 | media-field |
| OrchardCore.Media (Media Field) | topics/media/media-picker.md | The field settings table lists a setting called Media types, next to Multiple, Allow media text and Allow anchors. | The settings are Required, Multiple, Allow media text, Allow anchors, Hint and Allow all default media types; clearing the last shows one check box per file extension, grouped as image types, application types, audio types, text types and video types. No setting is labeled Media types. | src/OrchardCore.Modules/OrchardCore.Media/Views/MediaFieldSettings.Edit.cshtml:7-86 | media-field |
| Media Field settings: allowed file types | topics/media/media-picker.md | The field setting is called 'Media types'. | The settings are 'Allow all default media types' and a check box per extension, grouped as 'image types', 'application types' and so on. | src/OrchardCore.Modules/OrchardCore.Media/Views/MediaFieldSettings.Edit.cshtml:50-86 | media-field |
