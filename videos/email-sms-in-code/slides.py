"""The slides of the "Send Email, SMS and Notifications From Your Module" video. assemble.py calls slide() for each
step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video. The code is
trimmed from the documentation and the source of the Email, SMS and Notifications modules, with fake names and
addresses only.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

CODE_SIZE = 23
LINE_HEIGHT = int(CODE_SIZE * 1.42)
NOTES_BOX = (1190, 260, W - 96, 1030)

TYPES = {
    "OrderMailer", "OrderTexter", "OrderNotifier", "OrderNotificationTypes", "Startup", "StartupBase",
    "IEmailService", "MailMessage", "ISmsService", "SmsMessage", "INotificationService", "NotificationMessage",
    "NotificationSendResult", "INotificationManager", "NotificationFilter", "INotifier", "IHtmlLocalizer", "User",
    "Task", "IServiceCollection", "LocalizationSource", "INotificationMethodProvider", "IEmailServiceEvents",
    "ExampleMailProvider", "ExampleSmsProvider", "ExamplePushProvider", "ExampleEmailEvents", "Module",
}

SERVICES = [
    ("1", "IEmailService", "Sends a MailMessage through an email provider."),
    ("2", "ISmsService", "Sends an SmsMessage through the default SMS provider."),
    ("3", "INotificationService", "Stores a notification for a user, then sends it by each notification method."),
    ("4", "Results", "Result for email and SMS. NotificationSendResult for notifications."),
]

METHODS = [
    ("1", "Features", "Email Notifications and SMS Notifications add the Email and SMS methods."),
    ("2", "Preferences", "Each user turns methods on or off, for each type of notification."),
    ("3", "Failures", "A method that cannot reach the user fails alone. The notification is still stored."),
]

LOG_PROVIDER = [
    ("1", "Log provider", "Writes each message to the application log instead of sending it."),
    ("2", "Development only", "Orchard Core registers it when the host environment is Development."),
    ("3", "Default SMS provider", "Select it in the SMS settings to send messages through it."),
]

MANIFEST = """
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Order Messages",
    Author = "Jamie Chen",
    Version = "1.0.0",
    Description = "Sends messages about orders.",
    Category = "Communication",
    Dependencies =
    [
        "OrchardCore.Email",
        "OrchardCore.Sms",
        "OrchardCore.Notifications",
    ]
)]
"""

EMAIL = """
public sealed class OrderMailer
{
    private readonly IEmailService _emailService;

    public OrderMailer(IEmailService emailService)
        => _emailService = emailService;

    public async Task<bool> SendReceiptAsync(string email)
    {
        var message = new MailMessage
        {
            To = email,
            Subject = "Your receipt",
            HtmlBody = "<p>Thank you for your order.</p>",
            TextBody = "Thank you for your order.",
        };

        var result = await _emailService.SendAsync(message);
        return result.Succeeded;
    }
}
"""

EMAIL_CALLS = """
// The default provider.
var result = await _emailService.SendAsync(message);

// A provider, by its technical name.
result = await _emailService.SendAsync(message, "SMTP");

// The recipient, the subject and an HTML body.
result = await _emailService.SendAsync(
    "jamie.chen@example.com",
    "Your receipt",
    "<p>Thank you for your order.</p>");
"""

SMS = """
// _smsService is an ISmsService, injected like IEmailService.
public async Task<bool> SendShippedAsync(string phoneNumber)
{
    var message = new SmsMessage
    {
        To = phoneNumber,
        Body = "Your order has shipped.",
    };

    var result = await _smsService.SendAsync(message);

    return result.Succeeded;
}
"""

SMS_CALLS = """
// The default SMS provider, selected in the site settings.
var result = await _smsService.SendAsync(message);

// The phone number and the text.
result = await _smsService.SendAsync(
    "+14255550142",
    "Your order has shipped.");
"""

SMS_GUARD = """
if (string.IsNullOrWhiteSpace(phoneNumber)
    || string.IsNullOrWhiteSpace(text))
{
    return false;
}

var result = await _smsService.SendAsync(phoneNumber, text);

return result.Succeeded;
"""

NOTIFY = """
public sealed class OrderNotifier
{
    private readonly INotificationService _notificationService;

    public OrderNotifier(INotificationService notificationService)
        => _notificationService = notificationService;

    public Task<NotificationSendResult> NotifyShippedAsync(User user)
        => _notificationService.SendAsync(user, new NotificationMessage
        {
            Subject = "Your order shipped",
            Summary = "<strong>Order 1042</strong> left the warehouse.",
            TextBody = "Order 1042 left the warehouse.",
            HtmlBody = "<p>Order 1042 left the warehouse.</p>",
            IsHtmlPreferred = true,
        });
}
"""

NOTIFY_RESULT = """
var result = await _notificationService.SendAsync(user, message);

