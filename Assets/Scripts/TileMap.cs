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

    public UnitType[] unitTypes;
    public TileType[] tileTypes;

    int[,] tiles;
    int[,] units;
    Node[,] graph;

    ClickableTile[,] clickableTiles;

    int mapSizeX;
    int mapSizeY;

    private void Start()
    {
        GenerateMapData("map1.txt");
        GeneratePathfindingGraph();
        GenerateMapVisual();
        NextRound();
        NextTurn(true);
    }

    void NextRound()
    {
        roundCounter++;
        roundDisplay.text = "Round: " + roundCounter;
        turnQueue = new List<Unit>(activeUnits);
    }

    public void NextTurn(bool first = false)
    {
        if (!first)
        {
            //remove the first unit from the queue
            turnQueue.RemoveAt(0);
        }

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

        //Set up selected unit's initial x and y, as well as tell it what map it's on
        selectedUnit.tileX = (int)selectedUnit.gameObject.transform.position.x;
        selectedUnit.tileY = (int)selectedUnit.gameObject.transform.position.y;
        selectedUnit.map = this;
        GenerateMovementSet(selectedUnit);
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
                    float cost = dist[u] + CostToEnterTile(v.x, v.y, u.x, u.y);
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
            } else {
                clickableTiles[n.x, n.y].RemoveFromAllSets();
                clickableTiles[n.x, n.y].costToFinishHere = 0;
            }
        }

    }

    void GenerateMapData(string filename)
    {
        string[] mapText = File.ReadAllLines("Assets/Maps/" + filename);

        //read and interpret the 3 lines of info before the map begins
        //code code code

        string[] map = mapText[3..mapText.Length];

        mapSizeX = map[0].Length;
        mapSizeY = map.Length;

        //Allocate our map tiles and unit spots
        tiles = new int[mapSizeX, mapSizeY];
        units = new int[mapSizeX, mapSizeY];
        clickableTiles = new ClickableTile[mapSizeX, mapSizeY];

        int x, y;

        //Load map info into tiles[]
        for (x = 0; x < mapSizeX; x++)
        {
            for (y = 0; y < mapSizeY; y++)
            {
                if (map[mapSizeY - y - 1][x] == 'M') //mountain (impassable on foot)
                {
                    tiles[x, y] = 2;
                }
                else if (map[mapSizeY - y - 1][x] == 'W') //water
                {
                    tiles[x, y] = 3;
                }
                else if (map[mapSizeY - y - 1][x] == 'X') //unit
                {
                    //start unit here
                    units[x, y] = 1;
                }
                else if (map[mapSizeY - y - 1][x] == 'V') //unit
                {
                    //start unit here
                    units[x, y] = 2;
                }
                else //plain terrain for theme
                {
                    tiles[x, y] = 0;
                }
            }
        }
    }

    float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY)
    {
        float cost;

        if (selectedUnit.movementType == Unit.MovementType.flying)
        {
            cost = 5;
        } else
        {
            TileType tt = tileTypes[tiles[sourceX, sourceY]];
            cost = tt.movementCost;
        }

        if (sourceX != targetX && sourceY != targetY)
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

    void GenerateMapVisual()
    {
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                //spawn visual prefabs for tiles
                TileType tt = tileTypes[tiles[x, y]];
                GameObject tileGO = Instantiate(tt.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity);

                ClickableTile ct = tileGO.GetComponent<ClickableTile>();
                ct.tileX = x;
                ct.tileY = y;
                ct.map = this;
                clickableTiles[x, y] = ct;

                //spawn visual prefabs for units
                if (units[x, y] != 0)
                {
                    UnitType ut = unitTypes[units[x, y]];
                    GameObject unitGO = Instantiate(ut.unitVisualPrefab, new Vector3(x, y, 0), Quaternion.identity);

                    Unit un = unitGO.GetComponent<Unit>();
                    un.tileX = x;
                    un.tileY = y;
                    un.map = this;
                    activeUnits.Add(un);
                    clickableTiles[x, y].occupyingUnit = un;
                }
            }
        }
    }

    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        //if the map ever slides or scales, this will need to be more complicated
        return new Vector3(x, y, 0);
    }

    bool UnitCanEnterTile(Node n)
    {
        return selectedUnit.movementType == Unit.MovementType.walking && tileTypes[tiles[n.x, n.y]].isWalkable
                    || selectedUnit.movementType == Unit.MovementType.flying;
    }

    public void GeneratePathTo(int x, int y)
    {
        //Clear out any old paths the unit may have
        selectedUnit.currentPath = null;

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
                    float cost = dist[u] + CostToEnterTile(v.x, v.y, u.x, u.y);
                    if (cost < dist[v])
                    {
                        dist[v] = cost;
                        prev[v] = u;
                    }
                }

            }
        }

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
        selectedUnit.currentPath = currentPath;
    }

    public void ClearCurrentPath()
    {
        selectedUnit.currentPath = null;
    }

    public void MoveUnit(int x, int y, int cost)
    {
        if (selectedUnit.currentPath != null)
        {
            state = State.unitMoving;
            clickableTiles[selectedUnit.tileX, selectedUnit.tileY].occupyingUnit = null;
            clickableTiles[x, y].occupyingUnit = selectedUnit;
            selectedUnit.MoveNextTile(cost);
        }
        
    }

}
