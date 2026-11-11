"""Builds catalog/features.json, catalog/EXCLUDED.md and catalog/SOURCE.md from an Orchard Core checkout.

Standard library only. Usage:

    python tools/catalog.py --source <OrchardCore checkout> [--commit <sha>] [--date YYYY-MM-DD]

The catalog lists every feature Orchard Core ships, read from the source rather than from memory:

- every Module, Feature and Theme attribute of a Manifest.cs under src/OrchardCore.Modules and src/OrchardCore.Themes
  (a Module attribute is itself a feature, whose id is the project name);
- what each feature registers in its Startup classes (admin menus, permissions, content parts and fields, content
  types and widgets, workflow activities, deployment and recipe steps, settings pages, Liquid filters and tags,
  shortcodes, GraphQL and HTTP APIs, background tasks), nested under the feature's "surfaces";
- every documentation page under src/docs/reference (modules, core, themes), mapped to the features it documents, and
  the pages that document no feature as "topic" entries;
- the libraries of src/OrchardCore that have documentation but no feature, as "library" entries;
- the guides, getting-started and key-topics pages, as "guide" entries.

Everything else (the libraries a module uses, the project templates, the demo modules) is listed with a reason in
catalog/EXCLUDED.md. See catalog/README.md for the format.
"""
from __future__ import annotations

import argparse
import datetime
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CATALOG = ROOT / "catalog"

# Modules that exist to test or demonstrate the framework, never shipped to a site.
DEMO_MODULES = {
    "OrchardCore.Demo": "A sample module used by the functional tests and as a code sample; not shipped to sites.",
    "OrchardCore.Mvc.HelloWorld": "A sample MVC module for OrchardCore.Mvc.Web, used by the tests; not shipped to sites.",
}

# Documentation folders (src/docs/reference/modules/<Folder>) whose name does not match a module or feature id.
DOC_FOLDER_FEATURES = {
    "Apis.GraphQL.Abstractions": ["OrchardCore.Apis.GraphQL"],
    "AzureAISearch": ["OrchardCore.AzureAI"],
    "DataSources": ["OrchardCore.DataPipelines.Core"],
    "Flow": ["OrchardCore.Flows"],
    "Localize": ["OrchardCore.Localization"],
    "Media.Slugify": ["OrchardCore.Media.Slugify"],
    "SQLIndexing": ["OrchardCore.Queries.Sql"],
    "Search": ["OrchardCore.Search"],
    "X": ["OrchardCore.Twitter"],
    "Menu": ["OrchardCore.Menu"],
}

# Single pages whose features are not those of their folder.
DOC_PAGE_FEATURES = {
    "src/docs/reference/modules/Users/CustomUserSettings/README.md": ["OrchardCore.Users.CustomUserSettings"],
    "src/docs/reference/modules/Users/TicketStore.md": ["OrchardCore.Users"],
    "src/docs/reference/modules/Media/MediaGallery.md": ["OrchardCore.Media"],
    "src/docs/reference/modules/Flow/BagPart.md": ["OrchardCore.Flows"],
    "src/docs/reference/modules/Localize/javascript-localization.md": ["OrchardCore.Localization"],
    "src/docs/reference/modules/Security/Permissions.md": ["OrchardCore.Roles"],
}

# Libraries of src/OrchardCore with documentation of their own, and no feature: developer topics.
LIBRARIES = {
    "OrchardCore.Configuration.KeyVault": ("Azure Key Vault configuration", "Configuration",
                                           ["src/docs/reference/modules/KeyVault.Azure/README.md"],
                                           "Reads the site configuration from Azure Key Vault."),
    "OrchardCore.Logging.Serilog": ("Serilog logging", "Infrastructure",
                                    ["src/docs/reference/modules/Logging.Serilog/README.md"],
                                    "Writes the logs of a site with Serilog."),
    "OrchardCore.Logging.NLog": ("NLog logging", "Infrastructure", ["src/docs/getting-started/templates/README.md"],
                                 "Writes the logs of a site with NLog, the default of the CMS web project."),
    "OrchardCore.OpenTelemetry": ("OpenTelemetry", "Infrastructure",
                                  ["src/docs/reference/modules/OpenTelemetry/README.md"],
                                  "Collects traces, metrics and logs with OpenTelemetry."),
    "OrchardCore.Shells.Azure": ("Shells in Azure Blob Storage", "Infrastructure", ["src/docs/reference/modules/Shells/README.md"],
                                 "Keeps the tenants' settings and configuration in Azure Blob Storage."),
    "OrchardCore.SourceGenerators": ("Source generators", "Developer",
                                     ["src/docs/topics/source-generators/README.md"],
                                     "Generates code at build time for modules."),
    "OrchardCore.Localization.Extraction": ("Localization extraction", "Localization", ["src/docs/reference/modules/Localize/README.md"],
                                            "Extracts the localizable strings of modules into PO templates at build time."),
}

# The pages, among those of LIBRARIES, that document nothing but their library.
LIBRARY_PAGES = ("KeyVault.Azure/", "Logging.Serilog/", "OpenTelemetry/", "source-generators/")

# Reference pages that document a concept rather than one feature, keyed by the id they get in the catalog.
TOPIC_NAMES = {
    "Configuration": ("Configuration", "Infrastructure"),
    "ContentParts": ("Content parts", "Content Management"),
    "Data": ("Data and YesSql", "Infrastructure"),
    "DisplayManagement": ("Display management", "Display"),
    "Migrations": ("Data migrations", "Infrastructure"),
    "Modules": ("Modules", "Developer"),
    "Placement": ("Placement", "Display"),
    "Razor": ("Razor helpers", "Display"),
    "Sanitizer": ("HTML sanitizer", "Security"),
    "Shells": ("Shells and tenants", "Infrastructure"),
    "core/file-upload-security": ("File upload security", "Security"),
    "core/temporary-file-storage": ("Temporary file storage", "Infrastructure"),
    "themes/admin-theme": ("Admin theme conventions", "Theming"),
    "glossary": ("Glossary", "Getting Started"),
    "libraries": ("Libraries", "Developer"),
    "branding": ("Branding", "Getting Started"),
    "README": ("Modules overview", "Getting Started"),
}

