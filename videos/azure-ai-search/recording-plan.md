# Recording plan: Azure AI Search

How to record this video without deciding anything. The narration is in `storyboard.json`; this file says what is on
screen while each step is spoken. Labels are quoted as the code spells them, with the file and line of the
OrchardCore source they were read from (commit `92db4a83`, `src/...`).

The video needs a search service, and Azure AI Search has no local emulator. The recording therefore uses a
**stand-in**: a small program that answers the few requests the module makes (section 2). The stand-in is not part of
this repository (the validator allows only the files of the video format in this folder), so its code is in this file.

## 1. What is recorded

| Clip | Chapter | Starts on | Changes the site | Needs |
| --- | --- | --- | --- | --- |
| `02-enable` | 02 Enable the module | `/Admin/Features` | Enables the feature Azure AI Search | the fresh site |
| `03-connect` | 03 Connect to the service | `/Admin/Features` | Saves the Azure AI Search settings | `02-enable` |
| `04-create` | 04 Create an index | `/Admin/Settings/azureAISearch` | Creates the index `blog-posts` (site and stand-in) | `03-connect`, the stand-in running |
| `06-edit` | 06 Edit an index | `/Admin/indexing` | Saves the index again, unchanged | `04-create` |
| `07-actions` | 07 Index actions | `/Admin/indexing` | Nothing (the menu is opened, no item is clicked) | `04-create` |

The slide clips (`01-how`, `03-auth`, `04-name`, `05-holds`) are drawn by `slides.py` and need no recording.

Record in this order: `02-enable`, `03-connect`, `04-create`, `06-edit`, `07-actions`. Every clip starts from the
state the previous one left. Narrate first (`scripts/narrate.py`), because the recorder holds each step for the length
of its narration; add the entries of `"spoken"` to the check with `scripts/pronounce.py` (see `OPEN-QUESTIONS.md`).

## 2. The stand-in for the search service

### What the site asks of the service

The module uses the `Azure.Search.Documents` client, which sends these requests to the endpoint, each with an
`api-key` header when the Authentication Type is API Key. The endpoint must be **https** (next paragraph):

| When | Request | The stand-in answers |
| --- | --- | --- |
| Save on the create page (`AdminController.cs:243-257`) | `GET /indexes('<name>')` | `404` while the index does not exist, `200` after |
| the same | `POST /indexes` with the index definition | `201` with the same definition |
| right after, in the background (synchronizing) | `POST /indexes('<name>')/docs/search.index` | `200` with one status per document |
| Rebuild, Delete, Force Delete (not clicked in this video) | `DELETE /indexes('<name>')` | `204` |

`<name>` is `default-blog-posts`: the tenant name in lowercase, a hyphen, and the Index Name
(`AzureAISearchIndexNameProvider.cs:25`). The stand-in keeps the indexes in memory, so **restart it together with
every restore of the site's snapshot taken before `04-create`** (section 5).

**Why https.** The constructors of `SearchIndexClient` and `SearchClient` in `Azure.Search.Documents` call `AssertHttpsScheme`,
which throws an `ArgumentException` when the scheme of the endpoint is not `https` (read in the IL of versions 11.7.0 and 12.0.0,
including the short constructors that `AzureAIClientFactory.cs:80-82` uses). `AzureAISearchIndexManager.cs` builds the client
inside a `try`, so with an `http` address the create page ends on "Unable to create the index for the provider
'AzureAISearch'." The stand-in therefore serves HTTPS, and the Endpoint typed on camera is `https://localhost:9201`.

The paths and the `api-version` query value come from the SDK, not from this plan. The stand-in prints every
request it receives: read the log during the dry run (section 4) and, if a path differs, change the matching pattern
in the code below.

### The code

Save it outside both repositories as `standin.py` (Python 3.11 or later, standard library only) and start it with
`python standin.py 9201 <folder>/standin.pem <folder>/standin.key` (the certificate and key of the next section). Neither
the request handling nor the TLS wrapper has been run against the module: the dry run of section 4 is the test.

