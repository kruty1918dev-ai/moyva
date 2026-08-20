using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard030
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_05400();
            Case_05401();
            Case_05402();
            Case_05403();
            Case_05404();
            Case_05405();
            Case_05406();
            Case_05407();
            Case_05408();
            Case_05409();
            Case_05410();
            Case_05411();
            Case_05412();
            Case_05413();
            Case_05414();
            Case_05415();
            Case_05416();
            Case_05417();
            Case_05418();
            Case_05419();
            Case_05420();
            Case_05421();
            Case_05422();
            Case_05423();
            Case_05424();
            Case_05425();
            Case_05426();
            Case_05427();
            Case_05428();
            Case_05429();
            Case_05430();
            Case_05431();
            Case_05432();
            Case_05433();
            Case_05434();
            Case_05435();
            Case_05436();
            Case_05437();
            Case_05438();
            Case_05439();
            Case_05440();
            Case_05441();
            Case_05442();
            Case_05443();
            Case_05444();
            Case_05445();
            Case_05446();
            Case_05447();
            Case_05448();
            Case_05449();
            Case_05450();
            Case_05451();
            Case_05452();
            Case_05453();
            Case_05454();
            Case_05455();
            Case_05456();
            Case_05457();
            Case_05458();
            Case_05459();
            Case_05460();
            Case_05461();
            Case_05462();
            Case_05463();
            Case_05464();
            Case_05465();
            Case_05466();
            Case_05467();
            Case_05468();
            Case_05469();
            Case_05470();
            Case_05471();
            Case_05472();
            Case_05473();
            Case_05474();
            Case_05475();
            Case_05476();
            Case_05477();
            Case_05478();
            Case_05479();
            Case_05480();
            Case_05481();
            Case_05482();
            Case_05483();
            Case_05484();
            Case_05485();
            Case_05486();
            Case_05487();
            Case_05488();
            Case_05489();
            Case_05490();
            Case_05491();
            Case_05492();
            Case_05493();
            Case_05494();
            Case_05495();
            Case_05496();
            Case_05497();
            Case_05498();
            Case_05499();
            Case_05500();
            Case_05501();
            Case_05502();
            Case_05503();
            Case_05504();
            Case_05505();
            Case_05506();
            Case_05507();
            Case_05508();
            Case_05509();
            Case_05510();
            Case_05511();
            Case_05512();
            Case_05513();
            Case_05514();
            Case_05515();
            Case_05516();
            Case_05517();
            Case_05518();
            Case_05519();
            Case_05520();
            Case_05521();
            Case_05522();
            Case_05523();
            Case_05524();
            Case_05525();
            Case_05526();
            Case_05527();
            Case_05528();
            Case_05529();
            Case_05530();
            Case_05531();
            Case_05532();
            Case_05533();
            Case_05534();
            Case_05535();
            Case_05536();
            Case_05537();
            Case_05538();
            Case_05539();
            Case_05540();
            Case_05541();
            Case_05542();
            Case_05543();
            Case_05544();
            Case_05545();
            Case_05546();
            Case_05547();
            Case_05548();
            Case_05549();
            Case_05550();
            Case_05551();
            Case_05552();
            Case_05553();
            Case_05554();
            Case_05555();
            Case_05556();
            Case_05557();
            Case_05558();
            Case_05559();
            Case_05560();
            Case_05561();
            Case_05562();
            Case_05563();
            Case_05564();
            Case_05565();
            Case_05566();
            Case_05567();
            Case_05568();
            Case_05569();
            Case_05570();
            Case_05571();
            Case_05572();
            Case_05573();
            Case_05574();
            Case_05575();
            Case_05576();
            Case_05577();
            Case_05578();
            Case_05579();
        }

        private static void Case_05400()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5400,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-5,5,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-14,23,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-7,32,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-10,66,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,18,66,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,2,80,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,17,79,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,20,10,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-1,82,10,3), new GeneratedEnemyUnit(-14,3,43,8,3), new GeneratedEnemyUnit(18,-6,26,9,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "4e280d2054a42116c6dff138337a2c564b033cea6fdc2c9db3dd9eb9692e2ccd");
        }

        private static void Case_05401()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5401,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-3,54,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-11,50,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,6,89,38,4), new GeneratedEnemyUnit(7,19,55,22,1), new GeneratedEnemyUnit(13,-9,21,13,1), new GeneratedEnemyUnit(9,-10,86,17,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "eb14d4c008b4f86697fe2376bd20ef1b064b51b598824262baab59da4221fb9d");
        }

        private static void Case_05402()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5402,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-5,13,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,5,72,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-11,56,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "34aa50792161f1c2b28212f6a8e323747e50095fb12f04834d29190c1e7fcdd7");
        }

        private static void Case_05403()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5403,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-6,39,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-18,92,7,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "d198b8375de64a30a6d397d7a366faf1883c0ae086f1cb0bd04fa5b33b19a3c8");
        }

        private static void Case_05404()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5404,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,2,8,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-7,19,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,7,38,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,19,18,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,7,99,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,5,60,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-12,87,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-11,89,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "bbb864ba9631bad2eb6e0800140135b709c5d00f94f3b858c88c47807cc1cbc8");
        }

        private static void Case_05405()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5405,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,0,50,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,4,62,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-8,31,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,9,30,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-13,50,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,0,76,6,4), new GeneratedEnemyUnit(-2,-16,65,9,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "97bdb12ae54e6c4e68dfb8ca0bf3213e35207d51b9025a1ae72b62fa51843b86");
        }

        private static void Case_05406()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5406,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-12,16,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,2,11,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,8,76,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-7,45,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,10,45,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,6,44,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-17,21,21,3), new GeneratedEnemyUnit(11,16,30,26,1), new GeneratedEnemyUnit(11,19,67,6,1), new GeneratedEnemyUnit(-5,-8,41,15,1), new GeneratedEnemyUnit(-3,-9,92,27,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "b40ac2f745a7250b11485fbf5a5c84c95ab1f59581d1c7d8ee492fbbc35e2d87");
        }

        private static void Case_05407()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5407,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-18,98,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,6,94,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,17,53,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-12,99,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,6,14,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-19,67,6,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "1a581cbdebe3cd506085e0dfd65a1b932a99afaa811506335e73f9cda70a14ca");
        }

        private static void Case_05408()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5408,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-11,23,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,3,57,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,19,40,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-10,72,37,4), new GeneratedEnemyUnit(-4,-16,43,25,3), new GeneratedEnemyUnit(6,1,41,32,3), new GeneratedEnemyUnit(5,3,47,7,4), new GeneratedEnemyUnit(20,-15,22,22,1), new GeneratedEnemyUnit(13,-5,62,14,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "bbdd389f7bb25cc2e177b81dbd1101b1f89fd636524f98820797344819b71691");
        }

        private static void Case_05409()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5409,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-7,39,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,16,40,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-3,12,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,4,8,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,8,11,20,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1e5cd6973ac97c1c9b6df75bffae4324fc47343aa5639262e85d2b8e9719ce5c");
        }

        private static void Case_05410()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5410,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-3,94,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-15,73,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,10,97,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,8,98,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,17,5,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,3,77,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-12,42,3,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "6bf2f023e4a37292a6a82209a37c05dea1b8b8d3afa8a85720d2946659b8083a");
        }

        private static void Case_05411()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5411,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-15,61,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-17,61,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,18,20,25,3), new GeneratedEnemyUnit(-19,-11,84,3,4), new GeneratedEnemyUnit(6,-7,37,11,3), new GeneratedEnemyUnit(2,-17,19,43,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "0f6851fa81b6bcbcd5290f415de31774b28e88ce070ff55734b64a363017647f");
        }

        private static void Case_05412()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5412,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-14,48,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,18,67,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,19,26,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-13,58,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-4,50,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,20,70,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,2,56,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-10,33,19,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "621f10b1301b22b848f22aa02ebbb97999eb29d4731eb52a8b662e4c15c25a17");
        }

        private static void Case_05413()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5413,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,16,69,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,20,82,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,19,78,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,12,84,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,0,67,4,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "241f4de91df3f91c5901bdb66dbfa6750a8aaba741f0f503a1f2d457524ad417");
        }

        private static void Case_05414()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5414,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-11,25,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-1,29,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-7,55,3,3), new GeneratedEnemyUnit(-3,-16,75,48,4), new GeneratedEnemyUnit(-7,16,28,15,1), new GeneratedEnemyUnit(6,-9,53,16,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "5a2618df03e94d44ad40386dbd51e40437ab0a762f06c0729c5154895defed14");
        }

        private static void Case_05415()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5415,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,12,50,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-8,49,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-7,84,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,14,35,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,12,89,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,5,75,11,4), new GeneratedEnemyUnit(-6,17,90,17,1), new GeneratedEnemyUnit(7,-19,5,39,4), new GeneratedEnemyUnit(-2,12,71,31,2), new GeneratedEnemyUnit(-10,7,46,39,3), new GeneratedEnemyUnit(-8,-7,36,21,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "4d4f9b1ad1087e316babf208bb9afe1b33216232eb04864a9d7b6930b76d3241");
        }

        private static void Case_05416()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5416,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,9,49,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,19,83,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,2,65,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,19,83,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,10,6,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,11,51,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-9,75,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-8,19,41,2), new GeneratedEnemyUnit(5,0,51,39,4), new GeneratedEnemyUnit(-10,-15,14,43,4), new GeneratedEnemyUnit(19,10,54,15,1), new GeneratedEnemyUnit(18,5,96,3,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "04b2f718f38340925018d2f1e572d079f60b0285416cbd1c7daabefe5d32bb2f");
        }

        private static void Case_05417()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5417,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,13,88,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,11,50,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-14,13,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,13,69,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,14,18,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-3,17,10,1), new GeneratedEnemyUnit(-14,18,70,16,1), new GeneratedEnemyUnit(-3,-14,43,34,3), new GeneratedEnemyUnit(-14,-3,100,20,2), new GeneratedEnemyUnit(-17,-11,9,11,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "50a7aa01575ed08e2b5bb26f9a10ca60d355939f7a8eb4e7d5623dc85195c604");
        }

        private static void Case_05418()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5418,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,1,13,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-12,83,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-10,12,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-1,8,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,10,95,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,20,84,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,8,54,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,3,96,38,1), new GeneratedEnemyUnit(1,0,63,29,4), new GeneratedEnemyUnit(14,8,24,17,1), new GeneratedEnemyUnit(15,-1,50,8,3), new GeneratedEnemyUnit(3,-13,24,15,1), new GeneratedEnemyUnit(-2,-18,58,47,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "33cb0f1c1081246a4fb8e58818091bc52e265f5499669a65798ab9de27e48f0f");
        }

        private static void Case_05419()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5419,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-9,89,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-19,45,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-2,99,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-12,91,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,11,54,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-13,38,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-7,10,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-5,38,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,19,14,40,3), new GeneratedEnemyUnit(0,17,10,6,4), new GeneratedEnemyUnit(-16,10,41,17,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "5f7d8eadef3e39ae8f487b6ce48d5476f7c53fa38712524dfe0b1109aa5f6d96");
        }

        private static void Case_05420()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5420,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,4,20,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-9,40,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-15,21,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,7,20,4,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "90ed8a9d340905c171027a2028567c398221976ddf4eb34ddaa4c2ae8b9cae29");
        }

        private static void Case_05421()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5421,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-13,12,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-17,63,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,14,96,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,9,36,19,3), new GeneratedEnemyUnit(-6,14,10,19,4), new GeneratedEnemyUnit(-8,-6,64,35,3), new GeneratedEnemyUnit(-11,-14,56,43,1), new GeneratedEnemyUnit(20,7,43,16,3), new GeneratedEnemyUnit(7,1,90,31,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "29ae22684bfe6efc99115eac2db5e61a8eb0f36d0077f51fa9fb66ca0fbae4e5");
        }

        private static void Case_05422()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5422,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,0,20,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-15,22,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,12,48,41,3), new GeneratedEnemyUnit(-11,-4,57,5,3), new GeneratedEnemyUnit(-16,-4,27,10,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "9d1fc2400e11ba5654a771e48b354df92b5785009cba91cd48e307704e458658");
        }

        private static void Case_05423()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5423,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,0,29,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-4,36,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,9,36,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,19,40,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,3,82,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,6,41,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-2,16,49,2), new GeneratedEnemyUnit(-2,1,91,5,4), new GeneratedEnemyUnit(-10,14,13,38,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "66c849d686558d97b4940ac42ee0dc0d6c74c8152b2236dfd3a83fec247dd7d3");
        }

        private static void Case_05424()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5424,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,4,95,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,12,9,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-1,64,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,18,40,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,20,89,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,10,95,29,1), new GeneratedEnemyUnit(-20,-2,62,1,3), new GeneratedEnemyUnit(2,19,58,26,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "c072b012d829ca611fa64bac8016e73b28e2b5e87a4cd9e3001205d1ed2f377b");
        }

        private static void Case_05425()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5425,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-9,43,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,17,86,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-5,48,44,1), new GeneratedEnemyUnit(13,5,9,44,2), new GeneratedEnemyUnit(-2,2,79,42,1), new GeneratedEnemyUnit(-5,-3,21,33,4), new GeneratedEnemyUnit(-20,-9,45,24,3), new GeneratedEnemyUnit(-3,0,81,8,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "a60903e7f7345b8a74b581dcaded9ab3c3f401829049eea0eb765b63d905efc3");
        }

        private static void Case_05426()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5426,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,6,28,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-20,22,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-12,71,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-10,96,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,12,54,38,1), new GeneratedEnemyUnit(-14,-8,89,15,2), new GeneratedEnemyUnit(-20,3,68,36,4), new GeneratedEnemyUnit(20,-9,51,40,3), new GeneratedEnemyUnit(1,1,56,3,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "2e2efccae1cde1ef10e3275162ddb1337477038463f00c0e36352455350baf3e");
        }

        private static void Case_05427()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5427,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-15,42,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-4,94,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,7,67,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,17,42,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-18,98,46,4), new GeneratedEnemyUnit(-5,11,34,43,1), new GeneratedEnemyUnit(-3,4,24,25,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "0f2b3827874aadaf682891b268a6c9405679484b85b2d380d66ec058253a2546");
        }

        private static void Case_05428()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5428,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,17,41,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-12,7,19,2), new GeneratedEnemyUnit(2,16,74,13,3), new GeneratedEnemyUnit(9,16,31,36,3), new GeneratedEnemyUnit(-17,7,62,8,3), new GeneratedEnemyUnit(18,7,54,16,3), new GeneratedEnemyUnit(-9,16,16,39,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "b6480a6de14bb2e19db77cde97d137209cd850c5df7be08a434cd88f48c158eb");
        }

        private static void Case_05429()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5429,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-16,57,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,4,41,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-13,71,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,20,41,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,18,82,4,3), new GeneratedEnemyUnit(10,-7,98,36,2), new GeneratedEnemyUnit(8,-12,30,11,4), new GeneratedEnemyUnit(-19,-12,79,27,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "06582119089a649fa938f081c1d262505bf93761b44a719835c7a87c991f88c4");
        }

        private static void Case_05430()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5430,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,12,94,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,6,99,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-18,5,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,11,44,22,3), new GeneratedEnemyUnit(12,19,39,21,3), new GeneratedEnemyUnit(-18,18,26,29,1), new GeneratedEnemyUnit(0,-18,48,29,4), new GeneratedEnemyUnit(17,-20,8,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "aee7b8656f930067bc9519dc2bbb9e47be1daa3500a4581248ea0f05c7125823");
        }

        private static void Case_05431()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5431,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,7,32,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-1,6,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-19,95,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,3,33,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,2,64,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-18,78,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-20,75,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-4,91,12,4), new GeneratedEnemyUnit(-20,14,84,20,2), new GeneratedEnemyUnit(15,18,43,15,4), new GeneratedEnemyUnit(-6,20,98,11,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "79f3f7905cc05f3979aa295b1a67e7e376d051a7deabd40a8b7749a9a622555c");
        }

        private static void Case_05432()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5432,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,9,51,2,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "28728aabae835d7478344687d01ee72328a2e7d7696e0b1ae4cacd165a69d1ed");
        }

        private static void Case_05433()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5433,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-18,77,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-17,93,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,19,87,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,14,95,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,15,42,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,13,30,5,1), new GeneratedEnemyUnit(8,10,56,37,4), new GeneratedEnemyUnit(20,-6,41,36,2), new GeneratedEnemyUnit(0,-1,80,26,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "c58045c5e45c4a786d05b27b824db2e0fb7b2c148ad8cb4686675650ca935d7e");
        }

        private static void Case_05434()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5434,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-4,23,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-11,20,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,17,36,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-9,60,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-19,68,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,4,16,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,18,42,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-13,16,41,4), new GeneratedEnemyUnit(11,2,30,11,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "2963f6a285f7dae5a0ff79ef7ae4f3d66abad49e8ac6a3fb832b75bd2eec8234");
        }

        private static void Case_05435()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5435,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,16,10,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,5,94,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,7,61,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,15,18,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,11,90,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-6,51,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-12,73,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,7,8,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,4,33,31,1), new GeneratedEnemyUnit(7,-9,51,35,1), new GeneratedEnemyUnit(-13,-2,40,23,1), new GeneratedEnemyUnit(3,-2,100,39,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "d31a0d823ce3fcc16159a7d8cbcdd224c8ae69caf1990dad2c28e564145f30db");
        }

        private static void Case_05436()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5436,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,5,21,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-9,63,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-18,41,6,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6b86c5f380691d2d2cc8d9aa0e5a277802309fae6c848229082efabc94542965");
        }

        private static void Case_05437()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5437,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,10,84,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-13,68,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-3,37,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,11,21,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-10,61,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-6,69,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-12,15,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-1,51,14,2), new GeneratedEnemyUnit(-13,-8,82,50,4), new GeneratedEnemyUnit(10,7,29,23,3), new GeneratedEnemyUnit(-8,-10,31,48,4), new GeneratedEnemyUnit(-16,-20,40,48,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "005c3b2ddf95c0ee17000aa3e12624ee699c07feea3311c0c7cd8cd6fdb7ddbc");
        }

        private static void Case_05438()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5438,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,1,54,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,17,67,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-7,33,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,20,78,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,5,13,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-8,64,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-6,21,26,4), new GeneratedEnemyUnit(3,11,74,36,2), new GeneratedEnemyUnit(18,0,61,1,2), new GeneratedEnemyUnit(20,-3,86,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "b6d760561b1fcb43508e602c9c04ecac118d6ea4199fdfe883d056f46ed7c451");
        }

        private static void Case_05439()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5439,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,13,23,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-19,77,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,16,42,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,14,28,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-7,32,41,2), new GeneratedEnemyUnit(0,-8,8,3,4), new GeneratedEnemyUnit(-9,8,13,44,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 85,
                stableHash: "fe8f411591ade808ecd28ff24db572d14750a56688730506bed6e6c4bb71a331");
        }

        private static void Case_05440()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5440,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,1,12,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-1,95,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,4,36,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-18,64,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,17,23,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-8,86,42,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "2d5e5499247cb72e29b727bef034b0d3bb909e7ce8c21ecc9b0c261367069384");
        }

        private static void Case_05441()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5441,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-5,46,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-16,23,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,17,39,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-7,42,11,2), new GeneratedEnemyUnit(6,9,5,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "20b3b59707e2e9b266984a68af9679e369b78fc3adfa15041b895df2e189b9fa");
        }

        private static void Case_05442()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5442,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-12,65,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-17,25,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,7,5,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-1,38,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-10,32,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-7,98,45,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "861a746f55a57467321c1a25fe1a112062d2fdce43933732c9d8dbc12c97be82");
        }

        private static void Case_05443()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5443,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,17,29,2,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "b46883ae8dc912ec7cc037bd5dc7282e356ff5841b94eed7afd8e1f0c6b36d88");
        }

        private static void Case_05444()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5444,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,2,89,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,7,28,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,4,91,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,18,75,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,20,52,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-20,43,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-5,71,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-9,90,12,1), new GeneratedEnemyUnit(17,-2,79,29,1), new GeneratedEnemyUnit(3,0,60,15,1), new GeneratedEnemyUnit(5,-2,88,25,2), new GeneratedEnemyUnit(20,-15,47,36,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "cd4d541da9092b2e4ccbc40b25396fe72418d82637acb43c152a360fbefee946");
        }

        private static void Case_05445()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5445,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,14,8,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,3,21,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,15,40,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,1,84,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,20,6,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,12,43,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,6,34,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-10,50,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,5,51,3,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "1399a331674805d0b2d2a209051acfc887df52701f4e202e8448db71fc10dcba");
        }

        private static void Case_05446()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5446,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-19,85,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,15,67,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,13,68,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,20,36,32,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "e9cf283b57a6d611d2d1d29dc2fd9652d47807b33cd8dafbd1d74019ad55dfa5");
        }

        private static void Case_05447()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5447,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-15,28,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-1,78,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-9,50,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,0,92,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,3,8,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-13,38,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,12,78,25,3), new GeneratedEnemyUnit(16,4,9,46,4), new GeneratedEnemyUnit(2,-19,44,23,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "cedb198686833b55f378a83bd3a125f2d449e2eca43391531803346e59a01621");
        }

        private static void Case_05448()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5448,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-17,88,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,11,34,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,10,30,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,12,90,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,14,87,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,16,36,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,0,60,50,4), new GeneratedEnemyUnit(10,-20,61,10,4), new GeneratedEnemyUnit(4,-19,63,1,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e421607bc05ce72a76ba27d1f8c6e641ad698b48bf8feec382df6c193f6191d1");
        }

        private static void Case_05449()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5449,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,16,97,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-11,5,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-6,82,15,1), new GeneratedEnemyUnit(7,7,61,43,2), new GeneratedEnemyUnit(4,-8,83,39,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "2a5bfcb3fd2833b232d90c70b4eca73a1d6396f5b0f8353e2c418a575cb27c67");
        }

        private static void Case_05450()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5450,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,6,39,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-10,24,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-18,30,11,1), new GeneratedEnemyUnit(-12,13,85,1,3), new GeneratedEnemyUnit(-11,-17,95,39,2), new GeneratedEnemyUnit(16,5,36,3,2), new GeneratedEnemyUnit(5,0,97,26,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "47bae0959756b6771a09e08c0d0084eea84d69765c1fe1a144cf4354dde6dc8d");
        }

        private static void Case_05451()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5451,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,17,75,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,18,13,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,11,27,10,2), new GeneratedEnemyUnit(-10,-10,84,34,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "37abb1281485b0e8e6f0a7631a3aa951f4decfa835f1f64e02e1c88e6e1e26f6");
        }

        private static void Case_05452()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5452,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-12,86,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,12,98,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,20,10,33,3), new GeneratedEnemyUnit(-10,-2,9,27,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "4b8830fa47dec055b0158b028736ca23ccfd7e61034a237b7ed22c72f4851a08");
        }

        private static void Case_05453()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5453,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,0,65,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-2,17,31,4), new GeneratedEnemyUnit(-20,-14,97,3,2), new GeneratedEnemyUnit(0,-11,20,45,3), new GeneratedEnemyUnit(18,18,76,43,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "5d13d84c7dce76454a915cd25a2357a402bf9ce746c60b53a383803c8e86884f");
        }

        private static void Case_05454()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5454,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-6,52,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,1,98,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,17,63,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,12,39,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-4,27,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,2,47,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,20,25,5,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "55b3b55ba967757aa075a24722ad1d4d62666cc329ca737b596e79cd90689051");
        }

        private static void Case_05455()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5455,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-18,9,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,20,40,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-19,58,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,6,63,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-17,88,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-19,21,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,16,13,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,11,56,2,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "e7f1184ed73698b72ef0b2c4ffc2be79e40ebada490331f6a87ea1beb7bc7a5c");
        }

        private static void Case_05456()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5456,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-2,45,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,11,97,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-4,14,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-14,99,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-2,77,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-5,89,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,2,19,35,4), new GeneratedEnemyUnit(20,-9,39,45,3), new GeneratedEnemyUnit(-11,-7,13,9,2), new GeneratedEnemyUnit(-3,-14,60,33,2), new GeneratedEnemyUnit(19,-5,7,49,1), new GeneratedEnemyUnit(6,18,13,23,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "7984dc8b559a106c81cefe9fba3559c8506b76a021e53fd3fe274ad4d858d60f");
        }

        private static void Case_05457()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5457,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,6,59,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,20,76,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,1,58,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,1,60,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-1,75,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,7,66,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-14,66,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,20,53,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,13,85,31,1), new GeneratedEnemyUnit(-16,-17,89,47,1), new GeneratedEnemyUnit(2,6,53,9,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "b6ebe31710657f5bbb6b638fb6904a75a1198f51c92675dd8996ba20b59303f9");
        }

        private static void Case_05458()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5458,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,18,89,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,5,65,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,16,40,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-15,22,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,4,52,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,18,79,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-5,100,39,3), new GeneratedEnemyUnit(7,-8,83,15,4), new GeneratedEnemyUnit(2,3,76,7,3), new GeneratedEnemyUnit(7,6,34,8,4), new GeneratedEnemyUnit(-1,-8,44,38,1), new GeneratedEnemyUnit(12,-14,71,16,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "43e3f9f25b5d6652ab2dbda2c4dad51f12b581b1027e97be28d92c3082cc6aaa");
        }

        private static void Case_05459()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5459,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,2,67,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-6,55,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,15,73,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,16,80,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-12,28,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-20,5,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-1,37,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-16,99,7,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "855dab911b7a4c65c007b335e5dbf74ff22177932258d5473e2db5cd3f0ad178");
        }

        private static void Case_05460()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5460,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-15,76,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,5,71,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,14,45,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,5,8,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,6,64,6,1), new GeneratedEnemyUnit(2,0,75,14,3), new GeneratedEnemyUnit(14,13,13,16,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "a5c0d4e53829ae9550f123f1cb5f70536eae42fe37144fd73783e798bf1f2521");
        }

        private static void Case_05461()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5461,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,9,97,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-10,91,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,18,73,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-16,56,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-7,69,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-3,58,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,16,80,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-3,55,37,2), new GeneratedEnemyUnit(17,12,22,6,3), new GeneratedEnemyUnit(-4,12,20,6,1), new GeneratedEnemyUnit(-4,18,18,34,3), new GeneratedEnemyUnit(11,19,51,39,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "b7d04e89fc01eb7925ed8a6acbc7eeb7956c027d2b1f515827f94ca2484eb596");
        }

        private static void Case_05462()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5462,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,1,73,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,2,84,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-6,25,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-17,7,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-8,80,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-2,40,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-7,17,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-15,99,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,10,33,35,1), new GeneratedEnemyUnit(-6,7,90,47,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "867de3917397325f3dd600236a79f83b2a4d4cd596bf5778189223bf94060c13");
        }

        private static void Case_05463()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5463,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-11,56,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-6,69,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-20,9,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-16,23,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-20,18,48,1), new GeneratedEnemyUnit(-8,-8,28,2,1), new GeneratedEnemyUnit(20,-8,46,25,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "14f4c3e6f6dd7a0fc91f829791c4d2c6cc7a43311098f78cc43161c5fb3b8e4e");
        }

        private static void Case_05464()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5464,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,7,31,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-7,41,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,5,55,40,3), new GeneratedEnemyUnit(0,-5,32,38,2), new GeneratedEnemyUnit(13,-10,86,3,2), new GeneratedEnemyUnit(16,-13,15,31,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "458ecca8dadf39d794ec5c7087e6beaa6e49fa17d8b2bbdbef302aded7138615");
        }

        private static void Case_05465()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5465,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-5,53,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,4,63,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-17,94,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-2,70,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,12,70,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,20,25,24,2), new GeneratedEnemyUnit(-20,-2,25,2,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2d76dcaf2a6dadba2deb2dc42feea1922369b49ffb60fa1bd1261dac116ef02c");
        }

        private static void Case_05466()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5466,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-8,53,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-15,24,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,18,98,3,1), new GeneratedEnemyUnit(3,-15,26,37,4), new GeneratedEnemyUnit(-13,17,88,4,1), new GeneratedEnemyUnit(-1,-1,28,15,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "41231071b20ffd5289fd01890ddb707943633df15fb86a7842a2968eee33b52e");
        }

        private static void Case_05467()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5467,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-2,17,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,14,32,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,0,39,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-4,57,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-8,60,9,4), new GeneratedEnemyUnit(-3,-4,48,22,1), new GeneratedEnemyUnit(-7,-20,86,43,2), new GeneratedEnemyUnit(18,-11,93,40,2), new GeneratedEnemyUnit(-6,-12,57,8,1), new GeneratedEnemyUnit(17,-7,38,18,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e6338f1f4ac52b45340fb5d8f551598930d713bb9de5556f698bbd4ff81dd74c");
        }

        private static void Case_05468()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5468,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-18,28,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-20,47,33,1), new GeneratedEnemyUnit(-11,-20,34,44,3), new GeneratedEnemyUnit(-12,8,23,46,4), new GeneratedEnemyUnit(12,10,99,11,1), new GeneratedEnemyUnit(1,2,64,47,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "e0c0d84330881e2d6c8f181d96391b22e36feb1487dea175685f49e5b3c0aba1");
        }

        private static void Case_05469()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5469,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,14,49,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,18,27,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-12,91,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-18,26,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-14,65,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,15,37,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-1,57,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,17,54,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,8,97,27,2), new GeneratedEnemyUnit(-6,-18,60,1,1), new GeneratedEnemyUnit(5,9,28,29,1), new GeneratedEnemyUnit(11,-4,49,41,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "796ba19cca2c8ca8af68a9ecaccfd16a78226563a2ae90e085049bff5c581a00");
        }

        private static void Case_05470()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5470,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-18,45,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-18,56,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,14,8,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,7,70,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,13,54,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-17,54,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-18,61,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-1,21,50,3), new GeneratedEnemyUnit(-9,14,95,11,2), new GeneratedEnemyUnit(8,2,53,3,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "fce30165eb7f03ad44ecbbfd47a19350583799d51cd54b825a16925ab8d47a0c");
        }

        private static void Case_05471()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5471,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-6,33,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,8,9,19,4), new GeneratedEnemyUnit(-15,7,81,17,4), new GeneratedEnemyUnit(5,14,19,6,3), new GeneratedEnemyUnit(15,0,82,22,2), new GeneratedEnemyUnit(-19,7,10,37,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "b6cbef2223de3b82de4f8ffd94ecbb16fd5ae9e3fd51d3a18b46ad6c566c8f50");
        }

        private static void Case_05472()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5472,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,17,16,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,20,26,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-16,46,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,20,33,8,2), new GeneratedEnemyUnit(7,0,27,33,3), new GeneratedEnemyUnit(-3,11,25,25,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 85,
                stableHash: "1f96afea074ddfc22378872fd09bff697adb3b77d4b8495b3766d390a5c745fa");
        }

        private static void Case_05473()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5473,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,8,75,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-12,87,18,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "84ab1512f13839304e6b741745f96d3316e71e73354d9ad3c467811e2391bb47");
        }

        private static void Case_05474()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5474,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,7,42,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-8,62,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,4,29,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-13,14,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,10,73,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-14,74,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,0,38,5,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "74108a597928ccf429174f282408aaf7444654b4f748aa6e250a6197104c28ca");
        }

        private static void Case_05475()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5475,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,8,34,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-8,38,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,20,54,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,7,64,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,2,18,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,3,21,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-8,90,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,18,54,9,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "d4e9c8e77e6d6a4aa581ffe491fb88e80fb83d7dadeb238f0dacee6b6c1bb2de");
        }

        private static void Case_05476()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5476,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-4,36,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,16,34,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,19,79,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-8,47,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,4,48,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-9,20,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,10,78,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,10,81,1,1), new GeneratedEnemyUnit(-7,-13,13,37,4), new GeneratedEnemyUnit(-17,20,32,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "6487a759080b92e0aefdad1c34a6e773b3272afa2d206259f2ac157207c76a01");
        }

        private static void Case_05477()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5477,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-5,19,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-11,76,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-14,43,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,0,37,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,16,89,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-5,64,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-13,78,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,5,94,47,4), new GeneratedEnemyUnit(-8,-5,70,1,1), new GeneratedEnemyUnit(-18,17,35,24,3), new GeneratedEnemyUnit(18,-5,51,8,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "63e5ddac029aae8733508c19531152f4f43a2b9c0f46edf19f00d3986c8bd511");
        }

        private static void Case_05478()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5478,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,20,99,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,4,82,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-17,71,35,3), new GeneratedEnemyUnit(17,-7,94,8,2), new GeneratedEnemyUnit(8,-3,86,10,1), new GeneratedEnemyUnit(-9,-5,84,13,2), new GeneratedEnemyUnit(10,5,53,46,2), new GeneratedEnemyUnit(-14,8,86,24,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "ee94efba153fdef6d1e9abfc2645d27c2c319a21f0c1b82354be226ab1d6bb41");
        }

        private static void Case_05479()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5479,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,9,37,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,17,55,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,3,55,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,14,18,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,8,95,33,3), new GeneratedEnemyUnit(-6,-3,31,41,4), new GeneratedEnemyUnit(3,-4,39,19,3), new GeneratedEnemyUnit(7,-16,33,19,1), new GeneratedEnemyUnit(-9,-20,76,20,2), new GeneratedEnemyUnit(-4,-17,48,27,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "bfbf75792f072cb96a8727b525ee5ea55ac1856416df67687162fe011176c3e8");
        }

        private static void Case_05480()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5480,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-20,45,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-19,33,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-10,21,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-19,97,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,2,21,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,13,72,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,15,33,15,4), new GeneratedEnemyUnit(-7,-6,21,39,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e6be4586f1cf51af8ea3b92826acd6a2816d3eca1490c0fdd195b46edf925428");
        }

        private static void Case_05481()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5481,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,19,57,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-18,19,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-15,10,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-10,78,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,17,39,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-17,14,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,4,17,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,4,13,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-4,80,50,4), new GeneratedEnemyUnit(14,-10,54,40,1), new GeneratedEnemyUnit(-2,8,21,19,1), new GeneratedEnemyUnit(13,-13,78,43,1), new GeneratedEnemyUnit(-5,-7,15,49,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "129ce174c4ed552853a3274e31e65d3bd327c47c0b22b876a61e4b4928cf218c");
        }

        private static void Case_05482()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5482,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-14,59,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-8,8,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-15,63,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-7,26,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-9,9,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,11,13,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-13,51,7,2), new GeneratedEnemyUnit(-16,18,45,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "2d89a6c6eccc07fc8782cd016f7369422f194c5664d947380a804a05c1db088c");
        }

        private static void Case_05483()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5483,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,11,55,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-14,86,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,9,61,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,17,31,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-8,71,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-11,26,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-4,92,39,1), new GeneratedEnemyUnit(4,-19,83,7,1), new GeneratedEnemyUnit(11,9,58,21,1), new GeneratedEnemyUnit(2,6,76,2,4), new GeneratedEnemyUnit(19,9,92,23,1), new GeneratedEnemyUnit(8,20,84,28,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "07b3ad92e9c0b3944ca193c98126000f3093fe849e589a6f343bbf1930e7f6e6");
        }

        private static void Case_05484()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5484,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,10,80,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,20,15,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-4,67,19,1), new GeneratedEnemyUnit(-18,20,23,10,4), new GeneratedEnemyUnit(-13,3,66,41,4), new GeneratedEnemyUnit(-18,-17,95,46,4), new GeneratedEnemyUnit(-10,-11,48,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d69baf9bdd3824a9ff0ab96ca4a3b62f14df37e26c9b26e1ee3bc9ab30c548ab");
        }

        private static void Case_05485()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5485,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-5,24,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-15,67,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,17,88,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-17,79,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-9,70,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,7,91,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,6,81,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-4,89,4,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "65559f22d6c9e197275a3d38a5392f357aaed549e3e7849820c7b01fb9aad24a");
        }

        private static void Case_05486()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5486,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,2,58,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,20,98,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-19,53,48,2), new GeneratedEnemyUnit(12,18,8,32,2), new GeneratedEnemyUnit(-20,-7,54,12,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2801050b59ca20f6dbec8ff6752720f4933e1c86e5d3880960e42d8d706770cb");
        }

        private static void Case_05487()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5487,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,16,58,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,1,14,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-8,18,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,1,96,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,2,21,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-19,48,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,16,31,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-19,52,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-8,84,31,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "6fd12607c9a8f076d97174b87c7c7ef630ba2a0186f68e14522ddef08ba43819");
        }

        private static void Case_05488()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5488,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-3,18,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,1,42,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-13,88,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,6,67,5,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "9100d406201fb5f1f21b3d24039f5e4e07c38832c5499afede764a9b5f036592");
        }

        private static void Case_05489()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5489,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-16,47,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-9,22,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-19,9,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-19,49,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,19,39,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,1,56,6,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "f7bb3e83b5b105fc59f7a519d9773351b5e40fad0344302257e1576f0e3a5afd");
        }

        private static void Case_05490()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5490,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-2,71,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,14,8,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,3,36,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,13,28,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-13,46,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,0,61,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-18,79,50,1), new GeneratedEnemyUnit(0,16,49,38,1), new GeneratedEnemyUnit(0,20,52,14,1), new GeneratedEnemyUnit(-18,6,39,10,3), new GeneratedEnemyUnit(12,10,18,33,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "3b4c2d7174962337f012eb7f279ce36715d18e05fe220d32d39aa5e86a4add47");
        }

        private static void Case_05491()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5491,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-19,100,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "73d416ea22c4ba5001bcd306ccf1f09887c058ff3d8ae531b3f87ec77145bf94");
        }

        private static void Case_05492()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5492,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,6,30,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-2,31,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-4,53,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,7,40,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,11,37,44,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "1b65db129a61564c9c8db5cfd31341ea7af72db5f0272cb8d7deb032c86286ec");
        }

        private static void Case_05493()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5493,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,18,10,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-1,21,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-17,63,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-16,86,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-3,33,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,20,67,21,3), new GeneratedEnemyUnit(-2,1,51,30,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ae3a8dc5c77c2f2ff83cea3a310197f0556c12347b95e51be8e89a6725ab2b9f");
        }

        private static void Case_05494()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5494,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-15,35,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,12,26,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,8,77,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-5,91,32,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "a9f777dfadd627d45c58c988af9f311a42605e4557de1050fa524c4a53a2bc8a");
        }

        private static void Case_05495()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5495,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,20,100,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,11,70,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,18,23,27,2), new GeneratedEnemyUnit(5,-4,43,17,3), new GeneratedEnemyUnit(14,1,83,14,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9ca2bce37c0cfc8b52642def84ff2f865692b789188117f33b105f7f2ed83df6");
        }

        private static void Case_05496()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5496,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,13,50,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-1,26,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,11,41,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-10,9,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-1,56,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,8,24,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,3,88,32,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "c487f959aed7e4be732f87b720f180a4a81201b27a5161e3a344c9ed745bd1b4");
        }

        private static void Case_05497()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5497,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-11,28,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-10,19,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,12,62,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-17,30,42,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "10041a9a87cab8a8692bd4d9f489acb84c55334a996e06b21b017fcb97dcfc20");
        }

        private static void Case_05498()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5498,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-1,98,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,8,90,12,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "b536369a42732d4949d4220285bf0f1ca1d9faa29a67f84875f5aa7c113b2b1a");
        }

        private static void Case_05499()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5499,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-13,55,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-17,12,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,14,14,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-19,22,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-20,42,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,2,41,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-1,14,31,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "349271173fb0f056f94120fadea17e6c56ed8eb1c8a5a108797ca2fd30c1cb13");
        }

        private static void Case_05500()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5500,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,10,53,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,16,17,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-13,50,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,16,16,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,13,100,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,9,47,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-17,39,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,6,18,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-11,25,9,2), new GeneratedEnemyUnit(17,10,96,19,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "8f3e0a77abb6f86a84d96bc1fd9e89db29845f1037df438bbeefb798b33d1791");
        }

        private static void Case_05501()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5501,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-6,29,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,14,11,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,13,39,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-14,47,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-11,52,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,7,79,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-1,26,2,1), new GeneratedEnemyUnit(6,-2,80,47,2), new GeneratedEnemyUnit(13,-18,95,11,4), new GeneratedEnemyUnit(20,18,86,31,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7e3eca00183f8f875c855e9e67a5f8c225a4676c336eb6ab10d03a4eaf28eea7");
        }

        private static void Case_05502()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5502,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,1,80,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,16,22,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,1,48,26,1), new GeneratedEnemyUnit(-14,-9,58,28,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ee03d72d108dddd6ce566e999dc40ce7dd33d40d694a14d6cfe11135bbfaf305");
        }

        private static void Case_05503()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5503,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,0,86,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-2,94,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,18,91,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,1,28,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,5,76,28,4), new GeneratedEnemyUnit(-13,18,31,34,2), new GeneratedEnemyUnit(-13,-11,88,46,4), new GeneratedEnemyUnit(7,-19,53,25,1), new GeneratedEnemyUnit(12,-10,91,46,3), new GeneratedEnemyUnit(2,16,28,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "428a37d9088833eab3975cf1d5d5d98d265061f73d8640dc5ecac1e88ad2df33");
        }

        private static void Case_05504()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5504,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-1,7,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,14,61,33,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "b878822878b8668fca3487b6218265800ff8a4b9418041cf1dba3ed25a660f7a");
        }

        private static void Case_05505()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5505,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-20,97,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,5,57,37,1), new GeneratedEnemyUnit(11,-18,38,31,3), new GeneratedEnemyUnit(-19,-11,75,7,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "91574ff386b2654d454049ca761d965cbac16d0f11d4c4ee2ec42b419078e67a");
        }

        private static void Case_05506()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5506,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-4,44,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,18,70,9,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "6f7675c761c4d693b604a9e6c2ac21ba37a1621e7405265854f95b19ade1d0ef");
        }

        private static void Case_05507()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5507,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,1,42,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-2,64,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,2,42,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-19,65,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-1,32,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-17,30,12,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "e53460a90004fa07dc39686822ecae69851d6f5d347070b34ddbc9e2888127af");
        }

        private static void Case_05508()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5508,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-7,67,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-20,18,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,0,41,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-16,19,5,2), new GeneratedEnemyUnit(-8,1,61,37,3), new GeneratedEnemyUnit(-2,-18,56,5,3), new GeneratedEnemyUnit(-14,-19,87,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "cc6cd34faca3a5ca15c975914fa026796a49ed86ff96348e329e5f45e61ce586");
        }

        private static void Case_05509()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5509,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-14,40,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,1,34,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,16,79,44,1), new GeneratedEnemyUnit(-11,10,40,36,4), new GeneratedEnemyUnit(-13,13,54,25,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "41f2cd7d087e1e69a8068901abab3d42a6ff6c686f4b6949780b9f183d117a78");
        }

        private static void Case_05510()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5510,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-7,11,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-7,69,19,2), new GeneratedEnemyUnit(-1,-13,100,33,3), new GeneratedEnemyUnit(16,-13,38,9,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "c40c7842c7ae3b6fb9ed4cb4b6afb0a01960511f3ccd941d23be3d7a99340389");
        }

        private static void Case_05511()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5511,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,4,73,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-19,78,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-4,86,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-4,60,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,17,31,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,6,62,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-1,98,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,18,42,30,2), new GeneratedEnemyUnit(0,16,68,46,3), new GeneratedEnemyUnit(1,0,23,17,2), new GeneratedEnemyUnit(-10,-20,44,1,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "1c4e76e96a66a3d77816b6700849c2827b8843e5e59b9da7fdb6683481d9f335");
        }

        private static void Case_05512()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5512,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,5,16,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,11,89,47,4), new GeneratedEnemyUnit(-4,7,23,26,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "54a36947bb5465f7ad100241b05c6c2ee2695d7a0869c40ade5765d1898eedb7");
        }

        private static void Case_05513()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5513,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-3,20,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-6,39,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,9,45,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,18,35,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-13,95,6,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "91ab2963c4878fb47eafb12a13a1b24aad3e0f33a8a0d005f53ae2d32479ceb5");
        }

        private static void Case_05514()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5514,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-11,21,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-19,37,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-17,10,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-17,22,5,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ca857f8aa0cde2a3fbc45071c6f4a38e8d3971f0e0989bed7abb7dbe2aeb41ec");
        }

        private static void Case_05515()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5515,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,6,37,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,9,5,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-11,85,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,6,96,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-1,87,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-16,8,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-15,8,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-9,41,5,4), new GeneratedEnemyUnit(1,3,17,43,4), new GeneratedEnemyUnit(12,-20,20,32,2), new GeneratedEnemyUnit(0,-1,65,8,3), new GeneratedEnemyUnit(-8,-6,59,20,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "420f937448724813ae4d41dca2d2c94a413dee3165be335eb78ed651f1a3f298");
        }

        private static void Case_05516()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5516,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,14,68,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-19,15,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-20,11,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-18,75,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-10,75,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-15,84,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,16,46,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-7,62,5,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "73ebbe3f876b27f1910c7e3f0a9932c6efaf91e93db5aba080119f471c617f63");
        }

        private static void Case_05517()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5517,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-19,51,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,3,78,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-12,87,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-15,89,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-15,36,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-16,27,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,3,88,30,4), new GeneratedEnemyUnit(-20,14,63,13,4), new GeneratedEnemyUnit(-14,12,96,1,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "fb51d855f72e59ed8a11396dca967a1e898153c7e84646f502fe3d34be6cc3c2");
        }

        private static void Case_05518()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5518,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-11,85,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,4,63,35,2), new GeneratedEnemyUnit(-13,2,29,3,1), new GeneratedEnemyUnit(15,20,61,38,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "d9ed39e5872c64188ae6078b38f7993305a5cbe122dbf96094a92866139f90ed");
        }

        private static void Case_05519()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5519,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-11,36,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,4,95,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,10,89,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-7,32,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,14,5,9,1), new GeneratedEnemyUnit(1,-1,36,24,2), new GeneratedEnemyUnit(-17,-19,8,18,1), new GeneratedEnemyUnit(-15,-13,69,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "485bb2816b6b1cedd49d552981a743d63d59de944dce2660e5d1d107157eef21");
        }

        private static void Case_05520()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5520,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-5,7,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,13,46,9,1), new GeneratedEnemyUnit(15,-9,41,40,2), new GeneratedEnemyUnit(-2,19,50,30,3), new GeneratedEnemyUnit(20,-20,50,13,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "4f88759eff20f5bf2de18ebc7c50ef062b77d1ffe9e994f291bb8001e86002fa");
        }

        private static void Case_05521()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5521,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,19,91,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,16,81,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-16,92,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,2,47,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-1,87,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-7,85,48,4), new GeneratedEnemyUnit(4,-11,38,38,3), new GeneratedEnemyUnit(-18,19,83,19,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "481be4ad9d58a496c35873341fd1faa837de947c9ddb2762961e98ecf62c0995");
        }

        private static void Case_05522()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5522,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-4,10,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-2,5,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,9,79,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,13,66,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-4,29,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,6,39,31,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "faadf2ec510f7be546405e3884b5efe2b5f3537609e9b76e7ce2bae4185c182e");
        }

        private static void Case_05523()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5523,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-6,25,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-17,31,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-18,22,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,17,59,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,7,41,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,13,97,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-5,71,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,3,53,26,2), new GeneratedEnemyUnit(15,11,39,50,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "cd8b97757d1b63a9b8e0592f0a7abd8f2d6c70edcca87d24b9c93341f5d142aa");
        }

        private static void Case_05524()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5524,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-15,7,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,20,53,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-2,71,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,17,59,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-12,6,49,1), new GeneratedEnemyUnit(-12,-3,35,11,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "a908029bce6c446b77fa22a654ccf04ac676e01f9e8e7bb5915904d8caad3a00");
        }

        private static void Case_05525()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5525,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-4,71,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,11,7,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-1,50,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,1,84,11,2), new GeneratedEnemyUnit(17,-2,61,41,2), new GeneratedEnemyUnit(-17,19,85,32,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "60799042bcafb830193f78996426f141d6e489dc91edea2ca8e482500b45e385");
        }

        private static void Case_05526()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5526,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,10,77,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,0,83,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-9,45,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-17,24,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,7,30,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-2,22,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-19,22,17,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "f63bef9d56bf6b779754fce03aff3fe29cde4e94244175a5fc0303407ed1df0c");
        }

        private static void Case_05527()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5527,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-7,91,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,10,70,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-13,32,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,9,52,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,12,46,15,1), new GeneratedEnemyUnit(0,-13,86,43,1), new GeneratedEnemyUnit(2,-12,30,42,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "3a5701f04bfc69897e5ba12fad9df7b549891063ddcd853fcf8f4dd8cb9e0744");
        }

        private static void Case_05528()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5528,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,3,21,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-15,62,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,6,8,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,9,86,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,5,38,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,5,90,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-2,37,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,12,58,4,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "3df15b873fb897e182a6aca30e92841a759a9ac933ea8c0d2aee64b37f5e9256");
        }

        private static void Case_05529()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5529,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,17,66,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-12,22,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,1,29,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-2,66,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,0,98,8,3), new GeneratedEnemyUnit(2,14,78,29,4), new GeneratedEnemyUnit(-15,7,78,40,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "efd22963d60043663bcd79671332d891ab60a19abe40ce7b212e55ee08f660c7");
        }

        private static void Case_05530()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5530,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-8,64,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-5,56,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,11,62,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-15,56,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,1,97,12,2), new GeneratedEnemyUnit(5,5,33,43,3), new GeneratedEnemyUnit(18,-5,84,13,3), new GeneratedEnemyUnit(-4,14,91,7,4), new GeneratedEnemyUnit(9,-19,33,33,4), new GeneratedEnemyUnit(-9,12,83,31,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "c3925bea1987acb7e4ae8305529e632baba95b45aa6c317ea57136e89035aa95");
        }

        private static void Case_05531()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5531,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-17,25,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,5,6,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,13,92,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,2,34,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-20,82,17,4), new GeneratedEnemyUnit(11,18,100,38,2), new GeneratedEnemyUnit(-15,10,15,46,3), new GeneratedEnemyUnit(14,-11,64,36,4), new GeneratedEnemyUnit(4,0,89,25,1), new GeneratedEnemyUnit(6,-16,5,2,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "1ea8a55cae950547381da8e3cc7499d9c091f49e30ed5f59ee7c1be7d66fb388");
        }

        private static void Case_05532()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5532,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,1,73,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-12,64,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,8,79,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-9,29,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,5,37,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,0,26,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,17,35,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,19,66,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,12,47,45,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "2569a78807092d71702c57c65fea77f260e1d03f143e4572f9bc0a8f45cfed78");
        }

        private static void Case_05533()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5533,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-12,38,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-4,13,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,3,55,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,13,73,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-15,56,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,12,19,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-2,50,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-2,41,45,4), new GeneratedEnemyUnit(7,-4,21,11,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "b1a29631280a614c7a25b4a3519f5239dfa9a85a6017b10db10815d2a39ce430");
        }

        private static void Case_05534()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5534,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,11,31,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,7,36,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-13,92,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,11,68,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,18,99,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-13,57,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-16,48,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,18,96,21,4), new GeneratedEnemyUnit(-6,10,51,34,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "22728a62549c8160db7cafeda358c53f367aa969a5f0b7dabfc7a5713bb19abd");
        }

        private static void Case_05535()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5535,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-12,12,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,16,25,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-17,78,14,2), new GeneratedEnemyUnit(10,-13,91,23,3), new GeneratedEnemyUnit(5,-8,44,23,3), new GeneratedEnemyUnit(6,17,28,7,1), new GeneratedEnemyUnit(15,0,89,33,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "f1df34041b80ae0523ffdada004127585d51a78de5f3087f6f0c8e0f2e3ec716");
        }

        private static void Case_05536()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5536,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-18,9,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-13,24,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-18,38,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-19,12,35,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "7bb6073c617b644961396193af75b0bce8eea5cdb0f07297df83fc916963946a");
        }

        private static void Case_05537()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5537,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,6,49,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-10,16,33,1), new GeneratedEnemyUnit(14,14,68,28,1), new GeneratedEnemyUnit(1,-12,7,49,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "25ad448d2f5f989a2635a66fe1a4b765d995a0fb905ee09d030e5e5207e1d19d");
        }

        private static void Case_05538()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5538,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,2,36,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,6,78,2,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ce6a37bb92974cfa7ef6c8b04c657f0f3d9a7484974434450077f59cfd082e68");
        }

        private static void Case_05539()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5539,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-4,48,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,19,26,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,3,90,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-18,17,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,20,46,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,4,74,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-3,8,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,18,96,25,4), new GeneratedEnemyUnit(4,-18,97,23,4), new GeneratedEnemyUnit(-3,5,59,7,1), new GeneratedEnemyUnit(17,9,38,41,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "8ddbdd3cf369a96cabd02a434abbb5063bc07688c2d0cdeec479c2fdfc55a252");
        }

        private static void Case_05540()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5540,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,1,85,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-9,40,30,3), new GeneratedEnemyUnit(-18,-2,12,33,4), new GeneratedEnemyUnit(-15,-6,73,31,1), new GeneratedEnemyUnit(-20,6,80,21,3), new GeneratedEnemyUnit(-15,-1,56,9,1), new GeneratedEnemyUnit(-18,3,49,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "4b69d1b9c18cf9350400af3e76179a294021e3cd0188b08b49fc90af141aeca0");
        }

        private static void Case_05541()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5541,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-6,88,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-7,28,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,11,95,7,2), new GeneratedEnemyUnit(12,7,72,7,2), new GeneratedEnemyUnit(6,-2,86,21,3), new GeneratedEnemyUnit(-14,20,72,16,3), new GeneratedEnemyUnit(-5,11,7,47,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "2a9f4de57092e2f95f2a58bbb790a7eaf3fcba5edeada29e06daa37983314a42");
        }

        private static void Case_05542()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5542,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,10,31,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,1,29,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,20,73,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-5,28,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,10,25,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,9,35,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,13,58,16,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "ec3ecfde7a109e4fa3dba60221bbf6a083cd658a29946f9bf4db427e87c2c25a");
        }

        private static void Case_05543()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5543,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-7,73,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-3,23,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,20,53,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,2,34,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,12,75,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,18,75,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,11,48,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-15,72,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-20,75,38,4), new GeneratedEnemyUnit(8,1,74,35,4), new GeneratedEnemyUnit(-3,-17,96,49,4), new GeneratedEnemyUnit(13,-1,81,10,4), new GeneratedEnemyUnit(11,-14,65,21,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "b44862f94bee8ebe84e099cd9c0905cd5f4ce97b4172e6c74bc1e1e6fb1f83b2");
        }

        private static void Case_05544()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5544,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-12,24,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,16,73,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-16,11,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-12,83,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,3,9,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,12,46,47,3), new GeneratedEnemyUnit(20,-17,54,32,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "aeaca5ddc688f8caffed7e39450353f167f839ba9174e6b9acafdc46d8a13c8b");
        }

        private static void Case_05545()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5545,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,1,13,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,4,25,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,15,83,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,6,27,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,14,46,14,1), new GeneratedEnemyUnit(11,19,54,43,2), new GeneratedEnemyUnit(0,-8,7,28,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "c44f2056c1a06f02ffccc142589cba9c792f34f3fca6330194f60f60325a36ad");
        }

        private static void Case_05546()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5546,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-3,12,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,11,59,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-5,80,11,1), new GeneratedEnemyUnit(-9,-2,16,21,3), new GeneratedEnemyUnit(-13,20,93,5,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "edae1382870f4394232326fbfdf4351d4b2b53d01d932bf4abeb0c40ef632571");
        }

        private static void Case_05547()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5547,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,9,72,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,8,28,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,4,32,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-18,86,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,15,38,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,20,90,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,2,81,31,2), new GeneratedEnemyUnit(-10,19,35,46,3), new GeneratedEnemyUnit(-12,7,37,3,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "127d7d7c296e4f3c64f4ffc224c2e1f8237da854aaf3e3ebe151bc12cb754751");
        }

        private static void Case_05548()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5548,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,12,94,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,14,22,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,16,70,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-14,24,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,7,54,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-6,41,24,1), new GeneratedEnemyUnit(-9,15,61,25,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "ed61a8da2867459819873dff1c8c3f1944132330fc067270f858ca8056fe8d2a");
        }

        private static void Case_05549()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5549,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,0,44,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,5,29,41,3), new GeneratedEnemyUnit(2,12,21,36,3), new GeneratedEnemyUnit(-19,19,7,35,4), new GeneratedEnemyUnit(-10,16,69,18,3), new GeneratedEnemyUnit(3,-6,24,37,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "62c3c234fdf1609ad936216f982e387295339cfaab4f30df0f2ef5e0406144c2");
        }

        private static void Case_05550()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5550,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,18,12,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,15,15,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-7,47,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-11,52,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,10,11,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,17,13,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-2,72,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-14,54,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-20,34,29,2), new GeneratedEnemyUnit(-20,10,47,46,3), new GeneratedEnemyUnit(15,-7,42,46,3), new GeneratedEnemyUnit(8,12,21,33,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "8695b69fcd3a01d44a3313be274ea5661ade1375770f1b68e592bd3e55cfdb9f");
        }

        private static void Case_05551()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5551,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,0,45,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-6,19,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,19,75,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,20,89,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,4,13,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,14,59,37,4), new GeneratedEnemyUnit(13,17,50,32,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "08285f0266017b1d158ee0750014663254c9b87eb1f6fd498a904c7304bad1f1");
        }

        private static void Case_05552()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5552,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,14,31,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,13,21,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,13,20,44,2), new GeneratedEnemyUnit(-11,15,100,12,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0312d50f9177c8e26f696de8d41ed475daf430ce48aa66fa8f1124a8527e738c");
        }

        private static void Case_05553()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5553,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,13,82,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,16,35,6,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "12ea398cd5a11a6f798466a905504545621d357fe0253b9934a5052af593c1a5");
        }

        private static void Case_05554()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5554,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,2,89,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-13,95,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,5,56,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,8,55,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-18,14,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,6,80,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,12,44,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,19,65,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,15,90,10,2), new GeneratedEnemyUnit(-17,-15,58,30,4), new GeneratedEnemyUnit(15,-8,89,25,1), new GeneratedEnemyUnit(-3,20,31,19,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "7ab9ea7e3418b67b57762fbd7c3eb1f4ef3606412b33505f04a8bf6ece06cf65");
        }

        private static void Case_05555()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5555,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,8,80,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-18,86,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,3,35,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-1,18,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-12,50,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-10,82,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,2,5,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,11,92,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,19,95,36,4), new GeneratedEnemyUnit(-16,18,87,39,4), new GeneratedEnemyUnit(11,-19,95,5,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "48541d8a09bb4fa54dd02413669735e981e537740e7ec55701577bdcd7ff2da6");
        }

        private static void Case_05556()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5556,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,5,84,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,2,44,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,1,82,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,14,77,1,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "1f53f3c52cdb2e7b22646db4f3b9e19bb09414e38c9cc8f7eec11f55dd0df46d");
        }

        private static void Case_05557()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5557,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-14,58,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,19,57,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,0,39,34,2), new GeneratedEnemyUnit(15,-3,95,20,2), new GeneratedEnemyUnit(-15,-4,94,13,1), new GeneratedEnemyUnit(-16,16,36,29,3), new GeneratedEnemyUnit(14,4,25,39,2), new GeneratedEnemyUnit(-14,-6,39,39,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "3e966f319c75194f0294e9215b2f37a3c05a52bfb68d0c60992351a153125df8");
        }

        private static void Case_05558()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5558,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-17,77,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,10,77,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-1,67,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,12,49,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,3,43,43,4), new GeneratedEnemyUnit(7,-6,46,9,3), new GeneratedEnemyUnit(-20,-4,29,44,1), new GeneratedEnemyUnit(15,5,71,15,2), new GeneratedEnemyUnit(17,5,43,48,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "c169e882935d07c9fca7edbd4ee22ee822ea49cd1ab8823b4a7c982a13cd39c9");
        }

        private static void Case_05559()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5559,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-4,58,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,8,17,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,9,37,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-14,30,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,3,100,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-6,96,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,5,19,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,20,31,50,1), new GeneratedEnemyUnit(9,9,63,39,4), new GeneratedEnemyUnit(-2,-13,34,34,2), new GeneratedEnemyUnit(2,2,32,38,2), new GeneratedEnemyUnit(7,-15,16,40,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "c422474196562c397c8a7543490d604906d6316e65e9f9296e597d9a9428415e");
        }

        private static void Case_05560()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5560,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-3,7,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-3,51,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-9,9,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,12,85,50,2), new GeneratedEnemyUnit(-17,5,88,4,3), new GeneratedEnemyUnit(20,1,29,50,4), new GeneratedEnemyUnit(2,0,48,6,3), new GeneratedEnemyUnit(11,16,17,8,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "577b1f3fee5aac04eb68dff518f95d942118fca2c262b3348cd42ec7708b7dd4");
        }

        private static void Case_05561()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5561,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,5,90,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-2,26,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-18,94,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,15,63,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-15,75,7,3), new GeneratedEnemyUnit(-8,4,60,18,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "923266c170e3babb06e344cff223b67b157e453e86a079486b65632edeade9d9");
        }

        private static void Case_05562()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5562,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,17,24,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-19,67,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-4,79,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,7,72,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,13,26,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-4,34,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-17,56,4,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "9dd9e2be51a73369e51d0f78587daf15ea4c851c2dfd143a72fe86f3ba5a2944");
        }

        private static void Case_05563()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5563,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-5,48,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,10,100,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-1,66,5,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ccdbae6a76c88c08b5186fac3065a97ea5d38aa07662cd6dac8e730a55f292ed");
        }

        private static void Case_05564()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5564,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-11,80,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-12,39,12,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "b137a4e4e1a3c285702b10f009486b3a09ff524b7b26253d428d0d4e2dd85cc1");
        }

        private static void Case_05565()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5565,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,0,10,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-9,58,5,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "184fa1ee4fb84e14475baac527f4c81056adfe0de9731d21ef78d1c4f0afea06");
        }

        private static void Case_05566()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5566,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,9,67,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-14,70,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,20,5,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,0,79,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,7,97,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-7,35,26,2), new GeneratedEnemyUnit(20,9,19,49,2), new GeneratedEnemyUnit(0,-16,54,4,1), new GeneratedEnemyUnit(-3,-2,79,39,3), new GeneratedEnemyUnit(6,20,30,24,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "aaf7fc5f15778b03e3c899352c3bb2a7c309c774c3c6d5932ec2518817fc2612");
        }

        private static void Case_05567()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5567,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,10,51,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-8,11,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,18,87,50,1), new GeneratedEnemyUnit(0,8,73,14,2), new GeneratedEnemyUnit(-13,7,80,12,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "2e0bca5381efe4e92e2dc33a25434677aa53d9824bfb6915ace4317e2ba61ee6");
        }

        private static void Case_05568()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5568,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-7,23,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-12,83,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,12,50,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-6,12,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,14,51,41,1), new GeneratedEnemyUnit(18,7,91,5,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b8088b74ad0cf6e3cc8ec7a71bf8f0a10a04076eb5597a6693bd2ce783854780");
        }

        private static void Case_05569()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5569,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,18,96,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,16,48,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,3,58,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-2,71,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-4,58,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-5,27,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,16,98,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-5,43,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-20,9,49,3), new GeneratedEnemyUnit(3,-6,83,37,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "922d668bf6d78a8f498191695e40e89d7dde3d49e0ef3216fcc85eb7fa29ea34");
        }

        private static void Case_05570()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5570,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-19,66,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-2,89,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,15,36,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-16,38,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,14,81,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,10,12,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,17,16,46,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "3876ee1f78ae965b711b889c6d2b8a4c000d39b2bba79d380c5a97d9be6307e1");
        }

        private static void Case_05571()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5571,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,0,90,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,19,84,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-13,82,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-17,29,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,3,21,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,17,53,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,10,43,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,4,79,48,4), new GeneratedEnemyUnit(-17,7,59,34,4), new GeneratedEnemyUnit(5,-17,57,27,1), new GeneratedEnemyUnit(16,2,45,48,3), new GeneratedEnemyUnit(-5,13,19,1,1), new GeneratedEnemyUnit(-17,18,23,7,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "40bdfc3d2d0523058fefa1ce6a4610b8e65bb8d28db064600e35064d78c1e9f0");
        }

        private static void Case_05572()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5572,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-3,32,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-20,45,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-18,29,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,3,52,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,8,49,16,2), new GeneratedEnemyUnit(15,16,78,19,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "b7a1d78f9dc6c50397429511655c33ccd94145c8d2fe80bcf3af4757c871b301");
        }

        private static void Case_05573()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5573,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-16,94,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-20,61,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-1,42,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,1,52,50,1), new GeneratedEnemyUnit(15,16,34,15,4), new GeneratedEnemyUnit(-5,9,47,20,3), new GeneratedEnemyUnit(-14,-10,40,22,1), new GeneratedEnemyUnit(6,-13,56,24,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1ca6ad4ff35011c6955f62fdcd9e40b89d9190b4666912e50c42372931c6ca0b");
        }

        private static void Case_05574()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5574,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,4,72,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,6,85,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,3,25,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,20,96,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,9,73,45,4), new GeneratedEnemyUnit(-14,7,18,25,4), new GeneratedEnemyUnit(7,14,94,39,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "c8dfe13379621a2d6dc349024a67e948c5f471dc051e07775dbaaa78c3c84e88");
        }

        private static void Case_05575()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5575,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,17,47,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-9,63,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,2,27,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,19,93,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-17,57,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,16,54,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-16,63,45,3), new GeneratedEnemyUnit(-18,9,99,43,3), new GeneratedEnemyUnit(-19,-19,43,7,3), new GeneratedEnemyUnit(7,-11,70,29,3), new GeneratedEnemyUnit(-12,-4,38,44,2), new GeneratedEnemyUnit(16,16,57,20,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "306b137d77cd885258b2b9d39976a819fcb933847ef8a73034edf97d0f832e79");
        }

        private static void Case_05576()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5576,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-12,100,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-18,87,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-6,60,27,2), new GeneratedEnemyUnit(18,-14,88,8,1), new GeneratedEnemyUnit(-6,-13,64,7,4), new GeneratedEnemyUnit(14,-5,68,47,1), new GeneratedEnemyUnit(7,17,71,33,2), new GeneratedEnemyUnit(-18,5,27,37,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "6c07bedf006dd334836b60e5739aebd25d96a188639f67055faa00438eccf837");
        }

        private static void Case_05577()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5577,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,9,47,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,10,83,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-12,15,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,7,55,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,16,49,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-8,21,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-6,19,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-13,100,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-14,89,11,1), new GeneratedEnemyUnit(-20,11,13,20,3), new GeneratedEnemyUnit(17,-6,65,32,4), new GeneratedEnemyUnit(-14,-13,98,34,3), new GeneratedEnemyUnit(-3,7,18,1,4), new GeneratedEnemyUnit(8,3,59,8,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "f8938068c6c1f7432e565cc2e9f236872e675692528c5f3bcd75e539c6f8956c");
        }

        private static void Case_05578()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5578,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-10,99,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-12,65,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,9,62,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-4,17,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,8,13,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,13,48,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,19,39,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,7,44,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-9,98,34,4), new GeneratedEnemyUnit(-19,-6,9,45,1), new GeneratedEnemyUnit(-19,-5,99,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7c3104ab26e4f5e8f21e22dd0e946170e46372e6010dc06e0b29c96c3476084a");
        }

        private static void Case_05579()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5579,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-14,30,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,10,90,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,0,21,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,14,69,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,20,6,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-4,37,46,1), new GeneratedEnemyUnit(14,-18,25,8,4), new GeneratedEnemyUnit(1,-12,37,1,3), new GeneratedEnemyUnit(4,20,65,50,1), new GeneratedEnemyUnit(-16,-15,51,13,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "558030f3f2ffa38f7cdec17c487d0e7ce21a43566778a5df03a2f9ff4ad51e85");
        }

    }
}
