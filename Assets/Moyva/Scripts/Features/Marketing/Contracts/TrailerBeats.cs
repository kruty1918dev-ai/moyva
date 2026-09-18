using System;

namespace Kruty1918.Moyva.Marketing.Contracts
{
    public enum TrailerBeat
    {
        Hook = 0,
        World = 1,
        Build = 2,
        Expand = 3,
        Tension = 4,
        Action = 5,
        Climax = 6,
        End = 7,
    }

    [Serializable]
    public sealed class TrailerBeatPlan
    {
        public TrailerBeat beat;
        public float startSec;
        public float endSec;
        public ShotCategory preferredCategory;
        public string textClaimId = string.Empty;

        public float Duration => endSec - startSec;
    }
}
