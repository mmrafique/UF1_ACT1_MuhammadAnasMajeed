using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Controla la vida del jugador de forma encapsulada.
/// Variables privadas (serializables) y acceso mediante métodos públicos.
/// Dispara eventos cuando la vida cambia / muere / revive.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Ajustes de vida")]
    [Tooltip("Vida máxima que puede tener el jugador.")]
    [SerializeField] private int maxHealth = 100;

    [Tooltip("Vida inicial al empezar la escena.")]
    [SerializeField] private int startingHealth = 100;

    // Estado interno (privado)
    private int currentHealth;
    private bool alive = true;

    // Eventos para que la UI u otros listeners se suscriban
    public event Action<int> OnHealthChanged;
    public event Action OnDeath;
    public event Action OnRevive;

    void Awake()
    {
        // Inicializamos el estado asegurando que esté en rango válido
        currentHealth = Mathf.Clamp(startingHealth, 0, maxHealth);
        alive = currentHealth > 0;
    }

    // ---------------------------
    // Getters públicos (rúbrica)
    // ---------------------------
    public int GetHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsAlive()
    {
        return alive;
    }

    // ---------------------------
    // Métodos para modificar la vida
    // ---------------------------

    /// <summary>
    /// Inflige daño. No hace nada si el personaje ya está muerto.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (!alive) return;
        if (amount <= 0) return; // protección simple
        SetHealth(currentHealth - amount);
    }

    /// <summary>
    /// Cura al personaje si está vivo. Si está muerto, usar Revive.
    /// </summary>
    public void Heal(int amount)
    {
        if (!alive) return;
        if (amount <= 0) return;
        SetHealth(currentHealth + amount);
    }

    /// <summary>
    /// Mata inmediatamente al personaje (pone la vida a 0).
    /// </summary>
    public void Kill()
    {
        if (!alive) return;
        SetHealth(0);
        // no necesitamos llamar OnDeath aquí porque SetHealth lo hace cuando procede
    }

    /// <summary>
    /// Revive al personaje si está muerto.
    /// Por defecto lo revive con la mitad de la vida máxima (pero >= 1).
    /// Cambia este comportamiento si prefieres otra lógica.
    /// </summary>
    public void Revive()
    {
        if (alive) return;
        int reviveHealth = Mathf.Max(1, maxHealth / 2);
        SetHealth(reviveHealth);
    }

    // ---------------------------
    // Lógica centralizada de asignación de vida
    // ---------------------------
    private void SetHealth(int newHealth)
    {
        int clamped = Mathf.Clamp(newHealth, 0, maxHealth);

        // Si no hay cambio real, no hacemos nada: esto cumple la parte "solo actualizar cuando hay cambios".
        if (clamped == currentHealth) return;

        int oldHealth = currentHealth;
        bool wasAlive = alive;

        currentHealth = clamped;
        alive = currentHealth > 0;

        // Notificar cambio de vida (pasamos el nuevo valor)
        OnHealthChanged?.Invoke(currentHealth);

        // Notificar muerte o revive si cambió el estado
        if (wasAlive && !alive)
        {
            // Debug opcional para desarrollo
            Debug.Log($"{name}: Player has died (HP went from {oldHealth} to {currentHealth}).");
            OnDeath?.Invoke();
        }
        else if (!wasAlive && alive)
        {
            Debug.Log($"{name}: Player revived with {currentHealth} HP.");
            OnRevive?.Invoke();
        }
    }

    // Método de ayuda para pruebas desde el inspector (opcional)
    [ContextMenu("Do 10 Damage (debug)")]
    private void DebugDamage10()
    {
        TakeDamage(10);
    }
}