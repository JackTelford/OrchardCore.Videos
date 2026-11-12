"""The slides of the XML-RPC and Remote Publishing video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code
panels hold code of the Orchard Core source, trimmed with // ...
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {"HtmlBodyMetaWeblogDriver", "MetaWeblogDriver", "XRpcStruct", "XmlRpcContext", "ContentItem", "HtmlBodyPart",
         "AutorouteMetaWeblogDriver", "RemotePublishingStartup", "StartupBase", "IServiceCollection", "Action",
         "IMetaWeblogDriver", "IXmlRpcHandler", "MetaWeblogHandler", "XRpcMethodResponse", "Task", "RequireFeatures"}

FEATURES = [
    ("1", "XML-RPC", "Accepts a POST at /xmlrpc, runs the handlers, writes the XML response."),
    ("2", "Remote Publishing", "Adds the MetaWeblog API on top of XML-RPC."),
]

ANSWERS = [
    ("1", "The Lists module", "Answers the calls when Remote Publishing is enabled."),
    ("2", "A blog", "A content item whose type has a List part."),
    ("3", "A post", "An item that the list contains."),
]

METHODS = [
    ("1", "Find the blogs", "blogger.getUsersBlogs"),
    ("2", "Read posts", "metaWeblog.getRecentPosts, metaWeblog.getPost"),
    ("3", "Write posts", "metaWeblog.newPost, metaWeblog.editPost, blogger.deletePost"),
    ("4", "Upload media", "metaWeblog.newMediaObject"),
]

REQUEST = """
<methodCall>
  <methodName>metaWeblog.newPost</methodName>
  <params>
    <param><value><string>BLOG_ID</string></value></param>
    <param><value><string>admin</string></value></param>
    <param><value><string>PASSWORD</string></value></param>
    <param><value><struct>
      <member>
        <name>title</name>
        <value><string>Hello from a desktop client</string></value>
      </member>
      <!-- ... -->
    </struct></value></param>
    <param><value><boolean>1</boolean></value></param>
  </params>
</methodCall>
"""

MEMBERS = [
    ("1", "title", "The Title module: a posted title becomes the display text."),
    ("2", "description", "The Markdown and Html modules: the body of the item."),
    ("3", "wp_slug", "The Autoroute module: the path of the item."),
]

PERMISSIONS = [
    ("1", "User name and password", "Every call carries them, and the site checks them on every call."),
    ("2", "Edit content for others", "List the blogs, read posts, and save a draft."),
    ("3", "Publish or unpublish content for others", "Publish a post, or edit it with the publish flag."),
    ("4", "Delete content for others", "Delete a post. Uploading a file needs the media permissions."),
]

DRIVER = """
public sealed class HtmlBodyMetaWeblogDriver : MetaWeblogDriver
{
    public override void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
    {
        if (!contentItem.TryGet<HtmlBodyPart>(out var bodyPart))
        {
            return;
        }

        rpcStruct.Set("description", bodyPart.Html);
    }

    public override void EditPost(XRpcStruct rpcStruct, ContentItem contentItem)
    {
        if (contentItem.TryGet<HtmlBodyPart>(out _))
        {
            contentItem.Alter<HtmlBodyPart>(x => x.Html = rpcStruct.Optional<string>("description"));
        }
    }
}
"""

CAPABILITIES = """
public sealed class AutorouteMetaWeblogDriver : MetaWeblogDriver
{
    public override void SetCapabilities(Action<string, string> setCapability)
    {
        setCapability("supportsSlug", "Yes");
    }

    // ...
}
"""

DRIVER_STARTUP = """
[RequireFeatures("OrchardCore.RemotePublishing")]
public sealed class RemotePublishingStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IMetaWeblogDriver, AutorouteMetaWeblogDriver>();
    }
}
"""

HANDLER = """
[RequireFeatures("OrchardCore.RemotePublishing")]
public class MetaWeblogHandler : IXmlRpcHandler
{
    public async Task ProcessAsync(XmlRpcContext context)
    {
        if (context.RpcMethodCall.MethodName == "metaWeblog.getRecentPosts")
        {
            // ...
            context.RpcMethodResponse = new XRpcMethodResponse().Add(result);
        }

        // ...
    }

    // ...
}
"""

HANDLER_STARTUP = """
// ConfigureServices
services.AddScoped<IXmlRpcHandler, MetaWeblogHandler>();
"""


def slide(step_id, chapter_title, label):
    if step_id == "features-1":
        image = slide_base(chapter_title, label, "Two features in one module")
        bullets(image, FEATURES)
        return image

    if step_id == "features-2":
        image = slide_base(chapter_title, label, "Who answers the calls")
        bullets(image, ANSWERS)
        return image

    if step_id == "calls-1":
        image = slide_base(chapter_title, label, "The methods of the Lists module")
        bullets(image, METHODS)
        return image

    if step_id == "calls-2":
        image = slide_base(chapter_title, label, "A call to create a post")
        draw_code(image, (96, 240, 1230, 980), "newPost request", "xml", REQUEST, size=24)
        note_cards(image, [
            ("Blog identifier", "The id of the list item."),
            ("Post members", "title, description, wp_slug"),
            ("Publish flag", "1: publish. 0: draft."),
        ], (1270, 240, W - 96, 980))
        return image

    if step_id == "calls-3":
        image = slide_base(chapter_title, label, "Each module maps its members")
        bullets(image, MEMBERS)
        return image

    if step_id == "calls-4":
        image = slide_base(chapter_title, label, "Credentials and permissions")
        bullets(image, PERMISSIONS)
        return image

    if step_id == "extend-1":
        image = slide_base(chapter_title, label, "1. A driver maps a part")
        draw_code(image, (96, 240, W - 96, 1030), "HtmlBodyMetaWeblogDriver.cs", "csharp", DRIVER, types=TYPES, size=24)
        return image

    if step_id == "extend-2":
        image = slide_base(chapter_title, label, "2. Declare capabilities, then register")
        draw_code(image, (96, 240, W - 96, 630), "AutorouteMetaWeblogDriver.cs", "csharp", CAPABILITIES, types=TYPES, size=24)
        draw_code(image, (96, 660, W - 96, 1030), "RemotePublishingStartup.cs", "csharp", DRIVER_STARTUP, types=TYPES, size=24)
        return image

    if step_id == "extend-3":
        image = slide_base(chapter_title, label, "3. A handler answers methods")
        draw_code(image, (96, 240, W - 96, 870), "MetaWeblogHandler.cs", "csharp", HANDLER, types=TYPES, size=24)
        draw_code(image, (96, 890, W - 96, 1030), "RemotePublishingStartup.cs", "csharp", HANDLER_STARTUP, types=TYPES, size=22)
        return image

    raise SystemExit(f"No slide for {step_id}")
