using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Mathematics;
using Unity.Collections;

public class GameOfLife : MonoBehaviour
{
    [SerializeField] private int rows = 50;
    [SerializeField] private int cols = 50;
    [SerializeField] float interval = 0.1f;
    [SerializeField] private GameObject cell;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject pauseButton;

    private HashSet<int2> livingCells = new HashSet<int2>();
    Dictionary<string, bool> livingCellsMap = new Dictionary<string, bool>();

    private HashSet<int2> cellsToLive = new HashSet<int2>();
    private HashSet<int2> cellsToDie = new HashSet<int2>();

    GameObject[,] cubes;
    float time = 0f;

    bool isPlaying = true;

    void Start()
    {
        cubes = new GameObject[rows, cols];

        //Background size
        background.transform.localScale = new Vector3(rows/10, 1, cols/10);

        //position stage
        background.transform.position = new Vector3(rows / 2, 0, cols / 2);
        
        //position and point camera to stage
        Camera.main.transform.position = new Vector3(cols * .7f, cols * .7f, cols / 2);
        Camera.main.transform.LookAt(background.transform);

        Reset();
    }

    void Update()
    {
        if (isPlaying)
        {
            time += Time.deltaTime;
            while (time >= interval)
            {
                Step();
                time = 0;
            }
        }
    }

    // *************************** Functions ***************************


    //***************************
    // Play
    // - Continue Time 
    //***************************
    public void Play()
    {
        pauseButton.SetActive(true);
        playButton.SetActive(false);
        isPlaying = true;
    }

    //***************************
    // Pause
    // - Pause time
    //***************************
    public void Pause()
    {
        pauseButton.SetActive(false);
        playButton.SetActive(true);
        isPlaying = false;
    }

    //***************************
    // Restart
    // - Restarts game
    //***************************
    public void Reset()
    {
        livingCells.Clear();
        livingCellsMap.Clear();
        foreach (GameObject obj in cubes)
        {
            Destroy(obj);
        }
        GetRandomPatterns();
        CreateCells();
        Play();
    }

    //***************************
    // Step
    // - Makes the next step and next generation
    //***************************
    public void Step()
    {
        GetLivingAndDying();
        CreateCells();
        DestroyCells();
    }

    //***************************
    // CreateCells
    // - Creates all cells set in cellsToLive
    //***************************
    private void CreateCells()
    {
        foreach(int2 coord in cellsToLive)
        {
            cubes[coord.x, coord.y] = Instantiate(cell, this.gameObject.transform);
            cubes[coord.x, coord.y].transform.position = new Vector3(coord.x, 0, coord.y);
            livingCellsMap[coord.x + ":" + coord.y] = true;
        }

        cellsToLive.Clear();
    }

    //***************************
    // DestroyCells
    // - Destroys all cells set in cellsToDie
    //***************************
    private void DestroyCells()
    {
        foreach (int2 coord in cellsToDie)
        {
            Destroy(cubes[coord.x, coord.y]);
            livingCellsMap.Remove(coord.x + ":" + coord.y);
        }

        cellsToDie.Clear();
    }

