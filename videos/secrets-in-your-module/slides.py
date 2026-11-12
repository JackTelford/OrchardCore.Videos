"""The slides of the "Secrets in Your Module" video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
the code of the Secrets documentation (the MyService example), and of the SMTP provider of Orchard Core, trimmed with
// ... to what the narration talks about.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "SecretInputViewModel", "SecretInputUpdateContext", "SecretInputSource", "SecretMigration",
    "SecretMigrationItemViewModel", "MyServiceSettings", "MyServiceSettingsViewModel", "MyServiceConstants",
    "MyServiceSettingsDisplayDriver", "MyServiceSecretMigrationDisplayDriver", "SiteDisplayDriver", "DisplayDriver",
    "IDisplayResult", "ISite", "BuildEditorContext", "UpdateEditorContext", "Task", "StartupBase", "IServiceCollection",
    "SecretsStartup", "Startup", "SmtpOptions", "EmailProviderOptions", "EmailOptions", "RequireFeatures",
    "BindNever", "Module", "ManifestConstants",
}

PIECES = [
    ("1", "SecretInputViewModel", "The editor model of the credential."),
    ("2", "The secret-input tag helper", "Renders the editor of the credential."),
    ("3", "GetSecretValueAsync", "Reads the clear value at runtime."),
    ("4", "SecretMigration", "The model of the optional Migrate Secrets screen."),
]

MANIFEST = """
// The SMTP email provider depends on Email, not on Secrets.
[assembly: Module(
    Name = "SMTP Email Provider",
    // ...
    Dependencies =
    [
        "OrchardCore.Email"
    ],
    Category = "Communication"
)]
"""

SETTINGS = """
public class MyServiceSettings
{
    // The API key, protected with Data Protection.
    public string ApiKey { get; set; }

    // The name of the secret containing the API key, which takes precedence over ApiKey.
    public string ApiKeySecretName { get; set; }
}
"""

CONSTANTS = """
public static class MyServiceConstants
{
    public const string ProtectorName = "MyService";

    public const string ApiKeySecretName = "MyService.ApiKey";
}
"""

VIEW_MODEL = """
public class MyServiceSettingsViewModel
{
    public SecretInputViewModel ApiKey { get; set; } = new();
}
"""

INPUT_VIEW_MODEL = """
public class SecretInputViewModel
{
    public SecretInputSource Source { get; set; } // Value or Secret.
    public string Value { get; set; }             // A typed value. Empty keeps the current one.
    public string SecretName { get; set; }        // The name of the referenced secret.

    [BindNever]
    public bool HasValue { get; set; }            // A protected value is kept in the settings.

    public static SecretInputViewModel Create(string protectedValue, string secretName) => new() { /* ... */ };
}
"""

EDIT = """
public sealed class MyServiceSettingsDisplayDriver : SiteDisplayDriver<MyServiceSettings>
{
    protected override string SettingsGroupId => "MyService";

    public override IDisplayResult Edit(ISite site, MyServiceSettings settings, BuildEditorContext context)
        => Initialize<MyServiceSettingsViewModel>("MyServiceSettings_Edit", model =>
        {
            model.ApiKey = SecretInputViewModel.Create(settings.ApiKey, settings.ApiKeySecretName);
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);

    // ...
}
"""

UPDATE = """
public override async Task<IDisplayResult> UpdateAsync(ISite site, MyServiceSettings settings, UpdateEditorContext context)
{
    var model = new MyServiceSettingsViewModel();
    await context.Updater.TryUpdateModelAsync(model, Prefix);

    var apiKey = await model.ApiKey.UpdateAsync(new SecretInputUpdateContext(
        _serviceProvider,
        _dataProtectionProvider.CreateProtector(MyServiceConstants.ProtectorName),
        context.Updater.ModelState,
        $"{Prefix}.{nameof(model.ApiKey)}")
    {
        ProtectedValue = settings.ApiKey,
        SecretName = settings.ApiKeySecretName,
    });

    // ...
}
"""

RESULT = """
if (apiKey.Succeeded)
{
    settings.ApiKey = apiKey.ProtectedValue;
    settings.ApiKeySecretName = apiKey.SecretName;
}

return await EditAsync(site, settings, context);
"""

STARTUP = """
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
        => services.AddSiteDisplayDriver<MyServiceSettingsDisplayDriver>();
}
"""

VIEW_IMPORTS = """
@addTagHelper *, OrchardCore.Secrets.Abstractions
"""

VIEW = """
@model MyServiceSettingsViewModel

<div class="ocat-wrapper" asp-validation-class-for="ApiKey">
    <label asp-for="ApiKey" class="ocat-label">@T["API key"]</label>
    <div class="ocat-end">
        <secret-input asp-for="ApiKey" secret-name="@MyServiceConstants.ApiKeySecretName" />
        <span class="hint">@T["The API key of the service."]</span>
    </div>
