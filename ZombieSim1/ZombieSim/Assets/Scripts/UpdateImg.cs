using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateImg : MonoBehaviour
{
    [Header("Properties")]

    public int Length = 128;
    public int Height = 128;

    /// <summary>
    /// Interval between board updates
    /// </summary>
    public float UpdateRate = 0.25F;

    /// <summary>
    /// Number of zombies to place on the map
    /// </summary>
    public int Zombies = 60;

    /// <summary>
    /// Number of survivors to place on the map
    /// </summary>
    public int Survivors = 30;

    /// <summary>
    /// Number of walls to create
    /// </summary>
    public int Walls = 5;

    /// <summary>
    /// Minimum length for walls
    /// </summary>
    public int WallMinLength;

    /// <summary>
    /// Maximum length for walls
    /// </summary>
    public int WallMaxLength;

    /// <summary>
    /// Thickness of walls
    /// </summary>
    public int WallThickness = 3;

    /// <summary>
    /// Normal colors of cells.
    /// </summary>
    public Color Cell_Color;

    /// <summary>
    /// Color to display zombies as
    /// </summary>
    public Color Zombie_Color;

    /// <summary>
    /// Color to display survivors as
    /// </summary>
    public Color Survivor_Color;

    /// <summary>
    /// Color of walls
    /// </summary>
    public Color Wall_Color;


    [Header("Components - Set These!")]

    /// <summary>
    /// Image to update with current board
    /// </summary>
    public RawImage Img_To_Update;

    /// <summary>
    /// Displayed image texture
    /// </summary>
    Texture2D Img_Texture;

    ZombieAI _Zomb_AI;

    /// <summary>
    /// Zombie AI component - gets next generation for example
    /// </summary>
    ZombieAI Zombie_AI
    {
        get
        {
            if (_Zomb_AI == null) _Zomb_AI = FindObjectOfType<ZombieAI>();
            return _Zomb_AI;
        }
    }

    /// <summary>
    /// Time until next update
    /// </summary>
    public float TimeToNextUpdate = 0.0F;

    // Start is called before the first frame update
    void Start()
    {
        //Create image to display
        CreateImg();

        //Initialize zombie AI
        Zombie_AI.Init();

        //Build random walls
        Zombie_AI.BuildWalls(Walls, WallMinLength, WallMaxLength, WallThickness);

        //Randomize zombies, survivors, and empty cells
        Zombie_AI.Randomize(Zombies, Survivors);

    }


     void Update()
    {
        //If timer hits 0, then do update and reset timer.
        if(TimeToNextUpdate <= 0F)
        {
            TimeToNextUpdate = UpdateRate;
            UpdateBoard();
        }
        else
        {
            TimeToNextUpdate -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Create new image with provided length X width
    /// </summary>
    void CreateImg()
    {
        //Create fresh texture
        Img_Texture = new Texture2D(Length, Height);

        //Bind to raw image
        Img_To_Update.texture = Img_Texture;
    }

    /// <summary>
    /// Update the board 
    /// </summary>
    void UpdateBoard()
    {
        //Generate next generation
        Zombie_AI.NextGeneration();

        //Update image
        UpdatePixels();
    }

    /// <summary>
    /// Update pixels based on current zombie states
    /// </summary>
    void UpdatePixels()
    {
        //Final pixels to save to texture
        Color[] FinalPixels = new Color[Length * Height];

        //Loop through each cell. Set colors - empty, zombie, survivor
        for(int x = 0; x < Length; x++)
        {
            for(int y = 0; y < Height; y++)
            {
                //Get state at this position
                int State = Zombie_AI.GetStateAtPos(x, y);

                //Color to set. Default to normal cell color
                Color Final_Cell_Col = Cell_Color;
                
                //set color based on cell state
                switch(State)
                {
                    case 1:
                        Final_Cell_Col = Zombie_Color;
                        break;
                    case 2:
                        Final_Cell_Col = Survivor_Color;
                        break;
                    case 3:
                        Final_Cell_Col = Wall_Color;
                        break;
                }

                //Set this pixel
                int Index = (Height * y) + x;

                //Set final cell color
                FinalPixels[Index] = Final_Cell_Col;
            }
        }


        //Set final pixels
        Img_Texture.SetPixels(FinalPixels);
        Img_Texture.Apply();
        //Img_To_Update.texture = Img_Texture;

    }


}
