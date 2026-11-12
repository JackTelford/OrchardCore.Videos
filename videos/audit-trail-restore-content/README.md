# Audit Trail: Compare and Restore Content Versions

**YouTube:** not published yet · **Length:** 4:28 · **Feature:** `OrchardCore.AuditTrail` ([Audit Trail documentation](https://docs.orchardcore.net/en/latest/reference/modules/AuditTrail/))

The Audit Trail keeps a copy of a content item with each recorded event. The video explains what is kept and what limits it,
shows how to choose the content types to record, and follows one article through its history: finding its events, opening an
earlier version, comparing two versions on the Diff tab, and restoring an earlier version as a draft. It ends with a deleted
article that is restored from its Removed event. The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Its audience is the people who manage
content, so it shows the admin screens only.

1. **The site.** Build src/OrchardCore.Cms.Web of Orchard Core and run it with its own empty application data folder outside both
   repositories, on http://localhost:5010, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone
   America/Los_Angeles, user admin with the email admin@example.com. The password is chosen outside git and passed to the
   recorder in VIDEO_PASSWORD. The browser of the recorder needs internet access, because the Diff tab loads a script from a
   public content delivery network.
2. **The feature.** Enable **Audit Trail** in Tools, Features. Leave every Audit Trail setting as it is.
3. **The order.** Copy the site's data aside (snapshot S0). Record the clip 02-record, which allows the Article content type.
   Then create the fake history described in recording-plan.md: two articles, Summer Parking Notice and Harvest Festival
   Schedule, the second one edited and published three times and renamed Harvest Festival Cancelled. Copy the data aside again
   (snapshot S1).
4. **The clips.** From S1, record 03-history, 04-view and 05-compare, which change nothing. Record 06-restore and 07-deleted
   last: each one changes the site, so restore S1 before each further take of either.

recording-plan.md holds the exact actions, text and checks of every clip, and OPEN-QUESTIONS.md lists what a person has to
confirm on a running site before the first take.