```python
"""A stand-in for an Azure AI Search service, for recording only. Standard library only.

    python standin.py 9201 standin.pem standin.key

It serves HTTPS (the Azure SDK refuses any other scheme), keeps the indexes and documents it receives in memory,
prints one line per request, and refuses a request without an api-key header, as the service does. It is not part
of any repository.
"""
import json
import re
import ssl
import sys
import urllib.parse
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 9201
INDEXES = {}  # index name -> the definition the site sent
DOCUMENTS = {}  # index name -> {key: document}

NOT_FOUND = {"error": {"code": "", "message": "No index with the given name was found in the service."}}


def key_field(definition):
    for field in definition.get("fields", []):
        if field.get("key"):
            return field["name"]
    return None


class Handler(BaseHTTPRequestHandler):
    protocol_version = "HTTP/1.1"

    def log_message(self, *args):
        pass

    def reply(self, status, body=None):
        data = b"" if body is None else json.dumps(body).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; odata.metadata=minimal")
        self.send_header("Content-Length", str(len(data)))
        self.end_headers()
        if data:
            self.wfile.write(data)

    def handle_request(self):
        length = int(self.headers.get("Content-Length") or 0)
        raw = self.rfile.read(length) if length else b""
        url = urllib.parse.urlsplit(self.path)
        path = urllib.parse.unquote(url.path)
        has_key = bool(self.headers.get("api-key"))
        print(f"{self.command} {path}?{url.query} api-key={'yes' if has_key else 'no'} bytes={len(raw)}", flush=True)

        if not has_key:
            return self.reply(403, {"error": {"code": "", "message": "Forbidden: the api-key header is missing."}})

        body = json.loads(raw) if raw else None

        if path == "/indexes":
            if self.command == "POST":
                INDEXES[body["name"]] = body
                DOCUMENTS[body["name"]] = {}
                return self.reply(201, body)
            if self.command == "GET":
                return self.reply(200, {"value": list(INDEXES.values())})

        match = re.fullmatch(r"/indexes(?:\('([^']+)'\)|/([^/]+))", path)
        if match:
            name = match.group(1) or match.group(2)
            if self.command == "GET":
                return self.reply(200, INDEXES[name]) if name in INDEXES else self.reply(404, NOT_FOUND)
            if self.command == "PUT":
                INDEXES[name] = body
                DOCUMENTS.setdefault(name, {})
                return self.reply(200, body)
            if self.command == "DELETE":
                if name not in INDEXES:
                    return self.reply(404, NOT_FOUND)
                del INDEXES[name]
                DOCUMENTS.pop(name, None)
                return self.reply(204)

        match = re.fullmatch(r"/indexes(?:\('([^']+)'\)|/([^/]+))/docs(?:/(.+))?", path)
        if match:
            name = match.group(1) or match.group(2)
            action = match.group(3) or ""
            if name not in INDEXES:
                return self.reply(404, NOT_FOUND)
            key = key_field(INDEXES[name])
            if action in ("search.index", "index") and self.command == "POST":
                results = []
                for item in body.get("value", []):
                    kind = item.pop("@search.action", "mergeOrUpload")
                    identity = item.get(key)
                    if kind == "delete":
                        DOCUMENTS[name].pop(identity, None)
                    else:
                        DOCUMENTS[name][identity] = {**DOCUMENTS[name].get(identity, {}), **item}
                    results.append({"key": identity, "status": True, "errorMessage": None, "statusCode": 200})
                print(f"  documents in {name}: {len(DOCUMENTS[name])}", flush=True)
                return self.reply(200, {"value": results})
            if action in ("search.post.search", "search") and self.command == "POST":
                terms = [t.lower() for t in re.findall(r"\w+", (body or {}).get("search", "*")) if t != "*"]
                found = [d for d in DOCUMENTS[name].values()
                         if not terms or any(t in json.dumps(list(d.values())).lower() for t in terms)]
                skip, top = (body or {}).get("skip", 0), (body or {}).get("top")
                page = found[skip:(skip + top) if top else None]
                return self.reply(200, {"@odata.count": len(found), "value": [{"@search.score": 1.0, **d} for d in page]})
            if action == "$count" and self.command == "GET":
                return self.reply(200, len(DOCUMENTS[name]))

        return self.reply(404, {"error": {"code": "", "message": f"The stand-in does not answer {self.command} {path}."}})

    do_GET = do_POST = do_PUT = do_DELETE = handle_request


if __name__ == "__main__":
    if len(sys.argv) != 4:
        sys.exit("usage: python standin.py <port> <certificate.pem> <key.pem>")
    server = ThreadingHTTPServer(("127.0.0.1", PORT), Handler)
    context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
    context.load_cert_chain(sys.argv[2], sys.argv[3])
    server.socket = context.wrap_socket(server.socket, server_side=True)
    print(f"Stand-in for Azure AI Search on https://localhost:{PORT}", flush=True)
    server.serve_forever()
```

