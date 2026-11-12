# Documentation issues: guides integrate-facebook-plugins

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Facebook.Widgets | guides/integrate-facebook-plugins/README.md | Navigate to https://localhost:5001/OrchardCore.Layers/Admin/Index to add the widget to the Footer zone. | The Layers admin is routed at Admin/Layers by an Admin attribute and is reached from the menu Design, Widgets; the page title is Widgets and Layers. The area/controller/action URL is not a route of that controller. | src/OrchardCore.Modules/OrchardCore.Layers/Controllers/AdminController.cs:86 | meta-social-plugins |
| OrchardCore.Facebook.Widgets | guides/integrate-facebook-plugins/README.md | "enable the Facebook Social Plugins Widgets Feature" (and the heading "Integrate Facebook Social Plugins"). | The feature is listed as Meta Social Plugins Widgets (category Meta); only the seven widget types keep the Facebook display names (Facebook Quote, and so on). | src/OrchardCore.Modules/OrchardCore.Facebook/Manifest.cs:34 | meta-social-plugins |
| OrchardCore.Facebook.Widgets | guides/integrate-facebook-plugins/README.md | Line 25: at the Footer Zone press Add Widget, select Facebook Quote, then (line 29) 'Give a title and publish the widget'. Line 23 calls the Footer a 'Layer'. The guide never mentions choosing a layer. | Footer is a zone. The editor has a required drop-down 'The layer', and publishing without a layer fails with 'Layer is missing field.' | src/OrchardCore.Modules/OrchardCore.Layers/Views/LayerMetadata.Edit.cshtml:21-34; Drivers/LayerMetadataWelder.cs:83-86 | meta-social-plugins |