    //***************************
    // GetLivingAndDying
    // - Determines which cells to live and die
    // - Cells to continue are untouched
    //***************************
    private void GetLivingAndDying()
    {
        //List dead cells to check and next gen
        HashSet<int2> deadCellsToCheck = new HashSet<int2>();
        HashSet<int2> nextgenCells = new HashSet<int2>();

        foreach (int2 coord in livingCells)
        {
            //Get surrounding cells and check each
            HashSet<int2> surroundingCells = GetSurroundingCells(coord.x, coord.y);

            int livingSideCells = 0;
            foreach (int2 sideCellCoord in surroundingCells)
            {
                //Lookup if side cell is living
                string key = sideCellCoord.x + ":" + sideCellCoord.y;
                //Debug.Log(key);
                if (livingCellsMap.ContainsKey(key))
                {
                    livingSideCells++;
                }
                else
                {
                    deadCellsToCheck.Add(new int2(sideCellCoord.x, sideCellCoord.y));
                }
            }

            //If not exactly 3 then list to die
            if (livingSideCells == 2 || livingSideCells == 3)
            {
                nextgenCells.Add(new int2(coord.x, coord.y));
            } else
            {
                cellsToDie.Add(new int2(coord.x, coord.y));
            }

        }

        //Check who needs to be born
        foreach (int2 coord in deadCellsToCheck)
        {
            HashSet<int2> surroundingCells = GetSurroundingCells(coord.x, coord.y);

            int livingSideCells = 0;
            foreach (int2 sideCellCoord in surroundingCells)
            {
                //Lookup if side cell is living
                string key = sideCellCoord.x + ":" + sideCellCoord.y;
                if (livingCellsMap.ContainsKey(key))
                {
                    livingSideCells++;
                }
            }

            if (livingSideCells == 3)
            {
                cellsToLive.Add(new int2(coord.x, coord.y));
                nextgenCells.Add(new int2(coord.x, coord.y));
            }
        }

        livingCells = nextgenCells;
    }

    //***************************
    // GetSurroundingCells
    // - Gets indices of surrounding cells
    //***************************
    private HashSet<int2> GetSurroundingCells(int x, int y)
    {
        HashSet<int2> result = new HashSet<int2>();

        int currentX, currentY;

        //Top Left
        currentX = x - 1;
        if (currentX < 0) { currentX = rows - 1; }
        currentY = y - 1;
        if (currentY < 0) { currentY = cols - 1; }
        result.Add(new int2(currentX, currentY));

        //Top
        currentX = x;
        if (currentY < 0) { currentY = cols - 1; }
        result.Add(new int2(currentX, currentY));

        //Top Right
        currentX = x + 1;
        if (currentX > rows - 1) { currentX = 0; }
        if (currentY < 0) { currentY = cols - 1; }
        result.Add(new int2(currentX, currentY));

        // Left
        currentX = x - 1;
        if (currentX < 0) { currentX = rows - 1; }
        currentY = y;
        result.Add(new int2(currentX, currentY));

        // right
        currentX = x + 1;
        if (currentX > rows - 1) { currentX = 0; }
        result.Add(new int2(currentX, currentY));

        //Bottom Left
        currentX = x - 1;
        if (currentX < 0) { currentX = rows - 1; }
        currentY = y + 1;
        if (currentY > cols - 1) { currentY = 0; }
        result.Add(new int2(currentX, currentY));

        //Bottom
        currentX = x;
        result.Add(new int2(currentX, currentY));

        //Bottom Right
        currentX = x + 1;
        if (currentX > rows - 1) { currentX = 0; }
        result.Add(new int2(currentX, currentY));

        return result;
    }

    //***************************
    // GetRandomPatterns
    // - Gets random number of random patterns & sets them
    //***************************
    void GetRandomPatterns()
    {
        var patterns = new Patterns();

        int listCount = patterns.list.Count-1;
        
        for(var i=0; i < 6; i++)
        {
            HashSet<int2> pattern = patterns.list[UnityEngine.Random.Range(0, listCount)];

            //Position in 1,2,3 or 4th quadrant
            int startX = 0;
            int startY = 0;

            if (i == 0)
            {
                startX = rows / 4;
                startY = cols / 4;
            }
            else if(i == 1)
            {
                startX = rows / 4 * 2;
                startY = cols / 4 * 2;
            }
            else if (i == 2)
            {
                startX = rows / 4;
                startY = cols / 4 * 2;
            }
            else if (i == 3)
            {
                startX = rows / 4 * 2;
                startY = cols / 4;
            }
            else if (i == 4)
            {
                startX = rows / 4;
                startY = cols / 4 * 3;
            }
            else if (i == 5)
            {
                startX = rows / 4 * 2;
                startY = cols / 4 * 3;
            }

            foreach (int2 coords in pattern)
            {
                var newCoord = new int2(coords.x + startX, coords.y + startY);
                cellsToLive.Add(newCoord);
                livingCells.Add(newCoord);
            }
        }

    }
}
