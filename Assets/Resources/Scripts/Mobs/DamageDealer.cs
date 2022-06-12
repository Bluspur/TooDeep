using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    private bool firedOnce = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (firedOnce) { return; }
        Killable player = collision.gameObject.GetComponent<Killable>();
        if (player != null)
        {
            firedOnce = true;
            player.HandleHit();
        }
    }
}
