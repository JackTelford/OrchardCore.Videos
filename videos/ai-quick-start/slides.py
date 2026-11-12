"""The slides of the AI Quick Start video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from brand import W, bullets, draw_code, note_cards, slide_base

SERVER = "https://ai.orchardcore.net/mcp"

# Left column: code panels. Right column: note cards.
LEFT = 96
LEFT_END = 1130
RIGHT = 1180
RIGHT_END = W - 96

# The entry of the Copilot family of files: the Copilot CLI mcp-config.json, the MCP configuration of a repository on
# github.com, and a .mcp.json file. The tools key is required on github.com, and GitHub's examples carry it everywhere.
MCP_SERVERS_JSON = """
{
  "mcpServers": {
    "orchardcore": {
      "type": "http",
      "url": "https://ai.orchardcore.net/mcp",
      "tools": ["*"]
    }
  }
}
"""

CURSOR_JSON = """
{
  "mcpServers": {
    "orchardcore": {
      "url": "https://ai.orchardcore.net/mcp"
    }
  }
}
"""

COPILOT_COMMANDS = """
npm install -g @github/copilot
copilot
/mcp add
"""

CLAUDE_INSTALL = """
npm install -g @anthropic-ai/claude-code
claude --version
"""

CLAUDE_ADD = """
claude mcp add --transport http orchardcore https://ai.orchardcore.net/mcp
"""


def slide(step_id, chapter_title, label):
    # Why connect an assistant.
    if step_id in ("why-1", "why-2", "why-3"):
        if step_id == "why-1":
            image = slide_base(chapter_title, label, "What an assistant can do")
            bullets(image, [
                ("1", "Scaffold modules", "Create the structure of a module or a theme"),
                ("2", "Write recipes", "Describe the content types, menus and pages of a site"),
                ("3", "Generate migrations", "Add parts and fields to content types in code"),
                ("4", "Explain features", "Show how a feature works, without searching by hand"),
            ])
            return image
        image = slide_base(chapter_title, label, "Two ways to ground an assistant")
        bullets(image, [
            ("1", "The Orchard Core MCP server", "Tool-based access to the Orchard Core documentation"),
            ("2", "AI Skills", "Repeatable, Orchard Core specific workflows"),
        ], active={0} if step_id == "why-2" else {1})
        return image

    # The MCP server.
    if step_id == "mcp-1":
        image = slide_base(chapter_title, label, "Model Context Protocol")
        bullets(image, [
            ("1", "An open standard", "AI assistants call external tools and data sources"),
            ("2", "Tools for the documentation", "The Orchard Core MCP server searches it"),
            ("3", "Accurate answers", "Correct APIs, module names and conventions"),
        ])
        return image

    if step_id == "mcp-2":
        image = slide_base(chapter_title, label, "One address for every client")
        draw_code(image, (LEFT, 240, LEFT_END, 460), "Orchard Core MCP server", "bash", SERVER, size=36)
        note_cards(image, [
            ("Remote", "One endpoint for every client"),
            ("HTTP", "Register it as an HTTP server"),
            ("Tools", "Search the docs"),
        ], (RIGHT, 240, RIGHT_END, 1030))
        return image

    # GitHub Copilot.
    if step_id == "copilot-1":
        image = slide_base(chapter_title, label, "GitHub Copilot CLI")
        draw_code(image, (LEFT, 240, LEFT_END, 500), "Terminal", "bash", COPILOT_COMMANDS, size=28, highlight={1, 2})
        note_cards(image, [
            ("Requires", "Node.js"),
            ("Starts with", "the copilot command"),
        ], (RIGHT, 240, RIGHT_END, 1030))
        return image

    if step_id == "copilot-2":
        image = slide_base(chapter_title, label, "Register the server")
        draw_code(image, (LEFT, 240, LEFT_END, 500), "Terminal", "bash", COPILOT_COMMANDS, size=28, highlight={3})
        draw_code(image, (LEFT, 540, LEFT_END, 960), "mcp-config.json", "json", MCP_SERVERS_JSON, size=26)
        note_cards(image, [
            ("Server Name", "orchardcore"),
            ("Server Type", "HTTP"),
            ("URL", SERVER),
        ], (RIGHT, 240, RIGHT_END, 1030))
        return image

    if step_id == "copilot-3":
        image = slide_base(chapter_title, label, "Copilot on github.com")
        draw_code(image, (LEFT, 240, LEFT_END, 700), "MCP configuration", "json", MCP_SERVERS_JSON, size=28)
        note_cards(image, [
            ("Where", "Settings, Copilot, MCP servers"),
            ("Required key", "tools, with * for every tool"),
            ("Used by", "The cloud agent and code review"),
        ], (RIGHT, 240, RIGHT_END, 1030))
        return image

    if step_id == "copilot-4":
        image = slide_base(chapter_title, label, "Visual Studio Code")
        draw_code(image, (LEFT, 240, LEFT_END, 700), ".mcp.json", "json", MCP_SERVERS_JSON, size=28, highlight={2})
        note_cards(image, [
            ("Location", "The root of your workspace"),
            ("Top-level key", "mcpServers"),
        ], (RIGHT, 240, RIGHT_END, 1030))
        return image

    if step_id == "copilot-5":
        image = slide_base(chapter_title, label, "Check the connection")
        bullets(image, [
            ("1", "Open the Chat view", "Select Open Customizations in the title bar"),
            ("2", "Open the Tools tab", "Confirm the tools of the orchardcore server are listed"),
        ])
        return image

    # Claude Code and Claude Desktop.
    if step_id == "claude-1":
        image = slide_base(chapter_title, label, "Claude Code")
        draw_code(image, (LEFT, 240, W - 96, 520), "Terminal", "bash", CLAUDE_INSTALL, size=30)
        bullets(image, [
            ("1", "A terminal-based assistant", "An AI coding assistant for the command line"),
            ("2", "Install it with npm", "It requires Node.js; then check the version"),
        ], top=560)
        return image

    if step_id == "claude-2":
        image = slide_base(chapter_title, label, "Add the server")
        draw_code(image, (LEFT, 240, W - 96, 460), "Terminal", "bash", CLAUDE_ADD, size=26)
        bullets(image, [
            ("1", "Transport", "HTTP"),
            ("2", "Name", "orchardcore"),
            ("3", "Address", SERVER),
        ], top=500)
        return image

    if step_id == "claude-3":
        image = slide_base(chapter_title, label, "Claude Desktop")
        bullets(image, [
            ("1", "Open Settings", "Then choose Connectors in the sidebar"),
            ("2", "Add a custom connector", "Select Add, then Add custom connector"),
            ("3", "Enter the server address", SERVER),
        ])
        return image

    # Cursor and other clients.
    if step_id == "cursor-1":
        image = slide_base(chapter_title, label, "Cursor")
        draw_code(image, (LEFT, 240, LEFT_END, 660), ".cursor/mcp.json", "json", CURSOR_JSON, size=28)
        note_cards(image, [
            ("Project", ".cursor/mcp.json"),
            ("Global", "~/.cursor/mcp.json"),
        ], (RIGHT, 240, RIGHT_END, 1030))
        return image

    if step_id == "other-1":
        image = slide_base(chapter_title, label, "Other clients")
        bullets(image, [
            ("1", "Find the setting", "MCP, Model Context Protocol, or Tools/Connectors"),
            ("2", "Register an HTTP server", "Any client that supports remote HTTP servers"),
            ("3", "Use the Orchard Core address", SERVER),
        ])
        return image

    # AI Skills.
    if step_id == "skills-1":
        image = slide_base(chapter_title, label, "AI Skills")
        bullets(image, [
            ("1", "Repeatable workflows", "Specific to Orchard Core"),
            ("2", "Curated skills and plugins", "For building Orchard Core solutions with AI"),
            ("3", "A community project", "Linked from the AI Quick Start page"),
        ])
        return image

    if step_id == "skills-2":
        image = slide_base(chapter_title, label, "A community project")
        bullets(image, [
            ("1", "Maintained by the community", "Not an official part of Orchard Core"),
            ("2", "Plugins for two CLIs", "Claude Code and the GitHub Copilot CLI"),
            ("3", "Installed from its repository", "Follow the instructions it gives"),
        ])
        return image

    if step_id == "skills-3":
        image = slide_base(chapter_title, label, "Use both together")
        bullets(image, [
            ("1", "The MCP server", "Keeps the assistant grounded in accurate documentation"),
            ("2", "AI Skills", "Guide it through common tasks, step by step"),
        ])
        return image

    # What to ask for.
    if step_id in ("ask-1", "ask-2"):
        image = slide_base(chapter_title, label, "What to ask for")
        bullets(image, [
            ("1", "Scaffold a module or theme", "A module with a Product content type and a price field"),
            ("2", "Write recipes", "A blog content type, a menu and a home page"),
            ("3", "Model content", "A migration adding AutoroutePart and MarkdownBodyPart to Article"),
        ], active={0, 1} if step_id == "ask-1" else {2})
        return image

    if step_id == "ask-3":
        image = slide_base(chapter_title, label, "Explain and extend")
        bullets(image, [
            ("1", "Explain features", "How display drivers and shapes work, and how to customize a part"),
            ("2", "Extend behavior", "Handle a content event to send a notification on publish"),
        ])
        return image

    if step_id == "ask-4":
        image = slide_base(chapter_title, label, "Give the assistant context")
        bullets(image, [
            ("1", "Enabled modules", "The modules your site uses"),
            ("2", "Content types", "The content types you use"),
            ("3", "Target version", "The Orchard Core version you build for"),
        ])
        return image

    if step_id == "ask-5":
        image = slide_base(chapter_title, label, "You stay responsible")
        bullets(image, [
            ("1", "Review generated code", "Before you run it in your application"),
            ("2", "Review generated recipes", "Before you run them in your application"),
        ])
        return image

    # Next steps.
    if step_id == "next-1":
        image = slide_base(chapter_title, label, "In the documentation")
        bullets(image, [
            ("1", "Create a CMS Web application", "Add Orchard Core to a .NET web application"),
            ("2", "Recipes and Starter Themes", "The recipes and themes a site starts from"),
            ("3", "Code Generation Templates", "Create sites, modules and themes with dotnet new"),
        ])
        return image

    raise SystemExit(f"No slide for {step_id}")
