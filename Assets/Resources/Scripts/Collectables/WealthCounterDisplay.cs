using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bluspur.Collectables;
using TMPro;
using UnityEngine.UI;

public class WealthCounterDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Image angerFillImage;
    GameManager manager = null;
    CurseWall curseWall = null;

    private void Start()
    {
        manager = FindObjectOfType<GameManager>();
        curseWall = FindObjectOfType<CurseWall>();
        SetText(manager.CollectedCoins);
    }

    private void Update()
    {
        SetText(manager.CollectedCoins);
        SetAngerFill();
    }

    private void SetText(int value)
    {
        displayText.text = $"x{value}";
    }

    private void SetAngerFill()
    {
        float fill = (curseWall.currentSpeed - curseWall.GetMinSpeed()) / (curseWall.GetMaxSpeed() - curseWall.GetMinSpeed());
        angerFillImage.fillAmount = fill;
    }
}
