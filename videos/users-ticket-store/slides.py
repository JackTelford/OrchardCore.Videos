"""The slides of the Users Authentication Ticket Store video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
taken from the Orchard Core source (the files named in the panel titles), trimmed with comments.
"""
from brand import W, bullets, draw_code, font, mono, note_cards, slide_base

TYPES = {"AuthenticationTicket", "CacheTicketStore", "CacheTicketStoreStartup", "CookieAuthenticationOptions",
         "CookieAuthenticationOptionsConfigure", "DistributedCacheEntryOptions", "Feature", "Guid",
         "IConfigureNamedOptions", "IDataProtectionProvider", "IDataProtector", "IDistributedCache",
         "IServiceCollection", "ITicketStore", "IdentityConstants", "MemoryDistributedCache", "StartupBase", "Task",
         "UserConstants", "Exception"}

BEFORE = [
    ("1", "One claim for each role", "The Roles module adds the roles of the user to the ticket."),
    ("2", "One claim for each permission", "Taken from those roles, unless the user is an administrator."),
    ("3", "A cookie that grows", "The more roles and permissions, the larger the cookie."),
    ("4", "Requests that fail", "The documentation describes requests failing over the header limit."),
]

AFTER = [
    ("check", "The ticket stays on the server", "It is kept in a cache, protected with Data Protection."),
    ("check", "A small cookie", "It holds only a session identifier."),
    ("check", "The same ticket", "It is stored, retrieved and removed by its key."),
]

RECIPE = """
{
  "steps": [
    {
      "name": "Feature",
      "enable": [
        "OrchardCore.Users.Authentication.CacheTicketStore"
      ]
    }
  ]
}
"""

MANIFEST = """
[assembly: Feature(
    Id = "OrchardCore.Users.Authentication.CacheTicketStore",
    Name = "Users Authentication Ticket Store",
    // ...
    Dependencies = [UserConstants.Features.Users],
    Category = "Security"
)]
"""

STARTUP = """
[Feature("OrchardCore.Users.Authentication.CacheTicketStore")]
public sealed class CacheTicketStoreStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.ConfigureOptions<CookieAuthenticationOptionsConfigure>();
    }
}
"""

CONFIGURE = """
public sealed class CookieAuthenticationOptionsConfigure : IConfigureNamedOptions<CookieAuthenticationOptions>
{
    // ...

    public void Configure(string name, CookieAuthenticationOptions options)
    {
        if (name == IdentityConstants.ApplicationScheme)
        {
            options.SessionStore = new CacheTicketStore(_distributedCache, _dataProtectionProvider, _logger);
        }
    }

    // ...
}
"""

STORE = """
private const string KeyPrefix = "ocauth-ticket";

public async Task<string> StoreAsync(AuthenticationTicket ticket)
{
    var key = Guid.NewGuid().ToString();
    var cacheKey = $"{KeyPrefix}-{key}";

    try
    {
        var protectedBytes = DataProtector.Protect(SerializeTicket(ticket));
        await _distributedCache.SetAsync(cacheKey, protectedBytes,
            new DistributedCacheEntryOptions() { AbsoluteExpiration = ticket.Properties.ExpiresUtc });

        return key;
    }
    catch (Exception e)
    {
        _logger.LogError(e, "{MethodName} failed  for '{Key}'.", nameof(StoreAsync), cacheKey);
        throw;
    }
}
"""

RETRIEVE = """
public async Task<AuthenticationTicket?> RetrieveAsync(string key)
{
    var cacheKey = $"{KeyPrefix}-{key}";

    var bytes = await _distributedCache.GetAsync(cacheKey);
    if (bytes == null || bytes.Length == 0)
    {
        return null;
    }

    try
    {
        var ticket = DeserializeTicket(DataProtector.Unprotect(bytes));
        return ticket;
    }
    catch (Exception e)
    {
        _logger.LogError(e, "{MethodName} failed  for '{Key}'.", nameof(RetrieveAsync), cacheKey);
        return null;
    }
}
"""

REMOVE_RENEW = """
public async Task RemoveAsync(string key)
{
    var cacheKey = $"{KeyPrefix}-{key}";
    await _distributedCache.RemoveAsync(cacheKey);
}

public async Task RenewAsync(string key, AuthenticationTicket ticket)
{
    var cacheKey = $"{KeyPrefix}-{key}";

    try
    {
        var protectedBytes = DataProtector.Protect(SerializeTicket(ticket));
        await _distributedCache.SetAsync(cacheKey, protectedBytes,
            new DistributedCacheEntryOptions() { AbsoluteExpiration = ticket.Properties.ExpiresUtc });
    }
    catch (Exception e)
    {
        _logger.LogError(e, "{MethodName} failed  for '{Key}'.", nameof(RenewAsync), cacheKey);
    }
}
"""

MEMORY = """
// MemoryDistributedCache needs to be registered as a singleton as it owns a MemoryCache instance.
services.AddSingleton<IDistributedCache, MemoryDistributedCache>();
"""