# The framework libraries every module builds on.
FRAMEWORK = {"OrchardCore", "OrchardCore.Abstractions", "OrchardCore.Infrastructure", "OrchardCore.Infrastructure.Abstractions",
             "OrchardCore.Mvc.Core", "OrchardCore.Data", "OrchardCore.Data.Abstractions", "OrchardCore.Data.YesSql",
             "OrchardCore.Data.YesSql.Abstractions", "OrchardCore.DisplayManagement", "OrchardCore.DisplayManagement.Abstractions"}

# Registrations, in a Startup class, that give a feature a user-visible surface.
UI_PATTERNS = [
    r"AddNavigationProvider<", r"INavigationProvider,", r"AddDisplayDriver<", r"AddSiteDisplayDriver<",
    r"IDisplayDriver<", r"AddContentPart<", r"AddContentField<", r"AddActivity<", r"AddDeployment<",
    r"IContentPartDisplayDriver", r"IContentFieldDisplayDriver", r"IContentDisplayDriver", r"AddShapeAttributes<",
    r"IShapeTableProvider", r"AddShapeTableProvider<", r"AddContentPartDisplayDriver", r"UseDisplayDriver<",
    r"IContentTypePartDefinitionDisplayDriver", r"IContentPartFieldDefinitionDisplayDriver",
    r"IContentTypeDefinitionDisplayDriver", r"AddTheme", r"AddLiquidFilter<", r"AddShortcode",
]

# Audience and UI corrections the heuristics can't make. A feature with a UI defaults to "both".
AUDIENCE = {
    "OrchardCore.Title": "admin", "OrchardCore.Html": "admin", "OrchardCore.Alias": "admin",
    "OrchardCore.PublishLater": "admin", "OrchardCore.ArchiveLater": "admin", "OrchardCore.HomeRoute": "admin",
    "OrchardCore.Widgets": "admin", "OrchardCore.Taxonomies": "both", "OrchardCore.Features": "admin",
    "OrchardCore.AdminDashboard": "admin", "OrchardCore.Contents.QuickNavigation": "admin",
    "OrchardCore.Users.ChangeEmail": "admin", "OrchardCore.Users.Registration": "admin",
    "OrchardCore.Users.ResetPassword": "admin", "OrchardCore.Users.TwoFactorAuthentication": "admin",
    "OrchardCore.Users.Authentication.AuthenticatorApp": "admin",
    "OrchardCore.Users.Authentication.EmailAuthenticator": "admin",
    "OrchardCore.Users.Authentication.SmsAuthenticator": "admin",
    "OrchardCore.ContentPreview": "admin", "OrchardCore.AuditTrail": "admin", "OrchardCore.Notifications": "both",
    "TheAdmin": "admin", "TheTheme": "admin", "TheBlogTheme": "admin", "TheAgencyTheme": "admin",
    "TheComingSoonTheme": "admin", "SafeMode": "admin",
}
HAS_UI = {
    "OrchardCore.Contents.QuickNavigation": True, "OrchardCore.Contents.VersionPruning": True,
    "OrchardCore.Admin": True, "OrchardCore.Setup": True, "OrchardCore.Users.ChangeEmail": True,
    "OrchardCore.Users.Registration": True, "OrchardCore.Users.ResetPassword": True,
    "OrchardCore.Media.Slugify": True, "OrchardCore.Media.Tus": True, "OrchardCore.Media.SignalR": True,
    "OrchardCore.Media.Security": True, "OrchardCore.Resources": True, "OrchardCore.Https": True,
    "OrchardCore.Search.Lucene.ContentPicker": True, "OrchardCore.Search.Elasticsearch.ContentPicker": True,
    "OrchardCore.Contents.FileContentDefinition": False, "OrchardCore.Liquid.Core": False,
}


# ---------------------------------------------------------------------------------------------------------------------
# C# scanning helpers

STRING = re.compile(r'@?"(?:[^"\\]|\\.)*"')
COMMENT_LINE = re.compile(r"//[^\n]*")
COMMENT_BLOCK = re.compile(r"/\*.*?\*/", re.S)


TOKENS = re.compile(r'"""[\s\S]*?"""|@"(?:[^"]|"")*"|\$?"(?:[^"\\\n]|\\.)*"|\'(?:[^\'\\\n]|\\.)\'|//[^\n]*|/\*.*?\*/', re.S)


def strip_comments(text: str) -> str:
    """Removes comments, keeping strings (a "//" inside a string such as a URL stays)."""
    return TOKENS.sub(lambda m: "" if m.group(0).startswith("/") else m.group(0), text)


def read(path: Path) -> str:
    """Reads a file, even past the 260-character path limit of Windows."""
    name = str(path)
    if sys.platform == "win32" and len(name) > 240 and not name.startswith("\\\\?\\"):
        name = "\\\\?\\" + str(path.absolute()).replace("/", "\\")
    with open(name, encoding="utf-8-sig", errors="replace") as handle:
        return handle.read()


def matching(text: str, start: int, open_char: str, close_char: str) -> int:
    """Index of the bracket closing the one at text[start], skipping strings and chars."""
    depth, i, n = 0, start, len(text)
    while i < n:
        c = text[i]
        if c == '"' or (c == "@" and i + 1 < n and text[i + 1] == '"'):
            m = STRING.match(text, i)
            if m:
                i = m.end()
                continue
        if c == "'" and i + 2 < n:
            j = text.find("'", i + 1 if text[i + 1] != "\\" else i + 3)
            if 0 < j - i <= 4:
                i = j + 1
                continue
        if c == open_char:
            depth += 1
        elif c == close_char:
            depth -= 1
            if depth == 0:
                return i
        i += 1
    return -1


