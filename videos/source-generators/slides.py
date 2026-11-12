"""The slides of the source generators video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {
    "NavigationArguments", "PropertyBasedNamedEnumerable", "RouteData", "IReadOnlyList", "ArgumentOutOfRangeException",
    "INamedEnumerable", "IShapeFactory", "IShape", "IPositioned", "ValueTask", "PagerSlim", "ShapeMetadata",
    "DebugSettingsViewModel", "DebugSettings", "ISite", "BuildEditorContext", "IDisplayResult", "Task",
    "NavigationItemViewModel", "IEnumerable", "LocalizedString", "StartupBase", "ClamAVStartup", "ReportsStartup",
    "InterceptorsHelper", "Interceptor_0F3A9C", "GeneratedShape_A1B2C3", "IList", "Arguments", "Feature",
    "NavigationConstants", "ResultExecutingContext",
}

REASONS = [
    ("1", "Arguments by reflection", "Without generated code, properties are read through reflection."),
    ("2", "Typed shapes by proxy", "Without generated code, a proxy class derives from your model."),
    ("3", "Generators write the code", "They run during the build and add source to the project."),
    ("4", "Analyzers report mistakes", "They warn about shape and feature mistakes while you build."),
]

RULES = [
    ("1", "The class is partial", "The generator adds the other half of the class."),
    ("2", "Public instance properties", "Static properties and properties without a getter are left out."),
    ("3", "Declared on the class itself", "Properties inherited from a base class are left out."),
    ("4", "No other base class", "The generated half sets PropertyBasedNamedEnumerable as the base class."),
]

PATHS = [
    ("1", "Interceptor", "Arguments.From with an anonymous object is rewritten at build time."),
    ("2", "Reflection", "Any other object is read by reflection, with the property lookup cached per type."),
    ("3", "Skipped", "An anonymous object with no public property, or with an anonymous property."),
]

CHOICES = [
    ("1", "Arguments you reuse", "A named class marked with GenerateArguments."),
    ("2", "A value you pass once", "Arguments.From with an anonymous object."),
    ("3", "A view model you own", "A partial class marked with GenerateShape."),
    ("4", "Every other typed shape", "The generator intercepts Initialize and CreateAsync."),
]

ARGUMENTS_CLASS = """
[GenerateArguments]
internal sealed partial class NavigationArguments
{
    public string MenuName { get; set; }
    public RouteData RouteData { get; set; }
}
"""

ARGUMENTS_GENERATED = """
// Generated half, shortened: the generator writes global:: names
internal partial class NavigationArguments : PropertyBasedNamedEnumerable
{
    private static readonly string[] s_propertyNames = new string[]
    {
        "MenuName",
        "RouteData",
    };
    protected override int PropertyCount => 2;
    protected override IReadOnlyList<string> PropertyNames => s_propertyNames;
    protected override object? GetPropertyValue(int index)
    {
        return index switch
        {
            0 => this.MenuName,
            1 => this.RouteData,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }
}
"""

ARGUMENTS_CALL = """
var menuShape = await _shapeFactory.CreateAsync("Navigation",
    Arguments.From(new NavigationArguments
    {
        MenuName = NavigationConstants.AdminId,
        RouteData = filterContext.RouteData,
    }));
"""

ARGUMENTS_FROM = """
public static INamedEnumerable<object> From<T>(T propertyObject) where T : notnull
{
    // Fast path for source-generated types that directly implement INamedEnumerable<object>
    if (propertyObject is INamedEnumerable<object> namedEnumerable)
    {
        return namedEnumerable;
    }

    // ...
}
"""

PAGER = """
public static ValueTask<IShape> PagerSlimAsync(this IShapeFactory shapeFactory, PagerSlim pager)
    => shapeFactory.CreateAsync(nameof(PagerSlim), Arguments.From(new
    {
        pager.Before,
        pager.After,
        pager.PageSize,
    }));
"""

INTERCEPTOR = """
// Generated, shortened: one class per call, named Interceptor_ plus an id
file static class Interceptor_0F3A9C
{
    private static readonly string[] s_names = ["Before", "After", "PageSize"];

    // The InterceptsLocation attribute points at the call it replaces
    public static INamedEnumerable<object> InterceptFrom<T>(T anonymousObject) where T : notnull
    {
        var typedObject = InterceptorsHelper.Cast(anonymousObject,
            new { Before = (string)default, After = (string)default, PageSize = (int)default });

        return Arguments.From([typedObject.Before, typedObject.After, typedObject.PageSize], s_names);
    }
}
"""

DRIVER = """
public override async Task<IDisplayResult> EditAsync(ISite site, DebugSettings settings, BuildEditorContext context)
{
    // ...
    return Initialize<DebugSettingsViewModel>("DebugSettings_Edit", model =>
    {
        model.WriteShapeDebugInformation = settings.WriteShapeDebugInformation;
    }).Location("Content:5")
    .OnGroup(SettingsGroupId);
}
"""

GENERATED_SHAPE = """
// Generated, shortened: the name is GeneratedShape_ plus a hash
public sealed class GeneratedShape_A1B2C3 : DebugSettingsViewModel, IShape, IPositioned
{
    private ShapeMetadata? _metadata;
    // ...

