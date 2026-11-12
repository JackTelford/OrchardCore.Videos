# Questions about the specification

Questions about the skill's specification (`.agents/skills/orchardcore-video-producer/`) raised by the video
programme. The programme carries on with the answer given under each question until a maintainer decides otherwise;
the skill itself is not changed by the programme.

## 1. Narration length of the opening and closing cards

The programme limits every step to 8 to 45 words. The opening card of the worked example
(`examples/secrets-module/storyboard.json`, step `intro`) has 53 words, and two of its steps have 6.

**For now:** cards follow the same 8 to 45 words as every step, so an opening card says what the subject is and what
the video covers in two or three short sentences.

## 2. "now" in the worked example

The example says "SMTP now reads its password" (`input-6`) and "you can now choose its store" (`keyvault-3`). The
programme bans "now" in narration, as `references/storyboard.md` asks for timeless narration.

**For now:** "now" is banned in every narration; `tools/validate_content.py` fails on it.

## 3. Length of a video

`references/storyboard.md` says a module overview is "between 4 and 9 minutes". The programme asks for 3 to 8 minutes,
and splits anything longer into several videos.

**For now:** 3 to 8 minutes at 150 words per minute (450 to 1,200 words), checked by `validate_content.py`.

## 4. "JSON" in admin videos

Admin videos must not show code, and the programme's code markers include `json`. Some screens an administrator uses
are labeled with the word: **Tools > Deployments > JSON Import**, and the **View Or Download Content As JSON** feature.

**For now:** the label "JSON" (upper case) is allowed in admin narration and slides, as a name on screen;
`validate_content.py` fails on lower-case `json` (file names such as `recipe.json`, and code).

## 5. Files the programme adds to a video's folder

`SKILL.md` ("What a video folder keeps") doesn't list `recording-plan.md` or `OPEN-QUESTIONS.md`, which the
programme adds so a different session can record a video without deciding anything.

**For now:** both are kept in the folder. Should the skill list them, and should the template include a
`recording-plan.md` skeleton?

## 6. The README template's pull request link

`templates/video/README.md` starts with "**Feature:** `<id>` ([OrchardCore#NNNN](...))". Most videos of the programme
show features that exist on `main`, with no pull request of their own.

**For now:** the line names the feature id and links to its documentation page instead of a pull request, and
`video.json` has `"pullRequest": null`.

## 7. One playlist per series

`video.json` `playlists` holds the video's series, and the publish workflow creates a missing playlist. The plan has
40 series (`tracker/series.json`) in 22 learning paths.

**For now:** each video lists `"Orchard Core"` and its series. Should the channel have a playlist per series, per
learning path, or both?

## 8. The README template for admin videos

`templates/video/README.md` has the feature id in backticks and a "Recording it again" section with a shell block,
file names and environment variables. The programme applies the no-code rule of an admin video to the README, and
`tools/validate_content.py` does not read README files.

**For now:** an admin video's README has no backticks, no command lines and no file names ending in `.json`. The feature
id is plain text or link text, and "Recording it again" says in prose that the commands are in `recording-plan.md`.
Should the template have an admin variant, and should the validator scan admin READMEs for code markers?

## 9. "yet" in the README status line

The template and the task give the first line as "**YouTube:** not published yet". The programme bans "yet" in a
README, a description and a title, as in narration.

**For now:** the line is written "not published" in most videos; where a task prescribed the template line, it was kept
as given and no other banned word was used. Should the template say "not published", or should the status line be
exempt?

## 10. Rounding of the README "Length"

The README "Length" is the narration's words divided by 150, as m:ss. The specification does not say whether the
seconds are rounded or truncated (for example 579 words is 3:51.6), and `validate_content.py` passes any value. Video
authors also count words differently from the validator's tokenizer, so the value can differ by a few seconds.

**For now:** round to the nearest second, using the word count of `validate_content.py`. Should the validator compute
and compare the line, so that a stale value is caught after an edit?

## 11. Step length: 16 seconds or 45 words

`references/storyboard.md` says a step lasts 3 to 16 seconds. The programme and the validator allow 8 to 45 words,
which is 18 seconds at 150 words per minute; a 40-word step is 16 seconds. Opening cards and several steps of 41 to 45
words fall between the two limits.

**For now:** 45 words is the hard limit, and authors aim at 40 words or fewer. Should the validator's maximum be 40, or
should the reference say 18 seconds?

