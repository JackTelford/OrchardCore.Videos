# Documentation issues: contributing contributing-code

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Visual Studio requirement (source-3) | contributing/contributing-code.md | 'Download Visual Studio 2022 (v17.8+)' to open the solution and run OrchardCore.Cms.Web. | The projects target net11.0 (DefaultTargetFramework and CommonTargetFrameworks), and OrchardCore.Cms.Web.csproj uses $(CommonTargetFrameworks). The Visual Studio 2022 v17.8 line cannot build a net11.0 target, so the requirement looks out of date. Not run, and the scanned tree has no global.json. The narration names no Visual Studio version. | src/OrchardCore.Build/TargetFrameworks.props:7,11; src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj:7 | test-drive-orchard-core |
| Visual Studio requirement for building the source | contributing/contributing-code.md | Download Visual Studio 2022 (v17.8+) (line 21), then launch the solution by clicking OrchardCore.sln (line 22). | The default and common target framework of the solution is net11.0, which Visual Studio 2022 17.8 cannot build. src/docs/reference/modules/Localize/README.md:500 names OrchardCore.slnx for the same step. | src/OrchardCore.Build/TargetFrameworks.props:7 and :11 (DefaultTargetFramework and CommonTargetFrameworks are net11.0) | test-drive-orchard-core |