def split_top(text: str, sep: str = ",") -> list[str]:
    parts, depth, cur, i = [], 0, [], 0
    while i < len(text):
        c = text[i]
        if c == '"':
            m = STRING.match(text, i)
            cur.append(m.group(0))
            i = m.end()
            continue
        if c in "([{":
            depth += 1
        elif c in ")]}":
            depth -= 1
        if c == sep and depth == 0:
            parts.append("".join(cur).strip())
            cur = []
        else:
            cur.append(c)
        i += 1
    if "".join(cur).strip():
        parts.append("".join(cur).strip())
    return parts


def unquote(value: str) -> str:
    value = value.strip()
    if value.startswith('@"'):
        return value[2:-1].replace('""', '"')
    if value.startswith('"'):
        return bytes(value[1:-1], "utf-8").decode("unicode_escape")
    return value


class Constants:
    """Every `const string X = "..."` of the source, by its dotted class path."""

    DECL = re.compile(r"\b(?:class|struct|interface|record)\s+(\w+)")
    CONST = re.compile(r"\bconst\s+string\s+(\w+)\s*=\s*(\"(?:[^\"\\]|\\.)*\")\s*;")

    def __init__(self, files):
        self.values: dict[str, str] = {}
        for path in files:
            text = strip_comments(read(path))
            stack: list[tuple[str, int]] = []
            depth = 0
            pending = None
            i = 0
            for token in re.finditer(r"\{|\}|\b(?:class|struct|interface|record)\s+\w+|\bconst\s+string\s+\w+\s*=\s*\"(?:[^\"\\]|\\.)*\"\s*;", text):
                s = token.group(0)
                if s == "{":
                    depth += 1
                    if pending:
                        stack.append((pending, depth))
                        pending = None
                elif s == "}":
                    if stack and stack[-1][1] == depth:
                        stack.pop()
                    depth -= 1
                elif s.startswith("const"):
                    m = self.CONST.match(s)
                    names = [name for name, _ in stack]
                    for k in range(len(names) + 1):
                        key = ".".join(names[k:] + [m.group(1)])
                        self.values.setdefault(key, unquote(m.group(2)))
                else:
                    pending = s.split()[-1]

    def resolve(self, expression: str, where: str) -> str:
        expression = expression.strip()
        if expression.startswith('"'):
            return unquote(expression)
        if expression.startswith("nameof("):
            return expression[7:-1].split(".")[-1]
        if expression in self.values:
            return self.values[expression]
        parts = expression.split(".")
        for k in range(1, len(parts)):
            key = ".".join(parts[k:])
            if key in self.values:
                return self.values[key]
        raise SystemExit(f"Can't resolve the constant {expression} in {where}")


def parse_attribute_args(body: str, constants: Constants, where: str) -> dict:
    args = {}
    for part in split_top(body):
        if "=" not in part:
            continue
        key, value = part.split("=", 1)
        key, value = key.strip(), value.strip()
        if value.startswith("[") or value.startswith("new"):
            inner = value[value.index("[") + 1:value.rindex("]")] if value.startswith("[") else value[value.index("{") + 1:value.rindex("}")]
            args[key] = [constants.resolve(v, where) for v in split_top(inner) if v.strip()]
        elif value in ("true", "false"):
            args[key] = value == "true"
        elif value.startswith('"') or value.startswith('@"') or re.match(r"^[A-Z]\w*(\.\w+)+$", value):
            if key in ("Id", "Name", "Description", "Category", "BaseTheme") or value.startswith('"'):
                try:
                    args[key] = constants.resolve(value, where)
                except SystemExit:
                    if key == "Id":
                        raise
                    args[key] = value
        else:
            args[key] = value
    return args


def manifest_attributes(text: str):
    for m in re.finditer(r"\[assembly:\s*(Module|Feature|Theme)\s*\(", text):
        start = m.end() - 1
        end = matching(text, start, "(", ")")
        yield m.group(1), text[start + 1:end]


# ---------------------------------------------------------------------------------------------------------------------
# Admin menus

ADD_LABEL = re.compile(r'\.Add(?:Async)?\(\s*(?:S\["((?:[^"\\]|\\.)*)"\]|new\s+LocalizedString\()')
HELPER = re.compile(r"\b(?:void|NavigationBuilder|ValueTask|Task)\s+(\w+)\s*\(\s*NavigationBuilder\s+\w+\s*\)\s*\{")


def menu_paths(text: str) -> list[str]:
    """The paths of the entries an admin menu provider adds, as labeled on screen ("Settings > Media > Options").

    It follows the nesting of the Add calls, and the helper methods a provider passes a builder to. An entry that is
    built from data (one per content type, for example) shows as "(dynamic)". Only entries that link somewhere (an
    Action or a Url) are kept: the others only group their children.
    """
    m = re.search(r"\bBuild(?:Async)?\s*\([^)]*NavigationBuilder\s+\w+[^)]*\)", text)
    if not m:
        return []
    helpers = {}
    for hm in HELPER.finditer(text):
        end = matching(text, hm.end() - 1, "{", "}")
        helpers[hm.group(1)] = text[hm.end() - 1:end + 1]
    start = text.find("{", m.end())
    end = matching(text, start, "{", "}")
    paths: list[str] = []
    walk(text[start:end], [], helpers, paths, set())
    seen, unique = set(), []
    for p in paths:
        if p not in seen:
            seen.add(p)
            unique.append(p)
    return unique


def own_text(call: str) -> str:
    """An Add call's arguments without the nested Add calls of its children."""
    out, i = [], 1
    while i < len(call):
        am = ADD_LABEL.match(call, i)
        if am:
            open_paren = call.index("(", i)
            i = matching(call, open_paren, "(", ")") + 1
            continue
        out.append(call[i])
        i += 1
    return "".join(out)


