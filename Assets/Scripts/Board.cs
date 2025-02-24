using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Numerics;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject tilePrefab;
    // The Private keyword means that you can only access that method within the class it was defined in.
    public GameObject[] dots;
    public GameObject[,] allDots;
    private BackgroundTile[,] allTiles; //Creates the empty two-dimensional array
    

    // Start is called before the first frame update
    void Start()
    {
       allTiles = new BackgroundTile[width, height]; // Tells how big it wants the two attributes (width, height) to be (the array is still empty)
       allDots = new GameObject[width, height];
       SetUp();
    }

    private void SetUp(){
        // First condition (int i = 0;) creates the variable, the Second contition (i < width;) is the requirement needed for the loop to end, and the Third condition (i++) increases it
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
               UnityEngine.Vector2 tempPosition = new UnityEngine.Vector2(i,j);
               GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, UnityEngine.Quaternion.identity) as GameObject;
               backgroundTile.transform.parent = this.transform; // Sets the parent of the GameObject the board object
               backgroundTile.name = "(" + i + "," + j + ")";
               int dotToUse = Random.Range(0, dots.Length); // Choses a random range depending on the amount inside of the gameobject array, avoid the last digit (example: if there is one in the array, this will choose from 0 to 2, but ignore 2)
               
               int maxIterations = 0;
               while(MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100){
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
               }
               maxIterations = 0;

               GameObject dot = Instantiate(dots[dotToUse], tempPosition, UnityEngine.Quaternion.identity);
               dot.transform.parent = this.transform;
               dot.name = "(" + i + "," + j + ")";
               allDots[i,j] = dot;
            }    
        }
    }

    //checks if the current position has matching dots
    private bool MatchesAt(int column, int row, GameObject piece){
        if(column > 1 && row > 1){
            if(allDots[column -1, row].tag == piece.tag && allDots[column -2, row].tag == piece.tag){
                return true;
            }
             if(allDots[column, row -1].tag == piece.tag && allDots[column, row-2].tag == piece.tag){
                return true;
            }
        }else if(column <=1 || row <= 1){
            if(row > 1){
                if(allDots[column, row -1].tag == piece.tag && allDots[column, row -2].tag == piece.tag){
                    return true;
                }
            }
            if(column > 1){
                if(allDots[column -1, row].tag == piece.tag && allDots[column -2, row].tag == piece.tag){
                    return true;
                }
            }
        }

        return false;
    }

    private void DestroyMatchesAt(int column, int row){
        //checks in the dot in the column/row is matched, and destroys the match
        if(allDots[column, row].GetComponent<Dot>().isMatched){
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches(){
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                if (allDots[i,j] != null){
                    DestroyMatchesAt(i,j); 
                }
            }      
        }
        StartCoroutine(DecreaseRowCo());    
    }

    private IEnumerator DecreaseRowCo(){
        int nullCount = 0;
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                if (allDots[i,j] == null){
                    nullCount++;

                }else if (nullCount > 0){
                    allDots[i,j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard(){
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                if (allDots[i,j] == null){
                    UnityEngine.Vector2 tempPosition = new UnityEngine.Vector2(i,j);
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, UnityEngine.Quaternion.identity);
                    allDots[i,j] = piece;


                }

            }
        }
    }

    private bool MatchesOnBoard(){
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
               if(allDots[i,j] != null){
                    if(allDots[i,j].GetComponent<Dot>().isMatched){
                        return true;
                    }
               } 
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo(){
        RefillBoard();
        yield return new WaitForSeconds(.5f);
        
        while(MatchesOnBoard()){
            yield return new WaitForSeconds(.25f);
            DestroyMatches();
        }
    }
}
