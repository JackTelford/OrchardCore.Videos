# OpenID Connect Client: Sign In With Another Identity Provider

**YouTube:** not published yet · **Length:** 5:25 · **Feature:** [OrchardCore.OpenId.Client](https://docs.orchardcore.net/en/latest/reference/modules/OpenId/)

The OpenID Connect Client Integration feature lets people sign in to an Orchard Core site with an account from another
identity provider. The video explains how the sign-in works, enables the feature, connects a provider in Settings,
OpenID Connect, Authentication Client, chooses what first-time users are asked to enter, and follows a first sign-in
through to the linked local account. The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The site has two tenants: the
Default tenant is the site that the video shows, and a second tenant with the URL prefix idp plays the identity
provider. Both are installed by Auto Setup, and the provider is seeded with an application for the Default tenant before
recording. The Default tenant never uses Secrets, and its admin user is admin with the password chosen outside git.

The clips depend on each other and are recorded in the order they play: enable the feature, connect the provider, save the
registration options, sign in (without the admin session), and look at the account in the Users list. Each one changes
the site, except the last, so a copy of the site data is restored before another take of a clip.

recording-plan.md has everything a person needs: the settings of both tenants, the seed data of the provider, the order
and snapshots, and every action of every step. OPEN-QUESTIONS.md lists what nobody has confirmed on a running site.
