# SimpleMarkdown

A minimal, dark-themed markdown viewer. Cross-platform desktop app built with .NET and Avalonia.

## Folder overview

```
SimpleMarkdown/
├── .gitignore               Ignore rules (bin/, obj/, out/, IDE files)
├── CLAUDE.md                This file
├── LICENSE                  MIT license, requires keeping the copyright notice
├── README.md                Project description, features, run and publish instructions
├── sample.md                GFM test document used to try the viewer
└── src/
    └── app/                 Avalonia application
        ├── SimpleMarkdown.csproj   net10.0 project, package references
        ├── Program.cs              Entry point, AppBuilder setup
        ├── App.axaml               Application resources, dark Fluent theme
        ├── App.axaml.cs            Creates MainWindow from the first CLI argument
        ├── MainWindow.axaml        Window layout, styles for rendered markdown
        ├── MainWindow.axaml.cs     File loading, watching, rendering, window chrome
        ├── app.ico                 Application icon
        └── app.png                 Icon shown in the toolbar
```

The tree lists only files that are not ignored by `.gitignore`. Update it whenever files or folders are added, moved or removed.

## Stack

- .NET 10, Avalonia 12.1.2, Fluent theme in dark variant
- `Markdown.Avalonia` renders markdown to native controls (no web view)
- `Avalonia.Wayland` provides the native Wayland backend alongside X11, Windows and macOS
- Inter font via `Avalonia.Fonts.Inter`

## Behavior

- Opens the file given as the first non-`--` command line argument.
- Open button or Ctrl+O shows a file picker; dropping a file onto the window opens it.
- The open file is watched and re-rendered when it changes. Reload or F5 renders again.
- The Syntax toggle or Ctrl+M switches between the formatted view and raw markdown.
- The status bar shows word and character counts.

## Rendering

`Markdown.Avalonia` does not support GitHub alerts, task lists or bare-URL autolinks. `MainWindow.PrepareMarkdown` rewrites them before rendering:

- Alert blocks (`> [!NOTE]`) become a blockquote with a bold symbol and title. `ColorAlerts` tints each quote's left bar by alert type after rendering.
- Task list items become lines starting with ☑ or ☐.
- Bare URLs become `[url](url)` links.
- Fenced code blocks are left untouched.

Styles for headings, tables, blockquotes and code blocks are in `MainWindow.axaml` under `MarkdownScrollViewer.Styles`, using the `ColorTextBlock.Avalonia` `CTextBlock` type and the library's class names (`Heading1`, `Table`, `Blockquote`, `CodeBlock` and so on).

## Window chrome

The window uses `WindowDecorations="None"`. The toolbar acts as the title bar: it holds the logo, app name, Open, Reload, the filename, the Syntax toggle and minimize, maximize and close buttons. Dragging the toolbar moves the window, double-clicking it toggles maximize, and transparent border strips resize it.

## Run and build

```
cd src/app
dotnet run -- ../../sample.md
```

## Conventions

- Markdown files state facts in the present tense. They contain no changelogs, history or dated entries.
- `CLAUDE.md` is kept in sync with the code: update it in the same change that alters structure, behavior or conventions.
