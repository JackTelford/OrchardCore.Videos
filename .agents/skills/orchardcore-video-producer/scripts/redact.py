"""Hides words in a screencast: pixelates the ones that must not be seen, and paints a replacement over others.

Usage: python redact.py <in.mp4> <out.mp4> [--pixelate REGEX] [--replace WORDS=NEW ...] [--box X,Y,W,H@FROM-TO ...]

Every frame is read with the Windows OCR engine (ocr-words.ps1, so this runs on Windows only). A word matching
--pixelate is pixelated, on its frame and on the frames around it, since OCR misses a word on some frames and a word
being typed shows up before it is complete. Words equal to a --replace WORDS (ignoring case and the
punctuation after them) are covered with the background color and NEW is written in their place, at their size,
color and weight, so a real name becomes a fake one. WORDS can be a phrase ("Jordan Rivera=John Smith"), so "Rivera"
on its own is left alone when another person shares the surname.

Always check the result: OCR it again (the review script, or a scan at 5 fps and at every scene change), because OCR
can miss a word on every frame of a short flash. Never publish a screencast that still shows real data.
"""
import argparse
import collections
import os
import re
import shutil
import subprocess
import tempfile
from pathlib import Path

import imageio_ffmpeg
from PIL import Image, ImageChops, ImageDraw, ImageFont, ImageStat

FFMPEG = imageio_ffmpeg.get_ffmpeg_exe()
WORDS = Path(__file__).resolve().parent / "ocr-words.ps1"
FONT_DIR = Path(os.environ.get("WINDIR", "C:/Windows")) / "Fonts"
FONTS = {False: ("arial.ttf", "segoeui.ttf"), True: ("arialbd.ttf", "segoeuib.ttf")}


def ocr(frames_dir, out, scale=1.0):
    """The words OCR reads on each frame, with their boxes in the frame's own pixels (frames_dir holds the frames
    enlarged by scale, which helps OCR read small text)."""
    subprocess.run(["powershell.exe", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", str(WORDS),
                    "-Dir", str(frames_dir), "-Out", str(out)], check=True, stdin=subprocess.DEVNULL)
    words = collections.defaultdict(list)
    for line in open(out, encoding="utf-8-sig", errors="replace"):
        parts = line.rstrip("\n").split("|", 5)
        if len(parts) == 6:
            words[int(parts[0])].append((tuple(round(int(value) / scale) for value in parts[1:5]), parts[5]))
    return words


