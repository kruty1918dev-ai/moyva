using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Chokepoint Analyzer", "Analysis", "Аналізує карту на предмет вузьких проходів між непрохідними зонами. Використовується для пошуку стратегічних позицій, оборонних рубежів і потенційних місць для мостів, фортів чи застав.")]
    public sealed class ChokepointAnalyzerNode : NodeBase
    {
        [Header("Analysis Settings")]
        [Tooltip("Мінімальна ширина проходу, яка ще вважається вузьким місцем. Вужчі проходи будуть ігноруватися як занадто тісні або непрохідні.")]
        [SerializeField, Range(1, 10)] private int _passageMinWidth = 2;
        [Tooltip("Максимальна ширина проходу, яку нода ще сприймає як chokepoint. Ширші коридори вважаються звичайною відкритою місцевістю.")]
        [SerializeField, Range(1, 10)] private int _passageMaxWidth = 5;
        [Tooltip("Висота, починаючи з якої клітинка вважається непрохідною через рельєф. Дає змогу трактувати гори або урвища як бар'єри.")]
        [SerializeField, Range(0f, 1f)] private float _impassableMinHeight = 0.7f;
        [Tooltip("Чи вважати воду непрохідною при аналізі проходів. Якщо увімкнено, річки й озера братимуть участь у формуванні вузьких місць.")]
        [SerializeField] private bool _waterIsImpassable = true;

        [Header("Defense Scoring")]
        [Tooltip("Вага переваги висоти у фінальному score оборонної цінності. Чим більше значення, тим сильніше високі позиції підвищують оцінку chokepoint.")]
        [SerializeField, Range(0f, 1f)] private float _heightAdvantageWeight = 0.6f;
        [Tooltip("Вага вузькості проходу у фінальній оцінці. Більше значення робить пріоритетними найвужчі коридори навіть без явної висотної переваги.")]
        [SerializeField, Range(0f, 1f)] private float _narrownessWeight = 0.4f;

        public override string Title => "Chokepoint Analyzer";
        public override string Category => "Analysis";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<bool[,]>("WaterMask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("DefenseScore"),
            PortDefinition.Output<bool[,]>("ChokepointMask"),
            PortDefinition.Output<int[,]>("PassageWidth")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("HeightMap input is required.");

            var waterMask = inputs[1] as bool[,];

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);

            // Build impassable mask
            var impassable = new bool[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    impassable[x, y] = heightMap[x, y] >= _impassableMinHeight
                        || (_waterIsImpassable && waterMask != null && waterMask[x, y]);
                }
            }

            // Compute passage width for each passable cell
            // (minimum distance to nearest impassable terrain in any direction)
            var passageWidth = ComputePassageWidth(impassable, w, h);

            // Identify chokepoints — cells where passage is narrow
            var chokepointMask = new bool[w, h];
            var defenseScore = new float[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (impassable[x, y]) continue;

                    int pw = passageWidth[x, y];
                    if (pw < _passageMinWidth || pw > _passageMaxWidth) continue;

                    // Check if this is a genuine chokepoint
                    // (has impassable terrain on at least 2 opposing sides)
                    if (!HasOpposingBarriers(impassable, x, y, w, h, _passageMaxWidth + 2))
                        continue;

                    chokepointMask[x, y] = true;

                    // Compute defense score
                    float narrowness = 1f - (float)(pw - _passageMinWidth) / (_passageMaxWidth - _passageMinWidth + 1);
                    float heightAdv = ComputeLocalHeightAdvantage(heightMap, x, y, w, h);

                    defenseScore[x, y] = Mathf.Clamp01(
                        narrowness * _narrownessWeight + heightAdv * _heightAdvantageWeight);
                }
            }

            return NodeOutput.Success(defenseScore, chokepointMask, passageWidth);
        }

        private static int[,] ComputePassageWidth(bool[,] impassable, int w, int h)
        {
            // Distance transform: BFS from all impassable cells
            var dist = new int[w, h];
            var queue = new Queue<Vector2Int>();

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (impassable[x, y])
                    {
                        dist[x, y] = 0;
                        queue.Enqueue(new Vector2Int(x, y));
                    }
                    else
                    {
                        dist[x, y] = int.MaxValue;
                    }
                }
            }

            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                for (int d = 0; d < 4; d++)
                {
                    int nx = cell.x + dx[d];
                    int ny = cell.y + dy[d];
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                    int newDist = dist[cell.x, cell.y] + 1;
                    if (newDist < dist[nx, ny])
                    {
                        dist[nx, ny] = newDist;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }

            return dist;
        }

        private static bool HasOpposingBarriers(bool[,] impassable, int cx, int cy,
            int w, int h, int scanDist)
        {
            // Check if there are barriers on at least 2 opposing sides
            bool hasN = false, hasS = false, hasE = false, hasW = false;

            for (int d = 1; d <= scanDist; d++)
            {
                if (cy + d < h && impassable[cx, cy + d]) hasN = true;
                if (cy - d >= 0 && impassable[cx, cy - d]) hasS = true;
                if (cx + d < w && impassable[cx + d, cy]) hasE = true;
                if (cx - d >= 0 && impassable[cx - d, cy]) hasW = true;
            }

            return (hasN && hasS) || (hasE && hasW);
        }

        private static float ComputeLocalHeightAdvantage(float[,] heightMap,
            int cx, int cy, int w, int h)
        {
            // How much higher are neighbors compared to this cell?
            float centerH = heightMap[cx, cy];
            float maxNeighborH = 0f;
            int count = 0;

            int scanR = 3;
            for (int dx = -scanR; dx <= scanR; dx++)
            {
                for (int dy = -scanR; dy <= scanR; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = cx + dx;
                    int ny = cy + dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                    float nh = heightMap[nx, ny];
                    if (nh > maxNeighborH) maxNeighborH = nh;
                    count++;
                }
            }

            // Score: higher ground nearby = better defense
            return Mathf.Clamp01(maxNeighborH - centerH + 0.3f);
        }
    }
}
