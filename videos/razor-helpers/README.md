# Razor Helpers and Tag Helpers for Orchard Core Themes

**YouTube:** not published yet · **Length:** 5:16 · **Feature:** `topic:Razor` ([Razor helpers](https://docs.orchardcore.net/en/latest/reference/modules/Razor/))

The video is for theme and module authors who write Razor views. It shows the Orchard property of a view and the injected `IOrchardHelper`, the helpers that load and display content items, the helpers for culture, URLs, text and data, and the tag helpers that render shapes, menus, resources and media, or add content to the zones of a layout. The code on the slides comes from the Orchard Core source and from the documentation. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video has **no browser recording**: after the opening card, every step is a slide drawn by `slides.py`, and the closing card is drawn by `assemble.py`. It needs no site, no Auto Setup, no seed data, no sign-in and no features to enable, and its folder has no `recorder/` folder.

1. Install the prerequisites of the skill: `pip install pillow pygments imageio-ffmpeg "edge-tts>=7.3.1"`.
2. Check how the voice says the identifiers and attribute names of the narration (the list is in `OPEN-QUESTIONS.md`) with `scripts/pronounce.py`, and fix the `spoken` map of `storyboard.json` if one is wrong.
3. Narrate and assemble:

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/razor-helpers
   python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/razor-helpers
   ```

4. Look at a frame of every slide, then do the checks for private data at the end of `recording-plan.md`.

No clip depends on another one, and no clip changes a site, so there is no snapshot to restore and no order to respect beyond the storyboard's.
