"""The slides of the Amazon S3 Media Storage video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "IServiceCollection", "ServiceDescriptor", "IMediaFileProvider", "IMediaFileStoreCacheFileProvider",
    "IAmazonS3", "AwsFileStore", "IMediaFileStore", "DefaultMediaFileStore", "IModularTenantEvents",
    "MediaS3BucketTenantEvents", "ITusTempStore", "S3TusTempStore", "AwsStorageOptions", "IResizedImageCache",
    "AWSS3ResizedImageCache", "Task", "Stream", "TimeSpan", "CancellationToken", "Modules", "StartupBase",
    "MediaAmazonS3TusStartup", "MediaAmazonS3ImageCacheStartup",
}

PROBLEMS = [
    ("1", "The local disk is not permanent", "Containers and clean deployments can remove the files of a web server."),
    ("2", "Instances share media", "Several instances of a site need to read the same files."),
    ("3", "An S3 bucket keeps the files", "They live in a bucket, outside the web server."),
]

FEATURES = [
    ("1", "Amazon Media Storage", "OrchardCore.Media.AmazonS3: the media files, in a bucket"),
    ("2", "Amazon Media Image Cache", "OrchardCore.Media.AmazonS3.ImageCache: resized copies of images"),
    ("3", "Amazon Media Image Cache (Obsolete)", "OrchardCore.Media.AmazonS3.ImageSharpImageCache: backwards compatibility"),
]

REQUEST = [
    ("1", "Request", "The browser asks the site for a media path, such as /media/photo.jpg."),
    ("2", "Local copy", "The Media Cache looks for a copy in a folder under the web root."),
    ("3", "Bucket", "With no copy, the site reads the object and saves a copy."),
    ("4", "Response", "The site sends the file. Asset URLs point to the site."),
]

CREDENTIALS = [
    ("1", "Credentials section", "AccessKey and SecretKey, in the configuration."),
    ("2", "Profiles", "Profile and ProfilesLocation, for a profile of the AWS CLI."),
    ("3", "Environment variables", "The AWS access key ID and secret access key variables."),
    ("4", "The role of the host", "An ECS task or an EC2 instance, when hosted in AWS."),
]

CONFIGURATION = """
{
  "OrchardCore": {
    "Media": {
      "AmazonS3": {
        // Required: the name of the bucket.
        "BucketName": "oc-media",
        // Required: a region, or a ServiceURL.
        "Region": "eu-central-1",
        // Optional: a folder inside the bucket.
        "BasePath": "/media",
        // Optional: creates the bucket if it does not exist.
        "CreateBucket": false
      }
    }
  }
}
"""

ENVIRONMENT = """
export OrchardCore__Media__AmazonS3__BucketName="oc-media"
export OrchardCore__Media__AmazonS3__Region="eu-central-1"
export OrchardCore__Media__AmazonS3__Credentials__AccessKey="<access key>"
export OrchardCore__Media__AmazonS3__Credentials__SecretKey="<secret key>"
"""

DOCKER = """
docker run -p 9444:9090 -t adobe/s3mock:latest
"""

EMULATOR = """
{
  "OrchardCore": {
    "Media": {
      "AmazonS3": {
        "ServiceURL": "http://localhost:9444/",
        "ForcePathStyle": true,
        "Credentials": {
          "SecretKey": "dummy",
          "AccessKey": "dummy"
        },
        "CreateBucket": true,
        "BucketName": "oc-media"
      }
    }
  }
}
"""

STARTUP = """
// Startup.ConfigureServices, when validationErrors is empty.
// With invalid settings, an error is logged and none of this is registered.
// ...
services.Replace(ServiceDescriptor.Singleton<IMediaFileProvider>(serviceProvider =>
    serviceProvider.GetRequiredService<IMediaFileStoreCacheFileProvider>()));
// ...
services.AddAWSService<IAmazonS3>(storeOptions.AwsOptions);

