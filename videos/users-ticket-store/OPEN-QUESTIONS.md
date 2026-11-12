# Open questions: users-ticket-store

What the author checked in the Orchard Core source (commit 92db4a83), and what a person must confirm on a running site
before the video is recorded or published. The narration states only what the source and the documentation show.

## 1. What happens to a signed-in session when the feature is enabled

- **Checked:** `Enable` (`src/OrchardCore.Modules/OrchardCore.Features/Controllers/AdminController.cs:227`) enables the
  feature, notifies "The feature Users Authentication Ticket Store was enabled." (`AdminController.cs:317`) and redirects to
  the Features page (`AdminController.cs:255`, `GetNextUrl` at `:287`). `CookieAuthenticationOptionsConfigure.Configure`
  (`src/OrchardCore.Modules/OrchardCore.Users/CookieAuthenticationOptionsConfigure.cs:26`) sets `SessionStore` only for
  `IdentityConstants.ApplicationScheme`, and only while the feature is enabled.
- **Not verified:** how the framework's cookie handler treats an application cookie issued before the store existed
  (a ticket held in the cookie, with no session identifier). The page that loads after the click could be the Features
  page with the notification, or the sign-in page.
- **To confirm:** sign in as `admin`, enable the feature on the Features page, and note which page loads and whether the
  session survives. Until then `02-enable` ends on the Enable button without clicking it, and the narration says nothing
  about the result of the click.

## 2. Tickets after a restart or a tenant reload

- **Checked:** the default `IDistributedCache` is a `MemoryDistributedCache` registered in the services of the tenant
  (`src/OrchardCore/OrchardCore.Infrastructure/Cache/OrchardCoreBuilderExtensions.cs:49` and `:50`), and
  `CacheTicketStore.RetrieveAsync` returns no ticket when the entry is missing
  (`src/OrchardCore/OrchardCore.Users.Core/Authentication/CacheTicketStore.cs:58` to `:66`). The narration (`cache-1`) says
  that a restart or a reload of the tenant clears the tickets.
- **Checked:** enabling or disabling a feature updates the shell descriptor
  (`src/OrchardCore/OrchardCore/Shell/ShellDescriptorFeaturesManager.cs:112`), whose manager raises `ChangedAsync`
  (`src/OrchardCore/OrchardCore.Infrastructure/Shell/ShellDescriptorManager.cs:133` to `:136`); `ShellHost.ChangedAsync`
  releases the tenant so that a new shell is built (`src/OrchardCore/OrchardCore/Shell/ShellHost.cs:172` to `:175`). The
  new container registers a new `MemoryDistributedCache` singleton, so `cache-1` says that a reload of the tenant clears
  the tickets, as a restart does.
- **Not stated:** what the user sees when no ticket is found.
- **To confirm:** sign in, change a feature, request a page, and note whether the session survives.

## 3. What the cookie holds

- **Checked:** `StoreAsync` returns the unique key it generated (`CacheTicketStore.cs:83` and `:92`). The framework's documentation of
  `CookieAuthenticationOptions.SessionStore` says that, when it is used, only a session identifier is sent to the
  client, and the documentation of `ITicketStore` says that it keeps the identity on the server while only a simple
  identifier key is sent to the client. The narration (`why-3`) says the cookie holds only a session identifier.
- **To confirm:** in the browser's developer tools, compare the cookie `orchauth_Default` (name from
  `src/OrchardCore.Modules/OrchardCore.Users/Startup.cs:123`) for a user with several roles, with the feature disabled and
  enabled. No size in bytes is stated in the video, because none is in the source.

## 4. The header limit

- **Checked:** `src/docs/reference/modules/Users/TicketStore.md` says that a large cookie could make a request fail by
  exceeding the header limit. The narration (`why-2`) attributes this to the documentation and gives no number. The
  claims of a ticket (a claim for each role, and a claim for each permission of the roles unless the user is an
  administrator) are read from `src/OrchardCore.Modules/OrchardCore.Roles/Services/RoleClaimsProvider.cs:36` to `:66`.
- **To confirm:** whether a site with many roles and permissions reaches the limit of its server (it depends on the server
  and its settings; nothing in the video depends on it).

## 5. Instances that share a cache but not their Data Protection keys

- **Checked:** `RetrieveAsync` catches a failure to unprotect, logs it and returns no ticket (`CacheTicketStore.cs:68` to
  `:79`). The key ring of a tenant is isolated per tenant and stored in the tenant folder of `App_Data` unless another
  feature stores it (`src/OrchardCore/OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:555` to `:585`, and
  `src/docs/guides/data-protection/README.md`). The narration (`cache-3`) says that instances sharing a cache must share
  their keys too.
- **To confirm:** with two instances that share Redis but keep local key rings, a ticket written by one is not accepted by
  the other, and the log shows a `RetrieveAsync failed` entry.

## 6. The key of a ticket in Redis

- **Checked:** `RedisCacheOptionsSetup.Configure` sets `InstanceName` to the instance prefix followed by the tenant name
  (`src/OrchardCore.Modules/OrchardCore.Redis/Options/RedisCacheOptionsSetup.cs:31`), and the ticket key is
  `ocauth-ticket-` followed by the unique key (`CacheTicketStore.cs:14`). `RedisCacheStartup` registers the cache only
  when Redis is configured (`src/OrchardCore.Modules/OrchardCore.Redis/Startup.cs:62` to `:77`). The narration (`cache-2`)
  says the tickets go to Redis under the instance prefix and the tenant name.
- **To confirm:** list the keys in a Redis server after a sign-in, and check that they start with the instance prefix and
  the tenant name.

## 7. Pronunciation

The `spoken` map of the storyboard holds spellings that no video has used: "I Ticket Store", "I Distributed Cache", "Cache
Ticket Store", "Session Store", "Store Async", "Retrieve Async", "Remove Async" and "Renew Async". Check them, and the word
"Redis", with `scripts/pronounce.py` before narrating.

## 8. The Features page as recorded

- **Checked:** the labels, the markup and the default layout of the page are read from the views of
  `src/OrchardCore.Modules/OrchardCore.Features` (the file and line of each are in `recording-plan.md`).
- **To confirm:** on the first take, that the admin menu shows Tools open with Features as the current entry when the page is
  opened directly, that the search leaves one row, and that the row shows the Users badge, the status Disabled and an Enable
  button.
