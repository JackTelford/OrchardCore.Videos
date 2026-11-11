"""Checks the content of every video folder against the format and the writing rules, and against the tracker.

Standard library only. Run it before every commit:

    python tools/validate_content.py                 # every video folder, and the tracker
    python tools/validate_content.py <slug> [...]    # some videos only
    python tools/validate_content.py --source <OrchardCore checkout>   # also checks that docs paths exist

It fails (exit code 1) on:

- a storyboard.json or video.json that doesn't match the format of the skill (see
  .agents/skills/orchardcore-video-producer/references/storyboard.md and templates/video/video.json): a missing or
  unknown key, a wrong kind, a chapter id that isn't two digits, a missing opening (00) or closing (99) card;
- duplicate slugs, chapter ids, clip ids or step ids;
- a "clip" step whose id isn't in recording-plan.md (as `step-id`, in backticks);
- a "slide" step without a drawing branch in slides.py (its id as a string, or a startswith() prefix of it);
- a step narration over 45 words or under 8 words;
- a video shorter than 3 or longer than 8 minutes, at 150 words per minute;
- a YouTube title over 100 characters;
- a banned word in a narration ("powerful", "seamless", "simply", "easily", "new feature", "in this release", "now");
- a version number in a narration;
- an admin video whose narration or slides hold code: braces, backticks, "appsettings", ".cs", lowercase "json"
  (the UI label "JSON", as in "JSON Import", is allowed), or a draw_code() panel;
- a video.json without a link to docs.orchardcore.net, or a tracker entry without docs;
- a tracker entry past "planned" whose folder is missing, or a folder with no tracker entry.

It prints a summary table of every video it checked.
"""
from __future__ import annotations

import argparse
import ast
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
VIDEOS = ROOT / "videos"
TRACKER = ROOT / "tracker" / "videos.json"

WORDS_PER_MINUTE = 150
MIN_MINUTES, MAX_MINUTES = 3, 8
MIN_STEP_WORDS, MAX_STEP_WORDS = 8, 45

STORYBOARD_KEYS = {"title", "output", "spoken", "chapters"}
CHAPTER_KEYS = {"id", "title", "subtitle", "clips"}
CHAPTER_OPTIONAL = {"short"}
CLIP_KEYS = {"id", "kind", "steps"}
STEP_KEYS = {"id", "text"}
KINDS = {"card", "slide", "clip"}

VIDEO_KEYS = {"title", "description", "hashtags", "tags", "categoryId", "privacyStatus", "playlists", "chapters",
              "links", "embeddedIn", "source"}
VIDEO_TOOL_KEYS = {"youtube", "length", "license", "publishAt"}  # written by tools/youtube/youtube.py
SOURCE_KEYS = {"repository", "pullRequest", "feature"}
PRIVACY = {"public", "unlisted", "private"}
DOCS_HOST = "https://docs.orchardcore.net/"

FILES = {"storyboard.json", "video.json", "recording-plan.md", "README.md", "slides.py", "OPEN-QUESTIONS.md"}
GENERATED = {"durations.json", "thumbnail.jpg", "reel.json"}  # added by the recording sessions

BANNED = [
    (re.compile(r"\bpowerful\b", re.I), "powerful"),
    (re.compile(r"\bseamless(ly)?\b", re.I), "seamless"),
    (re.compile(r"\bsimply\b", re.I), "simply"),
    (re.compile(r"\beasily\b", re.I), "easily"),
    (re.compile(r"\bnew features?\b", re.I), "new feature"),
    (re.compile(r"\bin this release\b", re.I), "in this release"),
    (re.compile(r"\bnow\b", re.I), "now"),
]
VERSION = [
    re.compile(r"\bv?\d+\.\d+(?:\.\d+)*\b"),
    re.compile(r"\bversion\s+\d", re.I),
    re.compile(r"(?:\.NET|Orchard Core|ASP\.NET Core|C#)\s+\d+\b"),
]
CODE_MARKERS = [
    (re.compile(r"[{}]"), "a brace"),
    (re.compile(r"`"), "a backtick"),
    (re.compile(r"appsettings", re.I), "appsettings"),
    (re.compile(r"\.cs\b"), ".cs"),
    (re.compile(r"json"), "json"),
]


