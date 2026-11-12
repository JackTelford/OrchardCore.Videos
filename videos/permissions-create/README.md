# Permissions: Declare Them and Check Them in Your Module

**YouTube:** not published yet · **Length:** 4:46 · **Feature:** `OrchardCore.Roles` ([Roles documentation](https://docs.orchardcore.net/en/latest/reference/modules/Roles/))

A video for module authors. It explains what a permission is, then declares one with a name and a description,
lists it in an `IPermissionProvider`, registers the provider, shows how the roles editor displays a permission, and
checks the permission in a controller, a view and a menu item. The title, description, tags and chapters YouTube shows
are in `video.json`. The documentation pages it backs are [Roles](https://docs.orchardcore.net/en/latest/reference/modules/Roles/)
and [Permissions](https://docs.orchardcore.net/en/latest/reference/modules/Security/Permissions/).

## Recording it again

Only one clip is a browser recording: `04-roles`, the roles editor. The other chapters are slides, drawn from
`slides.py`. The full instructions, one entry per step, are in [recording-plan.md](recording-plan.md); the questions a
person must confirm on a running site are in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). No feature needs to be
   enabled: the Blog recipe enables the Roles module.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/permissions-create/recorder
   export VIDEO_ROOT=<repo>/videos/permissions-create VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Create the seed role by hand, outside the capture: a role named `Manager` that holds only the **Manage Roles**
   permission (the steps are in the recording plan). Stop the site and copy its `App_Data` aside: restore that copy
   before a full take. The clip does not change the site.
4. Narrate and record: `scripts/narrate.py videos/permissions-create`, then `Recorder record all`. There is a single
   clip, so there is no order to follow.
