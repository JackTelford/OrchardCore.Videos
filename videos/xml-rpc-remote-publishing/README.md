# XML-RPC: Publish From Desktop Blogging Clients

**YouTube:** not published yet · **Length:** 3:55 · **Feature:** `OrchardCore.XmlRpc` and `OrchardCore.RemotePublishing` ([XML-RPC documentation](https://docs.orchardcore.net/en/latest/reference/modules/XmlRpc/))

A developer video about the XML-RPC module and its Remote Publishing feature, which let a desktop blogging client such
as Open Live Writer publish posts to an Orchard Core site. It shows the two features, how to enable them, how a client
finds a blog through the manifest and the RSD document, the calls a client can make and the permissions they need, a
post that arrives from a client, and how a module adds its own drivers and handlers. The title, description, tags and
chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. `recording-plan.md` holds
every action of every clip, the exact fake data, and the checks for private data.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`, email `admin@example.com`). Do not enable `XML-RPC`
   or `Remote Publishing` before recording: the first clip enables them. Copy `App_Data` aside as snapshot A.
2. Sign the recorder in, outside the capture:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/xml-rpc-remote-publishing/recorder
   export VIDEO_ROOT=<repo>/videos/xml-rpc-remote-publishing VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

3. Narrate: `scripts/narrate.py videos/xml-rpc-remote-publishing`.
4. Record the clips in this order, copying `App_Data` aside after the clips that change the site:
   1. `02-enable` enables the two features. Copy `App_Data` aside as snapshot B.
   2. `03-discover` only reads. It needs snapshot B.
   3. Run the stand-in for the blogging client (a short script in `recording-plan.md`): it sends a draft and a
      published post through `/xmlrpc`. Copy `App_Data` aside as snapshot C.
   4. `05-post` only reads. It needs snapshot C.

   The slide chapters need no recording. To record a clip again, restore the snapshot it starts from.
