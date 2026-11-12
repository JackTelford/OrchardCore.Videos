# Timer Workflows: Run Tasks on a Schedule

**YouTube:** not published yet · **Length:** 3:59 · **Feature:** [OrchardCore.Workflows.Timers](https://docs.orchardcore.net/en/latest/reference/modules/Workflows/)

The video shows how the Timer Event of the Workflows module runs a workflow on a CRON schedule, or makes a workflow wait for the next scheduled time. It enables the Timer Workflows Activities feature, builds a workflow named Scheduled check with a Timer Event and a Log Task, publishes it, and follows its instances in the admin: one instance that waits, and one finished instance for each run. A closing chapter of slides covers the one-minute check, restarts, and keeping the list of instances short.

The title, description, tags and chapters YouTube shows are in the video's metadata file. The audience is administrators: the video shows only the admin interface.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The site is provisioned by Auto Setup with the Blog recipe, the site name Orchard Core, the time zone America/Los_Angeles and the user admin (email admin@example.com). The password is chosen outside git and passed in at recording time; it never appears in the repository.

The Blog recipe does not enable the Workflows feature, so enable Workflows before recording, outside the capture. Leave Timer Workflows Activities disabled: the first clip enables it on camera.

Three clips are recorded, in this order, because each one starts from the state the previous one left:

1. Enable the timer (Tools, Features).
2. Build the scheduled workflow (Design, Workflows), ending with Publish.
3. Watch it run, recorded at least three minutes after publishing, once the list of instances shows one Halted instance and at least one Finished instance.

The first two clips change the site, and a published timer workflow keeps creating instances every minute. Copy the site's data aside before the first clip and again after it, and restore the copy before every new take. The recording plan lists the exact steps, the waits, and the checks for private data.
