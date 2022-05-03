using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System.IO;

public class TileMap : MonoBehaviour
{

    public enum State { zero, unitMoving };

    public State state = State.zero;

    int roundCounter = 0;
    public TextMeshProUGUI roundDisplay;
    public TextMeshProUGUI turnDisplay;
    public TextMeshProUGUI endTurnDisplay;

    public List<Unit> activeUnits;
    Unit selectedUnit;

    List<Unit> turnQueue;

    int[,] units;
    Node[,] graph;

    ClickableTile[,] clickableTiles;

    public GameObject arrowHolder;

    int mapSizeX;
    int mapSizeY;

    private void Start()
    {
        GenerateMapTiles("map1.txt");
        GeneratePathfindingGraph();
        RollInitiative();
        NextRound();
        NextTurn();
    }

    void GenerateMapTiles(string filename)
    {
        string[] mapText = File.ReadAllLines("Assets/Maps/" + filename);

        //read and interpret the 3 lines of info before the map begins
        //code code code

        string[] map = mapText[3..mapText.Length];

        mapSizeX = map[0].Length;
        mapSizeY = map.Length;

        //generates arrow segments for later
        arrowHolder.GetComponent<Arrow>().GenerateArrowSegments(mapSizeX, mapSizeY);

        //Allocate our map tiles and unit spots
        units = new int[mapSizeX, mapSizeY];
        clickableTiles = new ClickableTile[mapSizeX, mapSizeY];

        int x, y;

        for (x = 0; x < mapSizeX; x++)
        {
            for (y = 0; y < mapSizeY; y++)
            {
                //spawn visual prefabs for tiles

                //grab tile letter from map txt file
                char letter = map[mapSizeY - y - 1][x];
                GameObject tileGO;
                ClickableTile ct;

                if (letter == 'X' || letter == 'V') //if it's a unit...
                {
                    //...instntiate a plain tile under it first
                    tileGO = Instantiate(LoadPrefabFromFile('-'), new Vector3(x, y, 0), Quaternion.identity);
                    ct = tileGO.GetComponent<ClickableTile>();
                    ct.tileX = x;
                    ct.tileY = y;
                    ct.map = this;
                    clickableTiles[x, y] = ct;


                    GameObject unitGO = Instantiate(LoadPrefabFromFile(letter), new Vector3(x, y, 0), Quaternion.identity);
                    Unit un = unitGO.GetComponent<Unit>();
                    un.tileX = x;
                    un.tileY = y;
                    un.map = this;
                    activeUnits.Add(un);
                    clickableTiles[x, y].occupyingUnit = un;

                }
                else
                {
                    tileGO = Instantiate(LoadPrefabFromFile(letter), new Vector3(x, y, 0), Quaternion.identity);

                    ct = tileGO.GetComponent<ClickableTile>();
                    ct.tileX = x;
                    ct.tileY = y;
                    ct.map = this;
                    clickableTiles[x, y] = ct;
                }
            }
        }
    }

    public void GenerateMovementSet(Unit selectedUnit)
    {
        //Dijkstra's Algorithm for shortest path
        //dist relates each node to it's distance to source
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        //prev holds the steps for the shortest path to source
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

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
                if (UnitCanEnterTile(v))
                {
                    float cost = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
                    if (cost < dist[v])
                    {
                        dist[v] = cost;
                        prev[v] = u;
                    }
                }
            }
        }

        foreach (Node n in dist.Keys)
        {
            if (dist[n] <= selectedUnit.remainingMovement + .5 && dist[n] != 0)
            {
                clickableTiles[n.x, n.y].AddToMovementSet();
                clickableTiles[n.x, n.y].costToFinishHere = (int)dist[n];
            }
            else
            {
                clickableTiles[n.x, n.y].RemoveFromAllSets();
                clickableTiles[n.x, n.y].costToFinishHere = 0;
            }
        }

    }

    void RollInitiative()
    {
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
        endTurnDisplay.text = "End " + selectedUnit.unitName + "'s Turn";

        GenerateMovementSet(selectedUnit);
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

    GameObject LoadPrefabFromFile(char firstLetter)
    {
        foreach (string filename in Directory.GetFiles("Assets/Resources"))
        {
            string fn = filename[17..(filename.Length-7)];
            if (fn[0] == firstLetter)
            {
                GameObject loadedObject = Resources.Load(fn) as GameObject;
                if (loadedObject == null)
                {
                    throw new FileNotFoundException("...no file found at " + fn);
                }
                return loadedObject;
            }
            
        }
                
        return null;
    }   

    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        //if the map ever slides or scales, this will need to be more complicated
        return new Vector3(x, y, 0);
    }

    bool UnitCanEnterTile(Node n)
    {
        return selectedUnit.movementType == Unit.MovementType.walking && clickableTiles[n.x, n.y].isWalkable
                    || selectedUnit.movementType == Unit.MovementType.flying;
    }

    public void GeneratePathTo(int x, int y)
    {
        //Clear out any old paths the unit may have
        ClearCurrentPath();

        //Dijkstra's Algorithm for shortest path
        //dist relates each node to it's distance to source
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        //prev holds the steps for the shortest path to source
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        //"Q" in the Wikipedia pseudocode
        List<Node> unvisited = new List<Node>();

        Node source = graph[
            selectedUnit.tileX,
            selectedUnit.tileY];
        Node target = graph[x, y];

        dist[source] = 0;
        prev[source] = null;

        //Initialize every node to be at infinite distance, snice we haven't started yet.
        //Also, it's possible some nodes will be unreachable from this source
        foreach (Node n in graph)
        {
            unvisited.Add(n);

            if (n != source){
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
        }

        while (unvisited.Count > 0)
        {
            //u is the unvisited node with the shortest distance to source
            Node u = null;
            foreach(Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            //if we found the target node, exit while loop
            if (u == target)
            {
                break;
            }

            //u is being visited right now
            unvisited.Remove(u);

            foreach (Node v in u.edges) // for all of u's neighbours...
            {
                if (UnitCanEnterTile(v))
                {
                    float cost = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
                    if (cost < dist[v])
                    {
                        dist[v] = cost;
                        prev[v] = u;
                    }
                }

            }
        }

        //Debug.Log("cost: " + dist[target] + "\nremaining movement: " + selectedUnit.remainingMovement);

        //if we get here, then either we found the shortest path to target, or there is no route to target

        //if there isn't a previous node for our target...
        if (prev[target] == null)
        {
            //then no path exists
            return;
        }

        List<Node> currentPath = new List<Node>();
        Node curr = target;

        //Step through prev and add each node in it to our path
        //prev was ordered target to source, so we Prepend to flip it
        while (curr != null)
        {
            currentPath.Insert(0, curr);
            curr = prev[curr];
        }

        //This means currentPath now contains a route from source to target
        selectedUnit.SetCurrentPath(currentPath);
        arrowHolder.GetComponent<Arrow>().DrawArrow(currentPath);
    }

    public void ClearCurrentPath()
    {
        selectedUnit.SetCurrentPath(null);
        arrowHolder.GetComponent<Arrow>().DrawArrow(null);
    }

    public void MoveUnit(int x, int y, int cost)
    {
        if (selectedUnit.GetCurrentPath() != null)
        {
            state = State.unitMoving;
            clickableTiles[selectedUnit.tileX, selectedUnit.tileY].occupyingUnit = null;
            clickableTiles[x, y].occupyingUnit = selectedUnit;
            selectedUnit.MoveNextTile(cost);
        }
        
    }

}
