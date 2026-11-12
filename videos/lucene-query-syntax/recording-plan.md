# Recording plan: Lucene Query Syntax

This video is a developer video. Its slides carry the code; the four recorded clips show one browser screen, the
public search page of a blog, while queries are typed into its search box. Nothing is recorded in the admin.
Labels and selectors come from the Orchard Core source (`OrchardCMS/OrchardCore`, at or after the commit named in
`catalog/SOURCE.md`); paths below are relative to its `src` folder.

## The site it needs

| What | Value |
| --- | --- |
| Site | `OrchardCore.Cms.Web` of an Orchard Core checkout, on `http://localhost:5010`, with `ORCHARD_APP_DATA` pointing at an empty folder outside both repositories |
| Auto Setup | `OrchardCore__AutoSetup__Tenants__0__ShellName=Default`, `...__SiteName=Orchard Core`, `...__SiteTimeZone=America/Los_Angeles`, `...__AdminUsername=admin`, `...__AdminEmail=admin@example.com`, `...__AdminPassword=<password>` (the value of `VIDEO_PASSWORD`, chosen outside git), `...__DatabaseProvider=Sqlite`, `...__RecipeName=Blog` |
| Tenant, time zone | The `Default` tenant, `America/Los_Angeles` |
| Outbound calls | None are needed. Set `HTTP_PROXY` and `HTTPS_PROXY` to a dead address as `references/recording.md` says |
| External services | None |
| Theme | `TheBlogTheme`, which the Blog setup recipe enables. The clips depend on its search templates (`OrchardCore.Themes/TheBlogTheme/Views/Search-Form.liquid`, `Search-Results.liquid`, `Content-BlogPost.Summary.liquid`) |
| Features | The Blog recipe does not enable search. The recipe **Blog - Lucene Search** enables `OrchardCore.Search` and `OrchardCore.Lucene`, creates the index **Search**, makes it the default index, rebuilds it, and lets the Anonymous role query it (`OrchardCore.Themes/TheBlogTheme/Recipes/blog.lucene.search.recipe.json:3, 13-104`). Do not enable anything else, and do not change any setting by hand |

### Preparing the site (outside the capture)

1. Start the site and open `/` once, so Auto Setup runs. Run `Recorder setup`, so `state.json` exists.
2. Create the six fake blog posts. In the admin, open **Tools** > **Deployments** > **JSON Import**
   (`OrchardCore.Modules/OrchardCore.Deployment/AdminMenu.cs:30`), paste the JSON below into the box labeled **JSON**,
   and click **Import** (`OrchardCore.Modules/OrchardCore.Deployment/Views/Import/Json.cshtml:15, 21`). Every post is
   published. Import them before the index exists: the index is built from them in the next step.
3. Open **Tools** > **Recipes** (`OrchardCore.Modules/OrchardCore.Recipes/AdminMenu.cs:19`). The group
   of the theme's recipes has the theme's name as its header, **The Blog Theme** (`OrchardCore.Themes/TheBlogTheme/Manifest.cs:5`); the search box above
   the list filters by name. Find **Blog - Lucene Search** and click its **Run** button, then **Yes**
   (`OrchardCore.Modules/OrchardCore.Recipes/Views/RecipeEntry.Buttons.SummaryAdmin.cshtml:18`).
4. Wait until `http://localhost:5010/search?terms=quince` lists two posts: **Apple Varieties for Cider** and
   **Pruning Quince Trees**. If it still lists nothing after one minute, open **Search** > **Indexes**, open the actions
   menu of the **Search** index, click **Rebuild**, then **Yes**
   (`OrchardCore.Modules/OrchardCore.Indexing/Views/IndexProfile.ActionsMenuItems.SummaryAdmin.cshtml:25-32`), and
   reload the search.
5. Check every query of the table below once, in a normal browser window, before recording. If a result differs, stop
   and read `OPEN-QUESTIONS.md`: the narration describes these results.
6. Stop the site, and copy its `App_Data` folder and `state.json` aside. Restore that copy before a full take.

