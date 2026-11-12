# Open questions: email-smtp

Everything in the narration about Orchard Core was read from the source (commit `92db4a83`) and the documentation. The
points below could not be settled without a running site, or do not come from the Orchard Core source. A person confirms
each one on the recording site and corrects `storyboard.json` or `recording-plan.md` if the screen differs.

## 1. The mail server (smtp4dev) is not part of the Orchard Core source

**Checked.** The image name (`rnwood/smtp4dev`) and the container ports (SMTP 25, web page 80, as in `-p 2525:25
-p 5000:80`) are in smtp4dev's own installation page. Its documentation does not state, in the pages read, the default
of the authentication and TLS settings, whether it relays, or the columns of the message list; the behaviors below are
from memory of smtp4dev, not from a file in the Orchard Core source.

**To confirm on a running container.**
- That `docker run -d --name smtp4dev-video -p 3000:80 -p 2525:25 rnwood/smtp4dev` starts it, the web page opens on
  `http://localhost:3000`, and the inbox is empty at the start.
- That it accepts a plain connection on port 25 without a sign-in: no TLS and no authentication required. The narration
  of `config-6` ("accepts plain connections") and `config-7` ("does not" ask for a sign-in) depends on it. If the
  default requires either, change the server's settings (not the narration).
- That it does not relay messages to a real server (the narration of `plan-1` says nothing reaches a real inbox).
- What its message list and message view look like, and a selector for the row: the plan uses the text of the subject,
  `Test message`. The narration of `inbox-1` and `inbox-2` names only the sender, the recipient, the subject and the
  text, and says the sender is the address of the SMTP settings (true when the Sender of the test page is empty:
  `SmtpEmailProviderBase.cs:54-56`).
- That the message view opens on the text of the message without opening headers or the raw source.

## 2. Defaults of the SMTP tab on a fresh site

**Checked.** `SmtpSettings` has `Port = 25`, `AutoSelectEncryption = false`, `EncryptionMethod = None`,
`DeliveryMethod = Network` (`src/OrchardCore/OrchardCore.Email.Abstractions/SmtpSettings.cs`, `SmtpDeliveryMethod.cs`).
`SmtpSettingsViewModel` defaults `AutoSelectEncryption` to `true`, but the display driver overwrites it with the stored
value (`SmtpSettingsDisplayDriver.cs:82`), so the check box should be unchecked and the select should show None. The
narration does not state these defaults: `config-6` says "we choose None", and the plan sets None explicitly.

**To confirm on screen.** That Auto Select Encryption method is unchecked, Encryption method to use is enabled and shows
None, Port number shows 25, and Delivery method shows Network when the tab is first opened and Enable SMTP Provider is
checked.

## 3. The Providers tab warning, and the tab shown after Save

**Checked.** The Providers tab shows the warning when no provider is enabled (`EmailSettings.Edit.cshtml:7-15`); the
SMTP provider is enabled only when its setting `IsEnabled` is true (`SmtpProviderOptionsConfigurations.cs`), so after the
feature is enabled and before the settings are saved the list is empty. After Save the page is loaded again and the first
tab is active: no script keeps the active tab (a search for tab persistence in the source found none; tabs are built in
`GroupShapes.cs`).

**To confirm on screen.** That the warning is shown at the start of `config-1`, that the page shows the Providers tab
after Save (and not the SMTP tab), and that Default provider already holds "Simple Mail Transfer Protocol (SMTP)" on that
page. The SMTP driver sets SMTP as the default when the provider is enabled and the site has no default provider
(`SmtpSettingsDisplayDriver.cs:193-200`); `config-9` says "because no default provider was set". The option list is built
from options refreshed in the same request, so check that the list is not empty on the first load after Save.

One more point to watch on that first Save. The Providers tab shows only the warning, so the form posts no Default
provider. On a site without a stored default, the Email driver compares null with null and changes nothing
(`EmailSettingsDisplayDriver.cs:74-79`). The drivers run in the order of the modules, and startups are sorted by module
dependency with a stable sort (`ShellContainerFactory.cs:120-122`), so the Email driver runs before the SMTP driver and
the SMTP driver's choice of SMTP as the default is kept. If Default provider is empty after Save, the order of the two
drivers is the cause: stop, and report it instead of recording around it. Even then the page would show SMTP, because it
falls back to the only enabled provider (`EmailOptionsConfiguration.cs:33-40`).

