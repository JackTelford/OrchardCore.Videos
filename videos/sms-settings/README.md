# SMS: Configure a Provider and Send a Test Message

**YouTube:** not published yet · **Length:** 3:52 · **Feature:** [OrchardCore.Sms](https://docs.orchardcore.net/en/latest/reference/modules/Sms/), with [OrchardCore.Sms.Azure](https://docs.orchardcore.net/en/latest/reference/modules/Sms.Azure/)

The video shows how an Orchard Core site sends text messages: what a provider is, how the default provider is chosen,
how to send a test message, and how to set up the Twilio and Azure Communication Services providers. It is for site
owners and administrators, so it stays in the admin and shows no code. The YouTube title, description, tags and links
are in the video's metadata file.

## Recording it again

The video is recorded on a **fresh site** with made-up data only, so it needs no mask script. The made-up phone
numbers, account identifier, token and connection string are listed in the recording plan, and a recording never
sends a message to a real provider.

- **Site.** A fresh Orchard Core site provisioned by Auto Setup with the Blog recipe, the site name Orchard Core, the
  time zone America/Los_Angeles and the user admin. It must run in the Development environment, because the Log
  provider that the video relies on exists only there. The SMS and Azure Communication Services SMS features start
  disabled and are enabled on camera, and the Secrets feature stays disabled. The site's process gets a proxy
  address that goes nowhere, so that the Twilio test message can only fail.
- **Order.** The clips depend on each other and are recorded in the order they play: enabling the SMS feature,
  choosing the default provider, sending the test message, configuring Twilio, then configuring Azure Communication
  Services. Four of them change the site, so a snapshot of the site is restored before a new take of any of them.
- **Details.** The recording plan has every action, every value to type, what must be on screen at the end of each
  step, the snapshots to take, and the checks for private data. Its section on the site holds the environment, the
  Auto Setup settings and the made-up values.
