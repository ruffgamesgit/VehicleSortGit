#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Core.PackageLoader
{
    public static class PackageLoader
    {
        private const string ManifestPath = "Packages/manifest.json";

        private static readonly HashSet<string> PackageNames = new()
        {
            "com.unity.textmeshpro",
            "com.cysharp.unitask",
            "com.neuecc.unirx",
            "com.bgtools.playerprefseditor",
            "com.lionstudios.release.bundle"
        };

        private static Stack<string> _packagesToLoad;
        private static Stack<string> _packagesToRemove;
        private static RemoveRequest _removeRequest;
        private static AddRequest _addRequest;

        [MenuItem("Ruff/Add Packages")]
        private static void AddPackages()
        {
            _packagesToLoad = new Stack<string>();
            foreach (var packageName in PackageNames)
            {
                _packagesToLoad.Push(packageName);
            }

            LoadOperation();
        }

        [MenuItem("Ruff/Reset All Packages")]
        private static void RemovePackages()
        {
            Client.ResetToEditorDefaults();
        }


        private static void LoadOperation()
        {
            if (_packagesToLoad.Count > 0)
            {
                var package = _packagesToLoad.Pop();
                if (!package.Contains("com.unity."))
                {
                    if (package.Contains("com.lionstudios"))
                    {
                        AddScopedRegistry("Lion Studios", "http://packages.lionstudios.cc:4873", "com.lionstudios.release");
                    }
                    else
                    {
                        AddScopedRegistry("package.openupm.com", "https://package.openupm.com", package);
                    }
                    
                }

                _addRequest = Client.Add(package);

                EditorApplication.update += LoadProgress;
            }
        }

        static void LoadProgress()
        {
            if (_addRequest.IsCompleted)
            {
                if (_addRequest.Status == StatusCode.Success)
                    Debug.Log("Installed: " + _addRequest.Result.packageId);
                else if (_addRequest.Status == StatusCode.Failure)
                    Debug.Log(_addRequest.Error.message);
                EditorApplication.update -= LoadProgress;
                LoadOperation();
            }
        }

        private static void AddScopedRegistry(string name, string url, string registry)
        {
            Manifest manifest = JsonUtility.FromJson<Manifest>(LoadManifest());

            ScopedRegistry newRegistry = new ScopedRegistry
            {
                name = name,
                url = url,
                scopes = new[] { registry }
            };

            if (manifest.scopedRegistries == null || manifest.scopedRegistries.Length == 0)
            {
                manifest.scopedRegistries = new[] { newRegistry };
                UpdateScopedRegistries(manifest.scopedRegistries);
            }
            else
            {
                var scope = manifest.scopedRegistries.FirstOrDefault((element) => element.name == name);

                if (scope == null)
                {
                    List<ScopedRegistry> scopedRegistriesTemp = manifest.scopedRegistries.ToList();
                    scopedRegistriesTemp.Add(newRegistry);
                    manifest.scopedRegistries = scopedRegistriesTemp.ToArray();
                    UpdateScopedRegistries(manifest.scopedRegistries);
                }
                else
                {
                    if (scope.scopes.Any(element => element == registry))
                    {
                        Debug.Log("Registry already exists");
                    }
                    else
                    {
                        var registryList = scope.scopes.ToList();
                        registryList.Add(registry);
                        scope.scopes = registryList.ToArray();
                        UpdateScopedRegistries(manifest.scopedRegistries);
                    }
                }
            }
        }

        private static string LoadManifest()
        {
            return File.ReadAllText(ManifestPath);
        }

        private static void UpdateScopedRegistries(ScopedRegistry[] scopedRegistry)
        {
            // string json = LoadManifest();
            // JObject manifestJObject = JObject.Parse(json);
            // JArray newScopedRegistries = new();
            // foreach (var registry in scopedRegistry)
            // {
            //     newScopedRegistries.Add(new JObject
            //     {
            //         ["name"] = registry.name,
            //         ["url"] = registry.url,
            //         ["scopes"] = new JArray { registry.scopes }
            //     });
            // }
            //
            // manifestJObject["scopedRegistries"] = newScopedRegistries;
            //
            // string output = manifestJObject.ToString();
            // File.WriteAllText(ManifestPath, output);
        }
    }
}
#endif