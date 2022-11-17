using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using System.IO;
using UnityEngine.UI;
using UnityEditor;

public class TileMap : MonoBehaviour
{
    public Camera cam;
    public enum State { zero, choosingMovement, unitMoving, choosingAttack };
    public State state = State.zero;

    public Tooltip tooltip;

    public RectTransform actionMenu;
    Button[] buttons;


    int roundCounter = 0;
    public TextMeshProUGUI roundDisplay;
    public TextMeshProUGUI turnDisplay;

    public List<Unit> activeUnits;
    public Unit selectedUnit;

    List<Unit> turnQueue;

    //int[,] units;
    Node[,] graph;
    Dictionary<Node, float> dist;
    Dictionary<Node, Node> prev;

    ClickableTile[,] clickableTiles;

    public Arrow arrow;
    public DiceHandler diceHandler;
    public GameObject tilePrefab;
    public GameObject unitPrefab;

    int mapSizeX;
    int mapSizeY;

    private void Start()
    {
        
        buttons = actionMenu.gameObject.GetComponentsInChildren<Button>();
        diceHandler.GenerateDieBlanks(18);
        GenerateMapTiles("map1");
        GeneratePathfindingGraph();
        RollInitiative();
        NextRound();
        NextTurn();
    }

    private Dictionary<char, List<Sprite>> LoadTileSprites(Dictionary<char, TileSpec> tileDict)
    {
        Dictionary<char, List<Sprite>> sprites = new Dictionary<char, List<Sprite>>();

        foreach (char key in tileDict.Keys)
        {
            string path = Application.streamingAssetsPath + "/Sprites/" + tileDict[key].spritesheet;
            
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                                
                List<Sprite> lst = new List<Sprite>();

                for (int i = 0; i < tileDict[key].numSprites; i++)
                {
                    Sprite sp = Sprite.Create(texture, new Rect(i*32, 0, 32, 32),
                        new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
                    lst.Add(sp);
                }

                sprites.Add(key, lst);
                
            }
            else
            {
                print("No file at: " + path);
            }
        }

        return sprites;

    }

    private Dictionary<int, List<Sprite>> LoadPartySprites(UnitSpec[] units)
    {
        Dictionary<int, List<Sprite>> sprites = new Dictionary<int, List<Sprite>>();

        for (int i = 0; i < units.Length; i++)
        {
            string path = Application.streamingAssetsPath + "/Sprites/" + units[i].spritesheet;

            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);

                List<Sprite> lst = new List<Sprite>();

                for (int j = 0; j < units[i].numSprites; j++)
                {
                    Sprite sp = Sprite.Create(texture, new Rect(j * 32, 0, 32, 32),
                        new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
                    lst.Add(sp);
                }

                sprites.Add(i, lst);

            }
            else
            {
                print("No file at: " + path);
            }
        }

