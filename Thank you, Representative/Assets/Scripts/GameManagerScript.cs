using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManagerScript : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI approvalText;
    public TextMeshProUGUI apText;

    [Header("Core stats")]
    private int money = 50000;
    private int approval = 50;
    private int actionPoints = 8;
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();
    }

    public void ApprovePolicy()
    {
        if (actionPoints >= 4)
        {
            actionPoints -= 4;

            money -= 10000;
            approval += 15;

            UpdateUI(); 
        }   
    }

    private void UpdateUI()
    {
        moneyText.text = "$" + money.ToString("N0");
        approvalText.text = "Approval: " + approval.ToString() + "%";
        apText.text = "AP: " + actionPoints.ToString();
    }
}
