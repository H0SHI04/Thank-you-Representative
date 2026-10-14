using UnityEngine;
using TMPro;
using UnityEngine.UI; 
using System.Collections; // Required for the screen shake Coroutine

[System.Serializable]
public class Policy
{
    public string policyName;
    [TextArea] public string description;
    public int apCost = 4;
    public int approvalChange;
    public int moneyChange;
    
    [Header("Lobbyist Trap")]
    public bool hasLobbyistOffer;
    public int riderMoneyBonus = 40000;
    public int riderApprovalPenalty = -20;
}

public class GameManagerScript : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI approvalText;
    public TextMeshProUGUI apText;
    public TextMeshProUGUI dayText;
    
    [Header("Policy UI")]
    public TextMeshProUGUI policyTitleText;
    public TextMeshProUGUI policyDescText;
    
    [Header("Card Buttons")]
    // Approve and Veto buttons removed to support physical stamps
    public Button inspectButton; 

    [Header("Campaign Buttons")]
    public Button townHallButton;
    public Button mediaAdsButton;

    [Header("Lobbyist UI")]
    public GameObject lobbyistPanel;
    public TextMeshProUGUI offerText;

    [Header("Core Stats")]
    private int money = 40000;
    private int approval = 50;
    private int actionPoints = 8;
    private int currentDay = 1;
    private int dailyApprovalDecay = 5; 

    [Header("Policy Roster")]
    public Policy[] dailyPolicies = new Policy[30]; 
    private int policyIndex = 0;
    
    private bool isInspected = false;
    private bool policyDealtWith = false; 

    void Start()
    {
        Generate30DayDeck();
        lobbyistPanel.SetActive(false); 
        LoadNextPolicy();
        UpdateUI();
    }

    private void Generate30DayDeck()
    {
        for(int i = 0; i < 30; i++)
        {
            Policy p = new Policy();
            int day = i + 1;
            if (day <= 10) 
            {
                p.policyName = "Day " + day + " Bill: Civic Upgrade";
                p.description = "A standard democratic bill proposing local improvements.";
                p.apCost = 4;
                p.moneyChange = Random.Range(-12000, -8000);
                p.approvalChange = Random.Range(8, 12); 
                p.hasLobbyistOffer = (day == 5 || day == 9); 
                p.riderMoneyBonus = 20000;
                p.riderApprovalPenalty = -10;
            }
            else if (day <= 20) 
            {
                p.policyName = "Day " + day + " Bill: Structural Reform";
                p.description = "A labor and regulation bill. The cost to execute is steep.";
                p.apCost = 4;
                p.moneyChange = Random.Range(-20000, -15000);
                p.approvalChange = 15;
                p.hasLobbyistOffer = (Random.value > 0.4f); 
                p.riderMoneyBonus = 30000;
                p.riderApprovalPenalty = -15;
            }
            else 
            {
                p.policyName = "Day " + day + " Bill: Desperate Measures";
                p.description = "A massive infrastructure overhaul. The lobbyists are circling.";
                p.apCost = Random.Range(2, 7); 
                p.moneyChange = Random.Range(-35000, -25000);
                p.approvalChange = 20;
                p.hasLobbyistOffer = (Random.value > 0.1f); 
                p.riderMoneyBonus = 50000;
                p.riderApprovalPenalty = Random.Range(-30, -20);
            }
            dailyPolicies[i] = p;
        }
    }

    private void LoadNextPolicy()
    {
        if (policyIndex < dailyPolicies.Length)
        {
            isInspected = false;
            policyDealtWith = false;
            
            if (inspectButton != null) inspectButton.interactable = true;
            
            Policy p = dailyPolicies[policyIndex];
            policyTitleText.text = p.policyName;
            
            policyDescText.text = "Contents unknown.\n\nInspect (4 AP) to read the bill.\nApprove blindly (0 AP) for a 50/50 gamble.";
        }
    }

    public void InspectPolicy()
    {
        if (!isInspected && !policyDealtWith && actionPoints >= 4)
        {
            actionPoints -= 4;
            isInspected = true;
            if (inspectButton != null) inspectButton.interactable = false; 
            
            Policy p = dailyPolicies[policyIndex];
           
            if (p.hasLobbyistOffer)
            {
                offerText.text = "Lobbyists are on the line.\nThey will " + 
                                 "donate $" + p.riderMoneyBonus.ToString("N0") + " to your campaign if you attach a deregulatory rider to this bill.\n\n" +
                                 "It will cost you " + Mathf.Abs(p.riderApprovalPenalty) + "% Voter Approval.";
                lobbyistPanel.SetActive(true);
            }
            else
            {
                RevealPolicyStats(); 
            }
            
            UpdateUI();
        }
    }
    
    public void AcceptRider()
    {
        Policy p = dailyPolicies[policyIndex];
        
        p.moneyChange += p.riderMoneyBonus;
        p.approvalChange += p.riderApprovalPenalty;
        p.description += "\n\n<color=red>[CORPORATE RIDER ATTACHED]</color>";
        
        lobbyistPanel.SetActive(false);
        RevealPolicyStats();
    }

    public void DeclineRider()
    {
        lobbyistPanel.SetActive(false);
        RevealPolicyStats();
    }

    private void RevealPolicyStats()
    {
        Policy p = dailyPolicies[policyIndex];
        policyDescText.text = p.description + "\n\nEffect: " + p.approvalChange + "% Approval, $" + p.moneyChange;
    }

    public void ApprovePolicy()
    {
        if (policyDealtWith) return;
        Policy p = dailyPolicies[policyIndex];

        // Add Juice: Shake the screen when the stamp hits!
        StartCoroutine(ScreenShake(0.2f, 25f));

        if (!isInspected && p.hasLobbyistOffer)
        {
            money = Mathf.Max(0, money + p.moneyChange + p.riderMoneyBonus);
            approval = Mathf.Clamp(approval + p.approvalChange + p.riderApprovalPenalty, 0, 100);
            
            LockCardAfterAction("You blindly approved the bill.\n\n<color=red>A hidden corporate rider was attached!</color>\nEffect: " + 
                                (p.approvalChange + p.riderApprovalPenalty) + "% Approval, $" + (p.moneyChange + p.riderMoneyBonus));
        }
        else
        {
            if (money + p.moneyChange < 0)
            {
                policyDescText.text = p.description + "\n\n<color=red>Insufficient funds to execute this policy. You must Veto, or find outside funding.</color>";
                return; 
            }

            money += p.moneyChange;
            approval = Mathf.Clamp(approval + p.approvalChange, 0, 100); 
            LockCardAfterAction("Policy Approved.\nEffect: " + p.approvalChange + "% Approval, $" + p.moneyChange);
        }
    }

    public void VetoPolicy()
    {
        if (policyDealtWith) return;

        // Add Juice: Shake the screen when the stamp hits!
        StartCoroutine(ScreenShake(0.2f, 15f));

        LockCardAfterAction("Policy Vetoed. Status quo maintained.");
    }

    private void LockCardAfterAction(string resultText)
    {
        policyDealtWith = true;
        if (inspectButton != null) inspectButton.interactable = false;
        policyDescText.text = resultText;
        
        UpdateUI();
        CheckLoseCondition();
    }

    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        Transform canvasTransform = FindObjectOfType<Canvas>().transform;
        float elapsed = 0.0f;
        Vector3 currentOffset = Vector3.zero;

        while (elapsed < duration)
        {
            // 1. Revert last frame's offset so we don't drift off screen
            foreach (Transform child in canvasTransform) { child.localPosition -= currentOffset; }

            // 2. Create a random punchy offset
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            currentOffset = new Vector3(x, y, 0);

            // 3. Apply the new offset to all UI elements (Desk, Cards, Text)
            foreach (Transform child in canvasTransform) { child.localPosition += currentOffset; }

            elapsed += Time.deltaTime;
            yield return null; // Wait for Unity to render the frame
        }

        // 4. Clean up the final offset so the desk goes exactly back to normal
        foreach (Transform child in canvasTransform) { child.localPosition -= currentOffset; }
    }

    public void HostTownHall()
    {
        if (actionPoints >= 3)
        {
            actionPoints -= 3;
            approval = Mathf.Clamp(approval + 5, 0, 100);
            UpdateUI();
        }
    }

    public void BuyMediaAds()
    {
        if (money >= 15000)
        {
            money -= 15000;
            approval = Mathf.Clamp(approval + 15, 0, 100);
            UpdateUI();
        }
    }

    public void EndDay()
    {
        currentDay++;
        actionPoints = 8; 
        approval -= dailyApprovalDecay; 
        
        if (currentDay > 30)
        {
            dayText.text = "You Survived 30 Days!";
            LockAllButtons();
            return;
        }

        policyIndex++;
        if (policyIndex < dailyPolicies.Length)
        {
            LoadNextPolicy();
        }
        else 
        {
            policyDescText.text = "No more policies in the deck.";
        }

        UpdateUI();
        CheckLoseCondition();
    }

    private void CheckLoseCondition()
    {
        if (approval <= 0)
        {
            approvalText.text = "Approval: 0% - RECALLED!";
            LockAllButtons();
        }
    }

    private void LockAllButtons()
    {
        if (inspectButton != null) inspectButton.interactable = false;
        if (townHallButton != null) townHallButton.interactable = false;
        if (mediaAdsButton != null) mediaAdsButton.interactable = false;
    }

    private void UpdateUI()
    {
        moneyText.text = "$" + money.ToString("N0");
        approvalText.text = "Approval: " + approval.ToString() + "%";
        apText.text = "AP: " + actionPoints.ToString();
        dayText.text = "Day: " + currentDay.ToString() + " / 30";

        if (townHallButton != null) townHallButton.interactable = (actionPoints >= 3);
        if (mediaAdsButton != null) mediaAdsButton.interactable = (money >= 15000);
    }
}