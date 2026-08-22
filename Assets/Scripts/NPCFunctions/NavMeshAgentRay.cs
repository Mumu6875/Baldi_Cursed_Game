#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class NavMeshAgentRay : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent == null || !agent.hasPath || agent.path.corners == null || agent.path.corners.Length < 2)
        {
            return;
        }

        Gizmos.color = pathColor;
        Vector3[] corners = agent.path.corners;

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Gizmos.DrawLine(corners[i], corners[i + 1]);
            Gizmos.DrawSphere(corners[i], 0.1f);
        }
    }
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [Header("Settings")]
    [SerializeField] private bool showPath = true;
    [SerializeField] private Color pathColor = Color.red;
    [SerializeField] private float pathWidth = 15f;
}
#endif