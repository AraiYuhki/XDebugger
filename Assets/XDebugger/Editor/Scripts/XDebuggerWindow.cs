using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Xeon.XDebugger.Console;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Profiler;
using Xeon.XDebugger.Common;
using Xeon.XDebugger.Editor.Model;
using Xeon.XDebugger.Model;
using System.Threading.Tasks;

using UnityEditor.IMGUI.Controls;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.TerrainTools;

namespace Xeon.XDebugger.Editor
{
    public class XDebuggerWindow : EditorWindow
    {
        [MenuItem("Window/XDebugger")]
        public static void Open() => GetWindow<XDebuggerWindow>("XDebugger");

        private static GUIStyle groupBoxStyle;

        private XDebugger instance;
        private int selectedTabIndex = 0;
        private Vector2 scrollPosition;
        private IPageModel currentPageModel;

        private List<IPageModel> pageStack = new List<IPageModel>();

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("XDebugger window only works in Play Mode.", MessageType.Warning);
                return;
            }

            instance ??= XDebugger.Instance;
            if (instance == null)
            {
                EditorGUILayout.HelpBox("XDebugger instance not found.", MessageType.Error);
                Repaint();
                return;
            }

            var tabList = instance.TabList.Where(tab => tab.Content is not IProfilerPage and not IConsolePage and not SystemInfoPage).ToList();
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabList.Select(tab => tab.Title).ToArray());
            var content = tabList[selectedTabIndex].Content;
            using var scope = new EditorGUILayout.ScrollViewScope(scrollPosition);
            if (content is GlobalMenuTabPage globalPage)
            {
                var pageModel = globalPage.GetPageModel();
                pageModel.Initialize(instance.UIFactory);
                foreach (var model in pageModel.ModelList)
                    DrawModel(model);
            }
            else
            {

                currentPageModel ??= XDebugger.GetOrCreateInitialPage();
                currentPageModel.Initialize(instance.UIFactory);

                foreach (var model in currentPageModel.ModelList)
                    DrawModel(model);
            }

            scrollPosition = scope.scrollPosition;
        }

        private void DrawModel(ControlModelBase model)
        {
            switch (model)
            {
                case HorizontalGroupModel horizontal:
                    DrawHorizontalGroup(horizontal);
                    break;
                case VerticalGroupModel vertical:
                    DrawVerticalGroup(vertical);
                    break;
                case FoldingGroupModel folding:
                    DrawFoldingGroup(folding);
                    break;
                case DisableGroupModel disable:
                    DrawDisableGroup(disable);
                    break;
                case LabelModel label:
                    DrawLabel(label);
                    break;
                case PageLinkActionModel pageLink:
                    DrawPageLinkButton(pageLink);
                    break;
                case ActionModel action:
                    DrawButton(action);
                    break;
                case StringModel text:
                    DrawText(text);
                    break;
                case IntSliderModel intSlider:
                    DrawIntSlider(intSlider);
                    break;
                case FloatSliderModel floatSlider:
                    DrawFloatSlider(floatSlider);
                    break;
                case NumberModel number:
                    DrawNumber(number);
                    break;
                case BoolModel boolean:
                    DrawBool(boolean);
                    break;
                case IEnumDropdownModel enumDropdown:
                    DrawEnumDropdown(enumDropdown);
                    break;
                case IDropdownModel dropdown:
                    DrawDropDown(dropdown);
                    break;
                default:
                    EditorGUILayout.LabelField($"Unknown model type: {model.GetType().Name}");
                    break;
            }
        }

        #region Draw Groups

        private static GUIStyle GroupBoxStyle => groupBoxStyle ??= new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(8, 8, 6, 6)
        };

        private static void DrawGroupHeader(string title)
        {
            if (!string.IsNullOrEmpty(title))
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        private void DrawHorizontalGroup(HorizontalGroupModel groupModel)
        {
            using var scope = new EditorGUILayout.HorizontalScope(GroupBoxStyle);
            DrawGroupHeader(groupModel.Title);
            foreach (var model in groupModel.Children)
            {
                DrawModel(model);
            }
        }

        private void DrawVerticalGroup(VerticalGroupModel groupModel)
        {
            using var scope = new EditorGUILayout.VerticalScope(GroupBoxStyle);
            DrawGroupHeader(groupModel.Title);
            foreach (var model in groupModel.Children)
            {
                DrawModel(model);
            }
        }

        private void DrawFoldingGroup(FoldingGroupModel groupModel)
        {
            using var scope = new EditorGUILayout.VerticalScope(GroupBoxStyle);
            var isFoldout = !groupModel.IsFolding;
            var newIsFoldout = EditorGUILayout.Foldout(isFoldout, groupModel.Title, true);
            if (newIsFoldout != isFoldout)
            {
                groupModel.IsFolding = !newIsFoldout;
            }
            if (groupModel.IsFolding)
                return;
            using (new EditorGUI.IndentLevelScope())
            {
                foreach (var model in groupModel.Children)
                {
                    DrawModel(model);
                }
            }
        }

        private void DrawDisableGroup(DisableGroupModel groupModel)
        {
            using var scope = new EditorGUI.DisabledGroupScope(groupModel.IsDisabled);
            using var boxScope = new EditorGUILayout.VerticalScope(GroupBoxStyle);
            DrawGroupHeader(groupModel.Title);
            foreach (var model in groupModel.Children)
            {
                DrawModel(model);
            }
        }
        #endregion

        #region Draw Controls
        private void DrawLabel(LabelModel model)
        {
            EditorGUILayout.LabelField(model.Title);
        }

        private void DrawButton(ActionModel model)
        {
            var rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.IndentedRect(rect);
            if (GUI.Button(rect, model.Title))
                model.ExecuteMethod();
        }

        private void DrawPageLinkButton(PageLinkActionModel model)
        {
            var rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.IndentedRect(rect);
            if (GUI.Button(rect, model.Title))
            {
                pageStack.Add(currentPageModel);
                currentPageModel = model.PageModel;
                currentPageModel.Initialize(instance.UIFactory);
                Repaint();
            }
        }

        private void DrawText(StringModel model)
        {
            EditorGUI.BeginChangeCheck();
            var text = EditorGUILayout.TextField(model.Title, model.Text);
            if (EditorGUI.EndChangeCheck())
                model.SetText(text, true);
        }

        private void DrawIntSlider(IntSliderModel model)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.IntSlider(model.Title, model.Value, model.Min, model.Max);
            if (EditorGUI.EndChangeCheck())
                model.SetValue(value, true);
        }

        private void DrawFloatSlider(FloatSliderModel model)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.Slider(model.Title, model.Value, model.Min, model.Max);
            if (EditorGUI.EndChangeCheck())
                model.SetValue(value, true);
        }

        private void DrawNumber(NumberModel model)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.FloatField(model.Title, model.Value);
            if (EditorGUI.EndChangeCheck())
                model.SetValue(value, true);
        }

        private void DrawBool(BoolModel model)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.Toggle(model.Title, model.Value);
            if (EditorGUI.EndChangeCheck())
                model.SetValue(value, true);
        }

        private void DrawEnumDropdown(IEnumDropdownModel model)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.EnumPopup(model.Title, model.Value);
            if (EditorGUI.EndChangeCheck())
                model.SetValue(value, true);
        }

        private void DrawDropDown(IDropdownModel model)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.Popup(model.Title, model.SelectedIndex, model.Labels.ToArray());
            if (EditorGUI.EndChangeCheck())
                model.SetSelectedIndex(value, true);
        }
        #endregion
    }
}
