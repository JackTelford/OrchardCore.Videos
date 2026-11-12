"""The slides of the Dynamic Cache video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
Liquid has no lexer in brand.py: its panels use the "html" one.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"ICacheContextProvider", "CacheContextEntry", "CacheContext", "ITagCache", "PublishContentContext",
         "ShapeResult", "ActionResult", "IEnumerable", "List", "Task", "HttpContext", "TimeSpan", "HttpPost"}

# Why: a page cache against a cache of sections.
POLICY = [
    ("1", "A page cache", "One entry for the whole response"),
    ("2", "Dynamic Cache", "One entry for each section of markup"),
    ("3", "A cache policy per section", "A key, contexts, dependencies and an expiration"),
]

LAYOUT = [
    ("L", "Layout", "Not cached"),
    ("A", "Section A", "Contains A1, which varies by role, and A2"),
    ("B", "Section B", "Contains B1, which varies by query string, and B2"),
]

REQUESTS = [
    ("1", "First request", "Every shape is evaluated, and the cacheable sections are stored"),
    ("2", "Later requests", "The markup is read from the cache, and the shape is not processed"),
    ("3", "Storage", "IDynamicCache, based on IDistributedCache: memory by default"),
]

# The cache block.
BLOCK = """
{% cache "recent-blog-posts" %}
    {% assign recentBlogPosts = Queries.RecentBlogPosts | query %}
    {% for item in recentBlogPosts %}
        {{ item | display_text }}
    {% endfor %}
{% endcache %}
"""

ARGUMENTS = [
    ("1", "vary_by", "Contexts that split the entry: user, user.roles, query, route, features"),
    ("2", "dependencies", "Tags that invalidate the entry"),
    ("3", "expires_after", "An absolute expiration, as a time span such as 00:05:00"),
    ("4", "expires_sliding", "A sliding expiration, as a time span such as 00:01:00"),
]

KNOWN_VALUE = """
{% cache "blog-post-summary", vary_by: Model.ContentItem.ContentItemId %}
    ...
{% endcache %}
"""

DEBUG_BLOCK = """
<!-- CACHE BLOCK: recent-blog-posts (...)
         VARY BY: user.roles
    DEPENDENCIES: alias:blog
      EXPIRES ON:
   EXPIRES AFTER: 00:05:00
 EXPIRES SLIDING:
-->
...
<!-- END CACHE BLOCK: recent-blog-posts -->
"""

DEBUG_SHAPE = """
<!-- START CACHED SHAPE: main-menu (...)
          VARY BY: user.roles
     DEPENDENCIES: alias:main-menu
       EXPIRES ON:
    EXPIRES AFTER: 00:05:00
  EXPIRES SLIDING:
-->
...
<!-- END CACHED SHAPE: main-menu (...) -->
"""

# Shapes.
SHAPE_LIQUID = """
{% shape "menu",
    alias: "alias:main-menu",
    cache_id: "main-menu",
    cache_fixed_duration: "00:05:00",
    cache_tag: "alias:main-menu",
    cache_context: "user.roles" %}
"""

SHAPE_RAZOR = """
<menu alias="alias:main-menu"
      cache-id="main-menu"
      cache-fixed-duration="00:05:00"
      cache-tag="alias:main-menu"
      cache-context="user.roles" />
"""

DYNAMIC_CACHE_TAG = """
@addTagHelper *, OrchardCore.DynamicCache

<dynamic-cache cache-id="recent-blog-posts"
               vary-by="user.roles"
               dependencies="alias:blog"
               expires-after="@TimeSpan.FromMinutes(5)">
    ...
</dynamic-cache>
"""

DRIVER = """
// NotificationNavbarDisplayDriver, used when an expiration option is above zero
result
    .Cache(NotificationConstants.TopUnreadUserNotificationCacheTag, context =>
    {
        context.AddContext("user")
            .AddTag(NotificationConstants.TopUnreadUserNotificationCacheTag)
            .AddTag(NotificationsHelper.GetUnreadUserNotificationTagKey(user.Identity.Name))
            .AddTag(NotificationsHelper.GetUnreadUserNotificationTagKeyByUserId(userId));

        if (_notificationOptions.AbsoluteCacheExpirationSeconds > 0)
        {
            context.WithExpiryAfter(TimeSpan.FromSeconds(_notificationOptions.AbsoluteCacheExpirationSeconds));
        }

        // ...
    });
