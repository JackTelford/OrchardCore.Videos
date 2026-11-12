# Documentation issues: releases 3.0.0

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Liquid parser options | releases/3.0.0.md | Lines 26-31 show `{ "OrchardCore_Liquid": { "AllowLiquidTags": true } }` to re-enable the liquid tag. | The option is `AllowLiquidTag` (no 's'), read from the `Liquid` section under OrchardCore. `OrchardCore_Liquid` is only a deprecated fallback. A key spelled `AllowLiquidTags` is ignored. | src/OrchardCore/OrchardCore.DisplayManagement.Liquid/Extensions/LiquidCoreServices.cs:25,30 | liquid-basics |
| Known Networks & Proxies setting | releases/3.0.0.md | The Reverse Proxy module has a setting called Known Networks & Proxies, for trusted networks and proxies. | The admin page has no field for known networks or known proxies. They can only be set through the Settings recipe step or the configuration (ConfigureReverseProxySettings()). | src/OrchardCore.Modules/OrchardCore.ReverseProxy/Views/ReverseProxySettings.Edit.cshtml; src/OrchardCore.Modules/OrchardCore.ReverseProxy/Configuration/ForwardedHeadersOptionsConfiguration.cs:24-32 | security-reverse-proxy |
