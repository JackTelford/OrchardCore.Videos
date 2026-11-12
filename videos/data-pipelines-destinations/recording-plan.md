# Recording plan: Data Pipelines: Deliver Files by Link, Email, FTP and SFTP

For the person who records the video. Everything to decide is decided here. Locators are Playwright locators of
`scene.Page`: `Locator("css")`, `GetByLabel("text")`. The `data-cy` attributes, the class names and the labels come from
the Orchard Core source at the commit named in `README.md`; each one says where it is read. Fake data only.

The video has seven recorded clips (`02-features` to `08-links`), two slide steps and two cards. It needs no mask
script: the site is made from scratch, and nothing real is on it.

## The site it needs

**Orchard Core.** `src/OrchardCore.Cms.Web` of an `OrchardCMS/OrchardCore` checkout, on `http://localhost:5010`, with
its own `ORCHARD_APP_DATA` folder outside both repositories, provisioned by Auto Setup (see `references/recording.md`
and the `orchardcore-tester` skill of that repository):

| Setting | Value |
|---|---|
| Recipe | Blog |
| Site name | Orchard Core |
| Time zone | `America/Los_Angeles` |
| User name | `admin` |
| Email | `admin@example.com` |
| Password | `<password>` (chosen outside git, passed in `VIDEO_PASSWORD`) |
| Outbound web calls | blocked: `HTTP_PROXY` and `HTTPS_PROXY` set to a dead address for the site process. The mail, FTP and SFTP connections are plain TCP to `localhost`, and are not affected |

This video is not about the Setup screen: record it signed in, with `Recorder setup` and a `state.json`.

**Features to enable before recording** (outside the capture), from Tools, Features, with Bulk Actions, Enable:

- Data Pipelines (`OrchardCore.DataPipelines`). It enables Data Pipelines Core and Users by dependency. The Blog recipe
  already enables Contents and Media.
- Email (`OrchardCore.Email`).
- SMTP Email Provider (`OrchardCore.Email.Smtp`).

Do **not** enable Data Pipelines - FTP (`OrchardCore.DataPipelines.Ftp`) or Data Pipelines - SFTP
(`OrchardCore.DataPipelines.Sftp`): clip `02-features` enables them on camera. Do not enable Workflows.

**External services and their local stand-ins** (Docker; remove the containers when the recording is done). The
passwords are chosen outside git and passed to the recorder as `VIDEO_FTP_PASSWORD` and `VIDEO_SFTP_PASSWORD`; they
are written here as `<ftp-password>` and `<sftp-password>`:

| Service | Stand-in | Used as |
|---|---|---|
| SMTP catcher | `docker run -d --name ocv-mail -p 1025:1025 -p 8025:8025 axllent/mailpit` | The site sends to `localhost` port 1025. The mailbox is read at `http://localhost:8025`, only to check a take |
| FTP server | `docker run -d --name ocv-ftp -p 21:21 -p 30000-30009:30000-30009 -e PUBLICHOST=localhost -e FTP_USER_NAME=demo -e FTP_USER_PASS=<ftp-password> -e FTP_USER_HOME=/home/demo stilliard/pure-ftpd` | Host `localhost`, port 21, user `demo`, plain FTP (no certificate) |
| SFTP server | `docker run -d --name ocv-sftp -p 2222:22 -v <keydir>/ssh_host_ed25519_key:/etc/ssh/ssh_host_ed25519_key atmoz/sftp demo:<sftp-password>:::upload` | Host `localhost`, port 2222, user `demo`, folder `upload` |

`<keydir>` is a folder outside both repositories that holds a host key made once with
`ssh-keygen -t ed25519 -N "" -f <keydir>/ssh_host_ed25519_key`. Mounting it keeps the server's host key, and so its
fingerprint, the same every time the container is created again. None of the three images was run while this plan was
written: check each one as `OPEN-QUESTIONS.md` says before the first take.

