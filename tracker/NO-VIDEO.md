# Catalog entries without a video

Every entry of `catalog/features.json` is covered by at least one video of `tracker/videos.json`, or listed here with
the reason it has none. `python tools/tracker.py validate` reads the first column (the entry's id, in backticks).

## Obsolete feature ids

These ids are kept so that sites that enabled them keep working: each one only enables the feature that replaced it,
and the videos of that feature cover them. Their names end with "(Obsolete)" on the Features page.

| Entry | Reason |
| --- | --- |
| `OrchardCore.Search.Lucene` | Obsolete id: enables `OrchardCore.Lucene`, covered by `lucene-search` and `queries-lucene-elasticsearch`. |
| `OrchardCore.Search.Elasticsearch` | Obsolete id: enables `OrchardCore.Elasticsearch`, covered by `elasticsearch-search`. |
| `OrchardCore.Search.AzureAI` | Obsolete id: enables `OrchardCore.AzureAI`, covered by `azure-ai-search`. |
| `OrchardCore.Media.Azure.ImageSharpImageCache` | Obsolete id: enables `OrchardCore.Media.Azure.ImageCache`, covered by `media-azure-blob-storage`. |
| `OrchardCore.Media.AmazonS3.ImageSharpImageCache` | Obsolete id: enables `OrchardCore.Media.AmazonS3.ImageCache`, covered by `media-amazon-s3`. |

## Pages for the maintainers and contributors of Orchard Core

These pages are about building and releasing Orchard Core itself, not about building a site or a module with it.

| Entry | Reason |
| --- | --- |
| `guide:topics/publishing-releases` | The release checklist of the Orchard Core maintainers. |
| `guide:topics/red-hat-ecosystem-catalog-certification` | How the maintainers keep the Red Hat certification of the Docker image. |
| `guide:guides/upgrading-sdk-version` | How contributors upgrade the .NET SDK of the Orchard Core repository. |
| `guide:guides/assets-manager` | The tool that builds the scripts and stylesheets of the Orchard Core repository. |
| `guide:guides/assets-manager/NPM_ALIASING_GUIDE` | A detail of the same build tool, for contributors. |
| `guide:guides/gulp-pipeline` | The page itself says the pipeline was removed and is kept for history only. |

## Lists of links and reference lists

A video would only read the list aloud; the overview video (`orchard-core-overview`) and the video descriptions link
to these pages instead.

| Entry | Reason |
| --- | --- |
| `guide:guides` | The index of the guides; each guide has its own video. |
| `guide:getting-started/external-resources` | A list of links to tutorials made by the community. |
| `topic:libraries` | The list of third-party libraries Orchard Core uses, and their licenses. |
| `topic:branding` | The logo, colors and fonts of the Orchard Core brand; the videos apply them rather than explain them. |
