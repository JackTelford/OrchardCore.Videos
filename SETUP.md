# One-time setup

This is everything you have to configure, once, so that publishing a GitHub release uploads a video to the
[Orchard Core YouTube channel](https://www.youtube.com/@OrchardCore) and opens the pull request that embeds it in
[docs.orchardcore.net](https://docs.orchardcore.net/) (the `src/docs` folder of
[OrchardCMS/OrchardCore](https://github.com/OrchardCMS/OrchardCore)).

Nothing here is stored in the repository. You end up with **4 secrets** and **1 optional variable** in this
repository's GitHub settings, plus one token file on your machine.

| # | What | Where it ends up |
| --- | --- | --- |
| 1 | Create the GitHub repository | `<owner>/OrchardCore.Videos` |
| 2 | Google Cloud project with the YouTube Data API | Google Cloud console |
| 3 | OAuth consent screen, published to production | Google Cloud console |
| 4 | OAuth client of type **Desktop app** | `tools/youtube/client_secret.json` (git-ignored) |
| 5 | Sign in as the channel owner | `~/.orchardcore-videos/youtube-token.json` (outside the repository) |
| 6 | Store the YouTube secrets | `YOUTUBE_CLIENT_ID`, `YOUTUBE_CLIENT_SECRET`, `YOUTUBE_REFRESH_TOKEN` |
| 7 | A token for the documentation pull requests | `DOCS_REPOSITORY_TOKEN` (and the variable `DOCS_FORK_REPOSITORY`) |
| 8 | Repository settings | Actions permissions and branch protection |
| 9 | Request the YouTube API audit | Needed before API uploads can be public |
| 10 | Check it all works | |

You need: owner or manager access to the Orchard Core YouTube channel, a Google account for the Google Cloud project
(the channel owner's account is simplest), admin access to this GitHub repository, Python 3.11+, and the
[GitHub CLI](https://cli.github.com/) (`gh auth login` once).

---

## 1. Create the GitHub repository

1. Create `OrchardCore.Videos` (public) under the account or organization that will own it, for example
   `OrchardCMS/OrchardCore.Videos`. Creating it in the `OrchardCMS` organization needs an organization owner.
2. Push this folder to it:

   ```bash
   cd C:/Code/OrchardCMS/OrchardCore.Videos
   git remote add origin https://github.com/<owner>/OrchardCore.Videos.git
   git push -u origin main
   ```

   Replace `<owner>` in the commands below with the same owner. The commands pass `--repo <owner>/OrchardCore.Videos`
   so they work from any folder.

## 2. Create the Google Cloud project and enable the API

1. Open the [Google Cloud console](https://console.cloud.google.com/) signed in as the Google account that owns the
   Orchard Core channel (or any account; the account that signs in at step 5 is the one that matters).
2. Project picker (top bar) > **New project**. Name: `Orchard Core Videos`. Organization: leave it as it is. **Create**,
   then select the project.
3. **APIs & Services > Library**, search for **YouTube Data API v3**, open it, and click **Enable**.

## 3. Configure the OAuth consent screen, and publish it

The API only acts for a Google account that signed in and allowed access, which is what the consent screen is for.

1. Open **Google Auth Platform** (APIs & Services > OAuth consent screen). If it says it isn't configured, click
   **Get started**.
2. **App information**: app name `Orchard Core Videos`, user support email = your email. **Next.**
3. **Audience**: choose **External**. **Next.**
4. **Contact information**: your email. **Next**, accept the policy, **Create**.
5. **Data Access** (left menu) > **Add or remove scopes**. In *Manually add scopes*, paste these two, **Add to
   table**, **Update**, then **Save**:

   ```
   https://www.googleapis.com/auth/youtube.upload
   https://www.googleapis.com/auth/youtube.force-ssl
   ```

   Both are *sensitive* scopes. You don't need Google's app verification for your own account, but see the warning in
   step 5.
6. **Audience** (left menu) > **Test users** > **Add users**: add the email of the account that owns the channel
   (needed while the app is in *Testing*).
7. **Audience > Publishing status > Publish app** > **Confirm**, so it reads **In production**.

   > **Do not skip this.** While the app is in *Testing*, Google expires its refresh tokens after **7 days**, and the
   > `YOUTUBE_REFRESH_TOKEN` secret would stop working a week after you stored it. An unverified app in production
   > keeps working for the accounts that sign in to it; Google only shows them a warning screen first.

## 4. Create the OAuth client and save its JSON

1. **Google Auth Platform > Clients > Create client**.
2. Application type: **Desktop app**. Name: `Orchard Core Videos`. **Create.**
3. In the dialog, **Download JSON**.
4. Save it as `tools/youtube/client_secret.json` in this repository (`C:\Code\OrchardCMS\OrchardCore.Videos\tools\youtube\client_secret.json`).
   `.gitignore` already excludes `client_secret*.json`; check with `git status` that it doesn't appear.

The file looks like `{"installed": {"client_id": "...", "client_secret": "...", ...}}`.

## 5. Sign in as the channel owner

```bash
cd C:/Code/OrchardCMS/OrchardCore.Videos
pip install -r tools/youtube/requirements.txt
python tools/youtube/youtube.py auth
```

A browser opens.

1. Choose the Google account that **owns or manages the Orchard Core channel**.
2. If the channel is a *brand account*, Google asks which channel to use: pick **Orchard Core**
   (`@OrchardCore`). The token is tied to that choice.
3. On *"Google hasn't verified this app"*, click **Advanced > Go to Orchard Core Videos (unsafe)**. That is expected
   for your own unverified app.
4. Tick both permissions (*Manage your YouTube videos* and *See, edit, and permanently delete your YouTube videos...*;
   these are the `youtube.upload` and `youtube.force-ssl` scopes) and **Continue**.

The terminal prints `Signed in. Uploads go to the channel: <name>`. **Check that the name is the Orchard Core channel,
not a personal one.** If it isn't, delete `~/.orchardcore-videos/youtube-token.json` and run `auth` again, choosing the
right channel.

The token is saved in `~/.orchardcore-videos/youtube-token.json` (on Windows,
`C:\Users\<you>\.orchardcore-videos\youtube-token.json`), outside the repository. Its `refresh_token` is the third
secret below.

## 6. Store the YouTube secrets in GitHub

Run these from the repository folder. Each command reads the value from a pipe, so it never appears on screen or in
your shell history.

```bash
cd C:/Code/OrchardCMS/OrchardCore.Videos

python -c "import json;print(json.load(open('tools/youtube/client_secret.json'))['installed']['client_id'],end='')" \
  | gh secret set YOUTUBE_CLIENT_ID --repo <owner>/OrchardCore.Videos

python -c "import json;print(json.load(open('tools/youtube/client_secret.json'))['installed']['client_secret'],end='')" \
  | gh secret set YOUTUBE_CLIENT_SECRET --repo <owner>/OrchardCore.Videos

python -c "import json,pathlib;print(json.loads((pathlib.Path.home()/'.orchardcore-videos'/'youtube-token.json').read_text())['refresh_token'],end='')" \
  | gh secret set YOUTUBE_REFRESH_TOKEN --repo <owner>/OrchardCore.Videos
```

Or in the browser: repository **Settings > Secrets and variables > Actions > New repository secret**, one for each of
the three names, pasting the `client_id`, `client_secret` and `refresh_token` values from the two files.

## 7. Create the token for the documentation pull requests

After a video is published, the workflow clones `OrchardCMS/OrchardCore`, fills in the YouTube iframe after each
`<!-- video:<slug> -->` marker under `src/docs`, and opens a pull request. That needs a token that can open pull
requests in a repository you don't own. Choose **one** option.

### Option A (recommended): a fork and a classic token

The workflow pushes its branch to **your fork** of OrchardCore and opens the pull request from there, like any
contributor. Nothing is pushed to `OrchardCMS/OrchardCore` itself.

1. Fork [OrchardCMS/OrchardCore](https://github.com/OrchardCMS/OrchardCore) under the account that will own the token,
   if you haven't (for example `MikeAlhayek/OrchardCore`).
2. On that account: **Settings > Developer settings > Personal access tokens > Tokens (classic) > Generate new token
   (classic)**. Note: `Orchard Core Videos docs`. Expiration: your choice (set a reminder to renew it). Scope:
   **`public_repo`** only. Generate, and copy the token.

   A classic token is needed because a fine-grained token can't open a pull request in a repository owned by someone
   else. If `OrchardCMS` restricts classic tokens, use Option B or a GitHub App.
3. Store it, and tell the workflow which fork to use:

   ```bash
   gh secret set DOCS_REPOSITORY_TOKEN --repo <owner>/OrchardCore.Videos
   gh variable set DOCS_FORK_REPOSITORY --repo <owner>/OrchardCore.Videos --body "<your-github-user>/OrchardCore"
   ```

   The first command asks you to paste the token.

### Option B: a fine-grained token with access to OrchardCMS/OrchardCore

Only if the `OrchardCMS` organization lets you create one (it needs an organization owner to allow fine-grained
tokens, and possibly to approve yours). The workflow then pushes `videos/<slug>-vN` branches into
`OrchardCMS/OrchardCore` itself.

1. **Settings > Developer settings > Personal access tokens > Fine-grained tokens > Generate new token.**
2. **Resource owner**: `OrchardCMS`. **Repository access**: *Only select repositories* > `OrchardCore`.
3. **Repository permissions**: **Contents: Read and write**, **Pull requests: Read and write**. (Metadata: Read is
   added automatically.)
4. Generate, store it with `gh secret set DOCS_REPOSITORY_TOKEN --repo <owner>/OrchardCore.Videos`, and **don't** set
   `DOCS_FORK_REPOSITORY`.

Without `DOCS_REPOSITORY_TOKEN` everything else still works: the video is uploaded, and the workflow's log says to run
`python tools/youtube/youtube.py docs-embed <slug> --docs <OrchardCore checkout>/src/docs` and open the pull request by
hand.

## 8. Check the repository settings

1. **Settings > Actions > General > Workflow permissions**: choose **Read and write permissions**. The workflow commits
   the YouTube id back to `main` and edits the release, so it needs write access.
2. **Branch protection on `main`**: if you require pull requests or status checks, the **Record the YouTube id** step
   (`git push origin HEAD:main` as `github-actions[bot]`) will be rejected. Either add the bot to the bypass list of the
   rule (Settings > Rules > Rulesets > *Bypass list* > add **GitHub Actions**), or don't protect `main` for pushes.
3. **Settings > Actions > General > Actions permissions**: allow actions (the workflows use `actions/checkout` and
   `actions/setup-python`).

## 9. Request the YouTube API audit (needed for public uploads)

YouTube locks **every video uploaded through the API by an unaudited project to private**, and it can't be made public
afterwards. Until the project passes the audit:

- the workflow uploads fine, but the video stays private and won't play in the docs; **or**
- upload the MP4 in [YouTube Studio](https://studio.youtube.com/) by hand, put its id in `video.json`
  (`"youtube": {"id": "...", "url": "https://youtu.be/...", "version": "v1"}`), run
  `python tools/youtube/youtube.py update <slug>` for the description and chapters, and
  `python tools/youtube/youtube.py docs-embed <slug> --docs <OrchardCore checkout>/src/docs` for the docs.

To unlock public uploads, submit the free
[YouTube API Services compliance audit form](https://support.google.com/youtube/contact/yt_api_form) once, for the Google
Cloud project of step 2 (give the project number, from the console's dashboard, and say it's the Orchard Core
project that publishes the channel's own videos). It takes a few weeks. The same form can ask for more than the default
quota of 10,000 units a day (a full publish costs about 2,150, so about four videos a day).

Also make sure the channel is verified by phone (YouTube Studio > Settings > Channel > Feature eligibility): custom
thumbnails and long videos need it.

## 10. Check that it all works

1. **Credentials**: `python tools/youtube/youtube.py auth` prints `Uploads go to the channel: ...` without opening a
   browser, because the token is still valid.
2. **Description**: copy `.agents/skills/orchardcore-video-producer/templates/video/` to `videos/test/`, then
   `python tools/youtube/youtube.py description test` prints the description as YouTube gets it. (Its chapters are
   empty; run it on a real video once you have one.) Delete `videos/test/` afterwards.
3. **Secrets exist**: `gh secret list --repo <owner>/OrchardCore.Videos` lists `DOCS_REPOSITORY_TOKEN` (if you did step 7),
   `YOUTUBE_CLIENT_ID`, `YOUTUBE_CLIENT_SECRET` and `YOUTUBE_REFRESH_TOKEN`.
4. **First real video**: follow [CONTRIBUTING.md](CONTRIBUTING.md). Before the first one, use a throwaway video with
   `"privacyStatus": "private"` in its `video.json` to try the whole chain: a draft release, publishing it, the YouTube
   upload, the commit that records the id, and the pull request in your fork or in OrchardCore. Then delete the test
   video in YouTube Studio, and close the test pull request.

## Keeping it working

- **Renew the docs token** before it expires (step 7), and update the secret.
- **`invalid_grant` / "Token has been expired or revoked"** in the workflow log: the refresh token no longer works
  (the app was in *Testing*, the account's password changed, or access was removed at
  https://myaccount.google.com/permissions). Run `auth` again and redo the `YOUTUBE_REFRESH_TOKEN` command of step 6.
- **`quotaExceeded`**: ten thousand units a day were used. Run the workflow again tomorrow (**Actions > Publish video >
  Run workflow**, with the release tag).
- **Never** commit `client_secret.json` or the token file, or paste them in an issue. If one leaks, delete the OAuth
  client in the Google Cloud console (and revoke the app at https://myaccount.google.com/permissions), then redo
  steps 4 to 6.
