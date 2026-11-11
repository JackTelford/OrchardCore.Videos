"""The tracker of the video programme: one entry per planned video in tracker/videos.json, and its dashboards.

Standard library only.

    python tools/tracker.py status
    python tools/tracker.py list [--status S] [--series X] [--priority N] [--audience A]
    python tools/tracker.py next [--count N] [--priority P] [--status S]
    python tools/tracker.py set <slug> <status> [--by NAME] [--pr URL] [--youtube ID] [--release TAG] [--note TEXT] [--force]
    python tools/tracker.py validate
    python tools/tracker.py report [--check]
    python tools/tracker.py sync [--ref REF]

Run `set` when you start or finish a step of a video, then `report`, and commit both with the video. The statuses and
what moves a video from one to the next are in tracker/README.md.
"""
from __future__ import annotations

import argparse
import datetime
import json
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
TRACKER_DIR = ROOT / "tracker"
VIDEOS_JSON = TRACKER_DIR / "videos.json"
SERIES_JSON = TRACKER_DIR / "series.json"
NO_VIDEO = TRACKER_DIR / "NO-VIDEO.md"
COVERAGE = TRACKER_DIR / "COVERAGE.md"
CATALOG = ROOT / "catalog" / "features.json"
DASHBOARD = ROOT / "TRACKER.md"
VIDEOS = ROOT / "videos"
REPOSITORY = "https://github.com/OrchardCMS/OrchardCore.Videos"

LIFECYCLE = ["planned", "content-draft", "content-ready", "recording", "recorded", "in-review", "merged", "published"]
SIDE = ["needs-update", "blocked"]
STATUSES = LIFECYCLE + SIDE
AUDIENCES = ["admin", "developer"]
FIELDS = ["slug", "title", "series", "order", "audience", "priority", "features", "docs", "status", "estimatedMinutes",
          "words", "contentCommit", "recordedBy", "recordedOn", "reviewedBy", "pullRequest", "release", "youtubeId",
          "publishedOn", "notes"]
WORDS_PER_MINUTE = 150
START, END = "<!-- tracker:start -->", "<!-- tracker:end -->"


# ---------------------------------------------------------------------------------------------------------------------
# Data

def load_videos() -> list[dict]:
    return json.loads(VIDEOS_JSON.read_text(encoding="utf-8"))


def save_videos(videos: list[dict]) -> None:
    ordered = [{key: video[key] for key in FIELDS} for video in videos]
    VIDEOS_JSON.write_text(json.dumps(ordered, indent=2, ensure_ascii=False) + "\n", encoding="utf-8", newline="\n")


def load_series() -> list[dict]:
    return json.loads(SERIES_JSON.read_text(encoding="utf-8"))


def series_order() -> dict[str, int]:
    return {s["series"]: s["order"] for s in load_series()}


def load_catalog() -> list[dict]:
    return json.loads(CATALOG.read_text(encoding="utf-8"))["features"] if CATALOG.exists() else []


def no_video_entries() -> dict[str, str]:
    """The catalog entries tracker/NO-VIDEO.md explains: `id` in the first column of a table row, the reason last."""
    result = {}
    if NO_VIDEO.exists():
        for line in NO_VIDEO.read_text(encoding="utf-8").splitlines():
            m = re.match(r"^\|\s*`([^`]+)`\s*\|(.*)\|\s*$", line)
            if m:
                result[m.group(1)] = m.group(2).split("|")[-1].strip()
    return result


def today() -> str:
    return datetime.date.today().isoformat()


def git(*args: str) -> str:
    try:
        return subprocess.run(["git", *args], cwd=ROOT, capture_output=True, text=True, check=True).stdout.strip()
    except (subprocess.CalledProcessError, FileNotFoundError):
        return ""


def storyboard_words(slug: str) -> int:
    path = VIDEOS / slug / "storyboard.json"
    if not path.exists():
        return 0
    try:
        storyboard = json.loads(path.read_text(encoding="utf-8"))
    except json.JSONDecodeError:
        return 0
    texts = [step.get("text", "") for chapter in storyboard.get("chapters", []) for clip in chapter.get("clips", [])
             for step in clip.get("steps", [])]
    return sum(len(re.findall(r"[\w'’.-]+", text)) for text in texts)


def sort_key(video: dict, order: dict[str, int]):
    return (video["priority"], order.get(video["series"], 999), video["order"])


def find(videos: list[dict], slug: str) -> dict:
    for video in videos:
        if video["slug"] == slug:
            return video
    raise SystemExit(f"No video {slug} in tracker/videos.json")


