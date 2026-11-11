<!-- A video's folder, videos/<slug>/. See CONTRIBUTING.md. Never attach or commit the MP4 itself. -->

**Video:** `videos/<slug>/`
**Watch it:** <!-- a link to the MP4: a shared drive, or a draft release -->
**Docs pages that embed it:** <!-- the OrchardCMS/OrchardCore pull request, or the pages with a video:<slug> marker -->

- [ ] `videos/<slug>/` only, with no MP4 or audio file
- [ ] Recorded on fake data, and checked for private data
- [ ] `video.json`: title of 100 characters or fewer, description, chapters (each 10 seconds or more), links, `embeddedIn`
- [ ] `thumbnail.jpg`, and captions for a narrated video
- [ ] `python tools/youtube/youtube.py description <slug>` runs without an error
- [ ] `python tools/youtube/youtube.py index` updated the table in `README.md`
- [ ] `python tools/tracker.py set <slug> ...` and `python tools/tracker.py report`, committed with the video
