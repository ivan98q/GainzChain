using System;
using UnityEngine;

public enum Direction {
    Left = 0,
    Right = 1,
    Bottom = 2, Down = 2,
    Top = 3, Up = 3,
    Backward = 4, Back = 4,
    Forward = 5, Front = 5
}

[Flags]
public enum DirectionFlag {
    None = 0,
    Left = 1,
    Right = 1 << 1,
    Bottom = 1 << 2, Down = 1 << 2,
    Top = 1 << 3, Up = 1 << 3,
    Backward = 1 << 4, Back = 1 << 4,
    Forward = 1 << 5, Front = 1 << 5
}

[Flags]
public enum CubeDirectionFlag {
    None = 0,
    LeftDownBack = 1 << 1,
    DownBack = 1 << 2,
    RightDownBack = 1 << 3,
    LeftBack = 1 << 4,
    Back = 1 << 5,
    RightBack = 1 << 6,
    LeftUpBack = 1 << 7,
    UpBack = 1 << 8,
    RightUpBack = 1 << 9,
    LeftDown = 1 << 10,
    Down = 1 << 11,
    RightDown = 1 << 12,
    Left = 1 << 13,
    Right = 1 << 14,
    LeftUp = 1 << 15,
    Up = 1 << 16,
    RightUp = 1 << 17,
    LeftDownForward = 1 << 18,
    DownForward = 1 << 19,
    RightDownForward = 1 << 20,
    LeftForward = 1 << 21,
    Forward = 1 << 22,
    RightForward = 1 << 23,
    LeftUpForward = 1 << 24,
    UpForward = 1 << 25,
    RightUpForward = 1 << 26,

    //All = 134217726,
    All = EdgesAndCorners | Faces,

    Edges = DownBack | LeftBack | RightBack | UpBack | LeftDown | RightDown | LeftUp | RightUp | DownForward | LeftForward | RightForward | UpForward,
    Corners = LeftDownBack | RightDownBack | LeftUpBack | RightUpBack | LeftDownForward | RightDownForward | LeftUpForward | RightUpForward,
    EdgesAndCorners = Edges | Corners,
    Faces = Left | Right | Down | Up | Back | Forward,

    AllLeft = LeftDownBack | LeftDown | LeftDownForward | LeftBack | Left | LeftForward | LeftUpBack | LeftUp | LeftUpForward,
    AllRight = RightDownBack | RightDown | RightDownForward | RightBack | Right | RightForward | RightUpBack | RightUp | RightUpForward,

    AllDown = LeftDownBack | DownBack | RightDownBack | LeftDown | Down | RightDown | LeftDownForward | DownForward | RightDownForward,
    AllUp = LeftUpBack | UpBack | RightUpBack | LeftUp | Up | RightUp | LeftUpForward | UpForward | RightUpForward,

    AllBack = LeftDownBack | DownBack | RightDownBack | LeftBack | Back | RightBack | LeftUpBack | UpBack | RightUpBack,
    AllForward = LeftDownForward | DownForward | RightDownForward | LeftForward | Forward | RightForward | LeftUpForward | UpForward | RightUpForward
}

public static class DirectionHelper {
    public const int DirectionCount = 26;
    public const int FaceCount = 6;

    public static VectorI3[] cubeDirections = new VectorI3[] {
        new VectorI3(-1, -1, -1),
        new VectorI3(0, -1, -1),
        new VectorI3(1, -1, -1),
        new VectorI3(-1, 0, -1),
        new VectorI3(0, 0, -1),
        new VectorI3(1, 0, -1),
        new VectorI3(-1, 1, -1),
        new VectorI3(0, 1, -1),
        new VectorI3(1, 1, -1),
        new VectorI3(-1, -1, 0),
        new VectorI3(0, -1, 0),
        new VectorI3(1, -1, 0),
        new VectorI3(-1, 0, 0),
        new VectorI3(1, 0, 0),
        new VectorI3(-1, 1, 0),
        new VectorI3(0, 1, 0),
        new VectorI3(1, 1, 0),
        new VectorI3(-1, -1, 1),
        new VectorI3(0, -1, 1),
        new VectorI3(1, -1, 1),
        new VectorI3(-1, 0, 1),
        new VectorI3(0, 0, 1),
        new VectorI3(1, 0, 1),
        new VectorI3(-1, 1, 1),
        new VectorI3(0, 1, 1),
        new VectorI3(1, 1, 1)
    };

