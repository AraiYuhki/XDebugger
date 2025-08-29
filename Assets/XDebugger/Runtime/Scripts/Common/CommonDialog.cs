using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.XDebugger.Dialog
{
    /// <summary>共通ダイアログの表示とボタン入力取得を行うコンポーネント。</summary>
    public class CommonDialog : MonoBehaviour
    {
        /// <summary>開アニメーションのステートID。</summary>
        private static readonly int OpenId = Animator.StringToHash("Open");
        /// <summary>閉アニメーションのステートID。</summary>
        private static readonly int CloseId = Animator.StringToHash("Close");

        /// <summary>タイトルのTMPテキスト。</summary>
        [SerializeField] private TMP_Text title;
        
        /// <summary>メッセージのTMPテキスト。</summary>
        [SerializeField] private TMP_Text message;
        
        /// <summary>開閉演出用のAnimator。</summary>
        [SerializeField] private Animator animator;
        
        /// <summary>生成ボタンの親コンテナ。</summary>
        [SerializeField] private Transform buttonContainer;
        
        /// <summary>ボタン生成に使うテンプレート。</summary>
        [SerializeField] private Button buttonTemplate;

        /// <summary>生成済みボタンの参照リスト。</summary>
        private List<Button> buttons = new();

        /// <summary>OKのみのダイアログを表示（返り値なし）。</summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="token">キャンセルトークン</param>
        public async UniTask ShowAsync(string title, string message, CancellationToken token = default)
        {
            Initialize(title, message);

            var completionSource = new UniTaskCompletionSource();
            using (token.Register(() => completionSource.TrySetCanceled()))
            {
                CreateButton("OK", () => completionSource.TrySetResult());

                await OpenAsync(token);
                await completionSource.Task;
                await CloseAsync(token);
            }
        }

        /// <summary>Yes/No の二択ダイアログを表示。</summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="token">キャンセルトークン</param>
        /// <returns>Yes なら true、No なら false。</returns>
        public async UniTask<bool> ShowYesNoAsync(string title, string message, CancellationToken token = default)
        {
            Initialize(title, message);

            var completionSource = new UniTaskCompletionSource<bool>();
            using (token.Register(() => completionSource.TrySetCanceled()))
            {
                CreateButton("Yes", () => completionSource.TrySetResult(true));
                CreateButton("No", () => completionSource.TrySetResult(false));

                await OpenAsync(token);
                var result = await completionSource.Task;
                await CloseAsync(token);
                return result;
            }
        }

        /// <summary>任意ラベルの複数ボタンを表示。</summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="token">キャンセルトークン</param>
        /// <param name="labels">ボタンラベル配列</param>
        /// <returns>押下ボタンのインデックス（0始まり）。</returns>
        public async UniTask<int> ShowAsync(string title, string message, CancellationToken token,
            params string[] labels)
        {
            Initialize(title, message);

            var completionSource = new UniTaskCompletionSource<int>();
            using (token.Register(() => completionSource.TrySetCanceled()))
            {
                for (var index = 0; index < labels.Length; index++)
                {
                    var label = labels[index];
                    var tmp = index;
                    CreateButton(label, () => completionSource.TrySetResult(tmp));
                }

                await OpenAsync(token);
                var result = await completionSource.Task;
                await CloseAsync(token);
                return result;
            }
        }

        /// <summary>
        /// タイトル/メッセージ設定と既存ボタンの破棄を行う。
        /// </summary>
        /// <param name="title">タイトル文字列</param>
        /// <param name="message">本文</param>
        private void Initialize(string title, string message)
        {
            this.title.text = title;
            this.message.text = message;
            foreach (var button in buttons)
                Destroy(button.gameObject);
            buttons.Clear();
        }

        /// <summary>
        /// テンプレートからボタンを生成しクリック動作を登録。
        /// </summary>
        /// <param name="label">ボタンに表示する文字列</param>
        /// <param name="onClick">クリック時に呼ばれるイベント</param>
        private void CreateButton(string label, Action onClick)
        {
            var button = Instantiate(buttonTemplate, buttonContainer);
            var text = button.GetComponentInChildren<TMP_Text>();
            text.text = label;
            button.onClick.AddListener(() => onClick?.Invoke());
            button.gameObject.SetActive(true);
            buttons.Add(button);
        }

        /// <summary>
        /// 開くアニメーションを再生し完了まで待機
        /// </summary>
        /// <param name="token">キャンセルトークン</param>
        private async UniTask OpenAsync(CancellationToken token = default)
        {
            animator.Play(OpenId, 0, 0f);
            await UniTask.Yield(cancellationToken: token);
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f,
                cancellationToken: token);
        }

        /// <summary>
        /// 閉じるアニメーションを再生し完了まで待機
        /// </summary>
        private async UniTask CloseAsync(CancellationToken token = default)
        {
            animator.Play(CloseId, 0, 0f);
            await UniTask.Yield(0, token);
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f,
                cancellationToken: token);
        }
    }
}