def walk(body: str, prefix: list[str], helpers: dict, paths: list[str], visiting: set) -> None:
    stack, depth, i = [], 0, 0  # stack entries: [label, depth, has_child, call text]
    while i < len(body):
        c = body[i]
        if c == '"' or (c == "@" and body[i:i + 2] == '@"'):
            sm = STRING.match(body, i)
            if sm:
                i = sm.end()
                continue
        am = ADD_LABEL.match(body, i)
        if am:
            if stack:
                stack[-1][2] = True
            open_paren = body.index("(", i)
            close = matching(body, open_paren, "(", ")")
            call = body[open_paren:close + 1]
            label = am.group(1) if am.group(1) is not None else "(dynamic)"
            depth += 1
            stack.append([label, depth, False, call])
            # A helper method that receives the builder adds the children.
            for name, helper in helpers.items():
                if name not in visiting and re.search(r"[,(]\s*" + re.escape(name) + r"\s*[,)]", own_text(call)):
                    stack[-1][2] = True
                    walk(helper, prefix + [s[0] for s in stack], helpers, paths, visiting | {name})
            i = am.end()
            continue
        if c == "(":
            depth += 1
        elif c == ")":
            depth -= 1
            while stack and stack[-1][1] > depth:
                label, _, has_child, call = stack.pop()
                links = re.search(r"\.(?:Action|Url)\(", own_text(call))
                if links:
                    paths.append(" > ".join(prefix + [s[0] for s in stack] + [label]))
        i += 1


# ---------------------------------------------------------------------------------------------------------------------
# The scan

class Source:
    def __init__(self, root: Path):
        self.root = root
        self.src = root / "src"
        self.modules_dir = self.src / "OrchardCore.Modules"
        self.themes_dir = self.src / "OrchardCore.Themes"
        self.core_dir = self.src / "OrchardCore"
        self.docs_dir = self.src / "docs"
        scanned = [self.modules_dir, self.themes_dir, self.core_dir]
        self.cs_files = [p for d in scanned for p in d.rglob("*.cs") if "/obj/" not in p.as_posix() and "/bin/" not in p.as_posix()]
        self.texts = {p: strip_comments(read(p)) for p in self.cs_files}
        self.constants = Constants(self.cs_files)
        self.class_files: dict[str, list[Path]] = {}
        for p, t in self.texts.items():
            for m in re.finditer(r"\b(?:class|record)\s+(\w+)", t):
                self.class_files.setdefault(m.group(1), []).append(p)
        self.permissions = self._permissions()

    def rel(self, path: Path) -> str:
        return path.relative_to(self.root).as_posix()

    def namespace(self, path: Path) -> str:
        m = re.search(r"^\s*namespace\s+([\w.]+)", self.texts.get(path, ""), re.M)
        return m.group(1) if m else ""

    def class_text(self, name: str, near: Path | None = None, startup: Path | None = None) -> tuple[Path, str] | None:
        """The declaration of a class, preferring the one a Startup file can see (its namespace or usings)."""
        parts = name.split(".")
        name, hint = parts[-1], ".".join(parts[:-1])
        files = self.class_files.get(name, [])
        if hint:
            files = [f for f in files if self.namespace(f).endswith(hint)] or files
        if near:
            local = [f for f in files if near in f.parents]
            files = local or files
        if startup is not None and len(files) > 1:
            text = self.texts.get(startup, "")
            visible = {self.namespace(startup)} | set(re.findall(r"^\s*using\s+([\w.]+)\s*;", text, re.M))
            files = [f for f in files if self.namespace(f) in visible] or files
        for f in files:
            t = self.texts[f]
            m = re.search(r"\b(?:class|record)\s+" + re.escape(name) + r"\b[^{;]*\{", t)
            if m:
                end = matching(t, m.end() - 1, "{", "}")
                return f, t[m.start():end + 1]
        return None

    def _permissions(self) -> dict[str, str]:
        """Field name (and Class.Field) to permission name, for every `new Permission("Name", ...)`."""
        result = {}
        decl = re.compile(r"(?:static\s+readonly\s+Permission|Permission)\s+(\w+)\s*=\s*new(?:\s+Permission)?\s*\(\s*(\"[^\"]+\"|nameof\([\w.]+\)|[\w.]+)")
        for p, t in self.texts.items():
            for cm in re.finditer(r"\bclass\s+(\w+)", t):
                pass
            for m in decl.finditer(t):
                try:
                    name = self.constants.resolve(m.group(2), self.rel(p))
                except SystemExit:
                    continue
                result[m.group(1)] = name
                owner = None
                for cm in re.finditer(r"\bclass\s+(\w+)", t[:m.start()]):
                    owner = cm.group(1)
                if owner:
                    result[f"{owner}.{m.group(1)}"] = name
        return result


def scan_features(source: Source) -> tuple[list[dict], dict[str, Path]]:
    features, module_dirs = [], {}
    for base in (source.modules_dir, source.themes_dir):
        for directory in sorted(d for d in base.iterdir() if d.is_dir()):
            manifest = directory / "Manifest.cs"
            if not manifest.exists():
                continue
            project = directory.name
            module_dirs[project] = directory
            text = strip_comments(read(manifest))
            where = source.rel(manifest)
            declared, module_args, kind = [], None, None
            for attr, body in manifest_attributes(text):
                args = parse_attribute_args(body, source.constants, where)
                if attr in ("Module", "Theme"):
                    module_args, kind = args, attr
                else:
                    declared.append(args)
            module_name = (module_args or {}).get("Name", project)
            if not declared:
                # As in FeaturesProvider: a module that declares no feature is a feature itself, with the project's id.
                main = dict(module_args or {})
                main["Id"] = project
                main.setdefault("Name", module_name)
                declared.append(main)
            for args in declared:
                fid = args["Id"]
                features.append({
                    "id": fid,
                    "name": args.get("Name") or module_name,
                    "kind": "theme" if kind == "Theme" and fid == project else "feature",
                    "module": project,
                    "moduleName": module_name,
                    "category": args.get("Category") or (module_args or {}).get("Category") or ("Theming" if kind == "Theme" else "Uncategorized"),
                    "summary": (args.get("Description") or (module_args or {}).get("Description") or "").strip(),
                    "dependencies": list(args.get("Dependencies", [])) + ([module_args["BaseTheme"]] if kind == "Theme" and fid == project and module_args.get("BaseTheme") else []),
                    "flags": [f for f in ("EnabledByDependencyOnly", "DefaultTenantOnly", "IsAlwaysEnabled") if args.get(f) is True]
                             + (["Obsolete"] if "(Obsolete)" in str(args.get("Name", "")) else [])
                             + (["AdminTheme"] if kind == "Theme" and "Admin" in str((module_args or {}).get("Tags", "")) else []),
                    "_dir": directory,
                })
    return features, module_dirs


