using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Draw key underlines from TMP glyph positions, without changing shared font atlases.</summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class KeyHintUnderlines : MonoBehaviour
    {
        private TMP_Text text;
        private readonly List<UnityEngine.UI.Image> lines = new List<UnityEngine.UI.Image>();
        private static readonly Regex Keys = new Regex(@"(?<![A-Za-z0-9_])(Space|Shift|Enter|Esc|E|Q|T)(?![A-Za-z0-9_])");
        private static readonly Regex ControlKeys = new Regex(@"(?<![A-Za-z0-9_])(Space|Shift|Enter|Esc|A|D|E|Q|T|\u2190|\u2192)(?![A-Za-z0-9_])");

        public static string Format(string value, bool includeMovementKeys = false)
        {
            if (string.IsNullOrEmpty(value) || value.Contains("<link=\"key\">")) return value;
            return (includeMovementKeys ? ControlKeys : Keys).Replace(value, "<link=\"key\">$1</link>");
        }

        public static void Attach(TMP_Text target)
        {
            if (target == null) return;
            target.richText = true;
            if (target.GetComponent<KeyHintUnderlines>() == null) target.gameObject.AddComponent<KeyHintUnderlines>();
        }

        private void OnEnable()
        {
            text = GetComponent<TMP_Text>();
            text.OnPreRenderText += Draw;
            text.SetVerticesDirty();
        }

        private void OnDisable()
        {
            if (text != null) text.OnPreRenderText -= Draw;
            foreach (var line in lines) if (line != null) line.enabled = false;
        }

        private void Draw(TMP_TextInfo info)
        {
            int used = 0;
            for (int i = 0; i < info.linkCount; i++)
            {
                var link = info.linkInfo[i];
                if (link.GetLinkID() != "key") continue;
                int start = link.linkTextfirstCharacterIndex;
                int end = Mathf.Min(info.characterCount, start + link.linkTextLength);
                for (int at = start; at < end;)
                {
                    var first = info.characterInfo[at];
                    float left = first.bottomLeft.x, right = first.topRight.x;
                    int lineNumber = first.lineNumber;
                    int next = at + 1;
                    while (next < end && info.characterInfo[next].lineNumber == lineNumber)
                    { right = Mathf.Max(right, info.characterInfo[next].topRight.x); next++; }
                    if (right > left)
                    {
                        var line = GetLine(used++);
                        var rect = line.rectTransform;
                        rect.anchorMin = rect.anchorMax = text.rectTransform.pivot;
                        rect.pivot = new Vector2(0, 0.5f);
                        rect.anchoredPosition = new Vector2(left, first.baseLine - first.pointSize * 0.10f);
                        rect.sizeDelta = new Vector2(right - left, Mathf.Max(1.5f, first.pointSize * 0.055f));
                        line.color = text.color;
                        line.enabled = true;
                    }
                    at = next;
                }
            }
            for (int i = used; i < lines.Count; i++) lines[i].enabled = false;
        }

        private UnityEngine.UI.Image GetLine(int index)
        {
            if (index < lines.Count) return lines[index];
            var obj = new GameObject("Key Underline", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            obj.transform.SetParent(transform, false);
            var line = obj.GetComponent<UnityEngine.UI.Image>();
            line.raycastTarget = false;
            lines.Add(line);
            return line;
        }
    }
}
