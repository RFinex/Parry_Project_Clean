using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

internal static class RoomTileLayout
{
    private const int ChunkWidth = 6;
    private const int ChunkHeight = 4;
    private const int LayoutPasses = 3;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private static readonly int[] Opposite = { 1, 0, 3, 2 };

    private sealed class Chunk
    {
        public readonly TileChangeData[] Tiles = new TileChangeData[ChunkWidth * ChunkHeight];
        public readonly int[] Edges = new int[4];
    }

    public static bool TryGenerate(GameObject room, GameObject[] references, System.Random random, out string summary)
    {
        Tilemap ground = FindGround(room);
        PolygonCollider2D boundary = room.GetComponentInChildren<PolygonCollider2D>(true);
        Grid grid = ground != null ? ground.GetComponentInParent<Grid>(true) : null;
        if (ground == null || grid == null || boundary == null)
        {
            summary = "Template is missing its Ground tilemap, parent Grid, or camera boundary.";
            return false;
        }

        var original = ReadLayout(ground, boundary);
        var layout = new Dictionary<Vector2Int, Chunk>(original);
        var rules = new HashSet<int>[4];
        for (int direction = 0; direction < rules.Length; direction++)
            rules[direction] = new HashSet<int>();

        // [수정] 기존 방에서 관찰한 양쪽 경계의 타일 점유 패턴을 방향별로 기록합니다.
        RecordConnections(original, rules);
        var donors = new List<Dictionary<Vector2Int, Chunk>>();
        var rejected = new List<string>();
        foreach (GameObject reference in references)
        {
            if (reference == null)
                continue;

            Tilemap donorGround = FindGround(reference);
            PolygonCollider2D donorBoundary = reference.GetComponentInChildren<PolygonCollider2D>(true);
            Grid donorGrid = donorGround != null ? donorGround.GetComponentInParent<Grid>(true) : null;
            if (donorGrid == null || donorBoundary == null ||
                donorGrid.cellSize != grid.cellSize || donorGrid.cellGap != grid.cellGap ||
                donorGrid.cellLayout != grid.cellLayout || donorGrid.cellSwizzle != grid.cellSwizzle)
            {
                rejected.Add(reference.name);
                continue;
            }

            var donor = ReadLayout(donorGround, donorBoundary);
            var interior = donor.Where(entry => IsInsideBoundary(donorGround, donorBoundary, entry.Key))
                .ToDictionary(entry => entry.Key, entry => entry.Value);
            if (interior.Count == 0)
            {
                rejected.Add(reference.name);
                continue;
            }

            RecordConnections(donor, rules);
            donors.Add(interior);
        }

        if (donors.Count == 0)
        {
            summary = $"No usable references ({references.Length} scanned). Rejected: {string.Join(", ", rejected)}";
            return false;
        }

        EnemySpawnPoint[] spawns = room.GetComponentsInChildren<EnemySpawnPoint>();
        Vector3[] anchors = room.GetComponentsInChildren<PlayerSpawnPoint>(true).Select(point => point.transform.position)
            .Concat(room.GetComponentsInChildren<RoomExitObject>(true).Select(exit => exit.transform.position))
            .ToArray();
        var protectedCells = new HashSet<Vector3Int>();
        foreach (Vector3 position in anchors.Concat(spawns.Select(spawn => spawn.transform.position)))
        {
            Vector3Int cell = ground.WorldToCell(position);
            for (int y = -3; y <= 2; y++)
            for (int x = -1; x <= 1; x++)
                protectedCells.Add(cell + new Vector3Int(x, y, 0));
        }

        Physics2D.SyncTransforms();
        Vector2Int[] editable = original.Keys
            .Where(key => IsInsideBoundary(ground, boundary, key))
            .ToArray();
        var editableSet = new HashSet<Vector2Int>(editable);

        // [수정] 여러 구간을 함께 대입해 단일 구간으로는 이어지지 않는 조합도 시도합니다.
        for (int pass = 0; pass < LayoutPasses; pass++)
        {
            foreach (Vector2Int key in editable.OrderBy(_ => random.Next()))
            {
                int size = pass == 0 ? 2 : 1;
                // [수정] 같은 높이의 다른 열에서도 구간을 가져올 수 있습니다.
                var candidates = donors.SelectMany(donor => donor.Keys.Where(source => source.y == key.y),
                        (donor, source) => new { Donor = donor, Source = source })
                    .OrderBy(_ => random.Next());
                foreach (var candidate in candidates)
                {
                    if (TryPlacePatch(layout, original, candidate.Donor, editableSet, protectedCells,
                        rules, key, candidate.Source, size))
                        break;
                }
            }
        }

        int changedChunks = 0;
        foreach (var entry in layout)
        {
            if (SameTiles(entry.Value, original[entry.Key]))
                continue;

            Vector3Int targetOrigin = ChunkOrigin(entry.Key);
            for (int index = 0; index < entry.Value.Tiles.Length; index++)
            {
                TileChangeData tile = entry.Value.Tiles[index];
                Vector3Int destination = targetOrigin + new Vector3Int(index % ChunkWidth, index / ChunkWidth, 0);
                ground.SetTile(new TileChangeData(destination, tile.tile, tile.color, tile.transform), true);
            }
            changedChunks++;
        }

        if (changedChunks == 0)
        {
            summary = $"No compatible layout variation | References: {donors.Count}/{references.Length} | Interior chunks: {editable.Length}";
            return false;
        }

        if (ground.TryGetComponent(out TilemapCollider2D collider))
            collider.ProcessTilemapChanges();

        int movedSpawns = PlaceSpawns(ground, boundary, spawns, anchors, random);
        summary = $"References: {donors.Count}/{references.Length} | Chunks changed: {changedChunks} | Spawns moved: {movedSpawns}";
        return true;
    }

