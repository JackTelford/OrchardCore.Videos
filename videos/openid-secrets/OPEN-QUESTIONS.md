# Open questions: openid-secrets

What could not be verified from the source or the documentation, and what a person must confirm on a running site. The
narration of the video states only what the screens and the source show; nothing below is claimed as a fact in it.

Source references are lines of the Orchard Core source at commit `92db4a83`.

## 1. BLOCKING: does an RSA Key secret created by Add Secret work as the signing key?

Answer this before any clip is recorded. If a check fails, stop: the video cannot be recorded as written.

**Proven from the source** (commit `92db4a83`, checked twice, independently)

- The page that creates an RSA Key secret generates the key pair and fills `PublicKey` and `PrivateKey`, but never sets
  `IncludesPrivateKey` (`src/OrchardCore.Modules/OrchardCore.Secrets/Controllers/AdminController.cs:556-571`). The property is
  `false` unless set (`src/OrchardCore/OrchardCore.Secrets.Abstractions/RsaKeySecret.cs:22`). The editor's model has no field for it
  (`OrchardCore.Secrets/ViewModels/SecretEditViewModel.cs`). The display driver and the views that do have a "Key Type" list
  (`Drivers/RsaSecretDisplayDriver.cs:67-98`, `Views/Items/RsaSecret_Fields_Edit.cshtml`) are not reachable from the editor: no code
  asks a display manager for an `RsaKeySecret` (`Views/Admin/Edit.cshtml` draws the RSA Key fields itself). No type provider or
  converter sets the flag (`OrchardCore.Secrets.Abstractions/SecretTypeProvider.cs:21-32`).
- The OpenID Connect Secrets feature imports the private key only when the flag is `true`
  (`src/OrchardCore.Modules/OrchardCore.OpenId/Secrets/Services/OpenIdSecretsOptionsConfiguration.cs:113`). With the flag `false`, the
  key it inserts first in the signing credentials (`:42`) and in the encryption credentials (`:57`) holds the public key only.
  So the two secrets of `03-create`, selected in `04-select`, give the server two keys without a private key.
- The Secrets documentation says the opposite of what the code does: "Or use the Admin UI to create an RSA secret with auto-generated
  keys" (`src/docs/reference/modules/Secrets/README.md:1576`), "Key generation: Generate in Admin UI" (`:720`), and the requirement
  "For `RsaKeySecret`: `IncludesPrivateKey` must be `true`" (`:680`). See the documentation issues of the hand-off.

**Not known: what the server does with such a key.** That is decided inside OpenIddict, which is not in the Orchard Core tree. Its
package holds the error texts "The asymmetric signing key doesn't contain the required private key." and "The asymmetric encryption key
doesn't contain the required private key." (in the resource strings of every OpenIddict 7.x release checked, 7.2.0 to 7.7.2), but the
tree cannot say where they are raised: when OpenIddict builds its options, which would break requests of the whole tenant (the server
scheme is registered for every request: `OpenId/Configuration/OpenIdServerConfiguration.cs:44-45`), or only when a token is signed or
decrypted. The order in which OpenIddict picks among the credentials is also its own: the feature only inserts the key first.

**What to confirm on a running site** (a throwaway site, fake data)

1. Follow `recording-plan.md` as far as the end of `04-select`, with the Debug log level set for
   `OrchardCore.OpenId.Secrets.Services.OpenIdSecretsOptionsConfiguration`.
2. Right after the save of `select-4`, the Authorization Server page must reload without an error, and the dashboard (`/Admin`) and
   the discovery document (`/.well-known/openid-configuration`) must still answer. A failure here would show on camera in
   `select-4`.
3. After the save and the reload of the tenant, the log must show "OpenID signing key loaded from secret 'ServerSigningKey'." and
   "OpenID encryption key loaded from secret 'ServerEncryptionKey'." (`OpenIdSecretsOptionsConfiguration.cs:43-46,59-62`), and no warning or
   error from that class or from OpenIddict.
