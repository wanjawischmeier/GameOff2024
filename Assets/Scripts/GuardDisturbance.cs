using UnityEngine;

public class GuardDisturbance : MonoBehaviour
{
    public Transform guardsParent, player;
    public float sinkDisruptionTime = 2;

    Transform[] guards;

    private void Start()
    {
        guards = new Transform[guardsParent.childCount];
        for (int childIndex = 0; childIndex < guards.Length; childIndex++)
        {
            guards[childIndex] = guardsParent.GetChild(childIndex);
        }
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
            if ((disruptionPosition - player.position).magnitude > PlayerController.Instance.interactionRadius)
            {
                return;
            }

            switch (hit.collider.name)
            {
                case "Sink":
                    Transform guardTransform = GuardController.ClosestToPlayer.transform;
                    guardTransform.GetComponent<GuardController>().Disrupt(disruptionPosition, sinkDisruptionTime);
                    break;

                default:
                    break;
            }
        }
    }
}
