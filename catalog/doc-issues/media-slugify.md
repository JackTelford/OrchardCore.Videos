# Documentation issues: Media.Slugify

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Media Slugify | reference/modules/Media.Slugify/README.md | Different files can have the same slug, making it impossible to upload both without renaming one (for example 'The team (2020).jpg' and 'The Team 2020.jpg'). | Without Media TUS Uploads the Upload endpoint refuses the second file with 'A file with this name already exists in the current folder.' With Media TUS Uploads enabled, the file is saved under the first free name with -1, -2, ... added (for example the-team-2020-1.jpg). | src/OrchardCore.Modules/OrchardCore.Media/Startup.cs:731 (GetAvailableMediaFilePathAsync) and Endpoints/Api/UploadMediaEndpoint.cs:124 | media-uploads |
