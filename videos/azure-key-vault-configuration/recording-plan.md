# Recording plan: Azure Key Vault Configuration

This video has no browser recording. Its two cards and fifteen slides are made from `storyboard.json` and `slides.py`
by `narrate.py` and `assemble.py`; the recorder is not used, and no clip method is written. Nothing here needs a site,
a sign-in, a snapshot or a seed.

## Why there is no clip

1. **No screen is part of the story.** `OrchardCore.Configuration.KeyVault` is a library that the host calls in
   `Program.cs`. It has no module, no admin menu, no permission and no view
   (`src/OrchardCore/OrchardCore.Configuration.KeyVault/` holds only `Extensions/`, `Services/`, a project file and
   `ConfigurationSchema.json`). The code, the settings and the secret names are shown on slides, as the programme does
   for a developer video.
2. **No stand-in for the vault is verified.** A recording would need a vault. A real one means a real Azure account,
   subscription and vault host on screen, which the programme forbids. The emulator that the Secrets documentation
   describes needs a client option (`DisableChallengeResourceVerification`) that `AddOrchardCoreAzureKeyVault` does not
   set or expose (`src/OrchardCore/OrchardCore.Configuration.KeyVault/Extensions/AzureKeyVaultConfigurationExtension.cs:88`
   passes only the manager and the reload interval). Whether an emulator can serve this provider is listed in
   `OPEN-QUESTIONS.md`; until a person confirms it, the video shows no running site.

## The site it needs

None.

| | |
|---|---|
| Auto Setup recipe, tenant, time zone | Not applicable: no site is started. |
| Features to enable before recording | Not applicable. The provider is not a feature. |
| Settings | Not applicable. The slides show example settings only. |
| External services and stand-ins | None. No Azure account, no vault, no emulator. |
| Seed data | None. The only values on the slides are invented: the vault name `example-vault`, the tenant name `MyTenant`, and the placeholder names `Part---Name` and `Part-Name`. |
| Sign-in (`state.json`), `VIDEO_PASSWORD`, `VIDEO_INIT_SCRIPT` | Not used. |

## Order, dependencies and snapshots

Nothing is recorded, so no clip depends on another and no clip changes a site. There is no snapshot to restore
before a new take.

## How to produce the video

From the repository root, with `pip install pillow pygments imageio-ffmpeg "edge-tts>=7.3.1"`:

```bash
# 1. Check the spoken forms of the storyboard first (see OPEN-QUESTIONS.md, "Spoken forms"), for example:
#    --save takes a folder outside the repository, audio is never committed.
python .agents/skills/orchardcore-video-producer/scripts/pronounce.py "Set {} to the full address of the vault." "VaultURI" "Vault U.R.I." --save <folder outside the repository>

# 2. Narrate: writes videos/azure-key-vault-configuration/audio/ and durations.json.
python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/azure-key-vault-configuration

# 3. Assemble: draws the slides from slides.py, adds the cards, the narration and the captions.
python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/azure-key-vault-configuration
```

To look at one slide before assembling, from `videos/azure-key-vault-configuration` with the skill's `scripts`
folder on `PYTHONPATH`:

```bash
python -c "import slides; slides.slide('names-1', 'Name the secrets', 'Chapter 5').save('x.png')"
```

Delete `x.png` afterwards: images are never committed.

## The steps

Chapters 01 to 06 are slides, chapters 00 and 99 are cards. Every text on a slide is 21 px or more. The labels come from
the code of the Orchard Core source (commit `92db4a83`), as noted on each line.

### Chapter 00, opening card

- `intro`: card "Azure Key Vault Configuration", subtitle "Keep settings out of files". Nothing else.

### Chapter 01, Settings in a vault (clip `01-why`, slides)

- `why-1`: heading "Where settings are kept", three cards: "appsettings.json files", "Environment variables", "Azure Key
  Vault". The first two are bright, the third is dimmed.
- `why-2`: the same slide with only the third card, "Azure Key Vault", bright.
- `why-3`: heading "Two ways to use a vault", two cards: "Azure Key Vault configuration" (a library in the host, no
  screen) and "Secrets - Azure Key Vault" (a feature of the Secrets module). The second title is the feature name of
  `src/OrchardCore.Modules/OrchardCore.Secrets.AzureKeyVault/Manifest.cs:10`.

### Chapter 02, Add the provider (clip `02-host`, slides)

- `host-1`: heading "Reference the package, then call the method". A terminal panel with the install command of the
  package, and a `Program.cs` panel that calls `builder.Configuration.AddOrchardCoreAzureKeyVault();` after
  `WebApplication.CreateBuilder(args)`. The `Program.cs` follows `src/OrchardCore.Cms.Web/Program.cs:3-9`; the call and
  the `using` come from `Extensions/AzureKeyVaultConfigurationExtension.cs:56`.
- `host-2`: heading "The same method, three targets". A `Program.cs` panel with the Generic Host example of
  `src/docs/reference/modules/KeyVault.Azure/README.md:39-52`, trimmed, and three cards: "IHostBuilder" (the Generic
  Host), "IWebHostBuilder" (the web host), "ConfigurationManager" (`builder.Configuration`). The three overloads are
  at `Extensions/AzureKeyVaultConfigurationExtension.cs:20`, `:38` and `:56`.