**The fingerprint of the SFTP server** (`VIDEO_SFTP_FINGERPRINT`). Read it from the product, so it is the one the step
sees. Once, on a scratch copy of the site that is thrown away after (or on the site, then restore snapshot `S0`):
publish a pipeline whose only destination is **Upload to an SFTP server** with the **Host key fingerprint** empty,
run it, and read the warning in the run's Log: `The host key of the SFTP server 'localhost' isn't verified. To verify
it, set its fingerprint: SHA256:...`. Keep the text from `SHA256:` on in `VIDEO_SFTP_FINGERPRINT`.

**Fake data to create outside the capture.**

1. **A recipient.** Access Control, Users, **Add User** (`UsersAdminListCreate.cshtml:7`): **User name** `jamie.chen`,
   **Email** `jamie.chen@example.com`, **Password** and **Password confirmation** a password chosen outside git,
   **Save**. The **Share a download link** step fails the run when a recipient is not a user of the site
   (`SharedLinkManager.cs:286`), and emails only the users who have an email address.
2. **An email provider.** Settings, Communication, Email (`OrchardCore.Email/AdminMenu.cs`). Check **Enable SMTP
   Provider**; **Sender email address** `pipelines@example.com`; **Delivery method** Network; **Host name** `localhost`;
   **Port number** `1025`; leave **Auto Select Encryption method** and **Require credentials** unchecked; **Encryption
   method to use** None; **Save**. Then **Default provider** `Simple Mail Transfer Protocol (SMTP)` (it is listed only
   once the provider is enabled and saved), **Save** again. Check it: Tools, Testing, **Email Test**, **Provider** SMTP,
   **To** `reports@example.com`, any **Subject** and **Body**, **Send**; the message must appear in the mailbox at
   `http://localhost:8025`. Empty the mailbox afterwards.
3. **The pipeline.** Tools, Deployments, **JSON Import** (a CodeMirror editor: set its content with the editor's
   `setValue`, or paste), then **Import**. It creates the pipeline as a draft (`DataPipelinesRecipeStep.cs`), with the
   step ids the clips rely on:

   ```json
   {
     "steps": [
       {
         "name": "DataPipelines",
         "Pipelines": [
           {
             "PipelineId": "blogpostsexport",
             "Name": "Blog posts export",
             "Description": "Exports the blog posts to a CSV file.",
             "IsEnabled": true,
             "Definition": {
               "Steps": [
                 {
                   "StepId": "source",
                   "Type": "DataSource",
                   "X": 120,
                   "Y": 160,
                   "Properties": { "DataSourceStepSettings": { "Source": "Contents", "DataSet": "BlogPost" } }
                 },
                 {
                   "StepId": "file",
                   "Type": "CreateFile",
                   "X": 440,
                   "Y": 160,
                   "Properties": { "CreateFileStepSettings": { "Format": "csv", "FileName": "blog-posts" } }
                 }
               ],
               "Connections": [
                 { "SourceStepId": "source", "SourcePort": "Output", "TargetStepId": "file", "TargetPort": "Input" }
               ]
             }
           }
         ]
       }
     ]
   }
   ```

   Check: Tools, Data Pipelines, Pipelines lists **Blog posts export** with the **Draft** badge, and its **Edit**
   button opens `/Admin/DataPipelines/blogpostsexport/Edit` with two cards, **Read a data source** and **Create a
   file**. The blog posts are the ones the Blog recipe creates; add none.

**Check before every take.** The three containers run; the mailbox is empty; the Email Test message arrives; a
client of your choice logs in to the FTP and SFTP servers with the demo credentials. The Tools group of the admin
menu is collapsed (click it if it is open), since clips open it on camera.

## Snapshots, and the order of the clips

Stop the site and copy its `App_Data` folder and `state.json` aside after each step below. To record a clip again,
restore the copy made before it.

| Copy | Taken | Holds |
|---|---|---|
| `S0` | After the seed above and `Recorder setup`, before `02-features` | Features on, user, provider, draft pipeline |
| `S2` | After `02-features` | Data Pipelines - FTP and Data Pipelines - SFTP enabled |
| `S3` | After `03-link` | The draft holds **Share a download link** |
| `S4` | After `04-email` | ... and **Send by email** |
| `S5` | After `05-ftp` | ... and **Upload to an FTP server** |
| `S6` | After `06-sftp` | ... and **Upload to an SFTP server** |
| `S7` | After `07-run` | The pipeline is published, one run ended, one shared link exists |

The clips depend on each other and are recorded in the order they play: `02-features`, `03-link`, `04-email`,
`05-ftp`, `06-sftp`, `07-run`, `08-links`. Clips `02` to `07` change the site (`02` enables features, `03` to `06` add
steps to the draft, `07` publishes, runs, and creates the link, the email and the uploaded files). `08-links` changes
nothing: never click **Revoke the link** or **Delete** on camera. Before recording `07-run` again, restore `S6` and
create the FTP container again (the SFTP container keeps its key, so its fingerprint stays the same); empty the mailbox.

