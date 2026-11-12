# Microsoft Account: Sign In With Personal Microsoft Accounts

**YouTube:** not published yet · **Length:** 3:34 · **Feature:** `OrchardCore.Microsoft.Authentication.MicrosoftAccount` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Microsoft.Authentication/))

The video shows how to let visitors sign in with their Microsoft account. It explains what you create at Microsoft,
enables the Microsoft Account Authentication feature, enters AppId, Secret Key, CallbackPath and Store tokens, and finds
the Microsoft Account button on the login page. It stops at the redirect to Microsoft, then describes in slides what
happens when Microsoft sends the visitor back. The title, description, tags and chapters YouTube shows are in
video.json; the exact steps of each clip are in recording-plan.md.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The AppId and Secret Key are
sample values typed on camera, and nothing is sent to Microsoft.

1. Run the web project of Orchard Core (the commit named in the catalog's source page) with its own empty data folder
   outside both repositories, on port 5010, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time
   zone America/Los_Angeles, user admin with the email admin@example.com. Block the site's outbound calls so it never
   reaches Microsoft. Enable no feature beforehand, and leave Secrets, Users Registration and Email disabled.
2. Sign the recorder in as admin with the password chosen outside git. This video needs no prepare step: the clips
   enable the feature and save the settings themselves. Stop the site and copy its data folder aside as snapshot S0.
3. Narrate, then record the clips in this order, because each one starts from the state the previous one left:
   02-enable, 03-settings, 04-login. Copy the data folder aside after 02-enable (S1) and after 03-settings (S2), and
   restore the snapshot a clip starts from before recording it again. The clips 01-how and 05-return are slides and
   need no site.

recording-plan.md holds the commands, the sample values and what each step must show.
