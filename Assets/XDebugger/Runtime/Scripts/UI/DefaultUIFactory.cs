using System;
using UnityEngine;
using Xeon.XDebugger.Console;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Profiler;

namespace Xeon.XDebugger.UI
{
    [CreateAssetMenu(fileName = "DefaultUIFactory", menuName = "XDebugger/DefaultUIFactory")]
    public class DefaultUIFactory : ScriptableObject, IUIFactory
    {
        [SerializeField]
        private LabelControl labelControlPrefab;
        [SerializeField]
        private ActionControl actionControlPrefab;
        [SerializeField]
        private StringControl stringControlPrefab;
        [SerializeField]
        private IntSliderControl intSliderControlPrefab;
        [SerializeField]
        private FloatSliderControl floatSliderControlPrefab;
        [SerializeField]
        private NumberControl numberControlPrefab;
        [SerializeField]
        private BoolControl boolControlPrefab;
        [SerializeField]
        private DropdownControl dropdownControlPrefab;

        [SerializeField]
        private ContentGroup verticalGroupPrefab;
        [SerializeField]
        private ContentGroup horizontalGroupPrefab;

        [SerializeField]
        private PageControl pageControlPrefab;
        [SerializeField]
        private ProfilerPage profilePagePrefab;
        [SerializeField]
        private ConsolePage consolePagePrefab;

        public T CreateControl<T>(Transform parent = null) where T : ControlBase, new()
        {
            var type = typeof(T);
            if (type == typeof(LabelControl))
                return Create<T>(labelControlPrefab, parent);
            if (type == typeof(ActionControl))
                return Create<T>(actionControlPrefab, parent);
            if (type == typeof(StringControl))
                return Create<T>(stringControlPrefab, parent);
            if (type == typeof(IntSliderControl))
                return Create<T>(intSliderControlPrefab, parent);
            if (type == typeof(FloatSliderControl))
                return Create<T>(floatSliderControlPrefab, parent);
            if (type == typeof(NumberControl))
                return Create<T>(numberControlPrefab, parent);
            if (type == typeof(BoolControl))
                return Create<T>(boolControlPrefab, parent);
            if (type == typeof(DropdownControl))
                return Create<T>(dropdownControlPrefab, parent);

            throw new InvalidOperationException($"{type} is not supported");
        }

        public ContentGroup CreateVerticalGroup(Transform parent = null) => Instantiate(verticalGroupPrefab, parent);
        public ContentGroup CreateHorizontalGroup(Transform parent = null) => Instantiate(horizontalGroupPrefab, parent);

        public T CreatePage<T>(Transform parent = null) where T : PageControl, new()
        {
            var type = typeof(T);
            if (type == typeof(PageControl))
                return Create<T>(pageControlPrefab, parent);
            if (type == typeof(ProfilerPage))
                return Create<T>(profilePagePrefab, parent);
            if (type == typeof(ConsolePage))
                return Create<T>(consolePagePrefab, parent);

            throw new InvalidOperationException($"{type} is not supported");
        }

        private T Create<T>(ControlBase prefab, Transform parent = null) where T : ControlBase, new()
        {
            return Instantiate(prefab, parent) as T;
        }

        private T Create<T>(PageControl prefab, Transform parent = null) where T : PageControl, new()
        {
            return Instantiate(prefab, parent) as T;
        }
    }
}
