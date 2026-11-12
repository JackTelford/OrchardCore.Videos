# OpenID Connect: Orchard Core as an Identity Provider

**YouTube:** not published yet · **Length:** 5:00 · **Feature:** OrchardCore.OpenId ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/OpenId/))

An overview of the OpenID Connect module for site owners and administrators. It explains what an identity provider does,
how an application signs people in through Orchard Core and receives tokens, and which features the module offers. It then
enables the OpenID Connect Authorization Server feature and shows where its settings, applications and scopes are in the admin.
The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only (the administrator that Auto Setup creates, and nothing else), so
it needs no mask script. recording-plan.md holds the exact actions and commands of every clip, and OPEN-QUESTIONS.md what a
person must confirm on a running site.

1. Run the Orchard Core web project on a fresh site, with its data folder outside both repositories, provisioned by Auto Setup:
   the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, administrator **admin** with the email
   admin@example.com, and a password chosen outside git. No OpenID Connect feature is enabled before the take.
2. Sign the recorder in. The video needs no other seed data. Stop the site and keep a copy of its data folder and of the
   sign-in as snapshot A.
3. Narrate first, then record the clips in this order. Record "03-features" from snapshot A. Restore snapshot A and record
   "04-enable", which enables the OpenID Connect Authorization Server feature and so changes the site. Keep a copy of the site
   as it is left by "04-enable" as snapshot B, and record "05-tour" from it. The slides need no recording.
