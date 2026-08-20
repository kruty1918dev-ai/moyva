using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard012
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_02160();
            Case_02161();
            Case_02162();
            Case_02163();
            Case_02164();
            Case_02165();
            Case_02166();
            Case_02167();
            Case_02168();
            Case_02169();
            Case_02170();
            Case_02171();
            Case_02172();
            Case_02173();
            Case_02174();
            Case_02175();
            Case_02176();
            Case_02177();
            Case_02178();
            Case_02179();
            Case_02180();
            Case_02181();
            Case_02182();
            Case_02183();
            Case_02184();
            Case_02185();
            Case_02186();
            Case_02187();
            Case_02188();
            Case_02189();
            Case_02190();
            Case_02191();
            Case_02192();
            Case_02193();
            Case_02194();
            Case_02195();
            Case_02196();
            Case_02197();
            Case_02198();
            Case_02199();
            Case_02200();
            Case_02201();
            Case_02202();
            Case_02203();
            Case_02204();
            Case_02205();
            Case_02206();
            Case_02207();
            Case_02208();
            Case_02209();
            Case_02210();
            Case_02211();
            Case_02212();
            Case_02213();
            Case_02214();
            Case_02215();
            Case_02216();
            Case_02217();
            Case_02218();
            Case_02219();
            Case_02220();
            Case_02221();
            Case_02222();
            Case_02223();
            Case_02224();
            Case_02225();
            Case_02226();
            Case_02227();
            Case_02228();
            Case_02229();
            Case_02230();
            Case_02231();
            Case_02232();
            Case_02233();
            Case_02234();
            Case_02235();
            Case_02236();
            Case_02237();
            Case_02238();
            Case_02239();
            Case_02240();
            Case_02241();
            Case_02242();
            Case_02243();
            Case_02244();
            Case_02245();
            Case_02246();
            Case_02247();
            Case_02248();
            Case_02249();
            Case_02250();
            Case_02251();
            Case_02252();
            Case_02253();
            Case_02254();
            Case_02255();
            Case_02256();
            Case_02257();
            Case_02258();
            Case_02259();
            Case_02260();
            Case_02261();
            Case_02262();
            Case_02263();
            Case_02264();
            Case_02265();
            Case_02266();
            Case_02267();
            Case_02268();
            Case_02269();
            Case_02270();
            Case_02271();
            Case_02272();
            Case_02273();
            Case_02274();
            Case_02275();
            Case_02276();
            Case_02277();
            Case_02278();
            Case_02279();
            Case_02280();
            Case_02281();
            Case_02282();
            Case_02283();
            Case_02284();
            Case_02285();
            Case_02286();
            Case_02287();
            Case_02288();
            Case_02289();
            Case_02290();
            Case_02291();
            Case_02292();
            Case_02293();
            Case_02294();
            Case_02295();
            Case_02296();
            Case_02297();
            Case_02298();
            Case_02299();
            Case_02300();
            Case_02301();
            Case_02302();
            Case_02303();
            Case_02304();
            Case_02305();
            Case_02306();
            Case_02307();
            Case_02308();
            Case_02309();
            Case_02310();
            Case_02311();
            Case_02312();
            Case_02313();
            Case_02314();
            Case_02315();
            Case_02316();
            Case_02317();
            Case_02318();
            Case_02319();
            Case_02320();
            Case_02321();
            Case_02322();
            Case_02323();
            Case_02324();
            Case_02325();
            Case_02326();
            Case_02327();
            Case_02328();
            Case_02329();
            Case_02330();
            Case_02331();
            Case_02332();
            Case_02333();
            Case_02334();
            Case_02335();
            Case_02336();
            Case_02337();
            Case_02338();
            Case_02339();
        }

        private static void Case_02160()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2160,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-6,32,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-4,34,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-20,16,45,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "6fe3a36e4cc0bb43416c1e8c3e1daf26c171015f9842dda6cf8d9c2bbefe6318");
        }

        private static void Case_02161()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2161,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-2,10,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-13,85,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-2,57,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-3,5,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,16,52,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,9,7,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-13,21,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-4,91,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,3,95,19,1), new GeneratedEnemyUnit(-8,11,88,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "33c608f7074f9292dcfd468f8d89e9a549f4957f7c478e9b81e9a41736fb9ee6");
        }

        private static void Case_02162()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2162,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,7,48,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-14,21,41,2), new GeneratedEnemyUnit(11,12,63,46,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "2bce8184f1f5c9b05144ea4579f7f9ce34350a46e7cd3599e0db00179b6e2522");
        }

        private static void Case_02163()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2163,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-12,22,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-9,52,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,11,75,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,13,8,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-3,38,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,11,38,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,16,10,16,1), new GeneratedEnemyUnit(17,-7,37,14,2), new GeneratedEnemyUnit(-4,16,93,44,2), new GeneratedEnemyUnit(2,3,69,50,1), new GeneratedEnemyUnit(-5,18,51,18,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "6c4b7c524470fc76ce319370e5ae2f9dcae5bfa1f2cb7b6f1b163d8c54e07dc6");
        }

        private static void Case_02164()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2164,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-15,58,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-17,30,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,14,97,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,10,56,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-4,45,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,8,81,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,9,70,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-5,16,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,0,45,24,4), new GeneratedEnemyUnit(3,-18,16,10,1), new GeneratedEnemyUnit(17,-13,75,2,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "31df8155c5e90ad2f7e610ab95a20f2d8a855c64fec74dc66b6c44165d7375b6");
        }

        private static void Case_02165()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2165,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,8,48,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,15,14,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-13,42,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,6,95,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-4,55,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-9,73,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,5,52,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,7,23,1,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "fd9730afaa04b2b541a8665dab8cedd305938ee3ccd09a0da8c6b4be5302c9cc");
        }

        private static void Case_02166()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2166,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,19,55,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-11,41,7,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "e138134f6c76f190662f422a4b62d2643945a80338b0172be395c555bbe6cfab");
        }

        private static void Case_02167()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2167,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-5,26,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-2,10,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-15,46,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,1,5,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-18,18,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-5,26,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,20,93,7,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "785a005d7133c794d855ba2596a62528ad3da0dcc0f49ad68d6b0e83b471e92d");
        }

        private static void Case_02168()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2168,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,1,95,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-12,76,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,15,58,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,9,68,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-16,69,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,11,70,28,4), new GeneratedEnemyUnit(-19,-8,14,7,3), new GeneratedEnemyUnit(13,13,15,12,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 85,
                stableHash: "208313f21d4298805dfab99e71ef73be031dc2a0a93226553f1634bfcdd7b81b");
        }

        private static void Case_02169()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2169,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-19,36,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-6,13,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,6,31,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-16,21,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-2,38,8,4), new GeneratedEnemyUnit(2,-12,29,39,3), new GeneratedEnemyUnit(-8,-17,100,35,4), new GeneratedEnemyUnit(1,-8,79,8,4), new GeneratedEnemyUnit(-3,-9,63,41,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "deb186a927428b1e5024e49895868b6f3331ce8c6e40f3925f7b751ee97fa3d4");
        }

        private static void Case_02170()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2170,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,17,55,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-1,19,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-20,27,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,3,15,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,9,15,47,4), new GeneratedEnemyUnit(9,-14,23,50,2), new GeneratedEnemyUnit(-7,-13,40,17,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "c31c5ccf715980c72b5edaef904debc38e6da5d43065b947fbc8da88ca57cb04");
        }

        private static void Case_02171()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2171,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-4,19,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,17,21,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-11,53,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-8,64,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,3,32,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-6,82,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,5,98,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-12,53,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,10,6,34,4), new GeneratedEnemyUnit(-20,5,68,11,1), new GeneratedEnemyUnit(13,14,44,49,3), new GeneratedEnemyUnit(16,-11,69,48,3), new GeneratedEnemyUnit(-14,-5,80,24,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "00a8aa3b17a3434e9041e02e1e8a95f9dfb57bc669da188026313b35a7ae8676");
        }

        private static void Case_02172()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2172,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,5,17,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-4,82,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-8,69,7,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "1ff486169b2752914f0f52fe47f881f07ade0ca19c13c1e4a1edecc5999f3fc9");
        }

        private static void Case_02173()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2173,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,17,27,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-4,16,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-12,52,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-12,24,28,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "fde83f9a9622d5f4d3448ce5eaa65bfa54fffe676a2ae600af1c59e9300a395b");
        }

        private static void Case_02174()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2174,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-8,33,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,7,31,50,3), new GeneratedEnemyUnit(-2,12,80,29,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "14eeecf3a67b0f6879161f1e8e73a874cf7ba6a5a15f67a1193567c0438a1156");
        }

        private static void Case_02175()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2175,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,20,64,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,13,23,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,17,73,30,2), new GeneratedEnemyUnit(18,3,94,2,1), new GeneratedEnemyUnit(19,-14,86,35,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "7593fb8bf8e573d6f23282cced17dffea9bc2e7d177bdc789191459150ff628e");
        }

        private static void Case_02176()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2176,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-15,34,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,6,45,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-6,48,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-11,85,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-15,100,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,14,93,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-2,48,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-12,22,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "91bcb28b89cb1848c54aeecd13e216b557f35a0ba8db9a40247de7e3148a6993");
        }

        private static void Case_02177()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2177,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,14,52,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,15,92,20,3), new GeneratedEnemyUnit(-13,-16,98,37,3), new GeneratedEnemyUnit(12,16,65,18,3), new GeneratedEnemyUnit(-6,-15,52,2,2), new GeneratedEnemyUnit(-17,-15,6,3,1), new GeneratedEnemyUnit(6,-11,85,39,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "ac41ebfe3f46a137b1707e62c3f17afa9007e632e523b9767bdb0432d76b2400");
        }

        private static void Case_02178()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2178,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,17,69,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-12,61,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,4,28,18,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "80bfc526c1867cbfff018d7cfc92818284614dbd25f5548fd4806dd6d3337fe8");
        }

        private static void Case_02179()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2179,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-9,99,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-4,80,8,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "db2b206fbdc4cf4c0f0ea929651dc784b7785864e4a23403bdce550431f8a62a");
        }

        private static void Case_02180()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2180,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,2,97,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,20,41,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-14,39,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-17,19,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-17,78,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,20,78,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,18,47,14,1), new GeneratedEnemyUnit(2,-1,39,3,2), new GeneratedEnemyUnit(8,8,12,5,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "5a42caa184f16897889674b2124e147bed6a781e8503b72f4b37de3b81b859ba");
        }

        private static void Case_02181()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2181,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-15,91,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,0,78,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-17,57,4,2), new GeneratedEnemyUnit(-12,9,52,21,3), new GeneratedEnemyUnit(-5,-20,63,3,4), new GeneratedEnemyUnit(-11,19,60,20,3), new GeneratedEnemyUnit(-20,-10,12,3,1), new GeneratedEnemyUnit(10,12,27,50,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "1acf10969838cc7fa5bad2a5d0a6fa155b1d875b778648ae7cc2bd24ec3e40bb");
        }

        private static void Case_02182()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2182,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,6,79,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,18,38,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-2,28,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,6,67,3,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "5b2e1a8080504c11e539a2c5460a31392832406f32b38c207cb984a1cf8f15d5");
        }

        private static void Case_02183()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2183,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-10,31,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,8,5,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,12,33,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-1,99,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,19,75,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-18,24,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,11,66,11,3), new GeneratedEnemyUnit(-20,4,6,46,3), new GeneratedEnemyUnit(-4,-17,51,11,4), new GeneratedEnemyUnit(-4,5,46,48,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "d5eda0c2f67a59e3414b4cdf79d19522efda326c6e1abcf54fbd848a685f6416");
        }

        private static void Case_02184()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2184,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-5,28,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,13,49,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,19,46,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,5,97,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,19,14,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-4,13,2,4), new GeneratedEnemyUnit(-1,-12,70,49,2), new GeneratedEnemyUnit(19,-7,73,10,1), new GeneratedEnemyUnit(-2,2,55,2,2), new GeneratedEnemyUnit(-1,0,14,8,2), new GeneratedEnemyUnit(17,-14,46,22,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "fcd51f891ab479e3db818bea7f18f9cf845f9833916640f03e7e046115f1b719");
        }

        private static void Case_02185()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2185,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-14,6,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-4,92,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,11,79,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,5,83,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-16,88,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,9,78,29,1), new GeneratedEnemyUnit(-20,15,86,11,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "a1af529dab9c6ec515f41e3bf95113763bab85aa84c5cabddeb5ae71d6137971");
        }

        private static void Case_02186()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2186,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,19,87,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-2,15,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-10,72,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,15,83,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-9,52,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-11,27,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,2,27,5,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "7a0dddd90f2ed2f090e5943d7e57381a5289fc75de5ed65d4d0258da59198a05");
        }

        private static void Case_02187()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2187,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-20,23,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-11,79,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,0,40,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-13,65,3,4), new GeneratedEnemyUnit(-19,18,90,19,1), new GeneratedEnemyUnit(1,-15,52,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "44d270cfd2da89d90bf4a92b3636277703bf144974a9be68069023c05fe295bd");
        }

        private static void Case_02188()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2188,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,2,69,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,15,37,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,6,8,23,4), new GeneratedEnemyUnit(-3,-13,48,32,1), new GeneratedEnemyUnit(-3,11,55,32,1), new GeneratedEnemyUnit(9,16,50,11,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "5fedef6b7009b35af06a2da3d0483d89b67338ae9ab11d27cda375ae70e5078b");
        }

        private static void Case_02189()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2189,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,4,9,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-16,20,33,1), new GeneratedEnemyUnit(3,17,62,45,3), new GeneratedEnemyUnit(11,5,17,43,4), new GeneratedEnemyUnit(0,15,24,13,3), new GeneratedEnemyUnit(6,-5,65,45,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "5bc64931930d527a5928ca67f05ee27a57581bb038f7a34ebd6169a454376c0a");
        }

        private static void Case_02190()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2190,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,2,35,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,1,11,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,0,81,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,2,46,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,3,82,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,10,27,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,6,58,20,1), new GeneratedEnemyUnit(10,9,83,18,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "d0741d0f67917ce8176751b6d936ef94628fd167191c9ee12f993369437651a5");
        }

        private static void Case_02191()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2191,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-5,73,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-20,97,5,1), new GeneratedEnemyUnit(-1,-19,45,44,1), new GeneratedEnemyUnit(-1,-14,66,33,3), new GeneratedEnemyUnit(10,-9,32,21,1), new GeneratedEnemyUnit(0,9,98,9,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "f099855a4b9c278f081aba7b1561ad0073477ad75dfb2e04f4835b01c87e871b");
        }

        private static void Case_02192()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2192,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,10,77,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-13,6,20,1), new GeneratedEnemyUnit(-1,20,23,48,1), new GeneratedEnemyUnit(6,15,29,46,4), new GeneratedEnemyUnit(12,15,36,45,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "d0e5c87f29a738b68f78ed3a3dfbe23f8a697e814f053b08c4c75464eeec29cc");
        }

        private static void Case_02193()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2193,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,7,73,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,15,26,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,1,31,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-13,55,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-10,20,47,1), new GeneratedEnemyUnit(-12,-12,73,31,4), new GeneratedEnemyUnit(1,-5,69,18,2), new GeneratedEnemyUnit(-11,13,82,39,4), new GeneratedEnemyUnit(3,-2,29,18,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "252b9760e8a396f1f75d9dcedf39a0ae30ec21f5260085727bfddeb61a7a0c81");
        }

        private static void Case_02194()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2194,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-19,60,5,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "1da7712a672cade91fd2c97f396a25bc936f24b74b461abaa0eb26c3da5cf2d4");
        }

        private static void Case_02195()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2195,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,17,6,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,1,23,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-3,28,3,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "0280917bc23f198924e58e6a86706d0621e5edeb6232af2343ed7f46a9730a42");
        }

        private static void Case_02196()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2196,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,8,11,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-15,57,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-15,24,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,12,95,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,9,85,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-4,61,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,8,14,3,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c9256b905a854dd998e683e569919a1e9c896781d773d39114ae5b40f51a4d2a");
        }

        private static void Case_02197()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2197,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,1,89,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-13,16,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,12,28,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,4,87,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,0,53,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,13,83,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-9,15,4,2), new GeneratedEnemyUnit(-3,-16,100,6,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "775a0edc9c3d9e12bb9b59d52fe7a14c77a2fa8afd5bc6f288b73631ca2d6e6b");
        }

        private static void Case_02198()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2198,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-10,77,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-5,63,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,7,37,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-7,41,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,5,56,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-1,27,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-14,64,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-4,13,21,3), new GeneratedEnemyUnit(14,-16,49,32,4), new GeneratedEnemyUnit(-4,-16,64,30,2), new GeneratedEnemyUnit(-10,17,63,43,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "2d767c7b00a087fee681484066bfcb1135fdf57c038d7e78668c0d8eefd6bd87");
        }

        private static void Case_02199()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2199,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-14,49,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-12,19,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,19,71,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-10,81,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,2,59,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-12,85,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-5,51,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,12,14,2,2), new GeneratedEnemyUnit(-10,-4,79,38,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "f4a2bed55a1b5295ec08f6584073f2628b56214b41a48dabf584cb67fbd09e6a");
        }

        private static void Case_02200()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2200,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,11,94,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-2,10,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,3,52,24,4), new GeneratedEnemyUnit(5,-12,41,27,4), new GeneratedEnemyUnit(-9,16,8,21,1), new GeneratedEnemyUnit(8,17,7,8,2), new GeneratedEnemyUnit(-19,17,85,43,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "9d67be1462bd48f9272fea0f2370426ca14cbc59799c2c74ed4ff2f58dd7c703");
        }

        private static void Case_02201()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2201,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,7,17,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,12,73,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,12,70,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-9,49,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,13,47,5,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "05e86e7ffd55d243b1344072dfc16c8e855e1d4d59eea24a1d9a654695395618");
        }

        private static void Case_02202()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2202,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,6,40,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,6,83,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-3,64,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,2,41,12,3), new GeneratedEnemyUnit(2,-11,52,10,4), new GeneratedEnemyUnit(3,5,25,40,1), new GeneratedEnemyUnit(-18,15,54,4,2), new GeneratedEnemyUnit(18,15,95,39,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "f3074692577062efffdad5a72f7a6c538814db021ba00b3c34ae27a9675631e3");
        }

        private static void Case_02203()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2203,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,1,56,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,12,48,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-7,17,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,15,52,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-20,23,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,0,87,7,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "c372a912d8914b0ad91b27a63b906ff134c3afad74040268a33888644502aea8");
        }

        private static void Case_02204()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2204,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-16,65,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-19,82,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-1,29,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,10,59,48,1), new GeneratedEnemyUnit(4,-2,7,4,2), new GeneratedEnemyUnit(17,-15,76,49,3), new GeneratedEnemyUnit(-1,15,67,1,1), new GeneratedEnemyUnit(-19,6,84,7,3), new GeneratedEnemyUnit(-7,-17,79,13,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "5789a349a736a6075bec1489983e11716241b2e8293a093119918cfb588c9c1c");
        }

        private static void Case_02205()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2205,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,10,10,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,16,55,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-5,33,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,16,62,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,16,51,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-3,6,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-12,63,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-10,37,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-14,75,30,4), new GeneratedEnemyUnit(0,17,5,45,2), new GeneratedEnemyUnit(1,13,72,33,3), new GeneratedEnemyUnit(-3,12,67,35,4), new GeneratedEnemyUnit(-14,-20,48,43,3), new GeneratedEnemyUnit(-17,8,87,30,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "35705cf8d9321d0d86d744ff595354a9dbda3b08e9775130d13b163259ed706e");
        }

        private static void Case_02206()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2206,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,20,60,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-18,93,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,6,89,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-18,27,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,12,43,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-9,67,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-16,54,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-16,23,3,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "29a910eecf694e5f45fd7d6729c60ae289d1a61414f7a0e429ce4a8c586e950a");
        }

        private static void Case_02207()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2207,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-5,41,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-3,98,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-4,42,26,4), new GeneratedEnemyUnit(-9,17,15,12,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "77102f81957a1431278b12579e9575cfbf287d37deb9a64edab875bfae341eb7");
        }

        private static void Case_02208()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2208,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-8,12,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-10,7,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,8,75,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,14,72,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,20,19,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-17,36,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,16,35,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,1,97,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-1,81,29,2), new GeneratedEnemyUnit(16,-14,78,6,4), new GeneratedEnemyUnit(-11,-12,82,12,2), new GeneratedEnemyUnit(2,18,11,16,2), new GeneratedEnemyUnit(13,-20,12,13,2), new GeneratedEnemyUnit(-17,2,93,48,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "3feae0feaeef71cb0748902448b516b04fe38ac5d3d29afd2a122f9773a8b8a2");
        }

        private static void Case_02209()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2209,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-4,79,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,17,25,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,12,49,20,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "855a07bb793d827768861dabe8d032a17759e6060d64917e135c3796a66f7dfb");
        }

        private static void Case_02210()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2210,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-5,10,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,8,37,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-15,46,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-17,9,10,2), new GeneratedEnemyUnit(-1,5,14,30,3), new GeneratedEnemyUnit(-16,-6,15,42,2), new GeneratedEnemyUnit(4,-2,14,12,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "95cbc7ca4ba9f356a891b80780d1a94107255c2a9e50b2c99746a5e181977c0f");
        }

        private static void Case_02211()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2211,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,3,79,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,10,72,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,11,94,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,17,38,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-19,77,7,4), new GeneratedEnemyUnit(-2,-13,96,18,2), new GeneratedEnemyUnit(-11,-8,30,34,4), new GeneratedEnemyUnit(-17,-20,40,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "0bd6852afb03539fe5ef6e32be4b009714a8f54d581656dd2908381f613062f8");
        }

        private static void Case_02212()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2212,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-6,31,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-8,99,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,9,41,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,10,87,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-7,17,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,10,65,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,14,13,45,2), new GeneratedEnemyUnit(-3,4,34,49,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "0934d37894e42ebbeb39ad0a6ab838b6d8b771d673e319699f4e65eeb5e24650");
        }

        private static void Case_02213()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2213,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,8,93,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,14,28,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,12,22,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,13,17,7,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b7bc294f689478dc441e3586e79a46b3edb5175719802ac28bcc3189b7dfd888");
        }

        private static void Case_02214()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2214,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,10,85,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-20,39,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,0,82,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-15,6,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-12,5,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-5,82,23,4), new GeneratedEnemyUnit(19,-16,44,37,3), new GeneratedEnemyUnit(-13,8,43,13,2), new GeneratedEnemyUnit(-6,-13,35,38,4), new GeneratedEnemyUnit(-17,-7,78,50,4), new GeneratedEnemyUnit(4,9,92,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "3c83b56dbdad08bc06f0e41362c1b4d60112e0cd563eb763a39283eac14605a9");
        }

        private static void Case_02215()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2215,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-5,11,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,11,98,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-16,83,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-13,43,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,20,83,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-14,53,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-10,79,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,8,46,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-2,20,12,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "57d7fc46f36f2c740dd49061863eb85921867b5308a7b546505b8ba16f9db0f4");
        }

        private static void Case_02216()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2216,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,0,19,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,14,35,4,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "f317f61c547e3b3f8a95508f15d939bc47969e050d3e3defa2accd71fc72f902");
        }

        private static void Case_02217()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2217,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,1,67,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,20,72,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,20,46,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-5,78,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-4,11,22,2), new GeneratedEnemyUnit(16,-3,45,7,4), new GeneratedEnemyUnit(12,14,99,41,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2643043cce0f94bcdfe1f5c03eacd5981e6ca7f454666cbd80a0f9b9f1cab9b7");
        }

        private static void Case_02218()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2218,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-11,81,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,20,65,12,1), new GeneratedEnemyUnit(-2,19,46,39,1), new GeneratedEnemyUnit(17,17,32,12,2), new GeneratedEnemyUnit(12,10,73,33,1), new GeneratedEnemyUnit(4,18,55,14,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "33973fa1a79c67d2a929b2e2694671babc8b863fe496a040d52a38ea70395699");
        }

        private static void Case_02219()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2219,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,6,30,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-19,54,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,17,48,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,8,80,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-14,45,8,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "0acfde011e813e078d1c50986fd09a90bc779dea317d4c16c4ed2187b4568549");
        }

        private static void Case_02220()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2220,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-20,99,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-2,78,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-4,43,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-3,19,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,4,48,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,2,9,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,0,79,32,4), new GeneratedEnemyUnit(17,-20,64,40,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "de336763e6d0245a781f2829f0651d89f0834c1a42bf9b4bf7b2f50134f58397");
        }

        private static void Case_02221()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2221,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-5,13,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-17,7,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,3,38,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,18,95,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-2,59,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,5,63,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,12,45,44,3), new GeneratedEnemyUnit(18,3,48,22,4), new GeneratedEnemyUnit(10,16,9,30,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9d70b9b0e9e4f48b862ec3611a0eb995e9ab527d4585d09142a7a5879f67cf93");
        }

        private static void Case_02222()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2222,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-7,37,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,4,78,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-4,16,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-7,76,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-15,22,30,1), new GeneratedEnemyUnit(-18,9,11,39,1), new GeneratedEnemyUnit(-9,5,53,32,2), new GeneratedEnemyUnit(-3,2,77,15,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "8deb401be00eebd8d3ce98088c55202b19984ac6f719ecef8e418ea94fa66c87");
        }

        private static void Case_02223()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2223,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,14,38,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,20,66,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-13,100,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,20,68,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-3,11,48,3), new GeneratedEnemyUnit(9,3,55,26,2), new GeneratedEnemyUnit(7,3,71,37,4), new GeneratedEnemyUnit(18,-1,93,29,1), new GeneratedEnemyUnit(-9,-6,35,39,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "feacc95d22ed4a4ad84060f8ade0b70ed268f57f210263da2785db6fee4e2803");
        }

        private static void Case_02224()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2224,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,3,65,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,2,64,47,3), new GeneratedEnemyUnit(0,-1,51,44,1), new GeneratedEnemyUnit(-9,-11,50,1,3), new GeneratedEnemyUnit(18,-14,11,49,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "638bbc535f6c0809d38c3fb3c13774ddb16e6f6da96a435afcbb393d08103acf");
        }

        private static void Case_02225()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2225,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,12,14,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-3,29,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,12,86,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-14,48,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,12,11,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,10,80,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,5,12,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,0,35,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,18,41,42,1), new GeneratedEnemyUnit(-9,20,45,6,2), new GeneratedEnemyUnit(3,-6,38,11,2), new GeneratedEnemyUnit(20,11,28,4,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "2402434aa54001cda49a60051649532739198026bf60ee0e3bddc92b7fc52801");
        }

        private static void Case_02226()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2226,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-2,33,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,11,56,6,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "cc31d68802ac453f3df9399f12a09afcf6f423ba839c9f7c6b601d85e226db75");
        }

        private static void Case_02227()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2227,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-20,33,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-2,61,5,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "d6dc77cbbc666605cec2ebdca745bf4d445dc80722d8731f29bf963160b9b13e");
        }

        private static void Case_02228()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2228,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-19,51,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-3,57,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-14,39,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-10,45,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-19,10,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,4,16,23,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "4a5f326cab15e8003eb71ef2eb8d7fc4b3232a531ff6e04c321804219cb2486f");
        }

        private static void Case_02229()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2229,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-3,14,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-5,12,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-17,84,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,4,19,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-20,14,42,2), new GeneratedEnemyUnit(-17,-5,33,10,3), new GeneratedEnemyUnit(17,9,11,5,2), new GeneratedEnemyUnit(9,-6,54,8,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "abe99783a01ff70a467ad79abddcb5f5492a1edaaffb57d1df5d8d85d7eeb9f4");
        }

        private static void Case_02230()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2230,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-8,80,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,10,37,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,15,83,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-4,86,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,4,85,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,18,11,4,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "9efefcc4fe3936f4ed0cbedebfcc7b0903763290cb576e0cbab01d574c425fc7");
        }

        private static void Case_02231()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2231,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-15,22,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-10,40,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-20,74,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-7,44,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-8,72,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,2,63,12,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0fcf975cf5d48698e5df9b2325b03b8162ac0c92cc7131e7e61146f138d289a8");
        }

        private static void Case_02232()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2232,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-13,15,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,14,8,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,16,89,7,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "27f9f6b495141d1091cd5ef3567f689fb608dcf9426c654884fb69dc1f39455f");
        }

        private static void Case_02233()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2233,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,10,99,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,12,63,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-14,9,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,2,40,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-4,54,6,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "fd6a423e4b95271c053c088d3a34182304d2e78dd770a3a7659e0466fea0733f");
        }

        private static void Case_02234()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2234,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-19,100,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,4,77,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,10,35,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,17,37,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,13,58,48,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "61bcf5c970e5a8b0217468269ea88ac3b8857379cbc167b91ec37b3f4557a56a");
        }

        private static void Case_02235()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2235,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,5,74,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-16,51,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,0,56,35,3), new GeneratedEnemyUnit(19,-13,41,12,3), new GeneratedEnemyUnit(-8,15,22,2,3), new GeneratedEnemyUnit(-3,15,32,25,3), new GeneratedEnemyUnit(-16,8,52,1,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "45119eb3d66daf1f6bb01f22e1a272687c77f4350a8113459810e2ce5d2ad8d8");
        }

        private static void Case_02236()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2236,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-16,70,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-11,86,42,1), new GeneratedEnemyUnit(4,1,51,4,2), new GeneratedEnemyUnit(-10,11,84,47,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "d83a8de23c6a48ba963cc904e4146d0b6d5a18b613e7ef07d9a7a63df90645b8");
        }

        private static void Case_02237()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2237,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-18,44,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-16,90,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,14,59,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,8,98,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-11,89,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-14,43,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,12,40,1,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "6c2cdbbfee36c7c03e071ec8b07da4cd9ac649bce54fae0cbc925d1e40c648c0");
        }

        private static void Case_02238()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2238,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-3,19,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,10,43,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,6,50,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-13,86,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,18,25,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-19,51,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-17,55,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,12,48,44,4), new GeneratedEnemyUnit(2,-16,51,34,4), new GeneratedEnemyUnit(16,-2,89,31,4), new GeneratedEnemyUnit(16,2,28,26,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "a2254adbbcebf416a78622ccaf1cfbe4bddfd09a05011682beb754c87df95178");
        }

        private static void Case_02239()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2239,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,6,29,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-6,10,3,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3b5b95aa49958aa70b701a31a4868724acbbe947486eed8c4224fe23851c6faf");
        }

        private static void Case_02240()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2240,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,12,79,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,11,78,8,4), new GeneratedEnemyUnit(-8,-15,23,46,2), new GeneratedEnemyUnit(-9,19,50,22,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "6b706187cbac4c212ec5aaca75a46dfc752b63f6b555748b1f9afd5e136f5f5a");
        }

        private static void Case_02241()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2241,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,7,31,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,13,16,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,0,13,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,19,9,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-14,40,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,19,59,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,19,31,17,1), new GeneratedEnemyUnit(12,4,98,10,2), new GeneratedEnemyUnit(19,19,16,14,1), new GeneratedEnemyUnit(-20,4,99,10,1), new GeneratedEnemyUnit(7,-16,13,31,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "b1bc36d35499549724acfb47345c61fc60cd949951edd00b681df99342f042e9");
        }

        private static void Case_02242()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2242,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-20,91,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-9,68,3,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "1189cfbec38a090580ca08adb73fae8628afcf362b88cca5311c65d60ff9f97f");
        }

        private static void Case_02243()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2243,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,0,57,5,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "5a8c6be7422a8d36e9918c94f590c7c4895040d566dc44b747d062ededef96f1");
        }

        private static void Case_02244()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2244,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,12,32,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,9,59,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,13,5,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,8,20,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,7,61,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-3,100,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-2,13,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-4,84,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,8,94,9,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "a935cd06ff0a7d6c65c539c461897b3081179636b291c20905bf2b69034a8c06");
        }

        private static void Case_02245()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2245,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,9,26,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-9,46,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,10,89,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,9,100,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-20,96,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,8,43,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,20,77,39,3), new GeneratedEnemyUnit(-4,-7,54,20,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "e232584e7633b2ec910a26404d057f6be1be24a64855f2c894cd687c9aef7e18");
        }

        private static void Case_02246()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2246,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-5,78,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-10,89,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-16,57,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-13,21,1,2), new GeneratedEnemyUnit(-3,6,86,30,1), new GeneratedEnemyUnit(-19,-13,23,7,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "cdb775f2f47be32c708f7839a5da9802b7f0ba31dc11e22da9544bc734178255");
        }

        private static void Case_02247()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2247,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,15,12,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,7,51,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,1,69,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-11,84,31,1), new GeneratedEnemyUnit(7,1,44,26,2), new GeneratedEnemyUnit(0,4,43,21,1), new GeneratedEnemyUnit(-8,-8,26,49,4), new GeneratedEnemyUnit(-14,-15,13,45,4), new GeneratedEnemyUnit(-6,13,98,9,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "1988add457afefe947032fd3ad075d862db3f84e30f9857ca0bec9f9dfef316f");
        }

        private static void Case_02248()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2248,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,2,49,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-14,31,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-2,12,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,9,37,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,6,97,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-15,52,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-14,28,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-5,79,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-18,37,29,1), new GeneratedEnemyUnit(-3,-19,40,43,4), new GeneratedEnemyUnit(14,-15,83,3,4), new GeneratedEnemyUnit(11,19,54,15,4), new GeneratedEnemyUnit(19,-5,96,3,3), new GeneratedEnemyUnit(-1,-2,37,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "bbb9a41a4dce52d01927978c4f14b96ee8e4b3c9eaa7cecaa9a6006889f8d328");
        }

        private static void Case_02249()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2249,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,17,96,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,11,28,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-9,18,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,0,38,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-20,23,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,3,14,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-15,93,18,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "1357c2bc038155367352af9ac65a654939a1d3b36dca575dbb1652e3e7fc631c");
        }

        private static void Case_02250()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2250,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,13,38,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-11,95,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-13,73,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-14,28,1,2), new GeneratedEnemyUnit(13,10,35,46,3), new GeneratedEnemyUnit(-6,-3,10,14,4), new GeneratedEnemyUnit(4,-5,86,43,4), new GeneratedEnemyUnit(-5,18,8,33,2), new GeneratedEnemyUnit(-5,2,21,16,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "fd5f70fba8caab0d59bb1be2b528c948fc1357ae88695809628bc89ae7ecb8d9");
        }

        private static void Case_02251()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2251,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,20,82,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-4,46,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-4,13,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-6,54,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-9,8,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-5,17,6,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2680f2af6fa586974567e1aea54224e6ea0f97486851820630ffb1e7102ca949");
        }

        private static void Case_02252()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2252,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,2,62,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-7,53,6,2), new GeneratedEnemyUnit(-15,-12,84,37,3), new GeneratedEnemyUnit(-5,3,8,29,4), new GeneratedEnemyUnit(-2,18,7,40,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "46aaf246c0e85682c25025cdd93ca234a4118be81b4a34b6768fb9dff5cf43dc");
        }

        private static void Case_02253()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2253,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-8,12,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,12,91,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,9,41,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,6,42,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,15,21,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-19,7,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-11,25,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-17,18,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-17,66,14,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2b078412d32e426d44c609ea5f5ef7b9ae47c18e09e84cc368ed53f2417ae807");
        }

        private static void Case_02254()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2254,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,5,15,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-4,44,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,20,36,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-20,94,39,4), new GeneratedEnemyUnit(15,-5,9,45,1), new GeneratedEnemyUnit(5,15,85,50,1), new GeneratedEnemyUnit(-13,3,30,23,4), new GeneratedEnemyUnit(17,-13,64,7,1), new GeneratedEnemyUnit(-2,-2,7,31,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "3506406903bc87b15bb237d3134c9e3bc845591b59f40a2d7be558851643942f");
        }

        private static void Case_02255()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2255,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-4,20,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-11,93,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-10,89,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,5,41,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,18,100,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-18,48,45,1), new GeneratedEnemyUnit(-4,-7,41,13,3), new GeneratedEnemyUnit(14,-6,35,9,1), new GeneratedEnemyUnit(7,10,95,39,3), new GeneratedEnemyUnit(-15,5,42,30,1), new GeneratedEnemyUnit(11,-4,94,8,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "d2a4f16f697132eb03dc2b4c7f841abb0ab9e0e5f20f535fb913e7cedcff9de7");
        }

        private static void Case_02256()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2256,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,16,28,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-6,20,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,15,30,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,16,78,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,12,42,29,4), new GeneratedEnemyUnit(-11,-3,48,21,2), new GeneratedEnemyUnit(3,2,69,4,4), new GeneratedEnemyUnit(14,-18,8,9,4), new GeneratedEnemyUnit(-4,8,97,37,3), new GeneratedEnemyUnit(2,1,73,29,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "b42590896ffd1ea68360f21408f5ef735149e39f0ef07de6c286da42f1213a17");
        }

        private static void Case_02257()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2257,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-4,15,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-13,10,22,3), new GeneratedEnemyUnit(10,18,24,19,3), new GeneratedEnemyUnit(11,10,94,28,3), new GeneratedEnemyUnit(6,-17,51,44,4), new GeneratedEnemyUnit(-16,-11,94,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "dc40ce5ea7dbb83c5a04f5a8bb5c7bf9a0320fc9a40356d5ddcacffd16ea6dec");
        }

        private static void Case_02258()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2258,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,19,91,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,8,44,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,3,60,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,9,71,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-8,43,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-2,53,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-6,94,47,3), new GeneratedEnemyUnit(-16,12,30,21,1), new GeneratedEnemyUnit(-2,-13,35,27,1), new GeneratedEnemyUnit(14,-15,29,5,1), new GeneratedEnemyUnit(-16,-8,99,28,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "de9277187f164c7b77076f7042c1bf1c95e0a5d00ccfe43f91c334be61172c2b");
        }

        private static void Case_02259()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2259,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-12,34,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-6,84,50,4), new GeneratedEnemyUnit(15,-4,66,21,4), new GeneratedEnemyUnit(19,15,35,23,3), new GeneratedEnemyUnit(-16,13,60,38,3), new GeneratedEnemyUnit(9,0,96,27,4), new GeneratedEnemyUnit(3,-11,63,33,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "4816fa0861a2e3073360a6095fe51736f44972e30d7af193e7599a2decfba39d");
        }

        private static void Case_02260()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2260,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,14,100,7,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "c871bd9e5129af2874ba0c2701d4d58cd71303e426fe80a1763ccdbd7903fc8a");
        }

        private static void Case_02261()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2261,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-10,71,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-19,67,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-9,36,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,20,87,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-19,11,24,1), new GeneratedEnemyUnit(-13,3,47,28,2), new GeneratedEnemyUnit(-9,-15,15,47,2), new GeneratedEnemyUnit(-19,-17,57,44,2), new GeneratedEnemyUnit(-19,-2,53,9,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "f538f0d5641469a1cb036b34d8ed5f13c5583554a79312c6bdd0ed67971404ab");
        }

        private static void Case_02262()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2262,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-4,14,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,0,86,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,0,74,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-8,55,38,2), new GeneratedEnemyUnit(4,1,20,28,3), new GeneratedEnemyUnit(16,0,21,30,1), new GeneratedEnemyUnit(2,15,16,29,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "a0ae9788b0cce8aec77ab3a7e099b27b20a95b29bc79a2480dbbc60e51c89666");
        }

        private static void Case_02263()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2263,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,18,30,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-6,69,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,19,12,14,3), new GeneratedEnemyUnit(19,-8,23,41,1), new GeneratedEnemyUnit(17,16,54,50,3), new GeneratedEnemyUnit(-8,-7,23,43,1), new GeneratedEnemyUnit(-14,7,25,10,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "3fa78b52bd498d8ae0da88f8b0f1e145fb7b1a8cad8dd018d02c555cecd53241");
        }

        private static void Case_02264()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2264,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-2,19,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-9,71,3,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "71768fed8e2e647faa36b246890650435a0072124f6ac366cc56b9c556b7e518");
        }

        private static void Case_02265()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2265,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-14,89,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-7,32,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,7,37,14,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "529318597eab80e601f4aec20f1f36343bd103706e623b945af63957597a65db");
        }

        private static void Case_02266()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2266,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,20,18,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,8,76,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,11,59,35,1), new GeneratedEnemyUnit(19,-1,55,14,4), new GeneratedEnemyUnit(14,-18,56,5,4), new GeneratedEnemyUnit(6,-13,39,9,1), new GeneratedEnemyUnit(4,-2,40,26,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "6f4fa13374f5bfb221719ac6695aa2bccb9add3f9eeb4b35bbe77018d5efab5c");
        }

        private static void Case_02267()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2267,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,3,68,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-14,19,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,11,52,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-20,19,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,11,7,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-10,67,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,12,89,2,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "f5b265b5b1703c8a7c39e1961c3cdb3f967dd2362732a0603538477d55a426e5");
        }

        private static void Case_02268()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2268,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-18,31,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,0,97,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,8,55,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-11,35,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,17,99,12,4), new GeneratedEnemyUnit(3,-17,36,8,3), new GeneratedEnemyUnit(-7,-4,63,25,4), new GeneratedEnemyUnit(-15,-6,20,39,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "cffe1dc03d0062f182d45fae30d191b8f7091660d509277ee95422de80feb45a");
        }

        private static void Case_02269()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2269,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,8,22,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-16,97,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,4,27,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-11,19,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-20,46,42,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "3de5fcd29816fe08b721aad7f6f5862a57e7c8d89320a79953cf100759c33935");
        }

        private static void Case_02270()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2270,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,16,84,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,9,88,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,20,63,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,13,25,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,11,95,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,20,25,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,17,100,15,1), new GeneratedEnemyUnit(-2,-17,58,26,4), new GeneratedEnemyUnit(2,-2,21,46,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "849fcb25a730b85e6a453c9a104d4da2fb9f2d93986841663e27e2c7cf358360");
        }

        private static void Case_02271()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2271,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,20,51,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,1,32,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,14,67,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-20,85,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-11,25,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-8,51,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,20,44,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-20,32,39,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "0d2588e0c74aa295b28f5cf60347118accdb6f99bc212f8974876e4d5e5b5357");
        }

        private static void Case_02272()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2272,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-1,48,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,7,40,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-16,69,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,18,23,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,4,71,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,1,57,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,16,80,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-9,76,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-13,73,26,4), new GeneratedEnemyUnit(12,20,13,12,4), new GeneratedEnemyUnit(18,20,16,13,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "6ae91ecc64b0a655604dc2942e5caeb22ee039b53ae6a8d507be7c28d196674a");
        }

        private static void Case_02273()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2273,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-12,5,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,6,79,21,2), new GeneratedEnemyUnit(-2,1,23,40,2), new GeneratedEnemyUnit(6,10,21,39,3), new GeneratedEnemyUnit(16,20,8,34,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "9d45ceed56c983f392953847727a2c9f308dc2a75fe1f24a3a06bdc4b3151d13");
        }

        private static void Case_02274()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2274,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,16,64,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-4,34,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-14,84,40,4), new GeneratedEnemyUnit(11,-5,55,9,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "87f0dacfa2ae37e0e6776c05900cbbe17d794c913c15ea80fd0ac3fd3a96a060");
        }

        private static void Case_02275()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2275,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,9,22,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,18,70,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-10,30,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,16,98,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-8,12,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,8,90,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-20,69,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-13,46,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-7,94,45,2), new GeneratedEnemyUnit(-11,3,61,23,2), new GeneratedEnemyUnit(6,-3,73,37,3), new GeneratedEnemyUnit(10,2,94,5,3), new GeneratedEnemyUnit(-12,14,62,9,2), new GeneratedEnemyUnit(-17,3,33,4,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "d604696ac1e119b60b35801871ae0d872aee60823a20dca98f78274de4f2d987");
        }

        private static void Case_02276()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2276,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-17,54,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,9,16,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,9,88,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,20,69,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,14,60,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,6,83,49,2), new GeneratedEnemyUnit(-18,-12,27,1,3), new GeneratedEnemyUnit(-9,15,55,9,4), new GeneratedEnemyUnit(6,-1,58,27,4), new GeneratedEnemyUnit(-17,10,83,21,4), new GeneratedEnemyUnit(11,2,26,37,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 35,
                stableHash: "877ca6c50391b8caadbbd7afe2dec8bb4c3ac5dae3f183e4c5b90157ebc5d480");
        }

        private static void Case_02277()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2277,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-20,88,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-15,97,50,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8e4b5b30e463bdd454b3e7ace5ead963f59c88a1a83332f73c85b106d3ae50d4");
        }

        private static void Case_02278()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2278,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-19,94,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-18,67,17,4), new GeneratedEnemyUnit(-3,13,43,43,2), new GeneratedEnemyUnit(20,13,74,19,3), new GeneratedEnemyUnit(-18,-1,16,46,4), new GeneratedEnemyUnit(-6,17,86,22,2), new GeneratedEnemyUnit(5,-20,25,1,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "abddfd198df10e3c8c05a4fe89b0f25a96c28966305083ce939463957d378a27");
        }

        private static void Case_02279()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2279,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,4,95,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,7,55,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,14,14,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-1,69,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-2,62,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,11,50,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,8,51,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-3,79,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-13,7,10,4), new GeneratedEnemyUnit(-5,9,86,26,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "75021b7d0d25f4e65b64a27ff72c15af230cf3ef96509361b5ed68cd7789e643");
        }

        private static void Case_02280()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2280,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,8,87,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-20,54,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,6,22,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,17,26,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,12,55,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,12,91,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-12,39,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-3,7,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,9,55,46,1), new GeneratedEnemyUnit(3,-16,26,7,1), new GeneratedEnemyUnit(19,18,93,41,1), new GeneratedEnemyUnit(-19,-20,79,19,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "90c6bdaa65fd971aa73d3d546c5156181965b41901f779b81217191e4fffc098");
        }

        private static void Case_02281()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2281,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,3,79,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-17,54,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-5,15,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,5,42,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,8,62,42,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "23d8236dac6bb214df2c6a99076d6bb3e73f6188f329b84c677d526e5c62efcd");
        }

        private static void Case_02282()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2282,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-5,38,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,3,87,4,4), new GeneratedEnemyUnit(19,10,35,45,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "36b85b5f717b4c2187cb845ffc4ad98760675dcef88fbb2e9c88f063119abc9c");
        }

        private static void Case_02283()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2283,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-14,21,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-19,68,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,17,5,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,12,97,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-12,67,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,0,25,16,2), new GeneratedEnemyUnit(8,18,31,9,2), new GeneratedEnemyUnit(-8,-1,88,19,3), new GeneratedEnemyUnit(-17,1,88,40,3), new GeneratedEnemyUnit(-14,19,20,18,3), new GeneratedEnemyUnit(-12,5,71,2,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "c59e03c35eccb8364a688fdb93d65e1f16e01125ac05553a74c682e4bd3bb70f");
        }

        private static void Case_02284()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2284,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,7,30,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,2,87,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-16,77,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,8,50,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,6,45,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-19,16,43,1), new GeneratedEnemyUnit(18,8,45,13,3), new GeneratedEnemyUnit(0,1,26,24,1), new GeneratedEnemyUnit(-15,19,68,36,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a7f825d77318650fa7935300bf888e2306c15a0c7027f441ae8e90133e9820c7");
        }

        private static void Case_02285()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2285,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-2,56,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,5,75,29,2), new GeneratedEnemyUnit(14,19,89,39,1), new GeneratedEnemyUnit(19,7,78,49,2), new GeneratedEnemyUnit(14,-1,85,1,1), new GeneratedEnemyUnit(-1,18,15,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "33a166c8a261d0417508a97b764e27a2f065b167bf80251ba1041f0d238eae0e");
        }

        private static void Case_02286()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2286,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-9,94,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,1,45,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,2,9,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,4,14,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,17,64,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,10,42,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-11,31,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,2,65,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-1,42,24,2), new GeneratedEnemyUnit(-16,-8,13,31,4), new GeneratedEnemyUnit(8,-11,7,24,2), new GeneratedEnemyUnit(18,1,99,1,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "a74d6d76d48dab336a996736f76a9607417563343e15e9137f5af8fa550c2fb3");
        }

        private static void Case_02287()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2287,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,2,47,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-15,11,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-1,44,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-19,89,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-12,10,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,13,33,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-7,89,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-3,46,7,4), new GeneratedEnemyUnit(-12,-20,53,28,2), new GeneratedEnemyUnit(12,4,93,20,3), new GeneratedEnemyUnit(-7,-13,85,9,3), new GeneratedEnemyUnit(6,-7,89,30,1), new GeneratedEnemyUnit(-2,3,96,42,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "ede5c166267a04ff2b8d5dd146689ccbb6ae998223050ff593a8a053529fea63");
        }

        private static void Case_02288()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2288,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-20,20,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,1,84,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-16,29,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,11,85,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-10,69,26,4), new GeneratedEnemyUnit(-3,-17,12,32,2), new GeneratedEnemyUnit(-6,7,42,1,4), new GeneratedEnemyUnit(-10,8,100,43,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "8c38842e300467d86891c14176b49b4d6cee378b3d9521df384ca39d1cc73280");
        }

        private static void Case_02289()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2289,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,13,59,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-2,19,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-19,27,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-8,97,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,0,6,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-1,35,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-12,37,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,17,97,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-14,87,1,1), new GeneratedEnemyUnit(-18,-5,88,21,3), new GeneratedEnemyUnit(-16,5,49,50,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "3e34ce20b596cd5716fac34df03644135b471b1fc620191163f2111a736de948");
        }

        private static void Case_02290()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2290,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-4,25,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,15,34,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-17,90,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,12,13,2,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "dcf16a7dd2a303774b56fb7a779e438cea75ec5b53465efdbdeb0b2e8fdbff0a");
        }

        private static void Case_02291()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2291,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,20,32,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-6,16,1,4), new GeneratedEnemyUnit(17,20,64,30,4), new GeneratedEnemyUnit(-20,7,70,1,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "0ce50090f2638bce287e491fca350e4155ecdd09dad5dcea18d2b6b051d6c91d");
        }

        private static void Case_02292()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2292,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-18,40,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-5,20,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-10,96,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,19,44,41,3), new GeneratedEnemyUnit(19,0,10,49,2), new GeneratedEnemyUnit(8,5,11,38,2), new GeneratedEnemyUnit(-6,18,34,41,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "dea5f88e48dab351574f5b979a82a19a28d2b11435dcd6422519aa299ceb7a65");
        }

        private static void Case_02293()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2293,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,8,19,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-5,82,1,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "a3eb421db8335e68187b7d5332ad745fcba4a38b462bb1d86ff3e49762ad5d79");
        }

        private static void Case_02294()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2294,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-20,81,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,19,97,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-13,63,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-11,94,2,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "ce7e01f5810540f82a3669591bcfd0159994da405120ac9f303f747aef60de89");
        }

        private static void Case_02295()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2295,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,5,26,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,12,39,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,8,47,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-9,51,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,13,76,20,4), new GeneratedEnemyUnit(-9,-13,54,14,4), new GeneratedEnemyUnit(11,4,63,34,2), new GeneratedEnemyUnit(16,-10,45,15,1), new GeneratedEnemyUnit(-2,17,59,32,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "45d41c8a67371eb1f7cdb85dcbf94b77e94036ead783990b1a135fddea87c8dd");
        }

        private static void Case_02296()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2296,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-3,67,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-5,27,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-18,63,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-11,54,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-16,42,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-10,86,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,2,98,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,6,45,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-17,8,10,4), new GeneratedEnemyUnit(-14,20,29,37,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "c95dc07da1bb716d3b0deed6bff05b5cff919c092dd2e368ad7302c631596ff5");
        }

        private static void Case_02297()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2297,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,3,35,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-18,56,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-4,21,50,2), new GeneratedEnemyUnit(14,-20,22,29,1), new GeneratedEnemyUnit(-11,-1,44,31,4), new GeneratedEnemyUnit(-11,7,87,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "24efec3c4672c6a388694518b1777741116876f1ee58946c15fbf49f343b5224");
        }

        private static void Case_02298()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2298,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,10,44,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,0,70,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,7,17,27,2), new GeneratedEnemyUnit(-13,20,67,29,3), new GeneratedEnemyUnit(-18,2,78,34,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "d35e98299da197f5634cb9863da33b1e20c3e31e7c97d554c986a6b6fab30ccc");
        }

        private static void Case_02299()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2299,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-18,66,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,17,21,31,1), new GeneratedEnemyUnit(-20,-1,6,11,3), new GeneratedEnemyUnit(10,-16,9,1,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "4d64f1ad810828140d24232ce32799a48074f7837cbe798b87bf69027639cd0a");
        }

        private static void Case_02300()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2300,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-3,33,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,6,87,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-19,38,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,9,48,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-12,53,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,5,28,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,1,92,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,4,65,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-18,7,33,4), new GeneratedEnemyUnit(-15,-8,62,5,2), new GeneratedEnemyUnit(13,10,92,23,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "1faa47942f0fa8c430e99ef2b45f47a6f9690c1881b8b52c38958c2d080667ee");
        }

        private static void Case_02301()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2301,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,11,54,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-14,55,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,8,85,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-19,79,35,4), new GeneratedEnemyUnit(20,8,50,43,4), new GeneratedEnemyUnit(12,2,93,45,4), new GeneratedEnemyUnit(13,-6,11,12,1), new GeneratedEnemyUnit(-3,1,83,42,2), new GeneratedEnemyUnit(20,-15,20,10,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "d038c86c4f5c674c204583cab3d7128bc20b621ff6ea8ff66ba4065e2fd41a79");
        }

        private static void Case_02302()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2302,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-20,23,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,20,45,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,17,84,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-20,97,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,3,68,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,5,95,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-19,87,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-12,98,40,3), new GeneratedEnemyUnit(-8,-20,69,31,1), new GeneratedEnemyUnit(-15,0,5,1,1), new GeneratedEnemyUnit(-17,10,98,34,3), new GeneratedEnemyUnit(-17,-4,84,49,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "5fbbd2b72437c25d6e2b93660464e9eaff00e54bd94bf45f99b06a0f6efac546");
        }

        private static void Case_02303()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2303,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,17,37,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,3,32,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,17,76,4,1), new GeneratedEnemyUnit(7,2,22,37,2), new GeneratedEnemyUnit(-20,-19,76,32,1), new GeneratedEnemyUnit(-12,-14,42,25,2), new GeneratedEnemyUnit(-15,3,11,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "f0ddb524390276ecc63863e4f9e8c751478b7b2283c8e2b8afedaf5bc2c75732");
        }

        private static void Case_02304()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2304,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-14,21,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,10,21,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,18,45,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-14,47,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-1,90,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,16,85,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-14,71,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,6,77,48,1), new GeneratedEnemyUnit(13,-8,74,4,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "c8a05fcd900a77027bfdd16ec24fd67cda8fc81f2bd6f82a80a954e5bb289226");
        }

        private static void Case_02305()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2305,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,0,86,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,9,78,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-7,65,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-20,33,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,7,5,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-3,46,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,6,31,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-5,93,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-19,40,13,3), new GeneratedEnemyUnit(-11,-17,36,8,3), new GeneratedEnemyUnit(14,5,86,48,3), new GeneratedEnemyUnit(-8,1,94,40,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "644f65c7b7c66db7e829ca79eaf2414786a46f6272849f2979231e5cf36d0928");
        }

        private static void Case_02306()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2306,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-1,89,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,2,47,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,15,46,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-12,11,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-20,13,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,3,41,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,5,67,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,7,81,16,1), new GeneratedEnemyUnit(-14,-18,87,15,2), new GeneratedEnemyUnit(19,-5,55,29,4), new GeneratedEnemyUnit(-14,-2,48,23,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "7fad1c962295b0c332a470e7cad7289a5ff59ddb1dbd06d02ceb559682b6ee03");
        }

        private static void Case_02307()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2307,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-1,34,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,0,81,25,1), new GeneratedEnemyUnit(-2,8,60,16,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "6c129cbdce2df19f93eef32a5f6b8cefc87727af3c7e8220889e88fcfb28da80");
        }

        private static void Case_02308()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2308,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-9,86,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-18,52,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,5,17,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,7,51,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,8,8,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,10,9,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-20,33,48,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "236b7e6f594780cd04c730592b04759021f11d021591df8ee7c42f7719167cc8");
        }

        private static void Case_02309()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2309,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-6,5,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-5,48,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,16,73,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-14,32,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-11,34,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-14,81,8,3), new GeneratedEnemyUnit(-8,-2,24,9,3), new GeneratedEnemyUnit(-13,-15,27,17,4), new GeneratedEnemyUnit(18,-4,97,49,4), new GeneratedEnemyUnit(-14,-8,58,9,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "f8e4c0d2212ad9abf9c336ab6deb633611985bfee40fb9a0dbd9cc130f55b85a");
        }

        private static void Case_02310()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2310,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-12,71,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-1,18,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,17,58,42,2), new GeneratedEnemyUnit(-13,4,96,34,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "9947daf5757ef7195d4ddcd0f0c8abbbcfd571f3edbf1838df37fe37a9bf5728");
        }

        private static void Case_02311()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2311,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,14,30,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-12,27,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-18,84,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,9,74,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-9,8,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,11,91,28,1), new GeneratedEnemyUnit(15,-1,29,37,3), new GeneratedEnemyUnit(7,20,83,17,1), new GeneratedEnemyUnit(8,-5,20,37,2), new GeneratedEnemyUnit(-3,15,95,32,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "f63798c326fb7bf487ad743928e43565f9c1406a2cee8b23c75b3df7b13dfe5c");
        }

        private static void Case_02312()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2312,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,16,15,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,3,20,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,20,86,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,18,84,6,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "d92d0e857a25841917bea9b74a8adae6ae6ae641cc26ab4995338dfd0edfab26");
        }

        private static void Case_02313()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2313,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-9,12,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,12,10,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,16,9,3,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "8688529bab352e51987867ceec717ef93d551ab74368877b9135f9fb5ea739b8");
        }

        private static void Case_02314()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2314,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,12,39,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,15,91,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-19,75,28,2), new GeneratedEnemyUnit(-10,16,12,48,2), new GeneratedEnemyUnit(-13,17,79,49,2), new GeneratedEnemyUnit(11,-18,54,4,2), new GeneratedEnemyUnit(-18,10,23,24,1), new GeneratedEnemyUnit(-5,17,73,33,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "e8350726be89e4ec18105e70d94d8397dfda574363f363a2fe881da1fb9922bf");
        }

        private static void Case_02315()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2315,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,17,63,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-9,42,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-4,88,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,7,38,38,2), new GeneratedEnemyUnit(-18,-4,12,21,4), new GeneratedEnemyUnit(-2,1,92,38,4), new GeneratedEnemyUnit(-5,16,52,43,2), new GeneratedEnemyUnit(18,-4,59,24,4), new GeneratedEnemyUnit(-13,-10,84,26,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5a1578c6b0b519aae905a602a939164be1159a240a29b4ba6ce6ea1874d1eff8");
        }

        private static void Case_02316()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2316,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,11,71,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,6,99,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,18,16,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,15,23,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-8,89,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,11,62,45,3), new GeneratedEnemyUnit(-4,2,26,49,2), new GeneratedEnemyUnit(19,9,62,6,4), new GeneratedEnemyUnit(5,5,74,32,2), new GeneratedEnemyUnit(-2,-10,92,38,2), new GeneratedEnemyUnit(-15,4,30,31,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "5f9e6ecb6b49e6793302a9b31f95e3d2fb5f7b44f26b2690f893ec44069e7981");
        }

        private static void Case_02317()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2317,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-20,39,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,5,69,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-3,57,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,19,76,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,10,6,6,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "cc57bb03e534a78bf50bdc801f03c29f6445defd517b4be846f9928571329b37");
        }

        private static void Case_02318()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2318,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-1,71,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,13,99,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-7,13,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,6,61,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-7,87,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,16,96,44,4), new GeneratedEnemyUnit(-7,19,25,35,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "343cc49485ff0f14dd44aa6940c37e94e0392d6a8e52d7fa3406eb7b2d37bd71");
        }

        private static void Case_02319()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2319,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-1,58,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-16,82,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,9,24,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-4,48,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,1,25,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-10,71,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,14,10,31,4), new GeneratedEnemyUnit(-4,20,15,19,4), new GeneratedEnemyUnit(-19,-17,20,13,3), new GeneratedEnemyUnit(7,12,100,6,2), new GeneratedEnemyUnit(1,-1,85,29,2), new GeneratedEnemyUnit(-13,0,36,22,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "3d21eba82b5af78d877c5436f3358c176aef1e58cdbf6fbb015f9ef1e6d0fd2f");
        }

        private static void Case_02320()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2320,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,7,99,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,1,95,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-11,48,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c5d065b8d3ecd38a3061015587a5c2a39fb352adf501a893304fe78ae88693f9");
        }

        private static void Case_02321()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2321,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,6,35,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,8,86,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,1,68,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-11,87,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,10,28,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-8,94,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,16,63,43,2), new GeneratedEnemyUnit(20,8,6,8,4), new GeneratedEnemyUnit(8,-10,86,29,3), new GeneratedEnemyUnit(1,-14,19,42,1), new GeneratedEnemyUnit(9,-16,36,20,1), new GeneratedEnemyUnit(-2,-15,85,16,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "a6948eb1da05a1c7cdf761d1737f6b06ca50fd721959787faa296daab3b7b71d");
        }

        private static void Case_02322()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2322,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-9,70,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,12,17,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-6,93,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,2,70,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-5,11,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,13,90,46,4), new GeneratedEnemyUnit(20,-8,88,43,3), new GeneratedEnemyUnit(8,9,74,24,4), new GeneratedEnemyUnit(11,8,90,29,2), new GeneratedEnemyUnit(5,-20,32,8,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "19c4cebde88686f9480c4c26ab31fdd35e350f8feb1083a25623bb3aca30de6f");
        }

        private static void Case_02323()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2323,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-15,98,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,13,15,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,11,98,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,11,94,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,17,42,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,11,79,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,4,37,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-1,63,7,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "03df676ef7c4c165bc107f7fa6ecc49d2dde3914a17db556aa3c06ef42ef6748");
        }

        private static void Case_02324()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2324,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,17,21,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,10,84,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-9,49,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-14,64,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,11,36,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-17,97,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,11,17,40,1), new GeneratedEnemyUnit(-20,0,79,8,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8b53ae67f988ecd9c9c2c8faac6a1e75732e4f436e1a74d900e48ea6adef0f33");
        }

        private static void Case_02325()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2325,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,6,93,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-3,52,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-20,85,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,2,26,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-17,93,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-7,64,50,1), new GeneratedEnemyUnit(12,5,31,34,4), new GeneratedEnemyUnit(5,-5,70,50,3), new GeneratedEnemyUnit(18,-4,55,1,2), new GeneratedEnemyUnit(11,6,87,14,4), new GeneratedEnemyUnit(4,14,26,42,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "1980c59aeb95357f7993a7a17877afcf207c808d6edc0953321f093e627521f3");
        }

        private static void Case_02326()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2326,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-8,51,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,10,86,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,14,66,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,6,64,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,8,62,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-3,91,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-10,85,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-13,29,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,15,83,21,2), new GeneratedEnemyUnit(-14,-4,27,19,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "1b6916e2d2054656f7e9a22bb50cda7901472e557b708ce81771d75640d4ffe9");
        }

        private static void Case_02327()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2327,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-7,75,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-2,80,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,2,10,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,15,77,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,4,39,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,3,86,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-13,91,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,18,63,45,1), new GeneratedEnemyUnit(-19,-13,18,19,4), new GeneratedEnemyUnit(0,7,19,9,2), new GeneratedEnemyUnit(-16,-18,66,29,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "82aaa16b25cd0313424912d9b5ce304575fe020e9e99078ae18fecf97ecaa4b4");
        }

        private static void Case_02328()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2328,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-15,38,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-1,60,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,9,38,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,8,55,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-2,38,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-2,50,3,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "f32e52b35f74920c3013b6421ed94fba5175ea390a548f34d7aa71002755006c");
        }

        private static void Case_02329()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2329,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-14,50,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,12,41,15,2), new GeneratedEnemyUnit(-19,4,34,30,2), new GeneratedEnemyUnit(12,-1,45,15,2), new GeneratedEnemyUnit(6,18,37,35,4), new GeneratedEnemyUnit(6,-7,67,10,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "f21bbccea1bdea88afb1b8c4fbf7fa2ab0ed536501bcd0d1369cf73f9b07f09c");
        }

        private static void Case_02330()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2330,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,12,32,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-4,70,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-10,7,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,7,83,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,8,75,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,19,55,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,10,51,33,1), new GeneratedEnemyUnit(-6,-12,60,41,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "6a50091c7cf2074f0986fc8fbc6ded362d085a7148d3614d26fce60546759d2b");
        }

        private static void Case_02331()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2331,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,6,20,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,9,27,19,4), new GeneratedEnemyUnit(-3,2,76,43,3), new GeneratedEnemyUnit(16,-10,91,30,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "9732b48d296758f6e4ad1de51f4834ca348896996efb880d78fc5d3cdfae548f");
        }

        private static void Case_02332()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2332,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,13,69,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-18,70,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-14,22,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-1,9,21,4), new GeneratedEnemyUnit(10,15,29,24,2), new GeneratedEnemyUnit(11,5,43,1,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "92268bf584988c9703e355c47408aea2754385c030046ffe01d52396a65eb7cd");
        }

        private static void Case_02333()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2333,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-17,14,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-14,90,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-18,49,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,10,37,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,7,87,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,4,29,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-5,11,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-5,36,10,2), new GeneratedEnemyUnit(4,4,10,26,1), new GeneratedEnemyUnit(19,4,62,50,1), new GeneratedEnemyUnit(13,10,37,19,4), new GeneratedEnemyUnit(8,-9,5,45,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "f99216abd67ce1bc5e0992ce79878f305b5d0392a97522ce1277b557ad757346");
        }

        private static void Case_02334()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2334,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-1,43,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-9,10,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-15,58,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,14,63,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,10,34,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,13,72,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,11,18,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,17,69,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-11,38,38,3), new GeneratedEnemyUnit(-20,-11,43,38,3), new GeneratedEnemyUnit(-6,17,33,29,4), new GeneratedEnemyUnit(11,-1,27,42,1), new GeneratedEnemyUnit(-16,-19,19,42,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "5e430e8d25f4a866f66cd41c7c4193775ca85d25595bfc5cc31962262aabd77c");
        }

        private static void Case_02335()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2335,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,4,86,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-20,36,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,3,77,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-18,92,2,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "8906f8498eea7333880546b75498be5417f3bd9a7cfe3e8401f2af4f7db6077c");
        }

        private static void Case_02336()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2336,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-9,47,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,7,51,34,1), new GeneratedEnemyUnit(-12,-1,71,19,1), new GeneratedEnemyUnit(3,-19,87,42,3), new GeneratedEnemyUnit(19,-2,12,42,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "12c8bcadf8d3a742b702f9ecf328a370b207b2d7885f8bbea14058018f88e066");
        }

        private static void Case_02337()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2337,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-15,18,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,9,84,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-20,92,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,1,99,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,4,91,28,1), new GeneratedEnemyUnit(-18,-5,42,50,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "c3ea544de1e7a4cdfdc0aff92c4c1e442dcc01918b890a3cb958d7154f8a14b1");
        }

        private static void Case_02338()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2338,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-5,83,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,6,79,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-10,12,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-14,98,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,5,64,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-17,99,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-20,87,4,3), new GeneratedEnemyUnit(5,20,76,50,1), new GeneratedEnemyUnit(16,-18,100,6,2), new GeneratedEnemyUnit(9,7,62,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "1d2490a4e5411966d2c6604193c0097540cc2d3c5d0ac7ce91d45484e8d9a173");
        }

        private static void Case_02339()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2339,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,14,53,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,9,11,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-20,21,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-5,69,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-6,63,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,14,25,3,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "6d3ce3a3ef42a374d57a24d49f9e7deb34e1a7a91c58d45fef1d349221c981b8");
        }

    }
}
