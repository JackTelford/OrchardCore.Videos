# Open questions: debugging-settings

The narration states only what the source and the documentation show. These points were read from the code and could
not be seen on a running site. Confirm them while recording; if one differs, the step named at its end must change.

1. **Menu markup.** The selectors of the plan (`#left-nav button.nav-group-label`, `#left-nav a.item-label`, the `.title`
   span) were read from `OrchardCore.Themes/TheAdmin/Views/NavigationItem-admin.cshtml`, `NavigationItemLink-admin.cshtml`
   and `OrchardCore.Settings/Views/NavigationItemText-settings.Id.cshtml`, not from a rendered page. Confirm that the group
   **Settings** is a button that expands, and that **Debugging** is a link inside it. Steps: `enable-1`, `off-1`.
2. **The heading of the Debugging page.** `OrchardCore.Settings/Views/Admin/Index.cshtml:4` gives the breadcrumb the title
   "Settings" for every group, and the narration does not name the heading. Confirm what the page shows above the option
   (the plan only needs the option and its hint on screen). Steps: `enable-1`, `enable-2`.
3. **The change applies without a reload of the site.** `DebugSettingsDisplayDriver.UpdateAsync` asks for an options
   refresh (`OrchardCore.Settings/Drivers/DebugSettingsDisplayDriver.cs:64`) and the documentation says the option takes
   effect without a shell release. The narration says only that the change "applies to the running site". Confirm that
   the page source fetched right after **Save** already holds the comments. Steps: `enable-3`, `read-2`.
4. **Where the comments appear in the home page of the Blog recipe.** The code writes a start comment before and an end
   comment after every shape that is rendered with at least one binding
   (`OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs:244-285`). It writes none for an empty shape, for
   a shape that is already HTML, or for a shape whose content came pre-rendered from a cache
   (`DefaultHtmlDisplay.cs:55-69`, `:138-141` and `:262`). The narration does not name any shape type or template. Confirm that the
   source of the home page holds comments from its first line on, that the first two comments are start comments of
   nested shapes, and that a `shape-end` comment follows within one screen. If not, adjust the rings of `read-3` and
   `read-4`.
5. **The page looks the same.** The narration of `read-1` says the page "looks like any other page" because browsers do
   not display comments. Confirm that the home page renders as usual with the option on (a comment placed before the
   document type declaration must not change the layout).
6. **Cached shapes.** A shape served from the Dynamic Cache has no binding in the current render
   (`DefaultHtmlDisplay.cs:262`), so the comments of a cached fragment would be those of the render that filled the cache.
   The narration does not mention caching. The plan keeps the Dynamic Cache feature off. Confirm that the Blog recipe
   does not enable it.
7. **The same comments in the admin.** `DefaultHtmlDisplay` has no check on the admin or on the user, so the option
   applies to the whole site and to every visitor, which `off-1` says. The plan shows the source of the front end only.
   If you want a second proof, fetch the source of an admin page signed in as admin and check that it holds the comments.
8. **The toast.** `.toast.message-success` carries "Site settings updated successfully." Confirm how long it stays on
   screen (`Message.cshtml` hides it after a delay when the notification has one). The plan asks that it is still visible
   when `enable-3` and `off-1` end. Steps: `enable-3`, `off-1`.
9. **The page-source view.** Headless Chromium has no page-source window, and navigating to a `view-source:` address is
   not part of this plan. The plan renders the HTML text of the page itself (the helper in `recording-plan.md`). Confirm
   that the helper shows the comments as written, and that the Recorder's cursor and rings stay visible over it.
   Steps: `read-2`, `read-3`, `read-4`, `off-2`.
10. **Private data in the source.** The page source of an anonymous request must hold no absolute path of the recording
    machine (the binding sources are virtual paths or type names, by `ShapeAlterationBuilder.BoundAs` and the Razor
    binding strategy) and no token. Read the contact sheets of `read-2` to `read-4` and `off-2`.
