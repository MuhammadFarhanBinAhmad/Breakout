using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(SOAbilityEffect))]
public class AbilityEffectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Build a mapping: controllerBoolName -> list of child field names
        var groupMap = new Dictionary<string, List<string>>();
        FieldInfo[] allFields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var f in allFields)
        {
            var groupAttr = f.GetCustomAttribute<GroupUnderAttribute>();
            if (groupAttr != null)
            {
                if (!groupMap.TryGetValue(groupAttr.BoolField, out var list))
                {
                    list = new List<string>();
                    groupMap[groupAttr.BoolField] = list;
                }
                list.Add(f.Name);
            }
        }

        // Keep track of children we've already drawn so we can skip them later
        var drawnChildren = new HashSet<string>();

        // Iterate serialized properties
        SerializedProperty prop = serializedObject.GetIterator();

        // Move to first visible property
        if (prop.NextVisible(true))
        {
            do
            {
                // If we've already drawn this property as a grouped child, skip it
                if (drawnChildren.Contains(prop.name))
                    continue;

                // Always draw the script reference first (m_Script)
                if (prop.name == "m_Script")
                {
                    EditorGUILayout.PropertyField(prop, true);
                    continue;
                }

                // If this property is a controller (i.e., has grouped children), draw it and then draw its children
                if (groupMap.TryGetValue(prop.name, out var children))
                {
                    EditorGUILayout.PropertyField(prop, true);

                    // If controller bool is true, draw children indented
                    // Only works for boolean controller properties - check the property type
                    if (prop.propertyType == SerializedPropertyType.Boolean && prop.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var childName in children)
                        {
                            var childProp = serializedObject.FindProperty(childName);
                            if (childProp != null)
                            {
                                EditorGUILayout.PropertyField(childProp, true);
                                drawnChildren.Add(childName);
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    // done with this controller (skip normal drawing of the children below)
                    continue;
                }

                // If this field itself is a grouped child (but hasn't been drawn yet) - skip it.
                // This prevents it appearing in its natural order later.
                FieldInfo thisField = target.GetType().GetField(prop.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (thisField != null && thisField.GetCustomAttribute<GroupUnderAttribute>() != null)
                {
                    // This grouped child will have already been drawn under its controller (or skipped if controller false).
                    // So skip drawing it here.
                    continue;
                }

                // Default drawing for everything else
                EditorGUILayout.PropertyField(prop, true);

            } while (prop.NextVisible(false));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
