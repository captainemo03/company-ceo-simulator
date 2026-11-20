using System;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    /// <summary>
    /// Loads the licensed GLB files at runtime. GLB files are intentionally kept in
    /// StreamingAssets so standalone builds do not depend on an editor-only import.
    /// </summary>
    public static class RuntimeGltfLoader
    {
        public static async Task<GameObject> LoadAsync(string relativePath, Transform parent, Vector3 position, float scale)
        {
            var path = Path.Combine(Application.streamingAssetsPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                Debug.LogError($"Missing runtime GLB: {path}");
                return null;
            }

            try
            {
                var importer = new GltfImport();
                var loaded = await importer.Load(new Uri(path, UriKind.Absolute));
                if (!loaded)
                {
                    Debug.LogError($"glTFast could not load GLB: {relativePath}");
                    return null;
                }

                var root = new GameObject(Path.GetFileNameWithoutExtension(path));
                root.transform.SetParent(parent, false);
                root.transform.position = position;
                root.transform.localScale = Vector3.one * scale;
                var instantiated = await importer.InstantiateMainSceneAsync(root.transform);
                if (!instantiated)
                {
                    UnityEngine.Object.Destroy(root);
                    Debug.LogError($"glTFast could not instantiate GLB scene: {relativePath}");
                    return null;
                }

                Debug.Log($"Loaded licensed GLB at runtime: {relativePath}");
                return root;
            }
            catch (Exception error)
            {
                Debug.LogError($"Runtime GLB load failed for {relativePath}: {error.GetBaseException().Message}");
                return null;
            }
        }
    }
}
