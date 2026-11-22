using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Lists in Templates". PrepareAsync removes the sample post of the Blog recipe, then imports seed.json:
/// four invented posts in the blog (three published, one draft) and the template Content__BlogPost. Only 05-site is recorded.
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
        ("05-site", SiteAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // The ids of the blog and of its sample post, read from the admin lists.
        var ids = await page.EvaluateAsync<string[]>(@"async () => {
            const token = document.querySelector('input[name=""__RequestVerificationToken""]').value;
            const find = async (url, pattern) => {
                const html = await (await fetch(url)).text();
                const match = html.match(pattern);
                return match ? match[1] : '';
            };
            const blog = await find('/Admin/Contents/ContentItems/Blog', /\/Admin\/Contents\/ContentItems\/([0-9a-z]{26})\/(?:Display|Edit)/);
            const post = await find('/Admin/Contents/ContentItems/BlogPost', /\/Admin\/Contents\/ContentItems\/([0-9a-z]{26})\/Edit/);
            let removed = 'none';
            if (post) {
                const response = await fetch('/Admin/Contents/ContentItems/' + post + '/Remove', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                    body: new URLSearchParams({ __RequestVerificationToken: token }),
                });
                removed = String(response.status);
            }
            return [blog, post, removed];
        }");
        Console.WriteLine($"Blog {ids[0]}, sample post {ids[1]}, removal status {ids[2]}.");

        if (string.IsNullOrEmpty(ids[0]))
        {
            throw new InvalidOperationException("The blog was not found.");
        }

        var json = File.ReadAllText(Path.Combine(Root, "recorder", "seed.json")).Replace("__BLOG__", ids[0]);
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        Console.WriteLine("Imported the posts and the template.");
        await context.CloseAsync();
    }

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static async Task SiteAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        await scene.OpenAsync("/Admin/Templates");
        await scene.Page.GetByRole(AriaRole.Link, new() { Name = "Content__BlogPost", Exact = true }).WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("site-1", async () =>
        {
            await scene.PauseAsync(600);
            await scene.HighlightAsync(scene.Page.GetByRole(AriaRole.Link, new() { Name = "Content__BlogPost", Exact = true }), "Content__BlogPost");
        });

        await scene.StepAsync("site-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.OffCameraAsync(async () =>
            {
                await scene.NavigateAsync("/blog/a-week-in-the-highlands");
                await scene.Page.Locator("h3", new() { HasText = "More in" }).First.WaitForAsync(new() { State = WaitForSelectorState.Attached });
            }, leadMilliseconds: 100);

            var heading = scene.Page.Locator("h3", new() { HasText = "More in" }).First;
            var count = scene.Page.Locator("p", new() { HasText = "published posts" }).First;
            await scene.ScrollAsync(0, 700);

            for (var i = 0; i < 10; i++)
            {
                var headingBox = await heading.BoundingBoxAsync();
                var countBox = await count.BoundingBoxAsync();

                if (headingBox is not null && countBox is not null && headingBox.Y > 0 && countBox.Y + countBox.Height < 864)
                {
                    break;
                }

                await scene.ScrollAsync(0, 100);
            }

            await scene.HighlightAsync(heading);
            await scene.HighlightAsync(count);
        });

        await scene.StepAsync("site-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator("p:has-text('published posts') + ul").First);
        });

        await scene.ClearHighlightsAsync();
    }
}
