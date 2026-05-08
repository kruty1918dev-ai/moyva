using System;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    /// <summary>
    /// Інтерфейс для відображення внутрішнього напрямку бітмаски в напрямок тайла.
    /// Дозволяє змінювати семантику напряму (наприклад, інвертувати для режиму "replace land").
    /// </summary>
    public interface IContourDirectionMapper
    {
        HillDirection Map(HillDirection direction);
    }

    /// <summary>
    /// Повертає напрямок без змін.
    /// </summary>
    public sealed class IdentityContourDirectionMapper : IContourDirectionMapper
    {
        public HillDirection Map(HillDirection direction) => direction;
    }

    /// <summary>
    /// Інвертує напрямок на протилежний (180°). Використовується коли кандидатом є суша,
    /// а нижчий рівень — вода (щоб повторно використати ті самі тайли, створені для legacy-режиму).
    /// </summary>
    public sealed class InvertedContourDirectionMapper : IContourDirectionMapper
    {
        public HillDirection Map(HillDirection dir)
        {
            // Інвертуємо кардинали (180°), а також міняємо зовнішні кути на внутрішні
            // у тій же орієнтації (NE -> InnerNE), і навпаки.
            return dir switch
            {
                HillDirection.North => HillDirection.South,
                HillDirection.South => HillDirection.North,
                HillDirection.East  => HillDirection.West,
                HillDirection.West  => HillDirection.East,

                // Зовнішні кути -> внутрішні кути, обернуті на 180° (NE -> Inner SW тощо)
                HillDirection.CornerNE => HillDirection.InnerCornerSW,
                HillDirection.CornerNW => HillDirection.InnerCornerSE,
                HillDirection.CornerSE => HillDirection.InnerCornerNW,
                HillDirection.CornerSW => HillDirection.InnerCornerNE,

                // Внутрішні кути -> зовнішні кути, обернені на 180°
                HillDirection.InnerCornerNE => HillDirection.CornerSW,
                HillDirection.InnerCornerNW => HillDirection.CornerSE,
                HillDirection.InnerCornerSE => HillDirection.CornerNW,
                HillDirection.InnerCornerSW => HillDirection.CornerNE,

                _ => dir
            };
        }
    }
}
