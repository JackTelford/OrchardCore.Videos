"""The slides of the "Extend GraphQL" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The code is taken from the Orchard Core source (the Autoroute, Localization and ContentManagement.GraphQL libraries)
and trimmed with // ...
"""
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "AutoroutePart", "AutorouteQueryObjectType", "AutorouteInputObjectType", "AutoroutePartIndex",
    "AutoroutePartIndexAliasProvider", "ContentPart", "ObjectGraphType", "WhereInputObjectGraphType",
    "IStringLocalizer", "StringGraphType", "BooleanGraphType", "IIndexAliasProvider", "IndexAlias", "ValueTask",
    "IEnumerable", "IServiceCollection", "StartupBase", "Startup", "ISchemaBuilder", "ISchema", "FieldType",
    "ListGraphType", "CultureQueryObjectType", "LockedAsyncFieldResolver", "SiteCulture", "SiteCulturesQuery",
    "IResolveFieldContext", "ILocalizationService", "GraphQLFilter", "ContentItem", "ContentItemFilters",
    "IQuery", "ContentItemIndex", "Task", "Contents", "CommonPermissions", "GraphQLContentOptions",
    "IGraphQLFilter",
}

CODE_LEFT, CODE_TOP, CODE_RIGHT = 96, 240, W - 96

AUTOROUTE_PART = """
public class AutoroutePart : ContentPart
{
    // ...

    public string Path { get; set; }

    public bool SetHomepage { get; set; }

    // ...
}
"""

AUTOROUTE_OBJECT_TYPE = """
public class AutorouteQueryObjectType : ObjectGraphType<AutoroutePart>
{
    public AutorouteQueryObjectType(IStringLocalizer<AutorouteQueryObjectType> S)
    {
        Name = "AutoroutePart";
        Description = S["Custom URLs (permalinks) for your content item."];

        Field(x => x.Path).Description(S["The permalinks for your content item."]);
    }
}
"""

STARTUP_OBJECT_TYPE = """
[RequireFeatures("OrchardCore.Apis.GraphQL")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddObjectGraphType<AutoroutePart, AutorouteQueryObjectType>();
        // ...
    }
}
"""

AUTOROUTE_INPUT_TYPE = """
public class AutorouteInputObjectType : WhereInputObjectGraphType<AutoroutePart>
{
    public AutorouteInputObjectType(IStringLocalizer<AutorouteInputObjectType> stringLocalizer)
        : base(stringLocalizer)
    {
        Name = "AutoroutePartInput";
        Description = S["the custom URL part of the content item"];

        AddScalarFilterFields<StringGraphType>("path", S["the path of the content item to filter"]);
    }
}
"""

ALIAS_PROVIDER = """
public class AutoroutePartIndexAliasProvider : IIndexAliasProvider
{
    private static readonly IndexAlias[] s_aliases =
    [
        new IndexAlias
        {
            Alias = "autoroutePart",
            Index = nameof(AutoroutePartIndex),
            IndexType = typeof(AutoroutePartIndex),
        }
    ];

    public ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync()
    {
        return ValueTask.FromResult<IEnumerable<IndexAlias>>(s_aliases);
    }
}
"""

STARTUP_FILTER = """
[RequireFeatures("OrchardCore.Apis.GraphQL")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddInputObjectGraphType<AutoroutePart, AutorouteInputObjectType>();
        services.AddObjectGraphType<AutoroutePart, AutorouteQueryObjectType>();
        services.AddTransient<IIndexAliasProvider, AutoroutePartIndexAliasProvider>();
        services.AddWhereInputIndexPropertyProvider<AutoroutePartIndex>();
    }
}
"""

SCHEMA_BUILDER = """
public sealed class SiteCulturesQuery : ISchemaBuilder
{
    // ...

    public Task BuildAsync(ISchema schema)
    {
        // ...

        var field = new FieldType
        {
            Name = "SiteCultures",
            Description = S["The active cultures configured for the site."],
            Type = typeof(ListGraphType<CultureQueryObjectType>),
            Resolver = new LockedAsyncFieldResolver<IEnumerable<SiteCulture>>(ResolveAsync),
        };

        schema.Query.AddField(field);

        return Task.CompletedTask;
    }
}
"""

