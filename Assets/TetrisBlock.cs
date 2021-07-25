using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlock : MonoBehaviour {
        
    float currentMoveTime = 0.0f;

    public bool controlable = true;
    protected float moveSpeedTime = CommonDefine.DEFAULT_MOVE_TIME;
    protected float addMoveSpeedTime = CommonDefine.DEFAULT_DOWN_ADD_SPEED_TIME;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        currentMoveTime += Time.deltaTime + addMoveSpeedTime;

        moveSpeedTime = GameManager.Instance.moveSpeedTime;
        //Falling
        if (controlable == false)
            moveSpeedTime = CommonDefine.FALL_DIRECTLY_MOVE_TIME;

        if (currentMoveTime >= moveSpeedTime)
        {
            MoveFalling();
            currentMoveTime = 0.0f;
        }          
    }

    public void Rotate()
    {
        if (controlable == false)
            return;

        transform.Rotate(new Vector3(0f, 0f, 1f), 90.0f);

        if (GameManager.Instance.IsValidPosition(this.transform) == false)
            transform.Rotate(new Vector3(0f, 0f, 1f), -90.0f);
    }

    public void MoveHorizontal(Vector2 direction)
    {
        if (controlable == false)
            return;

        float moveDistance = CommonDefine.BLOCK_SIZE;
        float deltaMovement = (direction.Equals(Vector2.right)) ? moveDistance : -moveDistance;

        transform.localPosition += new Vector3(deltaMovement, 0, 0);

        if(GameManager.Instance.IsValidPosition(this.transform) == false)
            transform.localPosition += new Vector3(-deltaMovement, 0, 0);


    }

    public void MoveFalling()
    {
        float moveDistance = CommonDefine.BLOCK_SIZE;

        transform.localPosition -= new Vector3(0, moveDistance, 0);

        if (GameManager.Instance.IsValidPosition(this.transform) == false)
        {
            transform.localPosition -= new Vector3(0, -moveDistance, 0);
            GameManager.Instance.OnPlaceBlock(this);
        }
            
    }

    public void MoveFallingDirectly()
    {
        controlable = false;
        moveSpeedTime = CommonDefine.FALL_DIRECTLY_MOVE_TIME;
    }

    public void AddMoveSpeedTime(float addTime)
    {
        addMoveSpeedTime = addTime;
    }
}
