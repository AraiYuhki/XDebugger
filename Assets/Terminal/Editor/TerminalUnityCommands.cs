using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using XDebugger.Terminal;

namespace XDebugger.TerminalEditor
{
    public sealed class TerminalUnityCommands
    {
        [TerminalCommand("hierarchy", Description = "現在のヒエラルキーをツリー表示します")]
        private TerminalCommandResult ShowHierarchy(TerminalCommandContext context, string[] args)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return TerminalCommandResult.Fail("アクティブなシーンが存在しません");
            }

            var builder = new StringBuilder();
            foreach (var root in scene.GetRootGameObjects())
            {
                AppendGameObjectTree(root, builder, 0);
            }

            return TerminalCommandResult.Ok(builder.ToString());
        }

        [TerminalCommand("set-transform", Description = "Transformを変更します (--pos x y z --rot x y z --scale x y z)")]
        private TerminalCommandResult SetTransform(TerminalCommandContext context, string[] args)
        {
            if (args.Length < 1)
            {
                return TerminalCommandResult.Fail("set-transform: InstanceIDを指定してください");
            }

            if (!TryGetGameObject(args[0], out var gameObject, out var error))
            {
                return TerminalCommandResult.Fail(error);
            }

            var transform = gameObject.transform;
            if (!TryParseTransformArgs(args.Skip(1).ToArray(), out var position, out var rotation, out var scale, out var parseError))
            {
                return TerminalCommandResult.Fail(parseError);
            }

            Undo.RecordObject(transform, "Terminal Set Transform");
            if (position.HasValue)
            {
                transform.position = position.Value;
            }

            if (rotation.HasValue)
            {
                transform.eulerAngles = rotation.Value;
            }

            if (scale.HasValue)
            {
                transform.localScale = scale.Value;
            }

            EditorSceneManager.MarkSceneDirty(transform.gameObject.scene);
            return TerminalCommandResult.Ok($"Transform updated: {gameObject.name} ({gameObject.GetInstanceID()})");
        }

        [TerminalCommand("add-component", Description = "コンポーネントを追加します")]
        private TerminalCommandResult AddComponent(TerminalCommandContext context, string[] args)
        {
            if (args.Length < 2)
            {
                return TerminalCommandResult.Fail("add-component: InstanceIDとコンポーネント型名を指定してください");
            }

            if (!TryGetGameObject(args[0], out var gameObject, out var error))
            {
                return TerminalCommandResult.Fail(error);
            }

            if (!TryResolveComponentType(args[1], out var type, out var typeError))
            {
                return TerminalCommandResult.Fail(typeError);
            }

            Undo.AddComponent(gameObject, type);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            return TerminalCommandResult.Ok($"Component added: {type.Name} -> {gameObject.name}");
        }

        [TerminalCommand("remove-component", Description = "コンポーネントを削除します")]
        private TerminalCommandResult RemoveComponent(TerminalCommandContext context, string[] args)
        {
            if (args.Length < 2)
            {
                return TerminalCommandResult.Fail("remove-component: InstanceIDとコンポーネント型名を指定してください");
            }

            if (!TryGetGameObject(args[0], out var gameObject, out var error))
            {
                return TerminalCommandResult.Fail(error);
            }

            if (!TryResolveComponentType(args[1], out var type, out var typeError))
            {
                return TerminalCommandResult.Fail(typeError);
            }

            var component = gameObject.GetComponent(type);
            if (component == null)
            {
                return TerminalCommandResult.Fail($"remove-component: {type.Name} が見つかりません");
            }

            Undo.DestroyObjectImmediate(component);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            return TerminalCommandResult.Ok($"Component removed: {type.Name} -> {gameObject.name}");
        }

        [TerminalCommand("list-components", Description = "コンポーネント一覧を表示します")]
        private TerminalCommandResult ListComponents(TerminalCommandContext context, string[] args)
        {
            if (args.Length < 1)
            {
                return TerminalCommandResult.Fail("list-components: InstanceIDを指定してください");
            }

            if (!TryGetGameObject(args[0], out var gameObject, out var error))
            {
                return TerminalCommandResult.Fail(error);
            }

            var builder = new StringBuilder();
            foreach (var component in gameObject.GetComponents<Component>())
            {
                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(component.GetType().Name);
                builder.Append(" (#");
                builder.Append(component.GetInstanceID());
                builder.Append(')');
            }

            return TerminalCommandResult.Ok(builder.ToString());
        }

        [TerminalCommand("set-component", Description = "コンポーネントのフィールド/プロパティを変更します")]
        private TerminalCommandResult SetComponentValue(TerminalCommandContext context, string[] args)
        {
            if (args.Length < 4)
            {
                return TerminalCommandResult.Fail("set-component: InstanceID 型名 メンバー名 値 を指定してください");
            }

            if (!TryGetGameObject(args[0], out var gameObject, out var error))
            {
                return TerminalCommandResult.Fail(error);
            }

            if (!TryResolveComponentType(args[1], out var type, out var typeError))
            {
                return TerminalCommandResult.Fail(typeError);
            }

            var memberName = args[2];
            var valueText = string.Join(" ", args.Skip(3));
            var component = gameObject.GetComponent(type);
            if (component == null)
            {
                return TerminalCommandResult.Fail($"set-component: {type.Name} が見つかりません");
            }

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var field = type.GetField(memberName, flags);
            if (field != null)
            {
                if (!TryConvertValue(field.FieldType, valueText, out var value, out var convertError))
                {
                    return TerminalCommandResult.Fail(convertError);
                }

                Undo.RecordObject(component, "Terminal Set Field");
                field.SetValue(component, value);
                EditorUtility.SetDirty(component);
                EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                return TerminalCommandResult.Ok($"Field updated: {type.Name}.{field.Name}");
            }

            var property = type.GetProperty(memberName, flags);
            if (property == null || !property.CanWrite)
            {
                return TerminalCommandResult.Fail($"set-component: 書き込み可能なメンバーが見つかりません: {memberName}");
            }

            if (!TryConvertValue(property.PropertyType, valueText, out var propertyValue, out var propertyError))
            {
                return TerminalCommandResult.Fail(propertyError);
            }

            Undo.RecordObject(component, "Terminal Set Property");
            property.SetValue(component, propertyValue);
            EditorUtility.SetDirty(component);
            EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
            return TerminalCommandResult.Ok($"Property updated: {type.Name}.{property.Name}");
        }

        [TerminalCommand("set-sibling-index", Description = "SiblingIndexを変更します")]
        private TerminalCommandResult SetSiblingIndex(TerminalCommandContext context, string[] args)
        {
            if (args.Length < 2)
            {
                return TerminalCommandResult.Fail("set-sibling-index: InstanceIDとIndexを指定してください");
            }

            if (!TryGetGameObject(args[0], out var gameObject, out var error))
            {
                return TerminalCommandResult.Fail(error);
            }

            if (!int.TryParse(args[1], out var index))
            {
                return TerminalCommandResult.Fail("set-sibling-index: Indexが不正です");
            }

            Undo.RecordObject(gameObject.transform, "Terminal Set Sibling Index");
            gameObject.transform.SetSiblingIndex(index);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            return TerminalCommandResult.Ok($"SiblingIndex updated: {gameObject.name} -> {index}");
        }

        [TerminalCommand("create-gameobject", Description = "GameObjectを作成します")]
        private TerminalCommandResult CreateGameObject(TerminalCommandContext context, string[] args)
        {
            if (args.Length < 1)
            {
                return TerminalCommandResult.Fail("create-gameobject: 名前を指定してください");
            }

            var name = args[0];
            var parent = args.Length > 1 && TryGetGameObject(args[1], out var parentObj, out _) ? parentObj.transform : null;
            var gameObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(gameObject, "Terminal Create GameObject");
            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }

            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            return TerminalCommandResult.Ok($"GameObject created: {gameObject.name} ({gameObject.GetInstanceID()})");
        }

        [TerminalCommand("instantiate-asset", Description = "AssetDatabaseからPrefabを生成します")]
        private TerminalCommandResult InstantiateAsset(TerminalCommandContext context, string[] args)
        {
            if (args.Length < 1)
            {
                return TerminalCommandResult.Fail("instantiate-asset: アセットパスを指定してください");
            }

            var assetPath = args[0];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                return TerminalCommandResult.Fail($"instantiate-asset: アセットが見つかりません: {assetPath}");
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                return TerminalCommandResult.Fail("instantiate-asset: Prefabの生成に失敗しました");
            }

            if (args.Length > 1 && TryGetGameObject(args[1], out var parent, out _))
            {
                instance.transform.SetParent(parent.transform, false);
            }

            Undo.RegisterCreatedObjectUndo(instance, "Terminal Instantiate Asset");
            EditorSceneManager.MarkSceneDirty(instance.scene);
            return TerminalCommandResult.Ok($"Asset instantiated: {instance.name} ({instance.GetInstanceID()})");
        }

        [TerminalCommand("instantiate-addressable", Description = "AddressablesからPrefabを生成します")]
        private async Task<TerminalCommandResult> InstantiateAddressable(TerminalCommandContext context, string[] args)
        {
            if (args.Length < 1)
            {
                return TerminalCommandResult.Fail("instantiate-addressable: キーを指定してください");
            }

            var key = args[0];
            var handle = Addressables.LoadAssetAsync<GameObject>(key);
            var prefab = await ToTask(handle);
            try
            {
                if (prefab == null)
                {
                    return TerminalCommandResult.Fail($"instantiate-addressable: アセットが見つかりません: {key}");
                }

                var instance = UnityEngine.Object.Instantiate(prefab);
                if (args.Length > 1 && TryGetGameObject(args[1], out var parent, out _))
                {
                    instance.transform.SetParent(parent.transform, false);
                }

                Undo.RegisterCreatedObjectUndo(instance, "Terminal Instantiate Addressable");
                EditorSceneManager.MarkSceneDirty(instance.scene);
                return TerminalCommandResult.Ok($"Addressable instantiated: {instance.name} ({instance.GetInstanceID()})");
            }
            finally
            {
                Addressables.Release(handle);
            }
        }

        private static void AppendGameObjectTree(GameObject gameObject, StringBuilder builder, int depth)
        {
            builder.Append(new string(' ', depth * 2));
            builder.Append(gameObject.name);
            builder.Append(" (#");
            builder.Append(gameObject.GetInstanceID());
            builder.Append(')');
            builder.Append('\n');

            foreach (Transform child in gameObject.transform)
            {
                AppendGameObjectTree(child.gameObject, builder, depth + 1);
            }
        }

        private static bool TryGetGameObject(string instanceIdText, out GameObject gameObject, out string error)
        {
            gameObject = null;
            error = string.Empty;
            if (!int.TryParse(instanceIdText, out var instanceId))
            {
                error = "InstanceIDが不正です";
                return false;
            }

            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj == null)
            {
                error = $"InstanceIDが見つかりません: {instanceId}";
                return false;
            }

            if (obj is GameObject foundGameObject)
            {
                gameObject = foundGameObject;
                return true;
            }

            if (obj is Component component)
            {
                gameObject = component.gameObject;
                return true;
            }

            error = "InstanceIDはGameObjectまたはComponentではありません";
            return false;
        }

        private static bool TryResolveComponentType(string typeName, out Type type, out string error)
        {
            type = null;
            error = string.Empty;

            var candidates = TypeCache.GetTypesDerivedFrom<Component>()
                .Where(candidate => string.Equals(candidate.Name, typeName, StringComparison.OrdinalIgnoreCase));

            type = candidates.FirstOrDefault();
            if (type != null)
            {
                return true;
            }

            error = $"コンポーネント型が見つかりません: {typeName}";
            return false;
        }

        private static bool TryParseTransformArgs(string[] args, out Vector3? position, out Vector3? rotation, out Vector3? scale, out string error)
        {
            position = null;
            rotation = null;
            scale = null;
            error = string.Empty;

            var index = 0;
            while (index < args.Length)
            {
                var token = args[index];
                if (token == "--pos" || token == "--rot" || token == "--scale")
                {
                    if (index + 3 >= args.Length)
                    {
                        error = $"{token}: 値が不足しています";
                        return false;
                    }

                    if (!TryParseVector3(args[index + 1], args[index + 2], args[index + 3], out var value))
                    {
                        error = $"{token}: Vector3の値が不正です";
                        return false;
                    }

                    if (token == "--pos")
                    {
                        position = value;
                    }
                    else if (token == "--rot")
                    {
                        rotation = value;
                    }
                    else
                    {
                        scale = value;
                    }

                    index += 4;
                    continue;
                }

                error = $"不明な引数: {token}";
                return false;
            }

            if (!position.HasValue && !rotation.HasValue && !scale.HasValue)
            {
                error = "変更対象が指定されていません";
                return false;
            }

            return true;
        }

        private static bool TryConvertValue(Type type, string rawValue, out object value, out string error)
        {
            value = null;
            error = string.Empty;

            if (type == typeof(string))
            {
                value = rawValue;
                return true;
            }

            if (type.IsEnum)
            {
                if (Enum.TryParse(type, rawValue, true, out var enumValue))
                {
                    value = enumValue;
                    return true;
                }

                error = $"Enumの値が不正です: {rawValue}";
                return false;
            }

            if (type == typeof(Vector2))
            {
                return TryParseVector(rawValue, 2, out value, out error, values => new Vector2(values[0], values[1]));
            }

            if (type == typeof(Vector3))
            {
                return TryParseVector(rawValue, 3, out value, out error, values => new Vector3(values[0], values[1], values[2]));
            }

            if (type == typeof(Vector4))
            {
                return TryParseVector(rawValue, 4, out value, out error, values => new Vector4(values[0], values[1], values[2], values[3]));
            }

            if (type == typeof(Color))
            {
                return TryParseVector(rawValue, 4, out value, out error, values => new Color(values[0], values[1], values[2], values[3]));
            }

            try
            {
                value = Convert.ChangeType(rawValue, type, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                error = $"型変換に失敗しました: {rawValue} -> {type.Name}";
                return false;
            }
        }

        private static bool TryParseVector3(string xText, string yText, string zText, out Vector3 value)
        {
            value = default;
            if (!float.TryParse(xText, NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
            {
                return false;
            }

            if (!float.TryParse(yText, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                return false;
            }

            if (!float.TryParse(zText, NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            {
                return false;
            }

            value = new Vector3(x, y, z);
            return true;
        }

        private static bool TryParseVector(string rawValue, int expectedCount, out object value, out string error, Func<float[], object> factory)
        {
            value = null;
            error = string.Empty;
            var parts = rawValue.Split(',');
            if (parts.Length != expectedCount)
            {
                error = $"値は{expectedCount}要素のカンマ区切りで指定してください";
                return false;
            }

            var values = new float[expectedCount];
            for (var i = 0; i < expectedCount; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
                {
                    error = "数値の解析に失敗しました";
                    return false;
                }
            }

            value = factory(values);
            return true;
        }

        private static Task<GameObject> ToTask(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.IsDone)
            {
                return Task.FromResult(handle.Result);
            }

            var source = new TaskCompletionSource<GameObject>();
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    source.SetResult(operation.Result);
                }
                else
                {
                    source.SetResult(null);
                }
            };

            return source.Task;
        }
    }
}
