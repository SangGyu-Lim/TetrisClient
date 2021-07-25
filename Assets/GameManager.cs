using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    static GameManager instance = null;
    public static GameManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public struct RankInfo
    {
        public string nickName;
        public int score;
    }

    protected List<RankInfo> listRankInfo = new List<RankInfo>();

    public GameObject[] goScenes;

    public UILabel labelStart;
    public UILabel labelGameLevel;
    public UILabel labelGameScore;

    protected int currentGameLevel = 0;
    protected int currentGameScore = 0;
    public float moveSpeedTime = CommonDefine.DEFAULT_MOVE_TIME;
    public UILabel labelSpeedUp;

    public GameObject goRankPopup;
    public UILabel labelRankList;

    public GameObject goNewRankPopup;
    public UIInput inputRankNickName;

    public GameObject goMultiPlayPopup;
    public GameObject goMultiPlayWait;
    public UIInput inputMultiPlayNickName;

    bool isGameOver = false;
    public GameObject goGameOver;
    public UILabel labelGameOver;

    public GAME_TYPE gameType = GAME_TYPE.SINGLE;

    // Use this for initialization
    void Start () {
        gameType = GAME_TYPE.SINGLE;
        ChangeStage(STAGE_TYPE.INTRO);
    }
	
	// Update is called once per frame
	void Update () {
        

    }

    protected void InitGameInfo()
    {
        currentGameScore = 0;
        currentGameLevel = 1;

        labelGameLevel.text = currentGameLevel.ToString();
        labelGameScore.text = currentGameScore.ToString();

        moveSpeedTime = CommonDefine.DEFAULT_MOVE_TIME;

        isGameOver = false;
    }

    protected void InitMultiGameInfo()
    {
        isGameOver = false;

        for (int i = 0; i < goScenes[(int)STAGE_TYPE.MULTI_GAME].transform.childCount; ++i)
        {
            goScenes[(int)STAGE_TYPE.MULTI_GAME].transform.GetChild(i).gameObject.SetActive(false);
        }

        goMultiPlayPopup.SetActive(true);
    }

    void ChangeStage(STAGE_TYPE type)
    {
        // 다 끄기
        for(int i = 0; i < (int)STAGE_TYPE.MAX; ++i)
            goScenes[i].SetActive(false);

        switch (type)
        {
            case STAGE_TYPE.INTRO:
                {
                    if (PlayerPrefs.HasKey(CommonDefine.SINGLE_RANK_LIST) == true)
                    {
                        SettingRankList(PlayerPrefs.GetString(CommonDefine.SINGLE_RANK_LIST));
                    }
                    // 기본 데이터가 없을때
                    else
                    {
                        InitRankList();
                    }

                    goRankPopup.gameObject.SetActive(false);
                    labelStart.gameObject.SetActive(false);
                    StopAllCoroutines();

                    goScenes[(int)STAGE_TYPE.INTRO].SetActive(true);
                    
                } break;
            case STAGE_TYPE.GAME:
                {
                    InitGameInfo();
                    TetrisBlockManager.Instance.Init();
                    UIBackgroundManager.Instance.Init();

                    StartCoroutine(StartGame());
                    goScenes[(int)STAGE_TYPE.GAME].SetActive(true);

                    if(gameType == GAME_TYPE.MULTI)
                        goScenes[(int)STAGE_TYPE.MULTI_GAME].SetActive(true);

                } break;

            case STAGE_TYPE.MULTI_GAME:
                {
                    InitMultiGameInfo();

                    goScenes[(int)STAGE_TYPE.MULTI_GAME].SetActive(true);
                } break;

        }
    }



    IEnumerator StartGame()
    {
        labelStart.gameObject.SetActive(true);

        labelStart.text = "3";

        yield return new WaitForSeconds(1f);
        labelStart.text = "2";

        yield return new WaitForSeconds(1f);
        labelStart.text = "1";

        yield return new WaitForSeconds(1f);
        labelStart.text = "START";

        yield return new WaitForSeconds(1f);
        labelStart.gameObject.SetActive(false);

        TetrisBlockManager.Instance.SpwanRandomBlock();

        StartCoroutine(OnLevelUpGame());
    }


    #region OnClick
    // 공통
    public void OnClickBack()
    {
        ChangeStage(STAGE_TYPE.INTRO);
    }

    // 싱글 플레이
    public void OnClickSinglePlay()
    {
        gameType = GAME_TYPE.SINGLE;

        ChangeStage(STAGE_TYPE.GAME);     
    }

    public void OnClickNewRankComfirm()
    {
        AddRankList(inputRankNickName.value, currentGameScore);
        goNewRankPopup.SetActive(false);
        OnRankPopup();
    }

    public void OnClickNewRankCancel()
    {
        goNewRankPopup.SetActive(false);
        OnRankPopup();
    }

    // 멀티 플레이
    public void OnClickMultiPlay()
    {
        gameType = GAME_TYPE.MULTI;

        SocketManager.Instance.ConnectMulti();

        ChangeStage(STAGE_TYPE.MULTI_GAME);
    }

    public void OnClickMultiPlayComfirm()
    {
        string nickName = "";
        nickName = inputMultiPlayNickName.value;
        SocketManager.Instance.StartMulti(nickName);

        goMultiPlayPopup.SetActive(false);
        goMultiPlayWait.SetActive(true);
    }

    public void OnClickMultiPlayCancel()
    {
        SocketManager.Instance.DisconnectMulti();
        ChangeStage(STAGE_TYPE.INTRO);
    }

    public void OnClickGameOver()
    {
        SocketManager.Instance.DisconnectMulti();
        ChangeStage(STAGE_TYPE.INTRO);
    }


    #endregion

    //레벨업 관리
    IEnumerator OnLevelUpGame()
    {
        for (int i = 0; i < CommonDefine.MAX_GAME_LEVEL; ++i)
        {
            yield return new WaitForSeconds(CommonDefine.LEVEL_UP_SECOND);

            currentGameLevel++;
            labelGameLevel.text = currentGameLevel.ToString();

            StartCoroutine(OnSpeedUp());
        }
    }

    IEnumerator OnSpeedUp()
    {
        moveSpeedTime *= CommonDefine.LEVEL_UP_SPEED_RATIO;//반띵..빨라지게.

        labelSpeedUp.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.8f);
        labelSpeedUp.gameObject.SetActive(false);
    }


    public bool IsValidPosition(Transform block)
    {
        for (int i = 0; i < block.transform.childCount; ++i)
        {
            Transform piece = block.transform.GetChild(i);

            // float 버그 해결
            float x = Mathf.Round(piece.transform.position.x * 100f) / 100f;

            // 테트리스 벽 충돌
            if (x > UIBackgroundManager.Instance.borderRightPosition
                || x < UIBackgroundManager.Instance.borderLeftPosition 
                || piece.transform.position.y < UIBackgroundManager.Instance.borderBottomPosition)
            {
                return false;
            }

            // 블럭이 이미 존재하는지
            if (UIBackgroundManager.Instance.IsExistHoldPiece(piece) == true)
                return false;
            
        }

        return true;
    }

    public void OnPlaceBlock(TetrisBlock block)
    {
        block.enabled = false;

        if (IsGameEndPosition(block.transform) == false && isGameOver == false)
        {
            // 현재 블럭 색상으로 배경 블럭 색 변경.
            for (int i = 0; i < block.transform.childCount; ++i)
            {
                Transform piece = block.transform.GetChild(i);

                UIBackgroundManager.Instance.OnPlacePiece(piece);

            }

            // 현재 블럭 제거
            GameObject.Destroy(block.transform.gameObject);

            UIBackgroundManager.Instance.DeleteRows();

            if(gameType == GAME_TYPE.MULTI)
            {
                SocketManager.Instance.UpdateBlock();
            }

            TetrisBlockManager.Instance.SpwanRandomBlock();
        }
        else
        {
            if(gameType == GAME_TYPE.SINGLE)
            {
                OnGameOverSinglePlay();
            }
            else if(gameType == GAME_TYPE.MULTI)
            {
                GameOverMultiPlay();
            }
                
        }

    }

    // 테트리스 블럭 벽 충돌체크
    public bool IsGameEndPosition(Transform block)
    {
        for (int i = 0; i < block.transform.childCount; ++i)
        {
            // 자식 가져오기
            Transform child = block.transform.GetChild(i);
            if (child.transform.position.y > UIBackgroundManager.Instance.borderTopPosition)
            {
                return true;
            }

        }
        return false;
    }

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

    protected void InitRankList()
    {
        for (int i = 0; i < CommonDefine.MAX_RANKING_COUNT; ++i)
        {
            AddRankList("Unknown", 10);
        }
        SaveRankList();
    }

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

    protected void AddRankList(string nickName, int score)
    {
        RankInfo rankInfo = new RankInfo();
        rankInfo.nickName = nickName;
        rankInfo.score = score;
        listRankInfo.Add(rankInfo);
    }

    protected void OnGameOverSinglePlay()
    {
        StartCoroutine(UIBackgroundManager.Instance.GameOverBlocks());

        // 랭크인포에 존재한다면.
        if (listRankInfo.Count > 0)
        {
            // 새로운 랭킹 등록
            if(currentGameScore > listRankInfo[CommonDefine.MAX_RANKING_COUNT - 1].score)
            {
                goNewRankPopup.SetActive(true);
                return;
            }
        }
        else
        {
            InitRankList();
        }

        OnRankPopup();
    }

    protected void OnRankPopup()
    {
        labelRankList.text = GetRankStringByList();

        goRankPopup.SetActive(true);
    }

    protected string GetRankStringByList()
    {
        //소팅
        SortListRankInfo();

        // 새로운 랭킹 등록
        if(listRankInfo.Count >= CommonDefine.MAX_RANKING_COUNT)
        {
            for (int i = CommonDefine.MAX_RANKING_COUNT; i < listRankInfo.Count; ++i)
                listRankInfo.RemoveAt(i);

            SaveRankList();
        }

        // 출력
        string stringRankInfo = "";
        for (int i = 0; i < listRankInfo.Count; ++i)
        {
            stringRankInfo += string.Format("{0}.{1} : {2}\n",
                (i + 1), listRankInfo[i].nickName, listRankInfo[i].score);
        }

        return stringRankInfo;
    }

    

    public void OnScoreUp()
    {
        currentGameScore += CommonDefine.DELETE_ONEROW_SCORE;
        labelGameScore.text = currentGameScore.ToString();
    }

    #region Multi

    public void OnGameStart()
    {
        goMultiPlayWait.SetActive(false);
        ChangeStage(STAGE_TYPE.GAME);
    }

    public void GameOverMultiPlay()
    {
        isGameOver = true;
        labelSpeedUp.gameObject.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(UIBackgroundManager.Instance.GameOverBlocks());

        labelGameOver.text = "You Lose....";

        goGameOver.SetActive(true);
        SocketManager.Instance.CallGameOver();
    }

    public void OnGameOverMultiPlay()
    {
        isGameOver = true;
        labelSpeedUp.gameObject.SetActive(false);
        TetrisBlockManager.Instance.Init();
        StopAllCoroutines();
        StartCoroutine(UIBackgroundManager.Instance.GameOverBlocks());

        labelGameOver.text = "You Win~~!";
        
        goGameOver.SetActive(true);

    }
    #endregion
}
