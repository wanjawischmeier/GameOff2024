using UnityEngine;

public static class Utility
{
    public static void SetAngleBasedOnVelocity(this Vector3 velocity, Transform transform, float rotationSpeed = 20)
    {
        if (velocity != Vector3.zero)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            if (rotationSpeed == -1)
            {
                transform.rotation = targetRotation;
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    public static Vector2 StripZ(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.y);
    }

    public static Vector3 RandomlySpreadVector(this Vector3 vector, float spreadFactor)
    {
        return new Vector3()
        {
            x = vector.x + Random.Range(-spreadFactor, spreadFactor),
            y = vector.y + Random.Range(-spreadFactor, spreadFactor),
            z = vector.z + Random.Range(-spreadFactor, spreadFactor),
        };
    }
}
