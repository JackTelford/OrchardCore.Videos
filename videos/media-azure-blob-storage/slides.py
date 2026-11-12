"""The slides of the Azure Media Storage video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "IServiceCollection", "ServiceDescriptor", "IMediaFileProvider", "IMediaFileStoreCacheFileProvider",
    "BlobFileStore", "IMediaFileStore", "DefaultMediaFileStore", "IModularTenantEvents",
    "MediaBlobContainerTenantEvents", "ITusTempStore", "AzureBlobTusTempStore", "IResizedImageCache",
    "AzureBlobResizedImageCache", "Task", "Stream", "TimeSpan", "CancellationToken", "Modules", "StartupBase",
    "MediaAzureTusStartup",
}

PROBLEMS = [
    ("1", "The local disk is not permanent", "Containers and clean deployments can remove the files of a web server."),
    ("2", "Instances share media", "Several instances of a site need to read the same files."),
    ("3", "Blob Storage keeps the files", "They live in a container, outside the web server."),
]

FEATURES = [
    ("1", "Azure Media Storage", "OrchardCore.Media.Azure.Storage: the media files, in a blob container"),
    ("2", "Azure Media Image Cache", "OrchardCore.Media.Azure.ImageCache: resized copies of images"),
    ("3", "Azure Media Image Cache (Obsolete)", "OrchardCore.Media.Azure.ImageSharpImageCache: backwards compatibility"),
]

REQUEST = [
    ("1", "Request", "The browser asks the site for a media path, such as /media/photo.jpg."),
    ("2", "Local copy", "The Media Cache looks for a copy in a folder under the web root."),
    ("3", "Container", "With no copy, the site reads the blob and saves a copy."),
    ("4", "Response", "The site sends the file. Asset URLs do not name the storage account."),
]

CONFIGURATION = """
{
  "OrchardCore": {
    "Media": {
      "Azure": {
        // The connection string of your Azure Storage account.
        "ConnectionString": "",
        // A valid DNS name, in lowercase.
        "ContainerName": "somecontainer",
        // Optional: a subdirectory inside the container.
        "BasePath": "some/base/path",
        // Creates the container if it does not exist.
        "CreateContainer": true,
        // Deletes the container when the tenant is removed.
        "RemoveContainer": false
      }
    }
  }
}
"""

ENVIRONMENT = """
export OrchardCore__Media__Azure__ConnectionString="<storage account connection string>"
export OrchardCore__Media__Azure__ContainerName="my-media"
"""

DOCKER = """
docker run -p 10000:10000 <Azurite image> \\
  azurite-blob --blobHost 0.0.0.0
"""

EMULATOR = """
export OrchardCore__Media__Azure__ConnectionString="UseDevelopmentStorage=true"
"""

STARTUP = """
if (CheckOptions(connectionString, containerName, _logger))
{
    // ...
    // Files are served from a local cache folder.
    services.Replace(ServiceDescriptor.Singleton<IMediaFileProvider>(serviceProvider =>
        serviceProvider.GetRequiredService<IMediaFileStoreCacheFileProvider>()));
    // The blob file store reads and writes the container.
    services.AddSingleton(serviceProvider =>
    {
        // ...
        return new BlobFileStore(blobStorageOptions, clock, contentTypeProvider, blobLogger);
    });
    // The media file store wraps it.
    services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
    {
        // ...
        return new DefaultMediaFileStore(
            fileStore, mediaUrlBase, mediaOptions.CdnBaseUrl, mediaEventHandlers,
            mediaCreatingEventHandlers, fileSizeHelper, logger);
    }));
    // ...
}
"""

TUS = """
[Feature("OrchardCore.Media.Azure.Storage")]
[RequireFeatures("OrchardCore.Media.Tus")]
public sealed class MediaAzureTusStartup : Modules.StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // ...
        if (!string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(containerName))
            services.Replace(ServiceDescriptor.Singleton<ITusTempStore, AzureBlobTusTempStore>());
    }
}
"""

IMAGE_CACHE_CONFIGURATION = """
{
  "OrchardCore": {
    "Media": {
      "Azure": {
        "ImageCache": {
          "ConnectionString": "",
          "ContainerName": "somecontainer",
          "BasePath": "some/base/path",
          "CreateContainer": true,
          "RemoveContainer": false
        }
      }
    }
  }
}
"""

RESIZED_IMAGE_CACHE = """
public interface IResizedImageCache
{
    Task<(Stream Content, string ContentType)?> GetAsync(
        string cacheKey, string fileExtension,
        CancellationToken cancellationToken = default);

