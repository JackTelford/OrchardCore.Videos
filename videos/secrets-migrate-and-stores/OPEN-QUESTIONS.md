# Open questions: secrets-migrate-and-stores

What was checked is the source of Orchard Core at commit `92db4a83` and `src/docs/reference/modules/Secrets/README.md`.
Nothing below was run: a person confirms each point on a running site before recording, and fixes the narration if a
point turns out differently.

## The second store (the largest risk)

The move chapters need two writable stores, and the only second store in the source is `AzureKeyVault`
(`OrchardCore.Secrets.AzureKeyVault/AzureKeyVaultSecretStore.cs:66`).

1. **The emulator has not been run with this store.** The Secrets documentation describes a third-party Azure Key Vault
   Emulator ("Local development with the Azure Key Vault Emulator") and says to create a text secret with
   `AzureKeyVault` selected as its store, then edit and delete it. Confirm that listing, saving, reading and deleting
   work end to end, including from the Secrets list. `GetSecretInfosAsync` of every store runs when the Secrets list
   loads, when the SMTP and GitHub Authentication settings pages render their secret input (the list of secrets is
   built even while `User-specified value` is selected, `SelectSecretViewComponent.cs:32`), and when Move to Secrets
   saves (`SecretMigrationExtensions.cs`); Secret Stores lists only the selected store. Loading Migrate Secrets itself
   does not list secrets. So the emulator must answer for the seed and for every clip. The container image name, the
   emulator's credential variables and the host packages are in the emulator's own documentation and were not checked
   here.
2. **The certificate.** The Secrets documentation says certificate validation stays enabled and the emulator's
   certificate must be trusted. `references/recording.md` says to accept a self-signed certificate only in the process,
   never in the machine's trust store. On Windows, .NET reads the machine store, so a person must decide how to trust
   the certificate for the recording (a store of the recording account only, or another way) and write it in the
   recording plan.
3. **The tracker note of `secrets-azure-key-vault`** says there is no local emulator for Key Vault. If the emulator
   cannot be used, the alternative is a throwaway store (an implementation of the store interface in the throwaway
   host, in memory, registered under the name `AzureKeyVault`). Its screens would look the same, but it is not Azure
   Key Vault, so a person has to decide whether that stand-in is acceptable before the narration keeps naming
   `AzureKeyVault` and the feature `Secrets - Azure Key Vault`.
4. **The feature must be enabled only on a host that registers the client.** The store takes its client from a keyed
   service when it is built (`OrchardCore.Secrets.AzureKeyVault/AzureKeyVaultSecretStore.cs:35-41`, `Startup.cs`), so a host
   without the registration is expected to fail on the first page that builds the secret manager; confirm this on the
   plain `Cms.Web` before relying on it. The narration says a developer connects the vault, and that a separate video
   covers it (`secrets-azure-key-vault` in the same series): confirm that video still covers it.

## Things the narration says that a running site must confirm

5. **Row order and names.** `list-1` names SMTP Password and GitHub Authentication Client secret without claiming an
   order. Confirm that both rows appear on Migrate Secrets and that their suggested names are `Smtp.Password` and
   `GitHub.ClientSecret` (`SmtpOptionsConfiguration.cs:15`, `GithubConstants.cs:12`).
6. **Both credentials move in one save.** Each driver loads the site settings, and the screen saves both after all the
   secrets (`SecretMigration.UpdateSettingsAsync`). Confirm on the SMTP page and on the GitHub Authentication page that both
   show `Use a secret` with the secret selected after one `Move to Secrets`. `check-2` shows only the SMTP page.
7. **The default store.** `store-1` and `stores-1` assume that `Database` is the first option of `Destination store` and
   `Source store`, and the selected one. The screens take the first writable store of `ISecretManager.GetStores()`
   (`MigrationController.cs:63`, `StoreController.cs:105`), which is the registration order of `ISecretStore`.
   `DatabaseSecretStore` is registered by the Secrets feature (`OrchardCore.Secrets/Startup.cs:24`) and
   `Secrets - Azure Key Vault` declares `Dependencies = ["OrchardCore.Secrets"]`
   (`OrchardCore.Secrets.AzureKeyVault/Manifest.cs`), so the dependency is expected to start first and `Database` to be
   listed first. Confirm on screen that `Database` is the selected option on both pages.
8. **The message text.** `done-1` says "2 credentials were moved to secrets", which comes from a plural form
   (`MigrationController.cs:118`). Confirm the rendered text and that it is still visible after the redirect.
9. **The total after the migration.** `stores-1` says the total shows the two active secrets of `Database`. This holds only
   if no other secret exists in the site. The Blog recipe (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.recipe.json`)
   enables no Secrets feature and creates no secret, so the Database store starts empty; confirm that the Secrets list is
   empty before the seed. The total counts every secret of the store, expired ones included
   (`StoreController.cs:115`), which makes no difference here because no secret expires.
10. **What a move does to the secret of a feature that is running.** The narration says the features that use a secret read it
    by name wherever it is kept (`SecretValueExtensions.GetSecretValueAsync` calls `GetSecretAsync<TextSecret>(name)`, which
    returns the secret from the first store that has it, `SecretManager.cs:27-39`). No email is sent in the recording (outbound
    calls are blocked), so this is not shown, and the video does not say that mail is delivered.
11. **The page right after Move.** `moved-1`, `result-1` and `one-4` assume that the response of the POST lists each secret
    with `Moved`, and that the total of the source store is read again in the same response and is `0`
    (`StoreController.cs:97-98` and `:115`). The database store removes a secret in a child scope and the move then lists
    the source again in the parent scope (`DatabaseSecretStore.cs:112-118`, `SecretStoreOperations.cs:118-123`). The
    document cache is invalidated when the child scope commits (`DocumentManager.cs`, `UpdateAsync`), so the read is
    expected to be fresh; if the page shows `Failed` badges or a stale total after moving from `Database`, do not
    record the clip and report it as a bug.

## Pronunciation

12. `storyboard.json` has `"spoken": {"AzureKeyVault": "Azure Key Vault"}`. Check it, "Secrets - Azure Key Vault" (the hyphen
    between the words), "Nothing to migrate!" and "SMTP" with `scripts/pronounce.py` before narrating.

## Recorder selectors

13. The admin menu links, the settings tabs (`SMTP`) and the breadcrumb link `Secrets` come from the admin theme's markup,
    which was not read. The plan gives the visible text; probe the pages and pick stable selectors.
