using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float defaultSpeed = 20f;
    public float health = 100f;

    private HPBar hp;
    private float currentSpeed;
    private Coroutine speedBoostCoroutine;

    private PlayerMovement movement;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        if (movement == null)
            Debug.LogWarning("no playermovement found on player");

        // init speed
        currentSpeed = defaultSpeed;
        hp = Object.FindObjectOfType<HPBar>(); 
    }

    public void ApplySpeedBoost(float amount, float duration)
    {
        // if a previous boost is still running stop it and revert speed
        if (speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
            currentSpeed = defaultSpeed;
            if (movement != null)
                movement.moveSpeed = amount;
        }

        speedBoostCoroutine = StartCoroutine(SpeedBoostRoutine(amount, duration));
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        hp.TakeDamage(damage);
        if (health == 0) health = 100;
    }

    public void Heal(float healAmount)
    {
        health += healAmount;
        if (health > 100f) health = 100f;
        hp.Heal(healAmount);
    }

    private IEnumerator SpeedBoostRoutine(float amount, float duration)
    {
        // increase speed
        currentSpeed += amount;
        if (movement != null)
            movement.moveSpeed = currentSpeed;

        // wait
        yield return new WaitForSeconds(duration);

        // return back to default speed
        currentSpeed = defaultSpeed;
        if (movement != null)
            movement.moveSpeed = currentSpeed;

        speedBoostCoroutine = null;
    }
}
