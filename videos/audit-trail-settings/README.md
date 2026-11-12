# Audit Trail Settings: Events, Retention and User Events

**YouTube:** not published yet · **Length:** 4:18 · **Feature:** [OrchardCore.AuditTrail](https://docs.orchardcore.net/en/latest/reference/modules/AuditTrail/) and [OrchardCore.Users.AuditTrail](https://docs.orchardcore.net/en/latest/reference/modules/Users/#audit-trail-integration)

This video shows the settings of the Audit Trail module: which events are recorded and for which content types, whether
the client IP address is stored, how long events are kept before the trimming task deletes them, and which properties of
a user are stored with a user event and how they are redacted. It ends with the Audit Trail list, to show what the
settings produce. The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. Everything a person needs, step
by step, is in recording-plan.md. In short:

1. Run the Orchard Core web project on http://localhost:5010 with an empty data folder outside both repositories,
   provisioned by Auto Setup: the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, and the
   user admin with the email admin@example.com. The password is chosen outside git and passed to the recorder in an
   environment variable.
2. In Tools, Features, enable Audit Trail and Users Audit Trail. Leave Users Registration, Users Reset Password and
   Custom User Settings off, so the User category shows only the base events. Sign the recorder in, then copy the site's
   data folder and the sign-in aside: this is the starting snapshot.
3. Record the clips in the order of the plan: the events and content types clip first. Copy the data aside again, then
   record the retention clip and the user events clip, each from that second copy. The clips that change the site are
   marked in the plan; restore the matching copy before each take.
4. Create the seed data of the plan (one failed sign-in with an invented user name, and one published Page), copy the data
   aside, and record the result clip from that copy.
5. Narrate before recording: the recorder holds every step for the length of its narration. The slides and the cards
   are drawn when the video is assembled, and nothing is recorded for them.

The questions a person must answer on the running site before the first take are in OPEN-QUESTIONS.md.
