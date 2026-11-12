# Menus: Build the Navigation of Your Site

**YouTube:** not published yet · **Length:** 4:59 · **Feature:** [OrchardCore.Menu](https://docs.orchardcore.net/en/latest/reference/modules/Menu/)

This video is for the people who manage a site. It shows how the Menu module builds the navigation that visitors use: where
the menus are, how to open one, add a link, put an item inside another one, change the order, publish the menu so that
visitors see it, and create a second menu. The title, description, tags and chapters that YouTube shows are kept with
the video. Related pages: [Menu](https://docs.orchardcore.net/en/latest/reference/modules/Menu/) and
[Navigation](https://docs.orchardcore.net/en/latest/topics/navigation/).

## Chapters

1. Menus and menu items (slides): what a menu is made of, the three types of menu items, and how a theme shows a menu.
2. Open a menu: the front end, then Content, Menus, then the editor of the Main Menu.
3. Add a menu item: a Link Menu Item named Browse.
4. Nest and reorder: Categories inside Browse, Browse dragged above About, Save Draft, and the confirmation of Delete.
5. Publish a menu: the site before and after, and the badges Published and Draft.
6. Create a menu: Footer Menu, and why it does not show on the site.

## Recording it again

The video is recorded on a fresh site with fake data only, so it needs no mask script.

1. Build the web project of Orchard Core and run it on port 5010 with its data folder outside both repositories, set up
   by Auto Setup with the Blog recipe, the site name "Orchard Core", the time zone America/Los_Angeles, and the user
   admin with the email admin@example.com. The password is chosen outside git and passed to the recorder; it is never
   written in this repository.
2. Sign the recorder in once, so that the sign-in is saved outside git. No seed data is needed: the Blog recipe creates the
   Main Menu with Home and About, and the pages that the added links point to.
3. Stop the site and copy its data folder aside. Restore that copy before a full take.
4. Narrate, then record the clips in this order: open, add, arrange, publish, create. Each clip starts from the state the
   previous one left: add leaves a draft with Browse, arrange adds Categories and the changed order to that draft, publish
   publishes it, and create adds Footer Menu. Copy the data folder aside after add, after arrange and after publish, so
   that a single clip can be recorded again from the copy before it.

The exact pages, actions, waits, commands and sources of every label are in the recording plan. Points that a person
confirms on a running site are in the open questions.
