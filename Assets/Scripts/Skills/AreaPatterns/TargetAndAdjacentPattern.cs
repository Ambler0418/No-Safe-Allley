using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TargetAndAdjacentPattern", menuName = "Skills/Area Patterns/Target and Adjacent")]
public class TargetAndAdjacentPattern : AreaPattern
{
    public override List<Vector3Int> GetAffectedTiles(Vector3Int targetTile)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();

        // 1. 선택한 타일 (Target) 추가
        // 2. 인접 타일 오프셋 정의 (Unity 기본 Pointy-top Hexagon 기준)
        Vector3Int[] offsets;

        if (targetTile.y % 2 == 0) // 짝수 행 (Even Row)
        {
            offsets = new Vector3Int[]
            {
                new Vector3Int(-1, 1, 0),  // 왼쪽 위
                new Vector3Int(0, 1, 0),   // 오른쪽 위
                new Vector3Int(1, 0, 0),   // 오른쪽
                new Vector3Int(0, -1, 0),  // 오른쪽 아래
                new Vector3Int(-1, -1, 0), // 왼쪽 아래
                new Vector3Int(-1, 0, 0),   // 왼쪽
                new Vector3Int(0, 0, 0)
            };
        }
        else // 홀수 행 (Odd Row)
        {
            offsets = new Vector3Int[]
            {
                new Vector3Int(0, 1, 0),   // 왼쪽 위
                new Vector3Int(1, 1, 0),   // 오른쪽 위
                new Vector3Int(1, 0, 0),   // 오른쪽
                new Vector3Int(1, -1, 0),  // 오른쪽 아래
                new Vector3Int(0, -1, 0),  // 왼쪽 아래
                new Vector3Int(-1, 0, 0),   // 왼쪽
                new Vector3Int(0, 0, 0)
            };
        }

        // 3. 오프셋을 더해 인접 타일 좌표 계산
        foreach (Vector3Int offset in offsets)
        {
            tiles.Add(offset + targetTile);
        }

        return tiles;
    }
}