"""

# Invalidation.
CONTENTS_HANDLER = """
public override Task PublishedAsync(PublishContentContext context)
{
    return _tagCache.RemoveTagAsync($"contentitemid:{context.ContentItem.ContentItemId}");
}
// UnpublishedAsync removes the same tag, and RemovedAsync does when no active version is left.
"""

TAGS = [
    ("1", "contentitemid:{ContentItemId}", "Contents: published, unpublished or removed"),
    ("2", "alias:{Alias}", "Alias: published, unpublished or removed"),
    ("3", "slug:{Path}", "Autoroute: published, unpublished or removed"),
]

TAG_CACHE = """
public interface ITagCache
{
    Task TagAsync(string key, params string[] tags);
    Task<IEnumerable<string>> GetTaggedItemsAsync(string tag);
    Task RemoveTagAsync(string tag);
}
"""

TAG_USAGE = """
public async Task<ActionResult> Tag(string tag)
{
    await _tagCache.RemoveTagAsync(tag);
    // ...
}
"""

NESTED = """
{% cache "a" %}
    {% cache "a1", dependencies: "a1", expires_after: "00:05:00" %}
        ...
    {% endcache %}
    {% cache "a2", dependencies: "a2", expires_after: "00:00:01" %}
        ...
    {% endcache %}
{% endcache %}
"""

NESTED_NOTES = [
    ("Dependencies", "When both render, a is tagged a1 and a2."),
    ("Expiration", "When both render, a expires after one second."),
]

QUERY_LIQUID = """
{% cache "recent-blog-posts" %}
    {% assign recentBlogPosts = Queries.RecentBlogPosts | query %}
    {% for item in recentBlogPosts %}
        {{ item | display_text }}

        {% assign dependency = "contentitemid:" | append: item.ContentItemId %}
        {% cache_dependency dependency %}
    {% endfor %}
{% endcache %}
"""

QUERY_RAZOR = """
<cache-dependency dependency="contentitemid:@item.ContentItemId" />
"""

RESOURCES = """
// A cached shape doesn't register its resources when its content comes from the cache,
// so they have their own shape.
return Combine(
    result,
    Dynamic("UserNotificationNavbarResources")
        .Location(OrchardCoreConstants.DisplayType.Detail, "Content:9")
        .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:9"));
"""

# Extending and configuring.
PROVIDER = """
public class RouteCacheContextProvider : ICacheContextProvider
{
    // ...

    public Task PopulateContextEntriesAsync(IEnumerable<string> contexts, List<CacheContextEntry> entries)
    {
        if (contexts.ContainsContext("route"))
        {
            var httpContext = _httpContextAccessor.HttpContext;
            entries.Add(new CacheContextEntry("route", httpContext.Request.Path.Value.ToLowerInvariant()));
        }

        return Task.CompletedTask;
    }
}
"""

REGISTRATION = """
services.AddScoped<ICacheContextProvider, RouteCacheContextProvider>();
"""

FAILOVER = """
{
  "OrchardCore": {
    "DynamicCache": {
      "FailoverRetryLatency": "00:00:30"
    }
  }
}
"""

STORAGE_NOTES = [
    ("IDynamicCache", "Based on IDistributedCache."),
    ("Memory by default", "A Redis cache replaces it."),
    ("When a read or write fails", "The cache is skipped for a while."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A page cache and a cache of sections")
        bullets(image, POLICY)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "A layout with cached sections")
        bullets(image, LAYOUT)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "What happens on each request")
        bullets(image, REQUESTS)
        return image

    if step_id == "block-1":
        image = slide_base(chapter_title, label, "The Liquid cache block")
        draw_code(image, (96, 240, W - 96, 700), "Template.liquid", "html", BLOCK, highlight={1, 6}, size=28)
        return image

    if step_id == "block-2":
        image = slide_base(chapter_title, label, "The arguments of the block")
        bullets(image, ARGUMENTS)
        return image

    if step_id == "block-3":
        image = slide_base(chapter_title, label, "Any value can vary an entry")
        draw_code(image, (96, 240, 1160, 560), "Template.liquid", "html", KNOWN_VALUE, size=24)
        note_cards(image, [
            ("Any other value", "Used as it is."),
            ("No expiration", "A sliding window of one minute."),
        ], (1200, 240, W - 96, 700))
        return image

    if step_id == "block-4":
        image = slide_base(chapter_title, label, "Cache debug mode")
        draw_code(image, (96, 240, 960, 760), "Page source: a Liquid block", "html", DEBUG_BLOCK, size=22)
        draw_code(image, (1000, 240, W - 96, 760), "Page source: a shape", "html", DEBUG_SHAPE, size=22)
        return image

    if step_id == "shape-1":
        image = slide_base(chapter_title, label, "Attributes on a shape")
        draw_code(image, (96, 240, 940, 640), "TheBlogTheme: Layout.liquid", "html", SHAPE_LIQUID, size=26)
        draw_code(image, (980, 240, W - 96, 640), "TheTheme: Layout.cshtml", "cshtml", SHAPE_RAZOR, size=26)
        note_cards(image, [
            ("Also available", "cache_sliding_duration, a sliding expiration"),
        ], (96, 700, W - 96, 900))
        return image

    if step_id == "shape-2":
        image = slide_base(chapter_title, label, "The dynamic-cache tag helper")
        draw_code(image, (96, 240, W - 96, 740), "RecentPosts.cshtml", "cshtml", DYNAMIC_CACHE_TAG, size=28)
        return image

    if step_id == "shape-3":
        image = slide_base(chapter_title, label, "Cache from a display driver")
        draw_code(image, (96, 240, W - 96, 900), "NotificationNavbarDisplayDriver.cs", "csharp", DRIVER,
                  types=TYPES, size=24)
        return image

    if step_id == "inval-1":
        image = slide_base(chapter_title, label, "Tags that Orchard Core removes")
        draw_code(image, (96, 240, W - 96, 510), "ContentsHandler.cs", "csharp", CONTENTS_HANDLER, types=TYPES, size=24)
        bullets(image, TAGS, top=525)
        return image

    if step_id == "inval-2":
        image = slide_base(chapter_title, label, "Remove your own tags")
        draw_code(image, (96, 240, 1010, 620), "ITagCache.cs", "csharp", TAG_CACHE, types=TYPES, size=24)
        draw_code(image, (1050, 240, W - 96, 620), "HomeController.cs", "csharp", TAG_USAGE, types=TYPES, size=24)
        return image

    if step_id == "inval-3":
        image = slide_base(chapter_title, label, "What a nested block adds to its parent")
        draw_code(image, (96, 240, 1000, 760), "Template.liquid", "html", NESTED, size=22)
        note_cards(image, NESTED_NOTES, (1040, 240, W - 96, 700))
        return image

    if step_id == "inval-4":
        image = slide_base(chapter_title, label, "Add dependencies while rendering")
        draw_code(image, (96, 240, W - 96, 740), "Template.liquid", "html", QUERY_LIQUID, size=24)
        draw_code(image, (96, 780, W - 96, 930), "Template.cshtml", "cshtml", QUERY_RAZOR, size=24)
        return image

    if step_id == "inval-5":
        image = slide_base(chapter_title, label, "Resources and cached shapes")
        draw_code(image, (96, 240, W - 96, 700), "NotificationNavbarDisplayDriver.cs", "csharp", RESOURCES,
                  types=TYPES, size=24)
        return image

    if step_id == "ext-1":
        image = slide_base(chapter_title, label, "Add a cache context")
        draw_code(image, (96, 240, W - 96, 850), "RouteCacheContextProvider.cs", "csharp", PROVIDER,
                  types=TYPES, size=24)
        draw_code(image, (96, 880, W - 96, 1020), "OrchardCoreBuilderExtensions.cs", "csharp", REGISTRATION,
                  types=TYPES, size=24)
        return image

    if step_id == "ext-2":
        image = slide_base(chapter_title, label, "Storage and failover")
        draw_code(image, (96, 240, 900, 640), "appsettings.json", "json", FAILOVER, size=26)
        note_cards(image, STORAGE_NOTES, (940, 240, W - 96, 900))
        return image

    raise SystemExit(f"No slide for {step_id}")
