# Shape Alternates: Customize the Display of One Type or One Item

**YouTube:** not published yet · **Length:** 5:22 · **Feature:** [`topic:DisplayManagement`](https://docs.orchardcore.net/en/latest/reference/modules/DisplayManagement/)

A video for developers of themes and modules. It explains how Orchard Core chooses the template of a shape, what an
alternate is, and which alternates a content item receives: its content type, its path and its alias. It adds two
template files to a theme, one for every event and one for a single event, reads the shape debug information to see which
template won, and ends with the ways to add alternates from a template or from code. The title, description, tags and
chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact steps, the seed data,
the two template files and the actions of every clip are in [recording-plan.md](recording-plan.md); questions that a person
must confirm on a running site are in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the `Blog` recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`).
2. Seed the site: import the `Event` content type and its three events with **Tools** > **Deployments** > **JSON Import**
   (the recipe is in the recording plan). Stop the site and copy its `App_Data` folder aside: this is the snapshot S0.
3. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/display-alternates/recorder
   export VIDEO_ROOT=<repo>/videos/display-alternates VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

4. Narrate: `python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/display-alternates`.
5. Record the four clips in this order, adding a template file to the theme between them, as the recording plan says (start the
   site again with `dotnet run` after each change, because a build lists the template files of a theme):
   `03-before` (no template file), `03-after` (`Content-Event.liquid`), `04-after` (and
   `Content-Slug-events-night-market.liquid`), `05-debug` (both files; this clip saves the debugging setting, so it is
   the last one). To record from the start again, remove both files and restore S0.
