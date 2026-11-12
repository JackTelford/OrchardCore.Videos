# Open questions: rules-conditions

The content was written from the Orchard Core source at commit `92db4a83` and its documentation, without running a site.
Everything the narration says as a fact was read in the source; the points below need a person at a running site
(recording plan: `recording-plan.md`).

## 1. The seed recipe

Checked: `LayerStep` (`OrchardCore.Layers/Recipes/LayerStep.cs`) accepts a `LayerRule` with an empty `Conditions` list and
generates the id of the rule; the content step (`OrchardCore.Contents/Recipes/ContentStep.cs`) imports content items as
they are written in the Blog recipe.

To confirm: importing the recipe of `recording-plan.md` through **Tools**, **Deployments**, **JSON Import** answers
"Recipe imported successfully!", creates the layer **Welcome** with no conditions, and creates the published widget
**Welcome note** in the **Content** zone of that layer (the **Design**, **Widgets** page shows it with the badge
**Welcome**). If the import needs another field (an owner, for example), the recipe in the plan must change.

## 2. Where the widget appears on the page

Checked: `LayerFilter` adds the widget to the `Content` zone before the view runs, and `ThemeLayout` adds the page body to
that zone afterwards, so the widget should come first. The narration never says where it is; it only says the note
"appears" (`site-1`, `site-2`) or is "hidden" (`site-3`).

To confirm: with `TheBlogTheme`, the **Welcome note** is visible at the top of the content column on `/` and `/about`
without scrolling. If it is not, scroll the page in `site-1` and `site-2` until it is visible when the narration ends.

## 3. The narration of `open-2` and `site-3` on a running site

The narration says that a layer with an empty rule hides its widget on every page (`RuleService` returns false for a rule
without conditions) and that a blog post matches neither condition of the group. Before the first condition is added,
`/` should not show the note; after `05-conditions`, `/blog/post-1` should not show it. The checks in the plan's seed section
cover the first one.

## 4. Dragging a condition

`know-1` says a condition can be dragged by its row to change its order or to move it into another group. This is read
from `OrchardCore.Rules/Assets/js/rules.js` (Sortable on every list of conditions, then the `UpdateOrder` action of
`OrchardCore.Layers/Controllers/LayerRuleController.cs:238`) and from the documentation. No clip shows it. To confirm:
drag the **Url** row above the **Is homepage** row of the group on a running site, and reload the page: the order is kept.
The narration says a condition can move into another group "that already holds a condition": the markup of an empty group has no list (`OrchardCore.Rules/Views/Items/ConditionGroup.Fields.Summary.cshtml:4-11`), so the script has nothing to drop into. To confirm: drag a row into an empty group and check that it is refused, and drag one into a group that holds a condition and check that it is kept after a reload.

## 5. Pronunciation (`pronounce.py`)

- `Url` is spoken as "U.R.L." through the `spoken` entry of `storyboard.json`. Confirm the spelling with `pronounce.py`
  before narrating.
- `/about` (steps `cond-4` and `site-2`) should be said as "slash about". `Javascript`, `Homepage` and `Boolean`
  are written as the labels of the screen; check that the voice reads them naturally.

## 6. The first frame of `03-open`

`/Admin` renders the admin dashboard shape (`OrchardCore.Admin/Views/Admin/Index.cshtml`), and the Blog recipe does not
enable a dashboard feature, so the page may be almost empty. Only the 0.7 seconds before `open-1` show it.

## 7. Admin menu behavior in the first clip

`open-1` selects **Design** in `#adminMenu` and then **Widgets**. Confirm on the site that **Design** opens its group
when selected and that the link **Widgets** is then visible, with the menu behavior of a fresh site
(`TheAdmin/Views/Navigation-admin.cshtml`: `data-menu-behavior`).

## 8. Quick checks of the Filter box

The **Filter** of the dialog runs on `keyup` and compares the text typed with the HTML of the card body (its title and its hint, `OrchardCore.Rules/Assets/ts/conditions-modal.ts:29-49`). Typing `any`, `homepage` and `url`
should each leave one card, and `authenticated` leaves two (**Is anonymous condition** has "not authenticated" in
its hint). The plan selects the card by its title, so the second case needs no change; confirm the first three.

The dialog also carries `data-list-management data-client-side-search="true"` and a `data-filter-value` on every card (`OrchardCore.Rules/Views/ConditionsModal.cshtml:6,21`). The generic script that reads them (`OrchardCore.Resources/Assets/js/list-management.js`) is requested only by the views that name it (`asp-name="list-management"`: ContentTypes, Roles, Secrets and Sitemaps), not by the Layers or Rules views, so only `conditions-modal.ts` should filter. To confirm: the page loads no `list-management` script. If it did, `authenticated` would match only the class name `IsAuthenticatedCondition` and leave one card, and the check above would change.

## 9. The editor field names

The plan selects `AnyConditionGroup.DisplayText`, `UrlCondition.SelectedOperation` and `UrlCondition.Value`, read from the way the display driver builds its prefix (`DisplayDriver.cs:13,75-84`) and from the `TryUpdateModelAsync(model, Prefix)` calls of the drivers. To confirm: on the **Create Rule** pages, the `name` attributes of the fields are those three (`Recorder probe` can list them). If they differ, change the selectors of `group-2` and `cond-4` in `recording-plan.md`.
