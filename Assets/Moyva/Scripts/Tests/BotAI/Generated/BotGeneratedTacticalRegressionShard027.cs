using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard027
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_04860();
            Case_04861();
            Case_04862();
            Case_04863();
            Case_04864();
            Case_04865();
            Case_04866();
            Case_04867();
            Case_04868();
            Case_04869();
            Case_04870();
            Case_04871();
            Case_04872();
            Case_04873();
            Case_04874();
            Case_04875();
            Case_04876();
            Case_04877();
            Case_04878();
            Case_04879();
            Case_04880();
            Case_04881();
            Case_04882();
            Case_04883();
            Case_04884();
            Case_04885();
            Case_04886();
            Case_04887();
            Case_04888();
            Case_04889();
            Case_04890();
            Case_04891();
            Case_04892();
            Case_04893();
            Case_04894();
            Case_04895();
            Case_04896();
            Case_04897();
            Case_04898();
            Case_04899();
            Case_04900();
            Case_04901();
            Case_04902();
            Case_04903();
            Case_04904();
            Case_04905();
            Case_04906();
            Case_04907();
            Case_04908();
            Case_04909();
            Case_04910();
            Case_04911();
            Case_04912();
            Case_04913();
            Case_04914();
            Case_04915();
            Case_04916();
            Case_04917();
            Case_04918();
            Case_04919();
            Case_04920();
            Case_04921();
            Case_04922();
            Case_04923();
            Case_04924();
            Case_04925();
            Case_04926();
            Case_04927();
            Case_04928();
            Case_04929();
            Case_04930();
            Case_04931();
            Case_04932();
            Case_04933();
            Case_04934();
            Case_04935();
            Case_04936();
            Case_04937();
            Case_04938();
            Case_04939();
            Case_04940();
            Case_04941();
            Case_04942();
            Case_04943();
            Case_04944();
            Case_04945();
            Case_04946();
            Case_04947();
            Case_04948();
            Case_04949();
            Case_04950();
            Case_04951();
            Case_04952();
            Case_04953();
            Case_04954();
            Case_04955();
            Case_04956();
            Case_04957();
            Case_04958();
            Case_04959();
            Case_04960();
            Case_04961();
            Case_04962();
            Case_04963();
            Case_04964();
            Case_04965();
            Case_04966();
            Case_04967();
            Case_04968();
            Case_04969();
            Case_04970();
            Case_04971();
            Case_04972();
            Case_04973();
            Case_04974();
            Case_04975();
            Case_04976();
            Case_04977();
            Case_04978();
            Case_04979();
            Case_04980();
            Case_04981();
            Case_04982();
            Case_04983();
            Case_04984();
            Case_04985();
            Case_04986();
            Case_04987();
            Case_04988();
            Case_04989();
            Case_04990();
            Case_04991();
            Case_04992();
            Case_04993();
            Case_04994();
            Case_04995();
            Case_04996();
            Case_04997();
            Case_04998();
            Case_04999();
            Case_05000();
            Case_05001();
            Case_05002();
            Case_05003();
            Case_05004();
            Case_05005();
            Case_05006();
            Case_05007();
            Case_05008();
            Case_05009();
            Case_05010();
            Case_05011();
            Case_05012();
            Case_05013();
            Case_05014();
            Case_05015();
            Case_05016();
            Case_05017();
            Case_05018();
            Case_05019();
            Case_05020();
            Case_05021();
            Case_05022();
            Case_05023();
            Case_05024();
            Case_05025();
            Case_05026();
            Case_05027();
            Case_05028();
            Case_05029();
            Case_05030();
            Case_05031();
            Case_05032();
            Case_05033();
            Case_05034();
            Case_05035();
            Case_05036();
            Case_05037();
            Case_05038();
            Case_05039();
        }

        private static void Case_04860()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4860,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-12,42,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-6,46,8,3), new GeneratedEnemyUnit(6,-20,20,28,2), new GeneratedEnemyUnit(-18,-19,19,31,3), new GeneratedEnemyUnit(17,9,36,3,2), new GeneratedEnemyUnit(-3,-18,9,40,1), new GeneratedEnemyUnit(3,11,92,49,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "41d39bb2bde82d44f962aa743c6eaf446f365d2ed097e219a4514c2233e875b2");
        }

        private static void Case_04861()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4861,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-12,79,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-20,42,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-15,9,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,3,54,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-14,10,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-13,47,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-6,30,42,2), new GeneratedEnemyUnit(19,-8,6,9,4), new GeneratedEnemyUnit(-8,-10,75,46,1), new GeneratedEnemyUnit(1,-11,18,42,3), new GeneratedEnemyUnit(-17,3,54,3,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "fba027777c6f2f7410a77a7085c2e204f0a735b9214ed4edc5329d3d2d970111");
        }

        private static void Case_04862()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4862,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,14,72,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,17,5,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,20,50,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-17,5,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,7,43,5,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "73e25b58a453fdf0dbec4855fd7f2d222591dbed00da63a80d21b1f8a9f91738");
        }

        private static void Case_04863()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4863,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,6,58,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-14,49,7,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "123965a126a40482a03c8c28c5f640e8f15b9c50f658824b389440e293126450");
        }

        private static void Case_04864()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4864,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-3,9,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-20,83,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,11,21,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-20,15,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-2,40,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-14,43,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-19,35,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-10,20,13,2), new GeneratedEnemyUnit(-13,3,72,38,4), new GeneratedEnemyUnit(-11,-11,88,12,1), new GeneratedEnemyUnit(-7,-5,71,27,2), new GeneratedEnemyUnit(-5,1,16,18,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "6928b45b2126aaa7f886f39d61b1d2095b17d48764e747c16c0d3552213fc363");
        }

        private static void Case_04865()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4865,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-8,36,2,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "8c8c3628d29af157f1852d16e0248f873d81dbd047801a8d9d50d7525ffa3fc7");
        }

        private static void Case_04866()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4866,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,5,36,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,0,53,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-9,78,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-9,13,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-17,92,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-19,30,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,7,61,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,12,19,36,1), new GeneratedEnemyUnit(-3,17,60,9,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "4cd9ccf9a5d6595d119522136be517a4d43f371cd16e6bcbf1824c1656e54a97");
        }

        private static void Case_04867()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4867,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,9,33,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,16,43,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,3,31,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-20,59,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-20,96,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-4,56,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-13,60,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-1,34,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,3,40,9,1), new GeneratedEnemyUnit(8,10,63,22,3), new GeneratedEnemyUnit(0,-8,91,14,1), new GeneratedEnemyUnit(11,14,67,44,1), new GeneratedEnemyUnit(-15,-16,63,38,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "ceeda7d7ccf0ab0254e6f2a2606d6d30caae605af266a854e5338ceb8515c5a0");
        }

        private static void Case_04868()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4868,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,16,63,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-2,21,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-7,96,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-17,24,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,18,56,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,2,27,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-19,72,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,9,60,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-16,25,49,3), new GeneratedEnemyUnit(-4,13,10,50,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "da3318f37956836131ed2080ecb90b35db903be7e12f1eb854240e9cafd6315e");
        }

        private static void Case_04869()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4869,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-17,70,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,7,97,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-19,80,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-1,63,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-12,27,2,1), new GeneratedEnemyUnit(2,15,13,10,1), new GeneratedEnemyUnit(20,-7,35,47,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "e1c1156bfb9b2142e9d4d1814928f08eab7b47de5effaf0c6404d598ae37bcbb");
        }

        private static void Case_04870()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4870,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,8,93,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-18,76,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-18,26,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,5,87,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,0,98,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,10,16,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,16,80,16,1), new GeneratedEnemyUnit(9,4,60,10,4), new GeneratedEnemyUnit(-18,-2,55,29,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "8438043dec0bcaac4dfc43a8d7ad3a4cb76deaee3a6e5ae9dad1b6c6f93698c2");
        }

        private static void Case_04871()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4871,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,9,97,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,0,42,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,4,27,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-5,29,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,6,8,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-12,100,50,2), new GeneratedEnemyUnit(-19,17,76,44,1), new GeneratedEnemyUnit(-14,-19,86,48,1), new GeneratedEnemyUnit(20,-9,80,4,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "65736b5b1bb47c7c82153bcf1831c8a47abddd74bd301b6c4a9ec7f48a0d6709");
        }

        private static void Case_04872()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4872,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-15,80,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,17,56,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-4,80,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,9,73,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,17,37,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-11,29,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,6,78,4,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "77d9fc7cceffff1d46a1c61954eb2fcc4d5c0ce3b6ef55d127032c8dda23506a");
        }

        private static void Case_04873()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4873,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-8,95,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-2,21,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-17,64,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-20,64,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,5,94,31,3), new GeneratedEnemyUnit(18,6,45,9,2), new GeneratedEnemyUnit(0,16,6,5,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "4418e98346a174fb2acc54680a607105ae4c380c64247c2a79c59dfa7cb3a4fa");
        }

        private static void Case_04874()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4874,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,7,91,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,1,79,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-5,28,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,13,41,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,4,25,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,5,93,46,3), new GeneratedEnemyUnit(13,6,56,16,2), new GeneratedEnemyUnit(15,-13,56,36,2), new GeneratedEnemyUnit(-16,20,24,12,3), new GeneratedEnemyUnit(0,3,61,22,3), new GeneratedEnemyUnit(9,5,99,43,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "fe16bb686543d4f3df0cea6e195367af3214741711228c3c42920f6becb0b1af");
        }

        private static void Case_04875()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4875,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-9,24,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-7,72,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,1,60,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,14,14,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,7,19,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-18,99,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-18,24,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,1,15,16,1), new GeneratedEnemyUnit(-1,16,16,16,2), new GeneratedEnemyUnit(-3,-13,38,13,4), new GeneratedEnemyUnit(-4,5,77,8,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "85e05a548894d666e6a963e5a6556d577234822f7171bee60d4ae0c257b342a2");
        }

        private static void Case_04876()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4876,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,10,45,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-10,24,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-4,99,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,11,22,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,13,88,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-12,83,10,2), new GeneratedEnemyUnit(7,-17,24,38,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "633e246e63bc656c7561a7bf576c830e5e4d09323bddeea5eb6a08ccaedff3c7");
        }

        private static void Case_04877()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4877,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-12,17,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-19,99,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-12,76,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-18,44,47,1), new GeneratedEnemyUnit(15,-20,19,20,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a3e2ad7f47d00330266bcd32bc2dc22e5b3616abfcfe15b8a71d82fa0c750f56");
        }

        private static void Case_04878()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4878,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,17,88,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-16,87,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-17,56,34,2), new GeneratedEnemyUnit(19,-2,18,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "ea28ef86fe1b4baaab8f7366639c02ade96eb02f64b742534d2124f04438e9f2");
        }

        private static void Case_04879()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4879,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,1,58,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-4,65,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-19,38,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-14,32,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,14,26,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-12,51,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-20,43,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,10,81,46,4), new GeneratedEnemyUnit(-18,1,58,23,1), new GeneratedEnemyUnit(14,16,94,14,3), new GeneratedEnemyUnit(11,3,85,14,3), new GeneratedEnemyUnit(10,6,77,38,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "830aef11c11010f561f8976c2e07b19057dff581a75086a68326084744827a64");
        }

        private static void Case_04880()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4880,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-16,75,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,5,84,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-8,26,34,3), new GeneratedEnemyUnit(17,-15,19,18,4), new GeneratedEnemyUnit(-14,3,58,38,3), new GeneratedEnemyUnit(-5,17,17,41,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "9eb42ee2c3a741288697fb91996a1df286faa8a1dfdc0c301baf70c39005953d");
        }

        private static void Case_04881()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4881,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,2,9,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,3,25,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,5,42,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,5,91,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,20,58,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-2,59,35,2), new GeneratedEnemyUnit(-17,-19,12,15,2), new GeneratedEnemyUnit(9,-19,82,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "7be8d54b59e9684d64068306d6d445e37b8969218d5106f46272ddae367cf93c");
        }

        private static void Case_04882()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4882,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-4,68,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-10,73,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,12,66,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,19,19,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,8,57,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-9,54,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,20,57,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-4,10,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-17,100,9,3), new GeneratedEnemyUnit(4,-8,20,36,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "c46b8a44b92c752a0334f2ae008e8944917c0ce90745c6aaa76a91b41b8c095f");
        }

        private static void Case_04883()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4883,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-10,11,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,11,88,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-12,40,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,9,77,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,18,65,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-12,9,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-13,5,12,4), new GeneratedEnemyUnit(3,13,47,39,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "2ce5c443d14de9e73e5a3a574feff4570d3f11b447ab36a262a5e0ad18e51d55");
        }

        private static void Case_04884()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4884,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-4,68,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-1,63,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,12,17,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-15,9,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-8,90,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-4,84,12,3), new GeneratedEnemyUnit(-13,3,52,44,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "f15d5efb6529ca0f3b4897d573efbd0fe3c5bcbbce84a63de5d6ff95d10e8be5");
        }

        private static void Case_04885()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4885,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-13,94,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,18,73,10,3), new GeneratedEnemyUnit(19,-17,70,35,3), new GeneratedEnemyUnit(-12,-11,64,34,3), new GeneratedEnemyUnit(-5,2,65,24,3), new GeneratedEnemyUnit(15,19,11,6,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "9cc4d6f4941498584e4f00f43eb51a4246892c34c416781efafbfb84bcd06165");
        }

        private static void Case_04886()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4886,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,4,8,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-20,87,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-13,47,47,2), new GeneratedEnemyUnit(-20,-5,91,8,4), new GeneratedEnemyUnit(-17,-3,41,43,1), new GeneratedEnemyUnit(7,-9,95,24,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8beec956b75e4103c01a88d9ca9817a41c78420e54c2c53df66c755b9029c2e5");
        }

        private static void Case_04887()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4887,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-16,9,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,13,89,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-1,51,37,1), new GeneratedEnemyUnit(11,-20,68,14,3), new GeneratedEnemyUnit(9,-19,34,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "1e435547a5ab00b2e481d411a1022c38b336f01fc72741abab8fc4cefdf944fa");
        }

        private static void Case_04888()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4888,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-6,30,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,18,74,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-2,17,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,3,54,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,10,63,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-13,70,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-9,11,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,18,90,11,3), new GeneratedEnemyUnit(11,-8,56,9,4), new GeneratedEnemyUnit(15,-1,85,24,3), new GeneratedEnemyUnit(7,-7,44,48,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "15243aacca2b773d371b8b219e0c4ba9af294eb9fba94a966deefaadffc5cd9c");
        }

        private static void Case_04889()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4889,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,20,16,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-20,9,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,4,8,41,1), new GeneratedEnemyUnit(11,-9,61,21,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2132a10ca72bc38e444df0ba431be10bfe12fc123394235683403a01dd0b56b4");
        }

        private static void Case_04890()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4890,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-13,16,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-11,68,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-2,99,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-8,27,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-20,74,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,17,87,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-1,93,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,5,75,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,0,76,21,4), new GeneratedEnemyUnit(-10,-1,62,49,1), new GeneratedEnemyUnit(-15,0,29,9,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "e658ad5d3513fdfee973def1d201497b75cd4b73c711948fbfe688f0e3bdee6d");
        }

        private static void Case_04891()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4891,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-2,30,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,15,28,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-13,44,1,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "fb8ee06656153e7b6c3166d33e25d41458d70b132ccf0807c3059ea9955cf8bb");
        }

        private static void Case_04892()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4892,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-12,98,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,9,43,3,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "9c63c9823e038b49f703aa094d70b47a4cdd65c4d99b4fdcb74102f7e1b63ca5");
        }

        private static void Case_04893()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4893,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,0,71,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,3,77,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,16,51,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-20,26,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-5,57,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-9,45,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,16,83,35,1), new GeneratedEnemyUnit(-13,-1,14,15,3), new GeneratedEnemyUnit(-10,13,88,4,2), new GeneratedEnemyUnit(20,3,23,9,4), new GeneratedEnemyUnit(9,12,54,31,3), new GeneratedEnemyUnit(-11,14,44,42,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "15aa89bb97e5bb0ae035af9275c4e0f14d5d4394cbf2f7278e6cfaa9b246c000");
        }

        private static void Case_04894()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4894,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,9,66,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,1,57,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-6,81,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-16,32,5,4), new GeneratedEnemyUnit(5,-13,38,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "a1fcc1eb4973950a1ebdd13ba2667d5dfec4dff9595e043786234fe30a509ab2");
        }

        private static void Case_04895()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4895,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-6,84,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-9,94,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-7,40,21,3), new GeneratedEnemyUnit(-9,18,9,41,4), new GeneratedEnemyUnit(-15,10,68,7,4), new GeneratedEnemyUnit(3,-20,13,31,4), new GeneratedEnemyUnit(14,-16,94,10,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "f696beae2daf5a06b8df0e4cf5d6e4cdd6df78359c75971d925d97de271cd1bd");
        }

        private static void Case_04896()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4896,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,8,37,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-5,35,36,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "018375c7a702d5ed4cc4eb143a8fd43ad7ce5df2e8bc1823fc36be5b86d27547");
        }

        private static void Case_04897()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4897,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-9,17,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-4,67,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,12,7,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,2,70,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,16,66,31,1), new GeneratedEnemyUnit(4,-5,43,31,2), new GeneratedEnemyUnit(-14,-14,72,22,1), new GeneratedEnemyUnit(10,0,5,28,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "48f769cb4f40d508a276bfa1bce78be354a23bfbbf89212c6a29d4f06c38ccc9");
        }

        private static void Case_04898()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4898,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-7,59,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-5,18,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-19,36,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-4,44,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,6,44,49,3), new GeneratedEnemyUnit(-5,-19,46,39,4), new GeneratedEnemyUnit(5,-6,13,11,4), new GeneratedEnemyUnit(-6,-12,69,21,1), new GeneratedEnemyUnit(-9,-11,95,29,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "0ef0c37ff28371ed815078aa312638bd9d732660b1b35fbf3439de21725b64cc");
        }

        private static void Case_04899()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4899,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-9,62,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-11,38,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,14,65,16,1), new GeneratedEnemyUnit(-9,-12,58,30,1), new GeneratedEnemyUnit(-10,0,87,10,3), new GeneratedEnemyUnit(-10,-12,55,11,3), new GeneratedEnemyUnit(19,-8,69,14,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "d5ac86d5513ca3ce79a67658b420fbfdf24c611d8901c4ac8e2d7b61464fff73");
        }

        private static void Case_04900()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4900,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-7,45,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-17,31,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,16,71,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,8,91,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-16,6,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-13,81,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,18,79,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,7,80,34,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "4f99eda98b9c0cbd3fa3b6695c5b490569ae7d0cd5110d312d6610341000976f");
        }

        private static void Case_04901()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4901,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,19,58,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-10,36,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-7,45,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-14,92,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,2,100,43,4), new GeneratedEnemyUnit(6,12,93,6,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "fc933fcc951f4a1fe43a071e0e444e12b458b791bf7d591f072f146574fa82fd");
        }

        private static void Case_04902()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4902,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-19,66,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-20,42,11,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "19d3b6cf152780736c9c86e413ba7ba40eefa077f095f9df73dcedcae5148b82");
        }

        private static void Case_04903()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4903,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,5,71,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,4,39,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-10,85,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,20,35,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,4,84,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-7,74,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,17,78,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,11,77,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-16,93,32,3), new GeneratedEnemyUnit(-5,12,75,37,2), new GeneratedEnemyUnit(-20,8,67,44,2), new GeneratedEnemyUnit(-14,14,19,28,3), new GeneratedEnemyUnit(-19,-17,92,33,3), new GeneratedEnemyUnit(-17,2,28,28,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "58984660b142d2d891f1ab1f52db8cfff801b072caeb342db2e641d924b7aeea");
        }

        private static void Case_04904()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4904,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-6,15,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,2,85,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,8,61,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-9,38,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-8,86,8,1), new GeneratedEnemyUnit(-15,-1,18,19,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "49fd3056128d4948cb8c1425fc318f776d22a4867e18314a8d3a51378c762d92");
        }

        private static void Case_04905()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4905,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-8,65,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,9,34,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-17,91,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-18,71,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-13,92,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,0,30,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-1,27,35,4), new GeneratedEnemyUnit(14,10,60,23,4), new GeneratedEnemyUnit(-11,6,46,22,1), new GeneratedEnemyUnit(15,-6,44,3,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a43a4b9c6e8640b40a7a22548476fa67c03edd66bbdeb258dcaaf4ad7dd4ae49");
        }

        private static void Case_04906()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4906,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-8,34,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-14,69,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,16,75,17,2), new GeneratedEnemyUnit(-17,6,25,8,2), new GeneratedEnemyUnit(-4,-7,37,9,1), new GeneratedEnemyUnit(1,-15,81,26,1), new GeneratedEnemyUnit(-8,-20,17,49,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "ec7fb66aa3f5f3069fb4f21b894bc467766712c5f4e7c0a29ca54ff4b399992d");
        }

        private static void Case_04907()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4907,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,2,55,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-13,96,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-12,60,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,12,98,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-12,11,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,0,7,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-6,76,22,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "00280881c1c0898f22cb2ceb10abe1e59ff154761d15e6457e6782f75f4fb295");
        }

        private static void Case_04908()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4908,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-8,78,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,8,51,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,11,7,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-10,31,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-9,47,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,4,64,5,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "237db4e3faf2e99e6b6ba566cb486458950378e124f21e6da54c05e02f92f476");
        }

        private static void Case_04909()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4909,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,17,68,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,5,24,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-6,81,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-16,83,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,9,60,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-18,44,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-5,13,9,2), new GeneratedEnemyUnit(1,-6,18,24,4), new GeneratedEnemyUnit(-14,8,19,27,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "f9cc076772b2286eae29a297ac015649a305a84b55035f68cb2672ff474fef5e");
        }

        private static void Case_04910()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4910,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,3,40,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-20,88,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,7,70,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-11,64,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-4,48,6,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "650ae2cc6de8a2e241169c1fe6772ba20d12c27defdca20c4553cb2ad3f4dedf");
        }

        private static void Case_04911()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4911,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-1,62,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-12,63,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,13,71,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-7,39,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-1,27,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,13,75,5,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "925b0ba3250fc117e4f1812d5a2e1f6946c74819af607449411dfe6b07f68cc0");
        }

        private static void Case_04912()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4912,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-7,46,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,11,21,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-3,69,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,13,75,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,18,16,39,3), new GeneratedEnemyUnit(3,2,98,39,3), new GeneratedEnemyUnit(-16,19,47,23,1), new GeneratedEnemyUnit(-16,0,92,31,4), new GeneratedEnemyUnit(-20,18,89,48,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "cb930ad1b1ff61a6d0a045f680046b19d9283dd6a105e3a7636b3d6cba0eabc8");
        }

        private static void Case_04913()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4913,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,11,87,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,7,54,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,20,87,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,15,34,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-15,26,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,5,54,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,0,8,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,0,43,3,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "893b77c6b06645f4c39334dd1b613db52ddbe400700ed9cf93bf71d005d4d841");
        }

        private static void Case_04914()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4914,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-1,9,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,4,99,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,5,52,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,2,29,3,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "646751d501a982da06a49a22bd8facd059d3332cceae9af4624f585c4dc30fa3");
        }

        private static void Case_04915()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4915,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-6,34,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,3,71,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,14,89,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-6,74,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,1,87,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,0,94,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,14,19,40,2), new GeneratedEnemyUnit(-3,5,80,13,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e8cbb766a356378db00982c8411313ecdfcd38c8a02223a3d71ab3e8bba14870");
        }

        private static void Case_04916()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4916,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,0,22,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-4,33,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-9,43,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-1,60,15,4), new GeneratedEnemyUnit(2,-4,70,11,1), new GeneratedEnemyUnit(12,9,88,42,4), new GeneratedEnemyUnit(6,11,77,41,1), new GeneratedEnemyUnit(9,15,64,41,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "b608bbf6c70c56d6dbee5a175079b3df0252be3d80a34da73c9da4632acdffbe");
        }

        private static void Case_04917()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4917,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,20,41,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-14,81,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-2,83,48,1), new GeneratedEnemyUnit(-9,1,42,40,1), new GeneratedEnemyUnit(11,12,76,11,3), new GeneratedEnemyUnit(-17,9,80,43,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "8d0b43b5b55923f8a41c0880716486b1c97d69bfe3a3beb393fce27645796530");
        }

        private static void Case_04918()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4918,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-11,39,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,0,30,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,5,93,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-18,5,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,20,68,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,16,31,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-13,67,23,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "a716bd0c9700bf3b0d8924e523b279f623e518c20eecff99c7441cd514fe1342");
        }

        private static void Case_04919()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4919,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-17,24,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,6,85,44,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "02372d29ccaabb51e46c947a4a8229330ea21e2c05fdc68ab8d31556a2ab8ef2");
        }

        private static void Case_04920()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4920,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,15,21,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,9,49,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,19,15,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-2,81,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,12,86,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-14,47,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,6,14,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,11,86,44,4), new GeneratedEnemyUnit(-9,-15,55,26,2), new GeneratedEnemyUnit(8,10,62,45,1), new GeneratedEnemyUnit(7,-9,38,7,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "28422bbbcfbd883f7a6c43465972d97af78aa0cd18a617ca0a3be9ea7ad77a54");
        }

        private static void Case_04921()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4921,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-13,15,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,14,69,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,18,99,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-7,11,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,19,43,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,14,88,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-3,70,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-12,81,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,18,76,3,2), new GeneratedEnemyUnit(-8,-10,45,48,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "f1eb8d56e99ab3191eec9853e14d9dbe7ecfafccb0b32bd5b6fd48fb9e06d9c1");
        }

        private static void Case_04922()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4922,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,9,73,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-4,51,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-2,17,1,1), new GeneratedEnemyUnit(11,-2,51,37,4), new GeneratedEnemyUnit(-5,18,92,14,1), new GeneratedEnemyUnit(6,-15,22,33,1), new GeneratedEnemyUnit(4,5,72,2,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "b1952873d03e5cb20f9debd71db70886189679231d766dd7ebd7251498cdcba5");
        }

        private static void Case_04923()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4923,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-20,54,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,13,76,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-12,51,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-6,84,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,8,38,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,12,85,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,18,62,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,18,28,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,12,80,18,4), new GeneratedEnemyUnit(-8,1,28,9,1), new GeneratedEnemyUnit(-2,14,91,41,2), new GeneratedEnemyUnit(3,-5,55,6,1), new GeneratedEnemyUnit(9,-11,80,28,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "7e01eb246661ee992023f4cec755db74fe3e4a97a8e30972345a0a208620351e");
        }

        private static void Case_04924()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4924,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-17,26,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,18,84,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-13,67,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-7,24,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-14,74,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-3,81,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-8,34,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-14,75,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,8,42,43,4), new GeneratedEnemyUnit(20,-4,66,2,2), new GeneratedEnemyUnit(13,-18,44,13,3), new GeneratedEnemyUnit(-2,-20,36,19,2), new GeneratedEnemyUnit(5,-6,53,36,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "5c69631a79293ab73fd49d2e0dac0c95b3e58a28edca4d63b18bc4a09d9605ba");
        }

        private static void Case_04925()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4925,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,16,79,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-6,47,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-5,33,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,20,32,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-18,83,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,11,11,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-6,8,42,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "cd4aff861f9a250cc98dc2bcffbdd65d7e9694841b71c926620293b768f1fa05");
        }

        private static void Case_04926()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4926,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,17,75,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,18,23,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,18,39,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,1,67,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-18,7,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,16,23,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,18,83,47,3), new GeneratedEnemyUnit(9,12,75,15,2), new GeneratedEnemyUnit(-15,-16,28,46,4), new GeneratedEnemyUnit(-12,1,48,49,1), new GeneratedEnemyUnit(9,3,22,40,2), new GeneratedEnemyUnit(-17,-17,69,24,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "e0dfadf02ca904f58c539f7306c804134ff6baa80c9c05d589e620b805593298");
        }

        private static void Case_04927()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4927,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-20,26,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-18,23,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-8,49,2,4), new GeneratedEnemyUnit(10,-4,64,9,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "560f3f38d2e964ccfea8c9ab8eb7775da5e24b9707368c5b4f1daf837bfa2ac0");
        }

        private static void Case_04928()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4928,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,0,30,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,11,77,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-1,83,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-20,9,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-3,71,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-7,77,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,8,60,10,3), new GeneratedEnemyUnit(-14,13,61,39,3), new GeneratedEnemyUnit(-14,15,25,47,3), new GeneratedEnemyUnit(15,-19,15,45,1), new GeneratedEnemyUnit(-2,20,56,38,4), new GeneratedEnemyUnit(-8,19,42,15,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "626bf3fa1851a8fbe2bea96c06a2d41ae59ef0d2316816df7c2f2eaaaad3c02d");
        }

        private static void Case_04929()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4929,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,20,58,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,15,9,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,19,57,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,16,66,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,5,76,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-13,14,6,4), new GeneratedEnemyUnit(19,-15,26,35,3), new GeneratedEnemyUnit(11,2,16,22,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "ff99e1b8867436c231c39647abc721e50084a6dc0ffb523abd988d37f437514e");
        }

        private static void Case_04930()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4930,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,16,42,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-13,92,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,12,39,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,17,77,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-20,86,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,5,10,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,11,43,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-13,40,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-3,73,35,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "c259cb226d94f9c86fa5188baeb90742d4975d2f2fb0f1ea4488a596e1597fe8");
        }

        private static void Case_04931()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4931,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-4,79,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,10,69,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-11,52,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,0,17,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-18,27,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,8,82,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-9,56,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,4,8,44,1), new GeneratedEnemyUnit(11,-3,97,11,3), new GeneratedEnemyUnit(-10,4,85,6,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "04143424635dc2aa8ed2f0e561c695ac883244469d1ce90ccf3d3da05f564f77");
        }

        private static void Case_04932()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4932,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-4,64,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-13,56,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,19,38,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,3,94,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,6,10,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-5,88,19,4), new GeneratedEnemyUnit(10,10,10,16,4), new GeneratedEnemyUnit(8,1,22,36,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "b3d301ebf6b526b05a66d54b72397e4b18a3f06f4a386bf585d2165864a027a7");
        }

        private static void Case_04933()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4933,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-10,18,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-17,20,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-20,50,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-9,96,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,8,43,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,4,20,26,3), new GeneratedEnemyUnit(2,-2,6,12,4), new GeneratedEnemyUnit(9,8,92,10,1), new GeneratedEnemyUnit(16,9,84,32,2), new GeneratedEnemyUnit(8,14,86,44,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "9e692b2db858d7f6b4755420162e7adbf5a30dea21381e7c52db5119d7509d6f");
        }

        private static void Case_04934()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4934,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,4,53,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-1,47,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-18,94,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,14,39,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,13,57,43,2), new GeneratedEnemyUnit(-5,12,17,20,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "42cd979bade8601da14bdb9c762e5df23b9136e9eb3dae3e3786ec725303a900");
        }

        private static void Case_04935()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4935,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-18,31,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,9,100,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-12,8,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,17,99,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-1,86,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,18,13,23,2), new GeneratedEnemyUnit(-8,7,64,39,3), new GeneratedEnemyUnit(1,17,76,29,4), new GeneratedEnemyUnit(-3,-5,55,47,1), new GeneratedEnemyUnit(9,-5,33,38,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "72ffcc0e431917bcf8f77349d19a58f666da78e3364a136a9b3ea58119179198");
        }

        private static void Case_04936()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4936,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,1,90,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-7,33,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-19,43,31,2), new GeneratedEnemyUnit(-15,6,35,40,2), new GeneratedEnemyUnit(7,-2,20,41,4), new GeneratedEnemyUnit(0,-3,50,13,2), new GeneratedEnemyUnit(7,-16,6,19,4), new GeneratedEnemyUnit(-5,12,68,47,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "c228e5a266f8db8f0eac6b6752343dd32d1397710df02a6cdec3350ba72f4801");
        }

        private static void Case_04937()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4937,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-4,21,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,4,29,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-12,98,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,6,28,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,14,26,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-19,67,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,0,48,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,12,58,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "93b77e9a9be90577f85ea0744d05c97c6d4c5b63ee9ce58280ee6ebd03177ba1");
        }

        private static void Case_04938()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4938,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,7,82,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-3,84,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,18,23,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,6,92,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-13,92,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,10,89,33,1), new GeneratedEnemyUnit(-9,2,40,12,4), new GeneratedEnemyUnit(6,15,61,36,1), new GeneratedEnemyUnit(6,-20,53,14,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "4fb168cf785eed855530abdf34f0fe12d1d3d93b049527034ef96b57fbbc6638");
        }

        private static void Case_04939()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4939,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,20,76,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,3,39,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-14,85,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-15,45,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-14,24,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-19,67,42,2), new GeneratedEnemyUnit(-3,1,68,10,4), new GeneratedEnemyUnit(2,-20,77,18,4), new GeneratedEnemyUnit(18,-10,41,48,1), new GeneratedEnemyUnit(16,-8,55,35,2), new GeneratedEnemyUnit(-13,-11,46,41,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "e76d7a5b255633436f98a02d5d996ac4b20c8b21b7c6d9268383b7ab651f474f");
        }

        private static void Case_04940()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4940,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-10,86,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,12,32,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,3,67,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,1,96,38,3), new GeneratedEnemyUnit(-17,7,70,13,4), new GeneratedEnemyUnit(11,-17,22,4,4), new GeneratedEnemyUnit(-20,4,47,11,4), new GeneratedEnemyUnit(1,-19,31,33,3), new GeneratedEnemyUnit(-18,-13,5,47,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 35,
                stableHash: "b3e84032d4fec2a2ad6442ca97fbf9358bb40bd113b851adf747cca9ca43a2dd");
        }

        private static void Case_04941()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4941,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-7,71,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-2,95,50,2), new GeneratedEnemyUnit(-11,8,34,8,1), new GeneratedEnemyUnit(-8,4,85,40,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "9b4946d57b30865a828cfe6747e43add1f35c76f2d297e0001e2e232f98d85d8");
        }

        private static void Case_04942()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4942,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-3,51,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-17,45,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,20,36,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-13,90,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-16,16,6,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "c54a6dc192db8a1433d4cd1e08ce8e37b6e4aece40c0c394ab3803b3f2a30929");
        }

        private static void Case_04943()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4943,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,2,38,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,16,42,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,20,7,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-14,94,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-7,20,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-14,50,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-11,19,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,17,12,30,3), new GeneratedEnemyUnit(-11,-8,97,45,2), new GeneratedEnemyUnit(-9,-2,62,9,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "207dad58fe62fd693c76d3a5a038ba6885e9ac0e29a8ee71008bfd7a75ce9ae2");
        }

        private static void Case_04944()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4944,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-4,50,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-13,60,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-17,50,6,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "f6357d1fdaa0d4f97b0173132a40018cf524643c325626d48a639dcc4751707e");
        }

        private static void Case_04945()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4945,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,10,28,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-9,96,43,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "23bc740078b609b7a6ef4e286f3510c02c5bb8b17726036e5df9806320229259");
        }

        private static void Case_04946()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4946,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-2,86,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,8,67,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-18,61,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-14,92,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,12,62,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,11,13,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,1,42,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,5,59,49,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "f88e0830eec999d86f632a853027b1ad41e0d4575268ff5e9fd4d5ea18919b49");
        }

        private static void Case_04947()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4947,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,7,78,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,16,50,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-10,89,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-1,30,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,8,18,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-9,65,33,3), new GeneratedEnemyUnit(-7,-14,93,43,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "7c4f5bdb7f5650fa15ad5faba89bf9bc375720a49ef05b8a2cc04f80f0a71a0a");
        }

        private static void Case_04948()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4948,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-18,24,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,16,69,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-8,32,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-10,54,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,11,37,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-9,67,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-19,52,9,3), new GeneratedEnemyUnit(18,12,8,22,3), new GeneratedEnemyUnit(-10,-7,57,39,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "52fbdff9ad2ca17067e825a74151b9e0752a53748a27397abafd4ae9e069100e");
        }

        private static void Case_04949()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4949,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-1,21,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,0,38,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-12,13,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,4,11,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-13,72,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,18,46,46,4), new GeneratedEnemyUnit(2,-5,27,27,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "641182f0b1d23d07dee6f8b9d4f3c8dbf5e9702bc2ccd734885557dbe5312c79");
        }

        private static void Case_04950()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4950,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,10,61,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-19,30,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-19,67,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-13,51,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,20,38,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,4,76,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-8,80,45,1), new GeneratedEnemyUnit(-4,0,95,26,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "5829f50df28cf5ce99defd3423e0286fb8f1756c4ff4304bf379b81d18eb637f");
        }

        private static void Case_04951()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4951,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,14,30,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-15,84,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,2,45,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,4,98,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,2,29,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,8,41,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,0,60,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-5,65,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,20,37,19,2), new GeneratedEnemyUnit(-15,9,33,48,3), new GeneratedEnemyUnit(-4,8,81,28,2), new GeneratedEnemyUnit(-1,-20,24,43,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "71e0c4bd3f12b84fa03b5c2d2fcb8b3b2f49562c64259504c54f97568368bc59");
        }

        private static void Case_04952()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4952,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,3,87,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,5,31,12,4), new GeneratedEnemyUnit(-1,-11,89,20,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "4115c77653e319368fe1da56b3e072d5c318bb5a12b5a51976682dba23d8e691");
        }

        private static void Case_04953()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4953,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-13,57,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-14,28,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,18,46,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,16,44,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-11,95,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-8,31,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-10,92,39,4), new GeneratedEnemyUnit(-4,14,94,17,1), new GeneratedEnemyUnit(4,-1,41,27,3), new GeneratedEnemyUnit(-7,15,50,5,2), new GeneratedEnemyUnit(-11,-2,28,49,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "0e77278125d9e160de4ebd74c558de2f0dc12b14b40bdf3ca170e33421213c73");
        }

        private static void Case_04954()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4954,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,9,95,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,11,72,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,17,98,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-5,34,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-19,65,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,12,75,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-10,48,24,2), new GeneratedEnemyUnit(-7,9,51,38,2), new GeneratedEnemyUnit(-6,15,52,30,2), new GeneratedEnemyUnit(-2,5,69,32,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "52630367f5756a3fe336c54c383ad0302e90fa66054856aa2cdcf083a39f7b4f");
        }

        private static void Case_04955()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4955,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-6,53,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,6,45,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-19,18,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,11,81,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-8,64,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-6,60,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,14,80,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-6,67,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-12,88,15,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "77f3dbe9b442e9300307e2279830a8cac762a5c72ff9e756cc40840b11f06561");
        }

        private static void Case_04956()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4956,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-6,38,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,0,76,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-12,12,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,16,26,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,15,56,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-16,83,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-5,41,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,10,68,28,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f89c49744226f96dcb2793e97da6c9abacd03e245d4891263c104ed85b03670b");
        }

        private static void Case_04957()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4957,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,20,74,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,13,61,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,14,45,17,2), new GeneratedEnemyUnit(-16,-7,75,27,3), new GeneratedEnemyUnit(7,-8,68,14,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "098ef8710275106989f352bb1cc9593dcc60fd93c9ef40f79a99f32f52cec602");
        }

        private static void Case_04958()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4958,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-3,93,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-14,55,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,13,38,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-18,30,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-7,80,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-19,14,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,10,18,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-19,25,28,4), new GeneratedEnemyUnit(13,-17,99,25,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "9702cb497018ec8ea977fec818ac69274bea070625c22858fc68c510161070a0");
        }

        private static void Case_04959()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4959,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-8,86,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-20,9,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-20,6,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,20,38,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,4,24,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-1,42,38,3), new GeneratedEnemyUnit(-10,9,20,43,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "10357b4d89e7571f0abba9aaa33dcc9634eeef1f1d2c0a29573f36da19280aee");
        }

        private static void Case_04960()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4960,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-18,38,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-17,68,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,1,84,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-18,69,38,3), new GeneratedEnemyUnit(-3,20,87,6,3), new GeneratedEnemyUnit(3,-9,58,35,2), new GeneratedEnemyUnit(11,-4,35,25,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "14e7e3bcda924743b8385b34f4f8c80d075d5300d276012b26cc868e89e6ec65");
        }

        private static void Case_04961()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4961,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,15,76,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,13,80,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,15,29,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,5,52,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,4,19,48,3), new GeneratedEnemyUnit(-6,-20,14,46,3), new GeneratedEnemyUnit(-1,-2,89,36,4), new GeneratedEnemyUnit(-2,-11,80,34,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "a45a515999b0b883468c9e9bcc3080997385e5a2dbef71ac886383561acb0904");
        }

        private static void Case_04962()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4962,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-6,31,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-6,53,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-16,32,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,15,41,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,19,92,44,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ef89efebd536876f96e93e9278d7b60cbe6e4a97232005774eb0de9b95c52f98");
        }

        private static void Case_04963()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4963,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-14,42,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-3,8,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,12,85,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,19,25,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-9,50,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,20,61,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-1,89,24,2), new GeneratedEnemyUnit(-6,14,12,12,3), new GeneratedEnemyUnit(19,-3,86,2,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "88c779c0dd8cfc90a9f6b4d1055d400a1a050a4cdb0bad37c3ba2a50a1df13e9");
        }

        private static void Case_04964()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4964,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-17,34,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-11,48,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-4,78,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,12,57,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-15,54,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,4,93,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-14,34,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,7,68,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,9,28,12,4), new GeneratedEnemyUnit(17,20,99,9,3), new GeneratedEnemyUnit(17,20,74,41,2), new GeneratedEnemyUnit(-11,-5,91,26,4), new GeneratedEnemyUnit(-6,-20,8,26,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "2aacdb8ce1136f5674e3ec8e264a280aa7395458284685b3c39c3554a9d8aa46");
        }

        private static void Case_04965()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4965,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-15,19,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,1,45,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,8,12,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-3,76,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,17,10,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-15,46,34,4), new GeneratedEnemyUnit(-10,-4,90,5,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "5ab6f31b7b37780a95ac19a629cc5cbcb98e5522f8e15f676326b2b0549d3a07");
        }

        private static void Case_04966()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4966,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,13,74,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,15,30,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-7,90,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-6,30,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-15,54,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-5,62,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,9,53,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,17,73,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,7,95,30,3), new GeneratedEnemyUnit(-9,2,43,19,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "a368bf82afe44b486b462375b419ff148724f73f0f96be665e6b2eb29e017b25");
        }

        private static void Case_04967()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4967,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-3,73,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,0,51,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,17,65,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,3,62,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,18,73,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-3,12,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,6,50,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-8,26,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,8,38,17,1), new GeneratedEnemyUnit(19,-1,97,12,3), new GeneratedEnemyUnit(9,-15,35,17,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "7c3dda4471fb60c4911895ba486375d144783081bac048806ff0943c09e343b1");
        }

        private static void Case_04968()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4968,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,13,78,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-17,62,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,4,39,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-3,66,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-18,35,17,1), new GeneratedEnemyUnit(19,14,12,50,4), new GeneratedEnemyUnit(-5,12,29,6,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "2d693d515fb9f5893b1a90edf034d3330a62d4b2e0d31f6a03bd16c43f043544");
        }

        private static void Case_04969()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4969,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,1,34,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-2,76,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,13,14,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-4,24,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,10,34,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-1,73,42,2), new GeneratedEnemyUnit(17,-17,70,6,4), new GeneratedEnemyUnit(11,-11,78,15,1), new GeneratedEnemyUnit(-19,-5,23,43,3), new GeneratedEnemyUnit(0,5,16,37,3), new GeneratedEnemyUnit(-20,-11,96,49,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "e738c44ee9fbea5f6d38552e79d6deb7bc687b5ec3b28ad1457f733523748519");
        }

        private static void Case_04970()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4970,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,18,62,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-17,86,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,6,69,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,19,39,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,19,64,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-4,87,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-11,64,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-7,75,39,4), new GeneratedEnemyUnit(-5,8,82,34,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "bb650b00ae2dd3911116fd471880269a94b641def184f2ce9d71e0f5c98a3401");
        }

        private static void Case_04971()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4971,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,9,49,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,10,90,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,17,79,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-18,50,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-2,87,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,11,38,25,4), new GeneratedEnemyUnit(-8,-17,16,28,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "47f82dda168f441fdbb1b9142a97ab023c149255ea16a1c484d7a5d9f0c5eb2d");
        }

        private static void Case_04972()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4972,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,13,74,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-19,77,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,16,83,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,17,91,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-15,74,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,8,8,8,2), new GeneratedEnemyUnit(11,17,79,39,3), new GeneratedEnemyUnit(-6,16,43,17,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "a3849106f106ea270b366d3e8f8441e0441e943605a368593db372c32b2fbe2a");
        }

        private static void Case_04973()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4973,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-11,70,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-16,28,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,7,17,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,18,56,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-10,78,43,3), new GeneratedEnemyUnit(15,10,59,39,4), new GeneratedEnemyUnit(-7,8,14,12,3), new GeneratedEnemyUnit(16,-14,92,9,3), new GeneratedEnemyUnit(5,-4,55,3,4), new GeneratedEnemyUnit(13,9,100,17,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "5158b1d5bfbb7dd94dec0a8110b1a0c2aafa761f7b2a78cdb9984d2bc19793c8");
        }

        private static void Case_04974()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4974,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,17,48,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,9,22,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,8,67,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,17,48,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-8,33,24,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "c3a5d9d3b90702e59c8d7d1ad41c0bf9fe44468bc36fabe6655d5a8f970347df");
        }

        private static void Case_04975()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4975,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,0,10,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-17,75,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,1,66,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,19,34,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-14,76,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-18,38,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,11,35,21,4), new GeneratedEnemyUnit(1,7,83,18,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "452178fdd6bcca6656348cd04f5feb0ba34d6b109f4ef432dd1f41249f2eb36b");
        }

        private static void Case_04976()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4976,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,20,76,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,16,38,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,8,78,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,8,88,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-17,42,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,14,42,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-7,32,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,17,89,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,2,50,32,1), new GeneratedEnemyUnit(-16,-8,30,25,4), new GeneratedEnemyUnit(-14,6,48,26,4), new GeneratedEnemyUnit(7,-19,6,11,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "c04e5fa83436112a9a462a4b08549e6072215c1f2260bad97efc2b1e7025c928");
        }

        private static void Case_04977()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4977,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,4,54,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,17,78,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-9,14,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,5,6,30,3), new GeneratedEnemyUnit(12,18,62,13,2), new GeneratedEnemyUnit(0,-5,61,16,2), new GeneratedEnemyUnit(15,14,6,20,2), new GeneratedEnemyUnit(-4,-7,87,36,1), new GeneratedEnemyUnit(-3,18,78,35,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "a539711bf599dda29f7f71fd531682c3e80ebfe61cb640c1a1916b472efb9ba0");
        }

        private static void Case_04978()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4978,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,19,99,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,4,31,45,2), new GeneratedEnemyUnit(17,-15,55,28,3), new GeneratedEnemyUnit(-2,10,42,33,3), new GeneratedEnemyUnit(-18,2,82,44,3), new GeneratedEnemyUnit(-15,-17,40,8,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "36a87db2a27d1dd3a33c7d24351c731a5f7ac799dee969abdd46da0809973fef");
        }

        private static void Case_04979()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4979,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,18,50,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,18,55,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-5,41,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,17,23,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-15,48,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-5,97,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-20,84,30,1), new GeneratedEnemyUnit(8,-4,49,13,1), new GeneratedEnemyUnit(16,2,32,23,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 30,
                stableHash: "165e6057403652e0c016e1be8a932ce60e510808b8f42b738594e09ed6b2bbfa");
        }

        private static void Case_04980()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4980,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-9,93,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-4,65,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-15,58,4,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "515f08abe4cbd3563edf2fb9b9b99554efb40b3ca19cc1bdf6f83b3c523736d7");
        }

        private static void Case_04981()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4981,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,10,83,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-5,89,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-11,47,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-10,18,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,4,84,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,16,24,11,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b7a5983a9d3366a8b7235d59d2c3c1c7ff2606e9d6d6107ffc0055e05a6b11d6");
        }

        private static void Case_04982()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4982,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-13,92,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-12,35,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,18,92,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-7,92,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,13,19,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,15,20,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,3,53,6,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "83b6e9c966e1b5461a5491fa07d177f50ee5efe866e2b7eab362e3028b6f3402");
        }

        private static void Case_04983()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4983,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,1,20,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,13,57,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-17,48,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,12,19,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,16,70,37,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "c92afd382b11d27b9241e1187c99b3ea0c4254c843801e0e06b3b55c4f19725a");
        }

        private static void Case_04984()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4984,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,9,22,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,5,32,50,1), new GeneratedEnemyUnit(7,-18,63,38,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "15221f7cdf8e9b5a0dfe2844ae6113e029a2f9a23a4caf1e4ca663035caee908");
        }

        private static void Case_04985()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4985,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-2,13,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-9,8,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-12,32,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,10,58,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-15,91,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-12,45,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,6,12,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,7,74,47,2), new GeneratedEnemyUnit(-8,2,82,48,3), new GeneratedEnemyUnit(18,0,47,48,2), new GeneratedEnemyUnit(-3,-4,13,29,4), new GeneratedEnemyUnit(13,14,69,33,4), new GeneratedEnemyUnit(-9,1,63,8,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "a65cf597d92272ab39b604706880e43c4f5340ea5172a5c7797f26a4db90bd81");
        }

        private static void Case_04986()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4986,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,20,60,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,4,54,11,1), new GeneratedEnemyUnit(1,-9,83,7,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "04d3ea8c3c81f39a170a6b9a5a75fc598f89c58f0a1f5e54c59e27ec788fb211");
        }

        private static void Case_04987()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4987,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,9,94,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,11,32,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,2,83,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,16,7,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-15,28,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-15,10,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-3,77,47,2), new GeneratedEnemyUnit(-11,-3,40,14,3), new GeneratedEnemyUnit(12,16,26,17,4), new GeneratedEnemyUnit(20,-12,52,47,1), new GeneratedEnemyUnit(-13,-15,62,32,3), new GeneratedEnemyUnit(8,-7,75,20,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "b272bd843ca91f51783e97dbc55abafcc78e0f55a448a28cfd7f9e004e598fae");
        }

        private static void Case_04988()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4988,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,18,75,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-15,77,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,5,58,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,1,49,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-13,9,37,1), new GeneratedEnemyUnit(3,7,78,7,3), new GeneratedEnemyUnit(4,-4,39,37,4), new GeneratedEnemyUnit(1,-16,51,37,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "0fe89ceca82f119f478aa4447a8011b1782595740cf38d2cd287620c8f77d9cf");
        }

        private static void Case_04989()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4989,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,9,18,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,16,11,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-8,93,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-5,16,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,11,21,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,1,29,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,3,73,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-10,13,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-18,42,8,3), new GeneratedEnemyUnit(14,-12,69,32,4), new GeneratedEnemyUnit(19,16,42,14,1), new GeneratedEnemyUnit(-1,2,63,33,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "9206a6a909534786603561f2ecea3ba9fe42eb7cbbc71b58565130c4d2025ceb");
        }

        private static void Case_04990()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4990,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-12,88,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,18,91,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,1,81,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-15,80,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-4,21,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-15,41,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-15,37,36,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "a6e52eb84f2be19f508d6a20b7ae823c4eb53bd9620d29cff318c893994bfb60");
        }

        private static void Case_04991()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4991,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-19,15,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-10,69,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,18,81,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,0,39,1,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "da81da0e0e208071b8af1d6c03f682a86bad73f5989ecc8b8957582b4fd976aa");
        }

        private static void Case_04992()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4992,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,12,55,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,18,72,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,20,12,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,10,63,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-1,64,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,5,38,1,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "b60bff2c3ee7e162c4908e40ac5b44393d3581a41aaef60487f6d858141f2510");
        }

        private static void Case_04993()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4993,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-6,22,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,4,91,47,2), new GeneratedEnemyUnit(10,4,98,32,3), new GeneratedEnemyUnit(11,3,25,18,4), new GeneratedEnemyUnit(-9,-13,80,47,4), new GeneratedEnemyUnit(13,3,42,11,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "5587114c6751e3eeea222ee8e897e8edfe65a458d77f01b409d01f4bdc9c9991");
        }

        private static void Case_04994()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4994,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,6,31,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,6,38,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-15,74,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,9,13,6,4), new GeneratedEnemyUnit(19,8,35,23,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "c2faeaa5d319e8b62cf787d3871d0463cb8ba7b068824c0377d8183e98a1bd5a");
        }

        private static void Case_04995()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4995,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-17,98,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,13,10,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,1,95,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-17,71,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-15,58,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-10,47,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-2,46,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,10,9,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-3,15,31,2), new GeneratedEnemyUnit(7,-18,58,24,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f551482a0cbd17c2ba7eb41d1427184e51fff98f27a0e5e6ea9828c9a74786ae");
        }

        private static void Case_04996()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4996,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,7,57,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-8,64,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-13,88,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-13,18,3,1), new GeneratedEnemyUnit(18,16,43,37,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "41076629df33d9e3e55202c8479901f7f57bc91b65c6f08aeeced12a5d24dabe");
        }

        private static void Case_04997()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4997,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-9,39,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-4,26,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,14,44,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,9,13,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-1,36,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-16,71,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-13,15,7,1), new GeneratedEnemyUnit(7,5,32,5,2), new GeneratedEnemyUnit(19,-16,7,9,2), new GeneratedEnemyUnit(15,7,73,33,4), new GeneratedEnemyUnit(-4,-18,5,42,1), new GeneratedEnemyUnit(7,2,48,22,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "96243756b5a45012f0e7879740718cd0cc1dca091df38d029d678ae1e93c899a");
        }

        private static void Case_04998()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4998,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,0,70,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,10,24,20,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "d66d02f43879c362d5fded96c1d614511a3221b9ff0e75b52e6960a53dc4a549");
        }

        private static void Case_04999()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4999,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,6,68,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,7,17,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-8,75,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,10,5,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-9,32,1,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "8a17c7eb666df0b0b934fa998ca47791bb04416ab4e7b9c8b9a265bfb061dab2");
        }

        private static void Case_05000()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5000,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,1,64,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,12,97,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,0,68,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-18,90,19,1), new GeneratedEnemyUnit(20,-3,54,36,2), new GeneratedEnemyUnit(-8,-19,51,18,1), new GeneratedEnemyUnit(-7,-10,62,7,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "7d35d0ce5eee6b3efdba3ccea1654732aed6cbeff2bad6058c5aa6e95b18ecf9");
        }

        private static void Case_05001()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5001,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,1,99,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-10,61,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-17,64,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,16,48,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-10,70,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,18,6,7,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "e77555311095d97f660285a286a44aa5a33dca38b68494a41ab20821edb54839");
        }

        private static void Case_05002()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5002,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,12,98,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,11,10,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-1,55,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-4,15,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,10,11,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-5,5,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-3,82,3,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3e847a86e33d2712ed9ce66939326c64b8ec25c781cf7b28efe3fa09f2760cf5");
        }

        private static void Case_05003()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5003,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,3,77,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-18,95,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,11,28,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,12,51,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-12,76,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,16,53,19,3), new GeneratedEnemyUnit(-8,-14,29,6,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "84f948e3f7c7f0670d736e9570f272591077423d672358252943e66ce8b07a9b");
        }

        private static void Case_05004()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5004,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-2,74,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,7,67,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-13,37,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,1,37,3,4), new GeneratedEnemyUnit(2,-17,13,29,2), new GeneratedEnemyUnit(-13,-11,79,39,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "5c791e268e5e2648e76955471d8d12d8ea1aa84bc30ca2c7a9d024f84060ebfb");
        }

        private static void Case_05005()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5005,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,17,30,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-3,21,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,2,58,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,16,60,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,17,41,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-2,61,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-13,74,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,10,77,17,1), new GeneratedEnemyUnit(14,-16,14,27,3), new GeneratedEnemyUnit(-7,13,87,25,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "dbb292420517709b6086ae7c48dd65ad3fc3ca0ca98fa228e6932becb7e7ee3c");
        }

        private static void Case_05006()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5006,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,2,67,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,5,19,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-20,46,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-2,98,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,17,13,4,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "19825afd6f4d23e62805d6d6b7472c7d684df9d47081d8b013a5aa87ca4bc5c9");
        }

        private static void Case_05007()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5007,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-19,100,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-4,40,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,7,26,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-19,57,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-2,70,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-12,66,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,10,57,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-14,71,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,9,36,2,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "629e3e3ed0e9e25619029e09270434d6506f9142a0cced927d0e70ac564fb043");
        }

        private static void Case_05008()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5008,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-3,67,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,12,56,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,14,41,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,4,28,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,14,85,12,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "494a3ef54846cee91410bb39c4b6f3ce32531f5401e3c900a670cecdfa78c4fc");
        }

        private static void Case_05009()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5009,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,7,19,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-16,21,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,18,15,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-11,76,34,2), new GeneratedEnemyUnit(15,-11,29,31,1), new GeneratedEnemyUnit(-4,11,44,40,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 30,
                stableHash: "e4add0dff88f1b24480daa4e6b1fad69fd9a87a6c559d4084c18271d3aef965a");
        }

        private static void Case_05010()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5010,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-18,39,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,9,23,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,10,36,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-17,11,4,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "d80f4d5c2309e6e601625890aaf8691310546545f236881281f57487c6030385");
        }

        private static void Case_05011()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5011,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,11,79,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-19,91,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,7,53,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-19,39,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-10,73,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,11,38,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,10,95,38,2), new GeneratedEnemyUnit(4,11,67,29,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "62161127f26169fac09e773ff063f20fb74e38d8b1c61973261965326c8e0522");
        }

        private static void Case_05012()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5012,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,7,15,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,5,34,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-7,16,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,6,82,11,1), new GeneratedEnemyUnit(-1,-14,22,31,3), new GeneratedEnemyUnit(-17,-19,49,25,1), new GeneratedEnemyUnit(16,7,45,49,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "ce46365e3c9d01cc280e309c36eac8dbd4882ee06ec3c595ce7b91668cfb56c4");
        }

        private static void Case_05013()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5013,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-10,21,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,1,31,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-14,40,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,5,75,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,16,100,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-1,43,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,9,50,8,1), new GeneratedEnemyUnit(7,-8,13,47,1), new GeneratedEnemyUnit(-18,3,64,29,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "e78a42bde81a54516f7970de3690bc5103cfdfa5e2f728c3bd837489319512e9");
        }

        private static void Case_05014()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5014,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,17,22,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-18,54,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-3,62,20,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0fdc1d49cb9a196d861df0bf25102b81be8c34e19a9083708ce98b04b18483b4");
        }

        private static void Case_05015()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5015,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-20,29,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-8,54,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-16,50,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,6,34,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,18,34,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-7,48,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,20,72,43,1), new GeneratedEnemyUnit(-1,8,22,1,2), new GeneratedEnemyUnit(13,-4,91,28,3), new GeneratedEnemyUnit(2,-17,93,50,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "4655e847e47d5b487fd60198f74c159c46ac100d201bb2bee18c5e6f0b2cdf36");
        }

        private static void Case_05016()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5016,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,16,63,1,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "bbdbefa20d290f4f6daffbbc15c32bf41b7aecf895288f74c3ae81bb505c61a9");
        }

        private static void Case_05017()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5017,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,18,65,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,5,32,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-3,50,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-12,79,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-14,41,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,17,54,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-19,97,43,3), new GeneratedEnemyUnit(-2,5,70,45,2), new GeneratedEnemyUnit(1,-6,81,29,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "2dde55e06cb68b46b95834a09c9871d559bb7b798c7182075696230ea197e620");
        }

        private static void Case_05018()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5018,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-20,11,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-10,23,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,6,80,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,15,68,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-8,86,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-4,45,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-13,40,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,16,60,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,13,54,32,4), new GeneratedEnemyUnit(15,4,88,41,4), new GeneratedEnemyUnit(2,9,58,21,1), new GeneratedEnemyUnit(-20,-17,31,12,3), new GeneratedEnemyUnit(-11,-18,47,38,2), new GeneratedEnemyUnit(-9,0,60,44,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "a964299bebaa8a2ee98eee118373fc358abf59cf76e2c3aac12b7214ac4878e6");
        }

        private static void Case_05019()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5019,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-20,69,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,9,68,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-2,34,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,7,8,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,12,25,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,8,17,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-12,97,35,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "ea451ec4b7fdcee33eb3c54680632f2a343c7ac56db1d5035e6b039f7bfea74f");
        }

        private static void Case_05020()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5020,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,14,72,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-6,54,16,3), new GeneratedEnemyUnit(-3,-13,75,27,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "1900843be6ce8bad80224da69ce94c87b6946ac31f4b832bc92311ea2bdd9995");
        }

        private static void Case_05021()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5021,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-14,82,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,3,75,5,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "97d33056b1bcfca81a2fe5ac867737a7318195aab24db2f235d0c440207f5011");
        }

        private static void Case_05022()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5022,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-11,61,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-8,68,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,11,39,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-5,94,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-17,97,37,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "7f38d1ef9581b739ca0c263453cbf7045fc445da71f1cce3513bc8db5ba254f7");
        }

        private static void Case_05023()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5023,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-4,84,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-16,96,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,16,38,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,18,87,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,3,78,1,4), new GeneratedEnemyUnit(-18,13,65,38,2), new GeneratedEnemyUnit(4,-18,45,28,4), new GeneratedEnemyUnit(7,-19,96,13,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "54e9dd644ba58dfc59eeedb83448d24ab97fca0808196066fd825464e7743692");
        }

        private static void Case_05024()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5024,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,7,90,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-14,73,1,1), new GeneratedEnemyUnit(4,-17,98,34,2), new GeneratedEnemyUnit(-12,9,44,21,1), new GeneratedEnemyUnit(3,-14,100,19,4), new GeneratedEnemyUnit(3,-15,59,42,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "6c8eff41889f29d67d1ce0c10ee138bba18139dfcda4d3eb4071a66fd6fd247c");
        }

        private static void Case_05025()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5025,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,5,56,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,6,64,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,8,48,49,2), new GeneratedEnemyUnit(9,-18,27,9,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "59a92533d40477655d56e6c1dd31ceb3d9718f83fdaca4c78bff1dfb724f87ea");
        }

        private static void Case_05026()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5026,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,10,78,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-19,31,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,17,25,16,4), new GeneratedEnemyUnit(9,15,13,7,1), new GeneratedEnemyUnit(-1,9,42,21,1), new GeneratedEnemyUnit(-10,-13,26,24,4), new GeneratedEnemyUnit(17,0,9,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "3a057d799f157d65b7ca0ee059abed6664b318aba13ceb3f6b87eada6156dcc8");
        }

        private static void Case_05027()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5027,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-1,50,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,8,78,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-15,84,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,12,96,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-13,66,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-18,17,6,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "609abc5707fd0571bf0c35019c7af09e901f96a1424a0cf3e6cedc17a10b563d");
        }

        private static void Case_05028()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5028,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,13,92,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-19,32,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-13,61,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,13,61,5,4), new GeneratedEnemyUnit(17,-3,79,17,3), new GeneratedEnemyUnit(-4,4,49,38,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "8b11032cb2c3d1b4cbf1408dfb20b0cc3bfaa62fe412c48de91c4954c1b2476b");
        }

        private static void Case_05029()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5029,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,19,72,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-6,23,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,7,30,6,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3ba1bdd3ea33dab3ef20a734b95793e7c56fc1de6811c6b6c0f16b6e91c4081b");
        }

        private static void Case_05030()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5030,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-9,92,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-12,61,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-18,36,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,2,49,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,11,50,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-4,66,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,14,12,39,1), new GeneratedEnemyUnit(-16,9,37,32,4), new GeneratedEnemyUnit(4,-10,14,16,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "1b58b80d798d3dcd5d45eaf3442417444fe981d42a65bd157953dd5158ca381a");
        }

        private static void Case_05031()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5031,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-2,56,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,9,91,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-13,59,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,1,94,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,14,79,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-6,70,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,7,27,39,2), new GeneratedEnemyUnit(-11,-7,35,3,4), new GeneratedEnemyUnit(20,-5,44,20,3), new GeneratedEnemyUnit(2,4,61,4,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "be6dc45c3574ce0e96e97d255aed76e2fb998c0450117dd70a31e818057513b3");
        }

        private static void Case_05032()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5032,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,5,80,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-7,44,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-16,49,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,5,56,26,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "31798d6aab1854c7f4b0324940518618dca9df6142735c9fa4ab6a0654d945cb");
        }

        private static void Case_05033()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5033,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,1,70,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,6,96,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-20,59,4,4), new GeneratedEnemyUnit(14,20,75,26,1), new GeneratedEnemyUnit(-6,14,18,33,2), new GeneratedEnemyUnit(-5,-6,96,10,2), new GeneratedEnemyUnit(-20,-1,53,16,4), new GeneratedEnemyUnit(17,10,100,34,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "a8a7a656ea7b962a919287b0b2e71464675fa88dc792b3b04d2ad0eea0529d8b");
        }

        private static void Case_05034()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5034,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-15,52,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,4,84,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-18,10,11,3), new GeneratedEnemyUnit(-13,-14,27,7,1), new GeneratedEnemyUnit(-19,-2,72,49,1), new GeneratedEnemyUnit(-14,-14,51,48,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "df15bc9b418e5c5756fc0a2c2d57f2dc54211293f934288d5113d5de1d203c3b");
        }

        private static void Case_05035()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5035,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,4,40,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-18,87,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,5,54,18,1), new GeneratedEnemyUnit(13,20,14,29,4), new GeneratedEnemyUnit(6,-9,54,9,2), new GeneratedEnemyUnit(-5,18,91,28,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "a8e03e7197c2bc1aa225ebf1e0ea17b81c9b5c28d85f56e1a3394d6304bb09f6");
        }

        private static void Case_05036()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5036,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,2,75,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,9,45,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,18,34,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-5,27,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,4,59,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-12,12,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,2,95,50,2), new GeneratedEnemyUnit(15,-6,63,25,1), new GeneratedEnemyUnit(-19,13,58,42,4), new GeneratedEnemyUnit(-7,4,40,48,3), new GeneratedEnemyUnit(-4,-14,26,4,4), new GeneratedEnemyUnit(12,10,53,2,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5011c734aff2a4750f054cb5f93a2fa77513d0b3933403a11612bc83b0188c9e");
        }

        private static void Case_05037()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5037,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-14,57,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,3,97,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-18,78,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-20,91,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,9,89,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-17,57,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-20,59,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,2,17,18,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "f77d39139cc53ae13ee5305ef9f49d83d04da484cc9e6803da68aa8d9feafb45");
        }

        private static void Case_05038()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5038,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,3,78,6,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "845043ea59e27813064494baf6a27f4bca71609fd5c70aa46263bdec2778e133");
        }

        private static void Case_05039()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5039,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-4,85,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,0,31,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-7,18,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-4,11,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-4,92,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,12,81,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-6,40,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,18,32,4,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "d12bcc9b8a9de7abe13078408781d42d45e8c98e0388a35a9659e3bbf397b653");
        }

    }
}
