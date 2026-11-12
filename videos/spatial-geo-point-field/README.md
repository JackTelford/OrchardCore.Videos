# Geo Point Field: Add Locations to Content Items

**YouTube:** not published yet · **Length:** 3:47 · **Feature:** [OrchardCore.Spatial](https://docs.orchardcore.net/en/latest/reference/modules/Spatial/)

This video shows how to give a content item a place on the map with the Geo Point field of the Spatial feature. It
explains what the field stores, enables the feature, adds the field to the Page content type with its Required and
Hint settings and the Leaflet editor, sets a location by clicking the map and by typing a latitude and a longitude,
and checks the saved location in the editor and on the site. The title, description, tags and chapters that YouTube
shows are in the video's metadata file.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The recording plan in this
folder lists every click, every value to type, and what must be on screen when each narrated step ends.

1. Provision a fresh site with Auto Setup from the Blog recipe, with the site name Orchard Core, the time zone
   America/Los_Angeles and the user admin. Run it on port 5010 with its data folder outside both repositories. The
   password is chosen outside git and passed to the recorder in the environment, never written in a file.
2. Leave the Spatial and Lucene features disabled. The first recorded clip enables them. The site needs no seed data:
   the Page content type and the empty content list of the Blog recipe are the starting point, and every other value is
   typed during the clips.
3. The browser needs internet access: the map background of the Leaflet editor comes from OpenStreetMap, and there
   is no local stand-in. The site itself should not reach any outside service.
4. Sign the recorder in once, narrate the video, and record the four clips in this order: enabling the feature, adding
   the field, setting a location, and checking the result. The first three clips change the site: copy the data folder
   aside before the first clip and after each of the first three, and restore the copy that precedes a clip before
   recording that clip again.
5. Before assembling, confirm the points listed in the open questions of this folder on the running site.
