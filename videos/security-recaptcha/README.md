# reCAPTCHA: Keep Bots Out of Sign-In, Registration and Forms

**YouTube:** not published yet · **Length:** 4:30 · **Feature:** OrchardCore.ReCaptcha ([ReCaptcha documentation](https://docs.orchardcore.net/en/latest/reference/modules/ReCaptcha/))

This video shows an administrator how to add a reCAPTCHA checkbox to an Orchard Core site. It turns on the ReCaptcha and
ReCaptcha Users features, saves the Site Key and the Secret Key in Settings, Security, reCaptcha, and shows the checkbox on
the Log in, Register and Forgot password pages. It then adds the Re Captcha widget to a form, and explains the Validate
ReCaptcha workflow task that checks the answer. The title, description, tags and chapters YouTube shows are kept in this
folder with the other files of the video.

## Recording it again

The video is recorded on a **fresh site** with fake data only, so it needs no mask script. The exact steps, locators and
values are in recording-plan.md. In short:

1. Run a local Orchard Core site provisioned by Auto Setup: the Blog recipe, the site name **Orchard Core**, the time zone
   America/Los_Angeles, and the user admin (admin@example.com). Unlike the other videos, this one needs the site to reach
   Google's reCAPTCHA service, so do not block outbound calls for it.
2. Before recording, enable Users Registration, Users Reset Password and Forms. Allow password reset in Settings, Security,
   Reset Password. Create a Contact page with a Form widget that holds a Name input and a Send button. Do **not** enable
   ReCaptcha or ReCaptcha Users: the first clip does it on camera. Sign the recorder in with the password chosen outside git,
   stop the site, and copy its data folder aside (snapshot A).
3. Narrate, then record the clips in the order they play: the features, the keys, the Log in page, the registration and
   password pages, the form. The features clip and the keys clip change the site, so restore the matching snapshot before
   another take of either of them. The form clip publishes the Contact page, so restore the snapshot saved after the keys clip
   before another take of it. The Log in clip and the registration and password pages clip are recorded signed out, in a
   browser without the saved sign-in; the other clips use it. The keys are the test keys that Google publishes, listed in the
   plan.
4. Read OPEN-QUESTIONS.md before recording: it lists what to confirm on the running site, such as what the Log in page shows
   after a refused sign-in and how the Re Captcha widget is labeled in the widget list.