def table(headers: list[str], rows: list[list], align: str | None = None) -> str:
    widths = [max(len(str(h)), *(len(str(r[i])) for r in rows)) if rows else len(str(h)) for i, h in enumerate(headers)]
    align = align or "l" * len(headers)

    def fmt(cells):
        return "  ".join(str(c).rjust(w) if a == "r" else str(c).ljust(w) for c, w, a in zip(cells, widths, align)).rstrip()

    return "\n".join([fmt(headers), "  ".join("-" * w for w in widths)] + [fmt(r) for r in rows])


def percent(part: int, whole: int) -> str:
    return f"{100 * part / whole:.0f}%" if whole else "-"


# ---------------------------------------------------------------------------------------------------------------------
# Commands

def cmd_status(args) -> int:
    videos = load_videos()
    order = series_order()
    shown = [s for s in STATUSES if any(v["status"] == s for v in videos)] or ["planned"]
    rows = []
    for series in sorted({v["series"] for v in videos}, key=lambda s: order.get(s, 999)):
        group = [v for v in videos if v["series"] == series]
        ready = sum(1 for v in group if v["status"] in LIFECYCLE[2:])
        published = sum(1 for v in group if v["status"] == "published")
        rows.append([series, len(group)] + [sum(1 for v in group if v["status"] == s) or "" for s in shown]
                    + [percent(ready, len(group)), percent(published, len(group))])
    ready = sum(1 for v in videos if v["status"] in LIFECYCLE[2:])
    published = sum(1 for v in videos if v["status"] == "published")
    rows.append(["Total", len(videos)] + [sum(1 for v in videos if v["status"] == s) for s in shown]
                + [percent(ready, len(videos)), percent(published, len(videos))])
    print(table(["Series", "Videos"] + shown + ["Content", "Published"], rows, "lr" + "r" * len(shown) + "rr"))
    print()
    print(table(["Status", "Videos", "Share"],
                [[s, sum(1 for v in videos if v["status"] == s), percent(sum(1 for v in videos if v["status"] == s), len(videos))]
                 for s in STATUSES], "lrr"))
    print("\nContent: content-ready or later. Published: on YouTube.")
    return 0


def filtered(videos, status=None, series=None, priority=None, audience=None):
    result = videos
    if status:
        result = [v for v in result if v["status"] == status]
    if series:
        result = [v for v in result if v["series"].lower() == series.lower()]
    if priority:
        result = [v for v in result if v["priority"] == priority]
    if audience:
        result = [v for v in result if v["audience"] == audience]
    return result


def print_videos(videos: list[dict]) -> None:
    rows = [[v["slug"], v["status"], v["series"], v["order"], v["audience"], v["priority"], v["estimatedMinutes"], v["title"]]
            for v in videos]
    print(table(["Slug", "Status", "Series", "#", "Audience", "P", "Min", "Title"], rows, "lllrlrrl"))
    print(f"\n{len(videos)} video{'s' if len(videos) != 1 else ''}")


def cmd_list(args) -> int:
    order = series_order()
    videos = filtered(load_videos(), args.status, args.series, args.priority, args.audience)
    videos.sort(key=lambda v: (order.get(v["series"], 999), v["order"]))
    print_videos(videos)
    return 0


def cmd_next(args) -> int:
    order = series_order()
    videos = filtered(load_videos(), args.status, None, args.priority, args.audience)
    videos.sort(key=lambda v: sort_key(v, order))
    print_videos(videos[:args.count])
    return 0


