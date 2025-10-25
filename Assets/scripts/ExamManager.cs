using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class Question
{
    public string id;
    public string stem;
    public List<string> choices;  // Exactly 4 choices per your server schema
    public int correctIndex;
    public string explanation;
}

[Serializable]
public class QuizData
{
    public string topic;
    public string difficulty;
    public List<Question> questions;
}

public class ExamManager : MonoBehaviour
{
    // UI References (assign in Inspector)
    public TextMeshProUGUI questionText;  // For question stem
    public TextMeshProUGUI explanationText;  // For explanation
    public Button[] optionButtons;  // Array of exactly 4 buttons
    public Button nextButton;
    public Button submitButton;

    // Exam parameters (set via Inspector or UI inputs)
    public string topic = "Computer Science Fundamentals";
    public string difficulty = "medium";  // easy, medium, hard
    public int questionCount = 5;
    public string language = "English";  // Or "Arabic"

    private List<Question> questions;
    private int currentQuestionIndex = 0;
    private string selectedAnswer;  // Track selected choice
    private int score = 0;  // Track correct answers

    // Server URL
    private string serverUrl = "http://localhost:3001/generate-quiz";

    void Start()
    {
        // Validate UI assignments
        if (questionText == null || explanationText == null || nextButton == null || submitButton == null)
        {
            Debug.LogError("One or more UI elements are not assigned in the Inspector.");
            return;
        }
        if (optionButtons.Length != 4)
        {
            Debug.LogError("Exactly 4 option buttons must be assigned in the Inspector.");
            return;
        }

        FetchQuestions();
        nextButton.onClick.AddListener(NextQuestion);
        submitButton.onClick.AddListener(SubmitExam);
    }

    private void FetchQuestions()
    {
        // Create JSON payload
        var payload = new
        {
            topic,
            difficulty,
            count = questionCount,
            language
        };
        string jsonPayload = JsonUtility.ToJson(payload);
        StartCoroutine(PostQuestionsToServer(jsonPayload));
    }

    private IEnumerator PostQuestionsToServer(string jsonPayload)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, jsonPayload, "application/json"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching questions: {www.error}");
                if (explanationText != null)
                    explanationText.text = "Failed to load questions. Check server.";
                yield break;  // Exit coroutine on error
            }

            string jsonResponse = www.downloadHandler.text;
            QuizData quizData = JsonUtility.FromJson<QuizData>(jsonResponse);
            if (quizData == null || quizData.questions == null || quizData.questions.Count == 0)
            {
                Debug.LogError("No questions received from server. Response: " + jsonResponse);
                if (explanationText != null)
                    explanationText.text = "No questions available.";
                yield break;
            }

            questions = quizData.questions;
            DisplayQuestion(0);  // Display first question
        }
    }

    private void DisplayQuestion(int index)
    {
        if (index >= questions.Count)
        {
            SubmitExam();  // Call SubmitExam directly, not as a return
            return;
        }

        Question q = questions[index];
        if (questionText != null)
            questionText.text = $"Q{index + 1}: {q.stem}";
        if (explanationText != null)
            explanationText.text = "";  // Clear explanation until answered

        // Display exactly 4 choices
        for (int i = 0; i < 4; i++)
        {
            if (optionButtons[i] == null || optionButtons[i].GetComponentInChildren<TextMeshProUGUI>() == null)
            {
                Debug.LogError($"Option button {i} or its TextMeshProUGUI is not assigned.");
                continue;
            }

            TextMeshProUGUI btnText = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = q.choices[i];

            // Remove old listeners
            optionButtons[i].onClick.RemoveAllListeners();

            // Add new listener
            int choiceIndex = i;
            optionButtons[i].onClick.AddListener(() => SelectOption(q.choices[choiceIndex], choiceIndex, q.correctIndex, q.explanation));
            optionButtons[i].interactable = true;  // Re-enable buttons
        }

        selectedAnswer = null;

        // Arabic support
        if (language == "Arabic" && questionText != null && explanationText != null)
        {
            questionText.alignment = TextAlignmentOptions.Right;
            explanationText.alignment = TextAlignmentOptions.Right;
            foreach (var btn in optionButtons)
                if (btn != null)
                    btn.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        }
    }

    private void SelectOption(string option, int selectedIndex, int correctIndex, string explanation)
    {
        selectedAnswer = option;
        if (selectedIndex == correctIndex)
        {
            score++;
            if (explanationText != null)
                explanationText.text = $"Correct! {explanation}";
        }
        else
        {
            if (explanationText != null)
                explanationText.text = $"Incorrect. {explanation}";
        }

        // Disable buttons until "Next" is clicked
        foreach (var btn in optionButtons)
            if (btn != null)
                btn.interactable = false;
    }

    private void NextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Count)
        {
            DisplayQuestion(currentQuestionIndex);
        }
        else
        {
            SubmitExam();
        }
    }

    private void SubmitExam()
    {
        if (questionText != null)
            questionText.text = "Exam Complete!";
        if (explanationText != null)
            explanationText.text = $"Your score: {score}/{questions.Count}";
        foreach (var btn in optionButtons)
            if (btn != null)
                btn.gameObject.SetActive(false);
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
        if (submitButton != null)
            submitButton.gameObject.SetActive(false);
    }
}