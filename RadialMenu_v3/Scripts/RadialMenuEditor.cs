#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 날짜 : 2021-04-29 PM 7:52:37
// 작성자 : Rito

namespace Rito.RadialMenu_v3.Editor
{
    [CustomEditor(typeof(RadialMenu))]
    public class RadialMenuEditor : UnityEditor.Editor
    {
        private RadialMenu rm;

        private static readonly Color LightRed = Color.red * 2f;
        private static readonly Color LightYellow = Color.yellow * 2f;
        private static readonly Color LightCyan = Color.cyan * 2f;

        private SerializedProperty _selectedIndex;

        private SerializedProperty _pieceCount;
        private SerializedProperty _pieceDist;
        private SerializedProperty _centerRange;

        private SerializedProperty _appearanceDuration;
        private SerializedProperty _disppearanceDuration;

        private SerializedProperty _pieceSample;
        private SerializedProperty _arrow;

        private SerializedProperty _appearanceType;
        private SerializedProperty _appearanceEasing;
        private SerializedProperty _mainType;
        private SerializedProperty _disappearanceType;
        private SerializedProperty _disappearanceEasing;

        private bool run = true;

        private void OnEnable()
        {
            rm = target as RadialMenu;

            try
            {
                _selectedIndex = serializedObject.FindProperty(nameof(_selectedIndex));
                _pieceCount = serializedObject.FindProperty(nameof(_pieceCount));
                _pieceDist = serializedObject.FindProperty(nameof(_pieceDist));
                _centerRange = serializedObject.FindProperty(nameof(_centerRange));
                _appearanceDuration = serializedObject.FindProperty(nameof(_appearanceDuration));
                _disppearanceDuration = serializedObject.FindProperty(nameof(_disppearanceDuration));
                _pieceSample = serializedObject.FindProperty(nameof(_pieceSample));
                _arrow = serializedObject.FindProperty(nameof(_arrow));
                _appearanceType = serializedObject.FindProperty(nameof(_appearanceType));
                _appearanceEasing = serializedObject.FindProperty(nameof(_appearanceEasing));
                _mainType = serializedObject.FindProperty(nameof(_mainType));
                _disappearanceType = serializedObject.FindProperty(nameof(_disappearanceType));
                _disappearanceEasing = serializedObject.FindProperty(nameof(_disappearanceEasing));
            }
            catch
            {
                run = false;
            }
        }

        public override void OnInspectorGUI()
        {
            if (!run)
            {
                base.OnInspectorGUI();
                return;
            }

            ColorLabelField("Information", LightRed);

            EditorGUILayout.LabelField("Selected Index", 
                _selectedIndex.intValue.ToString(), EditorStyles.boldLabel);

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("Piece Count",
                    _pieceCount.intValue.ToString(), EditorStyles.boldLabel);
            }

            EditorGUILayout.Space(12f);

            ColorLabelField("Options", LightYellow);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.IntSlider(_pieceCount, 2, 16, "Piece Count");
            }

            EditorGUILayout.Slider(_pieceDist, 100f, 300f, "Piece Distance From Center");
            EditorGUILayout.Slider(_centerRange, 0.01f, 0.3f, "Center Range");

            EditorGUILayout.Space(8f);
            EditorGUILayout.Slider(_appearanceDuration, 0.1f, 1f, "Appearance Duration");
            EditorGUILayout.Slider(_disppearanceDuration, 0.1f, 1f, "Disappearance Duration");

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.Space(8f);
                ColorLabelField("Objects", LightYellow);
                EditorGUILayout.PropertyField(_pieceSample);
                EditorGUILayout.PropertyField(_arrow);
            }

            EditorGUILayout.Space(12f);
            ColorLabelField("Animation Options", LightCyan);
            EditorGUILayout.PropertyField(_appearanceType);

            EditorGUI.BeginDisabledGroup(CheckAppearanceTypeIsProgressive(_appearanceType.enumValueIndex));
            EditorGUILayout.PropertyField(_appearanceEasing);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(6f);
            EditorGUILayout.PropertyField(_mainType);

            EditorGUILayout.Space(6f);
            EditorGUILayout.PropertyField(_disappearanceType);

            EditorGUI.BeginDisabledGroup(CheckAppearanceTypeIsProgressive(_disappearanceType.enumValueIndex));
            EditorGUILayout.PropertyField(_disappearanceEasing);
            EditorGUI.EndDisabledGroup();


            serializedObject.ApplyModifiedProperties();
        }

        private bool CheckAppearanceTypeIsProgressive(int enumValue)
        {
            return enumValue == 0 || enumValue >= (int)RadialMenu.AppearanceType.Progressive;
        }

        private static void ColorLabelField(string text, in Color color)
        {
            using (new LabelColorScope(color))
                EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        private class LabelColorScope : GUI.Scope
        {
            private readonly Color oldColor;
            public LabelColorScope(in Color color)
            {
                oldColor = GUI.color;
                GUI.color = color;
            }

            protected override void CloseScope()
            {
                GUI.color = oldColor;
            }
        }
    }
}

#endif