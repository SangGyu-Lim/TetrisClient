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

            // float ���� �ذ�
            float x = Mathf.Round(piece.transform.position.x * 100f) / 100f;

            // ��Ʈ���� �� �浹
            if (x > UIBackgroundManager.Instance.borderRightPosition
                || x < UIBackgroundManager.Instance.borderLeftPosition 
                || piece.transform.position.y < UIBackgroundManager.Instance.borderBottomPosition)
            {
                return false;
            }

            // ���� �̹� �����ϴ���
            if (UIBackgroundManager.Instance.IsExistHoldPiece(piece) == true)
                return false;
            
        }

        return true;
    }

    public void OnPlaceBlock(TetrisBlock block)
    {
        block.enabled = false;

        // ���� ����
        if (IsGameEndPosition(block.transform) == false && isGameOver == false)
        {
            // ���� �� �������� ��� �� �� ����.
            for (int i = 0; i < block.transform.childCount; ++i)
            {
                Transform piece = block.transform.GetChild(i);

                UIBackgroundManager.Instance.OnPlacePiece(piece);

            }

            // ���� �� ����
            GameObject.Destroy(block.transform.gameObject);

            UIBackgroundManager.Instance.DeleteRows();

            // ��Ƽ�� ��� ���� �� ��ġ ����
            if(gameType == GAME_TYPE.MULTI)
            {
                SocketManager.Instance.UpdateBlock();
            }

            // ���ο� �� ����
            TetrisBlockManager.Instance.SpwanRandomBlock();
        }
        // ���� ����
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