def cmd_set(args) -> int:
    if args.status not in STATUSES:
        raise SystemExit(f"Unknown status {args.status}: use one of {', '.join(STATUSES)}")
    videos = load_videos()
    video = find(videos, args.slug)
    current, target = video["status"], args.status
    if current == target and not args.force:
        raise SystemExit(f"{args.slug} is already {target}")

    if target in SIDE:
        if not args.note:
            raise SystemExit(f"Say why with --note when you set {target}")
    elif current in LIFECYCLE and LIFECYCLE.index(target) > LIFECYCLE.index(current) + 1 and not args.force:
        skipped = LIFECYCLE[LIFECYCLE.index(current) + 1:LIFECYCLE.index(target)]
        raise SystemExit(f"{args.slug} is {current}: going to {target} skips {', '.join(skipped)}. Use --force if that's intended.")
    elif current == "needs-update" and target not in ("content-draft", "content-ready") and not args.force:
        raise SystemExit(f"{args.slug} needs an update: revise its content first (content-draft), or use --force")

    folder = VIDEOS / args.slug
    if target in LIFECYCLE[1:] and not folder.exists() and not args.force:
        raise SystemExit(f"videos/{args.slug}/ doesn't exist")

    if target in ("content-draft", "content-ready"):
        words = storyboard_words(args.slug)
        if words:
            video["words"] = words
            video["estimatedMinutes"] = round(words / WORDS_PER_MINUTE, 1)
    if target == "content-ready" and not args.force:
        result = subprocess.run([sys.executable, str(ROOT / "tools" / "validate_content.py"), args.slug, "--quiet"],
                                cwd=ROOT, capture_output=True, text=True)
        if result.returncode:
            sys.stderr.write(result.stderr)
            raise SystemExit(f"{args.slug} doesn't pass tools/validate_content.py: it stays {current}")
        video["contentCommit"] = git("log", "-1", "--format=%h", "--", f"videos/{args.slug}") or video["contentCommit"]
    if target == "recording":
        if not (args.by or video["recordedBy"]):
            raise SystemExit("Say who records it with --by")
        video["recordedBy"] = args.by or video["recordedBy"]
    if target == "recorded":
        video["recordedOn"] = today()
        if args.by:
            video["recordedBy"] = args.by
    if target == "in-review":
        if not (args.pr or video["pullRequest"]):
            raise SystemExit("Give the pull request with --pr")
    if target == "merged" and args.by:
        video["reviewedBy"] = args.by
    if target == "published":
        youtube = args.youtube or youtube_id(args.slug) or video["youtubeId"]
        if not youtube:
            raise SystemExit("Give the YouTube id with --youtube (or let the publish workflow write it to video.json)")
        video["youtubeId"] = youtube
        video["publishedOn"] = video["publishedOn"] if current == "published" else today()
        video["release"] = args.release or latest_release(args.slug) or video["release"]
    if args.pr:
        video["pullRequest"] = args.pr
    if args.release and target != "published":
        video["release"] = args.release
    if args.note:
        entry = f"{today()} {target}: {args.note}"
        video["notes"] = f"{video['notes']} | {entry}" if video["notes"] else entry

    video["status"] = target
    save_videos(videos)
    print(f"{args.slug}: {current} -> {target}. Commit videos/{args.slug}/ with this entry of tracker/videos.json (TRACKER.md is refreshed in its own pull request).")
    return 0


def youtube_id(slug: str) -> str:
    path = VIDEOS / slug / "video.json"
    if not path.exists():
        return ""
    try:
        return (json.loads(path.read_text(encoding="utf-8")).get("youtube") or {}).get("id", "")
    except json.JSONDecodeError:
        return ""


def latest_release(slug: str) -> str:
    tags = git("tag", "--list", f"{slug}/v*", "--sort=-v:refname").splitlines()
    return tags[0] if tags else ""


