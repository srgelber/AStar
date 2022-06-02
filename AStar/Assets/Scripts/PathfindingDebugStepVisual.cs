/* 
CSC 378 Lab 6 - A*
By: Simon Gelber
Modified from Code Monkey A* Tutorial
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CodeMonkey.Utils;
//this script provides the visualization of the pathfinding algorithm. It displays the red, blue and green nodes.
public class PathfindingDebugStepVisual : MonoBehaviour {

    public static PathfindingDebugStepVisual Instance { get; private set; }
    //I had to add visualComplete and visualStart in order to track when the visualization was complete
    //This allowed me to not move the demon until the visual finished.
    public bool visualComplete;
    public bool visualStart;

    [SerializeField] private Transform pfPathfindingDebugStepVisualNode;
    private List<Transform> visualNodeList;
    private List<GridSnapshotAction> gridSnapshotActionList;
    private bool autoShowSnapshots;
    private float autoShowSnapshotsTimer;
    private Transform[,] visualNodeArray; 
    //initialize node lists and set visualComplete and visualStart to false
    private void Awake() {
        Instance = this;
        visualComplete = false;
        visualStart = false;
        visualNodeList = new List<Transform>();
        gridSnapshotActionList = new List<GridSnapshotAction>();
    }

    //setup and create nodes for visualization
    public void Setup(Grid<PathNode> grid) {
        visualNodeArray = new Transform[grid.GetWidth(), grid.GetHeight()];

        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {
                Vector3 gridPosition = new Vector3(x, y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * .5f;
                Transform visualNode = CreateVisualNode(gridPosition);
                visualNodeArray[x, y] = visualNode;
                visualNodeList.Add(visualNode);
            }
        }
        HideNodeVisuals();
    }

    //this function drives the visualization and updates the current grid snapshot
    //I changed it to display the visual every time the pathing algorithm is called
    private void Update() {

        autoShowSnapshots = true;
        if (autoShowSnapshots) {
            float autoShowSnapshotsTimerMax = .05f;
            autoShowSnapshotsTimer -= Time.deltaTime;
            if (autoShowSnapshotsTimer <= 0f) {
                autoShowSnapshotsTimer += autoShowSnapshotsTimerMax;
                ShowNextSnapshot();
                if (gridSnapshotActionList.Count == 0) {
                    autoShowSnapshots = false;
                }
            }
        }
    }
    //a method I created to tell the playermovement script when the visual is complete so they can move
    public bool visualCompleted(){
        if(visualComplete == true){
            return true;
        }else{
            return false;
        }
    }

    //this was for showing one step at a time, I didn't end up using this
    private void ShowNextSnapshot() {
        if (gridSnapshotActionList.Count > 0) {
            visualComplete = false;
            GridSnapshotAction gridSnapshotAction = gridSnapshotActionList[0];
            gridSnapshotActionList.RemoveAt(0);
            gridSnapshotAction.TriggerAction();
            visualStart = true;
        }else if (visualStart == true){
            visualComplete = true;
            visualStart = false;
        }
    }

    //clear grid
    public void ClearSnapshots() {
        gridSnapshotActionList.Clear();
    }

    //update the grid to represent the current state of the open and closed lists
    public void TakeSnapshot(Grid<PathNode> grid, PathNode current, List<PathNode> openList, List<PathNode> closedList) {
        GridSnapshotAction gridSnapshotAction = new GridSnapshotAction();
        gridSnapshotAction.AddAction(HideNodeVisuals);
        //for the size of the grid
        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {
                PathNode pathNode = grid.GetGridObject(x, y);
                //update g,h,and f costs
                int gCost = pathNode.gCost;
                int hCost = pathNode.hCost;
                int fCost = pathNode.fCost;
                Vector3 gridPosition = new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * .5f;
                bool isCurrent = pathNode == current;
                bool isInOpenList = openList.Contains(pathNode);
                bool isInClosedList = closedList.Contains(pathNode);
                int tmpX = x;
                int tmpY = y;

                gridSnapshotAction.AddAction(() => {
                    Transform visualNode = visualNodeArray[tmpX, tmpY];
                    SetupVisualNode(visualNode, gCost, hCost, fCost);
                    //color background and nodes either blue, red, green, or white
                    Color backgroundColor = UtilsClass.GetColorFromString("F1EFEF");

                    if (isInClosedList) {
                        backgroundColor = new Color(1, 0, 0);
                    }
                    if (isInOpenList) {
                        backgroundColor = UtilsClass.GetColorFromString("009AFF");
                    }
                    if (isCurrent) {
                        backgroundColor = new Color(0, 1, 0);
                    }

                    visualNode.Find("sprite").GetComponent<SpriteRenderer>().color = backgroundColor;
                });
            }
        }

        gridSnapshotActionList.Add(gridSnapshotAction);
    }

    //this function creates the final path grid and colors the path green
    //this is the last thing that happens before the player moves
    public void TakeSnapshotFinalPath(Grid<PathNode> grid, List<PathNode> path) {
        GridSnapshotAction gridSnapshotAction = new GridSnapshotAction();
        gridSnapshotAction.AddAction(HideNodeVisuals);
        //this is very similar to the function above
        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {
                PathNode pathNode = grid.GetGridObject(x, y);

                int gCost = pathNode.gCost;
                int hCost = pathNode.hCost;
                int fCost = pathNode.fCost;
                Vector3 gridPosition = new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * .5f;
                bool isInPath = path.Contains(pathNode);
                int tmpX = x;
                int tmpY = y;

                gridSnapshotAction.AddAction(() => { 
                    Transform visualNode = visualNodeArray[tmpX, tmpY];
                    SetupVisualNode(visualNode, gCost, hCost, fCost);

                    Color backgroundColor;
                    //color the path green
                    if (isInPath) {
                        backgroundColor = new Color(0, 1, 0);
                    } else {
                        backgroundColor = UtilsClass.GetColorFromString("F1EFEF");
                    }

                    visualNode.Find("sprite").GetComponent<SpriteRenderer>().color = backgroundColor;
                });
            }
        }

        gridSnapshotActionList.Add(gridSnapshotAction);
    }
    //hide visuals
    private void HideNodeVisuals() {
        foreach (Transform visualNodeTransform in visualNodeList) {
            SetupVisualNode(visualNodeTransform, 9999, 9999, 9999);
        }
    }
    //create new node
    private Transform CreateVisualNode(Vector3 position) {
        Transform visualNodeTransform = Instantiate(pfPathfindingDebugStepVisualNode, position, Quaternion.identity);
        return visualNodeTransform;
    }

    //setup node text
    private void SetupVisualNode(Transform visualNodeTransform, int gCost, int hCost, int fCost) {
        if (fCost < 1000) {
            visualNodeTransform.Find("gCostText").GetComponent<TextMeshPro>().SetText(gCost.ToString());
            visualNodeTransform.Find("hCostText").GetComponent<TextMeshPro>().SetText(hCost.ToString());
            visualNodeTransform.Find("fCostText").GetComponent<TextMeshPro>().SetText(fCost.ToString());
        } else {
            visualNodeTransform.Find("gCostText").GetComponent<TextMeshPro>().SetText("");
            visualNodeTransform.Find("hCostText").GetComponent<TextMeshPro>().SetText("");
            visualNodeTransform.Find("fCostText").GetComponent<TextMeshPro>().SetText("");
        }
    }
    //drives the grid snapshots
    private class GridSnapshotAction {

        private Action action;

        public GridSnapshotAction() {
            action = () => { };
        }

        public void AddAction(Action action) {
            this.action += action;
        }

        public void TriggerAction() {
            action();
        }

    }

}

