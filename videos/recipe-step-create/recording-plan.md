# Recording plan: Write a Recipe Step

This video has **no browser recording**. Every chapter between the opening and closing cards is a slide clip drawn by `slides.py` from code of the Orchard
Core source and examples of its documentation, so a person who makes the video needs no site, no tenant, no recorder, no
seed data and no sign-in. Do not run `Recorder`, and do not create a `recorder/` folder or a `state.json`.

The code on the slides is read-only material from the Orchard Core source (`OrchardCMS/OrchardCore` at commit
`92db4a83`) and from its documentation page `src/docs/reference/modules/Recipes/README.md`. Nothing in the video
needs a running Orchard Core.

## The site it needs

None.

| Item | Needed |
|---|---|
| Auto Setup recipe, tenant, time zone | No |
| Features to enable before recording | No |
| Settings to save | No |
| External services and local stand-ins | No |
| Seed data | No |
| `VIDEO_USER_NAME`, `VIDEO_PASSWORD`, `VIDEO_INIT_SCRIPT` | No: nothing signs in |

## How to make the video

1. From the repository root, with `pillow`, `pygments`, `imageio-ffmpeg` and `edge-tts` installed:

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/recipe-step-create
   python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/recipe-step-create
   ```

   `assemble.py` draws every slide with `slide()` of `slides.py`, the opening and closing cards, and the chapter
   cards. There is no clip to record between the two commands.
2. Check the pronunciation questions in `OPEN-QUESTIONS.md` before narrating for good.
3. Look at the frames of every slide (see the checks at the end of this file), then save the opening card as
   `thumbnail.jpg` as the skill describes.

## Order of the clips, and what they change

All clips are slide clips. They do not depend on each other, they do not touch any site, and no snapshot is needed.
They can be drawn in any order; `assemble.py` plays them in the order of the storyboard.

## The slides

Every code panel is trimmed with `// ...` where a part is left out, as `references/storyboard.md` asks. File paths are
relative to `src/` of the Orchard Core source at commit `92db4a83`, and line numbers are those of that commit.

### Chapter 01, What a recipe step is (`01-concept`)

- `concept-1`: slide "A recipe is a list of steps". A recipe file with the `name` and `displayName` of the recipe and
  two steps, `feature` and `themes`, next to two cards: "name" and "Other properties". The steps are the `feature`
  and `themes` examples of `docs/reference/modules/Recipes/README.md` (the sections "feature" and "themes"). The
  recipe head is written for the slide.
- `concept-2`: slide "How Orchard Core runs a step". Four cards: read the steps in order, evaluate the script
  expressions, hand the step to every handler, the matching handler acts. Source:
  `OrchardCore/OrchardCore.Recipes.Core/Services/RecipeExecutor.cs:79-144` (loop over the steps) and `:198-218`
  (evaluation, then every handler).
- `concept-3`: slide "Where the same handlers run". Four cards: the Recipes page
  (`OrchardCore.Modules/OrchardCore.Recipes/Controllers/AdminController.cs:113-157`), site setup
  (`OrchardCore/OrchardCore.Setup.Core/Services/SetupService.cs:245-248`), a deployment import
  (`OrchardCore.Modules/OrchardCore.Recipes/Services/RecipeDeploymentTargetHandler.cs:31-46`), a data migration
  (`OrchardCore/OrchardCore.Recipes.Core/Services/RecipeMigrator.cs:34-57`).

### Chapter 02, The step handler (`02-handler`)

- `handler-1`: slide "The step handler interface". Two code panels side by side, `IRecipeStepHandler.cs` and
  `RecipeExecutionContext.cs`, and two cards "Called for every step" and "Decides by name". Sources:
  `OrchardCore/OrchardCore.Recipes.Abstractions/Services/IRecipeStepHandler.cs:9-16` and
  `OrchardCore/OrchardCore.Recipes.Abstractions/Models/RecipeExecutionContext.cs:5-20` (the `Name`, `Step` and
  `Errors` properties; the other properties are left out with `// ...`).
- `handler-2`: slide "Named Recipe Step Handler". The whole class, with three cards for the constructor,
  `ExecuteAsync` and `HandleAsync`. Source:
  `OrchardCore/OrchardCore.Recipes.Abstractions/Services/NamedRecipeStepHandler.cs:5-28` (the XML comments are left
  out).
- `handler-3`: slide "A complete example: the Themes step". The usings, the class declaration, the two fields and the
  constructor of the Themes step; the lines of the declaration, the fields and the constructor are bright. Source:
  `OrchardCore.Modules/OrchardCore.Themes/Recipes/ThemesStep.cs:1-24`.
- `handler-4`: slide "Read the step with ToObject". `HandleAsync` of the Themes step on the left
  (`ThemesStep.cs:26-40`), the `ThemeStepModel` class on the upper right (`ThemesStep.cs:42-46`), the `themes` step of
  the documentation on the lower right (`docs/reference/modules/Recipes/README.md`, section "themes"), and a card
  "Names match". The case-insensitive match comes from `PropertyNameCaseInsensitive = true` in
  `OrchardCore/OrchardCore.Abstractions/Json/JOptions.cs:28`, copied into `JOptions.Default` (`:45`), which `ToObject` uses
  (`OrchardCore/OrchardCore.Abstractions/Json/Nodes/JNode.cs:95`).

### Chapter 03, Read the step (`03-read`)

- `read-1`: slide "Script expressions are evaluated first". The `settings` example of the documentation with a
  `variables` property, and two cards: "In the recipe file" and "In the context". Sources: the `settings` section and
  the `variables` example of `docs/reference/modules/Recipes/README.md`; the evaluation happens before the handlers in
  `RecipeExecutor.cs:206` and `:237-303` (`EvaluateJsonTreeAsync`).
