using System.Diagnostics;
using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // Generated fake pictures (Pillow noise with a label), kept outside the repository.
    private static string SeedDirectory => Path.Combine(Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath(), "media-seed");

    private static string Seed(string folder, string name) => Path.Combine(SeedDirectory, folder, name);

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-tour", TourAsync),
        ("03-upload", UploadAsync),
        ("04-folders", FoldersAsync),
        ("05-files", FilesAsync),
        ("06-find", FindAsync),
        ("07-reuse", ReuseAsync),
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

    // Seeds the library (state S0) outside the capture, through the library itself.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1600, Height = 900 } });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);
        await page.GotoAsync(BaseUrl + "/Admin/Media", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("div.table-row.file-item").First.WaitForAsync();

        async Task CreateFolderAsync(string name)
        {
            var root = page.Locator("#folder-tree .treeroot");
            await root.HoverAsync();
            await root.Locator(".folder-actions a.action-button").ClickAsync();
            await page.Locator("text='Create a subfolder'").ClickAsync();
            await page.Locator("input[name='create-folder']").FillAsync(name);
            await page.Locator("#btn-submit").ClickAsync();
            await page.Locator($"#folder-tree li.folder:has-text('{name}') .folder-name").WaitForAsync();
            await Task.Delay(800);
        }

        async Task UploadFilesAsync(string[] files, string lastName)
        {
            await page.Locator("#fileupload").SetInputFilesAsync(files);
            await page.Locator($".file-items-table .table-row.file-item:has-text(\"{lastName}\")").First.WaitForAsync();
            await Task.Delay(4000);
        }

        await UploadFilesAsync([Seed("root", "draft-photo-1.jpg"), Seed("root", "draft-photo-2.jpg")], "draft-photo-2.jpg");

        await CreateFolderAsync("Old Drafts");
        await page.Locator("#folder-tree li.folder:has-text('Old Drafts') .folder-name").ClickAsync();
        await Task.Delay(1000);
        await UploadFilesAsync([Seed("old-drafts", "old-flyer.jpg")], "old-flyer.jpg");

        await page.Locator("#folder-tree .treeroot .folder-name").ClickAsync();
        await Task.Delay(1000);
        await CreateFolderAsync("Orchard Photos");
        await page.Locator("#folder-tree li.folder:has-text('Orchard Photos') .folder-name").ClickAsync();
        await Task.Delay(1000);
        var photos = Enumerable.Range(1, 12).Select(n => Seed("orchard-photos", $"photo-{n:D2}.jpg")).ToArray();
        await UploadFilesAsync(photos, "photo-01.jpg");
        await Task.Delay(6000);
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // Opens the page with the browser's storage cleared, so every take starts from the defaults.
    private static async Task OpenCleanAsync(Scene scene, string path)
    {
        await scene.OpenAsync(path);
        await scene.Page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");
        await scene.Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Task.Delay(500);
    }

    // A shorter cursor path than Scene.ClickAsync: on a loaded machine every pointer move is a slow round trip, and the clicks must land on their sentence.
    private static async Task ClickAsync(Scene scene, ILocator locator, int pauseAfter = 500)
    {
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 12);
        await Task.Delay(120);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await Task.Delay(pauseAfter);
    }

    private static async Task TypeAsync(Scene scene, ILocator locator, string text)
    {
        await ClickAsync(scene, locator, 200);
        await locator.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = 55 });
        await Task.Delay(300);
    }

    // Holds until the step's stopwatch reaches the given second, so a click lands on its sentence of the narration.
    private static async Task UntilAsync(Scene scene, Stopwatch watch, double seconds, string label = null)
    {
        Console.WriteLine($"    {label ?? "at"} {watch.Elapsed.TotalSeconds:F1}s (target {seconds:F1}s)");
        var remaining = seconds - watch.Elapsed.TotalSeconds;

        if (remaining > 0)
        {
            await scene.HoldAsync(remaining);
        }
    }

    private static ILocator FileRow(IPage page, string name) => page.Locator($".file-items-table .table-row.file-item:has-text(\"{name}\")");

    private static ILocator RowMenuButton(IPage page, string name) => FileRow(page, name).Locator(".table-cell").Last.Locator("a.action-button");

    private static ILocator FolderRow(IPage page, string name) => page.Locator($"#folder-tree li.folder:has-text('{name}')").First;

    private static async Task OpenLibraryAsync(Scene scene)
    {
        await OpenCleanAsync(scene, "/Admin/Media");
        await scene.Page.Locator("#fileContainer").WaitForAsync();
        await scene.Page.Locator("div.table-row.file-item").Nth(2).WaitForAsync();
    }

    // Picks Harvest Festival in the folder selector of the Move file and Copy file dialogs.
    private static async Task ChooseHarvestFestivalAsync(Scene scene)
    {
        var page = scene.Page;
        await ClickAsync(scene, page.Locator(".action-modal .p-treeselect"));
        await Task.Delay(500);
        var panel = page.Locator(".p-treeselect-overlay").Last;
        await panel.WaitForAsync();

        if (await panel.Locator(".p-tree-node-label:text-is('Harvest Festival')").CountAsync() == 0)
        {
            await ClickAsync(scene, panel.Locator(".p-tree-node-toggle-button").First, 300);
        }

        await ClickAsync(scene, panel.Locator(".p-tree-node-label:text-is('Harvest Festival')").First);
        await scene.PauseAsync(400);
    }

    private static async Task TourAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenLibraryAsync(scene);

        var library = page.Locator("#left-nav ul.nav-group-items[data-title=\"Media\"] a.item-label:has-text(\"Library\")");

        if (!await library.IsVisibleAsync())
        {
            await page.Locator("#left-nav button.nav-group-label:has(span.title:text-is(\"Media\"))").ClickAsync();
            await library.WaitForAsync();
        }

        await scene.StartCaptureAsync();

        await scene.StepAsync("tour-1", async () =>
        {
            await scene.HighlightAsync(library, "Media, Library");
        });

        await scene.StepAsync("tour-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("#folder-tree"), "Folder tree");
            await scene.HoldAsync(4.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".file-items-table"), "Files of the folder");
        });

        await scene.StepAsync("tour-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator(".file-items-table .table-head").First, "Name, Last modified, Size");
        });

        await scene.StepAsync("tour-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("label.upload-button"), "Upload files");
            await scene.HoldAsync(3.0);
            await scene.HighlightAsync(page.Locator(".action-bar a[title=\"Delete\"]"), "Delete, Download");
            await scene.HighlightAsync(page.Locator(".action-bar a[title=\"Download\"]"));
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task UploadAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenLibraryAsync(scene);
        await scene.StartCaptureAsync();

        IFileChooser chooser = null;

        await scene.StepAsync("upload-1", async () =>
        {
            await scene.HighlightAsync(page.Locator("#folder-tree .treeroot"), "Files");
            await scene.HoldAsync(3.0);
            await scene.ClearHighlightsAsync();
            chooser = await page.RunAndWaitForFileChooserAsync(() => ClickAsync(scene, page.Locator("label.upload-button")));
        });

        await scene.StepAsync("upload-2", async () =>
        {
            await chooser.SetFilesAsync([Seed("upload", "apple-harvest.jpg"), Seed("upload", "cider-menu.jpg")]);
            await page.Locator(".file-items-table .file-name-cell:has-text(\"apple-harvest.jpg\")").WaitForAsync();
        });

        await scene.StepAsync("upload-3", async () =>
        {
            await scene.PauseAsync(2200);
            var second = await page.RunAndWaitForFileChooserAsync(() => ClickAsync(scene, page.Locator("label.upload-button")));
            await second.SetFilesAsync([Seed("upload", "apple-harvest.jpg")]);
            await page.Locator(".notification-toast-detail:has-text(\"A file with this name already exists in the current folder.\")").First.WaitForAsync();
        });
    }

    private static async Task FoldersAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenLibraryAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("folders-1", async () =>
        {
            var root = page.Locator("#folder-tree .treeroot");
            { var (rx, ry) = await scene.CenterOfAsync(root); await scene.MoveToAsync(rx, ry, 12); }
            await ClickAsync(scene, root.Locator(".folder-actions a.action-button"));
            await page.Locator("text='Create a subfolder'").WaitForAsync();
        });

        await scene.StepAsync("folders-2", async () =>
        {
            await ClickAsync(scene, page.Locator("text='Create a subfolder'"));
            var input = page.Locator("input[name='create-folder']");
            await input.WaitForAsync();
            await scene.PauseAsync(600);
            await TypeAsync(scene, input, "Harvest Festival");
            await scene.PauseAsync(500);
            await ClickAsync(scene, page.Locator("#btn-submit"));
            await FolderRow(page, "Harvest Festival").WaitForAsync();
        });

        await scene.StepAsync("folders-3", async () =>
        {
            await ClickAsync(scene, FolderRow(page, "Harvest Festival").Locator(".folder-name").First);
            await page.Locator("text='This folder is empty'").WaitForAsync();
            await scene.HighlightAsync(page.Locator(".breadcrumb-path"), "Breadcrumb");
        });

        await scene.StepAsync("folders-4", async () =>
        {
            var watch = Stopwatch.StartNew();
            await scene.ClearHighlightsAsync();
            var old = FolderRow(page, "Old Drafts");
            { var (ox, oy) = await scene.CenterOfAsync(old); await scene.MoveToAsync(ox, oy, 12); }
            await ClickAsync(scene, old.Locator(".folder-actions a.action-button").First);
            await page.Locator("text=\"Delete\"").First.WaitForAsync();
            await UntilAsync(scene, watch, 5.0, "menu open");
            await ClickAsync(scene, page.Locator("text=\"Delete\"").First);
            await page.Locator("#btn-submit").WaitForAsync();
            Console.WriteLine($"    dialog open {watch.Elapsed.TotalSeconds:F1}s");
        });

        await scene.StepAsync("folders-5", async () =>
        {
            var watch = Stopwatch.StartNew();
            await UntilAsync(scene, watch, 4.6, "ok");
            await ClickAsync(scene, page.Locator("#btn-submit"));
            await page.Locator("#folder-tree .folder-name:text-is('Old Drafts')").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        });
    }

    private static async Task FilesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenLibraryAsync(scene);
        await scene.StartCaptureAsync();

        var okButton = page.Locator(".action-modal button:has-text(\"Ok\")");

        await scene.StepAsync("files-1", async () =>
        {
            await ClickAsync(scene, RowMenuButton(page, "apple-harvest.jpg"));
            await page.Locator("text=\"Rename\"").First.WaitForAsync();
        });

        await scene.StepAsync("files-2", async () =>
        {
            await ClickAsync(scene, page.Locator("text='Rename'").First);
            var input = page.Locator("input[name='rename']");
            await input.WaitForAsync();
            await scene.PauseAsync(600);
            { var (ix, iy) = await scene.CenterOfAsync(input); await scene.MoveToAsync(ix, iy, 12); }
            await input.FillAsync(string.Empty);
            await TypeAsync(scene, input, "harvest-table.jpg");
            await scene.PauseAsync(500);

            if (await input.InputValueAsync() != "harvest-table.jpg")
            {
                await input.FillAsync("harvest-table.jpg");
                await scene.PauseAsync(500);
            }

            await ClickAsync(scene, page.Locator("#btn-submit"));
            await FileRow(page, "harvest-table.jpg").First.WaitForAsync();
        });

        await scene.StepAsync("files-3", async () =>
        {
            await ClickAsync(scene, RowMenuButton(page, "harvest-table.jpg"));
            await ClickAsync(scene, page.Locator("text=\"Move\"").First);
            await page.Locator(".action-modal .p-treeselect").WaitForAsync();
            await scene.PauseAsync(500);
            await ChooseHarvestFestivalAsync(scene);
            await ClickAsync(scene, okButton);
            await page.Locator("text=\"File(s) moved successfully.\"").First.WaitForAsync();
        });

        await scene.StepAsync("files-4", async () =>
        {
            await ClickAsync(scene, RowMenuButton(page, "cider-menu.jpg"));
            await ClickAsync(scene, page.Locator("text=\"Copy\"").First);
            await page.Locator(".action-modal .p-treeselect").WaitForAsync();
            await scene.PauseAsync(400);
            await ChooseHarvestFestivalAsync(scene);
            await scene.HoldAsync(1.0);
            await ClickAsync(scene, okButton);
            await page.Locator("text=\"File copied successfully.\"").First.WaitForAsync();
            await ClickAsync(scene, FolderRow(page, "Harvest Festival").Locator(".folder-name").First);
            await FileRow(page, "harvest-table.jpg").First.WaitForAsync();
        });

        await scene.StepAsync("files-5", async () =>
        {
            await ClickAsync(scene, page.Locator("#folder-tree .treeroot .folder-name"));
            await FileRow(page, "draft-photo-1.jpg").First.WaitForAsync();
            await scene.PauseAsync(500);
            await ClickAsync(scene, FileRow(page, "draft-photo-1.jpg").Locator("input[name='select-file']"));
            await ClickAsync(scene, FileRow(page, "draft-photo-2.jpg").Locator("input[name='select-file']"));
            await scene.HighlightAsync(page.Locator(".action-bar a[title=\"Delete\"]"), "Delete");
            await scene.HighlightAsync(page.Locator(".action-bar a[title=\"Download\"]"), "Download");
        });

        await scene.StepAsync("files-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, page.Locator(".action-bar a[title='Delete']"));
            await okButton.WaitForAsync();
            await scene.HoldAsync(3.0);
            await ClickAsync(scene, okButton);
            await FileRow(page, "draft-photo-1.jpg").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        });
    }

    private static async Task FindAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenLibraryAsync(scene);
        await scene.StartCaptureAsync();

        await scene.StepAsync("find-1", async () =>
        {
            await ClickAsync(scene, FolderRow(page, "Orchard Photos").Locator(".folder-name").First);
            await FileRow(page, "photo-01.jpg").First.WaitForAsync();
            await scene.HighlightAsync(page.Locator("#file-pager"), "Pager");
        });

        await scene.StepAsync("find-2", async () =>
        {
            await scene.ClearHighlightsAsync();
            await TypeAsync(scene, page.Locator("#file-filter-input"), "photo-1");
            await scene.PauseAsync(600);
        });

        await scene.StepAsync("find-3", async () =>
        {
            await ClickAsync(scene, page.Locator("#clear-file-filter-button"), 150);
            var size = page.Locator(".table-head .table-cell:has-text(\"Size\")").First;
            await ClickAsync(scene, size, 150);
            await scene.HoldAsync(1.0);
            await ClickAsync(scene, size, 150);
            await scene.HighlightAsync(page.Locator(".table-head").First, "Click a heading to sort");
        });

        await scene.StepAsync("find-4", async () =>
        {
            await scene.ClearHighlightsAsync();
            var grid = page.Locator(".action-bar a[title='Grid view']");
            await ClickAsync(scene, grid);
            await page.WaitForFunctionAsync("() => { const shown = [...document.querySelectorAll('.file-items-grid img')].filter(i => { const r = i.getBoundingClientRect(); return r.bottom > 0 && r.top < window.innerHeight; }); return shown.length > 0 && shown.every(i => i.complete && i.naturalWidth > 0); }", null, new PageWaitForFunctionOptions { Timeout = 10000 });
            var tableView = page.Locator(".action-bar a[title='Table view']");
            await scene.HighlightAsync(tableView, "Table view");
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, tableView);
            await FileRow(page, "photo-12.jpg").First.WaitForAsync();
        });

        await scene.StepAsync("find-5", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, page.Locator("a.settings-btn"));
            await page.Locator("#settings-page-size").WaitForAsync();
            await scene.HighlightAsync(page.Locator("a.settings-btn"), "Settings");
            await scene.HoldAsync(3.0);
            await scene.SelectAsync(page.Locator("#settings-page-size"), "20");
        });

        await scene.StepAsync("find-6", async () =>
        {
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, page.Locator("a.storage-info-btn"));
            await scene.HighlightAsync(page.Locator("a.storage-info-btn"), "Storage Info");
        });

        await scene.ClearHighlightsAsync();
    }

    private static async Task ReuseAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenCleanAsync(scene, "/Admin/Contents/ContentTypes/Article/Create");
        var field = page.Locator("div.media-field-app").First;
        await field.WaitForAsync();
        await page.EvaluateAsync("() => { const f = document.querySelector('div.media-field-app'); const r = f.getBoundingClientRect(); window.scrollBy(0, r.top - (window.innerHeight - r.height) / 2); }");
        await Task.Delay(500);
        await scene.StartCaptureAsync();

        var picker = page.Locator(".mf-picker-media-gallery");

        await scene.StepAsync("reuse-1", async () =>
        {
            await scene.HighlightAsync(field, "Banner Image");
            await scene.HoldAsync(4.5);
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, field.Locator("button:has-text('Add media')"));
            await scene.OffCameraAsync(() => picker.Locator(".file-items-table").WaitForAsync());
        });

        await scene.StepAsync("reuse-2", async () =>
        {
            await scene.HighlightAsync(picker.Locator("label.upload-button"), "Upload files");
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, picker.Locator("#folder-tree li.folder:has-text('Harvest Festival') .folder-name").First);
            var row = picker.Locator(".table-row.file-item:has-text(\"harvest-table.jpg\")");
            await row.WaitForAsync();
            await scene.PauseAsync(500);
            await ClickAsync(scene, row.Locator("input[name='select-file']"));
            await scene.HoldAsync(1.5);
            await ClickAsync(scene, page.Locator(".mf-modal-footer button.mf-btn-primary"));
        });

        await scene.StepAsync("reuse-3", async () =>
        {
            await scene.ClearHighlightsAsync();
            await field.Locator("img").First.WaitForAsync();
            await page.WaitForFunctionAsync("() => [...document.querySelectorAll('div.media-field-app img')].every(i => i.complete && i.naturalWidth > 0)");
            await scene.HighlightAsync(field, "Banner Image");
        });

        await scene.ClearHighlightsAsync();
    }
}
