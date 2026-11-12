# Tenants: Edit, Disable, Reload and Remove

**YouTube:** not published yet · **Length:** 4:52 · **Feature:** `OrchardCore.Tenants` ([Tenants](https://docs.orchardcore.net/en/latest/reference/modules/Tenants/))

An admin video about the Tenants list of the Default tenant, found under Multi-Tenancy. It shows how to search and
filter the list, what the state of a tenant allows, how to edit a tenant, how to disable and enable it, how to reload
it, how to disable several tenants at once, and how to remove a tenant that is disabled or not set up. Creating and
setting up a tenant, and feature profiles, are the subject of other videos. The title, description, tags and chapters
YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with invented tenants only, so it needs no mask script. The step by step
recipe, with the exact actions of every step, is in `recording-plan.md`; read `OPEN-QUESTIONS.md` as well.

1. **Run Orchard Core** (the CMS web project of a checkout of OrchardCMS/OrchardCore) with its own empty
   application data folder, outside both repositories, on port 5010 of localhost. Auto Setup installs the Default
   tenant with the **Software as a Service** recipe, the site name **Orchard Core**, the time zone
   America/Los_Angeles and the user admin. The password is chosen outside git and passed to the recorder.
   The site must allow tenant removal; `recording-plan.md` says how.
2. **Seed the tenants**, also with Auto Setup: Acme, Harbor, Juniper and Maple are set up by one request each, and
   Pilot is never requested, so it stays **Uninitialized**. No feature needs to be enabled, and the Tenant Feature
   Profiles feature must stay off.
3. **Sign the recorder in**, then stop the site and copy its application data and the sign-in aside. Restore that
   snapshot before every full take.
4. **Narrate, then record** the clips in this order: `01-list`, `03-edit`, `04-disable`, `05-reload`, `06-bulk`,
   `07-remove`. Four of them change the site (`03-edit`, `04-disable`, `06-bulk`, `07-remove`). `07-remove` removes
   Maple, which `06-bulk` disabled: to record it alone, disable Maple first and wait 15 seconds.
