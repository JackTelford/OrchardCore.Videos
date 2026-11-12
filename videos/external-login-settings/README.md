# External Login: Let Users Sign In With Other Accounts

**YouTube:** not published yet · **Length:** 5:02 · **Feature:** [OrchardCore.Users.ExternalAuthentication](https://docs.orchardcore.net/en/latest/reference/modules/Users/)

The video explains what the External Authentication feature adds to a site, whichever provider is used: a button on the
login page for each provider, the pages a visitor sees the first time they sign in, and two groups of settings. It
enables a provider feature, follows a visitor from a refused first sign-in to a created local account, and then shows
the registration settings (Settings, Security, Registration) and the login setting that sends visitors straight to the
provider (Settings, Security, User Login). The title, description, tags and chapters YouTube shows are in the video's
metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The browser never leaves the
local machine: the "other service" is a second tenant of the same Orchard Core instance, acting as an OpenID Connect
provider named Example Accounts. The recording plan holds every action, the recipes that seed the two tenants, the
environment, and the exact fake data. The open questions file lists what must be confirmed on a running site before the
first take.

1. Build the Orchard Core web project and run it with its data folder outside both repositories, on port 5010,
   provisioned by Auto Setup with two tenants: the default tenant (the Blog recipe, site name **Orchard Core**, time
   zone America/Los_Angeles, user admin) and a second tenant at the URL prefix idp (the Blog recipe, site name
   **Example Accounts**). The recording plan lists the environment, including the proxy exception that lets the two
   tenants reach each other.
2. Seed the idp tenant with the recipe of the recording plan (the OpenID Connect server features, one application, and
   one user, taylor.brooks, with an address at example.com). Sign the recorder in on the default tenant as admin, with
   the password chosen outside git and passed in the environment, then stop the site and copy its data folder aside as
   the first snapshot. Restore a snapshot before each take.
3. Narrate the video, then record the clips in this order, with the seed steps for the default tenant (in the recording
   plan) after the first clip: 02-features, 03-login, 04-registration, 05-register, 06-settings, 06-straight. Each
   clip starts from the snapshot named in the plan, because the clips enable a feature, save settings and create a
   user.
