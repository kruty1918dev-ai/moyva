using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Marketing.Contracts
{
    public enum ShotCategory
    {
        WorldBeauty = 0,
        Settlement = 1,
        Economy = 2,
        Building = 3,
        Army = 4,
        Tactical = 5,
        Combat = 6,
        Exploration = 7,
        Atmosphere = 8,
        UiGameplay = 9,
        EndCard = 10,
    }

    /// <summary>Reusable camera rig archetypes. Rigs are applied to the single
    /// capture camera — never a new GameObject per shot.</summary>
    public enum CameraRigType
    {
        Static = 0,
        WorldReveal = 1,
        TacticalOverview = 2,
        SettlementWide = 3,
        SettlementMedium = 4,
        BuildingHero = 5,
        UnitHero = 6,
        ArmyTrack = 7,
        ArmySideTrack = 8,
        BattleWide = 9,
        BattleMedium = 10,
        BattleClose = 11,
        Follow = 12,
        Dolly = 13,
        Orbit = 14,
        TopDown = 15,
        LowAngle = 16,
        Aftermath = 17,
    }

    public enum ShotTransition
    {
        Cut = 0,
        Dissolve = 1,
        Fade = 2,
        FadeThroughBlack = 3,
        MatchCut = 4,
        Whip = 5,
        LightImpact = 6,
    }

    public enum ShotMotion
    {
        Hold = 0,
        SlowPushIn = 1,
        SlowPullOut = 2,
        LateralTrack = 3,
        Orbit = 4,
        Crane = 5,
        FullOrbit = 6,
    }

    /// <summary>A resolved subject the camera should frame.</summary>
    [Serializable]
    public sealed class ShotSubject
    {
        public string contentId = string.Empty;   // index id, or "world"/"settlement"
        public string instanceId = string.Empty;  // runtime object id when applicable
        public float worldX;
        public float worldY;
        public float worldZ;
        public float approximateRadius = 2f;
    }

    [Serializable]
    public sealed class ShotPlan
    {
        public string shotId = string.Empty;
        public ShotCategory category;
        public CameraRigType rig = CameraRigType.Static;
        public ShotMotion motion = ShotMotion.Hold;
        public ShotTransition transitionIn = ShotTransition.Cut;
        public ShotSubject subject = new ShotSubject();
        public float durationSec = 4f;
        public float motionSpeed = 0.5f;
        public int seed;
        public string beat = string.Empty;         // trailer beat tag, empty for stills
        public string textClaimId = string.Empty;  // optional overlay line
        public float aspectIntent = 16f / 9f;      // framing target aspect
    }

    /// <summary>Planner output: ordered shots plus the reasoning needed to
    /// reproduce or review them.</summary>
    [Serializable]
    public sealed class ShotSequence
    {
        public List<ShotPlan> shots = new List<ShotPlan>();
        public int seed;
        public string recipeId = string.Empty;
    }
}
