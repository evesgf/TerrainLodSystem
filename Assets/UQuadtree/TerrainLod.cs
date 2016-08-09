using UnityEngine;
using System.Collections;

public class TerrainLod : MonoBehaviour {

    public GameObject Root;
    public GameObject newCube;
    public int tagDepth;

    public class quadTree_t
    {
        //根节点
        public QuadNode root;
        //层级深度
        public int depth;
    }

    public class QuadNode
    {
        //节点所代表的矩形区域
        public MapRect Box;
        public QuadNode[] children;
    }

    public enum QuadrantEnum
    {
        UR = 0,
        UL = 1,
        LL = 2,
        LR = 3
    }

    public class MapRect
    {
        public Vector3 centerPos;
        public float length;
        public float width;
        public Vector3 posUR;
        public Vector3 posUL;
        public Vector3 posLL;
        public Vector3 posLR;
    }

    // Use this for initialization
    void Start () {
        var tree = CreateQuadTree();
        ShowQuad(tree);
    }

    private void ShowQuad(quadTree_t tree)
    {
        InstanceCube(tree.root.children,0,4);
    }

    private void InstanceCube(QuadNode[] nodes, float nowDepth, float tagDepth)
    {
        if (nowDepth < tagDepth)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                var temp=(GameObject)Instantiate(newCube, nodes[i].Box.centerPos, new Quaternion(0, 0, 0, 0));
                temp.transform.localScale = new Vector3(nodes[i].Box.length, (1 - 0.1f * nowDepth), nodes[i].Box.width);

                InstanceCube(nodes[i].children, nowDepth + 1, tagDepth);
            }
        }

        return;
    }

    private quadTree_t CreateQuadTree()
    {
        quadTree_t tree = new quadTree_t();
        //确定根节点的坐标和长度
        QuadNode root = new QuadNode();
        root.Box = CalculateNodePos(Root.transform.position, Root.transform.lossyScale.x, Root.transform.lossyScale.z);
        tree.root = root;
        //计算根节点的子节点
        var temp = CalculateChildNode(root);

        root.children = new QuadNode[4] { temp[0], temp[1], temp[2], temp[3] };

        //迭代计算
        CreateChildNode(root,root.children, 0, tagDepth);

        return tree;
    }

    private void CreateChildNode(QuadNode root,QuadNode[] nodes,int nowDepth,int tagDepth)
    {
        if (nowDepth < tagDepth)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                var temp = CalculateChildNode(nodes[i]);

                nodes[i].children = new QuadNode[4] { temp[0], temp[1], temp[2], temp[3] };

                CreateChildNode(nodes[i],temp, nowDepth + 1, tagDepth);
            }
        }

        return;
    }

    /// <summary>
    /// 计算四个子节点
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private QuadNode[] CalculateChildNode(QuadNode node)
    {

        var tempUR = CalcuateChildNode(QuadrantEnum.UR, node.Box.centerPos, node.Box.length, node.Box.width);
        QuadNode ur = new QuadNode
        {
            Box = CalculateNodePos(tempUR.centerPos, tempUR.length, tempUR.width),
        };

        var tempUL = CalcuateChildNode(QuadrantEnum.UL, node.Box.centerPos, node.Box.length, node.Box.width);
        QuadNode ul = new QuadNode
        {
            Box = CalculateNodePos(tempUL.centerPos, tempUL.length, tempUL.width),
        };

        var tempLL = CalcuateChildNode(QuadrantEnum.LL, node.Box.centerPos, node.Box.length, node.Box.width);
        QuadNode ll = new QuadNode
        {
            Box = CalculateNodePos(tempLL.centerPos, tempLL.length, tempLL.width),
        };

        var tempLR = CalcuateChildNode(QuadrantEnum.LR, node.Box.centerPos, node.Box.length, node.Box.width);
        QuadNode lr = new QuadNode
        {
            Box = CalculateNodePos(tempLR.centerPos, tempLR.length, tempLR.width),
        };

        return new QuadNode[4] { ur, ul, ll, lr };
    }

    /// <summary>
    /// 根据父节点的坐标和长宽计算出子节点的中心，长宽
    /// 返回值只有centerPos和length，width
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="length"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    private MapRect CalcuateChildNode(QuadrantEnum quadrant, Vector3 pos, float length, float width)
    {
        MapRect rect = new MapRect();
        rect.length = length / 2;
        rect.width = width / 2;

        switch (quadrant)
        {
            case QuadrantEnum.UR:
                rect.centerPos = new Vector3(pos.x + length / 4, pos.y, pos.z + width / 4);
                break;
            case QuadrantEnum.UL:
                rect.centerPos = new Vector3(pos.x + length / 4, pos.y, pos.z - width / 4);
                break;
            case QuadrantEnum.LL:
                rect.centerPos = new Vector3(pos.x - length / 4, pos.y, pos.z - width / 4);
                break;
            case QuadrantEnum.LR:
                rect.centerPos = new Vector3(pos.x - length / 4, pos.y, pos.z + width / 4);
                break;
        }

        return rect;
    }

    /// <summary>
    /// 计算节点坐标
    ///  UL 1 | UR 0
    ///  -----------
    ///  LL 2 | LR 3
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="length"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    private MapRect CalculateNodePos(Vector3 pos, float length, float width)
    {
        MapRect rect = new MapRect();
        rect.centerPos = pos;
        rect.length = length;
        rect.width = width;

        float offsetX = length / 2;
        float offsetY = width / 2;

        rect.posUR = new Vector3(pos.x + offsetX, pos.y, pos.z + offsetY);
        rect.posUL = new Vector3(pos.x + offsetX, pos.y, pos.z - offsetY);
        rect.posLL = new Vector3(pos.x - offsetX, pos.y, pos.z - offsetY);
        rect.posLR = new Vector3(pos.x - offsetX, pos.y, pos.z + offsetY);

        return rect;
    }

}