    public static int BitRoot(int n) {
        return (int)Mathf.Log(n, 2);
    }

    public static int BitCount(int n) {
        int count = 0;

        while(n != 0) {
            count++;
            n &= (n - 1);
        }

        return count;
    }

    public static VectorI3 DirectionVector(this Direction direction) {
        switch(direction) {
            default: return new VectorI3(-1, 0, 0);
            case Direction.Right: return new VectorI3(1, 0, 0);
            case Direction.Down: return new VectorI3(0, -1, 0);
            case Direction.Up: return new VectorI3(0, 1, 0);
            case Direction.Back: return new VectorI3(0, 0, -1);
            case Direction.Forward: return new VectorI3(0, 0, 1);
        }
    }

    public static Direction ToDirection(int direction) {
        return (Direction)Enum.Parse(typeof(Direction), direction.ToString());
    }

    public static DirectionFlag GetDirectionFlag(int direction) {
        return (DirectionFlag)Enum.Parse(typeof(DirectionFlag), direction.ToString());
    }

    public static bool HasDirection(this DirectionFlag direction, DirectionFlag value) {
        return (direction & value) != DirectionFlag.None;
    }

    public static bool HasDirection(this CubeDirectionFlag direction, CubeDirectionFlag value) {
        return (direction & value) != CubeDirectionFlag.None;
    }

    public static CubeDirectionFlag FaceToCubeDirection(this int face) {
        switch(face) {
            default: return CubeDirectionFlag.None;
            case 0: return CubeDirectionFlag.Left;
            case 1: return CubeDirectionFlag.Right;
            case 2: return CubeDirectionFlag.Down;
            case 3: return CubeDirectionFlag.Up;
            case 4: return CubeDirectionFlag.Back;
            case 5: return CubeDirectionFlag.Forward;
        }
    }

    public static void ForEach(this CubeDirectionFlag directions, Action action) {
        for(int i = 0; i < 26; i++)
            if(((int)directions & (1 << (i + 1))) != 0) action();
    }

    public static CubeDirectionFlag ToCubeDirection(this int face) {
        switch(face) {
            default: return CubeDirectionFlag.None;
            case 0: return CubeDirectionFlag.LeftDownBack;
            case 1: return CubeDirectionFlag.DownBack;
            case 2: return CubeDirectionFlag.RightDownBack;
            case 3: return CubeDirectionFlag.LeftBack;
            case 4: return CubeDirectionFlag.Back;
            case 5: return CubeDirectionFlag.RightBack;
            case 6: return CubeDirectionFlag.LeftUpBack;
            case 7: return CubeDirectionFlag.UpBack;
            case 8: return CubeDirectionFlag.RightUpBack;
            case 9: return CubeDirectionFlag.LeftDown;
            case 10: return CubeDirectionFlag.Down;
            case 11: return CubeDirectionFlag.RightDown;
            case 12: return CubeDirectionFlag.Left;
            case 13: return CubeDirectionFlag.Right;
            case 14: return CubeDirectionFlag.LeftUp;
            case 15: return CubeDirectionFlag.Up;
            case 16: return CubeDirectionFlag.RightUp;
            case 17: return CubeDirectionFlag.LeftDownForward;
            case 18: return CubeDirectionFlag.DownForward;
            case 19: return CubeDirectionFlag.RightDownForward;
            case 20: return CubeDirectionFlag.LeftForward;
            case 21: return CubeDirectionFlag.Forward;
            case 22: return CubeDirectionFlag.RightForward;
            case 23: return CubeDirectionFlag.LeftUpForward;
            case 24: return CubeDirectionFlag.UpForward;
            case 25: return CubeDirectionFlag.RightUpForward;
        }
    }
    public static CubeDirectionFlag ToCubeDirection(this VectorI3 vector) {
        if(vector == VectorI3.zero) return CubeDirectionFlag.None;

        string enumName = "";

        if(vector.x != 0) enumName += Mathf.Sign(vector.x) > 0 ? "Left" : "Right";
        if(vector.y != 0) enumName += Mathf.Sign(vector.y) > 0 ? "Up" : "Down";
        if(vector.z != 0) enumName += Mathf.Sign(vector.z) > 0 ? "Forward" : "Back";

        return (CubeDirectionFlag)Enum.Parse(typeof(CubeDirectionFlag), enumName);
    }

