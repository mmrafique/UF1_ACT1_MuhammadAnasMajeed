using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// Controla que siempre haya un botón seleccionado en el EventSystem.
/// Mantiene la última selección válida y, si se pierde la selección,
/// intenta restaurarla (con una pequeña espera para no pelear con clicks).
/// </summary>
public class EventSystemController : MonoBehaviour
{
    [Tooltip("Botón que queremos que sea el 'first selected' al empezar.")]
    public GameObject firstSelected;

    private GameObject lastSelected;
    private Coroutine restoreCoroutine;

    void Start()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("No hay EventSystem en la escena. Agrega uno con GameObject -> UI -> EventSystem.", this);
            return;
        }

        if (firstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
            lastSelected = firstSelected;
        }
    }

    void Update()
    {
        if (EventSystem.current == null) return;

        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current != null)
        {
            // Guardamos el último seleccionado válido
            lastSelected = current;

            // Si hay una corrutina intentando restaurar, la cancelamos
            if (restoreCoroutine != null)
            {
                StopCoroutine(restoreCoroutine);
                restoreCoroutine = null;
            }
        }
        else
        {
            // Si no hay seleccionado, iniciamos una corrutina para restaurar después de un frame breve
            if (restoreCoroutine == null)
            {
                restoreCoroutine = StartCoroutine(RestoreSelectionNextFrame());
            }
        }
    }

    private IEnumerator RestoreSelectionNextFrame()
    {
        // Esperamos un frame para evitar interferir con eventos de click/hover
        yield return null;

        if (EventSystem.current == null)
        {
            restoreCoroutine = null;
            yield break;
        }

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            GameObject toSelect = lastSelected != null ? lastSelected : firstSelected;
            if (toSelect != null)
            {
                EventSystem.current.SetSelectedGameObject(toSelect);
            }
        }

        restoreCoroutine = null;
    }
}