STARTUP = re.compile(r"\bclass\s+(\w+)\s*(?:\([^)]*\))?\s*:\s*(?:[\w.]*\.)?StartupBase\b")


def scan_startups(source: Source, module_dirs: dict[str, Path], features: list[dict]) -> dict[str, list[dict]]:
    """Each feature id to the Startup classes that register its services."""
    by_feature: dict[str, list[dict]] = {}
    for project, directory in module_dirs.items():
        for path in sorted(directory.rglob("*.cs")):
            if path not in source.texts:
                continue
            text = source.texts[path]
            for m in STARTUP.finditer(text):
                # The attribute lines right above the class declaration.
                collected = []
                for line in reversed(text[:m.start()].split("\n")[:-1]):
                    stripped = line.strip()
                    if stripped.startswith("["):
                        collected.append(stripped)
                        continue
                    break
                attrs_text = " ".join(reversed(collected))
                feature = project
                fm = re.search(r"\[(?:[^\]]*,\s*)?Feature\(([^)]*)\)", attrs_text)
                if fm:
                    feature = source.constants.resolve(fm.group(1).split(",")[0], source.rel(path))
                requires = []
                for rm in re.finditer(r"RequireFeatures\(([^)]*)\)", attrs_text):
                    requires += [source.constants.resolve(v, source.rel(path)) for v in split_top(rm.group(1))]
                brace = text.find("{", m.end())
                end = matching(text, brace, "{", "}")
                by_feature.setdefault(feature, []).append({
                    "file": path, "class": m.group(1), "body": text[brace:end + 1], "requires": requires,
                })
    return by_feature


def first_generic(body: str, method: str) -> list[str]:
    return re.findall(re.escape(method) + r"<\s*([\w.]+)", body)


def describe_surfaces(source: Source, feature: dict, startups: list[dict]) -> dict:
    directory = feature["_dir"]
    s = {k: [] for k in ("adminMenu", "permissions", "settings", "contentParts", "contentFields", "contentTypes",
                         "workflowActivities", "deploymentSteps", "recipeSteps", "liquidFilters", "liquidTags",
                         "shortcodes", "graphql", "api", "backgroundTasks", "conditional")}
    ui = False
    for st in startups:
        body = st["body"]
        if any(re.search(p, body) for p in UI_PATTERNS):
            ui = True
        if st["requires"]:
            s["conditional"].append({"class": st["class"], "requires": st["requires"]})
        for name in first_generic(body, "AddNavigationProvider") + re.findall(r"INavigationProvider,\s*(\w+)>", body):
            found = source.class_text(name, directory, st["file"])
            if found:
                s["adminMenu"] += [p for p in menu_paths(found[1]) if p not in s["adminMenu"]]
        for name in first_generic(body, "AddPermissionProvider") + re.findall(r"IPermissionProvider,\s*(\w+)>", body):
            found = source.class_text(name, directory, st["file"])
            if not found:
                continue
            ptext = found[1]
            names = []
            for tok in re.finditer(r"\b(\w+)\.(\w+)\b|\b(\w+)\b", ptext):
                key = f"{tok.group(1)}.{tok.group(2)}" if tok.group(1) else tok.group(3)
                if key in source.permissions and (tok.group(1) or re.match(r"[A-Z]", key)):
                    names.append(source.permissions[key])
            names += re.findall(r"new\s+Permission\(\s*\"(\w+)\"", ptext)
            if "GetDynamicPermission" in ptext or "CreateDynamicPermission" in ptext or "ContentTypePermissionsHelper" in ptext:
                names.append("(one set per content type)")
            for n in names:
                if n not in s["permissions"]:
                    s["permissions"].append(n)
        for name in first_generic(body, "AddSiteDisplayDriver"):
            s["settings"].append(re.sub(r"(DisplayDriver|Driver)$", "", name))
        s["contentParts"] += first_generic(body, "AddContentPart")
        s["contentFields"] += first_generic(body, "AddContentField")
        for name in first_generic(body, "AddActivity"):
            found = source.class_text(name, directory, st["file"])
            if not found:
                continue
            label = name
            lm = (re.search(r"DisplayText\s*=>\s*S\[\"([^\"]+)\"\]", found[1])
                  or re.search(r"DisplayName\s*\{\s*get;\s*\}\s*=\s*LocalizationSource\.Create(?:<\w+>)?\(\s*\"([^\"]+)\"", found[1]))
            if lm:
                label = lm.group(1)
            s["workflowActivities"].append(label)
        for gm in re.finditer(r"AddDeployment<\s*([\w.]+)\s*,\s*([\w.]+)", body):
            s["deploymentSteps"].append(re.sub(r"DeploymentStep$", "", gm.group(2)))
        for name in first_generic(body, "AddDeploymentWithoutSource"):
            s["deploymentSteps"].append(re.sub(r"DeploymentStep$", "", name))
        for name in first_generic(body, "AddRecipeExecutionStep"):
            found = source.class_text(name, directory, st["file"])
            label = name
            if found:
                lm = re.search(r":\s*base\(\s*(\"[^\"]+\"|[\w.]+)\s*\)", found[1])
                if lm:
                    try:
                        label = source.constants.resolve(lm.group(1), name)
                    except SystemExit:
                        label = lm.group(1)
            s["recipeSteps"].append(label)
        s["liquidFilters"] += re.findall(r"AddLiquidFilter<[\w.]+>\(\s*\"([^\"]+)\"", body)
        s["shortcodes"] += re.findall(r"AddShortcode(?:<[\w.]+>)?\(\s*\"([^\"]+)\"", body)
        for name in re.findall(r"IBackgroundTask,\s*(\w+)>", body):
            s["backgroundTasks"].append(name)
        for gm in re.finditer(r"Add(?:ObjectGraphType|InputObjectGraphType|ContentPartGraphQL|ContentFieldGraphQL|"
                              r"GraphQLFilterType|GraphQL\w*)\s*<\s*([\w.]+)|ISchemaBuilder,\s*(\w+)>|"
                              r"IContentTypeBuilder,\s*(\w+)>|IContentFieldProvider,\s*(\w+)>", body):
            name = next(g for g in gm.groups() if g)
            if name not in s["graphql"]:
                s["graphql"].append(name)
        for name in first_generic(body, "AddDataMigration"):
            found = source.class_text(name, directory, st["file"])
            if not found:
                continue
            for tm in re.finditer(r"AlterTypeDefinitionAsync\(\s*(\"\w+\"|[\w.]+)", found[1]):
                try:
                    type_name = source.constants.resolve(tm.group(1), name)
                except SystemExit:
                    continue
                window = found[1][tm.end():tm.end() + 1500]
                stereo = re.search(r"\.Stereotype\(\s*\"(\w+)\"", window.split("AlterTypeDefinitionAsync")[0])
                label = f"{type_name} ({stereo.group(1)})" if stereo else type_name
                if label not in s["contentTypes"]:
                    s["contentTypes"].append(label)
        for tm in re.finditer(r"Register\w*(?:Tag|Block)\w*\(\s*\"([a-z_]+)\"", body):
            s["liquidTags"].append(tm.group(1))
    return {k: sorted(set(v)) if k not in ("adminMenu", "permissions", "conditional") else v for k, v in s.items()}, ui


