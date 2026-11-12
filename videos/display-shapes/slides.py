"""The slides of the "Shapes: How Orchard Core Renders a Page" video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py. The code comes from the documentation pages of the video
(topics/display/shapes.md, reference/modules/DisplayManagement) and from the Orchard Core source, trimmed to what the
narration says. The checks in _code(), _cards() and _notes() stop a slide whose text would overflow its box.
"""
from PIL import ImageFont

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import H, MARGIN, W, bullets, draw_code, font, mono, note_cards, slide_base

TYPES = {
    "Car", "CarShapes", "IShape", "IShapeFactory", "IDisplayHelper", "IHtmlContent", "ILayoutAccessor",
    "IDisplayResult", "IShapeAttributeProvider", "ProductPart", "ProductViewModel", "BuildPartDisplayContext",
    "ContentPartDisplayDriver", "ProductPartDisplayDriver", "TagBuilder",
}

# ----- Code of the slides -----

CREATE = """
@{
    dynamic car = await New.Car(Brand: "Acme", Color: "Blue");
}

@await DisplayAsync(car)
"""

CAR_TEMPLATE = """
<div>A @Model.Color @Model.Brand car.</div>
"""

LIQUID = """
{% assign car = "Car" | shape_new %}
{% shape_add_properties car Brand: "Acme", Color: "Blue" %}
{{ car | shape_render }}
"""

SERVICE = """
dynamic car = await _shapeFactory.CreateAsync("Car");
car.Brand = "Acme";
car.Color = "Blue";

var html = await _displayHelper.ShapeExecuteAsync((IShape)car);
"""

PROPERTIES = """
dynamic car = await _shapeFactory.CreateAsync("Car");

car.Brand = "Acme";

var shape = (IShape)car;
var brand = shape.Properties["Brand"]; // "Acme"
"""

TYPED = """
public class Car
{
    public string Brand { get; set; }
    public string Color { get; set; }
}

var shape = await _shapeFactory.CreateAsync<Car>("Car", car =>
{
    car.Brand = "Acme";
    car.Color = "Blue";
});

// ...
"""

EXCEPTION = """
The shape type 'Car' is not found for the theme 'Contoso'
"""

SHAPE_METHOD = """
public sealed class CarShapes : IShapeAttributeProvider
{
    [Shape("Car")]
    public IHtmlContent Car(IShape Shape)
    {
        var car = (Car)Shape;
        var tagBuilder = Shape.GetTagBuilder("div");
        tagBuilder.InnerHtml.Append($"A {car.Color} {car.Brand} car.");

        return tagBuilder;
    }
}

services.AddShapeAttributes<CarShapes>();
"""

LAYOUT_TREE = """
Layout
|-- Header
|   |-- Branding
|   `-- Menu
|-- Content
|   `-- Content
|       |-- TitlePart
|       `-- HtmlBodyPart
`-- Footer
    `-- Copyright
"""

ZONE_CODE = """
dynamic layout = await _layoutAccessor.GetLayoutAsync();
var menu = await _shapeFactory.CreateAsync("Menu");

await layout.Header.AddAsync(menu, "5");
"""

DRIVER = """
public sealed class ProductPartDisplayDriver
    : ContentPartDisplayDriver<ProductPart>
{
    public override IDisplayResult Display(
        ProductPart part,
        BuildPartDisplayContext context)
    {
        return Initialize<ProductViewModel>("ProductPart", model =>
        {
            model.Title = part.Title;
            model.Price = part.Price;
        })
        .Location("Detail", "Content:5")
        .Location("Summary", "Content:1");
    }
}
"""

DEBUG_COMMENTS = """
<!--shape-start type:Menu bindings:menu__main => Areas/Contoso/Views/Menu-Main.cshtml (razor) -->
...
<!--shape-end type:Menu -->
"""


# ----- Helpers with overflow checks -----

def _wide(text, text_font):
    return text_font.getlength(text)


def _code(image, box, title, language, code, size=26, highlight=None):
    """draw_code, after checking that the longest line and the number of lines fit in the panel."""
    x0, y0, x1, y1 = box
    lines = code.strip("\n").split("\n")
    code_font = mono(size)
    widest = max(_wide(line, code_font) for line in lines)
    height = 74 + len(lines) * int(size * 1.42)
    if widest > x1 - x0 - 60:
        raise SystemExit(f"Code panel {title!r} overflows: {widest:.0f} px of {x1 - x0 - 60} px")
    if height > y1 - y0 - 8:
        raise SystemExit(f"Code panel {title!r} overflows: {height} px of {y1 - y0 - 8} px")
    draw_code(image, box, title, language, code, types=TYPES, highlight=highlight, size=size)


