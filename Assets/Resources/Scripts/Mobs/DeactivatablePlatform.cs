using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivatablePlatform : MonoBehaviour
{
    [SerializeField] private float deactivationTime = 0.2f;

    public void DeactivatePlatform()
    {
        gameObject.GetComponent<Collider2D>().enabled = false;
        StartCoroutine(nameof(WaitToReactivate));
    }

    private IEnumerator WaitToReactivate()
    {
        yield return new WaitForSeconds(deactivationTime);
        gameObject.GetComponent<Collider2D>().enabled = true;
    }
}
