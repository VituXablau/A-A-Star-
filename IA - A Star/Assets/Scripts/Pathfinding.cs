using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    [SerializeField] private Vector2 gridSize;
    [SerializeField] private Vector2 cellSize;

    [SerializeField] private bool generatePath;
    private bool pathGenerated;

    private Dictionary<Vector2, Cell> cells;

    [SerializeField] private List<Vector2> cellsToSearch;
    [SerializeField] private List<Vector2> searchedCells;
    [SerializeField] private List<Vector2> finalPath;

    [SerializeField] private GameObject pathPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject finalPathPrefab;

    private List<GameObject> pathObjects = new List<GameObject>();
    private List<GameObject> wallObjects = new List<GameObject>();
    private List<GameObject> finalPathObjects = new List<GameObject>();

    private void Update()
    {
        if (generatePath && !pathGenerated)
        {
            ClearPathObjects();
            GenerateGrid();
            FindPath(new Vector2(0, 0), new Vector2(5, 7));
            SpawnObjects();
            pathGenerated = true;
        }
        else if (!generatePath)
        {
            pathGenerated = false;
            ClearPathObjects();
        }
    }

    private void GenerateGrid()
    {
        cells = new Dictionary<Vector2, Cell>();

        for (int x = 0; x < gridSize.x; x += (int)cellSize.x)
        {
            for (int y = 0; y < gridSize.y; y += (int)cellSize.y)
            {
                Vector2 pos = new Vector2(x, y);
                cells.Add(pos, new Cell(pos));

                if (Random.Range(0, 4) == 0)
                {
                    cells[pos].isWall = true;
                }
            }
        }
    }

    private void FindPath(Vector2 startPos, Vector2 endPos)
    {
        searchedCells = new List<Vector2>();
        cellsToSearch = new List<Vector2> { startPos };
        finalPath = new List<Vector2>();

        Cell startCell = cells[startPos];

        startCell.gCost = 0;
        startCell.hCost = GetDistance(startPos, endPos);
        startCell.fCost = GetDistance(startPos, endPos);

        while (cellsToSearch.Count > 0)
        {
            Vector2 cellToSearch = cellsToSearch[0];

            foreach (Vector2 pos in cellsToSearch)
            {
                Cell c = cells[pos];

                if (c.fCost < cells[cellToSearch].fCost || (c.fCost == cells[cellToSearch].fCost && c.hCost < cells[cellToSearch].hCost))
                {
                    cellToSearch = pos;
                }
            }

            cellsToSearch.Remove(cellToSearch);
            searchedCells.Add(cellToSearch);

            if (cellToSearch == endPos)
            {
                Cell pathCell = cells[endPos];

                while (pathCell.position != startPos)
                {
                    finalPath.Add(pathCell.position);
                    pathCell = cells[pathCell.conection];
                }
                return;
            }

            GetNeighbours(cellToSearch, endPos);
        }
    }

    private void GetNeighbours(Vector2 cellPos, Vector2 endPos)
    {
        for (float x = cellPos.x - cellSize.x; x <= cellPos.x + cellSize.x; x += cellSize.x)
        {
            for (float y = cellPos.y - cellSize.y; y <= cellPos.y + cellSize.y; y += cellSize.y)
            {
                Vector2 neighbourPos = new Vector2(x, y);

                if (cells.TryGetValue(neighbourPos, out Cell c) && !searchedCells.Contains(neighbourPos) && !cells[neighbourPos].isWall)
                {
                    int gCostToNeighbour = cells[cellPos].gCost + GetDistance(cellPos, neighbourPos);

                    if (gCostToNeighbour < cells[neighbourPos].gCost)
                    {
                        Cell neighbourCell = cells[neighbourPos];

                        neighbourCell.conection = cellPos;
                        neighbourCell.gCost = gCostToNeighbour;
                        neighbourCell.hCost = GetDistance(neighbourPos, endPos);
                        neighbourCell.fCost = neighbourCell.gCost + neighbourCell.hCost;

                        if (!cellsToSearch.Contains(neighbourPos))
                            cellsToSearch.Add(neighbourPos);
                    }
                }
            }
        }
    }

    private int GetDistance(Vector2 pos1, Vector2 pos2)
    {
        Vector2Int dist = new Vector2Int(Mathf.Abs((int)pos1.x - (int)pos2.x), Mathf.Abs((int)pos1.y - (int)pos2.y));

        int lowest = Mathf.Min(dist.x, dist.y);
        int highest = Mathf.Max(dist.x, dist.y);

        int horizontalMovesRequired = highest - lowest;

        return lowest * 14 + horizontalMovesRequired * 10;
    }

    private void SpawnObjects()
    {
        foreach (var cell in cells)
        {
            Vector3 worldPos = new Vector3(cell.Key.x, cell.Key.y, 0);

            if (cell.Value.isWall)
            {
                GameObject wall = Instantiate(wallPrefab, worldPos, Quaternion.identity, transform);
                wallObjects.Add(wall);
            }
            else
            {
                GameObject path = Instantiate(pathPrefab, worldPos, Quaternion.identity, transform);
                pathObjects.Add(path);
            }
        }

        foreach (Vector2 pos in finalPath)
        {
            Vector3 worldPos = new Vector3(pos.x * cellSize.x, pos.y * cellSize.y, 0);
            GameObject finalPathObj = Instantiate(finalPathPrefab, worldPos, Quaternion.identity, transform);
            finalPathObjects.Add(finalPathObj);
        }
    }

    private void ClearPathObjects()
    {
        foreach (GameObject obj in pathObjects)
            Destroy(obj);
        pathObjects.Clear();

        foreach (GameObject obj in wallObjects)
            Destroy(obj);
        wallObjects.Clear();

        foreach (GameObject obj in finalPathObjects)
            Destroy(obj);
        finalPathObjects.Clear();
    }

    private class Cell
    {
        public Vector2 position;
        public int gCost = int.MaxValue;
        public int hCost = int.MaxValue;
        public int fCost = int.MaxValue;
        public Vector2 conection;
        public bool isWall;

        public Cell(Vector2 position)
        {
            this.position = position;
        }
    }
}
