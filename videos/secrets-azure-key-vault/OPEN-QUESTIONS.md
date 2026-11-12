# Open questions: secrets-azure-key-vault

What the author could not confirm from the source or the documentation, and what a person must check on a running site
(the checkout at commit 92db4a83, the throwaway host of `recording-plan.md`, a local emulator) before or while recording.
The narration states only what the source and the documentation show; each item below is a stand-in detail, a screen detail
or an Azure behavior that neither can prove. The package versions of the checkout were not available to the author, and no
code or service could be run, so nothing here was compiled or tried. File references follow the convention of
`recording-plan.md`: a bare `README.md:<n>` is `src/docs/reference/modules/Secrets/README.md`, and a bare `AdminController.cs`,
`Index.cshtml`, `Edit.cshtml` or `AzureKeyVaultSecretStore.cs` is in the Secrets or Secrets - Azure Key Vault module, as described there.

## The recording stand-in

1. **Accepting the emulator's certificate in the process only.** The documentation says to trust the emulator's certificate
   (`src/docs/reference/modules/Secrets/README.md:420`), and the emulator's own setup page installs it as a trusted root.
   `references/recording.md` allows that for the process only, never for the machine. The plan therefore builds the
   `SecretClient` by hand in a throwaway host and pins the certificate there. The names used in that sketch are from memory
   of the Azure SDK, not compiled: `HttpClientTransport(HttpClient)`, `EnvironmentCredentialOptions.Transport` and
   `.DisableInstanceDiscovery`, `SecretClientOptions.Transport` and `.DisableChallengeResourceVerification`,
   `X509Certificate.GetCertHashString()`. The reviewer found most of them in the XML documentation of the Azure packages in the
   local NuGet cache of the author's machine (not the versions the checkout pins, which were not available, since no
   `Directory.Packages.props` came with the copy of the source): `HttpClientTransport(HttpClient)` and `ClientOptions.Transport`
   in Azure.Core 1.63.0, `EnvironmentCredentialOptions.DisableInstanceDiscovery` in Azure.Core, present in the cached 1.53.0 to 1.63.0 and absent
   in 1.51.1 and earlier (the identity types live in Azure.Core in those versions), and `SecretClientOptions.DisableChallengeResourceVerification` in
   Azure.Security.KeyVault.Secrets 4.11.0 and 4.11.2. `GetCertHashString()` returns the thumbprint in upper case, so the sketch
   compares without regard to case. Confirm that they compile against the Azure packages the checkout restores, that the
   pinned handler also serves the credential's token request (the same `Transport` is passed to both), and that
   `UseProxy = false` is enough with `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address (`NO_PROXY=localhost,127.0.0.1` is
   the second line of defense). If pinning cannot work, record on a disposable machine (a virtual machine or Windows Sandbox)
   whose trust store is thrown away afterwards; this is raised as a question about the specification.
2. **The emulator itself.** The documentation gives its settings but no command to start it (`README.md:374-424`); the image,
   the ports and the certificate generation are in the emulator's own setup page, which the documentation links. Confirm the
   image runs without persistence, that its certificate has the name `localhost`, and that its OAuth endpoint accepts the
   placeholder credentials of `README.md:410-413` with `AZURE_AUTHORITY_HOST` set (`README.md:418`). Confirm also that the
   emulator keeps and returns what the store reads back: the tags (`OrchardTenant`, `OrchardPrefix`, `OrchardName`,
   `OrchardType`, `OrchardDescription`) and the content type on a get, and the tags on a list, because an entry without them
   is skipped in the list and rejected on a read (`AzureKeyVaultSecretStore.cs:149-155,185-191`), and the creation and update
   times, which decide whether the list shows a last-updated badge (`AzureKeyVaultSecretStore.cs:163-164`,
   `OrchardCore.Secrets/Views/Admin/Index.cshtml:143`). The rehearsal of the plan (create a throwaway secret in the vault,
   see it in the list, open it) answers all of this at once.
3. **Reading the credential variables.** The plan uses `EnvironmentCredential`, which the documentation names for the
   emulator (`README.md:380`), but builds it in code. Confirm that it reads `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`,
   `AZURE_CLIENT_SECRET` and `AZURE_AUTHORITY_HOST` from the host process when constructed that way.
4. **The id of the Enable button.** `FeatureEntry.Buttons.SummaryAdmin.cshtml:40` builds it as `btn-enable-` followed by
   `Html.GenerateIdFromName(featureId)`; the plan expects `btn-enable-OrchardCore_Secrets_AzureKeyVault`. Confirm with
   `Recorder probe`, or use the fallback locator of the plan.
5. **The sidebar.** The ids of the menu entries are not in the source. Confirm that **Tools** opens to **Features** and
   **Security**, and that **Security** lists **Secrets**, **Migrate Secrets**, **Secret Stores** in that order
   (`OrchardCore.Secrets/AdminMenu.cs:33,40,47`).

## Slides

6. **The host registration on the slide `client-1`** is the one of the documentation (`src/docs/reference/modules/Secrets/README.md:319-327`),
   with its two `using` lines and the `SCME0002` pragma. It was not compiled. The documentation does not say which package
   provides `AddKeyedAzureClient`. In the XML documentation of the local NuGet cache it is
   `Azure.Identity.ConfigurationExtensions.AddKeyedAzureClient<TClient, TSettings>(IHostApplicationBuilder, string key, string sectionName)`,
   shipped in the Azure.Core assembly, present in the cached 1.53.0 to 1.63.0 and absent in 1.51.1 and earlier (hence
   `using Azure.Identity;`, although the code names no type of that package), and `SecretClientSettings` is in Azure.Security.KeyVault.Secrets 4.11.0 and later (hence the second `using`).
   The text `SCME0002` is in Azure.Core 1.63.0 and in Azure.Security.KeyVault.Secrets 4.11.2, which fits the pragma. The checkout's
   pinned versions were not found, so confirm that it restores an Azure.Core version that has the method (1.53.0 is the oldest cached version that does) and
   Azure.Security.KeyVault.Secrets 4.11.0 or later, and that the pragma still names the right diagnostic; update the slide, not the narration, if they change.
7. **The vault address.** The slide shows `<the URI of your vault>`, because a real vault address ends in an Azure domain
   that the programme's rules do not allow. Confirm that this placeholder is acceptable (specification question).

## Screen details to confirm

8. **The Store list** (`create-1`, `create-2`). The form shows it when two writable stores exist, with the stores in the order
   of their registration (`OrchardCore.Secrets/Controllers/AdminController.cs:245-253`, `Views/Admin/Edit.cshtml:39-47`).
   The narration only says to choose AzureKeyVault; confirm which store is preselected and that the options read
   **Database** and **AzureKeyVault**.
9. **Enabling restarts the tenant** (`enable-2`). Confirm that the redirect to the Features page, with the toast "The feature
   Secrets - Azure Key Vault was enabled.", arrives within the first seconds of the step, and that the first load of
   **Secrets** afterwards, which makes the first request to the emulator, finishes without an error.
10. **The search** (`enable-1`). The search text is the name, the id and the description of a feature
    (`OrchardCore.Features/Drivers/FeatureEntryDisplayDriver.cs:20`); the source holds one manifest that mentions Key Vault
    (`OrchardCore.Secrets.AzureKeyVault/Manifest.cs`). Confirm that "Key Vault" leaves exactly one row.
11. **The toast after the save** (`create-2`, `create-3`). The save makes several calls to the emulator, and the toast of
    "Secret created successfully." lasts 5 seconds (`OrchardCore/OrchardCore.DisplayManagement/Notify/NotifierExtensions.cs:71`). Confirm that it is on screen while `create-3`
    starts, or take the second sentence of that step as the moment to move the highlight.
12. **The store badge** (`create-3`). Every store uses the same database icon in its badge (`Index.cshtml:114`); the narration
    says only "a badge for its store". Confirm the text **AzureKeyVault** is readable at the size of the recording.
13. **The Edit page** (`create-4`). Confirm that the hint "To keep this secret in another store, use the Move action." is
    drawn under the **Store** row, and that opening the secret reads it from the vault without an error
    (`AdminController.cs:327-379`).

## Behavior that the narration states with care

14. **The default store** (`code-2`: "the secret goes to the default store, which is the database"). The manager saves to the
    first writable store (`OrchardCore.Secrets.Core/Services/SecretManager.cs:133-134`), and the release notes name the
    database as the default store (`src/docs/releases/4.0.0.md:117`, "The default store is the database"). The order of the
    two stores follows from the order of the startups: the container factory orders them by module dependency, with an `Order`
    of 0 by default and a stable sort (`OrchardCore/OrchardCore/Shell/Builders/ShellContainerFactory.cs:120-122`), and the Key
    Vault feature depends on Secrets (`OrchardCore.Secrets.AzureKeyVault/Manifest.cs:14`), whose startup registers the database
    store (`OrchardCore.Secrets/Startup.cs:24`); neither startup overrides `Order`. Confirm with both features enabled
    that `SaveSecretAsync(name, secret)` without a store name lands in the database (its badge reads **Database**), and that
    the **Store** list of the form shows **Database** first.
15. **Reading by name** (`store-2`: "Settings and code refer to a secret by name"). `GetSecretAsync(name)` asks the stores in
    order and returns the first hit (`SecretManager.cs:27-39`), so when the same name exists in two stores, the first store
    wins. The Add Secret form refuses a name that exists in any store (`AdminController.cs:295-299`), so this only happens
    after a direct write, so the narration of `create-3` says what the form refuses and does not call names unique. Settings
    keep only the secret name, never a store: the secret input stores `secretName` (`OrchardCore.Secrets.Abstractions/SecretInputExtensions.cs:44-55,87-88`),
    and the `SecretBinding` type that pairs a name with a store (`SecretBinding.cs`) is not used by any code of the source.
    The narration does not claim more than the first sentence of `store-2`.
16. **Deletion** (`names-4`). The store calls `StartDeleteSecretAsync` and never a purge (`AzureKeyVaultSecretStore.cs:128`);
    the documentation says the delete is recoverable under the soft-delete policy and that Azure's retention rules may
    prevent reusing a name (`README.md:370`). Whether a given vault has soft delete is a property of the vault, which the
    source cannot show.
17. **Expiration in the vault.** The store passes the expiration date of a secret to the vault entry
    (`AzureKeyVaultSecretStore.cs:106,165`). Whether Azure then refuses to return an expired value is outside the source, so
    the narration says nothing about expiration for this store, and the clip sets no date.
18. **Credentials** (`client-2`, `client-3`). "Key Vault does not support API keys" and "a managed identity needs no secret in
    the configuration" are statements of the documentation (`README.md:331`, `README.md:140`); the source cannot confirm
    Azure's side of them. The same holds for `local-2` ("both default to false", `README.md:422`): the two flags belong to the
    Azure SDK, whose defaults the source does not show.
19. **A failing store can fail more than the Secrets pages** (`config-1`, `enable-1`; plan, checks before every take). The
    secret manager is a singleton that builds every registered store (`OrchardCore.Secrets.Core/Services/SecretManager.cs:16-24`).
    The Key Vault store reads its options and its client in its constructor (`AzureKeyVaultSecretStore.cs:34-42`;
    `OrchardCore.Secrets.AzureKeyVault/Startup.cs:14-24`), and the expiration filter registered for MVC asks for the secret
    manager in its constructor (`OrchardCore.Secrets/Startup.cs:28`, `OrchardCore.Secrets/Filters/SecretExpirationWarningFilter.cs:24-35`).
    So with the feature enabled and a missing `AzureClient`, a reserved key or an unregistered client, the first request that
    builds the filter may throw, including the Features page and the pages of the database store. The narration only says that
    the store fails with an explicit error, which the options validation and the alias factory support. Confirm on a running
    host which pages fail, and that stopping the host and restoring Snapshot A is the way back.

## Pronunciation (check with `scripts/pronounce.py`, then adjust `"spoken"` in the storyboard)

- `"Secrets - Azure Key Vault"` is spoken "Secrets, Azure Key Vault", so the voice pauses at the hyphen of the label; the
  captions keep the label as the page writes it.
- `ISecretStore` is spoken "I Secret Store"; `AzureKeyVault` "Azure Key Vault"; `AzureClient` "Azure Client"; `NamePrefix`
  "Name Prefix"; `SecretClient` "Secret Client"; `SecretClientServiceKey` "Secret Client Service Key"; `FromKeyedServices`
  "From Keyed Services"; `GetSecretAsync` "Get Secret Async"; `SaveSecretAsync` "Save Secret Async". Check that the voice says
  "Async" as "a-sink", and that "Keyed" is not read as "key-ed" in a way that distracts.
- "localhost" (`local-1`) and "emulator" are read as ordinary words.

## Not covered on purpose

The Azure Key Vault configuration provider (`src/docs/reference/modules/KeyVault.Azure/README.md`) loads settings from a vault
and is a different feature from the secret store of this video. Moving secrets between stores, and the secret inputs of
settings pages, belong to other videos of the Secrets series.
