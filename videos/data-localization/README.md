# Dynamic Translations: Translate Type Names and Other Site Data

**YouTube:** not published yet · **Length:** 5:01 · **Feature:** [OrchardCore.DataLocalization](https://docs.orchardcore.net/en/latest/reference/modules/DataLocalization/)

This video is for site owners and administrators. It shows why some text of a multilingual site, such as content type
names, field names, custom admin menu items and some permission descriptions, cannot be translated with PO files, and how the
Data Localization feature translates it: enabling the feature, the translation editor, translating a content type, a
field and an admin menu item, seeing the result in the admin, following the progress with the statistics pages, the
permissions that control who translates, and the deployment step that moves translations to another site. The title,
description, tags and chapters YouTube shows are in the metadata file of this folder.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script. The file recording-plan.md in
this folder holds every value, label and wait condition; this is the outline.

1. **The site.** A fresh Orchard Core site from the Blog recipe, provisioned by Auto Setup (site name Orchard Core, time
   zone America/Los_Angeles, user admin with the address admin@example.com and a password chosen outside git), run on
   port 5010 with its data folder outside both repositories. No PO files are installed.
2. **Before recording.** Enable the Localization and Admin Culture Picker features, set the cultures (English
   (United States) as the default, French (France) and Spanish (Spain) as supported) and make the content type Blog
   Post creatable, so that the New menu of Manage Content lists it. All three are done outside the capture, with the
   JSON Import page. Data Localization stays disabled: the first clip enables it on camera. Sign the recorder in
   once. Copy the data folder and the sign-in aside as snapshot A.
3. **Clip order.** Record the Features clip first. Then, outside the capture, seed the translations (French complete
   except three strings, Spanish with three, English empty) and take snapshot B. Record the editor clip and the
   translate clip from snapshot B; the translate clip saves three French translations, so take snapshot C after it.
   Record the switch clip and the statistics clip from snapshot C, because they need those three translations.
   Restore the snapshot a clip starts from before every further take of it.
4. **Slides.** The chapters on why dynamic translations exist, on permissions and on moving translations are slides, drawn
   from the storyboard: there is nothing to record for them.
5. **Voice.** The narration says PO files with the spelling P.O. files, set in the storyboard. Check it with a sample
   from the pronunciation script before narrating the whole video.
