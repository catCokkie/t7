using Godot;
using System.Collections.Generic;
using SilentTestimony.Data;

namespace SilentTestimony.Systems
{
    /// <summary>
    /// 管理已收集的证据（Autoload）
    /// </summary>
    public partial class EvidenceManager : Node
    {
        [Signal] public delegate void EvidenceAddedEventHandler(string evidenceID);

        private readonly Dictionary<string, EvidenceData> _byId = new();
        private readonly List<EvidenceData> _list = new();

        public bool AddEvidence(EvidenceData evidence)
        {
            if (evidence == null || string.IsNullOrEmpty(evidence.EvidenceID))
            {
                GD.PushWarning("EvidenceManager: 无效的 EvidenceData");
                return false;
            }

            if (_byId.ContainsKey(evidence.EvidenceID))
            {
                return false; // 已存在
            }

            _byId[evidence.EvidenceID] = evidence;
            _list.Add(evidence);
            EmitSignal(SignalName.EvidenceAdded, evidence.EvidenceID);
            return true;
        }

        public bool HasEvidence(string evidenceId)
        {
            return !string.IsNullOrEmpty(evidenceId) && _byId.ContainsKey(evidenceId);
        }

        public IReadOnlyList<EvidenceData> GetAll()
        {
            return _list.AsReadOnly();
        }

        public EvidenceData GetById(string evidenceId)
        {
            return evidenceId != null && _byId.TryGetValue(evidenceId, out var data) ? data : null;
        }
    }
}

