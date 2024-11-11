using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public virtual float interactionRadius { get; } = 1;

    public virtual string interactionMessage { get; } = "interact with this object";

    public abstract void Interact(Transform interactionOverlayParent);

    Transform[] guards;

    private void Start()
    {
        guards = new Transform[GuardController.BodyTransform.childCount];
        for (int childIndex = 0; childIndex < guards.Length; childIndex++)
        {
            guards[childIndex] = GuardController.BodyTransform.GetChild(childIndex);
        }
    }

    public void DisruptGuard(float interactionTime, GameObject overlay, Transform interactionOverlayParent)
    {
        Transform guard = GetClosestGuard(transform.position);
        if (guard == null)
        {
            return;
        }

        RemoveInteractable();
        Instantiate(overlay, interactionOverlayParent);

        var guardController = guard.GetComponent<GuardController>();
        guardController.Disrupt(transform.position, interactionTime);
    }

    public void RemoveInteractable()
    {
        InteractionManager.Instance.RemoveInteractable(this);
        Destroy(gameObject);
    }

    public Transform GetClosestGuard(Vector3 position)
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
