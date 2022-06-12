using Bluspur.Helpers;
using Bluspur.Movement;
using Bluspur.Collectables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Image fadeableImage = null;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float deathFadeOutDuration = 1f;
    [SerializeField] private float levelEndFadeOutDuration = 0.5f;

    [Header("Level End References")]
    [SerializeField] private GameObject elevatorObject = null;
    [SerializeField] private GameObject playerObject = null;

    public static event Action OnGameEnded;

    private int coinsCollectedThisLevel = 0;

    private void Awake()
    {
        Collectable.OnCoinCollected += AddCoins;
        Killable.OnKilled += HandlePlayerKilled;
        fadeableImage.gameObject.SetActive(true);
    }

    private void Start()
    {
        GetComponent<FadeController>().FadeAlpha(fadeableImage, 1, 0, fadeInDuration);
    }

    private void OnDestroy()
    {
        Collectable.OnCoinCollected -= AddCoins;
        Killable.OnKilled -= HandlePlayerKilled;
    }

    private void HandlePlayerKilled()
    {
        GetComponent<FadeController>().FadeAlpha(fadeableImage, 0, 1, deathFadeOutDuration);
        
        StartCoroutine(RestartStageDelayed(deathFadeOutDuration));
    }

    public void HandleLevelEnd()
    {
        if(elevatorObject)
        {
            DisablePlayer();
            AnimateElevator();
        }

        StartCoroutine(DoLevelChange(levelEndFadeOutDuration));
    }

    private IEnumerator DoLevelChange(float duration)
    {
        GetComponent<FadeController>().FadeAlpha(fadeableImage, 0, 1, duration);
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void RestartGame()
    {
        OnGameEnded?.Invoke();
        SceneManager.LoadScene(0);
    }

    private void AnimateElevator()
    {
        elevatorObject.GetComponent<Animator>().SetTrigger("Up");
    }

    private void DisablePlayer()
    {
        playerObject.GetComponent<Killable>().enabled = false;
        playerObject.GetComponent<PlayerController>().DisableMovement();
        playerObject.transform.parent = elevatorObject.transform;
    }

    public void RestartStage()
    {
        Collectable.AddCoins(-coinsCollectedThisLevel);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator RestartStageDelayed(float delay)
    {
        Collectable.AddCoins(-coinsCollectedThisLevel);
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void AddCoins(int value)
    {
        coinsCollectedThisLevel += value;
    }
}
