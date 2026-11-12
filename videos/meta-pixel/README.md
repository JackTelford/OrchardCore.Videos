# Meta Pixel: Track Conversions From Meta Ads

**YouTube:** not published yet · **Length:** 3:48 · **Feature:** [OrchardCore.Facebook.Pixel](https://docs.orchardcore.net/en/latest/reference/modules/Facebook/#meta-pixel)

This video shows how the Meta Pixel feature connects a site to Meta. It enables the feature in Tools, Features, adds
the Pixel Identifier in Settings, Integrations, Meta Pixel, and explains what the pixel does on the pages of the site.
It then covers the optional Conversions API settings, the Conversions API Access Token and the Test Event Code, and the
Meta Conversions API Event task that a workflow uses to send events to Meta from the server. The title, description,
tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Nothing in it contacts Meta:
the three recordings never open a page that runs the pixel, and the recorder's browser blocks the Meta hosts all the
same. The recording plan has every action, label and value, and the open questions list what a person must confirm on
a running site.

1. Run the source of Orchard Core on a local site with an empty data folder outside both repositories, provisioned by
   Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone America/Los_Angeles, the user admin with
   the email admin@example.com, and a password chosen outside git. Block the outbound calls of the site, as the
   recording plan describes.
2. Build the recorder with the scenes of this video and sign it in, as the skill describes. No feature needs enabling
   and no data needs seeding: Meta Pixel stays disabled until the first clip enables it. Stop the site and copy its
   data folder and the sign-in aside, as the first snapshot of the plan.
3. Narrate the video, then record the clips 02-enable, 03-pixel and 05-capi, in that order. Each clip starts from the
   state the previous one left: 02-enable enables the feature, 03-pixel saves the Pixel Identifier, and 05-capi saves
   the access token and the test event code. Take a snapshot after each clip, and restore the one before it to record
   a clip again. The other chapters are slides that the assembly step draws from the video's slide script.
