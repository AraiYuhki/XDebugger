using UnityEngine;
using UnityEngine.UI;
using Xeon.XGraph.Model;

namespace Xeon.XGraph.View
{
    /// <summary>
    /// グラフの規定クラス
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer)), ExecuteInEditMode]
    public abstract class ChartBase : MaskableGraphic
    {
        [SerializeField] protected RectOffset padding = new();

        protected bool geometryDirty = false;

        protected float width => rectTransform.rect.width - padding.horizontal;
        protected float height => rectTransform.rect.height - padding.vertical;

        public virtual Color Color
        {
            set
            {
                if (color == value)
                    return;

                color = value;
                OnChangedColor();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            geometryDirty = true;
            SetVerticesDirty();
        }
        
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            geometryDirty = true;
            SetVerticesDirty();
        }

        protected void InitializeSeries(Series series)
        {
            if (series == null)
                return;
            
            FinalizeSeries(series);
            series.OnChangedCollection += OnChangedCollection;
            series.OnChangedColor += OnChangedColor;
        }

        protected void InitializeSeries(MultiValueSeries series)
        {
            if (series == null || series.Series == null)
                return;
            
            foreach (var s in series.Series)
                InitializeSeries(s);
        }

        protected void FinalizeSeries(Series series)
        {
            if (series == null)
                return;
            
            series.OnChangedCollection -= OnChangedCollection;
            series.OnChangedColor -= OnChangedColor;
        }

        protected void FinalizeSeries(MultiValueSeries series)
        {
            if (series == null || series.Series == null)
                return;
            
            foreach (var s in series.Series)
                FinalizeSeries(s);
        }

        protected void OnChangedCollection()
        {
            geometryDirty = true;
            SetVerticesDirty();
        }

        protected virtual void OnChangedColor()
        {
            geometryDirty = true;
            SetVerticesDirty();
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (Application.isPlaying)
                return;
            geometryDirty = true;
            SetVerticesDirty();
        }
#endif
    }
}
