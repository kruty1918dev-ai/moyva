using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard020
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_03600();
            Case_03601();
            Case_03602();
            Case_03603();
            Case_03604();
            Case_03605();
            Case_03606();
            Case_03607();
            Case_03608();
            Case_03609();
            Case_03610();
            Case_03611();
            Case_03612();
            Case_03613();
            Case_03614();
            Case_03615();
            Case_03616();
            Case_03617();
            Case_03618();
            Case_03619();
            Case_03620();
            Case_03621();
            Case_03622();
            Case_03623();
            Case_03624();
            Case_03625();
            Case_03626();
            Case_03627();
            Case_03628();
            Case_03629();
            Case_03630();
            Case_03631();
            Case_03632();
            Case_03633();
            Case_03634();
            Case_03635();
            Case_03636();
            Case_03637();
            Case_03638();
            Case_03639();
            Case_03640();
            Case_03641();
            Case_03642();
            Case_03643();
            Case_03644();
            Case_03645();
            Case_03646();
            Case_03647();
            Case_03648();
            Case_03649();
            Case_03650();
            Case_03651();
            Case_03652();
            Case_03653();
            Case_03654();
            Case_03655();
            Case_03656();
            Case_03657();
            Case_03658();
            Case_03659();
            Case_03660();
            Case_03661();
            Case_03662();
            Case_03663();
            Case_03664();
            Case_03665();
            Case_03666();
            Case_03667();
            Case_03668();
            Case_03669();
            Case_03670();
            Case_03671();
            Case_03672();
            Case_03673();
            Case_03674();
            Case_03675();
            Case_03676();
            Case_03677();
            Case_03678();
            Case_03679();
            Case_03680();
            Case_03681();
            Case_03682();
            Case_03683();
            Case_03684();
            Case_03685();
            Case_03686();
            Case_03687();
            Case_03688();
            Case_03689();
            Case_03690();
            Case_03691();
            Case_03692();
            Case_03693();
            Case_03694();
            Case_03695();
            Case_03696();
            Case_03697();
            Case_03698();
            Case_03699();
            Case_03700();
            Case_03701();
            Case_03702();
            Case_03703();
            Case_03704();
            Case_03705();
            Case_03706();
            Case_03707();
            Case_03708();
            Case_03709();
            Case_03710();
            Case_03711();
            Case_03712();
            Case_03713();
            Case_03714();
            Case_03715();
            Case_03716();
            Case_03717();
            Case_03718();
            Case_03719();
            Case_03720();
            Case_03721();
            Case_03722();
            Case_03723();
            Case_03724();
            Case_03725();
            Case_03726();
            Case_03727();
            Case_03728();
            Case_03729();
            Case_03730();
            Case_03731();
            Case_03732();
            Case_03733();
            Case_03734();
            Case_03735();
            Case_03736();
            Case_03737();
            Case_03738();
            Case_03739();
            Case_03740();
            Case_03741();
            Case_03742();
            Case_03743();
            Case_03744();
            Case_03745();
            Case_03746();
            Case_03747();
            Case_03748();
            Case_03749();
            Case_03750();
            Case_03751();
            Case_03752();
            Case_03753();
            Case_03754();
            Case_03755();
            Case_03756();
            Case_03757();
            Case_03758();
            Case_03759();
            Case_03760();
            Case_03761();
            Case_03762();
            Case_03763();
            Case_03764();
            Case_03765();
            Case_03766();
            Case_03767();
            Case_03768();
            Case_03769();
            Case_03770();
            Case_03771();
            Case_03772();
            Case_03773();
            Case_03774();
            Case_03775();
            Case_03776();
            Case_03777();
            Case_03778();
            Case_03779();
        }

        private static void Case_03600()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3600,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-6,40,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-9,39,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,7,55,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-20,25,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,2,36,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-13,77,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,4,27,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-14,19,8,2), new GeneratedEnemyUnit(11,-9,75,44,4), new GeneratedEnemyUnit(-4,11,63,50,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "86bd5a393ede395dec11a4abcb90c407f23ef8519d8b9fa903c176832129724f");
        }

        private static void Case_03601()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3601,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,7,93,6,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "16ee37fac1331af79cbd63f5a44e2ad24b38ca39d60a1cc1ccf0516b0c001502");
        }

        private static void Case_03602()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3602,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,12,9,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,0,58,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-2,30,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,5,79,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-15,87,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,3,35,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,20,92,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-8,44,28,2), new GeneratedEnemyUnit(4,-18,67,6,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "204e5daf4631fd7d109bd2c4c3d6daa13449982d19ec97894fe0c5f74c95c605");
        }

        private static void Case_03603()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3603,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,7,8,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-5,8,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,0,65,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,2,96,32,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "4d2e60fed90a5b7a7b14e1e242aa06ce44e8f27a3a4213d17504b488c3e56ec2");
        }

        private static void Case_03604()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3604,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,2,87,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-9,76,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-5,85,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,14,55,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-14,29,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,4,60,7,4), new GeneratedEnemyUnit(5,-15,55,16,2), new GeneratedEnemyUnit(-2,-5,11,17,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "a63aa8f68c0e537474c8b562b5777e6d7794300fedab6f658abd8d237cd1420d");
        }

        private static void Case_03605()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3605,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,17,11,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-10,11,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,8,66,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,18,11,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-12,49,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,1,24,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-17,5,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,12,9,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,7,94,47,1), new GeneratedEnemyUnit(16,12,61,28,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a5b520c26a8f60d2de3317c4159c5b292957c11d0fca78326da6e74e35285933");
        }

        private static void Case_03606()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3606,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,15,92,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-6,48,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-12,97,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-3,87,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,17,13,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-11,69,9,1), new GeneratedEnemyUnit(-7,15,51,21,3), new GeneratedEnemyUnit(10,13,100,42,4), new GeneratedEnemyUnit(-14,8,26,18,2), new GeneratedEnemyUnit(-2,14,89,26,2), new GeneratedEnemyUnit(9,-10,42,7,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "5745907fa67673b2cd4809a04b0980b949604ebd6a33fd8959b230621100303d");
        }

        private static void Case_03607()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3607,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-1,40,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,1,96,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,5,69,16,3), new GeneratedEnemyUnit(-9,-11,53,25,4), new GeneratedEnemyUnit(18,11,74,50,2), new GeneratedEnemyUnit(-17,17,68,13,3), new GeneratedEnemyUnit(20,3,48,1,4), new GeneratedEnemyUnit(-11,-4,52,29,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "6e8c31f4b9871430f9c283502d90f64f002c0b996d5f920e1d6170c1146940f7");
        }

        private static void Case_03608()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3608,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,17,47,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,10,89,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,15,68,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,4,29,50,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "e547ebce13e61865d2dcf92a9b1904bb0f9b6e99ae8218cd95c4f31f4e7b1ec9");
        }

        private static void Case_03609()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3609,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,7,15,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-9,43,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,18,95,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,12,31,29,1), new GeneratedEnemyUnit(8,7,97,14,1), new GeneratedEnemyUnit(11,-1,22,20,4), new GeneratedEnemyUnit(14,-5,15,21,4), new GeneratedEnemyUnit(10,-20,42,3,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "c8e080350b95bc22c34b67532c9baa1fae2bc06c877ef3ddb1e59829c135db4c");
        }

        private static void Case_03610()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3610,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-18,94,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-20,5,48,4), new GeneratedEnemyUnit(-14,8,59,10,2), new GeneratedEnemyUnit(-1,17,59,15,2), new GeneratedEnemyUnit(-3,11,43,20,1), new GeneratedEnemyUnit(-18,11,60,12,2), new GeneratedEnemyUnit(-20,20,81,39,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "aa8d0da78ed988c672990a377bf5a5f20a8b2b97ec7faa5672635b94e9d191df");
        }

        private static void Case_03611()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3611,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-19,54,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,11,24,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,1,43,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-10,85,24,2), new GeneratedEnemyUnit(4,19,97,31,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "51edcd35f5b2e3299926137ad74c72c41833fafde82029e67afdcba39bc877c1");
        }

        private static void Case_03612()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3612,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-7,75,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-5,50,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,14,67,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-15,33,10,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c61aa7d7dc027d2f069bf26f8174a4486d9756379eb741d799e3aa3c23049cbe");
        }

        private static void Case_03613()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3613,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,19,64,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,15,8,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,12,56,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,19,14,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-14,77,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-11,75,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,17,88,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,19,71,4,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "911a20db0a853ffdc51b97408047dc95648f64a03a1a9a5b9b0178646b1dbefc");
        }

        private static void Case_03614()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3614,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-7,16,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,14,13,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-3,100,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,8,86,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,4,91,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,9,99,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-14,67,29,4), new GeneratedEnemyUnit(5,0,20,4,4), new GeneratedEnemyUnit(-15,-15,5,28,1), new GeneratedEnemyUnit(-13,-2,14,9,2), new GeneratedEnemyUnit(8,-10,79,9,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "4f357f55ce40941d8916792ab20232bc337aa8fa91ef72e992c6048babc931b5");
        }

        private static void Case_03615()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3615,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,4,15,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,6,47,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-12,30,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,9,64,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,0,89,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,7,99,17,1), new GeneratedEnemyUnit(-18,8,46,8,1), new GeneratedEnemyUnit(-16,9,56,11,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "ef743d514bffcfd8c304b413862809dd3d94292e54e3b6b29f30d64ed35c72c9");
        }

        private static void Case_03616()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3616,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,3,41,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-14,43,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-6,44,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-8,70,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-15,50,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,18,99,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,19,96,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-11,49,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,5,6,42,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "80399e7fb56a25a8b3907dde2137d6526e8d983b5665b2a336182010f3c76e86");
        }

        private static void Case_03617()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3617,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-8,83,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-7,5,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,15,66,23,2), new GeneratedEnemyUnit(-20,16,83,24,3), new GeneratedEnemyUnit(11,-16,14,26,1), new GeneratedEnemyUnit(11,-17,49,49,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "9fa70757c06560f3f2ee1c63b916c3b6cf4e131961552cb606e69620d633a6e6");
        }

        private static void Case_03618()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3618,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,19,68,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,20,65,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,13,66,3,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "b4bf8a8c7845e65adc21b7f7c1007e5f65420f36f7cf8dbccfbd93d2c2a0c2e7");
        }

        private static void Case_03619()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3619,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-8,30,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,19,53,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,15,50,28,1), new GeneratedEnemyUnit(-16,14,66,22,1), new GeneratedEnemyUnit(-1,19,47,4,1), new GeneratedEnemyUnit(19,7,83,20,1), new GeneratedEnemyUnit(4,3,49,18,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "cedabbb05f0f95a65d5b201c0a907f4dc542d2a1f1e788a2e3b2e5a57a984682");
        }

        private static void Case_03620()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3620,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,2,72,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-7,22,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,7,17,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-19,25,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,4,65,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-4,52,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-13,48,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-15,88,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,0,74,17,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "d621b09915ec2dd3adc955a30e7a0c18de21ffc45e55e8103657beb8d064097f");
        }

        private static void Case_03621()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3621,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-12,72,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-12,50,24,2), new GeneratedEnemyUnit(18,-5,77,18,3), new GeneratedEnemyUnit(5,-9,41,9,1), new GeneratedEnemyUnit(-14,3,16,20,4), new GeneratedEnemyUnit(7,-19,84,43,1), new GeneratedEnemyUnit(6,10,16,29,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "bcd05ee0f4e8b9d308979083e4f7156f5cf07a0f88fa12e74ec51afa1c5b1953");
        }

        private static void Case_03622()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3622,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,3,61,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-17,50,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,6,56,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,19,73,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-2,77,3,3), new GeneratedEnemyUnit(-3,6,31,27,4), new GeneratedEnemyUnit(-3,-18,43,7,1), new GeneratedEnemyUnit(-9,-15,93,26,4), new GeneratedEnemyUnit(5,-4,94,32,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "96653d7b379afaa6680a19ca66d42869a7ba4974fb5e5f407a7e891582e7c3c9");
        }

        private static void Case_03623()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3623,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,16,82,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,17,29,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-13,43,25,2), new GeneratedEnemyUnit(7,-10,70,45,4), new GeneratedEnemyUnit(-3,-6,54,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "4b05ed3574616065d68e0dc5cff765b970dcbb7b48ff7a39b0d9e995dd64347d");
        }

        private static void Case_03624()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3624,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,18,20,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,19,74,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-17,97,4,2), new GeneratedEnemyUnit(18,0,23,48,3), new GeneratedEnemyUnit(10,0,82,37,2), new GeneratedEnemyUnit(-2,-15,55,4,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "cde25d5b752d3da9fbd2fa85b17fd5d020d9b03c928774bf6cefc996bb8fd3dd");
        }

        private static void Case_03625()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3625,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-1,44,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,12,71,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-16,23,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,19,11,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,11,33,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,8,41,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-16,26,3,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "f55e99da6dae39d1ce772951428d7a34e13bbb2ecbe4309b6e939ec7a3e183df");
        }

        private static void Case_03626()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3626,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,8,6,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-3,31,1,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "3380a2b5e8103707ad56d17f1fd2fc44b7db2820026c36073c13866d645be32b");
        }

        private static void Case_03627()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3627,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,3,33,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-5,51,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-3,37,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,6,59,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-3,83,8,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0ce1c49e1aee48df350afe90ad24c6205af8bb2e9ecf8c62566760b8fea4f2a9");
        }

        private static void Case_03628()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3628,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,19,82,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,18,66,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,6,73,46,1), new GeneratedEnemyUnit(-3,-17,85,8,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b49b6dff109d57043c04a2fe065946e84872d4a61dadbff2be5d5f34f4c87dcf");
        }

        private static void Case_03629()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3629,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-10,44,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-14,75,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,20,95,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,12,47,2,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "6f00fadcf04c372c6b02c5302fc3573d4239082933d585a36f89fa7bc1cdf9b1");
        }

        private static void Case_03630()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3630,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,15,50,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,15,17,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-20,29,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,3,17,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,17,67,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-19,21,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,0,60,4,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "7eaf2b508c1681d9322d36f9fd983ea4bf37db6b8a79419ebbbcd15db90b8c3c");
        }

        private static void Case_03631()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3631,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,8,13,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,13,79,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-6,74,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-13,83,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,8,31,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-13,41,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,11,90,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,6,68,30,4), new GeneratedEnemyUnit(5,7,93,15,2), new GeneratedEnemyUnit(-2,-8,55,42,3), new GeneratedEnemyUnit(-13,18,100,21,2), new GeneratedEnemyUnit(-1,4,97,49,2), new GeneratedEnemyUnit(0,-14,30,37,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "a2238f88784f83cf421ba0f27c78217bb35067d084ae648c03a760d4aa07f948");
        }

        private static void Case_03632()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3632,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-15,67,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,20,100,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,12,51,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-2,11,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-1,66,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,5,90,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,4,46,36,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "321df8c4ae0ce296f26f194734ca51c7f736eea58f4a59dc1f72353d91f8e7f2");
        }

        private static void Case_03633()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3633,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-18,59,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-2,94,7,2), new GeneratedEnemyUnit(10,-12,82,26,2), new GeneratedEnemyUnit(15,-16,86,32,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "3ee1bc16db8c14ab9e412f57581dd6c57b7fff78ff76275f59584640d49ffac5");
        }

        private static void Case_03634()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3634,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,18,32,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,2,89,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,16,92,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-17,94,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-4,56,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-7,39,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-17,33,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,2,79,43,2), new GeneratedEnemyUnit(-20,-10,38,18,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "89d891aa3a066790c6b3df4b51987dbb11350feed6782c80edeff730b8ffb861");
        }

        private static void Case_03635()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3635,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,15,12,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,18,85,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,1,12,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,1,40,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,9,36,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-10,6,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-3,75,6,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "21009a4336bdfa23c523e7b416f5c5f0575c7b1d2cd372be49aff1e52f09de64");
        }

        private static void Case_03636()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3636,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-16,26,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,6,72,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,3,56,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-16,47,46,3), new GeneratedEnemyUnit(2,18,22,5,2), new GeneratedEnemyUnit(-12,-1,11,24,3), new GeneratedEnemyUnit(-6,-14,18,44,1), new GeneratedEnemyUnit(7,2,69,40,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "3bc305220c90ce71faf7ab13d964ff6b84f52093987d0b00cdad3f82ccae4e97");
        }

        private static void Case_03637()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3637,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-18,8,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,1,57,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,18,27,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,9,32,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,14,5,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-10,64,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-1,7,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-11,30,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "65b56fbc45ce4fd3d7ef81b82cd1fd4c8d95a8bcb357fdf249895ea81ddd846f");
        }

        private static void Case_03638()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3638,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,20,72,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-4,99,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,16,63,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-9,28,5,1), new GeneratedEnemyUnit(13,-5,9,27,3), new GeneratedEnemyUnit(16,-5,71,19,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "5967fbe1bc84c2077dd28ceb71fe515ec0d86a33d279472a56da00ad04d83a34");
        }

        private static void Case_03639()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3639,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-1,45,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-12,51,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,16,11,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-10,59,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,15,97,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-1,85,40,4), new GeneratedEnemyUnit(2,-17,15,42,3), new GeneratedEnemyUnit(15,10,87,28,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "1cca8dc3cf26367ae090d970d2c90342c06f41f34280a039d0ce7de65771d2fc");
        }

        private static void Case_03640()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3640,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-6,52,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,1,55,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,15,60,38,4), new GeneratedEnemyUnit(-1,-7,64,37,2), new GeneratedEnemyUnit(-1,-20,52,41,4), new GeneratedEnemyUnit(1,-10,8,1,4), new GeneratedEnemyUnit(18,-16,25,10,3), new GeneratedEnemyUnit(-12,-14,48,2,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "6f83f2d627713f5f5e3cf6bd9495349fbc99ae734438875a95860a30bac59a74");
        }

        private static void Case_03641()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3641,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,13,48,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-13,72,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-11,16,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,11,23,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,9,33,35,1), new GeneratedEnemyUnit(0,-10,9,35,3), new GeneratedEnemyUnit(10,-1,57,15,3), new GeneratedEnemyUnit(-19,16,86,24,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "635e4dc0e309c5e81adb9f213e77b12434c9198a79f26d7f2435cc03e2fb193e");
        }

        private static void Case_03642()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3642,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,8,82,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-2,25,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-19,58,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-12,89,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,17,52,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-14,28,25,2), new GeneratedEnemyUnit(18,-19,18,17,1), new GeneratedEnemyUnit(-12,5,24,5,3), new GeneratedEnemyUnit(20,-3,69,19,3), new GeneratedEnemyUnit(8,20,22,27,2), new GeneratedEnemyUnit(11,17,82,25,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5a86ba9bc90b2d0a2c8f65cfafd86166dd6c63814007725ec3d8f5e347907265");
        }

        private static void Case_03643()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3643,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,11,61,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,19,53,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-1,70,28,3), new GeneratedEnemyUnit(16,-17,100,26,1), new GeneratedEnemyUnit(15,0,97,41,3), new GeneratedEnemyUnit(-13,13,10,9,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "661cc96aab649132a74392046d17ede08afbb400d3acfad8fa0c6d90ec73eed4");
        }

        private static void Case_03644()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3644,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-19,7,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-11,49,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,10,61,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,12,73,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-13,33,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,16,28,3,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "1450577deb3437bee328069eac3bd4de2ec322f205980dc2b9b0409cf7b27dfb");
        }

        private static void Case_03645()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3645,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-5,93,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,4,51,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,14,90,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,6,84,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,9,10,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,3,90,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-6,28,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,11,85,13,4), new GeneratedEnemyUnit(1,-5,54,6,3), new GeneratedEnemyUnit(-2,9,54,47,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "f9d761d26d73723e49bf76ee86f46f02d932da1ebd8273370042df38b4bfebdf");
        }

        private static void Case_03646()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3646,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,13,95,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,5,91,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-6,70,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-14,73,2,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "cd6334083a50557ec1a80df0b5290b7238d7021593d42b811e51fcd73b9bfecc");
        }

        private static void Case_03647()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3647,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,11,49,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-10,38,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,20,12,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-13,93,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,20,37,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-2,56,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,20,42,3,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "542938482df339c0427f9acee9ae55c20429b173d7d0dfde6d9955a4c36d7473");
        }

        private static void Case_03648()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3648,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,13,99,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,8,71,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-14,67,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-12,31,5,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "126176b4fb6eb73ada1c5c7c8c6016a16552c299caf3bcf9376e787cbf329430");
        }

        private static void Case_03649()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3649,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,8,9,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-6,52,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-6,50,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-16,37,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-15,77,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,8,83,15,1), new GeneratedEnemyUnit(-4,7,79,35,3), new GeneratedEnemyUnit(-18,8,8,16,1), new GeneratedEnemyUnit(3,1,80,30,3), new GeneratedEnemyUnit(-5,-2,68,23,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "5fd6340db0df0e6ad9f228f91ba86c24a3085eeeac8046fc76a7010d5e9f02b9");
        }

        private static void Case_03650()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3650,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,14,37,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-7,69,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,15,68,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,4,63,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-4,90,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-18,97,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,11,79,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,19,22,2,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "ec35d89c65982b380d27bc75436aabc92ea02e7c735c5689dbfc43fd44f77f12");
        }

        private static void Case_03651()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3651,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,3,12,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,5,16,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-19,36,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-8,31,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-3,33,5,4), new GeneratedEnemyUnit(-8,10,66,42,2), new GeneratedEnemyUnit(8,-6,70,18,4), new GeneratedEnemyUnit(3,2,20,22,3), new GeneratedEnemyUnit(-17,1,38,5,1), new GeneratedEnemyUnit(-8,-15,36,18,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "d5ebadca030c2bc727126894b64b5d4444a983921124df677a779069fa20d45b");
        }

        private static void Case_03652()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3652,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-14,82,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,8,55,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,7,82,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,17,60,5,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "1c90d6f3ab99ecfb57a3e8dba91bcaed89b907143f3095a8b3fb89b6c4af8263");
        }

        private static void Case_03653()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3653,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-4,52,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,14,64,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,17,83,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-20,60,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,4,45,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,13,65,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,2,68,33,3), new GeneratedEnemyUnit(-3,2,17,35,1), new GeneratedEnemyUnit(-10,-9,50,4,1), new GeneratedEnemyUnit(-4,18,60,6,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "8d0369fe92b762fcb2b9b598cdb2093eda35653bfd8b9593dc00a4bac7a73b03");
        }

        private static void Case_03654()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3654,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-4,78,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,18,33,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-9,35,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-11,96,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,19,65,7,4), new GeneratedEnemyUnit(-12,-13,58,11,3), new GeneratedEnemyUnit(-16,3,92,6,3), new GeneratedEnemyUnit(-20,8,79,17,3), new GeneratedEnemyUnit(-1,12,74,23,3), new GeneratedEnemyUnit(-8,-6,96,3,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "4042a266d085237f7442fcbd7b04067543abc2f4a3f4bc027eb840ac9a79d8ef");
        }

        private static void Case_03655()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3655,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,9,35,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,8,30,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,12,84,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,3,83,10,2), new GeneratedEnemyUnit(-1,-4,7,16,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "af0ba97dae7ff233f53d769851d5a479c2a9f6cc7142bb6f943628ef29ffd74b");
        }

        private static void Case_03656()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3656,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,11,87,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-12,53,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-19,68,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-10,9,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,10,95,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,14,18,3,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "6c7ff0f78b247502e0e9dd9acdfd83a952876687fd3d0782faeda8c82229d4a1");
        }

        private static void Case_03657()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3657,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-11,43,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-3,96,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,10,61,13,4), new GeneratedEnemyUnit(-1,17,98,24,2), new GeneratedEnemyUnit(-18,-1,27,24,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "6e9979066222b1eed42974a9e82bccf60f53b5abb9e4c78da2f3f6eaae2800f5");
        }

        private static void Case_03658()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3658,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-8,81,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,2,19,40,2), new GeneratedEnemyUnit(16,-5,8,29,4), new GeneratedEnemyUnit(-17,-12,13,23,4), new GeneratedEnemyUnit(4,19,20,13,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "75f359e41be02a6b2bef3bbaa09b3337a4ad63c15b054581691095218b700d34");
        }

        private static void Case_03659()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3659,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,16,94,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,12,99,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,14,93,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,3,50,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "8710e126d8ccf9b682692b31d35f4ee3b40101d4481a00b7c9201d235f545d8b");
        }

        private static void Case_03660()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3660,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-7,79,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,3,76,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-2,28,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,20,65,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-19,32,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-17,42,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-19,56,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-5,61,21,4), new GeneratedEnemyUnit(1,18,38,15,2), new GeneratedEnemyUnit(-20,2,57,39,4), new GeneratedEnemyUnit(14,-9,43,33,3), new GeneratedEnemyUnit(-19,-13,98,13,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "c32ec67b7bed2e5d96dc28bfc64adba017faa4f644802a16832af7b511f8f2a3");
        }

        private static void Case_03661()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3661,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,20,50,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,15,40,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-16,92,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-2,99,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-3,71,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,1,65,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,19,8,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,6,65,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-14,41,17,3), new GeneratedEnemyUnit(-9,5,12,5,3), new GeneratedEnemyUnit(-3,-9,59,42,3), new GeneratedEnemyUnit(13,7,49,3,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "3f34b46f84902a52abd1a19405db1fcc06a49d213af8d8d0a999c81143d85f02");
        }

        private static void Case_03662()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3662,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-18,36,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-11,74,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,10,25,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-14,47,31,4), new GeneratedEnemyUnit(18,-15,68,35,4), new GeneratedEnemyUnit(-8,-10,62,4,1), new GeneratedEnemyUnit(-14,3,98,42,4), new GeneratedEnemyUnit(18,13,93,4,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a7d7b5b09116c5df8639fa0830d3380d503e76bf00ffc937725661ecc609ba76");
        }

        private static void Case_03663()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3663,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-11,56,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,19,69,27,4), new GeneratedEnemyUnit(-4,-12,31,9,1), new GeneratedEnemyUnit(9,9,99,37,2), new GeneratedEnemyUnit(-8,-19,9,16,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "cc136769a50f226064e33056b6e47511848f3905562daa0401ae278b89988a8e");
        }

        private static void Case_03664()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3664,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,15,86,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-5,60,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-17,45,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-19,91,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,19,71,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-5,7,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,8,27,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,20,65,13,3), new GeneratedEnemyUnit(-4,-3,89,37,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "5deb51b946350b28d2023e058bf3eb235833678f3538a1d1fec6d5c1ec3f434f");
        }

        private static void Case_03665()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3665,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-15,95,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,1,54,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-16,46,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-19,100,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,18,67,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,7,91,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,19,43,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,4,79,46,1), new GeneratedEnemyUnit(14,-17,88,9,3), new GeneratedEnemyUnit(11,17,36,37,3), new GeneratedEnemyUnit(0,-14,50,24,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "fa17f14122d5ab7c3283987343e9295fa25da06f37078a0bc9a72d17f8f96547");
        }

        private static void Case_03666()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3666,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,17,6,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-12,39,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-15,17,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-5,52,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-16,8,13,3), new GeneratedEnemyUnit(12,8,60,3,4), new GeneratedEnemyUnit(10,-5,90,5,1), new GeneratedEnemyUnit(-10,-9,28,28,2), new GeneratedEnemyUnit(-6,12,10,26,2), new GeneratedEnemyUnit(-13,-13,82,14,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "7f67c5ce5ab3c86c72b33c4a92363563f5b1bcd5f1a2146bfcb69ce0ae9f4c9f");
        }

        private static void Case_03667()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3667,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,13,40,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-15,87,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,18,76,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-2,63,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,16,27,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,9,10,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-5,6,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,20,12,3,4), new GeneratedEnemyUnit(-12,-12,10,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "601a7d4c81bb71c679fc2cb8769b00fb47b7b4e357b6498333762ad46152672e");
        }

        private static void Case_03668()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3668,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,17,91,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,0,37,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,4,34,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,3,18,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-20,14,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-13,13,18,1), new GeneratedEnemyUnit(-19,-17,83,32,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "28661fd72584b664b38c70cf88a2512a6b82f8a38ce3aed7b45463c71180b0c5");
        }

        private static void Case_03669()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3669,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,4,43,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-17,92,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,19,70,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,19,82,14,3), new GeneratedEnemyUnit(14,5,86,37,4), new GeneratedEnemyUnit(1,6,27,14,2), new GeneratedEnemyUnit(-16,-3,79,6,1), new GeneratedEnemyUnit(4,20,19,8,2), new GeneratedEnemyUnit(13,20,37,23,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "7c7e2e47651fc08cd4336aa4596ddca29261df45f98e23e5f009d0400c9adef8");
        }

        private static void Case_03670()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3670,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-14,22,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,6,82,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,19,86,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,9,36,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,3,93,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-13,50,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,5,10,25,2), new GeneratedEnemyUnit(-7,5,84,44,4), new GeneratedEnemyUnit(-12,-2,94,23,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "15022e11726a3edb0cfb60e12b790d264589c637c6264ad783d7bdce53ea6585");
        }

        private static void Case_03671()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3671,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,12,97,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,17,14,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,13,75,3,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "82ca662b102afb339898c5fe5d9f318e633a87fc886dc08419b1a80687663009");
        }

        private static void Case_03672()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3672,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,10,53,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,19,94,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-8,67,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,5,59,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-19,10,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-7,90,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,11,21,24,3), new GeneratedEnemyUnit(7,-6,86,18,2), new GeneratedEnemyUnit(4,-15,24,44,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "076524078cb36d18dcfa5a8c72bdb518e3b038422a05f5a60a500455fb6217ea");
        }

        private static void Case_03673()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3673,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,16,66,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,4,70,19,2), new GeneratedEnemyUnit(5,-12,79,49,2), new GeneratedEnemyUnit(20,-16,98,28,2), new GeneratedEnemyUnit(-10,-7,42,37,2), new GeneratedEnemyUnit(1,11,81,27,2), new GeneratedEnemyUnit(-12,-17,86,32,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "c5ea15df07273d63bf1ed2f9d161c4cba51108b24d3f23e8a9e0d97192fceb29");
        }

        private static void Case_03674()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3674,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-14,24,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-2,92,36,4), new GeneratedEnemyUnit(4,-10,31,22,1), new GeneratedEnemyUnit(-19,6,73,35,3), new GeneratedEnemyUnit(1,-7,64,14,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "ffafc6e5cfd2d8cc001ad6cbb480058e2b46aa6da1fbfa724c3954d1db305564");
        }

        private static void Case_03675()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3675,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-13,46,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-12,25,29,3), new GeneratedEnemyUnit(-17,-11,60,7,2), new GeneratedEnemyUnit(1,13,36,1,1), new GeneratedEnemyUnit(-7,-19,8,19,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "3b784f73621fd763f6cd077a06d95d883986e267aa1659805c6c513f260c2674");
        }

        private static void Case_03676()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3676,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,9,39,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,6,69,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-3,72,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-18,75,10,4), new GeneratedEnemyUnit(9,9,65,45,4), new GeneratedEnemyUnit(-11,11,43,46,2), new GeneratedEnemyUnit(-20,-2,77,28,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "13f53142896ac77f6eeb8851a9781ccbb55d18cb66d8711583e8ec953f5b17ff");
        }

        private static void Case_03677()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3677,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-12,20,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,17,76,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-6,38,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-1,11,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,13,66,19,2), new GeneratedEnemyUnit(10,-11,72,33,4), new GeneratedEnemyUnit(11,10,100,30,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "ae1a750e5e356f1754326546ce36ad52ecf2f7d0468c84b88db82a6f2c5f9676");
        }

        private static void Case_03678()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3678,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,7,10,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,19,80,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,18,45,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-3,37,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,6,59,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,0,38,26,1), new GeneratedEnemyUnit(-3,-2,94,40,4), new GeneratedEnemyUnit(12,-7,16,45,2), new GeneratedEnemyUnit(-12,-3,36,3,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "c2d8a867ca507a93f1d136b0e548b083b0a4a06e0cbd2c773159e1a2abcae0a8");
        }

        private static void Case_03679()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3679,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-11,76,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-7,91,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,14,57,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-1,99,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,5,57,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,6,12,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-8,99,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-7,22,28,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "145f0227fa53f05bf333f1efadcf3e8504a6e6348231a8316a2228d67023835f");
        }

        private static void Case_03680()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3680,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-7,91,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-12,80,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,2,84,7,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "4588ca2dbb4d214a38b2e251f3f36edcffc6b2e97a0b5338aa1931bb5b3aad6c");
        }

        private static void Case_03681()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3681,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-6,72,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,19,54,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-10,23,8,2), new GeneratedEnemyUnit(-18,-13,16,33,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "37b3e0eb4bca1a385e97f276a8025ec2cda39140800a91e6369adcea6e201e6c");
        }

        private static void Case_03682()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3682,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-1,93,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-8,62,36,4), new GeneratedEnemyUnit(12,-15,93,14,4), new GeneratedEnemyUnit(-8,1,97,42,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "1f1db021e7e319500c35ff4991147439e3fa112ef4fd9c88715965306fa6ff46");
        }

        private static void Case_03683()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3683,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,18,61,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,11,96,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-5,73,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,12,15,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-11,88,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-5,68,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,9,63,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-5,79,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,7,34,38,2), new GeneratedEnemyUnit(-11,0,51,3,3), new GeneratedEnemyUnit(-16,0,86,42,2), new GeneratedEnemyUnit(-6,14,35,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "11c77206e849e5f66f6b1600c113c5ecbc9885ced3296c0daa46d2fbd6c5495e");
        }

        private static void Case_03684()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3684,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,15,52,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,10,31,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,5,50,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,20,58,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-2,37,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,0,60,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,8,6,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-15,29,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,8,12,50,4), new GeneratedEnemyUnit(19,18,91,2,1), new GeneratedEnemyUnit(0,-20,61,36,1), new GeneratedEnemyUnit(16,-4,45,2,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "4f95d268dde675f543f080a75bc01f1a12912a37a2352d56e873f7b705bb20fa");
        }

        private static void Case_03685()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3685,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-18,73,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,5,56,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-20,7,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,13,11,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-16,49,19,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "d09879d42f5e3e3127f42cd67eb20009224d5d0973b5acf6a24fbc571f773d10");
        }

        private static void Case_03686()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3686,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-6,88,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-10,91,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,19,80,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-16,52,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,18,36,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-15,40,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,6,98,1,1), new GeneratedEnemyUnit(-4,-7,30,49,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "674188323d5b3a3d399b4edd2adc5e447ba4e0826df960ba5f062f526c813605");
        }

        private static void Case_03687()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3687,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,10,52,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,9,36,21,3), new GeneratedEnemyUnit(-20,20,11,1,4), new GeneratedEnemyUnit(2,16,72,49,1), new GeneratedEnemyUnit(1,-13,66,10,2), new GeneratedEnemyUnit(18,5,72,8,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "a284a7be29729a6ac4f636a5d13811348b6b65d0413669111b5298a8f25854b6");
        }

        private static void Case_03688()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3688,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,17,78,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,6,59,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,7,23,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,2,99,49,3), new GeneratedEnemyUnit(3,-1,97,20,3), new GeneratedEnemyUnit(8,2,87,18,2), new GeneratedEnemyUnit(17,-7,31,31,2), new GeneratedEnemyUnit(-13,18,75,24,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "420764f104f984dece93471bdbc4d909773fbbbf384a90f93dc66e44319ad856");
        }

        private static void Case_03689()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3689,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,12,36,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-6,52,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,17,94,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,15,88,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,3,6,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-18,93,31,1), new GeneratedEnemyUnit(-8,0,77,29,1), new GeneratedEnemyUnit(13,-15,61,1,1), new GeneratedEnemyUnit(15,-14,67,44,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "c49c867969da177b49421d990e465fb6199ca4700387a883887e154a80a625ec");
        }

        private static void Case_03690()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3690,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-15,100,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-9,55,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-13,89,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,11,80,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,14,37,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,3,34,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-20,11,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-16,20,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,1,82,37,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ec5331fc33e3729f7d3a2188b45e40d81ddf5e30d4a7eab1398c54f019e96951");
        }

        private static void Case_03691()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3691,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-7,68,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-5,68,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-16,15,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,7,17,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-12,75,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,17,44,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,7,68,14,3), new GeneratedEnemyUnit(-15,4,7,37,2), new GeneratedEnemyUnit(2,-1,85,14,3), new GeneratedEnemyUnit(-7,5,26,6,2), new GeneratedEnemyUnit(-4,-9,24,13,3), new GeneratedEnemyUnit(3,-1,25,42,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "bf7d0bba8c6ed816f8971a6c01aa48d6e1b72d638d0a35175f1bb7095b50e311");
        }

        private static void Case_03692()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3692,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-16,13,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-19,83,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-3,18,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,14,28,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-18,75,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,6,26,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-17,98,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,0,95,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,14,44,50,4), new GeneratedEnemyUnit(2,17,37,31,1), new GeneratedEnemyUnit(-14,1,69,37,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "10a8e44ce2fe8feede5426155d94fd981f972054bd107ef9cc83c1f0d915355d");
        }

        private static void Case_03693()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3693,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-1,90,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-5,11,9,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "2decfe15080766ecf0fecb728927e31f63052388adc770f003826aecc4356824");
        }

        private static void Case_03694()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3694,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,19,40,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,7,19,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-1,25,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,18,39,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,3,33,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-17,17,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,8,63,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-7,74,26,4), new GeneratedEnemyUnit(-16,-10,10,45,1), new GeneratedEnemyUnit(18,-16,98,17,1), new GeneratedEnemyUnit(-11,-10,66,2,2), new GeneratedEnemyUnit(-4,16,59,25,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "229f12712b9b106acf9a9ed17e065a1a2137cda529de66dd6c928ea95ad739e6");
        }

        private static void Case_03695()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3695,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,13,13,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-19,11,2,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "468a22382522d33e8b6e3c149ef6c7d1ba7ca02a7d75d7a899fdaf3f03d73d2d");
        }

        private static void Case_03696()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3696,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,14,14,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-19,8,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-16,93,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,6,48,5,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "76856971566764ca5878b414bb3c296afdf725714504c13554e57e90a1bf5a14");
        }

        private static void Case_03697()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3697,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,11,79,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-16,33,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-7,11,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,9,72,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,18,77,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-4,16,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,13,75,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,6,15,1,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "263573840f0f0912950bb0a165833e2376e410358117ff4a32d5b04b2022d5c0");
        }

        private static void Case_03698()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3698,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,14,46,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,15,41,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,10,64,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-3,60,39,1), new GeneratedEnemyUnit(-3,-20,77,11,1), new GeneratedEnemyUnit(-14,9,55,4,2), new GeneratedEnemyUnit(9,19,43,27,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "8943d84b223dc89db696d96bd7832cfd1d7a6fab704e4f722c3921bdb7a11302");
        }

        private static void Case_03699()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3699,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,2,11,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,17,51,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,10,48,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,10,35,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,8,65,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,20,16,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-6,14,35,3), new GeneratedEnemyUnit(9,-8,12,25,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "5d73929d1849c0d9c7135b6254aca2c279844fa8bedf263577c24d98706cc05e");
        }

        private static void Case_03700()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3700,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,4,80,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-15,62,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-3,54,41,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "997223a05d39d3aa410b0d1dec90f6243da27476fd17f1378b734f72f1a6ea58");
        }

        private static void Case_03701()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3701,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-12,61,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-7,100,15,3), new GeneratedEnemyUnit(12,-5,14,41,2), new GeneratedEnemyUnit(-2,-14,58,23,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "d3cdc3914144dc1d12206f7e82469024b083a94dda530eed1f6614644e48cafd");
        }

        private static void Case_03702()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3702,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,15,72,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-20,92,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-6,56,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,15,99,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,1,40,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,9,81,18,3), new GeneratedEnemyUnit(2,-3,84,45,1), new GeneratedEnemyUnit(4,-6,9,15,4), new GeneratedEnemyUnit(20,15,28,46,2), new GeneratedEnemyUnit(19,-4,96,38,4), new GeneratedEnemyUnit(-17,-7,60,9,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "e5175f0bf282e22ca79d0a4dbe119e6b09ea3cdc7075c74962cb572a5abd7c4d");
        }

        private static void Case_03703()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3703,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,20,88,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-15,45,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-15,5,31,2), new GeneratedEnemyUnit(11,15,37,28,4), new GeneratedEnemyUnit(-9,11,34,44,3), new GeneratedEnemyUnit(-10,7,30,29,3), new GeneratedEnemyUnit(-13,9,40,1,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "43db15521a71a9e06d25e6a2aa18b82c66edcc5c1747ba95661ab5f0b1459e52");
        }

        private static void Case_03704()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3704,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,15,9,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-15,61,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-2,95,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,4,27,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,18,39,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-14,79,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-6,78,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-3,82,43,1), new GeneratedEnemyUnit(4,-7,73,19,2), new GeneratedEnemyUnit(-10,-11,41,43,3), new GeneratedEnemyUnit(5,7,72,47,3), new GeneratedEnemyUnit(-9,-1,64,24,3), new GeneratedEnemyUnit(17,-16,84,23,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "31ca57916e83a5d87d356b63a7910b16cb30625679f1a7aa0accb051b8074b95");
        }

        private static void Case_03705()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3705,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-9,20,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-13,25,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-13,98,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-13,20,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-12,33,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-18,50,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-2,28,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-20,38,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-12,34,25,2), new GeneratedEnemyUnit(14,-5,84,32,3), new GeneratedEnemyUnit(-18,7,77,19,4), new GeneratedEnemyUnit(4,-10,66,11,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "997e043e5927bec289e5f0dee3ccc5a22b400b86816b73fd4d15b34a049e0f0f");
        }

        private static void Case_03706()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3706,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-3,48,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-14,73,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,13,74,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-4,79,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,1,57,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,17,11,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-19,81,25,4), new GeneratedEnemyUnit(9,-19,20,37,1), new GeneratedEnemyUnit(-14,2,89,33,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "992b55bca63a3f17a4c9e491accea1791ec45a66f812b9f922c8ff2b272c5036");
        }

        private static void Case_03707()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3707,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-15,34,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,12,48,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-20,100,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,6,74,15,4), new GeneratedEnemyUnit(-1,8,32,28,2), new GeneratedEnemyUnit(10,-4,48,39,4), new GeneratedEnemyUnit(-15,2,48,12,2), new GeneratedEnemyUnit(4,-19,83,47,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "22d28216fbc4e7584ccf97d038b5c44915ff8a308dc838c5fd79e2c0c66cd425");
        }

        private static void Case_03708()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3708,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,8,85,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-4,62,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-19,48,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,8,97,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-11,78,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,4,97,21,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "552a6f1bec5d226873ead62bf419f3b75503765115500a1beb52745929d89b31");
        }

        private static void Case_03709()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3709,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-15,13,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,7,5,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,7,10,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,7,58,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,6,48,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-12,43,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,19,84,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-1,26,38,2), new GeneratedEnemyUnit(-19,-16,66,2,3), new GeneratedEnemyUnit(-1,11,29,21,1), new GeneratedEnemyUnit(14,14,97,44,2), new GeneratedEnemyUnit(-18,10,80,31,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a258f2d2e1f6b609f42048a5a981eee75901780dd1bc8700ad53c949e51b1049");
        }

        private static void Case_03710()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3710,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,3,60,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-6,51,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,16,51,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,13,35,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,17,45,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,18,100,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-11,72,18,1), new GeneratedEnemyUnit(16,12,92,50,2), new GeneratedEnemyUnit(-14,-18,19,20,4), new GeneratedEnemyUnit(-6,-2,73,28,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "e2904544cd51caf59300ce45e783251ef24c37cb3bead914e741e784e60bd696");
        }

        private static void Case_03711()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3711,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-16,83,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,2,99,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,6,45,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-14,32,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-11,66,43,2), new GeneratedEnemyUnit(-5,-2,67,15,2), new GeneratedEnemyUnit(20,-10,13,24,1), new GeneratedEnemyUnit(7,9,30,23,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "e0a146710b82fe0d84e4b4650f76493d704aa514532acd702ed47c7e7200422a");
        }

        private static void Case_03712()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3712,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,18,33,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,5,28,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,14,9,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-13,47,37,4), new GeneratedEnemyUnit(-1,17,13,46,3), new GeneratedEnemyUnit(-12,19,16,16,2), new GeneratedEnemyUnit(18,8,85,21,2), new GeneratedEnemyUnit(-11,-7,54,25,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "e1119ca0a744a2445643e43681f9aba124e8cea0b6fefc32843e0f04941f1019");
        }

        private static void Case_03713()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3713,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,14,6,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-14,92,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-12,44,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,18,34,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-20,71,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,15,91,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "716c209b6174e5c97b8ded9a6a19559ae947bbed4452b8bc559ef5e344564070");
        }

        private static void Case_03714()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3714,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,8,97,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-7,7,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,4,67,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,5,85,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-16,77,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,16,59,7,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "d6ea936c6cfc628b6beb338b50babeac645b15732cc0c761face432abf64ab60");
        }

        private static void Case_03715()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3715,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,0,11,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,9,99,1,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ebf17dac64e295b1be86838fc61b35ca526be8e1ef008e558c324bbf3987d37e");
        }

        private static void Case_03716()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3716,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,8,15,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-18,90,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,10,44,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-13,41,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-16,18,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-2,78,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,18,11,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-12,94,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,9,52,15,3), new GeneratedEnemyUnit(11,-4,13,8,2), new GeneratedEnemyUnit(-18,-7,22,42,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "af5ea187cc159bd9759b941f496aef170fb998a6da4b88ff350299dda0975ba8");
        }

        private static void Case_03717()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3717,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-12,90,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-4,16,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,15,57,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,5,69,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-4,25,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-3,42,7,3), new GeneratedEnemyUnit(18,-12,74,8,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "023120239829d2d2cb9832cdc565d65baa14eff1599d3fd9dec1b5e0bfb2cc24");
        }

        private static void Case_03718()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3718,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-4,63,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,5,81,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,15,39,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-3,73,43,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "fd6acbecee53f7c32520b028c4f70ba58da10d81c39c54f97755d991a37141dd");
        }

        private static void Case_03719()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3719,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-1,91,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-13,96,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,8,32,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,1,22,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,3,91,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-1,85,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,2,6,31,1), new GeneratedEnemyUnit(13,6,19,16,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a9746c234e5e27f73a647c612ac9f50320fb78ee2dfef1cc95f93b41a8b41779");
        }

        private static void Case_03720()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3720,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-10,78,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,1,71,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-20,22,43,1), new GeneratedEnemyUnit(10,-18,76,49,3), new GeneratedEnemyUnit(10,15,13,36,1), new GeneratedEnemyUnit(15,-13,52,36,4), new GeneratedEnemyUnit(15,7,6,25,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5a6578bdfc9e2e4c3783b042e79576bb33dd48831527037cc4f5c7e88124185a");
        }

        private static void Case_03721()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3721,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,12,58,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-15,86,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,13,91,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-11,66,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,13,81,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-11,19,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,5,25,21,1), new GeneratedEnemyUnit(20,-3,73,35,4), new GeneratedEnemyUnit(18,8,64,21,4), new GeneratedEnemyUnit(12,16,82,46,2), new GeneratedEnemyUnit(1,-2,35,43,3), new GeneratedEnemyUnit(20,3,68,7,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "eee95acff0e2967b7d05aa5f2962698ababed39b4b7650cbbf0191ebcfb70b27");
        }

        private static void Case_03722()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3722,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,6,51,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,2,85,24,4), new GeneratedEnemyUnit(10,20,17,23,1), new GeneratedEnemyUnit(-10,-20,25,20,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "6d98cd5def3a11d2fb41bfeee3f0541020599ff0d58e6411cc79f4b2594d57de");
        }

        private static void Case_03723()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3723,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,14,22,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-12,77,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,0,89,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,10,26,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,14,48,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,7,97,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-2,49,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-17,70,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,9,29,29,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "6d8bd6ca85fa4730fed7051e8de7d6c81301ae875534de708ae32c746e94b5b8");
        }

        private static void Case_03724()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3724,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,2,65,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,16,90,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,18,32,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,17,45,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-1,79,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-4,53,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,2,65,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-2,52,28,4), new GeneratedEnemyUnit(2,-3,23,39,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "955b8a978b049a968f7c61aa1e214a73754cc68049d1a0c442e0f883ba7d91fb");
        }

        private static void Case_03725()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3725,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-12,35,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-10,24,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "6e87ce88dbd14d608c258a2fcdf969074e252117e5f607d1c4a3a0944af5d6de");
        }

        private static void Case_03726()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3726,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,18,87,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,11,86,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-8,50,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,3,75,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,0,47,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,15,31,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-17,13,13,3), new GeneratedEnemyUnit(10,-4,64,42,3), new GeneratedEnemyUnit(-18,-2,13,3,2), new GeneratedEnemyUnit(-17,3,7,33,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "67923295f6b2ebc08c98a56b765357b63b984de1c8b09ab5a5dbca6345f95180");
        }

        private static void Case_03727()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3727,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-7,83,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-19,12,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-15,56,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,10,100,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,13,63,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,4,21,25,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "34492a3b70629e7eb431a6ddf2a2f2cbd322bf10e901904fadd3a148d72ed0c2");
        }

        private static void Case_03728()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3728,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-7,27,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,11,27,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,8,59,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-8,84,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,12,43,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,3,27,47,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "59281ac8d558acc17625ed65ba9f97a4832f85854a278cb2807d41ac5f872e82");
        }

        private static void Case_03729()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3729,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-15,29,7,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "b0ebb6f58cd3820ce3d1e2baf94039b8dfdedc97a93f0929ba1c2f7153ba0efe");
        }

        private static void Case_03730()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3730,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-12,33,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,15,21,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,17,28,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,5,79,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,16,39,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,2,100,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-8,38,20,4), new GeneratedEnemyUnit(-13,1,39,43,3), new GeneratedEnemyUnit(-16,4,78,20,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "38eb3979c4e6064e842df2b8cec2cbb6b912ca6bca88b55487cd735803d9f34a");
        }

        private static void Case_03731()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3731,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,13,11,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,20,67,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,19,56,18,2), new GeneratedEnemyUnit(8,2,27,2,1), new GeneratedEnemyUnit(-18,-12,66,29,3), new GeneratedEnemyUnit(-7,19,42,28,3), new GeneratedEnemyUnit(7,7,7,16,1), new GeneratedEnemyUnit(-1,12,37,13,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "ac66e5d961c403cf25dbc6e5828eb9490eff44b302184dbd5c0d6b80e176587a");
        }

        private static void Case_03732()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3732,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,18,68,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,11,15,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,11,69,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-18,84,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,7,94,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,4,80,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,17,95,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-15,66,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-2,47,42,2), new GeneratedEnemyUnit(13,5,36,11,3), new GeneratedEnemyUnit(2,1,14,14,2), new GeneratedEnemyUnit(0,15,36,32,2), new GeneratedEnemyUnit(8,12,12,35,3), new GeneratedEnemyUnit(-2,-4,52,14,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "e21519ac3d31d30dad3fccc5e28637490685681a5732ddcc023cbd1ab6173298");
        }

        private static void Case_03733()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3733,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-14,96,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-12,31,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,2,99,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,6,25,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-12,68,33,2), new GeneratedEnemyUnit(1,-1,41,13,3), new GeneratedEnemyUnit(-4,-3,88,26,4), new GeneratedEnemyUnit(1,1,76,45,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "ff33347f355c39667e0da5965a163cd663048446248d4a5f2955dce60269a10e");
        }

        private static void Case_03734()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3734,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-14,49,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,15,77,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-1,65,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,12,42,3,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "7b6accc223bf075563a8ea9d090dda714f5732f70e2f94dc48584a789fa3834f");
        }

        private static void Case_03735()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3735,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-20,83,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-8,46,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,11,61,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,20,63,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-1,40,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-9,31,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-15,66,12,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "e661254c4f01d6f920f043694efe6b4a87c59831447f7ce2e0cd5c164f9152d4");
        }

        private static void Case_03736()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3736,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-12,79,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,15,90,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,14,29,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-1,92,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,7,72,40,4), new GeneratedEnemyUnit(14,0,52,14,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "48c062bf8c8caecc5e8eef074a825a1a991938ec12ae160b2499c54387ec16a8");
        }

        private static void Case_03737()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3737,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-4,36,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,11,67,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-19,48,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-6,23,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,20,31,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,3,60,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-12,42,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-10,24,49,1), new GeneratedEnemyUnit(12,18,39,36,3), new GeneratedEnemyUnit(-12,-19,26,50,3), new GeneratedEnemyUnit(5,-16,21,45,2), new GeneratedEnemyUnit(-2,0,43,40,4), new GeneratedEnemyUnit(-13,5,22,32,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "2d2e7f5acf6f76092bb05ac42bb5ba6934f4f68e477f3c6ad690291848290887");
        }

        private static void Case_03738()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3738,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-6,75,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-16,15,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-11,34,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-1,80,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-20,62,38,3), new GeneratedEnemyUnit(-8,20,70,31,4), new GeneratedEnemyUnit(-20,13,24,45,1), new GeneratedEnemyUnit(17,-16,55,17,1), new GeneratedEnemyUnit(-6,-6,61,27,3), new GeneratedEnemyUnit(-8,17,82,31,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "41be7d0c2bb9277406c15d9a5d85ec57af0b862466230e0a7bdaa65e9dc404d8");
        }

        private static void Case_03739()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3739,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-19,35,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,10,43,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,19,46,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-20,67,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-3,39,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-4,65,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-6,81,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,18,46,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-3,28,39,1), new GeneratedEnemyUnit(-19,7,94,45,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ae55882f57ee7839901e6fe96be7e7dd448fbf25bffc10785cc461cdb0306239");
        }

        private static void Case_03740()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3740,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,13,73,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,20,18,12,1), new GeneratedEnemyUnit(-14,0,47,36,4), new GeneratedEnemyUnit(-8,19,66,17,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "340f3fe59bc419ba2fc63b003b4d4af311ceec2615e83f25ae3d838753598be4");
        }

        private static void Case_03741()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3741,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,18,49,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,18,97,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,17,39,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,8,83,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,16,76,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,5,95,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-16,62,14,2), new GeneratedEnemyUnit(11,16,24,48,1), new GeneratedEnemyUnit(-12,-6,84,23,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "2c2f8d0bff57c36925d42d9141876eb953012b4e106a0f2cb36455eb2bc69e3a");
        }

        private static void Case_03742()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3742,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-19,61,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,20,74,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-19,36,46,4), new GeneratedEnemyUnit(4,9,37,27,1), new GeneratedEnemyUnit(11,18,52,39,3), new GeneratedEnemyUnit(12,-1,10,43,2), new GeneratedEnemyUnit(4,13,90,15,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "4fbddb4d8d13993d93e9d484c69ece1759b68562513f56b32eaff61e0e4ada65");
        }

        private static void Case_03743()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3743,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,20,10,3,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "c8d0453f48cd144c6beb0e065f3371ad7b23f984c5a13315af3383b9254903c2");
        }

        private static void Case_03744()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3744,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,19,93,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-15,10,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,9,60,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-12,37,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-4,99,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,7,44,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-18,62,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,20,93,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-5,14,37,2), new GeneratedEnemyUnit(4,-2,15,48,1), new GeneratedEnemyUnit(13,18,80,9,2), new GeneratedEnemyUnit(-14,9,14,9,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "20cca40a1f03468e85a698a4248444a8eb0e47e826aa670f7fb3e76a800d81de");
        }

        private static void Case_03745()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3745,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,15,16,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,10,99,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-20,90,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-14,20,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,15,75,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,20,69,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-9,78,8,3), new GeneratedEnemyUnit(-6,-17,47,24,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "bf4ff6d345c319e869be048df4b13e00efdb49a4218c9d3f0167ebe9c7be0d77");
        }

        private static void Case_03746()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3746,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,8,82,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-3,58,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-16,55,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-4,27,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,18,8,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-17,33,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,17,24,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,12,42,34,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "267986464639608daa705588d6b4422b2da098ddfa20fb55462f30a6f12491c5");
        }

        private static void Case_03747()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3747,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-12,88,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-14,50,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,4,58,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,6,27,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,7,11,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-2,46,16,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "6f6d4bd153e70397f3336c9b098fba45966c20776fde78d9e8ed27985cf87d49");
        }

        private static void Case_03748()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3748,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-17,46,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,0,36,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,17,41,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-15,27,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,8,11,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-9,51,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,16,81,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-18,86,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-6,72,10,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "25e4233eacf6aa0ca2ef1d446ec610b70fb94434cef133faa3c1c662503a1e1f");
        }

        private static void Case_03749()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3749,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-3,31,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-11,6,41,2), new GeneratedEnemyUnit(6,-2,51,32,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "05cadf72ca220acbb228348abed64957a5a315ed378c839772ae6d46fcdf91fc");
        }

        private static void Case_03750()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3750,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,10,40,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-18,8,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-13,5,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,20,51,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-5,97,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-12,88,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-15,81,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,16,33,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-2,30,27,1), new GeneratedEnemyUnit(1,12,11,34,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "b1f0753223081494b225fe5a38d00b2e4daf2d32419a339f4720df74ec15c251");
        }

        private static void Case_03751()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3751,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-3,30,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-4,67,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,7,12,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-19,53,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-5,23,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-20,76,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,11,57,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-7,8,24,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "7e1e98d146ea4bf51f2c414656662a92aced8bd5db37bc8cd83f5aa7f93ec817");
        }

        private static void Case_03752()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3752,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,0,42,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,9,56,50,1), new GeneratedEnemyUnit(-10,4,59,12,2), new GeneratedEnemyUnit(-19,-8,55,30,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "98985f9dcc135eb8d67991c597ccd71d0b7f3463752e2a809f87ef235e72a79d");
        }

        private static void Case_03753()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3753,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,10,75,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-20,46,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-6,23,26,2), new GeneratedEnemyUnit(3,15,5,40,4), new GeneratedEnemyUnit(-7,14,25,1,2), new GeneratedEnemyUnit(7,7,75,37,4), new GeneratedEnemyUnit(-15,13,66,14,4), new GeneratedEnemyUnit(-13,20,56,7,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "9273fc128da54942bcfd09a316d0e895a77be3708ba830b98b973fbdffcea6c8");
        }

        private static void Case_03754()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3754,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-16,42,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,17,21,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,8,55,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-10,21,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-9,37,40,4), new GeneratedEnemyUnit(-19,6,17,10,3), new GeneratedEnemyUnit(16,11,11,28,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "dd9d9de27bbebea706b9b574344e52f5a4608c447588b2a4b77268170d77b429");
        }

        private static void Case_03755()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3755,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-13,29,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,12,35,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-19,98,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,1,15,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-17,40,4,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "bc4e579a25b1eaf75616d4e02949e400f571e63c7e4a4baf2a668225e0510cb3");
        }

        private static void Case_03756()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3756,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-16,7,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,2,16,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,14,77,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-18,8,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-7,70,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,16,63,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,7,96,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-2,15,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,11,78,4,2), new GeneratedEnemyUnit(10,-10,66,31,4), new GeneratedEnemyUnit(9,-14,62,32,4), new GeneratedEnemyUnit(0,-9,52,25,4), new GeneratedEnemyUnit(-4,3,64,43,1), new GeneratedEnemyUnit(-16,-19,83,23,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "13b8d9e360cbe3a6d5146b0aab1e23ace97a4af2d5c97cafe1764dd3184bb13c");
        }

        private static void Case_03757()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3757,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-5,94,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,13,79,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,11,82,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-12,68,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,11,100,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-20,27,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,15,32,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-5,81,29,1), new GeneratedEnemyUnit(10,16,52,29,1), new GeneratedEnemyUnit(4,12,94,27,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "05e3604db7c760930afec61e5dc1ed0bcb02a6da3116b06dee2ceee80221529e");
        }

        private static void Case_03758()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3758,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-6,20,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-16,67,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-7,5,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,16,20,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-16,26,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,8,32,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-14,100,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,10,52,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,4,96,1,1), new GeneratedEnemyUnit(9,10,91,15,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "66505e2f54709ccf7a1d57cf9b0be812f050d3442121743021bec8174f078fbf");
        }

        private static void Case_03759()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3759,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,13,100,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,16,53,32,2), new GeneratedEnemyUnit(-14,19,89,31,1), new GeneratedEnemyUnit(5,-5,9,44,1), new GeneratedEnemyUnit(11,-8,61,47,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "ff15c57277027df29d0de5d762a4717f3ada78151a515a40a79929787e1e1831");
        }

        private static void Case_03760()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3760,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-7,24,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-13,37,19,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "a05287ca210dc4f6b2864a3b7b705d4859196efefac74396dfd640d72d8572c7");
        }

        private static void Case_03761()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3761,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-9,64,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-1,15,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,13,26,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,2,98,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-5,52,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-18,88,21,4), new GeneratedEnemyUnit(-3,-6,6,24,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "cfe952973902042b8c90e4fd50fa96bedb78b166691ab788f7c1fc5050f6ed68");
        }

        private static void Case_03762()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3762,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,8,43,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-13,42,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,12,90,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-18,89,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,18,79,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-20,6,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-6,58,36,4), new GeneratedEnemyUnit(-19,4,67,36,2), new GeneratedEnemyUnit(-12,-2,44,33,2), new GeneratedEnemyUnit(-10,-10,78,49,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "e6d3927d0627701e03bd9736dee6c096bd10e83a433c112d3fc98a88ae2e8e5e");
        }

        private static void Case_03763()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3763,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-7,90,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,2,73,3,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "56f73e567da8da0ca22fa2375b946af0ebccb9438816700c9a250564e8515f03");
        }

        private static void Case_03764()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3764,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,7,59,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-3,26,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,10,16,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-18,95,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,0,44,9,4), new GeneratedEnemyUnit(9,-14,74,30,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "cd4f9f3a4529795181c273be2138df4d9238fa7703de3310fc4e53a255ef6d47");
        }

        private static void Case_03765()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3765,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-12,87,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,18,94,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-10,31,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-3,30,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,12,75,15,1), new GeneratedEnemyUnit(-4,-5,61,22,2), new GeneratedEnemyUnit(-11,-13,28,20,1), new GeneratedEnemyUnit(3,-14,51,23,3), new GeneratedEnemyUnit(17,-4,40,2,4), new GeneratedEnemyUnit(-11,4,79,33,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "65a51d90e432af865cded4ac5921adf0d8f20a985bd16fb49438b74746827750");
        }

        private static void Case_03766()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3766,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-14,55,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-16,47,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,5,41,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,5,71,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-14,62,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,11,18,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-18,38,44,1), new GeneratedEnemyUnit(9,-20,64,40,3), new GeneratedEnemyUnit(7,-16,80,12,4), new GeneratedEnemyUnit(-6,15,85,11,2), new GeneratedEnemyUnit(8,15,48,44,3), new GeneratedEnemyUnit(5,7,69,1,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "2be45bc9dc983435dc94327189ba82b0f8bc454b90584e9c7e30ec789795946f");
        }

        private static void Case_03767()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3767,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,4,6,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,4,52,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-17,83,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-2,28,23,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "668009ecd72ac0cc8f79e02f8f863cada48f92dcee8f0f410ea85ed75b628a90");
        }

        private static void Case_03768()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3768,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-18,68,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-19,62,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,13,76,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-19,14,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,9,72,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-6,29,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-15,24,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-9,89,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,12,91,20,4), new GeneratedEnemyUnit(12,-5,68,40,4), new GeneratedEnemyUnit(18,-6,95,28,3), new GeneratedEnemyUnit(-19,-4,45,9,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "ec43f0c8b3f9851cc4536a6161de34d72e03a36ca7db412a812e0a52e76a77e6");
        }

        private static void Case_03769()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3769,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,9,36,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,10,85,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,16,53,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,2,92,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,2,65,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-11,34,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-7,97,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-6,17,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-16,80,39,2), new GeneratedEnemyUnit(-1,-16,79,43,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "fa467ca23b246708a66201d1394da0761c6f6d58527267fe4aa8c222324356e7");
        }

        private static void Case_03770()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3770,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,3,62,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,16,90,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-1,64,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,5,97,20,3), new GeneratedEnemyUnit(-3,12,97,26,2), new GeneratedEnemyUnit(12,8,63,29,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "634c8a0f9a1eb7835bee882bbf1332298d107a3df12f8d9f2a887bd4c457d839");
        }

        private static void Case_03771()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3771,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-12,23,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-11,18,23,1), new GeneratedEnemyUnit(-16,-8,47,32,1), new GeneratedEnemyUnit(-18,-5,41,7,2), new GeneratedEnemyUnit(-3,17,14,18,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b4c28278064a25121f01782f19620964d22fb477cb88282fe0d8bec1698b5c9b");
        }

        private static void Case_03772()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3772,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,2,43,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-7,9,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,12,69,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,11,19,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,13,99,15,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "96ce12c6d2d0c5a8316901f0908cb78aa8c34815224881e69536790a60f19668");
        }

        private static void Case_03773()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3773,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,10,57,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-13,40,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,1,79,7,4), new GeneratedEnemyUnit(-12,-12,69,36,3), new GeneratedEnemyUnit(-6,11,6,18,2), new GeneratedEnemyUnit(20,-7,56,45,4), new GeneratedEnemyUnit(-3,-14,26,30,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "d95753cbf138a3fbb3e46232a1a7917f0a3e1c4a3a6fbd9bb6e2e1ae79a9746e");
        }

        private static void Case_03774()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3774,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,8,12,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-1,90,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,13,88,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-11,31,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,19,41,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-19,91,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-5,28,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-1,42,15,2), new GeneratedEnemyUnit(10,-16,80,46,4), new GeneratedEnemyUnit(3,-10,15,10,2), new GeneratedEnemyUnit(11,17,58,28,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "c88945ce6139f9b101ce49835834c9644506eb45b57a949a29d71c4810147733");
        }

        private static void Case_03775()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3775,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-9,61,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,9,29,23,2), new GeneratedEnemyUnit(-14,-5,39,15,4), new GeneratedEnemyUnit(-11,-8,63,16,1), new GeneratedEnemyUnit(19,-20,40,29,2), new GeneratedEnemyUnit(0,-7,96,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "94fd30728808860225a942f9b56393d06ece9e1eaafa1321e7c8a63b76f40e79");
        }

        private static void Case_03776()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3776,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-5,45,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-17,23,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-3,36,22,1), new GeneratedEnemyUnit(-18,7,90,6,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "fe0bf51a77c552ac60495d6afd0ccd9dd0c48f610653552f14ab1b6e5f444350");
        }

        private static void Case_03777()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3777,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,4,88,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,3,71,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,4,58,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,15,89,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,20,13,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,16,54,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-10,78,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-11,46,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,19,40,45,1), new GeneratedEnemyUnit(5,-3,77,14,2), new GeneratedEnemyUnit(14,19,6,22,3), new GeneratedEnemyUnit(-1,-6,25,46,2), new GeneratedEnemyUnit(-19,-18,63,40,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "0f45b5a5b69865c3cd3934dd353560d4cd69f3b0cf95d178e83a2cf96d87b905");
        }

        private static void Case_03778()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3778,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-1,58,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-15,38,43,2), new GeneratedEnemyUnit(-17,0,91,14,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "5cc6fee9873c2a2e478391693b0eb52f5868645bf7f71fd3ea8b127818ce3987");
        }

        private static void Case_03779()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3779,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,15,67,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-1,5,3,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "ed9303f00f4bdc95fada36e96695b331ecb8f82e142425793c104c1e33b5f98e");
        }

    }
}
