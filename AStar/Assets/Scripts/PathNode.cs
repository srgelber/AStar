/* 
CSC 378 Lab 6 - A*
By: Simon Gelber
Modified from Code Monkey A* Tutorial
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//a class representing each grid node
public class PathNode {

    //grid coords
    private Grid<PathNode> grid;
    public int x;
    public int y;

    //g, h, and f costs
    public int gCost;
    public int hCost;
    public int fCost;

    //is this tile an obstacle?
    public bool isWalkable;

    //reference to previous node in path so we can trace back through to get full path
    public PathNode cameFromNode;

    //node constructor
    public PathNode(Grid<PathNode> grid, int x, int y) {
        this.grid = grid;
        this.x = x;
        this.y = y;
        isWalkable = true;
    }

    //calculate f cost by adding g and h costs
    public void CalculateFCost() {
        fCost = gCost + hCost;
    }

    //method to change if the node is walkable
    public void SetIsWalkable(bool isWalkable) {
        this.isWalkable = isWalkable;
        grid.TriggerGridObjectChanged(x, y);
    }

    //convert coords to string
    public override string ToString() {
        return x + "," + y;
    }

}
