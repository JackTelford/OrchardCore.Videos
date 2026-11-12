# Open questions: Password and Lockout Options

What the author could not verify from the Orchard Core source or its documentation, how it was checked instead, and
what a person confirms on the running site. The narration states only what is verified; items 1 to 4 explain where it
stops short on purpose.

Source references are `file:line` under `src/` of `OrchardCMS/OrchardCore` at commit `92db4a83`.

## 1. How many failed attempts lock the account

The narration never gives the number of attempts that lock an account. It says `MaxFailedAccessAttempts` is "the number of
failed attempts allowed before an account is locked out", the wording of the ASP.NET Core Identity reference.

`OrchardCore.Users.Core/Services/UserStore.cs:757` implements `IncrementAccessFailedCountAsync` as
`return Task.FromResult(u.AccessFailedCount++);`, which returns the count **before** the increment. The reference of
`IUserLockoutStore.IncrementAccessFailedCountAsync` says the task contains "the incremented failed access count", and
`UserManager.AccessFailedAsync` locks the user when that returned count reaches `MaxFailedAccessAttempts`. If Identity
uses the value that way, an account locks on the attempt after the limit (the fourth wrong password with the value 3 of
the recording site), not on the limit itself.

A console program with a user store that returns the count as `UserStore` does (the value before the increment) and
`MaxFailedAccessAttempts = 3` locked the account on the 4th wrong password; with a store that returns the incremented
count it locked on the 3rd. So "allowed before" and "pass the limit" in the narration hold for Orchard Core, and the
number of attempts is still not stated.

Confirm on the recording site (`MaxFailedAccessAttempts = 3`): count the wrong passwords typed in `06-signin` until
"The account is locked out" appears. The plan loops until the message appears, so the clip does not depend on the
answer. Report a count of 4 as a bug upstream; it is also listed in the doc issues.

## 2. Defaults of ASP.NET Core Identity

Step `options-2` and the slide `options-2` state the defaults: six characters, a digit, a lowercase letter, an uppercase
letter, a non-alphanumeric character; five failed attempts; five minutes; lockout allowed for the users you create.
They are not in the Orchard Core source or documentation. Source used instead:

- the reference documentation shipped with the SDK (`Microsoft.Extensions.Identity.Core.xml`, in the
  `Microsoft.AspNetCore.App.Ref` pack of .NET 10), which gives `RequiredLength` 6, `RequiredUniqueChars` 1, the four
  `Require*` options true, `MaxFailedAccessAttempts` 5, `DefaultLockoutTimeSpan` 5 minutes, `AllowedForNewUsers` true;
- a console program that creates `new IdentityOptions()` on the .NET 10 runtime and prints the same values.

That Orchard Core sets none of the Password and Lockout options was checked by searching all of `src/` for
`Password.` / `Lockout.` / `IdentityOptions` configuration: only `User` options are set (`OrchardCore.Setup/Startup.cs:59`,
`OrchardCore.Users/Services/IdentityOptionsConfigurations.cs`), and the token provider names of the reset password,
change email and email confirmation features.

