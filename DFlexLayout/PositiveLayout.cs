using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class DFlexLayout
{
    private void SetChildPositionsPositive(ETypeDirection direction, Vector2 sumSizeVector, Vector2 maxSize,
        Vector2 fillSize, Vector2Int fillCount, IEnumerable<IFlexElement> flexChilds, Vector2 parentSize, float gapBase, ETypeGap typeGap,
        Vector2 pivotBase)
    {
        Vector2 offset;
        switch (direction)
        {
            case ETypeDirection.PositiveX:
                offset = GetHorizontalOffset(sumSizeVector, maxSize, fillCount, parentSize, pivotBase);
                break;
            case ETypeDirection.PositiveY:
                offset = GetVerticalOffset(sumSizeVector, maxSize, fillCount, parentSize);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }

        (ETypeAlign align, float size, float sumsize, bool reverse, float originalOffset, float alternativeOffset,
            ETypeAlign
            innerAlternativeAlign, float alternativeMaxSize, 
            float pivot, float alternativePivot,
            float originFillSize, float alternativeFillSize) layoutParams;

        switch (direction)
        {
            case ETypeDirection.PositiveX:
                layoutParams =
                    (HorizontalAlign, parentSize.x, sumSizeVector.x, false,
                        offset.x, offset.y, InnerVerticalAlign, maxSize.y, 
                        pivotBase.x, pivotBase.y,
                        fillSize.x, fillSize.y);
                break;
            case ETypeDirection.PositiveY:
                layoutParams =
                    (VerticalAlign, parentSize.y, sumSizeVector.y, true,
                        offset.y, offset.x, InnerHorizontalAlign, maxSize.x, 
                        pivotBase.y, pivotBase.x,
                        fillSize.y, fillSize.x);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }

        var gap = gapBase;
        var childsList = flexChilds;
        if (layoutParams.reverse)
        {
            childsList = childsList.Reverse();
        }

        var decideCount = childsList.Count();

        if (typeGap == ETypeGap.SpaceBetween)
        {
            if (layoutParams.align == ETypeAlign.Center)
            {
                decideCount++;
            }

            gap = (layoutParams.size - layoutParams.sumsize) / decideCount;
        }


        if (layoutParams.align == ETypeAlign.Center && typeGap == ETypeGap.SpaceBetween)
        {
            layoutParams.originalOffset = gap;
        }

        foreach (var child in childsList)
        {
            var childSizeParams = child.GetSize();

            (float originalSize, float alternativeSize, DFlexElement.ETypeSize originType, DFlexElement.ETypeSize alternativeType) childParams;
            switch (direction)
            {
                case ETypeDirection.PositiveX:
                    childParams = (childSizeParams.size.x,
                        childSizeParams.size.y, childSizeParams.widthType, childSizeParams.heightType);
                    break;
                case ETypeDirection.PositiveY:
                    childParams = (childSizeParams.size.y,
                        childSizeParams.size.x, childSizeParams.heightType, childSizeParams.widthType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (childParams.alternativeType == DFlexElement.ETypeSize.Fill)
            {
                childParams.alternativeSize = layoutParams.alternativeFillSize;
            }

            if (childParams.originType == DFlexElement.ETypeSize.Fill)
            {
                childParams.originalSize *= layoutParams.originFillSize;
            }

            int koof;
            Vector2 childSize;
            switch (direction)
            {
                case ETypeDirection.PositiveX:
                    koof = 1;
                    childSize = new Vector2(childParams.originalSize, childParams.alternativeSize);
                    break;
                case ETypeDirection.PositiveY:
                    koof = -1;
                    childSize = new Vector2(childParams.alternativeSize, childParams.originalSize);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (childSizeParams.widthType == DFlexElement.ETypeSize.Fill )
            {
                child.SetSize(childSize.x, null);
            }
            
            if (childSizeParams.heightType == DFlexElement.ETypeSize.Fill)
            {
                child.SetSize(null, childSize.y);
            }

            float innerAlternativeOffset = 0;
            switch (layoutParams.innerAlternativeAlign)
            {
                case ETypeAlign.Start:
                    innerAlternativeOffset =
                        (layoutParams.alternativeMaxSize / 2f - childParams.alternativeSize / 2f) * koof ;
                    break;
                case ETypeAlign.None:
                case ETypeAlign.Center:
                    break;
                case ETypeAlign.End:
                    innerAlternativeOffset =
                        (-layoutParams.alternativeMaxSize / 2f + childParams.alternativeSize / 2f) * koof;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var originalLocalPos = -layoutParams.size * layoutParams.pivot
                                   + layoutParams.originalOffset + childParams.originalSize / 2f;
            var alternativeLocalPos = layoutParams.alternativeOffset + innerAlternativeOffset;

            if (float.IsNaN(originalLocalPos) || float.IsInfinity(originalLocalPos))
            {
                originalLocalPos = 0;
            }
            
            switch (direction)
            {
                case ETypeDirection.PositiveX:
                    child.SetLocalPos(new Vector2(originalLocalPos, alternativeLocalPos));
                    break;
                case ETypeDirection.PositiveY:
                    child.SetLocalPos(new Vector2(alternativeLocalPos, originalLocalPos));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            layoutParams.originalOffset += childParams.originalSize + gap;
        }
    }

    private Vector2 GetHorizontalOffset(Vector2 sumSize,  Vector2 maxSize, Vector2Int fillCount, Vector2 parentSize, Vector2 pivotBase)
    {
        float offsetX = LeftPadding;
        if (fillCount.x < 1 )
        {
            switch (HorizontalAlign)
            {
                case ETypeAlign.None:
                case ETypeAlign.Start:
                    offsetX = LeftPadding;
                    break;
                case ETypeAlign.Center:
                    offsetX = (parentSize.x - sumSize.x) / 2f;
                    break;
                case ETypeAlign.End:
                    offsetX = parentSize.x
                        //- RightPadding 
                              - sumSize.x;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        float offsetY = -TopPadding;
       // if (_FlexElement.HeightType != DFlexElement.ETypeSize.Layout)
        {
            switch (VerticalAlign)
            {
                case ETypeAlign.Start:
                    offsetY = (parentSize.y * (1 - pivotBase.y) - maxSize.y / 2f)
                              - TopPadding
                        ;
                    break;
                case ETypeAlign.None:
                case ETypeAlign.Center:
                    offsetY = (parentSize.y * (0.5f - pivotBase.y));
                    break;
                case ETypeAlign.End:
                    offsetY = parentSize.y * -pivotBase.y + maxSize.y / 2f 
                                                          +BottomPadding
                        ;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return new Vector2(offsetX, offsetY);
    }

    private Vector2 GetVerticalOffset(Vector2 sumSize, Vector2 maxSize, Vector2Int fillCount, Vector2 parentSize)
    {
        float offsetY = TopPadding;
        if (fillCount.y < 1 )
        {
            switch (VerticalAlign)
            {
                case ETypeAlign.None:
                case ETypeAlign.Start:
                    offsetY = parentSize.y - sumSize.y 
                                                - TopPadding
                        ;
                    break;
                case ETypeAlign.Center:
                    offsetY = (parentSize.y - sumSize.y) / 2f;
                    break;
                case ETypeAlign.End:
                    offsetY = BottomPadding;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        float offsetX = LeftPadding;
        //if (_FlexElement.WidthType != DFlexElement.ETypeSize.Layout)
        {
            switch (HorizontalAlign)
            {
                case ETypeAlign.Start:
                    offsetX = -(parentSize.x - maxSize.x) / 2f 
                              + LeftPadding
                              ;
                    break;
                case ETypeAlign.None:
                case ETypeAlign.Center:
                    break;
                case ETypeAlign.End:
                    offsetX = (parentSize.x - maxSize.x) / 2f 
                              - RightPadding
                              ;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return new Vector2(offsetX, offsetY);
    }
}