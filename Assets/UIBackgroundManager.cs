using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBackgroundManager : MonoBehaviour {

    static UIBackgroundManager instance = null;
    public static UIBackgroundManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public UISprite spriteBlock;
    public GameObject goParentBackground;

    public UISprite spriteMultiPlayerBlock;
    public UISprite spriteMultiPlayerColoredBlock;
    public GameObject goMultiPlayerParentBackground;

    [HideInInspector]
    public float borderLeftPosition = 0f;
    [HideInInspector]
    public float borderRightPosition = 0f;
    [HideInInspector]
    public float borderBottomPosition = 0f;
    [HideInInspector]
    public float borderTopPosition = 0f;

    public struct GameBlock
    {
        public BACKGROUND_PIECE_TYPE type;
        public Transform piece;
    }

    // 내 블럭 정보
    protected GameBlock[,] gameBlocks;
    // 상대 블럭 정보
    protected GameBlock[,] multiPlayerGameBlocks;

    public void Init()
    {
        // 싱글 플레이 초기화
        if(gameBlocks == null)
        {
            gameBlocks = new GameBlock[CommonDefine.MAX_BACKGROUND_WIDTH, CommonDefine.MAX_BACKGROUND_HEIGHT];

            for (int i = 0; i < CommonDefine.MAX_BACKGROUND_WIDTH; ++i)
            {
                for (int j = 0; j < CommonDefine.MAX_BACKGROUND_HEIGHT; ++j)
                {
                    gameBlocks[i, j].piece = CreateBackgroundPiece(i, j);
                    gameBlocks[i, j].type = BACKGROUND_PIECE_TYPE.BLANK;
                }
            }
        }
        else
        {
            ClearBackgroundPieces();
        }

        // 멀티 플레이 초기화
        if (GameManager.Instance.gameType == GAME_TYPE.MULTI)
        {
            InitMulti();
        }
        else
        {
            goMultiPlayerParentBackground.SetActive(false);
        }
    }

    void ClearBackgroundPieces()
    {
        for (int i = 0; i < CommonDefine.MAX_BACKGROUND_WIDTH; ++i)
        {
            for (int j = 0; j < CommonDefine.MAX_BACKGROUND_HEIGHT; ++j)
            {
                gameBlocks[i, j].piece.GetComponent<UISprite>().color = spriteBlock.color;
                gameBlocks[i, j].type = BACKGROUND_PIECE_TYPE.BLANK;
            }
        }
    }

    public Transform CreateBackgroundPiece(int x, int y)
    {
        // 생성
        GameObject piece = GameObject.Instantiate(spriteBlock.gameObject);
        piece.transform.parent = goParentBackground.transform;
        piece.transform.localPosition = new Vector3(x * CommonDefine.BLOCK_SIZE - CommonDefine.MAX_BACKGROUND_WIDTH / 2 * CommonDefine.BLOCK_SIZE,
            -y * CommonDefine.BLOCK_SIZE + CommonDefine.MAX_BACKGROUND_HEIGHT / 2 * CommonDefine.BLOCK_SIZE, 0f);
        piece.name = string.Format("{0},{1}", x, y);

        // NGUI 영향으로 스케일 초기화
        piece.transform.localScale = Vector3.one;

        // 벽위치 설정
        if(x == 0 && borderLeftPosition == 0f) // 왼쪽
        {
            borderLeftPosition = piece.transform.position.x;
        }
        if(x == CommonDefine.MAX_BACKGROUND_WIDTH - 1 && borderRightPosition == 0f) // 오른쪽
        {
            borderRightPosition = piece.transform.position.x;
        }
        if (y == CommonDefine.MAX_BACKGROUND_HEIGHT - 1 && borderBottomPosition == 0f) // 바닥
        {
            borderBottomPosition = piece.transform.position.y;
        }
        // 게임 종료 위치 설정
        if (y == 0 && borderTopPosition == 0f)
        {
            borderTopPosition = piece.transform.position.y;

        }

        piece.SetActive(true);

        return piece.transform;
    }

    public void OnPlacePiece(Transform piece)
    {
        int x;
        int y;

        GetPiecePosition(piece, out x, out y);
        SetBlockPiece(piece, x, y);
    }

    public void GetPiecePosition(Transform piece, out int x, out int y)
    {
        x = -1;
        y = -1;

        for(int i = 0; i < CommonDefine.MAX_BACKGROUND_WIDTH; ++i)
        {
            for(int j = 0; j < CommonDefine.MAX_BACKGROUND_HEIGHT; ++j)
            {
                BoxCollider collider = gameBlocks[i, j].piece.GetComponent<BoxCollider>();
                if(collider.bounds.Contains(piece.position) == true)
                {
                    x = i;
                    y = j;
                }
            }
        }
    }

    protected void SetBlockPiece(Transform piece, int x, int y)
    {
        // 배경 스프라이트
        UISprite backgroundSprite = gameBlocks[x, y].piece.GetComponent<UISprite>();

        // 블럭 스프라이트
        UISprite pieceSprite = piece.GetComponent<UISprite>();

        backgroundSprite.color = pieceSprite.color;
        gameBlocks[x, y].type = BACKGROUND_PIECE_TYPE.HOLD;
    }

    // 현재 피스 검사(있는지? 있으면 누가 있는지)
    public bool IsExistHoldPiece(Transform piece)
    {
        int x;
        int y;
        GetPiecePosition(piece, out x, out y);

        if (x < 0 || y < 0)
            return false;

        return gameBlocks[x, y].type == BACKGROUND_PIECE_TYPE.HOLD ? true : false;
    }
    
    
    public void DeleteRows()
    {
        for(int y = CommonDefine.MAX_BACKGROUND_HEIGHT -1; y > 0; --y)
        {
            if(IsRowFull(y))
            {
                for (int x = 0; x < CommonDefine.MAX_BACKGROUND_WIDTH; ++x)
                {
                    // 배경 스프라이트
                    gameBlocks[x, y].piece.GetComponent<UISprite>().color = spriteBlock.color;
                    gameBlocks[x, y].type = BACKGROUND_PIECE_TYPE.BLANK;

                }
                DecreaseRow(y);
                ++y;

                //점수증가.
                GameManager.Instance.OnScoreUp();
            }            
        }            
    }

    public bool IsRowFull(int y)
    {
        for(int x = 0; x < CommonDefine.MAX_BACKGROUND_WIDTH; ++x)
        {
            if (gameBlocks[x, y].type == BACKGROUND_PIECE_TYPE.BLANK)
                return false;
        }

        return true;
    }

    void DecreaseRow(int y)
    {
        for(int i = (y - 1); i > 0; --i)
        {
            for(int x = 0; x < CommonDefine.MAX_BACKGROUND_WIDTH; ++x)
            {
                gameBlocks[x, i + 1].type = gameBlocks[x, i].type;
                gameBlocks[x, i + 1].piece.GetComponent<UISprite>().color = gameBlocks[x, i].piece.GetComponent<UISprite>().color;

                gameBlocks[x, i].type = BACKGROUND_PIECE_TYPE.BLANK;
                gameBlocks[x, i].piece.GetComponent<UISprite>().color = spriteBlock.color;
            }    
        }
    }

    // GameOver
    public IEnumerator GameOverBlocks()
    {
        for (int y = CommonDefine.MAX_BACKGROUND_HEIGHT - 1; y >= 0; y--)
        {
            for (int x = 0; x < CommonDefine.MAX_BACKGROUND_WIDTH; x++)
            {
                gameBlocks[x, y].piece.GetComponent<UISprite>().color = Color.gray;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }


    #region Multi

    void InitMulti()
    {
        goMultiPlayerParentBackground.SetActive(true);

        if (multiPlayerGameBlocks == null)
        {
            multiPlayerGameBlocks = new GameBlock[CommonDefine.MAX_BACKGROUND_WIDTH, CommonDefine.MAX_BACKGROUND_HEIGHT];

            for (int i = 0; i < CommonDefine.MAX_BACKGROUND_WIDTH; ++i)
            {
                for (int j = 0; j < CommonDefine.MAX_BACKGROUND_HEIGHT; ++j)
                {
                    multiPlayerGameBlocks[i, j].piece = CreateMultiPlayerBackgroundPiece(i, j);
                    multiPlayerGameBlocks[i, j].type = BACKGROUND_PIECE_TYPE.BLANK;
                }
            }
        }
        else
        {
            ClearBackgroundPiecesMulti();
        }
        
    }

    void ClearBackgroundPiecesMulti()
    {
        for (int i = 0; i < CommonDefine.MAX_BACKGROUND_WIDTH; ++i)
        {
            for (int j = 0; j < CommonDefine.MAX_BACKGROUND_HEIGHT; ++j)
            {
                multiPlayerGameBlocks[i, j].piece.GetComponent<UISprite>().color = spriteMultiPlayerBlock.color;
                multiPlayerGameBlocks[i, j].type = BACKGROUND_PIECE_TYPE.BLANK;
            }
        }
    }

    public Transform CreateMultiPlayerBackgroundPiece(int x, int y)
    {
        // 생성
        GameObject piece = GameObject.Instantiate(spriteMultiPlayerBlock.gameObject);
        piece.transform.parent = goMultiPlayerParentBackground.transform;
        piece.transform.localPosition = new Vector3(x * MultiPlayDefine.MULTI_PLAYER_BLOCK_SIZE,
            -y * MultiPlayDefine.MULTI_PLAYER_BLOCK_SIZE, 0f);
        piece.name = string.Format("{0},{1}", x, y);

        // NGUI 영향으로 스케일 초기화
        piece.transform.localScale = Vector3.one;

        piece.SetActive(true);

        return piece.transform;
    }

    public void UpdateMultiPlayerGameBlock(int i, int j, BACKGROUND_PIECE_TYPE updateBlockType)
    {
        if (multiPlayerGameBlocks[i, j].type != updateBlockType)
        {
            multiPlayerGameBlocks[i, j].type = updateBlockType;

            if (multiPlayerGameBlocks[i, j].type == BACKGROUND_PIECE_TYPE.BLANK)
            {
                multiPlayerGameBlocks[i, j].piece.GetComponent<UISprite>().color = spriteMultiPlayerBlock.color;
            }
            else
            {
                // 색칠합시다
                multiPlayerGameBlocks[i, j].piece.GetComponent<UISprite>().color = spriteMultiPlayerColoredBlock.color;
            }


        }
    }

    public BACKGROUND_PIECE_TYPE GetCurrentBlockType(int x, int y)
    {
        return gameBlocks[x, y].type;
    }
    #endregion
}
