"""Publishes the videos of this repository to the Orchard Core YouTube channel (https://www.youtube.com/@OrchardCore), and keeps them in step with video.json.

Each video lives in videos/<slug>/ with a video.json that holds what YouTube shows: the title, the description,
tags, chapters, links and playlists. This tool turns that into a YouTube upload or update, and records the video's
YouTube id back into video.json and the README index. The MP4 is never committed: it is attached to a GitHub release
(see `release`), and publishing the release sends it to YouTube through the publish-video workflow.

Commands:
  python tools/youtube/youtube.py auth                         sign in once, in a browser, and keep the token
  python tools/youtube/youtube.py release <slug> --file <mp4>  create the draft GitHub release of the next version
  python tools/youtube/youtube.py publish <slug> --file <mp4>  upload with metadata, captions, thumbnail, playlists
         [--version vN] [--captions <srt>] [--thumbnail <jpg>] [--privacy private|unlisted|public] [--keep-previous]
  python tools/youtube/youtube.py update <slug>... | --all     push video.json's title, description, tags to YouTube
  python tools/youtube/youtube.py chapters <slug> --build <dir> compute chapter times from an assembled build
  python tools/youtube/youtube.py description <slug>           print the description exactly as YouTube gets it
  python tools/youtube/youtube.py docs-embed <slug> --docs <dir> write the YouTube id into the docs pages that embed it
  python tools/youtube/youtube.py index                        rewrite the video table in README.md

Authentication (see tools/youtube/README.md):
  Locally, `auth` runs the OAuth flow with the client in YOUTUBE_CLIENT_SECRETS (default: tools/youtube/client_secret.json,
  which is git-ignored) and stores the token in ~/.orchardcore-videos/youtube-token.json, outside the repository.
  In CI, YOUTUBE_CLIENT_ID, YOUTUBE_CLIENT_SECRET and YOUTUBE_REFRESH_TOKEN are used instead.
"""
import argparse
import json
import os
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
VIDEOS = ROOT / "videos"
README = ROOT / "README.md"
TOKEN = Path(os.environ.get("YOUTUBE_TOKEN") or Path.home() / ".orchardcore-videos" / "youtube-token.json")
CLIENT_SECRETS = Path(os.environ.get("YOUTUBE_CLIENT_SECRETS") or Path(__file__).with_name("client_secret.json"))
SCOPES = ["https://www.googleapis.com/auth/youtube.upload", "https://www.googleapis.com/auth/youtube.force-ssl"]
INDEX_START, INDEX_END = "<!-- videos:start -->", "<!-- videos:end -->"


# ----- video.json -----

def load(slug):
    path = VIDEOS / slug / "video.json"
    if not path.exists():
        raise SystemExit(f"No video named '{slug}': {path} does not exist.")
    return json.loads(path.read_text(encoding="utf-8"))


