using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard014
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_02520();
            Case_02521();
            Case_02522();
            Case_02523();
            Case_02524();
            Case_02525();
            Case_02526();
            Case_02527();
            Case_02528();
            Case_02529();
            Case_02530();
            Case_02531();
            Case_02532();
            Case_02533();
            Case_02534();
            Case_02535();
            Case_02536();
            Case_02537();
            Case_02538();
            Case_02539();
            Case_02540();
            Case_02541();
            Case_02542();
            Case_02543();
            Case_02544();
            Case_02545();
            Case_02546();
            Case_02547();
            Case_02548();
            Case_02549();
            Case_02550();
            Case_02551();
            Case_02552();
            Case_02553();
            Case_02554();
            Case_02555();
            Case_02556();
            Case_02557();
            Case_02558();
            Case_02559();
            Case_02560();
            Case_02561();
            Case_02562();
            Case_02563();
            Case_02564();
            Case_02565();
            Case_02566();
            Case_02567();
            Case_02568();
            Case_02569();
            Case_02570();
            Case_02571();
            Case_02572();
            Case_02573();
            Case_02574();
            Case_02575();
            Case_02576();
            Case_02577();
            Case_02578();
            Case_02579();
            Case_02580();
            Case_02581();
            Case_02582();
            Case_02583();
            Case_02584();
            Case_02585();
            Case_02586();
            Case_02587();
            Case_02588();
            Case_02589();
            Case_02590();
            Case_02591();
            Case_02592();
            Case_02593();
            Case_02594();
            Case_02595();
            Case_02596();
            Case_02597();
            Case_02598();
            Case_02599();
            Case_02600();
            Case_02601();
            Case_02602();
            Case_02603();
            Case_02604();
            Case_02605();
            Case_02606();
            Case_02607();
            Case_02608();
            Case_02609();
            Case_02610();
            Case_02611();
            Case_02612();
            Case_02613();
            Case_02614();
            Case_02615();
            Case_02616();
            Case_02617();
            Case_02618();
            Case_02619();
            Case_02620();
            Case_02621();
            Case_02622();
            Case_02623();
            Case_02624();
            Case_02625();
            Case_02626();
            Case_02627();
            Case_02628();
            Case_02629();
            Case_02630();
            Case_02631();
            Case_02632();
            Case_02633();
            Case_02634();
            Case_02635();
            Case_02636();
            Case_02637();
            Case_02638();
            Case_02639();
            Case_02640();
            Case_02641();
            Case_02642();
            Case_02643();
            Case_02644();
            Case_02645();
            Case_02646();
            Case_02647();
            Case_02648();
            Case_02649();
            Case_02650();
            Case_02651();
            Case_02652();
            Case_02653();
            Case_02654();
            Case_02655();
            Case_02656();
            Case_02657();
            Case_02658();
            Case_02659();
            Case_02660();
            Case_02661();
            Case_02662();
            Case_02663();
            Case_02664();
            Case_02665();
            Case_02666();
            Case_02667();
            Case_02668();
            Case_02669();
            Case_02670();
            Case_02671();
            Case_02672();
            Case_02673();
            Case_02674();
            Case_02675();
            Case_02676();
            Case_02677();
            Case_02678();
            Case_02679();
            Case_02680();
            Case_02681();
            Case_02682();
            Case_02683();
            Case_02684();
            Case_02685();
            Case_02686();
            Case_02687();
            Case_02688();
            Case_02689();
            Case_02690();
            Case_02691();
            Case_02692();
            Case_02693();
            Case_02694();
            Case_02695();
            Case_02696();
            Case_02697();
            Case_02698();
            Case_02699();
        }

        private static void Case_02520()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2520,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,6,10,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-10,14,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,17,33,2,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "c77c81e434d71409732d64dd33688ad5b227200369022f172ac9e21e8c0b4811");
        }

        private static void Case_02521()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2521,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,15,81,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-2,69,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-10,35,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-16,53,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,12,58,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,9,53,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-14,33,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-18,92,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,19,48,28,4), new GeneratedEnemyUnit(10,16,65,7,2), new GeneratedEnemyUnit(11,0,88,33,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "1ff30ace07452c5fcd6332cd9ba16333a5eeaed2ff17851dc94120bf3241ae1d");
        }

        private static void Case_02522()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2522,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-12,95,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,16,41,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,0,41,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-14,93,10,3), new GeneratedEnemyUnit(10,3,56,21,4), new GeneratedEnemyUnit(16,12,65,19,1), new GeneratedEnemyUnit(-5,1,98,29,2), new GeneratedEnemyUnit(0,-15,46,5,1), new GeneratedEnemyUnit(-5,11,13,45,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "e96e76e4eb46f56095b3c9bb37c87d9fb6b71b2670ff90e67bfb42e70db4103e");
        }

        private static void Case_02523()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2523,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-12,51,5,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "90ef2afb7c2d522f79952dc25662158e5d86bfc3e7e76282c721b440d376a950");
        }

        private static void Case_02524()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2524,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,15,14,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-13,52,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-10,62,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,10,47,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-20,95,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,19,44,40,4), new GeneratedEnemyUnit(-3,16,63,9,3), new GeneratedEnemyUnit(-20,3,50,11,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ecbc1725e4b080a1b1cd26fbd0be2cc3636bb7af3e60dab6b997203bd91cb906");
        }

        private static void Case_02525()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2525,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-10,30,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,1,61,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-20,7,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-6,34,35,3), new GeneratedEnemyUnit(1,9,64,42,1), new GeneratedEnemyUnit(-18,12,33,39,4), new GeneratedEnemyUnit(-2,-7,33,33,2), new GeneratedEnemyUnit(18,-7,49,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "9163059523e599d6235452f5408f32271988dcac6e11f6c1c7b1bde4ba93a590");
        }

        private static void Case_02526()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2526,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-9,61,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-5,44,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,3,77,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,2,7,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,13,72,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-18,19,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,1,6,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-11,77,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,4,90,3,4), new GeneratedEnemyUnit(-4,-9,52,35,3), new GeneratedEnemyUnit(-10,-3,36,42,4), new GeneratedEnemyUnit(12,7,21,38,1), new GeneratedEnemyUnit(14,7,92,34,2), new GeneratedEnemyUnit(4,-9,63,41,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "e8223d5224426e7a559811356d221bb99e0b0a355208af4324822c532c6fa614");
        }

        private static void Case_02527()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2527,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,6,11,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,7,64,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,17,56,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,9,28,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,19,74,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-18,49,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-18,59,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-2,91,18,3), new GeneratedEnemyUnit(-12,14,92,20,2), new GeneratedEnemyUnit(-4,4,28,8,2), new GeneratedEnemyUnit(-7,-2,72,20,1), new GeneratedEnemyUnit(0,12,41,40,4), new GeneratedEnemyUnit(17,-1,76,28,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "03eaf63a47d4003b71eb2b889dcae4d3e1be8ae86aa825499c047619b9518642");
        }

        private static void Case_02528()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2528,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,9,50,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,3,30,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,7,67,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-6,13,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-5,53,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-13,74,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-15,68,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-10,95,20,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "53ae29cbe78951ed1096a38a16e814189bb9bfa58d73429cf493299c0c293b06");
        }

        private static void Case_02529()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2529,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,17,14,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-11,57,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,5,86,49,4), new GeneratedEnemyUnit(-20,5,19,20,3), new GeneratedEnemyUnit(-3,16,60,44,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "5a853e1a00ee311e8c769bc6db4d8f6dfc0c9f9a8c975baabca39a805a98c0d2");
        }

        private static void Case_02530()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2530,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,3,87,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,16,83,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-4,98,44,4), new GeneratedEnemyUnit(0,7,99,7,2), new GeneratedEnemyUnit(-2,9,75,40,1), new GeneratedEnemyUnit(-3,-5,85,45,2), new GeneratedEnemyUnit(19,-6,22,10,4), new GeneratedEnemyUnit(-12,1,87,15,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "25b94536a6e5919155793c223f3a629a978987255a18527f5f677aff0aa8ee22");
        }

        private static void Case_02531()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2531,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,19,77,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,4,97,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-2,66,50,4), new GeneratedEnemyUnit(4,14,15,21,2), new GeneratedEnemyUnit(11,1,5,4,1), new GeneratedEnemyUnit(16,1,80,5,2), new GeneratedEnemyUnit(-16,20,47,41,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "051dce754e28a116e2aa3535ee7d4a2b37f7b549ce099497d8160d9e87f16e8d");
        }

        private static void Case_02532()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2532,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,15,55,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,2,73,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,17,71,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-14,13,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,16,5,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-2,6,11,3), new GeneratedEnemyUnit(20,-9,44,47,1), new GeneratedEnemyUnit(-4,10,95,21,3), new GeneratedEnemyUnit(18,-3,91,16,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "8bd50cdb0a4cee7cb2651223c057afc3fa2a931f69f63727e62d288412e66a73");
        }

        private static void Case_02533()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2533,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,0,9,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,15,61,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,5,71,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-4,11,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,19,45,12,3), new GeneratedEnemyUnit(-20,4,68,45,3), new GeneratedEnemyUnit(-5,16,98,1,3), new GeneratedEnemyUnit(-20,13,36,19,1), new GeneratedEnemyUnit(16,-15,65,5,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "c990c113f0ea2ea2eb19011ba2a62f88bc173347e6411696e399cb84544d84ec");
        }

        private static void Case_02534()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2534,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-5,8,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,20,50,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,17,76,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,2,30,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-12,61,39,4), new GeneratedEnemyUnit(-17,3,44,41,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "b1aa8a7b48a96079835dfd920cc79d8d1d8d782b4b849e52232dee96fd4c6809");
        }

        private static void Case_02535()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2535,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-10,66,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,12,34,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-8,30,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,13,26,15,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6981f3e07f2701dc7f6cf83329b174d5c92dce419e83b27620172f3ece6e2d1f");
        }

        private static void Case_02536()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2536,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-9,51,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-16,81,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-9,33,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,20,71,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-16,73,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,20,71,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-3,97,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-11,59,50,2), new GeneratedEnemyUnit(-19,0,85,21,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "441451078b2a5ab8e305d906d3b40e012341fdb85aac3cba4c656f8477473e62");
        }

        private static void Case_02537()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2537,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-3,55,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,7,25,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-19,86,1,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "6fe15409ae374c888288b1c20261be99ce2ee69fff7d3fd23a1e042303ddb3da");
        }

        private static void Case_02538()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2538,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,18,76,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,4,91,35,2), new GeneratedEnemyUnit(10,-12,85,6,3), new GeneratedEnemyUnit(16,9,85,26,4), new GeneratedEnemyUnit(-11,5,75,6,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "31b068e74863196aa9acc5a1bd919d6ed1d7c53daf45cc6576e6c31a8818fbf6");
        }

        private static void Case_02539()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2539,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-3,41,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-11,75,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,14,80,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,15,36,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,19,19,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-18,77,7,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "4a60ff6390b232a2e441a4222b6242144f89c7f4ccbeb3ab05a2557c95ab8b62");
        }

        private static void Case_02540()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2540,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-7,52,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-16,19,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,15,35,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-19,15,7,4), new GeneratedEnemyUnit(2,-11,33,50,1), new GeneratedEnemyUnit(0,-13,35,48,1), new GeneratedEnemyUnit(-15,15,63,38,2), new GeneratedEnemyUnit(-14,-16,44,19,3), new GeneratedEnemyUnit(-12,-17,85,45,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "37025f5687938da066790dd89686e165a3de19ae04d214619828d3ac0a99d4f6");
        }

        private static void Case_02541()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2541,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,3,19,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,2,36,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-16,24,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,2,64,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-20,74,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-7,33,18,4), new GeneratedEnemyUnit(11,18,19,46,1), new GeneratedEnemyUnit(11,-11,21,8,3), new GeneratedEnemyUnit(12,-6,72,40,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "fee7aafe316f3e59b2e1921ff1981313470534743f13dff1f716f7e70f450514");
        }

        private static void Case_02542()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2542,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,20,67,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,5,85,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,15,78,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-7,100,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-16,17,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-13,76,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,14,13,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-13,27,18,4), new GeneratedEnemyUnit(8,-12,41,28,4), new GeneratedEnemyUnit(-12,-19,46,36,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "c9b27bcdf0ee41d341ce6b401d6fbb71128c1c217291a573689d35aa691305d8");
        }

        private static void Case_02543()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2543,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,9,94,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,1,74,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-5,34,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-19,81,17,1), new GeneratedEnemyUnit(-3,8,31,49,4), new GeneratedEnemyUnit(-6,-7,96,25,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "19977649dbbf468145d63a305c9b69849eb4a4cd0a8f15775fcc118431ffd3d6");
        }

        private static void Case_02544()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2544,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-20,15,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-3,81,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,3,61,24,4), new GeneratedEnemyUnit(-11,19,85,6,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "558cb7d50802e618402ac9189c1a9469f7b0fbe3e601f7e854b7f6f144f2ca33");
        }

        private static void Case_02545()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2545,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-1,36,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-14,17,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-12,30,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-15,52,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,7,74,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,12,52,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-7,44,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-9,97,5,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "10356f7dfd7fc6dd32028d95a01975be9e29a22bd54af68ad6ecc223fef631b9");
        }

        private static void Case_02546()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2546,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,2,36,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,12,63,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-10,70,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,10,99,28,2), new GeneratedEnemyUnit(-20,-17,13,46,3), new GeneratedEnemyUnit(8,17,27,1,3), new GeneratedEnemyUnit(19,-13,40,42,3), new GeneratedEnemyUnit(-14,20,49,40,3), new GeneratedEnemyUnit(14,-15,94,19,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "f52565351a2e35e9b7cc6e457b4bc227fc137ce8c332f54bc28c2043b4649f6f");
        }

        private static void Case_02547()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2547,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,15,63,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-14,74,37,1), new GeneratedEnemyUnit(-10,10,94,3,3), new GeneratedEnemyUnit(-2,-14,52,35,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "286a4bc505c29e9f2e7f4052db8f6f1ea2c7bff5f25efa2242dd2ebabd7ae5fe");
        }

        private static void Case_02548()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2548,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-6,18,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-7,25,44,3), new GeneratedEnemyUnit(-6,14,52,39,3), new GeneratedEnemyUnit(6,-17,82,17,3), new GeneratedEnemyUnit(7,19,53,42,1), new GeneratedEnemyUnit(-12,1,49,24,2), new GeneratedEnemyUnit(11,2,51,43,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "3457848847202003c2e94b5e97993a5209b01deaf0725502296b99cc88157b10");
        }

        private static void Case_02549()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2549,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-14,86,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,4,55,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,12,80,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-14,62,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,17,13,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-12,80,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-9,52,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-5,12,12,2), new GeneratedEnemyUnit(0,-10,98,18,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "aabdacbe7fcb850252f98721f2397b2514d4520b4f26faa38640aa8a36aa7083");
        }

        private static void Case_02550()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2550,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,11,20,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,6,43,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-11,20,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,3,32,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,6,10,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-17,52,9,4), new GeneratedEnemyUnit(-2,-19,68,43,1), new GeneratedEnemyUnit(6,-5,76,38,2), new GeneratedEnemyUnit(10,-13,52,34,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "852ec97cdea6f4cf853130f83368bb2a37194939754a7d63e1b3b2b7163cf117");
        }

        private static void Case_02551()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2551,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,10,19,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,1,77,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,16,9,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-9,98,14,3), new GeneratedEnemyUnit(-8,16,37,36,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "c7b104ab704d2e8c5650c6a4d36b1c78c78a1a209f366a4ad09f52a5c21c4b18");
        }

        private static void Case_02552()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2552,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,10,67,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,4,60,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,8,35,4,3), new GeneratedEnemyUnit(8,14,83,5,4), new GeneratedEnemyUnit(-18,11,31,45,4), new GeneratedEnemyUnit(-4,16,62,11,3), new GeneratedEnemyUnit(18,-16,89,15,2), new GeneratedEnemyUnit(18,-5,53,22,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "4d2da38da2796b988511a2b48a1a82ab4e76195fe4592b3913f3b649d991b8d7");
        }

        private static void Case_02553()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2553,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-17,38,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,4,81,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-3,8,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-7,82,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,2,28,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,15,96,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,3,20,18,2), new GeneratedEnemyUnit(20,-13,11,23,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "3ed9112b08467b0d3e3f982fb6fb6791a944f951d335ce3e466878011afdee5f");
        }

        private static void Case_02554()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2554,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,9,64,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,20,80,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,2,75,10,2), new GeneratedEnemyUnit(3,-7,8,31,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "fa41e7ddc63603bbc30bcfc1205fb1544fa4906241c56bc1dc50df173bf7adf2");
        }

        private static void Case_02555()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2555,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-11,95,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-14,29,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,11,13,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,12,46,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,8,9,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,20,34,25,4), new GeneratedEnemyUnit(-15,-20,7,14,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "75476655196342e912c7908d7a15b09a92be6122425703e438d00d8b4ef23e40");
        }

        private static void Case_02556()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2556,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,3,18,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-14,36,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,0,56,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,3,42,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,6,81,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,9,5,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,12,36,7,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3de28c343164ea31750b5fde5cf106a8f309ce5a17cd25432abbc9245b27585b");
        }

        private static void Case_02557()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2557,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,16,60,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,11,53,32,1), new GeneratedEnemyUnit(1,-10,47,9,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8e6b95184742bd407e6e21838e81b9d78c22ca22cafc6a561a083f1b3d72fb75");
        }

        private static void Case_02558()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2558,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-16,78,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-8,24,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,19,59,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-15,97,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-8,7,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,7,50,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-6,96,1,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b857af8d6a2f8d911cdb513f837e426a54f0a9e8617c8830be9d91efde19e97a");
        }

        private static void Case_02559()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2559,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,4,53,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,19,80,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,9,74,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,5,88,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-4,53,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,1,91,38,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "7d63062e2cb49109374a573712f1bc82e69c03457a13265eabfe96b8889d261b");
        }

        private static void Case_02560()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2560,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-8,64,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-5,42,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,19,100,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,5,64,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-10,45,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,2,64,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-3,10,22,1), new GeneratedEnemyUnit(0,16,35,29,2), new GeneratedEnemyUnit(20,7,45,25,3), new GeneratedEnemyUnit(11,7,89,33,4), new GeneratedEnemyUnit(-1,9,79,18,1), new GeneratedEnemyUnit(6,19,14,39,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "c5d15aeeab7ac23f406f6a18ca164d412a50ee208dda2ab03d5fb061410591d3");
        }

        private static void Case_02561()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2561,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,6,86,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,15,43,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-9,13,28,1), new GeneratedEnemyUnit(17,-13,53,50,1), new GeneratedEnemyUnit(4,8,85,34,2), new GeneratedEnemyUnit(3,6,81,10,3), new GeneratedEnemyUnit(10,-7,28,47,3), new GeneratedEnemyUnit(-12,13,54,16,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "fdfd73b7967b66860b9bf8eafd55a51421aa89d50e3791cee7c4032d8161da38");
        }

        private static void Case_02562()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2562,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-9,25,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,12,20,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-10,21,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-4,21,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-9,35,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,15,64,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,11,82,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-14,69,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,20,96,29,3), new GeneratedEnemyUnit(7,-9,75,4,1), new GeneratedEnemyUnit(8,-12,83,19,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "10348bf30c148ac34782af6c5ed5324d14817ad618d38e7995d88b156a7f6160");
        }

        private static void Case_02563()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2563,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-11,5,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-3,34,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,0,39,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,18,75,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,18,100,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-17,76,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,8,63,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-4,79,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,13,55,39,3), new GeneratedEnemyUnit(18,-16,35,1,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "5a8d7f6def6a07ff748c9cd01e48308b45f3951541433c8970c61c25ab301158");
        }

        private static void Case_02564()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2564,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-14,65,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-9,8,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-8,100,4,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e956b6a3f807da437539fab8aab418e748e8e58a996f69926457986934f9b93e");
        }

        private static void Case_02565()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2565,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,16,71,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-2,32,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,14,53,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,20,40,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,5,27,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-10,53,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,1,66,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-3,25,18,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "bae3984bb5c930338c2eab68d004c5e0fc52c26c1894ede79068a350279fd6ed");
        }

        private static void Case_02566()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2566,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-18,76,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,7,96,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-11,95,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-15,51,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-15,94,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,10,70,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,1,92,32,4), new GeneratedEnemyUnit(-2,18,79,47,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "bae60ce8d662a54f8bcf0fcb370773eb63d560362ece1bd60e68e05793866324");
        }

        private static void Case_02567()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2567,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,7,48,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-19,44,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,16,75,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-13,9,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-18,38,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,7,49,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,16,70,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,15,9,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,10,23,30,2), new GeneratedEnemyUnit(0,18,65,25,3), new GeneratedEnemyUnit(-3,-20,19,12,1), new GeneratedEnemyUnit(14,-7,38,21,1), new GeneratedEnemyUnit(3,9,37,8,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "4d698570ab44c86232d5219ad06d6b006779f4d5451abc307f1036b36d7ec496");
        }

        private static void Case_02568()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2568,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,1,97,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-2,55,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-18,26,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-15,15,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,1,44,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,10,92,26,4), new GeneratedEnemyUnit(-17,15,22,41,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "f0269d8f81aa716d73c135de069e72ee9c14d1c32e062a4d06e0fe6250ca7160");
        }

        private static void Case_02569()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2569,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-2,29,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,6,91,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,5,81,49,2), new GeneratedEnemyUnit(5,1,18,15,1), new GeneratedEnemyUnit(10,5,55,3,2), new GeneratedEnemyUnit(3,14,63,38,3), new GeneratedEnemyUnit(20,14,91,45,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "9fd7b3622597752d1d018f92df675f5942d41e6857973514a597c999c04066e1");
        }

        private static void Case_02570()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2570,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,16,23,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-11,77,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,9,37,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,3,9,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-10,86,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,11,68,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-17,23,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,14,97,34,2), new GeneratedEnemyUnit(-15,-12,68,33,3), new GeneratedEnemyUnit(-6,20,17,45,3), new GeneratedEnemyUnit(5,19,14,31,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "8bea35d0692ddf163e30e6cbe88d11979775c69b7d2136d7195dd3da219cdabd");
        }

        private static void Case_02571()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2571,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,6,14,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-20,84,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,14,11,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,17,96,43,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "85c2b267e98d7f957f1a4e0ca24079f0664e7dd5737ef053d34d39b96022d874");
        }

        private static void Case_02572()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2572,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-10,52,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-13,51,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,1,59,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,16,30,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-16,55,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-16,65,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,2,11,19,4), new GeneratedEnemyUnit(20,20,46,39,4), new GeneratedEnemyUnit(-6,12,76,38,4), new GeneratedEnemyUnit(0,-16,95,29,1), new GeneratedEnemyUnit(-18,-9,13,3,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "c3a58056e7f4bc1772982d409d9538181d981daaf5fbf84ff6c6e0b7a56cf834");
        }

        private static void Case_02573()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2573,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,16,79,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,0,44,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-15,78,43,4), new GeneratedEnemyUnit(-8,19,95,4,2), new GeneratedEnemyUnit(9,4,9,8,2), new GeneratedEnemyUnit(-1,20,99,37,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "fefadf6f58e143f189d8ed18ed8fc0587d7916a3d675c9217019232c225fccb9");
        }

        private static void Case_02574()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2574,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,14,80,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,17,43,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-17,11,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-18,38,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-18,39,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-1,26,35,2), new GeneratedEnemyUnit(15,-14,57,49,4), new GeneratedEnemyUnit(7,-12,58,49,1), new GeneratedEnemyUnit(-9,-6,75,14,4), new GeneratedEnemyUnit(17,15,84,32,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "54693d441b412eb8995442683b33abeda1f297fb28b1b911ae4b2a7573c67d83");
        }

        private static void Case_02575()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2575,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-15,92,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-5,70,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-9,13,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,1,58,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,20,42,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-2,51,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,0,43,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,4,68,49,4), new GeneratedEnemyUnit(3,-9,87,10,2), new GeneratedEnemyUnit(-5,15,57,18,2), new GeneratedEnemyUnit(9,5,85,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8f5bc3f63df992599e6f9515677d044a7da34b699e4f0883c682a4900c836a4c");
        }

        private static void Case_02576()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2576,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-3,6,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-8,99,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,1,40,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,5,73,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-9,80,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,4,94,16,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "b2d3f66cd58e6b8df883f579ae410dfdfb588ac2751c76a93c099fb0fed16fe0");
        }

        private static void Case_02577()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2577,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-9,88,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-17,17,3,1), new GeneratedEnemyUnit(4,18,23,41,3), new GeneratedEnemyUnit(13,16,40,44,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "4173509a3664fa8fbc925c6604c3129c52b9a228fc990b981309f6b1f2e3d782");
        }

        private static void Case_02578()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2578,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-8,70,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-18,31,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-18,78,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,9,45,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-8,46,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,8,70,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,13,36,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,14,30,4,1), new GeneratedEnemyUnit(-12,14,46,4,1), new GeneratedEnemyUnit(-12,-9,35,30,1), new GeneratedEnemyUnit(2,4,94,3,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "bdadd79ec032c6f56deeec25885da9a95730c8f1fd97fbda62dab442450c7325");
        }

        private static void Case_02579()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2579,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,4,36,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-14,62,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,19,96,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,11,49,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,15,8,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-8,71,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,11,23,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-2,69,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,20,87,45,4), new GeneratedEnemyUnit(-2,14,52,11,3), new GeneratedEnemyUnit(7,-9,59,44,4), new GeneratedEnemyUnit(-13,-7,56,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "a9b702cfe5d3aafd39ef1b1c4236337b22bfd7665424494debeb318310ff9366");
        }

        private static void Case_02580()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2580,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,4,84,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-6,94,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-6,35,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,15,26,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-16,93,18,4), new GeneratedEnemyUnit(18,16,53,20,4), new GeneratedEnemyUnit(-3,-13,93,8,2), new GeneratedEnemyUnit(-10,3,55,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "06abc4ab6968a1d6c5012bd140a7b174fcebdd19b8d49780ae8b134051653a83");
        }

        private static void Case_02581()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2581,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-3,65,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,11,95,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,2,90,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-17,97,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,15,76,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,8,16,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-6,28,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,2,13,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,10,33,43,4), new GeneratedEnemyUnit(19,16,72,25,4), new GeneratedEnemyUnit(2,16,13,5,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "9ced5536e8e594eb6fa23c6b4ca7e5c81b6df8bf8853092a258122028c4e245e");
        }

        private static void Case_02582()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2582,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-7,60,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,3,92,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-6,98,46,3), new GeneratedEnemyUnit(-7,-9,78,2,2), new GeneratedEnemyUnit(-4,-14,37,16,3), new GeneratedEnemyUnit(14,-17,9,42,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "5b08a70c82470c9c621e5c5fbaba522a249efecda23400358bdef4b8817976fa");
        }

        private static void Case_02583()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2583,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,20,9,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,5,7,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-5,55,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-19,7,11,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "1a292631ac485a4e4c2584169bb4c01ee3d092afe7456a4abf393f0cf4a1c5cd");
        }

        private static void Case_02584()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2584,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,14,78,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-17,61,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,9,92,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-18,47,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-6,18,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,10,41,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-20,20,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,7,36,3,1), new GeneratedEnemyUnit(-10,9,80,1,4), new GeneratedEnemyUnit(17,-10,95,10,2), new GeneratedEnemyUnit(3,-17,29,42,1), new GeneratedEnemyUnit(1,-6,59,21,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "0a6c4202b378f59632ab385df8d23d69f02839a1eb7ce25605982603ba8fbfd0");
        }

        private static void Case_02585()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2585,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-7,32,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,7,57,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,9,71,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-17,63,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-13,97,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,0,86,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,8,23,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-3,47,6,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "f500a1129e191ad8fe160e134f6effbe057e10bab4976534af8d8c96051204cc");
        }

        private static void Case_02586()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2586,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-13,33,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,10,61,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-6,86,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,5,64,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,20,21,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-20,45,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-11,21,13,2), new GeneratedEnemyUnit(2,13,19,25,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e460abb7ebba0600e609b24f89b8435f03151db4bfec20f208ef7ff765e4bd17");
        }

        private static void Case_02587()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2587,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,16,61,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,2,87,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-4,57,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-7,69,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,2,42,47,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c6ce1eef4cce0ee0aa4b659a0e57ee9789dc4c5d5e025934b27f81cdf8c8c924");
        }

        private static void Case_02588()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2588,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,15,23,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,6,75,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-5,53,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,15,86,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-1,56,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-4,5,6,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "18b78cb46e710a94c375621fe8441bf277fe58b8c4e6cf1a201edb0ea5140c80");
        }

        private static void Case_02589()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2589,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-3,62,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-6,90,14,3), new GeneratedEnemyUnit(-20,-10,84,5,3), new GeneratedEnemyUnit(12,-8,14,18,3), new GeneratedEnemyUnit(-13,-3,39,31,3), new GeneratedEnemyUnit(-11,-11,100,33,2), new GeneratedEnemyUnit(10,-15,82,31,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "2da77eae061b18c445e81af8ccb195e29966e4a81d58b394187843b1462f618c");
        }

        private static void Case_02590()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2590,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-6,96,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,9,44,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-1,38,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,3,5,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,2,78,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,7,47,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,20,31,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-15,25,2,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "2a011799f43190b546a5db6a8d88e9bea73502d88aa1b10764302d5efc1ac55a");
        }

        private static void Case_02591()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2591,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-9,20,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-6,19,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,8,74,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-2,98,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,9,99,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-15,39,28,1), new GeneratedEnemyUnit(-5,-10,36,33,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "0b0057df1561d704c88636d9b2248952915e19e2cbe81add57f3cc855024af99");
        }

        private static void Case_02592()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2592,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-5,21,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-3,85,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-5,47,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-12,9,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,15,26,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-20,74,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,8,16,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,20,31,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,9,17,43,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2f4870c192c9ff0b738a6eb7727ab87812ccc110bc03922a2e7f98ed35c52f38");
        }

        private static void Case_02593()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2593,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-19,51,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-17,68,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,14,66,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,17,50,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,15,55,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,10,63,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,0,95,3,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "07bae1db3ff8f4770b5ba9ecce1b708f30e9f2159f2e985bd0096424a5ff5cb2");
        }

        private static void Case_02594()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2594,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-12,69,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-15,37,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,7,94,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-7,48,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-16,22,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,8,60,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-5,8,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-15,65,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,6,88,45,1), new GeneratedEnemyUnit(2,-16,50,16,2), new GeneratedEnemyUnit(-19,5,10,41,1), new GeneratedEnemyUnit(-16,-7,81,48,2), new GeneratedEnemyUnit(-12,-11,5,10,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "bdad5a7e625bafe5f1689750b7e6993c9f9531cecf0381eb8f2b0dc6fad4da3c");
        }

        private static void Case_02595()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2595,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,1,20,7,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "b4b7a495bf191b9ce0cfcecbbe4cdcfc06fefaa7f77c47e504a0c1c374a24faf");
        }

        private static void Case_02596()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2596,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-5,74,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-20,5,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-7,78,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-19,32,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-14,83,46,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "b435cddf1dbdb79d94b055c41652725e9067f5f01fb274fd20df035a0339e6ee");
        }

        private static void Case_02597()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2597,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-7,33,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,9,81,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,11,97,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-8,58,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,1,93,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,5,89,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-15,91,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,16,5,42,1), new GeneratedEnemyUnit(-13,-18,18,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8e4e97e636e3899dfd5c691ecf96b6847bb77dbf18de427b33d730b3209197eb");
        }

        private static void Case_02598()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2598,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-17,23,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,20,32,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-12,82,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-19,53,11,3), new GeneratedEnemyUnit(1,-7,48,37,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "bb590cbbb4faea3194d7daf9c94c68d9f9e8a2a6a37ba7bb0dfd053da266fd72");
        }

        private static void Case_02599()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2599,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,16,61,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-18,18,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,9,19,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-11,56,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-9,62,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,16,28,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,17,37,22,4), new GeneratedEnemyUnit(-3,14,54,21,3), new GeneratedEnemyUnit(-15,15,39,45,4), new GeneratedEnemyUnit(17,-1,74,29,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "ca35540d6c88d2770fd27d099b11bc850bb7ded679f11e86366c7769d24427b4");
        }

        private static void Case_02600()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2600,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,0,10,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,18,59,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-20,90,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,5,70,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-3,13,31,1), new GeneratedEnemyUnit(-12,15,73,2,1), new GeneratedEnemyUnit(-13,-2,42,38,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "398dd1366d160812a9baec6d2eb7ceeabc1136561fcc2fccd63e01717d048393");
        }

        private static void Case_02601()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2601,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-4,94,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-1,13,15,3), new GeneratedEnemyUnit(6,-18,83,5,3), new GeneratedEnemyUnit(-2,-4,17,36,1), new GeneratedEnemyUnit(-14,19,9,9,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "00368ec7071db7b389b97133accc22a02f9b1ac30b1ce489edbec5c8e2430fad");
        }

        private static void Case_02602()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2602,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-14,74,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,4,23,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,18,18,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-4,82,44,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "6f94e14534bf96a9d5db708e31fc0f8c41864c9398422db63244749defe8659c");
        }

        private static void Case_02603()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2603,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,13,94,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-4,92,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,20,94,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-19,15,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-1,66,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,1,74,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,2,23,15,1), new GeneratedEnemyUnit(3,-17,18,46,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "97dcde2a0cc35ddcfb6cb8c0a66f47e318913a541ade645dad4b7055ed6dcc07");
        }

        private static void Case_02604()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2604,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,14,92,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,14,84,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,3,8,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,9,59,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-16,71,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-18,78,4,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "56f62e9f0427b36898fea90898a3870285fea9479d20ab678d5b5a9879371409");
        }

        private static void Case_02605()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2605,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-14,35,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-2,91,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,11,92,4,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "f17fce417915ffce16a3fedc63396a092ac97279b51baa84f8b5836a6331faf2");
        }

        private static void Case_02606()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2606,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,19,17,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-13,13,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-19,96,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-17,19,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,7,93,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-9,14,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,4,5,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,19,95,38,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a21368077c320136b14de51b9ce9129fdae56e5c4bee00393dfc1e4d670f17ed");
        }

        private static void Case_02607()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2607,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,1,99,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-6,8,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-20,89,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,4,5,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-13,93,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-7,26,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-3,30,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,14,25,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,0,91,22,1), new GeneratedEnemyUnit(-13,2,70,2,4), new GeneratedEnemyUnit(11,3,51,2,3), new GeneratedEnemyUnit(-18,16,92,35,1), new GeneratedEnemyUnit(0,-13,51,44,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "6173039a480639bec6117f7a8a23a35c294bccdcd8cbf04e17986a4aabe07730");
        }

        private static void Case_02608()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2608,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,19,27,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-8,49,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-4,73,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-13,52,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-4,13,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-10,37,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-10,31,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,11,96,50,1), new GeneratedEnemyUnit(2,-9,97,23,1), new GeneratedEnemyUnit(3,-10,74,38,4), new GeneratedEnemyUnit(14,9,58,41,4), new GeneratedEnemyUnit(19,-20,12,15,1), new GeneratedEnemyUnit(4,7,43,17,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "f5315cc190769671520e2df291f4528dfd60d40fce2e10abdafaa658d65282fb");
        }

        private static void Case_02609()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2609,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-10,97,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,12,9,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,12,37,5,4), new GeneratedEnemyUnit(-10,-10,14,8,2), new GeneratedEnemyUnit(-7,3,33,5,1), new GeneratedEnemyUnit(18,-7,96,34,4), new GeneratedEnemyUnit(16,-1,64,28,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "75b0e6bea5b79e78f6a6bec47e0f3372624ab7b54a80232b82ded0e4889500da");
        }

        private static void Case_02610()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2610,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-20,85,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-17,45,33,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "06fe4b0f8bb89f89daf1df5aad5e710a8de1cc8300c21fe07bdb0257a4d76425");
        }

        private static void Case_02611()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2611,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-5,31,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-3,27,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,17,12,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-6,90,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,11,66,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,15,73,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,11,68,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-9,27,8,4), new GeneratedEnemyUnit(-13,-18,73,39,3), new GeneratedEnemyUnit(-16,-14,12,18,1), new GeneratedEnemyUnit(-1,2,95,30,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "01f301ccb744a06b808532d6085e43b85a62a894a76345374cf5662cd2e60659");
        }

        private static void Case_02612()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2612,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-4,79,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,15,46,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,20,18,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-9,66,26,3), new GeneratedEnemyUnit(-5,3,46,18,3), new GeneratedEnemyUnit(-4,-3,87,45,4), new GeneratedEnemyUnit(-9,-10,69,8,4), new GeneratedEnemyUnit(4,-3,58,23,3), new GeneratedEnemyUnit(-16,-19,10,50,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "7c18892e45f0520dff617b5d81abe8e406e7bf50e72a288d53012ead1051786e");
        }

        private static void Case_02613()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2613,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,8,47,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,18,90,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,0,12,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-7,71,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,13,15,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,11,60,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,1,96,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-2,65,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-17,70,30,2), new GeneratedEnemyUnit(3,-15,93,32,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "815892c6c04ae7fc3ce673ea7dd237704d18361389749a28f83602352fa3f818");
        }

        private static void Case_02614()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2614,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-17,15,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-7,17,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-5,93,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,20,10,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,0,100,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,4,73,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-9,27,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,0,19,49,4), new GeneratedEnemyUnit(9,11,78,1,1), new GeneratedEnemyUnit(-10,-4,18,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "de81c3d85df87eaf838528d61d40a0c8eec35a7aabfd103548ed6f4c261107e4");
        }

        private static void Case_02615()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2615,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-1,12,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,7,27,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,13,18,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,19,12,49,2), new GeneratedEnemyUnit(9,-9,66,38,1), new GeneratedEnemyUnit(14,11,37,6,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "50b9620a25464136693735c4f97e5ee5277f28facbf0b3073c48a7c1ee37f580");
        }

        private static void Case_02616()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2616,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,12,34,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,16,61,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-4,33,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-7,34,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,6,51,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-15,7,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,12,63,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-2,88,8,4), new GeneratedEnemyUnit(-12,-3,42,18,1), new GeneratedEnemyUnit(-11,-5,66,26,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "124ae38eca7dfa8ffa609eb66f8995c0bdcee9a5e13f3d66575b78db8f1cfedd");
        }

        private static void Case_02617()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2617,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,1,68,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,0,90,6,1), new GeneratedEnemyUnit(-7,3,55,26,2), new GeneratedEnemyUnit(8,4,58,45,2), new GeneratedEnemyUnit(-9,-9,26,13,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "8cb16145a206027b7cbe28d8aaec726d8e66a0a239a23c9bd4c9e93abc9aefdf");
        }

        private static void Case_02618()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2618,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-17,48,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-18,20,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,3,30,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-4,84,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,17,63,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-19,13,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-7,71,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-17,17,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,1,39,29,4), new GeneratedEnemyUnit(6,-10,58,48,4), new GeneratedEnemyUnit(4,-3,98,7,1), new GeneratedEnemyUnit(-20,-4,74,43,1), new GeneratedEnemyUnit(19,12,24,41,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "db5dc27f9433278b39aada664d125b0a0145774c82bacb5624e8b5620a84d261");
        }

        private static void Case_02619()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2619,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,7,53,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,19,10,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,18,93,48,3), new GeneratedEnemyUnit(-11,-1,50,38,4), new GeneratedEnemyUnit(11,13,71,17,2), new GeneratedEnemyUnit(10,5,66,16,2), new GeneratedEnemyUnit(-15,-9,17,17,2), new GeneratedEnemyUnit(-19,-19,86,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "20aa9e478620080f50343bcc0e9311eafbc21b3741deb13194372925f3966f57");
        }

        private static void Case_02620()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2620,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,0,95,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-10,70,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-13,43,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,9,5,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-9,88,8,2), new GeneratedEnemyUnit(-11,-7,34,14,2), new GeneratedEnemyUnit(-9,-4,40,15,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "2e3fae201e62c04030ef48fa047a38e05bd71f029d76dbfd3fd7fafdc56dfd6f");
        }

        private static void Case_02621()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2621,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-8,40,2,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "7a72e86539fe21f104e1b6441a3fb9b60827090c3b1d31636c66ce10fc28b49c");
        }

        private static void Case_02622()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2622,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,9,90,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-17,61,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,18,24,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,12,50,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,14,54,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,10,62,30,2), new GeneratedEnemyUnit(17,10,33,49,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "7fbabe1f5c287f078de282c8fe518a24fea3dcbda88e3ca31d4694b741fe64f5");
        }

        private static void Case_02623()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2623,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,16,84,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-10,18,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-6,18,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,18,12,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,8,26,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,1,9,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-9,92,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,11,87,16,2), new GeneratedEnemyUnit(19,16,28,5,1), new GeneratedEnemyUnit(-8,18,99,11,3), new GeneratedEnemyUnit(-12,17,78,15,1), new GeneratedEnemyUnit(7,-8,61,20,1), new GeneratedEnemyUnit(-11,11,77,32,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "5da4d816878dde1a4a937c0d814aacf5b574e3bdf6825840dccf56bbaa4289f3");
        }

        private static void Case_02624()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2624,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-6,9,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,5,100,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,17,64,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,1,21,26,4), new GeneratedEnemyUnit(17,3,69,14,1), new GeneratedEnemyUnit(7,-18,6,31,2), new GeneratedEnemyUnit(-3,3,8,49,3), new GeneratedEnemyUnit(20,14,32,11,3), new GeneratedEnemyUnit(-18,-6,64,32,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "e4afd9b56725d726b95a868db8cb0bc377f12358ab9b31bd8f9a8fd4bd18329d");
        }

        private static void Case_02625()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2625,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-1,9,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-4,86,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,9,53,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,3,81,7,3), new GeneratedEnemyUnit(-2,-16,7,36,4), new GeneratedEnemyUnit(15,-1,58,36,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "a6cec4becea55d6c8021956170fa40fb1bc2ec92afd4729b285da029f9d2ff6e");
        }

        private static void Case_02626()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2626,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,9,48,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,18,93,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,0,100,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,8,52,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-8,42,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-6,55,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-6,24,38,4), new GeneratedEnemyUnit(-7,-8,77,7,3), new GeneratedEnemyUnit(7,1,75,11,4), new GeneratedEnemyUnit(17,-4,51,43,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "8caa660aa5e021ecbd94fa5123c80b581230951232953c9fd3876ce8db46869e");
        }

        private static void Case_02627()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2627,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,9,37,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,13,75,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,16,66,16,3), new GeneratedEnemyUnit(3,13,8,8,1), new GeneratedEnemyUnit(-4,-20,66,19,1), new GeneratedEnemyUnit(15,2,35,17,2), new GeneratedEnemyUnit(16,5,79,47,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "900923f15f08c16cdf783baaa6030aeb72f9cae7fa24a91c53bfe0bee5c0305f");
        }

        private static void Case_02628()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2628,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,12,98,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-6,32,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-13,84,7,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "97208b1aab0d57e5ff7ababa87848b682943060567d10d03942e2a96ab9625b7");
        }

        private static void Case_02629()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2629,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-10,23,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-13,83,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-11,76,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-10,19,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,3,16,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-18,72,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-19,47,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-12,71,15,2), new GeneratedEnemyUnit(15,-12,50,33,2), new GeneratedEnemyUnit(1,-10,22,22,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "77414b9e0f5db1667b76f042351ca86c1e027b72cc3062df345458e1716ef6ba");
        }

        private static void Case_02630()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2630,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,5,97,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-2,52,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-18,28,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,19,38,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-13,8,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,20,49,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,4,97,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,16,6,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,10,58,1,2), new GeneratedEnemyUnit(1,-19,70,42,2), new GeneratedEnemyUnit(-7,4,67,20,4), new GeneratedEnemyUnit(6,13,52,22,1), new GeneratedEnemyUnit(-7,1,54,45,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "21c138b2a06b4998fd95e6df02cf0f17099c16d0ae37a5f86c6cbad376c83f2c");
        }

        private static void Case_02631()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2631,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-9,86,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-15,42,1,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "cb199af10cab1f2055a1ec8efcb53138273b70c0686f1a876cdcdfbac11be2a6");
        }

        private static void Case_02632()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2632,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,13,56,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-1,62,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,10,31,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,20,11,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,3,16,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,7,68,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-9,5,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-4,50,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,3,86,15,4), new GeneratedEnemyUnit(-5,18,23,31,2), new GeneratedEnemyUnit(5,19,98,49,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e7de0b4b9423a85b5d112cf3a0edadd1cf00d0530da61abc0a859cd76316786e");
        }

        private static void Case_02633()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2633,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,15,82,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,16,61,2,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "020a307debee2cc1bffd663929527128f3958944997d364a458a80ede642aa87");
        }

        private static void Case_02634()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2634,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,15,89,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,16,30,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,19,92,29,3), new GeneratedEnemyUnit(-8,-16,34,5,4), new GeneratedEnemyUnit(-20,10,89,22,2), new GeneratedEnemyUnit(-1,-15,31,14,1), new GeneratedEnemyUnit(9,-1,68,48,2), new GeneratedEnemyUnit(-19,11,84,25,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "43dfae567cdb58f988b66b57c94bd4342d52986b18236d06ec450ad5d5e55b8f");
        }

        private static void Case_02635()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2635,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,10,25,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-18,22,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,7,87,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-5,14,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,12,76,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-17,99,45,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "399bcc9286661826bac9a29ede187d8532f8813c085513cd0bee7e026bb63315");
        }

        private static void Case_02636()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2636,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,1,62,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-14,68,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,14,16,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-10,21,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-9,51,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,4,91,50,2), new GeneratedEnemyUnit(15,-17,69,32,4), new GeneratedEnemyUnit(-2,13,52,24,2), new GeneratedEnemyUnit(10,-13,12,31,4), new GeneratedEnemyUnit(8,-8,29,19,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "c8b6b8b6ecf403d81793ac17b10d8b8713b029d620386925bf9277eb16aba754");
        }

        private static void Case_02637()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2637,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,15,33,6,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "3043abad31dae69658bb8a484438abe108e4fb841f02a5c8e9f703857227e103");
        }

        private static void Case_02638()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2638,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,16,37,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,7,35,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,3,5,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-16,92,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-8,70,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,0,46,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-11,86,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,17,28,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-11,89,36,1), new GeneratedEnemyUnit(14,-1,65,28,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "b840a7edcf06cdd5ea156d76ee871aeb8a7dd267405f43c95b61f2b076108588");
        }

        private static void Case_02639()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2639,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-8,43,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-9,80,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-12,69,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-15,56,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-16,71,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-4,90,35,1), new GeneratedEnemyUnit(7,19,78,6,1), new GeneratedEnemyUnit(-15,-17,55,8,1), new GeneratedEnemyUnit(12,-14,81,21,3), new GeneratedEnemyUnit(18,1,30,14,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "3c0ed9a17e0651b485b40b6f0baf618131ffcb883c23fee0ca267c7b666f28e3");
        }

        private static void Case_02640()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2640,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,15,16,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,18,68,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-6,58,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-15,73,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,9,94,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,19,82,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-14,42,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-15,46,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,4,67,8,2), new GeneratedEnemyUnit(-7,-10,77,34,3), new GeneratedEnemyUnit(18,0,90,8,2), new GeneratedEnemyUnit(-19,9,25,14,3), new GeneratedEnemyUnit(-6,-17,90,6,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "b297a536bbb4a22195304ed3319b38f26cc6a440b0a84b928a094bd907d9fe82");
        }

        private static void Case_02641()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2641,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-4,45,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-15,63,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,9,22,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-13,93,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-2,12,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-9,75,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,17,99,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-1,48,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,7,56,23,2), new GeneratedEnemyUnit(13,13,56,14,3), new GeneratedEnemyUnit(-2,4,79,21,1), new GeneratedEnemyUnit(-16,-11,42,12,3), new GeneratedEnemyUnit(-15,13,66,14,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "c9eacb91ca74ae4f67e11f2a2ed24c2ba7f9dda234e3f268306d75f69b9c3786");
        }

        private static void Case_02642()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2642,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-5,25,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,8,61,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-18,80,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,4,78,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-16,6,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-13,10,22,1), new GeneratedEnemyUnit(-8,2,34,44,1), new GeneratedEnemyUnit(-3,-9,36,4,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "c0fdf9744d1a1a78e22110bf85da43b1ffd3efc955e0e538312ca4abf67fa93c");
        }

        private static void Case_02643()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2643,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,9,90,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,11,7,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-1,10,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,19,53,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,10,58,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,13,24,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,16,90,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,13,75,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-12,99,26,3), new GeneratedEnemyUnit(16,6,66,17,3), new GeneratedEnemyUnit(6,15,41,19,4), new GeneratedEnemyUnit(10,18,55,11,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "33afc4fb48b799e1bcc9f6f741304d36bfdaa87fe8e195da150635adac42f3fd");
        }

        private static void Case_02644()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2644,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,8,64,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-7,94,21,3), new GeneratedEnemyUnit(9,17,27,14,4), new GeneratedEnemyUnit(-8,4,9,48,2), new GeneratedEnemyUnit(2,10,46,38,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "bcf4e91cbbc98eb12550a380c669ddd1024b1e3d9261b6aa68f93f4731e5ec4a");
        }

        private static void Case_02645()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2645,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-1,76,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-16,30,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-4,85,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,13,6,38,2), new GeneratedEnemyUnit(-11,-15,48,50,1), new GeneratedEnemyUnit(-17,-11,25,23,3), new GeneratedEnemyUnit(-3,-18,9,10,2), new GeneratedEnemyUnit(18,15,84,1,2), new GeneratedEnemyUnit(-18,17,17,10,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "a85bd59b4539235202188a96369caa47079acc1cbbd7494c8651cbd7d30aa6fb");
        }

        private static void Case_02646()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2646,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,12,41,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,20,12,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-19,62,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,11,41,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-19,28,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,15,77,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,6,41,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,16,21,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,13,13,17,1), new GeneratedEnemyUnit(18,4,43,13,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "fcc690083532169b020db7d1b375ef305c2c9f0ec1922291283065071be43c8e");
        }

        private static void Case_02647()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2647,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-6,85,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-11,81,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,13,20,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,8,7,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-2,61,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-9,9,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-4,27,7,3), new GeneratedEnemyUnit(-17,15,15,36,4), new GeneratedEnemyUnit(13,14,24,44,3), new GeneratedEnemyUnit(18,-9,46,15,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "9da4dd6d76951db8a122fd3c957ec903d05e13f5dc265cbb2f65f89e99f101e1");
        }

        private static void Case_02648()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2648,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-6,40,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,7,91,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-10,9,37,3), new GeneratedEnemyUnit(-20,2,73,8,2), new GeneratedEnemyUnit(12,18,81,41,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "f71da6621d75b812b82ec28b31c785b57da968c93c2d682f0369e9b677e1936d");
        }

        private static void Case_02649()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2649,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,6,84,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-8,96,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,18,91,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,18,60,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-16,80,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-7,40,5,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "36b9b83f32c19faaaa70790bda42fd662be087b25b1da34fa454b23adfd39fa2");
        }

        private static void Case_02650()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2650,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,5,97,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,10,46,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,0,47,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-18,70,50,2), new GeneratedEnemyUnit(11,5,11,36,1), new GeneratedEnemyUnit(13,-20,90,5,4), new GeneratedEnemyUnit(-19,-2,75,6,1), new GeneratedEnemyUnit(13,16,78,39,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "fccf150fb52092cc48369c512a6908fdce1ae5778982661fdbf0ab78ffc91e44");
        }

        private static void Case_02651()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2651,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,17,15,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-13,92,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,9,48,34,2), new GeneratedEnemyUnit(14,15,30,49,4), new GeneratedEnemyUnit(14,9,39,41,1), new GeneratedEnemyUnit(8,9,54,20,1), new GeneratedEnemyUnit(12,9,62,25,3), new GeneratedEnemyUnit(-20,18,91,36,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "df13b1f5156b5f3c87cb9eafc7f3063f719fda9a51cf43597b3717fa8b575cc1");
        }

        private static void Case_02652()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2652,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,8,70,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,18,44,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,5,17,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-3,51,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-9,21,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,1,54,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-2,27,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,15,24,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,6,62,30,4), new GeneratedEnemyUnit(6,-17,18,30,3), new GeneratedEnemyUnit(6,-14,27,48,1), new GeneratedEnemyUnit(-13,17,35,43,3), new GeneratedEnemyUnit(-16,2,18,47,4), new GeneratedEnemyUnit(6,-16,89,31,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "677af72d756646df6b63db7384ebbdeaa9d3a84fc5bb421e38973ff702feb5b7");
        }

        private static void Case_02653()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2653,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,14,6,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-14,41,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,14,71,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-18,64,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,14,43,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-1,36,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-14,9,45,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "3c247b217bf6b3b63d0be8ee773d55833f8d5390681c6bf7ff84854e8c88aec0");
        }

        private static void Case_02654()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2654,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-4,28,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,12,78,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,0,10,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,13,83,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-7,38,7,2), new GeneratedEnemyUnit(2,-19,50,41,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "3097ab6ee57eed94f5e4871d4972717d8897f1fdfb21e88cb1429a21d8ac7800");
        }

        private static void Case_02655()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2655,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-5,18,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,14,85,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,10,25,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-7,67,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,16,36,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,4,27,47,2), new GeneratedEnemyUnit(17,-6,58,12,4), new GeneratedEnemyUnit(-11,-10,75,50,2), new GeneratedEnemyUnit(5,-13,42,40,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "40fa85553f68442d1c46578b814f882d07169c175d73cb1907ae58b3d3a8bb44");
        }

        private static void Case_02656()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2656,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,18,7,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,11,78,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,18,14,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-17,100,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-11,25,16,1), new GeneratedEnemyUnit(-20,7,7,27,4), new GeneratedEnemyUnit(12,-13,72,38,3), new GeneratedEnemyUnit(-9,-14,91,32,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "6d796dbcc759cca75078d0e97b29da8a4760b1cab8ab7c53f555431de53cdfbf");
        }

        private static void Case_02657()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2657,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-16,16,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-17,34,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-13,50,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-10,90,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-13,36,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,2,36,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-10,5,28,2), new GeneratedEnemyUnit(-4,-17,41,16,2), new GeneratedEnemyUnit(-14,15,92,23,3), new GeneratedEnemyUnit(0,5,79,25,2), new GeneratedEnemyUnit(-12,-19,56,32,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "6cacfeed6f2f5538ab977687b2ecf7842754291e4af210fa1e71a3c77ec058ae");
        }

        private static void Case_02658()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2658,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,19,100,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-12,67,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-6,38,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,1,90,39,1), new GeneratedEnemyUnit(-17,9,66,34,1), new GeneratedEnemyUnit(-15,2,91,43,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "350955daf397d5f322d561e34e475949855036c994e7cd33e8f8f7e96fe15723");
        }

        private static void Case_02659()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2659,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-17,62,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-8,62,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,17,91,7,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "0e4ce6a1f10fce2592b9e195c032cc1c6f351840ff2516f5ed79d765244dc214");
        }

        private static void Case_02660()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2660,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-13,80,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-18,37,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-8,14,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,14,85,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-9,47,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-4,59,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-13,26,1,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "cfe218646db9227f6d9d9312ffbbb0314e0304f8ec04d0ae0ba5cdb73d2d75f6");
        }

        private static void Case_02661()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2661,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,1,19,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,20,42,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,10,54,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-19,28,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,0,87,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-3,61,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-19,57,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-7,59,39,3), new GeneratedEnemyUnit(-3,-12,16,49,4), new GeneratedEnemyUnit(-18,-11,55,7,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "32356f72af71245cd84815ebafc44c5d8a5d00807c4ed82b8e43249c36444b42");
        }

        private static void Case_02662()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2662,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-16,89,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,6,63,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,2,54,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-12,41,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,2,25,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-10,21,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-1,71,11,4), new GeneratedEnemyUnit(-9,-10,46,48,1), new GeneratedEnemyUnit(-14,7,23,36,1), new GeneratedEnemyUnit(-5,15,68,9,1), new GeneratedEnemyUnit(8,16,86,39,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "92b0517014c069d615cc286cbc35dfb56f4dc989454e1facdac2211b759f4a95");
        }

        private static void Case_02663()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2663,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-13,39,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,20,15,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,1,44,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-9,37,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,1,63,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,20,61,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-11,68,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,18,88,30,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "60821eb1ea23faf36021b9511874723b5f87338983962ad92c1e977d6c7a60d0");
        }

        private static void Case_02664()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2664,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-7,41,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-1,83,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,12,91,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,20,92,22,2), new GeneratedEnemyUnit(-19,4,71,4,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "bd6cef3abdd5615b001f32b33ae8e07ee84818f1b70590ef54a984330077dba3");
        }

        private static void Case_02665()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2665,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-17,39,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,6,98,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-8,82,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,12,96,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-5,9,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-14,42,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-6,87,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,6,77,18,3), new GeneratedEnemyUnit(-2,14,60,26,1), new GeneratedEnemyUnit(-9,-17,91,39,3), new GeneratedEnemyUnit(-12,-2,38,27,4), new GeneratedEnemyUnit(-14,-15,70,36,2), new GeneratedEnemyUnit(-6,-3,10,24,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "60d03d901ed5f2ef190a2b08005a272cd922d6c79efc7569d9784f576858d376");
        }

        private static void Case_02666()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2666,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-5,22,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,20,10,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-14,69,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-10,76,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,3,79,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-9,57,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-20,88,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-9,88,50,2), new GeneratedEnemyUnit(-7,13,52,48,1), new GeneratedEnemyUnit(6,3,57,7,1), new GeneratedEnemyUnit(-7,20,40,25,4), new GeneratedEnemyUnit(12,-18,99,35,1), new GeneratedEnemyUnit(3,17,27,32,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "4023ef10d82c4ffb5162ceab9a75930d4ac2cba9e3b686f6368b44db2e1a7470");
        }

        private static void Case_02667()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2667,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,12,64,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-16,98,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-18,20,37,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "7c024d06713a7b770f71b50b6673c5a10a18e857f23697857dd981ce0acbc3f2");
        }

        private static void Case_02668()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2668,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,12,66,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,12,17,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-15,83,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,14,91,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-7,69,4,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "d09947821dc985f19e458abbc240e48ca5b154551404d6595673f7e340706bc9");
        }

        private static void Case_02669()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2669,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,16,84,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,4,90,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,0,71,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,14,81,38,2), new GeneratedEnemyUnit(5,-16,31,46,2), new GeneratedEnemyUnit(7,16,34,2,3), new GeneratedEnemyUnit(18,2,81,7,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "a5250fb393af89a5b4eafde2b28311a460e15309cae71f9062e0d1dd4819c194");
        }

        private static void Case_02670()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2670,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-11,66,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,10,23,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,6,69,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-11,33,7,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "7187b3b9a0d46f1dc49dd3785c67fbdd41e3abb9af9f9a71aedc4c77d3b60e63");
        }

        private static void Case_02671()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2671,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-3,23,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,14,26,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-12,46,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-16,18,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-19,25,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,19,38,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,2,16,1,1), new GeneratedEnemyUnit(-7,6,32,43,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "42c20ac3d4a4c5ecdd8c76d8ab52264499658bdf1fd818dfb9f23f65e4dab3d3");
        }

        private static void Case_02672()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2672,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,16,85,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,14,98,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,17,75,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-18,14,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-18,84,49,2), new GeneratedEnemyUnit(-9,15,18,15,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "1d0fe487ff972b21150ca97c9a9d2b2b84ed52c891106cecc3f654661d84b512");
        }

        private static void Case_02673()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2673,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-2,10,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-19,35,6,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "e514f7fc81b3bb341be953606d0c748ddd9776c518e5199b2178be9f6eac1e16");
        }

        private static void Case_02674()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2674,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-15,95,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,11,17,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-11,11,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,15,15,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,6,13,16,3), new GeneratedEnemyUnit(16,4,41,45,1), new GeneratedEnemyUnit(19,1,74,2,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "447360e36b548b4d206007251ae42c77ac01dca25f77c7f0edba0a426d856bc7");
        }

        private static void Case_02675()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2675,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-1,6,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-19,93,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,0,18,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-1,18,15,3), new GeneratedEnemyUnit(-8,2,83,15,1), new GeneratedEnemyUnit(-11,-16,49,35,1), new GeneratedEnemyUnit(5,13,50,4,1), new GeneratedEnemyUnit(8,-15,69,15,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "429eb283cc5b44ccda93cf8af256762db39c9852d73789c0bdddd2ea05bab364");
        }

        private static void Case_02676()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2676,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-8,69,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-13,8,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,7,9,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-20,86,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,18,61,25,2), new GeneratedEnemyUnit(-2,17,79,41,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "c8d410dd9611a10c2a5a047cc307719e4399228ec96aa9dfa655f42ea2e9e7df");
        }

        private static void Case_02677()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2677,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,20,79,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,5,81,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-14,90,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-15,88,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-4,48,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-15,17,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-19,67,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-19,21,12,3), new GeneratedEnemyUnit(-13,1,79,24,4), new GeneratedEnemyUnit(18,-18,64,50,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 85,
                stableHash: "78186ec825a439a2f2976d329c48abfd68bc41fc0011b9e2d46dd66179099a4f");
        }

        private static void Case_02678()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2678,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,16,59,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-12,67,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,17,60,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-1,42,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-11,18,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-11,75,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,7,77,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,9,33,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,5,94,34,4), new GeneratedEnemyUnit(-18,9,90,45,1), new GeneratedEnemyUnit(18,5,93,5,3), new GeneratedEnemyUnit(-20,14,13,39,3), new GeneratedEnemyUnit(-8,-1,98,21,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "6f86bf13a5a1b7584d93263dd0b9de7b6e4e9efb1041dc0eaa3e3c17051fb8da");
        }

        private static void Case_02679()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2679,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-2,22,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-15,33,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,16,80,31,2), new GeneratedEnemyUnit(11,7,91,22,4), new GeneratedEnemyUnit(-4,-19,42,21,2), new GeneratedEnemyUnit(-17,-20,41,32,4), new GeneratedEnemyUnit(-6,-3,96,16,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "a1fcde297bc83720b15b7dcfcf96877e5329185e250ad9749d2c731339f3e788");
        }

        private static void Case_02680()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2680,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-1,71,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,11,80,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-8,90,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-12,66,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,11,39,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-2,94,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,17,96,2,1), new GeneratedEnemyUnit(-1,4,5,47,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "47654fb7dfa7c3e55cbdf556d263a7c3b7b48b52da088ddc3ddc91abed15daff");
        }

        private static void Case_02681()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2681,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-9,70,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-1,16,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-20,46,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,16,99,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,14,74,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-14,67,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,5,42,42,3), new GeneratedEnemyUnit(5,-20,73,45,1), new GeneratedEnemyUnit(-18,3,58,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "546f5044a301497027ba3f518f7f6688e46cfa1ee117c71ad447da3731bc79da");
        }

        private static void Case_02682()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2682,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-13,38,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,1,38,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,13,28,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,10,23,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,12,24,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-4,8,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,20,37,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-20,73,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,12,35,31,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "698541f7e0f9d864907277fb1427da71f53c1ae8006453a159b76e50fb086faf");
        }

        private static void Case_02683()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2683,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,14,91,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,14,31,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-4,43,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,14,91,2,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "aad07440258f861fa2b81f0493665644ffd0867028ec77887514e5717a5571df");
        }

        private static void Case_02684()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2684,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-11,90,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,9,77,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-4,66,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-8,21,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,3,80,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,12,58,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,6,26,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,12,64,19,4), new GeneratedEnemyUnit(18,-16,44,27,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "d2b750e2cb9a448c8575a3cc7c9c51ff76d8a443fb181e3b4c9f585157d3120d");
        }

        private static void Case_02685()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2685,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,12,6,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,12,35,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-18,51,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-3,9,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,0,97,41,2), new GeneratedEnemyUnit(20,19,67,35,4), new GeneratedEnemyUnit(-7,-12,13,33,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 85,
                stableHash: "2ec5672cf8be10a69af4422ff28b8af28d44cc7f884e74262d2ff973348e5567");
        }

        private static void Case_02686()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2686,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,16,59,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-18,50,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-2,14,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-8,44,3,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0325ee8599009f353158c26fe653a3ee6fd69dce7776f682f2cbc426b43e5109");
        }

        private static void Case_02687()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2687,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,11,29,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,11,33,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,15,76,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-6,90,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-17,64,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,20,35,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,20,12,39,3), new GeneratedEnemyUnit(-8,5,30,18,2), new GeneratedEnemyUnit(-11,6,28,33,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "e4e147580611dd24dde338705bc7d8116541f6e140131515dc95c57742a9539b");
        }

        private static void Case_02688()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2688,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-13,27,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,0,27,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-5,68,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,0,70,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-10,99,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,20,95,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,10,28,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-9,56,27,3), new GeneratedEnemyUnit(-2,-9,21,30,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "f5c7b2860c3ca12a14e34107a710528059cc64b64c15becbac55c2d91466e88b");
        }

        private static void Case_02689()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2689,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,2,89,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-5,36,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-8,23,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,19,65,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-8,20,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,10,21,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,14,30,11,2), new GeneratedEnemyUnit(-8,-13,57,34,3), new GeneratedEnemyUnit(2,-1,98,40,3), new GeneratedEnemyUnit(-8,1,35,23,1), new GeneratedEnemyUnit(4,7,52,4,1), new GeneratedEnemyUnit(-9,-4,20,26,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "1b95978d57973e4100529fc40230e81372ac11da04761a3dee0f963d7d3c22c0");
        }

        private static void Case_02690()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2690,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,17,55,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,9,92,30,3), new GeneratedEnemyUnit(-2,9,43,50,3), new GeneratedEnemyUnit(-19,-18,48,9,1), new GeneratedEnemyUnit(-7,-20,67,50,3), new GeneratedEnemyUnit(7,-19,44,50,2), new GeneratedEnemyUnit(-10,14,7,16,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "2367d41d59728f81c7c6700a7f9162e3f6d4529623d2918ee5ec566bf75886cd");
        }

        private static void Case_02691()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2691,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,8,61,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-8,24,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-16,23,40,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "19169bc7e63680c4ea9919182cc1567d3f9dc01d33d4f9e0ac5639c688fba862");
        }

        private static void Case_02692()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2692,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,17,5,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,14,30,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-6,100,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-14,85,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-10,41,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,0,9,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-3,10,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-5,38,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-11,49,8,3), new GeneratedEnemyUnit(-2,15,28,42,1), new GeneratedEnemyUnit(9,1,72,7,2), new GeneratedEnemyUnit(-1,-16,79,25,1), new GeneratedEnemyUnit(-5,18,41,4,4), new GeneratedEnemyUnit(9,1,38,49,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "93600f0c1ba64ca5a57b94173b573c6827992262c21f60becf491ad40d40b394");
        }

        private static void Case_02693()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2693,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,17,53,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,14,89,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-11,76,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-1,38,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-8,77,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,17,59,10,2), new GeneratedEnemyUnit(15,20,25,11,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "f7f434979134dd23f27ef9f59b580eee4be39732344241b572e05803888dba5b");
        }

        private static void Case_02694()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2694,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-19,85,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,14,9,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,20,64,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,20,53,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,19,23,6,3), new GeneratedEnemyUnit(3,-5,31,24,1), new GeneratedEnemyUnit(-16,16,98,26,1), new GeneratedEnemyUnit(-10,18,46,7,2), new GeneratedEnemyUnit(9,-10,33,6,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "e7df3ac1909b3125c37178d2da4dd0ce043c2a9a10f53dcf65552349dd36be78");
        }

        private static void Case_02695()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2695,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-5,97,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-16,79,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,13,76,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-4,11,22,2), new GeneratedEnemyUnit(18,9,66,31,2), new GeneratedEnemyUnit(6,19,24,20,2), new GeneratedEnemyUnit(6,5,70,43,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "a86a7c4d4eeb23acebdfee6d564bdef3ff4abcbe7929f4d6c83548f439946c74");
        }

        private static void Case_02696()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2696,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,16,85,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,14,13,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,1,33,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,13,22,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,7,30,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-2,43,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-14,19,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-8,35,41,3), new GeneratedEnemyUnit(-11,10,18,7,3), new GeneratedEnemyUnit(14,11,81,49,1), new GeneratedEnemyUnit(14,-7,16,13,2), new GeneratedEnemyUnit(16,20,45,41,2), new GeneratedEnemyUnit(-3,-2,36,12,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "563ae1e71bd54a000932ae180a12a32e737d275cae1fe52d037b98eb3416c5e2");
        }

        private static void Case_02697()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2697,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,1,29,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,13,24,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-15,31,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-17,81,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-8,26,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,11,29,42,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "57ffc883126d4f36bfb9920cb4645a3113660b8d8f5d4b34b970e63d463b577d");
        }

        private static void Case_02698()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2698,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-6,60,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,8,75,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-11,19,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-2,56,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-1,77,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-7,31,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-7,89,1,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "6ee5efff83e9c7af5f2e82c361faae44b2561e03ca418fd05929684c103ec70d");
        }

        private static void Case_02699()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2699,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-3,63,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-19,59,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,19,17,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-4,37,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-19,24,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,12,75,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-3,76,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-12,69,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,20,92,48,1), new GeneratedEnemyUnit(-13,14,57,35,4), new GeneratedEnemyUnit(8,-20,66,50,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "e9b03797ecc1f547a310ce7227f2090923abeac049207664e151bd21d64c5bc6");
        }

    }
}
