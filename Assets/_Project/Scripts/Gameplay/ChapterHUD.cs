using System.Collections;
using TMPro;
using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Minimal keyboard hints, interaction prompt, and short gameplay messages.</summary>
    public sealed class ChapterHUD : MonoBehaviour
    {
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
            if (promptText != null) promptText.text = prompt;
            if (promptPanel != null) promptPanel.SetActive(!string.IsNullOrEmpty(prompt));
        }

        public void ShowMessage(string message, float seconds = 2f)
        {
            if (messageText == null) return;
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            messageText.text = message;
            if (messagePanel != null) messagePanel.SetActive(true);
            messageRoutine = StartCoroutine(ClearMessageAfter(seconds));
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
