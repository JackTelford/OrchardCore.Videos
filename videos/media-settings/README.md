# Media Settings: Options, Allowed Files and the Media API

**YouTube:** not published yet · **Length:** 4:42 · **Feature:** `OrchardCore.Media` ([documentation](https://docs.orchardcore.net/en/latest/reference/modules/Media/))

The video shows the Options and API pages under Settings, Media. The Options page is read-only: it displays the values the site
uses for allowed and restricted file extensions, upload limits, caching and storage. The API page holds the one setting
an administrator changes, how the Media Library signs in to the Media API, with Cookie or Bearer authentication. The
title, description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The whole recipe is in
`recording-plan.md`; this is the summary.

1. Build and run `src/OrchardCore.Cms.Web` of the Orchard Core repository on `http://localhost:5010`, with its own
   application data folder outside both repositories, provisioned by Auto Setup: the Blog recipe, the site name
   **Orchard Core**, the time zone `America/Los_Angeles`, and the user `admin` with the address `admin@example.com`. The
   password is chosen outside git. Add no configuration for the Media module, so the Options page shows the defaults,
   and leave the Media TUS Uploads, Media SignalR, Secure Media and every OpenID Connect feature off.
2. Sign the recorder in as `admin`, then stop the site and copy its application data folder and the saved sign-in
   aside. That copy is the snapshot to restore before a full take.
3. Create two small files outside the repository: a text file and an SVG image, named in `recording-plan.md`. They are
   the files the upload recording picks.
4. Narrate, then record the clips in this order: `02-lists`, `02-role`, `03-values`, `04-api`, `04-recipes`,
   `02-upload`. None of them depends on another. Only `02-upload` changes the site, by adding the SVG image to the media
   library, so restore the snapshot before another take of it. Never save the Bearer scheme on the API page.

`OPEN-QUESTIONS.md` lists what could not be checked without a running site, and the words to try with the voice.
