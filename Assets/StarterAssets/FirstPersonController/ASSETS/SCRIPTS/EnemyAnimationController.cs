using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private bool isChasing = false;
    private bool isDead = false;
    
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        
        if (animator != null)
        {
            animator.applyRootMotion = false;
            Debug.Log($"? Animator listo en: {gameObject.name}");
        }
        else
        {
            Debug.LogError($"? NO hay Animator en: {gameObject.name}");
        }
    }
    
    public void StartChasing()
    {
        if (animator == null || isDead || isChasing) return;
        
        isChasing = true;
        
        // Activar directamente el estado "Zombie Running 0"
        animator.CrossFade("Zombie Running 0", 0.2f);
        
        Debug.Log($"?? {gameObject.name} comenzó a perseguir - Activando animación directamente");
    }
    
    public void StopChasing()
    {
        if (animator == null || isDead) return;
        
        isChasing = false;
        
        // Volver a idle si existe
        if (StateExists("Idle"))
        {
            animator.CrossFade("Idle", 0.2f);
        }
    }
    
    public void PlayAttack()
    {
        if (animator == null || isDead) return;
        
        // Intentar reproducir ataque directamente
        if (StateExists("Attack1"))
        {
            animator.CrossFade("Attack1", 0.1f);
        }
        else if (StateExists("Attack"))
        {
            animator.CrossFade("Attack", 0.1f);
        }
        
        Debug.Log($"?? {gameObject.name} atacando");
    }
    
    public void PlayHit()
    {
        if (animator == null || isDead) return;
        
        if (StateExists("Hit"))
        {
            animator.CrossFade("Hit", 0.1f);
        }
    }
    
    public void PlayDeath()
    {
        if (animator == null || isDead) return;
        
        isDead = true;
        
        if (StateExists("Death"))
        {
            animator.CrossFade("Death", 0.2f);
        }
        
        Debug.Log($"?? {gameObject.name} murió");
    }
    
    private bool StateExists(string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) 
            return false;
        
        // Verificar si el estado existe en el layer 0
        return animator.HasState(0, Animator.StringToHash(stateName));
    }
}
