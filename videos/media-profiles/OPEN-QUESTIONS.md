# Open questions: media profiles

What the author could not see on a running site, and what a person must confirm before the narration is recorded.
Paths are in the Orchard Core checkout (`src/OrchardCore.Modules/OrchardCore.Media` unless another path is given).

## 1. The padded header (steps `site-4`, `site-5`, `site-6`)

**Claimed in the narration:** after `banner` is changed to the Pad resize mode with a background color, the photo sits in
the middle of the header, with the color on both sides, on the home page and on a blog post. The narration does not say
"the whole photo", because the header may cut part of it (point 4 below).

**What was checked in the source:**

- The Blog theme asks for the profile when the item has an Image: `Views/Content-Blog.liquid:9`,
  `Content-BlogPost.liquid:9`, `Content-Article.liquid:9` (all in `src/OrchardCore.Themes/TheBlogTheme`). The recipe gives
  the three items an image: `Recipes/blog.recipe.json:983`, `:1024` and `:1081`.
- Pad fits the photo inside the width and height without enlarging it, then centers it on a canvas of the full size filled
  with the color: `Processing/VipsImageProcessingEngine.cs:163-211`. For `home-bg.jpg` (1900 by 1267 pixels) and a 2048 by
  600 box, that gives a photo about 900 pixels wide with about 574 pixels of color on each side.
- The header shows its image with `background-size: cover` under a 50% dark overlay:
  `Assets/TheBlogTheme/dist/css/styles.css:11120-11139` in the same theme.

**To confirm on a running site:**

1. After saving `banner` as Pad with `336699`, a reload of `/` and of `/blog/post-1` shows the color bands at once. The
   Blog recipe enables Dynamic Cache (`blog.recipe.json:37`); the theme templates use no cache tag, so no stale header is
   expected, but this is not proven.
2. The bands can be told from the photo under the dark overlay. If not, choose another code of six digits (digits only,
   so the voice reads it from the `spoken` map) and change `site-4`, the `spoken` entry and the plan together.
3. The first request for the new header image is slow enough to need the off-camera wait of the plan.
4. The header draws its image with `background-size: cover`, so a 2048 by 600 image is scaled until it covers the whole
   header and the overflow is cut (`Assets/TheBlogTheme/dist/css/styles.css:11120-11129`). The header height depends on
   the title and the window width, so at a wide window the top and bottom of the photo may be cut by a few percent, and at
   a narrow one the outer part of each color band. Confirm at the recorder's window size that the photo and a band on each
   side are visible.

## 2. Resize modes (steps `modes-1`, `modes-2`, `create-4`)

The form lists seven modes: Default (Max), Max, Crop, Pad, BoxPad, Min and Stretch
(`Controllers/MediaProfilesController.cs:362-368`). The narration describes Max, Crop, Pad and Stretch only.

- **Min**: the engine handles it exactly like Max (`Processing/VipsImageProcessingEngine.cs:68`), although the form's own
  translator note and the documentation (`src/docs/reference/modules/Media/README.md:94`) describe something else.
- **BoxPad**: the engine fits the photo inside the box without enlarging it, then takes the larger of the box and the fitted
  photo as the canvas (`VipsImageProcessingEngine.cs:167-184`). The fitted photo is never larger than the box, so the canvas
  is the box and BoxPad gives the same result as Pad in every case where both width and height are set.
- **Crop**: with no focal point, a picture smaller than the box is shrunk only, never enlarged
  (`VipsImageProcessingEngine.cs:115-123`, `Size.Down`), so it may not fill the box. With a focal point, as the Blog theme
  passes for the home page and the blog post (`Views/Content-Blog.liquid:9`, anchors in `blog.recipe.json:990-995` and `:1031-1036`), the
  picture is scaled to cover the box, even when that enlarges it, and then cut (`:126-160`). That is why the narration says
  "a large picture" fills the size in Crop. What a picture smaller than the box gives with no focal point is not verified
  from the source (it depends on libvips).

