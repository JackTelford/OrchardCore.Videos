"""Builds a reel: existing screencasts joined into one video, each as a chapter, in the Orchard Core look.

Usage: python reel.py <workspace> [--source <local clone of the source repository>]

A reel is for screencasts that were recorded without narration (the silent walkthroughs of the documentation): it
doesn't record or narrate anything. reel.json lists the screencasts, by their path in a source repository at a fixed
commit, so the reel can be built again after the files are deleted from that repository:

    {
      "output": "ai-walkthroughs",
      "title": "AI walkthroughs", "subtitle": "Orchard Core",
      "source": {"repository": "OrchardCMS/OrchardCore", "ref": "<commit>"},
      "chapters": [{"title": "Connect Azure OpenAI", "file": "src/docs/reference/modules/Secrets/videos/secrets.mp4"}]
    }

A chapter can name its own "ref", for a screencast deleted from the source repository before the reel's commit, and
"redact" to hide real data that the recording shows: {"pixelate": "<regex>", "replace": {"Jordan": "John"},
"ocrScale": 2 (for small text), "boxes": [{"rect": [x, y, w, h], "from": 51, "to": 54}] (seconds, for
text OCR can't read)}, applied
by redact.py (Windows only) to the downloaded screencast before it goes in the reel.

The video is an opening card, then for each screencast a chapter card and the screencast in the frame, then a closing
card. Every screencast starts and ends on a whole second, so a page can play exactly one with YouTube's start and end:
the build writes them into reel.json ("start", "end"), and the chapters, length and embeds into video.json.

The screencasts are downloaded from GitHub into clips/ (or read with git from --source), and the build is in build/;
both are git-ignored. Writes <output>.mp4 and thumbnail.jpg in the workspace.
"""
import argparse
import json
import math
import re
import subprocess
import sys
import urllib.request
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import brand  # noqa: E402
from assemble import FFMPEG, FPS, master_audio, master_video, publish, run  # noqa: E402

OPENING_SECONDS = 4.0
CHAPTER_CARD_SECONDS = 2.6  # at least: each card is lengthened so the screencast after it starts on a whole second
CLOSING_SECONDS = 4.0
FADE = 0.35


def length(path):
    output = subprocess.run([FFMPEG, "-hide_banner", "-i", str(path)], capture_output=True, text=True).stderr
    hours, minutes, seconds = re.search(r"Duration: (\d+):(\d+):([\d.]+)", output).groups()
    return int(hours) * 3600 + int(minutes) * 60 + float(seconds)


def dimensions(path):
    output = subprocess.run([FFMPEG, "-hide_banner", "-i", str(path)], capture_output=True, text=True).stderr
    width, height = re.search(r"Video: .*?, (\d{2,5})x(\d{2,5})", output).groups()
    return int(width), int(height)


def stamp(seconds):
    seconds = int(seconds)
    return f"{seconds // 3600}:{seconds % 3600 // 60:02d}:{seconds % 60:02d}" if seconds >= 3600 else f"{seconds // 60}:{seconds % 60:02d}"


def fetch(source, path, target, clone, ref=None):
    """Gets a screencast at the reel's commit, or at the chapter's own "ref" when it was deleted before that commit."""
    ref = ref or source["ref"]
    if target.exists():
        return
    target.parent.mkdir(parents=True, exist_ok=True)
    if clone:
        data = subprocess.run(["git", "-C", clone, "show", f"{ref}:{path}"], capture_output=True, check=True).stdout
        target.write_bytes(data)
    else:
        url = f'https://raw.githubusercontent.com/{source["repository"]}/{ref}/{path}'
        print("downloading", url, flush=True)
        urllib.request.urlretrieve(url, target)


def redact(clip, spec):
    """The screencast with redact.py applied, made once and kept next to it as <name>.redacted.mp4."""
    output = clip.with_suffix(".redacted.mp4")
    if not output.exists():
        command = [sys.executable, str(Path(__file__).resolve().parent / "redact.py"), str(clip), str(output)]
        if spec.get("pixelate"):
            command += ["--pixelate", spec["pixelate"]]
        for old, new in spec.get("replace", {}).items():
            command += ["--replace", f"{old}={new}"]
        if spec.get("ocrScale"):
            command += ["--ocr-scale", str(spec["ocrScale"])]
        for box in spec.get("boxes", []):
            command += ["--box", ",".join(map(str, box["rect"])) + f'@{box["from"]}-{box["to"]}']
        subprocess.run(command, check=True)
    return output


def still(image_path, seconds, output):
    run(["-loop", "1", "-framerate", str(FPS), "-t", f"{seconds:.3f}", "-i", str(image_path),
         "-f", "lavfi", "-t", f"{seconds:.3f}", "-i", "anullsrc=r=48000:cl=stereo",
         "-vf", f"fade=t=in:st=0:d=0.4,fade=t=out:st={seconds - 0.4:.3f}:d=0.4,format=yuv420p",
         "-map", "0:v", "-map", "1:a", *master_video(), *master_audio(), "-t", f"{seconds:.3f}", str(output)])


