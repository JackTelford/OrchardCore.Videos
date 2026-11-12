# Open questions: liquid-basics

What the author could not verify from the Orchard Core source (commit `92db4a83`) or from its documentation. The
narration says nothing about these points beyond what is written in the first column.

## 1. Parser options

The narration names `AllowParentheses`, `AllowLiquidTag` and `AllowTrailingQuestionMark` (step `parser-2`) and says
that they are read from the `Liquid` section of the configuration, under `OrchardCore`, and are off by default. The
code that reads them is `src/OrchardCore/OrchardCore.DisplayManagement.Liquid/Extensions/LiquidCoreServices.cs:22-38`
(`GetSectionCompat("Liquid", "OrchardCore_Liquid")` and `GetValue<bool>`, so a missing value is `false`). What each
option does is the description in the configuration schema of the same project
(`src/OrchardCore/OrchardCore.DisplayManagement.Liquid/ConfigurationSchema.json`): parentheses are allowed in
templates, the `liquid` tag is allowed, and an identifier can end with a question mark, which is stripped during
parsing. The narration says no more than that.

- The parsing rules themselves (where a parenthesis is accepted, what the trailing question mark is for) are defined by
  the Fluid package, which is not part of the source tree, so the video does not describe them. A person who reads the
  documentation of Fluid can add a sentence to `parser-2` and to the cards of `slides.py`.
- The release notes name the key `AllowLiquidTags` (with an `s`) under `OrchardCore_Liquid`
  (`src/docs/releases/3.0.0.md`). The code reads `AllowLiquidTag`. On a running site, confirm that
  `OrchardCore:Liquid:AllowLiquidTag` set to `true` makes a template that uses the `liquid` tag render (the video
  shows this key).

## 2. Pronunciation (`spoken` in `storyboard.json`)

`scripts/pronounce.py` writes audio, so it was not run. A person confirms these spellings with it before narrating,
and also listens for the names that have no entry:

| Written | Spoken as | Check |
| --- | --- | --- |
| `.liquid` | dot liquid | "files with the dot liquid extension" |
| `href` | H ref | other readings: "hurf", "H-R-E-F" |
| ` utc` (with a leading space, so that it never matches inside a word such as "outcome") | U.T.C. | "local, U.T.C. and json" |
| `CreatedUtc` | Created U.T.C. | |
| `sanitize_html`, `absolute_url`, `display_url` | sanitize HTML, absolute URL, display URL | the voice reads HTML and URL as letters |
| `display_text`, `has_permission`, `is_in_role`, `user_email` | the words without underscores | |
| `antiforgerytoken` | anti-forgery token | "the anti-forgery token tag writes the hidden field" |
| `appsettings` | app settings | |
| `DisplayText`, `ContentItem`, `ContentType`, `SiteName`, `PageSize`, `BlogPost` | the words with a space: Display Text, Content Item, Content Type, Site Name, Page Size, Blog Post | the voice may read the joined name as one word |
| `TrackingConsent`, `HttpContext` | Tracking Consent, HTTP Context | "HTTP" is read as letters |
| `AllowParentheses`, `AllowLiquidTag`, `AllowTrailingQuestionMark` | Allow Parentheses, Allow Liquid Tag, Allow Trailing Question Mark | |
| `Autoroute` | Auto route | other reading: "au-to-root" |

The captions keep the written form. Names with no entry that the voice may misread: the single letter `t` in "the t
filter" (other reading: "tee"); `slugify`, `markdownify`, `upcase`; the lower-case `json`; and `nil`.

## 3. The blog post page on a running site (clip `03-page`)

The steps `page-1` to `page-4` describe the page from the templates, not from a running site.

- That the Blog home page shows the post as a link whose text is the title (`Content-BlogPost.Summary.liquid`),
  so that the click of `page-1` works. The plan has a fallback (`NavigateAsync("/blog/post-1")`).
- That the navigation bar, the title, the subtitle and the Posted by line are all visible at 1536x864 without
  scrolling. The plan scrolls only when the Posted by line is below the visible area.
- That a signed-out visitor sees no extra control on the page. The template renders `Model.Content.ContentsMetadata`,
  which may show controls to a signed-in administrator; that is why the plan records the clip signed out.
- That the Posted by line reads "Posted by admin on", followed by a date in the format of the template. The author is
  set by the recipe (`blog.recipe.json:1006`); the date comes from a shape that this video does not describe.

## 4. Behavior of Liquid that comes from Fluid

The standard filters (`date`, `upcase`, `default`) and the standard tags (`assign`, `if`, `for`, `capture`) are Fluid's.
The narration names them from the documentation ("All the default filters that are available in the standard Liquid
syntax are available in OrchardCore") and from the list of the editor
(`src/OrchardCore.Modules/OrchardCore.Liquid/Assets/monaco/liquid-intellisense.ts`). The slides show
`date: "%c"` as the documentation does. The output of that format on a running site was not checked.

## 5. The `Content` object

Step `objects-4` says that Contents adds `Content`, which loads content items by identifier or handle. This is read
from `src/OrchardCore.Modules/OrchardCore.Contents/Startup.cs:73-120` (`ContentItemId`, `ContentItemVersionId`,
`Latest` and a lookup by handle). The documentation of the Liquid module does not describe the object, so the
syntax of a call is not shown. A person can add it after trying it on a running site.

## 6. The slides as images

The slides were rendered in memory and measured for overflow, but never saved or looked at: the programme forbids
saving images here. Look at each slide once after assembling, especially the code panels of `syntax-1`, `syntax-3`,
`tags-1` and `parser-2`. Their Liquid expressions use the plain text color, since `brand.py` has no Liquid lexer.
