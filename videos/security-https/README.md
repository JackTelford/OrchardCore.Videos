# HTTPS: Require Secure Connections

**YouTube:** not published yet · **Length:** 3:36 · **Feature:** [OrchardCore.Https](https://docs.orchardcore.net/en/latest/reference/modules/Https/)

The HTTPS module can redirect requests from HTTP to HTTPS, and can send the HSTS header that tells browsers to keep using
HTTPS. The video explains the four settings on slides, turns the module on in Tools, Features, and sets it up in
Settings, Security, HTTPS. It ends with the tenant, restart and load balancer points to know. The title, description,
tags and chapters YouTube shows are in video.json, and the narration is in storyboard.json.

## Recording it again

The video is recorded on a fresh local site with no data of its own, so it needs no mask script. The site must be
served over HTTPS only, in the Development environment, because the HTTPS settings page ignores the changes saved over
HTTP. The machine running the recorder must trust the ASP.NET Core development certificate, so the
browser opens the site without a certificate error.

1. Provision the site with Auto Setup: the Blog recipe, site name Orchard Core, time zone America/Los_Angeles, and the
   user admin with the email admin@example.com. Its data folder lives outside both repositories. There is no seed
   data, and the HTTPS feature stays disabled, because the first clip enables it.
2. Sign the recorder in once, outside the capture, with the password chosen outside git. Then stop the site and copy
   its data folder and the saved sign-in aside. This is the first snapshot.
3. Record the features clip. It enables the HTTPS feature and changes the site. Copy the data folder and the sign-in
   aside again. This is the second snapshot.
4. Record the settings clip. It depends on the features clip, and it saves the HTTPS settings, so it changes the site
   too. Restore the first snapshot before a full take, and the second one before another take of the settings clip.

recording-plan.md has the exact commands, the selectors and the source of every label.
