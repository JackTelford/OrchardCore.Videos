# Indexing in Code: Index Your Own Data and Fields

**YouTube:** not published yet · **Length:** 5:52 · **Feature:** `OrchardCore.Indexing` ([Indexing](https://docs.orchardcore.net/en/latest/reference/modules/Indexing/))

A video for module authors. It follows a change from the task log to the index, then writes a part handler, a field
handler, and a document index handler for records of your own. Two short recordings show the Lucene index settings of a
field and the Reset action of an index, and the last chapter lists the pieces of a source of your own: tasks, an
indexing service, a profile handler, their registrations, and what the provider reads from the profile. The title, description, tags and chapters YouTube shows are in `video.json`; the code is
on slides, drawn by `slides.py`.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. Only chapter 4 is recorded in
a browser; every other chapter is slides. `recording-plan.md` has every action, selector and source reference.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**,
   time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`).
2. Enable the **Lucene** feature, and create the index `blogposts` (Content in Lucene, content type **Blog Post**), as
   `recording-plan.md` describes. The Blog recipe already provides the **Blog Post** type, its **Subtitle** field and one
   sample post, so no other data is created.
3. Write `recorder/Scenes.cs` from `recording-plan.md` (one method for each of the clips `04-settings` and `04-reset`),
   and sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/indexing-developer/recorder
   export VIDEO_ROOT=<repo>/videos/indexing-developer VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` (the Lucene index is in it) and `state.json` aside. Restore both before every
   full take.
4. Narrate and record: `scripts/narrate.py videos/indexing-developer`, then `Recorder record 04-settings 04-reset`, in
   that order: `04-settings` saves the Lucene settings of the Subtitle field, and `04-reset` needs them. Both clips change
   the site, so restore the copy before another take.