**Timing.** `ftp-1` and `sftp-1` pack many actions (the list, the editor loading, the reload after the password) into about 13 to 16 seconds of narration. Type with a delay of 30 ms. If either step still overruns the narration, move the **Password** entry to the start of the next step (`ftp-2`, `sftp-2`), and record the clip again.

Every clip: open its page with `OpenAsync` before `StartCaptureAsync`, and end the clip with a wait until the
toolbar of the designer shows **Draft saved** (the designer saves the draft a moment after each change; the next clip
starts from that draft).

## Common to the designer clips

The designer is the page `/Admin/DataPipelines/blogpostsexport/Edit`. Its labels and `data-cy` values are read from
`Assets/designer/src` of the Data Pipelines module (`App.vue`, `canvas/`, `panel/`).

- A card is `[data-cy="step-<id>"]`: `step-source`, `step-file`. A new card gets a generated id: find it by the title
  inside it (`Locator(".dpd-node", new() { HasText = "Upload to an FTP server" })`).
- The **Files** output of **Create a file** is `[data-cy="port-file-out-Output"]` (`StepPort.vue`). Clicking an output
  opens the list of steps only while the output is not connected (`DesignerCanvas.vue:205`), so the first destination
  is added this way and the others from the menu of the card, below.
- The menu of a card: `RightClickAsync` on the card (`StepNode.vue:189`). Its item **Add a step after it...** is
  `[data-cy="menu-add-after-Output"]` (`DesignerCanvas.vue:258-260`).
- The list that opens (`QuickAddMenu.vue`): the title **Add a step after Files**, the search box
  `[data-cy="quick-add-search"]` (placeholder **Search the steps**), and one item per step,
  `[data-cy="quick-add-<StepName>"]`: `ShareDownloadLink`, `SendByEmail`, `UploadToFtp`, `UploadToSftp`. The search
  matches the names, the descriptions and the categories (`toolbox/filter.ts`).
- A step added from the list is placed to the right of its source, connected to it, selected, and its panel opens on
  the **Settings** tab (`App.vue:addStepWith`). The panel is `[data-cy="step-panel"]`; its fields are found by their
  label inside it: `Locator("[data-cy='step-panel']").GetByLabel("Recipients", new() { Exact = true })`. Close it with
  `[data-cy="step-panel-close"]`.
- The settings are applied as they are typed or selected. **Press Tab after typing** in a field, as the text box
  applies on blur. A field that already holds a default (a subject, a number) is cleared first with
  `FillAsync("")`, then typed with `TypeAsync(field, text, 30)`.
- A secret field (**Password**) reloads the editor once its value is applied; wait for the hint **Leave empty to keep
  the current password.** under it before the next field.
- The toolbar: **Publish** `[data-cy="toolbar-publish"]`, **Run** `[data-cy="toolbar-run"]`
  (`App.vue:759-772`). The draft state shows as **Draft saved** next to **Add a data source**.

## Clips

### `00-intro`, `99-outro`

Cards. Nothing to record.

### Chapter 1, slides (`01-why`)

- `why-1`: the slide **The last step of a pipeline**: three cards, **Receives files**, **Delivers them**, **One output,
  several destinations**.
- `why-2`: the slide **Four destinations in this video**: four cards, **Share a download link**, **Send by email**, **Upload to an
  FTP server**, **Upload to an SFTP server**, each with the feature it needs.

### `02-features` (chapter 2)

Starts on `/Admin` (the dashboard), the Tools group collapsed. Needs `S0`. Changes the site.

- `features-1`: click **Tools** in the admin menu (`#left-nav`, the group row whose title is Tools), then **Features**
  (`ClickAndWaitForNavigationAsync`); on `/Admin/Features` type `pipelines` in the search box `#search-box`.
  *Visible at the end:* the Features page filtered by the search, with the rows **Data Pipelines** (enabled), **Data
  Pipelines - FTP** and **Data Pipelines - SFTP** (not enabled). Source: `OrchardCore.Features/AdminMenu.cs:26-27`,
  `Views/Admin/Features.cshtml:27`, `Assets/ts/features-index.ts` (the search filters the rows in the page).
