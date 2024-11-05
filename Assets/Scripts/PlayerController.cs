using UnityEngine;

// based on: https://stuartspixelgames.com/2018/06/24/simple-2d-top-down-movement-unity-c/
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public Transform bodyTransform;
    public float runSpeed = 20.0f;
    public float interactionRadius = 2;

    private Rigidbody2D body;

    float horizontal;
    float vertical;
    float moveLimiter = 0.7f;


    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Gives a value between -1 and 1
        horizontal = Input.GetAxisRaw("Horizontal"); // -1 is left
        vertical = Input.GetAxisRaw("Vertical"); // -1 is down

        Vector3 direction = new Vector3(horizontal, vertical, 0);
        direction.SetAngleBasedOnVelocity(ref bodyTransform);
    }

    private void FixedUpdate()
    {
        if (horizontal != 0 && vertical != 0) // Check for diagonal movement
        {
            // limit movement speed diagonally, so you move at 70% speed
            horizontal *= moveLimiter;
            vertical *= moveLimiter;
        }

        body.linearVelocity = new Vector2(horizontal * runSpeed, vertical * runSpeed);
    }
}