def scan_api(source: Source, directory: Path) -> list[tuple[str, str | None]]:
    """HTTP API routes of a module ("api/..."), with the feature of the controller when it says."""
    routes = []
    for path in directory.rglob("*.cs"):
        if path not in source.texts:
            continue
        t = source.texts[path]
        if "api/" not in t:
            continue
        feature = None
        fm = re.search(r"\[Feature\(([^)]*)\)\]", t)
        if fm:
            try:
                feature = source.constants.resolve(fm.group(1), source.rel(path))
            except SystemExit:
                feature = None
        for rm in re.finditer(r"(?:Route|Http\w+|Map(?:Get|Post|Put|Delete|Patch|Methods)?)\(\s*\"(~?/?api/[^\"]*)\"", t):
            routes.append((rm.group(1).lstrip("~/"), feature))
    return routes


# ---------------------------------------------------------------------------------------------------------------------
# Documentation

def doc_pages(source: Source) -> list[Path]:
    reference = source.docs_dir / "reference"
    pages = [p for p in sorted(reference.rglob("*.md")) if "/assets/" not in p.as_posix()]
    return pages


def guide_pages(source: Source) -> list[Path]:
    pages = []
    for folder in ("getting-started", "guides", "topics"):
        pages += sorted((source.docs_dir / folder).rglob("*.md"))
    return [p for p in pages if "/assets/" not in p.as_posix() and "node_modules" not in p.as_posix()]


def h1(path: Path) -> str:
    for line in read(path).splitlines():
        if line.startswith("# "):
            return line[2:].strip()
    return path.stem


def map_docs(source: Source, features: list[dict], module_dirs: dict[str, Path]):
    by_id = {f["id"]: f for f in features}
    unmapped = []
    for page in doc_pages(source):
        rel = source.rel(page)
        rel_ref = page.relative_to(source.docs_dir / "reference").as_posix()
        targets = list(DOC_PAGE_FEATURES.get(rel, []))
        if not targets:
            title_ids = [i for i in re.findall(r"`(OrchardCore\.[\w.]+)`", h1(page)) if i in by_id]
            targets = title_ids
        if not targets and rel_ref.startswith("modules/"):
            folder = rel_ref.split("/")[1]
            if folder in DOC_FOLDER_FEATURES:
                targets = DOC_FOLDER_FEATURES[folder]
            elif f"OrchardCore.{folder}" in by_id:
                targets = [f"OrchardCore.{folder}"]
            elif f"OrchardCore.{folder}" in module_dirs:
                # A module with no feature of its own id: the page documents its features.
                targets = [f["id"] for f in features if f["module"] == f"OrchardCore.{folder}" and "Obsolete" not in f["flags"]]
        if not targets:
            unmapped.append(page)
            continue
        for t in targets:
            if t not in by_id:
                raise SystemExit(f"{rel} maps to {t}, which isn't a feature")
            if rel not in by_id[t]["docs"]:
                by_id[t]["docs"].append(rel)
        # Every feature of the same module that the page names gets it too.
        text = read(page)
        modules = {by_id[t]["module"] for t in targets}
        for f in features:
            if f["module"] in modules and f["id"] not in targets and re.search(r"(?<![\w.])" + re.escape(f["id"]) + r"(?![\w])", text):
                if rel not in f["docs"]:
                    f["docs"].append(rel)
    # Features still without a page: the reference pages that name them, then their module's main page.
    reference_texts = {source.rel(p): read(p) for p in doc_pages(source)}
    for f in features:
        if f["docs"]:
            continue
        hits = [rel for rel, text in reference_texts.items() if re.search(r"(?<![\w.])" + re.escape(f["id"]) + r"(?![\w])", text)]
        if hits:
            f["docs"] = hits[:3]
            continue
        main = by_id.get(f["module"]) or next((g for g in features if g["module"] == f["module"] and g["docs"]), None)
        if main and main["docs"]:
            f["docs"] = [main["docs"][0]]
        elif f["kind"] == "theme":
            f["docs"] = ["src/docs/reference/modules/Themes/README.md"]
    return unmapped


# ---------------------------------------------------------------------------------------------------------------------

