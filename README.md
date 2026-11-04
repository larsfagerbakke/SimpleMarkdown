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

## Install

Packages are published on the [releases page](https://github.com/larsfagerbakke/SimpleMarkdown/releases).

### Fedora

Install:

1. Download `simplemarkdown-<N>-1.x86_64.rpm` from the latest release, either in the browser or with the GitHub CLI:

   ```
   gh release download --repo larsfagerbakke/SimpleMarkdown --pattern '*.rpm'
   ```

2. Install it with `dnf`, which also pulls in missing dependencies:

   ```
   sudo dnf install ./simplemarkdown-*.rpm
   ```

3. Start the app from the application menu or from a terminal:

   ```
   simplemarkdown path/to/file.md
   ```

The package installs the app to `/usr/lib/simplemarkdown`, the `simplemarkdown` command to `/usr/bin`, a desktop entry that lists the app for markdown files, and an icon. To open a markdown file from the file manager, choose Open With and then SimpleMarkdown.

Update: install the RPM of a newer release the same way. `dnf` replaces the older version.

Uninstall:

```
sudo dnf remove simplemarkdown
```

This removes everything the package installed. The app stores no settings, so nothing else is left behind.

Check the installation:

```
rpm -q simplemarkdown
rpm -V simplemarkdown
```

### Other Linux distributions

Extract `simplemarkdown-v<N>-linux-x64.tar.gz` and run the `SimpleMarkdown` executable inside. To uninstall, delete the extracted folder.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) to build and run from source

## Run

```
cd src/app
dotnet run -- path/to/file.md
```

The file argument is optional. Without it the app starts empty and waits for a file to be opened or dropped.

## Releases

Every push to `main` creates a GitHub release named after the whole-number version in `src/app/SimpleMarkdown.csproj` (v1, v2, v3, ...). The release contains an RPM for Fedora and a tarball for other Linux distributions. The version is also shown at the lower left of the app window.

## Publish

Build a self-contained app for a target platform:

```
cd src/app
dotnet publish -c Release -r linux-x64 --self-contained
```

Other runtime identifiers include `win-x64`, `osx-arm64` and `osx-x64`.

## Platforms

The app targets Windows, macOS and Linux (X11 and Wayland). It is developed and tested on Linux with Wayland. Release packages are built for Linux x86_64.

## License

[MIT](LICENSE). The copyright notice and license text must be kept in all copies or substantial portions of the software.
