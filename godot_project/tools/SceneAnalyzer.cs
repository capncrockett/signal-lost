using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using FileAccess = Godot.FileAccess;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SignalLost.Tools
{
    [GlobalClass]
    public partial class SceneAnalyzer : Node
    {
        private Dictionary<string, string> _sceneUids = new Dictionary<string, string>();
        private Dictionary<string, List<string>> _sceneReferences = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _duplicateUids = new Dictionary<string, List<string>>();

        [Export]
        public string OutputPath { get; set; } = "res://scene_analysis.md";

        [Export]
        public string ProjectRoot { get; set; } = "res://";

        public override void _Ready()
        {
            AnalyzeProject();
        }

        public void AnalyzeProject()
        {
            GD.Print("Starting scene analysis...");

            // Clear previous data
            _sceneUids.Clear();
            _sceneReferences.Clear();
            _duplicateUids.Clear();

            // Find all .tscn files in the project
            var sceneFiles = FindAllSceneFiles(ProjectRoot);
            GD.Print($"Found {sceneFiles.Count} scene files");

            // Extract UIDs and references from each scene
            foreach (var scenePath in sceneFiles)
            {
                AnalyzeScene(scenePath);
            }

            // Find duplicate UIDs
            FindDuplicateUids();

            // Generate report
            GenerateReport();

            GD.Print("Scene analysis complete!");
        }

        private static List<string> FindAllSceneFiles(string rootPath)
        {
            var sceneFiles = new List<string>();
            var dir = DirAccess.Open(rootPath);

            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();

                while (!string.IsNullOrEmpty(fileName))
                {
                    if (fileName != "." && fileName != "..")
                    {
                        string fullPath = rootPath.TrimEnd('/') + "/" + fileName;

                        if (dir.CurrentIsDir())
                        {
                            // Skip .godot directory and addons
                            if (fileName != ".godot" && fileName != "addons")
                            {
                                sceneFiles.AddRange(FindAllSceneFiles(fullPath));
                            }
                        }
                        else if (fileName.EndsWith(".tscn"))
                        {
                            sceneFiles.Add(fullPath);
                        }
                    }

                    fileName = dir.GetNext();
                }

                dir.ListDirEnd();
            }

            return sceneFiles;
        }

        private void AnalyzeScene(string scenePath)
        {
            var file = FileAccess.Open(scenePath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"Failed to open file: {scenePath}");
                return;
            }

            string content = file.GetAsText();
            file.Close();

            // Extract UID
            var uidMatch = Regex.Match(content, @"\[gd_scene[^\]]*uid=""([^""]+)""");
            if (uidMatch.Success)
            {
                string uid = uidMatch.Groups[1].Value;
                _sceneUids[scenePath] = uid;
            }

            // Extract references to other scenes
            var references = new List<string>();
            var refMatches = Regex.Matches(content, @"\[ext_resource[^\]]*path=""([^""]+)""[^\]]*id=""([^""]+)""");

            foreach (Match match in refMatches)
            {
                string referencedPath = match.Groups[1].Value;
                if (referencedPath.EndsWith(".tscn"))
                {
                    references.Add(referencedPath);
                }
            }

            _sceneReferences[scenePath] = references;
        }

        private void FindDuplicateUids()
        {
            var uidToScenes = new Dictionary<string, List<string>>();

            foreach (var pair in _sceneUids)
            {
                string scenePath = pair.Key;
                string uid = pair.Value;

                if (!uidToScenes.ContainsKey(uid))
                {
                    uidToScenes[uid] = new List<string>();
                }

                uidToScenes[uid].Add(scenePath);
            }

            foreach (var pair in uidToScenes)
            {
                string uid = pair.Key;
                var scenes = pair.Value;

                if (scenes.Count > 1)
                {
                    _duplicateUids[uid] = scenes;
                }
            }
        }

        private void GenerateReport()
        {
            var sb = new StringBuilder();

            // Title
            sb.AppendLine("# Signal Lost Scene Analysis Report");
            sb.AppendLine();
            sb.AppendLine($"Generated on: {DateTime.Now}");
            sb.AppendLine();

            // Duplicate UIDs
            sb.AppendLine("## Duplicate UIDs");
            sb.AppendLine();

            if (_duplicateUids.Count == 0)
            {
                sb.AppendLine("No duplicate UIDs found! 🎉");
            }
            else
            {
                sb.AppendLine("| UID | Scenes |");
                sb.AppendLine("|-----|--------|");

                foreach (var pair in _duplicateUids)
                {
                    string uid = pair.Key;
                    var scenes = pair.Value;

                    sb.AppendLine($"| `{uid}` | {string.Join("<br>", scenes)} |");
                }
            }

            sb.AppendLine();

            // Scene Hierarchy
            sb.AppendLine("## Scene Hierarchy");
            sb.AppendLine();
            sb.AppendLine("```mermaid");
            sb.AppendLine("graph TD");

            // Add nodes
            foreach (var scenePath in _sceneUids.Keys)
            {
                string nodeId = GetNodeId(scenePath);
                string displayName = Path.GetFileNameWithoutExtension(scenePath);

                sb.AppendLine($"    {nodeId}[\"{displayName}\"]");
            }

            // Add connections
            foreach (var pair in _sceneReferences)
            {
                string scenePath = pair.Key;
                var references = pair.Value;

                string sourceNodeId = GetNodeId(scenePath);

                foreach (var referencedPath in references)
                {
                    string targetNodeId = GetNodeId(referencedPath);
                    sb.AppendLine($"    {sourceNodeId} --> {targetNodeId}");
                }
            }

            sb.AppendLine("```");
            sb.AppendLine();

            // Scene Details
            sb.AppendLine("## Scene Details");
            sb.AppendLine();

            foreach (var scenePath in _sceneUids.Keys.OrderBy(p => p))
            {
                string uid = _sceneUids[scenePath];
                var references = _sceneReferences[scenePath];

                sb.AppendLine($"### {Path.GetFileName(scenePath)}");
                sb.AppendLine();
                sb.AppendLine($"- **Path:** `{scenePath}`");
                sb.AppendLine($"- **UID:** `{uid}`");
                sb.AppendLine($"- **References ({references.Count}):**");

                if (references.Count > 0)
                {
                    foreach (var referencedPath in references)
                    {
                        sb.AppendLine($"  - `{referencedPath}`");
                    }
                }
                else
                {
                    sb.AppendLine("  - *None*");
                }

                sb.AppendLine();
            }

            // Write to file
            var file = FileAccess.Open(OutputPath, FileAccess.ModeFlags.Write);
            if (file != null)
            {
                file.StoreString(sb.ToString());
                file.Close();
                GD.Print($"Report generated at: {OutputPath}");
            }
            else
            {
                GD.PrintErr($"Failed to write report to: {OutputPath}");
            }
        }

        private static string GetNodeId(string path)
        {
            // Convert path to a valid mermaid node ID
            return "node_" + path.Replace(":", "_").Replace("/", "_").Replace(".", "_");
        }
    }
}
