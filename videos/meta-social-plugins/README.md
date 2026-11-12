# Meta Social Plugins: Like, Share and Comments Widgets

**YouTube:** not published yet · **Length:** 4:02 · **Feature:** [OrchardCore.Facebook.Widgets](https://docs.orchardcore.net/en/latest/reference/modules/Facebook/#meta-social-plugins-widgets)

The video covers the Meta Social Plugins Widgets feature of Orchard Core. It enables the feature, registers a Meta app
in Settings, Integrations, Meta App, adds the Facebook Like, Facebook Share and Facebook Comments widgets to the
Footer zone on the Always layer, and checks them on the site. It ends with the permission that guards the Body field of
these widgets. It follows the guide [Integrate Facebook Social Plugins](https://docs.orchardcore.net/en/latest/guides/integrate-facebook-plugins/).
It is an admin video: it shows the admin screens and the front end of the site, and no code. The title, description,
tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Run the web project of Orchard Core (the CMS one) on a local address, with its own empty data folder outside both
   repositories. Provision it with Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone
   America/Los_Angeles, and the user admin with the email admin@example.com. The password is chosen outside git and is
   never written in this repository. recording-plan.md lists the exact settings.
2. The Blog recipe supplies everything the video needs: the Layers and Widgets features, the zones Content and Footer,
   the layers Always and Homepage, and a blog post. There is no seed data. The only values typed on camera are made up:
   an application identifier, a secret key, and three widget titles.
3. The Meta features are disabled when the recording starts: the first recording enables them on camera. The secrets
   feature stays disabled.
4. The browser that records must be able to reach Meta's servers, because Meta's own script draws the plugins in the
   last recording. Check this before recording, as recording-plan.md describes, and read OPEN-QUESTIONS.md first.
5. Narrate, then record the four recordings in this order, because each one starts from what the previous one left:
   enable the feature, register the Meta app, add the three widgets, check the site. Keep a copy of the data folder
   after each recording, and restore the copy that precedes a recording before taking it again. The two slide
   chapters need no recording.
