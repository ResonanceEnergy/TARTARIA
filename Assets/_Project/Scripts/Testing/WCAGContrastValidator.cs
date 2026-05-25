using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Tartaria.Testing
{
    /// <summary>
    /// WCAG Contrast Ratio Validator — scans all UI text elements and validates
    /// contrast ratios against WCAG 2.1 AA standards (4.5:1 for normal text, 3:1 for large text).
    /// 
    /// Usage: Attach to any object, or invoke from menu Tools > Tartaria > Validate WCAG Contrast.
    /// Logs all violations and optionally highlights them in the scene.
    /// </summary>
    public class WCAGContrastValidator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] bool highlightViolations = true;
        [SerializeField] Color violationHighlightColor = Color.red;

        readonly List<ContrastViolation> _violations = new();

        public struct ContrastViolation
        {
            public string objectName;
            public float contrastRatio;
            public float requiredRatio;
            public Color foreground;
            public Color background;
            public bool isLargeText;
            public GameObject gameObject;
        }

        [ContextMenu("Validate All Text Contrast")]
        public void ValidateAllTextContrast()
        {
            _violations.Clear();
            
            // Find all TextMeshProUGUI components
            var tmpTexts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var tmp in tmpTexts)
            {
                ValidateTextElement(tmp.gameObject, tmp.color, tmp.fontSize);
            }

            // Find all Text components (legacy)
            var texts = FindObjectsOfType<Text>(true);
            foreach (var text in texts)
            {
                ValidateTextElement(text.gameObject, text.color, text.fontSize);
            }

            // Report results
            Debug.Log($"[WCAG] Contrast validation complete. Found {_violations.Count} violations.");

            if (_violations.Count > 0)
            {
                Debug.LogWarning("[WCAG] === CONTRAST VIOLATIONS ===");
                foreach (var violation in _violations)
                {
                    Debug.LogWarning(
                        $"[WCAG] {violation.objectName}: {violation.contrastRatio:F2}:1 " +
                        $"(required {violation.requiredRatio}:1 for {(violation.isLargeText ? "large" : "normal")} text)",
                        violation.gameObject);
                }
            }
            else
            {
                Debug.Log("[WCAG] ✓ All text elements meet WCAG 2.1 AA contrast requirements!");
            }
        }

        void ValidateTextElement(GameObject obj, Color foreground, float fontSize)
        {
            // Determine background color (check parent Image/Panel)
            Color background = GetBackgroundColor(obj.transform);

            // Calculate contrast ratio
            float contrastRatio = CalculateContrastRatio(foreground, background);

            // Determine if text is "large" (18pt+ or 14pt+ bold)
            bool isLargeText = fontSize >= 18f;

            // WCAG AA requirements: 4.5:1 for normal text, 3:1 for large text
            float requiredRatio = isLargeText ? 3f : 4.5f;

            if (contrastRatio < requiredRatio)
            {
                _violations.Add(new ContrastViolation
                {
                    objectName = GetHierarchyPath(obj.transform),
                    contrastRatio = contrastRatio,
                    requiredRatio = requiredRatio,
                    foreground = foreground,
                    background = background,
                    isLargeText = isLargeText,
                    gameObject = obj
                });

                if (highlightViolations)
                {
                    HighlightViolation(obj);
                }
            }
        }

        Color GetBackgroundColor(Transform t)
        {
            // Walk up hierarchy to find an Image or Panel component
            while (t != null)
            {
                var image = t.GetComponent<Image>();
                if (image != null) return image.color;

                var rawImage = t.GetComponent<RawImage>();
                if (rawImage != null) return rawImage.color;

                t = t.parent;
            }

            // Default to black background if none found
            return Color.black;
        }

        float CalculateContrastRatio(Color fg, Color bg)
        {
            // Convert to relative luminance (WCAG formula)
            float L1 = GetRelativeLuminance(fg);
            float L2 = GetRelativeLuminance(bg);

            // Ensure L1 is the lighter color
            if (L1 < L2)
            {
                float temp = L1;
                L1 = L2;
                L2 = temp;
            }

            // Contrast ratio formula: (L1 + 0.05) / (L2 + 0.05)
            return (L1 + 0.05f) / (L2 + 0.05f);
        }

        float GetRelativeLuminance(Color color)
        {
            // WCAG relative luminance formula
            float r = GetLuminanceComponent(color.r);
            float g = GetLuminanceComponent(color.g);
            float b = GetLuminanceComponent(color.b);

            return 0.2126f * r + 0.7152f * g + 0.0722f * b;
        }

        float GetLuminanceComponent(float component)
        {
            // Linearize sRGB component
            if (component <= 0.03928f)
                return component / 12.92f;
            else
                return Mathf.Pow((component + 0.055f) / 1.055f, 2.4f);
        }

        void HighlightViolation(GameObject obj)
        {
            // Add a red outline to highlight the violation
            var outline = obj.GetComponent<UnityEngine.UI.Outline>();
            if (outline == null)
                outline = obj.AddComponent<UnityEngine.UI.Outline>();

            outline.effectColor = violationHighlightColor;
            outline.effectDistance = new Vector2(2, -2);
        }

        string GetHierarchyPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(WCAGContrastValidator))]
    public class WCAGContrastValidatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var validator = (WCAGContrastValidator)target;

            if (GUILayout.Button("Validate All Text Contrast", GUILayout.Height(30)))
            {
                validator.ValidateAllTextContrast();
            }
        }
    }
#endif
}
