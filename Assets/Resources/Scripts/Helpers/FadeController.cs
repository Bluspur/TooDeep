using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bluspur.Helpers
{
    public class FadeController : MonoBehaviour
    {
        public void FadeAlpha(Image image, float startValue, float endValue, float time)
        {
            StartCoroutine(DoFade(image, startValue, endValue, time));
        }

        public void FadeAlpha(TMP_Text image, float startValue, float endValue, float time)
        {
            StartCoroutine(DoFade(image, startValue, endValue, time));
        }

        public void FadeAlpha(CanvasGroup image, float startValue, float endValue, float time)
        {
            StartCoroutine(DoFade(image, startValue, endValue, time));
        }

        private IEnumerator DoFade(Image image, float startValue, float endValue, float time)
        {
            float startTime = Time.time;
            image.color = new Color(image.color.r, image.color.g, image.color.b, startValue);
            if (startValue < endValue)
            {
                while (image.color.a < endValue)
                {
                    float newAlpha = Mathf.Lerp(startValue, endValue, Mathf.Clamp((Time.time - startTime) / time, 0f, 1f));
                    image.color = new Color(image.color.r, image.color.g, image.color.b, newAlpha);
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (startValue > endValue)
            {
                while (image.color.a > endValue)
                {
                    float newAlpha = Mathf.Lerp(startValue, endValue, Mathf.Clamp((Time.time - startTime) / time, 0f, 1f));
                    image.color = new Color(image.color.r, image.color.g, image.color.b, newAlpha);
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }

        private IEnumerator DoFade(TMP_Text image, float startValue, float endValue, float time)
        {
            float startTime = Time.time;
            image.color = new Color(image.color.r, image.color.g, image.color.b, startValue);
            if (startValue < endValue)
            {
                while (image.color.a < endValue)
                {
                    float newAlpha = Mathf.Lerp(startValue, endValue, Mathf.Clamp((Time.time - startTime) / time, 0f, 1f));
                    image.color = new Color(image.color.r, image.color.g, image.color.b, newAlpha);
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (startValue > endValue)
            {
                while (image.color.a > endValue)
                {
                    float newAlpha = Mathf.Lerp(startValue, endValue, Mathf.Clamp((Time.time - startTime) / time, 0f, 1f));
                    image.color = new Color(image.color.r, image.color.g, image.color.b, newAlpha);
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }

        private IEnumerator DoFade(CanvasGroup image, float startValue, float endValue, float time)
        {
            float startTime = Time.time;
            image.alpha = startValue;
            if (startValue < endValue)
            {
                while (image.alpha < endValue)
                {
                    float newAlpha = Mathf.Lerp(startValue, endValue, Mathf.Clamp((Time.time - startTime) / time, 0f, 1f));
                    image.alpha = newAlpha;
                    yield return new WaitForSeconds(0.01f);
                }
            }
            else if (startValue > endValue)
            {
                while (image.alpha > endValue)
                {
                    float newAlpha = Mathf.Lerp(startValue, endValue, Mathf.Clamp((Time.time - startTime) / time, 0f, 1f));
                    image.alpha = newAlpha;
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
    }
}

