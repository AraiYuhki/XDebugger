using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xeon.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Xeon.XGraph.Manager
{
    [Serializable]
    public class MarkerManager
    {
        public delegate float ValueToHeightDelegate(float value, out bool inRange);
        
        [SerializeField] private Transform parent;
        [SerializeField] private BarGraphMarker markerPrefab;
        [SerializeField, HideInInspector] private List<BarGraphMarker> markers = new();
        [SerializeField] private List<BarGraphMarkerData> markerDataList = new();

        private ValueToHeightDelegate valueToHeight;

        public void Initialize(ValueToHeightDelegate valueToHeight)
        {
            this.valueToHeight = valueToHeight;
        }

        public void SetMarkers(List<BarGraphMarkerData> newData)
        {
            if (ReferenceEquals(markerDataList, newData))
                return;
            
            markerDataList = newData;
            RefreshMarkers();
        }
        
        public void AddMarker(string label, float value)
        {
            markerDataList.Add(new BarGraphMarkerData(label, value));
            RefreshMarkers();
        }

        public void RemoveMarker(string label, float value)
        {
            var target = markerDataList.FirstOrDefault(data => data.Label == label && Mathf.Approximately(data.Value, value));
            if (target != null)
                markerDataList.Remove(target);
            RefreshMarkers();
        }

        public void RemoveMarker(int index)
        {
            markerDataList.RemoveAt(index);
            RefreshMarkers();
        }
        
        public void UpdateMarkers()
        {
            foreach (var marker in markers)
            {
                if (marker == null || marker.Data == null)
                    continue;
                var position = marker.transform.localPosition;
                position.y = valueToHeight(marker.Data.Value, out var inRange);
                marker.transform.localPosition = position;
                marker.gameObject.SetActive(inRange);
            }
        }

        public void ClearMarkers()
        {
            foreach (var marker in markers)
            {
                if (marker == null)
                    continue;
                if (Application.isPlaying)
                    GameObject.Destroy(marker.gameObject);
                else
                    GameObject.DestroyImmediate(marker.gameObject);
            }

            markers.Clear();
        }

        public void RefreshMarkers()
        {
            ClearMarkers();
            if (markerPrefab == null)
                return;

            bool hasDelegate = valueToHeight != null;

            foreach (var data in markerDataList)
            {
                var marker = GameObject.Instantiate(markerPrefab, parent);
                marker.Data = data;

                var position = marker.transform.localPosition;
                bool inRange = true;
                if (hasDelegate)
                {
                    position.y = valueToHeight(data.Value, out inRange);
                }
                else
                {
                    position.y = 0f;
                    inRange = true;
                }
                marker.transform.localPosition = position;
                marker.gameObject.SetActive(inRange);
                markers.Add(marker);
            }
        }
        
#if UNITY_EDITOR
        [UnityEditor.CustomPropertyDrawer(typeof(MarkerManager))]
        private class MarkerManagerEditor : UnityEditor.PropertyDrawer
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                // 折りたたみ時は1行のみ
                if (!property.isExpanded)
                    return EditorGUIUtility.singleLineHeight;

                var parentProp = property.FindPropertyRelative("parent");
                var prefabProp = property.FindPropertyRelative("markerPrefab");
                var listProp = property.FindPropertyRelative("markerDataList");

                float h = 0f;
                h += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Foldoutヘッダ
                h += EditorGUI.GetPropertyHeight(parentProp) + EditorGUIUtility.standardVerticalSpacing;
                h += EditorGUI.GetPropertyHeight(prefabProp) + EditorGUIUtility.standardVerticalSpacing;
                h += EditorGUI.GetPropertyHeight(listProp) + EditorGUIUtility.standardVerticalSpacing;
                h += EditorGUIUtility.singleLineHeight; // buttons
                return h;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                float line = EditorGUIUtility.singleLineHeight;
                float pad = EditorGUIUtility.standardVerticalSpacing;

                // Foldoutヘッダ
                Rect headerRect = new Rect(position.x, position.y, position.width, line);
                property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

                if (!property.isExpanded)
                {
                    EditorGUI.EndProperty();
                    return;
                }

                float y = headerRect.y + line + pad;

                var parentProp = property.FindPropertyRelative("parent");
                var prefabProp = property.FindPropertyRelative("markerPrefab");
                var listProp = property.FindPropertyRelative("markerDataList");

                // parent
                Rect rParent = new Rect(position.x, y, position.width, line);
                EditorGUI.PropertyField(rParent, parentProp);
                y += line + pad;

                // prefab
                Rect rPrefab = new Rect(position.x, y, position.width, line);
                EditorGUI.PropertyField(rPrefab, prefabProp);
                y += line + pad;

                // marker data list
                float listHeight = EditorGUI.GetPropertyHeight(listProp);
                Rect rList = new Rect(position.x, y, position.width, listHeight);
                EditorGUI.PropertyField(rList, listProp, true);
                y += listHeight + pad;

                // buttons
                Rect rButtons = new Rect(position.x, y, position.width, line);
                float half = (rButtons.width - 4f) * 0.5f;
                Rect rCreate = new Rect(rButtons.x, rButtons.y, half, line);
                Rect rClear = new Rect(rButtons.x + half + 4f, rButtons.y, half, line);

                var owner = property.serializedObject.targetObject;
                var markerManager = fieldInfo.GetValue(owner) as MarkerManager;

                using (new EditorGUI.DisabledScope(markerManager == null))
                {
                    if (GUI.Button(rCreate, "マーカー生成 / 更新"))
                    {
                        markerManager?.RefreshMarkers();
                        EditorUtility.SetDirty(owner);
                    }
                    if (GUI.Button(rClear, "全マーカー削除"))
                    {
                        markerManager?.ClearMarkers();
                        EditorUtility.SetDirty(owner);
                    }
                }

                EditorGUI.EndProperty();
            }
        }
#endif
    }
}
