"""The slides of the GraphQL settings, permissions and hidden fields video. assemble.py calls slide() for each step of a
"slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The helpers
below only measure the text before it is drawn, so a slide that would overflow fails when it is built.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, MARGIN, MUTED, TEXT, W, bullets, draw_code, font, mono, note_cards, slide_base

TYPES = {"GraphQLContentOptions", "ContentItemType", "ContentItem", "AutoroutePart", "IServiceCollection", "Permission",
         "LocalizationSource", "GraphQLPermissions", "ObjectGraphType", "UserPickerField", "UserType", "User",
         "UserPickerFieldQueryObjectType", "IStringLocalizer", "ListGraphType", "IEnumerable", "UsersPermissions"}

# The options of the OrchardCore:Apis:GraphQL section, with sample values.
CONFIG = """
{
  "OrchardCore": {
    "Apis": {
      "GraphQL": {
        "ExposeExceptions": false,
        "MaxDepth": 100,
        "MaxComplexity": 100,
        "FieldImpact": 2.0,
        "DefaultNumberOfResults": 100,
        "MaxNumberOfResults": 1000,
        "MaxNumberOfResultsValidationMode": "Default"
      }
    }
  }
}
"""

CONFIG_NOTES = [
    ("Under OrchardCore", "Apis, then GraphQL."),
    ("Shell configuration", "A tenant can override it."),
    ("Every option", "is optional."),
]

RESULTS = [
    ("1", "DefaultNumberOfResults: 100", "Results a content type query returns when it sets no first argument."),
    ("2", "MaxNumberOfResults: 1000", "The maximum for the first and last arguments."),
    ("3", "MaxNumberOfResultsValidationMode", "Enabled: validation error. Disabled: logged. Default: Enabled in Development."),
]

PROTECTION = [
    ("1", "MaxDepth: 100", "How deeply a query can nest."),
    ("2", "MaxComplexity", "The largest complexity a query can have. Not set by default."),
    ("3", "FieldImpact", "What each object field adds to the complexity. Not set by default."),
    ("4", "ExposeExceptions", "Sends exception details to clients. On by default only in Development."),
]

CHECKS = [
    ("1", "Execute GraphQL", "Required for the endpoint and for the Tools, GraphiQL menu item."),
    ("2", "Execute GraphQL Mutations", "Required for a mutation operation. It also grants Execute GraphQL."),
    ("3", "View own content", "Required to query a content type. View all content returns every item."),
    ("4", "The contentItem query", "Returns null when the user cannot view the item."),
]

PERMISSIONS = """
public static class GraphQLPermissions
{
    // ...

    public static readonly Permission ExecuteGraphQLMutations = new(
        "ExecuteGraphQLMutations",
        LocalizationSource.Create("Execute GraphQL Mutations.", typeof(GraphQLPermissions)));

    public static readonly Permission ExecuteGraphQL = new(
        "ExecuteGraphQL",
        LocalizationSource.Create("Execute GraphQL.", typeof(GraphQLPermissions)),
        [ExecuteGraphQLMutations]);
}
"""

FIELD = """
public class UserPickerFieldQueryObjectType : ObjectGraphType<UserPickerField>
{
    public UserPickerFieldQueryObjectType(
        IStringLocalizer<UserPickerFieldQueryObjectType> S)
    {
        // ...

        Field<ListGraphType<UserType>, IEnumerable<User>>("users")
            .Description(S["the user items"])
            .PagingArguments()
            .RequirePermission(UsersPermissions.ViewUsers)
            .ResolveAsync(resolve =>
            {
                // ...
            });
    }
}
"""

FIELD_NOTES = [
    ("Checked", "while the query is validated."),
    ("Unauthorized", "is returned as status 401."),
    ("The resource", "is optional."),
]

IGNORE = """
services.Configure<GraphQLContentOptions>(options =>
{
    // Removes the owner from the content item types
    // and from their where filter.
    options.IgnoreField<ContentItemType>(nameof(ContentItem.Owner));
});
"""

IGNORE_NOTES = [
    ("ContentItemType", "Every content type query returns it."),
    ("Also hidden", "from the where filter."),
]

CONFIGURE = """
services.Configure<GraphQLContentOptions>(options =>
{
    // Collapse a part for every content type.
    options.ConfigurePart<AutoroutePart>(partOptions =>
    {
        partOptions.Collapse = true;
    });

    // Hide a content type, or a query of a module.
    options.ConfigureContentType("SiteLayers", contentTypeOptions =>
    {
        contentTypeOptions.Hidden = true;
    });
});
"""

CONFIGURE_NOTES = [
    ("ConfigurePart", "A part, in every type."),
    ("ConfigureContentType", "A type, or a module query."),
]

STEREOTYPE = """
services.Configure<GraphQLContentOptions>(options =>
{
    // Content types with this stereotype get a query.
    options.DiscoverableSterotypes.Add("ExampleStereotype");
});
"""

STEREOTYPE_NOTES = [
    ("No stereotype", "The type gets a query."),
    ("A stereotype", "No query, unless it is listed."),
]

FORCE = [
    ("1", "In the editor", "Hide is selected on the content type."),
    ("2", "In code", "Hidden is set to false."),
    ("check", "In the schema", "The content type is hidden."),
]


def _fail(message):
    raise SystemExit(f"slides.py: {message}")


def _check_bullets(items):
    """A card's title and text start 140 px into the card and must end inside the right margin."""
    room = W - 2 * MARGIN - 140 - 30
    for _, title, text in items:
        if font(36, "Bold").getlength(title) > room or font(28).getlength(text) > room:
            _fail(f"a card is wider than the slide: {title!r}")


