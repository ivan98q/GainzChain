using System.Collections.Generic;
using UnityEngine;

public static class BlockRaycast {
    public struct Hit {
        public VectorI3 position;
        public CubeDirectionFlag face;
    }

    static int FastFloor(double x) {
        return x >= 0 ? (int)x : (int)x - 1;
    }

    public static IEnumerable<Hit> Cast(Vector3 origin, Vector3 direction) {
        int intX = FastFloor(origin.x);
        int intY = FastFloor(origin.y);
        int intZ = FastFloor(origin.z);

        int stepX = (int)Mathf.Sign(direction.x);
        int stepY = (int)Mathf.Sign(direction.y);
        int stepZ = (int)Mathf.Sign(direction.z);

        int boundaryX = intX + (stepX > 0 ? 1 : 0);
        int boundaryY = intY + (stepY > 0 ? 1 : 0);
        int boundaryZ = intZ + (stepZ > 0 ? 1 : 0);

        float tMaxX = (boundaryX - origin.x) / direction.x;
        float tMaxY = (boundaryY - origin.y) / direction.y;
        float tMaxZ = (boundaryZ - origin.z) / direction.z;

        float tDeltaX = stepX / direction.x;
        float tDeltaY = stepY / direction.y;
        float tDeltaZ = stepZ / direction.z;

        CubeDirectionFlag faceX = (stepX > 0 ? CubeDirectionFlag.Left : CubeDirectionFlag.Right);
        CubeDirectionFlag faceY = (stepY > 0 ? CubeDirectionFlag.Down : CubeDirectionFlag.Up);
        CubeDirectionFlag faceZ = (stepZ > 0 ? CubeDirectionFlag.Back : CubeDirectionFlag.Forward);
        CubeDirectionFlag face = faceX;

        while(true) {
            yield return new Hit {
                position = new VectorI3(intX, intY, intZ),
                face = face
            };

            if(tMaxX < tMaxY && tMaxX < tMaxZ) {
                intX += stepX;
                tMaxX += tDeltaX;
                face = faceX;
            }
            else if(tMaxY < tMaxZ) {
                intY += stepY;
                tMaxY += tDeltaY;
                face = faceY;
            }
            else {
                intZ += stepZ;
                tMaxZ += tDeltaZ;
                face = faceZ;
            }
        }
    }

    public static IEnumerable<Hit> CastInt(VectorI3 origin, Vector3 direction) {
        int stepX = (int)Mathf.Sign(direction.x);
        int stepY = (int)Mathf.Sign(direction.y);
        int stepZ = (int)Mathf.Sign(direction.z);

        int boundaryX = origin.x + (stepX > 0 ? 1 : 0);
        int boundaryY = origin.y + (stepY > 0 ? 1 : 0);
        int boundaryZ = origin.z + (stepZ > 0 ? 1 : 0);

        float tMaxX = (boundaryX - origin.x) / direction.x;
        float tMaxY = (boundaryY - origin.y) / direction.y;
        float tMaxZ = (boundaryZ - origin.z) / direction.z;

        float tDeltaX = stepX / direction.x;
        float tDeltaY = stepY / direction.y;
        float tDeltaZ = stepZ / direction.z;

        CubeDirectionFlag faceX = (stepX > 0 ? CubeDirectionFlag.Left : CubeDirectionFlag.Right);
        CubeDirectionFlag faceY = (stepY > 0 ? CubeDirectionFlag.Down : CubeDirectionFlag.Up);
        CubeDirectionFlag faceZ = (stepZ > 0 ? CubeDirectionFlag.Back : CubeDirectionFlag.Forward);
        CubeDirectionFlag face = faceX;

        while(true) {
            yield return new Hit {
                position = origin,
                face = face
            };

            if(tMaxX < tMaxY && tMaxX < tMaxZ) {
                origin.x += stepX;
                tMaxX += tDeltaX;
                face = faceX;
            }
            else if(tMaxY < tMaxZ) {
                origin.y += stepY;
                tMaxY += tDeltaY;
                face = faceY;
            }
            else {
                origin.z += stepZ;
                tMaxZ += tDeltaZ;
                face = faceZ;
            }
        }
    }
}