using UnityEngine;
using UnityEngine.UI;

public class DangerLevelIndicator : MonoBehaviour
{
    public float remainingDistanceThreshold = 2;
    public float dangerLevelDecrementStep = 0.75f;

    Slider slider;
    float dangerLevel = 0;

    private void Start()
    {
        slider = GetComponentInChildren<Slider>();
    }

    private void Update()
    {
        float currentDangerLevel = (remainingDistanceThreshold - GuardController.ClosestPlayerRemainingDistance) / remainingDistanceThreshold;
        if (currentDangerLevel >= dangerLevel)
        {
            dangerLevel = currentDangerLevel;
        }
        else
        {
            dangerLevel -= dangerLevelDecrementStep * Time.deltaTime;
        }

        slider.value = dangerLevel;
    }
}
