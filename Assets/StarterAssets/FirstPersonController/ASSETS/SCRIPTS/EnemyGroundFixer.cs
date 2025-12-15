using UnityEngine;
using UnityEngine.AI;

public class EnemyGroundFixer : MonoBehaviour
{
    private NavMeshAgent agent;
    private bool isFixed = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            // Configurar el agent para que se quede pegado al NavMesh
            agent.baseOffset = 0f;
            
            // Esperar un frame para que el NavMesh se inicialice
            Invoke("FixPosition", 0.1f);
        }
    }

    void FixPosition()
    {
        if (agent == null || isFixed) return;

        // Buscar posición en el NavMesh
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            transform.position = hit.position;
            agent.Warp(hit.position);
            isFixed = true;
        }
    }
}