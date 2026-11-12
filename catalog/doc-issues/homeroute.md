# Documentation issues: HomeRoute

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.HomeRoute | reference/modules/HomeRoute/README.md | Content items expose a 'Set as homepage' option (through the `Contents` feature), which points the home route at that item. | The 'Set as homepage' checkbox comes from the Autoroute part editor, not from the Contents feature. It is shown only when the type's Autoroute setting 'Show homepage options' is checked, the user has the SetHomepage permission ('Set homepage.'), and the item is not already the homepage. It is applied when the item is published. | src/OrchardCore.Modules/OrchardCore.Autoroute/Views/AutoroutePart.Edit.cshtml:55-77; src/OrchardCore.Modules/OrchardCore.Autoroute/Handlers/AutoroutePartHandler.cs:82; src/OrchardCore.Modules/OrchardCore.Autoroute/Views/AutoroutePartSettings.Edit.cshtml:44-46 | home-route |
