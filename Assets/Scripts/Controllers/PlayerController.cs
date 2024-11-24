using Unity.VisualScripting;
using UnityEngine;

// based on: https://stuartspixelgames.com/2018/06/24/simple-2d-top-down-movement-unity-c/
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public Transform bodyTransform;
    public Animator animator;
    public float runSpeed = 20.0f;
    public float interactionRadius = 2;

    [DoNotSerialize]
    public bool isStopped;

    public static Transform Transform { get; private set; }

    public static PlayerController Instance { get; private set; }

    Rigidbody2D body;
    Joystick joystick;

    float horizontal;
    float vertical;
    float moveLimiter = 0.7f;
    int animatorWalkingBoolId, animatorInteractTriggerId;

    private void Awake()
    {
        Transform = transform;

        // initialize singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }


    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        joystick = FindAnyObjectByType<Joystick>();
        if (Application.isMobilePlatform)
        {
            joystick.gameObject.SetActive(true);
        }

        animatorWalkingBoolId = Animator.StringToHash("Walking");
        animatorInteractTriggerId = Animator.StringToHash("Interact");
    }

    private void Update()
    {
        if (isStopped) return;

        // Gives a value between -1 and 1
        horizontal = Input.GetAxisRaw("Horizontal"); // -1 is left
        vertical = Input.GetAxisRaw("Vertical"); // -1 is down

        if (joystick != null)
        {
            horizontal += joystick.Horizontal * 1.5f;
            vertical += joystick.Vertical * 1.5f;
        }

        Vector3 direction = new Vector3(horizontal, vertical, 0);
        direction.SetAngleBasedOnVelocity(bodyTransform);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            InventoryManager.Instance.TryThrowItem();
        }
    }

    private void FixedUpdate()
    {
        if (isStopped)
        {
            body.linearVelocity = Vector3.zero;
            return;
        }

        if (horizontal == 0 && vertical == 0) // no player movement
        {
            animator.SetBool(animatorWalkingBoolId, false);
        }
        else
        {
            animator.SetBool(animatorWalkingBoolId, true);

            if (horizontal != 0 && vertical != 0) // check for diagonal movement
            {
                // limit movement speed diagonally, so you move at 70% speed
                horizontal *= moveLimiter;
                vertical *= moveLimiter;
            }
        }

        body.linearVelocity = new Vector2(horizontal * runSpeed, vertical * runSpeed);
    }

    public void Interact()
    {
        animator.SetTrigger(animatorInteractTriggerId);
    }
}
