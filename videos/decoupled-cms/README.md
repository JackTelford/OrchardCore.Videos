# Decoupled CMS: Razor Pages With Orchard Core Content

**YouTube:** not published yet · **Length:** 6:30 · **Feature:** `guide:guides/decoupled-cms` ([Creating a decoupled CMS website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/))

A developer video about the decoupled model: Orchard Core runs the administration and stores the content, and the Razor Pages of the same web
application render it. It adds Orchard Core to a Razor Pages project, sets the site up with the Blank site recipe, models a Blog Post content type
with the Title and MarkdownBody parts, and loads the content item in a page by its identifier and by an alias. It ends with the Preview part, which
points the preview of the editors at the Razor Page. The code is on slides; the browser is recorded for the Setup screen and the administration. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **throwaway Razor Pages host** (`src/OrchardSite` of an Orchard Core checkout, not the Orchard Core site `OrchardCore.Cms.Web`),
set up on camera, with fake data only. It needs no mask script. [recording-plan.md](recording-plan.md) has every detail: the project file, `Program.cs`, the
two versions of `Pages/BlogPost.cshtml`, the exact actions of every step, and the checks for private data. [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) lists what
a person has to confirm on a running site.

1. Build the host project of the plan, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on `http://localhost:5010`. There is
   no Auto Setup: the Setup screen is the first clip. Copy the empty folder aside as snapshot S0.
2. Record `03-setup` without a `state.json` (user `admin`, email `admin@example.com`, the password of `VIDEO_PASSWORD`, site name `Orchard Notes`, time zone
   `America/Los_Angeles`, recipe Blank site). Then sign the recorder in and save the session:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/decoupled-cms/recorder
   export VIDEO_ROOT=<repo>/videos/decoupled-cms VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   No `Recorder prepare` is needed: the clips create the content type and the blog post themselves. Stop the site and copy its `App_Data` folder aside (S1).
3. Narrate with `scripts/narrate.py videos/decoupled-cms`, then record the clips in this order, restoring the snapshots named in the plan between takes, and replacing
   `Pages/BlogPost.cshtml` between groups (the plan has both versions):
   `04-type` and `05-post`; then `06-page` (version B of the page, which loads the content item by its identifier); then `07-alias-part`; then `07-alias-page` and
   `07-alias-pattern` (version C, which loads it by its alias); then `08-preview`.
   `Recorder record all` is not used, because the page and the snapshots change between clips.
4. The slides are drawn by `slides.py`; the opening, chapter and closing cards by the assembly.