The seed (the recipe the JSON Import box takes; every name, title and sentence is invented):

```json
{
  "steps": [
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "4pqz8c7xw2m5n1r9t6v3k0h8jd",
          "ContentType": "BlogPost",
          "DisplayText": "Pruning Quince Trees",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "CreatedUtc": "2026-01-12T09:00:00Z",
          "ModifiedUtc": "2026-01-12T09:00:00Z",
          "PublishedUtc": "2026-01-12T09:00:00Z",
          "TitlePart": {
            "Title": "Pruning Quince Trees"
          },
          "MarkdownBodyPart": {
            "Markdown": "Quince trees need light pruning in late winter. Cut out crossing branches and keep the center open so the fruit gets sun."
          },
          "AutoroutePart": {
            "Path": "blog/pruning-quince-trees"
          },
          "BlogPost": {
            "Subtitle": {
              "Text": "A short guide for late winter"
            }
          }
        },
        {
          "ContentItemId": "7hd3k9f2s6b1x8m4w0q5n2e7ra",
          "ContentType": "BlogPost",
          "DisplayText": "Pressing Cider at Home",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "CreatedUtc": "2026-02-03T09:00:00Z",
          "ModifiedUtc": "2026-02-03T09:00:00Z",
          "PublishedUtc": "2026-02-03T09:00:00Z",
          "TitlePart": {
            "Title": "Pressing Cider at Home"
          },
          "MarkdownBodyPart": {
            "Markdown": "Cider starts with ripe apples and a clean press. Crush the apples, press the juice, and let it ferment for two weeks."
          },
          "AutoroutePart": {
            "Path": "blog/pressing-cider-at-home"
          },
          "BlogPost": {
            "Subtitle": {
              "Text": "From the first crush to the first glass"
            }
          }
        },
        {
          "ContentItemId": "2wn6t0j8c4v7z1p5g3y9k2b6ud",
          "ContentType": "BlogPost",
          "DisplayText": "Apple Jelly Recipe",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "CreatedUtc": "2026-02-24T09:00:00Z",
          "ModifiedUtc": "2026-02-24T09:00:00Z",
          "PublishedUtc": "2026-02-24T09:00:00Z",
          "TitlePart": {
            "Title": "Apple Jelly Recipe"
          },
          "MarkdownBodyPart": {
            "Markdown": "Apple jelly is firm and golden. Simmer chopped apples with water, strain the juice, and boil it with sugar until it sets."
          },
          "AutoroutePart": {
            "Path": "blog/apple-jelly-recipe"
          },
          "BlogPost": {
            "Subtitle": {
              "Text": "Golden and firm"
            }
          }
        },
        {
          "ContentItemId": "9ks1m5q7e3a0r8d2h6x4v1n9tf",
          "ContentType": "BlogPost",
          "DisplayText": "Apple Varieties for Cider",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "CreatedUtc": "2026-03-17T09:00:00Z",
          "ModifiedUtc": "2026-03-17T09:00:00Z",
          "PublishedUtc": "2026-03-17T09:00:00Z",
          "TitlePart": {
            "Title": "Apple Varieties for Cider"
          },
          "MarkdownBodyPart": {
            "Markdown": "Sharp and sweet apples balance a cider. Mix several varieties, and add a few quince for aroma."
          },
          "AutoroutePart": {
            "Path": "blog/apple-varieties-for-cider"
          },
          "BlogPost": {
            "Subtitle": {
              "Text": "Mixing sharp and sweet"
            }
          }
        },
        {
          "ContentItemId": "5bx0d4h8y2n6u1c7w3z9q5j4ma",
          "ContentType": "BlogPost",
          "DisplayText": "Planting Fruit Trees in Autumn",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "CreatedUtc": "2026-04-07T09:00:00Z",
          "ModifiedUtc": "2026-04-07T09:00:00Z",
          "PublishedUtc": "2026-04-07T09:00:00Z",
          "TitlePart": {
            "Title": "Planting Fruit Trees in Autumn"
          },
          "MarkdownBodyPart": {
            "Markdown": "Autumn is a good time to plant fruit trees. Dig a wide hole, water well, and stake the tree against wind."
          },
          "AutoroutePart": {
            "Path": "blog/planting-fruit-trees-in-autumn"
          },
          "BlogPost": {
            "Subtitle": {
              "Text": "Choosing the right spot"
            }
          }
        },
        {
          "ContentItemId": "3rv8g2l6p0t4e9s1f5k7a3c8wh",
          "ContentType": "BlogPost",
          "DisplayText": "Winter Pruning Checklist",
          "Latest": true,
          "Published": true,
          "Author": "admin",
          "CreatedUtc": "2026-05-12T09:00:00Z",
          "ModifiedUtc": "2026-05-12T09:00:00Z",
          "PublishedUtc": "2026-05-12T09:00:00Z",
          "TitlePart": {
            "Title": "Winter Pruning Checklist"
          },
          "MarkdownBodyPart": {
            "Markdown": "Winter pruning keeps fruit trees healthy. Remove dead wood first, then thin the crown. Clean your saw between trees."
          },
          "AutoroutePart": {
            "Path": "blog/winter-pruning-checklist"
          },
          "BlogPost": {
            "Subtitle": {
              "Text": "Before the buds open"
            }
          }
        }
      ]
    }
  ]
}
```