## 12. Length counted on written text, not spoken text

`validate_content.py` counts written tokens, so an identifier such as `IContentDefinitionManager` or a dotted name counts
as one word, while the voice says it as four to eight words through the `spoken` map. A developer video full of
identifiers runs 5 to 10 percent longer than words divided by 150, and a long step can pass the 45-word limit once
spoken.

**For now:** length, the README "Length" and `estimatedMinutes` use the written word count. Authors keep developer
videos below 7 minutes written, to stay inside 8 minutes spoken. Should the checks apply the `spoken` map first?

## 13. How the `spoken` map is applied

`scripts/narrate.py` replaces each `spoken` entry with a plain, case-sensitive substring replace in the order of the
map, with no word boundaries. A short key (`Url`, `IShape`, `js`, `PO`) also changes longer words that contain it, a
key that is a prefix of another works only when the longer key comes first, and an entry that ends in a full stop
doubles the period at the end of a sentence. `references/storyboard.md` says none of this.

**For now:** authors list longer keys first, use whole phrases as keys, and end no respelling with a period. Should
`spoken()` sort keys longest first and match whole words, and should the reference describe the rule?

## 14. Pronunciations that need a person

A new `spoken` spelling is meant to be confirmed with `scripts/pronounce.py`, which creates audio and which the content
step must not run. `narrate.py` has a global `SPOKEN` table with only a few entries, so the same words (GraphQL, CORS,
CSV, ID, Autoroute, `robots.txt`, `.xml`, PO) are decided again in each video.

**For now:** the spelling is written in the video's `spoken` map and listed in `OPEN-QUESTIONS.md` as unconfirmed.
Should confirmed spellings move into the global table, and who confirms them and when?

## 15. Written identifier or spoken form in the narration

The worked example writes the spoken form in the text ("Update Async", "Get Secret Value Async"), so captions show the
spaced words. Most developer videos of the programme write the identifier as in code and respell it through `spoken`,
so captions keep the exact spelling.

**For now:** the narration holds the written identifier and the voice gets the spoken form from the `spoken` map.
Should the specification name one convention, and say how to treat property names and URL paths (`ContentItemId`,
`api/content`)?

## 16. On-screen labels that contain a banned word

The programme bans "now", "new", "latest", "recently", "yet" and version numbers in narration, and also asks for UI
labels named exactly. Labels collide: Run now, Publish Now, Latest, Recently created, Index Latest version, New
application, Version 1, Lucene 4.8, OAuth 2.0, Display New menu. The validator rejects "now" and any version-like
number even inside a label, and does not scan slides.

**For now:** narration describes the item ("the item that runs it on demand") and the exact label appears only on
screen, in captions and on slides. Labels with "new" are quoted when nothing else works. Should a quoted label be
exempt from the word list, for example through an allow-list?

## 17. Words the validator does not check

`validate_content.py` bans seven phrases (powerful, seamless, simply, easily, new feature, in this release, now). The
programme also bans currently, recently, latest, yet, still, today, soon, easy, effortless, robust, great, amazing,
best, just, of course, obviously and "we added", and applies the rules to the README, `video.json` and titles. The
validator reads only narration.

**For now:** authors and reviewers scan for the full list by hand, in narration, README and `video.json`. Should the
validator check the full list in all three, leaving out the documentation URL segment `latest`?

## 18. Sentences per step and the closing card

The specification asks for one to three sentences per step and a closing card of one summing-up sentence followed by
"Thanks for watching." The validator checks only the word count, so steps of four or five sentences passed, and the
closing card is checked by its final words only. A quoted label that ends in a question mark or a period also changes
the count.

**For now:** reviewers count sentences by hand and treat a quoted label as part of its sentence. Should the validator
count sentences and check the closing card?

## 19. Card titles longer than the frame

`brand.card()` draws the title on one line at 72 px bold (76 px in the worked example), about 35 to 40 characters.
Tracker titles of 50 to 70 characters measure 1,900 to 2,300 px on a 1,920 px card, and the specification does not say
that the card title may differ from the YouTube title.

**For now:** the storyboard's title for chapters 00 and 99 is a short form of the tracker title (about 35 characters),
and `video.json` keeps the tracker title. Should `card()` wrap or shrink, or should the storyboard have a separate card
title field that the validator measures?

