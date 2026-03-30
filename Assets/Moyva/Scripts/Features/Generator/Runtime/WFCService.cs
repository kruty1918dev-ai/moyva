using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class WFCService : IWFCService
    {
        private readonly WFCDataSettings _wfcDataSettings;

        private static readonly Vector2Int[] Offsets = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Top
            new Vector2Int(1, 1),   // TopRight
            new Vector2Int(1, 0),   // Right
            new Vector2Int(1, -1),  // BottomRight
            new Vector2Int(0, -1),  // Bottom
            new Vector2Int(-1, -1), // BottomLeft
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(-1, 1)   // TopLeft
        };

        public WFCService(WFCDataSettings wFCDataSettings)
        {
            _wfcDataSettings = wFCDataSettings;
        }

        public void Apply(string[,] biomeMap, float[,] heightMap)
        {
            int width = biomeMap.GetLength(0);
            int height = biomeMap.GetLength(1);
            const string WaterID = "water";

            // ПЕРШИЙ КРОК: Заповнюємо контур водою
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Якщо це будь-яка крайня клітина
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        biomeMap[x, y] = WaterID;
                    }
                }
            }

            // ДРУГИЙ КРОК: Основний цикл ітерацій WFC
            for (int p = 0; p < _wfcDataSettings.PassCount; p++)
            {
                string[,] nextIterationMap = (string[,])biomeMap.Clone();
                bool anyChanges = false;

                // Починаємо з 1 і закінчуємо на length-1, щоб не чіпати вже встановлену воду на краях
                for (int x = 1; x < width - 1; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        string currentTile = biomeMap[x, y];

                        string bestMatch = FindBestMatch(x, y, currentTile, biomeMap, width, height);

                        if (bestMatch != null && bestMatch != currentTile)
                        {
                            nextIterationMap[x, y] = bestMatch;
                            anyChanges = true;
                        }
                    }
                }

                // Перенос змін з буфера в основну мапу
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        biomeMap[x, y] = nextIterationMap[x, y];

                if (!anyChanges) break;
            }
        }

        private string FindBestMatch(int x, int y, string currentTile, string[,] map, int width, int height)
        {
            string bestCandidate = null;
            int maxPriority = -1;

            foreach (var rule in _wfcDataSettings.TileRules)
            {
                // КЛЮЧОВА ЗМІНА: 
                // Правило розглядається тільки якщо TileCentralID збігається з тим, що зараз на мапі
                if (rule.TileCentralID != currentTile) continue;

                if (CheckRule(x, y, map, width, height, rule))
                {
                    if (rule.Priority > maxPriority)
                    {
                        maxPriority = rule.Priority;
                        bestCandidate = rule.TileID;
                    }
                }
            }

            return bestCandidate;
        }

        private bool CheckRule(int x, int y, string[,] map, int width, int height, WFCTileRule rule)
        {
            if (rule.Constraints == null || rule.Constraints.Count == 0) return true;

            int matchedConstraints = 0;
            const string BoundaryTileID = "water"; // ID тайла, який ми вважаємо "безоднею" за межами

            foreach (var constraint in rule.Constraints)
            {
                Vector2Int offset = Offsets[(int)constraint.Direction];
                int nx = x + offset.x;
                int ny = y + offset.y;

                string neighborTile;

                // ПЕРЕВІРКА МЕЖ
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                {
                    // Якщо ми за межами, кажемо, що там "water"
                    neighborTile = BoundaryTileID;
                }
                else
                {
                    neighborTile = map[nx, ny];
                }

                bool isAllowed = false;
                for (int i = 0; i < constraint.AllowedNeighbors.Count; i++)
                {
                    if (constraint.AllowedNeighbors[i] == neighborTile)
                    {
                        isAllowed = true;
                        break;
                    }
                }

                if (isAllowed) matchedConstraints++;
            }

            float matchRate = (float)matchedConstraints / rule.Constraints.Count;
            return matchRate >= rule.MatchThreshold;
        }
    }
}