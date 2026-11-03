# The Orchard Core video specification

Every Orchard Core video follows these values. They are applied by `scripts/brand.py`, `scripts/narrate.py`,
`scripts/assemble.py` and `scripts/recorder/Scene.cs`; change them there, and here, never for a single video.

## Output

| Setting | Value | Why |
|---|---|---|
| Resolution | 1920x1080 | Full HD: sharp text on any screen. |
| Frame rate | 30 fps | Smooth cursor movement, small files. |
| Video codec | H.264 High, `yuv420p`, `-preset veryslow -crf 30 -tune stillimage`, `-x264-params ref=8:bframes=8:b-adapt=2:me=umh:subme=10:aq-mode=3`, keyframe every 30 s | Plays everywhere; screen content compresses well, and small text stays crisp at CRF 30. The slower search makes the file about 15% smaller than `-preset slow` at the same or better quality (SSIM against the master). |
| Audio codec | AAC, 48 kbps, 48 kHz, mono | Speech only: higher rates add about 2 MB per 10 minutes and nothing audible. |
| Container | MP4 with `-movflags +faststart` | Starts playing before it's fully downloaded. |
| Size | Under 25 MB for the encode. Videos are published on YouTube (upload `build/master.mp4` or the encode) and never committed to the Orchard Core repository or this one | A 9-minute overview is about 9 MB. |
| Master | `build/master.mp4`, CRF 16, AAC 192 kbps stereo | Kept in the workspace to encode again without rebuilding. |
| Captions | WebVTT (docs) and SRT, at most about 13 words each, split at sentence ends | Accessibility, and viewing without sound. |

## Layout

- **Background**: `#181B1A`, with blurred dark green glows in the top right and bottom left corners.
- **Header** (recordings and slides): the logo, 250 px wide, at x = 96, centered on y = 47; a thin divider; the
  chapter title in Open Sans SemiBold 34 px; on the right, a green pill with the chapter label ("CHAPTER 3") in Open
  Sans Bold 22 px, white.
- **Recordings**: scaled to 1728x972 (90%), centered horizontally, 14 px from the bottom, with a soft shadow and a
  3 px `#41B670` rounded border.
- **Slides**: a heading in Open Sans Bold 52 px at y = 150, underlined by a 100x6 px green bar; content from y = 240,
  within 96 px margins.
- **Code panels**: `#141716` with a window bar (red, yellow and green dots) and the file name; Consolas (or the
  closest monospace font), 22 to 28 px; the syntax colors of the Visual Studio Code dark theme; at most about 20
  lines per panel.
- **Cards**: the opening and closing cards show the full logo (900 px) over the video's title, a green bar, and the
  subtitle. Chapter cards show the symbol (150 px), "CHAPTER N" in green, the chapter's title and subtitle.

## Branding

- Logo: `branding/OrchardCoreLogo.png`, a copy of `src/docs/reference/branding/assets/logo/color/orchard-core-logo-color-high-resolution.png`
  in `OrchardCMS/OrchardCore`, with its name turned white for the dark background (by `scripts/brand.py`), and its
  green symbol unchanged. The symbol alone is the part left of the name.
- Fonts: Open Sans, from `branding/fonts` (a copy of `src/docs/reference/branding/assets/fonts`: Regular, SemiBold,
  Bold; Apache License 2.0 in `LICENSE.txt`).
- Colors (from the Orchard Core branding guidelines):

| Name | Hex | Use |
|---|---|---|
| Light green | `#41B670` | Accents, frame, pills, highlight rings, click ripples, captions in the page |
| Dark green | `#1D5B36` | Background glows |
| Black | `#404041` | |
| Gray | `#C6C6C6` | |
| White | `#FFFFFF` | Titles, logo name, text on green |

## Recording

- Chromium, headless, through Playwright for .NET, captured with the DevTools screencast at the full device resolution (1920x1080, JPEG quality 95), scaled down to the frame by ffmpeg with Lanczos.
- Viewport 1536x864 CSS pixels, device scale factor 1.25, light color scheme, `en-US` locale.
- A drawn cursor (white arrow with a dark outline), a green ripple on each click, green highlight rings with an
  optional green caption in white text, all injected in the page by `Scene.cs`.
- The cursor moves with ease-in-out over 25 steps; typing at 25 to 90 ms per character; each step holds until its
  narration ends, plus 0.45 s.
- The admin menu is collapsed in builder-like screens, so the subject has the whole width.
- A fresh site, provisioned with Auto Setup, with the Blog recipe unless the subject needs another, and realistic
  but fake data. Never record real people, accounts, credentials, or tokens: use an init script
  (`VIDEO_INIT_SCRIPT`) that replaces any leftover real names if a site was not made from scratch.
## Timing

| Element | Duration |
|---|---|
| Opening and closing cards | 1.0 s + narration + 1.4 s, fading in and out |
| Chapter cards | 2.6 s, no narration |
| Slides | 0.6 s lead on the first, 0.25 s on the others; 0.5 s between slides; 1.0 s tail on the last |
| Recordings | 0.7 s before the first step, 1.0 s after the last narration, 0.35 s fades |

## Voice

- `en-US-AndrewMultilingualNeural` at `+2%` rate, through `edge-tts` 7.3.1 or later: a neutral, clear voice, the
  same in every Orchard Core video.
- Pronunciation fixes in `SPOKEN` (`scripts/narrate.py`), such as "ASP.NET" spoken as "A.S.P. .NET". The captions
  keep the written form.
- Letters said one by one are written as an abbreviation with periods ("A.S.P.", "C.S.V."), so the voice says them
  at a natural pace, the way people say an initialism. Never separate the letters with commas ("A, S, P"): the voice
  pauses almost a second after each letter. Never with spaces only ("A S P"): the letters run together. A person
  confirms each new spelling with a sample from `scripts/pronounce.py` before it's used.