        return sprites;

    }

    void GenerateMapTiles(string filename)
    {
        string path = Application.streamingAssetsPath + "/Maps/" + filename;

        string tileJSON = File.ReadAllText(path + "_data.json");
        Dictionary<char, TileSpec> tileDict = new Dictionary<char, TileSpec>();
        foreach (TileSpec t in JsonHelper.FromJson<TileSpec>(tileJSON))
        {
            tileDict[t.letter[0]] = t;
        }
        Dictionary<char, List<Sprite>> sprites = LoadTileSprites(tileDict);

        string[] mapText = File.ReadAllLines(path + ".txt");

        //read and interpret the 3 lines of info before the map begins
        //code code code

        string[] map = mapText[3..mapText.Length];

        mapSizeX = map[0].Length;
        mapSizeY = map.Length;

        //center camera over map
        cam.transform.position = new Vector3(mapSizeX/2f - .5f, mapSizeY/2f - .5f, -10f);

        //generates arrow segments for later
        arrow.GenerateArrowSegments(mapSizeX, mapSizeY);

        //Allocate our map tiles and unit spots
        //units = new int[mapSizeX, mapSizeY];
        clickableTiles = new ClickableTile[mapSizeX, mapSizeY];

        //values to store where the party begins (X on the map)
        int partyX = 0, partyY = 0;

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                //spawn visual prefabs for tiles

                //grab tile letter from map txt file
                char letter = map[mapSizeY - y - 1][x];
                GameObject tileGO;
                ClickableTile ct;

                if (letter == 'X') //if it's where the party starts...
                {
                    letter = '-'; //instantiate a plain tile for now

                    //remember where the party started, for later
                    partyX = x;
                    partyY = y;
                }

                tileGO = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.Euler(0, 0, 0));
                    
                ct = tileGO.GetComponent<ClickableTile>();
                ct.tileType = tileDict[letter].tileType;
                ct.isWalkable = tileDict[letter].isWalkable;
                ct.movementCost = tileDict[letter].movementCost;

                if (sprites[letter].Count > 1) // If tile has a sprite animation
                {
                    tileGO.GetComponent<SpriteAnimator>().LoadSprites(sprites[letter]);
                }
                else // if the tile is just a static sprite
                {
                    tileGO.GetComponent<SpriteRenderer>().sprite = sprites[letter][0];
                }

                ct.tileX = x;
                ct.tileY = y;
                ct.map = this;
                clickableTiles[x, y] = ct;
                    
                
            }
        }
        //instatiate party units
        LoadParty(partyX, partyY);
    }

    void LoadParty(int partyX, int partyY)
    {
        Dictionary<int, Tuple<int, int>> pcLocations = new Dictionary<int, Tuple<int, int>>();
        string[] orientationText = File.ReadAllLines(Application.streamingAssetsPath + "/Characters/Party/orientation.txt");

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (orientationText[y][x] != '-')
                {
                    pcLocations[(int)Char.GetNumericValue(orientationText[y][x])] = new Tuple<int, int>(x, y);
                }
            }
        }        

        string partyJSON = File.ReadAllText(Application.streamingAssetsPath + "/Characters/Party/party.json");
        UnitSpec[] units = JsonHelper.FromJson<UnitSpec>(partyJSON);

        Dictionary<int, List<Sprite>> sprites = LoadPartySprites(units);

        for (int i = 0; i < units.Length; i++)
        {
            int x = partyX + pcLocations[i].Item1 - 2;
            int y = partyY - pcLocations[i].Item2 + 2;

            GameObject unitGO = Instantiate(unitPrefab, new Vector3(x, y, 0), Quaternion.Euler(0, 0, 0));
            Unit un = unitGO.GetComponent<Unit>();

            un.name = units[i].name;
            un.unitName = units[i].name;
            un.AC = units[i].AC;
            un.HP = units[i].HP;
            un.STR = units[i].STR;
            un.DEX = units[i].DEX;
            un.CON = units[i].CON;
            un.INT = units[i].INT;
            un.WIS = units[i].WIS;
            un.CHA = units[i].CHA;
            un.movementSpeed = units[i].walkingSpeed;

            un.tileX = x;
            un.tileY = y;
            un.map = this;
            activeUnits.Add(un);
            clickableTiles[x, y].occupyingUnit = un;
            un.occupyingTile = clickableTiles[x, y];

            un.InitiateUnitValues();

            if (sprites[i].Count > 1) // If unit has a sprite animation
            {
                unitGO.GetComponentsInChildren<SpriteAnimator>()[0].LoadSprites(sprites[i]);
            }
            else // if the unit is just a static sprite
            {
                unitGO.GetComponentsInChildren<SpriteRenderer>()[0].sprite = sprites[i][0];
            }
        }
    }

    public void Dijkstra(Unit selectedUnit, bool isMoving, bool isAttack)
    {
        //Dijkstra's Algorithm for shortest path
        //dist relates each node to it's distance to source
        dist = new Dictionary<Node, float>();
        //prev holds the steps for the shortest path to source
        prev = new Dictionary<Node, Node>();

        //"Q" in the Wikipedia pseudocode
        List<Node> unvisited = new List<Node>();

        Node source = graph[
            selectedUnit.tileX,
            selectedUnit.tileY];

        dist[source] = 0;
        prev[source] = null;

        //Initialize every node to be at infinite distance, snice we haven't started yet.
        //Also, it's possible some nodes will be unreachable from this source
        foreach (Node n in graph)
        {
            unvisited.Add(n);

            if (n != source)
            {
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
        }

        while (unvisited.Count > 0)
        {
            //u is the unvisited node with the shortest distance to source
            Node u = null;
            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            //u is being visited right now
            unvisited.Remove(u);

            foreach (Node v in u.edges) // for all of u's neighbours...
            {
                if (UnitCanEnterTile(v, isMoving))
                {
                    float cost;
                    if(isAttack){
                        cost = dist[u] + 5;
                    }
                    else{
                        cost = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
                    }
                    if (cost < dist[v])
                    {
                        dist[v] = cost;
                        prev[v] = u;
                    }
                }
            }
        }

    }

    public void GenerateMovementSet()
    {
        //Dijkstra's Algorithm for shortest path
        //dist relates each node to it's distance to source
        //Dictionary<Node, float> dist = dijkstra.Item1;
        //prev holds the steps for the shortest path to source
        //Dictionary<Node, Node> prev = dijkstra.Item2;
        Dijkstra(selectedUnit, true, false);

        foreach (Node n in dist.Keys)
        {
            clickableTiles[n.x, n.y].costToFinishHere = (int)dist[n];

            //if tile is unoccupied, within reach, and not the one we're standing on already...
            //or if the tile is occupied but by a non-enemy...
            if (clickableTiles[n.x, n.y].occupyingUnit == null
                && dist[n] <= selectedUnit.currentMovement + .5
                && dist[n] != 0
                || (clickableTiles[n.x, n.y].occupyingUnit != null
                    && clickableTiles[n.x, n.y].occupyingUnit.GetType() != typeof(Enemy)))
            {
                clickableTiles[n.x, n.y].AddToMovementSet();
            }
            else
            {
                clickableTiles[n.x, n.y].RemoveFromAllSets();
            }
        }

        state = State.choosingMovement;
        actionMenu.gameObject.SetActive(false);

    }

    public void GenerateAttackSet()
    {
        //Dijkstra's Algorithm for shortest path
        //dist relates each node to it's distance to source
        //Dictionary<Node, float> dist = dijkstra.Item1;
        //prev holds the steps for the shortest path to source
        //Dictionary<Node, Node> prev = dijkstra.Item2;
        Dijkstra(selectedUnit, false, true);

        foreach (Node n in dist.Keys)
        {
            if (dist[n] <= selectedUnit.GetAttackRange() + .5 && dist[n] != 0)
            {
                clickableTiles[n.x, n.y].AddToAttackSet();
            }
            else
            {
                clickableTiles[n.x, n.y].RemoveFromAllSets();
            }
        }

        state = State.choosingAttack;
        actionMenu.gameObject.SetActive(false);

    }

    public void EndSet(){
        foreach (Node n in dist.Keys)
        {
            clickableTiles[n.x, n.y].RemoveFromAllSets();
        }

        actionMenu.gameObject.SetActive(true);
    }

    void RollInitiative()
    {
        Dice init = new Dice(1, 20, 0);
        foreach (Unit un in activeUnits)
        {
            un.initiative = init.Roll().Item2[0] + un.initiativeBonus; //Roll()[0] is always the total roll value
        }

        activeUnits.Sort((p, q) => q.initiative.CompareTo(p.initiative));
    }

    void NextRound()
    {
        roundCounter++;
        roundDisplay.text = "Round: " + roundCounter;
        turnQueue = new List<Unit>(activeUnits);
    }

    public void NextTurn()
    {

        if (turnQueue.Count == 0)
        {
            NextRound();
            if (turnQueue.Count == 0)
            {
                return;
            }
        }

        selectedUnit = turnQueue[0];
        selectedUnit.ResetTurnValues();
        turnDisplay.text = selectedUnit.unitName + "'s Turn!";

        Dijkstra(selectedUnit, true, false);
        OpenActionMenu();

        turnQueue.RemoveAt(0);
    }

    float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY)
    {
        float cost;

        if (selectedUnit.movementType == Unit.MovementType.flying)
        {
            cost = 5;
        } else
        {
            cost = clickableTiles[targetX, targetY].movementCost;
        }

        if (sourceX != targetX && sourceY != targetY) //diagonal movement
        {
            cost += 0.001f;
        }

        return cost;
    }

    void GeneratePathfindingGraph()
    {
        //initialize array
        graph = new Node[mapSizeX, mapSizeY];

        //Initialize a Node for each spot in the array
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                graph[x, y] = new Node(x, y); 
            }
        }

        //loop through again, adding neighbours
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                //add left, right, bottom and top neighbours
                if (x > 0)
                    graph[x, y].edges.Add(graph[x - 1, y]);
                if (x < mapSizeX - 1)
                    graph[x, y].edges.Add(graph[x + 1, y]);
                if (y > 0)
                    graph[x, y].edges.Add(graph[x, y - 1]);
                if (y < mapSizeY - 1)
                    graph[x, y].edges.Add(graph[x, y + 1]);
                /*
                //add diagonal neighbours
                if (x > 0 && y > 0)
                    graph[x, y].edges.Add(graph[x - 1, y - 1]);
                if (x > 0 && y < mapSizeY - 1)
                    graph[x, y].edges.Add(graph[x - 1, y + 1]);
                if (x < mapSizeX - 1 && y > 0)
                    graph[x, y].edges.Add(graph[x + 1, y - 1]);
                if (x < mapSizeX - 1 && y < mapSizeY - 1)
                    graph[x, y].edges.Add(graph[x + 1, y + 1]);
                */
            }
        }
    }

    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        //if the map ever slides or scales, this will need to be more complicated
        return new Vector3(x, y, 0);
    }

    bool UnitCanEnterTile(Node n, bool isMoving)
    {
        if (isMoving
            && clickableTiles[n.x, n.y].occupyingUnit != null
            && clickableTiles[n.x, n.y].occupyingUnit.GetType() == typeof(Enemy))
            return false;
        if (selectedUnit.movementType == Unit.MovementType.walking && clickableTiles[n.x, n.y].isWalkable)
            return true;
        if (selectedUnit.movementType == Unit.MovementType.flying)
            return true;
        
        return false;
            
    }

    public void GeneratePathTo(int targetX, int targetY, bool withinMovementSet)
    {
        //Clear out any old paths the unit may have
        ClearCurrentPath();
                
        List<Node> currentPath = new List<Node>();
        Node curr = graph[targetX, targetY];

        //Step through prev and add each node in it to our path
        //prev was ordered target to source, so we Prepend to flip it
        while (curr != null)
        {
            currentPath.Insert(0, curr);
            curr = prev[curr];
        }

        //This means currentPath now contains a route from source to target
        selectedUnit.currentPath = currentPath;

        if (withinMovementSet)
        {
            arrow.DrawArrow(currentPath);
        }
    }

    public void ClearCurrentPath()
    {
        selectedUnit.currentPath = null;
        arrow.DrawArrow(null);
    }

    public void MoveUnit(int x, int y, int cost)
    {
        if (selectedUnit.currentPath != null)
        {
            state = State.unitMoving;
            clickableTiles[selectedUnit.tileX, selectedUnit.tileY].occupyingUnit = null;
            clickableTiles[x, y].occupyingUnit = selectedUnit;
            selectedUnit.occupyingTile = clickableTiles[x, y];
            selectedUnit.MoveNextTile(cost);
        }

    }

    public void Attack(Unit target)
    {
        int damage = diceHandler.DrawDice(selectedUnit.Attack(), selectedUnit);
        target.TakeDamage(damage);
    }

    public void NewToolTip(ClickableTile tile)
    {
        tooltip.LoadTile(tile, (int)dist[graph[tile.tileX, tile.tileY]], selectedUnit);
    }

    public void OpenActionMenu()
    {
        foreach (Node n in dist.Keys)
        {
            clickableTiles[n.x, n.y].RemoveFromAllSets();
        }

        //can click Move/Attack if you have movement/attack actions remaining
        
        if (selectedUnit.currentMovement > 0)
        {
            buttons[1].interactable = true;
            buttons[1].GetComponentInChildren<TextMeshProUGUI>().alpha = 1f;
        }
        else
        {
            buttons[1].interactable = false;
            buttons[1].GetComponentInChildren<TextMeshProUGUI>().alpha = 0.25f;
        }

        if (selectedUnit.remainingAttackActions > 0)
        {
            buttons[0].interactable = true;
            buttons[0].GetComponentInChildren<TextMeshProUGUI>().alpha = 1f;

        }
        else
        {
            buttons[0].interactable = false;
            buttons[0].GetComponentInChildren<TextMeshProUGUI>().alpha = 0.25f;
        }


        //actionMenu.transform.position = cam.WorldToScreenPoint(selectedUnit.transform.position) + new Vector3(8, 8, 0);

        actionMenu.gameObject.SetActive(true);
    }

}
