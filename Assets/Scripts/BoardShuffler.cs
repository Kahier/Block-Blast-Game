using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using System.Threading.Tasks;

public class BoardShuffler : MonoBehaviour
{
    private int rows;
    private int columns;
    float blockSizeHorizontal = 2.25f;
    float verticalSpacing = 2.14f;
    float startPositionX;
    float startPositionY;
    Block[,] grid;
    public void ShuffleBoard(Block[,] grid, int rowCount, int columnCount)
    {
        rows = rowCount;
        columns = columnCount;
        startPositionX = (blockSizeHorizontal * (columns - 1))/2;
        startPositionY = (verticalSpacing * (rows - 1))/2;
        List<Block> blockList = new List<Block>();
        this.grid = grid;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Block block = grid[row, col];
                block.blockCollider.enabled = false;
                blockList.Add(block);
            }
        }
        
        for (int i = blockList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (blockList[i], blockList[j]) = (blockList[j], blockList[i]);
        }
        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Block block = blockList[index];
                grid[row, col] = block;
                grid[row, col].Initialize(row, col);
                Vector3 position = GetPositionFromGrid(row, col);
                index++;
            }
        }
        if (!HasValidGroup(grid))
        {
            ForceValidMatch(grid);
        }

        Sequence sequence = DOTween.Sequence();

        foreach (Block block in blockList)
        {
            sequence.Join(block.transform.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.InOutQuad));
            sequence.Join(block.transform.DOScale((rows > columns ? columns : rows) - 0.5f, 2f).SetEase(Ease.OutBack));
        }

        sequence.AppendCallback(() => 
        {
            MoveBlocks(blockList);
        });
    }

    private void MoveBlocks(List<Block> blockList)
    {

        foreach(Block block in blockList)
        {
            Vector3 position = GetPositionFromGrid(block.row, block.column);
            block.transform.DOLocalMove(position, 1.5f).SetEase(Ease.OutQuad);
            block.transform.DOScale(1f, 1.5f).SetEase(Ease.OutQuad).OnComplete(() => block.blockCollider.enabled = true);
        }
    }

    private bool HasValidGroup(Block[,] grid)
    {
        int[,] directions = { { 0, -1 }, { 0, 1 }, { -1, 0 }, { 1, 0 } };
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (IsPartOfGroup(grid, row, col, directions))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool IsPartOfGroup(Block[,] grid, int row, int column, int[,] directions)
    {
        int type = grid[row, column].type;
        for(int i = 0;i<4;i++)
        {
            int tempRow = row +  directions[i, 0];
            int tempColumn = column + directions[i, 1];
            if(!IsWithinBounds(tempRow,tempColumn)) continue;
            if(grid[tempRow, tempColumn].type == type) return true;
        }
        return false;
    }

    private void ForceValidMatch(Block[,] grid)
    {
        Dictionary<int, (int row, int col)> seenBlocks = new Dictionary<int, (int, int)>();
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int blockType = grid[row, col].type;

                if (seenBlocks.ContainsKey(blockType))
                {
                    var (firstRow, firstCol) = seenBlocks[blockType];

                    int[][] directions = { new int[] { 0, -1 }, new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 1, 0 } };
                    foreach (int[] dir in directions)  {
                        int newRow = row + dir[0];
                        int newCol = col + dir[1];
                        if(IsWithinBounds(newRow, newCol)){
                            row = seenBlocks[blockType].row;
                            col = seenBlocks[blockType].col;
                            Swap(grid, row, col, newRow, newCol);
                            return;
                        }
                    }
                }
                else
                {
                    seenBlocks[blockType] = (row, col);
                }
            }
        }
    }

    private bool IsWithinBounds(int row, int col)
    {
        return row >= 0 && row < rows && col >= 0 && col < columns;
    }

    private void Swap(Block[,] grid, int row1, int col1, int row2, int col2)
    {
        Block temp = grid[row1, col1];
        grid[row1, col1] = grid[row2, col2];
        Vector3 position = GetPositionFromGrid(row1, col1);
        grid[row2, col2] = temp;

        grid[row1, col1].Initialize(row1, col1);
        grid[row2, col2].Initialize(row2, col2);
    }

    private Vector3 GetPositionFromGrid(int row, int col)
    {
        return new Vector3(col * blockSizeHorizontal - startPositionX, row * verticalSpacing - startPositionY, 0);
    }
}
