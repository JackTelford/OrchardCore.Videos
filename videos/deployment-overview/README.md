# Deployment: Move Configuration and Content Between Sites

**YouTube:** not published yet · **Length:** 4:25 · **Feature:** OrchardCore.Deployment ([Deployment documentation](https://docs.orchardcore.net/en/latest/reference/modules/Deployment/))

An overview of the Deployment module for site owners and administrators. It explains why work is moved between sites
with a package instead of by hand, shows where the module is in the admin, and follows one setting from a source site
to a staging site: the Page size is changed on the source site, a deployment plan with a Site Settings step exports it
as a zip package, and Package Import loads the package on the staging site. A last chapter covers the three permissions
of the module and why Import Data is Security Critical. The title, description, tags and chapters YouTube shows are kept
in the video's metadata file.

## Recording it again

The video is recorded on a fresh Orchard Core site with fake data only, so it needs no mask script. It uses two sites
in one running application: the Default tenant is the source site, and a second tenant named Staging, with the URL
prefix staging, is the destination. Both are provisioned by Auto Setup with the Blog recipe, which enables the
Deployment feature on both, and both have the administrator admin with the address admin@example.com and a password
chosen outside git.

1. Set up both sites, then seed them off camera: the Page size is 25 on the source site and 10 on the Staging site, and
   neither site has a deployment plan. Sign in to each site once and keep the two sessions in files that git ignores.
2. Stop the site and copy its data folder aside. Restore that copy before every full take.
3. Narrate the video, then record the three recordings in this order: the one that finds the module, the one that
   builds and runs the plan, and the one that imports the package. The plan recording saves the downloaded zip file in a
   folder outside the repository, and the import recording uploads it, so the import recording depends on the plan
   recording. The plan and import recordings change the sites, so restore the copy before recording them again.

The site settings, the environment variables, the exact actions of every step, and the checks for private data are in
recording-plan.md. Questions that need a running site to answer are in OPEN-QUESTIONS.md.
