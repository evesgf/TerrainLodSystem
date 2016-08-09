using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainLodSystem : MonoBehaviour
{
    /// <summary>
    /// 地形根节点
    /// </summary>
    public GameObject Root;
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
        var tree = CreateQuadTree();
        InstanceTerrain(Root, tree.root.children,0,lodLevel.Length-1);
    }

    #region CreateQuad

    /// <summary>
    /// 传入根节点和深度层级创建四叉树
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    public QuadTree CreateQuadTree()
    {
        //创建根节点
        QuadNode rootNode = new QuadNode
        {
            box=CalcuateBoxData(Root.transform.position, Root.transform.localScale.x, Root.transform.localScale.z),
        };
        rootNode.children = CalcuateChildNode(rootNode);

        //创建四叉树
        QuadTree tree = new QuadTree
        {
            root = rootNode,
            depth = lodLevel.Length - 1
        };

        //迭代计算子节点
        CreateChildNode(tree.root.children,0,tree.depth);

        return tree;
    }

    /// <summary>
    /// 迭代计算子节点
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="nowDepth"></param>
    /// <param name="tagDepth"></param>
    private void CreateChildNode(QuadNode[] nodes, int nowDepth, int tagDepth)
    {
        if (nowDepth < tagDepth)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                var temp = CalcuateChildNode(nodes[i]);
                nodes[i].children = CalcuateChildNode(nodes[i]);

                CreateChildNode(nodes[i].children, nowDepth + 1, tagDepth);
            }
        }

        return;
    }

    /// <summary>
    /// 计算四个子节点的坐标
    ///  UL 1 | UR 0
    ///  -----------
    ///  LL 2 | LR 3
    /// </summary>
    /// <returns></returns>
    private QuadNode[] CalcuateChildNode(QuadNode node)
    {
        var boxUR = new QuadNode
        {
            box= CalcuateChildNode(QuadEnum.UR,node.box.Pos,node.box.length,node.box.width)
        };
        var boxUL = new QuadNode
        {
            box = CalcuateChildNode(QuadEnum.UL, node.box.Pos, node.box.length, node.box.width)
        };
        var boxLL = new QuadNode
        {
            box = CalcuateChildNode(QuadEnum.LL, node.box.Pos, node.box.length, node.box.width)
        };
        var boxLR = new QuadNode
        {
            box = CalcuateChildNode(QuadEnum.LR, node.box.Pos, node.box.length, node.box.width)
        };

        return new QuadNode[4] { boxUR, boxUL, boxLL, boxLR };
    }

    /// <summary>
    /// 根据父节点的坐标和长宽计算出子节点的中心，长宽
    ///  UL 1 | UR 0
    ///  -----------
    ///  LL 2 | LR 3
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="length"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    private TreeainBox CalcuateChildNode(QuadEnum eQuad, Vector3 pos, float length, float width)
    {
        TreeainBox box = new TreeainBox();
        box.length = length / 2;
        box.width = width / 2;

        switch (eQuad)
        {
            case QuadEnum.UR:
                box.Pos = new Vector3(pos.x + length / 4, pos.y, pos.z + width / 4);
                break;
            case QuadEnum.UL:
                box.Pos = new Vector3(pos.x + length / 4, pos.y, pos.z - width / 4);
                break;
            case QuadEnum.LL:
                box.Pos = new Vector3(pos.x - length / 4, pos.y, pos.z - width / 4);
                break;
            case QuadEnum.LR:
                box.Pos = new Vector3(pos.x - length / 4, pos.y, pos.z + width / 4);
                break;
        }

        return box;
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

    #region ShowQuad

    /// <summary>
    /// 传入节点并遍历子节点，创建地形
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="nowDepth"></param>
    /// <param name="tagDepth"></param>
    private void InstanceTerrain(GameObject root,QuadNode[] nodes, int nowDepth, int tagDepth)
    {
        if (nowDepth < tagDepth)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                //var terrain=CreateTerrain(new Vector3(nodes[i].box.length,1, nodes[i].box.width), nodes[i].box.Pos);

                var cube=CreateCube(new Vector3(nodes[i].box.length,1-0.1f*nowDepth, nodes[i].box.width), nodes[i].box.Pos);
                cube.transform.SetParent(root.transform);
                cube.name = root.name+"_"+nowDepth.ToString();

                InstanceTerrain(cube,nodes[i].children, nowDepth + 1, tagDepth);
            }
        }
        return;
    }
    #endregion

    public GameObject CreateCube(Vector3 size, Vector3 pos)
    {
        var cube = (GameObject.CreatePrimitive(PrimitiveType.Cube));
        cube.transform.localScale = size;
        cube.transform.position = pos;

        return cube;
    }

    /// <summary>
    /// 传入大小好位置创建地形
    /// </summary>
    /// <param name="size"></param>
    /// <param name="pos"></param>
    public GameObject CreateTerrain(Vector3 size,Vector3 pos)
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = 513;
        terrainData.baseMapResolution = 513;
        terrainData.size = size;
        terrainData.alphamapResolution = 512;
        terrainData.SetDetailResolution(32, 8);
        GameObject obj = Terrain.CreateTerrainGameObject(terrainData);
        obj.transform.position = pos;
        return obj;
    }
}