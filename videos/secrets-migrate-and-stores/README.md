# Secrets: Move Existing Credentials and Change Stores

**YouTube:** not published yet · **Length:** 4:25 · **Feature:** [OrchardCore.Secrets](https://docs.orchardcore.net/en/latest/reference/modules/Secrets/)

The video shows the two Secrets screens that move what a site already has. Migrate Secrets turns the credentials that
features keep in their own settings, here the SMTP password and the GitHub Authentication client secret, into secrets.
Secret Stores moves every secret of one store to another, and the Move button of the Secrets list moves a single
secret. The title, description, tags and chapters YouTube shows are in the video's metadata file; the instructions for
the recorder, clip by clip, are in the recording plan.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. It is an admin video: the
capture shows the admin UI only. It needs two secret stores, Database and AzureKeyVault, so the site runs on a
throwaway host that registers an Azure Key Vault client for a local emulator. The recording plan describes the host,
the emulator and the seed data, and the open questions file lists what a person must confirm first.

1. Start the Azure Key Vault Emulator in Docker, empty and without persistence, and build the throwaway host project
   outside both repositories, from the web project of OrchardCore, as the recording plan describes.
2. Run the host with its data folder in an empty location outside both repositories, on port 5010, provisioned by Auto
   Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user admin, email
   admin@example.com, and a password chosen outside git and passed through the recording environment.
3. Enable the features **Secrets**, **Secrets - Azure Key Vault**, **SMTP Email Provider** and **GitHub Authentication**.
   Then save the SMTP settings and the GitHub Authentication settings with the fake values of the recording plan,
   keeping both passwords on **User-specified value**.
4. Sign the recorder in, stop the site, and copy its data folder and the sign-in file aside together with an empty
   emulator: that snapshot is restored before every full take.
5. Narrate the video, then record the clips in this order, each one starting from the state the previous one left:
   01-before, 02-migrate, 03-check, 05-move-all, 06-move-one. The clips 02-migrate, 05-move-all and 06-move-one change
   the site, so restore the snapshot (and an empty emulator) before another take, and play the earlier clips again off
   camera.
