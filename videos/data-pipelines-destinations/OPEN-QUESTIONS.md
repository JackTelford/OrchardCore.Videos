# Open questions: Data Pipelines: Deliver Files by Link, Email, FTP and SFTP

Everything in the narration was read from the Orchard Core source (commit 92db4a83) and its documentation; nothing was
run, because the content was written without a running site. This file lists what a person must confirm on a running
site or with the voice before recording. None of it changes a fact the narration states, except where it says so.

## The voice

1. **Pronunciation.** Check with `scripts/pronounce.py` how the voice reads: FTP, SFTP, TLS, CSV, SSH, `Cc` and `Bcc`,
   `Data Pipelines - FTP` (the hyphen), `None (plain FTP)` and `Maximum size (MB)`. If an initialism is not read letter
   by letter, add a `spoken` entry with dots, such as `S.F.T.P.`. `spoken` is empty for now, since a new spelling is
   confirmed by a person.
2. **"Run now".** The labels **Run now** (the **Actions** menu of a pipeline, and the **History** tab) cannot be said
   in the narration, as the validator bans the word "now". The video uses the toolbar's **Run** button instead.

## The designer

3. **The publish dialog.** The step `run-1` names only the button **Publish anyway** (`publish-dialog-confirm`, read in
   the built designer bundle, `data-pipelines-designer.js`). The title and the message of the dialog come from the
   shared designer package, which is not in the source tree: the bundle's text is `Publish the draft?`, while
   `Assets/designer/src/i18n.ts` sets the defaults `Publish the pipeline?` and `The pipeline has {0} warnings...`. Look
   at which one the dialog shows. The narration does not depend on it.
4. **One warning only.** The narration says the plain FTP connection raises "a warning". Open the **Issues** tab before
   publishing: it must list that warning and no other (the SFTP step has its fingerprint, the other two steps have no
   warning in `DescribeAsync`). If another warning appears, the dialog still works, but the sentence must change.
5. **Selectors never run.** The `data-cy` values come from the Vue source, and the admin menu and the Features page
   from the Razor views, but no page was loaded. Confirm: `#left-nav` and its group rows; the checkboxes
   `[id="OrchardCore.DataPipelines.Ftp"]` and `[id="OrchardCore.DataPipelines.Sftp"]`; `a.dropdown-item[data-action]`;
   `li[data-pipeline-id]` and its **Edit** button; `li[data-link-id] button.actions`; the right-click menu and
   `menu-add-after-Output`.
6. **Clicking an output port.** `DesignerCanvas.vue:205` opens the list only for an output with no connection. A
   mouse click at a port may start a drag instead, depending on the pointer events; if the click does not open the
   list, use the menu of the card for the first destination too, and say `Add a step after it` in `link-2`.
7. **Four cards on the canvas.** The cards are placed to the right of **Create a file**, each at a free spot
   (`App.vue:onQuickAddPick`). Confirm that the four fit in the visible canvas at 1536 by 864, and find the control
   titled **Fit to the steps** (`FitToContent` in `i18n.ts`; its markup is in the shared package) if they do not.
8. **The save status.** The recording plan waits for the text **Draft saved** (`DraftSaved` in the bundle, from the
   shared package). After **Publish**, with no draft, the indicator reads **Saved**. Confirm both.
9. **Quick-add search.** `ftp` must list **Upload to an FTP server** and **Upload to an SFTP server** (a substring
   match, `toolbox/filter.ts`), and `sftp` only the second.
10. **The wait for the run.** A queued run starts when the request that queued it ends, or at the next minute of the
    background task (docs, "Runs"); the wait in `run-2` is off camera and allows two minutes.

## The destinations

11. **Email provider.** `GetProviderOptionsAsync` lists only enabled providers, so the plan enables and saves the SMTP
    provider before choosing it as **Default provider**. Confirm that order on the Settings, Communication, Email page,
    and where the SMTP fields sit on it.
12. **Mail catcher.** The site sends to `localhost` port 1025 with encryption **None**; confirm the Email Test message
    arrives in the catcher (Mailpit), and that the `Share a download link` email reaches it too (it is sent to the
    recipient's email address, `jamie.chen@example.com`, which the catcher accepts).
13. **The FTP stand-in.** The image `stilliard/pure-ftpd`, its variables (`PUBLICHOST`, `FTP_USER_NAME`, `FTP_USER_PASS`,
    `FTP_USER_HOME`), the passive ports 30000-30009 and the creation of `/exports` were written from memory. Log in
    with a client over plain FTP, upload a file to `/exports`, and run the video's upload once before the first take.
    On Windows, check that Docker publishes the passive ports.
14. **The SFTP stand-in.** The image `atmoz/sftp`, its user argument `demo:<password>:::upload`, the mounted host key,
    and the creation of `/upload/exports` below the `upload` folder were written from memory. Confirm the upload works.
15. **The fingerprint.** SSH.NET reports the fingerprint of the host key it negotiated; a server with several host keys
    (RSA, ECDSA, ED25519) may present another one than `ssh-keygen -lf` prints for the mounted key. The plan reads it from
    the log of a rehearsal run for that reason. Confirm that the log holds the line `isn't verified. To verify it, set
    its fingerprint: SHA256:...`, and that pasting it makes the step pass without the warning **Host key not verified**.
16. **The log line of the FTP step.** `run-4` highlights `The connection to the FTP server 'localhost' isn't
    encrypted...` in the **Log**. It is written once the connection is open (`UploadToFtpStep.cs:131`); confirm it is
    in the log of the run, and that the **Deliveries** list holds four lines.
17. **The Run page.** The page shows the rows and files of every step in its **Steps** table, and the **Delivered**
    list (`Views/Admin/Run.cshtml`). Confirm that the **Files** column holds numbers for the file and destination steps.
18. **The link.** The narration says the link is only in the email and the admin never shows it (documentation,
    "Shared links"; `ShareDownloadLinkStep.cs:99`). Open the email in the catcher once to see its link, and that following
    it as `jamie.chen`, signed in, downloads `blog-posts.csv`; do not put that email on camera, as it holds the token.
19. **The address of the link.** `SharedLinkManager.GetUrl` builds an absolute address from the current request, and falls
    back to a path without a host when there is no request (`SharedLinkManager.cs`, `GetUrl`). A run starts after the
    request that queued it, or from the background task each minute (documentation, "Runs"). Open the email in the
    catcher once: confirm the link holds the host and port of the site. Nothing on camera shows the link.
20. **The mailbox of a take.** A good take of `07-run` leaves two emails in the catcher: the one with the attachment
    sent to `reports@example.com`, and the one with the link sent to `jamie.chen@example.com`. Confirm both arrive.
