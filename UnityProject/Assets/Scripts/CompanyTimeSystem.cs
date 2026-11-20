using System;
using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    public sealed class CompanyTimeSystem : MonoBehaviour
    {
        public static CompanyTimeSystem Instance { get; private set; }
        public static float Speed => Instance == null ? 1f : Instance.speed;
        [SerializeField] private float speed = 1f;
        [SerializeField] private bool paused;
        [SerializeField] private float companyMinutes = 8f * 60f + 45f;
        public bool Paused => paused;
        public float CompanyMinutes => companyMinutes;
        public event Action<float> MinuteAdvanced;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (Instance != null) return;
            var root = new GameObject("Company Time System");
            DontDestroyOnLoad(root);
            Instance = root.AddComponent<CompanyTimeSystem>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (paused) return;
            var delta = Time.deltaTime * speed;
            companyMinutes = (companyMinutes + delta) % 1440f;
            MinuteAdvanced?.Invoke(delta);
        }

        public void SetSpeed(float next) => speed = Mathf.Clamp(next, 0f, 10f);
        public void Pause() => paused = true;
        public void Resume() => paused = false;
    }
}
