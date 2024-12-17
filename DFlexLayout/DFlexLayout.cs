using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(DFlexElement), typeof(RectTransform))]
public partial class DFlexLayout : MonoBehaviour
{
    [SerializeField] private ETypeDirection _direction;
    
    [Header("Wrap")]
    [SerializeField] private bool _wrap;
    [SerializeField] private ETypeGap _wrapGapType;
    [SerializeField] private float _wrapGap;
    
    [Header("Align")]
    [SerializeField] private ETypeAlign _horizontalAlign;
    [SerializeField] private ETypeAlign _verticalAlign;
    [SerializeField] private ETypeAlign _innerHorizontalAlign;
    [SerializeField] private ETypeAlign _innerVerticalAlign;
    
    [Header("Gap")]
    [SerializeField] private ETypeGap _gapType;
    [SerializeField] private float _gap;
    
    [Header("Padding")] 
     [SerializeField] private float _leftPadding;
     [SerializeField] private float _rightPadding;
     [SerializeField] private float _topPadding;
     [SerializeField] private float _bottomPadding;

    private DFlexElement _flexElement;
    private RectTransform _rect;
    private List<DFlexElement> _flexChilds;
    private bool _needUpdateChilds;
    
    private ETypeDirection _oldDirection;
    private ETypeAlign _oldHorizontalAlign;
    private ETypeAlign _oldVerticalAlign;
    private ETypeAlign _oldInnerHorizontalAlign;
    private ETypeAlign _oldInnerVerticalAlign;
    private ETypeGap _oldGapType;
    private float _oldGap;

    private bool _oldWrap;
    private ETypeGap _oldWrapGapType;
    private float _oldWrapGap;
    
    private float _oldLeftPadding;
    private float _oldRightPadding;
    private float _oldTopPadding;
    private float _oldBottomPadding;

    private Vector2 _oldSize;
    private Vector3 _oldPos;

    private DFlexElement _FlexElement
    {
        get
        {
            if (_flexElement == null)
            {
                _flexElement = GetComponent<DFlexElement>();
            }

            return _flexElement;
        }
    }