def words(text: str) -> int:
    return len(re.findall(r"[\w'’.-]+", text))


class Report:
    def __init__(self):
        self.errors: list[str] = []
        self.rows: list[dict] = []

    def error(self, slug: str, message: str) -> None:
        self.errors.append(f"{slug}: {message}")


def load_json(path: Path, slug: str, report: Report):
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as error:
        report.error(slug, f"{path.name} isn't valid JSON: {error}")
        return None


def check_keys(slug, where, data, required, optional, report):
    if not isinstance(data, dict):
        report.error(slug, f"{where} must be an object")
        return False
    missing = required - data.keys()
    unknown = data.keys() - required - optional
    if missing:
        report.error(slug, f"{where} is missing {', '.join(sorted(missing))}")
    if unknown:
        report.error(slug, f"{where} has unknown keys {', '.join(sorted(unknown))}")
    return not missing


def slide_ids(slides: Path) -> tuple[set[str], list[str], list[str], bool]:
    """The step ids slides.py draws (string literals), the startswith() prefixes it tests, its other strings, and
    whether it draws code panels."""
    tree = ast.parse(slides.read_text(encoding="utf-8"))
    literals, prefixes, strings, code = set(), [], [], False
    docstrings = set()
    for node in ast.walk(tree):
        if isinstance(node, (ast.Module, ast.FunctionDef, ast.ClassDef)) and node.body and isinstance(node.body[0], ast.Expr) \
                and isinstance(node.body[0].value, ast.Constant) and isinstance(node.body[0].value.value, str):
            docstrings.add(id(node.body[0].value))
    for node in ast.walk(tree):
        if isinstance(node, ast.Compare) and isinstance(node.left, ast.Name) and node.left.id == "step_id":
            for comparator in node.comparators:
                for value in ast.walk(comparator):
                    if isinstance(value, ast.Constant) and isinstance(value.value, str):
                        literals.add(value.value)
        if isinstance(node, ast.Call) and isinstance(node.func, ast.Attribute) and node.func.attr == "startswith" \
                and isinstance(node.func.value, ast.Name) and node.func.value.id == "step_id":
            for arg in node.args:
                for value in ast.walk(arg):
                    if isinstance(value, ast.Constant) and isinstance(value.value, str):
                        prefixes.append(value.value)
        if isinstance(node, ast.Call) and isinstance(node.func, ast.Name) and node.func.id == "draw_code":
            code = True
        if isinstance(node, ast.Constant) and isinstance(node.value, str) and id(node) not in docstrings:
            strings.append(node.value)
    return literals, prefixes, strings, code