- `read-2`: slide "Read the JSON node yourself". `HandleAsync` of the Shortcode Templates step and a card
  "TryGetPropertyValue". Source: `OrchardCore.Modules/OrchardCore.Shortcodes/Recipes/ShortcodeTemplateStep.cs:25-39`.

### Chapter 04, Register the handler (`04-register`)

- `register-1`: slide "Register the handler". The `Startup` class of the Themes module, with the line
  `services.AddRecipeExecutionStep<ThemesStep>();` bright, and a card "In your module". Source:
  `OrchardCore.Modules/OrchardCore.Themes/Startup.cs:1-26` (the other usings and registrations are left out).
- `register-2`: slide "What the registration does". The `AddRecipeExecutionStep` extension method and a card
  "Scoped". Source: `OrchardCore/OrchardCore.Recipes.Abstractions/ServiceCollectionExtensions.cs:9-16`.
- `register-3`: slide "A step that no handler claims is skipped". The loop of the executor that calls every
  handler, and two cards: "No Feature attribute" and "Feature disabled". Sources:
  `RecipeExecutor.cs:202` and `:215-218`; the feature of a type without a `Feature` attribute is the module's own
  in `OrchardCore/OrchardCore/Extensions/ExtensionManager.cs:418-423` (`GetSourceFeatureNameForType`, used at `:208`); the module
  `OrchardCore.Modules/OrchardCore.Recipes/Startup.cs` shows both kinds of `Startup` class.

### Chapter 05, Use the step in a recipe (`05-recipe`)

- `recipe-1`: slide "A recipe file that uses the step". A recipe with `name`, `displayName`, `description`, `tags`
  and one `themes` step, and three cards: "Recipes folder", "Top-level properties", "steps". The properties are those of
  the table "Key Properties" in `docs/reference/modules/Recipes/README.md` (their values, `MyRecipe`, "My recipe",
  the description and the tag, are written for the slide); the folder name is
  `OrchardCore/OrchardCore.Recipes.Abstractions/RecipesConstants.cs:5`.
- `recipe-2`: slide "The Recipes page". The filter that builds the list of the Recipes page and two cards: "Not
  listed" and "Run". Source: `OrchardCore.Modules/OrchardCore.Recipes/Controllers/AdminController.cs:160-169` (the
  long last condition is wrapped on two lines); the button label is at
  `OrchardCore.Modules/OrchardCore.Recipes/Views/RecipeEntry.Buttons.SummaryAdmin.cshtml:18`; the menu path
  Tools, Recipes is at `OrchardCore.Modules/OrchardCore.Recipes/AdminMenu.cs:18-26`.

### Chapter 06, Report problems (`06-errors`)

- `errors-1`: slide "Report a problem". `HandleAsync` of the Recipes step, trimmed to the loop that adds the error, and
  two cards: "Errors" and "S". The lines that add the error are bright. Source:
  `OrchardCore.Modules/OrchardCore.Recipes/RecipeSteps/RecipesStep.cs:26-47`.
- `errors-2`: slide "A step with errors stops the recipe". The two parts of the executor that mark the step as failed
  and throw, and two cards: "Failed" and "Stopped". Source: `RecipeExecutor.cs:106-111` and `:131-134`.
- `errors-3`: slide "What the person who runs the recipe sees". The `catch (RecipeExecutionException e)` block of
  the Recipes page (the notification line is wrapped on three lines) and two cards: "Your messages" and "Write them".
  Source: `OrchardCore.Modules/OrchardCore.Recipes/Controllers/AdminController.cs:144-149`. The advice to localize the
  messages and keep sensitive data out comes from the `docs/releases/2.0.0.md` section "Recipe Executions", and
  from the `S[...]` messages of the built-in steps.
- `errors-4`: slide "A thrown exception". The `catch (Exception e)` block of the executor and two cards: "In the
  notification" and "In the log". Sources: `RecipeExecutor.cs:116-125` and `AdminController.cs:146`.

### Chapter 07, More than one step name (`07-several`)

- `several-1`: slide "More than one step name". `ExecuteAsync` of the Translations step. Source:
  `OrchardCore.Modules/OrchardCore.DataLocalization/Recipes/TranslationsStep.cs:14` and `:27-38`.

### Chapter 08, Writing a recipe step (`08-recap`)

- `recap-1`: slide "Writing a recipe step". Four cards: derive from `NamedRecipeStepHandler`, read the step with
  `ToObject`, report problems in `Errors`, register with `AddRecipeExecutionStep`.
- `recap-2`: slide "Then use it". The recipe of `recipe-1` with its `themes` step bright, and two cards: "In the
  recipe" and "Deployment steps". The card points to the separate video on deployment steps; the claim that a
  deployment step contributes recipe steps comes from `docs/reference/modules/Deployment/README.md`.

The opening card (`intro`) and the closing card (`outro`) are drawn from the storyboard titles and subtitles.

## Checks before the video is published

- Look at every slide at full size: nothing is cut off at the edge of a code panel or a card, every text is at least
  21 px, and the bright and dimmed lines are the ones the narration talks about.
- Compare every code panel with the source line ranges above: the code is the source's, trimmed, never reworded.
- Private data: the slides show only Orchard Core source code and the example names of its documentation
  (`MyRecipe`, `MyCustomTheme`, `TheAdmin`). No person, company, email address, domain other than the Orchard Core
  ones, machine path, user name, password or token appears on any slide, in the captions, in `video.json` or in the
  README. The only recording-related secret of other videos, the password in `VIDEO_PASSWORD`, is not used here.
- No `Recorder`, `state.json`, `App_Data`, mask script, MP4 or audio file is committed with this folder.
