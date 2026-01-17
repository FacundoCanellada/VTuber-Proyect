using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace VTuberProject.Editor
{
    /// <summary>
    /// Herramienta de editor para configurar colisiones en batch
    /// Permite aplicar perfiles a múltiples objetos a la vez
    /// </summary>
    public class CollisionBatchSetup : EditorWindow
    {
        private Vector2 scrollPosition;
        private CollisionProfile selectedProfile;
        private GameObject[] selectedObjects;
        private string nameFilter = "";
        private bool showPreview = true;

        // Reglas automáticas por nombre
        private Dictionary<string, CollisionProfile> autoRules = new Dictionary<string, CollisionProfile>();

        [MenuItem("Tools/VTuber/Collision Batch Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<CollisionBatchSetup>("Collision Setup");
            window.minSize = new Vector2(400, 500);
        }

        private void OnEnable()
        {
            RefreshSelectedObjects();
        }

        private void OnSelectionChange()
        {
            RefreshSelectedObjects();
            Repaint();
        }

        private void RefreshSelectedObjects()
        {
            selectedObjects = Selection.gameObjects;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Collision Batch Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Configure colisiones para múltiples objetos a la vez", MessageType.Info);
            
            EditorGUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawManualSection();
            EditorGUILayout.Space(10);
            DrawAutoRulesSection();
            EditorGUILayout.Space(10);
            DrawSelectionSection();
            EditorGUILayout.Space(10);
            DrawUtilitiesSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawManualSection()
        {
            EditorGUILayout.LabelField("Manual Setup", EditorStyles.boldLabel);
            
            selectedProfile = (CollisionProfile)EditorGUILayout.ObjectField(
                "Profile to Apply", 
                selectedProfile, 
                typeof(CollisionProfile), 
                false
            );

            if (selectedProfile != null)
            {
                EditorGUILayout.HelpBox($"Profile: {selectedProfile.profileName}\n{selectedProfile.description}", MessageType.None);
            }

            EditorGUILayout.Space(5);

            GUI.enabled = selectedObjects != null && selectedObjects.Length > 0 && selectedProfile != null;
            
            if (GUILayout.Button($"Apply to Selected ({selectedObjects?.Length ?? 0} objects)", GUILayout.Height(30)))
            {
                ApplyProfileToSelected();
            }

            GUI.enabled = true;
        }

        private void DrawAutoRulesSection()
        {
            EditorGUILayout.LabelField("Auto Rules (By Name)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Aplica perfiles automáticamente según el nombre del objeto", MessageType.Info);

            nameFilter = EditorGUILayout.TextField("Name Contains:", nameFilter);

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Wall → Wall Profile"))
            {
                ApplyRuleByName("Wall", "WallProfile");
            }
            
            if (GUILayout.Button("Bed → Furniture Profile"))
            {
                ApplyRuleByName("Bed", "FurnitureProfile");
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Carpet → No Collision"))
            {
                ApplyRuleByName("Carpet", "DecorationProfile");
            }
            
            if (GUILayout.Button("Occluder → No Collision"))
            {
                ApplyRuleByName("Occluder", "OccluderProfile");
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Apply to Scene by Smart Rules", GUILayout.Height(25)))
            {
                ApplySmartRules();
            }
        }

        private void DrawSelectionSection()
        {
            EditorGUILayout.LabelField("Current Selection", EditorStyles.boldLabel);

            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                EditorGUILayout.HelpBox("No objects selected", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField($"Selected: {selectedObjects.Length} objects");

            if (showPreview)
            {
                EditorGUI.indentLevel++;
                foreach (var obj in selectedObjects.Take(10))
                {
                    if (obj != null)
                    {
                        EditorGUILayout.LabelField($"• {obj.name}");
                    }
                }
                if (selectedObjects.Length > 10)
                {
                    EditorGUILayout.LabelField($"... and {selectedObjects.Length - 10} more");
                }
                EditorGUI.indentLevel--;
            }

            showPreview = EditorGUILayout.Toggle("Show Preview", showPreview);
        }

        private void DrawUtilitiesSection()
        {
            EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Select All in Scene"))
            {
                SelectAllInScene();
            }

            if (GUILayout.Button("Select by Name"))
            {
                SelectByName(nameFilter);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUI.color = Color.yellow;
            if (GUILayout.Button("Remove All Colliders from Selected"))
            {
                if (EditorUtility.DisplayDialog("Remove Colliders", 
                    $"Remove all colliders from {selectedObjects?.Length ?? 0} objects?", 
                    "Yes", "Cancel"))
                {
                    RemoveCollidersFromSelected();
                }
            }
            GUI.color = Color.white;

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Create Default Profiles"))
            {
                CreateDefaultProfiles();
            }
        }

        private void ApplyProfileToSelected()
        {
            if (selectedProfile == null || selectedObjects == null) return;

            int count = 0;
            foreach (var obj in selectedObjects)
            {
                if (obj != null)
                {
                    Undo.RecordObject(obj, "Apply Collision Profile");
                    
                    var setup = obj.GetComponent<SceneObjectSetup>();
                    if (setup == null)
                    {
                        setup = obj.AddComponent<SceneObjectSetup>();
                    }

                    setup.profile = selectedProfile;
                    setup.ApplyProfile();
                    
                    EditorUtility.SetDirty(obj);
                    count++;
                }
            }

            Debug.Log($"[CollisionBatchSetup] Applied profile '{selectedProfile.profileName}' to {count} objects");
            EditorUtility.DisplayDialog("Success", $"Applied profile to {count} objects", "OK");
        }

        private void ApplyRuleByName(string nameContains, string profileName)
        {
            var profile = FindProfileByName(profileName);
            if (profile == null)
            {
                Debug.LogWarning($"Profile '{profileName}' not found. Create it first.");
                return;
            }

            var objects = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.name.Contains(nameContains, System.StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (objects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Objects", $"No objects found with name containing '{nameContains}'", "OK");
                return;
            }

            foreach (var obj in objects)
            {
                Undo.RecordObject(obj, "Apply Auto Rule");
                
                var setup = obj.GetComponent<SceneObjectSetup>();
                if (setup == null)
                {
                    setup = obj.AddComponent<SceneObjectSetup>();
                }

                setup.profile = profile;
                setup.ApplyProfile();
                
                EditorUtility.SetDirty(obj);
            }

            Debug.Log($"[CollisionBatchSetup] Applied '{profileName}' to {objects.Length} objects containing '{nameContains}'");
        }

        private void ApplySmartRules()
        {
            var rules = new Dictionary<string, string>
            {
                { "Wall", "WallProfile" },
                { "Boundary", "WallProfile" },
                { "Closet", "FurnitureProfile" },
                { "Bed", "FurnitureProfile" },
                { "Desk", "FurnitureProfile" },
                { "Chair", "FurnitureProfile" },
                { "Carpet", "DecorationProfile" },
                { "Flower", "DecorationProfile" },
                { "Plant", "DecorationProfile" },
                { "Occluder", "OccluderProfile" },
                { "Switch", "InteractableProfile" },
                { "Door", "InteractableProfile" }
            };

            int totalApplied = 0;

            foreach (var rule in rules)
            {
                var profile = FindProfileByName(rule.Value);
                if (profile == null) continue;

                var objects = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                    .Where(go => go.name.Contains(rule.Key, System.StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                foreach (var obj in objects)
                {
                    Undo.RecordObject(obj, "Apply Smart Rules");
                    
                    var setup = obj.GetComponent<SceneObjectSetup>();
                    if (setup == null)
                    {
                        setup = obj.AddComponent<SceneObjectSetup>();
                    }

                    setup.profile = profile;
                    setup.ApplyProfile();
                    
                    EditorUtility.SetDirty(obj);
                    totalApplied++;
                }
            }

            Debug.Log($"[CollisionBatchSetup] Applied smart rules to {totalApplied} objects");
            EditorUtility.DisplayDialog("Success", $"Applied smart rules to {totalApplied} objects", "OK");
        }

        private void SelectAllInScene()
        {
            var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            Selection.objects = allObjects;
            RefreshSelectedObjects();
        }

        private void SelectByName(string nameContains)
        {
            if (string.IsNullOrEmpty(nameContains)) return;

            var objects = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.name.Contains(nameContains, System.StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Selection.objects = objects;
            RefreshSelectedObjects();

            Debug.Log($"Selected {objects.Length} objects containing '{nameContains}'");
        }

        private void RemoveCollidersFromSelected()
        {
            if (selectedObjects == null) return;

            int count = 0;
            foreach (var obj in selectedObjects)
            {
                if (obj != null)
                {
                    Undo.RecordObject(obj, "Remove Colliders");
                    
                    var setup = obj.GetComponent<SceneObjectSetup>();
                    if (setup != null)
                    {
                        setup.RemoveSetup();
                    }
                    else
                    {
                        // Remover colliders manualmente
                        var colliders = obj.GetComponents<Collider2D>();
                        foreach (var col in colliders)
                        {
                            DestroyImmediate(col);
                        }
                    }
                    
                    EditorUtility.SetDirty(obj);
                    count++;
                }
            }

            Debug.Log($"[CollisionBatchSetup] Removed colliders from {count} objects");
        }

        private CollisionProfile FindProfileByName(string name)
        {
            string[] guids = AssetDatabase.FindAssets($"t:CollisionProfile {name}");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<CollisionProfile>(path);
            }
            return null;
        }

        private void CreateDefaultProfiles()
        {
            CreateProfile("WallProfile", "Paredes y límites", CollisionProfile.ColliderType.Box, false, true, false, 0);
            CreateProfile("FurnitureProfile", "Muebles grandes", CollisionProfile.ColliderType.Box, false, true, true, 0);
            CreateProfile("DecorationProfile", "Decoración sin colisión", CollisionProfile.ColliderType.None, false, true, true, 0);
            CreateProfile("OccluderProfile", "Occluders visuales", CollisionProfile.ColliderType.None, false, true, true, 0);
            CreateProfile("InteractableProfile", "Objetos interactivos", CollisionProfile.ColliderType.Box, true, true, true, 0);
            CreateProfile("DynamicProfile", "Objetos dinámicos", CollisionProfile.ColliderType.Box, false, false, true, 0);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "Created 6 default collision profiles in Assets folder", "OK");
        }

        private void CreateProfile(string name, string description, CollisionProfile.ColliderType colliderType, 
            bool isTrigger, bool isStatic, bool useYSorting, float sizeReduction)
        {
            var profile = CreateInstance<CollisionProfile>();
            profile.profileName = name;
            profile.description = description;
            profile.colliderType = colliderType;
            profile.isTrigger = isTrigger;
            profile.isStatic = isStatic;
            profile.useYSorting = useYSorting;
            profile.sizeReduction = sizeReduction;
            profile.autoFitToSprite = true;

            AssetDatabase.CreateAsset(profile, $"Assets/{name}.asset");
        }
    }
}