def check_video(slug: str, folder: Path, entry: dict | None, report: Report, source: Path | None) -> None:
    row = {"slug": slug, "audience": (entry or {}).get("audience", "?"), "chapters": 0, "steps": 0, "words": 0,
           "minutes": 0.0, "errors": 0}
    before = len(report.errors)
    audience = (entry or {}).get("audience")

    for name in ("storyboard.json", "video.json", "recording-plan.md", "README.md"):
        if not (folder / name).exists():
            report.error(slug, f"{name} is missing")
    for path in folder.iterdir():
        if path.name not in FILES | GENERATED and not path.name.endswith((".srt", ".vtt")) and path.name != "recorder":
            if not path.name.startswith(".") and path.suffix not in {".mp4", ".mp3", ".wav", ".png"}:
                report.error(slug, f"unexpected file {path.name}: a video folder holds only the files of the format")
    open_questions = folder / "OPEN-QUESTIONS.md"
    if open_questions.exists() and not open_questions.read_text(encoding="utf-8").strip():
        report.error(slug, "OPEN-QUESTIONS.md is empty: delete it")

    storyboard_path = folder / "storyboard.json"
    storyboard = load_json(storyboard_path, slug, report) if storyboard_path.exists() else None
    plan_text = (folder / "recording-plan.md").read_text(encoding="utf-8") if (folder / "recording-plan.md").exists() else ""
    slides_path = folder / "slides.py"
    narration: list[tuple[str, str]] = []

    if storyboard is not None and check_keys(slug, "storyboard.json", storyboard, STORYBOARD_KEYS, set(), report):
        if storyboard["output"] != slug:
            report.error(slug, f'storyboard.json "output" is "{storyboard["output"]}", not the slug')
        if not isinstance(storyboard["spoken"], dict) or not all(isinstance(v, str) for v in storyboard["spoken"].values()):
            report.error(slug, 'storyboard.json "spoken" must map words to how the voice says them')
        chapters = storyboard["chapters"] if isinstance(storyboard["chapters"], list) else []
        chapter_ids, clip_ids, step_ids = [], [], []
        slide_steps, clip_steps = [], []
        for chapter in chapters:
            if not check_keys(slug, f'chapter {chapter.get("id", "?")}', chapter, CHAPTER_KEYS, CHAPTER_OPTIONAL, report):
                continue
            cid = chapter["id"]
            if not re.fullmatch(r"\d\d", str(cid)):
                report.error(slug, f"chapter id {cid!r} must be two digits")
            chapter_ids.append(cid)
            kinds = set()
            for clip in chapter["clips"]:
                if not check_keys(slug, f'clip {clip.get("id", "?")}', clip, CLIP_KEYS, set(), report):
                    continue
                clip_ids.append(clip["id"])
                kinds.add(clip["kind"])
                if clip["kind"] not in KINDS:
                    report.error(slug, f'clip {clip["id"]} has kind {clip["kind"]!r}; use card, slide or clip')
                if clip["kind"] == "card" and len(clip["steps"]) != 1:
                    report.error(slug, f'card {clip["id"]} must have exactly one step')
                if not clip["steps"]:
                    report.error(slug, f'clip {clip["id"]} has no steps')
                for step in clip["steps"]:
                    if not check_keys(slug, f'step {step.get("id", "?")}', step, STEP_KEYS, set(), report):
                        continue
                    step_ids.append(step["id"])
                    narration.append((step["id"], step["text"]))
                    if clip["kind"] == "slide":
                        slide_steps.append(step["id"])
                    elif clip["kind"] == "clip":
                        clip_steps.append(step["id"])
            if cid in ("00", "99"):
                if kinds != {"card"}:
                    report.error(slug, f"chapter {cid} must hold only a card")
            elif "card" in kinds:
                report.error(slug, f"chapter {cid}: cards belong to the opening (00) and closing (99) chapters only")
        for label, ids in (("chapter", chapter_ids), ("clip", clip_ids), ("step", step_ids)):
            dupes = sorted({i for i in ids if ids.count(i) > 1})
            if dupes:
                report.error(slug, f"duplicate {label} ids: {', '.join(dupes)}")
        if chapter_ids != sorted(chapter_ids):
            report.error(slug, "chapter ids must be in increasing order")
        if not chapter_ids or chapter_ids[0] != "00" or chapter_ids[-1] != "99":
            report.error(slug, "the storyboard must open with chapter 00 and close with chapter 99")
        else:
            closing = chapters[-1]
            if closing.get("subtitle") != "docs.orchardcore.net":
                report.error(slug, 'the closing card\'s subtitle must be "docs.orchardcore.net"')
            if not closing["clips"][0]["steps"][0]["text"].rstrip().endswith("Thanks for watching."):
                report.error(slug, 'the closing card must end with "Thanks for watching."')
        if len(chapter_ids) < 3:
            report.error(slug, "a video needs at least one chapter between the opening and closing cards")
        row["chapters"] = max(0, len(chapter_ids) - 2)
        row["steps"] = len(step_ids)

        for sid in clip_steps:
            if f"`{sid}`" not in plan_text:
                report.error(slug, f"clip step {sid} has no entry in recording-plan.md (write it as `{sid}`)")
        if slide_steps:
            if not slides_path.exists():
                report.error(slug, "the storyboard has slide steps, but there is no slides.py")
            else:
                try:
                    literals, prefixes, strings, code = slide_ids(slides_path)
                except SyntaxError as error:
                    report.error(slug, f"slides.py doesn't parse: {error}")
                    literals, prefixes, strings, code = set(), [], [], False
                for sid in slide_steps:
                    if sid not in literals and not any(sid.startswith(p) for p in prefixes):
                        report.error(slug, f"slide step {sid} has no drawing branch in slides.py")
                if audience == "admin":
                    if code:
                        report.error(slug, "an admin video's slides draw a code panel (draw_code)")
                    for text in strings:
                        for pattern, name in CODE_MARKERS:
                            if pattern.search(text):
                                report.error(slug, f"an admin video's slide text holds {name}: {text[:60]!r}")
                                break
        elif slides_path.exists():
            report.error(slug, "slides.py exists, but the storyboard has no slide steps")

    total = 0
    for sid, text in narration:
        count = words(text)
        total += count
        if count > MAX_STEP_WORDS:
            report.error(slug, f"step {sid} has {count} words; at most {MAX_STEP_WORDS}")
        if count < MIN_STEP_WORDS:
            report.error(slug, f"step {sid} has {count} words; at least {MIN_STEP_WORDS}")
        for pattern, name in BANNED:
            if pattern.search(text):
                report.error(slug, f'step {sid} uses "{name}"')
        for pattern in VERSION:
            match = pattern.search(text)
            if match:
                report.error(slug, f"step {sid} has a version number: {match.group(0)!r}")
        if audience == "admin":
            for pattern, name in CODE_MARKERS:
                if pattern.search(text):
                    report.error(slug, f"step {sid} of an admin video holds {name}")
    row["words"] = total
    row["minutes"] = round(total / WORDS_PER_MINUTE, 1)
    if narration and not MIN_MINUTES <= total / WORDS_PER_MINUTE <= MAX_MINUTES:
        report.error(slug, f"{total} words is {total / WORDS_PER_MINUTE:.1f} minutes; a video is {MIN_MINUTES} to {MAX_MINUTES} minutes "
                           f"({MIN_MINUTES * WORDS_PER_MINUTE} to {MAX_MINUTES * WORDS_PER_MINUTE} words)")

    video_path = folder / "video.json"
    video = load_json(video_path, slug, report) if video_path.exists() else None
    if video is not None and check_keys(slug, "video.json", video, VIDEO_KEYS, VIDEO_TOOL_KEYS, report):
        title = video["title"]
        if len(title) > 100:
            report.error(slug, f"the title is {len(title)} characters; at most 100")
        if not title.endswith(" | Orchard Core"):
            report.error(slug, 'the title must end with " | Orchard Core"')
        for key in ("hashtags", "tags", "playlists", "chapters", "links", "embeddedIn"):
            if not isinstance(video[key], list):
                report.error(slug, f'video.json "{key}" must be a list')
        if video["privacyStatus"] not in PRIVACY:
            report.error(slug, f'privacyStatus must be one of {", ".join(sorted(PRIVACY))}')
        if not isinstance(video["description"], str) or not video["description"].strip():
            report.error(slug, "the description is empty")
        else:
            for pattern, name in BANNED:
                if pattern.search(video["description"]):
                    report.error(slug, f'the description uses "{name}"')
        links = video["links"] if isinstance(video["links"], list) else []
        if not any(isinstance(link, dict) and str(link.get("url", "")).startswith(DOCS_HOST) for link in links):
            report.error(slug, f"video.json has no link to {DOCS_HOST}")
        for link in links:
            if not isinstance(link, dict) or set(link) != {"label", "url"}:
                report.error(slug, 'each link is {"label": ..., "url": ...}')
        if isinstance(video["source"], dict):
            check_keys(slug, "video.json source", video["source"], SOURCE_KEYS, set(), report)
        if (entry or {}).get("status") not in ("published", "needs-update") and video.get("chapters"):
            report.error(slug, "chapters are filled in by youtube.py chapters after assembling; leave them empty")
        if source:
            for page in video["embeddedIn"]:
                if not (source / page).exists():
                    report.error(slug, f"embeddedIn page {page} doesn't exist in {source}")
            for link in links:
                url = str(link.get("url", ""))
                if url.startswith(DOCS_HOST + "en/latest/"):
                    rel = url[len(DOCS_HOST + "en/latest/"):].split("#")[0].rstrip("/")
                    candidates = [source / "src/docs" / rel / "README.md", source / "src/docs" / (rel + ".md")]
                    if rel and not any(c.exists() for c in candidates):
                        report.error(slug, f"the docs link {url} matches no page of {source}/src/docs")

    if entry is not None and not entry.get("docs"):
        report.error(slug, "the tracker entry has no docs")
    row["errors"] = len(report.errors) - before
    report.rows.append(row)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("slugs", nargs="*", help="The videos to check (all by default)")
    parser.add_argument("--source", type=Path, help="An Orchard Core checkout, to check that docs pages exist")
    parser.add_argument("--quiet", action="store_true", help="Print only the errors and the totals")
    args = parser.parse_args()

    report = Report()
    entries = json.loads(TRACKER.read_text(encoding="utf-8")) if TRACKER.exists() else []
    by_slug: dict[str, dict] = {}
    for entry in entries:
        if entry["slug"] in by_slug:
            report.error(entry["slug"], "duplicate slug in tracker/videos.json")
        by_slug[entry["slug"]] = entry
    folders = {p.name: p for p in VIDEOS.iterdir() if p.is_dir()} if VIDEOS.exists() else {}

    if not args.slugs:
        for slug, entry in by_slug.items():
            if entry["status"] not in ("planned",) and slug not in folders:
                report.error(slug, f'the tracker says "{entry["status"]}", but videos/{slug}/ doesn\'t exist')
        for slug in folders:
            if slug not in by_slug:
                report.error(slug, f"videos/{slug}/ has no entry in tracker/videos.json")
            elif by_slug[slug]["status"] == "planned":
                report.error(slug, f'videos/{slug}/ exists, but the tracker still says "planned"')

    for slug in args.slugs or sorted(folders):
        if slug not in folders:
            report.error(slug, f"videos/{slug}/ doesn't exist")
            continue
        check_video(slug, folders[slug], by_slug.get(slug), report, args.source)

    if report.rows and not args.quiet:
        print(f'{"Video":<42} {"Aud.":<9} {"Ch.":>3} {"Steps":>5} {"Words":>6} {"Min.":>5}  Result')
        for row in report.rows:
            result = "ok" if not row["errors"] else f'{row["errors"]} error{"s" if row["errors"] > 1 else ""}'
            print(f'{row["slug"]:<42} {row["audience"]:<9} {row["chapters"]:>3} {row["steps"]:>5} {row["words"]:>6} '
                  f'{row["minutes"]:>5}  {result}')
    for error in report.errors:
        print(f"ERROR {error}", file=sys.stderr)
    checked = len(report.rows)
    print(f"\n{checked} video folder{'s' if checked != 1 else ''} checked, {len(entries)} tracker entries, "
          f"{len(report.errors)} error{'s' if len(report.errors) != 1 else ''}.")
    return 1 if report.errors else 0


if __name__ == "__main__":
    sys.exit(main())
