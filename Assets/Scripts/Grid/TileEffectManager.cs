using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic; // 리스트 사용을 위해 추가
using System.Linq; // Linq 사용을 위해 추가

public class TileEffectManager : MonoBehaviour
{
    public static TileEffectManager Instance { get; private set; }

    [Header("Tilemaps")]
    public Tilemap effectTilemap; // 이동/공격 범위 등 임시 효과를 표시할 타일맵
    public Tilemap objectTilemap; // 안개, 유닛 상태 표시 등 영구적인 오브젝트를 표시할 타일맵

    [Header("Effect Tiles")]
    public Tile reconHighlightTile; // 정찰 완료 하이라이트
    public Tile assaultHighlightTile; // 공격 가능 하이라이트
    public Tile moveHighlightTile;  // 이동 가능 범위 하이라이트

    [Header("Object & Fog Tiles")]
    public Tile fogTile; // 전장의 안개 타일 (흐릿한 효과)
    public Tile hiddenIndicatorTile; // 아군 숨김 상태 표시 타일
    public Tile exposedIndicatorTile; // 아군 노출 상태 표시 타일

    [Header("Effect Prefabs")]
    [SerializeField] private GameObject selectionHighlightPrefab; // 선택 하이라이트 효과 프리팹

    // 영구적으로 하이라이트된 타일들을 추적하기 위한 HashSet
    private HashSet<Vector3Int> permanentlyHighlightedTiles = new HashSet<Vector3Int>();
    // Flash 효과가 진행 중인 타일들을 추적하기 위한 HashSet
    private HashSet<Vector3Int> flashingTiles = new HashSet<Vector3Int>();