## 20. Product text on screen in an admin video: code, Liquid, JSON, keys

The admin rule says an admin video never shows code, Liquid, JSON or configuration. Many admin screens print such text
themselves: the Pattern box (a Liquid default) of Autoroute, Title, Alias and Preview parts, the Page title format
field, hints reading "With Liquid support", the Variables tab of the workflow designer, endpoint paths, feature ids,
the Media Options notice that names `appsettings.json`, and hints that name a configuration key.

**For now:** the rule covers what the video writes (narration, slides, captions, `video.json`, README). Recorded
product text is accepted when the narration never reads it and no highlight points at it; a plan scrolls such fields
out of frame where it can. Should the rule say this, or call for a crop or mask?

## 21. Admin screens that are code editors or code viewers

Some admin pages are JSON or code editors by design: JSON Import, View As JSON, the Diff and User Snapshot panels of
Audit Trail, Run Lucene Query and Run Elasticsearch Query, the Rules of a Feature Profile, the Html Body and Monaco
editors, and the workflow Variables tab. A video about the feature cannot avoid the screen.

**For now:** a short recording is allowed when the narration describes the effect and never reads the content, and no
JSON appears on slides, in captions or in metadata. Pages whose only control is a query editor are left out and named
on a slide. Should these screens be allowed, or told in a developer video?

## 22. Values typed into fields and text that is the subject

Some admin videos teach text the viewer types: Markdown marks, shortcodes such as `[image width="480"]`, regular
expressions (`^`, `(.*)`, `$1`), CRON expressions, a short HTML tag, `_blank`, a Stereotype identifier, a path such as
`~/images/placeholder.png`, URL paths such as `/search`. The no-code rule does not say whether these are code.

**For now:** a value the viewer must type into a field, and text that is the video's subject, count as content, not
code, as long as there are no braces, backticks or code panels. Should the specification confirm this?

## 23. Success toasts hide after 5 seconds

Admin success messages are toasts that hide 5 seconds after the page loads, but a recorded step holds until its
narration ends, and the contact sheet checks the last frame of the step. A message named late in a narration is gone
from the frame, a highlight ring around it must be cleared before it hides, and the Scene API cannot say at which
second inside a step an action happens. `references/recording.md` does not mention any of this.

**For now:** a step that names a message triggers the click at the start of the step, and a ring around a toast is
cleared within 3 seconds. Should the reference list this pitfall, and should `Scene.cs` offer a way to hold a toast?

## 24. Recording signed out, or as another user

`NewSceneAsync` always passes `state.json`, and `Scene.StartAsync` loads it whenever the file exists. A visitor clip
(sign-in, registration, lockout, front-end views) or a clip as a second user has no documented way to start, and
`Recorder setup` writes only one sign-in. Setup also signs in only to one base URL, so a second tenant with a URL prefix
needs a second state file.

**For now:** plans pass a state path that does not exist for a signed-out clip, or clear the cookies before the first
step, and use `state-<user>.json` (ignored by `state*.json`) for a second user. Should `Scenes.cs` offer an anonymous
scene and named sign-ins, and should `recording.md` describe them?

## 25. Opening a page of another origin

`Scene.OpenAsync` and `NavigateAsync` always prepend the base URL. A clip on a second origin (a mail catcher, a stand-in
front end for CORS, a second instance, a cross-origin demo) must call `scene.Page.GotoAsync` before the capture, create a
second page and call `SwitchToAsync`, which leaves a second of the previous page at the start.

**For now:** plans create the second page by hand with `scene.Context.NewPageAsync()` and say so in the step. Should
`Scene` accept an absolute URL or offer a documented multi-page pattern?

## 26. Showing an address, a status code or page source

The recorder captures the page only, with no address bar, tab strip, console or page source. Videos about addresses,
redirects, SEO titles, health endpoints, shape debug comments or served XML and text files cannot show the address, the
HTTP status or the effect on the tab title. Slide text and a `HighlightAsync` caption on a page element are the
workarounds.

**For now:** the address or status is written as a ring caption (for example `/about`) or on a slide, and narration
about the head of the page is explained on slides. Should `Scene` draw an address and status overlay, and may a URL path
appear in an admin caption?

## 27. Served output as the subject: XML, robots.txt, feeds