    public static string ToCubeDirectionString(this VectorI3 vector) {
        if(vector == VectorI3.zero) return "";

        string enumName = "";

        if(vector.x != 0) enumName += Mathf.Sign(vector.x) > 0 ? "Left" : "Right";
        if(vector.y != 0) enumName += Mathf.Sign(vector.y) > 0 ? "Up" : "Down";
        if(vector.z != 0) enumName += Mathf.Sign(vector.z) > 0 ? "Forward" : "Back";

        return enumName;
    }

    public static VectorI3 ToDirectionVector(this CubeDirectionFlag direction) {
        return BitCount((int)direction) == 1 ? cubeDirections[BitRoot((int)direction) - 1] : VectorI3.zero;
    }
    public static VectorI3 ToDirectionVector(this int index) {
        return cubeDirections[index];
    }

    public static VectorI3 ToFace(this int index) {
        switch(index) {
            default: return VectorI3.left;
            case 1: return VectorI3.right;
            case 2: return VectorI3.down;
            case 3: return VectorI3.up;
            case 4: return VectorI3.back;
            case 5: return VectorI3.forward;
        }
    }

    public static int ToFaceIndex(this CubeDirectionFlag direction) {
        switch(direction) {
            default: return -1;
            case CubeDirectionFlag.Left: return 0;
            case CubeDirectionFlag.Right: return 1;
            case CubeDirectionFlag.Down: return 2;
            case CubeDirectionFlag.Up: return 3;
            case CubeDirectionFlag.Back: return 4;
            case CubeDirectionFlag.Forward: return 5;
        }
    }

    public static VectorI3 ToPlaneVector(this int index) {
        switch(index) {
            default: return new VectorI3(0, 1, -1); //+Y-Z
            case 1: return new VectorI3(0, 1, 1);   //+Y+Z;
            case 2: return new VectorI3(-1, 0, 1);  //-X+Z;
            case 3: return new VectorI3(-1, 0, -1); //-X-Z;
            case 4: return new VectorI3(-1, 1, 0);  //-X+Y;
            case 5: return new VectorI3(1, 1, 0);   //+X+Y;
        }
    }
    public static VectorI3 ToPlaneVector(this CubeDirectionFlag direction) {
        switch(direction) {
            default: return new VectorI3(0, 1, -1); //+Y-Z
            case CubeDirectionFlag.Right: return new VectorI3(0, 1, 1);   //+Y+Z;
            case CubeDirectionFlag.Down: return new VectorI3(-1, 0, 1);  //-X+Z;
            case CubeDirectionFlag.Up: return new VectorI3(-1, 0, -1); //-X-Z;
            case CubeDirectionFlag.Back: return new VectorI3(-1, 1, 0);  //-X+Y;
            case CubeDirectionFlag.Forward: return new VectorI3(1, 1, 0);   //+X+Y;
        }
    }
}