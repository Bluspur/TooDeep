using Bluspur.Helpers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScoreScreenManager : MonoBehaviour
{
    [Header("Gamedata references")]
    [SerializeField] private LevelManager levelManager;

    [Header("Coins Collected Display")]
    [SerializeField] private CanvasGroup coinDisplayPanel = null;
    [SerializeField] private TMP_Text coinCountText;
    [SerializeField] private float delayBeforeCoinCount = 1f;
    [SerializeField] private float delayPerCoinCounted = 0.1f;
    [SerializeField] private float fadeInCoinDelay = 2f;

    [Header("Input Prompt")]
    [SerializeField] private GameObject keyPromptObject;

    [Header("Credits Display")]
    [SerializeField] private CanvasGroup creditsDisplayPanel = null;
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] private string[] individualCredits;
    [SerializeField] private float creditFadeInTime = 1f;
    [SerializeField] private float creditShownTime = 2f;
    [SerializeField] private float creditFadeOutTime = 1f;

    [Header("Elevator Animation")]
    [SerializeField] private GameObject elevatorObject;
    [SerializeField] private float delayBeforeExitTransition = 2f;

    private FadeController fadeController;
    private GameManager gameManager;

    private bool awaitingInput = false;
    private bool inputReceived = false;
    private bool endReady = false;

    public void OnContinueKey(InputAction.CallbackContext context)
    {
        if(awaitingInput)
        {
            inputReceived = true;
            awaitingInput = false;
        }
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        fadeController = GetComponent<FadeController>();
        keyPromptObject.SetActive(false);
        coinDisplayPanel.alpha = 0f;
        creditsDisplayPanel.alpha = 0f;
        SetScoreText(0, gameManager.totalCoinsInGame);
        StartCoroutine(CountCoins());
    }

    private void Update()
    {
        if(inputReceived)
        {
            inputReceived = false;
            StartCoroutine(ShowCredits());
        }

        if(awaitingInput)
        {
            keyPromptObject.SetActive(true);
        }
        else
        {
            keyPromptObject.SetActive(false);
        }

        if(endReady)
        {
            endReady = false;
            StartCoroutine(EndScene());
        }
    }

    private IEnumerator CountCoins()
    {
        yield return new WaitForSeconds(delayBeforeCoinCount);
        fadeController.FadeAlpha(coinDisplayPanel, 0, 1, fadeInCoinDelay);
        yield return new WaitForSeconds(fadeInCoinDelay);
        for (int i = 0; i <= gameManager.CollectedCoins; i++)
        {
            SetScoreText(i, gameManager.totalCoinsInGame);
            yield return new WaitForSeconds(delayPerCoinCounted);
        }
        awaitingInput = true;
    }

    private IEnumerator ShowCredits()
    {
        fadeController.FadeAlpha(coinDisplayPanel, 1, 0, fadeInCoinDelay);
        yield return new WaitForSeconds(fadeInCoinDelay + 1);
        foreach (string credit in individualCredits)
        {
            creditsText.text = credit;
            fadeController.FadeAlpha(creditsDisplayPanel, 0, 1, creditFadeInTime);
            yield return new WaitForSeconds(creditFadeInTime + creditShownTime);
            fadeController.FadeAlpha(creditsDisplayPanel, 1, 0, creditFadeOutTime);
            yield return new WaitForSeconds(creditFadeOutTime + 0.5f);
        }
        endReady = true;
    }

    private IEnumerator EndScene()
    {
        elevatorObject.GetComponent<Animator>().SetTrigger("ExitReady");
        yield return new WaitForSeconds(delayBeforeExitTransition);
        levelManager.HandleLevelEnd();
    }

    private void SetScoreText(int collectedCoins, int maxCoins)
    {
        coinCountText.text = $"{collectedCoins} / {maxCoins}";
    }

}
