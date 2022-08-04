using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class script : MonoBehaviour {
    public KMBombInfo bomb;
    public KMAudio audio;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool moduleSolved = false;

    bool incorrect = false;

    public KMSelectable[] numberButtons;
    public KMSelectable[] colorButtons;
    public KMSelectable[] fillButtons;
    public KMSelectable[] shapeButtons;
    public KMSelectable[] cardButtons;
    int[] selectedTraits = new int[4] { 3, 3, 3, 3 };
    public Renderer[] buttonRenderers;
    public Material selectedMaterial;
    public Material unselectedMaterial;

    bool[,,,] containsShape = new bool[3, 3, 3, 3];
    bool[,,,] cardVisible = new bool[3, 3, 3, 3];
    int shapesPlaced = 0;
    int shapesVisible = 0;
    public int[,,,] boardShapes = new int[3, 3, 3, 3];
    public int[,,,] boardFills = new int[3, 3, 3, 3];
    public int[,,,] boardColors = new int[3, 3, 3, 3];
    public int[,,,] boardNums = new int[3, 3, 3, 3];
    List<int[]> cluePositions = new List<int[]>();
    public bool[,,,] clueShapes = new bool[3, 3, 3, 3];

    //Debugging shapes
    string[] shapeNames = new string[3] { "Capsule", "Diamond", "Squiggle" };
    string[] fillNames = new string[3] { "Empty", "Striped", "Solid" };
    string[] colorNames = new string[3] { "Red", "Yellow", "Blue" };

    string[] shapeLetters = new string[3] { "C", "D", "S" };
    string[] fillLetters = new string[3] { "□", "◪", "■" };
    string[] colorLetters = new string[3] { "R", "Y", "B" };

    //Coordinate names so they're more easily readable
    string[] horizontalNames = new string[3] { "Left", "Middle", "Right" };
    string[] verticalNames = new string[3] { "Top", "Middle", "Bottom" };

    public GameObject[] cardObjects;
    public Renderer[] cardSymbols;
    public Texture[] cardTextures;
    public Texture wrongTexture;

    public Renderer[] cardBacks;
    public Texture[] solveTextures;
    List<int[]> solveUnflippedCards = new List<int[]>();
    public Material solvedMaterial;

    List<int[]> autosolveCardOrder = new List<int[]>();
    bool cardInAnimation = false;

    bool tpNoCard = false;

    // Use this for initialization
    void Start () {
        while (shapesPlaced < 81)
        {
            choosePosition:
            int randomx = Rnd.Range(0, 3);
            int randomy = Rnd.Range(0, 3);
            int randomz = Rnd.Range(0, 3);
            int randomw = Rnd.Range(0, 3);

            int randomShape = Rnd.Range(0, 3);
            int randomFill = Rnd.Range(0, 3);
            int randomColor = Rnd.Range(0, 3);
            int randomNumber = Rnd.Range(0, 3);
            if (containsShape[randomx, randomy, randomz, randomw] || clueShapes[randomNumber, randomColor, randomFill, randomShape])
            {
                goto choosePosition;
            }
            cluePositions.Add( new int[4] { randomx, randomy, randomz, randomw});
            clueShapes[randomNumber, randomColor, randomFill, randomShape] = true;

            containsShape[randomx, randomy, randomz, randomw] = true;
            cardVisible[randomx, randomy, randomz, randomw] = true;
            boardShapes[randomx, randomy, randomz, randomw] = randomShape;
            boardFills[randomx, randomy, randomz, randomw] = randomFill;
            boardColors[randomx, randomy, randomz, randomw] = randomColor;
            boardNums[randomx, randomy, randomz, randomw] = randomNumber;
            cardSymbols[(randomx * 27) + (randomy * 9) + (randomz * 3) + randomw].material.mainTexture = cardTextures[(randomShape * 27) + (randomFill * 9) + (randomColor * 3) + randomNumber];
            shapesPlaced++;
            shapesVisible++;
            DebugMsg("Placing clue " + (boardNums[randomx, randomy, randomz, randomw] + 1) + " " + fillNames[boardFills[randomx, randomy, randomz, randomw]] + " " + colorNames[boardColors[randomx, randomy, randomz, randomw]] + " " + shapeNames[boardShapes[randomx, randomy, randomz, randomw]] + " at " + verticalNames[randomx] + " " + horizontalNames[randomy] + ", " + verticalNames[randomz] + " " + horizontalNames[randomw]);
            DebugMsg("Number of cards placed: " + shapesPlaced);
            generateBoard(randomx, randomy, randomz, randomw);
        }

        DebugMsg("Numbers:");
        DebugMsg("" + (boardNums[0, 0, 0, 0] + 1) + (boardNums[0, 0, 0, 1] + 1) + (boardNums[0, 0, 0, 2] + 1) + (boardNums[0, 1, 0, 0] + 1) + (boardNums[0, 1, 0, 1] + 1) + (boardNums[0, 1, 0, 2] + 1) + (boardNums[0, 2, 0, 0] + 1) + (boardNums[0, 2, 0, 1] + 1) + (boardNums[0, 2, 0, 2] + 1));
        DebugMsg("" + (boardNums[0, 0, 1, 0] + 1) + (boardNums[0, 0, 1, 1] + 1) + (boardNums[0, 0, 1, 2] + 1) + (boardNums[0, 1, 1, 0] + 1) + (boardNums[0, 1, 1, 1] + 1) + (boardNums[0, 1, 1, 2] + 1) + (boardNums[0, 2, 1, 0] + 1) + (boardNums[0, 2, 1, 1] + 1) + (boardNums[0, 2, 1, 2] + 1));
        DebugMsg("" + (boardNums[0, 0, 2, 0] + 1) + (boardNums[0, 0, 2, 1] + 1) + (boardNums[0, 0, 2, 2] + 1) + (boardNums[0, 1, 2, 0] + 1) + (boardNums[0, 1, 2, 1] + 1) + (boardNums[0, 1, 2, 2] + 1) + (boardNums[0, 2, 2, 0] + 1) + (boardNums[0, 2, 2, 1] + 1) + (boardNums[0, 2, 2, 2] + 1));
        DebugMsg("" + (boardNums[1, 0, 0, 0] + 1) + (boardNums[1, 0, 0, 1] + 1) + (boardNums[1, 0, 0, 2] + 1) + (boardNums[1, 1, 0, 0] + 1) + (boardNums[1, 1, 0, 1] + 1) + (boardNums[1, 1, 0, 2] + 1) + (boardNums[1, 2, 0, 0] + 1) + (boardNums[1, 2, 0, 1] + 1) + (boardNums[1, 2, 0, 2] + 1));
        DebugMsg("" + (boardNums[1, 0, 1, 0] + 1) + (boardNums[1, 0, 1, 1] + 1) + (boardNums[1, 0, 1, 2] + 1) + (boardNums[1, 1, 1, 0] + 1) + (boardNums[1, 1, 1, 1] + 1) + (boardNums[1, 1, 1, 2] + 1) + (boardNums[1, 2, 1, 0] + 1) + (boardNums[1, 2, 1, 1] + 1) + (boardNums[1, 2, 1, 2] + 1));
        DebugMsg("" + (boardNums[1, 0, 2, 0] + 1) + (boardNums[1, 0, 2, 1] + 1) + (boardNums[1, 0, 2, 2] + 1) + (boardNums[1, 1, 2, 0] + 1) + (boardNums[1, 1, 2, 1] + 1) + (boardNums[1, 1, 2, 2] + 1) + (boardNums[1, 2, 2, 0] + 1) + (boardNums[1, 2, 2, 1] + 1) + (boardNums[1, 2, 2, 2] + 1));
        DebugMsg("" + (boardNums[2, 0, 0, 0] + 1) + (boardNums[2, 0, 0, 1] + 1) + (boardNums[2, 0, 0, 2] + 1) + (boardNums[2, 1, 0, 0] + 1) + (boardNums[2, 1, 0, 1] + 1) + (boardNums[2, 1, 0, 2] + 1) + (boardNums[2, 2, 0, 0] + 1) + (boardNums[2, 2, 0, 1] + 1) + (boardNums[2, 2, 0, 2] + 1));
        DebugMsg("" + (boardNums[2, 0, 1, 0] + 1) + (boardNums[2, 0, 1, 1] + 1) + (boardNums[2, 0, 1, 2] + 1) + (boardNums[2, 1, 1, 0] + 1) + (boardNums[2, 1, 1, 1] + 1) + (boardNums[2, 1, 1, 2] + 1) + (boardNums[2, 2, 1, 0] + 1) + (boardNums[2, 2, 1, 1] + 1) + (boardNums[2, 2, 1, 2] + 1));
        DebugMsg("" + (boardNums[2, 0, 2, 0] + 1) + (boardNums[2, 0, 2, 1] + 1) + (boardNums[2, 0, 2, 2] + 1) + (boardNums[2, 1, 2, 0] + 1) + (boardNums[2, 1, 2, 1] + 1) + (boardNums[2, 1, 2, 2] + 1) + (boardNums[2, 2, 2, 0] + 1) + (boardNums[2, 2, 2, 1] + 1) + (boardNums[2, 2, 2, 2] + 1));
        DebugMsg("Colors:");
        DebugMsg("" + colorLetters[boardColors[0, 0, 0, 0]] + colorLetters[boardColors[0, 0, 0, 1]] + colorLetters[boardColors[0, 0, 0, 2]] + colorLetters[boardColors[0, 1, 0, 0]] + colorLetters[boardColors[0, 1, 0, 1]] + colorLetters[boardColors[0, 1, 0, 2]] + colorLetters[boardColors[0, 2, 0, 0]] + colorLetters[boardColors[0, 2, 0, 1]] + colorLetters[boardColors[0, 2, 0, 2]]);
        DebugMsg("" + colorLetters[boardColors[0, 0, 1, 0]] + colorLetters[boardColors[0, 0, 1, 1]] + colorLetters[boardColors[0, 0, 1, 2]] + colorLetters[boardColors[0, 1, 1, 0]] + colorLetters[boardColors[0, 1, 1, 1]] + colorLetters[boardColors[0, 1, 1, 2]] + colorLetters[boardColors[0, 2, 1, 0]] + colorLetters[boardColors[0, 2, 1, 1]] + colorLetters[boardColors[0, 2, 1, 2]]);
        DebugMsg("" + colorLetters[boardColors[0, 0, 2, 0]] + colorLetters[boardColors[0, 0, 2, 1]] + colorLetters[boardColors[0, 0, 2, 2]] + colorLetters[boardColors[0, 1, 2, 0]] + colorLetters[boardColors[0, 1, 2, 1]] + colorLetters[boardColors[0, 1, 2, 2]] + colorLetters[boardColors[0, 2, 2, 0]] + colorLetters[boardColors[0, 2, 2, 1]] + colorLetters[boardColors[0, 2, 2, 2]]);
        DebugMsg("" + colorLetters[boardColors[1, 0, 0, 0]] + colorLetters[boardColors[1, 0, 0, 1]] + colorLetters[boardColors[1, 0, 0, 2]] + colorLetters[boardColors[1, 1, 0, 0]] + colorLetters[boardColors[1, 1, 0, 1]] + colorLetters[boardColors[1, 1, 0, 2]] + colorLetters[boardColors[1, 2, 0, 0]] + colorLetters[boardColors[1, 2, 0, 1]] + colorLetters[boardColors[1, 2, 0, 2]]);
        DebugMsg("" + colorLetters[boardColors[1, 0, 1, 0]] + colorLetters[boardColors[1, 0, 1, 1]] + colorLetters[boardColors[1, 0, 1, 2]] + colorLetters[boardColors[1, 1, 1, 0]] + colorLetters[boardColors[1, 1, 1, 1]] + colorLetters[boardColors[1, 1, 1, 2]] + colorLetters[boardColors[1, 2, 1, 0]] + colorLetters[boardColors[1, 2, 1, 1]] + colorLetters[boardColors[1, 2, 1, 2]]);
        DebugMsg("" + colorLetters[boardColors[1, 0, 2, 0]] + colorLetters[boardColors[1, 0, 2, 1]] + colorLetters[boardColors[1, 0, 2, 2]] + colorLetters[boardColors[1, 1, 2, 0]] + colorLetters[boardColors[1, 1, 2, 1]] + colorLetters[boardColors[1, 1, 2, 2]] + colorLetters[boardColors[1, 2, 2, 0]] + colorLetters[boardColors[1, 2, 2, 1]] + colorLetters[boardColors[1, 2, 2, 2]]);
        DebugMsg("" + colorLetters[boardColors[2, 0, 0, 0]] + colorLetters[boardColors[2, 0, 0, 1]] + colorLetters[boardColors[2, 0, 0, 2]] + colorLetters[boardColors[2, 1, 0, 0]] + colorLetters[boardColors[2, 1, 0, 1]] + colorLetters[boardColors[2, 1, 0, 2]] + colorLetters[boardColors[2, 2, 0, 0]] + colorLetters[boardColors[2, 2, 0, 1]] + colorLetters[boardColors[2, 2, 0, 2]]);
        DebugMsg("" + colorLetters[boardColors[2, 0, 1, 0]] + colorLetters[boardColors[2, 0, 1, 1]] + colorLetters[boardColors[2, 0, 1, 2]] + colorLetters[boardColors[2, 1, 1, 0]] + colorLetters[boardColors[2, 1, 1, 1]] + colorLetters[boardColors[2, 1, 1, 2]] + colorLetters[boardColors[2, 2, 1, 0]] + colorLetters[boardColors[2, 2, 1, 1]] + colorLetters[boardColors[2, 2, 1, 2]]);
        DebugMsg("" + colorLetters[boardColors[2, 0, 2, 0]] + colorLetters[boardColors[2, 0, 2, 1]] + colorLetters[boardColors[2, 0, 2, 2]] + colorLetters[boardColors[2, 1, 2, 0]] + colorLetters[boardColors[2, 1, 2, 1]] + colorLetters[boardColors[2, 1, 2, 2]] + colorLetters[boardColors[2, 2, 2, 0]] + colorLetters[boardColors[2, 2, 2, 1]] + colorLetters[boardColors[2, 2, 2, 2]]);
        DebugMsg("Fills:");
        DebugMsg("" + fillLetters[boardFills[0, 0, 0, 0]] + fillLetters[boardFills[0, 0, 0, 1]] + fillLetters[boardFills[0, 0, 0, 2]] + fillLetters[boardFills[0, 1, 0, 0]] + fillLetters[boardFills[0, 1, 0, 1]] + fillLetters[boardFills[0, 1, 0, 2]] + fillLetters[boardFills[0, 2, 0, 0]] + fillLetters[boardFills[0, 2, 0, 1]] + fillLetters[boardFills[0, 2, 0, 2]]);
        DebugMsg("" + fillLetters[boardFills[0, 0, 1, 0]] + fillLetters[boardFills[0, 0, 1, 1]] + fillLetters[boardFills[0, 0, 1, 2]] + fillLetters[boardFills[0, 1, 1, 0]] + fillLetters[boardFills[0, 1, 1, 1]] + fillLetters[boardFills[0, 1, 1, 2]] + fillLetters[boardFills[0, 2, 1, 0]] + fillLetters[boardFills[0, 2, 1, 1]] + fillLetters[boardFills[0, 2, 1, 2]]);
        DebugMsg("" + fillLetters[boardFills[0, 0, 2, 0]] + fillLetters[boardFills[0, 0, 2, 1]] + fillLetters[boardFills[0, 0, 2, 2]] + fillLetters[boardFills[0, 1, 2, 0]] + fillLetters[boardFills[0, 1, 2, 1]] + fillLetters[boardFills[0, 1, 2, 2]] + fillLetters[boardFills[0, 2, 2, 0]] + fillLetters[boardFills[0, 2, 2, 1]] + fillLetters[boardFills[0, 2, 2, 2]]);
        DebugMsg("" + fillLetters[boardFills[1, 0, 0, 0]] + fillLetters[boardFills[1, 0, 0, 1]] + fillLetters[boardFills[1, 0, 0, 2]] + fillLetters[boardFills[1, 1, 0, 0]] + fillLetters[boardFills[1, 1, 0, 1]] + fillLetters[boardFills[1, 1, 0, 2]] + fillLetters[boardFills[1, 2, 0, 0]] + fillLetters[boardFills[1, 2, 0, 1]] + fillLetters[boardFills[1, 2, 0, 2]]);
        DebugMsg("" + fillLetters[boardFills[1, 0, 1, 0]] + fillLetters[boardFills[1, 0, 1, 1]] + fillLetters[boardFills[1, 0, 1, 2]] + fillLetters[boardFills[1, 1, 1, 0]] + fillLetters[boardFills[1, 1, 1, 1]] + fillLetters[boardFills[1, 1, 1, 2]] + fillLetters[boardFills[1, 2, 1, 0]] + fillLetters[boardFills[1, 2, 1, 1]] + fillLetters[boardFills[1, 2, 1, 2]]);
        DebugMsg("" + fillLetters[boardFills[1, 0, 2, 0]] + fillLetters[boardFills[1, 0, 2, 1]] + fillLetters[boardFills[1, 0, 2, 2]] + fillLetters[boardFills[1, 1, 2, 0]] + fillLetters[boardFills[1, 1, 2, 1]] + fillLetters[boardFills[1, 1, 2, 2]] + fillLetters[boardFills[1, 2, 2, 0]] + fillLetters[boardFills[1, 2, 2, 1]] + fillLetters[boardFills[1, 2, 2, 2]]);
        DebugMsg("" + fillLetters[boardFills[2, 0, 0, 0]] + fillLetters[boardFills[2, 0, 0, 1]] + fillLetters[boardFills[2, 0, 0, 2]] + fillLetters[boardFills[2, 1, 0, 0]] + fillLetters[boardFills[2, 1, 0, 1]] + fillLetters[boardFills[2, 1, 0, 2]] + fillLetters[boardFills[2, 2, 0, 0]] + fillLetters[boardFills[2, 2, 0, 1]] + fillLetters[boardFills[2, 2, 0, 2]]);
        DebugMsg("" + fillLetters[boardFills[2, 0, 1, 0]] + fillLetters[boardFills[2, 0, 1, 1]] + fillLetters[boardFills[2, 0, 1, 2]] + fillLetters[boardFills[2, 1, 1, 0]] + fillLetters[boardFills[2, 1, 1, 1]] + fillLetters[boardFills[2, 1, 1, 2]] + fillLetters[boardFills[2, 2, 1, 0]] + fillLetters[boardFills[2, 2, 1, 1]] + fillLetters[boardFills[2, 2, 1, 2]]);
        DebugMsg("" + fillLetters[boardFills[2, 0, 2, 0]] + fillLetters[boardFills[2, 0, 2, 1]] + fillLetters[boardFills[2, 0, 2, 2]] + fillLetters[boardFills[2, 1, 2, 0]] + fillLetters[boardFills[2, 1, 2, 1]] + fillLetters[boardFills[2, 1, 2, 2]] + fillLetters[boardFills[2, 2, 2, 0]] + fillLetters[boardFills[2, 2, 2, 1]] + fillLetters[boardFills[2, 2, 2, 2]]);
        DebugMsg("Shapes:");
        DebugMsg("" + shapeLetters[boardShapes[0, 0, 0, 0]] + shapeLetters[boardShapes[0, 0, 0, 1]] + shapeLetters[boardShapes[0, 0, 0, 2]] + shapeLetters[boardShapes[0, 1, 0, 0]] + shapeLetters[boardShapes[0, 1, 0, 1]] + shapeLetters[boardShapes[0, 1, 0, 2]] + shapeLetters[boardShapes[0, 2, 0, 0]] + shapeLetters[boardShapes[0, 2, 0, 1]] + shapeLetters[boardShapes[0, 2, 0, 2]]);
        DebugMsg("" + shapeLetters[boardShapes[0, 0, 1, 0]] + shapeLetters[boardShapes[0, 0, 1, 1]] + shapeLetters[boardShapes[0, 0, 1, 2]] + shapeLetters[boardShapes[0, 1, 1, 0]] + shapeLetters[boardShapes[0, 1, 1, 1]] + shapeLetters[boardShapes[0, 1, 1, 2]] + shapeLetters[boardShapes[0, 2, 1, 0]] + shapeLetters[boardShapes[0, 2, 1, 1]] + shapeLetters[boardShapes[0, 2, 1, 2]]);
        DebugMsg("" + shapeLetters[boardShapes[0, 0, 2, 0]] + shapeLetters[boardShapes[0, 0, 2, 1]] + shapeLetters[boardShapes[0, 0, 2, 2]] + shapeLetters[boardShapes[0, 1, 2, 0]] + shapeLetters[boardShapes[0, 1, 2, 1]] + shapeLetters[boardShapes[0, 1, 2, 2]] + shapeLetters[boardShapes[0, 2, 2, 0]] + shapeLetters[boardShapes[0, 2, 2, 1]] + shapeLetters[boardShapes[0, 2, 2, 2]]);
        DebugMsg("" + shapeLetters[boardShapes[1, 0, 0, 0]] + shapeLetters[boardShapes[1, 0, 0, 1]] + shapeLetters[boardShapes[1, 0, 0, 2]] + shapeLetters[boardShapes[1, 1, 0, 0]] + shapeLetters[boardShapes[1, 1, 0, 1]] + shapeLetters[boardShapes[1, 1, 0, 2]] + shapeLetters[boardShapes[1, 2, 0, 0]] + shapeLetters[boardShapes[1, 2, 0, 1]] + shapeLetters[boardShapes[1, 2, 0, 2]]);
        DebugMsg("" + shapeLetters[boardShapes[1, 0, 1, 0]] + shapeLetters[boardShapes[1, 0, 1, 1]] + shapeLetters[boardShapes[1, 0, 1, 2]] + shapeLetters[boardShapes[1, 1, 1, 0]] + shapeLetters[boardShapes[1, 1, 1, 1]] + shapeLetters[boardShapes[1, 1, 1, 2]] + shapeLetters[boardShapes[1, 2, 1, 0]] + shapeLetters[boardShapes[1, 2, 1, 1]] + shapeLetters[boardShapes[1, 2, 1, 2]]);
        DebugMsg("" + shapeLetters[boardShapes[1, 0, 2, 0]] + shapeLetters[boardShapes[1, 0, 2, 1]] + shapeLetters[boardShapes[1, 0, 2, 2]] + shapeLetters[boardShapes[1, 1, 2, 0]] + shapeLetters[boardShapes[1, 1, 2, 1]] + shapeLetters[boardShapes[1, 1, 2, 2]] + shapeLetters[boardShapes[1, 2, 2, 0]] + shapeLetters[boardShapes[1, 2, 2, 1]] + shapeLetters[boardShapes[1, 2, 2, 2]]);
        DebugMsg("" + shapeLetters[boardShapes[2, 0, 0, 0]] + shapeLetters[boardShapes[2, 0, 0, 1]] + shapeLetters[boardShapes[2, 0, 0, 2]] + shapeLetters[boardShapes[2, 1, 0, 0]] + shapeLetters[boardShapes[2, 1, 0, 1]] + shapeLetters[boardShapes[2, 1, 0, 2]] + shapeLetters[boardShapes[2, 2, 0, 0]] + shapeLetters[boardShapes[2, 2, 0, 1]] + shapeLetters[boardShapes[2, 2, 0, 2]]);
        DebugMsg("" + shapeLetters[boardShapes[2, 0, 1, 0]] + shapeLetters[boardShapes[2, 0, 1, 1]] + shapeLetters[boardShapes[2, 0, 1, 2]] + shapeLetters[boardShapes[2, 1, 1, 0]] + shapeLetters[boardShapes[2, 1, 1, 1]] + shapeLetters[boardShapes[2, 1, 1, 2]] + shapeLetters[boardShapes[2, 2, 1, 0]] + shapeLetters[boardShapes[2, 2, 1, 1]] + shapeLetters[boardShapes[2, 2, 1, 2]]);
        DebugMsg("" + shapeLetters[boardShapes[2, 0, 2, 0]] + shapeLetters[boardShapes[2, 0, 2, 1]] + shapeLetters[boardShapes[2, 0, 2, 2]] + shapeLetters[boardShapes[2, 1, 2, 0]] + shapeLetters[boardShapes[2, 1, 2, 1]] + shapeLetters[boardShapes[2, 1, 2, 2]] + shapeLetters[boardShapes[2, 2, 2, 0]] + shapeLetters[boardShapes[2, 2, 2, 1]] + shapeLetters[boardShapes[2, 2, 2, 2]]);
    }

    void generateBoard (int xcoord, int ycoord, int zcoord, int wcoord)
    {
        for(int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                for (int k = -1; k < 2; k++)
                {
                    for (int l = -1; l < 2; l++)
                    {
                        if (containsShape[(xcoord + i + 3) % 3, (ycoord + j + 3) % 3, (zcoord + k + 3) % 3, (wcoord + l + 3) % 3] /*the space in this direction has a shape*/ && !containsShape[(xcoord - i + 3) % 3, (ycoord - j + 3) % 3, (zcoord - k + 3) % 3, (wcoord - l + 3) % 3] /*there is not a shape in the next space/the opposite direction*/)
                        {
                            int newxcoord = (xcoord - i + 3) % 3;
                            int newycoord = (ycoord - j + 3) % 3;
                            int newzcoord = (zcoord - k + 3) % 3;
                            int newwcoord = (wcoord - l + 3) % 3;

                            int otherxcoord = (xcoord + i + 3) % 3;
                            int otherycoord = (ycoord + j + 3) % 3;
                            int otherzcoord = (zcoord + k + 3) % 3;
                            int otherwcoord = (wcoord + l + 3) % 3;

                            boardShapes[newxcoord, newycoord, newzcoord, newwcoord] = (boardShapes[xcoord, ycoord, zcoord, wcoord] + 3 + (boardShapes[xcoord, ycoord, zcoord, wcoord] - boardShapes[otherxcoord, otherycoord, otherzcoord, otherwcoord])) % 3;
                            boardFills[newxcoord, newycoord, newzcoord, newwcoord] = (boardFills[xcoord, ycoord, zcoord, wcoord] + 3 + (boardFills[xcoord, ycoord, zcoord, wcoord] - boardFills[otherxcoord, otherycoord, otherzcoord, otherwcoord])) % 3;
                            boardNums[newxcoord, newycoord, newzcoord, newwcoord] = (boardNums[xcoord, ycoord, zcoord, wcoord] + 3 + (boardNums[xcoord, ycoord, zcoord, wcoord] - boardNums[otherxcoord, otherycoord, otherzcoord, otherwcoord])) % 3;
                            boardColors[newxcoord, newycoord, newzcoord, newwcoord] = (boardColors[xcoord, ycoord, zcoord, wcoord] + 3 + (boardColors[xcoord, ycoord, zcoord, wcoord] - boardColors[otherxcoord, otherycoord, otherzcoord, otherwcoord])) % 3;
                            containsShape[newxcoord, newycoord, newzcoord, newwcoord] = true;
                            clueShapes[boardNums[newxcoord, newycoord, newzcoord, newwcoord], boardColors[newxcoord, newycoord, newzcoord, newwcoord], boardFills[newxcoord, newycoord, newzcoord, newwcoord], boardShapes[newxcoord, newycoord, newzcoord, newwcoord]] = true;
                            autosolveCardOrder.Add(new int[4] { newxcoord, newycoord, newzcoord, newwcoord });

                            DebugMsgSilent("Automatically placing " + boardColors[newxcoord, newycoord, newzcoord, newwcoord] + " " + fillNames[boardFills[newxcoord, newycoord, newzcoord, newwcoord]] + " " + colorNames[boardColors[newxcoord, newycoord, newzcoord, newwcoord]] + " " + shapeNames[boardShapes[newxcoord, newycoord, newzcoord, newwcoord]] + " at " + newxcoord + ", " + newycoord + ", " + newzcoord + ", " + newwcoord);
                            cardObjects[(newxcoord * 27) + (newycoord * 9) + (newzcoord * 3) + newwcoord].transform.localRotation = Quaternion.Euler(180.0f, 0.0f, 180.0f);
                            shapesPlaced++;
                            DebugMsgSilent("Number of cards placed: " + shapesPlaced);
                            generateBoard(otherxcoord, otherycoord, otherzcoord, otherwcoord);
                        }
                    }
                }
            }
        }
    }

	void Awake () {
        ModuleId = ModuleIdCounter++;

        foreach (KMSelectable button in numberButtons)
        {
            button.OnInteract += delegate () { findNumber(button); return false; };
        }
        foreach (KMSelectable button in colorButtons)
        {
            button.OnInteract += delegate () { findColor(button); return false; };
        }
        foreach (KMSelectable button in fillButtons)
        {
            button.OnInteract += delegate () { findFill(button); return false; };
        }
        foreach (KMSelectable button in shapeButtons)
        {
            button.OnInteract += delegate () { findShape(button); return false; };
        }
        foreach (KMSelectable button in cardButtons)
        {
            button.OnInteract += delegate () { cardPressed(button); return false; };
        }
    }

    void findNumber(KMSelectable pressedButton)
    {
        audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (selectedTraits[0] != 3)
        {
            buttonRenderers[selectedTraits[0]].material = unselectedMaterial;
        }
        for(int i = 0; i < 3; i++)
        {
            if(numberButtons[i] == pressedButton)
            {
                selectedTraits[0] = i;
                buttonRenderers[selectedTraits[0]].material = selectedMaterial;
                break;
            }
        }
    }
    void findColor(KMSelectable pressedButton)
    {
        audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (selectedTraits[1] != 3)
        {
            buttonRenderers[selectedTraits[1] + 3].material = unselectedMaterial;
        }
        for (int i = 0; i < 3; i++)
        {
            if (colorButtons[i] == pressedButton)
            {
                selectedTraits[1] = i;
                buttonRenderers[selectedTraits[1] + 3].material = selectedMaterial;
                break;
            }
        }
    }
    void findFill(KMSelectable pressedButton)
    {
        audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (selectedTraits[2] != 3)
        {
            buttonRenderers[selectedTraits[2] + 6].material = unselectedMaterial;
        }
        for (int i = 0; i < 3; i++)
        {
            if (fillButtons[i] == pressedButton)
            {
                selectedTraits[2] = i;
                buttonRenderers[selectedTraits[2] + 6].material = selectedMaterial;
                break;
            }
        }
    }
    void findShape(KMSelectable pressedButton)
    {
        audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (selectedTraits[3] != 3)
        {
            buttonRenderers[selectedTraits[3] + 9].material = unselectedMaterial;
        }
        for (int i = 0; i < 3; i++)
        {
            if (shapeButtons[i] == pressedButton)
            {
                selectedTraits[3] = i;
                buttonRenderers[selectedTraits[3] + 9].material = selectedMaterial;
                break;
            }
        }
    }
    void cardPressed(KMSelectable pressedButton)
    {
        int cardNum = 0;

        for(int i = 0; i < cardButtons.Length; i++)
        {
            if(cardButtons[i] == pressedButton)
            {
                cardNum = i;
                break;
            }
        }

        int cardw = cardNum % 3;
        int cardz = ((cardNum - cardw) % 9) / 3;
        int cardy = ((cardNum - (cardz * 3)) % 27) / 9;
        int cardx = (cardNum - (cardy * 9)) / 27;

        if (moduleSolved || cardVisible[cardx, cardy, cardz, cardw] || selectedTraits[0] == 3 || selectedTraits[1] == 3 || selectedTraits[2] == 3 || selectedTraits[3] == 3)
        {
            return;
        }

        GameObject cooler = cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.Find("card").Find("Plane").gameObject;
        
        if (selectedTraits[0] == 0)
        {
            DebugMsg("Submitting 1 " + fillNames[selectedTraits[2]] + " " + colorNames[selectedTraits[1]] + " " + shapeNames[selectedTraits[3]] + " at " + verticalNames[cardx] + " " + horizontalNames[cardy] + ", " + verticalNames[cardz] + " " + horizontalNames[cardw] + ".");
        }
        else
        {
            DebugMsg("Submitting " + (selectedTraits[0] + 1) + " " + fillNames[selectedTraits[2]] + " " + colorNames[selectedTraits[1]] + " " + shapeNames[selectedTraits[3]] + "s at " + verticalNames[cardx] + " " + horizontalNames[cardy] + ", " + verticalNames[cardz] + " " + horizontalNames[cardw] + ".");
        }

        if (boardNums[cardx, cardy, cardz, cardw] == 0)
        {
            DebugMsg("Expecting 1 " + fillNames[boardFills[cardx, cardy, cardz, cardw]] + " " + colorNames[boardColors[cardx, cardy, cardz, cardw]] + " " + shapeNames[boardShapes[cardx, cardy, cardz, cardw]] + ".");
        }
        else
        {
            DebugMsg("Expecting " + (boardNums[cardx, cardy, cardz, cardw] + 1) + " " + fillNames[boardFills[cardx, cardy, cardz, cardw]] + " " + colorNames[boardColors[cardx, cardy, cardz, cardw]] + " " + shapeNames[boardShapes[cardx, cardy, cardz, cardw]] + "s.");
        }

        cardVisible[cardx, cardy, cardz, cardw] = true;

        if (selectedTraits[0] != boardNums[cardx, cardy, cardz, cardw] || selectedTraits[1] != boardColors[cardx, cardy, cardz, cardw] || selectedTraits[2] != boardFills[cardx, cardy, cardz, cardw] || selectedTraits[3] != boardShapes[cardx, cardy, cardz, cardw])
        {
            cardSymbols[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].material.mainTexture = wrongTexture;
            cooler.transform.localScale = new Vector3(0.004192313f, 1f, 0.0025f);
            DebugMsg("Strike!");
            incorrect = true;
        }
        else
        {
            cardSymbols[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].material.mainTexture = cardTextures[(selectedTraits[3] * 27) + (selectedTraits[2] * 9) + (selectedTraits[1] * 3) + selectedTraits[0]];
            shapesVisible++;
            if(shapesVisible >= 54)
            {
                StartCoroutine(WaitingForSolve());
                moduleSolved = true;
                DebugMsg("Module solved.");
            }
        }

        StartCoroutine(FlipCard(cardx, cardy, cardz, cardw, incorrect, cooler));
        incorrect = false;
    }

    private IEnumerator FlipCard(int cardx, int cardy, int cardz, int cardw, bool incorrect, GameObject plane)
    {
        audio.PlaySoundAtTransform("flip", transform);

        cardInAnimation = true;
        float swag = 0f;
        Vector3 start = new Vector3(180, 0, 180);
        Vector3 middle = new Vector3(180, 0, 0);
        Vector3 end = new Vector3(180, 0, -180);
        float xcoord = cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition.x;
        float ycoord = cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition.y;
        float zcoord = cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition.z;

        while (swag < 1.005f)
        {
            cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localRotation = Quaternion.Euler(Vector3.Lerp(start, middle, swag));
            cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition = new Vector3(xcoord, ycoord - ((3 * ((swag - 1) * (swag - 1)) - 3) / 100), zcoord);

            yield return new WaitForSeconds(0.01f);
            swag += .015f;
        }

        if (incorrect)
        {
            GetComponent<KMBombModule>().HandleStrike();
            swag = 0f;

            while (swag < 1.005f)
            {
                cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localRotation = Quaternion.Euler(Vector3.Lerp(middle, end, swag));
                cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition = new Vector3(xcoord, ycoord - ((3 * ((swag) * (swag)) - 3) / 100), zcoord);
                if (cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition.y < ycoord)
                {
                    cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition = new Vector3(xcoord, ycoord, zcoord);
                }

                yield return new WaitForSeconds(0.01f);
                swag += .015f;
            }
            cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition = new Vector3(xcoord, ycoord, zcoord);
            plane.transform.localScale = new Vector3(0.00349359f, 1f, 0.002083334f);

            cardVisible[cardx, cardy, cardz, cardw] = false;
        }
        else
        {
            swag = 0f;

            while (swag < 1.005f)
            {
                cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition = new Vector3(xcoord, ycoord - ((3 * ((swag) * (swag)) - 3) / 100), zcoord);
                if (cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition.y < ycoord)
                {
                    cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition = new Vector3(xcoord, ycoord, zcoord);
                }

                yield return new WaitForSeconds(0.01f);
                swag += .015f;
            }

            if (moduleSolved)
            {
                GetComponent<KMBombModule>().HandlePass();
            }
            cardInAnimation = false;
        }
    }
     
    private IEnumerator WaitingForSolve()
    {
        while(!moduleSolved)
        {
            yield return new WaitForSeconds(1f);
        }

        cardInAnimation = false;
        yield return new WaitForSeconds(3f);
        for (int i = 0; i < cardBacks.Length; i++)
        {
            int swagw = i % 3;
            int swagz = ((i - swagw) % 9) / 3;
            int swagy = ((i - (swagz * 3)) % 27) / 9;
            int swagx = (i - (swagy * 9)) / 27;

            if(cardVisible[swagx, swagy, swagz, swagw])
            {
                cardBacks[i].material.mainTexture = solveTextures[i];
            }
            else
            {
                cardObjects[(swagx * 27) + (swagy * 9) + (swagz * 3) + swagw].transform.Find("card").Find("Plane").gameObject.transform.localScale = new Vector3(0.004192313f, 1f, 0.0025f);
                cardObjects[(swagx * 27) + (swagy * 9) + (swagz * 3) + swagw].transform.Find("card").Find("Plane").gameObject.transform.localRotation = Quaternion.Euler(90, 180, 0);
                cardSymbols[(swagx * 27) + (swagy * 9) + (swagz * 3) + swagw].material.mainTexture = solveTextures[i];
            }

            solveUnflippedCards.Add(new int[4] { swagx, swagy, swagz, swagw });
        }

        int cool;
        while (solveUnflippedCards.Count > 0)
        {
            cool = Rnd.Range(0, solveUnflippedCards.Count);
            StartCoroutine(SolveFlipCard(solveUnflippedCards[cool][0], solveUnflippedCards[cool][1], solveUnflippedCards[cool][2], solveUnflippedCards[cool][3]));
            solveUnflippedCards.RemoveAt(cool);
            yield return new WaitForSeconds(.1f);
        }
    }

    private IEnumerator SolveFlipCard(int cardx, int cardy, int cardz, int cardw)
    {
        float swag = 0f;
        Vector3 start = new Vector3(180, 0, 180);
        Vector3 end = new Vector3(180, 0, 0);
        float xcoord = cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition.x;
        float ycoord = cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition.y;
        float zcoord = cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition.z;

        if(!cardVisible[cardx, cardy, cardz, cardw])
        {
            start = new Vector3(180, 0, 0);
            end = new Vector3(180, 0, -180);
        }

        while (swag < 1.005f)
        {
            cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localRotation = Quaternion.Euler(Vector3.Lerp(end, start, swag));
            cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition = new Vector3(xcoord, ycoord - ((3 * ((swag - 1) * (swag - 1)) - 3) / 100), zcoord);

            yield return new WaitForSeconds(0.01f);
            swag += .015f;
        }
        swag = 0;
        while (swag < 1.005f)
        {
            cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition = new Vector3(xcoord, ycoord - ((3 * ((swag) * (swag)) - 3) / 100), zcoord);
            if (cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition.y < ycoord)
            {
                cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition = new Vector3(xcoord, ycoord, zcoord);
            }

            yield return new WaitForSeconds(0.01f);
            swag += .015f;
        }
        cardObjects[(cardx * 27) + (cardy * 9) + (cardz * 3) + cardw].transform.localPosition = new Vector3(xcoord, ycoord, zcoord);
    }

    private bool isCommandValid(string cmd)
    {
        string[] validbtns = { "one","1","two","2","three","3","red","yellow","blue","empty","striped","filled","solid","capsule","diamond","squiggle", "capsules", "diamonds", "squiggles" };

        if (!validbtns.Contains(cmd.ToLower()))
        {
            return false;
        }
        return true;
    }

    public string TwitchHelpMessage = "Use !{0} press 1 13 to press the first button 13 times.";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var parts = cmd.ToLowerInvariant().Split(new[] { ' ' });

        for(int i = 0; i < parts.Length; i++)
        {
            if (isCommandValid(parts[i]))
            {
                yield return null;
                switch(parts[i])
                {
                    case "1":
                    case "one":
                        numberButtons[0].OnInteract();
                        break;
                    case "2":
                    case "two":
                        numberButtons[1].OnInteract();
                        break;
                    case "3":
                    case "three":
                        numberButtons[2].OnInteract();
                        break;
                    case "red":
                        colorButtons[0].OnInteract();
                        break;
                    case "yellow":
                        colorButtons[1].OnInteract();
                        break;
                    case "blue":
                        colorButtons[2].OnInteract();
                        break;
                    case "empty":
                        fillButtons[0].OnInteract();
                        break;
                    case "striped":
                        fillButtons[1].OnInteract();
                        break;
                    case "solid":
                    case "filled":
                        fillButtons[2].OnInteract();
                        break;
                    case "capsule":
                    case "capsules":
                        shapeButtons[0].OnInteract();
                        break;
                    case "diamond":
                    case "diamonds":
                        shapeButtons[1].OnInteract();
                        break;
                    case "squiggle":
                    case "squiggles":
                        shapeButtons[2].OnInteract();
                        break;
                    default:
                        yield break;
                }
            }
            else if(parts[i].Length == 2)
            {
                int inputx;
                int inputy;
                int inputz;
                int inputw;

                switch(parts[i][0])
                {
                    case 'a':
                        inputy = 0;
                        inputw = 0;
                        break;
                    case 'b':
                        inputy = 0;
                        inputw = 1;
                        break;
                    case 'c':
                        inputy = 0;
                        inputw = 2;
                        break;
                    case 'd':
                        inputy = 1;
                        inputw = 0;
                        break;
                    case 'e':
                        inputy = 1;
                        inputw = 1;
                        break;
                    case 'f':
                        inputy = 1;
                        inputw = 2;
                        break;
                    case 'g':
                        inputy = 2;
                        inputw = 0;
                        break;
                    case 'h':
                        inputy = 2;
                        inputw = 1;
                        break;
                    case 'i':
                        inputy = 2;
                        inputw = 2;
                        break;
                    default:
                        tpNoCard = true;
                        yield break;
                }

                switch (parts[i][1])
                {
                    case '1':
                        inputx = 0;
                        inputz = 0;
                        break;
                    case '2':
                        inputx = 0;
                        inputz = 1;
                        break;
                    case '3':
                        inputx = 0;
                        inputz = 2;
                        break;
                    case '4':
                        inputx = 1;
                        inputz = 0;
                        break;
                    case '5':
                        inputx = 1;
                        inputz = 1;
                        break;
                    case '6':
                        inputx = 1;
                        inputz = 2;
                        break;
                    case '7':
                        inputx = 2;
                        inputz = 0;
                        break;
                    case '8':
                        inputx = 2;
                        inputz = 1;
                        break;
                    case '9':
                        inputx = 2;
                        inputz = 2;
                        break;
                    default:
                        tpNoCard = true;
                        yield break;
                }

                if(tpNoCard)
                {
                    tpNoCard = false;
                    yield break;
                }

                cardButtons[(inputx * 27) + (inputy * 9) + (inputz * 3) + inputw].OnInteract();
            }
            else
            {
                yield break;
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (cardInAnimation)
        {
            yield return new WaitForSeconds(.1f);
        }

        for (int i = 0; i < autosolveCardOrder.Count; i++)
        {
            if (moduleSolved)
            {
                break;
            }

            if (!cardVisible[autosolveCardOrder[i][0], autosolveCardOrder[i][1], autosolveCardOrder[i][2], autosolveCardOrder[i][3]])
            {
                numberButtons[boardNums[autosolveCardOrder[i][0], autosolveCardOrder[i][1], autosolveCardOrder[i][2], autosolveCardOrder[i][3]]].OnInteract();
                colorButtons[boardColors[autosolveCardOrder[i][0], autosolveCardOrder[i][1], autosolveCardOrder[i][2], autosolveCardOrder[i][3]]].OnInteract();
                fillButtons[boardFills[autosolveCardOrder[i][0], autosolveCardOrder[i][1], autosolveCardOrder[i][2], autosolveCardOrder[i][3]]].OnInteract();
                shapeButtons[boardShapes[autosolveCardOrder[i][0], autosolveCardOrder[i][1], autosolveCardOrder[i][2], autosolveCardOrder[i][3]]].OnInteract();

                cardButtons[(autosolveCardOrder[i][0] * 27) + (autosolveCardOrder[i][1] * 9) + (autosolveCardOrder[i][2] * 3) + autosolveCardOrder[i][3]].OnInteract();
                yield return new WaitForSeconds(.05f);
            }
        }
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Set Connections #{0}] {1}", ModuleId, msg);
    }
    void DebugMsgSilent(string msg)
    {
        Debug.LogFormat("<Set Connections #{0}> {1}", ModuleId, msg);
    }
}
