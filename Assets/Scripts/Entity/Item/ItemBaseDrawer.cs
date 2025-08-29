#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ItemBase))]
public class ItemBaseDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var nameProp = property.FindPropertyRelative("name");
        var hpProp = property.FindPropertyRelative("type");

        // ���x���� name �̒l�ɕύX
        string displayName = string.IsNullOrEmpty(nameProp.stringValue) ? "Unnamed" : nameProp.stringValue;
        label.text = displayName;

        // �܂肽���݉\�ȕ\��
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded, label);

        if (property.isExpanded) {
            EditorGUI.indentLevel++;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = 2f;
            Rect nameRect = new Rect(position.x, position.y + lineHeight + padding, position.width, lineHeight);
            Rect hpRect = new Rect(position.x, position.y + (lineHeight + padding) * 2, position.width, lineHeight);

            EditorGUI.PropertyField(nameRect, nameProp);
            EditorGUI.PropertyField(hpRect, hpProp);

            EditorGUI.indentLevel--;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) {
            return EditorGUIUtility.singleLineHeight;
        }

        return (EditorGUIUtility.singleLineHeight + 2f) * 3;
    }

}

#endif