using System.Collections.Generic;
using UnityEngine;

public class BlockPool : MonoBehaviour
{
    public LevelSetting levelSetting;
    public static BlockPool Instance;
    public Transform gridParent;
    private int rows;
    private int columns;
    private int numberOfColors;
    private float blockSizeHorizontal;
    private float verticalSpacing;
    private float startPositionX;
    private float startPositionY;
    public List<Block> blockPrefabs;
    private Dictionary<int, Queue<Block>> poolDictionary = new Dictionary<int, Queue<Block>>();

    void Awake()
    {
        Instance = this;
        InitializeBoard();
        InitializePools();
    }

    void InitializeBoard()
    {
        rows = levelSetting.rows;
        columns = levelSetting.columns;
        blockSizeHorizontal = levelSetting.blockSizeHorizontal;
        verticalSpacing = levelSetting.verticalSpacing;
        numberOfColors = levelSetting.numberOfColors;
        startPositionX = (blockSizeHorizontal * (columns - 1))/2;
        startPositionY = (verticalSpacing * (rows - 1))/2;
    }

    void InitializePools()
    {
        foreach (Block blockPrefab in blockPrefabs)
        {
            Queue<Block> pool = new Queue<Block>();
            for (int i = 0; i < 10; i++)
            {
                Block block = Instantiate(blockPrefab, gridParent);
                block.gameObject.SetActive(false);
                block.transform.SetParent(transform);
                pool.Enqueue(block);
            }
            poolDictionary[blockPrefab.type] = pool;
        }
    }

    public Block GetBlock(int row, int column)
    {
        Vector3 position = new Vector3(column * blockSizeHorizontal - startPositionX, row * verticalSpacing - startPositionY, 0);
        int type = Random.Range(0, numberOfColors);
        if (poolDictionary[type].Count > 0)
        {
            Block block = poolDictionary[type].Dequeue();
            block.gameObject.transform.SetParent(gridParent);
            block.gameObject.transform.localPosition = position;
            block.gameObject.transform.localScale = Vector3.one;
            block.Initialize(row, column);
            block.gameObject.SetActive(true);
            return block;
        }
        else
        {
            Block selectedPrefab = blockPrefabs[type];
            Block newBlock = Instantiate(selectedPrefab, gridParent);
            newBlock.gameObject.transform.localPosition = position;
            newBlock.Initialize(row, column);
            return newBlock;
        }
    }

    public void ReturnBlock(Block block, int type)
    {
        block.gameObject.SetActive(false);
        block.gameObject.transform.SetParent(transform);
        block.UpdateSprite(1);

        if (!poolDictionary.ContainsKey(type))
        {
            poolDictionary[type] = new Queue<Block>();
        }
        poolDictionary[type].Enqueue(block);
    }
}
