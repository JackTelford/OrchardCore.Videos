# The feature catalog

`features.json` lists everything Orchard Core ships that a video could show: every feature a site can enable, every
theme, the libraries that have documentation of their own, and the documentation pages that explain a concept rather
than a feature. It is read from the source of `OrchardCMS/OrchardCore` by `tools/catalog.py`, not written by hand;
[SOURCE.md](SOURCE.md) records the commit and the date of the scan, and how to refresh it.

The tracker (`tracker/videos.json`) maps every entry to at least one video, or to a reason in `tracker/NO-VIDEO.md`;
`python tools/tracker.py validate` fails when an entry is neither.

## Files

| File | What it is |
| --- | --- |
| `features.json` | The catalog: `{"source": {...}, "features": [...]}` |
| `SOURCE.md` | The commit and date of the scan, and how to run it again |
| `EXCLUDED.md` | Every module, theme, template and library folder with no entry, and why |
| `DOC-ISSUES.md` | Places where the documentation and the code disagree, found while writing the videos |

## An entry

```json
{
  "id": "OrchardCore.Media",
  "name": "Media",
  "kind": "feature",
  "module": "OrchardCore.Media",
  "category": "Content Management",
  "audience": "both",
  "docs": ["src/docs/reference/modules/Media/README.md"],
  "admin": "Media > Library",
  "permissions": ["ManageMediaContent", "..."],
  "dependencies": ["OrchardCore.ContentTypes"],
  "hasUi": true,
  "summary": "The media module adds media management support.",
  "flags": [],
  "surfaces": { "adminMenu": ["Media > Library", "Media > Profiles", "Settings > Media > Options"], "...": [] }
}
```

| Field | Meaning |
| --- | --- |
| `id` | The feature id (`[assembly: Feature(Id = ...)]`, or the project name of a module that declares no feature). Other kinds use a prefix: `topic:<page>`, `guide:<page>`; a library uses its project name. |
| `name` | The feature's name as the Features page shows it. |
| `kind` | `feature`, `theme`, `library` (a framework library with its own documentation), `topic` (a reference page about a concept, such as display management), or `guide` (a getting-started, guide or key-topics page). |
| `module` | The project that declares it (`OrchardCore.Media`), or `null` for topics and guides. |
| `category` | The manifest's `Category`: the group the Features page shows it in. Topics, libraries and guides get one of the categories below. |
| `audience` | `admin` (site owners and editors: the admin UI and the front end), `developer` (module and theme authors: code and configuration), or `both`. A feature with a screen defaults to `both`; the corrections are in `AUDIENCE` in `tools/catalog.py`. |
| `docs` | The documentation pages that describe it, relative to the Orchard Core repository root. |
| `admin` | The first admin menu entry it adds, as labeled on screen (`Settings > Media > Options`), or `null`. All of them are in `surfaces.adminMenu`. |
| `permissions` | The names of the permissions its permission providers register. |
| `dependencies` | The feature ids it depends on (a theme also lists its base theme). |
| `hasUi` | Whether it adds something a person sees: a menu entry, a settings page, an editor, a part or field, a workflow activity, a deployment step, a theme, a Liquid filter or a shortcode. |
| `summary` | The manifest's `Description`. |
| `flags` | From the manifest: `EnabledByDependencyOnly` (not listed on the Features page; enabled by the features that need it), `DefaultTenantOnly`, `IsAlwaysEnabled`, `Obsolete` (its name says so; a newer feature replaces it), `AdminTheme`. |
| `surfaces` | What it registers, read from its Startup classes: `adminMenu`, `permissions`, `settings` (site settings display drivers), `contentParts`, `contentFields`, `contentTypes` (with the stereotype, such as `Widget`), `workflowActivities` (by display name), `deploymentSteps`, `recipeSteps` (by step name), `liquidFilters`, `liquidTags`, `shortcodes`, `graphql`, `api` (HTTP routes), `backgroundTasks`, and `conditional` (Startup classes that only run when other features are enabled, with those features). |

Surfaces are nested under the feature that registers them, rather than listed as entries of their own: a permission,
a recipe step or a workflow activity is shown in the video of its feature. A Startup class with a `[Feature]`
attribute belongs to that feature; one without belongs to the feature with the module's id or, in a module that has
no such feature, to every feature of the module (the rule of `ExtensionManager`). `[RequireFeatures]` Startup classes
belong to their own feature, and are listed under `conditional`.

## Categories

Features keep the category of their manifest, so the catalog groups them the way the Features page does
(`Content Management`, `Search`, `Security`, `Infrastructure`, `Internationalization`, `Workflows`, and the
provider names `Google`, `Meta`, `OpenID Connect`, ...). The other kinds use these:

| Category | What it holds |
| --- | --- |
| `Getting Started` | Setting up a site and a development environment; the overview pages |
| `Guides` | The step-by-step guides of `src/docs/guides` |
| `Key Topics` | The concept pages of `src/docs/topics` |
| `Display`, `Theming` | Display management, shapes, placement, Razor helpers, theme conventions |
| `Infrastructure`, `Configuration` | Shells, data, migrations, configuration, logging, telemetry |
| `Developer` | Writing modules: the module system, source generators, libraries |
| `Security`, `Content Management`, `Localization` | Topics and libraries of those areas |

The videos are grouped differently, into learning paths (series) for each audience: see `tracker/README.md`.

## How the catalog was checked

`tools/catalog.py` fails when a feature id, a constant or a documentation path can't be resolved. After each scan,
an independent check, written separately from the generator, compared the catalog with the source:

- every folder of `src/OrchardCore.Modules`, `src/OrchardCore.Themes` and `src/OrchardCore` is a module of an entry
  or is listed in `EXCLUDED.md`;
- every `[assembly: Feature]` of every manifest (and every module that declares none) is an entry, or its module is
  in `EXCLUDED.md` (the sample modules and the `dotnet new` templates);
- every page under `src/docs/reference` is in the `docs` of an entry.

The scan of 2026-11-11 found 195 declared features (186 in the catalog, 3 in the sample modules, 6 themes) and no
unexplained omission.