Confirm Min and BoxPad on a running site (one photo, a 600 by 240 box). If the maintainers settle what Min and BoxPad should
do, the video may describe them.

## 3. Statements that come from a form hint or from the code only

- `create-5`: "at 100 the default quality applies" follows the hint (`Views/MediaProfiles/Create.cshtml:99`) and the code.
  The service sends a quality command only between 1 and 99 (`Services/MediaProfileService.cs:44`), and without one the
  engine uses its own quality of 85 (`VipsImageProcessingEngine.cs:262-266`). The narration does not say "full quality",
  and does not give the number 85. To confirm: request the banner image with and without a quality of 100 and compare the
  file sizes. The quality only applies to Jpg and WebP output (`:268-274`); a Png or Gif ignores it.
- `create-6`: Auto Orient is described by its hint only (`Create.cshtml:109`). Nothing in the video shows a rotated photo.
- `manage-2`: "A theme that still asks for the old name gets none of the profile's settings" follows from
  `Services/MediaProfileService.cs`: an unknown name returns no commands, so the image address carries none of the
  profile's settings. Other arguments of the theme's filter, such as the focal point, still apply
  (`Filters/ResizeUrlFilter.cs:43-55`). Confirm by renaming `banner` to another name on a throwaway copy and reloading `/`
  (the header then shows the original photo). Do this on a copy: the video itself does not rename anything.
- `create-2`: "saves it in lowercase" is `Services/MediaProfilesManager.cs:32`. Whether a theme that asks for `Banner`
  finds `banner` is not claimed.
- `create-3`: the Custom Size box holds `0` when it appears (`media-profile-editor.js` sets it to `0` when the list
  changes to another value, and the model default is `0`). The plan empties it before typing.
- `list-1`: the Administrator role is the only one that gets the Manage Media Profiles permission by default
  (`PermissionProvider.cs:31-40`; the Editor, Author and Contributor roles do not). To confirm: sign in as an editor
  and look for Media, Profiles.
- `manage-1`: pressing Enter in the search box runs the filter. The first submit control of the form is the hidden
  filter button (`Views/MediaProfiles/Index.cshtml:11`). Confirm that the list narrows after Enter.
- `manage-3`: the confirmation dialog and the message "Media profile deleted successfully." come from
  `src/OrchardCore.Themes/TheAdmin/Views/Layout.cshtml:99` and `Controllers/MediaProfilesController.cs:313`. The message
  may have faded when the narration ends.

## 4. Background Color formats

The hint of the form (`Create.cshtml:77`, `Edit.cshtml:66`) and the documentation list `white`, `ffff00`, `ffff0080`,
`128,64,32` and a four-number form as valid values. The default engine reads only a code of three or six hexadecimal
digits, with an optional `#`, and turns anything else into white (`VipsImageProcessingEngine.cs:213-234`). The narration
says only "a color code, such as 336699". Confirm what `ffff0080` gives on a running site. The two forms also disagree
with each other: the four-number example ends in `80` on the Create form and in `16` on the Edit form and in the
documentation.

## 5. Pronunciation, to check with `scripts/pronounce.py` before narrating

The storyboard maps `336699` to "three three six six nine nine". These written forms are left as they are and need a
listen: "Jpg", "WebP", "BoxPad", "EXIF", "2048 by 600" and "Hint". Add a `spoken` entry only after a person confirms it.

## 6. The photos on the pages (the home page and the blog post)

Both photos come with the Blog theme (`src/OrchardCore.Themes/TheBlogTheme/wwwroot/assets/img`). The home page photo
(`home-bg.jpg`) is a desk seen from above, with no person. The blog post photo (`post-bg.jpg`) shows an astronaut in a
spacesuit next to a solar panel. The face is hidden behind the visor, and a mission patch is on the sleeve. The author
found no face and no readable name, but the programme's rule is that fake data holds no real person, so the maintainer
must accept this photo or name a replacement (a landscape photo of about 1900 by 1267 pixels, uploaded in Media, Library
under the name `post-bg.jpg`). The video shows this photo in `site-2` and `site-6`, cut to the middle strip by the profile.
