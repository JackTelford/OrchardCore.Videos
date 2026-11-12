# Microsoft Entra ID: Sign In With Work Accounts

**YouTube:** not published yet · **Length:** 5:33 · **Feature:** [OrchardCore.Microsoft.Authentication.AzureAD](https://docs.orchardcore.net/en/latest/reference/modules/Microsoft.Authentication/)

This video shows how people sign in to an Orchard Core site with their Microsoft Entra ID account. It explains how the
sign-in works, what the app registration in the Azure portal provides, and who a tenant ID allows to sign in. It then
enables the feature, enters Display Name, AppId and TenantId, explains CallbackPath and Store tokens, shows the button on
the login page, and explains how the site matches the person to a user. The last chapter points to the
[documentation guide](https://docs.orchardcore.net/en/latest/guides/microsoft-entra-id-integration/) for roles from
groups. The title, description, tags and chapters YouTube shows are in the video's metadata file.

## Recording it again

The video was recorded on a **fresh site** with fake data only, so it needs no mask script. The sign-in itself is not
recorded: the chapters about the Azure portal are slides, and the clip of the login page stops before the button is
selected. The exact commands, selectors and labels are in recording-plan.md, and what a person must still confirm on a
running site is in OPEN-QUESTIONS.md.

1. Run the Orchard Core web application on port 5010, with its own empty data folder outside both repositories,
   provisioned by Auto Setup: the Blog recipe, site name **Orchard Core**, time zone America/Los_Angeles, user admin,
   email admin@example.com, and a password chosen outside git. Block the site's outbound calls, so it never contacts an
   outside service.
2. Sign the recorder in once, so it saves the session of the admin user. The video needs no seed data and no preparation
   step. Stop the site, and copy its data folder and the saved session aside: this is the snapshot to restore before
   every full take.
3. Enable no feature beforehand. The first clip enables Microsoft Entra ID (Azure Active Directory) Authentication on
   camera, and External Authentication is enabled along with it.
4. Narrate the video, then record three clips in this order: enabling the feature, configuring the settings, and the
   login page. The second clip needs the first, and the third needs the second. The first two clips change the site, so
   restore the snapshot before another take of either. The third clip is recorded signed out and changes nothing. The
   chapters about how the sign-in works, the app registration, the tenant ID, the matching of users and the roles are
   slides, and are not recorded.
