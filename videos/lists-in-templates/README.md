# Lists in Templates: Liquid Filters for Containers and Their Items

**YouTube:** not published yet · **Length:** 3:59 · **Feature:** `OrchardCore.Lists` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Lists/))

For developers of themes and templates. The video shows how a list is stored (the `ContainedPart` of each item), how the
`ListPart` shape renders one page of items, and how the three Liquid filters of the Lists module, `container`,
`list_items` and `list_count`, read a list from any template. It builds one template for blog posts that uses all
three, shows the result on a blog post page, and ends with the same queries in a Razor view. The title, description,
tags and chapters YouTube shows are in `video.json`.

Most of the video is cards and slides. Only one clip, `05-site`, is recorded in a browser.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. `recording-plan.md` holds the
exact seed data, the text of the template and, for every step, what to do and what must be on screen.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`; started with
   `dotnet run --project src/OrchardCore.Cms.Web --no-launch-profile --urls http://localhost:5010`). The Blog recipe
   enables the Lists and Templates features, so nothing else is enabled.
2. Seed the site outside the capture, as `recording-plan.md` describes: delete the sample post of the recipe, create
   three published posts and one draft in the blog, and create the template `Content__BlogPost` under **Design**,
   **Templates**. Check the post page, copy the `App_Data` folder aside, and keep it as the snapshot to restore
   before another take.
3. Sign the recorder in and build it with the scenes of the video:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/lists-in-templates/recorder
   export VIDEO_ROOT=<repo>/videos/lists-in-templates VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

4. Narrate and record: `scripts/narrate.py videos/lists-in-templates`, then `Recorder record 05-site`. The clip only
   reads the site, so it can be recorded again without restoring the snapshot. The cards and the slides need no site.
