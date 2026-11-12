# Workflow Instances: Follow and Troubleshoot Each Run

**YouTube:** not published yet · **Length:** 4:21 · **Feature:** `OrchardCore.Workflows` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/))

Every run of a workflow is an instance. This video shows the list of instances and its statuses, the page of an instance
(the activities it ran, its variables and its journal), how to find the error that stopped a run and retry it, and how to
retry, cancel or delete several instances at once. It is for site owners and administrators, so it shows the admin
screens only. The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. recording-plan.md holds
every action of every clip, the seed data, and the checks for private data. OPEN-QUESTIONS.md lists what a person must
confirm on the running site before the first take.

1. Run Orchard Core (the web project of an OrchardCore checkout) on a free port, 5010 by default, with its data folder
   outside both repositories, provisioned by Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone
   America/Los_Angeles, the user admin with the email admin@example.com, and a password chosen outside git.
2. In **Tools > Features**, enable **Workflows** and **HTTP Workflows Activities**.
3. Seed the site, outside the capture:
   - import the two workflows, **Order follow-up** and **Payment follow-up**, with **Tools > Deployments > JSON Import**
     (the text to import is in recording-plan.md);
   - start a small local file server on port 5099, which the **Check stock** activity calls;
   - start seven runs with the **Run…** button of each workflow's designer: two Finished, two Halted and three Faulted (the last
     three with the file server stopped). The counts of the instances list must then read 7, 2, 0, 3, 2 and 0.
4. Stop the site and keep a copy of its data folder, and of the recorder's sign-in, to restore before every full take.
5. Sign the recorder in, narrate the video, and record the clips in this order: the list of instances, the waiting
   instance, the faulted instance, and the bulk actions. The first two only read the site. The last two retry and cancel
   instances, so restore the copy before another take of either, and start the file server before them.
