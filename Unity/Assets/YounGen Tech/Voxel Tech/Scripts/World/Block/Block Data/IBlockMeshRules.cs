namespace YounGenTech.VoxelTech {
    public interface IBlockMeshRules {

        void BuildMeshSide(int face, Block[,,] neighborBlocks);

    }
}