# Orchard Core Videos

The source of every narrated Orchard Core video: the storyboards, slides, recorder scenes and YouTube metadata, and the
skill that turns them into a video with one look and one voice.

The videos themselves are published on the [Orchard Core YouTube channel](https://www.youtube.com/@OrchardCore) and
embedded from there in the [documentation](https://docs.orchardcore.net/). **No video file is ever committed**, here or
in the Orchard Core repository, so cloning a repository never downloads videos. Everything kept here is what's needed
to make a video again, or to change how it appears on YouTube.

> **First time here?** Follow [SETUP.md](SETUP.md): the one-time configuration of Google, YouTube and GitHub that lets
> the workflows upload videos and update the documentation.

## Videos

<!-- videos:start -->
| Video | Length | YouTube | Source |
| --- | --- | --- | --- |
<!-- videos:end -->

This table is generated from each video's `video.json` by `python tools/youtube/youtube.py index`.

## What's here

| Path | What it is |
| --- | --- |
| `videos/<slug>/` | One video: `video.json` (YouTube title, description, tags, chapters, links, id), `storyboard.json` (narration), `slides.py`, `recorder/Scenes*.cs`, `durations.json`, captions, and a README on how to record it again |
| `videos/*-walkthroughs/` | Reels: existing silent screencasts joined into one video per area, built by `scripts/reel.py` from `reel.json` |
| `.agents/skills/orchardcore-video-producer/` | The skill: the video specification, the narration, recording and assembly scripts, the shared recorder, a worked example (`examples/secrets-module/`) and a template for a video's folder (`templates/video/`) |
| `branding/` | The Orchard Core logo and the Open Sans font every video uses |
| `tools/youtube/` | Uploads a video, pushes `video.json` changes, computes chapters, fills in the docs embeds, and writes the table above |
| `.github/workflows/publish-video.yml` | Publishing a release uploads its video and opens the docs pull request |
| `.github/workflows/youtube-metadata.yml` | Pushes a changed `video.json` to YouTube when it is merged |
| `SETUP.md` | The one-time setup, step by step: Google Cloud, the YouTube API, the repository secrets, and the docs token |
| `CONTRIBUTING.md` | How to submit a video, and how it is published |

## Making a video

**To submit a video, read [CONTRIBUTING.md](CONTRIBUTING.md)**: what a pull request holds, how a video is published
to YouTube, and how the documentation pull request follows.

Follow `.agents/skills/orchardcore-video-producer/SKILL.md`. In short:

1. Copy `.agents/skills/orchardcore-video-producer/templates/video/` to `videos/<slug>/`, and write the storyboard
   (start from `examples/secrets-module/`) and `video.json`.
2. Narrate (`scripts/narrate.py`), record the clips against a local Orchard Core site with fake data (the recorder),
   and assemble (`scripts/assemble.py`); then `youtube.py chapters`.
3. Check the video for private data, and merge its folder (never the MP4) to `main`.
4. `python tools/youtube/youtube.py release <slug> --file <mp4>` creates a draft release with the MP4 attached.
   Watch it there, then **publish the release**: the **Publish video** workflow uploads it to YouTube with its
   metadata, captions, thumbnail and playlists, records the YouTube id here, and opens a pull request in
   OrchardCMS/OrchardCore that fills in the `<!-- video:<slug> -->` markers of the docs pages.

A video's versions are its releases (`<slug>/v1`, `<slug>/v2`, ...): a re-recording is a new release, and the previous
version on YouTube becomes unlisted. Changing a published video's title, description or chapters is an edit to its
`video.json`; merging it updates YouTube. See [SETUP.md](SETUP.md) and `tools/youtube/README.md` for the secrets these
workflows need.

## Requirements

Python 3.11+ (`pip install pillow pygments imageio-ffmpeg "edge-tts>=7.3.1"`, and `tools/youtube/requirements.txt`
to publish), the .NET 10 SDK for the recorder, and internet access for the voice.
