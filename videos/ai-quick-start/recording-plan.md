# Recording plan: AI Quick Start

The source of every statement and label of this video is one documentation page,
`src/docs/getting-started/ai-quick-start.md` of OrchardCMS/OrchardCore (cited below as `ai-quick-start.md:<line>`).
There is no Orchard Core code behind it, so there is no `file:line` in the Orchard Core source. Where the page
disagrees with the server it describes or with the documentation of a client's vendor, the video follows the server
and the vendor, and the step is marked "changed" below: the address and the tools of the server (`mcp-1`, `mcp-2`, and
every address on a slide), the tools key and the prompts of the Copilot CLI (`copilot-2`), the Copilot settings of a
repository on github.com (`copilot-3`), the file of Visual Studio Code and the place where its tools are listed
(`copilot-4`, `copilot-5`), the install of Claude Code (`claude-1`) and Claude Desktop (`claude-3`).
`OPEN-QUESTIONS.md` says what a person must confirm.

## Summary

This video has **no browser recording**. It is made of the opening card, eight chapters of slides drawn by `slides.py`,
and the closing card. Every step of the storyboard is a card or a slide. The terminal sessions, the editor
configuration files are slides (`draw_code`), as the programme decided for developer videos, the Claude Desktop
settings are a slide of cards, and the GitHub settings page needs a signed-in account, which is never recorded.

So there is nothing for the recorder to do, and none of the usual preparation applies:

- No site, no tenant, no Auto Setup recipe, no time zone, no features to enable, no settings to save.
- No external service and no local stand-in.
- No seed data to create.
- No `state.json`, no sign-in, no `VIDEO_PASSWORD`, no `VIDEO_BASE_URL`, no `Recorder` run (`setup`, `prepare` or
  `record`), and no snapshot of an `App_Data` folder.

## Order of the clips

There are no `clip` steps. The only order is the storyboard's: narrate, then assemble.

```bash
python .agents/skills/orchardcore-video-producer/scripts/narrate.py videos/ai-quick-start
python .agents/skills/orchardcore-video-producer/scripts/assemble.py videos/ai-quick-start
```

Before narrating, confirm each pronunciation fix of the storyboard's `spoken` map with
`.agents/skills/orchardcore-video-producer/scripts/pronounce.py` (the list is in `OPEN-QUESTIONS.md`).

## Clips that change the site

None. No clip touches a site, so no snapshot has to be restored before a new take.

## Slides: how to check them

`slides.py` draws every slide with the helpers of `brand.py` only, at 1920x1080. To check a slide without saving an
image, from the video's folder with the skill's `scripts` folder on `PYTHONPATH`, call
`slides.slide("<step id>", "<chapter title>", "Chapter 3")` for every slide step and look at exceptions and at the
size of the result (1920 by 1080). Run Python with `-B` so no `__pycache__` folder appears in the video's folder.

Code panels hold the commands and the configuration files of the documentation page, except where a step below is
marked "changed": the three panels of the Copilot family (`mcp-config.json`, the MCP configuration on github.com and
`.mcp.json`) carry the `tools` key that GitHub requires on github.com and shows in every example, and every address is
`https://ai.orchardcore.net/mcp`, the path at which the server answers (the page gives the address without the path).
No panel shows a machine's user name, a home folder, a prompt with a user name, an account, a token or a password.

## The cards

| Step | Card | What it shows |
|---|---|---|
| `intro` | Opening card, `00-intro` | The Orchard Core logo over "AI Quick Start", with the subtitle "Build Orchard Core sites with a coding agent". |
| `outro` | Closing card, `99-outro` | The same title, with the subtitle "docs.orchardcore.net". |

## The slides

One line per step, in the storyboard's order. A chapter has one slide per step, shown while the step is narrated.

### Chapter 1, `01-why`: Why connect an assistant

- `why-1`: "What an assistant can do": four cards, Scaffold modules, Write recipes, Generate migrations, Explain features,
  all bright. Source: `ai-quick-start.md:3`.
