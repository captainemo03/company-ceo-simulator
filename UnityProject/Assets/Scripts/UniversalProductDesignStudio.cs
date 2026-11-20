using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    public enum UniversalDesignMode { Blank, Template, Improve, Imported, Drawing, NewCategory }
    public enum UniversalPrimitive { Cube, Sphere, Cylinder, Capsule, Extruded, Imported }
    public enum UniversalMaterial { Plastic, Steel, Aluminium, Titanium, Wood, Glass, CarbonFiber, Rubber, Silicone, Copper, Composite, Custom }

    [Serializable]
    public sealed class UniversalPartData
    {
        public string id = Guid.NewGuid().ToString("N");
        public string name = "Part";
        public UniversalPrimitive primitive = UniversalPrimitive.Cube;
        public UniversalMaterial material = UniversalMaterial.Plastic;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
        public float volume = 1f;
        public float materialCost = 12f;
        public bool movable;
        public List<Vector2> sketch = new();
        public List<Vector3> meshVertices = new();
        public List<int> meshTriangles = new();
    }

    [Serializable]
    public sealed class UniversalMetric
    {
        public string name = "Performance";
        public string unit = "points";
        public float target;
        public float minimum;
        public float maximum = 100f;
        public float researchCost = 1000f;
        public float productionCost;
    }

    [Serializable]
    public sealed class UniversalProductVersion
    {
        public string label;
        public string json;
        public string createdAt;
    }

    [Serializable]
    public sealed class UniversalProductSave
    {
        public string productName = "My Invention";
        public string category = "New Product";
        public string problem = "";
        public string audience = "";
        public UniversalDesignMode mode;
        public List<UniversalPartData> parts = new();
        public List<UniversalMetric> metrics = new();
        public List<UniversalProductVersion> versions = new();
        public string customMaterialName = "My Composite";
        public float customMaterialCost = 40f;
        public float customMaterialDurability = 60f;
        public float cash = 1000000f;
        public int inventory;
        public int sold;
        public float development;
    }

    /// <summary>
    /// Runtime, category-agnostic product editor. It intentionally uses simple
    /// primitive geometry for the first playable loop and keeps every choice in
    /// serializable data so a licensed art pipeline can replace parts later.
    /// </summary>
    public sealed class UniversalProductDesignStudio : MonoBehaviour
    {
        [SerializeField] private UniversalProductSave data = new();
        [SerializeField] private Transform productRoot;
        [SerializeField] private UniversalPartData selectedPart;
        [SerializeField] private string externalModelPath = "";
        private readonly Dictionary<string, GameObject> objects = new();
        private readonly List<Vector2> drawing = new();
        private Texture2D white;
        private Rect drawingRect;
        private bool drawingActive;
        private bool showAdvanced;
        private string lastMessage = "Boş tasarım alanı hazır.";
        private int customMaterialIndex;

        public UniversalProductSave Data => data;
        public UniversalPartData SelectedPart => selectedPart;
        public float DesignComplexity => data.parts.Count * 2f + data.metrics.Count * 1.5f;
        public float UnitCost => CalculateUnitCost();
        public string LastMessage => lastMessage;

        private void Start()
        {
            productRoot = new GameObject("Universal Product · Editable Model").transform;
            productRoot.position = new Vector3(0f, 1.2f, 1.2f);
            white = Texture2D.whiteTexture;
            Load();
            if (data.parts.Count == 0) AddPrimitive(UniversalPrimitive.Cube, "Base Part");
        }

        public void AddPrimitive(UniversalPrimitive primitive, string name = null)
        {
            if (primitive == UniversalPrimitive.Extruded) { lastMessage = "Extrusion için önce çizim alanında kapalı bir şekil oluştur."; return; }
            var part = new UniversalPartData { name = string.IsNullOrWhiteSpace(name) ? primitive.ToString() : name, primitive = primitive, position = new Vector3((data.parts.Count % 3 - 1) * 1.25f, 0f, (data.parts.Count / 3) * 1.25f), scale = Vector3.one };
            data.parts.Add(part);
            CreateRuntimePart(part);
            selectedPart = part;
            lastMessage = $"{part.name} eklendi.";
            Changed();
        }

        private void CreateRuntimePart(UniversalPartData part)
        {
            if (objects.TryGetValue(part.id, out var existing) && existing != null) Destroy(existing);
            if ((part.primitive == UniversalPrimitive.Extruded && part.sketch.Count >= 3) || (part.primitive == UniversalPrimitive.Imported && part.meshVertices.Count > 0 && part.meshTriangles.Count > 0))
            {
                var meshObject = new GameObject(part.name);
                meshObject.transform.SetParent(productRoot);
                meshObject.transform.localPosition = part.position;
                meshObject.transform.localEulerAngles = part.rotation;
                meshObject.transform.localScale = part.scale;
                var meshFilter = meshObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = part.primitive == UniversalPrimitive.Extruded ? BuildExtrudedMesh(part.sketch) : BuildStoredMesh(part);
                meshObject.AddComponent<MeshRenderer>();
                ApplyMaterial(meshObject, part.material);
                objects[part.id] = meshObject;
                return;
            }
            var type = part.primitive switch { UniversalPrimitive.Sphere => PrimitiveType.Sphere, UniversalPrimitive.Cylinder => PrimitiveType.Cylinder, UniversalPrimitive.Capsule => PrimitiveType.Capsule, _ => PrimitiveType.Cube };
            var go = GameObject.CreatePrimitive(type);
            go.name = part.name;
            go.transform.SetParent(productRoot);
            go.transform.localPosition = part.position;
            go.transform.localEulerAngles = part.rotation;
            go.transform.localScale = part.scale;
            ApplyMaterial(go, part.material);
            objects[part.id] = go;
        }

        private void ApplyMaterial(GameObject go, UniversalMaterial material)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;
            var color = material switch
            {
                UniversalMaterial.Steel => new Color(.42f, .45f, .5f),
                UniversalMaterial.Aluminium => new Color(.7f, .73f, .77f),
                UniversalMaterial.Titanium => new Color(.32f, .38f, .44f),
                UniversalMaterial.Wood => new Color(.42f, .2f, .08f),
                UniversalMaterial.Glass => new Color(.12f, .5f, .8f, .45f),
                UniversalMaterial.CarbonFiber => new Color(.035f, .045f, .055f),
                UniversalMaterial.Rubber => new Color(.04f, .04f, .04f),
                UniversalMaterial.Silicone => new Color(.8f, .35f, .45f),
                UniversalMaterial.Copper => new Color(.6f, .25f, .08f),
                UniversalMaterial.Composite => new Color(.18f, .3f, .32f),
                _ => new Color(.18f, .34f, .62f)
            };
            renderer.material.color = color;
        }

        public void Select(string id)
        {
            selectedPart = data.parts.Find(x => x.id == id);
            lastMessage = selectedPart == null ? "Parça seçilemedi." : $"Seçili parça: {selectedPart.name}";
        }

        public void MoveSelected(Vector3 delta)
        {
            if (selectedPart == null) return;
            selectedPart.position += delta;
            SyncRuntime(selectedPart);
        }

        public void RotateSelected(Vector3 delta)
        {
            if (selectedPart == null) return;
            selectedPart.rotation += delta;
            SyncRuntime(selectedPart);
        }

        public void ScaleSelected(Vector3 factor)
        {
            if (selectedPart == null) return;
            selectedPart.scale = Vector3.Max(Vector3.one * .05f, Vector3.Scale(selectedPart.scale, factor));
            selectedPart.volume = Mathf.Max(.05f, selectedPart.scale.x * selectedPart.scale.y * selectedPart.scale.z);
            SyncRuntime(selectedPart);
            Changed();
        }

        public void CycleSelectedMaterial()
        {
            if (selectedPart == null) return;
            selectedPart.material = (UniversalMaterial)(((int)selectedPart.material + 1) % Enum.GetValues(typeof(UniversalMaterial)).Length);
            if (objects.TryGetValue(selectedPart.id, out var go)) ApplyMaterial(go, selectedPart.material);
            Changed();
        }

        private void SyncRuntime(UniversalPartData part)
        {
            if (!objects.TryGetValue(part.id, out var go) || go == null) { CreateRuntimePart(part); return; }
            go.transform.localPosition = part.position;
            go.transform.localEulerAngles = part.rotation;
            go.transform.localScale = part.scale;
        }

        public void DeleteSelected()
        {
            if (selectedPart == null) return;
            if (objects.TryGetValue(selectedPart.id, out var go) && go != null) Destroy(go);
            objects.Remove(selectedPart.id);
            data.parts.Remove(selectedPart);
            selectedPart = data.parts.Count == 0 ? null : data.parts[data.parts.Count - 1];
            Changed();
        }

        public void MergeSelectedWithNext()
        {
            if (selectedPart == null || data.parts.Count < 2) return;
            var index = data.parts.IndexOf(selectedPart);
            var other = data.parts[(index + 1) % data.parts.Count];
            if (other == selectedPart) return;
            selectedPart.scale += other.scale * .25f;
            selectedPart.volume += other.volume;
            DeletePartWithoutSelection(other);
            SyncRuntime(selectedPart);
            lastMessage = "Parçalar montaj ağacında birleştirildi.";
            Changed();
        }

        private void DeletePartWithoutSelection(UniversalPartData part)
        {
            if (objects.TryGetValue(part.id, out var go) && go != null) Destroy(go);
            objects.Remove(part.id);
            data.parts.Remove(part);
        }

        public void SetMaterialForSelected(UniversalMaterial material)
        {
            if (selectedPart == null) return;
            selectedPart.material = material;
            if (objects.TryGetValue(selectedPart.id, out var go)) ApplyMaterial(go, material);
            Changed();
        }

        public void StartDrawing() { drawing.Clear(); drawingActive = true; data.mode = UniversalDesignMode.Drawing; lastMessage = "Kapalı bir şekil çiz; sonra Extrude düğmesine bas."; }

        public void ExtrudeDrawing()
        {
            if (drawing.Count < 3) { lastMessage = "Extrusion için en az üç nokta gerekli."; return; }
            var part = new UniversalPartData { name = "Extruded Design", primitive = UniversalPrimitive.Extruded, material = UniversalMaterial.Composite, volume = drawing.Count * .18f, scale = Vector3.one, sketch = new List<Vector2>(drawing) };
            data.parts.Add(part);
            var go = new GameObject(part.name);
            go.transform.SetParent(productRoot);
            go.transform.localPosition = Vector3.zero;
            var filter = go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            filter.sharedMesh = BuildExtrudedMesh(part.sketch);
            ApplyMaterial(go, part.material);
            objects[part.id] = go;
            selectedPart = part;
            drawingActive = false;
            lastMessage = "Kapalı çizim 3D parçaya dönüştürüldü. Bu bir oyun içi tasarım simülasyonudur.";
            Changed();
        }

        private Mesh BuildExtrudedMesh(IReadOnlyList<Vector2> points)
        {
            var vertices = new List<Vector3>();
            foreach (var point in points) vertices.Add(new Vector3(point.x * .02f, -.25f, point.y * .02f));
            foreach (var point in points) vertices.Add(new Vector3(point.x * .02f, .25f, point.y * .02f));
            var triangles = new List<int>();
            for (var i = 1; i < points.Count - 1; i++) { triangles.Add(0); triangles.Add(i + 1); triangles.Add(i); triangles.Add(points.Count); triangles.Add(points.Count + i); triangles.Add(points.Count + i + 1); }
            for (var i = 0; i < points.Count; i++) { var next = (i + 1) % points.Count; triangles.Add(i); triangles.Add(next); triangles.Add(points.Count + next); triangles.Add(i); triangles.Add(points.Count + next); triangles.Add(points.Count + i); }
            var mesh = new Mesh { name = "Extruded player sketch" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildStoredMesh(UniversalPartData part)
        {
            var mesh = new Mesh { name = part.name + " · restored" };
            mesh.SetVertices(part.meshVertices); mesh.SetTriangles(part.meshTriangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        public float CalculateUnitCost()
        {
            var materials = 0f;
            foreach (var part in data.parts) materials += part.volume * (part.material == UniversalMaterial.Custom ? data.customMaterialCost : MaterialCost(part.material)) + part.materialCost;
            var complexity = 1f + DesignComplexity * .035f;
            return Mathf.Max(1f, (materials + data.parts.Count * 8f + data.metrics.Count * 5f) * complexity);
        }

        private static float MaterialCost(UniversalMaterial material) => material switch
        {
            UniversalMaterial.Plastic => 5f, UniversalMaterial.Steel => 18f, UniversalMaterial.Aluminium => 24f, UniversalMaterial.Titanium => 65f, UniversalMaterial.Wood => 10f, UniversalMaterial.Glass => 20f, UniversalMaterial.CarbonFiber => 54f, UniversalMaterial.Rubber => 7f, UniversalMaterial.Silicone => 8f, UniversalMaterial.Copper => 27f, UniversalMaterial.Composite => 32f, _ => 35f
        };

        public void AddCustomMetric()
        {
            data.metrics.Add(new UniversalMetric { name = "Custom metric " + (data.metrics.Count + 1), target = 50f, productionCost = 2f });
            Changed();
        }

        public void StartResearch()
        {
            if (data.development > 0f || data.cash < 5000f) return;
            data.cash -= 5000f + data.metrics.Count * 1000f;
            data.development = 1f;
            lastMessage = "Özgün ürün Ar-Ge projesi başlatıldı.";
            Changed();
        }

        public void AdvanceResearch()
        {
            if (data.development <= 0f) return;
            data.development = Mathf.Min(100f, data.development + Mathf.Max(4f, 18f - DesignComplexity * .12f));
            lastMessage = $"Ar-Ge ilerlemesi: {data.development:0}%";
            Changed();
        }

        public void ProducePrototype()
        {
            if (data.development < 100f) { lastMessage = "Prototip için Ar-Ge tamamlanmalı."; return; }
            var cost = CalculateUnitCost() * 3f;
            if (data.cash < cost) { lastMessage = "Prototip için yeterli nakit yok."; return; }
            data.cash -= cost; data.inventory += 1; lastMessage = "Prototip üretildi ve laboratuvar kataloğuna eklendi."; PublishToWorld(); Changed();
        }

        public void ProduceUnits(int quantity = 10)
        {
            if (data.development < 100f) return;
            var cost = CalculateUnitCost() * quantity;
            if (data.cash < cost) { lastMessage = "Üretim emri için yeterli nakit yok."; return; }
            data.cash -= cost; data.inventory += quantity; lastMessage = $"{quantity} adet üretildi."; Changed();
        }

        public void SellUnits()
        {
            if (data.inventory <= 0) return;
            var demand = Mathf.Max(0, Mathf.RoundToInt(25f + data.parts.Count * 7f + data.metrics.Count * 4f - DesignComplexity * 1.1f));
            var quantity = Mathf.Min(data.inventory, demand);
            data.inventory -= quantity; data.sold += quantity; data.cash += Mathf.RoundToInt(quantity * CalculateUnitCost() * 1.6f); lastMessage = $"{quantity} ürün satıldı."; Changed();
        }

        public void PublishToWorld()
        {
            var world = GameObject.Find("Universal Product · World Instance");
            if (world != null) Destroy(world);
            world = new GameObject("Universal Product · World Instance");
            foreach (var part in data.parts)
            {
                if (!objects.TryGetValue(part.id, out var source) || source == null) continue;
                var copy = Instantiate(source, world.transform);
                copy.name = part.name + " · player design";
                copy.transform.localPosition = part.position;
                copy.transform.localEulerAngles = part.rotation;
                copy.transform.localScale = part.scale;
            }
            world.transform.position = new Vector3(0f, .5f, 5f);
            lastMessage = "Oyuncunun modeli gerçek 3D dünya örneğine aktarıldı.";
        }

        public void SaveVersion(string label = null)
        {
            data.versions.Add(new UniversalProductVersion { label = string.IsNullOrWhiteSpace(label) ? "Version " + (data.versions.Count + 1) : label, json = JsonUtility.ToJson(data), createdAt = DateTime.UtcNow.ToString("O") });
            Save();
            lastMessage = "Tasarım sürümü kaydedildi.";
        }

        public void RestoreLatestVersion()
        {
            if (data.versions.Count == 0) { lastMessage = "Geri yüklenecek sürüm yok."; return; }
            var latest = data.versions[data.versions.Count - 1];
            var restored = JsonUtility.FromJson<UniversalProductSave>(latest.json);
            if (restored == null) return;
            restored.versions = data.versions;
            data = restored;
            foreach (var go in objects.Values) if (go != null) Destroy(go);
            objects.Clear();
            foreach (var part in data.parts) CreateRuntimePart(part);
            selectedPart = data.parts.Count > 0 ? data.parts[0] : null;
            Save();
            lastMessage = latest.label + " geri yüklendi.";
        }

        public void Save()
        {
            PlayerPrefs.SetString("company_ceo_universal_product", JsonUtility.ToJson(data)); PlayerPrefs.Save();
            lastMessage = "Universal ürün tasarımı kaydedildi.";
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey("company_ceo_universal_product")) return;
            var loaded = JsonUtility.FromJson<UniversalProductSave>(PlayerPrefs.GetString("company_ceo_universal_product"));
            if (loaded == null) return;
            data = loaded;
            foreach (var go in objects.Values) if (go != null) Destroy(go);
            objects.Clear();
            foreach (var part in data.parts) CreateRuntimePart(part);
            selectedPart = data.parts.Count > 0 ? data.parts[0] : null;
            lastMessage = "Universal ürün tasarımı yüklendi.";
        }

        public async void ImportExternalModel()
        {
            if (string.IsNullOrWhiteSpace(externalModelPath) || !File.Exists(externalModelPath)) { lastMessage = "Geçerli bir OBJ veya GLB/GLTF yolu gir."; return; }
            var extension = Path.GetExtension(externalModelPath).ToLowerInvariant();
            if (extension == ".obj") { ImportObj(externalModelPath); return; }
            if (extension == ".glb" || extension == ".gltf") { await ImportGltfByReflection(externalModelPath); return; }
            lastMessage = "Desteklenen formatlar: OBJ, GLB, GLTF.";
        }

        private void ImportObj(string path)
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length > 50L * 1024L * 1024L) { lastMessage = "OBJ dosyası 50 MB sınırını aşıyor; performans için içe aktarılmadı."; return; }
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            string[] lines;
            try { lines = File.ReadAllLines(path); }
            catch (Exception error) { lastMessage = "OBJ okunamadı: " + error.Message; return; }
            foreach (var line in lines)
            {
                if (vertices.Count > 200000 || triangles.Count > 600000) { lastMessage = "OBJ geometri sınırını aşıyor; içe aktarma durduruldu."; return; }
                var parts = line.Trim().Split(' ');
                if (parts.Length < 4) continue;
                if (parts[0] == "v" && float.TryParse(parts[1], out var x) && float.TryParse(parts[2], out var y) && float.TryParse(parts[3], out var z)) vertices.Add(new Vector3(x, y, z));
                if (parts[0] == "f" && parts.Length >= 4) for (var i = 1; i < parts.Length - 2; i++) { if (int.TryParse(parts[1].Split('/')[0], out var a) && int.TryParse(parts[i + 1].Split('/')[0], out var b) && int.TryParse(parts[i + 2].Split('/')[0], out var c)) { triangles.Add(a - 1); triangles.Add(b - 1); triangles.Add(c - 1); } }
            }
            if (vertices.Count == 0 || triangles.Count == 0) { lastMessage = "OBJ içinde geçerli geometri bulunamadı."; return; }
            var mesh = new Mesh { name = Path.GetFileNameWithoutExtension(path) }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals();
            var part = new UniversalPartData { name = Path.GetFileNameWithoutExtension(path), primitive = UniversalPrimitive.Imported, material = UniversalMaterial.Composite, volume = Mathf.Max(.1f, vertices.Count * .01f), meshVertices = new List<Vector3>(vertices), meshTriangles = new List<int>(triangles) }; data.parts.Add(part);
            var go = new GameObject(part.name); go.transform.SetParent(productRoot); var filter = go.AddComponent<MeshFilter>(); filter.sharedMesh = mesh; go.AddComponent<MeshRenderer>(); ApplyMaterial(go, part.material); objects[part.id] = go; selectedPart = part; data.mode = UniversalDesignMode.Imported; lastMessage = "OBJ model içe aktarıldı."; Changed();
        }

        private async Task ImportGltfByReflection(string path)
        {
            if (new FileInfo(path).Length > 100L * 1024L * 1024L) { lastMessage = "GLB/GLTF dosyası 100 MB sınırını aşıyor; içe aktarılmadı."; return; }
            var type = Type.GetType("GLTFast.GltfImport, glTFast");
            if (type == null) { lastMessage = "glTFast paketi bulunamadı; GLB içe aktarma devre dışı."; return; }
            try
            {
                var importer = Activator.CreateInstance(type);
                var load = Array.Find(type.GetMethods(), method =>
                    method.Name == "Load" && method.GetParameters().Length > 0 && method.GetParameters()[0].ParameterType == typeof(string));
                if (load == null) throw new MissingMethodException("GltfImport.Load(string,...)");
                var loadParameters = load.GetParameters();
                var loadArguments = new object[loadParameters.Length];
                loadArguments[0] = path;
                for (var i = 1; i < loadArguments.Length; i++) loadArguments[i] = OptionalArgument(loadParameters[i]);
                var task = load.Invoke(importer, loadArguments) as Task;
                if (task == null) throw new InvalidOperationException("GLB yükleme görevi oluşturulamadı.");
                await task;
                var root = new GameObject(Path.GetFileNameWithoutExtension(path)); root.transform.SetParent(productRoot);
                var instantiate = Array.Find(type.GetMethods(), x => x.Name == "InstantiateMainSceneAsync" && x.GetParameters().Length > 0 && x.GetParameters()[0].ParameterType == typeof(Transform));
                if (instantiate == null) throw new MissingMethodException("GltfImport.InstantiateMainSceneAsync");
                var instantiateParameters = instantiate.GetParameters();
                var parameters = new object[instantiateParameters.Length];
                parameters[0] = root.transform;
                for (var i = 1; i < parameters.Length; i++) parameters[i] = OptionalArgument(instantiateParameters[i]);
                if (instantiate.Invoke(importer, parameters) is Task instantiateTask) await instantiateTask;
                var part = new UniversalPartData { name = root.name, primitive = UniversalPrimitive.Imported, material = UniversalMaterial.Composite };
                CaptureMeshData(root, part);
                data.parts.Add(part); objects[part.id] = root.gameObject; selectedPart = part; data.mode = UniversalDesignMode.Imported; lastMessage = "GLB/GLTF model içe aktarıldı."; Changed();
            }
            catch (Exception error) { lastMessage = "GLB içe aktarma başarısız: " + error.GetBaseException().Message; }
        }

        private static object OptionalArgument(ParameterInfo parameter)
        {
            if (parameter.HasDefaultValue && parameter.DefaultValue != System.Reflection.Missing.Value) return parameter.DefaultValue;
            return parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null;
        }

        private static void CaptureMeshData(GameObject root, UniversalPartData part)
        {
            var rootWorldToLocal = root.transform.worldToLocalMatrix;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true)) AppendMesh(filter.sharedMesh, rootWorldToLocal * filter.transform.localToWorldMatrix, part);
            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)) AppendMesh(renderer.sharedMesh, rootWorldToLocal * renderer.transform.localToWorldMatrix, part);
        }

        private static void AppendMesh(Mesh mesh, Matrix4x4 matrix, UniversalPartData part)
        {
            if (mesh == null) return;
            var offset = part.meshVertices.Count;
            foreach (var vertex in mesh.vertices) part.meshVertices.Add(matrix.MultiplyPoint3x4(vertex));
            foreach (var triangle in mesh.triangles) part.meshTriangles.Add(offset + triangle);
        }

        private void Changed()
        {
            Save();
            var bridge = FindFirstObjectByType<CompanyEconomyBridge>();
            if (bridge == null) return;
            var economy = bridge.State;
            economy.cash = data.cash;
            bridge.Apply(economy);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(505f, 18f, 470f, 690f), GUI.skin.box);
            GUILayout.Label("UNIVERSAL PRODUCT DESIGN STUDIO");
            GUILayout.Label($"{data.productName} · {data.category} · Nakit {data.cash:N0} TRY");
            GUILayout.Label(lastMessage);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Boş tasarım")) { data.mode = UniversalDesignMode.Blank; lastMessage = "Boş tasarım alanı seçildi."; }
            if (GUILayout.Button("Şablon")) { data.mode = UniversalDesignMode.Template; AddPrimitive(UniversalPrimitive.Cylinder, "Template Body"); }
            if (GUILayout.Button("Yeni kategori")) data.mode = UniversalDesignMode.NewCategory;
            GUILayout.EndHorizontal();
            data.productName = GUILayout.TextField(data.productName);
            data.category = GUILayout.TextField(data.category);
            data.problem = GUILayout.TextField(data.problem);
            data.audience = GUILayout.TextField(data.audience);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Küp")) AddPrimitive(UniversalPrimitive.Cube);
            if (GUILayout.Button("Küre")) AddPrimitive(UniversalPrimitive.Sphere);
            if (GUILayout.Button("Silindir")) AddPrimitive(UniversalPrimitive.Cylinder);
            if (GUILayout.Button("Kapsül")) AddPrimitive(UniversalPrimitive.Capsule);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Çizim")) StartDrawing();
            if (GUILayout.Button("Extrude")) ExtrudeDrawing();
            if (GUILayout.Button("Parça sil")) DeleteSelected();
            if (GUILayout.Button("Birleştir")) MergeSelectedWithNext();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Move X+")) MoveSelected(Vector3.right * .25f);
            if (GUILayout.Button("Rotate Y+")) RotateSelected(Vector3.up * 15f);
            if (GUILayout.Button("Scale+")) ScaleSelected(Vector3.one * 1.1f);
            if (GUILayout.Button("Malzeme")) CycleSelectedMaterial();
            GUILayout.EndHorizontal();
            GUILayout.Label($"Parça: {data.parts.Count} · Birim maliyet: {UnitCost:0.00} · Karmaşıklık: {DesignComplexity:0.0}");
            if (selectedPart != null && selectedPart.material == UniversalMaterial.Custom)
            {
                data.customMaterialName = GUILayout.TextField(data.customMaterialName);
                GUILayout.Label($"Özel malzeme maliyeti: {data.customMaterialCost:0.00} · dayanıklılık {data.customMaterialDurability:0}");
                if (GUILayout.Button("Özel malzeme maliyetini +5")) { data.customMaterialCost += 5f; Changed(); }
            }
            GUILayout.Label("Assembly Tree");
            foreach (var part in data.parts) if (GUILayout.Button((selectedPart == part ? "> " : "") + part.name + " · " + part.material)) Select(part.id);
            if (GUILayout.Button("Özgün teknik özellik ekle")) AddCustomMetric();
            if (GUILayout.Button("Ar-Ge başlat")) StartResearch();
            if (GUILayout.Button("Ar-Ge ilerlet")) AdvanceResearch();
            GUILayout.Label($"Ar-Ge: {data.development:0}% · Stok: {data.inventory} · Satış: {data.sold}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prototip üret")) ProducePrototype();
            if (GUILayout.Button("10 adet üret")) ProduceUnits();
            if (GUILayout.Button("Satışa çıkar")) SellUnits();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Modeli 3D dünyaya aktar")) PublishToWorld();
            if (GUILayout.Button("Sürüm kaydet")) SaveVersion();
            if (GUILayout.Button("Son sürümü geri yükle")) RestoreLatestVersion();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kaydet")) Save();
            if (GUILayout.Button("Yükle")) Load();
            GUILayout.EndHorizontal();
            GUILayout.Label("OBJ/GLB/GLTF dosya yolu:");
            externalModelPath = GUILayout.TextField(externalModelPath);
            if (GUILayout.Button("Model içe aktar")) ImportExternalModel();
            showAdvanced = GUILayout.Toggle(showAdvanced, "İleri seviye çizim araçları");
            if (showAdvanced)
            {
                GUILayout.Label("İleri araçlar: eksen kilidi X/Y/Z, simetri ve parça çoğaltma");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Duplicate")) DuplicateSelected();
                if (GUILayout.Button("Mirror X")) MirrorSelected();
                if (GUILayout.Button("Grid snap")) SnapSelected();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
            drawingRect = new Rect(505f, 430f, 450f, 240f);
            if (drawingActive) DrawCanvas();
        }

        private void DrawCanvas()
        {
            GUI.Box(drawingRect, "Çizim alanı — kapalı şekil çiz");
            var current = Event.current;
            if (current.type == EventType.MouseDown && drawingRect.Contains(current.mousePosition)) { drawing.Add(current.mousePosition - drawingRect.position); current.Use(); }
            for (var i = 1; i < drawing.Count; i++) DrawSegment(drawing[i - 1] + drawingRect.position, drawing[i] + drawingRect.position, Color.cyan, 3f);
            if (drawing.Count > 2) DrawSegment(drawing[drawing.Count - 1] + drawingRect.position, drawing[0] + drawingRect.position, Color.gray, 1f);
        }

        private void DrawSegment(Vector2 a, Vector2 b, Color color, float width)
        {
            var delta = b - a; var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            var length = delta.magnitude; GUI.color = color; GUIUtility.RotateAroundPivot(angle, a); GUI.DrawTexture(new Rect(a.x, a.y - width * .5f, length, width), white); GUI.matrix = Matrix4x4.identity; GUI.color = Color.white;
        }

        private void DuplicateSelected()
        {
            if (selectedPart == null) return;
            var copy = JsonUtility.FromJson<UniversalPartData>(JsonUtility.ToJson(selectedPart));
            copy.id = Guid.NewGuid().ToString("N"); copy.name += " Copy"; copy.position += Vector3.right * .4f;
            data.parts.Add(copy); CreateRuntimePart(copy); selectedPart = copy; Changed();
        }

        private void MirrorSelected()
        {
            if (selectedPart == null) return;
            selectedPart.scale.x *= -1f; SyncRuntime(selectedPart); Changed();
        }

        private void SnapSelected()
        {
            if (selectedPart == null) return;
            selectedPart.position = new Vector3(Mathf.Round(selectedPart.position.x * 4f) / 4f, Mathf.Round(selectedPart.position.y * 4f) / 4f, Mathf.Round(selectedPart.position.z * 4f) / 4f);
            SyncRuntime(selectedPart); Changed();
        }

        private void OnDestroy() { if (white != null && white != Texture2D.whiteTexture) Destroy(white); }
    }
}
