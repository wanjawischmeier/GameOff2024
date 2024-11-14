using UnityEngine;

public class AnalogClock : MonoBehaviour
{
    public RectTransform minuteHand;
    public RectTransform hourHand;
    public float dayDuration = 30f;

    float totalTime = 0;
    float currentTime = 0;

    const int hoursInDay = 24, minutesInHour = 60;
    const float hoursToDegrees = 360 / 12, minutesToDegrees = 360 / 60;

    float Hour => currentTime * hoursInDay / dayDuration;

    float Minutes => (currentTime * hoursInDay * minutesInHour / dayDuration) % minutesInHour;

    string Clock24Hour => Mathf.FloorToInt(Hour).ToString("00") + ":" + Mathf.FloorToInt(Minutes).ToString("00");

    string Clock12Hour
    {
        get
        {
            int hour = Mathf.FloorToInt(Hour);
            string abbreviation = "AM";

            if (hour >= 12)
            {
                abbreviation = "PM";
                hour -= 12;
            }

            if (hour == 0) hour = 12;

            return hour.ToString("00") + ":" + Mathf.FloorToInt(Minutes).ToString("00") + " " + abbreviation;
        }
    }

    private void Update()
    {
        totalTime += Time.deltaTime;
        currentTime = totalTime % dayDuration;

        hourHand.rotation = Quaternion.Euler(0, 0, -Hour * hoursToDegrees);
        minuteHand.rotation = Quaternion.Euler(0, 0, -Minutes * minutesToDegrees);
    }
}