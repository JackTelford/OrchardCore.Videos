# Build a Company Website With the Agency Theme

**YouTube:** not published yet · **Length:** 7:31 · **Feature:** `TheAgencyTheme` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Flow/BagPart/))

The video builds a company website from the Agency setup recipe. It shows the landing page, whose sections are Bag parts of
the Flows module, edits one of them, enables the Forms module to build a contact form on its own page, and adds that
page to the main menu with a Content Menu Item. The title, description, tags and chapters YouTube shows are in
video.json.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The full procedure, with every
action of every step, the commands and the environment, is in [recording-plan.md](recording-plan.md); what could not be
checked without a running site is in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Run the CMS web application of Orchard Core with an empty data folder outside both repositories, on port 5010, and
   let Auto Setup install it: recipe **Agency**, site name **Alder Creek Studio**, time zone America/Los_Angeles, user
   admin with the e-mail admin@example.com. The password is chosen outside git and passed to the recorder in its
   environment.
2. Seed the site by hand, outside the capture: rename the three team members of the landing page, delete its four
   clients, publish, replace the hint of the Twitter field of the Team Member part (it names a real person), and check
   that the Forms feature is off. Stop the site and copy its data folder aside (snapshot A).
3. Sign the recorder in, narrate the video, and record the clips in this order: the tour of the starting site, the
   editing of the landing page, the enabling of Forms, the contact form page, the menu, and the finished site.
4. Four clips change the site: the editing of the landing page adds a service and publishes the landing page, enabling
   Forms turns the feature on, the contact form clip creates the page Start a project, and the menu clip adds the menu
   item. Copy the data folder aside after each of them, and restore the copy taken before a clip to record it again.