The result of some features is a served document: sitemap XML, `robots.txt`, an RSS feed. An admin video that opens it
in a browser tab shows markup or plain text, although the audience rule says never to show code or configuration files.

**For now:** the served output of a public URL counts as front-end output, not code; the narration never reads a tag or
a line, and names only addresses and dates. Should the specification confirm this?

## 28. Seed files and helper programs in a video folder

`SKILL.md` lists seed data files among what a folder keeps, but `validate_content.py` rejects any file outside
storyboard, video, README, slides, recording plan and open questions. Seed recipes, a fake PDF, a stand-in service of
about 125 lines, a sample front end, a throwaway module, host project or theme, and init scripts exist only as fenced
text in `recording-plan.md`, which the recorder copies by hand.

**For now:** text goes inside `recording-plan.md`; a binary file is described but not shipped. Should the format allow a
`seed/` or `host/` folder or a shared `tools/standins/`, or should `SKILL.md` stop listing seed files?

## 29. Slide-only videos and videos with no site

A video made only of cards and slides has no clip step, yet the validator requires `recording-plan.md`, and the README
template assumes a site, Auto Setup and the recorder. Terminal commands (`dotnet new`, `dotnet run`) and effects outside
the browser also become slides, with no terminal capture.

**For now:** the plan says "no recording", gives one line per slide step, and the README "Recording it again" says so.
Should the skill allow the plan to be omitted, say what it holds, and state how command output is shown on slides?

## 30. Code on slides: real, compilable and trimmed

The skill asks for real, compilable code from the docs or the source, trimmed with `// ...`. Developer videos need
excerpts that do not compile (documentation types left undefined, statements outside a method), samples written for the
video (client code, a custom extension, a protocol call), documentation shape listings with placeholders, long lines
re-wrapped to fit, and omission markers that are invalid in JSON, Razor, HTML or Liquid.

**For now:** a trimmed, docs-faithful excerpt or a sample checked against the source signatures is accepted, with
`/* ... */` for JSON, `@* ... *@` for Razor and `<!-- ... -->` for HTML and Liquid. A sample written for the video is
listed in `OPEN-QUESTIONS.md`. Should the specification say this?

## 31. Code panel lexers

`brand.LEXERS` holds csharp, json, html, cshtml, bash, powershell, javascript and xml. Liquid, Razor, SQL, GraphQL,
TypeScript, CSS, Dockerfile, YAML, PO and plain text (file trees, logs, URLs) are drawn with another lexer, so tags
come out uncoloured, and `draw_code(..., 'sql')` or `'liquid'` raises a `KeyError`.

**For now:** Liquid uses `html`, Razor `cshtml`, GraphQL and TypeScript `javascript`, and trees, logs and PO files
`bash`. Should `LEXERS` gain liquid, razor, sql, graphql, typescript, css, yaml, docker, po and text entries?

## 32. Code panel fit and the slide check

`draw_code` neither wraps, clips nor warns. The reference says "at most about 20 lines" but real excerpts need 22 to 24
lines, and `note_cards` allows four cards that end at the frame edge when the panel starts at y = 240. `storyboard.md`
asks authors to check a slide with `.save('x.png')`, while the programme forbids saving images.

**For now:** slides are rendered in memory, and text widths and panel heights are measured with a throwaway script
outside the repository; panels of up to 22 lines at 22 px and three cards are accepted. Should `brand.py` raise on
overflow and the reference describe an in-memory check?

## 33. Real domains and names on slides and in narration

The domain rule allows only example.com, orchardcore.net and docs.orchardcore.net. Subjects need other names: package
feeds (nuget.org, a preview feed), registry hosts in Dockerfiles and commands, the Azure vault domain, the project
repository address, a demo service, a hosted service, third-party tools and projects named in the documentation, and
`localhost` addresses.

**For now:** a placeholder (`<the URI of your vault>`) or a description replaces the address on slides, a tool is named
without its operator, and nothing is linked from `video.json` beyond the allowed domains. Should the rule list the
project's public infrastructure and the documentation's own products, and may a slide show quoted code with a registry
host?

## 34. Blocking outside calls and trusting certificates

`references/recording.md` says to set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address and to accept an emulator's
certificate only in the recording process. The proxy affects the site process only (not the browser, not SMTP) and also
blocks a localhost stand-in without `NO_PROXY`; the Azure SDK clients cannot take a process-only certificate exception;
and the HTTPS settings page is read-only over HTTP, while `Scene.cs` does not ignore certificate errors.

