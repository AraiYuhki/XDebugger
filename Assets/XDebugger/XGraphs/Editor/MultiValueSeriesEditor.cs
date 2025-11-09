using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Xeon.XGraph.Model.Editor
{
    [CustomPropertyDrawer(typeof(MultiValueSeries))]
    public class MultiValueSeriesEditor : PropertyDrawer
    {
        protected const string SeriesPropertyName = "series";
        protected const string SeriesNamePropertyName = "name";
        protected const string SeriesColorPropertyName = "color";
        protected const string SeriesTestDataPropertyName = "testData";

        protected static readonly GUIContent LegendCountContent = new("凡例数", "凡例の数を指定します。凡例数と列数は同じになります。");
        protected static readonly GUIContent LegendNameRowContent = new("凡例名");
        protected static readonly GUIContent LegendColorRowContent = new("凡例カラー");
        protected static readonly GUIContent TestDataHeaderContent = new("テストデータ", "各凡例の値を行ごとに編集できます。ドラッグで順序を入れ替えられます。");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded)
                return height;

            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var seriesProperty = property.FindPropertyRelative(SeriesPropertyName);

            height += spacing + EditorGUIUtility.singleLineHeight; // Legend count field

            if (seriesProperty != null && seriesProperty.arraySize > 0)
            {
                height += spacing + EditorGUIUtility.singleLineHeight; // Legend names
                height += spacing + EditorGUIUtility.singleLineHeight; // Legend colors

                var reorderableList = CreateValueList(property, seriesProperty);
                height += spacing + reorderableList.GetHeight();
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var seriesProperty = property.FindPropertyRelative(SeriesPropertyName);

            var foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            var y = foldoutRect.y + lineHeight + spacing;

            if (seriesProperty == null)
            {
                EditorGUI.indentLevel--;
                EditorGUI.EndProperty();
                return;
            }

            var legendRect = EditorGUI.IndentedRect(new Rect(position.x, y, position.width, lineHeight));
            EditorGUI.BeginChangeCheck();
            var newLegendCount = EditorGUI.DelayedIntField(legendRect, LegendCountContent, seriesProperty.arraySize);
            if (EditorGUI.EndChangeCheck())
            {
                ResizeSeries(seriesProperty, Mathf.Max(0, newLegendCount));
            }
            y += lineHeight;

            if (seriesProperty.arraySize > 0)
            {
                EnsureConsistentSeries(seriesProperty);

                y += spacing;
                var legendNameRect = EditorGUI.IndentedRect(new Rect(position.x, y, position.width, lineHeight));
                DrawSeriesRow(legendNameRect, seriesProperty, SeriesNamePropertyName, LegendNameRowContent);
                y += lineHeight;

                y += spacing;
                var legendColorRect = EditorGUI.IndentedRect(new Rect(position.x, y, position.width, lineHeight));
                DrawSeriesRow(legendColorRect, seriesProperty, SeriesColorPropertyName, LegendColorRowContent);
                y += lineHeight;

                y += spacing;
                var reorderableList = CreateValueList(property, seriesProperty);
                var listHeight = reorderableList.GetHeight();
                var listRect = EditorGUI.IndentedRect(new Rect(position.x, y, position.width, listHeight));
                reorderableList.DoList(listRect);
                y += listHeight;
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        protected static void DrawSeriesRow(Rect rect, SerializedProperty seriesProperty, string childPropertyName, GUIContent label)
        {
            var fieldRect = EditorGUI.PrefixLabel(rect, label);
            var count = seriesProperty.arraySize;
            if (count <= 0)
                return;

            var columnWidth = fieldRect.width / count;
            for (var index = 0; index < count; index++)
            {
                var columnRect = new Rect(fieldRect.x + columnWidth * index, fieldRect.y, Mathf.Max(0f, columnWidth - 2f), EditorGUIUtility.singleLineHeight);
                var elementProperty = seriesProperty.GetArrayElementAtIndex(index).FindPropertyRelative(childPropertyName);
                if (elementProperty != null)
                {
                    EditorGUI.PropertyField(columnRect, elementProperty, GUIContent.none);
                }
            }
        }

        protected static ReorderableList CreateValueList(SerializedProperty containerProperty, SerializedProperty seriesProperty)
        {
            var firstSeries = seriesProperty.GetArrayElementAtIndex(0);
            var primaryValues = firstSeries.FindPropertyRelative(SeriesTestDataPropertyName);
            var list = new ReorderableList(containerProperty.serializedObject, primaryValues, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, TestDataHeaderContent),
                elementHeight = EditorGUIUtility.singleLineHeight + 4f
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                rect.y += 2f;

                var columnWidth = rect.width / Mathf.Max(1, seriesProperty.arraySize);
                for (var i = 0; i < seriesProperty.arraySize; i++)
                {
                    var valuesProperty = seriesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(SeriesTestDataPropertyName);
                    if (valuesProperty == null || valuesProperty.arraySize <= index)
                        continue;

                    var columnRect = new Rect(rect.x + columnWidth * i, rect.y, Mathf.Max(0f, columnWidth - 2f), EditorGUIUtility.singleLineHeight);
                    var valueProperty = valuesProperty.GetArrayElementAtIndex(index);
                    valueProperty.floatValue = EditorGUI.FloatField(columnRect, GUIContent.none, valueProperty.floatValue);
                }
            };

            list.onAddCallback = reorderableList =>
            {
                for (var i = 0; i < seriesProperty.arraySize; i++)
                {
                    var valuesProperty = seriesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(SeriesTestDataPropertyName);
                    var newIndex = valuesProperty.arraySize;
                    valuesProperty.InsertArrayElementAtIndex(newIndex);
                    valuesProperty.GetArrayElementAtIndex(newIndex).floatValue = 0f;
                }
            };

            list.onRemoveCallback = reorderableList =>
            {
                var index = reorderableList.index >= 0 && reorderableList.index < reorderableList.count
                    ? reorderableList.index
                    : reorderableList.count - 1;

                if (index < 0)
                    return;

                for (var i = 0; i < seriesProperty.arraySize; i++)
                {
                    var valuesProperty = seriesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(SeriesTestDataPropertyName);
                    valuesProperty.DeleteArrayElementAtIndex(index);
                }
            };

            list.onReorderCallbackWithDetails = (reorderableList, oldIndex, newIndex) =>
            {
                if (oldIndex == newIndex)
                    return;

                for (var i = 0; i < seriesProperty.arraySize; i++)
                {
                    var valuesProperty = seriesProperty.GetArrayElementAtIndex(i).FindPropertyRelative(SeriesTestDataPropertyName);
                    valuesProperty.MoveArrayElement(oldIndex, newIndex);
                }
            };

            return list;
        }

        protected static void ResizeSeries(SerializedProperty seriesProperty, int newCount)
        {
            if (seriesProperty.arraySize == newCount)
                return;

            var previousCount = seriesProperty.arraySize;
            seriesProperty.arraySize = newCount;

            if (newCount <= 0)
                return;

            var referenceRowCount = 0;
            if (previousCount > 0)
            {
                referenceRowCount = seriesProperty.GetArrayElementAtIndex(0)
                    .FindPropertyRelative(SeriesTestDataPropertyName)
                    .arraySize;
            }

            for (var i = previousCount; i < newCount; i++)
            {
                var seriesElement = seriesProperty.GetArrayElementAtIndex(i);
                if (seriesElement == null)
                    continue;

                var nameProperty = seriesElement.FindPropertyRelative(SeriesNamePropertyName);
                if (nameProperty != null && string.IsNullOrEmpty(nameProperty.stringValue))
                {
                    nameProperty.stringValue = $"Legend {i + 1}";
                }

                var valuesProperty = seriesElement.FindPropertyRelative(SeriesTestDataPropertyName);
                if (valuesProperty != null)
                {
                    valuesProperty.arraySize = referenceRowCount;
                }
            }
        }

        protected static void EnsureConsistentSeries(SerializedProperty seriesProperty)
        {
            if (seriesProperty.arraySize <= 0)
                return;

            var targetCount = seriesProperty.GetArrayElementAtIndex(0)
                .FindPropertyRelative(SeriesTestDataPropertyName)
                .arraySize;

            for (var i = 1; i < seriesProperty.arraySize; i++)
            {
                var seriesElement = seriesProperty.GetArrayElementAtIndex(i);
                var valuesProperty = seriesElement.FindPropertyRelative(SeriesTestDataPropertyName);
                if (valuesProperty != null && valuesProperty.arraySize != targetCount)
                {
                    valuesProperty.arraySize = targetCount;
                }
            }
        }
    }
}
