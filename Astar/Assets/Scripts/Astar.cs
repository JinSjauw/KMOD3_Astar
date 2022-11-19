using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class Astar
{
    /// <summary>
    /// TODO: Implement this function so that it returns a list of Vector2Int positions which describes a path
    /// Note that you will probably need to add some helper functions
    /// from the startPos to the endPos
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    private const int STRAIGHT_COST = 10;
    private const int DIAGONAL_COST = 14;
    
    private Node startNode;
    private Node endNode;
    private List<Node> openList;
    private List<Node> closedList;
    private List<Node> nodeList;

    public List<Vector2Int> FindPathToTarget(Vector2Int startPos, Vector2Int endPos, Cell[,] grid)
    {
        nodeList = new List<Node>();
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                Node node = new Node(position, null, int.MaxValue, 0);
                node.walls = grid[x, y].walls;
                nodeList.Add(node);
                
                if (position.Equals(startPos))
                {
                    startNode = node;
                }
                if (position.Equals(endPos))
                {
                    endNode = node;
                }
            }
        }

        foreach (Node node in nodeList)
        {
            if (node.position == startPos)
            {
                startNode = node;
            }
            else if (node.position == endPos)
            {
                endNode = node;
            }
        }
        
        openList = new List<Node>{ startNode };
        closedList = new List<Node>();

        startNode.GScore = 0;
        startNode.HScore = CalculateDistance(startPos, endPos);

        //Cycle
        while (openList.Count > 0)
        {
            Node currentNode = getLowestFscore(openList);
            if (currentNode == endNode)
            {
                return ReturnPath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
            foreach (Cell neighbour in grid[currentNode.position.x, currentNode.position.y].GetNeighbours(grid))
            {
                if(closedList.Find(n => n.position == neighbour.gridPosition) != null) continue;
     
                if (neighbour.gridPosition.x < currentNode.position.x)
                {
                    if (neighbour.HasWall(Wall.RIGHT) || grid[currentNode.position.x, currentNode.position.y].HasWall(Wall.LEFT))
                    {
                        continue;
                    }
                }
                else if (neighbour.gridPosition.x > currentNode.position.x)
                {
                    if(neighbour.HasWall(Wall.LEFT) || grid[currentNode.position.x, currentNode.position.y].HasWall(Wall.RIGHT)) continue;
                }

                if (neighbour.gridPosition.y < currentNode.position.y)
                {
                    if (neighbour.HasWall(Wall.UP) || grid[currentNode.position.x, currentNode.position.y].HasWall(Wall.DOWN)) continue;
                }
                else if (neighbour.gridPosition.y > currentNode.position.y)
                {
                    if(neighbour.HasWall(Wall.DOWN) || grid[currentNode.position.x, currentNode.position.y].HasWall(Wall.UP)) continue;
                }

                Node neighbourNode = nodeList.Find(n => n.position == neighbour.gridPosition);
                int tentativeGScore = currentNode.GScore + CalculateDistance(currentNode.position, neighbourNode.position);
                if (tentativeGScore < neighbourNode.GScore)
                {
                    neighbourNode.parent = currentNode;
                    neighbourNode.GScore = tentativeGScore;
                    neighbourNode.HScore = CalculateDistance(neighbourNode.position, endNode.position);

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        return null;
    }

    private List<Vector2Int> ReturnPath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;
        path.Add(endNode.position);
        while (currentNode.parent != null)
        {
            path.Add(currentNode.parent.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
    
    private Node getLowestFscore(List<Node> nodeList)
    {
        if (nodeList.Count < 0)
        {
            Debug.Log("List is empty || in getLowestFscore()");
            return null;
        }
        
        Node lowestFcostNode = nodeList.First();
        foreach (Node node in nodeList)
        {
            if (node.FScore < lowestFcostNode.FScore)
            {
                lowestFcostNode = node;
            }
        }
        return lowestFcostNode;
    }

    private int CalculateDistance(Vector2Int a, Vector2Int b)
    {
        int distanceX = Math.Abs(a.x - b.x);
        int distanceY = Math.Abs(a.y - b.y);
        int remaining = Math.Abs(distanceX - distanceY);
        
        return DIAGONAL_COST * Mathf.Min(distanceX, distanceY) + STRAIGHT_COST * remaining;
    }

    /// <summary>
    /// This is the Node class you can use this class to store calculated FScores for the cells of the grid, you can leave this as it is
    /// </summary>
    public class Node
    {
        public Vector2Int position; //Position on the grid
        public Node parent; //Parent Node of this node << this is the node that was looking before.
        public Wall walls;
        public float FScore { //GScore + HScore
            get { return GScore + HScore; }
        }
        public int GScore; //Current Travelled Distance << distance traveled from the startNode. +1 for every cell traveled
        public int HScore; //Distance estimated based on Heuristic << Euclidean distance to endNode
        
        public Node() { }
        public Node(Vector2Int position, Node parent, int GScore, int HScore)
        {
            this.position = position;
            this.parent = parent;
            this.GScore = GScore;
            this.HScore = HScore;
        }
    }
}
