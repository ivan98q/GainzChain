using UnityEngine;

[System.Serializable]
public struct IntBounds {

    [SerializeField]
    VectorI3 _center;

    [SerializeField]
    VectorI3 _extents;

    #region Properties
    public VectorI3 Center {
        get { return _center; }
        set { _center = value; }
    }

    public VectorI3 Extents {
        get { return _extents; }
        set { _extents = value; }
    }

    public VectorI3 Max {
        get { return Center + Extents; }
        set { SetMinMax(Min, value); }
    }

    public VectorI3 Min {
        get { return Center - Extents; }
        set { SetMinMax(value, Max); }
    }

    public VectorI3 Size {
        get { return _extents * 2; }
        set { _extents = value * .5f; }
    }
    #endregion

    public IntBounds(VectorI3 center, VectorI3 size) {
        _center = center;
        _extents = size * .5f;
    }

    public bool Contains(VectorI3 point) {
        return point.x >= Min.x && point.x <= Max.x &&
               point.y >= Min.y && point.y <= Max.y &&
               point.z >= Min.z && point.z <= Max.z;
    }
    public bool Contains(Vector3 point) {
        return point.x >= Min.x && point.x <= Max.x &&
               point.y >= Min.y && point.y <= Max.y &&
               point.z >= Min.z && point.z <= Max.z;
    }

    public void DrawWireCube(Vector3 offset, Color color, float duration = 0, bool depthTest = true) {
        Debug.DrawLine(offset + new Vector3(Min.x, Min.y, Min.z), offset + new Vector3(Min.x, Max.y, Min.z), color, duration, depthTest); //Back Left
        Debug.DrawLine(offset + new Vector3(Min.x, Max.y, Min.z), offset + new Vector3(Min.x, Max.y, Max.z), color, duration, depthTest); //Top Left
        Debug.DrawLine(offset + new Vector3(Min.x, Max.y, Max.z), offset + new Vector3(Min.x, Min.y, Max.z), color, duration, depthTest); //Front Left
        Debug.DrawLine(offset + new Vector3(Min.x, Min.y, Min.z), offset + new Vector3(Min.x, Min.y, Max.z), color, duration, depthTest); //Bottom Left

        Debug.DrawLine(offset + new Vector3(Min.x, Max.y, Min.z), offset + new Vector3(Max.x, Max.y, Min.z), color, duration, depthTest); //Top Back
        Debug.DrawLine(offset + new Vector3(Min.x, Max.y, Max.z), offset + new Vector3(Max.x, Max.y, Max.z), color, duration, depthTest); //Top Front
        Debug.DrawLine(offset + new Vector3(Min.x, Min.y, Min.z), offset + new Vector3(Max.x, Min.y, Min.z), color, duration, depthTest); //Bottom Back
        Debug.DrawLine(offset + new Vector3(Min.x, Min.y, Max.z), offset + new Vector3(Max.x, Min.y, Max.z), color, duration, depthTest); //Bottom Front

        Debug.DrawLine(offset + new Vector3(Max.x, Min.y, Min.z), offset + new Vector3(Max.x, Max.y, Min.z), color, duration, depthTest); //Back Right
        Debug.DrawLine(offset + new Vector3(Max.x, Max.y, Min.z), offset + new Vector3(Max.x, Max.y, Max.z), color, duration, depthTest); //Top Right
        Debug.DrawLine(offset + new Vector3(Max.x, Max.y, Max.z), offset + new Vector3(Max.x, Min.y, Max.z), color, duration, depthTest); //Front Right
        Debug.DrawLine(offset + new Vector3(Max.x, Min.y, Min.z), offset + new Vector3(Max.x, Min.y, Max.z), color, duration, depthTest); //Bottom Right
    }

    public void Encapsulate(VectorI3 point) {
        SetMinMax(VectorI3.Min(Min, point), VectorI3.Max(Max, point));
    }
    public void Encapsulate(IntBounds bounds) {
        Encapsulate(bounds.Center - bounds.Extents);
        Encapsulate(bounds.Center + bounds.Extents);
    }

