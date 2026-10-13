using UnityEngine;
using TMPro;
using UnityEngine.UI; 

[System.Serializable]
public class Policy
{
    public string policyName;
    [TextArea] public string description;
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
    public Button approveButton;
    public Button inspectButton;
    public Button vetoButton;

    [Header("Campaign Buttons")]
    public Button townHallButton;
    public Button mediaAdsButton;

    [Header("Lobbyist UI")]
    public GameObject lobbyistPanel;
    public TextMeshProUGUI offerText;

    [Header("Core Stats")]
    private int money = 50000;
    private int approval = 50;
    private int actionPoints = 8;
    private int currentDay = 1;
    private int dailyApprovalDecay = 5; 

    [Header("Policy Roster")]
    public Policy[] dailyPolicies; 
    private int policyIndex = 0;
    
    private bool isInspected = false;
    private bool policyDealtWith = false; 

    void Start()
    {
        lobbyistPanel.SetActive(false); // Make sure the pop-up starts hidden
        LoadNextPolicy();
        UpdateUI();
    }

    private void LoadNextPolicy()
    {
        if (policyIndex < dailyPolicies.Length)
        {
            isInspected = false;
            policyDealtWith = false;
            
            approveButton.interactable = true;
            inspectButton.interactable = true;
            vetoButton.interactable = true;
            
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
            inspectButton.interactable = false; 
            
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
        
        // Modify the current bill's stats with the dirty money and bad approval
        p.moneyChange += p.riderMoneyBonus;
        p.approvalChange += p.riderApprovalPenalty;
        
        // Add a permanent mark to the description so the player sees what they did
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
        money += p.moneyChange;
        approval = Mathf.Clamp(approval + p.approvalChange, 0, 100); 
        
        LockCardAfterAction("Policy Approved.");
    }

    public void VetoPolicy()
    {
        if (policyDealtWith) return;
        LockCardAfterAction("Policy Vetoed. Status quo maintained.");
    }

    private void LockCardAfterAction(string resultText)
    {
        policyDealtWith = true;
        approveButton.interactable = false;
        inspectButton.interactable = false;
        vetoButton.interactable = false;
        policyDescText.text = resultText;
        
        UpdateUI();
        CheckLoseCondition();
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
            approveButton.interactable = false;
            inspectButton.interactable = false;
            vetoButton.interactable = false;
            if (townHallButton != null) townHallButton.interactable = false;
            if (mediaAdsButton != null) mediaAdsButton.interactable = false;
        }
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