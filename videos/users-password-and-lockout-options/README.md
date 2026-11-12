# Password and Lockout Options: Configure ASP.NET Core Identity

**YouTube:** not published yet · **Length:** 3:49 · **Feature:** [`guide:guides/password-configuration`](https://docs.orchardcore.net/en/latest/guides/password-configuration/) (and [`guide:guides/lockout-configuration`](https://docs.orchardcore.net/en/latest/guides/lockout-configuration/))

A video for module and web project developers. Orchard Core checks passwords and counts failed sign-ins with the
`Password` and `Lockout` options of ASP.NET Core Identity's `IdentityOptions`, and does not set them itself. The video
sets them in the `Startup` class of a module, in the `Program.cs` of the web project, and from configuration, then shows
the rules at work on the Add User form, on the sign-in page, and in the Users list. The title, description, tags and
chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only (the user `jamie`, `jamie@example.com`), so it needs no
mask script. Chapters 01 to 04 are slides (`slides.py`); only chapters 05 and 06 are recordings, in three clips.
`recording-plan.md` holds every action, selector and source reference, and `OPEN-QUESTIONS.md` what to confirm on the
first take.

1. Take a checkout of `OrchardCMS/OrchardCore`, and edit `src/OrchardCore.Cms.Web/Program.cs` so that it sets the
   `Password` and `Lockout` options the slides show (the exact code is in `recording-plan.md`). The edit is never
   committed. Run the site with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories, on
   `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name **Orchard Core**, time zone
   `America/Los_Angeles`, user `admin`, email `admin@example.com`). The password in `VIDEO_PASSWORD` must satisfy the
   configured rules.
2. Sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/users-password-and-lockout-options/recorder
   export VIDEO_ROOT=<repo>/videos/users-password-and-lockout-options VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` and `state.json` aside: restore that copy before every full take. No clip needs
   a `prepare` step.
3. Narrate and record: `scripts/narrate.py videos/users-password-and-lockout-options`, then `Recorder record 05-rules`,
   then `Recorder record 06-signin 06-unlock` in one command. `05-rules` creates the user, `06-signin` runs signed out
   and locks the user out for five minutes, and `06-unlock` needs that lockout, so it follows within five minutes.