def validate() -> list[str]:
    errors = []
    videos = load_videos()
    series = load_series()
    names = {s["series"] for s in series}
    catalog = load_catalog()
    ids = {e["id"] for e in catalog}
    explained = no_video_entries()

    if [s["order"] for s in series] != list(range(1, len(series) + 1)):
        errors.append("tracker/series.json: orders must be 1, 2, 3, ...")
    seen, places = set(), set()
    for v in videos:
        slug = v.get("slug", "?")
        if list(v.keys()) != FIELDS:
            missing = [k for k in FIELDS if k not in v]
            unknown = [k for k in v if k not in FIELDS]
            errors.append(f"{slug}: fields must be {', '.join(FIELDS)} in that order"
                          + (f"; missing {', '.join(missing)}" if missing else "") + (f"; unknown {', '.join(unknown)}" if unknown else ""))
            continue
        if slug in seen:
            errors.append(f"{slug}: duplicate slug")
        seen.add(slug)
        if not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", slug):
            errors.append(f"{slug}: a slug is lowercase kebab-case")
        if v["series"] not in names:
            errors.append(f"{slug}: series {v['series']!r} isn't in tracker/series.json")
        if (v["series"], v["order"]) in places:
            errors.append(f"{slug}: another video of {v['series']} has order {v['order']}")
        places.add((v["series"], v["order"]))
        if v["status"] not in STATUSES:
            errors.append(f"{slug}: unknown status {v['status']!r}")
        if v["audience"] not in AUDIENCES:
            errors.append(f"{slug}: audience is admin or developer")
        if v["priority"] not in (1, 2, 3):
            errors.append(f"{slug}: priority is 1, 2 or 3")
        if len(v["title"]) + len(" | Orchard Core") > 100:
            errors.append(f"{slug}: the title is too long for YouTube once \" | Orchard Core\" is added")
        if not v["features"]:
            errors.append(f"{slug}: no features")
        for f in v["features"]:
            if catalog and f not in ids:
                errors.append(f"{slug}: feature {f} isn't in catalog/features.json")
        if not v["docs"]:
            errors.append(f"{slug}: no docs")
        status = v["status"]
        folder = VIDEOS / slug
        if status != "planned" and not folder.exists():
            errors.append(f"{slug}: status {status}, but videos/{slug}/ doesn't exist")
        if status in LIFECYCLE[2:] and not v["words"]:
            errors.append(f"{slug}: status {status}, but words is 0")
        if status in LIFECYCLE[3:] and not v["recordedBy"]:
            errors.append(f"{slug}: status {status}, but recordedBy is empty")
        if status in LIFECYCLE[4:] and not v["recordedOn"]:
            errors.append(f"{slug}: status {status}, but recordedOn is empty")
        if status in LIFECYCLE[5:] and not v["pullRequest"]:
            errors.append(f"{slug}: status {status}, but pullRequest is empty")
        if status in SIDE and not v["notes"]:
            errors.append(f"{slug}: status {status} needs a note saying why")
        on_youtube = youtube_id(slug)
        if on_youtube and status not in ("published", "needs-update"):
            errors.append(f"{slug}: video.json has the YouTube id {on_youtube}, but the status is {status}: run sync")
        if on_youtube and v["youtubeId"] and on_youtube != v["youtubeId"]:
            errors.append(f"{slug}: video.json has the YouTube id {on_youtube}, the tracker {v['youtubeId']}")
        if status == "published" and not (v["youtubeId"] and v["publishedOn"]):
            errors.append(f"{slug}: published, but youtubeId or publishedOn is empty")
        if v["youtubeId"] and status not in ("published", "needs-update"):
            errors.append(f"{slug}: has a YouTube id, so it must be published")
    if VIDEOS.exists():
        for folder in sorted(p for p in VIDEOS.iterdir() if p.is_dir()):
            if folder.name not in seen:
                errors.append(f"videos/{folder.name}/ has no entry in tracker/videos.json")
            elif next(v for v in videos if v["slug"] == folder.name)["status"] == "planned":
                errors.append(f"videos/{folder.name}/ exists, but the tracker still says planned")

    covered = {f for v in videos for f in v["features"]}
    for e in catalog:
        if e["id"] not in covered and e["id"] not in explained:
            errors.append(f"catalog entry {e['id']} has no video, and tracker/NO-VIDEO.md doesn't explain why")
    for key in explained:
        if catalog and key not in ids:
            errors.append(f"tracker/NO-VIDEO.md lists {key}, which isn't in catalog/features.json")
        if key in covered:
            errors.append(f"tracker/NO-VIDEO.md lists {key}, but a video covers it: remove it from NO-VIDEO.md")
    for entry in series:
        if not any(v["series"] == entry["series"] for v in videos):
            errors.append(f"series {entry['series']} has no video")
    return errors


def cmd_validate(args) -> int:
    errors = validate()
    for error in errors:
        print(f"ERROR {error}", file=sys.stderr)
    videos = load_videos()
    print(f"{len(videos)} videos, {len(load_series())} series, {len(load_catalog())} catalog entries: "
          f"{len(errors)} error{'s' if len(errors) != 1 else ''}.")
    return 1 if errors else 0


# ---------------------------------------------------------------------------------------------------------------------
# Report

def bar(done: int, total: int, width: int = 20) -> str:
    filled = round(width * done / total) if total else 0
    return "█" * filled + "░" * (width - filled)


def youtube_url(video_id: str) -> str:
    return f"https://www.youtube.com/watch?v={video_id}"