### Chapter 03, Configure the vault (clip `03-config`, slides)

- `config-1`: heading "Point to the vault". An `appsettings.json` panel with `OrchardCore`, `KeyVault`, `Azure`,
  `KeyVaultName` (`example-vault`), `VaultURI` and `ReloadInterval`; the lines `KeyVaultName` and `VaultURI` are bright.
  Two cards on the right: "KeyVaultName", "VaultURI". Setting names: `ConfigurationSchema.json:15-23` and
  `Extensions/AzureKeyVaultConfigurationExtension.cs:81,93,105`; `VaultURI` first: `:93-103`.
- `config-2`: heading "Where the settings are read from", three cards: "Sources registered before the call", "The
  application configuration only", "Neither a name nor an address". Sources: `Extensions/AzureKeyVaultConfigurationExtension.cs:26-29`
  (the comment on the providers already added); application only: `ConfigurationSchema.json:13`; exception:
  `Extensions/AzureKeyVaultConfigurationExtension.cs:107-110`.
- `config-3`: the same JSON panel with only the `ReloadInterval` line bright (value `300`), and two cards: "ReloadInterval",
  "Empty". Source: `ConfigurationSchema.json:23-30`, `Extensions/AzureKeyVaultConfigurationExtension.cs:81-84`; the
  documentation says the same at `src/docs/reference/modules/KeyVault.Azure/README.md:29-30`.
- `config-4`: heading "Order of the sources", two cards: "Sources registered before the call" and "Azure Key Vault" (its
  values replace the same keys of the project's `appsettings.json`; environment variables are not named on this card,
  see `OPEN-QUESTIONS.md` section 3). Source: `Extensions/AzureKeyVaultConfigurationExtension.cs:88`; "last wins" is stated in
  `src/docs/reference/modules/Configuration/README.md`, section "Configuration Sources Order".

### Chapter 04, Choose the credential (clip `04-auth`, slides)

- `auth-1`: heading "The default credential". Two code panels titled `AzureKeyVaultConfigurationExtension.cs`: the
  signature of the `ConfigurationManager` overload with its optional `TokenCredential` (`:56-57`), and the lines of the
  private method that default the credential and call `AddAzureKeyVault` (`:68-70,86,88`); in that second panel only
  the line `tokenCredential ??= new DefaultAzureCredential(includeInteractiveCredentials: true);` is bright.
- `auth-2`: heading "Another credential, and access", three cards: "Another credential", "Access", "Development". The
  development line repeats `src/docs/reference/modules/KeyVault.Azure/README.md:14`.

### Chapter 05, Name the secrets (clip `05-names`, slides)

- `names-1`: heading "From secret names to configuration keys". A panel with the class `AzureKeyVaultSecretManager`
  (the whole file `Services/AzureKeyVaultSecretManager.cs:6-10`; the lines `GetKey` and `secret.Name.Replace(...)` are
  bright), and two cards: "Two hyphens" (icon `--`), "Three
  hyphens" (icon `---`).
- `names-2`: heading "Examples", three cards: the documentation's name `OrchardCore--Shells--Database--ConnectionString`
  becoming `OrchardCore:Shells:Database:ConnectionString` (`src/docs/reference/modules/KeyVault.Azure/README.md:5-7`),
  then `Part---Name` becoming `Part_Name`, then `Part-Name` staying `Part-Name`.
- `names-3`: heading "For every tenant, or for one", two cards: `OrchardCore--Media--Azure--ConnectionString` for every
  tenant, and `OrchardCore--MyTenant--Media--Azure--ConnectionString` for the tenant `MyTenant`. Sources:
  `src/docs/reference/modules/Configuration/README.md` ("Tenant Post-configuration" and the environment variable section),
  `src/OrchardCore/OrchardCore/Shell/ShellSettingsManager.cs:115,276`.

### Chapter 06, In short (clip `06-recap`, slides)

- `recap-1`: heading "The steps, in the order you do them", four cards: "Create the secrets", "Add the provider", "Point it
  at the vault", "Allow the identity".

### Chapter 99, closing card

- `outro`: card "Azure Key Vault Configuration", subtitle "docs.orchardcore.net". Nothing else.

## Checks for private data

Before the video is published:

- The slides and the narration hold no real vault name, vault address, Azure subscription, tenant ID, client ID, user,
  email address or token. The only vault name is `example-vault`, and no full vault host name is written. If a slide is
  ever changed, search `slides.py`, `storyboard.json` and `video.json` for `azure.net`, `@`, `http` and `Bearer`: the
  only matches allowed are the links of `video.json`.
- No Azure portal, no terminal of a real machine and no real `appsettings.json` is shown. The `Terminal` panel holds
  the install command of the package and nothing else.
- After `assemble.py`, extract a few frames with ffmpeg (a card, a slide with code, a slide with cards) and read them.
- Search the `.vtt` and `.srt` files for the same words as above.
