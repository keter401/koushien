#if UNITY_EDITOR

using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameState))]
public class GameStateDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // �e�I�u�W�F�N�g���擾
        object parentObject = GetParentObject(property);

        // �Ώۃt�B�[���h�̒l���擾
        FieldInfo fi = parentObject.GetType().GetField(property.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        GameState gameState = fi?.GetValue(parentObject) as GameState;

        string className = gameState?.GetType().Name ?? "None";

        EditorGUI.LabelField(position, label.text, className);
    }

    private object GetParentObject(SerializedProperty property)
    {
        string[] path = property.propertyPath.Split('.');
        object obj = property.serializedObject.targetObject;

        foreach (string part in path.Take(path.Length - 1)) {
            FieldInfo fi = obj.GetType().GetField(part, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            obj = fi?.GetValue(obj);
        }

        return obj;
    }

}

#endif