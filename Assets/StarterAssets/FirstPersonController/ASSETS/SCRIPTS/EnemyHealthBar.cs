using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Vector3 offset = new Vector3(0, 2.5f, 0); // Más alto

    private Transform enemyTransform;
    private EnemyHealth enemyHealth;
    private Camera mainCamera;
    private Canvas canvas;

    void Start()
    {
        mainCamera = Camera.main;

        // Buscar el enemigo padre
        enemyHealth = GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyTransform = enemyHealth.transform;
        }

        // Buscar el slider
        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
        }

        // ✅ CONFIGURAR CANVAS
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;

            // ✅ ESCALA CORRECTA (muy importante)
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 30);
            transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }

        Debug.Log("✅ HealthBar iniciada - Canvas: " + (canvas != null) + " | Slider: " + (healthSlider != null));
    }

    void LateUpdate() // ✅ Usar LateUpdate para UI
    {
        if (enemyHealth == null || enemyTransform == null) return;

        // Actualizar valor de la barra
        if (healthSlider != null)
        {
            healthSlider.value = (float)enemyHealth.currentHealth / enemyHealth.maxHealth;
        }

        // Seguir al enemigo
        transform.position = enemyTransform.position + offset;

        // Mirar siempre a la cámara
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}