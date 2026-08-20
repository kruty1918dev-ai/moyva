using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard016
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_02880();
            Case_02881();
            Case_02882();
            Case_02883();
            Case_02884();
            Case_02885();
            Case_02886();
            Case_02887();
            Case_02888();
            Case_02889();
            Case_02890();
            Case_02891();
            Case_02892();
            Case_02893();
            Case_02894();
            Case_02895();
            Case_02896();
            Case_02897();
            Case_02898();
            Case_02899();
            Case_02900();
            Case_02901();
            Case_02902();
            Case_02903();
            Case_02904();
            Case_02905();
            Case_02906();
            Case_02907();
            Case_02908();
            Case_02909();
            Case_02910();
            Case_02911();
            Case_02912();
            Case_02913();
            Case_02914();
            Case_02915();
            Case_02916();
            Case_02917();
            Case_02918();
            Case_02919();
            Case_02920();
            Case_02921();
            Case_02922();
            Case_02923();
            Case_02924();
            Case_02925();
            Case_02926();
            Case_02927();
            Case_02928();
            Case_02929();
            Case_02930();
            Case_02931();
            Case_02932();
            Case_02933();
            Case_02934();
            Case_02935();
            Case_02936();
            Case_02937();
            Case_02938();
            Case_02939();
            Case_02940();
            Case_02941();
            Case_02942();
            Case_02943();
            Case_02944();
            Case_02945();
            Case_02946();
            Case_02947();
            Case_02948();
            Case_02949();
            Case_02950();
            Case_02951();
            Case_02952();
            Case_02953();
            Case_02954();
            Case_02955();
            Case_02956();
            Case_02957();
            Case_02958();
            Case_02959();
            Case_02960();
            Case_02961();
            Case_02962();
            Case_02963();
            Case_02964();
            Case_02965();
            Case_02966();
            Case_02967();
            Case_02968();
            Case_02969();
            Case_02970();
            Case_02971();
            Case_02972();
            Case_02973();
            Case_02974();
            Case_02975();
            Case_02976();
            Case_02977();
            Case_02978();
            Case_02979();
            Case_02980();
            Case_02981();
            Case_02982();
            Case_02983();
            Case_02984();
            Case_02985();
            Case_02986();
            Case_02987();
            Case_02988();
            Case_02989();
            Case_02990();
            Case_02991();
            Case_02992();
            Case_02993();
            Case_02994();
            Case_02995();
            Case_02996();
            Case_02997();
            Case_02998();
            Case_02999();
            Case_03000();
            Case_03001();
            Case_03002();
            Case_03003();
            Case_03004();
            Case_03005();
            Case_03006();
            Case_03007();
            Case_03008();
            Case_03009();
            Case_03010();
            Case_03011();
            Case_03012();
            Case_03013();
            Case_03014();
            Case_03015();
            Case_03016();
            Case_03017();
            Case_03018();
            Case_03019();
            Case_03020();
            Case_03021();
            Case_03022();
            Case_03023();
            Case_03024();
            Case_03025();
            Case_03026();
            Case_03027();
            Case_03028();
            Case_03029();
            Case_03030();
            Case_03031();
            Case_03032();
            Case_03033();
            Case_03034();
            Case_03035();
            Case_03036();
            Case_03037();
            Case_03038();
            Case_03039();
            Case_03040();
            Case_03041();
            Case_03042();
            Case_03043();
            Case_03044();
            Case_03045();
            Case_03046();
            Case_03047();
            Case_03048();
            Case_03049();
            Case_03050();
            Case_03051();
            Case_03052();
            Case_03053();
            Case_03054();
            Case_03055();
            Case_03056();
            Case_03057();
            Case_03058();
            Case_03059();
        }

        private static void Case_02880()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2880,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,6,53,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-10,65,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,20,88,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,14,85,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-13,63,7,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "185baf165dc1ab6cd943a0e1f7548b6df8f8ad90bc3d32700ca0b5d501ff8d7d");
        }

        private static void Case_02881()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2881,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,4,70,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-17,46,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-15,73,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,0,66,7,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "5e53f0786350095a5f17196ae2042aee97b5c8bccc10b979aaf5ae4f2a4ba4a2");
        }

        private static void Case_02882()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2882,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,4,67,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,13,80,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-10,94,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,13,54,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-13,15,33,2), new GeneratedEnemyUnit(-5,18,21,21,3), new GeneratedEnemyUnit(-1,5,92,31,4), new GeneratedEnemyUnit(5,13,22,2,1), new GeneratedEnemyUnit(-17,-17,86,42,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "3909117974bc07f411ee88c4ca592558ded1c2c7d9bf8b7f13f4a9cfd3589c11");
        }

        private static void Case_02883()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2883,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,9,20,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-3,17,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-10,55,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,18,55,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,18,52,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-14,41,49,2), new GeneratedEnemyUnit(2,7,30,11,1), new GeneratedEnemyUnit(-11,17,56,32,2), new GeneratedEnemyUnit(0,-15,52,19,3), new GeneratedEnemyUnit(-20,-12,11,40,1), new GeneratedEnemyUnit(20,-6,29,37,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "18e09eb7e1dbb89920376d11fc9f3ba579a4b330da10b5ce7ea3385fafbba7ca");
        }

        private static void Case_02884()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2884,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-8,46,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,3,10,5,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "8f29f3491a80bf8135491241b997a147cfeeceac143f81e718a93bdd7470e7a5");
        }

        private static void Case_02885()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2885,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-15,31,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,9,93,46,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "65f645de2c5c9628010977a780d0d6beff8b7b1e67e833f40ab0042467e768c5");
        }

        private static void Case_02886()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2886,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,13,73,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,0,99,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,1,6,3,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "18eaae544712ce4be4ca90f042f718805de5731e83087b3b985f743710975e38");
        }

        private static void Case_02887()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2887,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,20,22,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-13,25,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-4,73,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-2,49,46,3), new GeneratedEnemyUnit(-12,-19,77,33,2), new GeneratedEnemyUnit(-1,15,82,5,4), new GeneratedEnemyUnit(-12,-12,14,22,4), new GeneratedEnemyUnit(4,-14,57,37,2), new GeneratedEnemyUnit(2,6,44,8,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "72c3276c7c43d6089e657fc5f8f4867653b8d36652cf9f03a20f9d02d412dcc6");
        }

        private static void Case_02888()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2888,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-13,64,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,4,59,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-14,10,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-17,27,41,1), new GeneratedEnemyUnit(15,1,82,40,2), new GeneratedEnemyUnit(-5,6,77,26,1), new GeneratedEnemyUnit(13,-19,86,15,1), new GeneratedEnemyUnit(6,18,49,27,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2b47e930d48792486bf7fff953e7f6ef7de5e1c352fb21eaf6f0af961e0616a3");
        }

        private static void Case_02889()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2889,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-1,67,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,1,37,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-20,88,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-15,88,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,0,64,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-11,86,7,3), new GeneratedEnemyUnit(8,-16,38,5,2), new GeneratedEnemyUnit(16,-3,32,24,1), new GeneratedEnemyUnit(13,-19,24,20,1), new GeneratedEnemyUnit(17,-10,48,18,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "59e3aa7fcc910b5de9e9c56ee315906540318dfae11c259c2ffd05d80f97d3e8");
        }

        private static void Case_02890()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2890,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,20,64,2,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "add253c317fec3cf87440d5677b02fe0e5991e5da41b33179f2882655d793455");
        }

        private static void Case_02891()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2891,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,14,43,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-15,91,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-13,96,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,20,51,15,2), new GeneratedEnemyUnit(-9,12,71,10,4), new GeneratedEnemyUnit(-19,-19,63,24,3), new GeneratedEnemyUnit(-9,12,67,25,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "cca3c36f5a2c3d06ff2846dddad6173e98b13a38892fe2909dbbe55f1ac2f827");
        }

        private static void Case_02892()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2892,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-4,15,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,13,10,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-5,42,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-5,65,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,12,9,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,18,92,30,4), new GeneratedEnemyUnit(-20,-2,64,23,4), new GeneratedEnemyUnit(-2,11,77,49,4), new GeneratedEnemyUnit(-10,8,61,29,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "6eb2ae861b445ebf01e60b011c285fdabd003ea43b19428c04b91470fc5334d6");
        }

        private static void Case_02893()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2893,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-2,33,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-17,25,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-7,63,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,13,77,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,14,35,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,13,41,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,0,39,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "f33bad2bc55442b30d23b3b2eaa59a42ec04d1ae06e2d061895ee93c28221bef");
        }

        private static void Case_02894()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2894,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,14,58,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-5,39,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,18,64,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-4,62,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,11,52,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,13,65,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,5,65,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-19,80,7,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "b385ae588e9ffd2830841f6b1986c97b54ee08a219ec118ce47f589bb77a9cce");
        }

        private static void Case_02895()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2895,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-1,31,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,0,39,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-1,44,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-18,79,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-17,36,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-9,64,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,0,54,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,14,59,45,3), new GeneratedEnemyUnit(1,-6,38,10,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b9a07e16c59808a0185b64b107436016f5ec14bb20eb6e99b700dc162a156841");
        }

        private static void Case_02896()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2896,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-3,81,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,11,85,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-6,5,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,14,37,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,18,36,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,8,79,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-16,10,1,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "8686a871c1ef0d689b5b1b8a6aed58d2291d13a1662b388e460cdbc896125be4");
        }

        private static void Case_02897()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2897,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,12,37,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,3,44,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-15,57,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-11,75,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,10,27,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,12,98,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-8,92,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-9,92,14,2), new GeneratedEnemyUnit(10,-13,65,49,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "cc966601bd53e63580d2e8af82aeab50c827b5c8da742b60b7eba6aad43de217");
        }

        private static void Case_02898()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2898,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-17,14,4,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "ea1faea23a41206db2649d96800da04aa6ca1626d961016c8a3f7dd6d80a6ca0");
        }

        private static void Case_02899()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2899,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,3,5,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-6,23,26,3), new GeneratedEnemyUnit(-9,15,98,25,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "94ea10bc6cf564ababeaeaf06eb203eb12868fe252a094b4126b88fc52e277c8");
        }

        private static void Case_02900()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2900,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,11,79,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-13,13,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,14,76,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-12,24,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-11,52,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-16,29,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-20,70,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-3,70,42,4), new GeneratedEnemyUnit(-15,2,22,10,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "d437f145228a5f86a3d7c3182f74302e1dcbc18a8bd02e1f09eb041723e3f772");
        }

        private static void Case_02901()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2901,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-3,57,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,18,28,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,14,82,24,3), new GeneratedEnemyUnit(-3,15,45,43,4), new GeneratedEnemyUnit(12,-10,37,49,2), new GeneratedEnemyUnit(3,-5,79,26,2), new GeneratedEnemyUnit(-19,-5,90,10,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "34303711fa144f6e07b9cb61fefad8c167097fc634c69e6870bdcb21aef65de0");
        }

        private static void Case_02902()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2902,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-5,62,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-4,66,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,10,94,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-2,93,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-9,79,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-14,49,4,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "1d737c3431fc391487341db2525586f8f1d8e38bc2f181ae9c4e6b119e851600");
        }

        private static void Case_02903()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2903,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,3,32,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-4,12,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-11,64,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-6,63,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-8,21,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,19,18,31,2), new GeneratedEnemyUnit(9,11,39,9,4), new GeneratedEnemyUnit(-4,-2,43,10,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "8257bf6095027bb46f5a20c3736eff2b77448cb13d698b9b249ca5e2def9e8c3");
        }

        private static void Case_02904()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2904,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,0,67,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-14,12,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-8,86,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,12,34,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,10,50,9,3), new GeneratedEnemyUnit(-8,0,15,18,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "0b78c72504e96e06fe6301efe2d859440de114771b28bc6a51add62b891b285c");
        }

        private static void Case_02905()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2905,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-10,58,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,5,35,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,12,82,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,16,33,4,3), new GeneratedEnemyUnit(20,-5,42,3,1), new GeneratedEnemyUnit(-11,12,26,35,1), new GeneratedEnemyUnit(-17,6,74,13,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "ecd61c40f90652528464bf6794f06d0105004409ecfc832ff1f1c7a39b6231b1");
        }

        private static void Case_02906()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2906,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-16,42,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,20,51,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,10,54,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,11,42,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,8,19,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,7,87,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,7,37,49,3), new GeneratedEnemyUnit(-3,-19,100,27,3), new GeneratedEnemyUnit(7,12,96,41,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "f0b3ca1187f9f60dbe8d036585f117eea24a75b2a37ce6361dc6e8ee449d5c49");
        }

        private static void Case_02907()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2907,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-8,78,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,1,83,9,4), new GeneratedEnemyUnit(-18,-16,41,12,1), new GeneratedEnemyUnit(2,16,26,38,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "585bc4ab96713b35b47ecce3c39ec9229ef3e1ba2f6b4c75ea27bffb21a17bcc");
        }

        private static void Case_02908()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2908,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-6,26,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,1,43,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-18,6,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-8,36,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-13,92,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,13,33,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,19,87,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-17,16,5,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "25044e1c98679738602b62d12e46654a2043b6cb09d905de4f48c95cd36301eb");
        }

        private static void Case_02909()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2909,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,7,17,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-18,42,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,11,34,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,11,24,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,19,72,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,5,13,22,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "2ec29d9e427b06c823998a7b11622db36aaa2153a04878c87b066e6131dd9571");
        }

        private static void Case_02910()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2910,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,3,58,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,7,29,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,0,100,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-20,72,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-2,38,14,3), new GeneratedEnemyUnit(-13,13,80,31,1), new GeneratedEnemyUnit(19,4,18,46,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "5075c4e3290fcc3ad2a7f6c5c35da29b1d688315019aa28558561091f5110b41");
        }

        private static void Case_02911()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2911,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,7,91,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-18,76,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,16,31,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,17,11,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-9,10,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,16,62,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-14,55,33,4), new GeneratedEnemyUnit(17,15,71,35,3), new GeneratedEnemyUnit(7,-10,76,14,4), new GeneratedEnemyUnit(-8,15,38,5,1), new GeneratedEnemyUnit(17,-18,24,28,2), new GeneratedEnemyUnit(10,8,31,33,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "174f846da8c79b0cf88c7fc262c7ad6e0d0ff88a1440c9c6006c20fc46b85956");
        }

        private static void Case_02912()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2912,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-19,18,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-13,48,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,2,40,28,4), new GeneratedEnemyUnit(-11,-17,50,10,3), new GeneratedEnemyUnit(19,19,98,1,2), new GeneratedEnemyUnit(2,13,44,36,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "341a08b8807cb54545e930db412e9716f6d698624e2bfae853197790106b2d81");
        }

        private static void Case_02913()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2913,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,10,90,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,20,81,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,8,90,6,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "33ae4d7f2e5a02b3592ea00cef5e5bffd57683fb50a7ac77342670111288d759");
        }

        private static void Case_02914()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2914,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-14,34,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-5,84,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-3,29,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,19,96,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,17,48,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-2,70,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,20,81,42,4), new GeneratedEnemyUnit(12,-11,66,49,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "5bf06edea90330b9e615c641d16d45e56af124ffe7a7c815a6a49c31159c9506");
        }

        private static void Case_02915()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2915,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,6,22,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-14,26,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,1,42,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,17,76,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-2,70,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-10,63,2,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "f681eeaffa7c2904b1e075150ff804d7f3971cc67d43078bc3530a00dff089d1");
        }

        private static void Case_02916()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2916,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-17,44,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,10,10,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,3,68,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,2,79,25,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "02bf612ed81b176c145078a6c23f51e58a72aec591da4cce043e941ca4ce0479");
        }

        private static void Case_02917()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2917,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,8,85,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,11,28,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,9,5,4,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "f07d73bb204bcd29feb6b85a1714dd397a8073458fd466553352e670d1706dc8");
        }

        private static void Case_02918()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2918,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-8,55,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-3,94,30,1), new GeneratedEnemyUnit(8,-13,47,37,2), new GeneratedEnemyUnit(6,-3,71,11,2), new GeneratedEnemyUnit(-1,3,65,24,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "0d2ceb1c072ecf86216580ba4fc57af650876689c37b82e4c74194b9c8cb04e1");
        }

        private static void Case_02919()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2919,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-9,21,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-3,84,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-8,27,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-15,86,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,4,19,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-7,11,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-5,22,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,1,49,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-19,99,34,3), new GeneratedEnemyUnit(2,-17,99,40,1), new GeneratedEnemyUnit(20,-13,23,36,4), new GeneratedEnemyUnit(20,8,96,37,3), new GeneratedEnemyUnit(-8,-3,49,34,4), new GeneratedEnemyUnit(16,17,92,16,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b904598148da675fe0a3892d4f683ddecf22c2b38b5e480a7f6d5e0413b7957d");
        }

        private static void Case_02920()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2920,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-4,21,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,2,75,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,9,82,5,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "c065b67f2d7d9c251d2a8d70c9831cf8edcbdb2d6739732ed36b1032b388a700");
        }

        private static void Case_02921()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2921,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-18,18,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,10,80,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-18,82,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,19,30,24,4), new GeneratedEnemyUnit(0,9,51,7,2), new GeneratedEnemyUnit(-7,11,95,48,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "940075aaa60d0442566af6830189bde81fa89af7c7b7732549de8e7a8b4af747");
        }

        private static void Case_02922()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2922,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,5,96,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,15,54,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-19,84,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-10,78,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,1,34,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-5,89,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,14,43,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,9,33,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "d3e9a417ae2745d2e0e2aac80f53889716297bd3a93ad5c8966cb4f147a3133d");
        }

        private static void Case_02923()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2923,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,13,58,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-8,84,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-10,96,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-19,68,13,2), new GeneratedEnemyUnit(5,8,20,38,1), new GeneratedEnemyUnit(4,10,42,24,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "95ba22b75d7abc0f4b7851236e9591a1610f7bddde3ccb3e31abd629ac458de8");
        }

        private static void Case_02924()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2924,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-10,65,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,2,35,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,8,83,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-2,95,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,5,27,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,15,32,40,3), new GeneratedEnemyUnit(8,4,22,40,2), new GeneratedEnemyUnit(14,-17,32,50,1), new GeneratedEnemyUnit(-16,6,94,26,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "18f0a2f2dcead3296cc76a849dea52299fb34b41efadde0210fbd83daeb1053b");
        }

        private static void Case_02925()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2925,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,10,78,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-3,47,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,12,67,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,18,29,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,14,80,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,2,71,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,8,25,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,7,76,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,11,22,6,1), new GeneratedEnemyUnit(-16,12,53,42,3), new GeneratedEnemyUnit(5,20,12,12,3), new GeneratedEnemyUnit(-15,10,24,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "2103e079082cc7675a1840dac5e7b74d728ff837ff7c0cd71524807e6d842ad6");
        }

        private static void Case_02926()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2926,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-17,16,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-6,46,4,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "3e56426c995aa6c8b5a23ec60b9de849084da07c1c865b0801de847d440f9285");
        }

        private static void Case_02927()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2927,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,16,23,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-18,92,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-11,70,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,8,83,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,3,39,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-7,18,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-6,92,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-19,19,27,4), new GeneratedEnemyUnit(11,0,62,13,1), new GeneratedEnemyUnit(-10,-11,7,10,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "11c7cce458b60af22492798933e7fd1ff5f9a07e9a6b45ac6f4ed2df9ff5bade");
        }

        private static void Case_02928()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2928,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-15,82,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-17,89,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-2,18,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-10,40,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,5,66,31,2), new GeneratedEnemyUnit(6,-3,15,42,1), new GeneratedEnemyUnit(-12,-13,49,22,2), new GeneratedEnemyUnit(-5,3,75,48,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "44c08a31fb2285cbd824cbb73aef63514c41b0082f9165432c0151c60f0fed33");
        }

        private static void Case_02929()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2929,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,0,76,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-13,16,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,11,94,48,1), new GeneratedEnemyUnit(-9,13,8,41,3), new GeneratedEnemyUnit(11,-17,13,30,3), new GeneratedEnemyUnit(4,15,83,8,4), new GeneratedEnemyUnit(-4,-18,99,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "674baca84d8cf70a96a01fb2c1d0d55eed94ee4013a553426446500f940f78ab");
        }

        private static void Case_02930()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2930,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,12,57,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-18,98,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-14,96,9,3), new GeneratedEnemyUnit(1,16,87,50,2), new GeneratedEnemyUnit(19,-3,41,35,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "adcc8f36f36838023cf042ca4668c9618042be1dd875cab29ffcb5ce0fc3059f");
        }

        private static void Case_02931()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2931,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-12,12,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-10,45,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,11,95,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-17,52,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-4,68,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-5,31,2,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "ba886518199819e2d9692c2a00514a569c6c8e22051d547650138fb9637a6c0e");
        }

        private static void Case_02932()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2932,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,18,69,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-16,75,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-6,15,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,9,49,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,8,22,5,1), new GeneratedEnemyUnit(-4,6,11,7,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "fee839a3cb1d64d8b8eb3c106dcd47fd799db54ad72c264983e93cd0d9e9e58e");
        }

        private static void Case_02933()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2933,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,17,39,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,1,80,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-7,24,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-7,25,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,0,38,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-15,81,6,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "57f0b738dfada0fd5fe56f5d53a6683df139a98f1b0a9bf27c99ce9d9273d818");
        }

        private static void Case_02934()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2934,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,17,14,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-8,46,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,3,13,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-10,97,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,7,11,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-16,63,23,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e6354a3ff05a71342241be913490158dfdec1e6fe09b16e8bb7bcaa3e64ca70b");
        }

        private static void Case_02935()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2935,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-4,43,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,4,61,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-18,51,15,2), new GeneratedEnemyUnit(16,-10,66,25,2), new GeneratedEnemyUnit(9,2,16,20,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "c14baff9234c2a45446c63a2538fddb0e683a669835386c0013d550415e9af71");
        }

        private static void Case_02936()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2936,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,3,6,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-8,87,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-18,41,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,17,77,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,14,77,2,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "865c033374ad03208150bc0ca7de41882d1d4f9925350d77e13386a373aeb646");
        }

        private static void Case_02937()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2937,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,13,34,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-20,41,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,6,29,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-2,46,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,5,58,35,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "580f7c8c5ceb25e9e983e116754faddf6ba0067e3dedffe64f124c4a19761717");
        }

        private static void Case_02938()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2938,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,11,82,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,1,40,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,5,86,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-17,91,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-7,6,2,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "49dd512cbcff4e8aba470f2e2484ff292b4f4128f707982d5ebfdb94c24abee6");
        }

        private static void Case_02939()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2939,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-11,42,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,3,74,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,20,24,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-15,38,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-12,39,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,0,38,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-11,81,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-18,84,15,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "053ee2262f538acc78c184dde41b0f0a9df11f9aef2a9f47889320f81bc849b0");
        }

        private static void Case_02940()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2940,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,7,59,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-17,49,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-4,85,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-6,75,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-14,18,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-4,36,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-19,76,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-5,38,3,3), new GeneratedEnemyUnit(-8,-8,77,7,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "9453a4513435409cb77f57f95eb579d6159a746546c6845e5e52248b27289391");
        }

        private static void Case_02941()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2941,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,16,11,6,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "fe12d1642445e8e2c88c48de77c4278c401bad84c9f1f818fb156bf60f48ffe5");
        }

        private static void Case_02942()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2942,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,14,21,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,4,8,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-12,26,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-10,52,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-2,56,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,13,17,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-15,24,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-13,30,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-13,49,26,1), new GeneratedEnemyUnit(8,20,81,5,4), new GeneratedEnemyUnit(-6,12,86,33,1), new GeneratedEnemyUnit(-10,-9,34,33,4), new GeneratedEnemyUnit(-10,1,78,45,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "8fcb64142e75cfa9b5b74e7f6fe51e1e4c1f536d0ab2a29606b5c6cdb78a9d05");
        }

        private static void Case_02943()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2943,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,5,16,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,0,37,10,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "2acc8a2ee142537987d1c18ad421783a2620ab8534874e68887bbdc181b622bb");
        }

        private static void Case_02944()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2944,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-3,99,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,8,96,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,15,6,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-13,48,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-15,29,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,3,20,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-16,32,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-15,23,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,9,34,41,2), new GeneratedEnemyUnit(-19,-18,44,48,1), new GeneratedEnemyUnit(17,-4,99,25,2), new GeneratedEnemyUnit(12,2,66,31,3), new GeneratedEnemyUnit(-10,-3,12,48,4), new GeneratedEnemyUnit(-3,3,69,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "daaf198dbe05062017bef910e19cd982589955ad6e90ffd2a11d7d474b1f9d52");
        }

        private static void Case_02945()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2945,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,16,99,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,6,86,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,2,92,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,17,23,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-1,75,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,4,31,39,2), new GeneratedEnemyUnit(14,-9,64,50,4), new GeneratedEnemyUnit(-16,-15,54,39,3), new GeneratedEnemyUnit(-19,12,75,33,2), new GeneratedEnemyUnit(-3,0,68,24,3), new GeneratedEnemyUnit(15,-8,100,49,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "c48c0c96c9c9bfcb4186ed2cbf7f660224154e179e9f7c2418be2f4d3f2bdfe5");
        }

        private static void Case_02946()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2946,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,14,85,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,2,41,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-14,81,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-18,31,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-13,98,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-16,48,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,6,19,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,10,98,41,4), new GeneratedEnemyUnit(-20,10,80,49,3), new GeneratedEnemyUnit(-7,-15,55,28,4), new GeneratedEnemyUnit(-6,3,97,31,1), new GeneratedEnemyUnit(17,18,80,28,3), new GeneratedEnemyUnit(1,-20,37,33,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "559340af794a4dda6fdfabd9d7de7d4209a5f456e3d9c02551f8767d25a375a2");
        }

        private static void Case_02947()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2947,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-18,58,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-17,70,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-10,72,1,2), new GeneratedEnemyUnit(-4,19,6,49,2), new GeneratedEnemyUnit(-14,4,59,3,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "fce0303b509ec9e457116f0f463b895ea3a357840ff2129bea5f58fac5b09a24");
        }

        private static void Case_02948()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2948,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-11,32,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-6,43,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-4,21,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-11,31,2,1), new GeneratedEnemyUnit(-8,-5,67,5,4), new GeneratedEnemyUnit(-18,-11,6,46,4), new GeneratedEnemyUnit(-1,-3,27,8,4), new GeneratedEnemyUnit(-5,-13,88,23,2), new GeneratedEnemyUnit(-9,-13,67,24,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "eb55cc0d1362d891b7b440f04c7bf25cbe35fefc41e0e778ea097dfa66447411");
        }

        private static void Case_02949()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2949,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,17,50,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,8,81,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-17,32,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,0,42,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-16,17,40,2), new GeneratedEnemyUnit(12,19,54,2,1), new GeneratedEnemyUnit(-5,1,95,16,4), new GeneratedEnemyUnit(-15,-13,44,45,2), new GeneratedEnemyUnit(4,18,38,21,3), new GeneratedEnemyUnit(-9,-17,34,37,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "7a036cb263b34e37c7431b55622a212faaa05fbadfd243d1138ca655108a4411");
        }

        private static void Case_02950()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2950,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,20,17,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,18,87,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-3,71,9,4), new GeneratedEnemyUnit(18,17,49,36,3), new GeneratedEnemyUnit(10,19,57,14,3), new GeneratedEnemyUnit(14,11,76,48,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "6a595b831ee9816ad8b87113a0ade056ae6c1f4482c6e78875c78f3f3ec22113");
        }

        private static void Case_02951()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2951,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,18,41,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-10,73,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-15,19,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,5,12,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,9,27,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,3,34,3,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "986c7d9c83d721be1759ed3764b2e68c32d874c5f042d450728346f6c3b99301");
        }

        private static void Case_02952()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2952,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-6,29,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-17,74,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-4,28,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,0,6,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,13,49,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-16,21,1,3), new GeneratedEnemyUnit(15,18,8,21,1), new GeneratedEnemyUnit(-12,-15,32,49,4), new GeneratedEnemyUnit(-4,19,80,41,4), new GeneratedEnemyUnit(-7,-7,19,13,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "d2e2705b93840e82f56d8551ba4b78eefd854f22c6939a70df981d1d14df8695");
        }

        private static void Case_02953()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2953,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-13,10,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,7,99,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,10,33,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,15,92,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,16,51,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,10,100,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-15,6,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-13,42,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,11,28,39,1), new GeneratedEnemyUnit(-11,0,16,32,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "4719749a7565073361e587b8cbda0f3815cda5bd2e8c36bee52c80ab40c8726a");
        }

        private static void Case_02954()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2954,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,4,27,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-4,39,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,6,75,49,2), new GeneratedEnemyUnit(15,9,50,13,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "df658d72e4155ebd70ec1197c61717dc4610f721595ef460b73197945e81d7bc");
        }

        private static void Case_02955()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2955,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-2,49,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,7,10,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,5,40,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-4,76,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-5,59,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,3,48,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-19,94,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-19,41,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,0,37,22,1), new GeneratedEnemyUnit(-18,11,11,11,2), new GeneratedEnemyUnit(-5,19,54,12,1), new GeneratedEnemyUnit(-16,11,63,16,3), new GeneratedEnemyUnit(-20,14,57,1,2), new GeneratedEnemyUnit(14,-7,6,19,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "6dfd340aa6bff76be2c6b6f750328f247e36b9ff802b86f5726e0d929eef4207");
        }

        private static void Case_02956()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2956,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-19,100,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-4,40,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,4,59,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-6,52,11,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "29587ade261aa0e3ca823b8af58250e57239b5389cb4fc0b46a66dc7cfa905a4");
        }

        private static void Case_02957()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2957,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,10,78,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,19,38,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,12,81,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-16,74,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,10,5,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-9,74,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-12,36,6,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "3a9d99e143ef8c1402c78ecf93dcd12773b7c7e40c660fd746251ad8a1bf47df");
        }

        private static void Case_02958()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2958,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,0,94,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,5,86,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-11,42,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,10,51,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-17,79,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,19,66,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,9,55,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,17,99,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-10,20,12,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "01faa0a94e28075148b3999e04d89997007723689ed190d164139b2574bd38ab");
        }

        private static void Case_02959()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2959,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,6,20,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,4,25,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,16,8,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-16,47,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,13,9,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-11,8,10,1), new GeneratedEnemyUnit(18,11,64,12,2), new GeneratedEnemyUnit(14,-9,22,24,4), new GeneratedEnemyUnit(3,-20,73,8,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "578ca1c0515c67dc60a579ffcc4820074ce1d6b8629f975bd95b89a1c57ca1ad");
        }

        private static void Case_02960()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2960,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-15,87,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-14,50,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,8,80,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-17,9,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-3,87,40,4), new GeneratedEnemyUnit(20,2,100,11,4), new GeneratedEnemyUnit(9,14,21,15,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "7926bb8b1196dbb1a8767b0a29117d6f7c5997e8e69a09bc30d316e96eca75fc");
        }

        private static void Case_02961()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2961,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-14,17,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,1,86,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-9,56,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-11,92,5,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "68fd253e48c08a4754aba0c6a5efc574033e1d9992466aff8e71e3619ac7da9d");
        }

        private static void Case_02962()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2962,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,20,62,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,8,79,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,5,6,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,0,21,7,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "ce1d0a9c115ab54b61824aaee3c77409466a9bbc5528203503048f1ae10f5aa8");
        }

        private static void Case_02963()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2963,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-19,86,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,1,42,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,18,42,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-10,46,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,12,58,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-3,80,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,2,41,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-1,9,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,1,29,9,3), new GeneratedEnemyUnit(-19,11,15,11,1), new GeneratedEnemyUnit(18,16,83,36,3), new GeneratedEnemyUnit(-3,-13,23,22,2), new GeneratedEnemyUnit(-16,-16,27,12,2), new GeneratedEnemyUnit(6,-2,56,27,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "fab3461dfe69dd16322789c593d7088d9a3e5b3c4d7f0204e62b541806d1bd6d");
        }

        private static void Case_02964()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2964,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-18,41,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,15,62,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,0,56,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-15,49,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,12,8,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-6,88,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-11,45,20,4), new GeneratedEnemyUnit(19,-18,11,1,4), new GeneratedEnemyUnit(-1,20,38,29,4), new GeneratedEnemyUnit(2,7,60,6,4), new GeneratedEnemyUnit(1,3,63,9,1), new GeneratedEnemyUnit(7,17,94,47,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "2560d639391d5ff7ee52a2be34787f70a0bce34c9b07deae848e4778d58c11ac");
        }

        private static void Case_02965()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2965,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-6,84,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-20,48,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,17,21,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,20,66,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-4,83,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-3,8,24,2), new GeneratedEnemyUnit(-9,6,25,31,2), new GeneratedEnemyUnit(-9,18,53,19,1), new GeneratedEnemyUnit(-10,9,35,30,1), new GeneratedEnemyUnit(14,1,25,42,1), new GeneratedEnemyUnit(9,9,31,42,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "3c37c8d582ca36bb62ad0484c86b39d14a4c4a0884a06a5486ddd09d2101975a");
        }

        private static void Case_02966()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2966,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,7,92,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,19,16,9,2), new GeneratedEnemyUnit(-9,-12,9,30,3), new GeneratedEnemyUnit(20,4,96,6,3), new GeneratedEnemyUnit(10,-20,78,23,2), new GeneratedEnemyUnit(-15,-5,67,41,3), new GeneratedEnemyUnit(20,9,90,44,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "a0af9612855a40fb0e05b1c03b50915adb170d5a766532b740b1d6f2ae5a9afc");
        }

        private static void Case_02967()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2967,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,0,64,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-14,23,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,2,91,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,2,57,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-8,27,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-12,12,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,16,91,30,4), new GeneratedEnemyUnit(-7,2,96,22,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c986289786656077d8d0cba3177ddc9de98d76e4556df8064c715bfa09b6b1cb");
        }

        private static void Case_02968()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2968,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-14,99,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,12,51,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,15,70,25,1), new GeneratedEnemyUnit(-12,1,92,25,3), new GeneratedEnemyUnit(-18,5,46,9,1), new GeneratedEnemyUnit(18,4,78,19,4), new GeneratedEnemyUnit(6,12,17,26,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "bfdd46c545561cd3d071c1265417605da39b61350ce68340bb4b7d8c817a2f93");
        }

        private static void Case_02969()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2969,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,19,76,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,19,71,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-8,60,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,2,83,18,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "f12b1d260e375e9cf88169adb1939ba8faa9b575ec86c38f1e903e84c4bc7825");
        }

        private static void Case_02970()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2970,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,17,69,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-7,86,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,20,60,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-20,45,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-13,31,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-3,47,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,20,48,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,1,53,30,4), new GeneratedEnemyUnit(17,-17,80,29,4), new GeneratedEnemyUnit(9,-7,56,47,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "739549c78615e29a57a7c2ccc2524074f471461223e46255d96813b05efea22f");
        }

        private static void Case_02971()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2971,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-12,55,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-18,42,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-4,65,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,4,18,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,12,62,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-9,22,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-11,30,6,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "4b5e9b56076f119a5d62a760297568da318144f50b82ba86e710af73bedef9f5");
        }

        private static void Case_02972()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2972,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-3,22,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,20,8,25,3), new GeneratedEnemyUnit(7,-5,71,3,2), new GeneratedEnemyUnit(14,-20,31,22,4), new GeneratedEnemyUnit(-15,18,60,41,4), new GeneratedEnemyUnit(-20,4,23,32,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "377fa11b6568ed04e4115da562fa87b0b8b9a7398a3dfa4c5a109069de5fce02");
        }

        private static void Case_02973()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2973,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-4,73,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,20,67,22,3), new GeneratedEnemyUnit(0,7,64,42,1), new GeneratedEnemyUnit(16,-5,54,45,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b3a8c1c6ee74f3dd1deb44b428f175507c6d02c710709e36b295f4d4c227b2f7");
        }

        private static void Case_02974()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2974,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,1,31,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-16,24,38,3), new GeneratedEnemyUnit(6,3,56,43,2), new GeneratedEnemyUnit(-10,-7,20,4,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "2700cdbfcdb5b8924092de898e72e1c26c4371cb50d076d027b92c314354a84e");
        }

        private static void Case_02975()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2975,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-1,20,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,1,34,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,14,77,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,19,13,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,6,33,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,4,78,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-3,84,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,11,47,31,3), new GeneratedEnemyUnit(1,-19,80,4,4), new GeneratedEnemyUnit(-15,-11,19,25,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "9ce489ff4d1b28bf70b8a8507ed6c205cadad00fadc99cb24988286c83480f1e");
        }

        private static void Case_02976()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2976,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-10,57,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,5,49,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-19,70,10,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "fea3e9c28b37677210f9a8ce20fb1bf8c2c7da742c7b4634baba747ce34a13c7");
        }

        private static void Case_02977()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2977,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-13,67,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,0,73,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-9,98,23,2), new GeneratedEnemyUnit(7,-5,30,32,4), new GeneratedEnemyUnit(8,6,46,8,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "124592b87bf21917ba756763f644e7afd73f59099d68bf58fffc16f2c511d308");
        }

        private static void Case_02978()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2978,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-3,69,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-20,11,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-10,13,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,14,27,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-6,46,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,12,74,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,0,90,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,0,51,22,4), new GeneratedEnemyUnit(-16,8,32,14,2), new GeneratedEnemyUnit(-9,-19,13,20,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "705c5e857c94129ef15dcea05485628e671b0b9983e80b57f1768f5868a36ee2");
        }

        private static void Case_02979()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2979,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,6,66,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,14,83,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,10,90,4,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ab85833120e4148d2f94017dff205b4c5ea44660b22d5eddd39670fe0f329e40");
        }

        private static void Case_02980()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2980,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,3,30,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,4,27,31,2), new GeneratedEnemyUnit(-19,-19,72,31,4), new GeneratedEnemyUnit(-20,0,20,5,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "bdc2fa1f04b0acf3a1eed81e977505ae1fec6451fe4d2b891adf0dfbd6bd77d6");
        }

        private static void Case_02981()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2981,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,3,74,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,0,100,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,15,92,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,10,8,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,9,9,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,14,33,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,11,52,6,4), new GeneratedEnemyUnit(-16,-2,44,45,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "1c1a9c4300e5909e89338f8937f23a3842864738e1b2bb935709ec98751c5fc1");
        }

        private static void Case_02982()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2982,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-4,90,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-16,77,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,12,18,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-6,43,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-12,18,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-5,20,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,4,80,20,4), new GeneratedEnemyUnit(7,-12,77,5,2), new GeneratedEnemyUnit(-16,6,96,18,2), new GeneratedEnemyUnit(11,3,93,47,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "dbf98a5d505c0b5e16d5f0156d79eece5c5f8d7057488d30b7d12ca35651ca79");
        }

        private static void Case_02983()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2983,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,5,56,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,7,24,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-4,63,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,19,21,45,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "192bc57afbde83720e0e34fe2952da5ef6246d649dd5432ab83b7fd9cf9a3897");
        }

        private static void Case_02984()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2984,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-13,74,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-4,30,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-17,50,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,13,11,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-10,36,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-4,24,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-5,91,38,2), new GeneratedEnemyUnit(10,12,42,17,3), new GeneratedEnemyUnit(2,-5,39,4,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "96c64b86bd88310c6b86d7cb2639d075944bb972b1fcc7abe3061ed18ece048e");
        }

        private static void Case_02985()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2985,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-1,5,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,18,24,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,12,86,22,3), new GeneratedEnemyUnit(10,13,67,44,4), new GeneratedEnemyUnit(-10,19,44,46,3), new GeneratedEnemyUnit(-3,-13,31,42,1), new GeneratedEnemyUnit(9,7,22,48,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "c0fdb51231b7f8cd740921ef9909e34693c5c4463c40cdf19cd55d7fedf8185e");
        }

        private static void Case_02986()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2986,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,20,88,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,0,18,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,1,67,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-2,11,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,1,73,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,4,32,43,4), new GeneratedEnemyUnit(0,-3,19,1,1), new GeneratedEnemyUnit(11,13,12,49,4), new GeneratedEnemyUnit(-13,-17,34,22,4), new GeneratedEnemyUnit(-14,12,77,3,1), new GeneratedEnemyUnit(10,-6,57,2,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "cff4f48b51f1646c808617609b69d30d278f3a97add7e49f9f7a1825bdc7e8af");
        }

        private static void Case_02987()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2987,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-18,66,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,8,15,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,1,67,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,14,66,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,16,90,38,1), new GeneratedEnemyUnit(17,-12,55,5,2), new GeneratedEnemyUnit(18,0,27,3,4), new GeneratedEnemyUnit(-5,20,27,31,1), new GeneratedEnemyUnit(-1,-16,21,30,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "210ceabb043e0ade2e9191183dd06d9ab53f2a889a8a08ecaee6c01e3522f161");
        }

        private static void Case_02988()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2988,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,3,11,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-5,23,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-19,14,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-4,9,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,0,62,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,1,87,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-18,67,30,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "62e1475e48762b5ccdfe81ce3a4d07027e8b18813afe4f1d8fbc9c521ce15e94");
        }

        private static void Case_02989()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2989,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,20,19,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,6,96,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,2,52,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-14,80,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,3,9,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,18,72,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-15,48,6,3), new GeneratedEnemyUnit(-9,10,30,44,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "771e04b28ef72355f04c8c52ddb0fbb56be5b20211dfe66f8072769f193f20a2");
        }

        private static void Case_02990()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2990,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,16,59,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,5,10,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,10,81,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-3,21,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,6,41,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-5,91,18,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "0fe80d82dfed5fd9298691e1579429ba67eea1d93de16dd1912d95089f0f2c12");
        }

        private static void Case_02991()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2991,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,12,37,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-18,72,2,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "89d6b11b06f3beeee22df6d2931442f65ce1688facf60fd17bc925583648a4f0");
        }

        private static void Case_02992()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2992,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,13,65,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-12,31,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-14,55,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-4,65,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,12,81,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,20,76,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,3,73,15,4), new GeneratedEnemyUnit(7,-18,64,47,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "76ab6d9fb49cfca1b152f05508f6c6c5fc88bc1d27acbbbb762275ce0606d437");
        }

        private static void Case_02993()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2993,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-4,16,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,6,15,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-6,86,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-17,45,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-13,95,21,4), new GeneratedEnemyUnit(4,-9,12,45,4), new GeneratedEnemyUnit(-20,-9,86,1,1), new GeneratedEnemyUnit(7,-20,15,11,1), new GeneratedEnemyUnit(1,17,77,13,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "cd02b1f32e14ae9736051758c50c66fd2da89351d470b5053038df034a51d774");
        }

        private static void Case_02994()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2994,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,18,39,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-18,58,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-4,73,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,4,11,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,1,25,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,3,100,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,15,65,38,3), new GeneratedEnemyUnit(-5,-9,25,12,3), new GeneratedEnemyUnit(1,-7,86,17,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "cc625af04da6ef3d4211c343640238b533e2671ca7c1359faa062e5cebf0e6a6");
        }

        private static void Case_02995()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2995,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,12,91,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-14,99,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-18,38,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,18,78,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,3,93,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,6,45,33,2), new GeneratedEnemyUnit(13,4,98,25,1), new GeneratedEnemyUnit(-13,5,23,1,3), new GeneratedEnemyUnit(5,-2,47,48,4), new GeneratedEnemyUnit(16,-6,77,9,1), new GeneratedEnemyUnit(3,-19,49,2,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "e67e7154eb3c1fd15d2d854d270b5e9d1a8d69858e735a474e694dde5a25021f");
        }

        private static void Case_02996()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2996,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,7,61,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,13,82,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-8,77,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,4,80,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,4,7,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,11,55,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-1,31,32,4), new GeneratedEnemyUnit(-14,11,26,4,1), new GeneratedEnemyUnit(15,-9,40,25,3), new GeneratedEnemyUnit(16,-9,69,27,1), new GeneratedEnemyUnit(5,-5,54,36,2), new GeneratedEnemyUnit(-6,-11,16,31,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "17a89f77cf8aee50f0448b5ea3b71d687b6454044c23829c7a7ab5f399b3b66d");
        }

        private static void Case_02997()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2997,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-1,12,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,18,21,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-7,97,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-18,36,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-11,53,1,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "6d149caa2dbed055be16c35ffa6e2ffe3c745ce2cfd56e3ac6618fc9d87960c7");
        }

        private static void Case_02998()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2998,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,0,87,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-3,68,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-5,34,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,11,47,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-6,50,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-4,82,38,4), new GeneratedEnemyUnit(-16,-20,42,39,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "9daf91b3055aa9c83ffaf31dd86f98311763bbbae695c916d7ac4e958a4a92e8");
        }

        private static void Case_02999()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2999,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-15,27,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,6,63,7,2), new GeneratedEnemyUnit(5,-5,68,36,3), new GeneratedEnemyUnit(-15,0,93,30,1), new GeneratedEnemyUnit(6,6,32,21,1), new GeneratedEnemyUnit(-17,14,16,32,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "a9ddc233b0b6424482047f5fd999a259a4ff521a7032b93e080c6a744112788f");
        }

        private static void Case_03000()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3000,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-6,48,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,14,6,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-15,42,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,15,35,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-9,35,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,10,66,38,3), new GeneratedEnemyUnit(-10,11,34,29,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "fc6a05b962268fb4e66ef49405c3fd210a753a9f6afa5bf4f13501bbb81fe321");
        }

        private static void Case_03001()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3001,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-10,39,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,7,71,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-14,6,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,16,41,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,7,5,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,17,63,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-9,34,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-17,18,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,7,67,8,4), new GeneratedEnemyUnit(-15,17,57,49,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "9723ce703ac67873fcd5d9d81bc45155196c0674d31ac96aeed1b920ae4652a7");
        }

        private static void Case_03002()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3002,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,9,15,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-7,43,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,7,19,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,16,81,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-10,45,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,2,70,6,2), new GeneratedEnemyUnit(-1,1,51,36,1), new GeneratedEnemyUnit(-5,-11,42,17,2), new GeneratedEnemyUnit(-9,17,41,35,3), new GeneratedEnemyUnit(17,-9,41,31,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "6edaf6c29a6569feaf01869faf9de0bf7266db8969b29dc34e8d839a10482c05");
        }

        private static void Case_03003()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3003,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-9,42,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-4,30,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,12,28,23,4), new GeneratedEnemyUnit(-17,17,75,39,4), new GeneratedEnemyUnit(-10,10,10,47,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "29b2a6b1b48d96f32ea6ea069c24601d1440d637b1ffcb42e974adf41e7d5664");
        }

        private static void Case_03004()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3004,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,13,30,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,20,41,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-13,85,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-11,90,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,2,31,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-15,23,25,4), new GeneratedEnemyUnit(-9,-20,74,18,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "4af1dc5c02698c8b2cf414529930dc9800d6d1c47eb0580770018cf14e7ded4d");
        }

        private static void Case_03005()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3005,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-8,61,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,10,40,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-1,95,27,3), new GeneratedEnemyUnit(16,-2,18,39,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "6f9dad2ffe2dd8cd66a7d3890cf4a919cf4d8816e33ab9d660f2cd0362db559c");
        }

        private static void Case_03006()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3006,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-7,37,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-11,85,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,9,10,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-10,25,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-9,100,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,16,88,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-8,86,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,13,78,44,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "a5dc4f1e051a52cea61bf8aae75a4b638dd7e8c43b08759811df75fef8e35925");
        }

        private static void Case_03007()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3007,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-2,88,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,0,75,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,10,43,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,7,72,12,3), new GeneratedEnemyUnit(3,14,44,15,2), new GeneratedEnemyUnit(7,-3,30,19,4), new GeneratedEnemyUnit(-18,-11,55,47,3), new GeneratedEnemyUnit(1,-16,69,50,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "ac813af00a3c01e15239cc929d013a2d72204741084819d0642c6e3bfe0d66b6");
        }

        private static void Case_03008()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3008,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-2,9,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,4,64,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-18,27,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-3,66,23,2), new GeneratedEnemyUnit(-20,12,7,49,3), new GeneratedEnemyUnit(-4,19,63,8,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "b597a5f31b6d4f55defb0087b362a482c072a22d6efbea0812d45a1f22500083");
        }

        private static void Case_03009()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3009,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-11,49,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,8,76,49,1), new GeneratedEnemyUnit(-20,-17,80,36,3), new GeneratedEnemyUnit(-10,20,6,29,4), new GeneratedEnemyUnit(7,-1,82,5,4), new GeneratedEnemyUnit(11,-14,13,21,4), new GeneratedEnemyUnit(15,2,24,10,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "86e050c77b2c2a93d3010807e4ae9b8f3f720dba40a5e304d88c6329176a9f2f");
        }

        private static void Case_03010()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3010,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-12,48,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,9,13,1,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "9b2e6bff0e488cb3a1f9d2c8fbcddb057c8008bde5c6767da867074799cce28e");
        }

        private static void Case_03011()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3011,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,1,53,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,1,44,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,1,19,7,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "ef92d08f26da4a857f59344d675da6d131cac2a9d93d1f43cabf0d13db4b00cf");
        }

        private static void Case_03012()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3012,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-16,38,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-13,66,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-6,39,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,2,37,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,4,9,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,12,37,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-5,14,34,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a464ef1a63548df2a4aa31bd09a88e31cdbe841fedf13e513e1fbce346c20f5a");
        }

        private static void Case_03013()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3013,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,17,71,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-20,74,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,12,81,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-16,81,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,19,59,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-10,93,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-8,66,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,13,87,9,1), new GeneratedEnemyUnit(17,14,53,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "57590d68ec01c7df690afed7015ce2b6caf2395d8acc5f7805230bbb3f92d2d7");
        }

        private static void Case_03014()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3014,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-6,93,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,14,34,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,7,84,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,9,34,2,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "a3a29436bfbf34368ffc454de3057df5980110b21ed7c74efa91ef0d2ae6e8e2");
        }

        private static void Case_03015()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3015,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,13,30,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-14,96,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,0,100,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,6,22,38,4), new GeneratedEnemyUnit(-16,17,8,35,4), new GeneratedEnemyUnit(-13,5,10,1,1), new GeneratedEnemyUnit(19,11,93,27,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "3601a22412173c870dbc49ed1c777931a3199db7c4a7a810567861c9c685f3b5");
        }

        private static void Case_03016()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3016,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-4,25,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,11,59,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-8,12,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-17,34,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-15,93,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-1,5,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-1,64,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,4,57,5,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "447f956ab6d2edca0dacd64ab39c696f741feb487325b94c77f166a6a3ff632d");
        }

        private static void Case_03017()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3017,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-5,33,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,11,8,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-12,33,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-17,34,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,5,67,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,19,25,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-10,94,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,1,46,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-19,11,43,1), new GeneratedEnemyUnit(-9,-18,8,31,2), new GeneratedEnemyUnit(-2,12,22,20,2), new GeneratedEnemyUnit(0,15,23,36,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "cb7ae9f54c3bdce4dea0e871aaf7dcab7bbcaf4a886c02ccd6f43da6fa5ef723");
        }

        private static void Case_03018()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3018,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,14,12,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-12,40,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,20,25,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,18,34,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-14,6,32,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e2c2b75cebc5f4270638ce0bb464482b62c0e400e3c363f5f5e235a41c2b836b");
        }

        private static void Case_03019()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3019,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,7,40,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,6,42,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-1,96,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,19,79,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,8,70,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,15,91,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,13,6,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,13,60,2,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "e2ef8469f8cc9e73335ec9b50d5b0642eb50f25249f6caee4c30787f190ea2ef");
        }

        private static void Case_03020()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3020,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,16,21,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,8,10,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-6,16,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,2,45,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-19,35,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-16,6,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,8,23,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,20,42,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-17,29,42,3), new GeneratedEnemyUnit(6,-13,64,17,3), new GeneratedEnemyUnit(-18,17,84,44,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "dde65bb5b30ddc5eaa48365aebea11bf1ba238846f12d7c70bb764cf5fab7b0f");
        }

        private static void Case_03021()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3021,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,6,75,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,3,45,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "5cd9d59a46cf6b46806be0545edac2f7833a2af23c91ca5c61bc73d1f389860b");
        }

        private static void Case_03022()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3022,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-3,72,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-10,73,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-2,34,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-14,89,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,12,86,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,15,28,19,1), new GeneratedEnemyUnit(-16,-5,89,16,1), new GeneratedEnemyUnit(-20,-3,6,13,2), new GeneratedEnemyUnit(8,7,38,2,4), new GeneratedEnemyUnit(-14,-20,97,46,1), new GeneratedEnemyUnit(14,-15,20,45,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "1cff58ec8f32593e50c1ad91f683e3d429a469208611c0bc555ad870b5779cdd");
        }

        private static void Case_03023()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3023,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,14,26,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,1,74,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,11,87,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,7,73,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-2,12,40,1), new GeneratedEnemyUnit(6,-17,28,23,1), new GeneratedEnemyUnit(14,4,32,48,2), new GeneratedEnemyUnit(-4,-14,100,23,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "bcc19f62b2df500520ffd0d7ce3cbc301a61118ff3c0a65761265d0ed3d550a8");
        }

        private static void Case_03024()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3024,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,10,33,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-19,85,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,18,35,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-12,49,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-15,55,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-15,67,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,5,8,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,14,29,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,1,12,8,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "f501f15980169679193ac47f203b982e9e91e5fd0ac21607c2b3b9bb572e4601");
        }

        private static void Case_03025()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3025,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,18,48,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-6,23,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-5,77,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,5,65,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,9,75,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,12,36,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,17,55,45,2), new GeneratedEnemyUnit(11,-16,7,48,3), new GeneratedEnemyUnit(-13,7,55,46,1), new GeneratedEnemyUnit(-19,-3,37,45,2), new GeneratedEnemyUnit(18,-6,31,26,1), new GeneratedEnemyUnit(17,10,67,29,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "9b1adcb4c9168ff4b370737f58236dd042ef9f6f239cf691efc75f2933503987");
        }

        private static void Case_03026()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3026,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-3,11,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-6,96,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,17,96,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,11,55,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-5,80,2,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "d3df1b1f670783d0abfe27388d9b6e3868a3f60a7ac829769eb9296baaa11b58");
        }

        private static void Case_03027()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3027,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,15,97,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,0,11,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,8,42,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,2,32,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-10,76,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-3,64,13,1), new GeneratedEnemyUnit(0,-16,21,6,4), new GeneratedEnemyUnit(-5,10,58,6,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "822f810efa0657b548b04252cb4ce3aa2b1d5f938f51204961fc1b5ca7185c00");
        }

        private static void Case_03028()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3028,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-12,85,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-11,61,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-7,60,37,3), new GeneratedEnemyUnit(16,-8,87,5,3), new GeneratedEnemyUnit(-19,9,89,20,4), new GeneratedEnemyUnit(-1,-9,25,28,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "e07924b33193ded47da4bd64da5fd90049c0ada73a4cf70037cb90d386eca4b0");
        }

        private static void Case_03029()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3029,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,15,31,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,1,42,30,2), new GeneratedEnemyUnit(-10,15,80,35,4), new GeneratedEnemyUnit(2,-11,37,37,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "e958f6e8f65d52b0b7a3599d197f49c0f82e75a3f6bf70b364e4541455fff4b6");
        }

        private static void Case_03030()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3030,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,19,24,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-13,96,43,2), new GeneratedEnemyUnit(-14,-19,80,9,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "0f8819e96930cab91bd8e57e498e7a64ff6d4234c33be6e33fde7f27edfc9544");
        }

        private static void Case_03031()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3031,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-16,59,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,14,34,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-14,29,17,3), new GeneratedEnemyUnit(20,3,52,13,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f16f80606d631d7ff15b55ade29356b02925176c438d80d16aa68d2b02aa20e4");
        }

        private static void Case_03032()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3032,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-20,29,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,8,45,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,11,54,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,16,54,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,11,54,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,0,79,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,2,83,47,1), new GeneratedEnemyUnit(-15,3,31,20,1), new GeneratedEnemyUnit(2,-15,70,34,2), new GeneratedEnemyUnit(6,15,96,13,2), new GeneratedEnemyUnit(-19,0,54,19,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "e9d8965e56d658e57fe6c67c686611e5e2a4776f3f6d24563757471be80004e4");
        }

        private static void Case_03033()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3033,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,4,60,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-6,58,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,10,93,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-19,84,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-18,57,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-17,61,6,4), new GeneratedEnemyUnit(4,10,26,7,3), new GeneratedEnemyUnit(5,-5,21,49,3), new GeneratedEnemyUnit(-14,-4,13,7,3), new GeneratedEnemyUnit(-11,-11,21,31,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "7f226465503402aa61822dd4a66c38f5538f4f630aea3931047ed0b514991aca");
        }

        private static void Case_03034()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3034,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,18,40,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,8,6,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-20,44,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-4,59,26,4), new GeneratedEnemyUnit(3,15,86,32,2), new GeneratedEnemyUnit(7,-6,31,6,4), new GeneratedEnemyUnit(13,-4,59,7,1), new GeneratedEnemyUnit(-14,14,14,7,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "22a860068d0c03db5fe09ff62ed3cf72f8619984bcfca1e6e3e9d0ef6392e6ff");
        }

        private static void Case_03035()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3035,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-15,63,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,5,46,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-3,47,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-14,75,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-2,37,2,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "1db042cf6e2718edd596f4b9eece8ab89bb80d44f526a6809ce85b79aa7e4a9d");
        }

        private static void Case_03036()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3036,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,9,90,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,5,29,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,8,29,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-10,79,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,20,99,7,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "eda0b25dfb1f0bc774f451398492ef5ebe239e25d55282432dad2e8cae66779f");
        }

        private static void Case_03037()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3037,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,10,74,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-13,90,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,6,86,45,2), new GeneratedEnemyUnit(13,19,91,21,3), new GeneratedEnemyUnit(8,2,91,45,3), new GeneratedEnemyUnit(-18,-14,99,1,2), new GeneratedEnemyUnit(5,-4,96,28,3), new GeneratedEnemyUnit(19,19,95,2,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "2b4b4b5f91c88c7424e112871db9fd7d81778d2d3d306c079fd8db31e1162f57");
        }

        private static void Case_03038()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3038,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-8,57,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,19,10,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-6,13,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-15,75,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-13,7,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-5,51,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-16,13,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-11,84,7,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "646056b687113b4e449fcaaa23cd1534bd5693eca1156d55b619fe7732e95e82");
        }

        private static void Case_03039()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3039,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-16,33,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-6,97,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-2,56,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,16,10,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-19,73,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-17,12,23,3), new GeneratedEnemyUnit(4,-11,43,15,1), new GeneratedEnemyUnit(-20,-10,32,17,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "bc8edf77bede83c65e32f282dfbab4c1cd3826e65656f19cdab308483f55ea78");
        }

        private static void Case_03040()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3040,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-8,27,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-1,97,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,1,10,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-20,90,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-19,30,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-11,79,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-10,8,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-14,56,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,14,79,26,4), new GeneratedEnemyUnit(12,-8,77,8,1), new GeneratedEnemyUnit(6,5,63,21,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "9912b23405d64fc491557e3d17a809383b126436c3c1d1b8b2b822415f835ed2");
        }

        private static void Case_03041()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3041,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-6,55,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-9,55,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-15,61,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-18,38,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,2,79,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,3,16,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,15,49,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-18,63,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,8,39,18,2), new GeneratedEnemyUnit(-1,14,82,34,3), new GeneratedEnemyUnit(-2,8,86,8,3), new GeneratedEnemyUnit(6,-12,19,35,4), new GeneratedEnemyUnit(14,18,47,38,1), new GeneratedEnemyUnit(20,8,9,41,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "3668d9e019a92723b14e6de3ea8768c4bb5fc3e6f4e698e3af8c64cb74ab6df6");
        }

        private static void Case_03042()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3042,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-13,95,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-15,17,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-1,99,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,11,45,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,2,78,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,19,83,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,19,61,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,18,21,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-10,64,42,1), new GeneratedEnemyUnit(13,-10,17,25,1), new GeneratedEnemyUnit(10,-7,55,40,3), new GeneratedEnemyUnit(-16,-20,64,6,2), new GeneratedEnemyUnit(19,-5,38,18,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "296f6fc2dce04c40030745717a28bd889b96f01c098ad7ec2a6e251e3c540885");
        }

        private static void Case_03043()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3043,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-3,53,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-17,86,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-17,100,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-20,34,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-15,20,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,3,90,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-14,7,12,3), new GeneratedEnemyUnit(6,-6,15,13,4), new GeneratedEnemyUnit(18,5,63,3,2), new GeneratedEnemyUnit(20,-3,67,39,4), new GeneratedEnemyUnit(2,-13,41,7,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "1fd328ba4e69bc2f085ed1f80bd383277f722449349133cad5da00a86a017530");
        }

        private static void Case_03044()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3044,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-4,11,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,8,71,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-15,28,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-11,88,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-17,85,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,8,99,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,18,13,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,14,26,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-14,25,7,3), new GeneratedEnemyUnit(-18,6,98,40,4), new GeneratedEnemyUnit(20,-13,44,23,3), new GeneratedEnemyUnit(3,-6,59,5,4), new GeneratedEnemyUnit(12,-20,40,7,3), new GeneratedEnemyUnit(7,12,27,2,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "ae92ed1abc9d6c1caa1abd38f1c8d6343393648e6c34b6ebc153ca794b13e883");
        }

        private static void Case_03045()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3045,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,11,94,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,4,54,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,18,58,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-20,12,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-9,52,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,7,93,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,7,62,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,6,87,10,2), new GeneratedEnemyUnit(-15,20,36,19,2), new GeneratedEnemyUnit(-6,-14,55,40,3), new GeneratedEnemyUnit(13,4,42,39,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "1cc6c8d418b7670cf92b0b0fa03dcb016e8f157911bfc15cb37d01aaaa29d627");
        }

        private static void Case_03046()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3046,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,10,50,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,1,78,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-17,10,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-13,11,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,16,24,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-19,85,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-1,44,31,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "371d898b9b5142bce20accbfe3527f131252d54757c4d0989d9890b43371223c");
        }

        private static void Case_03047()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3047,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,1,89,3,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "ba929d8a35f55fd912cff6e4c46826de5e1ef3cb24c8ac206ce958438d78d4fb");
        }

        private static void Case_03048()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3048,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,12,83,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-1,99,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,7,50,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,19,67,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,10,62,5,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "b9f8004af657f6e6bebdd8c59208f40100f911718e53f71d1eba3785205c22cd");
        }

        private static void Case_03049()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3049,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,14,63,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,12,51,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,13,82,37,4), new GeneratedEnemyUnit(-2,19,89,29,2), new GeneratedEnemyUnit(-2,6,14,50,2), new GeneratedEnemyUnit(7,1,15,18,4), new GeneratedEnemyUnit(10,-13,10,48,1), new GeneratedEnemyUnit(-2,16,35,31,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "22d9e49ba8360125dd31d5a9247aac7f99880df09291a4469166ce477fca0f56");
        }

        private static void Case_03050()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3050,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-13,29,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-10,83,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-19,80,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,16,11,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-13,35,5,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c7a101299e750bed0472580e540470d8cee3e48ca314808c737fcca0b80d3015");
        }

        private static void Case_03051()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3051,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,2,5,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,0,70,48,2), new GeneratedEnemyUnit(10,11,61,32,3), new GeneratedEnemyUnit(-6,17,59,39,1), new GeneratedEnemyUnit(-9,-20,78,19,2), new GeneratedEnemyUnit(-14,17,36,48,2), new GeneratedEnemyUnit(-17,-2,7,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "931e3f6fd4d42867c8174c525184b78ef88c09b348425a60e3be5b3b2303e3f8");
        }

        private static void Case_03052()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3052,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-5,74,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,11,49,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-8,63,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-15,29,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-2,67,32,1), new GeneratedEnemyUnit(-1,1,74,4,3), new GeneratedEnemyUnit(7,-5,98,23,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "55f886e7fc0fb2831174e19fdd66a1e00b7f92f34f00b3aa77f588614585e77e");
        }

        private static void Case_03053()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3053,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,4,39,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-6,89,14,2), new GeneratedEnemyUnit(-3,9,14,48,3), new GeneratedEnemyUnit(-12,15,80,32,4), new GeneratedEnemyUnit(-15,4,84,2,4), new GeneratedEnemyUnit(18,-12,92,41,3), new GeneratedEnemyUnit(-2,-4,82,42,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "11bd4e26549af760513bd1c6cb9b539d3d69a89c55149733bf7b885f8c0c8a7a");
        }

        private static void Case_03054()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3054,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,17,8,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-1,58,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-6,21,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-7,37,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,13,33,36,3), new GeneratedEnemyUnit(-20,-4,5,18,4), new GeneratedEnemyUnit(-14,3,86,39,4), new GeneratedEnemyUnit(-19,-17,21,25,3), new GeneratedEnemyUnit(16,4,45,11,2), new GeneratedEnemyUnit(8,-4,60,20,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "273d83942c49fb097c3fa98b80e05098b09153560a5dc1c7bc30f010a8459309");
        }

        private static void Case_03055()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3055,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,3,43,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,1,91,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,20,66,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,8,53,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-3,97,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,20,30,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,6,93,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-4,34,4,3), new GeneratedEnemyUnit(19,15,19,22,1), new GeneratedEnemyUnit(-12,2,68,50,4), new GeneratedEnemyUnit(13,17,79,46,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b296f85ec90fdee905e89a3b6f5611e1ff4c3d692540b3d6b1de3d0ac4cac0e4");
        }

        private static void Case_03056()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3056,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-13,59,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-7,8,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-8,70,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,8,17,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-19,22,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,5,97,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,2,69,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,12,8,29,4), new GeneratedEnemyUnit(4,-18,68,35,3), new GeneratedEnemyUnit(3,16,17,25,1), new GeneratedEnemyUnit(-11,12,55,22,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0a620c362b5c70a519724ebe10ae94d4fec5a83e133504d66a479540f6aa12f1");
        }

        private static void Case_03057()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3057,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,6,22,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,15,64,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-17,36,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-18,45,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,5,89,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-19,70,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,13,50,2,1), new GeneratedEnemyUnit(-8,-7,18,14,2), new GeneratedEnemyUnit(10,-6,81,25,4), new GeneratedEnemyUnit(0,-9,30,34,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "f36ceb65f05719b0de1544051ae6f02bd391c5a5ced91eafd573cf736de73360");
        }

        private static void Case_03058()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3058,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,11,27,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,15,60,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,9,60,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,1,95,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,16,40,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-14,94,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-2,52,48,3), new GeneratedEnemyUnit(-18,-16,47,8,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "e7730347be141287bc85213640ebf4eb4db846db33e21b21281ec0e7d7b5ab87");
        }

        private static void Case_03059()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3059,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-5,40,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,16,54,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-7,39,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-6,17,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,12,15,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,15,22,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,1,82,25,2), new GeneratedEnemyUnit(-6,-15,23,4,2), new GeneratedEnemyUnit(-4,-12,44,28,4), new GeneratedEnemyUnit(1,-17,14,47,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "92bbd4f5f9e71c150bb5fb6a25d6c4164c5fdffecd2a093a4e2a11ac093009bb");
        }

    }
}
