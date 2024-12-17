using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public partial class DFlexLayout
{

    private List<VirtualDFlexLayout> _wrapLayoutsList;
    public void SetChildWrap()
    {
        var parentWidth = _InnerSize.x;
        float childBlock = 0;
        int groupIndex = 0;
        var childs = _flexChilds.Where(c => c.SkipLayout == false);
        List<VirtualDFlexLayout> layouts = new List<VirtualDFlexLayout>();
        layouts.Add(new VirtualDFlexLayout());
        foreach (var child in childs)
        {
            var childSizeParams = child.GetSize();
            childBlock += childSizeParams.size.x;

            if (childBlock > parentWidth)
            {
                childBlock = childSizeParams.size.x;
                layouts.Add(new VirtualDFlexLayout());
            }

            layouts.Last().Childs.Add(child);
        }

        foreach (var layout in layouts)
        {
            layout.SetFlexElement();
        }

        var parentSize = _Rect.rect.size;
        if (_FlexElement.GetSize().heightType == DFlexElement.ETypeSize.Layout)
        {
            var size = new Vector2(_Rect.rect.width,
                layouts.Sum(l => l.FlexElement.GetSize().size.y) + WrapGap * layouts.Count);
            _FlexElement.SetSize(size.x, size.y, true);
            parentSize = _InnerSize;
        }
        
        _wrapLayoutsList = layouts;

        var layoutParams = GetChildSizeParams(layouts.Select(l => l.FlexElement), ETypeDirection.PositiveY, _Rect.rect, WrapGapType, WrapGap);
        Debug.Log($"layoutParams {gameObject.name} sumsize ={layoutParams.sumSize}");
        var fillSize = GetFillSize(layoutParams.sumSize, layoutParams.fillCount, _Rect.rect);
        SetChildPositionsPositive(ETypeDirection.PositiveY, layoutParams.sumSize, 
            new Vector2(layoutParams.maxWidth, layoutParams.maxHeight),  fillSize, layoutParams.fillCount, layouts.Select(l => l.FlexElement),
            parentSize, WrapGap, WrapGapType, _Rect.pivot, _InnerPosition);

        foreach (var layout in layouts)
        {
            var blockParams = GetChildSizeParams(layout.Childs, ETypeDirection.PositiveX, layout.GetRect(), GapType, Gap);
            var blockFillSize = GetFillSize(blockParams.sumSize, blockParams.fillCount, layout.GetRect());
            SetChildPositionsPositive(ETypeDirection.PositiveX, blockParams.sumSize,
                new Vector2(blockParams.maxWidth, blockParams.maxHeight), blockFillSize, blockParams.fillCount,
                layout.Childs, layout.FlexElement.GetSize().size, Gap, GapType, new Vector2(0.5f, 0.5f), Vector2.zero);
            foreach (var child in layout.Childs)
            {
                child.SetLocalPos( layout.FlexElement.GetLocalPos() + child.GetLocalPos());
            }
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (Wrap && _wrapLayoutsList != null)
        {
            foreach (var layout in _wrapLayoutsList)
            {
                Gizmos.color = Color.blue;
                var rect = layout.GetRect();
                DrawRect(_Rect.position + new Vector3(rect.center.x * _Rect.transform.lossyScale.x, rect.center.y * _Rect.transform.lossyScale.y)
                    , new Vector2(rect.size.x * _Rect.transform.lossyScale.x, rect.size.y * _Rect.transform.lossyScale.y));
            }
        }
    }
    
    void DrawRect(Vector3 position, Vector2 size)
    {
        Gizmos.DrawWireCube(position, new Vector3(size.x, size.y, 0.01f));
        
    }

#endif
    
    private class VirtualDFlexLayout : IFlexLayout
    {
        public List<IFlexElement> Childs;
        public IFlexElement FlexElement;

        public VirtualDFlexLayout()
        {
            Childs = new List<IFlexElement>();
        }

        public Rect GetRect()
        {
            return FlexElement.GetRect();
        }

        public void SetFlexElement()
        {
            float maxSize = 0;
            DFlexElement.ETypeSize typeSize = DFlexElement.ETypeSize.Fixed;
            foreach (var child in Childs)
            {
                var sizeParams = child.GetSize();

                if (sizeParams.heightType == DFlexElement.ETypeSize.Fill)
                {
                    typeSize = DFlexElement.ETypeSize.Fill;
                    if(maxSize < sizeParams.size.y)
                        maxSize = sizeParams.size.y;
                }
                else
                {
                    if (typeSize == DFlexElement.ETypeSize.Fill)
                    {
                        continue;
                    }
                    
                    if(maxSize < sizeParams.size.y)
                        maxSize = sizeParams.size.y;
                }
            }
            FlexElement = new VirtualDFlexElement(new Vector2(1, maxSize), DFlexElement.ETypeSize.Fill,
                typeSize);
        }
    }
}