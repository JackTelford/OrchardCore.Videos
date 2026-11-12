# Reverse Proxy Settings: Run Behind a Load Balancer

**YouTube:** not published yet · **Length:** 3:37 · **Feature:** [OrchardCore.ReverseProxy](https://docs.orchardcore.net/en/latest/reference/modules/ReverseProxy/)

Behind a reverse proxy or a load balancer, a site sees the proxy instead of the visitor. This video explains which
forwarded headers carry the original address, protocol and host, enables the Reverse Proxy Configuration feature,
chooses the headers on the Settings, Reverse Proxy page, and ends with why a site should trust only its own proxies.
The title, description, tags and chapters YouTube shows are kept with the video.

## Recording it again

The video is recorded on a **fresh site** with no seed data, so it needs no mask script. The site is called directly,
with no proxy in front of it. The recording plan has every action of every clip, and the open questions list what a
person confirms on a running site.

1. **The site.** A local Orchard Core site provisioned by Auto Setup with the Blog recipe, the site name **Orchard
   Core**, the time zone America/Los_Angeles, and the user admin with the email admin@example.com. The password is
   chosen outside git. It runs on port 5010, with its data in an empty folder outside both repositories.
2. **The seed.** There is none, and no feature to enable beforehand: the Reverse Proxy Configuration feature must be
   off when recording starts, since the video turns it on.
3. **Sign in once** with the recorder, outside the capture, and copy the site and the sign-in aside as snapshot A.
4. **Clip order.** Record 02-enable first. It enables the feature and restarts the tenant, so copy the site aside
   again as snapshot B afterwards. Then record 03-settings, which starts from snapshot B and saves the three
   headers. To record 02-enable again, restore snapshot A; to record 03-settings again, restore snapshot B. The
   chapters 01-why and 04-safe are slides and need no site.
