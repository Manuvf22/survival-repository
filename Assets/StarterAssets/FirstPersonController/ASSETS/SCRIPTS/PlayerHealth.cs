using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public TMP_Text healthText;
    public Image damageImage;

    [Header("Damage Flash")]
    public float flashSpeed = 5f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("Audio")]
    public AudioClip damageSound;
    private AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        UpdateHealthUI();
        
        if (damageImage != null)
        {
            damageImage.color = Color.clear;
        }
    }

    void Update()
    {
        if (damageImage != null)
        {
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (damageImage != null)
        {
            damageImage.color = flashColor;
        }

        UpdateHealthUI();

        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }
    
    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }

    void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}