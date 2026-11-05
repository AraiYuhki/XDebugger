using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XDebugger.Editor.Scripts
{
    /// <summary>
    /// スクリプトインポート時に文字コードをUTF-8へ自動変換するエディタ用ポストプロセッサ。
    /// 日本語コードの自動判定と変換を行い、Unityでの文字化けを防止します。
    /// </summary>
    public class ScriptImportPostProcessor : AssetPostprocessor
    {
        /// <summary>
        /// アセットインポート・移動・削除時に呼ばれるコールバック。
        /// .csファイルの文字コードをUTF-8へ変換します。
        /// </summary>
        /// <param name="importedAssets">インポートされたアセット</param>
        /// <param name="deletedAssets">削除されたアセット</param>
        /// <param name="movedAssets">移動されたアセット</param>
        /// <param name="movedFromAssetPaths">移動元パス</param>
        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // 対象となる.csファイルを抽出（Assets配下のプロジェクト所有ファイルのみ）
            var targets = importedAssets.Concat(movedAssets).Where(IsConvertTarget).Distinct();
            foreach (var path in targets)
            {
                var encoding = EncodeHelper.GetJpEncoding(path); // 文字コード判定
                if (encoding == null)
                {
                    Debug.LogError("Failed to get encoding");
                    continue;
                }
                if (encoding.EncodingName == Encoding.UTF8.EncodingName)
                {
                    continue; // 既にUTF-8なら何もしない
                }
                try
                {
                    // 元の文字コードでファイル内容を読み込む
                    string data;
                    using (var sr = new StreamReader(path, encoding))
                    {
                        data = sr.ReadToEnd();
                    }
                    // UTF-8で上書き保存
                    using (var sw = new StreamWriter(path, false, Encoding.UTF8))
                    {
                        sw.Write(data);
                    }
                    Debug.Log($"{path} is encoded {encoding.EncodingName} to UTF8");
                }
                catch (System.Exception e)
                {
                    // 変換失敗でインポート処理全体を止めない
                    Debug.LogWarning($"Failed to convert {path} to UTF8: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 変換対象の.csファイルかどうか。
        /// Packages配下（レジストリ/Git/ローカル問わず）やPackageCacheはパッケージ所有のファイルであり、
        /// 文字コードの誤判定による書き換え（文字化け）を避けるため対象外とする。
        /// </summary>
        private static bool IsConvertTarget(string path)
        {
            if (Path.GetExtension(path) != ".cs") return false;
            if (!path.StartsWith("Assets/", System.StringComparison.Ordinal)) return false;
            // Assets配下でもパッケージ管理下のファイルは対象外
            if (UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path) != null) return false;
            return true;
        }
    }
}
