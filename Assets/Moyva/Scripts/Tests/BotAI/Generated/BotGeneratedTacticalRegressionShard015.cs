using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard015
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_02700();
            Case_02701();
            Case_02702();
            Case_02703();
            Case_02704();
            Case_02705();
            Case_02706();
            Case_02707();
            Case_02708();
            Case_02709();
            Case_02710();
            Case_02711();
            Case_02712();
            Case_02713();
            Case_02714();
            Case_02715();
            Case_02716();
            Case_02717();
            Case_02718();
            Case_02719();
            Case_02720();
            Case_02721();
            Case_02722();
            Case_02723();
            Case_02724();
            Case_02725();
            Case_02726();
            Case_02727();
            Case_02728();
            Case_02729();
            Case_02730();
            Case_02731();
            Case_02732();
            Case_02733();
            Case_02734();
            Case_02735();
            Case_02736();
            Case_02737();
            Case_02738();
            Case_02739();
            Case_02740();
            Case_02741();
            Case_02742();
            Case_02743();
            Case_02744();
            Case_02745();
            Case_02746();
            Case_02747();
            Case_02748();
            Case_02749();
            Case_02750();
            Case_02751();
            Case_02752();
            Case_02753();
            Case_02754();
            Case_02755();
            Case_02756();
            Case_02757();
            Case_02758();
            Case_02759();
            Case_02760();
            Case_02761();
            Case_02762();
            Case_02763();
            Case_02764();
            Case_02765();
            Case_02766();
            Case_02767();
            Case_02768();
            Case_02769();
            Case_02770();
            Case_02771();
            Case_02772();
            Case_02773();
            Case_02774();
            Case_02775();
            Case_02776();
            Case_02777();
            Case_02778();
            Case_02779();
            Case_02780();
            Case_02781();
            Case_02782();
            Case_02783();
            Case_02784();
            Case_02785();
            Case_02786();
            Case_02787();
            Case_02788();
            Case_02789();
            Case_02790();
            Case_02791();
            Case_02792();
            Case_02793();
            Case_02794();
            Case_02795();
            Case_02796();
            Case_02797();
            Case_02798();
            Case_02799();
            Case_02800();
            Case_02801();
            Case_02802();
            Case_02803();
            Case_02804();
            Case_02805();
            Case_02806();
            Case_02807();
            Case_02808();
            Case_02809();
            Case_02810();
            Case_02811();
            Case_02812();
            Case_02813();
            Case_02814();
            Case_02815();
            Case_02816();
            Case_02817();
            Case_02818();
            Case_02819();
            Case_02820();
            Case_02821();
            Case_02822();
            Case_02823();
            Case_02824();
            Case_02825();
            Case_02826();
            Case_02827();
            Case_02828();
            Case_02829();
            Case_02830();
            Case_02831();
            Case_02832();
            Case_02833();
            Case_02834();
            Case_02835();
            Case_02836();
            Case_02837();
            Case_02838();
            Case_02839();
            Case_02840();
            Case_02841();
            Case_02842();
            Case_02843();
            Case_02844();
            Case_02845();
            Case_02846();
            Case_02847();
            Case_02848();
            Case_02849();
            Case_02850();
            Case_02851();
            Case_02852();
            Case_02853();
            Case_02854();
            Case_02855();
            Case_02856();
            Case_02857();
            Case_02858();
            Case_02859();
            Case_02860();
            Case_02861();
            Case_02862();
            Case_02863();
            Case_02864();
            Case_02865();
            Case_02866();
            Case_02867();
            Case_02868();
            Case_02869();
            Case_02870();
            Case_02871();
            Case_02872();
            Case_02873();
            Case_02874();
            Case_02875();
            Case_02876();
            Case_02877();
            Case_02878();
            Case_02879();
        }

        private static void Case_02700()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2700,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-11,73,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-6,99,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-18,75,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,7,96,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,10,83,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,20,58,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-17,84,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,2,58,50,1), new GeneratedEnemyUnit(1,17,66,33,3), new GeneratedEnemyUnit(11,-19,71,33,3), new GeneratedEnemyUnit(-8,-4,92,46,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a95504ed35aabdf52957f4f8fe52430d60126933e7005aa886b0b4450b709af3");
        }

        private static void Case_02701()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2701,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,11,19,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,13,80,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-13,76,18,4), new GeneratedEnemyUnit(13,-1,56,15,1), new GeneratedEnemyUnit(-1,-5,9,36,2), new GeneratedEnemyUnit(7,-5,56,37,3), new GeneratedEnemyUnit(-6,-2,32,20,2), new GeneratedEnemyUnit(17,14,13,34,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "fa58b89fe1dce229ccd9c78d43d199e973f5f56fbe9e4e7f1faf57739448ec7a");
        }

        private static void Case_02702()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2702,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,18,74,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,9,63,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-10,50,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-3,77,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,15,87,16,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "8c3f6b1adc5c9cb089be5c918daf1a3ab12132bb6780b4d38ec16b6c696398ee");
        }

        private static void Case_02703()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2703,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-16,44,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-9,31,20,2), new GeneratedEnemyUnit(2,-20,13,12,2), new GeneratedEnemyUnit(14,-11,65,13,3), new GeneratedEnemyUnit(-10,-4,68,32,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "a88db292acb46bed1d4d6ae7c6debfb55228271b31454b566e6181f83f52d147");
        }

        private static void Case_02704()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2704,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,3,26,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,1,55,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,10,66,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-9,57,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,13,66,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,5,37,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,15,76,5,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "069d6bec60d4a5f8f4b60cefa10ac2e0b8479915f27d05f903e222a8a4b531d4");
        }

        private static void Case_02705()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2705,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,7,29,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,17,16,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-15,7,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-6,55,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-12,9,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,19,25,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-3,59,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,3,23,38,4), new GeneratedEnemyUnit(-18,-18,89,28,4), new GeneratedEnemyUnit(0,-16,20,35,1), new GeneratedEnemyUnit(-16,-2,85,32,3), new GeneratedEnemyUnit(8,-18,93,4,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "7ad4f4a00905bf2a1da29da4968494b5188a19025bf8efa3e3580ebd250ea337");
        }

        private static void Case_02706()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2706,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-2,6,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-10,93,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,8,70,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,11,7,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-1,37,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-2,71,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,19,69,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-7,92,29,4), new GeneratedEnemyUnit(-12,-16,56,22,4), new GeneratedEnemyUnit(6,6,47,22,3), new GeneratedEnemyUnit(2,-6,23,1,4), new GeneratedEnemyUnit(-10,-11,98,47,2), new GeneratedEnemyUnit(-15,-8,17,34,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "2c8a038dd28a9c194fa4c4c049605d8bce5dda9f1e01263de8a8ac4b8e5e08d4");
        }

        private static void Case_02707()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2707,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-10,87,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-9,79,26,4), new GeneratedEnemyUnit(0,1,94,45,4), new GeneratedEnemyUnit(12,-6,87,16,4), new GeneratedEnemyUnit(-13,-3,24,37,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "6f4add6ff69b3541b8d5368edae3e20e7961eaec6553b29dde6ff97db40f93c6");
        }

        private static void Case_02708()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2708,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,2,47,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "f76cb1b3eff9c21ce409c988318c6ef212b6825a37c4085376dde25cfc72f988");
        }

        private static void Case_02709()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2709,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,18,51,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-15,98,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-9,10,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-16,33,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,4,14,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-1,25,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,12,76,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-1,17,24,1), new GeneratedEnemyUnit(-15,19,99,20,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "78f846e118cf1e2c87bde48ca9df65bad53643ff949914b27869de3441a0afd4");
        }

        private static void Case_02710()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2710,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,3,61,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-15,14,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,6,49,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-1,9,5,3), new GeneratedEnemyUnit(-1,4,68,20,1), new GeneratedEnemyUnit(15,13,58,36,2), new GeneratedEnemyUnit(-10,-2,75,4,2), new GeneratedEnemyUnit(-7,-15,35,20,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "5c5a09927dfb084d4ec1dc3da992df2ee9d269e55b99233d3d9d8fd654bd0112");
        }

        private static void Case_02711()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2711,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,10,74,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,2,43,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,9,54,37,2), new GeneratedEnemyUnit(-10,-19,44,28,2), new GeneratedEnemyUnit(-12,12,68,21,1), new GeneratedEnemyUnit(19,3,13,50,3), new GeneratedEnemyUnit(7,-13,63,49,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "cfbd3793e80cd01368c056c1de0b32c475f8b9172435855b8347a2f8020e8182");
        }

        private static void Case_02712()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2712,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,11,87,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-11,78,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-18,89,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,6,35,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-17,97,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-20,82,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,3,42,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-18,91,43,3), new GeneratedEnemyUnit(-15,-16,37,3,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "89ed95f0685db4040a1a8fc0130b4022164eb1bb471e08e003ccb1aae51d694c");
        }

        private static void Case_02713()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2713,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,11,96,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,9,11,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,2,84,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-3,73,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-5,76,7,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "dd143bce49bef2fc3db8a7e85cac61f2531bcb34098e93871f9e900895fcbd6f");
        }

        private static void Case_02714()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2714,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,5,91,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,14,7,50,1), new GeneratedEnemyUnit(13,-20,69,27,1), new GeneratedEnemyUnit(5,-15,75,43,4), new GeneratedEnemyUnit(13,0,100,41,1), new GeneratedEnemyUnit(1,-5,64,25,3), new GeneratedEnemyUnit(3,1,52,2,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "632a09c3f0cf1bfd7682586d5034f2cf7caeebe82ed0b8e98d4a215d30bdae10");
        }

        private static void Case_02715()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2715,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,11,81,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-19,51,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,5,83,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,2,56,1,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "df29b192bd6c010d3771584aa00fb2f96c2074ab9961fa3aa00ce14f81c93c09");
        }

        private static void Case_02716()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2716,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-20,87,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,11,40,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-14,32,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,12,93,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,0,89,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-14,53,2,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "1e64fb1980ff49371e730bddaaf2e24b1da8b4eb8fe0766fd7b4fd73616641de");
        }

        private static void Case_02717()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2717,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,14,42,6,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "dbfa60cd4f1a9b0c3964b04dc37fb98b0dfc632d40543d512aeaeef73db3a28e");
        }

        private static void Case_02718()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2718,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-19,18,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-17,100,37,2), new GeneratedEnemyUnit(-15,20,66,6,2), new GeneratedEnemyUnit(11,-5,38,38,4), new GeneratedEnemyUnit(-16,-14,47,44,1), new GeneratedEnemyUnit(-13,18,58,43,2), new GeneratedEnemyUnit(4,-9,42,18,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "5f4ffdc0e779a9a945c16a90f04c9a95dff016b256101d0e2c725deaebb1e52e");
        }

        private static void Case_02719()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2719,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-17,83,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,20,21,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,19,18,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-5,12,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-14,41,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-8,20,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,7,12,11,2), new GeneratedEnemyUnit(-19,-15,94,3,4), new GeneratedEnemyUnit(-5,4,88,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "4630c995967cd2ded13e9aadbfb6746e967461af769e31cbf6fb3fa152e209cf");
        }

        private static void Case_02720()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2720,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-7,33,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,5,64,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-2,38,32,2), new GeneratedEnemyUnit(15,-4,73,14,1), new GeneratedEnemyUnit(3,13,85,37,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "17697be8bf349ef2ba668a33e87c84b82f21e26c39d34e921cc51fd4c6ee437c");
        }

        private static void Case_02721()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2721,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-4,46,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-12,91,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,12,43,14,2), new GeneratedEnemyUnit(-18,1,60,46,4), new GeneratedEnemyUnit(-11,18,80,27,2), new GeneratedEnemyUnit(-8,19,9,12,1), new GeneratedEnemyUnit(3,7,70,27,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "df0c50c2e6b8baac4023848a6dab04479efe1ff9070c482af6cf5d8c437607fb");
        }

        private static void Case_02722()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2722,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-16,96,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,20,67,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,11,56,29,3), new GeneratedEnemyUnit(1,18,58,45,1), new GeneratedEnemyUnit(19,16,62,3,2), new GeneratedEnemyUnit(-13,16,80,4,4), new GeneratedEnemyUnit(-3,15,20,44,3), new GeneratedEnemyUnit(9,11,60,15,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "d9dae7a8501254e40f4f4284d6d4e341719502d9b641401fcdf40ca44c4ca39b");
        }

        private static void Case_02723()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2723,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-20,34,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-19,74,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,8,31,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,8,20,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,2,14,2,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3ae1ff9883608193ba749385d81f981a9c50569d30466d5db817b707f6a2aa07");
        }

        private static void Case_02724()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2724,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,20,55,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-5,10,11,3), new GeneratedEnemyUnit(16,-5,72,14,1), new GeneratedEnemyUnit(-2,17,21,23,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "07936b87ad26bd8a9570cc5f7dea09832b2793e94ec9cfc07f333aff1412abb6");
        }

        private static void Case_02725()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2725,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,1,11,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,5,36,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-16,55,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-6,6,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-10,98,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-11,12,39,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "9288281513098cd479a2b35fee8a89d7ed19a4cfa0bbabb18afb9fecc47300b8");
        }

        private static void Case_02726()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2726,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,4,40,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-3,59,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,1,24,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-1,91,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-5,41,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-15,49,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,4,86,18,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "cc1e3d8d5b50f8afbc66d993a0aa40455110a4b57415c10af7e68db796ac1073");
        }

        private static void Case_02727()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2727,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-10,76,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-2,77,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,0,9,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-19,17,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-17,67,43,1), new GeneratedEnemyUnit(-18,-11,31,1,4), new GeneratedEnemyUnit(1,16,23,47,3), new GeneratedEnemyUnit(-14,-17,31,9,1), new GeneratedEnemyUnit(-17,-9,97,39,2), new GeneratedEnemyUnit(9,-19,87,21,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "306770b5dceb97209be2698542dcc0ef027cfaa2123881aa5dc33d87a1bf5db0");
        }

        private static void Case_02728()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2728,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-16,54,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-7,8,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,7,28,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-20,35,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-4,76,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,8,35,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-14,55,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-7,14,5,3), new GeneratedEnemyUnit(19,20,79,21,3), new GeneratedEnemyUnit(7,14,16,47,3), new GeneratedEnemyUnit(-4,7,48,12,1), new GeneratedEnemyUnit(15,-18,19,46,3), new GeneratedEnemyUnit(3,2,92,22,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "614d1861c7ee88baec6e8b3358b3aab8271871860f67c8bd065ea630dab6f564");
        }

        private static void Case_02729()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2729,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,5,58,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-3,87,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,8,22,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-14,72,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,2,27,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,13,68,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-7,54,4,3), new GeneratedEnemyUnit(17,-2,24,35,4), new GeneratedEnemyUnit(14,18,61,38,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "4bf9c4b8be4f557b3d7b109996a7d4d291a5b6cb8b6940a7449e96b42f6b45c4");
        }

        private static void Case_02730()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2730,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,13,39,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-16,48,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-18,38,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,7,59,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-17,78,30,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "6d76cc6a2a5cad86904db77d6bc7a34eccd6d5fe0de2b5b2f49985fb3f127f6a");
        }

        private static void Case_02731()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2731,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-14,66,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,20,8,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-20,61,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,1,89,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,0,64,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-16,52,32,2), new GeneratedEnemyUnit(-11,16,58,21,3), new GeneratedEnemyUnit(12,-10,89,26,1), new GeneratedEnemyUnit(-2,-18,48,39,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "332a80f61bf8fa73e1d4d4fa162631785e179394edb1842a3bca6af748c98852");
        }

        private static void Case_02732()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2732,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,9,19,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-1,27,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-10,53,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,9,57,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-16,73,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-12,32,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-15,89,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-14,92,17,3), new GeneratedEnemyUnit(-12,-3,33,22,3), new GeneratedEnemyUnit(4,17,45,13,1), new GeneratedEnemyUnit(9,-7,6,14,3), new GeneratedEnemyUnit(-3,9,96,34,3), new GeneratedEnemyUnit(12,-3,54,42,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "fff38962ef8e94eae6c298a3dc142843b81685c4b5610116437a173badf888cd");
        }

        private static void Case_02733()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2733,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,0,59,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,14,89,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,20,99,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-8,29,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,13,29,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-18,79,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,4,71,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,13,43,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-1,7,23,1), new GeneratedEnemyUnit(-5,15,96,25,1), new GeneratedEnemyUnit(5,10,78,26,3), new GeneratedEnemyUnit(15,15,49,25,4), new GeneratedEnemyUnit(-13,-18,61,10,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "e7b201ca2074a17623dfcc76ae5a52e3eac010a0482e210c46b595c044a05bcc");
        }

        private static void Case_02734()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2734,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-10,45,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,4,15,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,2,32,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-13,30,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-11,24,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-7,66,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,17,80,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,20,31,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,18,35,17,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "64e147474004a77923be05cfbc8ff51183128aa16e04d83ad58f852787cced44");
        }

        private static void Case_02735()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2735,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,2,76,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,11,12,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-3,15,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-12,67,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-2,9,1,3), new GeneratedEnemyUnit(-10,-17,72,8,2), new GeneratedEnemyUnit(-16,14,96,9,1), new GeneratedEnemyUnit(-6,-2,38,28,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "36249e510b0c99873765e5310b3f88b8d464824e36f4efe4d0a164bc038d6884");
        }

        private static void Case_02736()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2736,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,9,9,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-19,88,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-14,50,6,3), new GeneratedEnemyUnit(10,18,15,45,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "1a3c1cfe158725296101e40e2e57e2f3946a47d632087f8b3970b689e3bba529");
        }

        private static void Case_02737()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2737,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-18,17,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,10,51,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,13,72,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-16,90,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,1,11,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,1,37,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-3,67,5,4), new GeneratedEnemyUnit(-14,-6,35,5,1), new GeneratedEnemyUnit(-11,-6,59,5,4), new GeneratedEnemyUnit(-7,-2,6,45,3), new GeneratedEnemyUnit(2,-5,44,1,1), new GeneratedEnemyUnit(-3,-3,66,39,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "574ae9cd176d75faaf7b7d50f21853b014b84eb86b2b7e114a32fdd7a636926c");
        }

        private static void Case_02738()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2738,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-17,60,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-6,91,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-20,45,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-2,34,25,4), new GeneratedEnemyUnit(-20,-4,76,35,4), new GeneratedEnemyUnit(-18,-19,88,25,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "263e266d715568f65295fc97151f62b918d98e8124b41a2391d3d13966ae96f9");
        }

        private static void Case_02739()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2739,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-17,41,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-9,28,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,13,93,48,3), new GeneratedEnemyUnit(-3,-18,89,7,4), new GeneratedEnemyUnit(-18,-17,80,37,3), new GeneratedEnemyUnit(-14,7,26,12,1), new GeneratedEnemyUnit(-16,-9,45,23,1), new GeneratedEnemyUnit(10,-8,49,19,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "552c4b097e1afdcf0418d96effe44fd130201092e78a55322a23e276c3f7eca6");
        }

        private static void Case_02740()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2740,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-17,49,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-10,5,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,9,47,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,6,11,6,2), new GeneratedEnemyUnit(10,-19,70,6,2), new GeneratedEnemyUnit(-7,2,39,11,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "d7ae69be705d236bcd3d5d0460e56c25ff706c2a5887bd91bd52a1bb13e42c0e");
        }

        private static void Case_02741()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2741,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,20,90,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,12,61,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,1,20,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,18,28,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,0,12,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,3,69,47,4), new GeneratedEnemyUnit(6,-20,21,1,3), new GeneratedEnemyUnit(-8,-12,27,8,3), new GeneratedEnemyUnit(-18,-3,32,13,2), new GeneratedEnemyUnit(-16,-2,17,35,1), new GeneratedEnemyUnit(-2,-6,11,48,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "1c8eab0f0ad77c2eacd33b871ef8a634efd58cb082da3d9e6dbf77bb803e63be");
        }

        private static void Case_02742()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2742,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,11,15,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,16,59,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,4,84,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-11,33,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,15,20,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-12,52,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-1,68,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,18,54,5,3), new GeneratedEnemyUnit(1,-7,33,3,4), new GeneratedEnemyUnit(1,-20,80,47,1), new GeneratedEnemyUnit(-13,-7,87,40,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "4f0552e5e20597b8111d4ad569d35ed4ebd1ca867e59a940244da881990d989f");
        }

        private static void Case_02743()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2743,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-20,9,2,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "27ee1a841631820d30d63b6b2683b03f11b98e6c9c68914dfd41868d3116c3b6");
        }

        private static void Case_02744()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2744,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-9,46,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,10,82,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-11,94,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,7,23,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,17,92,25,4), new GeneratedEnemyUnit(5,-15,48,9,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "e00480afed44c063989f98b05b7a9f97a3ba405ea74f65992f53c02c734973f1");
        }

        private static void Case_02745()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2745,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,17,14,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-19,10,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,7,7,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,4,21,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,10,20,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,19,81,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,11,66,34,4), new GeneratedEnemyUnit(1,-13,35,14,1), new GeneratedEnemyUnit(2,-2,17,44,1), new GeneratedEnemyUnit(7,5,78,17,4), new GeneratedEnemyUnit(1,-8,90,21,4), new GeneratedEnemyUnit(3,-14,60,11,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "600a5415f27cd4d34bebad7b16d12a119eda59061b51558e0911016b5f9591e8");
        }

        private static void Case_02746()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2746,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-8,59,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-19,58,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,6,100,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-4,44,38,4), new GeneratedEnemyUnit(-1,1,60,16,1), new GeneratedEnemyUnit(17,4,73,12,4), new GeneratedEnemyUnit(17,-16,60,7,1), new GeneratedEnemyUnit(2,13,13,38,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "1cbec494e115e5867e0423cc53b95f689cf975974d806e2e3938087be2693ad6");
        }

        private static void Case_02747()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2747,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-14,82,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-11,35,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,7,5,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-7,42,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-12,28,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-19,67,5,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ec32220daf7ebb3be41cde3061c7ca659416aad3972bdf6b5e204ad38ea92557");
        }

        private static void Case_02748()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2748,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-8,21,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-2,62,22,1), new GeneratedEnemyUnit(15,-6,29,50,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "8559aa0d419cd115eb2e6fe388dc7f3b4bd66a0039214de1672b27bc17d8f863");
        }

        private static void Case_02749()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2749,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,12,51,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,13,85,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,20,5,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,13,20,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-8,79,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,19,57,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,7,36,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-3,25,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,13,49,5,1), new GeneratedEnemyUnit(-8,16,13,32,2), new GeneratedEnemyUnit(20,5,58,29,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "1ed1c3081fc02614b80a6ebf19f66034349f8c16bdf5945e78dfb07d4c97085c");
        }

        private static void Case_02750()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2750,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,3,87,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-6,42,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-12,99,35,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "456654b6427bfdfc286c38602d220672cfd6f8651cfe5a6f0565d4bcb4be5586");
        }

        private static void Case_02751()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2751,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-4,50,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-2,26,6,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "7f0b6c952a7289fd6d7d66212cef3abd9dae3583ef741c4183e02bc1d42c621c");
        }

        private static void Case_02752()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2752,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,17,21,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-16,57,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,17,86,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,18,45,42,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e1adaa8a11f54cba112237c58bb3afde7f92eef4aeb64ee6405d89eb92ab8a94");
        }

        private static void Case_02753()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2753,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-14,92,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,4,75,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,9,77,23,2), new GeneratedEnemyUnit(-3,-14,36,48,3), new GeneratedEnemyUnit(3,-2,69,35,1), new GeneratedEnemyUnit(-10,-1,16,49,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "803759997c778819366bb8a5b3ab60e65d2d9d4e7e5e664db8b4042e406a72a0");
        }

        private static void Case_02754()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2754,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,1,5,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-2,38,3,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "2643196d696f8ff75c92519f79d403b6342af315dbddb78ecd6a2e8c823e64a9");
        }

        private static void Case_02755()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2755,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-6,100,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-12,60,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,1,19,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-8,98,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-9,11,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,2,82,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,12,19,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,10,86,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,5,17,12,4), new GeneratedEnemyUnit(-1,-7,82,37,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "7a9dd82b4a4a7558962012417cae02370d4747d85392e681150941664cff2db4");
        }

        private static void Case_02756()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2756,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,19,37,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-7,7,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,2,30,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,6,73,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-14,5,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-5,58,27,2), new GeneratedEnemyUnit(14,-18,23,43,3), new GeneratedEnemyUnit(-19,2,54,13,2), new GeneratedEnemyUnit(0,6,26,34,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "a521b2811b759ca2cc97c252ac9d3ac8f559420684a282ca34bdf1fc37264331");
        }

        private static void Case_02757()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2757,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,10,65,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-2,95,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,7,80,7,1), new GeneratedEnemyUnit(16,20,73,47,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "eaf4f5d78dd94b26c48d7f84e2904f846a3a82a2ab6f404ed1bfff78ed9a5594");
        }

        private static void Case_02758()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2758,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-16,55,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,20,17,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-14,50,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-1,95,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-12,89,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-5,83,12,1), new GeneratedEnemyUnit(-18,10,94,21,2), new GeneratedEnemyUnit(20,8,75,10,2), new GeneratedEnemyUnit(-16,13,59,28,2), new GeneratedEnemyUnit(-1,-14,77,15,3), new GeneratedEnemyUnit(-3,-15,38,33,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "d55cf4b489dce1dc94f35700f201edacdd089aeb1188e133a8049ba44e4d1965");
        }

        private static void Case_02759()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2759,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,8,37,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,14,74,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-14,36,4,1), new GeneratedEnemyUnit(19,3,93,6,2), new GeneratedEnemyUnit(-12,6,23,18,1), new GeneratedEnemyUnit(18,-4,74,35,3), new GeneratedEnemyUnit(15,7,26,3,1), new GeneratedEnemyUnit(8,-14,18,16,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "0fbfa53c3d50e03dea7f9fc84ab29c741a5e7b3de3092d10443526817f37e983");
        }

        private static void Case_02760()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2760,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-13,87,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-11,57,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-10,64,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,8,54,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,20,87,1,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "424b114f92d3de4c0225b02f0d4e8893e2ec8d628c18cc6b52fce21ff812609e");
        }

        private static void Case_02761()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2761,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,5,37,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,2,85,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-5,86,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-13,37,1,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "fae054b0f1f54652b4d1d5d006aaa690f616538e73211235617bbf210d3ee381");
        }

        private static void Case_02762()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2762,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,20,10,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-2,66,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-1,19,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,1,16,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,12,26,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,6,81,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,4,30,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,2,89,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,19,29,9,1), new GeneratedEnemyUnit(16,-1,47,11,3), new GeneratedEnemyUnit(0,-14,37,50,2), new GeneratedEnemyUnit(16,-6,10,46,1), new GeneratedEnemyUnit(8,10,80,19,4), new GeneratedEnemyUnit(-11,11,96,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "eba9f4d952781487af254b757014ec6cb5f3796248412e33d0efc766d1c2964b");
        }

        private static void Case_02763()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2763,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,11,47,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-6,83,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-10,5,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-10,60,43,3), new GeneratedEnemyUnit(1,-3,37,23,3), new GeneratedEnemyUnit(1,19,8,33,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "4ad7a14c65fba9fe4e9663f25a5811f93b7f38602f2630c07b1a3154c08c256a");
        }

        private static void Case_02764()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2764,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-15,14,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,10,82,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-7,75,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,17,14,43,3), new GeneratedEnemyUnit(-2,9,50,44,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f91fb339058aac3f1a049a6fe83cf3ca624fac92fcacd2c94844001da0cee3fe");
        }

        private static void Case_02765()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2765,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,6,70,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,19,79,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-11,46,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-19,33,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,20,76,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "22fadf875cc49e11a6858073e16ce019cdd80e442f623b91f78f94263267c40a");
        }

        private static void Case_02766()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2766,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-17,79,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-4,24,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,13,85,16,2), new GeneratedEnemyUnit(-20,7,98,22,3), new GeneratedEnemyUnit(-12,-13,5,19,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "4bdf9d21256528a49aa4a90fff88481c984a1f240ad74e9542a96efa58e6b0c1");
        }

        private static void Case_02767()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2767,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-1,26,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,18,45,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-5,79,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-3,42,18,1), new GeneratedEnemyUnit(-13,-14,83,14,2), new GeneratedEnemyUnit(-14,16,66,12,3), new GeneratedEnemyUnit(13,-17,42,37,2), new GeneratedEnemyUnit(-2,-9,53,14,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "40c012a21ecbecfbc2f81ba98f189a028b8c6941f99b7a022d249c162bd06c28");
        }

        private static void Case_02768()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2768,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-5,8,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,1,37,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,15,93,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,2,85,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,8,41,27,4), new GeneratedEnemyUnit(-7,-7,16,31,4), new GeneratedEnemyUnit(-15,-7,54,28,2), new GeneratedEnemyUnit(-3,10,33,48,4), new GeneratedEnemyUnit(0,-12,94,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "ba9e71f1467f8ea600c8edfdc19973f5ec015cecc0c8fdd9711b2e2230741342");
        }

        private static void Case_02769()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2769,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-8,41,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,11,66,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-3,59,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,20,63,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-14,82,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-3,89,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-16,33,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,9,90,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,12,39,46,1), new GeneratedEnemyUnit(9,-18,33,16,4), new GeneratedEnemyUnit(-13,-15,28,17,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "2b43776b0f9acdbffe912c778c67f67d92f21c3e79ba180063be3791e6d786e7");
        }

        private static void Case_02770()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2770,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,17,69,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-1,67,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,12,66,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,20,20,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-19,88,46,3), new GeneratedEnemyUnit(-7,-15,89,31,4), new GeneratedEnemyUnit(12,-5,9,49,2), new GeneratedEnemyUnit(-5,19,88,49,2), new GeneratedEnemyUnit(10,18,51,30,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "5640c93e218f6ee2168b20caf0f0e31ebbd2f6f766b98b64bd3e00ffac10dea5");
        }

        private static void Case_02771()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2771,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-16,45,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-7,90,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,20,15,9,3), new GeneratedEnemyUnit(8,-14,61,20,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a75cf4c800a111122f6ed2833b2e23b3e4d12bbe06b3631ddfade140a9650c5c");
        }

        private static void Case_02772()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2772,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,0,68,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-11,52,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,9,51,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-10,99,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-4,83,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-6,83,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,1,97,43,1), new GeneratedEnemyUnit(3,20,79,32,1), new GeneratedEnemyUnit(-5,19,98,22,4), new GeneratedEnemyUnit(8,0,96,31,2), new GeneratedEnemyUnit(13,17,51,12,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "96cc3a62e9e3d2f7ec0f434b65b773196bc32ca2d3c2d003717e90ac83aa1744");
        }

        private static void Case_02773()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2773,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-8,77,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-16,88,2,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "342bdaf8712e00a1dcfc3ba15b50c2214750575fce3873c551a985741017b34f");
        }

        private static void Case_02774()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2774,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,2,78,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,15,18,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,11,48,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,9,60,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-18,43,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,7,40,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,16,91,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-18,70,24,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "4d949a45ce3f8be524119334a163406712d94d5da270dbdc2b0b1bf12db01b8f");
        }

        private static void Case_02775()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2775,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-10,63,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,18,71,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-3,73,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-6,78,47,3), new GeneratedEnemyUnit(-20,-17,41,27,2), new GeneratedEnemyUnit(4,-10,41,33,4), new GeneratedEnemyUnit(6,-13,8,10,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "81b4246c7d08b273e9b1ced1f60f5627c675684f16d5e939152c02b4d69eac4b");
        }

        private static void Case_02776()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2776,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-7,99,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,19,15,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,12,96,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-17,76,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-8,16,7,3), new GeneratedEnemyUnit(16,5,28,7,2), new GeneratedEnemyUnit(-9,1,53,24,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d63dfae5059b33ad900fad7eb9f81776d84176cb7fdf412f523bff03fb4bf0fe");
        }

        private static void Case_02777()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2777,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,15,54,7,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "66b6129dddab09a8cebacaa2dff8b0b57259ff239445586f86cda39ac0328570");
        }

        private static void Case_02778()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2778,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,3,57,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-13,73,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-4,48,27,2), new GeneratedEnemyUnit(-17,7,6,28,1), new GeneratedEnemyUnit(8,11,14,8,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "f2f21bca998092e7eee5b215ce8eb0809a1a51f1f307aab43b4ef220a200a1d9");
        }

        private static void Case_02779()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2779,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,7,73,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,17,64,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-9,75,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,12,9,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,15,13,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,20,65,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,14,41,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,12,62,7,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "9eb323ac96b44d8c946c33b5ddda2dd3ee4bf329a8d3c790de2fad3b9ce849de");
        }

        private static void Case_02780()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2780,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-3,97,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-9,17,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-17,62,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,16,20,27,1), new GeneratedEnemyUnit(-19,-18,48,4,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "3f2dd8b25a7d84694092d853278382644c4521d3ccb82d2a42334774fa65ad44");
        }

        private static void Case_02781()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2781,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-17,56,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,0,55,14,1), new GeneratedEnemyUnit(-16,6,68,22,2), new GeneratedEnemyUnit(18,7,59,6,4), new GeneratedEnemyUnit(-14,3,85,30,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "df730908fffaa7871066a21997e85c0d1a0c3eec4319fe76415d02682fa73e57");
        }

        private static void Case_02782()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2782,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-12,73,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-1,43,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-13,12,19,4), new GeneratedEnemyUnit(7,-11,60,14,4), new GeneratedEnemyUnit(12,8,50,20,3), new GeneratedEnemyUnit(18,12,8,15,3), new GeneratedEnemyUnit(-9,9,27,33,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "7e3dcc4e3c1aa91fba6b4b11431491ac7b13988f8156a8b573116aeae0d65724");
        }

        private static void Case_02783()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2783,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,15,23,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,13,59,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-9,47,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-9,83,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,13,83,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,16,65,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-18,47,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-4,36,6,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "bb8e995faab63f464687c16775cf70cb323ece00b32f90968fb864412c56dbaa");
        }

        private static void Case_02784()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2784,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-2,39,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,14,29,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,14,17,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,9,26,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-2,77,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-20,74,24,1), new GeneratedEnemyUnit(-16,4,61,13,4), new GeneratedEnemyUnit(-11,3,49,34,2), new GeneratedEnemyUnit(-12,-5,32,6,1), new GeneratedEnemyUnit(-12,-19,61,2,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "cd3193e6420cf31b1a063e07381a04dccd9226bf40585094e9165b80043052d9");
        }

        private static void Case_02785()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2785,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-4,53,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-9,38,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,17,37,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,11,88,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-2,19,32,1), new GeneratedEnemyUnit(15,-4,100,14,2), new GeneratedEnemyUnit(-20,12,17,39,3), new GeneratedEnemyUnit(11,-20,44,1,2), new GeneratedEnemyUnit(3,2,35,14,2), new GeneratedEnemyUnit(5,1,39,16,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "e9f15feb086c31dbc611d4c0d4db6687bfd366f86f42514a9de8d9e686c12f0f");
        }

        private static void Case_02786()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2786,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,10,63,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,2,65,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,17,38,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-12,67,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,10,57,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-19,9,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-8,46,45,3), new GeneratedEnemyUnit(-2,17,72,7,3), new GeneratedEnemyUnit(13,2,5,17,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "5f9d9df9859e900bcde5590a11c2f313bcc1f7f8f0d41529acc42cf718cc5675");
        }

        private static void Case_02787()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2787,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-13,40,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,7,34,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-18,34,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,0,24,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-3,34,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,19,78,3,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "2a2087a53d5d5d132cbffa86a16ef9befc03a4da8c6d5699832c5e2c7d4c71ac");
        }

        private static void Case_02788()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2788,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,10,56,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-16,97,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-6,91,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,15,29,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-4,86,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-3,64,26,1), new GeneratedEnemyUnit(-5,20,24,21,2), new GeneratedEnemyUnit(15,15,38,9,4), new GeneratedEnemyUnit(-17,-5,85,40,3), new GeneratedEnemyUnit(-18,-12,44,23,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "f28beb73632ad58ff49e27057f445344d57e15c81691a64b535368bad0973941");
        }

        private static void Case_02789()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2789,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-2,66,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-2,94,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-4,22,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-13,94,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,2,83,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-15,7,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-10,6,7,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "9152057a412dbf94385f592023b0432498e935bc11dbc8ce7aadb7670d9940cf");
        }

        private static void Case_02790()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2790,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-17,75,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,14,73,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-1,93,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,12,11,48,2), new GeneratedEnemyUnit(7,-19,23,28,2), new GeneratedEnemyUnit(-16,-17,76,16,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "e9b8958473b9426c598498546462c92b457eefc377927bb2a58738a98ef99288");
        }

        private static void Case_02791()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2791,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-11,59,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,18,50,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,5,28,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-12,24,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,3,6,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,16,95,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,1,73,23,3), new GeneratedEnemyUnit(-17,4,88,48,3), new GeneratedEnemyUnit(8,-1,65,28,1), new GeneratedEnemyUnit(16,10,33,17,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "991ccd0a56605bf4c16224a66488a48b541ae55dc857711397e2821734150092");
        }

        private static void Case_02792()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2792,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,1,14,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-19,92,34,4), new GeneratedEnemyUnit(15,-11,92,25,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "0a7dc25b9bece1ab55dbabe3a7fad582fbe5e0348f865d295eed3fed689b6f1b");
        }

        private static void Case_02793()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2793,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,7,17,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-17,20,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,3,85,16,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "669a3f51d21ca11c551e1c1ae1d435d0ce0d7860fbf070951c976a3b19332a20");
        }

        private static void Case_02794()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2794,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,19,64,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,3,76,17,4), new GeneratedEnemyUnit(-15,13,53,4,3), new GeneratedEnemyUnit(-5,-6,92,33,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8a17fe210b545683a6a4276e1b5c99e4fc0196f7419a5356892467c591b549b5");
        }

        private static void Case_02795()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2795,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,13,80,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,4,53,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,3,20,43,4), new GeneratedEnemyUnit(-14,9,98,25,2), new GeneratedEnemyUnit(-6,-14,95,33,1), new GeneratedEnemyUnit(-6,-4,100,49,2), new GeneratedEnemyUnit(17,9,47,6,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "61bb63cc3f3c1e21cc5f7a23b94fe23e9483cea2b9b6b9156451892bf428e0c3");
        }

        private static void Case_02796()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2796,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-6,99,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-17,12,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,16,28,49,1), new GeneratedEnemyUnit(12,15,7,23,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "7f99154330acb89855d10233580dba60775e7970983974c83d122f3a05dbce2c");
        }

        private static void Case_02797()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2797,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,1,60,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-9,69,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,2,47,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-18,62,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,8,65,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,9,76,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-17,58,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,0,85,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,13,88,13,1), new GeneratedEnemyUnit(-9,-9,21,33,4), new GeneratedEnemyUnit(2,-8,84,26,4), new GeneratedEnemyUnit(1,13,10,31,2), new GeneratedEnemyUnit(10,-10,24,46,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "09fb0ad92bf6588cf2c0465262f5b703efea935ef40f1f6491cfe1d9b20dc97e");
        }

        private static void Case_02798()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2798,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-14,90,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,6,93,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,6,12,45,4), new GeneratedEnemyUnit(10,-15,36,7,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "b4c8bbd6c32e82dd0bb27949cd8f381f0ed9f728f3ab40c1d4f3d99b4e2c5c02");
        }

        private static void Case_02799()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2799,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-16,86,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,6,52,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-15,36,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-11,32,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,9,44,30,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "2b49d6dc0fb71ab3b2bf2787c80d0288bd535c5c9580bbdbdf4508baec599f6c");
        }

        private static void Case_02800()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2800,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-8,49,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-6,73,6,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "b361bde6dd0678c8b48663ba2a1baa975aaeb8e8cab2eb0ee013e451baa4e26d");
        }

        private static void Case_02801()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2801,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,11,14,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,11,87,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,1,38,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-15,34,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,11,37,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-4,38,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-20,89,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-3,94,48,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2c86fbbb04cfa975146b770d6e7d64e7eb382cb3926b3a1bc3a62b45e58f51cb");
        }

        private static void Case_02802()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2802,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,1,15,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-11,45,3,3), new GeneratedEnemyUnit(-18,3,30,13,1), new GeneratedEnemyUnit(-12,7,70,7,2), new GeneratedEnemyUnit(7,-9,27,48,4), new GeneratedEnemyUnit(-12,13,26,37,1), new GeneratedEnemyUnit(15,0,70,41,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "c5783f56faade1297dd904857f2aa3b6a8b501f8a093cefbc9c27819ac7815d5");
        }

        private static void Case_02803()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2803,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,9,34,4,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "c9a6abf782caeb2980282083d716d0284d0ddd1e02d7216854ab5a9cac8daff1");
        }

        private static void Case_02804()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2804,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-18,85,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,8,18,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,16,63,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,11,73,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,10,16,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-9,17,19,3), new GeneratedEnemyUnit(15,-7,61,34,3), new GeneratedEnemyUnit(4,13,63,38,1), new GeneratedEnemyUnit(-2,-6,99,34,4), new GeneratedEnemyUnit(1,14,83,20,3), new GeneratedEnemyUnit(2,-6,67,48,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "24d6c0e8c41690d570eef302f24cce73dbef3e917cc9e8db3731da9bfba7e410");
        }

        private static void Case_02805()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2805,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-7,96,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-6,36,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,9,6,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,4,94,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-10,31,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,14,9,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-10,82,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-8,28,15,4), new GeneratedEnemyUnit(-8,-18,58,1,1), new GeneratedEnemyUnit(16,1,59,8,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "83e9ec54903ffd8c964d8aa179199b82df0a9c0d34eae553cb004c2f74cef315");
        }

        private static void Case_02806()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2806,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-17,55,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-10,58,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,17,34,31,1), new GeneratedEnemyUnit(-6,-2,71,42,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "678f42fd92751ddc5dfad0b0af58059f1c6e0ed381092d2e4b5b20ce5d1e2079");
        }

        private static void Case_02807()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2807,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,13,25,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,16,29,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-14,60,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,2,81,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,13,78,45,2), new GeneratedEnemyUnit(11,2,93,3,4), new GeneratedEnemyUnit(-18,-10,79,43,2), new GeneratedEnemyUnit(15,-9,15,33,3), new GeneratedEnemyUnit(2,-1,36,35,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "abfca2e85d8c6e22e0371ea9e0cd5e6bd7bae0b9faada0760b3a0c2b8e1598f5");
        }

        private static void Case_02808()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2808,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-13,42,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-18,36,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,20,57,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-6,30,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,20,48,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-6,26,39,4), new GeneratedEnemyUnit(10,-19,39,41,2), new GeneratedEnemyUnit(20,14,12,4,4), new GeneratedEnemyUnit(0,18,77,1,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "86ba55432718b96d7eea5d4bab7f1b4d0711d5d8bfc610d8b92af83ac44f9db0");
        }

        private static void Case_02809()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2809,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,0,45,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,18,89,50,1), new GeneratedEnemyUnit(-13,10,86,50,2), new GeneratedEnemyUnit(-5,-12,35,25,4), new GeneratedEnemyUnit(3,-16,76,10,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "89b98602761e71ac42684acb67de65a2de711982666ab1130dbcf2b250626478");
        }

        private static void Case_02810()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2810,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-19,65,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,13,76,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,10,56,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,1,43,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-7,43,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-4,39,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-20,5,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-3,72,31,3), new GeneratedEnemyUnit(-12,-6,51,3,4), new GeneratedEnemyUnit(-8,4,98,39,4), new GeneratedEnemyUnit(-5,1,54,41,1), new GeneratedEnemyUnit(-12,18,21,23,3), new GeneratedEnemyUnit(-13,-12,79,10,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "345a69de8e08bd31e3efc11ab7bf76a5b606fb6e29481097ac331b9c93ff8052");
        }

        private static void Case_02811()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2811,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,4,62,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,6,89,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,3,92,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-6,38,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,6,95,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-6,26,45,3), new GeneratedEnemyUnit(15,-20,60,32,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "297d32f1fe5c29d72bb249f8e244eca22357c1ceb0d446f3f66bbcef53ecd4b9");
        }

        private static void Case_02812()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2812,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,20,16,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-10,50,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,2,92,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,18,12,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-14,80,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,6,87,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-15,67,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b4d40e2715e79eff5812a0190c22f9b9b232de1ff643f15295a7f49fc88083d5");
        }

        private static void Case_02813()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2813,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,1,39,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,11,14,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,17,93,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,1,97,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,20,11,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-14,71,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,14,76,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,19,58,29,3), new GeneratedEnemyUnit(-14,6,49,5,1), new GeneratedEnemyUnit(-10,17,40,13,2), new GeneratedEnemyUnit(15,9,21,30,4), new GeneratedEnemyUnit(-7,-4,38,22,4), new GeneratedEnemyUnit(-18,-1,68,46,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "6136f73fe695e81187d0abd80a2868626323c4ade2b3bc093881646abb2266f8");
        }

        private static void Case_02814()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2814,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-4,65,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-2,69,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-7,58,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-4,43,5,1), new GeneratedEnemyUnit(-13,2,96,26,2), new GeneratedEnemyUnit(13,5,71,33,3), new GeneratedEnemyUnit(-20,-19,49,39,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "681d3163fb287936ec11f4e95093756623125aae950c321ed20b2b0e335c846a");
        }

        private static void Case_02815()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2815,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,9,8,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,8,93,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,18,39,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-4,84,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-2,89,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,4,73,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-13,57,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-3,76,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-1,54,24,1), new GeneratedEnemyUnit(-15,10,15,4,3), new GeneratedEnemyUnit(6,-17,13,14,1), new GeneratedEnemyUnit(-15,9,29,2,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "2d5bd4bb655082c6746077fb45295d30e8c7b2cd9e0f1a77ffcce2bd4f2deb8c");
        }

        private static void Case_02816()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2816,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,15,84,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-13,43,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,18,61,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-12,46,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-8,28,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,13,18,14,3), new GeneratedEnemyUnit(19,-1,87,18,4), new GeneratedEnemyUnit(-15,-11,26,25,3), new GeneratedEnemyUnit(-17,-20,67,41,1), new GeneratedEnemyUnit(-12,5,44,19,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "4fe7d17d414d5bce9a664ef9f6300d43d957fe2f33c9f0c0a64016edec5c9ede");
        }

        private static void Case_02817()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2817,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-14,81,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,3,93,20,1), new GeneratedEnemyUnit(-5,-17,99,9,2), new GeneratedEnemyUnit(13,-18,46,3,4), new GeneratedEnemyUnit(18,-12,94,39,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "7ac430402b92b0b911019a4bf173647e778b903e31372cf7e9f8a6247d67fd0d");
        }

        private static void Case_02818()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2818,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-12,18,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-9,23,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,3,97,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-9,13,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-3,61,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,3,96,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-2,17,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,12,86,9,3), new GeneratedEnemyUnit(8,-3,42,26,1), new GeneratedEnemyUnit(15,9,46,24,1), new GeneratedEnemyUnit(7,13,77,49,2), new GeneratedEnemyUnit(-10,8,32,16,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "f9ec986d1816ea471ae781d2753e166ea3f6fb2447ecd0b92c7dc5b4e6cd8b7e");
        }

        private static void Case_02819()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2819,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,2,100,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,13,37,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-5,33,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,7,11,11,4), new GeneratedEnemyUnit(20,12,65,41,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "db7d11970d68f81dee30bd04ccb8ea59d0381e5bc48625246fc7742e7adb02ba");
        }

        private static void Case_02820()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2820,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-8,76,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,14,10,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,15,13,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-13,57,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,16,16,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,15,25,6,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "5829c969e015243c5daca26e649e32982560da454eb340e2eb8ae270f28d6297");
        }

        private static void Case_02821()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2821,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-17,83,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,14,83,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,8,51,47,2), new GeneratedEnemyUnit(4,3,94,1,3), new GeneratedEnemyUnit(13,-15,48,6,2), new GeneratedEnemyUnit(-17,-3,89,30,4), new GeneratedEnemyUnit(12,11,20,44,2), new GeneratedEnemyUnit(-12,6,66,38,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "e882ed92e2225d9e428565a442268c15c5c64788d10f519e94b207baa19c9127");
        }

        private static void Case_02822()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2822,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,19,91,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-8,72,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-7,29,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,4,22,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-9,13,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-4,34,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,2,53,44,1), new GeneratedEnemyUnit(7,-2,98,38,4), new GeneratedEnemyUnit(-11,-18,63,44,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2e2f59d59fd31d1d686ac4f0d496d20fc09daebfb03c7bd35deea938ac4cd15c");
        }

        private static void Case_02823()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2823,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-6,18,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-15,46,2,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "7106c584b7a25e3d10d440b3302e336a7415aa21b377ac100f9aa0ece482143b");
        }

        private static void Case_02824()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2824,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-20,62,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,19,85,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,3,23,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,8,91,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,2,7,4,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "b40491deb52ca7b1f0a3ef7fb8f90af88d9a3a876ee1736022de440f3cda1b89");
        }

        private static void Case_02825()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2825,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-9,37,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-10,74,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,18,82,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-2,65,32,2), new GeneratedEnemyUnit(8,-9,99,13,2), new GeneratedEnemyUnit(-10,-3,61,42,3), new GeneratedEnemyUnit(0,3,50,26,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8f85b6bc0479625112cade9ad9631d7b4301f023c0110cae7740f0a0c7bb8b89");
        }

        private static void Case_02826()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2826,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-14,6,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,8,60,28,1), new GeneratedEnemyUnit(-7,7,38,46,4), new GeneratedEnemyUnit(8,-2,79,11,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "6d94522f02d02de7c4664a1a2305318e1bf05c1f3b6bb2cb44fcec29c8afb2e4");
        }

        private static void Case_02827()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2827,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,0,63,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-6,24,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,18,87,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,11,16,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-17,70,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-10,96,9,1), new GeneratedEnemyUnit(9,-18,8,27,1), new GeneratedEnemyUnit(-13,-5,27,46,4), new GeneratedEnemyUnit(20,13,6,40,4), new GeneratedEnemyUnit(-20,-5,10,33,4), new GeneratedEnemyUnit(7,19,6,46,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "37d15d18076096e9093c749f116cb9766c1a2e32e6c7a093f05a6cdb534c2f8b");
        }

        private static void Case_02828()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2828,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,1,96,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,6,78,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-16,73,34,1), new GeneratedEnemyUnit(-5,-7,51,38,2), new GeneratedEnemyUnit(20,14,54,37,1), new GeneratedEnemyUnit(18,-1,8,29,4), new GeneratedEnemyUnit(1,12,47,32,4), new GeneratedEnemyUnit(-7,15,93,45,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "e1c9ef990f17c05efc184aa705232634ff9e93246790067b7c8f1ca4a059c6b5");
        }

        private static void Case_02829()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2829,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,17,68,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-14,58,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,18,49,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-6,11,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,20,28,2,2), new GeneratedEnemyUnit(-9,18,51,35,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "d247d3fe179fb39d6380a0447b9ea617aaa51a9440e2318fbea7a59b938f3338");
        }

        private static void Case_02830()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2830,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-7,70,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-9,93,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,18,77,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-10,79,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,20,17,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,11,91,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,2,71,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-1,35,12,4), new GeneratedEnemyUnit(17,14,20,44,1), new GeneratedEnemyUnit(1,-11,82,17,3), new GeneratedEnemyUnit(-1,7,17,1,3), new GeneratedEnemyUnit(20,-17,26,47,2), new GeneratedEnemyUnit(-1,-17,71,24,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "92e42d060878145f8ba9c7b354c2f41e48cfb92d74008dcc70b9c279119ede40");
        }

        private static void Case_02831()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2831,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,9,31,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-13,72,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-10,82,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-14,5,43,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b7bbc06b54c81301bc0eeaeae1dbecdc2d3129edaab45b23ea713ffbe2768e81");
        }

        private static void Case_02832()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2832,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,13,64,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,13,93,40,1), new GeneratedEnemyUnit(-4,-4,7,26,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "6191cf85faa5cba64b67235cf84d9469ac24c6a8ed1f97b74dc4e82ade9af13a");
        }

        private static void Case_02833()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2833,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,11,62,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,17,51,29,3), new GeneratedEnemyUnit(0,-12,30,46,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "f01b0f178cc103786d7d8fdbd7f382a55a5308ab80fbf6e31bd3bcbf81278141");
        }

        private static void Case_02834()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2834,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-16,19,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-20,41,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,18,49,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,16,31,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,6,30,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,1,72,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-11,24,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-16,77,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-11,75,28,1), new GeneratedEnemyUnit(-6,-5,74,37,3), new GeneratedEnemyUnit(-7,-2,45,33,2), new GeneratedEnemyUnit(-2,12,25,14,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "f19b598e87d25c683ac286d3f11ca768c0775e26348777d0cc965355d125fa0a");
        }

        private static void Case_02835()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2835,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-7,78,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,12,12,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-6,67,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,16,90,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-2,61,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,4,92,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,19,22,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-17,21,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-5,58,26,3), new GeneratedEnemyUnit(11,5,67,44,2), new GeneratedEnemyUnit(10,-12,79,25,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "e6f445281900d64e767a1608999597d0818260c86585283961b6cb63dbf0500e");
        }

        private static void Case_02836()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2836,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-18,14,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,13,64,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,20,95,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,2,13,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,11,99,44,4), new GeneratedEnemyUnit(-5,-18,6,44,1), new GeneratedEnemyUnit(13,16,72,15,3), new GeneratedEnemyUnit(19,-6,92,19,4), new GeneratedEnemyUnit(0,4,11,35,2), new GeneratedEnemyUnit(6,1,22,42,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "287477679023b06d882c215eaae8188c28277b5bd484569920483fcc2d2404bf");
        }

        private static void Case_02837()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2837,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-2,9,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-16,68,7,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "b00b748171b06e03681debbaa95c67941bf2a89d47b93be9324cd840e9a8cd56");
        }

        private static void Case_02838()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2838,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-17,18,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,5,71,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-16,82,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,5,82,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,13,61,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,19,100,26,3), new GeneratedEnemyUnit(12,19,17,42,2), new GeneratedEnemyUnit(-1,13,79,36,3), new GeneratedEnemyUnit(1,11,16,6,3), new GeneratedEnemyUnit(16,-17,74,47,4), new GeneratedEnemyUnit(-17,17,66,44,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "fd56683dbd2a8ddb5a0edb23ca4a76d0ad021fce59de3f2ad490e29a87e76cd0");
        }

        private static void Case_02839()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2839,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-18,62,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-14,96,8,4), new GeneratedEnemyUnit(20,7,72,35,3), new GeneratedEnemyUnit(0,-12,88,8,2), new GeneratedEnemyUnit(-7,-4,67,12,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "2d57760813b07c32e2018ddfca3d1bb29b214576c9e0a6e8da83707475615393");
        }

        private static void Case_02840()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2840,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-6,83,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-16,55,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,1,60,12,3), new GeneratedEnemyUnit(-2,7,75,38,1), new GeneratedEnemyUnit(-10,-13,98,36,3), new GeneratedEnemyUnit(20,-5,25,34,2), new GeneratedEnemyUnit(-10,5,68,28,4), new GeneratedEnemyUnit(-11,7,53,27,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "b0fa33af29cfb158cc6d3a2d1255bc7f5e4dd593c566c526b56a079c9a1c7e40");
        }

        private static void Case_02841()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2841,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-7,33,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,19,40,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,17,43,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,10,100,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-13,29,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-15,12,40,1), new GeneratedEnemyUnit(16,2,44,19,1), new GeneratedEnemyUnit(-9,12,40,44,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "e215f13e015157ed2a627ca531599df87f859b87b665e7e93b05bbfb5b4439e5");
        }

        private static void Case_02842()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2842,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,1,84,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-8,5,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-18,35,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,6,50,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-18,7,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-9,69,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-14,45,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,0,18,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,2,84,32,4), new GeneratedEnemyUnit(5,4,53,47,3), new GeneratedEnemyUnit(-18,-1,92,3,3), new GeneratedEnemyUnit(-15,-14,25,29,4), new GeneratedEnemyUnit(13,-19,57,20,4), new GeneratedEnemyUnit(11,-16,62,30,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "202ee2edd9bb493c185e7ae4e43e13c7aa7887276a4375f333625f07bb757937");
        }

        private static void Case_02843()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2843,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-19,78,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-20,91,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,4,6,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-9,51,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,3,99,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,1,43,4,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3560336e2ec442dcd99d03230c75ca9ec44fcee2258e445c464ac9888abaa274");
        }

        private static void Case_02844()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2844,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,2,63,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,2,14,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-13,38,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-4,17,7,3), new GeneratedEnemyUnit(16,8,56,40,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "5fa9844d793460185e90ff77c8d125978bb5524e3934da2b2587b1c4feb337c2");
        }

        private static void Case_02845()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2845,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,16,29,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-6,22,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,9,31,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-20,94,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,14,11,24,1), new GeneratedEnemyUnit(5,-8,38,3,2), new GeneratedEnemyUnit(-7,-16,58,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "015c6f16e9f2834794a0acbf5465df5e9f1083c7e320bd9fcd69a658215efc69");
        }

        private static void Case_02846()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2846,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,6,62,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-3,36,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,17,54,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,8,53,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,13,40,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-13,76,46,2), new GeneratedEnemyUnit(-6,-5,6,5,3), new GeneratedEnemyUnit(5,-18,17,21,3), new GeneratedEnemyUnit(5,-13,75,24,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "0648be45ad2c0273d2914b2c444726a5c92704947641d6f8cf33142449b6d2a2");
        }

        private static void Case_02847()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2847,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-10,12,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,6,66,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,12,54,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,20,29,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-15,14,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-20,84,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-7,92,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-11,55,34,2), new GeneratedEnemyUnit(14,4,6,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "668ce55e72e299393855953fd4f454275617d7bbfdc7c78d98ea6be5479b6291");
        }

        private static void Case_02848()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2848,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,13,49,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,9,52,24,3), new GeneratedEnemyUnit(18,-5,72,40,4), new GeneratedEnemyUnit(15,-14,36,12,4), new GeneratedEnemyUnit(-12,-1,48,48,1), new GeneratedEnemyUnit(-1,16,89,43,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "8a146d241d02a4b0a795122b9be06b6a3876357ebb3d38f292354eed8c9fd6a6");
        }

        private static void Case_02849()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2849,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,8,18,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,9,26,43,2), new GeneratedEnemyUnit(11,-4,30,28,1), new GeneratedEnemyUnit(-15,-14,71,19,3), new GeneratedEnemyUnit(17,-4,17,40,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "03846a08f0d568b9359d68aa63651453adc41326b43e6766fc4b4637cd81cc6f");
        }

        private static void Case_02850()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2850,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,11,51,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,4,41,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-20,9,26,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "954ff5eb249c1ee2aa31e7e00f1f500bdb8884698fb7771c65fee36852f407e4");
        }

        private static void Case_02851()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2851,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,10,55,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-6,45,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-11,52,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-17,78,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-3,97,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-7,64,21,3), new GeneratedEnemyUnit(9,17,82,35,1), new GeneratedEnemyUnit(19,18,90,15,1), new GeneratedEnemyUnit(-16,-2,91,49,4), new GeneratedEnemyUnit(-19,14,24,14,2), new GeneratedEnemyUnit(19,-7,99,26,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "edadfa113fe9db1960654cffbb5981f48676d1432d5df303489c64a90f9df6d6");
        }

        private static void Case_02852()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2852,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-3,52,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,9,78,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,12,83,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,10,77,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-6,97,3,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "a869cdedd3e7af14003d8c2a33979eaf3753f9905498d315e75e410f2a3e4668");
        }

        private static void Case_02853()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2853,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,11,42,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-18,91,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,4,70,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,19,60,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-10,39,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,9,26,29,4), new GeneratedEnemyUnit(3,-13,86,36,1), new GeneratedEnemyUnit(-14,-19,46,18,1), new GeneratedEnemyUnit(0,-16,20,29,4), new GeneratedEnemyUnit(5,-13,68,20,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "fc10959697fa6772f911c6649a3f22c985ce9b33639456dacfc75fb24b94a248");
        }

        private static void Case_02854()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2854,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-11,70,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-15,78,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,1,58,42,4), new GeneratedEnemyUnit(17,2,30,20,4), new GeneratedEnemyUnit(13,3,43,49,4), new GeneratedEnemyUnit(1,-12,52,26,2), new GeneratedEnemyUnit(11,3,92,29,1), new GeneratedEnemyUnit(-8,16,85,9,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "3161754c383480a40269f169feef4a28d182983822277bf593649f6e75c4e471");
        }

        private static void Case_02855()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2855,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-19,74,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-18,47,4,3), new GeneratedEnemyUnit(-17,16,17,15,1), new GeneratedEnemyUnit(2,-17,80,29,3), new GeneratedEnemyUnit(0,17,30,10,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "ca885a35bf7bbeb389cd7f7b3d4ddf0060c30be116b1af45064283a9ae939541");
        }

        private static void Case_02856()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2856,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,6,34,3,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "bc6c03108aa1d90811e1a32e32f29fad960287f04ff71cda92511f37e15e57b1");
        }

        private static void Case_02857()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2857,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,0,94,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-15,27,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-2,35,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-7,25,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,13,49,47,1), new GeneratedEnemyUnit(20,1,81,15,2), new GeneratedEnemyUnit(-8,-17,22,44,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "6cb60a9ea682b038ff683676b933188a0339f2d19c14a4c82e92a7259c9bc1e4");
        }

        private static void Case_02858()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2858,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,12,74,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,20,17,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-20,54,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-2,47,44,4), new GeneratedEnemyUnit(3,-20,68,37,1), new GeneratedEnemyUnit(15,17,16,38,2), new GeneratedEnemyUnit(10,-3,13,31,4), new GeneratedEnemyUnit(-6,3,53,24,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "85c3372c8f867501e3a00b5c8d26eabdf926b58d3db466e8f5b1db00cb756a75");
        }

        private static void Case_02859()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2859,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-2,46,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-10,83,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-15,7,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-17,39,36,4), new GeneratedEnemyUnit(-1,6,74,20,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8b4ab34228c9b5f219b57dd5c3932ba18ec233938f9177130f0ec588189db90d");
        }

        private static void Case_02860()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2860,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-11,96,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,13,54,29,4), new GeneratedEnemyUnit(10,16,98,5,3), new GeneratedEnemyUnit(8,1,93,46,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "6668e4c3df1895e052bf8662ba0466045910b991d0837ca9840f140ac7bf39a6");
        }

        private static void Case_02861()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2861,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-14,52,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-9,77,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,19,81,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,17,16,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-6,78,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-11,39,43,3), new GeneratedEnemyUnit(-9,5,47,26,2), new GeneratedEnemyUnit(-9,-9,11,9,4), new GeneratedEnemyUnit(10,11,87,44,2), new GeneratedEnemyUnit(2,9,29,30,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "db9c075c22f6897e141f6a4fc5e6d3ed5e2a4d57d4e018239a36635a73d7639e");
        }

        private static void Case_02862()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2862,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-1,99,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,4,99,30,4), new GeneratedEnemyUnit(6,6,20,9,4), new GeneratedEnemyUnit(-5,-12,47,50,3), new GeneratedEnemyUnit(-8,17,46,2,1), new GeneratedEnemyUnit(-20,-13,64,44,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "776bd790e6e79ddd4bff44b310f79f57106a0de725942c7245c798cbfa49cd25");
        }

        private static void Case_02863()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2863,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,0,47,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,0,59,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-16,7,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-11,69,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,12,24,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-15,20,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,8,67,11,3), new GeneratedEnemyUnit(-12,-10,40,45,2), new GeneratedEnemyUnit(20,13,43,29,1), new GeneratedEnemyUnit(15,-4,31,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "20b6738effe7a86546e2dd5db2e9918fea7e4dc7986047fea27407aab5d3e00f");
        }

        private static void Case_02864()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2864,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,16,86,1,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "e854ef40b7587bd8cae6f4481ca45f7e8254a114f162677cfc1f798f1030c8ef");
        }

        private static void Case_02865()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2865,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-20,43,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-4,57,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-6,33,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,9,62,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-10,75,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-20,89,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-12,85,10,1), new GeneratedEnemyUnit(8,13,89,49,2), new GeneratedEnemyUnit(14,1,36,21,3), new GeneratedEnemyUnit(6,-20,94,33,3), new GeneratedEnemyUnit(6,-19,59,37,4), new GeneratedEnemyUnit(-7,0,56,36,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "3bbbcfcf5da0f60aa4448d86e5cceabea534650ebb902e029b157f24ddf919e8");
        }

        private static void Case_02866()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2866,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,2,58,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,2,55,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-19,23,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-17,90,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,7,88,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,16,56,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,19,78,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-7,82,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-16,71,1,1), new GeneratedEnemyUnit(-11,20,67,33,4), new GeneratedEnemyUnit(-20,9,30,10,2), new GeneratedEnemyUnit(15,-1,63,50,2), new GeneratedEnemyUnit(4,7,90,3,2), new GeneratedEnemyUnit(14,5,5,8,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "f535ee870236e0036583d3b9dbe8ba3e2c9f77ef57275b0eb26c94b635e25875");
        }

        private static void Case_02867()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2867,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-6,25,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,2,12,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,17,19,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-13,100,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,18,95,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-6,16,28,3), new GeneratedEnemyUnit(-8,-6,52,27,3), new GeneratedEnemyUnit(-17,20,14,46,2), new GeneratedEnemyUnit(3,-10,78,11,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "85ba5de6069c2807bb62a50653ead7fe527cc1d0b27a37cb38a93f956e293bd5");
        }

        private static void Case_02868()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2868,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,16,75,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-9,72,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-9,20,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,14,67,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,20,32,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,10,59,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,18,23,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-19,9,9,4), new GeneratedEnemyUnit(-10,-3,45,33,2), new GeneratedEnemyUnit(18,-13,92,35,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "660f1abc17e701e692d8e4dd5711cbc1bad7dc7244430ccf7e4bad0cc3aab462");
        }

        private static void Case_02869()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2869,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-19,57,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-18,89,10,1), new GeneratedEnemyUnit(-10,-9,42,33,4), new GeneratedEnemyUnit(0,0,44,9,1), new GeneratedEnemyUnit(20,4,76,21,2), new GeneratedEnemyUnit(14,8,36,20,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "ee10fe5c06dbf2e988b2c7033990963490b2c660cb2be31374d881b3f78f9fb1");
        }

        private static void Case_02870()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2870,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,9,41,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,6,28,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,3,59,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-15,64,44,3), new GeneratedEnemyUnit(-17,17,30,34,1), new GeneratedEnemyUnit(-15,-5,39,8,4), new GeneratedEnemyUnit(9,-19,65,13,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "6d9381f90d5cc4b43aaf1e8818cc910ec394cad47039dc0e62b4711bd031ec70");
        }

        private static void Case_02871()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2871,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-19,45,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,18,80,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-6,54,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,13,99,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,13,77,12,3), new GeneratedEnemyUnit(-14,-2,77,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "f03c592f6df7d9a44cabb51e93a6315c20c05fdffe7a3159a2118e2da34d2a57");
        }

        private static void Case_02872()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2872,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,18,11,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-16,26,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,15,85,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,13,66,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,8,83,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-9,49,6,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "c91d7327a5fd14cced05871b4f36ec2834bccd09d278e1c883efe25a71e15991");
        }

        private static void Case_02873()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2873,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-7,38,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,20,31,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-4,90,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-3,51,12,3), new GeneratedEnemyUnit(8,-11,85,23,1), new GeneratedEnemyUnit(20,-8,9,44,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "a9a969e80b717290e88ebe07f9964032be174a7848bd83886058065eb47234ff");
        }

        private static void Case_02874()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2874,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-9,67,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,11,63,4,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "d0432f084eb79d062e69b7b43d5f3d9102d94694a533f161347db83450f9f44f");
        }

        private static void Case_02875()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2875,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,16,100,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,14,34,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-2,69,49,1), new GeneratedEnemyUnit(-16,9,14,21,4), new GeneratedEnemyUnit(20,0,85,25,4), new GeneratedEnemyUnit(11,18,57,14,1), new GeneratedEnemyUnit(-4,7,41,9,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "99cb239e99fa29802197ed0275bc0cf44fd2ee2d20c87385ffd494d2030e66cd");
        }

        private static void Case_02876()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2876,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-20,95,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-9,42,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-8,14,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-4,41,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-19,9,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,19,39,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-12,18,17,2), new GeneratedEnemyUnit(11,17,41,11,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "355fb002af5f3530bb40d18f1be359c42a2fb376721d57f5e16bee94fa9b8930");
        }

        private static void Case_02877()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2877,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-3,85,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-6,47,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,9,53,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-17,95,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-18,48,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,19,45,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-2,67,45,2), new GeneratedEnemyUnit(11,11,96,37,4), new GeneratedEnemyUnit(-7,8,96,8,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f91c43701e106afc33eb374719d3efd5ed542940175b1e482f1a6f525377e317");
        }

        private static void Case_02878()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2878,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,1,8,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,11,70,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,8,74,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,11,24,3,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "1461b0a3c5438aaf35e63044e747d4196d5e88bd894a911ad92b8dee1e540113");
        }

        private static void Case_02879()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2879,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,20,82,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,8,81,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,17,62,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-5,20,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,9,86,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,17,20,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-20,7,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,14,17,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,13,82,18,3), new GeneratedEnemyUnit(16,-10,67,48,2), new GeneratedEnemyUnit(15,-9,23,43,2), new GeneratedEnemyUnit(-15,-20,17,35,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "99f5b431f9988f0a3c9e1fe8363ec4aed020f94163e0ac8a9dd04aeaae8afa71");
        }

    }
}
