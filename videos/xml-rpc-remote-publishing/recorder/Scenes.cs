using System.Net.Http;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of "XML-RPC and Remote Publishing". Order: 02-enable, 03-discover, then "prepare" (it plays the part of
/// the blogging client and sends two posts), then 05-post.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-enable", EnableAsync),
        ("03-discover", DiscoverAsync),
        ("05-post", PostAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    // The stand-in for the blogging client: sends the XML-RPC calls of a desktop client to /xmlrpc, as admin.
    private static async Task<string> CallAsync(HttpClient client, string method, string parameters)
    {
        var body = $"<?xml version=\"1.0\"?><methodCall><methodName>{method}</methodName><params>{parameters}</params></methodCall>";
        var response = await client.PostAsync(BaseUrl + "/xmlrpc", new StringContent(body, Encoding.UTF8, "text/xml"));
        return await response.Content.ReadAsStringAsync();
    }

    private static string Text(string value) => $"<param><value><string>{SecurityElement.Escape(value)}</string></value></param>";

    /// <summary>Plays the part of the blogging client: sends a draft and a published post (run it once, after 03-discover).</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        using var handler = new HttpClientHandler { UseProxy = false };
        using var client = new HttpClient(handler);
        var blogs = await CallAsync(client, "blogger.getUsersBlogs", Text("appkey") + Text(UserName) + Text(Password));
        var blogId = Regex.Match(blogs, "<name>blogid</name>\\s*<value>\\s*<string>([^<]+)</string>").Groups[1].Value;

        async Task<string> NewPostAsync(string title, string description, bool publish)
        {
            var members = $"<member><name>title</name><value><string>{SecurityElement.Escape(title)}</string></value></member><member><name>description</name><value><string>{SecurityElement.Escape(description)}</string></value></member>";
            var structure = $"<param><value><struct>{members}</struct></value></param>";
            var flag = $"<param><value><boolean>{(publish ? 1 : 0)}</boolean></value></param>";
            return await CallAsync(client, "metaWeblog.newPost", Text(blogId) + Text(UserName) + Text(Password) + structure + flag);
        }

        var draft = await NewPostAsync("Draft written offline", "This draft waits for review.", false);
        var published = await NewPostAsync("Hello from a desktop client", "This post was written in a desktop blogging client and sent to the site.", true);

        if (draft.Contains("fault") || published.Contains("fault"))
        {
            throw new InvalidOperationException("The stand-in call failed: " + draft + published);
        }

        Console.WriteLine("Posts sent.");
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static async Task WaitTextAsync(IPage page, string text, int timeout = 60_000)
        => await page.WaitForFunctionAsync("t => document.body.innerText.includes(t)", text, new PageWaitForFunctionOptions { Timeout = timeout });

    // Clicks a link or button that loads a new page; the load is cut out of the video (see Scene.OffCameraAsync).
    private static async Task GoAsync(Scene scene, ILocator locator)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
    }

    private static async Task EnableAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        await scene.StepAsync("enable-1", async () =>
        {
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("#search-box"), "Open Live Writer", delay: 40);
            await page.Locator("#btn-enable-OrchardCore_RemotePublishing").WaitForAsync();
            await scene.HighlightAsync(page.Locator("h3").Filter(new() { HasTextString = "Infrastructure" }).First, "Infrastructure");
            await scene.PauseAsync(2000);
            await scene.ClearHighlightsAsync();
            var row = page.Locator("li.list-group-item").Filter(new() { Has = page.Locator("#btn-enable-OrchardCore_RemotePublishing") }).First;
            await scene.HighlightAsync(row.Locator(".direct-dependencies").First, "Dependency");
        });

        await scene.StepAsync("enable-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, page.Locator("#btn-enable-OrchardCore_RemotePublishing"));
            await WaitTextAsync(page, "The feature Remote Publishing was enabled.");
            await scene.ClickAsync(page.Locator("#search-box"), pauseAfter: 100);
            await scene.TypeAsync(page.Locator("#search-box"), "Open Live Writer", delay: 30);
        });
    }

    private static async Task DiscoverAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;

        // The recorder's cursor styles are added to every document: in an XML document they would show as text, so they are removed there.
        await page.AddInitScriptAsync("if (document.contentType && document.contentType.includes('xml')) { const clean = () => { document.getElementById('demo-cursor')?.remove(); document.querySelectorAll('style').forEach(s => { if (s.textContent.includes('demo-cursor')) s.remove(); }); }; setInterval(clean, 30); }");
        await scene.OpenAsync("/blog");
        await page.WaitForFunctionAsync("() => !!document.querySelector('link[rel=EditURI]')");
        var rsd = await page.EvaluateAsync<string>("() => document.querySelector('link[rel=EditURI]').href");
        var rsdPath = new Uri(rsd).AbsolutePath;
        await scene.StartCaptureAsync();

        await scene.StepAsync("discover-1", async () =>
        {
            await scene.PauseAsync(2500);
            await scene.NavigateAsync("/xmlrpc/metaweblog");
            await scene.MoveToAsync(700, 300);
        });

        await scene.StepAsync("discover-2", async () =>
        {
            // The browser shows an XML document as a tree without a body: a ring can't be drawn there, so the page is left as it is.
            await scene.MoveToAsync(500, 200);
            await scene.PauseAsync(3000);
            await scene.MoveToAsync(700, 350);
            await scene.PauseAsync(3000);
            await scene.MoveToAsync(600, 250);
        });

        await scene.StepAsync("discover-3", async () =>
        {
            await scene.NavigateAsync(rsdPath);
            await scene.MoveToAsync(700, 300);
        });
    }

    private static async Task PostAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems/BlogPost");
        await scene.StartCaptureAsync();

        await scene.StepAsync("post-1", async () =>
        {
            var row = page.Locator("li.list-group-item").Filter(new() { HasTextString = "Hello from a desktop client" }).First;
            await scene.MoveToAsync(row);
            await scene.HighlightAsync(row);
        });

        await scene.StepAsync("post-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = page.Locator("li.list-group-item").Filter(new() { HasTextString = "Draft written offline" }).First;
            await scene.MoveToAsync(row);
            await scene.HighlightAsync(row);
        });
    }
}
