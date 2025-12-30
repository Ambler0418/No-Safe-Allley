using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic; // 리스트 사용을 위해 추가
using System.Linq; // Linq 사용을 위해 추가

public class TileEffectManager : MonoBehaviour
{
    public static TileEffectManager Instance { get; private set; }

    [Header("Tilemaps")]
    public Tilemap effectTilemap; // 타일 효과를 표시할 별도의 타일맵

    [Header("Effect Tiles")]
    public Tile reconHighlightTile; // 정찰 효과에 사용할 붉은색 타일 애셋
    public Tile moveHighlightTile;  // 이동 가능 범위 표시에 사용할 타일 애셋

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// 지정된 위치에 정찰 하이라이트 타일을 표시합니다.
    /// </summary>
    public void HighlightReconTile(Vector3Int targetCell)
    {
        if (reconHighlightTile == null)
        {
            Debug.LogError("Recon Highlight Tile이 할당되지 않았습니다!");
            return;
        }
        effectTilemap.SetTile(targetCell, reconHighlightTile);
        Debug.Log($"{targetCell} 위치에 정찰 효과를 표시합니다.");
    }

    /// <summary>
    /// 이펙트 타일맵의 모든 타일을 지웁니다.
    /// </summary>
    public void ClearAllEffectTiles()
    {
        effectTilemap.ClearAllTiles();
    }
}
