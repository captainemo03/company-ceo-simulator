using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    public sealed class NpcInspectionPanel : MonoBehaviour
    {
        [SerializeField] private AutonomousOfficeAgent selected;
        public AutonomousOfficeAgent Selected => selected;
        public void Select(AutonomousOfficeAgent agent) => selected = agent;
        public string Summary()
        {
            if (selected == null) return "No employee selected";
            var p = selected.Profile;
            return $"{p.displayName} · {p.role} · {selected.State} · motivation {selected.Needs.motivation:0} · stress {selected.Needs.stress:0}";
        }
    }
}
