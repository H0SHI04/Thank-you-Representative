using UnityEngine;
using TMPro;
using UnityEngine.UI; 
using System.Collections; 
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    public TextMeshProUGUI notepadText;
    
    [Header("Policy UI")]
    public TextMeshProUGUI policyTitleText;
    public TextMeshProUGUI policyDescText;

    [Header("Lobbyist UI")]
    public GameObject lobbyistPanel;
    public TextMeshProUGUI offerText;

    [Header("Desk Clutter System")]
    public GameObject[] clutterPrefabs; 
    public RectTransform deskArea; 
    public RectTransform trashCan; 

    [Header("Tutorial & Fading")]
    public GameObject tutorialPanel; 
    public CanvasGroup fadeOverlay;
    public float fadeDuration = 1.0f;

    [Header("Core Stats")]
    private int money = 40000;
    private int approval = 50;
    private int actionPoints = 8;
    private int currentDay = 1;
    private int dailyApprovalDecay = 5; 
    private int dailyMoneyDecay = 2500;

    [Header("Policy Roster")]
    public Policy[] dailyPolicies = new Policy[30]; 
    private int policyIndex = 0;
    
    private bool isInspected = false;
    private bool policyDealtWith = false; 

    [Header("Audio (Juice)")]
    public AudioSource audioSource;
    public AudioClip stampThudSound;
    public AudioClip paperRustleSound;
    public AudioClip cashRegisterSound;
    public AudioClip errorBuzzerSound;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;

    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void Start()
    {
        Generate30DayDeck();
        lobbyistPanel.SetActive(false); 
        
        // Show tutorial if it's day 1, otherwise hide it
        if(gameOverPanel != null) gameOverPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(currentDay == 1);

        // Begin the day with a fade-in
        StartCoroutine(FadeInDay());
    }

    
    public void CloseTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        PlaySound(paperRustleSound);
    }

    
    private IEnumerator FadeInDay()
    {
        LoadNextPolicy(); // Sets up the desk while the screen is black
        UpdateUI();

        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true; 
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                fadeOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false; 
        }
    }

    private IEnumerator FadeOutToNextDay()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true; 
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                fadeOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        // Apply End of Day Math while screen is black
        currentDay++;
        actionPoints = 8; 
        approval -= dailyApprovalDecay; 
        money -= dailyMoneyDecay;
        
        if (currentDay > 30)
        {
            dayText.text = "You Survived 30 Days!";
            if (fadeOverlay != null) fadeOverlay.alpha = 0f; 
            yield break; 
        }

        policyIndex++;
        
        // Check if daily decay caused a game over
        if (approval <= 0) 
        {
            CheckLoseCondition();
            yield break;
        }

        // Fade back in for the new morning
        StartCoroutine(FadeInDay());
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

    private void SpawnHateMail()
    {
        if (clutterPrefabs.Length == 0) return;

        GameObject randomClutter = clutterPrefabs[Random.Range(0, clutterPrefabs.Length)];
        GameObject spawnedClutter = Instantiate(randomClutter, deskArea);
        
        RectTransform rt = spawnedClutter.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(Random.Range(-200f, 200f), Random.Range(-150f, 150f));

        DraggableClutter clutterScript = spawnedClutter.GetComponent<DraggableClutter>();
        if (clutterScript != null)
        {
            clutterScript.trashCan = this.trashCan;
            clutterScript.gameManager = this;
        }
    }

    private void LoadNextPolicy()
    {
        if (policyIndex < dailyPolicies.Length)
        {
            isInspected = false;
            policyDealtWith = false;

            PlaySound(paperRustleSound); 

            if (notepadText != null) notepadText.text = "Awaiting campaign actions...";
            
            Policy p = dailyPolicies[policyIndex];
            policyTitleText.text = p.policyName;
            
            policyDescText.text = "Contents unknown.\n\nHighlight (" + p.apCost + " AP) to read the bill.\nStamp blindly (0 AP) for a gamble.";

            int clutterAmount = 0;
            if (approval < 40) clutterAmount = 2; 
            if (approval < 20) clutterAmount = 5; 

            for (int i = 0; i < clutterAmount; i++)
            {
                SpawnHateMail();
            }
        }
    }

    public void InspectPolicy()
    {
        if (!isInspected && !policyDealtWith && actionPoints >= 4)
        {
            actionPoints -= 4;
            isInspected = true;
            
            Policy p = dailyPolicies[policyIndex];

            PlaySound(paperRustleSound); 
            StartCoroutine(ScreenShake(0.1f, 5f));
           
            if (p.hasLobbyistOffer)
            {
                PlaySound(errorBuzzerSound); 
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
        else if (!isInspected && actionPoints < 4)
        {
            PlaySound(errorBuzzerSound); 
        }
    }
    
    public void AcceptRider()
    {
        Policy p = dailyPolicies[policyIndex];
        
        p.moneyChange += p.riderMoneyBonus;
        p.approvalChange += p.riderApprovalPenalty;
        p.description += "\n\n<color=red>[CORPORATE RIDER ATTACHED]</color>";
        
        PlaySound(cashRegisterSound); 
        StartCoroutine(FlashText(moneyText, Color.green)); 
        
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

        if (!isInspected && p.hasLobbyistOffer)
        {
            PlaySound(stampThudSound);
            PlaySound(errorBuzzerSound); 
            StartCoroutine(ScreenShake(0.2f, 25f));
            
            money = Mathf.Max(0, money + p.moneyChange + p.riderMoneyBonus);
            approval = Mathf.Clamp(approval + p.approvalChange + p.riderApprovalPenalty, 0, 100);
            
            StartCoroutine(FlashText(moneyText, Color.green));
            StartCoroutine(FlashText(approvalText, Color.red));

            LockCardAfterAction("You blindly approved the bill.\n\n<color=red>A hidden corporate rider was attached!</color>\nEffect: " + 
                                (p.approvalChange + p.riderApprovalPenalty) + "% Approval, $" + (p.moneyChange + p.riderMoneyBonus));
        }
        else
        {
            if (money + p.moneyChange < 0)
            {
                PlaySound(errorBuzzerSound); 
                policyDescText.text = p.description + "\n\n<color=red>Insufficient funds to execute this policy. You must Veto, or find outside funding.</color>";
                return; 
            }

            PlaySound(stampThudSound);
            StartCoroutine(ScreenShake(0.2f, 25f));

            money += p.moneyChange;
            approval = Mathf.Clamp(approval + p.approvalChange, 0, 100); 
            
            if (p.moneyChange > 0) StartCoroutine(FlashText(moneyText, Color.green));
            else if (p.moneyChange < 0) StartCoroutine(FlashText(moneyText, Color.red));

            if (p.approvalChange > 0) StartCoroutine(FlashText(approvalText, Color.green));
            else if (p.approvalChange < 0) StartCoroutine(FlashText(approvalText, Color.red));

            LockCardAfterAction("Policy Approved.\nEffect: " + p.approvalChange + "% Approval, $" + p.moneyChange);
        }
    }

    public void VetoPolicy()
    {
        if (policyDealtWith) return;

        PlaySound(stampThudSound);
        StartCoroutine(ScreenShake(0.2f, 15f));

        LockCardAfterAction("Policy Vetoed. Status quo maintained.");
    }

    private void LockCardAfterAction(string resultText)
    {
        policyDealtWith = true;
        policyDescText.text = resultText;
        
        UpdateUI();
        CheckLoseCondition();
    }

    public IEnumerator FlashText(TextMeshProUGUI textElement, Color flashColor)
    {
        Color originalColor = Color.white; 
        textElement.color = flashColor;
        float elapsed = 0.0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            textElement.color = Color.Lerp(flashColor, originalColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        textElement.color = originalColor;
    }

    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        Transform canvasTransform = FindObjectOfType<Canvas>().transform;
        float elapsed = 0.0f;
        Vector3 currentOffset = Vector3.zero;

        while (elapsed < duration)
        {
            foreach (Transform child in canvasTransform) { child.localPosition -= currentOffset; }

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            currentOffset = new Vector3(x, y, 0);

            foreach (Transform child in canvasTransform) { child.localPosition += currentOffset; }

            elapsed += Time.deltaTime;
            yield return null; 
        }

        foreach (Transform child in canvasTransform) { child.localPosition -= currentOffset; }
    }

    public void CallGrassroots()
    {
        if (actionPoints >= 3)
        {
            PlaySound(paperRustleSound); 
            actionPoints -= 3;
            approval = Mathf.Clamp(approval + 5, 0, 100);
            
            StartCoroutine(FlashText(approvalText, Color.green)); 
            
            notepadText.text = "<b>Action:</b> Town Hall\nVoters appreciated the transparency.\n<color=green>+5% Approval</color>";
            UpdateUI();
        }
        else
        {
            PlaySound(errorBuzzerSound);
            notepadText.text = "<color=red>Not enough AP to organize a Town Hall. (Requires 3)</color>";
        }
    }

    public void CallPRFirm()
    {
        if (money >= 30000)
        {
            PlaySound(cashRegisterSound);
            money -= 30000;
            approval = Mathf.Clamp(approval + 15, 0, 100);
            
            StartCoroutine(FlashText(approvalText, Color.green));
            StartCoroutine(FlashText(moneyText, Color.red));

            notepadText.text = "<b>Action:</b> Ad Blitz\nPublic perception successfully manipulated.\n<color=green>+15% Approval</color>";
            UpdateUI();
        }
        else
        {
            PlaySound(errorBuzzerSound);
            notepadText.text = "<color=red>Insufficient funds to hire the PR Firm. (Requires $15k)</color>";
        }
    }

    public void CallCorporateFixer()
    {
        if (actionPoints >= 4)
        {
            PlaySound(cashRegisterSound);
            actionPoints -= 4;
            money += 10000;
            approval = Mathf.Clamp(approval - 10, 0, 100);
            
            StartCoroutine(FlashText(moneyText, Color.green));
            StartCoroutine(FlashText(approvalText, Color.red));

            notepadText.text = "<b>Action:</b> Fixer Meeting\nSecured dark money <color=green>(+$10,000)</color>.\nThe leaked meeting angered constituents <color=red>(-10% Approval)</color>.";
            
            StartCoroutine(ScreenShake(0.1f, 10f)); 
            UpdateUI();
            CheckLoseCondition();
        }
        else
        {
            PlaySound(errorBuzzerSound);
            notepadText.text = "<color=red>Not enough AP to meet with the Fixer. (Requires 2)</color>";
        }
    }

    public void EndDay()
    {
        PlaySound(paperRustleSound); 
        StartCoroutine(FadeOutToNextDay()); // Triggers the screen fade before moving to the next day
    }

    private void CheckLoseCondition()
    {
        if (approval <= 0)
        {
            PlaySound(errorBuzzerSound); 
            approvalText.text = "Approval: 0% - RECALLED!";
            if (fadeOverlay != null) fadeOverlay.alpha = 0f; 

            if(gameOverPanel != null)
            {
                gameOverText.text = "Approval is at 0%.\n\nYou have been voted out by your constituents";
                gameOverPanel.SetActive(true);
            }
        }
    }

    public void RestartGame()
    {
        PlaySound(paperRustleSound);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateUI()
    {
        moneyText.text = "$" + money.ToString("N0");
        approvalText.text = "Approval: " + approval.ToString() + "%";
        apText.text = "AP: " + actionPoints.ToString();
        dayText.text = "Day: " + currentDay.ToString() + " / 30";
    }
}