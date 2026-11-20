using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    public sealed class PerformanceBudget : MonoBehaviour
    {
        [SerializeField] private int decisionAgentsPerFrame = 3;
        [SerializeField] private float lodDistance = 18f;
        private int cursor;
        private float refresh;

        private void Update()
        {
            refresh -= Time.deltaTime;
            if (refresh > 0f) return;
            refresh = .12f;
            var agents = FindObjectsByType<AutonomousOfficeAgent>(FindObjectsSortMode.None);
            if (agents.Length == 0) return;
            for (var i = 0; i < Mathf.Min(decisionAgentsPerFrame, agents.Length); i++)
            {
                var agent = agents[(cursor + i) % agents.Length];
                var distance = Camera.main == null ? 0f : Vector3.Distance(Camera.main.transform.position, agent.transform.position);
                var animator = agent.GetComponentInChildren<Animator>();
                if (animator != null && distance > lodDistance) animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }
            cursor = (cursor + decisionAgentsPerFrame) % agents.Length;
        }
    }
}
