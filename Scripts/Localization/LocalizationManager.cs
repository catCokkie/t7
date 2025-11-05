using System;
using System.Collections.Generic;
using Godot;

namespace SilentTestimony.Localization
{
    public partial class LocalizationManager : Node
    {
        [Export] public Godot.Collections.Array<string> CsvPaths { get; set; } = new()
        {
            "res://Resources/Localization/dialogue.csv"
        };

        public override void _Ready()
        {
            try
            {
                LoadCsvTranslations(CsvPaths);
                // 默认优先中文，其次英文
                if (HasLocale("zh") || HasLocale("zh_CN") || HasLocale("zh-CN"))
                    TranslationServer.SetLocale("zh");
                else if (HasLocale("en"))
                    TranslationServer.SetLocale("en");
            }
            catch (Exception e)
            {
                GD.PushWarning($"LocalizationManager: load error {e.Message}");
            }
        }

        private static bool HasLocale(string locale)
        {
            foreach (var tr in TranslationServer.GetLoadedLocales())
            {
                if (string.Equals(tr, locale, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void LoadCsvTranslations(IEnumerable<string> paths)
        {
            var bundles = new Dictionary<string, Translation>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in paths)
            {
                if (!Godot.FileAccess.FileExists(p))
                {
                    GD.PushWarning($"LocalizationManager: CSV not found: {p}");
                    continue;
                }

                using var fa = Godot.FileAccess.Open(p, Godot.FileAccess.ModeFlags.Read);
                if (fa == null) continue;
                string all = fa.GetAsText();
                if (string.IsNullOrEmpty(all)) continue;

                var lines = all.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
                if (lines.Length < 2) continue;

                var header = SplitCsvLine(lines[0]);
                if (header.Count < 2 || !string.Equals(header[0], "key", StringComparison.OrdinalIgnoreCase))
                {
                    GD.PushWarning($"LocalizationManager: invalid header in {p}");
                    continue;
                }

                var locales = new List<string>();
                for (int i = 1; i < header.Count; i++)
                {
                    var loc = header[i].Trim();
                    if (string.IsNullOrEmpty(loc)) continue;
                    locales.Add(loc);
                    if (!bundles.ContainsKey(loc))
                    {
                        var tr = new Translation();
                        tr.SetLocale(loc);
                        bundles[loc] = tr;
                    }
                }

                for (int li = 1; li < lines.Length; li++)
                {
                    var row = lines[li];
                    if (string.IsNullOrWhiteSpace(row)) continue;
                    var cols = SplitCsvLine(row);
                    if (cols.Count == 0) continue;
                    var key = cols[0];
                    for (int i = 0; i < locales.Count; i++)
                    {
                        int colIndex = i + 1;
                        if (colIndex >= cols.Count) continue;
                        var text = cols[colIndex];
                        if (!string.IsNullOrEmpty(key) && bundles.TryGetValue(locales[i], out var tr))
                        {
                            tr.AddMessage(key, text);
                        }
                    }
                }
            }

            foreach (var kv in bundles)
            {
                TranslationServer.AddTranslation(kv.Value);
            }
        }

        private static List<string> SplitCsvLine(string line)
        {
            var result = new List<string>();
            if (line == null) return result;
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }
            result.Add(current.ToString());
            return result;
        }
    }
}