    public ShapeMetadata Metadata => _metadata ??= new ShapeMetadata();

    public string Position
    {
        get => Metadata.Position;
        set => Metadata.Position = value;
    }

    public string Id { get; set; } = null!;
    public IList<string> Classes => _classes ??= [];
    // ...
}
"""

SHAPE_MODEL = """
[GenerateShape]
public partial class NavigationItemViewModel : IEnumerable<object>
{
    public NavigationItemViewModel()
    {
        Metadata.Type = "NavigationItem";
    }

    public LocalizedString Text { get; set; }
    public string Href { get; set; }
    public bool Selected { get; set; }
    // ...
}
"""

SEALED_MODEL = """
public sealed class DebugSettingsViewModel
{
    public bool WriteShapeDebugInformation { get; set; }
}

return Initialize<DebugSettingsViewModel>("DebugSettings_Edit", model =>
{
    model.WriteShapeDebugInformation = settings.WriteShapeDebugInformation;
});
"""

SEALED_WARNING = """
warning OCSG001: Type 'DebugSettingsViewModel' is sealed and does not implement IShape,
so OrchardCore can't create a proxy-based shape for this 'Initialize' call
"""

FEATURE_VALID = """
// Manifest.cs
[assembly: Feature(
    Id = "OrchardCore.Antivirus.ClamAV",
    Name = "ClamAV Antivirus Scanner",
    // ...
)]

// Startup.cs
[Feature("OrchardCore.Antivirus.ClamAV")]
public sealed class ClamAVStartup : StartupBase
{
    // ...
}
"""

FEATURE_INVALID = """
// Startup.cs of MyCompany.Reports
[Feature("OrchardCore.Contents")]
public sealed class ReportsStartup : StartupBase
{
    // ...
}
"""

FEATURE_WARNING = """
warning OCSG002: Feature 'OrchardCore.Contents' is not
defined by the current module 'MyCompany.Reports', so
OrchardCore will ignore this target. Known local
feature IDs: MyCompany.Reports.
"""

EFFECTS = [
    ("1", "With the analyzer reference", "The generators write code into the project at build time."),
    ("2", "Without it", "A class marked with GenerateArguments has no generated half."),
    ("3", "At runtime", "Arguments.From reads the properties of that class by reflection."),
]

BUILD_PROPS = """
<PropertyGroup>
  <!-- Enable interceptors -->
  <InterceptorsNamespaces>$(InterceptorsNamespaces);OrchardCore.DisplayManagement.Generated</InterceptorsNamespaces>
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="$(MSBuildThisFileDirectory)/../OrchardCore/OrchardCore.SourceGenerators/OrchardCore.SourceGenerators.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
"""


def points(image, lines, x, y, gap=64):
    """Short statements under or beside a panel: a green dot, then the text."""
    draw = ImageDraw.Draw(image)
    for line in lines:
        draw.ellipse((x, y - 7, x + 14, y + 7), fill=GREEN)
        draw.text((x + 36, y), line, font=font(30), fill=TEXT, anchor="lm")
        y += gap


def slide(step_id, chapter_title, label):
    if step_id in ("why-1", "why-2"):
        image = slide_base(chapter_title, label, "What happens at runtime" if step_id == "why-1" else "OrchardCore.SourceGenerators")
        bullets(image, REASONS, active={0, 1} if step_id == "why-1" else {2, 3})
        return image

    if step_id == "args-1":
        image = slide_base(chapter_title, label, "1. Mark a partial class")
        draw_code(image, (96, 240, 1250, 700), "AdminMenuFilter.cs", "csharp", ARGUMENTS_CLASS, types=TYPES, size=28)
        note_cards(image, [
            ("Attribute", "GenerateArguments"),
            ("Modifier", "partial"),
            ("Properties", "public, with a getter"),
        ], (1300, 240, W - 96, 1030))
        return image

    if step_id == "args-2":
        image = slide_base(chapter_title, label, "2. The generator writes the other half")
        draw_code(image, (96, 240, 1250, 1030), "OrchardCore_Admin_NavigationArguments.g.cs", "csharp", ARGUMENTS_GENERATED, types=TYPES, size=24)
        note_cards(image, [
            ("Names", "kept in a static array"),
            ("Values", "read by a switch"),
            ("Reflection", "none"),
        ], (1300, 240, W - 96, 1030))
        return image

    if step_id == "args-3":
        image = slide_base(chapter_title, label, "3. Pass it to Arguments.From")
        draw_code(image, (96, 240, W - 96, 560), "AdminMenuFilter.cs", "csharp", ARGUMENTS_CALL, types=TYPES, size=26)
        draw_code(image, (96, 590, W - 96, 1030), "Arguments.cs", "csharp", ARGUMENTS_FROM, types=TYPES, size=24)
        return image

    if step_id == "args-4":
        image = slide_base(chapter_title, label, "What the generator needs")
        bullets(image, RULES)
        return image

    if step_id == "anon-1":
        image = slide_base(chapter_title, label, "An anonymous object passed to Arguments.From")
        draw_code(image, (96, 240, W - 96, 680), "ShapeFactoryExtensions.cs", "csharp", PAGER, types=TYPES, size=26)
        points(image, ["The generator finds the call to Arguments.From at build time.",
                       "The type argument is an anonymous type."], 110, 780)
        return image

    if step_id == "anon-2":
        image = slide_base(chapter_title, label, "The generated interceptor")
        draw_code(image, (96, 240, W - 96, 1030), "ArgumentsFromInterceptors.g.cs", "csharp", INTERCEPTOR, types=TYPES, size=24)
        return image

    if step_id == "anon-3":
        image = slide_base(chapter_title, label, "Interceptor or reflection")
        bullets(image, PATHS)
        return image

    if step_id == "shapes-1":
        image = slide_base(chapter_title, label, "Initialize in a display driver")
        draw_code(image, (96, 240, W - 96, 760), "DebugSettingsDisplayDriver.cs", "csharp", DRIVER, types=TYPES, size=26)
        points(image, ["The model is a class: DebugSettingsViewModel.",
                       "Without generated code, the shape is a proxy that derives from it."], 110, 860)
        return image

    if step_id == "shapes-2":
        image = slide_base(chapter_title, label, "A generated class replaces the proxy")
        draw_code(image, (96, 240, 1250, 1030), "ShapeFactoryGenerator.g.cs", "csharp", GENERATED_SHAPE, types=TYPES, size=22)
        note_cards(image, [
            ("Derives from", "your view model"),
            ("Implements", "IShape, IPositioned"),
            ("Intercepts", "the call that creates it"),
        ], (1300, 240, W - 96, 1030))
        return image

    if step_id == "shapes-3":
        image = slide_base(chapter_title, label, "GenerateShape on a model you own")
        draw_code(image, (96, 240, 1250, 840), "NavigationItemViewModel.cs", "csharp", SHAPE_MODEL, types=TYPES, size=24)
        note_cards(image, [
            ("Attribute", "GenerateShape"),
            ("Generated", "IShape, IPositioned"),
            ("Needs", "a parameterless constructor"),
        ], (1300, 240, W - 96, 1030))
        return image

    if step_id == "analyzers-1":
        image = slide_base(chapter_title, label, "OCSG001: a sealed model")
        draw_code(image, (96, 240, W - 96, 700), "Example from the documentation", "csharp", SEALED_MODEL, types=TYPES, size=24)
        draw_code(image, (96, 740, W - 96, 960), "Build output", "bash", SEALED_WARNING, types=TYPES, size=24)
        return image

    if step_id == "analyzers-2":
        image = slide_base(chapter_title, label, "OCSG002: a feature of another module")
        draw_code(image, (96, 240, 900, 800), "Declared by this module", "csharp", FEATURE_VALID, types=TYPES, size=22)
        draw_code(image, (940, 240, W - 96, 640), "Declared by another module", "csharp", FEATURE_INVALID, types=TYPES, size=22)
        draw_code(image, (940, 680, W - 96, 960), "Build output", "bash", FEATURE_WARNING, types=TYPES, size=22)
        return image

    if step_id == "build-1":
        image = slide_base(chapter_title, label, "OrchardCore.Commons.props")
        draw_code(image, (96, 240, W - 96, 660), "OrchardCore.Commons.props", "xml", BUILD_PROPS, types=TYPES, size=22)
        points(image, ["Imported by the Directory.Build.props of the framework, module and theme folders.",
                       "The generator project is an analyzer: its assembly is not referenced."], 110, 760)
        return image

    if step_id == "build-2":
        image = slide_base(chapter_title, label, "What the reference decides")
        bullets(image, EFFECTS)
        return image

    if step_id == "choose-1":
        image = slide_base(chapter_title, label, "Choosing an approach")
        bullets(image, CHOICES)
        return image

    raise SystemExit(f"No slide for {step_id}")
