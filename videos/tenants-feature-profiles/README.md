# Tenant Feature Profiles: Limit the Features of Each Tenant

**YouTube:** not published yet · **Length:** 4:17 · **Feature:** [OrchardCore.Tenants.FeatureProfiles](https://docs.orchardcore.net/en/latest/reference/modules/Tenants/)

By default, every tenant of an Orchard Core site is offered the same features. The video shows how a feature profile,
created on the Default tenant, limits what a tenant is offered. It explains the Exclude and Include rules, enables the
Tenant Feature Profiles feature, creates a profile, assigns it to a tenant, and compares the Features pages of three
tenants. The title, description, tags and chapters YouTube shows are in the video's metadata file, next to this one.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. The exact actions of every
step are in recording-plan.md; what could not be confirmed from the source is in OPEN-QUESTIONS.md.

1. Build the web project of Orchard Core and run it with an empty App_Data folder outside both repositories, on
   http://localhost:5010, provisioned by Auto Setup with three tenants. No Auto Setup tenant is given a feature profile.
   - The Default tenant: recipe SaaS, site name **Orchard Core**, time zone America/Los_Angeles, user admin.
   - **Bluebird**: URL prefix bluebird, recipe Blog.
   - **Maplewood**: URL prefix maplewood, recipe Blog.

   Request the home page of each tenant once so that every tenant is set up, then give Bluebird and Maplewood a
   **Description** on their Edit Tenant pages. The seed needs no scene of its own.
2. Sign the recorder in, then stop the site and copy its App_Data folder and the sign-in file aside as the first snapshot.
   The sign-in uses the user admin and a password chosen outside git.
3. Narrate and record in this order, because each recording starts from the state the previous one left: 03-enable,
   04-create, 05-assign, 06-effect. The first three change the site, so copy the site aside after each one and restore
   the right copy before a new take (the table is in recording-plan.md). The fourth only reads.
