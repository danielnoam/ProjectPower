using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace DNExtensions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute 
    {
        public string Name = "";
        public int Size = 30;
        public int Space = 3;
        public Color Color = Color.white;

        /// <summary>
        /// Adds a button for the method in the inspector
        /// </summary>
        public ButtonAttribute() {}
        
        
        /// <summary>
        /// Adds a button for the method in the inspector
        /// </summary>
        public ButtonAttribute(string name)
        {
            this.Name = name;
        }
        
        
        /// <summary>
        /// Adds a button for the method in the inspector
        /// </summary>
        public ButtonAttribute(string name, int size)
        {
            this.Name = name;
            this.Size = size;
        }
        
        
        /// <summary>
        /// Adds a button for the method in the inspector
        /// </summary>
        public ButtonAttribute(string name, int size, int space)
        {
            this.Name = name;
            this.Size = size;
            this.Space = space;
        }
        
        /// <summary>
        /// Adds a button for the method in the inspector
        /// </summary>
        public ButtonAttribute(string name, int size, int space, Color color)
        {
            this.Name = name;
            this.Size = size;
            this.Space = space;
            this.Color = color;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonAttributeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            // Get all methods with ButtonAttribute
            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            
            foreach (MethodInfo method in methods)
            {
                ButtonAttribute buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
                if (buttonAttr != null)
                {
                    DrawButton(method, buttonAttr);
                }
            }
        }
        
        private void DrawButton(MethodInfo method, ButtonAttribute buttonAttr)
        {
            // Apply spacing
            if (buttonAttr.Space > 0)
            {
                GUILayout.Space(buttonAttr.Space);
            }
            
            // Get button text
            string buttonText = string.IsNullOrEmpty(buttonAttr.Name) ? 
                ObjectNames.NicifyVariableName(method.Name) : buttonAttr.Name;
            
            // Set button color
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = buttonAttr.Color;
            
            // Draw the button
            if (GUILayout.Button(buttonText, GUILayout.Height(buttonAttr.Size)))
            {
                method.Invoke(target, null);
            }
            
            // Restore original color
            GUI.backgroundColor = originalColor;
        }
    }
    
    

    [CustomEditor(typeof(ScriptableObject), true)]
    public class ButtonAttributeScriptableObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            // Get all methods with ButtonAttribute
            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            
            foreach (MethodInfo method in methods)
            {
                ButtonAttribute buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
                if (buttonAttr != null)
                {
                    DrawButton(method, buttonAttr);
                }
            }
        }
        
        private void DrawButton(MethodInfo method, ButtonAttribute buttonAttr)
        {
            // Apply spacing
            if (buttonAttr.Space > 0)
            {
                GUILayout.Space(buttonAttr.Space);
            }
            
            // Get button text
            string buttonText = string.IsNullOrEmpty(buttonAttr.Name) ? 
                ObjectNames.NicifyVariableName(method.Name) : buttonAttr.Name;
            
            // Set button color
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = buttonAttr.Color;
            
            // Draw the button
            if (GUILayout.Button(buttonText, GUILayout.Height(buttonAttr.Size)))
            {
                method.Invoke(target, null);
            }
            
            // Restore original color
            GUI.backgroundColor = originalColor;
        }
    }
#endif
}