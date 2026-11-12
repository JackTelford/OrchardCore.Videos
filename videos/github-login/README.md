# GitHub Sign-In: Let Developers Sign In With GitHub

**YouTube:** not published yet · **Length:** 3:36 · **Feature:** `OrchardCore.GitHub.Authentication` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/GitHub/))

The video shows how a site owner lets people sign in with their GitHub account. It explains the round trip between the
site and GitHub, the app that is registered on GitHub, and the admin screens: enabling GitHub Authentication, entering
the Client ID, the client secret and the callback path under Settings, Security, Authentication, GitHub, and the GitHub
button on the login page. It ends on what the site does when GitHub sends the person back. The title, description,
tags and chapters YouTube shows are in the video's metadata file.

GitHub is never opened in the recording: the round trip needs a real GitHub app, so the video stops before the
redirect. The Client ID and the client secret on screen are made up.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan in this
folder holds every action of every clip, and the open questions file lists what a person confirms on a running site.

1. Run the default web host of Orchard Core with an empty data folder outside both repositories, on port 5010,
   provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user
   **admin**, email admin@example.com, and a password chosen outside git. Block the site's outbound web access, so it
   never reaches GitHub. GitHub Authentication, External Authentication and Secrets stay disabled until the first
   clip enables the first two.
2. Sign the recorder in once. The video needs no seed data, so there is no prepare step. Stop the site and copy its
   data folder and the saved sign-in aside as the first snapshot.
3. Narrate, then record the clips in this order, because each starts from the state the previous one leaves: enable
   the feature, save the settings, then the signed-out login page, which changes nothing. Take a snapshot of the data
   folder after the first two clips, and restore the snapshot a clip starts from before recording it again.
