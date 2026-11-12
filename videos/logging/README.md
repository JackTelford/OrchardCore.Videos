# Logging: Configure NLog and Serilog

**YouTube:** not published yet · **Length:** 5:03 · **Feature:** `OrchardCore.Logging.NLog` ([documentation](https://docs.orchardcore.net/en/latest/getting-started/templates/)) and `OrchardCore.Logging.Serilog` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Logging.Serilog/))

A video for developers about the two logging libraries of Orchard Core. It explains why the tenant name belongs in a log,
how the Orchard Core CMS project template chooses a logger (`nlog`, `serilog` or `none`), and how each library is
configured: NLog with one host call, `NLog.config` and the `Logging` section, and Serilog with the host call, the tenant
name middleware and the `Serilog` section. It ends with what differs between the two and what does not. The title,
description, tags and chapters YouTube shows are in `video.json`.

The video is made only of slides and cards: the two libraries have no admin screen, and code, configuration files and
command lines are shown on slides, so nothing is captured from a browser.

## Recording it again

There is no clip to record: no site, no browser, no recorder and no sign-in. The `recording-plan.md` lists the slide each
step plays over and the source it comes from.

1. Install the tools: `pip install pillow pygments imageio-ffmpeg "edge-tts>=7.3.1"`.
2. Confirm the spellings of the `spoken` map of `storyboard.json` with `scripts/pronounce.py` (the candidates are in
   `OPEN-QUESTIONS.md`).
3. Narrate and assemble, from the root of the repository:

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/logging
   python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/logging
   python tools/youtube/youtube.py chapters logging --build videos/logging/build
   ```

4. Look at a few frames of each chapter, and check that no code panel is cut at its right edge or at the bottom.

If the narration of a step changes, run `narrate.py` again: only the changed steps are generated again.