### How the site reaches it

1. **Certificate (once).** The site runs as your user, and .NET trusts what the Windows certificate store of that user
   trusts. Use the ASP.NET Core development certificate, which is valid for `localhost`:

   ```text
   dotnet dev-certs https --trust
   dotnet dev-certs https --export-path <folder outside both repositories>/standin.pem --format Pem --no-password
   ```

   The first command trusts the certificate for your user (a system dialog asks you to confirm; skip it when
   `dotnet dev-certs https --check --trust` already reports a trusted certificate). The second writes the certificate to
   `standin.pem` and, next to it, the private key as `standin.key`; if the tool names the key file differently, pass the name
   it printed. Never commit either file.
2. **Start the stand-in:** `python standin.py 9201 <folder>/standin.pem <folder>/standin.key`.
3. **Endpoint.** Type `https://localhost:9201` in `connect-1`. `localhost` is on the `NO_PROXY` list of section 3, so the
   request does not go through the proxy address that blocks the other outbound calls.
4. **If `localhost` cannot be used** (for example a policy forbids trusting the development certificate), a certificate for the
   host name `search.example.com` that your user trusts, the hosts line `127.0.0.1 search.example.com` (administrator rights;
   remove it after the recording), `search.example.com` added to `NO_PROXY`, and the Endpoint `https://search.example.com:9201`
   work the same way. This route has not been tried. Say in the pull request of the recording which route was used.

There is no route without HTTPS and a certificate that the site trusts. If the dry run (section 4) shows no request in the
log of the stand-in, read the log of the site (the exception names the endpoint or the TLS error) before changing anything
else; if no route works, the video stays blocked (see `OPEN-QUESTIONS.md`). An `HTTPS_PROXY` route does not work: the
stand-in does not implement `CONNECT`.

## 3. The site

- Run `src/OrchardCore.Cms.Web` of an OrchardCore checkout at `http://localhost:5010`, with `ORCHARD_APP_DATA` set to
  an empty folder outside both repositories, provisioned by Auto Setup (`Program.cs` of the project registers it):

  | Environment variable | Value |
  | --- | --- |
  | `OrchardCore__AutoSetup__Tenants__0__ShellName` | `Default` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteName` | `Orchard Core` |
  | `OrchardCore__AutoSetup__Tenants__0__SiteTimeZone` | `America/Los_Angeles` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminUsername` | `admin` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminEmail` | `admin@example.com` |
  | `OrchardCore__AutoSetup__Tenants__0__AdminPassword` | `<password>` (chosen outside git, the same as `VIDEO_PASSWORD`) |
  | `OrchardCore__AutoSetup__Tenants__0__DatabaseProvider` | `Sqlite` |
  | `OrchardCore__AutoSetup__Tenants__0__RecipeName` | `Blog` |

  Also set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, `http://127.0.0.1:9`, with `NO_PROXY=localhost,127.0.0.1`, so
  that every outbound call is blocked as `references/recording.md` asks, except the stand-in on `localhost`.
- The Blog recipe already enables Indexing, Contents, Content Types and Deployment, and creates one Blog Post
  (`Man must explore, and this is exploration at its greatest`), so the index has content to send. **Do not enable
  anything else before recording**: Azure AI Search is enabled on camera in `02-enable`; Secrets, Search, Lucene and
  Elasticsearch stay disabled. With Secrets disabled the Api Key field is a plain masked input
  (`SecretInputTagHelper.cs:96`); with Secrets enabled it is a different control and `connect-2` and `connect-3` do not match.
