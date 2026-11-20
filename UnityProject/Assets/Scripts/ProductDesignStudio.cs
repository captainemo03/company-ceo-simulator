using System;
using System.Collections.Generic;
using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    public enum ProductLifecycleStage { Development, Introduction, Growth, Maturity, Decline, Discontinued }
    public enum ProductSegment { Budget, Mainstream, Performance, Premium, Luxury, Professional }
    public enum SmartphoneColor { Graphite, Silver, Ocean, Forest, Rose }
    public enum SmartphoneMaterial { Plastic, Aluminium, Glass, Titanium }
    public enum ProductTestType { Performance, Durability, Battery, Thermal, Energy, Quality, Experience, Security, Reliability }

    [Serializable]
    public sealed class SmartphoneSpec
    {
        public string productName = "Liberty One";
        public string brandName = "Liberty Labs";
        public string modelNumber = "LL-01";
        public ProductSegment targetSegment = ProductSegment.Mainstream;
        [Range(1, 5)] public int processorTier = 3;
        [Range(1, 5)] public int gpuTier = 2;
        [Range(1, 5)] public int ramTier = 3;
        [Range(1, 5)] public int storageTier = 3;
        [Range(1, 5)] public int displayTier = 3;
        [Range(1, 5)] public int cameraTier = 3;
        [Range(1, 5)] public int batteryTier = 3;
        [Range(1, 5)] public int coolingTier = 2;
        [Range(1, 5)] public int softwareTier = 3;
        [Range(1, 5)] public int durabilityTier = 3;
        public SmartphoneMaterial material = SmartphoneMaterial.Aluminium;
        public SmartphoneColor color = SmartphoneColor.Graphite;
        [Range(0, 100)] public int waterResistance = 55;
        [Range(60, 240)] public int refreshRate = 120;
        [Range(20, 80)] public float screenInches = 6.4f;
        public int targetPrice = 799;
        public int researchBudget = 120000;
        public int developmentWeeks = 8;
    }

    [Serializable]
    public sealed class ProductMetrics
    {
        public float performance;
        public float quality;
        public float reliability;
        public float energyEfficiency;
        public float thermalRisk;
        public float compatibility;
        public float customerSatisfaction;
        public float unitCost;
        public float unitContribution;
        public float brandDemand;
        public string compatibilityNote;

        public ProductMetrics Clone() => (ProductMetrics)MemberwiseClone();
    }

    [Serializable]
    public sealed class ProductTestResult
    {
        public ProductTestType type;
        public float score;
        public bool passed;
        public float cost;
        public string note;
    }

    [Serializable]
    public sealed class ProductStudioSaveData
    {
        public SmartphoneSpec spec = new SmartphoneSpec();
        public ProductMetrics metrics = new ProductMetrics();
        public ProductLifecycleStage stage;
        public float researchProgress;
        public int cash;
        public int inventory;
        public int sold;
        public float brandAwareness;
        public List<ProductTestResult> tests = new();
    }

    /// <summary>
    /// Complete first playable product loop for the consumer-electronics scenario.
    /// All costs and scores are derived from the selected components; no button is decorative.
    /// </summary>
    public sealed class ProductDesignStudio : MonoBehaviour
    {
        [SerializeField] private SmartphoneSpec spec = new SmartphoneSpec();
        [SerializeField] private ProductMetrics metrics = new ProductMetrics();
        [SerializeField] private ProductLifecycleStage stage = ProductLifecycleStage.Development;
        [SerializeField] private float researchProgress;
        [SerializeField] private int cash = 1000000;
        [SerializeField] private int inventory;
        [SerializeField] private int sold;
        [SerializeField] private float brandAwareness = 35f;
        [SerializeField] private List<ProductTestResult> tests = new();
        private SmartphonePreview3D preview;
        private float simulationClock;

        public SmartphoneSpec Spec => spec;
        public ProductMetrics Metrics => metrics;
        public ProductLifecycleStage Stage => stage;
        public float ResearchProgress => researchProgress;
        public int Cash => cash;
        public int Inventory => inventory;
        public int Sold => sold;
        public IReadOnlyList<ProductTestResult> Tests => tests;
        public event Action Changed;

        private void Start()
        {
            Recalculate();
            var previewRoot = new GameObject("Product Design Studio · Smartphone Preview");
            preview = previewRoot.AddComponent<SmartphonePreview3D>();
            preview.Apply(spec);
            var cameraObject = new GameObject("Product Design Studio · Preview Camera");
            var camera = cameraObject.AddComponent<SmartphonePreviewCamera>();
            camera.Focus(previewRoot.transform);
            Load();
        }

        private void Update()
        {
            if (stage != ProductLifecycleStage.Development && stage != ProductLifecycleStage.Introduction) return;
            simulationClock += Time.deltaTime * CompanyTimeSystem.Speed;
            if (simulationClock < 8f) return;
            simulationClock = 0f;
            if (stage == ProductLifecycleStage.Development && researchProgress > 0f)
            {
                researchProgress = Mathf.Min(100f, researchProgress + 1.2f);
                if (researchProgress >= 100f && tests.Count >= 3) stage = ProductLifecycleStage.Introduction;
                Changed?.Invoke();
            }
        }

        public void Recalculate()
        {
            var raw = 18f + (int)spec.material * 8f;
            var cpu = spec.processorTier * 26f;
            var gpu = spec.gpuTier * 18f;
            var ram = spec.ramTier * 11f;
            var storage = spec.storageTier * 14f;
            var display = spec.displayTier * 19f;
            var camera = spec.cameraTier * 15f;
            var battery = spec.batteryTier * 12f;
            var cooling = spec.coolingTier * 9f;
            var software = spec.softwareTier * 6f;
            var labour = 34f + spec.durabilityTier * 7f;
            var energy = 7f + (spec.processorTier + spec.gpuTier + spec.refreshRate / 60f) * 1.4f;
            var packaging = 8f + (int)spec.material * 1.5f;
            var qualityControl = 4f + spec.durabilityTier * 3f;
            var logistics = 13f + (spec.screenInches - 6f) * 2f;
            metrics.unitCost = raw + cpu + gpu + ram + storage + display + camera + battery + cooling + software + labour + energy + packaging + qualityControl + logistics;
            metrics.performance = Mathf.Clamp(20f + spec.processorTier * 10f + spec.gpuTier * 8f + spec.ramTier * 5f + spec.displayTier * 3f, 0f, 100f);
            metrics.quality = Mathf.Clamp(18f + spec.material * 12f + spec.durabilityTier * 10f + spec.displayTier * 3f, 0f, 100f);
            metrics.reliability = Mathf.Clamp(24f + spec.durabilityTier * 11f + spec.coolingTier * 5f + spec.softwareTier * 3f - metrics.thermalRisk * .3f, 0f, 100f);
            var heat = spec.processorTier * 7f + spec.gpuTier * 6f + spec.refreshRate * .045f - spec.coolingTier * 9f;
            metrics.thermalRisk = Mathf.Clamp(heat, 0f, 100f);
            metrics.energyEfficiency = Mathf.Clamp(100f - (spec.processorTier * 7f + spec.gpuTier * 5f + spec.refreshRate * .12f) + spec.batteryTier * 5f, 0f, 100f);
            metrics.compatibility = metrics.thermalRisk > 58f && spec.coolingTier < 3 ? 62f : 96f;
            metrics.compatibilityNote = metrics.compatibility < 80f ? "Soğutma sistemi yük altında yetersiz; termal test riski yüksek." : "Bileşenler teknik olarak uyumlu.";
            var segmentFit = SegmentFit(spec.targetSegment, metrics);
            metrics.customerSatisfaction = Mathf.Clamp(segmentFit * .48f + metrics.quality * .2f + metrics.reliability * .18f + brandAwareness * .14f, 0f, 100f);
            metrics.unitContribution = spec.targetPrice - metrics.unitCost;
            metrics.brandDemand = Mathf.Clamp(brandAwareness * .6f + metrics.customerSatisfaction * .4f, 0f, 100f);
            Changed?.Invoke();
        }

        private static float SegmentFit(ProductSegment segment, ProductMetrics m)
        {
            return segment switch
            {
                ProductSegment.Budget => 100f - m.unitCost * .14f,
                ProductSegment.Mainstream => 55f + m.performance * .25f + m.quality * .2f,
                ProductSegment.Performance => m.performance * .7f + m.reliability * .3f,
                ProductSegment.Premium => m.quality * .55f + m.reliability * .25f + m.performance * .2f,
                ProductSegment.Luxury => m.quality * .7f + m.brandDemand * .3f,
                _ => m.performance * .5f + m.reliability * .5f
            };
        }

        public void CycleSegment()
        {
            spec.targetSegment = (ProductSegment)(((int)spec.targetSegment + 1) % Enum.GetValues(typeof(ProductSegment)).Length);
            Recalculate();
        }

        public void CycleMaterial()
        {
            spec.material = (SmartphoneMaterial)(((int)spec.material + 1) % Enum.GetValues(typeof(SmartphoneMaterial)).Length);
            Recalculate();
            preview?.Apply(spec);
        }

        public void CycleColor()
        {
            spec.color = (SmartphoneColor)(((int)spec.color + 1) % Enum.GetValues(typeof(SmartphoneColor)).Length);
            preview?.Apply(spec);
            Changed?.Invoke();
        }

        public void IncreasePerformance()
        {
            spec.processorTier = Mathf.Min(5, spec.processorTier + 1);
            spec.gpuTier = Mathf.Min(5, spec.gpuTier + 1);
            Recalculate();
            preview?.Apply(spec);
        }

        public void UpgradeComponent(string component)
        {
            switch (component)
            {
                case "CPU": spec.processorTier = Mathf.Min(5, spec.processorTier + 1); break;
                case "GPU": spec.gpuTier = Mathf.Min(5, spec.gpuTier + 1); break;
                case "RAM": spec.ramTier = Mathf.Min(5, spec.ramTier + 1); break;
                case "Storage": spec.storageTier = Mathf.Min(5, spec.storageTier + 1); break;
                case "Display": spec.displayTier = Mathf.Min(5, spec.displayTier + 1); break;
                case "Camera": spec.cameraTier = Mathf.Min(5, spec.cameraTier + 1); break;
                case "Battery": spec.batteryTier = Mathf.Min(5, spec.batteryTier + 1); break;
                case "Cooling": spec.coolingTier = Mathf.Min(5, spec.coolingTier + 1); break;
                case "Software": spec.softwareTier = Mathf.Min(5, spec.softwareTier + 1); break;
                case "Durability": spec.durabilityTier = Mathf.Min(5, spec.durabilityTier + 1); break;
            }
            Recalculate();
            preview?.Apply(spec);
        }

        public void StartResearch()
        {
            if (stage != ProductLifecycleStage.Development || cash < spec.researchBudget) return;
            cash -= spec.researchBudget;
            researchProgress = Mathf.Max(researchProgress, 1f);
            Changed?.Invoke();
        }

        public void SimulateWeek()
        {
            if (stage != ProductLifecycleStage.Development || researchProgress <= 0f) return;
            researchProgress = Mathf.Min(100f, researchProgress + Mathf.Clamp(100f / spec.developmentWeeks, 4f, 35f));
            if (researchProgress >= 100f && tests.Count >= 3) stage = ProductLifecycleStage.Introduction;
            Changed?.Invoke();
        }

        public ProductTestResult RunTest(ProductTestType type)
        {
            if (researchProgress < 25f || cash < 2500) return null;
            var score = type switch
            {
                ProductTestType.Performance => metrics.performance - metrics.thermalRisk * .25f,
                ProductTestType.Durability => metrics.quality,
                ProductTestType.Battery => metrics.energyEfficiency,
                ProductTestType.Thermal => 100f - metrics.thermalRisk,
                ProductTestType.Energy => metrics.energyEfficiency,
                ProductTestType.Quality => metrics.quality,
                ProductTestType.Experience => metrics.customerSatisfaction,
                ProductTestType.Security => spec.softwareTier * 14f + spec.durabilityTier * 6f,
                _ => metrics.reliability
            };
            cash -= 2500;
            var result = new ProductTestResult { type = type, score = Mathf.Clamp(score, 0f, 100f), cost = 2500, passed = score >= 60f, note = score >= 60f ? "Test başarılı" : "Test başarısız; ürün Ar-Ge’ye dönmeli." };
            tests.RemoveAll(x => x.type == type);
            tests.Add(result);
            if (!result.passed) researchProgress = Mathf.Max(25f, researchProgress - 8f);
            if (researchProgress >= 100f && tests.Count >= 3 && tests.TrueForAll(x => x.passed)) stage = ProductLifecycleStage.Introduction;
            Changed?.Invoke();
            return result;
        }

        public bool StartProduction(int quantity)
        {
            if (stage != ProductLifecycleStage.Introduction || quantity <= 0 || metrics.unitContribution <= 0f) return false;
            var total = Mathf.CeilToInt(metrics.unitCost * quantity);
            if (cash < total) return false;
            cash -= total;
            inventory += quantity;
            Changed?.Invoke();
            return true;
        }

        public int LaunchAndSell()
        {
            if (stage != ProductLifecycleStage.Introduction || inventory <= 0) return 0;
            var pricePenalty = Mathf.Clamp01((spec.targetPrice - 300f) / 1300f);
            var demand = Mathf.Max(0, Mathf.RoundToInt((metrics.brandDemand * 14f + metrics.customerSatisfaction * 10f) * (1f - pricePenalty * .45f)));
            var units = Mathf.Min(inventory, demand);
            inventory -= units;
            sold += units;
            cash += Mathf.RoundToInt(units * spec.targetPrice);
            brandAwareness = Mathf.Clamp(brandAwareness + units * .015f, 0f, 100f);
            stage = sold > 0 ? ProductLifecycleStage.Growth : stage;
            Recalculate();
            return units;
        }

        public void Save()
        {
            var data = new ProductStudioSaveData { spec = spec, metrics = metrics, stage = stage, researchProgress = researchProgress, cash = cash, inventory = inventory, sold = sold, brandAwareness = brandAwareness, tests = tests };
            PlayerPrefs.SetString("company_ceo_product_studio", JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey("company_ceo_product_studio")) return;
            var data = JsonUtility.FromJson<ProductStudioSaveData>(PlayerPrefs.GetString("company_ceo_product_studio"));
            if (data == null) return;
            spec = data.spec ?? new SmartphoneSpec(); metrics = data.metrics ?? new ProductMetrics(); stage = data.stage; researchProgress = data.researchProgress; cash = data.cash; inventory = data.inventory; sold = data.sold; brandAwareness = data.brandAwareness; tests = data.tests ?? new List<ProductTestResult>();
            Recalculate();
            preview?.Apply(spec);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(18f, 18f, 470f, 660f), GUI.skin.box);
            GUILayout.Label("PRODUCT DESIGN STUDIO · CONSUMER ELECTRONICS");
            GUILayout.Label("Ürün adı / marka / model");
            var productName = GUILayout.TextField(spec.productName);
            var brandName = GUILayout.TextField(spec.brandName);
            var modelNumber = GUILayout.TextField(spec.modelNumber);
            if (productName != spec.productName || brandName != spec.brandName || modelNumber != spec.modelNumber)
            {
                spec.productName = productName; spec.brandName = brandName; spec.modelNumber = modelNumber; Changed?.Invoke();
            }
            GUILayout.Label($"{spec.brandName} {spec.productName} · {stage} · Nakit {cash:N0} TRY");
            GUILayout.Label($"Segment: {spec.targetSegment} | Malzeme: {spec.material} | Renk: {spec.color}");
            GUILayout.Label($"CPU {spec.processorTier}/5 · GPU {spec.gpuTier}/5 · RAM {spec.ramTier}/5 · Depolama {spec.storageTier}/5");
            GUILayout.Label($"Ekran {spec.displayTier}/5 · Kamera {spec.cameraTier}/5 · Batarya {spec.batteryTier}/5 · Soğutma {spec.coolingTier}/5");
            GUILayout.Label($"Maliyet: {metrics.unitCost:0} · Hedef fiyat: {spec.targetPrice} · Birim katkı: {metrics.unitContribution:0}");
            GUILayout.Label($"Performans {metrics.performance:0} · Kalite {metrics.quality:0} · Güvenilirlik {metrics.reliability:0} · Memnuniyet {metrics.customerSatisfaction:0}");
            GUILayout.Label($"Termal risk {metrics.thermalRisk:0} · Enerji verimi {metrics.energyEfficiency:0} · Ar-Ge {researchProgress:0}%");
            GUILayout.Label(metrics.compatibilityNote);
            if (GUILayout.Button("Hedef segmenti değiştir")) CycleSegment();
            if (GUILayout.Button("Kasa malzemesini değiştir")) CycleMaterial();
            if (GUILayout.Button("Rengi değiştir")) CycleColor();
            if (GUILayout.Button("Performansı yükselt")) IncreasePerformance();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("CPU+")) UpgradeComponent("CPU");
            if (GUILayout.Button("GPU+")) UpgradeComponent("GPU");
            if (GUILayout.Button("RAM+")) UpgradeComponent("RAM");
            if (GUILayout.Button("Depolama+")) UpgradeComponent("Storage");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Ekran+")) UpgradeComponent("Display");
            if (GUILayout.Button("Kamera+")) UpgradeComponent("Camera");
            if (GUILayout.Button("Batarya+")) UpgradeComponent("Battery");
            if (GUILayout.Button("Soğutma+")) UpgradeComponent("Cooling");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Hedef fiyat -50")) { spec.targetPrice = Mathf.Max(300, spec.targetPrice - 50); Recalculate(); }
            if (GUILayout.Button("Hedef fiyat +50")) { spec.targetPrice = Mathf.Min(2500, spec.targetPrice + 50); Recalculate(); }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Ar-Ge projesini başlat")) StartResearch();
            if (GUILayout.Button("1 hafta simüle et")) SimulateWeek();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Performans testi")) RunTest(ProductTestType.Performance);
            if (GUILayout.Button("Batarya testi")) RunTest(ProductTestType.Battery);
            if (GUILayout.Button("Kalite testi")) RunTest(ProductTestType.Quality);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("100 adet üret")) StartProduction(100);
            if (GUILayout.Button("Ürünü piyasaya sür")) LaunchAndSell();
            GUILayout.Label($"Stok: {inventory} · Satılan: {sold} · Marka bilinirliği: {brandAwareness:0}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kaydet")) Save();
            if (GUILayout.Button("Yükle")) Load();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }

    public sealed class SmartphonePreview3D : MonoBehaviour
    {
        private Transform body;
        private Transform screen;
        private Transform cameraModule;
        private readonly List<Transform> lenses = new();
        private static readonly int PreviewLayer = 31;

        private void Awake()
        {
            gameObject.layer = PreviewLayer;
            body = Create(PrimitiveType.Cube, "Phone Body", transform);
            screen = Create(PrimitiveType.Cube, "Screen", body);
            cameraModule = Create(PrimitiveType.Cylinder, "Camera Module", body);
            for (var i = 0; i < 3; i++) lenses.Add(Create(PrimitiveType.Cylinder, "Camera Lens " + i, cameraModule));
        }

        private Transform Create(PrimitiveType type, string label, Transform parent)
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = label;
            primitive.transform.SetParent(parent);
            primitive.layer = PreviewLayer;
            return primitive.transform;
        }

        public void Apply(SmartphoneSpec spec)
        {
            if (body == null) return;
            body.localPosition = Vector3.zero;
            body.localRotation = Quaternion.identity;
            body.localScale = new Vector3(Mathf.Clamp(spec.screenInches / 6.4f, .85f, 1.12f), .12f, 1.9f);
            screen.localPosition = new Vector3(0f, .071f, .04f);
            screen.localScale = new Vector3(.86f, .035f, .86f);
            cameraModule.localPosition = new Vector3(-.25f, .08f, -.68f);
            cameraModule.localRotation = Quaternion.Euler(90f, 0f, 0f);
            cameraModule.localScale = new Vector3(.28f, .05f, .28f);
            for (var i = 0; i < lenses.Count; i++)
            {
                lenses[i].localPosition = new Vector3((i - 1) * .07f, .08f, 0f);
                lenses[i].localRotation = Quaternion.Euler(90f, 0f, 0f);
                lenses[i].localScale = Vector3.one * (.22f + spec.cameraTier * .012f);
            }
            var color = spec.color switch { SmartphoneColor.Silver => new Color(.65f, .7f, .76f), SmartphoneColor.Ocean => new Color(.05f, .23f, .55f), SmartphoneColor.Forest => new Color(.08f, .32f, .2f), SmartphoneColor.Rose => new Color(.55f, .2f, .3f), _ => new Color(.06f, .07f, .1f) };
            body.GetComponent<Renderer>().material.color = color;
            screen.GetComponent<Renderer>().material.color = new Color(.02f, .15f, .28f);
            cameraModule.GetComponent<Renderer>().material.color = new Color(.08f, .08f, .1f);
            foreach (var lens in lenses) lens.GetComponent<Renderer>().material.color = Color.black;
        }

        private void Update() => transform.Rotate(0f, 22f * Time.deltaTime, 0f, Space.World);
    }

    public sealed class SmartphonePreviewCamera : MonoBehaviour
    {
        private Camera previewCamera;

        public void Focus(Transform target)
        {
            previewCamera = gameObject.AddComponent<Camera>();
            previewCamera.name = "Product Preview Camera";
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(.015f, .025f, .05f);
            previewCamera.cullingMask = 1 << 31;
            previewCamera.rect = new Rect(.56f, .1f, .4f, .55f);
            transform.position = new Vector3(0f, .3f, -4.4f);
            transform.LookAt(target.position + Vector3.up * .1f);
        }
    }
}
