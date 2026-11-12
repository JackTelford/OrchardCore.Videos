# Tenants: Create a Tenant and Set It Up

**YouTube:** not published yet · **Length:** 5:22 · **Feature:** OrchardCore.Tenants ([Tenants documentation](https://docs.orchardcore.net/en/latest/reference/modules/Tenants/))

The video adds a tenant from the administration of the Default tenant, in the two stages the module has: the Create Tenant
form (name, category, description, Url Prefix, Hostname, recipe and database presets), then the Setup screen of the tenant
(site name, time zone and super user), and it opens the site that results. It also shows the Create & Setup button, which
saves the tenant and opens its Setup screen in one step. The title, description, tags and chapters YouTube shows are in
the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan gives every
action of every clip, with the commands to run. The open questions list what a person confirms on the running site.

1. Run the Orchard Core web project on http://localhost:5010, with its data folder outside both repositories. Provision the
   Default tenant once with Auto Setup and the **SaaS** recipe (site name **Orchard Core**, time zone America/Los_Angeles,
   user admin, email admin@example.com, a password chosen outside git): the SaaS recipe enables the Tenants feature. Then
   stop the site and start it again **without** the Auto Setup settings, because the Setup screen of a tenant is part of the
   subject, and with the site process in the time zone America/Los_Angeles, because the Setup screen opens on the time zone of
   the server. No feature to enable and no seed data: the tenants are created on camera.
2. Sign the recorder in (the session of the Default tenant only), and copy the data folder and the sign-in aside as snapshot A.
3. Narrate, then record the clips in this order: 02-create, 03-list, 04-setup, 05-open, 06-shortcut. The clips depend on
   each other and change the site: 02-create creates Maplewood, 04-setup sets it up, and 06-shortcut creates Riverside.
   Before a retake, stop the site and restore the snapshot the plan names (A before 02-create, B after it, C after 04-setup).
