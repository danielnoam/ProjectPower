using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace DNExtensions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute 
    {
        public readonly string Name = "";
        public readonly int Size = 30;
        public readonly int Space = 3;
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
        private readonly Dictionary<string, object[]> _methodParameters = new Dictionary<string, object[]>();
        private readonly Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();
        
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
            
            // Get method parameters
            ParameterInfo[] parameters = method.GetParameters();
            string methodKey = target.GetInstanceID() + "_" + method.Name;
            
            // Initialize parameter values if not exists
            if (!_methodParameters.ContainsKey(methodKey))
            {
                _methodParameters[methodKey] = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    _methodParameters[methodKey][i] = GetDefaultValue(parameters[i].ParameterType);
                }
            }
            
            // Initialize foldout state if not exists
            _foldoutStates.TryAdd(methodKey, false);
            
            // Set button color
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = buttonAttr.Color;
            
            // Draw the button
            if (GUILayout.Button(buttonText, GUILayout.Height(buttonAttr.Size)))
            {
                try
                {
                    method.Invoke(target, _methodParameters[methodKey]);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error invoking method {method.Name}: {e.Message}");
                }
            }
            
            // Restore original color
            GUI.backgroundColor = originalColor;
            
            // Draw parameter foldout under the button
            if (parameters.Length > 0)
            {
                EditorGUI.indentLevel++;
                _foldoutStates[methodKey] = EditorGUILayout.Foldout(
                    _foldoutStates[methodKey], 
                    $"Parameters ({parameters.Length})",
                    true,
                    EditorStyles.foldoutHeader
                );
                
                if (_foldoutStates[methodKey])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        _methodParameters[methodKey][i] = DrawParameterField(
                            parameters[i].Name, 
                            parameters[i].ParameterType, 
                            _methodParameters[methodKey][i]
                        );
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private object DrawParameterField(string paramName, Type paramType, object currentValue)
        {
            string niceName = ObjectNames.NicifyVariableName(paramName);
            
            if (paramType == typeof(int))
            {
                return EditorGUILayout.IntField(niceName, currentValue != null ? (int)currentValue : 0);
            }
            else if (paramType == typeof(float))
            {
                return EditorGUILayout.FloatField(niceName, currentValue != null ? (float)currentValue : 0f);
            }
            else if (paramType == typeof(string))
            {
                return EditorGUILayout.TextField(niceName, currentValue != null ? (string)currentValue : "");
            }
            else if (paramType == typeof(bool))
            {
                return EditorGUILayout.Toggle(niceName, currentValue != null && (bool)currentValue);
            }
            else if (paramType == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(niceName, currentValue != null ? (Vector2)currentValue : Vector2.zero);
            }
            else if (paramType == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(niceName, currentValue != null ? (Vector3)currentValue : Vector3.zero);
            }
            else if (paramType == typeof(Color))
            {
                return EditorGUILayout.ColorField(niceName, currentValue != null ? (Color)currentValue : Color.white);
            }
            else if (paramType.IsEnum)
            {
                return EditorGUILayout.EnumPopup(niceName, currentValue != null ? (Enum)currentValue : (Enum)Enum.GetValues(paramType).GetValue(0));
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(paramType))
            {
                return EditorGUILayout.ObjectField(niceName, (UnityEngine.Object)currentValue, paramType, true);
            }
            else
            {
                EditorGUILayout.LabelField(niceName, $"Unsupported type: {paramType.Name}");
                return currentValue;
            }
        }
        
        private object GetDefaultValue(Type type)
        {
            if (type == typeof(string)) return "";
            if (type == typeof(int)) return 0;
            if (type == typeof(float)) return 0f;
            if (type == typeof(bool)) return false;
            if (type == typeof(Vector2)) return Vector2.zero;
            if (type == typeof(Vector3)) return Vector3.zero;
            if (type == typeof(Color)) return Color.white;
            if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return null;
            
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
    
    [CustomEditor(typeof(ScriptableObject), true)]
    public class ButtonAttributeScriptableObjectEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, object[]> _methodParameters = new Dictionary<string, object[]>();
        private readonly Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();
        
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
            
            // Get method parameters
            ParameterInfo[] parameters = method.GetParameters();
            string methodKey = target.GetInstanceID() + "_" + method.Name;
            
            // Initialize parameter values if not exists
            if (!_methodParameters.ContainsKey(methodKey))
            {
                _methodParameters[methodKey] = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    _methodParameters[methodKey][i] = GetDefaultValue(parameters[i].ParameterType);
                }
            }
            
            // Initialize foldout state if not exists
            _foldoutStates.TryAdd(methodKey, false);
            
            // Set button color
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = buttonAttr.Color;
            
            // Draw the button
            if (GUILayout.Button(buttonText, GUILayout.Height(buttonAttr.Size)))
            {
                try
                {
                    method.Invoke(target, _methodParameters[methodKey]);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error invoking method {method.Name}: {e.Message}");
                }
            }
            
            // Restore original color
            GUI.backgroundColor = originalColor;
            
            // Draw parameter foldout under the button
            if (parameters.Length > 0)
            {
                EditorGUI.indentLevel++;
                _foldoutStates[methodKey] = EditorGUILayout.Foldout(
                    _foldoutStates[methodKey], 
                    $"Parameters ({parameters.Length})",
                    true,
                    EditorStyles.foldoutHeader
                );
                
                if (_foldoutStates[methodKey])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        _methodParameters[methodKey][i] = DrawParameterField(
                            parameters[i].Name, 
                            parameters[i].ParameterType, 
                            _methodParameters[methodKey][i]
                        );
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private object DrawParameterField(string paramName, Type paramType, object currentValue)
        {
            string niceName = ObjectNames.NicifyVariableName(paramName);
            
            if (paramType == typeof(int))
            {
                return EditorGUILayout.IntField(niceName, currentValue != null ? (int)currentValue : 0);
            }
            else if (paramType == typeof(float))
            {
                return EditorGUILayout.FloatField(niceName, currentValue != null ? (float)currentValue : 0f);
            }
            else if (paramType == typeof(string))
            {
                return EditorGUILayout.TextField(niceName, currentValue != null ? (string)currentValue : "");
            }
            else if (paramType == typeof(bool))
            {
                return EditorGUILayout.Toggle(niceName, currentValue != null && (bool)currentValue);
            }
            else if (paramType == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(niceName, currentValue != null ? (Vector2)currentValue : Vector2.zero);
            }
            else if (paramType == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(niceName, currentValue != null ? (Vector3)currentValue : Vector3.zero);
            }
            else if (paramType == typeof(Color))
            {
                return EditorGUILayout.ColorField(niceName, currentValue != null ? (Color)currentValue : Color.white);
            }
            else if (paramType.IsEnum)
            {
                return EditorGUILayout.EnumPopup(niceName, currentValue != null ? (Enum)currentValue : (Enum)Enum.GetValues(paramType).GetValue(0));
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(paramType))
            {
                return EditorGUILayout.ObjectField(niceName, (UnityEngine.Object)currentValue, paramType, true);
            }
            else
            {
                EditorGUILayout.LabelField(niceName, $"Unsupported type: {paramType.Name}");
                return currentValue;
            }
        }
        
        private object GetDefaultValue(Type type)
        {
            if (type == typeof(string)) return "";
            if (type == typeof(int)) return 0;
            if (type == typeof(float)) return 0f;
            if (type == typeof(bool)) return false;
            if (type == typeof(Vector2)) return Vector2.zero;
            if (type == typeof(Vector3)) return Vector3.zero;
            if (type == typeof(Color)) return Color.white;
            if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return null;
            
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
#endif
}