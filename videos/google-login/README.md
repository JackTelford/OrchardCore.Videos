# Google Sign-In: Let Users Sign In With Google

**YouTube:** not published yet · **Length:** 3:52 · **Feature:** `OrchardCore.Google.GoogleAuthentication` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Google/))

The video shows how people sign in to an Orchard Core site with their Google Account. It explains what Google provides and what happens when Google sends a person back, enables the Google Authentication feature, enters a Client ID and an API secret key under Settings, Security, Authentication, Google, and shows the Google button on the login page. It stops before the redirect to Google, because the credentials are fake. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact steps, selectors, values and checks are in [recording-plan.md](recording-plan.md); questions that a person must answer on a running site are in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Build the CMS web project of Orchard Core and run it on port 5010 with an empty App_Data folder outside both repositories, provisioned by Auto Setup: the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles, the user admin with the email admin@example.com, and a password chosen outside git. Block the site's outbound calls with a dead proxy address.
2. Leave every feature as the Blog recipe sets it. Google Authentication must be disabled at the start, and the Secrets feature must stay disabled, so the API secret key is a plain password box. Leave the User Login settings at their defaults.
3. Sign the recorder in as admin, outside the capture, and keep a copy of App_Data and of the sign-in as the first snapshot.
4. Narrate, then record the three recorded clips in this order: the Features page (enable Google Authentication), the Google settings page (type the fake Client ID and API secret key, save), and the login page. The first two change the site, so take a snapshot after each good take and restore the matching one before another take. The login clip only looks: it never clicks the Google button.
5. The two slide chapters (how Google sign-in works, related settings) need no recording.