    Task SetAsync(
        string cacheKey, Stream image, string contentType,
        TimeSpan maxAge, CancellationToken cancellationToken = default);

    // ClearAsync and ClearStaleAsync ...
}
"""

IMAGE_CACHE_REGISTRATION = """
// MediaAzureImageCacheStartup
services.Replace(ServiceDescriptor.Singleton<IResizedImageCache, AzureBlobResizedImageCache>());
"""

CONTAINER_PER_TENANT = """
{
  "ConnectionString": "",
  "ContainerName": "{{ ShellSettings.Name }}-media",
  "BasePath": "Media",
  "CreateContainer": true
}
"""

BASE_PATH_PER_TENANT = """
{
  "ConnectionString": "",
  "ContainerName": "somecontainer",
  "BasePath": "{{ ShellSettings.Name }}/Media",
  "CreateContainer": true
}
"""

REMOVAL = """
{
  // Deletes the container.
  "RemoveContainer": false,
  // Deletes only the files under BasePath.
  // Ignored when RemoveContainer is true.
  "RemoveFilesFromBasePath": false
}
"""

NAMESPACES = [
    ("1", "Gen1: flat namespace", "A folder exists as a marker file inside it, which listings hide."),
    ("2", "Gen2: hierarchical namespace", "Real directories, and atomic moves through the Data Lake API."),
    ("3", "Detected at startup", "The store asks the storage account which type it is."),
]

HIERARCHICAL_NAMESPACE = """
{
  // true for a Gen2 account, false for a Gen1 account.
  "UseHierarchicalNamespace": false
}
"""

# The panel of a code and notes slide: the code on the left, the notes on the right.
LEFT = (96, 240, 1146, 1030)
NOTES = (1186, 270, W - 96, 1030)
LEFT_NARROW = (96, 240, 1000, 1030)
NOTES_WIDE = (1040, 270, W - 96, 1030)


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        index = int(step_id[-1])
        if index == 1:
            image = slide_base(chapter_title, label, "Why Azure Blob Storage")
            bullets(image, PROBLEMS)
        elif index == 2:
            image = slide_base(chapter_title, label, "The features of the Azure Media module")
            bullets(image, FEATURES)
        else:
            image = slide_base(chapter_title, label, "How a request is served")
            bullets(image, REQUEST)
        return image

    if step_id in ("conf-1", "conf-2"):
        image = slide_base(chapter_title, label, "Connect a storage account")
        # Lines 2 to 4 are the section path, 5 to 8 the two required settings, 9 to 12 the two optional ones.
        highlight = {2, 3, 4, 5, 6, 7, 8} if step_id == "conf-1" else {9, 10, 11, 12}
        draw_code(image, LEFT, "appsettings.json", "json", CONFIGURATION, highlight=highlight, size=26)
        if step_id == "conf-1":
            notes = [("Required", "Connection string"), ("Required", "Container name")]
        else:
            notes = [("Optional", "Base Path"), ("Optional", "Create Container")]
        note_cards(image, notes, NOTES)
        return image

    if step_id == "conf-3":
        image = slide_base(chapter_title, label, "Keep the connection string out of the file")
        draw_code(image, (96, 240, W - 96, 430), "Environment variables", "bash", ENVIRONMENT, size=26)
        note_cards(image, [
            ("Overrides appsettings.json", "Environment variables are read after appsettings.json and the user secrets."),
            ("Two underscores", "Each double underscore is one level of the setting's path."),
        ], (96, 480, W - 96, 1030))
        return image

    if step_id == "conf-4":
        image = slide_base(chapter_title, label, "Try it with the Azurite emulator")
        draw_code(image, (96, 240, W - 96, 420), "Terminal", "bash", DOCKER, size=26)
        draw_code(image, (96, 460, W - 96, 600), "Environment variable", "bash", EMULATOR, size=26)
        note_cards(image, [
            ("Azurite image", "The Azurite documentation names the image to run."),
            ("Development storage", "The connection string of the local emulator."),
        ], (96, 630, W - 96, 1030))
        return image

    if step_id == "svc-1":
        image = slide_base(chapter_title, label, "What the feature registers")
        draw_code(image, (96, 240, W - 96, 1040), "Startup.cs", "csharp", STARTUP, types=TYPES, size=22)
        return image

    if step_id == "svc-2":
        image = slide_base(chapter_title, label, "Resumable uploads")
        draw_code(image, (96, 240, W - 96, 720), "Startup.cs", "csharp", TUS, types=TYPES, size=24)
        note_cards(image, [
            ("Needs Media TUS Uploads", "Partial uploads are staged in the container of the media."),
        ], (96, 780, W - 96, 1030))
        return image

    if step_id == "cache-1":
        image = slide_base(chapter_title, label, "Azure Media Image Cache settings")
        draw_code(image, LEFT_NARROW, "appsettings.json", "json", IMAGE_CACHE_CONFIGURATION, size=26)
        note_cards(image, [
            ("Its own settings", "Connection string, container name, base path."),
            ("Its own container", "Or a base path in a shared one."),
        ], NOTES_WIDE)
        return image

    if step_id == "cache-2":
        image = slide_base(chapter_title, label, "The same interface, stored as blobs")
        draw_code(image, (96, 240, W - 96, 720), "IResizedImageCache.cs", "csharp", RESIZED_IMAGE_CACHE,
                  types=TYPES, size=22)
        draw_code(image, (96, 770, W - 96, 960), "Startup.cs", "csharp", IMAGE_CACHE_REGISTRATION, types=TYPES, size=24)
        return image

    if step_id == "tenant-1":
        image = slide_base(chapter_title, label, "One container per tenant, or one folder")
        draw_code(image, (96, 240, 940, 640), "Container per tenant", "json", CONTAINER_PER_TENANT, size=24)
        draw_code(image, (980, 240, W - 96, 640), "Folder per tenant", "json", BASE_PATH_PER_TENANT, size=24)
        note_cards(image, [
            ("Only the default Liquid filters and tags", "Extra filters, such as slugify, are not available."),
        ], (96, 700, W - 96, 1030))
        return image

    if step_id == "tenant-2":
        image = slide_base(chapter_title, label, "When a tenant is removed")
        draw_code(image, LEFT, "appsettings.json", "json", REMOVAL, size=26)
        note_cards(image, [
            ("Both false by default", "The container is left alone."),
            ("Image cache", "Only RemoveContainer applies."),
        ], NOTES)
        return image

    if step_id == "tenant-3":
        image = slide_base(chapter_title, label, "Gen1 and Gen2 storage accounts")
        bullets(image, NAMESPACES)
        return image

    if step_id == "tenant-4":
        image = slide_base(chapter_title, label, "Choose the account type yourself")
        draw_code(image, (96, 240, 1146, 560), "appsettings.json", "json", HIERARCHICAL_NAMESPACE, size=26)
        note_cards(image, [
            ("Detection can fail", "It needs account-level permissions."),
            ("Media Library", "Warns when the account is Gen2."),
        ], NOTES)
        return image

    raise SystemExit(f"No slide for {step_id}")