### The results the narration describes

Each clip types the query into the search box of `/search` and presses Enter. The page lists the posts below, in any
order (do not describe an order). The search page shows no count: the posts are counted on screen.

| Step | Query typed | Posts listed |
| --- | --- | --- |
| `op-1` | `quince cider` | Apple Varieties for Cider; Pressing Cider at Home; Pruning Quince Trees |
| `op-2` | `quince AND cider` | Apple Varieties for Cider |
| `op-3` | `quince and cider` | the same three posts as `op-1` |
| `op-4` | `+quince cider` | Apple Varieties for Cider; Pruning Quince Trees |
| `op-5` | `quince NOT pruning` | Apple Varieties for Cider |
| `phrase-1` | `"fruit trees"` | Planting Fruit Trees in Autumn; Winter Pruning Checklist |
| `phrase-2` | `"pruning trees"~2` | Pruning Quince Trees; Winter Pruning Checklist |
| `wild-1` | `qu?nce` | Apple Varieties for Cider; Pruning Quince Trees |
| `wild-2` | `prun*` | Pruning Quince Trees; Winter Pruning Checklist |
| `wild-3` | `*ince` | none: the page shows **There are no such results.** |
| `wild-4` | `quinse~` | Apple Varieties for Cider; Pruning Quince Trees |
| `wild-5` | `/pr.*ing/` | Pressing Cider at Home; Pruning Quince Trees; Winter Pruning Checklist |
| `fields-2` | `Content.ContentItem.DisplayText.Analyzed:quince` | Pruning Quince Trees |
| `fields-3` | `Content.ContentItem.DisplayText.Analyzed:(fruit AND trees)` | Planting Fruit Trees in Autumn |
| `fields-4` | `Content.ContentItem.DisplayText.Normalized:[p TO r]` | Planting Fruit Trees in Autumn; Pressing Cider at Home; Pruning Quince Trees |

The items that the blog recipe itself creates (**Man must explore, and this is exploration at its greatest**, **About**,
**Blog**, **Main Menu**, **Footer**) match none of these queries. A result list that holds one of them is a mistake in
the seed or in the index: fix it before recording.

## Order of the clips, and what changes the site

The four clips only search. None changes the site, so they can be recorded in any order and again at any time, from
the same snapshot. The slide clips (`01-parser`, `05-names`, `06-saved`, `07-habits`) and the cards are not recorded.

Recording order, which is also the playing order: `02-operators`, `03-phrases`, `04-wildcards`, `05-fields`.

## What every recorded clip has in common

