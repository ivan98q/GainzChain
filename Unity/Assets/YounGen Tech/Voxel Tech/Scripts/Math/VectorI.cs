using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct VectorI3 {
    #region Constants
    public static VectorI3 back { get { return new VectorI3(0, 0, -1); } }
    public static VectorI3 bottom { get { return new VectorI3(0, -1, 0); } }
    public static VectorI3 down { get { return new VectorI3(0, -1, 0); } }
    public static VectorI3 forward { get { return new VectorI3(0, 0, 1); } }
    public static VectorI3 left { get { return new VectorI3(-1, 0, 0); } }
    public static VectorI3 negativeOne { get { return new VectorI3(-1, -1, -1); } }
    public static VectorI3 one { get { return new VectorI3(1, 1, 1); } }
    public static VectorI3 right { get { return new VectorI3(1, 0, 0); } }
    public static VectorI3 top { get { return new VectorI3(0, 1, 0); } }
    public static VectorI3 up { get { return new VectorI3(0, 1, 0); } }
    public static VectorI3 zero { get { return new VectorI3(0, 0, 0); } }

    #region Direction Array
    static VectorI3[] _direction = new VectorI3[]{
        VectorI3.left,
        VectorI3.right,
        VectorI3.up,
        VectorI3.down,
        VectorI3.forward,
        VectorI3.back,
		
		//Corner
		new VectorI3(-1,1,1),
        new VectorI3(1,-1,-1),

        new VectorI3(-1,-1,1),
        new VectorI3(1,1,-1),

        new VectorI3(-1,1,-1),
        new VectorI3(1,-1,1),

        new VectorI3(-1,-1,-1),
        new VectorI3(1,1,1),

		//Edge Corner
		new VectorI3(-1,0,1), //Left Front
		new VectorI3(1,0,-1), //Right Back
		new VectorI3(-1,0,-1), //Left Back
		new VectorI3(1,0,1), //Right Front

		new VectorI3(-1,1,0), //Left Top
		new VectorI3(1,-1,0), //Right Bottom
		new VectorI3(-1,-1,0), //Left Bottom
		new VectorI3(1,1,0), //Right Top

		new VectorI3(0,1,1), //Top Front
		new VectorI3(0,-1,-1), //Bottom Back
		new VectorI3(0,1,-1), //Top Back
		new VectorI3(0,-1,1), //Bottom Front
	};
    #endregion
    public static VectorI3[] direction {
        get { return _direction; }
    }
    #endregion

    public int x, y, z;
    public int this[int index] {
        get {
            switch(index) {
                default: return x;
                case 1: return y;
                case 2: return z;
            }
        }
        set {
            switch(index) {
                default: x = value; break;
                case 1: y = value; break;
                case 2: z = value; break;
            }
        }
    }

    public VectorI3(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public VectorI3(float x, float y, float z) {
        this.x = (int)x;
        this.y = (int)y;
        this.z = (int)z;
    }
    public VectorI3(VectorI3 vector) {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }
    public VectorI3(Vector3 vector) {
        this.x = (int)vector.x;
        this.y = (int)vector.y;
        this.z = (int)vector.z;
    }
    public VectorI3(VectorI2 vector) {
        this.x = vector.x;
        this.y = vector.y;
        z = 0;
    }

    #region VectorI3 / Vector3 Methods
    public void Abs() {
        x = Mathf.Abs(x);
        y = Mathf.Abs(y);
        z = Mathf.Abs(z);
    }

    public static VectorI3 GridCeil(Vector3 v, Vector3 roundBy) {
        return Round(new Vector3(
            FastMath.Ceil(v.x / roundBy.x) * roundBy.x,
            FastMath.Ceil(v.y / roundBy.y) * roundBy.y,
            FastMath.Ceil(v.z / roundBy.z) * roundBy.z
            ));
    }
    public static VectorI3 GridFloor(Vector3 v, Vector3 roundBy) {
        return Round(new Vector3(
            FastMath.Floor(v.x / roundBy.x) * roundBy.x,
            FastMath.Floor(v.y / roundBy.y) * roundBy.y,
            FastMath.Floor(v.z / roundBy.z) * roundBy.z
            ));
    }
    public static VectorI3 GridRound(Vector3 v, Vector3 roundBy) {
        return Round(new Vector3(
            FastMath.Round(v.x / roundBy.x) * roundBy.x,
            FastMath.Round(v.y / roundBy.y) * roundBy.y,
            FastMath.Round(v.z / roundBy.z) * roundBy.z
            ));
    }

    public static VectorI3 Ceil(Vector3 v) {
        return new VectorI3(FastMath.CeilToInt(v.x), FastMath.CeilToInt(v.y), FastMath.CeilToInt(v.z));
    }
    public static VectorI3 Floor(Vector3 v) {
        return new VectorI3(FastMath.FloorToInt(v.x), FastMath.FloorToInt(v.y), FastMath.FloorToInt(v.z));
    }
    public static VectorI3 Round(Vector3 v) {
        return new VectorI3(FastMath.RoundToInt(v.x), FastMath.RoundToInt(v.y), FastMath.RoundToInt(v.z));
    }
    public static VectorI3 FastRound(Vector3 v) {
        return new VectorI3(
            v.x > 0 ? (int)(v.x + .5f) : (int)(v.x - .5f),
            v.y > 0 ? (int)(v.y + .5f) : (int)(v.y - .5f),
            v.z > 0 ? (int)(v.z + .5f) : (int)(v.z - .5f)
        );
    }

    public void Ceil() {
        x = FastMath.CeilToInt(x);
        y = FastMath.CeilToInt(y);
        z = FastMath.CeilToInt(z);
    }
    public void Floor() {
        x = FastMath.FloorToInt(x);
        y = FastMath.FloorToInt(y);
        z = FastMath.FloorToInt(z);
    }
    public void Round() {
        x = FastMath.RoundToInt(x);
        y = FastMath.RoundToInt(y);
        z = FastMath.RoundToInt(z);
    }
    public void FastRound() {
        x = x > 0 ? (int)(x + .5f) : (int)(x - .5f);
        y = y > 0 ? (int)(y + .5f) : (int)(y - .5f);
        z = z > 0 ? (int)(z + .5f) : (int)(z - .5f);
    }

    public int size {
        get { return Size(this); }
    }
    public static int Size(VectorI3 v) {
        return v.x * v.y * v.z;
    }

    //public static int DirectionToIndex(VectorI3 dir) {
    //    if(_direction.Contains(dir)) {
    //        return _direction.IndexOf(dir);
    //    }
    //    else {
    //        return 0;
    //    }
    //}
    public static VectorI3 Wrap3DIndex(VectorI3 positionIndex, VectorI3 direction, VectorI3 arraySize) {
        VectorI3 newDirection = new VectorI3(
            ((positionIndex.x + direction.x) % (arraySize.x)),
            ((positionIndex.y + direction.y) % (arraySize.y)),
            ((positionIndex.z + direction.z) % (arraySize.z))
            );

        if(newDirection.x < 0) { newDirection.x = arraySize.x + newDirection.x; }
        if(newDirection.y < 0) { newDirection.y = arraySize.y + newDirection.y; }
        if(newDirection.z < 0) { newDirection.z = arraySize.z + newDirection.z; }

        return newDirection;
    }

    public static bool AnyGreater(VectorI3 a, VectorI3 b) {
        return a.x > b.x || a.y > b.y || a.z > b.z;
    }
    public static bool AllGreater(VectorI3 a, VectorI3 b) {
        return a.x > b.x && a.y > b.y && a.z > b.z;
    }
    public static bool AnyLower(VectorI3 a, VectorI3 b) {
        return a.x < b.x || a.y < b.y || a.z < b.z;
    }
    public static bool AllLower(VectorI3 a, VectorI3 b) {
        return a.x < b.x && a.y < b.y && a.z < b.z;
    }
    public static bool AnyGreaterAllEqual(VectorI3 a, VectorI3 b) {
        return a == b || AnyGreater(a, b);
    }
    public static bool AllGreaterEqual(VectorI3 a, VectorI3 b) {
        return a == b || AllGreater(a, b);
    }
    public static bool AnyLowerEqual(VectorI3 a, VectorI3 b) {
        return a == b || AnyLower(a, b);
    }
    public static bool AllLowerEqual(VectorI3 a, VectorI3 b) {
        return a == b || AllLower(a, b);
    }

    public static VectorI3 Max(VectorI3 lhs, VectorI3 rhs) {
        return new VectorI3(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Mathf.Max(lhs.z, rhs.z));
    }
    public static VectorI3 Min(VectorI3 lhs, VectorI3 rhs) {
        return new VectorI3(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Mathf.Min(lhs.z, rhs.z));
    }
    #endregion

    #region Advanced
    public static VectorI3 operator -(VectorI3 a) {
        return new VectorI3(-a.x, -a.y, -a.z);
    }
    public static VectorI3 operator -(VectorI3 a, VectorI3 b) {
        return new VectorI3(a.x - b.x, a.y - b.y, a.z - b.z);
    }
    public static Vector3 operator -(Vector3 a, VectorI3 b) {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }
    public static Vector3 operator -(VectorI3 a, Vector3 b) {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static bool operator !=(VectorI3 lhs, VectorI3 rhs) {
        return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
    }
    public static bool operator !=(Vector3 lhs, VectorI3 rhs) {
        return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
    }
    public static bool operator !=(VectorI3 lhs, Vector3 rhs) {
        return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
    }

    public static Vector3 operator *(float d, VectorI3 a) {
        return new Vector3(d * a.x, d * a.y, d * a.z);
    }
    public static VectorI3 operator *(int d, VectorI3 a) {
        return new VectorI3(d * a.x, d * a.y, d * a.z);
    }
    public static Vector3 operator *(VectorI3 a, float d) {
        return new Vector3(a.x * d, a.y * d, a.z * d);
    }
    public static VectorI3 operator *(VectorI3 a, int d) {
        return new VectorI3(a.x * d, a.y * d, a.z * d);
    }
    public static Vector3 operator /(VectorI3 a, float d) {
        return new Vector3(a.x / d, a.y / d, a.z / d);
    }
    public static VectorI3 operator /(VectorI3 a, int d) {
        return new VectorI3(a.x / d, a.y / d, a.z / d);
    }
    public static float operator /(float d, VectorI3 a) {
        d /= a.x; d /= a.y; d /= a.z;
        return d;
    }

    public static VectorI3 operator *(VectorI3 a, VectorI3 b) {
        return new VectorI3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
    public static Vector3 operator *(Vector3 a, VectorI3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
    public static Vector3 operator *(VectorI3 a, Vector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static VectorI3 operator /(VectorI3 a, VectorI3 b) {
        return new VectorI3(a.x / b.x, a.y / b.y, a.z / b.z);
    }
    public static Vector3 operator /(Vector3 a, VectorI3 b) {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }
    public static Vector3 operator /(VectorI3 a, Vector3 b) {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static VectorI3 operator +(VectorI3 a, VectorI3 b) {
        return new VectorI3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static Vector3 operator +(Vector3 a, VectorI3 b) {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static Vector3 operator +(VectorI3 a, Vector3 b) {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3 operator +(VectorI3 a, float d) {
        return new Vector3(a.x + d, a.y + d, a.z + d);
    }
    public static VectorI3 operator +(VectorI3 a, int d) {
        return new VectorI3(a.x + d, a.y + d, a.z + d);
    }

    public static bool operator ==(VectorI3 lhs, VectorI3 rhs) {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
    }
    public static bool operator ==(Vector3 lhs, VectorI3 rhs) {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
    }
    public static bool operator ==(VectorI3 lhs, Vector3 rhs) {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
    }

    public static implicit operator VectorI3(Vector2 v) {
        return new VectorI3(v.x, v.y, 0);
    }
    public static implicit operator VectorI3(Vector3 v) {
        return new VectorI3(v);
    }
    public static implicit operator VectorI3(Vector4 v) {
        return new VectorI3(v.x, v.y, v.z);
    }
    public static implicit operator Vector2(VectorI3 v) {
        return new Vector3(v.x, v.y, v.z);
    }
    public static implicit operator Vector3(VectorI3 v) {
        return new Vector3(v.x, v.y, v.z);
    }
    public static implicit operator Vector4(VectorI3 v) {
        return new Vector3(v.x, v.y, v.z);
    }

    public static implicit operator int[] (VectorI3 v) {
        return new int[] { v.x, v.y, v.z };
    }

    public override bool Equals(object obj) {
        if(obj.GetType() == typeof(VectorI3)) {
            VectorI3 v = (VectorI3)obj;
            return this.x == v.x && this.y == v.y && this.z == v.z;
        }

        return false;
    }
    public override int GetHashCode() {
        //return base.GetHashCode();
        return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
    }
    public override string ToString() {
        return "(" + x + ", " + y + ", " + z + ")";
    }
    #endregion
}

[System.Serializable]
public struct VectorI2 {
    #region Constants
    public static VectorI2 bottom { get { return new VectorI2(0, -1); } }
    public static VectorI2 down { get { return new VectorI2(0, -1); } }
    public static VectorI2 left { get { return new VectorI2(-1, 0); } }
    public static VectorI2 negativeOne { get { return new VectorI2(-1, -1); } }
    public static VectorI2 one { get { return new VectorI2(1, 1); } }
    public static VectorI2 right { get { return new VectorI2(1, 0); } }
    public static VectorI2 top { get { return new VectorI2(0, 1); } }
    public static VectorI2 up { get { return new VectorI2(0, 1); } }
    public static VectorI2 zero { get { return new VectorI2(0, 0); } }
    #endregion

    public int x, y;
    public int this[int index] {
        get {
            switch(index) {
                default: return x;
                case 1: return y;
            }
        }
        set {
            switch(index) {
                default: x = value; break;
                case 1: y = value; break;
            }
        }
    }

    public VectorI2(int x, int y) {
        this.x = x;
        this.y = y;
    }
    public VectorI2(float x, float y) {
        this.x = (int)x;
        this.y = (int)y;
    }
    public VectorI2(VectorI2 vector) {
        this.x = vector.x;
        this.y = vector.y;
    }
    public VectorI2(Vector2 vector) {
        this.x = (int)vector.x;
        this.y = (int)vector.y;
    }

    #region VectorI2 / Vector3 Methods

    public static VectorI2 GridCeil(Vector2 v, Vector2 roundBy) {
        return Round(new Vector2(
            FastMath.Ceil(v.x / roundBy.x) * roundBy.x,
            FastMath.Ceil(v.y / roundBy.y) * roundBy.y
            ));
    }
    public static VectorI2 GridFloor(Vector2 v, Vector2 roundBy) {
        return Round(new Vector2(
            FastMath.Floor(v.x / roundBy.x) * roundBy.x,
            FastMath.Floor(v.y / roundBy.y) * roundBy.y
            ));
    }
    public static VectorI2 GridRound(Vector2 v, Vector2 roundBy) {
        return Round(new Vector2(
            FastMath.Round(v.x / roundBy.x) * roundBy.x,
            FastMath.Round(v.y / roundBy.y) * roundBy.y
            ));
    }

    public static VectorI2 Ceil(Vector2 v) {
        return new VectorI2(FastMath.CeilToInt(v.x), FastMath.CeilToInt(v.y));
    }
    public static VectorI2 Floor(Vector2 v) {
        return new VectorI2(FastMath.FloorToInt(v.x), FastMath.FloorToInt(v.y));
    }
    public static VectorI2 Round(Vector2 v) {
        return new VectorI2(FastMath.RoundToInt(v.x), FastMath.RoundToInt(v.y));
    }

    public int size {
        get { return Size(this); }
    }
    public static int Size(VectorI2 v) {
        return v.x * v.y;
    }

    public static VectorI2 Wrap2DIndex(VectorI2 positionIndex, VectorI2 direction, VectorI2 arraySize) {
        VectorI2 newDirection = new VectorI2(
            ((positionIndex.x + direction.x) % (arraySize.x)),
            ((positionIndex.y + direction.y) % (arraySize.y))
            );

        if(newDirection.x < 0) { newDirection.x = arraySize.x + newDirection.x; }
        if(newDirection.y < 0) { newDirection.y = arraySize.y + newDirection.y; }

        return newDirection;
    }

    public static bool AnyGreater(VectorI2 a, VectorI2 b) {
        return a.x > b.x || a.y > b.y;
    }
    public static bool AllGreater(VectorI2 a, VectorI2 b) {
        return a.x > b.x && a.y > b.y;
    }
    public static bool AnyLower(VectorI2 a, VectorI2 b) {
        return a.x < b.x || a.y < b.y;
    }
    public static bool AllLower(VectorI2 a, VectorI2 b) {
        return a.x < b.x && a.y < b.y;
    }
    public static bool AnyGreaterAllEqual(VectorI2 a, VectorI2 b) {
        return a == b || AnyGreater(a, b);
    }
    public static bool AllGreaterEqual(VectorI2 a, VectorI2 b) {
        return a == b || AllGreater(a, b);
    }
    public static bool AnyLowerEqual(VectorI2 a, VectorI2 b) {
        return a == b || AnyLower(a, b);
    }
    public static bool AllLowerEqual(VectorI2 a, VectorI2 b) {
        return a == b || AllLower(a, b);
    }

    public static VectorI2 Max(VectorI2 lhs, VectorI2 rhs) {
        return new VectorI2(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));
    }
    public static VectorI2 Min(VectorI2 lhs, VectorI2 rhs) {
        return new VectorI2(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
    }
    #endregion

    #region Advanced
    public static VectorI2 operator -(VectorI2 a) {
        return new VectorI2(-a.x, -a.y);
    }
    public static VectorI2 operator -(VectorI2 a, VectorI2 b) {
        return new VectorI2(a.x - b.x, a.y - b.y);
    }
    public static Vector2 operator -(Vector3 a, VectorI2 b) {
        return new Vector2(a.x - b.x, a.y - b.y);
    }
    public static Vector2 operator -(VectorI2 a, Vector2 b) {
        return new Vector2(a.x - b.x, a.y - b.y);
    }

    public static bool operator !=(VectorI2 lhs, VectorI2 rhs) {
        return lhs.x != rhs.x && lhs.y != rhs.y;
    }
    public static bool operator !=(Vector2 lhs, VectorI2 rhs) {
        return lhs.x != rhs.x && lhs.y != rhs.y;
    }
    public static bool operator !=(VectorI2 lhs, Vector2 rhs) {
        return lhs.x != rhs.x && lhs.y != rhs.y;
    }

    public static Vector2 operator *(float d, VectorI2 a) {
        return new Vector2(d * a.x, d * a.y);
    }
    public static Vector2 operator *(VectorI2 a, float d) {
        return new Vector2(a.x * d, a.y * d);
    }
    public static Vector2 operator /(VectorI2 a, float d) {
        return new Vector2(a.x / d, a.y / d);
    }
    public static float operator /(float d, VectorI2 a) {
        d /= a.x; d /= a.y;
        return d;
    }

    public static VectorI2 operator *(VectorI2 a, VectorI2 b) {
        return new VectorI2(a.x * b.x, a.y * b.y);
    }
    public static Vector2 operator *(Vector2 a, VectorI2 b) {
        return new Vector2(a.x * b.x, a.y * b.y);
    }
    public static Vector2 operator *(VectorI2 a, Vector2 b) {
        return new Vector2(a.x * b.x, a.y * b.y);
    }

    public static VectorI2 operator /(VectorI2 a, VectorI2 b) {
        return new VectorI2(a.x / b.x, a.y / b.y);
    }
    public static Vector2 operator /(Vector2 a, VectorI2 b) {
        return new Vector2(a.x / b.x, a.y / b.y);
    }
    public static Vector2 operator /(VectorI2 a, Vector2 b) {
        return new Vector2(a.x / b.x, a.y / b.y);
    }

    public static VectorI3 operator +(VectorI2 a, VectorI3 b) {
        return new VectorI3(a.x + b.x, a.y + b.y, b.z);
    }
    public static VectorI3 operator +(VectorI3 a, VectorI2 b) {
        return new VectorI3(a.x + b.x, a.y + b.y, a.z);
    }
    public static Vector3 operator +(VectorI2 a, Vector3 b) {
        return new VectorI3(a.x + b.x, a.y + b.y, b.z);
    }
    public static Vector3 operator +(Vector3 a, VectorI2 b) {
        return new VectorI3(a.x + b.x, a.y + b.y, a.z);
    }
    public static VectorI2 operator +(VectorI2 a, VectorI2 b) {
        return new VectorI2(a.x + b.x, a.y + b.y);
    }
    public static Vector2 operator +(Vector2 a, VectorI2 b) {
        return new Vector2(a.x + b.x, a.y + b.y);
    }
    public static Vector2 operator +(VectorI2 a, Vector2 b) {
        return new Vector2(a.x + b.x, a.y + b.y);
    }

    public static Vector2 operator +(VectorI2 a, float d) {
        return new Vector2(a.x + d, a.y + d);
    }

    public static bool operator ==(VectorI2 lhs, VectorI2 rhs) {
        return lhs.x == rhs.x && lhs.y == rhs.y;
    }
    public static bool operator ==(Vector2 lhs, VectorI2 rhs) {
        return lhs.x == rhs.x && lhs.y == rhs.y;
    }
    public static bool operator ==(VectorI2 lhs, Vector2 rhs) {
        return lhs.x == rhs.x && lhs.y == rhs.y;
    }

    public static implicit operator VectorI2(VectorI3 v) {
        return new VectorI2(v.x, v.y);
    }
    public static implicit operator VectorI3(VectorI2 v) {
        return new VectorI3(v.x, v.y, 0);
    }
    public static implicit operator VectorI2(Vector2 v) {
        return new VectorI2(v);
    }
    public static implicit operator VectorI2(Vector4 v) {
        return new VectorI2(v.x, v.y);
    }
    public static implicit operator Vector2(VectorI2 v) {
        return new Vector2(v.x, v.y);
    }
    public static implicit operator Vector3(VectorI2 v) {
        return new Vector3(v.x, v.y, 0);
    }
    public static implicit operator Vector4(VectorI2 v) {
        return new Vector4(v.x, v.y, 0);
    }

    public override bool Equals(object obj) {
        if(obj.GetType() == typeof(VectorI2)) {
            VectorI2 v = (VectorI2)obj;
            return this.x == v.x && this.y == v.y;
        }

        return false;
    }
    public override int GetHashCode() {
        //return base.GetHashCode();
        return x.GetHashCode() ^ y.GetHashCode() << 2;
    }
    public override string ToString() {
        return "(" + x + ", " + y + ")";
    }
    #endregion
}

public static class FastMath {
    public static float Ceil(float x) {
        return Mathf.Ceil(x);
        //return x < 0 ? (int)x : (int)(++x);
        //return x > 0 ? (int)(++x) : (int)x;
    }

    public static int CeilToInt(float x) {
        return Mathf.CeilToInt(x);
        //return x < 0 ? (int)x : (int)(++x);
        //return x > 0 ? (int)(++x) : (int)x;
    }

    public static float Floor(float x) {
        return Mathf.Floor(x);
        //return x > 0 ? (int)x : (int)(--x);
        //return x < 0 ? (int)(--x) : (int)x;
    }

    public static int FloorToInt(float x) {
        return Mathf.FloorToInt(x);
        //return x > 0 ? (int)x : (int)(--x);
        //return x < 0 ? (int)(--x) : (int)x;
    }

    public static float Round(float x) {
        //return Mathf.Round(x);
        return x > 0 ? (int)(x + .5f) : (int)(x - .5f);
    }

    public static int RoundToInt(float x) {
        //return Mathf.RoundToInt(x);
        return x > 0 ? (int)(x + .5f) : (int)(x - .5f);
    }
}
/*class VectorIComparer : IEqualityComparer<VectorI3> {
	public bool Equals(VectorI3 a, VectorI3 b) {
		return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
	}
	public int GetHashCode(VectorI3 a) {
		return base.GetHashCode();
	}
}

class VectorI2Comparer : IEqualityComparer<VectorI2> {
	public bool Equals(VectorI2 a, VectorI2 b) {
		return (a.x == b.x) && (a.y == b.y);
	}
	public int GetHashCode(VectorI2 a) {
		return base.GetHashCode();
	}
}*/
