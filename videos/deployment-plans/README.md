# Deployment Plans: Choose What to Export

**YouTube:** not published yet · **Length:** 5:18 · **Feature:** [OrchardCore.Deployment](https://docs.orchardcore.net/en/latest/reference/modules/Deployment/)

A deployment plan is a saved, named list of steps that decides what an exported package holds. The video creates a plan,
adds steps from the available categories (content, content definitions, a file, and the recipe details), puts the
steps in order, executes the plan with File Download, and shows how plans are searched, renamed, and protected by
permissions. It is a video for site owners and administrators: it shows the admin and no code. The title, description,
tags and chapters YouTube shows are kept in the video's metadata file.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script.

1. Provision a throwaway site from an Orchard Core checkout with Auto Setup: the Blog recipe, the site name Orchard
   Core, the time zone America/Los_Angeles, and the user admin with the email admin@example.com. The password is
   chosen outside git and passed to the recorder in the environment. The Blog recipe already enables Deployment, so no
   feature has to be enabled, and no remote instance may be created.
2. Seed two empty plans, Archive Old Posts and Weekly Settings Backup, outside the capture. Sign the recorder in, then
   stop the site and copy its data folder aside: restore that copy before every full take.
3. Narrate, then record the clips in the order they play: 02-create, 03-steps, 04-order, 05-general, 06-execute, and
   07-manage. Each clip starts from the state the previous one left, except 07-manage, which only needs the two seeded
   plans. 06-execute downloads a file, which is deleted afterwards and never committed.

The exact actions of every step, the fake values to type, the snapshots to take, and the settings of the site are in
recording-plan.md. What could not be checked without a running site is in OPEN-QUESTIONS.md: read it before the first
take.
