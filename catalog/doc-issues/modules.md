# Documentation issues: Modules

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| topic:Modules | reference/modules/Modules/README.md | Getting started: edit Startup.cs, add services.AddOrchardCms() to ConfigureServices, and replace the Hello World app.Run block in Configure with app.UseOrchardCore(). | The Orchard Core project templates generate a top-level Program.cs with builder.Services.AddOrchardCms() and app.UseOrchardCore(), and no Startup.cs in the web project. | src/Templates/OrchardCore.ProjectTemplates/content/OrchardCore.Templates.Cms.Web/Program.cs:23,46; src/OrchardCore.Cms.Web/Program.cs | module-create |
| topic:Modules | reference/modules/Modules/README.md | Additional frameworks: the wrappers work side by side, so you can have ASP.NET MVC and Nancy within your pipeline. | The source has no Nancy wrapper; the application targets are OrchardCore.Application.Cms.Core.Targets, Cms.Targets, Mvc.Targets and Targets only. | src/OrchardCore/ (folders OrchardCore.Application.*.Targets) | module-create |
