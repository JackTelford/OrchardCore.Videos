# Build a Members Area: Registration, Roles and Members-Only Pages

**YouTube:** not published yet · **Length:** 6:56 · **Feature:** OrchardCore.Users.Registration ([Users module documentation](https://docs.orchardcore.net/en/latest/reference/modules/Users/))

This video builds a members area with the admin screens. Visitors register and wait for approval, an administrator
gives them the Member role, and the permissions of a securable content type decide who can open its pages. It follows
the sign-up from both sides: the visitor who registers, and the administrator who approves the account. The title,
description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The site uses the Blank site
recipe, because the Blog recipe has content types that are not securable and would disappear for visitors. The file
recording-plan.md holds everything a person needs, without any other decision:

1. The site: the Auto Setup values, the time zone, and the two passwords chosen outside git (the administrator's and the
   member's).
2. The seed: one recipe pasted into JSON Import, which adds the default theme, the content types Page and Members Page,
   and the public page Welcome. No external service is needed, because email verification stays off.
3. The clip order: each recording starts from the site the previous one left, so record them in the order they play:
   registration, the securable type, the Member role, the restricted roles, the members-only page, the visitor's
   sign-up, the approval, and the member's sign-in. The two visitor clips start signed out.
4. A snapshot of the site after every recording: restore the snapshot of the clip before the one you record again.

Narrate first, then record, then assemble, as the video producer skill describes.
