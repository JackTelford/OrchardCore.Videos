# Source Generators: Less Boilerplate in Your Modules

**YouTube:** not published yet · **Length:** 4:50 · **Feature:** `OrchardCore.SourceGenerators` ([Source Generators](https://docs.orchardcore.net/en/latest/topics/source-generators/))

A video for module authors about the generators and analyzers of the `OrchardCore.SourceGenerators` project. It covers
argument classes marked with `GenerateArguments`, anonymous objects passed to `Arguments.From`, typed shapes created with
`Initialize` and `CreateAsync` or marked with `GenerateShape`, the `OCSG001` and `OCSG002` analyzers, and the build
properties that turn the generators on in the Orchard Core solution. The title, description, tags and chapters YouTube
shows are in `video.json`.

## Recording it again

The video has no recording: the opening and closing cards are drawn by the producer, and every chapter between them
is made of slides drawn by `slides.py` from code of the Orchard Core source, so it needs no site, no seed data, no
sign-in and no recorder. The generated code on the slides was written
from the source of the generators, and `OPEN-QUESTIONS.md` lists what a person must confirm by building a project.

1. Render every slide once in memory, with the skill's `scripts` folder on `PYTHONPATH`, to check that nothing overflows.
   `recording-plan.md` has the command; it writes no file.
2. Narrate: `python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/source-generators`. Listen to the
   identifiers of the `"spoken"` map of `storyboard.json` first (`scripts/pronounce.py`).
3. Assemble: `python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/source-generators`.

There are no clips to record, so there is no order to follow and no snapshot to restore.
