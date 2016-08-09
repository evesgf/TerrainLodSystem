using UnityEngine;
using System.Collections;

public class TerrainLodSystem : MonoBehaviour
{
    /// <summary>
    /// 地形根节点
    /// </summary>
    public GameObject Root;
    /// <summary>
    /// 插值实例化的地形
    /// </summary>
    public Terrain newTerrain;
    /// <summary>
    /// 地形层级Lod
    /// </summary>
    public float[] lodLevel;
    /// <summary>
    /// 目标玩家坐标
    /// </summary>
    public Transform player;
    /// <summary>
    /// lod更新帧数
    /// </summary>
    public float lodFPS=0.2f;

    ///  UL 1 | UR 0
    ///  -----------
    ///  LL 2 | LR 3
    #region Quadclass
    
    //象限枚举
    public enum QuadEnum
    {
        UR = 0,
        UL = 1,
        LL = 2,
        LR = 3
    }

    /// <summary>
    /// 地形参数
    /// </summary>
    public struct TreeainBox
    {
        public Vector3 Pos;
        public float length;
        public float width;
    }

    /// <summary>
    /// 四叉树节点
    /// </summary>
    public class QuadNode
    {
        public TreeainBox box;
        public QuadNode[] children;
    }

    /// <summary>
    /// 四叉树
    /// </summary>
    public class QuadTree
    {
        public QuadNode root;
        public int depth;
    }
    #endregion

        // Use this for initialization
        void Start()
    {

    }

    #region CreateQuad

    /// <summary>
    /// 传入根节点和深度层级创建四叉树
    /// </summary>
    /// <param name="root"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    public QuadTree CreateQuadTree(GameObject root,int depth)
    {
        //创建根节点
        QuadNode rootNode = new QuadNode
        {
            box=CalcuateBoxData(root.transform.position,root.transform.localScale.x,root.transform.localScale.z)
        };
        //创建四叉树

        return null;
    }

    /// <summary>
    /// 计算四个节点的坐标
    ///  UL 1 | UR 0
    ///  -----------
    ///  LL 2 | LR 3
    /// </summary>
    /// <returns></returns>
    private QuadNode[] CalcuateChildNode(QuadNode node)
    {

    }

    /// <summary>
    /// 计算节点偏移坐标
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="length"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    private TreeainBox CalcuateBoxData(Vector3 pos,float length,float width)
    {
        float offsetX = length / 2;
        float offsetY = width / 2;

        TreeainBox box = new TreeainBox
        {
            Pos = pos,
            length = length,
            width = width
        };

        return box;
    }

    #endregion

    /// <summary>
    /// 传入大小好位置创建地形
    /// </summary>
    /// <param name="size"></param>
    /// <param name="pos"></param>
    public void CreateTerrain(Vector3 size,Vector3 pos)
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = 513;
        terrainData.baseMapResolution = 513;
        terrainData.size = size;
        terrainData.alphamapResolution = 512;
        terrainData.SetDetailResolution(32, 8);
        GameObject obj = Terrain.CreateTerrainGameObject(terrainData);
        obj.transform.position = pos;
    }
}