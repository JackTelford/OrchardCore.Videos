# Open questions: XML-RPC and Remote Publishing

The narration was written from the source of Orchard Core alone; nothing was run. The video claims only what the code
and the documentation show. These are the points a person must check on a running site (the Blog recipe, with `XML-RPC`
and `Remote Publishing` enabled) before the video is recorded. Paths are under `src/OrchardCore.Modules/` unless they
start with `src/`.

## 1. What the stand-in and the first take must confirm

- The stand-in script of `recording-plan.md` returns an array with one blog for `blogger.getUsersBlogs`, and a string
  (the id of the item) for each `metaWeblog.newPost`. The call on the slide `calls-2` is written by hand from
  `OrchardCore.XmlRpc/Services/XmlRpcReader.cs` and the parameter order in
  `OrchardCore.Lists/RemotePublishing/MetaWeblogHandler.cs:98-109`; the stand-in sends the same shape, so a successful
  response confirms the slide. If the response is a fault or a status 500, correct the slide and the narration of
  `calls-2` before recording.
- The documentation says the MetaWeblog calls go to `/xmlrpc/metaweblog` (see `docIssues`); the narration follows the
  code: the calls go to `/xmlrpc` (`OrchardCore.XmlRpc/Controllers/HomeController.cs:27-29`), and `/xmlrpc/metaweblog`
  answers the manifest (`MetaWeblogController.cs:25-53`), whatever the method. Confirm it by sending the stand-in's
  first call to `/xmlrpc/metaweblog` and checking that the answer is the manifest, not a `methodResponse`; and with a
  client configured by hand.
- `/blog` holds `<link rel="wlwmanifest">` and `<link rel="EditURI">` in its head
  (`OrchardCore.Lists/Views/ListPart.RemotePublishing.cshtml:8-9`, rendered through the shape of
  `ListMetaWeblogDriver.cs:14-17`). The plan checks it before `03-discover`; if the links are missing, the sentence
  "The blog's page declares two links in its head" of `discover-1` must go.
- The narration says "Two links in the head of a blog's page point a client to its details", and the chapter and the
  introduction say "how a client finds your blog". That is how RSD and manifest discovery are meant to work, but it was
  not checked with Open Live Writer or another MetaWeblog client. Add the blog to a client by its address, and confirm
  that the client finds the account on its own. Also confirm which address a client needs when it is configured by
  hand (the RSD document gives `/xmlrpc`).

## 2. Behavior of the Title driver

`OrchardCore.Title/RemotePublishing/TitleMetaWeblogDriver.cs` writes the posted `title` to `ContentItem.DisplayText`
(line 31) and reads `TitlePart.Title` back (line 26, through `HtmlEncoder.Encode`). `OrchardCore.Title/Handlers/TitlePartHandler.cs:79-87` copies
`Title` to `DisplayText` and never the other way round, so a post created by a client may have a `DisplayText` and an
empty `TitlePart.Title`. The video claims only that the item appears in Content Items with its title (the list shows
the display text). Check on the site, after the stand-in has run:

- Open the post in the admin editor: is the Title field filled?
- Call `metaWeblog.getRecentPosts` and `metaWeblog.getPost` for the blog: is a `title` returned, or a fault?
- Is the path of the post generated from its title (the Blog Post pattern uses `display_text`:
  `blog.recipe.json`, type `BlogPost`)?

- Likely consequence, to confirm: `TitleMetaWeblogDriver.BuildPost` calls `HtmlEncoder.Encode(titlePart.Title)`
  (line 40). `TitlePart.Title` is null for a post created by a client (only `DisplayText` is set, line 45), and
  `TextEncoder.Encode(string)` throws for null (.NET behavior, not read here). If so, `metaWeblog.getPost` for such a post
  returns a fault, and `metaWeblog.getRecentPosts` faults for the whole blog once a client has created a post. The
  stand-in therefore never calls those two methods. The narration of `calls-3` says only that a posted title becomes
  the display text.

If any of these fails, report it to the Orchard Core team as a bug; it does not change this video, which does not open
the editor.

## 3. Which features the calls need

The narration says "The calls need the Lists and Media features as well". Lists: `MetaWeblogHandler.cs:26` and
`src/OrchardCore/OrchardCore/Shell/Builders/CompositionStrategy.cs:56-61`: a type marked `[RequireFeatures]` is composed
only when the features it names are enabled together with the feature of its module. Media: the constructor of
`MetaWeblogHandler` (lines 40-50) takes `IMediaFileStore` and `FileCreationService`, which only the Media feature
registers (`OrchardCore.Media/Startup.cs:96,119`, in the `Startup` class of the `OrchardCore.Media` feature), and the
Lists manifest does not depend on Media. So the handler cannot be created without Media, which should make `/xmlrpc` and
`/xmlrpc/metaweblog` fail. This is read from the dependency injection setup, not run: with Lists and Remote Publishing
enabled and Media disabled, confirm that `/xmlrpc` fails. If it still answers, the narration of `enable-2` must drop
Media.

## 4. Permissions

Verified from the source, not run. The permission names in the narration and on the slide are the descriptions that the
role editor shows (`CommonPermissions.cs:17,21,25`; `OrchardCore.Roles/Views/Admin/_RolePermissions.cshtml:14`): "Edit
content for others", "Publish or unpublish content for others", "Delete content for others". `newPost` and
`getRecentPosts` check `EditContent` or `PublishContent` with a null resource (`MetaWeblogHandler.cs:262,302`), and
`ContentTypeAuthorizationHandler.cs:29-32` returns for a null resource before it tries the owner variants. The permission
handler grants a permission only through the permissions that imply it (`DefaultPermissionGrantingService.cs`), and
`Edit own content` does not imply `Edit content for others` (`CommonPermissions.cs:21-23`). A user that holds only the
"own" permissions, such as the Author role, is therefore refused by those two calls. Confirm on a site with a user of
the Author role if the video ever shows it; this video does not. `CheckAccessAsync` (lines 524-530) reports "Not
authorized to delete this content" for every denied access check.

## 5. Spoken forms

New spellings in the `spoken` map of `storyboard.json` that a person must confirm with `scripts/pronounce.py` before
`narrate.py` is run: `X.M.L. R.P.C.` (for XML-RPC and /xmlrpc), `R.S.D.`, `H.T.M.L.` (for Html), `Meta Weblog`,
`supports File Upload`, `supports Slug`, `Build Post`, `Edit Post`, `Process Async`, `I Meta Weblog Driver`,
`Meta Weblog Driver`, `I X.M.L. R.P.C. Handler`, `Blog Post`, and the spoken form of the two routes
(`slash X.M.L. R.P.C. slash Meta Weblog`). The captions keep the written forms.

## 6. Small things to see on the screen

- The notification after `Enable` names only Remote Publishing (`OrchardCore.Features/Controllers/AdminController.cs:317`
  receives the requested feature, not its dependencies). Check that XML-RPC shows `Enabled` in the list after the
  reload, as `recording-plan.md` expects.
- How long the tenant takes to restart after enabling the features, and whether the click of `Enable` and the reload
  fit in the narration of `enable-2` (about 13 seconds).
- That the browser shows the manifest and the RSD document as an XML tree, and that `GetByText` finds the element and
  attribute names for the highlights of `discover-2` and `discover-3`. If not, remove the highlights; the narration
  does not depend on them.
- That the rows of Content Items are in the order the plan expects (newest modification first), and that the badge
  of the draft is `Draft` with no `Published` badge.
