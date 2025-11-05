using System.Collections.Generic;

namespace SilentTestimony.Dialogue
{
    public class DialogueState
    {
        public Dictionary<string, bool> Flags { get; } = new();

        public bool GetFlag(string key) => !string.IsNullOrEmpty(key) && Flags.TryGetValue(key, out var v) && v;
        public void SetFlag(string key, bool value)
        {
            if (string.IsNullOrEmpty(key)) return;
            Flags[key] = value;
        }
    }
}

