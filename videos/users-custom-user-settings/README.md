# Custom User Settings: Add Your Own Fields to User Profiles

**YouTube:** not published yet · **Length:** 4:48 · **Feature:** `OrchardCore.Users.CustomUserSettings` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Users/CustomUserSettings/))

The video shows how a site keeps its own information about each user. It explains that a section of custom user
settings is a content type with the Custom User Settings stereotype, enables the feature, creates a section with a
text field and a Boolean field, and fills it in on the Edit User page. It then grants the permission of the section to
a role and adds the Custom User Settings step to a deployment plan. The title, description, tags and chapters YouTube
shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The page
[recording-plan.md](recording-plan.md) gives every action of every clip, with the text to type and where each label
comes from; [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md) lists what a person must confirm on the running site.

1. Run the Orchard Core web project with an empty data folder outside both repositories, on port 5010, provisioned by
   Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone America/Los_Angeles, and the
   administrator admin with the email admin@example.com. Leave the Custom User Settings feature off: the first clip
   turns it on.
2. Seed the site outside the capture, with the Users and Deployments pages of the admin: the users jamie and priya
   (addresses at example.com; priya in the Editor role) and an empty deployment plan named **Team profiles**. Choose
   their passwords, and the administrator's, outside git, and never show them on camera.
3. Sign the recorder in as the administrator, then stop the site and copy its data folder and the saved sign-in aside
   as the first snapshot.
4. Narrate, then record the clips in the order Enable the feature, Create a section, Fill in a user's section, Control
   who can edit a section, Export the values. Each clip changes the site and starts from what the previous one left:
   take a snapshot after each clip, and restore the one before a clip to record it again. The table in the recording
   plan lists them.
