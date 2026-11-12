# Write a Recipe Step

**YouTube:** not published yet · **Length:** 5:14 · **Feature:** `OrchardCore.Recipes` ([Recipes](https://docs.orchardcore.net/en/latest/reference/modules/Recipes/))

How a module adds a step to the recipes of Orchard Core. The video covers the step handler and
`NamedRecipeStepHandler`, reading a step with `ToObject` or as a JSON node, registering the handler with
`AddRecipeExecutionStep`, using the step in a recipe, and reporting problems in the `Errors` list. Every code panel is
code from the Orchard Core source, or an example from its documentation, trimmed. The title, description, tags and chapters YouTube shows are in
`video.json`.

## Recording it again

The video has no browser recording: all its chapters between the opening and closing cards are slides, drawn from `slides.py`, so it needs no site, no seed
data, no sign-in and no recorder. `recording-plan.md` lists every slide with the file and lines of the Orchard Core
source its code comes from.

1. Check the questions in `OPEN-QUESTIONS.md` about how the voice reads the names of the code.
2. Narrate and assemble, from the root of the repository:

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/recipe-step-create
   python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/recipe-step-create
   ```

3. No clip depends on another one, and no clip changes a site, so there is no order to follow and no snapshot to
   restore. Look at the frame of every slide before publishing.
