# Open questions: Users: Create, Edit, Disable and Delete Accounts

Everything the narration says was read in the Orchard Core source at commit 92db4a83 (file and line references are in
`recording-plan.md`). What follows could not be confirmed from the source alone. A person with a running site must
check each item before recording, and tell the author if the result differs.

## Things to confirm on a running site

1. **The seed recipe.** The six `createUser` commands in section 2 of the recording plan are run by the `command` recipe step
   from Tools > Deployments > JSON Import. A command that fails (for example, a password that breaks the site's password
   rules) only writes a line to the command output and does not fail the import, so the page still says "Recipe
   imported successfully!". Check the Users list afterwards: it must show the seven users of the plan, each with its
   role and none with an Unconfirmed badge. The step name is matched without regard to case
   (`src/OrchardCore/OrchardCore.Recipes.Abstractions/Services/NamedRecipeStepHandler.cs`), so the lower case `command` of
   the documentation works; the recipe itself has not been run.
2. **The phone number `+12065550142`** (step `edit-2`). Saving runs a phone number library check
   (`src/OrchardCore/OrchardCore.Infrastructure/PhoneFormatValidator.cs`, `IsValidNumber`). A fictional 555-01xx number
   may be refused ("Please provide a valid phone number."). If it is refused, try `+12125550142`, then `+14155550142`
   (still fictional 555-01xx numbers). The narration does not read the number, only says to type it "with its country
   code".
3. **Selectors.** The plan's selectors were derived from the views and scripts, not from a running page. Check each with
   `Recorder probe` and confirm that it matches exactly one element, because Playwright refuses a locator that matches two:
   - the left menu: `#left-nav button.nav-group-label:has(span.title:text-is("Access Control"))` and
     `#left-nav a.item-label:has(span.title:text-is("Users"))` (the view `TheAdmin/Views/NavigationItemLink-admin.cshtml`
     puts the text in a `span.title` inside the `a` or `button`, and `:text-is` matches the smallest element holding the
     text, which is why `a:text-is("Users")` alone finds nothing);
   - the rows: `ROW(name)` is `li.list-group-item:has-text("<name>@example.com")`. Check that `ROW(admin)` matches one
     element. The header line of the list is also an `li.list-group-item` and holds the text "Administrator" (in the Role
     picker), so a locator on the user name `admin` would match it; the plan therefore uses the email;
   - the field ids (`#User_UserName`, `#User_Email`, `#User_PhoneNumber`, derived from the model prefix "User"),
     `#NewPassword` and `#PasswordConfirmation` on the Change Password page, the row buttons (`button.actions`), the
     filter button (`#filter-dropdown`);
   - the Search box `.action-bar input[type="search"]` (its name should be `Options.SearchText`, from the prefix
     "Options" of `UserOptionsDisplayDriver.cs:12`; `input[name="Options.SearchText"]` is the fallback);
   - the three bootstrap-select pickers (Role, Show, Sort): `.filter button.dropdown-toggle` should match three buttons
     in that order, each showing its title whichever option is chosen, and the option links
     `.filter a.dropdown-item:has(span.text:text-is("Author"))` and `...("Disabled Users"))` should match one element
     each. They are not native lists: a click on the button opens the menu, then a click on the option.
4. **The header Actions menu of the list.** Read in the compiled script
   `src/OrchardCore.Modules/OrchardCore.Users/wwwroot/Scripts/users-admin-list/users-admin-list.js`: the bulk Actions
   menu appears only after two or more rows are ticked (one ticked row shows no menu). Confirm that ticking
   `taylor.brooks` and `riley.nguyen` shows "2 selected" and the **Actions** button (step `find-4`), and note the order
   of the two notifications that follow: the code does not sort the checked users (`AdminController.cs:232`), so
   "User taylor.brooks successfully disabled." and "User riley.nguyen successfully disabled." can appear in either order.
   The same script hides the three pickers while two or more rows are ticked.
5. **Read-only User name and Email** (step `edit-1`). The code disables both inputs while Settings > Security > User
   Login leaves the matching option off, and the options default to off. Confirm that both inputs look disabled in the
   Edit User page of a freshly set up site.
6. **The roles listed on the Create User page** (step `create-3`). The Blog recipe creates Moderator, Editor, Author and
   Contributor (`src/OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`, the `Roles` step), and the page lists
   the roles the signed-in user may assign. Confirm that **Author** and **Contributor** are listed; the narration does
   not read the whole list.
7. **Pronunciation.** The storyboard's `spoken` map says `morgan.ellis` as "morgan dot ellis" and `example.com` as
   "example dot com" (step `create-1`), so the voice does not guess at the dots. These two spellings are new: check them
   with `scripts/pronounce.py` and keep or change them. Also listen to how the voice says "sam" (step `find-1`, a user
   name, not the letters S.A.M.), "Ok" (step `create-5`, the label of the dialog button), and "Settings, Security, User
   Login" (step `edit-1`), and add a `spoken` entry for any that is wrong.
8. **The length of the actions.** The actions listed for a step must end before its narration does. The estimated lengths in
   the plan are from the word counts; after `narrate.py`, compare them with `durations.json`, in particular
   `actions-2`, `actions-3`, `find-3` and `find-4`, which chain several page loads.

## For the maintainers

- **A page that embeds this video.** The tracker entry lists `src/docs/reference/modules/Users/TicketStore.md` among its
  docs, and `video.json` therefore lists that page in `embeddedIn` and links to it. The page is about the Users
  authentication ticket store, which a separate developer video covers; this video does not mention it. Decide whether
  the page should embed this video.
- **A quirk found in the code, not narrated.** The bulk **Disable** action reports "User ... successfully disabled."
  after calling the service, without reading its result (`AdminController.cs:259-264`). The service refuses to disable
  the only enabled administrator (`UserService.cs:419-428`), so a bulk Disable that includes that account, when it is not
  the signed-in one, could show the success message although the account stays enabled. The video does not rely on it.