def dashboard() -> str:
    videos = load_videos()
    order = series_order()
    series = load_series()
    total = len(videos)
    count = {s: sum(1 for v in videos if v["status"] == s) for s in STATUSES}
    content = sum(1 for v in videos if v["status"] in LIFECYCLE[2:])
    published = count["published"]
    minutes = sum(v["estimatedMinutes"] for v in videos)
    lines = [
        START,
        "",
        f"**{total} videos** in {len(series)} series, about {minutes / 60:.0f} hours in all. "
        f"Content written: **{content}** ({percent(content, total)}). Published: **{published}** ({percent(published, total)}).",
        "",
        "| Status | Videos |",
        "| --- | ---: |",
    ]
    lines += [f"| `{s}` | {count[s]} |" for s in STATUSES]
    lines += ["", "## Progress by series", "",
              "Content is the share of videos that are `content-ready` or later; published, the share on YouTube.", "",
              "| Path | Series | Videos | Content | Published |", "| --- | --- | ---: | --- | --- |"]
    for entry in series:
        group = [v for v in videos if v["series"] == entry["series"]]
        done = sum(1 for v in group if v["status"] in LIFECYCLE[2:])
        live = sum(1 for v in group if v["status"] == "published")
        lines.append(f"| {entry['path']} | {entry['series']} | {len(group)} | `{bar(done, len(group), 10)}` {done} "
                     f"| `{bar(live, len(group), 10)}` {live} |")
    lines += ["", "## Every video", "",
              "In series order. P is the priority: 1 core, 2 common, 3 niche.", "",
              "| Series | # | Video | Audience | P | Min | Status | Links |", "| --- | ---: | --- | --- | ---: | ---: | --- | --- |"]
    for v in sorted(videos, key=lambda v: (order.get(v["series"], 999), v["order"])):
        links = []
        if (VIDEOS / v["slug"]).exists():
            links.append(f"[folder](videos/{v['slug']})")
        if v["pullRequest"]:
            links.append(f"[PR]({v['pullRequest']})")
        if v["youtubeId"]:
            links.append(f"[YouTube]({youtube_url(v['youtubeId'])})")
        title = v["title"].replace("|", "\\|")
        lines.append(f"| {v['series']} | {v['order']} | {title} (`{v['slug']}`) | {v['audience']} | {v['priority']} "
                     f"| {v['estimatedMinutes']:g} | `{v['status']}` | {' · '.join(links)} |")
    lines += ["", f"Generated by `python tools/tracker.py report`. See [tracker/README.md](tracker/README.md).", "", END]
    return "\n".join(lines)


def coverage() -> str:
    videos = load_videos()
    catalog = load_catalog()
    explained = no_video_entries()
    by_feature: dict[str, list[str]] = {}
    for v in videos:
        for f in v["features"]:
            by_feature.setdefault(f, []).append(v["slug"])
    covered = sum(1 for e in catalog if e["id"] in by_feature)
    lines = [
        "# Coverage",
        "",
        "<!-- Generated by `python tools/tracker.py report`: don't edit by hand. -->",
        "",
        f"Every entry of `catalog/features.json` and the videos that cover it. {covered} of {len(catalog)} entries have at "
        f"least one video; {len(explained)} are explained in [NO-VIDEO.md](NO-VIDEO.md); "
        f"{len(catalog) - covered - len([k for k in explained if k not in by_feature])} have neither.",
        "",
        "| Entry | Kind | Category | Audience | Videos |",
        "| --- | --- | --- | --- | --- |",
    ]
    for e in catalog:
        if e["id"] in by_feature:
            cell = ", ".join(f"`{s}`" for s in by_feature[e["id"]])
        elif e["id"] in explained:
            cell = "none: see [NO-VIDEO.md](NO-VIDEO.md)"
        else:
            cell = "**none**"
        lines.append(f"| `{e['id']}` | {e['kind']} | {e['category']} | {e['audience']} | {cell} |")
    return "\n".join(lines) + "\n"


def cmd_report(args) -> int:
    text = dashboard()
    current = DASHBOARD.read_text(encoding="utf-8") if DASHBOARD.exists() else ""
    if START in current and END in current:
        updated = current[:current.index(START)] + text + current[current.index(END) + len(END):]
    else:
        updated = ("# Video tracker\n\nThe progress of every planned Orchard Core video. The source of truth is "
                   "`tracker/videos.json`; this page is generated from it.\n\n" + text + "\n")
    cover = coverage()
    if args.check:
        stale = [name for name, path, content in (("TRACKER.md", DASHBOARD, updated), ("tracker/COVERAGE.md", COVERAGE, cover))
                 if not path.exists() or path.read_text(encoding="utf-8") != content]
        if stale:
            print(f"Out of date: {', '.join(stale)}. Run `python tools/tracker.py report` and commit it.", file=sys.stderr)
            return 1
        print("TRACKER.md and tracker/COVERAGE.md are up to date.")
        return 0
    DASHBOARD.write_text(updated, encoding="utf-8", newline="\n")
    COVERAGE.write_text(cover, encoding="utf-8", newline="\n")
    print("Wrote TRACKER.md and tracker/COVERAGE.md.")
    return 0


