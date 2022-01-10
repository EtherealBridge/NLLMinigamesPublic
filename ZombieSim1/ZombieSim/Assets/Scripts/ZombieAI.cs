using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    /// <summary>
    /// Current zombie states. 0 = empty cell, 1 = zombie, 2 = survivor, 3 = Wall
    /// </summary>
    public int[,] States;

    /// <summary>
    /// Maximum X value
    /// </summary>
    int Max_X
    {
        get
        {
            return States.GetLength(0);
        }
    }

    /// <summary>
    /// Maximum Y value
    /// </summary>
    int Max_Y
    {
        get
        {
            return States.GetLength(1);
        }
    }

    UpdateImg _UpdImg;

    /// <summary>
    /// Update image class cached
    /// </summary>
    UpdateImg Update_Img
    {
        get
        {
            if (_UpdImg == null) _UpdImg = FindObjectOfType<UpdateImg>();
            return _UpdImg;
        }
    }

    /// <summary>
    /// Initialize zombie AI
    /// </summary>
    public void Init()
    {
        States = new int[Update_Img.Length, Update_Img.Height];
    }

    /// <summary>
    /// Move zombies and living for this generation
    /// </summary>
    public void NextGeneration()
    {
        //Loop through each cell, update it.
        for (int x = 0; x < Max_X; x++)
        {
            for (int y = 0; y < Max_Y; y++)
            {
                UpdateCell(x, y);
            }
        }
    }

    /// <summary>
    /// Update this cell. Move zombies and survivors.
    /// </summary>
    /// <param name="x">Cell X position</param>
    /// <param name="y">Cell Y position</param>
    void UpdateCell(int x, int y)
    {
        //This current cell.
        int ThisState = GetStateAtPos(x, y);

        //If this is a survivor, then
        if (ThisState == 2)
        {
            CheckForZombieBite(x,y);
        }

        //If this is a zombie or survivor, get random move direction
        if (ThisState == 1 || ThisState == 2)
        {
            //Randomized move. X and Y are between -1 and 1. -1 = west / south, 0 = same row / column, 1 = north / east
            int x_move_dir = Random.Range(-1, 2);
            int y_move_dir = Random.Range(-1, 2);

            //Move entity
            MoveToCell(ThisState, x, y, x_move_dir, y_move_dir);
        }
    }

    /// <summary>
    /// Check if this survivor was bitten by a zombie
    /// </summary>
    /// <param name="x">This survivor's current x position</param>
    /// <param name="y">This survivor's current y position</param>
    void CheckForZombieBite(int x, int y)
    {
        //Get zombie neighbors. If any zombie neighbors, bite.
        int ZombieNeighbors = 0;
        for(int a = -1; a <= 1; a++)
        {
            for (int b = -1; b <= 1; b++)
            {
                //Get positions to check
                int check_x = x + a;
                int check_y = y + b;

                //Get neighbor state if it is not this position
                int Neighbor_State = GetStateAtPos(check_x, check_y);
                if (Neighbor_State == 1) ZombieNeighbors += 1;
            }
        }

        //If at least one zombie neighbor, then bite.
        if (ZombieNeighbors > 0) ZombieBite(x, y);
    }

    /// <summary>
    /// If this is a survivor, and neighbor is zombie, then turn
    /// </summary>
    /// <param name="x">X position of the victim cell</param>
    /// <param name="y">Y Position of the victim cell</param>
    void ZombieBite(int x, int y)
    {
        SetState(x, y, 1);
    }

    /// <summary>
    /// Offset cell at x,y by given offset x,y.
    /// </summary>
    /// <param name="CurrState">This cell's current state</param>
    /// <param name="CurrCellX">This cell'x X value</param>
    /// <param name="CurrCellY">This cell's Y value</param>
    /// <param name="OffsetX">Amount to offset this cell by X</param>
    /// <param name="OffsetY">Amount to offset this cell by Y.</param>
    void MoveToCell(int CurrState, int CurrCellX, int CurrCellY, int OffsetX, int OffsetY)
    {
        //Get destinations
        int Dest_Cell_X = Mathf.Clamp(CurrCellX + OffsetX, 0, Max_X - 1);
        int Dest_Cell_Y = Mathf.Clamp(CurrCellY + OffsetY, 0, Max_Y - 1);

        //Get state at this destination
        int Dest_State = GetStateAtPos(Dest_Cell_X, Dest_Cell_Y);

        //Avoid edges
        if (Dest_Cell_X != 0 && Dest_Cell_X != Update_Img.Length && Dest_Cell_Y != 0 && Dest_Cell_Y != Update_Img.Height)
        {

            //Destination must be empty to move to a given destination.
            if (Dest_State == 0)
            {
                //Do switch to simulate movement.
                SetState(CurrCellX, CurrCellY, 0);
                SetState(Dest_Cell_X, Dest_Cell_Y,CurrState);
            }
        }
    }

    /// <summary>
    /// Get state of a cell at x, y
    /// </summary>
    /// <param name="x">Cell X</param>
    /// <param name="y">Cell Y</param>
    /// <returns>The current state of the supplied cell.</returns>
    public int GetStateAtPos(int x, int y)
    {
        //Clamp x and y positions
        x = Mathf.Clamp(x, 0, Max_X - 1);
        y = Mathf.Clamp(y, 0, Max_Y - 1);

        return States[x, y];
    }

    /// <summary>
    /// Randomize zombies and survivors on the map
    /// </summary>
    /// <param name="Zombies">Number of zombies to place on the map</param>
    /// <param name="Survivors">Number of survivors to place on the map.</param>
    public void Randomize(int Zombies, int Survivors)
    {
        // Place zombies
        PlaceRandomOfState(1, Zombies);

        //Place survivors
        PlaceRandomOfState(2, Survivors);
    }

    /// <summary>
    /// Place (State) Number of items on the board
    /// </summary>
    /// <param name="State">The state for this item. 1 = zombies, 2 = survivors</param>
    /// <param name="Amount"></param>
    void PlaceRandomOfState(int State, int Amount)
    {
        //Loop through and place (Amount) number of items of (State) on the board.
        for(int i = 0; i < Amount; i++)
        {
            bool Success = false;
            Vector2 RandLoc = Vector2.zero;

            //Get position to place object that is not something else and is not a wall. Can only place in empty cell
            while (!Success)
            {
                //Get random position
                RandLoc = GetRandomPlacementLoc();
                int Dest_State = GetStateAtPos((int)RandLoc.x, (int)RandLoc.y);
                if(Dest_State == 0)
                {
                    Success = true;
                }
            }

            //Place object here
            SetState((int)RandLoc.x, (int)RandLoc.y, State);
        }
            
    }

    /// <summary>
    /// Spits out a random placement location for zombies or survivors.
    /// </summary>
    /// <returns></returns>
    Vector2 GetRandomPlacementLoc()
    {
        int x = Random.Range(1, Max_X - 1);
        int y = Random.Range(1, Max_Y - 1);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Build walls on the grid
    /// </summary>
    /// <param name="WallCount">Number of walls to build</param>
    /// <param name="MinLength">Wall max length</param>
    /// <param name="MaxLength">Wall min. length</param>
    /// <param name="WallThickness">Wall thickness</param>
    public void BuildWalls(int WallCount, int MinLength, int MaxLength, int WallThickness)
    {
        //Loop through and create walls
        for(int i = 0; i < WallCount; i++)
        {
            //Get randomized position
            Vector2 Loc = GetRandomPlacementLoc();

            //Get build direction. 0 = north, 1 = east, 2 = south, 3 = west
            int BuildDir = Random.Range(0, 3);

            //X and Y directions to build wall
            int x_dir = 0;
            int y_dir = 0;

            //Based on direction, set offset
            switch(BuildDir)
            {
                case 0:
                    y_dir = 1;
                    x_dir = 0;
                    break;
                case 1:
                    y_dir = 0;
                    x_dir = 1;
                    break;
                case 2:
                    y_dir = -1;
                    x_dir = 0;
                    break;
                case 3:
                    y_dir = 0;
                    x_dir = -1;
                    break;
            }

            //Get amount of walls to build
            int WallLength = Random.Range(MinLength, MaxLength);

            //Build wall
            for(int a = 0; a < WallLength; a++)
            {
                //For wall thickness, build
                for (int b = 0; b < WallThickness; b++)
                {
                    //Set state to 3 (wall) at position. Find positionf or this wall based on current block being built for wall.
                    int x_pos = (int)Loc.x + ((a * x_dir) + (b * y_dir));
                    int y_pos = (int)Loc.y + ((a * y_dir) + (b * x_dir));

                    SetState(x_pos, y_pos, 3);
                }
            }
        }
    }


    /// <summary>
    /// Set state at position x,y
    /// </summary>
    /// <param name="x">x position to set state for</param>
    /// <param name="y">y position to set state for</param>
    /// <param name="NewState">New state for this position</param>
    void SetState(int x, int y, int NewState)
    {
        //Do not allow overflow
        if (x < 0 || x >= Max_X) return;
        if (y < 0 || y >= Max_Y) return;

        States[x, y] = NewState;
    }
}
