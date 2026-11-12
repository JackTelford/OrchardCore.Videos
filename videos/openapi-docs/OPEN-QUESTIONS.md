# Open questions: openapi-docs

Everything in the narration was read from the Orchard Core source (`OrchardCore.OpenApi`, `OrchardCore.OpenApi.Core`,
the Features, Recipes, Settings and OpenId modules) or from `src/docs/reference/modules/OpenApi/README.md`. The points
below could not be confirmed without a running site. A person confirms them while rehearsing the recording
(`recording-plan.md`, "Provisioning", step 3) and changes the video if one is wrong.

## Behavior that needs a running site

1. **"Try it out" returns 200.** `swagger-3` relies on the whole chain: the silent sign-in (`openapi-ui-auth.ts`), the
   OpenID Connect server with its default keys, token validation for the `Default` tenant, the `roles` claim in the token,
   and the permissions of `GET api/content/{contentItemId}` (`AccessContentApi`, which the Administrator role has by
   default, `OrchardCore.Contents/Permissions.cs:46`, and `ViewContent` on the item). Nothing here was run. If the
   response is 401 or 403, fix the site, not the narration, and keep the step as it is. The `Api` scheme only forwards
   to the scheme named in `ApiAuthorizationOptions` (`Bearer` by default,
   `OrchardCore.Infrastructure.Abstractions/Security/ApiAuthorizationOptions.cs:7`, handler in
   `OrchardCore.Infrastructure/Security/ApiAuthenticationHandler.cs`), so `access-2` says that endpoints on the `Api`
   scheme take bearer tokens, and does not say that every API of the site does: the Media API uses the admin cookie by
   default (`OrchardCore.Media/MediaApiSettings.cs`).
2. **No visible redirect on the first load.** The recorder opens a fresh browser context with only the cookies of
   `state.json`, so `sessionStorage` is empty. The bundle redirects to the OpenID server only for `login_required`,
   `consent_required`, `interaction_required` or `account_selection_required` (`openapi-ui-auth.ts:63-68`). The recipe
   uses implicit consent and the admin has a cookie session, so none is expected. If the page does redirect on
   camera, rehearse again and record only after the silent sign-in works.
3. **The redirect address.** `OpenIdApplicationSettings.cs:237-253` builds absolute URIs from the Redirect Uris field. Confirm
   that OpenIddict accepts `http://localhost:5010/OrchardCore.OpenApi/openapi-oidc-silent.html` for the public client when
   the application is saved, and that the silent page loads in the hidden frame (the site may send a frame-blocking
   header). The module's own recipe uses `https://localhost:5001`, which is why the plan edits the application.
4. **Tags in Swagger UI.** `swagger-1` says "grouped by tag" (a Swagger UI behavior, not Orchard Core code). The tag
   names listed in the plan (`CreateEndpoint`, `DeleteEndpoint`, `GetEndpoint`, `GetIntellisenseEndpoint`, `MediaApi`,
   `QueryApi`) are inferred from the client classes NSwag generated from the schema
   (`OrchardCore.OpenApi/Services/OpenApiClient.cs`), and that schema was generated on a tenant with more features
   than a Blog site (Lucene, Elasticsearch, Tenants). The Blog site's own list was not read from a running site: the
   features it enables expose the Contents endpoints (`OrchardCore.Contents/Startup.cs:214`), the Media endpoints
   (`.WithTags("MediaApi")`), `QueryApiController` and the Liquid script endpoint (`OrchardCore.Liquid/Startup.cs:34`),
   and the document may list others. Confirm the real tags and that `GET /api/content/{contentItemId}` is listed. The
   narration does not name a tag.
5. **Third-party labels and selectors.** "Try it out" is the label the documentation gives for Swagger UI, and the
   narration says "execute the request" for its **Execute** button. The selectors in the plan for Swagger UI
   (`.opblock-get`, `.opblock-summary-path[data-path]`, `button.try-out__btn`, `button.execute`,
   `.live-responses-table`, `button.authorization__btn.locked`), ReDoc and Scalar come from those libraries, not from
   Orchard Core. Probe the pages before writing the scenes.