def project_references(source: Source) -> dict[str, set[str]]:
    """For each library of src/OrchardCore, the modules and themes whose project references it."""
    users: dict[str, set[str]] = {}
    for base in (source.modules_dir, source.themes_dir):
        for csproj in base.rglob("*.csproj"):
            text = read(csproj)
            for ref in re.findall(r"ProjectReference\s+Include=\"[^\"]*?([\w.]+)\.csproj\"", text):
                users.setdefault(ref, set()).add(csproj.stem)
    # Libraries referenced by other libraries count their users' users.
    for _ in range(3):
        for csproj in source.core_dir.rglob("*.csproj"):
            text = read(csproj)
            for ref in re.findall(r"ProjectReference\s+Include=\"[^\"]*?([\w.]+)\.csproj\"", text):
                users.setdefault(ref, set()).update(users.get(csproj.stem, set()) or {csproj.stem})
    return users


def build(source_root: Path, commit: str, date: str) -> None:
    source = Source(source_root)
    features, module_dirs = scan_features(source)
    startups = scan_startups(source, module_dirs, features)
    known = {f["id"] for f in features}
    for fid in list(startups):
        if fid in known:
            continue
        if fid in module_dirs:
            # As in ExtensionManager: a type with no Feature attribute, in a module that has no feature of its own id,
            # belongs to every feature of the module.
            for f in features:
                if f["module"] == fid:
                    startups.setdefault(f["id"], []).extend(startups[fid])
            del startups[fid]
            continue
        raise SystemExit(f"A Startup class targets {fid}, which no manifest declares")

    api_by_module = {project: scan_api(source, d) for project, d in module_dirs.items()}

    for f in features:
        f["docs"] = []
        surfaces, ui = describe_surfaces(source, f, startups.get(f["id"], []))
        for route, owner in api_by_module.get(f["module"], []):
            if (owner or f["module"]) == f["id"] and route not in surfaces["api"]:
                surfaces["api"].append(route)
        surfaces["api"].sort()
        f["surfaces"] = {k: v for k, v in surfaces.items() if v}
        f["_ui"] = ui or f["kind"] == "theme"

    # The Liquid tags and filters the framework libraries register are there whenever Liquid is.
    liquid = next(f for f in features if f["id"] == "OrchardCore.Liquid")
    for path, text in source.texts.items():
        if source.core_dir not in path.parents:
            continue
        tags = re.findall(r"Register\w*(?:Tag|Block)\w*\(\s*\"([a-z_]+)\"", text)
        filters = re.findall(r"AddLiquidFilter<[\w.]+>\(\s*\"([^\"]+)\"", text) + re.findall(r"Filters\.AddFilter\(\s*\"([^\"]+)\"", text)
        for key, values in (("liquidTags", tags), ("liquidFilters", filters)):
            if values:
                liquid["surfaces"][key] = sorted(set(liquid["surfaces"].get(key, [])) | set(values))

    unmapped = map_docs(source, features, module_dirs)

    # Libraries with documentation.
    entries = []
    for lib, (name, category, docs, summary) in LIBRARIES.items():
        if not (source.core_dir / lib).exists():
            raise SystemExit(f"Library {lib} doesn't exist")
        for d in docs:
            if not (source_root / d).exists():
                raise SystemExit(f"{d} doesn't exist")
        entries.append({"id": lib, "name": name, "kind": "library", "module": lib, "category": category,
                        "audience": "developer", "docs": docs, "admin": None, "permissions": [], "dependencies": [],
                        "hasUi": False, "summary": summary, "flags": [], "surfaces": {}})
    # Pages that document only a library; the other pages of LIBRARIES also stay topics or guides of their own.
    library_docs = {docs[0] for (_, _, docs, _) in LIBRARIES.values()
                    if docs and any(k in docs[0] for k in LIBRARY_PAGES)}

    # Reference pages that document no feature: topics.
    topics = []
    for page in unmapped:
        rel = source.rel(page)
        if rel in library_docs:
            continue
        rel_ref = page.relative_to(source.docs_dir / "reference").with_suffix("").as_posix()
        key = rel_ref
        if key.startswith("modules/"):
            key = key.split("/")[1]
        elif key.endswith("/README"):
            key = key[:-7]
        name, category = TOPIC_NAMES.get(key, (h1(page), "Developer"))
        existing = next((t for t in topics if t["id"] == f"topic:{key}"), None)
        if existing:
            existing["docs"].append(rel)
            continue
        topics.append({"id": f"topic:{key}", "name": name, "kind": "topic", "module": None, "category": category,
                       "audience": "developer", "docs": [rel], "admin": None, "permissions": [], "dependencies": [],
                       "hasUi": False, "summary": f"Reference page: {h1(page)}.", "flags": [], "surfaces": {}})

    # Guides, getting started and key topics.
    guides = []
    for page in guide_pages(source):
        rel = source.rel(page)
        if rel in library_docs:
            continue
        rel_docs = page.relative_to(source.docs_dir).with_suffix("").as_posix()
        key = rel_docs[:-7] if rel_docs.endswith("/README") else rel_docs
        guides.append({"id": f"guide:{key}", "name": h1(page), "kind": "guide", "module": None,
                       "category": {"getting-started": "Getting Started", "guides": "Guides", "topics": "Key Topics"}[key.split("/")[0]],
                       "audience": "developer", "docs": [rel], "admin": None, "permissions": [], "dependencies": [],
                       "hasUi": False, "summary": f"Documentation page: {h1(page)}.", "flags": [], "surfaces": {}})

    out = []
    for f in features:
        if f["module"] in DEMO_MODULES:
            continue
        surfaces = f["surfaces"]
        has_ui = HAS_UI.get(f["id"], f["_ui"])
        audience = AUDIENCE.get(f["id"], "both" if has_ui else "developer")
        out.append({
            "id": f["id"], "name": f["name"], "kind": f["kind"], "module": f["module"], "category": f["category"],
            "audience": audience, "docs": f["docs"],
            "admin": surfaces.get("adminMenu", [None])[0],
            "permissions": [p for p in surfaces.get("permissions", []) if not p.startswith("(")],
            "dependencies": f["dependencies"], "hasUi": has_ui, "summary": f["summary"], "flags": f["flags"],
            "surfaces": surfaces,
        })
    out += entries + topics + guides
    out.sort(key=lambda e: ({"feature": 0, "theme": 1, "library": 2, "topic": 3, "guide": 4}[e["kind"]], e["id"].lower()))

    ids = [e["id"] for e in out]
    dupes = {i for i in ids if ids.count(i) > 1}
    if dupes:
        raise SystemExit(f"Duplicate ids: {sorted(dupes)}")

    CATALOG.mkdir(exist_ok=True)
    data = {"source": {"repository": "OrchardCMS/OrchardCore", "commit": commit, "scannedOn": date}, "features": out}
    (CATALOG / "features.json").write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8", newline="\n")

    write_excluded(source, features, out)
    write_source(commit, date, out)
    counts = {}
    for e in out:
        counts[e["kind"]] = counts.get(e["kind"], 0) + 1
    print(f"{len(out)} entries: " + ", ".join(f"{v} {k}" for k, v in sorted(counts.items())))


