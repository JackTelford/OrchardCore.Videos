# Open questions: signalr

What the author could not verify from the source or the documentation, and what a person must confirm on a running
site or with the voice before the video is recorded. The narration keeps to what the source shows; none of the points
below is stated as a fact in it.

## 1. Pronunciations

The storyboard's `spoken` map adds these spellings. Check each with `scripts/pronounce.py` before narrating:

| Written | Spoken as | Where |
|---|---|---|
| `SignalR` | `Signal R` | nearly every step |
| `signalr` | `signal R` | `why-2`, `client-1` |
| `MediaChanged` | `Media Changed` | `send-2` |
| `SubscribePath` | `Subscribe Path` | `send-1`, `client-2` |
| `HubOptions` | `Hub Options` | `host-3` |
| `RequireFeatures` | `Require Features` | `host-2` |

Also listen to "Redis", "backplane", "Azure", "keep-alive" and "camel-cased" (the last one is on a slide only).

## 2. Which backplane wins when both are enabled

The documentation says that when the Azure and Redis backplanes are both enabled and configured, the Azure backplane is
used and an error is logged for Redis. The code only shows that whichever feature registers first keeps the backplane
and the other logs an error and stays inactive (`OrchardCore.SignalR.Azure/Startup.cs:46-58`,
`OrchardCore.SignalR.Redis/Startup.cs:33-46`, both through the keyed service `OrchardCore.SignalR.Backplane`). Which
one registers first depends on the order in which the startup classes are run, which the author could not read from the
source. The narration (`scale-4`) says only that one of them logs an error and stays inactive.

To confirm: on a throwaway site, configure both (`OrchardCore__Redis__Configuration` and
`OrchardCore__SignalR__Azure__ConnectionString` with a made-up value), enable both features, and read the log lines
that start with `The 'OrchardCore.SignalR.` .

## 3. The rules for the Azure application name

The documentation says `ApplicationName` must start with a letter and contain only letters, numbers and underscores.
`AzureSignalROptionsExtension.cs` validates only the connection string; the application name is used as given, or
`OrchardCore` when empty, followed by `_` and the SHA-256 hash of the tenant name in hexadecimal
(`OrchardCore.SignalR.Azure/Startup.cs:72-81`). Any rule on the name may come from the Azure SDK. The narration does not
state the rule.

## 4. Is the confirmation message still on screen at the end of `features-3`

After **Enable**, the page redirects to the Features page, which shows "The feature SignalR Redis Backplane was
enabled." (`OrchardCore.Features/Controllers/AdminController.cs:317`). The author did not check how long a
notification stays on the admin pages. If it fades before the narration ends (about 9 seconds after the click), move
the `TypeAsync(#search-box, "redis")` and the highlight earlier, or shorten the hold of `features-2`.

## 5. Does the Redis backplane become active when the feature is enabled on a running site

`SignalR Redis Backplane` registers only if `IsRedisConfigured()` is true while the tenant's services are built
(`OrchardCore.SignalR.Redis/Startup.cs:31`), and the Redis feature (a dependency) registers the real service only when
`OrchardCore:Redis:Configuration` is set (`OrchardCore.Redis/Startup.cs:34-58`). Enabling a feature restarts the
tenant's shell, so it should work with the variable set before the site starts, but this was not run. To confirm after
`02-features`: the site log has no line starting with `'Redis' features are not active`, and after a hub connection
is opened (a Media Library tab) this prints a channel:

```bash
docker exec ocvideo-redis redis-cli PUBSUB CHANNELS "*SignalR*"
```

## 6. Delivery between two instances is not shown

`03-live` runs on one instance, with the Redis backplane active. The narration says that the Media module sends a
message to the hub and that the library asks for the files again; it does not say that the message went through Redis,
or that it reached another instance. The claim that a backplane relays messages between instances rests on the
documentation and on the manifests' descriptions. A person with two instances of the same site (the same `App_Data`
and database, two ports, the same Redis) can confirm it by renaming a file on one and watching the other; this is not
recorded.

## 7. The gallery's connection message and the hub in a background tab

`03-live` waits for the console message `SignalR connected to MediaHub`. The line is in the gallery
(`.../Assets/media-gallery/src/services/SignalR.ts:112`), but it is printed from the callback of `SignalRApp.onConnect`,
which is in the Bloom scripts (`@bloom/services/signalr/signalr-app`). That folder (`.scripts/bloom` of the repository)
is not part of the source the author read, so when the callback runs, and what else Bloom does (bearer-token
negotiation, transport, reconnection, as the Media gallery documentation describes), is unverified. If the message never
appears, wait 3 seconds after both lists are shown instead.

Also check that the first tab, which is in the background while the second tab renames the file, has received the
update when `live-3` brings it to the front. If the list only changes once the tab is shown, wait for it as the plan
says (up to 10 seconds); the frame at the end of the step must show `post-header.jpg`.

## 8. Selectors of the Media Library

The row (`.file-item`), the menu button (`a.action-button`), the menu (`#overlay_file_menu`, drawn by PrimeVue), the
field (`input[name=rename]`) and the button (`#btn-submit`) were read in the Vue sources, not on a running page. Check
each with `Recorder probe /Admin/Media "<script>"` before writing `Scenes.cs`, and check that the menu item **Rename**
is found by its text inside `#overlay_file_menu`.

## 9. The Features page after the search is typed

The plan expects the buttons `#btn-disable-OrchardCore_SignalR`, `#btn-disable-OrchardCore_Media_SignalR` and
`#btn-disable-OrchardCore_Redis` on enabled rows (a feature that is enabled and may be disabled), and the
`.direct-dependencies` element in the dependencies column of the list layout. The list layout is assumed to be the
default; if the page opens in the grid layout, switch it to the list layout outside the capture and keep that setting
in the snapshot.

## 10. The bundled-app statement

`client-2` says that a bundled app, such as the Media gallery, includes the client in its own bundle and does not load
the `signalr` resource. The source shows that the gallery's page (`OrchardCore.Media/Views/Admin/Index.cshtml`) loads
only its own `media` script and that `SignalR.ts` imports `@microsoft/signalr`. The documentation's list of what the
Bloom layer provides (`signalr-app`, `eventbus`, `useSignalRService`) could not be checked, and is not narrated.

## 11. Waiting for the network to be idle on a page with a live connection

`Scene.OpenAsync` and `Scene.SwitchToAsync` wait for the network to be idle. The Media Library holds a SignalR
connection open. A WebSocket does not count as network activity, so the wait should end; if the connection falls back
to long polling it would not. To confirm: open `/Admin/Media` with `Recorder probe` and check that the page reaches
the idle state within 20 seconds; if it does not, replace the wait by `WaitUntilState.Load` plus the `.file-item`
wait the plan already has.
