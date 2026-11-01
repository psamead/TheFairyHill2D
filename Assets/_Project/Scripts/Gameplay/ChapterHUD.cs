using System.Collections;
using TMPro;
using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Minimal keyboard hints, interaction prompt, and short gameplay messages.</summary>
    public sealed class ChapterHUD : MonoBehaviour
    {
        private const float FramePadding = 12f;

        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private GameObject messagePanel;
        private Coroutine messageRoutine;

        public void Configure(TMP_Text prompt, TMP_Text message, GameObject promptFrame = null, GameObject messageFrame = null)
        {
            promptText = prompt;
            messageText = message;
            promptPanel = promptFrame;
            messagePanel = messageFrame;
            if (promptPanel != null) promptPanel.SetActive(false);
            if (messagePanel != null) messagePanel.SetActive(false);
        }

        public void SetPrompt(string prompt)
        {
            if (promptText != null && promptText.text != prompt)
            {
                promptText.text = prompt;
                FitPanelToText(promptText, promptPanel);
            }
            if (promptPanel != null && promptPanel.activeSelf != !string.IsNullOrEmpty(prompt))
                promptPanel.SetActive(!string.IsNullOrEmpty(prompt));
        }

        public void ShowMessage(string message, float seconds = 2f)
        {
            if (messageText == null) return;
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            messageText.text = message;
            FitPanelToText(messageText, messagePanel);
            if (messagePanel != null) messagePanel.SetActive(true);
            messageRoutine = StartCoroutine(ClearMessageAfter(seconds));
        }

        private static void FitPanelToText(TMP_Text text, GameObject panel)
        {
            if (text == null || panel == null) return;
            Vector2 textSize = text.GetPreferredValues(text.text);
            textSize.x = Mathf.Ceil(Mathf.Max(1f, textSize.x));
            textSize.y = Mathf.Ceil(Mathf.Max(1f, textSize.y));
            text.rectTransform.sizeDelta = textSize;
            RectTransform panelRect = panel.transform as RectTransform;
            if (panelRect != null)
                panelRect.sizeDelta = textSize + Vector2.one * (FramePadding * 2f);
        }

        private IEnumerator ClearMessageAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (messageText != null) messageText.text = string.Empty;
            if (messagePanel != null) messagePanel.SetActive(false);
            messageRoutine = null;
        }

        private void OnDisable()
        {
            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
                messageRoutine = null;
            }
        }
    }
}
