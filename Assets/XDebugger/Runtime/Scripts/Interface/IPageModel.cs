using System;
using System.Collections.ObjectModel;
using UnityEngine;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Model
{
    /// <summary>
    /// ページモデルの標準インターフェイス
    /// </summary>
    public interface IPageModel
    {
        string Title { get; }
        Transform Content { get; set; }
        IGroupModel CurrentGroup { get; }
        ReadOnlyCollection<ControlModelBase> ModelList { get; }
        ControlModelBase this[int index] { get; }
        int Count { get; }

        void Initialize(IUIFactory uiFactory);
        void Update();
        void SetGroup(IGroupModel model);
        MonoBehaviour GetControl();
        void OpenPage(Transform parent, IPageModel pageModel, IUIFactory uiFactory);
        void Close(Action onClose = null);
        void Show(bool isRefresh = false);
        void Hide(Action onHidden = null);
        void Refresh(bool doRecreate = false);
    }
}