RESOLVER_LINE = """
Resolver = new LockedAsyncFieldResolver<IEnumerable<SiteCulture>>(ResolveAsync),
"""

RESOLVE = """
private async ValueTask<IEnumerable<SiteCulture>> ResolveAsync(IResolveFieldContext resolveContext)
{
    var localizationService = resolveContext.RequestServices.GetService<ILocalizationService>();

    var defaultCulture = await localizationService.GetDefaultCultureAsync();
    var supportedCultures = await localizationService.GetSupportedCulturesAsync();

    var cultures = supportedCultures.Select(culture =>
        new SiteCulture
        {
            Culture = culture,
            IsDefault = string.Equals(defaultCulture, culture, StringComparison.OrdinalIgnoreCase),
        });

    return cultures;
}
"""

CULTURE_TYPES = """
public class SiteCulture
{
    public string Culture { get; set; }

    public bool IsDefault { get; set; }
}

public class CultureQueryObjectType : ObjectGraphType<SiteCulture>
{
    public CultureQueryObjectType(IStringLocalizer<CultureQueryObjectType> S)
    {
        Name = "SiteCulture";

        Field<StringGraphType>("culture")
            .Description(S["The culture code."])
            .Resolve(context => context.Source.Culture);

        Field<BooleanGraphType>("default")
            .Description(S["Whether this is the default culture."])
            .Resolve(context => context.Source.IsDefault);
    }
}
"""

STARTUP_BUILDER = """
[RequireFeatures("OrchardCore.Apis.GraphQL")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISchemaBuilder, SiteCulturesQuery>();
        services.AddTransient<CultureQueryObjectType>();
    }
}
"""

IDENTIFIER = """
public Task<string> GetIdentifierAsync()
    => Task.FromResult(string.Empty);
"""

HIDDEN_CHECK = """
public Task BuildAsync(ISchema schema)
{
    if (_graphQLContentOptions.IsHiddenByDefault("SiteCultures"))
    {
        return Task.CompletedTask;
    }

    // ...
}
"""

HIDE_OPTION = """
services.Configure<GraphQLContentOptions>(options =>
{
    options.ConfigureContentType("SiteCultures", x =>
    {
        x.Hidden = true;
    });
});
"""

REQUIRE_PERMISSION = """
query.RequirePermission(
    Contents.CommonPermissions.ViewOwnContent,
    typeDefinition.Name);
"""

FILTER_CLASS = """
public sealed class ContentItemFilters : GraphQLFilter<ContentItem>
{
    public override async Task<IQuery<ContentItem>> PreQueryAsync(
        IQuery<ContentItem> query, IResolveFieldContext context)
    {
        // ...
    }

    public override async Task<IEnumerable<ContentItem>> PostQueryAsync(
        IEnumerable<ContentItem> contentItems, IResolveFieldContext context)
    {
        // ...
    }
}
"""

REGISTER_FILTER = """
services.AddGraphQLFilterType<ContentItem, ContentItemFilters>();
"""

RUN_FILTERS = """
foreach (var filter in filters)
{
    preQuery = await filter.PreQueryAsync(preQuery, context);
}

var query = preQuery.With<ContentItemIndex>();
"""


