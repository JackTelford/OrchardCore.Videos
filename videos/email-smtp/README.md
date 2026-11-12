# Email: Configure SMTP and Send a Test Email

**YouTube:** not published yet · **Length:** 4:25 · **Feature:** [OrchardCore.Email](https://docs.orchardcore.net/en/latest/reference/modules/Email/)

How a site sends email, and how to check it. The video enables the SMTP Email Provider (the Email feature comes with
it), connects the site to an SMTP server on the SMTP tab under Settings, Communication, Email, sends a message from
Tools, Testing, Email Test, and looks at the message in a local test mail server. It ends with what to check when a test
fails. The title, description, tags and chapters YouTube shows are in the video's metadata file. The narration is in the
storyboard, and everything a person needs to record the video is in the recording plan of this folder.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script.

1. Run the CMS web application of Orchard Core with its own empty data folder outside both repositories, on port 5010,
   provisioned by Auto Setup: the **Blog** recipe, site name **Orchard Core**, time zone America/Los_Angeles, user
   **admin** with the email admin@example.com, and a password chosen outside git. No feature is enabled before
   recording: the clip 02-enable enables the Email feature and the SMTP Email Provider on camera.
2. Start a local test mail server (smtp4dev, in a container) that takes plain SMTP connections without a sign-in, with
   SMTP on port 2525 of the machine and its web page on port 3000. Its inbox must be empty before the clip 04-test. The
   recording plan has the exact start-up and clean-up steps.
3. Sign the recorder in once, outside the capture, as the skill describes. Stop the site and copy its data folder aside,
   together with the saved sign-in (snapshot A). Take two more copies: one after 02-enable (snapshot B) and one after
   03-configure (snapshot C).
4. Narrate the video, then record the clips in the order 02-enable, 03-configure, 04-test and 05-inbox. The first two
   change the site (they enable the features and save the settings); 04-test sends one message to the mail server, and
   05-inbox shows that message, so record it straight after 04-test. The slide chapters (01-concepts and 06-fail) need
   no recording.
