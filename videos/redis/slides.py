"""The slides of the Redis video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The code comes from the Redis module of OrchardCore and from its documentation, trimmed with // ...
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "IDistributedCache", "RedisCacheWrapper", "ITagCache", "RedisTagCache", "StartupBase", "IServiceCollection",
    "IMessageBus", "RedisBus", "IDistributedLock", "RedisLock", "ILock", "ILocker", "Signal", "ISignal", "Task",
    "DistributedSignal", "RedisBusStartup", "RedisCacheStartup", "RedisLockStartup", "RedisChannel", "IRedisService",
    "NullRedisService", "IModularTenantEvents", "IConnectionMultiplexer", "IDatabase", "ShellSettings",
    "CounterService", "ICounterStore", "RedisCounterStore", "Startup", "RedisXmlRepository", "IBackgroundTask",
    "ReportBackgroundTask", "IServiceProvider", "CancellationToken", "WebApplication", "Exception", "StringComparison",
    "RedisCacheOptionsSetup", "IConfigureOptions", "RedisCacheOptions", "BackgroundTask", "ServiceDescriptor",
}

LOCAL_STATE = [
    ("1", "Cache", "Documents and rendered output are cached in the memory of each server."),
    ("2", "Signals", "A signal that refreshes caches reaches only the server that raised it."),
    ("3", "Locks", "A named lock holds back only the code of its own server."),
    ("4", "Keys", "The data protection keys are files in the App_Data folder of each server."),
]

REDIS_FEATURES = [
    ("1", "Redis Cache", "Shares the distributed cache and the tag cache."),
    ("2", "Redis Bus", "Publishes the signals to every server."),
    ("3", "Redis Lock", "Holds a named lock for all the servers."),
    ("4", "Distributed Data Protection (Redis)", "Keeps the data protection keys in Redis."),
]

TENANT_KEYS = [
    ("1", "One connection", "Tenants with the same connection string share one connection."),
    ("2", "Prefixed keys", "The instance prefix, then the tenant name, then the name of the key."),
    ("3", "An example", "MyApp:Default:DataProtection-Keys is the key ring of the Default tenant."),
]

NOT_CONFIGURED = [
    ("1", "An error is logged", "The tenant keeps running with the default services."),
    ("2", "The Redis features stay inactive", "Cache, bus, lock, data protection and the SignalR backplane."),
    ("3", "The health check is Unhealthy", "It reports the Redis server as unhealthy."),
]

PROTECTED = [
    ("1", "Authentication cookies", "Every server of the site must be able to read them."),
    ("2", "Anti-forgery tokens", "A form may be posted to another server than the one that rendered it."),
    ("3", "Stored secrets", "Values such as an SMTP password, decrypted when they are used."),
    ("4", "Local by default", "Each server keeps its key ring in the App_Data folder of the tenant."),
]

SETTINGS = """
{
  "OrchardCore": {
    "Redis": {
      "Configuration": "localhost:6379",
      "InstancePrefix": "MyApp:",
      "AllowAdmin": true,
      "DisableCertificateVerification": false
    }
  }
}
"""

DOCKER = """
docker run --name redis -p 6379:6379 -d redis
"""

CACHE_STARTUP = """
[Feature("OrchardCore.Redis.Cache")]
public sealed class RedisCacheStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        if (services.IsRedisConfigured())
        {
            services.AddSingleton<IDistributedCache, RedisCacheWrapper>(sp =>
            {
                // ...
            });
            // ...
            services.AddScoped<ITagCache, RedisTagCache>();
        }
    }
}
"""

BUS_STARTUP = """
public sealed class RedisBusStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        if (services.IsRedisConfigured())
        {
            services.AddSingleton<IMessageBus, RedisBus>();
        }
    }
}
"""

DISTRIBUTED_SIGNAL = """
public class DistributedSignal : Signal, ISignal
{
    // ...
    Task ISignal.SignalTokenAsync(string key)
    {
        SignalToken(key);
        return _messageBus.PublishAsync("Signal", key);
    }
}
"""

BUS_PUBLISH = """
// _messagePrefix is the host name and the process ID of this server, then '/'.
await _redis.Connection.GetSubscriber().PublishAsync(
    RedisChannel.Literal(_channelPrefix + channel), _messagePrefix + message);
