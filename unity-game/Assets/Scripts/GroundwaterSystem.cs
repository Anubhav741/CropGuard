
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GroundwaterSystem : MonoBehaviour
{
    public float groundwaterLevel = 70f;
    public float maxGroundwater = 100f;

    public float targetGroundwater = 80f;
    public int totalActions = 5;

    public TMP_Text groundwaterValue;
    public TMP_Text groundwaterMessage;

    public TMP_Text challengeInstructions;
    public TMP_Text scoreValue;
    public TMP_Text actionsValue;
    public TMP_Text resultMessage;

    public Button rainfallButton;
    public Button extractWaterButton;
    public Button rechargeButton;

    private float startingGroundwater;
    private int actionsUsed = 0;
    private int score = 0;
    private bool gameFinished = false;

    void Start()
    {
        startingGroundwater = groundwaterLevel;

        if (resultMessage != null)
            resultMessage.text = "";

        SetButtons(true);
        UpdateUI();
    }

    public void AddRainfall()
    {
        if (gameFinished || actionsUsed >= totalActions)
            return;

        groundwaterLevel += 8f;
        score += 80;

        CompleteAction("Rainfall added!");
    }

    public void ExtractWater()
    {
        if (gameFinished || actionsUsed >= totalActions)
            return;

        groundwaterLevel -= 5f;
        score = Mathf.Max(0, score - 50);

        CompleteAction("Water extracted!");
    }

    public void ArtificialRecharge()
    {
        if (gameFinished || actionsUsed >= totalActions)
            return;

        groundwaterLevel += 12f;
        score += 120;

        CompleteAction("Artificial recharge done!");
    }

    void CompleteAction(string message)
    {
        groundwaterLevel = Mathf.Clamp(
            groundwaterLevel, 0f, maxGroundwater
        );

        actionsUsed++;

        if (groundwaterMessage != null)
            groundwaterMessage.text = message;

        if (actionsUsed >= totalActions)
            FinishGame();

        UpdateUI();
    }

    void FinishGame()
    {
        gameFinished = true;

        if (resultMessage != null)
        {
            if (groundwaterLevel >= targetGroundwater)
            {
                resultMessage.text =
                    "YOU WIN! Village groundwater saved!";
            }
            else
            {
                resultMessage.text =
                    "TRY AGAIN! Groundwater target not achieved.";
            }
        }

        SetButtons(false);
    }

    void SetButtons(bool interactable)
    {
        if (rainfallButton != null)
            rainfallButton.interactable = interactable;

        if (extractWaterButton != null)
            extractWaterButton.interactable = interactable;

        if (rechargeButton != null)
            rechargeButton.interactable = interactable;
    }

    void UpdateUI()
    {
        if (groundwaterValue != null)
        {
            groundwaterValue.text =
                groundwaterLevel + " / " + maxGroundwater;
        }
        else
        {
            Debug.LogError("Missing reference: GroundwaterValue");
        }

        if (actionsValue != null)
        {
            actionsValue.text =
                "Actions: " + actionsUsed + " / " + totalActions;
        }
        else
        {
            Debug.LogError("Missing reference: ActionsValue");
        }

        if (scoreValue != null)
        {
            scoreValue.text = "Score: " + score;
        }
        else
        {
            Debug.LogError("Missing reference: ScoreValue");
        }

        if (challengeInstructions != null)
        {
            challengeInstructions.text =
                "VILLAGE CHALLENGE | TARGET: " +
                targetGroundwater + " | " +
                totalActions + " ACTIONS";
        }
        else
        {
            Debug.LogError("Missing reference: ChallengeInstructions");
        }

        if (groundwaterMessage != null)
        {
            if (groundwaterLevel >= 60)
                groundwaterMessage.text =
                    "Groundwater is at a healthy level";
            else if (groundwaterLevel >= 30)
                groundwaterMessage.text =
                    "Warning: Groundwater is decreasing!";
            else
                groundwaterMessage.text =
                    "Critical: Save groundwater!";
        }
        else
        {
            Debug.LogError("Missing reference: GroundwaterMessage");
        }
    }
}
