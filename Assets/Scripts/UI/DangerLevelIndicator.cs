using UnityEngine;
using UnityEngine.UI;

public class DangerLevelIndicator : MonoBehaviour
{
    public float remainingDistanceThreshold = 2;
    public float dangerLevelDecrementStep = 0.75f;
    public RectTransform background, fill;

    Slider slider;
    float dangerLevel = 0;

    static float sliderFadeinDuration = 0.05f;
    static float sliderFadeoutDuration = 0.1f;

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
            dangerLevel = Mathf.Max(0, dangerLevel - dangerLevelDecrementStep * Time.deltaTime);
        }

        slider.value = dangerLevel;

        if (dangerLevel == 0)
        {
            LeanTween.cancel(fill);
            LeanTween.cancel(background);
            LeanTween.alpha(fill, 0, sliderFadeoutDuration);
            LeanTween.alpha(background, 0, sliderFadeoutDuration);
        }
        else
        {
            LeanTween.cancel(fill);
            LeanTween.cancel(background);
            LeanTween.alpha(fill, 1, sliderFadeinDuration);
            LeanTween.alpha(background, 1, sliderFadeinDuration);
        }
    }
}
