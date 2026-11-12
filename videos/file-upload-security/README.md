# File Upload Security: Antivirus Scanning and Temporary Files

**YouTube:** not published yet · **Length:** 5:21 · **Feature:** `OrchardCore.Antivirus.ClamAV` ([Antivirus documentation](https://docs.orchardcore.net/en/latest/reference/modules/Antivirus/))

This video is for module and theme authors. It shows the pipeline that runs on an uploaded file before Orchard Core
stores it (`FileCreationService` and `IFileEventHandler`), the ClamAV Antivirus Scanner that plugs into it, an upload
that passes, one that the scanner rejects and one while the scanner is stopped, and how to use the pipeline and the tenant-scoped temporary file
storage (`ITempDirectoryProvider`) in your own module. The title, description, tags and chapters YouTube shows are in
`video.json`. The documentation pages it backs are
[File Upload Security](https://docs.orchardcore.net/en/latest/reference/core/file-upload-security/) and
[Temporary File Storage](https://docs.orchardcore.net/en/latest/reference/core/temporary-file-storage/).

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. Only chapter 3 is recorded
in a browser (`03-enable`, `03-clean`, `03-test`, `03-down`); the other chapters are slides drawn by `slides.py`.
`recording-plan.md` has every action, the seed files and the checks for private data; this is the summary.

1. Build `src/OrchardCore.Cms.Web` of OrchardCore and run it with `ORCHARD_APP_DATA` pointing at an empty folder
   outside both repositories, on `http://localhost:5010`, provisioned by Auto Setup (the Blog recipe, site name
   **Orchard Core**, time zone `America/Los_Angeles`, user `admin`). Set the ClamAV connection in the environment
   before the site starts: `OrchardCore__Antivirus__ClamAV__Host=localhost` and
   `OrchardCore__Antivirus__ClamAV__Port=3310`.
2. Start a `clamd` service in Docker, published on port 3310, and wait until it has loaded its signatures. Keep the
   **Media TUS Uploads** feature disabled.
3. Create the seed files outside both repositories (two small CSV files and the standard antivirus test file saved
   with a `.csv` name), then sign the recorder in:

   ```bash
   cd .agents/skills/orchardcore-video-producer/scripts/recorder
   dotnet build -c Release -p:VideoScenes=<repo>/videos/file-upload-security/recorder
   export VIDEO_ROOT=<repo>/videos/file-upload-security VIDEO_BASE_URL=http://localhost:5010 VIDEO_USER_NAME=admin VIDEO_PASSWORD=<password>
   <bin>/Recorder setup
   ```

   Stop the site and copy its `App_Data` aside after setup, after `03-enable` and after `03-clean`: restore the
   matching copy before a new take. Start the site afresh before each of `03-clean`, `03-test` and `03-down`, so that
   each of them makes the first scan of the process.
4. Narrate with `scripts/narrate.py videos/file-upload-security`, then record one clip at a time, in this order:
   `Recorder record 03-enable`, `Recorder record 03-clean`, `Recorder record 03-test`, `Recorder record 03-down`, with
   the site stopped and started again between them as described above. `03-clean` and `03-test` need the feature
   enabled and the scanner running; `03-test` needs the file of `03-clean` in the library. `03-down` needs the scanner
   stopped (`docker stop clamav`, outside the capture) and the same file in the library.
