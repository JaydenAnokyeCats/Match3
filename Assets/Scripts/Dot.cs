using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.ExceptionServices;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;


public class Dot : MonoBehaviour
{   
    [Header("Border Variables")]
    public int column; //The current column index of the dot on the board
    public int row;// The current row index of the dot on the board
    public int previousColumn;//Previous column inex for reverting moves
    public int previousRow;//Previous row index for reverting moves

    [Header("Position Variables")]
    public int targetX;
    public int targetY; // Helps to move the pieces around, along with target X
    public bool isMatched = false; 

    private FindMatches findMatches;
    private Board board;
    private GameObject otherDot; //Reference to the other dot being swapped with
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    // Start is called before the first frame update
    void Start()
    {   

        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //column = targetX;
        //row = targetY;
        //previousRow = row;
        //previousColumn = column;


    }

    // Update is called once per frame
    void Update()
    {   //FindMatches();
        if (isMatched){
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(1f, 1f, 1f, .2f);
        }
        targetX = column;
        targetY = row; // Main engine for moving the pieces, along with targetX

        //Moves towards the target X position
        if (Mathf.Abs(targetX - transform.position.x) > .1){
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if(board.allDots[column, row] != this.gameObject){
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();

        }else{
            //Set the position directly
            tempPosition = new Vector2(targetX,transform.position.y);
            transform.position=tempPosition;
            
        }
        if (Mathf.Abs(targetY - transform.position.y) > .1){
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if(board.allDots[column, row] != this.gameObject){
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
            
        }else{
            //Set the position directly
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position=tempPosition;
            
        }
    }


    public IEnumerator CheckMoveCo(){
        yield return new WaitForSeconds(.5f);
        if(otherDot != null){
            if(!isMatched && !otherDot.GetComponent<Dot>().isMatched){
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.25f);
                board.currentState = GameState.move;
            }else{
                board.DestroyMatches(); 
           
            }
            otherDot = null;
        }
    }
    private void OnMouseDown()
    {
        if(board.currentState == GameState.move){ //checks if this equation is true, and if so, allows for the player to move once more
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log(firstTouchPosition);
        }
    }

    private void OnMouseUp()
    {
        if(board.currentState == GameState.move){
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle(){
        if(Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y)> swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x)> swipeResist)
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            //Debug.Log(swipeAngle);
            MovePieces();
            board.currentState = GameState.wait; //sets gamestate to wait after calculating the angle
        }else{
            board.currentState = GameState.move;
        }
    }

    void MovePieces(){
        if(swipeAngle > -45 && swipeAngle <= 45 && column < board.width-1){
            //This will be a Right swipe
            otherDot = board.allDots[column + 1, row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().column -=1;
            column += 1;
        } else if(swipeAngle > 45 && swipeAngle <= 135 && row < board.height-1){
            //This will be an Up swipe
            otherDot = board.allDots[column , row + 1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().row -=1;
            row += 1;
        } else if((swipeAngle > 135 || swipeAngle <= -135) && column > 0){
            //This will be a Left swipe
            otherDot = board.allDots[column - 1, row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().column +=1;
            column -= 1;
        } else if(swipeAngle < -45 && swipeAngle >= -135 && row > 0){
            //This will be a Down swipe
            otherDot = board.allDots[column, row - 1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().row +=1;
            row -= 1;
        }
        StartCoroutine(CheckMoveCo());
    }

    void FindMatches(){
        if(column > 0 && column < board.width - 1){
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            //v Checks to see if the left and right dot tags are equal to the game object tag
            if(leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.gameObject.GetComponent<Dot>().isMatched = true;
                    rightDot1.gameObject.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }         
        }
        if(row > 0 && row < board.height - 1){
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            //v Checks to see if the left and right dot tags are equal to the game object tag
            if(upDot1 != null && downDot1 != null) 
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.gameObject.GetComponent<Dot>().isMatched = true;
                    downDot1.gameObject.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }
}

