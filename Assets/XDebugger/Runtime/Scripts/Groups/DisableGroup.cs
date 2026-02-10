using UnityEngine;

namespace Xeon.XDebugger
{
    /// <summary>
    /// 無効化パネルを重ねて子コントロールの操作を制御できるグループ
    /// </summary>
    public class DisableGroup : ContentGroup
    {
        [SerializeField]
        private GameObject disablePanel;

        [SerializeField]
        private bool isDisable = false;

        /// <summary>
        /// グループの無効化状態を設定する
        /// </summary>
        /// <param name="disabled">trueで無効化パネルを表示する</param>
        public void SetDisable(bool disabled)
        {
            isDisable = disabled;
            disablePanel.SetActive(disabled);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (disablePanel == null)
                return;
            disablePanel.SetActive(isDisable);
        }
#endif
    }
}
