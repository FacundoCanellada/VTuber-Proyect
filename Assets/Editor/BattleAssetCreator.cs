using UnityEditor;
using UnityEngine;

public class BattleAssetCreator
{
    [MenuItem("Tools/Create Battle RT")]
    public static void CreateRT()
    {
        RenderTexture rt = new RenderTexture(1152, 648, 24);
        AssetDatabase.CreateAsset(rt, "Assets/BattleRT.renderTexture");
        AssetDatabase.SaveAssets();
        Debug.Log("BattleRT created at Assets/BattleRT.renderTexture");
    }
}
