# Tenants: Run Many Sites From One Application

**YouTube:** not published yet · **Length:** 4:33 · **Feature:** [OrchardCore.Tenants](https://docs.orchardcore.net/en/latest/reference/modules/Tenants/)

The video explains what a tenant is, what the features of the Tenants module are, and who can manage tenants. It then
enables the Tenants feature on the Default tenant, reads the Multi-Tenancy, Tenants list with its search, filters and
bulk actions, shows which buttons each tenant state has, and looks at the Add Tenant form without saving anything. It is
an introduction: creating, managing and limiting tenants are covered in the other videos of the series. The title,
description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The recording plan gives the
exact settings, values, selectors and clip steps.

1. Run the web project of Orchard Core on http://localhost:5010, with an empty data folder outside both repositories,
   provisioned by Auto Setup: the Default tenant (Blog recipe, site name Orchard Core, time zone America/Los_Angeles,
   user admin) and two more tenants, Bakery (Blog recipe, URL prefix bakery) and Studio (Agency recipe, URL prefix
   studio). Request the home page first, which installs the Default tenant and creates the other two, and then the
   address of Bakery and of Studio once each, so that Auto Setup installs them.
2. Sign the recorder in as admin, with the password chosen outside git. Keep the admin lists in their default layout.
3. Copy the data folder aside (snapshot A). Record the clip of chapter 3, which enables the Tenants feature. To record it again, restore snapshot A.
4. Seed the rest outside the capture: edit Bakery and Studio (category, description, and the host name of Studio),
   disable Studio, and add the Workshop tenant without setting it up. Copy the data folder aside again (snapshot C).
5. Record chapters 4, 5 and 6 from snapshot C, in any order: they do not change the site.