def _cards(image, items, top=250, active=None):
    """bullets, after checking that every card fits in the slide."""
    limit = W - 2 * MARGIN - 140 - 24
    for _, title, text in items:
        for value, face in ((title, font(36, "Bold")), (text, font(28))):
            if _wide(value, face) > limit:
                raise SystemExit(f"Card text overflows: {value!r}")
    if top + 150 + 176 * (len(items) - 1) > H - 30:
        raise SystemExit(f"Cards overflow the slide: {items[0][1]!r}")
    bullets(image, items, top=top, active=active)


def _notes(image, notes, box):
    """note_cards, after checking that every card fits in its box."""
    x0, y0, x1, _ = box
    for title, text in notes:
        for value, face in ((title, font(34, "Bold")), (text, font(30))):
            if _wide(value, face) > x1 - x0 - 40 - 24:
                raise SystemExit(f"Note card text overflows: {value!r}")
    if y0 + 180 + 220 * (len(notes) - 1) > H - 30:
        raise SystemExit(f"Note cards overflow the slide: {notes[0][0]!r}")
    note_cards(image, notes, box)


def _base(chapter_title, label, heading):
    if _wide(heading, font(52, "Bold")) > W - 2 * MARGIN:
        raise SystemExit(f"Heading overflows: {heading!r}")
    return slide_base(chapter_title, label, heading)


# ----- The slides -----

