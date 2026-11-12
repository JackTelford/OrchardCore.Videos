# Open questions: Write a Recipe Step

## 1. How the voice reads the names of the code

The narration spells every identifier as separate words, as the worked example does ("Update Async"). The `spoken` map
of the storyboard is empty. Nobody has listened to the voice yet, because the narration was not generated. A person
confirms each of these with `scripts/pronounce.py` before the video is narrated for good, and adds an entry to
`spoken` for any that the voice misreads (a key that is part of another key goes after the longer one):

- "I Recipe Step Handler" (the letter I, then three words), for `IRecipeStepHandler`.
- "Named Recipe Step Handler", "Execute Async", "Handle Async", "To Object", "Try Get Property Value",
  "Add Recipe Execution Step".
- "Startup" and "JSON".