- No host configuration for Azure AI Search: the connection page must show Endpoint and Authentication Type
  directly, without the checkbox "Use custom Azure Search AI settings" (`AzureAISearchDefaultSettings.Edit.cshtml:9-29`).
- Seed data: none. The recipe's content is the only content, and no real name or address is on the site.
- Sign in once outside the capture with the recorder (`Recorder setup`), which saves `state.json` (git-ignored). The
  admin user is `admin`, the password `<password>`.
- Recorder environment: `VIDEO_ROOT=<repo>/videos/azure-ai-search`, `VIDEO_BASE_URL=http://localhost:5010`,
  `VIDEO_USER_NAME=admin`, `VIDEO_PASSWORD=<password>`; `MSYS_NO_PATHCONV=1` in Git Bash.

## 4. Before the first take: the dry run

1. Start the stand-in (`python standin.py 9201 <folder>/standin.pem <folder>/standin.key`) and the site; wait for Auto Setup
   to finish.
2. `curl -i -H "api-key: example" "https://localhost:9201/indexes"` must answer `200`; without the header it must answer
   `403`. If curl refuses the certificate and its build keeps its own certificate list, use `curl -k` for this check only: the
   first request of the site in step 3 is the real test of the trust.
3. Click through clips `02-enable`, `03-connect` and `04-create` by hand with the values of this plan.
   - After `03-connect` the stand-in's log must still be **empty**: saving does not contact the service (the narration of
     `connect-3` says so). If a request appears, report it: the narration is wrong.
   - If the stand-in log stays empty after `04-create` and the page shows the error message, the site could not connect:
     the certificate is not trusted for the user that runs the site, or the endpoint is not https (section 2).
   - After `04-create` the log must show `GET /indexes('default-blog-posts')`, then `POST /indexes`, then (the
     synchronizing) another `GET` of the index, `POST /indexes('default-blog-posts')/docs/search.index` and
     `documents in default-blog-posts: 1` (the Blog Post).
     The page must show the green message of `create-6`, not the error "Unable to create the index for the provider
     'AzureAISearch'." (`AdminController.cs:250`).
4. Write down what the log shows for the paths and the `api-version`, and open the **Index Name** and **Name**
   fields once to see their input names (the selectors below assume `name` attributes ending in `IndexName`, `.Name`,
   `Culture`, `IndexLatest`, `AnalyzerName`, `QueryAnalyzerName`).
5. Stop the site and the stand-in, restore the empty `App_Data`, and start both again.

## 5. Snapshots and retakes

Copy `App_Data` (and `state.json`) aside at these points; the stand-in holds the remote half of the state:

| Snapshot | Taken | Restore it before a retake of | The stand-in |
| --- | --- | --- | --- |
| S0 | after Auto Setup and `Recorder setup` | `02-enable` | restart it (empty) |
| S1 | after `02-enable` | `03-connect` | restart it (empty) |
| S2 | after `03-connect` | `04-create` | restart it (empty) |
| S3 | after `04-create` | `06-edit`, `07-actions` | keep it running; if it was restarted, restart the site too, load any page, and wait until the stand-in log shows `POST /indexes` (the site creates the missing index when the tenant starts, `ContentIndexInitializerService.cs:20-64`) |

`06-edit` saves the index with its own values and `07-actions` changes nothing, so either can be taken again without
restoring S3.

## 6. The slides (no recording)

| Step | What the slide shows |
| --- | --- |
| `how-1` | "What the module does": three cards, Creates indexes, Sends content, Answers searches |
| `how-2` | "What you need from Azure": three cards, A search service, Its endpoint, A way to sign in |
| `auth-1` | "Three authentication types": three cards, Default, Managed Identity, API Key |
| `name-1` | "How an index is named in the service": three cards, default, blog-posts, default-blog-posts |
| `holds-1` | "What goes into an index": the cards Content item id and Full-text field bright, Included elements dimmed |
| `holds-2` | The same slide with only the card Included elements bright |