PLURALS = {"feature": "features", "theme": "themes", "library": "libraries", "topic": "topics", "guide": "guides"}


def write_source(commit: str, date: str, entries: list[dict]) -> None:
    counts: dict[str, int] = {}
    for e in entries:
        counts[e["kind"]] = counts.get(e["kind"], 0) + 1
    lines = [
        "# Source of the catalog",
        "",
        "<!-- Generated by tools/catalog.py. -->",
        "",
        "| | |",
        "| --- | --- |",
        "| Repository | [OrchardCMS/OrchardCore](https://github.com/OrchardCMS/OrchardCore) |",
        "| Branch | `main` |",
        f"| Commit | [`{commit[:10] or 'unknown'}`](https://github.com/OrchardCMS/OrchardCore/commit/{commit}) |" if commit else "| Commit | unknown |",
        f"| Scanned on | {date} |",
        "| Scanned | `src/OrchardCore.Modules`, `src/OrchardCore.Themes`, `src/OrchardCore`, `src/docs` |",
        "| Entries | " + ", ".join(f"{counts[k]} {PLURALS[k] if counts[k] != 1 else k}" for k in ("feature", "theme", "library", "topic", "guide") if k in counts) + " |",
        "",
        "To refresh the catalog, check out `main` of OrchardCMS/OrchardCore (or extract it with",
        "`git archive <commit> src mkdocs.yml | tar -x -C <folder>`, which leaves a working copy untouched), then run:",
        "",
        "```bash",
        "python tools/catalog.py --source <OrchardCore checkout> --commit <sha> --date <YYYY-MM-DD>",
        "```",
        "",
        "Then compare `features.json` with the tracker: `python tools/tracker.py validate` lists every feature that no",
        "video covers and that `tracker/NO-VIDEO.md` doesn't explain.",
    ]
    (CATALOG / "SOURCE.md").write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")


def write_excluded(source: Source, features: list[dict], entries: list[dict]) -> None:
    users = project_references(source)
    covered_libraries = {e["id"] for e in entries if e["kind"] == "library"}
    feature_modules = {f["module"] for f in features}
    lines = [
        "# Excluded from the catalog",
        "",
        "<!-- Generated by tools/catalog.py; edit the reasons there. -->",
        "",
        "Every module, theme and library folder of the Orchard Core source that has **no entry** in `features.json`,",
        "with the reason. Everything a site can enable is in the catalog; what is listed here is either a building block",
        "of a module (whose features are in the catalog), a build tool, a template, or a sample.",
        "",
        "## Sample modules",
        "",
        "| Folder | Reason |",
        "| --- | --- |",
    ]
    for name, reason in DEMO_MODULES.items():
        lines.append(f"| `src/OrchardCore.Modules/{name}` | {reason} |")
    templates = sorted({p.parent.relative_to(source.root).as_posix() for p in (source.src / "Templates").rglob("Manifest.cs")})
    lines += ["", "## Project templates", "", "| Folder | Reason |", "| --- | --- |"]
    for t in templates:
        lines.append(f"| `{t}` | A `dotnet new` template with placeholder ids, not a feature; covered by the code generation templates video (`guide:getting-started/templates`). |")
    lines += ["", "## Libraries of `src/OrchardCore`", "",
              "Libraries have no feature of their own: they hold the abstractions and services of the modules that use",
              "them, and are covered by those modules' entries.", "", "| Library | Reason |", "| --- | --- |"]
    for d in sorted(x for x in source.core_dir.iterdir() if x.is_dir()):
        name = d.name
        if name in covered_libraries:
            continue
        used_by = sorted(m for m in users.get(name, set()) if m in feature_modules)
        if name.endswith("Targets") or name in ("OrchardCore.Module.Targets", "OrchardCore.Theme.Targets"):
            reason = "MSBuild package that references the libraries and modules of an application, module or theme; nothing to show beyond the getting-started videos."
        elif name.startswith("OrchardCore.Localization.") and name != "OrchardCore.Localization.Core":
            reason = "Build-time tooling for localizable strings (analyzers, code fixes, MSBuild tasks); covered by the localization developer videos."
        elif name in FRAMEWORK:
            reason = "The modular application framework every module builds on; covered by the topics on modules, shells, configuration and data (`topic:Modules`, `topic:Shells`, `topic:Configuration`, `topic:Data`)."
        elif used_by:
            shown = ", ".join(f"`{m}`" for m in used_by[:6]) + (f" and {len(used_by) - 6} more" if len(used_by) > 6 else "")
            reason = f"Building block of {shown}; covered by their entries."
        else:
            reason = "Library with no feature, screen or documentation page of its own."
        lines.append(f"| `src/OrchardCore/{name}` | {reason} |")
    (CATALOG / "EXCLUDED.md").write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--source", required=True, type=Path, help="An Orchard Core checkout (the folder that holds src/)")
    parser.add_argument("--commit", default="", help="The commit scanned, recorded in features.json")
    parser.add_argument("--date", default=datetime.date.today().isoformat())
    args = parser.parse_args()
    if not (args.source / "src" / "OrchardCore.Modules").is_dir():
        raise SystemExit(f"{args.source} isn't an Orchard Core checkout")
    build(args.source.resolve(), args.commit, args.date)
    return 0


if __name__ == "__main__":
    sys.exit(main())
