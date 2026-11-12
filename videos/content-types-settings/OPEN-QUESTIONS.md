# Open questions: content-types-settings

Everything the narration states was read from the Orchard Core source (commit 92db4a83). The points below could not
be seen on a running site while the content was written. A person confirms each one on the recording site before
recording, and tells the author if the site disagrees. File references use the abbreviations of recording-plan.md
(CT, CO, RO, FL, CORE).

## 1. A type created in the admin starts with five boxes checked

- Narration and slide: `options-1` says a type created in the admin starts with Creatable, Listable, Draftable,
  Versionable and Securable all checked. The slide "All five options on" says the same.
- Checked in the source: `CT/Services/ContentDefinitionService.cs:85-91` (`AddTypeAsync` sets all five and writes the
  default thumbnail and category) and `CT/Startup.cs:38-48` (the default thumbnail is `~/OrchardCore.ContentTypes/placeholder.png`).
- Confirm: after the seed in recording-plan.md section 2.1, the Edit Content Type page of Event shows the five boxes
  checked and the placeholder path in Thumbnail, without the person having touched them.

## 2. Versionable on the running site

- Narration: `drafts-3` says the published version stays in place while a draft waits, that Versionable lets an item
  keep more than one version, and that without it an item has a single version.
- Checked in the source: `CORE/OrchardCore.ContentManagement/DefaultContentManager.cs:160-180` (a Versionable item
  that is edited gets a version of its own; the published one is saved as it was) and the comment at line 167
  ("meaning we use only one version") for a type that is not Versionable. The badges are drawn by
  `CO/Views/ContentsTags_SummaryAdmin.cshtml`.
- Confirm in clip `03-drafts`: after Save Draft, the row of the edited Event shows both Published and Draft badges.
- Not in the narration, on purpose: what happens when a type is Draftable but not Versionable. The source sets
  `Published = false` on the single version (`DefaultContentManager.cs:168-171` and `283-290`), which would take the
  item offline when its draft is saved. Confirm on a running site before any narration mentions it. A way to look:
  clear Versionable on a copy of the site, edit a published Event, choose Save Draft, and read the badges.

## 3. How the voice says the setting names

- The narration uses Creatable, Listable, Draftable, Versionable, Securable and Stereotype, and reads the group name
  "Event Content Type - Event" (`securable-2`) with a hyphen between two spaces.
- Listen to a sample of each with the pronunciation tool of the video producer skill before narrating. If a word is
  misread, add a spoken entry to the storyboard, written the way the skill describes. The captions keep the written form.

## 4. The permission search in the Editor role (`securable-2`, `securable-4`)

- The search box filters the rows by their description (`RO/Views/Admin/Edit.cshtml:15`, `RO/Views/Admin/_RolePermissions.cshtml:49`).
  Searching `Event` should leave only the group "Event Content Type - Event". Another permission whose description
  contains "event" would keep its own group on screen.
- Confirm on the recording site. If another group stays, report it: the author then changes the search text and the
  narration of the two steps together.
- Confirm that the descriptions listed in recording-plan.md for `securable-2` match the page (they come from
  `CORE/OrchardCore.Contents.Core/ContentTypePermissionsHelper.cs:13-26`).

## 5. Selectors and layout the source cannot show

- The Content Type filter is a bootstrap-select control built by script. The selector in recording-plan.md
  (`.contenttypes-selectpicker button.dropdown-toggle`) comes from the class on the select element
  (`CO/Views/ContentsAdminListFilters.cshtml:8`); check it with the recorder's probe command.
- The notification shown after Save ("Content type updated successfully.", `CT/Controllers/AdminController.cs:225`) is
  drawn by the admin theme; the selector for waiting on it in recording-plan.md is a guess to check.
- At 1536x864 the five checkboxes of the Edit Content Type page should be visible without scrolling. The Templates
  module puts a dropdown button labeled "Edit templates" above Technical Name for an administrator
  (`src/OrchardCore.Modules/OrchardCore.Templates/Settings/TemplateContentTypeDefinitionDriver.cs:59`,
  `src/OrchardCore.Modules/OrchardCore.Templates/Views/TemplateSettings.cshtml:5`), which pushes the form down. The
  narration of `visibility-3` therefore says the form "begins with" Technical Name. If the checkboxes are not visible,
  the plan says to scroll inside step `visibility-3`.
- Manage Content is sorted by the latest modification by default, so the three Events should be at the top. If they
  are not on the first page, the plan says to sort with Recently created.

## 6. The widget picker (`pickers-4`, `pickers-5`)

- The picker is a script-built modal (`FL/Views/ContentTypePicker.cshtml`, `FL/Assets/ts/content-type-picker.ts`).
  The source says it lists a sidebar starting with All, then each category, and shows cards with the thumbnail when the
  type has one. Confirm that, after the seed and the Save of `pickers-3`, the sidebar lists exactly All and Text, and
  that the Paragraph card shows the placeholder image at the address `/OrchardCore.ContentTypes/placeholder.png`.
- Confirm that the Page editor shows a plus button titled "Add Widget" once the FlowPart editor is set to Blocks.

## 7. Where the hint of a checkbox is drawn (`visibility-4`)

- Narration: `visibility-4` says each checkbox has a hint next to its label.
- Checked in the source: the hint is a text span that follows the label inside the same check row
  (`CT/Views/ContentTypeSettings.Edit.cshtml:17-18`). Its style adds a dash before the text and sets no block layout
  (`src/OrchardCore.Themes/TheAdmin/Assets/scss/components/_forms.scss:4-14`), so it should sit on the line of the
  label. The hints of Stereotype, Category and Thumbnail follow a full-width text box and sit below it.
- Confirm on the recording site that the checkbox hint is beside the label, not on a line of its own. If it is on its
  own line, the narration may say "beneath it" again.

## 8. Roles that already hold a permission of the type when Securable is cleared (`securable-4`)

- Narration: `securable-4` says the Roles page offers no permissions for the type, and that the general content
  permissions decide what a role can do with its items.
- Checked in the source: per-type permissions are offered only for Securable types
  (`src/OrchardCore.Modules/OrchardCore.Contents/Security/ContentTypePermissions.cs:18-20`). The
  handler that evaluates them looks the permission up by the name of the type and does not read the Securable
  setting (`src/OrchardCore.Modules/OrchardCore.Contents/Security/ContentTypeAuthorizationHandler.cs:54-69`). A role
  that was granted a permission of the type before Securable was cleared may therefore keep it.
- The seed grants no permission of the type, so the video does not meet this case, and the narration does not claim
  that such grants stop working. Confirm on a running site before any narration says more.