def _check_notes(notes, width):
    room = width - 80
    for title, text in notes:
        if font(34, "Bold").getlength(title) > room or font(30).getlength(text) > room:
            _fail(f"a note card is wider than its box: {title!r}")


def _cards(image, items, active=None):
    _check_bullets(items)
    bullets(image, items, active=active)


def _notes(image, notes, box):
    _check_notes(notes, box[2] - box[0])
    note_cards(image, notes, box)


def _code(image, box, title, language, code, size):
    """A code panel; fails when a line is wider than the panel, or the lines are taller than it."""
    x0, y0, x1, y1 = box
    lines = code.strip("\n").split("\n")
    if len(lines) > 20:
        _fail(f"{title} has {len(lines)} lines; at most 20")
    if max(mono(size).getlength(line) for line in lines) > x1 - x0 - 60:
        _fail(f"a line of {title} is wider than the panel")
    if 74 + int(size * 1.42) * len(lines) + 20 > y1 - y0:
        _fail(f"{title} is taller than the panel")
    draw_code(image, box, title, language, code, types=TYPES, size=size)


def _heading(heading):
    if font(52, "Bold").getlength(heading) > W - 2 * MARGIN:
        _fail(f"the heading is wider than the slide: {heading!r}")


def _base(chapter_title, label, heading):
    _heading(heading)
    return slide_base(chapter_title, label, heading)


def slide(step_id, chapter_title, label):
    if step_id == "options-1":
        image = _base(chapter_title, label, "The GraphQL configuration section")
        _code(image, (96, 240, 1060, 920), "appsettings.json", "json", CONFIG, 26)
        _notes(image, CONFIG_NOTES, (1100, 240, W - 96, 1030))
        return image

    if step_id in ("options-2", "options-3"):
        image = _base(chapter_title, label, "Limit the number of results")
        _cards(image, RESULTS, active={0, 1} if step_id == "options-2" else {2})
        return image

    if step_id in ("options-4", "options-5"):
        image = _base(chapter_title, label, "Protect the server")
        _cards(image, PROTECTION, active={0, 1, 2} if step_id == "options-4" else {3})
        return image

    if step_id in ("levels-1", "levels-3", "levels-4"):
        image = _base(chapter_title, label, "Where permissions apply")
        _cards(image, CHECKS, active={{"levels-1": 0, "levels-3": 2, "levels-4": 3}[step_id]})
        return image

    if step_id == "levels-2":
        image = _base(chapter_title, label, "One permission implies another")
        _code(image, (96, 240, W - 96, 830), "GraphQLPermissions.cs", "csharp", PERMISSIONS, 26)
        draw = ImageDraw.Draw(image)
        y = 895
        for index, note in enumerate([
            "The last argument lists the permissions that imply this one.",
            "Holding Execute GraphQL Mutations satisfies a check for Execute GraphQL.",
        ]):
            if font(30).getlength(note) > W - 2 * MARGIN - 60:
                _fail(f"a note is wider than the slide: {note!r}")
            draw.ellipse((MARGIN + 4, y - 7, MARGIN + 18, y + 7), fill=GREEN)
            draw.text((MARGIN + 40, y), note, font=font(30), fill=TEXT if index == 0 else MUTED, anchor="lm")
            y += 64
        return image

    if step_id == "field-1":
        image = _base(chapter_title, label, "Require a permission on a field")
        _code(image, (96, 240, 1160, 900), "UserPickerFieldQueryObjectType.cs", "csharp", FIELD, 22)
        _notes(image, FIELD_NOTES, (1200, 240, W - 96, 1030))
        return image

    if step_id == "code-1":
        image = _base(chapter_title, label, "Hide a field of the content item types")
        _code(image, (96, 240, 1160, 640), "Startup.cs", "csharp", IGNORE, 26)
        _notes(image, IGNORE_NOTES, (1200, 240, W - 96, 1030))
        return image

    if step_id == "code-2":
        image = _base(chapter_title, label, "Collapse a part, hide a type or a query")
        _code(image, (96, 240, 1160, 860), "Startup.cs", "csharp", CONFIGURE, 24)
        _notes(image, CONFIGURE_NOTES, (1200, 240, W - 96, 1030))
        return image

    if step_id == "code-3":
        image = _base(chapter_title, label, "Expose the content types of a stereotype")
        _code(image, (96, 240, 1160, 600), "Startup.cs", "csharp", STEREOTYPE, 26)
        _notes(image, STEREOTYPE_NOTES, (1200, 240, W - 96, 1030))
        return image

    if step_id == "code-4":
        image = _base(chapter_title, label, "Code only turns a setting on")
        _cards(image, FORCE)
        return image

    raise SystemExit(f"No slide for {step_id}")
