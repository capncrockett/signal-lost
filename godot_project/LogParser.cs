using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SignalLost
{
    public partial class LogParser : Node
    {
        public class LogEntry
        {
            public enum LogLevel
            {
                Info,
                Warning,
                Error,
                ScriptError
            }

            public LogLevel Level { get; set; }
            public string Message { get; set; }
            public string Location { get; set; }
            public int LineNumber { get; set; }
            public DateTime Timestamp { get; set; }

            public override string ToString()
            {
                return $"[{Level}] {Message} {(string.IsNullOrEmpty(Location) ? "" : $"at: {Location}")} {(LineNumber > 0 ? $"line: {LineNumber}" : "")}";
            }
        }

        public static List<LogEntry> ParseLatestLog()
        {
            var logEntries = new List<LogEntry>();
            var logsDir = "res://logs";

            // Ensure the directory exists
            if (!Directory.Exists(logsDir))
            {
                GD.Print($"Logs directory not found: {logsDir}");
                return logEntries;
            }

            // Get the latest log file
            var logFiles = Directory.GetFiles(logsDir, "godot_log_*.log")
                                   .OrderByDescending(f => File.GetLastWriteTime(f))
                                   .ToList();

            if (logFiles.Count == 0)
            {
                GD.Print("No log files found");
                return logEntries;
            }

            var latestLogFile = logFiles[0];
            GD.Print($"Parsing log file: {latestLogFile}");

            // Parse the log file
            var lines = File.ReadAllLines(latestLogFile);
            var errorRegex = new Regex(@"ERROR:\s+(.+?)(?:\s+at:\s+(.+?)(?:\((.+?):(\d+)\))?)?$");
            var warningRegex = new Regex(@"WARNING:\s+(.+?)(?:\s+at:\s+(.+?)(?:\((.+?):(\d+)\))?)?$");
            var scriptErrorRegex = new Regex(@"SCRIPT ERROR:\s+(.+?)(?:\s+at:\s+(.+?)(?:\((.+?):(\d+)\))?)?$");

            foreach (var line in lines)
            {
                LogEntry entry = null;

                if (line.Contains("ERROR:"))
                {
                    var match = errorRegex.Match(line);
                    if (match.Success)
                    {
                        entry = new LogEntry
                        {
                            Level = LogEntry.LogLevel.Error,
                            Message = match.Groups[1].Value.Trim(),
                            Location = match.Groups[2].Value.Trim(),
                            LineNumber = match.Groups.Count > 4 && int.TryParse(match.Groups[4].Value, out int lineNum) ? lineNum : 0,
                            Timestamp = DateTime.Now
                        };
                    }
                }
                else if (line.Contains("SCRIPT ERROR:"))
                {
                    var match = scriptErrorRegex.Match(line);
                    if (match.Success)
                    {
                        entry = new LogEntry
                        {
                            Level = LogEntry.LogLevel.ScriptError,
                            Message = match.Groups[1].Value.Trim(),
                            Location = match.Groups[2].Value.Trim(),
                            LineNumber = match.Groups.Count > 4 && int.TryParse(match.Groups[4].Value, out int lineNum) ? lineNum : 0,
                            Timestamp = DateTime.Now
                        };
                    }
                }
                else if (line.Contains("WARNING:"))
                {
                    var match = warningRegex.Match(line);
                    if (match.Success)
                    {
                        entry = new LogEntry
                        {
                            Level = LogEntry.LogLevel.Warning,
                            Message = match.Groups[1].Value.Trim(),
                            Location = match.Groups[2].Value.Trim(),
                            LineNumber = match.Groups.Count > 4 && int.TryParse(match.Groups[4].Value, out int lineNum) ? lineNum : 0,
                            Timestamp = DateTime.Now
                        };
                    }
                }

                if (entry != null)
                {
                    logEntries.Add(entry);
                }
            }

            return logEntries;
        }

        public static void PrintErrorSummary()
        {
            var entries = ParseLatestLog();

            GD.Print($"Found {entries.Count} log entries");

            var errors = entries.Where(e => e.Level == LogEntry.LogLevel.Error || e.Level == LogEntry.LogLevel.ScriptError).ToList();
            var warnings = entries.Where(e => e.Level == LogEntry.LogLevel.Warning).ToList();

            GD.Print($"Errors: {errors.Count}, Warnings: {warnings.Count}");

            if (errors.Count > 0)
            {
                GD.Print("\n=== ERRORS ===");
                foreach (var error in errors)
                {
                    GD.Print(error);
                }
            }

            if (warnings.Count > 0)
            {
                GD.Print("\n=== WARNINGS ===");
                foreach (var warning in warnings)
                {
                    GD.Print(warning);
                }
            }
        }
    }
}
