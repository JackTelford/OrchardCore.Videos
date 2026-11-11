# Questions about the specification

Questions about the skill's specification (`.agents/skills/orchardcore-video-producer/`) raised by the video
programme. The programme carries on with the answer given under each question until a maintainer decides otherwise;
the skill itself is not changed by the programme.

## 1. Narration length of the opening and closing cards

The programme limits every step to 8 to 45 words. The opening card of the worked example
(`examples/secrets-module/storyboard.json`, step `intro`) has 53 words, and two of its steps have 6.

**For now:** cards follow the same 8 to 45 words as every step, so an opening card says what the subject is and what
the video covers in two or three short sentences.

## 2. "now" in the worked example

The example says "SMTP now reads its password" (`input-6`) and "you can now choose its store" (`keyvault-3`). The
programme bans "now" in narration, as `references/storyboard.md` asks for timeless narration.

**For now:** "now" is banned in every narration; `tools/validate_content.py` fails on it.

## 3. Length of a video

`references/storyboard.md` says a module overview is "between 4 and 9 minutes". The programme asks for 3 to 8 minutes,
and splits anything longer into several videos.

**For now:** 3 to 8 minutes at 150 words per minute (450 to 1,200 words), checked by `validate_content.py`.

## 4. "JSON" in admin videos

Admin videos must not show code, and the programme's code markers include `json`. Some screens an administrator uses
are labeled with the word: **Tools > Deployments > JSON Import**, and the **View Or Download Content As JSON** feature.

**For now:** the label "JSON" (upper case) is allowed in admin narration and slides, as a name on screen;
`validate_content.py` fails on lower-case `json` (file names such as `recipe.json`, and code).

## 5. Files the programme adds to a video's folder

`SKILL.md` ("What a video folder keeps") doesn't list `recording-plan.md` or `OPEN-QUESTIONS.md`, which the
programme adds so a different session can record a video without deciding anything.

**For now:** both are kept in the folder. Should the skill list them, and should the template include a
`recording-plan.md` skeleton?

## 6. The README template's pull request link

`templates/video/README.md` starts with "**Feature:** `<id>` ([OrchardCore#NNNN](...))". Most videos of the programme
show features that exist on `main`, with no pull request of their own.

**For now:** the line names the feature id and links to its documentation page instead of a pull request, and
`video.json` has `"pullRequest": null`.

## 7. One playlist per series

`video.json` `playlists` holds the video's series, and the publish workflow creates a missing playlist. The plan has
40 series (`tracker/series.json`) in 22 learning paths.

**For now:** each video lists `"Orchard Core"` and its series. Should the channel have a playlist per series, per
learning path, or both?
