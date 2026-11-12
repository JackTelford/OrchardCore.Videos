# User Login Settings: Sign-In Options and Lockout

**YouTube:** not published yet · **Length:** 4:38 · **Feature:** [OrchardCore.Users](https://docs.orchardcore.net/en/latest/reference/modules/Users/)

An admin video about the User Login page under Settings, Security. It shows the login page, then each option of the
page: Remember me and persistent sign-in cookies, the theme of the login page, local password login, and the details of a
user that can be edited. It ends with account lockout as an administrator sees it: the failed attempts, the Locked badge in
the Users list, and Unlock. The title, description, tags and chapters YouTube shows are in the video's metadata file; the
clip-by-clip instructions for the recorder are in recording-plan.md, and what a person must confirm on a running
site is in OPEN-QUESTIONS.md.

## Recording it again

The video is recorded on a **fresh site** with fake data only (the user jamie.chen, with the email
jamie.chen@example.com), so it needs no mask script. The environment variables, the locators and the exact actions of
every step are in recording-plan.md.

1. Run a throwaway Orchard Core site on port 5010, with its data folder outside both repositories, provisioned by Auto Setup:
   the Blog recipe, the site name **Orchard Core**, the time zone America/Los_Angeles, and the user admin with the email
   admin@example.com and a password chosen outside git. Enable no feature beyond the ones the Blog recipe enables:
   two-factor authentication, external authentication, registration, reset password, change email and rate limits would
   change the User Login page and the login page.
2. Write the recorder scenes from recording-plan.md (one method for each clip, one step for each narrated step), and sign
   the recorder in as admin.
3. Create the one seed user in the browser, outside the capture: **Access Control**, **Users**, **Add User**, with the user
   name jamie.chen, the email jamie.chen@example.com, a password and no role. Then open **Actions** in the row of the new
   user and choose **Confirm email**, so that the Users list shows no **Unconfirmed** badge. Stop the site and copy its
   data folder and the recorder's sign-in aside as snapshot S0.
4. Narrate the video, then record the clips in the order of the chapters: 02-open, 03-remember, 04-theme, 05-local,
   06-details, 07-lockout. The clips do not depend on each other, because each one starts from S0. Restore S0 before
   03-remember, 04-theme, 06-details and every take of 07-lockout. The clip 05-local must never save the settings page:
   disabling local password login on a site with no external provider removes the login form.
