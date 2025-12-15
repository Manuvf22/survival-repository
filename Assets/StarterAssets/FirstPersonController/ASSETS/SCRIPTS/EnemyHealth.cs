using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    
    private EnemyAnimationController animController;
    private EnemyAI enemyAI;
    private bool isDead = false;

    void Start()
    {
        if (currentHealth == 0) currentHealth = maxHealth;
        animController = GetComponent<EnemyAnimationController>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (animController != null && currentHealth > 0)
        {
            animController.PlayHit();
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(GameManager.Instance.pointsPerHit);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Método para muerte instantánea (usado por Nuke power-up)
    public void InstantKill()
    {
        if (isDead) return;
        
        currentHealth = 0;
        Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        if (animController != null)
        {
            animController.PlayDeath();
        }
        
        if (enemyAI != null)
        {
            enemyAI.Die();
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyKilled();
        }

        Destroy(gameObject, 3f);
    }
}