- Start page: `/search`, opened with `scene.OpenAsync("/search")` before the capture. It shows the theme's header, the
  search box (`Search-Form.liquid:16`; its placeholder **Enter your search term(s)** is set at `Search-Form.liquid:9`, since
  the recipe sets no placeholder in the search settings) and its **Search** button (`Search-Form.liquid:17`), and no results.
- A helper, used by every step that types a query (the selector stands for `scene.Page.Locator("input[name='Terms']")`):
  1. `FillAsync("")` on `input[name='Terms']`, which clears the previous query without a pause (not visible).
  2. `scene.TypeAsync(input, <query>)`, which types it visibly.
  3. `scene.WaitForNavigationAsync(() => scene.Page.Keyboard.PressAsync("Enter"))`, which submits the form: the page
     reloads at its top, with the query still in the box (`value="{{ Model.Terms }}"`, `Search-Form.liquid:16`).
  4. Wait until a result is listed: `h2.post-title` is visible (`Content-BlogPost.Summary.liquid:3`), or, when the
     query lists nothing, `.alert-warning` is visible (`Search-Results.liquid:23`).
  5. `scene.ScrollAsync(0, 420)`, so that the search box stays visible at the top and the results are below it. 420 is
     the starting value. Accept it when, in a still frame, the box shows the whole query and the titles of the largest
     list of the clip are all visible; otherwise change the offset by steps of 40 and use the same value in every clip.
- The actions of a step are shorter than its narration (the longest query takes about four seconds to type).
- Frames at the end of a step must show no error page, no empty box and no real name.

## Clip 02-operators (chapter 02)

Starts on `/search`. Steps, in order:

- `op-1`: use the helper with `quince cider`. When the narration ends, the box shows `quince cider` and three posts
  are listed: Apple Varieties for Cider, Pressing Cider at Home, Pruning Quince Trees. Source of the search box:
  `Search-Form.liquid:16`; of the post titles: `Content-BlogPost.Summary.liquid:3-5`.
- `op-2`: helper with `quince AND cider` (capital letters). When the narration ends, one post is listed: Apple
  Varieties for Cider.
- `op-3`: helper with `quince and cider` (lowercase). When the narration ends, the same three posts as `op-1` are
  listed.
- `op-4`: helper with `+quince cider`. When the narration ends, two posts are listed: Apple Varieties for Cider and
  Pruning Quince Trees.
- `op-5`: helper with `quince NOT pruning`. When the narration ends, one post is listed: Apple Varieties for Cider.

## Clip 03-phrases (chapter 03)

Starts on `/search`. Steps:

- `phrase-1`: helper with `"fruit trees"` (typed with the quotation marks). When the narration ends, two posts are
  listed: Planting Fruit Trees in Autumn and Winter Pruning Checklist.
- `phrase-2`: helper with `"pruning trees"~2`. When the narration ends, two posts are listed: Pruning Quince Trees and
  Winter Pruning Checklist.

## Clip 04-wildcards (chapter 04)

Starts on `/search`. Steps:

- `wild-1`: helper with `qu?nce`. When the narration ends, two posts are listed: Apple Varieties for Cider and
  Pruning Quince Trees.
- `wild-2`: helper with `prun*`. When the narration ends, two posts are listed: Pruning Quince Trees and Winter
  Pruning Checklist.
- `wild-3`: helper with `*ince`. When the narration ends, the box shows `*ince` and the yellow alert **There are no such
  results.** is visible, with no post below it (`Search-Results.liquid:23`). The page shows no error: the parse error
  is only logged (`OrchardCore/OrchardCore.Lucene.Core/Services/LuceneSearchService.cs:79-82`).
- `wild-4`: helper with `quinse~`. When the narration ends, two posts are listed: Apple Varieties for Cider and
  Pruning Quince Trees.
- `wild-5`: helper with `/pr.*ing/`. When the narration ends, three posts are listed: Pressing Cider at Home, Pruning
  Quince Trees and Winter Pruning Checklist.