"""

BUS_SUBSCRIBE = """
await subscriber.SubscribeAsync(RedisChannel.Literal(_channelPrefix + channel), (redisChannel, redisValue) =>
{
    var tokens = redisValue.ToString().Split('/').ToArray();

    if (tokens.Length != 2 || tokens[0].Length == 0 || tokens[0].Equals(_hostName, StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    handler(channel, tokens[1]);
});
"""

LOCK_STARTUP = """
public sealed class RedisLockStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        if (services.IsRedisConfigured())
        {
            services.AddSingleton<IDistributedLock, RedisLock>();
        }
    }
}
"""

LOCK_TAKE = """
// _prefix is the instance prefix, the tenant name and ':'. _hostName is the host name and the process ID.
return await _redis.Database.LockTakeAsync(_prefix + key, _hostName, expiry);
"""

PROGRAM = """
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.Redis.Lock", "OrchardCore.AutoSetup");

// ...
"""

AUTOSETUP_ENVIRONMENT = """
{
  "OrchardCore__Redis__Configuration": "localhost:6379",
  "OrchardCore__AutoSetup__LockOptions__LockTimeout": "10000",
  "OrchardCore__AutoSetup__LockOptions__LockExpiration": "10000"
}
"""

ATOMIC_TASK = """
[BackgroundTask(
    Title = "Rebuild the report",
    Schedule = "*/5 * * * *",
    LockTimeout = 3_000,
    LockExpiration = 30_000)]
public sealed class ReportBackgroundTask : IBackgroundTask
{
    // ...
}
"""

KEY_RING = """
var redisKey = $"{redis.InstancePrefix}{_tenant}:DataProtection-Keys";

// ...

options.XmlRepository = new RedisXmlRepository(() =>
    {
        // ...
        return redis.Database;
    },
    redisKey);
"""

PERSISTENCE = """
if (_logger.IsEnabled(LogLevel.Warning) && !CheckRedisPersistenceEnabled())
{
    _logger.LogWarning("Redis data protection is enabled. Ensure your Redis server has a backup strategy ...");
}

// ...

private bool CheckRedisPersistenceEnabled()
{
    if (!_allowAdmin)
    {
        // Redis server admin commands are not allowed, we cannot check persistence.
        return false;
    }

    // ...
}
"""

COUNTER_CONSTRUCTOR = """
public sealed class CounterService
{
    private readonly IRedisService _redis;
    private readonly string _prefix;

    public CounterService(IRedisService redis, ShellSettings shellSettings)
    {
        _redis = redis;
        _prefix = redis.InstancePrefix + shellSettings.Name + ':';
    }
}
"""

REDIS_SERVICE = """
public interface IRedisService : IModularTenantEvents
{
    Task ConnectAsync();
    IConnectionMultiplexer Connection { get; }
    string InstancePrefix { get; }
    IDatabase Database { get; }
}
"""

COUNTER_METHOD = """
public async Task<long> IncrementAsync(string key)
{
    if (_redis.Database is null)
    {
        await _redis.ConnectAsync();

        if (_redis.Database is null)
        {
            return 0;
        }
    }
    try
    {
        return await _redis.Database.StringIncrementAsync(_prefix + key);
    }
    catch (Exception e)
    {
        // ...
        return 0;
    }
}
"""

COUNTER_STARTUP = """
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        if (services.IsRedisConfigured())
        {
            services.AddSingleton<ICounterStore, RedisCounterStore>();
        }
    }
}
"""

IS_REDIS_CONFIGURED = """
public static bool IsRedisConfigured(this IServiceCollection services)
    => services.Any(descriptor =>
        descriptor.ServiceType == typeof(IRedisService) &&
        descriptor.GetImplementationType() != typeof(NullRedisService));
