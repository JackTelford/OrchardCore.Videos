using Microsoft.Playwright;

namespace Recorder;

/// <summary>The clips of "Headless front end", recorded in order on one fresh site.</summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-graphql", GraphqlAsync),
        ("03-cors", CorsAsync),
        ("04-openid", OpenIdAsync),
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

    private const string FrontEndHtml = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <title>Sample Front End</title>
          <style>
            body { font-family: "Open Sans", Arial, sans-serif; font-size: 24px; margin: 56px; color: #404041; }
            h1 { font-size: 40px; margin: 0 0 8px 0; }
            .origins { color: #707070; font-size: 22px; margin-bottom: 32px; }
            button { font-size: 24px; padding: 12px 28px; cursor: pointer; }
            #status { margin: 28px 0; min-height: 36px; }
            #status.error { color: #b00020; }
            li { margin: 10px 0; }
          </style>
        </head>
        <body>
          <h1>Sample Front End</h1>
          <div class="origins">This page: __ORIGIN__ &middot; Content from: __BASE__</div>
          <button id="load">Load posts</button>
          <p id="status">Nothing loaded.</p>
          <ul id="posts"></ul>
          <script>
            const endpoint = "__BASE__/api/graphql";
            const status = document.getElementById("status");
            const posts = document.getElementById("posts");
        
            document.getElementById("load").addEventListener("click", async () => {
              posts.innerHTML = "";
              status.className = "";
              status.textContent = "Loading...";
              try {
                const response = await fetch(endpoint, {
                  method: "POST",
                  headers: { "Content-Type": "application/json" },
                  body: JSON.stringify({ query: "{ blogPost { displayText } }" })
                });
                const result = await response.json();
                if (result.errors) {
                  throw new Error(result.errors[0].message);
                }
                status.textContent = "Loaded " + result.data.blogPost.length + " posts.";
                for (const post of result.data.blogPost) {
                  const item = document.createElement("li");
                  item.textContent = post.displayText;
                  posts.appendChild(item);
                }
              } catch (error) {
                status.className = "error";
                status.textContent = "The browser blocked the request: " + error.message;
              }
            });
          </script>
        </body>
        </html>
        """;

    private const string PostsJson = """
        {"steps":[{"name":"content","data":[
          {"ContentType":"BlogPost","ContentItemId":"[js: uuid()]","DisplayText":"Summer Menu Preview","Latest":true,"Published":true,"TitlePart":{"Title":"Summer Menu Preview"},"MarkdownBodyPart":{"Markdown":"Cold brew, lemonade and fruit tarts arrive next week."}},
          {"ContentType":"BlogPost","ContentItemId":"[js: uuid()]","DisplayText":"A Morning at the Roastery","Latest":true,"Published":true,"TitlePart":{"Title":"A Morning at the Roastery"},"MarkdownBodyPart":{"Markdown":"The beans are roasted in small batches before the doors open."}}
        ]}]}
        """;

    // The stand-in front end: a one-page static site on the loopback address 127.0.0.2, on the same port as the site, so it is another origin than the site (localhost).
    private static string FrontEndOrigin => "http://127.0.0.2:" + new Uri(BaseUrl).Port;

    private sealed class FrontEnd : IDisposable
    {
        private readonly System.Net.Sockets.TcpListener _listener;
        private readonly byte[] _response;

        public FrontEnd()
        {
            var html = FrontEndHtml.Replace("__BASE__", BaseUrl).Replace("__ORIGIN__", FrontEndOrigin);
            var body = System.Text.Encoding.UTF8.GetBytes(html);
            var head = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: " + body.Length + "\r\nConnection: close\r\n\r\n");
            _response = head.Concat(body).ToArray();
            _listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Parse("127.0.0.2"), new Uri(BaseUrl).Port);
            _listener.Start();
            _ = Task.Run(AcceptAsync);
        }

        private async Task AcceptAsync()
        {
            try
            {
                while (true)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = Task.Run(async () =>
                    {
                        using (client)
                        {
                            var stream = client.GetStream();
                            var buffer = new byte[4096];
                            var read = await stream.ReadAsync(buffer);
                            var request = System.Text.Encoding.ASCII.GetString(buffer, 0, read);
                            var favicon = request.StartsWith("GET /favicon");
                            if (favicon)
                            {
                                var none = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 404 Not Found\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                                await stream.WriteAsync(none);
                            }
                            else
                            {
                                await stream.WriteAsync(_response);
                            }
                        }
                    });
                }
            }
            catch (Exception)
            {
            }
        }

        public void Dispose() => _listener.Stop();
    }

    /// <summary>Two more blog posts (the query returns three), outside the capture.</summary>
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US", ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60_000);

        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json");
        await page.WaitForFunctionAsync("() => !!document.querySelector('.CodeMirror')");
        await page.EvaluateAsync("j => document.querySelector('.CodeMirror').CodeMirror.setValue(j)", PostsJson);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await WaitTextAsync(page, "Recipe imported successfully!");

        foreach (var path in new[] { "/Admin/Features", "/Admin/Contents/ContentItems", "/Admin/Roles/Edit/Anonymous", "/" })
        {
            await page.GotoAsync(BaseUrl + path);
        }

        await context.CloseAsync();
        Console.WriteLine("Posts created.");
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

    // Opens a page during the capture, with the load cut out of the video.
    private static Task VisitAsync(Scene scene, string path)
        => scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.GotoAsync(BaseUrl + path)), leadMilliseconds: 250);

    private static async Task ShowAsync(Scene scene, ILocator locator, int ms = 1000)
    {
        await scene.HighlightAsync(locator);
        await scene.PauseAsync(ms);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator Row(IPage page, string text) => page.Locator(".list-group-item, tr").Filter(new() { HasTextString = text }).First;

    private static async Task OpenAdminGroupAsync(IPage page, string name)
    {
        var group = page.Locator("#left-nav").GetByText(name, new() { Exact = true }).First;
        await group.ClickAsync();
        await Task.Delay(500);
    }

    private static async Task<Scene> StartAtAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations, string path, Func<IPage, Task> ready = null)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        await scene.OpenAsync(path);
        if (ready != null)
        {
            await ready(scene.Page);
        }

        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 700);
        return scene;
    }

    private static async Task GoOffAsync(Scene scene, ILocator locator, int lead = 250)
    {
        await scene.MoveToAsync(locator);
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: lead);
    }

    private static ILocator FeatureRow(IPage page, string id) => page.Locator(".list-group-item").Filter(new() { Has = page.Locator($"#btn-enable-{id}") }).First;

    private static async Task SearchAsync(Scene scene, string text, int delay = 70)
    {
        await scene.ClickAsync(scene.Page.Locator("#search-box"), pauseAfter: 150);
        await scene.Page.Keyboard.PressAsync("Control+A");
        await scene.Page.Keyboard.TypeAsync(text, new() { Delay = delay });
    }

    private static async Task GraphqlAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Features", p => p.Locator("#search-box").WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("graphql-1", async () =>
        {
            await SearchAsync(scene, "GraphQL", 90);
            await FeatureRow(page, "OrchardCore_Apis_GraphQL").WaitForAsync();
            await ShowAsync(scene, FeatureRow(page, "OrchardCore_Apis_GraphQL"), 1200);
            await GoOffAsync(scene, page.Locator("#btn-enable-OrchardCore_Apis_GraphQL"));
            await WaitTextAsync(page, "The feature GraphQL was enabled.", 120_000);
            await scene.MoveToAsync(900, 700);
        });

        await scene.StepAsync("graphql-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Roles/Edit/Anonymous");
            await scene.TypeAsync(page.Locator("#search-box"), "Execute GraphQL", delay: 60);
            var row = page.Locator("tr[data-text='Execute GraphQL.']").First;
            await row.WaitForAsync();
            await scene.ClickAsync(page.Locator("[id='Checkbox.ExecuteGraphQL']"), pauseAfter: 300);
            await ShowAsync(scene, row, 1200);
            await GoAsync(scene, page.Locator("button.save").First);
            await WaitTextAsync(page, "Role updated successfully.");
        });

        await scene.StepAsync("graphql-3", async () =>
        {
            await scene.NavigateAsync("/Admin/GraphQL?query=%7B%20blogPost%20%7B%20displayText%20%7D%20%7D");
            await page.WaitForFunctionAsync("() => document.body.innerText.includes('blogPost')");
            var prettify = page.GetByRole(AriaRole.Button, new() { Name = "Prettify" }).Or(page.Locator("button[title^='Prettify']")).First;
            if (await prettify.CountAsync() > 0)
            {
                await scene.ClickAsync(prettify, pauseAfter: 400);
            }

            await scene.ClickAsync(page.Locator("button.execute-button").First, pauseAfter: 300);
            await page.WaitForFunctionAsync("() => document.body.innerText.includes('Summer Menu Preview')");
        });
    }

    private static async Task CorsAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await NewSceneAsync(browser, directory, durations);
        await using var _ = scene;
        scene.Page.SetDefaultNavigationTimeout(120_000);
        scene.Page.SetDefaultTimeout(60_000);
        using var frontEnd = new FrontEnd();
        var app = scene.Page;
        await app.GotoAsync(FrontEndOrigin + "/", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var admin = await scene.Context.NewPageAsync();
        admin.SetDefaultTimeout(60_000);
        admin.SetDefaultNavigationTimeout(120_000);
        await admin.GotoAsync(BaseUrl + "/Admin/Features", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await app.BringToFrontAsync();
        await scene.StartCaptureAsync();
        await scene.MoveToAsync(900, 700);

        await scene.StepAsync("cors-1", async () =>
        {
            await scene.ClickAsync(app.GetByRole(AriaRole.Button, new() { Name = "Load posts" }), pauseAfter: 300);
            await app.WaitForFunctionAsync("() => document.getElementById('status').innerText.includes('blocked')");
            await scene.HighlightAsync(app.Locator("#status"));
        });

        await scene.StepAsync("cors-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await admin.BringToFrontAsync();
            await scene.SwitchToAsync(admin);
            await scene.ClickAsync(admin.Locator("#search-box"), pauseAfter: 150);
            await admin.Keyboard.TypeAsync("CORS", new() { Delay = 90 });
            await FeatureRow(admin, "OrchardCore_Cors").WaitForAsync();
            var enable = admin.Locator("#btn-enable-OrchardCore_Cors");
            await scene.MoveToAsync(enable);
            var (x, y) = await scene.CenterOfAsync(enable);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => admin.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
            await WaitTextAsync(admin, "The feature CORS Configuration was enabled.", 120_000);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => admin.GotoAsync(BaseUrl + "/Admin/Cors")), leadMilliseconds: 250);
            await admin.GetByRole(AriaRole.Button, new() { Name = "Add a policy" }).WaitForAsync();
        });

        await scene.StepAsync("cors-3", async () =>
        {
            await scene.ClickAsync(admin.GetByRole(AriaRole.Button, new() { Name = "Add a policy" }), pauseAfter: 400);
            var field = admin.Locator("#corsAdmin input[type=text]").First;
            await scene.ClickAsync(field, pauseAfter: 150);
            await admin.Keyboard.PressAsync("Control+A");
            await field.PressSequentiallyAsync("Frontend", new() { Delay = 55 });
            await scene.ClickAsync(admin.Locator("#set-default-policy"), pauseAfter: 300);
            await scene.ClickAsync(admin.Locator("#allow-credentials"), pauseAfter: 300);
        });

        await scene.StepAsync("cors-4", async () =>
        {
            var field = admin.Locator("#corsAdmin input[type=text]").Nth(1);
            await scene.TypeAsync(field, FrontEndOrigin, delay: 45);
            await scene.ClickAsync(admin.GetByRole(AriaRole.Button, new() { Name = "Add Origin" }), pauseAfter: 400);
            await ShowAsync(scene, admin.GetByText(FrontEndOrigin).First, 1200);
            var save = admin.Locator(".action-bar-bottom button").Filter(new() { HasTextString = "Save" }).First;
            await scene.MoveToAsync(save);
            var (x, y) = await scene.CenterOfAsync(save);
            await scene.OffCameraAsync(() => scene.WaitForNavigationAsync(() => admin.Mouse.ClickAsync((float)x, (float)y)), leadMilliseconds: 250);
            await WaitTextAsync(admin, "The CORS settings have updated successfully.", 120_000);
        });

        await scene.StepAsync("cors-5", async () =>
        {
            await app.BringToFrontAsync();
            await scene.SwitchToAsync(app);
            await WaitCorsReadyAsync();
            await scene.ClickAsync(app.GetByRole(AriaRole.Button, new() { Name = "Load posts" }), pauseAfter: 300);
            await app.WaitForFunctionAsync("() => document.getElementById('status').innerText.startsWith('Loaded') && document.querySelectorAll('#posts li').length === 3");
        });
    }

    // Waits until the site answers a preflight request with the CORS headers (the tenant reloads after the policy is saved).
    private static async Task WaitCorsReadyAsync()
    {
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
        for (var i = 0; i < 60; i++)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Options, BaseUrl + "/api/graphql");
                request.Headers.Add("Origin", FrontEndOrigin);
                request.Headers.Add("Access-Control-Request-Method", "POST");
                var response = await client.SendAsync(request);
                if (response.Headers.Contains("Access-Control-Allow-Origin"))
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(1000);
        }
    }

    private static async Task OpenIdAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAtAsync(browser, directory, durations, "/Admin/Features", p => p.Locator("#search-box").WaitForAsync());
        var page = scene.Page;

        await scene.StepAsync("openid-1", async () =>
        {
            await SearchAsync(scene, "OpenID", 90);
            await FeatureRow(page, "OrchardCore_OpenId_Server").WaitForAsync();
            await GoOffAsync(scene, page.Locator("#btn-enable-OrchardCore_OpenId_Server"));
            await WaitTextAsync(page, "was enabled", 180_000);
            await SearchAsync(scene, "OpenID", 90);
            await FeatureRow(page, "OrchardCore_OpenId_Validation").WaitForAsync();
            await GoOffAsync(scene, page.Locator("#btn-enable-OrchardCore_OpenId_Validation"));
            await WaitTextAsync(page, "The feature OpenID Connect Token Validation was enabled.", 180_000);
        });

        await scene.StepAsync("openid-2", async () =>
        {
            await scene.NavigateAsync("/Admin/OpenId/ServerConfiguration");
            await page.GetByText("Endpoints").First.WaitForAsync();
            await scene.ScrollAsync(0, 450);
            await scene.HighlightAsync(page.Locator("label").Filter(new() { HasTextString = "Allow Authorization Code Flow" }).First);
            await scene.HighlightAsync(page.Locator("label").Filter(new() { HasTextString = "Allow Refresh Token Flow" }).First);
        });

        await scene.StepAsync("openid-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.NavigateAsync("/Admin/OpenId/Application");
            await GoAsync(scene, page.Locator("a.create").First);
            await scene.TypeAsync(page.Locator("#DisplayName"), "Sample Front End", delay: 45);
            await scene.SelectAsync(page.Locator("[data-openid-field='Type']"), "public");
            await scene.TypeAsync(page.Locator("[data-openid-field='ClientId']"), "frontend", delay: 45);
        });

        await scene.StepAsync("openid-4", async () =>
        {
            await scene.ClickAsync(page.Locator("#AllowAuthorizationCodeFlow"), pauseAfter: 400);
            await page.Locator("#RedirectSection").WaitForAsync();
            await scene.TypeAsync(page.Locator("#RedirectUris"), FrontEndOrigin + "/callback.html", delay: 30);
            await scene.ClickAsync(page.Locator("#RequireProofKeyForCodeExchange"), pauseAfter: 300);
            await scene.ScrollAsync(0, 500);
            await scene.ClickAsync(page.Locator("#ScopeGroup label").Filter(new() { HasTextString = "profile" }).First, pauseAfter: 300);
            await scene.ClickAsync(page.Locator("#ScopeGroup label").Filter(new() { HasTextString = "roles" }).First, pauseAfter: 300);
            await GoAsync(scene, page.Locator("button.save").First);
        });

        await scene.StepAsync("openid-5", async () =>
        {
            await scene.NavigateAsync("/Admin/OpenId/ValidationConfiguration");
            var field = page.GetByLabel("Authorization server tenant");
            await field.WaitForAsync();
            await scene.HighlightAsync(field);
        });
    }
}

internal static class LocatorExtensions
{
    // Scrolls an element to the middle of the window, clear of the sticky button bar at the bottom of the editors.
    public static async Task ScrollToMiddleAsync(this ILocator locator)
    {
        await locator.EvaluateAsync("e => e.scrollIntoView({ block: 'center', behavior: 'instant' })");
        await Task.Delay(300);
    }
}
