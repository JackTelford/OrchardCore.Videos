"""The slides of the OpenID Connect Token Validation video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

TYPES = {
    "ApiAuthenticationHandler", "AuthenticationHandler", "ApiAuthorizationOptions", "AuthenticateResult",
    "AuthenticationProperties", "AuthenticationOptions", "OpenIddictValidationAspNetCoreHandler",
    "OpenIddictValidationAspNetCoreDefaults", "OrchardCoreConstants", "ContentApiController", "ControllerBase",
    "IActionResult", "Permissions", "ObjectResult", "Task",
}

# From OrchardCore.Infrastructure: Security/ApiAuthenticationHandler.cs, trimmed.
HANDLER = """
public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthorizationOptions>
{
    // ...
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!_authenticationOptions.Value.SchemeMap.ContainsKey(Options.ApiAuthenticationScheme))
        {
            return Task.FromResult<AuthenticateResult>(AuthenticateResult.NoResult());
        }
        return Context.AuthenticateAsync(Options.ApiAuthenticationScheme);
    }
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // ...
        if (!_authenticationOptions.Value.SchemeMap.ContainsKey(Options.ApiAuthenticationScheme))
        {
            // ...
            Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
            return Task.CompletedTask;
        }
        return Context.ChallengeAsync(Options.ApiAuthenticationScheme);
    }
}
"""

# From OrchardCore.Infrastructure.Abstractions: Security/ApiAuthorizationOptions.cs.
OPTIONS = """
public class ApiAuthorizationOptions : AuthenticationSchemeOptions
{
    public string ApiAuthenticationScheme { get; set; } = "Bearer";
}
"""

# From OrchardCore.OpenId: Configuration/OpenIdValidationConfiguration.cs, trimmed.
CONFIGURATION = """
public void Configure(AuthenticationOptions options)
{
    // ...
    if (settings.Authority != null)
    {
        options.AddScheme<OpenIddictValidationAspNetCoreHandler>(
            OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, displayName: null);
        return;
    }
    // ...
}

public void Configure(string name, ApiAuthorizationOptions options)
{
    options.ApiAuthenticationScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
}
"""

# From OrchardCore.Demo: Controllers/ContentApiController.cs, trimmed.
CONTROLLER = """
[Route("api/demo")]
[Authorize(AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api), IgnoreAntiforgeryToken, AllowAnonymous]
[ApiController]
public sealed class ContentApiController : ControllerBase
{
    // ...

    public async Task<IActionResult> GetAuthorizedById(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.DemoAPIAccess))
        {
            return this.ChallengeOrForbid(OrchardCoreConstants.AuthenticationSchemes.Api);
        }

        // ...
        return new ObjectResult(contentItem);
    }
}
"""

RECIPE_LOCAL = """
{
  "steps": [
    {
      "name": "feature",
      "enable": [ "OrchardCore.OpenId.Server", "OrchardCore.OpenId.Validation" ]
    },
    { "name": "OpenIdValidationSettings", "Tenant": "Default" }
  ]
}
"""

RECIPE_REMOTE = """
{
  "name": "OpenIdValidationSettings",
  "Authority": "https://idp.example.com",
  "Audience": "inventory-api"
}
"""

FLOW = [
    ("1", "Client", "Sends an access token with each request."),
    ("2", "API scheme", "Passes the request to a token handler."),
    ("3", "Token handler", "Checks the issuer, the audience and the signing keys."),
    ("4", "Your endpoint", "Checks the permissions of the user in the token."),
]

ISSUER = [
    ("1", "Authorization server tenant", "Keys are read from that tenant. The audience is fixed."),
    ("2", "Authority", "Keys come from discovery. An audience is required."),
    ("3", "Exactly one", "Both empty, or both set, is refused when you save."),
]

TENANT_NOTES = [
    ("A scope on the server tenant", "Additional resources (API audiences): oct:Reporting"),
    ("Reporting is the name of this tenant", "Without such a scope: No appropriate scope was found."),
    ("Not accepted", "A server tenant that uses reference access tokens."),
]

OUTCOME_NOTES = [
    ("No valid token", "The caller is challenged: asked to authenticate."),
    ("Valid token, permission missing", "The caller is forbidden."),
    ("Valid token, permission granted", "The action carries on."),
]

RECIPE_NOTES = [
    ("The page validates", "Saving in the admin checks the values."),
    ("The recipe step does not", "It stores what it receives."),
    ("Invalid values", "No handler is registered, and the site logs a warning."),
]


def slide(step_id, chapter_title, label):
    if step_id == "flow-1":
        image = slide_base(chapter_title, label, "Four parts of an API call")
        bullets(image, FLOW, active={0, 1})
        return image

    if step_id == "flow-2":
        image = slide_base(chapter_title, label, "The API scheme forwards the request")
        draw_code(image, (96, 240, W - 96, 1030), "ApiAuthenticationHandler.cs", "csharp", HANDLER, types=TYPES, size=22)
        return image

    if step_id == "flow-3":
        image = slide_base(chapter_title, label, "The feature chooses what it forwards to")
        draw_code(image, (96, 240, W - 96, 440), "ApiAuthorizationOptions.cs", "csharp", OPTIONS, types=TYPES, size=22)
        draw_code(image, (96, 456, W - 96, 1030), "OpenIdValidationConfiguration.cs", "csharp", CONFIGURATION,
                  types=TYPES, size=22)
        return image

    if step_id == "issuer-1":
        image = slide_base(chapter_title, label, "Who issues the tokens")
        bullets(image, ISSUER)
        return image

    if step_id == "issuer-2":
        image = slide_base(chapter_title, label, "Who issues the tokens")
        bullets(image, ISSUER, active={0})
        return image

    if step_id == "issuer-3":
        image = slide_base(chapter_title, label, "Who issues the tokens")
        bullets(image, ISSUER, active={1})
        return image

    if step_id == "issuer-4":
        image = slide_base(chapter_title, label, "Another tenant as the issuer")
        note_cards(image, TENANT_NOTES, (96, 250, W - 96, 1000))
        return image

    if step_id == "endpoint-1":
        image = slide_base(chapter_title, label, "Name the API scheme")
        draw_code(image, (96, 240, W - 96, 1030), "ContentApiController.cs", "csharp", CONTROLLER, types=TYPES,
                  highlight={1, 2, 3, 4}, size=22)
        return image

    if step_id == "endpoint-2":
        image = slide_base(chapter_title, label, "Check the permission in the action")
        draw_code(image, (96, 240, W - 96, 1030), "ContentApiController.cs", "csharp", CONTROLLER, types=TYPES,
                  highlight={10, 11, 12, 13}, size=22)
        return image

    if step_id == "endpoint-3":
        image = slide_base(chapter_title, label, "What the caller receives")
        note_cards(image, OUTCOME_NOTES, (96, 250, W - 96, 1000))
        return image

    if step_id == "recipe-1":
        image = slide_base(chapter_title, label, "The validation settings step")
        draw_code(image, (96, 240, W - 96, 660), "recipe.json", "json", RECIPE_LOCAL, types=TYPES, size=26)
        draw_code(image, (96, 700, W - 96, 1030), "recipe.json", "json", RECIPE_REMOTE, types=TYPES, size=26)
        return image

    if step_id == "recipe-2":
        image = slide_base(chapter_title, label, "A recipe is not validated on save")
        note_cards(image, RECIPE_NOTES, (96, 250, W - 96, 1000))
        return image

    raise SystemExit(f"No slide for {step_id}")
