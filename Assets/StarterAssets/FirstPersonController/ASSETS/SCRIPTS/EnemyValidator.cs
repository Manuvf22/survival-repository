using UnityEngine;
using UnityEngine.AI;

public class EnemyValidator : MonoBehaviour
{
    [Header("Validation Settings")]
    public float checkInterval = 2f; // Cada cuántos segundos verificar
    public float maxDistanceFromNavMesh = 2f;
    public bool autoFix = true; // Intentar arreglar automáticamente

    private float checkTimer;
    private NavMeshAgent agent;
    private EnemyHealth health;
    private EnemyAI ai;
    private Collider enemyCollider;
    private bool hasWarned = false;

    void Start()
    {
        ValidateOnStart();
        checkTimer = checkInterval;
    }

    void ValidateOnStart()
    {
        // Verificar componentes esenciales
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
        ai = GetComponent<EnemyAI>();
        enemyCollider = GetComponent<Collider>();

        bool hasErrors = false;

        // Verificar NavMeshAgent
        if (agent == null)
        {
            Debug.LogError("[ENEMY VALIDATOR] " + gameObject.name + " - Missing NavMeshAgent!");
            hasErrors = true;
        }

        // Verificar EnemyHealth
        if (health == null)
        {
            Debug.LogError("[ENEMY VALIDATOR] " + gameObject.name + " - Missing EnemyHealth script!");
            hasErrors = true;
        }

        // Verificar EnemyAI
        if (ai == null)
        {
            Debug.LogError("[ENEMY VALIDATOR] " + gameObject.name + " - Missing EnemyAI script!");
            hasErrors = true;
        }

        // Verificar Collider
        if (enemyCollider == null)
        {
            Debug.LogError("[ENEMY VALIDATOR] " + gameObject.name + " - Missing Collider!");
            hasErrors = true;
        }
        else if (enemyCollider.isTrigger)
        {
            Debug.LogWarning("[ENEMY VALIDATOR] " + gameObject.name + " - Collider is set to Trigger! This will prevent hits.");
            if (autoFix)
            {
                enemyCollider.isTrigger = false;
                Debug.Log("[ENEMY VALIDATOR] Fixed: Collider is no longer a trigger");
            }
        }

        // Verificar Tag
        if (!gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning("[ENEMY VALIDATOR] " + gameObject.name + " - Wrong tag: " + gameObject.tag);
            if (autoFix)
            {
                gameObject.tag = "Enemy";
                Debug.Log("[ENEMY VALIDATOR] Fixed: Tag set to 'Enemy'");
            }
        }

        // Verificar Layer
        if (gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
        {
            Debug.LogWarning("[ENEMY VALIDATOR] " + gameObject.name + " - On 'Ignore Raycast' layer!");
            if (autoFix)
            {
                gameObject.layer = 0; // Default layer
                Debug.Log("[ENEMY VALIDATOR] Fixed: Changed to Default layer");
            }
        }

        // Verificar NavMesh
        if (agent != null && !agent.isOnNavMesh)
        {
            Debug.LogWarning("[ENEMY VALIDATOR] " + gameObject.name + " - NOT on NavMesh at position: " + transform.position);

            if (autoFix)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                    Debug.Log("[ENEMY VALIDATOR] Fixed: Repositioned to NavMesh at " + hit.position);
                }
                else
                {
                    Debug.LogError("[ENEMY VALIDATOR] CRITICAL: Cannot find valid NavMesh position! Destroying enemy...");
                    Destroy(gameObject);
                    return;
                }
            }
        }

        if (!hasErrors)
        {
            Debug.Log("[ENEMY VALIDATOR] ✓ " + gameObject.name + " - All checks passed!");
        }
    }

    void Update()
    {
        checkTimer -= Time.deltaTime;

        if (checkTimer <= 0)
        {
            checkTimer = checkInterval;
            PerformRuntimeChecks();
        }
    }

    void PerformRuntimeChecks()
    {
        // Verificar si sigue en el NavMesh
        if (agent != null && agent.enabled)
        {
            if (!agent.isOnNavMesh)
            {
                if (!hasWarned)
                {
                    Debug.LogWarning("[ENEMY VALIDATOR] " + gameObject.name + " fell off NavMesh!");
                    hasWarned = true;
                }

                if (autoFix)
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(transform.position, out hit, maxDistanceFromNavMesh, NavMesh.AllAreas))
                    {
                        agent.Warp(hit.position);
                        Debug.Log("[ENEMY VALIDATOR] Repositioned enemy back to NavMesh");
                        hasWarned = false;
                    }
                    else
                    {
                        Debug.LogError("[ENEMY VALIDATOR] Enemy too far from NavMesh. Destroying...");
                        Destroy(gameObject);
                    }
                }
            }
        }

        // Verificar si está cayendo infinitamente
        if (transform.position.y < -10f)
        {
            Debug.LogError("[ENEMY VALIDATOR] Enemy fell through world! Destroying...");
            Destroy(gameObject);
        }
    }

    // Método público para forzar validación
    public void ForceValidation()
    {
        ValidateOnStart();
        PerformRuntimeChecks();
    }

    void OnDrawGizmos()
    {
        if (agent != null && !agent.isOnNavMesh)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}