    private Grid grid; // 타일 좌표를 월드 좌표로 변환하기 위한 참조
    private GameObject currentSelectionHighlight; // 현재 활성화된 선택 하이라이트 오브젝트

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
        grid = FindObjectOfType<Grid>();
    }

    // --- 새로운 Fog of War 및 상태 표시 메서드 ---

    /// <summary>
    /// 적군 영역에 전장의 안개를 초기화합니다.
    /// </summary>
    public void InitializeFogOfWar()
    {
        if (fogTile == null || objectTilemap == null || PlacementManager.Instance == null)
        {
            Debug.LogWarning("Fog of War 초기화에 필요한 컴포넌트가 없습니다 (fogTile, objectTilemap, PlacementManager).");
            return;
        }

        objectTilemap.ClearAllTiles(); // 시작 시 오브젝트 타일맵 초기화
        Tilemap enemyTerritory = PlacementManager.Instance.enemyTilemap;

        foreach (var pos in enemyTerritory.cellBounds.allPositionsWithin)
        {
            if (enemyTerritory.HasTile(pos))
            {
                objectTilemap.SetTile(pos, fogTile);
            }
        }
    }

    /// <summary>
    /// 지정된 위치의 안개를 제거합니다.
    /// </summary>
    public void ClearFog(Vector3Int position)
    {
        if (objectTilemap != null && objectTilemap.GetTile(position) == fogTile)
        {
            objectTilemap.SetTile(position, null);
        }
    }
    
    /// <summary>
    /// 모든 아군 유닛의 노출/숨김 상태 표시를 업데이트합니다.
    /// </summary>
    public void UpdateUnitStatusIndicators()
    {
        if (objectTilemap == null || hiddenIndicatorTile == null || exposedIndicatorTile == null)
        {
             Debug.LogWarning("유닛 상태 표시기에 필요한 타일이 할당되지 않았습니다.");
             return;
        }

        // 1. 기존의 모든 상태 표시기 타일을 먼저 지웁니다.
        //    (안개 타일은 건드리지 않기 위해 특정 타일만 순회하며 지웁니다)
        List<Vector3Int> tilesToRemove = new List<Vector3Int>();
        foreach (var pos in objectTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = objectTilemap.GetTile(pos);
            if (tile == hiddenIndicatorTile || tile == exposedIndicatorTile)
            {
                tilesToRemove.Add(pos);
            }
        }
        foreach(var pos in tilesToRemove)
        {
            objectTilemap.SetTile(pos, null);
        }


        // 2. GameManager의 유닛 레지스트리를 기반으로 새 표시기를 설정합니다.
        if (GameManager.Instance == null) return;

        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit.owner == GameManager.Player.Player1) // 아군 유닛만
            {
                Tile toPlace = unit.isRevealed ? exposedIndicatorTile : hiddenIndicatorTile;
                objectTilemap.SetTile(unit.location, toPlace);
            }
        }
    }


    // --- 기존 하이라이트 및 이펙트 메서드 (단순화) ---

    public void AddMoveHighlight(Vector3Int cell)
    {
        effectTilemap.SetTile(cell, moveHighlightTile);
    }

    public void RemoveMoveHighlight(Vector3Int cell)
    {
        if (effectTilemap.GetTile(cell) == moveHighlightTile)
        {
            effectTilemap.SetTile(cell, null);
        }
    }
    
    public void ShowSelectionHighlight(Vector3Int cell)
    {
        HideSelectionHighlight();
        if (selectionHighlightPrefab != null && grid != null)
        {
            Vector3 worldPos = grid.GetCellCenterWorld(cell);
            currentSelectionHighlight = Instantiate(selectionHighlightPrefab, worldPos, Quaternion.identity);
        }
    }

    public void HideSelectionHighlight()
    {
        if (currentSelectionHighlight != null)
        {
            Destroy(currentSelectionHighlight);
            currentSelectionHighlight = null;
        }
    }
    
    public void removePermanentlyHighlightedTile(Vector3Int targetCell)
    {
        if (permanentlyHighlightedTiles.Contains(targetCell))
        {
            permanentlyHighlightedTiles.Remove(targetCell);
            if (effectTilemap.GetTile(targetCell) == reconHighlightTile)
            {
                effectTilemap.SetTile(targetCell, null);
            }
        }
    }

    public void HighlightReconTile(Vector3Int targetCell)
    {
        if (reconHighlightTile == null) return;

        if (GameManager.Instance != null)
        {
            UnitInstance unit = GameManager.Instance.GetUnitAt(targetCell);
            if (unit != null && unit.owner == GameManager.Player.Player1)
            {
                return;
            }
        }

        permanentlyHighlightedTiles.Add(targetCell);
        effectTilemap.SetTile(targetCell, reconHighlightTile);
    }

    public void RemoveReconHighlight(Vector3Int targetCell)
    {
        if (permanentlyHighlightedTiles.Contains(targetCell))
        {
            permanentlyHighlightedTiles.Remove(targetCell);
            if (effectTilemap.GetTile(targetCell) == reconHighlightTile)
            {
                effectTilemap.SetTile(targetCell, null);
            }
        }
    }

    public void FlashReconTile(Vector3Int targetCell, float duration = 0.5f)
    {
        StartCoroutine(FlashRoutine(targetCell, duration));
    }

    public void ClearEffectTileSafe(Vector3Int cell)
    {
        if (!permanentlyHighlightedTiles.Contains(cell) && !flashingTiles.Contains(cell))
        {
            effectTilemap.SetTile(cell, null);
        }
    }

    private System.Collections.IEnumerator FlashRoutine(Vector3Int cell, float duration)
    {
        if (PlacementManager.Instance != null)
        {
            if (!PlacementManager.Instance.allyTilemap.HasTile(cell) && !PlacementManager.Instance.enemyTilemap.HasTile(cell))
            {
                yield break;
            }
        }

        flashingTiles.Add(cell);

        if (reconHighlightTile != null)
        {
            effectTilemap.SetTile(cell, reconHighlightTile);
        }

        yield return new WaitForSeconds(duration);
        
        flashingTiles.Remove(cell);

        if (!permanentlyHighlightedTiles.Contains(cell))
        {
            if (effectTilemap.GetTile(cell) == reconHighlightTile)
            {
                effectTilemap.SetTile(cell, null);
            }
        }
    }

    public void ClearTemporaryTiles()
    {
        foreach (var pos in effectTilemap.cellBounds.allPositionsWithin)
        {
            if (effectTilemap.HasTile(pos) && !permanentlyHighlightedTiles.Contains(pos) && !flashingTiles.Contains(pos))
            {
                effectTilemap.SetTile(pos, null);
            }
        }
    }

    public void ClearAllEffectTiles()
    {
        permanentlyHighlightedTiles.Clear();
        flashingTiles.Clear();
        effectTilemap.ClearAllTiles();
    }
}

