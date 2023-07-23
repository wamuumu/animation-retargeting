using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class MovementAI : MonoBehaviour
{
    public float area = 30; // Radius of the walkable area
    public float xCenter = 0; // x coordinate of the area
    public float zCenter = 0; // z coordinate of the area

    private Animator anim;
    private NavMeshAgent agent;

    // Useful for debug
    public bool velocity;
    public bool path;

    // Default methods

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        InitSettings();
    }

    void Update()
    {
        if(agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.destination = RandomNavSphere(transform.position, area);
        }

        anim.SetFloat("SpeedMultiplier", agent.velocity.magnitude);
    }

    void OnAnimatorMove()
    {
        // Update position based on animation movement using navigation surface height
        Vector3 position = anim.rootPosition;
        position.y = agent.nextPosition.y;
        transform.position = position;
        agent.nextPosition = transform.position;
    }

    void OnDrawGizmos()
    {
        if (velocity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + agent.velocity);
        }

        if (path)
        {   
            Gizmos.color = Color.black;
            var agentPath = agent.path;
            Vector3 prevCorner = transform.position;
            foreach (var corner in agentPath.corners)
            {
                Gizmos.DrawLine(prevCorner, corner);
                Gizmos.DrawSphere(corner, 0.1f);
                prevCorner = corner;
            }
        }
    }

    // Settings and Random Point

    void InitSettings()
    {
        #region Agent Settings
        agent.speed = 1;
        agent.angularSpeed = 720;
        agent.radius = .75f;
        agent.acceleration = 5;
        agent.stoppingDistance = 1;

        // Disable position update to allow synchronization between agent and animator
        agent.updatePosition = false;
        agent.updateRotation = true;
        #endregion
    }

    Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        origin.x = xCenter;
        origin.z = zCenter;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, -1);

        return navHit.position;
    }
}