# Documentation issues: docs ReCaptcha

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| reCaptcha | docs ReCaptcha | A challenge is shown after a threshold of attempts per IP address | The challenge is shown at once on sign-in, registration and password recovery when keys are set and the ReCaptcha Users feature is enabled, with no attempt threshold | ReCaptchaLoginFormDisplayDriver.cs:21, LoginFormEventEventHandler.cs:30 | secure-your-site |
