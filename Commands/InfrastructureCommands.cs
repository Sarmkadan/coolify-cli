#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifyCli.Commands;

using System.CommandLine;
using CoolifyCli.Extensions;
using CoolifyCli.Formatters;
using CoolifyCli.Infrastructure;
using CoolifyCli.Models;
using CoolifyCli.Services;

/// <summary>
/// Factory that creates the top-level <c>iac</c> command group and its complete subcommand
/// tree, exposing the full infrastructure-as-code lifecycle from the terminal:
/// <list type="bullet">
///   <item><c>iac apply</c>    — reconcile the live environment with a template</item>
///   <item><c>iac validate</c> — validate a template without touching live state</item>
///   <item><c>iac diff</c>     — show resource-level differences against live state</item>
///   <item><c>iac export</c>   — snapshot live state as a portable YAML template</item>
///   <item><c>iac init</c>     — generate a starter template scaffold file</item>
/// </list>
/// </summary>
public static class InfrastructureCommands
{
    /// <summary>
    /// Builds and returns the <c>iac</c> <see cref="Command"/> with all subcommands wired
    /// to the provided service dependencies.  The returned command is ready to be attached
    /// to a <see cref="RootCommand"/> via <c>AddCommand</c>.
    /// </summary>
    /// <param name="appService">Application lifecycle service used by the template engine.</param>
    /// <param name="dbService">Database management service used by the template engine.</param>
    /// <param name="logger">Structured diagnostic logger.</param>
    /// <returns>The fully configured <c>iac</c> command group.</returns>
    public static Command CreateIacCommand(
        ApplicationService appService,
        DatabaseService    dbService,
        ILogger            logger)
    {
        var engine   = IacServiceExtensions.CreateTemplateEngine(appService, dbService, logger);
        var resolver = new TemplateVariableResolver(logger);

        var iacCommand = new Command(
            "iac",
            "Manage infrastructure as declarative YAML templates (infrastructure-as-code)");

        iacCommand.Add(CreateApplyCommand(engine, resolver, logger));
        iacCommand.Add(CreateValidateCommand(engine, resolver, logger));
        iacCommand.Add(CreateDiffCommand(engine, resolver, logger));
        iacCommand.Add(CreateExportCommand(engine, logger));
        iacCommand.Add(CreateInitCommand(logger));

        return iacCommand;
    }

