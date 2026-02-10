using System.Collections.Generic;
using UnityEngine;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger
{

    /// <summary>
    /// 子コントロールを格納するグループの基底クラス
    /// </summary>
    public class ContentGroup : ControlBase
    {
        [SerializeField]
        protected Transform content;

        protected IGroupModel model;
        protected ContentGroup parent;
        protected List<ControlBase> children;

        /// <summary>
        /// 子コントロールの配置先となるTransformを取得する
        /// </summary>
        /// <returns>コンテンツ領域のTransform</returns>
        public Transform GetContent() => content;

        /// <summary>
        /// グループモデルと親グループを設定する
        /// </summary>
        /// <param name="model">グループのデータモデル</param>
        /// <param name="parent">親のContentGroup</param>
        public void Setup(IGroupModel model, ContentGroup parent)
        {
            Setup(model.Title);
            title.gameObject.SetActive(!string.IsNullOrEmpty(model.Title));
            this.model = model;
            this.parent = parent;
        }

        /// <summary>
        /// 全ての子コントロールを再描画する
        /// </summary>
        public override void Refresh()
        {
            if (children == null) return;

            foreach (var control in children)
                control.Refresh();
        }
    }
}