- `features-2`: click the checkbox of each of the two rows, `[id="OrchardCore.DataPipelines.Ftp"]` and
  `[id="OrchardCore.DataPipelines.Sftp"]` (`FeatureEntry.Checkbox.SummaryAdmin.cshtml`); click the **Bulk Actions**
  button `#bulk-action-menu-button`; click **Enable** (`a.dropdown-item[data-action="Enable"]`, with
  `ClickAndWaitForNavigationAsync`); wait for the message of the page to appear. *Visible at the end:* the Features
  page with the green message `The following features were enabled: Data Pipelines - FTP, Data Pipelines - SFTP.`
  Source: `Features.cshtml:34-38`, `Controllers/AdminController.cs:317`.

End of clip: nothing to wait for. Take `S2`.

### `03-link` (chapter 3)

Starts on `/Admin`, the Tools group collapsed. Needs `S2`. Changes the site.

- `link-1`: click **Tools**, **Data Pipelines**, then **Pipelines** (`ClickAndWaitForNavigationAsync`); on the list,
  click the **Edit** button of the row `li[data-pipeline-id="blogpostsexport"]` (`ClickAndWaitForNavigationAsync`);
  wait for `[data-cy="step-source"]` and `[data-cy="step-file"]`. *Visible at the end:* the designer with the two cards
  **Read a data source** and **Create a file**, no panel open. Source: `DataPipelines/AdminMenu.cs:19-23`,
  `Views/Admin/Index.cshtml:84`.
- `link-2`: click `[data-cy="port-file-out-Output"]`; wait for `[data-cy="quick-add"]`; type `share` in
  `[data-cy="quick-add-search"]`; click `[data-cy="quick-add-ShareDownloadLink"]`; wait for the label **Recipients** in
  the panel. *Visible at the end:* a third card **Share a download link**, connected to **Create a file**, and its
  panel open on **Settings** with the **Recipients** box. Source: `ShareDownloadLinkStep.cs:36`.
- `link-3`: type `jamie.chen` in **Recipients**, press Tab. *Visible at the end:* **Recipients** holding `jamie.chen`,
  with its hint (`The user names or emails of the users of this site the files are shared with, separated by
  commas...`). Source: `ShareDownloadLink.Fields.Edit.cshtml:4-8`.
- `link-4`: leave **Days the link works** at `7`; clear **Maximum downloads** and type `3`; clear **Email subject** and
  type `Your blog posts export is ready`; type `The file is ready to download.` in **Message**; Tab. Scroll the panel so
  the four fields are visible. *Visible at the end:* **Days the link works** `7`, **Maximum downloads** `3`,
  **Email subject** and **Message** filled. Source: `ShareDownloadLink.Fields.Edit.cshtml:13-42`.

End of clip: wait for **Draft saved**. Take `S3`.

### `04-email` (chapter 4)

Starts on `/Admin/DataPipelines/blogpostsexport/Edit`. Needs `S3`. Changes the site.

- `email-1`: right-click `[data-cy="step-file"]`; click `[data-cy="menu-add-after-Output"]`; type `email` in
  `[data-cy="quick-add-search"]`; click `[data-cy="quick-add-SendByEmail"]`; wait for the label **To**. *Visible at
  the end:* a **Send by email** card connected to **Create a file**, its panel open on **Settings**. Source:
  `SendByEmailStep.cs:38`, `DesignerCanvas.vue:239-262`.
- `email-2`: type `reports@example.com` in **To** (exact label), Tab; highlight the **Cc** and **Bcc** boxes. *Visible
  at the end:* **To** filled, with `Email addresses, separated by commas or semicolons.` under it, and **Cc** and
  **Bcc** empty. Source: `SendByEmail.Fields.Edit.cshtml:4-27`.
- `email-3`: clear **Subject** and type `Blog posts export`; clear **Body** and type `The blog posts export is
  attached.`; Tab; highlight **The body is HTML** (leave it unchecked). *Visible at the end:* **Subject**, **Body**
  and the two checkboxes **The body is HTML** and **Send the email when there are no files to attach**. Source:
  `SendByEmail.Fields.Edit.cshtml:29-53`.
- `email-4`: scroll the panel to **Maximum size (MB)** (it holds `10`, leave it) and highlight it. *Visible at the
  end:* **Maximum size (MB)** `10` with its hint (`The step fails when the attachments weigh more...`), and the hint
  `By default, no email is sent when the step receives no files.` above it. Source:
  `SendByEmail.Fields.Edit.cshtml:52,58-62`.

