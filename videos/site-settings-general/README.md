# Site Settings: Name, Time Zone, Page Size and More

**YouTube:** not published yet · **Length:** 5:17 · **Feature:** OrchardCore.Settings ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Settings/))

This video opens Settings, General, the page that holds the site-wide options of an Orchard Core site. It goes through
the Site tab (site name, page title format, base url, default time zone, page size and page size selection) and the
Resources and Cache tabs (scripts, stylesheets, CDN, resource debug mode, cache mode), and names the permission that
opens the page. The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video was recorded on a **fresh site** with fake data only (the site name Harbor Lane Journal, and ten articles named
Harbor Lane Note 1 to Harbor Lane Note 10), so it needs no mask script. It needs no feature enabled beyond what the Blog
recipe enables. recording-plan.md has the exact actions, selectors and source references; this is the outline.

1. Run the Orchard Core CMS web application with its own empty data folder outside both repositories, on port 5010,
   provisioned by Auto Setup: the Blog recipe, site name Orchard Core, time zone America/Los_Angeles, user admin, email
   admin@example.com, and a password chosen outside git.
2. Sign the recorder in, then seed the site: import a recipe that creates the ten published articles, so that Content
   Items lists more than ten items (15 are expected: the five that the Blog recipe leaves in the list, and the ten).
   recording-plan.md gives the recipe and the check of the starting values of Settings, General.
3. Stop the site and copy its data folder and the saved sign-in aside. Restore that copy before every full take.
4. Narrate, then record the clips in the order of the video, in one take. Three clips save settings and leave the site
   as the next clip expects: the site name, then the time zone, then the page size with its selection and options.
   recording-plan.md has the table of what each clip changes and how to start a single clip again.