6. **Scalar sends requests.** `scalar-1` says Scalar "sends requests from the page with a token that it acquires silently" (the
   documentation: `"Try it out" / "Send"`; `ScalarUIStartup` injects the bundle). The clip does not send a request.
7. **Recipes page.** The recipe is listed under the name of the first feature whose folder contains the recipe
   (`OrchardCore.Recipes/Controllers/AdminController.cs:81`). The module has four features, so the group heading is
   probably **OpenApi**; the plan does not depend on it (it searches for the recipe name).
8. **Waits.** Enabling features (`enable-2`) and saving the settings (`schema-3`) restart the tenant. How long the reload
   takes depends on the machine.

## Words the narration does not say as a fact

- **The permission label.** The narration uses the permission's name, `ViewOpenApiContent`, as the documentation does.
  The settings page calls it "View OpenAPI Content" (`OpenApiSettings.Edit.cshtml:49`), and the Roles screen should
  show its description, "Access view content endpoints" (`OrchardCore.OpenApi.Core/OpenApiPermissions.cs:13-17`). Open
  the role editor of Administrator once to see which one appears.
- **XML comments.** The documentation says XML documentation comments give richer Swagger descriptions. The module
  never calls `IncludeXmlComments` on its Swagger generator (`Startup.cs:32-69`), so the video does not say it.
- **Disabled explorers return 404.** Documentation only. The access gate runs for every path under `/swagger`, `/redoc`
  and `/scalar` (`Startup.cs:163-196`) whether or not the explorer is enabled; the video does not say what a disabled
  explorer returns.
- **The `/openapi` address and the second document.** The module registers two generators: `AddOpenApi` with
  `routes.MapOpenApi()` (`Startup.cs:27-30`, `205`) and Swashbuckle with `app.UseSwagger()` (`Startup.cs:32-69`, `206`).
  All three explorers read `/swagger/v1/swagger.json` (`Startup.cs:250-252`, `292-294`, `330`). The address of the
  `MapOpenApi` document is a framework default that is not in the Orchard Core source, so the video says "an OpenAPI
  document", names only `/swagger/v1/swagger.json`, and does not say that the module builds one document only.
- **The TypeScript client.** `own-3` names only the C# client. `OrchardCore.OpenApi.nswag:109` generates the C# client
  with `MultipleClientsFromFirstTagAndOperationId`, so `WithName("ApiGetContentItem")` becomes `ApiGetContentItemAsync`
  (`OpenApiClient.cs:589`); the TypeScript client (`OrchardCore.OpenApi.nswag:43`, `...AndOperationName`) is generated
  into `.scripts/bloom/services/OpenApiClient.ts`, which is not in the source that was read, so how it names its
  methods was not checked.
- **The locked Authorize button.** The plan waits for `button.authorization__btn.locked` in Swagger UI. The module
  calls `preauthorizeApiKey("Bearer", token)` (`openapi-ui-auth.ts:242`) on a security scheme declared as an HTTP bearer
  scheme (`Startup.cs:55-63`); whether Swagger UI marks such a scheme as authorized was not checked. The plan has a
  time-based fallback.

## Pronunciation (the voice is not run by this step)

The `spoken` map of `storyboard.json` adds: "Open API" for OpenApi and OpenAPI, "P.K.C.E." for PKCE, "N Swag" for
NSwag, "slash swagger", "slash redoc" and "slash scalar slash v1" for the three addresses, "slash api slash", "API category", "U.R.I.", and a
spaced reading of `ViewOpenApiContent`, `ApiController`, `EndpointName`, `WithName` and `ProducesResponseType`. A person
checks each with `scripts/pronounce.py` before narrating. "ReDoc", "Swagger" and "Scalar" are left as they are written;
add a spelling if the voice reads "ReDoc" badly.
