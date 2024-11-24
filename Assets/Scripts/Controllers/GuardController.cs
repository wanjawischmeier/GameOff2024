using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardController : MonoBehaviour
{
    public enum WaypointMode
    {
        Circular, BackAndForth
    }

    public enum BehaviourMode
    {
        GameOver, Pursue
    }

    public Transform route;
    public Transform bodyTransform;
    public PolygonCollider2D flashlightConeCollider;
    public WaypointMode waypointMode;
    public BehaviourMode behaviourMode = BehaviourMode.GameOver;
    public float waypointTolerance = 0.2f;
    public float waypointTime = 0.5f;
    public float moveSpeed = 1;
    public float sprintSpeed = 2;
    public float rotationSpeed = 5;
    public float pursuitCatchDistance = 2;

    public bool isStopped
    { 
        get => agent.isStopped;
        set
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = value;
        }
    }

    public static Transform BodyTransform { get; private set; }

    Transform[] waypoints;

    [HideInInspector]
    public NavMeshAgent agent;
    [HideInInspector]
    public int waypointIndex = 0;
    [HideInInspector]
    public int waypointDirection = 1;
    [HideInInspector]
    public float currentWaypointTime = 0;

    const float pursuitTargetUpdateTime = 0.01f;

    private void Awake()
    {
        BodyTransform = transform.parent;
    }

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
        // move to next waypoint if enough time has passed
        if (agent.remainingDistance < agent.radius + waypointTolerance)
        {
            if (currentWaypointTime == 0)
            {
                MoveToNextWaypoint();
            }
            else
            {
                currentWaypointTime = Mathf.Max(0, currentWaypointTime - Time.deltaTime);
            }
        }

        agent.velocity.SetAngleBasedOnVelocity(bodyTransform, rotationSpeed);
    }

    private void MoveToNextWaypoint()
    {
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
        
        currentWaypointTime = waypointTime;
        agent.speed = moveSpeed;
        agent.SetDestination(waypoints[waypointIndex].position);
    }

    public void Disrupt(Vector3 disruptionPosition, float disruptionTime)
    {
        agent.SetDestination(disruptionPosition);
        currentWaypointTime = disruptionTime;
        agent.speed = sprintSpeed;
    }

    public IEnumerator PursuePlayer()
    {
        agent.speed = sprintSpeed;
        rotationSpeed = -1;

        Transform target = PlayerController.Transform;

        while ((transform.position - target.position).magnitude > pursuitCatchDistance)
        {
            if (enabled)
            {
                agent.SetDestination(target.position);
            }

            yield return new WaitForSeconds(0.1f);
        }

        // player has been caught
        SceneStateManager.ResetSceneState();
        SceneStateManager.ResetInventoryState();
        SceneTransitionFader.TransitionToScene("CampusOutside");
    }   
}