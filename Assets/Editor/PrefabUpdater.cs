using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UndertaleEncounter;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PrefabUpdater
{
    [MenuItem("Tools/Update EnemySelect Prefab")]
    public static void UpdatePrefab()
    {
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null) {
            Debug.LogError("No estás en el Prefab Stage de EnemySelect");
            return;
        }

        GameObject root = stage.prefabContentsRoot;
        BattleButton oldBtn = root.GetComponent<BattleButton>();
        if (oldBtn == null) {
             Debug.LogError("BattleButton no encontrado en el root");
             return;
        }

        // Guardar valores
        Sprite norm = oldBtn.normalSprite;
        Sprite foc = oldBtn.focusedSprite;
        RectTransform heart = oldBtn.heartAnchor;

        // Reemplazar componente
        Object.DestroyImmediate(oldBtn);
        EnemySelectButton newBtn = root.AddComponent<EnemySelectButton>();

        // Restaurar y configurar
        newBtn.normalSprite = norm;
        newBtn.focusedSprite = foc;
        newBtn.heartAnchor = heart;

        Transform nameObj = root.transform.Find("Name");
        if (nameObj != null) newBtn.nameText = nameObj.GetComponent<TextMeshProUGUI>();

        Transform healthObj = root.transform.Find("Health");
        if (healthObj != null) newBtn.healthBar = healthObj.GetComponent<Image>();

        EditorUtility.SetDirty(root);
        Debug.Log("Prefab EnemySelect actualizado con éxito.");
    }
}
