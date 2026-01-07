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
    public Tile reconHighlightTile;
    
    public Tile assaultHighlightTile; // 정찰 효과에 사용할 붉은색 타일 애셋
    public Tile moveHighlightTile;  // 이동 가능 범위 표시에 사용할 타일 애셋

    // 영구적으로 하이라이트된 타일들을 추적하기 위한 HashSet
    private HashSet<Vector3Int> permanentlyHighlightedTiles = new HashSet<Vector3Int>();
    // Flash 효과가 진행 중인 타일들을 추적하기 위한 HashSet
    private HashSet<Vector3Int> flashingTiles = new HashSet<Vector3Int>();

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
    /// 지정된 위치에 정찰 하이라이트 타일을 표시하고 영구 목록에 추가합니다.
    /// 아군 유닛의 위치인 경우 영구 마킹을 생략합니다.
    /// </summary>
    public void HighlightReconTile(Vector3Int targetCell)
    {
        if (reconHighlightTile == null)
        {
            Debug.LogError("Recon Highlight Tile이 할당되지 않았습니다!");
            return;
        }

        // 아군 유닛(Player1)의 위치인 경우 영구 마킹을 하지 않음 (사용자 요청)
        if (GameManager.Instance != null)
        {
            UnitInstance unit = GameManager.Instance.GetUnitAt(targetCell);
            if (unit != null && unit.owner == GameManager.Player.Player1)
            {
                return;
            }
        }

        permanentlyHighlightedTiles.Add(targetCell); // 목록에 추가
        effectTilemap.SetTile(targetCell, reconHighlightTile);
    }

    /// <summary>
    /// 지정된 위치의 정찰 하이라이트를 제거합니다.
    /// </summary>
    public void RemoveReconHighlight(Vector3Int targetCell)
    {
        if (permanentlyHighlightedTiles.Contains(targetCell))
        {
            permanentlyHighlightedTiles.Remove(targetCell);
            // 해당 위치의 타일이 reconHighlightTile일 경우에만 지움
            if (effectTilemap.GetTile(targetCell) == reconHighlightTile)
            {
                effectTilemap.SetTile(targetCell, null);
            }
        }
    }

    /// <summary>
    /// 타일을 잠시 동안 하이라이트하고 지웁니다. (빈 땅 정찰 연출용)
    /// </summary>
    public void FlashReconTile(Vector3Int targetCell, float duration = 0.5f)
    {
        StartCoroutine(FlashRoutine(targetCell, duration));
    }

    /// <summary>
    /// 영구 하이라이트된 타일이 아니라면 해당 위치의 효과를 지웁니다.
    /// (UI 호버링 해제 등으로 인해 게임 로직상 중요한 표시가 지워지는 것을 방지)
    /// </summary>
    public void ClearEffectTileSafe(Vector3Int cell)
    {
        // 영구 하이라이트도 아니고, Flash 중인 타일도 아닐 때만 지움
        if (!permanentlyHighlightedTiles.Contains(cell) && !flashingTiles.Contains(cell))
        {
            effectTilemap.SetTile(cell, null);
        }
    }

    private System.Collections.IEnumerator FlashRoutine(Vector3Int cell, float duration)
    {
        // Flash 시작: 보호 목록에 추가
        flashingTiles.Add(cell);

        // Flash는 영구 목록에 추가하지 않고 타일만 설정
        if (reconHighlightTile != null)
        {
            // Debug.Log($"FlashRoutine: {cell}");
            effectTilemap.SetTile(cell, reconHighlightTile);
        }

        yield return new WaitForSeconds(duration);
        
        // Flash 종료: 보호 목록에서 제거
        flashingTiles.Remove(cell);

        // 영구 하이라이트 목록에 없는 경우에만 지움
        if (!permanentlyHighlightedTiles.Contains(cell))
        {
            TileBase current = effectTilemap.GetTile(cell);
            if (current == reconHighlightTile)
            {
                effectTilemap.SetTile(cell, null);
            }
        }
    }

    /// <summary>
    /// 영구 하이라이트된 타일을 제외한 모든 임시 효과(호버링, 스킬 범위 등)를 지웁니다.
    /// 스킬 모드 종료 시 호출하여 붉은색 잔상을 제거합니다.
    /// </summary>
    public void ClearTemporaryTiles()
    {
        // 타일맵의 모든 위치를 순회하며 영구 목록에 없는 타일만 제거
        // (성능상 비효율적일 수 있으나, 맵 크기가 작다면 허용 범위)
        foreach (var pos in effectTilemap.cellBounds.allPositionsWithin)
        {
            // Flash 중인 타일도 지우지 않음
            if (effectTilemap.HasTile(pos) && !permanentlyHighlightedTiles.Contains(pos) && !flashingTiles.Contains(pos))
            {
                effectTilemap.SetTile(pos, null);
            }
        }
    }

    /// <summary>
    /// 이펙트 타일맵의 모든 타일을 지웁니다.
    /// </summary>
    public void ClearAllEffectTiles()
    {
        permanentlyHighlightedTiles.Clear();
        flashingTiles.Clear(); // Flash 목록도 초기화
        effectTilemap.ClearAllTiles();
    }
}
