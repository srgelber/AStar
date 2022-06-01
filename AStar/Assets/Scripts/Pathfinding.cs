/* 
CSC 378 Lab 6 - A*
By: Simon Gelber
Modified from Code Monkey A* Tutorial
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script implements the actual A* pathing algorithm.
public class Pathfinding {

    //setup costs for straight and diagonal movement
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    //create pathing instance
    public static Pathfinding Instance { get; private set; }

    private Grid<PathNode> grid;
    //nodes to explore
    private List<PathNode> openList;

    //nodes already explored
    private List<PathNode> closedList;

    //pathfinding constructor
    public Pathfinding(int width, int height) {
        Instance = this;
        grid = new Grid<PathNode>(width, height, 10f, Vector3.zero, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));
    }

    //get the navigation grid
    public Grid<PathNode> GetGrid() {
        return grid;
    }

    //FindPath method which is used to ensure the path and target location
    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        List<PathNode> path = FindPath(startX, startY, endX, endY);
        if (path == null) {
            return null;
        } else {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode pathNode in path) {
                vectorPath.Add(new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * .5f);
            }
            return vectorPath;
        }
    }

    //the method that does the heavy lifting and implements A*
    public List<PathNode> FindPath(int startX, int startY, int endX, int endY) {
        //find start and end points
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        if (startNode == null || endNode == null) {
            // Invalid Path
            return null;
        }

        //add the start node to the openList to begin searching
        openList = new List<PathNode> { startNode };

        //create the empty closedList
        closedList = new List<PathNode>();

        //populate the grid with uninitialized pathNodes (ie pathNodes with very high gCosts)
        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {
                PathNode pathNode = grid.GetGridObject(x, y);
                pathNode.gCost = 99999999;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }
        //setup the start node to have a gCost of 0 and calculate the hCost (distance to endnode)
        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        //calculate startnode Fcost (G+H)
        startNode.CalculateFCost();
        
        //update the grid visual
        PathfindingDebugStepVisual.Instance.ClearSnapshots();
        PathfindingDebugStepVisual.Instance.TakeSnapshot(grid, startNode, openList, closedList);

        //while there are still nodes to explore
        while (openList.Count > 0) {
            //find the node of lowest F cost
            PathNode currentNode = GetLowestFCostNode(openList);
            //are we at the final node?
            if (currentNode == endNode) {
                //update final path on visual
                PathfindingDebugStepVisual.Instance.TakeSnapshot(grid, currentNode, openList, closedList);
                PathfindingDebugStepVisual.Instance.TakeSnapshotFinalPath(grid, CalculatePath(endNode));
                //return final path
                return CalculatePath(endNode);
            }

            //else if it isnt the final node
            //remove the current node from the openList and add it to the closedList
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            //iterate through each neighbor node
            foreach (PathNode neighbourNode in GetNeighbourList(currentNode)) {
                //check if the neighbor has already been added to the closed list
                if (closedList.Contains(neighbourNode)) continue;
                //check if the neighbor node is an obstacle
                if (!neighbourNode.isWalkable) {
                    closedList.Add(neighbourNode);
                    continue;
                }
                //calculate the neighbors g cost(distance to startnode)
                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                //if calculated gcost is less than the current gcost, update neighbor node with calculated g,h, and f costs
                if (tentativeGCost < neighbourNode.gCost) {
                    //set the cameFromNode value to the currentNode so we can trace back through the nodes to find the path
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost; //distance to startnode
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode); //distance to endnode
                    neighbourNode.CalculateFCost(); //measure of both
                    //if the neighbor is not already in the openList, add it
                    if (!openList.Contains(neighbourNode)) {
                        openList.Add(neighbourNode);
                    }
                }
                //update the pathing visual
                PathfindingDebugStepVisual.Instance.TakeSnapshot(grid, currentNode, openList, closedList);
            }
        }

        // Out of nodes on the openList
        return null;
    }

    //find all neighbors of currentNode
    private List<PathNode> GetNeighbourList(PathNode currentNode) {
        List<PathNode> neighbourList = new List<PathNode>();

        if (currentNode.x - 1 >= 0) {
            // Left
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
            // Left Down
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            // Left Up
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
        }
        if (currentNode.x + 1 < grid.GetWidth()) {
            // Right
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
            // Right Down
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            // Right Up
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
        }
        // Down
        if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
        // Up
        if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

        return neighbourList;
    }

    //get a specific node
    public PathNode GetNode(int x, int y) {
        return grid.GetGridObject(x, y);
    }

    //find the final path by navigating through the cameFromNode path.
    private List<PathNode> CalculatePath(PathNode endNode) {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null) {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    //calculate the distance cost between two points, used for finding g and h costs...includes diagonal
    private int CalculateDistanceCost(PathNode a, PathNode b) {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    //find the node with the lowest Fcost in a list of nodes
    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList) {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++) {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost) {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

}
