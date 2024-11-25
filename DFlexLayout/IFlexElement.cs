using UnityEngine;

public interface IFlexElement
{
    public void SetLocalPos(Vector2 pos);
    public void SetSize(float? width, float? height, bool withUpdate = false);
    public (Vector2 size, DFlexElement.ETypeSize widthType, DFlexElement.ETypeSize heightType) GetSize();
    public Rect GetRect();
    public Vector2 GetLocalPos();
}