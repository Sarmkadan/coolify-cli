#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifyCli.Infrastructure;
using CoolifyCli.Services;
using System.CommandLine;

namespace CoolifyCli.Commands;

/// <summary>
/// Provides the interactive TUI (Terminal User Interface) command, giving operators a
/// keyboard-driven dashboard for managing applications and databases without memorising
/// individual sub-commands.
/// </summary>
public class TuiCommands : CommandBase
{
    private readonly ApplicationService _appService;
    private readonly DatabaseService _dbService;

    public TuiCommands(CoolifyApiClient apiClient, ILogger logger, CoolifyConfiguration config)
        : base(apiClient, logger, config)
    {
        _appService = new ApplicationService(apiClient, logger);
        _dbService = new DatabaseService(apiClient, logger);
    }

    /// <summary>
    /// Creates the <c>tui</c> command that launches the interactive dashboard.
    /// </summary>
    public Command CreateTuiCommand()
    {
        var tuiCmd = new Command("tui", "Launch interactive terminal dashboard (keyboard-driven UI)");

        tuiCmd.SetAction(async (parseResult, ct) =>
        {
            if (Console.IsInputRedirected || Console.IsOutputRedirected)
            {
                WriteError("TUI mode requires an interactive terminal. Stdin/stdout must not be redirected.");
                Environment.ExitCode = 1;
                return;
            }

            Logger.Info("Starting interactive TUI");

            var service = new TuiService(_appService, _dbService, Logger);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

            try
            {
                await service.RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Normal exit via Ctrl+C
            }
            catch (Exception ex)
            {
                Console.CursorVisible = true;
                Console.ResetColor();
                WriteError($"TUI error: {ex.Message}");
                Logger.Error(ex, "TUI session terminated with an error");
                Environment.ExitCode = 1;
            }
        });

        return tuiCmd;
    }
}
