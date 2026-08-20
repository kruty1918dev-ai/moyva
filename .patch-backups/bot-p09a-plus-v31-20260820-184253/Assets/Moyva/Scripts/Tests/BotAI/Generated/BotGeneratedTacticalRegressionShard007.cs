using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard007
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_01260();
            Case_01261();
            Case_01262();
            Case_01263();
            Case_01264();
            Case_01265();
            Case_01266();
            Case_01267();
            Case_01268();
            Case_01269();
            Case_01270();
            Case_01271();
            Case_01272();
            Case_01273();
            Case_01274();
            Case_01275();
            Case_01276();
            Case_01277();
            Case_01278();
            Case_01279();
            Case_01280();
            Case_01281();
            Case_01282();
            Case_01283();
            Case_01284();
            Case_01285();
            Case_01286();
            Case_01287();
            Case_01288();
            Case_01289();
            Case_01290();
            Case_01291();
            Case_01292();
            Case_01293();
            Case_01294();
            Case_01295();
            Case_01296();
            Case_01297();
            Case_01298();
            Case_01299();
            Case_01300();
            Case_01301();
            Case_01302();
            Case_01303();
            Case_01304();
            Case_01305();
            Case_01306();
            Case_01307();
            Case_01308();
            Case_01309();
            Case_01310();
            Case_01311();
            Case_01312();
            Case_01313();
            Case_01314();
            Case_01315();
            Case_01316();
            Case_01317();
            Case_01318();
            Case_01319();
            Case_01320();
            Case_01321();
            Case_01322();
            Case_01323();
            Case_01324();
            Case_01325();
            Case_01326();
            Case_01327();
            Case_01328();
            Case_01329();
            Case_01330();
            Case_01331();
            Case_01332();
            Case_01333();
            Case_01334();
            Case_01335();
            Case_01336();
            Case_01337();
            Case_01338();
            Case_01339();
            Case_01340();
            Case_01341();
            Case_01342();
            Case_01343();
            Case_01344();
            Case_01345();
            Case_01346();
            Case_01347();
            Case_01348();
            Case_01349();
            Case_01350();
            Case_01351();
            Case_01352();
            Case_01353();
            Case_01354();
            Case_01355();
            Case_01356();
            Case_01357();
            Case_01358();
            Case_01359();
            Case_01360();
            Case_01361();
            Case_01362();
            Case_01363();
            Case_01364();
            Case_01365();
            Case_01366();
            Case_01367();
            Case_01368();
            Case_01369();
            Case_01370();
            Case_01371();
            Case_01372();
            Case_01373();
            Case_01374();
            Case_01375();
            Case_01376();
            Case_01377();
            Case_01378();
            Case_01379();
            Case_01380();
            Case_01381();
            Case_01382();
            Case_01383();
            Case_01384();
            Case_01385();
            Case_01386();
            Case_01387();
            Case_01388();
            Case_01389();
            Case_01390();
            Case_01391();
            Case_01392();
            Case_01393();
            Case_01394();
            Case_01395();
            Case_01396();
            Case_01397();
            Case_01398();
            Case_01399();
            Case_01400();
            Case_01401();
            Case_01402();
            Case_01403();
            Case_01404();
            Case_01405();
            Case_01406();
            Case_01407();
            Case_01408();
            Case_01409();
            Case_01410();
            Case_01411();
            Case_01412();
            Case_01413();
            Case_01414();
            Case_01415();
            Case_01416();
            Case_01417();
            Case_01418();
            Case_01419();
            Case_01420();
            Case_01421();
            Case_01422();
            Case_01423();
            Case_01424();
            Case_01425();
            Case_01426();
            Case_01427();
            Case_01428();
            Case_01429();
            Case_01430();
            Case_01431();
            Case_01432();
            Case_01433();
            Case_01434();
            Case_01435();
            Case_01436();
            Case_01437();
            Case_01438();
            Case_01439();
        }

        private static void Case_01260()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1260,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-3,86,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,19,96,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-20,88,48,2), new GeneratedEnemyUnit(-8,-15,27,4,1), new GeneratedEnemyUnit(4,-1,52,27,3), new GeneratedEnemyUnit(-20,11,57,24,3), new GeneratedEnemyUnit(8,12,71,41,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "d8fcb86ee860ec2f866ba1f3637402ed8cacfbb84a994fe074e5e3c45909d9d6");
        }

        private static void Case_01261()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1261,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,14,17,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-14,31,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-5,54,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,13,39,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-18,49,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,11,73,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,19,37,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,14,85,27,2), new GeneratedEnemyUnit(-5,-5,10,41,4), new GeneratedEnemyUnit(-4,-11,76,47,1), new GeneratedEnemyUnit(10,19,41,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "981654254be471d441a787f68e280df875aed83727c57f2ca70932913cdab1dd");
        }

        private static void Case_01262()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1262,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-14,35,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,13,17,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,3,44,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,12,54,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,7,61,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,13,91,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,4,8,1,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3f27f83d13d3e3d691e7b53fede691be07155c888b6dab56a914929b8966e48b");
        }

        private static void Case_01263()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1263,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-20,87,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,4,53,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-15,46,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,6,5,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,13,42,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-18,97,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,1,5,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,1,47,44,2), new GeneratedEnemyUnit(-4,18,90,33,1), new GeneratedEnemyUnit(7,-5,95,3,2), new GeneratedEnemyUnit(13,-20,37,28,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "37a7656e60ad19f0cec6dd049be746c7537cc0c2a4dd9f57ffb51c14bc72bf5b");
        }

        private static void Case_01264()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1264,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,12,28,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,20,19,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-5,26,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,1,36,35,1), new GeneratedEnemyUnit(15,-2,23,31,1), new GeneratedEnemyUnit(-15,10,78,23,1), new GeneratedEnemyUnit(2,10,93,2,1), new GeneratedEnemyUnit(-2,7,70,24,1), new GeneratedEnemyUnit(-16,-7,80,27,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "c735e94e593ba8129fca526e943c9057ff74bc1f1eaff2a882dd55b0b67ac5f2");
        }

        private static void Case_01265()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1265,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,4,70,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,9,52,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,17,9,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-18,9,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,0,52,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-5,78,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,17,46,48,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a7f4e61363770ff7fad8bb45e9766a2bed66a5e27fe8e44a964b0e3b8f6acec8");
        }

        private static void Case_01266()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1266,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,13,40,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-11,85,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-20,25,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-19,57,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,11,54,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,10,23,10,1), new GeneratedEnemyUnit(20,-4,32,41,3), new GeneratedEnemyUnit(6,4,100,2,2), new GeneratedEnemyUnit(3,-14,48,46,3), new GeneratedEnemyUnit(-16,5,30,15,3), new GeneratedEnemyUnit(-7,2,34,22,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "f05fcddc870c0ebe29d4c104bb88e9c6c34a51fd62cd683b056709bfbb868d75");
        }

        private static void Case_01267()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1267,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-4,23,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,6,96,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,19,7,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-3,88,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-10,39,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,2,39,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,0,15,36,3), new GeneratedEnemyUnit(7,14,85,40,3), new GeneratedEnemyUnit(-17,12,34,49,1), new GeneratedEnemyUnit(7,-16,73,8,3), new GeneratedEnemyUnit(5,16,25,7,4), new GeneratedEnemyUnit(2,-15,41,4,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "82604411d769ed2bdb2b100dd41d08a7fafc4c2b13efdc2dc4759d20f1884a45");
        }

        private static void Case_01268()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1268,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,4,92,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-15,71,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-14,57,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,11,8,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-19,70,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-17,95,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,2,63,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-7,31,8,4), new GeneratedEnemyUnit(-5,-8,63,48,3), new GeneratedEnemyUnit(-20,-16,15,47,4), new GeneratedEnemyUnit(0,-14,85,42,1), new GeneratedEnemyUnit(-17,-17,39,43,3), new GeneratedEnemyUnit(14,-1,58,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "51e6f2d515d3519fb7b580990386c618dcabd748ccec529561ae825f36e34ad5");
        }

        private static void Case_01269()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1269,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,17,9,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,8,31,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-17,79,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-5,87,27,4), new GeneratedEnemyUnit(-10,-20,72,13,4), new GeneratedEnemyUnit(17,2,61,4,4), new GeneratedEnemyUnit(1,-2,80,16,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "16eb3875c4a3b4bf514aa0315c30a7e3665ddf2f13618133546c2d6353579381");
        }

        private static void Case_01270()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1270,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,9,18,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,4,55,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,10,47,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-13,71,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,17,24,19,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "f3ad6f90ab69ab3bbf7df3d1788840fe4979a4183d593c244e5f3944232b1f20");
        }

        private static void Case_01271()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1271,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,2,97,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,11,12,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,3,36,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,2,7,41,2), new GeneratedEnemyUnit(-20,8,8,49,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "6947dbaa00cb5703e56ab6a6029a94dea01a13c5b401e30b570157a3acf67426");
        }

        private static void Case_01272()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1272,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-14,91,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,3,78,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,4,28,8,4), new GeneratedEnemyUnit(18,-7,72,45,1), new GeneratedEnemyUnit(-3,-5,27,13,1), new GeneratedEnemyUnit(-4,18,98,40,2), new GeneratedEnemyUnit(-9,13,60,31,1), new GeneratedEnemyUnit(-2,16,32,10,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "ad5a74ad2048ede165f377a9292da655a8cade7b9f204030baddca65eb9d04a1");
        }

        private static void Case_01273()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1273,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,0,22,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-13,11,40,3), new GeneratedEnemyUnit(-12,19,36,48,4), new GeneratedEnemyUnit(5,-13,22,27,2), new GeneratedEnemyUnit(17,-6,81,43,3), new GeneratedEnemyUnit(2,-13,7,46,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "3cde648db5673a9cbfaf50b94a56f12187deb1c6ffda54bf0e47a1c871935db1");
        }

        private static void Case_01274()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1274,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,18,20,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,11,41,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,8,46,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,14,69,6,4), new GeneratedEnemyUnit(-7,7,97,36,2), new GeneratedEnemyUnit(7,8,99,33,3), new GeneratedEnemyUnit(15,-9,67,20,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "1a031faa8170f979fb9ac28c160bc4df951ce4b5feed1c2fa99b4a83ee0ee9f9");
        }

        private static void Case_01275()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1275,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-20,65,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-16,77,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-1,37,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-6,32,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,9,93,23,1), new GeneratedEnemyUnit(8,-16,23,17,4), new GeneratedEnemyUnit(-16,11,25,12,1), new GeneratedEnemyUnit(8,-5,28,42,3), new GeneratedEnemyUnit(0,19,52,29,1), new GeneratedEnemyUnit(11,12,83,47,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "a85bb20892b5d596582a5ce697fb4e099800041ebebd16cd91e9355199e50cd7");
        }

        private static void Case_01276()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1276,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,3,80,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,12,12,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-18,73,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-4,29,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-16,96,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-2,8,13,4), new GeneratedEnemyUnit(-12,19,28,16,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "6878d5aed1c1eeda7d9fc8fe80841074cc8987f91493aa534d324428ebe403b8");
        }

        private static void Case_01277()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1277,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,1,64,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-11,12,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-10,82,46,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "6c5e41c2abccdcf07f234de7f8740ae13124196c869abd42a8d15bda4577523f");
        }

        private static void Case_01278()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1278,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,9,32,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,13,95,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,13,96,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-7,87,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,7,17,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-14,72,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,18,51,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,15,77,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2b0151f1c41e2634afefd53848b999ab82d112ff5e41d1a0ee2ec6de53916621");
        }

        private static void Case_01279()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1279,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,11,21,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-7,15,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-7,23,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,16,43,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,16,36,29,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "5e090bd621b66a145a0b7ba33834e8b0131b4a5b9c6e4d5d981bf0bd14dc2050");
        }

        private static void Case_01280()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1280,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-6,33,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,15,78,29,2), new GeneratedEnemyUnit(17,-20,14,5,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "117aa32ae45569b88ae65da7c61b36e52a09abf2032ca27ad9fdd314be519386");
        }

        private static void Case_01281()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1281,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,9,17,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,1,98,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-5,26,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-13,56,37,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b69a44b611e90eeb570d7d0f6e10b07e14d366907a14657d4965fcfaf8379b9a");
        }

        private static void Case_01282()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1282,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-8,71,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-4,59,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,10,85,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,16,100,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,0,18,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-3,6,43,1), new GeneratedEnemyUnit(-9,-8,35,22,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "a5bdfe7607d33bea868b2c2ef1e30e3a013072e48fee4d2cb1426d98b7e864ac");
        }

        private static void Case_01283()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1283,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-20,26,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,0,42,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-13,89,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,6,91,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,1,17,3,4), new GeneratedEnemyUnit(7,7,85,33,4), new GeneratedEnemyUnit(9,13,36,9,4), new GeneratedEnemyUnit(-20,15,56,44,4), new GeneratedEnemyUnit(10,-2,40,10,4), new GeneratedEnemyUnit(19,5,100,47,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "135c3b55076a3b376cfcaa0270cd94b157531ff8c61ac3170854a213a671a4f3");
        }

        private static void Case_01284()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1284,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,17,9,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-11,78,7,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "7e09294cf04406022067c1f9aa92c6183cce95237e448a8a675777aa2d485ac3");
        }

        private static void Case_01285()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1285,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-6,42,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,4,86,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-19,66,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-15,93,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-20,17,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,9,88,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-18,68,3,3), new GeneratedEnemyUnit(-8,-16,44,16,4), new GeneratedEnemyUnit(-17,-13,30,41,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "c2b7e6bd772d8a13acce190c3c589f29929547302d89bde3ea435ebfc144bdc5");
        }

        private static void Case_01286()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1286,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-14,33,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-5,7,2,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "ce5847f8eaf34a124e3ccfdbd3d620ec118ee6ec8b2d3e91a8ee42edb8ce55c3");
        }

        private static void Case_01287()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1287,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-17,53,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,2,73,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-3,92,21,2), new GeneratedEnemyUnit(19,11,58,27,4), new GeneratedEnemyUnit(-3,0,77,13,2), new GeneratedEnemyUnit(0,8,55,40,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "454b77000700c723b3a159bc891589cb5ae1815198bf6443ccc2791f01bb0bf5");
        }

        private static void Case_01288()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1288,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,12,99,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-9,60,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,2,8,40,3), new GeneratedEnemyUnit(-2,11,37,5,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "dae1aac7f726327844778e16ab6a31142d00e1699808a54a570a99f04716b2dc");
        }

        private static void Case_01289()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1289,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,18,16,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-15,53,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,0,28,9,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "bb4221f1f81b637ad51cf6e8a34467e381cf94a138765cbe4d7e73d02a613817");
        }

        private static void Case_01290()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1290,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-1,41,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,1,66,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,1,7,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,8,95,40,4), new GeneratedEnemyUnit(-9,13,83,50,1), new GeneratedEnemyUnit(-12,18,45,1,3), new GeneratedEnemyUnit(20,-13,93,17,3), new GeneratedEnemyUnit(3,8,38,35,2), new GeneratedEnemyUnit(-12,13,69,46,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "0dfc7d72c914426e30ecf96737c43e95705cc41e9b88582eccc0e7b857096703");
        }

        private static void Case_01291()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1291,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,8,24,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,9,91,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-1,88,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,19,19,20,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "de748667e878f57475000cc1606e7181099e82fbbe4c41f6e9245812b8692c0d");
        }

        private static void Case_01292()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1292,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-3,15,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-9,71,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,20,21,12,2), new GeneratedEnemyUnit(-1,-11,69,8,3), new GeneratedEnemyUnit(-2,0,10,3,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "be106230a0b5c2341d497a8c5b033ac457c97b08bfa3f337a660e49a501c2eb3");
        }

        private static void Case_01293()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1293,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,4,82,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,18,34,26,4), new GeneratedEnemyUnit(10,-13,97,49,2), new GeneratedEnemyUnit(-3,-8,68,10,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b2c5f47139cfc8b1c7e280509685d1865ecd084eb1d9a49c9f7cdd22f796a7b0");
        }

        private static void Case_01294()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1294,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-1,52,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,14,69,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,9,50,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-10,55,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,15,27,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-16,56,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,7,54,40,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "d560ec293ba59f258d4e082a28d77534b76b9003ce34a0ed91ee353b6422849d");
        }

        private static void Case_01295()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1295,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-4,33,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,12,50,11,3), new GeneratedEnemyUnit(16,11,29,49,2), new GeneratedEnemyUnit(9,-2,9,15,1), new GeneratedEnemyUnit(-18,-4,57,12,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b563449c9c78b90f8357e20a4045ed069da86eb3f1e1577a386fb46f0a1ac580");
        }

        private static void Case_01296()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1296,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,11,67,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-15,91,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,4,29,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-10,24,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,4,96,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-2,69,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,13,66,32,3), new GeneratedEnemyUnit(5,18,30,35,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "07795d11ac5009d9852815335aeeba502be47123b0f80156eabe677e0cd54b57");
        }

        private static void Case_01297()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1297,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,10,49,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-5,60,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-6,86,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,14,36,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,0,42,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-5,23,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "dadc0c0a2a715334557d97da0e43ede23f697730386e9be9745fd552c89d6156");
        }

        private static void Case_01298()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1298,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-13,43,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,12,23,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,3,11,1,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "6725dc16cc6778d6f68bfda97d92671d6e24b50ed8ff6b3011609d4f1adbf452");
        }

        private static void Case_01299()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1299,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-3,100,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-10,56,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-16,47,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,14,50,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-20,67,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,1,25,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-16,48,6,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "1854c565ad108e3f439ba5bbf56e49635c0d4b00767e937379f5408e5d1558d8");
        }

        private static void Case_01300()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1300,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-6,67,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-12,98,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,20,57,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,8,57,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,10,11,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,0,42,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,13,51,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-5,25,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,9,79,48,3), new GeneratedEnemyUnit(-8,7,74,26,1), new GeneratedEnemyUnit(18,-12,11,10,2), new GeneratedEnemyUnit(1,-8,20,8,1), new GeneratedEnemyUnit(16,6,41,17,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "260e4e3c6b7dd08c73da23dbebf9eedeb3fdcc3529ad55a4ea059bd74f6d193e");
        }

        private static void Case_01301()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1301,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,4,46,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-9,100,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-18,26,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-4,97,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,10,6,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-19,68,26,4), new GeneratedEnemyUnit(12,7,47,37,2), new GeneratedEnemyUnit(17,10,87,8,3), new GeneratedEnemyUnit(-8,4,27,1,3), new GeneratedEnemyUnit(1,-3,100,32,1), new GeneratedEnemyUnit(-7,-14,76,14,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "4cb8f028812c5593e624ca5f61b3d57a7ffef90c30a0363b62ea7b94afdf2bff");
        }

        private static void Case_01302()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1302,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-14,13,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-4,20,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,17,90,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-3,36,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-15,43,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,1,89,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,19,49,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,3,50,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,1,12,50,3), new GeneratedEnemyUnit(17,16,67,34,2), new GeneratedEnemyUnit(-5,5,29,24,1), new GeneratedEnemyUnit(5,-13,36,22,4), new GeneratedEnemyUnit(-5,20,10,24,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5298b47ca95348af9a9bf3deb9f998d72b51b124b05fdc4cb38ef0fadd42163b");
        }

        private static void Case_01303()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1303,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-7,35,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-12,28,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-12,85,40,4), new GeneratedEnemyUnit(-9,-5,13,15,3), new GeneratedEnemyUnit(17,14,22,24,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "38ba54a296ab7550d4183f13ec16f08ea2fd57917451cf91029c682edd2515c4");
        }

        private static void Case_01304()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1304,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-2,42,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-12,75,6,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "151d74fa73640a7129d60993fd2a7516274cfe1d39fc1b2b2ec90faead0a231a");
        }

        private static void Case_01305()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1305,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-12,97,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,15,55,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-2,41,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,17,68,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,0,92,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-16,16,8,1), new GeneratedEnemyUnit(-10,-1,33,48,1), new GeneratedEnemyUnit(-8,0,87,20,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "0a2ae0c810918967313c87a5b6f3f75c878b9fca8ce7e2de38754e8f4acb84a1");
        }

        private static void Case_01306()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1306,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-2,66,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,1,15,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-8,94,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,19,100,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,12,38,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,11,15,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-11,80,49,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "109766987d3494da8cb1cc8c5aa4cfb990f883dd4fd17284da93aa6b68e19a5c");
        }

        private static void Case_01307()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1307,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,15,56,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,15,28,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,13,95,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,16,28,7,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "91f4108a8ab5b893eb254bc93aec0a3601416ac715bbc985b8f2d914f11508f9");
        }

        private static void Case_01308()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1308,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,16,20,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,8,17,4,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "cc54e998addf159f6f9112adc622f62bc08afc5f1c8fc2b7627530abbd54032f");
        }

        private static void Case_01309()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1309,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,17,31,1,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "fb2729cffd67f85150c87faddc83afda4c551899f41ba6324108fef6a07abc9c");
        }

        private static void Case_01310()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1310,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,9,40,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,9,84,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-7,15,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-15,16,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-6,46,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-13,34,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,17,16,3,3), new GeneratedEnemyUnit(18,-18,95,43,3), new GeneratedEnemyUnit(17,-5,14,25,2), new GeneratedEnemyUnit(-6,0,67,13,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "083bba06ae23fa18aeba094f838b3933629fe5b8453d588bfafedfab36ee76ee");
        }

        private static void Case_01311()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1311,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,11,24,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,17,12,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,3,11,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,18,90,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-2,95,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-19,25,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-12,51,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-11,20,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,9,40,31,1), new GeneratedEnemyUnit(10,14,85,13,4), new GeneratedEnemyUnit(14,-18,37,43,3), new GeneratedEnemyUnit(-12,8,48,4,1), new GeneratedEnemyUnit(-5,-2,56,3,2), new GeneratedEnemyUnit(-18,-3,10,17,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "eafce0a50a83ba512b8630637ae013e101be9616810c5d5a52df0b1df5281c72");
        }

        private static void Case_01312()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1312,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-20,96,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,4,65,11,3), new GeneratedEnemyUnit(0,2,16,31,4), new GeneratedEnemyUnit(11,6,67,43,4), new GeneratedEnemyUnit(-17,2,96,20,4), new GeneratedEnemyUnit(-2,-10,43,36,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "727d81b6d3857c717897341a7dc87c29cf7ed48a82bf64ae9d81bcb9d854f9a3");
        }

        private static void Case_01313()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1313,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,14,27,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,7,25,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-5,73,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-7,11,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,17,16,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-13,36,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-14,23,25,1), new GeneratedEnemyUnit(11,8,10,24,3), new GeneratedEnemyUnit(3,-10,14,42,3), new GeneratedEnemyUnit(15,-17,57,50,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "57a288cc0605e3f9e034050f0bf3d227cd396cfb641057e11e82524eba427d2e");
        }

        private static void Case_01314()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1314,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-11,49,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,11,8,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-7,81,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-20,55,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-10,80,25,4), new GeneratedEnemyUnit(4,4,78,4,1), new GeneratedEnemyUnit(-8,12,45,49,2), new GeneratedEnemyUnit(0,4,19,7,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "bb5e916209d7a0710b3ebe66bce4a234e26fa96291b238cf84657d589585e885");
        }

        private static void Case_01315()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1315,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,6,72,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-10,18,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-7,50,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-5,90,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,13,38,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,2,87,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,12,9,14,1), new GeneratedEnemyUnit(16,-6,53,1,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "93a58f005ee02276f2ea5a8d381000dfac5727864469736e60af380dfaacd817");
        }

        private static void Case_01316()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1316,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,4,70,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,10,95,25,4), new GeneratedEnemyUnit(-13,1,19,8,3), new GeneratedEnemyUnit(-12,1,67,26,1), new GeneratedEnemyUnit(-7,3,17,50,2), new GeneratedEnemyUnit(15,16,84,46,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "c31a6d92f49c75ca165322340c38d9dcac228382d2175c85ee14470029f56339");
        }

        private static void Case_01317()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1317,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,15,99,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,2,40,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-7,21,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-19,63,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-12,99,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,20,94,8,1), new GeneratedEnemyUnit(11,-17,9,3,4), new GeneratedEnemyUnit(-13,-19,77,43,2), new GeneratedEnemyUnit(6,8,54,11,3), new GeneratedEnemyUnit(5,15,49,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "18cb979e5fd5b763fa0708665f59c6242de77724f926fbeafe69ed83c9737965");
        }

        private static void Case_01318()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1318,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-14,59,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,13,85,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,7,75,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-8,83,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-15,25,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-1,29,32,2), new GeneratedEnemyUnit(19,-6,43,30,3), new GeneratedEnemyUnit(8,-17,90,42,2), new GeneratedEnemyUnit(19,-14,25,34,3), new GeneratedEnemyUnit(3,4,95,28,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "6786de907d42426268a354682f6144efa2fc3908aafc384b8abb63c69f0c5c37");
        }

        private static void Case_01319()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1319,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,19,23,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,9,90,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-12,80,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-1,84,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,8,8,30,1), new GeneratedEnemyUnit(15,-16,6,26,4), new GeneratedEnemyUnit(17,-15,98,49,3), new GeneratedEnemyUnit(4,-9,42,12,1), new GeneratedEnemyUnit(19,-7,52,23,4), new GeneratedEnemyUnit(-8,3,27,33,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "fe07fa6250944c5597964b85842e7e65569c917408f5254345408d725168ff14");
        }

        private static void Case_01320()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1320,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,4,66,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,15,75,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,18,25,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-18,74,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-10,35,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,2,29,17,1), new GeneratedEnemyUnit(-13,-8,95,24,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6abc4ec7c5e26d0db39c413fb54ded94df6b1a5eafe211d276d20bd1d49fab0b");
        }

        private static void Case_01321()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1321,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,14,67,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,18,98,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-9,68,11,2), new GeneratedEnemyUnit(-5,-14,100,36,1), new GeneratedEnemyUnit(14,5,78,41,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "e62db2d55e7425b6f7b0546adee5127ea1a125bc6244251ab8dfd519883850f7");
        }

        private static void Case_01322()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1322,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,0,17,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,20,53,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-16,55,6,4), new GeneratedEnemyUnit(3,-9,38,13,2), new GeneratedEnemyUnit(19,0,81,19,2), new GeneratedEnemyUnit(-15,12,49,9,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "e6615823b3acf65b8de704a07b00458819a6febe2ed7f1457a676fe04b897441");
        }

        private static void Case_01323()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1323,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-17,88,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,8,20,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-3,84,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,12,67,24,3), new GeneratedEnemyUnit(-9,-10,77,9,3), new GeneratedEnemyUnit(-8,8,16,38,4), new GeneratedEnemyUnit(6,17,67,40,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "50fd84b7aae9cfc5bf0e7cc3dd0c1176047b3c6644c89a4c6c06fb21b2718d31");
        }

        private static void Case_01324()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1324,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,6,20,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,3,63,43,2), new GeneratedEnemyUnit(-11,-10,95,14,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "5a15fc18ab814fabef117ea18fe339be2d808c1f650dd019ae655100cb4c5a6b");
        }

        private static void Case_01325()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1325,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,15,77,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,8,17,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-8,100,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,12,30,1,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "984c29d580c58143e9addcc895a059b270df808f10ea0c59f846d59411e75816");
        }

        private static void Case_01326()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1326,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,16,34,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-15,47,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,12,13,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-3,33,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-4,42,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,13,85,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-1,26,22,3), new GeneratedEnemyUnit(19,4,16,40,2), new GeneratedEnemyUnit(9,-13,33,20,3), new GeneratedEnemyUnit(-18,-17,61,40,3), new GeneratedEnemyUnit(4,5,91,15,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "1d32e49d386d6250fd2ba1c60ab91f3a9684319b80f705e7828184cb9a37a166");
        }

        private static void Case_01327()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1327,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,4,26,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,18,54,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-18,13,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,14,37,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,9,11,49,4), new GeneratedEnemyUnit(12,-9,75,36,2), new GeneratedEnemyUnit(2,11,26,35,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ed06be937990cac865115e1e89059d51d4fa0e5bc7af485db6293236d1f8b0ae");
        }

        private static void Case_01328()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1328,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-15,64,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,1,46,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,18,53,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,16,21,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,1,5,29,3), new GeneratedEnemyUnit(5,-10,11,33,4), new GeneratedEnemyUnit(-6,-1,52,37,4), new GeneratedEnemyUnit(12,-2,37,5,2), new GeneratedEnemyUnit(-14,-17,36,4,4), new GeneratedEnemyUnit(14,8,29,12,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "e87f252d995de082df46c3855bfa578bb773a9bb7d8b8bf044dee35c78f87216");
        }

        private static void Case_01329()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1329,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-12,31,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,1,38,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,10,46,21,4), new GeneratedEnemyUnit(4,-8,67,17,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "082abb144ef0dd56adfefc186d1a97c30602f2f25b135d3f6dd7d135bb579ff7");
        }

        private static void Case_01330()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1330,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-14,32,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-14,46,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,5,28,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,0,68,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,1,22,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,12,100,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-14,95,30,1), new GeneratedEnemyUnit(6,-12,94,19,1), new GeneratedEnemyUnit(16,10,33,5,2), new GeneratedEnemyUnit(20,-10,75,25,2), new GeneratedEnemyUnit(1,5,85,24,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "762897fdca2336ede280f30ce3259a84e4156f343f447968291b57b7a78da496");
        }

        private static void Case_01331()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1331,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,9,62,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,0,23,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-11,79,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-4,72,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,17,90,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,9,95,37,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "4aae348ba1b4a1eae93aba2f6939a5a1ba01d0ddc1071c6bb957c15c372857d3");
        }

        private static void Case_01332()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1332,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,12,31,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-15,42,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-19,24,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,16,91,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-1,25,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,15,48,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,6,19,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-18,66,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,8,75,40,4), new GeneratedEnemyUnit(-13,-6,79,36,2), new GeneratedEnemyUnit(-19,1,67,24,2), new GeneratedEnemyUnit(-13,-2,66,25,1), new GeneratedEnemyUnit(5,12,34,28,1), new GeneratedEnemyUnit(-11,-16,21,16,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "bd6c1a334d0ece370137ca0d3552faf1f8e36715f61c8b1aebfef71cc685c34b");
        }

        private static void Case_01333()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1333,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-11,19,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-17,81,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,7,80,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,1,78,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,14,39,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,15,53,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-1,31,4,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "2f75b7659f3f3abb62e49f084ab70621e4ba1435c7fef8151f3a7cc354424a3c");
        }

        private static void Case_01334()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1334,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,10,72,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,19,6,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-20,70,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,12,93,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-13,48,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,7,62,5,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "80c54169bb0ed35c16bc4a76b46611d962df6cdc9868d37998b4070f8ef13a07");
        }

        private static void Case_01335()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1335,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-13,58,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-1,66,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,14,54,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-11,10,21,3), new GeneratedEnemyUnit(-7,4,16,25,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "73ddeb97ef27eae958e2508deb7c759a1b4a00f02d167033e841c23679943165");
        }

        private static void Case_01336()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1336,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-12,88,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,7,34,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,18,82,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-9,54,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,9,89,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-12,16,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,17,58,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-4,8,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-5,50,11,4), new GeneratedEnemyUnit(-13,-18,50,34,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "7a27537a9a78136da16f7c713558654e274458cbae324dc977e8b599ec6771f4");
        }

        private static void Case_01337()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1337,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-18,64,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-1,94,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,12,27,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-2,8,23,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "406c27959c65964950d21edbdf4f0e986d964969584957c22c17fc1fd98cfb74");
        }

        private static void Case_01338()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1338,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,19,31,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-16,79,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-10,45,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,5,79,39,4), new GeneratedEnemyUnit(-14,2,68,4,4), new GeneratedEnemyUnit(-8,-7,64,10,1), new GeneratedEnemyUnit(-16,-18,87,49,1), new GeneratedEnemyUnit(17,-1,41,35,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "d9977efb62f45803c697e9b8af68e9c425a04cce26962cbbdd84d38bb3d89007");
        }

        private static void Case_01339()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1339,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,2,40,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,15,34,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,12,15,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-20,77,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,5,92,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,13,83,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,1,89,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,5,58,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-19,61,4,1), new GeneratedEnemyUnit(-19,-12,92,14,3), new GeneratedEnemyUnit(11,7,37,4,2), new GeneratedEnemyUnit(18,15,61,23,4), new GeneratedEnemyUnit(0,16,15,2,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "49b010c62db975cd24db7e18639b90fff77486fd0e5df00b863a92aa37baa776");
        }

        private static void Case_01340()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1340,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-7,43,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-8,43,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-6,27,26,3), new GeneratedEnemyUnit(13,-20,37,49,4), new GeneratedEnemyUnit(12,-14,83,45,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "5fd140ff4f8d4366c5632ea279750adc092f93b6cc5f1f2549682e4f1efb7eeb");
        }

        private static void Case_01341()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1341,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-20,49,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-1,68,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,13,87,4,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "7ee419c2e2d697172a9e226383ef5857abc06debffea28c06058ad11406404ae");
        }

        private static void Case_01342()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1342,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-5,100,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-4,6,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,12,70,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-12,21,20,2), new GeneratedEnemyUnit(6,-16,64,24,2), new GeneratedEnemyUnit(12,4,12,9,3), new GeneratedEnemyUnit(2,-17,62,50,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "b11b4280d0907d024c5bdb27188390308324f4cea2ae7e98ff607137300e43c8");
        }

        private static void Case_01343()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1343,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-9,73,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,19,42,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,15,79,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-14,18,44,2), new GeneratedEnemyUnit(12,3,40,39,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "8bf16f80a6117363d429a1217ec221a8989308f0f3378330d90b2d9e8e82d8e2");
        }

        private static void Case_01344()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1344,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,2,31,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-4,23,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,14,68,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-8,36,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,17,78,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-6,25,5,2), new GeneratedEnemyUnit(1,-18,66,35,1), new GeneratedEnemyUnit(16,11,30,26,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "a2a4b3a3873976623cd8ec2f4fca558b17a1d01a2f8bf25eede7489c78fb97ab");
        }

        private static void Case_01345()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1345,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-10,99,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,18,67,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-12,70,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,3,71,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-15,96,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,17,93,36,4), new GeneratedEnemyUnit(-4,-5,28,27,1), new GeneratedEnemyUnit(0,10,50,37,4), new GeneratedEnemyUnit(17,-15,91,3,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "b8647b43af05dc32a3be02543b211ea428771e4089602fb9597b52dddd4b40e6");
        }

        private static void Case_01346()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1346,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,13,94,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,12,39,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,12,83,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,18,66,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-18,97,15,2), new GeneratedEnemyUnit(-6,3,31,5,1), new GeneratedEnemyUnit(-12,0,7,32,2), new GeneratedEnemyUnit(-1,-16,62,12,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "e718d8bcba3add33653b2bad64a15e642dec03711ea367ecfa7882b6a64a62ce");
        }

        private static void Case_01347()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1347,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,5,31,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,4,28,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-19,42,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,10,77,37,1), new GeneratedEnemyUnit(9,16,55,4,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "478269388656d58ba4adc22bc16b3a12e82400da638b101368ad57d994f62f0a");
        }

        private static void Case_01348()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1348,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,20,62,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,2,46,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,14,74,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,16,56,1,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "54f7fa5d1a3ddddbd194b991ee975ff0a7ec4b3bcacb29587ed4805e827f4fb2");
        }

        private static void Case_01349()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1349,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,18,60,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,13,16,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,12,38,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-2,29,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-11,71,35,1), new GeneratedEnemyUnit(-14,14,20,10,2), new GeneratedEnemyUnit(10,-11,93,44,2), new GeneratedEnemyUnit(-12,6,58,40,2), new GeneratedEnemyUnit(17,-11,25,39,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "b7c5dad94444b50c764aecdf971bf40df1235a1d03b66c5466cc92465919f6a6");
        }

        private static void Case_01350()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1350,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,6,8,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-7,10,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,17,48,4,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "655afab0802ed573ba30ba026b5821acc6d824d7258769663aad688db9dfc22f");
        }

        private static void Case_01351()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1351,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-5,22,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,18,63,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,17,24,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-10,48,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,19,54,29,1), new GeneratedEnemyUnit(-19,-12,88,11,4), new GeneratedEnemyUnit(2,-14,13,1,4), new GeneratedEnemyUnit(3,-10,86,23,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "fc816d3ef0dc5b532f6b69493beac90648b091897a356de5a90fe8b54e8dd04f");
        }

        private static void Case_01352()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1352,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,9,91,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-12,63,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,17,13,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,12,49,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,16,49,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,8,43,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-10,79,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,7,29,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-17,74,21,4), new GeneratedEnemyUnit(14,5,40,31,1), new GeneratedEnemyUnit(-15,7,53,33,3), new GeneratedEnemyUnit(-11,-18,67,4,4), new GeneratedEnemyUnit(20,-4,57,25,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "fa9cfa269c59e7820b4491ff38e379f27eb7b398abdc507f9b9a36f852417f60");
        }

        private static void Case_01353()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1353,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-20,69,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,11,88,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-15,70,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-18,61,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,1,74,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-4,53,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-2,51,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-2,86,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,19,35,9,2), new GeneratedEnemyUnit(-10,15,49,15,3), new GeneratedEnemyUnit(2,-2,70,48,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "f56ad90d194ebebf2e05dbd6915b55c1c4c89d0e7e13145a91f3b945d8f53c61");
        }

        private static void Case_01354()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1354,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,18,99,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,12,84,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-2,26,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-10,60,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-18,66,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,9,32,4,3), new GeneratedEnemyUnit(7,20,60,20,3), new GeneratedEnemyUnit(-2,-18,59,34,2), new GeneratedEnemyUnit(19,-12,88,1,3), new GeneratedEnemyUnit(-9,2,43,27,4), new GeneratedEnemyUnit(-1,-17,21,49,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "3a6538ffaf34d957c12a7d08021bcf53b73f54130de8bb5ff3ecbba338e2dd36");
        }

        private static void Case_01355()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1355,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-8,55,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-12,29,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,3,65,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,6,31,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,15,48,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-15,28,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,18,79,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,20,47,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,11,22,24,1), new GeneratedEnemyUnit(13,8,82,38,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "5fc9e5b1232508f122a631cdb84f517962617ffed962d4987f99766bea5d7047");
        }

        private static void Case_01356()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1356,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-20,82,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,1,53,4,3), new GeneratedEnemyUnit(-7,13,92,35,3), new GeneratedEnemyUnit(7,-11,84,8,3), new GeneratedEnemyUnit(10,16,74,44,3), new GeneratedEnemyUnit(-19,1,31,38,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "312183ef17399aa428dddc4762097cabb5824ff3fe31e8e3764b14800976c22f");
        }

        private static void Case_01357()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1357,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-6,45,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-9,36,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,15,68,18,3), new GeneratedEnemyUnit(17,-4,58,6,4), new GeneratedEnemyUnit(8,16,99,25,1), new GeneratedEnemyUnit(14,0,50,1,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "1c7e40acad7eb0a96eaa1c363ef643d23459a3d67185f2396457765eb7047215");
        }

        private static void Case_01358()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1358,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-18,64,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-8,21,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,5,22,6,3), new GeneratedEnemyUnit(0,8,31,15,2), new GeneratedEnemyUnit(15,-17,23,12,2), new GeneratedEnemyUnit(-3,13,21,19,4), new GeneratedEnemyUnit(2,20,82,10,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "77a40627f68aefcbbfc3349ed3cbb234dea3e716199110cace0d304f091db0b9");
        }

        private static void Case_01359()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1359,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-2,57,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-14,9,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,18,22,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,1,46,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,5,44,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,18,91,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,9,100,4,2), new GeneratedEnemyUnit(-6,-18,5,28,1), new GeneratedEnemyUnit(15,-4,79,10,4), new GeneratedEnemyUnit(-14,-14,17,29,2), new GeneratedEnemyUnit(18,-8,83,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "0644e1bf4735fdd5c5b8f512cbea170cb4552fa11e256c3f0775fc86388e495f");
        }

        private static void Case_01360()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1360,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-9,70,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,18,11,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-9,97,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-20,54,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-13,59,12,2), new GeneratedEnemyUnit(-18,1,38,42,3), new GeneratedEnemyUnit(-15,-19,25,23,2), new GeneratedEnemyUnit(-19,15,92,49,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "71cc89a1289da38719814f78d55ef5e0f8bfca4a362715f11dbd7395928e3061");
        }

        private static void Case_01361()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1361,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,14,73,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-15,53,16,4), new GeneratedEnemyUnit(18,-8,92,41,3), new GeneratedEnemyUnit(-6,15,22,47,3), new GeneratedEnemyUnit(14,12,30,10,2), new GeneratedEnemyUnit(-16,4,91,27,4), new GeneratedEnemyUnit(6,20,84,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "816cfcb321cd4998e6e0f34a9eb3852071683249d2d2e05b9360a8edf4a5631a");
        }

        private static void Case_01362()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1362,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-15,70,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-4,37,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-9,89,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-3,26,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,11,69,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-19,11,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-17,96,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,18,64,17,3), new GeneratedEnemyUnit(2,10,83,43,3), new GeneratedEnemyUnit(5,-12,79,49,2), new GeneratedEnemyUnit(16,-6,95,30,2), new GeneratedEnemyUnit(17,14,20,38,1), new GeneratedEnemyUnit(11,-14,75,49,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "193abfe921ebb55199014ebe60c87f956b44af3f6c9ed0049faa96d82678a69b");
        }

        private static void Case_01363()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1363,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,4,60,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-15,6,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,4,71,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,12,43,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,14,10,39,3), new GeneratedEnemyUnit(-14,-20,7,2,3), new GeneratedEnemyUnit(-18,13,69,31,4), new GeneratedEnemyUnit(9,-6,96,32,2), new GeneratedEnemyUnit(0,-1,24,38,4), new GeneratedEnemyUnit(-1,19,61,5,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "93782cb0f4120511fa104a9cc3fc10500a6a119bb4ec28f094da6b2e9fa3c392");
        }

        private static void Case_01364()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1364,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,20,19,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-3,29,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,18,21,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,3,36,8,3), new GeneratedEnemyUnit(20,-8,15,25,3), new GeneratedEnemyUnit(19,-20,40,5,3), new GeneratedEnemyUnit(-8,9,82,15,4), new GeneratedEnemyUnit(17,-3,75,9,4), new GeneratedEnemyUnit(9,-5,97,48,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "05a9feffc817412b9fdde80b69efdcf592d1c2c42d6ea52fdbcbdac7211c0b4c");
        }

        private static void Case_01365()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1365,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-10,69,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-2,80,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-6,88,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-16,15,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-1,68,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-3,63,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,16,35,50,2), new GeneratedEnemyUnit(-3,3,44,50,4), new GeneratedEnemyUnit(7,13,74,22,3), new GeneratedEnemyUnit(-6,-6,5,1,4), new GeneratedEnemyUnit(-13,-19,38,21,1), new GeneratedEnemyUnit(9,-4,16,27,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "a8bcf32f5f7e756c486fd7ad91b19f5ca39d89aab0989a67cbadfbbe7e951ea0");
        }

        private static void Case_01366()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1366,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-3,49,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-9,63,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-6,66,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-7,82,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-9,87,3,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "a9d9159d224442a9e7b7a46c40d995fbfc79ab2a49c59a58ed5eaac0db9ed0f5");
        }

        private static void Case_01367()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1367,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,14,80,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,19,17,19,3), new GeneratedEnemyUnit(5,-18,7,41,3), new GeneratedEnemyUnit(3,11,27,12,1), new GeneratedEnemyUnit(-11,13,48,16,4), new GeneratedEnemyUnit(-9,-18,40,15,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "e29c039e69d1b770cf503fb5a88e35d84b56827949cd74809e62a45614e1d76c");
        }

        private static void Case_01368()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1368,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-11,49,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-9,37,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,16,11,44,2), new GeneratedEnemyUnit(-16,7,9,16,4), new GeneratedEnemyUnit(14,2,95,25,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a3d741585f7928fd83b8386195d92453bc7c09fbf299a74415b8a8d345ec233a");
        }

        private static void Case_01369()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1369,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-11,62,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-11,26,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,15,93,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,16,41,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-19,5,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,4,91,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-16,80,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-6,84,33,4), new GeneratedEnemyUnit(-15,-9,14,12,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "e130ec7bc92ac7d01c7d813493b966d2ee5b57c16004253cea2d35fab27cbd80");
        }

        private static void Case_01370()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1370,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,2,35,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-15,96,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,1,35,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-6,69,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,1,35,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,4,7,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,15,52,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-19,12,45,4), new GeneratedEnemyUnit(-9,7,53,37,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "d6dcd6f337fa744d633c80148ca071b81ac91500704271e5736c3867568ffcec");
        }

        private static void Case_01371()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1371,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,16,9,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-20,31,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,9,5,29,4), new GeneratedEnemyUnit(-5,2,97,20,3), new GeneratedEnemyUnit(0,-17,27,40,1), new GeneratedEnemyUnit(13,15,79,6,4), new GeneratedEnemyUnit(20,14,38,31,4), new GeneratedEnemyUnit(13,-1,44,8,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "9c4da051e3fea91a7792d34e96e6565d2a0bd4aa1d1b269cd049a45ca2d7d2b0");
        }

        private static void Case_01372()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1372,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,15,25,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-2,60,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-10,42,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,10,80,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,0,86,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,9,74,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,20,6,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,16,6,8,3), new GeneratedEnemyUnit(-11,-8,46,46,3), new GeneratedEnemyUnit(-1,-1,12,13,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "d9b212c79601682cfa134701add0a3fc33dafed429a7b250f72a0d58ad6e6ba4");
        }

        private static void Case_01373()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1373,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,20,70,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-1,63,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,9,82,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-13,37,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,20,28,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,17,97,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,1,40,1,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "705fa65c71c5638157cbb4b2162e45bcef9c76f96e4c8a69086bfe4445c6a458");
        }

        private static void Case_01374()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1374,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,1,41,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,0,7,43,1), new GeneratedEnemyUnit(-9,11,52,14,3), new GeneratedEnemyUnit(1,-8,17,43,4), new GeneratedEnemyUnit(-15,-6,28,32,4), new GeneratedEnemyUnit(15,14,34,33,3), new GeneratedEnemyUnit(-14,17,56,18,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "f917b338047d5a0b577381257001f0ea2502b8a6f8638db4f9318bbe2b07d693");
        }

        private static void Case_01375()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1375,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-8,99,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-20,92,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-11,53,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,16,73,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-19,48,6,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ec2e6162c654cbc06e725e8af8d763b254ca045b79f20ae9fac2480dfb9788a3");
        }

        private static void Case_01376()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1376,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,11,60,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-15,24,4,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "6097ff2c9ba9cbd3ca58bdedbd3fca02ead466c4a9b12f0cdc1d538ff6e1e90e");
        }

        private static void Case_01377()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1377,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,20,95,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,19,24,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,19,51,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-11,79,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,7,24,34,2), new GeneratedEnemyUnit(12,2,55,41,1), new GeneratedEnemyUnit(-5,18,40,29,1), new GeneratedEnemyUnit(-18,0,77,23,3), new GeneratedEnemyUnit(8,13,58,40,3), new GeneratedEnemyUnit(6,2,90,36,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "39b3777a6395b04eb6b501245c88ce820724bc908e5039ff984fe469e36f108b");
        }

        private static void Case_01378()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1378,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,11,17,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,3,91,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-6,95,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,17,33,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-4,68,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-16,15,4,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "77346610243787a066a15514cfd3e08a9bc372e3fb92567db41c403661eeb46f");
        }

        private static void Case_01379()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1379,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,14,23,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-3,51,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-9,8,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,7,85,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,19,19,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-17,16,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,8,72,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-4,95,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,17,7,12,3), new GeneratedEnemyUnit(-18,3,16,28,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "4e74a1c8bba46484e9571fb6a1966a60add565a3703b640aac12f014eeeaf7fa");
        }

        private static void Case_01380()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1380,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,19,12,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,11,31,21,1), new GeneratedEnemyUnit(19,19,11,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "ec99ede2fd95aa20423e3a002627cdad8f6a77eebe1981431f5567e4073a8ee8");
        }

        private static void Case_01381()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1381,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,15,24,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,19,56,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,20,8,47,1), new GeneratedEnemyUnit(-14,8,70,10,3), new GeneratedEnemyUnit(2,-19,35,2,4), new GeneratedEnemyUnit(12,20,53,5,1), new GeneratedEnemyUnit(-17,18,16,31,3), new GeneratedEnemyUnit(-5,-7,90,43,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e080c2fd52fa8eea91ff85ce8d7bbf1408b8bad5ca860569c6e1e2234f69f9e9");
        }

        private static void Case_01382()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1382,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-2,20,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,2,86,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,8,85,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,2,98,21,3), new GeneratedEnemyUnit(7,6,81,20,4), new GeneratedEnemyUnit(-17,6,31,26,4), new GeneratedEnemyUnit(1,4,10,7,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "abbd460bbb32112156a2582f33d39867b54de6a800e1d2ebdbe84c758b465795");
        }

        private static void Case_01383()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1383,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,0,59,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,2,70,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,16,71,44,1), new GeneratedEnemyUnit(-7,-16,43,43,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "3cd72e180bdd38f7b60cf9c80c7b46e0fa53384ac6ed06332913f6301a1be4f1");
        }

        private static void Case_01384()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1384,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,19,14,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,0,41,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,5,82,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,4,81,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-19,95,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,5,48,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-11,92,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-4,60,4,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "f3b4b9cf26c85cde1d3cb3080c266f73de0855f7ceda2474505eb67cea794d8f");
        }

        private static void Case_01385()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1385,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-2,63,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-4,52,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,15,88,29,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e17ca945532e37cfd0a969811ed6c16e5bdc95e4923a5e6939646fb6ba002e0c");
        }

        private static void Case_01386()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1386,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,19,76,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-9,69,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,3,87,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,16,53,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,13,66,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-14,65,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-14,59,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,10,17,6,2), new GeneratedEnemyUnit(3,-11,5,50,4), new GeneratedEnemyUnit(0,10,26,43,1), new GeneratedEnemyUnit(4,-19,89,38,2), new GeneratedEnemyUnit(19,-14,79,30,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "dda47a16a9a51df04d7c1b6b2a588f9b5db34c2dd35cec6b1a9d6fe6e9b7fa76");
        }

        private static void Case_01387()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1387,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,10,78,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,20,82,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-9,6,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-3,95,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,0,24,5,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "25bd5a8d92fae2d7659513153f4f307a978711a45c1bfd7a8134225028f22df5");
        }

        private static void Case_01388()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1388,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-10,40,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-15,93,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,10,27,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,6,24,2,3), new GeneratedEnemyUnit(-16,19,37,35,1), new GeneratedEnemyUnit(13,15,99,22,2), new GeneratedEnemyUnit(18,16,84,31,1), new GeneratedEnemyUnit(-8,7,65,29,1), new GeneratedEnemyUnit(-12,-9,11,43,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "5a0826a905e9249b7b0e9a0f8de77ac4a7edfddb92bb82aa4a15032b21e8578f");
        }

        private static void Case_01389()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1389,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,18,76,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-14,35,45,4), new GeneratedEnemyUnit(5,-12,18,8,2), new GeneratedEnemyUnit(13,-19,5,11,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "5c1d91ea10a19037463a5d3e63da37a346e54fbc36e1e2ec13e03e6c47ab0945");
        }

        private static void Case_01390()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1390,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,2,68,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-5,88,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-18,67,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-12,33,24,1), new GeneratedEnemyUnit(-20,19,60,50,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "547c80fd6939e77364b637ad965b247578dc1982f4def6c22cf434130f400b3b");
        }

        private static void Case_01391()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1391,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,6,86,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,7,78,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-6,13,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-7,98,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,13,71,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-14,21,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,16,24,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-14,25,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-18,70,48,2), new GeneratedEnemyUnit(20,6,25,17,4), new GeneratedEnemyUnit(-19,-17,14,39,1), new GeneratedEnemyUnit(-5,-17,30,44,2), new GeneratedEnemyUnit(11,-8,56,11,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "95ce884470c7b2b5af1237f116efdb9d57f950e6615e692d201aadea4106e551");
        }

        private static void Case_01392()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1392,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-19,92,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,18,82,42,4), new GeneratedEnemyUnit(-20,18,22,27,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "94f5b5c6826336d9b5026587dbb19e6c4d9a71ba9f0091316e47a7db1f3cd702");
        }

        private static void Case_01393()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1393,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-8,67,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-6,39,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,17,33,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-12,18,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,18,40,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-5,27,33,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6649e0496b6a1ecc489fa54c064aa9d9703c8f8c0f9745b012a8ac1a0cbb9d73");
        }

        private static void Case_01394()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1394,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,3,86,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-3,87,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-5,7,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-18,41,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-19,67,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,15,8,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-19,82,49,4), new GeneratedEnemyUnit(-11,2,44,37,1), new GeneratedEnemyUnit(18,0,50,4,2), new GeneratedEnemyUnit(-16,3,72,13,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "4809092884817f35d5b921c12687b075199d4df034452b2a4a2797e5675d4748");
        }

        private static void Case_01395()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1395,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-16,38,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-2,58,40,2), new GeneratedEnemyUnit(-2,10,79,21,1), new GeneratedEnemyUnit(9,-12,20,18,3), new GeneratedEnemyUnit(-2,3,79,29,1), new GeneratedEnemyUnit(-19,19,68,43,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "f0ad702bb423a1bf3513f3b0af46f52362f60bde694ab798d933dc8fe01ae4ae");
        }

        private static void Case_01396()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1396,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,9,8,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,5,39,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,18,12,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,1,22,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,16,29,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,10,76,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,19,99,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-9,44,50,1), new GeneratedEnemyUnit(-8,-17,94,10,1), new GeneratedEnemyUnit(13,-5,53,24,1), new GeneratedEnemyUnit(12,1,70,18,2), new GeneratedEnemyUnit(18,-10,90,12,2), new GeneratedEnemyUnit(-3,-12,38,9,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "7708cf9bbb2f95e947b1c964ccde54530ff306328c1d65d2e8358f9b6e1158ac");
        }

        private static void Case_01397()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1397,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-8,9,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-5,15,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-19,54,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-4,21,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-20,97,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,18,86,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-3,43,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-4,21,35,4), new GeneratedEnemyUnit(5,8,19,32,4), new GeneratedEnemyUnit(20,19,60,10,4), new GeneratedEnemyUnit(-6,4,18,4,1), new GeneratedEnemyUnit(4,-9,24,37,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "ae1713d0b044a9d07c60f63afe8f8555e0c06393691c61446c98711944cd6dff");
        }

        private static void Case_01398()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1398,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,16,79,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-11,64,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-11,29,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-7,27,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,15,17,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-17,30,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-2,7,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-3,80,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-7,58,36,4), new GeneratedEnemyUnit(13,2,82,47,4), new GeneratedEnemyUnit(-6,5,81,9,3), new GeneratedEnemyUnit(15,1,75,22,1), new GeneratedEnemyUnit(5,9,87,24,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "7666bdf31f22d65788994820a7646898d69757ed7a0337f692c8a61a4e710d42");
        }

        private static void Case_01399()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1399,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,4,52,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,1,20,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,1,98,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-6,61,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,5,19,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,10,58,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,7,84,45,1), new GeneratedEnemyUnit(-14,-17,16,47,4), new GeneratedEnemyUnit(19,-20,53,10,2), new GeneratedEnemyUnit(-3,17,50,47,4), new GeneratedEnemyUnit(-10,16,86,10,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "75aee8108c49a6fac63e994142951d2c04185235ece57f23ed5546a9d77d7c1d");
        }

        private static void Case_01400()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1400,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-5,95,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-1,73,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,12,80,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-8,98,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-11,62,31,2), new GeneratedEnemyUnit(-9,3,32,9,1), new GeneratedEnemyUnit(12,-6,86,10,3), new GeneratedEnemyUnit(-1,15,30,6,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "46409870c8ca9959dbd02e1de40a61faa33da0fe2520cf11e1543714134a6fd1");
        }

        private static void Case_01401()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1401,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,6,15,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-12,65,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,20,34,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,18,80,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-14,21,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-13,95,27,2), new GeneratedEnemyUnit(-19,16,52,48,3), new GeneratedEnemyUnit(-4,10,54,16,3), new GeneratedEnemyUnit(7,-17,57,32,3), new GeneratedEnemyUnit(7,18,5,16,3), new GeneratedEnemyUnit(-5,9,84,20,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "d5ccc4f3ee6bb1ca8c6af71f79be2050fd3ec15e29550ecf0f4f0023578871dc");
        }

        private static void Case_01402()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1402,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-12,39,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,18,85,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-3,63,8,4), new GeneratedEnemyUnit(-10,12,72,26,4), new GeneratedEnemyUnit(-10,-2,34,36,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "a4c3e8478f6cee23dc34cc1d4b8b57aa66d12d0070824de724d31f8d202aab4b");
        }

        private static void Case_01403()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1403,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,11,33,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,14,73,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,13,8,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-11,10,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,0,87,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,4,54,5,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3494e082ab15063602f03b041a02173496fa970f851981983dc9c89b7933c915");
        }

        private static void Case_01404()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1404,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,20,81,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,17,18,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,15,75,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-15,67,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,19,24,16,4), new GeneratedEnemyUnit(-16,-14,79,4,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a2073fedde849569a80e32f21cb4760bfeed4e0208e65ef124b717ae7dd6c6fe");
        }

        private static void Case_01405()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1405,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,5,15,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-6,16,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-1,44,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-7,86,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,10,80,13,3), new GeneratedEnemyUnit(-4,-16,98,32,4), new GeneratedEnemyUnit(1,-11,78,29,3), new GeneratedEnemyUnit(3,-6,38,50,2), new GeneratedEnemyUnit(17,13,14,15,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "fc0d4a93b74143ae0ec72dd8f77589851eb49ced780ba2d299339e6cf5b7348f");
        }

        private static void Case_01406()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1406,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-13,76,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-17,24,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-17,36,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,4,80,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-7,80,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,13,13,32,2), new GeneratedEnemyUnit(3,4,51,5,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "cd2502b3f88a229560dc698fce3961d6298f85da614f977e1363cc0210a3a8f9");
        }

        private static void Case_01407()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1407,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,5,9,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,2,7,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,11,44,2,3), new GeneratedEnemyUnit(-1,6,83,3,4), new GeneratedEnemyUnit(0,15,8,20,3), new GeneratedEnemyUnit(-19,-19,66,15,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "6b6a45f7ef1c102b8b8f33fe617774c7e1b5420507ad33e65643d6b8e8f36932");
        }

        private static void Case_01408()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1408,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-8,60,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,0,51,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,0,58,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "235127d8538151435f5201bdf6ad631272b954ce030b96339b311eb1bfcb292b");
        }

        private static void Case_01409()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1409,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-5,95,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,4,31,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-6,29,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,14,15,39,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0d9cd972769a02bc92f11c5d6ae35c7be26ace10be4b83c2360d786eba1bc587");
        }

        private static void Case_01410()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1410,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,14,83,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-20,86,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,15,75,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,1,92,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,11,36,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,8,48,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,13,13,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-10,78,30,4), new GeneratedEnemyUnit(7,14,60,3,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "5f191cc69e833d140cd71e9546e01d7d8a4737664c678c802bf81954580867b0");
        }

        private static void Case_01411()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1411,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,5,31,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-8,61,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,7,37,48,2), new GeneratedEnemyUnit(14,15,22,23,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "c23e2f68b3d843dd6c0527343f51f35ae16d7081a5a9c04100644ce9e6278f3f");
        }

        private static void Case_01412()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1412,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-5,15,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-11,53,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,0,70,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-10,32,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-13,22,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-2,66,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,8,98,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-14,99,28,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "6e095265d6c2301330c73d1a4e4272b1acde28636908718254514308ee3cba85");
        }

        private static void Case_01413()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1413,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,5,82,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,15,13,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-2,51,3,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "22106533de21a33363ed6752c86714f6b5a99ad347b15cddb8f025514465a800");
        }

        private static void Case_01414()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1414,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,0,82,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,8,53,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,18,67,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,3,96,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,7,96,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,17,25,1,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3422858a92a521c0dae056dccac4414634980f58cca17aa125cfd26d47469741");
        }

        private static void Case_01415()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1415,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,13,97,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,14,77,15,3), new GeneratedEnemyUnit(-17,9,41,10,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "14aa198cf74205ca7bee6053fdafa8fb68512057b5228a87deec1d9cee499d35");
        }

        private static void Case_01416()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1416,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-1,99,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-1,53,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-19,16,23,4), new GeneratedEnemyUnit(14,-2,46,28,2), new GeneratedEnemyUnit(6,14,16,3,1), new GeneratedEnemyUnit(8,-17,41,49,3), new GeneratedEnemyUnit(9,-11,11,15,3), new GeneratedEnemyUnit(20,-9,87,50,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "f8e61764a8581a436bcdc1719c585b2ccd7e0d9d9a3e827b4d1a9253fdd47be9");
        }

        private static void Case_01417()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1417,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,8,65,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-1,7,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-6,68,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-17,85,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,9,87,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,0,76,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,7,44,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-13,85,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "566129e522b9d479db5b3cfd2ace3c4cd5a7985ec70458ebb2de74d17bed1776");
        }

        private static void Case_01418()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1418,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,16,7,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-3,38,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,8,35,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,12,13,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-6,57,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,20,60,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,18,27,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-9,69,16,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "7f7d4d669d475ab1b4487e58daf15fe0c35780b35efb7be26c616ac603ea40d3");
        }

        private static void Case_01419()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1419,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-20,39,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,6,32,10,2), new GeneratedEnemyUnit(20,-19,47,31,1), new GeneratedEnemyUnit(8,16,54,44,1), new GeneratedEnemyUnit(8,-13,45,16,4), new GeneratedEnemyUnit(0,-13,72,29,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "e969d6ae249937f34d5259794c1b08bf4da970b6293accd9880a55761bf12774");
        }

        private static void Case_01420()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1420,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-11,79,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-3,76,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-18,96,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,1,31,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,2,95,43,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "30b604475efebbb8437dede78bec99e1b6b24842baf56339c0025c352013164a");
        }

        private static void Case_01421()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1421,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,15,75,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,9,94,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-17,81,23,2), new GeneratedEnemyUnit(-9,14,15,37,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "53c4809272df9ff49fbd7a4f4108ab5c7a849059df64d50ee41b7f0fe053fa42");
        }

        private static void Case_01422()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1422,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-6,33,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-8,66,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-20,25,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,9,90,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-3,33,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-17,97,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,13,84,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-13,37,26,4), new GeneratedEnemyUnit(20,2,12,39,4), new GeneratedEnemyUnit(13,15,99,28,4), new GeneratedEnemyUnit(-5,-20,15,38,4), new GeneratedEnemyUnit(16,-4,97,30,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "b66bd10cce182bea29812a63c39d238efc9e6d3f962c9a1591223aed3b74adbd");
        }

        private static void Case_01423()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1423,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-15,44,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,8,40,37,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "3a35bdac5624341ac4b17ed31f13451af97a9f4fd6d42013b3cfc055faa64034");
        }

        private static void Case_01424()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1424,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-11,49,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,14,68,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-15,56,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-16,71,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,2,24,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-2,7,47,2), new GeneratedEnemyUnit(-7,10,26,16,4), new GeneratedEnemyUnit(-9,4,29,2,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "91bc144a3800aa5cd48c9210c53ce45ceb4de2cf738713ee81b88a81b5f9806b");
        }

        private static void Case_01425()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1425,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,13,90,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-13,99,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-10,15,12,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "79bebe4e1222f7805473af36443ec813a7732ae51f1da6f5eff2571cba35c5e8");
        }

        private static void Case_01426()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1426,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,1,91,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-17,63,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,5,92,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-2,72,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,9,59,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,9,62,7,3), new GeneratedEnemyUnit(2,4,89,26,1), new GeneratedEnemyUnit(-15,-11,62,29,3), new GeneratedEnemyUnit(3,-5,35,2,2), new GeneratedEnemyUnit(-3,17,20,9,2), new GeneratedEnemyUnit(19,15,67,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "f730b3376c8948e1abb0018aadcbccb1f863af0fc0ca6a02513253dc68dc89be");
        }

        private static void Case_01427()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1427,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,9,67,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-9,40,46,3), new GeneratedEnemyUnit(-2,11,42,37,3), new GeneratedEnemyUnit(-4,8,88,48,2), new GeneratedEnemyUnit(10,14,95,30,1), new GeneratedEnemyUnit(-15,-6,72,40,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "6a5050572183ee3a277e30725593883b60db3291164edcf00371233d9e9414e6");
        }

        private static void Case_01428()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1428,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,19,96,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-3,79,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-7,90,48,4), new GeneratedEnemyUnit(-2,12,36,40,1), new GeneratedEnemyUnit(14,-14,82,1,1), new GeneratedEnemyUnit(-15,-14,27,5,1), new GeneratedEnemyUnit(1,14,58,21,2), new GeneratedEnemyUnit(-5,-4,72,16,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "0eb12d4e4dac425c30c6f3b12b1dfdab9f0c9368a927b45f3b440058c5f18915");
        }

        private static void Case_01429()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1429,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-10,61,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,13,89,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,5,76,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,6,8,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,3,18,19,1), new GeneratedEnemyUnit(18,-19,88,20,1), new GeneratedEnemyUnit(-14,8,12,32,4), new GeneratedEnemyUnit(7,-6,64,47,4), new GeneratedEnemyUnit(-6,6,40,49,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "7b8c0592521783d6f7415e31bcc6bc7e9403e9e8f39dc57fcad25bf4ea6d06ae");
        }

        private static void Case_01430()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1430,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-16,83,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-2,29,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-11,16,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,0,24,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-18,37,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,14,11,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,3,73,3,2), new GeneratedEnemyUnit(7,11,53,43,3), new GeneratedEnemyUnit(14,-16,89,6,1), new GeneratedEnemyUnit(16,9,47,15,1), new GeneratedEnemyUnit(0,-2,79,27,1), new GeneratedEnemyUnit(14,-5,23,29,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "f57d07593845a88da30b118fe42633568620e4b3fc27b921db6516f379f1d105");
        }

        private static void Case_01431()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1431,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-20,12,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-16,18,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-17,47,42,4), new GeneratedEnemyUnit(13,-10,10,33,2), new GeneratedEnemyUnit(-20,-10,36,7,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "0f7b35364f91594cbffe42df8785f302f661548c2a6ff06d9e0ce5a82e03287a");
        }

        private static void Case_01432()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1432,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,19,44,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,13,88,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,0,90,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-12,85,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-1,47,40,1), new GeneratedEnemyUnit(16,18,60,16,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "7696b023abc883d7c1d97d433a7e0ddb8c654983221a964cf06bcd30ccaffa9c");
        }

        private static void Case_01433()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1433,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,15,100,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-2,85,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-16,97,31,3), new GeneratedEnemyUnit(19,7,43,17,2), new GeneratedEnemyUnit(1,9,85,19,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "fe4b2314df50baaa1f90995714bc5014519fb7258708ef54646c423c28d8ac2c");
        }

        private static void Case_01434()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1434,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,6,96,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-10,65,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,12,36,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,20,14,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,0,52,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,8,49,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-16,64,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,12,77,22,3), new GeneratedEnemyUnit(-16,-7,95,46,4), new GeneratedEnemyUnit(3,-7,37,49,3), new GeneratedEnemyUnit(14,-11,26,49,3), new GeneratedEnemyUnit(-16,-13,46,9,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "9b114080825f1c8dfb924dccdd29368ea724987d63229cb3db171a9ecae04401");
        }

        private static void Case_01435()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1435,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,17,57,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-8,37,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-4,70,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,16,72,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-3,69,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-15,25,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-2,12,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-15,66,24,2), new GeneratedEnemyUnit(17,-8,82,39,1), new GeneratedEnemyUnit(-17,14,77,30,3), new GeneratedEnemyUnit(-5,-10,38,21,2), new GeneratedEnemyUnit(3,-20,44,15,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "96637d2de3fab35e7fe114408f6347aadc9b7a1de417fe04c6af241a8fb9642c");
        }

        private static void Case_01436()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1436,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-15,35,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-20,47,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,15,16,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,10,62,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-14,41,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,3,28,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,5,5,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-13,7,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-7,55,6,2), new GeneratedEnemyUnit(-6,4,77,49,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "c8500f24a0d21a39392b7aaf6121296dcd8e64d8b326ab8f39c8f24b39560005");
        }

        private static void Case_01437()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1437,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,0,33,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,12,51,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-3,8,16,1), new GeneratedEnemyUnit(15,-3,24,14,3), new GeneratedEnemyUnit(-8,-10,43,5,3), new GeneratedEnemyUnit(-14,4,15,35,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "83fefe5c5aa7ef52b550e65e6004bd91882f5508d24195ccd2777eed2d173859");
        }

        private static void Case_01438()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1438,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,5,45,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,13,61,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-2,29,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-19,61,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,12,24,44,2), new GeneratedEnemyUnit(6,-9,30,5,2), new GeneratedEnemyUnit(19,14,91,38,1), new GeneratedEnemyUnit(-3,13,35,30,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "77d7b6b26f889962b26d48acd46ccddd23d0bda93eb5ae03e5096e0798f2d188");
        }

        private static void Case_01439()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1439,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,10,82,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-12,28,8,4), new GeneratedEnemyUnit(-15,-4,70,19,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "82f01bddf1049bd707f55ff7ce731879eca02ce1b07baf561f89b1a9a93354bc");
        }

    }
}
