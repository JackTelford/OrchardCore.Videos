# Recording plan: Customizing encoding settings

This video has **no recordings**. Every chapter between the opening and closing cards is made of slides, and the
storyboard has no "clip" step, so nobody opens a browser, runs a site, or signs in. The page source that the video
discusses is shown on slides, because a rendered page looks the same with either setting: only its source differs,
and the recorder cannot show a page source.

Nothing here needs a person to decide anything: the video is built from `storyboard.json` and `slides.py` alone.

## The site it needs

None. There is no Auto Setup recipe, tenant, time zone, feature to enable, setting to save, external service, local
stand-in or seed data. No `state.json` is needed, and the recorder is not run for this video.

## The order of the clips

There are no clips to record, so there is no order and no snapshot to restore. The slide clips are drawn by
`slides.py` when the video is assembled, in the order of the storyboard: `01-why`, `02-config`, `03-inject`.

## Clips that change the site

None.

## Building the video

1. Narrate (`scripts/narrate.py videos/encoding-settings`). The storyboard's `spoken` map says how the voice reads the
   identifiers. A person confirms each spelling first (see `OPEN-QUESTIONS.md`).
2. Assemble (`scripts/assemble.py videos/encoding-settings`). It calls `slide()` of `slides.py` for every step below,
   and puts the cards and the slides together.

## The slides

Each slide shows what its narration describes. The code of the slides comes from the documentation page
`src/docs/guides/encoding-settings/README.md` and from the Orchard Core source, trimmed with `// ...`.

### Chapter 1, Why text is encoded (`01-why`)

- `why-1`: the heading "The encoders of ASP.NET Core" and four cards: HtmlEncoder, UrlEncoder, JavaScriptEncoder, and
  the default safe list (Basic Latin only, which is ASCII). All four are bright.
- `why-2`: two single-line code panels. "The text to write" holds the title `Café Münster` in an `h1` element. "Page
  source, default encoders" holds the same title with each accented letter written as a character code. Two lines of
  text under them say that each accented letter becomes a character code and that the browser shows the same text.
- `why-3`: the heading "One setting reaches the rendering" and three cards: the web encoders are registered once for
  the host (`src/OrchardCore/OrchardCore/Modules/Extensions/ServiceCollectionExtensions.cs:154`), Liquid templates and
  the shape renderer ask for an HtmlEncoder (`src/OrchardCore/OrchardCore.DisplayManagement.Liquid/LiquidViewTemplate.cs:71`,
  `LiquidShapeTemplateViewEngine.cs:39`, `src/OrchardCore/OrchardCore.DisplayManagement/ShapeHtmlRenderer.cs:26`),
  and WebEncoderOptions decides which characters stay as they are.

### Chapter 2, Allow the ranges you use (`02-config`)

- `config-1`: a Program.cs panel (the host of `src/OrchardCore.Cms.Web/Program.cs`, trimmed, with the `Configure`
  call of the documentation page). The three `using` lines and the four lines of the `Configure` call are bright, the
  other lines are dimmed.
- `config-2`: the heading "One call for three encoders" and four cards: UnicodeRanges.All (the whole Basic
  Multilingual Plane), and HtmlEncoder, UrlEncoder and JavaScriptEncoder, which use the options of WebEncoderOptions.
- `config-3`: the heading "Allow only what you use" and two cards: the default is cautious (ASP.NET Core documents it
  as protection against browser bugs), and the code names the ranges the text needs instead of all of them.
- `config-4`: a Program.cs panel with the `Configure` call that lists `UnicodeRanges.BasicLatin` and
  `UnicodeRanges.Latin1Supplement`, and two note cards: BasicLatin (the ASCII range) and Latin1Supplement (letters
  such as é and ü).
- `config-5`: the heading "One setting for the host" and three cards: the Program.cs of the web application holds the
  call; singleton services of the host are shared across tenant containers
  (`src/OrchardCore/OrchardCore/Shell/Builders/Extensions/ServiceProviderExtensions.cs:44`, used by
  `ShellContainerFactory.cs:45`); every tenant uses the encoders of the host.
- `config-6`: two single-line code panels side by side. "Default encoders" shows the title with character codes,
  "BasicLatin and Latin1Supplement" shows the title as it is written. Two lines of text under them say that the browser
  shows the same text either way and that the source is shorter and more readable.

### Chapter 3, Encoders in your code (`03-inject`)

- `inject-1`: the `MyService` class of the documentation page (a constructor that takes an HtmlEncoder and keeps it in
  a field), and two note cards: the encoder comes from the container, and the configured ranges apply to it.
- `inject-2`: the `TitleMetaWeblogDriver` class of the Title module, trimmed
  (`src/OrchardCore.Modules/OrchardCore.Title/RemotePublishing/TitleMetaWeblogDriver.cs:12-26`): the HtmlEncoder
  field, the constructor, and the call to `_encoder.Encode(titlePart.Title)`.
- `inject-3`: the heading "Static or injected" and two cards: `HtmlEncoder.Default` always uses the default safe list
  (Basic Latin only), and an injected HtmlEncoder follows the ranges you configured.
- `inject-4`: the `GoogleAnalyticsFilter` class of the Google module, trimmed
  (`src/OrchardCore.Modules/OrchardCore.Google/Analytics/GoogleAnalyticsFilter.cs:12-48`): the JavaScriptEncoder and
  UrlEncoder fields and constructor parameters, and the `RegisterHeadScript` call that encodes the tracking ID with
  each of them.

The opening card (`intro`) and the closing card (`outro`) show the title "Customizing encoding settings" and their
subtitles. They have no recording either.

## Checks for private data

The video holds no capture of a site, so no real person, account, address, credential or token can appear on screen.
Before publishing, still check:

- The only invented text is the title `Café Münster`. It names a fictional place and is not a real person or business.
- No slide shows a real tracking ID: the Google Analytics slide holds only the code, never a value.
- No slide, caption or description holds a password, a token, a real email address or a domain other than
  `example.com`, `orchardcore.net` and `docs.orchardcore.net`.
- The rendered slides contain no path of a machine, and no user name.
