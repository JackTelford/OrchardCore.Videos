using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Recorder;

public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    private static IReadOnlyDictionary<string, double> s_durations = new Dictionary<string, double>();

    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-open", OpenAsync),
        ("03-text", TextAsync),
        ("04-numeric", NumericAsync),
        ("05-boolean", BooleanAsync),
        ("06-dates", DatesAsync),
        ("07-edit", EditAsync),
        ("08-view", ViewAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        s_durations = durations;
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    // Seeds state S0 outside the capture: the content type Workshop with the Title part and nothing else.
    public static async Task PrepareAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = StatePath, ViewportSize = new ViewportSize { Width = 1536, Height = 864 }, Locale = "en-US" });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(30_000);
        await page.GotoAsync(BaseUrl + "/Admin/ContentTypes/List", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("a[href$='/Admin/ContentTypes/Create']").First.ClickAsync();
        await page.GetByLabel("Display Name", new() { Exact = true }).FillAsync("Workshop");
        await page.GetByLabel("Display Name", new() { Exact = true }).PressAsync("Tab");
        Console.WriteLine("technical name: " + await page.GetByLabel("Technical Name", new() { Exact = true }).InputValueAsync());
        await page.GetByRole(AriaRole.Button, new() { Name = "Create", Exact = true }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.GetByLabel("Title", new() { Exact = true }).CheckAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Add Selected Parts" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine("now on " + page.Url + " : " + await page.TitleAsync());
        await context.CloseAsync();
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // The clock of a step: its time without the off-camera waits, so a click lands on its sentence of the narration.
    private sealed class Clock(Scene scene, string id)
    {
        private readonly Stopwatch _watch = Stopwatch.StartNew();
        private double _skipped;

        public double Duration { get; } = s_durations.TryGetValue(id, out var d) ? d : 2;

        public double Elapsed => _watch.Elapsed.TotalSeconds - _skipped;

        public async Task UntilAsync(double seconds)
        {
            Console.WriteLine($"    at {Elapsed:F1}s (target {seconds:F1}s of {Duration:F1}s)");
            var remaining = seconds - Elapsed;

            if (remaining > 0)
            {
                await scene.HoldAsync(remaining);
            }
        }

        // Holds until this many seconds before the end of the narration.
        public Task BeforeEndAsync(double seconds) => UntilAsync(Duration - seconds);

        public async Task OffAsync(Func<Task> wait, int lead = 400)
        {
            await scene.OffCameraAsync(async () =>
            {
                var t = Stopwatch.StartNew();
                await wait();
                _skipped += t.Elapsed.TotalSeconds + 0.4;
            }, lead);
        }
    }

    private static Task StepAsync(Scene scene, string id, Func<Clock, Task> actions)
        => scene.StepAsync(id, async () =>
        {
            var clock = new Clock(scene, id);
            await actions(clock);
            Console.WriteLine($"    done at {clock.Elapsed:F1}s of {clock.Duration:F1}s");
        });

    // A short cursor path: on a loaded machine every pointer move is a slow round trip.
    private static async Task ClickAsync(Scene scene, ILocator locator, int pauseAfter = 300)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 8);
        await Task.Delay(100);
        await scene.Page.Mouse.ClickAsync((float)x, (float)y);
        await Task.Delay(pauseAfter);
    }

    // Clicks something that loads a page; the load is cut out of the clip.
    private static async Task GoAsync(Scene scene, Clock clock, ILocator locator, int lead = 500)
    {
        await locator.WaitForAsync();
        var (x, y) = await scene.CenterOfAsync(locator);
        await scene.MoveToAsync(x, y, 8);
        await Task.Delay(100);
        await clock.OffAsync(() => scene.WaitForNavigationAsync(() => scene.Page.Mouse.ClickAsync((float)x, (float)y)), lead);
        await Task.Delay(150);
    }

    private static async Task TypeAsync(Scene scene, ILocator field, string text, int delay = 3)
    {
        await ClickAsync(scene, field, 150);
        await field.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = delay });
        await Task.Delay(200);

        if (await field.InputValueAsync() != text)
        {
            await field.FillAsync(text);
        }
    }

    private static ILocator Label(IPage page, string name) => page.GetByLabel(name, new() { Exact = true });

    private static ILocator Heading(IPage page, string name) => page.GetByRole(AriaRole.Heading, new() { Name = name, Exact = true });

    private static ILocator Button(IPage page, string name) => page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private static async Task ScrollToAsync(Scene scene, ILocator element, double top)
    {
        var box = await element.BoundingBoxAsync();
        await scene.ScrollAsync(0, box!.Y - top, 6);
    }

    private static async Task ScrollTopAsync(Scene scene)
    {
        if (await scene.Page.EvaluateAsync<double>("() => window.scrollY") > 0)
        {
            await scene.ScrollAsync(0, -3000, 6);
        }
    }

    private static async Task BringIntoViewAsync(Scene scene, ILocator element)
    {
        var box = await element.BoundingBoxAsync();

        if (box!.Y < 100 || box.Y + box.Height > 760)
        {
            await ScrollToAsync(scene, element, 400);
        }
    }

    private static async Task RingAsync(Scene scene, ILocator element, double seconds, string caption = null)
    {
        await scene.HighlightAsync(element, caption);
        await scene.HoldAsync(seconds);
        await scene.ClearHighlightsAsync();
    }

    private static ILocator MenuCaption(IPage page, string label)
        => page.Locator("#left-nav figcaption.nav-item-row").Filter(new() { HasTextRegex = new Regex($@"^\s*{Regex.Escape(label)}\s*$") }).First;

    // Opens a closed group of the admin menu without following its link.
    private static async Task OpenGroupAsync(Scene scene, string label, bool onCamera)
    {
        var toggle = MenuCaption(scene.Page, label).Locator("[aria-expanded]").First;

        if (await toggle.GetAttributeAsync("aria-expanded") != "false")
        {
            return;
        }

        if (onCamera)
        {
            await ClickAsync(scene, toggle, 300);
        }
        else
        {
            await toggle.ClickAsync();
            await Task.Delay(300);
        }
    }

    private static ILocator FieldTypeLabel(IPage page, string type)
        => page.Locator("form[action*='AddFieldsTo'] ul.list-group label").Filter(new() { HasTextRegex = new Regex($@"^\s*{Regex.Escape(type)}\s*$") }).First;

    // The page of a content type, scrolled to the Fields heading so Add Field is in view.
    private static async Task OpenWorkshopAsync(Scene scene)
    {
        await scene.OpenAsync("/Admin/ContentTypes/Edit/Workshop");
        await ScrollToAsync(scene, Heading(scene.Page, "Fields"), 120);
    }

    private static async Task AddFieldAsync(Scene scene, Clock clock)
    {
        await GoAsync(scene, clock, scene.Page.GetByRole(AriaRole.Button, new() { Name = "Add Field", Exact = true }));
        await Label(scene.Page, "Display Name").WaitForAsync();
        await scene.PauseAsync(300);
    }

    private static async Task ChooseTypeAsync(Scene scene, string type)
    {
        var label = FieldTypeLabel(scene.Page, type);
        await label.WaitForAsync();
        await BringIntoViewAsync(scene, label);
        await ClickAsync(scene, label, 300);

        if (!await Label(scene.Page, type).IsCheckedAsync())
        {
            await Label(scene.Page, type).CheckAsync();
        }
    }

    private static Task SaveConfigureAsync(Scene scene, Clock clock) => GoAsync(scene, clock, Button(scene.Page, "Save & Configure"));

    private static async Task SaveAsync(Scene scene, Clock clock)
    {
        await GoAsync(scene, clock, Button(scene.Page, "Save"));
    }

    // Scrolls so that the Fields of the type are in view after a save.
    private static async Task ShowFieldsAsync(Scene scene, double top = 120)
    {
        await Heading(scene.Page, "Fields").WaitForAsync();
        await ScrollToAsync(scene, Heading(scene.Page, "Fields"), top);
        await scene.MoveToAsync(760, 420, 8);
    }

    private static async Task<Scene> StartAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => await NewSceneAsync(browser, directory, durations);

    // 02-open: find the feature, open the type.
    private static async Task OpenAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Features");
        await OpenGroupAsync(scene, "Tools", false);

        ILocator Row(string id)
            => page.Locator(".list-group-item").Filter(new() { Has = page.Locator($"#btn-enable-{id}, #btn-disable-{id}") }).First;

        await scene.StartCaptureAsync();

        await StepAsync(scene, "open-1", async clock =>
        {
            await clock.UntilAsync(2.8);
            await TypeAsync(scene, page.Locator("#search-box"), "Content Fields", 20);
            await Row("OrchardCore_ContentFields").WaitForAsync();
            await clock.UntilAsync(7.4);
            await scene.HighlightAsync(Row("OrchardCore_ContentFields"), "Enabled");
        });

        await StepAsync(scene, "open-2", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await clock.UntilAsync(1.6);
            await OpenGroupAsync(scene, "Design", true);
            await OpenGroupAsync(scene, "Content Definition", true);
            await GoAsync(scene, clock, MenuCaption(page, "Content Types").Locator("a.item-label").First);
            await clock.UntilAsync(5.8);
            await TypeAsync(scene, page.Locator("#search-box"), "Workshop", 20);
            await scene.PauseAsync(600);
            await clock.UntilAsync(7.8);
            await scene.HighlightAsync(page.Locator("a.edit-button[href$='/ContentTypes/Edit/Workshop']"), "Edit");
        });

        await StepAsync(scene, "open-3", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await GoAsync(scene, clock, page.Locator("a.edit-button[href$='/ContentTypes/Edit/Workshop']"));
            await Heading(page, "Fields").WaitForAsync();
            await ScrollToAsync(scene, Heading(page, "Fields"), 120);
            await clock.UntilAsync(2.4);
            await scene.HighlightAsync(page.GetByRole(AriaRole.Button, new() { Name = "Add Field", Exact = true }));
            await clock.UntilAsync(5.6);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("ul#parts"));
        });

        await scene.ClearHighlightsAsync();
    }

    // 03-text: Speaker, a Text Field.
    private static async Task TextAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenWorkshopAsync(scene);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "text-1", async clock =>
        {
            await AddFieldAsync(scene, clock);
            await RingAsync(scene, Label(page, "Display Name"), 1.0);
            await RingAsync(scene, Label(page, "Technical Name"), 1.0);
            await RingAsync(scene, page.Locator("form[action*='AddFieldsTo'] ul.list-group"), 1.0);
            await clock.UntilAsync(6.3);
            await TypeAsync(scene, Label(page, "Display Name"), "Speaker");
            await scene.PauseAsync(300);
            await scene.HighlightAsync(Label(page, "Technical Name"));
        });

        await StepAsync(scene, "text-2", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(page.Locator("form[action*='AddFieldsTo'] ul.list-group"));
            await scene.HoldAsync(2.5);
            await scene.ClearHighlightsAsync();
            await clock.UntilAsync(4.0);
            await ChooseTypeAsync(scene, "Text Field");
            await clock.BeforeEndAsync(3.2);
            await SaveConfigureAsync(scene, clock);
            await Label(page, "Required").WaitForAsync();
        });

        await StepAsync(scene, "text-3", async clock =>
        {
            foreach (var name in new[] { "Required", "Hint", "Placeholder", "Default value", "Minimum length", "Maximum length" })
            {
                await RingAsync(scene, Label(page, name), 1.0);
            }

            var editor = Label(page, "What type of editor should be used?");
            await clock.UntilAsync(7.6);
            await ScrollToAsync(scene, editor, 300);
            await RingAsync(scene, editor, 2.0);
            await scene.HighlightAsync(Label(page, "What type of display mode should be used?"));
        });

        await StepAsync(scene, "text-4", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await ScrollTopAsync(scene);
            await ClickAsync(scene, Label(page, "Required"), 200);
            await TypeAsync(scene, Label(page, "Hint"), "The person who leads the workshop.");
            await clock.BeforeEndAsync(4.4);
            await SaveAsync(scene, clock);
            await ShowFieldsAsync(scene);
        });
    }

    // 04-numeric: Seats, a Numeric Field.
    private static async Task NumericAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenWorkshopAsync(scene);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "num-1", async clock =>
        {
            await AddFieldAsync(scene, clock);
            await TypeAsync(scene, Label(page, "Display Name"), "Seats");
            await ChooseTypeAsync(scene, "Numeric Field");
            await clock.BeforeEndAsync(3.2);
            await SaveConfigureAsync(scene, clock);
            await Label(page, "Required").WaitForAsync();
        });

        await StepAsync(scene, "num-2", async clock =>
        {
            await ScrollToAsync(scene, Label(page, "Scale"), 200);
            await scene.HighlightAsync(Label(page, "Scale"));
            await clock.UntilAsync(5.3);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(page, "Minimum"));
            await clock.UntilAsync(7.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(page, "Maximum"));
            await clock.UntilAsync(8.6);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(page, "Default value"));
        });

        await StepAsync(scene, "num-3", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await ScrollTopAsync(scene);
            await ClickAsync(scene, Label(page, "Required"), 100);
            await TypeAsync(scene, Label(page, "Minimum"), "1");
            await TypeAsync(scene, Label(page, "Maximum"), "40");
            await TypeAsync(scene, Label(page, "Default value"), "20");
            await page.Keyboard.PressAsync("Tab");
            await clock.BeforeEndAsync(4.0);
            await SaveAsync(scene, clock);
            await ShowFieldsAsync(scene);
        });
    }

    // 05-boolean: Registration Open, a Boolean Field.
    private static async Task BooleanAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenWorkshopAsync(scene);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "bool-1", async clock =>
        {
            await AddFieldAsync(scene, clock);
            await TypeAsync(scene, Label(page, "Display Name"), "Registration Open");
            await ChooseTypeAsync(scene, "Boolean Field");
            await clock.BeforeEndAsync(3.2);
            await SaveConfigureAsync(scene, clock);
            await Label(page, "Default value is true").WaitForAsync();
        });

        await StepAsync(scene, "bool-2", async clock =>
        {
            await scene.HighlightAsync(Label(page, "Label"));
            await scene.HoldAsync(2.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Label(page, "Default value is true"));
            await scene.HoldAsync(4.0);
            await scene.ClearHighlightsAsync();
            await TypeAsync(scene, Label(page, "Label"), "Participants can register");
            await ClickAsync(scene, Label(page, "Default value is true"), 200);
            await clock.BeforeEndAsync(4.4);
            await SaveAsync(scene, clock);
            await ShowFieldsAsync(scene);
        });
    }

    // 06-dates: Registration Deadline, Starts At, Doors Open.
    private static async Task DatesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAsync(browser, directory, durations);
        var page = scene.Page;
        await OpenWorkshopAsync(scene);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "dates-1", async clock =>
        {
            await AddFieldAsync(scene, clock);
            await TypeAsync(scene, Label(page, "Display Name"), "Registration Deadline");
            await ChooseTypeAsync(scene, "Date Field");
            await SaveConfigureAsync(scene, clock);
            await Label(page, "Required").WaitForAsync();
            await RingAsync(scene, Label(page, "Required"), 0.7);
            await RingAsync(scene, Label(page, "Hint"), 0.7);
            await RingAsync(scene, Label(page, "What type of editor should be used?"), 0.7);
            await RingAsync(scene, Label(page, "What type of display mode should be used?"), 0.7);
            await clock.BeforeEndAsync(4.4);
            await SaveAsync(scene, clock);
            await ShowFieldsAsync(scene);
        });

        await StepAsync(scene, "dates-2", async clock =>
        {
            await AddFieldAsync(scene, clock);
            await TypeAsync(scene, Label(page, "Display Name"), "Starts At");
            await ChooseTypeAsync(scene, "Date Time Field");
            await SaveConfigureAsync(scene, clock);
            await Label(page, "Required").WaitForAsync();
            await scene.HoldAsync(1.0);
            await clock.BeforeEndAsync(4.4);
            await SaveAsync(scene, clock);
            await ShowFieldsAsync(scene);
        });

        await StepAsync(scene, "dates-3", async clock =>
        {
            await AddFieldAsync(scene, clock);
            await TypeAsync(scene, Label(page, "Display Name"), "Doors Open");
            await ChooseTypeAsync(scene, "Time Field");
            await SaveConfigureAsync(scene, clock);
            await Label(page, "Step").WaitForAsync();
            await RingAsync(scene, Label(page, "Step"), 1.0);
            await clock.BeforeEndAsync(4.4);
            await SaveAsync(scene, clock);
            await ShowFieldsAsync(scene, 100);
        });

        await StepAsync(scene, "dates-4", async clock =>
        {
            await ScrollToAsync(scene, Heading(page, "Fields"), 100);
            await scene.HighlightAsync(page.Locator("ul#fields"));
        });
    }

    private static ILocator Wrapper(IPage page, string name) => page.Locator($".field-wrapper-workshop-{name}");

    // 07-edit: fill in a workshop.
    private static async Task EditAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await OpenGroupAsync(scene, "Content", false);
        await scene.StartCaptureAsync();

        await StepAsync(scene, "edit-1", async clock =>
        {
            await ClickAsync(scene, page.Locator("#new-dropdown"), 200);
            await clock.UntilAsync(3.2);
            await GoAsync(scene, clock, page.Locator("a.dropdown-item").Filter(new() { HasTextRegex = new Regex(@"^\s*Workshop\s*$") }));
            await Label(page, "Title").WaitForAsync();
        });

        await StepAsync(scene, "edit-2", async clock =>
        {
            var doors = Wrapper(page, "doors-open");
            await doors.WaitForAsync();
            var box = await doors.BoundingBoxAsync();

            if (box!.Y + box.Height > 800)
            {
                await ScrollToAsync(scene, Wrapper(page, "speaker"), 150);
            }

            await clock.UntilAsync(8.3);
            await scene.HighlightAsync(Wrapper(page, "speaker"));
            await scene.HighlightAsync(Wrapper(page, "seats"));
        });

        await StepAsync(scene, "edit-3", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Wrapper(page, "seats"));
            await scene.HoldAsync(4.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(Wrapper(page, "registration-open"));
        });

        await StepAsync(scene, "edit-4", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await TypeAsync(scene, Label(page, "Title"), "Planning a Content Model");
            await TypeAsync(scene, Label(page, "Speaker"), "Jamie Chen");
            await ClickAsync(scene, Label(page, "Seats"), 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("50", new() { Delay = 40 });
        });

        await StepAsync(scene, "edit-5", async clock =>
        {
            foreach (var name in new[] { "Registration Deadline", "Starts At", "Doors Open" })
            {
                await RingAsync(scene, page.GetByLabel(name), 0.5);
            }

            await EnterAsync(scene, page.GetByLabel("Registration Deadline"), "2030-03-12", "03122030");
            await EnterAsync(scene, page.GetByLabel("Starts At"), "2030-03-18T14:00", "03182030", "ArrowRight", "0200", "P");
            await EnterAsync(scene, page.GetByLabel("Doors Open"), "13:30", "0130", "P");
        });

        await StepAsync(scene, "edit-6", async clock =>
        {
            var message = Wrapper(page, "seats").GetByText("The value must be less than 40.");
            await GoAsync(scene, clock, Button(page, "Publish"));
            await message.WaitForAsync();
            await scene.HighlightAsync(message);
        });

        await StepAsync(scene, "edit-7", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await ClickAsync(scene, Label(page, "Seats"), 200);
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("25", new() { Delay = 40 });
            await clock.BeforeEndAsync(3.0);
            await GoAsync(scene, clock, Button(page, "Publish"));
        });
    }

    // Enters a value in a date, date-time or time input by keystrokes, starting on its first segment.
    private static async Task EnterAsync(Scene scene, ILocator input, string expected, params string[] keys)
    {
        var box = await input.BoundingBoxAsync();
        await scene.ClickAsync(input, offsetX: 22 - box!.Width / 2, pauseAfter: 150);

        foreach (var key in keys)
        {
            if (key.StartsWith("Arrow"))
            {
                await scene.Page.Keyboard.PressAsync(key);
            }
            else
            {
                await scene.Page.Keyboard.TypeAsync(key, new() { Delay = 30 });
            }
        }

        if (await input.InputValueAsync() != expected)
        {
            Console.WriteLine($"    (filled {expected} directly)");
            await input.FillAsync(expected);
        }

        await scene.PauseAsync(200);
    }

    // 08-view: the values on the site.
    private static async Task ViewAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await StartAsync(browser, directory, durations);
        var page = scene.Page;
        await scene.OpenAsync("/Admin/Contents/ContentItems");
        await OpenGroupAsync(scene, "Content", false);
        await scene.StartCaptureAsync();

        var row = page.Locator(".list-group-item").Filter(new() { HasText = "Planning a Content Model" }).First;

        await StepAsync(scene, "view-1", async clock =>
        {
            await scene.HighlightAsync(row);
            await clock.UntilAsync(4.0);
            await scene.ClearHighlightsAsync();
            var view = row.Locator("a.view");
            await view.WaitForAsync();
            var (x, y) = await scene.CenterOfAsync(view);
            await scene.MoveToAsync(x, y, 8);
            await clock.UntilAsync(5.0);
            IPage tab = null;
            await clock.OffAsync(async () =>
            {
                tab = await page.Context.RunAndWaitForPageAsync(() => page.Mouse.ClickAsync((float)x, (float)y));
                await tab.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await scene.SwitchToAsync(tab);
            }, 400);
            page = scene.Page;
            await page.Locator(".field-name-workshop-speaker").WaitForAsync();
            await ScrollToAsync(scene, page.Locator(".field-name-workshop-speaker"), 200);
        });

        await StepAsync(scene, "view-2", async clock =>
        {
            await clock.UntilAsync(3.8);
            await scene.HighlightAsync(scene.Page.Locator(".field-name-workshop-speaker"));
            await clock.UntilAsync(6.0);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".field-name-workshop-seats"));
            await clock.UntilAsync(8.2);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".field-name-workshop-registration-open"));
        });

        await StepAsync(scene, "view-3", async clock =>
        {
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".field-name-workshop-registration-deadline"));
            await clock.UntilAsync(3.2);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".field-name-workshop-starts-at"));
            await clock.UntilAsync(7.6);
            await scene.ClearHighlightsAsync();
            await scene.HighlightAsync(scene.Page.Locator(".field-name-workshop-doors-open"));
        });
    }
}
