using UnityEngine;

/// <summary>
/// Gestiona los Audio Listeners en la escena para asegurar que solo haya uno activo.
/// Coloca este script en un GameObject vacío en tu escena o en el GameManager.
/// </summary>
public class AudioListenerManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Mantener el listener de la Main Camera si existe")]
    public bool keepMainCameraListener = true;

    void Awake()
    {
        CleanupAudioListeners();
    }

    void CleanupAudioListeners()
    {
        // Encontrar todos los Audio Listeners en la escena
        AudioListener[] allListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        
        if (allListeners.Length <= 1)
        {
            Debug.Log("? Audio Listener OK - Solo hay " + allListeners.Length + " listener(s)");
            return;
        }

        Debug.LogWarning("?? Encontrados " + allListeners.Length + " Audio Listeners! Limpiando...");

        AudioListener mainCameraListener = null;
        
        // Buscar el listener de la Main Camera
        if (keepMainCameraListener)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCameraListener = mainCamera.GetComponent<AudioListener>();
            }
        }

        // Eliminar todos los listeners excepto el de la Main Camera
        int removedCount = 0;
        foreach (AudioListener listener in allListeners)
        {
            if (listener == null) continue;

            // No eliminar el de la Main Camera
            if (mainCameraListener != null && listener == mainCameraListener)
            {
                Debug.Log("? Manteniendo Audio Listener de Main Camera: " + listener.gameObject.name);
                continue;
            }

            // Eliminar listener duplicado
            Debug.Log("? Eliminando Audio Listener duplicado de: " + listener.gameObject.name);
            Destroy(listener);
            removedCount++;
        }

        Debug.Log($"? Limpieza completa: Eliminados {removedCount} Audio Listeners duplicados");
    }

    // Para debugging: mostrar cuántos listeners hay
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            Debug.Log($"?? Audio Listeners activos: {listeners.Length}");
            foreach (AudioListener l in listeners)
            {
                if (l != null)
                {
                    Debug.Log($"  - {l.gameObject.name} (Active: {l.enabled})");
                }
            }
        }
    }
}
