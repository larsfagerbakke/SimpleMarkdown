# SimpleMarkdown

A minimal, dark-themed markdown viewer for the desktop. Built with .NET and Avalonia.

## Features

- Renders GitHub-flavored markdown: headings, tables, task lists, strikethrough, code blocks with syntax highlighting and links
- GitHub alerts (`NOTE`, `TIP`, `IMPORTANT`, `WARNING`, `CAUTION`) with a colored bar per type
- Reloads automatically when the file changes on disk
- Switches between the formatted view and the raw markdown syntax
- Opens files from the command line, a file picker or drag and drop
- Shows word and character counts

## Keyboard shortcuts

| Shortcut | Action |
| --- | --- |
| Ctrl+O | Open a file |
| F5 | Reload |
| Ctrl+M | Toggle formatted view and markdown syntax |

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) to build and run from source

## Run

```
cd src/app
dotnet run -- path/to/file.md
```

The file argument is optional. Without it the app starts empty and waits for a file to be opened or dropped.

## Publish

Build a self-contained app for a target platform:

```
cd src/app
dotnet publish -c Release -r linux-x64 --self-contained
```

Other runtime identifiers include `win-x64`, `osx-arm64` and `osx-x64`.

## Platforms

The app targets Windows, macOS and Linux (X11 and Wayland). It is developed and tested on Linux with Wayland.

## License

[MIT](LICENSE). The copyright notice and license text must be kept in all copies or substantial portions of the software.
