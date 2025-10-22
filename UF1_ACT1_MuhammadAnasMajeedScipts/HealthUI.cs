using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Por si usas TextMeshPro

public class HealthUI : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerHealth player;

    [Header("Text (elige uno)")]
    public Text healthText;
    public TextMeshProUGUI healthTextTMP;

    [Header("Opcional - barra")]
    public Slider healthSlider;

    [Header("Botones")]
    public Button damageButton;
    public Button healButton;
    public Button killButton;
    public Button reviveButton;

    private int lastHealth = -1;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("HealthUI: player NO asignado. Arrastra el GameObject con PlayerHealth al campo 'player' en el inspector.", this);
            enabled = false;
            return;
        }

        player.OnHealthChanged += OnHealthChanged;
        player.OnDeath += OnDeath;
        player.OnRevive += OnRevive;

        RefreshAll(player.GetHealth());
    }

    void OnDestroy()
    {
        if (player != null)
        {
            player.OnHealthChanged -= OnHealthChanged;
            player.OnDeath -= OnDeath;
            player.OnRevive -= OnRevive;
        }
    }

    private void OnHealthChanged(int newHealth)
    {
        if (newHealth == lastHealth) return;
        RefreshAll(newHealth);
    }

    private void OnDeath()
    {
        RefreshButtons();
    }

    private void OnRevive()
    {
        RefreshButtons();
    }

    private void RefreshAll(int health)
    {
        lastHealth = health;

        if (healthTextTMP != null)
            healthTextTMP.text = $"HP: {health} / {player.GetMaxHealth()}";
        else if (healthText != null)
            healthText.text = $"HP: {health} / {player.GetMaxHealth()}";

        if (healthSlider != null)
        {
            healthSlider.maxValue = player.GetMaxHealth();
            healthSlider.value = health;
        }

        RefreshButtons();
    }

    private void RefreshButtons()
    {
        int health = player.GetHealth();
        int max = player.GetMaxHealth();
        bool isAlive = player.IsAlive();

        // Damage: solo si está vivo y tiene >0 HP
        if (damageButton != null) damageButton.interactable = isAlive && health > 0;

        // Heal: solo si está vivo y NO está al máximo (esto evita curar en 100%)
        bool canHeal = isAlive && health < max;
        if (healButton != null) healButton.interactable = canHeal;

        // Kill: solo si está vivo
        if (killButton != null) killButton.interactable = isAlive;

        // Revive: dejamos la lógica existente (no la tocamos aquí)
        if (reviveButton != null) reviveButton.interactable = !isAlive && health == 0;

        Debug.Log($"RefreshButtons -> HP: {health}/{max}, isAlive: {isAlive}, Heal interactable: {(healButton != null && healButton.interactable)}");
    }

    // Wrapper seguro para OnClick del botón Heal.
    // Verifica estado antes de llamar a PlayerHealth.Heal para evitar revivir accidentalmente.
    public void OnHealButtonPressed()
    {
        if (player == null)
        {
            Debug.LogWarning("OnHealButtonPressed: player no asignado.");
            return;
        }

        int health = player.GetHealth();
        int max = player.GetMaxHealth();
        bool isAlive = player.IsAlive();

        if (!isAlive)
        {
            Debug.Log("OnHealButtonPressed: el jugador está muerto, Heal no hará revive (operación ignorada).");
            return;
        }

        if (health >= max)
        {
            Debug.Log("OnHealButtonPressed: vida al máximo, no se puede curar.");
            return;
        }

        Debug.Log($"OnHealButtonPressed: curando. HP antes: {health}/{max}");
        player.Heal(15); // ajusta la cantidad de curación si quieres
    }

    public void OnDamageButtonPressed() => player?.TakeDamage(20);
    public void OnKillButtonPressed() => player?.Kill();
    public void OnReviveButtonPressed() => player?.Revive();
}