## 7. The clips

In every step, the actions are shorter than the narration of the step; a locator written as plain CSS stands for
`scene.Page.Locator("...")`. Clear the highlights (`scene.ClearHighlightsAsync()`) before a click that loads a page
and before scrolling.

### Clip `02-enable`

**Starts on:** `/Admin/Features`, opened with `scene.OpenAsync` before `StartCaptureAsync`. The page lists every feature
in groups; its search box is `#search-box` (`Features.cshtml:27`, placeholder "Search").

- `enable-1`
  - Actions: `scene.TypeAsync(#search-box, "Azure AI Search", 60)`. Wait until the link
    `a[href*="/Features/OrchardCore.AzureAI/Enable"]` is visible. `scene.HighlightAsync` the row whose text contains
    `Azure AI Search (Obsolete)` (`.list-group-item` with that text) with the caption "Obsolete", hold 1500 ms, clear it, then
    `scene.MoveToAsync` the Enable link above (the cursor rests on it; it is clicked in `enable-2`).
  - Visible at the end: the search box with "Azure AI Search", the filtered list with the row "Azure AI Search" and its
    Enable button, and the row "Azure AI Search (Obsolete)", the cursor on the first Enable button.
  - Labels: Features, under Tools (`Features/AdminMenu.cs:27`); the names "Azure AI Search" and "Azure AI Search (Obsolete)"
    (`OrchardCore.AzureAI/Manifest.cs:12,23`); Enable (`FeatureEntry.Buttons.SummaryAdmin.cshtml:40`).
- `enable-2`
  - Actions: `scene.ClickAndWaitForNavigationAsync(a[href*="/Features/OrchardCore.AzureAI/Enable"])`. Wait until the text
    "was enabled" is on the page (the site restarts the tenant; this can take several seconds, so wrap the wait in
    `scene.OffCameraAsync`). Then click the menu entry **Settings** (`figcaption#settings button.item-label`), and the entry
    **Search** inside it (`ul[data-title="Settings"] button.item-label:text-is("Search")`; the top-level **Search** entry has
    the same text, so do not use a plain text locator), and `scene.HighlightAsync(li#azureaisearch a)`.
  - Visible at the end: the green message "The feature Azure AI Search was enabled." (the message names only the feature
    that was clicked, never the features enabled with it: `OrchardCore.Features.Core/Services/FeatureService.cs`,
    `EnableOrDisableFeaturesAsync`), and the admin menu open on Settings, Search, with Azure AI Search ringed.
  - Labels: the message `OrchardCore.Features/Controllers/AdminController.cs:317`; Settings, Search, Azure AI Search
    `OrchardCore.AzureAI/AdminMenu.cs:39-41` (the item id `azureaisearch` is set at line 43).

### Clip `03-connect`

**Starts on:** `/Admin/Features`. Before `StartCaptureAsync`, click Settings (`figcaption#settings button.item-label`) and the
Search entry inside it (`ul[data-title="Settings"] button.item-label:text-is("Search")`), so that the link `li#azureaisearch a`
is visible in the menu (the menu keeps what was left open).

- `connect-1`
  - Actions: `scene.ClickAndWaitForNavigationAsync(li#azureaisearch a)` (the page is `/Admin/Settings/azureAISearch`).
    Wait for `input[type="url"]`. `scene.TypeAsync(input[type="url"], "https://localhost:9201", 45)`.
  - Visible at the end: the settings page with Endpoint filled in with `https://localhost:9201`, and
    Authentication Type showing "Default".
  - Labels: Endpoint (`AzureAISearchDefaultSettings.Edit.cshtml:33`); the path `Settings/{groupId}`
    (`OrchardCore.Settings/Controllers/AdminController.cs:50`, group id `azureAISearch` in
    `AzureAISearchDefaultSettingsDisplayDriver.cs:22`).
