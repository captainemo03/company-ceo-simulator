using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CompanyCEO.UnityRemake
{
    public enum LibertyBayWeather { Clear, Rain, Storm, Heatwave, EconomicFog }
    public enum CityTransportMode { Walking, CompanyCar, PublicTransit }

    [Serializable]
    public sealed class CityCompanyPresence
    {
        public string companyId;
        public string displayName;
        public string sector;
        public Vector3 headquarters;
        public int employees = 25;
        public bool factory;
        public bool terminal;
    }

    /// <summary>
    /// First Liberty Bay slice: a bounded 500 x 500 business district with
    /// explicit asset hooks, time/weather state, rival presence and traffic
    /// routes. City art is loaded from Resources/City; no human placeholder is
    /// ever generated when an optional licensed asset is missing.
    /// </summary>
    public sealed class LibertyBayWorld : MonoBehaviour
    {
        [SerializeField] private float districtSize = 500f;
        [SerializeField] private float blockSize = 50f;
        [SerializeField] private float roadWidth = 8f;
        [SerializeField] private LibertyBayWeather weather = LibertyBayWeather.Clear;
        [SerializeField] private bool buildScaffoldWhenAssetsAreMissing = true;
        [SerializeField] private List<CityCompanyPresence> companies = new();
        private readonly List<Vector3> trafficWaypoints = new();
        private Transform cityRoot;
        private Light sun;
        private float lastWeatherRoll;
        private bool warnedMissingCityAssets;

        public float DistrictSize => districtSize;
        public LibertyBayWeather Weather => weather;
        public IReadOnlyList<CityCompanyPresence> Companies => companies;
        public IReadOnlyList<Vector3> TrafficWaypoints => trafficWaypoints;

        private void Start()
        {
            SeedCompanyPresence();
            BuildDistrict();
            BuildLighting();
            var clock = CompanyTimeSystem.Instance;
            if (clock != null) clock.MinuteAdvanced += OnCompanyMinute;
        }

        private void OnDestroy()
        {
            if (CompanyTimeSystem.Instance != null) CompanyTimeSystem.Instance.MinuteAdvanced -= OnCompanyMinute;
        }

        private void SeedCompanyPresence()
        {
            if (companies.Count > 0) return;
            companies.Add(new CityCompanyPresence { companyId = "player", displayName = "Player Company", sector = "Technology", headquarters = new Vector3(-70f, 0f, 40f), employees = 120 });
            companies.Add(new CityCompanyPresence { companyId = "northstar", displayName = "Northstar Labs", sector = "Software", headquarters = new Vector3(90f, 0f, 65f), employees = 210 });
            companies.Add(new CityCompanyPresence { companyId = "meridian", displayName = "Meridian Manufacturing", sector = "Manufacturing", headquarters = new Vector3(-120f, 0f, -100f), employees = 480, factory = true });
            companies.Add(new CityCompanyPresence { companyId = "atlas", displayName = "Atlas Logistics", sector = "Logistics", headquarters = new Vector3(145f, 0f, -120f), employees = 160, terminal = true });
        }

        private void BuildDistrict()
        {
            cityRoot = new GameObject("Liberty Bay · Business District 500x500").transform;
            CreateGround();
            CreateRoadGrid();
            foreach (var company in companies) CreateCompanyPresence(company);
            BuildTrafficNetwork();
            Debug.Log("Liberty Bay district initialized: 500x500m business district, rival presence and traffic routes ready.");
        }

        private void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Liberty Bay ground (scaffold)";
            ground.transform.SetParent(cityRoot);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * (districtSize / 10f);
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = new Color(.035f, .06f, .08f);
        }

        private void CreateRoadGrid()
        {
            var extent = districtSize * .5f;
            for (var axis = -extent; axis <= extent; axis += blockSize)
            {
                CreateRoad(new Vector3(axis, .015f, 0f), new Vector3(roadWidth, .04f, districtSize));
                CreateRoad(new Vector3(0f, .02f, axis), new Vector3(districtSize, .04f, roadWidth));
                trafficWaypoints.Add(new Vector3(axis, .2f, -extent + roadWidth));
                trafficWaypoints.Add(new Vector3(axis, .2f, extent - roadWidth));
                trafficWaypoints.Add(new Vector3(-extent + roadWidth, .2f, axis));
                trafficWaypoints.Add(new Vector3(extent - roadWidth, .2f, axis));
            }
        }

        private void CreateRoad(Vector3 position, Vector3 size)
        {
            var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Liberty Bay road scaffold";
            road.transform.SetParent(cityRoot);
            road.transform.position = position;
            road.transform.localScale = size;
            var renderer = road.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = new Color(.06f, .07f, .09f);
        }

        private void CreateCompanyPresence(CityCompanyPresence company)
        {
            var loaded = Resources.Load<GameObject>("City/Companies/" + company.companyId + "/Headquarters");
            if (loaded != null)
            {
                var instance = Instantiate(loaded, company.headquarters, Quaternion.identity, cityRoot);
                instance.name = company.displayName + " · licensed headquarters";
            }
            else if (buildScaffoldWhenAssetsAreMissing)
            {
                CreateBuildingScaffold(company);
                if (!warnedMissingCityAssets)
                {
                    warnedMissingCityAssets = true;
                    Debug.LogWarning("No licensed city building pack was found under Resources/City/Companies. Liberty Bay uses clearly named building scaffolds until assets are added.");
                }
            }

            if (company.factory) CreateOptionalAsset(company, "Factory", "factory");
            if (company.terminal) CreateOptionalAsset(company, "ContainerTerminal", "container terminal");
        }

        private void CreateOptionalAsset(CityCompanyPresence company, string assetName, string label)
        {
            var asset = Resources.Load<GameObject>("City/Companies/" + company.companyId + "/" + assetName);
            if (asset == null)
            {
                Debug.LogWarning($"Missing licensed {label} asset for {company.displayName}; logistics data remains available but no visual proxy is claimed as final art.");
                return;
            }
            Instantiate(asset, company.headquarters + new Vector3(18f, 0f, 0f), Quaternion.identity, cityRoot);
        }

        private void CreateBuildingScaffold(CityCompanyPresence company)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = company.displayName + " · building scaffold (replace with licensed asset)";
            block.transform.SetParent(cityRoot);
            block.transform.position = company.headquarters + Vector3.up * 6f;
            block.transform.localScale = new Vector3(22f, 12f, 18f);
            var renderer = block.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = company.companyId == "player" ? new Color(.08f, .35f, .65f) : new Color(.22f, .25f, .29f);
        }

        private void BuildTrafficNetwork()
        {
            var root = new GameObject("Traffic network · route points").transform;
            root.SetParent(cityRoot);
            for (var i = 0; i < trafficWaypoints.Count; i++)
            {
                var point = new GameObject("Traffic waypoint " + i).transform;
                point.SetParent(root);
                point.position = trafficWaypoints[i];
            }
        }

        private void BuildLighting()
        {
            sun = RenderSettings.sun;
            if (sun == null)
            {
                var lightObject = new GameObject("Liberty Bay Sun");
                sun = lightObject.AddComponent<Light>();
                sun.type = LightType.Directional;
                RenderSettings.sun = sun;
            }
            sun.shadows = LightShadows.Soft;
            UpdateLighting(CompanyTimeSystem.Instance?.CompanyMinutes ?? 9f * 60f);
        }

        private void OnCompanyMinute(float delta)
        {
            var minutes = CompanyTimeSystem.Instance?.CompanyMinutes ?? 0f;
            UpdateLighting(minutes);
            if (minutes - lastWeatherRoll > 90f)
            {
                lastWeatherRoll = minutes;
                RollWeather(minutes);
            }
        }

        private void UpdateLighting(float minutes)
        {
            if (sun == null) return;
            var normalized = (minutes - 360f) / 900f;
            var angle = Mathf.Lerp(-20f, 200f, Mathf.Clamp01(normalized));
            sun.transform.rotation = Quaternion.Euler(angle, -30f, 0f);
            sun.intensity = Mathf.Lerp(.18f, 1.15f, Mathf.Clamp01(Mathf.Sin(normalized * Mathf.PI)));
            RenderSettings.ambientIntensity = Mathf.Lerp(.22f, 1f, Mathf.Clamp01(Mathf.Sin(normalized * Mathf.PI)));
        }

        private void RollWeather(float minutes)
        {
            var roll = Mathf.Abs(Mathf.FloorToInt(minutes * .13f)) % 100;
            weather = roll < 58 ? LibertyBayWeather.Clear : roll < 78 ? LibertyBayWeather.Rain : roll < 88 ? LibertyBayWeather.Heatwave : roll < 95 ? LibertyBayWeather.EconomicFog : LibertyBayWeather.Storm;
            Debug.Log($"Liberty Bay weather changed: {weather}");
        }

        public bool TryGetNearestTrafficWaypoint(Vector3 from, out Vector3 waypoint)
        {
            waypoint = Vector3.zero;
            var best = float.MaxValue;
            foreach (var candidate in trafficWaypoints)
            {
                var distance = (candidate - from).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                waypoint = candidate;
            }
            return best < float.MaxValue;
        }
    }

    /// <summary>Autonomous route follower used by licensed vehicle prefabs.</summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class TrafficVehicleAgent : MonoBehaviour
    {
        [SerializeField] private float cruiseSpeed = 7f;
        [SerializeField] private float waypointRadius = 3f;
        private NavMeshAgent navigation;
        private LibertyBayWorld world;
        private int waypoint;
        private readonly List<Vector3> route = new();

        private void Start()
        {
            navigation = GetComponent<NavMeshAgent>();
            world = FindFirstObjectByType<LibertyBayWorld>();
            if (world == null || world.TrafficWaypoints.Count == 0) return;
            route.AddRange(world.TrafficWaypoints);
            waypoint = Mathf.Abs(GetInstanceID()) % route.Count;
            navigation.speed = cruiseSpeed;
            navigation.angularSpeed = 180f;
            navigation.acceleration = 12f;
            navigation.SetDestination(route[waypoint]);
        }

        private void Update()
        {
            if (navigation == null || route.Count == 0 || !navigation.isOnNavMesh) return;
            if (navigation.pathPending || navigation.remainingDistance > waypointRadius) return;
            waypoint = (waypoint + 1) % route.Count;
            navigation.SetDestination(route[waypoint]);
        }
    }

    /// <summary>CEO travel state; uses walking until a licensed car prefab is configured.</summary>
    public sealed class CEOCommuteController : MonoBehaviour
    {
        [SerializeField] private CityTransportMode mode = CityTransportMode.Walking;
        [SerializeField] private Transform destination;
        [SerializeField] private GameObject licensedCompanyCarPrefab;
        private AutonomousOfficeAgent ceo;
        private NavMeshAgent navigation;
        public CityTransportMode Mode => mode;

        private void Start()
        {
            ceo = FindFirstObjectByType<AutonomousOfficeAgent>();
            if (ceo != null) navigation = ceo.GetComponent<NavMeshAgent>();
            if (destination == null)
            {
                var world = FindFirstObjectByType<LibertyBayWorld>();
                if (world != null && world.TryGetNearestTrafficWaypoint(ceo == null ? Vector3.zero : ceo.transform.position, out var waypoint))
                {
                    var target = new GameObject("CEO commute destination");
                    target.transform.position = waypoint;
                    destination = target.transform;
                }
            }
        }

        public void SetTransport(CityTransportMode nextMode)
        {
            mode = nextMode;
            if (mode == CityTransportMode.CompanyCar && licensedCompanyCarPrefab == null)
                Debug.LogWarning("Company car mode selected, but no licensed vehicle prefab is configured. CEO remains on foot until the asset is installed.");
        }

        public void CommuteTo(Transform nextDestination)
        {
            destination = nextDestination;
            if (navigation != null && navigation.isOnNavMesh && destination != null) navigation.SetDestination(destination.position);
        }
    }
}
