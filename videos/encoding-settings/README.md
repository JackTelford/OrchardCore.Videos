# Encoding Settings: Render Non-ASCII Text Without Encoding

**YouTube:** not published yet · **Length:** 3:17 · **Feature:** `guide:guides/encoding-settings` ([Customizing encoding settings](https://docs.orchardcore.net/en/latest/guides/encoding-settings/))

A developer video about the encoders of ASP.NET Core in an Orchard Core application. By default the HTML, URL and
JavaScript encoders leave only the Basic Latin range as it is and encode every other character, so accented and
non-Latin text reaches the page source as character codes. The video shows how to allow the Unicode ranges you use with
`WebEncoderOptions` in the `Program.cs` file of the web application, why every tenant shares the setting, and how to
take the encoders from dependency injection in your own code so that they follow it. The title, description, tags and
chapters YouTube shows are in `video.json`.

## Recording it again

The video has **no recordings**: its chapters are slides, drawn by `slides.py`, and the opening and closing cards. It
needs no Orchard Core site, no Auto Setup, no seed data, no sign-in (`state.json`) and no recorder run. The details are in
`recording-plan.md`, and what a person must still confirm is in `OPEN-QUESTIONS.md`.

1. Install the tools of the skill: `pip install pillow pygments imageio-ffmpeg "edge-tts>=7.3.1"`.
2. Narrate, after a person has confirmed the spellings of the `spoken` map in `storyboard.json`:
   `python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/encoding-settings`.
3. Assemble: `python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/encoding-settings`. It draws
   the slides, so look at one slide of each chapter and at the two cards before publishing.
4. Check the frames for private data as the skill describes. The only invented text is the title `Café Münster`.