"""


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Local to each server")
        bullets(image, LOCAL_STATE)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "One connection, four features")
        bullets(image, REDIS_FEATURES)
        return image

    if step_id == "connect-1":
        image = slide_base(chapter_title, label, "Start Redis, then connect")
        draw_code(image, (96, 240, 1150, 730), "appsettings.json", "json", SETTINGS, types=TYPES, size=26,
                  highlight={1, 2, 3, 4, 8, 9, 10})
        draw_code(image, (96, 770, 1150, 930), "Terminal", "bash", DOCKER, types=TYPES, size=26)
        note_cards(image, [
            ("Configuration", "The connection string."),
            ("The only required value", "Every other value is optional."),
        ], (1190, 300, W - 96, 1030))
        return image

    if step_id == "connect-2":
        image = slide_base(chapter_title, label, "Optional values")
        draw_code(image, (96, 240, 1150, 730), "appsettings.json", "json", SETTINGS, types=TYPES, size=26,
                  highlight={1, 2, 3, 5, 6, 7, 8, 9, 10})
        draw_code(image, (96, 770, 1150, 930), "Terminal", "bash", DOCKER, types=TYPES, size=26, highlight=set())
        note_cards(image, [
            ("InstancePrefix", "Starts every key, such as MyApp:"),
            ("AllowAdmin", "Permits admin commands."),
            ("DisableCertificateVerification", "Accepts a self-signed certificate."),
        ], (1190, 300, W - 96, 1030))
        return image

    if step_id == "connect-3":
        image = slide_base(chapter_title, label, "Shared connection, separate keys")
        bullets(image, TENANT_KEYS)
        return image

    if step_id == "connect-4":
        image = slide_base(chapter_title, label, "Missing or invalid Configuration")
        bullets(image, NOT_CONFIGURED)
        return image

    if step_id == "cache-1":
        image = slide_base(chapter_title, label, "Redis Cache")
        draw_code(image, (96, 240, 1250, 890), "RedisCacheStartup.cs", "csharp", CACHE_STARTUP, types=TYPES, size=24,
                  highlight={1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 15, 16})
        note_cards(image, [
            ("Documents", "Site settings, roles and more."),
            ("Dynamic Cache", "Its rendered output."),
        ], (1290, 300, W - 96, 1030))
        return image

    if step_id == "cache-2":
        image = slide_base(chapter_title, label, "Redis Cache")
        draw_code(image, (96, 240, 1250, 890), "RedisCacheStartup.cs", "csharp", CACHE_STARTUP, types=TYPES, size=24,
                  highlight={1, 2, 3, 4, 5, 6, 7, 13, 14, 15, 16})
        note_cards(image, [
            ("Tag cache", "The cached items of each tag."),
            ("Kept in Redis", "Seen by every server."),
        ], (1290, 300, W - 96, 1030))
        return image

    if step_id == "bus-1":
        image = slide_base(chapter_title, label, "Redis Bus")
        draw_code(image, (96, 240, W - 96, 650), "RedisBusStartup.cs", "csharp", BUS_STARTUP, types=TYPES, size=22)
        draw_code(image, (96, 670, W - 96, 1040), "DistributedSignal.cs", "csharp", DISTRIBUTED_SIGNAL, types=TYPES, size=22)
        return image

    if step_id == "bus-2":
        image = slide_base(chapter_title, label, "One channel for every server")
        draw_code(image, (96, 240, W - 96, 470), "RedisBus.cs: publish", "csharp", BUS_PUBLISH, types=TYPES, size=22)
        draw_code(image, (96, 500, W - 96, 1030), "RedisBus.cs: subscribe", "csharp", BUS_SUBSCRIBE, types=TYPES, size=22)
        return image

    if step_id == "lock-1":
        image = slide_base(chapter_title, label, "Redis Lock replaces the local lock")
        draw_code(image, (96, 240, W - 96, 660), "RedisLockStartup.cs", "csharp", LOCK_STARTUP, types=TYPES, size=22)
        draw_code(image, (96, 700, W - 96, 900), "RedisLock.cs", "csharp", LOCK_TAKE, types=TYPES, size=22)
        return image

    if step_id == "lock-2":
        image = slide_base(chapter_title, label, "Locking the setup of a site")
        draw_code(image, (96, 240, 1150, 560), "Program.cs", "csharp", PROGRAM, types=TYPES, size=22)
        draw_code(image, (96, 600, 1150, 900), "Environment variables", "json", AUTOSETUP_ENVIRONMENT, types=TYPES, size=22)
        note_cards(image, [
            ("The lock name", "AUTOSETUP_LOCK"),
            ("Both in milliseconds", "Sixty seconds when not set."),
            ("Set them together", "Each one above zero."),
        ], (1190, 300, W - 96, 1030))
        return image

    if step_id == "lock-3":
        image = slide_base(chapter_title, label, "Atomic background tasks")
        draw_code(image, (96, 240, 1100, 700), "ReportBackgroundTask.cs", "csharp", ATOMIC_TASK, types=TYPES, size=24)
        note_cards(image, [
            ("Atomic task", "Both values above zero."),
            ("Without a distributed lock", "Both values are ignored."),
            ("The lock name", "The full type name, then _LOCK."),
        ], (1140, 300, W - 96, 1030))
        return image

    if step_id == "protect-1":
        image = slide_base(chapter_title, label, "What the keys protect")
        bullets(image, PROTECTED)
        return image

    if step_id == "protect-2":
        image = slide_base(chapter_title, label, "Keys in Redis")
        draw_code(image, (96, 240, 1150, 760), "RedisKeyManagementOptionsSetup.cs", "csharp", KEY_RING, types=TYPES, size=22)
        note_cards(image, [
            ("Key name", "MyApp:Default:DataProtection-Keys"),
            ("Persistence", "AOF or RDB, not a cache."),
            ("Backup", "Keep a backup strategy in place."),
        ], (1190, 300, W - 96, 1030))
        return image

    if step_id == "protect-3":
        image = slide_base(chapter_title, label, "Checking persistence")
        draw_code(image, (96, 240, W - 96, 900), "RedisKeyManagementOptionsSetup.cs", "csharp", PERSISTENCE, types=TYPES, size=22)
        return image

    if step_id == "module-1":
        image = slide_base(chapter_title, label, "Take IRedisService")
        draw_code(image, (96, 240, W - 96, 690), "CounterService.cs", "csharp", COUNTER_CONSTRUCTOR, types=TYPES, size=22)
        draw_code(image, (96, 720, W - 96, 1040), "IRedisService.cs", "csharp", REDIS_SERVICE, types=TYPES, size=22)
        return image

    if step_id == "module-2":
        image = slide_base(chapter_title, label, "Connect when needed")
        draw_code(image, (96, 240, W - 96, 1010), "CounterService.cs", "csharp", COUNTER_METHOD, types=TYPES, size=22)
        return image

    if step_id == "module-3":
        image = slide_base(chapter_title, label, "Replace a default only when Redis is configured")
        draw_code(image, (96, 240, W - 96, 660), "Startup.cs", "csharp", COUNTER_STARTUP, types=TYPES, size=22)
        draw_code(image, (96, 700, W - 96, 1000), "RedisServiceCollectionExtensions.cs", "csharp", IS_REDIS_CONFIGURED,
                  types=TYPES, size=22)
        return image

    raise SystemExit(f"No slide for {step_id}")
