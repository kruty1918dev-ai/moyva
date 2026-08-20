using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard017
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_03060();
            Case_03061();
            Case_03062();
            Case_03063();
            Case_03064();
            Case_03065();
            Case_03066();
            Case_03067();
            Case_03068();
            Case_03069();
            Case_03070();
            Case_03071();
            Case_03072();
            Case_03073();
            Case_03074();
            Case_03075();
            Case_03076();
            Case_03077();
            Case_03078();
            Case_03079();
            Case_03080();
            Case_03081();
            Case_03082();
            Case_03083();
            Case_03084();
            Case_03085();
            Case_03086();
            Case_03087();
            Case_03088();
            Case_03089();
            Case_03090();
            Case_03091();
            Case_03092();
            Case_03093();
            Case_03094();
            Case_03095();
            Case_03096();
            Case_03097();
            Case_03098();
            Case_03099();
            Case_03100();
            Case_03101();
            Case_03102();
            Case_03103();
            Case_03104();
            Case_03105();
            Case_03106();
            Case_03107();
            Case_03108();
            Case_03109();
            Case_03110();
            Case_03111();
            Case_03112();
            Case_03113();
            Case_03114();
            Case_03115();
            Case_03116();
            Case_03117();
            Case_03118();
            Case_03119();
            Case_03120();
            Case_03121();
            Case_03122();
            Case_03123();
            Case_03124();
            Case_03125();
            Case_03126();
            Case_03127();
            Case_03128();
            Case_03129();
            Case_03130();
            Case_03131();
            Case_03132();
            Case_03133();
            Case_03134();
            Case_03135();
            Case_03136();
            Case_03137();
            Case_03138();
            Case_03139();
            Case_03140();
            Case_03141();
            Case_03142();
            Case_03143();
            Case_03144();
            Case_03145();
            Case_03146();
            Case_03147();
            Case_03148();
            Case_03149();
            Case_03150();
            Case_03151();
            Case_03152();
            Case_03153();
            Case_03154();
            Case_03155();
            Case_03156();
            Case_03157();
            Case_03158();
            Case_03159();
            Case_03160();
            Case_03161();
            Case_03162();
            Case_03163();
            Case_03164();
            Case_03165();
            Case_03166();
            Case_03167();
            Case_03168();
            Case_03169();
            Case_03170();
            Case_03171();
            Case_03172();
            Case_03173();
            Case_03174();
            Case_03175();
            Case_03176();
            Case_03177();
            Case_03178();
            Case_03179();
            Case_03180();
            Case_03181();
            Case_03182();
            Case_03183();
            Case_03184();
            Case_03185();
            Case_03186();
            Case_03187();
            Case_03188();
            Case_03189();
            Case_03190();
            Case_03191();
            Case_03192();
            Case_03193();
            Case_03194();
            Case_03195();
            Case_03196();
            Case_03197();
            Case_03198();
            Case_03199();
            Case_03200();
            Case_03201();
            Case_03202();
            Case_03203();
            Case_03204();
            Case_03205();
            Case_03206();
            Case_03207();
            Case_03208();
            Case_03209();
            Case_03210();
            Case_03211();
            Case_03212();
            Case_03213();
            Case_03214();
            Case_03215();
            Case_03216();
            Case_03217();
            Case_03218();
            Case_03219();
            Case_03220();
            Case_03221();
            Case_03222();
            Case_03223();
            Case_03224();
            Case_03225();
            Case_03226();
            Case_03227();
            Case_03228();
            Case_03229();
            Case_03230();
            Case_03231();
            Case_03232();
            Case_03233();
            Case_03234();
            Case_03235();
            Case_03236();
            Case_03237();
            Case_03238();
            Case_03239();
        }

        private static void Case_03060()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3060,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,18,32,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-16,13,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-10,93,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,15,10,5,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "5867f8175244b5ca3abe57d73c88dcfdbd650ea7b177014189e8084af78c5dad");
        }

        private static void Case_03061()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3061,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-14,73,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-17,8,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,0,23,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,16,93,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,11,35,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-17,73,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,19,29,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,18,40,39,2), new GeneratedEnemyUnit(-7,-5,31,29,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "b669e716663dd747db3284a28f3dfe9787d1bbfb298cbed0e412357365886675");
        }

        private static void Case_03062()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3062,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-14,6,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,8,6,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-1,43,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-19,38,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,9,91,25,1), new GeneratedEnemyUnit(-6,4,81,7,4), new GeneratedEnemyUnit(-17,-10,33,41,4), new GeneratedEnemyUnit(20,-13,97,28,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "3c67ab6352add87ca7152cd8aca16932c46944b1555b81bf9c00f40924a2d697");
        }

        private static void Case_03063()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3063,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-19,11,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,16,17,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,7,100,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-17,54,9,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "f5ca5e7e84b54c4701290a0e95aa38420a6ec5c907ae52b978ab970da073f2d5");
        }

        private static void Case_03064()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3064,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-11,99,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-2,83,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-17,21,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-16,44,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,19,83,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,5,52,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-16,86,49,4), new GeneratedEnemyUnit(1,13,56,6,3), new GeneratedEnemyUnit(-9,4,42,40,3), new GeneratedEnemyUnit(-17,5,18,30,2), new GeneratedEnemyUnit(-2,-3,5,21,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "f113566c4adf1ccc04f8222816b8fc350b34a718523251ce73cb25a11582db89");
        }

        private static void Case_03065()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3065,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-1,98,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,4,30,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-9,43,30,1), new GeneratedEnemyUnit(-17,-1,52,7,4), new GeneratedEnemyUnit(-14,1,57,14,4), new GeneratedEnemyUnit(8,-18,96,5,4), new GeneratedEnemyUnit(12,16,64,36,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "b8e269ba95ad9666e3a2b944d3edafae056fd7113609adebd33b8de59b858059");
        }

        private static void Case_03066()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3066,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-11,96,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-7,19,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-9,41,20,3), new GeneratedEnemyUnit(-14,9,67,3,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "a18badb2241321314cb4b1493f75240e373ab4bfe770a6921266a0b64c49e53f");
        }

        private static void Case_03067()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3067,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,13,12,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,2,50,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,15,33,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,7,18,47,3), new GeneratedEnemyUnit(-9,-17,99,4,1), new GeneratedEnemyUnit(-18,13,58,21,2), new GeneratedEnemyUnit(12,-6,16,8,3), new GeneratedEnemyUnit(14,5,50,9,3), new GeneratedEnemyUnit(-9,16,92,28,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5b238b553c15ec67455313235d1b79b2e26e3470922c8d009a63b524ceb68842");
        }

        private static void Case_03068()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3068,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,3,78,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-15,24,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-17,59,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-18,27,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-20,39,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-20,86,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,10,44,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,5,33,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,17,59,18,2), new GeneratedEnemyUnit(-7,-9,18,19,1), new GeneratedEnemyUnit(-2,-12,42,31,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "8b212ee8ec164af125477e8d22a2504dbc8dd904f47879cdb0a88e2599eb9ee0");
        }

        private static void Case_03069()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3069,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,3,12,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,2,85,1,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "71e8227475554fa137003bff11a557c35a32b92e375f4b6faa7f565b4f7c06a0");
        }

        private static void Case_03070()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3070,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-17,35,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,20,89,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,12,93,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,6,98,37,2), new GeneratedEnemyUnit(-11,-7,54,11,1), new GeneratedEnemyUnit(9,-9,52,47,2), new GeneratedEnemyUnit(10,-8,6,34,3), new GeneratedEnemyUnit(15,15,19,46,3), new GeneratedEnemyUnit(-16,-2,17,49,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "c219880bab43f0ef80f589d479bc8edffebf2da60eb0aa7651d53758c57ce256");
        }

        private static void Case_03071()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3071,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-9,36,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,15,22,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,2,11,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-11,29,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-14,12,30,3), new GeneratedEnemyUnit(1,3,41,47,2), new GeneratedEnemyUnit(-17,2,13,15,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "52ece4a439dd5094ef64c28783917e7eb458879651d61ff9c388239df9171e05");
        }

        private static void Case_03072()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3072,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-7,97,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-14,97,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,0,36,42,4), new GeneratedEnemyUnit(-1,12,81,11,2), new GeneratedEnemyUnit(6,-14,62,4,1), new GeneratedEnemyUnit(-16,0,73,8,3), new GeneratedEnemyUnit(-7,-20,57,7,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "fa0fd4e7027457684b6b72481012fbca2d00c54220f91e009a74aefa96e70521");
        }

        private static void Case_03073()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3073,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,8,34,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,20,26,31,4), new GeneratedEnemyUnit(-14,-4,60,41,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "fab051751f7c28f8d56e3d69fb9742494c5ca2d66a7786efa73400cea7eeff28");
        }

        private static void Case_03074()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3074,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-19,31,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,14,37,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-18,99,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-17,25,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-10,62,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-18,90,2,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c777665d7f0aa01ddc4b39886bdd13ec44f650212abd96908a57f62f92d2bf17");
        }

        private static void Case_03075()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3075,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,11,7,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-5,96,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,17,17,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-15,23,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,5,45,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-12,18,34,4), new GeneratedEnemyUnit(-4,14,41,50,4), new GeneratedEnemyUnit(0,-2,67,19,4), new GeneratedEnemyUnit(-10,8,100,15,4), new GeneratedEnemyUnit(16,-5,32,4,1), new GeneratedEnemyUnit(5,15,67,5,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "43080fcff1b4c887a58678b0cbc32420f96c5214afe481c7e9668a8cb1402178");
        }

        private static void Case_03076()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3076,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-8,51,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-13,89,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,18,35,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-19,89,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,19,85,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,2,89,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,5,39,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,17,46,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-4,96,50,2), new GeneratedEnemyUnit(0,-10,73,39,2), new GeneratedEnemyUnit(-8,-15,61,16,1), new GeneratedEnemyUnit(-10,-18,70,42,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8ea0a97ab3f5a94dbaccd11fe4ff6ff0dc02fc79d86268e5f05c3d01548c4af8");
        }

        private static void Case_03077()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3077,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-8,51,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,14,52,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,10,12,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-2,47,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-5,97,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,6,6,23,3), new GeneratedEnemyUnit(5,16,26,21,2), new GeneratedEnemyUnit(-14,20,7,23,4), new GeneratedEnemyUnit(2,13,72,20,1), new GeneratedEnemyUnit(17,-5,76,10,3), new GeneratedEnemyUnit(19,-6,32,26,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "11aee6018eb2d66831ffc5f4be155238046a74f3beb7a309d5aef56e1a20d2e6");
        }

        private static void Case_03078()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3078,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,10,89,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-11,56,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,17,59,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,1,29,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-10,8,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-6,88,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,7,63,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-1,17,15,2), new GeneratedEnemyUnit(14,12,60,9,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "01d8b1add56463f906e8b2dea33e7c65aa734ddaaefd3c33a03984935a9ba3d1");
        }

        private static void Case_03079()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3079,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,12,92,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,16,71,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,17,47,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,14,86,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,7,36,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-20,51,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-15,40,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-20,15,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,3,97,9,3), new GeneratedEnemyUnit(11,12,69,11,1), new GeneratedEnemyUnit(10,17,39,29,2), new GeneratedEnemyUnit(-20,-10,14,17,1), new GeneratedEnemyUnit(-11,5,61,37,1), new GeneratedEnemyUnit(6,18,29,30,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "7ac0143d258dda13e63d9e9bdccbaa25a3d65d1cc62af3c2b37e8405ccf4085d");
        }

        private static void Case_03080()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3080,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-9,66,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,11,56,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,3,84,21,2), new GeneratedEnemyUnit(9,-14,89,33,4), new GeneratedEnemyUnit(-5,-4,95,18,2), new GeneratedEnemyUnit(8,-12,8,19,1), new GeneratedEnemyUnit(-14,-3,26,50,1), new GeneratedEnemyUnit(6,13,74,9,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "ee4091b18787731893b4493eb2d2f2eb065c68c6f196697cf8f2fc3e7259f93b");
        }

        private static void Case_03081()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3081,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,3,89,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-4,33,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-19,56,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-1,40,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,8,92,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-18,9,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,13,22,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,11,7,4,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "91ac545867f2593772569c0a3ee5d157abd9749e7adde30e748287b5695c2990");
        }

        private static void Case_03082()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3082,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,18,19,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,14,37,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,7,64,31,1), new GeneratedEnemyUnit(-19,7,23,25,1), new GeneratedEnemyUnit(18,7,58,24,3), new GeneratedEnemyUnit(19,17,39,31,2), new GeneratedEnemyUnit(1,5,19,32,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "98acb355e2a622dc5d2f900233247556eb9567aff509aff426eb7e78972b3302");
        }

        private static void Case_03083()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3083,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,8,75,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-14,54,19,3), new GeneratedEnemyUnit(17,4,79,39,3), new GeneratedEnemyUnit(-16,9,96,45,4), new GeneratedEnemyUnit(-3,16,88,21,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "fc5efddddea97dbef61c59e769edd65d421dfb3a5cd837065dfddf347dc01c18");
        }

        private static void Case_03084()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3084,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,5,68,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-19,14,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-10,25,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-15,39,43,1), new GeneratedEnemyUnit(9,-3,7,2,2), new GeneratedEnemyUnit(-20,14,20,23,4), new GeneratedEnemyUnit(20,-19,39,29,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "296f6342a0a1a008f1a756910ee02c884f1ce83ba5a1ef89aa4cfd8ac27ee5cc");
        }

        private static void Case_03085()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3085,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,7,16,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,15,27,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-19,94,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,1,42,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-16,78,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-15,40,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,16,79,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-7,47,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-3,88,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "4db37ea19876c0eb105c446f6accf57c20a7af0aa510495d8d06ea79cca95107");
        }

        private static void Case_03086()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3086,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,7,66,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-17,85,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-7,36,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-7,23,7,2), new GeneratedEnemyUnit(-7,14,72,48,2), new GeneratedEnemyUnit(-20,5,9,14,4), new GeneratedEnemyUnit(-7,1,96,42,1), new GeneratedEnemyUnit(1,-20,69,35,3), new GeneratedEnemyUnit(6,20,74,20,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "28f5c5ff5b128250f35dc1b271434a1dd2c73d64414107675d076230deed8d6a");
        }

        private static void Case_03087()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3087,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-10,33,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-18,24,34,3), new GeneratedEnemyUnit(-19,-6,87,47,1), new GeneratedEnemyUnit(14,-17,80,23,3), new GeneratedEnemyUnit(-20,2,32,49,2), new GeneratedEnemyUnit(6,3,67,41,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "3b846d51586311c394a9391432c64bbe6e99f3d39cbc5e3b148f96546d588af6");
        }

        private static void Case_03088()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3088,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-5,70,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,9,39,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-7,65,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,18,42,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-20,48,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-4,15,20,3), new GeneratedEnemyUnit(-5,1,44,2,3), new GeneratedEnemyUnit(18,1,15,23,2), new GeneratedEnemyUnit(-12,20,15,18,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8f4658680b3d33b688df9ecc3a043f872c0b4f463329566aa9de488a6b1625f2");
        }

        private static void Case_03089()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3089,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,6,59,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,18,53,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-13,65,20,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "39ab4c097ee84f587de7ea479d7f6f5069dfe622f1fba0437112cba83df00e82");
        }

        private static void Case_03090()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3090,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,15,96,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-16,36,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,15,34,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-11,26,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,2,36,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,17,59,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,19,39,6,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "e38ed33519986b59c12b19d57aab3f7ba712550fa28d8282e5a2588c6c53331a");
        }

        private static void Case_03091()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3091,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-3,41,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-4,37,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,3,82,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,13,19,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,2,53,2,1), new GeneratedEnemyUnit(2,1,24,43,4), new GeneratedEnemyUnit(-20,-17,31,14,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "bb112a8e08fa0dacb964ac20e2f97a4ba9c7a614d3dd8262c499b629434ae79c");
        }

        private static void Case_03092()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3092,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-18,21,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,13,41,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-6,24,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-12,71,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,8,87,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-6,15,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,10,27,4,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "7a5c5d3b3f5b82682d65802139a31e8200f6466407d774228bb8945d0a35c810");
        }

        private static void Case_03093()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3093,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,18,38,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-20,53,10,4), new GeneratedEnemyUnit(19,0,36,2,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "f0dd43b9f0ef96f05bfd18b1025566c28b5cffce1b75bf367757bf381fbaeb33");
        }

        private static void Case_03094()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3094,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-13,15,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,19,6,12,3), new GeneratedEnemyUnit(-15,12,98,9,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "44a5cdca2d25b945577824e93eb62e07222d55adcc97b5a4a1ce930172e60d66");
        }

        private static void Case_03095()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3095,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-16,69,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,19,86,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,2,82,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,11,28,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,17,99,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-12,25,10,4), new GeneratedEnemyUnit(-18,-3,13,40,1), new GeneratedEnemyUnit(2,4,18,36,4), new GeneratedEnemyUnit(0,20,77,23,2), new GeneratedEnemyUnit(-17,-6,41,39,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "5c7b761e39283ac75902445207d09d697038c7ea2868219b91b4d08a85c4e4ff");
        }

        private static void Case_03096()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3096,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-18,52,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,16,71,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,20,31,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,3,44,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-20,41,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-14,18,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,8,53,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,14,34,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,1,85,48,2), new GeneratedEnemyUnit(-3,-8,16,50,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "a7ba196b7cdca65b37b66c489880fcab6fbe8721665675c0261fd45283d19f59");
        }

        private static void Case_03097()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3097,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,5,85,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-8,74,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,2,5,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-9,73,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,18,72,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-20,99,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-9,41,2,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "810ae3da04e37d222aa98f40ce92672d1f7079dc433fea33b6568378cd696dd0");
        }

        private static void Case_03098()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3098,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,6,30,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,17,12,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,18,11,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-6,74,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "d04dbcaa186ff0c8d5cac56fd18179457bec722aef7978ef58177aca514958d8");
        }

        private static void Case_03099()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3099,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-7,15,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-6,76,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-1,22,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,0,11,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,9,92,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-2,47,2,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "d1ce572357df6bce9cfaaac67c5b0b4a102851747a34b9ef8d7d134a968c9bde");
        }

        private static void Case_03100()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3100,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-11,13,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-7,9,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,5,89,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-1,7,33,1), new GeneratedEnemyUnit(-1,-15,51,7,1), new GeneratedEnemyUnit(20,20,88,40,3), new GeneratedEnemyUnit(-10,8,39,12,2), new GeneratedEnemyUnit(-8,7,99,13,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "ea4d8f09309aa6b774ee9cd84e338b96a84c3cb19296412d56503f6eb3252349");
        }

        private static void Case_03101()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3101,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-6,57,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-6,39,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,17,30,1,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "ca42f29439e2fecd803b23319ac4ef44693b3997415bbf2552d3883c6dbcfcb0");
        }

        private static void Case_03102()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3102,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-18,9,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-18,73,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-5,51,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-4,21,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,18,70,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,10,7,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,20,78,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,1,38,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-11,51,25,1), new GeneratedEnemyUnit(0,-15,43,36,4), new GeneratedEnemyUnit(7,0,28,40,3), new GeneratedEnemyUnit(-19,-17,5,41,1), new GeneratedEnemyUnit(-11,-1,29,16,4), new GeneratedEnemyUnit(2,8,11,49,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 35,
                stableHash: "42a1aa04f5be85f3c35a63ac64a62c8f68b7cb26cb59b29a87c3dd658170c2a6");
        }

        private static void Case_03103()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3103,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-11,58,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,3,24,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,12,51,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-5,46,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,9,9,3,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "4fc3010b6e7d2511048be8b0b78b1ac58e736473c3dcdd499a5e274509b27312");
        }

        private static void Case_03104()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3104,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,2,9,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-11,48,3,2), new GeneratedEnemyUnit(-18,-5,39,3,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "87fe5fbc696f4ce20295a4d38ed1e9e9caf9943f052956b3eba27c9716457b54");
        }

        private static void Case_03105()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3105,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,0,82,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-5,38,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-11,71,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,5,11,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,4,40,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-10,78,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,6,97,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,0,95,12,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "d095c4150501035eae3f9c668d6afa49e02b81b832042daa90ff384e5559cc32");
        }

        private static void Case_03106()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3106,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,16,18,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-1,39,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-10,44,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,9,86,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-20,6,28,3), new GeneratedEnemyUnit(12,-2,30,4,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0176e984a269b0c2f6c76ecd61f0e175621b2be711bd29d1f29ebcefe4ba9b29");
        }

        private static void Case_03107()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3107,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,19,36,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,9,5,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,9,48,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,13,33,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-4,52,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,13,44,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-5,38,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,10,49,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-18,85,26,4), new GeneratedEnemyUnit(-16,0,60,39,3), new GeneratedEnemyUnit(16,17,32,17,3), new GeneratedEnemyUnit(-6,-15,56,5,4), new GeneratedEnemyUnit(13,-5,43,29,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "9b6447165b0612cf31b07fe705bea52e975890e035e8c43a28bfd2a535430687");
        }

        private static void Case_03108()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3108,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-1,29,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,19,49,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-19,94,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,8,28,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-5,99,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-2,49,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-18,78,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,19,44,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,13,80,25,4), new GeneratedEnemyUnit(16,4,10,30,2), new GeneratedEnemyUnit(3,13,59,13,1), new GeneratedEnemyUnit(-14,-14,74,49,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "8f0c11c92cf6028b30949791aad932e9fd009ef302fdd5652dfdbe596ec6cfbf");
        }

        private static void Case_03109()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3109,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,19,55,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,8,92,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,13,47,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-17,22,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,15,18,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-11,88,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-8,78,29,1), new GeneratedEnemyUnit(11,-17,41,37,2), new GeneratedEnemyUnit(-16,-14,27,16,4), new GeneratedEnemyUnit(-18,17,52,43,4), new GeneratedEnemyUnit(11,-1,6,4,3), new GeneratedEnemyUnit(9,-2,94,32,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "b4007c849acd839c6149d7231ce1716c691afa434f34d70c900999a29485f084");
        }

        private static void Case_03110()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3110,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-7,62,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-11,19,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-6,8,5,1), new GeneratedEnemyUnit(-20,-16,38,3,4), new GeneratedEnemyUnit(-20,5,68,19,4), new GeneratedEnemyUnit(18,-4,41,42,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "16f72c1c450c95bcbe8393d76f1a2a1e8a9aaee5fb8252ae26b758f164639228");
        }

        private static void Case_03111()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3111,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,20,49,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,3,31,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,2,54,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,11,27,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-17,98,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-5,43,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,13,51,1,1), new GeneratedEnemyUnit(-8,-9,98,38,1), new GeneratedEnemyUnit(11,4,71,39,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "d2d8ef5bbfd8feb9c8a458a7741605d849e7f8f152638bf7feebe00be122426b");
        }

        private static void Case_03112()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3112,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,3,91,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,13,74,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-20,37,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,19,26,49,2), new GeneratedEnemyUnit(10,15,53,32,1), new GeneratedEnemyUnit(-2,14,72,21,3), new GeneratedEnemyUnit(-5,4,40,20,1), new GeneratedEnemyUnit(-10,10,38,15,4), new GeneratedEnemyUnit(7,0,60,23,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "03d4cce054906644c892e58ff27fcd57f3fd35f6ff41a14268bd37b978567716");
        }

        private static void Case_03113()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3113,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,1,85,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,10,84,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-3,10,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,15,32,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-18,37,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-2,26,14,4), new GeneratedEnemyUnit(12,-12,11,29,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "4e735625bd8c7ef8539fbb45681cc4775ee22a3288cf6af80ec326c067c4452a");
        }

        private static void Case_03114()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3114,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,17,53,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,0,68,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-15,51,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-12,91,24,3), new GeneratedEnemyUnit(19,-1,85,2,3), new GeneratedEnemyUnit(10,-1,17,24,4), new GeneratedEnemyUnit(14,-19,83,25,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "dce9fac27d274fe2aa687e2be0814ec30efbbba0635c0e1f3dd30ebde5270abe");
        }

        private static void Case_03115()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3115,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-16,96,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-13,9,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,12,80,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,17,89,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-16,19,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,3,30,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-17,15,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-8,46,40,1), new GeneratedEnemyUnit(-9,-14,89,48,2), new GeneratedEnemyUnit(12,-8,82,3,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "0fb7760a2abbf38885e87a4a09e0fd7b5e2d6391748f85fe48896d59a2b29d0c");
        }

        private static void Case_03116()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3116,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-7,12,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,16,35,47,1), new GeneratedEnemyUnit(-7,-19,22,4,2), new GeneratedEnemyUnit(20,11,92,21,1), new GeneratedEnemyUnit(10,-13,99,7,1), new GeneratedEnemyUnit(11,-20,5,15,1), new GeneratedEnemyUnit(-17,-7,18,13,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "18760de852fbb3ef81a80c641f72949658c96dfc2172863130f4da7e5f20687f");
        }

        private static void Case_03117()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3117,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-4,20,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,20,20,39,2), new GeneratedEnemyUnit(-19,13,62,42,2), new GeneratedEnemyUnit(6,2,71,33,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "4026f4cb8047ec3ac349f104cf44410b86a9c31c736caff1558f8972668992d1");
        }

        private static void Case_03118()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3118,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,7,37,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,18,40,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,11,14,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-10,50,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,15,81,33,4), new GeneratedEnemyUnit(-13,-9,63,47,3), new GeneratedEnemyUnit(15,-11,33,31,4), new GeneratedEnemyUnit(-9,19,95,18,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "3676a161b1b07d9046858a7c8bea8a7f957df58ba953e23f66cc739cbacf958c");
        }

        private static void Case_03119()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3119,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,8,42,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-1,87,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,2,21,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,17,76,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,4,45,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-13,38,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,4,83,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,0,43,40,4), new GeneratedEnemyUnit(0,-14,24,30,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "5538a058f4137df447d7bac7cb54703a8118862e13870315728f08d217c00a29");
        }

        private static void Case_03120()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3120,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-9,91,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-15,61,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,6,19,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-9,82,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,0,63,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-9,25,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-3,64,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,10,23,7,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "559741b5a11792d63a1ee73d8c9e192fe86d113dc8d58edcf8cbebf468e29a5e");
        }

        private static void Case_03121()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3121,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-10,20,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-5,17,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,9,35,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,1,11,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-16,57,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,16,57,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-6,69,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-5,22,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,9,31,39,4), new GeneratedEnemyUnit(0,-19,9,36,3), new GeneratedEnemyUnit(10,-20,88,19,3), new GeneratedEnemyUnit(-11,-9,100,3,1), new GeneratedEnemyUnit(-3,19,61,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "1e69d7caa8cb2abea980439d2056af452f3bd82d30b43be9485d962e0ef6ceb5");
        }

        private static void Case_03122()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3122,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-16,89,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,10,77,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-4,38,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,20,68,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-4,21,42,4), new GeneratedEnemyUnit(12,17,72,19,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "38bb68fcbf21bf4e1acc167e9ae890e6272df72bd52a368717e8336ff5175ee7");
        }

        private static void Case_03123()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3123,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-5,54,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-13,35,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,5,76,18,3), new GeneratedEnemyUnit(6,-19,74,39,4), new GeneratedEnemyUnit(14,-11,34,50,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "1814ff47951a7ba2426e7ad3c0ad532da35d4c22ccee7f746c0a828f8acd7628");
        }

        private static void Case_03124()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3124,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,14,100,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-9,71,14,2), new GeneratedEnemyUnit(19,-12,67,42,4), new GeneratedEnemyUnit(-8,-1,99,34,4), new GeneratedEnemyUnit(8,11,58,20,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "eb83b24e8ab36dfbd79b48f906a0b13d852808fefd8b2074763fee6dbb9e8c03");
        }

        private static void Case_03125()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3125,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-19,97,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-17,49,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,11,74,41,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "ebf039ac378d415c2e291f4e79949834f1249d235223858f797a39d1e73899e2");
        }

        private static void Case_03126()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3126,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,5,26,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-2,81,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,16,28,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,12,82,49,3), new GeneratedEnemyUnit(-16,20,84,42,1), new GeneratedEnemyUnit(-14,20,37,10,1), new GeneratedEnemyUnit(11,-14,39,16,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "486d07b5990dc58f2bc62529c6326a44c8cf66a77c22635e548bc705c1df30de");
        }

        private static void Case_03127()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3127,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-12,35,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,7,45,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-17,87,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-9,64,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,17,63,43,1), new GeneratedEnemyUnit(-7,-13,56,38,2), new GeneratedEnemyUnit(16,-3,7,41,2), new GeneratedEnemyUnit(16,-14,64,33,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "f7f049f43ffbbaf3a8ecc61c403c0d6f1e7a3c1b7723f5b62c531bafc7f7436c");
        }

        private static void Case_03128()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3128,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-13,41,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,16,8,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "c90ccc6ff0c8446e3a7688a669792678991b01305723ad4573dcc5ab2d8a5ccc");
        }

        private static void Case_03129()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3129,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,17,63,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,8,10,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-3,95,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,4,80,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,9,22,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,18,15,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-19,9,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-8,38,50,1), new GeneratedEnemyUnit(13,-2,52,44,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "40e7cf79b1b11332f1d534de13f026f56b6e69b7c6a73a8e7e4bc1a0a433164a");
        }

        private static void Case_03130()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3130,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-15,87,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-19,31,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-2,60,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-6,98,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,19,71,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-18,67,20,4), new GeneratedEnemyUnit(7,10,69,26,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "abd40a4ab3a8a582b26b7021aa1a03b21c3e999028834985445681a1aaddb25f");
        }

        private static void Case_03131()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3131,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,18,42,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-11,24,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,8,46,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-13,55,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-9,80,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,20,77,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,15,61,6,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "370994e3ac8a8e015f69bc6395e7494fd5daf75a791f2d3beabd460864ef47ff");
        }

        private static void Case_03132()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3132,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,8,66,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,0,49,32,2), new GeneratedEnemyUnit(16,2,93,33,2), new GeneratedEnemyUnit(-8,7,36,35,2), new GeneratedEnemyUnit(-18,13,67,15,2), new GeneratedEnemyUnit(-4,5,70,48,3), new GeneratedEnemyUnit(-11,-3,10,28,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "25ba7aff926f170da2f69fdd3f72859456fda7267abf34c4b94f2be6cdca6956");
        }

        private static void Case_03133()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3133,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-6,48,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,8,79,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,18,91,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,16,69,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,9,59,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,20,96,2,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "201bf6d0c5b63ae1def149b4bd24f44c45e95a055b9385874ed2c6f6341dd09b");
        }

        private static void Case_03134()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3134,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,6,51,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,11,26,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-17,19,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,14,17,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,3,97,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,2,7,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-20,46,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,3,9,33,4), new GeneratedEnemyUnit(15,-2,65,18,1), new GeneratedEnemyUnit(8,7,46,5,4), new GeneratedEnemyUnit(6,-19,16,8,4), new GeneratedEnemyUnit(17,-10,18,16,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "1e56b5bf41e9e337949dacfeb5e36955dd37686232d4590ede4cea7daa5f5f9b");
        }

        private static void Case_03135()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3135,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-4,45,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,7,61,48,3), new GeneratedEnemyUnit(0,0,64,42,1), new GeneratedEnemyUnit(-8,6,28,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "d7117af6fd7aa89391627cc898369abfe0045cd4f04db81d6b1bfd45cefcd629");
        }

        private static void Case_03136()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3136,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,5,58,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-6,87,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,0,76,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-3,26,35,3), new GeneratedEnemyUnit(-15,12,29,29,1), new GeneratedEnemyUnit(-6,-3,37,7,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "51c576a2bbf508ec77db71ba0748e26e9ec29c4dc238d25fdec6378bb64f6b2f");
        }

        private static void Case_03137()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3137,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-14,8,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,15,10,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-5,38,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,12,9,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-19,53,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,19,28,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-3,9,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,11,30,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-11,90,35,1), new GeneratedEnemyUnit(16,17,70,12,3), new GeneratedEnemyUnit(-17,-2,53,6,2), new GeneratedEnemyUnit(-4,-17,29,6,1), new GeneratedEnemyUnit(-12,-3,58,40,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "957e79de2dc4f5777df7e4ff6c386c6eba13d3426c29ff5bb0fef512e267de59");
        }

        private static void Case_03138()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3138,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-17,57,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,1,58,49,2), new GeneratedEnemyUnit(6,11,56,1,4), new GeneratedEnemyUnit(-18,0,61,20,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "882e86372b18b1beff32e23fb0e184e8f79581a14f647862704b355d3c3269fa");
        }

        private static void Case_03139()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3139,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-1,15,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-14,73,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,6,23,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-8,99,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-6,80,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-16,61,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,9,89,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-11,96,38,1), new GeneratedEnemyUnit(1,8,58,21,3), new GeneratedEnemyUnit(1,10,84,40,3), new GeneratedEnemyUnit(0,5,31,20,1), new GeneratedEnemyUnit(7,-12,25,49,4), new GeneratedEnemyUnit(19,-18,82,28,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "27ddc119b26b6a0aff1ce0964b86439b09a6f38c9b0912e8fc9991ed7c024283");
        }

        private static void Case_03140()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3140,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-9,41,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-20,34,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,14,87,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-17,6,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,14,29,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,13,80,39,2), new GeneratedEnemyUnit(18,1,60,43,1), new GeneratedEnemyUnit(18,2,69,7,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "d82d6813b4f329a8cf0a142dd5e11a721899264b97a1b26b6925b04cff88530b");
        }

        private static void Case_03141()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3141,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,9,93,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-1,24,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,13,52,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,8,83,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,15,59,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,5,80,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,13,18,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,10,96,9,2), new GeneratedEnemyUnit(-1,-15,5,24,3), new GeneratedEnemyUnit(16,-9,53,41,1), new GeneratedEnemyUnit(17,-11,27,45,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "59990e35a923236565b553299651247acc46bf477e4d08f7768cd18271921d3d");
        }

        private static void Case_03142()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3142,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,18,43,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,12,99,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,2,85,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-7,37,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-15,73,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-8,57,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-12,80,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,12,25,4,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "c31da6213fbc78a27a16651c250ef808bb3a71771187e2bc655d747afe5f5f86");
        }

        private static void Case_03143()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3143,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,20,52,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,19,93,20,3), new GeneratedEnemyUnit(14,-4,58,46,2), new GeneratedEnemyUnit(9,-15,26,34,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "8a4ab0835594a63fdea500b96a875017998bb32fb59af54ca8eebff8614ab324");
        }

        private static void Case_03144()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3144,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,3,47,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,0,93,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,17,94,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,17,77,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,19,12,1,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "41ffd4e3f476f9c02d68990b9dd6ddb46311cdfaa299c1ec9c00f9eced696991");
        }

        private static void Case_03145()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3145,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,1,39,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,17,5,6,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "c4da2a5a7cdcd06e0bf87e3628e8732f1055823d8c50a069701eb20afe56d31b");
        }

        private static void Case_03146()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3146,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-5,22,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-4,94,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-18,33,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-11,36,38,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "38c55ff6cc817e26d1d074d513122d29051ae683ee71e24ee69a9568962f8ca8");
        }

        private static void Case_03147()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3147,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-1,77,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,6,11,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,18,89,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-13,42,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-13,27,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-20,28,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-15,100,41,4), new GeneratedEnemyUnit(8,5,86,44,4), new GeneratedEnemyUnit(-8,2,27,29,1), new GeneratedEnemyUnit(-5,4,96,41,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "29255c707a360de60265f53f7f7e4387384264559267c4af01434064df4dd7f0");
        }

        private static void Case_03148()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3148,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-10,68,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-19,75,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,16,61,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,2,34,9,4), new GeneratedEnemyUnit(1,-4,43,32,2), new GeneratedEnemyUnit(7,10,43,39,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "1eb89f2c9527529646898181a1878521ff495d27ad90512fd5836ff56d83eb15");
        }

        private static void Case_03149()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3149,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-17,53,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-9,66,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-18,95,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-20,84,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,8,9,3,4), new GeneratedEnemyUnit(0,-19,98,7,1), new GeneratedEnemyUnit(1,-11,46,17,4), new GeneratedEnemyUnit(10,-9,48,23,2), new GeneratedEnemyUnit(17,14,79,10,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "2f0b26a30521e4221a7f9d80b221ea0415c5c5bd6c2b5399373a613ed89339a1");
        }

        private static void Case_03150()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3150,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-14,10,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-16,39,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-15,6,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-13,86,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,2,98,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-18,54,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,11,56,15,4), new GeneratedEnemyUnit(-3,11,70,8,1), new GeneratedEnemyUnit(13,-14,27,30,4), new GeneratedEnemyUnit(14,19,43,45,3), new GeneratedEnemyUnit(-3,4,98,10,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "e31a27259f848eb100073a98bfd0026e1c9af8c71495d03b7566cb7f04d905dc");
        }

        private static void Case_03151()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3151,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,17,68,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,19,83,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,14,90,36,2), new GeneratedEnemyUnit(-10,-10,29,15,2), new GeneratedEnemyUnit(5,-15,98,19,1), new GeneratedEnemyUnit(15,-3,31,37,2), new GeneratedEnemyUnit(8,15,39,31,3), new GeneratedEnemyUnit(19,11,20,16,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "7ffca937986879cdc552c62ffdb09c99be6689e7f5103619c6084dfe60a95d75");
        }

        private static void Case_03152()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3152,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,9,36,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,4,23,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,10,68,37,1), new GeneratedEnemyUnit(-16,13,91,45,4), new GeneratedEnemyUnit(10,-7,13,36,4), new GeneratedEnemyUnit(-1,1,65,13,3), new GeneratedEnemyUnit(13,-2,40,7,3), new GeneratedEnemyUnit(-5,1,35,36,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "c1e6e52f21fd6907fba32fc9afb69602e7abb643873d7b54569f7e9a6fb2f742");
        }

        private static void Case_03153()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3153,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,7,77,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,4,23,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-11,21,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-17,20,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-18,10,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-15,97,3,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "d28278c5ec9f8b3639e3c0245309312614953da3e363946abb71bdc7d27244ec");
        }

        private static void Case_03154()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3154,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,15,71,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,11,18,38,4), new GeneratedEnemyUnit(12,-9,48,27,2), new GeneratedEnemyUnit(6,-3,42,37,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "ae7e4a182d4d553723cfed9d1e5def2bff33ccbda557dd779125bc9d08935528");
        }

        private static void Case_03155()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3155,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,17,93,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,4,8,24,2), new GeneratedEnemyUnit(5,16,6,15,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "a5d59a1532cbddcd073a4c21744983ee99145603446098b4970301c06791be4e");
        }

        private static void Case_03156()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3156,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-10,66,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,16,11,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,8,84,13,4), new GeneratedEnemyUnit(0,2,92,33,1), new GeneratedEnemyUnit(-4,4,31,21,2), new GeneratedEnemyUnit(10,19,6,2,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "9c32b0d01c09ec23f0d721b06e21df5a0b3d89785db4258e42c18b7848d9744f");
        }

        private static void Case_03157()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3157,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,8,55,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-14,26,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-12,17,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,8,97,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-9,70,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-11,15,15,2), new GeneratedEnemyUnit(14,-9,12,47,1), new GeneratedEnemyUnit(19,9,63,49,2), new GeneratedEnemyUnit(-7,-2,94,32,4), new GeneratedEnemyUnit(-9,13,25,13,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "071412d2fe750fbd955ed0bf9097aaf99b481652d2f2a05c99664f871b802268");
        }

        private static void Case_03158()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3158,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-13,80,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,11,87,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-17,89,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,14,58,13,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "3aec06a597acab3f87f59fad74949cd23dea0af93060cf1c91061fee06081c23");
        }

        private static void Case_03159()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3159,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-11,49,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-13,72,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,13,27,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-16,74,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-19,37,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,3,22,4,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "2a303b2e59dff433f95e4da9a1cbf504c8e3fe28df0b0e6aac542070121106f6");
        }

        private static void Case_03160()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3160,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-11,32,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,14,80,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-16,52,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,3,16,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,6,48,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-4,51,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-9,37,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-15,20,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,7,69,23,1), new GeneratedEnemyUnit(-16,2,20,38,2), new GeneratedEnemyUnit(-16,-8,46,16,1), new GeneratedEnemyUnit(7,-16,69,20,1), new GeneratedEnemyUnit(-16,15,36,42,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "6e9d51d7f3f942093004e2295ac1b33948c58fd9d3c129e2239d58f49f487aaa");
        }

        private static void Case_03161()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3161,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,8,54,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-17,33,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,1,66,15,1), new GeneratedEnemyUnit(-16,-6,38,10,3), new GeneratedEnemyUnit(7,-5,88,27,2), new GeneratedEnemyUnit(12,6,92,37,3), new GeneratedEnemyUnit(16,-13,89,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "b0b49f1f9f03e7328445791fc4ccaaf2661e9bf7e5a1d45620c2f62480b98bea");
        }

        private static void Case_03162()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3162,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-12,87,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,16,87,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,12,11,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,18,18,7,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "4e7c731e223d46336b8beb7f0d6edda35498bb827c75a4ff3132f3d2a1f8ee1a");
        }

        private static void Case_03163()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3163,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-17,41,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,8,71,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,9,70,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,15,18,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-1,45,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-19,92,35,1), new GeneratedEnemyUnit(5,5,83,19,1), new GeneratedEnemyUnit(8,-16,49,33,2), new GeneratedEnemyUnit(19,-20,61,33,1), new GeneratedEnemyUnit(3,-1,26,10,1), new GeneratedEnemyUnit(-5,-12,96,15,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "54f2146ba0b62f8c9b8b7afec058a2b13bb332944ece97c5dc32f1b6cd63a68a");
        }

        private static void Case_03164()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3164,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-11,75,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-4,57,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,13,62,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,19,35,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,1,58,6,2), new GeneratedEnemyUnit(-5,19,13,48,4), new GeneratedEnemyUnit(-17,-4,37,4,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "d3f34aab611402cbb316744f6cae543faad887796a7dfd3e4aeb271d974cd0c7");
        }

        private static void Case_03165()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3165,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,3,60,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,16,29,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,18,99,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-7,71,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-14,45,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,0,17,41,2), new GeneratedEnemyUnit(-8,7,36,46,1), new GeneratedEnemyUnit(-18,-2,90,43,4), new GeneratedEnemyUnit(-17,3,62,27,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b75294cc45d773daf8eeb43ba4f940be331bcb7fe6ad1f7a5cab7fe5bc79e5cb");
        }

        private static void Case_03166()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3166,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,14,39,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,19,8,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,19,93,2,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "99c63cc15a16ce18f445dbf0feeab64484e7a3b83732105c8c2f1bae055e5981");
        }

        private static void Case_03167()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3167,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,15,99,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,15,68,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,16,8,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-12,80,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "d912f68a83034f340a3bf4f781d01bdf2f0222fdd710f2465ce72b414d294bff");
        }

        private static void Case_03168()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3168,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-19,46,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,12,57,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,19,30,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-1,46,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-12,25,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,9,28,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,0,86,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,12,32,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,6,56,9,3), new GeneratedEnemyUnit(12,14,39,39,1), new GeneratedEnemyUnit(17,1,35,49,4), new GeneratedEnemyUnit(0,-7,81,28,2), new GeneratedEnemyUnit(14,-9,95,33,4), new GeneratedEnemyUnit(13,-19,92,48,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "1343009686f0e87b04badf96e3f6ab8a0282057d2fb4e3f8cf10faa97c3a5b48");
        }

        private static void Case_03169()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3169,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,9,86,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,9,77,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-3,26,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-16,85,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,5,21,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,17,90,21,3), new GeneratedEnemyUnit(18,-2,79,46,1), new GeneratedEnemyUnit(5,-15,21,6,2), new GeneratedEnemyUnit(7,-6,25,31,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "408ac27cb1a7c47492095a209ec8d1e596270177acb396b4ad63a01fcf84e6fe");
        }

        private static void Case_03170()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3170,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-9,46,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-7,27,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-9,97,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,9,39,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-8,47,7,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "073af9164735690d35349664c6bd3da3ea86b60738301b613d3da4fd3f13e2f9");
        }

        private static void Case_03171()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3171,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-13,41,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,7,15,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-1,54,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,19,51,36,4), new GeneratedEnemyUnit(-17,-16,34,1,1), new GeneratedEnemyUnit(-16,3,15,47,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "e2494d30873972478b7072ecdae8123f7cccbab3dbcdf4eb5a7e3f5fd168a9af");
        }

        private static void Case_03172()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3172,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,9,72,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-10,75,46,2), new GeneratedEnemyUnit(4,-3,86,15,2), new GeneratedEnemyUnit(-16,9,100,36,4), new GeneratedEnemyUnit(-17,8,61,18,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "fd49b80ce81e3e99a0dd6d5e84af0d9a401e4ca2be7a79fbe4660c0611a33097");
        }

        private static void Case_03173()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3173,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,1,48,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,8,7,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,14,26,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-2,62,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,5,26,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,17,35,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,19,15,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,19,29,5,4), new GeneratedEnemyUnit(2,-1,80,31,3), new GeneratedEnemyUnit(4,3,93,14,3), new GeneratedEnemyUnit(-9,3,33,40,1), new GeneratedEnemyUnit(-5,-3,35,30,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "80beec02b918b9dedb30f5facaf32f8df3bc19c3de5f79667467c421363a4035");
        }

        private static void Case_03174()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3174,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-14,18,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,2,40,6,4), new GeneratedEnemyUnit(-9,4,14,41,2), new GeneratedEnemyUnit(-15,3,77,2,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "da2cf75a9194d3760c21276052ca9db960c4962be318f72e6b1631b17552261e");
        }

        private static void Case_03175()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3175,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-8,63,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,8,44,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,10,59,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-4,92,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,5,9,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-9,82,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-13,98,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-3,65,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,9,43,2,4), new GeneratedEnemyUnit(2,11,67,31,2), new GeneratedEnemyUnit(-1,19,90,23,2), new GeneratedEnemyUnit(3,0,14,25,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "f106960f3223dc72c9819c0b4c49651af238fac0c6e124a2cead7c2eff364a40");
        }

        private static void Case_03176()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3176,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,5,62,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-8,29,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-9,72,1,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3e74337f68cd5b122f41383e07498f1a19c0175616e7aa95a605d4b555619178");
        }

        private static void Case_03177()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3177,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,10,31,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-11,17,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-5,76,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-11,45,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,19,77,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,1,47,25,3), new GeneratedEnemyUnit(11,-17,97,14,2), new GeneratedEnemyUnit(-3,6,82,48,1), new GeneratedEnemyUnit(-6,-11,5,27,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "80dbcc08e909e2658f4f6887a291fcf36702c910e1d207598c10bd1cb398b53b");
        }

        private static void Case_03178()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3178,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,0,47,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-12,77,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,7,47,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,14,25,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-20,62,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,12,38,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,3,40,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-9,29,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-1,60,26,3), new GeneratedEnemyUnit(-10,7,88,10,1), new GeneratedEnemyUnit(-6,-3,23,46,4), new GeneratedEnemyUnit(6,-12,6,20,4), new GeneratedEnemyUnit(12,13,71,37,4), new GeneratedEnemyUnit(-8,10,38,34,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "54f28d12bd8f57b69763186fdd339d3fbde4aca29b57dda4970ddfc34d2c08ab");
        }

        private static void Case_03179()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3179,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-17,8,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,10,52,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-12,13,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-2,26,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,17,58,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-6,87,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-16,90,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,10,26,3,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "9f816af80f9042ec94bf21e3a5fc870e76f12414dcee0cc774ec36b2a43f0404");
        }

        private static void Case_03180()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3180,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-5,95,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,16,11,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,6,44,3,3), new GeneratedEnemyUnit(-16,14,49,16,1), new GeneratedEnemyUnit(7,-11,18,4,3), new GeneratedEnemyUnit(-19,-11,52,5,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "06a3f73f01b6db4f3b3c912a22bdde1377b2ace7c485ba1c424d8e0d3f3aca82");
        }

        private static void Case_03181()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3181,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-19,36,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,6,84,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-4,40,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,10,16,8,2), new GeneratedEnemyUnit(11,3,72,4,1), new GeneratedEnemyUnit(20,15,22,7,4), new GeneratedEnemyUnit(-16,4,30,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "127e5a1a3d827872806753098ae5dc897669219a7871c63e2391600359e9a0b7");
        }

        private static void Case_03182()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3182,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,17,42,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-14,42,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,14,46,18,2), new GeneratedEnemyUnit(-18,-14,9,10,2), new GeneratedEnemyUnit(6,-6,33,21,2), new GeneratedEnemyUnit(9,2,36,43,1), new GeneratedEnemyUnit(-13,-12,61,47,1), new GeneratedEnemyUnit(-11,-19,67,7,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "24d647390aee7bb87c84821f91f3b579ad0e1c5b6e934fb42902e8d58a4f0626");
        }

        private static void Case_03183()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3183,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,17,80,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-4,45,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-5,12,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-13,43,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,12,46,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,16,24,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-11,45,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,18,58,32,3), new GeneratedEnemyUnit(4,19,89,37,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "77deab2e0f5790a47ad41a61424bbe6d02c022d9ead5c200539b38d39beb66f0");
        }

        private static void Case_03184()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3184,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,11,9,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-8,53,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-16,7,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,8,36,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,3,13,7,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "8efd1c662d8902ceabcd49832f37178e24f6b4cac76bcd2cba9626b38a27db31");
        }

        private static void Case_03185()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3185,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,16,79,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-5,61,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,7,39,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,14,36,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,20,29,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-19,35,3,2), new GeneratedEnemyUnit(4,-10,88,9,2), new GeneratedEnemyUnit(-8,12,50,5,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "0454957c54aa1865dd997a82662bd397230be30630a3060510c918b103c3c57f");
        }

        private static void Case_03186()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3186,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-20,16,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,4,75,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-18,15,34,2), new GeneratedEnemyUnit(-1,16,70,25,2), new GeneratedEnemyUnit(16,-17,75,9,3), new GeneratedEnemyUnit(12,-2,46,5,4), new GeneratedEnemyUnit(13,-9,43,6,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "d64ac58a53574ed778b9d4c762fed256349bf1447f02c299de8d46fa5cedcd98");
        }

        private static void Case_03187()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3187,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-19,64,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,16,35,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,0,25,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,11,56,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-13,68,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,9,98,49,2), new GeneratedEnemyUnit(16,15,34,34,3), new GeneratedEnemyUnit(-2,-18,100,14,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1eca9f6aa2a4377eeed11b13fdfb3e48716bb552e02a91d6231268ca0b5a2d03");
        }

        private static void Case_03188()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3188,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,18,31,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,19,93,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-18,48,22,3), new GeneratedEnemyUnit(10,4,95,4,4), new GeneratedEnemyUnit(-17,5,62,27,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5c466adb075e26eb7c0bd051c3d9cdbe6b5a045e5c4dde97ad1d5a96a34ad340");
        }

        private static void Case_03189()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3189,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-20,15,7,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "bdddabc1fb9786fdffea37d7d4dbdf0e4e39c1a82200d4ab10fe82fb7b208113");
        }

        private static void Case_03190()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3190,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-6,22,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,19,16,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-11,100,4,4), new GeneratedEnemyUnit(3,4,5,44,3), new GeneratedEnemyUnit(-4,-2,16,30,2), new GeneratedEnemyUnit(3,7,17,1,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "24c6e242d94182ebe9c041f62bc2d5cd5e5ddedb36ffb41c25ca65abe8b0567e");
        }

        private static void Case_03191()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3191,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-5,24,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,3,61,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-12,90,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-2,55,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,16,11,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-10,38,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,14,43,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,2,98,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,18,87,46,1), new GeneratedEnemyUnit(-11,-14,24,7,4), new GeneratedEnemyUnit(12,1,46,24,4), new GeneratedEnemyUnit(-8,11,29,26,4), new GeneratedEnemyUnit(-3,-2,62,15,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "a477f2169efeb154c705cd4120e534b0e546904ab0e3d5aa6507c7ff97331cd9");
        }

        private static void Case_03192()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3192,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,2,78,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-11,77,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-17,42,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-5,68,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,7,90,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-9,43,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,15,18,28,2), new GeneratedEnemyUnit(12,-10,71,48,1), new GeneratedEnemyUnit(-2,13,30,28,4), new GeneratedEnemyUnit(-11,8,39,3,3), new GeneratedEnemyUnit(15,-7,5,19,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "4a036b38058b85c982d140742a32161c40231d4959f2b0262a1fdc81b8826cd1");
        }

        private static void Case_03193()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3193,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-11,81,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-4,68,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,10,42,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,9,67,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,18,12,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,18,95,23,2), new GeneratedEnemyUnit(-3,18,51,20,1), new GeneratedEnemyUnit(0,18,49,9,1), new GeneratedEnemyUnit(2,6,97,28,1), new GeneratedEnemyUnit(12,-8,58,12,2), new GeneratedEnemyUnit(-7,18,55,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "de5587f165851494f7e34b98e51e0a25c09fc577cae5d2c714aacbe6af2f7bf6");
        }

        private static void Case_03194()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3194,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-10,70,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,5,5,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,15,60,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-18,9,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-17,54,19,4), new GeneratedEnemyUnit(-4,19,89,28,2), new GeneratedEnemyUnit(-15,9,67,16,3), new GeneratedEnemyUnit(-2,14,31,34,2), new GeneratedEnemyUnit(2,-4,44,12,4), new GeneratedEnemyUnit(-13,-6,57,31,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "4123ea7ab13e8a8e980d9ce19a51d5066a69c0c5aea1f016e6ef6d88f2684243");
        }

        private static void Case_03195()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3195,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-4,74,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-19,5,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-20,50,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,18,21,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-10,96,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-4,51,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,14,84,27,3), new GeneratedEnemyUnit(-16,17,78,3,2), new GeneratedEnemyUnit(5,7,21,13,2), new GeneratedEnemyUnit(-11,4,78,36,3), new GeneratedEnemyUnit(-2,-14,84,47,2), new GeneratedEnemyUnit(20,-15,54,26,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "ae89ab1c7f41a681d7f8469ffa43a5403b8317c6f44cf24cc8efba4c1c9dfdc5");
        }

        private static void Case_03196()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3196,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,9,13,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,4,76,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,1,7,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,13,9,49,1), new GeneratedEnemyUnit(18,11,89,36,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6a8683de200cb0c7509acbb10865cb965cd67fab64d52c0bc78d073a1ea49f47");
        }

        private static void Case_03197()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3197,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-18,38,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,17,26,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,13,89,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,9,75,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-18,9,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,8,82,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,10,64,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-9,65,17,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "308f3439f9ccf72e04a6eaa8e1c6a40a24a1ae00a47b175b776a65adfd9dd25d");
        }

        private static void Case_03198()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3198,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-9,80,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,6,91,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,20,35,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-1,84,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-12,14,44,1), new GeneratedEnemyUnit(-20,15,36,19,4), new GeneratedEnemyUnit(18,-10,26,34,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "6cd42e45bad78db55f74d7046cdc7f65f73590d6f7ed30d96cc8f00010ea387f");
        }

        private static void Case_03199()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3199,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,20,76,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,19,16,27,1), new GeneratedEnemyUnit(-9,-17,73,15,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "7ffa07f8102e6576fcb49f3ea04223799a4055e66092de6d27e6b3a874c5dbce");
        }

        private static void Case_03200()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3200,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,1,78,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-18,77,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-16,7,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-10,58,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-19,84,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-2,46,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,5,22,26,1), new GeneratedEnemyUnit(0,-8,21,31,4), new GeneratedEnemyUnit(14,7,46,22,4), new GeneratedEnemyUnit(-14,-6,85,50,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "e201c69650194bd6e512b3de8c02eed0ee5ac504b95ddb9fbdda88e733bf7e58");
        }

        private static void Case_03201()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3201,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-6,83,6,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "c11d0af9e7e3b648f8521948361ca9af130cff27c4e2c9e060d7082c043b17b8");
        }

        private static void Case_03202()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3202,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,11,82,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,10,79,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-9,20,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-5,5,4,1), new GeneratedEnemyUnit(-15,12,55,38,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "6fc3c4fc1324ea77cac32eff4e65ac2d4f26daba7f0d025921f49b3b03aa206d");
        }

        private static void Case_03203()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3203,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-3,92,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,19,43,43,3), new GeneratedEnemyUnit(17,15,74,43,4), new GeneratedEnemyUnit(20,-5,46,42,4), new GeneratedEnemyUnit(-11,7,28,5,2), new GeneratedEnemyUnit(0,-9,51,11,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "85b726a8a67853096dc157c3e55c979146294485c606d2099436648bdb6e6731");
        }

        private static void Case_03204()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3204,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-5,89,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-11,40,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-20,38,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-7,52,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-4,78,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-7,65,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-11,96,6,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "9297fec2c7286ff109318ab90c1875342c711d5fe75ec5bbf7e54902aa0dcfdb");
        }

        private static void Case_03205()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3205,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-2,67,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,20,89,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,8,10,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-19,84,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,0,38,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-4,25,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-7,26,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,18,42,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,2,70,26,2), new GeneratedEnemyUnit(7,-15,30,31,1), new GeneratedEnemyUnit(-19,19,18,34,4), new GeneratedEnemyUnit(-2,-7,21,24,4), new GeneratedEnemyUnit(-8,4,79,15,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "03c3b3aff2a772f08b29ad3f68642fc9160f714e6fa0b07be3f7f833e8b4fc0e");
        }

        private static void Case_03206()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3206,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-20,33,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,2,51,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,5,21,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-14,29,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,17,15,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-5,46,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-10,17,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,6,10,34,2), new GeneratedEnemyUnit(-16,-15,23,30,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "edd8a17b802c22a799076fac99991c036466a9816f8d7439f9e3f0df9fb45a49");
        }

        private static void Case_03207()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3207,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-19,71,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-13,27,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-2,35,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-8,26,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-5,23,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-6,20,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-10,19,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-2,49,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,5,51,6,1), new GeneratedEnemyUnit(-10,-14,58,8,3), new GeneratedEnemyUnit(-7,-9,67,20,2), new GeneratedEnemyUnit(-11,-7,28,46,4), new GeneratedEnemyUnit(7,-11,86,28,2), new GeneratedEnemyUnit(-17,12,94,32,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "fbc216b4b941397e1de6210ae688545012544a3ac9487bfbe34ee3c33b65ca1a");
        }

        private static void Case_03208()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3208,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-15,86,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-1,95,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-9,62,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,12,23,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-7,84,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,3,72,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,19,46,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-20,32,10,4), new GeneratedEnemyUnit(-16,4,97,30,4), new GeneratedEnemyUnit(17,14,31,35,1), new GeneratedEnemyUnit(9,7,23,40,1), new GeneratedEnemyUnit(17,11,37,6,3), new GeneratedEnemyUnit(-20,-1,17,15,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "f256584cd10dbfda68ec647e391540adc7bcaf0bb4c0eb655e06e69a425e94ab");
        }

        private static void Case_03209()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3209,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,18,32,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-6,70,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-14,9,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,2,40,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,8,7,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,11,35,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,13,42,15,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1bbd3ef9c256c050caab5c4594111798932d43eff9dce8be7dc01829f0e0c8be");
        }

        private static void Case_03210()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3210,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-20,19,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-16,44,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-4,6,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-11,23,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-13,92,2,1), new GeneratedEnemyUnit(1,8,89,36,4), new GeneratedEnemyUnit(3,-1,86,28,2), new GeneratedEnemyUnit(5,16,98,50,3), new GeneratedEnemyUnit(11,0,79,38,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "f2d43112f771e0d7bb473f779c6bda0e0f37e4f2eec7b67aa6f81c1bf7d99b99");
        }

        private static void Case_03211()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3211,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-4,67,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,10,52,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-15,25,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-8,99,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,2,48,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-1,87,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-7,34,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-2,100,35,1), new GeneratedEnemyUnit(-18,-9,52,20,2), new GeneratedEnemyUnit(2,-13,24,34,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 30,
                stableHash: "adedeacfffece20b0a1c002e6163d8ae73eabed18e347c5309a68fd7bef8b3e9");
        }

        private static void Case_03212()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3212,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,7,82,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-9,59,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,14,22,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-4,55,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,4,53,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-8,94,3,4), new GeneratedEnemyUnit(6,-14,27,34,3), new GeneratedEnemyUnit(13,-3,10,23,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "7ce8bced359820ba4ad73014677a6b44b5bbc43ba2ec2c478788dcf52f5d3980");
        }

        private static void Case_03213()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3213,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-13,26,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,14,74,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,18,86,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,14,22,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-16,51,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,12,40,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,5,66,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,0,9,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-16,80,44,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "aa588f09d9325ae498efcce86f568d834eb2c1c55d4a5e10a0fcbab686789f35");
        }

        private static void Case_03214()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3214,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,17,63,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-8,36,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-14,91,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-7,98,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,15,42,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,5,21,5,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "109b01d58922689dea6ea6c0eb6bc651cfe7456bd82dddcb5b3d9fd2385d4029");
        }

        private static void Case_03215()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3215,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,2,57,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-15,30,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-8,49,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-12,71,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,4,86,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-19,46,17,1), new GeneratedEnemyUnit(18,11,46,26,2), new GeneratedEnemyUnit(18,4,66,33,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "ca2b37ad7ff24c3515e97ed079b9f0b26c0d68e2f7530a2c6e803f3ab96b6d3f");
        }

        private static void Case_03216()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3216,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-16,72,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,3,76,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-10,91,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-5,42,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,10,35,46,2), new GeneratedEnemyUnit(16,-18,66,1,2), new GeneratedEnemyUnit(-8,10,95,3,1), new GeneratedEnemyUnit(-5,-1,90,25,3), new GeneratedEnemyUnit(-10,20,16,41,2), new GeneratedEnemyUnit(-9,9,11,3,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "e8b02e11e4a10b9e86883bb70b94eb720ab28a470e9d446ef84d189e9ec0257b");
        }

        private static void Case_03217()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3217,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,11,94,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,10,83,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,17,20,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-12,31,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-7,16,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,6,86,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-18,99,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-2,47,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-3,41,15,2), new GeneratedEnemyUnit(3,20,22,21,2), new GeneratedEnemyUnit(20,-6,38,29,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "8abf1e9ac8ea316aa54bc2884d38723b9aa7a9915ae0613f30ceb4e64d65aa5e");
        }

        private static void Case_03218()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3218,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,8,73,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,11,85,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-20,30,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-19,23,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,3,92,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-10,70,21,2), new GeneratedEnemyUnit(-20,-6,33,39,2), new GeneratedEnemyUnit(-19,-13,98,15,2), new GeneratedEnemyUnit(-6,-16,80,30,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "f10fdec99812d95f1f684cc6d02fb28c5a4091fac5f0fe0c0d16d323e3e3f292");
        }

        private static void Case_03219()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3219,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-7,31,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-1,42,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-11,28,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,5,29,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,17,65,47,2), new GeneratedEnemyUnit(-15,-15,35,30,2), new GeneratedEnemyUnit(-14,5,11,32,4), new GeneratedEnemyUnit(-1,9,25,17,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "323000b5703241d77ef2f5eed9e9747795da2c13b1898a728f9f8beb121092dc");
        }

        private static void Case_03220()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3220,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-10,99,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,11,13,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,3,7,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,18,92,28,2), new GeneratedEnemyUnit(-20,-8,92,49,2), new GeneratedEnemyUnit(-19,-11,30,13,4), new GeneratedEnemyUnit(-2,-8,44,50,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "a26979a197303c2d58146bbb03a6167b292925f934769adac60416e55739e0d3");
        }

        private static void Case_03221()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3221,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-11,17,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,16,95,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-13,28,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-2,18,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-20,9,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-10,10,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-1,90,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-12,16,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,18,10,36,1), new GeneratedEnemyUnit(-16,1,22,45,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "a91f48e8f4cfef10ba3440033fa71b21fa7e8245179e08a7269ed4a9ea2f1853");
        }

        private static void Case_03222()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3222,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-6,45,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,8,47,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,15,74,1,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "05993147994fb919381e1db105c7bb42010b1cf718a8d30d7a8f3f75fa6cf5bf");
        }

        private static void Case_03223()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3223,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-15,57,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,7,13,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,4,80,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,7,91,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,20,46,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,5,99,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,12,23,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-4,100,1,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "14a19a562d3a1a02bbbf22acec847bc7bbca16bc0e63ece7bd3246b0446b9a3e");
        }

        private static void Case_03224()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3224,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,6,89,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-14,67,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-3,84,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,3,22,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,1,19,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-12,94,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,9,11,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-15,64,5,2), new GeneratedEnemyUnit(-6,1,73,22,1), new GeneratedEnemyUnit(4,1,46,28,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "562f981d3bcc8dd971677591637172799f829d2589d16ec63e2bd34b7a86647d");
        }

        private static void Case_03225()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3225,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,18,72,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,0,41,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,4,67,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,18,55,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,12,19,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-12,46,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-19,49,41,2), new GeneratedEnemyUnit(-18,12,91,8,2), new GeneratedEnemyUnit(17,-17,40,28,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "24a4ba98f9463a4fddaf395ec5b0cc7d72c330da7c7cb142fbc88525ceab5bf4");
        }

        private static void Case_03226()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3226,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,6,74,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,10,46,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,13,29,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-15,65,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,8,84,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-7,47,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-1,54,30,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e55f80c38de9d02d8f57f48f4442e6efa6c697cc98ef1aa2649437e7e67fbccf");
        }

        private static void Case_03227()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3227,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,9,15,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,8,93,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,15,22,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-11,95,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-14,43,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-19,91,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,13,80,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,2,45,6,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "adec51d8d8934723d5f5e72f5500a6a196aecfb978ccfd0ab2d4688a19b5ab81");
        }

        private static void Case_03228()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3228,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,14,42,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-4,74,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,10,14,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-3,60,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,14,46,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,0,74,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-19,40,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-15,37,14,4), new GeneratedEnemyUnit(-2,-6,61,31,2), new GeneratedEnemyUnit(11,-3,5,29,1), new GeneratedEnemyUnit(0,-5,12,15,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "606d9970681b032a1041fde9919a38b409a665bd8d42fd01610ae839d3c1fca6");
        }

        private static void Case_03229()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3229,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,11,51,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,18,40,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-3,40,2,2), new GeneratedEnemyUnit(12,1,5,29,2), new GeneratedEnemyUnit(13,-4,74,5,4), new GeneratedEnemyUnit(-18,-19,31,1,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "62235d2aeed790a01b336989670fe7c9f22a423b59a0917ba1bff4f622b54ba5");
        }

        private static void Case_03230()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3230,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,6,40,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-18,40,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,4,9,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-16,79,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,6,77,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-4,24,3,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "94e007503a6d9365ce9fc9ab40de0c7d078446129e0c0b7eb504cb52e8d7618f");
        }

        private static void Case_03231()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3231,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,8,52,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-10,42,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-6,58,45,3), new GeneratedEnemyUnit(-15,-3,58,27,1), new GeneratedEnemyUnit(16,-15,36,20,2), new GeneratedEnemyUnit(10,20,54,16,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "7752da9455585acddec1afa10d18876948b7cadffddbdde1e7bac6ea2eead147");
        }

        private static void Case_03232()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3232,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-19,22,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,19,24,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-12,57,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-18,78,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-19,36,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,18,8,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,20,15,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-1,57,20,4), new GeneratedEnemyUnit(-19,1,62,1,3), new GeneratedEnemyUnit(19,5,52,46,4), new GeneratedEnemyUnit(9,5,63,19,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "ab5fa8c133de8cf8723f300c08701f105edc60d08a1bc61f3cf4c4758bc7696d");
        }

        private static void Case_03233()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3233,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-2,83,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-1,46,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-12,34,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,1,6,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,4,82,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,5,55,25,4), new GeneratedEnemyUnit(-14,-18,28,20,2), new GeneratedEnemyUnit(17,20,60,5,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "d047cc0dd26508b850216a41f77015ab2c13d8c93707e14daa5eb60acc1ade06");
        }

        private static void Case_03234()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3234,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,9,9,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-5,75,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,15,80,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,2,23,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-1,29,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,4,90,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-14,6,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,7,43,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-1,97,9,4), new GeneratedEnemyUnit(-9,-8,55,27,4), new GeneratedEnemyUnit(5,-12,34,31,2), new GeneratedEnemyUnit(-19,-6,18,46,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "87f8d34b1478b31422eccbb241471fb26d1dc782f2f52c72f10742c1523851df");
        }

        private static void Case_03235()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3235,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,15,87,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-14,93,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,3,48,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,3,86,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-7,86,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,15,12,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,5,93,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-7,23,41,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "f2d81e592865633d79fc32a72d421a5b29d0dea4366d47858adc22d9e8895334");
        }

        private static void Case_03236()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3236,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-5,63,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,16,51,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,0,67,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,15,74,7,2), new GeneratedEnemyUnit(-20,10,19,37,1), new GeneratedEnemyUnit(11,-17,78,4,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "08487823af759348d68209466b7e8752d1cf7cee65abac6ec0dcbfd983722897");
        }

        private static void Case_03237()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3237,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,2,15,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,7,82,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,20,85,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,9,34,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,17,24,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,16,79,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,3,28,28,1), new GeneratedEnemyUnit(19,-10,10,10,4), new GeneratedEnemyUnit(11,1,49,33,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "6dfb23e02a2ca51fb93ae5499ecd2b77b14dfbd4a7d6d311913698b19a971b0f");
        }

        private static void Case_03238()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3238,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,17,33,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-8,33,4,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "08b5d764cbf161119d0a15eec2596793eea088418037f2d1e4eb1c7ba9e7c643");
        }

        private static void Case_03239()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3239,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-6,38,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-20,67,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-12,82,16,2), new GeneratedEnemyUnit(8,-7,89,2,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1c37cf0dccb1571047ee975844de98d8a1b8eb0623f479edbfc33e7d773f6653");
        }

    }
}
