using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard025
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_04500();
            Case_04501();
            Case_04502();
            Case_04503();
            Case_04504();
            Case_04505();
            Case_04506();
            Case_04507();
            Case_04508();
            Case_04509();
            Case_04510();
            Case_04511();
            Case_04512();
            Case_04513();
            Case_04514();
            Case_04515();
            Case_04516();
            Case_04517();
            Case_04518();
            Case_04519();
            Case_04520();
            Case_04521();
            Case_04522();
            Case_04523();
            Case_04524();
            Case_04525();
            Case_04526();
            Case_04527();
            Case_04528();
            Case_04529();
            Case_04530();
            Case_04531();
            Case_04532();
            Case_04533();
            Case_04534();
            Case_04535();
            Case_04536();
            Case_04537();
            Case_04538();
            Case_04539();
            Case_04540();
            Case_04541();
            Case_04542();
            Case_04543();
            Case_04544();
            Case_04545();
            Case_04546();
            Case_04547();
            Case_04548();
            Case_04549();
            Case_04550();
            Case_04551();
            Case_04552();
            Case_04553();
            Case_04554();
            Case_04555();
            Case_04556();
            Case_04557();
            Case_04558();
            Case_04559();
            Case_04560();
            Case_04561();
            Case_04562();
            Case_04563();
            Case_04564();
            Case_04565();
            Case_04566();
            Case_04567();
            Case_04568();
            Case_04569();
            Case_04570();
            Case_04571();
            Case_04572();
            Case_04573();
            Case_04574();
            Case_04575();
            Case_04576();
            Case_04577();
            Case_04578();
            Case_04579();
            Case_04580();
            Case_04581();
            Case_04582();
            Case_04583();
            Case_04584();
            Case_04585();
            Case_04586();
            Case_04587();
            Case_04588();
            Case_04589();
            Case_04590();
            Case_04591();
            Case_04592();
            Case_04593();
            Case_04594();
            Case_04595();
            Case_04596();
            Case_04597();
            Case_04598();
            Case_04599();
            Case_04600();
            Case_04601();
            Case_04602();
            Case_04603();
            Case_04604();
            Case_04605();
            Case_04606();
            Case_04607();
            Case_04608();
            Case_04609();
            Case_04610();
            Case_04611();
            Case_04612();
            Case_04613();
            Case_04614();
            Case_04615();
            Case_04616();
            Case_04617();
            Case_04618();
            Case_04619();
            Case_04620();
            Case_04621();
            Case_04622();
            Case_04623();
            Case_04624();
            Case_04625();
            Case_04626();
            Case_04627();
            Case_04628();
            Case_04629();
            Case_04630();
            Case_04631();
            Case_04632();
            Case_04633();
            Case_04634();
            Case_04635();
            Case_04636();
            Case_04637();
            Case_04638();
            Case_04639();
            Case_04640();
            Case_04641();
            Case_04642();
            Case_04643();
            Case_04644();
            Case_04645();
            Case_04646();
            Case_04647();
            Case_04648();
            Case_04649();
            Case_04650();
            Case_04651();
            Case_04652();
            Case_04653();
            Case_04654();
            Case_04655();
            Case_04656();
            Case_04657();
            Case_04658();
            Case_04659();
            Case_04660();
            Case_04661();
            Case_04662();
            Case_04663();
            Case_04664();
            Case_04665();
            Case_04666();
            Case_04667();
            Case_04668();
            Case_04669();
            Case_04670();
            Case_04671();
            Case_04672();
            Case_04673();
            Case_04674();
            Case_04675();
            Case_04676();
            Case_04677();
            Case_04678();
            Case_04679();
        }

        private static void Case_04500()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4500,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-2,21,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-3,55,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,10,41,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,1,35,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-8,14,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-3,56,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,1,40,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,17,38,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-14,40,42,4), new GeneratedEnemyUnit(-11,-4,51,5,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "cd41c40e968ae25baa0d7bd47f0a76ba696872b0b7f1a219641f9c38331a236f");
        }

        private static void Case_04501()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4501,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,20,48,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,9,49,46,1), new GeneratedEnemyUnit(-20,7,42,6,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "180b988fc4948cb3b689e936f17e08954feb01433ab10224db52191e7bd6d221");
        }

        private static void Case_04502()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4502,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,13,14,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,15,11,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,0,83,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,8,90,19,1), new GeneratedEnemyUnit(-6,18,72,21,3), new GeneratedEnemyUnit(9,1,62,44,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "f1eccbcc30dccd18c7186679940a83c5aafdf3b6114ae8a175ac8fad992ecd81");
        }

        private static void Case_04503()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4503,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,17,84,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,2,14,43,3), new GeneratedEnemyUnit(-13,11,90,37,3), new GeneratedEnemyUnit(13,-12,97,44,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8f9ac29621df0cf2394fb5fc0f1ce446be3cad65f424c7fe6e42d56ff0576e62");
        }

        private static void Case_04504()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4504,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,10,36,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,2,76,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,13,20,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,6,81,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,13,89,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-2,61,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-15,28,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,19,52,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,2,58,33,1), new GeneratedEnemyUnit(14,-19,45,3,3), new GeneratedEnemyUnit(-8,-13,25,16,3), new GeneratedEnemyUnit(15,-18,34,15,1), new GeneratedEnemyUnit(-10,-13,78,28,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "4ccda5b0e090514f5b73a6e5ad96641895350cbe1674f6aad9b28de57a959a63");
        }

        private static void Case_04505()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4505,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,17,74,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,5,90,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-15,63,20,3), new GeneratedEnemyUnit(15,-11,57,20,2), new GeneratedEnemyUnit(-2,-11,71,4,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e5305b653c5d90e4d44ea592220c3961298e052c2c5a34506b50476e75d91b71");
        }

        private static void Case_04506()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4506,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-14,28,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-11,43,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,14,91,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,13,35,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-7,84,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,2,84,21,4), new GeneratedEnemyUnit(17,14,92,2,3), new GeneratedEnemyUnit(2,3,68,13,3), new GeneratedEnemyUnit(-17,5,59,43,4), new GeneratedEnemyUnit(4,-14,82,16,4), new GeneratedEnemyUnit(-16,15,92,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "4a22345ee2029fc6f2f349d9d36296e33d726f999fcba2d4fecb12f52fc44d19");
        }

        private static void Case_04507()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4507,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,8,69,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,10,19,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-13,80,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,8,95,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,9,62,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,11,76,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-10,97,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-6,13,4,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "77ca99f7afb1e9ab1d414b1bf59953739dbb886e7ca74fd00097d06cc1348127");
        }

        private static void Case_04508()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4508,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,0,68,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,3,36,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,3,6,15,4), new GeneratedEnemyUnit(-3,10,93,23,3), new GeneratedEnemyUnit(12,-12,68,42,3), new GeneratedEnemyUnit(7,8,41,41,1), new GeneratedEnemyUnit(0,4,31,42,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "757fba0c9a01b439417540ba57e1e4dea1aa1ff916228bf6f82de01b6ae5ee93");
        }

        private static void Case_04509()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4509,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,11,64,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,14,61,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,16,19,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,19,14,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,2,18,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,8,49,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-11,18,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,10,41,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,15,100,5,4), new GeneratedEnemyUnit(-19,15,65,31,2), new GeneratedEnemyUnit(7,12,36,3,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "9003f889aca0083db119fca2644e4723d1de3710eed54788b727fc4e7ef2bc6f");
        }

        private static void Case_04510()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4510,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,2,49,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-4,52,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,5,16,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-10,5,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,11,87,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-12,38,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-1,6,46,3), new GeneratedEnemyUnit(15,9,89,5,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "6d6708ed0216f11065f31d95f22def97fc9c13c2188c8d16d8b4dc5c561f5550");
        }

        private static void Case_04511()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4511,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-3,36,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-8,81,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,18,88,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,11,47,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,9,93,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,3,88,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,16,91,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-20,71,44,1), new GeneratedEnemyUnit(0,-16,74,33,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "b7e2a561510709712a56e8ee5d55ef8bec5ec2136d0d290a1dafd885ae50e18e");
        }

        private static void Case_04512()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4512,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,17,74,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,13,35,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-12,79,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-11,21,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,4,92,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-11,35,38,1), new GeneratedEnemyUnit(18,-5,36,43,4), new GeneratedEnemyUnit(-18,3,59,47,3), new GeneratedEnemyUnit(-19,-19,52,9,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "88945b8e25bdb633e70169184e0d31dffc50a782d4180f2a8fd1a35c1b7ab383");
        }

        private static void Case_04513()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4513,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,14,42,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,16,36,38,3), new GeneratedEnemyUnit(-12,-5,53,24,3), new GeneratedEnemyUnit(-3,-10,57,34,3), new GeneratedEnemyUnit(8,-16,42,35,1), new GeneratedEnemyUnit(5,-11,45,13,1), new GeneratedEnemyUnit(-18,8,94,19,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "5b67d45f6685c5fd6388099f265ce7b0a78431a923519734c392dc03ff2f5b0c");
        }

        private static void Case_04514()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4514,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-11,24,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,12,50,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,8,24,24,1), new GeneratedEnemyUnit(8,7,85,3,4), new GeneratedEnemyUnit(-15,17,91,23,4), new GeneratedEnemyUnit(-17,-20,89,16,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "aa213e45f67aa3880fed65023654cec35172d3c5c4507a41406d903e88886ed3");
        }

        private static void Case_04515()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4515,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-16,82,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-15,65,1,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "6cf6ba9fff4f3333978901b96c5d6dbea8d6a756d4544905a0b7cb2f53030f39");
        }

        private static void Case_04516()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4516,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-14,23,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-12,11,17,3), new GeneratedEnemyUnit(-9,20,37,27,1), new GeneratedEnemyUnit(-13,17,84,42,3), new GeneratedEnemyUnit(-5,3,82,18,4), new GeneratedEnemyUnit(3,-3,74,20,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "9e990b2295b4c593f06d89ad3838d3cbc9b9c68e945f5c1caf067afa8dc97444");
        }

        private static void Case_04517()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4517,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-20,30,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,0,85,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,1,75,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-8,8,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,4,46,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,8,39,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-19,88,4,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "8d83c416ec081f3e8a7c60cff0bf6f686c184bb0ff90b5cdff9f8190f8bfca92");
        }

        private static void Case_04518()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4518,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,19,93,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,19,43,26,4), new GeneratedEnemyUnit(-15,4,62,31,4), new GeneratedEnemyUnit(-13,12,69,8,1), new GeneratedEnemyUnit(-18,6,40,28,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "1f79825a5655fff20397e3862609db4d8a645bb741d47c94c7c9ee7f61a00dc8");
        }

        private static void Case_04519()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4519,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,15,16,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,2,6,14,4), new GeneratedEnemyUnit(5,-3,100,29,3), new GeneratedEnemyUnit(0,-14,54,40,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "2c500bfb3ed12ccb2527a61e5dbed9444b6057820657e9de69c3789e831e9f74");
        }

        private static void Case_04520()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4520,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,4,68,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-15,89,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-15,74,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-8,6,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-9,7,2,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "b6d62b19de6d5b170f288c1629bc3733b7e5b2875ab889c3ecb84191aa441425");
        }

        private static void Case_04521()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4521,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,4,29,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,5,11,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,0,22,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,13,41,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-1,28,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-5,66,7,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "6fa9be0b713fe8524f552799ec2ba2e6b5b7dae67512a41516631a82668e282a");
        }

        private static void Case_04522()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4522,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,11,61,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-3,84,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,2,87,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,2,63,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-2,70,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,1,32,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,6,24,11,1), new GeneratedEnemyUnit(-8,8,54,48,4), new GeneratedEnemyUnit(-5,0,89,22,4), new GeneratedEnemyUnit(9,-15,81,32,3), new GeneratedEnemyUnit(10,4,75,28,4), new GeneratedEnemyUnit(10,8,92,39,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "790b969da4ea2a6675c67a89589a88b5fb62c306099515957676a0c52592a386");
        }

        private static void Case_04523()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4523,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,0,28,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,9,49,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-5,78,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,13,8,24,2), new GeneratedEnemyUnit(1,19,44,16,3), new GeneratedEnemyUnit(-7,-20,80,45,1), new GeneratedEnemyUnit(-3,-1,47,9,1), new GeneratedEnemyUnit(-13,-1,70,39,2), new GeneratedEnemyUnit(-9,-15,87,17,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "377fe3a743c1a913ff3223e92414a689d17de4e50cfddedc6b3a32bf0af6a756");
        }

        private static void Case_04524()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4524,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-7,69,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,11,73,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,20,22,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-6,84,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-19,31,13,1), new GeneratedEnemyUnit(-11,0,78,12,2), new GeneratedEnemyUnit(-1,13,83,38,2), new GeneratedEnemyUnit(11,-12,9,4,2), new GeneratedEnemyUnit(19,9,97,32,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "ea6c58d578a78617f2c7973bd37587cd16f5b47f25e875d1e009e62fba004dde");
        }

        private static void Case_04525()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4525,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-4,19,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-12,40,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,1,84,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,11,27,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,19,17,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,12,85,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-11,29,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-3,7,20,4), new GeneratedEnemyUnit(-19,19,39,30,4), new GeneratedEnemyUnit(-10,20,28,23,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "0eaf5381fb27d3fe163a81036347aae5de41361413d62e02db6d067d4f1a3d2a");
        }

        private static void Case_04526()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4526,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,8,59,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,11,61,18,4), new GeneratedEnemyUnit(13,-6,57,13,1), new GeneratedEnemyUnit(-7,-19,31,4,2), new GeneratedEnemyUnit(-5,-13,82,29,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "6476497ed4d5d893d18f619dd120df94483aa2e109131c401c81b7523ebb690d");
        }

        private static void Case_04527()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4527,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,5,47,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-1,40,37,1), new GeneratedEnemyUnit(-7,2,79,30,2), new GeneratedEnemyUnit(-1,-20,55,13,1), new GeneratedEnemyUnit(11,10,52,1,4), new GeneratedEnemyUnit(-11,11,30,19,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "5d9789782fd679d6164b6f65e949b6af1b4fe2ddf7e2b6ceb268ab273848ba4c");
        }

        private static void Case_04528()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4528,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-16,25,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-11,93,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-1,89,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,4,70,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,11,46,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,2,45,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,7,82,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-5,19,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-17,32,44,1), new GeneratedEnemyUnit(-2,-9,32,29,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f06c6b8747178d59da0b475b6bdb75b7562aff908755728cc1829976854ac8b0");
        }

        private static void Case_04529()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4529,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,19,92,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,13,67,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-12,53,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,3,38,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,16,66,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-9,64,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-14,78,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,20,82,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-6,45,24,2), new GeneratedEnemyUnit(3,-3,40,16,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "484b823fffb6414ba5cc8e881fd3b71682bc36389d748556af942eae05d71abc");
        }

        private static void Case_04530()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4530,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,14,13,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-12,76,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,7,49,16,3), new GeneratedEnemyUnit(-3,-4,31,50,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "94634934adf860e6a4b3b8b589270dd518d4014103552d9c5adbb7b9016c3b08");
        }

        private static void Case_04531()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4531,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,15,100,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-1,30,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,7,89,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,4,7,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-4,87,2,4), new GeneratedEnemyUnit(1,-16,40,43,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "f9e971222a0c8cdb69909fe4c4229447f5f50721e7ea755c86d8dd2a8a31eab4");
        }

        private static void Case_04532()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4532,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-15,84,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-12,40,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,5,79,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,11,94,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,1,30,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-10,19,14,4), new GeneratedEnemyUnit(-13,1,94,15,1), new GeneratedEnemyUnit(-17,-12,28,43,3), new GeneratedEnemyUnit(19,1,92,15,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "ef5a64b0837baef0cd084ec5cfbd74532bde46ac22bb3ce83cf5e92119a7ea55");
        }

        private static void Case_04533()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4533,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-12,43,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,17,96,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,19,88,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,11,83,1,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "adf3308b80cafe6ce6fe1698580bd03ff3a9e041808db88c57ce27c9535e794b");
        }

        private static void Case_04534()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4534,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,8,76,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-5,95,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,12,42,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-20,97,40,3), new GeneratedEnemyUnit(-19,13,48,49,1), new GeneratedEnemyUnit(3,5,100,42,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "70f526940675f2bce202cd67e857a755206b771ce8b6994807de21ab75fa9a6c");
        }

        private static void Case_04535()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4535,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,19,58,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-14,13,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-1,50,19,4), new GeneratedEnemyUnit(-7,18,79,38,2), new GeneratedEnemyUnit(16,-19,88,36,1), new GeneratedEnemyUnit(-9,-15,16,12,2), new GeneratedEnemyUnit(-3,15,30,11,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e51a1583ef4d774f8a3cc6b508d69ce65622203f7da744943a869acd96696b8b");
        }

        private static void Case_04536()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4536,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-8,74,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-14,34,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,19,71,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,0,72,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,15,46,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-9,9,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-9,43,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,17,77,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "405b6e3930e69aeb4a1d4a461e8c0f45c309cb9774de672e91232959a2043884");
        }

        private static void Case_04537()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4537,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-14,94,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-13,46,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,0,51,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-6,69,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-1,57,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,17,44,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-8,94,33,1), new GeneratedEnemyUnit(13,-1,78,21,1), new GeneratedEnemyUnit(12,5,27,48,4), new GeneratedEnemyUnit(-1,-7,96,42,4), new GeneratedEnemyUnit(-6,2,31,44,3), new GeneratedEnemyUnit(-11,8,41,19,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "2990ec97f89ae15d1d4198c82aa1f55e562ba77090b0d55819d85b8b4a12d8be");
        }

        private static void Case_04538()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4538,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-11,21,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,20,63,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-14,8,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,2,42,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,5,45,19,1), new GeneratedEnemyUnit(-12,20,96,36,4), new GeneratedEnemyUnit(-14,-17,8,44,3), new GeneratedEnemyUnit(-12,-20,56,12,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "3d87a54ccb814b68e63d44cfc435a76dde4a13ffc3347b24aafc9b6b0ab46479");
        }

        private static void Case_04539()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4539,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-19,15,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-16,89,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,10,86,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,3,6,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,8,6,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-14,71,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,1,28,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-3,85,29,4), new GeneratedEnemyUnit(2,-12,88,8,2), new GeneratedEnemyUnit(-2,-3,98,18,4), new GeneratedEnemyUnit(-18,18,22,37,4), new GeneratedEnemyUnit(-11,-18,18,20,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "89e2301920f95b5b5eac93f1022e86682ada4fcb9df237a7f5c1aecb9d972cef");
        }

        private static void Case_04540()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4540,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,5,70,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-5,6,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,1,50,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,18,5,2,2), new GeneratedEnemyUnit(11,10,22,41,1), new GeneratedEnemyUnit(20,8,14,26,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "8b45d33340171af21122f8a3f7ab1568f402cc701b090959ec70af09f5cc5cbc");
        }

        private static void Case_04541()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4541,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-10,56,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,1,52,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-19,94,20,1), new GeneratedEnemyUnit(16,1,11,16,3), new GeneratedEnemyUnit(8,-18,9,7,3), new GeneratedEnemyUnit(-5,10,95,36,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "69939b2e9f86c6f4b056d2d838dd405a48c623cb1d9f18cc3f6cfd8704667ea8");
        }

        private static void Case_04542()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4542,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-8,92,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,16,11,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-16,36,39,1), new GeneratedEnemyUnit(-20,-6,74,2,4), new GeneratedEnemyUnit(18,14,72,25,3), new GeneratedEnemyUnit(14,7,15,7,4), new GeneratedEnemyUnit(20,-10,90,26,4), new GeneratedEnemyUnit(-7,2,37,50,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "ee2dddd5b827869dc8687405c829d0492343c82f97862ebbfed6875b681c9f5f");
        }

        private static void Case_04543()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4543,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-2,27,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-6,43,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,12,88,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-6,31,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,3,26,46,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ea32fd3c48d256b12f6c8eadcd898d4f89c1e483e45333a9b7328ee3721ceaa9");
        }

        private static void Case_04544()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4544,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-16,43,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,12,19,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-11,86,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-14,27,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-15,26,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,0,84,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,7,30,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,16,8,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-7,37,26,1), new GeneratedEnemyUnit(-16,1,32,18,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "eaf8b8c05447206d55c95bf1f0911a3e6ddcc5521ecbf3d2846bb5c27cf24ede");
        }

        private static void Case_04545()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4545,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,2,78,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-1,54,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-12,47,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,10,63,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,19,98,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-6,49,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-5,18,5,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "99047766a8577f404121df5c85893ebfb798ff6bc42e2b5b9fb54f114d9ebb60");
        }

        private static void Case_04546()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4546,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-8,37,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-5,76,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,18,76,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,9,89,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-11,90,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-14,45,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-2,24,2,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "810c34fb1cc75ca1a82d54e511d846131e5acecf2bdaeb1c9c18a5800f50b4e7");
        }

        private static void Case_04547()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4547,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-5,53,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-20,67,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,7,26,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,15,92,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,19,91,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,13,84,50,4), new GeneratedEnemyUnit(-9,-15,78,39,2), new GeneratedEnemyUnit(-3,6,88,37,2), new GeneratedEnemyUnit(-17,6,93,4,4), new GeneratedEnemyUnit(13,7,74,29,3), new GeneratedEnemyUnit(7,6,10,25,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "f299f4a3e2eb382eaec8af29eeebb9fc24a7fa317f275258319233dbd0a5119c");
        }

        private static void Case_04548()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4548,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-6,42,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-2,27,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,17,97,40,2), new GeneratedEnemyUnit(-8,-16,55,43,3), new GeneratedEnemyUnit(-10,14,44,35,1), new GeneratedEnemyUnit(7,7,97,49,1), new GeneratedEnemyUnit(20,-16,59,46,3), new GeneratedEnemyUnit(14,16,16,9,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "f7aabe2f33826195d24e7c861d5b23b84ac6e0cfa3ae4439a4b5d75242633718");
        }

        private static void Case_04549()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4549,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,4,69,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-6,18,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,4,83,30,4), new GeneratedEnemyUnit(-11,-20,58,3,1), new GeneratedEnemyUnit(-11,-18,27,11,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "65da052725783db69be6567b07db487f2c970214d672a7363a56a85b40748782");
        }

        private static void Case_04550()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4550,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,17,35,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-20,46,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-8,92,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-2,40,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,14,36,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-16,26,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-18,64,17,3), new GeneratedEnemyUnit(-4,10,89,17,4), new GeneratedEnemyUnit(15,16,81,37,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "04abce2d4607ebcec6f3bda25623b1c67e08e109dd7c647fa6e57abaebdfb793");
        }

        private static void Case_04551()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4551,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-11,49,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,17,78,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,17,46,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-14,6,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,18,73,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,19,85,3,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "b13a2403ee7f52f689b8d7f2775328d4c391c63901f42451c5fd2ca799c9eaed");
        }

        private static void Case_04552()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4552,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,20,34,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-17,84,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-18,23,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-11,49,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,16,37,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-15,56,20,4), new GeneratedEnemyUnit(-11,7,47,7,1), new GeneratedEnemyUnit(-5,3,65,30,4), new GeneratedEnemyUnit(14,12,29,27,4), new GeneratedEnemyUnit(7,-19,20,13,4), new GeneratedEnemyUnit(-17,-20,11,43,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "a7c37696211b5eb867323ed8448bbae79b21a40de50612d6240125329bdd2f53");
        }

        private static void Case_04553()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4553,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,18,18,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,15,26,23,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "f37ae8c1f822377eff590ac806a236f791b0ce3b8de070549dc1e06537eb0783");
        }

        private static void Case_04554()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4554,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,17,68,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-3,51,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,8,35,28,1), new GeneratedEnemyUnit(-7,9,16,32,3), new GeneratedEnemyUnit(-5,-16,28,47,4), new GeneratedEnemyUnit(-2,-10,20,42,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "d106ba75c5c01678a008609d47e35bfb2ad84d639e7b5fd414a7879aed8167b0");
        }

        private static void Case_04555()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4555,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,5,81,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-15,62,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,3,38,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-10,24,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,7,63,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-10,8,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,6,79,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-12,56,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-6,78,30,4), new GeneratedEnemyUnit(9,-10,87,47,1), new GeneratedEnemyUnit(-8,5,13,33,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "7dccc8383168928bc4c89af42b7fc72c6037fe861a2fb16caf356ccf7e4323a6");
        }

        private static void Case_04556()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4556,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-1,34,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-1,42,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-18,39,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-7,58,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,14,63,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-1,13,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-17,59,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-8,82,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,9,78,30,4), new GeneratedEnemyUnit(-11,16,31,27,4), new GeneratedEnemyUnit(-1,3,22,39,2), new GeneratedEnemyUnit(8,-3,24,5,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "7d0c9902f758fd49ccaeab105fb93e70dfac6e2e422bd28d98dba7efae7ed1b0");
        }

        private static void Case_04557()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4557,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-18,90,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-6,63,4,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "a3ba15ec3e53aaf9186922cbc0db22e95da8fb8dc96a7b6985e0759d27e69fae");
        }

        private static void Case_04558()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4558,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,10,72,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-1,9,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-16,58,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,19,13,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-3,94,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-17,83,12,2), new GeneratedEnemyUnit(-9,18,92,14,4), new GeneratedEnemyUnit(0,-5,87,19,4), new GeneratedEnemyUnit(15,1,78,10,3), new GeneratedEnemyUnit(17,-2,21,43,3), new GeneratedEnemyUnit(-11,-18,40,15,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "358e19f6274940f56fb44da8738fe4c874bf6e7afbb72acda483bbfeb4a51727");
        }

        private static void Case_04559()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4559,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,8,70,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-16,55,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,7,54,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,9,87,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,10,87,23,4), new GeneratedEnemyUnit(5,2,48,26,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "4dce5bf9e6bae4415d88623c7a793977e888d4204a4fef33dc5e32da61a8e4c1");
        }

        private static void Case_04560()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4560,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-7,14,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-12,96,1,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "7efb9538844cffa815b438b008f7ba70c24a2177953b3baab31cd75be81b03b9");
        }

        private static void Case_04561()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4561,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-11,79,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,6,97,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-13,39,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,7,40,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,20,82,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-5,64,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,10,42,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-15,49,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-17,19,23,3), new GeneratedEnemyUnit(-14,12,96,36,2), new GeneratedEnemyUnit(10,17,23,3,3), new GeneratedEnemyUnit(10,-16,27,48,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "e4f411888e7488647ca5bb7bc666ede38455aa88e522a2cb4b20a5bfa55fe100");
        }

        private static void Case_04562()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4562,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-20,29,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,9,62,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-19,63,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,11,59,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,1,22,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-1,62,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,14,9,27,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "aeb149538c9454495c384d668c77d942e8c25a4413b27192e101dc5582fddb9c");
        }

        private static void Case_04563()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4563,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-16,61,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-5,79,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-3,100,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,4,17,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-20,10,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-16,9,6,2), new GeneratedEnemyUnit(-14,-12,55,48,4), new GeneratedEnemyUnit(0,-19,88,43,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "6b895220561cad457c3bb14b6bea340e542be64559969438dd7f89d47029ad8d");
        }

        private static void Case_04564()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4564,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-12,72,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,7,89,11,3), new GeneratedEnemyUnit(-12,-7,84,21,4), new GeneratedEnemyUnit(1,19,99,7,3), new GeneratedEnemyUnit(-10,-14,20,22,3), new GeneratedEnemyUnit(-3,13,54,13,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "4ee4ad83bee3fe902d93e2a63093e46dcaa0973dd0aeeff09bdf6138ddef2c4c");
        }

        private static void Case_04565()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4565,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-20,19,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-2,98,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,3,72,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-1,55,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,8,39,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,8,95,42,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "93f0f44d630c4c329f2cfde81d1b797eb642d7f772c9250c4bb3d840a845325b");
        }

        private static void Case_04566()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4566,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,5,60,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-20,84,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,12,73,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-9,20,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-9,53,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,15,52,47,3), new GeneratedEnemyUnit(15,6,59,47,4), new GeneratedEnemyUnit(-17,-5,92,8,4), new GeneratedEnemyUnit(7,-17,97,33,3), new GeneratedEnemyUnit(-15,16,48,9,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "8c2e6caf3f31a0c8ea9433d6dc8ea3cfad97b0dc0d0ea7624d0aabc1c5c08133");
        }

        private static void Case_04567()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4567,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-2,66,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,19,30,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,0,17,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-20,39,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-15,60,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,13,60,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-20,15,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-14,14,23,1), new GeneratedEnemyUnit(17,-5,89,45,4), new GeneratedEnemyUnit(16,3,34,41,1), new GeneratedEnemyUnit(3,4,17,45,2), new GeneratedEnemyUnit(0,-19,25,47,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "d6eca9b5d76745cd081868a069fe281fa1cdc1e2ac63d4391d410455213f3bf8");
        }

        private static void Case_04568()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4568,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-12,44,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,16,81,31,1), new GeneratedEnemyUnit(-15,-8,38,4,4), new GeneratedEnemyUnit(15,13,25,1,3), new GeneratedEnemyUnit(-11,19,22,45,2), new GeneratedEnemyUnit(13,-16,67,37,3), new GeneratedEnemyUnit(-13,12,82,20,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "8fdbc61631502343ee5249e5e265a28bef8970cd7b885f04ff8568e1e02fb6cd");
        }

        private static void Case_04569()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4569,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,7,37,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,6,47,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,13,66,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-7,32,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,10,54,5,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "bd55011923d5811501b6f46b9124bae3539a49bd04fc174a1642964533214354");
        }

        private static void Case_04570()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4570,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-4,94,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-20,26,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,10,59,48,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "533c2f147cb7d17c03818d90428297c20b3c27fa211cfe6366db2eb830161f95");
        }

        private static void Case_04571()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4571,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-4,81,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,2,32,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-13,9,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,19,37,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-15,92,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-3,36,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,0,86,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,17,10,43,3), new GeneratedEnemyUnit(13,-12,71,41,4), new GeneratedEnemyUnit(-16,-8,47,33,1), new GeneratedEnemyUnit(13,-10,62,49,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "39c66d73b5d20bee4c0f96e8578e3faf25833a54759f57e39dc9b384514bdcd5");
        }

        private static void Case_04572()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4572,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,1,40,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,15,39,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,20,57,6,3), new GeneratedEnemyUnit(3,12,50,24,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "ab1df7a9484338606bbf83f4fad5ace920c1e3042bbf45e7d56a5fd4e9ffc9c5");
        }

        private static void Case_04573()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4573,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,20,95,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,8,91,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-11,30,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,2,97,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,20,57,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,14,57,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-2,35,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,14,17,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-11,18,2,4), new GeneratedEnemyUnit(6,-4,29,22,3), new GeneratedEnemyUnit(-4,-11,76,23,2), new GeneratedEnemyUnit(-14,-16,100,26,3), new GeneratedEnemyUnit(-20,12,75,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "8774026e6ff4b7e00d316abc263d6c2d5c3f4b0c6b41f4460cace3bb872c0939");
        }

        private static void Case_04574()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4574,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-12,67,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,6,84,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,18,5,8,4), new GeneratedEnemyUnit(16,-11,52,20,4), new GeneratedEnemyUnit(-11,-6,21,17,3), new GeneratedEnemyUnit(-5,-6,39,23,4), new GeneratedEnemyUnit(-20,-11,54,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "8d1a9b267e53f2a3ae1322efa6bbc1eb90fbb74484a55a0f88235878a57f9393");
        }

        private static void Case_04575()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4575,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,4,19,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-3,74,42,2), new GeneratedEnemyUnit(5,-4,83,4,1), new GeneratedEnemyUnit(-2,-10,26,23,1), new GeneratedEnemyUnit(-10,-13,91,50,1), new GeneratedEnemyUnit(17,3,45,29,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "202dacdf5961b49f189cc6725cb7b809c0fb6b12515f17cf424bb7d1e3621c9d");
        }

        private static void Case_04576()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4576,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-6,84,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,0,81,34,2), new GeneratedEnemyUnit(-2,-3,21,20,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "7459f9ffd6f5ab91eed067d0afdc4a4272db63ecb53b086fdc78891a4e9e9006");
        }

        private static void Case_04577()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4577,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-1,50,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-18,81,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,9,53,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,13,77,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,9,36,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-17,46,10,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "49eb7e4b520b0cbc8f654760ef3c043062c8bd6796e882eedfc893804f9b5671");
        }

        private static void Case_04578()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4578,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,3,49,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-6,37,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,19,52,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,15,61,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,11,28,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-4,70,12,2), new GeneratedEnemyUnit(7,11,63,4,3), new GeneratedEnemyUnit(7,13,44,34,2), new GeneratedEnemyUnit(-7,-8,46,40,4), new GeneratedEnemyUnit(2,6,58,35,2), new GeneratedEnemyUnit(-20,-7,48,17,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "c4bbdfb2a902c1664b039e9ad493bfd300d9f2f5e1fc066ed29dc303f6903dfb");
        }

        private static void Case_04579()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4579,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,8,57,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,8,91,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-14,44,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,10,93,42,1), new GeneratedEnemyUnit(0,5,16,20,4), new GeneratedEnemyUnit(-7,9,6,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "9de34c04605ab00d71ba2a19159bd19715f7abd417ce2a4bd0cd75f878183426");
        }

        private static void Case_04580()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4580,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-5,68,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,14,18,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,20,50,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-6,44,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-14,62,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-20,91,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,19,8,13,3), new GeneratedEnemyUnit(-1,-7,65,17,1), new GeneratedEnemyUnit(-20,2,86,4,4), new GeneratedEnemyUnit(15,-5,25,24,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "a5efaf5a6e42ca02332fa1ba6e15ab6b84e75a9b12df8404c85aa7e126d3bd41");
        }

        private static void Case_04581()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4581,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-13,51,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,19,66,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,20,43,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,2,83,44,1), new GeneratedEnemyUnit(4,0,98,42,4), new GeneratedEnemyUnit(13,-18,74,21,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "1a3101c8a711aada01bd5bcca98d7ea0398c5e05e957e9d624c28fff86747bd3");
        }

        private static void Case_04582()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4582,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,11,26,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,7,23,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,20,30,1,4), new GeneratedEnemyUnit(11,6,16,31,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ad0c7363c5a7a6593300ebcae78daae3bc9aaf1fd2c0098a7ce1a6f0481bc87a");
        }

        private static void Case_04583()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4583,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,11,32,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,7,91,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-17,89,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-5,28,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-18,29,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,3,56,33,1), new GeneratedEnemyUnit(-3,9,83,41,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "9daf10e10307095702afbb851451260d1b12fb001910b3c509aaa606c92fea39");
        }

        private static void Case_04584()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4584,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,3,25,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-14,70,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-3,66,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-20,63,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-2,15,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-20,81,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,10,93,23,3), new GeneratedEnemyUnit(-12,-10,78,48,2), new GeneratedEnemyUnit(-11,20,25,3,4), new GeneratedEnemyUnit(4,-14,17,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "d53077e5c0a323eaf00d3c1ad383572052e9fe603d466ebd3e09aa3ae6cf9577");
        }

        private static void Case_04585()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4585,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-6,85,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-1,80,42,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "eacb3fa74e1fa3785ac9d233468aa0f071e4fe3408482b57cf64504e78d7125f");
        }

        private static void Case_04586()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4586,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-15,18,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-3,13,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,16,98,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,0,99,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-5,91,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,20,45,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,1,38,4,1), new GeneratedEnemyUnit(6,-6,31,24,1), new GeneratedEnemyUnit(-14,9,76,39,1), new GeneratedEnemyUnit(15,0,51,48,4), new GeneratedEnemyUnit(7,-17,32,49,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "1cacfa14f3ee26a84ab33564f6e9367e94e566cd9060c16f122628e45648dcd7");
        }

        private static void Case_04587()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4587,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,4,73,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-7,84,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-6,86,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-10,99,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-14,19,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-14,37,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,10,74,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-7,22,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,19,99,9,1), new GeneratedEnemyUnit(-6,15,43,24,1), new GeneratedEnemyUnit(-7,-5,61,15,2), new GeneratedEnemyUnit(15,2,49,29,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "546bc5baec4a2c02b6b723559408d34bfbf2bcd8a02b09a8d47a1162b502ded2");
        }

        private static void Case_04588()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4588,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-14,10,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-11,27,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-3,27,18,3), new GeneratedEnemyUnit(7,-3,39,30,2), new GeneratedEnemyUnit(-13,9,89,15,1), new GeneratedEnemyUnit(-18,9,58,36,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "156a841169f84bbf6c6b38fb89e9d9498b457c08845282f618cf5ca206805ee0");
        }

        private static void Case_04589()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4589,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,0,35,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-3,19,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-9,74,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,8,5,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-19,27,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,5,32,37,1), new GeneratedEnemyUnit(11,-9,99,3,1), new GeneratedEnemyUnit(18,17,85,38,1), new GeneratedEnemyUnit(-20,-3,22,12,2), new GeneratedEnemyUnit(13,17,55,27,1), new GeneratedEnemyUnit(-2,9,50,32,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "ad143df795ea51fb652694009e3ab179480513df31c7ff65a72a4d8a97b89221");
        }

        private static void Case_04590()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4590,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,19,21,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-7,83,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-10,75,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-17,76,14,4), new GeneratedEnemyUnit(1,2,33,23,4), new GeneratedEnemyUnit(-4,1,67,13,1), new GeneratedEnemyUnit(-12,-18,92,7,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e9a3449b1073b9622c93a62a19bf8b29533b2df21446a7575dbb96042d5e8909");
        }

        private static void Case_04591()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4591,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,2,84,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,0,96,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-2,85,41,1), new GeneratedEnemyUnit(-20,8,77,28,4), new GeneratedEnemyUnit(14,15,41,26,4), new GeneratedEnemyUnit(-19,-13,10,36,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "365a19345785a65c0c9cda45a0514fc4b1a8a7371035caf46448b7ae4e230b24");
        }

        private static void Case_04592()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4592,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,2,13,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,6,23,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,19,14,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-17,23,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,11,32,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-14,78,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-19,76,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-4,43,50,4), new GeneratedEnemyUnit(2,-2,59,30,2), new GeneratedEnemyUnit(19,-20,59,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "9ac7997608bb0cbca1e87abf85326ac2163e165991f33c222f69a3609f6eb61e");
        }

        private static void Case_04593()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4593,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,3,48,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,19,76,33,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8598b076d69d198e7be956a5581199380551218d2849da300180d00aff485d9f");
        }

        private static void Case_04594()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4594,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,17,72,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-1,36,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-18,78,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-10,43,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-5,89,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-6,31,17,1), new GeneratedEnemyUnit(18,-19,97,29,3), new GeneratedEnemyUnit(-2,-6,44,46,3), new GeneratedEnemyUnit(-1,10,28,26,3), new GeneratedEnemyUnit(19,3,100,16,3), new GeneratedEnemyUnit(-14,-15,32,37,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "72cec65118b1441eb41cd7d9821c061eb6a66ff0c1092b0ad32e0ebdd8f7f2ae");
        }

        private static void Case_04595()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4595,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,17,5,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-16,45,5,4), new GeneratedEnemyUnit(-10,-17,29,4,4), new GeneratedEnemyUnit(-17,-12,7,10,1), new GeneratedEnemyUnit(-14,-9,37,46,1), new GeneratedEnemyUnit(1,-2,69,10,2), new GeneratedEnemyUnit(16,5,48,41,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "f5a5cb52df277b647488e9875e22ecfd508ad917d3878348309689333352e90a");
        }

        private static void Case_04596()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4596,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-8,19,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-17,64,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,20,28,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-7,61,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-4,59,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-13,67,5,1), new GeneratedEnemyUnit(2,-6,89,4,1), new GeneratedEnemyUnit(-16,-2,54,44,3), new GeneratedEnemyUnit(-11,-9,76,12,3), new GeneratedEnemyUnit(8,20,24,48,2), new GeneratedEnemyUnit(9,-12,94,17,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "d16ca5a190becefbc94c94941eccf5239525bc0373cdec2bb51abd151917099a");
        }

        private static void Case_04597()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4597,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,6,90,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,17,96,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-12,36,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,6,76,26,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "4d2d236e5ae04b319a77454a2e41f426f960d3da96b02fbd5cf5b0116ff4f8c6");
        }

        private static void Case_04598()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4598,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-9,38,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,3,80,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,11,92,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-8,91,29,2), new GeneratedEnemyUnit(-4,-19,44,49,3), new GeneratedEnemyUnit(-5,7,27,14,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "3cb328b9d0f2da68eda77c1d0be1d49fa7659133de460638d5e03ac311258242");
        }

        private static void Case_04599()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4599,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,19,87,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-19,87,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,17,44,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,14,66,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-12,86,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-19,55,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-15,81,10,3), new GeneratedEnemyUnit(-4,10,52,4,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "c7426270c79408b98da11dd11d05ce1b836417a85da7e1f3d08310a63f07d9b4");
        }

        private static void Case_04600()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4600,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,18,22,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-18,75,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-13,6,21,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "49af6c7297472e17de564531d747fada4860dcc12b7db1bbcab31fbc66db9c67");
        }

        private static void Case_04601()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4601,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-16,30,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,20,68,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-11,91,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-18,36,21,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f022d8e245142768bbc9382a7075e0fd47d15f2bf51410c6d94aec526ba8307a");
        }

        private static void Case_04602()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4602,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,3,95,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,6,36,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,16,29,41,2), new GeneratedEnemyUnit(-17,2,56,45,1), new GeneratedEnemyUnit(17,-13,14,6,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "bf66f58f886a705261be54ef41ddfb1c18d04f68a281e84573a121c34d035fb3");
        }

        private static void Case_04603()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4603,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-20,100,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-7,90,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,10,75,27,1), new GeneratedEnemyUnit(13,-19,65,43,2), new GeneratedEnemyUnit(11,-16,72,31,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "2bcf1ae8d3c936507b051adb5d61cfd8364a8afc381c55fb783d810e0a202f61");
        }

        private static void Case_04604()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4604,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-19,32,4,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "224bc1d304b5abbbcae66f1dafbbc8a405d29ebfc30f6e11f00f42936d29c6a5");
        }

        private static void Case_04605()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4605,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,17,87,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,13,97,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-18,64,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,5,5,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-17,94,45,2), new GeneratedEnemyUnit(-18,-17,27,12,4), new GeneratedEnemyUnit(-11,-5,50,19,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "fdc3cdcf76be8376ede5b495d6d9076f88801bf166c27a5600b29538e137c6d1");
        }

        private static void Case_04606()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4606,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-20,9,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-4,18,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,1,53,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,13,8,2,2), new GeneratedEnemyUnit(-5,-16,93,39,2), new GeneratedEnemyUnit(-3,-1,65,15,3), new GeneratedEnemyUnit(14,8,28,41,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "4817a65e0191a884c160fd5fd2c776e83e46552e0ebfea5aab51adb4c8b52609");
        }

        private static void Case_04607()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4607,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,8,84,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,7,33,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-20,81,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-10,65,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,20,20,5,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "5ac358a34ae06edf3a69a2b7531348b8c77d84f3ded72dbfabf69eb5995b3023");
        }

        private static void Case_04608()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4608,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,8,46,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,16,49,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-11,25,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-2,89,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-8,66,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-2,25,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,19,30,22,3), new GeneratedEnemyUnit(15,4,34,41,2), new GeneratedEnemyUnit(-19,15,61,32,1), new GeneratedEnemyUnit(7,-9,60,38,2), new GeneratedEnemyUnit(15,17,54,37,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "90d0307393f45a55ccf5fb9c327501f8f24b6bbb905e9972c4ed47b1f1ae234b");
        }

        private static void Case_04609()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4609,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-7,44,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-11,39,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,11,19,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-12,86,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-3,28,3,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "0fa0516d8283b943684c780b83cdc3a2f8c169d017c201e54b883f55098bbb9b");
        }

        private static void Case_04610()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4610,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,13,48,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,3,93,28,3), new GeneratedEnemyUnit(-7,1,69,32,4), new GeneratedEnemyUnit(-5,4,65,48,4), new GeneratedEnemyUnit(20,3,68,49,4), new GeneratedEnemyUnit(20,-2,52,10,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "8656d22d8b466b65b29e35d8b03141c6d6bc6439991cb13ec22437a773a7f0a7");
        }

        private static void Case_04611()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4611,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-5,57,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,19,7,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,20,64,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-16,69,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-14,6,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,9,94,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-11,5,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-11,67,5,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "65ae6d6e22213159cffc4a2b05b3305826b48307fce06eaddb9cfe7c4d97748c");
        }

        private static void Case_04612()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4612,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-6,26,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,10,92,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,18,18,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,14,77,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,9,64,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,10,94,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,15,21,3,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "1051b2626c3b48e4a2226d9c03eb7e7665965cc49496b244252728a374523435");
        }

        private static void Case_04613()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4613,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-2,44,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,1,50,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,16,98,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-6,49,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,15,49,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-20,74,32,4), new GeneratedEnemyUnit(-19,-6,19,9,2), new GeneratedEnemyUnit(-13,-7,38,36,3), new GeneratedEnemyUnit(7,-7,6,50,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "ad141c43fef77737e1b6fc5d22b268c54976814e0b33a0436cf8e6c9522dd262");
        }

        private static void Case_04614()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4614,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,4,37,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,1,70,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-2,88,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-15,33,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,15,37,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,15,59,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-3,30,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-18,5,4,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "19a0fb4446c594815cfab15884101537b0d5a216dd7e25b5566a5f62d6c51793");
        }

        private static void Case_04615()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4615,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,2,61,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-2,58,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,12,56,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-8,69,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-20,31,1,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "d881e7ae1b15ecab7c9c5198d309f6a77978f2f3d0ad23a75474904e9ec1931f");
        }

        private static void Case_04616()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4616,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-8,45,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,20,19,1,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "46bb64eea666b5d4cffb33971e7c9a8580f109133e8c97c13fbcdc58cf54257f");
        }

        private static void Case_04617()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4617,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,6,81,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-19,22,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,11,85,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-10,19,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-2,20,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,6,25,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,9,43,19,2), new GeneratedEnemyUnit(16,0,9,29,1), new GeneratedEnemyUnit(12,7,81,50,1), new GeneratedEnemyUnit(18,-8,71,10,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "3c9c40f2a10f501adfdf403f19a26284aa9efb53f90643d17397b412535feac6");
        }

        private static void Case_04618()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4618,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,1,37,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,12,93,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,15,97,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,14,29,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,13,20,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-6,80,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,11,25,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,1,79,8,4), new GeneratedEnemyUnit(-15,13,16,50,2), new GeneratedEnemyUnit(-9,-20,98,6,4), new GeneratedEnemyUnit(-13,-18,98,14,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "0cb594af53bea06e56622de43ebf471c2c377e7097810e9063e3b6b78debe78b");
        }

        private static void Case_04619()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4619,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,1,9,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,11,77,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,18,96,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-12,50,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,14,57,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,11,57,8,3), new GeneratedEnemyUnit(12,-1,97,26,4), new GeneratedEnemyUnit(-8,-12,95,27,2), new GeneratedEnemyUnit(9,18,69,25,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "7ae49398177e0d55a6ae9c38e9ce75d6cd69a51d2ec7c3e4e89e34671f81d3fc");
        }

        private static void Case_04620()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4620,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,18,71,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-12,93,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-8,27,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,16,14,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-17,69,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,2,100,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-12,47,40,4), new GeneratedEnemyUnit(15,15,35,3,4), new GeneratedEnemyUnit(-5,-2,84,22,1), new GeneratedEnemyUnit(15,-19,48,1,1), new GeneratedEnemyUnit(-11,7,69,35,3), new GeneratedEnemyUnit(1,16,28,46,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "34f35ea9c0d5ea57471a2eeb2bdc64acf53fc3e71a68cbaffd3cf82cd49ac5c2");
        }

        private static void Case_04621()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4621,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,16,22,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,3,53,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,12,94,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-20,56,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,6,45,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,2,28,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-10,39,44,2), new GeneratedEnemyUnit(-9,3,43,10,2), new GeneratedEnemyUnit(-14,17,86,44,1), new GeneratedEnemyUnit(-15,14,74,10,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "6b4736810702dca7a03b431dd3690bd9024f186adf591b5825b835a5ba7181f6");
        }

        private static void Case_04622()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4622,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-11,67,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,3,33,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-14,48,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,8,99,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,4,73,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-17,37,22,1), new GeneratedEnemyUnit(-16,9,29,47,1), new GeneratedEnemyUnit(10,-12,24,17,1), new GeneratedEnemyUnit(-18,-6,21,32,1), new GeneratedEnemyUnit(-3,-9,52,6,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "4e257542abfa8b5bb5ee02a79d5ab71cf1d449151538019c8f2dd5a2e181ea5a");
        }

        private static void Case_04623()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4623,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,10,75,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-20,26,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,19,76,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-7,71,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,13,75,7,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "320a98e1c286a6c9970119702dd67b0c67cf167462569398f076f3d8311805e5");
        }

        private static void Case_04624()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4624,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-14,89,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-15,54,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-13,89,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,6,89,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-2,40,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-17,21,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-12,45,25,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "6c382cbb2e326cd769e5fec34f67a7ce0c77271292663af91069de969bd49cc7");
        }

        private static void Case_04625()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4625,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,3,7,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-8,67,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,0,32,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-2,79,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,20,74,7,2), new GeneratedEnemyUnit(-18,20,38,50,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "0e1b94c84bbacbba5373583ac9993533d472a1d9e56fb82d2e1d4ec9e91a1078");
        }

        private static void Case_04626()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4626,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-1,50,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-17,89,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,18,67,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-9,71,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,15,94,2,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "ea9d8b4335d8947c7599fda7399a2136020d68ec130d7247b23bd9e7125e3bd1");
        }

        private static void Case_04627()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4627,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,7,31,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,1,8,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,3,93,30,4), new GeneratedEnemyUnit(-2,-11,19,28,3), new GeneratedEnemyUnit(19,-20,36,23,2), new GeneratedEnemyUnit(3,-3,68,50,1), new GeneratedEnemyUnit(16,-5,13,26,4), new GeneratedEnemyUnit(-2,-2,40,36,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "434db903c0565393ce3195c006edc5ffaff1e89403c885f077f1c0c2f9994092");
        }

        private static void Case_04628()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4628,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-12,32,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,5,59,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,15,73,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,0,34,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-5,26,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,18,22,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-3,12,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,6,14,10,4), new GeneratedEnemyUnit(-2,12,35,24,4), new GeneratedEnemyUnit(4,-18,85,20,3), new GeneratedEnemyUnit(11,3,32,12,2), new GeneratedEnemyUnit(5,19,66,49,2), new GeneratedEnemyUnit(-4,2,63,36,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "7d2029930eccbba6557759ab13d45014a51331ab0652998c9dd334bc749130d8");
        }

        private static void Case_04629()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4629,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-20,63,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,2,38,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-14,49,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-7,78,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,3,9,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,12,16,50,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f651640f82796e67f3e508e1a0f85e6d8735c20dc968b605ef95e8ba1af53b2a");
        }

        private static void Case_04630()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4630,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,10,100,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-9,55,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-12,40,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-7,81,20,3), new GeneratedEnemyUnit(-2,2,25,33,1), new GeneratedEnemyUnit(17,-19,50,1,2), new GeneratedEnemyUnit(1,-4,14,9,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "5853e493e50a6f5d04d113211f74dc39f0dfa0dbd410ddf63df130fa9a1df1bf");
        }

        private static void Case_04631()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4631,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,14,45,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,19,35,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,5,77,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-7,76,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,13,69,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-20,50,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,0,78,39,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "aea071b578972e5bd7ec08eb188fff84bd284bd0e41a156b781fc363f541c17e");
        }

        private static void Case_04632()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4632,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,14,87,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,19,90,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,14,7,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-12,64,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,12,70,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-9,5,27,1), new GeneratedEnemyUnit(10,-18,25,20,3), new GeneratedEnemyUnit(-14,-12,39,34,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "3331df3287f048f06600fcd22b0d23479f8b42dbe688bcf040ec2f1f90f9b1a0");
        }

        private static void Case_04633()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4633,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-15,50,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-12,87,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,4,31,16,1), new GeneratedEnemyUnit(-13,1,57,2,2), new GeneratedEnemyUnit(5,11,26,49,4), new GeneratedEnemyUnit(12,-15,97,46,3), new GeneratedEnemyUnit(-2,20,17,11,1), new GeneratedEnemyUnit(-14,20,17,43,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "59109ed2d5c8648b4546499cc63916bb34044a93ee18f7ed6117cc228c1c75a1");
        }

        private static void Case_04634()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4634,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,8,9,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-11,79,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-5,95,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-4,31,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,16,79,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-5,39,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,7,52,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-15,10,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-13,42,11,4), new GeneratedEnemyUnit(20,-2,33,24,1), new GeneratedEnemyUnit(16,1,23,18,4), new GeneratedEnemyUnit(-15,-20,70,6,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "19fa8bc1c989563016057483c404461fefd72bc05e4dd7747cf656ef9ad3aaf5");
        }

        private static void Case_04635()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4635,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-20,42,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-3,53,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-9,83,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-6,71,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,10,74,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,10,27,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-3,29,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,12,8,2,2), new GeneratedEnemyUnit(15,6,78,28,4), new GeneratedEnemyUnit(-17,2,49,9,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "f77df492df0800b1afab1e7c32b9b087a017c39712d7d1bef17577031fa15fe4");
        }

        private static void Case_04636()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4636,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,9,31,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-9,69,10,4), new GeneratedEnemyUnit(14,-16,48,35,2), new GeneratedEnemyUnit(10,9,5,3,2), new GeneratedEnemyUnit(10,2,45,16,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "4196fb3ece57282af916a6a74d825e5639ae0b3f07601a370779ec71c9a33c5d");
        }

        private static void Case_04637()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4637,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-16,59,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-10,98,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-16,95,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-14,86,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,2,16,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,17,82,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-13,52,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-2,17,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,2,31,43,3), new GeneratedEnemyUnit(5,-16,76,17,2), new GeneratedEnemyUnit(4,-10,74,5,2), new GeneratedEnemyUnit(20,7,100,7,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "c39c663786b1cf20240a0354a5e2141cf4eecae715d71d5ed20e4d110d3fbce4");
        }

        private static void Case_04638()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4638,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,12,12,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-19,73,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,4,76,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-11,94,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-6,66,6,4), new GeneratedEnemyUnit(-5,12,82,17,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "133195a224695ab07ab9e69b57b1b2f04116feea6b8c48b65c98839cc53fec8c");
        }

        private static void Case_04639()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4639,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-16,78,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,9,81,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-17,79,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-10,36,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-2,72,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-2,46,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,16,68,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,1,6,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,9,9,27,4), new GeneratedEnemyUnit(-9,8,99,44,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ad0384d72ce8af9ac406ea867ae27487f506489cac5296d431c030918a845c09");
        }

        private static void Case_04640()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4640,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,13,98,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-12,77,1,3), new GeneratedEnemyUnit(10,13,11,41,2), new GeneratedEnemyUnit(14,5,31,11,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "d282663719d66c51ca6c5e4d3d5bfebf0ba3bfabf3be9847e613da86a0bf3595");
        }

        private static void Case_04641()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4641,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-4,87,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,13,27,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,3,51,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-1,43,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-15,9,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,16,33,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-14,95,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,15,47,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-10,45,50,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "ae437e82a38630a5c871e0adca28c6c26d6cf07128a04779393848374c2b3261");
        }

        private static void Case_04642()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4642,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-9,50,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-10,72,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-7,95,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,9,81,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-17,22,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,16,64,3,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "17eb177e41ce236aedc2bf0b4015caa03c03de5d85675a29eec81cf84e11592e");
        }

        private static void Case_04643()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4643,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-6,91,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,3,62,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-16,70,33,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "6a755c94b14bb3d7b5ee27a20b163e8ffbce78a59feb6f45af633d83c50e946b");
        }

        private static void Case_04644()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4644,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,10,31,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-13,68,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,20,46,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,13,46,13,2), new GeneratedEnemyUnit(-20,10,64,1,3), new GeneratedEnemyUnit(-19,7,16,38,3), new GeneratedEnemyUnit(-3,-13,17,20,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "d11ec63b22730d7755ff0be8a088b5a0c73cbec94ac4ed22a0ca7325cd9e97e1");
        }

        private static void Case_04645()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4645,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-9,17,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-20,7,48,3), new GeneratedEnemyUnit(3,10,35,42,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "974227ea3bee09e2d2282e510e4f41636dbd106e28db1bcab4aeb7882506c5ef");
        }

        private static void Case_04646()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4646,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-9,61,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-6,49,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-14,99,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-11,89,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-2,92,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,14,47,12,3), new GeneratedEnemyUnit(8,11,27,29,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "b6bfc2e4a66b49764ddb576b8f3f2fe4018f5cd22fb586d173f31c5ecf524d15");
        }

        private static void Case_04647()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4647,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,2,90,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,7,38,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,5,85,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-4,41,1,3), new GeneratedEnemyUnit(-15,1,89,32,2), new GeneratedEnemyUnit(3,17,26,42,4), new GeneratedEnemyUnit(-17,1,35,2,1), new GeneratedEnemyUnit(-13,17,72,29,4), new GeneratedEnemyUnit(10,4,87,44,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "4068fdf2545ecb2ff6f3a4e32e1b42070f6c85825ef426b23e8867a5ac82b97a");
        }

        private static void Case_04648()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4648,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,6,47,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-3,89,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-6,60,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-20,17,12,3), new GeneratedEnemyUnit(8,15,42,40,3), new GeneratedEnemyUnit(-1,4,27,48,1), new GeneratedEnemyUnit(5,12,28,25,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "23bd7994b3a71a2db905a8b8996e6f5e3e76d5b34641eb9c5666f8c37f32f769");
        }

        private static void Case_04649()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4649,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,10,25,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-14,94,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-2,18,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-11,25,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,6,78,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-10,76,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,5,19,28,2), new GeneratedEnemyUnit(-3,-5,14,46,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "47805398fd23eab70e9876250f349ceb3195a589351b36456f24929d03ff3383");
        }

        private static void Case_04650()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4650,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,3,15,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,13,83,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-3,91,25,4), new GeneratedEnemyUnit(3,-1,60,32,1), new GeneratedEnemyUnit(-7,11,52,47,1), new GeneratedEnemyUnit(-2,17,74,36,1), new GeneratedEnemyUnit(-10,13,49,28,3), new GeneratedEnemyUnit(-20,7,50,33,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "559e1f79005741c385184a019d887936c2646d63b0b4187d051ed9a778a7d76c");
        }

        private static void Case_04651()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4651,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,0,51,5,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "8b2cd9a5f3cdf2eb8f96d322cf8f40696fcc78f508b1ec9d4c424d014b20a1f3");
        }

        private static void Case_04652()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4652,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-1,46,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,13,78,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-3,74,23,2), new GeneratedEnemyUnit(-1,17,83,13,3), new GeneratedEnemyUnit(-14,0,85,48,2), new GeneratedEnemyUnit(18,-16,42,28,1), new GeneratedEnemyUnit(11,-16,8,48,1), new GeneratedEnemyUnit(10,-3,55,50,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "46eb4cdf24afe2ed609969572152a0bf422902ae7d0b7c2fab4446b669c63d12");
        }

        private static void Case_04653()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4653,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,14,75,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-12,53,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-20,91,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-5,31,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,10,18,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-18,24,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,9,89,37,4), new GeneratedEnemyUnit(-15,-7,48,12,1), new GeneratedEnemyUnit(-15,12,67,37,1), new GeneratedEnemyUnit(19,16,81,31,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "fe9095525a20c616f63af60a067143d6303830ca975a5cbb8769bb6603f6502e");
        }

        private static void Case_04654()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4654,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,0,26,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,15,6,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,12,79,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,6,53,33,2), new GeneratedEnemyUnit(-5,-13,20,11,3), new GeneratedEnemyUnit(-6,-9,54,33,2), new GeneratedEnemyUnit(17,15,18,36,4), new GeneratedEnemyUnit(18,16,25,11,1), new GeneratedEnemyUnit(-15,-18,44,29,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "79fb7d0b876b5dca14000b0d9c40b8cc266c861a9b60294d8c85a04feee7496c");
        }

        private static void Case_04655()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4655,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,2,72,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-20,27,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-6,65,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,2,22,14,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "fc18fc8d913572e20adf86ee1a11fff03fed14d4f5b0966cde5940ddde2dd6ae");
        }

        private static void Case_04656()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4656,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-1,34,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-10,20,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,16,49,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,17,43,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-14,25,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-4,48,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-8,13,38,1), new GeneratedEnemyUnit(-4,13,20,23,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "977ae831b7c55a28dee40a2eecbaebfc683fe953a4e2c57fe2aecfb06c4b845d");
        }

        private static void Case_04657()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4657,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,5,85,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-9,60,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-4,31,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-10,77,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,3,14,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,2,8,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,0,36,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,6,36,31,4), new GeneratedEnemyUnit(-8,-3,45,16,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "6465267ee074ac646fb6fd895e6ed424568528eba4624e64d487ae5c7bebded2");
        }

        private static void Case_04658()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4658,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,8,98,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,12,63,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-15,41,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,10,79,3,1), new GeneratedEnemyUnit(12,10,9,36,3), new GeneratedEnemyUnit(-11,20,87,28,4), new GeneratedEnemyUnit(-18,13,51,37,4), new GeneratedEnemyUnit(1,17,27,27,2), new GeneratedEnemyUnit(-4,17,13,41,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "ecb0694160862e8196f293a6bcb2b431bbf9726f31d3cffa091352f2aed606fe");
        }

        private static void Case_04659()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4659,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-9,83,2,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "98e7620582a7f1418f312dcabf02d2d23eb9c5497d1aea55baa43ec94c26f53d");
        }

        private static void Case_04660()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4660,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-17,18,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-13,27,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-13,34,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,3,59,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-19,67,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,17,93,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,6,25,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,10,60,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,17,41,12,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a836105c8090035343dc68a920a982c8d5106b6929a56f6d78159feddb6949a5");
        }

        private static void Case_04661()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4661,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-2,93,7,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "d9127f727d766cb085c263be80b795339c719d22c0364674b2d4f3dea893d51f");
        }

        private static void Case_04662()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4662,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-9,42,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,16,86,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,1,100,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,1,33,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-18,78,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,5,81,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,3,70,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-16,57,2,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "f90ee6ecde88177b714f7002424704e767ab5f04239d7298f5d5cdc7bdc6ef21");
        }

        private static void Case_04663()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4663,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,6,92,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-9,83,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-20,32,34,1), new GeneratedEnemyUnit(15,-20,63,25,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "079e44e4e40e2d952453720e45a13937d97da27b5411f9dc4a94d12106d01b73");
        }

        private static void Case_04664()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4664,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,8,45,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,16,82,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-11,75,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,4,36,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-11,41,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-9,97,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-20,32,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,20,9,3,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "e97e90629dd9630633418851d9540480fd9ca043ed3d036338f0513e53f13627");
        }

        private static void Case_04665()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4665,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-1,83,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,14,88,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,2,20,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,20,59,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-15,71,25,3), new GeneratedEnemyUnit(7,0,85,25,3), new GeneratedEnemyUnit(-1,5,9,29,2), new GeneratedEnemyUnit(9,-9,45,15,1), new GeneratedEnemyUnit(-7,14,67,30,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "569a987be2bdc5ed27c04be111c3b5984868847f3444fa0597f9470374296105");
        }

        private static void Case_04666()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4666,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,4,5,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,17,55,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,2,42,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-16,41,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,10,75,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,15,30,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-8,89,1,3), new GeneratedEnemyUnit(-16,-7,94,38,1), new GeneratedEnemyUnit(16,-9,44,46,1), new GeneratedEnemyUnit(5,-1,49,15,4), new GeneratedEnemyUnit(-18,6,97,42,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "811694b2889e49f5eb362580f1903fe0bdc6eeb3ae1c8cc7ff4fd2411f375f48");
        }

        private static void Case_04667()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4667,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,19,11,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-14,7,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-12,79,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,12,56,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,18,74,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,15,47,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,20,68,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-7,55,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,7,25,22,4), new GeneratedEnemyUnit(-8,-9,33,4,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "d5c23a0c272b0d6e4487a694f34c259089c9b1811cc212a6bbafea9b5afe3df2");
        }

        private static void Case_04668()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4668,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-5,55,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-3,43,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-8,36,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-17,12,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-6,97,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-5,90,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-10,81,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,19,92,12,4), new GeneratedEnemyUnit(-20,-18,51,37,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "626500bb2155fafbd7325575281e7f0718955bed7b1ab2be6eb6654c83469ebe");
        }

        private static void Case_04669()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4669,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,3,60,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-19,33,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-13,54,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,17,33,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,12,55,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-18,72,38,1), new GeneratedEnemyUnit(-2,-16,71,9,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "cc2f0297719dc0db8465f21dca2affcf20ce9ee534ea190674def62100d3e05a");
        }

        private static void Case_04670()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4670,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,11,85,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,20,64,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,15,7,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-6,36,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-14,42,16,3), new GeneratedEnemyUnit(7,-20,38,9,2), new GeneratedEnemyUnit(7,3,10,15,2), new GeneratedEnemyUnit(-7,16,12,44,2), new GeneratedEnemyUnit(5,8,9,46,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "45b3c0f50af476299d3e1069084916f466df31b296063a879e896b4d4a7b2144");
        }

        private static void Case_04671()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4671,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-11,28,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,4,93,2,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "2ba54d295460228fcb7b80d7efaebbe7c1baba3c823b1cf24fa96da5658e446f");
        }

        private static void Case_04672()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4672,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,12,86,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,19,50,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-5,91,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-7,91,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,0,29,1,4), new GeneratedEnemyUnit(-13,10,15,31,1), new GeneratedEnemyUnit(-13,-8,42,42,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "2533f51375740968865d292f6adf90754a756113073849d376da9c49f8cd6705");
        }

        private static void Case_04673()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4673,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,1,48,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-2,42,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-11,11,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-5,78,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,14,64,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-8,16,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-6,79,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-13,37,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,6,36,32,4), new GeneratedEnemyUnit(19,-10,34,15,1), new GeneratedEnemyUnit(-8,11,85,11,3), new GeneratedEnemyUnit(6,17,76,21,4), new GeneratedEnemyUnit(-14,8,52,44,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "44562ebf26fea980e8c81fdd4585b765235bfef705c95d73285868ac59043e51");
        }

        private static void Case_04674()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4674,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,2,63,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,8,54,5,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "04a562bbd5da22f2c3defd2e0924cca137a21e32046ff955a9341b584862b082");
        }

        private static void Case_04675()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4675,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,12,69,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-15,71,25,3), new GeneratedEnemyUnit(-10,-9,53,44,1), new GeneratedEnemyUnit(-4,18,75,47,4), new GeneratedEnemyUnit(-8,-20,60,5,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "0b3e71a5997b8bf55bf9b813932976dcc7af3f0456780096f4b7851bfcca0321");
        }

        private static void Case_04676()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4676,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-11,72,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-18,11,1,1), new GeneratedEnemyUnit(13,14,96,1,3), new GeneratedEnemyUnit(18,-15,79,38,4), new GeneratedEnemyUnit(-3,1,61,13,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "d0acc63c4d9cd087aed567d6824652d8b3a7bbf317ec2b020ebe0435aac65189");
        }

        private static void Case_04677()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4677,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,19,19,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,13,37,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,6,52,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-8,83,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,15,24,7,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "abd16b3f5374de3b36b0730b2874b0c978bd89259fd3bda0c18e64119b7274f3");
        }

        private static void Case_04678()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4678,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,19,92,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,10,15,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,12,58,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,10,48,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,7,45,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-8,15,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-14,28,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,12,41,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-5,89,22,4), new GeneratedEnemyUnit(9,-10,59,19,2), new GeneratedEnemyUnit(-18,-13,22,11,1), new GeneratedEnemyUnit(11,12,59,9,4), new GeneratedEnemyUnit(-6,-18,91,18,1), new GeneratedEnemyUnit(-5,-14,5,3,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "29b4b5ae0691e207f196767ba12754d54c08bd45b1692c91ad1c2cc4e7e450ce");
        }

        private static void Case_04679()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4679,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,13,26,1,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "6ceca82a98318323a92eda7b893d33b7826d29145abffb3468ec16db010ae91e");
        }

    }
}
