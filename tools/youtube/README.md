# Publishing to YouTube

`youtube.py` publishes the videos of this repository to the Orchard Core YouTube channel (https://www.youtube.com/@OrchardCore) through the YouTube Data API v3,
and keeps each video's YouTube metadata in its `video.json`.

```bash
pip install -r tools/youtube/requirements.txt
python tools/youtube/youtube.py auth                                   # once per machine
python tools/youtube/youtube.py release <slug> --file <mp4>             # draft release; publishing it uploads
python tools/youtube/youtube.py publish <slug> --file <mp4>             # or upload from this machine
python tools/youtube/youtube.py update <slug>                          # after editing video.json
python tools/youtube/youtube.py chapters <slug> --build videos/<slug>/build
python tools/youtube/youtube.py description <slug>                     # preview what YouTube gets
python tools/youtube/youtube.py docs-embed <slug> --docs <dir>        # fill in the docs iframes (the workflow does this)
python tools/youtube/youtube.py index                                  # rewrite the table in README.md
```

## Publishing a video

A video goes to YouTube when its **release is published**:

1. Merge the video's folder to `main` (storyboard, scenes, captions, `thumbnail.jpg`, `video.json`).
2. Create the draft release of its next version, with the MP4 attached:

   ```bash
   python tools/youtube/youtube.py release <slug> --file videos/<slug>/build/master.mp4
   ```

   This tags `<slug>/v1` (then `v2`, ...) on `main`. A draft is visible only to the repository's members: watch the
   video on the release page.
3. **Publish the release** on GitHub. The **Publish video** workflow then:
   - uploads the MP4 with the title, description, chapters, links, tags, captions, thumbnail and playlists of
     `video.json` (scheduled with `publishAt` when it is set);
   - makes the previous version unlisted when this is `v2` or later, since YouTube can't replace a video's file;
   - commits the YouTube id to `video.json` and the README index, and adds the link to the release notes;
   - opens a pull request in OrchardCMS/OrchardCore that fills in the YouTube `<iframe>` after every
     `<!-- video:<slug> -->` marker of the pages under `src/docs` (`youtube.py docs-embed`).

To run it again for a release (after adding a missing secret, say), use **Run workflow** with the tag.

Changing a published video's title, description or chapters needs no release: edit `video.json` and merge, and the
**YouTube metadata** workflow updates YouTube.

### Repository secrets and variables

| Name | Kind | What it is |
| --- | --- | --- |
| `YOUTUBE_CLIENT_ID`, `YOUTUBE_CLIENT_SECRET` | secrets | The OAuth client created below |
| `YOUTUBE_REFRESH_TOKEN` | secret | The `refresh_token` in the token file `auth` writes |
| `DOCS_REPOSITORY_TOKEN` | secret | A token that can open pull requests in **OrchardCMS/OrchardCore** (see [SETUP.md](../../SETUP.md), step 7). Without it the video is still published, and the workflow says what to run to fill in the docs by hand. |
| `DOCS_FORK_REPOSITORY` | variable (optional) | A fork of OrchardCMS/OrchardCore owned by the token's account, such as `MikeAlhayek/OrchardCore`. When set, the workflow pushes its branch there and opens the pull request from the fork. |

**[SETUP.md](../../SETUP.md) walks through every step, with the exact commands.** The summary:

## One-time setup

The API only acts for a Google account that signed in and granted access, so a person who owns or manages the Orchard Core
channel does this once:

1. In the [Google Cloud console](https://console.cloud.google.com/), create a project (for example "Orchard Core Videos")
   and enable the **YouTube Data API v3**.
2. Under **Google Auth Platform**, configure the consent screen as **External**, add the scopes
   `youtube.upload` and `youtube.force-ssl`, and add the channel owner's account as a test user. Then **publish the
   app** ("In production"): while it stays in testing, its refresh tokens expire after 7 days. An unverified app in
   production still works for the accounts that sign in to it; Google shows them a warning first.
3. Create an **OAuth client ID** of type **Desktop app**, download its JSON, and save it as
   `tools/youtube/client_secret.json` (git-ignored), or point `YOUTUBE_CLIENT_SECRETS` at it.
4. Run `python tools/youtube/youtube.py auth`, sign in with the account that owns the Orchard Core channel (pick the
   channel if it's a brand account), and allow access. The token is kept in `~/.orchardcore-videos/youtube-token.json`,
   outside the repository.

## What the API allows, and what it doesn't

- **Uploads from an unaudited API project are locked to private.** YouTube restricts every video uploaded through
  `videos.insert` by a project that hasn't passed its [API compliance audit](https://support.google.com/youtube/contact/yt_api_form)
  to private viewing, and it can't be made public. Either request the audit once, which is free and takes a few weeks,
  or upload in YouTube Studio and use this tool for everything else (captions, thumbnail, metadata updates and the
  index), which isn't restricted. Record a Studio upload's id in `video.json` as `"youtube": { "id": "...", "url": "https://youtu.be/..." }`.
- **Quota**: 10,000 units a day by default. A full publish (upload 1,600, captions 400, thumbnail 50, playlist 50,
  plus a playlist lookup) costs about 2,150, so about four videos a day; the audit form can ask for more.
- **Not in the API**: end screens, cards, monetization and pinning a comment are set in YouTube Studio.
- **Custom thumbnails** need a channel with phone verification or channel history; the tool reports when YouTube
  refuses one.
- **AI disclosure**: uploads declare synthetic media (`containsSyntheticMedia`), because the narration is a neural voice.
- **Chapters** come from the timestamps in the description; the first must be `0:00`, and there must be at least three.