- `why-2`: "Two ways to ground an assistant": two cards, the first bright (The Orchard Core MCP server: tool-based
  access to the Orchard Core documentation), the second dimmed (AI Skills). Source: `ai-quick-start.md:5`.
- `why-3`: the same two cards, the second bright (AI Skills: repeatable, Orchard Core specific workflows), the first
  dimmed. Source: `ai-quick-start.md:142-144`.

### Chapter 2, `02-server`: The Orchard Core MCP server

- `mcp-1` (changed): "Model Context Protocol": three cards, An open standard, Tools for the documentation (the
  server searches it), Accurate answers. Source: `ai-quick-start.md:7-9`, except that the page says the tools "search
  and read" the documentation, where the server lists three tools that search (see `OPEN-QUESTIONS.md`).
- `mcp-2` (changed): "One address for every client": a panel titled "Orchard Core MCP server" with the address
  `https://ai.orchardcore.net/mcp`, and three note cards (Remote, HTTP, Tools). Source: `ai-quick-start.md:11-17`,
  except the path `/mcp`: the server answers an MCP request at that address and not at the address of the page.

### Chapter 3, `03-copilot`: GitHub Copilot

- `copilot-1`: "GitHub Copilot CLI": a Terminal panel with three lines, `npm install -g @github/copilot`, `copilot` and
  `/mcp add`, the first two bright and the third dimmed; note cards Requires (Node.js) and Starts with (the
  copilot command). Source: `ai-quick-start.md:23-36`.
- `copilot-2` (changed): "Register the server": the same Terminal panel with only `/mcp add` bright, above a panel
  titled `mcp-config.json` with the JSON that has the top-level key `mcpServers` and the `tools` key; note cards
  Server Name (orchardcore), Server Type (HTTP) and URL, the labels of the `/mcp add` form in GitHub's documentation.
  Source: `ai-quick-start.md:33-49`, and GitHub's page "Adding MCP servers" for the Copilot CLI (the form, and the
  examples of `mcp-config.json`, which carry `"tools": ["*"]`).
- `copilot-3` (changed): "Copilot on github.com": a panel titled "MCP configuration" with the same JSON; note cards
  Where (Settings, Copilot, MCP servers), Required key (tools, with * for every tool) and Used by (the cloud agent and
  code review). Source: GitHub's page "Configure MCP servers for your repository" (the path, the section
  "MCP configuration", the required `tools` key, the asterisk, and the sentence that the configuration is shared by the
  Copilot cloud agent and Copilot code review), not the Orchard Core page (`ai-quick-start.md:51-66`), which says
  repository or organization settings, Settings, Copilot, Coding agent, MCP configuration, Copilot Chat and the
  Copilot coding agent, and gives no `tools` key.
- `copilot-4` (changed): "Visual Studio Code": a panel titled `.mcp.json` with the same JSON (its second line, the
  top-level key `mcpServers`, is bright, the others dimmed); note cards Location (the root of your workspace) and
  Top-level key (mcpServers). Source: the Visual Studio Code page "MCP configuration reference" ("Use a top-level
  `mcpServers` object in workspace-root `.mcp.json` or Copilot user `mcp-config.json`", and `.vscode/mcp.json` with
  `servers` is the older format of the Local harness, deprecated). The Orchard Core page gives `.vscode/mcp.json`
  with `servers` (`ai-quick-start.md:68-81`).
- `copilot-5` (changed): "Check the connection": two cards, Open the Chat view (Select Open Customizations in the
  title bar) and Open the Tools tab (Confirm the tools of the orchardcore server are listed). Source: the Visual
  Studio Code page "Agent tools" ("In the Chat view, select Open Customizations in the title bar", then the Tools
  tab). The Orchard Core page says "Agent mode" and "the available tools" (`ai-quick-start.md:83`).

### Chapter 4, `04-claude`: Claude Code and Claude Desktop

- `claude-1` (changed): "Claude Code": a Terminal panel with `npm install -g @anthropic-ai/claude-code` and
  `claude --version`, above two cards (a terminal-based assistant; install it with npm, which requires Node.js).
  Source: `ai-quick-start.md:87-97`, and Anthropic's page "Advanced setup" for the Node.js requirement of the npm
  route (the page recommends a native installer and lists npm as an alternative).
