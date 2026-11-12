# X (Twitter): Sign In With X and Post From Workflows

**YouTube:** not published yet · **Length:** 5:00 · **Feature:** [OrchardCore.Twitter.Signin](https://docs.orchardcore.net/en/latest/reference/modules/X/)

The video shows the two features of the X (Twitter) module: Sign in with X (Twitter), which adds an X (Twitter) button
to the login page, and X (Twitter) Integration, which stores the keys of an X app and adds the Update X (Twitter)
Status Task to Workflows. It enables the features, enters fake keys, sets up the callback path, shows the login page,
explains what happens at the first sign-in, and builds a workflow that sends a post when a Blog Post is published. It
never signs in with a real X account and never posts. The title, description, tags and chapters YouTube shows are kept
with the video.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Nothing in it is a real
account: the four keys are invented values, and no call to X is made.

1. Build the web project of Orchard Core and run it with its own empty data folder outside both repositories, on
   http://localhost:5010, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone
   America/Los_Angeles, user admin, email admin@example.com, a password chosen outside git). Start it with outbound
   requests blocked (the proxy variables set to an address that does not answer), so that nothing can reach X.
2. Before recording, enable the Workflows feature on the site, outside the capture. The Secrets feature stays
   disabled: the video shows the two secret fields as plain password fields. The Blog recipe provides the Blog Post
   content type that the workflow chapter picks.
3. Sign the recorder in, then copy the site's data folder and the sign-in aside. The plan names the copies to take
   after each clip.
4. Narrate, then record the clips in this order: the chapter that enables the features, the chapter that enters the
   keys, the chapter that sets up Sign in with X, the login page, and the workflow chapter. Each one starts from the
   state the one before it left. All of them change the site except the login page, so restore the copy that the plan
   names before taking any of the others again. The two chapters made of slides need no site.

The exact steps, selectors and fake values of every clip are in recording-plan.md. Questions that only a person with a
running site (and, for two of them, an X developer app) can answer are in OPEN-QUESTIONS.md.
