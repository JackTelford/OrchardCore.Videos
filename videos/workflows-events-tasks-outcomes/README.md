# Workflows: Events, Tasks and Outcomes

**YouTube:** not published yet · **Length:** 5:02 · **Feature:** [`OrchardCore.Workflows`](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/)

This video explains the three building blocks of a workflow: events, which start a workflow or make it wait, tasks,
which do the work, and outcomes, which connect one activity to the next. It builds a small workflow in the designer
that reacts to a published page, with a Fork Task, a Notify Task and a Log Task, publishes it, and checks the
instance that ran. The title, description, tags and chapters YouTube shows are kept with the video.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. It shows the admin screens
and no code. The exact actions, selectors and checks are in recording-plan.md; what a person could not confirm from
the source is in OPEN-QUESTIONS.md.

1. **The site.** A fresh Orchard Core site provisioned by Auto Setup: the Blog recipe, the site name Orchard Core,
   the time zone America/Los_Angeles, and the user admin with the email admin@example.com. The password is chosen
   outside git and given to the recorder through the environment. The site calls no external service.
2. **The feature.** Enable only Workflows, under Tools, Features, before recording. The other Workflows features stay
   off, so the Activities pane and the start picker list fewer activities. Create no workflow and no
   page beforehand: the list of workflows is empty when the first recording starts.
3. **The designer's right panel.** The panel opens on a tab that prints sample expressions, so it stays collapsed
   to its rail for the whole video. recording-plan.md says how to do that for every recording.
4. **Order.** Narrate first, then record the clips in this order: 02-event, 03-outcomes, 04-tasks and 05-run. Each
   clip starts from the site that the one before left, because the workflow is saved as a draft on the server. Take a
   copy of the site's data folder before the first clip and after each one, and restore the copy taken after the
   previous clip before recording a clip again. The last clip publishes the workflow and creates a page and an
   instance, so it must run on a site where the workflow is still a draft.
5. **Slides and cards.** The opening and closing cards and the four slides of the first chapter need no site.
