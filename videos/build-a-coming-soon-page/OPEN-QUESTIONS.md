# Open questions: Launch a Coming Soon Page While You Build Your Site

What was checked in the Orchard Core source (commit 92db4a83), and what a person has to confirm on a running site
before or while recording. Paths are under `src/` of `OrchardCMS/OrchardCore`. Nothing below is stated in the
narration as a fact beyond what the source shows.

## Labels that come from code defaults, not from a literal string

1. **"Flow"** (narration `edit-3`, and the recording plan). The section of the editor that holds the widgets has no
   literal label in the recipe. `OrchardCore.Contents/Views/ContentPart-FlowPart.Edit.cshtml:22` prints
   `typePartDefinition.DisplayName()`; for the part named `FlowPart`, with no display name set in the recipe
   (`comingsoon.recipe.json`, the `FlowPart` entry), the default is the part name without the trailing "Part"
   (`OrchardCore.ContentManagement.Abstractions/Metadata/Settings/ContentPartSettingsExtensions.cs:54-63`), that is
   "Flow". **Confirm the section is labelled "Flow".** If it is not, change `edit-3` and the plan.
2. **Widget card headers.** `ContentCard-FlowPart.Edit.cshtml:38` prints "{DisplayText} {type display name}". The
   Register widget has no display text and its type display name is "Register" (recipe line 443 and the type
   definition), so its header reads "Register". The other two widgets have types created without a display name, which
   defaults to the name split at capitals (`ContentDefinitionManagerExtensions.cs:12`, `StringExtensions.cs:30`), so
   their headers should read "Validation Summary" and "Re Captcha". The narration names only "Register".
3. **The rich-text editor label.** It prints the display name of the part `HtmlBodyPart` in the type Register
   (`HtmlBodyPart-Wysiwyg.Edit.cshtml:22`), which defaults to "HtmlBody". The narration does not name it.
4. **The Trumbowyg selectors.** The Register card uses the rich-text editor (the recipe sets the editor
   "Wysiwyg", `comingsoon.recipe.json`, the `HtmlBodyPart` entry of the Register type). The plan assumes the editable
   area has the class `trumbowyg-editor` and that a triple-click selects the paragraph. Confirm with
   `Recorder probe`, and confirm the typed message keeps its paragraph (the page styles `.masthead-content p`). If
   the paragraph is lost after typing, select the text with Ctrl+A instead, or type inside the existing paragraph.
5. **Left-menu entries in the redesigned admin.** The plan highlights the active entry Content, Content Items with an
   `href` selector; the menu labels are from `OrchardCore.Contents/AdminMenu.cs:65,68`. Confirm the selector.

## Behaviour that needs a running site

6. **Sign-up end to end.** The narration (`signup-1`, `signup-2`) describes the steps of the workflow that the recipe
   creates (`comingsoon.recipe.json`, the WorkflowType step): form fields validated, the reCAPTCHA check, a user
   created (`OrchardCore.Users/Workflows/Activities/RegisterUserTask.cs:100-117`), a confirmation email, a redirect
   to the home page. Nothing in the video submits the form. Three things in the source could not be settled by
   reading, and a person should try a sign-up on a site with a reCAPTCHA test key pair and a local SMTP sink before
   anyone describes the outcome more precisely:
   - **Missing keys do not block a sign-up.** `ValidateReCaptchaTask.cs:48` passes a callback that sets the outcome to
     "Invalid" and adds the error, but `ReCaptchaService.ValidateCaptchaAsync` (`OrchardCore.ReCaptcha.Core/Services/ReCaptchaService.cs:60-65`)
     returns false without calling the callback when the settings are missing, and the task ignores the returned
     value. By reading, a site without keys therefore skips the reCAPTCHA check instead of refusing the sign-up. The
     narration only says that the page shows a notice (`ReCaptchaPart.cshtml:9`) and that keys are entered under
     Settings, Security, reCaptcha. Is this intended?
   - **Field name.** The workflow validates the form field `ValidationSummaryPart.Email`
     (`comingsoon.recipe.json`, the `ValidateFormFieldTask` activity, and the script that calls
     `requestForm('ValidationSummaryPart.Email')`), while `Widget__Register.liquid:4` names the input from the field
     "InputFieldName", whose value is "Email". Which name the browser posts depends on the prefix the tag helper
     adds, which could not be read from the source alone. Check the posted field name in the browser; if the names
     differ, the validation of the email address would fail for every visitor. The narration tells the viewer to leave
     InputFieldName as it is, which is safe either way.
   - **Email provider.** The recipe enables the Email feature but no provider. `DefaultEmailService.cs:39` fails when
     no provider is resolved, and `RegisterUserTask.cs:107-114` creates the user before it sends the email, so a
     failed email leaves the user created. The narration says only that the email needs a provider, configured
     under Settings, Communication, Email (`OrchardCore.Email/AdminMenu.cs:38-40`).
