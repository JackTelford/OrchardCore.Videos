# Open questions: Mini Profiler

What the author could not see on a running site, and what a person must confirm before or during the first take. Every
claim of the narration was read from the Orchard Core source (commit `92db4a837d`) or its documentation page
(`src/docs/reference/modules/MiniProfiler/README.md`); the items below are the places where the widget itself, which is
the upstream MiniProfiler script, decides what is on screen.

## 1. The version of the upstream package

`OrchardCore.MiniProfiler.csproj` references `MiniProfiler.AspNetCore.Mvc` without a version: the version lives in a
central package file that the extracted source does not contain. A recent preview NuGet package of the module depends
on version 4.5.4, and the labels below were read from that package's script (`MiniProfiler.Shared.dll`, the embedded
`includes` script and style). **Confirm:** the site that is recorded runs the same widget (open the popup and compare
the labels listed in question 2).

## 2. Labels written by the upstream script

The narration and the plan use these texts, which Orchard Core does not write (they are not in any `.cshtml`, `.cs` or
`.pot` file of the module, and cannot be checked in the source):

- the request button shows the duration of the request in milliseconds (`profile-1`);
- the popup's tree has a column **duration (ms)**, whose cells hold the duration of a step without its children
  (`profile-2`); the columns **with children (ms)** and **from start (ms)** appear under **more columns**;
- quick steps (under two milliseconds by default) are hidden, and a link **show trivial** lists them, then reads
  **hide trivial** (`profile-3`); the link exists only when the profile has a quick step;
- a column named after the custom timing type, **sql (ms)**, holds the SQL duration as a link (`profile-4`); the
  documentation calls it "the SQL duration of a timing" and the narration says "the SQL duration of a step".

**Confirm on the site, after enabling the feature:** each label above is on screen as written, **show trivial** is a
link of the popup, and at least one `tr.oc-mp-shape` row exists after choosing it. If a label differs, change the
narration of the step and the plan together.

## 3. SQL on the home page

`profile-4` needs at least one SQL link in the tree of the Blog home page. The home page is the blog item
(`blog.recipe.json:113-119`), which is read from the database, and the theme caches only the main menu
(`TheBlogTheme/Views/Layout.liquid:32`), so the queries should be there; nobody ran it. **Confirm** that a `.mp-queries-show`
link exists on `/` after the warm-up. If it does not, tell the author which page does (the content list of the admin,
`/Admin/Contents/ContentItems`, runs a query on every request) so that `profile-1` can name that page instead.

## 4. What the queries view shows on this page

The queries view prints, for each query, its number, **Duration**, **Started at**, **Step** and the statement; **First
result** appears only for a query that reports one (`Assets/ts/miniprofiler.ts:244-253`). The **Duplicate** and **Error**
badges and the gap rows (**... ms without queries**, **Mostly in ...**) depend on the page and are not guaranteed on
this one, so the video explains them on slides (`flags-1`, `flags-2`) and the clip does not promise them. Gap rows
shorter than a few milliseconds are hidden by the upstream script until "toggle trivial gaps" is chosen. **Confirm:**
whether this page shows a **Duplicate** badge. If it does, the author may add a step that points at it.

## 5. Pronunciation

`spoken` in `storyboard.json` splits the camel-case names into words. Nobody has heard these spellings: check them
with `scripts/pronounce.py` before narrating:

- "Add Orchard C.M.S." for `AddOrchardCms`;
- "Mini Profiler dot Current" for `MiniProfiler.Current`;
- "Yes S.Q.L." for `YesSql` and "M.V.C." for `MVC`: confirm the voice says them cleanly. `SQL` and `HTML` are left as
  written: confirm the voice says "sequel" (or "S.Q.L.") consistently and "H.T.M.L.". If not, add an entry to `spoken`.

## 6. Fonts of TheBlogTheme

`TheBlogTheme/Views/Layout.liquid:12-13` loads two Google Fonts stylesheets. On a machine without internet access, the
page can wait for them before the browser's load event, and the widget is created at that event. **Confirm** that
`/` shows the widget button within the 30 second limit of the plan; otherwise block those two requests in the
recorder's browser context.

## 7. Claims taken from the documentation only

These are in the narration because the documentation page states them; the source does not show them:

- results are held in the tenant's memory cache for 30 minutes and are not shared between instances (`missing-1`; the
  default storage and its duration are set by the upstream package, not by the module);
- the options set with `Configure<MiniProfilerOptions>` after `AddOrchardCms` take effect (`conf-1`; the module's
  `Startup.cs` only calls `AddMiniProfiler()` and binds nothing from configuration, which the source confirms);
- `ResultsAuthorize` protects the result endpoints, and nothing in the module sets it (`conf-2`; the second half is
  confirmed by `Startup.cs`).

**Confirm** on the recording site only if time allows: nothing in the video shows them.

## 8. The Administrator role

`roles-2` says the Administrator role has both permissions by default. `Permissions.cs` grants both to the
Administrator stereotype, and the documentation says the same. The Administrator role's edit page lists no permissions
(it shows "This system role provides comprehensive access by granting all available permissions."
`_RolePermissions.cshtml:32`), which is why the clip opens the **Editor** role. **Confirm** that the **Editor** edit page
lists the group **Mini Profiler** with both **Allow** boxes unchecked, and that the Blog recipe gives **Editor** neither.

## 9. For the maintainer

The documentation page already ends with a **Video** section holding an embedded player of another video
(`src/docs/reference/modules/MiniProfiler/README.md`), not a `<!-- video:mini-profiler -->` marker. `video.json` lists
the page in `embeddedIn`. Decide whether this video replaces that player or sits next to it before publishing.
