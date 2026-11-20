using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The recordings of "Admin menus". PrepareAsync adds a second fake blog ("Travel Notes"). Record in one take, in order:
/// 03-create makes the menu "Editor Shortcuts", 04-link, 05-group and 06-dynamic fill it, 07-access sets permissions.
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
        ("02-list", ListAsync),
        ("03-create", CreateAsync),
        ("04-link", LinkAsync),
        ("05-group", GroupAsync),
        ("06-dynamic", DynamicAsync),
        ("07-access", AccessAsync),
    ];

    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, Locale = "en-US" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl + "/Admin/DeploymentPlan/Import/Json", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var item = new Dictionary<string, object>
        {
            ["ContentType"] = "Blog",
            ["ContentItemId"] = Guid.NewGuid().ToString("N")[..26],
            ["ContentItemVersionId"] = Guid.NewGuid().ToString("N")[..26],
            ["DisplayText"] = "Travel Notes",
            ["Latest"] = true,
            ["Published"] = true,
            ["Author"] = "admin",
            ["TitlePart"] = new Dictionary<string, string> { ["Title"] = "Travel Notes" },
        };
        var json = JsonSerializer.Serialize(new { steps = new[] { new { name = "content", data = new[] { item } } } });
        await page.EvaluateAsync("(json) => { document.querySelector('.CodeMirror').CodeMirror.setValue(json); }", json);
        await page.GetByRole(AriaRole.Button, new() { Name = "Import" }).ClickAsync();
        await page.GetByText("Recipe imported successfully").First.WaitForAsync();
        Console.WriteLine("Seeded the blog Travel Notes.");
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

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    private static Regex Exact(string text) => new($"^\\s*{Regex.Escape(text)}\\s*$");

    private static async Task ShowAsync(Scene scene, ILocator target, double seconds, string caption = null, bool clear = true)
    {
        await scene.HighlightAsync(target, caption);
        await scene.HoldAsync(seconds);

        if (clear)
        {
            await scene.ClearHighlightsAsync();
        }
    }

    // A click that loads a page: the cursor goes to the target on camera, the load is cut out.
    private static async Task NavAsync(Scene scene, ILocator target)
    {
        await scene.MoveToAsync(target);
        await scene.OffCameraAsync(() => scene.ClickAndWaitForNavigationAsync(target), leadMilliseconds: 150);
    }

    private static ILocator Left(Scene scene, string label)
        => scene.Page.Locator("#left-nav .item-label").Filter(new() { HasTextRegex = Exact(label) }).First;

    private static ILocator MenuRow(Scene scene, string name)
        => scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = name }).First;

    private static ILocator NodeRow(Scene scene, string text)
        => scene.Page.Locator($"li.menu-item:has(> div.menu-item-title:has-text(\"{text}\"))").First;

    // The empty child list of a node has no height, so a pointer drag cannot nest a node into it: the page's own MoveNode
    // request is sent from the page, then the tree reloads, as it does after a drag (see OPEN-QUESTIONS.md, question 3).
    private static async Task DropIntoAsync(Scene scene, ILocator from, string target)
    {
        var moved = await from.Locator("xpath=ancestor::li[contains(@class,'menu-item')][1]").First.GetAttributeAsync("data-treenode-id");
        var dest = NodeRow(scene, target);
        var destId = await dest.GetAttributeAsync("data-treenode-id");
        var position = await dest.Locator("ol.menu-item-links > li").CountAsync();
        await scene.Page.EvaluateAsync("""
            async ([moved, destId, position]) => {
              const form = new FormData();
              form.append('__RequestVerificationToken', document.querySelector("input[name='__RequestVerificationToken']").value);
              form.append('treeId', document.querySelector('#menu').dataset.treeId);
              form.append('nodeToMoveId', moved);
              form.append('destinationNodeId', destId);
              form.append('position', position);
              await fetch(document.querySelector('#menu').dataset.moveNodeUrl, { method: 'POST', body: form });
            }
            """, new object[] { moved, destId, position });
        await scene.HoldAsync(0.6);
        await scene.Page.ReloadAsync();
    }

    private static async Task OpenTreeAsync(Scene scene, string menu)
    {
        await scene.OpenAsync("/Admin/AdminMenu/List");
        await MenuRow(scene, menu).WaitForAsync();
        await MenuRow(scene, menu).Locator("a:has-text('Edit Nodes')").First.ClickAsync();
        await scene.Page.GetByText($"Edit Nodes for '{menu}'").First.WaitForAsync();
        await scene.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static async Task OpenAddNodeAsync(Scene scene, string card)
    {
        await scene.ClickAsync(scene.Page.Locator("a, button").Filter(new() { HasTextRegex = Exact("Add Node") }).First);
        var thumb = scene.Page.Locator("#modalTreeNodes .admin-node-thumbnail, #modalTreeNodes .card, #modalTreeNodes .item").Filter(new() { HasTextString = card }).First;
        await thumb.WaitForAsync();
        await scene.HighlightAsync(thumb);
    }

    private static async Task PickIconAsync(Scene scene, string search)
    {
        await scene.ClickAsync(scene.Page.Locator("button.icon-picker-trigger").First);
        var box = scene.Page.Locator("#iconPickerModal input.iconpicker-search").First;
        await box.WaitForAsync();
        await scene.TypeAsync(box, search, delay: 60);
        await scene.HoldAsync(0.4);
        await scene.ClickAsync(scene.Page.Locator("#iconPickerModal .iconpicker-item:visible").First);
    }

    private static async Task ListAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin");
        await scene.Page.GetByText("Welcome to Orchard Core").First.WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("list-1", async () =>
        {
            var tools = scene.Page.Locator("#left-nav button").Filter(new() { HasTextRegex = Exact("Tools") }).First;

            if (await tools.GetAttributeAsync("aria-expanded") == "false")
            {
                await scene.ClickAsync(tools, pauseAfter: 400);
            }

            await NavAsync(scene, Left(scene, "Admin Menus"));
            await scene.HighlightAsync(Left(scene, "Features"), "Enable the feature here");
        });

        await scene.StepAsync("list-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            var row = MenuRow(scene, "Admin menus");
            await ShowAsync(scene, row.Locator(".badge:has-text('Enabled')").First, 0.8);

            foreach (var button in new[] { "Edit Nodes", "Edit", "Delete" })
            {
                await ShowAsync(scene, row.Locator("a, button").Filter(new() { HasTextRegex = Exact(button) }).First, 0.8);
            }

            await scene.ClickAsync(row.Locator("button.dropdown-toggle, .btn-group > button").Filter(new() { HasTextString = "Actions" }).First);
        });

        await scene.StepAsync("list-3", async () =>
        {
            await scene.Page.Keyboard.PressAsync("Escape");
            await NavAsync(scene, MenuRow(scene, "Admin menus").Locator("a:has-text('Edit Nodes')").First);
            await scene.Page.GetByText("Edit Nodes for 'Admin menus'").First.WaitForAsync();

            foreach (var name in new[] { "Blog", "Main Menu", "Content" })
            {
                await ShowAsync(scene, NodeRow(scene, name), 0.8);
            }

            await ShowAsync(scene, Left(scene, "Blog"), 0.8);
            await scene.HighlightAsync(Left(scene, "Main Menu"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task CreateAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/AdminMenu/List");
        await MenuRow(scene, "Admin menus").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("create-1", async () =>
        {
            await NavAsync(scene, scene.Page.Locator("a").Filter(new() { HasTextRegex = Exact("Add Admin Menu") }).First);
            await scene.TypeAsync(scene.Page.Locator("input[name='Name']").First, "Editor Shortcuts", delay: 50);
            await NavAsync(scene, scene.Page.Locator("button.create").First);
        });

        await scene.StepAsync("create-2", async () =>
        {
            await MenuRow(scene, "Editor Shortcuts").WaitForAsync();
            await scene.HighlightAsync(MenuRow(scene, "Editor Shortcuts").Locator(".badge:has-text('Enabled')").First);
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task LinkAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await scene.OpenAsync("/Admin/AdminMenu/List");
        await MenuRow(scene, "Editor Shortcuts").WaitForAsync();
        await scene.StartCaptureAsync();

        await scene.StepAsync("link-1", async () =>
        {
            await NavAsync(scene, MenuRow(scene, "Editor Shortcuts").Locator("a:has-text('Edit Nodes')").First);
            await scene.Page.GetByText("Nothing here!").First.WaitForAsync();
            await OpenAddNodeAsync(scene, "Link Admin Node");
        });

        await scene.StepAsync("link-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("#modalTreeNodes").Locator(".admin-node-thumbnail, .card, .item").Filter(new() { HasTextString = "Link Admin Node" }).First.Locator("a.btn").First);
            await scene.TypeAsync(scene.Page.Locator("input[name$='LinkText']").First, "Media Library", delay: 40);
            await scene.TypeAsync(scene.Page.Locator("input[name$='LinkUrl']").First, "Media", delay: 40);
            await scene.HighlightAsync(scene.Page.Locator("input[name$='LinkUrl']").First.Locator("xpath=following::*[contains(@class,'hint')][1]"));
        });

        await scene.StepAsync("link-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            var target = scene.Page.Locator("input[name$='Target']").First;
            await target.ScrollIntoViewIfNeededAsync();
            await scene.MoveToAsync(target);
            await scene.HighlightAsync(target);
        });

        await scene.StepAsync("link-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await PickIconAsync(scene, "image");
        });

        await scene.StepAsync("link-5", async () =>
        {
            await NavAsync(scene, scene.Page.Locator("button.create").First);
            await scene.Page.GetByText("Admin node added successfully").First.WaitForAsync();
            await scene.HoldAsync(1);
            await NavAsync(scene, Left(scene, "Media Library"));
        });
    }

    private static async Task GroupAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenTreeAsync(scene, "Editor Shortcuts");
        await scene.StartCaptureAsync();

        await scene.StepAsync("group-1", async () =>
        {
            await OpenAddNodeAsync(scene, "Placeholder Admin Node");
            await scene.HoldAsync(0.8);
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("#modalTreeNodes").Locator(".admin-node-thumbnail, .card, .item").Filter(new() { HasTextString = "Placeholder Admin Node" }).First.Locator("a.btn").First);

            foreach (var label in new[] { "Link Text", "Icon", "Permissions" })
            {
                await ShowAsync(scene, scene.Page.Locator("label").Filter(new() { HasTextRegex = Exact(label) }).First, 0.7);
            }
        });

        await scene.StepAsync("group-2", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("input[name$='LinkText']").First, "Editorial", delay: 50);
            await PickIconAsync(scene, "pen");
            await NavAsync(scene, scene.Page.Locator("button.create").First);
        });

        await scene.StepAsync("group-3", async () =>
        {
            var from = NodeRow(scene, "Media Library").Locator(".menu-item-title").First;
            await DropIntoAsync(scene, from, "Editorial");
        });

        await scene.StepAsync("group-4", async () =>
        {
            await scene.ClickAsync(Left(scene, "Editorial"));
            await scene.HighlightAsync(Left(scene, "Media Library"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task DynamicAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenTreeAsync(scene, "Editor Shortcuts");
        var editorialMenu = Left(scene, "Editorial");
        if (await scene.Page.Locator("#left-nav a").Filter(new() { HasTextRegex = Exact("Media Library") }).First.IsVisibleAsync())
        {
            await editorialMenu.ClickAsync();
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("dynamic-1", async () =>
        {
            await OpenAddNodeAsync(scene, "Content Types");
            await scene.HoldAsync(0.6);
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("#modalTreeNodes").Locator(".admin-node-thumbnail, .card, .item").Filter(new() { HasTextString = "Content Types" }).First.Locator("a.btn").First);
            await ShowAsync(scene, scene.Page.GetByText("Show All Content Types").First, 1);

            foreach (var type in new[] { "Blog Post", "Page" })
            {
                await scene.ClickAsync(scene.Page.Locator("label").Filter(new() { HasTextRegex = Exact(type) }).First, pauseAfter: 250);
            }

            await NavAsync(scene, scene.Page.Locator("button.create").First);
        });

        await scene.StepAsync("dynamic-2", async () =>
        {
            await OpenAddNodeAsync(scene, "Lists");
            await scene.HoldAsync(0.6);
            await scene.ClearHighlightsAsync();
            await NavAsync(scene, scene.Page.Locator("#modalTreeNodes").Locator(".admin-node-thumbnail, .card, .item").Filter(new() { HasTextString = "Adds links for list content items" }).First.Locator("a.btn").First);
            await scene.SelectAsync(scene.Page.Locator("select[name$='ContentType']").First, "Blog");
            var parent = scene.Page.Locator("input.add-parent-link-checkbox").First;

            if (await parent.IsCheckedAsync())
            {
                await scene.ClickAsync(parent);
            }

            await NavAsync(scene, scene.Page.Locator("button.create").First);
        });

        await scene.StepAsync("dynamic-3", async () =>
        {
            await DropIntoAsync(scene, NodeRow(scene, "Content Types").Locator(".menu-item-title").First, "Editorial");
            await DropIntoAsync(scene, NodeRow(scene, "Lists").Locator(".menu-item-title").First, "Editorial");
            await scene.ClickAsync(Left(scene, "Editorial"));
        });
    }

    private static async Task AccessAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        await OpenTreeAsync(scene, "Editor Shortcuts");
        await scene.StartCaptureAsync();

        await scene.StepAsync("access-1", async () =>
        {
            await NavAsync(scene, NodeRow(scene, "Media Library").Locator("a.btn-primary:has-text('Edit')").First);
            var picker = scene.Page.Locator("#PermissionPicker").First;
            await picker.ScrollIntoViewIfNeededAsync();
            await scene.TypeAsync(picker.Locator("input.multiselect__input").First, "Manage Media", delay: 40);
            await scene.ClickAsync(picker.Locator(".multiselect__element").Filter(new() { HasTextRegex = new Regex(@"^\s*Manage Media\s*$") }).First.Locator(".multiselect__option").First);
            await scene.HoldAsync(1.5);
            await NavAsync(scene, scene.Page.Locator("button.save").First);
        });

        await scene.StepAsync("access-2", async () =>
        {
            await scene.NavigateAsync("/Admin/Roles/Index");
            await NavAsync(scene, scene.Page.Locator("li.list-group-item").Filter(new() { HasTextString = "Author" }).First.Locator("a.edit, a:has-text('Edit')").First);
            await scene.TypeAsync(scene.Page.Locator("#search-box").First, "View Admin Menu", delay: 40);
            var row = scene.Page.Locator("tr[data-text='View Admin Menu - Editor Shortcuts']").First;
            await row.WaitForAsync();
            await scene.ClickAsync(row.Locator("input[name^='Checkbox.']").First);
            await scene.HoldAsync(1.5);
            await NavAsync(scene, scene.Page.Locator("button.save").First);
        });

        await scene.StepAsync("access-3", async () =>
        {
            await scene.NavigateAsync("/Admin/AdminMenu/List");
            await ShowAsync(scene, Left(scene, "Editorial"), 1);
            var row = MenuRow(scene, "Editor Shortcuts");
            await scene.ClickAsync(row.Locator("button.dropdown-toggle, .btn-group > button").Filter(new() { HasTextString = "Actions" }).First);
            await NavAsync(scene, row.Locator("a.dropdown-item").Filter(new() { HasTextString = "Disable" }).First);
            await scene.HoldAsync(1);
            row = MenuRow(scene, "Editor Shortcuts");
            await scene.ClickAsync(row.Locator("button.dropdown-toggle, .btn-group > button").Filter(new() { HasTextString = "Actions" }).First);
            await NavAsync(scene, row.Locator("a.dropdown-item").Filter(new() { HasTextString = "Enable" }).First);
        });
    }
}
