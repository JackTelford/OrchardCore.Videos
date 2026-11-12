# Content Fields: Text, Numbers, Dates and Yes or No

**YouTube:** not published yet · **Length:** 6:31 · **Feature:** [OrchardCore.ContentFields](https://docs.orchardcore.net/en/latest/reference/modules/ContentFields/)

This video shows how a content type keeps its data in fields. It follows one example, a content type named Workshop: you
find the Content Fields feature, add a Text Field, a Numeric Field, a Boolean Field, a Date Field, a Date Time Field and a
Time Field with their settings, fill in a workshop, see a validation message, and look at the values on the site. The
title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan in this folder
holds every click, every value to type, and the order of the clips.

1. Build the Orchard Core web project and run it with its data folder in an empty folder outside both repositories, on port
   5010, provisioned by Auto Setup: recipe **Blog**, site name **Orchard Core**, time zone America/Los_Angeles, user **admin**,
   email admin@example.com. The password is chosen outside git. Keep the **User Time Zone** feature disabled.
2. Sign the recorder in, outside the capture, as the plan describes. The sign-in file stays out of git.
3. Seed the site by hand, as the plan says: create the content type **Workshop** with the **Title** part, and nothing else.
   Stop the site and copy its data folder aside as the first snapshot.
4. Narrate, then record the clips in this order: open the type, Text Field, Numeric Field, Boolean Field, dates and times, fill
   in a workshop, the site. The clips depend on each other: each one adds to the site, first the fields, then one published
   workshop. Copy the data folder aside after each clip; to record one clip again, restore the copy the plan names for it.
5. The open questions file lists what could not be checked without a running site. Read it before the first take.
