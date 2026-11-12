# Workflows: Automate Tasks Without Writing Code

**YouTube:** not published yet · **Length:** 5:31 · **Feature:** OrchardCore.Workflows ([Workflows documentation](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/))

An introduction to the Workflows module for site owners and administrators. It explains what a workflow is (events,
tasks, outcomes, transitions and instances), then enables the feature, creates a workflow, takes a tour of the
designer, builds a workflow that shows a notification when an article is published, runs it, and opens the instance
it created. The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact actions, selectors,
labels and commands of every clip are in [recording-plan.md](recording-plan.md); what a person must confirm on a
running site is in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. **The site.** Build the Orchard Core web application and run it on port 5010 with an empty data folder outside both
   repositories, provisioned by Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone
   America/Los_Angeles, the user **admin** with the email address admin@example.com and a password chosen outside git.
   Block outbound calls. The site needs no seed data: the Blog recipe creates the Article, named About, that the video publishes.
2. **Features.** None to enable beforehand. The **Workflows** feature is enabled on camera in the first clip, and the
   other three Workflows features stay disabled. The video has no preparation step.
3. **Sign in and narrate.** Sign the recorder in once, outside the capture, and generate the narration before recording,
   so each step is held for its narration.
4. **Clip order.** Record the clips in the order they play: Enable Workflows, Create a workflow, The designer, Build a
   workflow, Run the workflow, Follow the instances. Each starts from the state the one before left. The designer tour
   and the instances clip change nothing. With the site stopped, copy the data folder aside before the first clip and
   after the enable, create, build and run clips, and restore the copy that precedes a clip before recording it again.
