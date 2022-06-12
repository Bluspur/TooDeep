using Bluspur.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitZone : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager = null;

    private bool firedOnce = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if(player && !firedOnce)
        {
            firedOnce = true;
            levelManager.HandleLevelEnd();
        }
    }
}
