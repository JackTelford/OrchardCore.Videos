# Run Code When a Tenant Starts

**YouTube:** not published yet · **Length:** 4:06 · **Feature:** `guide:guides/run-code-on-startup` ([How to run tasks on application startup from a module](https://docs.orchardcore.net/en/latest/guides/run-code-on-startup/))

A developer video about tenant events. It shows how a module runs its own code when a tenant starts: a class that
inherits from `ModularTenantEvents`, one registration line in the module's `Startup` class, and what the console shows
after the first request. It also covers the order in which the five methods of `IModularTenantEvents` are called, what
happens when a tenant is released or removed, and how Orchard Core's own handlers deal with errors and with the state of
the tenant. The title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

There is nothing to record in a browser. Every chapter is a card or a set of slides, drawn from `storyboard.json` and
`slides.py`; the code on the slides is taken from the guide's documentation page and from the Orchard Core source, and
`recording-plan.md` lists where each excerpt comes from.

1. No Orchard Core site is needed: no Auto Setup, no `ORCHARD_APP_DATA`, no sign-in, no `state.json`, no seed data, no
   `recorder/` folder, and no clip order or snapshot to restore.
2. Narrate, then assemble (the slides are drawn by `assemble.py`; install `pillow` and `pygments` for them):

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/run-code-on-startup
   python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/run-code-on-startup
   ```

3. The storyboard's `spoken` map says the identifiers of the narration word by word
   ("I Modular Tenant Events", "Activating Async"). Listen to the first take of the voice before publishing, and
   adjust the map if a name is misread. `OPEN-QUESTIONS.md` lists what to confirm on a running site, since the
   narration was written from the source and the documentation without running them.
