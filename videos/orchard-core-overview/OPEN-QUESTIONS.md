# Open questions: Orchard Core overview

Every label in the narration was read in the Orchard Core source (commit 92db4a83) and is listed with its file and line
in `recording-plan.md`. What the source cannot show is how a page looks on a running site. A person with the site
from `recording-plan.md` confirms the points below before recording; if one is wrong, the step it names is changed
(the narration or the actions), not worked around.

## The left menu (`tour-3`, `items-1`, `types-1`, `design-1`, `features-1`)

- **Checked:** the groups and entries are built by `AdminMenu.cs` of each module: Content > Content Items,
  Design > Content Definition > Content Types, Design > Themes, Design > Widgets, Tools > Features, Tools > Recipes.
  Entries with the same caption are merged (`OrchardCore.Navigation.Core/NavigationManager.cs`, `Merge`), and a group
  without a page is a button (`TheAdmin/Views/NavigationItemLink-admin.cshtml:21-28`).
- **The Blog recipe adds its own entries** to the admin menu (`blog.recipe.json:1188`, the step AdminMenu): "Blog",
  "Main Menu", and a "Content" group with Content Items, Article, Page and Taxonomy.
- **Also checked:** the menu is "Focused" by default (`AdminSettings.MenuBehavior`): on `/Admin` every group is
  collapsed, on another page the group that holds the page is open, and opening a group closes the others
  (`TheAdmin/Assets/js/TheAdmin/sidebar.ts`, `applyNavStateFromPreferences`, `applyFocusedAccordion`). The recorder
  starts each clip in a new browser context (`Scene.cs:105`), so no state carries over from one clip to the next.
- **To confirm:** (1) clicking Content, Design and Tools expands them (and does not open a page); (2) the merged
  Content group still shows "Content Items", and its other entries from the Blog recipe (Article, Page, Taxonomy) do not
  distract from the narration; (3) the selectors `#left-nav >> text="..."` of the plan find exactly one
  element each, or the plan must name the first one.

## The Manage Content list (`items-1`)

- **Checked:** the page title is "Manage Content" (`Views/Admin/List.cshtml:17`); only listable types are listed
  (`Services/DefaultContentsAdminListFilterProvider.cs:182`), so the Blog Post is not in the list (it is not listable:
  `blog.recipe.json`, type BlogPost) while the Blog and the Article "About" are. The default layout is "List", so a
  row shows badges for the type, the status ("Published" or "Draft") and the modified time, not column titles.
- **To confirm:** the About row is there with a button labeled Edit, its status reads "Published", and
  `.list-group-item:has-text("About")` finds that row.

## The editor (`items-2`, `items-3`)

- **Checked:** the editor of the Article type shows Title, Permalink, the body, then the fields Subtitle and Banner
  Image (the order is the part positions in `blog.recipe.json`). The action bar adds Publish, Save Draft, Delete
  (`Drivers/ContentsDriver.cs:110-115`) and Preview (`OrchardCore.ContentPreview/Views/ContentPreview.Button.cshtml:13`).
- **To confirm:** Publish, Save Draft and Preview are all visible together when the editor is scrolled to the bottom
  (the action bar may be sticky), and Preview is a link labeled "Preview", not an icon.
- **Checked since:** the label of the body editor is the display name of the part, which is the part name without
  "Part": "HtmlBody" (`OrchardCore.Html/Views/HtmlBodyPart-Wysiwyg.Edit.cshtml:22`, `ContentTypePartExtensions.cs:7-26`,
  `ContentPartSettingsExtensions.cs:54-64`; no module or recipe sets another display name), with the hint "The body of
  the content item." The narration of `items-2` names "HtmlBody" for this reason. **To confirm:** the label reads
  "HtmlBody" on screen, and the editor has the Subtitle and Banner Image fields below it.

## The Content Type editor (`types-2`, `types-3`)

- **Checked:** headings "Fields" and "Parts" and the buttons "Add Field", "Add Parts" and "Add Named Part"
  (`OrchardCore.ContentTypes/Views/Admin/Edit.cshtml:88,144,137,201,202`); the fields of Article are Subtitle and Banner
  Image (`blog.recipe.json`, part Article).
- **Parts list, read in the source:** each part is labeled with its display name, which is the part name without
  "Part" (`ContentTypePartExtensions.cs:7-26`, `ContentPartSettingsExtensions.cs:54-64`, `Views/Admin/Edit.cshtml:153`),
  so the Article type lists Title, Autoroute, HtmlBody and Article, in the positions of `blog.recipe.json`, each with
  the description its module sets (`OrchardCore.Title/Migrations.cs:33`, `OrchardCore.Autoroute/Migrations.cs:23`,
  `OrchardCore.Html/Migrations.cs:34`); the row Article is the part of the type itself and carries the hint "Contains the
  fields for the current type". The narration of `types-2` names Title, Autoroute and HtmlBody, and says "Autoroute for
  the permalink" because that part shows the field Permalink in the editor. **To confirm:** the four labels and
  their order on screen, and the order of the two fields: Subtitle and Banner Image both have the position "0" in
  `blog.recipe.json`, so the order comes from a stable sort of the stored order of the fields, not from the positions
  (`Views/Admin/Edit.cshtml:44-55`). If Banner Image comes first, swap the two highlights, and keep the narration, which
  names both fields, so it still fits.
