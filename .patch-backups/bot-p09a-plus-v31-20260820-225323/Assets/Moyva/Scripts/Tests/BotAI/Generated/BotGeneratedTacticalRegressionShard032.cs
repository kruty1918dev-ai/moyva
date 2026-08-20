using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard032
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_05760();
            Case_05761();
            Case_05762();
            Case_05763();
            Case_05764();
            Case_05765();
            Case_05766();
            Case_05767();
            Case_05768();
            Case_05769();
            Case_05770();
            Case_05771();
            Case_05772();
            Case_05773();
            Case_05774();
            Case_05775();
            Case_05776();
            Case_05777();
            Case_05778();
            Case_05779();
            Case_05780();
            Case_05781();
            Case_05782();
            Case_05783();
            Case_05784();
            Case_05785();
            Case_05786();
            Case_05787();
            Case_05788();
            Case_05789();
            Case_05790();
            Case_05791();
            Case_05792();
            Case_05793();
            Case_05794();
            Case_05795();
            Case_05796();
            Case_05797();
            Case_05798();
            Case_05799();
            Case_05800();
            Case_05801();
            Case_05802();
            Case_05803();
            Case_05804();
            Case_05805();
            Case_05806();
            Case_05807();
            Case_05808();
            Case_05809();
            Case_05810();
            Case_05811();
            Case_05812();
            Case_05813();
            Case_05814();
            Case_05815();
            Case_05816();
            Case_05817();
            Case_05818();
            Case_05819();
            Case_05820();
            Case_05821();
            Case_05822();
            Case_05823();
            Case_05824();
            Case_05825();
            Case_05826();
            Case_05827();
            Case_05828();
            Case_05829();
            Case_05830();
            Case_05831();
            Case_05832();
            Case_05833();
            Case_05834();
            Case_05835();
            Case_05836();
            Case_05837();
            Case_05838();
            Case_05839();
            Case_05840();
            Case_05841();
            Case_05842();
            Case_05843();
            Case_05844();
            Case_05845();
            Case_05846();
            Case_05847();
            Case_05848();
            Case_05849();
            Case_05850();
            Case_05851();
            Case_05852();
            Case_05853();
            Case_05854();
            Case_05855();
            Case_05856();
            Case_05857();
            Case_05858();
            Case_05859();
            Case_05860();
            Case_05861();
            Case_05862();
            Case_05863();
            Case_05864();
            Case_05865();
            Case_05866();
            Case_05867();
            Case_05868();
            Case_05869();
            Case_05870();
            Case_05871();
            Case_05872();
            Case_05873();
            Case_05874();
            Case_05875();
            Case_05876();
            Case_05877();
            Case_05878();
            Case_05879();
            Case_05880();
            Case_05881();
            Case_05882();
            Case_05883();
            Case_05884();
            Case_05885();
            Case_05886();
            Case_05887();
            Case_05888();
            Case_05889();
            Case_05890();
            Case_05891();
            Case_05892();
            Case_05893();
            Case_05894();
            Case_05895();
            Case_05896();
            Case_05897();
            Case_05898();
            Case_05899();
            Case_05900();
            Case_05901();
            Case_05902();
            Case_05903();
            Case_05904();
            Case_05905();
            Case_05906();
            Case_05907();
            Case_05908();
            Case_05909();
            Case_05910();
            Case_05911();
            Case_05912();
            Case_05913();
            Case_05914();
            Case_05915();
            Case_05916();
            Case_05917();
            Case_05918();
            Case_05919();
            Case_05920();
            Case_05921();
            Case_05922();
            Case_05923();
            Case_05924();
            Case_05925();
            Case_05926();
            Case_05927();
            Case_05928();
            Case_05929();
            Case_05930();
            Case_05931();
            Case_05932();
            Case_05933();
            Case_05934();
            Case_05935();
            Case_05936();
            Case_05937();
            Case_05938();
            Case_05939();
        }

        private static void Case_05760()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5760,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,14,68,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,18,73,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-16,29,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,5,99,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,12,86,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-20,14,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,17,42,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,1,17,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-1,71,35,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0223f972efef32f558acd1c410a3b366aa1448a7fce4ab530540e4bafc08a5a9");
        }

        private static void Case_05761()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5761,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-5,96,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,6,84,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,2,79,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,10,71,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,8,73,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,7,21,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-11,53,5,3), new GeneratedEnemyUnit(-2,-18,95,16,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "91dd0d0ae56e8398c57f7d905882d22cac5c3284ee805c759a0cb4778d75767b");
        }

        private static void Case_05762()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5762,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,3,63,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-11,81,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-4,47,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,5,8,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0ed5b5314d827d0d81cda38d29a59a33b1041892f6a55f17afd5ca5b546d1d41");
        }

        private static void Case_05763()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5763,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-9,46,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-8,81,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,17,62,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-8,37,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-10,98,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-3,76,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,15,73,14,2), new GeneratedEnemyUnit(5,15,91,42,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "f086e1a8906ca33db013f38728b3b5738a8c6127c12fc839ec39cdd55383787b");
        }

        private static void Case_05764()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5764,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-14,71,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-17,5,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-10,28,2,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "9189c5763e50b464892af175addac069aea64b36a64757bc205cb1b94aa9f6d4");
        }

        private static void Case_05765()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5765,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,4,33,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,19,55,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,10,82,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-18,73,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,15,77,1,2), new GeneratedEnemyUnit(0,1,68,3,4), new GeneratedEnemyUnit(-3,-20,33,40,3), new GeneratedEnemyUnit(-3,18,91,12,4), new GeneratedEnemyUnit(5,5,65,33,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "2dbbb74c752842d262bb1cfba6f7780b76e0b91fca4958a8672a873f40f7ff7f");
        }

        private static void Case_05766()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5766,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-6,44,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,19,45,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,3,98,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-1,8,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,0,60,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,13,98,10,4), new GeneratedEnemyUnit(11,-17,75,17,4), new GeneratedEnemyUnit(19,-16,46,25,2), new GeneratedEnemyUnit(5,9,81,39,3), new GeneratedEnemyUnit(2,19,20,16,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "d879ada9f6205083754b720030fdf164d9d3d01b11d91708b0dcb6dbc2a1a635");
        }

        private static void Case_05767()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5767,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-12,49,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-4,80,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,9,93,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-6,8,47,1), new GeneratedEnemyUnit(11,5,79,28,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ad193914846c962fc3ace667de4267c45fafc9390d443294135241325735b731");
        }

        private static void Case_05768()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5768,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-17,56,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,3,61,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-18,68,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-4,66,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,20,15,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-9,59,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,8,77,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,2,67,6,3), new GeneratedEnemyUnit(-7,7,93,39,2), new GeneratedEnemyUnit(-19,15,17,2,2), new GeneratedEnemyUnit(7,-20,46,17,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "3e5c18829c5fc762e68fd35de03b43f088018e5e88e5e505bbd9bb38a36e122e");
        }

        private static void Case_05769()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5769,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-18,22,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-6,29,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-15,23,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-7,68,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,0,56,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-13,78,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,11,52,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-11,21,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,17,54,24,4), new GeneratedEnemyUnit(-18,8,20,22,4), new GeneratedEnemyUnit(-19,18,63,15,1), new GeneratedEnemyUnit(14,-14,96,12,1), new GeneratedEnemyUnit(11,17,52,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "ca6c59cccec6b490b520e6a34fa8a8ba19ed23cdb4ee2c78b01f518e580cc27a");
        }

        private static void Case_05770()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5770,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,13,52,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-10,11,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-12,65,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-8,28,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-11,72,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-8,29,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-14,28,19,3), new GeneratedEnemyUnit(17,6,57,44,3), new GeneratedEnemyUnit(9,7,31,26,2), new GeneratedEnemyUnit(-5,-1,92,32,4), new GeneratedEnemyUnit(6,-19,58,9,3), new GeneratedEnemyUnit(-1,-8,23,26,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "8145dfc24b045e35bd210a8f48723ae780f71cd6975ba8e62536d9ac720ff6e5");
        }

        private static void Case_05771()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5771,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,18,41,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-15,90,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,18,75,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,12,42,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,1,25,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-5,41,22,3), new GeneratedEnemyUnit(2,-12,16,31,4), new GeneratedEnemyUnit(10,3,72,24,3), new GeneratedEnemyUnit(11,-11,62,19,3), new GeneratedEnemyUnit(20,-3,71,19,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "78da4eb091c8b1c10532598a621ee21c425e27b31016dbfe2f1c94a46fbeb2c6");
        }

        private static void Case_05772()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5772,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,7,83,1,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "0004a4312a4f2cba6ad15245d37d02e8fae0df4eecbc745877d7d72e172f6fbb");
        }

        private static void Case_05773()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5773,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,17,25,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,16,77,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,4,43,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-2,88,7,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "6a89ea53d69d6411e61f4a20c475d30f777cc41c4370793af0c3226dd870c6b5");
        }

        private static void Case_05774()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5774,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-18,62,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-7,23,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,1,47,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,14,89,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,8,31,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,7,15,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-17,19,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "a7e12687b35f7876e44e33fb5fbf63945c0200a068519cc6625ad45dcefc43aa");
        }

        private static void Case_05775()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5775,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-1,73,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,20,20,45,2), new GeneratedEnemyUnit(-2,14,13,40,2), new GeneratedEnemyUnit(18,9,13,3,1), new GeneratedEnemyUnit(-7,20,61,30,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8c21fef92983c6fa02901f0c3417e7d5e6ea4d7a23bfd9e966a775d300e03342");
        }

        private static void Case_05776()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5776,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-1,29,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-1,16,38,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "d89eb3684e3ddb4ab0add2c867fa2d04ab6a5d7d1d614247cdb53760304d4334");
        }

        private static void Case_05777()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5777,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,7,68,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,6,42,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,10,18,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-15,99,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,0,77,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,11,85,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,0,67,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-12,79,47,4), new GeneratedEnemyUnit(3,-3,27,2,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "f24d8f186ce007e7783d6e33880a79ec7e4ebcca3b56b78938815f020526a22d");
        }

        private static void Case_05778()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5778,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-8,59,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-6,74,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-20,28,5,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "315904d59aeaedb8df438ff554f3c49bf9f61dc2388862e4580a8b7bb0dd3aa9");
        }

        private static void Case_05779()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5779,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,8,43,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,3,23,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-6,40,14,3), new GeneratedEnemyUnit(-1,19,66,25,3), new GeneratedEnemyUnit(-9,9,24,22,1), new GeneratedEnemyUnit(20,-13,52,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "6f84ff63eda00f29751404fad5cf053fff9c206f6fdafc96a6ae364fb7029f3f");
        }

        private static void Case_05780()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5780,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,20,21,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-12,34,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-16,84,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-10,71,25,1), new GeneratedEnemyUnit(18,5,16,18,1), new GeneratedEnemyUnit(10,9,73,10,2), new GeneratedEnemyUnit(7,11,96,29,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "b37fb33c74ac50ac57ca3fc8c3bcad09f060517c895c8c76644f6d1feaac88bb");
        }

        private static void Case_05781()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5781,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,15,49,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,17,87,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,5,95,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,19,25,6,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "abecba9a08ba9987a3924ebc93ae6133b041cd9f1f713ad447bd6e95db367301");
        }

        private static void Case_05782()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5782,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,1,68,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,19,26,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-13,81,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-12,5,29,1), new GeneratedEnemyUnit(-1,-14,72,47,4), new GeneratedEnemyUnit(-17,4,8,39,3), new GeneratedEnemyUnit(15,-11,74,1,3), new GeneratedEnemyUnit(5,17,16,50,1), new GeneratedEnemyUnit(11,-6,14,32,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "7901cab9d16573b9aef6733fa54cdea34e06fba05f53b5a47f65337f40c382ed");
        }

        private static void Case_05783()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5783,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-9,19,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,20,45,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-16,78,50,3), new GeneratedEnemyUnit(7,7,83,14,2), new GeneratedEnemyUnit(15,1,12,37,3), new GeneratedEnemyUnit(1,-5,92,49,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "811739002402b80f04a495ac6e55b82670d872d80006471474acce5950ce67bb");
        }

        private static void Case_05784()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5784,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-12,36,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-16,93,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-6,83,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-13,55,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-14,19,2,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "94b339353ccf25d5d7817940ca77a18c7c78cfe8350d18bb08aeb3c3ffaba0db");
        }

        private static void Case_05785()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5785,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,2,36,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,17,5,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,14,86,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,2,84,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,9,80,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,5,45,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-14,60,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-6,70,30,1), new GeneratedEnemyUnit(-3,7,46,7,4), new GeneratedEnemyUnit(-10,13,90,37,1), new GeneratedEnemyUnit(-11,-1,45,23,2), new GeneratedEnemyUnit(-16,19,57,19,3), new GeneratedEnemyUnit(-18,13,6,31,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 35,
                stableHash: "2ef816f5e95bd8f9318909d7de576f4ef1f3137832a2d33ddfa0ab2d7d2e3223");
        }

        private static void Case_05786()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5786,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,19,14,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,0,12,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-3,47,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,15,69,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-4,61,19,1), new GeneratedEnemyUnit(10,-9,35,15,3), new GeneratedEnemyUnit(-4,9,92,33,4), new GeneratedEnemyUnit(10,-15,14,13,1), new GeneratedEnemyUnit(-7,1,7,29,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "f9ac17d955116d130c08beb79d7add45a9ad87373ea110ccb69d666670588d0e");
        }

        private static void Case_05787()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5787,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-18,65,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,12,34,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,4,7,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,15,47,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-7,53,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,19,19,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,6,29,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,12,23,11,3), new GeneratedEnemyUnit(-16,-19,33,49,4), new GeneratedEnemyUnit(19,-9,64,34,2), new GeneratedEnemyUnit(9,-12,93,43,3), new GeneratedEnemyUnit(13,20,32,28,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ec6663fa3759d8ab7e7e0753d327122b2310ad076cd713c47e54753390f6db22");
        }

        private static void Case_05788()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5788,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-13,31,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,0,33,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,6,34,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0b3ced9262b69cc2f3e85a1a33c7c0f141a57a13e305d80c9935e6402afa4afd");
        }

        private static void Case_05789()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5789,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-4,14,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-3,21,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-10,76,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-14,10,39,2), new GeneratedEnemyUnit(11,5,39,26,4), new GeneratedEnemyUnit(-20,14,24,21,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "cb1a74f409a68aebfd673b901d0649813a242b4ad802d34422f326b538f20dea");
        }

        private static void Case_05790()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5790,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-17,59,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,7,42,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,17,47,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-7,18,19,4), new GeneratedEnemyUnit(-5,-16,31,2,4), new GeneratedEnemyUnit(20,-2,29,11,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "92cd6a00d1094e6ea684d222f86b6b07d2d4adda4f805a928a4bc3df157d020d");
        }

        private static void Case_05791()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5791,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-13,25,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,3,38,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-14,6,1,2), new GeneratedEnemyUnit(8,12,16,26,1), new GeneratedEnemyUnit(20,-5,10,39,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "a642aa9436d8d4c1c757fb3313a6f11e92e42f1796eb1c4ca66ed1866c71a249");
        }

        private static void Case_05792()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5792,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-13,59,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-20,53,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-15,34,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,5,9,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-18,96,4,1), new GeneratedEnemyUnit(4,-3,44,13,2), new GeneratedEnemyUnit(-13,-11,77,25,4), new GeneratedEnemyUnit(11,16,20,10,1), new GeneratedEnemyUnit(-15,2,62,12,2), new GeneratedEnemyUnit(-13,-11,8,24,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "dd0c4c1edb321276709c255b9ac1dfdabef7508bc6448a26812086b6d6ea270e");
        }

        private static void Case_05793()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5793,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-6,77,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-6,99,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-9,5,6,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "6e8bf7383a11f43337117223bb0ff0f5ca9c8b9bfb08186475568a499de332d2");
        }

        private static void Case_05794()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5794,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-10,79,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,9,40,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-7,38,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,15,28,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-14,94,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,11,22,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-3,71,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-12,98,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,9,34,36,2), new GeneratedEnemyUnit(10,-19,59,17,1), new GeneratedEnemyUnit(17,-20,100,11,2), new GeneratedEnemyUnit(-11,5,88,4,3), new GeneratedEnemyUnit(-13,18,77,41,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "9371a380dd0ad702be43a786b23c52bf720a75b6b22d0573351ba685ca70829a");
        }

        private static void Case_05795()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5795,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-16,100,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-15,78,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-17,21,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,15,72,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,12,63,2,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "0ba885dbe51bdc3f15c6cdefd9e327cfea23928e6c5906264f20c6fb9c60937d");
        }

        private static void Case_05796()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5796,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-17,73,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-16,25,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-6,17,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,14,51,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-2,5,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,9,89,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,13,99,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,11,41,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-14,82,3,4), new GeneratedEnemyUnit(15,2,65,40,1), new GeneratedEnemyUnit(-10,-2,92,13,3), new GeneratedEnemyUnit(18,18,80,1,1), new GeneratedEnemyUnit(9,-7,94,31,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "9cceb9a56a034056ea44653110dfe781c7c8528e51884533f70626d47f6d06a0");
        }

        private static void Case_05797()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5797,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,6,7,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-18,92,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-1,85,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-10,25,41,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "6863c60b1a27d2ebed3fdb85124b8e1b5f32bad7cf70ef1c0f6006b7c89ac779");
        }

        private static void Case_05798()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5798,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-14,75,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-18,53,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-18,41,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-6,17,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,6,64,3,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "ab47de88984d4b25bb4efb89c3c502c72ab63c95b6376980e07c8bd38e6a2399");
        }

        private static void Case_05799()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5799,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,18,69,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-5,63,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-7,74,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-8,89,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-1,63,35,1), new GeneratedEnemyUnit(-8,-10,81,35,4), new GeneratedEnemyUnit(-18,7,17,26,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "3da95417f8077e81545c96a31a2783876994dc1a1fd73976136accd80029729c");
        }

        private static void Case_05800()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5800,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,20,90,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,3,60,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-10,75,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-20,8,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,13,57,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-13,11,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,1,97,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,1,90,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-20,55,28,4), new GeneratedEnemyUnit(20,5,97,12,4), new GeneratedEnemyUnit(-7,-6,41,41,2), new GeneratedEnemyUnit(15,-15,51,39,2), new GeneratedEnemyUnit(-17,-1,33,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "2db6c0593d334bde685c4be1db6d62178b2bb23673c2367c611f2426852b088f");
        }

        private static void Case_05801()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5801,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,7,20,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-8,89,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-11,7,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,12,87,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,15,26,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-15,13,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-1,15,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-7,86,17,3), new GeneratedEnemyUnit(-4,-20,97,39,2), new GeneratedEnemyUnit(-9,5,79,7,1), new GeneratedEnemyUnit(-7,13,81,37,4), new GeneratedEnemyUnit(17,1,67,29,4), new GeneratedEnemyUnit(-6,-11,69,17,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "fc66bd1bfaee3053aa722a77b84e938ffa39c5fd360d7a460e55b63186139b8a");
        }

        private static void Case_05802()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5802,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-11,24,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,10,31,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-13,62,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-19,21,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,10,46,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-15,56,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-8,87,27,2), new GeneratedEnemyUnit(2,-18,41,32,2), new GeneratedEnemyUnit(16,2,20,34,3), new GeneratedEnemyUnit(-13,11,35,35,1), new GeneratedEnemyUnit(20,15,88,4,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "cb4860b33c4c1093bd188c60b0c3fa4657acd89d71e1d22007dc13082c676cf4");
        }

        private static void Case_05803()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5803,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,14,20,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-14,58,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,14,92,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-3,70,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,2,7,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-13,97,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-20,78,15,3), new GeneratedEnemyUnit(1,-12,7,11,3), new GeneratedEnemyUnit(-15,-15,86,14,2), new GeneratedEnemyUnit(12,8,79,35,1), new GeneratedEnemyUnit(1,16,51,29,4), new GeneratedEnemyUnit(-4,-18,70,11,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "dd407f84d821c2fff999b7a750863e8616cdfc24f88dd1a47cc2042fbb2cc617");
        }

        private static void Case_05804()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5804,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-4,36,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,5,12,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,14,53,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,17,49,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-12,44,46,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "8e52e76f1f505e016fbe2f89e2998b432391f6410e1ca006bf86ac51f20083da");
        }

        private static void Case_05805()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5805,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-14,53,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-9,13,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,9,94,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,1,85,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,1,61,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-20,64,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-8,95,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,12,13,10,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8257cad8cbcc26b6ff550be6897b5f99d579294528b71929027981666f812c01");
        }

        private static void Case_05806()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5806,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,9,19,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-17,25,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,3,12,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-14,35,33,4), new GeneratedEnemyUnit(-15,-12,68,21,2), new GeneratedEnemyUnit(-20,5,12,37,3), new GeneratedEnemyUnit(10,0,64,38,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "c8d9a0f802893fbb9aaf591323ecd823c13bb10695aaa9c034561997ad239dae");
        }

        private static void Case_05807()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5807,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-20,30,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,6,21,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-3,74,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,12,33,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,0,93,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-16,49,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-3,77,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,9,13,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-19,24,34,1), new GeneratedEnemyUnit(9,-14,60,9,3), new GeneratedEnemyUnit(15,-11,100,20,1), new GeneratedEnemyUnit(-2,18,16,29,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "25e9a36fd559efb3ea449d4d189ecbfb63fe668099513a885569549ff6386f94");
        }

        private static void Case_05808()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5808,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,5,6,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-9,97,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,10,45,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,7,100,38,2), new GeneratedEnemyUnit(-14,12,100,16,1), new GeneratedEnemyUnit(6,-16,89,15,1), new GeneratedEnemyUnit(15,-9,34,21,3), new GeneratedEnemyUnit(18,6,60,8,2), new GeneratedEnemyUnit(3,-9,44,20,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "ad801d3ee88948772c9ebc84a02436e79c7dc6ed2bc0c31c8102d9f1e81f87a8");
        }

        private static void Case_05809()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5809,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,5,9,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,3,41,17,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "2aaf95d206ef6bee7d8aef653772c10a0bedfd16cf1965e6659472d9ee78aa7d");
        }

        private static void Case_05810()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5810,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,20,91,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-12,81,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,2,27,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-1,81,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-8,85,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,15,36,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-8,73,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,4,18,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-5,72,10,4), new GeneratedEnemyUnit(15,8,96,44,1), new GeneratedEnemyUnit(-14,-6,96,35,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "2354859b3bc53dcec72e3f6f07ee8a1ce9c068adb122e51db90b181537938d13");
        }

        private static void Case_05811()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5811,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-19,54,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-2,39,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,6,19,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-6,94,1,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "097a7eef794c54752d0b480a4618a595e449396c1f8846c4dbae58392093f9e8");
        }

        private static void Case_05812()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5812,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-7,70,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,5,44,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,18,50,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,17,54,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-19,43,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-14,88,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,5,59,18,3), new GeneratedEnemyUnit(13,19,63,28,4), new GeneratedEnemyUnit(20,-13,80,35,2), new GeneratedEnemyUnit(20,7,96,40,3), new GeneratedEnemyUnit(-9,8,88,14,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "7f3a2d314a9730437ddfc3fec4060c21b041387c6694fed37b0ffc541e4bd1cb");
        }

        private static void Case_05813()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5813,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,1,47,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,15,36,35,2), new GeneratedEnemyUnit(3,5,24,10,1), new GeneratedEnemyUnit(-20,-4,88,36,3), new GeneratedEnemyUnit(11,16,9,29,3), new GeneratedEnemyUnit(-12,-11,27,25,1), new GeneratedEnemyUnit(3,12,29,9,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "f8a5432bee89d13ad943eee84057d76f3e75279a81565166030a78f26bc3a12b");
        }

        private static void Case_05814()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5814,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-16,21,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-16,48,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-1,78,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,9,89,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-2,35,5,1), new GeneratedEnemyUnit(-20,7,43,5,1), new GeneratedEnemyUnit(-9,6,93,31,4), new GeneratedEnemyUnit(-8,13,77,48,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "081ecbb6c8031056321246fd655d89206f40d7fbef6345f6984a73979bf7fbe0");
        }

        private static void Case_05815()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5815,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,11,84,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,0,12,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-5,25,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-10,65,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-10,44,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,17,22,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,0,33,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,4,41,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,5,29,6,2), new GeneratedEnemyUnit(18,-4,79,22,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "157803cb9bbbec3b72ab4449c7d32d706c169d9227d9b10fc04bf6caa91f535f");
        }

        private static void Case_05816()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5816,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,7,18,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-19,62,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,4,37,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-16,92,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,7,40,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-9,46,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,17,57,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-10,27,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-14,84,12,3), new GeneratedEnemyUnit(11,-8,14,50,1), new GeneratedEnemyUnit(6,-9,66,24,2), new GeneratedEnemyUnit(6,13,97,19,2), new GeneratedEnemyUnit(-19,-2,22,49,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "1259fe7a8b7ca4eb54971b9cadee461e2b525e4cd594ac548bd95fc1543d3d2a");
        }

        private static void Case_05817()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5817,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,12,89,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,12,31,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,11,54,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,19,82,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,11,46,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,14,23,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-8,68,21,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ef4207a56c18c4366c2f6f2beafc0f1303337ca12ae57e83be2e1a5e5265d46f");
        }

        private static void Case_05818()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5818,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-4,89,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,16,24,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,3,93,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,9,25,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,13,23,3,1), new GeneratedEnemyUnit(8,-16,64,45,4), new GeneratedEnemyUnit(19,20,52,42,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "a8254fd7ee155c9a647ef049a396f1a7b868f49c0a36d060a8cdc8972e204a56");
        }

        private static void Case_05819()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5819,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-10,12,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,1,90,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,3,68,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-6,34,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,5,11,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,10,31,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-1,44,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,17,92,22,4), new GeneratedEnemyUnit(5,2,27,46,2), new GeneratedEnemyUnit(2,4,77,31,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "e3181e4fccd1e69fe9e8e6a2db30909ee42ee1d06dc05846c7a24228b359f387");
        }

        private static void Case_05820()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5820,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,2,61,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-17,73,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,8,54,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-1,60,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,12,25,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-13,63,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,5,82,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-17,6,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-20,36,20,4), new GeneratedEnemyUnit(10,-6,34,25,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "f617c2e46b59fdab27f08866e6bb87584ca057acff10aa4f2050ea0ac96a22aa");
        }

        private static void Case_05821()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5821,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,15,12,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,15,29,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-3,47,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-13,24,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,1,50,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-1,24,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,4,15,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-8,35,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-13,79,19,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "4611a5103e86f683eba4de41e8d740805702fc2dd53e0130005bc87317097d4a");
        }

        private static void Case_05822()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5822,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,2,64,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,12,87,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,6,32,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-5,68,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,14,53,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-18,78,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-2,11,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-1,94,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,4,57,14,4), new GeneratedEnemyUnit(-20,-19,70,41,2), new GeneratedEnemyUnit(-17,11,70,2,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "8858d25f681077b853b218d93594c3e8b0ccd7a051e43247ed667a5ef6b21244");
        }

        private static void Case_05823()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5823,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,10,77,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-15,88,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,10,84,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,17,26,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-5,42,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-10,83,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,10,10,1,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "7765da23969b1b2aa81d376671cab9b27a024599a0e4af80055f4a129eb8c301");
        }

        private static void Case_05824()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5824,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,8,95,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-7,74,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,19,6,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,14,27,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-6,73,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-4,29,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,17,90,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-6,52,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,3,39,42,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c86769f31cd67c8d572389e65afb88dc717cf71c865e2a339551e2e843d99eb0");
        }

        private static void Case_05825()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5825,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-12,33,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,19,65,25,1), new GeneratedEnemyUnit(7,12,35,29,3), new GeneratedEnemyUnit(-13,1,74,29,1), new GeneratedEnemyUnit(-17,10,20,27,1), new GeneratedEnemyUnit(14,-15,57,16,2), new GeneratedEnemyUnit(14,11,17,50,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "9c3749d8b7fb44c4653b7b9e7e475679bf9c302ff833c4464f2320a413c8fe02");
        }

        private static void Case_05826()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5826,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-2,71,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,7,42,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-1,31,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-14,74,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-4,54,32,3), new GeneratedEnemyUnit(-1,-11,70,2,3), new GeneratedEnemyUnit(-2,12,94,29,1), new GeneratedEnemyUnit(-12,20,70,31,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0aa556ea9b18a9980854a68c049258866c782e601d3fbaf332429131bda9cf4c");
        }

        private static void Case_05827()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5827,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,15,42,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,5,15,2,2), new GeneratedEnemyUnit(-20,19,58,13,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "c1d7fe21a2d181ad575e7dd4322a385830f7a6945927f2b24efcb14922a3f1d8");
        }

        private static void Case_05828()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5828,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,5,30,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-19,65,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-18,33,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-16,64,21,1), new GeneratedEnemyUnit(4,14,65,30,2), new GeneratedEnemyUnit(7,-8,27,23,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "af3d620b3082326de791c942fce6a78b3794015e2a47f42eb96c70529753470b");
        }

        private static void Case_05829()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5829,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,15,82,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,0,92,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-3,73,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-7,22,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,16,9,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,2,65,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-13,73,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,3,60,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,8,6,29,1), new GeneratedEnemyUnit(7,-5,57,44,4), new GeneratedEnemyUnit(6,-20,30,41,4), new GeneratedEnemyUnit(15,1,87,25,4), new GeneratedEnemyUnit(15,-5,29,3,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "e89e355716003ec420ff4138df89f8a47f73a41dc226784ef2a3858d39c5c1f9");
        }

        private static void Case_05830()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5830,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,6,96,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-18,35,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,2,29,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,8,22,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-10,29,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,1,38,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-19,6,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-5,75,47,1), new GeneratedEnemyUnit(4,-2,70,47,3), new GeneratedEnemyUnit(15,-10,96,13,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "0eec662042243bb82a37201a1db15d4c079e718db6d1f2e55882b57f7c712fe0");
        }

        private static void Case_05831()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5831,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,17,78,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,16,83,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-20,59,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,17,80,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,13,10,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-9,6,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,6,48,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-1,15,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,4,59,49,4), new GeneratedEnemyUnit(-7,5,68,12,1), new GeneratedEnemyUnit(13,14,43,2,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ccc8b9b432dba7f892769e26841589412a442aa5fa382644c4cbe15c27b3db4c");
        }

        private static void Case_05832()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5832,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-9,28,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-8,91,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,17,59,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,4,68,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,11,78,41,3), new GeneratedEnemyUnit(-10,-6,43,41,3), new GeneratedEnemyUnit(-12,-7,66,38,4), new GeneratedEnemyUnit(-11,-12,18,12,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "8f3445d0fdaf91887d5df4aa160109d762569e8caafd1549afc677dd3cd61fed");
        }

        private static void Case_05833()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5833,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,5,79,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-9,71,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,17,79,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-15,76,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,20,94,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-11,63,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,19,68,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,5,66,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,0,54,44,1), new GeneratedEnemyUnit(1,2,80,38,1), new GeneratedEnemyUnit(8,-13,22,19,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "e6afef5bf4debf515f09be002df05301d554569e38be8a61da93f99e98a2511a");
        }

        private static void Case_05834()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5834,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,0,44,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-7,11,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,0,54,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-6,90,41,3), new GeneratedEnemyUnit(-12,-1,61,50,1), new GeneratedEnemyUnit(7,-5,88,15,3), new GeneratedEnemyUnit(2,-2,11,46,2), new GeneratedEnemyUnit(-12,2,38,50,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "b88c0fc087a5225b1cd677aa34eab9422d7d34a3496a6fa7b0382688bcab4fcc");
        }

        private static void Case_05835()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5835,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,11,51,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-2,37,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-2,55,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-9,63,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,10,23,9,4), new GeneratedEnemyUnit(-14,-14,62,8,3), new GeneratedEnemyUnit(7,15,81,14,1), new GeneratedEnemyUnit(-17,8,14,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "da80cbac010e88c517c0cc23f898707cade5cf0afd3d6a913f1f63ea216e82d8");
        }

        private static void Case_05836()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5836,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-7,93,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-10,77,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,13,8,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-7,37,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,2,52,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-3,32,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,10,63,26,1), new GeneratedEnemyUnit(-16,8,75,17,4), new GeneratedEnemyUnit(2,-6,20,7,3), new GeneratedEnemyUnit(13,-7,63,8,3), new GeneratedEnemyUnit(15,9,75,20,3), new GeneratedEnemyUnit(-15,-7,22,19,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "0ce77c76d10e101d52b9ba7f93f41746259f87a9a70e687413ba90ea610d1afa");
        }

        private static void Case_05837()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5837,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-4,12,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,5,74,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,17,70,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,16,41,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-16,61,7,1), new GeneratedEnemyUnit(-13,7,52,18,1), new GeneratedEnemyUnit(-1,-10,29,10,4), new GeneratedEnemyUnit(-16,-7,43,19,1), new GeneratedEnemyUnit(-5,-2,72,37,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "f809717562254e26ad1c7fe9fd0d5f09a4d59c7c40d2ac846a5169aaafb0f30e");
        }

        private static void Case_05838()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5838,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,15,35,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-14,33,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-12,45,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-7,86,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,16,60,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-15,31,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,19,11,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,5,68,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-20,37,33,2), new GeneratedEnemyUnit(14,1,52,30,3), new GeneratedEnemyUnit(2,-20,78,21,2), new GeneratedEnemyUnit(0,-14,39,40,4), new GeneratedEnemyUnit(13,7,30,1,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "6bf99879a4d88a5d114802684d425db31a1d7fc674c5ab12949a4d9405cc538b");
        }

        private static void Case_05839()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5839,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-20,15,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,9,18,1,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "83211b4de875e7ef3931e04bf3ac75683a5e221049311a0cd6f16f26dbe5dba3");
        }

        private static void Case_05840()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5840,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,2,52,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,17,64,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-5,31,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-18,94,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,2,29,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-3,21,43,2), new GeneratedEnemyUnit(-6,-11,56,9,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f6488f1f0f2eef3ec8a10042af873465951ea71cdf515385329d557192210c4b");
        }

        private static void Case_05841()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5841,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,15,53,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,4,81,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-2,46,37,1), new GeneratedEnemyUnit(-17,8,22,5,2), new GeneratedEnemyUnit(5,-18,74,1,2), new GeneratedEnemyUnit(-12,8,21,1,1), new GeneratedEnemyUnit(-17,3,83,44,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "536b48b458493705182e115acd0e9ce20a04993a4a878688c1e9a8ab7e665478");
        }

        private static void Case_05842()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5842,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,1,78,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,7,52,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-2,96,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-14,60,42,1), new GeneratedEnemyUnit(-4,5,67,23,2), new GeneratedEnemyUnit(-6,-3,26,24,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "a7c1b891e8509014bd46b948328c5978e6a4fbc564ba365ef3d0a22c07b5f286");
        }

        private static void Case_05843()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5843,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,4,6,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,17,99,10,4), new GeneratedEnemyUnit(-16,-5,77,30,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "697a485503968d1bad46942a24f3be31194afc2070df89b95abc4b94077f5806");
        }

        private static void Case_05844()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5844,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-18,85,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,14,57,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-18,94,3,3), new GeneratedEnemyUnit(2,-11,24,29,1), new GeneratedEnemyUnit(5,-2,59,18,4), new GeneratedEnemyUnit(-13,0,52,18,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "e009c87276156b9a3d097108a501871452c79d5177ff6e314c13730362a02737");
        }

        private static void Case_05845()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5845,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,1,19,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-10,7,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,19,8,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,20,28,25,3), new GeneratedEnemyUnit(16,-14,33,10,2), new GeneratedEnemyUnit(12,-5,13,3,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "35d140fbb6ec0adf8c671b4297f1b77cf462629654cd7f65ed54f21efc5ba025");
        }

        private static void Case_05846()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5846,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,0,89,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,10,55,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,2,79,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,16,53,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-3,86,8,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e4d0d3af05d666160c97cf2c34da6aea73c36557e801e2298ca603acfa8f2804");
        }

        private static void Case_05847()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5847,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-16,53,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-3,100,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,15,43,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,8,46,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,13,81,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-2,71,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,12,60,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,14,90,21,3), new GeneratedEnemyUnit(-12,5,55,37,1), new GeneratedEnemyUnit(16,-7,69,47,2), new GeneratedEnemyUnit(9,-5,96,26,1), new GeneratedEnemyUnit(9,-1,23,24,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "f701f3d6528dde739e27858b86ccd045f0555a0d94309f1bdd513de3a2fada06");
        }

        private static void Case_05848()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5848,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,5,57,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,18,7,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,20,80,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-10,92,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-19,35,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,10,88,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-10,14,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-9,73,2,3), new GeneratedEnemyUnit(-3,19,35,28,1), new GeneratedEnemyUnit(14,-12,28,44,3), new GeneratedEnemyUnit(-17,-17,42,13,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "5366e8ebf8c4b0ae22f99b66de1c5a5bf70f590582eac79711be341a364e996d");
        }

        private static void Case_05849()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5849,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,17,6,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,16,77,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-1,45,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,1,94,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-13,76,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,7,50,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-8,12,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,10,72,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,10,37,44,2), new GeneratedEnemyUnit(13,16,12,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "810a07380333057a2b66565b7d5586baea4c29d43a2be5b56aa0f776861f8403");
        }

        private static void Case_05850()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5850,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-10,56,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-8,25,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-11,91,28,4), new GeneratedEnemyUnit(20,-13,18,41,1), new GeneratedEnemyUnit(-20,-1,27,5,2), new GeneratedEnemyUnit(-10,7,69,35,4), new GeneratedEnemyUnit(14,-8,31,45,2), new GeneratedEnemyUnit(-4,10,62,3,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "d8c7b9dde0f7d203417bc83028648730e09a1d583df1b214df500285c8bc0d59");
        }

        private static void Case_05851()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5851,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-17,91,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-14,56,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,16,89,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-2,14,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-10,78,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-8,90,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,4,33,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-11,56,41,4), new GeneratedEnemyUnit(-5,-7,38,32,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e9306c47b1425fed405cdc637e5e9277fbbd832159428b8712dcb401a7a142da");
        }

        private static void Case_05852()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5852,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-19,10,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-3,44,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,14,45,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,10,11,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "434c04bdb224a9ce0ad99b5ca8bddba24131d9fc1e15ab3f1357ec1a7da8cab9");
        }

        private static void Case_05853()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5853,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,1,92,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,15,61,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-18,33,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-6,56,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,0,47,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,3,22,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-11,38,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-14,43,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-20,48,2,2), new GeneratedEnemyUnit(-9,12,32,41,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "5f78f71aa944cea66c993f75447b4de53f713df33baea7308601fed8baa0f4d9");
        }

        private static void Case_05854()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5854,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,4,85,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,18,77,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-16,25,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-16,87,33,3), new GeneratedEnemyUnit(-13,-14,96,25,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "d22c0cc167f946abf388d68a2407c0723421e2c1b3dc5438a5f0a935bdfc35d0");
        }

        private static void Case_05855()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5855,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,10,13,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,11,71,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-8,14,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-7,41,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,11,97,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-6,78,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,18,79,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,15,80,49,2), new GeneratedEnemyUnit(13,5,92,13,4), new GeneratedEnemyUnit(16,-10,5,46,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "d84121fc93e540bc464f93318bb4c033f9965991579ca65d4f0521f55d8c6e84");
        }

        private static void Case_05856()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5856,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,10,99,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-4,19,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-12,73,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,12,69,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,1,65,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-11,92,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,18,64,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,3,87,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,13,60,45,4), new GeneratedEnemyUnit(3,11,53,12,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "127dc0119ff33a48eb0e6cdc438c7293319e18c9a0ba573c98db27bc8260bce5");
        }

        private static void Case_05857()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5857,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,11,73,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-8,46,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-7,10,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-3,41,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-11,71,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-15,83,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-12,35,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,5,32,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,20,39,44,1), new GeneratedEnemyUnit(8,9,8,21,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "970bd54f921ce87481190c4741c793f342682f87a3e40101a4399670835dbb6a");
        }

        private static void Case_05858()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5858,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-9,53,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-1,92,46,3), new GeneratedEnemyUnit(9,-10,72,28,3), new GeneratedEnemyUnit(-6,19,44,3,2), new GeneratedEnemyUnit(2,-20,19,22,2), new GeneratedEnemyUnit(4,-4,97,29,3), new GeneratedEnemyUnit(-18,19,70,4,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "bc826335383987a09ed8cf0f85cb943aa1229379daba9cda584c418ad56be79b");
        }

        private static void Case_05859()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5859,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-4,99,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-18,76,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,20,54,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,1,84,1,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "6f1f940d26b581da54d12df34e707a005e76ad05c2e4f02e847b1c280138bd6a");
        }

        private static void Case_05860()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5860,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,0,89,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,18,69,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,14,82,49,3), new GeneratedEnemyUnit(4,-10,41,22,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "f9faa2e112a12bd554a707db5a050d112d1b681b38d6056d6575e01f85bfe545");
        }

        private static void Case_05861()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5861,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,18,36,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,6,57,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,15,13,31,2), new GeneratedEnemyUnit(17,7,76,8,2), new GeneratedEnemyUnit(-7,13,93,19,2), new GeneratedEnemyUnit(-18,-6,78,47,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "fbae10876f96ba99e8cd872470eecb8c56a5ce0fcb3b7d8dab967dd4d9a93342");
        }

        private static void Case_05862()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5862,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-19,32,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-17,69,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,7,5,15,4), new GeneratedEnemyUnit(-12,15,69,18,1), new GeneratedEnemyUnit(-14,-11,55,38,1), new GeneratedEnemyUnit(15,-1,19,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "89d93bf27dbc98a1f5749db6dc627607d4b714bba62690ac509298d5577f1171");
        }

        private static void Case_05863()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5863,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-20,21,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-15,71,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-11,83,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-11,40,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,17,53,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,17,9,40,3), new GeneratedEnemyUnit(-9,-18,60,4,2), new GeneratedEnemyUnit(0,19,51,6,3), new GeneratedEnemyUnit(-14,-3,27,13,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "97f0611bc09f168ee134fe4e9018097caa7baec0eebaacdc11809e2b867dac41");
        }

        private static void Case_05864()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5864,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,7,90,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-11,38,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,6,22,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,2,69,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,5,93,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-19,12,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-8,17,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,6,73,31,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6a00006656ad8023d95a18d41070697a1d37e9186d8b064e29fe4ebca906f508");
        }

        private static void Case_05865()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5865,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,15,39,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-15,13,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,12,99,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-18,96,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,6,55,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-3,13,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,11,5,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-13,15,3,3), new GeneratedEnemyUnit(-16,0,89,11,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "d951192b1f0e9921b42a66dc8dcbe6c6428ec83207138ec6b6c1d16264fd7f4f");
        }

        private static void Case_05866()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5866,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-16,16,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,18,68,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-16,32,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-3,39,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-13,91,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-14,21,45,3), new GeneratedEnemyUnit(-2,5,74,33,1), new GeneratedEnemyUnit(-7,6,97,15,3), new GeneratedEnemyUnit(9,-1,71,40,3), new GeneratedEnemyUnit(1,-13,60,29,4), new GeneratedEnemyUnit(10,12,74,32,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1b571f2484670229c1f140c724e8751434cf7241ca8e5f7a73f7df66facbcf0f");
        }

        private static void Case_05867()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5867,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,20,80,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-6,52,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,14,24,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,0,47,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,1,18,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,20,59,14,2), new GeneratedEnemyUnit(-18,-11,28,37,3), new GeneratedEnemyUnit(10,-1,22,15,4), new GeneratedEnemyUnit(16,9,54,48,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "f368e634b439fcf3125c07cfa6a90f2db5e071405b03d807f810cf0781144f2b");
        }

        private static void Case_05868()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5868,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,8,57,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,2,54,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,12,69,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-6,59,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-12,56,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,9,32,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,1,89,20,4), new GeneratedEnemyUnit(-16,-3,24,22,4), new GeneratedEnemyUnit(-15,4,22,16,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "531f692d8e88112086dc9a6f61995f41c5ff7258dd4e107c02b5887adfa1e53b");
        }

        private static void Case_05869()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5869,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,2,86,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,12,52,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,6,24,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,6,80,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-1,38,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,4,56,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-2,61,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,11,53,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,11,38,13,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "a5724cdaef9d32a67e40b0bb7d985658f47ef77a491b028c95287182a1302592");
        }

        private static void Case_05870()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5870,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-1,56,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-11,67,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,20,36,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-4,56,18,4), new GeneratedEnemyUnit(-19,-2,8,38,1), new GeneratedEnemyUnit(-1,4,69,30,2), new GeneratedEnemyUnit(-6,16,81,40,1), new GeneratedEnemyUnit(19,7,69,49,4), new GeneratedEnemyUnit(2,-14,97,35,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "796f3d28af29877c854407c0f9a8611f444f1399348c7bc92965e742d87191d4");
        }

        private static void Case_05871()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5871,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,8,85,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,2,28,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,2,55,46,1), new GeneratedEnemyUnit(-1,18,15,2,3), new GeneratedEnemyUnit(9,-2,8,15,4), new GeneratedEnemyUnit(1,-9,93,26,2), new GeneratedEnemyUnit(-7,18,85,22,2), new GeneratedEnemyUnit(3,8,95,15,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8b52d230b6e3510c82ada83a5b48a655aa0db6521ddd23d9f383a7279fc58be5");
        }

        private static void Case_05872()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5872,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-14,8,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,13,71,12,1), new GeneratedEnemyUnit(13,13,62,32,2), new GeneratedEnemyUnit(-16,-19,30,18,1), new GeneratedEnemyUnit(6,-2,45,47,2), new GeneratedEnemyUnit(10,-6,78,24,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "2a968a543dc238ce788b080a9ed4a125a90852cf67c3236c7915bfb8b263ef91");
        }

        private static void Case_05873()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5873,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-17,55,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-20,50,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-12,64,44,4), new GeneratedEnemyUnit(9,11,90,14,4), new GeneratedEnemyUnit(11,10,28,39,2), new GeneratedEnemyUnit(-11,-13,39,16,2), new GeneratedEnemyUnit(15,1,26,32,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "6ed48c23186edcf52118837a96d5595aeb1bcd85fefb2d2d68a2d0db1ddab8a4");
        }

        private static void Case_05874()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5874,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-6,20,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,5,86,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,9,42,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,16,62,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-4,32,8,1), new GeneratedEnemyUnit(-7,-1,41,31,3), new GeneratedEnemyUnit(20,-1,87,18,1), new GeneratedEnemyUnit(-1,17,65,17,4), new GeneratedEnemyUnit(-12,-11,52,44,3), new GeneratedEnemyUnit(20,20,61,48,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "3bf0ea61952590c6a45981e680f0cc2d34cff8b70e66aca05d14d6e32996cb9d");
        }

        private static void Case_05875()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5875,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,16,16,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,0,32,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,7,55,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,1,56,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,14,73,4,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "90542d5308f10e56a8219829dc6c8236654640e7a8ee3b900ab74458f525514b");
        }

        private static void Case_05876()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5876,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,6,27,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,3,26,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-8,98,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,10,6,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,5,33,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,1,54,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-13,14,47,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "02a804e040b7b7dafb88ce1c05a81b73d771ab22d195ba48e37898c80bb167e6");
        }

        private static void Case_05877()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5877,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,3,41,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,20,55,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-3,77,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,2,23,3,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "97f28ecf2e598fbd8fbf455d35e184a9e23399b18739590f2451dea5fd383659");
        }

        private static void Case_05878()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5878,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-1,13,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,3,39,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,0,83,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-8,46,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,1,93,3,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "99b9998c92aacce0f1be79369163b033dedae5d9072935d77b85c8acb5a4f695");
        }

        private static void Case_05879()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5879,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,10,99,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,10,97,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,14,6,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-14,62,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-18,31,16,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6bc2c718973aa4e35ada16089c7bb8b750452bf1040cd12b79836a6790e5090e");
        }

        private static void Case_05880()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5880,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-5,7,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,15,5,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,1,55,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,17,18,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-11,82,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,10,43,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-16,6,48,2), new GeneratedEnemyUnit(-17,13,82,43,4), new GeneratedEnemyUnit(15,-5,9,3,3), new GeneratedEnemyUnit(-19,-10,52,43,4), new GeneratedEnemyUnit(12,0,60,30,3), new GeneratedEnemyUnit(5,-6,58,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "5e3f7e33a330e5e4d99bd2de34021ab2ab91eff3b37c4ddfb3c9792c68cdf9d1");
        }

        private static void Case_05881()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5881,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-20,81,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,7,36,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-9,46,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-20,8,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-10,17,4,4), new GeneratedEnemyUnit(-1,6,32,33,1), new GeneratedEnemyUnit(17,10,7,22,3), new GeneratedEnemyUnit(3,-14,85,1,1), new GeneratedEnemyUnit(-6,16,96,8,4), new GeneratedEnemyUnit(-15,-16,16,14,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "32586854017eb3c4a21c7cc9fca959f75797b96e6a826b29d2acb317583f577e");
        }

        private static void Case_05882()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5882,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,0,54,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-11,70,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-5,65,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,13,18,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-9,56,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,5,97,45,3), new GeneratedEnemyUnit(-19,18,18,3,3), new GeneratedEnemyUnit(-9,7,80,50,4), new GeneratedEnemyUnit(19,-19,15,30,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "b191b191e198daee2d72b09bb39b66154e5fef51fd5b99ef27bd068d44aed2d8");
        }

        private static void Case_05883()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5883,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,1,86,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-15,9,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,16,55,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-11,57,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-8,23,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,12,60,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-14,62,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,15,59,30,2), new GeneratedEnemyUnit(5,-9,37,3,2), new GeneratedEnemyUnit(8,11,52,34,4), new GeneratedEnemyUnit(15,-15,35,10,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "4f709c8f404bedacbbd971ede9ec3d8c5fda974185a7b7243d30b56c33b56436");
        }

        private static void Case_05884()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5884,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-16,9,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,0,26,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,3,90,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,13,79,35,2), new GeneratedEnemyUnit(3,-10,80,15,4), new GeneratedEnemyUnit(-2,5,70,32,1), new GeneratedEnemyUnit(12,7,22,41,1), new GeneratedEnemyUnit(-14,-12,16,38,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "5ad6d3d4c0b06485e432447c0c7d3de388e318346fc74e3c488f502bae07ab81");
        }

        private static void Case_05885()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5885,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-2,19,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-3,67,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,5,83,14,2), new GeneratedEnemyUnit(-5,-7,77,40,2), new GeneratedEnemyUnit(19,-16,61,2,4), new GeneratedEnemyUnit(-15,-9,77,15,1), new GeneratedEnemyUnit(19,4,100,16,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "014339065a870b57b639f01dbbea6354035908fba636f325e28d8d6afafd2d16");
        }

        private static void Case_05886()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5886,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-20,62,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,2,84,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-11,71,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-20,52,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-8,89,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,19,48,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-11,91,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-14,24,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-14,73,34,2), new GeneratedEnemyUnit(-4,-15,90,5,1), new GeneratedEnemyUnit(-7,6,18,49,3), new GeneratedEnemyUnit(-15,-13,32,47,1), new GeneratedEnemyUnit(20,-20,24,17,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "a4df53e5064b72cfd34a158d078cf94275e73dd54cabd1e24fb94c998f790b95");
        }

        private static void Case_05887()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5887,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,16,55,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-10,38,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-5,60,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,20,75,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-16,67,4,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "dbbeda427db5ab00be15caa5dda712237e0050f158e3e17c9043723a31af00b3");
        }

        private static void Case_05888()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5888,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,17,88,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,19,5,48,1), new GeneratedEnemyUnit(1,-14,87,48,1), new GeneratedEnemyUnit(14,12,25,30,4), new GeneratedEnemyUnit(8,0,8,8,1), new GeneratedEnemyUnit(20,-7,62,47,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "849b43f2036a80e424b4d435886a7c037e153ec5dd1a704397e195fed1369a65");
        }

        private static void Case_05889()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5889,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,2,46,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-12,26,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,12,54,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-5,35,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-2,73,11,1), new GeneratedEnemyUnit(-12,-5,47,13,1), new GeneratedEnemyUnit(-6,-8,44,4,1), new GeneratedEnemyUnit(8,7,35,6,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "c80cb929c472d83f586ce71cf440186cd3ca5adc67fb0eb79aa2ab34cc43701d");
        }

        private static void Case_05890()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5890,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,14,88,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,3,35,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,20,18,15,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ced5684a753607ae6cb028967c183fbf9d9c31140642f985b99955151dc6f711");
        }

        private static void Case_05891()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5891,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-6,96,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,9,9,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,10,50,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-8,49,50,4), new GeneratedEnemyUnit(-20,-8,21,26,2), new GeneratedEnemyUnit(19,9,68,16,2), new GeneratedEnemyUnit(-8,-3,46,30,4), new GeneratedEnemyUnit(1,11,55,33,1), new GeneratedEnemyUnit(19,-3,23,31,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "7bd287c3eeb45d06edbf34f5978a3cd4a3133717f5c8294c52ce2b385fb85914");
        }

        private static void Case_05892()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5892,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-5,52,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,8,17,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,19,7,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,4,21,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-12,25,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-19,15,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,10,83,42,1), new GeneratedEnemyUnit(10,16,76,30,3), new GeneratedEnemyUnit(9,-1,62,21,1), new GeneratedEnemyUnit(19,0,51,47,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "fe829f45d1e248b6966c50bba94cdfea35ca0085fcd845ed91ad63aa055b10d8");
        }

        private static void Case_05893()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5893,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,1,10,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,12,78,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-12,89,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,13,27,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-2,9,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-8,57,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-3,80,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-14,96,28,4), new GeneratedEnemyUnit(-1,17,55,10,2), new GeneratedEnemyUnit(-11,-1,30,43,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "aa2f5ea9fa6f71a30a29ed647bdc47d43fabd256ef091b2e37cb9a15ec65aa9f");
        }

        private static void Case_05894()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5894,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,0,85,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-16,89,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,18,12,20,3), new GeneratedEnemyUnit(14,-16,68,8,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "788388006985c7a5db88eaafa143a344ae7d546fb47ca1b6b827d126d2e52b97");
        }

        private static void Case_05895()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5895,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,12,16,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-15,16,44,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "b967502ee256c76a538de9aa8e81248e49b12286c23acdd9bcfd96325fe70572");
        }

        private static void Case_05896()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5896,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,12,73,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-11,98,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-19,77,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-8,18,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-20,22,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,17,63,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,19,20,6,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "844e59011de613be2b4a42d4f3db4081c1edbf6a97e5ed053afb18afcb199e12");
        }

        private static void Case_05897()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5897,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-9,68,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,8,72,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,20,23,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,2,41,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,20,88,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-5,96,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-9,37,17,1), new GeneratedEnemyUnit(-20,2,43,47,3), new GeneratedEnemyUnit(-13,11,82,4,2), new GeneratedEnemyUnit(3,-20,49,46,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "bb838647d565ea8712d2d55e588dbecf2e7cb61a150fe5eb08f019713533d192");
        }

        private static void Case_05898()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5898,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-5,58,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-2,10,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,20,26,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,18,63,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-20,70,40,1), new GeneratedEnemyUnit(-14,-3,51,43,4), new GeneratedEnemyUnit(12,17,73,48,2), new GeneratedEnemyUnit(-9,-1,51,1,1), new GeneratedEnemyUnit(1,-9,94,35,2), new GeneratedEnemyUnit(1,-5,97,41,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "a4d2096b5c0c7d533024b369c7f88fe35a07d31ecf9651d33907e449eeb235d7");
        }

        private static void Case_05899()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5899,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-17,19,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,9,35,1,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ab9c530837709621e80454cef73c018d849b7e56d94eb716a9d548ec73498371");
        }

        private static void Case_05900()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5900,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,10,44,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-3,68,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-12,27,22,2), new GeneratedEnemyUnit(1,19,59,29,4), new GeneratedEnemyUnit(14,11,22,4,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "d15c1c9da29d3d818d575cf7a067c56a945b067b9f900984f49717f429659dc1");
        }

        private static void Case_05901()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5901,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,5,56,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-6,60,27,1), new GeneratedEnemyUnit(-3,-15,99,21,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "799b1dc509d891ac52f7c4d936cdef9a1629e94519b4159940512caff512caf3");
        }

        private static void Case_05902()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5902,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-10,34,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-20,87,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-1,100,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,2,94,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,6,62,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,11,72,42,1), new GeneratedEnemyUnit(-15,-5,22,19,4), new GeneratedEnemyUnit(16,-17,91,32,4), new GeneratedEnemyUnit(-1,-6,62,4,1), new GeneratedEnemyUnit(9,-16,26,2,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "e7323cfb07c3c0875f746331deb8c10946beaa67b02b83f7ed1e4584e58a813a");
        }

        private static void Case_05903()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5903,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-3,55,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,10,38,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,17,91,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,7,74,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-10,60,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,12,99,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-9,80,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-16,19,2,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "976e34ddcba350534c23a1bc2601b56fc2ed82b91d5e84f588a05b3da8043a6f");
        }

        private static void Case_05904()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5904,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,17,81,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-2,99,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-6,32,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-2,18,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-12,63,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,1,49,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,4,92,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,1,100,37,3), new GeneratedEnemyUnit(-5,-17,27,8,3), new GeneratedEnemyUnit(1,-2,63,37,4), new GeneratedEnemyUnit(10,-13,97,9,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "01c1a5871ca92b67fda9f172b31415137eb62763ecfad733f997cd2ffb22f3fa");
        }

        private static void Case_05905()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5905,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,9,52,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,19,48,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-20,50,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,11,76,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-2,81,35,2), new GeneratedEnemyUnit(-14,19,92,29,2), new GeneratedEnemyUnit(16,-16,27,13,4), new GeneratedEnemyUnit(-3,-11,68,31,2), new GeneratedEnemyUnit(6,-4,27,9,3), new GeneratedEnemyUnit(18,2,91,24,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "7ed010e35a569a006d5f38c1d049ab9c0b1ccab936db815c4bbff09900a3ac7f");
        }

        private static void Case_05906()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5906,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,20,38,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,19,43,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,10,96,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-2,92,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-20,19,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,1,19,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-9,26,21,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "c2d9ad62a685f1083ae4c2a091571d2842dc613c397af7ce5902aaf9f00d36e8");
        }

        private static void Case_05907()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5907,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,12,67,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-7,89,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-4,17,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,2,95,2,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "7e906256e514529dd6aaebd577b31193565f0fe7c71bca6f34d379cb16e2f057");
        }

        private static void Case_05908()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5908,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-5,50,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-18,8,8,3), new GeneratedEnemyUnit(9,-16,85,31,4), new GeneratedEnemyUnit(18,-19,17,3,1), new GeneratedEnemyUnit(-12,-3,8,50,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "6a63980fdfe59c37c11e866af2e0ff33d18ce7e86792ad9a5e5125e3508fe80d");
        }

        private static void Case_05909()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5909,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,14,89,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,10,21,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,13,27,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-4,25,23,2), new GeneratedEnemyUnit(-9,10,38,30,4), new GeneratedEnemyUnit(11,9,19,43,2), new GeneratedEnemyUnit(-12,-2,85,24,1), new GeneratedEnemyUnit(9,-14,68,50,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "2e490d3e385523774301c622f9600448797c976c133626821f7b4b326edffda5");
        }

        private static void Case_05910()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5910,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,12,69,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-20,24,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-14,82,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-2,66,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-4,18,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-2,60,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,5,57,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,17,92,42,4), new GeneratedEnemyUnit(6,11,47,16,1), new GeneratedEnemyUnit(5,-18,85,24,3), new GeneratedEnemyUnit(-1,-19,90,28,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "9ca68bc6c52815c9bbd0c9ac5a3b5ef842600642901b521446c853a192bb798f");
        }

        private static void Case_05911()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5911,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,14,54,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,13,77,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-4,52,19,1), new GeneratedEnemyUnit(-4,4,90,42,4), new GeneratedEnemyUnit(13,8,43,13,1), new GeneratedEnemyUnit(-5,-15,89,15,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "7e9a2d5186adeca941b7d966045aec3a1af37a65f94259dc259b7e01be4d5ca7");
        }

        private static void Case_05912()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5912,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,6,92,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,11,68,34,3), new GeneratedEnemyUnit(18,-6,10,43,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b2b9c0aa4f86a44183eb59af30c1843bad6cc8f2762e0eec850d99fe5d9e1f17");
        }

        private static void Case_05913()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5913,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,18,78,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-9,33,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-18,19,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,12,62,20,3), new GeneratedEnemyUnit(-11,-4,71,11,4), new GeneratedEnemyUnit(19,12,79,9,3), new GeneratedEnemyUnit(6,15,38,33,3), new GeneratedEnemyUnit(15,20,11,21,3), new GeneratedEnemyUnit(14,11,47,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "a9e4cc8f3390e99f32dbc53186a504017753c4df0a3a5f0aa30148ada2b11c33");
        }

        private static void Case_05914()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5914,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-15,83,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,14,27,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-2,13,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-7,14,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,7,30,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-9,51,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-20,8,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-13,80,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,13,81,12,3), new GeneratedEnemyUnit(-3,-10,13,14,1), new GeneratedEnemyUnit(14,4,89,12,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "324fc309127c9bd6eaa9b2999e67a9ff877bea8c8fef949adbdd583e3179fd24");
        }

        private static void Case_05915()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5915,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-9,66,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-6,62,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-18,99,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-12,15,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,14,76,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-4,34,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-9,29,39,3), new GeneratedEnemyUnit(5,10,19,44,3), new GeneratedEnemyUnit(-3,-9,70,43,4), new GeneratedEnemyUnit(20,0,53,5,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "da920b8836cb39709a467c1d853e3bb82cf58fb333354161ec193df4ed50c63f");
        }

        private static void Case_05916()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5916,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-2,96,3,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "bc689f55274bbf242d7ed2a1bf7ae72b68d3ae6d96ec5a449fe778f149495e34");
        }

        private static void Case_05917()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5917,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,10,62,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,5,38,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,20,91,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-19,65,9,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "57f7ff3f978121e322fdaf6a596d83ba86aa4723979067b0222d7795ce39bc6b");
        }

        private static void Case_05918()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5918,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-2,83,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,0,21,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-9,62,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,8,88,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-12,28,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,15,41,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-15,26,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,3,98,2,2), new GeneratedEnemyUnit(1,17,39,43,2), new GeneratedEnemyUnit(2,-1,83,25,3), new GeneratedEnemyUnit(-5,-9,38,8,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "084a6d12f3f418264d4bf857abb09202a85977df52b80f5b9e5786d9248504b6");
        }

        private static void Case_05919()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5919,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,14,60,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-4,64,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-4,65,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,10,78,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-5,58,23,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "58aadc82056a3c67b04be0a516df8104b4c7e139a3f054fb944ea7e34481fcfc");
        }

        private static void Case_05920()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5920,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-9,20,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,2,68,17,4), new GeneratedEnemyUnit(-2,-2,97,44,3), new GeneratedEnemyUnit(11,1,71,1,1), new GeneratedEnemyUnit(-17,15,32,46,2), new GeneratedEnemyUnit(-16,12,22,43,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "21a7cbcd564fb9acdd44e3e1fa7530c5fd32946b0f2f661e2731b3cca4ba4ba6");
        }

        private static void Case_05921()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5921,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-20,71,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-16,71,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-14,100,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,13,80,4,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "732c03e5af8c7692f5ed747b88f0302a3d8a1dd0943039daf2e0263092a19636");
        }

        private static void Case_05922()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5922,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,9,24,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,5,75,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,20,9,20,4), new GeneratedEnemyUnit(-17,-4,12,42,2), new GeneratedEnemyUnit(7,-10,29,1,4), new GeneratedEnemyUnit(-6,-15,78,11,4), new GeneratedEnemyUnit(-20,1,83,5,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "c0605e8845864b9aac7910c6177fb0ae8a4b991185637bb023c38f36d3a5ec55");
        }

        private static void Case_05923()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5923,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-16,67,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-20,46,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,14,37,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-5,14,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,0,57,46,3), new GeneratedEnemyUnit(6,1,89,39,2), new GeneratedEnemyUnit(9,-14,89,34,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "9f6b2a5bef84d8c0ab401fbdd5bebdc35c7129e2baa0fe8d1456613d21c89462");
        }

        private static void Case_05924()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5924,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,20,43,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-8,5,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-9,74,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,19,19,44,4), new GeneratedEnemyUnit(3,13,28,33,1), new GeneratedEnemyUnit(11,-9,21,23,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "e539cdccea9de10d8ad468f39e334b1d5117ed3dea5f8901dcaf6c0cb25a3b5d");
        }

        private static void Case_05925()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5925,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,3,7,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,20,69,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,19,66,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,17,60,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,2,64,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,8,47,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-14,89,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,14,9,31,1), new GeneratedEnemyUnit(2,-7,29,8,2), new GeneratedEnemyUnit(19,-17,85,22,1), new GeneratedEnemyUnit(-16,15,34,15,3), new GeneratedEnemyUnit(17,20,67,39,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "409f1d845d0e9cdeb0bc65de0686c509d2bcfe372a0be9fe96221d163487899d");
        }

        private static void Case_05926()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5926,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,10,71,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,15,85,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-14,31,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-14,23,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,5,77,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,1,92,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-18,94,21,1), new GeneratedEnemyUnit(18,-18,8,46,3), new GeneratedEnemyUnit(15,-17,97,22,2), new GeneratedEnemyUnit(16,10,76,47,3), new GeneratedEnemyUnit(19,-1,81,44,3), new GeneratedEnemyUnit(3,-20,41,36,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "e1b3ab2d11840ece8c0a6ab312d1b7192ca62e457ea68683f3b3b09141061bc7");
        }

        private static void Case_05927()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5927,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,2,47,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-11,87,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,9,79,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-15,25,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,18,24,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,12,68,1,4), new GeneratedEnemyUnit(11,3,65,16,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "0771fb7b25031afdf83591c246d9df53f11d58e4fb00c1580442e971d486cb92");
        }

        private static void Case_05928()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5928,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,16,82,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,10,78,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-9,76,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,3,14,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,7,95,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,14,93,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,4,63,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,0,25,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,0,60,14,4), new GeneratedEnemyUnit(-13,20,51,25,2), new GeneratedEnemyUnit(6,-5,69,32,1), new GeneratedEnemyUnit(16,-4,41,41,4), new GeneratedEnemyUnit(10,-18,57,20,2), new GeneratedEnemyUnit(12,9,27,42,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "79cb6b489684c8c3c30b2cd1f779259261886dde2a25ef09db6ed661c3057608");
        }

        private static void Case_05929()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5929,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-7,52,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-8,97,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-3,92,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-19,87,5,1), new GeneratedEnemyUnit(-9,0,31,19,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "6b6bd193a8ad5905534a173b4792ca6159d2c3c1d4901dbbde4185201b847a20");
        }

        private static void Case_05930()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5930,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,10,87,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,10,94,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,19,56,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,4,88,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-12,98,40,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "54aff70cab78c4c8d14efaa72ba143d0b7bbfeaa32cb71167cdab8a768d9d284");
        }

        private static void Case_05931()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5931,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-14,64,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-20,42,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,0,92,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-2,79,6,1), new GeneratedEnemyUnit(14,-11,67,18,2), new GeneratedEnemyUnit(-2,12,69,28,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "1c2061320b11de018fa91cc13a7735ebc5f9e3566a3d7a9a90a4f5f65f7e3581");
        }

        private static void Case_05932()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5932,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-7,73,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,0,68,2,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "23f2c592da951f288b9d49a358ce378391a9706ef7a35a0bd7404884b67edbf4");
        }

        private static void Case_05933()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5933,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,2,65,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,12,5,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,8,82,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,6,87,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-7,58,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,0,23,4,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "f90517eac59273718cf1eb115ae645496baa82315f906d60d2cb38a4b1cdc974");
        }

        private static void Case_05934()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5934,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-20,62,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,12,41,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,6,25,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-8,53,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-5,84,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-19,7,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,10,70,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-13,46,6,2), new GeneratedEnemyUnit(-1,20,53,18,4), new GeneratedEnemyUnit(-20,2,56,31,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "d58c34ed6d53029d46fe00eb0f5f0867a4b2d3c6a777a31c23451a9629946578");
        }

        private static void Case_05935()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5935,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,5,40,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-5,53,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,9,78,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-3,75,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,20,50,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-3,9,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-6,57,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,11,66,18,3), new GeneratedEnemyUnit(-8,-7,87,31,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "438b64e1e50fc871e01e073f451188e5e2964a0118a398add57c59562574b865");
        }

        private static void Case_05936()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5936,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-12,52,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,17,76,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-9,81,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-5,36,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,8,99,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-12,99,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,10,72,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,15,61,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-7,14,34,4), new GeneratedEnemyUnit(-15,-4,25,36,4), new GeneratedEnemyUnit(10,20,48,43,4), new GeneratedEnemyUnit(-11,1,45,43,4), new GeneratedEnemyUnit(7,-18,14,7,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "c698a7d296a0bcbe2a87143bb3aa47c48823241148e4b26ba52bb29c2e7b39cd");
        }

        private static void Case_05937()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5937,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,0,47,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-17,6,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,16,89,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-3,100,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,14,32,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,13,46,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-15,30,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-2,85,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,17,52,29,2), new GeneratedEnemyUnit(6,13,91,46,3), new GeneratedEnemyUnit(6,12,34,32,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "a3d35034eec3fbd0a1aa569820a5f3ef333f5a4e7c717ef146411f06056431c0");
        }

        private static void Case_05938()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5938,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-5,53,5,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "b4cbee1efc0d46f1a4aea1e3d186a68a0d966dca1558b1423dc9306edb7204bb");
        }

        private static void Case_05939()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5939,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-4,44,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,12,40,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,19,37,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,0,12,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,6,96,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-13,10,44,2), new GeneratedEnemyUnit(11,1,35,42,1), new GeneratedEnemyUnit(-8,-9,61,34,4), new GeneratedEnemyUnit(-17,19,44,45,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "7c0e6a48977c6d6a589e8552fc6ddacafbbb0c8aa03bce77bb6647bd98998e61");
        }

    }
}
