using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LineCircles
{
    [CustomPropertyDrawer(typeof(Oscillator))]
    public class OscillatorEditor : PropertyDrawer
    {

        private const float Padding = 6f;
        private const float LabelWidth = 50f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.HelpBox(position, "", MessageType.None);
            var innerRect = new Rect(position.x, position.y + Padding, position.width,
                position.height - (2f * Padding));
            var rects = new Rect[3][];
            for (var i = 0; i < 3; i++) {
                var startY = innerRect.y + i * (16f + 2f);
                var fieldWidth = (innerRect.width - 2f * LabelWidth) / 2f;
                var widths = new[] {
                    LabelWidth, fieldWidth, LabelWidth, fieldWidth
                };
                rects[i] = new Rect[4];
                var x = innerRect.x;
                for (var j = 0; j < 4; j++) {
                    rects[i][j] = new Rect(x, startY, widths[j], 16f);
                    x += widths[j];
                }
            }
            //var rects = new[] {
            //    new Rect(innerRect.x                         , innerRect.y + 0f , innerRect.width * 0.5f, 16f),
            //    new Rect(innerRect.x + innerRect.width * 0.5f, innerRect.y + 0f , innerRect.width * 0.5f, 16f),
            //    new Rect(innerRect.x                         , innerRect.y + 16f, innerRect.width * 0.5f, 16f),
            //    new Rect(innerRect.x + innerRect.width * 0.5f, innerRect.y + 16f, innerRect.width * 0.5f, 16f),
            //    new Rect(innerRect.x                         , innerRect.y + 32f, innerRect.width * 0.5f, 16f),
            //    new Rect(innerRect.x + innerRect.width * 0.5f, innerRect.y + 32f, innerRect.width * 0.5f, 16f),
            //};

            var style = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleRight
            };
            var pre = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80f;

            EditorGUI.LabelField(rects[0][0], "Name", style);
            EditorGUI.PropertyField(rects[0][1], property.FindPropertyRelative("Name"), GUIContent.none);
            EditorGUI.LabelField(rects[0][2], "Type", style);
            EditorGUI.PropertyField(rects[0][3], property.FindPropertyRelative("Type"), GUIContent.none);
            
            EditorGUI.LabelField(rects[1][0], "Center", style);
            EditorGUI.PropertyField(rects[1][1], property.FindPropertyRelative("Center"), GUIContent.none);
            EditorGUI.LabelField(rects[1][2], "Amp.", style);
            EditorGUI.PropertyField(rects[1][3], property.FindPropertyRelative("Amplitude"), GUIContent.none);
            
            EditorGUI.LabelField(rects[2][0], "Period", style);
            EditorGUI.PropertyField(rects[2][1], property.FindPropertyRelative("Period"), GUIContent.none);
            EditorGUI.LabelField(rects[2][2], "Phase", style);
            EditorGUI.PropertyField(rects[2][3], property.FindPropertyRelative("Phase"), GUIContent.none);

            EditorGUIUtility.labelWidth = pre;
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 32f + 2f * Padding + base.GetPropertyHeight(property, label);
        }
    }
}