- `connect-2`
  - Actions: `scene.HighlightAsync(select.azure-ai-auth-type)`, hold 1200 ms, clear it, then
    `scene.SelectAsync(select.azure-ai-auth-type, "API Key")`. Wait until `.azure-ai-api-key-wrapper` is no longer hidden.
  - Visible at the end: Authentication Type showing "API Key", and the field Api Key below it, empty, with the placeholder
    "Provide an authentication key".
  - Labels: Authentication Type (`AzureAISearchDefaultSettings.Edit.cshtml:41`); the options Default, Managed Identity, API Key
    (`AzureAISearchDefaultSettingsDisplayDriver.cs:67-69`); Api Key (`AzureAISearchDefaultSettings.Edit.cshtml:49`, note the
    lowercase "pi"); the placeholder (line 51).
- `connect-3`
  - Actions: `scene.TypeAsync(input[type="password"][name$="ApiKey.Value"], "standin-key-0001", 60)` (a fake value, shown as
    dots). `scene.ClickAndWaitForNavigationAsync(button.save)`. Wait for the text "Site settings updated successfully."
    and for `input[type="password"][placeholder^="The value is securely stored"]`. `scene.HighlightAsync` that field, hold 2000 ms.
  - Visible at the end: the message "Site settings updated successfully.", and the Api Key field ringed, empty, with the
    placeholder "The value is securely stored. Enter a new value to update it, or leave it blank to keep it."; Endpoint
    still filled in and Authentication Type "API Key".
  - Labels: Save (`OrchardCore.Settings/Views/SiteSettings.SaveButton.cshtml:2`); the message
    (`OrchardCore.Settings/Controllers/AdminController.cs:119`); the placeholder (`SecretInputTagHelper.cs:103`).

### Clip `04-create`

**Starts on:** `/Admin/Settings/azureAISearch`. Before `StartCaptureAsync`, click the top-level menu entry **Search**
(`figcaption#search button.item-label`; the id `search` is set by `OrchardCore.Indexing/AdminMenu.cs`, not by the Search
entry inside Settings) so that `li#indexes a` ("Indexes") is visible.

