using UnityEngine;

public class WwiseEncounterTester : MonoBehaviour
{
    [Header("Wwise Evento Principal")]
    [Tooltip("Arrastra aquí el evento Play_EvelythBattle")]
    public AK.Wwise.Event iniciarMusicaEvent;

    [Header("Wwise Estados")]
    [Tooltip("Arrastra aquí el estado Pre")]
    public AK.Wwise.State estadoPre;
    [Tooltip("Arrastra aquí el estado Mid")]
    public AK.Wwise.State estadoMid;
    [Tooltip("Arrastra aquí el estado Batalla Tensa")]
    public AK.Wwise.State estadoBatallaTensa;
    [Tooltip("Arrastra aquí el estado Batalla Suave")]
    public AK.Wwise.State estadoBatallaSuave;

    void Start()
    {
        // Nos aseguramos de que el sistema esté limpio y en "Pre" al darle Play a Unity
        estadoPre.SetValue();
    }

    // ==========================================
    // ESTOS MÉTODOS VAN CONECTADOS A LOS BOTONES
    // ==========================================

    public void IniciarEncuentro()
    {
        estadoPre.SetValue();
        uint playingID = iniciarMusicaEvent.Post(gameObject);
        
        if (playingID == AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
        {
            Debug.LogError("❌ Wwise: Error al postear el evento (Invalid Playing ID)");
        }
        else if (playingID == AkUnitySoundEngine.AK_PENDING_EVENT_LOAD_ID)
        {
            Debug.LogWarning("⏳ Wwise: El evento está pendiente de carga (Banco aún no cargado). Se disparará automáticamente al terminar.");
        }
        else
        {
            Debug.Log($"🎵 Wwise: Iniciando evento en estado [PRE]. PlayingID: {playingID}");
        }
    }

    public void CalentarDialogo()
    {
        estadoMid.SetValue();
        Debug.Log("🎵 Wwise: Cambio a estado [MID]. Esperando al próximo compás...");
    }

    public void ArrancarCombate()
    {
        estadoBatallaTensa.SetValue();
        Debug.Log("🎵 Wwise: Cambio a [BATALLA TENSA]. Debería sonar el puente POST ahora mismo.");
    }

    public void AbrirMenuAccion()
    {
        estadoBatallaSuave.SetValue();
        Debug.Log("🎵 Wwise: Cambio a [BATALLA SUAVE]. Entrando a menú de items/actuar.");
    }

    public void ReanudarCombate()
    {
        estadoBatallaTensa.SetValue();
        Debug.Log("🎵 Wwise: Cambio a [BATALLA TENSA]. Regresando al combate movido.");
    }
}