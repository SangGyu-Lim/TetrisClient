using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BLOCK_TYPE
{
    I,
    T,
    L,
    J,
    S,
    Z,
    O,
    MAX
}

public enum STAGE_TYPE
{
    INTRO,
    GAME,
    MULTI_GAME,
    MAX
}

public enum BACKGROUND_PIECE_TYPE
{
    BLANK,
    HOLD
}

public enum ARROW
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public enum GAME_TYPE
{
    SINGLE,
    MULTI
}

public static class CommonDefine
{
    public const int MAX_BACKGROUND_WIDTH = 10;
    public const int MAX_BACKGROUND_HEIGHT = 16;
    public const float CREATE_BACKGROUND_MOVE_POSITION = -82f;
           
    public const float BLOCK_SIZE = 40f;
          
    public const float DEFAULT_MOVE_TIME = 0.6f;
    public const float FALL_DIRECTLY_MOVE_TIME = 0.001f;
           
    public const float DEFAULT_DOWN_ADD_SPEED_TIME = 0.0f;
    public const float DOWN_ADD_SPEED_TIME = 0.3f;
           
    public const int BLOCK_START_Y_POSITION = -2;
           
    public const float LEVEL_UP_SECOND = 10f;// 기준마다 레벨업.
    public const int MAX_GAME_LEVEL = 10; //최대레벨
           
    public const float LEVEL_UP_SPEED_RATIO = 0.5f;//레벨업당 증가되는 속도.
    public const int DELETE_ONEROW_SCORE = 10;//한줄당 증가 스코어
           
    public const string SINGLE_RANK_LIST = "SINGLE_RANK_LIST";
           
    public const int MAX_RANKING_COUNT = 10;// 랭킹 최대 갯수
}

public static class MultiPlayDefine
{
    public static float MULTI_PLAYER_BLOCK_SIZE = 20f;
}
