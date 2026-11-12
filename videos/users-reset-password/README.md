# Reset Password: Let Users Recover Their Accounts

**YouTube:** not published yet · **Length:** 4:29 · **Feature:** `OrchardCore.Users.ResetPassword` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Users/))

The video shows how a site lets users recover their own accounts. An administrator enables the Users Reset Password
feature, checks that the site can send email, and allows the reset in the settings. A user then asks for a reset link on
the login page, receives the email, chooses a password and logs in. It ends with what to know about the limit of one email
per minute, delivery problems, unverified email addresses and the Rate Limits feature. The title, description, tags and
chapters YouTube shows are in the video's metadata file; the exact actions, labels and seed data of every clip are in the
recording plan, and the points that need a person with a running site are in the list of open questions.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Besides Orchard Core it needs a
local mail catcher (smtp4dev, run in Docker), because the reset link travels by email.

1. Build the web project of Orchard Core and run it with an empty data folder outside both repositories, on port 5010,
   provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user admin).
   Start the mail catcher, and point the recorder at both with the settings of the recording plan.
2. Seed the site as the recording plan says: enable **Email** and **SMTP Email Provider** and leave **Users Reset Password**
   disabled, save the SMTP settings (sender noreply@example.com, host localhost, the catcher's SMTP port), and create the
   user jamie.chen (jamie.chen@example.com). Three passwords are chosen outside git: the administrator's, Jamie's first
   password, and the password typed during the reset. None is written in the repository.
3. Sign the recorder in, then copy the data folder and the sign-in aside as the first snapshot.
4. Narrate, then record in this order: the clips 02-enable, 02-email and 03-allow (take a second snapshot after
   03-allow), then 04-request, 05-mail and 06-reset together in one run, within 15 minutes.
   02-enable and 03-allow change the site; 04-request sends the email and starts a cooldown of one minute for Jamie;
   06-reset changes Jamie's password. To record a clip again, restore the matching snapshot, restart the site, empty the
   inbox of the catcher, and record again the clips that depend on each other.
5. When the video is done, remove the catcher's container and delete the snapshots and the sign-in.
