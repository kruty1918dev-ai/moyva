using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Marketing.Contracts
{
    /// <summary>Live-world subjects discovered after the gameplay scene built
    /// the world. Produced by the runtime scanner; consumed by ShotPlanner.</summary>
    [Serializable]
    public sealed class WorldSubjects
    {
        public List<ShotSubject> units = new List<ShotSubject>();
        public List<ShotSubject> buildings = new List<ShotSubject>();
        public List<ShotSubject> landmarks = new List<ShotSubject>();
        public float worldCenterX;
        public float worldCenterY;
        public float worldCenterZ;
        public float worldRadius = 20f;

        public List<ShotSubject> For(ShotCategory category)
        {
            switch (category)
            {
                case ShotCategory.Army:
                case ShotCategory.Combat:
                case ShotCategory.Tactical:
                    return units;
                case ShotCategory.Building:
                case ShotCategory.Settlement:
                case ShotCategory.Economy:
                    return buildings;
                case ShotCategory.WorldBeauty:
                case ShotCategory.Atmosphere:
                case ShotCategory.Exploration:
                    return landmarks.Count > 0 ? landmarks : buildings;
                default:
                    return buildings;
            }
        }
    }
}
