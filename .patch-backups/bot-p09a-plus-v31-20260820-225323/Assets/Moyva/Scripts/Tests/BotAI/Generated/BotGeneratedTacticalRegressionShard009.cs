using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard009
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_01620();
            Case_01621();
            Case_01622();
            Case_01623();
            Case_01624();
            Case_01625();
            Case_01626();
            Case_01627();
            Case_01628();
            Case_01629();
            Case_01630();
            Case_01631();
            Case_01632();
            Case_01633();
            Case_01634();
            Case_01635();
            Case_01636();
            Case_01637();
            Case_01638();
            Case_01639();
            Case_01640();
            Case_01641();
            Case_01642();
            Case_01643();
            Case_01644();
            Case_01645();
            Case_01646();
            Case_01647();
            Case_01648();
            Case_01649();
            Case_01650();
            Case_01651();
            Case_01652();
            Case_01653();
            Case_01654();
            Case_01655();
            Case_01656();
            Case_01657();
            Case_01658();
            Case_01659();
            Case_01660();
            Case_01661();
            Case_01662();
            Case_01663();
            Case_01664();
            Case_01665();
            Case_01666();
            Case_01667();
            Case_01668();
            Case_01669();
            Case_01670();
            Case_01671();
            Case_01672();
            Case_01673();
            Case_01674();
            Case_01675();
            Case_01676();
            Case_01677();
            Case_01678();
            Case_01679();
            Case_01680();
            Case_01681();
            Case_01682();
            Case_01683();
            Case_01684();
            Case_01685();
            Case_01686();
            Case_01687();
            Case_01688();
            Case_01689();
            Case_01690();
            Case_01691();
            Case_01692();
            Case_01693();
            Case_01694();
            Case_01695();
            Case_01696();
            Case_01697();
            Case_01698();
            Case_01699();
            Case_01700();
            Case_01701();
            Case_01702();
            Case_01703();
            Case_01704();
            Case_01705();
            Case_01706();
            Case_01707();
            Case_01708();
            Case_01709();
            Case_01710();
            Case_01711();
            Case_01712();
            Case_01713();
            Case_01714();
            Case_01715();
            Case_01716();
            Case_01717();
            Case_01718();
            Case_01719();
            Case_01720();
            Case_01721();
            Case_01722();
            Case_01723();
            Case_01724();
            Case_01725();
            Case_01726();
            Case_01727();
            Case_01728();
            Case_01729();
            Case_01730();
            Case_01731();
            Case_01732();
            Case_01733();
            Case_01734();
            Case_01735();
            Case_01736();
            Case_01737();
            Case_01738();
            Case_01739();
            Case_01740();
            Case_01741();
            Case_01742();
            Case_01743();
            Case_01744();
            Case_01745();
            Case_01746();
            Case_01747();
            Case_01748();
            Case_01749();
            Case_01750();
            Case_01751();
            Case_01752();
            Case_01753();
            Case_01754();
            Case_01755();
            Case_01756();
            Case_01757();
            Case_01758();
            Case_01759();
            Case_01760();
            Case_01761();
            Case_01762();
            Case_01763();
            Case_01764();
            Case_01765();
            Case_01766();
            Case_01767();
            Case_01768();
            Case_01769();
            Case_01770();
            Case_01771();
            Case_01772();
            Case_01773();
            Case_01774();
            Case_01775();
            Case_01776();
            Case_01777();
            Case_01778();
            Case_01779();
            Case_01780();
            Case_01781();
            Case_01782();
            Case_01783();
            Case_01784();
            Case_01785();
            Case_01786();
            Case_01787();
            Case_01788();
            Case_01789();
            Case_01790();
            Case_01791();
            Case_01792();
            Case_01793();
            Case_01794();
            Case_01795();
            Case_01796();
            Case_01797();
            Case_01798();
            Case_01799();
        }

        private static void Case_01620()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1620,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,13,53,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,13,78,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-16,65,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,3,85,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-18,25,12,2), new GeneratedEnemyUnit(-18,0,9,5,2), new GeneratedEnemyUnit(11,11,46,18,3), new GeneratedEnemyUnit(1,7,49,38,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "1d9d77b479ae7bcb11128145c318e5e65e4981e633893930b3d08d06ae61acae");
        }

        private static void Case_01621()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1621,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,18,51,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,16,22,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-2,88,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,7,20,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,14,98,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-2,41,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-17,90,2,1), new GeneratedEnemyUnit(-13,3,76,49,2), new GeneratedEnemyUnit(-1,-1,77,11,3), new GeneratedEnemyUnit(13,-14,43,45,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "e6f87aabc1bb62137502fe7389fcbd7920696f09641fc013091ade2a735fe5df");
        }

        private static void Case_01622()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1622,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-19,11,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,16,88,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,3,84,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-2,10,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-5,58,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,6,12,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,6,70,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-16,73,41,4), new GeneratedEnemyUnit(-9,-9,14,28,2), new GeneratedEnemyUnit(9,11,8,31,3), new GeneratedEnemyUnit(9,-19,35,28,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "5db9b4ef9254e3d050e9bc8a4810f6688513b346c1d25ba70c5c617fcfea9e54");
        }

        private static void Case_01623()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1623,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-9,87,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-19,51,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,2,74,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-18,67,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-2,39,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,11,77,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,1,60,24,2), new GeneratedEnemyUnit(-20,11,78,27,4), new GeneratedEnemyUnit(7,-18,91,39,4), new GeneratedEnemyUnit(9,-4,6,11,1), new GeneratedEnemyUnit(-13,1,42,11,4), new GeneratedEnemyUnit(-16,-13,43,23,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "265c6065efd6402e76e2f4bfbea9512825950d534e7cb51a897576c09911cc36");
        }

        private static void Case_01624()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1624,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,9,14,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,12,82,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,18,26,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-17,19,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-10,80,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,1,8,1,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "b272ce0cc75834c4f1dc36c1d3c9e2957c12512ff2645b5634057933b0324ba8");
        }

        private static void Case_01625()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1625,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-9,51,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-4,28,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-14,61,7,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "06e718654212737b76bb3f7ef04c54da36cbfea240630013f67405f1ab3cf4d1");
        }

        private static void Case_01626()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1626,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,0,38,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,9,48,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-17,26,5,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "db6d98a3639d23a5b681881ecc364248f0836ae16ae771d8b182d24dad50a944");
        }

        private static void Case_01627()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1627,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,1,100,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-5,88,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,1,14,30,4), new GeneratedEnemyUnit(-16,-6,21,31,2), new GeneratedEnemyUnit(13,-5,74,33,1), new GeneratedEnemyUnit(7,4,74,33,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "6c533046f59eddf6df6454a097784762b6fa4406b24a42b47ab9824f2e61a3a8");
        }

        private static void Case_01628()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1628,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-4,26,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,7,90,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-5,54,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-17,94,12,3), new GeneratedEnemyUnit(-15,6,91,25,1), new GeneratedEnemyUnit(15,-12,37,50,2), new GeneratedEnemyUnit(-5,-17,73,23,4), new GeneratedEnemyUnit(7,-13,33,10,2), new GeneratedEnemyUnit(-10,0,60,10,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "a84a3be858f2d9eac3cbf12eff33260de0ee92ca810b5c1a5e43c715bb870c32");
        }

        private static void Case_01629()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1629,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,10,42,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,10,87,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-7,62,1,2), new GeneratedEnemyUnit(12,17,45,16,1), new GeneratedEnemyUnit(14,17,35,32,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "997f2102759eb8f153cb50660ea127d4449be153190e422a47dde5eda94d14fd");
        }

        private static void Case_01630()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1630,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-10,40,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,2,8,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-16,86,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-14,33,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-2,44,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,19,35,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-4,31,13,3), new GeneratedEnemyUnit(-19,-12,60,27,1), new GeneratedEnemyUnit(6,-9,65,41,1), new GeneratedEnemyUnit(-6,1,43,50,2), new GeneratedEnemyUnit(-15,0,76,7,2), new GeneratedEnemyUnit(-3,0,75,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "65e5bc2b79ef55d2907d1584cfdf9dc75714bdf371982fd1e0119144c8d3364f");
        }

        private static void Case_01631()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1631,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,16,31,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,2,57,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-3,85,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,14,41,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,0,83,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-3,83,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,13,86,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,0,53,6,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "f1513fe5b5ea9a9a0d1d0394f47c7270c3545fb2bdf5d20bc2119158af76f585");
        }

        private static void Case_01632()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1632,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,14,58,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-17,74,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,7,5,21,4), new GeneratedEnemyUnit(-12,-13,21,29,1), new GeneratedEnemyUnit(1,13,28,10,3), new GeneratedEnemyUnit(19,10,99,42,3), new GeneratedEnemyUnit(-7,-12,100,28,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "306a099aec883c214178b0f36f121cf875e135935256c6a52726ea0122e151b4");
        }

        private static void Case_01633()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1633,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,16,76,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-4,12,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-2,57,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,12,99,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,12,52,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-2,48,35,4), new GeneratedEnemyUnit(8,-10,56,12,4), new GeneratedEnemyUnit(-17,-19,99,36,2), new GeneratedEnemyUnit(-20,-16,83,25,4), new GeneratedEnemyUnit(16,1,25,4,2), new GeneratedEnemyUnit(18,-19,84,22,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "72dac9697c7927ecce72252e2d95882fc17defb02677695737c3e6996caa64da");
        }

        private static void Case_01634()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1634,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-4,100,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,4,98,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-17,28,6,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "015b302af45d5f1bf2efff4fca95a7965ac724f840955b9a19c18e16f2d5e04d");
        }

        private static void Case_01635()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1635,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,2,66,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-13,15,7,1), new GeneratedEnemyUnit(-20,-10,46,2,3), new GeneratedEnemyUnit(-4,-8,23,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "7d29ee41eefca86c28e023ac4a4fc32ee03cbff84e80894bf3fdf66cb36605a8");
        }

        private static void Case_01636()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1636,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-15,66,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-15,66,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,11,19,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,13,42,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,15,66,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,7,59,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,20,85,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,6,76,31,3), new GeneratedEnemyUnit(5,-9,53,41,3), new GeneratedEnemyUnit(-3,10,34,24,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "e864dc03bf5ac1f97a5f37b748c9701f81b42f4d908555dd7b36ecc6d034eb73");
        }

        private static void Case_01637()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1637,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-6,8,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,14,76,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-19,53,47,4), new GeneratedEnemyUnit(10,11,67,29,2), new GeneratedEnemyUnit(8,-7,9,34,2), new GeneratedEnemyUnit(-10,16,51,1,4), new GeneratedEnemyUnit(-3,-14,83,43,3), new GeneratedEnemyUnit(-20,-20,97,44,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "e9cefaab04c7cfa21a575c02a23f1481ba42678b35f03c7bc2f4176f744f39e0");
        }

        private static void Case_01638()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1638,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-13,59,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-7,52,22,2), new GeneratedEnemyUnit(12,0,56,41,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "19f17703848bb6f733f67cd43d2e1067453178f112d0537a74c228fcc71b048d");
        }

        private static void Case_01639()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1639,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,19,61,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,13,68,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-3,52,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,19,69,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-5,65,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,16,100,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-9,37,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-6,61,13,3), new GeneratedEnemyUnit(1,-8,56,5,2), new GeneratedEnemyUnit(-15,11,68,25,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "54ea1a7f223e1ac9d7d82fb3f0d6fe053408bfa79a8bba8c58c65d3a99aff70b");
        }

        private static void Case_01640()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1640,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-14,98,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-17,67,43,1), new GeneratedEnemyUnit(1,18,92,46,1), new GeneratedEnemyUnit(-14,11,63,34,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "92a3489b09066fed471cd9a2b3dfcc90e3a7ee36e6def28a4f7e504ccc7338d6");
        }

        private static void Case_01641()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1641,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-2,47,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,12,59,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-15,87,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-15,71,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,3,52,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-2,53,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,17,60,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-15,73,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-15,25,28,3), new GeneratedEnemyUnit(-8,1,88,12,1), new GeneratedEnemyUnit(16,-6,79,24,1), new GeneratedEnemyUnit(20,-4,91,3,2), new GeneratedEnemyUnit(2,-17,95,47,4), new GeneratedEnemyUnit(14,12,89,32,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "0d87c0b2c9217429b781f3acb6cf550e2d27957243ad5b3229ba608a566d0b9b");
        }

        private static void Case_01642()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1642,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,9,53,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-8,44,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,4,79,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-10,78,17,4), new GeneratedEnemyUnit(14,-18,56,21,2), new GeneratedEnemyUnit(18,-8,59,49,3), new GeneratedEnemyUnit(1,-10,17,37,2), new GeneratedEnemyUnit(8,19,27,11,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "363a5bbbf69ae6af5e47468db47338aca5e23bb369257aed3a6ded05acba582b");
        }

        private static void Case_01643()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1643,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-3,40,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,20,67,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-2,17,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,19,17,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-15,40,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,8,28,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-12,28,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,20,86,16,2), new GeneratedEnemyUnit(15,12,88,48,1), new GeneratedEnemyUnit(-5,-3,72,6,4), new GeneratedEnemyUnit(9,-13,47,50,1), new GeneratedEnemyUnit(-11,8,37,22,4), new GeneratedEnemyUnit(-9,-5,76,29,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "7456e0fe246bd0b231432255c268b366a572defa1f4b11962741f17250b31ec0");
        }

        private static void Case_01644()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1644,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-20,89,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,18,36,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,12,59,19,4), new GeneratedEnemyUnit(-10,17,54,1,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "a639651485bfee7d7b8971aa03450fed9c0b0fe30fef3048be5b600edc101f19");
        }

        private static void Case_01645()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1645,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,2,16,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,15,97,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-4,13,8,3), new GeneratedEnemyUnit(17,-4,72,15,3), new GeneratedEnemyUnit(2,17,82,1,2), new GeneratedEnemyUnit(-7,-5,5,44,4), new GeneratedEnemyUnit(6,17,74,45,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "cd9fb5349a6707e70d6401755582487dfe904335e9ac015b3fc93eba532d1cde");
        }

        private static void Case_01646()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1646,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,2,78,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-6,36,42,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "bec550841ff58f4c7e53445eb9e9b5a8a9893a13d40d9873ac7628e35a79a7f6");
        }

        private static void Case_01647()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1647,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,18,17,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-17,32,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,6,24,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,6,94,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-4,46,6,4), new GeneratedEnemyUnit(8,12,47,11,2), new GeneratedEnemyUnit(-8,18,8,50,3), new GeneratedEnemyUnit(-6,-13,75,42,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "ae9354ca633c6474c0ed84074b88c53aefc3e49dd6f80ddcfe6c8552d970ab02");
        }

        private static void Case_01648()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1648,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-17,5,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-20,41,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,12,16,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-2,33,8,2), new GeneratedEnemyUnit(-20,1,29,30,4), new GeneratedEnemyUnit(3,-4,8,18,1), new GeneratedEnemyUnit(-8,15,82,11,1), new GeneratedEnemyUnit(-19,3,95,37,4), new GeneratedEnemyUnit(-9,4,25,50,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "b6a5b9195c71bdf6c3c57030f06265a640f104ae95d0dffcb59ceca7810649b1");
        }

        private static void Case_01649()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1649,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-3,93,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-18,9,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,14,37,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,0,90,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,11,24,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,6,93,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,18,91,42,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "d12f922bba6f2c14b3dfc4cac693630c9a2c103816432daeabc4c0056f9f2242");
        }

        private static void Case_01650()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1650,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,19,57,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-20,96,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,20,13,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-12,55,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-19,88,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-12,7,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,9,12,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,20,33,8,3), new GeneratedEnemyUnit(2,-2,31,48,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "e48698edbf0e6e2cc37072bb55da327cfce6e508a2eef541bfd1c58dd2930e07");
        }

        private static void Case_01651()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1651,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,4,29,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-14,49,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-18,72,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-10,38,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-12,100,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,12,29,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-1,50,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-5,49,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,11,37,15,2), new GeneratedEnemyUnit(8,-19,17,10,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "9cc6ecbca6beae8d68bdcdf305786a44a165a8285b17d1a26eb8df741d35c835");
        }

        private static void Case_01652()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1652,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,19,9,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-8,39,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,1,57,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,11,15,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,0,13,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,0,59,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,3,58,50,1), new GeneratedEnemyUnit(-6,0,61,9,3), new GeneratedEnemyUnit(-9,15,53,13,4), new GeneratedEnemyUnit(0,-10,78,45,1), new GeneratedEnemyUnit(-3,18,96,40,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "18cc4b3566b661241add375f30f9d6e1488f10de26dbaaee68b14c54ed8d10ce");
        }

        private static void Case_01653()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1653,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,20,77,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,17,20,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,14,97,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-11,36,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-16,96,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,13,66,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,2,43,12,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "aa96a9912746335053831d71f2d02ce7fe98d4a361e94ff1c9c868200ad83148");
        }

        private static void Case_01654()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1654,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,1,33,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,12,40,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-15,22,5,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "48547beacf4e2895d274ca32dede79975b20863ecf71f0af5bed5e6acb190e5f");
        }

        private static void Case_01655()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1655,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,12,60,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-12,23,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-12,82,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,10,24,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-16,82,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-6,80,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,10,52,1,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "a93b03e5d7aec93d6529bb628d9c6bda2dd576453bbea3933ee5516bfd910c1c");
        }

        private static void Case_01656()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1656,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-5,15,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,0,38,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,17,54,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-14,80,10,1), new GeneratedEnemyUnit(19,6,53,34,2), new GeneratedEnemyUnit(17,9,64,49,3), new GeneratedEnemyUnit(-10,-4,80,27,2), new GeneratedEnemyUnit(-5,-7,59,8,2), new GeneratedEnemyUnit(2,-2,53,8,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "7793dfcef68f064ba29d4fc03c51f8c70cf2e7ef9b77743d6b413f293dc2136a");
        }

        private static void Case_01657()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1657,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-18,41,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,5,84,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-8,38,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-8,29,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-19,96,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-20,52,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,16,80,1,1), new GeneratedEnemyUnit(17,-14,6,49,4), new GeneratedEnemyUnit(-18,14,39,30,4), new GeneratedEnemyUnit(-10,20,22,41,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "65bf9e6ed638efee898fe37181a832a2f35b4c9d166b23bdbcebecf8488af207");
        }

        private static void Case_01658()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1658,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-9,56,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-14,45,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-1,63,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-9,16,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-20,54,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,10,15,8,1), new GeneratedEnemyUnit(18,-1,71,15,2), new GeneratedEnemyUnit(-13,-7,20,44,2), new GeneratedEnemyUnit(-3,-14,44,17,1), new GeneratedEnemyUnit(10,-19,94,34,4), new GeneratedEnemyUnit(13,2,86,5,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "3ae5d418979908fc6d7c972623dfd970b51253942e2b0ed4e7f8dd1f22499de3");
        }

        private static void Case_01659()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1659,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-8,37,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-16,30,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,19,65,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-11,89,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-12,62,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-15,75,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,4,77,24,3), new GeneratedEnemyUnit(1,8,50,48,2), new GeneratedEnemyUnit(4,-4,66,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "ca7e56b6acd69c9aa51e2ee73d9d8a96974a53d399dac8e1293fc89eafda2a49");
        }

        private static void Case_01660()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1660,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,4,43,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-3,33,22,2), new GeneratedEnemyUnit(-6,8,64,1,3), new GeneratedEnemyUnit(-8,-12,15,4,1), new GeneratedEnemyUnit(3,-14,64,14,2), new GeneratedEnemyUnit(19,-6,32,22,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "7ea8c47aa9024497ea7cb2f8e52a08246b064a329a4ded9a92a47b1dc3b7a83d");
        }

        private static void Case_01661()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1661,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,15,61,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-6,13,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,16,40,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,17,48,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,15,98,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,15,35,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,2,95,31,2), new GeneratedEnemyUnit(5,8,51,30,4), new GeneratedEnemyUnit(-9,1,43,3,1), new GeneratedEnemyUnit(-11,13,96,13,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "5e74e49cc54bbc14eb9826472f91df5141548a7161e29df57ce6edd289a9003b");
        }

        private static void Case_01662()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1662,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-8,94,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-20,82,49,3), new GeneratedEnemyUnit(1,-3,16,32,3), new GeneratedEnemyUnit(8,-9,12,49,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "d90ccf2c3b1aa09852fc0c4debd75a8370eef7b8820753c7563ff208f4853817");
        }

        private static void Case_01663()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1663,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,0,12,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,15,80,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,0,25,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,3,60,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-8,83,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-16,11,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,16,5,46,1), new GeneratedEnemyUnit(8,-12,23,41,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "48b3473738a2fe8152b9e3ac7c99975057a2553f4b29ce1742aaafd42005bbb2");
        }

        private static void Case_01664()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1664,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,7,21,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-12,75,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,15,48,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,19,14,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-7,92,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,12,7,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,8,53,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,13,68,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-14,35,45,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "0598d33c6b6e483f4eca7c56d50e246be4dc8c2553b9e1dc999412c4352a053f");
        }

        private static void Case_01665()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1665,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,7,63,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-7,54,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,13,26,40,4), new GeneratedEnemyUnit(-2,-13,29,28,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2fcd29eac7327ed64424b6ee27bb43dc0e91afbf95756b88bcdda59462a67b5e");
        }

        private static void Case_01666()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1666,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,7,8,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-1,93,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-19,31,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-14,56,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,20,20,24,4), new GeneratedEnemyUnit(9,-17,20,22,4), new GeneratedEnemyUnit(-19,-4,67,11,3), new GeneratedEnemyUnit(-2,20,5,40,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "4b629b4b5b44ae6b80e027b3efbacdd71ff31fde28068c5a240d333cb9566f99");
        }

        private static void Case_01667()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1667,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,11,9,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,10,38,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,18,17,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,2,31,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,20,77,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-14,77,4,1), new GeneratedEnemyUnit(7,-10,80,17,4), new GeneratedEnemyUnit(20,0,86,6,4), new GeneratedEnemyUnit(15,10,71,23,2), new GeneratedEnemyUnit(2,-5,48,45,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "1bac71f28b5b3aa0a20f7fcac4e7fc0f9c13d4b91698d94f72d276b81937793d");
        }

        private static void Case_01668()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1668,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,13,49,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,11,14,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-17,7,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,14,76,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,17,13,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,17,54,34,4), new GeneratedEnemyUnit(10,-13,41,42,1), new GeneratedEnemyUnit(-6,-3,76,22,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "42df0d24c7353a4ff94c97d9dedb7946f072b9f2d4cf5d60c91657e5d00a576b");
        }

        private static void Case_01669()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1669,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,12,20,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-9,61,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,7,41,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,18,50,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,4,65,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,8,36,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-9,79,45,4), new GeneratedEnemyUnit(0,6,19,10,4), new GeneratedEnemyUnit(-6,14,59,22,2), new GeneratedEnemyUnit(-19,-9,100,9,2), new GeneratedEnemyUnit(4,11,24,43,4), new GeneratedEnemyUnit(12,13,90,24,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "63af6c9c6c99ccd129c204d83defc645d2c6983f938aa79ab968a93ac618ae8d");
        }

        private static void Case_01670()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1670,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-5,28,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,11,56,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,16,40,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,9,41,25,3), new GeneratedEnemyUnit(-14,12,8,24,1), new GeneratedEnemyUnit(-1,16,81,47,3), new GeneratedEnemyUnit(20,-1,41,6,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "1f09076a7992352ae2a77d2ed1ba57d970755fb31e98b62cf50307b97b74f38c");
        }

        private static void Case_01671()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1671,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-5,40,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-5,46,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-20,24,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-15,90,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-15,48,13,1), new GeneratedEnemyUnit(18,-1,97,16,1), new GeneratedEnemyUnit(-3,-1,76,30,2), new GeneratedEnemyUnit(7,-12,13,39,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "b3b144dee3e13a33f50d8d441309dea333d9fb06503726b9b8401d0a209ebc29");
        }

        private static void Case_01672()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1672,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-3,61,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,5,56,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,20,51,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-14,93,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-16,69,4,4), new GeneratedEnemyUnit(9,4,95,46,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "a28c7a135ac9f5cb03597f7c05eddb53767416c319807edbea01600669c1f76d");
        }

        private static void Case_01673()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1673,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,18,36,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-15,38,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,11,96,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,20,40,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,2,74,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-4,26,3,3), new GeneratedEnemyUnit(11,-4,80,34,3), new GeneratedEnemyUnit(13,7,99,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "caca5f1be4c55e383a707c2b4d5fcbc08b96b4f13c579aa963aa6aa77a24fcce");
        }

        private static void Case_01674()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1674,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,9,78,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,10,72,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,18,26,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,5,25,5,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "a310fc89204e91506b278cfd5dc499a320310f4a216633ef97697b5b9a6c5d1a");
        }

        private static void Case_01675()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1675,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,11,73,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-2,74,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,2,56,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-11,86,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,14,25,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,20,51,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-8,70,31,3), new GeneratedEnemyUnit(9,-16,32,9,2), new GeneratedEnemyUnit(11,17,55,2,1), new GeneratedEnemyUnit(-2,20,53,25,1), new GeneratedEnemyUnit(3,1,98,25,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "3e9bd42724d4a253d3600fa87aea0a840973b81f9ed0d3d3d54d6ef2bf557b55");
        }

        private static void Case_01676()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1676,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-11,57,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,17,92,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,14,99,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,8,6,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,19,68,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-14,38,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,6,53,2,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "f7f4c10974b8c3d2bfc2649e2d3bb750c70afa547559eb2b9f7aaf7e02684c71");
        }

        private static void Case_01677()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1677,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,19,63,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,4,88,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,7,78,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,11,14,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,16,63,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-15,87,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,10,47,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-15,91,37,3), new GeneratedEnemyUnit(-8,-15,28,43,1), new GeneratedEnemyUnit(10,-9,39,17,3), new GeneratedEnemyUnit(5,-14,27,34,1), new GeneratedEnemyUnit(-12,-10,25,30,1), new GeneratedEnemyUnit(7,-8,43,19,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "df0aadb1a5281f357bad9bb437cae2b176166b2a878ea1ddfd27f12d816e2b3d");
        }

        private static void Case_01678()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1678,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,20,62,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,5,76,1,1), new GeneratedEnemyUnit(-5,-1,86,11,1), new GeneratedEnemyUnit(-3,-15,19,28,2), new GeneratedEnemyUnit(3,20,80,28,2), new GeneratedEnemyUnit(-8,10,62,38,3), new GeneratedEnemyUnit(5,4,52,28,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "4a5480cdb7eba7b91b6dfb7fb16d8c9e11386425f3803b96c8f2ca8e55b21d29");
        }

        private static void Case_01679()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1679,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-15,58,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-4,33,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,3,9,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,14,47,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,16,20,3,2), new GeneratedEnemyUnit(1,-4,75,10,1), new GeneratedEnemyUnit(-4,-20,68,13,1), new GeneratedEnemyUnit(20,17,68,15,3), new GeneratedEnemyUnit(-19,2,40,25,3), new GeneratedEnemyUnit(1,8,65,10,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "8dc96cbb88e023aa3b18dabcf500db7d70a6da7c580f9704821e67ed277fb924");
        }

        private static void Case_01680()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1680,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,19,95,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-18,65,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-6,33,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-10,28,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,18,7,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,11,16,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,8,69,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-11,64,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-12,13,39,3), new GeneratedEnemyUnit(18,4,63,17,3), new GeneratedEnemyUnit(2,6,80,30,2), new GeneratedEnemyUnit(-9,13,70,28,4), new GeneratedEnemyUnit(16,-12,93,23,2), new GeneratedEnemyUnit(14,8,51,33,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "963ce53359b925fae31b68b21eec907b4044bc4df799deef9a27e3264ca3becf");
        }

        private static void Case_01681()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1681,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,6,23,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,3,77,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-12,82,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,17,8,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-3,98,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-15,23,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,19,65,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-19,6,6,4), new GeneratedEnemyUnit(-1,-12,66,39,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "8b95ad426f1ab5aa8f6a4a96baedeb2648b5a6b6af138724db7a54ae71071f58");
        }

        private static void Case_01682()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1682,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,8,27,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-19,80,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-6,70,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-6,6,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,7,83,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,15,19,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-12,7,3,1), new GeneratedEnemyUnit(-5,6,23,38,3), new GeneratedEnemyUnit(-13,12,36,42,3), new GeneratedEnemyUnit(-12,2,59,35,2), new GeneratedEnemyUnit(-13,15,15,7,4), new GeneratedEnemyUnit(20,-12,32,21,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "59e46def3f1d107216f561fbe071310ff1b949b984593c75a43bc1ae46e18fa3");
        }

        private static void Case_01683()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1683,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,2,72,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-4,86,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,0,13,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-4,92,24,2), new GeneratedEnemyUnit(-19,18,19,23,1), new GeneratedEnemyUnit(-12,-15,65,40,2), new GeneratedEnemyUnit(11,-17,59,15,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "f908c810b600992742bfb70d057c4238336c9ef95be77d764a1b111fc84fa967");
        }

        private static void Case_01684()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1684,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-1,75,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,0,32,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,7,76,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-7,83,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-12,28,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,3,17,14,4), new GeneratedEnemyUnit(3,-12,67,15,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "38c792760fd2ba8746f099a8f58aead04fd2fa40325ec0bfe22a62f8b348c6e7");
        }

        private static void Case_01685()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1685,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,8,52,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-15,31,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,5,67,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-5,41,26,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8d74f9a14a3289e4d6e8fc80ad7a975b575e08d58e7c64cdbc69c46f838c7f6e");
        }

        private static void Case_01686()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1686,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,16,59,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-1,29,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-11,69,6,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "1beb9bade18bf64f2f3e2e058e990bbf11373bf9a72ad58b5a82bc55a32d475d");
        }

        private static void Case_01687()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1687,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,1,6,4,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "ee868f8fd7b88e93a7776f8f398f38107667b48e1176ef847cf1f65c66d1c0ac");
        }

        private static void Case_01688()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1688,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-11,78,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,18,32,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-9,70,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,5,9,35,3), new GeneratedEnemyUnit(-8,6,99,40,4), new GeneratedEnemyUnit(13,1,69,24,3), new GeneratedEnemyUnit(18,9,60,4,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "eed69a3fb0e2b8ea72c72ed822398ac7db0607e9e3385b302a28f81349b74536");
        }

        private static void Case_01689()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1689,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-14,18,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,10,99,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,18,45,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-6,9,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,13,36,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-8,86,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,19,86,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-3,32,35,2), new GeneratedEnemyUnit(4,3,68,30,3), new GeneratedEnemyUnit(1,3,85,25,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "d01d862b403f698ec6fd017c7eff555d4483fd094af4d1936e7100bbba1444f0");
        }

        private static void Case_01690()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1690,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-20,55,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,2,19,33,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "b05759fa863e824630efdf21d46aee09ffee389421263841f3d8215b77bc0a5e");
        }

        private static void Case_01691()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1691,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,19,9,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-3,36,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,11,32,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,8,48,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,8,77,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,18,36,38,2), new GeneratedEnemyUnit(5,13,22,16,1), new GeneratedEnemyUnit(4,-9,64,42,4), new GeneratedEnemyUnit(2,12,83,44,1), new GeneratedEnemyUnit(-2,6,55,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b0eb1d043a00c046d624e6405eb01bfe1005e5a44485d4aca914f072eb597150");
        }

        private static void Case_01692()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1692,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,15,50,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-5,29,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-7,100,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-20,81,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-13,35,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,0,16,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-2,57,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,1,13,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-9,26,27,4), new GeneratedEnemyUnit(-3,11,15,1,1), new GeneratedEnemyUnit(-4,0,29,16,2), new GeneratedEnemyUnit(-10,-4,76,40,1), new GeneratedEnemyUnit(19,9,28,19,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "582b5611c663c86e0d782fee089294f1f380723a362075d0979fbbc3f71d33b7");
        }

        private static void Case_01693()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1693,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,3,100,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-7,72,4,4), new GeneratedEnemyUnit(-12,7,61,15,4), new GeneratedEnemyUnit(17,16,33,27,3), new GeneratedEnemyUnit(-19,2,48,25,2), new GeneratedEnemyUnit(16,-3,43,50,3), new GeneratedEnemyUnit(-13,20,49,12,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "e323eb1ab3bf64185949b27ce27857cf104ebd4001e68e9f2534f61001bd2de5");
        }

        private static void Case_01694()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1694,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,20,42,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-5,91,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,1,15,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,10,89,8,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "a57f8a746d6df4fe594642c79bfbcc7ae365c822300018f710f5161f8e6ed994");
        }

        private static void Case_01695()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1695,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,0,73,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-1,89,39,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "450539a5deafa5365fb733db438e9c20f11ad32c936fe58c199a0da7a18071cc");
        }

        private static void Case_01696()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1696,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,1,98,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,17,66,49,3), new GeneratedEnemyUnit(-6,-4,68,47,3), new GeneratedEnemyUnit(-20,7,32,50,3), new GeneratedEnemyUnit(-8,10,90,43,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "9e3e64ee46baf977bf8452455640916b8e911b58eeaceb9be90b00a8bbbd9734");
        }

        private static void Case_01697()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1697,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,3,64,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,9,37,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,7,41,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-2,78,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-1,73,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,14,6,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,8,68,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-6,56,3,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "c5904c20f66f83e1794a1019cdd5affc7dd996c8276116d0e8dbc4cad83639c4");
        }

        private static void Case_01698()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1698,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,18,90,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-12,21,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,13,53,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,5,80,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,20,53,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-4,48,24,4), new GeneratedEnemyUnit(-12,10,85,27,4), new GeneratedEnemyUnit(11,10,26,29,3), new GeneratedEnemyUnit(13,-19,84,11,3), new GeneratedEnemyUnit(3,4,30,22,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "7f06d4c311430e00357d832d1dee6e170ebbc282ffe7ef897972856027252caf");
        }

        private static void Case_01699()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1699,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-13,82,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,14,75,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,14,94,27,3), new GeneratedEnemyUnit(-18,-1,97,33,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "f897887ec8b2e10cd281ccc9624a34f63351a19136eb9175d9a4a94cef1effb2");
        }

        private static void Case_01700()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1700,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,10,42,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-4,27,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-14,20,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,15,40,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-20,88,31,2), new GeneratedEnemyUnit(-12,-19,65,22,4), new GeneratedEnemyUnit(-4,-3,37,48,3), new GeneratedEnemyUnit(7,-6,26,19,3), new GeneratedEnemyUnit(12,-4,58,13,3), new GeneratedEnemyUnit(-2,-6,98,22,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "63e6374ec05bf04ba4f6bb103187fd8eff850736e4f70fe635d4c772e9cc02ea");
        }

        private static void Case_01701()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1701,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,19,55,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-16,43,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-6,9,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,16,27,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,4,7,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,19,63,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,1,58,38,1), new GeneratedEnemyUnit(-2,-3,94,40,2), new GeneratedEnemyUnit(13,6,54,39,1), new GeneratedEnemyUnit(-7,-14,46,22,3), new GeneratedEnemyUnit(-5,-15,52,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "d070c49aaebbc9c6dbdbd8d63cb265597a00429bf095cfe88f01f137238461c4");
        }

        private static void Case_01702()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1702,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,15,67,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-1,83,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,13,99,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-14,46,45,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "795fddf50487c761d45a7ceecfc9445b38bae1453bd7f6104d28ac97d7d5f9dd");
        }

        private static void Case_01703()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1703,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-19,53,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-20,83,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-8,69,22,3), new GeneratedEnemyUnit(9,2,85,34,3), new GeneratedEnemyUnit(3,-20,12,6,2), new GeneratedEnemyUnit(5,1,71,14,1), new GeneratedEnemyUnit(13,-1,63,32,3), new GeneratedEnemyUnit(15,-6,52,39,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "9be2925c4c90a374043519efd04108c21508f837099f8de02da864a9c7e45e5f");
        }

        private static void Case_01704()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1704,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-12,38,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,9,78,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-19,46,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-13,19,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,9,63,16,2), new GeneratedEnemyUnit(-4,-13,90,39,1), new GeneratedEnemyUnit(-16,20,28,46,4), new GeneratedEnemyUnit(15,4,91,11,2), new GeneratedEnemyUnit(4,-6,32,12,2), new GeneratedEnemyUnit(7,-3,16,6,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "d02cd50da3ba2c9b3e2e348164099a53f953cbf4b3b7c8ed83527f8b1f92a721");
        }

        private static void Case_01705()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1705,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,18,39,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,5,13,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,12,75,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,10,46,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,12,85,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,9,21,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-4,50,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,13,43,36,3), new GeneratedEnemyUnit(-4,-7,100,25,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "7334982ff4d65915d0c5e714b85e2d8ae2483da2f9b41fc056ffcc9fbf3bc769");
        }

        private static void Case_01706()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1706,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,1,37,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,17,61,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,4,38,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,17,59,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-3,37,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,2,46,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,6,85,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,18,78,2,1), new GeneratedEnemyUnit(13,19,25,42,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7f54dcee0330e68b91c38bdc3787326bd5f2bb8120264dd288c914968d8ac481");
        }

        private static void Case_01707()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1707,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-4,22,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-4,15,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,19,69,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-6,43,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,13,24,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-2,18,19,3), new GeneratedEnemyUnit(-13,-5,50,43,2), new GeneratedEnemyUnit(0,1,36,49,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "53d8dea0d4b9230fbb5fc71ca1b6ec7613bbee754c53aec988b971bf75f2c139");
        }

        private static void Case_01708()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1708,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,5,76,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,20,89,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-2,6,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,3,76,8,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "b3635001dc4baeb8f22cfb1334a9896a030308997b568a4cff95069b9a90138a");
        }

        private static void Case_01709()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1709,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-16,33,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-11,26,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-20,13,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-7,68,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,0,51,43,1), new GeneratedEnemyUnit(-5,5,9,49,4), new GeneratedEnemyUnit(-10,18,55,15,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "73791dfa34cdda9bd5e1c3cd8b1fe40de55f64bfdd10ffd98fc268d57735185e");
        }

        private static void Case_01710()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1710,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-7,56,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-9,17,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,11,63,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,13,13,37,1), new GeneratedEnemyUnit(3,-13,17,42,4), new GeneratedEnemyUnit(-2,8,52,28,2), new GeneratedEnemyUnit(10,-1,33,33,3), new GeneratedEnemyUnit(-16,-10,60,17,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "86083a611577957ca87d15ff72ce6487d4e2babc38bae5d567065e118ac7cf02");
        }

        private static void Case_01711()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1711,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-15,35,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,5,81,17,3), new GeneratedEnemyUnit(18,-19,95,10,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "b5bc0aa51031ff0c3f9ed780d17c907519a7a5a6aa076357d835e13985aac92a");
        }

        private static void Case_01712()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1712,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,20,10,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,9,82,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-9,15,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,5,21,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,17,92,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-12,76,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-13,49,16,1), new GeneratedEnemyUnit(11,9,44,8,4), new GeneratedEnemyUnit(-18,15,10,26,3), new GeneratedEnemyUnit(13,0,99,26,3), new GeneratedEnemyUnit(16,2,76,31,3), new GeneratedEnemyUnit(12,9,33,18,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "c377d90219e51c74796dacabe1547f81d3b0fe386805146a11206c576141e5a1");
        }

        private static void Case_01713()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1713,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-2,69,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,13,7,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,0,34,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,12,50,37,1), new GeneratedEnemyUnit(-9,0,58,24,3), new GeneratedEnemyUnit(-15,-15,96,22,4), new GeneratedEnemyUnit(19,-10,94,27,3), new GeneratedEnemyUnit(9,-10,23,16,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "14fee85e2c2d863248d7447d612e2a4dee41daaab583c1b67675cb9b8d76b8c8");
        }

        private static void Case_01714()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1714,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-9,57,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-14,19,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-16,9,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,14,29,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-9,36,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-16,23,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,18,79,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,13,31,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,9,100,5,1), new GeneratedEnemyUnit(-10,-7,15,6,4), new GeneratedEnemyUnit(-4,-19,88,43,3), new GeneratedEnemyUnit(9,3,64,20,4), new GeneratedEnemyUnit(7,-11,31,9,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "38eee2d70743c26f18bc18e7fcffa6cbf9b812cb73a3fb4544c9b0a8b0f90fa4");
        }

        private static void Case_01715()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1715,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,19,99,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,14,9,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-8,30,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-17,24,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,19,41,12,1), new GeneratedEnemyUnit(8,8,93,28,3), new GeneratedEnemyUnit(16,16,67,33,2), new GeneratedEnemyUnit(-6,17,34,18,4), new GeneratedEnemyUnit(-16,-11,58,1,1), new GeneratedEnemyUnit(13,15,23,36,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "3843cddf7a66c18060b2155fb58663c97f594d57a14b46c6919d2e1ed2fc7bb5");
        }

        private static void Case_01716()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1716,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,18,87,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-2,25,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-11,29,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-19,39,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,11,100,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-2,23,29,2), new GeneratedEnemyUnit(1,9,19,47,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "2ed37bb6179d4820093e94b6ff06f5672a56f9583dc4c91c54294f90cf12a73b");
        }

        private static void Case_01717()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1717,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-12,55,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-18,99,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-5,68,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,12,68,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,9,35,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,20,73,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,8,83,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,15,96,20,1), new GeneratedEnemyUnit(-14,9,61,26,4), new GeneratedEnemyUnit(2,18,21,43,2), new GeneratedEnemyUnit(10,0,57,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "818a1fc41736447bd5d2a75d4a7068c96be2ca1c3b0737613015931e4ad579ce");
        }

        private static void Case_01718()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1718,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,8,80,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-18,51,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,19,12,25,3), new GeneratedEnemyUnit(-20,-6,54,23,3), new GeneratedEnemyUnit(10,19,55,33,2), new GeneratedEnemyUnit(11,-13,11,18,1), new GeneratedEnemyUnit(11,-18,87,27,2), new GeneratedEnemyUnit(16,-10,17,32,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "70d3782bd8597fe107f064546598165fb9d15a7d876292024056c301aedf54d0");
        }

        private static void Case_01719()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1719,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-4,40,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-11,81,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,1,52,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,14,7,44,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "86f082a3fafda5ff0adccc67b9aa757ed6a67704da90f668c6fcda6c5e85dec8");
        }

        private static void Case_01720()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1720,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-18,31,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-4,9,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,11,39,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-11,16,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-11,92,50,4), new GeneratedEnemyUnit(13,6,14,18,3), new GeneratedEnemyUnit(16,-7,66,9,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "55570f96055f9586d92d604a968b80b0d18bd3be4b83e96f4f9e60584f2b8f73");
        }

        private static void Case_01721()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1721,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,14,79,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-5,48,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-4,72,46,4), new GeneratedEnemyUnit(-2,12,45,10,1), new GeneratedEnemyUnit(5,14,95,20,2), new GeneratedEnemyUnit(-5,-7,6,9,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "024146d759d7159146f2670d1151a154e926696f430626e050fdc9b10ac67834");
        }

        private static void Case_01722()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1722,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-3,64,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,16,48,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-6,64,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,4,42,30,1), new GeneratedEnemyUnit(-13,-4,20,13,3), new GeneratedEnemyUnit(-1,-15,93,3,1), new GeneratedEnemyUnit(-9,-9,87,14,3), new GeneratedEnemyUnit(16,6,28,15,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "a815d4bed4e07f4a7199faa756db6bbcf7b0f3205be7abfee34a010fb80b9dfa");
        }

        private static void Case_01723()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1723,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-10,51,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-16,63,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-7,76,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,20,99,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-13,42,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,0,44,26,1), new GeneratedEnemyUnit(-8,7,36,17,2), new GeneratedEnemyUnit(5,1,95,20,1), new GeneratedEnemyUnit(-7,-14,9,25,2), new GeneratedEnemyUnit(12,9,5,47,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "92783489b315e480abd40fa9b5c5740f2c8f34548a9259da7500bbc01f29cbc8");
        }

        private static void Case_01724()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1724,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-13,100,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,15,54,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-17,80,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,4,71,41,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c0b5f3f5d1ddb722b51b3e064423ceaa3352058f047a8848c1686150d1bf8c5e");
        }

        private static void Case_01725()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1725,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,3,66,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-16,82,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-13,17,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-15,60,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,12,51,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,10,70,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,6,96,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-19,83,5,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "f38db4dee683ed3007d1ffefa8ebbb4a459aee5938e8377e210dc03984906457");
        }

        private static void Case_01726()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1726,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,18,27,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-10,32,20,4), new GeneratedEnemyUnit(7,18,65,40,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "9b03187b3237cfe948f7fdcf603539c3e327db2cb1158a445ae2405c519ff14a");
        }

        private static void Case_01727()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1727,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-20,18,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-9,67,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,13,73,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,6,23,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,3,40,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-20,33,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-15,54,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,1,70,15,2), new GeneratedEnemyUnit(17,2,65,21,4), new GeneratedEnemyUnit(7,15,64,48,3), new GeneratedEnemyUnit(13,-9,55,39,4), new GeneratedEnemyUnit(-20,-12,86,12,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "23539c09cb8a0dc8ed9e4bc6b9834180c6fc8bdd01e0a4c74e21e1f2ebd48c6f");
        }

        private static void Case_01728()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1728,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-4,46,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-14,12,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-8,94,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,17,39,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-19,95,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,8,89,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-13,71,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-3,41,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,2,37,29,2), new GeneratedEnemyUnit(5,15,67,29,3), new GeneratedEnemyUnit(-20,0,46,34,1), new GeneratedEnemyUnit(-6,-15,36,28,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "76bef701c58180926050b7a85bf827497947e894f4803fc506940d1f288faa13");
        }

        private static void Case_01729()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1729,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-6,72,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-8,7,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-15,62,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,20,27,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-17,72,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,20,54,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-6,19,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-18,36,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,16,100,50,3), new GeneratedEnemyUnit(11,3,28,47,1), new GeneratedEnemyUnit(3,3,85,50,4), new GeneratedEnemyUnit(9,20,74,9,4), new GeneratedEnemyUnit(4,7,61,7,2), new GeneratedEnemyUnit(-20,-13,66,22,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "27a50d703110ea7aaea5528f4bbe9c0625a8d8b18521c0c271f7cb9f058eb673");
        }

        private static void Case_01730()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1730,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-10,96,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-12,78,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-9,19,16,2), new GeneratedEnemyUnit(-18,19,90,1,1), new GeneratedEnemyUnit(-2,-4,89,23,1), new GeneratedEnemyUnit(14,-13,17,50,1), new GeneratedEnemyUnit(-7,6,35,27,3), new GeneratedEnemyUnit(-10,16,78,1,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "532e5d444df70c828920f17ab6b5b05ee8071a860b699d51da55d10f16029f1d");
        }

        private static void Case_01731()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1731,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,14,11,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-14,20,13,3), new GeneratedEnemyUnit(-20,7,76,16,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "f24fc1fcb8df3ab37e5e571e15a81cb0a8b972b55c24771768568ccab2f7c0c3");
        }

        private static void Case_01732()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1732,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-10,43,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,13,26,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,6,57,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-14,43,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,7,54,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,17,94,6,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "1cb86ff94a99a4aaf0c79302c05406631ba5bdf98bfde1b38670b3bea0e47aea");
        }

        private static void Case_01733()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1733,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-2,46,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,4,53,24,2), new GeneratedEnemyUnit(-11,-5,52,30,1), new GeneratedEnemyUnit(9,4,48,7,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "1a7c7b3f3d8d33fed320c841a46fd02363b47e9366375632df4ebc6e88ac0bdb");
        }

        private static void Case_01734()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1734,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-15,78,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-11,94,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-17,26,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,12,36,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,4,52,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,3,52,21,2), new GeneratedEnemyUnit(-16,8,73,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "c09628498ecb0063887e2e9e81876f89ecae926ba80f37991029f5cc062188f9");
        }

        private static void Case_01735()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1735,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-3,61,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-1,52,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,11,49,41,1), new GeneratedEnemyUnit(-13,19,67,50,1), new GeneratedEnemyUnit(3,20,88,44,4), new GeneratedEnemyUnit(16,-12,22,47,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "275c6df4293b3ba6fe0195857ded3e7c4320545d0bf834fbd35b81de721d9426");
        }

        private static void Case_01736()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1736,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,11,68,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-1,44,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-16,69,16,4), new GeneratedEnemyUnit(-8,1,20,2,4), new GeneratedEnemyUnit(-5,-8,97,14,3), new GeneratedEnemyUnit(-14,-15,44,24,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "3d687daeecbe8b1c3f8e943b8e67fe26125a474a04e797af7e10163b6f49baa2");
        }

        private static void Case_01737()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1737,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-2,18,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-13,94,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-4,6,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-14,69,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,3,73,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,0,81,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-5,18,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-10,94,47,3), new GeneratedEnemyUnit(19,-9,84,45,4), new GeneratedEnemyUnit(4,-1,58,48,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "ed264e247681ef2868cbe0ab2621d54159417fc80e14a77e4f77cbedfe88fb30");
        }

        private static void Case_01738()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1738,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,18,45,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-10,31,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-15,84,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,13,19,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,0,64,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,3,57,42,1), new GeneratedEnemyUnit(4,10,83,24,2), new GeneratedEnemyUnit(9,11,74,23,1), new GeneratedEnemyUnit(18,-2,47,36,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "286bad396841c35835bcd626cd0451b83843b07f1d4252782548c5d9bb183cc1");
        }

        private static void Case_01739()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1739,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,11,65,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-5,58,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-8,46,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,3,25,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-1,96,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,20,78,7,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "1bb26485c7a8b44455d1d0580d213286da7a7777217549af16efee05c0d646a0");
        }

        private static void Case_01740()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1740,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-2,59,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-18,10,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-5,79,38,2), new GeneratedEnemyUnit(18,0,14,13,2), new GeneratedEnemyUnit(-13,-6,17,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "f68ad4746d457615b6e89cd9c8f5f93cb0fa500e66ca333b8a9dc14af5fd380d");
        }

        private static void Case_01741()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1741,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-8,47,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-9,79,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,10,76,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-11,76,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-16,79,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,16,78,49,3), new GeneratedEnemyUnit(7,7,66,4,1), new GeneratedEnemyUnit(18,-20,26,18,1), new GeneratedEnemyUnit(-1,1,83,43,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "d2cd88a147f49c18e637b60310bfca66e480a3c362d2e1ee053fbef9d7830150");
        }

        private static void Case_01742()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1742,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-5,99,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,16,63,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,6,19,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,11,76,4,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "872bfe4c0e9179cfdf9f5ebb5905039def9ed2c9a1aac1684bc6b210dfd4d7ff");
        }

        private static void Case_01743()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1743,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,0,55,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,13,23,27,2), new GeneratedEnemyUnit(10,-13,25,33,4), new GeneratedEnemyUnit(0,-15,77,45,4), new GeneratedEnemyUnit(-1,19,45,43,4), new GeneratedEnemyUnit(-16,-17,57,32,1), new GeneratedEnemyUnit(6,19,85,14,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "06679875dc82c511b2246e06a7df5cc7ab0c9e38dd56e1322a806a53b4ab1a94");
        }

        private static void Case_01744()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1744,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-18,54,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-3,75,23,2), new GeneratedEnemyUnit(-9,6,13,14,4), new GeneratedEnemyUnit(20,4,29,10,1), new GeneratedEnemyUnit(-15,6,58,43,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "11e56bf4a347914459bcd654ed7ccb3b3de0e301d011da8fdd0b4f4c3a1a90cf");
        }

        private static void Case_01745()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1745,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-13,21,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,10,93,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,19,64,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,17,61,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,4,47,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-19,94,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-6,55,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-14,63,7,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "0e7a20caf2c11e16e544b48678a38f78049454de3904d153c785525e3fe64893");
        }

        private static void Case_01746()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1746,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-9,75,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,17,34,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-11,19,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,5,73,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,0,83,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-1,98,9,3), new GeneratedEnemyUnit(7,2,17,7,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "04d2ca243c794edd8796b76e2ccd703bbd9e00aa89fc8a116b3b0b8eee939f0d");
        }

        private static void Case_01747()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1747,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-4,62,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,14,8,37,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "69427651032fd948a4af1e4fa9968bd50320e6c56bb7c39ffff8614d39f12a39");
        }

        private static void Case_01748()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1748,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-3,65,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-6,77,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-4,60,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,10,100,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-19,82,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,13,12,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,14,90,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,6,14,31,3), new GeneratedEnemyUnit(-12,17,38,2,3), new GeneratedEnemyUnit(-20,-3,6,37,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "2b63f7ee251d1d187308eb512514fcb9e046f63d73fe6e8c694af7ec918add27");
        }

        private static void Case_01749()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1749,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-7,71,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-15,74,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-13,45,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,20,68,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,9,74,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-17,40,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,1,16,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-1,60,6,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "06885efc42ae7744baf1692e0fef530f8e9491fd13cffe1c61167f98fe7de4f3");
        }

        private static void Case_01750()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1750,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-11,39,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,6,30,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-5,90,15,1), new GeneratedEnemyUnit(-10,0,20,20,4), new GeneratedEnemyUnit(-5,2,37,40,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "e826e5836c3ab26c2871baf0d4ec6e7ca8fc1f85362c2d249de8052b8cf75f70");
        }

        private static void Case_01751()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1751,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,19,67,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-8,99,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-7,82,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,4,37,46,2), new GeneratedEnemyUnit(13,-1,42,16,3), new GeneratedEnemyUnit(-17,12,72,1,4), new GeneratedEnemyUnit(16,8,48,21,2), new GeneratedEnemyUnit(-6,-4,42,1,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "37ebdd3876174cde7bef87a3de287c63bf1ddec59b05f3f2f4ce21b948929a4e");
        }

        private static void Case_01752()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1752,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-9,86,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-3,81,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,1,79,1,3), new GeneratedEnemyUnit(19,-11,41,49,2), new GeneratedEnemyUnit(-3,15,47,18,2), new GeneratedEnemyUnit(13,-4,31,27,2), new GeneratedEnemyUnit(11,-8,82,3,2), new GeneratedEnemyUnit(-17,16,19,15,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "e1feae56c7853bc8b4998e85f4850e44f446bfff5e1fb71204c9b6b87be48a14");
        }

        private static void Case_01753()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1753,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-13,66,6,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "00bfe04d007e12c4b28cce8dac339e59b1ef4fb4ed3c87d9f5d666923bb2625b");
        }

        private static void Case_01754()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1754,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,17,76,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,6,90,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-10,15,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,19,57,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-13,34,20,2), new GeneratedEnemyUnit(-17,-18,67,8,3), new GeneratedEnemyUnit(18,14,14,9,2), new GeneratedEnemyUnit(-14,-4,23,32,3), new GeneratedEnemyUnit(-14,-12,72,44,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "318fd4faaee1d69ee74e44a450c6959f7669c58b0f9bea07ff19627c8766d706");
        }

        private static void Case_01755()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1755,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,13,86,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-12,95,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,3,16,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-16,22,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,9,29,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-19,33,23,1), new GeneratedEnemyUnit(14,5,71,17,3), new GeneratedEnemyUnit(-13,2,33,14,3), new GeneratedEnemyUnit(-18,4,49,21,1), new GeneratedEnemyUnit(-15,14,62,39,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "e99b18ca99e5ad697ff55da370487f91878dc030a310d8daff44c087bdf1bca5");
        }

        private static void Case_01756()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1756,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-16,92,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,2,85,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-15,99,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-1,72,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,13,75,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,13,42,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,17,89,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-18,97,24,4), new GeneratedEnemyUnit(-4,-18,96,40,3), new GeneratedEnemyUnit(2,-2,61,34,4), new GeneratedEnemyUnit(8,-19,73,11,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "7953ce48e031c6870baaf74bf17b49607dd06672f13d222ba3877afbb3b1efa4");
        }

        private static void Case_01757()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1757,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,15,99,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-9,12,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,10,91,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,15,40,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,18,93,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,15,9,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,16,94,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-19,81,14,4), new GeneratedEnemyUnit(-12,11,94,44,4), new GeneratedEnemyUnit(16,-12,30,32,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "0c82c57480e440045481a91fcafa822123258e0e85383214e5474d235c279f14");
        }

        private static void Case_01758()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1758,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-4,73,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-10,28,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-20,47,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-3,12,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-17,12,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,20,68,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,15,41,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-15,54,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-3,74,33,4), new GeneratedEnemyUnit(4,-11,92,5,4), new GeneratedEnemyUnit(-1,-11,55,20,3), new GeneratedEnemyUnit(10,5,42,11,1), new GeneratedEnemyUnit(-6,20,52,44,4), new GeneratedEnemyUnit(19,10,58,50,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "e8a4e05cb39d84101ba51e87060d710592bcc0102489e98caead37d38d75b507");
        }

        private static void Case_01759()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1759,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,15,34,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,3,98,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,5,90,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,2,63,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,9,32,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-19,14,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-6,72,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-4,34,5,2), new GeneratedEnemyUnit(1,-4,64,40,2), new GeneratedEnemyUnit(-20,-15,19,45,4), new GeneratedEnemyUnit(-20,6,62,11,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "6309b7487df9d0e0abf12eede8aed742a008692257553609dd6f5f7405f5b329");
        }

        private static void Case_01760()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1760,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-18,87,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,20,84,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-15,42,33,2), new GeneratedEnemyUnit(4,15,74,19,3), new GeneratedEnemyUnit(1,20,5,23,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "632d1d56fdaac2d208481135bc5cabeb75ea9dc116ef66bddf4a53d33ad8ccfe");
        }

        private static void Case_01761()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1761,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-15,83,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,14,45,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,6,100,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,20,42,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,4,39,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-17,19,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-3,80,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,10,75,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-15,47,27,2), new GeneratedEnemyUnit(-20,7,7,25,2), new GeneratedEnemyUnit(-17,-10,5,7,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "c451bb5a67c0fd609f9cb82ac2a3a77b0259d2f00c87cafc515f01c0f08c64bf");
        }

        private static void Case_01762()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1762,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,12,59,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,9,13,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,3,14,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-20,98,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-18,80,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-15,78,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-5,21,40,3), new GeneratedEnemyUnit(20,-4,32,13,3), new GeneratedEnemyUnit(-7,19,76,44,2), new GeneratedEnemyUnit(15,15,46,40,1), new GeneratedEnemyUnit(11,-2,20,21,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "d373f2285d238957c85a2daed0f3f40e536480e01042c72a99d5d2330a145e08");
        }

        private static void Case_01763()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1763,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,13,78,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-4,89,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,2,17,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,8,36,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,4,16,2,3), new GeneratedEnemyUnit(-17,7,74,32,3), new GeneratedEnemyUnit(-9,6,57,26,3), new GeneratedEnemyUnit(-1,-7,51,8,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "7b08a04654b6c2d9afeb1fbd777574b47266d8bc822ae17cec125a8f8243b30c");
        }

        private static void Case_01764()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1764,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-8,21,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,3,34,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,3,62,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-12,60,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-19,41,3,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "f2bcdd7e7515b997c2695e45fc943b9d2bb169d9794861de85be4a35f0ee5ebb");
        }

        private static void Case_01765()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1765,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,0,49,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-10,46,40,2), new GeneratedEnemyUnit(16,9,51,12,4), new GeneratedEnemyUnit(11,14,84,42,4), new GeneratedEnemyUnit(-17,-19,35,39,2), new GeneratedEnemyUnit(6,16,56,48,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "5ced609fefdd85ed1a6406d53f805f3f7034832be833c6dd48cc9543d5daf5b4");
        }

        private static void Case_01766()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1766,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-18,100,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,5,20,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,7,63,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-7,43,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-1,82,4,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "a367c8e92377f5d9c7ee65be837a744feed7139d233c760d86fb1c67597d979b");
        }

        private static void Case_01767()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1767,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-20,86,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,15,15,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,4,18,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,13,28,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,17,25,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-9,47,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-1,56,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,0,34,8,2), new GeneratedEnemyUnit(20,-15,18,49,4), new GeneratedEnemyUnit(13,3,9,13,4), new GeneratedEnemyUnit(10,-7,83,30,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "5e28c5de573a72ec11c8016960e9cad6a93bf2c97e09a6210ed8b6da23de8742");
        }

        private static void Case_01768()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1768,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-8,86,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,7,29,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,8,84,6,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "bf7475e9026e49602ac784d5212a35b1807690f37bf8afe6ae27d6e26494db08");
        }

        private static void Case_01769()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1769,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-17,76,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-7,47,15,4), new GeneratedEnemyUnit(-14,-3,39,44,2), new GeneratedEnemyUnit(-2,-11,46,24,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "75c020cda629ade1c8bfcd16b0b998f8515ff9d0121930b75891b0811f73144a");
        }

        private static void Case_01770()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1770,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-3,78,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-3,30,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,16,37,12,4), new GeneratedEnemyUnit(-8,17,64,15,1), new GeneratedEnemyUnit(-4,5,95,6,3), new GeneratedEnemyUnit(3,-14,76,44,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "686536dcf726f67549a2c68c3b6fdce0a6358382f19a61abe8a86479aa3a0e4e");
        }

        private static void Case_01771()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1771,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,0,27,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-3,78,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,18,85,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,5,68,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,9,29,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,15,34,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,0,46,50,4), new GeneratedEnemyUnit(-9,1,32,17,1), new GeneratedEnemyUnit(-1,3,64,47,4), new GeneratedEnemyUnit(-8,2,80,10,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "bcc894a9f8616c1b4fffafd94526a9be8c9d2141ad0d24b0174f12d6c4c7761b");
        }

        private static void Case_01772()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1772,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,1,6,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,20,77,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,3,31,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-7,20,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,5,18,6,2), new GeneratedEnemyUnit(1,-16,41,46,3), new GeneratedEnemyUnit(17,-4,84,12,2), new GeneratedEnemyUnit(-18,3,69,15,4), new GeneratedEnemyUnit(-17,10,48,9,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "4086a175e900ad0023fe1b455ad19b034507bc1a5507bb5505b769c014e7af2f");
        }

        private static void Case_01773()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1773,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,8,59,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,17,87,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,17,12,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-11,38,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,0,29,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,18,15,2,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "98253ea7866913e1a20daded14ab5a172bccc334d18db958f8f61fe6d1d8c182");
        }

        private static void Case_01774()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1774,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,8,41,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-16,28,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,8,54,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,9,48,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-1,71,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,19,9,45,4), new GeneratedEnemyUnit(-10,-18,7,18,1), new GeneratedEnemyUnit(4,17,67,16,4), new GeneratedEnemyUnit(-6,9,25,12,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "c8153abb241f6e5a8cde1fd2065095000ea43ec6d54c06c88441b7bc590da70c");
        }

        private static void Case_01775()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1775,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,6,97,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,3,67,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-1,51,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-14,88,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,5,91,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-20,25,39,4), new GeneratedEnemyUnit(16,4,18,27,2), new GeneratedEnemyUnit(17,11,76,6,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "65fcc776aa42bef43e7c7a22f1a2623c2c70f5efa315914690bb9ec53f3a8ccd");
        }

        private static void Case_01776()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1776,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-5,87,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-19,48,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,2,41,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-8,67,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,15,100,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-14,99,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,20,10,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,9,13,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,20,36,29,4), new GeneratedEnemyUnit(15,-6,98,13,4), new GeneratedEnemyUnit(1,4,82,37,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "c8314ed6d4af77986d288fb30b2189941201cd63bdd321dc0e96985196b5fc98");
        }

        private static void Case_01777()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1777,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-11,88,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-17,83,44,1), new GeneratedEnemyUnit(10,-7,99,4,1), new GeneratedEnemyUnit(2,8,29,43,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "dcc8946945caf36dde33be8b0ed7cebe99fe006d19f37b9df1dbce4b8f3a068c");
        }

        private static void Case_01778()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1778,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,15,70,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,9,38,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-19,23,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,9,99,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,1,66,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-15,32,22,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "219b7e1df60957b87327f1879b514529ed82678f87a168aec3335c4b165023bc");
        }

        private static void Case_01779()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1779,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-9,84,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-7,48,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,18,58,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,18,66,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,2,37,7,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "a2766741ba30790fa104a3bd61f19c1730e6e23d7be65a5c68197f5bb99e7858");
        }

        private static void Case_01780()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1780,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,6,73,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-15,76,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-7,36,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-17,53,40,3), new GeneratedEnemyUnit(1,-9,10,18,2), new GeneratedEnemyUnit(-14,0,77,37,2), new GeneratedEnemyUnit(20,-12,70,33,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "b3bf3d3ee895b2a0cbb1fd90ceba3df4cf5c93c95e62c5a3a0bcb972c6d31b75");
        }

        private static void Case_01781()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1781,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,12,67,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,10,22,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,12,87,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-10,88,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-20,49,23,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "61c383307783a73ab8dede24d46de4e95e1f32afa67dd583eecc550d4f4a7f90");
        }

        private static void Case_01782()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1782,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-17,94,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,4,5,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-19,88,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-3,47,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,13,27,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,4,39,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,5,24,44,1), new GeneratedEnemyUnit(-17,10,55,42,2), new GeneratedEnemyUnit(-1,-16,61,20,1), new GeneratedEnemyUnit(-4,11,80,44,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "c48dc215df0eae232d1246b3a58917e58eb7caa9106ae58bc335891818d518d5");
        }

        private static void Case_01783()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1783,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,19,65,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,3,63,2,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "680ab795aa288d66dd3fbddc078d39bf8d3104a0c47f1659935149490f925218");
        }

        private static void Case_01784()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1784,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-16,99,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-7,87,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-12,52,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,9,13,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-18,28,41,2), new GeneratedEnemyUnit(-5,14,58,35,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "1747ac7e1d89ac7336d5d20e5b9b70dce03d19f62d39a8bc9579ffca468d3d37");
        }

        private static void Case_01785()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1785,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,14,100,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,20,12,8,1), new GeneratedEnemyUnit(-9,-15,49,24,3), new GeneratedEnemyUnit(2,19,28,13,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "13f41ba49c31e142e7d60d11260270c852755d0a97f27752cbb85dcc61e7ebbd");
        }

        private static void Case_01786()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1786,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-17,28,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,5,94,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-11,45,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-2,49,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,15,40,2,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "43fb54b6df3700eda3af79a8ecd683d97cd75a862ca079bccb54bf0f8d7111b5");
        }

        private static void Case_01787()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1787,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,8,77,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,10,61,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,18,75,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,17,23,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-7,96,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,10,73,6,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "2352e0863dc0b216d1b73c87b2fe6943549fd541bb3c89f4f69a8304b0d2a5bd");
        }

        private static void Case_01788()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1788,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-14,50,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-19,31,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,7,45,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-5,9,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-3,16,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-2,92,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,4,73,7,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "436b5e5e251de075c1041e39472e9b40a8aef2b7813ee4ce3dc03dcfdff2f5f5");
        }

        private static void Case_01789()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1789,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,18,55,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-7,65,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,3,23,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,0,87,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,5,18,31,1), new GeneratedEnemyUnit(-6,20,88,23,2), new GeneratedEnemyUnit(13,-12,28,2,2), new GeneratedEnemyUnit(-8,-5,65,47,2), new GeneratedEnemyUnit(6,4,17,46,1), new GeneratedEnemyUnit(2,-7,32,30,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "92b9cfe93ac8e4f90cd0201f96335d40d294dc347e2fe1785bf8c5825f2befce");
        }

        private static void Case_01790()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1790,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-3,15,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-17,99,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,10,88,24,2), new GeneratedEnemyUnit(15,9,5,18,1), new GeneratedEnemyUnit(9,8,14,26,2), new GeneratedEnemyUnit(-8,16,50,23,4), new GeneratedEnemyUnit(-3,-1,8,33,4), new GeneratedEnemyUnit(4,-9,35,43,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "1ddb0af4932a7e6ac6e3e2ca396dfd233eadb690ba059dcfd386e9a8231498d1");
        }

        private static void Case_01791()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1791,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-1,41,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-13,40,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-14,62,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-6,15,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-16,69,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,4,91,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-15,85,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,4,18,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,12,31,24,1), new GeneratedEnemyUnit(6,-4,93,8,4), new GeneratedEnemyUnit(-6,-1,54,45,4), new GeneratedEnemyUnit(11,-2,53,29,2), new GeneratedEnemyUnit(10,-14,97,17,2), new GeneratedEnemyUnit(3,13,74,16,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "22e43cc13c8a6ea299406f2e76bd10dc7b7358bc66129e1a4d219d68878df586");
        }

        private static void Case_01792()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1792,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,3,58,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-19,39,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-19,38,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-7,66,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-7,78,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-17,53,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,4,23,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-13,57,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-12,86,43,4), new GeneratedEnemyUnit(-11,-20,90,6,4), new GeneratedEnemyUnit(-16,17,40,3,2), new GeneratedEnemyUnit(14,-9,68,46,3), new GeneratedEnemyUnit(19,5,33,33,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "f1dd73a08e1de54b90f89a8c1dda50597fe57104bf0d9f3a70e988276e13ca3f");
        }

        private static void Case_01793()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1793,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,17,87,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,2,93,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,5,56,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-7,82,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-9,51,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,12,12,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,7,42,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,3,59,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,8,70,45,4), new GeneratedEnemyUnit(20,-17,40,33,4), new GeneratedEnemyUnit(11,15,5,46,1), new GeneratedEnemyUnit(8,5,97,46,1), new GeneratedEnemyUnit(-5,16,85,13,2), new GeneratedEnemyUnit(-15,-11,60,24,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "a075f4ac5c1b03ac900e1c10fe211bcc9c80b297556ed98a68ce344a67808164");
        }

        private static void Case_01794()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1794,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,18,9,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,5,69,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-16,58,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-14,71,6,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "0fa08bd2490ae34dfa9b9f83292d676885fd9658854fd4e35cfa164a1ae5faee");
        }

        private static void Case_01795()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1795,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,15,93,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-17,50,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,0,9,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-7,73,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,10,23,5,4), new GeneratedEnemyUnit(16,8,99,37,1), new GeneratedEnemyUnit(-10,-8,75,39,3), new GeneratedEnemyUnit(-11,-3,68,13,4), new GeneratedEnemyUnit(10,1,29,42,3), new GeneratedEnemyUnit(-20,11,66,25,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "b149f979d313926cd17306218a3dd577e8a6b3e9c9181adf477e82bd38cdc613");
        }

        private static void Case_01796()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1796,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,12,28,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-13,61,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,19,17,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-9,88,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,8,55,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-7,89,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,11,89,12,3), new GeneratedEnemyUnit(7,-10,60,44,4), new GeneratedEnemyUnit(13,-20,36,21,2), new GeneratedEnemyUnit(-15,-13,12,25,3), new GeneratedEnemyUnit(-15,1,40,45,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "8eee6dee5edf23f0b232a7b67b1b416fca960bd18e4f8b56abffe611dd60e50b");
        }

        private static void Case_01797()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1797,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-17,36,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-12,42,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,12,27,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,7,37,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,17,61,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,15,90,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,10,52,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,8,78,1,1), new GeneratedEnemyUnit(18,6,77,22,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "c73f1a83ac29f957face66f965530b279a85ffd045e02d29543c28f6dd780e04");
        }

        private static void Case_01798()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1798,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-19,70,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,16,57,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,8,9,23,3), new GeneratedEnemyUnit(2,14,75,1,4), new GeneratedEnemyUnit(-19,-5,32,26,4), new GeneratedEnemyUnit(16,-12,32,30,1), new GeneratedEnemyUnit(-2,4,40,6,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "7da7c46e361cbeea2b670f926665629c805e67b37b0f80e3bf82c1c2f35df289");
        }

        private static void Case_01799()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1799,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,17,17,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-10,66,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-10,12,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,16,83,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-18,29,15,1), new GeneratedEnemyUnit(5,-1,88,47,4), new GeneratedEnemyUnit(-15,16,55,40,2), new GeneratedEnemyUnit(0,-17,69,10,2), new GeneratedEnemyUnit(7,-19,99,3,4), new GeneratedEnemyUnit(-12,-19,59,24,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "15dde5ae923d71db7795abcbcbefba91fd0b32f95c9ec8b0f86a49928c841e0e");
        }

    }
}