## 4. The Features page: layout of the rows and of the actions

**Checked.** The rows are drawn by the admin list in the List layout by default, with the actions as buttons by default
(`AdminListOptions.cs`), and the dependencies as badges next to the name (`FeatureEntry.DefaultTags.SummaryAdmin.cshtml`).
The Enable link posts through a page script (`data-url-af="UnsafeUrl"`) and returns to `/Admin/Features`.

**To confirm on screen.** The selectors of the row (`:is(li, tr):has(span.fw-semibold:text-is("SMTP Email Provider"))`),
of the badge Email, and of the Enable link (`a[href*="OrchardCore.Email.Smtp/Enable"]`), in the layout the site shows;
that the search for `smtp` leaves one row; that the toast "The feature SMTP Email Provider was enabled." is the only
message (it names the requested feature, not the Email feature that is enabled with it:
`Features/Controllers/AdminController.cs:247,317`).

## 5. Toasts hide after five seconds

**Checked.** Success messages hide 5 seconds after the page loads (`NotifierExtensions.cs:71`, `Message.cshtml`). The
narration of `enable-3`, `config-8` and `test-4` mentions the message in the second sentence.

**To confirm on screen.** On the contact sheets of those three steps, that the toast is on a frame while the sentence is
spoken. If it is not, shorten the actions of the step (the click is the first action), not the narration.

## 6. Pronunciation

`storyboard.json` has six `spoken` entries, none of them tried with `scripts/pronounce.py`. A person confirms each one
with a sample and the gaps around it:

- `SSL/TLS` is spoken "S.S.L. slash T.L.S.".
- `STARTTLS` is spoken "Start T.L.S.".
- `2525` is spoken "twenty five twenty five".
- `noreply@example.com` is spoken "no reply at example dot com", and `jamie.chen@example.com` is spoken "jamie dot chen
at example dot com".
- `Cc, Bcc, and Reply To` is spoken "C.C., B.C.C., and Reply To" (the whole phrase is the key so that no other word is
  changed).

Also listen to how the voice reads, without a `spoken` entry: "localhost", "SMTP", "Simple Mail Transfer Protocol (SMTP)" and "Email Task".
The plan types `smtp` in lower case in the search box, but the narration says "SMTP". The slides and the tags hold the
name smtp4dev, which the narration never says.

## 7. What the narration does not say, on purpose

- What Use Windows authentication does. The hint on the page says the identity of the application pool or the host process
  is used, while the code sends empty credentials (`SmtpEmailProviderBase.cs:170-174`). The narration names the option
  only (see the doc issues).
- Anything about the proxy fields (Proxy host name, Proxy port number). The code builds a SOCKS5 client after the connection
  is made (`SmtpEmailProviderBase.cs:166,181-184`), which may not route the connection; the narration does not mention
  them.
- The Default SMTP provider, which exists only when the configuration holds SMTP settings (`Startup.cs`,
  `SmtpProviderOptionsConfigurations.cs`). It is a configuration topic, outside an admin video.
- The Specified pickup directory options: the narration names the choice and says it saves each message as a file
  (`SmtpEmailProviderBase.cs:193-200`, one file with the extension .eml per message); the plan never selects it, because
  its hint shows template tokens.

## 8. Roles

`parts-2` says the Administrator role has the Manage Email Settings permission and "the admin user can open both"
pages. The permission is granted to the Administrator stereotype (`OrchardCore.Email.Core/Permissions.cs`); confirm that
the user created by Auto Setup is in the Administrator role (it is by default; the pages opened in `config-1` and
`test-1` show it).

## 9. What the Email Test page lists when a test fails

**Checked.** `fail-1` says the page lists the error. A failed send returns its errors, and the page adds each one to the
validation summary (`Controllers/AdminController.cs:87-90`). The SMTP provider reports a failure as "An error occurred
while sending an email:" followed by the message of the exception (`SmtpEmailProviderBase.cs:98-101`); an invalid address
is reported by the message validator (`EmailMessageValidator.cs:18-55`).

**To confirm on screen.** Nothing is recorded for `fail-1` (it is a slide). A person can stop the mail server once and
send a test to see the text, but the narration does not quote it.
