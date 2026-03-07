using UnityEngine;

public class TrustManager : MonoBehaviour
{
    public static TrustManager Instance { get; private set; }

    [Header("Configuración LC (Level of Confidence)")]
    [Tooltip("Nivel actual de confianza (0 - 70/100)")]
    [Range(0, 100)]
    public float LC = 43f; // Inicio base

    // Definición de Rangos para cálculos rápidos
    // 60-70: Confianza Plena (HP 95-100%)
    // 45-59: Estable (HP 75-90%)
    // 30-44: Duda (HP 55-70%) -> Nivel INICIAL (43 entra aquí)
    // 15-29: Sin confianza (HP 35-50%)
    // 0-14: Nada (HP 25-35%)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Modifica el LC. Se usa en decisiones y eventos.
    /// </summary>
    public void ModifyLC(float amount)
    {
        LC += amount;
        LC = Mathf.Clamp(LC, 0f, 100f); // Topeamos en 100 por seguridad, aunque la tabla llega a 70+
        Debug.Log($"[TrustSystem] LC actualizado: {LC} (Cambio: {amount})");
    }

    /// <summary>
    /// Calcula el porcentaje de HP disponible basado en el LC.
    /// </summary>
    public float GetHPMultiplier()
    {
        // Lógica basada en la tabla del usuario
        // LC aproximado     Estado             HP resultante
        // 60 – 70           Confianza plena    95% – 100%
        // 45 – 59           Confianza estable  75% – 90%
        // 30 – 44           Duda               55% – 70%
        // 15 – 29           ya no hay confi    35% – 50%
        // 0 – 14            ps nada alv        25% – 35%

        if (LC >= 60) return Random.Range(0.95f, 1.0f); 
        if (LC >= 45) return Random.Range(0.75f, 0.90f); 
        if (LC >= 30) return Random.Range(0.55f, 0.70f); 
        if (LC >= 15) return Random.Range(0.35f, 0.50f); 
        return Random.Range(0.25f, 0.35f);
    }

    /// <summary>
    /// Determina si Ahiiruw tiene iniciativa propia (Autonomía).
    /// </summary>
    public bool HasAutonomy()
    {
        // Si el LC es alto, ella tiene personalidad y cuestiona cosas.
        // Si es bajo (ej. menos de 30), es un "muñeco".
        return LC >= 45; 
    }

    /// <summary>
    /// Chequea si Ahiiruw rechaza un objeto que el jugador quiere que tome.
    /// </summary>
    public bool WillRejectItem(string itemID, bool isItemUgly)
    {
        // Solo tiene voluntad para rechazar si tiene autonomía
        if (!HasAutonomy()) return false;

        // Lógica de ejemplo: Si el objeto es feo y la confianza es alta, lo tira.
        if (isItemUgly) 
        {
            Debug.Log("Ahiiruw: 'Iugh, no voy a guardar eso'. (Item rechazado por LC alto)");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Chequea si Ahiiruw toma algo por su cuenta (iniciativa).
    /// </summary>
    public bool WillPickupCuteItem()
    {
        if (!HasAutonomy()) return false;
        
        // Probabilidad basada en qué tan alto es el LC
        float chance = (LC - 40) * 2; // Ejemplo básico
        return Random.Range(0, 100) < chance;
    }
}
