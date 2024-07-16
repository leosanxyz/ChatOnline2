using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]

public class SlotPersonaje : MonoBehaviour
{
    [Header("SO")]
    [SerializeField] private PersonajeSO personajeSO;

    [Header("UI")]
    [SerializeField] private Image personajeImage;
    [SerializeField] private TMP_Text personajeNombreText;

    private void OnValidate()
    {
        if (personajeSO)
        {
            personajeNombreText.text = personajeSO.name;
            personajeImage.sprite = personajeSO.pfPersonajeImage.sprite;
            personajeImage.rectTransform.anchoredPosition = personajeSO.pfPersonajeImage.rectTransform.anchoredPosition;

        }
    }

    public void OnPostRender()
    {
        
    }
}

