using NUnit.Framework.Interfaces;
using UnityEngine;

public class GuardDisturbance : MonoBehaviour
{
    public Transform guardsParent, player;

    Transform[] guards;
    PlayerController playerController;

    private void Start()
    {
        guards = new Transform[guardsParent.childCount];
        for (int childIndex = 0; childIndex < guards.Length; childIndex++)
        {
            guards[childIndex] = guardsParent.GetChild(childIndex);
        }

        playerController = player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ProcessTilemapClick();
        }


    }

    private void ProcessTilemapClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            // skip clicks outside the player's interaction range
            Vector3 disruptionPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if ((disruptionPosition - player.position).magnitude > playerController.interactionRadius)
            {
                return;
            }

            switch (hit.collider.name)
            {
                case "Sink":
                    Transform guard = GetClosestGuard(disruptionPosition);
                    guard.GetComponent<GuardController>().Disrupt(disruptionPosition);
                    break;

                default:
                    break;
            }
        }
    }


    private Transform GetClosestGuard(Vector3 position)
    {
        Transform guard;
        Transform closestGuard = guards[0];

        float sqMagnitude = (closestGuard.position - position).sqrMagnitude;
        float closestSqMagnitude = sqMagnitude;

        for (int i = 1; i < guards.Length; i++)
        {
            guard = guards[i];
            sqMagnitude = (guard.position - position).sqrMagnitude;

            if (sqMagnitude < closestSqMagnitude)
            {
                closestSqMagnitude = sqMagnitude;
                closestGuard = guard;
            }
        }

        return closestGuard;
    }
}