- **To confirm:** how far to scroll so Fields, Parts and both buttons are visible, and that `a[role='btn-edit-Article']`
  finds the Edit button of the Article row.

## Themes, widgets and layers (`design-1` to `design-3`)

- **Checked:** the Themes page shows a card per theme with "This is the current Site theme" or "This is the current
  Admin theme" on the current ones, and "Make Current" on every other card (`ThemeEntryDisplayDriver.cs:19-27`). The
  Widgets and Layers page lists the zones set by the recipe (Content and Footer), the widget Footer on the layer
  Always, and the layers Always and Homepage.
- **To confirm:** at least one card other than the two current ones is listed, so "Make Current" can be shown (the
  host project references the themes Agency, Coming Soon, The Default Theme and Safe Mode too:
  `OrchardCore.Application.Cms.Targets.csproj`); the Footer widget appears in the Footer zone card with the badge
  "Always", and the Content zone card holds no widget (the narration says only that the Footer zone holds one widget).

## The Features page (`features-1`, `features-2`)

- **Checked:** the page groups features by category, shows the description and the dependencies of each, a status
  ("Enabled" or "Disabled") and the buttons Enable and Disable; the search narrows the list while you type
  (`features-index.ts`, `input` handler) and matches the name, the id and the description. The recipe does not
  enable Workflows.
- **Also checked:** no module the Blog recipe enables depends on Workflows (no other Manifest.cs lists
  "OrchardCore.Workflows" as a dependency), so the feature stays disabled; Workflows is not on demand and not always
  enabled, so the default filters keep its row. Typing "Workflows" also keeps the rows HTTP Workflows Activities,
  Session Workflows Activities and Timer Workflows Activities, because the search matches name, id and description, so
  the plan finds the row by its exact name.
- **To confirm:** the row named exactly "Workflows" reads "Disabled" and has a button "Enable"; the id of that button is
  `btn-enable-OrchardCore_Workflows` (the id comes from `Html.GenerateIdFromName`, whose dot replacement is the ASP.NET
  Core default "_"; if it is another character, the plan's alternative selector by row still works); the first screen of
  the page has a row with dependency badges. The first category is the first one in alphabetical order that still has a
  row after the default filters hide the on-demand and always-enabled features (`AdminController.cs`, `Features`; the
  page script hides a category whose rows are all hidden), so the plan takes the first visible heading and row, and
  which one that is must be read on the running site.

## Claims taken from the documentation

These come from the documentation pages of the tracker entry and could not be tested in the source with a running site:

- the framework is distributed on its own, separate from the CMS, and a framework-only application has none of the
  CMS features (`src/docs/README.md`, "Building Software as a Service (SaaS) solutions with the Orchard Core Framework");
- decoupled and headless sites, and the glossary's definition of a tenant ("an independent subsite with its own URL")
  and the isolation of content, users and themes between tenants (`src/docs/README.md`, "Scalability";
  `reference/glossary/README.md`). The source confirms that a tenant has its own URL host or prefix, its own database
  settings and its own recipe (`OrchardCore.Tenants/Views/Admin/Create.cshtml`, `Shared/_TenantInfo.cshtml`,
  `Shared/_TenantDatabaseInfo.cshtml`, `Shared/_TenantRecipes.cshtml`) and that the Tenants menu exists on the Default
  tenant only (`OrchardCore.Tenants/AdminMenu.cs:24`);
- the deployment sentences (`reference/modules/Deployment/README.md`, first paragraph and "Import a package"; and
  `reference/modules/Deployment.Remote/README.md`, first paragraph, for the package sent to another site).

## Speech

`narrate.py` was not run. The voice reads "ASP.NET" through the shared `SPOKEN` list. The storyboard adds one entry of
its own, written the way the storyboard format asks (dots between the letters, no spaces): "HtmlBody" is spoken
"H.T.M.L. Body". "HtmlBody" is the on-screen label of `items-2` and `types-2`, a camel-case name the voice may run
together. **To confirm:** listen to the spelling with `scripts/pronounce.py` before the narration is generated, keep it
if the letters and the gap are clear, change it or remove it if the voice reads the label correctly without it. Also
listen to "CMS", "GraphQL", "REST", "multi-tenant" and "Autoroute" (the last is an on-screen label of `types-2`); add
an entry only after `pronounce.py` confirms a spelling.