services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
{
    // ...
    var fileStore = new AwsFileStore(clock, options.Value, amazonS3Client);
    // ...
    return new DefaultMediaFileStore(
        fileStore, mediaUrlBase, mediaOptions.CdnBaseUrl, mediaEventHandlers,
        mediaCreatingEventHandlers, fileSizeHelper, logger);
}));
// ...
services.AddScoped<IModularTenantEvents, MediaS3BucketTenantEvents>();
"""

TUS = """
[RequireFeatures("OrchardCore.Media.Tus")]
public sealed class MediaAmazonS3TusStartup : Modules.StartupBase
{
    // ...
    public override void ConfigureServices(IServiceCollection services)
    {
        // Only replace the default local store if the media store is active.
        if (_configuration.TryBindConfiguration<AwsStorageOptions>(
                AmazonS3Constants.ConfigSections.AmazonS3,
                AmazonS3Constants.ConfigSections.LegacyAmazonS3,
                _logger, out var storeOptions) &&
            !storeOptions.Validate().Any())
        {
            // Partial uploads are staged in the media bucket.
            services.Replace(ServiceDescriptor.Singleton<ITusTempStore, S3TusTempStore>());
        }
    }
}
"""

IMAGE_CACHE_CONFIGURATION = """
{
  "OrchardCore": {
    "Media": {
      "AmazonS3": {
        "Region": "eu-central-1",
        "ImageCache": {
          "BucketName": "oc-imagecache",
          "CreateBucket": true,
          "RemoveBucket": false
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
// MediaAmazonS3ImageCacheStartup
services.TryAddAWSService<IAmazonS3>(storeOptions.AwsOptions);
services.Replace(ServiceDescriptor.Singleton<IResizedImageCache, AWSS3ResizedImageCache>());
"""

BUCKET_PER_TENANT = """
{
  "BucketName": "{{ ShellSettings.Name }}-media",
  "Region": "eu-central-1",
  "BasePath": "/media"
}
"""

BASE_PATH_PER_TENANT = """
{
  "BucketName": "oc-media",
  "Region": "eu-central-1",
  "BasePath": "{{ ShellSettings.Name }}/Media"
}
"""

REMOVAL = """
{
  // Deletes the bucket when the tenant is removed.
  "RemoveBucket": false
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
            image = slide_base(chapter_title, label, "Why Amazon S3")
            bullets(image, PROBLEMS)
        elif index == 2:
            image = slide_base(chapter_title, label, "The features of the Amazon S3 Media module")
            bullets(image, FEATURES)
        else:
            image = slide_base(chapter_title, label, "How a request is served")
            bullets(image, REQUEST)
        return image

    if step_id in ("conf-1", "conf-3"):
        image = slide_base(chapter_title, label, "Connect a bucket")
        highlight = {5, 6, 7, 8} if step_id == "conf-1" else {9, 10, 11, 12}
        draw_code(image, LEFT, "appsettings.json", "json", CONFIGURATION, highlight=highlight, size=26)
        if step_id == "conf-1":
            notes = [("Required", "Bucket Name"), ("Required", "Region or ServiceURL")]
        else:
            notes = [("Optional", "Base Path"), ("Optional", "Create Bucket")]
        note_cards(image, notes, NOTES)
        return image

    if step_id == "conf-2":
        image = slide_base(chapter_title, label, "Where the credentials come from")
        bullets(image, CREDENTIALS)
        return image

    if step_id == "conf-4":
        image = slide_base(chapter_title, label, "Keep the keys out of the file")
        draw_code(image, (96, 240, W - 96, 500), "Environment variables", "bash", ENVIRONMENT, size=26)
        note_cards(image, [
            ("They override the file", "They are read after appsettings.json and the user secrets."),
            ("Two underscores", "Each double underscore is one level of the setting's path."),
        ], (96, 550, W - 96, 1030))
        return image

    if step_id == "conf-5":
        image = slide_base(chapter_title, label, "Try it with an S3 emulator")
        draw_code(image, (96, 240, W - 96, 380), "Terminal", "bash", DOCKER, size=26)
        draw_code(image, (96, 410, 1146, 1030), "appsettings.json", "json", EMULATOR, size=23)
        note_cards(image, [
            ("ServiceURL", "Replaces the region."),
            ("ForcePathStyle", "Bucket in the path."),
            ("Dummy keys", "Emulators ignore them."),
        ], (1186, 410, W - 96, 1030))
        return image

    if step_id == "svc-1":
        image = slide_base(chapter_title, label, "What the feature registers")
        draw_code(image, (96, 240, W - 96, 1030), "Startup.cs", "csharp", STARTUP, types=TYPES, size=22)
        return image

    if step_id == "svc-2":
        image = slide_base(chapter_title, label, "Resumable uploads")
        draw_code(image, (96, 240, W - 96, 1030), "Startup.cs", "csharp", TUS, types=TYPES, size=24)
        return image

    if step_id == "cache-1":
        image = slide_base(chapter_title, label, "Amazon Media Image Cache settings")
        draw_code(image, LEFT_NARROW, "appsettings.json", "json", IMAGE_CACHE_CONFIGURATION, size=26)
        note_cards(image, [
            ("Its own bucket", "Name, creation and removal."),
            ("The rest is shared", "Region and AWS settings."),
        ], NOTES_WIDE)
        return image

    if step_id == "cache-2":
        image = slide_base(chapter_title, label, "The same interface, stored in a bucket")
        draw_code(image, (96, 240, W - 96, 700), "IResizedImageCache.cs", "csharp", RESIZED_IMAGE_CACHE,
                  types=TYPES, size=22)
        draw_code(image, (96, 740, W - 96, 960), "Startup.cs", "csharp", IMAGE_CACHE_REGISTRATION, types=TYPES, size=22)
        return image

    if step_id == "tenant-1":
        image = slide_base(chapter_title, label, "One bucket per tenant, or one folder")
        draw_code(image, (96, 240, 940, 560), "Bucket per tenant", "json", BUCKET_PER_TENANT, size=24)
        draw_code(image, (980, 240, W - 96, 560), "Folder per tenant", "json", BASE_PATH_PER_TENANT, size=24)
        note_cards(image, [
            ("Used as rendered, never lowercased", "A tenant named Default gives Default-media, with a capital D."),
            ("It must be a valid bucket name", "Bucket names have no capital letters. Only Bucket Name and Base Path accept Liquid."),
        ], (96, 620, W - 96, 1030))
        return image

    if step_id == "tenant-2":
        image = slide_base(chapter_title, label, "When a tenant is removed")
        draw_code(image, LEFT, "appsettings.json", "json", REMOVAL, size=26)
        note_cards(image, [
            ("False by default", "The bucket is left alone."),
            ("Bucket only", "Its objects are not deleted first."),
            ("Image cache", "Its own bucket, its own setting."),
        ], NOTES)
        return image

    raise SystemExit(f"No slide for {step_id}")