A second console program (a reviewer's) printed the same defaults on .NET 10, and showed that `jam` breaks only the length
and the uppercase rule under the options of the recording site, in that order.

Confirm on a site without the `Program.cs` edit: the Setup screen's password popover
(`OrchardCore.Setup/Views/Setup/Index.cshtml:32-73`) names the four character rules and 6 characters, and Add User with
`jam` lists the length, digit, uppercase and non-alphanumeric messages.

## 3. What `AllowedForNewUsers` does in Orchard Core

Step `lockout-2` says `AllowedForNewUsers` "decides whether a user can be locked out at all" and that Orchard Core "stores
that choice on each user when the account is created". Verified in the source: the choice is the `IsLockoutEnabled` value
of each user (`OrchardCore.Users.Core/Models/User.cs:34`, read by `UserStore.GetLockoutEnabledAsync`, `UserStore.cs:727`), whose
column defaults to false (`OrchardCore.Users/Migrations.cs:22`); nothing in Orchard Core sets it except the Users recipe
step (`Recipes/UsersStep.cs:55`), which copies it from the imported user. Not visible in the source: that `UserManager.CreateAsync` sets it from the
option when a user is created. That comes from the Identity reference ("whether a newly created user can be locked out").

A console program with a store like `UserStore` showed that `UserManager.CreateAsync` sets the flag to true when
`AllowedForNewUsers` is true. Only that case was run: with false, Identity leaves the stored flag, which is false by default.
The narration therefore says "a newly created user" and "saved on each user when the account is created", and nothing about
users that exist already.

Confirm: with `AllowedForNewUsers = false`, a user created through Add User never shows the **Locked** badge after wrong
passwords; with `true` (the recording site) it does, which is what `06-signin` and `06-unlock` show.

## 4. What the sign-in page shows after the lockout answer

When the sign-in answers "The account is locked out", the controller returns `View()` without the form shape
(`OrchardCore.Users/Controllers/AccountController.cs:195`). `Login.cshtml` then calls `DisplayAsync(Model)` with a null model,
which renders nothing (`OrchardCore.DisplayManagement/Implementation/DisplayHelper.cs:66`). So the page should show the
message and no form. Confirm on the first take of `06-signin` that the message is readable and the page is not an error page.

## 5. The configuration slides were not run inside Orchard Core

- The code of the slides compiles: the four snippets were compiled on .NET 10 against stubs of `StartupBase` and
  `IShellConfiguration` declared as in `OrchardCore.Abstractions` (`Modules/StartupBase.cs`,
  `Shell/Configuration/IShellConfiguration.cs`), and the real `IdentityOptions`.
- The JSON of `where-3` binds: a console program bound it with `GetSection("OrchardCore").GetSection("IdentityOptions").Bind(options)`
  and got the expected values (including `DefaultLockoutTimeSpan` as `00:05:00`).
- That the section is `OrchardCore:IdentityOptions` for `IShellConfiguration` is read from
  `OrchardCore/Shell/ShellSettingsManager.cs:261` (the shell configuration is the `OrchardCore` section) and
  `OrchardCore.Roles/Startup.cs:33-46` (a startup reading `IShellConfiguration`). The environment variable form
  `OrchardCore__IdentityOptions__...` follows from the same section.
- Not done: a module with this `Startup` running in a site. The recording uses the `Program.cs` form (`where-1`), whose
  `ConfigureServices` of the builder is documented for MiniProfiler and runs for each tenant (`OrchardCoreBuilder.cs:32-53`).
  Confirm the `where-2` / `where-3` form once in a throwaway module: set
  `OrchardCore__IdentityOptions__Lockout__MaxFailedAccessAttempts=2` and check the lockout threshold changes. Also check that a
  tenant's own `App_Data/Sites/<tenant>/appsettings.json` (not nested in an `OrchardCore` section, as
  `docs/reference/modules/Configuration/README.md` says) overrides it, since `where-3` says each tenant reads its own configuration.

## 6. The guide's lockout JSON

`docs/guides/lockout-configuration/README.md` writes the keys `"DefaultLockoutTimeSpan "` and `"MaxFailedAccessAttempts "`
with a space before the closing quote. A console program bound that JSON to `IdentityOptions` on .NET 10: both values stayed
at their defaults (5 minutes, 5 attempts), so the sample does nothing. The video's JSON has no such space. Reported in the doc issues.

## 7. Selectors to probe on the first take

`recording-plan.md` derives these from the source; check them with `Recorder probe` before recording:

- the admin menu markup of **Access Control** and **Users** (`OrchardCore.Users/AdminMenu.cs:27-30`; the theme's markup is not read);
- the input names `User.UserName`, `User.Email`, `User.Password`, `User.PasswordConfirmation` (from the display driver prefix
  `User`, `DisplayDriver.cs:13,75-84`, and `[Bind(Prefix = "User.Password")]`, `AdminController.cs:297`), and `LoginForm.UserName`,
  `LoginForm.Password` on the sign-in page;
- whether the Users list shows its row actions as an **Actions** button or an ellipsis menu (the default is read from
  `OrchardCore.Admin/AdminListShapeTableProvider.cs:118`, `Buttons`).

## 8. Spoken forms

The `spoken` map of `storyboard.json` spells out `IdentityOptions`, `ConfigureServices`, `IShellConfiguration`, the option
names, `Program.cs` ("Program dot C.S.") and `appsettings.json` ("app settings dot JSON"). A person checks each with
`scripts/pronounce.py` before narrating.