var stored = result.Notification;   // The notification kept for the user.
var sent = result.SuccessfulCount;  // The methods that sent it.
var failed = result.FailedCount;    // The methods that failed.
var errors = result.Errors;         // Why they failed.
var status = result.Status;         // None, Success, PartiallySuccessful or Failed.
"""

NOTIFIER = """
// INotifier: a message in the next page, shown once.
await _notifier.SuccessAsync(H["The order was saved."]);

// INotificationService: a notification kept for the user.
await _notificationService.SendAsync(user, message);
"""

TYPE_STARTUP = """
public static class OrderNotificationTypes
{
    public const string Shipped = "OrderShipped";
}

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNotificationType(
            OrderNotificationTypes.Shipped,
            LocalizationSource.Create("Shipped orders", typeof(Startup)),
            type =>
            {
                type.Description = LocalizationSource.Create(
                    "When one of your orders ships.", typeof(Startup));
                type.DisabledMethods.Add("SMS");
            });
    }
}
"""

TYPE_SEND = """
var message = new NotificationMessage
{
    Subject = "Your order shipped",
    Summary = "Order 1042 left the warehouse.",
    TextBody = "Order 1042 left the warehouse.",
    Type = OrderNotificationTypes.Shipped,
    ReferenceId = "1042",
    Url = "~/orders/1042",
};

await _notificationService.SendAsync(user, message);
"""

TYPE_MANAGER = """
// The order was cancelled: its notifications go away.
await _notificationManager.DeleteAsync(new NotificationFilter
{
    Type = OrderNotificationTypes.Shipped,
    ReferenceId = "1042",
});

// The order was delivered: mark them as read.
await _notificationManager.MarkAsReadAsync(new NotificationFilter
{
    Type = OrderNotificationTypes.Shipped,
    ReferenceId = "1042",
});
"""

PROVIDERS = """
// IEmailProvider: DisplayName and SendAsync(MailMessage).
services.AddEmailProvider<ExampleMailProvider>("ExampleMail");

// ISmsProvider: Name and SendAsync(SmsMessage).
services.AddSmsProvider<ExampleSmsProvider>("ExampleSms");
"""

METHOD_AND_EVENTS = """
// INotificationMethodProvider: Method, Name and SendAsync.
services.AddScoped<INotificationMethodProvider, ExamplePushProvider>();

