# Open questions: Dynamic Cache

What was read from the Orchard Core source (commit `92db4a83`) and what a person must confirm on a running site before
recording. Nothing in the narration depends on an unconfirmed item as a fact, except where noted. Paths are relative to
`src/` of the Orchard Core checkout.

## To confirm on a running site (the recording depends on them)

1. **The tab of Cache Mode.** `OrchardCore.Modules/OrchardCore.Settings/Drivers/DefaultSiteSettingsDisplayDriver.cs:57`
   places the cache editor in a group `#Cache` of the **Settings, General** page. The tab strip is drawn by
   `AdminTabs` in `OrchardCore/OrchardCore.DisplayManagement/Shapes/GroupShapes.cs:14-65`: an `a` element with
   `role="tab"` whose text is the placement key (`Cache`), the first tab being the active one. No theme overrides it in
   this checkout. Confirm on the site that the tab **Cache** exists and that `GetByRole(AriaRole.Tab, Name = "Cache")`
   finds it, and that the page shows the first tab again after **Save**.
2. **The list box trick.** `recording-plan.md` (`mode-2`) sets `size = 4` on the Cache Mode `<select>` so that its
   four options are drawn in a headless browser. Check that the long first label ("From environment — enabled in
   Production, disabled otherwise") is not cut off in a way that hides what the narration says, and that
   `SelectAsync(..., "Enabled")` still works on the list box.
3. **The template wins over the theme.** The seed template `Content__Article` is meant to replace the Blog theme's
   `Content-Article.liquid` for the About page. `OrchardCore/OrchardCore.DisplayManagement/Implementation/DefaultHtmlDisplay.cs`
   (`GetShapeBindingAsync`) asks the shape binding resolvers, which include the Templates module
   (`OrchardCore.Modules/OrchardCore.Templates/Services/TemplatesShapeBindingResolver.cs`), before the shape table of the
   theme, so the template should win. Check that `/about` shows **Rendered at** and **Cached at**.
4. **`{{ "now" | date: "%T" }}` in a database template.** The format comes from the examples of the module's page. Check
   that it prints a time such as `14:03:21` in a template of the Templates module, and that two lines of the template
   are visible without scrolling.
5. **Publishing without changes removes the entry.** `watch-3` publishes the About page without editing it.
   `OrchardCore.Modules/OrchardCore.Contents/Handlers/ContentsHandler.cs` removes the tag
   `contentitemid:<id>` in `PublishedAsync`, and the editor asks for a draft before publishing
   (`Controllers/AdminController.cs`, `VersionOptions.DraftRequired`). Check that **Cached at** changes after the publish
   and does not change before it. If it does not, edit the text of the About page before clicking **Publish** and say
   so here.
6. **Entry lifetime.** The cache block in the seed expires after `00:05:00` (`expires_after`). `04-watch` must run within
   that time after the page was opened before the capture, or the entry expires on its own and `watch-2` shows a changed
   cached time.

## Read in the source, to confirm when convenient

7. **Debug comments.** The slide `block-4` shows the comment text written by cache debug mode:
   `OrchardCore/OrchardCore.DisplayManagement.Liquid/Cache/CacheTag.cs` (`CACHE BLOCK:`, `END CACHE BLOCK:`) and
   `OrchardCore.Modules/OrchardCore.DynamicCache/EventHandlers/DynamicCacheShapeDisplayEvents.cs` (`START CACHED SHAPE:`,
   `END CACHED SHAPE:`). The strings and their spacing were compared with the source and match. Enable **Enabled with cache debug mode**, view the page source of the About page, and compare the
   spacing and the values (`EXPIRES AFTER: 00:05:00`, empty `EXPIRES ON:`). The `dynamic-cache` tag helper writes the
   same `CACHE BLOCK:` comment (`TagHelpers/DynamicCacheTagHelper.cs`).
8. **The Razor slide.** `shape-2` shows `@addTagHelper *, OrchardCore.DynamicCache` and
   `expires-after="@TimeSpan.FromMinutes(5)"`. Neither is in the documentation. The first follows from the assembly name of
   the module (`OrchardCore.DynamicCache.csproj`) and the rule that Razor needs `@addTagHelper` for a tag helper outside
   the default ones; the second from the attribute type (`TimeSpan?` in `DynamicCacheTagHelper.cs`), which Razor cannot
   fill from a text such as `00:05:00`. `TheTheme/Views/_ViewImports.cshtml` shows the same form for the tag helpers of another module
   (`@addTagHelper *, OrchardCore.Menu`). Build a view with the snippet to confirm both. The shape attributes of
   `shape-1` (`cache-fixed-duration="00:05:00"`) are strings parsed by the tag helper
   (`OrchardCore/OrchardCore.DisplayManagement/TagHelpers/BaseShapeTagHelper.cs`), as in `TheTheme/Views/Layout.cshtml`.
9. **`item.ContentItemId` in a query loop (`inval-4`).** The documentation example appends
   `Model.ContentItem.ContentItemId`, the content item of the page, for every listed item. The slide uses
   `item.ContentItemId` instead, as the narration says ("each item adds its own content item id"). The Blog recipe defines `RecentBlogPosts` with `ReturnContentItems` set, and `QueryFilter.cs` returns the items of the
   result, so they should be content items. Confirm that `ContentItemId` can be read on them in Liquid, and that the
   entry is removed when one of them is published. Also check `<cache-dependency dependency="contentitemid:@item.ContentItemId" />`
   compiles.
10. **A parent block, its children and its contexts (`inval-3`).** `CacheScopeManager.ExitScope`
    (`OrchardCore/OrchardCore.Infrastructure/Cache/CacheScopeManager.cs`) adds the contexts, tags and expirations of a
    nested section to its parent. In `CacheTag.cs` a nested Liquid block that is found in the cache writes its content and
    returns before `EnterScope`, so a child served from the cache adds nothing to its parent. After the first render,
    invalidating the tag of a child then does not invalidate a parent that was stored on a render where the child was
    cached, which differs from the page of the module ("invalidating a block will also invalidate all parent blocks", reported as a
    documentation issue). A
    shape enters its scope before the lookup (`DynamicCacheShapeDisplayEvents.DisplayingAsync`), so it behaves differently.
    The parent's lookup also happens before its children render, with only the contexts the parent declared, so a parent
    whose only `vary_by` comes from a child may never find its entry. The video says only that a nested Liquid block that
    renders adds its dependencies to its parent, the shorter expiration winning, and that one read from the cache adds
    nothing (`CacheTag.cs`, which returns before `EnterScope`; the `dynamic-cache` tag helper enters its scope before the
    lookup, so the statement is limited to Liquid blocks). Test
    two cases: a parent with no `vary_by` around a child with `vary_by: "user"`, and publishing the content item of a
    child after a second load of the page.
11. **Resources of a cached shape (`inval-5`).** The statement is the comment in
    `OrchardCore.Modules/OrchardCore.Notifications/Drivers/NotificationNavbarDisplayDriver.cs`. Check on a shape that
    registers a script with `{% script %}`: on a cache hit the script must be missing from the page.
12. **Cache mode takes effect after the save.** `DefaultSiteSettingsDisplayDriver.cs:140` requests a release of the tenant
    on every save of the **General** settings, and `CacheOptionsConfiguration.cs` reads `CacheMode` when the options are
    created. Check that the first request after the save already caches (`watch-1` depends on it), and how long the reload
    takes.

## Pronunciation, to confirm with `scripts/pronounce.py`

13. **Hyphenated names.** `shape-2` and `inval-4` read `dynamic-cache`, `cache-id`, `vary-by`, `expires-on`,
    `expires-after`, `expires-sliding` and `cache-dependency`. `storyboard.json` maps each to the words without the
    hyphen. Listen to the narration of both steps and to `user dot roles` (`inval`/`shape` steps) before recording.
