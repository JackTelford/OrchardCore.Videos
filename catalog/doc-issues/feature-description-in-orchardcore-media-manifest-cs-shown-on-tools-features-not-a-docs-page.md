# Documentation issues: Feature description in OrchardCore.Media Manifest.cs, shown on Tools, Features (not a docs page)

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Media.Tus | Feature description in OrchardCore.Media/Manifest.cs, shown on Tools, Features (not a docs page) | 'When enabled, replaces the default chunked upload mechanism with the TUS standard.' | The default upload is not chunked. The gallery passes max-upload-chunk-size 0, and the default XHR uploader sends each file in a single request without retry. The Options hint says 'If 0 or null, no chunked upload is used.' | OrchardCore.Media/Manifest.cs:88; Views/Admin/Index.cshtml:21; Assets/media-gallery/src/services/UppyFileUpload.ts:261-268 | media-uploads |
