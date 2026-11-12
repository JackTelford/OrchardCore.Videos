# Documentation issues: releases 3.0.0.md (Liquid Tag Disabled by Default); src

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.Liquid.Core | releases/3.0.0.md (Liquid Tag Disabled by Default); src/docs/reference/modules/Liquid/README.md | To re-enable the liquid tag, set AllowLiquidTags to true in the OrchardCore_Liquid section. The Liquid module page documents none of the parser options. | The parser options are read from the Liquid section (OrchardCore_Liquid is a deprecated fallback) with the keys AllowParentheses, AllowLiquidTag and AllowTrailingQuestionMark, all false by default. The key is AllowLiquidTag, without an s. | src/OrchardCore/OrchardCore.DisplayManagement.Liquid/Extensions/LiquidCoreServices.cs:25-37 | liquid-basics |