def pixelate(image, box):
    x, y, w, h = box
    x0, y0, x1, y1 = max(0, x - 4), max(0, y - 3), min(image.width, x + w + 4), min(image.height, y + h + 3)
    if x1 <= x0 or y1 <= y0:
        return
    region = image.crop((x0, y0, x1, y1))
    small = region.resize((max(1, (x1 - x0) // 8), max(1, (y1 - y0) // 8)), Image.BILINEAR)
    image.paste(small.resize(region.size, Image.NEAREST), (x0, y0))


def font_path(bold=False):
    return next(str(FONT_DIR / name) for name in FONTS[bold] if (FONT_DIR / name).exists())


def density(image, threshold=96):
    """The share of a text's ink box that is ink: higher for bold text."""
    gray = image.convert("L")
    box = gray.point(lambda v: 255 if v > threshold else 0).getbbox()
    if not box:
        return 0
    area = gray.crop(box)
    return sum(1 for v in area.tobytes() if v > threshold) / (area.width * area.height)


def replace(image, box, old, new):
    """Covers the words with the color around them and writes the new ones, at the old words' size and color."""
    x, y, w, h = box
    x0, y0, x1, y1 = max(0, x - 2), max(0, y - 2), min(image.width, x + w + 2), min(image.height, y + h + 2)
    pixels = image.load()
    ring = [pixels[i, y0] for i in range(x0, x1)] + [pixels[i, y1 - 1] for i in range(x0, x1)]
    ring += [pixels[x0, j] for j in range(y0, y1)] + [pixels[x1 - 1, j] for j in range(y0, y1)]
    background = tuple(sorted(channel)[len(channel) // 2] for channel in zip(*ring))
    distance = lambda c: sum((a - b) ** 2 for a, b in zip(c, background))
    inside = {(i, j): pixels[i, j] for i in range(x0, x1) for j in range(y0, y1)}
    strongest = max(map(distance, inside.values()))
    if strongest == 0:
        return
    marks = [(i, j) for (i, j), c in inside.items() if distance(c) > strongest * 0.35]
    far = sorted(inside.values(), key=distance)[-max(1, len(inside) // 12):]
    ink = tuple(sum(channel) // len(far) for channel in zip(*far))
    left, top = min(i for i, _ in marks), min(j for _, j in marks)
    height = max(j for _, j in marks) - top + 1
    if height < 8:
        return  # too small to be the words: OCR read them into a blur, or a page in transition

    # The size at which the old words are as tall as their ink, and bold when their strokes are heavier than regular.
    size = 6
    while size < 200 and (lambda b: b[3] - b[1])(ImageFont.truetype(font_path(), size + 1).getbbox(old)) <= height:
        size += 1
    original = Image.new("L", (x1 - x0, y1 - y0), 0)
    original.putdata([min(255, int(distance(pixels[i, j]) ** 0.5 * 255 / strongest ** 0.5)) for j in range(y0, y1) for i in range(x0, x1)])
    sample = Image.new("L", (x1 - x0 + 40, y1 - y0 + 20), 0)
    ImageDraw.Draw(sample).text((10, 5), old, font=ImageFont.truetype(font_path(), size), fill=255)
    font = ImageFont.truetype(font_path(density(original) > density(sample) * 1.2), size)
    draw = ImageDraw.Draw(image)
    draw.rectangle((x0, y0, x1 - 1, y1 - 1), fill=background)
    bbox = font.getbbox(old)
    draw.text((left - bbox[0], top - bbox[1]), new, font=font, fill=ink)


PUNCTUATION = ",.;:!?—)"


def key(text):
    """A word as the replacements name it: lowercase, without the punctuation that follows it ("Rivera," -> "rivera")."""
    return text.lower().rstrip(PUNCTUATION)


def phrases(found, replacements):
    """Joins words that follow each other on a line ("Jordan" "Rivera") into one phrase, and gives each phrase its new
    text: the replacement of the whole phrase ("Jordan Rivera=John Smith"), or of each of its words ("Jordan=John").
    A phrase with no replacement (like "Rivera" alone, when only "Jordan Rivera" is replaced) is left as it is."""
    found = sorted(found, key=lambda item: (item[0][1] // 8, item[0][0]))
    joined = []
    for box, text in found:
        if joined:
            (px, py, pw, ph), old, parts = joined[-1]
            x, y, w, h = box
            if abs(y - py) < ph / 2 and 0 <= x - (px + pw) < 1.5 * ph:
                right, bottom = max(px + pw, x + w), max(py + ph, y + h)
                joined[-1] = ((px, min(py, y), right - px, bottom - min(py, y)), old + " " + text, parts + [(box, text)])
                continue
        joined.append((box, text, [(box, text)]))
    result = []
    for box, old, parts in joined:
        trailing = old[len(old.rstrip(PUNCTUATION)):]
        whole = " ".join(key(text) for _, text in parts)
        if whole in replacements:
            new = replacements[whole] + trailing
        elif all(key(text) in replacements for _, text in parts):
            new = " ".join(replacements[key(text)] + text[len(text.rstrip(PUNCTUATION)):] for _, text in parts)
        else:
            continue
        words = [(word_box, text, replacements[key(text)] + text[len(text.rstrip(PUNCTUATION)):])
                 for word_box, text in parts if key(text) in replacements]
        result.append((box, old, new, words))
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("source")
    parser.add_argument("output")
    parser.add_argument("--pixelate", help="a regular expression, matched against each word (ignoring case)")
    parser.add_argument("--replace", action="append", default=[], help="WORDS=NEW, repeatable: a word or a phrase")
    parser.add_argument("--box", action="append", default=[], metavar="X,Y,W,H@FROM-TO",
                        help="pixelate this rectangle from FROM to TO seconds, for text OCR can't read; repeatable")
    parser.add_argument("--ocr-scale", type=float, default=1.0, help="enlarge the frames OCR reads, for small text (2 = twice)")
    args = parser.parse_args()

    bad = re.compile(args.pixelate, re.I) if args.pixelate else None
    replacements = {old.lower(): new for old, new in (item.split("=", 1) for item in args.replace)}
    vocabulary = {word for old in replacements for word in old.split()}

    probe = subprocess.run([FFMPEG, "-hide_banner", "-i", args.source], capture_output=True, text=True).stderr
    fps = float(re.search(r"([\d.]+) fps", probe).group(1))
    work = Path(tempfile.mkdtemp(prefix="redact-"))
    try:
        frames_dir = work / "f"
        frames_dir.mkdir()
        subprocess.run([FFMPEG, "-nostdin", "-loglevel", "error", "-i", args.source, str(frames_dir / "%06d.png")], check=True)
        if args.ocr_scale != 1:
            ocr_dir = work / "ocr"
            ocr_dir.mkdir()
            subprocess.run([FFMPEG, "-nostdin", "-loglevel", "error", "-i", args.source, "-vf",
                            f"scale=iw*{args.ocr_scale:g}:ih*{args.ocr_scale:g}:flags=lanczos", str(ocr_dir / "%06d.png")], check=True)
            words = ocr(ocr_dir, work / "words.txt", args.ocr_scale)
            shutil.rmtree(ocr_dir, ignore_errors=True)
        else:
            words = ocr(frames_dir, work / "words.txt")

        hide = collections.defaultdict(list)
        swap = collections.defaultdict(list)
        for frame, items in words.items():
            for box, text in items:
                # An exact word is replaced; anything else the pattern matches (two words OCR read as one) is pixelated.
                if key(text) in vocabulary:
                    swap[frame].append((box, text))
                elif bad and bad.search(text):
                    hide[frame].append(box)

        back, forward = int(fps * 2.5) + 1, int(fps * 0.5) + 1
        window = int(fps * 5)
        frames = sorted(int(name[:-4]) for name in os.listdir(frames_dir))

        # The phrases to replace on each frame OCR read them on, with how they looked, before any frame is changed.
        found = {}
        for frame, items in swap.items():
            image = Image.open(frames_dir / f"{frame:06d}.png").convert("RGB")
            crop = lambda box: image.crop((box[0], box[1], box[0] + box[2], box[1] + box[3]))
            found[frame] = [(box, old, new, crop(box), [(word_box, word, swapped, crop(word_box)) for word_box, word, swapped in words])
                            for box, old, new, words in phrases(items, replacements)]

        def matching_columns(image, box, crop):
            """How many columns, from the left, still look as they did: a menu can slide over the end of a word."""
            here = image.crop((box[0], box[1], box[0] + box[2], box[1] + box[3]))
            difference = ImageChops.difference(here, crop).convert("L")
            for column in range(box[2]):
                if sum(ImageStat.Stat(difference.crop((column, 0, column + 1, box[3]))).mean) >= 24:
                    return column
            return box[2]

        def same(image, box, crop):
            here = image.crop((box[0], box[1], box[0] + box[2], box[1] + box[3]))
            return sum(ImageStat.Stat(ImageChops.difference(here, crop)).mean) / 3 < 12

        fixed = []
        for item in args.box:
            rect, span = item.split("@")
            start, end = (float(value) for value in span.split("-"))
            fixed.append((tuple(int(value) for value in rect.split(",")), int(start * fps) + 1, int(end * fps) + 1))

        changed = 0
        for frame in frames:
            boxes = {box for other in range(frame - forward, frame + back + 1) for box in hide.get(other, [])}
            boxes |= {rect for rect, first, last in fixed if first <= frame <= last}
            # A phrase OCR missed on this frame is taken from the nearest frame that has it, within a few seconds, but
            # only where the same words are still on screen: the page may have moved on.
            candidates = {}
            for offset in range(window + 1):
                for other in (frame - offset, frame + offset):
                    for phrase in found.get(other, []):
                        slot = (round(phrase[0][0] / 6), round(phrase[0][1] / 6))
                        candidates.setdefault(slot, (other, phrase))
            if not boxes and not candidates:
                continue
            path = frames_dir / f"{frame:06d}.png"
            image = Image.open(path).convert("RGB")
            for source, (box, old, new, crop, words) in candidates.values():
                if source == frame or same(image, box, crop):
                    replace(image, box, old, new)
                else:
                    # Part of the phrase is covered now (a menu opened over it): replace the words still showing.
                    for word_box, word, swapped, word_crop in words:
                        if same(image, word_box, word_crop):
                            replace(image, word_box, word, swapped)
                            continue
                        visible = matching_columns(image, word_box, word_crop)
                        if visible >= word_box[2] * 0.4:
                            # Replace it, then put back what covers its end.
                            x, y, w, h = word_box
                            cover = image.crop((x + visible, y - 2, x + w + 2, y + h + 2))
                            replace(image, word_box, word, swapped)
                            image.paste(cover, (x + visible, y - 2))
            for box in boxes:
                pixelate(image, box)
            image.save(path)
            changed += 1

        subprocess.run([FFMPEG, "-nostdin", "-loglevel", "error", "-y", "-framerate", f"{fps:g}", "-i", str(frames_dir / "%06d.png"),
                        "-c:v", "libx264", "-preset", "medium", "-crf", "16", "-pix_fmt", "yuv420p", args.output], check=True)
        print(f"{Path(args.source).name}: {len(frames)} frames, {sum(map(len, hide.values()))} words pixelated, "
              f"{sum(map(len, swap.values()))} replaced, {changed} frames changed")
    finally:
        shutil.rmtree(work, ignore_errors=True)


if __name__ == "__main__":
    main()