def slide(step_id, chapter_title, label):
    if step_id == "schema-1":
        image = slide_base(chapter_title, label, "Built from registered services")
        bullets(image, [
            ("1", "Registered types", "Every object type and input type in the services is added to the schema."),
            ("2", "Schema builders", "Each ISchemaBuilder adds its fields, such as the content type queries."),
            ("3", "Empty roots", "A Query, Mutation or Subscription root without fields is removed."),
        ], active={0, 1})
        return image

    if step_id == "schema-2":
        image = slide_base(chapter_title, label, "Kept for each culture")
        bullets(image, [
            ("1", "One schema per culture", "A request in another culture gets a schema of its own."),
            ("2", "An identifier per builder", "GetIdentifierAsync returns a value that changes with the data a builder reads."),
            ("3", "Content definitions", "The content type builder reports the identifier of the content definitions."),
        ])
        return image

    if step_id == "schema-3":
        image = slide_base(chapter_title, label, "Four extension points")
        bullets(image, [
            ("1", "Object type", "Describes a content part, or any class, for the schema."),
            ("2", "Input type and index", "Filters the content type queries by a part."),
            ("3", "Schema builder", "Adds a query that belongs to no content type."),
            ("4", "GraphQLFilter", "Runs code before and after the content type queries."),
        ])
        return image

    if step_id == "schema-4":
        image = slide_base(chapter_title, label, "What a module needs")
        bullets(image, [
            ("1", "Reference the libraries",
             "OrchardCore.Apis.GraphQL.Abstractions, and OrchardCore.ContentManagement.GraphQL for content."),
            ("2", "Register in a Startup class", "Add the types, builders and filters to the services of your module."),
            ("3", "Test in GraphiQL", "Tools, GraphiQL shows the schema and runs queries."),
        ])
        return image

    if step_id == "part-1":
        image = slide_base(chapter_title, label, "A content part")
        draw_code(image, (CODE_LEFT, CODE_TOP, 940, 700), "AutoroutePart.cs", "csharp", AUTOROUTE_PART,
                  types=TYPES, size=26)
        note_cards(image, [
            ("A content part", "is a C# class with properties."),
            ("GraphQL needs", "a graph type that describes it."),
        ], (990, 260, CODE_RIGHT, 700))
        return image

    if step_id == "part-2":
        image = slide_base(chapter_title, label, "Describe it with an object type")
        draw_code(image, (CODE_LEFT, CODE_TOP, CODE_RIGHT, 800), "AutorouteQueryObjectType.cs", "csharp",
                  AUTOROUTE_OBJECT_TYPE, types=TYPES, size=26, highlight={1, 5, 6, 8})
        return image

    if step_id == "part-3":
        image = slide_base(chapter_title, label, "Register it")
        draw_code(image, (CODE_LEFT, CODE_TOP, CODE_RIGHT, 700), "Startup.cs", "csharp", STARTUP_OBJECT_TYPE,
                  types=TYPES, size=28, highlight={1, 6})
        return image

    if step_id == "part-4":
        image = slide_base(chapter_title, label, "How the part is found")
        bullets(image, [
            ("1", "Matched by name", "The name of the part class must equal the name of the part on the content type."),
            ("2", "One field per part", "AutoroutePart becomes the field autoroute: the name without Part, in camel case."),
            ("3", "Every content type", "Each content type that has the part gets the field, unless its GraphQL options hide or collapse the part."),
        ])
        return image

    if step_id == "filter-1":
        image = slide_base(chapter_title, label, "Add an input type")
        draw_code(image, (CODE_LEFT, CODE_TOP, CODE_RIGHT, 780), "AutorouteInputObjectType.cs", "csharp",
                  AUTOROUTE_INPUT_TYPE, types=TYPES, size=26, highlight={1, 3, 4, 9})
        return image

    if step_id == "filter-2":
        image = slide_base(chapter_title, label, "Connect it to an index")
        draw_code(image, (CODE_LEFT, CODE_TOP, CODE_RIGHT, 960), "AutoroutePartIndexAliasProvider.cs", "csharp",
                  ALIAS_PROVIDER, types=TYPES, size=25, highlight={1, 7, 8, 9})
        return image

    if step_id == "filter-3":
        image = slide_base(chapter_title, label, "Register all three")
        draw_code(image, (CODE_LEFT, CODE_TOP, CODE_RIGHT, 790), "Startup.cs", "csharp", STARTUP_FILTER,
                  types=TYPES, size=26, highlight={6, 8, 9})
        return image

    if step_id == "query-1":
        image = slide_base(chapter_title, label, "A schema builder adds a field")
        draw_code(image, (CODE_LEFT, CODE_TOP, CODE_RIGHT, 1030), "SiteCulturesQuery.cs", "csharp", SCHEMA_BUILDER,
                  types=TYPES, size=23, highlight={1, 5, 9, 11, 12, 13, 14, 17})
        return image

    if step_id == "query-2":
        image = slide_base(chapter_title, label, "Resolve the field")
        draw_code(image, (CODE_LEFT, CODE_TOP, CODE_RIGHT, 380), "SiteCulturesQuery.cs: BuildAsync", "csharp",
                  RESOLVER_LINE, types=TYPES, size=24)
        draw_code(image, (CODE_LEFT, 410, CODE_RIGHT, 1030), "SiteCulturesQuery.cs: ResolveAsync", "csharp", RESOLVE,
                  types=TYPES, size=23, highlight={1, 3})
        return image

    if step_id == "query-3":
        image = slide_base(chapter_title, label, "An object type for each item")
        draw_code(image, (CODE_LEFT, CODE_TOP, CODE_RIGHT, 1030), "SiteCulture.cs, CultureQueryObjectType.cs", "csharp",
                  CULTURE_TYPES, types=TYPES, size=22)
        return image

    if step_id == "query-4":
        image = slide_base(chapter_title, label, "Register the builder")
        draw_code(image, (CODE_LEFT, CODE_TOP, CODE_RIGHT, 660), "Startup.cs", "csharp", STARTUP_BUILDER,
                  types=TYPES, size=26, highlight={6, 7})
        draw_code(image, (CODE_LEFT, 710, CODE_RIGHT, 880), "SiteCulturesQuery.cs", "csharp", IDENTIFIER,
                  types=TYPES, size=26)
        return image

    if step_id == "query-5":
        image = slide_base(chapter_title, label, "Hide it, or guard it")
        draw_code(image, (CODE_LEFT, CODE_TOP, 1060, 640), "SiteCulturesQuery.cs", "csharp", HIDDEN_CHECK,
                  types=TYPES, size=24)
        draw_code(image, (CODE_LEFT, 680, 1060, 1020), "Startup.cs", "csharp", HIDE_OPTION, types=TYPES, size=24)
        draw_code(image, (1090, CODE_TOP, CODE_RIGHT, 520), "ContentTypeQuery.cs", "csharp", REQUIRE_PERMISSION,
                  types=TYPES, size=24)
        return image

    if step_id == "hook-1":
        image = slide_base(chapter_title, label, "A GraphQLFilter")
        draw_code(image, (CODE_LEFT, CODE_TOP, CODE_RIGHT, 880), "ContentItemFilters.cs", "csharp", FILTER_CLASS,
                  types=TYPES, size=26)
        return image

    if step_id == "hook-2":
        image = slide_base(chapter_title, label, "What ContentItemFilters does")
        bullets(image, [
            ("1", "PreQueryAsync", "Limits the query to the content the user may view."),
            ("2", "PostQueryAsync", "Checks each loaded item again with the authorization service."),
        ])
        return image

    if step_id == "hook-3":
        image = slide_base(chapter_title, label, "Register the filter")
        draw_code(image, (CODE_LEFT, CODE_TOP, CODE_RIGHT, 400), "ServiceCollectionExtensions.cs", "csharp", REGISTER_FILTER,
                  types=TYPES, size=28)
        draw_code(image, (CODE_LEFT, 450, CODE_RIGHT, 860), "ContentItemsFieldType.cs", "csharp", RUN_FILTERS,
                  types=TYPES, size=26)
        return image

    if step_id == "source-1":
        image = slide_base(chapter_title, label, "Working examples in the source")
        bullets(image, [
            ("1", "Autoroute and Alias", "An object type, an input type, an alias provider and an index."),
            ("2", "Localization and Layers", "A schema builder with an object type."),
            ("3", "Media", "A schema builder with arguments, and an object type for the media field."),
            ("4", "Content management GraphQL library", "ContentItemFilters, a GraphQLFilter registered with AddGraphQLFilterType."),
        ])
        return image

    raise SystemExit(f"No slide for {step_id}")
