# Create a CMS Web Application With dotnet new

**YouTube:** not published yet · **Length:** 6:01 · **Feature:** `guide:getting-started` ([Getting started with Orchard Core as a NuGet package](https://docs.orchardcore.net/en/latest/getting-started/))

A developer video. It installs the Orchard Core project templates, creates a CMS web application with the `occms` template,
and walks through the files the template writes: the project file, `Program.cs`, the app settings, the NLog configuration and
the launch settings. Then it runs the project, completes the Setup screen with the Blog recipe, signs in to the admin, and shows
where the logs and the data of the site are kept. The documentation pages it backs are
[Code Generation Templates](https://docs.orchardcore.net/en/latest/getting-started/templates/),
[Creating an Orchard Core CMS website](https://docs.orchardcore.net/en/latest/guides/create-cms-application/) and
[Getting started with Orchard Core as a NuGet package](https://docs.orchardcore.net/en/latest/getting-started/). The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

Only two clips are recordings (`05-setup` and `06-admin`). The commands, the files and the console output are slides drawn by
`slides.py`, because the recorder captures a browser, not a terminal. `recording-plan.md` has every action of every step; this
is the outline.

The video is recorded on a **fresh site made by the template** with fake data only, so it needs no mask script.

1. Install the templates (`dotnet new install OrchardCore.ProjectTemplates`), create the project in an empty folder outside
   this repository (`dotnet new occms -n MySite`), restore and build it, and run it on `http://localhost:5010` with
   `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories. It is **not** provisioned by Auto Setup: the Setup
   screen is recorded.
2. Write the recorder's scenes from `recording-plan.md` into `recorder/Scenes.cs` of this folder (two clip methods, `05-setup`
   and `06-admin`; the content of the video does not include that file), build the recorder with them, and set `VIDEO_ROOT`,
   `VIDEO_BASE_URL=http://localhost:5010` and `VIDEO_PASSWORD` (a password chosen outside git):

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/create-cms-web-application/recorder
   export VIDEO_ROOT=<repo>/videos/create-cms-web-application VIDEO_BASE_URL=http://localhost:5010 VIDEO_PASSWORD=<password>
   ```

   Do not run `Recorder setup`: both clips start signed out, and the site does not exist before `05-setup`.
3. Narrate (`scripts/narrate.py videos/create-cms-web-application`), then record `05-setup` and `06-admin`, in that order:
   `06-admin` needs the site that `05-setup` creates. `05-setup` changes the site, so before another take stop the site,
   empty the data folder and start the site again. Copy the data folder aside after `05-setup`, and restore that copy before
   another take of `06-admin`.
4. Assemble, and check the video for private data (OCR) as `recording-plan.md` lists.
