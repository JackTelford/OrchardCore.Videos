# Background Tasks: See, Schedule and Run Them

**YouTube:** not published yet · **Length:** 4:44 · **Feature:** [OrchardCore.BackgroundTasks](https://docs.orchardcore.net/en/latest/reference/modules/BackgroundTasks/)

For site owners, editors and administrators. The video shows what background tasks are, enables the Background Tasks
module, and walks through the task list (badges, layouts, search and filters), the details of a task, changing a
schedule, running one task or several on demand, and enabling or disabling a task. It ends with what the pages cannot
show: failed runs, restarts and web farms. The title, description, tags and chapters YouTube shows are in the video's
metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Everything a person needs
is in the recording plan of this folder: the site, the order of the clips, the exact actions of every step, and the
checks for private data. In short:

1. Run the web project OrchardCore.Cms.Web of a checkout of OrchardCore with its own empty data folder outside both
   repositories, on port 5010, provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone
   America/Los_Angeles, user admin with the email admin@example.com. The password is chosen outside git.
2. Before recording, enable two features besides the Blog recipe's: **Publish Later**, which adds a task that runs
   every minute, and **Content Version Pruning**, which adds a task that is disabled by default. Leave **Background
   Tasks** disabled: the first clip enables it on camera. Keep a copy of the data folder at that point.
3. Start the site and open its home page once, because the background service runs the tasks of a tenant only after
   the tenant has handled a request. Keep the site running for at least five minutes after that request, so the task
   that runs every minute has several runs. Sign the recorder in, narrate, and record the first clip, which enables
   the module. Then stop the site, keep a second copy of the data folder, start the site, open its home page once, and
   wait five minutes again. Record the other clips in order: the list, the details, the schedule, running tasks on
   demand, enabling and disabling.
4. The execution state of the tasks is kept in memory by the running site. To take a clip again, stop the site,
   restore the copy named in the recording plan, start the site, open its home page once, and wait five minutes again.
