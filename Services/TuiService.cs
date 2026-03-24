#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifyCli.Models;

namespace CoolifyCli.Services;

/// <summary>
/// Drives the interactive terminal user interface (TUI) loop.
/// Handles rendering, keyboard input dispatch, and data refresh against the API.
/// </summary>
public class TuiService
{
    private readonly ApplicationService _appService;
    private readonly DatabaseService _dbService;
    private readonly ILogger _logger;

    private const int HeaderRows = 4;
    private const int FooterRows = 2;

    public TuiService(ApplicationService appService, DatabaseService dbService, ILogger logger)
    {
        _appService = appService ?? throw new ArgumentNullException(nameof(appService));
        _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Starts the TUI event loop. Blocks until the user quits.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the loop from outside.</param>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var state = new TuiState();

        Console.CursorVisible = false;
        Console.Clear();

        await RefreshDataAsync(state);

        while (!state.ShouldExit && !cancellationToken.IsCancellationRequested)
        {
            Render(state);

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                await HandleKeyAsync(key, state, cancellationToken);
            }
            else
            {
                await Task.Delay(50, cancellationToken).ConfigureAwait(false);
            }
        }

        Console.CursorVisible = true;
        Console.Clear();
    }

    private async Task RefreshDataAsync(TuiState state)
    {
        state.IsRefreshing = true;
        state.StatusMessage = "Refreshing…";
        Render(state);

        var appResult = await _appService.GetAllApplicationsAsync();
        if (appResult.Success && appResult.Data is not null)
            state.Applications = appResult.Data;

        var dbResult = await _dbService.GetAllDatabasesAsync();
        if (dbResult.Success && dbResult.Data is not null)
            state.Databases = dbResult.Data;

        state.IsRefreshing = false;
        state.LastRefreshedAt = DateTime.UtcNow;
        state.StatusMessage = $"Last refreshed: {state.LastRefreshedAt:HH:mm:ss}";
    }

    private async Task HandleKeyAsync(ConsoleKeyInfo key, TuiState state, CancellationToken ct)
    {
        switch (key.Key)
        {
            case ConsoleKey.Q:
                state.ShouldExit = true;
                break;

            case ConsoleKey.DownArrow:
            case ConsoleKey.J:
                int listSize = state.ActiveView == TuiView.AppList
                    ? state.Applications.Count
                    : state.Databases.Count;
                state.MoveDown(listSize);
                break;

            case ConsoleKey.UpArrow:
            case ConsoleKey.K:
                state.MoveUp();
                break;

            case ConsoleKey.Enter:
                if (state.ActiveView == TuiView.AppList)
                {
                    var selected = state.GetSelectedApp();
                    if (selected is not null)
                    {
                        state.SelectedAppId = selected.Id;
                        state.ActiveView = TuiView.AppDetail;
                    }
                }
                break;

            case ConsoleKey.Escape:
            case ConsoleKey.Backspace:
                state.ActiveView = TuiView.AppList;
                state.SelectedAppId = null;
                state.ResetSelection();
                break;

            case ConsoleKey.D:
                if (state.ActiveView == TuiView.DbList)
                {
                    state.ActiveView = TuiView.AppList;
                    state.ResetSelection();
                }
                else
                {
                    state.ActiveView = TuiView.DbList;
                    state.ResetSelection();
                }
                break;

            case ConsoleKey.R:
                await RefreshDataAsync(state);
                break;

            case ConsoleKey.H:
            case ConsoleKey.F1:
                state.ActiveView = state.ActiveView == TuiView.Help
                    ? TuiView.AppList
                    : TuiView.Help;
                break;
        }
    }

    private void Render(TuiState state)
    {
        Console.SetCursorPosition(0, 0);

        int width = Math.Max(Console.WindowWidth, 60);
        int height = Math.Max(Console.WindowHeight, 12);
        int listRows = height - HeaderRows - FooterRows;

        RenderHeader(state, width);
        RenderBody(state, width, listRows);
        RenderFooter(state, width, height);
    }

    private static void RenderHeader(TuiState state, int width)
    {
        var title = " Coolify TUI  ";
        var viewName = state.ActiveView switch
        {
            TuiView.AppList => "Applications",
            TuiView.AppDetail => "App Detail",
            TuiView.DbList => "Databases",
            TuiView.Help => "Help",
            _ => ""
        };

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('─', width));
        Console.WriteLine($"{title}│ {viewName}".PadRight(width));
        Console.WriteLine(new string('─', width));
        Console.ResetColor();
    }

    private static void RenderBody(TuiState state, int width, int listRows)
    {
        switch (state.ActiveView)
        {
            case TuiView.AppList:
                RenderAppList(state, width, listRows);
                break;
            case TuiView.AppDetail:
                RenderAppDetail(state, width, listRows);
                break;
            case TuiView.DbList:
                RenderDbList(state, width, listRows);
                break;
            case TuiView.Help:
                RenderHelp(width, listRows);
                break;
        }
    }

    private static void RenderAppList(TuiState state, int width, int listRows)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($" {"#",-4} {"Name",-28} {"Status",-14} {"Last Deploy",-20}");
        Console.WriteLine(new string('-', Math.Min(width, 68)));
        Console.ResetColor();

        var visible = state.GetVisibleApps(Math.Max(listRows - 2, 1));
        int rowIndex = state.ScrollOffset;

        foreach (var app in visible)
        {
            bool isSelected = rowIndex == state.SelectedIndex;
            var deployed = app.LastDeployedAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
            var line = $" {app.Id,-4} {app.Name,-28} {app.Status,-14} {deployed,-20}";

            if (isSelected)
            {
                Console.BackgroundColor = ConsoleColor.DarkCyan;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = app.Status == DeploymentStatus.Deployed
                    ? ConsoleColor.Green
                    : app.Status == DeploymentStatus.Failed
                        ? ConsoleColor.Red
                        : ConsoleColor.Gray;
            }

            Console.WriteLine(line.PadRight(width));
            Console.ResetColor();
            rowIndex++;
        }

        // Pad remaining rows
        for (int i = visible.Count; i < listRows - 2; i++)
            Console.WriteLine(new string(' ', width));

        if (state.Applications.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  No applications found. Press 'r' to refresh.");
            Console.ResetColor();
        }
    }

    private static void RenderAppDetail(TuiState state, int width, int listRows)
    {
        var app = state.Applications.FirstOrDefault(a => a.Id == state.SelectedAppId);
        if (app is null)
        {
            Console.WriteLine("  Application not found.".PadRight(width));
            return;
        }

        var lines = new[]
        {
            $"  Name         : {app.Name}",
            $"  ID           : {app.Id}",
            $"  Status       : {app.Status}",
            $"  Repository   : {app.Repository}",
            $"  Branch       : {app.Branch}",
            $"  Environment  : {app.EnvironmentId}",
            $"  Last Deploy  : {app.LastDeployedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}",
            $"  Health Check : {app.HealthCheckUrl ?? "Not configured"}",
            $"  Failures     : {app.FailureCount}",
            $"  Ports        : {string.Join(", ", app.Ports)}",
        };

        foreach (var l in lines.Take(listRows))
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(l.PadRight(width));
            Console.ResetColor();
        }

        for (int i = lines.Length; i < listRows; i++)
            Console.WriteLine(new string(' ', width));
    }

    private static void RenderDbList(TuiState state, int width, int listRows)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($" {"#",-4} {"Name",-24} {"Type",-14} {"Host",-20} {"OK",-6}");
        Console.WriteLine(new string('-', Math.Min(width, 70)));
        Console.ResetColor();

        int row = 0;
        foreach (var db in state.Databases.Take(listRows - 2))
        {
            bool selected = row == state.SelectedIndex;
            var ok = db.IsHealthy ? "✓" : "✗";
            var line = $" {db.Id,-4} {db.Name,-24} {db.Type,-14} {db.Host,-20} {ok,-6}";

            if (selected) { Console.BackgroundColor = ConsoleColor.DarkCyan; Console.ForegroundColor = ConsoleColor.White; }
            else { Console.ForegroundColor = db.IsHealthy ? ConsoleColor.Green : ConsoleColor.Red; }

            Console.WriteLine(line.PadRight(width));
            Console.ResetColor();
            row++;
        }

        for (int i = state.Databases.Count; i < listRows - 2; i++)
            Console.WriteLine(new string(' ', width));
    }

    private static void RenderHelp(int width, int listRows)
    {
        var lines = new[]
        {
            "",
            "  Keyboard shortcuts:",
            "  ───────────────────────────────────",
            "  ↑ / k       Move selection up",
            "  ↓ / j       Move selection down",
            "  Enter       Open application detail",
            "  Esc / Bksp  Back to application list",
            "  d           Toggle database list",
            "  r           Refresh data from API",
            "  h / F1      Toggle this help panel",
            "  q           Quit TUI mode",
            "",
        };

        foreach (var l in lines.Take(listRows))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(l.PadRight(width));
            Console.ResetColor();
        }

        for (int i = lines.Length; i < listRows; i++)
            Console.WriteLine(new string(' ', width));
    }

    private static void RenderFooter(TuiState state, int width, int height)
    {
        Console.SetCursorPosition(0, height - FooterRows);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(new string('─', width));

        var hint = state.ActiveView == TuiView.AppList
            ? "Enter: detail  d: databases  r: refresh  h: help  q: quit"
            : "Esc: back  r: refresh  q: quit";

        var status = state.IsRefreshing ? " [refreshing…]" : $" {state.StatusMessage}";
        var footer = $"{hint}  │{status}";
        Console.WriteLine(footer.PadRight(width));
        Console.ResetColor();
    }
}
