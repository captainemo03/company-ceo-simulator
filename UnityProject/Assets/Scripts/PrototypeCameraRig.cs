using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    public sealed class PrototypeCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private float distance = 8f;
        [SerializeField] private float height = 5f;

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                var agents = FindObjectsByType<AutonomousOfficeAgent>(FindObjectsSortMode.None);
                if (agents.Length > 0) followTarget = agents[0].transform;
            }
            if (followTarget == null) return;
            var desired = followTarget.position + new Vector3(0f, height, -distance);
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * 2f);
            transform.LookAt(followTarget.position + Vector3.up * 1.1f);
        }
    }
}
