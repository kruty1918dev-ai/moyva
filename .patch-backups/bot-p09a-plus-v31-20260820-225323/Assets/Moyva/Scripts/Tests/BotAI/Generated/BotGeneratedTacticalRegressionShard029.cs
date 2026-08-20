using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard029
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_05220();
            Case_05221();
            Case_05222();
            Case_05223();
            Case_05224();
            Case_05225();
            Case_05226();
            Case_05227();
            Case_05228();
            Case_05229();
            Case_05230();
            Case_05231();
            Case_05232();
            Case_05233();
            Case_05234();
            Case_05235();
            Case_05236();
            Case_05237();
            Case_05238();
            Case_05239();
            Case_05240();
            Case_05241();
            Case_05242();
            Case_05243();
            Case_05244();
            Case_05245();
            Case_05246();
            Case_05247();
            Case_05248();
            Case_05249();
            Case_05250();
            Case_05251();
            Case_05252();
            Case_05253();
            Case_05254();
            Case_05255();
            Case_05256();
            Case_05257();
            Case_05258();
            Case_05259();
            Case_05260();
            Case_05261();
            Case_05262();
            Case_05263();
            Case_05264();
            Case_05265();
            Case_05266();
            Case_05267();
            Case_05268();
            Case_05269();
            Case_05270();
            Case_05271();
            Case_05272();
            Case_05273();
            Case_05274();
            Case_05275();
            Case_05276();
            Case_05277();
            Case_05278();
            Case_05279();
            Case_05280();
            Case_05281();
            Case_05282();
            Case_05283();
            Case_05284();
            Case_05285();
            Case_05286();
            Case_05287();
            Case_05288();
            Case_05289();
            Case_05290();
            Case_05291();
            Case_05292();
            Case_05293();
            Case_05294();
            Case_05295();
            Case_05296();
            Case_05297();
            Case_05298();
            Case_05299();
            Case_05300();
            Case_05301();
            Case_05302();
            Case_05303();
            Case_05304();
            Case_05305();
            Case_05306();
            Case_05307();
            Case_05308();
            Case_05309();
            Case_05310();
            Case_05311();
            Case_05312();
            Case_05313();
            Case_05314();
            Case_05315();
            Case_05316();
            Case_05317();
            Case_05318();
            Case_05319();
            Case_05320();
            Case_05321();
            Case_05322();
            Case_05323();
            Case_05324();
            Case_05325();
            Case_05326();
            Case_05327();
            Case_05328();
            Case_05329();
            Case_05330();
            Case_05331();
            Case_05332();
            Case_05333();
            Case_05334();
            Case_05335();
            Case_05336();
            Case_05337();
            Case_05338();
            Case_05339();
            Case_05340();
            Case_05341();
            Case_05342();
            Case_05343();
            Case_05344();
            Case_05345();
            Case_05346();
            Case_05347();
            Case_05348();
            Case_05349();
            Case_05350();
            Case_05351();
            Case_05352();
            Case_05353();
            Case_05354();
            Case_05355();
            Case_05356();
            Case_05357();
            Case_05358();
            Case_05359();
            Case_05360();
            Case_05361();
            Case_05362();
            Case_05363();
            Case_05364();
            Case_05365();
            Case_05366();
            Case_05367();
            Case_05368();
            Case_05369();
            Case_05370();
            Case_05371();
            Case_05372();
            Case_05373();
            Case_05374();
            Case_05375();
            Case_05376();
            Case_05377();
            Case_05378();
            Case_05379();
            Case_05380();
            Case_05381();
            Case_05382();
            Case_05383();
            Case_05384();
            Case_05385();
            Case_05386();
            Case_05387();
            Case_05388();
            Case_05389();
            Case_05390();
            Case_05391();
            Case_05392();
            Case_05393();
            Case_05394();
            Case_05395();
            Case_05396();
            Case_05397();
            Case_05398();
            Case_05399();
        }

        private static void Case_05220()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5220,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,0,44,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,12,61,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-7,95,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,7,95,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,19,39,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,14,46,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,4,28,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-14,76,37,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "857f01f0e38f0379eeb74d8977bce6476297c52af21135c83d39426834fa0ac4");
        }

        private static void Case_05221()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5221,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,20,44,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,3,24,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,12,51,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-1,32,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,14,31,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-13,97,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,4,94,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-10,98,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-19,89,43,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "3659c94d1ec1ffe325c8636e3323a2c698b61e390cd10d816f8a54dfb746dd43");
        }

        private static void Case_05222()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5222,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-7,96,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,4,86,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,18,54,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,4,32,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-18,38,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-16,68,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-16,38,40,4), new GeneratedEnemyUnit(3,16,48,36,1), new GeneratedEnemyUnit(5,6,74,34,2), new GeneratedEnemyUnit(0,16,6,50,3), new GeneratedEnemyUnit(-6,3,59,5,1), new GeneratedEnemyUnit(-12,-9,90,6,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "c527016215dbf9980bf39275042a819adde55544f25d0754b22de69a1ae24212");
        }

        private static void Case_05223()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5223,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,2,83,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-20,18,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,13,11,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,4,21,47,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "8fffbd7972360c3306aacc757c23804a88780d7cd8dc1c30de96750db2f60a98");
        }

        private static void Case_05224()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5224,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-7,76,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,6,39,26,2), new GeneratedEnemyUnit(14,-6,60,5,4), new GeneratedEnemyUnit(19,-7,61,37,1), new GeneratedEnemyUnit(-18,4,50,24,1), new GeneratedEnemyUnit(6,9,72,40,4), new GeneratedEnemyUnit(8,4,54,16,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "1fbe5f013706778c036a0ee91c48ffcb1450635feb9c47aa11da16c0e93edceb");
        }

        private static void Case_05225()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5225,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,19,86,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-6,38,42,4), new GeneratedEnemyUnit(-4,8,64,21,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "69608bdaab7cdf9064b170873fb6ca359c8e2e2b9e960414f71cf8a16c122b11");
        }

        private static void Case_05226()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5226,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,9,85,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-9,76,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,12,95,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,6,56,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-5,47,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-12,78,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,10,59,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-8,27,7,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "ce840366d3b974291312281ed1074b4b9da0fea195463ca8080cdcf9a2043ba2");
        }

        private static void Case_05227()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5227,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-15,54,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,7,98,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-8,35,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,14,48,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-2,19,3,1), new GeneratedEnemyUnit(17,16,33,15,2), new GeneratedEnemyUnit(13,14,20,20,2), new GeneratedEnemyUnit(0,-6,53,28,1), new GeneratedEnemyUnit(-4,13,65,23,1), new GeneratedEnemyUnit(2,11,57,24,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "0ddc9c6eb80a9cbdc97df83b0bac9cea747b234508528bb21dadbdb798979326");
        }

        private static void Case_05228()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5228,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,9,69,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-6,10,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,15,51,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-7,5,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,15,74,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,10,71,24,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "71ecbfc660938429d47b227c80752669c8b429b492fbf914e622ac8081f453d9");
        }

        private static void Case_05229()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5229,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,8,40,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,0,18,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,2,40,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-1,81,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,6,51,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,14,50,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-12,51,37,3), new GeneratedEnemyUnit(-16,13,5,21,3), new GeneratedEnemyUnit(9,-5,22,35,2), new GeneratedEnemyUnit(-7,13,90,6,2), new GeneratedEnemyUnit(7,-3,70,35,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "4b0fc8220d6ef32ae3974249473e427f2f569f6dc0b8515681d856ca722534d6");
        }

        private static void Case_05230()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5230,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-11,66,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-13,83,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-2,54,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-14,46,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-2,61,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-2,41,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-11,21,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-4,8,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,2,20,9,1), new GeneratedEnemyUnit(20,1,51,25,4), new GeneratedEnemyUnit(0,14,31,19,1), new GeneratedEnemyUnit(-6,-7,41,30,1), new GeneratedEnemyUnit(-2,-7,21,19,2), new GeneratedEnemyUnit(5,16,15,18,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "14ede4f9b33b4407496ddbd6f796bb7a4054a5f50243f1a946e9caa81454dd80");
        }

        private static void Case_05231()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5231,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,7,82,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-10,62,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,4,32,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-1,38,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,3,11,18,2), new GeneratedEnemyUnit(13,17,40,41,4), new GeneratedEnemyUnit(16,-12,79,41,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "739f23659c7a728610e54eca8f6887a8cbc1b9a4db932adcc69da2db6b453ac0");
        }

        private static void Case_05232()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5232,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,15,46,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-13,64,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,18,90,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-1,59,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-11,61,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-6,48,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-2,84,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0e6500f1d81bd3a6e62e35681b0e8895923a1b1596b832a656062d21f4b08c43");
        }

        private static void Case_05233()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5233,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,18,89,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-6,64,39,1), new GeneratedEnemyUnit(17,-18,46,47,2), new GeneratedEnemyUnit(1,-13,75,2,2), new GeneratedEnemyUnit(-15,-2,63,49,3), new GeneratedEnemyUnit(-7,-18,68,8,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b136f0b0cf528b30f1bb2a6e6d72dafb8d55163659d020dedc713cf0747887bd");
        }

        private static void Case_05234()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5234,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-14,55,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,0,39,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-7,71,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,19,40,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,5,59,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-11,62,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-5,85,16,1), new GeneratedEnemyUnit(0,-4,30,24,4), new GeneratedEnemyUnit(-18,-15,69,2,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "22b8806ef771a80e09c3eff096672be1c90456e87db9585c933397202e8bc83b");
        }

        private static void Case_05235()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5235,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,13,81,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,16,10,2,4), new GeneratedEnemyUnit(15,12,85,32,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "0b64611a29d18803aadb62ed5801ef57ed8675223629efc5e67689773fb6dfaf");
        }

        private static void Case_05236()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5236,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-5,55,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-8,14,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,14,10,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,5,14,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,6,50,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,4,84,4,1), new GeneratedEnemyUnit(1,8,27,19,1), new GeneratedEnemyUnit(-11,-16,75,34,3), new GeneratedEnemyUnit(12,-15,66,50,1), new GeneratedEnemyUnit(10,9,91,9,2), new GeneratedEnemyUnit(0,-20,72,15,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "aff61e731b5cb3b8b15a7a3ba67acfd7b7c1e221bca568e229cdc256e1f27eaa");
        }

        private static void Case_05237()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5237,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-19,15,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,12,35,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,14,48,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,19,31,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,12,90,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-17,68,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,6,83,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-4,87,1,2), new GeneratedEnemyUnit(8,5,7,37,4), new GeneratedEnemyUnit(-5,-14,9,45,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "3ecb602e9150ef12c026dac1ed554ddd2798c19a76d0f85d3804aa9d87354673");
        }

        private static void Case_05238()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5238,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,20,75,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,12,15,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-7,48,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-7,98,9,2), new GeneratedEnemyUnit(19,-5,70,10,3), new GeneratedEnemyUnit(-5,-3,90,33,3), new GeneratedEnemyUnit(8,11,100,32,2), new GeneratedEnemyUnit(1,2,24,32,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "82630773fa06bf839cae776807849778c30f7c551073ae5fe8ac6fd636561929");
        }

        private static void Case_05239()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5239,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-18,48,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,9,39,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,2,23,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,16,11,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,20,9,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,18,63,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-19,45,29,3), new GeneratedEnemyUnit(11,8,80,21,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "4e16db9f27c046598b4a09a6fcb7c0914f5a49803a1384943f15539b8871e8f9");
        }

        private static void Case_05240()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5240,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-8,48,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-8,47,31,1), new GeneratedEnemyUnit(1,-6,24,24,3), new GeneratedEnemyUnit(-15,-12,90,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "a761a7beb4d53e778c6d6b53c9c5637420e7b83c9f199ee5693ae32860345000");
        }

        private static void Case_05241()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5241,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-14,77,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,13,52,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,11,83,3,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "29fb80b26e2a94519a6653cef437f1cf1e6fd90a1cf1a8e57d4c25f84891a3eb");
        }

        private static void Case_05242()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5242,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,9,50,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,3,31,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,6,24,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,17,83,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-2,37,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,5,92,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-7,8,40,4), new GeneratedEnemyUnit(11,19,94,35,1), new GeneratedEnemyUnit(-14,-9,10,30,3), new GeneratedEnemyUnit(-17,5,49,23,4), new GeneratedEnemyUnit(20,-18,20,9,2), new GeneratedEnemyUnit(18,1,81,10,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "2262fec43081028be057a8d7c11365ae744f381f0d47aebafb316221e7cf2119");
        }

        private static void Case_05243()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5243,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-5,22,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,2,47,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,16,73,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,2,7,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,14,99,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,11,38,6,2), new GeneratedEnemyUnit(6,-13,6,30,4), new GeneratedEnemyUnit(8,7,16,36,2), new GeneratedEnemyUnit(-17,5,38,23,4), new GeneratedEnemyUnit(-10,-8,67,38,1), new GeneratedEnemyUnit(13,-19,18,45,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "c885ec6ee67bf2a9c2d0c73d00904bf44651e86d0efed1cc15bc034937402a4e");
        }

        private static void Case_05244()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5244,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-17,86,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,1,21,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,11,63,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,7,88,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-12,30,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,16,47,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,0,32,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,2,17,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-6,10,48,2), new GeneratedEnemyUnit(-15,17,54,21,4), new GeneratedEnemyUnit(-13,-10,36,12,2), new GeneratedEnemyUnit(-2,-10,7,34,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "17d5cc67ce7e5922387e500dfbc294fe3dec327dfbb0d6596907909074f96f48");
        }

        private static void Case_05245()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5245,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-20,27,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-20,42,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,8,96,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,12,18,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,7,41,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-18,13,31,2), new GeneratedEnemyUnit(20,-12,81,16,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "d585aa5ea9d5dcaabc04d50f7c6e44ed35d5ef72575356dd66801b1118c1c869");
        }

        private static void Case_05246()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5246,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-7,96,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-9,23,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,5,42,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-16,74,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-1,34,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,16,73,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,11,90,2,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "da75a61fe7e5675456924703d316892167423a0c99c235ea7f186c06820361bc");
        }

        private static void Case_05247()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5247,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,18,19,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,4,50,43,2), new GeneratedEnemyUnit(10,4,44,20,4), new GeneratedEnemyUnit(-4,15,44,2,2), new GeneratedEnemyUnit(18,-5,28,40,1), new GeneratedEnemyUnit(20,2,44,6,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "d4196bc922c20571d269d789a63b1b9217ec05c1e4b15c468ae4c541e35c71fa");
        }

        private static void Case_05248()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5248,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-10,77,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-16,15,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-15,40,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,19,40,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-5,35,3,4), new GeneratedEnemyUnit(14,-20,82,29,2), new GeneratedEnemyUnit(-3,17,87,29,3), new GeneratedEnemyUnit(-7,-12,13,24,3), new GeneratedEnemyUnit(-5,-16,59,1,4), new GeneratedEnemyUnit(15,14,35,11,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "2beb4b16a0887808b37d8216f73ff2d39d10e69f93f75d2826aab4b210ac8b64");
        }

        private static void Case_05249()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5249,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,4,33,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-1,45,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-15,28,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-17,70,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-7,79,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-1,90,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,8,29,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,3,34,14,2), new GeneratedEnemyUnit(1,-12,45,21,1), new GeneratedEnemyUnit(-6,-3,16,20,2), new GeneratedEnemyUnit(2,-20,51,1,1), new GeneratedEnemyUnit(-9,-3,98,8,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "b4857450d12a8e595226890b56b0ff289e3b11b7193a3c2dc89267261864f4eb");
        }

        private static void Case_05250()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5250,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-4,62,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,20,16,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-4,66,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-12,5,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,14,78,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,19,14,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,6,18,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-19,79,5,2), new GeneratedEnemyUnit(-1,16,89,17,2), new GeneratedEnemyUnit(-13,13,24,23,4), new GeneratedEnemyUnit(20,14,46,50,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "56597e870a2a38cd84997036a20b0f4f51f714877487cfbcc834ad9749f15447");
        }

        private static void Case_05251()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5251,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-16,19,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,12,56,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-5,20,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,13,51,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,1,65,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-10,86,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,0,91,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-14,24,38,4), new GeneratedEnemyUnit(-1,-17,65,41,3), new GeneratedEnemyUnit(6,7,34,46,4), new GeneratedEnemyUnit(-20,-17,92,24,3), new GeneratedEnemyUnit(10,14,84,39,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "7aa90624869b2a6bdb661de5a5fe8273d22d455fa1711e017f844dbc67d4d6be");
        }

        private static void Case_05252()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5252,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-14,50,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,18,77,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,13,54,15,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "ff28aa9993ade1427ab56b6e2c7fa09b316f6eb734bb1943d7f6b9204f6254cc");
        }

        private static void Case_05253()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5253,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,3,39,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-6,93,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,8,97,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,9,42,16,3), new GeneratedEnemyUnit(4,10,10,8,3), new GeneratedEnemyUnit(-5,9,68,9,1), new GeneratedEnemyUnit(17,8,54,39,4), new GeneratedEnemyUnit(18,4,50,6,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "5489cea3f8bd8734ebcf956d21a262806c78d29013022df401a8538f02144393");
        }

        private static void Case_05254()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5254,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,8,95,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,19,58,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,12,29,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-12,81,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,9,89,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-1,37,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-3,89,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-7,46,38,4), new GeneratedEnemyUnit(-11,-11,72,14,2), new GeneratedEnemyUnit(13,10,14,12,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "e164035e6cf83e24399464db856bc01322f99cc722fb4ebd52096233ae2c609e");
        }

        private static void Case_05255()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5255,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,5,94,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-19,21,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,13,44,6,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "7c0bfd053eaa31ab9c306c347d454c640eb89ff1df9ea8e598c4a16e57c3b392");
        }

        private static void Case_05256()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5256,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-15,68,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-20,50,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-4,48,2,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "ae3b3cc3a3377734496327175a3e6a1aea436fc08d017184277e41bec5d723e0");
        }

        private static void Case_05257()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5257,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,17,87,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-2,83,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-20,32,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-19,15,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,7,60,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-13,59,1,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "61316c58596ba227dccda457fe6f887e858dec9d0b7da088298b714dac76b091");
        }

        private static void Case_05258()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5258,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-9,41,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-20,50,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-4,13,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,3,80,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,0,41,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,18,55,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,2,37,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,6,62,13,4), new GeneratedEnemyUnit(-18,9,51,27,1), new GeneratedEnemyUnit(7,12,48,34,2), new GeneratedEnemyUnit(5,-17,77,10,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "d6922ee44e8689beddf9d5322e94244f1bef5044c2fea1c9bbbfaba04981d8bf");
        }

        private static void Case_05259()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5259,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,4,31,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-15,78,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-16,92,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,9,47,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,4,83,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,15,51,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,9,10,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,14,55,1,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "25bf83ea624313e1eb42b99435413240a3c5a10c78763cb1af136e26495cd1bd");
        }

        private static void Case_05260()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5260,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,10,57,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-12,57,38,1), new GeneratedEnemyUnit(15,12,81,29,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "0b99c0bc424fb30fd08893adf48991297bb22f7d8415db87416668b0c34173b2");
        }

        private static void Case_05261()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5261,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-8,59,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,10,25,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-5,50,22,4), new GeneratedEnemyUnit(-14,17,43,20,3), new GeneratedEnemyUnit(-5,16,68,36,1), new GeneratedEnemyUnit(-12,-10,13,9,4), new GeneratedEnemyUnit(-12,11,11,29,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "4b5a4e52723b3daf701091f4bb4e24dc2ca81d6ecdc59b5d29d68a6e23dc12fe");
        }

        private static void Case_05262()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5262,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-2,26,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,6,18,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-7,56,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-15,64,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,7,29,24,1), new GeneratedEnemyUnit(-3,-14,25,25,2), new GeneratedEnemyUnit(-12,-2,22,24,1), new GeneratedEnemyUnit(-3,11,65,35,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "b8bff4aec49f35166ad43745a13b8f6d143992220fcfb82626c4aa2d547d3909");
        }

        private static void Case_05263()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5263,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,19,5,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,9,94,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-16,90,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-12,64,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,18,84,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-4,60,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,12,43,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-3,10,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,13,96,18,4), new GeneratedEnemyUnit(-10,-5,35,9,4), new GeneratedEnemyUnit(11,0,34,44,3), new GeneratedEnemyUnit(3,19,23,20,1), new GeneratedEnemyUnit(10,-1,50,40,4), new GeneratedEnemyUnit(-6,-1,42,19,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "6d16765c749b36a361dc5d53058c0afb603d96ab608f9a54a21628ff6a0ce627");
        }

        private static void Case_05264()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5264,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-6,62,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-3,53,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,10,86,50,2), new GeneratedEnemyUnit(12,-9,100,5,1), new GeneratedEnemyUnit(19,0,62,29,1), new GeneratedEnemyUnit(3,-19,89,35,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "5bf74f73ef7c5c3d4c9a49522beb955a41538ab0a498b78b80c40b2043d7427f");
        }

        private static void Case_05265()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5265,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,7,78,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-11,8,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,11,40,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-10,9,2,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "b53711c17b5f366098cac56150b3642bb2d3492ddd364ec001cf96a7e0f59fe6");
        }

        private static void Case_05266()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5266,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-6,16,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-9,11,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-20,24,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-14,52,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,12,31,12,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "4de8c6e2316f8f6d37290eb271f484e2352358d6427f8d5131cf1419d7cc7895");
        }

        private static void Case_05267()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5267,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,16,39,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,12,8,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-20,53,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,7,16,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-4,83,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-18,20,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-7,63,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,12,73,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,1,72,33,3), new GeneratedEnemyUnit(5,-18,43,3,3), new GeneratedEnemyUnit(-1,8,93,12,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "de20a69a981685299f9b506911f3048cc957b068155e2c4b06aa9f8d1ae9cd26");
        }

        private static void Case_05268()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5268,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-1,46,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-11,20,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,0,78,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,17,96,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,16,12,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,2,94,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-11,94,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "8021dbf707a457be670204047b719584ef734f537cc7715ea1d477aaede832e5");
        }

        private static void Case_05269()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5269,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,3,64,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,10,5,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-3,47,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,6,33,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,7,26,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,15,56,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,18,29,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-4,90,17,2), new GeneratedEnemyUnit(-19,1,76,29,2), new GeneratedEnemyUnit(6,19,61,40,1), new GeneratedEnemyUnit(8,17,55,28,1), new GeneratedEnemyUnit(10,8,13,10,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "dd9e7de7345e169f8459c4ce3f862c1f714a283c4983d92c288994913b06abed");
        }

        private static void Case_05270()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5270,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,0,83,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-18,87,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-10,97,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,19,95,17,4), new GeneratedEnemyUnit(20,20,42,40,2), new GeneratedEnemyUnit(-17,5,88,8,4), new GeneratedEnemyUnit(0,11,59,35,4), new GeneratedEnemyUnit(-1,-20,16,7,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "48f823b24b01e7e2757c35fe1a6fe00007bafe1a7d29b253157d3b83ebc43df7");
        }

        private static void Case_05271()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5271,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-20,61,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-3,23,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-9,60,3,3), new GeneratedEnemyUnit(-10,-14,95,44,2), new GeneratedEnemyUnit(14,-5,93,23,1), new GeneratedEnemyUnit(1,11,87,30,3), new GeneratedEnemyUnit(9,3,10,1,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "161712fb3a10abf8656edf1562b092392e422680dca945549031ac54f7fb9b6c");
        }

        private static void Case_05272()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5272,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,20,81,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,14,25,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-6,5,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-16,37,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,19,12,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,19,33,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-6,64,5,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "d5fe3f1cca44b848ccb9b33fd94274ea893ff05f31ab444e87a327fcb5defbbd");
        }

        private static void Case_05273()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5273,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,13,16,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-5,22,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,15,18,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,3,22,48,3), new GeneratedEnemyUnit(15,-1,28,21,4), new GeneratedEnemyUnit(-14,-18,90,43,3), new GeneratedEnemyUnit(-10,-6,34,43,1), new GeneratedEnemyUnit(-15,5,20,40,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "7530a4b40bdbf348bb0ec0803cab11fbe7d78deaf26a6540ce684b51e686d796");
        }

        private static void Case_05274()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5274,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,13,31,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,5,76,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-19,61,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,16,77,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-15,21,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-11,48,18,3), new GeneratedEnemyUnit(1,-1,13,46,3), new GeneratedEnemyUnit(20,-14,26,41,2), new GeneratedEnemyUnit(-13,6,92,38,4), new GeneratedEnemyUnit(-12,20,74,21,3), new GeneratedEnemyUnit(-4,-9,26,28,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "32fa24b8bab78759c9224cbaa8a4ca83b5408935dda6a84fd358c6d87b3b0195");
        }

        private static void Case_05275()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5275,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,9,88,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-4,100,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,20,100,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,18,22,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-3,94,13,2), new GeneratedEnemyUnit(-14,2,85,1,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "e15e5cb0639f12cd56bb6dfdabb32aa0110d1c2e115dcb4b755f13bbca70ed2a");
        }

        private static void Case_05276()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5276,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,0,31,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,18,52,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,14,9,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-10,12,6,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "467f1453ebb65d53ae571f4f61030faa113552b2cb6582ee7891ffe0c3a22fc6");
        }

        private static void Case_05277()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5277,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-2,79,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,16,7,33,3), new GeneratedEnemyUnit(-3,0,29,21,2), new GeneratedEnemyUnit(3,-15,53,12,1), new GeneratedEnemyUnit(18,5,23,49,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "4622e89ddd098b8844f7306ea0ca767562c66385d65a0b2202f03c6cd19af6ac");
        }

        private static void Case_05278()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5278,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-18,25,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,6,40,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-11,99,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-12,52,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-10,72,9,4), new GeneratedEnemyUnit(-19,9,61,6,2), new GeneratedEnemyUnit(9,2,33,25,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "96569b0666e4352577717c9ea514bafa188db30cf4910af0954ab95866482a5d");
        }

        private static void Case_05279()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5279,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,16,91,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-20,24,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,8,31,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-7,89,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,16,24,1,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "876ace6aecaaf43df04b03242c5b3ef68d5dbf16e7974f18b4b00406df4c8772");
        }

        private static void Case_05280()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5280,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-15,16,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,12,31,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-5,70,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-7,95,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,5,92,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,12,76,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-4,16,25,3), new GeneratedEnemyUnit(8,-15,17,50,3), new GeneratedEnemyUnit(-14,13,98,10,1), new GeneratedEnemyUnit(-20,6,83,12,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "d957d52f56836f89752df98a904992f8b447ddca0bacf880557a25df5b541975");
        }

        private static void Case_05281()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5281,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-12,36,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,9,48,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-14,17,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "09223d11364298fd03804f0b61b58b570ff08228e672192a996e49d7b2ec29c2");
        }

        private static void Case_05282()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5282,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-13,70,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-6,40,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-10,34,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-17,87,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,20,92,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-12,29,34,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e4bcfba48baae23695f86ef393eec2aee6a98a4ddf77b075ca3219e4b68f7152");
        }

        private static void Case_05283()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5283,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-4,59,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-6,19,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,14,80,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-1,44,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-8,90,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-5,5,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,12,55,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,16,54,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,9,47,41,3), new GeneratedEnemyUnit(-6,17,17,37,1), new GeneratedEnemyUnit(0,19,78,47,2), new GeneratedEnemyUnit(-8,-20,69,36,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "798fd869cf07f9da5c62d493c20c8f2dd51d2dbc0ad2ed8bafcceb819839fa72");
        }

        private static void Case_05284()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5284,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,9,58,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,13,68,44,4), new GeneratedEnemyUnit(2,8,7,3,4), new GeneratedEnemyUnit(13,18,7,29,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "db50a4fb842592605e7a476dd512239e1811c40a0abe567bca503ecb3e59f715");
        }

        private static void Case_05285()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5285,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-12,51,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,3,15,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,8,29,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,3,56,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-13,91,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-5,38,5,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "526a2cf8b464f0f875ef1b939c6b9aa0f9aaeb85cbc82c57057162793bf648c3");
        }

        private static void Case_05286()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5286,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-8,64,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,12,71,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,6,74,6,3), new GeneratedEnemyUnit(-16,-17,74,7,3), new GeneratedEnemyUnit(-8,-13,57,21,3), new GeneratedEnemyUnit(-4,16,79,42,3), new GeneratedEnemyUnit(-18,-5,24,34,2), new GeneratedEnemyUnit(-18,-12,44,28,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "1628cf6863dca633e7e9549964003116c427d364070999585309c980db145482");
        }

        private static void Case_05287()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5287,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,18,31,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-17,65,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-17,52,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-9,40,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,16,60,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,7,87,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-18,45,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-14,35,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,6,14,45,3), new GeneratedEnemyUnit(20,-18,59,29,3), new GeneratedEnemyUnit(4,18,40,31,4), new GeneratedEnemyUnit(17,-10,17,11,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "d164200055578df51c0e0f8807bda230712c50b4096776db401f915e837b5daa");
        }

        private static void Case_05288()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5288,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-12,36,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-12,75,12,1), new GeneratedEnemyUnit(-8,15,13,28,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "e82e7f82dd31cfe2eadad613e3a734905836969a6c0f6a37f461558374af688c");
        }

        private static void Case_05289()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5289,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,10,25,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-18,48,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,17,47,39,4), new GeneratedEnemyUnit(-14,-11,57,48,2), new GeneratedEnemyUnit(-19,-13,37,47,4), new GeneratedEnemyUnit(-6,-5,59,11,1), new GeneratedEnemyUnit(-8,5,42,3,4), new GeneratedEnemyUnit(-11,2,98,40,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d1384a80d62c993b16460e3499585c3bea09299789e6a76c7d6d8a0f0dcc523a");
        }

        private static void Case_05290()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5290,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-16,23,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,11,34,41,4), new GeneratedEnemyUnit(-6,7,17,47,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "f5be65973dfc2328ee3156a426a1102fe014a6ba03625a6d72e48ec4464e8239");
        }

        private static void Case_05291()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5291,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-20,41,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-14,5,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,3,78,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,12,25,27,3), new GeneratedEnemyUnit(8,9,52,20,2), new GeneratedEnemyUnit(14,19,49,50,3), new GeneratedEnemyUnit(2,12,72,39,2), new GeneratedEnemyUnit(-19,5,45,45,2), new GeneratedEnemyUnit(-14,-20,93,14,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "55c457825156682945be0b82f3c58fc46ea5f14a9bf4a17713eef5ec42585957");
        }

        private static void Case_05292()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5292,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-9,61,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,9,51,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,14,91,6,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "41f3b262b526d92663221eeefbf954e58ef92c86d659ce2f757f0f3c9d4f4489");
        }

        private static void Case_05293()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5293,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-1,92,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,15,26,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-10,23,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,2,87,14,4), new GeneratedEnemyUnit(13,-13,23,31,2), new GeneratedEnemyUnit(13,-12,11,13,4), new GeneratedEnemyUnit(-11,-2,57,23,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "48c66d6e2fd9446a8208a95dda7a4e8696fcb5697d4707bd58e9b58f19d9b728");
        }

        private static void Case_05294()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5294,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-16,73,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-12,88,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,3,30,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,15,36,47,3), new GeneratedEnemyUnit(-18,14,28,21,2), new GeneratedEnemyUnit(5,16,29,40,3), new GeneratedEnemyUnit(15,-18,75,23,1), new GeneratedEnemyUnit(3,-13,73,14,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "52a3d2cfaf3106cce48a81c3ff2a6eef33b1c4ae76d85f3c9b4ad2ebb29095e0");
        }

        private static void Case_05295()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5295,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-12,75,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,15,89,27,4), new GeneratedEnemyUnit(14,1,7,43,2), new GeneratedEnemyUnit(6,18,18,27,2), new GeneratedEnemyUnit(-17,-17,84,13,1), new GeneratedEnemyUnit(6,-3,27,15,3), new GeneratedEnemyUnit(-19,-13,54,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b947338f5e4f187f243e0edf52c6739c549b84bb41676e7ec8aa717bd9f99d56");
        }

        private static void Case_05296()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5296,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-19,87,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,3,30,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-8,66,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-13,74,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,8,51,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-13,62,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-3,39,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-19,57,4,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "7f39869286600625e427f7f99f3c7bbd27f35d256268359210c2707f12d48578");
        }

        private static void Case_05297()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5297,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-20,75,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,2,66,1,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "4c3b4af20eedb4f1b3fbd90f50c5a0ade4fc127d9d7582712e7992994d0ef7a6");
        }

        private static void Case_05298()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5298,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-18,69,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,0,66,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,13,20,19,1), new GeneratedEnemyUnit(19,-5,65,39,1), new GeneratedEnemyUnit(-10,-14,7,37,4), new GeneratedEnemyUnit(-14,10,60,44,3), new GeneratedEnemyUnit(15,-13,95,7,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "d29a244ee4694710fe46b0c0220f420745ac59dd9b91f8af4c868a8ac5e856a2");
        }

        private static void Case_05299()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5299,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-6,89,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,18,50,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,11,60,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,14,66,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-18,61,40,3), new GeneratedEnemyUnit(-16,-11,52,46,1), new GeneratedEnemyUnit(11,18,9,29,3), new GeneratedEnemyUnit(18,-5,93,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "b5e29da91efe7f7cfece00adc1ccb486bce58f2e6ecf46826d3cf1772d5600bd");
        }

        private static void Case_05300()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5300,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-8,42,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-13,38,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,8,78,50,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e1a12ece6ed1408cc9197a7ba3f3e15aebd1687054994782c77becae36edd579");
        }

        private static void Case_05301()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5301,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-16,36,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,3,46,11,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "9b47ce60b1f2e3a712c9990fd5e2cc6afb377b7f796dbb1881a29095b12b042e");
        }

        private static void Case_05302()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5302,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,4,40,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,18,58,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,19,31,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-19,12,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,4,61,6,1), new GeneratedEnemyUnit(14,-9,94,27,2), new GeneratedEnemyUnit(15,-9,59,3,1), new GeneratedEnemyUnit(2,-2,25,37,4), new GeneratedEnemyUnit(2,-19,11,13,4), new GeneratedEnemyUnit(-8,19,10,13,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "226e13ad1c30d7f58d1878c73357f4aefa63f774c06d77cf4c6a5926f00c04a7");
        }

        private static void Case_05303()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5303,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-11,75,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,19,24,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,18,75,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,18,66,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-16,34,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-16,62,38,1), new GeneratedEnemyUnit(15,-7,81,13,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "78cbdddeae29188eacc20d94cfc36e6e934e5c55d67ceeaa3806eb61fc07b41d");
        }

        private static void Case_05304()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5304,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-5,69,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-10,89,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-4,33,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-6,48,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-3,67,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,6,79,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-13,37,2,1), new GeneratedEnemyUnit(-5,3,85,6,4), new GeneratedEnemyUnit(-4,-8,43,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "649d095d6c8ac8e0017db551d2536d264ca0ac27f9a6a00466f1f8b3e8890339");
        }

        private static void Case_05305()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5305,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,9,99,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-3,94,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-20,20,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,5,20,16,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "f75a5706ee93673121956c259871d27ef908b613b1c0d37762b2787ca4b52a08");
        }

        private static void Case_05306()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5306,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-1,80,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-3,91,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-18,27,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,2,73,20,4), new GeneratedEnemyUnit(15,-1,57,42,3), new GeneratedEnemyUnit(-15,-5,42,50,4), new GeneratedEnemyUnit(10,1,34,36,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "64ccfed75ac29e0032c7cff69d471ddeb24c08a54fa2952b52886b4864bc334d");
        }

        private static void Case_05307()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5307,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,5,21,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,17,55,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-14,86,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,20,57,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,1,55,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,2,33,8,1), new GeneratedEnemyUnit(-5,-7,83,1,4), new GeneratedEnemyUnit(9,0,46,34,2), new GeneratedEnemyUnit(-9,-2,82,36,4), new GeneratedEnemyUnit(-11,1,34,32,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "224940c5f4e385f790ec91362bfa5f5484eb8df11c7ee701a53b09e901fb4a3c");
        }

        private static void Case_05308()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5308,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-15,14,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,1,53,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-17,15,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,0,76,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-9,10,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,0,54,6,2), new GeneratedEnemyUnit(11,20,49,42,1), new GeneratedEnemyUnit(-17,15,12,37,4), new GeneratedEnemyUnit(19,-14,58,19,1), new GeneratedEnemyUnit(3,11,65,25,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "ae603808c6802769aa2f770e7a202af71999acf1d8a1550904e3537a6d62a7cd");
        }

        private static void Case_05309()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5309,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,0,83,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-11,36,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-20,83,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-15,50,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,9,26,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,15,6,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,18,72,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-20,68,28,1), new GeneratedEnemyUnit(3,12,61,26,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "8e3a8dc5392ff848df865b911f82ddb9a23addcd7c0293b03cd9d58d36625c4b");
        }

        private static void Case_05310()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5310,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,20,82,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,18,22,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-9,70,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,6,38,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-1,96,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,17,90,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,7,96,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,0,26,37,2), new GeneratedEnemyUnit(20,19,97,10,1), new GeneratedEnemyUnit(-14,7,83,1,1), new GeneratedEnemyUnit(-6,6,49,36,2), new GeneratedEnemyUnit(-20,6,24,1,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "9b55275e5b12b38c6b926331e17c1b77c70301a20a93ac4d344cbe4600c9a32a");
        }

        private static void Case_05311()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5311,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,0,52,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,18,61,5,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "95277e895012a5c486bff1b38f3ce9e1ca59bdab9b58f5ad8dbc66ef17abe820");
        }

        private static void Case_05312()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5312,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-11,78,5,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "5a78ae61b47dcbd87861f557ab13c7261b6fcd4186aa68eccaf699b7b0aa0290");
        }

        private static void Case_05313()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5313,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-19,5,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-6,6,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,1,11,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-7,9,20,2), new GeneratedEnemyUnit(-18,20,83,41,3), new GeneratedEnemyUnit(12,12,35,17,1), new GeneratedEnemyUnit(9,-3,44,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "8db093c65e6273be5d591c4b6d4f3215eeaaec396a87808797ba0cc3830994c0");
        }

        private static void Case_05314()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5314,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,4,16,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-17,8,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-17,89,21,1), new GeneratedEnemyUnit(0,-7,82,10,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "5ae33571449335ed037230a7415ddd013e10388c2f14f629cd225f703b654041");
        }

        private static void Case_05315()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5315,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-8,52,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,13,32,22,2), new GeneratedEnemyUnit(-18,-17,55,22,1), new GeneratedEnemyUnit(7,16,10,12,2), new GeneratedEnemyUnit(11,5,21,46,3), new GeneratedEnemyUnit(11,-1,52,44,3), new GeneratedEnemyUnit(-14,14,10,21,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "2ebbc31426fe9429d4291e2702d58bd38cda66e5fdc233438951334ea6070f97");
        }

        private static void Case_05316()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5316,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-5,60,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-5,77,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,7,57,36,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e288d83e849f154f17d190dbc81d69fdbe4e21cd1d4993d8501c0d9c83023fd3");
        }

        private static void Case_05317()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5317,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-11,20,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-2,82,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-15,58,5,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "1f0f7e3f31e56c261fb6edc66c7d60ad30c6636f213d393c46573bedf53787a9");
        }

        private static void Case_05318()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5318,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-3,85,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-18,8,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-8,39,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-18,12,37,1), new GeneratedEnemyUnit(8,-12,94,35,4), new GeneratedEnemyUnit(20,-1,34,15,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "fa47b25e8d95447a01ea56c3658e43b7f16b1d76dad35cdec500ff8abfe58980");
        }

        private static void Case_05319()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5319,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-5,94,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,3,22,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-10,89,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-18,10,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,12,99,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,17,9,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-4,66,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-6,79,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-8,63,34,1), new GeneratedEnemyUnit(8,-10,67,47,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "eb87b510cb21dde1daf66265c33ca09a0c6cd3f4e596979c7835efccc2df03aa");
        }

        private static void Case_05320()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5320,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,7,59,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,1,84,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-2,23,44,2), new GeneratedEnemyUnit(18,6,68,30,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "fa72e077855c8be65f106569f2f872e1b13d4267271e331b376dc9813dd01d6c");
        }

        private static void Case_05321()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5321,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,3,98,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-16,65,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,10,57,38,4), new GeneratedEnemyUnit(-11,0,35,4,2), new GeneratedEnemyUnit(7,12,85,29,4), new GeneratedEnemyUnit(18,-11,97,3,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "11872158ffa34e0c5cb2294e2cd723241a73d50acbbf1ba5ae6c9d46d1d2ba1f");
        }

        private static void Case_05322()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5322,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,20,47,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,11,10,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,8,35,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,18,16,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-20,97,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,8,46,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,19,50,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-8,39,11,2), new GeneratedEnemyUnit(14,-19,15,3,1), new GeneratedEnemyUnit(-15,-20,92,20,3), new GeneratedEnemyUnit(20,-16,51,25,4), new GeneratedEnemyUnit(8,1,20,1,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "82ed8308f1a548ef9c1efbf0f4db4868df17e40b415ef7042eb3b6441e3c492b");
        }

        private static void Case_05323()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5323,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-3,81,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,20,43,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-7,65,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-3,90,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,7,9,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,4,15,34,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "7b53d0d64b1a04c831e41fd3759ed114f4d1238a386eb9e78bc0ff17f0bc8be0");
        }

        private static void Case_05324()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5324,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-3,27,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-14,57,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-2,6,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,2,64,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,8,95,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-10,22,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,18,10,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,2,80,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-16,81,28,3), new GeneratedEnemyUnit(-18,4,23,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "9840aa08f56341e6014ea9d2cddc6ceaa2a0e8a747c397760a600c0463c5b086");
        }

        private static void Case_05325()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5325,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-17,32,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,11,79,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,17,62,5,2), new GeneratedEnemyUnit(9,3,70,31,4), new GeneratedEnemyUnit(-5,-10,91,23,3), new GeneratedEnemyUnit(7,5,49,47,4), new GeneratedEnemyUnit(-6,-11,87,13,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "8f8a17f8fe59588a50daee411e6b316d1f947af914c00e37c200fb08a506971f");
        }

        private static void Case_05326()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5326,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,3,51,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,0,24,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-4,60,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,2,69,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-16,100,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-4,20,46,2), new GeneratedEnemyUnit(4,19,41,24,2), new GeneratedEnemyUnit(8,-7,20,6,3), new GeneratedEnemyUnit(-5,-20,40,3,4), new GeneratedEnemyUnit(-13,-7,41,32,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8a0eb64a90434a1b212fae36efc2840689e8f0ba86ff04db0e34e87eda81df03");
        }

        private static void Case_05327()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5327,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,18,53,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,10,58,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,16,95,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-19,88,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-1,39,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-9,59,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "373a954f190cc55017cff6f6d06c5d43cda6d431d348e4d12efd62dfde228934");
        }

        private static void Case_05328()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5328,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,15,83,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-8,53,21,3), new GeneratedEnemyUnit(18,4,54,49,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "9b35ac10a22b8e2d05cf2142d3c1d3aee675385c6361355b2783f41b52fafe6e");
        }

        private static void Case_05329()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5329,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,15,84,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-18,44,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-11,54,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-20,55,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-2,13,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-8,68,15,2), new GeneratedEnemyUnit(-15,-12,76,39,3), new GeneratedEnemyUnit(-19,10,27,7,2), new GeneratedEnemyUnit(7,16,44,38,3), new GeneratedEnemyUnit(-15,1,77,50,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "48ef7485259b9dbfc208c6f5bf0b28f05e4235c3631b28cb1baffaf6645f02bc");
        }

        private static void Case_05330()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5330,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-7,96,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,19,26,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-6,62,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-18,5,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,18,34,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-15,36,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-19,41,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,10,52,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,3,51,21,1), new GeneratedEnemyUnit(15,9,32,26,3), new GeneratedEnemyUnit(-16,-18,23,5,2), new GeneratedEnemyUnit(-6,19,20,24,2), new GeneratedEnemyUnit(-15,-5,46,15,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "9af4eb1d790fcb1ab7a23314977db16a428da3bbc5fd2228b1147b4bfee6f429");
        }

        private static void Case_05331()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5331,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-15,90,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-14,29,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,6,64,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-19,35,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-14,10,1,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ebb4bed18c5bc44739d737dc3e3bad2e5b9bd013a3f290d30bac0c2de669f3c4");
        }

        private static void Case_05332()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5332,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-6,43,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,18,5,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-2,45,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,7,49,36,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "72788cf4041e2e7919d4021d063dea031c311c43438212867a21265b4562c1b8");
        }

        private static void Case_05333()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5333,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-6,5,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,3,84,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,6,14,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,16,62,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-11,43,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-7,98,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,1,85,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,4,90,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-12,70,14,2), new GeneratedEnemyUnit(-11,-12,91,42,3), new GeneratedEnemyUnit(7,18,83,3,3), new GeneratedEnemyUnit(-20,4,74,41,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "52c68dc0476d5cf54e3bc0789feaca235f1530e3a0ffd5faa657be6afc42509a");
        }

        private static void Case_05334()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5334,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,16,60,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-3,74,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-16,65,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-4,60,46,3), new GeneratedEnemyUnit(-18,-12,70,44,2), new GeneratedEnemyUnit(9,1,78,7,2), new GeneratedEnemyUnit(-16,-15,13,26,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8a998b31246a5290e1765a7ab020f8e45d875a76789c7dbb43acaeb073ba3b95");
        }

        private static void Case_05335()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5335,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-8,76,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-11,9,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,7,16,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-16,52,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-8,93,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,10,32,45,2), new GeneratedEnemyUnit(16,-4,59,12,2), new GeneratedEnemyUnit(-6,4,61,39,1), new GeneratedEnemyUnit(4,8,28,32,1), new GeneratedEnemyUnit(16,-3,61,49,2), new GeneratedEnemyUnit(-19,14,60,1,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "093ac6bcfaa50712fcacb0358509753aee78e74905edfce61ad409093adc01d4");
        }

        private static void Case_05336()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5336,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-16,26,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-18,91,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,9,50,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,18,86,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,0,52,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,10,25,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,1,17,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,6,40,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-19,72,11,1), new GeneratedEnemyUnit(2,4,27,19,4), new GeneratedEnemyUnit(8,20,16,14,2), new GeneratedEnemyUnit(6,-8,88,42,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "b9bc256d48c41812cc6c91ca432d48d33dac5937d7b6190d52eebab004aa09bd");
        }

        private static void Case_05337()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5337,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,6,17,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-8,36,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-19,41,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,5,46,25,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0a64d90e9d1469af4698bb871b0fcdf2df291e752a87335ec10f5aa63577af01");
        }

        private static void Case_05338()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5338,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,18,6,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,18,69,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,9,74,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,16,78,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,7,67,49,2), new GeneratedEnemyUnit(-16,3,24,34,1), new GeneratedEnemyUnit(-5,-1,21,23,4), new GeneratedEnemyUnit(9,-1,32,9,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "1017bc68903a5db965532f8d573f3e0ac22ce08e0a62ef9b6d935493f7527337");
        }

        private static void Case_05339()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5339,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,19,92,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-4,86,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,5,7,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-5,32,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,0,38,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,18,15,14,2), new GeneratedEnemyUnit(20,16,92,20,4), new GeneratedEnemyUnit(10,-20,23,4,4), new GeneratedEnemyUnit(3,14,41,7,4), new GeneratedEnemyUnit(-17,20,96,50,1), new GeneratedEnemyUnit(-20,4,82,20,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "ea296aa5f5e48e9974816acc61b7ed8839ed082bb91e0e0847698f3f9a2637e7");
        }

        private static void Case_05340()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5340,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-6,32,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,19,84,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-1,21,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,14,81,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,13,53,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,8,41,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-9,32,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,13,16,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,13,27,9,3), new GeneratedEnemyUnit(-13,-2,88,1,4), new GeneratedEnemyUnit(8,16,31,26,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "8e22358bff39c647a1d55d443d5c6d49a1bd2ff18a3518e820cde18245a96dc9");
        }

        private static void Case_05341()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5341,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-1,50,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,18,69,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,17,70,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,15,34,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,19,75,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-8,26,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,7,35,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,11,58,7,1), new GeneratedEnemyUnit(13,-9,13,39,1), new GeneratedEnemyUnit(-10,-12,40,31,3), new GeneratedEnemyUnit(-2,-17,12,8,2), new GeneratedEnemyUnit(13,-11,45,35,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "41a2384d3758c2acdfb51b4a54964dbce7213b30602f80a422aa3de3e467eae0");
        }

        private static void Case_05342()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5342,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-11,33,7,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "28cc8e64273055d76cfc6dbae52fbcce0f9e3a259f26d690c9acbb9b1ce8f43c");
        }

        private static void Case_05343()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5343,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,14,56,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-15,36,5,1), new GeneratedEnemyUnit(17,-11,64,25,2), new GeneratedEnemyUnit(7,15,6,14,2), new GeneratedEnemyUnit(6,5,93,3,4), new GeneratedEnemyUnit(11,18,60,8,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "82c5540cb1f25c57e869f50f02c816434cb23a7aa4c4b5781ae20611f0e6777d");
        }

        private static void Case_05344()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5344,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-14,91,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,3,36,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,14,28,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-11,47,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-16,78,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,14,11,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,20,12,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,4,74,3,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "f96d02fc03494b51c1be9d0f0c8169fe405567fa43db1ba41057b86e49eb1bc2");
        }

        private static void Case_05345()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5345,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,0,24,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,3,50,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,17,84,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,16,87,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-2,21,40,2), new GeneratedEnemyUnit(2,-7,67,33,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "267d998c4e3e7cc867b843833a45c6a6fce9a9d146fa169cec8f3d2201025a55");
        }

        private static void Case_05346()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5346,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-13,78,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,15,76,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-4,60,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,15,21,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,3,79,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,8,61,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,12,41,21,3), new GeneratedEnemyUnit(-18,-5,10,18,4), new GeneratedEnemyUnit(17,-1,100,31,4), new GeneratedEnemyUnit(7,9,36,10,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "bd437a9b832302842ffb10a9f2b0517848311a33b0a044e16bed302eaec373e4");
        }

        private static void Case_05347()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5347,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,17,74,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-16,5,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,11,40,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,10,58,14,2), new GeneratedEnemyUnit(-10,-5,63,33,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "bdf097b1e05f2e311b76aaf064768a701100880d4b65a2d6199dfe396f47257d");
        }

        private static void Case_05348()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5348,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,2,25,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-11,17,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,5,40,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-10,71,3,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "69e016de6daf059ec04e9f7d41605386a3cc7e92010ced4e299f05326abdea95");
        }

        private static void Case_05349()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5349,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,20,18,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-20,85,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-2,31,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-11,27,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-10,69,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,4,30,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-19,54,7,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "5c11d9aca4f6510a23519ec0ef08837d2870814ff39112be0b8f38497745a6aa");
        }

        private static void Case_05350()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5350,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-14,70,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-11,53,45,4), new GeneratedEnemyUnit(-17,-17,92,47,3), new GeneratedEnemyUnit(-12,-1,78,32,2), new GeneratedEnemyUnit(-16,-18,14,32,3), new GeneratedEnemyUnit(9,-4,5,16,3), new GeneratedEnemyUnit(16,-18,89,45,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "443fb7c42f1482df668e1a3b76e5fffb893a9996dd5f0c08cedf41f5173d38cf");
        }

        private static void Case_05351()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5351,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-9,72,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,6,32,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,12,89,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,19,33,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,1,31,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-17,28,2,2), new GeneratedEnemyUnit(-14,13,53,46,1), new GeneratedEnemyUnit(18,-15,93,37,4), new GeneratedEnemyUnit(11,9,45,6,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "fc3cda668b4fcacca5ea57307442682ef42209b7d209bda20380ad1017e8e0df");
        }

        private static void Case_05352()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5352,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,7,13,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-14,85,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,3,47,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-1,91,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-12,95,11,4), new GeneratedEnemyUnit(12,-13,23,46,4), new GeneratedEnemyUnit(2,7,66,33,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "b23d9ba6de9684a1b82e08c59ce7c93d6e1ebaa915bf63ca0f6d73570c5a6eaf");
        }

        private static void Case_05353()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5353,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,1,38,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-13,92,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,1,83,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-9,75,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,20,56,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-6,59,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-18,18,28,1), new GeneratedEnemyUnit(-20,-5,7,33,3), new GeneratedEnemyUnit(-16,-18,80,46,3), new GeneratedEnemyUnit(-19,19,94,41,4), new GeneratedEnemyUnit(-5,-3,31,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "ddb2d0458740292c7c07e2a642ee0457e96338b54732837fd896ec016ad1cbfc");
        }

        private static void Case_05354()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5354,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-12,40,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-11,77,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,14,78,50,2), new GeneratedEnemyUnit(14,-6,60,27,4), new GeneratedEnemyUnit(18,2,70,40,2), new GeneratedEnemyUnit(-5,8,39,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "00aa6c87b1f63bdb8e0a8a8fbc13de39a9feed7044f5aaf86afeaa3fae949921");
        }

        private static void Case_05355()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5355,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-1,43,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-13,37,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-5,89,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,19,28,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-15,7,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-15,25,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-3,64,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,4,99,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,1,76,34,3), new GeneratedEnemyUnit(0,-8,51,14,4), new GeneratedEnemyUnit(-11,0,77,7,1), new GeneratedEnemyUnit(7,-14,97,1,2), new GeneratedEnemyUnit(-16,-19,19,4,4), new GeneratedEnemyUnit(-13,0,66,33,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "ed003aeb5475a42a309688fe78f1a169612509bd22c33e6f9ebbe6c19e4e056b");
        }

        private static void Case_05356()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5356,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-17,17,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-19,67,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-15,60,18,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "51d36598769cb8bfaa3c6194a9774612b9d5e48ce2666045b8f8a83e4fafe9e8");
        }

        private static void Case_05357()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5357,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-7,38,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,13,7,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-4,54,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,20,63,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-14,55,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-9,26,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-7,45,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "f5943bb68dcbabacd585f00f52e0f4ca98d5299f59713c3fa065db4f38a6ba85");
        }

        private static void Case_05358()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5358,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,7,69,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-17,85,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-12,39,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,1,24,26,3), new GeneratedEnemyUnit(-12,2,32,44,3), new GeneratedEnemyUnit(14,-18,67,33,4), new GeneratedEnemyUnit(-15,-1,11,47,1), new GeneratedEnemyUnit(13,15,86,34,1), new GeneratedEnemyUnit(16,2,26,44,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "d556bd8e76766a347ccc547c0433d96be1ae147d98f3b9093b69a309ce54fcda");
        }

        private static void Case_05359()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5359,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-14,50,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-16,15,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-14,89,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-11,16,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-10,97,14,4), new GeneratedEnemyUnit(19,-7,58,19,2), new GeneratedEnemyUnit(11,4,84,42,4), new GeneratedEnemyUnit(1,20,45,21,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "39406daf635745380afea14228682f48217641dd3cb59dc51b9beff59101197f");
        }

        private static void Case_05360()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5360,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-17,9,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,4,67,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,1,26,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-1,27,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,15,64,2,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "15536824a3f5ee53555129e27f80b4fd282e1597bff6ea7b5c7148a1c282cc69");
        }

        private static void Case_05361()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5361,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,13,65,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,0,43,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-20,80,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-6,29,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,1,37,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-13,6,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-6,50,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-20,84,37,3), new GeneratedEnemyUnit(3,-16,60,23,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "05684faa5d9996459a973e25a644d7993216107d5124e37cf445cba52f3e205f");
        }

        private static void Case_05362()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5362,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-11,34,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-9,14,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,9,87,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,11,75,6,4), new GeneratedEnemyUnit(0,18,54,45,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "c41811ec6f14519e0c0b3cc72dcce2cc89e8240d5e24a71bd98c683c587be532");
        }

        private static void Case_05363()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5363,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,17,65,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-17,99,16,3), new GeneratedEnemyUnit(-6,-19,69,15,3), new GeneratedEnemyUnit(-15,3,29,48,4), new GeneratedEnemyUnit(17,14,43,30,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "76050d627075c1d5dafc6217dccc582b82eead058e1884913f658d2280c79b21");
        }

        private static void Case_05364()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5364,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,10,93,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-6,15,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-6,47,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,14,20,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,8,47,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-10,96,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-7,27,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-5,38,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-5,12,34,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "96bfec424aa4dcc7775df249da0b83410e6b82162734f61097488ee0701ced48");
        }

        private static void Case_05365()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5365,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,6,63,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,5,70,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-20,98,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-2,26,16,3), new GeneratedEnemyUnit(3,15,89,11,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "5aab9dff77e53ae6e7699e233bbf4d6143940047ac6e16ae2382ef2f2249368b");
        }

        private static void Case_05366()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5366,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-20,98,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-2,5,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,11,7,17,2), new GeneratedEnemyUnit(-20,-7,39,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b1d244642c7afb6bcffab8c3a83cb087e72bb1f7e536b30ff74421c23f20e872");
        }

        private static void Case_05367()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5367,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-4,15,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-19,56,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,14,87,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-13,17,24,1), new GeneratedEnemyUnit(-19,-19,88,49,1), new GeneratedEnemyUnit(1,-17,81,16,2), new GeneratedEnemyUnit(-2,-8,17,20,2), new GeneratedEnemyUnit(-18,18,26,3,4), new GeneratedEnemyUnit(19,-8,22,2,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "4420536729c87dbfb43aeb7ca362aa664bd1383a89562fd6f68cf3f20590e6bb");
        }

        private static void Case_05368()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5368,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-5,62,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,2,49,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,5,37,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,15,66,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,20,99,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,18,13,6,1), new GeneratedEnemyUnit(13,5,64,32,4), new GeneratedEnemyUnit(-1,-5,68,34,2), new GeneratedEnemyUnit(19,-18,50,32,3), new GeneratedEnemyUnit(-14,-1,38,48,2), new GeneratedEnemyUnit(-9,14,82,39,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "f6d2b123a4b6028388bc40114f9cf9acc18201ec4b27f85388a8d5aa862e845a");
        }

        private static void Case_05369()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5369,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-17,53,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,2,41,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,12,12,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-16,29,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,10,87,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-16,63,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,11,86,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-18,84,4,3), new GeneratedEnemyUnit(-1,15,61,4,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "07db131922a8453eb21fe0fe7225f355bad79eefadd18026b03b8a029b3436d5");
        }

        private static void Case_05370()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5370,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,9,87,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-17,38,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-9,98,37,2), new GeneratedEnemyUnit(-4,13,59,47,1), new GeneratedEnemyUnit(5,5,14,41,2), new GeneratedEnemyUnit(-9,-1,82,32,3), new GeneratedEnemyUnit(2,-5,99,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "bc8766e6589b993c60c6fd9a7df7d178e65ae4ea1b3f78ca4313a83eee469af7");
        }

        private static void Case_05371()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5371,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,14,38,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,18,78,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,5,82,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,2,57,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-19,45,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,18,82,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-10,62,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,16,75,3,4), new GeneratedEnemyUnit(-14,0,53,7,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "23314fc3b7c7112445ef8f7b1071521ccdf93fa2b74637f594b5b9bbec4eddc5");
        }

        private static void Case_05372()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5372,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-9,57,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,13,76,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,16,52,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,4,39,26,2), new GeneratedEnemyUnit(1,-20,42,28,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "51a753b068f8b350945d2c417dc004a23ab9739917ba379437ad4f512b037c0a");
        }

        private static void Case_05373()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5373,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,4,61,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,14,82,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,1,38,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-1,9,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-18,52,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,16,39,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-3,58,34,2), new GeneratedEnemyUnit(13,-14,47,17,4), new GeneratedEnemyUnit(2,10,84,23,4), new GeneratedEnemyUnit(6,-5,64,12,3), new GeneratedEnemyUnit(20,-1,86,36,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "f790fe797013fafba5fa2f8d15106109c291e9331d38cc668d025d5f708d6ac7");
        }

        private static void Case_05374()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5374,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-9,10,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-13,48,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,14,49,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,15,83,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-6,31,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-4,47,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-19,34,9,1), new GeneratedEnemyUnit(19,9,90,21,4), new GeneratedEnemyUnit(-5,-12,49,31,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "991137b6588195b20da51a767d9aa2dd5e6b7443c6bc5fe37aea1883b6000e10");
        }

        private static void Case_05375()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5375,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-12,100,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-7,89,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,20,30,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,9,10,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,20,50,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-11,40,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,14,97,14,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a97e745523e12af08a56a7bc6d5b194e2c66a41964d7c20bd64336da4fe7241b");
        }

        private static void Case_05376()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5376,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,6,53,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-12,56,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-1,43,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-7,31,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,3,75,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,7,11,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-13,20,47,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "8412ff6b3d258901292d26a42633db68c1bfdfee940c645c831dc3cf112b5619");
        }

        private static void Case_05377()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5377,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,12,56,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-15,28,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-7,74,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,18,8,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,1,52,12,3), new GeneratedEnemyUnit(-2,-1,79,10,4), new GeneratedEnemyUnit(8,6,14,23,1), new GeneratedEnemyUnit(-10,14,43,4,3), new GeneratedEnemyUnit(-17,-17,91,42,3), new GeneratedEnemyUnit(-11,7,78,15,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "0e4f13d8ecb3ac44283293050d372eb6ef071d0c086873afc9f0858ef0388acf");
        }

        private static void Case_05378()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5378,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,16,42,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-2,92,3,3), new GeneratedEnemyUnit(13,10,10,2,1), new GeneratedEnemyUnit(-2,18,9,44,4), new GeneratedEnemyUnit(-9,14,16,43,4), new GeneratedEnemyUnit(13,5,95,42,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "106d69a55404440db82aeb68193b85c6e67e134aba7dd9d3ed68832922635a39");
        }

        private static void Case_05379()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5379,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-13,34,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,13,25,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,3,73,48,4), new GeneratedEnemyUnit(5,-10,93,3,1), new GeneratedEnemyUnit(-7,-3,87,35,1), new GeneratedEnemyUnit(17,-16,55,37,2), new GeneratedEnemyUnit(20,-19,90,30,2), new GeneratedEnemyUnit(-1,-10,9,32,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "9aec6fca044d210753fa361e11a0f7d0fb75b352686ca9f655f5abdc4449bfa5");
        }

        private static void Case_05380()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5380,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-3,62,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-19,82,2,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "61967f2088b6efff7218ba434a7c00bc859393c2efa96f24ae657af0dbfcc150");
        }

        private static void Case_05381()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5381,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,16,59,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,4,51,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,11,57,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-10,72,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-7,83,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,14,38,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,16,51,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-19,36,48,2), new GeneratedEnemyUnit(8,-4,60,28,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "2714cfdbec7b4fdcfc6d9c1d9dd3d1724738d0ae9267ce75c15d89e701a7b0ba");
        }

        private static void Case_05382()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5382,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-6,27,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,9,81,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-4,65,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-19,89,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,7,49,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-5,15,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,2,93,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,3,82,49,2), new GeneratedEnemyUnit(6,-19,89,2,3), new GeneratedEnemyUnit(9,1,51,21,3), new GeneratedEnemyUnit(-2,20,73,46,1), new GeneratedEnemyUnit(10,-7,68,1,1), new GeneratedEnemyUnit(4,-7,69,28,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "af0fe57232f68b3a133e518f21a878681be72da5e63f8e46fe4bb7ed56d5d27a");
        }

        private static void Case_05383()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5383,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-7,57,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,5,81,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-2,77,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,5,94,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,20,26,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-16,21,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-4,56,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,6,54,4,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "19c0189bac72107e21f0ad9012bcf24525baec0f64e94ad3dc16160934004915");
        }

        private static void Case_05384()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5384,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-17,17,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,8,69,33,3), new GeneratedEnemyUnit(14,16,96,34,4), new GeneratedEnemyUnit(17,-1,76,18,2), new GeneratedEnemyUnit(-18,-7,23,47,4), new GeneratedEnemyUnit(-16,7,33,12,3), new GeneratedEnemyUnit(-2,10,38,16,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "44d579d478e5be9120aefe059ff0645490b873adef34ff48244f90ff75481dae");
        }

        private static void Case_05385()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5385,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,8,29,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,3,89,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-8,100,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-9,26,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,4,25,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-4,5,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-10,27,2,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "996ee7830ba99d79d120e212c8d7bcd966fdb5428ea93c3aa2cdcd2b9dedf6b0");
        }

        private static void Case_05386()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5386,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,18,89,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,18,7,21,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "da2ea3854e4faec3932f5ae9ce80dd9a7b36305241377d028e5b8e32090c849b");
        }

        private static void Case_05387()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5387,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,6,40,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-10,65,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,12,15,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-1,35,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,17,30,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-15,43,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,13,15,4,2), new GeneratedEnemyUnit(10,-2,66,4,4), new GeneratedEnemyUnit(3,-3,79,5,2), new GeneratedEnemyUnit(17,12,96,7,1), new GeneratedEnemyUnit(5,-3,72,45,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "17d5ba0bf7424a49867a50e34304c9a83ba246edf18145a209a758197604fced");
        }

        private static void Case_05388()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5388,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,5,31,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,10,96,8,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "0540f7266d4414920d578505fb4a18ea6e4be8f8038dad6f90742fa7be545aa5");
        }

        private static void Case_05389()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5389,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,2,61,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,17,24,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,12,30,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-17,32,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-9,38,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,2,13,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,6,45,24,4), new GeneratedEnemyUnit(-2,-7,73,8,4), new GeneratedEnemyUnit(-3,3,39,41,3), new GeneratedEnemyUnit(9,3,34,25,1), new GeneratedEnemyUnit(-2,16,57,20,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "5f37f9bae813811a182e27c51159557e0f1908e0ce85550f3ef959213241574f");
        }

        private static void Case_05390()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5390,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,6,11,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-2,59,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,6,30,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-7,67,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-18,28,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-18,75,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,2,17,31,1), new GeneratedEnemyUnit(3,5,98,7,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "3831b55e5e3ba1c4e1779838938dffb0ce845ce841db96bdb70d1b5443e40f07");
        }

        private static void Case_05391()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5391,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,18,94,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-12,69,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-19,67,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-13,29,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-19,70,2,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "6ee81b436e1626541fd4adc388edc42aa7fc897e516cba11bb1c011e99d73e03");
        }

        private static void Case_05392()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5392,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-16,63,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,7,100,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,12,89,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-13,46,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,8,14,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,9,91,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-8,96,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,4,7,38,4), new GeneratedEnemyUnit(-19,19,51,5,2), new GeneratedEnemyUnit(-7,4,88,20,2), new GeneratedEnemyUnit(-4,12,12,26,2), new GeneratedEnemyUnit(-1,-18,75,46,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "9eb75c576b9efa5fb1b120dfaa3bdc3aaebd10896188b335cb08f2e61bf3130b");
        }

        private static void Case_05393()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5393,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,5,63,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,16,29,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-5,18,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-10,34,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,10,89,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-9,41,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,15,38,24,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "490a5453dd83165cfed62a317fcc731241036c2143dfa42194c8b8d60932c541");
        }

        private static void Case_05394()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5394,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,15,50,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-3,74,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,9,30,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-8,35,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-2,22,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,11,54,1,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "4089fde85a4ace0b7d71125a768c8937b9fe93cf4be023d0e18c1c36d8b96900");
        }

        private static void Case_05395()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5395,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,6,30,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,18,40,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,1,54,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,14,16,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-2,41,9,4), new GeneratedEnemyUnit(-15,19,93,2,3), new GeneratedEnemyUnit(-12,-20,10,13,1), new GeneratedEnemyUnit(9,1,75,35,4), new GeneratedEnemyUnit(-2,-11,70,6,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "a55e333aae00dd9fafb10a646474c02b54dc336fa145a64d19fe281fcdc0fbbb");
        }

        private static void Case_05396()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5396,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-7,13,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,5,95,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-6,37,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-12,47,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,19,88,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,6,86,30,1), new GeneratedEnemyUnit(-3,-17,64,47,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "e0a3e3836e7465e63f39711b6a8cb57047239bb2fc91c5618acd146ead4a6a64");
        }

        private static void Case_05397()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5397,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,17,8,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,13,32,40,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "1511c42d605e2f1dbfb4f49a28bfbee59b772c027731832979abbc35b98e7749");
        }

        private static void Case_05398()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5398,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-15,32,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,1,12,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,15,38,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,10,74,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-4,91,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,19,81,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-7,35,28,4), new GeneratedEnemyUnit(9,-4,68,33,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "41edb261d281b8edb0ce30b1869f5d53c57132f3bef64b9e4c3745418a559a4e");
        }

        private static void Case_05399()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5399,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-6,11,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,19,72,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,0,36,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-3,78,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,19,16,49,3), new GeneratedEnemyUnit(15,2,92,39,4), new GeneratedEnemyUnit(-18,11,41,30,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "352346353dfcb5b7c9c9f4a68b3fd2d54fe3acefd67844ebeb93a4cc2aa00f5f");
        }

    }
}
