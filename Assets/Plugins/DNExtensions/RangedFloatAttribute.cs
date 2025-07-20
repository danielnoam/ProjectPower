/*
 * RangedFloat Custom Attribute
 * 
 * This attribute creates a min-max slider in the Unity Inspector for float ranges.
 * 
 * Usage Examples:
 * 1. With MinMaxRange attribute (recommended):
 *    [MinMaxRange(-5f, 5f)] 
 *    public RangedFloat spawnDelay;
 * 
 * 2. With MinMaxRange and direct float initialization:
 *    [MinMaxRange(-5f, 5f)] 
 *    public RangedFloat damage = 5f;  // Creates range from -5 to 5
 * 
 * 3. With direct value initialization (no attribute):
 *    public RangedFloat health = new RangedFloat(50f, 100f);
 * 
 * 4. Without any initialization (defaults to 0-1 range):
 *    public RangedFloat defaultRange;
 * 
 * Available Functions:
 * - RandomValue: Get a random value within the range
 *     float random = myRange.RandomValue;
 * 
 * - Lerp: Interpolate within the range
 *     float interpolated = myRange.Lerp(0.5f);  // Get middle value
 * 
 * - Contains: Check if a value is within the range
 *     bool isInRange = myRange.Contains(value);
 * 
 * - Clamp: Force a value to be within the range
 *     float clamped = myRange.Clamp(value);
 * 
 * - Range: Get the size of the range
 *     float size = myRange.Range;
 * 
 * - Average: Get the middle value of the range
 *     float middle = myRange.Average;
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DNExtensions
{
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RangedFloat), true)]
    public class RangedFloatDrawer : PropertyDrawer
    {
        // Cached style for better performance
        private static GUIStyle _labelStyle;
        private static GUIStyle GetLabelStyle()
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.UpperCenter
                };
            }
            return _labelStyle;
        }
        
        // Customizable UI constants
        private const float FieldPadding = 5f;     // Spacing between UI elements
        private const float FieldWidth = 50f;      // Width of the min/max input fields
        private const float FieldHeight = 18f;    // Height of the min/max input fields
        private const float DefaultMinRange = -1f; // Default minimum range when no attribute is specified
        private const float DefaultMaxRange = 1f;  // Default maximum range when no attribute is specified
        private const float RangePaddingPercent = 0.2f; // Padding percentage for dynamic range calculation
        
        // Range label settings
        private const bool ShowRangeValue = true;  // Toggle to show/hide the range value
        private const float LabelYOffset = 15f;   // How far above the slider to show the label
        private const int DecimalPlaces = 1;        // Decimal places for range value

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShowRangeValue ? FieldHeight + 15f : FieldHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Draw the label
            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty minProp = property.FindPropertyRelative("minValue");
            SerializedProperty maxProp = property.FindPropertyRelative("maxValue");

            // Get custom range attributes or use defaults
            float rangeMin, rangeMax;
            var ranges = (MinMaxRangeAttribute[])fieldInfo.GetCustomAttributes(typeof(MinMaxRangeAttribute), true);
            if (ranges.Length > 0)
            {
                rangeMin = ranges[0].Min;
                rangeMax = ranges[0].Max;
            }
            else
            {
                if (minProp.floatValue == 0 && maxProp.floatValue == 0)
                {
                    rangeMin = DefaultMinRange;
                    rangeMax = DefaultMaxRange;
                }
                else
                {
                    float padding = (maxProp.floatValue - minProp.floatValue) * RangePaddingPercent;
                    rangeMin = minProp.floatValue - padding;
                    rangeMax = maxProp.floatValue + padding;
                }
            }

            // Calculate rects
            Rect minFieldRect = new Rect(position.x, position.y, FieldWidth, FieldHeight);
            Rect sliderRect = new Rect(minFieldRect.xMax + FieldPadding, position.y, 
                position.width - (FieldWidth * 2) - (FieldPadding * 2), FieldHeight);
            Rect maxFieldRect = new Rect(sliderRect.xMax + FieldPadding, position.y, FieldWidth, FieldHeight);

            // Min field
            EditorGUI.BeginChangeCheck();
            float minValue = EditorGUI.FloatField(minFieldRect, minProp.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                minProp.floatValue = Mathf.Min(minValue, maxProp.floatValue);
            }

            // Draw range value above slider if enabled
            if (ShowRangeValue)
            {
                float rangeValue = maxProp.floatValue - minProp.floatValue;
    
                Rect labelRect = new Rect(
                    sliderRect.x, 
                    sliderRect.y + LabelYOffset, 
                    sliderRect.width, 
                    20
                );
    
                EditorGUI.LabelField(labelRect, "Range " + rangeValue.ToString($"F{DecimalPlaces}"), GetLabelStyle());
            }

            // Slider
            EditorGUI.BeginChangeCheck();
            float tempMin = minProp.floatValue;
            float tempMax = maxProp.floatValue;
            EditorGUI.MinMaxSlider(sliderRect, ref tempMin, ref tempMax, rangeMin, rangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                minProp.floatValue = tempMin;
                maxProp.floatValue = tempMax;
            }

            // Max field
            EditorGUI.BeginChangeCheck();
            float maxValue = EditorGUI.FloatField(maxFieldRect, maxProp.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                maxProp.floatValue = Mathf.Max(maxValue, minProp.floatValue);
            }

            EditorGUI.EndProperty();
        }
    }
    #endif
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public MinMaxRangeAttribute(float min, float max)
        {
            Min = Mathf.Min(min, max);
            Max = Mathf.Max(min, max);
        }
    }

    [System.Serializable]
    public struct RangedFloat
    {
        public float minValue;
        public float maxValue;

        // Constructor
        public RangedFloat(float min, float max)
        {
            minValue = min;
            maxValue = max;
        }

        // Implicit conversion from float
        public static implicit operator RangedFloat(float value)
        {
            return new RangedFloat(-value, value);
        }

        // Utility properties
        public float RandomValue => Random.Range(minValue, maxValue);
        public float Range => maxValue - minValue;
        public float Average => (minValue + maxValue) * 0.5f;
        public float Lerp(float t) => Mathf.Lerp(minValue, maxValue, t);
        public bool Contains(float value) => value >= minValue && value <= maxValue;
        public float Clamp(float value) => Mathf.Clamp(value, minValue, maxValue);
        public override string ToString() => $"({minValue:F2} - {maxValue:F2})";
    }
}