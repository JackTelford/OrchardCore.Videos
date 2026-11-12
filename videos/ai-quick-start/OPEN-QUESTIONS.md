# Open questions: AI Quick Start

The video is written from one documentation page, `src/docs/getting-started/ai-quick-start.md`. It is a guide about
third-party tools, so there is no Orchard Core code to check a label or a step against. The review checked the server
itself (a request to it, and the list of its tools) and each client step against the vendor's own documentation, and
where they disagree with the page the video follows them. What follows is what that check could not settle, and what a
person must confirm before the voice is generated or the video is published.

## 1. The address of the server

The documentation page gives `https://ai.orchardcore.net` as the address to register in every client
(`ai-quick-start.md:11-17`). A request to it does not reach the MCP server: an MCP `initialize` request posted to
`https://ai.orchardcore.net` returned status 400 and the HTML page "Orchard Core AI Hub", and the same request posted to
`https://ai.orchardcore.net/mcp` returned status 200, a `text/event-stream` answer and the server name
"Orchard Core MCP Server" (protocol version 2025-03-26, no sign-in asked). The home page of the server gives the
`/mcp` address in its Copilot CLI and Claude Code commands. So the narration, the slides and `video.json` use
`https://ai.orchardcore.net/mcp`, and the page is listed as a documentation issue.

A person must still register the server in Claude Code with `claude mcp add --transport http orchardcore
https://ai.orchardcore.net/mcp` and look at `/mcp` in a session to see that it connects, because no client could be run
here. The home page names the server `orchard-core` where the page and the video use `orchardcore`; the name is free
text in every client, so `orchardcore` stays.

## 2. What the server offers

The page says the server gives "tool-based access to the Orchard Core documentation and APIs"
(`ai-quick-start.md:5`) and that its tools "search and read" the documentation (`ai-quick-start.md:9`). The list of
tools the server returned (`tools/list`) has three tools, all searches: `tool_instance_search_orchardcore_docs`
("Search the Official Orchard Core Documentations"), `tool_instance_search_orchardcore_gallery` (the Gallery of
community modules and themes) and `tool_instance_search_crestapps_orchardcore_docs` (the CrestApps documentation). It
has no tool that reads a page, and none for APIs. The narration says only that the tools search the Orchard Core
documentation, and names no tool. The list can change: confirm it on the server at the time of recording, and decide
whether the video should mention the Gallery.

## 3. Pronunciation

Each entry of the storyboard's `spoken` map is a new spelling, and none could be heard here. Check each one with
`scripts/pronounce.py` (the map is a case-sensitive substring replacement applied in the order of the file; the key
`ai.orchardcore.net/mcp` is the only place the narration writes the address, and `.mcp.json` the only place it writes
a file name):

| Written | Spoken as | Where |
|---|---|---|
| `MCP` | M.C.P. | `intro`, `why-2`, `mcp-1`, `mcp-2`, `copilot-3`, `cursor-1`, `other-1`, `skills-1`, `skills-3`, `ask-5`, `outro` |
| `/mcp add` | slash M.C.P. add | `copilot-2` |
| `claude mcp add` | Claude M.C.P. add | `claude-2` |
| `mcpServers` | M.C.P. Servers | `copilot-4` |
| `.mcp.json` | dot M.C.P. dot JSON | `copilot-4` |
| `npm` | N.P.M. | `copilot-1`, `claude-1` |
| `CLI` | C.L.I. | `copilot-1`, `copilot-2`, `copilot-4`, `skills-2` |
| `ai.orchardcore.net/mcp` | A.I. dot orchardcore dot net slash M.C.P. | `mcp-2` |
| `AutoroutePart` | Autoroute Part | `ask-2` |
| `MarkdownBodyPart` | Markdown Body Part | `ask-2` |

Words left to the voice, to listen to: HTTP, JSON, Node.js, github.com, orchardcore (the server name, one word),
copilot and claude (the commands), "tools key", "asterisk" and "Tools or Connectors".

## 4. Steps of the third-party clients

The Orchard Core documentation page is the source of the client steps except the ones marked "changed", where the page
disagrees with the vendor's documentation and the video follows the vendor. None was run on a client. Third-party
screens change, so a person must confirm each one on a current client before the video is published:

