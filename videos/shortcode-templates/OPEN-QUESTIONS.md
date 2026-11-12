# Open questions: Shortcode Templates

What was checked in the source and what a person must confirm on a running site before recording. The narration states
only what the source shows; the items below are about the recording and about behavior that reading the source cannot
prove. Commit: the Orchard Core source at `92db4a83`.

## Checked in the source

- The labels of the narration and of the plan: Design, Shortcodes (`OrchardCore.Shortcodes/AdminMenu.cs:18-19`); Add Shortcode
  Template (`Views/Admin/Index.cshtml:25`); Name, Hint, Usage, Categories, Default Shortcode Value, Content, Save
  (`Views/Admin/Create.cshtml`); Available Shortcodes, Filter, Add (`Views/ShortcodeModal.cshtml`,
  `Views/ShortcodeDescriptor-Button-Actions.SummaryAdmin.cshtml`); Insert Shortcode
  (`OrchardCore.Resources/Assets/trumbowyg/js/trumbowyg.shortcodes.js`); Tools, Features and Enable
  (`OrchardCore.Features`); All Shortcode Templates (`Views/Items/AllShortcodeTemplatesDeploymentStep.*.cshtml`); Manage
  shortcode templates (`OrchardCore.Shortcodes.Abstractions/ShortcodesPermissions.cs`, security critical;
  `Permissions.cs`: Administrator and Editor).
- The values a template receives (`Args`, `Content`, `Context`): `Services/TemplateShortcodeProvider.cs`. `Context` holds
  `ContentItem` for the Html Body part, the Html field, the Markdown Body part and the Markdown field
  (`OrchardCore.Html.Core/Services/HtmlDisplayService.cs`, `OrchardCore.Markdown/Drivers/*`).
- The templates are asked before the shortcodes registered in code: `ShortcodeTemplatesStartup.Order` is -10 with the comment
  "Register this first so the templates provide overrides for any code driven shortcodes" (`Startup.cs`), and
  `ShortcodeDescriptorManager` lets the first registered provider win.
- Shortcodes are processed before the HTML is sanitized, and the Html Body part sanitizes by default
  (`HtmlDisplayService.cs`, `HtmlBodyPartSettings.cs`: `SanitizeHtml = true`, label "Sanitize Html").
- The recipe step is named `ShortcodeTemplates` and the deployment step writes the same step (`Recipes/ShortcodeTemplateStep.cs`,
  `Deployment/AllShortcodeTemplatesDeploymentSource.cs`). Property names are read without regard to case
  (`JOptions.Base.PropertyNameCaseInsensitive`), and the slide uses the names of the C# properties.
- The four setup recipes (Blank, Agency, Blog, Coming Soon) enable the Shortcode Templates feature, which is why the plan
  disables it before `02-enable`.

## To confirm on a running site (nothing here changes the narration unless it fails)

1. **Liquid output was not executed.** The `primary` and `display_text` templates were read, not run. Confirm that
   `[primary text="Read this first."]` and `[primary]Read this first.[/primary]` both print the text in a `span` with the class
   `text-primary`, and that `[display_text]` prints the title of the item. The named arguments were tested against the
   Shortcodes library from a local NuGet cache (version 1.4.0, the version Orchard Core pins was not read): a name is found with
   any case, and `[primary Hello]` gives a positional argument that `Args.Text` does not see. The same cache gave a
   second check, with Fluid.Core 3.0.0-beta.9 (again not the version Orchard Core pins): inside `capture`, `{{ Args.Text }}`
   is encoded and the captured markup is not encoded a second time, so `<b>x</b>` as a value is printed as text. Confirm
   on the site that `[primary text="<b>x</b>"]` shows the tags as text, since `template-2` says values cannot add markup.
2. **CodeMirror editors.** `#Usage` and `#Content` are CodeMirror editors started with `autoCloseTags: true`. The plan sets their
   value with `setValue`, which always works. To show typing instead, confirm whether typing `>` inserts a closing tag in the
   `liquid` mode; if it does, type the template without the closing tags and check the result line by line.
3. **Layout of the Create page.** The fields are stacked at full width, the preview card stays hidden until a key is pressed, and
   the 10-row Usage editor pushes Content below the fold. The scroll distances of the plan (450) are a start; set them on the
   site.
4. **Categories control.** It is a vue-multiselect made by a package that is not in the source (`@orchardcore/bloom`,
   `shortcode-categories-editor`). Confirm the selector (`#shortcodeCategories .multiselect`), that `HTML Content` is listed
   (the category of the `image` shortcode of Media), and the fallback (type, then Enter).
5. **Admin menu.** The plan assumes the Design menu opens in place and lists Shortcodes. If the admin menu behaves
   differently, open `/Admin/Shortcodes` with `NavigateAsync` instead. The route is `Shortcodes`
   (`AdminController.cs`, `[Admin("Shortcodes", ...)]`).
6. **Features page.** Confirm the selector of the Enable link of the row, the notification text after enabling ("The feature
   Shortcode Templates was enabled."), and that the Design menu lists Shortcodes in the page that follows.
7. **Row actions and bulk actions.** The Edit and Delete buttons are in an admin-list "actions" zone that the admin settings
   lay out as buttons or as a menu. Confirm which one the site shows, and what ticking the row checkbox shows.
8. **The editor and the picker.** Confirm the class of the toolbar button (`trumbowyg-insertShortcode-button`), that the caret is
   in the new paragraph after `Control+End` and `Enter`, that the picker inserts the default value at the caret, and where
   the browser goes after Publish (it should return to the Content Items list).
9. **The page.** Confirm that `text-primary` is clearly visible in the Blog theme, and that the shortcode leaves no
   brackets and no empty paragraph on `/about`.
10. **Pronunciation.** The `spoken` map of the storyboard uses plain words: `Args` is said "Arguments", `ContentItem` "Content
    Item", `ShortcodeTemplates` "Shortcode Templates". Listen to the narration of `idea-3`, `more-1` and `deploy-1`. The
    narration also says "the sanitize HTML filter", "the raw filter" and "primary".
11. **Names are saved in lower case.** `ShortcodeTemplatesManager.UpdateShortcodeTemplateAsync` stores the name with
    `ToLower()`. The video uses a lower-case name and does not say so.
12. **Override by name.** `more-2` says a template with the name of a code shortcode, such as `image`, replaces it. Checked:
    `ShortcodeTemplatesStartup.Order` is -10 (`Startup.cs`), the startups are sorted by ascending `Order`
    (`ShellContainerFactory.cs`), and `ShortcodesProcessor` of Shortcodes 1.4.0 uses the first provider that returns a
    value (a two-provider test). Confirm on a scratch site that a template named `image` changes the output of
    `[image]foo.jpg[/image]`, and that the picker lists one `image` card. Do this outside the recording: it changes the site.
13. **Deployment step.** `deploy-1` says the step exists when Deployment is enabled: the registration has
    `RequireFeatures("OrchardCore.Deployment", "OrchardCore.Shortcodes.Templates")` (`Startup.cs`). Not run: confirm that
    the step is offered when a step is added to a plan (Tools, Deployments, Plans,
    `OrchardCore.Deployment/AdminMenu.cs:18-20`).

## Documentation page of the video

The tracker entry and the catalog name `src/docs/reference/modules/Security/Permissions.md` as the page of this feature (it
lists the permission of the feature). The feature is documented in `src/docs/reference/modules/Shortcodes/README.md`, in the
section "Shortcode Templates". `video.json` keeps the tracker's page in `embeddedIn` and links to both pages, the Shortcodes
page first. A maintainer decides where the marker `<!-- video:shortcode-templates -->` goes.
