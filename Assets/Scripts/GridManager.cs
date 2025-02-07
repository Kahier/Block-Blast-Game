using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using System.Threading.Tasks;

public class GridManager : MonoBehaviour
{
    public LevelSetting levelSetting;
    public BoardShuffler boardShuffler;
    public Transform gridParent;
    public Transform environment;
    private int rows;
    private int columns;
    private float blockSizeHorizontal = 2.25f;
    private float verticalSpacing = 2.14f;
    private float startPositionX;
    private float startPositionY;
    private Block[,] grid;
    private bool[,] visited;


    List<List<Block>> validGroups = new List<List<Block>>();
    Dictionary<Block, List<Block>> blockToGroupMap = new Dictionary<Block, List<Block>>();

    void Start()
    {
        InitializeBoard();
        GenerateGrid();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    void InitializeBoard()
    {
        rows = levelSetting.rows;
        columns = levelSetting.columns;
        blockSizeHorizontal = levelSetting.blockSizeHorizontal;
        verticalSpacing = levelSetting.verticalSpacing;
        environment.localScale = new Vector3(columns,rows,1);
        grid = new Block[rows*2, columns];
        startPositionX = (blockSizeHorizontal * (columns - 1))/2;
        startPositionY = (verticalSpacing * (rows - 1))/2;
        float fullSizeScale = 2.4f / (columns > rows ? columns : rows);
        gridParent.localScale = new Vector3(fullSizeScale, fullSizeScale, 1);
        if (rows * columns > 120)
        {
            DOTween.SetTweensCapacity(1250, 50);
        }
        else if (rows * columns > 49)
        {
            DOTween.SetTweensCapacity(500, 50);
        }
    }

    void HandleMouseClick()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
        if (hit.collider is not null)
        {
            hit.collider.TryGetComponent(out Block clickedBlock);

            if (blockToGroupMap.ContainsKey(clickedBlock))
            {
                List<Block> group = blockToGroupMap[clickedBlock];
                BlastBlocks(group);
                ScanForGroups();
            }
        }
    }

    void GenerateGrid() 
    {
        for (int row = rows; row < rows * 2; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                grid[row, column] = BlockPool.Instance.GetBlock(row, column);
            }
        }
        DropBlocks();
        ScanForGroups();
    }

    void FloodFill(int row, int column, int type, List<Block> connected)
    {
        if (row < 0 || row >= rows || column < 0 || column >= columns || visited[row, column])
            return;

        Block block = grid[row, column];
        if (block is null || block.type != type)
            return;

        visited[row, column] = true;
        connected.Add(block);

        FloodFill(row - 1, column, type, connected);
        FloodFill(row + 1, column, type, connected);
        FloodFill(row, column - 1, type, connected);
        FloodFill(row, column + 1, type, connected);
    }

    void BlastBlocks(List<Block> blocks)
    {
        foreach (Block block in blocks)
        {
            grid[block.row, block.column] = null;
            BlockPool.Instance.ReturnBlock(block, block.type);
            PutNewBlock(block.column);

        }
        DropBlocks();
    }

    void PutNewBlock(int column)
    {
        for(int row = rows; row <=rows*2; row++)
        {
            if(grid[row, column] is null)
            {
                Block block = BlockPool.Instance.GetBlock(row, column);
                grid[row, column] = block;
                break;
            }
        }
    }

    void DropBlocks()
    {
        for (int column = 0; column < columns; column++)
        {
            int emptyRow = -1;

            for (int row = 0; row < rows*2; row++)
            {
                if (grid[row, column] is null)
                {
                    if(row >= rows) break;
                    if (emptyRow == -1) emptyRow = row;
                }
                else if (emptyRow != -1)
                {
                    Block block = grid[row, column];
                    grid[emptyRow, column] = block;
                    grid[row, column] = null;

                    Vector3 position = new Vector3(column * blockSizeHorizontal - startPositionX, emptyRow * verticalSpacing - startPositionY, 0);
                    block.transform.DOLocalMove(position, 0.8f).SetEase(Ease.OutBounce);

                    block.Initialize(emptyRow, column);

                    emptyRow++;
                }
            }
        }
    }

    void ScanForGroups()
    {
        validGroups.Clear();
        blockToGroupMap.Clear();
        visited = new bool[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                if (!visited[row, column])
                {
                    List<Block> connectedBlocks = new List<Block>();
                    FloodFill(row, column, grid[row, column].type, connectedBlocks);

                    if (connectedBlocks.Count >= 2)
                    {
                        validGroups.Add(connectedBlocks);

                        foreach (Block block in connectedBlocks)
                        {
                            blockToGroupMap[block] = connectedBlocks;
                            block.UpdateSprite(connectedBlocks.Count);
                        }
                    }
                    else
                    {
                        foreach (Block block in connectedBlocks)
                        {
                            block.UpdateSprite(0);
                        }
                    }
                }
            }
        }
        if (validGroups.Count == 0)
        {
            boardShuffler.ShuffleBoard(grid, rows, columns);
            ScanForGroups();
        }
    }

}
