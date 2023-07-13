// LocomotionSimpleAgent.cs
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class MovementAI : MonoBehaviour
{
    public float area = 30;
    public float xCenter = 0;
    public float zCenter = 0;

    private Animator anim;
    private NavMeshAgent agent;

    void Start()
    {
        // Animator
        anim = GetComponent<Animator>();
        anim.applyRootMotion = true;

        // NavMesh Agent
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 1;
        agent.radius = .75f;
        agent.acceleration = 8;

        // Don’t update position automatically
        agent.updatePosition = false;
        agent.updateRotation = true;

        Vector3 newPos = RandomNavSphere(transform.position, area, -1);
        agent.SetDestination(newPos);
    }

    void Update()
    {
        anim.SetFloat("SpeedMultiplier", agent.desiredVelocity.magnitude);

        if(agent.remainingDistance < 1)
        {
            Vector3 newPos = RandomNavSphere(transform.position, area, -1);
            agent.SetDestination(newPos);
        }
    }

    void OnAnimatorMove()
    {
        // Update position based on animation movement using navigation surface height
        Vector3 position = anim.rootPosition;
        position.y = agent.nextPosition.y;
        transform.position = position;
        agent.nextPosition = transform.position;
    }

    Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        origin.x = xCenter;
        origin.z = zCenter;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}