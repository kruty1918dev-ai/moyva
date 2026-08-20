using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard019
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_03420();
            Case_03421();
            Case_03422();
            Case_03423();
            Case_03424();
            Case_03425();
            Case_03426();
            Case_03427();
            Case_03428();
            Case_03429();
            Case_03430();
            Case_03431();
            Case_03432();
            Case_03433();
            Case_03434();
            Case_03435();
            Case_03436();
            Case_03437();
            Case_03438();
            Case_03439();
            Case_03440();
            Case_03441();
            Case_03442();
            Case_03443();
            Case_03444();
            Case_03445();
            Case_03446();
            Case_03447();
            Case_03448();
            Case_03449();
            Case_03450();
            Case_03451();
            Case_03452();
            Case_03453();
            Case_03454();
            Case_03455();
            Case_03456();
            Case_03457();
            Case_03458();
            Case_03459();
            Case_03460();
            Case_03461();
            Case_03462();
            Case_03463();
            Case_03464();
            Case_03465();
            Case_03466();
            Case_03467();
            Case_03468();
            Case_03469();
            Case_03470();
            Case_03471();
            Case_03472();
            Case_03473();
            Case_03474();
            Case_03475();
            Case_03476();
            Case_03477();
            Case_03478();
            Case_03479();
            Case_03480();
            Case_03481();
            Case_03482();
            Case_03483();
            Case_03484();
            Case_03485();
            Case_03486();
            Case_03487();
            Case_03488();
            Case_03489();
            Case_03490();
            Case_03491();
            Case_03492();
            Case_03493();
            Case_03494();
            Case_03495();
            Case_03496();
            Case_03497();
            Case_03498();
            Case_03499();
            Case_03500();
            Case_03501();
            Case_03502();
            Case_03503();
            Case_03504();
            Case_03505();
            Case_03506();
            Case_03507();
            Case_03508();
            Case_03509();
            Case_03510();
            Case_03511();
            Case_03512();
            Case_03513();
            Case_03514();
            Case_03515();
            Case_03516();
            Case_03517();
            Case_03518();
            Case_03519();
            Case_03520();
            Case_03521();
            Case_03522();
            Case_03523();
            Case_03524();
            Case_03525();
            Case_03526();
            Case_03527();
            Case_03528();
            Case_03529();
            Case_03530();
            Case_03531();
            Case_03532();
            Case_03533();
            Case_03534();
            Case_03535();
            Case_03536();
            Case_03537();
            Case_03538();
            Case_03539();
            Case_03540();
            Case_03541();
            Case_03542();
            Case_03543();
            Case_03544();
            Case_03545();
            Case_03546();
            Case_03547();
            Case_03548();
            Case_03549();
            Case_03550();
            Case_03551();
            Case_03552();
            Case_03553();
            Case_03554();
            Case_03555();
            Case_03556();
            Case_03557();
            Case_03558();
            Case_03559();
            Case_03560();
            Case_03561();
            Case_03562();
            Case_03563();
            Case_03564();
            Case_03565();
            Case_03566();
            Case_03567();
            Case_03568();
            Case_03569();
            Case_03570();
            Case_03571();
            Case_03572();
            Case_03573();
            Case_03574();
            Case_03575();
            Case_03576();
            Case_03577();
            Case_03578();
            Case_03579();
            Case_03580();
            Case_03581();
            Case_03582();
            Case_03583();
            Case_03584();
            Case_03585();
            Case_03586();
            Case_03587();
            Case_03588();
            Case_03589();
            Case_03590();
            Case_03591();
            Case_03592();
            Case_03593();
            Case_03594();
            Case_03595();
            Case_03596();
            Case_03597();
            Case_03598();
            Case_03599();
        }

        private static void Case_03420()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3420,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,5,43,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-15,91,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-13,18,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-8,56,50,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "72967bae4545198dee91265539ba6ee2c1b84bf6d3ee33b62b23c0f319f9da36");
        }

        private static void Case_03421()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3421,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,12,51,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-12,37,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,3,98,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-12,51,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-20,19,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,7,86,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-20,95,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,11,74,22,4), new GeneratedEnemyUnit(13,-7,91,26,1), new GeneratedEnemyUnit(19,8,32,1,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "9b4068ca5f5a4b4268b5c7a88f40f159c097ad3f925ffc8cd9630a43cbdd2b9e");
        }

        private static void Case_03422()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3422,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-13,78,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-4,31,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-7,17,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,6,85,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-12,97,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,16,20,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-11,49,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-9,50,46,1), new GeneratedEnemyUnit(1,0,25,2,4), new GeneratedEnemyUnit(1,4,39,17,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "2df23ccd33919c4ae03fddb7e04d4eaa01a374c1609ebe24f1ddcec33e4a4e49");
        }

        private static void Case_03423()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3423,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-19,28,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-7,39,30,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "103d5b46279daa5afd2bb5dc6552130bcd2d77a40fa1fa42fb0f762b92b9e348");
        }

        private static void Case_03424()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3424,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,8,26,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,1,67,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-2,42,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-18,7,6,4), new GeneratedEnemyUnit(-17,10,38,40,3), new GeneratedEnemyUnit(-7,-17,13,34,4), new GeneratedEnemyUnit(9,-14,77,3,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "abc81fa81d159ba1a221bc1f9840af825fd5aaebc8ea43c69a693b85d60ecab1");
        }

        private static void Case_03425()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3425,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,12,31,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-3,50,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,20,20,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-7,38,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,13,79,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-16,36,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-18,84,34,1), new GeneratedEnemyUnit(4,6,89,30,1), new GeneratedEnemyUnit(-14,-19,92,24,2), new GeneratedEnemyUnit(-12,-8,14,36,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "9694a24fc2cfe5c93275b43a0b779d2ed9c557373b181806bd15693aad11a74b");
        }

        private static void Case_03426()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3426,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,17,31,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,15,31,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,5,67,12,4), new GeneratedEnemyUnit(20,-3,47,10,3), new GeneratedEnemyUnit(-6,-2,60,4,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "3fb5e8d0522be2a1e7bdf8a436e901a3de1008c73f4d110e289629028b864189");
        }

        private static void Case_03427()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3427,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,10,77,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-3,39,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,17,66,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-10,27,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,17,58,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-19,57,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,5,54,2,3), new GeneratedEnemyUnit(4,11,77,14,4), new GeneratedEnemyUnit(-2,-12,39,28,4), new GeneratedEnemyUnit(-18,20,41,47,2), new GeneratedEnemyUnit(-18,-7,14,31,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "58a724ca06271e9ed36e6c93c40344bd2547bee9fc2bef1f32d48523de5ec862");
        }

        private static void Case_03428()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3428,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-11,83,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,12,11,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-5,23,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,6,64,21,2), new GeneratedEnemyUnit(0,-3,86,4,3), new GeneratedEnemyUnit(13,5,14,18,2), new GeneratedEnemyUnit(-11,-10,33,18,2), new GeneratedEnemyUnit(-11,-1,90,39,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "d92a940bbc5ecdba0d941c3a94d0852aa8271a1182e27314ad892a7211499464");
        }

        private static void Case_03429()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3429,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,19,16,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-6,94,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,10,93,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,7,20,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-3,8,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-19,40,6,2), new GeneratedEnemyUnit(3,8,36,5,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "241c5e1b10b5e10af93b8059972144c4701e4543ed6efa5d931edc297e19fece");
        }

        private static void Case_03430()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3430,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,11,84,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,11,44,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-9,79,27,2), new GeneratedEnemyUnit(11,9,31,38,1), new GeneratedEnemyUnit(-17,1,67,44,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "a13a5771b877e0de048f137fd12db74c35b75e97caa29a6661a75d22d7fa7e4d");
        }

        private static void Case_03431()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3431,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,0,38,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,6,9,21,1), new GeneratedEnemyUnit(-13,11,20,37,4), new GeneratedEnemyUnit(8,7,26,8,1), new GeneratedEnemyUnit(-15,-2,59,46,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "59d717585ff7ff12cf8f5e9fd707a6d771e4c8571af87bfdd49d91e307276cf7");
        }

        private static void Case_03432()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3432,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-2,54,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,14,56,23,3), new GeneratedEnemyUnit(-12,-6,36,20,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "edfd05542aa32dba126757072e71cae09dd1c308b60b002798ca5113f67fa454");
        }

        private static void Case_03433()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3433,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,15,44,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,9,77,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,17,45,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,1,32,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,19,17,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-15,68,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-10,32,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,15,70,38,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "8cbdd81f272e551bdc3a3fbe834237797f1f33b7028c7957669c6fb3defe6859");
        }

        private static void Case_03434()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3434,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-10,86,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-9,79,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-19,36,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-7,95,44,4), new GeneratedEnemyUnit(-19,-19,38,16,1), new GeneratedEnemyUnit(-2,8,22,38,1), new GeneratedEnemyUnit(-5,-10,50,23,3), new GeneratedEnemyUnit(16,9,79,38,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "a71a30298997c42a54de0624c7ae26aa44156e25b4bb0b1b8361d940ea2ad623");
        }

        private static void Case_03435()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3435,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-13,71,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,17,27,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-16,22,46,2), new GeneratedEnemyUnit(-8,1,97,25,1), new GeneratedEnemyUnit(-2,-11,85,16,4), new GeneratedEnemyUnit(0,18,93,15,3), new GeneratedEnemyUnit(-4,13,26,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "a3012c9a155beba591e82017ae4387cf45424dc084016bcbf9cd1457d4fbc39d");
        }

        private static void Case_03436()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3436,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,15,36,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-8,26,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-8,13,7,4), new GeneratedEnemyUnit(-4,-19,80,29,4), new GeneratedEnemyUnit(2,-16,53,11,4), new GeneratedEnemyUnit(-11,10,27,3,2), new GeneratedEnemyUnit(-7,-6,10,36,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "f5a278487b3fa7a34229abe8509558c438df4a2e254815870dfdd88704ed773c");
        }

        private static void Case_03437()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3437,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-8,94,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,13,64,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,7,94,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-19,49,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,19,54,31,2), new GeneratedEnemyUnit(-17,14,59,12,1), new GeneratedEnemyUnit(3,14,54,18,1), new GeneratedEnemyUnit(0,9,25,20,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "e39643d65661a84735bf89a5be623e116cc476507720b7029f4f9e11ce436bd1");
        }

        private static void Case_03438()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3438,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-18,74,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,15,54,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,8,65,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-13,97,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,9,85,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,6,36,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,15,90,3,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "2a6784ea75de904031d23dfa47beea518e8c3829a48e4e0cf531ea581574d155");
        }

        private static void Case_03439()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3439,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,2,53,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,2,17,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,12,61,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-17,62,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-7,49,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-5,34,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,0,85,6,3), new GeneratedEnemyUnit(-19,18,79,48,3), new GeneratedEnemyUnit(16,13,40,14,3), new GeneratedEnemyUnit(12,4,53,27,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "a14ad7fb1df666ff9df49449f3b53798a1bc3e5a000bc5068cd0e92814a287b3");
        }

        private static void Case_03440()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3440,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-6,27,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,6,77,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,12,72,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,5,54,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,0,70,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-12,24,45,4), new GeneratedEnemyUnit(7,15,70,36,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e8d121be021a7e2b6c47a6543d52b21fdebf32384ca09ab4a8a7148b5f715b2b");
        }

        private static void Case_03441()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3441,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-3,35,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,12,32,43,1), new GeneratedEnemyUnit(2,-19,79,31,4), new GeneratedEnemyUnit(0,14,79,43,1), new GeneratedEnemyUnit(3,10,88,10,4), new GeneratedEnemyUnit(-2,-3,87,13,4), new GeneratedEnemyUnit(6,-10,11,25,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "3c696c12b5235c0ab2eea1df4efa47a38aad14fe736c00f90555963dbf77dcad");
        }

        private static void Case_03442()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3442,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,17,63,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-2,88,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,12,97,6,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "8d97f1b344e5ff48eb2f5f8107aed9dbf513fff71dc6c34ba31f2f3796af3c67");
        }

        private static void Case_03443()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3443,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-7,34,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,14,66,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-17,20,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-12,65,48,4), new GeneratedEnemyUnit(-5,-3,96,5,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "323ba00c9fcc03cce2bd6d5e9c1e8561b520d9691ef3014c90558198134fc85c");
        }

        private static void Case_03444()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3444,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-5,69,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-14,17,16,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "66bdb1a77a213ce877bf249ff86da1dd5919459e7cfa4613c99005236344143e");
        }

        private static void Case_03445()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3445,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,15,35,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-18,78,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-19,49,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-2,76,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-15,45,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,13,22,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,8,8,23,4), new GeneratedEnemyUnit(3,1,65,7,3), new GeneratedEnemyUnit(3,15,65,30,2), new GeneratedEnemyUnit(-7,-15,71,42,1), new GeneratedEnemyUnit(-18,-19,25,6,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "e53f82447492d6e32f904d9697abb572a77f1f1a2174f76407902cfdc88a4d50");
        }

        private static void Case_03446()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3446,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,2,78,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-1,85,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-8,20,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,0,31,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,16,43,23,1), new GeneratedEnemyUnit(20,-17,45,11,1), new GeneratedEnemyUnit(11,-18,98,11,3), new GeneratedEnemyUnit(7,-19,44,47,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "761d71e43b94d4fe7b450f0b75da0c882cc8cb8542d4e34a40c559229cd46475");
        }

        private static void Case_03447()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3447,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-1,10,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-17,27,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,4,42,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-3,40,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-18,85,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-19,74,15,1), new GeneratedEnemyUnit(12,-16,39,50,4), new GeneratedEnemyUnit(0,-7,88,41,4), new GeneratedEnemyUnit(14,10,5,8,1), new GeneratedEnemyUnit(-20,-12,70,49,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "a7bf05cc437f35ddb50578226d9bb3cd91dc904dee382e5bdc77687dc93a290c");
        }

        private static void Case_03448()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3448,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,1,10,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,14,29,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,10,11,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,7,5,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-5,100,34,3), new GeneratedEnemyUnit(-5,-7,15,39,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "f213e14243f146f2a209b38f6f431a246a99f41a383898acd85b48a2630cad3f");
        }

        private static void Case_03449()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3449,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-19,44,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,17,17,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,17,44,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,3,56,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-5,59,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,16,60,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,15,62,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-17,15,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-19,58,6,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "fb54fd2825c1aedb0af3ad58b3f13efb4cb8a52470149c92ce7c411b4b91d0a8");
        }

        private static void Case_03450()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3450,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,16,8,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-6,19,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,8,44,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,15,58,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-8,64,1,3), new GeneratedEnemyUnit(-20,-6,16,46,4), new GeneratedEnemyUnit(-4,15,7,43,2), new GeneratedEnemyUnit(-4,20,47,45,4), new GeneratedEnemyUnit(-14,17,61,1,4), new GeneratedEnemyUnit(4,7,83,49,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "eec7b75c8f01c7465c3a139e954eacf5d4f7d6fcf606d8693533d86f8cf19b8d");
        }

        private static void Case_03451()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3451,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,16,43,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-3,72,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-1,51,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,3,59,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,17,87,2,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "911c5088308e48033223997117b45168d6531aa2b225392a551a0c31c5cdaf6b");
        }

        private static void Case_03452()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3452,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,14,9,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,10,71,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-3,70,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-14,81,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,0,16,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,5,75,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,19,49,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,16,35,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,2,85,32,3), new GeneratedEnemyUnit(2,8,91,34,2), new GeneratedEnemyUnit(-15,-4,24,39,1), new GeneratedEnemyUnit(15,19,100,21,1), new GeneratedEnemyUnit(7,8,30,10,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "7c5d61934000c1a94fd99f39d0c0ed99bd6983f8587b7629aabf51fa1aa0f62a");
        }

        private static void Case_03453()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3453,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-3,41,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-14,77,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,10,45,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,14,26,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,1,85,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-12,40,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,9,56,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,12,37,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,7,82,16,4), new GeneratedEnemyUnit(10,-6,43,2,1), new GeneratedEnemyUnit(-6,-6,11,6,3), new GeneratedEnemyUnit(9,2,14,46,2), new GeneratedEnemyUnit(16,-16,48,11,4), new GeneratedEnemyUnit(9,14,69,1,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "cfdf7e5968f64bfd6ee241cd358049c8d9fec946e003586427306561bb5bc3d4");
        }

        private static void Case_03454()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3454,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-9,37,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-9,27,42,1), new GeneratedEnemyUnit(-15,-15,74,30,2), new GeneratedEnemyUnit(6,1,31,24,3), new GeneratedEnemyUnit(1,-3,77,30,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "487205670349a52b918125d2ed69ed9282c667fdd6c335f00531f07538e7b811");
        }

        private static void Case_03455()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3455,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,2,47,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,7,79,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,2,86,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,19,97,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-12,43,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,6,92,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-1,99,26,1), new GeneratedEnemyUnit(-18,0,52,27,3), new GeneratedEnemyUnit(-7,-6,23,19,4), new GeneratedEnemyUnit(-8,1,5,27,1), new GeneratedEnemyUnit(17,7,11,26,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "36df43c2379bd02045efedc99c03de47c3cc020eb358c649325a904c884b88ab");
        }

        private static void Case_03456()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3456,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,14,99,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-6,97,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,18,46,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,17,21,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,12,55,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-2,16,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,7,60,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-14,46,41,3), new GeneratedEnemyUnit(-9,-18,91,50,3), new GeneratedEnemyUnit(-12,-16,20,34,2), new GeneratedEnemyUnit(3,8,63,40,2), new GeneratedEnemyUnit(10,-9,78,3,1), new GeneratedEnemyUnit(14,-2,55,27,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b8934d24ee81e6c3d1c9e1f9b153473b6d35173d293a7d84aa9e58dd2d00854e");
        }

        private static void Case_03457()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3457,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-14,86,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,17,16,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-7,8,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,0,53,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-1,85,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-4,46,3,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "d477714df6d981a860a8d2304b7f915b109f8e6f28ccc69c1fb5c7d30250f991");
        }

        private static void Case_03458()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3458,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-18,16,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-15,96,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,9,64,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,14,26,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-20,16,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-6,82,6,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "6be5671d7eb6a75bf43a749effa1e46a52f14bc28edbff3c8383e9e931b2daf7");
        }

        private static void Case_03459()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3459,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-5,34,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,17,25,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,4,26,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-8,35,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,14,77,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,14,84,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-18,94,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,10,90,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,1,50,15,1), new GeneratedEnemyUnit(11,3,55,9,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "6f2523f539f4763a90aed2268ad1ed8b3f3399dcd07e17e333e331bbb7188615");
        }

        private static void Case_03460()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3460,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-17,11,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-13,76,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-14,47,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,2,32,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,11,75,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,1,99,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-12,79,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-16,67,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,11,7,38,2), new GeneratedEnemyUnit(-15,-13,18,19,1), new GeneratedEnemyUnit(-20,-7,57,42,2), new GeneratedEnemyUnit(-6,7,73,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "ceab7cc679f35134ba3cbe1872de607a8152a663af06b9436c56203f87b75068");
        }

        private static void Case_03461()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3461,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,18,87,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-19,66,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,5,92,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-19,77,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,12,72,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-10,38,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,9,69,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,6,77,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-20,45,38,1), new GeneratedEnemyUnit(-7,5,63,5,3), new GeneratedEnemyUnit(-19,6,54,31,3), new GeneratedEnemyUnit(-19,1,31,38,4), new GeneratedEnemyUnit(7,-7,72,3,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "1fc08449a9280a75c75a5b7fcfd36acc5c7061f0cca1082ae934ad5082f99521");
        }

        private static void Case_03462()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3462,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-18,24,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-11,59,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-4,46,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,16,21,32,1), new GeneratedEnemyUnit(-17,2,68,21,3), new GeneratedEnemyUnit(12,-16,67,12,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "3058113b2eb1b055428af07d8857b3285cc84052dd2b4c4e50d73737afe88cfc");
        }

        private static void Case_03463()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3463,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-12,91,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,18,47,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-16,57,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,18,11,16,4), new GeneratedEnemyUnit(6,-8,85,28,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "70d7b62b14305c979b48236d329caac1a8de84ec236c39d1753c15c2515bf424");
        }

        private static void Case_03464()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3464,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,20,12,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,4,63,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-13,72,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,15,14,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-20,18,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-9,13,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-6,5,37,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f158c05c4a62e3bf0f87b5484d98ba915a98ac2af402cf3de568c6b5423f7edf");
        }

        private static void Case_03465()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3465,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-2,59,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-2,18,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,13,29,3,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "3c7adf1fe14758f41c21cc2d919ac7f1a1fd9bbddf7f9abf86dc814656a47007");
        }

        private static void Case_03466()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3466,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-15,58,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,19,67,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,18,6,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-1,59,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-6,68,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,3,13,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,3,23,26,4), new GeneratedEnemyUnit(4,-6,79,7,3), new GeneratedEnemyUnit(-14,1,49,21,2), new GeneratedEnemyUnit(-10,1,43,24,3), new GeneratedEnemyUnit(-19,20,47,11,2), new GeneratedEnemyUnit(-7,17,53,27,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "7f97f341fb59fbfa2817efdb54096a43af1118a7ce0f546d32a4672493430f68");
        }

        private static void Case_03467()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3467,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,12,99,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,18,61,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,8,70,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,14,69,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,0,20,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-8,32,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,6,40,1,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "40acc88496f046516dfa753006e5d6b63e28199705d974e6e7cfe8e06320bcf2");
        }

        private static void Case_03468()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3468,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,1,65,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-9,63,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,18,38,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-11,88,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-4,84,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-8,25,4,4), new GeneratedEnemyUnit(7,-10,99,23,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "856435c5278de2693131729ab1b37fab151f938aba3ecc51fae5c794a5bd4b29");
        }

        private static void Case_03469()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3469,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,18,9,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-20,28,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,10,21,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-2,85,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,15,21,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-7,78,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-9,60,1,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "b670c6fdfc6f1bbbe678d137f7da63725894ca6148178abf2123528b7b97295e");
        }

        private static void Case_03470()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3470,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,10,84,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,16,10,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,7,69,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-1,90,8,1), new GeneratedEnemyUnit(6,-15,93,42,3), new GeneratedEnemyUnit(-10,6,21,7,2), new GeneratedEnemyUnit(-7,-8,56,47,1), new GeneratedEnemyUnit(17,8,53,1,1), new GeneratedEnemyUnit(-2,-17,84,32,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "08d5719f1df47bf56c55d830a88ff04e52f490fac408e2d4c3086526a66fe434");
        }

        private static void Case_03471()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3471,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-14,41,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,15,92,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-4,43,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-2,70,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-10,58,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,20,46,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,8,61,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,11,11,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-19,80,23,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "a6f78530a7600071dd012b243a8ae3821f57cc6efd19c2c6b5768f6c1d709a69");
        }

        private static void Case_03472()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3472,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-7,55,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-17,42,2,2), new GeneratedEnemyUnit(3,-7,39,31,1), new GeneratedEnemyUnit(-5,-15,100,5,1), new GeneratedEnemyUnit(-2,17,28,37,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "11cb5ca4415dc4f851dd9960e88977e74a8b2f9ec536ba5d03f2a092fe8e3106");
        }

        private static void Case_03473()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3473,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,20,95,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,14,97,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-17,27,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,18,72,24,3), new GeneratedEnemyUnit(6,-9,75,43,2), new GeneratedEnemyUnit(10,6,35,10,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "365c609514063f5fbabed27aa5825232c4afe14cfb761db51d889b291ab3d121");
        }

        private static void Case_03474()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3474,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-16,51,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,17,57,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,11,18,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-20,59,38,3), new GeneratedEnemyUnit(-3,17,19,11,2), new GeneratedEnemyUnit(20,13,54,14,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "66bd768449d54ef4e956bc5ec14c006afe30f2a328c8b6caa54dc04809ae1f27");
        }

        private static void Case_03475()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3475,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,2,94,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-18,89,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,14,57,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-16,79,4,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "062a536b9ab92126f953d728b98f6a1264780600c1d6e26bb67be0ce54a0fa1c");
        }

        private static void Case_03476()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3476,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,15,19,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-7,38,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,16,12,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,12,69,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,10,85,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-7,36,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,14,13,21,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "d8395cf47de486f4ee674d8015d6be9f2a6e57c682770b0e6dadb88c89691a53");
        }

        private static void Case_03477()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3477,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-11,59,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-18,68,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,15,55,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,12,30,41,4), new GeneratedEnemyUnit(-9,16,71,19,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "e654aaa4a1cc7b937621c33896f49dbe7d5ca80f21a954fa6ad2517332c68bf9");
        }

        private static void Case_03478()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3478,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-5,26,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,13,75,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,17,66,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-17,47,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,4,46,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,12,52,12,3), new GeneratedEnemyUnit(-6,-3,17,45,3), new GeneratedEnemyUnit(19,-7,62,38,1), new GeneratedEnemyUnit(-20,-13,92,11,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "9dc8c582fc59d7b5fe92d86d3cecfe04536d05ac222f5d2309e7d3a86e8dc5d2");
        }

        private static void Case_03479()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3479,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,12,8,1,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "ad331855110c8a4fcf5808fec5b8b077ca8adc884a6a5a079cb77ab5ff4ee427");
        }

        private static void Case_03480()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3480,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,1,72,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-13,58,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,18,24,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-20,7,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,7,93,48,1), new GeneratedEnemyUnit(-15,13,50,50,2), new GeneratedEnemyUnit(-12,-2,39,4,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "df213bd2f29a0f5a7d6009c7eae8ff7891945e2662972dbc9333d388ad637b9c");
        }

        private static void Case_03481()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3481,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-20,68,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-17,10,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-7,55,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,18,98,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-19,37,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,14,72,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,13,82,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,20,40,7,2), new GeneratedEnemyUnit(10,10,29,5,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "0411c69dae0a3f826847760f05ec2ab55b1f451888a45aa49105f0409f6e931c");
        }

        private static void Case_03482()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3482,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,13,63,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,16,71,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,19,50,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-19,30,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-6,84,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-19,26,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,13,71,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,5,38,20,1), new GeneratedEnemyUnit(-15,15,100,42,3), new GeneratedEnemyUnit(-5,-4,88,45,2), new GeneratedEnemyUnit(12,-3,81,20,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "ea1468e7bfbb0ec9c2955ef08b469dad21f460b7a84c325fc5758f9356c1c415");
        }

        private static void Case_03483()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3483,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-13,92,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-13,57,22,2), new GeneratedEnemyUnit(-7,-2,44,45,4), new GeneratedEnemyUnit(-6,-19,88,8,2), new GeneratedEnemyUnit(20,14,30,39,1), new GeneratedEnemyUnit(3,14,79,40,2), new GeneratedEnemyUnit(-5,17,59,10,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "7450b7ec3aa04f1233ae53e7381582c4b445dab67560bffddabf4db439a1dbd1");
        }

        private static void Case_03484()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3484,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-2,30,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-8,63,3,1), new GeneratedEnemyUnit(12,13,72,27,3), new GeneratedEnemyUnit(9,17,81,29,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "8538d112990dda7482edce098eac8ce5e7802797e8ec65b135e5313819bba17a");
        }

        private static void Case_03485()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3485,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,0,77,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-19,12,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,4,60,7,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "abc561bc2d4e6fcf596c3ee157c4782327cc48e1263d7d92114a8b267d0b92f3");
        }

        private static void Case_03486()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3486,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-20,10,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,12,14,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-17,64,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-9,90,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-4,93,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,2,44,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,20,40,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,9,39,38,2), new GeneratedEnemyUnit(-19,16,52,46,4), new GeneratedEnemyUnit(10,1,11,44,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "612a3d25333f6e9452e603390b8faae2b14c402addb711e6ae2126d06e8f534e");
        }

        private static void Case_03487()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3487,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-8,28,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,18,74,3,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "eadaf75cad015a245deebe74894b8c87b6508eaef9165e2b4ca085c754aff303");
        }

        private static void Case_03488()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3488,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,6,47,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,1,53,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,16,95,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-19,68,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-5,15,43,1), new GeneratedEnemyUnit(-18,4,71,1,3), new GeneratedEnemyUnit(-8,3,88,40,2), new GeneratedEnemyUnit(-12,-20,38,14,1), new GeneratedEnemyUnit(-15,0,73,25,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b6929038b5f22e82dead7412270987214cc69461acbedca7c1464e1ae6b5c829");
        }

        private static void Case_03489()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3489,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-15,71,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,11,70,8,4), new GeneratedEnemyUnit(-10,8,96,36,2), new GeneratedEnemyUnit(-1,4,10,38,4), new GeneratedEnemyUnit(17,17,30,50,3), new GeneratedEnemyUnit(11,10,45,28,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "4dc91bca57ce437391f2775132f6e33de9ece2441f46d5cde1a3282139fabe77");
        }

        private static void Case_03490()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3490,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,4,80,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,16,9,23,1), new GeneratedEnemyUnit(-16,17,34,31,4), new GeneratedEnemyUnit(9,20,30,28,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "c37b8630bfcb428a6f9886b94d1d99b9ddf548a19a01625a2496ff66c83a80d7");
        }

        private static void Case_03491()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3491,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,1,45,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,12,79,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,20,20,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "e2c33a2d2f6008153d76720235a95f173037fcdb4a26a9f7acd35c0b05d6ab61");
        }

        private static void Case_03492()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3492,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,2,94,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-4,90,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-3,45,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,1,34,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-14,91,31,4), new GeneratedEnemyUnit(7,18,72,6,3), new GeneratedEnemyUnit(2,-9,83,4,2), new GeneratedEnemyUnit(0,15,91,41,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "aa3fcb7831f6479465639e524ae862a47b8f0d4ea9ccd8b5db07d77886d8f726");
        }

        private static void Case_03493()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3493,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-10,49,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,6,75,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,20,28,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,9,80,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-2,55,23,2), new GeneratedEnemyUnit(-4,-14,60,40,2), new GeneratedEnemyUnit(1,-16,49,34,4), new GeneratedEnemyUnit(-12,-14,95,28,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "7218d44bcd819b296a7582b748073dc1232ae66bf5d06bf813018268e3c596d8");
        }

        private static void Case_03494()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3494,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,3,71,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-14,5,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-9,69,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,8,32,21,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "55eebcf15fea48e8e91d14e302ffed6b05e36b62e5f5de7465f7cb2d1af0bcad");
        }

        private static void Case_03495()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3495,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-6,5,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,7,53,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-15,33,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-13,91,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,20,93,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,19,82,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-2,10,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,19,54,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-17,13,39,3), new GeneratedEnemyUnit(-8,-7,37,34,3), new GeneratedEnemyUnit(-9,10,93,50,2), new GeneratedEnemyUnit(-20,-2,74,14,4), new GeneratedEnemyUnit(16,-6,85,7,3), new GeneratedEnemyUnit(-2,-8,89,26,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "db94715a5ba3d43270e62f97a3f3fb33109433283359afeb643aa0f61c78e422");
        }

        private static void Case_03496()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3496,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-13,57,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-16,19,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,5,63,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,4,67,6,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c50208991deffd4ac9b7ff547ce9bf626164e9644f64dc633f586df6338f6498");
        }

        private static void Case_03497()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3497,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,3,60,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-1,27,4,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "4f1002ab9c83e003d63d822c9ff71b1d00bfa0836315ff9e6f5562a65ecd473e");
        }

        private static void Case_03498()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3498,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,4,42,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,10,91,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,17,78,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-15,89,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-3,67,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-14,39,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,17,24,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-18,11,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-7,72,24,3), new GeneratedEnemyUnit(-18,7,98,7,2), new GeneratedEnemyUnit(11,1,80,36,2), new GeneratedEnemyUnit(5,-14,33,37,4), new GeneratedEnemyUnit(-9,-13,45,32,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "b6a68ba2c5e5e92669adcd85d4b201dcd599d456e95475684dfee967e06084ef");
        }

        private static void Case_03499()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3499,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-13,29,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,4,71,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-9,9,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-4,33,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,9,96,11,2), new GeneratedEnemyUnit(13,-7,82,29,2), new GeneratedEnemyUnit(6,18,100,37,2), new GeneratedEnemyUnit(-9,-2,52,46,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "e888dcfc743b3f45c28ffe9529027f3f2bfac470052dfb1fe311175c43804262");
        }

        private static void Case_03500()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3500,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-14,70,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-13,14,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,18,40,34,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b69263be2127274a5dc33e38081b75397b9291de541e6283b6ce0f977197dce4");
        }

        private static void Case_03501()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3501,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,0,64,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-17,99,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,6,70,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,4,53,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,15,53,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,11,86,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,12,70,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-6,6,2,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "7c4dc1756d7b1d99130a64a6eb08e820798e93e051744ce7851696075fb4d346");
        }

        private static void Case_03502()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3502,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-1,8,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-4,48,7,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "73ba5ac5be12d97f16441d19fc8f028eec7f730793dcb6fd7df076f942e0f065");
        }

        private static void Case_03503()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3503,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-8,24,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-18,55,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,3,7,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,10,68,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-4,25,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-15,63,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,0,86,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-19,64,45,3), new GeneratedEnemyUnit(20,-18,96,39,4), new GeneratedEnemyUnit(-3,18,40,45,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "4098a2dafde5604f81dc9cb927bc4b13bf2215ec3a731954489251760fd5c1ad");
        }

        private static void Case_03504()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3504,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-8,52,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-3,13,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,14,72,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,12,88,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-9,20,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-18,66,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-5,92,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-11,84,14,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "0f13968d2f2a85452a0648f46d69783b29ad9070cf47e5a928025b74a8c2e93d");
        }

        private static void Case_03505()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3505,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-18,15,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,11,5,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,7,90,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-1,80,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,0,93,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,15,93,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-9,19,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-9,83,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,4,73,25,2), new GeneratedEnemyUnit(-3,-7,21,6,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "608cf6820b91028666cd59e32bd55468df4d85e178e2e667004efb1e8be11b3b");
        }

        private static void Case_03506()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3506,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-17,93,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,0,8,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,4,50,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,17,20,6,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "95b958478ba5ba8faeca913fb91fc65017a775632cf7fd655b76ad164fdc8284");
        }

        private static void Case_03507()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3507,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-4,66,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,18,57,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,17,9,24,1), new GeneratedEnemyUnit(-2,0,59,13,4), new GeneratedEnemyUnit(-14,-7,50,49,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "866689607ba4d669cecf7f9d797bc2a7ae6e477096dd9f02042255b6a9d6b3a6");
        }

        private static void Case_03508()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3508,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,1,50,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,9,86,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-1,11,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,15,95,16,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "56fb23a298d5fa8980579fccf65efed53fdcfc300b22649f355905e2952b6ea3");
        }

        private static void Case_03509()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3509,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-8,87,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,19,100,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,0,48,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,0,20,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-12,80,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,20,68,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,5,27,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-18,66,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,18,70,46,1), new GeneratedEnemyUnit(-1,-9,96,8,3), new GeneratedEnemyUnit(-3,6,86,10,3), new GeneratedEnemyUnit(7,20,93,39,2), new GeneratedEnemyUnit(9,-19,63,37,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "12aa89b99c18dadb6b34b259a105eb557c380abcf20fc257c946e8a79f47d7be");
        }

        private static void Case_03510()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3510,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-10,43,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,20,16,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,3,72,11,4), new GeneratedEnemyUnit(-16,-18,68,22,2), new GeneratedEnemyUnit(-16,-10,17,21,2), new GeneratedEnemyUnit(-11,13,49,8,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "552ef58cd338c536769238a96db60462df8a47a27fd94a8ab1616526d5e2d363");
        }

        private static void Case_03511()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3511,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-14,95,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-2,58,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,14,10,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-11,17,3,1), new GeneratedEnemyUnit(8,-7,46,12,2), new GeneratedEnemyUnit(19,8,28,47,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "3181f45817e4d0ea0d3aa8e3b1e06b4641755e5de15112edd98003dc8cc51ba2");
        }

        private static void Case_03512()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3512,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-4,76,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,3,64,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,14,31,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-17,35,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-7,97,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-4,11,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,15,30,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,6,98,4,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "73880168f2c28aec148183f8a3fe8dc4b2c17a43ccbbebe0bbac65e9efaa4b12");
        }

        private static void Case_03513()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3513,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,11,8,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,7,24,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,1,95,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-9,78,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,18,83,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-19,33,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,10,60,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-17,58,38,1), new GeneratedEnemyUnit(13,18,13,5,2), new GeneratedEnemyUnit(-6,-4,22,40,3), new GeneratedEnemyUnit(13,-20,51,42,1), new GeneratedEnemyUnit(20,9,83,13,2), new GeneratedEnemyUnit(-15,7,29,48,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "e0a2396977ab7ee4a0fe2aad4218401752a4d930fb7e03651abed78151f461c0");
        }

        private static void Case_03514()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3514,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,9,5,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-7,100,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-1,15,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-20,67,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,4,83,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,20,50,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-5,94,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-17,61,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-1,17,3,1), new GeneratedEnemyUnit(19,-19,24,17,1), new GeneratedEnemyUnit(12,6,71,47,4), new GeneratedEnemyUnit(9,0,99,13,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "034d194a0e8914a3f5432f1ac78d3f3946965b9d5ab33e73577f1ebc0c3f1320");
        }

        private static void Case_03515()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3515,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-19,92,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,17,42,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,8,36,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,8,79,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,14,30,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,7,6,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-8,100,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,7,36,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-18,9,40,1), new GeneratedEnemyUnit(19,10,56,19,2), new GeneratedEnemyUnit(6,-16,36,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "20639545b70dc72ac3d6103234562cafccdd9a3e97914094df8ddbaa9fa50f01");
        }

        private static void Case_03516()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3516,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,8,56,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-10,100,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-17,72,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-9,45,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,13,29,3,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "f34c41443bf378c1fc3e9b82377bafd752341304bd0aeac5231e3910f6f0974d");
        }

        private static void Case_03517()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3517,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-11,48,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,18,66,43,1), new GeneratedEnemyUnit(-7,-2,44,44,1), new GeneratedEnemyUnit(5,-15,19,2,1), new GeneratedEnemyUnit(8,2,80,38,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "ad4f52d1e6816cf6477e0db3a660bed4bb29f2e4884209adace07edcdebfa39b");
        }

        private static void Case_03518()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3518,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-6,61,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,1,32,14,2), new GeneratedEnemyUnit(-7,-20,47,27,1), new GeneratedEnemyUnit(2,-5,71,21,3), new GeneratedEnemyUnit(2,13,86,15,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "ee12e94c140528c43b47fa2239352498b90ea7979ec99231a808b7996bd879f8");
        }

        private static void Case_03519()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3519,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-13,96,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,13,65,22,1), new GeneratedEnemyUnit(-14,-3,29,35,2), new GeneratedEnemyUnit(11,10,71,27,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "867042de070658965ca0b93ca3c8dacb2723d898ddd59c26949ea8b2e3ac3b36");
        }

        private static void Case_03520()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3520,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-12,31,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,17,78,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,3,13,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-11,27,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-20,79,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-17,68,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,19,72,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-3,85,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-15,100,10,3), new GeneratedEnemyUnit(18,-10,81,15,4), new GeneratedEnemyUnit(9,-7,48,31,3), new GeneratedEnemyUnit(-15,18,49,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "e36e554c8ba431c87d91f1cf163ab6062e16b25fcef7a16ece2e922aa4203b78");
        }

        private static void Case_03521()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3521,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-11,71,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,2,73,19,2), new GeneratedEnemyUnit(3,-16,23,3,1), new GeneratedEnemyUnit(14,4,66,35,2), new GeneratedEnemyUnit(20,0,39,2,1), new GeneratedEnemyUnit(-1,5,31,37,2), new GeneratedEnemyUnit(5,-9,6,36,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "e38062b25b2b32681ed8abf7162d41531841bb4f2c8ae61a391dcab8031ff916");
        }

        private static void Case_03522()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3522,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-1,93,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-4,64,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-17,53,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-12,51,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-18,16,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,7,63,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-14,63,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-19,81,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-6,61,48,4), new GeneratedEnemyUnit(-19,11,95,9,2), new GeneratedEnemyUnit(-3,-3,23,28,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "99079d3b17849bfc7df8802d82ab9ad42df3667bca05ac2e31d69c5fb323aa62");
        }

        private static void Case_03523()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3523,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,11,34,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-16,98,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-4,44,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-3,63,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-19,67,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,12,53,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,1,97,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,8,52,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-13,95,43,4), new GeneratedEnemyUnit(2,0,18,8,4), new GeneratedEnemyUnit(8,-20,43,38,4), new GeneratedEnemyUnit(8,19,69,9,3), new GeneratedEnemyUnit(-2,9,51,30,3), new GeneratedEnemyUnit(16,3,62,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "e4226febe7c62f2042f0b567eba0262e84f868485c94d042cc4f57075496ddbd");
        }

        private static void Case_03524()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3524,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,0,33,7,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "0a41e79ce8a195b62187aeba8dea756c5d12e09a061b2903abc5a3ec5496a283");
        }

        private static void Case_03525()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3525,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-14,63,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,16,61,22,2), new GeneratedEnemyUnit(-7,-8,61,17,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "581f00c50124a9e355aa0a17757f27de79d8d142305d3c82f5371c802774e324");
        }

        private static void Case_03526()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3526,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-18,55,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,6,90,9,1), new GeneratedEnemyUnit(-7,5,85,31,1), new GeneratedEnemyUnit(-19,9,92,50,3), new GeneratedEnemyUnit(-4,15,67,3,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "428ee4c4235ce81b04dd4cb875d179becbfcfd0a368ab4ba0bea9180fa634f12");
        }

        private static void Case_03527()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3527,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,8,45,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,14,44,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,15,94,6,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "16dc97fd8591ff8f960960769f3868c18a3c0c319741c214097b132339032979");
        }

        private static void Case_03528()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3528,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,9,82,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-15,55,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,18,29,27,2), new GeneratedEnemyUnit(-4,4,76,44,3), new GeneratedEnemyUnit(6,-7,35,24,1), new GeneratedEnemyUnit(-20,-12,45,16,4), new GeneratedEnemyUnit(-10,12,14,11,3), new GeneratedEnemyUnit(3,-13,37,7,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "f7b24bd5d2c4a931a78a286cc995ab7ec80024af71675effce7f5f0e338c6e96");
        }

        private static void Case_03529()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3529,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,4,47,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-3,34,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,2,8,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-20,77,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,18,43,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,12,7,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,17,52,16,3), new GeneratedEnemyUnit(17,-12,18,47,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "f4da03d063f8c7fd97bd3ec5a33035a1790216a42353bfd70a87fbb25c4784aa");
        }

        private static void Case_03530()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3530,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-2,80,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,13,66,43,1), new GeneratedEnemyUnit(17,-11,42,8,4), new GeneratedEnemyUnit(-13,5,35,3,3), new GeneratedEnemyUnit(18,-16,24,12,4), new GeneratedEnemyUnit(-1,17,27,31,3), new GeneratedEnemyUnit(-15,-14,85,50,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "bb2c1661ef76ef9e99f5a73a9ccf48e2da18818340ed408d40678b0d436c4436");
        }

        private static void Case_03531()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3531,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,19,34,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,17,22,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,11,32,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,17,25,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,5,89,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,2,21,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-17,6,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,13,84,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-16,56,5,4), new GeneratedEnemyUnit(-6,-15,73,25,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "e2ac70325547a44f8709fd44f64c60a9bf2ced3b79b3e26f9c4e0df8ab07707b");
        }

        private static void Case_03532()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3532,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,17,63,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-19,50,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-19,44,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-6,69,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,13,86,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,10,79,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-4,36,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,13,26,2,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "7e786e81faaed24781d919f1934d261553e391273009c3548bbee810d389bf36");
        }

        private static void Case_03533()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3533,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-7,50,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-16,11,14,3), new GeneratedEnemyUnit(0,9,36,27,3), new GeneratedEnemyUnit(4,-12,24,44,1), new GeneratedEnemyUnit(-13,-17,51,11,2), new GeneratedEnemyUnit(-5,15,50,1,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "c4453f233630a4ad53d10edb59c4e209eda6eee6f23acd63f56a1844fd442441");
        }

        private static void Case_03534()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3534,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-3,41,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-3,81,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,4,48,13,1), new GeneratedEnemyUnit(19,15,79,24,3), new GeneratedEnemyUnit(13,10,11,3,4), new GeneratedEnemyUnit(18,-13,8,24,4), new GeneratedEnemyUnit(6,-5,26,43,3), new GeneratedEnemyUnit(-16,-6,23,17,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "17bd9df6e8c6ede8d8e96f79c91fcbf989d6265246204aeecca9a583a63523a4");
        }

        private static void Case_03535()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3535,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-5,31,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,16,27,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,13,29,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-10,73,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,4,85,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,19,72,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,17,86,18,3), new GeneratedEnemyUnit(-18,18,49,16,1), new GeneratedEnemyUnit(16,-4,61,46,1), new GeneratedEnemyUnit(-18,-10,52,4,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "239e668deff4fd9de693c93dd76e39b334e62b2ddffae6cfd97c96366fb631c0");
        }

        private static void Case_03536()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3536,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,3,74,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,14,25,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-2,5,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-7,19,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-20,63,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-14,24,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-1,53,18,2), new GeneratedEnemyUnit(7,-20,24,29,4), new GeneratedEnemyUnit(-5,12,97,2,1), new GeneratedEnemyUnit(11,14,45,5,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "2f02712f30e3f908835398cac6c7c57bab40c15e5fd539b8845e26a2c7f22a17");
        }

        private static void Case_03537()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3537,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,8,56,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,2,91,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-15,76,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-14,78,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-1,85,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,0,95,26,1), new GeneratedEnemyUnit(4,-19,91,32,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "68ba42b7a9bb2bb327f419b2c36638bb69cc00440e53197bdd102e76e599f850");
        }

        private static void Case_03538()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3538,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,18,90,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-14,75,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,12,99,50,3), new GeneratedEnemyUnit(17,-6,22,18,3), new GeneratedEnemyUnit(12,-11,38,33,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "0afbe7066c2f6865b90ee524bc5b27603c049c70d71105b28c777c80e973693d");
        }

        private static void Case_03539()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3539,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-12,90,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-16,18,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,12,62,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-20,6,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,19,10,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-10,68,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-1,48,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-3,21,39,3), new GeneratedEnemyUnit(-8,16,5,25,3), new GeneratedEnemyUnit(15,7,41,20,4), new GeneratedEnemyUnit(9,-3,98,38,1), new GeneratedEnemyUnit(-11,-7,5,19,2), new GeneratedEnemyUnit(-13,-16,86,37,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "d837022997364b1be346d5addc070b574ad2485563e0700afffc96879a6ff892");
        }

        private static void Case_03540()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3540,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,9,31,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-7,54,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-7,77,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,17,100,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,11,90,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-6,75,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-2,36,1,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "0927d310976ae98f89a13966a9e20480001e92a1c752cbcf754f93691c399437");
        }

        private static void Case_03541()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3541,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,19,72,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-7,48,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,1,97,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-20,100,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,9,31,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-4,46,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,15,42,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,8,15,37,2), new GeneratedEnemyUnit(-2,19,65,49,3), new GeneratedEnemyUnit(-10,-19,6,14,3), new GeneratedEnemyUnit(-16,-3,27,18,4), new GeneratedEnemyUnit(-20,10,27,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "00e1db2b41ecc536148f70840a6c271a345909668249b1297d31f36dfac31695");
        }

        private static void Case_03542()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3542,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,11,88,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,16,83,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,9,92,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-11,43,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-4,91,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,17,51,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,19,18,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,0,17,7,1), new GeneratedEnemyUnit(-2,2,39,47,4), new GeneratedEnemyUnit(4,-8,59,9,4), new GeneratedEnemyUnit(-15,-15,86,44,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "6b0f144d1232c4d3185e02df4d63959f61f77128f7569675910b905515773558");
        }

        private static void Case_03543()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3543,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,8,64,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,18,99,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-10,73,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-12,49,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-2,71,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-1,54,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-6,22,31,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "31f24fbb95fcb2954c8eaa23b50924ae3fed025cb7b3cb51a8e301421992e1e0");
        }

        private static void Case_03544()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3544,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-10,79,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-15,9,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-10,95,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,2,78,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-12,94,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,4,12,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,15,96,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "2d13a30c72a59eda28122173fefd9a044a2b9ad018c81f979e369d35723f00be");
        }

        private static void Case_03545()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3545,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-5,92,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,14,13,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,17,90,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,9,71,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-12,10,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,2,10,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,15,16,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,10,33,8,2), new GeneratedEnemyUnit(-1,-7,50,48,3), new GeneratedEnemyUnit(-19,-1,23,22,1), new GeneratedEnemyUnit(-2,-14,83,11,3), new GeneratedEnemyUnit(6,-6,46,4,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "76989ac483c63bd4d8845d9de20a71704ccb0c0172448d113bb6afa884799de3");
        }

        private static void Case_03546()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3546,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-9,91,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-18,82,21,2), new GeneratedEnemyUnit(-8,16,16,24,3), new GeneratedEnemyUnit(2,0,33,31,2), new GeneratedEnemyUnit(-10,-16,58,20,3), new GeneratedEnemyUnit(-4,20,58,28,2), new GeneratedEnemyUnit(1,8,82,25,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "5e99958f75fdf9be15dfab3f559da1cbab3f1b4f00b016b051cb9d5d7af07394");
        }

        private static void Case_03547()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3547,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,9,100,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-17,7,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,4,41,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-12,12,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,13,100,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-13,99,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-17,70,11,4), new GeneratedEnemyUnit(18,-16,57,4,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "30735a8fbdf46245175beef33c6bb74b63390e7435a372e6692eb919459522dc");
        }

        private static void Case_03548()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3548,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-6,27,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,8,42,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,9,70,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-1,98,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-1,40,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-19,66,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-16,58,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,11,27,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-2,58,40,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "6703941a6d5638241e68e3c86f1299ab0a1434b5eeab16a398912dee5354c976");
        }

        private static void Case_03549()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3549,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,7,76,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-4,98,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,9,19,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,10,18,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-6,14,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,9,53,31,2), new GeneratedEnemyUnit(4,4,30,20,1), new GeneratedEnemyUnit(-12,6,71,20,2), new GeneratedEnemyUnit(-1,12,34,4,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "2cfaaa297d0f3027748e9cb915eca965adc76429f7d7d948e0cf718d2472eec6");
        }

        private static void Case_03550()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3550,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-6,65,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,7,90,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,12,36,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-9,32,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,5,26,8,3), new GeneratedEnemyUnit(10,7,82,28,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "c145927c3ae9df31ad96363ba4a03dbd4bdad9b01963dce1ff50d1b528f263dd");
        }

        private static void Case_03551()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3551,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-20,32,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-8,21,12,3), new GeneratedEnemyUnit(4,-18,76,8,2), new GeneratedEnemyUnit(-18,1,40,49,2), new GeneratedEnemyUnit(-4,11,28,30,3), new GeneratedEnemyUnit(-3,17,54,49,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "c4e481da2cf8e3f1730ba488ea540a2c43b623b250dc9b36d6125016e38308c6");
        }

        private static void Case_03552()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3552,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,15,10,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,2,41,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,16,37,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,11,43,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-8,12,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,7,99,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,19,45,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,10,60,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-14,31,2,4), new GeneratedEnemyUnit(12,1,30,28,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "52e23cc8826348d725a7bcb7d1793e61a3e771bfbfd5d1473c6696bb463f96b8");
        }

        private static void Case_03553()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3553,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-5,51,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-20,22,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,14,88,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,18,31,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,13,18,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-10,40,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,11,16,4,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "2be5fcfea937131ad210286b25baa2e46bf192edb091806da8899235ba53e350");
        }

        private static void Case_03554()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3554,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,19,93,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-12,97,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,4,39,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-19,62,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-8,78,26,1), new GeneratedEnemyUnit(5,9,95,30,3), new GeneratedEnemyUnit(16,7,18,48,1), new GeneratedEnemyUnit(-7,14,17,44,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "0d1963800fefda78e0fae2fee236d8fc1fa4b37cbee3248e2a499b42bb196520");
        }

        private static void Case_03555()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3555,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,6,33,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,17,11,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-2,93,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,10,17,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,20,71,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,2,38,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-17,100,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-7,24,12,2), new GeneratedEnemyUnit(4,15,30,26,3), new GeneratedEnemyUnit(12,-15,94,37,2), new GeneratedEnemyUnit(5,-9,7,26,2), new GeneratedEnemyUnit(-6,17,49,24,2), new GeneratedEnemyUnit(0,3,98,18,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "201346f62fda8315433943d4749e0eb3160d262114db36d72a9f2aa830c2006c");
        }

        private static void Case_03556()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3556,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,10,51,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-18,96,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-11,36,13,2), new GeneratedEnemyUnit(-19,-2,57,24,4), new GeneratedEnemyUnit(-11,-19,87,30,3), new GeneratedEnemyUnit(-3,14,76,10,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "204ee216fa4712383ae9722da6dc5d8e94e3924ffaa5eee4420c15b6c06a8680");
        }

        private static void Case_03557()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3557,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-14,66,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,14,10,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,14,38,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,5,86,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-11,55,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,16,56,20,3), new GeneratedEnemyUnit(0,-10,9,37,4), new GeneratedEnemyUnit(-11,-6,46,6,3), new GeneratedEnemyUnit(-4,13,87,21,1), new GeneratedEnemyUnit(11,2,98,48,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "0e843895b3048fd8a569055039412dab51eeb59da88ccca2c93c538066b3bc23");
        }

        private static void Case_03558()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3558,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,0,59,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-17,33,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,17,81,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-16,70,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-12,22,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,5,93,17,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "db37894b3152c6e3d1e9d5f05885fba7114ab27ad7292848017ffd3a52065682");
        }

        private static void Case_03559()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3559,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,5,47,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-11,14,29,1), new GeneratedEnemyUnit(5,9,97,20,3), new GeneratedEnemyUnit(-15,-17,92,33,3), new GeneratedEnemyUnit(12,-9,69,5,1), new GeneratedEnemyUnit(4,2,29,37,3), new GeneratedEnemyUnit(4,16,81,11,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "ba676229989b880494e1d13d295da24c5a86876a79eaa79f621588727b188a11");
        }

        private static void Case_03560()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3560,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,4,90,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,13,94,1,1), new GeneratedEnemyUnit(-3,-6,26,33,4), new GeneratedEnemyUnit(-19,10,72,32,1), new GeneratedEnemyUnit(-4,11,37,18,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "4a8406d4fbd0fb8f104efec1df71a286724636049e3b8c3e8296ffdae6778a33");
        }

        private static void Case_03561()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3561,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,16,94,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,2,12,3,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "d326130f4a25950813b6edc7ced2ed1ed253d99898b40190b802e6085f19a697");
        }

        private static void Case_03562()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3562,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-3,88,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-13,75,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,18,55,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,19,80,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-18,71,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,5,33,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,0,49,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,16,78,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,20,74,28,3), new GeneratedEnemyUnit(0,-13,61,36,2), new GeneratedEnemyUnit(-8,-7,12,45,3), new GeneratedEnemyUnit(2,0,90,47,4), new GeneratedEnemyUnit(13,12,48,13,1), new GeneratedEnemyUnit(-3,8,20,18,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "57545b5516d12102a0e90c8c6360314b696bbccd2efc8cae2191f41349c64e05");
        }

        private static void Case_03563()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3563,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-15,65,3,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "5dd30d3c47c81274374de22b5b5773da337f3fe19bae3bff67e2bb9d2f01f8c4");
        }

        private static void Case_03564()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3564,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,7,27,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-20,98,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-1,95,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-4,57,23,2), new GeneratedEnemyUnit(8,-2,22,4,4), new GeneratedEnemyUnit(9,-10,50,25,4), new GeneratedEnemyUnit(3,5,63,34,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "419ea2ad1659aa829a5145757f94348d233bd4f14b707d52ecf44ef4feac8a32");
        }

        private static void Case_03565()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3565,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-19,96,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,3,68,43,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "928dd91fa79bf58dc11b8c9fe85582084a353d0608f3d301f87d6afc4364a365");
        }

        private static void Case_03566()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3566,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,6,8,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,2,49,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-20,5,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-2,75,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-20,49,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,17,98,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-5,42,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-4,9,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-1,70,48,2), new GeneratedEnemyUnit(-7,-7,53,31,1), new GeneratedEnemyUnit(-2,0,93,17,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "0cc9dd4b9a7555bc731f841316e9a06c57d4e4866f41255e4482e026f4550841");
        }

        private static void Case_03567()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3567,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,11,45,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,11,83,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,12,41,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-8,35,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,13,17,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,7,11,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-16,82,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,13,62,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-18,48,32,2), new GeneratedEnemyUnit(-18,7,6,12,1), new GeneratedEnemyUnit(3,19,5,46,2), new GeneratedEnemyUnit(-18,18,99,18,3), new GeneratedEnemyUnit(14,11,11,47,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "7de691b15f7bbf65b0bd1fe224fc543a38a18b3d0f2a3a02bfb05e0a106c48ac");
        }

        private static void Case_03568()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3568,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-16,8,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-4,66,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-14,67,1,1), new GeneratedEnemyUnit(7,9,62,37,4), new GeneratedEnemyUnit(-7,6,25,36,4), new GeneratedEnemyUnit(9,13,91,29,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "e5d9cbf3bf439417b15df246103933fdad19690b57d001ea58912ed045294dac");
        }

        private static void Case_03569()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3569,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,19,34,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-7,43,11,1), new GeneratedEnemyUnit(-9,5,5,18,3), new GeneratedEnemyUnit(-1,-11,13,20,1), new GeneratedEnemyUnit(5,-11,76,40,2), new GeneratedEnemyUnit(11,18,49,26,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "c3c0e4ef2190b91821ad895aaf26fdbb98a49235f0c2b1f34c5d58005ec4343a");
        }

        private static void Case_03570()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3570,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-19,30,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-18,100,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,9,71,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,2,43,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,18,78,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,14,62,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,13,48,10,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "b5ee6792b2318177271496a8d65ca397fa930646dc6b6f25b829bf4462562035");
        }

        private static void Case_03571()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3571,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-6,53,1,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "be488dd1b533ff6c6aa90ca62494325cf688ae78fe8c042c677604d59357df8b");
        }

        private static void Case_03572()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3572,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,6,24,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,19,19,17,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "a67982c33aa7f53d46d6eb8fbcd743d274568899b05c5cbdb502b6eab94e9e81");
        }

        private static void Case_03573()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3573,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,2,42,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-12,87,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,13,42,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-4,81,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,6,95,22,3), new GeneratedEnemyUnit(-14,-7,45,18,4), new GeneratedEnemyUnit(6,13,72,26,2), new GeneratedEnemyUnit(-12,-20,93,11,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "c4e7e00fd9a88ee66fc3c33421159288bb05751963b5ed84903d18a9b96450c6");
        }

        private static void Case_03574()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3574,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-13,33,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-14,34,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-12,49,46,1), new GeneratedEnemyUnit(-9,3,49,11,4), new GeneratedEnemyUnit(13,-1,83,31,4), new GeneratedEnemyUnit(-12,-15,36,41,3), new GeneratedEnemyUnit(13,-15,13,35,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "3306b6e0a271fa42bb742a26a06f5e409cd84a0fe435ced815dd9182acf9a867");
        }

        private static void Case_03575()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3575,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-9,75,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,18,52,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-15,78,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-18,84,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,4,78,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-2,18,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-2,80,38,2), new GeneratedEnemyUnit(4,-11,87,17,2), new GeneratedEnemyUnit(10,-8,67,45,2), new GeneratedEnemyUnit(1,6,75,46,1), new GeneratedEnemyUnit(-19,-20,33,34,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "e9a3dc4937e16c7f49e709067d7b138237c8fe42da487f3ea3d72bbdd31427fb");
        }

        private static void Case_03576()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3576,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,17,67,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,9,14,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,11,27,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-18,98,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,11,85,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-9,20,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-10,61,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,14,18,43,3), new GeneratedEnemyUnit(11,-2,47,2,1), new GeneratedEnemyUnit(4,8,65,26,4), new GeneratedEnemyUnit(12,-17,19,43,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "fb4c0ed8294d1ce3175caebeb98ae46e05d7ccd2576709ec9423a976ea9c37e9");
        }

        private static void Case_03577()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3577,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,12,57,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,12,88,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-20,69,2,4), new GeneratedEnemyUnit(-16,-15,47,21,1), new GeneratedEnemyUnit(7,-6,66,28,3), new GeneratedEnemyUnit(16,-2,20,26,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "5c9d65fc6272991234ab2f2b33cbb28c0c58ad347931a26297b318937b290ffa");
        }

        private static void Case_03578()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3578,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,6,20,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,11,90,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-11,91,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,5,38,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-1,58,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,2,86,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,19,35,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-17,70,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-14,37,20,1), new GeneratedEnemyUnit(6,20,85,46,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "91f9389d0b31a8e32869d03161ea55a808ac44dcb436cf5699de9cce8a3c605d");
        }

        private static void Case_03579()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3579,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-12,51,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,0,68,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-13,9,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,10,96,2,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "495cbaaf00a85d61e95dd8ac772935c6c0166303f516b1f62f5093313a54cccf");
        }

        private static void Case_03580()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3580,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-5,14,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,16,16,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,3,34,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,1,75,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,3,21,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-2,84,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-18,60,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-15,51,15,2), new GeneratedEnemyUnit(-18,19,76,45,1), new GeneratedEnemyUnit(-4,20,58,46,3), new GeneratedEnemyUnit(-17,18,70,27,4), new GeneratedEnemyUnit(10,8,55,42,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b9df69b8ea7b82c6b224bd437e0d610f2e81ec56c5ec8decff513e6b24b9b2b9");
        }

        private static void Case_03581()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3581,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,20,11,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-18,13,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-19,68,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-13,53,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,2,17,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-16,50,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-15,10,1,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "26a335fe0eec5fab3a54cfe2fd2be3624fd791cda5e5c87e3459df154d5b57d1");
        }

        private static void Case_03582()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3582,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-1,62,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,5,47,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,2,20,16,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8bfd6948504ac4f84cf35c7c5c06e4646340c324fb2472b6b71750756c31cc4c");
        }

        private static void Case_03583()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3583,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-15,99,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-3,16,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,3,10,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-11,33,25,4), new GeneratedEnemyUnit(-17,18,20,41,4), new GeneratedEnemyUnit(18,4,64,39,1), new GeneratedEnemyUnit(-15,6,97,23,3), new GeneratedEnemyUnit(-13,4,12,39,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "c75a38fd568b59304503f8868587f542ec4815d0af11553916c8e83ce1f0d306");
        }

        private static void Case_03584()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3584,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,3,47,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,0,51,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-11,16,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-18,84,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-19,88,2,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "7d7683c680e3fbd158e9f42ba3c2ab22f8297f61284d07d56f304e223ab58fe8");
        }

        private static void Case_03585()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3585,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,9,96,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,4,93,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-15,39,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,15,75,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-5,25,29,2), new GeneratedEnemyUnit(17,-20,5,50,4), new GeneratedEnemyUnit(2,19,30,14,3), new GeneratedEnemyUnit(19,-10,38,9,2), new GeneratedEnemyUnit(11,12,65,43,3), new GeneratedEnemyUnit(2,-3,30,39,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "d7fb51413d77fd5ccc33361c184787126fa70cc613cdfd42e8da220eaa997b46");
        }

        private static void Case_03586()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3586,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-5,74,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,12,99,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-16,94,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,16,70,27,1), new GeneratedEnemyUnit(9,-3,79,27,4), new GeneratedEnemyUnit(-16,-8,29,9,4), new GeneratedEnemyUnit(-17,-1,14,39,3), new GeneratedEnemyUnit(17,-19,80,20,2), new GeneratedEnemyUnit(3,-18,66,48,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "a04f4abdd03482714e9aa6d03099398f1e9aafd9af7a257d94feb243dcbda8da");
        }

        private static void Case_03587()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3587,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-8,18,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-1,20,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-12,62,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,12,86,3,4), new GeneratedEnemyUnit(15,-17,69,19,2), new GeneratedEnemyUnit(-3,17,89,44,4), new GeneratedEnemyUnit(2,18,74,49,2), new GeneratedEnemyUnit(-12,-5,87,1,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "ad075a3e66fb2ff017abf927f4ed18d42ff7f734b0cbf8ca178487a77b104f94");
        }

        private static void Case_03588()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3588,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,0,18,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-18,67,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-3,26,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,14,95,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-1,87,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-17,47,20,2), new GeneratedEnemyUnit(-3,-5,45,16,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "5641c01c725e8bfa3dee6bcafe3ba37a997a3e667eab5e492b9b03a18c84e5ee");
        }

        private static void Case_03589()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3589,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,1,19,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,19,50,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,3,34,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,2,21,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-13,73,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,14,82,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-6,77,30,2), new GeneratedEnemyUnit(-6,-16,93,46,1), new GeneratedEnemyUnit(-17,-20,79,30,4), new GeneratedEnemyUnit(-19,-16,40,15,1), new GeneratedEnemyUnit(-20,10,35,39,2), new GeneratedEnemyUnit(-7,19,73,32,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "bc78be14a642a0196ee9981c329736749346868a9464d5945705498d4d5350e2");
        }

        private static void Case_03590()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3590,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-15,93,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-16,37,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,2,87,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,17,77,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,1,85,9,2), new GeneratedEnemyUnit(6,14,99,32,4), new GeneratedEnemyUnit(-13,-19,95,22,1), new GeneratedEnemyUnit(-6,10,6,47,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "f128f296211a88df01c1834ab8b4594f99a7349f58e764a2ae25d6528f63e8b1");
        }

        private static void Case_03591()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3591,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,20,9,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-15,95,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,15,89,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,19,65,17,2), new GeneratedEnemyUnit(-5,12,18,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "f338e91615b759c258c16862cc564ad216337f75751bbba26ae8b0092b01df3f");
        }

        private static void Case_03592()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3592,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,7,78,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,19,100,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-3,55,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,19,9,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,9,68,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-17,89,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-19,48,3,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c256e6fd619ebbbffb05a32f8fed681a3e427293f6801fd64903c798eeaf0d8a");
        }

        private static void Case_03593()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3593,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-4,54,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,2,55,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,3,49,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,5,13,3,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "b73e33efa0d637c836ad4dd9d9c387319a564620cabcf28410a5e72ab2f877e6");
        }

        private static void Case_03594()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3594,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-6,91,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-14,61,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,9,17,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-4,22,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,11,36,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,20,12,38,2), new GeneratedEnemyUnit(-15,-13,41,49,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "722df0620b1b94ee31808ec7a68b3943a2761880761ed074e8ac57505c8137b0");
        }

        private static void Case_03595()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3595,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,20,25,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-11,19,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,10,7,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-16,93,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-14,9,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-7,20,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,2,37,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,5,49,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,2,60,1,1), new GeneratedEnemyUnit(3,10,58,1,2), new GeneratedEnemyUnit(12,-1,90,21,3), new GeneratedEnemyUnit(9,17,39,5,3), new GeneratedEnemyUnit(-11,-15,31,25,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "0eb6f70348ecb2eaeaa08e12efd714b0726e01d93d6f5bb1e4cdd34c1e5c96f7");
        }

        private static void Case_03596()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3596,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,6,94,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,13,44,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,20,38,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,18,38,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,14,94,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,15,52,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-5,51,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-16,21,29,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "48e53f77b13f283ced63b48b78200e35dda8075106a643e3b42e8d2aaafe1527");
        }

        private static void Case_03597()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3597,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-17,59,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-17,62,3,3), new GeneratedEnemyUnit(-14,-6,18,24,3), new GeneratedEnemyUnit(-9,18,25,37,4), new GeneratedEnemyUnit(-7,-18,50,44,4), new GeneratedEnemyUnit(-15,-20,80,2,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "07046ee373086524bed575a67436a38e4f85abb4b5fdfe704d18b730924d3771");
        }

        private static void Case_03598()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3598,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-1,69,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,9,84,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,1,31,15,2), new GeneratedEnemyUnit(-20,5,71,50,3), new GeneratedEnemyUnit(0,-16,83,27,3), new GeneratedEnemyUnit(15,-9,83,14,2), new GeneratedEnemyUnit(-19,-8,15,50,3), new GeneratedEnemyUnit(3,-15,19,38,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "4ca218989da986ec0f5bd5e0d4e34bef6223427f789d7f56bc0d31136ff340b1");
        }

        private static void Case_03599()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3599,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-10,63,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,20,58,38,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "7a4715fae1372e7bcc2e68355d7ed39bcaa3dddce9c0ee32c08401e3af0b0f36");
        }

    }
}
