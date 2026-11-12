# Open questions: workflows-user-events

The narration states only what the Orchard Core source (commit `92db4a83`) and its documentation show. The points below
are what the author could not see without a running site. Each says what was checked and what a person must confirm
before the first take. Nothing here changes the narration unless the check fails.

## 1. Pronunciation (`spoken` in `storyboard.json`)

Checked: the written forms are the labels of the code. `User Loggedin Event` and `User Loggedout Event` are written
that way in the source (`Users/Workflows/Activities/UserLoggedInEvent.cs:17`, `UserLoggedOutEvent.cs:22`).
To confirm: run `scripts/pronounce.py` on the five entries of `spoken` (`Loggedin` as "Logged in", `Loggedout` as
"Logged out", `loggedin` as "logged in", `jamie.chen` as "jamie dot chen", `example.com` as "example dot com"), and on "Notify", before narrating. The
captions keep the written form.

## 2. One activity, three labels

Checked: the same activity has three different texts on screen, from three views of the same module.

| Surface | User Created | User Loggedin | Notify |
|---|---|---|---|
| Name of the activity: start picker, node header, quick-add list, activity panel | User Created Event | User Loggedin Event | Notify Task |
| Card in the Activities pane (`Thumbnail` view) | User Created | User Logged in | Notify |
| Body of the node on the canvas (`Design` view) | User Created | User LoggedIn | the type and "Notification", such as "Information Notification" |

The narration says the label that is on screen when it speaks: "User Created Event" when it selects the button of the
start picker, "the User Updated card" and "the User Logged in card" when it selects a card of the Activities pane, and
"User Loggedin Event" for the node once it is added. To confirm: the three texts appear as listed in the designer.
If the Activities pane or the start picker shows another text, change the sentence of that step, not the plan.

## 3. The start picker offers User Created Event

Checked: `StartPicker.vue:13-14` lists at most six events, puts `UserCreatedEvent` among the suggested ones, and shows
the others in alphabetical order. To confirm: on the recording site, with only Workflows enabled as the plan says, the
heading "Start with an event" shows a button "User Created Event". The other two workflows use the search of the
Activities pane because their events are not suggested.

## 4. Notification after a sign-in, and an extra User Updated run

Checked: the Notify Task adds its message to the notifier; a response that is a redirect stores the notifications in a
cookie, and the next page that renders a view shows them (`OrchardCore.DisplayManagement/Notify/NotifyFilter.cs`). A
sign-in ends in a redirect to the return URL (`Users/Controllers/AccountBaseController.cs:11-31`), and the admin layout
renders the notifications as toasts (`TheAdmin/Views/NotifyMessages.cshtml`). The sign-in does not save the user in the
Orchard Core code: the only `LoggedInAsync` implementation is the one of the Users Audit Trail feature, which is off
(`Users/AuditTrail/Handlers/UserEventHandler.cs:62`); the login form events of the Users module
(`EmailConfirmationLoginFormEvent`, `DisabledUserLoginFormEvent`, `UserModerationLoginFormEvent`) only override
`ValidatingLoginAsync` and save nothing; there is no custom sign-in manager (`Users/Startup.cs:93` registers the
standard Identity one); and `Users/Controllers/AccountController.cs:164-173` calls `PasswordSignInAsync` and then
`LoggedInActionResultAsync`, which only triggers the event and redirects.
To confirm:

