using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheFairyHills.UI
{
    /// <summary>One selectable menu row, including pointer and controller focus feedback.</summary>
    public sealed class MainMenuOption : MonoBehaviour, IPointerEnterHandler, ISelectHandler, ICancelHandler
    {
        [SerializeField] private MainMenuController controller;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;
        [SerializeField] private bool isSettingsBack;
        [SerializeField] private Color normalColor = new Color(0.937f, 0.890f, 0.761f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.949f, 0.824f, 0.541f, 1f);
        [SerializeField] private Color disabledColor = new Color(0.937f, 0.890f, 0.761f, 0.5f);

        public Button Button => button;
        public bool IsSettingsBack => isSettingsBack;

        /// <summary>Called once by the editor scene builder to bind the authored row.</summary>
        public void Configure(MainMenuController owner, Button sourceButton, TMP_Text sourceLabel, bool settingsBack)
        {
            controller = owner;
            button = sourceButton;
            label = sourceLabel;
            isSettingsBack = settingsBack;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (button != null && button.interactable && controller != null)
                controller.FocusOption(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && button.interactable && controller != null)
                controller.FocusOption(this);
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (isSettingsBack && controller != null)
                controller.CloseSettings();
        }

        public void SetHighlighted(bool highlighted)
        {
            if (label == null)
                return;

            bool canSelect = button != null && button.interactable;
            label.color = !canSelect ? disabledColor : highlighted ? selectedColor : normalColor;

            Material material = label.fontMaterial;
            SetMaterialColor(material, "_OutlineColor", new Color(0.025f, 0.055f, 0.09f, 0.9f));
            SetMaterialFloat(material, "_OutlineWidth", 0.12f);
            SetMaterialColor(material, "_UnderlayColor", highlighted
                ? new Color(0.949f, 0.824f, 0.541f, 0.82f)
                : new Color(0.015f, 0.035f, 0.07f, 0.78f));
            SetMaterialFloat(material, "_UnderlayOffsetX", 0.04f);
            SetMaterialFloat(material, "_UnderlayOffsetY", -0.06f);
            SetMaterialFloat(material, "_UnderlayDilate", 0.12f);
            SetMaterialFloat(material, "_UnderlaySoftness", 0.35f);
        }

        private static void SetMaterialColor(Material material, string propertyName, Color value)
        {
            if (material != null && material.HasProperty(propertyName))
                material.SetColor(propertyName, value);
        }

        private static void SetMaterialFloat(Material material, string propertyName, float value)
        {
            if (material != null && material.HasProperty(propertyName))
                material.SetFloat(propertyName, value);
        }
    }
}
