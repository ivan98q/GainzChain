using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class MathHelper {
    /// <summary>
    /// Get which face of a rotated block will be on a side (face parameter)
    /// </summary>
    public static int GetRotatedFace(this int rotationIndex, int face) {
        Quaternion rotation = rotationIndex.FlatIndexToRotation();
        //Vector3 direction = VectorI3.FastRound(Quaternion.Inverse(rotation) * face.IntToDirection());
        Vector3 direction = VectorI3.FastRound(Quaternion.Inverse(rotation) * GetCubeFace(face));

        return direction.DirectionToIndex();
    }

    /// <summary>
    /// Get which face of a rotated block will be on a side (face parameter)
    /// </summary>
    public static int GetRotatedFace(this byte rotationIndex, int face) {
        Quaternion rotation = rotationIndex.FlatIndexToRotation();
        //Vector3 direction = VectorI3.FastRound(Quaternion.Inverse(rotation) * face.IntToDirection());
        Vector3 direction = VectorI3.FastRound(Quaternion.Inverse(rotation) * GetCubeFace(face));

        return direction.DirectionToIndex();
    }

    [Obsolete("Ew gross don't use this")]
    public static int GetRotatedFaceOld(this byte rotationIndex, int face) {
        Quaternion rotation = rotationIndex.FlatIndexToRotation();

        for(int i = 0; i < 6; i++) {
            VectorI3 direction = VectorI3.FastRound(rotation * i.IntToDirection());
            int trueFace = direction.DirectionToIndex();

            if(face == trueFace) return i;
        }

        return 0;
    }

    public static int RotationToFlatIndex(this Quaternion rotation) {
        VectorI3 cameraRotation = VectorI3.zero;

        cameraRotation.x = Mathf.RoundToInt(MathHelper.WrapAngle((int)rotation.eulerAngles.x + 45) / 90);
        cameraRotation.y = Mathf.RoundToInt(MathHelper.WrapAngle((int)rotation.eulerAngles.y + 45) / 90);
        cameraRotation.z = Mathf.RoundToInt(MathHelper.WrapAngle((int)rotation.eulerAngles.z + 45) / 90);

        return cameraRotation.FlatIndex(new VectorI3(4, 4, 4));
    }

    //public static int RotationIndexToFaceIndex(this int rotationIndex) {

    //}

    public static Quaternion FlatIndexToRotation(this int index) {
        return Quaternion.Euler(
            (index % 4) * 90,
            (index / 4 % 4) * 90,
            (index / (4 * 4) % 4) * 90
        );
    }
    public static Quaternion FlatIndexToRotation(this byte index) {
        //Vector3 rotation = ((int)index).FlatTo3DIndex(new VectorI3(4, 4, 4)) * 90;
        //Quaternion rot = Quaternion.identity;

        //rot.eulerAngles = new Vector3((index % 4) * 90, (index / 4 % 4) * 90, (index / (4 * 4) % 4) * 90);

        //return rot;
        return Quaternion.Euler(
            (index % 4) * 90,
            (index / 4 % 4) * 90,
            (index / (4 * 4) % 4) * 90
        );
    }

    public static float WrapAngle(float angle) {
        float returnAngle = angle % 360;

        if(angle < 0) returnAngle += 360;

        return returnAngle;
    }

    public static int WrapAngle(int angle) {
        int returnAngle = angle % 360;

        if(angle < 0) returnAngle += 360;

        return returnAngle;
    }

    public static Vector3 SnapTo(this Vector3 v3, float snapAngle) {
        float angle = Vector3.Angle(v3, Vector3.up);
        if(angle < snapAngle / 2.0f) // Cannot do cross product
            return Vector3.up * v3.magnitude; // with angles 0 & 180
        if(angle > 180.0f - snapAngle / 2.0f)
            return Vector3.down * v3.magnitude;

        float t = Mathf.Round(angle / snapAngle);

        float deltaAngle = (t * snapAngle) - angle;

        Vector3 axis = Vector3.Cross(Vector3.up, v3);
        Quaternion q = Quaternion.AngleAxis(deltaAngle, axis);
        return q * v3;
    }

    /// <summary>
    /// IDK Anymore
    /// </summary>
    public static float SDAS(Transform transform, Transform target, Vector3 forward) {
        return Vector3.Dot(Vector3.Cross(forward, (target.position - transform.position).normalized), Vector3.right);
    }

    /// <summary>
    /// Yaw
    /// </summary>
    public static float Bearing(Transform transform, Transform target, Vector3 forward) {
        return Vector3.Dot(Vector3.Cross(forward, (target.position - transform.position).normalized), Vector3.up);
    }
    public static float Bearing(Transform transform, Vector3 target, Vector3 forward) {
        return Vector3.Dot(Vector3.Cross(forward, (target - transform.position).normalized), Vector3.up);
    }

    /// <summary>
    /// Pitch
    /// </summary>
    public static float Elevation(Transform transform, Transform target, Vector3 forward) {
        return Vector3.Dot(Vector3.Cross(forward, (target.position - transform.position).normalized), Vector3.forward);
    }
    public static float Elevation(Transform transform, Vector3 target, Vector3 forward) {
        return Vector3.Dot(Vector3.Cross(forward, (target - transform.position).normalized), Vector3.forward);
    }
    public static float Elevation(Vector3 position, Vector3 target, Vector3 forward) {
        return Vector3.Dot(Vector3.Cross(forward, (target - position).normalized), Vector3.forward);
    }

    public static float DirectionOnAxis(Transform transform, Transform target, Vector3 forward, Vector3 axis) {
        return Vector3.Dot(Vector3.Cross(forward, (target.position - transform.position).normalized), axis);
    }

    public static float ForwardDirection(Transform transform, Transform target, Vector3 forward) {
        return Vector3.Dot(forward, (target.position - transform.position).normalized);
    }

    public static bool ObjectInFrustum(Transform transform, Transform target, float nearClip, float farClip, float horizontalFOV, float verticalFOV) {
        float dist = Vector3.Distance(target.position, transform.position);

        if(dist < nearClip || dist > farClip) return false;

        float frustumHeight = 2f * dist * Mathf.Tan(horizontalFOV * .5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * (horizontalFOV / verticalFOV);

        Vector3 topLeft = transform.TransformPoint(-frustumWidth * .5f, frustumHeight * .5f, dist);
        Vector3 topRight = transform.TransformPoint(frustumWidth * .5f, frustumHeight * .5f, dist);
        Vector3 bottomLeft = transform.TransformPoint(-frustumWidth * .5f, -frustumHeight * .5f, dist);

        float dotLeft = Vector3.Dot(transform.right, (target.position - topLeft).normalized);
        float dotRight = Vector3.Dot(-transform.right, (target.position - topRight).normalized);

        float dotBottom = Vector3.Dot(transform.up, (target.position - bottomLeft).normalized);
        float dotTop = Vector3.Dot(-transform.up, (target.position - topLeft).normalized);

        return dotLeft > 0 && dotRight > 0 && dotBottom > 0 && dotTop > 0;
    }

    public static VectorI3 ArrayToDirection(this Vector3 arrayPosition, Vector3 arraySize) {
        if(arrayPosition.x == 0) { arrayPosition.x = -1; }
        else if(arrayPosition.x == arraySize.x - 1) { arrayPosition.x = 1; }
        else { arrayPosition.x = 0; }

        if(arrayPosition.y == 0) { arrayPosition.y = -1; }
        else if(arrayPosition.y == arraySize.y - 1) { arrayPosition.y = 1; }
        else { arrayPosition.y = 0; }

        if(arrayPosition.z == 0) { arrayPosition.z = -1; }
        else if(arrayPosition.z == arraySize.z - 1) { arrayPosition.z = 1; }
        else { arrayPosition.z = 0; }

        return arrayPosition;
    }

    public static bool IsEdgeOfArray(this VectorI3 arrayPosition, VectorI3 arraySize) {
        arraySize -= VectorI3.one;
        return (arrayPosition.x == 0 || arrayPosition.x == arraySize.x || arrayPosition.y == 0 || arrayPosition.y == arraySize.y || arrayPosition.z == 0 || arrayPosition.z == arraySize.z);
    }
    public static bool IsCornerEdgeOfArray(this VectorI3 arrayPosition, VectorI3 arraySize) {
        if(arrayPosition.x == 0 || arrayPosition.x == arraySize.x - 1) {
            if(arrayPosition.y == 0 || arrayPosition.y == arraySize.y - 1) { return true; }
            else if(arrayPosition.z == 0 || arrayPosition.z == arraySize.z - 1) { return true; }
        }

        if(arrayPosition.y == 0 || arrayPosition.y == arraySize.y - 1) {
            if(arrayPosition.x == 0 || arrayPosition.x == arraySize.x - 1) { return true; }
            else if(arrayPosition.z == 0 || arrayPosition.z == arraySize.z - 1) { return true; }
        }

        if(arrayPosition.z == 0 || arrayPosition.z == arraySize.z - 1) {
            if(arrayPosition.x == 0 || arrayPosition.x == arraySize.x - 1) { return true; }
            else if(arrayPosition.y == 0 || arrayPosition.y == arraySize.y - 1) { return true; }
        }

        return false;
    }
    public static bool IsCornerOfArray(this VectorI3 arrayPosition, VectorI3 arraySize) {
        return
            ((arrayPosition.x == 0) || (arrayPosition.x == arraySize.x - 1)) &&
            ((arrayPosition.y == 0) || (arrayPosition.y == arraySize.y - 1)) &&
            ((arrayPosition.z == 0) || (arrayPosition.z == arraySize.z - 1));
    }

    public static bool ArrayOutOfBounds(this VectorI3 index, VectorI3 arraySize) {
        //Debug.LogWarning("ArrayOutOfBounds("+index+", "+arraySize+") x = " + (index.x > arraySize.x - 1 || index.x < 0) + " y = " + (index.y > arraySize.y - 1 || index.y < 0) + " z = " + (index.z > arraySize.z - 1 || index.z < 0));
        //return index.x > (arraySize.x - 1) || index.x < 0 ||
        //       index.y > (arraySize.y - 1) || index.y < 0 ||
        //       index.z > (arraySize.z - 1) || index.z < 0;
        
        // TODO Check ArrayOutOfBounds commented code

        if(index.x > (arraySize.x - 1) || index.x < 0) return true;
        if(index.y > (arraySize.y - 1) || index.y < 0) return true;
        if(index.z > (arraySize.z - 1) || index.z < 0) return true;

        return false;
    }
    public static bool ArrayOnBounds(this VectorI3 index, VectorI3 arraySize) {
        if(index.x == arraySize.x || index.x == 0) return true;
        if(index.y == arraySize.y || index.y == 0) return true;
        if(index.z == arraySize.z || index.z == 0) return true;

        return false;
    }

    public static int Wrap(this int integer, int wrapTo) {
        int i = integer & wrapTo;

        if(i < 0) i += wrapTo;

        return i;
    }

    public static VectorI3 Wrap3DIndex(this VectorI3 positionIndex, VectorI3 arraySize) {
        VectorI3 newDirection = new VectorI3(
            positionIndex.x % arraySize.x,
            positionIndex.y % arraySize.y,
            positionIndex.z % arraySize.z
        );

        if(newDirection.x < 0) { newDirection.x = arraySize.x + newDirection.x; }
        if(newDirection.y < 0) { newDirection.y = arraySize.y + newDirection.y; }
        if(newDirection.z < 0) { newDirection.z = arraySize.z + newDirection.z; }

        return newDirection;
    }

    public static VectorI3 Wrap3DIndex(this VectorI3 positionIndex, VectorI3 direction, VectorI3 arraySize) {
        VectorI3 newDirection = new VectorI3(
            ((positionIndex.x + direction.x) % arraySize.x),
            ((positionIndex.y + direction.y) % arraySize.y),
            ((positionIndex.z + direction.z) % arraySize.z)
            );

        if(newDirection.x < 0) { newDirection.x = arraySize.x + newDirection.x; }
        if(newDirection.y < 0) { newDirection.y = arraySize.y + newDirection.y; }
        if(newDirection.z < 0) { newDirection.z = arraySize.z + newDirection.z; }

        return newDirection;
    }

    public static int WrapIndexMod(this int index, int endIndex, int maxSize) {
        return (endIndex + index) % maxSize;
    }

    public static int FlatIndex(this VectorI2 index, VectorI2 size) {
        return index.x + index.y * size.x;
    }
    public static int FlatIndex(this VectorI3 index, VectorI3 size) {
        return index.x + index.y * size.x + index.z * size.x * size.y;
    }

    public static VectorI2 FlatTo2DIndex(this int index, VectorI2 size) {
        return new VectorI2(
            index % size.x,
            index / size.x % size.y
        );
    }
    public static VectorI3 FlatTo3DIndex(this int index, VectorI3 size) {
        return new VectorI3(
            index % size.x,
            index / size.x % size.y,
            index / (size.x * size.y) % size.z
        );
    }

    public static Vector3 Distance3(this Vector3 vector, Vector3 from) {
        return new Vector3(
            Mathf.Abs(vector.x - from.x),
            Mathf.Abs(vector.y - from.y),
            Mathf.Abs(vector.z - from.z)
        );
    }

    public static VectorI3 Distance3(this VectorI3 vector, VectorI3 from) {
        return new VectorI3(
            Mathf.Abs(vector.x - from.x),
            Mathf.Abs(vector.y - from.y),
            Mathf.Abs(vector.z - from.z)
        );
    }

    public const int directionCount = 27;
    public const int faceCount = 6;

    #region Direction Array
    public static readonly int[] opposite = new int[]{
        1, 0,
        3, 2,
        5, 4,

        7, 6,
        9, 8,
        11, 10,
        13, 12,

        15, 14,
        17, 16,
        19, 18,
        21, 20,
        23, 22,
        25, 24
    };

    public static readonly VectorI3[] direction = new VectorI3[]{
        VectorI3.left,
        VectorI3.right,
        VectorI3.down,
        VectorI3.up,
        VectorI3.back,
        VectorI3.forward,
		
	    //Corner
	    new VectorI3(-1,-1,-1),
        new VectorI3(1,1,1),

        new VectorI3(-1,1,-1),
        new VectorI3(1,-1,1),

        new VectorI3(-1,-1,1),
        new VectorI3(1,1,-1),

        new VectorI3(-1,1,1),
        new VectorI3(1,-1,-1),

	    //Edge Corner
	    new VectorI3(-1,0,-1), //Left Back
	    new VectorI3(1,0,1), //Right Front
	    new VectorI3(-1,0,1), //Left Front
	    new VectorI3(1,0,-1), //Right Back

	    new VectorI3(-1,-1,0), //Left Bottom
	    new VectorI3(1,1,0), //Right Top
	    new VectorI3(-1,1,0), //Left Top
	    new VectorI3(1,-1,0), //Right Bottom

	    new VectorI3(0,-1,-1), //Bottom Front
	    new VectorI3(0,1,1), //Top Back
	    new VectorI3(0,-1,1), //Bottom Back
	    new VectorI3(0,1,-1), //Top Front
	};
    #endregion

    //public static readonly VectorI3[] direction = new VectorI3[]{
    //    VectorI3.left,
    //    VectorI3.right,
    //    VectorI3.up,
    //    VectorI3.down,
    //    VectorI3.forward,
    //    VectorI3.back,

    //    //Corner
    //    new VectorI3(-1,1,1),
    //    new VectorI3(1,-1,-1),

    //    new VectorI3(-1,-1,1),
    //    new VectorI3(1,1,-1),

    //    new VectorI3(-1,1,-1),
    //    new VectorI3(1,-1,1),

    //    new VectorI3(-1,-1,-1),
    //    new VectorI3(1,1,1),

    //    //Edge Corner
    //    new VectorI3(-1,0,1), //Left Front
    //    new VectorI3(1,0,-1), //Right Back
    //    new VectorI3(-1,0,-1), //Left Back
    //    new VectorI3(1,0,1), //Right Front

    //    new VectorI3(-1,1,0), //Left Top
    //    new VectorI3(1,-1,0), //Right Bottom
    //    new VectorI3(-1,-1,0), //Left Bottom
    //    new VectorI3(1,1,0), //Right Top

    //    new VectorI3(0,1,1), //Top Front
    //    new VectorI3(0,-1,-1), //Bottom Back
    //    new VectorI3(0,1,-1), //Top Back
    //    new VectorI3(0,-1,1), //Bottom Front
    //};

    public static VectorI3 GetCubeFace(int index) {
        switch(index) {
            default: return VectorI3.zero;
            case 0: return VectorI3.left;
            case 1: return VectorI3.right;
            case 2: return VectorI3.down;
            case 3: return VectorI3.up;
            case 4: return VectorI3.back;
            case 5: return VectorI3.forward;
        }
    }

    public static VectorI3 IntToDirection(this int index) {
        if(index < 0) index = 0;
        return direction[index];
    }

    public static int DirectionToIndex2(this VectorI3 direction) {
        direction += VectorI3.one;

        direction.x = Mathf.Clamp(direction.x, 0, 2);
        direction.y = Mathf.Clamp(direction.y, 0, 2);
        direction.z = Mathf.Clamp(direction.z, 0, 2);

        return direction.FlatIndex(VectorI3.one * 3);
    }
    public static VectorI3 GetDirection(this int index) {
        VectorI3 direction = index.FlatTo3DIndex(VectorI3.one * 3);

        return direction - VectorI3.one;
    }

    public static int DirectionToIndex(this Vector3 dir) {
        for(int i = 0; i < direction.Length; i++)
            if(direction[i] == dir) return i;

        return 0;
    }
    public static int DirectionToIndex(this VectorI3 dir) {
        for(int i = 0; i < direction.Length; i++)
            if(direction[i] == dir) return i;

        return 0;
    }

    public static Vector3 Vector3Clamp(this Vector3 value, Vector3 min, Vector3 max) {
        value.x = value.x > max.x ? value.x = max.x : (value.x < min.x ? value.x = min.x : value.x);
        value.y = value.y > max.y ? value.y = max.y : (value.y < min.y ? value.y = min.y : value.y);
        value.z = value.z > max.z ? value.z = max.z : (value.z < min.z ? value.z = min.z : value.z);

        return value;
    }

    public static int VectorSize(this Vector3 vector) {
        return Mathf.RoundToInt(vector.x * vector.y * vector.z);
    }

    public static Vector3[] OffsetVector3(int count, Vector3[] vectors, Vector3 offset) {
        Vector3[] offsets = new Vector3[count];
        (new List<Vector3>(vectors)).CopyTo(0, offsets, 0, count);

        for(int i = 0; i < count; i++) {
            offsets[i] += offset;
        }

        return offsets;
    }
    public static Vector3[] OffsetVector3(int count, Vector3[] vectors, Vector3[] offset) {
        Vector3[] offsets = new Vector3[count];
        (new List<Vector3>(vectors)).CopyTo(0, offsets, 0, count);

        for(int i = 0; i < offset.Length; i++) {
            offsets[i] += offset[i];
        }

        return offsets;
    }
    public static Vector3[] OffsetVector3(int count, Vector3[] vectors, Vector3[] offset, Vector3 offset2) {
        Vector3[] offsets = new Vector3[count];
        (new List<Vector3>(vectors)).CopyTo(0, offsets, 0, count);

        for(int i = 0; i < count; i++) {
            offsets[i] += offset2 + offset[i];
        }

        return offsets;
    }

    public static Vector2[] OffsetVector2(int count, Vector2[] vectors, Vector2 offset) {
        Vector2[] offsets = new Vector2[count];
        (new List<Vector2>(vectors)).CopyTo(0, offsets, 0, count);

        for(int i = 0; i < count; i++) {
            offsets[i] += offset;
        }

        return offsets;
    }

    public static int[] OffsetInt(int count, int[] ints, int offset) {
        int[] offsets = new int[count];
        (new List<int>(ints)).CopyTo(0, offsets, 0, count);

        for(int i = 0; i < count; i++) {
            offsets[i] += offset;
        }

        return offsets;
    }

    public static Vector3 ParseVector3(string sourceString) {
        string outString;
        Vector3 outVector3;
        string[] splitString = new string[3];

        // Trim extranious parenthesis
        outString = sourceString.Substring(1, sourceString.Length - 2);

        // Split delimted values into an array
        splitString = outString.Split(","[0]);

        // Build new Vector3 from array elements
        outVector3.x = float.Parse(splitString[0]);
        outVector3.y = float.Parse(splitString[1]);
        outVector3.z = float.Parse(splitString[2]);

        return outVector3;
    }

    public static Vector3 SetValue(this Vector3 vector, int index, float value) {
        switch(index) {
            default: vector.x = value; break;
            case 1: vector.y = value; break;
            case 2: vector.z = value; break;
        }

        return vector;
    }

    public static VectorI3 SetValue(this VectorI3 vector, int index, int value) {
        switch(index) {
            default: vector.x = value; break;
            case 1: vector.y = value; break;
            case 2: vector.z = value; break;
        }

        return vector;
    }

    [Obsolete("Use Vector3[index] instead")]
    public static float GetValue(this Vector3 vector, int index) {
        switch(index) {
            default: return vector.x;
            case 1: return vector.y;
            case 2: return vector.z;
        }
    }

    [Obsolete("Use VectorI3[index] instead")]
    public static int GetValue(this VectorI3 vector, int index) {
        switch(index) {
            default: return vector.x;
            case 1: return vector.y;
            case 2: return vector.z;
        }
    }
}