- `claude-2` (changed): "Add the server": a Terminal panel with the single command
  `claude mcp add --transport http orchardcore https://ai.orchardcore.net/mcp`, above three cards, Transport (HTTP), Name
  (orchardcore) and Address. Source: `ai-quick-start.md:99-105`, except the path `/mcp` of the address.
- `claude-3` (changed): "Claude Desktop": three cards, Open Settings (then choose Connectors in the sidebar), Add a custom
  connector (select Add, then Add custom connector) and Enter the server address. Source: the Model Context Protocol
  documentation page "Connect to remote MCP Servers", not the Orchard Core page, which puts the server in
  `claude_desktop_config.json` (`ai-quick-start.md:107-122`); the Model Context Protocol documentation shows that
  file launching local servers with a command and arguments.

### Chapter 5, `05-clients`: Cursor and other clients

- `cursor-1`: "Cursor": a panel titled `.cursor/mcp.json` with the `mcpServers` JSON that has only the `url` key; note
  cards Project (`.cursor/mcp.json`) and Global (`~/.cursor/mcp.json`). Source: `ai-quick-start.md:124-136`.
- `other-1`: "Other clients": three cards, Find the setting (MCP, Model Context Protocol, or Tools/Connectors),
  Register an HTTP server, and Use the Orchard Core address. Source: `ai-quick-start.md:138-140`.

### Chapter 6, `06-skills`: AI Skills

- `skills-1`: "AI Skills": three cards, Repeatable workflows, Curated skills and plugins, A community project.
  Source: `ai-quick-start.md:142-144`.
- `skills-2`: "A community project": three cards, Maintained by the community (not an official part of Orchard Core),
  Plugins for two CLIs (Claude Code and the GitHub Copilot CLI), Installed from its repository.
  Source: `ai-quick-start.md:146-149` (the page writes "Claude CLI"; the tool is named Claude Code, as in chapter 4, and the repository of the project gives the install commands of its plugins for Claude Code and for the GitHub Copilot CLI).
- `skills-3`: "Use both together": two cards, The MCP server and AI Skills. Source: `ai-quick-start.md:151-152`.

### Chapter 7, `07-ask`: What to ask for

- `ask-1`: "What to ask for": three cards, Scaffold a module or theme, Write recipes and Model content; the first two
  bright, the third dimmed. Source: `ai-quick-start.md:156-159`.
- `ask-2`: the same three cards, the third bright, the first two dimmed. Source: `ai-quick-start.md:156` and `160`.
- `ask-3`: "Explain and extend": two cards, Explain features and Extend behavior. Source: `ai-quick-start.md:161-162`.
- `ask-4`: "Give the assistant context": three cards, Enabled modules, Content types, Target version.
  Source: `ai-quick-start.md:164-165`.
- `ask-5`: "You stay responsible": two cards, Review generated code and Review generated recipes. Source:
  `ai-quick-start.md:167-168`.

### Chapter 8, `08-next`: Next steps

- `next-1`: "In the documentation": three cards named like the pages of the "Next steps" list, Create a CMS Web
  application, Recipes and Starter Themes and Code Generation Templates (the names are those of `mkdocs.yml`
  lines 134, 137 and 138). Source: `ai-quick-start.md:170-174` and
  the navigation of `mkdocs.yml`.

## Checks for private data

The video holds no recording, so there is nothing to OCR for a leaked name, but check the result anyway:

- Extract a frame of each slide and card with ffmpeg and read them. The only address on screen is
  `ai.orchardcore.net/mcp` (and the documentation host `docs.orchardcore.net` on the closing card).
- No user name, home folder, machine name, IP address, email address, account name, token, API key or password is on
  any slide or in `video.json`.
- The terminal panels show no prompt with a user name or a path.
- No chat reply of an assistant is shown or invented: the video shows what to ask, not what an assistant answers.
- The captions (`.srt`, `.vtt`) are made from the narration and add nothing else.
