using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float startTimeSeconds = 600f;
    public TMP_Text timerText;
    public GameObject gameOverPanel;

    private float remainingTime;
    private bool isCounting = true;

    void Start()
    {
        remainingTime = startTimeSeconds;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateTimerDisplay(remainingTime);
    }

    void Update()
    {
        if (!isCounting)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isCounting = false;
            OnTimerFinished();
        }

        UpdateTimerDisplay(remainingTime);
    }

    private void UpdateTimerDisplay(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60f);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60f);

        // shows mins and second
        string timeText = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.text = timeText;
    }

    private void OnTimerFinished()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // pause game
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // reset timer
    public void ResetTimer()
    {
        remainingTime = startTimeSeconds;
        isCounting = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateTimerDisplay(remainingTime);
        Time.timeScale = 1f;
    }
}