## Clip 05-fields (chapter 05)

Starts on `/search`. The field names come from `OrchardCore/OrchardCore.Contents.Core/Indexing/ContentIndexingConstants.cs:29, 31`
and are written by `OrchardCore.Modules/OrchardCore.Contents/Indexing/AspectsContentIndexHandler.cs:35, 47`. Steps:

- `fields-2`: helper with `Content.ContentItem.DisplayText.Analyzed:quince`. When the narration ends, the whole query
  is typed in the box (the longest queries of the video are in this clip: if the start of the text scrolls out of the
  box while the end stays visible, that is accepted; never shorten the query, never change the browser zoom), and one
  post is listed: Pruning Quince Trees.
- `fields-3`: helper with `Content.ContentItem.DisplayText.Analyzed:(fruit AND trees)`. When the narration ends, one
  post is listed: Planting Fruit Trees in Autumn.
- `fields-4`: helper with `Content.ContentItem.DisplayText.Normalized:[p TO r]`. When the narration ends, three posts
  are listed: Planting Fruit Trees in Autumn, Pressing Cider at Home and Pruning Quince Trees.

## Slides (nothing to record; `slides.py` draws them)

- `parse-1`: the parts of `LuceneSearchService.SearchAsync` that read the index's default query settings, build the
  `MultiFieldQueryParser`, parse the text and log a `ParseException`
  (`OrchardCore/OrchardCore.Lucene.Core/Services/LuceneSearchService.cs:47-52, 70-82`).
- `parse-2`: three cards: default fields (`LuceneSearchService.cs:47-60`; the field is `ContentIndexingConstants.FullTextKey`),
  the standard analyzer (`OrchardCore.Modules/OrchardCore.Lucene/Startup.cs:47-48`, the only analyzer the module registers), and parse errors (`LuceneSearchService.cs:79-82`).
- `fields-1`: cards with the field names `Content.ContentItem.DisplayText.Analyzed`, `Content.ContentItem.FullText`,
  and `BlogPost.Subtitle` (`OrchardCore/OrchardCore.Indexing.Abstractions/ContentFieldIndexHandler.cs:38, 43`: a field is indexed
  under the content type and under the part name, each followed by the field name, and both are `BlogPost` here; only when the
  field is included, `OrchardCore.Modules/OrchardCore.Contents/Indexing/ContentItemIndexCoordinator.cs:75-78`; and the box
  **Include this element in the index**, `OrchardCore.Modules/OrchardCore.Lucene/Views/LuceneContentIndexSettings.Edit.cshtml:13`);
  a dimmed fourth card names `Content.ContentItem.DisplayText.Normalized`, which `fields-4` uses.
- `saved-1`: the `query_string` example of the Lucene module's documentation, and `QueryStringQueryProvider.CreateQuery`
  (`OrchardCore/OrchardCore.Lucene.Core/QueryProviders/QueryStringQueryProvider.cs:16-20`). The page **Run Lucene Query** is
  under **Search** > **Queries** (`OrchardCore.Modules/OrchardCore.Lucene/AdminMenu.cs:20`).
- `saved-2`: two cards: **Match all** (`*:*`) and **Parse errors**, where **Run Lucene Query** shows an error that starts
  with **Invalid query:** and continues with the parser's message (`OrchardCore.Modules/OrchardCore.Lucene/Controllers/AdminController.cs:199`).
- `habits-1`: four cards: capital letters, brackets for ranges, a space before a minus sign, and no leading wildcard.

## Cards

`intro` and `outro` are drawn by the assembler; there is nothing to record.

## Checks for private data

- The only person-like text on screen is the user name **admin** in the post summaries (**Posted by admin**). The seed
  holds no real name, email address or phone number.
- Search the contact sheet of every clip, and an OCR of the video at one frame per second and at every scene change,
  for a real name, an email address other than `admin@example.com`, a phone number or a token. The site is fresh, so
  no mask script is needed.
- Confirm that no frame shows the password, the Auto Setup values or the address bar.
