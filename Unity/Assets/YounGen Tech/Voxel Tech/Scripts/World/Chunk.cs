using System;
using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.VoxelTech {
    //[System.Serializable]
    public class Chunk {

        Block[] _blocks;
        VectorI3 _size;

        #region Properties
        public Vector3 Center {
            get { return Position + (Size * .5f); }
        }

        public bool HasBeenRemoved { get; set; }

        public bool HasChanged { get; protected set; }

        public bool IsGenerating { get; set; }

        public World InWorld { get; private set; }

        public VectorI3 Position { get; set; }

        public VectorI3 Size {
            get { return _size; }
            set {
                _size = value;
                TotalSize = Size.size;
            }
        }

        public int TotalSize { get; private set; }
        #endregion

        public Chunk(World world, VectorI3 size) {
            Size = size;
            _blocks = new Block[TotalSize];
            InWorld = world;
        }

        public bool BlockNeighborFirst(int index, CubeDirectionFlag directions, Func<Block, bool> checkBlock) {
            for(int i = 0; i < 26; i++)
                if(((int)directions & (1 << (i + 1))) != 0) {
                    var newDirection = i.ToCubeDirection();
                    var block = GetBlockNeighbor(index, newDirection);

                    if(checkBlock(block)) return true;
                }

            return false;
        }

        public void BlockNeighborForEach(int index, Action<Block> action) {
            for(int i = 0; i < 26; i++) {
                var newDirection = i.ToCubeDirection();
                var block = GetBlockNeighbor(index, newDirection);

                action(block);
            }

        }
        public void BlockNeighborForEach(int index, CubeDirectionFlag directions, Action<Block> action) {
            for(int i = 0; i < 26; i++)
                if(((int)directions & (1 << (i + 1))) != 0) {
                    var newDirection = i.ToCubeDirection();
                    var block = GetBlockNeighbor(index, newDirection);

                    action(block);
                }
        }

        public CubeDirectionFlag BlockNeighborKeep(int index, CubeDirectionFlag directions) {
            return BlockNeighborKeep(index, directions, GetBlockNeighbor, TestForAir);
        }
        public static CubeDirectionFlag BlockNeighborKeep(int index, CubeDirectionFlag directions, Func<int, CubeDirectionFlag, Block> getBlockNeighbor, Func<Block, bool> airCheckMethod) {
            for(int i = 0; i < 26; i++)
                if(((int)directions & (1 << (i + 1))) != 0) {
                    var newDirection = i.ToCubeDirection();
                    var block = getBlockNeighbor(index, newDirection);
                    // TODO 56kb to GC here
                    if(!airCheckMethod(block)) directions &= ~newDirection;
                }

            return directions;
        }

        public void ClearBlockArray() {
            _blocks = null;
        }

        public Block[] CloneBlockArray() {
            return (Block[])_blocks.Clone();
        }

        public Block GetBlock(int index) {
            return _blocks[index];
        }
        public Block GetBlock(VectorI3 index) {
            return _blocks[index.FlatIndex(Size)];
        }

        public VectorI3 GetBlockLocalPosition(int index) {
            return index.FlatTo3DIndex(Size);
        }

        public Block GetBlockNeighbor(int index, CubeDirectionFlag direction) {
            return GetBlockNeighbor(index, direction.ToDirectionVector());
        }
        public Block GetBlockNeighbor(int index, VectorI3 direction) {
            var position = index.FlatTo3DIndex(Size) + direction;
            int neighborIndex = position.FlatIndex(Size);

            return !position.ArrayOutOfBounds(Size) ?
                _blocks[neighborIndex] :
                InWorld.GetBlock(Position + position);
        }

        public VectorI3 GetBlockWorldPosition(int index) {
            return Position + index.FlatTo3DIndex(Size);
        }

        public List<BlockNeighbor> GetBlockNeighbors(int index, CubeDirectionFlag directions) {
            List<BlockNeighbor> blocks = new List<BlockNeighbor>();

            for(int i = 0; i < 26; i++)
                if(((int)directions & (1 << (i + 1))) != 0) {
                    var newDirection = i.ToCubeDirection();
                    var block = GetBlockNeighbor(index, newDirection);

                    blocks.Add(new BlockNeighbor(block, newDirection));
                }

            return blocks;
        }
        public void GetBlockNeighbors(int index, CubeDirectionFlag directions, List<BlockNeighbor> blocks) {
            blocks.Clear();

            for(int i = 0; i < 26; i++)
                if(((int)directions & (1 << (i + 1))) != 0) {
                    var newDirection = i.ToCubeDirection();
                    var block = GetBlockNeighbor(index, newDirection);

                    blocks.Add(new BlockNeighbor(block, newDirection));
                }
        }

        public CubeDirectionFlag GetDrawableBlockFaces(int index) {
            return BlockNeighborKeep(index, CubeDirectionFlag.Faces);
        }

        public void LoadBlocks(Vector3 position, Func<Vector3, Block> noiseMethod) {
            for(int i = 0; i < TotalSize; i++)
                _blocks[i] = noiseMethod(position + i.FlatTo3DIndex(Size));
        }

        public void ResetBlockArray() {
            _blocks = new Block[TotalSize];
        }

        public void SetBlock(int index, Block block) {
            if(_blocks[index] != block) {
                SetDirty();

                if(index.FlatTo3DIndex(Size).ArrayOnBounds(Size)) {

                }

                _blocks[index] = block;
            }
        }
        public void SetBlock(VectorI3 index, Block block) {
            int flatIndex = index.FlatIndex(Size);

            if(_blocks[flatIndex] != block) {
                SetDirty();
                SetNeighborsDirty();
                _blocks[flatIndex] = block;
            }
        }

        public void SetClean() {
            HasChanged = false;
        }

        public void SetDirty() {
            HasChanged = true;
        }

        public void SetNeighborsDirty() {
            InWorld.ForEachChunkNeighbor(Position, CubeDirectionFlag.Faces, s => s.chunkObject.SetDirty());
        }

        public List<BlockBounds> SpanBlocks() {
            return SpanBlocks(Size, InWorld.BlockDatabaseAsset, GetBlock, GetDrawableBlockFaces);
        }

        public static List<BlockBounds> SpanBlocks(VectorI3 chunkSize, BlockDatabase blockDatabase, Func<int, Block> getBlockAction, Func<int, CubeDirectionFlag> drawDirectionMethod) {
            SpanData[] data = new SpanData[chunkSize.size];

            //Get all faces that can be drawn
            for(int i = 0; i < data.Length; i++) {
                Block block = getBlockAction(i);
                BlockData blockData = blockDatabase.GetBlockData(block.ID);

                data[i] = new SpanData(CubeDirectionFlag.None, block);

                if(blockData.IsSolid)
                    data[i].drawableFaces = drawDirectionMethod(i);
            }

            List<BlockBounds> islands = new List<BlockBounds>();

            //Create islands by looking through each cube direction flag until all are set to none
            var faces = CubeDirectionFlag.Faces;

            for(int directionIndex = 0; directionIndex < 26; directionIndex++)
                if(((int)faces & (1 << (directionIndex + 1))) != 0) {
                    CubeDirectionFlag currentDirection = directionIndex.ToCubeDirection();
                    VectorI3 vectorDirection = currentDirection.ToDirectionVector();
                    int spanXIndex;
                    int spanYIndex;

                    switch(currentDirection) {
                        default:
                        case CubeDirectionFlag.Right:
                            spanXIndex = 2; //YZ Plane
                            spanYIndex = 1; //
                            break;

                        case CubeDirectionFlag.Down:
                        case CubeDirectionFlag.Up:
                            spanXIndex = 0; //XZ Plane
                            spanYIndex = 2;
                            break;

                        case CubeDirectionFlag.Back:
                        case CubeDirectionFlag.Forward:
                            spanXIndex = 0; //XY Plane
                            spanYIndex = 1;
                            break;
                    }

                    VectorI3 plane = VectorI3.zero;
                    plane[spanXIndex] = plane[spanYIndex] = 1;

                    while(true) {
                        bool spannableBlock = false;

                        for(int i = 0; i < data.Length; i++) {
                            var spanData = data[i];

                            if(spanData.drawableFaces.HasDirection(currentDirection)) {
                                spannableBlock = true;

                                VectorI3 startIndex = i.FlatTo3DIndex(chunkSize);
                                Bounds face = new Bounds(startIndex, VectorI3.zero);
                                int width = chunkSize[spanXIndex] - startIndex[spanXIndex];
                                int height = 0;

                                for(int y = startIndex[spanYIndex]; y < chunkSize[spanYIndex]; y++) {
                                    VectorI3 currentIndex = startIndex;
                                    int x;

                                    currentIndex[spanYIndex] = y;

                                    for(x = 0; x < width; x++) {
                                        currentIndex[spanXIndex] = startIndex[spanXIndex] + x;

                                        int currentFlatIndex = currentIndex.FlatIndex(chunkSize);
                                        var currentSpanData = data[currentFlatIndex];
                                        var currentBlock = getBlockAction(currentFlatIndex);

                                        if(spanData.block != currentBlock || !currentSpanData.drawableFaces.HasDirection(currentDirection)) {
                                            if(y == startIndex[spanYIndex])
                                                width = x; //Set the width of the island then continue to the next Y iteration                                            
                                            else
                                                x = -1;

                                            break;
                                        }
                                    }

                                    if(x == width) {
                                        height++;

                                        for(int xx = 0; xx < width; xx++) {
                                            currentIndex[spanXIndex] = startIndex[spanXIndex] + xx;

                                            int currentFlatIndex = currentIndex.FlatIndex(chunkSize);

                                            data[currentFlatIndex].drawableFaces &= ~currentDirection;
                                        }
                                    }
                                    else if(x == -1) break;
                                }

                                VectorI3 totalIndex = startIndex;

                                totalIndex[spanXIndex] += width;
                                totalIndex[spanYIndex] += height;

                                face.Encapsulate(totalIndex);

                                if(Mathf.Max(vectorDirection.x, vectorDirection.y, vectorDirection.z) > 0)
                                    face.center += vectorDirection;

                                face.center -= (chunkSize * .5f);

                                islands.Add(new BlockBounds(spanData.block, face, Vector3.Normalize(vectorDirection), plane, spanXIndex, spanYIndex, currentDirection.ToFaceIndex()));
                            }
                        }

                        if(!spannableBlock) break;
                    }
                }

            return islands;
        }

        bool TestForAir(Block neighborBlock) {
            return TestForAir(neighborBlock, InWorld.BlockDatabaseAsset);
        }

        public static bool TestForAir(Block neighborBlock, BlockDatabase blockDatabase) {
            var blockData = blockDatabase.GetBlockData(neighborBlock.ID);

            return !blockData.IsSolid && neighborBlock.ID != 0;
        }
    }

    public struct BlockNeighbor {
        public Block block;
        public CubeDirectionFlag direction;

        public BlockNeighbor(Block block, CubeDirectionFlag direction) {
            this.block = block;
            this.direction = direction;
        }
    }
}