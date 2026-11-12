# Data Pipelines: Deliver Files by Link, Email, FTP and SFTP

**YouTube:** not published yet · **Length:** 5:57 · **Feature:** OrchardCore.DataPipelines ([Data Pipelines](https://docs.orchardcore.net/en/latest/reference/modules/DataPipelines/))

The video adds the four destinations that deliver the files of a pipeline: Share a download link, Send by email, Upload
to an FTP server and Upload to an SFTP server. It starts from a pipeline that already creates a CSV file, enables the
two upload features, sets up each destination, publishes the pipeline, and reads what the run delivered and the link it
shared. The title, description, tags and chapters YouTube shows are in video.json; the exact actions of every clip are
in recording-plan.md.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. It needs three local
services in Docker: an SMTP catcher, an FTP server and an SFTP server. recording-plan.md gives the commands that start
them, the settings, the fake data and the actions of every clip; OPEN-QUESTIONS.md lists what was not checked on a
running site.

1. Build the Orchard Core web project (the commit 92db4a83, or any commit with the same Data Pipelines pages) and run it
   with its own empty App_Data folder outside both repositories, on http://localhost:5010, provisioned by Auto Setup:
   the Blog recipe, site name Orchard Core, time zone America/Los_Angeles, user admin, email admin@example.com, and a
   password chosen outside git.
2. Seed the site as recording-plan.md says: enable Data Pipelines, Email and SMTP Email Provider; set up the email
   provider for the SMTP catcher; create the user jamie.chen; import the draft pipeline Blog posts export. Leave
   Data Pipelines - FTP and Data Pipelines - SFTP off: the first clip turns them on.
3. Start the three containers, and read the fingerprint of the SFTP server from the log of a rehearsal run.
4. Sign the recorder in, as the recorder's own documentation describes, then stop the site and copy its App_Data folder
   aside as the snapshot S0.
5. Narrate, then record the clips in the order they play: 02-features, 03-link, 04-email, 05-ftp, 06-sftp, 07-run,
   08-links. Each clip starts from the draft the previous one left, so copy App_Data aside after each clip (S2 to S7)
   and restore the copy made before a clip to record it again. The clip 08-links changes nothing.
