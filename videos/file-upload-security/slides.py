"""The slides of the File Upload Security video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "FileCreationService", "FileCreatingContext", "FileCreatingResult", "FileStoreException", "IFileEventHandler",
    "IFileStore", "IFileStoreEntry", "IFormFile", "ITempDirectoryProvider", "IMediaFileStore", "ResultError",
    "LocalizedString", "CustomUploadService", "RejectExecutableFileEventHandler", "MyImportService",
    "ClamAvFileEventHandler", "Stream", "Task", "CancellationToken", "FileStream", "StartupBase", "Startup",
    "IServiceCollection", "ServiceDescriptor", "StringComparison", "FileMode", "FileAccess", "FileShare",
    "FileOptions", "Path", "File", "Directory", "ZipFile",
}

# The pipeline, step by step.
PIPELINE = [
    ("1", "CreateAsync", "FileCreationService passes the stream to every IFileEventHandler."),
    ("2", "CreatingAsync", "A handler returns Success to continue, or Failed to stop."),
    ("3", "Store", "Only a file that passed is written to the file store."),
    ("4", "CreatedAsync", "After storage, every handler is told that the file exists."),
]

# Where the pipeline runs.
CALLERS = [
    ("check", "Media upload API", "api/media/Upload, which the media library uses."),
    ("check", "Media recipe step", "Each file of a recipe is checked before it is stored."),
    ("check", "Package Import", "Deployment packages, uploaded locally or sent by a remote instance."),
    ("!", "Skipped", "Media TUS Uploads, and direct calls to the media store."),
]

# What the ClamAV handler decides.
HANDLER = [
    ("check", "Clean", "clamd answers OK, and the upload continues."),
    ("x", "Detected", "The result fails, and its message names the file and the signature."),
    ("!", "Scanner problem", "An exception stops the upload: no host, no connection, a timeout or a bad answer."),
]

CLAMAV_JSON = """
{
  "OrchardCore": {
    "Antivirus": {
      "ClamAV": {
        "Host": "localhost",
        "Port": 3310,
        "ConnectTimeoutSeconds": 5,
        "TransferTimeoutSeconds": 30
      }
    }
  }
}
"""

CLAMAV_ENVIRONMENT = """
OrchardCore__Antivirus__ClamAV__Host=localhost
OrchardCore__Antivirus__ClamAV__Port=3310
OrchardCore__Antivirus__ClamAV__ConnectTimeoutSeconds=5
OrchardCore__Antivirus__ClamAV__TransferTimeoutSeconds=30
"""

CLAMAV_REWIND = """
if (scanStream.CanSeek)
{
    scanStream.Position = 0;
}
else
{
    scanStream = await CreateSeekableStreamAsync(scanStream, cancellationToken);
}
// ...
scanStream.Position = 0;
"""

CLAMAV_TEMP = """
var tempFilePath = _tempDirectoryProvider.GetTempFileName();
var tempStream = new FileStream(
    tempFilePath,
    FileMode.CreateNew,
    FileAccess.ReadWrite,
    FileShare.None,
    81920,
    FileOptions.Asynchronous | FileOptions.DeleteOnClose);
"""

UPLOAD_SERVICE = """
public async Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken)
{
    await using var uploadedStream = file.OpenReadStream();
    await using var fileCreatingResult = await _fileCreationService.CreateAsync(
        new FileCreatingContext(file.FileName, file.Length, file.ContentType),
        uploadedStream,
        cancellationToken);

    if (!fileCreatingResult.Succeeded)
    {
        throw new FileStoreException(
            fileCreatingResult.ErrorMessage ?? "The uploaded file was rejected.");
    }

    var path = await _fileStore.CreateFileFromStreamAsync(
        file.FileName, fileCreatingResult.Stream);
    var fileInfo = await _fileStore.GetFileInfoAsync(path);

    await _fileCreationService.CreatedAsync(fileInfo, cancellationToken);

    return path;
}
"""

REGISTER_SERVICE = """
public override void ConfigureServices(IServiceCollection services)
{
    services.TryAddTransient<FileCreationService>();
}
"""

MEDIA_STORE = """
// Runs the pipeline, then stores the file:
await _mediaFileStore.CreateFileFromStreamAsync(
    _fileCreationService, file.TargetPath, stream, overwrite: true);

