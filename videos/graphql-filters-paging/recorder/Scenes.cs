using System.Text;
using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private const string Execute = "button.execute-button";

    private const string BlogPostRow = ".graphiql-explorer-root [data-field-name=\"blogPost\"]";

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-filter", FilterAsync),
        ("04-sort", SortAsync),
        ("05-page", PageAsync),
        ("06-status", StatusAsync),
        ("07-combine", CombineAsync),
        ("08-limit-error", LimitErrorAsync),
    ];

    private const string Q1 = """
        {
          blogPost {
            displayText
          }
        }
        """;

    private const string Q2 = """
        {
          blogPost(where: { displayText_contains: "with" }) {
            displayText
          }
        }
        """;

    private const string Q3 = """
        {
          blogPost(
            where: {
              path_starts_with: "tutorials/"
            }
          ) {
            displayText
            path
          }
        }
        """;

    private const string Q4 = """
        {
          blogPost(
            where: {
              OR: [
                { displayText_contains: "Liquid" }
                { displayText_contains: "GraphQL" }
              ]
            }
          ) {
            displayText
          }
        }
        """;

    private const string Q5 = """
        {
          blogPost(orderBy: { displayText: ASC }) {
            displayText
          }
        }
        """;

    private const string Q6 = """
        {
          blogPost(orderBy: { createdUtc: ASC }) {
            displayText
            createdUtc
          }
        }
        """;

    private const string Q7 = """
        {
          blogPost(first: 3) {
            displayText
          }
        }
        """;

    private const string Q8 = """
        {
          blogPost(first: 3, skip: 3) {
            displayText
          }
        }
        """;

    private const string Q9 = """
        {
          blogPost(first: 3, skip: 6) {
            displayText
          }
        }
        """;

    private const string Q10 = """
        {
          blogPost(status: DRAFT) {
            displayText
            published
          }
        }
        """;

    private const string Q11 = """
        {
          blogPost(status: LATEST) {
            displayText
            published
          }
        }
        """;

    private const string Q12 = """
        {
          blogPost(
            where: {
              path_starts_with: "guides/"
            }
            orderBy: { displayText: ASC }
            first: 2
            skip: 2
          ) {
            displayText
          }
        }
        """;

    private const string Q13 = """
        {
          blogPost(first: 1001) {
            displayText
          }
        }
        """;

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static string Post(int number, string title, string path, string created, bool published)
    {
        var id = "4gqlfilters" + number.ToString("D14");
        var publishedLines = published
            ? $"\"Published\": true, \"PublishedUtc\": \"{created}\","
            : "\"Published\": false,";

        return $$"""
            {
              "ContentItemId": "{{id}}",
              "ContentType": "BlogPost",
              "DisplayText": "{{title}}",
              "Latest": true,
              {{publishedLines}}
              "CreatedUtc": "{{created}}",
              "ModifiedUtc": "{{created}}",
              "Author": "admin",
              "TitlePart": { "Title": "{{title}}" },
              "AutoroutePart": { "Path": "{{path}}", "SetHomepage": false }
            }
            """;
    }

    private static string TypeAndContent()
    {
        var posts = new[]
        {
            Post(1, "Getting started with Orchard Core", "tutorials/getting-started", "2025-01-06T09:00:00Z", true),
            Post(2, "Content types explained", "tutorials/content-types", "2025-01-20T09:00:00Z", true),
            Post(3, "Liquid templates in practice", "tutorials/liquid-templates", "2025-02-03T09:00:00Z", true),
            Post(4, "Securing the admin", "guides/securing-the-admin", "2025-02-17T09:00:00Z", true),
            Post(5, "Deploying to a container", "guides/deploying-to-a-container", "2025-03-03T09:00:00Z", true),
            Post(6, "Searching your content", "guides/searching-your-content", "2025-03-17T09:00:00Z", true),
            Post(7, "Headless with GraphQL", "guides/headless-with-graphql", "2025-04-07T09:00:00Z", true),
            Post(8, "Backing up a tenant", "guides/backing-up-a-tenant", "2025-04-21T09:00:00Z", true),
            Post(9, "Roadmap notes", "guides/roadmap-notes", "2025-05-05T09:00:00Z", false),
        };

        var sb = new StringBuilder();
        sb.Append("""
            { "steps": [
              { "name": "ContentDefinition",
                "ContentTypes": [
                  { "Name": "BlogPost", "DisplayName": "Blog Post",
                    "Settings": { "ContentTypeSettings": { "Creatable": true, "Draftable": true, "Versionable": true, "Listable": true } },
                    "ContentTypePartDefinitionRecords": [
                      { "PartName": "TitlePart", "Name": "TitlePart", "Settings": { "ContentTypePartSettings": { "Position": "0" } } },
                      { "PartName": "AutoroutePart", "Name": "AutoroutePart", "Settings": { "ContentTypePartSettings": { "Position": "1" }, "AutoroutePartSettings": { "AllowCustomPath": true, "Pattern": "blog/{{ Model.ContentItem | display_text | slugify }}", "ShowHomepageOption": false } } }
                    ] }
                ],
                "ContentParts": [] },
              { "name": "content", "Data": [
            """);
        sb.Append(string.Join(",\n", posts));
        sb.Append("\n] } ] }");

        return sb.ToString();
    }

    private static async Task<string> ImportAsync(IPage page, string json)
    {
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        return await page.EvaluateAsync<string>("""
            async (json) => {
              const token = document.querySelector('form input[name="__RequestVerificationToken"]').value;
              const body = new URLSearchParams({ Json: json, __RequestVerificationToken: token });
              const response = await fetch('/Admin/DeploymentPlan/Import/Json', { method: 'POST', body, headers: { 'Content-Type': 'application/x-www-form-urlencoded' } });
              const text = await response.text();
              return response.status + ' ' + (text.includes('Recipe imported successfully') ? 'imported' : 'NOT imported');
            }
            """, json);
    }

    // Enables Autoroute, then creates the Blog Post type and its nine posts (fake data), outside the capture.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);

        Console.WriteLine("autoroute: " + await ImportAsync(page, """{ "steps": [ { "name": "feature", "enable": [ "OrchardCore.Autoroute" ], "disable": [] } ] }"""));

        for (var i = 0; i < 60; i++)
        {
            await Task.Delay(3000);

            try
            {
                var code = await page.EvaluateAsync<int>("async () => (await fetch('/Admin/DeploymentPlan/Import/Json')).status");

                if (code == 200)
                {
                    break;
                }
            }
            catch (PlaywrightException)
            {
            }
        }

        Console.WriteLine("type and posts: " + await ImportAsync(page, TypeAndContent()));

        var body = await page.EvaluateAsync<string>("async () => await (await fetch('/api/graphql?query=' + encodeURIComponent('{ blogPost(status: LATEST) { displayText published createdUtc path } }'))).text()");
        Console.WriteLine("seed check: " + body);
        await context.CloseAsync();
    }

    private static async Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        var scene = await Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);
        scene.Page.SetDefaultTimeout(120_000);

        return scene;
    }

    private static string Url(string query) => "/Admin/GraphQL?query=" + Uri.EscapeDataString(query);

    // Waits until GraphiQL has drawn the editor, the run button and the schema in the Explorer pane.
    private static async Task ReadyAsync(Scene scene, bool collapse)
    {
        var page = scene.Page;
        await page.Locator("#graphiql .loader").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
        await page.Locator(".query-editor .CodeMirror").First.WaitForAsync();
        await page.Locator(Execute).First.WaitForAsync();
        await page.Locator(BlogPostRow).First.WaitForAsync();

        if (collapse)
        {
            await page.EvaluateAsync("document.body.classList.add('left-sidebar-compact')");
            await Task.Delay(500);
        }

        await page.Mouse.MoveAsync(760, 520);
    }

    // Before the capture: opens the page with the query in the editor and the result empty.
    private static async Task OpenQueryAsync(Scene scene, string query, bool collapse)
    {
        await scene.OpenAsync(Url(query));
        await ReadyAsync(scene, collapse);
    }

    // On camera, with the page load cut out: puts the next query in the editor, with an empty result.
    private static Task NextQueryAsync(Scene scene, string query)
        => scene.OffCameraAsync(async () =>
        {
            await scene.NavigateAsync(Url(query));
            await ReadyAsync(scene, true);
        }, 300);

    // On camera: runs the query, and waits for the text that only its result has.
    private static async Task RunAsync(Scene scene, string expected)
    {
        await scene.ClickAsync(scene.Page.Locator(Execute).First, pauseAfter: 400);
        await scene.Page.WaitForFunctionAsync("text => document.querySelector('.result-window')?.innerText.includes(text)", expected);
    }

    private static async Task FilterAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenQueryAsync(scene, Q1, false);
        await scene.StartCaptureAsync();

        await scene.StepAsync("where-1", async () =>
        {
            await scene.MoveToAsync(page.Locator("#left-nav a[href$=\"/Admin/GraphQL\"]").First);
            await scene.PauseAsync(500);
            await RunAsync(scene, "Backing up a tenant");
        });

        await scene.StepAsync("where-2", async () =>
        {
            await scene.ClickAsync(page.Locator("button.leftbar-compactor").First, pauseAfter: 700);
            await NextQueryAsync(scene, Q2);
            await RunAsync(scene, "Headless with GraphQL");
        });

        await scene.StepAsync("where-3", async () =>
        {
            await NextQueryAsync(scene, Q3);
            await RunAsync(scene, "tutorials/liquid-templates");
        });

        await scene.StepAsync("where-4", async () =>
        {
            await NextQueryAsync(scene, Q4);
            await RunAsync(scene, "Liquid templates in practice");
        });
    }

    private static async Task SortAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenQueryAsync(scene, Q5, true);
        await scene.StartCaptureAsync();

        await scene.StepAsync("sort-1", async () =>
        {
            await RunAsync(scene, "Backing up a tenant");
        });

        await scene.StepAsync("sort-2", async () =>
        {
            await NextQueryAsync(scene, Q6);
            await RunAsync(scene, "Getting started with Orchard Core");
        });
    }

    private static async Task PageAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenQueryAsync(scene, Q7, true);
        await scene.StartCaptureAsync();

        await scene.StepAsync("page-1", async () =>
        {
            await RunAsync(scene, "Searching your content");
        });

        await scene.StepAsync("page-2", async () =>
        {
            await NextQueryAsync(scene, Q8);
            await RunAsync(scene, "Securing the admin");
        });

        await scene.StepAsync("page-3", async () =>
        {
            await NextQueryAsync(scene, Q9);
            await RunAsync(scene, "Getting started with Orchard Core");
        });
    }

    private static async Task StatusAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenQueryAsync(scene, Q10, true);
        await scene.StartCaptureAsync();

        await scene.StepAsync("status-1", async () =>
        {
            await RunAsync(scene, "Roadmap notes");
        });

        await scene.StepAsync("status-2", async () =>
        {
            await NextQueryAsync(scene, Q11);
            await RunAsync(scene, "Roadmap notes");
        });
    }

    private static async Task CombineAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenQueryAsync(scene, Q12, true);
        await scene.StartCaptureAsync();

        await scene.StepAsync("combine-1", async () =>
        {
            await RunAsync(scene, "Searching your content");
        });
    }

    private static async Task LimitErrorAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenQueryAsync(scene, Q13, true);
        await scene.StartCaptureAsync();

        await scene.StepAsync("limits-3", async () =>
        {
            await RunAsync(scene, "exceeds the maximum number of results");
        });
    }
}
