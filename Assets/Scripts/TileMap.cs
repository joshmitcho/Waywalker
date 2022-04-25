using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class TileMap : MonoBehaviour
{

    public enum State { zero, unitMoving };

    public State state = State.zero;

    int roundCounter = 0;
    public TextMeshProUGUI roundDisplay;
    public TextMeshProUGUI turnDisplay;
    public TextMeshProUGUI endTurnDisplay;

    public List<Unit> units;
    Unit selectedUnit;

    List<Unit> turnQueue;

    public TileType[] tileTypes;

    int[,] tiles;
    Node[,] graph;

    int mapSizeX = 20;
    int mapSizeY = 11;

    private void Start()
    {
        NextRound();
        NextTurn(true);
        GenerateMapData();
        GeneratePathfindingGraph();
        GenerateMapVisual();
    }

    void NextRound()
    {
        roundCounter++;
        roundDisplay.text = "Round: " + roundCounter;
        turnQueue = new List<Unit>(units);
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
        }

        selectedUnit = turnQueue[0];
        turnDisplay.text = selectedUnit.unitName + "'s Turn!";
        endTurnDisplay.text = "End " + selectedUnit.unitName + "'s Turn";

        //Set up selected unit's initial x and y, as well as tell it what map it's on
        selectedUnit.tileX = (int)selectedUnit.gameObject.transform.position.x;
        selectedUnit.tileY = (int)selectedUnit.gameObject.transform.position.y;
        selectedUnit.map = this;
    }

    void GenerateMapData()
    {
        //Allocate our map tiles
        tiles = new int[mapSizeX, mapSizeY];

        int x, y;

        //Initialize map tiles to be grass (0 = grass)
        for (x = 0; x < mapSizeX; x++)
        {
            for (y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 0;
            }
        }

        //swamp area _1 = swamp)
        for (x = 3; x <= 5; x++)
        {
            for (y = 0; y < 4; y++)
            {
                tiles[x, y] = 1;
            }
        }

        //u-shaped mountain range (2 = mountain)
        tiles[4, 4] = 2;
        tiles[5, 4] = 2;
        tiles[6, 4] = 2;
        tiles[7, 4] = 2;
        tiles[8, 4] = 2;

        tiles[4, 5] = 2;
        tiles[4, 6] = 2;
        tiles[8, 5] = 2;
        tiles[8, 6] = 2;
    }

    float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY)
    {
        TileType tt = tileTypes[tiles[sourceX, sourceY]];

        float cost = tt.movementCost;
        
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
                //add diagonal neighbours
                if (x > 0 && y > 0)
                    graph[x, y].edges.Add(graph[x - 1, y - 1]);
                if (x > 0 && y < mapSizeY - 1)
                    graph[x, y].edges.Add(graph[x - 1, y + 1]);
                if (x < mapSizeX - 1 && y > 0)
                    graph[x, y].edges.Add(graph[x + 1, y - 1]);
                if (x < mapSizeX - 1 && y < mapSizeY - 1)
                    graph[x, y].edges.Add(graph[x + 1, y + 1]);
            }
        }
    }

    void GenerateMapVisual()
    {
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                //spawn visual prefabs
                TileType tt = tileTypes[tiles[x, y]];
                GameObject go = Instantiate(tt.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity);

                ClickableTile ct = go.GetComponent<ClickableTile>();
                ct.tileX = x;
                ct.tileY = y;
                ct.map = this;
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
                    float cost = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
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

    public void MoveUnit()
    {
        if (selectedUnit.currentPath != null)
        {
            state = State.unitMoving;
            selectedUnit.MoveNextTile();
        }
        
    }

}