def slide(step_id, chapter_title, label):
    if step_id in ("what-1", "what-2"):
        image = _base(chapter_title, label, "A shape: data, metadata and a binding")
        _cards(image, [
            ("1", "Properties", "The data a template reads, such as Brand and Color"),
            ("2", "Metadata", "Type, display type, alternates and wrappers"),
            ("3", "Items", "Ordered child shapes, such as the content of a zone"),
            ("4", "Binding", "The template or code that turns a shape into HTML"),
        ], active={0, 1, 2} if step_id == "what-1" else {3})
        return image

    if step_id == "create-1":
        image = _base(chapter_title, label, "Create and render a shape in Razor")
        _code(image, (96, 240, W - 96, 570), "Any Razor template", "csharp", CREATE, size=32)
        _cards(image, [
            ("1", "New", "Creates a shape from its type name and its properties"),
            ("2", "DisplayAsync", "Renders the shape and returns its HTML"),
        ], top=620)
        return image

    if step_id == "create-2":
        image = _base(chapter_title, label, "A Razor template is a binding")
        _code(image, (96, 240, W - 96, 400), "Views/Car.cshtml", "html", CAR_TEMPLATE, size=32)
        _cards(image, [
            ("1", "Shape type: Car", "Orchard Core looks for a binding with this name"),
            ("2", "Model", "The shape itself: Model.Color and Model.Brand are its properties"),
            ("3", "Result", "A Blue Acme car."),
        ], top=440)
        return image

    if step_id == "create-3":
        image = _base(chapter_title, label, "The same operations in Liquid")
        _code(image, (96, 240, W - 96, 490), "A Liquid template", "html", LIQUID, size=32)
        _cards(image, [
            ("1", "shape_new filter", "Creates a shape from its type name"),
            ("2", "shape_add_properties tag", "Sets properties on the shape"),
            ("3", "shape_render filter", "Renders the shape and writes its HTML"),
        ], top=530)
        return image

    if step_id == "create-4":
        image = _base(chapter_title, label, "Create and render a shape in a service")
        _code(image, (96, 240, W - 96, 560), "A service", "csharp", SERVICE, size=30)
        _cards(image, [
            ("1", "IShapeFactory", "Creates the shape from its type name"),
            ("2", "IDisplayHelper", "Renders the shape and returns IHtmlContent"),
        ], top=610)
        return image

    if step_id == "contract-1":
        image = _base(chapter_title, label, "Every shape implements IShape")
        _code(image, (96, 240, 1230, 640), "Properties", "csharp", PROPERTIES, size=30)
        _notes(image, [
            ("Metadata", "How to render the shape"),
            ("Properties", "The data a template reads"),
            ("Items", "Ordered child items"),
        ], (1280, 240, W - 96, 900))
        return image

    if step_id == "contract-2":
        image = _base(chapter_title, label, "The metadata of a shape")
        _cards(image, [
            ("1", "Type", "The logical name used to find a binding"),
            ("2", "DisplayType", "The context: Detail, Summary or Edit"),
            ("3", "Alternates", "More specific binding names, from lowest to highest priority"),
            ("4", "Wrappers", "Shape types rendered around this shape"),
        ])
        return image

    if step_id == "contract-3":
        image = _base(chapter_title, label, "A strongly typed shape")
        _code(image, (96, 240, 1250, 840), "A model class", "csharp", TYPED, size=28)
        _notes(image, [
            ("Shape type", "Car, the logical name"),
            (".NET type", "A proxy that derives from Car"),
            ("Requirements", "Not sealed, parameterless"),
        ], (1300, 240, W - 96, 900))
        return image

    if step_id == "render-1":
        image = _base(chapter_title, label, "The rendering pipeline")
        _cards(image, [
            ("1", "Displaying", "Global, descriptor and shape handlers run first"),
            ("2", "Resolve a binding", "Alternates from highest to lowest priority, then the shape type and its fallbacks"),
            ("3", "Execute the binding", "Then each wrapper renders around the result"),
            ("4", "Displayed", "Handlers run, and the final HTML is returned"),
        ])
        return image

    if step_id == "render-2":
        image = _base(chapter_title, label, "What a binding can be")
        _cards(image, [
            ("1", "Razor template", "A file in a Views folder, such as Car.cshtml"),
            ("2", "Liquid template", "A file in a Views folder, such as Car.liquid"),
            ("3", "Shape method", "A method marked with the Shape attribute"),
            ("4", "Binding resolver", "A dynamic implementation of IShapeBindingResolver"),
        ])
        return image

    if step_id == "render-3":
        image = _base(chapter_title, label, "The shape table")
        _cards(image, [
            ("1", "The application", "Highest priority"),
            ("2", "The active theme and its base themes", "Override the modules"),
            ("3", "The modules", "Lowest priority, in dependency order"),
        ], top=250)
        _code(image, (96, 820, W - 96, 970), "When no binding is found", "bash", EXCEPTION, size=30)
        return image

    if step_id == "name-1":
        image = _base(chapter_title, label, "From a file name to a binding name")
        _cards(image, [
            ("1", "Car.cshtml", "Binding Car"),
            ("2", "Car.Color-Blue.cshtml", "Binding Car_Color__Blue"),
            ("3", "Content-BlogPost.Summary.cshtml", "Binding Content_Summary__BlogPost"),
            ("4", "Views/Fields/Common.Text-FirstName.cshtml", "Binding Fields_Common_Text__FirstName"),
        ])
        return image

    if step_id == "code-1":
        image = _base(chapter_title, label, "A binding written in code")
        _code(image, (96, 240, 1230, 900), "CarShapes", "csharp", SHAPE_METHOD, size=26)
        _notes(image, [
            ("[Shape]", "Marks a binding method"),
            ("Parameters", "Shape data and services"),
            ("Registration", "AddShapeAttributes"),
        ], (1280, 240, W - 96, 900))
        return image

    if step_id == "zone-1":
        image = _base(chapter_title, label, "A layout holds zones")
        _code(image, (96, 240, 900, 780), "The Layout shape", "bash", LAYOUT_TREE, size=30)
        _notes(image, [
            ("Layout", "The root shape of a page"),
            ("Zone", "Renders its child items in order"),
            ("Theme", "Decides which zones it renders"),
        ], (950, 240, W - 96, 900))
        return image

    if step_id == "zone-2":
        image = _base(chapter_title, label, "Add a shape to a zone")
        _code(image, (96, 240, W - 96, 500), "A service", "csharp", ZONE_CODE, size=30)
        _cards(image, [
            ("1", "On demand", "Reading a zone that does not exist returns an empty placeholder"),
            ("2", "By position", "Numbers sort as numbers: 2 comes before 10"),
        ], top=550)
        return image

    if step_id == "driver-1":
        image = _base(chapter_title, label, "A display driver returns shapes")
        _cards(image, [
            ("1", "IDisplayResult", "What a display driver returns for each shape"),
            ("2", "ShapeResult", "Creates, configures and places one shape"),
            ("3", "Placement first", "A hidden or non-matching result is never created"),
        ])
        return image

    if step_id == "driver-2":
        image = _base(chapter_title, label, "Initialize and Location")
        _code(image, (96, 240, 1120, 900), "ProductPartDisplayDriver", "csharp", DRIVER, size=24)
        _notes(image, [
            ("Shape type", "The string ProductPart"),
            (".NET base type", "ProductViewModel"),
            ("Location", "Zone and position, per display type"),
        ], (1170, 240, W - 96, 900))
        return image

    if step_id == "debug-3":
        image = _base(chapter_title, label, "The shape debug comments")
        _code(image, (96, 240, W - 96, 450), "Page source", "html", DEBUG_COMMENTS, size=26)
        _cards(image, [
            ("1", "type", "The shape type, here Menu"),
            ("2", "bindings", "The name (lower case for a template), the source, and the engine: razor, liquid or code"),
            ("3", "shape-end", "Repeats the shape type after the content"),
        ], top=490)
        return image

    if step_id == "debug-4":
        image = _base(chapter_title, label, "When your template is not used")
        _cards(image, [
            ("1", "Read the comment", "The shape type, and the binding that won"),
            ("2", "Check the folder", "The template sits in one of the Views folders"),
            ("3", "Check the name", "The file name matches the binding name"),
            ("4", "Check the order", "A later alternate has higher priority"),
        ])
        return image

    raise SystemExit(f"No slide for {step_id}")
