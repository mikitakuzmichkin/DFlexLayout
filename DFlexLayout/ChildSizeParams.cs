using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class DFlexLayout
{
         private (Vector2 sumSize, float maxWidth, float maxHeight, Vector2Int fillCount) 
             GetChildSizeParams(IEnumerable<IFlexElement> childs, DFlexLayout.ETypeDirection direction, Rect rect, DFlexLayout.ETypeGap gapType,
                 float gap, float topPadding, float bottomPadding, float leftPadding, float rightPadding)
    {
        Vector2 size = new Vector2();
        float maxWidth = Single.MinValue;
        float maxHeight = Single.MinValue;
        Vector2Int fillCount = new Vector2Int();
        foreach (var child in childs)
        {
            var childParam = child.GetSize();
            if (childParam.widthType != DFlexElement.ETypeSize.Fill)
            {
                size.x += childParam.size.x;

                if (maxWidth < childParam.size.x)
                {
                    maxWidth = childParam.size.x;
                }
            }
            else
            {
                fillCount.x += (int)childParam.size.x;
            }
            
            if (childParam.heightType != DFlexElement.ETypeSize.Fill)
            {
                size.y += childParam.size.y;
                
                if (maxHeight < childParam.size.y)
                {
                    maxHeight = childParam.size.y;
                }
            }
            else
            {
                fillCount.y += (int)childParam.size.y;
            }
        }

        if (direction == DFlexLayout.ETypeDirection.PositiveX)
        {
            size.y = 0;
            if (fillCount.y > 0)
            {
                fillCount.y = 1;
                maxHeight = rect.height;
                
            }

            if (gapType == DFlexLayout.ETypeGap.Fixed)
            {
                size.x += gap * childs.Count();
            }
        }
        else
        {
            size.y += (topPadding + bottomPadding);
        }
        
        if (direction == DFlexLayout.ETypeDirection.PositiveY)
        {
            size.x = 0;
            fillCount.x = 1;
            if (gapType == DFlexLayout.ETypeGap.Fixed)
            {
                size.y += gap * childs.Count();
            }
        }
        else
        {
            size.x += (leftPadding + rightPadding);
        }
        
        return (size, maxWidth, maxHeight, fillCount);
    }

}