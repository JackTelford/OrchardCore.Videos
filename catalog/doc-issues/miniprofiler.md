# Documentation issues: MiniProfiler

Where the documentation and the code disagree. The code wins. See [the index](../DOC-ISSUES.md).

| Feature | Page | What the docs say | What the code does | Code (file:line) | Found by |
| --- | --- | --- | --- | --- | --- |
| OrchardCore.MiniProfiler (Enable the feature) | reference/modules/MiniProfiler/README.md | Enable the Mini Profiler feature from Configuration → Features. | The Features page is under Tools: the admin menu builds Tools &gt; Features. There is no Configuration &gt; Features entry. | src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27 | mini-profiler |
| OrchardCore.MiniProfiler (View profiling results) | reference/modules/MiniProfiler/README.md | Grant either permission to another role from Security → Roles. | The Roles page is under Access Control: the admin menu builds Access Control &gt; Roles. The Security menu does not hold Roles. | src/OrchardCore.Modules/OrchardCore.Roles/AdminMenu.cs:18-19 | mini-profiler |
| OrchardCore.MiniProfiler | reference/modules/MiniProfiler/README.md | Shape timings appear for shapes rendered through display management, without mentioning a limit. | ShapeStep stops timing shapes once the profiler head is 20 levels deep (MaxTimingDepth = 20). Deeper shapes keep their time in the closest timed parent, because MiniProfiler's System.Text.Json serialization fails past 64 nesting levels. The docs omit this. | src/OrchardCore.Modules/OrchardCore.MiniProfiler/ShapeStep.cs:9-11,31 | mini-profiler |
| OrchardCore.MiniProfiler | reference/modules/MiniProfiler/README.md:9 | Enable the Mini Profiler feature from Configuration &gt; Features. | The Features page is under Tools &gt; Features. | src/OrchardCore.Modules/OrchardCore.Features/AdminMenu.cs:26-27 | features-manage |
