# Open questions: Admin Templates

What was read from the source (`OrchardCMS/OrchardCore` at commit `92db4a83`) but not seen on a running site. A person
confirms each point while recording, and corrects the narration if the site disagrees.

1. **A template that cannot be parsed** (`good-1`, "the shape shows the parser's error text instead of your markup").
   Read in `OrchardCore.Liquid/Services/LiquidTemplateManager.cs:98-114`: a source that fails to parse is replaced by a
   template that holds the parser errors (the error text is parsed once more, and if that fails too, the code has no
   template to render: `LiquidTemplateManager.cs:102-105`). `TemplateController.ValidateModelAsync`
   (`Controllers/TemplateController.cs:394-422`) checks that the name and the content are filled in and that no other
   template has the name, and never calls `ILiquidTemplateManager.Validate`.
   To confirm: on a copy of S1, save an admin template named `VisitSiteNavbarItem` whose content is `{% if %}`, open any
   admin page, and look at what the top bar shows in place of the link. A template that breaks the top bar breaks it on
   every admin page, the Admin Templates list included, so do not try to delete it through the admin: restore S1 after
   the test.
2. **A template wins over the view of the same name** (`why-3`). Read in
   `OrchardCore/OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs:349-411`: for every name tried,
   the binding resolvers (the templates) are asked before the view files. Clip `04-create` shows it, since the
   `VisitSiteNavbarItem` view exists in the Admin module; if the top bar does not change, the narration is wrong.
3. **The `t` filter in an admin template** (`create-3`, `create-4`, `create-6`). The documentation uses
   `"Visit Site" | t` in the same shape (`Admin/README.md`, the Liquid form of `VisitSiteNavbarItem`). Confirm that the
   top bar reads **Visit Site** after the icon, and not an empty text or a key.
4. **The look of the changed link**. The first version of the Content adds the text after the icon, with `me-1` on the
   icon. If the spacing or the tooltip looks wrong, change only the classes in the Liquid; the narration says only that
   the words **Visit Site** are added beside the icon.
5. **Setting the text of the editor with a script** (`create-3`, `manage-1`). `window.__orchardCoreMonacoReady` is read
   from `OrchardCore.Resources/Assets/monaco/monaco-loader.ts:69`, and `monaco.editor.getEditors()` is a Monaco API,
   but neither was run. The fallback is `monaco.editor.getModels()[0].setValue(text)`. After the call, check that the
   hidden field holds the text (the page posts it on Save).
6. **The search of the Features page** (`enable-1`). `Drivers/FeatureEntryDisplayDriver.cs:20` builds the filter text
   from the name, the id and the description. Confirm that `admin templates` leaves the Admin Templates row on screen.
7. **The sidebar menus** (`enable-1`, `enable-3`, `create-6`). The menu items are read from `AdminMenu.cs` files, not
   from the rendered sidebar of `TheAdmin`. If a click on **Tools**, **Design** or **Content** does not open its
   submenu, navigate by address, as the plan says.
8. **The Delete confirmation** (`manage-2`). The dialog texts are read from `TheAdmin/Views/Layout.cshtml:99`
   (title **Delete**, "Are you sure you want to remove this element?", **Ok**, **Cancel**), and the **Delete** link is
   marked `data-url-af="RemoveUrl UnsafeUrl"`. Confirm that this is the dialog the admin shows.
9. **A feature profile** (`good-3`). `OrchardCore/OrchardCore/Shell/FeatureProfilesValidationProvider.cs` refuses a
   feature that a rule excludes. Confirm on a multi-tenant host that a tenant whose profile holds the `Exclude` rule
   does not list **Admin Templates** on its Features page.
10. **Pronunciation**. The `spoken` map of the storyboard splits `AdminBranding`, `VisitSiteNavbarItem`,
    `NotifyMessages` and `AdminTemplates`. Check these, "the t filter" (`create-3`) and "URL" with `scripts/pronounce.py`
    before narrating.

Left out on purpose: the documentation shows a Liquid `Message` template for the admin toasts
(`Notifications/README.md`, "Admin Templates UI"). `TheAdmin` shows a toast only for elements with the class `toast`
inside its container (`OrchardCore.Themes/TheAdmin/Assets/js/NotifyMessages/NotifyMessages.ts`), so an alert-style
`Message` template alone may not behave as the page describes; the video names the shape and does not show it.
