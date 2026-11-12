# Azure Key Vault Configuration: Keep Settings Out of Files

**YouTube:** not published yet · **Length:** 3:51 · **Feature:** `OrchardCore.Configuration.KeyVault` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/KeyVault.Azure/))

The Azure Key Vault configuration provider reads the secrets of a vault into the application configuration, so the
settings of an Orchard Core site, such as a connection string, can stay out of `appsettings.json` files and environment
variables. The video is for developers: it shows where to reference the package and call `AddOrchardCoreAzureKeyVault`,
the settings that point to the vault, the default credential and how to pass another one, and how the names of the
secrets map to configuration keys. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is made of two cards and fifteen slides, with code taken from the Orchard Core source and its documentation. It
has **no browser recording**: it needs no site, no sign-in, no `App_Data` copy, no Azure account and no vault, and the
recorder is not used. `recording-plan.md` says why, and describes every slide. The only values on the slides are
invented (`example-vault`, `MyTenant`), so it needs no mask script.

1. Install the tools once: `pip install pillow pygments imageio-ffmpeg "edge-tts>=7.3.1"`.
2. Read `OPEN-QUESTIONS.md`: it lists the spoken forms to confirm with `scripts/pronounce.py` before narrating.
3. Narrate, then assemble, from the root of this repository:

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/azure-key-vault-configuration
   python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/azure-key-vault-configuration
   ```

   `assemble.py` draws the slides with `slides.py`. No clip depends on another, and nothing changes a site, so the
   order of the steps is the order of the storyboard.
4. Look at a few frames of the result (a card, a slide with code, a slide with cards) and search the captions for private
   data, as `recording-plan.md` ends.