    private static Tilemap FindGround(GameObject room)
    {
        return room.GetComponentsInChildren<Tilemap>(true).FirstOrDefault(map => map.name == "Ground");
    }

    private static Dictionary<Vector2Int, Chunk> ReadLayout(Tilemap ground, PolygonCollider2D boundary)
    {
        // Prefab assets do not have live physics bounds. Use the serialized polygon points.
        Vector2[] points = boundary.points;
        var layout = new Dictionary<Vector2Int, Chunk>();
        if (points.Length == 0)
            return layout;

        Vector3Int[] cells = points.Select(point =>
            ground.WorldToCell(boundary.transform.TransformPoint(point + boundary.offset))).ToArray();
        Vector2Int min = ToChunk(new Vector3Int(cells.Min(cell => cell.x), cells.Min(cell => cell.y), 0)) - Vector2Int.one;
        Vector2Int max = ToChunk(new Vector3Int(cells.Max(cell => cell.x), cells.Max(cell => cell.y), 0)) + Vector2Int.one;
        for (int y = min.y; y <= max.y; y++)
        for (int x = min.x; x <= max.x; x++)
        {
            Vector2Int key = new Vector2Int(x, y);
            Vector3Int origin = ChunkOrigin(key);
            var chunk = new Chunk();
            for (int row = 0; row < ChunkHeight; row++)
            for (int column = 0; column < ChunkWidth; column++)
            {
                Vector3Int cell = origin + new Vector3Int(column, row, 0);
                TileBase tile = ground.GetTile(cell);
                chunk.Tiles[row * ChunkWidth + column] = new TileChangeData(
                    cell, tile, ground.GetColor(cell), ground.GetTransformMatrix(cell));
                if (tile == null)
                    continue;

                if (row == ChunkHeight - 1)
                    chunk.Edges[0] |= 1 << column;
                if (row == 0)
                    chunk.Edges[1] |= 1 << column;
                if (column == 0)
                    chunk.Edges[2] |= 1 << row;
                if (column == ChunkWidth - 1)
                    chunk.Edges[3] |= 1 << row;
            }
            layout.Add(key, chunk);
        }
        return layout;
    }

    private static void RecordConnections(Dictionary<Vector2Int, Chunk> layout, HashSet<int>[] rules)
    {
        foreach (var entry in layout)
        for (int direction = 0; direction < Directions.Length; direction++)
        {
            if (layout.TryGetValue(entry.Key + Directions[direction], out Chunk neighbor))
                rules[direction].Add(Connection(entry.Value, neighbor, direction));
        }
    }

    private static int Connection(Chunk first, Chunk second, int direction)
    {
        return (first.Edges[direction] << ChunkWidth) | second.Edges[Opposite[direction]];
    }

    private static bool TryPlacePatch(Dictionary<Vector2Int, Chunk> layout, Dictionary<Vector2Int, Chunk> original,
        Dictionary<Vector2Int, Chunk> donor, HashSet<Vector2Int> editable, HashSet<Vector3Int> protectedCells,
        HashSet<int>[] rules, Vector2Int start, Vector2Int sourceStart, int size)
    {
        var patch = new Dictionary<Vector2Int, Chunk>();
        bool changesTiles = false;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Vector2Int key = start + new Vector2Int(x, y);
            if (!editable.Contains(key) || !donor.TryGetValue(sourceStart + new Vector2Int(x, y), out Chunk replacement))
                return false;

            // Keep accepted changes in place so later passes cannot undo them.
            if (!SameTiles(layout[key], original[key]))
                return false;

            Chunk previous = original[key];
            Vector3Int origin = ChunkOrigin(key);
            for (int index = 0; index < previous.Tiles.Length; index++)
            {
                if ((previous.Tiles[index].tile == null) == (replacement.Tiles[index].tile == null))
                    continue;

                Vector3Int cell = origin + new Vector3Int(index % ChunkWidth, index / ChunkWidth, 0);
                if (protectedCells.Contains(cell))
                    return false;
                changesTiles = true;
            }
            patch.Add(key, replacement);
        }

