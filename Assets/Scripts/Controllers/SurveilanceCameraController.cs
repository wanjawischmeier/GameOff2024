using System.Collections;
using UnityEngine;

public class SurveilanceCameraController : MonoBehaviour
{
    public float rotationTarget = 0;
    public float rotationTime = 2;

    float startRotation;
    Coroutine coroutine;

    private void Start()
    {
        if (rotationTarget == 0)
        {
            return;
        }

        startRotation = transform.eulerAngles.z;
        coroutine = StartCoroutine(Rotate());
    }

    private void OnDestroy()
    {
        StopCoroutine(coroutine);
    }

    private IEnumerator Rotate()
    {
        while (true)
        {
            var tween = LeanTween.rotateZ(gameObject, rotationTarget, rotationTime);
            tween.setEaseInOutQuad();

            yield return new WaitForSeconds(rotationTime);

            tween = LeanTween.rotateZ(gameObject, startRotation, rotationTime);
            tween.setEaseInOutQuad();

            yield return new WaitForSeconds(rotationTime);
        }
    }
}
