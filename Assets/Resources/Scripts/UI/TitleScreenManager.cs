using Bluspur.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject CoinDisplayObject = null;
    [SerializeField] private GameObject TitleDisplayObject = null;
    [SerializeField] private GameObject CutsceneObject = null;
    [SerializeField] private GameObject playerObject = null;
    [SerializeField] private CurseWall curseWall = null;
    [SerializeField] private float cutsceneDuration = 1f;

    private void Awake()
    {
        CoinDisplayObject.SetActive(false);
        TitleDisplayObject.SetActive(true);
        playerObject.GetComponent<PlayerController>().DisableMovement();
        playerObject.GetComponent<PlayerInput>().SwitchCurrentActionMap("TitleMenu");
        curseWall.paused = true;
    }

    public void OnStartKey(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        CoinDisplayObject.SetActive(true);
        TitleDisplayObject.SetActive(false);
        CutsceneObject.GetComponent<Animator>().SetTrigger("Start");
        StartCoroutine(EnablePlayer(cutsceneDuration));
    }

    private IEnumerator EnablePlayer(float duration)
    {
        yield return new WaitForSeconds(duration);
        playerObject.GetComponent<PlayerController>().EnableMovement();
        playerObject.GetComponent<PlayerInput>().SwitchCurrentActionMap("Gameplay");
        playerObject.GetComponentInChildren<SpriteRenderer>().enabled = true;
        curseWall.paused = false;
    }
}
