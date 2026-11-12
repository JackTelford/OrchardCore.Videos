# Notifications: Keep Users Informed in the Admin, by Email and SMS

**YouTube:** not published yet · **Length:** 4:58 · **Feature:** [OrchardCore.Notifications](https://docs.orchardcore.net/en/latest/reference/modules/Notifications/) (a feature already in Orchard Core, with Email Notifications and SMS Notifications)

The video shows how a site tells its users what happens. It enables the three Notifications features, sends notifications
with a workflow, reads them from the bell and the notification center, and lets a user choose between email and SMS
on their profile. Two slide chapters explain how notifications reach users and what each method sends and needs.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan in this
folder holds every action, selector and label of every step; this section only gives the order.

1. Run the Cms.Web project of Orchard Core in the Development environment (the Log SMS provider exists only there), with an
   empty application data folder outside both repositories, on port 5010, provisioned by Auto Setup: the Blog recipe,
   site name **Orchard Core**, time zone America/Los_Angeles, the Sqlite database provider, user admin with the email admin@example.com and a
   password chosen outside git. Block outbound calls with a dead proxy address.
2. Put the site in its starting state, as section 1.2 of the recording plan describes: the Workflows, Email, SMTP Email
   Provider and SMS features enabled, email sent to a pickup folder, SMS written to the log, a phone number for admin, a
   second user, and one draft page. The Notifications features stay disabled. Sign the recorder in, stop the site, and copy
   the application data aside as the first snapshot.
3. Narrate the video, then record the clips in this order: the clip that enables the features; then the seed of section 1.3
   (two workflows and two pages, one of them unpublished again, which creates three unread notifications) and a second snapshot; then the workflow clip,
   which publishes the draft page and creates the fourth notification; then a third snapshot.
4. Record the bell, the notification center and the preferences clips, each from the third snapshot. The center and the
   preferences clips change the site, so restore the third snapshot before each of them.
5. Record within about an hour of the seed, so that the notification times read as minutes.
