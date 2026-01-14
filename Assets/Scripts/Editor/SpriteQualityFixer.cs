#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace VTuberProject.Tools
{
    /// <summary>
    /// Herramienta para optimizar la calidad de sprites pixel art
    /// </summary>
    public class SpriteQualityFixer : EditorWindow
    {
        private string folderPath = "Assets/Art";
        private bool applyToSubfolders = true;
        private bool fixFilterMode = true;
        private bool fixCompression = true;
        private bool fixMaxSize = true;
        private int targetMaxSize = 2048;

        [MenuItem("Tools/VTuber/Fix Sprite Quality")]
        public static void ShowWindow()
        {
            GetWindow<SpriteQualityFixer>("Sprite Quality Fixer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Sprite Quality Fixer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Esta herramienta arregla la calidad de sprites pixel art.\n" +
                "- Filter Mode: Point (sin blur)\n" +
                "- Compression: None (máxima calidad)\n" +
                "- Max Size: 2048 o superior\n" +
                "- Format: RGBA 32 bit",
                MessageType.Info);

            GUILayout.Space(10);

            folderPath = EditorGUILayout.TextField("Carpeta:", folderPath);
            applyToSubfolders = EditorGUILayout.Toggle("Incluir subcarpetas", applyToSubfolders);

            GUILayout.Space(10);

            fixFilterMode = EditorGUILayout.Toggle("Arreglar Filter Mode (Point)", fixFilterMode);
            fixCompression = EditorGUILayout.Toggle("Arreglar Compresión (None)", fixCompression);
            fixMaxSize = EditorGUILayout.Toggle("Arreglar Max Size", fixMaxSize);
            
            if (fixMaxSize)
            {
                targetMaxSize = EditorGUILayout.IntField("Max Size:", targetMaxSize);
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Arreglar Todos los Sprites", GUILayout.Height(40)))
            {
                FixAllSprites();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Arreglar Solo Sprites Seleccionados"))
            {
                FixSelectedSprites();
            }
        }

        private void FixAllSprites()
        {
            if (!Directory.Exists(folderPath))
            {
                EditorUtility.DisplayDialog("Error", "La carpeta no existe: " + folderPath, "OK");
                return;
            }

            string[] guids;
            if (applyToSubfolders)
            {
                guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            }
            else
            {
                guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            }

            int fixedCount = 0;
            int total = guids.Length;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                
                if (EditorUtility.DisplayCancelableProgressBar(
                    "Arreglando Sprites",
                    $"Procesando: {Path.GetFileName(path)}",
                    (float)i / total))
                {
                    break;
                }

                if (FixSprite(path))
                {
                    fixedCount++;
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Completado",
                $"Se arreglaron {fixedCount} de {total} sprites.",
                "OK");
        }

        private void FixSelectedSprites()
        {
            Object[] selected = Selection.objects;
            int fixedCount = 0;

            foreach (Object obj in selected)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (FixSprite(path))
                {
                    fixedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Completado",
                $"Se arreglaron {fixedCount} sprites seleccionados.",
                "OK");
        }

        private bool FixSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer == null || importer.textureType != TextureImporterType.Sprite)
            {
                return false;
            }

            bool changed = false;

            // Filter Mode: Point (no blur para pixel art)
            if (fixFilterMode && importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

            // Compression: None (máxima calidad)
            if (fixCompression && importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            // Max Size
            if (fixMaxSize && importer.maxTextureSize < targetMaxSize)
            {
                importer.maxTextureSize = targetMaxSize;
                changed = true;
            }

            // Configuraciones adicionales para pixel art
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            // Formato de textura (RGBA 32 bit para máxima calidad)
            TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();
            if (settings.format != TextureImporterFormat.RGBA32)
            {
                settings.format = TextureImporterFormat.RGBA32;
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(settings);
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
                Debug.Log($"Sprite arreglado: {Path.GetFileName(path)}");
            }

            return changed;
        }
    }
}
#endif
