# Open questions: Customizing encoding settings

What was checked in the source and in the documentation, and what a person must still confirm. Nothing here is stated
in the narration as a fact that the sources do not show; the items below are about the illustrations and the voice.

## What was verified, and where

- Orchard Core registers the web encoders for the host: `services.AddWebEncoders()` in `AddDefaultServices`
  (`src/OrchardCore/OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:154`).
- Liquid templates and the shape renderer take `HtmlEncoder` from dependency injection
  (`OrchardCore.DisplayManagement.Liquid/LiquidViewTemplate.cs:71`, `LiquidShapeTemplateViewEngine.cs:39`,
  `OrchardCore.DisplayManagement/ShapeHtmlRenderer.cs:26`).
- Tenants share the singleton services of the host: `ShellContainerFactory.cs:45` clones the host's services with
  `CreateChildContainer`, which resolves a singleton registered by a factory from the host container
  (`ServiceProviderExtensions.cs:44-47`). The host's service collection is the one registered at
  `ServiceCollectionExtensions.cs:111`.
- The web application of the repository is configured in `src/OrchardCore.Cms.Web/Program.cs` (no `Startup` class).
- The code of `inject-2` and `inject-4` is from `TitleMetaWeblogDriver.cs:12-26` and `GoogleAnalyticsFilter.cs:12-48`.
  The code of `inject-1` and `config-1` is from the documentation page, with the `Program.cs` context of the web
  application.
- The sources of Orchard Core do not show the behavior of ASP.NET Core itself. These facts come from the Microsoft
  documentation, in the articles "Prevent Cross-Site Scripting (XSS) in ASP.NET Core" (the sections "Access encoders
  in code" and "Customize the encoders") and the API pages of `WebEncoderOptions` and `UnicodeRanges`, and from the
  source of `AddWebEncoders` in the ASP.NET Core repository, read on the day the content was written:
  - the default safe list is Basic Latin, and everything outside it is written as a character code;
  - customizing the safe list affects only the encoders taken from dependency injection, not `*Encoder.Default`;
  - `WebEncoderOptions` holds the options common to the HTML, JavaScript and URL encoders;
  - `UnicodeRanges.All` is the entire Basic Multilingual Plane;
  - the safe list is limited to Basic Latin as protection against browser bugs;
  - the three encoders are registered as singletons, built from `TextEncoderSettings` when it is set.

## To confirm on a running site

1. **The character codes on the slides** (`why-2`, `config-6`). The slides show the title `Café Münster` as
   `Caf&#xE9; M&#xFC;nster` with the default encoders. The Microsoft article shows the same pattern for Chinese text
   (`&#x6C49;`), but the two codes here are derived from it. Build a scratch `OrchardCore.Cms.Web` with the Blog
   recipe, create a blog post titled `Café Münster`, open its page and view the page source: confirm that the title in
   the heading is written as `Caf&#xE9; M&#xFC;nster`. Then add the `Configure<WebEncoderOptions>` call of `config-4`
   to `Program.cs`, restart, and confirm that the source holds `Café Münster`. The element in the source depends on the
   theme (an `h1` is an illustration): adjust the slide if the theme writes the title elsewhere.
2. **The tenants** (`config-5`). The claim follows from the source above. With a second tenant, confirm that the
   accented title of that tenant is written as it is too.
3. **The code compiles.** Compile the Program.cs panel of `config-1` and `config-4` (with the three `using` lines) in a
   scratch web project that references Orchard Core, and the `MyService` panel of `inject-1`. The panels of `inject-2`
   and `inject-4` are trimmed from the source with `// ...` and are not meant to compile on their own.
4. **The voice.** The `spoken` map of `storyboard.json` was not run through `pronounce.py`, which writes audio. A person
   must confirm: "HTML Encoder", "HTML Encoder dot Default", "Web Encoder Options", "Text Encoder Settings", "Unicode
   Ranges dot All", "Latin 1 Supplement", "Basic Latin", "Program dot C.S.", and how the voice reads "Café Münster",
   "ASCII", "Multilingual" and "MetaWeblog".

## Left out on purpose

- Whether `UnicodeRanges.All` leaves the characters that HTML treats specially (such as angle brackets and ampersands)
  as they are, and what happens to characters outside the Basic Multilingual Plane. The narration says neither.
- Whether the call has an effect in the `Startup` class of a module. The narration only says where the call goes in the
  host application.
- Places where Orchard Core itself uses a static encoder (see the report of the documentation issues): the narration
  claims the injected encoder only for Liquid templates and the shape renderer.
