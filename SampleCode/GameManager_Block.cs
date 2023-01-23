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

        // 게임 진행
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

            // 멀티일 경우 현재 블럭 위치 전송
            if(gameType == GAME_TYPE.MULTI)
            {
                SocketManager.Instance.UpdateBlock();
            }

            // 새로운 블럭 생성
            TetrisBlockManager.Instance.SpwanRandomBlock();
        }
        // 게임 종료
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

}