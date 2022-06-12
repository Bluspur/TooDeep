using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bluspur.Helpers
{
    public class UIHelpers : MonoBehaviour
    {
        public void ShowCanvasForSeconds(GameObject canvas, float duration)
        {
            StartCoroutine(DoShowCanvasForSeconds(canvas, duration));
        }

        private IEnumerator DoShowCanvasForSeconds(GameObject canvas, float duration)
        {
            canvas.SetActive(true);
            yield return new WaitForSeconds(duration);
            canvas.SetActive(false);
        }
    }

}