// Does not run the pipeline:
await _mediaFileStore.CreateFileFromStreamAsync(
    file.TargetPath, stream, overwrite: true);
"""

HANDLER_CODE = """
public sealed class RejectExecutableFileEventHandler : IFileEventHandler
{
    public Task<FileCreatingResult> CreatingAsync(
        FileCreatingContext context, Stream stream, CancellationToken cancellationToken = default)
    {
        if (string.Equals(Path.GetExtension(context.FileName), ".exe", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(FileCreatingResult.Failed(stream, new ResultError
            {
                Message = new LocalizedString(nameof(RejectExecutableFileEventHandler), "Executable files are not allowed."),
            }));
        }

        return Task.FromResult(FileCreatingResult.Success(stream));
    }

    // CreatedAsync(...) ...
}
"""

REGISTER_HANDLER = """
public override void ConfigureServices(IServiceCollection services)
{
    services.TryAddEnumerable(
        ServiceDescriptor.Singleton<
            IFileEventHandler,
            RejectExecutableFileEventHandler>());
}
"""

TEMP_CONSUMERS = [
    ("1", "Chunked uploads", "The ChunkedFileUploads folder of the tenant."),
    ("2", "Resumable uploads", "The TusUploads folder of the tenant, with the default disk store."),
    ("3", "Deployment imports", "The uploaded package and the folder it is extracted to."),
    ("4", "Tenants API", "The recipe sent with a tenant setup request."),
]

TEMP_JSON = """
{
  "OrchardCore": {
    "TempDirectory": {
      "Path": "/mnt/shared/temp"
    }
  }
}
"""

TEMP_SERVICE = """
public async Task ImportAsync(IFormFile package, CancellationToken cancellationToken)
{
    var archivePath = _tempDirectoryProvider.GetTempFileName(
        Path.GetExtension(package.FileName));
    var extractPath = _tempDirectoryProvider.CreateTempSubdirectory();

    try
    {
        // ...
    }
    finally
    {
        File.Delete(archivePath);
        Directory.Delete(extractPath, recursive: true);
    }
}
"""


def slide(step_id, chapter_title, label):
    if step_id in ("pipeline-1", "pipeline-2"):
        image = slide_base(chapter_title, label, "Before the file is stored")
        bullets(image, PIPELINE, active={0, 1} if step_id == "pipeline-1" else {2, 3})
        return image

    if step_id == "pipeline-3":
        image = slide_base(chapter_title, label, "Where the pipeline runs")
        bullets(image, CALLERS)
        return image

    if step_id == "clamav-1":
        image = slide_base(chapter_title, label, "What the ClamAV handler decides")
        bullets(image, HANDLER)
        return image

    if step_id == "clamav-2":
        image = slide_base(chapter_title, label, "Configure the connection")
        draw_code(image, (96, 240, 960, 760), "appsettings.json", "json", CLAMAV_JSON, types=TYPES, size=24)
        draw_code(image, (96, 790, 960, 1040), "Environment variables", "bash", CLAMAV_ENVIRONMENT, types=TYPES, size=22)
        note_cards(image, [
            ("Host", "Required. It has no default."),
            ("Port", "3310 by default."),
            ("Timeouts", "5 seconds to connect, 30 to transfer."),
        ], (1000, 280, W - 96, 1030))
        return image

    if step_id == "clamav-3":
        image = slide_base(chapter_title, label, "Rewind the stream, copy it if needed")
        draw_code(image, (96, 240, W - 96, 650), "ClamAvFileEventHandler.cs", "csharp", CLAMAV_REWIND, types=TYPES, size=22)
        draw_code(image, (96, 685, W - 96, 1040), "ClamAvFileEventHandler.cs", "csharp", CLAMAV_TEMP, types=TYPES, size=22)
        return image

    if step_id == "own-1":
        image = slide_base(chapter_title, label, "1. Run the pipeline before the write")
        draw_code(image, (96, 240, W - 96, 1040), "CustomUploadService.cs", "csharp", UPLOAD_SERVICE, types=TYPES,
                  highlight={1, 2, 3, 4, 5, 6, 7}, size=22)
        return image

    if step_id == "own-2":
        image = slide_base(chapter_title, label, "2. Check the result, then store")
        draw_code(image, (96, 240, W - 96, 1040), "CustomUploadService.cs", "csharp", UPLOAD_SERVICE, types=TYPES,
                  highlight={1, 9, 10, 11, 12, 13, 15, 16, 17, 19, 21}, size=22)
        return image

    if step_id == "own-3":
        image = slide_base(chapter_title, label, "3. Register the service, use the right overload")
        draw_code(image, (96, 240, W - 96, 520), "Startup.cs", "csharp", REGISTER_SERVICE, types=TYPES, size=26)
        draw_code(image, (96, 560, W - 96, 1030), "IMediaFileStore calls", "csharp", MEDIA_STORE, types=TYPES, size=26)
        return image

    if step_id == "own-4":
        image = slide_base(chapter_title, label, "4. Implement IFileEventHandler")
        draw_code(image, (96, 240, W - 96, 1030), "RejectExecutableFileEventHandler.cs", "csharp", HANDLER_CODE, types=TYPES, size=22)
        return image

    if step_id == "own-5":
        image = slide_base(chapter_title, label, "5. Register the handler")
        draw_code(image, (96, 240, 1100, 640), "Startup.cs", "csharp", REGISTER_HANDLER, types=TYPES, size=26)
        note_cards(image, [
            ("Failed", "Stops the upload with a message."),
            ("Success", "Continues with the stream."),
            ("Exception", "Stops it too, as ClamAV does."),
        ], (1140, 240, W - 96, 1030))
        return image

    if step_id == "temp-1":
        image = slide_base(chapter_title, label, "Who asks for a temporary path")
        bullets(image, TEMP_CONSUMERS)
        return image

    if step_id == "temp-2":
        image = slide_base(chapter_title, label, "Where the files go")
        draw_code(image, (96, 240, 900, 640), "appsettings.json", "json", TEMP_JSON, types=TYPES, size=26)
        note_cards(image, [
            ("Default", "OrchardCore/Temp in LocalApplicationData."),
            ("Per tenant", "{Path}/{TenantName}/..."),
            ("Unix-like systems", "Tenant folders are created owner-only: 700."),
        ], (940, 240, W - 96, 1030))
        return image

    if step_id == "temp-3":
        image = slide_base(chapter_title, label, "Use a mounted file share")
        note_cards(image, [
            ("Works", "A mounted file share: Azure Files (SMB), AWS EFS or FSx (NFS)."),
            ("Does not work", "Object storage: Azure Blob or Amazon S3."),
            ("Why", "Consumers seek in files and extract archives to a folder."),
        ], (96, 260, W - 96, 1030))
        return image

    if step_id == "temp-4":
        image = slide_base(chapter_title, label, "Inject ITempDirectoryProvider")
        draw_code(image, (96, 240, 1180, 1030), "MyImportService.cs", "csharp", TEMP_SERVICE, types=TYPES, size=22)
        note_cards(image, [
            ("GetRootDirectory", "The tenant root folder."),
            ("GetTempFileName", "A unique file path, not created."),
            ("CreateTempSubdirectory", "A created sub-folder, uniquely named."),
        ], (1220, 240, W - 96, 1030))
        return image

    raise SystemExit(f"No slide for {step_id}")
