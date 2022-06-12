using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bluspur.Collectables;
using System;

public class GameManager : MonoBehaviour
{
    public int totalCoinsInGame = 75;
    public int CollectedCoins { get; private set; }

    private void Awake()
    {
        Cursor.visible = false;

        int countGameManagers = FindObjectsOfType<GameManager>().Length;
        if(countGameManagers > 1)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            LevelManager.OnGameEnded += Reset;
            Collectable.OnCoinCollected += AddCoins;
        }
    }

    private void OnDestroy()
    {
        Collectable.OnCoinCollected -= AddCoins;
        LevelManager.OnGameEnded += Reset;
    }

    private void AddCoins(int value)
    {
        CollectedCoins += value;
    }

    private void Reset()
    {
        CollectedCoins = 0;
    }
}
