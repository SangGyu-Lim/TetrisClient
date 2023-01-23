using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using System;
using LitJson;

public class SocketManager : MonoBehaviour
{
    #region SingleTon

    static SocketManager instance = null;
    public static SocketManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    #endregion

    public void ConnectMulti()
    {
        Socket = new Client(url);
        Socket.Opened += SocketOpened;
        Socket.Message += SocketMessage;
        Socket.SocketConnectionClosed += SocketConnectionClosed;
        Socket.Error += SocketError;
        Socket.Connect();

        Socket.On("USER_CONNECTED", OnUserConnected);
        Socket.On("USER_READY", OnUserReady);
        Socket.On("GAME_START", OnGameStart);
        Socket.On("UPDATE_BLOCK", OnUpdateBlock);
        Socket.On("GAME_OVER", OnGameOver);
    }

    public void StartMulti(string nickName)
    {
        StartCoroutine(ConnectToServer(nickName));
    }

    IEnumerator ConnectToServer(string nickName)
    {
        yield return new WaitForSeconds(0.5f);

        Dictionary<string, string> data = new Dictionary<string, string>();
        data["name"] = nickName;

        string str = JsonMapper.ToJson(data);
        Socket.Emit("USER_CONNECT", str);

        yield return new WaitForSeconds(0.5f);

        Socket.Emit("USER_READY", null);

        yield return new WaitForSeconds(0.5f);

        Socket.Emit("CHECK_MULTI", null);
    }

    void OnGameOver(SocketIOClient.Messages.IMessage e)
    {
        Debug.LogWarning("OnGameOver");

        GameManager.Instance.OnGameOverMultiPlay();
    }
}