End of clip: wait for **Draft saved**. Take `S4`.

### `05-ftp` (chapter 5)

Starts on the designer. Needs `S4`. Changes the site.

- `ftp-1`: right-click `[data-cy="step-file"]`; click `[data-cy="menu-add-after-Output"]`; type `ftp` in the search
  box (the list shows **Upload to an FTP server** and **Upload to an SFTP server**: the search is a substring match);
  click `[data-cy="quick-add-UploadToFtp"]`; type `localhost` in **Host**; leave **Port** at `21`; type `demo` in
  **User name** (exact label); type `<ftp-password>` (`VIDEO_FTP_PASSWORD`) in **Password**, Tab; wait for the hint
  `Leave empty to keep the current password.` *Visible at the end:* the FTP panel with **Host** `localhost`, **Port**
  `21`, **User name** `demo`, and **Password** shown as dots. Source: `UploadToFtpStep.cs:34`,
  `UploadToFtp.Fields.Edit.cshtml:4-62`.
- `ftp-2`: `SelectAsync` on **Encryption** with `None (plain FTP)`; wait for the warning
  `Plain FTP isn't encrypted: the user name, the password and the files travel in clear text.`, and highlight it.
  *Visible at the end:* **Encryption** `None (plain FTP)` and the yellow warning (the **Validate the certificate of the
  server** checkbox is gone). The options are `Explicit TLS (FTPES)`, `Implicit TLS (FTPS)` and `None (plain FTP)`.
  Source: `UploadToFtp.Fields.Edit.cshtml:21-46`.
- `ftp-3`: scroll the panel to **Remote folder**; type `/exports`; Tab; highlight **Replace existing files** (checked
  by default). *Visible at the end:* **Remote folder** `/exports` and the checked **Replace existing files**, with its
  hint. Source: `UploadToFtp.Fields.Edit.cshtml:68-81`, `UploadToFtpStepSettings.cs:55`.
- `ftp-4`: scroll the panel back to **Password** and highlight it with its hint and the **Remove the password**
  checkbox; then click `[data-cy="step-panel-close"]` and highlight the FTP card. *Visible at the end:* the canvas with
  the FTP card showing `ftp://localhost:21/exports` and, under it, the warning text **Not encrypted**. Source:
  `UploadToFtp.Fields.Design.cshtml:1-7`, `UploadToFtpStep.cs:45-56`.

End of clip: wait for **Draft saved**. Take `S5`.

### `06-sftp` (chapter 6)

Starts on the designer. Needs `S5`. Changes the site.

- `sftp-1`: right-click `[data-cy="step-file"]`; click `[data-cy="menu-add-after-Output"]`; type `sftp`; click
  `[data-cy="quick-add-UploadToSftp"]`; type `localhost` in **Host**; clear **Port** and type `2222`; type `demo` in
  **User name**; type `<sftp-password>` (`VIDEO_SFTP_PASSWORD`) in **Password**, Tab; wait for the hint under it;
  highlight the **Private key** box (leave it empty). *Visible at the end:* the SFTP panel with **Host**, **Port**
  `2222`, **User name**, the masked **Password** and the empty **Private key** with the sentence `Authenticate with a
  password, a private key, or both when the server asks for both.` Source: `UploadToSftpStep.cs:35`,
  `UploadToSftp.Fields.Edit.cshtml:4-48`.
- `sftp-2`: scroll to **Host key fingerprint**; type the value of `VIDEO_SFTP_FINGERPRINT` (`SHA256:` and 43
  characters); Tab. *Visible at the end:* **Host key fingerprint** filled, with its hint (`The SHA-256 fingerprint of
  the server's host key...`). Source: `UploadToSftp.Fields.Edit.cshtml:66-72`, `UploadToSftpStep.cs:152`.
- `sftp-3`: type `/upload/exports` in **Remote folder**, Tab; click `[data-cy="step-panel-close"]`; if the four
  destination cards are not all visible, use the control titled **Fit to the steps** of the canvas. *Visible at the
  end:* the canvas with **Create a file** and four destination cards connected to it: **Share a download link**,
  **Send by email**, **Upload to an FTP server** (with **Not encrypted**) and **Upload to an SFTP server**, which
  shows `sftp://localhost:2222/upload/exports` and no **Host key not verified** line. Source:
  `UploadToSftp.Fields.Design.cshtml`.

