using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Image hp;
    public float maxHealth = 100f;

    private float currentHealth;
    private int currImage = 0;

    public GameObject lifesPanel;
    public GameObject gameOverPanel;
    [HideInInspector]
    public bool isGameOver;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        isGameOver = false;
        ResetHealth();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        hp.fillAmount = 1f;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateFill();

        if (currentHealth == 0f)
        {
            currentHealth = maxHealth;
            ResetHealth();

            if (currImage == 3)
            {
                isGameOver = true;
                LevelManager.solvedCount = 1;
                currentHealth = 0f;
                gameOverPanel.SetActive(true);
                Time.timeScale = 0f;
                AudioListener.pause = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            // disable the next heart and continue
            Image[] hearts = lifesPanel.GetComponentsInChildren<Image>();
            hearts[currImage].enabled = false;
            currImage++;
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateFill();
    }

    private void UpdateFill()
    {
        hp.fillAmount = currentHealth / maxHealth;
    }
}