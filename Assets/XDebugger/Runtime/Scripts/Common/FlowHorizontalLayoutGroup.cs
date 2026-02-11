using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class FlowHorizontalLayoutGroup : LayoutGroup
{
    [SerializeField]
    private float maxLineWidth;
    [SerializeField]
    private float spaceX;
    [SerializeField]
    private float spaceY;
    [SerializeField]
    private bool controlChildWidth = true;
    [SerializeField]
    private bool controlChildHeight = true;

    [SerializeField]
    private List<Line> lineList = new();

    [Serializable]
    private struct Item
    {
        public RectTransform Target;
        public float Width;
        public float Height;

        public Item(RectTransform target, float width, float height)
        {
            Target = target;
            Width = width;
            Height = height;
        }
    }

    [Serializable]
    private struct Line
    {
        public List<Item> Items;
        public float Width;
        public float Height;
    }


    public override void CalculateLayoutInputVertical()
    {
        float totalHeight = padding.vertical;
        for(var index = 0; index < lineList.Count; index++)
        {
            if (index > 0)
                totalHeight += spaceY;
            totalHeight += lineList[index].Height;
        }
        SetLayoutInputForAxis(totalHeight, totalHeight, -1, 1);
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        var minWidth = GetPreferredWidth();
        var preferredWidth = minWidth;
        SetLayoutInputForAxis(minWidth, preferredWidth, -1, 0);
    }

    private float GetPreferredWidth()
    {
        return Mathf.Max(0f, GetCanvasSpaceScreenWidth());
    }

    public override void SetLayoutHorizontal()
    {
        var availableWidth = Mathf.Max(0f, GetCanvasSpaceScreenWidth() - padding.horizontal);

        lineList.Clear();
        var currentLine = new Line() { Items = new() };
        
        foreach (var child in rectChildren)
        {
            var childWidth = LayoutUtility.GetPreferredWidth(child);
            var childHeight = LayoutUtility.GetPreferredHeight(child);

            var additionalWidth = (currentLine.Items.Count <= 0 ? 0f : spaceX) + childWidth;
            var shouldWrap = currentLine.Items.Count > 0;
            shouldWrap &= currentLine.Width + additionalWidth > availableWidth;
            shouldWrap &= availableWidth > 0f;

            if (shouldWrap)
            {
                lineList.Add(currentLine);
                currentLine = new Line() { Items = new() };
                additionalWidth = childWidth;
            }

            currentLine.Items.Add(new Item(child, childWidth, childHeight));
            currentLine.Width += additionalWidth;
            currentLine.Height = Mathf.Max(currentLine.Height, childHeight);
        }
        if (currentLine.Items.Count > 0)
            lineList.Add(currentLine);

        var contentHeight = GetContentHeightWithoutPadding(lineList);
        var positionY = GetStartOffset(1, contentHeight);
        var verticalAlign = GetAlignmentOnAxis(1);
        foreach (var line in lineList)
        {
            LayoutLine(line, positionY, verticalAlign);
            positionY += line.Height + spaceY;
        }
    }

    private void LayoutLine(Line line, float topPositionY, float verticalAlign)
    {
        var positionX = GetStartOffset(0, line.Width);
        foreach (var item in line.Items)
        {
            var verticalOffset = (line.Height - item.Height) * verticalAlign;
            if (controlChildWidth)
                SetChildAlongAxis(item.Target, 0, positionX, item.Width);
            else
                SetChildAlongAxis(item.Target, 0, positionX);
            if (controlChildHeight)
                SetChildAlongAxis(item.Target, 1, topPositionY + verticalOffset, item.Height);
            else
                SetChildAlongAxis(item.Target, 1, topPositionY + verticalOffset);
            positionX += item.Width + spaceX;
        }
    }

    private float GetContentHeightWithoutPadding(List<Line> lines)
    {
        var result = 0f;
        foreach (var line in lines)
        {
            result += line.Height;
        }
        if (lines.Count > 1)
            result += spaceY * (lines.Count - 1);
        return result;
    }

    public override void SetLayoutVertical()
    {
    }

    private float GetCanvasSpaceScreenWidth()
    {
        var canvas = GetRootCanvas();
        if (canvas != null)
        {
            float scale = canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
            return canvas.pixelRect.width / scale;
        }
        return Screen.width;
    }

    private Canvas GetRootCanvas()
    {
        var list = ListPool<Canvas>.Get();
        gameObject.GetComponentsInParent<Canvas>(false, list);
        if (list.Count == 0) return null;
        Canvas rootCanvas = list.Last();
        foreach (var canvas in list)
        {
            if (canvas.isRootCanvas || canvas.overrideSorting)
            {
                rootCanvas = canvas;
                break;
            }
        }

        ListPool<Canvas>.Release(list);
        return rootCanvas;
    }
}
