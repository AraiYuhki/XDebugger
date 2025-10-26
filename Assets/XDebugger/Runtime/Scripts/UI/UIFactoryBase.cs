using System;
using UnityEngine;
using Xeon.XDebugger.Console;
using Xeon.XDebugger.Control;
using Xeon.XDebugger.Profiler;

namespace Xeon.XDebugger.UI
{
    public abstract class UIFactoryBase : ScriptableObject, IUIFactory
    {
        [Header("Controls")]
        [SerializeField]
        protected LabelControl labelControlPrefab;
        [SerializeField]
        protected ActionControl actionControlPrefab;
        [SerializeField]
        protected StringControl stringControlPrefab;
        [SerializeField]
        protected IntSliderControl intSliderControlPrefab;
        [SerializeField]
        protected FloatSliderControl floatSliderControlPrefab;
        [SerializeField]
        protected NumberControl numberControlPrefab;
        [SerializeField]
        protected BoolControl boolControlPrefab;
        [SerializeField]
        protected DropdownControl dropdownControlPrefab;

        [Header("Groupd")]
        [SerializeField]
        protected ContentGroup verticalGroupPrefab;
        [SerializeField]
        protected ContentGroup horizontalGroupPrefab;

        [Header("Pages")]
        [SerializeField]
        protected PageControl pageControlPrefab;
        [SerializeField]
        protected ProfilerPage profilePagePrefab;
        [SerializeField]
        protected ConsolePage consolePagePrefab;

        public virtual T CreateControl<T>(Transform parent = null) where T : ControlBase, new()
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

        public virtual T CreatePage<T>(Transform parent = null) where T : PageControl, new()
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

        public virtual ContentGroup CreateVerticalGroup(Transform parent = null) => Instantiate(verticalGroupPrefab, parent);
        public virtual ContentGroup CreateHorizontalGroup(Transform parent = null) => Instantiate(horizontalGroupPrefab, parent);

        protected virtual T Create<T>(ControlBase prefab, Transform parent = null) where T : ControlBase, new()
        {
            return Instantiate(prefab, parent) as T;
        }

        protected virtual T Create<T>(PageControl prefab, Transform parent = null) where T : PageControl, new()
        {
            return Instantiate(prefab, parent) as T;
        }
    }
}