    // ─── apply ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates the <c>iac apply</c> subcommand that reconciles the live environment with a
    /// declarative YAML template file.
    /// </summary>
    private static Command CreateApplyCommand(
        IInfrastructureTemplateEngine engine,
        TemplateVariableResolver      resolver,
        ILogger                       logger)
    {
        var fileArg = new Argument<string>("file") { Description = "Path to the YAML infrastructure template to apply" };
        var dryOpt = new Option<bool>("--dry-run", ["-d"]) { Description = "Simulate all operations without mutating live state" };
        var yesOpt = new Option<bool>("--yes", ["-y"]) { Description = "Skip the confirmation prompt before applying" };
        var ciOpt = new Option<bool>("--ci") { Description = "Use CI-optimised options: auto-approve, JSON output, fail-fast" };
        var fmtOpt = new Option<string>("--format", ["-f"]) { Description = "Output format: text (default) | json", DefaultValueFactory = _ => "text" };

        var cmd = new Command(
            "apply",
            "Reconcile the live Coolify environment to match a declarative template")
        {
            fileArg, dryOpt, yesOpt, ciOpt, fmtOpt
        };

        cmd.SetAction(async (parseResult, ct) =>
        {
            var file = parseResult.GetValue(fileArg);
            var dry  = parseResult.GetValue(dryOpt);
            var yes  = parseResult.GetValue(yesOpt);
            var ci   = parseResult.GetValue(ciOpt);
            var fmt  = parseResult.GetValue(fmtOpt);
            try
            {
                var options = ci
                    ? IacTemplateOptions.CiMode
                    : new IacTemplateOptions
                      {
                          DryRun       = dry,
                          AutoApprove  = yes,
                          OutputFormat = fmt!,
                          ShowDiff     = true
                      };

                var loadResult = await engine.LoadWithVariablesAsync(file!, resolver);
                if (!loadResult.Success)
                {
                    logger.Error(loadResult.Message!);
                    System.Environment.Exit(Constants.ExitCodes.ValidationError);
                    return;
                }

                var template = loadResult.Data!;

                if (options.ShowDiff)
                {
                    var diffResult = await engine.ComputeDiffAsync(template);
                    if (diffResult.Success && diffResult.Data is { } d)
                        TemplateDiffFormatter.FormatDiff(d, options);
                }

                if (!options.DryRun && !options.AutoApprove)
                {
                    Console.Write("Apply these changes to the live environment? [y/N] ");
                    var answer = Console.ReadLine();
                    if (!string.Equals(answer?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info("Apply cancelled.");
                        return;
                    }
                }

                var applyResponse = await engine.ApplyTemplateAsync(template, options);

                if (applyResponse.Data is { } applied)
                    TemplateDiffFormatter.FormatApplyResult(applied, options);

                if (!applyResponse.Success || applyResponse.Data?.Success == false)
                {
                    logger.Error(applyResponse.Message ?? "Apply completed with failures.");
                    System.Environment.Exit(Constants.ExitCodes.GeneralError);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unexpected error during iac apply");
                System.Environment.Exit(Constants.ExitCodes.UnhandledError);
            }
        });

        return cmd;
    }

    // ─── validate ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates the <c>iac validate</c> subcommand that checks structural and semantic
    /// integrity of a template without contacting the Coolify API.
    /// </summary>
    private static Command CreateValidateCommand(
        IInfrastructureTemplateEngine engine,
        TemplateVariableResolver      resolver,
        ILogger                       logger)
    {
        var fileArg = new Argument<string>("file") { Description = "Path to the YAML infrastructure template to validate" };
        var fmtOpt = new Option<string>("--format", ["-f"]) { Description = "Output format: text (default) | json", DefaultValueFactory = _ => "text" };

        var cmd = new Command(
            "validate",
            "Validate a template structure and emit errors/warnings without applying it")
        {
            fileArg, fmtOpt
        };

        cmd.SetAction(async (parseResult, ct) =>
        {
            var file = parseResult.GetValue(fileArg);
            var fmt  = parseResult.GetValue(fmtOpt);
            try
            {
                var options    = new IacTemplateOptions { OutputFormat = fmt! };
                var loadResult = await engine.LoadWithVariablesAsync(file!, resolver);

                if (!loadResult.Success)
                {
                    logger.Error(loadResult.Message!);
                    System.Environment.Exit(Constants.ExitCodes.ValidationError);
                    return;
                }

                var valResponse = await engine.ValidateTemplateAsync(loadResult.Data!);
                var validation  = valResponse.Data!;

                TemplateDiffFormatter.FormatValidationResult(validation, options);

                if (!validation.IsValid)
                    System.Environment.Exit(Constants.ExitCodes.ValidationError);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unexpected error during iac validate");
                System.Environment.Exit(Constants.ExitCodes.UnhandledError);
            }
        });

        return cmd;
    }

    // ─── diff ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates the <c>iac diff</c> subcommand that compares a template against live state
    /// and emits a categorised change summary without making any modifications.
    /// </summary>
    private static Command CreateDiffCommand(
        IInfrastructureTemplateEngine engine,
        TemplateVariableResolver      resolver,
        ILogger                       logger)
    {
        var fileArg = new Argument<string>("file") { Description = "Path to the YAML infrastructure template to diff" };
        var fmtOpt = new Option<string>("--format", ["-f"]) { Description = "Output format: text (default) | json", DefaultValueFactory = _ => "text" };

        var cmd = new Command(
            "diff",
            "Show resource-level differences between a template and the live Coolify environment")
        {
            fileArg, fmtOpt
        };

        cmd.SetAction(async (parseResult, ct) =>
        {
            var file = parseResult.GetValue(fileArg);
            var fmt  = parseResult.GetValue(fmtOpt);
            try
            {
                var options    = new IacTemplateOptions { OutputFormat = fmt!, ShowDiff = true };
                var loadResult = await engine.LoadWithVariablesAsync(file!, resolver);

                if (!loadResult.Success)
                {
                    logger.Error(loadResult.Message!);
                    System.Environment.Exit(Constants.ExitCodes.ValidationError);
                    return;
                }

                var diffResponse = await engine.ComputeDiffAsync(loadResult.Data!);

                if (!diffResponse.Success)
                {
                    logger.Error($"Diff computation failed: {diffResponse.Message}");
                    System.Environment.Exit(Constants.ExitCodes.ApiError);
                    return;
                }

                TemplateDiffFormatter.FormatDiff(diffResponse.Data!, options);

                // Exit with a non-zero code only when there are pending changes so that
                // CI pipelines can gate on drift detection without treating "in sync" as
                // an error.
                if (diffResponse.Data!.HasChanges)
                    System.Environment.Exit(Constants.ExitCodes.GeneralError);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unexpected error during iac diff");
                System.Environment.Exit(Constants.ExitCodes.UnhandledError);
            }
        });

        return cmd;
    }

    // ─── export ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates the <c>iac export</c> subcommand that snapshots the live Coolify environment
    /// and emits it as a portable YAML template, seeding a declarative workflow for an
    /// existing deployment.
    /// </summary>
    private static Command CreateExportCommand(
        IInfrastructureTemplateEngine engine,
        ILogger                       logger)
    {
        var outOpt = new Option<string?>("--output", ["-o"]) { Description = "Write the exported YAML to this file path; prints to stdout when omitted" };
        var nameOpt = new Option<string>("--name", ["-n"]) { Description = "Override the metadata.name field of the exported template", DefaultValueFactory = _ => "exported-stack" };

        var cmd = new Command(
            "export",
            "Snapshot the live Coolify environment and emit it as a YAML template")
        {
            outOpt, nameOpt
        };

        cmd.SetAction(async (parseResult, ct) =>
        {
            var output = parseResult.GetValue(outOpt);
            var name   = parseResult.GetValue(nameOpt);
            try
            {
                var exportResponse = await engine.ExportCurrentStateAsync();

                if (!exportResponse.Success || exportResponse.Data is null)
                {
                    logger.Error($"Export failed: {exportResponse.Message}");
                    System.Environment.Exit(Constants.ExitCodes.ApiError);
                    return;
                }

                // Override the metadata name while keeping all other exported properties.
                var exported = exportResponse.Data with
                {
                    Metadata = exportResponse.Data.Metadata with { Name = name! }
                };

                var yaml = InfrastructureTemplateEngine.SerializeToYaml(exported);

                if (string.IsNullOrWhiteSpace(output))
                {
                    Console.Write(yaml);
                }
                else
                {
                    await File.WriteAllTextAsync(output, yaml);
                    logger.Info($"Template exported to '{output}'");
                    Console.WriteLine(
                        $"  {exported.Applications.Count} application(s), " +
                        $"{exported.Databases.Count} database(s) captured");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unexpected error during iac export");
                System.Environment.Exit(Constants.ExitCodes.UnhandledError);
            }
        });

        return cmd;
    }

    // ─── init ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates the <c>iac init</c> subcommand that generates an annotated starter template
    /// scaffold pre-populated with sensible defaults and placeholder guidance.
    /// </summary>
    private static Command CreateInitCommand(ILogger logger)
    {
        var nameOpt = new Option<string>("--name", ["-n"]) { Description = "Infrastructure stack name written into template metadata", DefaultValueFactory = _ => "my-stack" };
        var outOpt = new Option<string>("--output", ["-o"]) { Description = "Destination file path for the generated template scaffold", DefaultValueFactory = _ => Constants.Iac.DefaultTemplateFileName };
        var envOpt = new Option<string>("--environment", ["-e"]) { Description = "Target environment label embedded in the template metadata", DefaultValueFactory = _ => "production" };

        var cmd = new Command(
            "init",
            $"Generate a starter template scaffold (default output: {Constants.Iac.DefaultTemplateFileName})")
        {
            nameOpt, outOpt, envOpt
        };

        cmd.SetAction(async (parseResult, ct) =>
        {
            var name        = parseResult.GetValue(nameOpt);
            var outputPath  = parseResult.GetValue(outOpt);
            var environment = parseResult.GetValue(envOpt);

            if (File.Exists(outputPath))
            {
                Console.Write($"  '{outputPath}' already exists. Overwrite? [y/N] ");
                var answer = Console.ReadLine();
                if (!string.Equals(answer?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
                {
                    logger.Info("Init cancelled.");
                    return;
                }
            }

            var scaffold = BuildScaffold(name!, environment!);
            await File.WriteAllTextAsync(outputPath!, scaffold);

            logger.Info($"Scaffold written to '{outputPath}'");
            Console.WriteLine();
            Console.WriteLine($"  Template created: {outputPath}");
            Console.WriteLine($"  Next steps:");
            Console.WriteLine($"    1. Edit '{outputPath}' to describe your infrastructure");
            Console.WriteLine($"    2. Run: coolify-cli iac validate {outputPath}");
            Console.WriteLine($"    3. Run: coolify-cli iac diff     {outputPath}");
            Console.WriteLine($"    4. Run: coolify-cli iac apply    {outputPath}");
            Console.WriteLine();
        });

        return cmd;
    }

    // ─── Scaffold builder ─────────────────────────────────────────────────────

    /// <summary>
    /// Produces a fully-annotated YAML scaffold string that demonstrates all supported
    /// template fields, serving as both documentation and a starting point for new stacks.
    /// </summary>
    private static string BuildScaffold(string name, string environment) =>
        $$"""
        # Coolify Infrastructure Template
        # coolify-cli v{{Constants.ApplicationVersion}} | {{Constants.Author}} | {{Constants.AuthorUrl}}
        #
        # Variable substitution: wrap any environment variable name in dollar-brace notation
        # to inject its value at apply time. Example: environmentId: ${COOLIFY_ENVIRONMENT_ID}
        #
        # Apply with:    coolify-cli iac apply    {{Constants.Iac.DefaultTemplateFileName}}
        # Dry-run with:  coolify-cli iac apply -d {{Constants.Iac.DefaultTemplateFileName}}
        # Show diff:     coolify-cli iac diff     {{Constants.Iac.DefaultTemplateFileName}}

        apiVersion: v2
        kind: CoolifyInfrastructure

        metadata:
          name: {{name}}
          description: "Infrastructure stack for {{name}}"
          environment: {{environment}}
          version: "1.0.0"
          labels:
            managed-by: coolify-cli
            environment: {{environment}}

        applications:
          - name: web
            repository: https://github.com/your-org/your-repo
            branch: main
            runtime: Docker
            environmentId: ${COOLIFY_ENVIRONMENT_ID}
            ports:
              - 3000
            healthCheck:
              url: /health
              intervalSeconds: 30
              failureThreshold: 3
            environment:
              NODE_ENV: {{environment}}
            scaling:
              instances: 1
              policy: Manual
            resources:
              cpuLimit: "500m"
              memoryLimit: "512Mi"

        databases:
          - name: app-db
            type: PostgreSQL
            version: "15"
            maxConnections: 100
            connectionTimeoutSeconds: 30
            backup:
              enabled: true
              strategy: Snapshot
              retentionDays: 30
              schedule: "0 2 * * *"
        """;
}
