using UnityEngine;
using YounGenTech.VoxelTech;

public class TestWorldPositions : MonoBehaviour {

    public World world;

    void OnDrawGizmos() {
        Gizmos.color = new Color(.3f, 1, .3f, .5f);
        Gizmos.DrawCube(world.GetPosition(transform.position, PositionStyle.Block, Pivot.Center), Vector3.one * 1.1f);
        Gizmos.color = new Color(1f, 1, .3f, .5f);
        Gizmos.DrawWireCube(world.GetPosition(transform.position, PositionStyle.Block, Pivot.Center), Vector3.one * 1.1f);

        Gizmos.color = new Color(.6f, .6f, 1, .2f);
        Gizmos.DrawCube(world.GetPosition(transform.position, PositionStyle.Chunk, Pivot.Center), world.DefaultChunkSize * 1.01f);
        Gizmos.color = new Color(.6f, 1, 1, .2f);
        Gizmos.DrawWireCube(world.GetPosition(transform.position, PositionStyle.Chunk, Pivot.Center), world.DefaultChunkSize * 1.01f);
    }

    void OnGUI() {
        GUILayout.BeginVertical();
        {
            GUILayout.Label("Chunk Position " + world.GetPosition(transform.position, PositionStyle.Chunk));
            GUILayout.Label("Block Position " + world.GetPosition(transform.position, PositionStyle.Block));
        }
        GUILayout.EndVertical();
    }
}