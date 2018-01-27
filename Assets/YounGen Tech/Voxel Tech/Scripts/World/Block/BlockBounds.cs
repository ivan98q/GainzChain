using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.VoxelTech {
    public struct BlockBounds {

        public Block block;
        public Bounds bounds;
        public Vector3 normal;
        public VectorI3 plane;
        public int axisA, axisB;
        public bool counterClockwise;
        public int faceIndex;

        #region Properties
        public Vector3 VertexBottomLeft {
            get { return bounds.min; }
        }

        public Vector3 VertexBottomRight {
            get {
                Vector3 position = bounds.min;

                position[axisA] = bounds.max[axisA];

                return position;
            }
        }

        public Vector3 VertexTopLeft {
            get {
                Vector3 position = bounds.min;

                position[axisB] = bounds.max[axisB];

                return position;
            }
        }

        public Vector3 VertexTopRight {
            get { return bounds.max; }
        }
        #endregion

        public BlockBounds(Block block, Bounds bounds, Vector3 normal, VectorI3 plane, int axisA, int axisB, int faceIndex) {
            this.block = block;
            this.bounds = bounds;
            this.normal = normal;
            this.plane = plane;
            this.axisA = axisA;
            this.axisB = axisB;
            this.faceIndex = faceIndex;

            //counterClockwise = Mathf.Min(normal.x, normal.y, normal.z) < 0;
            //counterClockwise = false;
            //counterClockwise = normal.x > 0 || normal.y < 0 || normal.z > 0;
            counterClockwise = normal.x < 0 || normal.y < 0 || normal.z > 0;
        }

        public void AddIndexArray(List<int> indices, int indexOffset, bool flip = false) {
            if(flip) {
                indices.Add(indexOffset + 0);
                indices.Add(indexOffset + 2);
                indices.Add(indexOffset + 1);
                indices.Add(indexOffset + 3);
                indices.Add(indexOffset + 1);
                indices.Add(indexOffset + 2);
            }
            else {
                indices.Add(indexOffset + 0);
                indices.Add(indexOffset + 1);
                indices.Add(indexOffset + 2);
                indices.Add(indexOffset + 3);
                indices.Add(indexOffset + 2);
                indices.Add(indexOffset + 1);
            }
        }

        public void AddNormalsArray(List<Vector3> normals) {
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
        }

        public void AddUVArray(List<Vector2> uv) {
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(bounds.size[axisA], 0));
            uv.Add(new Vector2(0, bounds.size[axisB]));
            uv.Add(new Vector2(bounds.size[axisA], bounds.size[axisB]));
        }

        public void AddVertexArray(List<Vector3> vertices) {
            vertices.Add(VertexBottomLeft);
            vertices.Add(VertexBottomRight);
            vertices.Add(VertexTopLeft);
            vertices.Add(VertexTopRight);
        }

        public Color[] GetColorArray() {
            return new Color[] {
                Color.white,
                Color.white,
                Color.white,
                Color.white
            };
        }
        public Color[] GetColorArray(Color color) {
            return new Color[] {
                color,
                color,
                color,
                color
            };
        }

        public int[] GetIndexArray(int indexOffset, bool flip = false) {
            if(flip)
                return new int[] {
                    indexOffset + 0,
                    indexOffset + 2,
                    indexOffset + 1,
                    indexOffset + 3,
                    indexOffset + 1,
                    indexOffset + 2,
                };
            else
                return new int[] {
                    indexOffset + 0,
                    indexOffset + 1,
                    indexOffset + 2,
                    indexOffset + 3,
                    indexOffset + 2,
                    indexOffset + 1,
                };
        }

        public Vector3[] GetNormalsArray() {
            return new Vector3[] { normal, normal, normal, normal };
        }

        public Vector3 GetQuadVertex(int index) {
            switch(index) {
                default: return VertexBottomLeft;
                case 1: return VertexBottomRight;
                case 2: return VertexTopLeft;
                case 3: return VertexTopRight;
            }
        }

        public Vector3 GetTriangleVertex(int index) {
            switch(index) {
                default: return VertexBottomLeft;
                case 1: return VertexBottomRight;
                case 2: return VertexTopLeft;
                case 3: return VertexTopRight;
                case 4: return VertexTopLeft;
                case 5: return VertexBottomRight;
            }
        }

        public int GetTriangleVertexIndex(int index) {
            switch(index) {
                default: return 0;
                case 1: return 1;
                case 2: return 2;
                case 3: return 3;
                case 4: return 2;
                case 5: return 1;
            }
        }

        public Vector3[] GetVertexArray() {
            return new Vector3[] {
                VertexBottomLeft,
                VertexBottomRight,
                VertexTopLeft,
                VertexTopRight
            };
        }

        public Vector2[] GetUVArray() {
            return new Vector2[] {
                new Vector2(0, 0),
                new Vector2(bounds.size[axisA], 0),
                new Vector2(0, bounds.size[axisB]),
                new Vector2(bounds.size[axisA], bounds.size[axisB])
            };
        }
    }
}