</div>
"""

RUNTIME = """
var apiKey = await _serviceProvider.GetSecretValueAsync(
    settings.ApiKeySecretName,
    settings.ApiKey,
    _dataProtectionProvider.CreateProtector(MyServiceConstants.ProtectorName),
    _logger);
"""

SMTP_SAVE = """
// SmtpSettingsDisplayDriver.UpdateAsync
if (password.Succeeded)
{
    hasChanges |= password.ProtectedValue != settings.Password;
    hasChanges |= password.SecretName != settings.PasswordSecretName;
    // ...
}

// ...

if (context.Updater.ModelState.IsValid)
{
    // ...
    if (hasChanges)
    {
        _optionsUpdateNotifier.RequestUpdate<SmtpOptions>(); // and the options that build on it
    }
}

// Startup.ConfigureServices
services.AddSignalOptionsChangeTokenSource<SmtpOptions>();
"""

MIGRATION_CLASS = """
public sealed class MyServiceSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    // ...
}

[RequireFeatures("OrchardCore.Secrets")]
public sealed class SecretsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
        => services.AddDisplayDriver<SecretMigration, MyServiceSecretMigrationDisplayDriver>();
}
"""

MIGRATION_EDIT = """
protected override void BuildPrefix(SecretMigration model, string htmlFieldPrefix)
{
    base.BuildPrefix(model, htmlFieldPrefix);
    Prefix += ".MyService";
}

public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
{
    var settings = await _siteService.GetSettingsAsync<MyServiceSettings>();
    // CanMigrate: a value is kept, no secret is referenced. AuthorizeAsync: the permission of the settings.
    if (!CanMigrate(settings) || !await AuthorizeAsync())
    {
        return null;
    }

    return Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
    {
        model.Group = S["My service"];
        model.DisplayName = S["API key"];
        model.SecretName = MyServiceConstants.ApiKeySecretName;
    }).Location("Content");
}
"""

MIGRATION_UPDATE = """
public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
{
    // ...
    if (model.Migrate)
    {
        await migration.MoveToSecretAsync(
            _secretManager,
            _dataProtectionProvider.CreateProtector(MyServiceConstants.ProtectorName),
            settings.ApiKey,
            model.SecretName,
            S["My service: API key"],
            async secretName =>
            {
                settings.ApiKeySecretName = secretName;
                settings.ApiKey = null;
                site.Put(settings);
                await _siteService.UpdateSiteSettingsAsync(site);
            });
    }
    // ...
}
"""

CONTENT_PART = """
public sealed class WebhookPart : ContentPart
{
    public string Url { get; set; }

    // The signing key, protected with Data Protection.
    public string SigningKey { get; set; }