def screencast(clip, chapter_title, label, output, build):
    """A screencast in the frame, scaled to fit it, with a silent audio track; as long as the screencast rounded up to
    a whole second, holding its last frame."""
    size = dimensions(clip)
    x, y, width, height = brand.video_box(size)
    frame_png = build / f"{output.stem}.frame.png"
    brand.frame(chapter_title, label, size).save(frame_png)
    seconds = math.ceil(length(clip))
    run(["-loop", "1", "-framerate", str(FPS), "-t", f"{seconds:.3f}", "-i", str(frame_png), "-i", str(clip),
         "-f", "lavfi", "-t", f"{seconds:.3f}", "-i", "anullsrc=r=48000:cl=stereo",
         "-filter_complex",
         f"[1:v]scale={width}:{height}:flags=lanczos,fps={FPS},setpts=PTS-STARTPTS,tpad=stop_mode=clone:stop_duration={seconds}[rec];"
         f"[0:v][rec]overlay={x}:{y}:shortest=0,trim=0:{seconds:.3f},"
         f"fade=t=in:st=0:d={FADE},fade=t=out:st={seconds - FADE:.3f}:d={FADE},format=yuv420p[vout]",
         "-map", "[vout]", "-map", "2:a", *master_video(), *master_audio(), "-t", f"{seconds:.3f}", str(output)])
    return seconds


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("workspace")
    parser.add_argument("--source", help="a local clone of the source repository, instead of downloading from GitHub")
    args = parser.parse_args()

    workspace = Path(args.workspace).resolve()
    reel = json.loads((workspace / "reel.json").read_text(encoding="utf-8"))
    video_path = workspace / "video.json"
    video = json.loads(video_path.read_text(encoding="utf-8"))
    build, clips = workspace / "build", workspace / "clips"
    build.mkdir(exist_ok=True)

    segments, chapters, clock = [], [], 0.0

    opening = build / "00-intro.png"
    brand.card(reel["title"], reel["subtitle"], big=True).save(opening)
    segments.append(build / "00-intro.mp4")
    still(opening, OPENING_SECONDS, segments[-1])
    clock += OPENING_SECONDS

    total = len(reel["chapters"])
    for number, chapter in enumerate(reel["chapters"], 1):
        name = Path(chapter["file"]).name
        print(f"encoding {number}/{total} {name}", flush=True)
        clip = clips / name
        fetch(reel["source"], chapter["file"], clip, args.source, chapter.get("ref"))
        if chapter.get("redact"):
            clip = redact(clip, chapter["redact"])
        label = f"Chapter {number}"

        # The card runs until the next whole second, so the screencast starts on one.
        card_seconds = math.ceil(clock + CHAPTER_CARD_SECONDS) - clock
        png, path = build / f"card-{number:02d}.png", build / f"card-{number:02d}.mp4"
        brand.card(chapter["title"], reel["title"], label=label).save(png)
        still(png, card_seconds, path)
        # YouTube shows chapters only when each is at least 10 seconds long, so the opening and closing cards belong
        # to the first and last chapters instead of being chapters of their own.
        chapters.append({"time": "0:00" if number == 1 else stamp(clock), "title": chapter["title"]})
        segments.append(path)
        clock += card_seconds

        path = build / f"clip-{number:02d}.mp4"
        seconds = screencast(clip, chapter["title"], label, path, build)
        segments.append(path)
        chapter["start"], chapter["end"] = round(clock), round(clock + seconds)
        clock += seconds

    closing = build / "99-outro.png"
    brand.card(reel["title"], "docs.orchardcore.net", big=True).save(closing)
    segments.append(build / "99-outro.mp4")
    still(closing, CLOSING_SECONDS, segments[-1])
    clock += CLOSING_SECONDS

    concat = build / "segments.txt"
    concat.write_text("".join(f"file '{path.as_posix()}'\n" for path in segments), encoding="utf-8")
    master = build / "master.mp4"
    run(["-f", "concat", "-safe", "0", "-i", str(concat), "-c", "copy", str(master)])

    actual = length(master)
    if abs(actual - clock) > 1:
        raise SystemExit(f"The segments add up to {clock:.1f}s but master.mp4 is {actual:.1f}s: check the build.")

    output = workspace / f'{reel["output"]}.mp4'
    print("compressing", flush=True)
    publish(master, output)

    from PIL import Image
    Image.open(opening).convert("RGB").save(workspace / "thumbnail.jpg", quality=90)

    (workspace / "reel.json").write_text(json.dumps(reel, indent=2, ensure_ascii=False) + "\n", encoding="utf-8", newline="\n")
    video["chapters"] = chapters
    video["length"] = stamp(clock)
    video_path.write_text(json.dumps(video, indent=2, ensure_ascii=False) + "\n", encoding="utf-8", newline="\n")
    print(f"{output}: {clock / 60:.1f} minutes, {total} screencasts, {output.stat().st_size / 1_000_000:.1f} MB")


if __name__ == "__main__":
    main()
