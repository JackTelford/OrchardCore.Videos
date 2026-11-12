# Meta Login: Let Users Sign In With Facebook

**YouTube:** not published yet · **Length:** 4:33 · **Feature:** `OrchardCore.Facebook.Login` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Facebook/))

The Meta Login feature adds a Facebook button to the login page of a site. The video shows what to prepare on the Meta
side, how to enable the feature, the two settings pages (Meta App, and Meta under Authentication), and the button on the
login page. It stops before the button is selected, and ends with what the site does when Meta returns the visitor.
The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. It never contacts Meta: the
application ID and the secret key are made up, and no clip selects the Facebook button. The full step-by-step, with the
commands that sign the recorder in, is in `recording-plan.md`; open questions to confirm on a running site are in
`OPEN-QUESTIONS.md`.

1. Build the web project of Orchard Core (`src/OrchardCore.Cms.Web`) and run it on `http://localhost:5010`, with its
   data folder outside both repositories, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time
   zone `America/Los_Angeles`, user `admin`, email `admin@example.com`, a password chosen outside git). Leave every
   feature at the recipe's state: Meta Login, External Authentication and Secrets are disabled.
2. Sign the recorder in as `recording-plan.md` describes. This video has nothing to seed. Stop the site and copy its
   data folder aside as snapshot S0.
3. Narrate, then record the clips in this order, copying the data folder aside after the first two (S1, S2) and
   restoring the right snapshot before another take:

   | Clip | Starts from | Changes the site |
   | --- | --- | --- |
   | `02-enable` | S0 | enables Meta Login and its dependencies |
   | `03-app` | S1 | saves the application identifier and the secret key |
   | `04-login` | S2 | no |
   | `05-signin` | S2, signed out | no |
