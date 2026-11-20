using System;
using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    public enum DirectorEventType { Meeting, InvestorVisit, Conflict, ProductLaunch, Crisis, CEOActivity }

    [Serializable]
    public sealed class DirectorEvent
    {
        public DirectorEventType type;
        public float priority;
        public Transform focus;
        public float expiresAt;
    }

    public sealed class AutomaticSceneDirector : MonoBehaviour
    {
        [SerializeField] private Camera directorCamera;
        [SerializeField] private bool automatic = true;
        private DirectorEvent activeEvent;
        public bool Automatic => automatic;

        private void Awake() => directorCamera ??= Camera.main;

        private void Update()
        {
            if (!automatic || activeEvent == null || Time.time > activeEvent.expiresAt || activeEvent.focus == null) return;
            var focus = activeEvent.focus.position + Vector3.up * 1.1f;
            var desired = focus + new Vector3(0f, 2.8f, -4.8f);
            directorCamera.transform.position = Vector3.Lerp(directorCamera.transform.position, desired, Time.deltaTime * 2.5f);
            directorCamera.transform.LookAt(focus);
        }

        public void Publish(DirectorEventType type, Transform focus, float priority, float duration = 8f)
        {
            if (activeEvent != null && activeEvent.priority > priority) return;
            activeEvent = new DirectorEvent { type = type, focus = focus, priority = priority, expiresAt = Time.time + duration };
        }

        public void SetAutomatic(bool enabled) => automatic = enabled;
    }
}
