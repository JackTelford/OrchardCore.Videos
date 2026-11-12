# AI Quick Start: Build Orchard Core Sites With a Coding Agent

**YouTube:** not published yet · **Length:** 5:34 · **Feature:** `guide:getting-started/ai-quick-start` ([AI Quick Start](https://docs.orchardcore.net/en/latest/getting-started/ai-quick-start/))

This video shows how to connect an AI assistant to the Orchard Core MCP server, hosted at `ai.orchardcore.net/mcp`, in
GitHub Copilot (the CLI, github.com and Visual Studio Code), Claude Code, Claude Desktop, Cursor and any other client
that supports remote HTTP MCP servers. It then covers AI Skills, a community project that the documentation links,
and what to ask an assistant to build, with the context to give it and the review that stays with you. The title,
description, tags and chapters YouTube shows are in `video.json`.

## Recording it again

The video has **no browser recording**: its terminal sessions and configuration files are slides, and the opening and
closing cards are drawn by the scripts. There is no site to run, no Auto Setup, no seed data, no sign-in and no
`Recorder` run, so no mask script is needed. The words and the code on screen come from the documentation page
`src/docs/getting-started/ai-quick-start.md`, except where the page disagrees with the server or with the client
vendors' documentation (the address of the server, the tools it offers, the Copilot configuration files, Visual Studio
Code, Claude Desktop): `recording-plan.md` and `OPEN-QUESTIONS.md` say which steps, and what to confirm.

1. Read `OPEN-QUESTIONS.md` and confirm on current clients the steps it lists, then check the pronunciation fixes of
   the storyboard's `spoken` map with `scripts/pronounce.py` (the list is in the same file).
2. Narrate and assemble. There are no clips to record, so the order is the storyboard's:

   ```bash
   python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/ai-quick-start
   python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/ai-quick-start
   ```

3. Extract a frame of each slide and read it: the only address on screen is `ai.orchardcore.net/mcp`, and no slide
   shows a user name, a path, an account or a token.
