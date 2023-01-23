using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public struct RankInfo
    {
        public string nickName;
        public int score;
    }

    protected List<RankInfo> listRankInfo = new List<RankInfo>();

    // 랭킹 셋팅
    protected void SettingRankList(string rankList)
    {
        listRankInfo.Clear();

        // 이름, 점수 ;
        string[] scoreList = rankList.Split(';');

        for (int i = 0; i < scoreList.Length; ++i)
        {
            string[] stringRankInfo = scoreList[i].Split(',');
            AddRankList(stringRankInfo[0], int.Parse(stringRankInfo[1]));
        }
    }

    // 점수로 정렬
    protected void SortListRankInfo()
    {
        if (listRankInfo.Count >= 2)
        {
            listRankInfo.Sort(delegate (RankInfo A, RankInfo B)
            {
                if (A.score > B.score) return -1;
                else if (A.score < B.score) return 1;
                return 0;
            });
        }
    }

    // 랭킹 초기화
    protected void InitRankList()
    {
        for (int i = 0; i < CommonDefine.MAX_RANKING_COUNT; ++i)
        {
            AddRankList("Unknown", 10);
        }
        SaveRankList();
    }

    // 랭킹 갱신
    protected void SaveRankList()
    {
        string rankList = "";

        for (int i = 0; i < CommonDefine.MAX_RANKING_COUNT; ++i)
        {
            rankList += string.Format("{0},{1}", listRankInfo[i].nickName, listRankInfo[i].score);

            if(i != (CommonDefine.MAX_RANKING_COUNT - 1))
            {
                rankList += ";";
            }
        }

        PlayerPrefs.SetString(CommonDefine.SINGLE_RANK_LIST, rankList);
    }

    // 새로운 랭킹 추가
    protected void AddRankList(string nickName, int score)
    {
        RankInfo rankInfo = new RankInfo();
        rankInfo.nickName = nickName;
        rankInfo.score = score;
        listRankInfo.Add(rankInfo);
    }
}