# Search Media Files: Index PDF, Office and Text Documents

**YouTube:** not published yet · **Length:** 4:20 · **Feature:** [OrchardCore.Media.Indexing](https://docs.orchardcore.net/en/latest/reference/modules/Media/)

The Media Indexing features add the text of files attached to a content item to a search index, so site search finds the
content item by words that only its files hold. The video explains what is read (PDF files, Word and PowerPoint files,
text and Markdown files) and what must be in place, enables the three features, includes a Media Field in the index,
attaches a PDF to an article, resets the index and chooses the fields to query, and searches the site for a word that only
the PDF holds. The title, description, tags and chapters YouTube shows are in video.json.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. Everything a person needs, step by
step, is in recording-plan.md; this is the short version.

1. **Site.** A local Orchard Core site provisioned by Auto Setup with the Blog recipe, site name Orchard Core, time zone
   America/Los_Angeles, user admin with the email admin@example.com, and a password chosen outside git. The Lucene and
   Search features are enabled before recording. The three Media Indexing features are not: the first clip enables them.
2. **Seed.** A fake one-page PDF named garden-planting-guide.pdf, uploaded to the Media Library, whose text contains the
   word persimmon and appears nowhere else on the site. A Lucene index called Articles for the Article content type.
   Settings, Search, Site Search with Articles as the default search index. The recording plan gives the exact text of the
   PDF and the exact steps. The PDF is not committed.
3. **Snapshots.** The site changes in every recording clip, so copy its data folder aside after the seed and after each clip,
   and restore the one before a clip before recording that clip again.
4. **Clip order.** Record the clips in the order they play in, because each one needs the state the one before it left: the
   features, then the field, then the article, then the index, then the search. The two slide chapters need no site.
5. **Narrate first, then record.** The recorder holds each step for the length of its narration. Two spoken spellings (the
   name OpenXML and the two field name endings) have to be confirmed by a person before narrating; OPEN-QUESTIONS.md says how.
