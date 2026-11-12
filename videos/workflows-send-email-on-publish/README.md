# Workflows: Send an Email When Content Is Published

**YouTube:** not published yet · **Length:** 5:35 · **Feature:** [OrchardCore.Workflows](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/)

The video builds a workflow in the designer that emails the editors each time an article is published: a Content Published
Event, limited to the Article content type, followed by an Email Task that sends a plain text message through the SMTP
provider to a local test mail server. It then publishes the workflow, publishes an article to run it, and reads the email.
The Workflows, Contents and Email modules work together in it. The title, description, tags and chapters YouTube shows are
in `video.json`, and `recording-plan.md` says what to do on screen, step by step.

## Recording it again

The video is recorded on a **fresh site** with fake data only (every address is at example.com), so it needs no mask script.

1. Build the Orchard Core web project and run it with its application data folder in an empty place outside both
   repositories, on port 5010, provisioned by Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone
   America/Los_Angeles, and the user admin with the email admin@example.com. The password is chosen outside git.
2. Start a local test mail server: smtp4dev in Docker, with its web page on port 3000 and its SMTP port on 2525. The
   exact command is in recording-plan.md.
3. Seed the site outside the capture, as recording-plan.md describes. Enable **Workflows** and **SMTP Email Provider**
   (which enables **Email**) under Tools, Features. On Settings, Communication, Email, enable the SMTP provider with the
   sender noreply@example.com, the host localhost and the port 2525, and check it with Tools, Testing, Email Test. Make
   sure Design, Workflows lists no workflow. Then copy the application data folder and the recorder's sign-in aside, as the
   starting snapshot.
4. Sign the recorder in, narrate the video with the skill's narration script, and record the clips **in this order**, because
   each one starts from the state the previous one left: 02-setup, 03-create, 04-event, 05-email, 06-publish, 07-try. Only
   02-setup leaves the site as it found it. Copy the application data folder aside after each clip, restore the copy to take a
   clip again, and restart the test mail server before 02-setup and before 07-try so that its inbox is empty.
