# Headless: Serve Content to Any Front End

**YouTube:** not published yet · **Length:** 5:54 · **Feature:** `OrchardCore.Apis.GraphQL` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Apis.GraphQL/))

Orchard Core can be the content back end of a front end that lives in its own application. The video explains the
pattern, then connects three modules: GraphQL serves the content from one endpoint, CORS lets a browser app on another
origin call it, and the OpenID Connect Authorization Server and Token Validation features issue and accept access
tokens. It ends with the same setup written as a recipe. The title, description, tags and chapters YouTube shows are in
`video.json`.

The video is for developers. It has three recorded clips (GraphQL, CORS, OpenID Connect) and slides for the idea, the
HTTP request, the preflight test, the authorization code flow and the recipe steps. Each module also has a video of its
own.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. `recording-plan.md` holds
every action of every clip, the selectors, the labels and where each one comes from in the source.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder outside
   both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard
   Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). Enable none of the GraphQL, CORS
   Configuration or OpenID Connect features: the clips enable them on camera.
2. Start the stand-in front end of `recording-plan.md` on `http://127.0.0.1:5020`: one static page, served by Python
   from a folder outside the repository. It is the "front end on its own address" that the CORS clip shows.
3. Sign the recorder in and seed the site:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/headless-front-end/recorder
   export VIDEO_ROOT=<repo>/videos/headless-front-end VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Then import the two fake blog posts of `recording-plan.md` with **Tools > Deployments > JSON Import**. Stop the site
   and copy its `App_Data` aside as snapshot S0.
4. Narrate and record: `scripts/narrate.py videos/headless-front-end`, then `Recorder record all`. The clips depend on
   each other and are recorded in this order: `02-graphql`, `03-cors`, `04-openid`. Each one changes the site, so copy
   `App_Data` aside after `02-graphql` (S1) and after `03-cors` (S2), and restore the snapshot a clip starts from
   before another take. `03-cors` also needs the front end server running.