7. **The launch date of the message.** The recipe computes it with a date calculation that adds one month to the day of
   setup (`comingsoon.recipe.json:17`). On the 29th to the 31st of a month, the calculation can land one month later
   than expected. Set the site up on a day from 1 to 28 (the plan says so), then read the date on screen. The
   narration says "in the month after setup".
8. **Set as homepage on another item.** `launch-1` says: open the item, check Set as homepage in the Permalink
   section, publish. In the source the checkbox appears only if the type's Autoroute settings show the homepage
   option and the user has the permission `SetHomepage` (`AutoroutePart.Edit.cshtml:56`), and a homepage is set at
   publish only if the item's Permalink is not empty (`AutoroutePartHandler.cs:82`). The Coming Soon recipe has no
   other content type; the narration of `launch-1` names the condition (Show homepage options in the type's Autoroute settings, default off, `AutoroutePartSettings.Edit.cshtml:45`); confirm the steps on a type created in a sibling video of the series (for example a page type
   with an Autoroute pattern).
9. **Cache.** The recipe enables Dynamic Cache. Confirm the front end shows the published message and button label
   right after `edit-5` without a restart.
10. **Background video.** `layout.liquid:24` plays `bg.mp4` (H.264). The recorder's Chromium may lack that codec; the
    narration does not describe the video, and slide `why-2` says only "A background video behind the page".
11. **Icons.** The icons are Font Awesome classes served by the site (`layout.liquid:10`). Confirm three icons are
    drawn on the front end and that the "Icon Class" field of the editor shows its icon picker without a network
    request failing.
12. **Recipe list.** Confirm the Setup screen lists "Coming Soon" with the description "Coming Soon website." when the
    host is `OrchardCore.Cms.Web`, and that the entry's `data-recipe-name` is `ComingSoon`.

15. **Position of the button and the notice** (`visit-2`). The narration says the theme draws the button text in capital
    letters (`_masthead.scss:57`, verified) and that the notice is "below the form" (the ReCaptcha widget follows the
    Register widget in the Flow, `comingsoon.recipe.json`, the `FlowPart` entry). Whether the button sits beside or under
    the field depends on the layout (`.input-group-newsletter input` is set to `width: 100%` in `_masthead.scss`, inside a
    wrapping flex row), which could not be settled by reading. The narration does not state the position. Confirm on
    screen and keep the highlight order of the plan.
16. **reCaptcha after saving the keys** (`signup-3`). `ReCaptchaPartDisplayDriver.cs:22` reads the options monitor, and
    `ReCaptchaSettings.ConfigurationExists()` needs a non-empty Site Key and Secret Key (`ReCaptchaSettings.cs:15`), so the
    notice is replaced by the check once both are saved. Confirm on a running site that the front end shows the check
    right after saving, with Dynamic Cache enabled.
17. **Email provider** (`signup-4`). With only the Email feature enabled, the Email settings page reads "No email
    providers are currently enabled." (`EmailSettings.Edit.cshtml:6-14`). The narration therefore says to enable a
    provider under Tools, Features first, and to set it up under Settings, Communication, Email. Confirm that after
    enabling "SMTP Email Provider" the Email settings page shows the SMTP tab (`SmtpSettingsDisplayDriver.cs:88`).

## Pronunciation

13. Check with `scripts/pronounce.py` how the voice reads "reCaptcha", "ReCaptcha" and "Url" (in "Social Site Url"),
    and how it reads "Notify Me!" if it is spoken. No `spoken` entry was added, because a person confirms a new
    spelling first. If "Url" is read as a word, a candidate is "U.R.L.".

## Documentation

14. The page named by the tracker for this video, `docs/reference/modules/Themes/README.md`, explains theming with a
    portfolio example and never mentions TheComingSoonTheme or the Coming Soon recipe. They are described in
    `docs/getting-started/starter-recipes.md`. `embeddedIn` keeps the tracker's page; if the iframe is added to the
    Themes page it will sit beside text about another subject. Consider embedding the video in the Starter Recipes
    page, under "ComingSoon Recipe and TheComingSoonTheme".
