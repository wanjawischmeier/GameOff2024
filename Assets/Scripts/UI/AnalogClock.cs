using UnityEngine;

public class AnalogClock : MonoBehaviour
{
    public RectTransform minuteHand;
    public RectTransform hourHand;
    public float dayDuration = 30f;

    float totalTime = 0;
    float currentTime = 0;
    float hourRotationOffset, minuteRotationOffset;

    const int hoursInDay = 24, minutesInHour = 60;
    const float hoursToDegrees = 360 / 12, minutesToDegrees = 360 / 60;

    float Hour => currentTime * hoursInDay / dayDuration;

    float Minutes => (currentTime * hoursInDay * minutesInHour / dayDuration) % minutesInHour;

    private void Start()
    {
        hourRotationOffset = hourHand.rotation.z * Mathf.Rad2Deg;
        minuteRotationOffset = minuteHand.rotation.z * Mathf.Rad2Deg;
    }

    private void Update()
    {
        totalTime += Time.deltaTime;
        currentTime = totalTime % dayDuration;

        hourHand.rotation = Quaternion.Euler(0, 0, -Hour * hoursToDegrees + hourRotationOffset);
        minuteHand.rotation = Quaternion.Euler(0, 0, -Minutes * minutesToDegrees + minuteRotationOffset);
    }
}