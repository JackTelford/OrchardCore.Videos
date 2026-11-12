# Development Tools: Preview Packages and Local NuGet Feeds

**YouTube:** not published yet · **Length:** 4:29 · **Feature:** `guide:getting-started/development-tools` ([documentation](https://docs.orchardcore.net/en/latest/getting-started/development-tools/))

A video for developers who build sites, modules and themes with Orchard Core. It covers the editors and utilities the
documentation suggests, the three ways to add the preview package feed (Visual Studio, a NuGet.config file and the .NET
CLI), and how to pack a fork or a branch of the source code into a local NuGet feed. The title, description, tags and
chapters YouTube shows are in `video.json`. It backs the documentation pages
[Development Tools](https://docs.orchardcore.net/en/latest/getting-started/development-tools/),
[Add preview package source](https://docs.orchardcore.net/en/latest/getting-started/preview-package-source/) and
[Using a local copy of Orchard Core source code as nuget packages](https://docs.orchardcore.net/en/latest/topics/local-nuget-packages/).

## Recording it again

This video has **no recording**: its 19 steps are two cards and 17 slides, and the slides, which show commands, a
NuGet.config file and Visual Studio's steps, are drawn from `slides.py`. It needs no site, no Auto Setup, no
sign-in, no seed data and no recorder scenes, and nothing in it comes from a real machine: the slides show
placeholders such as `\\{YourServer}\NuGetServer`, `c:\OrchardCoreNuget` and `YOUR_VERSION`.

1. Check that the slides draw (Pillow and Pygments installed, the skill's `scripts` folder on `PYTHONPATH`): from this
   folder, call `slides.slide(step_id, chapter_title, label)` for each slide step of `storyboard.json`.
2. Before narrating, listen to the respellings of the `"spoken"` map of `storyboard.json` with
   `scripts/pronounce.py`, and have a person confirm them (the list is in `OPEN-QUESTIONS.md`).
3. Generate the voice and assemble, from the root of the repository:

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/development-tools
   python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/development-tools
   ```

4. Look at one frame of each slide: no text cut off, nothing overlapping.

`recording-plan.md` lists every slide with the source of what it shows, and the checks for private data.
`OPEN-QUESTIONS.md` lists what a person must confirm on a machine with Visual Studio, a clone of the source code and
network access to the preview feed.
