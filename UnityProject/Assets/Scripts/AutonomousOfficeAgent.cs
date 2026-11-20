using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace CompanyCEO.UnityRemake
{
    public enum OfficeAgentState
    {
        Entering, Working, Walking, Talking, Meeting, DrinkingCoffee, PhoneCall, Sitting, Leaving
    }

    [Serializable]
    public sealed class OfficeAgentProfile
    {
        public string id;
        public string displayName;
        public string role;
        public OfficeAgentState initialState = OfficeAgentState.Entering;
        public float decisionJitter = 2f;
        public float activityDuration = 8f;
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class AutonomousOfficeAgent : MonoBehaviour
    {
        [SerializeField] private OfficeAgentProfile profile = new OfficeAgentProfile();
        [SerializeField] private NavMeshAgent navigation;
        [SerializeField] private Animator animator;
        [SerializeField] private OfficeAgentState state;
        [SerializeField] private EmployeeNeeds needs = new EmployeeNeeds();
        [SerializeField] private EmployeeMemory memory;
        private Transform[] route;
        private int routeIndex;
        private float nextDecision;
        private Vector3 spawnPosition;
        private Vector3 lastPosition;
        private float stuckSeconds;

        public OfficeAgentProfile Profile => profile;
        public OfficeAgentState State => state;
        public EmployeeNeeds Needs => needs;
        public EmployeeMemory Memory => memory;
        public Vector3 SpawnPosition => spawnPosition;
        public Vector3 CurrentTarget => route != null && route.Length > 0 ? route[Mathf.Clamp(routeIndex, 0, route.Length - 1)].position : transform.position;
        public float StuckSeconds => stuckSeconds;

        public void Configure(OfficeAgentProfile nextProfile, Transform[] routeTargets)
        {
            profile = nextProfile ?? new OfficeAgentProfile();
            route = routeTargets ?? Array.Empty<Transform>();
            routeIndex = 0;
            state = profile.initialState;
            spawnPosition = transform.position;
            lastPosition = transform.position;
            nextDecision = Time.time + 1f + UnityEngine.Random.Range(0f, profile.decisionJitter);
            navigation ??= GetComponent<NavMeshAgent>();
            navigation.stoppingDistance = .55f;
            navigation.speed = 1.1f + UnityEngine.Random.Range(-.12f, .18f);
            navigation.acceleration = 5f;
            navigation.angularSpeed = 360f;
            navigation.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            navigation.avoidancePriority = 20 + UnityEngine.Random.Range(0, 60);
            if (animator == null) animator = GetComponentInChildren<Animator>();
            memory ??= GetComponent<EmployeeMemory>() ?? gameObject.AddComponent<EmployeeMemory>();
            SetState(state);
        }

        private void Awake()
        {
            navigation ??= GetComponent<NavMeshAgent>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            memory ??= GetComponent<EmployeeMemory>() ?? gameObject.AddComponent<EmployeeMemory>();
        }

        private void Update()
        {
            if (route == null || route.Length == 0 || navigation == null || !navigation.isOnNavMesh) return;
            var timeScale = CompanyTimeSystem.Speed;
            needs.Tick(Time.deltaTime * timeScale, state == OfficeAgentState.Working || state == OfficeAgentState.Meeting);
            if (Time.time >= nextDecision - profile.activityDuration * .15f && navigation.remainingDistance <= navigation.stoppingDistance)
            {
                AdvanceActivity();
                return;
            }
            if (navigation.pathPending) return;
            if (navigation.remainingDistance <= navigation.stoppingDistance)
            {
                if (Time.time >= nextDecision) AdvanceActivity();
                else SetState(state == OfficeAgentState.Entering ? OfficeAgentState.Working : state);
            }
            else SetState(OfficeAgentState.Walking);

            var moved = Vector3.Distance(transform.position, lastPosition);
            stuckSeconds = moved < .002f && navigation.hasPath ? stuckSeconds + Time.deltaTime : 0f;
            lastPosition = transform.position;
            if (stuckSeconds > 10f)
            {
                stuckSeconds = 0f;
                navigation.ResetPath();
                AdvanceActivity(true);
            }
        }

        private void AdvanceActivity(bool forced = false)
        {
            if (route.Length == 0) return;
            routeIndex = (routeIndex + 1) % route.Length;
            var target = route[routeIndex];
            if (target == null) return;
            navigation.SetDestination(target.position);
            var selected = UtilityAIDecision.Choose(needs, profile.role, routeIndex + profile.id.GetHashCode());
            state = selected switch
            {
                UtilityActivity.Drink => OfficeAgentState.DrinkingCoffee,
                UtilityActivity.Meeting => OfficeAgentState.Meeting,
                UtilityActivity.Phone => OfficeAgentState.PhoneCall,
                UtilityActivity.Socialize => OfficeAgentState.Talking,
                UtilityActivity.Eat => OfficeAgentState.Sitting,
                UtilityActivity.Rest => OfficeAgentState.Sitting,
                _ => OfficeAgentState.Working
            };
            memory?.Remember("activity", $"{profile.displayName} selected {selected}", 20f);
            nextDecision = Time.time + profile.activityDuration + UnityEngine.Random.Range(-profile.decisionJitter, profile.decisionJitter);
            if (forced) navigation.avoidancePriority = 10 + UnityEngine.Random.Range(0, 80);
        }

        private void SetState(OfficeAgentState next)
        {
            if (state != next) state = next;
            if (animator == null) return;
            animator.SetFloat("Speed", navigation == null ? 0f : navigation.velocity.magnitude);
            animator.SetInteger("State", (int)state);
            var clip = next switch
            {
                OfficeAgentState.Walking => "Walk",
                OfficeAgentState.Working => "Typing",
                OfficeAgentState.Talking => "Talking",
                OfficeAgentState.Meeting => "Listening",
                OfficeAgentState.DrinkingCoffee => "Drinking Coffee",
                OfficeAgentState.PhoneCall => "Phone Call",
                OfficeAgentState.Sitting => "Sitting",
                OfficeAgentState.Leaving => "Walk",
                _ => "Idle"
            };
            animator.CrossFadeInFixedTime(clip, .18f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, CurrentTarget);
            Gizmos.DrawWireSphere(CurrentTarget, .18f);
        }
    }
}
