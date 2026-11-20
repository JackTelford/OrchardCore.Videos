## Producing videos

Use the `orchardcore-video-producer` skill in `.agents/skills/orchardcore-video-producer`. Each video is a folder in
`videos/<slug>/`; never commit an MP4, an audio file, a sign-in (`state.json`), a mask script with real names, a
YouTube client secret or token, or a copy of a site's `App_Data`.

## Publishing videos

Videos go to YouTube **unlisted** by default: `privacyStatus` is `unlisted` in every `video.json` and in the template,
and `youtube.py` uses `unlisted` when it's missing. The playlists it creates are unlisted too. Make a video `public`
only when a maintainer asks for it, by changing its `video.json` (a published video's privacy is then changed in
YouTube Studio). Publish at most about 18 videos a day (the YouTube API quota), through a release `<slug>/v<N>` of
OrchardCMS/OrchardCore.Videos, after the video's folder is merged to `main`.
