using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurseWall : BaseSimpleMovement
{
    [SerializeField] private float speedWithMaxCoins = 6f;

    public bool paused = false;
    private GameManager manager = null;
    public float currentSpeed;

    private void Start()
    {
        manager = FindObjectOfType<GameManager>();
    }

    protected override void MoveObject(Vector2 velocity)
    {
        if(!paused)
        {
            base.objectRigidBody.MovePosition(base.objectRigidBody.position + new Vector2(currentSpeed, 0f) * Time.fixedDeltaTime);
        }
    }

    private void Update()
    {
        currentSpeed = CalculateSpeedScaling();
    }

    private float CalculateSpeedScaling()
    {
        float growthRate = Mathf.Log(speedWithMaxCoins / movementSpeed) / manager.totalCoinsInGame;
        float currentSpeed = movementSpeed * Mathf.Exp(growthRate * manager.CollectedCoins);
        return Mathf.Clamp(currentSpeed, movementSpeed, speedWithMaxCoins);
    }

    public float GetMinSpeed()
    {
        return movementSpeed;
    }

    public float GetMaxSpeed()
    {
        return speedWithMaxCoins;
    }
}