        if (!changesTiles)
            return false;

        foreach (var entry in patch)
        for (int direction = 0; direction < Directions.Length; direction++)
        {
            Vector2Int neighborKey = entry.Key + Directions[direction];
            if (!patch.TryGetValue(neighborKey, out Chunk neighbor) && !layout.TryGetValue(neighborKey, out neighbor))
                return false;
            if (!rules[direction].Contains(Connection(entry.Value, neighbor, direction)))
                return false;
        }

        foreach (var entry in patch)
            layout[entry.Key] = entry.Value;
        return true;
    }

    private static bool SameTiles(Chunk first, Chunk second)
    {
        if (first == second)
            return true;
        for (int index = 0; index < first.Tiles.Length; index++)
        {
            TileChangeData a = first.Tiles[index];
            TileChangeData b = second.Tiles[index];
            if (a.tile != b.tile || (a.tile != null && (a.color != b.color || a.transform != b.transform)))
                return false;
        }
        return true;
    }

    private static bool IsInsideBoundary(Tilemap ground, PolygonCollider2D boundary, Vector2Int key)
    {
        Vector3Int origin = ChunkOrigin(key);
        Vector2[] points = boundary.points;
        for (int y = -1; y <= ChunkHeight; y++)
        for (int x = -1; x <= ChunkWidth; x++)
        {
            Vector3 world = ground.GetCellCenterWorld(origin + new Vector3Int(x, y, 0));
            Vector2 point = (Vector2)boundary.transform.InverseTransformPoint(world) - boundary.offset;
            if (!ContainsPoint(points, point))
                return false;
        }
        return true;
    }

    private static bool ContainsPoint(Vector2[] polygon, Vector2 point)
    {
        bool inside = false;
        for (int current = 0, previous = polygon.Length - 1; current < polygon.Length; previous = current++)
        {
            Vector2 a = polygon[previous];
            Vector2 b = polygon[current];
            if ((a.y > point.y) == (b.y > point.y))
                continue;
            if (point.x < a.x + (b.x - a.x) * (point.y - a.y) / (b.y - a.y))
                inside = !inside;
        }
        return inside;
    }

    private static int PlaceSpawns(Tilemap ground, PolygonCollider2D boundary, EnemySpawnPoint[] spawns,
        Vector3[] anchors, System.Random random)
    {
        int moved = 0;
        foreach (EnemySpawnPoint spawn in spawns)
        {
            Vector3 originalPosition = spawn.transform.position;
            Vector3Int origin = ground.WorldToCell(originalPosition);
            int depth = Enumerable.Range(1, 3).FirstOrDefault(distance =>
                ground.GetColliderType(origin - Vector3Int.up * distance) != Tile.ColliderType.None);
            if (depth == 0)
                continue;

            Vector3 subCellOffset = originalPosition - ground.GetCellCenterWorld(origin);
            foreach (int horizontal in new[] { -4, -3, -2, 2, 3, 4 }.OrderBy(_ => random.Next()))
            {
                Vector3Int target = origin + Vector3Int.right * horizontal;
                Vector3 position = ground.GetCellCenterWorld(target) + subCellOffset;
                if (!boundary.OverlapPoint(position) ||
                    anchors.Any(anchor => Vector2.Distance(anchor, position) < 2f) ||
                    spawns.Any(other => other != spawn && Vector2.Distance(other.transform.position, position) < 1.5f))
                    continue;

                Vector3Int support = target - Vector3Int.up * depth;
                if (ground.GetColliderType(support) == Tile.ColliderType.None ||
                    Enumerable.Range(0, depth + 1).Any(height => ground.HasTile(target + Vector3Int.up * (1 - height))))
                    continue;

                spawn.transform.position = position;
                moved++;
                break;
            }
        }
        return moved;
    }

    private static Vector2Int ToChunk(Vector3Int cell)
    {
        return new Vector2Int(Mathf.FloorToInt((float)cell.x / ChunkWidth), Mathf.FloorToInt((float)cell.y / ChunkHeight));
    }

    private static Vector3Int ChunkOrigin(Vector2Int key)
    {
        return new Vector3Int(key.x * ChunkWidth, key.y * ChunkHeight, 0);
    }
}
