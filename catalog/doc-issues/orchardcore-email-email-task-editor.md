# Documentation issues: OrchardCore.Email Email Task editor

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| ReplyTo field label in the Email Task editor (code oddity, not a documentation page) | OrchardCore.Email Email Task editor | The documentation lists no separate ReplyTo field. | The ReplyTo label is bound with asp-for="SenderExpression", so clicking the ReplyTo label focuses the Sender input instead of the ReplyTo input. | OrchardCore.Modules/OrchardCore.Email/Views/Items/EmailTask.Fields.Edit.cshtml:25 | workflows-send-email-on-publish |