End of clip: wait for **Draft saved**. Take `S6`.

### `07-run` (chapter 7)

Starts on the designer. Needs `S6`. Changes the site: publishes, queues one run, sends one email, copies two files.

- `run-1`: click `[data-cy="toolbar-publish"]`; wait for `[data-cy="publish-confirm-dialog"]` (the pipeline has one
  warning, the plain FTP connection); highlight it; at about two thirds of the narration click
  `[data-cy="publish-dialog-confirm"]` (its label is **Publish anyway**). *Visible at the end:* the designer after the
  confirmation, **Publish** disabled and **Run** enabled. Source: `App.vue:769`, the bundle's publish dialog,
  `UploadToFtpStep.cs:94`.
- `run-2`: click `[data-cy="toolbar-run"]`. The **History** tab of the right panel opens with the new run selected.
  Wait off camera (`scene.OffCameraAsync`) until `Locator(".dpd-run-head.is-selected .badge", new() { HasText =
  "Succeeded" })` is visible (give it up to two minutes); the run starts when the request that queued it ends.
  *Visible at the end:* the **History** tab with the selected run and the badge **Succeeded**. Source: `App.vue:594-610`,
  `panel/RunsTab.vue`.
- `run-3`: scroll the panel to `[data-cy="run-deliveries"]` and highlight it. *Visible at the end:* the **Deliveries**
  list with four lines: `Shared 'blog-posts.csv' with jamie.chen until ...`, `Sent 'blog-posts.csv' by email to
  reports@example.com`, `Uploaded 'blog-posts.csv' to ftp://localhost:21/exports/blog-posts.csv` and `Uploaded
  'blog-posts.csv' to sftp://localhost:2222/upload/exports/blog-posts.csv` (in any order). Source:
  `ShareDownloadLinkStep.cs:100`, `SendByEmailStep.cs:174-176`, `UploadToFtpStep.cs:140`, `UploadToSftpStep.cs:157`.
- `run-4`: scroll to `[data-cy="run-log"]` and highlight the line about the FTP connection (`The connection to the FTP
  server 'localhost' isn't encrypted...`); then click `[data-cy="run-page"]` (**Open the run**,
  `ClickAndWaitForNavigationAsync`) and highlight the **Delivered** heading of the run page. *Visible at the end:* the
  page `Run of Blog posts export` scrolled to **Delivered**, with the same four lines. Source: `RunsTab.vue`,
  `Views/Admin/Run.cshtml:98`.

End of clip: nothing to wait for. Take `S7`. Open `http://localhost:8025`: the mailbox holds two emails: one to
`reports@example.com` with `blog-posts.csv` attached (**Send by email**), and one to `jamie.chen@example.com` with the
link (**Share a download link**, subject `Your blog posts export is ready`). The file is in the two servers. A different outcome means the take is not good.

### `08-links` (chapter 8)

Starts on `/Admin`, the Tools group collapsed. Needs `S7`. Changes nothing.

- `links-1`: click **Tools**, then **Shared Links** (`ClickAndWaitForNavigationAsync`); highlight the row. *Visible at
  the end:* `/Admin/SharedLinks` with one row: the title `blog-posts.csv`, the badges **Active**, **Pipeline file**,
  `jamie.chen`, `Blog posts export`, and `0 of 3 uses`. Source: `SharedLinks/SharedLinksAdminMenu.cs:19-20`,
  `Views/SharedLinksAdmin/Index.cshtml:66-79`.
- `links-2`: click **Actions** on the row (`li[data-link-id] button.actions`) and highlight the item **Revoke the
  link**, without clicking it. *Visible at the end:* the open menu with **View the source**, **Revoke the link** and
  **Delete**. Source: `Index.cshtml:97-104`.

## Checks for private data

Before assembling, look at every contact sheet and OCR the video as `SKILL.md` describes. The frames may hold only:
`admin`, `jamie.chen`, `demo`, `reports@example.com`, `jamie.chen@example.com`, `pipelines@example.com`, `localhost`,
the blog post of the Blog recipe, and the fingerprint of the throwaway SFTP key. They must not hold: a password (every
password box shows dots; no browser autofill suggestion), the address bar of a machine, a real mail address, the
mailbox at port 8025 (the email carries the link and its token), or the `ORCHARD_APP_DATA` path.
