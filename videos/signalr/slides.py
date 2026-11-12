"""The slides of the SignalR video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, MUTED, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"Hub", "WorkflowsHub", "MediaHub", "IHubContext", "HubOptions", "StartupBase", "IApplicationBuilder",
         "IEndpointRouteBuilder", "IServiceProvider", "AuthorizationOptions", "PermissionRequirement",
         "IdentityConstants", "OrchardCoreConstants", "WorkflowsPermissions", "WorkflowsRealTime", "RealTimeStartup",
         "IMediaEventHandler", "MediaCreatedContext", "MediaSignalREventHandler", "MediaPermissions",
         "IAuthorizationService", "MyHub", "Task", "TimeSpan", "Feature", "Authorize", "RequireFeatures"}

WHY_HUB = [
    ("1", "A connection stays open", "Between the browser and the server."),
    ("2", "The server sends", "A message to one client, a group or every client, when something changes."),
    ("3", "A hub receives the connections", "It is the class whose methods the clients can call."),
]

WHY_MODULE = [
    ("1", "Hosting", "Registers SignalR once for the tenant, with a camel-cased JSON protocol."),
    ("2", "The client", "Provides the JavaScript client as the signalr resource."),
    ("3", "Backplanes", "Redis and Azure, each in a separate module."),
    ("4", "Hubs stay with their modules", "Each module that hosts a hub maps it and sets its access rules."),
]

HUB = """
[Authorize(Policy = PolicyName)]
public sealed class WorkflowsHub : Hub
{
    public const string PolicyName = "WorkflowsHub";

    // ...
}
"""

POLICY = """
// In ConfigureServices:
services.Configure<AuthorizationOptions>(options =>
    options.AddPolicy(WorkflowsHub.PolicyName, policy =>
    {
        policy.AddAuthenticationSchemes(
            OrchardCoreConstants.AuthenticationSchemes.Api,
            IdentityConstants.ApplicationScheme);
        policy.Requirements.Add(
            new PermissionRequirement(WorkflowsPermissions.ManageWorkflows));
        policy.RequireAuthenticatedUser();
    }));
"""

MANIFEST = """
[assembly: Feature(
    Id = "OrchardCore.Media.SignalR",
    Name = "Media SignalR",
    // ...
    Dependencies =
    [
        "OrchardCore.Media",
        "OrchardCore.SignalR"
    ]
)]
"""

STARTUP = """
[RequireFeatures("OrchardCore.SignalR")]
public sealed class RealTimeStartup : StartupBase
{
    // ...
    public override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider)
    {
        routes.MapHub<WorkflowsHub>(WorkflowsRealTime.HubPath);
    }
}
"""

HUB_OPTIONS = """
// One hub:
services.Configure<HubOptions<MyHub>>(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(10);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// Every hub:
services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 128 * 1024;
});
"""

MEDIA_HUB = """
[Authorize(Policy = MediaApiConstants.HubAuthorizationPolicyName)]
public sealed class MediaHub : Hub
{
    // ...
    public async Task SubscribePath(string path)
    {
        if (!await _authorizationService.AuthorizeAsync(
            Context.User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return;
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId, GetFolderGroupName(path));
    }
}
"""

MEDIA_HANDLER = """
public class MediaSignalREventHandler : IMediaEventHandler
{
    private readonly IHubContext<MediaHub> _hubContext;