- the toast "Welcome back" appears on the Workflows list after `signin-5`, with `ReturnUrl` pointing to it;
- the sign-in does not also start the workflow "Role changed". What is left open belongs to the ASP.NET Core Identity
  framework, which is not in this source tree and was not read: it is expected to save the user at a sign-in only
  when it resets a count of failed attempts or upgrades the password hash, neither of which should apply to a first
  successful sign-in of the seeded administrator. If a second toast "A user account was updated." appears, and one
  more instance, the narration of `signin-5` and `instances-1` still holds ("its notification appears", "every
  instance is Finished"); keep the take, or disable "Role changed" on its Workflow tab just before it.

## 5. Where the user menu is, and its entries

Checked: the entry is the button "Log off" of the user menu (`Users/Views/UserMenuItems-SignOut.cshtml:4`); the admin
layout renders the navbar in the top bar (`TheAdmin/Views/Layout.cshtml:16` and `:69`) with the display type
`DetailAdmin`. In the admin the toggle is the button `#navbarDropdown` (`Users/Views/UserMenu.DetailAdmin.cshtml:7`) and
it shows only a user icon, with the user name as a tooltip (`Users/Views/UserMenuItems-Title.DetailAdmin.cshtml:1`); the
front-end menu is the one that prints the name (`UserMenuItems-Title.cshtml`). The menu opens with the line "Signed in
as admin" (`UserMenuItems-SignedUser.cshtml:4`).
To confirm: that the toggle is the user icon, that `#navbarDropdown` opens the menu, and that "Log off" is in it, with
`Recorder probe`. In the same pass, confirm that the
sidebar entries are labeled "Access Control", "Users", "Design", "Workflows" (the menu builders say so:
`Users/AdminMenu.cs:27-30`, `Workflows/AdminMenu.cs:18-19`).

## 6. The instance page

Checked: the page of an instance shows the labels "Workflow ID", "Created", "Status" and "Correlation ID"
(`Workflows/Views/Workflow/Details.cshtml`), and the designer in read-only mode highlights the activities and
connections the instance ran, from the journal (the documentation's "Execution Journal" section;
`ActivityNode.vue:211` marks them with the executed state). The correlation id of the three user events is the identifier
of the user (`Users/Workflows/Handlers/UserEventHandler.cs:45-47`, `AccountBaseController.cs:28`).
To confirm: the highlight is visible at the size of the recording; and that the Correlation ID equals the identifier in
the address of the page that edits the user (`/Admin/Users/Edit/<id>`), for the instance of "Account created", the
row that `instances-2` opens.

## 7. The Message field of the Notify Task, and its hint "With Liquid support."

Checked: the field **Message** of a Notify Task is a plain one-line text box (`NotifyTaskViewModel.cs`,
`NotifyTask.Fields.Edit.cshtml:16-18`). The syntax picker and the code editor of the module belong to the activities
that render the expression editor shape (`If/Else`, `Set Variable`, `Set Property`, and the other activities that
use `WorkflowExpressionEditor.cshtml`), not to Notify Task, Log Task or Assign User Role Task. The field has the hint
"The message to show. With Liquid support." (`NotifyTask.Fields.Edit.cshtml:19`). It is on screen in `build-4`,
`roles-3` and `signin-3`, and the video types a plain text there, never an expression, as the tracker's note asks.
To confirm: that the field is a plain text box in the activity panel at the recorder's viewport, so that `TypeAsync`
works on it, and that the hint, which is a label of the product and not an expression, is acceptable in an admin
video. If not, the recording cannot hide it without changing the page.

## 8. The activity panel

Checked: adding an activity that has an editor opens the activity panel at the bottom of the canvas
(`App.vue:274`), and the user events have an editor with no field in it (`UserCreatedEvent.Fields.Edit.cshtml`).
To confirm: how much of the canvas the panel covers at the recorder's viewport, and whether it hides the **Done**
port of the event before the third step of `02-build`. The plan says to close it with
`[data-cy='activity-panel-close']` when it does.

## 9. The documentation pages in the tracker entry

The tracker lists `src/docs/reference/modules/Users/TicketStore.md` among the pages this video backs. That page is about
storing authentication tickets on the server, which the video does not mention. It stays in `embeddedIn` as the
tracker has it, and the description links only to the Workflows and Users pages. A maintainer may want to remove it
from the entry.

## 10. What the author did not verify and the narration does not claim

- Assign User Role Task, Validate User Task and Register User Task are described on a slide from their source only.
  The video does not run them.
- The narration says a role change in the user editor is an update (`Users/Controllers/AdminController.cs:389`). It does
  not say that Assign User Role Task also raises User Updated Event, which depends on ASP.NET Core Identity and was not
  checked in this source tree.
- The Confirmed event runs when the confirmation link is followed (`Users/Controllers/EmailConfirmationController.cs:82`);
  the administrator's **Confirm email** action in the Users list does not call it, and the narration does not mention
  that action.

## 11. The number in the toast after Publish

Checked: the toast after Publish is "The workflow was published as version N." (`WorkflowsDesignerJSLocalizer.cs:221`,
`App.vue:358`). Creating a workflow saves it once through `WorkflowTypeStore.SaveAsync`, and that call creates a version
whenever the saved definition differs from the latest version, or when there is none
(`Workflows/Services/WorkflowTypeStore.cs:87`, `Workflows/Services/WorkflowTypeVersionStore.cs:95-111`). So a new,
empty workflow already has a version 1, and the first Publish is expected to create version 2. The narration never says
the number, and the plan waits for the text without it.
To confirm: the number in the toast of the first Publish, and the version badge in the toolbar after it. If the number
matters to a caption, write the one that appears.
