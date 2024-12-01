using UnityEngine;
using UnityEngine.UI;

public class DangerLevelIndicator : MonoBehaviour
{
    public float remainingDistanceThreshold = 2;
    public float dangerLevelDecrementStep = 0.75f;
    public RectTransform background, fill;

    [HideInInspector]
    public Slider slider;
    float dangerLevel = 0;

    public const float sliderFadeinDuration = 0.05f;
    public const float sliderFadeoutDuration = 0.1f;

    public static DangerLevelIndicator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

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

        if (!ConePlayerTrigger.playerCaught)
        {
            slider.value = dangerLevel;
        }

        if (dangerLevel == 0)
        {
            HideSlider();
        }
        else
        {
            ShowSlider();
        }
    }

    public void ShowSlider()
    {
        LeanTween.cancel(fill);
        LeanTween.cancel(background);
        LeanTween.alpha(fill, 1, sliderFadeinDuration);
        LeanTween.alpha(background, 1, sliderFadeinDuration);
    }

    public void HideSlider()
    {
        LeanTween.cancel(fill);
        LeanTween.cancel(background);
        LeanTween.alpha(fill, 0, sliderFadeoutDuration);
        LeanTween.alpha(background, 0, sliderFadeoutDuration);
    }
}
