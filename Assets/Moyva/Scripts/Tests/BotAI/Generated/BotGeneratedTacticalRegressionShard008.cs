using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard008
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_01440();
            Case_01441();
            Case_01442();
            Case_01443();
            Case_01444();
            Case_01445();
            Case_01446();
            Case_01447();
            Case_01448();
            Case_01449();
            Case_01450();
            Case_01451();
            Case_01452();
            Case_01453();
            Case_01454();
            Case_01455();
            Case_01456();
            Case_01457();
            Case_01458();
            Case_01459();
            Case_01460();
            Case_01461();
            Case_01462();
            Case_01463();
            Case_01464();
            Case_01465();
            Case_01466();
            Case_01467();
            Case_01468();
            Case_01469();
            Case_01470();
            Case_01471();
            Case_01472();
            Case_01473();
            Case_01474();
            Case_01475();
            Case_01476();
            Case_01477();
            Case_01478();
            Case_01479();
            Case_01480();
            Case_01481();
            Case_01482();
            Case_01483();
            Case_01484();
            Case_01485();
            Case_01486();
            Case_01487();
            Case_01488();
            Case_01489();
            Case_01490();
            Case_01491();
            Case_01492();
            Case_01493();
            Case_01494();
            Case_01495();
            Case_01496();
            Case_01497();
            Case_01498();
            Case_01499();
            Case_01500();
            Case_01501();
            Case_01502();
            Case_01503();
            Case_01504();
            Case_01505();
            Case_01506();
            Case_01507();
            Case_01508();
            Case_01509();
            Case_01510();
            Case_01511();
            Case_01512();
            Case_01513();
            Case_01514();
            Case_01515();
            Case_01516();
            Case_01517();
            Case_01518();
            Case_01519();
            Case_01520();
            Case_01521();
            Case_01522();
            Case_01523();
            Case_01524();
            Case_01525();
            Case_01526();
            Case_01527();
            Case_01528();
            Case_01529();
            Case_01530();
            Case_01531();
            Case_01532();
            Case_01533();
            Case_01534();
            Case_01535();
            Case_01536();
            Case_01537();
            Case_01538();
            Case_01539();
            Case_01540();
            Case_01541();
            Case_01542();
            Case_01543();
            Case_01544();
            Case_01545();
            Case_01546();
            Case_01547();
            Case_01548();
            Case_01549();
            Case_01550();
            Case_01551();
            Case_01552();
            Case_01553();
            Case_01554();
            Case_01555();
            Case_01556();
            Case_01557();
            Case_01558();
            Case_01559();
            Case_01560();
            Case_01561();
            Case_01562();
            Case_01563();
            Case_01564();
            Case_01565();
            Case_01566();
            Case_01567();
            Case_01568();
            Case_01569();
            Case_01570();
            Case_01571();
            Case_01572();
            Case_01573();
            Case_01574();
            Case_01575();
            Case_01576();
            Case_01577();
            Case_01578();
            Case_01579();
            Case_01580();
            Case_01581();
            Case_01582();
            Case_01583();
            Case_01584();
            Case_01585();
            Case_01586();
            Case_01587();
            Case_01588();
            Case_01589();
            Case_01590();
            Case_01591();
            Case_01592();
            Case_01593();
            Case_01594();
            Case_01595();
            Case_01596();
            Case_01597();
            Case_01598();
            Case_01599();
            Case_01600();
            Case_01601();
            Case_01602();
            Case_01603();
            Case_01604();
            Case_01605();
            Case_01606();
            Case_01607();
            Case_01608();
            Case_01609();
            Case_01610();
            Case_01611();
            Case_01612();
            Case_01613();
            Case_01614();
            Case_01615();
            Case_01616();
            Case_01617();
            Case_01618();
            Case_01619();
        }

        private static void Case_01440()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1440,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,18,18,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,5,86,43,1), new GeneratedEnemyUnit(-10,14,84,20,2), new GeneratedEnemyUnit(18,-13,84,45,2), new GeneratedEnemyUnit(-5,-12,51,26,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "eb7d3b6778bda80e4b8bf0d47417d690ea794f7cf44775aeda6e925cb12f830d");
        }

        private static void Case_01441()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1441,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,10,27,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,8,82,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,12,74,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,14,6,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,7,78,48,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "720388335d09871af88dd29caf7a0526bfb9a2bb587c0fff4fdb4248cf9fdc1d");
        }

        private static void Case_01442()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1442,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-8,23,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-17,7,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,8,43,7,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "01f2fd62535810b331b5f284c30c2ba8e7d44fbcbee0b044b67ad84d634b1ff1");
        }

        private static void Case_01443()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1443,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-14,67,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,15,57,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,12,39,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-18,79,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,15,78,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,3,14,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-9,28,28,4), new GeneratedEnemyUnit(17,0,41,43,2), new GeneratedEnemyUnit(7,20,92,46,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "4c92bab7a8ca66c948bea8eb87f981760e5d6e8ca4f1a86af4e07da19e381b27");
        }

        private static void Case_01444()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1444,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,1,41,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-15,8,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,1,96,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,8,18,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,4,85,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,19,76,31,3), new GeneratedEnemyUnit(18,-20,7,19,3), new GeneratedEnemyUnit(-1,-1,53,4,1), new GeneratedEnemyUnit(15,-1,35,32,4), new GeneratedEnemyUnit(-5,-14,41,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "88fdcd0194ef8aa5be143e44f02cf830ffa27f16eba3179cd7e1de7808a461f7");
        }

        private static void Case_01445()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1445,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-12,37,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,13,70,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-3,68,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-7,15,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,20,80,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-14,51,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "528b4b99ab7581a80f558c42c9c1e4e1bf59d67f3923ea398f4149cd5f2d2723");
        }

        private static void Case_01446()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1446,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,2,88,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,18,78,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-1,91,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,5,87,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,7,12,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-2,29,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-8,12,28,3), new GeneratedEnemyUnit(0,-19,39,43,2), new GeneratedEnemyUnit(-5,0,93,5,1), new GeneratedEnemyUnit(-8,20,62,22,2), new GeneratedEnemyUnit(-12,15,37,13,3), new GeneratedEnemyUnit(8,-17,37,21,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "9b7351522133f14f9a715339abe3ca0e566c8e20d146ce8f3e11c73c4bcbf5d5");
        }

        private static void Case_01447()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1447,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,9,81,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,14,28,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-16,68,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,16,37,3,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "861a75611e64817c1c5e6f09bc0e82d89dc1634711474cc67bb407836705e163");
        }

        private static void Case_01448()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1448,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,2,48,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-8,32,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-8,8,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-15,90,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-13,65,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-6,63,38,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "ca26ee44232d704982f128c3f062be90409bff8daa6bb91afad321144f3b88bd");
        }

        private static void Case_01449()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1449,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,12,37,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-6,81,4,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "9271b8fb3eca4b684e9c6eb0f312f8369ed24e087955c298388f1d04bfa57d90");
        }

        private static void Case_01450()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1450,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-3,95,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-7,89,1,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "f86111e479df05501f16511e4cbe21f93c0b05eafa5b48b0dd22e7862c55faff");
        }

        private static void Case_01451()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1451,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,2,99,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-18,35,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-4,17,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,19,90,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,17,15,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-11,66,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,5,100,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,5,24,39,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "bff6b8306acc2a4b8307f8197a407b62568ef36a0ce3d485e7c8dfcc76d879e4");
        }

        private static void Case_01452()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1452,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,6,95,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,15,83,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,20,67,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,10,63,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,3,43,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,19,49,10,4), new GeneratedEnemyUnit(-8,5,37,18,3), new GeneratedEnemyUnit(-9,-19,48,47,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "819bbdbc2caf9eae5ae812f8f153c5b2eec4143f573b5658abe8f33011a5c8ae");
        }

        private static void Case_01453()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1453,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-2,15,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-1,36,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,1,22,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-20,66,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,15,51,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-9,52,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-4,82,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,7,100,27,2), new GeneratedEnemyUnit(-4,0,71,9,3), new GeneratedEnemyUnit(13,18,62,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "b97307564cad70b721d3685d2222a3f966d9ca7bd4b0445646975726f4cae7e1");
        }

        private static void Case_01454()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1454,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-9,64,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,2,79,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-9,68,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,4,26,4,1), new GeneratedEnemyUnit(-17,18,20,30,3), new GeneratedEnemyUnit(19,-12,31,12,2), new GeneratedEnemyUnit(-11,17,85,15,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "35ce60e981a78987a339e6d4596cee1366f7e87b848e2f2c365a52f74edcffca");
        }

        private static void Case_01455()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1455,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,4,58,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-19,74,2,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "8eba106c25dbce8ed80323010a6337f8ec45ab14d252ec0c86c49f629201af0a");
        }

        private static void Case_01456()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1456,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,13,70,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,14,14,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-10,68,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-18,52,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-9,35,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-4,84,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-3,89,20,4), new GeneratedEnemyUnit(17,-7,46,36,4), new GeneratedEnemyUnit(10,15,60,42,2), new GeneratedEnemyUnit(-7,-6,88,44,2), new GeneratedEnemyUnit(9,-8,14,31,4), new GeneratedEnemyUnit(-15,-4,20,13,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "4192150e851581d2b85d5e2d0fe113bbd0fb3e6095706142655e69e75d55a1f3");
        }

        private static void Case_01457()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1457,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,6,72,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-8,30,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,8,17,6,4), new GeneratedEnemyUnit(-10,5,88,34,4), new GeneratedEnemyUnit(3,-15,59,9,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "49d644a711eee496ea6ed9b875930e5969319c5e3656580e8e253aec765b2025");
        }

        private static void Case_01458()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1458,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-19,85,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-11,45,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,19,15,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,0,14,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-3,60,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-7,55,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-13,60,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,13,65,21,3), new GeneratedEnemyUnit(-1,8,13,41,1), new GeneratedEnemyUnit(1,2,87,20,2), new GeneratedEnemyUnit(3,-15,100,47,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "52dac57f8c1e624bd9678da94279e3c528f89700ed69c4c0934a2ac1fe4757d4");
        }

        private static void Case_01459()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1459,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-14,76,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,16,55,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,19,100,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,17,84,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-3,37,44,3), new GeneratedEnemyUnit(-8,-19,71,33,4), new GeneratedEnemyUnit(-14,14,56,40,4), new GeneratedEnemyUnit(2,-4,93,35,4), new GeneratedEnemyUnit(11,-2,47,36,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8dcea86eae2a401859116f169d082b6ee08517cae25cceda1807e2e84cfac967");
        }

        private static void Case_01460()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1460,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-15,84,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,9,94,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,6,98,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-19,77,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,7,56,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-3,73,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,14,27,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,9,56,34,4), new GeneratedEnemyUnit(-14,-10,31,40,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "90c8ba2eba2d07ce19628aa3c005f1dd13863b22bd1eb6de21d942c95d60a63c");
        }

        private static void Case_01461()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1461,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-16,69,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,16,90,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,18,10,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-9,41,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,10,42,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,3,80,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,18,80,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,17,70,44,4), new GeneratedEnemyUnit(-1,19,6,13,1), new GeneratedEnemyUnit(3,2,20,6,4), new GeneratedEnemyUnit(-18,7,16,42,3), new GeneratedEnemyUnit(8,20,81,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "d05c0ecd2a04c05ffe249343792968404f8420414d34586fa8c31cfe0ff240e0");
        }

        private static void Case_01462()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1462,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-9,28,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,7,27,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,19,72,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-20,63,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-18,10,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,1,27,36,3), new GeneratedEnemyUnit(-14,9,49,18,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "141e6f851faca08be62717f723d6e1d197dcfdcd0f1ae52a26e0d4d22db01ef5");
        }

        private static void Case_01463()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1463,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,6,37,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,15,49,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-13,52,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,1,58,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,17,37,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,19,17,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,2,79,17,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "4181bc469ed51c8569082817f128f40eba549aebd48455fc92bdad53f95c116b");
        }

        private static void Case_01464()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1464,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-1,23,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,6,45,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-1,94,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-8,33,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,14,20,39,1), new GeneratedEnemyUnit(10,7,6,1,1), new GeneratedEnemyUnit(-3,15,8,16,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "0f566ab33703a969c1f6c2841f5be8471187686256d299adb463bb8ce081dcee");
        }

        private static void Case_01465()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1465,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-1,94,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,20,82,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,4,98,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-7,82,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-2,91,46,4), new GeneratedEnemyUnit(12,-12,8,13,2), new GeneratedEnemyUnit(-3,15,58,7,2), new GeneratedEnemyUnit(-16,2,58,5,2), new GeneratedEnemyUnit(20,3,30,28,3), new GeneratedEnemyUnit(8,8,93,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "61d3e7d067be35731cba137664b929b15864ba6d792455ab75f6934a114c5773");
        }

        private static void Case_01466()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1466,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-9,66,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,11,61,42,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "de17305b8ed95137244cd7aeb2bb83f516ff5cf2d6e4ee188b5e3793fac6eb27");
        }

        private static void Case_01467()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1467,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,7,36,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-14,90,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,11,5,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-20,52,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-13,12,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-5,40,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-1,19,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,6,73,27,3), new GeneratedEnemyUnit(19,-6,37,28,3), new GeneratedEnemyUnit(19,11,96,45,2), new GeneratedEnemyUnit(-10,-6,5,27,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "da23a2b1a0a79060c55bdd7a6a160c55725678d8af5090978fa6f8eb5aa970c6");
        }

        private static void Case_01468()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1468,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,19,61,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-16,76,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-1,52,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-6,53,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,7,93,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,5,51,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,7,10,28,1), new GeneratedEnemyUnit(-15,15,82,21,4), new GeneratedEnemyUnit(-8,-5,55,38,3), new GeneratedEnemyUnit(-16,7,85,34,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "e20ba29dbabf7a9b33c88f4b250a22ba9eef5a2249fb1a81138479f7b52f4f9a");
        }

        private static void Case_01469()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1469,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-10,19,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-10,57,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,12,48,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-5,100,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-3,88,3,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "815c5b007e05707496cd2a3ce6426e718e4874e489374ca11415b107242b43d8");
        }

        private static void Case_01470()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1470,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-14,39,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,17,68,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,3,7,4,2), new GeneratedEnemyUnit(14,6,24,35,4), new GeneratedEnemyUnit(14,6,76,20,4), new GeneratedEnemyUnit(-18,-4,40,27,1), new GeneratedEnemyUnit(-9,14,59,32,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "d2174eb1a172aca1b553cbce3e7f2c419f6aa4845f21b2e9cf9d03467dda48b6");
        }

        private static void Case_01471()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1471,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,14,23,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-2,62,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,15,83,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-12,5,4,1), new GeneratedEnemyUnit(-19,12,31,23,3), new GeneratedEnemyUnit(-14,2,76,12,1), new GeneratedEnemyUnit(9,9,97,2,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "b02e1a676865196c329f83db4d9947db86782efa2d70a724f77587bd9620218d");
        }

        private static void Case_01472()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1472,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-7,40,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-20,29,28,3), new GeneratedEnemyUnit(12,15,68,27,4), new GeneratedEnemyUnit(-10,5,55,43,4), new GeneratedEnemyUnit(-17,-18,75,34,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "025e25454abbbf73c60bcf8fa6419b3f975c777f41cb40d5e1b18b8ec533df59");
        }

        private static void Case_01473()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1473,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,5,14,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,20,29,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-8,30,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,3,84,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-5,10,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,2,35,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,17,48,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,12,54,3,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "4a616a261a7094be8ff0ee1fe11d8dbc8fc93a67628a73fa6a699dfaa02c17c1");
        }

        private static void Case_01474()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1474,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,14,83,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-5,19,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,15,32,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-1,37,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,19,17,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-16,99,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-7,57,23,3), new GeneratedEnemyUnit(-18,3,5,48,4), new GeneratedEnemyUnit(14,-4,53,23,2), new GeneratedEnemyUnit(-13,-7,84,36,2), new GeneratedEnemyUnit(17,-3,94,21,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "3a7bf0d5d6138172bb7f8b4899f745bf3824587ecdc00bff3745ce4af606fff9");
        }

        private static void Case_01475()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1475,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,17,6,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,17,28,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,13,28,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-12,81,34,1), new GeneratedEnemyUnit(15,-14,19,8,1), new GeneratedEnemyUnit(10,8,46,40,2), new GeneratedEnemyUnit(9,-19,17,49,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "33f56eb985381d1e605a1d40fd5bcb9174b9a13068e2fb19040ba1f4eb1cc935");
        }

        private static void Case_01476()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1476,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-3,42,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,7,27,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-11,78,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,2,59,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,17,78,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,16,27,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,6,82,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-18,61,13,2), new GeneratedEnemyUnit(17,15,95,33,4), new GeneratedEnemyUnit(-19,14,48,13,4), new GeneratedEnemyUnit(2,14,87,35,3), new GeneratedEnemyUnit(-14,-5,15,9,4), new GeneratedEnemyUnit(-12,-1,48,45,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "2f0a0c3abb518ae6823f659565aba20d737cf8e68da756aa60ff1e832e1e3c7d");
        }

        private static void Case_01477()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1477,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-6,99,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-3,10,7,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "88a046fc2e115b7f72bdb7cc432fd90b4d68886ffe17de79ca2fc4c58bcc0a2e");
        }

        private static void Case_01478()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1478,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,5,59,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-14,18,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-1,88,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,1,73,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,6,86,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,2,55,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-2,29,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-20,38,2,2), new GeneratedEnemyUnit(14,-20,28,38,2), new GeneratedEnemyUnit(14,-2,68,44,1), new GeneratedEnemyUnit(-11,-7,22,10,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "533e81af482d9dcec0e3ce91ea550d0ce25f44c0d2bc988cd0a2c678b39ee25c");
        }

        private static void Case_01479()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1479,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,12,24,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-20,84,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,5,18,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,11,38,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,20,33,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,5,60,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,1,18,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,9,79,26,4), new GeneratedEnemyUnit(17,7,78,30,2), new GeneratedEnemyUnit(-6,19,75,9,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "dbba2ef2529a27f0f69e906165200f29ac59d0e084074d3662fc94c780afa320");
        }

        private static void Case_01480()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1480,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,8,99,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,18,76,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,7,100,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,1,94,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-3,23,17,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ede12d333795507927663a2031cec52bb9f678a7da22401940285e1a1a4e3d24");
        }

        private static void Case_01481()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1481,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-12,56,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,1,66,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-11,34,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-16,56,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,9,51,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-3,93,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,6,48,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,11,50,2,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "707c349368465175e4a5b739f3fa5d1ab4de2cbf7e0745450361e93fa1b213f0");
        }

        private static void Case_01482()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1482,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-4,47,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-15,89,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,13,51,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,5,35,36,4), new GeneratedEnemyUnit(-10,-6,12,19,1), new GeneratedEnemyUnit(12,-18,51,23,2), new GeneratedEnemyUnit(-10,16,48,21,2), new GeneratedEnemyUnit(11,16,29,34,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "130133088f0e40a1be0d76d7d60ceb660f5e782833124db2690c617e35c90e94");
        }

        private static void Case_01483()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1483,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-17,87,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-11,49,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-11,38,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-16,49,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,12,28,50,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0161acb40c10537996fc3c7f844c5e4e83f86bcbd362d7d03414c9486e0c6f3e");
        }

        private static void Case_01484()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1484,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-11,7,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-11,95,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-4,45,1,3), new GeneratedEnemyUnit(-8,-11,75,3,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "cc94851e2eef85891e72e2e9da6d8fc79d4e5186d10a7200a71302539a9cb104");
        }

        private static void Case_01485()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1485,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,17,100,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-9,57,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-7,74,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-7,27,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-14,22,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,2,65,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-5,84,3,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "6e31673745a9d489eaaf3c1ed374b9eeb225e4fc3a80a1db70efe5602445477f");
        }

        private static void Case_01486()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1486,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-7,88,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-2,62,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,19,68,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-20,78,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,17,49,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-2,13,34,4), new GeneratedEnemyUnit(-5,17,23,29,2), new GeneratedEnemyUnit(-3,-3,14,49,1), new GeneratedEnemyUnit(5,3,53,48,3), new GeneratedEnemyUnit(3,0,10,21,3), new GeneratedEnemyUnit(-18,19,80,23,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "4265becb627385a41568276c0718ccbebe2a55ae88b8e98087d97c6fe74d4b92");
        }

        private static void Case_01487()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1487,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,15,94,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,17,88,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,13,42,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-2,78,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-9,77,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-3,56,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,14,50,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,12,58,5,2), new GeneratedEnemyUnit(-20,-13,64,2,3), new GeneratedEnemyUnit(4,-19,46,23,2), new GeneratedEnemyUnit(7,5,17,28,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "643c6e1c8fa887b2d6839704dcee1f3ae4b2b4383f2cbfe05f043e540f6255fc");
        }

        private static void Case_01488()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1488,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-15,69,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,15,56,16,3), new GeneratedEnemyUnit(6,-12,32,37,2), new GeneratedEnemyUnit(1,-1,72,45,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "10c7b50ac5e8377ef142f1a897cd2b4624259eb4e0873dcd4c595ba4e5fba9c9");
        }

        private static void Case_01489()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1489,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,0,70,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,16,88,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,14,47,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-16,19,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,10,13,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-10,40,5,3), new GeneratedEnemyUnit(-19,-16,96,41,1), new GeneratedEnemyUnit(-3,-8,89,19,4), new GeneratedEnemyUnit(12,2,66,8,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "1b5c0030134c95675649b88778b121f02135070bd3e78a2ef1af5048679f5e02");
        }

        private static void Case_01490()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1490,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,13,85,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-4,10,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-7,34,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,12,47,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-6,68,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-13,54,39,1), new GeneratedEnemyUnit(-12,-2,12,50,2), new GeneratedEnemyUnit(-19,10,38,3,2), new GeneratedEnemyUnit(1,16,77,30,4), new GeneratedEnemyUnit(7,7,82,37,1), new GeneratedEnemyUnit(-2,-17,71,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "9be5adb7a15072c45342790c1799b9169c94177ae9520f186f1329f3db894db5");
        }

        private static void Case_01491()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1491,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-6,5,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-17,66,25,3), new GeneratedEnemyUnit(16,-4,78,13,1), new GeneratedEnemyUnit(15,-10,100,17,3), new GeneratedEnemyUnit(7,12,32,36,1), new GeneratedEnemyUnit(10,9,78,30,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "76f94345ef5465823bed7b2a58f2f568fb0cb8fa0372f0deb0b8fec926b8f603");
        }

        private static void Case_01492()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1492,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,14,95,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-20,78,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,7,64,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-2,98,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,14,78,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-10,79,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,5,53,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,8,68,1,3), new GeneratedEnemyUnit(1,12,77,28,2), new GeneratedEnemyUnit(-10,16,86,20,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "75694d6c2d73a9eeaa9b12411708151d34ed53ba763d93f3e513e791d6064e97");
        }

        private static void Case_01493()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1493,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-12,25,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-4,23,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-6,86,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-18,84,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,17,85,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-11,32,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-1,51,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,15,100,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,0,73,22,1), new GeneratedEnemyUnit(-18,-1,61,5,1), new GeneratedEnemyUnit(17,15,88,26,3), new GeneratedEnemyUnit(3,4,61,29,4), new GeneratedEnemyUnit(-8,17,43,17,3), new GeneratedEnemyUnit(-6,-10,39,12,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "685c420fb6419239d325b8a99ab16863338b82c03fe119cf56a338ea1487f585");
        }

        private static void Case_01494()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1494,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-2,57,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-11,86,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-8,20,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-6,97,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,17,81,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-18,72,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-12,32,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,11,31,26,2), new GeneratedEnemyUnit(20,-9,38,32,4), new GeneratedEnemyUnit(4,8,98,15,3), new GeneratedEnemyUnit(-18,9,38,44,3), new GeneratedEnemyUnit(-19,19,55,50,3), new GeneratedEnemyUnit(7,-11,98,35,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "08126a09399843cb55661fb31e77c3dac1dca254e757cc09f83a2971ce17c749");
        }

        private static void Case_01495()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1495,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,20,78,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,0,73,40,3), new GeneratedEnemyUnit(11,10,35,27,3), new GeneratedEnemyUnit(-13,-16,16,10,2), new GeneratedEnemyUnit(-12,18,28,6,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "f87c0533f1379f2219b855e0b2446ce99bbf79af4a468cbc3f9f5837d7057855");
        }

        private static void Case_01496()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1496,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,0,6,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-17,62,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,17,74,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,15,17,49,2), new GeneratedEnemyUnit(10,-15,93,10,3), new GeneratedEnemyUnit(16,19,75,24,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "a727ac735cbe270bd0a3d908326ad687472f11a016a82344fc493239dd05f591");
        }

        private static void Case_01497()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1497,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-6,55,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,5,47,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,16,42,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,2,53,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,10,27,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,10,48,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,18,18,48,2), new GeneratedEnemyUnit(1,8,95,6,1), new GeneratedEnemyUnit(15,-15,99,11,2), new GeneratedEnemyUnit(5,-8,94,46,1), new GeneratedEnemyUnit(19,12,83,13,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "9c002a64b67f924d2467f812f79af1999cc16eba1201c094ad0f3cbcf3248451");
        }

        private static void Case_01498()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1498,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-8,96,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-8,56,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,5,5,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,5,79,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,3,25,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,5,51,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-7,8,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-12,64,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,19,35,6,1), new GeneratedEnemyUnit(-2,-10,99,42,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "95dae37360886209b9f349b3e3e681c619c847e753fe627f17d4b729741666e2");
        }

        private static void Case_01499()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1499,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,2,72,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-13,37,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,14,99,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,6,53,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,14,14,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-5,6,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,11,27,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-13,23,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-3,36,3,4), new GeneratedEnemyUnit(-7,10,71,6,4), new GeneratedEnemyUnit(19,2,82,11,1), new GeneratedEnemyUnit(20,-18,94,20,4), new GeneratedEnemyUnit(-12,-11,29,42,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "c5cd0d8787b0bacdbe3e37978b49f6cd4482bd931c45e31b18e31704cdf0a137");
        }

        private static void Case_01500()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1500,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-18,42,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,10,32,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,8,72,6,2), new GeneratedEnemyUnit(1,-20,100,7,2), new GeneratedEnemyUnit(14,-14,19,27,1), new GeneratedEnemyUnit(11,-11,54,6,1), new GeneratedEnemyUnit(-19,19,14,25,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "3f9e9e7fa84314a7a5b1852d13837a0625aea2db63a74f3b07909fdd4db849c4");
        }

        private static void Case_01501()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1501,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,8,24,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-14,83,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-5,30,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,14,34,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-3,69,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,20,61,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-15,25,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,12,16,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-7,44,46,1), new GeneratedEnemyUnit(-5,11,81,30,2), new GeneratedEnemyUnit(-19,5,78,37,1), new GeneratedEnemyUnit(4,-12,33,45,3), new GeneratedEnemyUnit(-18,12,41,30,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "ad5221bd76d3b62a7438467e301ce319361f3f51944f85f4cdd72087699131bc");
        }

        private static void Case_01502()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1502,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,18,20,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-13,62,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-18,11,17,4), new GeneratedEnemyUnit(-1,19,91,11,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "14bb03e1d2aaf98c424b92eaca679240bccd91d509c5aa08baf34eeb256dfbf6");
        }

        private static void Case_01503()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1503,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-16,66,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,15,86,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,10,23,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,16,80,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-18,85,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,6,23,24,4), new GeneratedEnemyUnit(-10,-16,69,23,4), new GeneratedEnemyUnit(11,11,25,41,2), new GeneratedEnemyUnit(-11,15,59,27,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8025a9287c04453cab42c7c9224e04c2e1ac996748e2e4fb90d61d7c40a39e4e");
        }

        private static void Case_01504()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1504,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-3,6,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-11,30,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,10,17,8,4), new GeneratedEnemyUnit(1,-7,62,16,3), new GeneratedEnemyUnit(4,-18,67,14,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "7b5a2ac2934f162d93df79e79d35b3074ceb2c3030f3ce916eb88bf53f049e80");
        }

        private static void Case_01505()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1505,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-10,31,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-10,67,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-3,71,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,6,30,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,8,38,9,1), new GeneratedEnemyUnit(-19,-2,83,7,4), new GeneratedEnemyUnit(15,-9,6,11,2), new GeneratedEnemyUnit(14,-14,14,33,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "dd0dca05b140fe7098eaf5aeadc8383d109972f3bc18e5a75960bf3799ef62a8");
        }

        private static void Case_01506()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1506,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,4,64,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,18,27,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-4,88,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,18,33,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,10,77,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-8,53,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-8,87,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,2,94,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-20,31,17,1), new GeneratedEnemyUnit(13,12,100,29,1), new GeneratedEnemyUnit(2,19,45,31,1), new GeneratedEnemyUnit(16,16,42,30,4), new GeneratedEnemyUnit(17,7,21,22,4), new GeneratedEnemyUnit(-12,14,35,25,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "1ccd703ef34bfdacff4743fad04b556a6be3b4d4ee0e85a073b87b025a50a43c");
        }

        private static void Case_01507()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1507,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-2,15,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-5,83,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-16,75,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-3,45,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,20,27,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,20,52,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-1,76,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,17,28,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,18,39,26,2), new GeneratedEnemyUnit(1,4,29,40,4), new GeneratedEnemyUnit(5,11,40,34,3), new GeneratedEnemyUnit(-4,17,75,21,1), new GeneratedEnemyUnit(11,20,62,42,4), new GeneratedEnemyUnit(-15,-13,89,33,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "84e6a34319bc9e10bb01522e7ee2c95f52c957c25114817d3849d954eabf6b71");
        }

        private static void Case_01508()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1508,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,17,60,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-14,89,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,6,95,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-2,22,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-2,68,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,10,98,23,1), new GeneratedEnemyUnit(-16,-17,60,5,3), new GeneratedEnemyUnit(2,-3,6,15,4), new GeneratedEnemyUnit(-1,6,57,47,4), new GeneratedEnemyUnit(16,12,95,44,4), new GeneratedEnemyUnit(10,2,24,23,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "1cad992bda9520ff473dd9dfa340a4ef8551dbbf0ea93287f0c0c503e90ec706");
        }

        private static void Case_01509()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1509,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-5,53,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,15,91,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,2,66,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,14,90,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,2,26,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,1,86,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-13,82,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,5,87,1,1), new GeneratedEnemyUnit(-1,-9,65,6,4), new GeneratedEnemyUnit(-7,-5,91,39,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "55fb5f9b21e030e620ce8f56e12698e5382a30f7d708ec105c939744adc155e0");
        }

        private static void Case_01510()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1510,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,11,52,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-2,31,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,10,57,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-4,25,16,4), new GeneratedEnemyUnit(12,-14,75,39,1), new GeneratedEnemyUnit(-18,-11,77,3,3), new GeneratedEnemyUnit(9,-14,79,45,2), new GeneratedEnemyUnit(-18,15,33,44,3), new GeneratedEnemyUnit(1,9,31,34,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "2ae21a2b0bdc292ebe54fbbc32e5211c2e812cf144f0179cb9638d7a113d1552");
        }

        private static void Case_01511()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1511,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-12,19,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,19,41,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,16,23,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,0,53,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-14,87,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,17,87,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,9,55,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,15,36,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,8,22,48,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "5a298df54b7956124d7592d1e831a6f59d22749385595743d373e29321fece34");
        }

        private static void Case_01512()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1512,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,4,14,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-10,57,4,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "9ebb5f94b529bcd5d7179ca70dd79f1ce0181e599977766091193935d2a49504");
        }

        private static void Case_01513()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1513,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-19,100,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,19,35,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,12,55,14,3), new GeneratedEnemyUnit(-18,-1,7,13,3), new GeneratedEnemyUnit(7,18,38,47,1), new GeneratedEnemyUnit(18,-18,87,46,3), new GeneratedEnemyUnit(-20,-4,31,18,3), new GeneratedEnemyUnit(-8,-5,12,3,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "0fb6e686f3eabb25cff95a4e295e80efb92ea46d8acf20e3d927e662d28567d3");
        }

        private static void Case_01514()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1514,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-20,48,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-11,26,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,3,41,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-16,45,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-14,42,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,12,89,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,7,60,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-18,36,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,12,79,48,3), new GeneratedEnemyUnit(10,-11,13,44,2), new GeneratedEnemyUnit(18,-1,82,29,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "0d3e14e6b2eff23ce0e1d5a4167c913243c1cd23cbb9ace4d67d310b9fe41928");
        }

        private static void Case_01515()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1515,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-12,23,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-12,87,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,19,25,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,20,27,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,5,31,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,13,57,24,1), new GeneratedEnemyUnit(1,4,14,41,1), new GeneratedEnemyUnit(-17,-16,66,44,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "c32aa9d084537aec78854e0155639ceb4e2844886b5be4b41aa68c8920d71621");
        }

        private static void Case_01516()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1516,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-18,64,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,8,91,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-10,36,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-13,52,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-16,31,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,1,31,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-7,90,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,4,78,20,4), new GeneratedEnemyUnit(3,4,83,23,1), new GeneratedEnemyUnit(-3,-1,59,41,2), new GeneratedEnemyUnit(11,-6,95,39,4), new GeneratedEnemyUnit(-15,14,99,15,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "a47632e48c7f745bc3bbaaaf50e9fe992d59546a43c89e443401380fd510768c");
        }

        private static void Case_01517()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1517,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,10,54,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-9,94,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-10,44,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-19,17,4,1), new GeneratedEnemyUnit(9,-1,8,42,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "7d40aebb0a3f00d17371f0233abe82ac222e95c247cf4056c1588840e69e9c7d");
        }

        private static void Case_01518()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1518,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-5,69,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,4,24,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-9,21,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,7,95,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,9,99,3,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "486bd8672217e248531dbafe22db33f3c1abd40e59b3a216ca283bfc2f7f52f7");
        }

        private static void Case_01519()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1519,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-18,31,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-13,21,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-10,51,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,4,9,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,14,10,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,7,31,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,11,18,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-7,76,31,4), new GeneratedEnemyUnit(-18,-15,43,3,2), new GeneratedEnemyUnit(14,5,22,39,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "1fb21745b9eb0e1fbe73eeabf9b061d4bae83feb6b776863dd3d01e4c27f8a1c");
        }

        private static void Case_01520()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1520,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-7,8,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-3,10,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,3,81,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-1,68,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,17,8,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,14,10,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-18,34,38,1), new GeneratedEnemyUnit(-3,-17,64,6,3), new GeneratedEnemyUnit(8,-11,33,42,4), new GeneratedEnemyUnit(14,-13,75,4,3), new GeneratedEnemyUnit(-17,-14,39,31,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "fabfef475cd7fd01bbf4afd37a7f020a7d98fa6b6ba1fa575cf3e32baedd4ceb");
        }

        private static void Case_01521()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1521,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,5,74,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,0,28,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-7,83,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-14,44,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-11,84,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,10,50,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,13,99,12,3), new GeneratedEnemyUnit(3,-11,88,26,1), new GeneratedEnemyUnit(7,-6,94,28,3), new GeneratedEnemyUnit(-1,14,57,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "af0cc15a171433d29a51ab05a6bb1c607118226ed1ae194daa2ffbf32e1db478");
        }

        private static void Case_01522()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1522,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-19,76,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-1,52,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-5,100,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-2,12,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,14,15,21,2), new GeneratedEnemyUnit(13,1,18,2,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "5424bafee002341e0b335a9abd2f95d6694d918b69c60d317f15dda09f48ad1c");
        }

        private static void Case_01523()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1523,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,8,16,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-10,43,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-8,30,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,0,22,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-3,58,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,12,68,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-5,49,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-17,10,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-10,27,13,2), new GeneratedEnemyUnit(6,-16,73,17,2), new GeneratedEnemyUnit(0,12,66,36,4), new GeneratedEnemyUnit(-14,-12,55,14,2), new GeneratedEnemyUnit(-13,-14,82,36,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "7724029304059dc67cf2147d02622d0f689073624798a11da27386783a405eb6");
        }

        private static void Case_01524()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1524,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-15,56,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-20,55,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,15,29,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-8,65,43,1), new GeneratedEnemyUnit(2,-1,18,24,3), new GeneratedEnemyUnit(-16,8,69,36,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "836a409ef41503355f4b84b0ee6a5e226d890bfd7d2f78d98a58360a08a4c421");
        }

        private static void Case_01525()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1525,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,16,28,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,14,19,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-18,58,39,2), new GeneratedEnemyUnit(-1,0,23,9,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "268d6fa824a8e3ce67fdc657efb41784635cbcbdc9f654571778975703afe88a");
        }

        private static void Case_01526()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1526,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-8,94,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-2,57,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-17,21,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-10,73,21,1), new GeneratedEnemyUnit(-14,7,98,41,1), new GeneratedEnemyUnit(1,4,30,22,3), new GeneratedEnemyUnit(13,8,9,18,3), new GeneratedEnemyUnit(3,-19,86,43,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "eeba3d2c563ee01703a76de45ea74cd6db04fb82ce4cce4621f228a1a2f2f927");
        }

        private static void Case_01527()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1527,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-9,23,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-9,35,31,3), new GeneratedEnemyUnit(3,-13,79,44,1), new GeneratedEnemyUnit(14,-10,29,33,2), new GeneratedEnemyUnit(14,-14,52,39,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "4e4d25edf0edcd1cfe3ffae4086b27191fee9492d5815e18b49cc60e09ffdbe3");
        }

        private static void Case_01528()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1528,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,9,5,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,16,9,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-12,83,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-9,73,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,5,77,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-12,61,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,13,93,40,4), new GeneratedEnemyUnit(14,14,93,22,4), new GeneratedEnemyUnit(2,14,98,40,4), new GeneratedEnemyUnit(-11,19,45,21,4), new GeneratedEnemyUnit(17,-13,99,11,3), new GeneratedEnemyUnit(-13,-10,53,12,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "04194916b6dd0d0ec557913bdffd39766165a7c337cd54d5d9fff6a70a47620d");
        }

        private static void Case_01529()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1529,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-14,65,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,13,35,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-16,15,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-7,61,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,12,77,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-14,45,49,4), new GeneratedEnemyUnit(7,3,51,18,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "909928b58d76f9ff2eb546305170a6b02acb79b270ab42d14db10d68e9aaacc0");
        }

        private static void Case_01530()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1530,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-17,48,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-5,8,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-11,5,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,17,23,19,2), new GeneratedEnemyUnit(4,10,34,19,2), new GeneratedEnemyUnit(-10,-18,23,37,2), new GeneratedEnemyUnit(6,6,87,34,4), new GeneratedEnemyUnit(20,-3,78,29,4), new GeneratedEnemyUnit(-10,-14,33,20,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "9445e56a24ab5c636546bee310fb4483a39f267535fc5050c114e89d32d6eb00");
        }

        private static void Case_01531()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1531,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-9,54,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-16,34,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,7,66,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,4,10,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,4,18,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-16,68,14,1), new GeneratedEnemyUnit(-13,17,26,16,2), new GeneratedEnemyUnit(-14,-14,67,43,3), new GeneratedEnemyUnit(-5,1,99,16,2), new GeneratedEnemyUnit(20,10,42,15,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "81ea3505aeb8cc08a64c57b288d818327e39238a86989511f3a58827319a58ec");
        }

        private static void Case_01532()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1532,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,15,97,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-20,69,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-6,80,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-9,73,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-9,73,26,1), new GeneratedEnemyUnit(0,-17,57,15,3), new GeneratedEnemyUnit(-9,9,7,13,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "e0aef0c0c8c3075ab1c46bf257b4926b56fed089ae5e39043e250d8fc41f1499");
        }

        private static void Case_01533()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1533,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,20,83,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,4,71,24,2), new GeneratedEnemyUnit(7,16,88,12,3), new GeneratedEnemyUnit(9,-7,73,31,1), new GeneratedEnemyUnit(12,16,95,6,4), new GeneratedEnemyUnit(-6,0,25,35,1), new GeneratedEnemyUnit(-18,-20,99,47,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "3a7d359596234ab777bf5e910268510114598c1e47c509c19c9cb172a6c773c5");
        }

        private static void Case_01534()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1534,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-5,95,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-3,24,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-8,36,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-15,42,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-17,41,10,1), new GeneratedEnemyUnit(5,5,63,46,3), new GeneratedEnemyUnit(19,10,42,11,1), new GeneratedEnemyUnit(-14,18,70,31,3), new GeneratedEnemyUnit(-2,16,12,23,2), new GeneratedEnemyUnit(-2,-19,96,17,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "9957db3cacca1061785c2dcfe9e1cb6a71d0d7930501f9c41bf66dc9e2ed730f");
        }

        private static void Case_01535()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1535,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-1,30,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,18,67,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,6,33,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,9,85,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-11,96,5,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "139dafddbb6c6337a9ffc29081618352ad7372bf878e7f994e6461a2a14b705b");
        }

        private static void Case_01536()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1536,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-19,78,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-5,6,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,11,6,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-7,96,21,3), new GeneratedEnemyUnit(-7,-2,53,37,3), new GeneratedEnemyUnit(-15,-12,94,9,3), new GeneratedEnemyUnit(3,-8,7,28,4), new GeneratedEnemyUnit(-1,-15,99,8,2), new GeneratedEnemyUnit(-3,-12,73,45,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "38e33a9f0030e36338f6fb204b9c35206b8a4ea17b885da54c42e97c68aa7b53");
        }

        private static void Case_01537()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1537,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,3,19,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,15,38,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-11,22,9,1), new GeneratedEnemyUnit(8,-15,80,12,4), new GeneratedEnemyUnit(-10,14,20,4,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "7f288a1f10998505f2f0c3575a1f2b8781d680a15b0122fa1e6c9387c4cae6eb");
        }

        private static void Case_01538()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1538,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,8,95,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,2,9,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,6,52,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,3,68,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,0,39,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-11,29,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,11,7,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-18,97,2,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "bf9183799d2f816cfb58dadb3c4a999f0fddfc2b6a9052fea53ebe3fb0469b25");
        }

        private static void Case_01539()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1539,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,2,66,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,6,47,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,11,94,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-5,71,45,1), new GeneratedEnemyUnit(14,19,59,44,2), new GeneratedEnemyUnit(-5,18,45,14,1), new GeneratedEnemyUnit(11,14,83,44,1), new GeneratedEnemyUnit(5,1,100,36,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "12ab9349f70ab7ecf66c8c05424fea16982f064f5fe413f9ec2d55156fe32204");
        }

        private static void Case_01540()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1540,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,13,61,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-9,44,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,3,28,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-11,46,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-11,89,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,6,27,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-10,23,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-5,95,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,5,46,49,4), new GeneratedEnemyUnit(13,-3,59,28,2), new GeneratedEnemyUnit(1,3,50,25,4), new GeneratedEnemyUnit(5,6,6,40,1), new GeneratedEnemyUnit(-5,20,12,1,3), new GeneratedEnemyUnit(-20,0,48,21,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "594fc6c8edd02aa591587d48bcde8c45c74ac15ee27aac924d935d8b1bbe8ad4");
        }

        private static void Case_01541()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1541,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,14,13,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,17,49,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-12,31,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,16,43,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,17,43,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,6,5,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-15,76,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "ccebf251116f871942fc4fa8c030de3067e19827884631f407b6b32f0bf85a7e");
        }

        private static void Case_01542()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1542,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-17,39,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,9,19,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,1,24,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,17,12,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,4,95,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,5,33,41,2), new GeneratedEnemyUnit(15,-20,9,36,1), new GeneratedEnemyUnit(-17,-7,69,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "b4ac9b37a1fa0c0a7b65af855acaedcd2f36f2a47db72e601ba113645064f861");
        }

        private static void Case_01543()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1543,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-9,60,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-14,22,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-4,20,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-12,14,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-16,39,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,20,28,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-19,56,29,2), new GeneratedEnemyUnit(-11,11,43,20,4), new GeneratedEnemyUnit(13,2,36,20,1), new GeneratedEnemyUnit(-2,17,56,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "b36fe08b17f6e490f1766d1eac963828e8396d41549264398518690b2ff1915c");
        }

        private static void Case_01544()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1544,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-5,21,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-12,76,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-1,71,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,10,64,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,17,45,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-2,23,3,2), new GeneratedEnemyUnit(-5,2,18,32,4), new GeneratedEnemyUnit(-10,-18,64,21,3), new GeneratedEnemyUnit(4,11,12,16,3), new GeneratedEnemyUnit(1,-20,77,15,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "8bd93c1aa624bf0351ec4d8c385216dc68c523281582395cece67f0bc6484cec");
        }

        private static void Case_01545()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1545,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-10,94,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,20,14,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-16,42,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,13,42,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,8,57,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,16,42,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,18,86,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-7,83,36,2), new GeneratedEnemyUnit(-7,16,54,4,1), new GeneratedEnemyUnit(-10,20,85,15,4), new GeneratedEnemyUnit(11,14,20,10,1), new GeneratedEnemyUnit(-16,-16,68,17,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "80e11286007a090ff0f6437a4af699874c34d77fedbaa5da5ec2d41bdcd13212");
        }

        private static void Case_01546()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1546,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-2,44,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-18,94,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,14,69,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,15,63,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,7,19,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-10,13,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-2,70,30,4), new GeneratedEnemyUnit(-10,18,11,6,2), new GeneratedEnemyUnit(11,11,8,45,4), new GeneratedEnemyUnit(-5,-17,62,26,4), new GeneratedEnemyUnit(0,-16,36,20,1), new GeneratedEnemyUnit(-6,-5,12,17,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "4104e3f45d0139a1995cfeae773e75269b23770128d87fe85f9cc98bcb343570");
        }

        private static void Case_01547()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1547,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,10,40,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,2,56,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,12,25,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,6,78,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,20,56,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-18,7,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,5,50,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-13,60,6,3), new GeneratedEnemyUnit(17,-5,57,2,3), new GeneratedEnemyUnit(-8,-6,98,25,3), new GeneratedEnemyUnit(10,-11,46,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "52fb4471f5ec6c7cc06bd1f0dac3523035c46ca8136e0eb7d90bdfdcd9e47583");
        }

        private static void Case_01548()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1548,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-6,42,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-17,13,26,4), new GeneratedEnemyUnit(-4,-15,76,48,3), new GeneratedEnemyUnit(5,11,28,49,2), new GeneratedEnemyUnit(-20,0,66,45,2), new GeneratedEnemyUnit(10,5,59,21,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "da4460814f08920c94ecc806ce26c73b1324d8f98a60eefc3aea36c653de8d61");
        }

        private static void Case_01549()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1549,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,6,66,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-18,81,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-3,6,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-19,47,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-4,55,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,6,71,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,13,27,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-10,76,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,20,59,46,4), new GeneratedEnemyUnit(-2,4,16,44,2), new GeneratedEnemyUnit(-20,11,20,43,1), new GeneratedEnemyUnit(0,14,15,38,4), new GeneratedEnemyUnit(20,11,51,32,3), new GeneratedEnemyUnit(11,13,89,6,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "496a22648fbc9efc9c2703b6f8b12b11bd1d23442da53331b8e554d26ffdace3");
        }

        private static void Case_01550()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1550,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,15,31,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,1,83,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-7,97,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-6,74,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,18,62,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-2,10,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,14,51,36,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "854f815cbc9368ba95782faf0e195664d186c636477918f3b089ed8046f64636");
        }

        private static void Case_01551()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1551,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,19,97,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-14,51,4,4), new GeneratedEnemyUnit(-6,-5,86,43,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "44abe5b295da833f2c4a19ee2a3e5746ba03c27c9c20dd39c2523bb1e8b63342");
        }

        private static void Case_01552()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1552,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,20,33,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,8,42,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-9,50,17,3), new GeneratedEnemyUnit(-2,-16,50,38,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "f6aaaa3263f2a852f6d2261d931607e6627ce158f332751a853e3a3e84287428");
        }

        private static void Case_01553()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1553,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-4,100,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-4,95,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-2,68,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,12,49,24,3), new GeneratedEnemyUnit(10,11,23,25,1), new GeneratedEnemyUnit(-9,-15,95,28,1), new GeneratedEnemyUnit(2,-8,17,27,2), new GeneratedEnemyUnit(1,-15,30,50,3), new GeneratedEnemyUnit(2,19,87,34,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "c384974d386343b80b48d5dc32f8ea8e7229681761c241a1766d548bed148582");
        }

        private static void Case_01554()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1554,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,10,21,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-2,30,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-1,44,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,1,80,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-13,26,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-1,56,35,2), new GeneratedEnemyUnit(-17,2,18,15,2), new GeneratedEnemyUnit(12,20,42,37,1), new GeneratedEnemyUnit(9,19,28,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "4c22887676d7ef2d723c6e1d8d73ba2d4d4f117215b2131911afd74bc8df8e9e");
        }

        private static void Case_01555()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1555,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-9,62,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,3,44,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,12,19,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,3,31,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,3,94,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,9,52,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,6,10,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,7,66,50,2), new GeneratedEnemyUnit(-6,-9,17,3,3), new GeneratedEnemyUnit(-18,11,86,25,3), new GeneratedEnemyUnit(-4,-14,72,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "fbe40b4a15c3d2fa6e01db5e18e1b75ab625a0a362fb15b605388734769c990b");
        }

        private static void Case_01556()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1556,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,8,5,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,1,56,36,4), new GeneratedEnemyUnit(18,-12,98,41,2), new GeneratedEnemyUnit(14,15,23,5,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "784ef620760a6b8b026b8cc90fbcfb1a5b2a8baa355260ee0c34e35b555e9f52");
        }

        private static void Case_01557()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1557,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-19,51,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-2,92,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,9,82,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,6,38,45,3), new GeneratedEnemyUnit(-12,9,95,27,2), new GeneratedEnemyUnit(-16,-11,86,5,2), new GeneratedEnemyUnit(10,-6,38,43,4), new GeneratedEnemyUnit(0,-19,85,45,4), new GeneratedEnemyUnit(-11,11,83,12,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "60476ae6e25ab93e7674c8e80b992a2a1cdfc980c363443380f15af5c11a638b");
        }

        private static void Case_01558()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1558,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,6,78,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-5,52,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,8,58,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,1,53,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-13,44,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,8,70,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,11,69,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,0,19,41,4), new GeneratedEnemyUnit(-4,-18,81,44,4), new GeneratedEnemyUnit(-10,12,28,20,2), new GeneratedEnemyUnit(2,-16,30,15,3), new GeneratedEnemyUnit(-19,12,47,22,4), new GeneratedEnemyUnit(3,10,100,17,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8188c79632ffd7955024ea7d7a0a172d14c7f59db37ff54bb4d74e3d27533f8d");
        }

        private static void Case_01559()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1559,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-2,95,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-3,62,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,17,99,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-4,41,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "1de47b0f64a46998778c857bde3a2227e5b0a6fdd0114845d038d2cf57b10b3a");
        }

        private static void Case_01560()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1560,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,5,80,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-2,10,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-15,70,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,3,88,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-7,14,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-1,54,37,2), new GeneratedEnemyUnit(18,18,70,8,4), new GeneratedEnemyUnit(6,13,70,14,1), new GeneratedEnemyUnit(6,-4,23,18,1), new GeneratedEnemyUnit(16,8,16,28,4), new GeneratedEnemyUnit(18,17,9,33,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "a8fbf9f9e9bf7516bb465c1f7a9af4c267beea40d911447073b32e542da9a64b");
        }

        private static void Case_01561()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1561,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,15,36,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-17,23,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,2,36,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-17,27,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-16,80,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,12,29,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,18,7,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,20,72,6,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "f79602f4e5b1324c91760bf9180ea28e3dd374ae2c1a970fbd4d421d3707e7c2");
        }

        private static void Case_01562()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1562,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-18,44,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,1,15,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,8,99,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,14,81,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,18,84,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,6,59,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,0,85,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-10,37,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,15,68,50,1), new GeneratedEnemyUnit(18,5,89,36,1), new GeneratedEnemyUnit(15,13,35,38,2), new GeneratedEnemyUnit(-1,-20,78,4,1), new GeneratedEnemyUnit(-10,3,23,41,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "e7525203782026041e03f22392d060c60571efe6060785b04a4d0aa866a73bde");
        }

        private static void Case_01563()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1563,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,14,87,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-14,34,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,9,76,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,5,6,26,2), new GeneratedEnemyUnit(-9,-20,96,29,4), new GeneratedEnemyUnit(-8,-2,80,48,1), new GeneratedEnemyUnit(-2,20,44,45,1), new GeneratedEnemyUnit(20,5,39,31,3), new GeneratedEnemyUnit(-20,14,11,10,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "8e4fcbc331ce56297b727ebb4765f8c9efd2938ae20a70cb3dbf688a8ec9d473");
        }

        private static void Case_01564()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1564,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-18,97,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,6,58,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-13,49,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-19,31,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-12,81,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-12,6,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,2,39,13,4), new GeneratedEnemyUnit(11,-6,42,14,4), new GeneratedEnemyUnit(13,12,95,18,2), new GeneratedEnemyUnit(18,-5,85,35,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "ed72de635a0addfd3dd9ae90b25de70c9bb78af1ff73e365432db8afc9659526");
        }

        private static void Case_01565()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1565,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-14,60,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,12,97,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-14,22,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,9,22,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,17,27,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,14,68,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-8,7,45,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2cce502ba57d3bb8015bbbae862f1f2a96c9d0a377ab628ff2feaee37ff944b5");
        }

        private static void Case_01566()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1566,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,0,83,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,18,41,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-14,66,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-3,83,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-16,91,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,6,60,19,3), new GeneratedEnemyUnit(17,-15,22,3,1), new GeneratedEnemyUnit(-5,4,21,40,2), new GeneratedEnemyUnit(-3,4,30,44,1), new GeneratedEnemyUnit(-7,4,59,36,4), new GeneratedEnemyUnit(17,-17,7,30,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "812b82b7b6af7a978304120921176b754ef903d5a6aaaa7f1483885141209f75");
        }

        private static void Case_01567()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1567,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-14,61,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,13,97,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-8,19,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-2,63,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,1,85,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-19,42,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,12,63,35,2), new GeneratedEnemyUnit(-12,-15,6,50,2), new GeneratedEnemyUnit(-19,-18,44,41,3), new GeneratedEnemyUnit(-15,10,69,19,2), new GeneratedEnemyUnit(-1,13,35,24,3), new GeneratedEnemyUnit(13,-8,62,37,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "c6ee8de3b214d372a1a4b2496b99c07484dbb721c6d20f8a9fef274a424405e5");
        }

        private static void Case_01568()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1568,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-16,63,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,2,39,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,19,12,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,3,10,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,10,27,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-6,36,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-8,69,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,7,78,46,3), new GeneratedEnemyUnit(12,3,98,38,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "a347c71e9cea981fd225b8f943a5ae22775a9c10e313e574c5e0211101562534");
        }

        private static void Case_01569()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1569,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,13,30,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-17,91,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,8,11,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-15,95,6,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "79be24155840ff15b5dbe971c37a7c7c0e7bf7529a94c1fe67fe5e110ce188a4");
        }

        private static void Case_01570()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1570,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-1,14,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-4,63,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,16,73,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-17,30,13,2), new GeneratedEnemyUnit(-10,-14,52,45,2), new GeneratedEnemyUnit(6,19,22,37,3), new GeneratedEnemyUnit(5,-7,39,33,4), new GeneratedEnemyUnit(9,-3,58,47,4), new GeneratedEnemyUnit(-17,5,16,10,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "6dd12cb3479dbcabbb40df2512dfe1d31074ad1e538d54c776a7746998eaba17");
        }

        private static void Case_01571()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1571,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-3,24,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-11,26,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,13,97,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-16,78,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,6,81,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-14,78,8,3), new GeneratedEnemyUnit(5,10,14,21,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "1e0853ed1d3eb644138b786d65f8a895d123ffff126278f76237439c50a30536");
        }

        private static void Case_01572()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1572,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-2,29,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,4,56,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,8,32,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,14,38,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,3,83,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,18,97,28,2), new GeneratedEnemyUnit(-13,-9,90,13,3), new GeneratedEnemyUnit(-10,15,36,18,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "000bda589fd1967547a61730a0bfcf5aebaa27d8a7e82e31d0bb76b823615f3d");
        }

        private static void Case_01573()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1573,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-12,90,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,16,9,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,0,6,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-16,8,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,10,23,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,12,16,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-7,14,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-1,24,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-12,40,45,4), new GeneratedEnemyUnit(19,-7,24,34,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "5826cef58fe830d772844256c677033c79c254257c0745d7d6183983878db21c");
        }

        private static void Case_01574()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1574,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,15,37,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-18,76,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,12,53,29,2), new GeneratedEnemyUnit(20,-18,69,30,3), new GeneratedEnemyUnit(19,-3,22,48,1), new GeneratedEnemyUnit(-16,-12,70,46,4), new GeneratedEnemyUnit(-8,-10,91,46,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "2463f44fc2b8db05c1347fd9f2ed738347b299915b0b2bc0410ec56a7fdcb598");
        }

        private static void Case_01575()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1575,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,7,48,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,13,78,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-7,69,48,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "dab9c9c9912d95a396fda51f76480db7f18ec14bf56dedcef3b6509f617f29e6");
        }

        private static void Case_01576()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1576,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,4,8,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-2,7,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,14,32,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-15,100,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,5,57,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,15,17,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-6,38,3,4), new GeneratedEnemyUnit(10,6,99,48,2), new GeneratedEnemyUnit(15,9,62,12,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "cba8685060a8d15ef9233645105435ee1451e0ab21d6f7cae84d163176bd02c2");
        }

        private static void Case_01577()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1577,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-19,93,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-14,51,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-3,87,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-4,76,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,13,46,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-13,74,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,13,94,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,17,25,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,2,96,33,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "b37b567c61783c0df9eb2130891e8679e3013aaa6f2e2a4101b6465dc11633ec");
        }

        private static void Case_01578()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1578,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-4,68,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,14,89,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,13,58,18,3), new GeneratedEnemyUnit(-16,-17,68,46,4), new GeneratedEnemyUnit(20,19,75,33,3), new GeneratedEnemyUnit(-8,0,19,36,4), new GeneratedEnemyUnit(-9,-20,99,30,3), new GeneratedEnemyUnit(-18,6,67,13,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "fed25e95b6e11d78f77a1bc5e1dca6764cc5927e44322cbc7484a05ac0bfae26");
        }

        private static void Case_01579()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1579,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-1,88,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,20,85,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,14,91,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,10,37,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-12,38,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,15,50,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,6,18,4,2), new GeneratedEnemyUnit(3,3,89,46,1), new GeneratedEnemyUnit(17,0,62,22,1), new GeneratedEnemyUnit(8,6,80,41,2), new GeneratedEnemyUnit(-17,12,57,30,4), new GeneratedEnemyUnit(16,-13,10,4,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "34f25ce8705d1f41ea3a1637ca165c5c1f7bff34f8d8dcddbdaadd47f293f886");
        }

        private static void Case_01580()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1580,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,7,100,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-4,75,42,3), new GeneratedEnemyUnit(3,4,33,17,3), new GeneratedEnemyUnit(20,-15,28,28,4), new GeneratedEnemyUnit(16,5,25,25,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "4a1e3627bfaca1493a98660600a3c3cfd884e9d36ee828b18bed73742b718ff8");
        }

        private static void Case_01581()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1581,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-17,46,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,14,22,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,12,27,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-2,97,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,9,77,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,19,46,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,0,79,43,3), new GeneratedEnemyUnit(20,-12,82,41,2), new GeneratedEnemyUnit(-6,-1,89,29,2), new GeneratedEnemyUnit(-17,-17,33,8,2), new GeneratedEnemyUnit(11,19,66,1,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "84e12a7e3f95589d7cf7e2d773f94b6e00e49f5da93547a9db038683f403b0b6");
        }

        private static void Case_01582()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1582,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,17,79,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,4,34,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,16,52,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-2,34,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,14,11,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,0,91,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,2,75,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-3,7,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-15,76,6,3), new GeneratedEnemyUnit(1,13,70,27,2), new GeneratedEnemyUnit(-11,4,77,28,4), new GeneratedEnemyUnit(11,6,70,33,4), new GeneratedEnemyUnit(-15,-9,54,45,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "76090fb0ce1dd8ac5d60ecdcd8ab85ee6b3f697ecd5905fd2b38ca154f45b262");
        }

        private static void Case_01583()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1583,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-5,40,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,7,41,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-1,48,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,10,69,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,17,52,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-10,64,2,2), new GeneratedEnemyUnit(17,-20,80,29,1), new GeneratedEnemyUnit(3,11,78,38,3), new GeneratedEnemyUnit(3,0,74,40,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "d37e1139493849d4d2eec51ad4d9a793e3b0c3dc4a29ab16258b25b1cde5e51f");
        }

        private static void Case_01584()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1584,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-14,49,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,11,54,18,3), new GeneratedEnemyUnit(-8,11,27,33,4), new GeneratedEnemyUnit(-9,12,63,26,2), new GeneratedEnemyUnit(4,11,85,12,1), new GeneratedEnemyUnit(9,17,47,45,2), new GeneratedEnemyUnit(3,12,19,42,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "8f4c762e6467b5b299978616a2e5a0735c51b43e629e915f4e81195082df64e9");
        }

        private static void Case_01585()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1585,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,10,86,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,15,41,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,6,97,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-11,89,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,2,28,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,0,85,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-8,36,7,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ab48aa9ac56f32c3a2729b6aec0b58f7ccdb6eb05bbd3c74287cc9cb3e78b55d");
        }

        private static void Case_01586()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1586,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,8,23,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-12,43,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-9,68,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,7,86,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,6,94,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-16,23,41,2), new GeneratedEnemyUnit(5,-18,41,26,2), new GeneratedEnemyUnit(-9,-10,41,35,1), new GeneratedEnemyUnit(3,-3,26,13,3), new GeneratedEnemyUnit(6,9,98,10,3), new GeneratedEnemyUnit(10,11,15,18,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "c8ce5841555f50cd577093ee716d43f257831be2437734ad70a34301341475f3");
        }

        private static void Case_01587()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1587,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-6,66,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,0,75,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,3,92,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-12,28,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-14,62,27,3), new GeneratedEnemyUnit(13,2,92,49,1), new GeneratedEnemyUnit(3,-4,86,15,3), new GeneratedEnemyUnit(-12,14,81,46,4), new GeneratedEnemyUnit(-16,9,11,28,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "d102e1c00f6674aab736851f1690f4bfd91b8745a56898b7d752a6b0cb2fbd0e");
        }

        private static void Case_01588()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1588,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-20,22,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-17,72,16,4), new GeneratedEnemyUnit(-10,-13,74,42,2), new GeneratedEnemyUnit(6,-10,60,27,1), new GeneratedEnemyUnit(16,1,12,16,4), new GeneratedEnemyUnit(-6,-17,81,5,3), new GeneratedEnemyUnit(-15,-11,21,26,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "788bf104351ce8bce80a5b0fa78ac913ff6d419add49c5cead0e99febcb11182");
        }

        private static void Case_01589()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1589,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,8,24,4,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "085ec6f30eaa954ae76e6577cca76ff86cec7ec1982434c4be8fba4485586763");
        }

        private static void Case_01590()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1590,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-7,22,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-10,57,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,10,92,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,18,35,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,4,67,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-4,24,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-12,94,42,4), new GeneratedEnemyUnit(4,16,88,20,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "3f10394f241b4bcd492827879ab02372941620fbd416d8cb69722ffc7de3241f");
        }

        private static void Case_01591()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1591,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-18,41,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-9,94,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-3,5,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,12,96,19,2), new GeneratedEnemyUnit(6,13,100,2,1), new GeneratedEnemyUnit(-18,-3,10,29,1), new GeneratedEnemyUnit(10,5,99,39,3), new GeneratedEnemyUnit(-4,-12,20,46,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "c583492b20cf3d8bb877469b2220a499b6f920fdd4976f9e70453e7ce34bf7b5");
        }

        private static void Case_01592()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1592,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-6,56,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,19,7,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-15,77,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,5,21,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,2,27,30,4), new GeneratedEnemyUnit(-7,-13,17,42,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "667c4b9f1c8a86dfc2dd6d45ca76a706f2ae0dc5917e9a3a28e1a2b593ef774a");
        }

        private static void Case_01593()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1593,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-4,38,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-15,64,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,12,74,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-7,26,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,4,88,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-18,90,42,2), new GeneratedEnemyUnit(-3,9,17,18,1), new GeneratedEnemyUnit(-7,14,23,49,4), new GeneratedEnemyUnit(19,2,99,6,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "989f8b6a9ee178ff9dcd7c6ddc36ceb95002e478f9bb6dc5c72216c430899b57");
        }

        private static void Case_01594()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1594,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-20,80,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-5,62,28,3), new GeneratedEnemyUnit(16,-5,63,15,4), new GeneratedEnemyUnit(-12,-10,5,30,3), new GeneratedEnemyUnit(5,19,71,34,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "eb95b8351f525104ebb42a51746855db8a366d6bff177f5f71b42dd04dbbf6b7");
        }

        private static void Case_01595()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1595,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,13,28,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,1,79,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,13,62,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-2,60,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-10,25,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-20,82,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-10,68,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,19,68,19,4), new GeneratedEnemyUnit(-13,-17,77,28,2), new GeneratedEnemyUnit(11,-19,60,42,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "69a60a6846fab2442deeacd32100db4b66dbcab48ed917d0abb80f4fbf145054");
        }

        private static void Case_01596()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1596,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,15,37,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,17,7,1,4), new GeneratedEnemyUnit(-11,0,83,29,4), new GeneratedEnemyUnit(-8,-7,97,29,2), new GeneratedEnemyUnit(-18,-10,67,14,3), new GeneratedEnemyUnit(18,-16,15,18,4), new GeneratedEnemyUnit(11,17,64,8,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "15a8af043987f1059d012777e198527f9c7b244e3cb5ad4f0014f9e2614df128");
        }

        private static void Case_01597()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1597,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,19,97,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,11,74,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,8,71,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-7,94,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-8,72,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-10,73,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-14,54,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-10,16,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,10,71,38,3), new GeneratedEnemyUnit(13,20,37,22,1), new GeneratedEnemyUnit(-6,5,55,20,1), new GeneratedEnemyUnit(-13,12,90,6,4), new GeneratedEnemyUnit(-15,15,46,27,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "5a1c0ded04d41859b774705299c09657b57c322f380f2492c2a0b850955dff14");
        }

        private static void Case_01598()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1598,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-17,5,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,2,73,45,3), new GeneratedEnemyUnit(4,-13,85,22,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "03867b7fc3298aa5b30cf1b817a26eb2e0c6b65dc2389b99b58d6c6d671b3dd2");
        }

        private static void Case_01599()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1599,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,9,82,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,0,92,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-20,19,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,3,22,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,13,36,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,0,78,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,8,84,20,1), new GeneratedEnemyUnit(2,-18,86,14,2), new GeneratedEnemyUnit(-9,-11,77,49,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "6a11cdfacff649d104ed687222c505aada98fe007cf3e638fcd90347deb6b6e0");
        }

        private static void Case_01600()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1600,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-7,22,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-11,16,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,12,25,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,16,12,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,5,99,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-20,83,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-14,93,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,5,27,43,1), new GeneratedEnemyUnit(12,8,91,26,4), new GeneratedEnemyUnit(3,18,95,32,1), new GeneratedEnemyUnit(2,-20,97,15,4), new GeneratedEnemyUnit(-16,-18,78,5,2), new GeneratedEnemyUnit(16,8,21,19,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "78ef0ff21151cb9f7140c7dedaa9b776e0d915a74abe09fab816c109caba549f");
        }

        private static void Case_01601()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1601,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,2,19,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-10,17,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,4,47,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,9,45,28,3), new GeneratedEnemyUnit(2,-5,29,6,3), new GeneratedEnemyUnit(8,11,44,42,4), new GeneratedEnemyUnit(3,-14,65,29,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "c8552f6925be4c40e73f27382391f2ace80871613221f4c6e01818ec2c2fc401");
        }

        private static void Case_01602()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1602,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-13,95,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-9,38,3,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "cdc05122e14c7071099133872ddda2c13ac7763afc00e09adb84727421e00967");
        }

        private static void Case_01603()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1603,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-5,74,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,19,19,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,0,44,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,1,36,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,20,94,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-15,50,38,4), new GeneratedEnemyUnit(-14,10,95,21,1), new GeneratedEnemyUnit(-3,-17,74,3,2), new GeneratedEnemyUnit(-9,-6,47,2,4), new GeneratedEnemyUnit(0,14,92,27,4), new GeneratedEnemyUnit(5,-19,79,20,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "356850af1374c1abf6c238d467ae95b365ff146943887ecc261a1be097317c68");
        }

        private static void Case_01604()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1604,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-15,33,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-5,41,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,3,73,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,13,75,20,4), new GeneratedEnemyUnit(10,7,9,47,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "1913a600803173295501293bf71751e1bab1c4a82c2ce7fdf1e3baaf9eed4b33");
        }

        private static void Case_01605()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1605,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-12,55,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-17,23,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-20,79,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-13,18,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,15,96,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,7,5,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-1,67,50,3), new GeneratedEnemyUnit(-6,8,39,41,4), new GeneratedEnemyUnit(14,-11,100,13,3), new GeneratedEnemyUnit(-13,15,58,40,2), new GeneratedEnemyUnit(-16,7,87,37,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7b2a9744e77cdf6ec15445bfd20b708d82457423adb47c4e9eedf5054dd03dd8");
        }

        private static void Case_01606()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1606,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-6,69,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-19,77,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-10,91,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,3,21,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,10,75,11,2), new GeneratedEnemyUnit(-8,16,92,35,1), new GeneratedEnemyUnit(16,12,15,3,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "de4db0fad3a595686a72e0fe19428d1d305354e74f8e74d1e7257a7e5478cf48");
        }

        private static void Case_01607()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1607,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-20,52,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-16,38,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,0,8,18,3), new GeneratedEnemyUnit(20,0,42,12,1), new GeneratedEnemyUnit(17,-2,33,17,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "e3be5d0c48b9706d8d3cd4b0a5040e69aa5d17b54d63ae9ea504babea146ea92");
        }

        private static void Case_01608()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1608,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,12,87,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-6,35,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,2,86,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-18,18,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-16,95,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,6,82,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-10,7,10,4), new GeneratedEnemyUnit(2,2,54,19,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "8abdd01a0bede4dcb004120fc1b3e9ce3d106d4ab830e19b2eb4393fe8d492f5");
        }

        private static void Case_01609()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1609,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-6,29,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-7,71,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-3,63,8,2), new GeneratedEnemyUnit(-3,-8,81,39,1), new GeneratedEnemyUnit(15,-17,6,44,1), new GeneratedEnemyUnit(4,11,60,26,4), new GeneratedEnemyUnit(-7,-18,8,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "ead5d35b059a16a4cdf7d6d2ca0fa65b023cbde6e5fae6169d45c142a6699ede");
        }

        private static void Case_01610()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1610,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-7,99,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,4,30,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-18,33,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-13,5,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,15,49,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-10,91,31,2), new GeneratedEnemyUnit(14,-6,19,14,3), new GeneratedEnemyUnit(1,-7,40,36,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "34f8693ca55436cbc2a4f2a132fb5f52d34ae95a49cf874f6fc630bbec936f8a");
        }

        private static void Case_01611()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1611,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-14,32,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,12,65,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,9,49,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-17,13,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,8,87,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-20,71,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,12,54,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-8,8,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-9,82,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2143b7cce22bcc674a64f7de1218ddfca8ac8750fd45dfafc33844832be2f8e1");
        }

        private static void Case_01612()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1612,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-3,9,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-7,18,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,3,55,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-15,11,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-7,41,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,15,10,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-19,60,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-16,10,2,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "67a0f3be4a90d27faf1249d4597dc0e465efa7b21a69b59d2c20aac8909f09e0");
        }

        private static void Case_01613()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1613,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-11,15,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,12,11,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-6,98,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-13,72,2,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "7c283f47cd3d93c0fc1288b6a31093edfd2afc7908b633b04768b7b3650835c3");
        }

        private static void Case_01614()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1614,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,6,40,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-11,69,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-14,16,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-1,45,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,4,17,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,2,44,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,4,25,2,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "2efc52e5d42934fbd557555663d24f04bd90fe7e2da2095d76667cc17b4ac19e");
        }

        private static void Case_01615()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1615,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-19,53,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-3,26,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,16,89,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,7,24,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,3,85,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-18,54,4,4), new GeneratedEnemyUnit(-10,-10,71,45,2), new GeneratedEnemyUnit(14,7,18,28,2), new GeneratedEnemyUnit(-9,10,68,5,4), new GeneratedEnemyUnit(1,18,18,27,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "a51eec054c6675ba100b3332eaae1ffa9d61f5f77d03d24f274777c48acbede3");
        }

        private static void Case_01616()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1616,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,13,71,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,2,18,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,14,91,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-4,77,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-9,48,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-15,19,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,12,6,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,7,99,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-11,58,6,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "5c7ab3380abcdd5d33d0133f770fee8559b31b52bafcc3c461cd360ed3593732");
        }

        private static void Case_01617()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1617,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-2,97,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,0,62,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-3,97,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-3,64,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-19,30,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,7,42,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,1,91,49,2), new GeneratedEnemyUnit(0,16,52,8,4), new GeneratedEnemyUnit(-1,-20,24,42,1), new GeneratedEnemyUnit(-4,-1,71,32,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d757cdfdf244afe7848ee486b3f61730e2d1d6b27105b1b3f0fa5803849abe31");
        }

        private static void Case_01618()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1618,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,2,5,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,17,10,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-9,86,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-10,91,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-4,22,12,3), new GeneratedEnemyUnit(4,-9,88,3,3), new GeneratedEnemyUnit(-9,-17,92,20,3), new GeneratedEnemyUnit(-10,-15,71,47,3), new GeneratedEnemyUnit(12,11,93,36,4), new GeneratedEnemyUnit(8,6,75,17,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "69b491656a113a3a11f6c4aa49cb24ba9ec7ed7a204a3aa0ae23acc34337b774");
        }

        private static void Case_01619()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1619,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-8,13,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-11,16,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,18,18,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,7,32,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-6,8,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,17,82,42,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "13fcdab3206ea4973d2a117aba792cefd8eabc551d479baa1194382da641711f");
        }

    }
}
