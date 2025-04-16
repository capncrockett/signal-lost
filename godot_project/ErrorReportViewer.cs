using Godot;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SignalLost
{
    public partial class ErrorReportViewer : Node
    {
        [Signal]
        public delegate void ErrorsLoadedEventHandler(int errorCount, int warningCount);

        private List<string> _errors = new List<string>();
        private List<string> _warnings = new List<string>();

        public override void _Ready()
        {
            LoadLatestErrorReport();
        }

        public void LoadLatestErrorReport()
        {
            var logsDir = "res://logs";
            
            // Ensure the directory exists
            if (!Directory.Exists(logsDir))
            {
                GD.Print($"Logs directory not found: {logsDir}");
                return;
            }

            // Get the latest error report
            var reportFiles = Directory.GetFiles(logsDir, "error_report_*.md")
                                      .OrderByDescending(f => File.GetLastWriteTime(f))
                                      .ToList();

            if (reportFiles.Count == 0)
            {
                GD.Print("No error reports found");
                return;
            }

            var latestReport = reportFiles[0];
            GD.Print($"Loading error report: {latestReport}");

            // Parse the error report
            _errors.Clear();
            _warnings.Clear();

            var lines = File.ReadAllLines(latestReport);
            bool inErrorsSection = false;
            bool inWarningsSection = false;

            foreach (var line in lines)
            {
                if (line.StartsWith("## Errors"))
                {
                    inErrorsSection = true;
                    inWarningsSection = false;
                    continue;
                }
                else if (line.StartsWith("## Warnings"))
                {
                    inErrorsSection = false;
                    inWarningsSection = true;
                    continue;
                }
                else if (line.StartsWith("##"))
                {
                    inErrorsSection = false;
                    inWarningsSection = false;
                    continue;
                }

                if (inErrorsSection && !string.IsNullOrWhiteSpace(line))
                {
                    _errors.Add(line);
                }
                else if (inWarningsSection && !string.IsNullOrWhiteSpace(line))
                {
                    _warnings.Add(line);
                }
            }

            GD.Print($"Loaded {_errors.Count} errors and {_warnings.Count} warnings");
            EmitSignal(SignalName.ErrorsLoaded, _errors.Count, _warnings.Count);
        }

        public List<string> GetErrors()
        {
            return _errors;
        }

        public List<string> GetWarnings()
        {
            return _warnings;
        }

        public void PrintSummary()
        {
            GD.Print($"Error Report Summary: {_errors.Count} errors, {_warnings.Count} warnings");
            
            if (_errors.Count > 0)
            {
                GD.Print("\n=== ERRORS ===");
                foreach (var error in _errors)
                {
                    GD.Print(error);
                }
            }
            
            if (_warnings.Count > 0)
            {
                GD.Print("\n=== WARNINGS ===");
                foreach (var warning in _warnings)
                {
                    GD.Print(warning);
                }
            }
        }
    }
}
