namespace YounGenTech.VoxelTech {
    public struct BlockPositionData {
        public Block block;
        public VectorI3 worldPosition;

        public BlockPositionData(Block block, VectorI3 worldPosition) {
            this.block = block;
            this.worldPosition = worldPosition;
        }
    }
}