REDIS_SETTINGS = """
{
  "OrchardCore": {
    "Redis": {
      "Configuration": "localhost:6379",
      "InstancePrefix": "MyApp:"
    }
  }
}
"""

REDIS_RECIPE = """
{
  "steps": [
    {
      "name": "Feature",
      "enable": [
        "OrchardCore.Redis",
        "OrchardCore.Redis.Cache"
      ]
    }
  ]
}
"""

PROTECTOR = """
private IDataProtector DataProtector => _dataProtector ??= _dataProtectionProvider
    .CreateProtector($"{nameof(CacheTicketStore)}_{IdentityConstants.ApplicationScheme}");
"""


def code_panel(image, box, title, language, code, size, highlight=None):
    """A code panel that must hold its code: the lines fit its height and the longest one fits its width."""
    x0, y0, x1, y1 = box
    lines = code.strip("\n").split("\n")
    height = 74 + int(size * 1.42) * (len(lines) - 1) + size + 14
    width = max(mono(size).getlength(line) for line in lines) + 60
    if height > y1 - y0 or width > x1 - x0:
        raise SystemExit(f"The code of {title} ({len(lines)} lines, {width:.0f} px wide) overflows its panel")
    draw_code(image, box, title, language, code, types=TYPES, highlight=highlight, size=size)


def notes(image, items, box):
    """Note cards whose texts must fit the width of the box."""
    x0, _, x1, _ = box
    for title, text in items:
        if max(font(34, "Bold").getlength(title), font(30).getlength(text)) + 80 > x1 - x0:
            raise SystemExit(f"The note {title!r} overflows its card")
    note_cards(image, items, box)


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2"):
        image = slide_base(chapter_title, label, "The ticket in the cookie")
        bullets(image, BEFORE, active={0, 1} if step_id == "why-1" else {2, 3})
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "With the ticket store")
        bullets(image, AFTER)
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "Enable it from a recipe")
        code_panel(image, (96, 240, 1180, 700), "example.recipe.json", "json", RECIPE, 26)
        notes(image, [
            ("The Feature step", "enables features by id."),
            ("The identifier", "is on the Features page."),
        ], (1220, 240, W - 96, 1030))
        return image

    if step_id == "how-1":
        image = slide_base(chapter_title, label, "1. Register the cookie configuration")
        code_panel(image, (96, 240, W - 96, 580), "Manifest.cs", "csharp", MANIFEST, 25)
        code_panel(image, (96, 620, W - 96, 1030), "CacheTicketStoreStartup.cs", "csharp", STARTUP, 26)
        return image

    if step_id == "how-2":
        image = slide_base(chapter_title, label, "2. Set the session store")
        code_panel(image, (96, 240, W - 96, 1030), "CookieAuthenticationOptionsConfigure.cs", "csharp", CONFIGURE, 26)
        return image

    if step_id == "how-3":
        image = slide_base(chapter_title, label, "3. Store the ticket")
        code_panel(image, (96, 240, W - 96, 1030), "CacheTicketStore.cs", "csharp", STORE, 23)
        return image

    if step_id == "how-4":
        image = slide_base(chapter_title, label, "4. Retrieve the ticket")
        code_panel(image, (96, 240, W - 96, 1030), "CacheTicketStore.cs", "csharp", RETRIEVE, 23)
        return image

    if step_id == "how-5":
        image = slide_base(chapter_title, label, "5. Remove and renew")
        code_panel(image, (96, 240, W - 96, 1030), "CacheTicketStore.cs", "csharp", REMOVE_RENEW, 23)
        return image

    if step_id == "cache-1":
        image = slide_base(chapter_title, label, "The default: a memory cache")
        code_panel(image, (96, 240, W - 96, 500), "OrchardCoreBuilderExtensions.cs", "csharp", MEMORY, 26)
        notes(image, [
            ("One cache for each tenant", "It is registered in the services of the tenant."),
            ("In memory", "A restart or a reload of the tenant clears the tickets."),
        ], (96, 560, W - 96, 1030))
        return image

    if step_id == "cache-2":
        image = slide_base(chapter_title, label, "Share the tickets with Redis")
        code_panel(image, (96, 240, 900, 740), "appsettings.json", "json", REDIS_SETTINGS, 24)
        code_panel(image, (940, 240, W - 96, 740), "example.recipe.json", "json", REDIS_RECIPE, 24)
        notes(image, [
            ("Missing or invalid configuration", "The tenant keeps the memory cache."),
        ], (96, 790, W - 96, 1030))
        return image

    if step_id == "cache-3":
        image = slide_base(chapter_title, label, "Share the Data Protection keys too")
        code_panel(image, (96, 240, W - 96, 450), "CacheTicketStore.cs", "csharp", PROTECTOR, 26)
        notes(image, [
            ("Distributed Data Protection (Redis)", "Keeps the key ring in Redis."),
            ("Distributed Data Protection (Azure Storage)", "Keeps the key ring in Azure Blob Storage."),
        ], (96, 520, W - 96, 1030))
        return image

    raise SystemExit(f"No slide for {step_id}")