// IEmailServiceEvents: Validating, Validated, Sending, Sent, Failed.
services.AddScoped<IEmailServiceEvents, ExampleEmailEvents>();
"""


def code_box(code, x1=1150, x0=96, y0=240):
    """The box of a code panel just tall enough for its lines: a window bar, then one line height per line."""
    lines = len(code.strip("\n").split("\n"))
    return x0, y0, x1, y0 + 74 + lines * LINE_HEIGHT + 36


def code_slide(chapter_title, label, heading, name, language, code, highlight=None, notes=None, wide=False):
    image = slide_base(chapter_title, label, heading)
    x0, y0, x1, y1 = code_box(code, x1=W - 96 if wide else 1150)
    draw_code(image, (x0, y0, x1, y1), name, language, code, types=TYPES, highlight=highlight, size=CODE_SIZE)
    if notes:
        note_cards(image, notes, NOTES_BOX)
    return image


def slide(step_id, chapter_title, label):
    if step_id in ("services-1", "services-2"):
        image = slide_base(chapter_title, label, "Three services, one pattern")
        bullets(image, SERVICES, active={0, 1, 2} if step_id == "services-1" else {3})
        return image

    if step_id == "services-3":
        return code_slide(chapter_title, label, "Declare the features in the manifest", "Manifest.cs", "csharp", MANIFEST,
                          notes=[("Email Notifications", "OrchardCore.Notifications.Email"),
                                 ("SMS Notifications", "OrchardCore.Notifications.Sms"),
                                 ("Optional features", "Each one adds a method.")])

    if step_id == "email-1":
        return code_slide(chapter_title, label, "1. Inject IEmailService", "OrderMailer.cs", "csharp", EMAIL,
                          highlight={1, 2, 3, 5, 6, 21},
                          notes=[("Namespace", "OrchardCore.Email"), ("Registered by", "The Email feature.")])
    if step_id == "email-2":
        return code_slide(chapter_title, label, "2. Create a MailMessage", "OrderMailer.cs", "csharp", EMAIL,
                          highlight=set(range(10, 17)),
                          notes=[("To, Cc, Bcc", "Separate addresses with , or ;"),
                                 ("HtmlBody", "The HTML version."),
                                 ("TextBody", "The plain text version.")])
    if step_id == "email-3":
        return code_slide(chapter_title, label, "3. Send it and check the Result", "OrderMailer.cs", "csharp", EMAIL,
                          highlight={18, 19},
                          notes=[("Succeeded", "True when it was sent."),
                                 ("Errors", "A Key and a Message each."),
                                 ("Validation", "Needs To, Cc or Bcc.")])
    if step_id == "email-4":
        return code_slide(chapter_title, label, "4. Choose a provider, or a shorter overload", "OrderMailer.cs", "csharp",
                          EMAIL_CALLS,
                          notes=[("Technical name", "SMTP, or the name you register."),
                                 ("Unknown name", "Throws an exception."),
                                 ("Shorter overload", "Recipient, subject and body.")])

    if step_id == "sms-1":
        return code_slide(chapter_title, label, "1. Create an SmsMessage", "OrderTexter.cs", "csharp", SMS,
                          highlight=set(range(4, 9)),
                          notes=[("To", "Phone number, with country code."),
                                 ("Body", "The text to send."),
                                 ("From", "Optional: the provider's number.")])
    if step_id == "sms-2":
        return code_slide(chapter_title, label, "2. Send it to the default provider", "OrderTexter.cs", "csharp", SMS_CALLS,
                          notes=[("No provider argument", "The default SMS provider."),
                                 ("Shorter overload", "Phone number and text.")])
    if step_id == "sms-3":
        return code_slide(chapter_title, label, "3. Check the values first", "OrderTexter.cs", "csharp", SMS_GUARD,
                          notes=[("Twilio", "Throws an exception."),
                                 ("Azure", "Returns a failed Result."),
                                 ("Your code", "Check To and Body first.")])
    if step_id == "sms-4":
        image = slide_base(chapter_title, label, "While you develop: the Log provider")
        bullets(image, LOG_PROVIDER)
        return image

    if step_id == "notify-1":
        return code_slide(chapter_title, label, "1. Inject INotificationService", "OrderNotifier.cs", "csharp", NOTIFY,
                          highlight={1, 2, 3, 5, 6, 8, 17},
                          notes=[("Recipient", "A User object."),
                                 ("Stored first", "In the user's notification center."),
                                 ("Then sent", "By each method the user receives.")])
    if step_id == "notify-2":
        return code_slide(chapter_title, label, "2. Build a NotificationMessage", "OrderNotifier.cs", "csharp", NOTIFY,
                          highlight=set(range(9, 17)),
                          notes=[("Subject", "Plain text."),
                                 ("Summary", "May hold HTML."),
                                 ("TextBody", "Sent by email and SMS: set it.")])
    if step_id == "notify-3":
        return code_slide(chapter_title, label, "3. Read the NotificationSendResult", "OrderNotifier.cs", "csharp",
                          NOTIFY_RESULT, wide=True)
    if step_id == "notify-4":
        image = slide_base(chapter_title, label, "Which methods run")
        bullets(image, METHODS)
        return image
    if step_id == "notify-5":
        return code_slide(chapter_title, label, "INotifier or INotificationService?", "Examples.cs", "csharp", NOTIFIER,
                          notes=[("INotifier", "A message in the page, shown once."),
                                 ("INotificationService", "A notification for one user.")])

    if step_id == "types-1":
        return code_slide(chapter_title, label, "1. Register the type", "Startup.cs", "csharp", TYPE_STARTUP,
                          highlight={1, 2, 3, 4, 10, 11, 12, 18},
                          notes=[("Technical name", "Stored on each notification."),
                                 ("Display name", "A LocalizationSource.")])
    if step_id == "types-2":
        return code_slide(chapter_title, label, "2. Display name and default methods", "Startup.cs", "csharp", TYPE_STARTUP,
                          highlight={12, 15, 16, 17},
                          notes=[("Type filter", "Lists the registered types."),
                                 ("Preferences", "A row per type."),
                                 ("DisabledMethods", "Off until the user turns it on.")])
    if step_id == "types-3":
        return code_slide(chapter_title, label, "3. Send notifications of the type", "OrderNotifier.cs", "csharp", TYPE_SEND,
                          highlight={6, 7, 8},
                          notes=[("Type", "At most 64 characters."),
                                 ("ReferenceId", "At most 255 characters."),
                                 ("Url", "~/ path, / path or http(s) URL.")])
    if step_id == "types-4":
        return code_slide(chapter_title, label, "4. Keep them in sync with the record", "OrderNotifier.cs", "csharp",
                          TYPE_MANAGER,
                          notes=[("Filter", "UserId, Type, ReferenceId, IsRead."),
                                 ("Required", "One of the first three."),
                                 ("Also", "ListAsync and UpdateAsync.")])

    if step_id == "extend-1":
        return code_slide(chapter_title, label, "Add an email or SMS provider", "Startup.cs", "csharp", PROVIDERS,
                          notes=[("Technical name", "Listed as a default provider."),
                                 ("Email only", "Also accepted by SendAsync.")])
    if step_id == "extend-2":
        return code_slide(chapter_title, label, "Add a notification method or follow email events", "Startup.cs", "csharp",
                          METHOD_AND_EVENTS,
                          notes=[("Notification method", "Listed in user preferences."),
                                 ("EmailServiceEventsBase", "Override only what you need.")])

    raise SystemExit(f"No slide for {step_id}")
