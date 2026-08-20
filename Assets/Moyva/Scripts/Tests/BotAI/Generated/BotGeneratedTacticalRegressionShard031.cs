using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard031
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_05580();
            Case_05581();
            Case_05582();
            Case_05583();
            Case_05584();
            Case_05585();
            Case_05586();
            Case_05587();
            Case_05588();
            Case_05589();
            Case_05590();
            Case_05591();
            Case_05592();
            Case_05593();
            Case_05594();
            Case_05595();
            Case_05596();
            Case_05597();
            Case_05598();
            Case_05599();
            Case_05600();
            Case_05601();
            Case_05602();
            Case_05603();
            Case_05604();
            Case_05605();
            Case_05606();
            Case_05607();
            Case_05608();
            Case_05609();
            Case_05610();
            Case_05611();
            Case_05612();
            Case_05613();
            Case_05614();
            Case_05615();
            Case_05616();
            Case_05617();
            Case_05618();
            Case_05619();
            Case_05620();
            Case_05621();
            Case_05622();
            Case_05623();
            Case_05624();
            Case_05625();
            Case_05626();
            Case_05627();
            Case_05628();
            Case_05629();
            Case_05630();
            Case_05631();
            Case_05632();
            Case_05633();
            Case_05634();
            Case_05635();
            Case_05636();
            Case_05637();
            Case_05638();
            Case_05639();
            Case_05640();
            Case_05641();
            Case_05642();
            Case_05643();
            Case_05644();
            Case_05645();
            Case_05646();
            Case_05647();
            Case_05648();
            Case_05649();
            Case_05650();
            Case_05651();
            Case_05652();
            Case_05653();
            Case_05654();
            Case_05655();
            Case_05656();
            Case_05657();
            Case_05658();
            Case_05659();
            Case_05660();
            Case_05661();
            Case_05662();
            Case_05663();
            Case_05664();
            Case_05665();
            Case_05666();
            Case_05667();
            Case_05668();
            Case_05669();
            Case_05670();
            Case_05671();
            Case_05672();
            Case_05673();
            Case_05674();
            Case_05675();
            Case_05676();
            Case_05677();
            Case_05678();
            Case_05679();
            Case_05680();
            Case_05681();
            Case_05682();
            Case_05683();
            Case_05684();
            Case_05685();
            Case_05686();
            Case_05687();
            Case_05688();
            Case_05689();
            Case_05690();
            Case_05691();
            Case_05692();
            Case_05693();
            Case_05694();
            Case_05695();
            Case_05696();
            Case_05697();
            Case_05698();
            Case_05699();
            Case_05700();
            Case_05701();
            Case_05702();
            Case_05703();
            Case_05704();
            Case_05705();
            Case_05706();
            Case_05707();
            Case_05708();
            Case_05709();
            Case_05710();
            Case_05711();
            Case_05712();
            Case_05713();
            Case_05714();
            Case_05715();
            Case_05716();
            Case_05717();
            Case_05718();
            Case_05719();
            Case_05720();
            Case_05721();
            Case_05722();
            Case_05723();
            Case_05724();
            Case_05725();
            Case_05726();
            Case_05727();
            Case_05728();
            Case_05729();
            Case_05730();
            Case_05731();
            Case_05732();
            Case_05733();
            Case_05734();
            Case_05735();
            Case_05736();
            Case_05737();
            Case_05738();
            Case_05739();
            Case_05740();
            Case_05741();
            Case_05742();
            Case_05743();
            Case_05744();
            Case_05745();
            Case_05746();
            Case_05747();
            Case_05748();
            Case_05749();
            Case_05750();
            Case_05751();
            Case_05752();
            Case_05753();
            Case_05754();
            Case_05755();
            Case_05756();
            Case_05757();
            Case_05758();
            Case_05759();
        }

        private static void Case_05580()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5580,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-1,91,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-13,52,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-5,80,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,5,71,33,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "f56956373f33bf447484ad04bd8db51a35f734974e5f5bd1e7b818e137416eca");
        }

        private static void Case_05581()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5581,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-18,16,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-13,23,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-9,76,5,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "c9e0406b163d96acea57f2028782a166135dd7d35a874c151ebbe9819f200ae3");
        }

        private static void Case_05582()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5582,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-4,79,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,17,48,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,19,74,5,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "a6ce66ff0c09d081abd35d2781b9478e0d9b0732fa6efe780008796355604751");
        }

        private static void Case_05583()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5583,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-3,41,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,1,7,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-12,39,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,11,74,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,15,46,7,3), new GeneratedEnemyUnit(8,6,70,45,1), new GeneratedEnemyUnit(16,-8,73,29,2), new GeneratedEnemyUnit(-7,11,69,20,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "a295a23741363de8c82ce0211c3e0ba643e6af45a74a17b8fbcd87b73960be51");
        }

        private static void Case_05584()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5584,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-14,34,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-1,78,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-9,28,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-2,12,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,19,86,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-16,65,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,6,99,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-7,90,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,10,32,8,3), new GeneratedEnemyUnit(14,19,25,1,2), new GeneratedEnemyUnit(-20,-1,36,47,2), new GeneratedEnemyUnit(-1,-1,65,26,3), new GeneratedEnemyUnit(-1,13,13,8,1), new GeneratedEnemyUnit(-15,-5,24,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "c70ec818ce1102fb9e3edb283504532486ddefd1991dc4a7c28db8d1f0c69ab4");
        }

        private static void Case_05585()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5585,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,5,83,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,8,100,24,4), new GeneratedEnemyUnit(-10,-3,36,46,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "dba4a4a5a343c92fdfc0b820f9e4675c82427daec1ef23a7df9985691272e6e3");
        }

        private static void Case_05586()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5586,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-10,75,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,3,97,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-15,47,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,11,10,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,14,28,3,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "4c34252cd8c163f34131ff94da9ba4ed1fff9f0c1422cc558cacd4ade4a24100");
        }

        private static void Case_05587()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5587,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,17,39,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,19,35,48,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "aaff67b89ff068645417167fcb98c99c6b634d9e908679986bf484b7b5178b35");
        }

        private static void Case_05588()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5588,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-20,67,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-20,56,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-3,79,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-8,49,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-20,86,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-17,89,30,3), new GeneratedEnemyUnit(10,-13,94,2,1), new GeneratedEnemyUnit(12,12,99,47,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "15720c76f192727a713c0256d76ca188fc095e84aa4f724d86fadc97560be4e2");
        }

        private static void Case_05589()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5589,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-19,96,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,9,33,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,15,20,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,7,22,49,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "12a28d0ba63ae3c09a3cc9eb254c8f2bd3a24d5e2d2de5fbd93277405d2a1fcc");
        }

        private static void Case_05590()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5590,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,8,16,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,15,86,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-14,8,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-18,87,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-4,11,37,3), new GeneratedEnemyUnit(-13,3,85,9,2), new GeneratedEnemyUnit(20,1,69,25,1), new GeneratedEnemyUnit(-18,11,53,24,1), new GeneratedEnemyUnit(1,9,35,11,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "f4f801aeb547fb9ad34235627b03a531eef7244df143fe5f0a848c090bcc7ef1");
        }

        private static void Case_05591()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5591,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-3,85,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-20,72,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,17,88,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,17,58,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,8,76,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,15,61,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,16,9,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,17,100,41,1), new GeneratedEnemyUnit(-13,-7,35,43,2), new GeneratedEnemyUnit(-6,-20,93,9,3), new GeneratedEnemyUnit(-6,15,45,13,4), new GeneratedEnemyUnit(14,-7,71,11,1), new GeneratedEnemyUnit(-4,13,45,37,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "b6be96db82cb003abebef683415042e2f48d0b2696c18710cb38a2b6c8496425");
        }

        private static void Case_05592()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5592,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-5,41,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,2,87,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,8,76,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-4,99,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,5,20,49,1), new GeneratedEnemyUnit(-14,-4,42,20,4), new GeneratedEnemyUnit(-8,2,47,44,4), new GeneratedEnemyUnit(14,16,49,49,3), new GeneratedEnemyUnit(1,-15,51,8,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1133c99d0d736f0152d4fe9ac2a5529df7bbf4a57dd459f93f83930f32a45a4c");
        }

        private static void Case_05593()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5593,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,20,80,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-17,89,43,3), new GeneratedEnemyUnit(10,5,56,48,3), new GeneratedEnemyUnit(6,6,89,44,3), new GeneratedEnemyUnit(20,20,67,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "21e4563a6c70c431dc1b76a769273acc86abb7197a227f1204597cf04e2e3715");
        }

        private static void Case_05594()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5594,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,8,41,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-15,18,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,1,30,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,20,9,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-9,53,11,2), new GeneratedEnemyUnit(-7,-13,9,31,4), new GeneratedEnemyUnit(16,-9,67,36,2), new GeneratedEnemyUnit(-16,4,62,39,3), new GeneratedEnemyUnit(-16,19,67,38,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "ea20ed434da119454180fced36bb67c78814d26fa09c04cb67186f2003171827");
        }

        private static void Case_05595()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5595,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,19,27,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-12,99,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-11,83,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-7,96,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,17,73,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-8,89,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,2,93,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,20,6,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-10,48,19,1), new GeneratedEnemyUnit(16,7,8,20,1), new GeneratedEnemyUnit(-13,-15,26,25,4), new GeneratedEnemyUnit(17,-3,23,18,2), new GeneratedEnemyUnit(14,-3,25,34,1), new GeneratedEnemyUnit(19,-2,7,47,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "0b8430ad393a502c40e4ac05b0c57c7768892994f7c34444596daf07e158c438");
        }

        private static void Case_05596()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5596,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,6,28,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-9,69,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,14,48,27,2), new GeneratedEnemyUnit(-20,5,76,20,1), new GeneratedEnemyUnit(16,-17,90,11,3), new GeneratedEnemyUnit(-3,19,79,31,4), new GeneratedEnemyUnit(8,3,12,5,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "b00192a5cf52a357d07ccbd231979dcedb72998154863933e964c28ad7e9cd67");
        }

        private static void Case_05597()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5597,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,6,18,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,18,26,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,1,91,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,9,84,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,3,42,1,2), new GeneratedEnemyUnit(-2,10,16,1,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "5bbdf47298198c9a3491f617d1d8e25dd27dd3bdf6df936515cb7b91be0c4c48");
        }

        private static void Case_05598()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5598,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,7,69,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-14,19,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,7,31,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,14,74,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,11,42,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-15,61,45,3), new GeneratedEnemyUnit(8,-14,76,41,2), new GeneratedEnemyUnit(0,-13,61,4,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "049dd0ddf35255c7e85345e6743215b092e7230ff29610628a67ac723de36882");
        }

        private static void Case_05599()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5599,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-11,9,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-15,60,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,13,26,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-12,70,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,2,100,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-16,33,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,5,43,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-14,24,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,14,16,45,1), new GeneratedEnemyUnit(8,-4,6,12,1), new GeneratedEnemyUnit(9,-20,8,6,2), new GeneratedEnemyUnit(-12,-15,32,16,1), new GeneratedEnemyUnit(11,4,53,2,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "044e20bc8ba187fe023968f0656f6140c797f14900dab05020bfb13c37f519f8");
        }

        private static void Case_05600()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5600,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,16,95,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-16,57,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-20,88,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-20,39,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-20,86,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-2,91,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-10,43,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,13,31,29,4), new GeneratedEnemyUnit(-6,15,45,48,2), new GeneratedEnemyUnit(-15,15,10,50,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "dc509cd7c98119a328f0a0139ce81e1bf55d0d05aa1668a760a79e638c1f1ead");
        }

        private static void Case_05601()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5601,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,18,8,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-19,71,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,17,87,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-8,45,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,20,66,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-14,95,41,2), new GeneratedEnemyUnit(1,-10,25,6,4), new GeneratedEnemyUnit(-15,16,93,49,4), new GeneratedEnemyUnit(6,20,48,45,4), new GeneratedEnemyUnit(-17,1,40,13,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "650f4db5bfa7133ad8953d95dc80acbb16af311aeb2e5a86d93322b3986bf43e");
        }

        private static void Case_05602()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5602,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-13,23,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,7,54,22,3), new GeneratedEnemyUnit(-9,11,48,50,2), new GeneratedEnemyUnit(-12,-12,97,46,2), new GeneratedEnemyUnit(-2,-19,46,22,3), new GeneratedEnemyUnit(-16,11,29,10,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "edc9a51db6b6e29ceac06aee560f2be42813ac4a8dd4acac889bbaba71f192ba");
        }

        private static void Case_05603()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5603,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-2,29,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-5,47,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,13,36,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,3,6,50,3), new GeneratedEnemyUnit(0,12,47,38,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "70552d3e056a3439d7214eff344d88c0d6391d5a025cd26a8befaf442e9c19b2");
        }

        private static void Case_05604()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5604,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,14,87,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,9,44,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-19,63,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-8,16,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-20,70,41,4), new GeneratedEnemyUnit(16,0,59,23,3), new GeneratedEnemyUnit(5,18,55,47,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "627458507cd8aa0a3d131e7d9d5a574c4aa4719753e423ebb818c4456f632f5c");
        }

        private static void Case_05605()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5605,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-15,8,6,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "4e0360328ea3314bd4bcc5521e33981dec9c6e011c00d47fa083dcc0427738d0");
        }

        private static void Case_05606()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5606,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,20,88,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,12,21,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-12,85,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-15,78,8,3), new GeneratedEnemyUnit(-11,-20,51,26,4), new GeneratedEnemyUnit(15,-9,78,24,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "25edbe4305dd2a3e1a86aada7696d9f560738119f2c26a8d629dd49214158a29");
        }

        private static void Case_05607()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5607,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,20,79,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,7,53,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,20,57,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,14,54,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,2,90,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-13,47,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-15,97,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,0,37,4,4), new GeneratedEnemyUnit(8,9,63,7,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0eeedba353bd56a9e96a89146821871775808e8eb0b37091821b7caf4f266d3c");
        }

        private static void Case_05608()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5608,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-9,65,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,11,93,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,3,65,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-9,76,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,13,17,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-20,94,13,2), new GeneratedEnemyUnit(-16,-7,96,29,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "9f2d1cee1b743e39b890119ac5e67a0b4f1a6f7aafcc4cbbd2da7d35bcb8bce2");
        }

        private static void Case_05609()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5609,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-14,91,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,7,28,14,1), new GeneratedEnemyUnit(-14,-3,8,38,3), new GeneratedEnemyUnit(-5,-1,42,16,4), new GeneratedEnemyUnit(19,14,18,31,1), new GeneratedEnemyUnit(8,-20,8,34,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "772434a07c939dff82520d4527a0db7e175c9ccc428d5f181bd0ebce2b0e399b");
        }

        private static void Case_05610()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5610,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-3,82,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,13,100,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-3,61,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,12,17,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,8,95,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,18,11,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-15,58,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,6,22,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,17,59,14,3), new GeneratedEnemyUnit(-4,16,54,47,4), new GeneratedEnemyUnit(17,6,86,16,2), new GeneratedEnemyUnit(10,16,6,9,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "ab9d4d77c0aac52c066ebdd85612ed73d7362d023e7cc148f5b98486a3879462");
        }

        private static void Case_05611()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5611,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-3,58,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,11,96,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,16,87,37,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "52e3c62a62bdff373a6f58dc58268d09b6b1248e14b6e0de86e89158587e5918");
        }

        private static void Case_05612()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5612,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,9,46,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,12,78,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,11,43,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-10,69,27,1), new GeneratedEnemyUnit(17,-16,39,3,3), new GeneratedEnemyUnit(-11,15,65,34,3), new GeneratedEnemyUnit(7,-11,29,20,2), new GeneratedEnemyUnit(-18,20,18,9,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "428f27511fdc1a6b45c99595a2e7e3a6f2b644745bccde26c5d2598fdcafc427");
        }

        private static void Case_05613()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5613,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,9,83,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,20,20,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-7,68,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-11,51,34,2), new GeneratedEnemyUnit(-10,-18,26,49,2), new GeneratedEnemyUnit(7,11,94,4,2), new GeneratedEnemyUnit(7,19,86,47,4), new GeneratedEnemyUnit(-8,19,60,28,4), new GeneratedEnemyUnit(-4,-3,70,3,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "45e2e0131b302b19bc0b144259f4ce9ea4888f26bbc03f77aaca9992f81f6012");
        }

        private static void Case_05614()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5614,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-19,24,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,7,38,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-11,6,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-17,87,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,10,16,47,1), new GeneratedEnemyUnit(0,-15,16,21,4), new GeneratedEnemyUnit(12,6,89,47,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "4ac8bef69111d3250158a38f9db1e47611e8d4fbcc2fcb5569c6381a80be3012");
        }

        private static void Case_05615()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5615,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,7,5,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,16,86,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-12,52,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,20,23,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,1,82,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,2,59,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-9,9,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-15,56,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,20,13,43,4), new GeneratedEnemyUnit(-17,4,23,2,2), new GeneratedEnemyUnit(12,19,33,36,2), new GeneratedEnemyUnit(18,6,27,35,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "c5ec1f4b330f586e88f72a48ffa8e0a01a49690137c4f5aec40d336a4cbcdb72");
        }

        private static void Case_05616()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5616,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-17,67,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-7,64,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,7,21,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,19,10,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-20,97,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,17,74,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,1,49,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,20,78,28,3), new GeneratedEnemyUnit(7,9,9,44,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "eda9f8cc77813ea35e9aa027eca5b0105bcd6c295dce746113be1a0781bae33e");
        }

        private static void Case_05617()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5617,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,9,44,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,18,36,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-13,45,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,2,87,25,3), new GeneratedEnemyUnit(7,-8,43,6,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "0e678c4ddf7675cad5616653de16e80edde7d91ec92a027e31b7f9c316c14bdd");
        }

        private static void Case_05618()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5618,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-18,83,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-14,7,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,0,71,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-14,49,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-14,73,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-12,35,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-14,23,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,15,49,4,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "19bab3a22c4f403d7d6485ca7461adae88cb919b77b18fbf448665eb6d890404");
        }

        private static void Case_05619()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5619,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-1,81,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,8,58,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,20,100,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,10,37,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,1,55,27,3), new GeneratedEnemyUnit(-5,-7,70,20,3), new GeneratedEnemyUnit(10,-17,19,46,4), new GeneratedEnemyUnit(1,-5,7,34,3), new GeneratedEnemyUnit(-1,11,45,37,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "b43b743da237939584b80412e2e4eb1e6b2c0387c984d0743372f3de8c5d4065");
        }

        private static void Case_05620()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5620,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,6,42,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,8,8,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,8,100,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-3,5,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,15,96,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,11,33,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-10,43,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,0,24,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,5,9,3,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "fd3bb88ed217c29755f3b5d5e77adc4eaa685a3856944cbe016a709e4b425756");
        }

        private static void Case_05621()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5621,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,1,9,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,16,13,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-7,44,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,6,85,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-5,20,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-3,21,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-16,31,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-8,78,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-12,11,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2319d45c8e62338ae5009db46604c3dda9fb2772dd0a5eefa822611bd97e9093");
        }

        private static void Case_05622()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5622,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,11,23,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-16,15,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,11,84,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,16,82,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,1,56,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-18,16,14,3), new GeneratedEnemyUnit(7,-10,78,17,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "88e5d0eb6f5c8d839772a48e28e67238f580c945c6437b967a10a166d41014cc");
        }

        private static void Case_05623()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5623,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-16,22,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-1,18,13,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "9927230779bf9e3dd485639203d5eba2535a986f9d862cb2be2d2569a8628466");
        }

        private static void Case_05624()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5624,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,1,100,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,11,16,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,20,76,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-3,21,9,4), new GeneratedEnemyUnit(-16,19,20,6,1), new GeneratedEnemyUnit(12,9,23,22,3), new GeneratedEnemyUnit(20,1,53,9,3), new GeneratedEnemyUnit(6,-14,16,11,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "f2bdb9c4701bda6683604744d6eafdbfefa281c573c03f994ad7bf2f3fc9b148");
        }

        private static void Case_05625()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5625,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,17,74,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-16,74,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,15,74,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-6,54,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-15,89,20,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "a988bd635b9bf882ccbaac33b6b2af58154fe397abeb9b9e5dea848ba217873b");
        }

        private static void Case_05626()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5626,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,10,63,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-9,95,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,7,50,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-4,15,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,12,66,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,9,45,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,17,11,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-19,21,12,1), new GeneratedEnemyUnit(-2,-20,18,6,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "32aa7fb846a7cb1c267db908d3dc837289cbead05485e92e818f2c7add81f24d");
        }

        private static void Case_05627()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5627,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,13,91,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,0,17,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,13,53,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-18,77,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,13,15,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-3,35,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,17,86,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-15,45,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,7,60,48,3), new GeneratedEnemyUnit(12,-4,69,39,1), new GeneratedEnemyUnit(-1,12,15,38,2), new GeneratedEnemyUnit(14,-16,12,27,4), new GeneratedEnemyUnit(-3,12,52,38,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "7b5450192fe6a7e33b1057e384cca27d8bedd682400f9b277de49dd092f15f18");
        }

        private static void Case_05628()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5628,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-5,28,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-3,79,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,19,98,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,1,87,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-11,76,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-17,47,44,4), new GeneratedEnemyUnit(-10,-9,48,8,4), new GeneratedEnemyUnit(-5,-18,54,17,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "99483294f3f549d09819952aa00518cb3e6e848239b4d8c07ba4c5d278ce2c47");
        }

        private static void Case_05629()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5629,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-14,61,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-6,62,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,18,89,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-13,89,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,11,86,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-11,6,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-15,23,11,1), new GeneratedEnemyUnit(-12,19,44,31,2), new GeneratedEnemyUnit(18,15,34,5,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "7454814d0de8c1d7bdf19f3dd55d45acaddbc191c394b90b842a81ef18bdcb51");
        }

        private static void Case_05630()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5630,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,6,74,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,4,73,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-20,72,46,3), new GeneratedEnemyUnit(14,15,60,37,1), new GeneratedEnemyUnit(14,1,23,10,4), new GeneratedEnemyUnit(-15,-13,83,28,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "6d8958bd02a936fb1bb2faf6aafc989dca36b70f1f4a0935001f09c8a0d2d712");
        }

        private static void Case_05631()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5631,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-8,67,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-7,14,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-7,8,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-4,82,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "4c769c2c92038106f8feb272dd4742af707aec2d51a1a1feb39106fc2a228438");
        }

        private static void Case_05632()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5632,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-13,29,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-17,15,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,4,38,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,19,76,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,1,87,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-16,26,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-2,94,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-8,74,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,0,63,2,4), new GeneratedEnemyUnit(13,5,72,19,2), new GeneratedEnemyUnit(-7,-17,86,24,4), new GeneratedEnemyUnit(5,20,91,31,3), new GeneratedEnemyUnit(-19,-8,79,38,4), new GeneratedEnemyUnit(-19,-3,48,31,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 35,
                stableHash: "0e49ee8821075f4198f6cdabe1920dc3095483944ac376a53c0957b0f7a0e1bc");
        }

        private static void Case_05633()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5633,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-8,91,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,7,95,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-4,72,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-17,28,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,19,6,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,10,43,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,4,96,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-2,35,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,12,53,37,1), new GeneratedEnemyUnit(-14,-3,53,32,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "77f89b772c6e478c7fc979859cb79258df0060186aa452591f0bc82ee19ab19c");
        }

        private static void Case_05634()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5634,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,19,43,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-6,68,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-14,90,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-2,91,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,6,80,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-3,46,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,3,34,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-17,8,11,4), new GeneratedEnemyUnit(3,5,60,43,3), new GeneratedEnemyUnit(-14,-5,56,12,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "f760829fcdd8447fd053aa6be67690c588c2316dee0e970d54d49a49c02eea4a");
        }

        private static void Case_05635()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5635,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,0,64,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,3,32,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,3,47,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-20,89,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,12,76,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-4,67,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,8,65,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,16,38,42,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1d55eae2882c029e3958176b00271730a2f3e0baffcace3f9c94880c4f836bd4");
        }

        private static void Case_05636()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5636,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,19,8,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,14,7,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,15,45,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-3,52,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,17,70,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,12,14,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,20,30,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-19,58,7,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "63becf3767cc5235e4dbda398eea4bd0419149fd67d4255235444e050768683d");
        }

        private static void Case_05637()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5637,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-4,36,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,8,8,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-18,81,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-15,8,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-20,98,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-5,75,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-6,75,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-1,86,31,2), new GeneratedEnemyUnit(-2,3,29,6,3), new GeneratedEnemyUnit(12,0,17,18,4), new GeneratedEnemyUnit(8,-6,75,38,1), new GeneratedEnemyUnit(17,-7,13,25,2), new GeneratedEnemyUnit(10,-8,66,12,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "37b19853b4bbcb49e09c62c6567d87f859f17eb524bc81239e863544dae6c5f4");
        }

        private static void Case_05638()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5638,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,7,36,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-14,40,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,11,19,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-8,20,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-7,30,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,15,71,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-5,18,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-20,100,22,4), new GeneratedEnemyUnit(16,-6,23,9,2), new GeneratedEnemyUnit(-16,-16,61,30,1), new GeneratedEnemyUnit(2,2,52,25,1), new GeneratedEnemyUnit(12,-2,83,28,3), new GeneratedEnemyUnit(-19,19,57,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "433224405e8593987b42770da637d4754eb12e04d1bfee0c8c15379c1aea31f0");
        }

        private static void Case_05639()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5639,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-5,92,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,15,15,35,3), new GeneratedEnemyUnit(14,-7,71,14,3), new GeneratedEnemyUnit(-15,3,85,7,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "a921ed47c8dc8f6802f2987a20bb04995cdce6b09f7b1601a761c28ba68b33ff");
        }

        private static void Case_05640()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5640,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,19,18,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-16,46,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-5,27,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-4,70,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,1,84,13,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f218b4121b907d8e8153088aff9540d153638a0d506635432258d387abd7dcaf");
        }

        private static void Case_05641()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5641,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,13,12,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,12,94,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-3,98,32,2), new GeneratedEnemyUnit(-15,-1,9,22,3), new GeneratedEnemyUnit(-5,18,97,46,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "9a3ee0fcf279a9678ee260061be9664bf73908993c45e51417ff9ab8495784b0");
        }

        private static void Case_05642()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5642,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,13,46,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-4,63,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-14,57,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-6,39,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,0,94,38,4), new GeneratedEnemyUnit(-16,-8,32,18,2), new GeneratedEnemyUnit(18,-20,53,44,3), new GeneratedEnemyUnit(-6,7,71,39,2), new GeneratedEnemyUnit(9,20,20,48,1), new GeneratedEnemyUnit(6,19,54,38,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "c67c0af2072bae661c21c0fbacc92f6f4dbaca33c8339fec0009a364fe23802d");
        }

        private static void Case_05643()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5643,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-5,14,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-11,95,34,2), new GeneratedEnemyUnit(4,9,31,25,3), new GeneratedEnemyUnit(7,14,53,15,2), new GeneratedEnemyUnit(-5,6,32,28,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "acae359e6ad18ca3899a1e6e99911fc98752a0cf2f91ea4bd38f56240fc305c9");
        }

        private static void Case_05644()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5644,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,13,36,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-8,94,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,6,42,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,20,54,16,2), new GeneratedEnemyUnit(-8,-16,33,23,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "47c682a818bd72dc4f627dd9841af66cc70a7850e1d160efad14d2769f549146");
        }

        private static void Case_05645()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5645,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,3,84,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-16,97,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-11,83,50,1), new GeneratedEnemyUnit(3,-9,92,49,4), new GeneratedEnemyUnit(-14,6,18,14,3), new GeneratedEnemyUnit(-1,15,49,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "9527998d9bffcf6b942a0161a12031e07173b29ed18ecb89b6e8ac9bba35a489");
        }

        private static void Case_05646()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5646,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,1,19,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-4,71,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,3,33,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,5,63,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-5,73,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-17,78,33,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "29b5dd1a048bcf42dd6f20a27c5e3d62edd0c8de532bfda51df171beb34cc0f1");
        }

        private static void Case_05647()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5647,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,10,33,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-3,43,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-19,29,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-15,88,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,15,51,2,2), new GeneratedEnemyUnit(-8,-18,72,49,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "93f81d4960c5523f918ae34f30a3f741220b2fd6828a82cddc39bc25c0344142");
        }

        private static void Case_05648()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5648,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-12,20,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-5,89,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,1,15,43,2), new GeneratedEnemyUnit(-12,3,56,2,4), new GeneratedEnemyUnit(15,1,10,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "d6098afeecf71d610b67887beab4459338f3ecbf759e5c0e087c84114940a38c");
        }

        private static void Case_05649()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5649,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,4,77,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,12,65,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-4,99,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-17,38,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,15,22,43,1), new GeneratedEnemyUnit(-14,15,16,2,2), new GeneratedEnemyUnit(-14,-17,48,17,3), new GeneratedEnemyUnit(7,7,42,48,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "149dc39b7bed4722709204d1b041495ff3c2f0295b62d9cd85120ac18cd40dca");
        }

        private static void Case_05650()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5650,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,16,96,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,5,81,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-11,78,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,3,97,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,9,74,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,15,66,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,3,64,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "2de11a918029201d81884d3e30d6668b90942a5cbb08f7f20a89de225cacff2e");
        }

        private static void Case_05651()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5651,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,17,96,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,20,16,28,4), new GeneratedEnemyUnit(-15,2,85,3,1), new GeneratedEnemyUnit(-7,-5,5,1,4), new GeneratedEnemyUnit(10,-19,60,40,2), new GeneratedEnemyUnit(4,-17,71,16,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "07c6134afa834e3c211ed6e4199f20f0eca27f6abdab260df5cf42c5ec65f4f9");
        }

        private static void Case_05652()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5652,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,15,17,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,16,66,42,2), new GeneratedEnemyUnit(-15,-5,72,2,1), new GeneratedEnemyUnit(6,20,88,17,4), new GeneratedEnemyUnit(9,-11,78,29,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "5f6af1a429022b0f574cbc9fa694680693ae6884f9fee2e5359bf12b853514c9");
        }

        private static void Case_05653()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5653,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-13,35,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-14,86,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,5,65,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-17,12,4,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "92c1aacfbb473b5b12fc99052f4ebda5627e596d4104649f9a465b0f2eb0b669");
        }

        private static void Case_05654()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5654,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,9,32,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,15,85,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,8,77,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,12,8,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,20,33,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-1,89,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e891c43fda5f28531be998ad9084c8e80bbe2cd641340d26ea3ec1ff3fec9488");
        }

        private static void Case_05655()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5655,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,18,53,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,18,41,40,4), new GeneratedEnemyUnit(-16,20,41,42,3), new GeneratedEnemyUnit(-18,-15,33,17,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "6417ae955a9499faac2a72c7f688d549faf6679b9a3cbf7cd54c9da179d80044");
        }

        private static void Case_05656()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5656,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,16,53,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,2,60,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,7,64,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-4,44,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,18,27,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-15,86,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,2,93,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-13,36,27,1), new GeneratedEnemyUnit(8,-8,57,43,4), new GeneratedEnemyUnit(-19,0,55,24,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "b72b180387c2ef49c59fecca9ab7442123519abb13ca53f2eb76f03415ccd408");
        }

        private static void Case_05657()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5657,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,11,85,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-7,23,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,13,71,4,1), new GeneratedEnemyUnit(1,2,93,34,2), new GeneratedEnemyUnit(16,8,49,21,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "cbf1dce6ff8a6f170ea9bc238461cf1fcb45a1704aa9ea942756b57055a4a4d6");
        }

        private static void Case_05658()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5658,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-15,85,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,14,32,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,12,97,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-3,86,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-3,93,18,4), new GeneratedEnemyUnit(15,3,25,19,3), new GeneratedEnemyUnit(0,6,23,13,4), new GeneratedEnemyUnit(9,6,73,28,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "caf9cdb64eee7693c2e227ee2b02ae43bcce9cc93e7362df9a530b656beeb1d1");
        }

        private static void Case_05659()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5659,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,4,42,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,4,34,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-10,80,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-14,38,23,2), new GeneratedEnemyUnit(0,-6,51,28,1), new GeneratedEnemyUnit(-2,-6,30,40,2), new GeneratedEnemyUnit(-4,14,52,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "7b426f7370adca3a5c8f48a37f5bcd6401c01e7f1026108a61c13eefa44b2f0b");
        }

        private static void Case_05660()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5660,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,8,50,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-4,93,1,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "07b4c99d06f0116e954e140abaab7daa12aeb377a8a56b592bccc8003bed91e3");
        }

        private static void Case_05661()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5661,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,14,17,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-20,94,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,4,37,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-2,87,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,9,12,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,13,53,50,2), new GeneratedEnemyUnit(-4,18,19,35,4), new GeneratedEnemyUnit(-3,-17,64,49,1), new GeneratedEnemyUnit(11,-16,8,29,1), new GeneratedEnemyUnit(-13,5,65,20,1), new GeneratedEnemyUnit(-19,2,28,27,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "3e1dd365baee7b85ae120076a93f043cb7cda38bb068ba42d4e56d6ceec76a30");
        }

        private static void Case_05662()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5662,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,5,35,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,10,57,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-8,57,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-10,16,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,5,48,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-7,29,4,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "1f981d642488e0ae4e019557b09a3962b3f56ab964e2bd67ae0ff3a06c90bd04");
        }

        private static void Case_05663()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5663,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-14,45,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-13,78,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-2,69,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-15,37,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-15,69,37,1), new GeneratedEnemyUnit(19,-18,25,18,3), new GeneratedEnemyUnit(-2,-2,53,49,2), new GeneratedEnemyUnit(12,4,5,23,2), new GeneratedEnemyUnit(9,5,68,36,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "bbcd1bee2841f711784830c6f5a8599b001bb142a8c16accd8a73673df85e755");
        }

        private static void Case_05664()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5664,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,17,40,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-19,27,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-5,97,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-12,7,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-1,55,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,18,24,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-5,31,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,17,59,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,10,58,41,4), new GeneratedEnemyUnit(-19,-10,97,12,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "4e81d30c5c408ec7c71d4460ef6a35eac247a6e9c7c3e37a1aa66e3d6e0e2e82");
        }

        private static void Case_05665()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5665,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-15,31,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,3,42,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,15,50,47,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "839937ec5388f1dc7e0dc8109300f72a2010bfd5c467c20345345e741c21df74");
        }

        private static void Case_05666()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5666,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,6,36,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-9,37,2,2), new GeneratedEnemyUnit(-16,0,24,28,4), new GeneratedEnemyUnit(20,10,43,22,2), new GeneratedEnemyUnit(2,-19,48,39,2), new GeneratedEnemyUnit(2,8,53,20,4), new GeneratedEnemyUnit(15,-8,21,4,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "bcd11b8391deaba6ddb3646b08126e9f8cb737f306907a271bc94823d7c986bf");
        }

        private static void Case_05667()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5667,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,1,87,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-13,86,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-1,66,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,1,57,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-6,75,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-16,9,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,2,95,36,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f8cb2e9cc9285a091fbc816f7fe14858e01e90155930b0baa8d093cc454aaee7");
        }

        private static void Case_05668()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5668,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-16,41,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-5,42,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,3,60,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-1,88,47,2), new GeneratedEnemyUnit(17,-19,100,28,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "50fda6ca94d34594adfd66a41b54fd1f48013f7010d9a725f8fcf5e11009015c");
        }

        private static void Case_05669()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5669,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-15,14,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-4,74,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-17,74,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,16,97,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,19,38,31,1), new GeneratedEnemyUnit(-8,10,81,20,1), new GeneratedEnemyUnit(-1,20,71,14,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "5ab35c9847eec00c995d7a1f6aa9f1fa162c5554936b00e9eb477c8281b45f58");
        }

        private static void Case_05670()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5670,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,9,70,5,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "b61c4470f5ca4796fb89defb5809ea19b900f93e5d337c3379fa95821198f781");
        }

        private static void Case_05671()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5671,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,16,53,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,15,84,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-6,60,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,2,46,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,20,90,34,3), new GeneratedEnemyUnit(-20,-18,53,16,3), new GeneratedEnemyUnit(8,3,20,35,3), new GeneratedEnemyUnit(11,-1,19,42,3), new GeneratedEnemyUnit(14,0,99,33,1), new GeneratedEnemyUnit(18,-20,32,14,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "9f154b99099724fcd8d95059386777a55d73aace478253b90e893a210a77e12b");
        }

        private static void Case_05672()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5672,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,3,20,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,9,20,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,12,65,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,8,9,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-16,60,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-11,16,14,1), new GeneratedEnemyUnit(10,-12,65,7,3), new GeneratedEnemyUnit(-20,11,30,26,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "29c0a1b994787f3c45383fd04b06856f7eab79c49c5b2377bedce9ea6d6c317d");
        }

        private static void Case_05673()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5673,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,8,20,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-2,65,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-10,59,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,18,39,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-6,54,3,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "160f0fd11b7dc4635ea0f8c51f4499c9bdeffe6bd98e4d844ac15a7cfe79dd50");
        }

        private static void Case_05674()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5674,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,11,58,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-9,69,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,5,11,1,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "8ffe857b42ecde232ed44f437140aa6ab16045f3eb8705a5e554dbe547bfbba9");
        }

        private static void Case_05675()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5675,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,18,20,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,1,87,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-19,72,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,18,96,26,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "0a9dabdc7a3a2ce6d74a22369db5301017981266b0f942c71a77d189e7f74952");
        }

        private static void Case_05676()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5676,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-19,23,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,20,85,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,16,40,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-9,5,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,0,68,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,8,49,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,7,52,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,4,35,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,19,53,13,3), new GeneratedEnemyUnit(9,7,28,47,1), new GeneratedEnemyUnit(2,-9,20,14,4), new GeneratedEnemyUnit(-17,14,73,17,1), new GeneratedEnemyUnit(8,-13,29,35,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "d7fe29a27d27796c089ff69391014552ac98e6554d0a257807306c4cd2c684e0");
        }

        private static void Case_05677()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5677,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-17,55,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,6,52,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,15,77,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-12,65,32,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "01c4e8e138efa75603ec419f932e64477a37f84ad4d7c2c61fbb8752819d6350");
        }

        private static void Case_05678()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5678,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-7,12,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-14,57,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-3,24,17,4), new GeneratedEnemyUnit(1,9,31,35,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "9cebdcf766bc727ef6813b9555471a97bd388b5b7b991cd4aed84ae3f1a5971f");
        }

        private static void Case_05679()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5679,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-10,13,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,11,67,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,11,16,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-9,34,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-14,63,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,16,96,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,19,56,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-7,65,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,7,100,23,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "dab1e05a4dccbcf8c0bba0f44038086a382c65a74918733f033b700dd76cb587");
        }

        private static void Case_05680()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5680,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,10,5,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,10,55,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-10,35,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-3,15,19,3), new GeneratedEnemyUnit(-17,-6,5,29,3), new GeneratedEnemyUnit(17,18,31,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "b9943b5fd1c5e5dd33706bb442b74721cd59a0367cbc2ca46c19c3249ac567cc");
        }

        private static void Case_05681()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5681,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-12,46,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-3,42,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,20,41,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-14,71,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-10,13,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,16,97,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-3,85,6,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "cc35734a06a5335c82a3f10149f466baa5de06bf0c84c06bef4241053c60f29c");
        }

        private static void Case_05682()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5682,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,7,17,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,8,9,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-4,11,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,12,59,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-16,11,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,18,48,13,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "97cfe446585ccae9032cd339e1c1f46c7e9df8560b58f64f6ed3405002ee1ec0");
        }

        private static void Case_05683()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5683,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-5,98,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-5,69,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-9,22,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,0,8,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,12,39,35,4), new GeneratedEnemyUnit(5,13,74,20,4), new GeneratedEnemyUnit(5,4,25,35,4), new GeneratedEnemyUnit(-19,4,79,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "678260ab79838a8f9e895aced67add463ff23de5895c5c85f0a80dbe1219c33a");
        }

        private static void Case_05684()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5684,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-20,25,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,4,52,14,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "e731c153f97e9e5331b4208575dd54545d46bbc61ff24544e782d89bb4ac4417");
        }

        private static void Case_05685()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5685,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-20,37,4,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "a705ef4a836fe15b971e7ad6cfaf2f98446f68d7dfedb655806be0789dada9c5");
        }

        private static void Case_05686()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5686,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-8,85,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-19,94,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,13,26,41,1), new GeneratedEnemyUnit(19,-16,83,44,1), new GeneratedEnemyUnit(-10,10,72,42,3), new GeneratedEnemyUnit(-8,6,82,17,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "0a4efd4e760fcd2590c9ee2f2c4746ca541271885393fcc773a012aa3e4e0dda");
        }

        private static void Case_05687()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5687,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,3,30,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,2,70,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-7,57,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-18,6,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,20,12,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-13,20,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-4,99,30,2), new GeneratedEnemyUnit(-10,7,81,9,2), new GeneratedEnemyUnit(11,-15,58,12,4), new GeneratedEnemyUnit(2,0,59,28,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "b2acbc9f9cba710073478cf69703489cbe9f9e1f7ed6917c07f13b5556543d2b");
        }

        private static void Case_05688()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5688,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-18,36,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,18,46,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,5,86,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-19,53,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,18,88,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,10,37,29,2), new GeneratedEnemyUnit(13,4,94,48,4), new GeneratedEnemyUnit(-4,13,32,29,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "25da5432820b7ebbc6895537ce4039edbc99f812f323445b2fed05bc622ce8ed");
        }

        private static void Case_05689()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5689,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-13,38,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,4,46,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-19,76,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-11,11,7,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "e28249fdabe38e305d9237ce05d532f5175f6598a95345e7a259afb50f788967");
        }

        private static void Case_05690()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5690,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,17,18,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-12,20,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,0,11,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-18,44,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,8,90,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,11,38,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-7,48,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-16,34,11,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "e9360963eb6ba0bef622c1aa2898dabdb2eceec77bb77fa5073754013e4303d1");
        }

        private static void Case_05691()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5691,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-8,53,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-1,60,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,4,92,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-5,67,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-11,64,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,2,27,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,18,60,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-20,27,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,2,38,26,3), new GeneratedEnemyUnit(16,20,15,43,1), new GeneratedEnemyUnit(15,-3,74,34,2), new GeneratedEnemyUnit(2,-11,9,20,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "ed78472f47040aef2a5a9f2189cdb73df03e575ddbaa06e4bfcd03eaaccd744f");
        }

        private static void Case_05692()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5692,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,5,42,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-10,98,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-16,89,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-1,61,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,1,90,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-20,80,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-2,62,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,4,50,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-17,63,49,3), new GeneratedEnemyUnit(10,13,53,5,3), new GeneratedEnemyUnit(3,-17,58,13,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "aa429a69976836f4aba89b647b968379f8e174bc639ef11adfe478fd402d84e5");
        }

        private static void Case_05693()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5693,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-5,14,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-7,99,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-8,62,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-9,53,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-11,60,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-11,74,36,4), new GeneratedEnemyUnit(15,-15,66,30,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "d731bb3200a4b3b3b8421bfd1d65c17a38b236f45db1cbb2a2066bc231de8728");
        }

        private static void Case_05694()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5694,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,1,5,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,19,96,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-14,70,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-1,10,7,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "6b989ef6be88f071d20bf7b27c0876567a3ec8cffdb7eebd7cce46dd87a0de6d");
        }

        private static void Case_05695()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5695,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,9,77,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,20,54,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,13,64,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-13,100,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-9,67,1,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "3f5910571b3c549eaed056d1c13461add4995d3a0bdde0959e98aac95c66f406");
        }

        private static void Case_05696()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5696,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,15,22,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-5,60,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,19,97,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,15,92,25,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "37286d17304c203676b9a6634a91cecf299220104a29b7d319b67c7c571e95db");
        }

        private static void Case_05697()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5697,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-11,10,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-15,63,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,8,23,6,2), new GeneratedEnemyUnit(0,14,22,6,3), new GeneratedEnemyUnit(20,8,96,9,2), new GeneratedEnemyUnit(20,-17,15,47,1), new GeneratedEnemyUnit(12,-3,91,42,2), new GeneratedEnemyUnit(-15,1,28,19,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "0bbf2cf39c8b2dd6875ab8017aaa37eebc5db6c2ecc55e21b9260daa2f72e76a");
        }

        private static void Case_05698()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5698,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-15,91,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,17,11,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,18,59,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-17,46,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-2,62,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,16,50,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-11,38,42,1), new GeneratedEnemyUnit(10,10,13,48,3), new GeneratedEnemyUnit(-17,-2,87,44,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "bc73156278e305a9a2dee6f5f7b673235a1adfa09e23c5b899f4d4d642132f79");
        }

        private static void Case_05699()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5699,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-3,12,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,1,53,14,1), new GeneratedEnemyUnit(7,-20,16,22,4), new GeneratedEnemyUnit(-15,-9,24,48,2), new GeneratedEnemyUnit(-18,-17,62,34,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "c3fef9ac1b660aea6de69ab47c01ed9e298434eff057ea09aa86441982909f7f");
        }

        private static void Case_05700()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5700,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,10,100,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,8,99,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,0,13,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,11,98,15,4), new GeneratedEnemyUnit(0,-7,40,17,2), new GeneratedEnemyUnit(8,2,32,12,1), new GeneratedEnemyUnit(-6,-18,47,33,1), new GeneratedEnemyUnit(8,19,89,42,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "ccea4a13133726b7eef8614b69a54a83ad8afc73f5510d42fbf306dc09a4c88b");
        }

        private static void Case_05701()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5701,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,14,92,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-7,62,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-9,10,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,20,28,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-16,6,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,13,95,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-4,65,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,17,99,32,2), new GeneratedEnemyUnit(-6,2,18,3,2), new GeneratedEnemyUnit(-8,6,72,1,3), new GeneratedEnemyUnit(-8,20,69,3,4), new GeneratedEnemyUnit(-13,6,43,3,2), new GeneratedEnemyUnit(-2,-2,31,13,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "9508a4b329df5b49d3c4ec15bce01009f3b6df1b182a29003917a598fedaca7a");
        }

        private static void Case_05702()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5702,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-4,22,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,3,43,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,6,83,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-10,95,28,2), new GeneratedEnemyUnit(10,8,35,28,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "70f5772cb0a8ff819a72f3782b6edc987cfcbb97b7d64cc9f2e4993661b73f8d");
        }

        private static void Case_05703()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5703,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,6,74,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,12,100,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,9,90,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-12,40,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,13,21,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,18,13,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-11,68,11,2), new GeneratedEnemyUnit(11,2,29,1,3), new GeneratedEnemyUnit(-11,-10,23,25,2), new GeneratedEnemyUnit(-3,-14,80,42,1), new GeneratedEnemyUnit(-7,20,15,25,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "0575592738b3b6f158d7e6418b249c271c08eb1230bb92db15ff5ad9e9666ecd");
        }

        private static void Case_05704()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5704,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-1,31,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-14,81,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-2,30,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-16,11,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-11,43,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-20,29,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,14,91,16,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "6d5fa223c4619b2a6a7492c2302162af56103cc9faa3be99a090927a96f86a95");
        }

        private static void Case_05705()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5705,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-8,40,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,2,34,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-2,72,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,0,42,38,2), new GeneratedEnemyUnit(6,-19,40,42,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "9ff07815d40688e035f97069d907734fd67ae718249b52c9ebd1b6057740d67e");
        }

        private static void Case_05706()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5706,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,9,74,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-3,30,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-7,99,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,0,89,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,9,92,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,19,6,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-19,45,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-19,69,22,3), new GeneratedEnemyUnit(-16,-2,64,29,1), new GeneratedEnemyUnit(-9,-16,66,24,2), new GeneratedEnemyUnit(18,17,72,50,2), new GeneratedEnemyUnit(2,-2,18,30,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "cd802838dc58e1e7ece28b51dbaae38dcc3e622293078d85b5884c4831fb05d1");
        }

        private static void Case_05707()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5707,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,15,83,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,12,72,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-12,35,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-20,28,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,4,46,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-9,75,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-5,100,49,2), new GeneratedEnemyUnit(2,5,91,28,1), new GeneratedEnemyUnit(-6,-2,35,42,1), new GeneratedEnemyUnit(9,9,78,14,3), new GeneratedEnemyUnit(5,13,91,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "de7d3ace918625912a036677cf538c077bb97123100fdf8035045e8fc3ed1cea");
        }

        private static void Case_05708()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5708,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,12,96,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,4,61,15,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "a3cd9cd3acf961de1308782a8ff436cd57353e582338f3fe9f8c3ad1e7b911dc");
        }

        private static void Case_05709()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5709,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,17,24,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,17,85,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,2,7,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,7,78,19,4), new GeneratedEnemyUnit(19,17,35,15,1), new GeneratedEnemyUnit(-7,-14,6,28,1), new GeneratedEnemyUnit(-11,-1,46,28,4), new GeneratedEnemyUnit(-6,5,11,20,4), new GeneratedEnemyUnit(-13,5,95,36,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "6b84c02dc52c947de6439fd74c08ab6d77b4bd416aa2b10636e159e053708f51");
        }

        private static void Case_05710()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5710,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,16,59,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-10,40,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-1,45,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-9,57,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-15,52,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-17,56,6,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "0c769e565dbd7239e8ea4f80eafa0d0a0a08d9545cce7bba57d3bc6045b5ad93");
        }

        private static void Case_05711()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5711,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-20,58,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-8,71,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-6,19,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-6,8,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,16,56,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-15,69,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,0,90,41,3), new GeneratedEnemyUnit(15,14,18,19,2), new GeneratedEnemyUnit(-1,18,27,38,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "0c6155abf13a8f27b2b480e983671417f910f4eec51151097f616071988634d1");
        }

        private static void Case_05712()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5712,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-2,88,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-11,30,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-3,40,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,16,6,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,12,12,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-9,20,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-15,78,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,4,43,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-10,65,18,3), new GeneratedEnemyUnit(-1,-16,64,24,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "169de7b293e49a5fa9797854e3490b82213ffaa5687538a11c879e28cf01ea04");
        }

        private static void Case_05713()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5713,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-11,63,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,19,44,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-12,44,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,4,91,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-2,6,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,3,6,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-8,18,50,2), new GeneratedEnemyUnit(-13,-6,83,38,4), new GeneratedEnemyUnit(9,-11,64,22,1), new GeneratedEnemyUnit(-11,-14,95,5,4), new GeneratedEnemyUnit(13,-8,98,48,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "bfa687fda7eb0cf30274c878a9a4b3e6fdc2e99b7ae8d3fc76341fa64067e430");
        }

        private static void Case_05714()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5714,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-11,88,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,14,78,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-2,32,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-1,74,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,20,54,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-9,21,28,2), new GeneratedEnemyUnit(5,-6,62,18,4), new GeneratedEnemyUnit(-5,0,32,10,3), new GeneratedEnemyUnit(11,-10,33,30,2), new GeneratedEnemyUnit(-18,10,70,38,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "c0b3689a36e5dff1fdc2727a68e2aa8d1a7033f1aff404df192d2f093b1f927b");
        }

        private static void Case_05715()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5715,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-15,90,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,6,22,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-19,39,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,8,11,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,10,61,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,8,40,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-10,22,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-10,84,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,18,51,27,3), new GeneratedEnemyUnit(-7,13,86,48,1), new GeneratedEnemyUnit(-10,13,77,32,2), new GeneratedEnemyUnit(19,14,14,4,1), new GeneratedEnemyUnit(-6,-15,64,17,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "4dfe06878981417dc73e648550af755770cf8c62c33629081a0629d99aa22b53");
        }

        private static void Case_05716()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5716,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,10,7,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-14,80,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-4,23,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-7,90,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-18,59,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-18,56,3,3), new GeneratedEnemyUnit(3,-15,36,40,4), new GeneratedEnemyUnit(0,-17,18,13,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "85795d2eb6ede259881675ef5dfa087768268f5ccf57ea36b6d5ce462ed8f8f5");
        }

        private static void Case_05717()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5717,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,6,93,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,7,61,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-17,38,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-18,37,3,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "5bbacaefbb85a0c026f481370d18bf817826e46b677550dc8e04839d20e70b4d");
        }

        private static void Case_05718()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5718,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-10,46,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,12,37,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-11,61,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,11,93,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,2,48,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-1,62,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-1,75,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,19,97,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,1,39,46,3), new GeneratedEnemyUnit(-11,3,29,48,2), new GeneratedEnemyUnit(-14,11,26,21,3), new GeneratedEnemyUnit(-17,-12,42,25,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "b0b517cfcbf80d72f8efa3999ad7cbe48ccb5a1d98c869d0629690d9303ec71f");
        }

        private static void Case_05719()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5719,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,0,16,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,16,42,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-12,33,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,5,60,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-19,56,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,17,27,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,1,6,50,3), new GeneratedEnemyUnit(-11,-1,95,27,4), new GeneratedEnemyUnit(-20,6,26,19,3), new GeneratedEnemyUnit(17,9,76,38,1), new GeneratedEnemyUnit(11,-7,19,41,3), new GeneratedEnemyUnit(10,-2,6,40,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "153f364acaa9a327ed4d4da9ff775abd56b6b39efdf08c7461b8aa123b4484e9");
        }

        private static void Case_05720()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5720,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,0,72,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,16,16,16,3), new GeneratedEnemyUnit(5,-20,100,9,1), new GeneratedEnemyUnit(18,7,56,19,4), new GeneratedEnemyUnit(10,-7,83,5,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8564eea346d6e5fb329d6c38bc074e2b34c34d911faf783835858a95ff2b6e86");
        }

        private static void Case_05721()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5721,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,3,93,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,5,74,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,18,21,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-15,90,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,16,46,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-14,52,33,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "5b50ab4385f531c5a2a54227f37fed218d545db31d33d5f176970dedaf1c01ae");
        }

        private static void Case_05722()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5722,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,13,29,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,5,43,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-6,36,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,16,75,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,14,71,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,17,33,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-10,58,2,4), new GeneratedEnemyUnit(-16,14,39,16,3), new GeneratedEnemyUnit(18,4,79,39,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "8da62cce757a4e1ed6b17c1fba070b37cd3c1d21d4fbeeb90b950167c2f47edd");
        }

        private static void Case_05723()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5723,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-10,72,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,11,78,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-4,91,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-17,44,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,13,50,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-12,43,17,1), new GeneratedEnemyUnit(-18,5,61,1,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "01bcc706eff33f967358887d30b466b4d6efbe18cb4967f48b909c949e9ea52a");
        }

        private static void Case_05724()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5724,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-9,85,2,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "24d2122d9ab00220870159bf7b82ad12793053a985a8b0fdd6a9d59a649b2ca6");
        }

        private static void Case_05725()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5725,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-7,70,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-17,52,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,13,88,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-4,26,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,1,46,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,9,59,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-1,94,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,19,68,15,2), new GeneratedEnemyUnit(18,5,69,1,4), new GeneratedEnemyUnit(13,-16,28,34,2), new GeneratedEnemyUnit(15,1,30,2,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "ec3b3ec00a91b0ee263e2d097bc113b7173c1281942f94e93f3fb7553e7638fb");
        }

        private static void Case_05726()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5726,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-1,71,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-19,11,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-10,86,4,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "86735157295d3d2af34d6cb94ecbbbb8dfde976bc9318f0d2a05e07a12a7868f");
        }

        private static void Case_05727()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5727,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-18,71,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-10,84,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-12,83,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-5,36,12,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "89fcd0d327da4e14c3d567aed1a80a099a4891b69d3ec8520282ac8cdf92b3f8");
        }

        private static void Case_05728()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5728,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,13,34,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,15,56,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,18,94,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,5,11,5,4), new GeneratedEnemyUnit(15,7,88,24,2), new GeneratedEnemyUnit(-10,-13,95,41,3), new GeneratedEnemyUnit(-12,8,61,13,4), new GeneratedEnemyUnit(5,-3,38,37,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "d9df553171e77d51c9c45f31e2dc0dde6d1fbbdff17260838516d296281be60d");
        }

        private static void Case_05729()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5729,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-5,39,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-17,62,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-9,25,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-6,83,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-4,43,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-12,82,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,19,44,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-1,59,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,15,52,17,2), new GeneratedEnemyUnit(7,0,98,22,2), new GeneratedEnemyUnit(3,8,28,30,2), new GeneratedEnemyUnit(-3,19,83,30,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "d3b2407d88b9d5fe960f177bbd96c73651d580e485ba07d968e4d2d330dd438c");
        }

        private static void Case_05730()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5730,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,15,34,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-2,14,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-10,13,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,1,61,4,3), new GeneratedEnemyUnit(3,-17,13,43,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "4ede10f607f0e8de6becbbb2d521d94cf299c4c499b8c6b1ca7cd981a22d69fd");
        }

        private static void Case_05731()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5731,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-9,24,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,2,36,27,2), new GeneratedEnemyUnit(3,-2,70,1,1), new GeneratedEnemyUnit(-5,-10,74,24,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "7b03e7323c5a49ecdf399a3ce24e21b4c2f4e1656519484de61d71d2eb957d37");
        }

        private static void Case_05732()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5732,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-2,10,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-17,25,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,5,6,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,4,80,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,7,60,8,3), new GeneratedEnemyUnit(2,-3,62,3,2), new GeneratedEnemyUnit(-5,13,90,29,3), new GeneratedEnemyUnit(-8,-1,27,23,4), new GeneratedEnemyUnit(15,7,19,8,4), new GeneratedEnemyUnit(-16,-20,30,39,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "f761f547cf4f68b31cc2bb30df22384613297a92fac115afda56edd2d11a8025");
        }

        private static void Case_05733()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5733,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,15,52,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,6,61,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,12,51,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-10,11,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,1,46,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,3,81,20,3), new GeneratedEnemyUnit(-18,10,61,24,4), new GeneratedEnemyUnit(1,-1,48,22,4), new GeneratedEnemyUnit(20,-7,6,30,4), new GeneratedEnemyUnit(0,16,24,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "fd7e59de3c530108622ba166a16c221de51cd8e98fedd68e0b27ef927fc61856");
        }

        private static void Case_05734()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5734,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-14,89,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,5,37,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,17,45,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,0,45,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,11,29,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,19,46,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-12,14,32,1), new GeneratedEnemyUnit(-11,16,6,8,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0e845138b46b8ecf20621c75d3a8ce99e25d464534c4ad7cdbc47c5f1ec7c79e");
        }

        private static void Case_05735()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5735,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,19,89,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-15,43,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,18,88,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,4,44,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,5,51,33,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "0a78b5a537882e69e1de363675df9b6b0c6d6f869f48c7f13c1bbd9872ed4816");
        }

        private static void Case_05736()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5736,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-1,86,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-1,5,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-15,32,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,3,86,48,1), new GeneratedEnemyUnit(14,12,92,24,4), new GeneratedEnemyUnit(-18,6,16,49,1), new GeneratedEnemyUnit(8,-19,9,45,3), new GeneratedEnemyUnit(13,20,14,35,1), new GeneratedEnemyUnit(-9,8,69,40,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "f1ee244561d36708eefe17acd507e3c2355df9489fc504f4089fb22841c5bc5c");
        }

        private static void Case_05737()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5737,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-4,91,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-3,100,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-5,54,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,9,47,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-19,20,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-2,6,34,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ac3d71975c5bc0144db61b750794b654de17a43f93860655f14e4415817b5f6c");
        }

        private static void Case_05738()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5738,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-17,77,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,2,89,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-11,12,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-15,14,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-14,83,33,1), new GeneratedEnemyUnit(18,-15,30,43,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "9355afa8c6d449c3cb4cb4e937d9d8c45293855fd7a6241f82570dfc332d211c");
        }

        private static void Case_05739()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5739,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-11,73,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,13,49,50,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "f194e47a3e0be0cf6603993203a3571f8b44f0b980f5adbf190a8a955a95b169");
        }

        private static void Case_05740()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5740,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,9,29,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-19,79,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,3,89,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,19,31,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,20,85,37,1), new GeneratedEnemyUnit(2,4,52,46,1), new GeneratedEnemyUnit(10,2,15,26,1), new GeneratedEnemyUnit(-14,-19,83,25,4), new GeneratedEnemyUnit(-14,-9,59,50,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "803e852557a4ccdf0de032f0846109c5761324dc06aab1bba017456440f90b5f");
        }

        private static void Case_05741()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5741,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-3,31,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,4,51,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,18,49,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-8,21,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,5,46,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,12,12,50,4), new GeneratedEnemyUnit(18,-1,35,33,2), new GeneratedEnemyUnit(10,-15,98,44,1), new GeneratedEnemyUnit(-11,13,27,21,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "713f3eb09c3c3692453e552a02d55227e41e983c8dca3813c211e004fcf5e267");
        }

        private static void Case_05742()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5742,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-4,56,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-16,10,9,1), new GeneratedEnemyUnit(-16,1,76,2,1), new GeneratedEnemyUnit(-19,1,92,17,3), new GeneratedEnemyUnit(-19,14,96,38,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "ed2e6d7ab33599f7f3634e8b3a56a68eeff842322cb7c3d7cadf7ebf67333b88");
        }

        private static void Case_05743()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5743,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,14,84,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-13,53,4,2), new GeneratedEnemyUnit(16,5,55,43,2), new GeneratedEnemyUnit(3,-1,10,20,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "fee049f0d2f8c4d1d38cd5051c099881044a903bee3b65e418c1430786001af3");
        }

        private static void Case_05744()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5744,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-13,26,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-10,40,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,2,37,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-10,56,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-8,66,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,15,36,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,1,80,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,11,94,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,15,79,17,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "75e14511e1b43dd8af56d2887ef67080a4b62a56508a0ce24ea4a3364300cebe");
        }

        private static void Case_05745()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5745,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,2,82,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-9,22,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-19,73,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,18,32,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,20,96,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-6,17,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,9,48,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,16,79,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,5,46,36,1), new GeneratedEnemyUnit(17,9,21,2,1), new GeneratedEnemyUnit(-2,-9,63,27,1), new GeneratedEnemyUnit(17,-20,6,22,4), new GeneratedEnemyUnit(20,-15,34,19,2), new GeneratedEnemyUnit(17,-1,55,22,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "47e747990e3224cff24684cff50c0b100201b5e43a80fdbc58caef2814804c4b");
        }

        private static void Case_05746()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5746,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-14,64,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-10,64,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-16,44,5,3), new GeneratedEnemyUnit(-10,6,79,24,4), new GeneratedEnemyUnit(-20,-3,7,40,2), new GeneratedEnemyUnit(10,20,62,16,2), new GeneratedEnemyUnit(-11,19,59,13,3), new GeneratedEnemyUnit(-1,18,44,21,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "31d798b7e34e3f24410c178318feccfaba5d2f9ffd1cc473baffa565aa454f0a");
        }

        private static void Case_05747()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5747,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-11,30,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-19,57,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,11,43,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-3,17,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,0,61,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-12,76,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,10,28,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,8,75,12,2), new GeneratedEnemyUnit(1,8,99,41,1), new GeneratedEnemyUnit(12,10,50,33,1), new GeneratedEnemyUnit(-2,7,31,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "3961bf983dc17748be064fb849ae44c00e9f986288bf809e0152259b254af350");
        }

        private static void Case_05748()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5748,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-14,14,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,15,91,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,15,84,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-3,12,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,1,62,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-11,57,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,2,49,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-10,50,4,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "1ddd1244fb00bcdd1bf9be4838cf555a56271810c801d26c93269086b4ee28db");
        }

        private static void Case_05749()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5749,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,10,11,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-7,69,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-13,24,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,12,82,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-16,42,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,14,46,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-3,65,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-20,78,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-17,51,13,4), new GeneratedEnemyUnit(-1,17,68,17,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "dfc931cce1face3a4169ed961ed49b94a411c91349998192e6cccb23425ee389");
        }

        private static void Case_05750()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5750,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-5,38,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,0,27,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,2,36,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,7,13,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-2,20,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,16,61,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,8,68,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,1,56,31,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "05a58ecaa79007124b1bca28e9cc62e379f1d88392b35b479e04d4f2eab3c0ec");
        }

        private static void Case_05751()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5751,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-15,87,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,7,29,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,4,32,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,3,78,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-18,83,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,4,8,2,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "91d03901ad5a2e907e9c836165bdb13336b19589c58a682bde9a8f44509157af");
        }

        private static void Case_05752()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5752,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-10,37,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,1,41,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-9,43,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-16,18,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-5,28,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,11,72,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,15,91,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,20,8,36,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "880362b62560e73eb79686f5eca336b274b3ce2a3793feb0cfe4dd567c8b5a35");
        }

        private static void Case_05753()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5753,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-6,61,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,13,26,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,2,63,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,19,42,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-18,38,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,14,63,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,15,89,17,2), new GeneratedEnemyUnit(6,1,89,10,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "1b276147b73bf4b308145e476b5f373a2b7da2274cff0d81b8b45f09af292092");
        }

        private static void Case_05754()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5754,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,7,17,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-16,61,1,4), new GeneratedEnemyUnit(10,-20,10,31,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "d4607e5125db82c5d0b1f28737852e7cacc55755df7092df663780a0a16635e6");
        }

        private static void Case_05755()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5755,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-13,89,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-11,74,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,4,38,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-4,71,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-12,20,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,8,68,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-7,37,7,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3a8e37e1e5701435bd6d8b43b0222ebc9eb52ca61417c2625e96be19d0e36d8d");
        }

        private static void Case_05756()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5756,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,18,93,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,16,69,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-9,22,6,3), new GeneratedEnemyUnit(1,8,59,19,4), new GeneratedEnemyUnit(-18,-9,7,2,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "22049fc57a1300f71cabde66bbb00556483aba872295edb914fea4486f0d514e");
        }

        private static void Case_05757()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5757,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,13,40,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,2,80,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-9,17,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,3,22,2,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3ebd02d11e5d9d47c1bc12ea0faedc6e6c93384eec6dc27d26728f215ff9643e");
        }

        private static void Case_05758()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5758,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-12,75,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,5,55,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-16,95,8,1), new GeneratedEnemyUnit(-20,-13,70,19,3), new GeneratedEnemyUnit(-8,-13,63,18,4), new GeneratedEnemyUnit(-20,2,36,2,3), new GeneratedEnemyUnit(13,12,20,20,2), new GeneratedEnemyUnit(-8,-4,85,6,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "0777b451329f7747a89bccb208bf851fe40af96a86705b86095851822ee42bdb");
        }

        private static void Case_05759()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5759,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,11,98,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-17,92,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,10,39,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,14,41,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-4,42,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-16,17,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-17,74,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,1,73,11,4), new GeneratedEnemyUnit(-7,6,89,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8622f7ff6c737624a57b2638c88757c425e85a4ffe8956d205ee1a03639dbb9e");
        }

    }
}
