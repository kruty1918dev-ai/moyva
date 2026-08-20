using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard002
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_00360();
            Case_00361();
            Case_00362();
            Case_00363();
            Case_00364();
            Case_00365();
            Case_00366();
            Case_00367();
            Case_00368();
            Case_00369();
            Case_00370();
            Case_00371();
            Case_00372();
            Case_00373();
            Case_00374();
            Case_00375();
            Case_00376();
            Case_00377();
            Case_00378();
            Case_00379();
            Case_00380();
            Case_00381();
            Case_00382();
            Case_00383();
            Case_00384();
            Case_00385();
            Case_00386();
            Case_00387();
            Case_00388();
            Case_00389();
            Case_00390();
            Case_00391();
            Case_00392();
            Case_00393();
            Case_00394();
            Case_00395();
            Case_00396();
            Case_00397();
            Case_00398();
            Case_00399();
            Case_00400();
            Case_00401();
            Case_00402();
            Case_00403();
            Case_00404();
            Case_00405();
            Case_00406();
            Case_00407();
            Case_00408();
            Case_00409();
            Case_00410();
            Case_00411();
            Case_00412();
            Case_00413();
            Case_00414();
            Case_00415();
            Case_00416();
            Case_00417();
            Case_00418();
            Case_00419();
            Case_00420();
            Case_00421();
            Case_00422();
            Case_00423();
            Case_00424();
            Case_00425();
            Case_00426();
            Case_00427();
            Case_00428();
            Case_00429();
            Case_00430();
            Case_00431();
            Case_00432();
            Case_00433();
            Case_00434();
            Case_00435();
            Case_00436();
            Case_00437();
            Case_00438();
            Case_00439();
            Case_00440();
            Case_00441();
            Case_00442();
            Case_00443();
            Case_00444();
            Case_00445();
            Case_00446();
            Case_00447();
            Case_00448();
            Case_00449();
            Case_00450();
            Case_00451();
            Case_00452();
            Case_00453();
            Case_00454();
            Case_00455();
            Case_00456();
            Case_00457();
            Case_00458();
            Case_00459();
            Case_00460();
            Case_00461();
            Case_00462();
            Case_00463();
            Case_00464();
            Case_00465();
            Case_00466();
            Case_00467();
            Case_00468();
            Case_00469();
            Case_00470();
            Case_00471();
            Case_00472();
            Case_00473();
            Case_00474();
            Case_00475();
            Case_00476();
            Case_00477();
            Case_00478();
            Case_00479();
            Case_00480();
            Case_00481();
            Case_00482();
            Case_00483();
            Case_00484();
            Case_00485();
            Case_00486();
            Case_00487();
            Case_00488();
            Case_00489();
            Case_00490();
            Case_00491();
            Case_00492();
            Case_00493();
            Case_00494();
            Case_00495();
            Case_00496();
            Case_00497();
            Case_00498();
            Case_00499();
            Case_00500();
            Case_00501();
            Case_00502();
            Case_00503();
            Case_00504();
            Case_00505();
            Case_00506();
            Case_00507();
            Case_00508();
            Case_00509();
            Case_00510();
            Case_00511();
            Case_00512();
            Case_00513();
            Case_00514();
            Case_00515();
            Case_00516();
            Case_00517();
            Case_00518();
            Case_00519();
            Case_00520();
            Case_00521();
            Case_00522();
            Case_00523();
            Case_00524();
            Case_00525();
            Case_00526();
            Case_00527();
            Case_00528();
            Case_00529();
            Case_00530();
            Case_00531();
            Case_00532();
            Case_00533();
            Case_00534();
            Case_00535();
            Case_00536();
            Case_00537();
            Case_00538();
            Case_00539();
        }

        private static void Case_00360()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 360,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-7,47,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,5,71,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,13,29,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-9,87,39,1), new GeneratedEnemyUnit(-2,-20,47,15,1), new GeneratedEnemyUnit(3,-7,91,44,1), new GeneratedEnemyUnit(19,-13,41,31,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "5518e3ad78d088d47f1f4d41621eeedf10ac2ccb23617389ac7b8441116c8f76");
        }

        private static void Case_00361()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 361,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-12,45,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,0,24,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,16,88,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,0,51,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,9,90,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,13,94,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,1,61,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,9,12,19,1), new GeneratedEnemyUnit(-15,-3,18,30,4), new GeneratedEnemyUnit(12,20,26,2,3), new GeneratedEnemyUnit(2,16,67,8,4), new GeneratedEnemyUnit(2,-20,8,38,3), new GeneratedEnemyUnit(0,-17,37,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "65cd368c19c28cebded217fda503235187e928a9edb885841988859bff3164a3");
        }

        private static void Case_00362()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 362,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-15,15,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,3,77,5,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "4abcf377c85d4ca72e6000c6f6157e751e6dd9082e7626c9ac53a0db0a351e21");
        }

        private static void Case_00363()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 363,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,5,100,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-20,44,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-17,57,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-9,6,14,4), new GeneratedEnemyUnit(4,-20,21,24,4), new GeneratedEnemyUnit(-12,16,96,4,2), new GeneratedEnemyUnit(14,17,100,35,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "08cd8194729457c4c1e10ba3f20b8db0a452f2f974ffe0d66760fcad01578e66");
        }

        private static void Case_00364()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 364,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,19,53,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,20,56,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,2,34,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,1,86,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-13,18,6,4), new GeneratedEnemyUnit(-13,5,87,26,3), new GeneratedEnemyUnit(-7,2,99,14,4), new GeneratedEnemyUnit(-11,16,24,15,1), new GeneratedEnemyUnit(14,-13,70,48,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "1f01ff8f8c8064423c940f97aff69c87a8a9839ea6d43d399c451c835e78705c");
        }

        private static void Case_00365()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 365,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-10,59,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-17,47,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-1,50,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-10,98,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-19,68,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,7,15,9,1), new GeneratedEnemyUnit(-13,-17,79,21,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "09574f57b3d5e16858f9f3247886086308bd6420bac268e00049d9d7b63ab119");
        }

        private static void Case_00366()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 366,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,3,21,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,12,11,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,18,42,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,10,83,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-12,70,49,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1ac2a5e4f78ff4611dbe0b08d7f220e8d774fa2c72eed545cbdf1eb62beea5a9");
        }

        private static void Case_00367()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 367,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-16,85,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,18,42,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-10,95,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-8,55,7,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "938f8f6d3181ce41ec0933e2cf68d2d6f17b68587f1d9c4ad8926a74c4763aa5");
        }

        private static void Case_00368()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 368,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-9,65,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,5,53,7,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "06daf7ca689bfdbeb66443492b226b6381c211a90eb141aa6c8cf6643fcaef36");
        }

        private static void Case_00369()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 369,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-14,83,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,18,85,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,2,6,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-5,49,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,10,55,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,0,64,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-14,72,35,3), new GeneratedEnemyUnit(3,20,16,3,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "4e1dde2d2f37fe57276f88a109487d67b0f82c2751ef5f2d9fbc5f47426958b4");
        }

        private static void Case_00370()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 370,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,13,8,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-13,28,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-10,84,24,1), new GeneratedEnemyUnit(18,17,58,11,1), new GeneratedEnemyUnit(6,16,77,30,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "8b26be40f1879d1459a7d50438865f839e61749afdc441f77d40c4ad889f392a");
        }

        private static void Case_00371()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 371,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,5,25,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-13,33,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-7,99,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-12,91,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-10,97,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-3,90,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-12,91,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-7,49,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,6,59,36,1), new GeneratedEnemyUnit(-15,18,83,25,3), new GeneratedEnemyUnit(10,8,42,9,2), new GeneratedEnemyUnit(0,-8,68,14,1), new GeneratedEnemyUnit(18,9,10,22,3), new GeneratedEnemyUnit(-7,0,22,1,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "d46a0e7f76317769df6a0cd65ede0e6f3481b00efa1a9e7fff3ca90cd9cc6786");
        }

        private static void Case_00372()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 372,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,16,42,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,5,39,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,10,84,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-12,28,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-6,77,5,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "fbc72aaa45709eb91935a284dd42ec2200c8c812c5657590229ae13ec02b18a9");
        }

        private static void Case_00373()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 373,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-18,65,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,5,72,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-2,47,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-20,15,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-8,70,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-17,60,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-12,60,1,3), new GeneratedEnemyUnit(12,-15,88,14,1), new GeneratedEnemyUnit(18,11,34,12,4), new GeneratedEnemyUnit(3,-9,5,45,2), new GeneratedEnemyUnit(2,18,52,13,4), new GeneratedEnemyUnit(-11,15,68,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "9d016e9b11643d069cbc8725a2ffa359c05c4bb44cbd24c040c67dbfd6751428");
        }

        private static void Case_00374()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 374,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,5,64,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-5,90,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,10,73,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-9,51,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-7,10,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,12,91,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,11,42,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-4,73,45,4), new GeneratedEnemyUnit(-9,-9,22,6,3), new GeneratedEnemyUnit(14,3,52,8,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "d55e13503bef2c020f253dfe4489ab70cd5123da36811631216ceb67469cb64f");
        }

        private static void Case_00375()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 375,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-19,64,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,14,10,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-12,86,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,4,65,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,17,7,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,8,20,13,4), new GeneratedEnemyUnit(-14,7,9,22,1), new GeneratedEnemyUnit(-16,-7,64,6,3), new GeneratedEnemyUnit(-12,1,14,17,4), new GeneratedEnemyUnit(-8,6,92,47,3), new GeneratedEnemyUnit(15,-9,21,47,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "210f82e081d268eaf7a7b8037d32eb1725c1868c71188d47b59b6c4e48c98c80");
        }

        private static void Case_00376()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 376,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-11,7,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,7,42,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,6,97,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,16,58,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,5,39,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,13,74,17,1), new GeneratedEnemyUnit(-4,3,15,30,2), new GeneratedEnemyUnit(2,-14,63,48,2), new GeneratedEnemyUnit(-13,5,47,30,2), new GeneratedEnemyUnit(-18,18,21,37,1), new GeneratedEnemyUnit(-17,-4,93,34,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "f0dda73a28fba8319d27ed31f538a1a67b25420b9537ed5469dd381d2b047f1b");
        }

        private static void Case_00377()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 377,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-15,54,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-3,37,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,11,36,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,6,19,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,8,83,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-17,33,50,2), new GeneratedEnemyUnit(8,-16,40,23,2), new GeneratedEnemyUnit(10,-8,26,48,3), new GeneratedEnemyUnit(-15,10,23,9,1), new GeneratedEnemyUnit(-19,16,78,12,4), new GeneratedEnemyUnit(-20,-1,20,30,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "b6fe36c03ab17cfd026e7df87e31faaf9658d0ac480e103d97cf2443593b406b");
        }

        private static void Case_00378()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 378,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,1,45,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,8,97,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,1,64,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-10,91,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-17,63,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-12,10,10,1), new GeneratedEnemyUnit(-8,3,94,28,3), new GeneratedEnemyUnit(-20,-14,74,16,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "5e3d12a241356483aeda276392b0fcd0427f8ce15d1194d8e2ef4a264c3a3730");
        }

        private static void Case_00379()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 379,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,10,67,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-7,79,4,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "90a4ee01a3bcecb0dbcf943c08dbdc97b3fc9d87894b774641eb5e3d841de703");
        }

        private static void Case_00380()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 380,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,7,30,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,20,29,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,17,24,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,16,28,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-12,99,2,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "de07025f550ef8444335d010f4ed1c1e54557dcd70dbc687ca2bc9b1334ecd98");
        }

        private static void Case_00381()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 381,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-1,10,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-15,25,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,20,89,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-9,46,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-1,10,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-16,60,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,19,74,44,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f15679c48b1e02ff88112a18d02073f4ba736a345c19ba3d6548206f8f30b02c");
        }

        private static void Case_00382()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 382,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,0,84,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,8,76,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,1,85,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,6,27,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-13,100,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-19,58,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,12,22,30,1), new GeneratedEnemyUnit(-11,-9,48,44,3), new GeneratedEnemyUnit(1,5,66,18,3), new GeneratedEnemyUnit(-10,9,30,7,1), new GeneratedEnemyUnit(4,-20,18,3,4), new GeneratedEnemyUnit(-16,0,23,19,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "b4b90e6c67a9aa2a3f4619d0c98c0431b4be767be0675b5b8e5683679db146da");
        }

        private static void Case_00383()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 383,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,6,73,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-15,14,6,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "aadcd51adfc08f8c0cb3cfa56ac39d1b7a218be6b4b6800abdaace1c64fb53fe");
        }

        private static void Case_00384()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 384,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,16,34,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,13,25,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-17,46,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,10,31,23,1), new GeneratedEnemyUnit(-19,-12,51,4,4), new GeneratedEnemyUnit(3,-15,16,24,2), new GeneratedEnemyUnit(9,-7,29,20,2), new GeneratedEnemyUnit(20,-2,26,49,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "ea6473b7ec1865151f0b77107f75fd7ecc90b8d3879f4ca44265425299243df9");
        }

        private static void Case_00385()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 385,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-1,75,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,15,83,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,8,29,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-11,33,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-13,43,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-3,65,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,10,42,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,2,46,4,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "8a124036bb5310e3e7b1a82489e9bfee8cf735d5987dc3e3765d102db487d7d6");
        }

        private static void Case_00386()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 386,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-16,45,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,8,53,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,11,30,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,3,32,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-18,15,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,6,57,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-3,73,39,4), new GeneratedEnemyUnit(-1,-18,43,25,4), new GeneratedEnemyUnit(-9,6,74,47,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "da03318404890228fbe94ebf829b50b1b82ed33a0afe6c178ee8116395dcfa71");
        }

        private static void Case_00387()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 387,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-9,86,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-8,22,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,0,55,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,10,40,8,3), new GeneratedEnemyUnit(-11,1,49,5,1), new GeneratedEnemyUnit(8,-14,36,28,3), new GeneratedEnemyUnit(-10,-11,26,37,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "89bc6a5ce498b4c6c91b6948cef9b52885f56dfaefe740801e32649bbdd6e96f");
        }

        private static void Case_00388()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 388,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,14,31,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,5,57,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,9,85,14,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f3a616eaf2ab1449e4edb5e0c50b0b10856497f3711f64d4c59d525a047cf15c");
        }

        private static void Case_00389()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 389,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-9,26,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,10,36,40,2), new GeneratedEnemyUnit(18,-1,91,3,3), new GeneratedEnemyUnit(-19,-2,31,41,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "517a0668093c6f0410803c0256e7a32e6a241d3745b87cfdc5b86cef6e1d5b4d");
        }

        private static void Case_00390()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 390,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-12,23,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-20,84,38,3), new GeneratedEnemyUnit(12,7,79,49,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b42ddd305ab665677790ac3f94f9412e65231e0b789797468ee257762aa6820d");
        }

        private static void Case_00391()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 391,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,15,6,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-5,40,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,16,23,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,20,59,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,19,68,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,18,19,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-11,95,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-5,57,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-14,35,17,4), new GeneratedEnemyUnit(5,1,60,11,2), new GeneratedEnemyUnit(9,14,62,11,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "802c2e430f87911f256d31315b0069b2842a22b341b8c210167bdd078db7f36d");
        }

        private static void Case_00392()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 392,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,5,69,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,15,8,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,4,20,2,4), new GeneratedEnemyUnit(5,-17,42,15,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "98fc7d2c517f643a303f9065986788ba9eab64c76d74fd64c1a9cda52815d7ca");
        }

        private static void Case_00393()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 393,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,6,60,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,1,44,32,3), new GeneratedEnemyUnit(14,8,73,11,2), new GeneratedEnemyUnit(-12,-2,22,36,3), new GeneratedEnemyUnit(18,6,100,6,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "83f22c03450585cf12fb58878c0c4a63d75da447d160a87d19deb09f215cea00");
        }

        private static void Case_00394()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 394,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,10,49,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-20,95,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,12,99,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,20,66,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,19,23,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-13,55,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,20,86,29,4), new GeneratedEnemyUnit(0,0,76,5,4), new GeneratedEnemyUnit(-6,16,39,21,3), new GeneratedEnemyUnit(-12,-5,84,36,1), new GeneratedEnemyUnit(13,1,95,18,3), new GeneratedEnemyUnit(20,-12,80,31,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "dff871642a4d508bf9fd732e16c60fdc97232f24397d21b777b5f59b40019fd6");
        }

        private static void Case_00395()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 395,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,6,30,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-15,30,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,16,59,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-15,87,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-13,86,40,2), new GeneratedEnemyUnit(-4,5,6,15,3), new GeneratedEnemyUnit(0,17,29,42,1), new GeneratedEnemyUnit(2,9,80,22,3), new GeneratedEnemyUnit(2,-15,14,49,3), new GeneratedEnemyUnit(17,-8,19,25,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "a1092eff680df7bf7ec3e984d98b1e1dd2499bc7e98faaa09899a92a95768c09");
        }

        private static void Case_00396()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 396,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-15,72,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,13,92,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,10,92,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-20,75,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,3,55,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-6,57,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,15,9,30,3), new GeneratedEnemyUnit(-14,-18,18,13,3), new GeneratedEnemyUnit(0,-5,63,1,4), new GeneratedEnemyUnit(-12,-8,25,21,4), new GeneratedEnemyUnit(19,-6,43,19,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "cfb396e32b4454f69640b3cf5b7760edf50203283544afd55e976a6936738e4d");
        }

        private static void Case_00397()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 397,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,6,99,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,12,79,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,19,33,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-20,7,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,18,100,4,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "05df5f889921ab630fb0ea59d8d73eec1447cbdb6a0013ea7e5a78945138458b");
        }

        private static void Case_00398()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 398,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,2,64,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-20,29,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-3,100,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,19,54,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-5,70,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-13,77,9,4), new GeneratedEnemyUnit(7,8,60,24,4), new GeneratedEnemyUnit(16,-18,72,43,2), new GeneratedEnemyUnit(17,-19,28,46,2), new GeneratedEnemyUnit(5,3,53,11,4), new GeneratedEnemyUnit(3,-7,65,40,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "37795f51f67d603d9bd961ae72ca377c2269230e9cb2b4adaa41793be28d0cf5");
        }

        private static void Case_00399()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 399,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,1,61,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-10,85,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,0,53,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-18,15,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-11,90,9,1), new GeneratedEnemyUnit(9,16,86,3,2), new GeneratedEnemyUnit(9,11,51,29,1), new GeneratedEnemyUnit(19,-17,18,6,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "7be4e52b18a7eb28dfa41e2042b88dc09a32411eab885d000365e3dc086ef297");
        }

        private static void Case_00400()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 400,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-14,74,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,13,20,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,10,95,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-12,93,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-4,16,23,2), new GeneratedEnemyUnit(20,1,91,35,2), new GeneratedEnemyUnit(6,7,86,35,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "ceb06ae90b86f2f1e18277944dcbcf093b15a892acd7ea94ad6b704f84664436");
        }

        private static void Case_00401()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 401,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,6,63,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,5,97,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,20,54,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-2,19,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,12,12,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-18,37,17,4), new GeneratedEnemyUnit(-18,15,37,5,2), new GeneratedEnemyUnit(5,-11,35,37,3), new GeneratedEnemyUnit(18,5,77,25,2), new GeneratedEnemyUnit(-4,-18,56,21,1), new GeneratedEnemyUnit(-4,18,43,12,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "7be0de3ce77a61917e39ffca720017801303223dbc3cbf4f6b71da338b411ef7");
        }

        private static void Case_00402()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 402,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,7,58,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,6,83,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-9,26,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-17,44,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,13,32,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,13,96,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-7,55,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,0,89,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-20,45,3,2), new GeneratedEnemyUnit(-13,11,60,42,2), new GeneratedEnemyUnit(-2,5,22,13,1), new GeneratedEnemyUnit(-16,-8,60,17,2), new GeneratedEnemyUnit(11,-11,37,12,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "9114dd359c6ccbb90fbbc813b86b5bf3bf559a55d59204ec8f45e34893cb2919");
        }

        private static void Case_00403()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 403,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,5,61,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-14,15,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,14,64,8,4), new GeneratedEnemyUnit(-12,1,41,9,2), new GeneratedEnemyUnit(-15,-19,17,24,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "49164c42574314f25d37ef1f9a4e61441463a0485d3d8e86d4f20cad57092e6e");
        }

        private static void Case_00404()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 404,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-7,62,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-18,24,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,13,34,20,2), new GeneratedEnemyUnit(7,-19,32,37,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "1800b1e527097a75b06c578db5db8860c86bd449e3a9ad1ac32d43a6a80f4709");
        }

        private static void Case_00405()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 405,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-8,77,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,8,30,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,19,14,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,1,64,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,10,40,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,7,23,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,2,93,44,4), new GeneratedEnemyUnit(6,-4,76,30,2), new GeneratedEnemyUnit(5,-2,58,49,1), new GeneratedEnemyUnit(20,7,8,46,2), new GeneratedEnemyUnit(-1,-12,36,17,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "859375cffadae4c0845218a2065acf4dda8739ec87d0d916bfa16b72fa184cd8");
        }

        private static void Case_00406()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 406,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-14,64,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-5,76,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-7,72,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,13,72,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,8,26,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-5,6,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,1,10,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,13,88,35,2), new GeneratedEnemyUnit(4,17,6,24,1), new GeneratedEnemyUnit(10,12,86,38,4), new GeneratedEnemyUnit(18,17,91,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "5740326356ebc00ff1fb2462be5bc24c9734e70a35d88baa58842191e198a794");
        }

        private static void Case_00407()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 407,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,2,39,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-12,12,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-9,80,22,1), new GeneratedEnemyUnit(-3,-15,16,21,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "e62833d9271ae4892fac6af2e68f516749c4c4f685bee356ccf681029a2f5a1f");
        }

        private static void Case_00408()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 408,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-13,19,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,15,84,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,17,88,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,6,35,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-11,33,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-15,23,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-17,31,36,1), new GeneratedEnemyUnit(-17,-10,67,42,3), new GeneratedEnemyUnit(1,-6,93,34,4), new GeneratedEnemyUnit(7,-4,56,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "47678d492af3a0c8c8d797bebacd9e575227bbf84b5309878cd447b70ff1ac3c");
        }

        private static void Case_00409()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 409,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-7,72,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-20,44,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-10,43,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,1,90,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-15,27,21,4), new GeneratedEnemyUnit(0,-18,7,26,2), new GeneratedEnemyUnit(15,-14,29,35,4), new GeneratedEnemyUnit(20,17,54,27,2), new GeneratedEnemyUnit(-9,-20,74,39,3), new GeneratedEnemyUnit(-20,15,95,3,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "a543edf85c2277a8d1155e37490ebc27fedcc301208397ed7bd653503f6aec41");
        }

        private static void Case_00410()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 410,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-15,24,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,0,61,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-17,55,42,1), new GeneratedEnemyUnit(-4,0,27,49,4), new GeneratedEnemyUnit(16,8,52,50,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "6f12a4a53cbb2f72cd3df5c653721a3eb0176eb7b8c9f40502947d42f7504566");
        }

        private static void Case_00411()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 411,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,2,67,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,4,17,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,20,17,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,8,80,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-12,14,11,2), new GeneratedEnemyUnit(-12,3,15,29,2), new GeneratedEnemyUnit(-19,8,18,25,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "761fd3fb1913b83e232ac6d51a842852b293f8de4698684b18d44ec91558ce8f");
        }

        private static void Case_00412()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 412,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-8,10,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-5,26,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,18,74,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,20,31,32,3), new GeneratedEnemyUnit(-16,-4,40,16,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "dc0fd9a2caba7efa52613b519fdccc6279d408c254628b18ea9ab596a2761e34");
        }

        private static void Case_00413()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 413,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,6,34,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,19,41,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,11,30,47,2), new GeneratedEnemyUnit(0,-1,35,45,1), new GeneratedEnemyUnit(3,-5,21,50,3), new GeneratedEnemyUnit(17,-15,54,20,4), new GeneratedEnemyUnit(-11,14,79,46,4), new GeneratedEnemyUnit(-18,-16,83,22,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "bf4218f7d9fbcc3379f32db6f3bb2160d1eb09351da3553a62662c0070ec4ff2");
        }

        private static void Case_00414()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 414,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,0,35,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,2,92,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-16,32,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,18,11,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,7,16,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-10,80,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,7,35,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,5,46,7,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "538d1504035608ae7c6bdb046dbec5f5f7a11bdb59ef9f15dd988aaa2300039a");
        }

        private static void Case_00415()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 415,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-12,74,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-3,85,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-14,60,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,8,63,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-1,28,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-19,91,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-20,97,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,0,61,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,17,50,13,1), new GeneratedEnemyUnit(-20,-6,62,19,2), new GeneratedEnemyUnit(14,1,45,28,3), new GeneratedEnemyUnit(-16,11,76,28,3), new GeneratedEnemyUnit(-6,-7,68,17,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "ef6b6811c1c1a2ec0aab584ca993baa550a355b08047842153a1a7fe71ebbf67");
        }

        private static void Case_00416()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 416,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-18,88,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-15,82,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-17,57,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-2,71,3,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "27b9e3eb190b30c11615367c5352a0810423a287a9421b99b92f843c531671b5");
        }

        private static void Case_00417()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 417,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,0,27,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-5,58,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-8,85,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,8,60,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,11,36,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-14,61,38,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "b2231ca2aef9b271c163ff208e3f5253e1088ae29a6f3937866f3aa4bda9cc9d");
        }

        private static void Case_00418()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 418,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-13,68,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,17,59,13,1), new GeneratedEnemyUnit(7,-1,31,18,2), new GeneratedEnemyUnit(-16,-14,13,12,2), new GeneratedEnemyUnit(-14,8,65,45,3), new GeneratedEnemyUnit(2,0,66,18,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "9af7b8f447542ea3f3c61ff294a3cf4d5bd7873497b4f0c2c695ad96dcceffac");
        }

        private static void Case_00419()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 419,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,13,92,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,7,28,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,8,21,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,19,20,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,7,8,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-14,8,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,4,86,5,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "b992ed7d7d7eeeee63dd0f3ee8e069c0d1c1aa64ad7db6581a0ec995c7c9fdea");
        }

        private static void Case_00420()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 420,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,2,52,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,6,41,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,2,53,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,16,89,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,20,23,3,1), new GeneratedEnemyUnit(10,-2,89,43,1), new GeneratedEnemyUnit(-4,19,67,10,3), new GeneratedEnemyUnit(-4,12,66,13,2), new GeneratedEnemyUnit(17,-3,70,38,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "494f988d13ef8966121c3b9beb9cf31b182c5e131dfb1c7eaaaa0209b36d3e25");
        }

        private static void Case_00421()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 421,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,7,38,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-5,19,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-11,30,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-14,21,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-12,81,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,0,26,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,11,13,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-7,10,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,12,32,17,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f6ccea4e1222fdceafe5fd2a1cf91ecd8cec56d009fbe6d403e7149d068cbfcd");
        }

        private static void Case_00422()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 422,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-10,24,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-13,10,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,2,10,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-20,44,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-17,97,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-12,51,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-7,5,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,16,65,14,3), new GeneratedEnemyUnit(9,-13,50,26,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "48f3b37ebfbeaa3a441d872e683c4b30e05520079ae3e813ec908589a687b008");
        }

        private static void Case_00423()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 423,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-8,92,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,7,52,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-11,97,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,7,55,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,15,20,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,9,62,1,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "ae5ff5228efe68273f3d3fae38dc00fbd9b6c3efa67222226c639f7e0b268631");
        }

        private static void Case_00424()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 424,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-20,41,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-10,94,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,13,48,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,10,98,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,7,25,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-10,11,28,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "81fe726edf55c050e8058560e007157fd5505e229d848c2cd7727f55e4074ddd");
        }

        private static void Case_00425()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 425,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,3,45,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,3,59,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,14,36,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,9,91,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,15,25,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-20,15,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,7,77,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,13,30,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,18,45,38,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "b787bdda4e153292fad9a57283c96cdbaaa781ca5b4338f2c43f70c7aedcd288");
        }

        private static void Case_00426()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 426,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,10,49,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-5,52,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,7,86,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,7,63,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,7,56,14,1), new GeneratedEnemyUnit(20,20,98,41,4), new GeneratedEnemyUnit(16,0,95,36,4), new GeneratedEnemyUnit(-11,0,17,39,4), new GeneratedEnemyUnit(10,-2,33,21,2), new GeneratedEnemyUnit(-15,-12,88,2,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "56bfa55551bea47915ed0121cdfd1f9c0c93c74c3d4e7b9599cdb69bdd92f240");
        }

        private static void Case_00427()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 427,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,9,21,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-16,86,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,3,77,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-18,65,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-19,7,46,3), new GeneratedEnemyUnit(12,-3,97,30,3), new GeneratedEnemyUnit(11,-17,57,10,3), new GeneratedEnemyUnit(-18,5,42,48,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "14e6af87441b437018f8cb3c69ae06295260def17f8fd80dc4eaf586214a40d8");
        }

        private static void Case_00428()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 428,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-11,41,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-6,41,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,10,13,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-1,19,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-1,84,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-4,97,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-11,69,33,1), new GeneratedEnemyUnit(-19,12,72,48,3), new GeneratedEnemyUnit(-20,14,100,14,4), new GeneratedEnemyUnit(3,1,97,6,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "b3294fb812cd5d6820c381892d166fcdcf938cf2cc2b7ee3d9e112d8f25c2b69");
        }

        private static void Case_00429()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 429,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,7,74,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,1,68,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-9,88,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-11,5,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-2,14,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-12,82,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-6,72,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,16,90,7,2), new GeneratedEnemyUnit(7,3,11,43,4), new GeneratedEnemyUnit(-20,-19,80,23,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "67452fca49a94860ab698459faffc35e76f70a0c048e626e987b21ee7a108b7e");
        }

        private static void Case_00430()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 430,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,3,54,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,15,76,19,3), new GeneratedEnemyUnit(7,7,10,27,2), new GeneratedEnemyUnit(-9,13,24,12,1), new GeneratedEnemyUnit(-3,15,33,10,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "711e38ace6a9fec9af29fd4bcf56324d5699928d40eb933b31b44db18c4406bb");
        }

        private static void Case_00431()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 431,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,9,90,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-6,76,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-20,98,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,10,26,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,5,58,42,4), new GeneratedEnemyUnit(4,-14,92,37,4), new GeneratedEnemyUnit(-20,-9,72,8,4), new GeneratedEnemyUnit(11,-8,76,6,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "03f285eac8ffe0a855681ca0bf3fa6b1a8c83ba02b95209deb620e3b6ab90bcd");
        }

        private static void Case_00432()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 432,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,9,73,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,11,87,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,17,11,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-17,90,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-18,30,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-11,14,15,4), new GeneratedEnemyUnit(1,18,36,25,4), new GeneratedEnemyUnit(-11,16,8,19,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "aedcf662c7283310f5d37f8304bd0ab39416b0df17d3d97be099923f4677c99b");
        }

        private static void Case_00433()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 433,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,10,37,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,0,94,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,10,22,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-12,70,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "11c10acded8bac760333b1f44ffe3a9b5b8b3be2ce880de317fc75fc87452e82");
        }

        private static void Case_00434()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 434,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,9,50,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,12,43,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-7,80,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-2,49,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-10,54,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-3,85,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,11,34,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,1,46,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-3,22,22,1), new GeneratedEnemyUnit(-3,9,7,17,2), new GeneratedEnemyUnit(3,-12,88,17,3), new GeneratedEnemyUnit(17,10,34,39,2), new GeneratedEnemyUnit(19,-16,54,48,3), new GeneratedEnemyUnit(-12,-16,62,45,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "f8834e4ad61a9b6b5e5bb61a4001dfa56a27526b93172c5cabebb25d702c48df");
        }

        private static void Case_00435()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 435,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,0,67,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-18,23,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-7,39,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-4,12,44,4), new GeneratedEnemyUnit(-3,3,24,37,1), new GeneratedEnemyUnit(-2,5,69,19,2), new GeneratedEnemyUnit(1,7,21,49,1), new GeneratedEnemyUnit(-5,11,83,35,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "94bd77d1818a073b962559e11aab4c26ccd9f7eb0d17e9dfb0e3db9277413668");
        }

        private static void Case_00436()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 436,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-1,60,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,5,95,18,1), new GeneratedEnemyUnit(-11,13,9,10,2), new GeneratedEnemyUnit(8,-16,63,25,2), new GeneratedEnemyUnit(-1,9,28,47,1), new GeneratedEnemyUnit(-2,-4,84,46,3), new GeneratedEnemyUnit(2,5,29,7,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "88c7dbac2cc1ec4e4eedf1775fb1ea0f7c751630d444e6b255873774adc969f7");
        }

        private static void Case_00437()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 437,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-19,83,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-4,20,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,4,44,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,13,94,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-17,39,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-18,91,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,6,25,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-6,62,46,4), new GeneratedEnemyUnit(-4,-7,64,13,4), new GeneratedEnemyUnit(6,13,54,15,3), new GeneratedEnemyUnit(-15,17,96,35,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "2c8ae69f51f66231eb9366b93b451462489b358d357ddae208867a7ccc9304c9");
        }

        private static void Case_00438()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 438,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,19,30,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,6,17,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,7,62,4,1), new GeneratedEnemyUnit(-6,20,85,7,3), new GeneratedEnemyUnit(-2,-15,65,48,4), new GeneratedEnemyUnit(10,0,76,31,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d94267ae1b8b75d999f40c5babfcb39253ad56e97f61db1e8f9c04babdaaf704");
        }

        private static void Case_00439()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 439,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,15,15,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,20,44,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,13,99,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,6,27,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,20,30,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,6,21,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,2,18,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-10,65,3,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "af8af8903b64a5d1800b8fa82324b24b87b0e51a5f8440f084bb7267b89d2e37");
        }

        private static void Case_00440()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 440,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,9,80,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-17,85,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,3,32,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,3,28,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-12,52,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-15,92,7,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "a25ed53df2317a978fa8c9557978f6265118c6e50a4688f11b55466cb5df7cd9");
        }

        private static void Case_00441()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 441,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,8,6,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,20,9,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-12,93,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,17,54,29,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "be341599144a5c5b00eca6f5acebb4ce0d0b3220ae5411a3c92be11c1aebd666");
        }

        private static void Case_00442()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 442,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,17,41,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-16,45,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,4,68,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-14,57,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,10,38,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-11,95,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-17,28,33,3), new GeneratedEnemyUnit(-19,18,81,41,3), new GeneratedEnemyUnit(2,-14,27,10,3), new GeneratedEnemyUnit(-17,-18,65,27,3), new GeneratedEnemyUnit(1,15,69,15,1), new GeneratedEnemyUnit(19,-1,50,25,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "c9523797af4fcd7d95d7c1982417afc1775c9de770c0827314cf04b10587abdd");
        }

        private static void Case_00443()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 443,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-18,30,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,3,70,6,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "3935da9df2f616c3657f819576e54830ca7dc27b437b5fca68538f4737b48acf");
        }

        private static void Case_00444()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 444,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,17,94,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,2,88,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,9,99,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-14,52,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,9,17,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,0,25,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,6,74,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,2,71,7,1), new GeneratedEnemyUnit(20,18,99,43,1), new GeneratedEnemyUnit(4,0,21,44,4), new GeneratedEnemyUnit(20,-14,95,26,2), new GeneratedEnemyUnit(-19,20,26,35,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "fc923020f41ae0833217fb10c8277001e7585e4ad35c0a6584a35a12f1f2b129");
        }

        private static void Case_00445()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 445,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,9,49,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,0,59,4,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "b388069fe7e4698f9e0c8f0236a8648a5da244b9ed920ed36d78808904ba082d");
        }

        private static void Case_00446()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 446,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-11,18,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-4,51,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,3,73,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,3,35,41,3), new GeneratedEnemyUnit(-9,3,35,19,1), new GeneratedEnemyUnit(3,15,22,21,1), new GeneratedEnemyUnit(-12,-19,86,7,2), new GeneratedEnemyUnit(-4,11,5,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "d4d3406931fb2e6219528263cdbddaa115be9ce5959a24a8dbad2a74a0e109dc");
        }

        private static void Case_00447()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 447,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-19,99,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,14,66,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,1,11,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,5,91,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,2,25,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-1,79,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,6,34,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,18,9,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,10,47,33,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "93ba830af69b1586ec8027771bd598df088927c03486d84b5cfad24a3e4a0f01");
        }

        private static void Case_00448()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 448,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,2,46,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-4,100,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-7,73,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,10,93,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,15,58,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-2,30,21,4), new GeneratedEnemyUnit(3,-8,36,5,4), new GeneratedEnemyUnit(-12,1,18,3,1), new GeneratedEnemyUnit(-20,-17,87,29,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "6faeb59e5ff2f56aabaea123c357346c85803c0b13df378129afc78b4d47c5a8");
        }

        private static void Case_00449()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 449,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-13,20,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,1,93,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,1,45,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-6,33,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-7,90,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,6,74,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-11,10,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-9,60,16,3), new GeneratedEnemyUnit(-1,-7,48,31,1), new GeneratedEnemyUnit(19,6,74,6,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "363a2ac9e5d69c2bf502c91ff590964ae74058a720eea09a87752f5b31983c07");
        }

        private static void Case_00450()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 450,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,7,97,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-16,52,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-13,97,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,12,77,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-14,83,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,2,38,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,9,58,50,4), new GeneratedEnemyUnit(0,-12,99,44,1), new GeneratedEnemyUnit(-1,-10,63,31,2), new GeneratedEnemyUnit(-3,6,29,49,4), new GeneratedEnemyUnit(7,3,29,5,1), new GeneratedEnemyUnit(9,-3,36,2,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "3b92d0b6d32527914287aaa184b322cb358f5ca37cd137697f719b9bc30d0ace");
        }

        private static void Case_00451()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 451,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-16,29,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-16,86,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,11,11,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,1,92,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,5,42,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-8,12,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-14,90,16,1), new GeneratedEnemyUnit(18,14,97,18,1), new GeneratedEnemyUnit(19,19,48,16,3), new GeneratedEnemyUnit(-14,-7,31,27,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "a871efde3bdf9ede7e3359c5327b614fd35da4b44a4b45d4fe440c0e312e6977");
        }

        private static void Case_00452()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 452,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,17,41,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,0,72,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-10,73,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,9,93,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,1,61,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,14,15,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,13,19,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,19,93,35,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "11b6c18e3a2603925d640be30259397379324253e2f181c98717af9360eadea8");
        }

        private static void Case_00453()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 453,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,2,45,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-3,25,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,8,10,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,16,78,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,6,16,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-4,97,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,2,87,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-20,96,20,2), new GeneratedEnemyUnit(17,-4,94,40,1), new GeneratedEnemyUnit(-14,-9,21,8,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "15a43a12a11d76eee1c085a0c965da9d50bb0ce6621d8cfa59cd7aff07e0bbc1");
        }

        private static void Case_00454()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 454,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-7,5,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,3,86,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-14,72,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-11,35,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-3,9,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-12,38,15,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "532c0be5579500aa67593c9543dd5b8eb4c304c5b79f2ddb8465bb6588f88d28");
        }

        private static void Case_00455()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 455,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,0,62,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,17,77,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-20,33,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,3,40,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-11,36,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-5,28,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-10,69,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-3,60,30,1), new GeneratedEnemyUnit(5,-19,24,17,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0a59d2348bbaf395ad0a17f2e778d435c3e4b98ed4201f0c7322e5ba54a7950d");
        }

        private static void Case_00456()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 456,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,19,83,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-15,59,1,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "bd791a7c651e662d06d2c5a78ca05a5146d7020d23eee8fe304e36a9fa7cc484");
        }

        private static void Case_00457()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 457,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,12,8,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,13,59,50,4), new GeneratedEnemyUnit(14,-9,50,4,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "ad307a97244c7766af67a8ae2754f4cce28ab46ad2294eb03af162b6a36d0cd1");
        }

        private static void Case_00458()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 458,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,1,23,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-6,85,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,9,28,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,11,38,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,6,22,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-17,69,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-1,33,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-16,9,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,10,28,30,3), new GeneratedEnemyUnit(-18,17,56,8,4), new GeneratedEnemyUnit(11,5,37,30,3), new GeneratedEnemyUnit(4,7,33,6,1), new GeneratedEnemyUnit(-11,-16,45,47,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "3499bd9be06d70eafe995d2e5f3264fc8891915e8103f11258af17495ef7de45");
        }

        private static void Case_00459()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 459,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,0,25,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-9,17,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-8,39,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,14,46,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,1,57,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,17,92,42,4), new GeneratedEnemyUnit(-9,-11,68,31,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "4086a06c5bbc126860b6344590b3b8d717b9887812934ba6a3e458116dbe30f7");
        }

        private static void Case_00460()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 460,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,3,79,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-2,76,38,4), new GeneratedEnemyUnit(-12,7,95,44,1), new GeneratedEnemyUnit(-3,-19,64,45,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "1f3659fd232290fe5b22d9edb41d02fc60a56a1258728d55b0ef3a521e73358c");
        }

        private static void Case_00461()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 461,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,4,96,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,0,85,47,3), new GeneratedEnemyUnit(20,13,54,44,1), new GeneratedEnemyUnit(17,-14,84,29,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "e3810d26b608a86372bd2301515c2e9479cde12201c558c5c1c064bf7a16f298");
        }

        private static void Case_00462()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 462,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,7,40,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,3,66,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,15,49,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-8,52,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,12,87,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-8,76,41,1), new GeneratedEnemyUnit(-20,16,7,19,3), new GeneratedEnemyUnit(-19,18,94,19,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "94afb44faa56905af2a08f8fc06e0529c01bbc20d9076960c35f7bc9156881e3");
        }

        private static void Case_00463()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 463,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-6,67,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-6,75,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-18,75,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-16,93,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-18,71,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-5,55,25,2), new GeneratedEnemyUnit(-6,-7,76,14,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "742addbde195c762396e139138d9b5aec9a9f1bb8f3add6717b5b5299e635587");
        }

        private static void Case_00464()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 464,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,9,26,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-4,26,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,10,62,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-19,8,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,20,59,8,4), new GeneratedEnemyUnit(7,13,67,26,3), new GeneratedEnemyUnit(20,7,49,43,1), new GeneratedEnemyUnit(10,-9,38,4,4), new GeneratedEnemyUnit(2,1,22,16,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "e778b8271864143b247f0fcbdef7a4d5735be724985a709d2f78e94a2fd214e5");
        }

        private static void Case_00465()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 465,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-18,28,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,11,47,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-10,63,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,19,17,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,19,99,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,2,16,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,0,18,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,20,57,1,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "bde3f24fbbc416506d64e2a46b5c266e758ed1b98ea71ca02e0394414d99c5e5");
        }

        private static void Case_00466()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 466,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-18,42,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,8,7,50,1), new GeneratedEnemyUnit(-12,9,92,30,1), new GeneratedEnemyUnit(-13,3,18,25,1), new GeneratedEnemyUnit(3,13,80,46,1), new GeneratedEnemyUnit(7,-20,45,18,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "6511ff41b86806a10899b245b680aed65f5cda970f716d9fad8a4b072e9eddb5");
        }

        private static void Case_00467()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 467,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-4,69,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-10,80,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-8,93,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,7,51,48,2), new GeneratedEnemyUnit(1,12,10,6,2), new GeneratedEnemyUnit(-7,0,7,3,1), new GeneratedEnemyUnit(0,11,55,48,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "5320efdd6c7a4eb48ad8e069a90e19b25030ffb26e9ea1ee9a2941d85d7817a6");
        }

        private static void Case_00468()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 468,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-5,39,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-17,57,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-1,6,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,9,94,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-18,23,2,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "49dad34b0021db9007a6416d5f41dee171e0fe3fa9e4438e690cc16a1fe00ed5");
        }

        private static void Case_00469()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 469,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,19,37,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-6,5,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-15,67,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,1,16,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-17,49,3,4), new GeneratedEnemyUnit(20,15,76,10,3), new GeneratedEnemyUnit(-2,-17,39,38,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "ea04c0a7cc41821a941e3e8b24d8a90ca99b556761b11a6477839dbc488d88c4");
        }

        private static void Case_00470()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 470,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-8,49,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,15,76,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "a71537460ba3bf2b56d06c7b728289ffb5781195abae100a63a2d30f909885a9");
        }

        private static void Case_00471()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 471,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-10,62,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,2,54,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-10,86,49,4), new GeneratedEnemyUnit(19,-8,13,9,2), new GeneratedEnemyUnit(-1,-15,30,10,4), new GeneratedEnemyUnit(-9,-13,17,7,1), new GeneratedEnemyUnit(6,8,61,22,3), new GeneratedEnemyUnit(12,13,49,48,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "e9b907e33f4e9c39c7ee74d21460374f37a1ae2b0b5525ee5317c5dd2a11a136");
        }

        private static void Case_00472()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 472,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-7,57,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,0,65,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,19,81,34,3), new GeneratedEnemyUnit(15,-5,100,32,3), new GeneratedEnemyUnit(-2,0,94,3,1), new GeneratedEnemyUnit(-19,1,52,14,2), new GeneratedEnemyUnit(19,2,55,45,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "73f7923c40d26e749d15485c0e91d32636862266ba684c13d564703a9ad61f10");
        }

        private static void Case_00473()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 473,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,15,99,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-12,58,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-3,69,30,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "501447652763adece70a7326ac4fcce5691a07f2d9eb6f5eae64a39a474341dc");
        }

        private static void Case_00474()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 474,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-18,14,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-16,29,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,11,6,4,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "7cc1c6cf7245b1fc8f870cd5d181ea789e1e99cd0f41885faba1e67914ace0a5");
        }

        private static void Case_00475()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 475,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,6,52,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-2,52,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,6,69,43,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e1e5031acd74c18cbec2c62fa80bf99c5ae4974a248960326f48563ba0525b12");
        }

        private static void Case_00476()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 476,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-12,47,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,15,11,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,18,5,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-11,89,32,3), new GeneratedEnemyUnit(9,1,25,8,1), new GeneratedEnemyUnit(3,11,60,18,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "eaf6efcad458e38045007edad47a65d9e582b997aa3624df0628b05c2d8f5138");
        }

        private static void Case_00477()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 477,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-20,71,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,19,65,33,4), new GeneratedEnemyUnit(-14,18,82,3,2), new GeneratedEnemyUnit(10,-19,13,43,3), new GeneratedEnemyUnit(8,-7,39,49,3), new GeneratedEnemyUnit(-17,-14,35,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "4881662c3da6573fadf92967749d01035d9a3039d63369ef0edb36d0855c6b60");
        }

        private static void Case_00478()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 478,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-13,47,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-18,35,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-4,59,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,4,24,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,15,39,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,6,20,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,8,55,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,10,13,50,3), new GeneratedEnemyUnit(6,-19,58,27,4), new GeneratedEnemyUnit(15,5,50,20,3), new GeneratedEnemyUnit(9,-13,51,17,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "7944fe9b4fba19ee01c53172736f4980900b9ef4924e07ba1d9cc80982ce110b");
        }

        private static void Case_00479()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 479,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-5,50,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,20,9,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,7,20,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-20,74,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,5,43,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,3,56,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-16,51,24,3), new GeneratedEnemyUnit(-8,-7,66,42,4), new GeneratedEnemyUnit(10,-12,67,39,3), new GeneratedEnemyUnit(14,-10,94,3,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "ab36551add652fb693c9ccb9bd377508cd2f1ddfd3f8a968d027a71936c46dca");
        }

        private static void Case_00480()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 480,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-8,67,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-7,72,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,3,83,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,1,29,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,18,72,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-14,39,29,3), new GeneratedEnemyUnit(-3,-14,72,8,1), new GeneratedEnemyUnit(-1,1,9,2,4), new GeneratedEnemyUnit(-8,18,13,39,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "f957891590f0005258755a820bd9779b65d5a406e83e4a82115197b5697a9166");
        }

        private static void Case_00481()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 481,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-15,18,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,19,47,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-4,66,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,20,61,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,14,53,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-6,98,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,10,68,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-9,65,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,14,26,36,1), new GeneratedEnemyUnit(16,-2,51,10,3), new GeneratedEnemyUnit(-1,-20,84,33,3), new GeneratedEnemyUnit(-12,-19,17,7,2), new GeneratedEnemyUnit(20,-13,27,19,1), new GeneratedEnemyUnit(-4,-13,70,16,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "e2d4fbda761e9c22dbe3839c4cade4ad92004cc80e618b89f9457f048645a2ef");
        }

        private static void Case_00482()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 482,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,8,10,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,17,33,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,5,90,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,5,47,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,16,66,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,5,48,10,4), new GeneratedEnemyUnit(-12,4,92,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "2924a3d1cdc69ebc2d013c0bb5d2efbd0c686ab68994d0e781fe2a32aca3b48d");
        }

        private static void Case_00483()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 483,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,15,54,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,0,80,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-4,66,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-15,19,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,10,10,22,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "356746b8dc463c68f5b0a29a6fcc3d181e34bcac18afffbb315156d72a66a814");
        }

        private static void Case_00484()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 484,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-19,85,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-18,14,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-12,93,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-15,33,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,17,24,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-5,50,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-10,47,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-20,77,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,5,79,38,3), new GeneratedEnemyUnit(-11,-20,67,9,3), new GeneratedEnemyUnit(2,10,79,40,4), new GeneratedEnemyUnit(6,14,98,13,4), new GeneratedEnemyUnit(-15,17,82,44,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "01112144e71aeca1531dc5baa4fb52ee528190eae53904edb2197c4ee0f39e58");
        }

        private static void Case_00485()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 485,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-6,14,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,18,51,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,20,14,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-11,85,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,6,67,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-19,68,48,1), new GeneratedEnemyUnit(20,4,70,12,1), new GeneratedEnemyUnit(13,6,85,17,2), new GeneratedEnemyUnit(6,6,98,28,3), new GeneratedEnemyUnit(1,17,46,29,4), new GeneratedEnemyUnit(15,-8,70,43,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "4b7da50ff5d0f02846365f02f46a4cb278963328094e57a409d90f2e5b0ca4d1");
        }

        private static void Case_00486()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 486,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,3,45,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-18,79,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-2,99,46,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "5513c55a633d965fce0cd3609ef07a263f55b90d295c43c096861cbc1eb8d6bc");
        }

        private static void Case_00487()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 487,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,7,75,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,9,100,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,6,16,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,1,65,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,18,93,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,9,48,13,1), new GeneratedEnemyUnit(-20,15,31,44,1), new GeneratedEnemyUnit(18,3,62,8,3), new GeneratedEnemyUnit(-2,-11,66,43,2), new GeneratedEnemyUnit(8,-19,9,23,1), new GeneratedEnemyUnit(11,-4,31,23,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "3eeca217066784cbacbf3b9193d7f66e86a36c874ee291aa7447b7f5d2df462e");
        }

        private static void Case_00488()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 488,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-20,66,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,19,15,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-12,85,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,18,96,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-11,86,36,1), new GeneratedEnemyUnit(-5,18,87,43,1), new GeneratedEnemyUnit(1,-2,16,43,4), new GeneratedEnemyUnit(16,-18,29,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "74ece53c3260f8813b6a2fdb640a778f9fa83f6b0fa4aaed02ed4453da7aeae8");
        }

        private static void Case_00489()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 489,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-8,14,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-18,11,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,17,49,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-13,48,7,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "2969ba1f1289ab046d51af43d3819dc21037f4c3cb993253502eb4735dab3a31");
        }

        private static void Case_00490()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 490,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-19,38,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,5,74,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-18,10,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,8,22,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,14,74,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,4,75,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-16,24,7,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "186e3a39d47ba587cb7e1f8912c152060f55cbec3ff4c8ed9a5f746373de4aec");
        }

        private static void Case_00491()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 491,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-9,95,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-11,97,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-8,54,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-12,81,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-17,42,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,17,61,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-3,27,46,1), new GeneratedEnemyUnit(5,-5,49,29,2), new GeneratedEnemyUnit(6,-8,44,44,3), new GeneratedEnemyUnit(-1,-2,13,18,3), new GeneratedEnemyUnit(16,18,99,27,4), new GeneratedEnemyUnit(-3,-17,40,15,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "f7793007684e257a61dc0c69926056f2b898968c990fe545f64a67bf26d76931");
        }

        private static void Case_00492()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 492,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,20,80,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,18,46,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-13,22,1,1), new GeneratedEnemyUnit(4,-14,75,29,3), new GeneratedEnemyUnit(-8,-3,85,14,4), new GeneratedEnemyUnit(-12,-11,89,6,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "450dc81acc0d8af27e9a4fd080b5e71b32e38469ec8e570c87c8cfedd25ed6e3");
        }

        private static void Case_00493()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 493,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-7,95,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-14,94,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-8,98,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,19,23,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,5,28,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,2,76,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,18,75,6,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "9f935dafa2b6d5c3f5dfedc05000bd989a1e49c4135ea5d1a84cf308e22811f8");
        }

        private static void Case_00494()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 494,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-11,30,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,20,59,2,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "d48a2c4f3ae4a29c74d24478ae8b2be8b90a44d14a157f0ce8ef01e38c8ed5bf");
        }

        private static void Case_00495()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 495,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-1,9,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-12,29,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,19,30,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,20,36,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-17,86,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,1,20,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-10,66,2,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "bff9836a2b3fc666a0e635563b95212b02a308db1d38d472fe5595a26c14845b");
        }

        private static void Case_00496()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 496,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-10,52,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,3,88,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-3,42,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,1,91,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,7,61,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,5,95,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-1,98,4,3), new GeneratedEnemyUnit(5,-4,22,6,1), new GeneratedEnemyUnit(-14,-4,26,50,1), new GeneratedEnemyUnit(8,-18,76,11,4), new GeneratedEnemyUnit(-14,-9,88,7,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "51b2c6fd5a7a2ca5a90da0c5d8df762789daf66235375b13e73c8fb8eef56c36");
        }

        private static void Case_00497()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 497,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-4,87,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,2,66,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-1,24,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,9,41,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-15,57,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-1,44,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,13,92,7,3), new GeneratedEnemyUnit(10,12,17,10,3), new GeneratedEnemyUnit(-9,-1,16,43,3), new GeneratedEnemyUnit(8,-18,53,37,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "3528d1bd6773866ea978140ada357382c7b58ee05f6bb55f7a7e2701577b2832");
        }

        private static void Case_00498()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 498,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-7,11,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,2,42,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,6,30,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-16,33,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-17,74,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-11,100,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-11,68,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-20,93,6,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "1892cdb20bbeef9e4cf16ef4c1bbd39a1f7ede851ed8447e6aadb6c9b0a1bfa9");
        }

        private static void Case_00499()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 499,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,19,53,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-12,32,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,2,25,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-11,40,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,5,92,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-5,24,42,4), new GeneratedEnemyUnit(10,3,9,8,2), new GeneratedEnemyUnit(-10,4,79,28,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "4f2165c3a9583127fc5b8d17335c004926b22f76ec05ccd820832120068c8590");
        }

        private static void Case_00500()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 500,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-5,76,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-13,25,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,12,21,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,7,53,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-18,11,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,5,52,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,10,45,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-2,90,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,11,21,28,1), new GeneratedEnemyUnit(5,0,61,18,3), new GeneratedEnemyUnit(13,-10,23,21,4), new GeneratedEnemyUnit(20,10,5,50,3), new GeneratedEnemyUnit(-18,-15,35,5,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "93b2b93c3fd7b0c36124e5b85fbf9c3320101130436a290b6ca8ff7fcc3e057d");
        }

        private static void Case_00501()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 501,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-4,97,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-12,30,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,0,11,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,3,76,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-16,63,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,9,60,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,20,76,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,14,79,46,2), new GeneratedEnemyUnit(-9,10,89,18,4), new GeneratedEnemyUnit(-12,-12,22,38,3), new GeneratedEnemyUnit(6,3,62,9,2), new GeneratedEnemyUnit(11,13,61,2,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "462196f25e675d6a82fc40a1f0f7452da62f78053343c9c9c17a3435de556bdd");
        }

        private static void Case_00502()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 502,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,3,34,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,3,17,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-19,50,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,19,8,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,16,45,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,0,71,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,5,72,21,2), new GeneratedEnemyUnit(-5,-20,56,14,3), new GeneratedEnemyUnit(19,-6,71,5,3), new GeneratedEnemyUnit(14,-19,67,19,3), new GeneratedEnemyUnit(-13,5,84,46,3), new GeneratedEnemyUnit(0,13,8,10,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "5d6505ec62d302f4ff8d0c0a1878f83c59ef6e425c9d434330337ca8453d5cdd");
        }

        private static void Case_00503()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 503,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-11,69,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-19,42,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,14,64,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,15,38,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-17,40,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,4,29,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,9,24,44,3), new GeneratedEnemyUnit(20,3,89,7,1), new GeneratedEnemyUnit(8,4,17,47,3), new GeneratedEnemyUnit(2,-7,58,48,1), new GeneratedEnemyUnit(-1,5,81,5,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "393699c0d8874527ea51d759ada75a16a0f65345cdc460dd1c7db1d6134bf247");
        }

        private static void Case_00504()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 504,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-9,74,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-19,5,45,1), new GeneratedEnemyUnit(-15,-9,79,40,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "e893a9769799ba05316aa5d53074b63f46d215640b9cc7e30b0fe673165d1d3d");
        }

        private static void Case_00505()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 505,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,11,16,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-17,9,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-13,77,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,17,10,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-20,67,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,3,45,4,2), new GeneratedEnemyUnit(-17,15,93,17,3), new GeneratedEnemyUnit(7,6,86,29,4), new GeneratedEnemyUnit(-6,-7,7,34,2), new GeneratedEnemyUnit(-14,10,6,4,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "e46b9bd151137e77f36f7ecbf3df10b6373e75faf7a965c21390c0ee92a03713");
        }

        private static void Case_00506()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 506,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-19,72,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-19,64,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,7,5,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-18,5,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-10,39,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,16,68,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-18,44,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,3,74,7,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "bd2a3953af07be795fad511bd9541822f2580f8e43f3771482c44eec15d10cdf");
        }

        private static void Case_00507()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 507,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-19,63,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-9,95,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,2,35,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-19,92,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-14,17,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,8,18,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,19,29,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,16,98,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,14,49,21,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "7bd57cdb7a8dfb55ba9893fb92c179124ba5aee4f1a74e45f7d0f3ced3e26d05");
        }

        private static void Case_00508()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 508,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,8,51,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,3,10,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,14,85,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,13,16,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-4,86,7,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "2a7ea49cf6996ef240c884632ef003f37ab8b12b0df2b0941b20a539623a97e4");
        }

        private static void Case_00509()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 509,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,16,48,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,6,27,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-18,21,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,6,66,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,15,87,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,4,21,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-13,56,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,2,45,32,4), new GeneratedEnemyUnit(0,5,15,5,3), new GeneratedEnemyUnit(-14,-1,47,4,3), new GeneratedEnemyUnit(15,-20,76,9,1), new GeneratedEnemyUnit(4,-18,67,37,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a597bc94db79a3c64dff0f5d5ac2e4f3237816632ca404fd6caf1460ec82b3f6");
        }

        private static void Case_00510()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 510,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-15,71,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,9,9,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,2,54,20,2), new GeneratedEnemyUnit(12,19,52,37,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "d3e999961b8f4af9a488de903fae4e392f04a0ae0b5d68c8dd2f977d481bd97a");
        }

        private static void Case_00511()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 511,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-4,87,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-12,99,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,7,93,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,5,91,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,13,61,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,10,24,4,4), new GeneratedEnemyUnit(-8,-12,72,32,2), new GeneratedEnemyUnit(2,-4,32,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "237601710c4352bff6c9ee20785a8893e8ade1a864e2d3facfa91ffb5cde261e");
        }

        private static void Case_00512()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 512,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,0,83,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-13,70,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-3,56,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-7,97,10,3), new GeneratedEnemyUnit(10,2,76,27,3), new GeneratedEnemyUnit(0,2,88,39,4), new GeneratedEnemyUnit(-3,11,83,7,2), new GeneratedEnemyUnit(-1,20,14,28,3), new GeneratedEnemyUnit(-15,-5,47,17,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "95feb2de224ced0cb1775344a05093cff7ab43a8524ddb17311476724624b34c");
        }

        private static void Case_00513()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 513,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-7,94,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-6,61,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-11,93,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-19,69,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,7,41,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,6,54,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,2,86,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-2,11,27,3), new GeneratedEnemyUnit(-2,-4,13,36,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "f8b08c681eded2c2dd9251eb96eff800ae36e013838fe704e007eb99864d6c6a");
        }

        private static void Case_00514()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 514,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-10,99,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,12,48,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-10,85,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-8,96,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,18,13,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-6,79,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,17,78,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-14,8,20,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "ada4d492bc85c2de46eb8693738cb76d1d7f2cc7e5b83e3bd81ea1785a6e0cba");
        }

        private static void Case_00515()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 515,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,8,54,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,10,18,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,19,5,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-13,43,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-14,66,50,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "25d83d951ecc91f3f6dc995ceaa1397687a8c2e64942147457b1ef4eba1391f8");
        }

        private static void Case_00516()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 516,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-9,13,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,0,91,14,2), new GeneratedEnemyUnit(5,-13,92,32,1), new GeneratedEnemyUnit(18,-13,64,31,2), new GeneratedEnemyUnit(-17,-18,69,2,2), new GeneratedEnemyUnit(20,19,63,29,1), new GeneratedEnemyUnit(9,11,50,41,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "3918489232445e4588908d297d3a364a158a65213495ac0642fe5d88557f95fa");
        }

        private static void Case_00517()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 517,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,5,30,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-10,25,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,7,55,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-10,54,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-16,62,2,3), new GeneratedEnemyUnit(-12,-13,40,17,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e2fe9a6914c8d580add8690bb945658de283f55f66561b3e5a252696b6102aaa");
        }

        private static void Case_00518()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 518,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-20,85,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,4,52,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-20,41,14,3), new GeneratedEnemyUnit(9,13,93,44,2), new GeneratedEnemyUnit(17,-5,38,11,4), new GeneratedEnemyUnit(-8,-8,41,31,3), new GeneratedEnemyUnit(-3,-1,41,41,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "8984e07c7163a9f0e6f6db173216e89f0ec346498723d235544f166897e0f0f5");
        }

        private static void Case_00519()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 519,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,5,64,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,17,64,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,18,65,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-7,39,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,12,76,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,5,41,21,1), new GeneratedEnemyUnit(-20,15,10,40,1), new GeneratedEnemyUnit(-19,-15,20,18,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "53ae427ed0cc87bc964f1c6985e7e7d96beba416e70eae5080389421af11352c");
        }

        private static void Case_00520()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 520,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,13,88,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,16,11,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-13,65,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-8,49,5,1), new GeneratedEnemyUnit(15,0,43,10,1), new GeneratedEnemyUnit(-19,-11,92,24,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "b07c6b2e6b9ae2a0e10fae87e9a777d8062948852609683564def343fd20065f");
        }

        private static void Case_00521()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 521,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,18,69,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-12,8,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,9,60,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-16,83,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-3,80,4,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "8897882deeaf509d20adf591978ece87639eb6878203ce9e99c50a533c1be06c");
        }

        private static void Case_00522()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 522,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,7,38,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,3,33,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,12,30,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,9,16,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-2,37,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,5,8,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-10,93,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,8,73,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-19,96,8,2), new GeneratedEnemyUnit(-4,14,55,45,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "95ce773c6e9fee05f0604952c6d58a9c5691f0f520c2162c99f43471e21490b4");
        }

        private static void Case_00523()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 523,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-18,27,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,14,15,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-17,81,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,9,50,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-9,18,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,16,71,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-17,83,30,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "0e9b6f07269c07202946702025d4cc84f8b7aa1e461a43e8c15fc7e6afbe5156");
        }

        private static void Case_00524()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 524,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,16,78,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-10,89,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,20,16,44,3), new GeneratedEnemyUnit(-3,4,38,12,1), new GeneratedEnemyUnit(-4,-18,63,40,1), new GeneratedEnemyUnit(14,-9,38,48,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "15e8652cbaa968c74a4d013d87b155d463bfa10682fe2346ac90345d911dd7e8");
        }

        private static void Case_00525()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 525,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-2,9,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-2,81,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,3,39,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,18,15,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-8,44,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-20,87,4,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "6503e9397f447d266e80a7722d4658e0b975abb5c16fb7c93cb35023f22624c3");
        }

        private static void Case_00526()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 526,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-9,42,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-8,34,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-20,32,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-16,33,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,6,33,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,14,64,4,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "be0ebe5f9195ebc0b0806e097dfca2e049773b8395b07f12c0577d286a578e50");
        }

        private static void Case_00527()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 527,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-2,64,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,1,56,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-8,35,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,11,30,22,4), new GeneratedEnemyUnit(0,-16,58,3,2), new GeneratedEnemyUnit(-5,-17,44,45,3), new GeneratedEnemyUnit(11,6,98,38,2), new GeneratedEnemyUnit(11,-8,72,18,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "7ddd5d29e82e164ebb60a617e176903341d0185bb0d240cfcf5b0d826fca3299");
        }

        private static void Case_00528()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 528,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,10,63,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-2,20,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,8,36,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,9,44,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,18,7,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-20,98,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,6,20,45,1), new GeneratedEnemyUnit(5,-10,98,44,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "84c2fa820ddc704953677b9984e0bb7119fac5aef4935c3d41be9d58867a3441");
        }

        private static void Case_00529()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 529,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-16,17,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-18,81,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-8,69,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,15,98,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,17,60,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,20,21,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,9,23,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,0,25,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-6,7,48,4), new GeneratedEnemyUnit(2,-19,10,28,2), new GeneratedEnemyUnit(-20,14,85,49,2), new GeneratedEnemyUnit(-13,-3,30,49,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "4af7544781bf13434820cf7f6ece8e97fb76fc188d17f7f6acc377fb8fbedb93");
        }

        private static void Case_00530()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 530,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-1,7,3,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "3dc01a828ca116d70b90b62e6bae8bdcd352a0d9279e9a3b55d1f12c15429963");
        }

        private static void Case_00531()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 531,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,7,24,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,8,51,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,13,18,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-18,8,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-16,68,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,1,73,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-13,15,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,14,87,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,6,81,14,1), new GeneratedEnemyUnit(14,3,47,35,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "327090175ed5c0ec02af3724ac049dba33e1b617c5e12acfb1ee1289c84c1c8e");
        }

        private static void Case_00532()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 532,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,0,93,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-6,54,40,2), new GeneratedEnemyUnit(18,-8,33,49,3), new GeneratedEnemyUnit(4,-5,14,28,4), new GeneratedEnemyUnit(-14,2,61,6,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b45fa5a994a3d6a52f89e922335bd879ffbee84c1114511053b7da5b2b5b7089");
        }

        private static void Case_00533()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 533,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-11,32,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,17,79,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-20,5,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,9,49,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-8,90,46,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9ffbe0e4dd455dd7ef4778cbc620e0d4aadbdf0603598eff89677c32e32bd343");
        }

        private static void Case_00534()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 534,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-8,27,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-13,100,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,4,7,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-8,31,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,2,49,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-12,91,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-3,24,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-20,10,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,1,43,7,1), new GeneratedEnemyUnit(-4,1,58,49,3), new GeneratedEnemyUnit(-20,8,7,18,1), new GeneratedEnemyUnit(-3,16,7,15,4), new GeneratedEnemyUnit(-10,-4,29,22,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "f8a79c874dfd020d360732137bbfcdd69c68e71e0f6a9b10807659a80b7cd203");
        }

        private static void Case_00535()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 535,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-11,46,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,3,7,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,14,39,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-2,82,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,3,93,32,1), new GeneratedEnemyUnit(-19,7,43,27,1), new GeneratedEnemyUnit(-13,16,14,5,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "5637373c9c3c40015df40c0087825c75e19a30ec8db1875058479a7ff7b43505");
        }

        private static void Case_00536()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 536,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-19,47,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-19,89,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,6,81,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,8,6,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-15,19,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-13,99,11,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9b55c97fda10396e0768efb64592b4e43085d912320dda48181752d557e4ab8c");
        }

        private static void Case_00537()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 537,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,2,31,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-9,81,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-15,98,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,18,68,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-19,13,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-8,46,15,1), new GeneratedEnemyUnit(6,3,22,16,2), new GeneratedEnemyUnit(0,-14,65,20,1), new GeneratedEnemyUnit(1,-10,22,39,4), new GeneratedEnemyUnit(-8,10,50,7,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "5cb3701371db85edab3d4fa77b28e0a0276fbea59379ea52173a682142985cbe");
        }

        private static void Case_00538()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 538,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-17,53,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-8,49,32,3), new GeneratedEnemyUnit(13,18,93,8,4), new GeneratedEnemyUnit(-9,-16,82,17,3), new GeneratedEnemyUnit(-16,-19,13,37,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "f4b795bf7c315997914de28ab48f0e8bbd46271ffd5cd81998fef72d42dc9f0e");
        }

        private static void Case_00539()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 539,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-18,13,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-7,75,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,5,5,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-4,58,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,18,85,33,4), new GeneratedEnemyUnit(10,20,94,27,4), new GeneratedEnemyUnit(-7,-17,90,49,1), new GeneratedEnemyUnit(-5,2,81,20,3), new GeneratedEnemyUnit(-16,10,84,10,4), new GeneratedEnemyUnit(9,-2,22,39,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "3a81be0f4910b8d7664d58f3d4e4e20f829150054e2a0e9a934dd4487fa8ef6c");
        }

    }
}
