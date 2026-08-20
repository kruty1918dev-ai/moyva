using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard011
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_01980();
            Case_01981();
            Case_01982();
            Case_01983();
            Case_01984();
            Case_01985();
            Case_01986();
            Case_01987();
            Case_01988();
            Case_01989();
            Case_01990();
            Case_01991();
            Case_01992();
            Case_01993();
            Case_01994();
            Case_01995();
            Case_01996();
            Case_01997();
            Case_01998();
            Case_01999();
            Case_02000();
            Case_02001();
            Case_02002();
            Case_02003();
            Case_02004();
            Case_02005();
            Case_02006();
            Case_02007();
            Case_02008();
            Case_02009();
            Case_02010();
            Case_02011();
            Case_02012();
            Case_02013();
            Case_02014();
            Case_02015();
            Case_02016();
            Case_02017();
            Case_02018();
            Case_02019();
            Case_02020();
            Case_02021();
            Case_02022();
            Case_02023();
            Case_02024();
            Case_02025();
            Case_02026();
            Case_02027();
            Case_02028();
            Case_02029();
            Case_02030();
            Case_02031();
            Case_02032();
            Case_02033();
            Case_02034();
            Case_02035();
            Case_02036();
            Case_02037();
            Case_02038();
            Case_02039();
            Case_02040();
            Case_02041();
            Case_02042();
            Case_02043();
            Case_02044();
            Case_02045();
            Case_02046();
            Case_02047();
            Case_02048();
            Case_02049();
            Case_02050();
            Case_02051();
            Case_02052();
            Case_02053();
            Case_02054();
            Case_02055();
            Case_02056();
            Case_02057();
            Case_02058();
            Case_02059();
            Case_02060();
            Case_02061();
            Case_02062();
            Case_02063();
            Case_02064();
            Case_02065();
            Case_02066();
            Case_02067();
            Case_02068();
            Case_02069();
            Case_02070();
            Case_02071();
            Case_02072();
            Case_02073();
            Case_02074();
            Case_02075();
            Case_02076();
            Case_02077();
            Case_02078();
            Case_02079();
            Case_02080();
            Case_02081();
            Case_02082();
            Case_02083();
            Case_02084();
            Case_02085();
            Case_02086();
            Case_02087();
            Case_02088();
            Case_02089();
            Case_02090();
            Case_02091();
            Case_02092();
            Case_02093();
            Case_02094();
            Case_02095();
            Case_02096();
            Case_02097();
            Case_02098();
            Case_02099();
            Case_02100();
            Case_02101();
            Case_02102();
            Case_02103();
            Case_02104();
            Case_02105();
            Case_02106();
            Case_02107();
            Case_02108();
            Case_02109();
            Case_02110();
            Case_02111();
            Case_02112();
            Case_02113();
            Case_02114();
            Case_02115();
            Case_02116();
            Case_02117();
            Case_02118();
            Case_02119();
            Case_02120();
            Case_02121();
            Case_02122();
            Case_02123();
            Case_02124();
            Case_02125();
            Case_02126();
            Case_02127();
            Case_02128();
            Case_02129();
            Case_02130();
            Case_02131();
            Case_02132();
            Case_02133();
            Case_02134();
            Case_02135();
            Case_02136();
            Case_02137();
            Case_02138();
            Case_02139();
            Case_02140();
            Case_02141();
            Case_02142();
            Case_02143();
            Case_02144();
            Case_02145();
            Case_02146();
            Case_02147();
            Case_02148();
            Case_02149();
            Case_02150();
            Case_02151();
            Case_02152();
            Case_02153();
            Case_02154();
            Case_02155();
            Case_02156();
            Case_02157();
            Case_02158();
            Case_02159();
        }

        private static void Case_01980()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1980,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-5,89,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,9,58,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-1,96,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-18,89,2,4), new GeneratedEnemyUnit(-8,-2,36,21,2), new GeneratedEnemyUnit(-10,19,78,10,2), new GeneratedEnemyUnit(17,4,86,7,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "6cecef15919a9f82e7a4902645b39640ac012a140f485e42c5f74110c40c7931");
        }

        private static void Case_01981()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1981,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-16,53,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,1,79,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-20,22,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,0,80,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-11,40,13,1), new GeneratedEnemyUnit(-18,7,12,2,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "b7cb778a82de825a30e2c960a6a3c1d9d7778d30ab17527b3a2d01db9c15af9d");
        }

        private static void Case_01982()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1982,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-19,58,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-7,46,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-12,95,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-16,12,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-16,58,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-4,40,19,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "2e674f33d2aee0da6a775a0c189330e6cd5ecb961de99788aec5ff25ac3bbb52");
        }

        private static void Case_01983()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1983,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,10,71,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,20,85,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-17,68,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-6,99,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,11,45,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-8,88,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,5,89,2,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "7a9b5a7fe09569eaf4fb442b08ba2755e59f7160088e11587d45169b1831943d");
        }

        private static void Case_01984()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1984,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,0,89,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-12,69,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-11,64,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-1,90,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,20,17,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-10,27,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-19,68,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-2,82,20,3), new GeneratedEnemyUnit(19,5,43,38,4), new GeneratedEnemyUnit(9,-2,7,37,1), new GeneratedEnemyUnit(9,6,42,18,3), new GeneratedEnemyUnit(7,-17,29,13,3), new GeneratedEnemyUnit(-19,11,30,17,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "ac6078c4f3e1bafbeac7ef0d7b20e5fc89becd692a90d479154f8407371fcf8f");
        }

        private static void Case_01985()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1985,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-7,100,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-7,86,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-6,58,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-4,51,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,9,41,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,15,6,6,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "79de38b8a07af16c9417419f74fd08b9917dbbd8ce8637ebb76bf7bff8b0d016");
        }

        private static void Case_01986()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1986,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-10,90,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-6,35,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,18,19,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-11,27,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,20,66,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-8,27,48,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "9165f2f4746ca39801ff81ecd02eb048013b1773017079df6e0759de1b9acf19");
        }

        private static void Case_01987()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1987,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,17,94,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-11,25,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,12,92,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,11,100,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,18,40,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,3,68,26,3), new GeneratedEnemyUnit(-17,4,52,2,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a2f946c4e22c8884be01c9ecd07b0c5282dee5db2cabebdac7a42d60e121f039");
        }

        private static void Case_01988()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1988,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-17,17,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,1,27,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,12,30,4,1), new GeneratedEnemyUnit(-15,12,57,20,1), new GeneratedEnemyUnit(18,-13,33,30,3), new GeneratedEnemyUnit(-3,7,77,49,2), new GeneratedEnemyUnit(-9,-18,99,36,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "273c13ef61ef1e2e006c289477a35ba0f7f5339219a6f8192ec78f7f6f989755");
        }

        private static void Case_01989()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1989,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,3,27,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-12,24,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-18,92,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-1,43,41,3), new GeneratedEnemyUnit(-10,6,98,32,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "29a727bce5827a9efb8942accff34a27c9430f7f51888564a29e47a0f56e07c8");
        }

        private static void Case_01990()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1990,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-8,43,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-1,70,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-4,37,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-4,50,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-4,73,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,10,72,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,5,8,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-3,66,11,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "b64e4de0e3405a1d47ff9ac2c8cd23fbc446e198dc7312e702c256bfd77d27f3");
        }

        private static void Case_01991()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1991,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,19,58,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,5,34,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-4,19,7,3), new GeneratedEnemyUnit(-17,10,78,42,2), new GeneratedEnemyUnit(-10,7,8,41,3), new GeneratedEnemyUnit(-11,-11,60,7,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "a2b4f244680afa3515af4dd6ac5dc8ca6d292a5e921aff21a58c1fa2e8334ac3");
        }

        private static void Case_01992()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1992,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-17,82,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,10,45,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-3,9,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,3,11,40,2), new GeneratedEnemyUnit(-4,5,43,4,4), new GeneratedEnemyUnit(0,5,55,48,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "036b96b1a5ac4cd918973b75285630f49e12e7d115f5d23ed9120f81db481363");
        }

        private static void Case_01993()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1993,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-14,95,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-17,72,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,12,99,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,11,99,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,11,83,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,19,97,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,15,81,3,4), new GeneratedEnemyUnit(-12,12,26,23,3), new GeneratedEnemyUnit(16,1,20,9,1), new GeneratedEnemyUnit(11,10,44,23,2), new GeneratedEnemyUnit(-9,-7,59,3,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "dbd3646df15ac49f531bd952ca091325aae8c17dc397cb0d0c8a6ac85ce943e0");
        }

        private static void Case_01994()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1994,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-6,99,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-18,78,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,8,38,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,6,8,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-14,27,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,13,82,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,6,19,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-12,13,1,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "4d34a483f5689d83de5dbf422033eb35bb25b1ef1f24fd36df0889164cc7164f");
        }

        private static void Case_01995()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1995,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-10,84,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-8,46,30,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "42f2acc03df36824c4db9a37edd3ee2224637188cb05da00e96408870d9cee1f");
        }

        private static void Case_01996()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1996,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,17,91,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-11,26,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-17,99,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,4,22,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,18,21,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-1,53,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,2,82,3,2), new GeneratedEnemyUnit(-1,4,40,1,4), new GeneratedEnemyUnit(-18,19,40,44,4), new GeneratedEnemyUnit(6,0,41,26,4), new GeneratedEnemyUnit(12,-4,34,14,4), new GeneratedEnemyUnit(16,9,36,45,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "88fbbea5cb1a5aa969008486932ab3a5e19fbca6cb8ca8990c786f97f13e425c");
        }

        private static void Case_01997()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1997,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,10,59,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-4,76,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,4,93,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,14,58,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,6,62,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-5,23,43,2), new GeneratedEnemyUnit(-10,4,97,43,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "93fef4bc3165b27d68dd584e90d9ca6ea5fc6a9dc5d3435630836be0727e36b0");
        }

        private static void Case_01998()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1998,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-7,29,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-4,48,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,17,54,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-1,74,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,12,61,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-13,79,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,11,100,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,15,62,25,1), new GeneratedEnemyUnit(-4,1,36,28,4), new GeneratedEnemyUnit(9,13,6,24,4), new GeneratedEnemyUnit(18,-1,81,7,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "99ca52d69dfa696a66ab3924ce411804c598d64964c1c806bfcd890f5ea748d4");
        }

        private static void Case_01999()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1999,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-14,17,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-19,40,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-7,60,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,2,87,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-9,26,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,13,77,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,15,62,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-2,76,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-16,38,27,4), new GeneratedEnemyUnit(-2,-4,59,7,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9eabad13d7af4249e3dca45cff6f596ca95ad98cb84aaf7cee0132b57792baf1");
        }

        private static void Case_02000()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2000,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-9,36,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,6,43,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,18,96,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-2,68,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-8,31,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,9,93,37,1), new GeneratedEnemyUnit(-15,-14,47,17,1), new GeneratedEnemyUnit(-8,13,58,16,4), new GeneratedEnemyUnit(15,6,64,18,3), new GeneratedEnemyUnit(14,-11,98,20,4), new GeneratedEnemyUnit(-4,-13,46,49,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 35,
                stableHash: "0c0a9d9ce569645692194ed54abe6ac89bdf38a1c7e93e1f6324d5afdb260e77");
        }

        private static void Case_02001()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2001,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-6,46,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,5,13,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,5,95,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-1,82,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,7,86,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-16,72,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-1,84,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,13,50,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,9,43,7,4), new GeneratedEnemyUnit(7,-19,88,7,2), new GeneratedEnemyUnit(-5,-14,58,5,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "cf3b6155ad366a959890c07c264f8c64bbd8f528a097a1bde0d44d78c08903f0");
        }

        private static void Case_02002()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2002,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,7,38,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-12,42,32,4), new GeneratedEnemyUnit(-10,13,52,23,4), new GeneratedEnemyUnit(20,-11,5,35,1), new GeneratedEnemyUnit(0,7,73,9,4), new GeneratedEnemyUnit(15,11,94,34,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "e6b374a920a892f4d81834fef9cc982beb05b053df53514aef2bdfc2bebbf0b1");
        }

        private static void Case_02003()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2003,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-17,42,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,19,25,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-4,42,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-8,71,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-11,57,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-15,65,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,15,75,14,4), new GeneratedEnemyUnit(-5,5,91,36,2), new GeneratedEnemyUnit(-17,-20,94,10,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "6a68b094b8ac423b9b1f9c3c19b32b8909cc5851a9d507b9236098040d72d290");
        }

        private static void Case_02004()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2004,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,5,21,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-12,44,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,15,25,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-13,100,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,5,19,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,20,28,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,13,33,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-4,45,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,2,97,1,3), new GeneratedEnemyUnit(14,-10,36,4,4), new GeneratedEnemyUnit(-12,-13,55,35,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "61ccb2ecdcb9d549bc107f335554d980b98cdbcef51bba23bfa7078a3ebdba36");
        }

        private static void Case_02005()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2005,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-10,99,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-17,18,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-17,97,24,1), new GeneratedEnemyUnit(5,-17,26,13,1), new GeneratedEnemyUnit(12,1,43,26,4), new GeneratedEnemyUnit(9,-4,80,17,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "3624de47675b2601f3c849acd36107ad372d1791c7f4b9b10c90f89671044d86");
        }

        private static void Case_02006()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2006,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-10,75,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-13,20,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-5,11,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,15,44,42,1), new GeneratedEnemyUnit(7,2,74,22,2), new GeneratedEnemyUnit(-16,13,82,33,3), new GeneratedEnemyUnit(3,17,21,18,1), new GeneratedEnemyUnit(-18,-11,21,19,1), new GeneratedEnemyUnit(-18,17,44,24,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "be5661ad68bcbdede3602b252f9840826630d0fb256c0d3ee91dbe9f5858637e");
        }

        private static void Case_02007()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2007,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,13,95,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-2,57,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "baf10fe4c6ae1eea9a97000ba5fd595afab73ec48545218ec23dc73f5f408488");
        }

        private static void Case_02008()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2008,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,13,37,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-16,17,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-1,97,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-5,74,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-7,34,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-6,59,23,3), new GeneratedEnemyUnit(2,20,42,43,3), new GeneratedEnemyUnit(-9,-15,49,32,3), new GeneratedEnemyUnit(10,-1,15,23,2), new GeneratedEnemyUnit(-14,-15,70,48,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "0512be84f2b3e42d1d329445dab9f1a540cd5ec71a5319c4fe79f00cf710f565");
        }

        private static void Case_02009()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2009,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,19,78,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,16,42,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-19,66,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "97cf48251dbf185ddd7fc46bc6cf71a87f9f31018b5ef61ecd2702348b6626b7");
        }

        private static void Case_02010()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2010,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,8,67,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-8,67,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,12,69,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-7,53,1,1), new GeneratedEnemyUnit(14,-6,32,2,3), new GeneratedEnemyUnit(2,2,39,33,3), new GeneratedEnemyUnit(-4,-2,72,34,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "956e01c085ad596b3d7d41f86b9ac1ca72c54cf3f14a2c6d71a92da17500e94a");
        }

        private static void Case_02011()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2011,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-18,86,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-9,56,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,11,99,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,13,23,50,3), new GeneratedEnemyUnit(-3,-17,53,18,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "2c23fec9f17dc6ff877cad13f6d2c9105ecdbb72210535955a9337ee50ebbec9");
        }

        private static void Case_02012()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2012,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,3,43,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-16,88,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,7,89,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,16,8,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-13,46,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,13,23,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,12,42,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-16,34,33,3), new GeneratedEnemyUnit(-5,-19,16,9,3), new GeneratedEnemyUnit(-17,-19,98,2,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "5848307dcb78af36af0d5382481a3f013e8bcdd69a8cc66f2ee47780b4ee1f75");
        }

        private static void Case_02013()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2013,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-2,93,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,9,44,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,11,46,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,1,44,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,20,9,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,14,90,20,1), new GeneratedEnemyUnit(-10,17,68,23,2), new GeneratedEnemyUnit(15,18,69,14,1), new GeneratedEnemyUnit(15,-17,39,10,4), new GeneratedEnemyUnit(11,7,63,8,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "909c9bcde3ae59e609506bfa9abb27d79c81e0f1483643876d012db4e3f8ed08");
        }

        private static void Case_02014()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2014,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-4,87,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-19,63,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-19,70,23,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "cb8e57efa827207026c6ce263b555326eb32b4ae85aa09511228b529116a11cc");
        }

        private static void Case_02015()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2015,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-10,83,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,7,84,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,0,34,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,3,50,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,11,58,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,9,74,3,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "cb67956d5973db21ce42af224b3317f7287a5718a5aa3a16ec35aa737ed05832");
        }

        private static void Case_02016()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2016,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-7,64,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,13,99,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-13,48,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-2,48,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-1,60,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-17,93,17,2), new GeneratedEnemyUnit(9,0,32,37,4), new GeneratedEnemyUnit(4,7,78,2,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "d8774f1c5e85bc43a7c399dbb606f05b8aa4d922aad172e7c0299e7bf62cc568");
        }

        private static void Case_02017()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2017,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-11,61,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-13,81,47,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "bb813b3ae28234634838ce0e6c70b7e2e7b089d3f14fc2d36cdd8b174e412e4d");
        }

        private static void Case_02018()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2018,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,4,27,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,14,72,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,18,37,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-6,53,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-15,75,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,6,92,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-18,66,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-11,88,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-18,54,5,2), new GeneratedEnemyUnit(2,7,32,11,3), new GeneratedEnemyUnit(-13,-7,88,20,3), new GeneratedEnemyUnit(-12,6,40,26,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "bbf4ac359f16c8bf181be398f8d31e3d44c9df83835f22b41e01637b6311ca68");
        }

        private static void Case_02019()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2019,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-12,99,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,5,44,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,10,14,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-11,36,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,15,81,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-4,39,48,1), new GeneratedEnemyUnit(-2,-17,81,8,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f5b720e7f3aaa4eb43d2b95488972b9b11797d2d9263957475b353d093a9e7c4");
        }

        private static void Case_02020()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2020,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,8,20,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-7,5,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-9,66,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,11,77,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,0,59,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,10,40,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,15,78,24,4), new GeneratedEnemyUnit(-13,-18,31,46,1), new GeneratedEnemyUnit(-9,17,40,5,3), new GeneratedEnemyUnit(10,-6,91,10,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "90b3ab4fc065b153eff1c255b4ba3e79514a0eb0d0bb9adfc029894311572b3b");
        }

        private static void Case_02021()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2021,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,1,33,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,16,61,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,2,25,4,2), new GeneratedEnemyUnit(12,14,46,36,3), new GeneratedEnemyUnit(1,1,85,33,4), new GeneratedEnemyUnit(-4,15,95,42,1), new GeneratedEnemyUnit(-3,17,25,5,4), new GeneratedEnemyUnit(-2,17,18,40,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "d1ffa7002ce48d85b8a72269f674a08cea35212bb4b6d4eea46fe6d8e3ea0be4");
        }

        private static void Case_02022()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2022,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-1,75,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,17,89,23,2), new GeneratedEnemyUnit(0,-12,30,40,4), new GeneratedEnemyUnit(2,-5,98,22,3), new GeneratedEnemyUnit(-13,-11,27,34,2), new GeneratedEnemyUnit(-12,-2,21,43,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "8c64532ac3469d570f6b087460417cf1fa2695a62eaa62f0de71b0afd42b0cb7");
        }

        private static void Case_02023()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2023,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,7,38,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-3,68,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,1,38,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-6,38,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-14,71,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,16,90,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,8,33,8,3), new GeneratedEnemyUnit(9,-3,20,12,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "93323690fdcd848dc9eefe9ec9b293996fc2fe7c111347aa00692af778598882");
        }

        private static void Case_02024()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2024,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,14,52,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,11,76,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,15,26,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,0,92,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,4,37,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,13,10,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-7,36,38,4), new GeneratedEnemyUnit(-1,-12,38,17,2), new GeneratedEnemyUnit(16,-11,56,24,1), new GeneratedEnemyUnit(3,16,96,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "d5f5a40f95345c89921a9a8b7a0abefc4325fd178fcb90f01c01989d84206881");
        }

        private static void Case_02025()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2025,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,12,12,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,19,14,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,6,91,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-17,57,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-13,88,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-15,18,2,4), new GeneratedEnemyUnit(-10,6,47,36,4), new GeneratedEnemyUnit(20,-20,63,24,1), new GeneratedEnemyUnit(0,-4,10,13,3), new GeneratedEnemyUnit(-11,-11,32,40,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "2dd5960bef56b213a949691c0824fc6e3d3d0a0b5a55f7c3a02ee5d122190e10");
        }

        private static void Case_02026()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2026,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,11,57,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,11,99,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-18,30,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,13,10,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,12,43,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-6,32,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-2,66,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-5,22,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-2,19,43,1), new GeneratedEnemyUnit(-3,-5,41,31,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "6913b60208724ba7f89c8d7a11f162a945eb3705de02efd2776ac54287d07182");
        }

        private static void Case_02027()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2027,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,20,66,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,19,16,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-15,65,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-20,17,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-5,67,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-11,65,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,2,33,32,4), new GeneratedEnemyUnit(1,6,35,40,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "ad5d7d2fd3a835a6583df56e7e5b71ad9fdca375c8650bbeca35a85fff70a0e6");
        }

        private static void Case_02028()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2028,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,3,14,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,8,73,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,20,84,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,7,74,22,3), new GeneratedEnemyUnit(-5,16,57,31,4), new GeneratedEnemyUnit(3,5,91,24,4), new GeneratedEnemyUnit(11,0,12,39,4), new GeneratedEnemyUnit(0,-2,36,34,4), new GeneratedEnemyUnit(-3,5,36,23,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "0fc047d38d2b2a8c61841ad288f0b0aa1aeb9e4c93932fc10500028ffaa920e6");
        }

        private static void Case_02029()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2029,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,11,56,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,17,90,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,19,21,42,3), new GeneratedEnemyUnit(-7,-14,49,6,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "5b7fb4d79845b193f29b9afa82893565decdad9aaad8669032746c2149d3cd1e");
        }

        private static void Case_02030()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2030,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,2,14,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-4,47,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-12,95,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-7,54,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,16,88,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-7,24,4,1), new GeneratedEnemyUnit(-1,2,92,36,1), new GeneratedEnemyUnit(-4,4,35,41,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "a9a1e0dbb73ff046b0fc596db247e96b55f061131ba9be3a368d28f205055fd7");
        }

        private static void Case_02031()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2031,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,19,56,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,1,72,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-8,55,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-12,19,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,13,11,3,2), new GeneratedEnemyUnit(-8,-4,65,22,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "7d5e93f3843896dc80cea0e3244f4176f04cab3465d1aff70e3ba6631324201d");
        }

        private static void Case_02032()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2032,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,0,57,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,0,60,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-14,11,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,5,84,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,17,22,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,1,22,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,4,44,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-11,40,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-12,82,2,3), new GeneratedEnemyUnit(-20,-16,99,14,3), new GeneratedEnemyUnit(0,9,7,11,2), new GeneratedEnemyUnit(17,-16,62,25,1), new GeneratedEnemyUnit(-6,-14,10,34,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "544ad3c554b936ebf669d9a550b992c1b935e85b28d3f277ca54db68cd2d5413");
        }

        private static void Case_02033()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2033,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-4,25,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-11,99,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,1,78,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-18,59,7,1), new GeneratedEnemyUnit(-18,16,31,1,4), new GeneratedEnemyUnit(-17,18,63,31,2), new GeneratedEnemyUnit(17,-12,93,19,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "30c62df66fe108c9283c4a161b833d46c8652781b3d8733abfbeae4863578947");
        }

        private static void Case_02034()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2034,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-10,20,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-15,41,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-9,52,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-5,26,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,6,56,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-20,63,23,2), new GeneratedEnemyUnit(20,17,27,32,2), new GeneratedEnemyUnit(17,6,79,8,4), new GeneratedEnemyUnit(14,-6,6,18,3), new GeneratedEnemyUnit(-12,6,48,5,3), new GeneratedEnemyUnit(5,20,30,32,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "294959de7183343656fe9527413a91c7002f0f2a80f2efec23571a77da295575");
        }

        private static void Case_02035()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2035,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,15,84,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-5,82,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,0,89,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-4,78,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-20,94,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,8,27,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-12,68,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-13,83,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,19,11,50,3), new GeneratedEnemyUnit(15,-16,98,50,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "0b02705ce514200d6484bbc627c2b705af41eb797575e67c053de9f2641b37d2");
        }

        private static void Case_02036()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2036,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-16,29,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-6,62,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-6,85,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,3,87,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-6,71,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-5,79,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,0,42,12,1), new GeneratedEnemyUnit(19,-16,43,10,1), new GeneratedEnemyUnit(10,-5,98,27,2), new GeneratedEnemyUnit(-7,9,35,48,3), new GeneratedEnemyUnit(-13,-18,35,50,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "06476939c61fd580f1d961e4f211f715d4301c60d7124652698676cbdc3f44b5");
        }

        private static void Case_02037()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2037,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,12,43,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-3,24,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-9,45,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,9,78,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-3,86,13,1), new GeneratedEnemyUnit(-9,16,16,37,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "0ab93a39efe5925f3b76a9757563f3bc81390dca445e710ef0ddcc2b8d05c8d3");
        }

        private static void Case_02038()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2038,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-19,40,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,15,25,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,14,18,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-11,88,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,7,59,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,12,96,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,11,99,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-12,28,47,1), new GeneratedEnemyUnit(-8,18,7,39,3), new GeneratedEnemyUnit(20,-17,51,33,1), new GeneratedEnemyUnit(-18,15,74,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "1e1b5c4c6eb688b31f7ffe07db93e607ed23839bbe2191be27f970f70f686eca");
        }

        private static void Case_02039()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2039,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-17,90,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,14,98,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-20,81,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,17,38,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,19,80,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-20,99,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-11,37,7,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "26739cb2164a703b59961e85423e0054d0d24b480af2484da3cdb7774c1ad08c");
        }

        private static void Case_02040()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2040,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-11,73,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-7,14,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-18,86,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,7,50,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,4,21,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-12,16,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-5,84,10,1), new GeneratedEnemyUnit(-3,-9,58,37,1), new GeneratedEnemyUnit(11,-10,22,7,1), new GeneratedEnemyUnit(-9,-16,13,46,1), new GeneratedEnemyUnit(20,4,5,2,2), new GeneratedEnemyUnit(-18,-9,57,47,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "4dc20e401042f86a0fbe1531624bb1932262d956e73ff7e6a0881f097f867e98");
        }

        private static void Case_02041()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2041,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,10,14,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,0,46,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-12,71,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,0,31,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,12,74,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,15,16,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-20,10,5,2), new GeneratedEnemyUnit(-18,-7,57,33,1), new GeneratedEnemyUnit(-5,-13,75,2,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "f9ba16125cdf0e40955eb9d4bdc94f9e07c574fd5c5be608410f7628f7e12be6");
        }

        private static void Case_02042()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2042,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,2,96,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-7,83,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,5,87,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-5,53,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-15,87,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-20,42,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,13,20,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-11,53,30,3), new GeneratedEnemyUnit(15,1,86,32,3), new GeneratedEnemyUnit(-16,-10,75,28,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "20b7d392680708833aad0bbd7dc12baeb28b11956d9e7f469ea3e4b33712cba6");
        }

        private static void Case_02043()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2043,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-10,33,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,2,47,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,10,53,32,1), new GeneratedEnemyUnit(20,-2,98,37,4), new GeneratedEnemyUnit(18,13,75,18,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "6bc4250871c8dfecbb0ca09d41d49ae8d8efc5e717857dee468bf1399f69dac3");
        }

        private static void Case_02044()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2044,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,11,83,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-2,70,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-18,12,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,7,7,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-14,95,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,6,38,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,13,49,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,11,30,18,3), new GeneratedEnemyUnit(-11,8,27,13,1), new GeneratedEnemyUnit(11,16,94,14,3), new GeneratedEnemyUnit(-10,-1,18,16,4), new GeneratedEnemyUnit(-6,12,91,30,3), new GeneratedEnemyUnit(-15,-11,46,12,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "56d8b8a819849167b57cc129e5f2481e92db1521e89e8e7d69bb39bcf577ff33");
        }

        private static void Case_02045()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2045,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,12,41,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,7,49,26,2), new GeneratedEnemyUnit(-16,-18,54,24,4), new GeneratedEnemyUnit(-16,10,7,34,1), new GeneratedEnemyUnit(-16,8,77,49,1), new GeneratedEnemyUnit(-10,5,12,7,2), new GeneratedEnemyUnit(15,-6,89,25,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "3c825a80e148cf9f2ecadecff6cad9717915c9ff107c9633ff4c87cc17d2aa81");
        }

        private static void Case_02046()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2046,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,9,56,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,11,51,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-3,97,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,5,90,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,11,37,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-9,74,2,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "ec87528b4e5cbaaeed0318b57d6f487a320a435eb83bb7981d06023701aa52cc");
        }

        private static void Case_02047()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2047,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,8,8,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-12,100,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-14,24,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-12,14,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,11,10,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,19,84,24,1), new GeneratedEnemyUnit(-6,-10,42,42,3), new GeneratedEnemyUnit(14,-13,77,28,4), new GeneratedEnemyUnit(13,-6,6,22,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "e3060dad1fe58757e6c8a975d890bdce9b086cc9498994dfd0654e940b6b12d2");
        }

        private static void Case_02048()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2048,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,18,68,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,15,35,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,17,38,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-16,60,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-13,49,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-7,22,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,19,32,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,13,54,50,2), new GeneratedEnemyUnit(8,19,10,41,2), new GeneratedEnemyUnit(6,8,9,44,2), new GeneratedEnemyUnit(-5,-19,12,36,1), new GeneratedEnemyUnit(-2,17,79,38,1), new GeneratedEnemyUnit(3,17,99,48,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "a21c36fb30fa6427d6061ffa6494d2213b917da3f953909a7ffbed2889f4ebb8");
        }

        private static void Case_02049()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2049,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-20,13,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,5,94,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,3,85,46,3), new GeneratedEnemyUnit(10,-6,49,36,4), new GeneratedEnemyUnit(-1,-3,13,6,3), new GeneratedEnemyUnit(-9,2,30,24,4), new GeneratedEnemyUnit(19,16,95,39,3), new GeneratedEnemyUnit(19,-18,65,19,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "6352ca70265f0f25ff1a91c7c082250edd403839b7fb789d1ffa118fe1481e2a");
        }

        private static void Case_02050()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2050,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,1,82,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-5,79,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-15,61,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,12,55,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-10,63,39,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "45eefe19b809f62fdc8c37da677b83218c4139e6cdf5a649b489db49aa51d9c3");
        }

        private static void Case_02051()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2051,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,16,11,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-9,89,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-1,25,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,0,52,5,3), new GeneratedEnemyUnit(12,-20,17,40,2), new GeneratedEnemyUnit(3,-18,27,21,4), new GeneratedEnemyUnit(13,4,45,13,4), new GeneratedEnemyUnit(6,-8,98,27,2), new GeneratedEnemyUnit(-10,-9,100,46,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7cdd7c805672eed7b29a0c0c5175462bbc6de8354433999b35e29e96c0267057");
        }

        private static void Case_02052()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2052,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,19,89,5,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "3def656169173909ee14e6c4979f8ebf1159d331babda65214b5b22480be817d");
        }

        private static void Case_02053()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2053,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,11,25,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,3,95,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,14,99,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-3,100,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-14,100,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,13,66,17,3), new GeneratedEnemyUnit(-8,1,32,28,1), new GeneratedEnemyUnit(-13,11,7,50,1), new GeneratedEnemyUnit(20,1,29,26,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "72b4e06f509a650b3b83f140a5c226a51143babbefd42190c9559f361a309cc9");
        }

        private static void Case_02054()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2054,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-7,9,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-1,31,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-4,96,25,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "b39f04d7965bb76503907c5bfd048c16f2b20044746648acb283bdf8c124237a");
        }

        private static void Case_02055()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2055,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-5,59,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,18,70,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-17,91,35,4), new GeneratedEnemyUnit(-18,14,23,20,1), new GeneratedEnemyUnit(3,8,62,14,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "71b47e7b04b4f1121161e36ce02ed60e0e47275002f7260bef9131bafbfb5c0b");
        }

        private static void Case_02056()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2056,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,18,65,3,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "7a570c42eed222218bd68406d9331e3a54a5b60b3a5ea9871361d4a3c9f7102b");
        }

        private static void Case_02057()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2057,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-7,63,4,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "7e3bd7f5550e57c8b26e5a14645093b1ea850cea39821632a632c83643526c6a");
        }

        private static void Case_02058()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2058,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,15,8,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-14,90,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-8,95,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,19,68,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,4,74,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-15,86,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-15,23,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,6,17,1,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "66fb64f3cd8aea8c47c729a04473d527b1ab4ecb7e3471220184829ef56cc771");
        }

        private static void Case_02059()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2059,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-15,6,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-12,78,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,9,69,37,1), new GeneratedEnemyUnit(4,18,20,43,2), new GeneratedEnemyUnit(5,-15,96,19,2), new GeneratedEnemyUnit(-13,-20,33,19,3), new GeneratedEnemyUnit(8,-2,82,17,1), new GeneratedEnemyUnit(14,-9,57,32,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "43cb6281584312bc6098d0c16e538dc7a5c802e76df31166dcc801a880ed3500");
        }

        private static void Case_02060()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2060,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,5,66,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-18,44,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,9,79,50,3), new GeneratedEnemyUnit(-10,-18,8,3,3), new GeneratedEnemyUnit(-4,-2,84,25,4), new GeneratedEnemyUnit(-16,5,20,47,1), new GeneratedEnemyUnit(-5,-17,78,6,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e71410fdf4370f866b527287a8b6ed8df5f9c89a524c407606e2b4cd30a910e7");
        }

        private static void Case_02061()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2061,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,20,13,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-19,87,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,0,68,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-9,97,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-14,69,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,7,61,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,0,40,6,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "40104db27214fb6c78feec3506502bdfc2472c208d72b271634c31e5008a3739");
        }

        private static void Case_02062()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2062,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-1,75,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,6,16,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,15,20,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-4,29,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-15,67,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-2,91,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-5,35,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-7,66,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,3,32,20,1), new GeneratedEnemyUnit(-4,-8,44,2,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "53914aacc8f7c4a1ad281f6fc89a73db144866f86e1aad9a41b413ceeadb8179");
        }

        private static void Case_02063()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2063,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,9,79,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,19,38,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-10,15,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-4,57,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,5,82,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,5,71,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,11,38,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,6,97,16,4), new GeneratedEnemyUnit(-1,16,14,33,4), new GeneratedEnemyUnit(4,12,33,41,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "e9db73c7921641e002bda0366a0e368a208d96e9e8e993044593d312e9a4d6db");
        }

        private static void Case_02064()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2064,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,1,5,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-4,32,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,0,47,26,2), new GeneratedEnemyUnit(-5,-2,44,23,3), new GeneratedEnemyUnit(-13,-19,85,9,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "6ae22aa8b3bbba393124f0a9dbe6f7227937c1e8dc1c8113ff1c07eb325ee180");
        }

        private static void Case_02065()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2065,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-17,18,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,5,15,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,1,58,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-6,27,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,12,44,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-4,22,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-11,32,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,0,63,40,4), new GeneratedEnemyUnit(5,-7,23,45,4), new GeneratedEnemyUnit(-16,-4,55,16,3), new GeneratedEnemyUnit(-8,-16,64,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "4122fb2454834dff89cb1456df33c741aa47a45dc72a3003a9764a950f1433b8");
        }

        private static void Case_02066()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2066,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-12,20,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,16,57,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,0,88,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-4,51,33,3), new GeneratedEnemyUnit(12,3,65,29,3), new GeneratedEnemyUnit(20,-7,28,41,3), new GeneratedEnemyUnit(-9,-14,27,26,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "5ecff16b229ed5d8ced96157ff357280fa4e4a06a4962d0ef4b573a164623f31");
        }

        private static void Case_02067()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2067,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,2,39,4,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "dbeeb31538072a990f4ea461bc38701209db6046ff488cf866fcf144e32b3e2f");
        }

        private static void Case_02068()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2068,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,16,64,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-1,98,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-3,98,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-10,49,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,16,38,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,16,6,41,2), new GeneratedEnemyUnit(-15,-17,52,47,4), new GeneratedEnemyUnit(2,4,96,1,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "7ac89092e5eab566288543eecf3e1f017e64100dda0af47c99bed342fc92b345");
        }

        private static void Case_02069()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2069,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,19,40,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-2,89,20,3), new GeneratedEnemyUnit(17,-3,39,33,3), new GeneratedEnemyUnit(-8,15,62,33,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "39c529226fa786727ddf0a6389bbf8064f721d4519b561a0f7137f83630789c0");
        }

        private static void Case_02070()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2070,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,12,73,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,11,54,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-13,65,49,4), new GeneratedEnemyUnit(5,-14,77,9,3), new GeneratedEnemyUnit(-9,-3,6,8,2), new GeneratedEnemyUnit(2,18,65,45,2), new GeneratedEnemyUnit(11,-15,92,18,2), new GeneratedEnemyUnit(-2,-11,38,45,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "7de342f899140b9412b6d771b6b22848d7a0390e04866e64efb3dcec4c063411");
        }

        private static void Case_02071()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2071,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-16,43,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-2,55,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-16,55,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-19,100,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-12,96,45,4), new GeneratedEnemyUnit(9,7,78,38,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "65eb1cc0186dd99a60e535398a8ae9c0f4fe3c86544cc8cf1100bcec7b063f75");
        }

        private static void Case_02072()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2072,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-2,47,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,14,93,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,2,100,19,4), new GeneratedEnemyUnit(-6,9,54,29,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "baa09f7c370143b9ccbe10a1f9be577a0c47b11654e87e291214077e08f0529e");
        }

        private static void Case_02073()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2073,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,20,25,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,7,33,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,19,100,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-7,30,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,20,55,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-10,93,37,1), new GeneratedEnemyUnit(-19,-9,40,40,1), new GeneratedEnemyUnit(6,-1,24,35,3), new GeneratedEnemyUnit(17,12,28,19,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "bc535bbb8fee14b00f63cc8ed6ca71c66eb2e2de3e9a5014e3449de20cabe615");
        }

        private static void Case_02074()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2074,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,20,76,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,5,22,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,10,55,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,0,58,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-20,50,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-8,23,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-2,96,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,3,52,22,4), new GeneratedEnemyUnit(-17,-14,45,7,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1111a928aa2d575d68cb9e3c22ed834fb6a9bd04cd2b01dcb00082bd4a0cbf52");
        }

        private static void Case_02075()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2075,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-1,84,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,8,78,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-1,19,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,5,100,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-10,44,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,2,94,14,1), new GeneratedEnemyUnit(15,-19,23,38,1), new GeneratedEnemyUnit(-9,20,8,47,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "1fa2fc2359cc90cf4c4ba335472c8846d3cf325ca9dd4ed9344451c511eb10ac");
        }

        private static void Case_02076()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2076,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,12,15,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,8,75,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-19,86,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,11,57,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,17,33,18,2), new GeneratedEnemyUnit(6,10,54,36,3), new GeneratedEnemyUnit(20,-14,75,19,1), new GeneratedEnemyUnit(-4,-11,25,23,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "4855b979213baaf2880fbcd8ef8a3d75bac84da0d8d5a8b66eb78c0d421edb79");
        }

        private static void Case_02077()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2077,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-13,60,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-15,45,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-3,12,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-15,82,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-16,14,6,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "362c935511e6dcff772c431b4f4f5049e3716bced100937ae95f9152e92e884f");
        }

        private static void Case_02078()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2078,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-9,88,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,0,38,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-10,45,17,4), new GeneratedEnemyUnit(5,10,24,23,4), new GeneratedEnemyUnit(-9,-3,72,28,2), new GeneratedEnemyUnit(19,2,86,10,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "85f358fe79ec18ebdd792da73f312436d7323affde597b182466ad3f9918bc2c");
        }

        private static void Case_02079()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2079,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,20,50,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-5,50,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,3,90,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,8,13,3,4), new GeneratedEnemyUnit(0,-4,11,12,3), new GeneratedEnemyUnit(-7,4,32,5,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "c2836e488eeaa5462e8002a4609074dd4007f230702ea25ed6b836239a58881e");
        }

        private static void Case_02080()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2080,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-18,9,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-6,91,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,14,9,31,4), new GeneratedEnemyUnit(-11,15,62,35,1), new GeneratedEnemyUnit(6,-1,33,12,2), new GeneratedEnemyUnit(-15,9,21,34,1), new GeneratedEnemyUnit(-4,10,17,9,3), new GeneratedEnemyUnit(-2,-19,85,9,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "2dc6355f5abe049aa002b99eb17322defeb83cf9a38065ba407ac0ce73f9775b");
        }

        private static void Case_02081()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2081,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-12,5,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,14,91,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-11,58,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-17,69,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,10,98,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,3,64,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-3,37,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,20,29,37,1), new GeneratedEnemyUnit(-18,-11,85,40,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "4385116428fb379c8799c73022507c082201a7bd72342c26f289d21fa3027e14");
        }

        private static void Case_02082()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2082,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,9,17,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-11,34,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-7,54,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-18,30,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,19,87,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,18,22,36,2), new GeneratedEnemyUnit(-7,9,60,46,2), new GeneratedEnemyUnit(11,-17,6,46,1), new GeneratedEnemyUnit(10,8,30,26,2), new GeneratedEnemyUnit(0,5,60,17,1), new GeneratedEnemyUnit(0,18,47,21,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "9af29c14317180d02877bd4dcf1f8f5d99450e8faa6bcc5b017c2ffb4af869d1");
        }

        private static void Case_02083()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2083,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,18,85,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,7,24,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-19,90,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,20,92,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-5,59,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,5,78,24,3), new GeneratedEnemyUnit(-17,-15,30,49,3), new GeneratedEnemyUnit(8,2,39,34,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "80cf605a9e13083a12ccb7ee3fabe109974917143998eb0794fd6596508f01ab");
        }

        private static void Case_02084()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2084,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-11,40,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-8,32,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,10,68,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,18,5,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,9,48,9,4), new GeneratedEnemyUnit(18,-7,7,32,2), new GeneratedEnemyUnit(9,3,46,4,3), new GeneratedEnemyUnit(-18,-9,70,6,2), new GeneratedEnemyUnit(14,-17,83,38,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "255783438202974951c2bdf2d48207f780d2854bf0d47eaf7775e66baa92b31a");
        }

        private static void Case_02085()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2085,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-20,91,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-2,98,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-14,7,40,4), new GeneratedEnemyUnit(-16,-12,35,42,2), new GeneratedEnemyUnit(7,-2,40,18,4), new GeneratedEnemyUnit(12,-20,85,16,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "242973d092c392ddd3b129639445528a5870662325a030d24423b212cc973514");
        }

        private static void Case_02086()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2086,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-3,15,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,8,47,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-12,47,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,18,79,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,13,61,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,13,13,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,1,74,44,1), new GeneratedEnemyUnit(-9,15,71,18,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "01f6a7d9d14ff7c9b7ec2554e6e562c828fe94fb37102264fb0381e73b2728e5");
        }

        private static void Case_02087()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2087,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-4,22,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,17,29,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-12,95,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-10,77,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-11,99,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-1,100,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-3,45,47,3), new GeneratedEnemyUnit(3,-11,8,19,4), new GeneratedEnemyUnit(-5,-18,38,29,1), new GeneratedEnemyUnit(6,-6,94,10,4), new GeneratedEnemyUnit(18,-7,36,48,4), new GeneratedEnemyUnit(-18,-13,31,17,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "831c11b8b825be382b3d8ccc609a1794c0653c67687ba107281abadcdbf35846");
        }

        private static void Case_02088()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2088,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-2,99,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,13,60,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-11,50,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,8,37,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,11,18,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,0,74,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,11,92,46,1), new GeneratedEnemyUnit(4,-10,13,47,1), new GeneratedEnemyUnit(7,9,8,14,2), new GeneratedEnemyUnit(2,-2,67,16,3), new GeneratedEnemyUnit(-13,-9,58,28,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "88566cef070fa37323eec614a787b7a058610416593dd0088aff7c3ebeabf96e");
        }

        private static void Case_02089()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2089,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-15,31,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,3,67,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-3,55,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,5,20,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-11,18,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-13,5,45,2), new GeneratedEnemyUnit(0,-14,44,10,3), new GeneratedEnemyUnit(18,19,17,21,3), new GeneratedEnemyUnit(13,2,7,18,1), new GeneratedEnemyUnit(-4,-16,37,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "26deb3ebbd32fa1f14ea53da1488e47dedafec4ceb324b7bb19a8711f536509f");
        }

        private static void Case_02090()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2090,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,3,45,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,10,8,46,2), new GeneratedEnemyUnit(5,10,95,19,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "cfae6167e6189b4905f145ce2bff51a2e0e5b5795551ed06f2ab52b5c5e59452");
        }

        private static void Case_02091()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2091,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,2,36,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,6,19,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-6,18,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,11,65,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,16,81,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-10,51,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-6,95,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-8,19,33,2), new GeneratedEnemyUnit(-17,11,14,45,3), new GeneratedEnemyUnit(-8,10,53,43,3), new GeneratedEnemyUnit(-7,10,22,9,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "abeea3bd45bf24e15cb2539cfeaf42f255bdc09f48f926aacddbd06758bfb634");
        }

        private static void Case_02092()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2092,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,4,67,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-3,54,19,4), new GeneratedEnemyUnit(-11,-5,98,26,4), new GeneratedEnemyUnit(15,18,66,49,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "7b763ce752f47462902d0d8bb38c18899db010cf7225e53e49e58166f4b8d206");
        }

        private static void Case_02093()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2093,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,14,96,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-20,55,7,4), new GeneratedEnemyUnit(6,3,86,5,4), new GeneratedEnemyUnit(-14,-5,36,30,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "9cbc36ca74fc394443afd93611a0209390d0dc62a9da75e589516a0a8eaea309");
        }

        private static void Case_02094()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2094,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-11,5,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-19,12,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,12,60,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-19,28,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,11,60,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-19,27,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-5,50,10,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "5dcc68c431cafd9f91b95b9fcae0262ea2ac5fec1fe5e56b1f224a2aed48ddd7");
        }

        private static void Case_02095()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2095,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-1,78,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-9,96,34,1), new GeneratedEnemyUnit(4,-20,59,23,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "d7e1f903f2112d2707e961f9208cab5c02fa136fcaab89d9cbe13b972b5f7f66");
        }

        private static void Case_02096()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2096,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,17,51,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-2,28,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-12,15,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-10,66,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-8,27,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,14,82,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,0,20,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-3,79,11,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "e4f0768505b168f5df6982ff106be705dfe92b6511717d956ca32b4d3507afc4");
        }

        private static void Case_02097()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2097,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,10,48,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-20,7,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-9,84,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,13,45,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,15,66,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-12,48,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-10,31,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-4,39,14,1), new GeneratedEnemyUnit(-14,6,38,2,4), new GeneratedEnemyUnit(-18,-1,51,42,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "c0f2c4633ccf6580e16d973c45f1ec8611a22fea868d3ee40253c18f90b87823");
        }

        private static void Case_02098()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2098,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,3,77,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-3,18,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,12,40,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-10,87,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-3,7,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-6,78,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,16,18,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-10,15,17,4), new GeneratedEnemyUnit(-4,3,70,29,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "fc0a8967a5ba3d4b047e64d03614f389ff1fc6d85fa79e84f34efe9c689a3d9c");
        }

        private static void Case_02099()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2099,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,6,44,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,13,38,33,4), new GeneratedEnemyUnit(4,4,12,3,4), new GeneratedEnemyUnit(-20,-19,70,15,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "29aeeca13b8ca4783a9389980bbd695ea693cadb49e0ba677191f47afb8f9469");
        }

        private static void Case_02100()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2100,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,7,8,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-16,22,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-12,94,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-13,47,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-11,88,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-19,14,41,2), new GeneratedEnemyUnit(-7,-14,93,36,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ae9a09950b65819eaaf3d779dec58fc806354570bcf87fcb1b4003191482e8a0");
        }

        private static void Case_02101()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2101,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-14,68,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,12,61,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,10,44,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-8,91,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-1,69,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-16,14,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,9,16,24,4), new GeneratedEnemyUnit(20,-15,75,45,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "d9d87af6b31c354cbf85448e41d5c8bf2eccff24bfc986d94cffae09c7fe14b0");
        }

        private static void Case_02102()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2102,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,17,5,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,17,44,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,13,88,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,7,96,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,15,39,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,19,13,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-8,11,6,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ca288d9ec530e8ed5130ebea85f044003e504c45804d19a3cf0afa0829d9d553");
        }

        private static void Case_02103()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2103,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,12,58,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,9,88,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,7,87,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-1,99,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-15,96,18,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "5ffda1bd4920107f1b780c3758e2896c4a74477d418366cb1f5e1fbaf99a4cfc");
        }

        private static void Case_02104()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2104,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-6,93,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-4,59,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-4,54,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,10,59,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-12,55,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-16,73,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,13,76,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,1,42,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-15,65,5,3), new GeneratedEnemyUnit(17,15,50,19,2), new GeneratedEnemyUnit(11,20,24,7,1), new GeneratedEnemyUnit(-8,11,50,10,1), new GeneratedEnemyUnit(-20,-1,21,45,4), new GeneratedEnemyUnit(3,10,56,49,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "15713c0eeb5791eb4bab1a75686fd8e835e2318dabcc236068277d21bbd516bc");
        }

        private static void Case_02105()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2105,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-6,48,5,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "c14050d4b4db926770c400ebeed9545d3fcd3f6a1bf6c259e1d53efdfa53eaeb");
        }

        private static void Case_02106()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2106,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,9,89,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,17,38,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,20,95,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-4,53,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-7,63,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,4,87,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-10,53,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,0,82,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,13,56,22,2), new GeneratedEnemyUnit(8,8,70,24,2), new GeneratedEnemyUnit(-8,9,5,40,2), new GeneratedEnemyUnit(6,6,32,14,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "c3ce5fc3be36fb118131ff5dfa7d5129fcaa27f91221dfcd224abcf6b429ce4e");
        }

        private static void Case_02107()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2107,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,15,76,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,11,52,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,2,13,5,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "5b9fdce14dabdd001f002bb3c37ea5a05b4010950ae56a0fbd639481c8726e72");
        }

        private static void Case_02108()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2108,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,2,97,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,3,60,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-8,27,12,2), new GeneratedEnemyUnit(8,-20,53,37,4), new GeneratedEnemyUnit(5,-3,12,20,3), new GeneratedEnemyUnit(8,-10,36,5,1), new GeneratedEnemyUnit(-18,-7,7,23,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "1b9b7957d458fd25f2e4ef5e3cde57831c0ddb9ac850646a917ef8c96927a200");
        }

        private static void Case_02109()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2109,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,19,22,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-16,35,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,18,47,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,18,92,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,10,24,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-3,12,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-12,67,8,1), new GeneratedEnemyUnit(-16,-10,72,48,3), new GeneratedEnemyUnit(-10,-5,22,43,4), new GeneratedEnemyUnit(5,4,53,36,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "2af68b1baf9e1066dc7425b4f37a1a4266b22268d725fe809873725be4ef9c7c");
        }

        private static void Case_02110()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2110,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-6,59,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-6,68,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-4,69,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,7,39,25,2), new GeneratedEnemyUnit(12,-19,63,6,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "be7248ade8588a0c81975376073bf881531190aabe93d146bf67e8a5a62e2620");
        }

        private static void Case_02111()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2111,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-6,61,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,12,62,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,5,77,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,11,67,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,10,90,16,1), new GeneratedEnemyUnit(6,11,9,11,4), new GeneratedEnemyUnit(7,4,10,32,2), new GeneratedEnemyUnit(0,3,9,37,2), new GeneratedEnemyUnit(-20,-10,63,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "0e41a86223ea824116c35e6316ab5efc795bf7522601e8cdcbee5f16e38865ba");
        }

        private static void Case_02112()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2112,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-20,94,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-4,45,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,4,22,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-4,54,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,20,46,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,7,78,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,13,70,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-9,14,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-16,78,9,1), new GeneratedEnemyUnit(10,-13,13,2,2), new GeneratedEnemyUnit(-15,11,89,31,3), new GeneratedEnemyUnit(0,-3,67,24,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "ee21bbc44dcdfa3f78c9076f5c5b69fc8f9ea558f22c8be602457e53df2f7a21");
        }

        private static void Case_02113()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2113,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,13,66,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,5,43,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-12,49,5,4), new GeneratedEnemyUnit(-11,-14,86,22,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "465f7d78de7bee3a84b9e4f9bcf7490f38078022c3ac22e0e752552a0f7e887d");
        }

        private static void Case_02114()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2114,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,0,80,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-2,58,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,18,77,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-16,37,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,19,29,10,3), new GeneratedEnemyUnit(15,9,49,19,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0aa821c02916be82a5cebc1647f13b3fcf33e1d414a82d18165c912c3d218bde");
        }

        private static void Case_02115()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2115,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-17,6,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-3,74,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,19,76,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,9,53,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,12,21,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,6,91,13,4), new GeneratedEnemyUnit(-11,-9,92,11,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "f74540ab1bd589ade3df1c015778362975c1144193dbdc21984826a449cf4698");
        }

        private static void Case_02116()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2116,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,20,95,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,6,35,13,2), new GeneratedEnemyUnit(1,-8,57,46,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "e119355d8ca115c4a3ea0c55785899e45d0eb9ad7d1852edaa7c2802b7b56ada");
        }

        private static void Case_02117()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2117,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,7,92,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-12,12,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c559d2c7af9517b3ec05aa4f3a37c4c92e9b9454fefb1f297def855a64a7ee82");
        }

        private static void Case_02118()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2118,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-7,81,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-17,83,13,2), new GeneratedEnemyUnit(12,-19,26,14,1), new GeneratedEnemyUnit(-10,1,25,28,3), new GeneratedEnemyUnit(-9,-15,98,20,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "5181153a358ff5174e0c023a3be0dfb71b995992727ed37aed0f98054fd7b574");
        }

        private static void Case_02119()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2119,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-19,53,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,14,27,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,1,51,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-14,35,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,4,95,27,4), new GeneratedEnemyUnit(-20,-1,63,26,2), new GeneratedEnemyUnit(7,0,34,7,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "f8dc111040875bb0e244184262a04dffb7e2308ff8e9fc00b7eee6716879a458");
        }

        private static void Case_02120()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2120,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,3,73,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-10,90,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-13,99,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-7,82,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,7,18,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,1,12,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-10,14,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,6,66,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,12,62,20,3), new GeneratedEnemyUnit(-5,-13,79,43,3), new GeneratedEnemyUnit(-15,3,53,48,3), new GeneratedEnemyUnit(-15,8,36,26,1), new GeneratedEnemyUnit(-9,-11,59,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "222f88c60535e6c5ddd9889ac5a40254616fe0bbc27b9739faec0133dd23f809");
        }

        private static void Case_02121()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2121,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,18,46,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,3,98,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-10,24,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,18,58,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-2,91,12,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "2f8c529ece9bf2c35847b3b3fe59e3321e32bbd5bf81fa73fd5f6cb9ffd74799");
        }

        private static void Case_02122()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2122,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-19,100,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-10,29,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,18,44,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-1,24,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,15,81,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-12,99,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,1,52,7,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "fc2d5520bff988a27bd2c6e49a3facd8ee79269d3872d0e69ef100e880d8c71f");
        }

        private static void Case_02123()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2123,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,14,89,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,19,96,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-12,58,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-10,46,27,4), new GeneratedEnemyUnit(4,-14,47,38,3), new GeneratedEnemyUnit(-16,5,66,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "91b963e3432922807e63bfcce1ce5b7cf4e10ba3c94d4cca1bb3cbc91fc47a4e");
        }

        private static void Case_02124()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2124,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-19,97,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,20,26,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,9,89,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,11,12,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-11,21,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,18,62,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "fb6db008ed13829f7965c177d213ced235cd4b4046433ea37e95c026111fa129");
        }

        private static void Case_02125()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2125,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-7,37,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-4,75,6,3), new GeneratedEnemyUnit(12,4,50,48,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "04c6bc16c481f800c00d0e77c8bbfe850be1dc198aaa255c6f1e0b9e8502194b");
        }

        private static void Case_02126()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2126,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,19,41,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,0,15,29,1), new GeneratedEnemyUnit(12,7,96,13,3), new GeneratedEnemyUnit(6,-15,15,40,4), new GeneratedEnemyUnit(6,-20,78,12,3), new GeneratedEnemyUnit(11,15,75,34,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "5d395f8bf966a8d938377c47901d05320d270f00b3cd620e071c7c5225955787");
        }

        private static void Case_02127()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2127,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-13,82,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-18,83,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-8,77,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-8,80,27,2), new GeneratedEnemyUnit(-20,-1,55,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "410ffd335f4d0ac7cbff4b82970c76fc7846ba24709d44d9f3181b78e999ca84");
        }

        private static void Case_02128()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2128,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-10,77,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,1,97,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-11,98,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-12,69,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-6,30,5,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "de62f1065fddadb787b5da06fc9a68da2bdaa0f416de67f105d4d83d3b3117eb");
        }

        private static void Case_02129()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2129,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-3,30,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,1,94,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,1,19,21,2), new GeneratedEnemyUnit(4,-15,80,36,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "9d2542734a4e9b3558cd032565c553c98e2f97e3c8efed9d6f93560a65a7d7c8");
        }

        private static void Case_02130()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2130,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,9,77,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-17,77,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,6,58,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-13,43,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-15,95,36,4), new GeneratedEnemyUnit(0,-12,41,3,2), new GeneratedEnemyUnit(12,-3,85,16,4), new GeneratedEnemyUnit(-2,11,6,18,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "5ca5b9c0ea8863b05d896d932bfda8877c4b2d4c1893e096a060ce9c61a5efc1");
        }

        private static void Case_02131()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2131,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,13,17,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,1,9,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-6,63,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,18,57,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,3,43,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,12,98,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-19,65,11,3), new GeneratedEnemyUnit(-6,-14,90,44,3), new GeneratedEnemyUnit(5,-8,54,28,3), new GeneratedEnemyUnit(5,0,90,44,2), new GeneratedEnemyUnit(5,16,33,28,2), new GeneratedEnemyUnit(5,14,49,30,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "ff5cf3529285c298f21641aa5017be3548f3c41747299a5a5ac5e1585e05411f");
        }

        private static void Case_02132()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2132,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,13,22,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-2,62,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-7,75,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-7,82,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,8,22,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,10,82,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,3,59,4,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "11640108ea328cefe6787ac6dec805ebb78b4d91a324d9e04baf66f9da28e74c");
        }

        private static void Case_02133()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2133,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-20,38,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-9,38,40,4), new GeneratedEnemyUnit(-6,-17,65,40,3), new GeneratedEnemyUnit(-4,-19,65,50,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "d70cc7f6129565c5bfc9ac8d6369e05a9de27e068b37d8fd0806f8c3b8bf850a");
        }

        private static void Case_02134()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2134,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,9,20,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-8,86,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-14,67,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,13,27,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,13,67,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-16,10,31,3), new GeneratedEnemyUnit(1,-16,58,39,2), new GeneratedEnemyUnit(-18,-13,100,14,2), new GeneratedEnemyUnit(-18,9,83,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "af568dacde896e5c8b7b9390ba5ba70ea6db0d547d0a653886e9bfde6a6e4450");
        }

        private static void Case_02135()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2135,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,16,57,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-3,97,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,11,85,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-12,14,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,18,80,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,8,8,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-12,61,37,3), new GeneratedEnemyUnit(-20,-10,61,7,1), new GeneratedEnemyUnit(15,18,29,33,3), new GeneratedEnemyUnit(2,17,16,34,1), new GeneratedEnemyUnit(6,4,42,44,1), new GeneratedEnemyUnit(2,-7,44,44,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "cc73a99ae6dc30145fa41c80adc75a25c344476d6c5928a4fbd3d409740f1c97");
        }

        private static void Case_02136()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2136,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-8,91,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,6,21,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-7,49,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,20,66,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,1,60,24,3), new GeneratedEnemyUnit(-1,-16,36,32,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "9f510bd1d8a6c14850f86ba7f4481ccb9e1d973634e1961ed7f0b8098dcf2ba7");
        }

        private static void Case_02137()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2137,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,3,79,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-13,11,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-7,55,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,13,37,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-6,93,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,8,73,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-19,84,24,4), new GeneratedEnemyUnit(-19,8,16,9,4), new GeneratedEnemyUnit(8,16,54,18,1), new GeneratedEnemyUnit(4,-10,49,30,4), new GeneratedEnemyUnit(-17,-16,39,34,2), new GeneratedEnemyUnit(3,12,86,29,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "5acba4013fc0c89d3a6dbbf7820e46652ced61ad7ed096e6e8cdf26a074883ee");
        }

        private static void Case_02138()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2138,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,2,92,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,8,42,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,6,88,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-17,72,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,9,69,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,11,97,30,4), new GeneratedEnemyUnit(-11,5,28,26,2), new GeneratedEnemyUnit(18,2,83,30,1), new GeneratedEnemyUnit(18,-11,14,46,2), new GeneratedEnemyUnit(-18,-2,11,14,3), new GeneratedEnemyUnit(0,-10,79,4,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "bb743dd9645dd79b9fa0cd54c0cd3e6e450953c6a70a16f83fdcfbfabd73e20c");
        }

        private static void Case_02139()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2139,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-1,20,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,2,42,12,4), new GeneratedEnemyUnit(-5,-14,34,1,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "054ad24933a13405bd61412b20a9a165fa0a9cade3032668093fcc757c2bb069");
        }

        private static void Case_02140()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2140,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-6,77,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-8,77,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-17,12,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-9,75,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,15,97,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,10,95,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,14,40,46,3), new GeneratedEnemyUnit(-7,2,79,33,2), new GeneratedEnemyUnit(8,20,91,24,2), new GeneratedEnemyUnit(-14,-20,45,18,2), new GeneratedEnemyUnit(6,8,48,28,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "00f262ae156f22ff07b22583c77aa3320c2d6b2adeebf65fcb1af51fb5742f2d");
        }

        private static void Case_02141()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2141,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,12,79,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-19,55,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,7,18,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-15,76,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-7,81,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-8,92,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-17,7,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-10,48,2,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "055f25f7762097443a12588016bf93f9a34df9032ce1068086a5814a2b3c12f7");
        }

        private static void Case_02142()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2142,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,13,39,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-19,76,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,15,86,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,17,92,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,15,45,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,10,24,3,3), new GeneratedEnemyUnit(-10,3,17,17,3), new GeneratedEnemyUnit(-13,-15,53,44,1), new GeneratedEnemyUnit(-10,-1,6,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "b8de52cdf5431800719928a5b405d08f3b9290eedcf15bd4a6ea0c1ea5e50ae6");
        }

        private static void Case_02143()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2143,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-12,68,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-11,47,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-11,67,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,4,30,11,3), new GeneratedEnemyUnit(1,1,34,7,3), new GeneratedEnemyUnit(14,-13,46,24,2), new GeneratedEnemyUnit(-14,20,71,27,3), new GeneratedEnemyUnit(4,1,12,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "db77914688a172646e87a26363b96cb185c6073362c238bb3a2ad4f72ec5f627");
        }

        private static void Case_02144()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2144,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-3,54,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,4,29,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,13,52,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-7,42,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,3,22,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-10,62,12,3), new GeneratedEnemyUnit(15,-9,55,31,1), new GeneratedEnemyUnit(17,7,83,17,1), new GeneratedEnemyUnit(5,-1,79,31,4), new GeneratedEnemyUnit(17,-12,47,36,1), new GeneratedEnemyUnit(20,7,97,29,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "a5835698e91d7b5052ad134fa354222aaef8320ea681a06f4711741f66e55053");
        }

        private static void Case_02145()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2145,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,10,35,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,3,54,39,3), new GeneratedEnemyUnit(-2,14,24,20,4), new GeneratedEnemyUnit(4,-16,97,20,2), new GeneratedEnemyUnit(-4,-9,51,4,3), new GeneratedEnemyUnit(15,8,13,10,2), new GeneratedEnemyUnit(-14,-14,11,45,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "b528d1a4a837bc04a7c332fe4c9e3b831852f1fa1c985f6fe68aed61fd0f2900");
        }

        private static void Case_02146()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2146,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-10,11,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-7,57,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,0,83,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,6,38,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-3,81,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-11,23,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,9,23,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,3,72,37,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2cbad4122b9f5d070b7849b9964fd0a177ccbff5c88f4cb4275f8121002c0d9e");
        }

        private static void Case_02147()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2147,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,0,33,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,3,59,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,1,19,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,18,94,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-14,27,42,3), new GeneratedEnemyUnit(0,12,11,39,4), new GeneratedEnemyUnit(3,-5,13,28,2), new GeneratedEnemyUnit(12,13,63,23,1), new GeneratedEnemyUnit(-3,-17,57,43,3), new GeneratedEnemyUnit(20,1,93,41,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "02a1ab53e3ce4318d9a5743dbf3aa66b673a567762dedffbd706f1678c3fd73c");
        }

        private static void Case_02148()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2148,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-2,61,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-18,88,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,2,52,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,12,75,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,17,61,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,1,8,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-16,98,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-3,46,32,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a528d97348fdfdb6936ddd73108b11484d53baff1f9943aac2f6d237dc981529");
        }

        private static void Case_02149()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2149,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,3,99,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,16,74,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,7,64,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-19,43,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-7,85,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-13,40,14,2), new GeneratedEnemyUnit(-5,-18,72,38,1), new GeneratedEnemyUnit(2,18,40,9,1), new GeneratedEnemyUnit(11,-10,38,16,4), new GeneratedEnemyUnit(14,-11,52,36,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "2fa6a364a377c4dd4e1f0d3ca1c43414a17d472cc9c9422493509248a50c3678");
        }

        private static void Case_02150()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2150,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-9,87,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,16,17,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-10,39,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-13,48,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,18,77,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-20,24,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-10,11,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-2,77,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-6,37,13,1), new GeneratedEnemyUnit(-13,16,56,29,2), new GeneratedEnemyUnit(19,-1,27,8,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "0a7c9b2cdea995d957304bf7a9bed91dd46063f4d104cc01b2ff3cd8d0c722d0");
        }

        private static void Case_02151()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2151,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-4,17,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-11,11,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-11,40,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-4,37,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,10,54,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,7,30,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-4,19,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-10,39,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-5,54,1,3), new GeneratedEnemyUnit(-19,-17,13,6,1), new GeneratedEnemyUnit(-18,-18,14,33,2), new GeneratedEnemyUnit(9,10,21,22,4), new GeneratedEnemyUnit(-11,-14,85,43,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "2a875e45659d4e0ba40826cea173b336efe37223530f4a46d2201de23cfda901");
        }

        private static void Case_02152()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2152,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,11,84,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,12,98,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,18,15,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-3,95,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,3,61,33,4), new GeneratedEnemyUnit(-14,-12,74,38,3), new GeneratedEnemyUnit(10,-17,27,21,2), new GeneratedEnemyUnit(17,8,86,48,3), new GeneratedEnemyUnit(17,6,90,1,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "7bcaaead9a5ffef3254e1f66a6fcaa2bd3343a20e97b435ec14f6d96fc2ee210");
        }

        private static void Case_02153()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2153,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,15,42,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-2,61,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,6,41,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-6,61,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,15,51,27,3), new GeneratedEnemyUnit(-6,-5,46,29,1), new GeneratedEnemyUnit(-11,-14,5,4,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "741ea144d8cd6f87203bb8195e67012e913e7fd0ccb647e364193a25deb60c9d");
        }

        private static void Case_02154()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2154,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-12,69,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,1,70,30,1), new GeneratedEnemyUnit(15,11,53,33,4), new GeneratedEnemyUnit(3,-6,20,31,1), new GeneratedEnemyUnit(13,-20,7,34,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "c15202c619c1a50bb4a14ccc93ea262ff3c056e3a319949d4f992340825519cf");
        }

        private static void Case_02155()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2155,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-19,28,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,11,61,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-6,81,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,6,52,36,4), new GeneratedEnemyUnit(15,-19,53,12,4), new GeneratedEnemyUnit(16,-11,43,48,2), new GeneratedEnemyUnit(-9,1,76,26,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "fa3fe35bed1d429b751b7885a83712d002bf88b20f8b13c1b040456114a58090");
        }

        private static void Case_02156()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2156,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,9,98,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-12,7,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,5,27,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,11,71,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-11,17,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,4,6,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,13,100,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,1,21,2,1), new GeneratedEnemyUnit(0,6,48,21,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "493c00f6f8372902ac976c3ca8c097f2cdd3e8e17a1cdac6ef62a3231267c4c9");
        }

        private static void Case_02157()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2157,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-13,81,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,13,97,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-3,58,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,16,28,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,7,99,3,2), new GeneratedEnemyUnit(14,3,82,45,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "8572f0fb2167b51445be9e78e7bfdfbf19e8a60dcf5886e99580025e0417a9a7");
        }

        private static void Case_02158()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2158,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,19,38,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,13,94,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-19,83,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-2,86,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-1,34,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-5,12,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-20,96,17,3), new GeneratedEnemyUnit(-8,1,14,12,4), new GeneratedEnemyUnit(-20,-6,87,36,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "6bb5910a165ef66dea17e86b7fc0f8fb607bd926e95cbec714497e3fd0e62af3");
        }

        private static void Case_02159()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2159,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-5,48,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,9,20,4,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "e3643bfdb5d256c68dd890894fe8e76258d3719d47fc2701070a9580de605209");
        }

    }
}
