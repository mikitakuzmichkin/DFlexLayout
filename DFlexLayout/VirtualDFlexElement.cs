using UnityEngine;

public class VirtualDFlexElement : IFlexElement
{
    private Vector2 _position;
    private Vector2 _size;
    private DFlexElement.ETypeSize _widthType;
    private DFlexElement.ETypeSize _heightType;

    public VirtualDFlexElement(Vector2 size, DFlexElement.ETypeSize widthType, DFlexElement.ETypeSize heightType)
    {
        _size = size;
        _widthType = widthType;
        _heightType = heightType;
    }
    
    public void SetLocalPos(Vector2 pos)
    {
        _position = pos;
    }

    public void SetSize(float? width, float? height, bool withUpdate = false)
    {
        if (width != null)
        {
            _size.x = (float) width;
        }

        if (height != null)
        {
            _size.y = (float) height;
        }
    }

    private Vector2 GetPos()
    {
        return _position;
    }

    public (Vector2 size, DFlexElement.ETypeSize widthType, DFlexElement.ETypeSize heightType) GetSize()
    {
        return (_size, _widthType, _heightType);
    }

    public Rect GetRect()
    {
        return new Rect(_position.x - _size.x / 2f, _position.y - _size.y / 2f, _size.x, _size.y);
    }

    public Vector2 GetLocalPos()
    {
        return _position;
    }
}