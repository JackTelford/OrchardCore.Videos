# Content Items: Create, Edit and Publish

**YouTube:** not published yet · **Length:** 4:49 (722 words at 150 words per minute) · **Feature:** `OrchardCore.Contents` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Contents/))

This video is for editors and site owners. It explains what a content item and a content type are, then follows one
item through its life in the admin UI: finding the Content Items page, creating an Article and publishing it, viewing
the published page, changing it and publishing the change, and taking it off the site with Unpublish and Delete. The
title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. `recording-plan.md` is the
script for the person who records: the exact actions of every step, what must be on screen when its narration ends, and
the source line of every label.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup: the **Blog** recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, and a password chosen outside git
   (`VIDEO_PASSWORD`). Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address, so the site never calls out.
2. Enable no other feature and change no setting: the clips use the screens of the Blog recipe as they are. There is
   nothing to seed; the clips type their own fake values (an Article titled "Spring Garden Checklist").
3. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/contents-create-edit-publish/recorder
   export VIDEO_ROOT=<repo>/videos/contents-create-edit-publish VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside as snapshot **S0** (before the first clip).
4. Narrate and record: `scripts/narrate.py videos/contents-create-edit-publish`, then `Recorder record 02-list`,
   `03-create`, `04-view`, `05-edit` and `06-remove`, in that order. The clips depend on each other: `03-create`
   creates the Article that `04-view`, `05-edit` and `06-remove` use. `03-create`, `05-edit` and `06-remove` change the
   site, so copy `App_Data` aside after `03-create` (**S1**) and after `05-edit` (**S2**), and restore the snapshot named
   in the table of `recording-plan.md` before a new take. `02-list` and `04-view` leave the site as they found it.
