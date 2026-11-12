# Open questions: User Login Settings

What the narration says was read from the Orchard Core source (the commit of `catalog/SOURCE.md`) and its documentation.
Paths that do not start with `src/` or `.agents/` are relative to `src/OrchardCore.Modules/OrchardCore.Users` of that
source. The points below could not be settled from the source or the documentation. A person confirms each one on a
running site before recording; the narration holds none of them as a fact, except where noted.

## 1. The lockout limits

- **What was checked:** `src/OrchardCore.Modules/OrchardCore.Users` and `src/OrchardCore/OrchardCore.Users.Core` never set
  `LockoutOptions` (a search for "Lockout" finds only the user store, the sign-in calls with `lockoutOnFailure: true`, and
  the unlock action). `src/docs/guides/lockout-configuration/README.md` names the options but states no default. The values
  in its JSON example (five attempts, five minutes) are an example, not a documented default.
- **Also checked:** `src/OrchardCore/OrchardCore.Users.Core/Services/UserStore.cs:748-756` returns `u.AccessFailedCount++`
  from `IncrementAccessFailedCountAsync`, which is the count before the increment. ASP.NET Core Identity compares the
  returned count with its limit (from the ASP.NET Core source, which is not in this tree), so the lock may start one
  wrong password later than the configured limit (the sixth with a limit of five). This is a second reason why the
  narration gives no number.
- **Narration:** `overview-3`, `lockout-2` give no number of attempts and no duration.
- **To confirm:** the defaults that ASP.NET Core Identity applies (believed to be five failed attempts and a five minute
  lock, with lockout allowed for new users; these come from the ASP.NET Core documentation, not from the files read here).
  The recorder does not need the number: `lockout-2` repeats the wrong password until the message appears. Count the
  attempts of the first take, together with the time the lock lasts, and report both.

## 2. Lockout is enabled for `jamie.chen`

- **What was checked:** a user created through `UserService.CreateUserAsync` goes through `UserManager.CreateAsync`
  (`src/OrchardCore/OrchardCore.Users.Core/Services/UserService.cs:136-137`), which is expected to enable lockout for the
  user when the Identity options allow it for new users.
- **To confirm:** after the lockout, `Access Control > Users` shows the **Locked** badge on `jamie.chen`
  (`Views/UserFields.cshtml:16-17`, which uses `UserManager.IsLockedOutAsync`). If it does not, look at `IsLockoutEnabled`
  of the user (`src/OrchardCore/OrchardCore.Users.Core/Models/User.cs:34`) and report it; clips `07-lockout` would then change.

## 3. What the login page shows after the lockout

- **What was checked:** when the account is locked out, `Controllers/AccountController.cs:190-196` adds the error
  "The account is locked out" and returns `View()` without the login form model, so `Model` is null in
  `Views/Account/Login.cshtml:30` (`@await DisplayAsync(Model)`, where `Model` is `dynamic`). The page has two overloads
  to bind to. `src/OrchardCore/OrchardCore.DisplayManagement/Razor/RazorPage.cs:68` (`DisplayAsync(dynamic)`) throws an
  `ArgumentException` for a value that is not a shape (`:87`). `RazorPage.cs:94` (`DisplayAsync(IShape)`) ends in
  `Implementation/DisplayHelper.cs:63-67`, which renders nothing for a null shape. Which one a null `dynamic` value binds to
  is decided by the C# runtime binder, not by the source. A small test project with the same two overloads and a
  `RazorPage<dynamic>`-like base class binds to the `IShape` overload, so the page should render the message from the
  validation summary (`Login.cshtml:17-22`) and no form. That test did not run Orchard Core.
- **To confirm:** the page shows the message "The account is locked out" and, probably, no form fields. If the page shows
  an error page instead (the `ArgumentException` above), `lockout-2` cannot be recorded as written and its narration must
  be reduced to "the account is locked". The narration of `lockout-2` says that the page reports that the account is
  locked out; it stands only if this is confirmed.

## 4. The user can sign in again after Unlock

- **What was checked:** `Controllers/AdminController.cs:628-629` resets the failed access count and clears the lock end
  date.
- **To confirm:** after `lockout-5`, sign in as `jamie.chen` with the right password in another browser context and check
  that the sign-in works. The narration of `lockout-5` states it.

## 5. The admin menu at the recorder's size

- **To confirm:** at 1536x864 CSS pixels, `#left-nav` is visible without opening a toggle, and **Settings** and
  **Security** expand by a click (they are items without a page of their own, so the whole row toggles:
  `src/OrchardCore.Themes/TheAdmin/Views/NavigationItemLink-admin.cshtml`). `open-2` assumes it. If the menu is collapsed,
  `open-2` needs an action that opens it first, and its narration ("In the menu, let's expand Settings, then Security")
  would then need to say that the menu is opened.

## 6. The layout of the Users list

- **To confirm:** whether the Users list is drawn as a list (`.list-group-item`) or as a grid, and whether the row actions
  use the Buttons layout (an **Actions** button next to **Edit** and **View**, the default,
  `src/OrchardCore.Modules/OrchardCore.Admin/AdminListShapeTableProvider.cs:116-118`) or the Menu layout (an ellipsis button
  with the accessible name **Actions**). `06-details` and `07-lockout` locate the row by the text `jamie.chen`. Adjust the
  row locator once with `Recorder probe /Admin/Users`.

## 7. Pronunciation

- **To confirm with `scripts/pronounce.py`:** how the voice says "Ok" (the label of the confirmation button, `lockout-4`),
  "Log in", and "Username". The storyboard's `spoken` entry makes `jamie.chen` sound like "Jamie Chen"; confirm that spelling.
  Nothing else is changed.

## 8. The login page with the site theme

- **What was checked:** `Services/UsersThemeSelector.cs:45,86` selects the site theme when **Use site theme for login page** is
  ticked. `TheBlogTheme` has no `Layout-Login` template, and its `src/OrchardCore.Themes/TheBlogTheme/Views/Layout.liquid:27` shows the site name in the navigation bar.
- **To confirm:** `theme-2` shows a login form inside that layout, with the site name in the navigation bar, and no
  admin-only element. The narration claims only the layout and the site name.

## 9. The input ids of the login page

- **To confirm:** `#LoginForm_RememberMe` is the id of the **Remember me** checkbox (`#LoginForm_UserName` and
  `#LoginForm_Password` are used by the recorder's own sign-in). Use `Recorder probe /Login "() => [...document.querySelectorAll('input')].map(i => i.id)"`.
