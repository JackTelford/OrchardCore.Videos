# Content Fields: Links, Rich Text, Markdown and YouTube Videos

**YouTube:** not published yet · **Length:** 5:00 · **Feature:** OrchardCore.ContentFields ([Content Fields](https://docs.orchardcore.net/en/latest/reference/modules/ContentFields/)) and OrchardCore.Markdown ([Markdown](https://docs.orchardcore.net/en/latest/reference/modules/Markdown/))

The video adds a Link Field, an Html Field, a Markdown Field and a Youtube Field to a content type named Workshop, fills
them in, publishes the content item, and shows how each field is displayed on the site. It ends with what the Sanitize
Html setting does to Html and Markdown content. It is for site owners and editors: it shows screens and the site, and no
code. The title, description, tags and chapters that YouTube shows are kept in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact steps, the values to
type, the labels and where each one comes from in the Orchard Core source are in recording-plan.md; read
OPEN-QUESTIONS.md before the first take.

1. Build the Orchard Core web project of a checkout of OrchardCMS/OrchardCore and run it on port 5010 with its own empty
   App_Data folder outside both repositories, provisioned by Auto Setup: the Blog recipe, the site name **Orchard Core**, the
   time zone America/Los_Angeles, and the user **admin** (email admin@example.com). The password is chosen outside git and
   passed to the recorder; it is never written in the repository. The Blog recipe already enables Content Fields, Content
   Types, Contents, Features, Markdown and Title, so no feature is enabled by hand.
2. Sign the recorder in once, so the sign-in is not recorded.
3. Create the seed outside the capture: the content type **Workshop**, with the **Title** part and no fields. The plan lists
   the steps.
4. The Youtube Field needs a video address. The video uses an example.com address and a stand-in page for the player, set
   up in the plan, so no real video service is called.
5. Stop the site and copy its App_Data folder aside after the seed and after each clip. Restore the copy taken before a
   clip to record that clip again.
6. Narrate first, then record the clips in this order, each one starting from the state the previous one left: Link Field,
   Html Field, Markdown Field, Youtube Field, Fill in a Workshop, On the site. The first four add the fields to the
   Workshop type, the fifth creates the content item Pottery Basics, and the last one only reads. The two slide chapters
   and the two cards need no recording.
