# Create a Theme From an HTML Template

**YouTube:** not published yet · **Length:** 7:06 · **Feature:** `guide:topics/display/theme-from-html-template` ([Create an Orchard Core theme from an HTML template](https://docs.orchardcore.net/en/latest/topics/display/theme-from-html-template/))

A static HTML template is a finished design. This video turns one into an Orchard Core theme, the way the built-in
TheBlogTheme was made from the Clean Blog template: the assets go to `wwwroot`, the stylesheets and scripts become named
resources, the main page becomes a Liquid layout with zones, the inner pages become content templates, and an optional
setup recipe makes the theme work out of the box. The code is on slides; two recordings show the zones in the admin and
the result on a site. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. Only two of its chapters are
recordings (`05-zones` and `08-result`); the others are cards and slides. [recording-plan.md](recording-plan.md) has every
click, wait and expected screen, and [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) lists what a person confirms on the running site.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup with the **Blog** recipe (site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). The Blog recipe enables
   every feature the clips need (Layers, Widgets, Menu, Themes, Templates) and sets **TheBlogTheme** as the site theme,
   so there is nothing to enable and nothing to seed by hand.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/theme-from-html-template/recorder
   export VIDEO_ROOT=<repo>/videos/theme-from-html-template VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside: restore that copy if a button was pressed by mistake.
3. Narrate and record: `scripts/narrate.py videos/theme-from-html-template`, then `Recorder record 05-zones 08-result`.
   The two clips only open pages and highlight elements, so neither depends on the other, neither changes the site,
   and they can be recorded in either order.
