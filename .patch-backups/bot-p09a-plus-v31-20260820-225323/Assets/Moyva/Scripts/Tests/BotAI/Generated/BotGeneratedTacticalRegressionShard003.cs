using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard003
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_00540();
            Case_00541();
            Case_00542();
            Case_00543();
            Case_00544();
            Case_00545();
            Case_00546();
            Case_00547();
            Case_00548();
            Case_00549();
            Case_00550();
            Case_00551();
            Case_00552();
            Case_00553();
            Case_00554();
            Case_00555();
            Case_00556();
            Case_00557();
            Case_00558();
            Case_00559();
            Case_00560();
            Case_00561();
            Case_00562();
            Case_00563();
            Case_00564();
            Case_00565();
            Case_00566();
            Case_00567();
            Case_00568();
            Case_00569();
            Case_00570();
            Case_00571();
            Case_00572();
            Case_00573();
            Case_00574();
            Case_00575();
            Case_00576();
            Case_00577();
            Case_00578();
            Case_00579();
            Case_00580();
            Case_00581();
            Case_00582();
            Case_00583();
            Case_00584();
            Case_00585();
            Case_00586();
            Case_00587();
            Case_00588();
            Case_00589();
            Case_00590();
            Case_00591();
            Case_00592();
            Case_00593();
            Case_00594();
            Case_00595();
            Case_00596();
            Case_00597();
            Case_00598();
            Case_00599();
            Case_00600();
            Case_00601();
            Case_00602();
            Case_00603();
            Case_00604();
            Case_00605();
            Case_00606();
            Case_00607();
            Case_00608();
            Case_00609();
            Case_00610();
            Case_00611();
            Case_00612();
            Case_00613();
            Case_00614();
            Case_00615();
            Case_00616();
            Case_00617();
            Case_00618();
            Case_00619();
            Case_00620();
            Case_00621();
            Case_00622();
            Case_00623();
            Case_00624();
            Case_00625();
            Case_00626();
            Case_00627();
            Case_00628();
            Case_00629();
            Case_00630();
            Case_00631();
            Case_00632();
            Case_00633();
            Case_00634();
            Case_00635();
            Case_00636();
            Case_00637();
            Case_00638();
            Case_00639();
            Case_00640();
            Case_00641();
            Case_00642();
            Case_00643();
            Case_00644();
            Case_00645();
            Case_00646();
            Case_00647();
            Case_00648();
            Case_00649();
            Case_00650();
            Case_00651();
            Case_00652();
            Case_00653();
            Case_00654();
            Case_00655();
            Case_00656();
            Case_00657();
            Case_00658();
            Case_00659();
            Case_00660();
            Case_00661();
            Case_00662();
            Case_00663();
            Case_00664();
            Case_00665();
            Case_00666();
            Case_00667();
            Case_00668();
            Case_00669();
            Case_00670();
            Case_00671();
            Case_00672();
            Case_00673();
            Case_00674();
            Case_00675();
            Case_00676();
            Case_00677();
            Case_00678();
            Case_00679();
            Case_00680();
            Case_00681();
            Case_00682();
            Case_00683();
            Case_00684();
            Case_00685();
            Case_00686();
            Case_00687();
            Case_00688();
            Case_00689();
            Case_00690();
            Case_00691();
            Case_00692();
            Case_00693();
            Case_00694();
            Case_00695();
            Case_00696();
            Case_00697();
            Case_00698();
            Case_00699();
            Case_00700();
            Case_00701();
            Case_00702();
            Case_00703();
            Case_00704();
            Case_00705();
            Case_00706();
            Case_00707();
            Case_00708();
            Case_00709();
            Case_00710();
            Case_00711();
            Case_00712();
            Case_00713();
            Case_00714();
            Case_00715();
            Case_00716();
            Case_00717();
            Case_00718();
            Case_00719();
        }

        private static void Case_00540()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 540,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-1,17,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-4,15,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-3,85,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,8,43,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-5,24,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,6,61,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-15,55,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,8,20,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-11,66,21,3), new GeneratedEnemyUnit(-20,-6,33,8,3), new GeneratedEnemyUnit(-8,13,61,50,2), new GeneratedEnemyUnit(-9,5,33,17,2), new GeneratedEnemyUnit(1,19,56,30,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "e896a01bc68530a3d3f67e2dcc27e6e835dfd48e738a7f78e33709c32643e209");
        }

        private static void Case_00541()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 541,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,19,59,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,3,87,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-3,17,4,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "283494c4d325148a07a8ffb0961d23c77ce3319bcbc6c42194b971a3367cc7e3");
        }

        private static void Case_00542()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 542,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-4,34,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,18,11,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,4,33,38,4), new GeneratedEnemyUnit(-1,-18,78,35,4), new GeneratedEnemyUnit(20,-7,28,32,3), new GeneratedEnemyUnit(1,-7,43,46,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "772c9815d5b03211983ee12adff959a482c63fd8cfc23f13db328410eebcd3b1");
        }

        private static void Case_00543()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 543,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-11,86,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-12,11,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-6,9,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,6,75,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,17,63,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,1,45,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,18,36,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-20,93,44,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "778ef059a733863677facc655f8465169c435cdb535fdb68a6fc077b7581dd44");
        }

        private static void Case_00544()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 544,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,11,47,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-4,98,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,9,50,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-16,22,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,10,90,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-11,91,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-11,94,12,4), new GeneratedEnemyUnit(-11,-20,48,10,1), new GeneratedEnemyUnit(17,-1,11,29,4), new GeneratedEnemyUnit(8,-3,68,29,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "96ac54665b2d0ea3f659f47b25563ac2230089f9261bcff5beecf9f0d4f26de0");
        }

        private static void Case_00545()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 545,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,9,40,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,11,74,8,3), new GeneratedEnemyUnit(11,-17,86,2,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "84ee4cfab7ec662e0ea320b89f9a1ed29ff6bbfe1cdf1a01f5b4338994bfd6a1");
        }

        private static void Case_00546()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 546,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,3,63,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-9,62,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,14,10,42,1), new GeneratedEnemyUnit(3,-7,14,50,2), new GeneratedEnemyUnit(-19,-2,75,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "ddaa90e5119b22d8ccd97a3d3d71f3dc85a22cb877cfcdcc7e0fcb7bc83e36be");
        }

        private static void Case_00547()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 547,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,1,77,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,17,25,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,14,68,2,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "a76b910631b387ba017f94b473d0d79f5a91a95a88b243c9aa3158a39ba0e637");
        }

        private static void Case_00548()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 548,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-19,93,4,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "ee7b6a121829a1411bd3ce662d41905a5e9b78ee090f59024aee78eaf841be53");
        }

        private static void Case_00549()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 549,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-9,41,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,11,14,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-17,16,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,18,61,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-20,46,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-11,28,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-5,38,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,9,35,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-4,12,1,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "5a2458a8a622b5d5cc102e5d1222baaeb8ec1d1055c784390745a1f6562f7b54");
        }

        private static void Case_00550()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 550,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-1,25,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,0,20,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,10,49,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-16,29,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-18,31,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,11,75,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,12,87,3,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "5341aaa1bed8def3bdc00e01e214ba7e29bddaba3a406898e88f3bf2d021e44a");
        }

        private static void Case_00551()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 551,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-9,81,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,18,49,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,1,32,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,16,11,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-16,40,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-3,48,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-17,9,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,19,44,40,1), new GeneratedEnemyUnit(-13,3,29,7,1), new GeneratedEnemyUnit(20,20,20,31,2), new GeneratedEnemyUnit(-20,16,7,28,2), new GeneratedEnemyUnit(12,5,70,18,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "d806b53c6565b134d94833db5d506fe47b8b39222c3e6ce6b53de7eaab076a44");
        }

        private static void Case_00552()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 552,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-3,93,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-14,17,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,2,43,39,2), new GeneratedEnemyUnit(-17,2,34,33,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "417994963fadf7bd180b5b6a16231d4125e6779b6856db1f7dc895c20d0f3e65");
        }

        private static void Case_00553()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 553,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,19,23,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-18,68,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,3,92,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-19,22,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,9,49,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,13,41,2,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f1f3e129959ac68d48aac65c5c0ae691122ffe8a2541148661d205d5cecb3bea");
        }

        private static void Case_00554()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 554,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-1,81,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,8,93,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,12,7,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-11,29,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,17,13,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-18,22,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,19,77,25,3), new GeneratedEnemyUnit(11,-1,39,4,4), new GeneratedEnemyUnit(14,-11,35,46,4), new GeneratedEnemyUnit(-16,-3,71,30,4), new GeneratedEnemyUnit(-1,18,25,28,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "0c39804f8db5ce1f6c5872753b58a98138a75507fd4639dec4bcd730de8407fd");
        }

        private static void Case_00555()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 555,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,12,40,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-19,59,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-19,47,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,14,34,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,11,66,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,10,10,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,9,86,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-4,81,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,8,58,13,2), new GeneratedEnemyUnit(19,0,33,16,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "5a9105d1db020a6ff61d961e54e00d9509d51b586d92a1b050f9fd9f5f795f81");
        }

        private static void Case_00556()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 556,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-13,72,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-16,33,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-11,52,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,15,38,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-6,85,21,2), new GeneratedEnemyUnit(-14,12,63,10,2), new GeneratedEnemyUnit(15,8,10,42,2), new GeneratedEnemyUnit(19,3,51,14,4), new GeneratedEnemyUnit(-8,-18,91,20,2), new GeneratedEnemyUnit(13,-14,5,32,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "22b149ed80c45ee338c9553779658f42bc5e32eca32710fe390ecf5e4f2e4c3a");
        }

        private static void Case_00557()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 557,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,20,96,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,4,42,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,19,65,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-6,62,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-17,33,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-2,49,36,2), new GeneratedEnemyUnit(17,16,93,2,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6ade6660f86636448390436fd81e905cf3dd0ae5a58b087abf8b7b226b03ef8d");
        }

        private static void Case_00558()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 558,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-11,27,3,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "fd02596e3c9a1ef4f34cd20da51866845660c6477317e206785e773c3828c530");
        }

        private static void Case_00559()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 559,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,14,35,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-14,84,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,7,98,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,19,79,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-14,58,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-20,50,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-11,84,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-3,55,6,1), new GeneratedEnemyUnit(20,-9,49,11,2), new GeneratedEnemyUnit(-9,16,65,28,3), new GeneratedEnemyUnit(10,1,32,1,3), new GeneratedEnemyUnit(-6,-16,21,37,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "710094eeb2c060752d8e017fdcd928515ef04c823da928def0a14230bc6e8681");
        }

        private static void Case_00560()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 560,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-15,47,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,14,83,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-17,24,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-1,83,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-6,30,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,12,74,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-2,42,43,2), new GeneratedEnemyUnit(-18,-1,83,17,4), new GeneratedEnemyUnit(7,11,90,24,4), new GeneratedEnemyUnit(14,2,41,29,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8a2da3ad07870264a53397ef6df1584fbfc52c5ca8aa24fd3b64e4471142e601");
        }

        private static void Case_00561()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 561,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,15,81,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-4,71,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-11,73,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-7,19,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,16,20,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,2,15,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-7,96,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-10,92,33,4), new GeneratedEnemyUnit(5,-16,74,44,4), new GeneratedEnemyUnit(13,20,41,17,4), new GeneratedEnemyUnit(4,-16,22,39,2), new GeneratedEnemyUnit(-7,-13,24,14,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "a77ab8ed05467f4f076d800163f860099c4ea3838b1e0ce952c18a9cea69f806");
        }

        private static void Case_00562()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 562,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,10,74,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-17,55,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,16,36,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,17,26,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-18,31,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-8,61,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-4,100,6,1), new GeneratedEnemyUnit(4,9,47,48,2), new GeneratedEnemyUnit(11,8,90,27,4), new GeneratedEnemyUnit(-6,16,16,13,4), new GeneratedEnemyUnit(-17,3,84,40,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "cd1565fcf83bb1cf94935d5201f12840f935069c2ceb5b170cae6ae7a6849482");
        }

        private static void Case_00563()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 563,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-18,80,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,6,63,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "363eeef8d1407ebc8a6de40bb5bc40fbe1a452ce3f0cdd3bda474a2ce541ed58");
        }

        private static void Case_00564()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 564,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-18,9,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-2,99,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-6,34,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-11,68,30,4), new GeneratedEnemyUnit(-2,5,53,9,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "52f4e21bcca6560b2e891affda07f12f51943899011136346cc65314f5ea22fa");
        }

        private static void Case_00565()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 565,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-17,46,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,2,55,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,6,32,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,6,7,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,8,79,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-6,53,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-10,21,7,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "6964250f89d612500c9d001db79f4aae6975a867f3bed21ed16daffc51e722a2");
        }

        private static void Case_00566()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 566,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,13,79,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-15,5,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,0,34,21,4), new GeneratedEnemyUnit(9,7,80,13,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "116ea0b3647de9778e792486394e9505614fbda057c16713da5a8bfd242ad038");
        }

        private static void Case_00567()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 567,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-4,86,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,1,9,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-6,50,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,1,97,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-9,83,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-12,85,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,3,33,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-8,16,1,1), new GeneratedEnemyUnit(-14,14,60,48,4), new GeneratedEnemyUnit(0,17,15,13,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "18a336159f412d9a298f0f52e45f65194b11175187e30715d39d78db4de6b471");
        }

        private static void Case_00568()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 568,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,14,62,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-18,70,6,1), new GeneratedEnemyUnit(-9,-16,55,49,1), new GeneratedEnemyUnit(-8,-5,52,27,2), new GeneratedEnemyUnit(-13,-17,84,27,1), new GeneratedEnemyUnit(-20,6,25,48,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "9b28f6967c3c1a8c8650154a572ba4666682ad4e1ef6aa02f550d04eebb47648");
        }

        private static void Case_00569()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 569,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-4,39,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-16,83,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-8,89,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,9,29,26,1), new GeneratedEnemyUnit(13,-2,25,33,1), new GeneratedEnemyUnit(-7,-7,88,38,2), new GeneratedEnemyUnit(10,8,52,12,2), new GeneratedEnemyUnit(7,5,55,8,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "3361838ffdaae503732378c8a811f971b8bf5570ebc45d8058457abfbdd37aa8");
        }

        private static void Case_00570()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 570,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,7,51,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,18,68,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,16,12,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,18,91,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-19,25,36,2), new GeneratedEnemyUnit(-4,2,85,30,1), new GeneratedEnemyUnit(-4,5,45,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "dfb578843cbe21b92d758be3baa6fec0bba85c55a1946735093db90441faaf4a");
        }

        private static void Case_00571()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 571,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-3,26,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,5,65,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,13,92,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,15,58,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,8,86,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,9,15,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,10,78,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-10,74,14,1), new GeneratedEnemyUnit(-19,9,78,5,2), new GeneratedEnemyUnit(-5,-15,62,36,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 30,
                stableHash: "adb2911d8f4b684a3ec86626e132579bb8e99bf7c11a17ea8ceee13d9a2c3572");
        }

        private static void Case_00572()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 572,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,13,66,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,17,27,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-6,33,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,3,17,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,8,41,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-13,51,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-5,74,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-2,69,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-20,95,16,3), new GeneratedEnemyUnit(-20,-5,59,37,2), new GeneratedEnemyUnit(-17,12,68,37,2), new GeneratedEnemyUnit(-17,-14,85,22,1), new GeneratedEnemyUnit(3,19,23,19,2), new GeneratedEnemyUnit(-11,-11,83,15,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "48e58b57a2bb42463f25f28771f6d20dca8ac64ec450cbb5e69ff596217d3d8f");
        }

        private static void Case_00573()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 573,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-19,17,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-18,60,48,1), new GeneratedEnemyUnit(17,15,13,6,1), new GeneratedEnemyUnit(6,18,26,24,1), new GeneratedEnemyUnit(20,9,78,33,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "7f32a958097e1d5a5c1433f18e3ea64bfaea705de944fa0d7a6ee2a9f35f0a24");
        }

        private static void Case_00574()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 574,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,16,42,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,20,35,6,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "a29f8f4599db0e045856e63529e15ca24997629464221699361b4c2bc2e13145");
        }

        private static void Case_00575()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 575,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-14,75,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-6,59,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-17,42,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-1,84,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,19,50,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-20,53,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,6,8,17,2), new GeneratedEnemyUnit(-5,-14,56,27,2), new GeneratedEnemyUnit(-3,-3,40,15,1), new GeneratedEnemyUnit(-1,14,16,16,3), new GeneratedEnemyUnit(15,3,99,7,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "65f2d4bac8d623c8f5187666b05550cf70db19cb2177e89d70a3bf1f57c07aca");
        }

        private static void Case_00576()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 576,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,0,62,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-16,29,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-9,24,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-17,9,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,15,53,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,12,5,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,17,99,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-3,62,33,4), new GeneratedEnemyUnit(-1,-19,21,34,4), new GeneratedEnemyUnit(-8,17,69,42,3), new GeneratedEnemyUnit(13,-18,43,39,2), new GeneratedEnemyUnit(16,-9,46,40,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "f0e8ca0f2ceed4cdeb16caa77a7ea25218a0a2d4576406783dae089d07842df8");
        }

        private static void Case_00577()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 577,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-3,82,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,15,15,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-13,33,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,5,100,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,10,15,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,2,63,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,1,19,29,3), new GeneratedEnemyUnit(12,1,49,8,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "64df1f7b3721e866b09089a004e1819d164b7c0cb0022b86bac7b4ef8d247fbe");
        }

        private static void Case_00578()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 578,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,14,96,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,11,75,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-7,40,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,13,13,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-14,87,3,3), new GeneratedEnemyUnit(-18,-10,66,29,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "2520ec150e590dc7e1bfa1aba3f0a36e5f90ea64493769b50067e0800f24c0ec");
        }

        private static void Case_00579()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 579,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,2,45,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-7,8,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,4,81,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,11,100,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-8,57,7,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "689914b4c414896527201b77a31494cb99597957325509f1d61a09b2545b4d70");
        }

        private static void Case_00580()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 580,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,6,49,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-1,76,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-2,31,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-1,78,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-2,23,22,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f220c515dc010c388c2900f66059e79aa9090b97adae6550e2e5c7bf8ac85d0f");
        }

        private static void Case_00581()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 581,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-19,14,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,13,32,25,2), new GeneratedEnemyUnit(-11,-4,20,27,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "422c716e3683f80949792a9fb9233796f6718494d9c393888f6f34dc89ee87e1");
        }

        private static void Case_00582()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 582,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-17,65,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-13,36,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,8,22,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-11,40,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-17,67,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,14,13,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,16,76,6,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "e8a19602f7b66e1e9e319ca5f73f3224f07d5fe6e805f8dab9542f3453b18383");
        }

        private static void Case_00583()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 583,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-3,74,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,4,32,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-5,51,28,2), new GeneratedEnemyUnit(13,4,12,32,4), new GeneratedEnemyUnit(14,-16,41,13,1), new GeneratedEnemyUnit(-18,-10,73,29,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "d4d528188ec38ac80bb7b3cd019897d5f0d10398e458ec679c77c7fc589daf64");
        }

        private static void Case_00584()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 584,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-10,65,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,7,24,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-11,15,20,1), new GeneratedEnemyUnit(7,-17,50,3,2), new GeneratedEnemyUnit(0,-9,61,19,3), new GeneratedEnemyUnit(5,3,22,13,2), new GeneratedEnemyUnit(-8,6,84,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "6f75839141d1a63a89244cf02664a2172d0636ff35a7e116aeee83185973fac9");
        }

        private static void Case_00585()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 585,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,17,78,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,7,26,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,15,34,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-16,86,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,0,21,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,20,60,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,12,91,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-5,98,12,1), new GeneratedEnemyUnit(14,13,11,39,1), new GeneratedEnemyUnit(8,-15,92,32,4), new GeneratedEnemyUnit(10,2,58,9,3), new GeneratedEnemyUnit(-16,-15,95,10,1), new GeneratedEnemyUnit(20,-1,25,13,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7e7d5d7534ee5510b2fa016ea3977c111c885c56c38492dec08294c5c9a87c85");
        }

        private static void Case_00586()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 586,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-1,90,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-10,78,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,1,35,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,9,7,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-10,52,25,4), new GeneratedEnemyUnit(-19,-3,98,31,1), new GeneratedEnemyUnit(1,-10,27,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "8e2f2debc286270d248bb803cdced26107c936942603f4c8dfc117255584b016");
        }

        private static void Case_00587()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 587,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-5,8,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-19,99,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,13,96,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,6,56,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,9,36,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,16,59,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,8,71,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-4,99,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-17,53,22,2), new GeneratedEnemyUnit(-2,-18,66,10,2), new GeneratedEnemyUnit(13,18,47,13,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "676acc079852bc847b9839c889a23961d0aacdb2fcb975533fda0835c53cc560");
        }

        private static void Case_00588()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 588,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,14,25,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-16,62,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,0,66,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,16,10,32,4), new GeneratedEnemyUnit(15,14,74,22,4), new GeneratedEnemyUnit(-19,-12,48,4,2), new GeneratedEnemyUnit(-20,-13,17,29,4), new GeneratedEnemyUnit(-10,8,18,49,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "f27b8cb233c63c95d188bdc69e7442b0649f243f818b6983f2db88d1eae4372e");
        }

        private static void Case_00589()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 589,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-17,9,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,7,32,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,1,17,35,3), new GeneratedEnemyUnit(8,13,52,12,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8b7b203279356b3abef40ffc4ad7aa2ef4eb81f634dadfa267b7a832a68dfd67");
        }

        private static void Case_00590()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 590,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,11,44,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,16,80,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,1,79,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,17,81,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-13,74,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-16,81,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,17,68,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-2,76,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-11,87,22,3), new GeneratedEnemyUnit(-2,6,34,40,3), new GeneratedEnemyUnit(12,9,20,33,3), new GeneratedEnemyUnit(4,-2,54,25,2), new GeneratedEnemyUnit(-12,-20,80,14,3), new GeneratedEnemyUnit(-20,-5,9,41,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "d1b06739c271567e7ccdfcf6f23fac9dc419d098576301296c0434e2517e0dd9");
        }

        private static void Case_00591()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 591,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-20,95,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,17,42,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-13,95,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-20,70,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-8,64,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,11,43,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-17,97,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,20,75,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,9,54,40,3), new GeneratedEnemyUnit(-9,-8,10,1,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "85309d9393fee45d1ab511a96941fd9ca7fa518c7c1777b041053476cfa122eb");
        }

        private static void Case_00592()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 592,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,16,65,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,8,52,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-12,50,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-2,70,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,13,20,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,1,11,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-6,9,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-20,90,26,2), new GeneratedEnemyUnit(-5,-14,33,32,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "7e16937e1585aae14e480c121a683377dbe1045a6c109a0c8ff1ddb4135c7673");
        }

        private static void Case_00593()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 593,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-2,47,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-3,8,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-19,76,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-1,39,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,7,54,5,2), new GeneratedEnemyUnit(15,11,66,32,1), new GeneratedEnemyUnit(14,0,64,5,1), new GeneratedEnemyUnit(14,13,53,35,3), new GeneratedEnemyUnit(19,-18,14,30,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "63c951b0701e9562f545c2c9f89d4ab63f15e69c53b25ca4f2760f58c91d0e99");
        }

        private static void Case_00594()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 594,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-8,35,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,12,69,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-19,11,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,5,42,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-11,96,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,15,62,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-5,92,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,7,91,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-6,6,10,4), new GeneratedEnemyUnit(3,11,9,28,3), new GeneratedEnemyUnit(-7,13,21,19,1), new GeneratedEnemyUnit(20,-16,45,46,2), new GeneratedEnemyUnit(10,-13,95,29,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "c898a56d8bec20446e0ec47aafb802ec5c13c29b46267ed8568425eed7655d50");
        }

        private static void Case_00595()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 595,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-2,40,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-1,56,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,19,66,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-11,47,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,2,84,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,2,40,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,16,73,48,4), new GeneratedEnemyUnit(6,9,16,42,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "53cbd437b2ab626fc3a82fb282b01640a2eb73a9b5b5caed4e80c09a6fbbf5c6");
        }

        private static void Case_00596()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 596,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,1,39,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,10,18,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-5,78,31,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "843f473f2ad207c8788fa37a300665bc2c251aa2a2b7cf30783b93ded6c669a0");
        }

        private static void Case_00597()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 597,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-12,65,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,4,90,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,11,38,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,6,26,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,0,69,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-6,19,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,6,14,38,2), new GeneratedEnemyUnit(-19,-14,60,34,3), new GeneratedEnemyUnit(-10,17,18,45,3), new GeneratedEnemyUnit(-9,0,5,46,1), new GeneratedEnemyUnit(-6,-17,57,43,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "e43ef081f2bb64a9493be029b1b76f5ce32cb1b3b4be7b5683837f5f8edf2ec8");
        }

        private static void Case_00598()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 598,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-15,89,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-10,6,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,8,22,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,5,95,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,6,72,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,18,7,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,17,16,33,4), new GeneratedEnemyUnit(16,-4,43,32,2), new GeneratedEnemyUnit(-20,-18,76,20,2), new GeneratedEnemyUnit(-8,-13,83,20,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "19c7e82291f32a2a78c4a44f8568742ff6a466bdbdab0bc5ff05039f4efb9d9c");
        }

        private static void Case_00599()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 599,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-6,69,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,18,85,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-13,18,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-18,17,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-12,40,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-2,76,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-1,36,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,4,14,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-6,42,42,4), new GeneratedEnemyUnit(-12,-4,79,7,2), new GeneratedEnemyUnit(-17,-11,60,3,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "92a1153018253cc3870ad76c197abf343dbdcf306e2d63c75d4305cbe722c283");
        }

        private static void Case_00600()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 600,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-17,15,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-16,45,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-12,31,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,19,29,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,5,55,40,1), new GeneratedEnemyUnit(-17,9,38,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "4340657429999e5daceb087777869d44e9d2d873a9efb0d7ee9d929c65006f69");
        }

        private static void Case_00601()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 601,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,12,78,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,18,97,33,4), new GeneratedEnemyUnit(-14,14,19,31,3), new GeneratedEnemyUnit(4,11,69,14,1), new GeneratedEnemyUnit(17,-20,8,38,4), new GeneratedEnemyUnit(-4,-15,35,6,1), new GeneratedEnemyUnit(-13,8,56,31,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "b364274a89e6bb5014c1905caf8179e41313337fc6f63e75bf9205ba1b159e67");
        }

        private static void Case_00602()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 602,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-19,35,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,7,10,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-8,26,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-2,87,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,15,70,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,2,12,33,3), new GeneratedEnemyUnit(-13,7,18,9,3), new GeneratedEnemyUnit(-9,20,88,39,4), new GeneratedEnemyUnit(18,16,30,19,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "f0899fb900d95d67bd2c5309f84955bbe081499047954153800aae653f7c333f");
        }

        private static void Case_00603()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 603,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,7,7,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-3,94,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-7,80,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-4,24,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-5,70,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-9,99,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,20,92,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,20,41,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,13,50,38,1), new GeneratedEnemyUnit(2,-1,29,2,1), new GeneratedEnemyUnit(-5,-6,62,41,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "63f4b9f45f80f178626014fea51296407c72d542b78bfffce3a9b82683ecfe69");
        }

        private static void Case_00604()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 604,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,8,40,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,9,46,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-17,73,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,20,62,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,13,73,11,4), new GeneratedEnemyUnit(-13,11,17,28,1), new GeneratedEnemyUnit(7,20,75,26,4), new GeneratedEnemyUnit(18,12,79,17,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "1b7ae84a30025ac5cd8859c3d9a795a5dd2f095bc354b0360d0ebd7bdd06c94e");
        }

        private static void Case_00605()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 605,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-16,84,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-12,69,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,17,98,31,3), new GeneratedEnemyUnit(-15,-10,79,15,2), new GeneratedEnemyUnit(-15,11,79,13,2), new GeneratedEnemyUnit(-9,11,92,14,2), new GeneratedEnemyUnit(18,-6,97,3,1), new GeneratedEnemyUnit(-13,-7,86,8,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "1cb35c49c2c2d601da2c85694438b9ab7ac443b54731789d46ffc4036117e11b");
        }

        private static void Case_00606()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 606,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,4,27,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-12,14,46,2), new GeneratedEnemyUnit(-9,10,17,19,3), new GeneratedEnemyUnit(1,9,76,39,3), new GeneratedEnemyUnit(-14,-12,75,32,2), new GeneratedEnemyUnit(7,-1,5,34,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "666f0f6ffae279d984390129ba61c175e8ed6329494d6840407f1766cf5b5476");
        }

        private static void Case_00607()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 607,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,0,5,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-11,97,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-11,59,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,18,10,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-15,12,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-16,48,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-3,58,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-11,12,35,4), new GeneratedEnemyUnit(-8,15,71,6,4), new GeneratedEnemyUnit(-11,13,65,49,4), new GeneratedEnemyUnit(14,18,23,41,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "827d67db67ca05146a62b2577d22e48208ddfb5e0c7b94a7d4637a235702cb7d");
        }

        private static void Case_00608()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 608,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-1,30,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-11,33,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,20,33,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-4,76,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,12,25,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-16,14,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-7,64,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,3,46,31,1), new GeneratedEnemyUnit(17,11,75,23,2), new GeneratedEnemyUnit(20,-7,17,14,1), new GeneratedEnemyUnit(11,15,72,10,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a5e32da832f1a8891d304801698ccbf380618533c9f65331389a0d1fda45d70b");
        }

        private static void Case_00609()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 609,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,13,23,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,18,13,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,13,92,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-16,9,2,3), new GeneratedEnemyUnit(-17,7,45,44,4), new GeneratedEnemyUnit(12,17,94,33,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "82a7684aaa222a0cbef2c5b8339acdaa68875b6fa9b2773e34befddda16c122f");
        }

        private static void Case_00610()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 610,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-17,8,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,7,21,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-7,67,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,11,45,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,13,88,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,7,45,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-20,69,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,2,45,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,14,37,26,4), new GeneratedEnemyUnit(5,-2,86,50,3), new GeneratedEnemyUnit(8,0,98,21,1), new GeneratedEnemyUnit(2,-4,20,21,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "132887e3662a3379c5bad236d1f360275713fb31e4c480b08f1893b8a92bd44c");
        }

        private static void Case_00611()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 611,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-11,58,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,1,40,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-16,31,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,6,91,44,4), new GeneratedEnemyUnit(14,-12,43,19,2), new GeneratedEnemyUnit(-17,-12,27,1,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e38712abebf34e0a4f01907b852be6b9a8b72c54d43f57a42e821eb98dee0d22");
        }

        private static void Case_00612()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 612,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-5,87,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,20,18,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-10,83,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,14,80,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,15,63,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-6,38,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,5,81,4,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "2e49c83802f9e0a2624b33220256679fe43af608ba9f01defb519df07aebe456");
        }

        private static void Case_00613()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 613,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-15,36,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-20,88,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-8,15,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-11,44,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-7,64,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-20,38,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,3,10,2,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "21329bf8b6372f9b6e9ddb40bc4953deeaba37528d64a36df1296c0a6f8383e2");
        }

        private static void Case_00614()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 614,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-4,26,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,1,42,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,5,90,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,20,86,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,9,21,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-11,20,7,1), new GeneratedEnemyUnit(-17,-15,77,45,4), new GeneratedEnemyUnit(13,-19,87,22,3), new GeneratedEnemyUnit(-12,0,74,45,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "53f34de3cad0d4042e49e66c1c83e75a003fe9cf0ef8a112dfc9d6d613266523");
        }

        private static void Case_00615()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 615,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-8,67,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,8,21,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-12,79,24,4), new GeneratedEnemyUnit(-18,-7,9,8,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "6d6637383a43c470ecf662a07d483cfb03f7c6b3c7128654f2f04c6550863148");
        }

        private static void Case_00616()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 616,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,20,25,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,0,89,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-6,95,22,1), new GeneratedEnemyUnit(13,16,45,30,2), new GeneratedEnemyUnit(16,-4,5,37,2), new GeneratedEnemyUnit(1,11,48,25,3), new GeneratedEnemyUnit(6,-19,40,15,2), new GeneratedEnemyUnit(2,15,10,7,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "3c7afde3d7e491ed49cbc790c181fe150c6cf92813f1193542ab6b45ad956a46");
        }

        private static void Case_00617()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 617,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-4,36,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,3,15,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,5,44,48,3), new GeneratedEnemyUnit(-10,5,86,19,4), new GeneratedEnemyUnit(6,-14,88,49,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8a12af6596fc80be236f0cbda62ebe156ffe0306febc1a6909ae47828ccd8218");
        }

        private static void Case_00618()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 618,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-8,56,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-17,60,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,14,32,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,17,87,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-14,32,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-6,59,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-10,27,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,19,98,42,4), new GeneratedEnemyUnit(10,1,82,44,3), new GeneratedEnemyUnit(-17,20,91,44,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "6291cd893e1658a40c92bcd8ea8c5c35b15149fd7f092d942e649a5445407022");
        }

        private static void Case_00619()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 619,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-10,98,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,1,40,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-14,80,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,8,74,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-14,10,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,14,90,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,19,34,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-3,63,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,2,41,12,2), new GeneratedEnemyUnit(6,-20,66,32,1), new GeneratedEnemyUnit(-17,-10,98,25,4), new GeneratedEnemyUnit(13,-9,91,44,2), new GeneratedEnemyUnit(14,2,7,24,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "f1365fc969712a4218b3800a2f54896baa37cf65853e6cac0665e67731ad53c4");
        }

        private static void Case_00620()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 620,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,14,61,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-6,66,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-11,100,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-20,99,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-5,46,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-1,17,13,1), new GeneratedEnemyUnit(-16,-2,24,41,1), new GeneratedEnemyUnit(-6,-20,87,42,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "f0a10c0146e3529eccfe631e959d576744338f5802974e78fc8ffceb6e2ba07c");
        }

        private static void Case_00621()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 621,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,2,93,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-18,72,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,0,5,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,2,93,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,11,7,47,1), new GeneratedEnemyUnit(4,10,83,37,1), new GeneratedEnemyUnit(9,-11,30,5,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5a15577a45eb54cf9df8dc0bcbbb2ef7d49c5cc9e25f6282db4e286105fd197f");
        }

        private static void Case_00622()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 622,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,13,5,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,4,8,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-17,72,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,11,48,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,10,49,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,1,80,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,18,32,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-10,76,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,9,22,32,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f0282d70ededff72e358c405a6d44ed50f98144fdfe21648fb04593a1e973d19");
        }

        private static void Case_00623()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 623,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,14,51,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,20,55,46,2), new GeneratedEnemyUnit(-10,17,24,7,4), new GeneratedEnemyUnit(20,2,39,33,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "41724f9252f09b781162cea4c670635fae289c1aa8028c5b7f180f1268c0a299");
        }

        private static void Case_00624()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 624,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,0,11,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-16,9,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-8,18,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-4,10,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-8,49,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,19,22,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-7,82,50,1), new GeneratedEnemyUnit(-5,-2,34,17,1), new GeneratedEnemyUnit(-1,-20,47,9,2), new GeneratedEnemyUnit(4,-15,28,33,1), new GeneratedEnemyUnit(-4,12,81,9,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "85cabe563c3c09fc5214894d5465377990bbc756df494c8fd7fe6d9677fbed85");
        }

        private static void Case_00625()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 625,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,15,52,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-10,52,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,11,90,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-18,31,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,0,24,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-1,21,7,2), new GeneratedEnemyUnit(-13,7,53,14,3), new GeneratedEnemyUnit(-20,-16,35,10,4), new GeneratedEnemyUnit(-11,11,33,37,1), new GeneratedEnemyUnit(4,-4,59,26,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "afd9de1a5ef9580b3b30f5a6c760ddd7165b8553aca2b3732ec15d518232a83f");
        }

        private static void Case_00626()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 626,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,1,46,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,16,8,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-16,30,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,20,22,12,3), new GeneratedEnemyUnit(7,-11,64,41,2), new GeneratedEnemyUnit(2,-11,36,34,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 85,
                stableHash: "8692bd3edcda36eaf1458826cfa1d8863dfa64e3d789ca1d00e1ab80b4632864");
        }

        private static void Case_00627()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 627,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,10,97,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-20,8,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-3,90,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-5,45,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-14,16,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,9,44,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,6,72,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-16,22,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-3,13,15,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "b5d32f90523ecbd1baca3db0bc9e3f305ed80e0ed23b5d871592249cefd5b9ca");
        }

        private static void Case_00628()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 628,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-7,35,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-20,71,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-7,18,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-11,49,49,2), new GeneratedEnemyUnit(-13,15,24,43,1), new GeneratedEnemyUnit(17,5,61,30,1), new GeneratedEnemyUnit(17,14,73,15,3), new GeneratedEnemyUnit(10,7,34,38,4), new GeneratedEnemyUnit(6,-16,10,39,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "4757e10f76c0bc08b1a02b9b65c3408d6fa85d63147f7ba640aa4458dc9bcfc9");
        }

        private static void Case_00629()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 629,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,11,61,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,12,87,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,11,43,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,0,9,46,4), new GeneratedEnemyUnit(-17,9,77,19,1), new GeneratedEnemyUnit(1,19,84,31,1), new GeneratedEnemyUnit(1,-12,41,39,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "dc790ce6c850adb157b4fd66e4ac31824e419036000f91a6feaa9a3de5a5dc94");
        }

        private static void Case_00630()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 630,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-16,90,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,14,14,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,10,6,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,12,85,27,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "69adb8288478f85959a0dfd313930df991b56a762aad84d1d7a3d7a21f062714");
        }

        private static void Case_00631()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 631,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,12,59,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,13,88,41,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "da20bff4d2f0bbc93853491c49bc3602f94523394e1cb858de3f73228de3ba2c");
        }

        private static void Case_00632()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 632,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-7,83,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-10,51,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,9,22,7,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "6389f1a2abe59522ac2767f4c8e106c5ea3186274ce254914385d6e2db4ebd9a");
        }

        private static void Case_00633()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 633,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,19,75,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,12,43,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,12,18,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,13,44,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-15,50,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,8,65,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-11,64,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-2,64,36,1), new GeneratedEnemyUnit(3,0,43,11,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "8ad153d9e15e4ab5326b6ec526575eb90cf891eb9d5be8422e3f8efd6913bac9");
        }

        private static void Case_00634()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 634,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,7,60,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,13,61,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-18,74,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,20,58,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,3,57,34,4), new GeneratedEnemyUnit(-1,4,39,48,4), new GeneratedEnemyUnit(-17,15,85,24,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "eb9b22b4d5e6e94a5f09f1e6f80da62d43affd9c36ede38d78f5fd47776b5d28");
        }

        private static void Case_00635()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 635,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-16,16,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-1,47,26,2), new GeneratedEnemyUnit(-12,-4,5,31,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "e5079602f3f388ce1959046751f18766b970319f5e12c393d98ca78d7faf89a8");
        }

        private static void Case_00636()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 636,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,20,53,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-4,88,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,7,38,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-14,76,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,19,59,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,2,6,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,13,5,23,2), new GeneratedEnemyUnit(-17,-11,34,43,3), new GeneratedEnemyUnit(-9,0,14,4,1), new GeneratedEnemyUnit(17,-15,92,33,3), new GeneratedEnemyUnit(-10,-11,50,48,4), new GeneratedEnemyUnit(4,-11,67,45,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "9ed2e22956403c6d025c04108d8ebc9f107e4c9c92b44850f284fb9715968676");
        }

        private static void Case_00637()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 637,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,7,69,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,3,96,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,8,64,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-19,89,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,3,33,50,2), new GeneratedEnemyUnit(5,18,100,22,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e83ccc1bcf02c275f0dde4979afcb0032332d7aa54f96ed830e331ae179b5056");
        }

        private static void Case_00638()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 638,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,4,52,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-11,26,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,15,93,26,3), new GeneratedEnemyUnit(-4,20,94,45,1), new GeneratedEnemyUnit(-11,15,19,7,2), new GeneratedEnemyUnit(3,-6,58,35,2), new GeneratedEnemyUnit(-20,-14,75,42,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "ba138df627fd70cb5026b781ccf6e2f2eb9964f9a447f614246651be492f5353");
        }

        private static void Case_00639()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 639,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-15,30,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,3,18,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-14,94,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,10,91,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-20,73,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,19,26,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-20,39,19,1), new GeneratedEnemyUnit(-8,15,93,49,4), new GeneratedEnemyUnit(6,-6,8,14,1), new GeneratedEnemyUnit(-15,5,38,9,1), new GeneratedEnemyUnit(2,-10,89,23,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "2cf93eefea32901d6890d6e5947d62e8e89aad834f5adb771750901995f87888");
        }

        private static void Case_00640()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 640,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,14,98,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-6,41,10,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "349188cfbf5519ba6f005b3b481b27a5addab29129b2d0bce5b6b998fba53cb1");
        }

        private static void Case_00641()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 641,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,1,99,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-2,55,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-16,63,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-1,28,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,11,11,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-11,25,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-7,32,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-20,63,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,1,34,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "595450803c4935e3f33a8c4b74eb7757e86d13175d942571f97c785854fd5221");
        }

        private static void Case_00642()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 642,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,3,85,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,18,10,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,10,8,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-13,67,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,2,49,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,18,61,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-16,66,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,6,93,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,15,47,30,2), new GeneratedEnemyUnit(18,19,34,12,2), new GeneratedEnemyUnit(9,-13,64,24,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "bc837ce993d7bea3126dbb3af3f4bf4d9bf084bbe21f834594988ee10f1d371a");
        }

        private static void Case_00643()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 643,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,14,77,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,12,87,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,18,86,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,16,49,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,17,93,4,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "5745543236bc05647d93bf981605b9c3254feb71037289c121165b0c702ea3b4");
        }

        private static void Case_00644()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 644,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-13,62,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-4,21,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,1,93,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,14,75,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,3,95,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-18,29,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-3,78,41,4), new GeneratedEnemyUnit(-2,3,18,47,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ebc920c888e55d641b8a2c8dd9a5b6e7565b1e385c0aff864b4e8b289e41fafe");
        }

        private static void Case_00645()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 645,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,5,13,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-17,68,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-17,20,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-3,42,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,19,36,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,13,87,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,18,77,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-1,99,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-8,27,14,2), new GeneratedEnemyUnit(-19,-18,9,18,2), new GeneratedEnemyUnit(8,-1,7,44,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "25c78b8d29bb7a68e1b70cddcc562b477c60da01283abe88b2c476d2bbd378a2");
        }

        private static void Case_00646()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 646,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-19,51,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,15,38,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,0,59,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,7,9,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,5,55,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,0,97,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,3,35,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,6,84,15,4), new GeneratedEnemyUnit(2,4,98,1,4), new GeneratedEnemyUnit(-8,-6,64,20,3), new GeneratedEnemyUnit(-14,-3,81,33,1), new GeneratedEnemyUnit(12,2,46,2,4), new GeneratedEnemyUnit(-9,11,41,47,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "32061ef5c7bafa6204d51378b52f4ada8ee8196bc02f577932de1b040c200adf");
        }

        private static void Case_00647()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 647,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-12,55,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,2,74,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,0,84,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-1,65,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,3,56,18,2), new GeneratedEnemyUnit(5,9,44,31,2), new GeneratedEnemyUnit(-5,-5,17,39,3), new GeneratedEnemyUnit(19,15,22,45,4), new GeneratedEnemyUnit(-10,-13,90,33,2), new GeneratedEnemyUnit(9,-13,93,37,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "a028caaf05e19b766cf99d3e2aadb630c1789ec2ca269d200e20bb9f116936fa");
        }

        private static void Case_00648()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 648,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-6,53,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-4,40,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-12,53,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-16,31,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-12,61,36,3), new GeneratedEnemyUnit(20,-20,87,19,4), new GeneratedEnemyUnit(6,6,71,13,1), new GeneratedEnemyUnit(10,-17,16,26,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "7b6b54d3d516f1199e565cf7ce2b2544d66576ef8caa8d2411b99391b4b0b9ed");
        }

        private static void Case_00649()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 649,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,17,100,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,0,84,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,20,69,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,7,8,1,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "e50324e9b3faebe3312e367ba4b3438c7d95605e15671689c7030302605c1d95");
        }

        private static void Case_00650()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 650,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,16,14,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-18,22,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,1,48,14,1), new GeneratedEnemyUnit(-12,0,14,12,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "cc2a3344a1422a59358097e60ae5c0ba40a9080c0ab60bae73bf77ffc74b6029");
        }

        private static void Case_00651()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 651,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,15,78,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-15,61,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,12,30,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,6,97,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,1,69,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-15,63,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,4,58,45,4), new GeneratedEnemyUnit(-15,18,77,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "9140e0e95df376c7b3fec967b020250e39b14ef990c74e68047beeb4f22789db");
        }

        private static void Case_00652()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 652,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,3,42,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,7,66,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-6,9,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,3,72,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,10,100,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-17,71,38,2), new GeneratedEnemyUnit(-16,19,75,10,3), new GeneratedEnemyUnit(0,0,36,15,2), new GeneratedEnemyUnit(-10,-6,38,28,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "5cb84afd195820dd04644d7a9542817461842009d958ed5aac5bda2f5163d36e");
        }

        private static void Case_00653()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 653,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-4,96,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-1,66,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,16,60,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-19,66,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-3,96,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,1,51,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,14,46,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,0,6,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-19,21,21,4), new GeneratedEnemyUnit(-12,0,89,35,4), new GeneratedEnemyUnit(1,-11,45,40,2), new GeneratedEnemyUnit(-14,-9,97,22,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "05b1d9ffe11fa1119dd200335498f6ff1087fe3a760a56c2a51a7723273f1933");
        }

        private static void Case_00654()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 654,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-14,7,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-8,46,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-14,92,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,19,19,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,3,22,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-4,63,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-4,16,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,5,7,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-10,92,37,1), new GeneratedEnemyUnit(8,9,90,7,4), new GeneratedEnemyUnit(-3,11,21,34,3), new GeneratedEnemyUnit(-10,-7,81,15,4), new GeneratedEnemyUnit(5,-19,70,41,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "5d89b6ffe9191ab3a655c130573d540f14db9a18338f6e387dcdb705b8d932b1");
        }

        private static void Case_00655()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 655,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-6,62,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-4,40,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,14,95,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-6,79,48,3), new GeneratedEnemyUnit(-13,-18,22,16,2), new GeneratedEnemyUnit(3,1,97,21,4), new GeneratedEnemyUnit(-15,-15,47,24,1), new GeneratedEnemyUnit(11,-6,22,35,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "eb1902107c54e0ab43f0f9f71f1fc952e64dc3dbfe2ac0a8e91ed8a7f23278f2");
        }

        private static void Case_00656()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 656,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-12,15,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,11,59,3,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c07fe5befa9f90dab6fe005fd003856ba1b6e050728094e331eb44dad9f7a481");
        }

        private static void Case_00657()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 657,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,11,91,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,13,24,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-5,39,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-1,96,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,18,78,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-14,53,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,14,28,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,19,99,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-1,55,26,2), new GeneratedEnemyUnit(17,17,24,8,1), new GeneratedEnemyUnit(-7,4,57,41,1), new GeneratedEnemyUnit(-12,-1,86,12,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "af41d730c87ef61bb5789c8b8fc97da8d8248daa7dde141b314432fe5f193b1a");
        }

        private static void Case_00658()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 658,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-13,18,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,18,96,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-7,77,10,1), new GeneratedEnemyUnit(-8,16,25,14,1), new GeneratedEnemyUnit(10,-17,73,23,4), new GeneratedEnemyUnit(18,-1,82,2,4), new GeneratedEnemyUnit(7,-17,37,10,2), new GeneratedEnemyUnit(-10,5,42,3,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "526308724c256733e7b4e52796bddd03827a716fbb18a367787975cc0383b327");
        }

        private static void Case_00659()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 659,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-4,49,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,15,49,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,11,32,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,18,56,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-19,73,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-20,85,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-12,48,44,1), new GeneratedEnemyUnit(1,-9,45,15,4), new GeneratedEnemyUnit(9,-1,22,48,2), new GeneratedEnemyUnit(-7,10,30,49,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "ddb038346a1837e010470eca0e4205734f72f1a52ed81046d1d673952f049834");
        }

        private static void Case_00660()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 660,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,10,27,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,18,54,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-10,5,35,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "6536eca2f07cf3b73f1593ffe158c111a033a5ff491d5e3b350f40aba068a3df");
        }

        private static void Case_00661()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 661,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-7,53,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,12,35,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-10,74,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-6,85,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,0,20,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-20,91,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,13,99,3,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "cd2a69332d993f67390cd1727d1fee62fc427ffe4c9750d86c007a39b95c84e3");
        }

        private static void Case_00662()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 662,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-1,21,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,18,20,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-13,24,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,17,51,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,1,87,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-1,15,22,2), new GeneratedEnemyUnit(-5,-18,84,46,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "0cd911d6f97a09092698d970abab2ae5c707f43cd9c092354d0e67f0e7824f23");
        }

        private static void Case_00663()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 663,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-17,44,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-3,59,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,0,85,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,0,100,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-17,67,43,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "826bc5d107f31e9bc6a2deb8907a2ef1b60c672c71df9c7de3b90db2c8835178");
        }

        private static void Case_00664()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 664,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,4,81,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-16,72,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-8,85,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-17,50,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-13,14,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,7,62,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-4,15,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,3,16,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-6,46,31,3), new GeneratedEnemyUnit(5,15,41,6,1), new GeneratedEnemyUnit(4,-6,42,2,4), new GeneratedEnemyUnit(-6,-15,54,17,3), new GeneratedEnemyUnit(12,3,51,10,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a64cd9997c587da17db0d4b55e2ba7b60eefe24619890f6f018272949ff7cdc0");
        }

        private static void Case_00665()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 665,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-13,36,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,15,21,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,16,57,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,17,51,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-20,92,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,14,57,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-5,94,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-4,34,13,3), new GeneratedEnemyUnit(-7,18,30,26,3), new GeneratedEnemyUnit(-7,12,76,27,2), new GeneratedEnemyUnit(-6,-8,95,24,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "534f69fbb3601d40037cb6ea91fcf7d74def3cf248f2ad37cad2bd9da3414466");
        }

        private static void Case_00666()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 666,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,7,76,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-7,61,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,20,77,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,1,58,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,14,29,2,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "029e26eaa02272b4367424114d5bddd50133f96ae01cb856c32f7332cec7c8f8");
        }

        private static void Case_00667()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 667,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-3,92,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,11,97,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,16,42,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,19,64,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-10,96,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-7,49,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,15,64,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-13,100,36,3), new GeneratedEnemyUnit(14,13,16,24,3), new GeneratedEnemyUnit(2,-17,68,47,3), new GeneratedEnemyUnit(20,5,75,10,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "8756364a006b6c0ae2321bd9da235685652d7277fe0d6140a8f3502832d5805c");
        }

        private static void Case_00668()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 668,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-12,50,6,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "4049ac4ea33260280fc6cead42d14b36669c253a6cb655629abe28e9a428a7a3");
        }

        private static void Case_00669()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 669,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,5,57,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,12,56,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,6,30,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,8,98,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,14,86,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-10,50,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,5,70,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-10,40,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-19,35,15,2), new GeneratedEnemyUnit(-12,-8,83,29,1), new GeneratedEnemyUnit(4,14,60,47,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "bf3c83b54539cbb89e91ed884adcf204ae91918492e84e0b013df9e2063c49f4");
        }

        private static void Case_00670()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 670,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,16,51,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-3,64,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,2,23,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-11,36,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,11,58,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,4,34,25,4), new GeneratedEnemyUnit(20,-17,19,25,4), new GeneratedEnemyUnit(-19,-8,97,45,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "117d33a70c3febcffe58344c3d56991165d3396dc0e2345d4749d14aa29ac5f4");
        }

        private static void Case_00671()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 671,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,11,19,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,9,7,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,17,38,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,4,8,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,12,90,27,3), new GeneratedEnemyUnit(-3,-20,69,5,1), new GeneratedEnemyUnit(19,-9,85,26,4), new GeneratedEnemyUnit(12,-11,82,20,4), new GeneratedEnemyUnit(-9,-10,54,35,4), new GeneratedEnemyUnit(-14,-15,44,33,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "7213dc115606829cf6bde6bcad1da57921c7eb50427e59bd5366e7a8083ed38b");
        }

        private static void Case_00672()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 672,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,15,91,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,11,95,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-16,11,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-19,45,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-13,77,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,6,26,26,3), new GeneratedEnemyUnit(13,5,31,2,1), new GeneratedEnemyUnit(-9,15,23,9,4), new GeneratedEnemyUnit(-18,-3,55,17,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "696b1b92772f9ea82ce6028e547ee06d45d01341b1b045fa16eacf4f36030709");
        }

        private static void Case_00673()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 673,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,0,98,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,10,29,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,15,15,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,17,9,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,5,16,27,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e790462ab636df0ede65b8cb0a3b865c492c39aff53fa13dcab9357680460a04");
        }

        private static void Case_00674()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 674,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-16,16,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,1,51,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,16,52,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,11,86,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-2,60,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,0,6,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "d020cea13bca496bdaa85435364cbd3bf0ec7ba464ff8647f6f55269aff58b0e");
        }

        private static void Case_00675()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 675,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-7,74,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-1,20,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-1,65,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-3,63,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,19,47,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,10,15,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,20,81,3,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "a960336b219040bd941a34fcbedcc12e8f9ef39b0888d0b0cea4daf962ef1080");
        }

        private static void Case_00676()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 676,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,1,52,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,4,18,6,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "e35b65890df37116f17fe37a6384ecc6bec8427da886e9ddefbb7d3cba6ec498");
        }

        private static void Case_00677()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 677,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-4,89,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-2,28,23,3), new GeneratedEnemyUnit(13,5,24,4,1), new GeneratedEnemyUnit(-18,6,12,29,4), new GeneratedEnemyUnit(2,11,11,37,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "07818082a56af9a8710a539059a84b8c994561b9c53bbe86302a310e244792e3");
        }

        private static void Case_00678()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 678,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,4,23,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,16,65,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,2,54,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,14,85,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-8,83,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-6,25,48,2), new GeneratedEnemyUnit(-12,-11,82,19,3), new GeneratedEnemyUnit(2,6,22,23,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "9923b0d543103fe72c74304b1fe192769bc94ac8e793121385dcb493a07c76a8");
        }

        private static void Case_00679()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 679,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,8,81,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,15,57,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-3,16,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-6,66,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,8,98,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,18,57,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-18,31,25,3), new GeneratedEnemyUnit(-12,-10,5,41,2), new GeneratedEnemyUnit(11,6,58,18,1), new GeneratedEnemyUnit(9,-1,22,20,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "5eba94ac39ff53d4a87283315ba03f7e762be56238857bae26d84f958f06ad73");
        }

        private static void Case_00680()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 680,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,17,46,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,7,31,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,6,29,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,15,56,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-12,72,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,11,26,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,7,19,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,7,91,1,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "d0da19472a21bbc923f613013d409715d3bdb8a305c4b4693fe03532877a609d");
        }

        private static void Case_00681()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 681,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-16,81,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-13,49,39,2), new GeneratedEnemyUnit(-6,1,6,46,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "23bec7a81aeb9f9f7e8fedd7d3d7a27c32366dad04e490ea09bf48638c7998c0");
        }

        private static void Case_00682()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 682,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-19,95,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-3,51,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,17,79,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-9,56,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-19,15,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,14,56,40,1), new GeneratedEnemyUnit(-8,-10,50,34,3), new GeneratedEnemyUnit(-12,0,80,27,1), new GeneratedEnemyUnit(13,3,44,38,2), new GeneratedEnemyUnit(-16,10,21,9,3), new GeneratedEnemyUnit(6,-3,51,36,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "34b85f40309908df4a954ebf6d553b26f4e464c786da34863b40b8770614ff97");
        }

        private static void Case_00683()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 683,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-19,19,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,14,26,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,5,59,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-17,24,32,3), new GeneratedEnemyUnit(18,-12,80,14,1), new GeneratedEnemyUnit(-2,-7,74,35,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "085ecc32df4b6c294f74d7005e032f438ad0ee4df03c93e6d89ac516e1edd973");
        }

        private static void Case_00684()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 684,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-3,85,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-2,69,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-11,76,3,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "591c4c12c1709cd3c1ea3bcea19a31d064344e74077a5f0eef225c1ee7c3e110");
        }

        private static void Case_00685()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 685,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,11,46,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-12,72,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,15,58,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-6,28,34,1), new GeneratedEnemyUnit(19,11,21,38,4), new GeneratedEnemyUnit(12,18,34,37,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "1105d11240b7d00770e8b7eb3bd3f9d3582c606072c25eb81a532f9ff3196583");
        }

        private static void Case_00686()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 686,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,15,34,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,0,90,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,7,89,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,16,88,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-5,28,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-17,65,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-3,97,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-5,50,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,3,84,44,2), new GeneratedEnemyUnit(-17,-5,26,4,3), new GeneratedEnemyUnit(12,-5,30,16,2), new GeneratedEnemyUnit(13,19,74,26,2), new GeneratedEnemyUnit(2,0,74,37,2), new GeneratedEnemyUnit(16,12,23,47,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "dccea0fa147e3a6efc046d598feea9cef8ebec2092dd62fa42043e84a4cee4be");
        }

        private static void Case_00687()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 687,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-13,9,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-9,89,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,4,28,6,2), new GeneratedEnemyUnit(13,-4,44,35,3), new GeneratedEnemyUnit(18,-17,64,18,4), new GeneratedEnemyUnit(-7,-16,85,3,1), new GeneratedEnemyUnit(12,7,49,10,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "23d9286eed9a94f67a166ddce626539cce76e60b0ca8aae43c33c6694cd7dede");
        }

        private static void Case_00688()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 688,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,9,81,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-4,71,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,0,85,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,14,90,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,2,33,42,3), new GeneratedEnemyUnit(10,-2,46,16,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "753772812c8efe608136e7a29a7fe5ae10f01c7cce028e1ae50109231cd7b762");
        }

        private static void Case_00689()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 689,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,20,30,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-13,41,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-11,7,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,16,53,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,3,10,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-13,15,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "d10ac5f066704afb59887c8ba1fa5d7d4e08d73e627ba266df07883bd35f2f05");
        }

        private static void Case_00690()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 690,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,14,66,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-2,29,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-10,12,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-10,85,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,1,51,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,11,78,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-9,91,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-12,42,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,2,36,26,2), new GeneratedEnemyUnit(-9,-19,86,48,2), new GeneratedEnemyUnit(-11,-9,77,4,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "4fde9507816b0049cbe4b50108ba9302ddd85c914c543b350b2e551743a9ce8e");
        }

        private static void Case_00691()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 691,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,9,16,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,19,72,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-7,11,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,17,24,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,19,5,6,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "fd2f02f0c83f08c20bc9b6d6fa30858bf834762b443d3676137ccc8781acc6f6");
        }

        private static void Case_00692()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 692,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,17,71,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-15,10,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,20,90,43,3), new GeneratedEnemyUnit(-4,-14,51,40,3), new GeneratedEnemyUnit(-18,-3,66,4,4), new GeneratedEnemyUnit(-14,-18,94,28,3), new GeneratedEnemyUnit(18,9,40,7,2), new GeneratedEnemyUnit(-17,16,93,26,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "a39f4d8e976cb31c9664ba23651edd111cbba8510244a205180f7fc620589c72");
        }

        private static void Case_00693()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 693,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-18,8,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,13,15,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,17,75,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,2,22,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-17,43,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,17,41,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,3,52,17,4), new GeneratedEnemyUnit(-13,18,62,41,3), new GeneratedEnemyUnit(-17,-12,96,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "47c93d3f58eb886f3767b3f63098b5dff3f5355bd99dfc3804cf61e98e5ecb1c");
        }

        private static void Case_00694()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 694,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,5,51,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,16,48,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-17,19,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-20,69,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,6,44,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-15,79,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,10,31,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-17,55,2,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "8bd39ff6dfd8c76536e176fe9bcef625c0f3f522e0d094e32ada9cc542a94fb0");
        }

        private static void Case_00695()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 695,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,17,28,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,9,65,16,2), new GeneratedEnemyUnit(11,-13,46,11,2), new GeneratedEnemyUnit(3,-2,54,15,3), new GeneratedEnemyUnit(-14,17,89,6,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "720d43493b7c2edf5ec1ebccec4719e65a69b0c2a037e300c302b758d744b8b6");
        }

        private static void Case_00696()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 696,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-3,90,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,9,26,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,2,15,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-2,94,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-15,68,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,9,76,7,1), new GeneratedEnemyUnit(-8,19,49,12,1), new GeneratedEnemyUnit(-8,16,28,8,4), new GeneratedEnemyUnit(-12,-1,47,20,2), new GeneratedEnemyUnit(6,-14,8,9,4), new GeneratedEnemyUnit(20,12,52,3,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "f32a0b8ef1e210ff57c7f5f55bd6ad02379118acf8cb372879e2afebfc559ce1");
        }

        private static void Case_00697()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 697,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,4,81,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-19,93,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,11,8,1,2), new GeneratedEnemyUnit(-16,-17,95,1,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "4f08e113485b2110be1db14e290b06dffd64eaaffd1069c9390ba8de04ba2516");
        }

        private static void Case_00698()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 698,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-20,43,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,5,98,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-11,37,27,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "cbe62678e557ec4228e5058dda1da7e3238334de9f5b55f1a745b5374dea922f");
        }

        private static void Case_00699()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 699,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,13,22,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,15,90,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-5,67,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-12,43,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,3,98,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-11,80,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-20,78,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,19,21,27,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "26b2be926aab5cad30f366bb399587c476ed733e5ff30d39ab9402eff8bfdfb8");
        }

        private static void Case_00700()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 700,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,19,8,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-3,53,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,13,64,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,5,71,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-6,21,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,4,6,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-11,55,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-16,11,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,5,27,12,2), new GeneratedEnemyUnit(20,18,10,12,4), new GeneratedEnemyUnit(19,-13,10,17,3), new GeneratedEnemyUnit(6,0,12,19,1), new GeneratedEnemyUnit(-14,-4,44,4,1), new GeneratedEnemyUnit(20,17,80,38,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "d99d09cf3638a6c92ccd696c6e3f52fb27756f899603ac65126042a0c44619a9");
        }

        private static void Case_00701()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 701,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-13,68,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,18,96,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-7,61,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-18,68,49,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "1fcbad399a7b55dadb4ed61c9d573d090406bdff2258daf01428dfd4636a3c19");
        }

        private static void Case_00702()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 702,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-6,19,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,7,65,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,16,33,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,13,84,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,19,79,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,1,81,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-3,63,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,1,48,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,8,7,48,2), new GeneratedEnemyUnit(-5,-9,90,7,4), new GeneratedEnemyUnit(-12,7,78,26,3), new GeneratedEnemyUnit(-20,3,33,4,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "24c8e89494b943bf0c1c3ec29d0646f5a93cb2cdd8bd8cd3fe23d30ca73e65a3");
        }

        private static void Case_00703()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 703,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,6,11,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,7,34,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-9,47,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-1,13,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-2,79,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-6,100,46,2), new GeneratedEnemyUnit(11,13,69,11,1), new GeneratedEnemyUnit(-10,-6,21,9,4), new GeneratedEnemyUnit(-14,-9,15,37,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "9f6b8f89d77046da15fd29886bbdb6f26e643f34bf09a51fe95956c8d2c3d461");
        }

        private static void Case_00704()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 704,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,4,30,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-6,20,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-9,65,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,15,30,3,3), new GeneratedEnemyUnit(-3,9,54,23,3), new GeneratedEnemyUnit(7,-17,25,2,3), new GeneratedEnemyUnit(-6,-12,73,46,1), new GeneratedEnemyUnit(-15,1,56,32,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "0754d97735ab060b657050ef28b79a2a9c2c0bcb0a6042b6526b1d2c3ee05af4");
        }

        private static void Case_00705()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 705,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,11,51,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-11,54,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,6,21,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-11,37,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,20,88,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,14,65,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-13,24,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,12,25,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,2,6,39,1), new GeneratedEnemyUnit(-14,0,83,3,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "e5e4c2d74ee1fec30748bf45303fa9c2d6379fd39d3f8fa961d77bc02207f30f");
        }

        private static void Case_00706()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 706,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,8,32,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,19,24,30,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "23705c5e9c255101060fc3ef2628aa7fc78249f52c11b8b4f5fc1d8b4b791d41");
        }

        private static void Case_00707()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 707,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-13,71,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-5,50,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-12,25,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-4,11,34,1), new GeneratedEnemyUnit(-11,-4,33,21,1), new GeneratedEnemyUnit(-17,-9,16,41,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "9480711e5fa8a99cb20aa0210304db61d4bca7c76071cf4b7832b92550400036");
        }

        private static void Case_00708()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 708,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-8,26,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,13,79,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,1,23,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-14,86,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-5,99,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,19,9,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-20,74,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-6,63,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,10,6,15,2), new GeneratedEnemyUnit(18,-12,23,1,4), new GeneratedEnemyUnit(15,5,41,34,1), new GeneratedEnemyUnit(2,14,38,41,1), new GeneratedEnemyUnit(17,19,93,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "8a0ef8b21d72ca8801af30a8bdb752a343c4eb3bf57d42cb7797cf49acd50dc3");
        }

        private static void Case_00709()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 709,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-7,69,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-12,69,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-6,50,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,8,44,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,14,52,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-19,100,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,20,50,27,4), new GeneratedEnemyUnit(15,16,34,28,1), new GeneratedEnemyUnit(7,4,29,15,2), new GeneratedEnemyUnit(-15,-20,10,7,3), new GeneratedEnemyUnit(-16,8,98,37,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "3bf2bb582c9097ab7b62c6702ed2f5e4d2b21525aa6124a74cd55ede6f01daee");
        }

        private static void Case_00710()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 710,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-8,9,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-19,25,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,20,66,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,2,72,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,14,52,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,15,59,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,17,22,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,7,50,6,1), new GeneratedEnemyUnit(-9,10,92,4,4), new GeneratedEnemyUnit(16,-16,78,5,1), new GeneratedEnemyUnit(9,-7,83,20,2), new GeneratedEnemyUnit(-5,-8,32,20,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "2a0101a31f7ff82cb8632a7b723e2691fd5d18de16d04c976f6ac52b7aba4b6b");
        }

        private static void Case_00711()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 711,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,18,6,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,0,91,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,4,28,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-2,49,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,14,98,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,1,74,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-12,76,12,3), new GeneratedEnemyUnit(9,6,22,38,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "89c6ec0c9ba007feeeb35c62b0158099d91c2d754f548a0bb52af256e3523ff5");
        }

        private static void Case_00712()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 712,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,19,30,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-11,64,29,1), new GeneratedEnemyUnit(-11,-15,48,46,2), new GeneratedEnemyUnit(-13,8,54,35,2), new GeneratedEnemyUnit(6,-12,58,18,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "4af489e1ff1a9faf4ea393318b82bc0212de499db6f839e8a7faca259afeffa5");
        }

        private static void Case_00713()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 713,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-20,83,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,5,25,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,18,44,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-11,74,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,3,44,30,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "53be5876b25aebc84d8e6f45a2db527699f41d45592b74b26f9f581331762554");
        }

        private static void Case_00714()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 714,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,17,24,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-18,11,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,12,73,35,1), new GeneratedEnemyUnit(7,17,44,26,4), new GeneratedEnemyUnit(18,6,40,15,3), new GeneratedEnemyUnit(-13,0,64,17,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "0d19029ee614e5670e57923db2aa5f7f067718af3ad349c5942a6e55512056e2");
        }

        private static void Case_00715()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 715,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-19,86,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-14,16,39,2), new GeneratedEnemyUnit(-6,-12,19,20,4), new GeneratedEnemyUnit(19,5,99,4,4), new GeneratedEnemyUnit(17,-1,91,26,3), new GeneratedEnemyUnit(15,2,51,26,2), new GeneratedEnemyUnit(-3,-10,67,12,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "f02d0848cd5977b41f36b47ef2dda080c44e6eda6675762d67fb3c9d1ff5f7c6");
        }

        private static void Case_00716()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 716,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,19,56,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,17,13,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,8,17,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,17,58,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,7,77,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,9,57,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-4,13,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-13,67,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-16,15,10,2), new GeneratedEnemyUnit(10,-15,69,30,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "37c42d47c97a1bbd66601487163c5306563af19556fb32a461c1108ce394fd77");
        }

        private static void Case_00717()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 717,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-14,8,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-14,23,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,9,17,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,20,38,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-19,89,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,0,23,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-7,35,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-1,16,1,1), new GeneratedEnemyUnit(17,5,20,23,2), new GeneratedEnemyUnit(17,-15,92,20,1), new GeneratedEnemyUnit(-4,15,80,32,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "cde53604f999a9816c9f5290db527ede94a8c8076de6e59f81d04c61bdb136b5");
        }

        private static void Case_00718()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 718,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-15,8,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-15,22,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-11,88,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-7,67,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,10,67,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,4,6,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-9,21,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-9,91,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-4,11,47,2), new GeneratedEnemyUnit(9,-18,96,37,2), new GeneratedEnemyUnit(-13,16,15,24,2), new GeneratedEnemyUnit(20,-14,22,30,2), new GeneratedEnemyUnit(-17,7,28,36,4), new GeneratedEnemyUnit(9,-15,76,34,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "f3a05f5ecf91195cb18a145408e1a8e99db5d929ab09895c66f9f152e07b5773");
        }

        private static void Case_00719()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 719,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,18,57,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,3,61,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-5,50,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,10,74,14,4), new GeneratedEnemyUnit(1,-9,42,41,4), new GeneratedEnemyUnit(-4,-1,97,41,2), new GeneratedEnemyUnit(20,-10,96,28,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "d8bf48e3e1e9bdcb32e38b230b6a9d898b7fba6caa812b9847584ecb8697c9bf");
        }

    }
}
