# Module Metadata in Your Project File

**YouTube:** not published yet · **Length:** 3:42 · **Feature:** `guide:guides/leverage-csproj-meta-info` ([Leveraging your CSPROJ meta information](https://docs.orchardcore.net/en/latest/guides/leverage-csproj-meta-info/))

The video shows how a module, its features, or a theme can describe itself to Orchard Core from its project file instead of from `Manifest.cs`. It declares a module with an `OrchardCoreModules` item and its metadata, then features, themes and the `OrchardCoreAttributes` list, and covers the rules the build checks, the `OrchardCoreDebugTargets` property, the properties that order the embedding, and where the values show in the admin. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video has **no recording**: every chapter between the opening and closing cards is a slide, drawn by `slides.py`, so it needs no Orchard Core site, no sign-in, no seed data and no recorder. `recording-plan.md` describes each slide.

1. Install the dependencies of the skill (`pip install pillow pygments imageio-ffmpeg "edge-tts>=7.3.1"`).
2. Narrate and assemble from the root of the repository:

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/csproj-module-metadata
   python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/csproj-module-metadata
   ```

3. Look at one slide of each chapter and at the opening and closing cards, then run the check for private data of the skill.