    // ...
    public Task MediaCreatedFileAsync(MediaCreatedContext context)
        => _hubContext.Clients.Group(FolderGroup(context.Path))
            .SendAsync("MediaChanged", new
            {
                action = "fileUploaded",
                path = context.Path,
            });
}
"""

CLIENT = """
<script asp-name="signalr" at="Foot"></script>

<script type="text/javascript" at="Foot" depends-on="signalr">
    document.addEventListener("DOMContentLoaded", function () {
        var connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/my-hub")
            .build();

        connection.start()
            .then(function () { console.log("Connected to the SignalR hub."); })
            .catch(function (error) { console.error("Connection failed:", error.message); });
    });
</script>
"""

CLIENT_CHOICES = [
    ("1", "Server-rendered pages", "Load the signalr resource, then use the global signalR object."),
    ("2", "Bundled apps", "Include the client in the bundle, as the Media gallery does."),
]

BACKPLANE = [
    ("1", "Instance A", "Holds the connections of some of the browsers in its memory."),
    ("2", "Instance B", "Holds the connections of the other browsers."),
    ("3", "Backplane", "Relays each hub message to every instance of the tenant."),
]

REDIS = """
{
  "OrchardCore": {
    "Redis": {
      "Configuration": "your-redis-host:6379"
    }
  }
}
"""

AZURE = """
{
  "OrchardCore": {
    "SignalR": {
      "Azure": {
        "ConnectionString": "Endpoint=https://signalr.example.com;AccessKey=...;Version=1.0;",
        "ApplicationName": "OrchardCore"
      }
    }
  }
}
"""

RULES = [
    ("1", "One backplane per tenant", "If both are enabled and configured, one logs an error and stays inactive."),
    ("2", "No connection string", "The backplane stays inactive, and a message is logged at startup."),
    ("3", "In-memory fallback", "SignalR keeps working, but messages reach only the same instance."),
]


def code_height(code, size, extra=24):
    """The height of a code panel that holds code at the given size."""
    return 74 + len(code.strip("\n").split("\n")) * int(size * 1.42) + extra


def notes(image, lines, x, y):
    """Dotted lines of text, as the worked example does under a code panel."""
    draw = ImageDraw.Draw(image)
    for line in lines:
        draw.ellipse((x, y - 7, x + 14, y + 7), fill=GREEN)
        draw.text((x + 36, y), line, font=font(30), fill=TEXT, anchor="lm")
        y += 60


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "A hub pushes messages to the browser")
        bullets(image, WHY_HUB)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "The SignalR module")
        bullets(image, WHY_MODULE)
        return image

    if step_id == "host-1":
        image = slide_base(chapter_title, label, "1. Declare the hub and its policy")
        draw_code(image, (96, 240, 790, 240 + code_height(HUB, 22)), "WorkflowsHub.cs", "csharp", HUB,
                  types=TYPES, size=22)
        draw_code(image, (814, 240, W - 96, 240 + code_height(POLICY, 22)), "RealTimeStartup.cs", "csharp", POLICY,
                  types=TYPES, size=22)
        return image

    if step_id == "host-2":
        image = slide_base(chapter_title, label, "2. Require SignalR, and map the hub")
        draw_code(image, (96, 240, 690, 240 + code_height(MANIFEST, 26)), "OrchardCore.Media/Manifest.cs", "csharp",
                  MANIFEST, types=TYPES, size=26)
        draw_code(image, (720, 240, W - 96, 240 + code_height(STARTUP, 26)), "RealTimeStartup.cs", "csharp", STARTUP,
                  types=TYPES, size=26)
        return image

    if step_id == "host-3":
        image = slide_base(chapter_title, label, "3. Configure hub options")
        draw_code(image, (96, 240, W - 96, 240 + code_height(HUB_OPTIONS, 28)), "Startup.cs", "csharp", HUB_OPTIONS,
                  types=TYPES, size=28)
        return image

    if step_id == "send-1":
        image = slide_base(chapter_title, label, "Groups decide who receives a message")
        draw_code(image, (96, 240, W - 96, 240 + code_height(MEDIA_HUB, 27)), "MediaHub.cs", "csharp", MEDIA_HUB,
                  types=TYPES, size=27)
        return image

    if step_id == "send-2":
        image = slide_base(chapter_title, label, "Send through the hub context")
        draw_code(image, (96, 240, W - 96, 240 + code_height(MEDIA_HANDLER, 27)), "MediaSignalREventHandler.cs",
                  "csharp", MEDIA_HANDLER, types=TYPES, size=27)
        return image

    if step_id == "client-1":
        image = slide_base(chapter_title, label, "Server-rendered pages: the signalr resource")
        draw_code(image, (96, 240, W - 96, 240 + code_height(CLIENT, 25)), "MyHub.cshtml", "html", CLIENT,
                  types=TYPES, size=25)
        return image

    if step_id == "client-2":
        image = slide_base(chapter_title, label, "Two ways to reach a hub from the browser")
        bullets(image, CLIENT_CHOICES, active={1})
        return image

    if step_id == "scale-1":
        image = slide_base(chapter_title, label, "Why a backplane")
        bullets(image, BACKPLANE)
        return image

    if step_id == "scale-2":
        image = slide_base(chapter_title, label, "The Redis backplane")
        draw_code(image, (96, 240, 1100, 240 + code_height(REDIS, 28)), "appsettings.json", "json", REDIS,
                  types=TYPES, size=28)
        note_cards(image, [
            ("Enable the feature", "SignalR Redis Backplane"),
            ("The Redis module's string", "OrchardCore:Redis:Configuration"),
            ("A channel prefix per tenant", "Instance prefix, tenant, SignalR"),
        ], (1140, 240, W - 96, 1030))
        return image

    if step_id == "scale-3":
        image = slide_base(chapter_title, label, "The Azure SignalR Service backplane")
        bottom = 240 + code_height(AZURE, 26)
        draw_code(image, (96, 240, W - 96, bottom), "appsettings.json", "json", AZURE, types=TYPES, size=26)
        notes(image, [
            "Enable the feature SignalR Azure Backplane.",
            "ConnectionString is required.",
            "ApplicationName is optional, and defaults to OrchardCore.",
            "A hash of the tenant name is added to it.",
        ], 110, bottom + 60)
        return image

    if step_id == "scale-4":
        image = slide_base(chapter_title, label, "Rules for backplanes")
        bullets(image, RULES)
        return image

    raise SystemExit(f"No slide for {step_id}")
