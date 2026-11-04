# SimpleMarkdown

A minimal, dark-themed markdown viewer. Cross-platform desktop app built with .NET and Avalonia.

## Folder overview

```
SimpleMarkdown/
├── .github/
│   └── workflows/
│       └── release.yml      Builds packages and creates a release on every push to main
├── .gitignore               Ignore rules (bin/, obj/, out/, dist/, IDE files)
├── CLAUDE.md                This file
├── LICENSE                  MIT license, requires keeping the copyright notice
├── README.md                Project description, features, run and publish instructions
├── packaging/
│   ├── build.sh                    Publishes the app and builds the tarball and RPM into dist/
│   ├── simplemarkdown.spec         RPM spec for the self-contained publish output
│   ├── SimpleMarkdown.desktop      Desktop entry that registers the app for markdown files
│   └── simplemarkdown.png          256px icon installed by the RPM
├── sample.md                GFM test document used to try the viewer
└── src/
    └── app/                 Avalonia application
        ├── SimpleMarkdown.csproj   net10.0 project, package references
        ├── Program.cs              Entry point, AppBuilder setup
        ├── App.axaml               Application resources, dark Fluent theme
        ├── App.axaml.cs            Creates MainWindow from the first CLI argument
        ├── MainWindow.axaml        Window layout, styles for rendered markdown
        ├── MainWindow.axaml.cs     File loading, watching, rendering, window chrome, version label
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
- The status bar shows the version as `v<N>` at the lower left and word and character counts at the lower right.

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

## Versioning and releases

- The version is a whole number (1, 2, 3, ...) stored in `<Version>` in `src/app/SimpleMarkdown.csproj`. It is the single source of truth.
- The app reads it from the assembly and shows it in the status bar as `v<N>`.
- Every push to `main` increases the version by one. The release workflow fails when a release for the current version already exists.
- `.github/workflows/release.yml` runs on every push to `main`: it runs `packaging/build.sh`, then creates the GitHub release `v<N>` with generated notes and two assets:
  - `simplemarkdown-<N>-1.x86_64.rpm`, the Fedora package. It installs the app to `/usr/lib/simplemarkdown`, a `simplemarkdown` command to `/usr/bin`, a desktop entry for markdown files and an icon.
  - `simplemarkdown-v<N>-linux-x64.tar.gz`, a self-contained build for other Linux distributions.
- `packaging/build.sh` also runs locally and writes to `dist/`, which is ignored by git.

## Conventions

- Markdown files state facts in the present tense. They contain no changelogs, history or dated entries.
- `CLAUDE.md` is kept in sync with the code: update it in the same change that alters structure, behavior or conventions.
