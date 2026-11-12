# Open questions

What could not be checked from the Orchard Core source and documentation alone, and what a person must confirm on a
running site. The narration keeps to what the source or the documentation supports. The questions below are about what
the screen looks like, or about the test mail server, which is not part of Orchard Core.

## The test mail server

1. The tracker plans smtp4dev in Docker. The command in `recording-plan.md` (`docker run --rm -p 3000:80 -p 2525:25
   rnwood/smtp4dev`), its web port, its SMTP port and the way its web page lists and opens a message are from memory, not
   from Orchard Core. Confirm them, and write the selectors of the message list and of the message view into the scene
   (`try-5`). The narration calls it "the test mail server" and names no label of its page.
2. With the SMTP settings of the plan (Network, `localhost`, port `2525`, encryption None, no credentials), confirm that the
   server accepts the message. If it needs STARTTLS, set **Encryption method to use** accordingly, and tick **Ignore
   Invalid SSL Certificate** only if the certificate is self-signed.

## Settings page

3. **Providers** and **SMTP** are the tab names: the code places the editors at `Content:1#Providers` and `Content:5#SMTP`
   (`Drivers/EmailSettingsDisplayDriver.cs:59`, `Drivers/SmtpSettingsDisplayDriver.cs:88`), and the documentation of the SMTP
   module also says "the SMTP tab". Confirm the tab strip shows those two texts. The narration says "the SMTP tab".
4. The plan expects **Port number** to be visible without scrolling on the SMTP tab at 1536 x 864; otherwise scroll.

## The designer

The designer is built with the shared `@bloom/designer` components, whose source (`.scripts/bloom`) is not in the checkout.
Their compiled code is in the bundle (`OrchardCore.Workflows/wwwroot/Scripts/Workflows/designer/workflows-designer.js`), which
was read. Checked there and in `Assets/designer/src`, so not open: a click on an outcome port without a move of 3 pixels
opens the menu "Add the next activity, after Done" (`port-click`, then `App.vue` `onAddAfter`); the toolbar text "All changes
saved" (the dictionary of `WorkflowsDesignerJSLocalizer` is also loaded into the shared components, `i18n.ts`); the toast "The
workflow was published as version 1." and the badge "Version 1" (`Publish` returns the version, `markPublished` keeps it); the
**Apply** button, always in view at the foot of the panel (`position: sticky`), and the form applying itself 0.6 s after a
change; **Saved!**, shown for about 1.8 seconds after an apply; and the resize handle of the activity panel. After a form is
applied, the server returns the activity again and the card on the canvas is replaced (`panel/ActivityPanel.vue`,
`onActivityApplied`, `api/types.ts`, `EditorApplied`), so the card reads "Article" after `event-3` and shows the subject
after `email-4`, as the narration says. What is left to look at on screen:

5. `email-1`: the menu "Add the next activity, after Done" opens beside the port, and after typing `email` the row **Email
   Task** is in view without a scroll.
6. `email-2`: dragging the top edge of the activity panel (`[data-cy="activity-panel-resizer"]`) up enlarges it. By how many
   pixels is a choice to make on screen: the panel must show **Recipients**, **Subject** and **Format** with one or two
   scrolls. The height of the panel is also kept in the browser (local storage, under the prefix
   `orchardcore:workflows-designer:` with the name `activity-panel-height`, from the designer's sources), and so is the pinned
   state. The recorder opens every clip with the browser state saved in `state.json`: create it with `Recorder setup` (the
   sign-in only), never from a browser that has used the designer, so that the panel starts at 40 % of the designer.
7. The **Content Types** picker is a bootstrap-select list (`Picker.cshtml`, the `@crestapps/bootstrap-select` resource); its
   library is not in the checkout, so the selectors for its button, its search box and its options must be found on screen. If
   the list does not close when you click outside it, press Escape. The select fires `change` itself
   (`Assets/ts/content-type-picker.ts:19`); if the card on the canvas does not change within about 1.5 s, click **Apply**.
8. **Text Body** and **HTML Body** are text areas turned into Liquid pattern editors (`Assets/ts/email-task.ts`); the editor
   that results is either `.CodeMirror` or `.cm-content`. Click inside it and type with the keyboard; `TypeAsync` on the text
   area will not work. Confirm that a change in it is sent to the form (**Apply** works in any case).

The narration of `next-4` names the **Available data** tab of an activity (key `AvailableData`), and the values of the content
item that it lists (the member `DisplayText`, "The display text, its title.", in `WorkflowValueMembers.ContentItem`). The video
never opens that tab: it shows expressions, which an administrator video does not show.

## Behaviors to confirm on the running site

9. After **Publish** of the Article, the workflow runs in the same request: `ContentsHandler.PublishedAsync` awaits
   `TriggerEventAsync`, which starts the workflow, and `EmailTask` awaits `SendAsync`. So the email is already in the test mail
   server when `try-5` opens it, unless its web page refreshes late. If it arrives a moment later, wait for the message before
   the step starts.
10. The instance ends with the status **Finished**. The code ends an instance without a blocking activity as finished
    (`WorkflowManager.ExecuteWorkflowAsync`; documentation, "Short-running workflows"), and **Failed** of the Email Task has no
    transition. If the send fails, the instance still reads Finished, so the mail server is the proof, not the status.

## Voice

11. `storyboard.json` has four entries in `spoken`: `"editors@example.com"` as "editors at example dot com", `"ReplyTo"` as
    "Reply To", `"Cc"` as "C.C." and `"Bcc"` as "B.C.C." (`narrate.py` replaces them as plain text, case-sensitive, so
    `Cc` does not touch `Bcc`). Check with `scripts/pronounce.py` how the voice reads each one with and without its entry,
    and keep the one that sounds right. Also check "SMTP", "Email Task" and the typed values that the narration quotes
    (`create-2`, `email-3`, `email-4`): the voice must not run a quoted value into the words before it.
