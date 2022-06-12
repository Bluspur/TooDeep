using Bluspur.Movement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killable : MonoBehaviour
{
    public static event Action OnKilled;

    public void HandleHit()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        playerController.enabled = false;
        OnKilled?.Invoke();
        Destroy(gameObject);
    }
}
