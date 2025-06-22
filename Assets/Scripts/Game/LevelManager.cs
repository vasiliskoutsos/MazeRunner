using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public TMP_Text counter;
    public string mazeScene = "SampleScene";
    public static int solvedCount = 1;
    
    private Transform panel;
    private bool isLoadingNext = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);  
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
            panel = canvas.transform.Find("NextLevel");
        else
            Debug.LogWarning("Not found in the scene");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void MazeCompleted()
    {
        if (panel != null)
        {
            var btn = panel.GetComponentInChildren<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(MoveNextLevel);
            panel.gameObject.SetActive(true);
        }
        else
            Debug.Log("Could not find nextLevel");
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoadingNext = false;
        counter = FindObjectOfType<TextMeshProUGUI>();
        if (counter != null)
            counter.text = $"{solvedCount}";
        
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
            panel = canvas.transform.Find("NextLevel");

        if (panel != null)
            panel.gameObject.SetActive(false);

        if (solvedCount > 1)
            changeDifficulty();
    }

    public void MoveNextLevel()
    {
        if (isLoadingNext) return;
        isLoadingNext = true;

        solvedCount++;
        Time.timeScale   = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (panel != null)
        {
            var btn = panel.GetComponentInChildren<Button>();
            btn.interactable = false;
        }

        SceneManager.LoadScene(mazeScene);
    }

    private void changeDifficulty()
    {
        Timer timer = FindObjectOfType<Timer>();
        if (timer == null)
        {
            Debug.Log("No timer");
            return;
        }

        float baseTime = 600f;
        float reductionPerLevel = 60f;
        float minTime = 200f;
        float newTime = Mathf.Max(minTime, baseTime - reductionPerLevel * (solvedCount - 1));
        timer.startTimeSeconds = newTime;
        timer.ResetTimer();
    }
}