def save(slug, video):
    path = VIDEOS / slug / "video.json"
    path.write_text(json.dumps(video, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def slugs():
    return sorted(path.parent.name for path in VIDEOS.glob("*/video.json"))


def output_name(slug):
    storyboard = VIDEOS / slug / "storyboard.json"
    return json.loads(storyboard.read_text(encoding="utf-8")).get("output", slug) if storyboard.exists() else slug


def default_file(slug, name):
    path = VIDEOS / slug / name
    return str(path) if path.exists() else None


def to_seconds(stamp):
    seconds = 0
    for part in stamp.split(":"):
        seconds = seconds * 60 + int(part)
    return seconds


def check_chapters(video):
    """YouTube shows a video's chapters only when there are at least three, the first at 0:00, and each is at least
    10 seconds long; otherwise it silently shows none."""
    chapters = video["chapters"]
    times = [to_seconds(chapter["time"]) for chapter in chapters]

    if len(chapters) < 3 or times[0] != 0:
        raise SystemExit("YouTube needs at least three chapters, the first at 0:00.")

    if video.get("length"):
        times.append(to_seconds(video["length"]))

    short = [chapters[i]["title"] for i in range(len(times) - 1) if times[i + 1] - times[i] < 10]

    if short:
        raise SystemExit(f"YouTube shows no chapters when one is shorter than 10 seconds: {', '.join(short)}. Merge it into the chapter before it.")


def description(video):
    """The description YouTube shows: the text, then the chapters, the links and the hashtags."""
    parts = [video["description"].strip()]

    if video.get("chapters"):
        check_chapters(video)
        parts.append("Chapters:\n" + "\n".join(f'{c["time"]} {c["title"]}' for c in video["chapters"]))

    if video.get("links"):
        parts.append("\n".join(f'{link["label"]}: {link["url"]}' for link in video["links"]))

    if video.get("hashtags"):
        parts.append(" ".join("#" + tag for tag in video["hashtags"]))

    text = "\n\n".join(parts)

    if len(text) > 5000:
        raise SystemExit(f"The description is {len(text)} characters; YouTube allows 5000.")

    return text


def snippet(video):
    if len(video["title"]) > 100:
        raise SystemExit(f'The title is {len(video["title"])} characters; YouTube allows 100.')

    return {
        "title": video["title"],
        "description": description(video),
        "tags": video.get("tags", []),
        "categoryId": video.get("categoryId", "28"),
        "defaultLanguage": "en",
        "defaultAudioLanguage": "en",
    }


def github_output(**values):
    if os.environ.get("GITHUB_OUTPUT"):
        with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as outputs:
            for key, value in values.items():
                outputs.write(f"{key}={value}\n")


# ----- YouTube -----

def credentials(interactive=False):
    from google.auth.transport.requests import Request
    from google.oauth2.credentials import Credentials

    if os.environ.get("YOUTUBE_REFRESH_TOKEN"):
        creds = Credentials(
            None,
            refresh_token=os.environ["YOUTUBE_REFRESH_TOKEN"],
            client_id=os.environ["YOUTUBE_CLIENT_ID"],
            client_secret=os.environ["YOUTUBE_CLIENT_SECRET"],
            token_uri="https://oauth2.googleapis.com/token",
            scopes=SCOPES,
        )
        creds.refresh(Request())
        return creds

    creds = Credentials.from_authorized_user_file(str(TOKEN), SCOPES) if TOKEN.exists() else None

    if creds and creds.expired and creds.refresh_token:
        creds.refresh(Request())
    elif not creds or not creds.valid:
        if not interactive:
            raise SystemExit("Not signed in to YouTube: run `python tools/youtube/youtube.py auth` first.")
        from google_auth_oauthlib.flow import InstalledAppFlow

        if not CLIENT_SECRETS.exists():
            raise SystemExit(f"No OAuth client at {CLIENT_SECRETS}. See tools/youtube/README.md.")
        creds = InstalledAppFlow.from_client_secrets_file(str(CLIENT_SECRETS), SCOPES).run_local_server(port=0)

    TOKEN.parent.mkdir(parents=True, exist_ok=True)
    TOKEN.write_text(creds.to_json(), encoding="utf-8")
    return creds


def service():
    from googleapiclient.discovery import build

    return build("youtube", "v3", credentials=credentials(), cache_discovery=False)


def playlist_ids(youtube, titles):
    """Finds the channel's playlists by title, and creates the ones that don't exist yet."""
    if not titles:
        return []

    existing, page = {}, None
    while True:
        response = youtube.playlists().list(part="snippet", mine=True, maxResults=50, pageToken=page).execute()
        existing.update({item["snippet"]["title"]: item["id"] for item in response.get("items", [])})
        page = response.get("nextPageToken")
        if not page:
            break

    ids = []
    for title in titles:
        if title not in existing:
            created = youtube.playlists().insert(
                part="snippet,status",
                body={"snippet": {"title": title, "defaultLanguage": "en"}, "status": {"privacyStatus": "public"}},
            ).execute()
            existing[title] = created["id"]
            print(f"  created the playlist {title}")
        ids.append(existing[title])

    return ids


# ----- Commands -----

def cmd_auth(args):
    creds = credentials(interactive=True)
    from googleapiclient.discovery import build

    channel = build("youtube", "v3", credentials=creds, cache_discovery=False).channels().list(part="snippet", mine=True).execute()
    title = channel["items"][0]["snippet"]["title"] if channel.get("items") else "(no channel)"
    print(f"Signed in. Uploads go to the channel: {title}")
    print(f"Token saved to {TOKEN}. For GitHub, store its refresh_token as the YOUTUBE_REFRESH_TOKEN secret.")


def cmd_publish(args):
    """Uploads a video with everything YouTube takes from video.json: the metadata, the status, the captions, the
    thumbnail and the playlists. Publishing a new version of a video that is already on YouTube uploads a new video
    (YouTube can't replace a video's file) and makes the previous one unlisted, so its links keep working."""
    from googleapiclient.http import MediaFileUpload

    video = load(args.slug)
    previous = video.get("youtube") or {}

    if previous.get("id") and not args.version and not args.force:
        raise SystemExit(f'{args.slug} is already on YouTube ({previous["url"]}). Pass --version vN to publish a new version, or use `update` for metadata.')

    version = args.version or "v1"
    captions = args.captions or default_file(args.slug, f"{output_name(args.slug)}.srt")
    thumbnail = args.thumbnail or default_file(args.slug, "thumbnail.jpg")
    status = {
        "privacyStatus": args.privacy or video.get("privacyStatus", "private"),
        "selfDeclaredMadeForKids": False,
        "containsSyntheticMedia": True,
        "embeddable": True,
        "license": video.get("license", "youtube"),
    }

    # A scheduled video is uploaded private, and YouTube makes it public by itself at publishAt.
    if video.get("publishAt") and not args.privacy:
        status["privacyStatus"] = "private"
        status["publishAt"] = video["publishAt"]

    youtube = service()
    request = youtube.videos().insert(
        part="snippet,status",
        body={"snippet": snippet(video), "status": status},
        media_body=MediaFileUpload(args.file, chunksize=8 * 1024 * 1024, resumable=True),
    )

    response = None
    while response is None:
        progress, response = request.next_chunk()
        if progress:
            print(f"  uploaded {int(progress.progress() * 100)}%")

    video_id = response["id"]
    url = f"https://youtu.be/{video_id}"
    print(f"Uploaded {args.slug} {version}: {url}")

    if captions:
        youtube.captions().insert(
            part="snippet",
            body={"snippet": {"videoId": video_id, "language": "en", "name": "English", "isDraft": False}},
            media_body=MediaFileUpload(captions, mimetype="application/octet-stream"),
        ).execute()
        print(f"  captions: {Path(captions).name}")

    if thumbnail:
        try:
            youtube.thumbnails().set(videoId=video_id, media_body=MediaFileUpload(thumbnail)).execute()
            print(f"  thumbnail: {Path(thumbnail).name}")
        except Exception as error:  # Custom thumbnails need a channel that is allowed to use them.
            print(f"  thumbnail not set: {error}")

    for playlist_id in playlist_ids(youtube, video.get("playlists", [])):
        youtube.playlistItems().insert(
            part="snippet",
            body={"snippet": {"playlistId": playlist_id, "resourceId": {"kind": "youtube#video", "videoId": video_id}}},
        ).execute()

    if video.get("playlists"):
        print(f'  playlists: {", ".join(video["playlists"])}')

    history = previous.get("history", [])

    if previous.get("id"):
        history.append({"version": previous.get("version", "v1"), "id": previous["id"], "url": previous["url"]})

        if not args.keep_previous:
            youtube.videos().update(part="status", body={"id": previous["id"], "status": {"privacyStatus": "unlisted"}}).execute()
            print(f'  previous version {previous["id"]} is now unlisted')

    video["youtube"] = {"id": video_id, "url": url, "version": version}

    if history:
        video["youtube"]["history"] = history

    save(args.slug, video)
    cmd_index(args)
    github_output(id=video_id, url=url, version=version)


def cmd_release(args):
    """Creates the draft GitHub release of a video's next version, with the MP4 attached. Publishing the draft on
    GitHub is what sends the video to YouTube, through the publish-video workflow."""
    video = load(args.slug)
    listed = subprocess.run(
        ["gh", "release", "list", "--limit", "500", "--json", "tagName", "--jq", ".[].tagName"],
        capture_output=True, text=True, cwd=ROOT,
    ).stdout.split()
    local = subprocess.run(["git", "tag", "--list", f"{args.slug}/v*"], capture_output=True, text=True, cwd=ROOT).stdout.split()
    numbers = [
        int(tag.rsplit("/v", 1)[1])
        for tag in set(listed) | set(local)
        if tag.startswith(f"{args.slug}/v") and tag.rsplit("/v", 1)[1].isdigit()
    ]
    tag = f"{args.slug}/v{max(numbers, default=0) + 1}"
    title = video["title"].split(" | ")[0]
    notes = (
        f"{title}\n\n"
        f"Publishing this release uploads the attached video to YouTube with the metadata in "
        f"`videos/{args.slug}/video.json`, then opens a pull request that points the documentation at it."
    )
    command = [
        "gh", "release", "create", tag, args.file,
        "--draft", "--target", args.target,
        "--title", f"{title} ({tag.rsplit('/', 1)[1]})",
        "--notes", notes,
    ]

    if args.dry_run:
        print(" ".join(f'"{part}"' if " " in part else part for part in command))
        return

    subprocess.run(command, check=True, cwd=ROOT)
    print(f"Draft release {tag} created. Watch the video on its release page, then publish the release to send it to YouTube.")


def cmd_update(args):
    youtube = service()
    targets = slugs() if args.all else args.slugs

    for slug in targets:
        video = load(slug)
        video_id = video.get("youtube", {}).get("id")

        if not video_id:
            print(f"{slug}: not on YouTube yet, skipped")
            continue

        youtube.videos().update(part="snippet", body={"id": video_id, "snippet": snippet(video)}).execute()
        print(f"{slug}: updated https://youtu.be/{video_id}")


def cmd_chapters(args):
    """Reads the assembled segments of a build (build/<clip>.mp4, in storyboard order) and writes the chapter times."""
    import imageio_ffmpeg

    video = load(args.slug)
    storyboard = json.loads((VIDEOS / args.slug / "storyboard.json").read_text(encoding="utf-8"))
    build = Path(args.build)
    ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()

    def length(path):
        output = subprocess.run([ffmpeg, "-i", str(path)], capture_output=True, text=True).stderr
        hours, minutes, seconds = re.search(r"Duration: (\d+):(\d+):([\d.]+)", output).groups()
        return int(hours) * 3600 + int(minutes) * 60 + float(seconds)

    def stamp(seconds):
        seconds = int(seconds)
        return f"{seconds // 3600}:{seconds % 3600 // 60:02d}:{seconds % 60:02d}" if seconds >= 3600 else f"{seconds // 60}:{seconds % 60:02d}"

    chapters, elapsed = [], 0.0

    for chapter in storyboard["chapters"]:
        title = {"00": "Introduction", "99": "Wrap-up"}.get(chapter["id"], chapter["title"])
        chapters.append({"time": stamp(elapsed), "title": title})

        if not all(clip["kind"] == "card" for clip in chapter["clips"]):
            elapsed += length(build / f'card-{chapter["id"]}.mp4')

        for clip in chapter["clips"]:
            if clip["kind"] == "slide":
                elapsed += sum(length(build / f'slide-{step["id"]}.mp4') for step in clip["steps"])
            else:
                elapsed += length(build / f'{clip["id"]}.mp4')

    master = build / "master.mp4"

    if master.exists() and abs(length(master) - elapsed) > 1:
        raise SystemExit(f"The segments add up to {elapsed:.1f}s but master.mp4 is {length(master):.1f}s: check the build.")

    video["chapters"] = chapters
    video["length"] = stamp(elapsed)
    save(args.slug, video)
    print("\n".join(f'{c["time"]} {c["title"]}' for c in chapters))


def cmd_description(args):
    print(description(load(args.slug)))


EMBED = re.compile(r"<!-- video:(?P<slug>[a-z0-9-]+)(?P<attributes>[^>]*?)-->(?P<old>[ \t]*\r?\n[ \t]*<iframe[^\n]*?</iframe>)?")
ATTRIBUTE = re.compile(r'(\w+)=(?:"([^"]*)"|([^\s"]+))')
EMBED_ALLOW = "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"


def embed_html(video_id, title, start=None, end=None):
    """The iframe the Orchard Core docs use for every YouTube video (the privacy-enhanced youtube-nocookie.com host)."""
    query = "&".join(f"{name}={value}" for name, value in (("start", start), ("end", end)) if value)
    source = f"https://www.youtube-nocookie.com/embed/{video_id}" + (f"?{query}" if query else "")
    return (
        f'<iframe width="560" height="315" src="{source}" title="{title}" frameborder="0" '
        f'allow="{EMBED_ALLOW}" allowfullscreen></iframe>'
    )


def cmd_docs_embed(args):
    """Writes a video's YouTube id into the Orchard Core docs pages that embed it.

    A page marks where a video goes with an HTML comment on its own line, which renders nothing until the video is
    published:

        <!-- video:secrets-module -->
        <!-- video:secrets-module start=99 end=146 title="Video of creating a secret" -->

    This command puts the iframe on the line after each marker, and changes it when the marker is already followed
    by one. `start` and `end` (whole seconds) play one chapter of a longer video. `title` is the iframe's accessible
    name (default: the video's title). Pages are searched in --docs, the docs folder of OrchardCMS/OrchardCore
    (src/docs)."""
    video = load(args.slug)
    video_id = video.get("youtube", {}).get("id")

    if not video_id:
        raise SystemExit(f"{args.slug} is not on YouTube yet.")

    default_title = video["title"].split(" | ")[0]
    changed = []

    for page in sorted(Path(args.docs).rglob("*.md")):
        text = page.read_bytes().decode("utf-8")

        if f"<!-- video:{args.slug}" not in text:
            continue

        def replace(match):
            if match["slug"] != args.slug:
                return match[0]

            attributes = {name: quoted or bare for name, quoted, bare in ATTRIBUTE.findall(match["attributes"])}
            marker = match[0][: len(match[0]) - len(match["old"] or "")]
            newline = "\r\n" if "\r\n" in match[0] else "\n"
            iframe = embed_html(video_id, attributes.get("title", default_title), attributes.get("start"), attributes.get("end"))
            return f"{marker}{newline}{iframe}"

        updated = EMBED.sub(replace, text)

        if updated != text:
            page.write_bytes(updated.encode("utf-8"))
            changed.append(page)
            print(f"{page}: embedded {args.slug} = {video_id}")

    if not changed:
        print(f"Nothing to change: no <!-- video:{args.slug} --> marker under {args.docs} needs updating.")

    github_output(changed=str(bool(changed)).lower(), pages=",".join(page.as_posix() for page in changed))


def cmd_index(args):
    rows = ["| Video | Length | YouTube | Source |", "| --- | --- | --- | --- |"]

    for slug in slugs():
        video = load(slug)
        youtube = video.get("youtube", {}).get("url")
        source = video.get("source", {})
        pr = (
            f'[{source["repository"]}#{source["pullRequest"]}](https://github.com/{source["repository"]}/pull/{source["pullRequest"]})'
            if source.get("pullRequest") else source.get("repository", "")
        )
        link = f"[{youtube}]({youtube})" if youtube else "not uploaded"
        rows.append(f'| [{video["title"].split(" | ")[0]}](videos/{slug}) | {video.get("length", "")} | {link} | {pr} |')

    text = README.read_text(encoding="utf-8")
    start, end = text.index(INDEX_START) + len(INDEX_START), text.index(INDEX_END)
    README.write_text(text[:start] + "\n" + "\n".join(rows) + "\n" + text[end:], encoding="utf-8")
    print(f"README.md: {len(rows) - 2} videos")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    commands = parser.add_subparsers(dest="command", required=True)

    commands.add_parser("auth").set_defaults(run=cmd_auth)

    for name in ("publish", "upload"):
        publish = commands.add_parser(name)
        publish.add_argument("slug")
        publish.add_argument("--file", required=True, help="the MP4 to upload")
        publish.add_argument("--version", help="the version being published, such as v2 (from the release tag)")
        publish.add_argument("--captions", help="default: videos/<slug>/<output>.srt")
        publish.add_argument("--thumbnail", help="default: videos/<slug>/thumbnail.jpg")
        publish.add_argument("--privacy", choices=["private", "unlisted", "public"], help="default: video.json's privacyStatus")
        publish.add_argument("--keep-previous", action="store_true", help="leave the previous version as it is")
        publish.add_argument("--force", action="store_true")
        publish.set_defaults(run=cmd_publish)

    release = commands.add_parser("release")
    release.add_argument("slug")
    release.add_argument("--file", required=True, help="the MP4 to attach")
    release.add_argument("--target", default="main", help="the commit or branch the tag points to")
    release.add_argument("--dry-run", action="store_true")
    release.set_defaults(run=cmd_release)

    update = commands.add_parser("update")
    update.add_argument("slugs", nargs="*")
    update.add_argument("--all", action="store_true")
    update.set_defaults(run=cmd_update)

    chapters = commands.add_parser("chapters")
    chapters.add_argument("slug")
    chapters.add_argument("--build", required=True)
    chapters.set_defaults(run=cmd_chapters)

    describe = commands.add_parser("description")
    describe.add_argument("slug")
    describe.set_defaults(run=cmd_description)

    docs_embed = commands.add_parser("docs-embed")
    docs_embed.add_argument("slug")
    docs_embed.add_argument("--docs", required=True, help="the docs folder of OrchardCMS/OrchardCore (src/docs)")
    docs_embed.set_defaults(run=cmd_docs_embed)

    commands.add_parser("index").set_defaults(run=cmd_index)

    args = parser.parse_args()
    args.run(args)


if __name__ == "__main__":
    sys.exit(main())
