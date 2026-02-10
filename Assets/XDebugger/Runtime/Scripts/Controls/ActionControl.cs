using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger.Control
{
    /// <summary>
    /// ボタン押下でアクションを実行するコントロール
    /// </summary>
    public class ActionControl : ControlBase
    {
        [SerializeField]
        protected Button button;

        protected ActionModel model;

        /// <summary>
        /// アクションモデルを設定し、ボタンにコールバックを登録する
        /// </summary>
        /// <param name="model">アクションのデータモデル</param>
        public void Setup(ActionModel model)
        {
            Setup(model.Title);
            
            this.model = model;

            button.onClick.RemoveListener(model.ExecuteMethod);
            button.onClick.AddListener(model.ExecuteMethod);
        }

        private void OnDestroy()
        {
            if (button != null && model != null)
                button.onClick.RemoveListener(model.ExecuteMethod);
        }
    }
}
