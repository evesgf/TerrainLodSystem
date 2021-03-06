﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainLodCtrl : MonoBehaviour {

    public Transform playerTransform;
    public float[] lodLevel;

    public GameObject Root;

    public float updateLodFPS;

    public GameObject newCube;

    private int nowLevel;
    private List<Vector3> nowPosList;


    ///  UL 1 | UR 0
    ///  -----------
    ///  LL 2 | LR 3
    #region QuadTreeBase

    public enum QuadrantEnum
    {
        UR = 0,
        UL = 1,
        LL = 2,
        LR = 3
    }

    public struct MapRect
    {
        public Vector3 centerPos;
        public float length;
        public float width;
        public Vector3 posUR;
        public Vector3 posUL;
        public Vector3 posLL;
        public Vector3 posLR;
    }

    public struct SHPMBRInfo
    {
        //空间对象ID号
        public int nID;
        //空间对象MBR范围坐标
        public MapRect Box;
    }

    public struct QuadNode
    {
        //节点所代表的矩形区域
        public MapRect Box;
        //节点所包含的所有空间对象个数
        public int nShpCount;
        //空间对象数组
        public SHPMBRInfo[] pShapObj;
        //子节点个数
        public int nChildCount;
        //指向节点的四个子对象
        public QuadNode[] children;
    }
    public struct quadTree_t
    {
        //根节点
        public QuadNode root;
        //层级深度
        public int depth;
    }

    #endregion

    public quadTree_t quad;

    // Use this for initialization
    void Start () {
        //计算四叉树
        quad = CalculateQuad();

        ShowQuad(new QuadNode[] { quad.root },4,0);

        //开始更新Lod
        //StartCoroutine(UpdateLod());
    }

    private void ShowQuad(QuadNode[] nodes,int tagDepth,int nowDepth)
    {
        if (nodes != null)
        {
            if (tagDepth == nowDepth) return;

            for (int i = 0; i < nodes.Length; i++)
            {
                Instantiate(newCube, nodes[i].Box.centerPos, new Quaternion(1, 1, 1, 1));
                ShowQuad(nodes[i].children, tagDepth, nowDepth + 1);
            }
        }

        return;
    }

    /// <summary>
    /// 计算四叉树
    /// </summary>
    private quadTree_t CalculateQuad()
    {
        quadTree_t q = new quadTree_t();
        q.depth = lodLevel.Length-1;

        //根节点
        QuadNode root = new QuadNode();

        root.Box = CalculateNodePos(Root.transform.position, Root.transform.lossyScale.x, Root.transform.lossyScale.z);

        RecursionNode(new QuadNode[] { root }, 4, 0);

        q.root = root;
        return q;
    }

    /// <summary>
    /// 迭代节点坐标
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="tagDepth"></param>
    /// <param name="nowDepth"></param>
    /// <returns></returns>
    private QuadNode[] RecursionNode(QuadNode[] nodes,int tagDepth,int nowDepth)
    {
        if (tagDepth == nowDepth)
        {
            return nodes;
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            //计算四个子节点
            var temp = CalculateChildNode(nodes[i]);
            nodes[i].children = new QuadNode[4] { temp[0], temp[1], temp[2], temp[3] };
            RecursionNode(nodes[i].children, tagDepth, nowDepth + 1);
        }

        return nodes;
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

        switch(quadrant)
        {
            case QuadrantEnum.UR:
            rect.centerPos = new Vector3(pos.x+ length/4, pos.y, pos.z + width / 4);
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
    private MapRect CalculateNodePos(Vector3 pos,float length,float width)
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

    /// <summary>
    /// 间隔指定帧数计算LOD
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdateLod()
    {
        //CalculatePos(nowPosList, CheckLevel(), nowLevel);

        //获取当前应该有的层级

        //确定玩家坐标对应区块，依次迭代

        yield return new WaitForSeconds(updateLodFPS);
        StartCoroutine(UpdateLod());
    }

    /// <summary>
    /// 计算当前迭代层级
    /// </summary>
    /// <returns></returns>
    private int CheckLevel()
    {
        var height = playerTransform.position.y - transform.position.y;

        //最粗粒度
        if (height > lodLevel[lodLevel.Length - 1])
        {
            return lodLevel.Length - 1;
        }

        //最细粒度
        if (height < lodLevel[0])
        {
            return 0;
        }

        //计算层级
        for(int i = 0; i < lodLevel.Length-1; i++)
        {
            if (lodLevel[i] < height && height < lodLevel[i + 1])
            {
                return i;
            }
        }

        return 0;
    }
}
