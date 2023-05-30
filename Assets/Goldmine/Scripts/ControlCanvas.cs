using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEPL;

public class ControlCanvas : MonoBehaviour
{
    public Text centralDisplay; // large text in the middle of the screen
    public Text centralDisplay2; // large text (not bolded) in the middle of the screen
    public Text topDisplay; // large text in the center above the central display
    public Text bottomDisplay; // large text in the center below the central display
    public Text scoreDisplay; // text that shows the player's score
    public Text taskDirectionsDisplay; // text that tells the player what they should be doing
    public Text timedTrialDisplay; // text that says if the current trial is timed or untimed
    public int scoreDefaultFontSize = 30;
    public int scoreBigFontSize = 50;
    public GameObject rightArrow;
    public GameObject leftArrow;
    public GameObject background;
    protected InterfaceManager manager;

    private string lastScoreText;

    void Awake()
    {
        manager = InterfaceManager.Instance;
    }

    public void SetCentralDisplay(string msg, string color, float duration)
    {
        Color color_ = new Color(0f, 0f, 0f);

        switch (color)
        {
            case "default":
                color_ = new Color(1f, 0.925f, 0.231f); // gold
                break;
            case "positive":
                color_ = new Color(0.24f, 1f, 0.24f); // green
                break;
            case "negative":
                color_ = new Color(1f, 0.24f, 0.24f); // red
                break;
        }

        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "CentralDisplay" }, { "textDisplayed", msg } });
        centralDisplay.text = msg;
        centralDisplay.color = color_;
        Invoke("ResetCentralDisplay", duration);
    }

    public void ResetCentralDisplay()
    {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "CentralDisplay" }, { "textDisplayed", "" } });
        centralDisplay.text = "";
    }

    public void SetCentralDisplay2(string msg, string color, float duration)
    {
        Color color_ = new Color(0f, 0f, 0f);

        switch (color)
        {
            case "default":
                color_ = new Color(1f, 0.925f, 0.231f); // gold
                break;
            case "positive":
                color_ = new Color(0.24f, 1f, 0.24f); // green
                break;
            case "negative":
                color_ = new Color(1f, 0.24f, 0.24f); // red
                break;
        }

        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "CentralDisplay2" }, { "textDisplayed", msg } });
        centralDisplay2.text = msg;
        centralDisplay2.color = color_;
        Invoke("ResetCentralDisplay2", duration);
    }

    public void ResetCentralDisplay2()
    {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "CentralDisplay" }, { "textDisplayed", "" } });
        centralDisplay2.text = "";
    }

    public void SetTopDisplay(string msg, string color, float duration)
    {
        Color color_ = new Color(0f, 0f, 0f);

        switch (color)
        {
            case "default":
                color_ = new Color(1f, 0.925f, 0.231f); // gold
                break;
            case "positive":
                color_ = new Color(0.24f, 1f, 0.24f); // green
                break;
            case "negative":
                color_ = new Color(1f, 0.24f, 0.24f); // red
                break;
        }

        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "TopDisplay" }, { "textDisplayed", msg } });
        topDisplay.text = msg;
        topDisplay.color = color_;
        Invoke("ResetTopDisplay", duration);
    }

    public void ResetTopDisplay()
    {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "TopDisplay" }, { "textDisplayed", "" } });
        topDisplay.text = "";
    }

    public void SetBottomDisplay(string msg, string color, float duration)
    {
        Color color_ = new Color(0f, 0f, 0f);

        switch (color)
        {
            case "default":
                color_ = new Color(1f, 0.925f, 0.231f); // gold
                break;
            case "positive":
                color_ = new Color(0.24f, 1f, 0.24f); // green
                break;
            case "negative":
                color_ = new Color(1f, 0.24f, 0.24f); // red
                break;
        }

        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "BottomDisplay" }, { "textDisplayed", msg } });
        bottomDisplay.text = msg;
        bottomDisplay.color = color_;
        Invoke("ResetBottomDisplay", duration);
    }

    public void ResetBottomDisplay()
    {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "BottomDisplay" }, { "textDisplayed", "" } });
        bottomDisplay.text = "";
    }

    public void SetScoreDisplay(string msg, string color, float duration, bool bigFont=true)
    {
        Color color_ = new Color(0f, 0f, 0f);

        switch (color)
        {
            case "default":
                color_ = new Color(1f, 0.925f, 0.231f); // gold
                break;
            case "positive":
                color_ = new Color(0.24f, 1f, 0.24f); // green
                break;
            case "negative":
                color_ = new Color(1f, 0.24f, 0.24f); // red
                break;
        }

        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "Score" }, { "textDisplayed", msg } });
        scoreDisplay.text = "SCORE: " + msg;
        if (bigFont)
        {
            scoreDisplay.fontSize = scoreBigFontSize;
        }
        else
        {
            scoreDisplay.fontSize = scoreDefaultFontSize;
        }
        scoreDisplay.color = color_;

        Invoke("ResetScoreDisplay", duration);
    }

    public void ResetScoreDisplay()
    {
        scoreDisplay.fontSize = scoreDefaultFontSize;
        scoreDisplay.color = new Color(1f, 0.925f, 0.231f); // gold
    }

    public void SetTaskDirectionsDisplay(string msg, string color="default")
    {
        Color color_ = new Color(0f, 0f, 0f);

        switch (color)
        {
            case "default":
                color_ = new Color(1f, 0.925f, 0.231f); // gold
                break;
            case "positive":
                color_ = new Color(0.24f, 1f, 0.24f); // green
                break;
            case "negative":
                color_ = new Color(1f, 0.24f, 0.24f); // red
                break;
        }

        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "TaskDirections" }, { "textDisplayed", msg } });
        taskDirectionsDisplay.text = msg;
        taskDirectionsDisplay.color = color_;
    }

    public void SetTimedTrialDisplay(string msg, string color="default")
    {
        Color color_ = new Color(0f, 0f, 0f);

        switch (color)
        {
            case "default":
                color_ = new Color(1f, 0.925f, 0.231f); // gold
                break;
            case "positive":
                color_ = new Color(0.24f, 1f, 0.24f); // green
                break;
            case "negative":
                color_ = new Color(1f, 0.24f, 0.24f); // red
                break;
        }

        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "TimedTrial" }, { "textDisplayed", msg } });
        timedTrialDisplay.text = msg;
        timedTrialDisplay.color = color_;
    }

    public void ShowRightArrow(float duration)
    {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "RightArrow" }, { "textDisplayed", "Active" } });
        rightArrow.SetActive(true);
        Invoke("ResetRightArrow", duration);
    }

    void ResetRightArrow()
    {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "RightArrow" }, { "textDisplayed", "Inactive" } });
        rightArrow.SetActive(false);
    }

    public void ShowLeftArrow(float duration)
    {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "LeftArrow" }, { "textDisplayed", "Active" } });
        leftArrow.SetActive(true);
        Invoke("ResetLeftArrow", duration);
    }

    void ResetLeftArrow()
    {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "LeftArrow" }, { "textDisplayed", "Inactive" } });
        leftArrow.SetActive(false);
    }

    public void ShowBackground(float duration)
    {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "Background" }, { "textDisplayed", "Active" } });
        background.SetActive(true);
        Invoke("ResetBackground", duration);
    }

    void ResetBackground()
    {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "MainCanvas" }, { "canvasChildObjectName", "Background" }, { "textDisplayed", "Inactive" } });
        background.SetActive(false);
    }
}
