using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard006
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_01080();
            Case_01081();
            Case_01082();
            Case_01083();
            Case_01084();
            Case_01085();
            Case_01086();
            Case_01087();
            Case_01088();
            Case_01089();
            Case_01090();
            Case_01091();
            Case_01092();
            Case_01093();
            Case_01094();
            Case_01095();
            Case_01096();
            Case_01097();
            Case_01098();
            Case_01099();
            Case_01100();
            Case_01101();
            Case_01102();
            Case_01103();
            Case_01104();
            Case_01105();
            Case_01106();
            Case_01107();
            Case_01108();
            Case_01109();
            Case_01110();
            Case_01111();
            Case_01112();
            Case_01113();
            Case_01114();
            Case_01115();
            Case_01116();
            Case_01117();
            Case_01118();
            Case_01119();
            Case_01120();
            Case_01121();
            Case_01122();
            Case_01123();
            Case_01124();
            Case_01125();
            Case_01126();
            Case_01127();
            Case_01128();
            Case_01129();
            Case_01130();
            Case_01131();
            Case_01132();
            Case_01133();
            Case_01134();
            Case_01135();
            Case_01136();
            Case_01137();
            Case_01138();
            Case_01139();
            Case_01140();
            Case_01141();
            Case_01142();
            Case_01143();
            Case_01144();
            Case_01145();
            Case_01146();
            Case_01147();
            Case_01148();
            Case_01149();
            Case_01150();
            Case_01151();
            Case_01152();
            Case_01153();
            Case_01154();
            Case_01155();
            Case_01156();
            Case_01157();
            Case_01158();
            Case_01159();
            Case_01160();
            Case_01161();
            Case_01162();
            Case_01163();
            Case_01164();
            Case_01165();
            Case_01166();
            Case_01167();
            Case_01168();
            Case_01169();
            Case_01170();
            Case_01171();
            Case_01172();
            Case_01173();
            Case_01174();
            Case_01175();
            Case_01176();
            Case_01177();
            Case_01178();
            Case_01179();
            Case_01180();
            Case_01181();
            Case_01182();
            Case_01183();
            Case_01184();
            Case_01185();
            Case_01186();
            Case_01187();
            Case_01188();
            Case_01189();
            Case_01190();
            Case_01191();
            Case_01192();
            Case_01193();
            Case_01194();
            Case_01195();
            Case_01196();
            Case_01197();
            Case_01198();
            Case_01199();
            Case_01200();
            Case_01201();
            Case_01202();
            Case_01203();
            Case_01204();
            Case_01205();
            Case_01206();
            Case_01207();
            Case_01208();
            Case_01209();
            Case_01210();
            Case_01211();
            Case_01212();
            Case_01213();
            Case_01214();
            Case_01215();
            Case_01216();
            Case_01217();
            Case_01218();
            Case_01219();
            Case_01220();
            Case_01221();
            Case_01222();
            Case_01223();
            Case_01224();
            Case_01225();
            Case_01226();
            Case_01227();
            Case_01228();
            Case_01229();
            Case_01230();
            Case_01231();
            Case_01232();
            Case_01233();
            Case_01234();
            Case_01235();
            Case_01236();
            Case_01237();
            Case_01238();
            Case_01239();
            Case_01240();
            Case_01241();
            Case_01242();
            Case_01243();
            Case_01244();
            Case_01245();
            Case_01246();
            Case_01247();
            Case_01248();
            Case_01249();
            Case_01250();
            Case_01251();
            Case_01252();
            Case_01253();
            Case_01254();
            Case_01255();
            Case_01256();
            Case_01257();
            Case_01258();
            Case_01259();
        }

        private static void Case_01080()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1080,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-16,25,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-2,79,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,3,82,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-11,39,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,1,17,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,9,10,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-17,91,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-13,29,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-8,85,37,1), new GeneratedEnemyUnit(5,-8,29,15,2), new GeneratedEnemyUnit(-8,-14,100,9,1), new GeneratedEnemyUnit(11,17,86,50,3), new GeneratedEnemyUnit(-17,16,39,18,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "fdca0fbaaa2865d059c7d774e07dcb2a093665bba1336335adb0f006c386e3c1");
        }

        private static void Case_01081()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1081,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-16,55,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,0,94,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-15,66,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-13,66,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,6,84,35,4), new GeneratedEnemyUnit(3,-2,11,22,4), new GeneratedEnemyUnit(-19,-6,46,26,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "70adc4a072ec1a8a16562c407fda8bad01eb47284c7965214da26a170c5ea6f6");
        }

        private static void Case_01082()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1082,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,10,18,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,15,38,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-1,98,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,11,13,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-11,83,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-4,61,46,1), new GeneratedEnemyUnit(-2,14,24,28,4), new GeneratedEnemyUnit(-7,18,94,9,4), new GeneratedEnemyUnit(16,19,8,8,3), new GeneratedEnemyUnit(20,20,52,22,3), new GeneratedEnemyUnit(-9,-8,12,45,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "a942232b342541d824fdb9f7fc3c82486566580bc030469c8d6fe56463e658f6");
        }

        private static void Case_01083()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1083,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,3,58,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-4,39,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-3,77,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-10,55,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,8,50,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,15,52,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-5,81,31,3), new GeneratedEnemyUnit(0,-3,39,45,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "b49915de7c29de4ac5dfcde816fcd18e696c01bb006ef6c5d26e2678fc683584");
        }

        private static void Case_01084()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1084,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,18,70,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,19,16,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,14,16,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,5,55,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-3,90,46,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "b13fb6899e78a8fb272271538050079eb8758331b690d1dea7684a60b7300592");
        }

        private static void Case_01085()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1085,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-12,76,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,12,78,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,6,89,13,3), new GeneratedEnemyUnit(12,2,61,21,4), new GeneratedEnemyUnit(3,-15,46,13,2), new GeneratedEnemyUnit(16,-1,90,10,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "e4f371ad4684d848f0c81c1546343d16336437c5bceec3262731432bd138f06d");
        }

        private static void Case_01086()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1086,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,2,23,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,16,74,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,11,83,40,4), new GeneratedEnemyUnit(-9,7,96,3,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "3defb0948314b822de6f988063d613596f25781465be5a0c4cdfd41ce893dd0e");
        }

        private static void Case_01087()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1087,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-6,41,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,16,86,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-3,59,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,1,35,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-3,12,44,1), new GeneratedEnemyUnit(3,10,66,19,2), new GeneratedEnemyUnit(-4,19,92,45,4), new GeneratedEnemyUnit(12,2,95,19,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "4b3de987a20124d9f779df6d00598b61b696df95e36fb4eee8dad9f4793bd993");
        }

        private static void Case_01088()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1088,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-12,44,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-10,96,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,13,32,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,19,12,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-1,23,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,13,16,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-9,27,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,6,67,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-1,19,37,3), new GeneratedEnemyUnit(5,-12,37,3,4), new GeneratedEnemyUnit(1,-2,57,26,3), new GeneratedEnemyUnit(12,-17,63,26,4), new GeneratedEnemyUnit(-10,3,60,34,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a66928dcb0fff85fa482c64a19091bdf4db0f799b390e61c7938fb15f41e2339");
        }

        private static void Case_01089()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1089,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,12,5,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,17,71,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-6,11,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,13,53,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,4,48,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-18,44,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,13,53,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,18,95,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-11,59,28,4), new GeneratedEnemyUnit(-13,14,77,19,2), new GeneratedEnemyUnit(-17,2,47,16,2), new GeneratedEnemyUnit(-4,13,8,40,4), new GeneratedEnemyUnit(-15,19,19,5,1), new GeneratedEnemyUnit(3,-5,99,12,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "7904a76e640d58f8af5bdec89af74bccb3f164fba651656842ea56055dbc3640");
        }

        private static void Case_01090()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1090,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,0,59,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-8,15,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,5,62,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,16,38,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-15,60,1,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "7aeb06595e8082049a6929463412ce5a88afdf646e40f54906dc83bf1a5c839f");
        }

        private static void Case_01091()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1091,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-18,19,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-14,83,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-11,75,39,3), new GeneratedEnemyUnit(5,-7,25,5,2), new GeneratedEnemyUnit(-16,5,32,9,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "902885a4ae482a77fb7536abda15d83e6b2d9017069b2d42731ecb215c9a9658");
        }

        private static void Case_01092()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1092,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,14,23,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,10,94,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-15,84,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-11,89,33,4), new GeneratedEnemyUnit(8,5,79,37,3), new GeneratedEnemyUnit(-16,7,68,28,4), new GeneratedEnemyUnit(-19,14,74,1,1), new GeneratedEnemyUnit(18,16,48,8,3), new GeneratedEnemyUnit(16,9,25,11,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "6af7d962d0e5e921d7129819bbd8ec77f763dee2a4bba6c663466e23c4e30863");
        }

        private static void Case_01093()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1093,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-8,30,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,13,31,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-2,51,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,1,29,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-2,89,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-5,21,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-13,44,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-2,30,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-19,97,41,2), new GeneratedEnemyUnit(-6,1,16,2,1), new GeneratedEnemyUnit(14,-4,89,19,3), new GeneratedEnemyUnit(4,7,86,17,1), new GeneratedEnemyUnit(16,-13,28,50,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "4ca51db676bafebce3e6b0259bdc0c378609194052b68bd824c54c50d3c5cc54");
        }

        private static void Case_01094()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1094,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-8,9,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,9,76,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,15,47,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,5,61,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,18,36,30,2), new GeneratedEnemyUnit(-18,-20,54,22,1), new GeneratedEnemyUnit(13,6,27,19,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "b99e9697bac34b96088395070dca04a0ab01bb544ad7e9d4e6ad8339a1fadcf5");
        }

        private static void Case_01095()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1095,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-11,21,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-10,23,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-18,10,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,17,14,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-14,57,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,10,5,1,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "cc52a669eb3fe4a307082913f578d0779b7ca0a06d69a0932d0c40e39e080eee");
        }

        private static void Case_01096()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1096,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-20,91,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,6,50,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,20,74,4,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "752d9be301d6833acdff955a7bd4dac1f30c7dc839e30bbb7c75b96cda5a2526");
        }

        private static void Case_01097()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1097,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-9,32,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-2,59,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,0,31,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,10,9,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-6,43,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,9,66,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,5,68,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,2,11,42,1), new GeneratedEnemyUnit(-7,-15,64,21,2), new GeneratedEnemyUnit(15,-16,47,29,4), new GeneratedEnemyUnit(-6,-17,88,14,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6ea5bb6400af004cf153c8e76dbe306502084d929711638813bec6b8a9d707d8");
        }

        private static void Case_01098()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1098,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-20,66,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,6,49,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,6,35,23,1), new GeneratedEnemyUnit(-2,5,92,9,2), new GeneratedEnemyUnit(-5,5,88,17,4), new GeneratedEnemyUnit(-6,6,71,19,3), new GeneratedEnemyUnit(8,5,56,22,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "0d5d77451f68a173d3848aa94d6230617976acc11ab729b67372d9fca3ab7057");
        }

        private static void Case_01099()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1099,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-6,86,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,19,52,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-12,62,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-1,46,14,4), new GeneratedEnemyUnit(-8,8,25,15,4), new GeneratedEnemyUnit(-12,-3,26,36,4), new GeneratedEnemyUnit(-4,-19,15,15,2), new GeneratedEnemyUnit(-12,19,8,44,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "a30111f467fee10bd4101cd31f55d15a83dd250917a16e6e2bc5754327b7ec15");
        }

        private static void Case_01100()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1100,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,9,73,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-13,13,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-18,28,3,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "d1fd2f2854395b97b77dcc10ce46e96f749436b476d449295ff06d0d0bc43e0a");
        }

        private static void Case_01101()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1101,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-19,80,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-16,64,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,19,78,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-12,68,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,11,66,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-19,98,9,1), new GeneratedEnemyUnit(-2,-20,15,36,3), new GeneratedEnemyUnit(18,-17,34,35,3), new GeneratedEnemyUnit(-17,-10,16,46,3), new GeneratedEnemyUnit(0,3,21,47,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "c656bf8f7547d99527d7e4555bbf3b5e66e0465d8a2653247170cfb66866f4ef");
        }

        private static void Case_01102()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1102,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,15,52,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,17,72,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,0,68,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,6,46,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-14,12,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,19,20,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,1,15,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,20,57,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,0,10,28,4), new GeneratedEnemyUnit(-12,-8,71,31,1), new GeneratedEnemyUnit(-10,-18,29,5,3), new GeneratedEnemyUnit(-2,-8,43,7,2), new GeneratedEnemyUnit(-16,-15,34,17,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "9b465ca7cbab2512e8d851132e769d6d745a47adb3208355b56cc196b569f1fb");
        }

        private static void Case_01103()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1103,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-5,27,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-8,43,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,20,46,45,3), new GeneratedEnemyUnit(12,1,86,24,2), new GeneratedEnemyUnit(-3,-4,99,43,1), new GeneratedEnemyUnit(19,14,95,20,4), new GeneratedEnemyUnit(2,10,24,32,3), new GeneratedEnemyUnit(-3,8,67,28,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "8f11a50ab3a8c150653ff8b325c390742fad793f1f48456ea2d2ba401e203355");
        }

        private static void Case_01104()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1104,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,12,57,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-9,26,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,6,48,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,17,69,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-18,93,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-8,25,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-19,82,5,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "f97bb8307da121315228edfb54b9cb4fb7cfd336bf83d64dab24fd9970468c7b");
        }

        private static void Case_01105()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1105,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-6,84,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,5,33,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-19,5,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-1,66,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-18,36,7,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "acd8264d1ee29695d334541a874f6f69790d0670e5f69d721da9bd7b6ab0677f");
        }

        private static void Case_01106()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1106,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-9,34,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,4,87,34,4), new GeneratedEnemyUnit(16,-12,72,4,4), new GeneratedEnemyUnit(15,-3,44,32,3), new GeneratedEnemyUnit(14,5,82,44,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "f0884ac79dd50fe0580ce6b0d054298df98018d135ed01209ad4005e82cc2774");
        }

        private static void Case_01107()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1107,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-11,75,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,1,63,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,13,97,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,13,30,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,16,60,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-16,12,3,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c298b09059bd788735b1422404bf2b4ffb1762031ea1e63a2cb62f2545ee2df1");
        }

        private static void Case_01108()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1108,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,10,37,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,7,6,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,17,21,34,1), new GeneratedEnemyUnit(19,0,15,7,2), new GeneratedEnemyUnit(6,7,60,41,4), new GeneratedEnemyUnit(16,-14,23,29,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "490f3a8be95d915e945c18ed0b42fc1f3249506230725c0df83e5a7021ad684d");
        }

        private static void Case_01109()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1109,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,4,24,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-19,95,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,16,12,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,11,72,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,6,79,39,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "c8e50a46d00d5a68988021dce2a534351172bb53736e51da4a09fddb009009a8");
        }

        private static void Case_01110()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1110,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,6,31,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,0,19,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,5,15,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-14,89,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-2,87,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,13,28,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,10,69,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,9,25,40,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0d29438641af6aa7ef33ddaaf9580d8b2b7f290152106bb0858334a86acb3c40");
        }

        private static void Case_01111()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1111,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,17,62,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-2,9,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-2,15,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-11,72,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-18,49,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,3,31,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-10,53,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,7,79,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,14,32,3,2), new GeneratedEnemyUnit(-5,2,10,35,4), new GeneratedEnemyUnit(9,-10,68,23,1), new GeneratedEnemyUnit(-1,-19,53,40,4), new GeneratedEnemyUnit(-12,-13,21,32,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "2826cbc2ab19c1ce31ef208d3cd2f79b317c3d8282108531472b3d2aa8c7d1d6");
        }

        private static void Case_01112()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1112,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,17,100,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,16,54,9,3), new GeneratedEnemyUnit(-20,15,92,45,1), new GeneratedEnemyUnit(-20,-6,17,19,2), new GeneratedEnemyUnit(-1,-5,26,14,3), new GeneratedEnemyUnit(17,20,100,20,2), new GeneratedEnemyUnit(-4,-11,74,19,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "efa79dd14bd507aba30d7cec3bc5a5bb929a403d32662c1f794eb5253eeccb9d");
        }

        private static void Case_01113()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1113,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-15,46,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,20,44,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-6,50,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-12,99,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-18,15,1,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "208e9b28dc4c01a61fe26950b3e9afb131c2fa4bf27a7fe34663c6a1bfa9ee08");
        }

        private static void Case_01114()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1114,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,7,43,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,1,77,26,3), new GeneratedEnemyUnit(-15,-1,32,37,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "407dcd377f34d3a1d1878aea242c6a5ac3c4a229401edab171562a0f2db5f4ec");
        }

        private static void Case_01115()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1115,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,19,66,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-2,67,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,1,18,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-3,61,43,4), new GeneratedEnemyUnit(8,-3,53,50,3), new GeneratedEnemyUnit(3,3,87,11,2), new GeneratedEnemyUnit(-14,-14,42,23,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "14c3aaf5e11c7eab803380d177f2efe849995f30fc1f50309a0e3ce9a62af7cc");
        }

        private static void Case_01116()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1116,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-2,32,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,10,68,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-20,18,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,14,14,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,10,85,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-2,6,30,2), new GeneratedEnemyUnit(-15,-10,42,27,3), new GeneratedEnemyUnit(2,-12,24,36,2), new GeneratedEnemyUnit(-16,-7,54,40,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "6cae55bb48df9ae8827d205532b8608e14697f672353b9e9a5d78754cbd2d9a8");
        }

        private static void Case_01117()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1117,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-18,13,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,0,40,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,10,61,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-2,56,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,7,97,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,16,36,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-13,84,22,3), new GeneratedEnemyUnit(6,-11,51,35,3), new GeneratedEnemyUnit(10,-20,75,39,2), new GeneratedEnemyUnit(14,2,92,9,3), new GeneratedEnemyUnit(1,-11,38,2,2), new GeneratedEnemyUnit(12,9,19,10,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "3be999417cbb80a7ae652e42d0f6281f820cf15642cb656e90a8c3392a3c75d2");
        }

        private static void Case_01118()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1118,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-3,46,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,3,35,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-18,96,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,20,64,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-8,56,9,3), new GeneratedEnemyUnit(16,17,11,6,3), new GeneratedEnemyUnit(2,9,71,30,3), new GeneratedEnemyUnit(4,-7,80,23,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "b7aa9997898cda7f093ce027e1e789d995145a381c43dc844c5c1b1c3ab13660");
        }

        private static void Case_01119()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1119,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,19,5,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,12,23,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-17,22,45,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "fdafc122eb865315e9ceed391a5c6d4add41e2b314e68fa660b236bdf9d81b60");
        }

        private static void Case_01120()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1120,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,2,17,5,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "aa08e6ada88a1ed05a1ac33dbeaa7a249ac9107485e0e681d5f011536aa7ebf8");
        }

        private static void Case_01121()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1121,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-17,84,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-16,23,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,12,26,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,9,43,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,7,13,11,1), new GeneratedEnemyUnit(7,5,56,27,3), new GeneratedEnemyUnit(-18,0,31,42,3), new GeneratedEnemyUnit(11,9,60,15,2), new GeneratedEnemyUnit(-17,-17,41,8,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "3880286abc257b6629694c48381488025a463876903fdc74af3e14de5860620e");
        }

        private static void Case_01122()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1122,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,4,83,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-9,56,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,1,48,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-15,20,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-9,15,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,2,48,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,12,7,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-16,53,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-5,85,24,2), new GeneratedEnemyUnit(11,-17,13,41,4), new GeneratedEnemyUnit(-19,-7,40,31,3), new GeneratedEnemyUnit(15,11,11,3,2), new GeneratedEnemyUnit(9,9,78,49,3), new GeneratedEnemyUnit(-16,7,66,24,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "2a10d8793461e1690c3649baed664a8dca69bb6c549d179bf3aef12bc6079665");
        }

        private static void Case_01123()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1123,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,4,50,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-4,15,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-14,55,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-12,88,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,20,34,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-7,19,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-10,17,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-19,42,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-15,90,30,4), new GeneratedEnemyUnit(7,-4,59,10,3), new GeneratedEnemyUnit(-11,-10,11,8,4), new GeneratedEnemyUnit(20,-6,55,7,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "033fb8fda2a84014044c83f8f573bba6453143d0bf48bf59d8859172cddfd24a");
        }

        private static void Case_01124()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1124,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-6,67,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,11,45,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-1,47,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-17,91,1,4), new GeneratedEnemyUnit(18,15,35,21,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "6c610ac8e263ae0570b729f1979edfb9944d14a96211853ab6137ae6d8e8d6f7");
        }

        private static void Case_01125()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1125,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-8,69,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,17,79,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-12,11,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-14,58,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,1,79,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,5,39,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,5,52,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,5,27,30,4), new GeneratedEnemyUnit(-3,-20,17,7,2), new GeneratedEnemyUnit(-8,-6,37,18,2), new GeneratedEnemyUnit(19,-14,80,47,3), new GeneratedEnemyUnit(13,12,6,9,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "18fabc6d3199d4cf85db8ea791164a53c5f4da8b445c7bd1a698dcb42b1c6721");
        }

        private static void Case_01126()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1126,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,8,7,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,16,72,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-8,58,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-3,42,4,4), new GeneratedEnemyUnit(11,13,46,21,1), new GeneratedEnemyUnit(-13,-13,16,7,3), new GeneratedEnemyUnit(-14,3,85,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "3592240de7c9c507a24858eb605d4da39b8609035db01f6e29dd2c061a146e5d");
        }

        private static void Case_01127()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1127,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,6,17,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,3,94,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,17,20,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,14,45,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,15,41,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,7,71,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,1,12,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,16,34,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-9,24,4,1), new GeneratedEnemyUnit(11,4,7,50,4), new GeneratedEnemyUnit(-9,-1,20,10,2), new GeneratedEnemyUnit(5,5,15,26,3), new GeneratedEnemyUnit(2,-10,94,11,4), new GeneratedEnemyUnit(-3,1,46,7,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "b7a3a666472a3120f98ec1c80467c87a9f5e6113f8cf8d47ba7988f12f517b84");
        }

        private static void Case_01128()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1128,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,18,71,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,6,9,34,1), new GeneratedEnemyUnit(5,16,49,47,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "e92856a3bf9999babc0019a2031a07f31913c86ee8770ce5db6ec51070caa5cd");
        }

        private static void Case_01129()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1129,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,18,98,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,8,96,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-15,11,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,20,57,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,2,10,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,17,92,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-8,17,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-1,12,4,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "97aebe1b89ab936e8722b82f8bf5171d96990d5bfba03107ef434272b747fb98");
        }

        private static void Case_01130()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1130,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-13,70,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-19,50,48,4), new GeneratedEnemyUnit(15,-18,60,13,1), new GeneratedEnemyUnit(-7,17,47,19,1), new GeneratedEnemyUnit(1,-4,10,2,4), new GeneratedEnemyUnit(0,-6,20,7,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "973b8c5208bf88cfe458e69599bc68005be292d2b827835338f27ece0c2d46a3");
        }

        private static void Case_01131()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1131,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-18,92,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,16,57,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,0,27,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,6,22,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-12,14,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,16,61,16,3), new GeneratedEnemyUnit(-19,-1,95,13,1), new GeneratedEnemyUnit(5,18,15,6,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "13376d19fcd0a8145e0381766fec58ae8899f42d1ad1f7922b5229f2b0a48f9a");
        }

        private static void Case_01132()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1132,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,16,97,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-4,61,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-16,36,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-18,30,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-20,23,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,13,93,23,3), new GeneratedEnemyUnit(5,-14,32,7,3), new GeneratedEnemyUnit(-1,13,71,47,1), new GeneratedEnemyUnit(-2,11,75,8,3), new GeneratedEnemyUnit(5,0,22,23,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "c9f6aca0d9a623fea3a3d76b4a1c622dac8e6a51678151dfc5c08456e773ddfd");
        }

        private static void Case_01133()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1133,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,10,9,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,9,89,18,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "fcefdd6c37a2ba985e8f9aa70cea676b480503824e29df765af30cc7fe8c7c07");
        }

        private static void Case_01134()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1134,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-13,34,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-7,39,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-16,74,16,4), new GeneratedEnemyUnit(-5,-18,37,1,4), new GeneratedEnemyUnit(-9,7,39,16,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "f858b2d5e5ded3323a06af34aac64e39a2830470baf28d184770122b6b6d1f55");
        }

        private static void Case_01135()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1135,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-2,65,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-15,6,34,1), new GeneratedEnemyUnit(18,10,20,18,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "18e96e2acc0576215a81604fd4aa37e2a53df63d652fbb01fd8e030f8ca028e0");
        }

        private static void Case_01136()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1136,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,14,73,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-8,47,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,14,39,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-18,19,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,2,19,34,1), new GeneratedEnemyUnit(-15,7,73,48,2), new GeneratedEnemyUnit(10,4,99,48,1), new GeneratedEnemyUnit(-5,-13,64,6,2), new GeneratedEnemyUnit(-13,9,40,7,3), new GeneratedEnemyUnit(-18,-19,77,12,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "562bcb0f183374e16ca67bb3eea0e3d11716657dd8378da78ebc1d4608cb5fe1");
        }

        private static void Case_01137()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1137,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-13,89,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-19,63,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,13,79,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-10,36,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-1,51,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-20,10,6,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "2c35281712de8f47f579a4f23eca0c9dc66fb52b11e6c475159ad25fce208752");
        }

        private static void Case_01138()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1138,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,12,94,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,6,68,16,1), new GeneratedEnemyUnit(13,-3,38,4,1), new GeneratedEnemyUnit(-14,-11,88,28,3), new GeneratedEnemyUnit(-13,0,84,12,4), new GeneratedEnemyUnit(-4,-6,59,23,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "d8bea02b4536941252ead0202824c68caf270d3963d0783f8098b47ba3b7cca2");
        }

        private static void Case_01139()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1139,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,10,92,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,7,47,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-13,43,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-9,55,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,1,25,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,0,56,50,2), new GeneratedEnemyUnit(16,-9,51,39,2), new GeneratedEnemyUnit(16,-6,84,47,3), new GeneratedEnemyUnit(-15,6,72,18,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "af596ff2f3884e6f02db3df5caebc853d90a06255909f4e314249a27bf5526df");
        }

        private static void Case_01140()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1140,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,5,87,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-18,29,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-3,50,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,15,20,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,7,94,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-20,36,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,13,13,1,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "be7983d71c192ff8a152e12da2f9cae42e96d91cb902804b25428926d35453fc");
        }

        private static void Case_01141()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1141,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-9,51,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-9,20,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-18,70,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,14,49,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,14,14,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-8,15,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,7,67,49,1), new GeneratedEnemyUnit(15,-17,36,9,2), new GeneratedEnemyUnit(6,5,58,49,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "8798660dd10c296316b24e0b3695713e983461b3b42773b6f61d5bb9f13d1fa6");
        }

        private static void Case_01142()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1142,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,18,60,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,14,91,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-19,43,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-1,72,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,8,73,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,9,70,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,14,77,19,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2589c6a373fc7804d23b5ad6f484f870066b88b80fc991dccc7671ecb6a7ccb5");
        }

        private static void Case_01143()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1143,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,10,51,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,17,64,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,8,63,5,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "1fbf7947d0c285626199a88ff13af0c20baef16d8d55bcd6762032e45ca6d492");
        }

        private static void Case_01144()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1144,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,14,28,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,15,100,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,0,89,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,18,83,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-10,65,7,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "9e8d38289e252905f34b2567562b13ee9c4d4647986da03d935dc6069911dfef");
        }

        private static void Case_01145()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1145,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,6,85,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-10,94,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,14,40,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-13,34,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-7,51,45,2), new GeneratedEnemyUnit(2,-3,28,12,4), new GeneratedEnemyUnit(19,17,65,44,2), new GeneratedEnemyUnit(-17,4,54,39,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "2c18df16b2c729908c186172dcf8150e51436d073c004ff86826ba4986827f71");
        }

        private static void Case_01146()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1146,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-12,97,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,16,82,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,6,31,23,4), new GeneratedEnemyUnit(-15,-2,41,36,4), new GeneratedEnemyUnit(19,-15,30,8,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "338e162cb62e1b6d133dd0d20f3e67d0a3a5933ef293976447883b0625ba16a7");
        }

        private static void Case_01147()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1147,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,4,51,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-13,16,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,14,24,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,20,48,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,6,27,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,5,74,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,5,73,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,10,62,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-20,36,44,3), new GeneratedEnemyUnit(-19,-12,33,14,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "094cf507f35fa0b3b1dd48045196eb7084a00f72330926e87631848023a3b515");
        }

        private static void Case_01148()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1148,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,9,58,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-4,60,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-18,26,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,6,82,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-16,16,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-3,44,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-14,86,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,16,97,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-3,87,26,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2725e6840992f563b628959a3ac3e886209708e39e87f47dd0ec9661178094fd");
        }

        private static void Case_01149()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1149,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-5,31,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,16,87,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,9,71,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,16,77,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,2,33,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-13,27,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-20,83,3,2), new GeneratedEnemyUnit(-16,12,44,36,4), new GeneratedEnemyUnit(9,-11,36,12,1), new GeneratedEnemyUnit(3,-6,80,50,2), new GeneratedEnemyUnit(6,-20,86,33,4), new GeneratedEnemyUnit(18,13,51,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "5e4cdb6ef9660ecedfa8b1f5aefa065005d115baf270a851d738d32ceb55985c");
        }

        private static void Case_01150()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1150,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-20,97,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-3,13,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-20,80,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-8,18,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,20,64,41,4), new GeneratedEnemyUnit(7,-19,30,5,1), new GeneratedEnemyUnit(-4,5,83,17,2), new GeneratedEnemyUnit(-17,-1,80,2,4), new GeneratedEnemyUnit(-16,-19,80,21,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "95fd7b6c43abef5d15e2a3f8dce8645f19462b51ff4ed738a027c127a5986574");
        }

        private static void Case_01151()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1151,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-5,56,4,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "019038bbbd00803bd721f40dd14c2658fbcc6ecf71e4640d16a9a8b2e0c464df");
        }

        private static void Case_01152()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1152,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-14,70,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-7,10,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,4,82,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,18,20,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-1,44,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-9,49,6,1), new GeneratedEnemyUnit(3,-19,33,20,1), new GeneratedEnemyUnit(-15,-18,34,10,4), new GeneratedEnemyUnit(-17,3,90,11,3), new GeneratedEnemyUnit(11,-18,73,10,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "094b51b447adad5063a3aceb5e1603a7eca0c494ef168697ab4c975fe0ddd2ae");
        }

        private static void Case_01153()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1153,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-3,25,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,19,80,25,4), new GeneratedEnemyUnit(-17,13,61,17,2), new GeneratedEnemyUnit(-17,11,21,26,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "f3fe11cfc7fc95f7d585303691f7718576a4b9275219102a509aa58ce43a7611");
        }

        private static void Case_01154()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1154,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-12,42,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-16,14,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-6,93,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,19,95,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,1,72,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,1,28,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,7,70,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-9,80,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-16,55,50,1), new GeneratedEnemyUnit(15,2,19,20,2), new GeneratedEnemyUnit(11,-11,10,2,1), new GeneratedEnemyUnit(10,6,62,38,4), new GeneratedEnemyUnit(-16,14,26,36,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8e6071431abc718ad574a3028df1abb9ee8ceed4abe773773cd81387be17dc51");
        }

        private static void Case_01155()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1155,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,3,23,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-5,55,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-16,23,3,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "4ef50393d0cddfa66cd49ad88ac662e6431a12f2f9692e710743968d03f64168");
        }

        private static void Case_01156()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1156,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,4,69,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-4,73,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-1,71,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-12,99,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,9,53,6,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "534ae7998b5a0a602a32b33d182612a8e6421f96705788122fc5232f87d3c7c5");
        }

        private static void Case_01157()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1157,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,4,73,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,1,47,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-15,22,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,10,21,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,16,91,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,14,89,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,6,89,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,0,43,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-12,26,49,3), new GeneratedEnemyUnit(3,-7,41,5,3), new GeneratedEnemyUnit(-19,19,50,30,3), new GeneratedEnemyUnit(-13,9,75,45,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8e30a10b0338feeb6f2462e6e5c68f2ef6e90397a558fc07c583f14859b5f0b9");
        }

        private static void Case_01158()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1158,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-4,71,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,15,97,10,2), new GeneratedEnemyUnit(-7,19,37,5,3), new GeneratedEnemyUnit(-18,5,61,13,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "cd561841f9300125941a5f53189a7fd3b0a14e148ca2f891dee852fab547ae29");
        }

        private static void Case_01159()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1159,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,8,99,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,5,63,34,1), new GeneratedEnemyUnit(13,-2,90,47,2), new GeneratedEnemyUnit(9,-19,50,48,4), new GeneratedEnemyUnit(-8,-18,71,10,3), new GeneratedEnemyUnit(-2,-18,48,46,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "808a952aaccc79b4b25fafba44e3e6a1bdeadb3989b3e211e276a93fe69c00d0");
        }

        private static void Case_01160()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1160,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-20,25,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-19,37,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,3,100,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,0,31,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,6,63,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,19,53,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,0,52,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,4,63,9,2), new GeneratedEnemyUnit(6,8,66,1,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "cad02937ca4e3a1036a9f69bbe49c1a293857e800014f6d6628b0207bd729f2a");
        }

        private static void Case_01161()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1161,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,16,47,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,14,35,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,8,35,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-13,75,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,20,85,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,11,43,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-8,63,11,3), new GeneratedEnemyUnit(-17,2,8,8,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "ff837f14e5a45b52c7dfdfe419b00fd263c6bffb8a40118f3f3e01003657ef04");
        }

        private static void Case_01162()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1162,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-3,25,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,9,57,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-3,34,22,2), new GeneratedEnemyUnit(15,2,46,23,2), new GeneratedEnemyUnit(4,-6,21,3,1), new GeneratedEnemyUnit(3,18,23,3,2), new GeneratedEnemyUnit(11,0,98,47,3), new GeneratedEnemyUnit(-7,15,33,50,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "f23d74b6334081f9b0b858b798b1a14f6a78876d35240fbe7257573b838a525b");
        }

        private static void Case_01163()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1163,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-6,67,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,18,41,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,19,91,8,4), new GeneratedEnemyUnit(-12,-5,36,12,4), new GeneratedEnemyUnit(9,10,34,10,4), new GeneratedEnemyUnit(14,-3,22,23,1), new GeneratedEnemyUnit(1,20,38,37,4), new GeneratedEnemyUnit(-12,10,91,33,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "3d485018f9db8271f3b922a6a989745aab7474f1d696cbec8bde04d63430b520");
        }

        private static void Case_01164()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1164,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,5,71,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,19,19,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-9,91,1,2), new GeneratedEnemyUnit(-2,4,47,6,1), new GeneratedEnemyUnit(-16,-7,83,46,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "5c7897a903a33656f15bda87e30c3142e17962755dc4e55abe6e27612721e6b4");
        }

        private static void Case_01165()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1165,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-1,96,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,5,64,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-20,77,44,1), new GeneratedEnemyUnit(-18,2,39,36,3), new GeneratedEnemyUnit(2,-15,35,11,1), new GeneratedEnemyUnit(12,19,48,50,2), new GeneratedEnemyUnit(-1,-16,30,41,3), new GeneratedEnemyUnit(20,10,97,7,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "0716362df269e59831aac4e3d38f1dfe2f8939f8493d938e51ceacbb49f46662");
        }

        private static void Case_01166()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1166,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,12,14,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-5,28,49,2), new GeneratedEnemyUnit(4,17,51,32,2), new GeneratedEnemyUnit(5,-13,67,42,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "a471f5d9c439d96bde7f0728a2428929d556cae6a78624f34907f8986c5d98df");
        }

        private static void Case_01167()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1167,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,5,49,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-20,84,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,6,14,33,4), new GeneratedEnemyUnit(20,-1,7,22,4), new GeneratedEnemyUnit(14,-1,92,19,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9231eab06c0f3e5c1ce3dde72876b187e48c3f4c3c70b1f2499fc5a92b182584");
        }

        private static void Case_01168()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1168,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-10,29,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-17,55,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,8,85,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-8,97,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,17,63,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-1,73,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,10,49,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-15,84,34,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9976291bddec4b16da7dbfc8ff37738ca4493abe9073510076bc30a5e52b8b25");
        }

        private static void Case_01169()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1169,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-12,95,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,7,76,40,4), new GeneratedEnemyUnit(9,20,33,37,3), new GeneratedEnemyUnit(13,18,99,7,3), new GeneratedEnemyUnit(-10,19,28,7,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "05a0df92109d679d89bd82d339c018dff38a274bcb32b0ff33960a78af5acbe4");
        }

        private static void Case_01170()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1170,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,20,86,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,12,85,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-9,60,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-14,44,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-4,77,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,2,7,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,0,17,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,15,93,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-3,53,4,1), new GeneratedEnemyUnit(-15,-17,36,20,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ffaaa1c6b2a4e95634b0915b53ee97fbb39c154a5eb84b3fc59dd28fea81b90d");
        }

        private static void Case_01171()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1171,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-19,31,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-19,69,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,9,41,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-17,96,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-16,10,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,10,74,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-12,61,1,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "fe2b5b9a3ea2ce025a936f932c880e05a9d984bc4e2d633083a4f664a374ed67");
        }

        private static void Case_01172()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1172,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-19,88,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,17,18,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,19,83,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-6,34,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,13,68,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-19,44,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-1,66,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-11,79,14,2), new GeneratedEnemyUnit(2,-17,17,44,1), new GeneratedEnemyUnit(-5,16,46,22,4), new GeneratedEnemyUnit(19,19,37,12,4), new GeneratedEnemyUnit(8,-20,60,47,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "fde66e015969ff58e2166f229bca96149e7831f8e7a5a8291930b957a461653b");
        }

        private static void Case_01173()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1173,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-5,9,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-19,30,42,4), new GeneratedEnemyUnit(-11,12,39,39,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "5c3b12ef961388135e9615a658d6fe7870985a5c9433b18fe4f880ffda356943");
        }

        private static void Case_01174()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1174,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,18,56,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,4,77,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,4,49,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-14,60,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-13,59,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-12,62,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-13,36,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,14,42,14,1), new GeneratedEnemyUnit(13,-13,85,25,2), new GeneratedEnemyUnit(-9,-9,20,30,1), new GeneratedEnemyUnit(-4,17,31,38,3), new GeneratedEnemyUnit(18,15,8,2,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "1f9d99f4170e48d5b49bbaf4e420719c812ab40a151927bd75a7041334111d9c");
        }

        private static void Case_01175()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1175,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-17,32,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-5,87,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-2,68,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-11,53,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,3,67,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,11,67,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-8,94,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,3,59,50,3), new GeneratedEnemyUnit(0,-1,18,46,2), new GeneratedEnemyUnit(-4,10,76,41,2), new GeneratedEnemyUnit(-12,-1,40,9,1), new GeneratedEnemyUnit(-9,-4,22,31,4), new GeneratedEnemyUnit(-2,-5,45,43,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "8b27fc2b543e5dac797147c866a408ed14749f6ad8e16c93141f022e097b4f82");
        }

        private static void Case_01176()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1176,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-13,51,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-5,44,6,2), new GeneratedEnemyUnit(8,-8,66,39,2), new GeneratedEnemyUnit(4,19,15,42,2), new GeneratedEnemyUnit(-4,20,100,13,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "68df47533f854262fa7da06b051342727b8ac5820bb91c79cecc325f580c1f18");
        }

        private static void Case_01177()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1177,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-19,41,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,17,44,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-10,24,20,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "1d823aedae1e38f590a41df31ac59972da20e2ce0f5479eccb6094eee4e80a80");
        }

        private static void Case_01178()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1178,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-19,5,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,0,18,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,16,7,19,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "efbda4ba358205408a5a0f1d955c4a544c5164c43c7a5539018b0bd256323254");
        }

        private static void Case_01179()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1179,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-10,89,1,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "621eff957d46be126b0e810ebcd769b531c6404cf96c820dd0978d3ce093fef6");
        }

        private static void Case_01180()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1180,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-13,100,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-7,16,20,2), new GeneratedEnemyUnit(-9,16,26,7,3), new GeneratedEnemyUnit(-20,20,79,11,4), new GeneratedEnemyUnit(-3,-19,83,21,3), new GeneratedEnemyUnit(-20,-14,93,46,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "1df7d1cff5757bac134e3f0e70436368a9ad1a7ecee1841c21ceba9296c50dcc");
        }

        private static void Case_01181()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1181,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,5,58,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-8,82,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-17,49,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,12,49,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-10,68,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-12,21,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-10,50,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-5,65,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-19,29,22,1), new GeneratedEnemyUnit(-12,16,7,33,1), new GeneratedEnemyUnit(20,3,6,2,3), new GeneratedEnemyUnit(11,19,30,19,3), new GeneratedEnemyUnit(-12,9,9,42,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "d5087498c0945008901c5185519968f2866b1130f45f5bf0623941a875a08438");
        }

        private static void Case_01182()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1182,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,16,71,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-3,84,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-20,99,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,9,97,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-13,17,11,2), new GeneratedEnemyUnit(-16,-1,57,42,2), new GeneratedEnemyUnit(12,0,53,8,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "371aa7e0dcece26e66d13fa1cd6d0546508f9305788b0f133184323c5205e9d9");
        }

        private static void Case_01183()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1183,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,20,56,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-18,82,6,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "5c2184a27bbbb00b97ea622d670386954e975810e8b43bf62b699ac341f52199");
        }

        private static void Case_01184()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1184,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,2,19,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-9,22,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-8,28,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-18,54,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-9,22,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,9,62,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-19,59,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-1,55,26,3), new GeneratedEnemyUnit(-9,11,6,1,2), new GeneratedEnemyUnit(-12,-12,73,12,4), new GeneratedEnemyUnit(12,-8,60,33,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "dce05c7ba378fb6cb9687958e4198a74f403b9d3aacf394bd09b8c43dc4d9f94");
        }

        private static void Case_01185()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1185,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-15,28,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,10,84,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-15,60,7,2), new GeneratedEnemyUnit(-11,-8,69,19,4), new GeneratedEnemyUnit(-20,-16,59,27,3), new GeneratedEnemyUnit(14,1,73,27,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "e4e11790335a02d140c9ec060d519b191e1e72e5df4bcc861db5edfb574cfe65");
        }

        private static void Case_01186()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1186,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,6,27,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,6,26,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-11,86,40,4), new GeneratedEnemyUnit(1,-10,35,3,4), new GeneratedEnemyUnit(1,-7,74,32,2), new GeneratedEnemyUnit(0,18,89,31,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "096c1bda12cc23bbe52fa4ad846d10b83486a9725bd7d89249c9d6322a92c272");
        }

        private static void Case_01187()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1187,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-12,49,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,4,19,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,11,54,2,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "fcf34dd9ffdf4d55516dbb82df7715f5651bb5698b33cb0c0bab3ecfda03d056");
        }

        private static void Case_01188()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1188,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-16,29,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,15,78,35,3), new GeneratedEnemyUnit(-13,-1,19,37,2), new GeneratedEnemyUnit(-20,-18,30,22,4), new GeneratedEnemyUnit(-20,6,69,18,1), new GeneratedEnemyUnit(-9,2,31,14,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "ee12a4db14b9cfa95ebe3e00d3984dd4d06c0d64d4202aa5d47e356a36413fbc");
        }

        private static void Case_01189()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1189,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-14,75,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,0,93,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-14,80,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,3,87,22,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "a50ef2c8d03ad4dc4cfd4371f879e5543b9f8c15d181a32ad655a7c1b612b227");
        }

        private static void Case_01190()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1190,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-12,95,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,0,81,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,6,97,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,7,40,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,10,94,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-15,15,10,1), new GeneratedEnemyUnit(17,-10,94,17,4), new GeneratedEnemyUnit(16,-13,5,30,3), new GeneratedEnemyUnit(5,-2,52,43,2), new GeneratedEnemyUnit(0,-4,30,41,4), new GeneratedEnemyUnit(-8,-9,74,13,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "35b220debf0792e2ef321c5878c1df1cb51c96cba1434bf0a4770d67a3d47b8a");
        }

        private static void Case_01191()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1191,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,16,39,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,5,54,19,4), new GeneratedEnemyUnit(-1,-10,9,13,4), new GeneratedEnemyUnit(-7,-12,64,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "ebc7ed523295d5a3a59ca157f447417948f423a0be634b8c6e36a3ab473ad482");
        }

        private static void Case_01192()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1192,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,3,67,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,7,23,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-7,5,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,14,92,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,2,36,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,5,43,6,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "16637c80b817069b20cf3cb0db0ea975a8bc6650954a4bb416af20cbb85ce2c3");
        }

        private static void Case_01193()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1193,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,10,60,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-10,13,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,14,92,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-1,27,7,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "6c66d0517affb1607db98d47f35d68d7caf1d35174d816ff32c5827b0a2b17c9");
        }

        private static void Case_01194()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1194,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,18,14,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-17,86,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-17,77,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,1,52,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,9,100,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,19,40,16,2), new GeneratedEnemyUnit(13,-7,24,1,3), new GeneratedEnemyUnit(17,-12,12,27,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "6f07e02a435abb81ba4bf6649408cb6803edde30b4e5885afd89fe399aca0b71");
        }

        private static void Case_01195()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1195,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-11,32,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,10,69,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-9,81,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-4,47,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,12,9,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,8,41,42,1), new GeneratedEnemyUnit(0,-1,21,41,1), new GeneratedEnemyUnit(-7,8,51,29,3), new GeneratedEnemyUnit(-13,-5,18,38,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "a8c806987f4bcb3c714b8aaf29ac2e32457550b772e6367b2072618374a9a9fc");
        }

        private static void Case_01196()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1196,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-12,43,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,15,94,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,14,26,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-12,49,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-4,33,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,17,30,50,2), new GeneratedEnemyUnit(-12,-11,42,28,1), new GeneratedEnemyUnit(11,5,63,6,3), new GeneratedEnemyUnit(10,10,81,13,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5296f6f97f07bc066e8dffb548c6e06b8a302e964e8efb30b5081e00b5216a57");
        }

        private static void Case_01197()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1197,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-20,34,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-15,20,21,2), new GeneratedEnemyUnit(8,-13,97,11,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "cdb4abd63f82fade445e59f75b8a8b81f8908bb49d11e9aacf60f3155456ab07");
        }

        private static void Case_01198()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1198,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,14,22,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,10,21,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,0,78,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,17,43,21,4), new GeneratedEnemyUnit(-11,8,70,33,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "637a6e791091e2dd8370d025e4a734e7efa0c775c7f99a925e32bfa8205a1057");
        }

        private static void Case_01199()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1199,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-12,6,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,1,88,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,14,24,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-18,16,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-6,63,20,2), new GeneratedEnemyUnit(1,-14,96,49,3), new GeneratedEnemyUnit(-2,8,5,18,2), new GeneratedEnemyUnit(11,6,58,47,2), new GeneratedEnemyUnit(-13,13,16,30,4), new GeneratedEnemyUnit(17,-10,25,1,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "74acac718fb2acf49af5b00fd21ec1f6b4626866a301b1d8870dea5fc5b474dc");
        }

        private static void Case_01200()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1200,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-6,71,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,10,97,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,1,58,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-16,67,28,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "865a025291fa13ed1906d3d85aa70bdea43996eec28194b720aad256485a7b57");
        }

        private static void Case_01201()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1201,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-4,23,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-20,55,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-18,67,27,4), new GeneratedEnemyUnit(-7,-10,74,16,3), new GeneratedEnemyUnit(-8,1,55,12,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "45b2668a86b18cff63054ad41da75bfd3de46a59052559b30a10963dd0102d7f");
        }

        private static void Case_01202()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1202,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,9,58,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-5,26,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,1,64,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-5,21,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,9,95,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-18,29,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,13,35,6,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "7ec3477a938121efe3730cde3d0608a850431dca76da2827105645e586e9b70f");
        }

        private static void Case_01203()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1203,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-6,24,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,2,5,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,4,74,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,17,60,46,4), new GeneratedEnemyUnit(5,3,60,26,1), new GeneratedEnemyUnit(20,-15,53,39,1), new GeneratedEnemyUnit(4,-1,59,9,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "d3b6b941534701fb3d77e4d0a927b07ffed6082cda96dc64058509728ad87c31");
        }

        private static void Case_01204()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1204,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,12,61,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,9,11,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,20,15,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-5,5,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,9,91,36,2), new GeneratedEnemyUnit(-8,-6,71,38,1), new GeneratedEnemyUnit(-3,8,61,30,1), new GeneratedEnemyUnit(18,-9,86,13,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "ababfe764cc14f5410cad5b5c0f8a9b32e1b72a227e335e1a6f0e9b751ebc54d");
        }

        private static void Case_01205()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1205,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,2,86,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-20,11,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-19,71,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,10,50,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-3,49,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,1,96,26,2), new GeneratedEnemyUnit(-5,-9,24,38,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "df3a042b7cc0300e1b6ef3dc957800fd9d125adcb476169b375efb4186a063f6");
        }

        private static void Case_01206()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1206,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-7,98,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,13,82,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,10,51,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,18,96,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-2,30,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,0,43,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-1,66,22,4), new GeneratedEnemyUnit(19,-9,47,12,2), new GeneratedEnemyUnit(-8,2,17,18,2), new GeneratedEnemyUnit(-14,-8,43,14,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "59ee6135d2eef4469514828cc7d4d4475e361131d026001f4683e25476a108cd");
        }

        private static void Case_01207()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1207,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,0,29,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-9,19,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,10,18,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,14,73,13,2), new GeneratedEnemyUnit(4,-4,75,7,4), new GeneratedEnemyUnit(-12,-12,36,38,2), new GeneratedEnemyUnit(5,1,58,37,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "3aebccb6f6d2a667aa0949bb64aac8f31cd2b00b28efcab8c6e06615d401f36d");
        }

        private static void Case_01208()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1208,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,15,47,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,0,21,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-6,13,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,15,13,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-20,89,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,19,89,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,14,53,45,2), new GeneratedEnemyUnit(4,-19,81,32,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0b8a9cd870e6a9d443925ccf119133a35216097d2bea158285081706c8bd40d7");
        }

        private static void Case_01209()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1209,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-11,46,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,6,71,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,12,37,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-2,36,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-16,87,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-3,81,7,1), new GeneratedEnemyUnit(0,-1,65,30,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "996f675f569397b4e49c364e99000e5e5fa11a0f934e0577db745051aac61992");
        }

        private static void Case_01210()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1210,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,5,51,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-13,37,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,0,13,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-5,51,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-16,85,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-17,87,5,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "f5d10cbebfa331435efcf1c4771acdefdf80d1c4a1acf05c3e1aeeeb490e3970");
        }

        private static void Case_01211()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1211,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-6,94,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-19,45,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-5,35,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-17,81,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,1,96,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,0,20,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,14,41,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,7,74,7,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "c15e3edd5d47085f8802b8302e7cf2bb5a29e49ba060f71164a4fd3ef7558a9a");
        }

        private static void Case_01212()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1212,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-3,73,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,11,87,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,15,11,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,14,14,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-19,46,2,1), new GeneratedEnemyUnit(3,-13,16,37,4), new GeneratedEnemyUnit(7,-16,22,16,3), new GeneratedEnemyUnit(2,-15,6,23,1), new GeneratedEnemyUnit(6,10,19,10,1), new GeneratedEnemyUnit(20,-13,87,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "063fdba8be074dd3f49add1e3c5090f41b36c1d7abb37f58db936a946226234e");
        }

        private static void Case_01213()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1213,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-14,11,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,18,7,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,13,37,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-15,18,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-1,24,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,6,88,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,3,14,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-3,93,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-2,94,10,3), new GeneratedEnemyUnit(-20,12,47,32,3), new GeneratedEnemyUnit(-7,0,28,19,2), new GeneratedEnemyUnit(-14,-3,55,11,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "65b17b101883bebd15b1ae7520b2dceb1fb180a3d7233c9d7b403d39d6a7537d");
        }

        private static void Case_01214()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1214,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,0,79,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,20,46,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-16,34,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-9,70,39,4), new GeneratedEnemyUnit(-18,-11,85,25,1), new GeneratedEnemyUnit(18,0,34,25,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "913097250e65837348a6668b1db0411624542034ed6db5bdb300eaa9b60b4bd4");
        }

        private static void Case_01215()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1215,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-8,89,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-20,20,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-6,30,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,11,17,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,6,22,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-19,25,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,4,56,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,12,100,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,9,35,36,2), new GeneratedEnemyUnit(-9,6,87,2,3), new GeneratedEnemyUnit(12,9,67,2,2), new GeneratedEnemyUnit(10,-20,99,16,2), new GeneratedEnemyUnit(0,-19,44,13,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5c067b6c3f6e52c517da3706a22f1aa6aab08c25fbb986eae6e7ef2f2ebb3cbe");
        }

        private static void Case_01216()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1216,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,16,41,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,1,22,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-18,29,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,18,94,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,14,13,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,0,66,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-10,24,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-1,77,1,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "026d276109e5b019b75532cde808a36898b392669d6c8da360a649c78f40e1a4");
        }

        private static void Case_01217()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1217,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,4,30,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,19,100,3,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "0c95e8321ac65ea16b6360bd55455780c31f15120a816029b722103f2d11b9e7");
        }

        private static void Case_01218()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1218,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,3,98,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-16,82,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-17,50,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,1,63,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-19,19,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,10,92,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,5,60,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-14,61,13,2), new GeneratedEnemyUnit(2,-18,42,11,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "f42632687a065df65fc0878b475a174a55e884475d7c72278cfd2ef4659a6e0c");
        }

        private static void Case_01219()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1219,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-9,30,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,12,6,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,20,23,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-11,22,40,2), new GeneratedEnemyUnit(12,13,26,24,2), new GeneratedEnemyUnit(-18,-13,81,3,1), new GeneratedEnemyUnit(2,17,45,40,1), new GeneratedEnemyUnit(-1,-18,55,18,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "4b3202fb1b6a2d9ca6462b84531ad2d32581ac2eb66cde60af9c98654338f187");
        }

        private static void Case_01220()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1220,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-7,67,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,3,19,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-1,100,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-13,58,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,12,67,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,0,32,16,1), new GeneratedEnemyUnit(-7,-8,28,1,1), new GeneratedEnemyUnit(-6,-4,78,7,1), new GeneratedEnemyUnit(-7,20,61,45,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "ebaae00b013ffc7d30aab31fdd589fc51775b4d109f69ce1a27e46deebf09f49");
        }

        private static void Case_01221()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1221,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,1,14,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-13,82,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,13,92,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,7,29,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-3,36,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-5,67,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-2,6,33,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "06db325cb3cf6921f87ef67030d4a07a5337e9ec630627af6eee6ede55216ade");
        }

        private static void Case_01222()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1222,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,18,99,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-14,78,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-13,59,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,14,17,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-16,88,1,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "6b68b00e53b5949e32d4f7028d3054f067e4dcdb439476d9982312726b7edfdd");
        }

        private static void Case_01223()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1223,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,9,86,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,3,52,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,5,38,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,15,94,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-5,71,40,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "81bc81a07ff0fa121f4fe61eee99692125bd2986614702a69ece9caadb905294");
        }

        private static void Case_01224()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1224,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-11,43,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-9,97,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-8,11,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,2,39,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-10,46,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-10,31,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,2,78,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,10,97,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,12,70,40,4), new GeneratedEnemyUnit(6,-12,75,20,4), new GeneratedEnemyUnit(-18,-14,9,35,2), new GeneratedEnemyUnit(5,19,34,29,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "6ef867fe48d0a35bc6611b3988765ffd9aaf7e7d5c0e6cc3406e2be15c31e1ee");
        }

        private static void Case_01225()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1225,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,6,59,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-19,69,1,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "168b6281468cff12fa8d16334692c0c17520f0b6a3cc2494180b883ea8136db3");
        }

        private static void Case_01226()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1226,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,4,91,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-8,16,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-9,73,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,11,17,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,16,67,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,6,42,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-11,62,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-1,81,47,1), new GeneratedEnemyUnit(9,2,26,31,4), new GeneratedEnemyUnit(-12,4,11,41,1), new GeneratedEnemyUnit(-3,5,60,7,4), new GeneratedEnemyUnit(18,-2,63,17,2), new GeneratedEnemyUnit(17,8,32,40,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "662eb0489f9e8ed2ba217677e2bd694b17baddaf0a185643d7bc7f7a06addcfb");
        }

        private static void Case_01227()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1227,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,7,43,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,10,9,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,17,34,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-12,61,46,1), new GeneratedEnemyUnit(4,-3,60,28,2), new GeneratedEnemyUnit(8,12,86,29,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "abb271f495c665d1897f3577cad5003208e923dd0ae533e5ad0c606753142e7d");
        }

        private static void Case_01228()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1228,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-13,68,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-8,40,38,2), new GeneratedEnemyUnit(7,-12,19,33,1), new GeneratedEnemyUnit(10,5,19,17,1), new GeneratedEnemyUnit(-16,8,28,32,1), new GeneratedEnemyUnit(2,17,93,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "9280806e380f37a2b7de058d5b1538311683caeb3d0ea918c9e25e32c2d9bc16");
        }

        private static void Case_01229()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1229,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-5,36,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-8,62,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-12,36,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,1,24,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,19,88,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-3,7,24,1), new GeneratedEnemyUnit(14,11,36,9,1), new GeneratedEnemyUnit(-2,-3,10,43,1), new GeneratedEnemyUnit(-7,9,39,43,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "7fbf16be4f4d48996c47050cc583d157b1aa8310b0fa4f42cb6d13295f84d0be");
        }

        private static void Case_01230()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1230,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-9,25,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-2,62,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-10,100,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,4,14,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-4,55,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,14,35,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,14,35,18,2), new GeneratedEnemyUnit(16,-19,14,24,1), new GeneratedEnemyUnit(-19,-4,87,9,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "a202aa3dbfc54d7bad073ea5bc2b21469a73afe5c4d812ba92c826417203f53d");
        }

        private static void Case_01231()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1231,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,2,30,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-13,33,24,1), new GeneratedEnemyUnit(-5,7,69,24,4), new GeneratedEnemyUnit(8,11,40,20,3), new GeneratedEnemyUnit(4,-2,74,46,4), new GeneratedEnemyUnit(-20,9,24,19,1), new GeneratedEnemyUnit(-5,14,12,20,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "8a02bdff0eb84ff74685dd80ea0569934ecb3685fcfa303a0a894603f0430dbc");
        }

        private static void Case_01232()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1232,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,10,19,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-15,10,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,11,20,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-9,82,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,4,31,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,19,77,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-6,25,3,1), new GeneratedEnemyUnit(7,2,9,36,2), new GeneratedEnemyUnit(-2,-15,21,18,1), new GeneratedEnemyUnit(-17,-8,50,38,2), new GeneratedEnemyUnit(12,-13,56,29,4), new GeneratedEnemyUnit(-3,13,19,16,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "aa3b86bf68d82a7ed62c3e88c193e25aa2bceeb1c33b76900dbaff94b03dc595");
        }

        private static void Case_01233()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1233,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,8,72,3,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "103d1d99a49c890581e137164e86629bf249770dc58603bdf6d215df5d2fff7f");
        }

        private static void Case_01234()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1234,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-12,45,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,7,88,3,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "ade9d00591547fc9bc5e83f2f1e3594b45a67f656ebeac8a7a724b6cdb0b8af3");
        }

        private static void Case_01235()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1235,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,16,21,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-3,49,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,1,36,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-3,31,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-4,53,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-4,84,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-8,62,40,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "840e95229c76d850dafe36773941f84456248fa57b850d5667d8516aec4c8736");
        }

        private static void Case_01236()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1236,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,0,13,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-7,7,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-15,58,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-18,92,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-6,65,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,14,52,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,16,10,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,4,63,7,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "60c0b91917d163ae67f2f6b3d084ddadce097dfbb0bdcdf81c76cbf663f49b16");
        }

        private static void Case_01237()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1237,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-13,9,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-9,98,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-1,52,44,4), new GeneratedEnemyUnit(-6,7,26,18,1), new GeneratedEnemyUnit(-9,-8,42,26,4), new GeneratedEnemyUnit(2,19,48,14,3), new GeneratedEnemyUnit(4,14,28,28,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "1665b57982887483c8a26d65dfc2d2a6def1750fd8887260a158e5d5a42808bf");
        }

        private static void Case_01238()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1238,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-2,79,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,12,89,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-9,56,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-1,35,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,9,71,6,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "2a0957ad23a58a090b14b38734439e5fc2d8f914fb371d4f294355aeaa6bf714");
        }

        private static void Case_01239()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1239,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,3,47,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-9,56,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,0,76,1,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "5478b45d71b03093ef298b08a4fa44abf92e427756b14d6e598fd61f1090bb69");
        }

        private static void Case_01240()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1240,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-8,45,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,11,60,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-8,67,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-3,48,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,19,93,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,8,46,36,1), new GeneratedEnemyUnit(20,-9,93,40,4), new GeneratedEnemyUnit(16,-6,14,46,4), new GeneratedEnemyUnit(16,6,61,48,4), new GeneratedEnemyUnit(-12,17,20,15,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "524e5f5c89e2f5b753ef560a139cbf1b6f0474bf7620c48a1211378cc92bfcc8");
        }

        private static void Case_01241()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1241,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,7,37,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-19,48,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-2,83,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-14,20,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-20,36,49,3), new GeneratedEnemyUnit(-13,-14,31,39,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "e2bee7400cc537e368fb71f2f63c62224b98bff52cc15b9a868d3e6248e4bb7a");
        }

        private static void Case_01242()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1242,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-15,80,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-14,97,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-9,32,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,7,33,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-5,8,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-6,44,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-5,19,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,14,67,23,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "a4c2bfff4f8f34e190eb8385b27e2c50fed33262feb9b5af8450842c104a2bdb");
        }

        private static void Case_01243()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1243,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-1,36,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,6,72,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,17,38,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,6,59,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,13,30,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-9,26,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,14,13,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-12,35,4,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "fcbd9c327400d4ffb4ac17726eff0e221d598edb56ca237e1d892460a6bacc62");
        }

        private static void Case_01244()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1244,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-18,78,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,8,7,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,12,72,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,6,44,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,13,54,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-3,52,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,13,82,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-8,23,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,15,8,17,1), new GeneratedEnemyUnit(7,-6,88,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "559051aa0038141b4de4e2187f5528dd1d97e90f5b7ea9b44abb9b4d29576e2c");
        }

        private static void Case_01245()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1245,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-8,24,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,4,44,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,17,62,1,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "2729b3f16cc0592807e4a1e4f4079f216fa337425e57208582efd03b02c1a79f");
        }

        private static void Case_01246()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1246,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,10,82,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-5,34,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,3,90,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-4,28,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,3,12,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-16,64,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-17,23,12,4), new GeneratedEnemyUnit(-9,-4,67,39,1), new GeneratedEnemyUnit(18,-6,78,4,2), new GeneratedEnemyUnit(9,-7,11,34,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "952fd3222f1af67206b88330173090aa61cb10d966bce86177a2a23b6ecfedef");
        }

        private static void Case_01247()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1247,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,17,39,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-16,43,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-13,42,2,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "4151c8aa2470eb99949eca8fddeff4012af421a639395151e95cc448cb524b9c");
        }

        private static void Case_01248()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1248,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,1,42,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-16,20,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-18,21,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,18,20,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,9,90,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-3,61,3,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "17a1257ca0a77165ea6463bb72e415461dac7ffc85d357fb80a2195fe4f04144");
        }

        private static void Case_01249()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1249,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,5,78,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-20,73,7,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "d6d9b6765b89ab801d96d3795c5de4d918af043e0ac681cbeeacf94e145c03b8");
        }

        private static void Case_01250()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1250,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,18,13,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-14,61,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,14,85,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-15,73,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,2,42,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,3,77,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,12,83,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-8,73,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,5,97,46,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "22391ef708358543c43ca1db14d2831393b79339526831ab742abfc2940c3c23");
        }

        private static void Case_01251()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1251,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,5,40,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,16,60,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,11,87,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,17,59,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-8,89,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-17,10,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,12,48,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-15,68,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,16,10,45,3), new GeneratedEnemyUnit(-10,-3,45,8,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "543b9b87ec08c197a5204bbd9cd64fa4ec5a6596bfac8c77d6e3cdbf31c72e4f");
        }

        private static void Case_01252()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1252,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-5,84,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-18,41,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-6,10,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,11,9,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-20,99,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,12,79,2,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "e20f2a5b7813838f1bfa87096b04833e5a4cf761efec1a9f22b4a8311e00b251");
        }

        private static void Case_01253()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1253,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-2,74,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,9,10,24,3), new GeneratedEnemyUnit(-4,19,94,11,4), new GeneratedEnemyUnit(-6,14,31,26,4), new GeneratedEnemyUnit(-8,-6,87,43,2), new GeneratedEnemyUnit(-18,8,7,28,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "1ae5442ebedd2196452b45d0fce4873ec43de2f1b0564ed767681141b6f69af2");
        }

        private static void Case_01254()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1254,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-15,94,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-5,51,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,13,36,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-6,33,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,16,57,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,0,61,38,2), new GeneratedEnemyUnit(-19,-2,23,1,4), new GeneratedEnemyUnit(20,15,70,49,2), new GeneratedEnemyUnit(7,-16,72,3,4), new GeneratedEnemyUnit(7,-4,48,45,4), new GeneratedEnemyUnit(-1,0,74,42,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "0ae5cc048210e805eba211aac26220acc86b94e45adc3c140824b59e5c916034");
        }

        private static void Case_01255()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1255,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,11,54,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,2,65,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,11,84,24,1), new GeneratedEnemyUnit(15,-18,13,38,2), new GeneratedEnemyUnit(13,10,62,48,1), new GeneratedEnemyUnit(-6,18,62,19,3), new GeneratedEnemyUnit(7,10,37,43,2), new GeneratedEnemyUnit(-16,5,90,7,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "b06a5d686b40f67b1cf10ccddbb25fade62c94ff38a99d2459e24e59022419b2");
        }

        private static void Case_01256()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1256,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,15,35,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,9,52,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,18,16,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-9,98,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-10,84,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-20,62,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-6,49,49,4), new GeneratedEnemyUnit(-2,-17,8,39,4), new GeneratedEnemyUnit(0,-10,30,42,2), new GeneratedEnemyUnit(18,12,81,38,2), new GeneratedEnemyUnit(13,-5,14,23,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "577b290194c699ffc384e100ee3d19b08c4b50538a89020a837ab541b0802668");
        }

        private static void Case_01257()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1257,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,9,98,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-4,86,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,8,89,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,0,91,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-13,100,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,17,68,39,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "9cec1941524741f28dfa25c979161074286bb81bd987b19cd50ba15960f3921d");
        }

        private static void Case_01258()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1258,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-17,13,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-5,10,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-1,9,35,3), new GeneratedEnemyUnit(6,9,71,16,4), new GeneratedEnemyUnit(-3,8,92,9,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "67610743504161058e906a15be2c5dc9575d31117b987070dac583ea98f56b4d");
        }

        private static void Case_01259()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1259,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,3,31,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-3,55,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,15,32,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,19,96,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,17,91,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-8,93,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-6,84,48,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "80de875df27894b563e524f7afc8231109b66607b21508cfd1bceff6d5c482dc");
        }

    }
}