- `create-1`
  - Actions: `scene.ClickAndWaitForNavigationAsync(li#indexes a)` (the page is `/Admin/indexing`). `scene.ClickAsync(button.create)`
    (the button "Add index"). Wait for `#modalAddIndex.show`.
  - Visible at the end: the dialog "Available Sources" over the list page (behind it: "Nothing here! There are no indexes at the
    moment."), showing the heading "Azure AI Search" and one card "Content in Azure AI Search".
  - Labels: Search, Indexes (`OrchardCore.Indexing/AdminMenu.cs:19-22`); Add index (`Views/Admin/Index.cshtml:31`); the empty
    message (line 40); Available Sources (line 61).
- `create-2`
  - Actions: `scene.HighlightAsync(#modalAddIndex .modal-body section h5)` (the provider heading "Azure AI Search"; a plain
    `#modalAddIndex h5` also matches the dialog title), hold 1000 ms, clear it;
    `scene.HighlightAsync(#modalAddIndex .card)` (the card), hold 1500 ms, clear it;
    `scene.ClickAndWaitForNavigationAsync(#modalAddIndex .card a:text-is("Add"))`.
  - Visible at the end: the page titled "New 'Content in Azure AI Search' index" with the fields Index Name and Name.
  - Labels: the provider `OrchardCore.AzureAI.Core/Extensions/ServiceCollectionExtensions.cs:23`; the card and its description
    `OrchardCore.AzureAI/Startup.cs:94-95`; Add (`IndexProfile.Link.cshtml:29`); the title (`Views/Admin/Create.cshtml:6`).
- `create-3`
  - Actions: `scene.TypeAsync(input[name$="IndexName"], "blog-posts", 70)`. Wait until the Name field holds `blog-posts`
    (the page script fills it in; the field is the input whose `name` ends in `.Name`). `scene.HighlightAsync` the hint under
    Name, hold 1200 ms.
  - Visible at the end: Index Name and Name both showing `blog-posts`, and the hint "A unique name for the index."
  - Labels: Index Name, Name and the hint (`OrchardCore.Indexing/Views/IndexProfileFields.Edit.cshtml:6,14,18`); the fill-in
    (`OrchardCore.Indexing/Assets/js/technical-name-generator.js`). A name with capital letters, spaces or underscores is
    rejected (`AzureAISearchIndexNamingHelper.cs:88`); do not type one.
- `create-4`
  - Actions: scroll (`scene.ScrollAsync(0, 300)`, adjust) so that Culture and the content types are visible.
    `scene.HighlightAsync(select[name$="Culture"])`, hold 1200 ms, clear it. `scene.ClickAsync(input[type="checkbox"][value="BlogPost"])`.
  - Visible at the end: Culture showing "Any culture", and the content type checkbox "Blog Post" checked (all the others
    unchecked).
  - Labels: Culture, Any culture and its hint (`ContentIndexMetadata.Edit.cshtml:6,9,11`); Content types and its hint
    (lines 17,19); the checkboxes are labelled with the display names of the content types (`Blog Post` is the content type
    `BlogPost` of the Blog recipe).
- `create-5`
  - Actions: scroll as needed. `scene.HighlightAsync` the checkbox "Index Latest version" (`input[type="checkbox"][name$="IndexLatest"]`; the tag
    helper also writes a hidden input with the same name), hold 1500 ms, clear it;
    `scene.HighlightAsync(select[name$="AnalyzerName"]:not([name$="QueryAnalyzerName"]))`, hold 1500 ms. Change nothing.
  - Visible at the end: Index Latest version unchecked, and the Analyzer list showing `standard.lucene`. Further down, the
    create page already shows "Default Query Settings" (Query analyzer and Query field names with ContentItemId and
    ContentItemVersionId, unchecked): do not scroll to it in this step and do not touch it (see `edit-2`).
  - Labels: Index Latest version and its hint (`ContentIndexMetadata.Edit.cshtml:31-32`); Analyzer
    (`AzureAISearchIndexProfile.Edit.cshtml:4`); the default `standard.lucene` (`AzureAISearchDefaultOptions.cs:9`). The order of
    Content types and Analyzer on the page is not known (both are at `Content:5`); scroll to whichever comes first.
- `create-6`
  - Actions: `scene.ClickAndWaitForNavigationAsync(button.btn-primary:text-is("Save"))`. Wait for the text
    "An index has been created successfully." If the message "Unable to create the index for the provider 'AzureAISearch'."
    appears instead, stop: the stand-in did not answer (section 4).
  - Visible at the end: the list page with the green message "An index has been created successfully. The synchronizing
    process was triggered in the background."
  - Labels: Save (`Views/Admin/Create.cshtml:16`); the message (`Controllers/AdminController.cs:257`).
- `create-7`
  - Actions: `scene.HighlightAsync` the row's title link `blog-posts`, hold 1200 ms, then the two badges
    (`.badge` with the text `AzureAISearch`, then `.badge` with the text `Content`), 1200 ms each.
  - Visible at the end: the row "blog-posts" with the badges AzureAISearch (tooltip Provider) and Content (tooltip Type), the
    buttons Edit and Delete, and the Actions button.
  - Labels: `IndexProfile.DefaultTags.SummaryAdmin.cshtml:7,11` (the values of the provider and the type);
    `IndexProfile.Buttons.SummaryAdmin.cshtml:8,13`; Actions (`OrchardCore.Admin/Views/AdminListActions.cshtml:32`).

### Clip `06-edit`

**Starts on:** `/Admin/indexing`, with the row `blog-posts`.

- `edit-1`
  - Actions: `scene.ClickAndWaitForNavigationAsync` the row's button `a.btn:text-is("Edit")` (`/Admin/indexing/edit/<id>`).
    `scene.HighlightAsync(input[name$="IndexName"])` (the field is disabled), hold 1500 ms.
  - Visible at the end: the page titled "Edit 'blog-posts' index", Index Name disabled and ringed, Name editable.
  - Labels: Edit (`IndexProfile.Buttons.SummaryAdmin.cshtml:8`); the title (`Views/Admin/Edit.cshtml:6`); the field disabled when the
    index exists (`IndexProfileFields.Edit.cshtml:8`).
- `edit-2`
  - Actions: clear the highlights, scroll down (`scene.ScrollAsync(0, 600)`, adjust) until the heading "Default Query Settings" is
    visible. `scene.HighlightAsync(select[name$="QueryAnalyzerName"])`, hold 1500 ms, clear it, then `scene.HighlightAsync` the list of
    check boxes under "Query field names" (`ul.list-group` after that label), hold 2000 ms.
  - Visible at the end: the heading "Default Query Settings", the list Query analyzer, and the list of fields under
    "Query field names" with their check boxes and the hint "The fields to use for query. Leave it blank to query all searchable fields."
  - Labels: `AzureAISearchQuerySettings.Edit.cshtml:10,15,23,46`. The heading and the list are on the create page too: the mapping holds
    ContentItemId and ContentItemVersionId from the start, and every Text entry is made searchable
    (`DefaultAzureAISearchFieldIndexEvents.cs:12-16`), so the view does not return at `:3-6`. On the edit page the list holds all
    the searchable fields of the Blog Post mapping, ordered by name (`AzureAISearchIndexProfileDisplayDriver.cs:55-66`), and
    `Content__ContentItem__FullText` is checked, because it is made the default when the index is saved
    (`AzureAISearchContentIndexProfileHandler.cs:42-45`). Do not read the field names aloud.
- `edit-3`
  - Actions: clear the highlights. `scene.ClickAndWaitForNavigationAsync(button.btn-primary:text-is("Save"))`. Wait for the text
    "An index has been updated successfully."
  - Visible at the end: the list page with the green message "An index has been updated successfully."
  - Labels: `Controllers/AdminController.cs:336`.

### Clip `07-actions`

**Starts on:** `/Admin/indexing`, with the row `blog-posts`. No item of the menu is clicked in this clip.

- `actions-1`
  - Actions: `scene.HighlightAsync` the row's `a.btn:text-is("Edit")` and `a.btn:text-is("Delete")` together (one ring each; the bulk actions
    of the toolbar also contain a "Delete" link, which is why the locator names the button), hold 1200 ms, clear them;
    `scene.ClickAsync` the row's `button.actions` (labelled "Actions").
  - Visible at the end: the menu open under the Actions button, with Synchronize, Reset, Rebuild and Force Delete.
  - Labels: `IndexProfile.Buttons.SummaryAdmin.cshtml:8,13`; Actions (`AdminListActions.cshtml:32`).