def cmd_sync(args) -> int:
    videos = load_videos()
    ref = args.ref or ("origin/main" if git("rev-parse", "--verify", "--quiet", "origin/main") else "main")
    changes = []
    for v in videos:
        slug, status = v["slug"], v["status"]
        folder = VIDEOS / slug
        words = storyboard_words(slug)
        if words and status in LIFECYCLE[1:3] and words != v["words"]:
            v["words"] = words
            v["estimatedMinutes"] = round(words / WORDS_PER_MINUTE, 1)
            changes.append(f"{slug}: {words} words")
        if status == "planned" and (folder / "storyboard.json").exists():
            v["status"] = "content-draft"
            changes.append(f"{slug}: planned -> content-draft (its storyboard exists)")
        if v["status"] in LIFECYCLE[2:] and not v["contentCommit"]:
            commit = git("log", "-1", "--format=%h", "--", f"videos/{slug}/storyboard.json")
            if commit:
                v["contentCommit"] = commit
                changes.append(f"{slug}: contentCommit {commit}")
        if v["status"] in ("recording", "recorded", "in-review") and subprocess.run(
                ["git", "cat-file", "-e", f"{ref}:videos/{slug}/durations.json"], cwd=ROOT, capture_output=True).returncode == 0:
            changes.append(f"{slug}: {v['status']} -> merged (its recording is on {ref})")
            v["status"] = "merged"
        on_youtube = youtube_id(slug)
        if on_youtube and v["status"] not in ("published", "needs-update"):
            changes.append(f"{slug}: {v['status']} -> published ({on_youtube})")
            v["status"] = "published"
            v["youtubeId"] = on_youtube
            v["publishedOn"] = v["publishedOn"] or today()
            v["release"] = latest_release(slug) or v["release"]
        elif on_youtube and v["youtubeId"] != on_youtube:
            changes.append(f"{slug}: YouTube id {v['youtubeId'] or '(none)'} -> {on_youtube}")
            v["youtubeId"] = on_youtube
    save_videos(videos)
    print("\n".join(changes) if changes else "Nothing to update.")
    if changes:
        print("Run `python tools/tracker.py report`, and commit tracker/ and TRACKER.md.")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    commands = parser.add_subparsers(dest="command", required=True)
    commands.add_parser("status", help="A table by series and by status").set_defaults(run=cmd_status)
    for name, run, help_text in (("list", cmd_list, "A filtered list"), ("next", cmd_next, "The next videos to work on")):
        p = commands.add_parser(name, help=help_text)
        p.add_argument("--status", default="content-ready" if name == "next" else None,
                       help="content-ready by default for next (the next to record); planned gives the next to write")
        if name == "list":
            p.add_argument("--series")
        else:
            p.add_argument("--count", type=int, default=10)
        p.add_argument("--priority", type=int)
        p.add_argument("--audience", choices=AUDIENCES)
        p.set_defaults(run=run)
    s = commands.add_parser("set", help="Change the status of a video")
    s.add_argument("slug")
    s.add_argument("status", choices=STATUSES)
    s.add_argument("--by", help="Who records (recording, recorded) or reviewed (merged)")
    s.add_argument("--pr", help="The pull request URL (in-review)")
    s.add_argument("--youtube", help="The YouTube id (published)")
    s.add_argument("--release", help="The release tag, such as <slug>/v1 (published)")
    s.add_argument("--note", help="Why (required for blocked and needs-update)")
    s.add_argument("--force", action="store_true", help="Allow a jump that skips a step, or a status that doesn't validate")
    s.set_defaults(run=cmd_set)
    commands.add_parser("validate", help="Check the tracker against the videos, the catalog and video.json").set_defaults(run=cmd_validate)
    r = commands.add_parser("report", help="Rewrite TRACKER.md and tracker/COVERAGE.md")
    r.add_argument("--check", action="store_true", help="Fail if they are out of date, without writing them")
    r.set_defaults(run=cmd_report)
    y = commands.add_parser("sync", help="Catch up statuses from the folders, git and video.json")
    y.add_argument("--ref", help="The branch merged folders are on (origin/main, or main)")
    y.set_defaults(run=cmd_sync)
    args = parser.parse_args()
    return args.run(args)


if __name__ == "__main__":
    sys.exit(main())
