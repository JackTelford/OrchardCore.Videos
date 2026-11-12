# OpenID Connect Server: Flows, Endpoints and Keys

**YouTube:** not published yet · **Length:** 6:00 · **Feature:** [OrchardCore.OpenId.Server](https://docs.orchardcore.net/en/latest/reference/modules/OpenId/)

A tour of the OpenID Connect Authorization Server settings page, for the administrator of a site. It enables the
feature, then goes through the endpoints applications call, the flows they use to get a token, the requirements that
apply to every application, and the keys and token options, and it ends by saving a configuration. It shows the admin
screens only. The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact actions of every
step, the labels and where they come from in the Orchard Core source are in recording-plan.md; what could not be
checked without a running site is in OPEN-QUESTIONS.md.

1. **Site.** A local Orchard Core site provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time
   zone America/Los_Angeles, user admin with the email admin@example.com. The password is chosen outside git and given
   to the recorder when it signs in. The site makes no outbound calls.
2. **Features.** None are enabled in advance. The first recording enables **OpenID Connect Authorization Server** on
   camera. The OpenID Connect Secrets, Client Integration and Token Validation features stay disabled, so the settings page shows only its own sections.
3. **Seed data.** None. The video shows no records.
4. **Order and snapshots.** Copy the site's data aside after Auto Setup (snapshot A), record the chapter that enables
   the feature, and copy the data again (snapshot B). The chapters on endpoints, flows, requirements and keys leave the
   site in snapshot B and can be recorded in any order. The chapter that saves the settings is recorded last, and
   snapshot B is restored before another take of it.
5. **Certificates.** The settings page can list the certificates of the machine that runs the site, and it needs at
   least one that has a private key (the ASP.NET Core HTTPS development certificate is enough); without one it shows
   two warnings instead. No recording chooses a certificate store, so no certificate is ever on screen; check the
   frames for it before publishing.
