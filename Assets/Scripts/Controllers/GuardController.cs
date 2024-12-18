using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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
    public Transform distractionTimeIndicator;
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

    public static GuardController ClosestToPlayer
    {
        get
        {
            Vector3 playerPosition = PlayerController.Transform.position;
            Transform guardTransform, closestGuardTransform = null;
            float sqMagnitude, closestSqMagnitude = -1;

            for (int guardIndex = 0; guardIndex < BodyTransform.childCount; guardIndex++)
            {
                guardTransform = BodyTransform.GetChild(guardIndex);
                sqMagnitude = (guardTransform.position - playerPosition).sqrMagnitude;

                if (closestSqMagnitude == -1 || sqMagnitude < closestSqMagnitude)
                {
                    closestSqMagnitude = sqMagnitude;
                    closestGuardTransform = guardTransform;
                }
            }

            return closestGuardTransform.GetComponent<GuardController>();
        }
    }

    public static Transform BodyTransform { get; private set; }

    Transform[] waypoints;
    Image distractionTimeLoadingBar;

    [HideInInspector]
    public ConePlayerTrigger conePlayerTrigger;
    [HideInInspector]
    public NavMeshAgent agent;
    public int waypointIndex = 0;
    [HideInInspector]
    public int waypointDirection = 1;
    public float currentWaypointTime = -1;

    GameObject toDestroyOponWaypointCompletion = null;
    bool inPursuit = false;

    const float pursuitTargetUpdateTime = 0.01f;
    const float reactionTime = 0.5f;

    private void Awake()
    {
        if (BodyTransform == null)
        {
            BodyTransform = transform.parent;
        }
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        conePlayerTrigger = GetComponentInChildren<ConePlayerTrigger>();
        distractionTimeLoadingBar = distractionTimeIndicator.GetChild(0).GetComponent<Image>();

        if (route == null)
        {
            return;
        }
        
        waypoints = new Transform[route.childCount];
        for (int childIndex = 0; childIndex < waypoints.Length; childIndex++)
        {
            waypoints[childIndex] = route.GetChild(childIndex);
        }
    }

    private void Update()
    {
        if (currentWaypointTime == -1 || inPursuit)
        {
            // no waypoint or currently in pursuit
            return;
        }

        // move to next waypoint if enough time has passed
        if ((transform.position - agent.destination).magnitude < agent.radius + waypointTolerance)
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
    }

    private void FixedUpdate()
    {
        agent.velocity.SetAngleBasedOnVelocity(bodyTransform, rotationSpeed);
    }

    private void MoveToNextWaypoint()
    {
        if (waypoints == null)
        {
            currentWaypointTime = -1;
            return;
        }

        if (toDestroyOponWaypointCompletion != null)
        {
            Destroy(toDestroyOponWaypointCompletion);
        }

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
        inPursuit = false;
        agent.SetDestination(waypoints[waypointIndex].position);
    }

    private IEnumerator MoveToTarget(Vector3 target)
    {
        agent.SetDestination(target);

        while ((transform.position - target).magnitude > pursuitCatchDistance)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator TrackTarget(Transform target)
    {
        while ((transform.position - target.position).magnitude > pursuitCatchDistance)
        {
            if (enabled)
            {
                agent.SetDestination(target.position);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void Disrupt(Vector3 disruptionPosition, float disruptionTime, GameObject toDestroy = null)
    {
        toDestroyOponWaypointCompletion = toDestroy;
        agent.SetDestination(disruptionPosition);
        currentWaypointTime = disruptionTime;
        agent.speed = sprintSpeed;
        inPursuit = true;
    }

    public IEnumerator DisruptItem(GameObject item, float disruptionTime)
    {
        Debug.Log($"Disrupted by {item} for {disruptionTime}s");
        yield return new WaitForSeconds(reactionTime);

        var originalPosition = transform.position;
        var originalRotation = transform.eulerAngles;
        agent.speed = sprintSpeed;

        yield return TrackTarget(item.transform);

        distractionTimeIndicator.gameObject.SetActive(true);
        LeanTween.value(distractionTimeLoadingBar.gameObject, (value) =>
        {
            distractionTimeLoadingBar.fillAmount = value;
        }, 1, 0, disruptionTime).setOnComplete(() =>
        {
            distractionTimeIndicator.gameObject.SetActive(false);
        });
        yield return new WaitForSeconds(disruptionTime);

        Destroy(item);
        agent.speed = moveSpeed;

        if (waypoints == null)
        {
            yield return MoveToTarget(originalPosition);
            LeanTween.rotate(gameObject, originalRotation, 0.1f);
        }
    }

    public IEnumerator PursuePlayer()
    {
        agent.speed = sprintSpeed;
        rotationSpeed = -1;
        inPursuit = true;

        InteractionManager.Instance.disableInteractions = true;
        yield return TrackTarget(PlayerController.Transform);

        // player has been caught
        SceneTransitionFader.ClearStateAndTransitionToChat();
    }
}