    private RectTransform _Rect
    {
        get
        {
            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>();
            }

            return _rect;
        }
    }

    public ETypeDirection Direction
    {
        get => _direction;
        set => _direction = value;
    }

    public bool Wrap
    {
        get => _wrap;
        set => _wrap = value;
    }

    public ETypeGap WrapGapType
    {
        get => _wrapGapType;
        set => _wrapGapType = value;
    }

    public float WrapGap
    {
        get => _wrapGap;
        set => _wrapGap = value;
    }

    public ETypeAlign HorizontalAlign
    {
        get => _horizontalAlign;
        set => _horizontalAlign = value;
    }

    public ETypeAlign VerticalAlign
    {
        get => _verticalAlign;
        set => _verticalAlign = value;
    }

    public ETypeAlign InnerHorizontalAlign
    {
        get => _innerHorizontalAlign;
        set => _innerHorizontalAlign = value;
    }

    public ETypeAlign InnerVerticalAlign
    {
        get => _innerVerticalAlign;
        set => _innerVerticalAlign = value;
    }

    public ETypeGap GapType
    {
        get => _gapType;
        set => _gapType = value;
    }

    public float Gap
    {
        get => _gap;
        set => _gap = value;
    }

    public float LeftPadding
    {
        get => _leftPadding;
        set => _leftPadding = value;
    }

    public float RightPadding
    {
        get => _rightPadding;
        set => _rightPadding = value;
    }

    public float TopPadding
    {
        get => _topPadding;
        set => _topPadding = value;
    }

    public float BottomPadding
    {
        get => _bottomPadding;
        set => _bottomPadding = value;
    }

    private Vector2 _InnerPosition => new Vector2(_leftPadding * (1 - _Rect.pivot.x) - _rightPadding * _Rect.pivot.x, -_topPadding * _Rect.pivot.y + _bottomPadding * (1 - _Rect.pivot.y));
    private Vector2 _InnerSize => _FlexElement.GetSize().size - new Vector2(_leftPadding  + _rightPadding, _topPadding + _bottomPadding);

    private void LateUpdate()
    {
        bool wasUpdateChilds = false;
        if (_flexChilds == null || _needUpdateChilds || CheckChilds())
        {
            UpdateChilds();
            wasUpdateChilds = true;
        } 
        if (CheckUpdate() || CheckLayoutChanges() || wasUpdateChilds)
        {
            //Debug.Log(gameObject.name + " is reloaded");
            UpdateElements();
        }

        UpdateOldChanges();
    }

    private void UpdateChilds()
    {
        _flexChilds = GetComponentsInChildren<DFlexElement>(true).Where(c => c.transform.parent == transform).ToList();
        _flexChilds.Remove(_FlexElement);
        _needUpdateChilds = false;
    }

    private bool CheckChilds()
    {
        foreach (var child in _flexChilds)
        {
            if (child == null || child.gameObject == null)
            {
                return true;
            }
        }

        return false;
    }

    private void OnTransformChildrenChanged()
    {
       // Debug.Log("OnTransformChildrenChanged");
        UpdatedChild();
    }

    public void UpdatedChild()
    {
        //Debug.Log("UpdatedChild "+ gameObject.name);
        _needUpdateChilds = true;
    }

    private bool CheckUpdate()
    {
        foreach (var child in _flexChilds)
        {
            if (child.HasChanges)
            {
               // Debug.Log($"gameobject {gameObject.name} has changes");
                return true;
            }
        }

        // if (_FlexElement.HasChanges)
        // {
        //     Debug.Log($"gameobject {gameObject.name} layout has changes");
        //     return true;
        // }

        return false;
    }

    private void UpdateElements()
    {
        if (Direction == ETypeDirection.PositiveX && Wrap && _FlexElement.GetSize().widthType != DFlexElement.ETypeSize.Layout)
        {
            SetChildWrap();
        }
        else
        {
            var sizeParams = GetChildSizeParams(_flexChilds.Where(c => c.SkipLayout == false), Direction, _Rect.rect, GapType, Gap);
            var fillSize = GetFillSize(sizeParams.sumSize, sizeParams.fillCount, _Rect.rect);
            SetChildPositions(sizeParams.sumSize, new Vector2(sizeParams.maxWidth, sizeParams.maxHeight), fillSize, sizeParams.fillCount);
        }
        
        foreach (var child in _flexChilds)
        {
            child.ElemChanged();
        }
    }

    private Vector2 GetFillSize(Vector2 sumSize, Vector2 fillCount, Rect rect)
    {
        var x = Mathf.Max(0, (_InnerSize.x - sumSize.x) / (fillCount.x * 1f));
        var y = Mathf.Max(0, (_InnerSize.y - sumSize.y) / (fillCount.y * 1f));
        return new Vector2(x, y);
    }

    private void SetChildPositions(Vector2 sumSize, Vector2 maxSize, Vector2 fillSize, Vector2Int fillCount)
    {
        float? width = null;
        float? height = null;
        switch (Direction)
        {
            case ETypeDirection.PositiveX:
                if (_FlexElement.HeightType == DFlexElement.ETypeSize.Layout ||
                    _FlexElement.WidthType == DFlexElement.ETypeSize.Layout)
                {
                    if (_FlexElement.WidthType == DFlexElement.ETypeSize.Layout)
                    {
                        width = sumSize.x;
                    }

                    if (_FlexElement.HeightType == DFlexElement.ETypeSize.Layout)
                    {
                        height = maxSize.y;
                    }
                    _FlexElement.SetSize(width, height, true);
                }
                break;
            case ETypeDirection.PositiveY:
                if (_FlexElement.HeightType == DFlexElement.ETypeSize.Layout ||
                    _FlexElement.WidthType == DFlexElement.ETypeSize.Layout)
                {
                    if (_FlexElement.WidthType == DFlexElement.ETypeSize.Layout)
                    {
                        width = maxSize.x;
                    }

                    if (_FlexElement.HeightType == DFlexElement.ETypeSize.Layout)
                    {
                        height = sumSize.y;
                    }
                    _FlexElement.SetSize(width, height
                       , true);
                }
                break;
            // case ETypeDirection.NegativeX:
            //     break;
            // case ETypeDirection.NegativeY:
            //     break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var parentSize = _InnerSize;
        if (_FlexElement.WidthType == DFlexElement.ETypeSize.Layout && Direction == ETypeDirection.PositiveY)
        {
            parentSize.x = maxSize.x;
        }
        
        if (_FlexElement.HeightType == DFlexElement.ETypeSize.Layout && Direction == ETypeDirection.PositiveX)
        {
            parentSize.y = maxSize.y;
        }

        var childs = _flexChilds.Where(c => c.SkipLayout == false);
        {
            SetChildPositionsPositive(Direction, sumSize, maxSize, fillSize, fillCount, childs, 
                parentSize, Gap, GapType, _Rect.pivot, _InnerPosition);
        }
        
    }
    
    private bool CheckLayoutChanges()
    {
        float TOLERANCE = 0.01f;
        if (_oldDirection != Direction)
        {
            //Debug.Log($"gameobject {gameObject.name} direction changed");
            return true;
        }
        
        if (_oldHorizontalAlign != HorizontalAlign ||
            _oldVerticalAlign != VerticalAlign ||
            _oldInnerHorizontalAlign != InnerHorizontalAlign ||
            _oldInnerVerticalAlign != InnerVerticalAlign)
        {
            //Debug.Log($"gameobject {gameObject.name} align changed");
            return true;
        }
        
        if (_oldGapType != GapType ||
            Math.Abs(_oldGap - Gap) > TOLERANCE)
        {
            //Debug.Log($"gameobject {gameObject.name} gap changed");
            return true;
        }

        if (LeftPadding < 0)
        {
            LeftPadding = _oldLeftPadding;
        }
        if (RightPadding < 0)
        {
            RightPadding = _oldRightPadding;
        }
        if (TopPadding < 0)
        {
            TopPadding = _oldTopPadding;
        }
        if (BottomPadding < 0)
        {
            BottomPadding = _oldBottomPadding;
        }
        
        if (Math.Abs(_oldLeftPadding - LeftPadding) > TOLERANCE ||
            Math.Abs(_oldRightPadding - RightPadding) > TOLERANCE ||
            Math.Abs(_oldTopPadding - TopPadding) > TOLERANCE ||
            Math.Abs(_oldBottomPadding - BottomPadding) > TOLERANCE)
        {
            //Debug.Log($"gameobject {gameObject.name} paddings changed");
            return true;
        } 
        
        if (Wrap != _oldWrap
            || WrapGapType != _oldWrapGapType
            || Math.Abs(WrapGap - _oldWrapGap) > TOLERANCE)
        {
            //Debug.Log($"gameobject {gameObject.name} wrap changed");
            return true;
        }

        // if (Math.Abs(_oldPos.x - _Rect.position.x) > TOLERANCE || 
        //     Math.Abs(_oldPos.y - _Rect.position.y) > TOLERANCE)
        // {
        //     Debug.Log($"gameobject {gameObject.name} pos changed");
        //     return true;
        // }
        
        if (Math.Abs(_oldSize.x - _Rect.rect.size.x) > TOLERANCE || 
            Math.Abs(_oldSize.y - _Rect.rect.size.y) > TOLERANCE)
        {
            //Debug.Log($"gameobject {gameObject.name} size changed");
            return true;
        }

        return false;
    }

    private void UpdateOldChanges()
    {
        _oldDirection = Direction;
            _oldHorizontalAlign = HorizontalAlign;
            _oldVerticalAlign = VerticalAlign;
            _oldInnerHorizontalAlign = InnerHorizontalAlign;
            _oldInnerVerticalAlign = InnerVerticalAlign;
            _oldGapType = GapType;
            _oldGap = Gap;
            _oldLeftPadding = LeftPadding;
            _oldRightPadding = RightPadding;
            _oldTopPadding = TopPadding;
            _oldBottomPadding = BottomPadding;

            _oldWrap = Wrap;
            _oldWrapGap = WrapGap;
            _oldWrapGapType = WrapGapType;
            _oldSize = _Rect.rect.size;
            _oldPos = _Rect.position;
    }
    
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        LateUpdate();
    }
#endif

    public enum ETypeDirection
    {
        PositiveX,
        PositiveY,
        // NegativeX,
        // NegativeY
    }
    
    public enum ETypeAlign
    {
        None,
        Start,
        Center,
        End
    }

    public enum ETypeGap
    {
        Fixed,
        SpaceBetween
    }
}
