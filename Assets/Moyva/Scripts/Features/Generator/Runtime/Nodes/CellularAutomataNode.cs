using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Cellular Automata", "Processing", "Запускає клітинний автомат над булевою маскою. Зазвичай використовується для печер, плям лісу, нерівномірних берегів або інших органічних форм, які мають бути менш випадковими та більш зв'язними.")]
    public sealed class CellularAutomataNode : NodeBase
    {
        [Header("Automata Settings")]
        [Tooltip("Скільки поколінь клітинного автомата буде виконано. Більше ітерацій сильніше змінюють початкову маску та формують стабільніші структури.")]
        [SerializeField, Range(1, 20)] private int _iterations = 4;
        [Tooltip("Кількість живих сусідів, необхідна порожній клітинці для народження. Дозволяє контролювати, наскільки активно заповнюються прогалини.")]
        [SerializeField, Range(0, 8)] private int _birthThreshold = 5;
        [Tooltip("Кількість живих сусідів, потрібна вже живій клітинці для виживання. Налаштовує щільність і стійкість сформованих кластерів.")]
        [SerializeField, Range(0, 8)] private int _survivalThreshold = 4;

        public override string Title => "Cellular Automata";
        public override string Category => "Processing";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("InitialState")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Result")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var state = inputs[0] as bool[,];
            if (state == null)
                return NodeOutput.Error("InitialState input is required.");

            int w = state.GetLength(0);
            int h = state.GetLength(1);
            var current = (bool[,])state.Clone();
            var next = new bool[w, h];

            for (int iter = 0; iter < _iterations; iter++)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        int neighbours = CountNeighbours(current, x, y, w, h);
                        next[x, y] = current[x, y]
                            ? neighbours >= _survivalThreshold
                            : neighbours >= _birthThreshold;
                    }
                }

                // Swap buffers
                (current, next) = (next, current);
            }

            return NodeOutput.Success(current);
        }

        private static int CountNeighbours(bool[,] grid, int cx, int cy, int w, int h)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = cx + dx;
                    int ny = cy + dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                    {
                        count++; // Borders count as alive
                    }
                    else if (grid[nx, ny])
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