    // The name of the secret holding the signing key.
    public string SigningKeySecretName { get; set; }
}
"""


def slide(step_id, chapter_title, label):
    if step_id == "pieces-1":
        image = slide_base(chapter_title, label, "What the abstractions provide")
        bullets(image, PIECES)
        return image

    if step_id == "pieces-2":
        image = slide_base(chapter_title, label, "Step 1: Reference the abstractions")
        draw_code(image, (96, 240, 1000, 700), "Manifest.cs", "csharp", MANIFEST, types=TYPES, size=24)
        note_cards(image, [
            ("A reference", "OrchardCore.Secrets.Abstractions"),
            ("No dependency", "on the Secrets feature."),
            ("Detected at runtime", "The feature registers ISecretManager."),
        ], (1040, 240, W - 96, 1030))
        return image

    if step_id == "settings-1":
        image = slide_base(chapter_title, label, "Step 2: Store the value and the name")
        draw_code(image, (96, 240, W - 96, 700), "MyServiceSettings.cs", "csharp", SETTINGS, types=TYPES, size=28)
        return image

    if step_id == "settings-2":
        image = slide_base(chapter_title, label, "Step 2: Constants for the purpose and the name")
        draw_code(image, (96, 240, 1060, 640), "MyServiceConstants.cs", "csharp", CONSTANTS, types=TYPES, size=25)
        note_cards(image, [
            ("Never change the purpose", "Another purpose can't decrypt it."),
            ("The suggested name", "Offered when a secret is created."),
        ], (1100, 240, W - 96, 1030))
        return image

    if step_id == "driver-1":
        image = slide_base(chapter_title, label, "Step 3: The editor model")
        draw_code(image, (96, 240, W - 96, 490), "MyServiceSettingsViewModel.cs", "csharp", VIEW_MODEL, types=TYPES, size=26)
        draw_code(image, (96, 530, W - 96, 1030), "SecretInputViewModel.cs", "csharp", INPUT_VIEW_MODEL, types=TYPES, size=24)
        return image

    if step_id == "driver-2":
        image = slide_base(chapter_title, label, "Step 4: Create the model in the driver")
        draw_code(image, (96, 240, W - 96, 800), "MyServiceSettingsDisplayDriver.cs", "csharp", EDIT, types=TYPES, size=24)
        return image

    if step_id == "driver-3":
        image = slide_base(chapter_title, label, "Step 4: Apply the posted model")
        draw_code(image, (96, 240, W - 96, 940), "MyServiceSettingsDisplayDriver.cs", "csharp", UPDATE, types=TYPES, size=24)
        return image

    if step_id == "driver-4":
        image = slide_base(chapter_title, label, "Step 4: Store the result when it succeeded")
        draw_code(image, (96, 240, 1000, 740), "MyServiceSettingsDisplayDriver.cs", "csharp", RESULT, types=TYPES, size=24)
        note_cards(image, [
            ("A secret is selected", "The settings keep its name only."),
            ("A value is typed", "It is protected, and kept."),
            ("The input is not valid", "Errors are added. The settings stay as they are."),
        ], (1040, 240, W - 96, 1030))
        return image

    if step_id == "driver-5":
        image = slide_base(chapter_title, label, "Step 4: Register the driver")
        draw_code(image, (96, 240, W - 96, 700), "Startup.cs", "csharp", STARTUP, types=TYPES, size=28)
        return image

    if step_id == "view-1":
        image = slide_base(chapter_title, label, "Step 5: Render the editor")
        draw_code(image, (96, 240, W - 96, 380), "Views/_ViewImports.cshtml", "html", VIEW_IMPORTS, types=TYPES, size=26)
        draw_code(image, (96, 420, W - 96, 1030), "Views/MyServiceSettings.Edit.cshtml", "html", VIEW, types=TYPES, size=26)
        return image

    if step_id == "view-2":
        image = slide_base(chapter_title, label, "What the tag helper renders")
        note_cards(image, [
            ("The Secrets feature is disabled", "A password input. The value is protected, and kept in the settings."),
            ("The Secrets feature is enabled", "A choice: User-specified value, or Use a secret."),
            ("The placeholder attribute", "Optional. The placeholder of the value input."),
        ], (96, 260, W - 96, 1030))
        return image

    if step_id == "runtime-1":
        image = slide_base(chapter_title, label, "Step 6: Read the value at runtime")
        draw_code(image, (96, 240, W - 96, 540), "MyService.cs", "csharp", RUNTIME, types=TYPES, size=24)
        note_cards(image, [
            ("The secret first", "When it is referenced and has a value."),
            ("Otherwise", "The protected value is decrypted."),
        ], (96, 590, W - 96, 1030))
        return image

    if step_id == "runtime-2":
        image = slide_base(chapter_title, label, "Step 6: Read the value at runtime")
        draw_code(image, (96, 240, W - 96, 540), "MyService.cs", "csharp", RUNTIME, types=TYPES, highlight={5}, size=24)
        note_cards(image, [
            ("Null", "No value is available."),
            ("The optional logger", "Receives the reason, never the value."),
        ], (96, 590, W - 96, 1030))
        return image

    if step_id == "runtime-3":
        image = slide_base(chapter_title, label, "Cached options: signal the change")
        draw_code(image, (96, 240, W - 96, 1030), "SmtpSettingsDisplayDriver.cs and Startup.cs", "csharp", SMTP_SAVE,
                  types=TYPES, size=23)
        return image

    if step_id == "migrate-1":
        image = slide_base(chapter_title, label, "Step 7: List the credential on Migrate Secrets")
        draw_code(image, (96, 240, W - 96, 800), "MyServiceSecretMigrationDisplayDriver.cs", "csharp", MIGRATION_CLASS,
                  types=TYPES, size=24)
        return image

    if step_id == "migrate-2":
        image = slide_base(chapter_title, label, "Step 7: The editor of the migration")
        draw_code(image, (96, 240, W - 96, 1030), "MyServiceSecretMigrationDisplayDriver.cs", "csharp", MIGRATION_EDIT,
                  types=TYPES, size=22)
        return image

    if step_id == "migrate-3":
        image = slide_base(chapter_title, label, "Step 7: Move the credential")
        draw_code(image, (96, 240, W - 96, 1030), "MyServiceSecretMigrationDisplayDriver.cs", "csharp", MIGRATION_UPDATE,
                  types=TYPES, size=23)
        return image

    if step_id == "other-1":
        image = slide_base(chapter_title, label, "Credentials outside site settings")
        draw_code(image, (96, 240, 1000, 700), "WebhookPart.cs", "csharp", CONTENT_PART, types=TYPES, size=24)
        note_cards(image, [
            ("Both values, side by side", "The protected value, and the secret name."),
            ("Edit and read", "SecretInputViewModel, and GetSecretValueAsync."),
            ("What changes", "The driver, and where the values are saved."),
        ], (1040, 240, W - 96, 1030))
        return image

    raise SystemExit(f"No slide for {step_id}")