- `actions-2`
  - Actions: `scene.HighlightAsync(.actions-menu a.dropdown-item:text-is("Synchronize"))`, 2000 ms, clear; the same for "Reset", then for
    "Rebuild". The prefix `.actions-menu` matters: the hidden bulk-action menu of the toolbar has links with the same texts.
  - Visible at the end: the menu open with "Rebuild" ringed.
  - Labels: Synchronize, Reset, Rebuild (`IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:13,23,32`).
- `actions-3`
  - Actions: clear the highlights; `scene.HighlightAsync` the row's `a.btn:text-is("Delete")`, 1500 ms, clear it;
    `scene.HighlightAsync(.actions-menu a.dropdown-item:text-is("Force Delete"))` and keep it. Click nothing.
  - Visible at the end: the menu open with "Force Delete" ringed, and nothing deleted.
  - Labels: Delete and Force Delete (`IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:42`).

## 8. Checks for private data

Before assembling, look at every contact sheet, then OCR the video at one frame per second, at every scene change and at 5
frames per second, and search the text for anything that is not one of these:

- the host `localhost` (the Endpoint `https://localhost:9201`; `search.example.com` only if route 4 of section 2 was used), and the
  placeholder of Endpoint (`https://[search service name].search.windows.net`);
- the email `admin@example.com`, the user `admin`, the site name "Orchard Core";
- the content of the Blog recipe (the Blog Post and the pages it creates).

Nothing else may appear: no machine name or user name in a path, no real search service, subscription or key, no
password (the Api Key field shows dots, and its placeholder never shows the value), no other site. The stand-in's console, the certificate files and
the hosts file are never on screen. Stop the stand-in when the recording is finished, and remove the hosts entry if route 4 was used.
