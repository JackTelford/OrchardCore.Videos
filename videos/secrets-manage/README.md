# Secrets: Store Passwords and API Keys Safely

**YouTube:** not published yet · **Length:** 5:52 · **Feature:** [OrchardCore.Secrets](https://docs.orchardcore.net/en/latest/reference/modules/Secrets/)

A video for administrators and site owners. It explains why the Secrets module keeps passwords and API keys together,
turns the feature on, and creates a text secret, an RSA key, and the form of an X.509 certificate. It reviews the list
with its search, its filters and its expiration warnings, replaces a value, deletes secrets one at a time and in bulk,
and selects a secret in the SMTP settings instead of typing a password. The permissions that control access close the
video. Moving credentials that settings already hold, moving secrets between stores, Azure Key Vault, and the developer
side of the module are covered by other videos of the Secrets series. The title, description, tags and chapters that
YouTube shows are kept in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The step-by-step plan, with
every click, every value to type, the fake secrets, and the snapshots to restore, is in [recording-plan.md](recording-plan.md).

1. Run the CMS web project of an Orchard Core checkout with its data folder outside both repositories, on port 5010,
   provisioned by Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone America/Los_Angeles, and
   the user admin with the email admin@example.com. The password is chosen outside the repository and handed to the
   recorder through the video password variable; it is never written in the repository.
2. Before recording, enable the **SMTP Email Provider** feature in **Tools, Features**, which also enables **Email**.
   Leave **Secrets** disabled: the first clip enables it on camera.
3. Sign the recorder in, then narrate. Record the clips in this order, each from the site as the previous one left it:
   02-enable, 03-create, 04-types, then seed four fake secrets outside the capture, then 05-review,
   06-update, 07-delete and 08-settings. The plan names the snapshots of the site to make along the way and to
   restore before another take.
4. The slide chapters (01-why, 09-access) and the opening and closing cards are not recorded.

The expiration dates of the fake secrets are computed from the recording day, so seed them again on the day of another
take.
