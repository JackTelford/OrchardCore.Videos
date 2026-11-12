# Field Editors and Display Modes: One Field, Many Faces

**YouTube:** not published yet · **Length:** 4:57 · **Feature:** [OrchardCore.ContentFields](https://docs.orchardcore.net/en/latest/reference/modules/ContentFields/)

A content field stores one kind of value, and two settings of the field decide how that value looks: the editor, which
changes how it is entered on the edit screen, and the display mode, which changes how it is shown on the site. The video
sets an editor and a display mode on the fields of a content type named Workshop: a Numeric Field as a slider, a Text Field
as a list of radio buttons through the Predefined List editor, and a Text Field as a heading through the Header display
mode. It ends with the editors and display modes that each field of the Content Fields module offers. The title,
description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script.

1. Run the Orchard Core web project of an OrchardCore checkout with an empty data folder outside both repositories, on
   port 5010, provisioned by Auto Setup with the Blog recipe, the site name Orchard Core, the time zone
   America/Los_Angeles, and the user admin with the email admin@example.com. The password is chosen outside git and passed
   to the recorder; it is not written anywhere in this repository. The Blog recipe already turns on every feature the video
   needs.
2. Seed the site outside the capture, as the recording plan describes: paste the recipe from the plan into Tools,
   Deployments, JSON Import. It creates the Workshop content type, with the fields Headline, Seats and Skill Level, and one
   published Workshop named Spring Pottery Evening. Then stop the site and copy its data folder aside, together with the
   recorder's sign-in file, as the seeded snapshot.
3. Narrate first, then record. The video has three recordings: the editor of Seats, the editor of Skill Level, and the
   display mode of Headline. They do not depend on each other, but each one changes the site, so restore the seeded
   snapshot before every take. The recording plan gives the order, the exact actions of each step, and what must be on
   screen when its narration ends.

The two groups of slides, the opening card and the closing card need no site.
