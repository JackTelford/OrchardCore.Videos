# Secrets in Azure Key Vault

**YouTube:** not published yet · **Length:** 4:28 · **Feature:** [OrchardCore.Secrets.AzureKeyVault](https://docs.orchardcore.net/en/latest/reference/modules/Secrets/)

A video for module and host developers. It shows the second secret store of the Secrets module, the one that keeps each
secret as an entry in an Azure Key Vault: the host registers the Azure client, the configuration selects it, and the
feature is enabled. A short recording creates a secret in the vault and shows its store in the Secrets list. Slides then
show how the entries are named in the vault, how code reads and saves a secret in a given store and shares the client, and
how the client is configured for a local emulator. Moving secrets between stores and the secret inputs of settings pages
are covered by other videos of the Secrets series. The title, description, tags and chapters that YouTube shows are kept in
the video's metadata file.

## Recording it again

Only one chapter is recorded, `04-create`; the other chapters are slides and cards. A real vault would show a real Azure
account, so the video is recorded against a local Azure Key Vault emulator, on a **fresh site** with fake data only, so it
needs no mask script. The step-by-step plan, with the stand-in for the vault, every click, every value to type, and the
snapshot to restore, is in [recording-plan.md](recording-plan.md). What could not be checked without a running emulator is
listed in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

1. Start a local Azure Key Vault emulator, as the Secrets documentation describes, without persistence. Its certificate is
   not added to the trust store of the machine.
2. Run a throwaway web host, outside both repositories, that mirrors the CMS web project of an Orchard Core checkout and
   registers a keyed secret client for the emulator. Give it a data folder outside both repositories, port 5010, and Auto
   Setup: the Blog recipe, the site name **Orchard Core**, the time zone America/Los_Angeles, and the user admin with the
   email admin@example.com. The password is chosen outside the repository and handed to the recorder through the video
   password variable; it is never written in the repository. The Azure Client and Name Prefix options of the feature are set
   in the host's environment.
3. In **Tools, Features**, enable **Secrets**, but not **Secrets - Azure Key Vault**: the clip enables that one on camera.
   Create the one fake secret of the plan in the Database store, sign the recorder in, then stop the site and copy its data
   folder and sign-in aside as the snapshot.
4. Narrate, then record `04-create`. Before every other take, restore the snapshot **and start a fresh emulator**: the
   vault is not part of the snapshot, and the form refuses a name that exists in any store.
5. Remove the emulator container, its certificate folder and the throwaway host when the recording is finished.
