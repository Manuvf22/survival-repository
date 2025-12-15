using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float detectionRadius = 20f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;

    [Header("Audio")]
    public AudioClip attackSound;

    private Transform player;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private EnemyAnimationController animController;
    private float nextAttackTime = 0f;
    private bool isDead = false;
    private bool hasDetectedPlayer = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<EnemyAnimationController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isDead || player == null || agent == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            // Primera vez que detecta al player
            if (!hasDetectedPlayer)
            {
                hasDetectedPlayer = true;
                if (animController != null)
                {
                    animController.StartChasing();
                }
            }
            
            if (distance > attackRange)
            {
                // Perseguir al jugador
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else
            {
                // Atacar
                agent.isStopped = true;
                LookAtPlayer();

                if (Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }
        else
        {
            // Si sale del rango de detección, detener
            if (hasDetectedPlayer)
            {
                hasDetectedPlayer = false;
                agent.isStopped = true;
                if (animController != null)
                {
                    animController.StopChasing();
                }
            }
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    void Attack()
    {
        if (animController != null)
        {
            animController.PlayAttack();
        }

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null)
        {
            health = player.GetComponentInChildren<PlayerHealth>();
        }

        if (health != null)
        {
            health.TakeDamage(attackDamage);
        }

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    public void Die()
    {
        isDead = true;
        if (agent != null) agent.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}