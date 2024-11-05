using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardController : MonoBehaviour
{
    public enum WaypointMode
    {
        Circular, BackAndForth
    }

    public Transform route;
    public Transform bodyTransform;
    public WaypointMode waypointMode;
    public float waypointTolerance = 0.2f;

    Transform[] waypoints;
    NavMeshAgent agent;
    int waypointIndex = 0;
    int waypointDirection = 1;
    bool disrupted = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        waypoints = new Transform[route.childCount];
        for (int childIndex = 0; childIndex < waypoints.Length; childIndex++)
        {
            waypoints[childIndex] = route.GetChild(childIndex);
        }
    }

    private void Update()
    {
        if (agent.remainingDistance < agent.radius + waypointTolerance)
        {
            disrupted = false;

            // next waypoint
            switch (waypointMode)
            {
                case WaypointMode.Circular:
                    if (waypointIndex == waypoints.Length - 1)
                    {
                        // wrap back around
                        waypointIndex = 0;
                    }
                    else waypointIndex++;
                    break;

                case WaypointMode.BackAndForth:
                    if (waypointDirection == 1)
                    {
                        if (waypointIndex == waypoints.Length - 1)
                        {
                            waypointDirection = -1;
                        }
                    }
                    else
                    {
                        if (waypointIndex == 0)
                        {
                            waypointDirection = 1;
                        }
                    }

                    waypointIndex += waypointDirection;
                    break;

                default:
                    break;
            }
        }

        if (!disrupted)
        {
            agent.SetDestination(waypoints[waypointIndex].position);
        }

        agent.velocity.SetAngleBasedOnVelocity(ref bodyTransform);
    }

    public void Disrupt(Vector3 disruptionPosition)
    {
        agent.SetDestination(disruptionPosition);
        disrupted = true;
    }
}
