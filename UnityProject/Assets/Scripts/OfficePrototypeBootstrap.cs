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
            BuildNavigationSurfaceIfAvailable();
            SpawnAgent("Characters/CEO", new OfficeAgentProfile { id = "ceo", displayName = "Daniel Brooks", role = "CEO", initialState = OfficeAgentState.Working }, new Vector3(-3.2f, 0f, 1.2f));
            SpawnAgent("Characters/FinanceManager", new OfficeAgentProfile { id = "cfo", displayName = "Sophia Bennett", role = "CFO", initialState = OfficeAgentState.Entering }, new Vector3(-1.4f, 0f, -2.4f));
            SetupCamera();
            Debug.Log("Company CEO Unity prototype initialized with real GLB assets and two autonomous agents.");
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

        private void SpawnAgent(string resourcePath, OfficeAgentProfile profile, Vector3 position)
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
            agent.Configure(profile, targets.ToArray());
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
