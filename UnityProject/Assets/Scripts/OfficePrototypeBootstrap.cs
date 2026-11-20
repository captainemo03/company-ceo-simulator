using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace CompanyCEO.UnityRemake
{
    /// <summary>Creates the first two-character autonomous office scene without geometric human placeholders.</summary>
    public sealed class OfficePrototypeBootstrap : MonoBehaviour
    {
        private readonly List<AutonomousOfficeAgent> agents = new();
        private readonly List<Transform> targets = new();
        private Transform targetRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<OfficePrototypeBootstrap>() != null) return;
            var root = new GameObject("Company CEO Simulator · Unity Prototype");
            DontDestroyOnLoad(root);
            root.AddComponent<OfficePrototypeBootstrap>();
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            BuildTargets();
            LoadOfficeAssets();
            var city = gameObject.AddComponent<LibertyBayWorld>();
            city.BuildNow();
            BuildNavigationSurfaceIfAvailable();
            var profiles = new[]
            {
                new OfficeAgentProfile { id = "ceo", displayName = "Daniel Brooks", role = "CEO", initialState = OfficeAgentState.Working },
                new OfficeAgentProfile { id = "cfo", displayName = "Sophia Bennett", role = "CFO", initialState = OfficeAgentState.Entering },
                new OfficeAgentProfile { id = "coo", displayName = "Marcus Reed", role = "COO" },
                new OfficeAgentProfile { id = "cmo", displayName = "Elena Carter", role = "Marketing" },
                new OfficeAgentProfile { id = "chro", displayName = "Rebecca Stone", role = "HR" },
                new OfficeAgentProfile { id = "secretary", displayName = "Aylin Kaya", role = "Secretary" },
                new OfficeAgentProfile { id = "analyst", displayName = "Ethan Walker", role = "Finance" },
                new OfficeAgentProfile { id = "engineer", displayName = "Nathan Cross", role = "R&D" },
                new OfficeAgentProfile { id = "sales", displayName = "Pelin Kurt", role = "Sales" },
                new OfficeAgentProfile { id = "operations", displayName = "Ozan Tunç", role = "Operations" }
            };
            var positions = new[] { new Vector3(-3.2f, 0f, 1.2f), new Vector3(-1.4f, 0f, -2.4f), new Vector3(-.6f, 0f, -2.4f), new Vector3(.2f, 0f, -2.4f), new Vector3(1f, 0f, -2.4f), new Vector3(1.8f, 0f, -2.4f), new Vector3(2.6f, 0f, -2.4f), new Vector3(3.4f, 0f, -2.4f), new Vector3(4.2f, 0f, -2.4f), new Vector3(5f, 0f, -2.4f) };
            for (var i = 0; i < profiles.Length; i++) SpawnAgent(i % 2 == 0 ? "Characters/CEO" : "Characters/FinanceManager", profiles[i], positions[i], i);
            SetupCamera();
            gameObject.AddComponent<MeetingSystem>();
            gameObject.AddComponent<AutomaticSceneDirector>();
            gameObject.AddComponent<NpcInspectionPanel>();
            gameObject.AddComponent<PerformanceBudget>();
            gameObject.AddComponent<CEOCommuteController>();
            Debug.Log("Company CEO Unity prototype initialized with 10 independent real-GLB agents.");
        }

        private void BuildTargets()
        {
            targetRoot = new GameObject("Office Targets").transform;
            targets.Add(Target("CEO Office", new Vector3(-3.2f, 0f, 1.2f)));
            targets.Add(Target("Finance Desk", new Vector3(-1.4f, 0f, 1.2f)));
            targets.Add(Target("Corridor", new Vector3(0f, 0f, -1.2f)));
            targets.Add(Target("Boardroom", new Vector3(2.8f, 0f, 1.2f)));
            targets.Add(Target("Coffee Area", new Vector3(2.8f, 0f, -1.8f)));
        }

        private Transform Target(string label, Vector3 position)
        {
            var t = new GameObject(label).transform;
            t.SetParent(targetRoot);
            t.position = position;
            return t;
        }

        private void LoadOfficeAssets()
        {
            var root = new GameObject("Licensed Office Environment");
            InstantiateResource("Office/MeetingRoom", root.transform, new Vector3(2.8f, 0f, 1.2f), .72f);
            InstantiateResource("Office/CubiclePod", root.transform, new Vector3(-1.8f, 0f, 1.3f), .82f);
            InstantiateResource("Office/Reception", root.transform, new Vector3(3.8f, 0f, -2.2f), .62f);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Office Navigation Floor";
            floor.transform.localScale = new Vector3(1.2f, 1f, .8f);
            floor.GetComponent<Renderer>().material.color = new Color(.08f, .12f, .22f);
            CreateSmartObject("Finance Computers", SmartObjectAction.Work, new Vector3(-1.4f, 0f, 1.2f), 2);
            CreateSmartObject("Coffee Machine", SmartObjectAction.Drink, new Vector3(2.8f, 0f, -1.8f), 1);
            CreateSmartObject("Boardroom Table", SmartObjectAction.Present, new Vector3(2.8f, 0f, 1.2f), 6);
            CreateSmartObject("Reception Phone", SmartObjectAction.Phone, new Vector3(3.8f, 0f, -2.2f), 1);
        }

        private static void CreateSmartObject(string label, SmartObjectAction action, Vector3 position, int capacity)
        {
            var root = new GameObject(label);
            root.transform.position = position;
            var smart = root.AddComponent<SmartObject>();
            smart.name = label;
            smart.Configure(action, capacity);
            var marker = new GameObject("Interaction Point").transform;
            marker.SetParent(root.transform);
            marker.localPosition = Vector3.zero;
        }

        private static GameObject InstantiateResource(string path, Transform parent, Vector3 position, float scale)
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"Missing real 3D asset: Resources/{path}.glb. Install glTFast and reimport the GLB.");
                return null;
            }
            var instance = Instantiate(prefab, position, Quaternion.identity, parent);
            instance.transform.localScale = Vector3.one * scale;
            return instance;
        }

        private Transform[] BuildAgentRoute(int index)
        {
            var route = new Transform[targets.Count];
            var offset = (index % 5 - 2) * .28f;
            for (var i = 0; i < targets.Count; i++)
            {
                var t = Target($"Agent {index} target {i}", targets[i].position + new Vector3(offset, 0f, (index % 2 == 0 ? .12f : -.12f)));
                route[i] = t;
            }
            return route;
        }

        private void SpawnAgent(string resourcePath, OfficeAgentProfile profile, Vector3 position, int index)
        {
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogError($"Missing real human model: Resources/{resourcePath}.glb. No placeholder will be created.");
                return;
            }
            var instance = Instantiate(prefab, position, Quaternion.identity);
            instance.name = profile.displayName + " · " + profile.role;
            instance.transform.localScale = Vector3.one * .72f;
            var nav = instance.GetComponent<NavMeshAgent>() ?? instance.AddComponent<NavMeshAgent>();
            nav.baseOffset = 0f;
            var agent = instance.GetComponent<AutonomousOfficeAgent>() ?? instance.AddComponent<AutonomousOfficeAgent>();
            agent.Configure(profile, BuildAgentRoute(index));
            agents.Add(agent);
        }

        private static void BuildNavigationSurfaceIfAvailable()
        {
            var surfaceType = Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
            if (surfaceType == null)
            {
                Debug.LogWarning("AI Navigation package is not available yet; install com.unity.ai.navigation before baking the office NavMesh.");
                return;
            }
            var go = new GameObject("Office NavMesh Surface");
            var surface = go.AddComponent(surfaceType);
            surfaceType.GetMethod("BuildNavMesh", BindingFlags.Instance | BindingFlags.Public)?.Invoke(surface, null);
        }

        private static void SetupCamera()
        {
            var camera = Camera.main ?? new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 7f, -9f);
            camera.transform.rotation = Quaternion.Euler(32f, 0f, 0f);
            var rig = camera.gameObject.GetComponent<PrototypeCameraRig>() ?? camera.gameObject.AddComponent<PrototypeCameraRig>();
        }
    }
}