    public override bool Equals(object obj) {
        if(!(obj is IntBounds))
            return false;
        else {
            IntBounds bounds = (IntBounds)obj;

            return Center == bounds.Center && Extents == bounds.Extents;
        }
    }

    public void Expand(float amount) {
        amount *= .5f;
        Extents += new VectorI3(amount, amount, amount);
    }
    public void Expand(VectorI3 amount) {
        Extents += amount * .5f;
    }

    public bool Intersects(IntBounds bounds) {
        return Min.x <= bounds.Max.x && Max.x >= bounds.Min.x && Min.y <= bounds.Max.y && Max.y >= bounds.Min.y && Min.z <= bounds.Max.z && Max.z >= bounds.Min.z;
    }

    public override int GetHashCode() {
        return Center.GetHashCode() ^ Extents.GetHashCode() << 2;
    }

    public void SetMinMax(VectorI3 min, VectorI3 max) {
        Extents = (max - min) * .5f;
        Center = min + Extents;
    }

    public override string ToString() {
        return string.Format("Center: {0}, Extents: {1}", new object[] {
            Center,
            Extents
        });
    }

    public static bool operator ==(IntBounds lhs, IntBounds rhs) {
        return lhs.Center == rhs.Center && lhs.Extents == rhs.Extents;
    }

    public static bool operator !=(IntBounds lhs, IntBounds rhs) {
        return !(lhs == rhs);
    }

    public static implicit operator Bounds(IntBounds bounds) {
        return new Bounds(bounds.Center, bounds.Size);
    }
    public static implicit operator IntBounds(Bounds bounds) {
        return new IntBounds(bounds.center, bounds.size);
    }

    public static void DrawWireCube(Vector3 offset, Vector3 min, Vector3 max, Color color, float duration = 0, bool depthTest = true) {
        Debug.DrawLine(offset + new Vector3(min.x, min.y, min.z), offset + new Vector3(min.x, max.y, min.z), color, duration, depthTest); //Back Left
        Debug.DrawLine(offset + new Vector3(min.x, max.y, min.z), offset + new Vector3(min.x, max.y, max.z), color, duration, depthTest); //Top Left
        Debug.DrawLine(offset + new Vector3(min.x, max.y, max.z), offset + new Vector3(min.x, min.y, max.z), color, duration, depthTest); //Front Left
        Debug.DrawLine(offset + new Vector3(min.x, min.y, min.z), offset + new Vector3(min.x, min.y, max.z), color, duration, depthTest); //Bottom Left

        Debug.DrawLine(offset + new Vector3(min.x, max.y, min.z), offset + new Vector3(max.x, max.y, min.z), color, duration, depthTest); //Top Back
        Debug.DrawLine(offset + new Vector3(min.x, max.y, max.z), offset + new Vector3(max.x, max.y, max.z), color, duration, depthTest); //Top Front
        Debug.DrawLine(offset + new Vector3(min.x, min.y, min.z), offset + new Vector3(max.x, min.y, min.z), color, duration, depthTest); //Bottom Back
        Debug.DrawLine(offset + new Vector3(min.x, min.y, max.z), offset + new Vector3(max.x, min.y, max.z), color, duration, depthTest); //Bottom Front

        Debug.DrawLine(offset + new Vector3(max.x, min.y, min.z), offset + new Vector3(max.x, max.y, min.z), color, duration, depthTest); //Back Right
        Debug.DrawLine(offset + new Vector3(max.x, max.y, min.z), offset + new Vector3(max.x, max.y, max.z), color, duration, depthTest); //Top Right
        Debug.DrawLine(offset + new Vector3(max.x, max.y, max.z), offset + new Vector3(max.x, min.y, max.z), color, duration, depthTest); //Front Right
        Debug.DrawLine(offset + new Vector3(max.x, min.y, min.z), offset + new Vector3(max.x, min.y, max.z), color, duration, depthTest); //Bottom Right
    }
}