4. Request the key set of the server (OpenIddict's default cryptography endpoint, `/.well-known/jwks`; confirm that it answers) before
   and after the save: a key must be added.
5. Create a test application with the Client Credentials flow (a planned video of the series covers that setup). The flow is off by
   default (`OpenId/Services/OpenIdServerService.cs:72-83` enables Authorization Code and Refresh Token only): on the Authorization Server
   page, check "Allow Client Credentials Flow" under Flows, set "Token Format" to "JSON Web Token (JWT)" and save. Request a token from
   `/connect/token`. A token must be returned, and the `kid` in its header must be the key added in step 4, not the key of a managed
   certificate. That also settles whether the key inserted first is the one OpenIddict signs with, which the card claims ("these take
   precedence"). A signing key without its private key is expected to fail here (not tested).
6. With the encryption key selected and "Disable Access Token Encryption (only for JWT tokens)" left unchecked, encrypting the access
   token needs only the public key, so a returned token proves nothing about the private key. Post that token to `/connect/introspect`,
   as the same confidential client (confirm that the application may call the introspection endpoint): the answer must say
   `"active": true`. That forces the server to decrypt its own token. The same missing flag also breaks decryption with an RSA Key made
   in the admin for the encrypted deployment of secrets (`src/OrchardCore/OrchardCore.Secrets.Core/Services/SecretEncryptionService.cs:145`).

**If it fails**: the defect is in Orchard Core (the editor should save the key with its private key flag), not in this video. Report it, and
do not work around it by seeding the keys through code or a recipe: the video shows the admin screens. Do not switch the demo to an
X.509 Certificate secret either: the PEM form puts key material on screen and the store form shows the certificates of the machine.
The tracker entry then moves to `blocked` until Orchard Core is fixed.

**What the narration says meanwhile**: `select-1` reads the text of the card, `select-2` and `select-3` say what the two hints ask
for, and the other steps describe the screens. None says that a token was signed with the key, or that every server uses it. The
description typed in `create-3` names the role of the key, not what it does.

## 2. Texts the narration repeats, checked in the source only

The hints, the card text and the toasts are read from the views and controllers (references in `recording-plan.md`). Confirm on the
site, while recording, that the toasts read "The feature OpenID Connect Secrets was enabled." (singular: only one feature is enabled
by the click, because the preparation enables the two it depends on), "Secret created successfully." and "OpenID server configuration
successfully updated.", and that the success toast of `enable-3`, `create-3`, `create-4` and `select-4` is on screen when the
narration of the step ends (it hides after 5 seconds).

## 3. The Features row

`enable-2` says the badges beside the feature show the features it depends on: "OpenID Connect Authorization Server" and "Secrets". They are
the names of the direct dependencies in the manifest. Confirm they appear in the layout the site uses (List or Grid) and that
the identifier shown beside the name is not read aloud or highlighted. The Actions column of the row can show a menu instead of
buttons (the admin list options: `OrchardCore.Modules/OrchardCore.Admin/AdminListShapeTableProvider.cs:116-118`; the default is
buttons): confirm that the Enable button is on the row without opening a menu.

## 4. Selectors

The admin menu markup, the Enable button id (`btn-enable-OrchardCore_OpenId_Secrets`, built by `Html.GenerateIdFromName`), the
card's Create link and the select ids (`#SigningKeySecretName`, `#EncryptionKeySecretName`, `#Name`, `#RsaKeySize`,
`#Description`, `#ExpiresUtc`) are read from the views, not run. Confirm them with `Recorder probe`.

## 5. Existing certificates and fallbacks (`carry-1`, `carry-2`)

Both slides come from the code and are not shown on a screen:

- `carry-1`: `OpenId/Secrets/Migrations.cs:34-87` saves an X.509 Certificate secret named `OpenId.SigningCertificate` (and
  `OpenId.EncryptionCertificate`) when the signing (or encryption) certificate store location, store name and thumbprint are set and no
  secret is selected yet, and selects it. To confirm: on a site with a fake self-signed certificate (subject `connect.example.com`) in a
  certificate store, select it under "Advanced Options" of the Authorization Server page, save, then enable "OpenID Connect Secrets". The
  Secrets list must show the secret with the description "Migrated from OpenID Server signing certificate settings", and the card must
  have it selected.
- `carry-2`: with no secret selected, or with a secret that is missing or cannot be loaded, nothing is inserted in the credentials
  (`OpenIdSecretsOptionsConfiguration.cs:36-48,71-97`), so the certificates stay. To confirm: delete a selected secret, restart the
  tenant, and request a token: it must still be issued, and the log must show a warning naming the secret.

## 6. Several servers

`why-2` and `why-3` rest on the code (the certificates are written as files in the tenant's folder of the data folder:
`OpenId/Services/OpenIdServerService.cs:356-411,484-487,650-656`) and on the documentation (`Secrets/README.md:23,728-730,1347-1360,1406-1410`:
file-based certificates are not available on every server, and all servers must share the Data Protection keys that encrypt the
secrets). Neither was tested with two servers.

## 7. Pronunciation

`storyboard.json` has three `spoken` entries, not yet checked by ear: "ServerSigningKey" read as "Server Signing Key",
"ServerEncryptionKey" as "Server Encryption Key", and "X.509" as "X five oh nine". Before narrating, check them, and how the voice
reads "OpenID" and "JSON Web Tokens", with `scripts/pronounce.py`. A person confirms every spelling.

## 8. For information

The Key size list offers 2048, 3072 and 4096 bits and the generated key has the size chosen, but the `KeySize` property of the saved
secret keeps its default of 2048 (`AdminController.cs:565-570`). The narration says only that the size is that of the generated key
pair, and the recording leaves the default.
