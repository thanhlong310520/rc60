using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Graphic tùy chỉnh vẽ hình bình hành, dùng chung với component Mask
/// để clip các UI con theo hình dạng bình hành thay vì hình chữ nhật.
///
/// Cách dùng:
/// 1. Tạo 1 GameObject UI trống (Rectangle) trong Canvas.
/// 2. Add component "Parallelogram Graphic" (script này).
/// 3. Add thêm component "Mask" (Unity có sẵn: UI > Mask).
///    - Nếu không muốn hình bình hành hiện ra (chỉ dùng để cắt),
///      bỏ tick "Show Mask Graphic" trong component Mask.
/// 4. Kéo các UI con (Image, Text, ...) vào làm con của GameObject này.
///    Chúng sẽ chỉ hiển thị phần nằm trong vùng hình bình hành.
/// 5. Chỉnh "Skew" trong Inspector để đổi độ nghiêng (đơn vị: pixel).
/// </summary>
[AddComponentMenu("UI/Parallelogram Graphic")]
public class ParallelogramGraphic : Graphic
{
    [Tooltip("Độ lệch ngang (px) của cạnh trên so với cạnh dưới. " +
             "Giá trị dương nghiêng sang phải, âm nghiêng sang trái.")]
    [SerializeField] private float skew = 20f;

    public float Skew
    {
        get => skew;
        set
        {
            skew = value;
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect r = rectTransform.rect;

        // 4 đỉnh: đáy giữ nguyên, cạnh trên lệch ngang theo "skew"
        Vector2 bl = new Vector2(r.xMin, r.yMin);
        Vector2 br = new Vector2(r.xMax, r.yMin);
        Vector2 tr = new Vector2(r.xMax + skew, r.yMax);
        Vector2 tl = new Vector2(r.xMin + skew, r.yMax);

        UIVertex v = UIVertex.simpleVert;
        v.color = color;

        v.position = bl; v.uv0 = new Vector2(0, 0); vh.AddVert(v);
        v.position = br; v.uv0 = new Vector2(1, 0); vh.AddVert(v);
        v.position = tr; v.uv0 = new Vector2(1, 1); vh.AddVert(v);
        v.position = tl; v.uv0 = new Vector2(0, 1); vh.AddVert(v);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