**For now:** plans add `NO_PROXY=localhost,127.0.0.1`, route-abort third-party hosts per scene, rely on the trusted
ASP.NET Core development certificate where an SDK needs one, and record HTTPS-only pages read-only over HTTP. Should the
reference say so and offer an opt-in switch for the development certificate?

## 35. Recorder helpers that the Scene API lacks

Plans fall back to raw Playwright calls for: replacing the text of a box, typing into a CodeMirror or Monaco editor,
a date-time input, a bootstrap-select picker, a file upload or download, a key press, a hover, and `GetByLabel` with
`Exact`. Each plan rediscovers the workaround, and `Scene` takes `ILocator`, not selector strings.

**For now:** plans name the Playwright call (`FillAsync`, `Keyboard.TypeAsync`, `RunAndWaitForDownloadAsync`) and mark it
as off camera or instant. Should `Scene.cs` gain tested helpers for these, and should the plan format say once that a
selector stands for an `ILocator`?

## 36. Strict locators and silent highlights

All Scene methods resolve locators in strict mode, and `HighlightAsync` catches the strict-mode violation and prints
"(nothing to highlight)", so a take can end with no ring and no error. Typical causes are the hidden input that follows
an `asp-for` checkbox, substring label matches, `:text-is` on text inside a child span, and a tooltip that removes the
`title` attribute.

**For now:** plan selectors must match exactly one element, and the review step checks the log for "nothing to
highlight". Should `HighlightAsync` fail the take, and should `recording.md` list these pitfalls?

## 37. Cutting a slow navigation and a click that restarts the tenant

Enabling a feature or saving settings releases the tenant, so the page after the click loads slowly. `OffCameraAsync`
placed after `ClickAndWaitForNavigationAsync` cuts nothing, and wrapping the click drops the ripple and needs a longer
timeout than the 20 seconds of `Scene.StartAsync`. Narrated menu paths are also opened by address, so the path never
appears on screen.

**For now:** plans wrap the click in `OffCameraAsync` after resting the cursor on the button, raise the timeout, and open
pages by address. Should the skill document a pattern or a method, and say whether to click through the menu?

## 38. Site state between clips and takes

`recording.md` models state as `App_Data` snapshots and clips that run in order. Plans also need: setup or seeding
between two clips, a restart or a file added to the checkout between takes, a different start-up state per clip,
a warm-up that takes minutes, a second tenant created by Auto Setup, a site that is not installed (Setup screen), and
the written default credentials `admin@example.com` and `Password1!`, which conflict with the programme's rules.

**For now:** plans state manual steps and snapshots between clips, record such clips one `record` call at a time, and
use `admin@example.com` with a password from `VIDEO_PASSWORD`. Should `recording.md` describe these cases and drop the
written credentials?

## 39. Estimated minutes overwritten by `tracker.py set`

`tools/tracker.py set <slug> content-ready` (and `--force`) replaces `estimatedMinutes` with the measured narration
length (4.5 becomes 4.8). The plan's target is lost, so "aim close to the estimate" can no longer be checked, and the
one-decimal rounding differs from the README's m:ss.

**For now:** the measured value replaces the estimate. Should the plan's target be kept in a separate field, and should
the validator warn when the narration is more than about 10 percent away from it?

## 40. Documentation pages in the tracker that the video does not cover

The `docs` list of a tracker entry becomes `embeddedIn` in `video.json` and a link in the description. Several entries
list pages that do not describe the video: a developer page on shapes for an admin Themes video, the Tenants page for a
Session activity, the Permissions table for a feature documented elsewhere, Media Slugify, TicketStore, Localize or
UrlRewriting pages, and a page that already embeds a video by iframe.

**For now:** `embeddedIn` follows the tracker; the description also links the page that really covers the feature, and
the tracker is not edited. Should `embeddedIn` list only pages the video is about, and what replaces an existing iframe?

## 41. Tracker entries that do not match the code

Some entries promise more or something different from the code: a title that names a feature the module lacks (lockout
options, an Azure query source, "Show Errors" for shape debug output, "Links" for an address-only shortcode), a feature
with no behavior of its own (a Worker feature), a guide id as the main feature, and an audience that differs from the
catalog (admin versus developer). A pending defect can also change a form field.

