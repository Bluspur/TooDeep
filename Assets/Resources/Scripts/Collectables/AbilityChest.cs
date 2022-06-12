using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bluspur.Movement;
using Bluspur.Helpers;
using TMPro;

public class AbilityChest : MonoBehaviour
{
    private enum AbilityToUnlock
    {
        multiJump,
        dash,
        pogo
    }
    
    [SerializeField] private AbilityToUnlock ability = AbilityToUnlock.multiJump;
    [SerializeField] private GameObject UIObject = null;
    [SerializeField] private GameObject UnlockCanvasObject = null;
    [SerializeField] private TMP_Text unlockText = null;
    [SerializeField] private float unlockMessageDuration = 4f;

    private bool firedOnce = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(firedOnce) { return; }

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if(player)
        {
            firedOnce = true;

            GetComponent<Animator>().SetTrigger("Open");

            switch (ability)
            {
                case AbilityToUnlock.multiJump:
                    unlockText.text = "!! double jump unlocked !!";
                    player.UnlockMultiJump();
                    break;
                case AbilityToUnlock.dash:
                    unlockText.text = "!! dash unlocked !!";
                    player.UnlockDash();
                    break;
                case AbilityToUnlock.pogo:
                    unlockText.text = "!! pickaxe pogo unlocked !!";
                    player.UnlockPogo();
                    break;
                default:
                    break;
            }
            UIObject.GetComponent<UIHelpers>().ShowCanvasForSeconds(UnlockCanvasObject, unlockMessageDuration);
        }
    }
}
