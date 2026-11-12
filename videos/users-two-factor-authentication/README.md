# Two-Factor Authentication: Authenticator App, Email and SMS

**YouTube:** not published yet · **Length:** 6:04 · **Feature:** [OrchardCore.Users.2FA](https://docs.orchardcore.net/en/latest/reference/modules/Users/#two-factor-authentication)

For site owners and administrators. The video explains how two-factor sign-in works, then enables the authenticator app,
Email and SMS methods, sets the rules of the site (remembered browsers, the number of recovery codes, and who must use
it), and shows how a user sets up each method and signs in with it. It ends with the page a user sees when the site
requires two-factor authentication, and with the way an administrator turns it off for a user who lost access. The
title, description, tags and chapters YouTube shows are kept in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The step by step plan, with
every action, every label and every check, is recording-plan.md; this is the short version.

1. Run the Orchard Core web site in the **Development** environment (the Log SMS provider that stands in for an SMS
   service exists only there), on a throwaway data folder outside both repositories, provisioned by Auto Setup: the
   Blog recipe, the site name **Orchard Core**, the time zone America/Los_Angeles, and the user admin with the email
   admin@example.com. The password is chosen outside git. Block outbound calls, so that nothing leaves the machine.
2. Seed the site, as listed in the plan: enable **SMTP Email Provider** and **SMS**, deliver the email to a pickup
   folder, choose the **Log** SMS provider, and add the user jamie with a verified email address. Sign the recorder in,
   and copy the site's data folder and the saved sign-in aside as the first snapshot. Enable **no** two-factor feature:
   the first clip does it.
3. Narrate the video, write the recorder scenes from the plan, and record the clips in this order, restoring the
   snapshot named in the plan before a new take of a clip that changes the site: 02-enable, 04-app, 03-settings,
   07-required, 08-disable, 06-methods, 05-signin, 06-email, 06-sms. The order differs from the order the clips play
   in. The admin user needs two-factor authentication before the site requires it, because a user who has not enabled
   it can open nothing but the two-factor pages; so 04-app comes before 03-settings, and only 02-enable and 04-app use
   the saved sign-in.
4. The scenes type the codes themselves: the authenticator code is computed from the key shown on the page, the email
   code is read from the pickup folder, and the SMS code from the site's log. None of this is committed.
