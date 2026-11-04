using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace SimpleMarkdown;

public partial class MainWindow : Window
{
    string? _path;
    FileSystemWatcher? _watcher;
    DateTime _lastChange = DateTime.MinValue;

    // Parameterless ctor required by the Avalonia XAML loader.
    public MainWindow() : this(null) { }

    public MainWindow(string? path)
    {
        InitializeComponent();
        VersionText.Text = "v" + (typeof(MainWindow).Assembly.GetName().Version?.Major ?? 0);

        OpenButton.Click += async (_, _) => await OpenAsync();
        ReloadButton.Click += (_, _) => Render();
        SyntaxToggle.Click += (_, _) => ApplySyntaxToggle(resetScroll: true);

        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.O, KeyModifiers.Control), Command = new Cmd(async () => await OpenAsync()) });
        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.F5), Command = new Cmd(() => Render()) });
        KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.M, KeyModifiers.Control),
            Command = new Cmd(() => { SyntaxToggle.IsChecked = SyntaxToggle.IsChecked != true; ApplySyntaxToggle(true); })
        });

        AddHandler(DragDrop.DropEvent, OnDrop);
        Closed += (_, _) => _watcher?.Dispose();
        Opened += (_, _) => { WindowState = WindowState.Normal; Activate(); };

        // Custom window chrome (the platform's default frame is white/black and doesn't match the theme).
        TitleBar.PointerPressed += (_, e) =>
        {
            if (!e.GetCurrentPoint(TitleBar).Properties.IsLeftButtonPressed || e.Source is Button or Avalonia.Controls.Primitives.ToggleButton) return;
            if (e.ClickCount == 2) ToggleMaximize(); else BeginMoveDrag(e);
        };
        MinButton.Click += (_, _) => WindowState = WindowState.Minimized;
        MaxButton.Click += (_, _) => ToggleMaximize();
        CloseButton.Click += (_, _) => Close();
        void Edge(Border b, WindowEdge edge) => b.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(b).Properties.IsLeftButtonPressed && WindowState == WindowState.Normal) BeginResizeDrag(edge, e);
        };
        Edge(EdgeN, WindowEdge.North); Edge(EdgeS, WindowEdge.South); Edge(EdgeW, WindowEdge.West); Edge(EdgeE, WindowEdge.East);
        Edge(CornerNW, WindowEdge.NorthWest); Edge(CornerNE, WindowEdge.NorthEast);
        Edge(CornerSW, WindowEdge.SouthWest); Edge(CornerSE, WindowEdge.SouthEast);
        if (path != null) Opened += (_, _) => LoadFile(path);
    }

    sealed class Cmd(Action a) : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? p) => true;
        public void Execute(object? p) => a();
    }

    void ToggleMaximize() =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    static string Stats(string text)
    {
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        return $"{words:N0} words   {text.Length:N0} characters";
    }

    // Markdown.Avalonia lacks GitHub alerts, task lists and bare-URL autolinks; rewrite them into plain markdown.
    static readonly Regex AlertRx = new(@"^(\s*>\s*)\[!(NOTE|TIP|IMPORTANT|WARNING|CAUTION)\]\s*$", RegexOptions.IgnoreCase);
    static readonly Regex TaskRx = new(@"^(\s*(?:[-*+]|\d+[.)])\s+)\[([ xX])\]\s", RegexOptions.None);
    static readonly Regex UrlRx = new(@"(?<![\(\[<""'`=/\w])(https?://[^\s<>)\]]*[^\s<>)\].,;:!?'""])", RegexOptions.None);
    static readonly Regex FenceRx = new(@"^\s*(```|~~~)", RegexOptions.None);

    static readonly Dictionary<string, (string Icon, string Title, string Color)> Alerts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["NOTE"] = ("ℹ", "Note", "#4A9EFF"),
        ["TIP"] = ("✦", "Tip", "#3FB950"),
        ["IMPORTANT"] = ("❖", "Important", "#A371F7"),
        ["WARNING"] = ("▲", "Warning", "#D29922"),
        ["CAUTION"] = ("⬣", "Caution", "#F85149"),
    };

    static string PrepareMarkdown(string text)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n');
        bool fenced = false;
        for (int i = 0; i < lines.Length; i++)
        {
            var l = lines[i];
            if (FenceRx.IsMatch(l)) { fenced = !fenced; continue; }
            if (fenced) continue;
            var am = AlertRx.Match(l);
            if (am.Success)
            {
                var a = Alerts[am.Groups[2].Value];
                lines[i] = $"{am.Groups[1].Value}**{a.Icon} {a.Title}**  ";
                continue;
            }
            l = TaskRx.Replace(l, m => (m.Groups[2].Value == " " ? "☐  " : "☑  "));
            if (l.StartsWith("☐") || l.StartsWith("☑")) { lines[i] = l + "  "; continue; }
            if (!l.Contains("](") && !l.Contains('`')) l = UrlRx.Replace(l, "[$1]($1)");
            lines[i] = l;
        }
        return string.Join("\n", lines);
    }

    // Tint each alert's left bar with its own color (matched by the title text we injected above).
    void ColorAlerts()
    {
        foreach (var v in Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(Viewer))
        {
            if (v is not Border b || !b.Classes.Contains("Blockquote")) continue;
            var first = Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(b)
                .FirstOrDefault(d => d.GetType().Name == "CTextBlock");
            var txt = first?.GetType().GetProperty("Text")?.GetValue(first) as string;
            if (string.IsNullOrEmpty(txt)) continue;
            foreach (var a in Alerts.Values)
                if (txt.Contains(a.Icon + " " + a.Title))
                {
                    b.BorderBrush = Avalonia.Media.Brush.Parse(a.Color);
                    break;
                }
        }
    }

    void ApplySyntaxToggle(bool resetScroll)
    {
        SyntaxToggle.Content = SyntaxToggle.IsChecked == true ? "</> Formatted" : "</> Syntax";
        Render(resetScroll);
    }

    async Task OpenAsync()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Markdown files") { Patterns = ["*.md", "*.markdown", "*.mdown", "*.mkd"] },
                new FilePickerFileType("Text files") { Patterns = ["*.txt"] },
                FilePickerFileTypes.All
            ]
        });
        var local = files.FirstOrDefault()?.TryGetLocalPath();
        if (local != null) LoadFile(local);
    }

    void OnDrop(object? sender, DragEventArgs e)
    {
        var local = e.DataTransfer.TryGetFiles()?.FirstOrDefault()?.TryGetLocalPath();
        if (local != null) LoadFile(local);
    }

    void LoadFile(string path)
    {
        try
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);
            _path = path;
            TitleText.Text = Path.GetFileName(path);
            ToolTip.SetTip(TitleText, path);
            Hint.IsVisible = false;
            Watch(path);
            Render(resetScroll: true);
        }
        catch (Exception ex)
        {
            TitleText.Text = ex.Message;
        }
    }

    void Watch(string path)
    {
        _watcher?.Dispose();
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(path)!, Path.GetFileName(path))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        _watcher.Changed += (_, _) =>
        {
            if ((DateTime.Now - _lastChange).TotalMilliseconds < 300) return;
            _lastChange = DateTime.Now;
            Dispatcher.UIThread.InvokeAsync(async () => { await Task.Delay(150); Render(); });
        };
    }

    string ReadText()
    {
        for (int i = 0; ; i++)
        {
            try
            {
                using var fs = new FileStream(_path!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var sr = new StreamReader(fs, detectEncodingFromByteOrderMarks: true);
                return sr.ReadToEnd();
            }
            catch (IOException) when (i < 5) { Thread.Sleep(100); }
        }
    }

    void Render(bool resetScroll = false)
    {
        if (_path == null) return;
        string text;
        try { text = ReadText(); } catch { return; }

        StatsText.Text = Stats(text);
        var raw = SyntaxToggle.IsChecked == true;
        Viewer.IsVisible = !raw;
        RawScroll.IsVisible = raw;
        if (raw)
        {
            RawText.Text = text;
            if (resetScroll) RawScroll.Offset = default;
        }
        else
        {
            var offset = Viewer.ScrollValue;
            Viewer.Markdown = PrepareMarkdown(text);
            Dispatcher.UIThread.Post(ColorAlerts, DispatcherPriority.Background);
            if (!resetScroll) Dispatcher.UIThread.Post(() => Viewer.ScrollValue = offset, DispatcherPriority.Background);
        }
    }
}
