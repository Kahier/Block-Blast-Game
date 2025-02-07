using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    public LevelSetting levelSetting;
    public SpriteRenderer spriteRenderer;
    public Collider2D blockCollider;
    public int row;
    public int column;
    public int type;
    public Sprite Sprite_Default;
    private int conditionForGroupA;
    private int conditionForGroupB;
    private int conditionForGroupC;
    public Sprite Sprite_A;
    public Sprite Sprite_B;
    public Sprite Sprite_C;

    void Awake()
    {
        InitializeBoard();
    }

    void InitializeBoard()
    {
        conditionForGroupA = levelSetting.conditionForGroupA;
        conditionForGroupB = levelSetting.conditionForGroupB;
        conditionForGroupC = levelSetting.conditionForGroupC;
    }
    public void Initialize(int row, int column)
    {
        this.row = row;
        this.column = column;
        spriteRenderer.sortingOrder = row*2;
    }

    public void UpdateSprite(int groupSize)
    {
        if (groupSize >= conditionForGroupC)
            spriteRenderer.sprite = Sprite_C;
        else if (groupSize >= conditionForGroupB)
            spriteRenderer.sprite = Sprite_B;
        else if (groupSize >= conditionForGroupA)
            spriteRenderer.sprite = Sprite_A;
        else
            spriteRenderer.sprite = Sprite_Default;
    }

    void OnMouseEnter()
    {
        transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad);
        spriteRenderer.sortingOrder += 1;
    }

    void OnMouseExit()
    {
        transform.DOScale(1f, 0.2f).SetEase(Ease.InQuad);
        spriteRenderer.sortingOrder -= 1;
    }
}