**For now:** the video follows the code, keeps the tracker title in `video.json`, records the gap in
`OPEN-QUESTIONS.md`, and stays `content-ready` with a gate in the plan. Should the entry be retitled, re-scoped or
blocked, and who reports a defect upstream?

## 42. Names for controls and activities with more than one label

The rule is to name UI labels exactly. Several controls have no single exact name: icon-only buttons (a tooltip or an
`aria-label` is the only name, and a headless capture never draws a tooltip), a menu entry and the page heading it
opens, a workflow activity that reads differently in the toolbox, the quick-add list and the canvas header, and outside
screens (the cloud portal, a third-party editor) whose labels are not in the source.

**For now:** the narration uses the label of the surface the clip is on, names an icon button by its title and shows the
name in a ring caption, and takes outside labels from the documentation page, listed in `OPEN-QUESTIONS.md`. Should the
specification say which name wins?

## 43. Labels with punctuation, case or a parenthesis

Exact labels do not always read as speech: a trailing period ("Execute GraphQL."), an ellipsis (the search placeholder),
an ampersand, a hyphen ("Execute Api - All queries"), a label that starts in lower case ("and continue"), labels that
differ only in case (Ok and OK), a parenthesis ("JSON Web Token (JWT)"), and a lone hyphen or period that the validator
counts as a word.

**For now:** the narration drops the final period, the ellipsis and the parenthesis, keeps case as in the code, and uses
`spoken` for an ampersand or a separator hyphen; the exact label stays in the plan. Should the specification say so,
and should the word counter ignore tokens made of punctuation?

## 44. Decimals and version numbers in narration

The version pattern in `validate_content.py` rejects any `digits.digits` and any "version" followed by a digit. That
blocks "OAuth 2.0", a placement position of 1.5, a price of 19.50, an address such as 127.0.0.1, an integer stored
version in a migrations video, a resource major version, and minimum SDK versions. Code slides also carry image tags and
`--framework` values that date the video.

**For now:** narration spells the number out ("one point five"), drops the standard's number, or says "a major number,
such as five". Slides keep real tags only when the code needs them. Should the check apply only after "v" or "version",
or to numbers of three parts, and should slides elide versions with a placeholder?

## 45. Avoiding numbers that change

`storyboard.md` says to avoid numbers that change, unless the video shows them. The narration needs some: the last 50
items of a palette, "ten days by default" for a trimming code default, "four parts" for a fixed structure, and the
number of kinds of node. It is unclear whether a count that is fixed by design, or a default that is not shown, is
allowed.

**For now:** a count is kept when the video shows it or when it describes the fixed structure of the guide; a default
that no clip shows is left out or confirmed in `OPEN-QUESTIONS.md`.

## 46. A sample file or data that differs from what the viewer gets

A recording sometimes needs data that a viewer's site does not have: a seeded list from the Blog recipe, a small
stand-in translation file, stock recipe content with person-like names, a field hint with a handle, a date in a near
year, a fake phone number in a range that may belong to a real person, a default recipe that breaks a permission video,
and a password typed on screen for a throwaway account.

**For now:** fake data follows the existing rule (names, dates in 2030, `555-01XX` numbers); a seed that differs from a
viewer's site is said in the narration or the plan; stock content with real-looking names is replaced in the seed, not
masked. Should the skill say which route a stock real name takes, and which recipe a permission video uses?

## 47. Pointers to other videos and overlapping scope

A pointer such as "a separate video covers X" is allowed "if truly needed", but the tracker has no field that records the
topics of a planned video, so an author cannot check it. Sibling videos also share a module or a screen (SEO meta and
robots, Lucene and indexing workers, the Setup screen in a developer and an admin video), and thumbnails share the same
card title.

**For now:** a pointer names the sibling by topic only and is kept to one per video; overlap is trimmed to a short
mention. Should the tracker note the topics each planned video covers?

## 48. Narration that states what the capture cannot show

A page-only capture cannot show a token in an address, a hint in a tooltip, an effect on the tab title, an outside
effect (blobs in a container), a generated head element, or a result drawn by a third-party script. The specification
says a step should describe what is on screen, but does not say whether source-verified facts that no frame shows may
be narrated.

**For now:** such facts are stated in narration only when a slide or caption supports them, otherwise they are moved to
the plan as an off-camera check. Should the storyboard format require that a step describes only what the capture shows?
