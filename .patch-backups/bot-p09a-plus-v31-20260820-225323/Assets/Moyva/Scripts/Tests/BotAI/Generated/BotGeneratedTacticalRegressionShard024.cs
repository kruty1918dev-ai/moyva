using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard024
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_04320();
            Case_04321();
            Case_04322();
            Case_04323();
            Case_04324();
            Case_04325();
            Case_04326();
            Case_04327();
            Case_04328();
            Case_04329();
            Case_04330();
            Case_04331();
            Case_04332();
            Case_04333();
            Case_04334();
            Case_04335();
            Case_04336();
            Case_04337();
            Case_04338();
            Case_04339();
            Case_04340();
            Case_04341();
            Case_04342();
            Case_04343();
            Case_04344();
            Case_04345();
            Case_04346();
            Case_04347();
            Case_04348();
            Case_04349();
            Case_04350();
            Case_04351();
            Case_04352();
            Case_04353();
            Case_04354();
            Case_04355();
            Case_04356();
            Case_04357();
            Case_04358();
            Case_04359();
            Case_04360();
            Case_04361();
            Case_04362();
            Case_04363();
            Case_04364();
            Case_04365();
            Case_04366();
            Case_04367();
            Case_04368();
            Case_04369();
            Case_04370();
            Case_04371();
            Case_04372();
            Case_04373();
            Case_04374();
            Case_04375();
            Case_04376();
            Case_04377();
            Case_04378();
            Case_04379();
            Case_04380();
            Case_04381();
            Case_04382();
            Case_04383();
            Case_04384();
            Case_04385();
            Case_04386();
            Case_04387();
            Case_04388();
            Case_04389();
            Case_04390();
            Case_04391();
            Case_04392();
            Case_04393();
            Case_04394();
            Case_04395();
            Case_04396();
            Case_04397();
            Case_04398();
            Case_04399();
            Case_04400();
            Case_04401();
            Case_04402();
            Case_04403();
            Case_04404();
            Case_04405();
            Case_04406();
            Case_04407();
            Case_04408();
            Case_04409();
            Case_04410();
            Case_04411();
            Case_04412();
            Case_04413();
            Case_04414();
            Case_04415();
            Case_04416();
            Case_04417();
            Case_04418();
            Case_04419();
            Case_04420();
            Case_04421();
            Case_04422();
            Case_04423();
            Case_04424();
            Case_04425();
            Case_04426();
            Case_04427();
            Case_04428();
            Case_04429();
            Case_04430();
            Case_04431();
            Case_04432();
            Case_04433();
            Case_04434();
            Case_04435();
            Case_04436();
            Case_04437();
            Case_04438();
            Case_04439();
            Case_04440();
            Case_04441();
            Case_04442();
            Case_04443();
            Case_04444();
            Case_04445();
            Case_04446();
            Case_04447();
            Case_04448();
            Case_04449();
            Case_04450();
            Case_04451();
            Case_04452();
            Case_04453();
            Case_04454();
            Case_04455();
            Case_04456();
            Case_04457();
            Case_04458();
            Case_04459();
            Case_04460();
            Case_04461();
            Case_04462();
            Case_04463();
            Case_04464();
            Case_04465();
            Case_04466();
            Case_04467();
            Case_04468();
            Case_04469();
            Case_04470();
            Case_04471();
            Case_04472();
            Case_04473();
            Case_04474();
            Case_04475();
            Case_04476();
            Case_04477();
            Case_04478();
            Case_04479();
            Case_04480();
            Case_04481();
            Case_04482();
            Case_04483();
            Case_04484();
            Case_04485();
            Case_04486();
            Case_04487();
            Case_04488();
            Case_04489();
            Case_04490();
            Case_04491();
            Case_04492();
            Case_04493();
            Case_04494();
            Case_04495();
            Case_04496();
            Case_04497();
            Case_04498();
            Case_04499();
        }

        private static void Case_04320()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4320,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,17,61,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,15,71,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-11,41,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,9,48,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-20,21,17,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9cd06917158d8cbbaa0b4a523fe48c4567b3d958e5d1f22c216f72d607506b7e");
        }

        private static void Case_04321()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4321,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,14,66,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,18,77,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-14,58,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-4,42,15,1), new GeneratedEnemyUnit(5,-19,55,46,1), new GeneratedEnemyUnit(4,-4,56,7,1), new GeneratedEnemyUnit(16,6,38,28,4), new GeneratedEnemyUnit(2,-7,96,33,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "941e74f1567b4322e114d528873d1b8f922a085514d6c65c0db8c13178df2fb3");
        }

        private static void Case_04322()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4322,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,19,24,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,10,50,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,11,90,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,10,42,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,16,57,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,14,57,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,6,16,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,1,85,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-12,5,41,2), new GeneratedEnemyUnit(-14,19,58,9,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "fd4852c5139c234d8238866a4b90a879d8e787f2d7537b833045890732a5614b");
        }

        private static void Case_04323()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4323,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,8,17,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-17,15,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-20,20,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,20,16,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,6,36,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,18,73,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,13,69,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,4,29,27,2), new GeneratedEnemyUnit(-16,14,23,43,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "787e0cbd86583b3c65d4dbd06e09c9bef03b4f9094260c2190d11c7c672a53c4");
        }

        private static void Case_04324()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4324,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,16,28,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-2,54,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,6,19,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-1,38,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-15,5,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,0,80,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-20,7,25,2), new GeneratedEnemyUnit(-10,6,34,43,4), new GeneratedEnemyUnit(9,-3,46,12,4), new GeneratedEnemyUnit(14,-15,6,6,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "f77d2024e79ff299b5b0f1da65da0ef3f031f390e162b9015bf0c35d1fe3854a");
        }

        private static void Case_04325()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4325,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,6,48,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,19,82,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,2,17,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,16,30,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-2,76,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,19,22,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,9,92,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,4,27,7,3), new GeneratedEnemyUnit(-20,-1,30,17,4), new GeneratedEnemyUnit(1,-5,26,40,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "1bc10215dfc2b3ad803a1772676b452104e4bfb816eb5cd32f906055bb11177d");
        }

        private static void Case_04326()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4326,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-11,59,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-1,55,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,14,41,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,6,67,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,20,14,1,2), new GeneratedEnemyUnit(-19,-14,36,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "e3ae0eba21b9203fea8471a514c251086687649547e402092947a1ef99f34cdd");
        }

        private static void Case_04327()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4327,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-11,59,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,7,21,38,2), new GeneratedEnemyUnit(17,14,89,43,1), new GeneratedEnemyUnit(10,5,48,12,2), new GeneratedEnemyUnit(-8,-15,94,25,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "e36205426a533e10439df0cd77f59d187001902925b2e534c598d3c4745181eb");
        }

        private static void Case_04328()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4328,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,9,37,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,19,41,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-11,53,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-18,24,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,17,73,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,19,80,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-10,84,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,18,93,50,1), new GeneratedEnemyUnit(3,-11,92,17,3), new GeneratedEnemyUnit(-8,16,42,35,4), new GeneratedEnemyUnit(1,-18,77,32,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "77c907fc9a353ce9e9b42670bfd0d0813ec70aadcf7cd551ed7b5c0f12a70e32");
        }

        private static void Case_04329()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4329,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,2,5,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,6,80,50,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "58d8a5baebfb13a7a0c3184facaaa4a7724fa68665a925529a7fdd69e7c813ef");
        }

        private static void Case_04330()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4330,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-18,9,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,12,40,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,6,93,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-8,44,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,17,38,41,4), new GeneratedEnemyUnit(-17,0,99,30,4), new GeneratedEnemyUnit(-6,-13,63,32,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "4faf174a3bd386a0eff4ed19d13aba3d7fbd69fb4b03d7d1a9eafb34b85899d2");
        }

        private static void Case_04331()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4331,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,11,58,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-9,49,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,0,32,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,19,39,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-11,32,16,4), new GeneratedEnemyUnit(-7,12,30,13,1), new GeneratedEnemyUnit(19,-3,87,20,1), new GeneratedEnemyUnit(18,-2,40,46,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "f5c327c82d2a85f29a10c345a03fca5c61fe679fe626d44d4417d8e713ed8f57");
        }

        private static void Case_04332()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4332,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-5,17,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-10,9,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,16,69,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,3,38,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,7,84,1,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "102031087271124d152953d9acf2e89d1dc221057f44a8b15c6795862c3cb16c");
        }

        private static void Case_04333()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4333,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-20,42,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-20,48,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-15,77,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,9,93,12,1), new GeneratedEnemyUnit(8,12,79,7,3), new GeneratedEnemyUnit(4,13,19,14,4), new GeneratedEnemyUnit(-6,-12,95,39,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "69df8303e8830131a1c501593ec989b009ae3612ea98b1e22af8cfd9d1f988c8");
        }

        private static void Case_04334()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4334,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,4,40,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,20,100,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-18,45,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,16,26,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-2,94,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,18,87,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-8,95,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-18,84,2,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "5145031ce175e01046f3b6f2a74e5cbad8794c307f75d9fc158d8d00a2ff6f58");
        }

        private static void Case_04335()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4335,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-2,31,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-17,10,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-16,22,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-12,74,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-3,34,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,5,35,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,11,6,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-12,70,40,3), new GeneratedEnemyUnit(18,7,6,36,4), new GeneratedEnemyUnit(-9,-9,82,31,3), new GeneratedEnemyUnit(-18,-14,60,25,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "4973c22fac30c795ec73dc43a0037dc54f06b2225c7a99437b6fce58332e1820");
        }

        private static void Case_04336()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4336,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,19,28,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,20,19,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-6,20,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-10,92,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-8,69,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,12,55,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-5,12,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,18,59,27,1), new GeneratedEnemyUnit(10,-2,9,47,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "3304dad4f65fc0940fb94cb12ae5fc400b4234cf0da8d651732c8c801dde1e26");
        }

        private static void Case_04337()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4337,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-7,36,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-17,46,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,17,83,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-4,17,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,15,71,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,13,44,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-3,79,36,2), new GeneratedEnemyUnit(-4,9,81,43,2), new GeneratedEnemyUnit(-8,1,31,21,4), new GeneratedEnemyUnit(-10,20,67,1,1), new GeneratedEnemyUnit(17,-3,73,11,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "9f7b10b209a19892c3e1827f24fa2a0510aa382c0bed63ca180f1c1462c13128");
        }

        private static void Case_04338()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4338,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,4,52,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,16,35,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-15,37,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-3,36,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-3,58,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-19,43,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,1,63,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-19,97,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-17,29,25,3), new GeneratedEnemyUnit(10,-16,12,47,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "b48f92272b83444fcb1e566723d3005a37322ed19573970049d39fd3fe6ec875");
        }

        private static void Case_04339()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4339,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,3,83,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,6,97,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,2,69,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,8,23,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-2,67,5,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "21dd89b1e9fe32d9a63739a50e1cf29b9a025c7be0ecb92c62dfed5a987d14ee");
        }

        private static void Case_04340()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4340,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-2,74,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,15,95,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-5,11,17,2), new GeneratedEnemyUnit(-15,-19,56,39,4), new GeneratedEnemyUnit(-2,-10,22,22,2), new GeneratedEnemyUnit(18,1,34,50,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "e205352814119e783acb4bccb88dd8e4e1da470f2fda24f122ce8fdbb6699c4e");
        }

        private static void Case_04341()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4341,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-6,69,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-1,17,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,19,39,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,7,72,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-20,26,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,13,90,44,3), new GeneratedEnemyUnit(0,-1,99,23,1), new GeneratedEnemyUnit(-12,-11,43,32,1), new GeneratedEnemyUnit(3,20,74,11,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "662a7f1f1ed4b5dde90606d5b71c98765e38e20c11ece07e516ceefbfb889d90");
        }

        private static void Case_04342()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4342,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-2,66,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,19,24,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,13,87,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-14,100,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,6,65,7,1), new GeneratedEnemyUnit(0,-7,43,16,4), new GeneratedEnemyUnit(4,2,73,40,3), new GeneratedEnemyUnit(-20,-16,94,26,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "47c2b405741e18b225e6951d9fc145cc4e435cc948ac46930877dd2edc57defd");
        }

        private static void Case_04343()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4343,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,15,90,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,13,33,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,1,24,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-15,92,42,1), new GeneratedEnemyUnit(7,4,80,24,1), new GeneratedEnemyUnit(-11,17,15,6,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6b9b8202c682515cb5a8c122e4ad85693f3d2b86f310d18b5529547c67d264ca");
        }

        private static void Case_04344()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4344,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-16,57,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,8,97,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-20,52,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,3,10,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,18,42,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,19,92,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,4,42,46,2), new GeneratedEnemyUnit(-19,-1,42,9,2), new GeneratedEnemyUnit(19,2,51,6,1), new GeneratedEnemyUnit(8,14,86,38,1), new GeneratedEnemyUnit(-17,-4,28,2,2), new GeneratedEnemyUnit(-7,18,12,15,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "02edd077e7751465ae9918750fa8d5b4e486469ebbc0b0071c4da2c04b94b223");
        }

        private static void Case_04345()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4345,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,5,21,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-20,94,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,18,47,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-2,18,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-7,90,47,1), new GeneratedEnemyUnit(-8,19,70,4,1), new GeneratedEnemyUnit(-19,-2,28,3,4), new GeneratedEnemyUnit(12,8,88,7,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "3b2cfd770ef15c18031a44aa6b3e0d9499d516a0338d1bcd71ab39626583b2c7");
        }

        private static void Case_04346()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4346,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,1,46,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,11,80,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-17,99,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,14,66,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-5,76,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-14,78,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,14,91,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-8,83,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,20,78,1,4), new GeneratedEnemyUnit(0,-10,64,22,4), new GeneratedEnemyUnit(17,2,19,42,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "053105e6cbb44b19f0e1b8d3d4ba85b0f4193acaf9525c8cbae834d53eba409d");
        }

        private static void Case_04347()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4347,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-17,14,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-6,44,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-6,96,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,19,60,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,12,63,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-17,43,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,13,71,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,7,10,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-7,70,50,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "36bfce883334e47e7c071b28aa0306cd074c162eec424f8604ee170e891b866e");
        }

        private static void Case_04348()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4348,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,14,99,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,16,32,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,0,48,27,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "1a4d97edb5fbc7be12c3dda7d4386d9bb07b484d528841a689e1318280d71b5e");
        }

        private static void Case_04349()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4349,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-8,83,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,9,84,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-15,84,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-3,70,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-16,67,26,4), new GeneratedEnemyUnit(-14,9,66,24,3), new GeneratedEnemyUnit(-12,17,81,38,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "a016b32f145a78e493c4dad002db187e81215a5b8477c44db4783bbead1bb365");
        }

        private static void Case_04350()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4350,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,10,32,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,0,18,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,3,61,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,0,80,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "826470c5c591206ef31e019c2aa1530fefd421af49087fe7d250e950760ed303");
        }

        private static void Case_04351()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4351,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-16,5,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-11,60,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-12,53,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-15,49,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,6,12,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-7,85,18,4), new GeneratedEnemyUnit(18,-10,39,36,4), new GeneratedEnemyUnit(-6,-2,52,24,3), new GeneratedEnemyUnit(12,10,86,1,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "9bc6bb279169893d75a8f7641a97b232ae7f5b9274c75404edc512c4c83f61ec");
        }

        private static void Case_04352()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4352,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,4,40,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-17,83,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-7,6,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,11,58,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-9,36,25,1), new GeneratedEnemyUnit(10,-6,41,9,2), new GeneratedEnemyUnit(5,6,28,50,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "3bc060ec0734d64d0a1e59c0e5bf3367958f662a26e4ef25a8ccd3ad61d0f7da");
        }

        private static void Case_04353()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4353,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-5,26,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-14,83,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,19,52,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-5,66,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,18,31,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,4,66,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,8,11,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,8,70,8,2), new GeneratedEnemyUnit(-10,-6,11,18,4), new GeneratedEnemyUnit(-10,11,34,6,3), new GeneratedEnemyUnit(-11,-20,6,13,2), new GeneratedEnemyUnit(-10,4,98,36,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "5c77901eb5e78711b72ebc6987d9ff9dd7832d1041634cf8a478a09d13eb900f");
        }

        private static void Case_04354()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4354,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-8,27,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-17,25,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,12,6,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-17,41,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-4,61,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-6,99,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-8,74,25,2), new GeneratedEnemyUnit(-7,-17,68,36,3), new GeneratedEnemyUnit(-2,-18,42,7,1), new GeneratedEnemyUnit(19,-15,89,36,3), new GeneratedEnemyUnit(-18,-11,35,2,4), new GeneratedEnemyUnit(11,-8,61,29,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "91f45e06fa8368caa79d4852f2ccae5726ac00d583ccf594c510c2812889ebc2");
        }

        private static void Case_04355()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4355,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-4,13,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,11,85,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-2,21,10,1), new GeneratedEnemyUnit(-12,20,48,39,4), new GeneratedEnemyUnit(-18,-2,69,1,2), new GeneratedEnemyUnit(-11,7,7,37,4), new GeneratedEnemyUnit(-13,4,36,24,1), new GeneratedEnemyUnit(-17,19,48,7,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "845bf3a0194401aad4c29f213394195c7d6857a2ab02fc8d79b27c2857fbd1f9");
        }

        private static void Case_04356()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4356,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,10,99,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-10,23,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-5,100,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-2,11,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-4,51,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-12,17,14,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "60419b79f4f8e753bce90986a94fe7747f3e78f4b75f8a8e18fbea5eb77ef7e7");
        }

        private static void Case_04357()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4357,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-9,41,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-1,36,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-13,81,33,1), new GeneratedEnemyUnit(-16,16,44,27,4), new GeneratedEnemyUnit(20,8,7,24,4), new GeneratedEnemyUnit(10,20,85,42,2), new GeneratedEnemyUnit(11,5,73,48,3), new GeneratedEnemyUnit(10,7,10,33,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "59773fdf2e990145cca6620c79bd69933e217cf98a35810640222fed48b67e12");
        }

        private static void Case_04358()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4358,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,19,62,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,13,85,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,4,88,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-14,79,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-19,93,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,15,76,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,17,55,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-17,76,9,3), new GeneratedEnemyUnit(-3,-18,90,42,4), new GeneratedEnemyUnit(-17,-15,57,21,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "fcdaa51ba0406c0e74621880c0a1d82aef4cccb65d08f57368d76820347d5ad1");
        }

        private static void Case_04359()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4359,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-3,93,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,12,87,41,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "1def7b7050ac0f99a27d34572791bc3c5fbc6364f7d1aa51741b49b09d44709d");
        }

        private static void Case_04360()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4360,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-18,49,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,8,40,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-10,28,13,4), new GeneratedEnemyUnit(9,-20,21,47,4), new GeneratedEnemyUnit(8,13,81,18,3), new GeneratedEnemyUnit(19,16,72,25,1), new GeneratedEnemyUnit(2,12,8,39,2), new GeneratedEnemyUnit(-8,-9,75,35,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "bde28381f05d8ae09aa856a09b74eff457869616cf945b29a88f39db755c151b");
        }

        private static void Case_04361()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4361,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-1,13,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,10,75,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,13,58,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-18,50,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,5,6,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,12,59,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-12,12,7,1), new GeneratedEnemyUnit(-19,3,67,36,4), new GeneratedEnemyUnit(11,-16,5,44,1), new GeneratedEnemyUnit(-20,11,79,12,1), new GeneratedEnemyUnit(-9,-14,38,11,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "ee901704abe13f2755debebc0b7cf8675fd514a76926f7c10cf02709a06be4d4");
        }

        private static void Case_04362()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4362,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-17,27,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,12,96,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,16,30,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-18,12,38,3), new GeneratedEnemyUnit(13,10,65,37,1), new GeneratedEnemyUnit(10,-13,6,46,1), new GeneratedEnemyUnit(11,-16,75,35,4), new GeneratedEnemyUnit(-10,-18,44,50,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "52c9ab3a074f317f8fa18ebd94353de82667676b79fd76e99f37f5884079f30a");
        }

        private static void Case_04363()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4363,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,14,71,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,8,7,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-20,53,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,18,76,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,3,82,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,8,50,3,3), new GeneratedEnemyUnit(12,-7,40,22,4), new GeneratedEnemyUnit(3,-16,34,29,4), new GeneratedEnemyUnit(7,13,68,24,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "a1fb48fedb4b207d5805411b6d8e4a5458c79b341e2f484ac6affc7345661037");
        }

        private static void Case_04364()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4364,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-10,33,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,0,93,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-2,92,33,3), new GeneratedEnemyUnit(19,17,6,20,3), new GeneratedEnemyUnit(-3,10,18,42,3), new GeneratedEnemyUnit(7,10,82,27,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "188178793be167b4ac603c742c1bf8ba90310024bee6a0bcd54e811faf87d4f1");
        }

        private static void Case_04365()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4365,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,17,32,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,4,96,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,2,55,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-4,100,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,12,80,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,5,65,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,16,37,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-18,5,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-5,6,12,2), new GeneratedEnemyUnit(-15,-1,58,12,2), new GeneratedEnemyUnit(-8,15,65,38,2), new GeneratedEnemyUnit(-18,-6,82,50,1), new GeneratedEnemyUnit(5,9,53,35,4), new GeneratedEnemyUnit(-13,15,30,13,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "91fcb52b200164eb4a4c9a65c4d4c53cd1cd81d887a35bedf1b9c09de20c86ed");
        }

        private static void Case_04366()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4366,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-9,47,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-16,6,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,9,63,49,2), new GeneratedEnemyUnit(7,18,69,18,1), new GeneratedEnemyUnit(-18,14,68,34,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "25c090d40d5f555e6407c07b4891eda6de200f4659b1f4b70fcaeaed1aa869a8");
        }

        private static void Case_04367()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4367,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,1,78,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,3,20,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-7,72,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-14,83,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-19,52,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-11,65,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,13,14,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-8,10,12,2), new GeneratedEnemyUnit(8,5,90,27,4), new GeneratedEnemyUnit(7,14,14,21,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "97f5614392b90f512d2a2dfb4db9b9a4e639873c3c8e89ba2298d1fb9f496467");
        }

        private static void Case_04368()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4368,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,2,14,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-5,8,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,6,43,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,6,82,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-3,55,29,1), new GeneratedEnemyUnit(13,17,100,49,1), new GeneratedEnemyUnit(17,18,39,36,1), new GeneratedEnemyUnit(-17,17,8,14,2), new GeneratedEnemyUnit(-3,1,90,43,1), new GeneratedEnemyUnit(-2,-3,71,47,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "0145cae8d0236938a845d5b0a6d5a44c22849be81a3ac09cfd9bb5a967bffa07");
        }

        private static void Case_04369()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4369,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,18,12,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,15,100,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-4,58,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,4,91,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-17,79,3,1), new GeneratedEnemyUnit(0,-9,47,10,1), new GeneratedEnemyUnit(-16,11,7,35,2), new GeneratedEnemyUnit(-18,7,41,39,4), new GeneratedEnemyUnit(-15,8,7,3,4), new GeneratedEnemyUnit(-4,6,27,50,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "10f4aa2ee5723246cac67e536f3fea2ec98aa7cfd757a89c87ba320a04fdb41d");
        }

        private static void Case_04370()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4370,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-5,40,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,7,39,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-10,65,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-9,34,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,11,71,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,7,34,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-14,8,8,2), new GeneratedEnemyUnit(0,1,56,1,1), new GeneratedEnemyUnit(-12,1,49,14,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "fa12588602d84f7a5334c34c492c51dad88b89a0f6fb9966049ad7aaaf0addeb");
        }

        private static void Case_04371()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4371,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,8,25,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-6,83,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-17,86,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-3,88,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-19,33,33,4), new GeneratedEnemyUnit(-2,10,59,26,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b07dc684cc36adfbcba65425ba56cc69c18357ef91d895456bf8d5eeddcf5ac9");
        }

        private static void Case_04372()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4372,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-1,35,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-10,49,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-2,94,38,4), new GeneratedEnemyUnit(-18,-17,38,30,1), new GeneratedEnemyUnit(2,-17,34,30,4), new GeneratedEnemyUnit(-16,9,99,2,4), new GeneratedEnemyUnit(-16,6,76,45,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "dd7e531bb16748850db4255285f7e70119a43da05350c801f03471367b99a224");
        }

        private static void Case_04373()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4373,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-19,69,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-14,54,1,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "b613238a528b241317b8a06c1492068361354349c15d0d4d911349c0868e3e12");
        }

        private static void Case_04374()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4374,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,8,56,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,8,22,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,13,83,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-3,84,43,4), new GeneratedEnemyUnit(9,-1,95,31,3), new GeneratedEnemyUnit(7,17,96,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "2c0b7ed4a465f7fd9e218f9d819309546c5729ba78fdf36d1a31466312e3b273");
        }

        private static void Case_04375()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4375,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,19,97,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,20,5,31,1), new GeneratedEnemyUnit(-17,9,23,13,1), new GeneratedEnemyUnit(6,-7,74,31,4), new GeneratedEnemyUnit(15,3,30,16,3), new GeneratedEnemyUnit(5,10,79,22,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "65a94b940796717726d1fcc9c96d504623bc48fc836c04bcc7acbb81cd06fbed");
        }

        private static void Case_04376()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4376,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-9,35,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-11,23,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-4,10,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-15,41,17,1), new GeneratedEnemyUnit(-18,4,7,36,4), new GeneratedEnemyUnit(-6,-11,54,25,3), new GeneratedEnemyUnit(-8,-18,83,30,3), new GeneratedEnemyUnit(0,20,16,32,3), new GeneratedEnemyUnit(-18,-3,34,13,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "798d4b3757f861449c612ed9bd93500fceb760f6bf6409d53a13b1a4b630a7a5");
        }

        private static void Case_04377()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4377,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-7,84,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-17,83,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-2,97,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,17,44,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-20,54,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,2,95,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-11,12,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-11,53,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-7,88,38,4), new GeneratedEnemyUnit(-9,17,68,8,2), new GeneratedEnemyUnit(14,13,30,22,1), new GeneratedEnemyUnit(2,8,88,47,2), new GeneratedEnemyUnit(16,-4,83,48,2), new GeneratedEnemyUnit(19,6,87,33,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "28424ecddaaa09849536acbb90f5486842ebe6f7a3c03fa31fd34f2c2953b486");
        }

        private static void Case_04378()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4378,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,4,56,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-1,13,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-1,89,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,7,11,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,3,73,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,13,33,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,18,91,21,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "e2dff08239627fb7208a496088f9af75fc4aaa885952b537ff77ca1993a66946");
        }

        private static void Case_04379()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4379,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-15,51,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,9,35,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-1,52,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-4,52,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,20,73,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-3,40,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-9,28,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-3,48,20,3), new GeneratedEnemyUnit(16,17,97,20,4), new GeneratedEnemyUnit(-7,-8,82,3,4), new GeneratedEnemyUnit(8,-17,20,30,3), new GeneratedEnemyUnit(-8,-5,24,40,1), new GeneratedEnemyUnit(5,7,97,7,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "c976f9d0fc0fcc3961960ebeecd199492358da7088c474591af86c81d53ddccc");
        }

        private static void Case_04380()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4380,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-1,57,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-13,82,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-8,99,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,0,59,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,2,77,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-20,36,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-14,97,35,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "e2c145af8244247b626af94169d17d934c980baffdbbbcd0dfc2a8db67019097");
        }

        private static void Case_04381()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4381,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-2,86,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,12,17,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-5,78,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-3,60,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,0,47,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,13,11,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-2,47,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,4,13,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-18,59,15,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "63c03ab257e416321cbdff840abc79c1424b741de2e88d39e341a50b83cb0ca6");
        }

        private static void Case_04382()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4382,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,13,18,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,4,39,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-19,35,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-17,47,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,8,27,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-2,53,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,0,89,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-2,80,26,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "259672ae05c40ff65dd5530e1155ca7d44763ddb604520a9a61353c26f926e93");
        }

        private static void Case_04383()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4383,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-18,34,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-16,32,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,8,80,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,11,38,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,17,37,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,15,87,18,2), new GeneratedEnemyUnit(-19,0,43,27,2), new GeneratedEnemyUnit(-18,-19,62,22,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "7b67460a6297f4dd1cb675fb5abd9192be6053cfba7392cda3eef471d387207d");
        }

        private static void Case_04384()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4384,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-17,57,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-7,22,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,14,75,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-14,53,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,18,92,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,6,49,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,15,100,3,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "f4134479051856e3292bf8628a8b57a8b9beb24858528dbb7c9a747c2ab936e8");
        }

        private static void Case_04385()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4385,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,18,82,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,20,50,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,3,60,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,12,35,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,2,92,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,2,64,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-13,54,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-3,23,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-1,71,26,1), new GeneratedEnemyUnit(17,-4,58,20,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "5425fbb1a70f128453671c5eeb94a5168b99499cd05f2bc7ac29ecf35d070fe2");
        }

        private static void Case_04386()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4386,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,17,62,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-4,68,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-16,74,5,1), new GeneratedEnemyUnit(-17,4,10,23,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "5e454057689d0c787b6392924076754e8a57ccfb9f558690ffea2674be8fed94");
        }

        private static void Case_04387()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4387,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-15,12,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-14,56,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,7,98,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,20,47,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-5,24,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-11,5,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-20,48,6,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c7d8362213ae53c08476480a72e89c4cf1d4e26d88c8c28f7aa0e8fdec4f7c70");
        }

        private static void Case_04388()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4388,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,5,76,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,8,59,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-13,59,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,1,20,19,4), new GeneratedEnemyUnit(-11,9,43,20,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "b98f9924df8a2facb3a2cfd6cb41719139855ba1159e83bee4bf318493d357bc");
        }

        private static void Case_04389()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4389,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,11,47,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-20,36,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-4,28,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,4,50,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,16,66,21,4), new GeneratedEnemyUnit(14,-5,15,30,1), new GeneratedEnemyUnit(-13,9,57,39,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "89050026be0487485eb3d663b72d766e2b293473b39b7e158563cd6caa004fc9");
        }

        private static void Case_04390()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4390,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,12,22,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-5,9,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,12,20,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-1,33,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-19,30,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,4,37,46,4), new GeneratedEnemyUnit(-1,-10,11,28,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "61cbf691961bcb30ffc020a0d64529dd2b9e9fa4b90059ff6cd139be80b89519");
        }

        private static void Case_04391()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4391,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-12,9,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-13,74,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,12,21,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,13,78,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-2,73,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-16,16,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-5,90,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,5,20,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,4,85,31,1), new GeneratedEnemyUnit(20,-6,39,6,4), new GeneratedEnemyUnit(-6,9,58,18,1), new GeneratedEnemyUnit(13,20,94,30,2), new GeneratedEnemyUnit(-16,-6,67,27,2), new GeneratedEnemyUnit(15,0,43,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "9e078dca8f4a517bbd219ff505af8b5533fd52c88dd226b5642fe4f989040fc7");
        }

        private static void Case_04392()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4392,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,7,81,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,3,54,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,20,14,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,1,36,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,11,89,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-3,66,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,16,26,11,4), new GeneratedEnemyUnit(-18,18,18,15,3), new GeneratedEnemyUnit(20,6,38,15,4), new GeneratedEnemyUnit(18,4,54,50,2), new GeneratedEnemyUnit(-2,-19,51,28,2), new GeneratedEnemyUnit(-9,20,84,19,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "5cb0d59fec8a8aab2c74588701b7cdadf5704166924509a622ef34d7e619aa15");
        }

        private static void Case_04393()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4393,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,2,53,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,8,51,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-3,67,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,7,32,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-16,89,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-9,46,25,4), new GeneratedEnemyUnit(3,10,5,21,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "27e4216e0faaa228fb1dda46f591d5dfb0b62cb237e980b15522e518578f4fa3");
        }

        private static void Case_04394()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4394,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-11,21,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-9,74,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,16,77,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,16,61,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,2,85,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-11,81,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-12,38,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,18,62,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,10,26,4,3), new GeneratedEnemyUnit(-2,10,58,32,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "89e63a59812f518658c7a8c45d461e895c298c8c2aec92d9b82ac3098b8bc205");
        }

        private static void Case_04395()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4395,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-2,91,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,4,54,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-3,89,26,1), new GeneratedEnemyUnit(-8,20,57,31,3), new GeneratedEnemyUnit(19,-3,36,31,4), new GeneratedEnemyUnit(2,11,81,47,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "639d7e8389ff93c9344850a16d56807d75f53eb1b2670845aa5fe29bafe1b530");
        }

        private static void Case_04396()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4396,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-17,97,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,16,85,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,0,5,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,14,96,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,18,18,28,3), new GeneratedEnemyUnit(-2,17,35,8,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "d7719ee30a4614218f3722a5545ba7876620f39b966cfd807f63d1e60999199a");
        }

        private static void Case_04397()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4397,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,7,72,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,18,76,10,4), new GeneratedEnemyUnit(-15,-2,81,34,4), new GeneratedEnemyUnit(14,17,27,46,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "8d08e2dbead07d7b6d8a3a9f2fe2b3f0ea6a668c4ed5d34804477f1284af3e49");
        }

        private static void Case_04398()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4398,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,10,42,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,8,31,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-18,76,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-13,42,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-4,70,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-13,79,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,8,38,4,4), new GeneratedEnemyUnit(-9,-7,86,11,2), new GeneratedEnemyUnit(-1,1,43,10,1), new GeneratedEnemyUnit(15,-20,33,50,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "265cb6f56f63d2a5ab45185b8a1d1a99310520ba907b1a57f6ce7c8d2e02251a");
        }

        private static void Case_04399()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4399,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-5,55,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,6,35,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,15,59,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,18,44,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,9,89,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,3,40,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,12,60,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-11,84,1,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "01f19f61d32b70830d013d0e328c87bb66ce079ea85c5b74e50e92331ee2224e");
        }

        private static void Case_04400()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4400,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-12,93,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-1,87,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-19,55,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,17,21,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-20,63,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-14,13,33,1), new GeneratedEnemyUnit(-10,20,29,4,1), new GeneratedEnemyUnit(-10,-16,49,6,3), new GeneratedEnemyUnit(5,11,35,48,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "aa9b94b01ac1ac87bc185303986ef705cefbf594e2611c4aeb5a97e2b2dfdcb6");
        }

        private static void Case_04401()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4401,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-19,57,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,7,99,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,15,25,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-7,14,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,2,55,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,19,22,22,4), new GeneratedEnemyUnit(13,2,44,5,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "822a917a1d0425afcc5ef73da1d77007672a3758c1a22abd10cd873947c6ba5e");
        }

        private static void Case_04402()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4402,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-20,88,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-5,68,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,12,21,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,6,87,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,4,99,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,20,87,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,2,71,14,3), new GeneratedEnemyUnit(18,20,76,3,4), new GeneratedEnemyUnit(-9,12,47,5,2), new GeneratedEnemyUnit(17,-9,47,26,3), new GeneratedEnemyUnit(-8,-6,65,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "6e6519a4a26ad81f5e021f142b586800f2b67092d86d0de7afbc0fe40abe8bd8");
        }

        private static void Case_04403()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4403,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,7,14,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-11,5,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,4,85,4,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "b9e795136f52472822d9e168ebaa1af35510c1cf636de63d70100ed17b834ba0");
        }

        private static void Case_04404()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4404,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-14,60,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,0,77,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-11,91,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-18,89,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-2,89,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-9,55,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,16,23,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,1,79,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-14,85,6,4), new GeneratedEnemyUnit(18,12,73,20,3), new GeneratedEnemyUnit(-1,8,29,7,4), new GeneratedEnemyUnit(5,-14,95,5,4), new GeneratedEnemyUnit(13,-9,62,24,4), new GeneratedEnemyUnit(2,-18,24,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "85b548d2b2876e5a31de18e39bc5bb8751b37ebfd3dcd7c4846872a4d377744f");
        }

        private static void Case_04405()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4405,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,13,46,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,17,76,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-7,53,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,15,33,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-9,75,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-8,27,34,3), new GeneratedEnemyUnit(-1,10,51,17,1), new GeneratedEnemyUnit(15,-3,27,45,1), new GeneratedEnemyUnit(-1,-9,93,35,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "8edece40d1a7f7dad5039797593b6bf9eb8797c942ad57e401d7683b983675df");
        }

        private static void Case_04406()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4406,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-12,62,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-15,25,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-19,37,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,7,15,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,9,96,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,5,37,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "e45594035c368c6b4f298847e2c66816fdee2a6b405a99261d9cd52fd6b6c225");
        }

        private static void Case_04407()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4407,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,6,47,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,8,35,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-15,37,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,3,19,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-9,32,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-20,87,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,0,84,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,6,26,1,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "548ddf86da4356bd5029f18463738ed39cd4f22408e76bef35dbfd385cd262d7");
        }

        private static void Case_04408()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4408,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,14,38,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,5,94,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,16,87,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,16,70,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-3,43,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,1,53,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-20,32,36,2), new GeneratedEnemyUnit(-5,9,49,32,1), new GeneratedEnemyUnit(-14,14,11,11,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "dba37012f6e76200ca283189e1d6c763e39c7b18086e95bc1ce0a69f7becaa50");
        }

        private static void Case_04409()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4409,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-12,61,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,1,95,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-7,18,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-15,45,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,13,99,42,1), new GeneratedEnemyUnit(-18,10,96,44,4), new GeneratedEnemyUnit(-16,14,48,36,4), new GeneratedEnemyUnit(2,-12,14,15,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "c2b9f76ab27826d71be7e2549ff1092dc8e464718720195eb8ea2d981d96275b");
        }

        private static void Case_04410()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4410,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-20,85,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-19,19,39,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "727a72d80a2654b8525ed6b2e1876c7baa8fd832a455ed6ed0805ac374acc323");
        }

        private static void Case_04411()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4411,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-9,87,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,6,90,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,14,61,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,16,95,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,9,69,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,4,10,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-15,43,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,7,77,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-15,27,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "dbfa06bd264d069a014af2deb7101f0fdb00ea78fc865807891dd52d17461297");
        }

        private static void Case_04412()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4412,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-8,12,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,0,60,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,1,14,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,5,95,50,3), new GeneratedEnemyUnit(-3,-10,13,45,1), new GeneratedEnemyUnit(-1,14,35,13,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "9470cfa4e31c3f7f92abf2ebac4b6b2ab34bc86bc3b779f4a6e4b5c198259b83");
        }

        private static void Case_04413()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4413,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,5,98,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,20,60,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-20,65,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-20,23,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-8,99,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,19,85,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-14,32,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-14,39,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,1,100,17,2), new GeneratedEnemyUnit(-1,-3,19,5,4), new GeneratedEnemyUnit(11,1,39,29,1), new GeneratedEnemyUnit(-16,-5,47,17,3), new GeneratedEnemyUnit(-12,7,81,13,1), new GeneratedEnemyUnit(-10,4,38,9,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "4cd3b8c1ca1d1eb452ab70f9274cb774fe18a778db3a3ad929d75b8b3f8cea1c");
        }

        private static void Case_04414()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4414,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-17,44,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-10,93,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-5,18,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-5,67,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,12,51,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,3,78,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-12,10,10,1), new GeneratedEnemyUnit(17,4,95,15,2), new GeneratedEnemyUnit(0,4,66,30,3), new GeneratedEnemyUnit(-9,3,8,21,3), new GeneratedEnemyUnit(16,-12,73,27,3), new GeneratedEnemyUnit(14,-17,61,15,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b3c38d0c0ce2fba82eaf0dd3c3404ed7c05f72710e8dcbf78c482752aad3a181");
        }

        private static void Case_04415()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4415,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,11,38,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-11,84,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-12,95,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-3,22,11,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "011ed7c9a03a08a4a105fb92a00da6939467f05cff521f3cb49246e0bc312922");
        }

        private static void Case_04416()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4416,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,0,7,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-20,50,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,17,82,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-14,92,3,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "28a15c93890f08c82258254f0317f46badac1a8493cd894a4811131f5e708110");
        }

        private static void Case_04417()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4417,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,0,34,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-13,38,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,8,18,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-16,81,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,2,25,4,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "eb8375b335be92c314c149f35fc1252ab178fed543687f661b5fda4cc087fa30");
        }

        private static void Case_04418()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4418,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,1,52,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,8,92,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,0,21,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,7,73,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-15,47,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,12,87,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,17,75,13,1), new GeneratedEnemyUnit(-3,19,67,36,4), new GeneratedEnemyUnit(-11,1,89,30,2), new GeneratedEnemyUnit(14,-4,87,43,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "a7c5e247f3f2deceeb038dc54ae3928729397d188b9ed98c3dc00091b5b14fb6");
        }

        private static void Case_04419()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4419,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-15,7,2,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "270072c41c9e7ebb9203e7b32fa11d0f10b48e50afc692329c175365128cdd9c");
        }

        private static void Case_04420()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4420,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,12,76,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-13,38,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-7,72,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-6,49,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-19,6,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-6,64,46,1), new GeneratedEnemyUnit(-7,0,56,13,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "8f39f2fbaeba0bad9a7de2785caa9b25872805c700b9cb9042f446109c76ff73");
        }

        private static void Case_04421()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4421,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,10,80,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,3,84,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,14,45,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-11,97,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,11,78,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-20,44,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-7,52,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-11,69,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-3,23,29,3), new GeneratedEnemyUnit(-5,11,14,12,4), new GeneratedEnemyUnit(1,-8,75,16,3), new GeneratedEnemyUnit(-10,-7,88,26,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "312662fd8abcfa193d699da811ea91bdc802eea27543d3a2555fa215e93d1ca6");
        }

        private static void Case_04422()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4422,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-4,79,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-2,22,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-15,12,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-6,15,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,8,70,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-2,58,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-17,25,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,6,47,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-14,66,33,2), new GeneratedEnemyUnit(-9,19,9,29,1), new GeneratedEnemyUnit(12,8,83,44,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "4c7054385f84bbcf6c625aa45baeea0daa52ae3a8f72f453dd93562f41ab43eb");
        }

        private static void Case_04423()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4423,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,1,84,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-12,65,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,9,82,45,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8d47713c74a8a9e928e58c9f2d77fcf7d5f21cf88a0979f95c1c876bcbc3270a");
        }

        private static void Case_04424()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4424,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,16,47,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-19,15,3,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "97bd99b263a151828b64824ea011db00e0009a78065ab7dc98319a81d748baf3");
        }

        private static void Case_04425()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4425,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,15,80,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,17,91,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-6,90,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,5,97,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,19,94,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-14,83,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-11,65,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,7,80,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-5,59,46,3), new GeneratedEnemyUnit(11,7,24,22,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "3e48fca1abebc0f7377bd4a35bf11c269aedffee1e3f67b53dfe9f19abdc918c");
        }

        private static void Case_04426()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4426,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-14,42,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-18,8,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,11,19,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,3,84,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-13,82,7,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "5a541c94044bb6b272619a5acb8afb5557f89479673900a3bc0ea9955ae53073");
        }

        private static void Case_04427()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4427,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-2,82,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,9,24,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,7,99,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-10,52,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-17,7,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,8,24,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,14,91,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,6,40,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,11,57,18,2), new GeneratedEnemyUnit(18,-16,61,17,1), new GeneratedEnemyUnit(9,-17,30,41,3), new GeneratedEnemyUnit(7,-6,47,8,1), new GeneratedEnemyUnit(-12,-16,34,30,2), new GeneratedEnemyUnit(-11,-19,75,23,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d30233ffce0303d2955b23e4dfdc406411f07866cde9efa8cab6eb3d18ef4407");
        }

        private static void Case_04428()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4428,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-2,78,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-6,42,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,13,96,11,1), new GeneratedEnemyUnit(-20,-6,22,40,3), new GeneratedEnemyUnit(7,-12,68,17,4), new GeneratedEnemyUnit(-20,-3,59,14,3), new GeneratedEnemyUnit(14,3,93,45,3), new GeneratedEnemyUnit(16,-12,37,30,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "7e6a312c19b25daa7df19f430f004aeb88dd4430a4b0a1d7a5da9b680eeb9165");
        }

        private static void Case_04429()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4429,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-13,93,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-19,35,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,5,47,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-3,71,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-1,60,20,1), new GeneratedEnemyUnit(-3,-12,79,44,4), new GeneratedEnemyUnit(6,-14,78,24,1), new GeneratedEnemyUnit(-11,-16,42,30,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "1459c9029d7cae0263def221075cacc9a4381b14ef4e8bda7ac4ec909d868a60");
        }

        private static void Case_04430()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4430,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,11,53,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-2,48,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,20,28,2,3), new GeneratedEnemyUnit(-2,-2,21,27,3), new GeneratedEnemyUnit(-7,-5,48,22,2), new GeneratedEnemyUnit(4,2,12,9,1), new GeneratedEnemyUnit(12,-4,13,5,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "5b49faeadfbfcb7e63df42a5212421fc40626aeccdf718530682e86c67c8521a");
        }

        private static void Case_04431()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4431,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-15,89,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-16,62,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,19,16,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,15,20,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-3,50,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,15,73,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,14,49,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,14,59,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-3,38,19,4), new GeneratedEnemyUnit(15,-2,78,19,1), new GeneratedEnemyUnit(0,12,22,1,1), new GeneratedEnemyUnit(-12,-7,100,3,1), new GeneratedEnemyUnit(11,-20,37,18,3), new GeneratedEnemyUnit(-9,-7,76,11,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "58c53e952881c87e92c6f5fce9e83f2abff0013eed6c5afd742c2252c7cc69f2");
        }

        private static void Case_04432()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4432,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,20,20,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,17,33,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-13,78,42,3), new GeneratedEnemyUnit(-9,13,16,46,2), new GeneratedEnemyUnit(-19,-19,21,49,3), new GeneratedEnemyUnit(0,2,28,43,4), new GeneratedEnemyUnit(18,16,50,50,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "04bfa1a831453dfe637c7ad843a0051e0781fb277568859ed64933ed34556384");
        }

        private static void Case_04433()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4433,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,4,5,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-8,86,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-4,96,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-3,48,40,3), new GeneratedEnemyUnit(16,-1,37,22,3), new GeneratedEnemyUnit(8,-19,76,36,3), new GeneratedEnemyUnit(-9,13,18,3,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "a4bc03f2d7b8e212b3172167a1c34da547397ef9c45ad7fca95892e6b3ae0213");
        }

        private static void Case_04434()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4434,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,15,67,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-20,27,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-7,55,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-16,39,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-7,59,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-8,18,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,16,99,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,5,23,38,1), new GeneratedEnemyUnit(-1,10,11,7,2), new GeneratedEnemyUnit(-14,-20,82,17,2), new GeneratedEnemyUnit(-11,-1,62,7,2), new GeneratedEnemyUnit(-3,16,69,49,3), new GeneratedEnemyUnit(3,-17,55,44,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "d9cf7e307277e3a182b256974e67fb90789a60eaacd3d776ca47e660159432cd");
        }

        private static void Case_04435()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4435,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-18,85,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,18,85,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-20,18,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,18,83,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-19,10,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-4,10,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,19,17,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,17,20,1,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "4e8c86d360342d88f04dcbbff4b8238a406b738ec990f71ed367eb041dbdb27d");
        }

        private static void Case_04436()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4436,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,19,48,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,16,89,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-8,19,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,5,47,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "604f2092f998fae293e2c95e4e52bb2eac72d2fa7f87fb82017fc5734a8224a0");
        }

        private static void Case_04437()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4437,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-2,55,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-2,80,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-19,28,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-6,59,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-20,24,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-2,51,8,3), new GeneratedEnemyUnit(-5,-18,72,22,1), new GeneratedEnemyUnit(-4,-9,9,26,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "a94afc9b03c707a5d2abde2e1c79a9c441b5b7932485537ec2122edd57ad616c");
        }

        private static void Case_04438()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4438,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-19,75,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-1,63,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,14,89,6,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "9a112f5d21df021ef501621b5d04109950bf34154e66fa14506d5bf4d628f60b");
        }

        private static void Case_04439()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4439,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,8,99,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,4,58,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-7,55,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-9,85,10,2), new GeneratedEnemyUnit(19,16,10,6,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "fd719daec339dcb23380229f69eedee425e684cf937bfe9fa20d74a24f394c15");
        }

        private static void Case_04440()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4440,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,12,61,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-3,38,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,2,18,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,7,34,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,18,67,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,0,91,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-19,16,1,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "ec008863a7ec59ef949b46770d3fd92bad2cc1c6cabd7f659e0857210d26fde3");
        }

        private static void Case_04441()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4441,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-3,33,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,7,65,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,19,36,45,1), new GeneratedEnemyUnit(12,18,65,19,2), new GeneratedEnemyUnit(16,19,90,40,1), new GeneratedEnemyUnit(-18,-10,22,6,1), new GeneratedEnemyUnit(6,-2,32,11,1), new GeneratedEnemyUnit(20,-18,26,2,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "b083847c834397f7f9e8f394b6e993ccec77e6a6cc708e3316422bc4bcc69b62");
        }

        private static void Case_04442()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4442,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-15,27,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-2,85,4,2), new GeneratedEnemyUnit(-4,14,72,16,3), new GeneratedEnemyUnit(9,-12,75,34,3), new GeneratedEnemyUnit(3,-12,58,49,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "b225154b768636a6cb02f789a02d909caf3d5b94becb949229dbfc01f2fb36a2");
        }

        private static void Case_04443()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4443,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-9,37,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-6,91,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-6,51,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-1,29,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,17,38,16,1), new GeneratedEnemyUnit(16,-3,64,50,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "316f0aff267e1009492861f503685a97f41d79101e9347a746df407179ae808a");
        }

        private static void Case_04444()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4444,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,3,96,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,3,82,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,14,76,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-19,8,10,1), new GeneratedEnemyUnit(3,20,94,24,3), new GeneratedEnemyUnit(20,1,39,35,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "dd5177fa2fcc89b397f6079320161bd1cf24430dd2a3b4e47ed82115bb1d363d");
        }

        private static void Case_04445()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4445,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-2,36,4,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "6b44b9e9c6a1a01dfbddca02b152b44f108f63d4618509010c9fb830b32c0a1b");
        }

        private static void Case_04446()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4446,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-20,69,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,5,63,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,13,53,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,17,81,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,2,32,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,14,60,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-16,50,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,7,33,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,18,8,11,4), new GeneratedEnemyUnit(11,-19,9,22,4), new GeneratedEnemyUnit(13,20,52,25,1), new GeneratedEnemyUnit(2,2,79,41,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "b0107cf06a518d203f7cf4580eca2a575a6dfa9d7e4a1c2cb6ae677d55d05627");
        }

        private static void Case_04447()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4447,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-18,88,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,13,7,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,11,55,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,12,95,7,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "1f7785541cdc38bb47e8412a5376774a4b52e9eea9957197ff65751b5040d230");
        }

        private static void Case_04448()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4448,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,2,36,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-20,95,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-10,80,50,2), new GeneratedEnemyUnit(-10,-4,99,29,1), new GeneratedEnemyUnit(-9,-4,22,26,3), new GeneratedEnemyUnit(-17,11,40,13,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "d652a3f2ccc2e4beb6dfaa31cb6ee20731b15147954916ff22753f036facc111");
        }

        private static void Case_04449()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4449,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-20,22,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,4,21,1,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "7a2ed63ffa3ee786491aee1776b7f687f9978e711e341f2eefdcc12d8d2375d9");
        }

        private static void Case_04450()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4450,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,9,57,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-5,56,1,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "5379681dd86c3540cfd664a93727a7746d26ea5e976afc98a2a573fa31de34c9");
        }

        private static void Case_04451()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4451,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,8,10,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-9,25,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-19,100,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-13,85,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-19,77,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-17,10,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,14,60,8,1), new GeneratedEnemyUnit(-11,-18,27,26,4), new GeneratedEnemyUnit(-16,19,8,12,1), new GeneratedEnemyUnit(1,-1,86,27,2), new GeneratedEnemyUnit(-14,-6,75,50,2), new GeneratedEnemyUnit(19,-8,21,47,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "2fd7b4702859add77c3a76cf9ac9cc226ab5eb8acb86e0f2089c8db9d81b6460");
        }

        private static void Case_04452()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4452,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,12,35,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-20,27,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,8,51,23,4), new GeneratedEnemyUnit(-4,-4,81,30,3), new GeneratedEnemyUnit(-4,-8,57,15,4), new GeneratedEnemyUnit(6,-9,74,10,3), new GeneratedEnemyUnit(-5,-5,23,37,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "a257c97797b9820f8916b01980b49c29172a1e1130b00b521be42732aeffadde");
        }

        private static void Case_04453()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4453,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,7,77,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-1,80,3,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "ee5f3b00e536795ae6a65841381681cfd7c5a238dd3669f7d34ac28c25d94b15");
        }

        private static void Case_04454()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4454,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-2,73,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-18,47,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,11,47,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,3,69,43,3), new GeneratedEnemyUnit(9,5,93,20,1), new GeneratedEnemyUnit(-10,-17,40,40,1), new GeneratedEnemyUnit(18,-16,64,45,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "c72a633b8ff4b72d9f7fd9f6813f32653648e082f1a131ff53bc9011012fd6b4");
        }

        private static void Case_04455()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4455,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,0,73,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-19,61,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-18,65,17,4), new GeneratedEnemyUnit(-20,1,42,18,3), new GeneratedEnemyUnit(-10,-20,62,34,4), new GeneratedEnemyUnit(-7,13,61,19,1), new GeneratedEnemyUnit(-19,12,52,6,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "2acd2b04231330b16a9f14a5159a311c40adab27be4e7735c705f17de7cd07ca");
        }

        private static void Case_04456()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4456,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-4,66,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-13,15,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-8,21,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,4,63,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,18,71,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,13,95,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-9,64,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-10,100,38,4), new GeneratedEnemyUnit(0,8,59,33,1), new GeneratedEnemyUnit(3,3,25,15,2), new GeneratedEnemyUnit(-12,-8,51,19,1), new GeneratedEnemyUnit(17,-20,20,37,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "d76158101e13724cb439b4393e9a3d31a7f65aa4e53e2a6bf8c15dc6fbaf5d55");
        }

        private static void Case_04457()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4457,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-18,70,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,9,73,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,18,63,40,4), new GeneratedEnemyUnit(-16,9,40,10,3), new GeneratedEnemyUnit(-12,19,37,20,1), new GeneratedEnemyUnit(4,18,75,13,3), new GeneratedEnemyUnit(16,-8,51,30,1), new GeneratedEnemyUnit(-2,4,58,4,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "b3c60c7be4b9c65c3cea75b146a0c2d994905eed4a3645fdd5091b2389818ff0");
        }

        private static void Case_04458()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4458,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,12,55,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-20,22,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-5,78,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-19,42,37,2), new GeneratedEnemyUnit(13,-16,95,37,1), new GeneratedEnemyUnit(-9,-1,86,48,2), new GeneratedEnemyUnit(16,-2,97,11,4), new GeneratedEnemyUnit(-4,5,47,49,3), new GeneratedEnemyUnit(-2,-6,20,48,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "df8b15631929838e7e2c9d20858b74682d1f058db6c023b5f900f5c2165bd14c");
        }

        private static void Case_04459()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4459,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-7,11,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-4,39,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,9,60,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-18,20,4,2), new GeneratedEnemyUnit(-7,-3,90,17,3), new GeneratedEnemyUnit(16,15,57,47,2), new GeneratedEnemyUnit(-16,-15,20,7,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "9c564a5e1f0e2ad3abc74fdedf56477ce8c394158f9edb158aeb6fc136eec8f4");
        }

        private static void Case_04460()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4460,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-18,37,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,13,82,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-14,75,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-14,16,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,1,82,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-6,78,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-11,48,28,4), new GeneratedEnemyUnit(5,-18,36,11,3), new GeneratedEnemyUnit(11,7,27,38,4), new GeneratedEnemyUnit(5,10,38,21,1), new GeneratedEnemyUnit(-13,7,14,2,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "db7e84c428aa307bd4b9ea5e294235406f9e9b63817e47a1601ab9b69aec114f");
        }

        private static void Case_04461()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4461,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,3,66,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,9,7,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,16,89,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,18,53,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-2,46,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-20,46,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,14,40,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-15,59,39,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2d4b8715b0b5dc2a67b068f8054b2b346410ff4cfe09a7a429dbfb959f955404");
        }

        private static void Case_04462()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4462,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,3,33,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,17,23,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,13,75,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-1,71,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-18,58,37,4), new GeneratedEnemyUnit(4,15,50,40,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "ec581996005b1a63b4ea8d23b5b344fc22312eb60f59275d93a64f63232cec6e");
        }

        private static void Case_04463()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4463,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,2,52,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,6,70,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,13,28,23,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "d8d28a253305304a208c004e660dbd8ec6739af1cdc0c0ce76687250bad6be1e");
        }

        private static void Case_04464()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4464,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-20,65,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,17,75,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,20,78,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-9,14,32,4), new GeneratedEnemyUnit(5,-15,25,18,1), new GeneratedEnemyUnit(-14,-1,13,5,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "6d0662817b3c0cafd9dd0daca5a43e72f689887144b8ec1e02c77dd961d8b1f2");
        }

        private static void Case_04465()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4465,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,18,9,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-8,80,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-11,86,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,3,8,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,3,71,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-7,57,5,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "7eae7ba7519a2a770b7ba87f18ab9f0ceab7d140f10b8f9e686ebdcc7898bfe4");
        }

        private static void Case_04466()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4466,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-16,60,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-10,84,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,6,98,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-15,95,35,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "3ff874158078c6091ab3dae670580411f8ed182356261a7dcd4a79c470f41180");
        }

        private static void Case_04467()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4467,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,6,11,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-6,9,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,19,90,10,2), new GeneratedEnemyUnit(6,10,83,22,4), new GeneratedEnemyUnit(8,5,95,6,2), new GeneratedEnemyUnit(5,-13,15,44,1), new GeneratedEnemyUnit(-6,3,34,10,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "f89a3b9142ceb5d06c097d3854f5f09363557bf98dd543336abd3c32431fba3c");
        }

        private static void Case_04468()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4468,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,18,53,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,4,93,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-12,65,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,1,62,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,16,49,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,7,48,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,18,48,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,4,52,2,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "58e88d0048d5b781205f9b4c1f631732ac7385ddf4add6f8acd329537df1b8ef");
        }

        private static void Case_04469()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4469,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,12,13,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-10,37,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,8,76,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,16,75,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-16,90,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-3,87,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-11,19,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,19,70,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-9,61,36,4), new GeneratedEnemyUnit(-18,-5,17,18,2), new GeneratedEnemyUnit(-7,10,56,49,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "165a2c6342d26269450c3f0c244deb8f6a9f8b15c9911ef6d146151daf7804cb");
        }

        private static void Case_04470()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4470,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-15,43,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,4,50,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-18,71,36,3), new GeneratedEnemyUnit(15,-3,67,17,4), new GeneratedEnemyUnit(-6,-11,53,15,1), new GeneratedEnemyUnit(4,12,43,14,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "d8ebc99d543ce020bee5f38e25c026c17f23277176982ab6c0d28d3231588e73");
        }

        private static void Case_04471()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4471,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-12,64,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,7,97,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-16,33,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-13,30,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-7,17,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-7,56,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,9,22,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,13,78,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-17,28,18,1), new GeneratedEnemyUnit(0,-2,9,32,4), new GeneratedEnemyUnit(-13,19,17,31,1), new GeneratedEnemyUnit(-20,6,92,7,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "cb5ebeb2f592e9c0e2ff9fdfaa6a17eb109af344b512786cc6f9c9e34bef0efd");
        }

        private static void Case_04472()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4472,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,1,82,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,13,32,7,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "976ba9c754a60b02f35de51557c946a1fb3a36200eea4e8e920e9920965b9094");
        }

        private static void Case_04473()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4473,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,2,60,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-19,43,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-7,35,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-9,100,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-18,54,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,18,94,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,6,57,49,2), new GeneratedEnemyUnit(-9,13,60,43,3), new GeneratedEnemyUnit(4,9,8,48,3), new GeneratedEnemyUnit(-6,15,74,18,3), new GeneratedEnemyUnit(8,3,59,46,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "2b8422a138ad6f1b7f9f508e47da3b9b936ff8bbc5c10cf654d23e019f476b73");
        }

        private static void Case_04474()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4474,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,3,51,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-2,77,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,16,31,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,14,43,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-6,42,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-12,50,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,9,16,9,2), new GeneratedEnemyUnit(15,11,61,18,2), new GeneratedEnemyUnit(16,-16,88,50,1), new GeneratedEnemyUnit(13,-1,85,3,1), new GeneratedEnemyUnit(-18,13,71,32,4), new GeneratedEnemyUnit(-16,1,12,38,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "70d8334aacc01ff81c54bdf07acf3830650f7c91ceac18203914a1f9ae401c25");
        }

        private static void Case_04475()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4475,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,2,76,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-5,73,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,3,90,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,2,88,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,13,65,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,10,88,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-8,46,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-5,79,12,3), new GeneratedEnemyUnit(-15,-5,11,34,3), new GeneratedEnemyUnit(20,19,22,26,2), new GeneratedEnemyUnit(19,7,56,39,1), new GeneratedEnemyUnit(12,0,33,31,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "7ffb2a2490b4a261f9448ce3bbaa40425a1cdde5a8d9b04613695e47936bd266");
        }

        private static void Case_04476()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4476,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-17,91,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,15,13,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-15,49,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,13,80,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,8,16,29,1), new GeneratedEnemyUnit(-10,-18,29,10,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8161092b452e063e1586387ccf5c6ae2ee2e2f345e1dff794994a8ad699560cd");
        }

        private static void Case_04477()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4477,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,10,54,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-7,44,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,11,41,5,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "745c897d5444a463941450bd7e9ca579a9897327140be3c522967da9c12ce5b6");
        }

        private static void Case_04478()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4478,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,18,93,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,7,29,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-5,54,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-6,96,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-18,71,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,7,45,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,18,7,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-9,93,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,17,50,26,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "1adb903985d2707b62de686c6bfd03f3345fe9e768a5d480714f26e624c2891a");
        }

        private static void Case_04479()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4479,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,2,7,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,20,40,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-14,61,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,16,26,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,0,51,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-16,53,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-15,79,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-19,55,40,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "5df0fbde139b96c23bb780d0f3f41b0443a3457abdd7e116ba4869ce196ece3e");
        }

        private static void Case_04480()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4480,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,3,96,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,15,41,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,15,35,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-12,30,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-13,20,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,14,70,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,4,84,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-20,57,6,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "757d094fdcc89ec0e29e8661afcf5649a4e2e6c54956a549d507fe85b026ac1f");
        }

        private static void Case_04481()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4481,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-11,5,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,14,48,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-1,68,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,7,20,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-18,20,1,2), new GeneratedEnemyUnit(-3,-6,53,3,3), new GeneratedEnemyUnit(0,8,48,41,2), new GeneratedEnemyUnit(-14,14,49,6,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "a6a637272c7dc25475e84065166b1be362350a1af6a07015708036a357c20873");
        }

        private static void Case_04482()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4482,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-16,76,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,2,16,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,10,18,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,20,84,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,10,91,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,18,63,1,4), new GeneratedEnemyUnit(-16,7,80,29,3), new GeneratedEnemyUnit(-9,-13,21,15,1), new GeneratedEnemyUnit(2,11,74,46,2), new GeneratedEnemyUnit(-18,-18,7,6,3), new GeneratedEnemyUnit(-3,8,44,24,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "b1c26ddf92634d7d9872ecc4550e83a54dafa342317ae6c24428a6d6d670209d");
        }

        private static void Case_04483()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4483,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,16,12,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,10,24,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-5,81,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,10,76,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,3,5,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-2,64,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-4,31,49,1), new GeneratedEnemyUnit(-15,-13,72,35,2), new GeneratedEnemyUnit(-1,9,96,17,2), new GeneratedEnemyUnit(15,16,27,40,1), new GeneratedEnemyUnit(-15,-8,52,27,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "36f644f8a154e46dc470e891a1e74a15921c3b9d84b6a9b759001a2007caad8c");
        }

        private static void Case_04484()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4484,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-7,29,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-20,99,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,12,57,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,3,39,10,4), new GeneratedEnemyUnit(-8,11,56,46,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "5e13af0c29b2c4cf4e395fa2ac3b112e5561e1f8c75aa006577e6eb4ec43f89a");
        }

        private static void Case_04485()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4485,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,16,50,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-14,5,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-12,66,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,20,35,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-9,65,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,5,7,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,2,23,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-19,20,40,4), new GeneratedEnemyUnit(-19,9,44,42,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "dbbb3014c0e17231496d58dde2b3669a040fd638e2009d48f50b111fdfe7efb7");
        }

        private static void Case_04486()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4486,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-9,9,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,8,37,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-14,63,22,3), new GeneratedEnemyUnit(-6,8,42,12,1), new GeneratedEnemyUnit(8,9,14,44,4), new GeneratedEnemyUnit(14,14,13,46,4), new GeneratedEnemyUnit(11,-15,14,45,4), new GeneratedEnemyUnit(-19,-16,91,17,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "8bcaccf73583dae5100a4240c33ba1aa3145800eaa3537d5d9a35e32fc3260ce");
        }

        private static void Case_04487()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4487,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,12,51,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,20,72,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,3,100,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-16,34,3,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "e5c25e806c1ab4f6dacd93eec46caa1da5c2b3da6557f7897177838bde804cbf");
        }

        private static void Case_04488()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4488,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,5,74,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,10,59,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,2,74,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,3,47,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,20,45,43,4), new GeneratedEnemyUnit(17,4,78,42,1), new GeneratedEnemyUnit(-8,-19,23,33,3), new GeneratedEnemyUnit(-2,19,43,14,1), new GeneratedEnemyUnit(11,10,98,28,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "4f1c56acca39f2aa8fe6ee09e8d2d45c67c318d875f3abb17500d8dbaa20f438");
        }

        private static void Case_04489()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4489,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-14,66,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-9,34,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,17,15,44,3), new GeneratedEnemyUnit(14,19,21,31,2), new GeneratedEnemyUnit(18,-12,64,22,3), new GeneratedEnemyUnit(3,20,8,45,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "7d6291d8f45119297f873e4fc6360e3be7853e5f65c13f19dc7a8a5bbdbb2589");
        }

        private static void Case_04490()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4490,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-5,88,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-20,21,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,11,95,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,19,92,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-14,32,33,3), new GeneratedEnemyUnit(-12,17,13,34,3), new GeneratedEnemyUnit(-17,4,23,14,1), new GeneratedEnemyUnit(5,20,13,15,4), new GeneratedEnemyUnit(8,-20,31,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "e1224a3a449c3f69792c1d3c0372944524ba36126e7da7b5acbc5f88f46281c6");
        }

        private static void Case_04491()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4491,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,2,12,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-8,8,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,2,85,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,1,69,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-16,59,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,2,95,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,5,98,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,18,76,13,2), new GeneratedEnemyUnit(18,18,29,12,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "8f76b314d61a48b2685241a2edddc70aad8572c61ead9320a86d914137f83deb");
        }

        private static void Case_04492()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4492,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,14,66,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,11,33,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,1,70,6,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "f8d78ee697fcdd3fdc120300cc0db7ca621c3738498a33e6ed0605f4e85842bc");
        }

        private static void Case_04493()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4493,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,15,95,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,17,23,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-1,7,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,4,90,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-14,74,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-8,8,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,8,95,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,19,22,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-14,46,49,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "a29b788ac12c4ee693ce9a2bd33a640cb15f0b853cc934b29c40933f166f492b");
        }

        private static void Case_04494()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4494,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,20,92,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,9,32,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-4,53,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,13,80,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,9,60,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,13,70,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-6,31,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-2,81,21,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "3e8020adfb89bbaf26452259e15031ab1600e4e92d06033e03e7ad6eccb2c312");
        }

        private static void Case_04495()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4495,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-7,53,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,10,61,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-2,47,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-1,83,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,20,15,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,20,78,20,3), new GeneratedEnemyUnit(3,7,80,48,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "29a1e0c3e86f510dadb9334cf4704dddd08dc073d595c408117350a2fe8710be");
        }

        private static void Case_04496()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4496,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-8,93,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-12,12,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-4,77,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-19,14,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,0,60,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-8,38,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,6,83,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,5,47,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-5,16,23,1), new GeneratedEnemyUnit(-6,-10,77,2,4), new GeneratedEnemyUnit(7,14,35,40,3), new GeneratedEnemyUnit(-19,20,5,13,2), new GeneratedEnemyUnit(12,-19,6,38,1), new GeneratedEnemyUnit(-3,-7,100,12,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "a2406994efb7ff81dc1f4bb70d8b38ecdbe98d71f702be4cd35bd889207e9a96");
        }

        private static void Case_04497()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4497,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,16,91,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-19,76,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,7,79,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,15,56,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,14,55,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-4,47,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,7,56,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,18,73,17,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "5000de789fdaf17df0259e391a351c28249ce1732110f0260bd793b8ffd85d77");
        }

        private static void Case_04498()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4498,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-20,22,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,13,42,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-7,33,33,1), new GeneratedEnemyUnit(-3,17,38,10,1), new GeneratedEnemyUnit(6,0,41,2,1), new GeneratedEnemyUnit(2,20,10,24,1), new GeneratedEnemyUnit(-19,14,5,27,1), new GeneratedEnemyUnit(-19,-16,21,48,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "c0669a2d3591c6113575f12ea24c57e4b7339bb0d57a7f916c97a6ac982665ed");
        }

        private static void Case_04499()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4499,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-3,34,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-10,62,4,2), new GeneratedEnemyUnit(12,11,75,21,1), new GeneratedEnemyUnit(-17,6,71,34,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "028de8876c1bc2439beac5bdab5d82e28bc8cf262954d75a447a2f7cc8b2ac6b");
        }

    }
}