- GitHub Copilot CLI (changed): the `/mcp add` form of GitHub's documentation has the fields Server Name, Server Type
  (STDIO, HTTP or SSE), URL, HTTP Headers and Tools (default `*`), and saves with Ctrl+S; the page's wording is "choose
  an HTTP server, name it, use the URL" (`ai-quick-start.md:38`). The file `mcp-config.json` is in `~/.copilot/` and its
  top-level key is `mcpServers`. The page's entry has no `tools` key, GitHub's examples of the file all carry
  `"tools": ["*"]`, and GitHub does not say whether an entry without it is accepted, so the slide and the narration
  include it. Confirm that the entry of the slide connects, and that the form has the labels the slide shows.
- GitHub Copilot on github.com (changed): the page says Settings, Copilot, Coding agent, MCP configuration, in the
  repository or organization settings, for Copilot Chat and the Copilot coding agent, and shows no `tools` key
  (`ai-quick-start.md:53`). GitHub's page "Configure MCP servers for your repository" says: on the repository, Settings,
  then Copilot, then MCP servers (under "Code, planning, and automation"), then the "MCP configuration" section and the
  button "Save MCP configuration"; the entry needs `type`, `url` and `tools`; the configuration is shared by the Copilot
  cloud agent and Copilot code review. The page does not describe an organization-level page or Copilot Chat. GitHub
  calls the agent "Copilot cloud agent" where the Orchard Core page says "Copilot coding agent", so the narration says
  "cloud agent". Confirm on a repository the labels of the path, that this entry is accepted, and whether the name
  of the agent has changed again.
- Visual Studio Code (changed): the page puts the entry in `.vscode/mcp.json` under the top-level key `servers`, and
  tells you to open the Chat view in "Agent" mode and look at the available tools (`ai-quick-start.md:70-83`). The
  Visual Studio Code page "MCP configuration reference" says that file is the editor-managed format of the Local
  harness, "deprecated and will be removed in February 2027", and that new Copilot configurations use a top-level
  `mcpServers` object in a workspace-root `.mcp.json` (or the Copilot user `mcp-config.json`); the setting that offers
  `.mcp.json` in the guided flow is experimental, and existing `.mcp.json` files "remain available" when it is off. The
  page "Agent tools" does not use the words "Agent mode": it says to select "Open Customizations" in the title bar of
  the Chat view and to use the Tools tab. The video follows these pages. Confirm on a current Visual Studio Code that a
  hand-made `.mcp.json` at the root of a workspace is read without changing any setting, that the title bar has
  "Open Customizations", and that the Tools tab lists the tools of the server. If not, `copilot-4`, `copilot-5` and
  their slides change.
- Claude Code (changed): `claude mcp add --transport http orchardcore https://ai.orchardcore.net/mcp` matches the
  vendor's syntax (`claude mcp add --transport http <name> <url>`). Anthropic's page "Advanced setup" recommends a
  native installer and lists the npm package `@anthropic-ai/claude-code` as an alternative that requires Node.js 22 or
  later; the page and the video show the npm route, and the narration adds that it requires Node.js. The page says a
  missing command means Claude Code is not installed (`ai-quick-start.md:87`); the vendor says it can also be a
  directory missing from the PATH. Decide whether the video should show the native installer.
- Claude Desktop (changed): the page puts a server with `"type": "http"` and a `url` in `claude_desktop_config.json`
  (`ai-quick-start.md:109-122`). The Model Context Protocol documentation page "Connect to remote MCP Servers" sends a
  remote server through Settings, Connectors, the Add button, Add custom connector, and a dialog that asks for the
  server URL; the narration and the slide follow it. Confirm on a running Claude Desktop that the labels are spelled so,
  that an address with the path `/mcp` is accepted, and that the plan of the signed-in account allows a custom
  connector.
- Cursor: `.cursor/mcp.json` (or the global one in the home directory) with only a `url`
  (`ai-quick-start.md:126-136`). Cursor's documentation shows both files and the `mcpServers` object with a `url`.

## 5. The community project

The page names the project of the AI Skills section (`ai-quick-start.md:144-149`) and links its repository. The
narration, the slides and `video.json` call it "a community project" and link nothing to it, because the programme
allows only a few domains and asks for invented companies in its data. Decide whether the video may name the
project, and whether `video.json` may link its repository.

The page writes "Claude CLI" in that section (`ai-quick-start.md:149`) where the section on Claude Code calls the tool
Claude Code. The project's repository gives `claude plugin` commands for Claude Code and `copilot plugin` commands for
the GitHub Copilot CLI, so the narration and the slide say "Claude Code". The repository does not say whether the
project is official or maintained by the community; the narration repeats the page's note that it is a community
project and not an official part of Orchard Core.
