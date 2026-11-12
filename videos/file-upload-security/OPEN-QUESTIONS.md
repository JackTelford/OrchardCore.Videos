# Open questions: File Upload Security

What was checked in the Orchard Core source, and what a person must confirm on a running site before the video is
recorded or published. The narration states only what the source shows; each item below says which sentence depends on it.

## 1. Which uploads the pipeline covers (narration `pipeline-3`, `own-3`)

Checked in the source:

- `DefaultMediaFileStore.CreateFileFromStreamAsync` (`src/OrchardCore/OrchardCore.Media.Core/DefaultMediaFileStore.cs:175`)
  does not call `FileCreationService`. The pipeline runs only in the extension method that takes the service
  (`src/OrchardCore/OrchardCore.Media.Abstractions/MediaFileStoreExtensions.cs:13`), which the Media upload endpoint
  (`UploadMediaEndpoint.cs:131`), the Media recipe step (`MediaStep.cs:87`) and the MetaWeblog handler
  (`MetaWeblogHandler.cs:198`) call, and in `ImportController.cs:80` and `ImportRemoteInstanceController.cs:87`.
- The completion of a resumable upload (the Media TUS Uploads feature) calls the plain overload
  (`src/OrchardCore.Modules/OrchardCore.Media/Startup.cs:712`), as does `SaveToMediaStep.cs:84` of the Data Pipelines
  module.

To confirm: with the ClamAV Antivirus Scanner and Media TUS Uploads both enabled, upload the test file from Media,
Library. The source says it is stored, and the narration says resumable uploads skip the pipeline. If the file is
rejected, `pipeline-3` and its slide must change. The recording keeps Media TUS Uploads disabled either way.

The narration of `down-1` and the closing card are worded for this: the scanner checks the files that arrive at the Media
upload API, and uploads that call `FileCreationService` are checked before storage. They do not say that every upload is
scanned.

## 2. Detection of the test file (clip `03-test`, narration `upload-3`, `upload-4`)

The default list of allowed extensions has `.csv` but not `.txt` or `.com`
(`MediaOptionsConfiguration.cs:12-51`), so the test file is saved as `eicar-test.csv`. To confirm:

- The Docker image detects it under that name (ClamAV may decide by the content, or by the type it detects).
- The exact message, which ends with the name of the signature (`ClamAvFileEventHandler.cs:152`). The narration says
  the message names the file and the signature, and does not say the signature.
- The machine's antivirus program does not remove the seed file before the take.

## 3. A stopped scanner (clip `03-down`, narration `down-1`, `down-2`)

The handler turns a refused connection, a timeout and an I/O failure into three different messages
(`ClamAvFileEventHandler.cs:66`, `:77`, `:88`), and an empty or unrecognised answer into a fourth: "returned an
unexpected response while scanning '...': " followed by the answer (`:156-157`). To confirm which one appears when the
container is stopped on the recording machine, and how long the upload takes to fail (the connection timeout is 5
seconds by default). The plan starts the site afresh before `03-down`, so the connection is new and a refused connection
("could not be reached") is the expected case; with a connection left over from an earlier scan, "failed" or the
unexpected-response message is more likely (item 7). The narration says only that the message names the ClamAV antivirus
scanner and the file, which holds for all four.

## 4. The container (recording plan, "The scanner")

The image name `clamav/clamav`, the tag `stable`, the published port, the time the signatures take to load and how to
see that the service is ready are not in the Orchard Core source or documentation. Confirm them, and correct the
plan, before the first take.

## 5. What the Media Library draws (clips `03-clean`, `03-test`, `03-down`)

Read from `UploadToast.vue`, `file.css` and `UppyFileUpload.ts`; not seen on a running site:

- The message line of an error is cut to one row until it is clicked (`file.css:1579-1594`). The plan clicks it.
- A "Validation error" warning is raised for the same file (`UppyFileUpload.ts:530-540`). It may overlap the panel.
- The panel shows a check mark for three seconds after a successful upload and then removes the file
  (`UploadToast.vue`).
- The file list shows the three images of the Blog recipe as well as the uploaded file.
- A rejected file is not listed. The source adds a placeholder for every chosen file to a store that holds the folders
  (`UppyFileUpload.ts:377-390`, "Add uploading placeholders"), and splices a file into the visible list only when the
  server accepted it (`:557-599`). The narration of `upload-5` says the test file is not in the library; confirm that
  the list does not show it, and that it does not appear after a page reload either.

## 6. The Features page (clip `03-enable`)

To confirm: the search box filters the list as the text is typed, the "Security" heading stays with the single
remaining row, the Enable link has a stable locator (read its id with `Recorder probe`), and the notification "The
feature ClamAV Antivirus Scanner was enabled." stays on screen for the length of `enable-2`.

## 7. The scanner connection is reused (clips `03-clean`, `03-test`, `03-down`; narration `upload-4`)

Checked in the source:

- `ClamAvConnectionFactory.cs:31-37` returns one `ClamAvConnection` per configuration for the whole process, and
  `ClamAvConnection.cs:78-83` reuses its socket when `TcpClient.Connected` is true.
- The command is a plain `nINSTREAM` (`ClamAvConnection.cs:10`), not inside an `IDSESSION`, and the answer is read up to
  the first line feed without reading to the end of the stream (`ClamAvConnection.cs:113-131`).
- The clamd protocol page of the ClamAV manual ("ClamdProtocol") describes a client reading replies until clamd closes the
  connection for commands outside a session, and describes several requests on one socket only for `IDSESSION`.

If clamd closes the connection after every reply, the next scan in the same process writes to a closed socket. It then
fails with "failed while scanning" (an `IOException`), or reads an empty answer and fails with "returned an unexpected
response". The connection is dropped after that failure, so the scan after it works again. If the test file were
the second scan of a process, it would fail without a detection, and the narration of `upload-4` would not be true on
screen. This cannot be settled from the source.

To confirm, with the check in the plan ("Check the scanner before the first take"): two uploads in a row on a site
that was started afresh. The plan already records every clip that scans after a fresh start (`03-clean`, `03-test` and `03-down`
are separate clips for this reason), which is correct whether or not clamd closes the connection. If the second upload
fails, report it to the Orchard Core project, and decide whether to publish the video before it is fixed: the video
shows only what happens to the first scan of a process.

## 8. Pronunciation (`storyboard.json`, `spoken`)

The `spoken` entries are written, not heard. A person confirms each with `scripts/pronounce.py` before narrating:
`IFileEventHandler`, `ITempDirectoryProvider`, `FileCreationService`, `FileCreatingResult`,
`CreateFileFromStreamAsync`, `CreatingAsync`, `CreatedAsync`, `CreateAsync`, `TryAddTransient`, `TryAddEnumerable`,
`ClamAV` ("Clam A.V.") and `clamd` ("clam D."). `TCP` ("T.C.P."), `AWS` ("A.W.S.") and `EFS` ("E.F.S.") are also in `spoken`. Also listen to "TUS" (in "Media TUS Uploads"),
"3310" and "OK" as the voice says them.
