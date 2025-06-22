using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text volumeValue = null;
    [SerializeField]
    private Slider volumeSlider = null;
    [SerializeField]
    private float defaultVolume = 0.5f;
    [SerializeField]
    private Toggle fScreen;

    private bool fullScreen;

    [SerializeField]
    private GameObject pauseMenuPanel;

    private bool isPaused = false;

    private void Awake()
    {
        bool fs = PlayerPrefs.GetInt("masterFullscreen", 0) == 1;
        fullScreen = fs;
        fScreen.isOn = fs;
        Screen.fullScreen = fs;
    }

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("masterVolume", defaultVolume);
        AudioListener.volume = savedVolume;
        volumeSlider.value = savedVolume;
        volumeValue.text = savedVolume.ToString("0.0");
        pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isPaused = true;
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;
        pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
        SceneManager.LoadScene(0);
    }

    public void QuitGameButton()
    {
        Application.Quit();
    }

    public void StartGameButton()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(1);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        volumeValue.text = volume.ToString("0.0");
    }

    public void VolumeApply()
    {
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
    }

    public void ResetButton(string menuType)
    {
        if (menuType == "Audio")
        {
            AudioListener.volume = defaultVolume;
            volumeSlider.value = defaultVolume;
            volumeValue.text = defaultVolume.ToString("0.0");
            VolumeApply();
        }

        if (menuType == "Graphics")
        {
            fScreen.isOn = false;
            Screen.fullScreen = false;
        }
    }

    public void SetScreen(bool Screen)
    {
        fullScreen = Screen;
        ScreenApply();
    }

    public void ScreenApply()
    {
        PlayerPrefs.SetInt("masterFullscreen", fullScreen ? 1 : 0);
        Screen.fullScreen = fullScreen;
    }
}