# Send Email, SMS and Notifications From Your Module

**YouTube:** not published yet · **Length:** 5:33 · **Feature:** `OrchardCore.Email` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Email/)), `OrchardCore.Sms` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Sms/)), `OrchardCore.Notifications` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Notifications/))

A video for module authors. It shows, on slides, how a class of a module sends an email with `IEmailService`, an SMS
with `ISmsService`, and a notification with `INotificationService`: what to inject, what to put in the message, and what
the result holds. It then registers a notification type, keeps notifications in sync with `INotificationManager`, and
lists the places where a module adds its own provider, notification method or email event handler. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

This video has no browser recording: the opening card, the closing card and the slides are made from
`storyboard.json` and `slides.py`, so it needs no site, no seed data and no `state.json`. The code on the slides is
trimmed from the documentation and the source of the three modules, and holds fake names and addresses only
(`jamie.chen@example.com`, `+14255550142`), so it needs no mask script.

1. Read `OPEN-QUESTIONS.md`: a person confirms the spelling the voice reads for each identifier with
   `scripts/pronounce.py` before the voice is generated.
2. Narrate and assemble from the repository root:

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/email-sms-in-code
   python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/email-sms-in-code
   ```

   There are no clips to record, so `Recorder` is not used. `recording-plan.md` lists what each slide shows.
3. Look at one frame of every slide, and at the opening and closing cards, before publishing.
