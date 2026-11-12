# Documentation issues: releases 3.0.0.md (Liquid Tag Disabled by Default)

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| Parser options (AllowLiquidTag) | releases/3.0.0.md (Liquid Tag Disabled by Default) | Configure it in appsettings under `OrchardCore_Liquid` with the key `AllowLiquidTags`. | Reads `AllowLiquidTag` (no s) from the `Liquid` section, which is `OrchardCore:Liquid`. `OrchardCore_Liquid` is the deprecated name, and `AllowLiquidTags` is not read. | src/OrchardCore/OrchardCore.DisplayManagement.Liquid/Extensions/LiquidCoreServices.cs:22-38; ConfigurationSchema.json | liquid-basics |
