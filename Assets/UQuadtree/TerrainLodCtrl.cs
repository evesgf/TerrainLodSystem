using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainLodCtrl : MonoBehaviour {

    public Transform playerTransform;
    public float[] lodLevel;
    public GameObject Root;
    public float updateLodFPS;

    private int nowLevel;
    private List<Vector3> nowPosList;

	// Use this for initialization
	void Start () {
        StartCoroutine(UpdateLod());
	}

    /// <summary>
    /// 间隔指定帧数计算LOD
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdateLod()
    {
        CalculatePos(nowPosList, CheckLevel(), nowLevel);
        yield return new WaitForSeconds(updateLodFPS);
        StartCoroutine(UpdateLod());
    }

    /// <summary>
    /// 迭代计算层级
    /// </summary>
    /// <param name="posList"></param>
    /// <param name="tagDepth"></param>
    /// <param name="nowDepth"></param>
    /// <returns></returns>
    private List<Vector3> CalculatePos(List<Vector3> posList,int tagDepth,int nowDepth)
    {
        if (nowDepth == tagDepth)
        {
            nowLevel = nowDepth;
            nowPosList = posList;
            return posList;
        }

        var newPosList = new List<Vector3>();

        //计算补间坐标
        for (int i = 0; i < posList.Count; i++)
        {
            var temp = CalculateLerpPos(posList[i],10,10);
            for (int j = 0; i < temp.Count; j++)
            {
                newPosList.Add(temp[i]);
            }
        }

        return CalculatePos(newPosList, tagDepth, nowDepth + 1);
    }

    /// <summary>
    /// 传入基准坐标，通过长宽计算四个插值坐标
    ///  2 | 1 
    /// -------
    ///  3 | 4
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private List<Vector3> CalculateLerpPos(Vector3 pos,float length,float width)
    {
        List<Vector3> lerpPoslist = new List<Vector3>();

        var offsetL = length / 2;
        var offsetW = width / 2;

        //1
        lerpPoslist.Add(new Vector3(pos.x+ offsetL, pos.y,pos.z+offsetW));
        //2
        lerpPoslist.Add(new Vector3(pos.x + offsetL, pos.y, pos.z - offsetW));
        //3
        lerpPoslist.Add(new Vector3(pos.x - offsetL, pos.y, pos.z - offsetW));
        //4
        lerpPoslist.Add(new Vector3(pos.x - offsetL, pos.y, pos.z + offsetW));

        return lerpPoslist;
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
