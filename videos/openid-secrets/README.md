# OpenID Connect Signing and Encryption Keys as Secrets

**YouTube:** not published yet · **Length:** 3:45 · **Feature:** [OrchardCore.OpenId.Secrets](https://docs.orchardcore.net/en/latest/reference/modules/OpenId/)

An Orchard Core site that works as an OpenID Connect server uses a signing key and an encryption key for its tokens. This video
explains why the certificate files the server generates by default stay on one server, enables the OpenID Connect Secrets
feature, creates two RSA Key secrets, and selects them under RSA Keys from Secrets in the Authorization Server settings. It ends
with what happens to a certificate that is already selected, and what stays in use when no secret is selected. The title,
description, tags and chapters YouTube shows are kept with the video.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The details of every clip, with the
labels and where the code spells them, are in recording-plan.md. Read OPEN-QUESTIONS.md first: its first question is a check
on a running site that must pass before recording.

1. Run a fresh Orchard Core site, provisioned by Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone
   America/Los_Angeles, the user **admin** with the email **admin@example.com**. The password is chosen outside the repository
   and handed to the recorder, never written down.
2. Sign the recorder in. Then, outside the capture, enable the features **OpenID Connect Authorization Server** and **Secrets**,
   but not **OpenID Connect Secrets**: the first clip enables it. No other data is seeded; the secrets are created on camera.
3. Copy the site's data folder and the sign-in aside: restore them before every full take.
4. Narrate, then record the three clips in the order they play, because each one starts from the state the previous one left:
   Enable the feature, Create the keys, Select the keys. The slides and the cards need no recording.
5. Check the recording for private data, including the whole Authorization Server page: its certificate lists read the certificates
   of the recording machine.
