# Change Email: Let Users Update Their Email Address

**YouTube:** not published yet · **Length:** 3:19 · **Feature:** [OrchardCore.Users.ChangeEmail](https://docs.orchardcore.net/en/latest/reference/modules/Users/)

The video shows what the Users Change Email feature adds: a Change Email item in the user menu and a page where a
signed-in user enters a different address. It enables the feature under Tools, Features, turns on its setting under
Settings, Security, Change Email, changes the address of the signed-in account, and then shows what the page refuses: the
current address, an address another user has, and a value that isn't an email address. The title, description, tags and chapters YouTube shows are kept in the metadata file of this folder.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The file recording-plan.md
holds the commands, every action, the text to type and what must be on screen at the end of each step.

1. Run a throwaway Orchard Core site from a checkout of OrchardCMS/OrchardCore, on an empty data folder outside both
   repositories, provisioned by Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone
   America/Los_Angeles, and the user admin with the email admin@example.com. The plan lists every setting.
2. Sign the recorder in as admin, with a password chosen outside git, so that the saved sign-in stays out of the
   repository.
3. Seed the site by hand, outside the capture: create the user jamie with the email jamie.chen@example.com and
   confirm that email from the Users list, so the checks have an address that is already taken. Leave the feature
   Users Change Email disabled. Stop the site and copy its data folder and the saved sign-in aside: restore that copy
   before every full take.
4. Narrate the video, then record the clips in the order they depend on each other: 02-enable, 03-setting,
   04-change, and 05-checks. Clip 04-change changes the email address of admin, so sign the recorder in again before it
   and after it. To take one clip again, restore the copy of the data folder made before it.
