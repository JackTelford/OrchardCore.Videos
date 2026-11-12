# Open questions: alias-part

Everything in the narration was read from the source (Orchard Core commit `92db4a83`) and the documentation. The
points below could not be settled without a running site. A person confirms each one on the recording site and corrects
`storyboard.json` or `recording-plan.md` if the screen differs.

## 1. What the narration relies on, and what to look at on screen

**Checked in the source.**
- The Alias feature is enabled by the Blog recipe (`blog.recipe.json:48`) and is listed under the category Content
  Management (`OrchardCore.Alias/Manifest.cs:10`). The Menu feature depends on it (`OrchardCore.Menu/Manifest.cs:13`).
- The labels read in the narration: Tools, Features; Design, Content Definition, Content Types; Edit; Add Parts; Add &
  Configure; Options; `Alias is editable`; `Alias is generated and input is disabled`; Pattern; Save; Content, Content
  Items; New; Publish; Content, Menus; Alias. Their lines are in `recording-plan.md`.
- The default pattern builds the alias from the display text, in lowercase, with hyphens (`AliasPartSettings.cs`,
  `SlugService.cs`); an alias is computed only while it is empty (`AliasPartHandler.cs:90-96`); it is unique across the
  whole site, whatever the content type (`AliasPartExtensions.cs:26-29`); the limit is 735 characters
  (`Models/AliasPart.cs`); lookups lowercase the alias (`AliasPartContentHandleProvider.cs`, `Startup.cs`).

**To confirm on screen.**
- `feature-1`: that typing `alias` in the search box hides the other rows at once, that the Alias row shows the
  dependency badge Content Types and the badge Enabled, and that the category heading above it reads Content
  Management.
- `add-1`, `add-2`, `add-4`: that the Parts list of the Article type, and the Alias row of the Add Parts page, show
  the part as `Alias` (the display name is the part name without its ending "Part", from the code), and that the
  Alias part is the last entry of the Parts list after saving.
- `generate-1`: that the Alias field is at the bottom of the Article editor, under the Banner Image field, and that
  its hint reads as in the view (`AliasPart.Edit.cshtml:15-18`).
- `generate-2` to `generate-4`, `disabled-2`: that **Publish** from an item opened through the list returns to the
  list (the Edit link carries the list as its return address, `Content.PublishButton.cshtml`,
  `Contents/Controllers/AdminController.cs:769-858`), and where the Edit button of a list row is (the Buttons layout of
  the list, or the row's actions menu).
- `generate-3`: that renaming the article does not change its permalink or alias (the Autoroute part keeps its path
  unless the option to refresh it is ticked).
- `unique-1`: that the error shows both in the summary at the top of the form and under the Alias field, and that the
  message reads `Your alias is already in use. "main-menu"`.
- `disabled-2`: that the disabled, empty Alias field of `About` is filled with `about` after publishing. From the code,
  the part is added to the loaded item, a disabled input is not posted, and the alias is computed when it is empty;
  nothing was run.
- `use-2`: that the navigation bar of the Blog theme shows the links Home and About at 1536 pixels wide, and the
  elements `#mainNav` and its links exist as written in the plan.
- Every clip that uses a list (`feature-1`, `add-1`, `generate-2` to `generate-4`, `disabled-2`): that the Features,
  Content Types and Content Items lists are drawn in the default List layout with the actions as buttons
  (`AdminListOptions.cs`: `DefaultLayout` List, `DefaultActionsLayout` Buttons), so that the rows are
  `.list-group-item` and the Edit button sits in the row. The columns Display name and Technical name exist only in the
  Grid layout. If the admin settings of the recording site choose another layout, the selectors of the plan change.
- `add-2`, `add-3`: that the settings page of the part draws, besides Options and Pattern, a green box about the Part
  Definition and a select "What type of editor should be used?" (option Standard), as the views say, and that none of
  them hides the Options select or the Pattern label at 1536x864.

## 2. Pronunciation

`storyboard.json` has one `spoken` entry: "Add & Configure" is read "Add and Configure". It was not tried with
`scripts/pronounce.py`. A person also confirms, with a sample, how the voice reads the hyphenated aliases `main-menu`
and `contact-us` (it should say "main menu" and "contact us", with no pause or the word "dash"), the long label
"Alias is generated and input is disabled", and "Autoroute". If the voice misreads one, add it to `spoken`.

## 3. Behaviors seen in the code that the video does not show or claim

- **The number added to a taken alias.** The slide says only that a number is added at the end. In
  `AliasPartHandler.cs:142-160`, `int.TryParse(..., out version)` sets `version` to 0 when the text after the last
  hyphen is not a number, so a taken alias with a hyphen (for example `contact-us`) would first become `contact-us-0`,
  while one without a hyphen (for example `about`) becomes `about-1`. The documentation says "such as `-1`". Reported as
  a documentation issue in the result of this video; confirm it on a running site.
- **Aliases that differ only by case.** The uniqueness check compares the typed alias, not lowercased, with the stored
  (lowercase) aliases (`AliasPartExtensions.cs:26-29`), so whether `Support` is refused when `support` exists may
  depend on the database (case-sensitive comparison or not). The documentation implies that such a duplicate is
  rejected. Reported as a documentation issue, worded as conditional. The narration says only that lookups ignore
  letter case, and advises against aliases that differ only by case.
- **An empty pattern.** Whether clearing the Pattern field and saving stops the generation, or restores the default
  pattern, was not checked, so the video never clears the field.
- **Cloning.** A clone gets a suffixed alias (`AliasPartHandler.cs:79-88`). The video does not clone an item.

## 4. The default pattern is visible on screen

The part settings page (clip `03-add`, steps `add-3` and `add-4`) shows the default pattern in the Pattern field. The
default pattern is a Liquid expression, and the programme's rule for an admin video is never to show Liquid, but the
page has no way to leave the field out: it sits beside its label, which the narration names. The narration does not
read the pattern, the slides do not show it, and the plan rings only the label. A person decides whether the capture
needs a crop or an overlay there (see the specification question in the result of this video), and confirms how the
field is drawn on the running site (the script of the page turns it into a pattern editor,
`OrchardCore.Alias/Assets/ts/alias-part-settings.ts`).
