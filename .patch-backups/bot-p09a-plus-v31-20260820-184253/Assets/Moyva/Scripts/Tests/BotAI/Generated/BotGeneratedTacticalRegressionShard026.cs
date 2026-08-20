using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard026
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_04680();
            Case_04681();
            Case_04682();
            Case_04683();
            Case_04684();
            Case_04685();
            Case_04686();
            Case_04687();
            Case_04688();
            Case_04689();
            Case_04690();
            Case_04691();
            Case_04692();
            Case_04693();
            Case_04694();
            Case_04695();
            Case_04696();
            Case_04697();
            Case_04698();
            Case_04699();
            Case_04700();
            Case_04701();
            Case_04702();
            Case_04703();
            Case_04704();
            Case_04705();
            Case_04706();
            Case_04707();
            Case_04708();
            Case_04709();
            Case_04710();
            Case_04711();
            Case_04712();
            Case_04713();
            Case_04714();
            Case_04715();
            Case_04716();
            Case_04717();
            Case_04718();
            Case_04719();
            Case_04720();
            Case_04721();
            Case_04722();
            Case_04723();
            Case_04724();
            Case_04725();
            Case_04726();
            Case_04727();
            Case_04728();
            Case_04729();
            Case_04730();
            Case_04731();
            Case_04732();
            Case_04733();
            Case_04734();
            Case_04735();
            Case_04736();
            Case_04737();
            Case_04738();
            Case_04739();
            Case_04740();
            Case_04741();
            Case_04742();
            Case_04743();
            Case_04744();
            Case_04745();
            Case_04746();
            Case_04747();
            Case_04748();
            Case_04749();
            Case_04750();
            Case_04751();
            Case_04752();
            Case_04753();
            Case_04754();
            Case_04755();
            Case_04756();
            Case_04757();
            Case_04758();
            Case_04759();
            Case_04760();
            Case_04761();
            Case_04762();
            Case_04763();
            Case_04764();
            Case_04765();
            Case_04766();
            Case_04767();
            Case_04768();
            Case_04769();
            Case_04770();
            Case_04771();
            Case_04772();
            Case_04773();
            Case_04774();
            Case_04775();
            Case_04776();
            Case_04777();
            Case_04778();
            Case_04779();
            Case_04780();
            Case_04781();
            Case_04782();
            Case_04783();
            Case_04784();
            Case_04785();
            Case_04786();
            Case_04787();
            Case_04788();
            Case_04789();
            Case_04790();
            Case_04791();
            Case_04792();
            Case_04793();
            Case_04794();
            Case_04795();
            Case_04796();
            Case_04797();
            Case_04798();
            Case_04799();
            Case_04800();
            Case_04801();
            Case_04802();
            Case_04803();
            Case_04804();
            Case_04805();
            Case_04806();
            Case_04807();
            Case_04808();
            Case_04809();
            Case_04810();
            Case_04811();
            Case_04812();
            Case_04813();
            Case_04814();
            Case_04815();
            Case_04816();
            Case_04817();
            Case_04818();
            Case_04819();
            Case_04820();
            Case_04821();
            Case_04822();
            Case_04823();
            Case_04824();
            Case_04825();
            Case_04826();
            Case_04827();
            Case_04828();
            Case_04829();
            Case_04830();
            Case_04831();
            Case_04832();
            Case_04833();
            Case_04834();
            Case_04835();
            Case_04836();
            Case_04837();
            Case_04838();
            Case_04839();
            Case_04840();
            Case_04841();
            Case_04842();
            Case_04843();
            Case_04844();
            Case_04845();
            Case_04846();
            Case_04847();
            Case_04848();
            Case_04849();
            Case_04850();
            Case_04851();
            Case_04852();
            Case_04853();
            Case_04854();
            Case_04855();
            Case_04856();
            Case_04857();
            Case_04858();
            Case_04859();
        }

        private static void Case_04680()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4680,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,3,42,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-14,22,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,17,74,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,13,35,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,10,63,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-10,9,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,14,58,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,6,27,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-11,49,25,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "a6f7ca09cb8b9d32d8df61f4578fb10b64ef5b9b05c13a22791153ac591f7720");
        }

        private static void Case_04681()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4681,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,14,62,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,2,20,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,6,38,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,13,52,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-19,89,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-4,16,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-13,92,40,4), new GeneratedEnemyUnit(-3,19,100,39,4), new GeneratedEnemyUnit(-17,0,12,45,3), new GeneratedEnemyUnit(4,12,46,39,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "e0bfa5b873d8ddfc0641c10f099c535e4d7054c93f765c80f60b0feef52a0b59");
        }

        private static void Case_04682()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4682,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-20,59,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-6,31,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-15,47,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-19,67,36,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "a9a3d4fda10dc273a0e6ab9d6ab262094e89f062dc6887c10ca98bae12a0d45b");
        }

        private static void Case_04683()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4683,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,20,64,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,5,75,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-5,49,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,8,31,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-15,86,5,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "1a84269c1427927ae7f5e9814ed9fc2030909d6da3defca27262361e95fffd4c");
        }

        private static void Case_04684()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4684,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,9,47,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-9,74,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-12,86,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-5,21,11,4), new GeneratedEnemyUnit(-2,20,52,35,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "285d30cbd05f320ccdf97fa8e594d0666ddae916df7b3deb7dec66c30de2cdc2");
        }

        private static void Case_04685()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4685,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-7,61,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-13,6,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-18,15,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,16,89,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,9,6,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,11,39,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,16,53,11,4), new GeneratedEnemyUnit(12,-5,5,8,4), new GeneratedEnemyUnit(16,14,66,8,1), new GeneratedEnemyUnit(-2,2,60,39,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "a67f4114b03e2fedaced39a7d7ff9b2992283a678974d286d6926123606affc9");
        }

        private static void Case_04686()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4686,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-10,67,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,11,97,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-8,89,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,8,6,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,4,73,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-1,85,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,12,11,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-20,8,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,20,47,3,4), new GeneratedEnemyUnit(-7,-1,64,40,4), new GeneratedEnemyUnit(3,9,96,5,1), new GeneratedEnemyUnit(19,13,90,7,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "4a8d3ae2c5c070f0c18c0079c076e5ceebb89882ebf9e81b689a5a24227429a4");
        }

        private static void Case_04687()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4687,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,4,24,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,20,7,25,2), new GeneratedEnemyUnit(4,-20,8,14,4), new GeneratedEnemyUnit(-16,18,70,12,1), new GeneratedEnemyUnit(1,-16,68,20,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "c93be3352cb339940e530b1ff66a4f7f7450726b6a608563d474eff1ebd6cce3");
        }

        private static void Case_04688()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4688,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-19,68,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,3,48,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,4,62,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,5,65,7,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "840306e02f5ab7a27c1ad303b5aca46bc1f6ca25f9d55a671f36d59e8272ade9");
        }

        private static void Case_04689()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4689,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,16,28,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-17,18,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-13,50,25,4), new GeneratedEnemyUnit(-9,15,23,8,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "70e2604b367887da0380e3db06c2b81d9da1b1133966548daf9a6dd4a6209c6c");
        }

        private static void Case_04690()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4690,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-9,12,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-17,98,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,14,65,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,15,85,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-17,38,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-8,61,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-13,56,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-6,16,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-11,43,49,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "e35ae6b387e047dda6f8fe883a6e157512cb3245807fd66d916e8ce6ae984097");
        }

        private static void Case_04691()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4691,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,9,88,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-16,27,49,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "d33529b8526314da5f5390daa2c58f661e473f150a714abd696dc603ae3d4955");
        }

        private static void Case_04692()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4692,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-18,30,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-11,58,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,15,31,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-11,19,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,13,26,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-2,40,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-13,11,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,0,13,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,4,54,25,1), new GeneratedEnemyUnit(-5,2,65,41,3), new GeneratedEnemyUnit(-10,-20,24,28,1), new GeneratedEnemyUnit(5,-11,70,10,3), new GeneratedEnemyUnit(17,-8,52,19,2), new GeneratedEnemyUnit(-9,-8,33,47,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "7322208d24d6cb1883de4a105f3f34961323a17c392153fb92be9bfc48009e59");
        }

        private static void Case_04693()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4693,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,2,83,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,6,31,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,4,31,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,9,93,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-14,65,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,3,21,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,2,51,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,19,56,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,0,88,7,3), new GeneratedEnemyUnit(-15,20,91,43,2), new GeneratedEnemyUnit(18,-9,15,20,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "49a76b1adab490db0333215b47d869d05c4048e789ea2ee955bfd8e2b43bcb19");
        }

        private static void Case_04694()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4694,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,3,38,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-14,66,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,13,65,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-20,19,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,6,86,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,5,85,47,3), new GeneratedEnemyUnit(-17,-17,50,6,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "1fa9b21300ff34146b1d5dad8ba7b93abde75fd1334e2309d4156ae590a94d67");
        }

        private static void Case_04695()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4695,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-11,65,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,0,97,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,17,72,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,1,45,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,10,85,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,16,42,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,7,61,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-14,5,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,0,39,39,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "35bc21de703116ef9cec6f7bc5d236c5a9c57da2836df09df1abd93c3fc1f4df");
        }

        private static void Case_04696()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4696,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,5,17,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-17,96,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,20,14,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-5,55,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-5,7,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-3,42,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,2,58,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,12,44,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,4,67,33,2), new GeneratedEnemyUnit(-2,19,94,17,2), new GeneratedEnemyUnit(-8,19,27,50,2), new GeneratedEnemyUnit(-13,12,26,50,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "7644c59ea178ff27c36f708a610a840be7ddbccd86dbfc088cb8352054cd7a27");
        }

        private static void Case_04697()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4697,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,1,99,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-15,48,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-10,36,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-1,91,17,3), new GeneratedEnemyUnit(-7,-16,63,20,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "8b71bb5f426c11081eecacc1d143b33c1533d58750ba0ac70a8b0da70d9a74f9");
        }

        private static void Case_04698()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4698,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-6,60,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-6,14,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,11,83,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,15,40,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-12,27,18,2), new GeneratedEnemyUnit(9,-10,85,20,1), new GeneratedEnemyUnit(-7,-2,38,24,2), new GeneratedEnemyUnit(0,10,58,35,2), new GeneratedEnemyUnit(-19,10,58,41,3), new GeneratedEnemyUnit(16,11,21,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "3ee714aef7edd6e877959a334f283418436f5579e196740c22ebbb7dc81c13e9");
        }

        private static void Case_04699()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4699,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,0,86,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,17,75,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-1,90,30,4), new GeneratedEnemyUnit(18,9,17,39,1), new GeneratedEnemyUnit(17,-1,38,6,4), new GeneratedEnemyUnit(13,-11,17,44,4), new GeneratedEnemyUnit(-2,-13,42,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "b56516fe98aae4024df5939fcb2b7b32a44144112bbe7c960a01300ea975bf92");
        }

        private static void Case_04700()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4700,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-14,57,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,13,94,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,8,7,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,4,63,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-19,52,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-20,97,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,15,59,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-8,21,5,4), new GeneratedEnemyUnit(-15,-7,61,43,1), new GeneratedEnemyUnit(-1,-2,24,48,2), new GeneratedEnemyUnit(15,8,92,23,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "233fb3ef13b3ac1b1e00185b7b7e893e68508ac10e8316129e0bee4b8005c39c");
        }

        private static void Case_04701()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4701,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,14,58,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,17,52,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,6,38,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,7,47,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,11,36,32,3), new GeneratedEnemyUnit(-11,-14,98,35,2), new GeneratedEnemyUnit(7,-20,94,45,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "889d3bd24c9719a0d5cca2f82346af6bdbd93a2c5216f3596156e11bdbd2109c");
        }

        private static void Case_04702()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4702,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-16,88,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,2,49,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-20,100,46,1), new GeneratedEnemyUnit(-7,4,26,4,3), new GeneratedEnemyUnit(-19,19,43,15,2), new GeneratedEnemyUnit(19,-3,86,38,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "05e6c9c9ff059b243ab6e73db5c6f81541e49a556453df54334b5122d403a977");
        }

        private static void Case_04703()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4703,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-12,32,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,0,41,5,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "714d1a652c8c21c7b04e38afc072b62bfc1b658afdeedf6517981ff3a3e95199");
        }

        private static void Case_04704()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4704,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-3,17,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,1,67,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,7,21,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,7,22,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,12,61,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "8af166a0af4e5772e9d8d39f25a3d2e0586fe178f350eff8c5d122532e77970a");
        }

        private static void Case_04705()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4705,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,10,70,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-18,6,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,1,93,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,16,39,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,14,45,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,10,38,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-3,60,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-2,73,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,9,73,33,1), new GeneratedEnemyUnit(7,10,99,47,2), new GeneratedEnemyUnit(-8,2,60,2,1), new GeneratedEnemyUnit(-16,8,89,30,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "64b6e4dc1707cb123a711a873d5b6eaf07cbbaba6b7dc2893d43038c6fee07db");
        }

        private static void Case_04706()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4706,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,20,27,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-1,83,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-15,30,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-6,55,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,7,27,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-19,51,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-8,81,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-14,73,23,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a056efd482279a305c90bbc0c49099d443b9a36969a1967cf0bfa772408d8614");
        }

        private static void Case_04707()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4707,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,15,36,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-20,21,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-17,79,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-14,42,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,17,7,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,14,82,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-18,82,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,13,70,5,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "9a6200592cd9130771702429036d16d11eb24400203813d79fb8cca8c1184113");
        }

        private static void Case_04708()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4708,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,20,95,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,18,61,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,18,74,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,8,24,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-11,89,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-1,80,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "caee589ab379044acf4aebbc64bc7e8d1af109cb9f7e5b815ecb133b3c82cdab");
        }

        private static void Case_04709()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4709,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-7,69,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-7,74,23,4), new GeneratedEnemyUnit(5,-20,9,28,2), new GeneratedEnemyUnit(-3,11,72,12,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "74192cb81dddab57b4abd80991853fe8ce5620aeb3a3c4297a19d3b51f3cc293");
        }

        private static void Case_04710()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4710,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-16,46,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,16,81,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-5,99,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-12,79,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,10,58,4,2), new GeneratedEnemyUnit(-17,18,54,9,2), new GeneratedEnemyUnit(-13,-8,67,45,4), new GeneratedEnemyUnit(10,-19,51,50,4), new GeneratedEnemyUnit(13,-8,96,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "001ea4458ae16920fb18b6281bc305c3239994655c7e73fd2b693219f441733f");
        }

        private static void Case_04711()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4711,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,5,28,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-18,76,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-2,85,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-15,63,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,10,61,11,3), new GeneratedEnemyUnit(-12,-13,19,45,3), new GeneratedEnemyUnit(-7,3,77,11,4), new GeneratedEnemyUnit(14,-8,46,45,2), new GeneratedEnemyUnit(-7,-20,95,24,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "89224a9922a819d171d508e8c3acc8e6f4e5e73f8473f8151276dc6ca5a0905f");
        }

        private static void Case_04712()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4712,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-5,72,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,13,67,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-2,91,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,15,95,40,1), new GeneratedEnemyUnit(-15,4,20,35,3), new GeneratedEnemyUnit(-18,-4,61,30,1), new GeneratedEnemyUnit(-14,-9,22,37,1), new GeneratedEnemyUnit(-16,10,23,8,2), new GeneratedEnemyUnit(2,9,30,36,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "92055ab6937d4f27f99e6133fe93eaff66dec33aa86f6fc22c3276a25756fa32");
        }

        private static void Case_04713()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4713,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-14,43,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-6,52,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-3,82,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-13,76,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-15,34,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,6,89,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-12,34,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "c36bba1cc0769b0bb7625f8391aad983d9104c7349d2cc63d2d4543d47335c84");
        }

        private static void Case_04714()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4714,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,1,87,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-16,49,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,10,72,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-6,35,47,4), new GeneratedEnemyUnit(13,12,6,36,2), new GeneratedEnemyUnit(20,-11,41,24,1), new GeneratedEnemyUnit(5,19,98,22,1), new GeneratedEnemyUnit(19,5,6,16,4), new GeneratedEnemyUnit(-4,-4,13,12,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "3e1a1f4cfe21632249c7756696e50573d9a7782e6d8efd49b5f415f40b8909b0");
        }

        private static void Case_04715()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4715,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-2,16,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-13,96,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-13,83,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-8,10,45,1), new GeneratedEnemyUnit(-2,5,20,50,2), new GeneratedEnemyUnit(19,11,65,49,3), new GeneratedEnemyUnit(14,1,30,43,2), new GeneratedEnemyUnit(10,-16,65,11,2), new GeneratedEnemyUnit(3,8,58,41,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "ea4eff932c145b84cbb0e7cef8a9daa46ffcd1cbc0d9b2c9d5d1d80d28283a38");
        }

        private static void Case_04716()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4716,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,10,26,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-9,76,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-15,45,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-13,60,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-12,43,19,1), new GeneratedEnemyUnit(-13,11,50,43,1), new GeneratedEnemyUnit(-4,-19,77,25,2), new GeneratedEnemyUnit(-15,-11,66,6,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "b8a61f9d419fd1cef14d71de70bffb52926fdc6c458d1d233b090062dc005fc7");
        }

        private static void Case_04717()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4717,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-11,98,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,0,55,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,12,96,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-19,57,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-16,54,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-8,69,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,6,10,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,6,49,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,15,16,10,2), new GeneratedEnemyUnit(-10,9,57,37,4), new GeneratedEnemyUnit(13,-1,64,27,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "a09cd93ecfb296a37aa115e45316d1a0eef27ebfe06dc90f7d715df8737b528a");
        }

        private static void Case_04718()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4718,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-10,36,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-4,18,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,1,97,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,20,86,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,4,59,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-6,63,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-16,62,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-1,11,14,2), new GeneratedEnemyUnit(18,10,76,21,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "41dee41dcd0a93da50571c70bdbf543d9fd0328a69effddf2aabf30e66b439ba");
        }

        private static void Case_04719()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4719,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-14,51,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,11,81,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-16,78,26,3), new GeneratedEnemyUnit(-1,-9,21,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "062c444410137a4014030619be247333cbd5a7ba22064985778c05278f9f2de1");
        }

        private static void Case_04720()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4720,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,19,44,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-17,82,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-4,11,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-9,96,2,3), new GeneratedEnemyUnit(-19,-2,77,7,1), new GeneratedEnemyUnit(-11,14,96,13,3), new GeneratedEnemyUnit(-4,-11,62,38,3), new GeneratedEnemyUnit(20,11,63,16,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "88c92d4f4bc1c1043fe43f6e7cdecfa1b07a5888208b8d0faf0a952c259ab86f");
        }

        private static void Case_04721()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4721,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,16,62,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,3,99,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-15,66,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,3,70,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-6,94,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-8,65,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-14,75,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-18,71,38,3), new GeneratedEnemyUnit(13,11,56,27,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "78dd52d60d9c0cfe1c8f527d2e42da66320ab96e293ffaf9536242b1997df130");
        }

        private static void Case_04722()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4722,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-17,13,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-6,97,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-14,82,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,2,60,26,3), new GeneratedEnemyUnit(-10,18,63,14,2), new GeneratedEnemyUnit(-6,19,19,23,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "e309e0133db7a1e41f753ea130204b4bafce553d6f704ffe5eb09e3e271d6456");
        }

        private static void Case_04723()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4723,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-17,20,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-4,6,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,16,99,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-12,22,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-20,53,41,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "efb6ec8e67883a83e4f652b496a2c1518479fe0302b4d83db54f9c862cbbd9cd");
        }

        private static void Case_04724()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4724,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,9,37,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,19,38,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,14,80,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-15,49,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-8,25,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,16,11,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-7,58,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-16,53,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,15,70,26,4), new GeneratedEnemyUnit(-17,20,23,7,3), new GeneratedEnemyUnit(8,11,6,8,1), new GeneratedEnemyUnit(-20,5,95,9,1), new GeneratedEnemyUnit(9,11,48,11,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "e7d32928f9d4be7ba35c2700697e63821ff91fd5370604fc42fb14b296b18a5d");
        }

        private static void Case_04725()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4725,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,6,100,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,12,42,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,5,89,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,10,19,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,1,57,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,13,44,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,13,14,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-11,27,22,4), new GeneratedEnemyUnit(9,-9,46,20,3), new GeneratedEnemyUnit(5,-13,56,9,4), new GeneratedEnemyUnit(7,20,76,24,4), new GeneratedEnemyUnit(3,12,91,4,1), new GeneratedEnemyUnit(-2,14,64,50,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "986d80001eec86fc87da61282d0da6eeb7e6f4c5e3856baeccc16710c13f5afa");
        }

        private static void Case_04726()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4726,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-7,54,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-17,18,29,1), new GeneratedEnemyUnit(2,-1,73,45,1), new GeneratedEnemyUnit(14,16,81,36,4), new GeneratedEnemyUnit(-9,3,59,22,1), new GeneratedEnemyUnit(-12,-6,31,10,2), new GeneratedEnemyUnit(16,-4,30,3,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "613101a35290ebed91e96fa1e6e0f29d359f7c895066dc6e6cd781acfc817591");
        }

        private static void Case_04727()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4727,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,3,23,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,8,88,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,16,20,17,2), new GeneratedEnemyUnit(10,20,18,14,1), new GeneratedEnemyUnit(-17,10,77,34,3), new GeneratedEnemyUnit(8,-17,77,9,1), new GeneratedEnemyUnit(-19,-12,75,42,4), new GeneratedEnemyUnit(-16,-15,36,21,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "24848f9e5468e3a24b4b9928085c1f4bb46795d87dfbf0a29056d1b083c61c3c");
        }

        private static void Case_04728()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4728,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-18,94,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,2,87,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,5,78,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,11,65,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-1,55,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,4,99,30,3), new GeneratedEnemyUnit(20,-2,80,10,4), new GeneratedEnemyUnit(-7,20,52,30,1), new GeneratedEnemyUnit(14,-17,75,4,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "fb5addcaa1c6ae2e73383c9042946bf86d19fc2e149c43666704ae7a0932f572");
        }

        private static void Case_04729()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4729,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-18,24,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-9,18,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,12,88,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-7,18,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-2,65,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,1,41,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,10,69,9,2), new GeneratedEnemyUnit(4,-7,80,37,3), new GeneratedEnemyUnit(-15,2,6,45,1), new GeneratedEnemyUnit(-9,12,55,3,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "d4e541de01cd293064cddde773d422a7fa76540eed35d5bfaf792751f574e93b");
        }

        private static void Case_04730()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4730,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-14,75,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,2,62,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-20,93,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-3,81,46,3), new GeneratedEnemyUnit(4,-4,93,47,1), new GeneratedEnemyUnit(-20,-6,88,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "4419ee762810cd4d97c7ae9186846a4e3b9101fd0b502ca86c82bc83f8ffacdd");
        }

        private static void Case_04731()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4731,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,20,60,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-12,71,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,7,12,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,13,44,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-6,21,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-13,98,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,9,24,15,4), new GeneratedEnemyUnit(-12,-1,35,48,2), new GeneratedEnemyUnit(7,-5,32,4,3), new GeneratedEnemyUnit(-3,17,5,5,3), new GeneratedEnemyUnit(19,-12,34,37,1), new GeneratedEnemyUnit(2,7,26,15,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "b58a5a3ed1ca688980715c12fcb9a4c16da0bec1d9927f26eff7607acd0ae339");
        }

        private static void Case_04732()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4732,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,9,89,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,10,73,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-18,18,5,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "96c8415b016137e7f249341161a8f13cbe245d36ad15e8a967e8f8bba09e7326");
        }

        private static void Case_04733()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4733,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,13,22,6,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "4b9987a6035e723a12f694046e09bb59d08508ea49572b900b9b63bd50a88f0a");
        }

        private static void Case_04734()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4734,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,19,20,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-5,61,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,15,77,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-20,17,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-9,53,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,12,16,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,8,66,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-12,64,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-13,56,40,2), new GeneratedEnemyUnit(-9,-7,37,6,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "9889969926802b5d5a9fa482a8178ef8c877cb146b21dcc9cf712a90deb3bfb4");
        }

        private static void Case_04735()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4735,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,9,58,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,8,14,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-7,47,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,17,27,50,1), new GeneratedEnemyUnit(5,17,64,21,2), new GeneratedEnemyUnit(2,4,35,25,2), new GeneratedEnemyUnit(6,18,100,9,2), new GeneratedEnemyUnit(7,9,77,10,3), new GeneratedEnemyUnit(-9,-13,27,4,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8af0bb694f04246de92ac3d9b876e91662b3fffa5b4d5438f5db3685bb6ecd86");
        }

        private static void Case_04736()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4736,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,0,64,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,2,89,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-11,7,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,10,32,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-14,20,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,7,39,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,6,99,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-15,77,47,2), new GeneratedEnemyUnit(5,-9,6,37,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e1dac3bd2c49807b3be8f29cde876a1e7ab38e68e4103bba1cf502c38b01bb69");
        }

        private static void Case_04737()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4737,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-15,79,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,14,64,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,18,46,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,16,89,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,8,55,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,6,80,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-5,80,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,0,24,48,1), new GeneratedEnemyUnit(17,10,17,5,1), new GeneratedEnemyUnit(-9,-3,10,10,4), new GeneratedEnemyUnit(0,6,57,21,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "20e4d5d9cf96448985a593fd9f713f1465c2e18229ae2d3c269be50454153e66");
        }

        private static void Case_04738()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4738,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,10,59,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-11,30,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-4,41,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,2,34,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-5,53,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,8,38,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,12,46,43,2), new GeneratedEnemyUnit(-12,-11,72,34,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "04a4dfba009e790f15dc8394314a4908a6ef0617d1dc2a29167682ece7fb3333");
        }

        private static void Case_04739()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4739,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-8,20,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,2,92,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-10,40,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,12,50,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-7,96,48,4), new GeneratedEnemyUnit(-16,-17,79,40,2), new GeneratedEnemyUnit(20,8,28,36,2), new GeneratedEnemyUnit(-4,-4,53,25,3), new GeneratedEnemyUnit(-9,13,60,46,3), new GeneratedEnemyUnit(13,-18,70,12,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "e75e50a2430b72a62538373df679027ce39507e8851f58909c7cd081d7c9df28");
        }

        private static void Case_04740()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4740,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,5,45,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-16,35,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-17,76,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,1,90,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-6,8,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-16,27,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,8,63,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,13,56,7,2), new GeneratedEnemyUnit(8,11,45,2,4), new GeneratedEnemyUnit(3,8,47,7,1), new GeneratedEnemyUnit(16,-5,11,45,1), new GeneratedEnemyUnit(12,5,6,23,1), new GeneratedEnemyUnit(-7,-4,30,20,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "4035240b65010b5d3f880cc39f5bae9c9def246f4faa2e2f8b2e5ad6373b8f37");
        }

        private static void Case_04741()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4741,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-17,61,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,13,80,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-13,75,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,4,29,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-20,93,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-19,78,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,12,48,28,3), new GeneratedEnemyUnit(-13,2,38,45,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "ccf5d6fb974ee363e82e508bc1f0f999774e1c06814582016f23dce29b2fab56");
        }

        private static void Case_04742()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4742,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-3,54,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,15,26,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,15,36,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-15,70,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,4,5,49,2), new GeneratedEnemyUnit(-13,16,11,42,1), new GeneratedEnemyUnit(11,-4,85,33,4), new GeneratedEnemyUnit(13,9,87,16,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "33a354260a1c585a56cf733e49651e7cc7579a290e8a19fa0cdee816ba1bddce");
        }

        private static void Case_04743()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4743,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,18,33,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,11,23,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-15,61,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,14,20,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,13,23,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-16,29,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-1,54,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,7,99,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,17,76,7,4), new GeneratedEnemyUnit(-13,0,32,8,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "3242e2ee45a3a65d44e3b98dc1177b75f8e749d55c89de1e28609d0c1cc243b5");
        }

        private static void Case_04744()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4744,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-11,25,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,13,44,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-12,37,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-19,95,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-9,10,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,7,74,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-18,44,32,2), new GeneratedEnemyUnit(-8,19,36,5,4), new GeneratedEnemyUnit(20,0,39,36,3), new GeneratedEnemyUnit(0,2,79,37,2), new GeneratedEnemyUnit(-1,-7,21,11,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "67270d256d7457cb934fa43dd4c18938760cf6e48772e1a30751c65136391981");
        }

        private static void Case_04745()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4745,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-8,65,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-10,40,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-2,55,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-12,50,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-3,30,1,2), new GeneratedEnemyUnit(6,2,53,3,1), new GeneratedEnemyUnit(2,-5,91,40,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "04d03eb56b59f8ff2e3fa226ff7854990429d3a007735b88711e31d839d6f3f0");
        }

        private static void Case_04746()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4746,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-3,54,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,0,12,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-8,80,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-13,45,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,12,78,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-4,73,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-1,87,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-12,64,31,1), new GeneratedEnemyUnit(7,-19,75,18,3), new GeneratedEnemyUnit(7,-20,58,10,4), new GeneratedEnemyUnit(16,13,54,29,2), new GeneratedEnemyUnit(-6,-17,61,14,3), new GeneratedEnemyUnit(-17,-14,33,50,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "c62766f6d06e3d924102261db928c37c6bb6f52033bf283e1d7f91b535d944bf");
        }

        private static void Case_04747()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4747,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,14,36,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,5,63,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-15,26,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-19,94,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-5,22,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-19,54,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-18,12,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-16,91,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,11,21,34,2), new GeneratedEnemyUnit(18,9,31,40,2), new GeneratedEnemyUnit(16,15,98,12,1), new GeneratedEnemyUnit(20,7,9,21,4), new GeneratedEnemyUnit(-4,-7,71,1,2), new GeneratedEnemyUnit(-5,-13,16,6,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "81efa53a37724bf3e19867189cf5aee6e7e7566053f6398c68f0bcc3aa041b3c");
        }

        private static void Case_04748()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4748,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,7,69,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,17,80,23,1), new GeneratedEnemyUnit(-9,-8,59,8,2), new GeneratedEnemyUnit(3,-6,94,47,2), new GeneratedEnemyUnit(-18,-4,62,49,1), new GeneratedEnemyUnit(16,0,34,1,1), new GeneratedEnemyUnit(6,-11,93,48,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "907c5df127f953f0e92b16130d3e558150e55440537d0080b3f165df688d575f");
        }

        private static void Case_04749()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4749,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,11,69,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,19,60,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-13,100,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-17,35,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-19,39,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-9,37,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-20,8,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-4,19,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,4,46,26,1), new GeneratedEnemyUnit(14,-1,60,30,1), new GeneratedEnemyUnit(-20,-15,39,1,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c68fd1c4193b80e1ccd2ec486580d45332544bb237c80365bada86fed126dce1");
        }

        private static void Case_04750()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4750,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-10,52,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,13,95,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,11,71,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-15,90,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-1,69,43,3), new GeneratedEnemyUnit(15,14,80,45,2), new GeneratedEnemyUnit(-1,-19,15,45,3), new GeneratedEnemyUnit(-8,-16,75,10,2), new GeneratedEnemyUnit(-12,-5,61,14,2), new GeneratedEnemyUnit(-3,8,12,30,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "4a0c14d6993f33758964a5e099eb2ffe3464c5b00ae40142e400e812730c6e02");
        }

        private static void Case_04751()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4751,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-15,58,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-10,74,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-19,23,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-10,63,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,10,74,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,5,33,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,17,99,48,2), new GeneratedEnemyUnit(-2,3,42,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "f546747045165a13cafc7aaaa3412b0e7804ab6b99cbb15007ba3a222f2e390a");
        }

        private static void Case_04752()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4752,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-17,23,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-10,52,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,19,33,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,12,95,27,1), new GeneratedEnemyUnit(-6,10,87,23,1), new GeneratedEnemyUnit(-18,18,98,47,3), new GeneratedEnemyUnit(-4,-10,55,13,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "08a97f6354f547c6d87553b80981b251ada5a994e23201c7c4e6ff78af155c14");
        }

        private static void Case_04753()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4753,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,18,74,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-9,70,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,14,27,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,18,24,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,0,55,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,7,17,43,3), new GeneratedEnemyUnit(-9,11,62,46,1), new GeneratedEnemyUnit(-12,-6,64,47,2), new GeneratedEnemyUnit(18,9,100,14,4), new GeneratedEnemyUnit(-6,-1,22,28,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "4f1ff19e6c655085c14990d640d730fac45ef7adcf76ac5d3cbdfb7b0bc747ab");
        }

        private static void Case_04754()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4754,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,17,88,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,6,89,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,15,8,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-17,5,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,4,53,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-14,40,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,5,62,21,1), new GeneratedEnemyUnit(-1,6,12,21,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "fa74c81ccbdfa1ae462ac9a0a9b7f4883da6351f109db53bbec5c8a6edccbf3a");
        }

        private static void Case_04755()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4755,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-6,64,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,3,34,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,11,93,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,12,86,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-11,39,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-8,70,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,8,28,1,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "1cdec5fc6ac92ec9e533cb628d5884ac4cd4cb6acdc7a04febca6e1b5b5297c7");
        }

        private static void Case_04756()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4756,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,2,27,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-2,54,36,4), new GeneratedEnemyUnit(-15,-9,8,33,1), new GeneratedEnemyUnit(18,15,69,44,1), new GeneratedEnemyUnit(-20,-2,65,39,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "de46f9ef931a25f76f7544c580a6d2a0eb694724d72e0804d7f162444c3184e7");
        }

        private static void Case_04757()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4757,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,15,90,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,11,43,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-2,29,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-5,14,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-1,50,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-7,30,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,18,13,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,3,30,28,1), new GeneratedEnemyUnit(5,19,72,4,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "10d395e0fdf48412f3d1de5a09753723065e729f3c10ad5ce0bcca0c0bb70703");
        }

        private static void Case_04758()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4758,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,17,74,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-2,70,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-1,44,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-14,37,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-2,84,12,2), new GeneratedEnemyUnit(4,-6,56,32,1), new GeneratedEnemyUnit(16,17,29,10,1), new GeneratedEnemyUnit(-2,-2,89,28,2), new GeneratedEnemyUnit(-10,14,55,39,4), new GeneratedEnemyUnit(-1,5,15,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "c38397235b40a62e295813db67d6770d454ad7c8aafb8212071abd4fafb3a5d1");
        }

        private static void Case_04759()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4759,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,10,36,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,16,6,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,10,83,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,20,76,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-8,61,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,5,17,3,3), new GeneratedEnemyUnit(2,9,63,38,3), new GeneratedEnemyUnit(11,13,39,32,4), new GeneratedEnemyUnit(1,3,44,2,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "720ae93c8af325d8b837db4e4beb141263ba06c338cde465d13c8a5dc8f32c2c");
        }

        private static void Case_04760()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4760,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,15,27,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-17,90,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,12,34,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-13,56,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-4,14,17,2), new GeneratedEnemyUnit(-18,9,26,16,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "4ab5bd11d38f9f51cd694218306d3f5f3692d31b03e22afaa038cdcef51fec77");
        }

        private static void Case_04761()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4761,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,17,61,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-10,74,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-2,6,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,11,57,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,6,58,39,3), new GeneratedEnemyUnit(-10,-12,41,33,1), new GeneratedEnemyUnit(-20,-15,63,33,3), new GeneratedEnemyUnit(6,15,20,20,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "843e99224841fb0723d1caf8e2652eec7d93fb58c111ba47323a5d1bb0c54ec6");
        }

        private static void Case_04762()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4762,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,5,87,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,20,42,6,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "53404bd61501ab37a966c6a17d75ff5a75da306ee15c9fbaee6d42b1e2a8aa8a");
        }

        private static void Case_04763()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4763,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,11,8,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-14,85,6,1), new GeneratedEnemyUnit(-11,11,14,33,4), new GeneratedEnemyUnit(-10,18,45,44,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "afd8799119d8754407e325cd9ba53c882f6398ae22f14ce9a3f6b8e94c11a44d");
        }

        private static void Case_04764()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4764,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-4,34,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-7,95,48,4), new GeneratedEnemyUnit(20,9,85,17,1), new GeneratedEnemyUnit(1,-4,88,3,1), new GeneratedEnemyUnit(18,2,72,28,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "728fe9fbeac7fd95516511b8e194e710235e05c8371d379f7aa2a5af93ce6ba4");
        }

        private static void Case_04765()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4765,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,5,94,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,18,80,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,11,79,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,16,25,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-3,93,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,14,33,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,9,99,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-20,29,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,0,46,26,3), new GeneratedEnemyUnit(11,-10,99,46,3), new GeneratedEnemyUnit(13,3,67,40,1), new GeneratedEnemyUnit(13,-3,64,4,2), new GeneratedEnemyUnit(20,-14,55,24,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "7359055613e1aa13fcc69ec9d135f44d2c08901cdc9948313b2474e48a9081f9");
        }

        private static void Case_04766()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4766,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,19,65,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,2,82,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-16,61,2,2), new GeneratedEnemyUnit(19,19,56,44,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "58da91879f25538d34f7afca4f71af1c7d8fc0e6bb9ac90ea47696d19045c197");
        }

        private static void Case_04767()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4767,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-14,27,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-11,60,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,1,66,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,2,50,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,19,86,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-13,41,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,17,7,48,4), new GeneratedEnemyUnit(-15,-20,27,31,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "a532eca78bebb9c21fb8b58a578a46f487e0b2d58e79c6862c20cb1d2fe86f4d");
        }

        private static void Case_04768()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4768,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,13,96,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-11,16,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-5,17,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-11,58,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,6,82,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-7,84,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,10,15,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,15,20,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,3,10,38,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "c8660cf472731ffcce1d99ac42e695bf1441bd701e84a6b3fc990efb53de6cd3");
        }

        private static void Case_04769()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4769,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,20,60,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-17,59,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-17,59,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-3,88,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-5,9,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,0,15,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,11,88,3,1), new GeneratedEnemyUnit(-16,-16,64,3,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "a936e4890b5fcb3c6acfb09cab272006578e336af826162eed3974cea4290492");
        }

        private static void Case_04770()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4770,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-8,21,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,4,88,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,10,87,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,3,94,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,6,66,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-16,18,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,13,99,7,2), new GeneratedEnemyUnit(-9,-6,76,23,4), new GeneratedEnemyUnit(12,-4,24,45,1), new GeneratedEnemyUnit(-1,20,49,27,2), new GeneratedEnemyUnit(18,-4,61,37,4), new GeneratedEnemyUnit(18,20,6,33,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "cdff55df659dd017c35b5d14d57747550dd03f3db201c6dbe43cb9f9a0d0cd58");
        }

        private static void Case_04771()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4771,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,8,32,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,5,79,4,4), new GeneratedEnemyUnit(-1,7,64,32,3), new GeneratedEnemyUnit(-11,19,66,45,3), new GeneratedEnemyUnit(-14,-10,41,21,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "bf206cac7d6ec796f3e81bcd341d2d5c0299006c184d3728411c2fc63e75448a");
        }

        private static void Case_04772()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4772,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,1,93,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-11,12,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,20,25,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-1,30,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-18,75,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-12,5,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-6,93,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-3,53,50,4), new GeneratedEnemyUnit(-4,10,10,31,2), new GeneratedEnemyUnit(-8,-10,95,25,1), new GeneratedEnemyUnit(17,7,22,16,1), new GeneratedEnemyUnit(-9,-11,90,18,1), new GeneratedEnemyUnit(13,11,14,28,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "daa919bf18612d01adcd6eede97a869d158aaf920b60077418155cdeea62ccd6");
        }

        private static void Case_04773()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4773,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,19,39,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,14,54,40,1), new GeneratedEnemyUnit(-17,-2,92,22,4), new GeneratedEnemyUnit(-12,0,28,28,4), new GeneratedEnemyUnit(-13,-16,16,2,1), new GeneratedEnemyUnit(1,-6,67,6,2), new GeneratedEnemyUnit(-19,15,34,33,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "31c2cd81a5712cba8029d3156549b3a11220b6987674ce629b79125863e56ea4");
        }

        private static void Case_04774()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4774,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-14,78,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-15,13,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,9,69,32,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "5da40a96bbf58ce9e358bade7a19ed83b4ad4bd1570219ad89c4feaaf9413a35");
        }

        private static void Case_04775()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4775,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,15,38,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-9,82,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,17,57,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,16,47,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-15,97,48,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "23cfd638ab65505df900b53dd61ac187ec9df1d22393f655387217157e0ae491");
        }

        private static void Case_04776()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4776,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,19,42,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,1,91,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-11,63,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-4,15,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-18,49,9,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "db45e039e687562006003c2d85a3617f2c2b44b3d3623b027e0cc7c4dc82ba6b");
        }

        private static void Case_04777()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4777,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-2,61,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-20,52,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,0,100,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-8,63,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,11,26,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,8,49,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-2,28,24,3), new GeneratedEnemyUnit(-8,3,87,18,4), new GeneratedEnemyUnit(-8,13,90,50,4), new GeneratedEnemyUnit(-6,-13,27,18,2), new GeneratedEnemyUnit(7,0,97,16,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "51f51784bf545956a91cebb6883c7bd3a3700b122990d3cb2fe7289ac7b7a45e");
        }

        private static void Case_04778()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4778,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,6,20,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-4,67,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-13,14,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,16,69,14,2), new GeneratedEnemyUnit(5,-10,62,35,4), new GeneratedEnemyUnit(18,11,32,40,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "70e2f7e03c1888a005010ec96672966f2451c45328f2f6acd9214a403f4039fa");
        }

        private static void Case_04779()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4779,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,19,91,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,15,74,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-19,82,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-14,33,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,14,23,22,3), new GeneratedEnemyUnit(4,-13,35,11,2), new GeneratedEnemyUnit(-1,-20,50,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "e45f1621bbf30d60e475d940357780a05e6ec82d208a08d9ccfbd5a3f7a4a14c");
        }

        private static void Case_04780()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4780,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,20,31,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,2,88,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-10,16,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-13,82,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,14,54,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-6,14,44,4), new GeneratedEnemyUnit(-19,1,92,49,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "05ad9c9309aef6a8efbc36b10746d9fa08bc83f2629e7d308e23337b025f8fce");
        }

        private static void Case_04781()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4781,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,17,73,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,12,37,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,17,88,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,16,48,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,11,33,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,9,22,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-19,19,3,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "d479bddbf5d632dbfb744e077b4997edf22b91bf1d261dccde1f7b84ca356f75");
        }

        private static void Case_04782()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4782,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-8,80,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-8,95,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-5,58,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-12,62,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,3,71,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-19,41,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,6,69,28,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "3b108b417a0950c770e0d329fcf4aee1199331bad477a4d44b5148ba93f8a8d8");
        }

        private static void Case_04783()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4783,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,17,76,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,17,69,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-15,6,46,4), new GeneratedEnemyUnit(-1,-18,64,26,4), new GeneratedEnemyUnit(8,7,25,48,2), new GeneratedEnemyUnit(-19,-6,39,12,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "97ceade3f30dae6b8931c45688aeb7669611fac0ee18a9668249f7dee7f12a68");
        }

        private static void Case_04784()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4784,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,3,86,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-13,32,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,19,49,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,5,16,48,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "9deeab809790d066068af2c1728988c96a44a844189b902ef1c1c1fb7d951c0a");
        }

        private static void Case_04785()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4785,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-12,22,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,3,41,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,14,48,50,3), new GeneratedEnemyUnit(-7,15,68,46,1), new GeneratedEnemyUnit(13,17,84,25,3), new GeneratedEnemyUnit(-10,-18,48,16,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "a22923f67f83084eb4986a124e4fb6da6349990d3b3b25c1e9961e7f579d0a66");
        }

        private static void Case_04786()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4786,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-11,96,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,9,19,1,3), new GeneratedEnemyUnit(-11,16,41,2,4), new GeneratedEnemyUnit(12,-10,34,46,3), new GeneratedEnemyUnit(11,-13,26,13,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "494481f0c96336a9695e51a864bd7ceb03091173405016857fe0d84f155b1016");
        }

        private static void Case_04787()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4787,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-1,77,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-17,34,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-16,17,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,17,60,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,18,94,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-8,46,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-8,91,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,17,52,32,1), new GeneratedEnemyUnit(3,-6,16,49,2), new GeneratedEnemyUnit(8,-19,84,3,2), new GeneratedEnemyUnit(1,4,45,2,1), new GeneratedEnemyUnit(-14,-16,99,43,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "df45edf6e3ff77b5b98283b86351b3f3aa5114585f2f15867853ff0505a9f02c");
        }

        private static void Case_04788()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4788,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,11,91,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,17,58,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,6,19,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-10,26,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-11,95,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-9,71,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,4,81,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-19,36,49,4), new GeneratedEnemyUnit(13,-10,64,26,2), new GeneratedEnemyUnit(-16,-17,17,2,4), new GeneratedEnemyUnit(1,-5,94,19,4), new GeneratedEnemyUnit(-1,-15,100,34,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "aa26171c3ca55d06fcb6db20460658170d5ddeee94af84eccded76d5225bbcd2");
        }

        private static void Case_04789()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4789,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-4,59,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,16,36,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-20,34,11,3), new GeneratedEnemyUnit(15,7,42,25,4), new GeneratedEnemyUnit(19,7,83,23,3), new GeneratedEnemyUnit(-10,1,6,36,4), new GeneratedEnemyUnit(17,9,98,39,4), new GeneratedEnemyUnit(-17,-8,38,30,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "6d47372702fa87852e48244691f26d382cf3c7195bedcfe3eb84d7d1e738fb63");
        }

        private static void Case_04790()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4790,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-7,48,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-18,12,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-5,68,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,7,49,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-3,45,41,4), new GeneratedEnemyUnit(3,5,73,37,3), new GeneratedEnemyUnit(-13,-2,76,46,2), new GeneratedEnemyUnit(15,11,77,19,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8c80a5e46a0abb9bb09c5b90777361b866fc7488885298dce429423baf45efde");
        }

        private static void Case_04791()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4791,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-1,28,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,8,58,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,7,99,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-11,68,50,1), new GeneratedEnemyUnit(4,-13,66,25,2), new GeneratedEnemyUnit(14,9,72,29,2), new GeneratedEnemyUnit(-19,-7,54,19,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "f8b08317ccd79a3b1be720d9a9a7e3d1b4de38660ea19314eac0f9c82c47ceed");
        }

        private static void Case_04792()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4792,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,3,85,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-14,5,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-3,52,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,0,55,1,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "2ccad559efb0efec1d032c7e44ecc278f20b0e1d6482fbface44052ad86cb2fb");
        }

        private static void Case_04793()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4793,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,18,32,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-7,93,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-19,89,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,1,52,14,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "3e3ef54e74b2dbaf20d0582b876d38b5542e7845accb53e75abaff36fe8e572b");
        }

        private static void Case_04794()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4794,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-14,91,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,16,57,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-11,76,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,5,23,23,4), new GeneratedEnemyUnit(17,4,81,29,4), new GeneratedEnemyUnit(-2,-9,33,5,4), new GeneratedEnemyUnit(15,10,32,29,3), new GeneratedEnemyUnit(6,-1,30,34,4), new GeneratedEnemyUnit(11,15,72,10,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "4c137f794a55ea6840923ce60f08ec3fde9bf9873f1da5872735c5575d7f6ccb");
        }

        private static void Case_04795()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4795,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,2,66,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-14,48,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,6,51,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,6,42,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,18,12,13,1), new GeneratedEnemyUnit(7,-5,67,25,2), new GeneratedEnemyUnit(-6,-7,18,6,3), new GeneratedEnemyUnit(-17,10,67,49,4), new GeneratedEnemyUnit(-19,12,19,27,1), new GeneratedEnemyUnit(-4,-9,35,2,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "a3fc01b3cf81030bf4c8769c95736d96ddf19ac5f4990ccbf52df83231396ec3");
        }

        private static void Case_04796()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4796,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-14,93,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,14,57,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-15,54,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-15,10,12,2), new GeneratedEnemyUnit(2,2,34,30,2), new GeneratedEnemyUnit(-12,-8,76,18,4), new GeneratedEnemyUnit(0,4,46,44,3), new GeneratedEnemyUnit(-4,-14,33,31,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "cecd148b1d12085a3a2c204a4817490b9ccddda96f2b3452c52920cc1a4127fb");
        }

        private static void Case_04797()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4797,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-16,10,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-20,18,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-3,54,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,14,57,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-6,71,10,3), new GeneratedEnemyUnit(-20,5,100,34,2), new GeneratedEnemyUnit(0,20,20,15,2), new GeneratedEnemyUnit(19,-19,90,44,4), new GeneratedEnemyUnit(1,4,18,13,3), new GeneratedEnemyUnit(9,-3,62,24,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "37d10b02154cf315798c907ae363cd8d0dd3ab530c4e535d4d3cde2665dfc965");
        }

        private static void Case_04798()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4798,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,2,23,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,12,84,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,8,91,38,1), new GeneratedEnemyUnit(-11,2,51,43,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "4139109db8c77d54ebba5e1a66e084452b2222253390e7fb52a52beb637e8a72");
        }

        private static void Case_04799()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4799,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-11,73,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-18,74,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,2,14,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,12,5,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,15,8,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,0,74,30,2), new GeneratedEnemyUnit(5,-4,37,18,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "69ecd617ede293d30a35da3970790339974c7ce59c93a804be13f281112dccb0");
        }

        private static void Case_04800()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4800,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,8,72,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-12,85,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,14,40,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-7,16,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,3,39,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,12,15,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,17,32,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-14,81,46,1), new GeneratedEnemyUnit(0,10,86,13,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "1860c702cfc69c333286fb037b8080b162c424e0c47e02cae81a16370cd5a161");
        }

        private static void Case_04801()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4801,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-14,67,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-12,62,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,0,42,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-9,78,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-13,75,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,18,37,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-4,74,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-8,45,5,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "e4873332880ed0667d3c488b0c3f85ced0c641f065ee74884ca65d481a90e109");
        }

        private static void Case_04802()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4802,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,13,15,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-6,29,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-3,42,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,7,94,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,17,18,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,17,88,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-19,97,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-19,40,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-20,57,36,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "80d6d9afdac95381d7af76d29c10586438b2dc1ce853834721e2be0f681064b0");
        }

        private static void Case_04803()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4803,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-13,90,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-15,23,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,2,37,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-16,74,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-6,60,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,19,42,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,18,79,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,8,63,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-7,52,17,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "28e47ffcc89376009a1d3030d93928e7c78464e99b8a9ee671db903011e05db6");
        }

        private static void Case_04804()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4804,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,0,25,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,4,61,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,7,25,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-11,81,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,17,10,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-9,8,6,3), new GeneratedEnemyUnit(17,16,82,46,3), new GeneratedEnemyUnit(15,11,67,35,3), new GeneratedEnemyUnit(-15,16,36,6,1), new GeneratedEnemyUnit(-14,-20,63,5,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "fa5f38c96bfb3a64738b6da575a0c88ae8b24e31fee866fa36c4f6ac4604148a");
        }

        private static void Case_04805()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4805,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-12,94,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,19,77,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-17,62,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,12,88,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,13,69,49,3), new GeneratedEnemyUnit(6,8,79,43,1), new GeneratedEnemyUnit(15,7,47,50,2), new GeneratedEnemyUnit(20,8,17,4,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "75e87e2b12acbb3ae1f300b0775e886d97645f5785129b0f60d1d61310e1cd40");
        }

        private static void Case_04806()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4806,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-12,40,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-19,41,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-20,24,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-14,31,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,14,68,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-6,64,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,14,45,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,8,40,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-12,23,16,1), new GeneratedEnemyUnit(6,7,35,19,3), new GeneratedEnemyUnit(-1,-6,86,39,2), new GeneratedEnemyUnit(-15,-13,83,28,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "9b595fde96cc892169547a76d5a2b7a05a4f551a15a53b15cd157bf3ef1ea77c");
        }

        private static void Case_04807()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4807,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-11,93,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,15,80,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,16,74,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,11,90,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,11,61,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-13,27,25,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "b78779320a93ec2f1277f33daf202abd62bdfee2c430d003112ebf6485b40560");
        }

        private static void Case_04808()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4808,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-20,85,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,16,35,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,6,58,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-17,91,50,1), new GeneratedEnemyUnit(3,10,15,21,4), new GeneratedEnemyUnit(-5,-16,5,27,1), new GeneratedEnemyUnit(-19,-19,22,21,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "a1befd41cbe4fc17ca984c1a5d017e5f68420f552983c71e0634f009e47dd778");
        }

        private static void Case_04809()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4809,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-14,94,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,12,52,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,10,82,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,19,25,25,1), new GeneratedEnemyUnit(15,-19,9,44,4), new GeneratedEnemyUnit(-16,18,36,41,3), new GeneratedEnemyUnit(-12,-8,49,16,1), new GeneratedEnemyUnit(13,16,63,44,3), new GeneratedEnemyUnit(-16,17,25,46,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "3148944d86027e810fc191444462406b7d933f43917524410a2580edb41b32b6");
        }

        private static void Case_04810()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4810,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,2,54,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-12,97,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-9,34,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,14,31,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-9,70,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-14,100,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-17,53,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-8,27,49,4), new GeneratedEnemyUnit(20,14,52,35,3), new GeneratedEnemyUnit(-15,-15,20,14,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "d9b5931e6422b8823629783ee1fbcd199f3cec58124647d25a7d251b446d8864");
        }

        private static void Case_04811()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4811,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-16,96,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-14,68,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-10,25,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,8,24,6,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "0ca8fd4e2e1de0b28064e5e3d697480bb0c510dfddd0d33d5d9c5d8b04cd2d84");
        }

        private static void Case_04812()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4812,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,14,15,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-16,17,30,3), new GeneratedEnemyUnit(-4,-8,13,23,1), new GeneratedEnemyUnit(-10,-15,13,31,1), new GeneratedEnemyUnit(5,-7,14,7,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "de6f87edb58c4c170d27a528b853f2e21f2fd7d13a0ec147ba00e352bf10d57f");
        }

        private static void Case_04813()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4813,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-9,74,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,14,5,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,4,12,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-12,81,19,4), new GeneratedEnemyUnit(-10,6,95,12,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "4eed29e1baefcbcb8bf963935506bc1a4e5c384ef33a8bc8d60cf5a40360a089");
        }

        private static void Case_04814()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4814,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,4,66,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,1,25,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,12,86,1,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "32090d23bedebd415a2abb411377d64831726a621928a5151889356955c1a2d8");
        }

        private static void Case_04815()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4815,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-12,64,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-1,69,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,0,13,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,14,17,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,10,11,32,4), new GeneratedEnemyUnit(-18,-16,98,2,4), new GeneratedEnemyUnit(-15,-16,58,17,4), new GeneratedEnemyUnit(9,15,10,36,2), new GeneratedEnemyUnit(19,-8,86,19,2), new GeneratedEnemyUnit(-2,-1,49,45,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "e2acd45f7fb80e24917063a94467c4fa68ec4ad6d4d4547848ecdfed4d9b39be");
        }

        private static void Case_04816()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4816,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,17,50,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,18,10,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,14,61,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,4,74,18,3), new GeneratedEnemyUnit(19,-5,66,3,1), new GeneratedEnemyUnit(13,3,73,2,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c8cc63638c021969b05e2d59cf064c57e585202ff0e70727731fedca1e6bc974");
        }

        private static void Case_04817()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4817,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,2,63,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,16,80,40,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "0fc17ec11006bbb95423e19b5182fdeae12ca6473773cdefb5b2b63316ba7676");
        }

        private static void Case_04818()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4818,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-1,35,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,15,16,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,17,64,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,18,72,44,1), new GeneratedEnemyUnit(-13,-15,79,35,2), new GeneratedEnemyUnit(-7,-6,52,19,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5030befe4469a002999c5be79d079b92f0bf1b6a18660be72ca817e6bd4049e3");
        }

        private static void Case_04819()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4819,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-4,43,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,11,60,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-7,32,11,2), new GeneratedEnemyUnit(11,17,46,43,2), new GeneratedEnemyUnit(0,-18,85,18,1), new GeneratedEnemyUnit(-16,8,23,16,1), new GeneratedEnemyUnit(0,-13,25,17,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "c1342f9992c9496bf101cac1b9894444ed1ee088f6ec1194d683b9fd841c1e5f");
        }

        private static void Case_04820()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4820,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,18,8,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-10,16,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-16,18,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-12,85,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-7,74,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-18,8,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-11,71,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-5,10,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-2,7,27,1), new GeneratedEnemyUnit(-5,16,27,16,1), new GeneratedEnemyUnit(0,-18,49,26,3), new GeneratedEnemyUnit(-3,5,58,2,2), new GeneratedEnemyUnit(15,-17,26,7,2), new GeneratedEnemyUnit(-9,-9,47,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "d19dc952a6fa287f37502af2b54cef1c9459d01b5dc680002ff09be13b387f0d");
        }

        private static void Case_04821()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4821,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,8,15,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-16,19,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-3,36,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-1,54,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-11,30,4,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "6cf9467098755e2bd0903328824ec1a086142d8bbb3f52fed46208da1de9893e");
        }

        private static void Case_04822()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4822,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-20,15,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-5,82,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,5,62,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-1,96,3,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "541dd3deb94f891d701399083df404fada4ddd38a4cadaf155d2582cabdf2ea1");
        }

        private static void Case_04823()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4823,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,6,87,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,0,100,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,17,34,4,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "a0410291b34ffa34158a34c36cdda88718934e38b4ca87bce872fb528b63050c");
        }

        private static void Case_04824()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4824,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-5,21,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-20,72,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-1,94,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,11,17,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,20,47,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-9,77,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-16,30,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,5,62,48,1), new GeneratedEnemyUnit(-11,7,61,22,3), new GeneratedEnemyUnit(4,-18,12,39,3), new GeneratedEnemyUnit(-11,-6,80,17,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "9e8045e22b0139c4b2bd3490749f4c8debf08fc25afbae30edad5c533325746f");
        }

        private static void Case_04825()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4825,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,0,44,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,10,68,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-6,43,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-16,77,8,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "fae4c6508f24262bf842f805b52f41bf264da67ba3cf39dc7cf0da14310c4f03");
        }

        private static void Case_04826()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4826,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-7,86,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-17,14,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-10,67,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-19,21,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,3,99,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-12,80,6,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "e4195258743faa4e76084f7f8f4bd40edaa1754c9788cee452a8946319e3298e");
        }

        private static void Case_04827()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4827,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-11,92,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,15,29,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-11,86,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,14,54,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-14,92,7,4), new GeneratedEnemyUnit(14,0,61,50,2), new GeneratedEnemyUnit(8,5,30,6,3), new GeneratedEnemyUnit(-17,4,95,1,4), new GeneratedEnemyUnit(-4,15,9,7,4), new GeneratedEnemyUnit(-9,4,41,17,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "09dea58b67d387bafea36fd063cce76690f2f801fe02de4ffa904c0f2d522b85");
        }

        private static void Case_04828()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4828,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,15,44,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,19,59,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,2,68,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,3,41,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,5,25,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-2,6,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,8,90,5,4), new GeneratedEnemyUnit(-6,10,69,46,1), new GeneratedEnemyUnit(20,15,62,2,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "1eeba2a7d33f559365617e906d08fdc5e11f482d91697e7ef0da26583720edcd");
        }

        private static void Case_04829()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4829,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,3,8,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-17,44,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,11,94,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,18,28,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,18,77,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-3,16,23,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1ff76fe995497606162c7f6b2995d90493a891ef2cca98649c8e9e0550f15934");
        }

        private static void Case_04830()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4830,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-17,31,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-4,25,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,19,43,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-20,9,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-9,94,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-16,91,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-1,87,45,1), new GeneratedEnemyUnit(15,-2,97,50,3), new GeneratedEnemyUnit(-17,-18,19,3,3), new GeneratedEnemyUnit(5,-1,31,43,4), new GeneratedEnemyUnit(-17,4,96,37,3), new GeneratedEnemyUnit(-3,3,63,34,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "7db4c29ddcc4d9a146df2102077c9999f61cfbd7342be294cc38a030f6fda1c3");
        }

        private static void Case_04831()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4831,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-10,93,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-2,61,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-18,29,49,2), new GeneratedEnemyUnit(-10,11,54,29,3), new GeneratedEnemyUnit(14,-2,21,25,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "083e86e04b3b4016a19704229bc226949a68207439715332da5bec1720c6d846");
        }

        private static void Case_04832()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4832,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-4,84,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-16,22,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,20,65,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-6,55,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,8,19,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-4,69,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,11,18,10,4), new GeneratedEnemyUnit(-6,-17,64,48,2), new GeneratedEnemyUnit(-20,-15,83,42,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "23a36e4d720acb2e1c9dfe87728e0d054041d5e83969a2e255b99ec21b58ec92");
        }

        private static void Case_04833()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4833,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-5,96,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,1,87,20,2), new GeneratedEnemyUnit(20,-15,14,35,2), new GeneratedEnemyUnit(-16,-3,95,15,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b2a95f8f6a9c80296109d09ffaa20f7e7fe466fd68d548295cbe1951ba581562");
        }

        private static void Case_04834()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4834,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,20,70,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-19,78,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,14,96,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,6,96,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,15,67,42,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "5b6fb3d63d86a0fbc0274bbd26fad11d3c585704bc717b2a24e0ca8f94230212");
        }

        private static void Case_04835()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4835,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,14,48,2,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "ddfa359d0b40e5c8e44cd367ff05cb7073b028fae17d2c5e881bf1bb2cf1b83a");
        }

        private static void Case_04836()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4836,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-13,88,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-13,9,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-9,38,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-3,32,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,17,6,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,10,75,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,20,71,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,2,10,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,1,36,28,3), new GeneratedEnemyUnit(-7,-16,21,8,1), new GeneratedEnemyUnit(-4,-15,98,7,3), new GeneratedEnemyUnit(15,1,91,6,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "579771e4ca82c50cbc102271f950ff0b1755e605990941bc93c935bc3cb39b96");
        }

        private static void Case_04837()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4837,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-8,73,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-8,92,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-7,27,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-17,20,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-16,22,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,11,34,6,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "0fe63673d47b33abec2c198316fdab7b9dad203cb98d8e7126d7858c58fdfa89");
        }

        private static void Case_04838()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4838,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-14,92,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-2,14,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-2,25,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-14,24,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-8,20,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-7,67,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-8,85,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,17,63,50,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "b10851c8d97d5fb533a09f0e24c39f7310e86e134207d10bad97c5e348c85a72");
        }

        private static void Case_04839()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4839,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,11,18,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,20,75,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,3,34,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,19,98,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-14,36,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-3,37,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-7,21,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,3,6,34,1), new GeneratedEnemyUnit(-13,2,98,34,2), new GeneratedEnemyUnit(-4,-11,8,19,2), new GeneratedEnemyUnit(9,-9,85,41,4), new GeneratedEnemyUnit(8,20,67,1,4), new GeneratedEnemyUnit(13,-17,20,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "30872a540e501e910d3cc366f48ecc17cc88a5430d371452bab68cf64d7646eb");
        }

        private static void Case_04840()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4840,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-11,62,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,20,13,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-7,18,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,0,70,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,1,58,45,2), new GeneratedEnemyUnit(6,-2,38,45,1), new GeneratedEnemyUnit(2,-9,73,9,1), new GeneratedEnemyUnit(7,-12,91,30,4), new GeneratedEnemyUnit(6,16,77,32,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "96b3cf53e258620f392851e8fef6508e3b52ea32a28b83dc983768af868e72ba");
        }

        private static void Case_04841()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4841,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,15,53,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,9,59,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,5,100,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-4,100,39,2), new GeneratedEnemyUnit(-3,-16,28,3,4), new GeneratedEnemyUnit(-13,17,51,18,3), new GeneratedEnemyUnit(7,-5,83,46,1), new GeneratedEnemyUnit(3,2,57,34,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "f5c8c55af4cff1a71dad8528e388f240a2e37e2c674bfb5418acdca80366a684");
        }

        private static void Case_04842()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4842,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,15,43,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,19,40,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,11,63,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,8,50,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-14,15,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-1,70,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,1,95,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,6,65,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-18,35,6,4), new GeneratedEnemyUnit(11,-2,98,14,3), new GeneratedEnemyUnit(7,-6,31,11,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 85,
                stableHash: "70da8670e98df845feef667ea4e4c3aacadeca2290ac02644fb2df7624636dae");
        }

        private static void Case_04843()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4843,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-5,7,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-17,62,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-2,12,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-5,76,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-3,100,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,18,17,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-15,14,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-14,100,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-13,81,32,3), new GeneratedEnemyUnit(-5,-20,48,2,4), new GeneratedEnemyUnit(10,-14,7,7,3), new GeneratedEnemyUnit(7,7,73,48,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "b86f0d4ef220e8a7b04951b48de4f5c92e652a3447f80a28b410948ac0f9a2fb");
        }

        private static void Case_04844()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4844,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-3,42,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,19,64,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,12,45,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,4,30,28,2), new GeneratedEnemyUnit(17,7,80,3,4), new GeneratedEnemyUnit(18,-7,9,31,2), new GeneratedEnemyUnit(12,9,96,14,1), new GeneratedEnemyUnit(-11,7,55,1,4), new GeneratedEnemyUnit(-8,14,49,29,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "ca67bed4119adf6e04990631de92026e3bd0c9be999281475c02b77778557396");
        }

        private static void Case_04845()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4845,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-16,34,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,8,70,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,2,14,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,1,10,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,5,35,31,2), new GeneratedEnemyUnit(-9,-3,64,43,1), new GeneratedEnemyUnit(-11,17,78,41,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "c53bc8e318bc3318552b76c49fc8c30923f2b3394f9fbcec83ce0290f534c970");
        }

        private static void Case_04846()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4846,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-8,70,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-2,69,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-4,56,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-6,38,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-3,74,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,19,100,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-17,28,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-3,15,49,4), new GeneratedEnemyUnit(-9,7,27,39,3), new GeneratedEnemyUnit(5,-5,98,33,2), new GeneratedEnemyUnit(-16,-15,11,40,1), new GeneratedEnemyUnit(-19,12,76,30,1), new GeneratedEnemyUnit(-2,15,47,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "986f78f03846f0c2081592d2a6e3f4fe398f1efcf9fdb7864c70c1c104cf4ecd");
        }

        private static void Case_04847()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4847,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-8,5,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,12,38,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,6,45,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-2,63,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,3,11,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,7,72,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-17,11,8,1), new GeneratedEnemyUnit(7,20,47,50,1), new GeneratedEnemyUnit(-8,19,31,33,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "461ca1699692e48d60bc97db3752da65695f095dbb5873492a761eb6873d407c");
        }

        private static void Case_04848()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4848,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,10,6,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-11,64,9,4), new GeneratedEnemyUnit(-16,1,45,6,4), new GeneratedEnemyUnit(3,-4,89,49,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "f31db8168d806d66ebb1517dcb60fc7a5a677c3b3e0879a6efa50e3c48d58a37");
        }

        private static void Case_04849()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4849,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,4,76,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,18,81,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,4,89,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-2,62,2,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "52c5f14647133bcfd438740c9bec062f8ed6ce275ae16cad5a95e1e73bdea5d6");
        }

        private static void Case_04850()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4850,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-8,74,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,17,32,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-6,16,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,18,54,33,3), new GeneratedEnemyUnit(1,-1,67,38,4), new GeneratedEnemyUnit(15,7,70,11,3), new GeneratedEnemyUnit(-16,-11,81,6,2), new GeneratedEnemyUnit(-18,4,59,19,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e296745de3ff2697716c2314a836c965a2b3fc24e8bad9d380d5d5a7c935d1d5");
        }

        private static void Case_04851()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4851,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,0,30,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,16,14,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-15,97,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-10,68,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,4,78,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,20,39,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-12,58,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-2,80,44,1), new GeneratedEnemyUnit(-9,-15,48,26,4), new GeneratedEnemyUnit(-15,14,79,27,1), new GeneratedEnemyUnit(17,-11,72,23,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e1de65a7592ed440fc3c1bd98b4d7e8764217c8dbef76559ce7d77f43a289698");
        }

        private static void Case_04852()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4852,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,3,39,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,10,21,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,13,8,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-7,60,2,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "ccd75abda221523488d76612945f6ec1c055db8ddfbc64eb9131d26acfc03d9c");
        }

        private static void Case_04853()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4853,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,11,71,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-17,89,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,20,81,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,17,88,30,2), new GeneratedEnemyUnit(-7,-1,76,47,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "f8cb747b10fc8644bfe07604858b77e49d9a20188ce60c94e12b081145efdb1e");
        }

        private static void Case_04854()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4854,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,4,64,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,10,10,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,8,13,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,8,91,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,19,32,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-8,87,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,14,34,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-6,64,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-8,22,18,3), new GeneratedEnemyUnit(-16,12,30,30,4), new GeneratedEnemyUnit(-3,12,21,37,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "075fc70763e225106ace9cdb248fa4da7cb898936c207f9f810e9e7a9cd16430");
        }

        private static void Case_04855()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4855,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,12,79,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,17,32,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,3,50,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,0,44,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,6,8,10,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "f744ac37f0e4a9e5f48fd88f79ec7368e031c38ff3a2251788c3952bfbdf90ce");
        }

        private static void Case_04856()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4856,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-7,54,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,4,26,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,17,71,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,0,100,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-18,18,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,20,91,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,19,60,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,5,51,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-12,51,31,4), new GeneratedEnemyUnit(5,15,28,4,1), new GeneratedEnemyUnit(8,-13,81,11,1), new GeneratedEnemyUnit(17,-2,38,2,2), new GeneratedEnemyUnit(-11,2,82,45,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "6485651243b1067a71427bd5ffe9dc49e5379184ca7f5496e3d938a9e5fbb431");
        }

        private static void Case_04857()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4857,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,15,87,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-17,48,34,4), new GeneratedEnemyUnit(-15,16,50,50,2), new GeneratedEnemyUnit(-8,0,96,21,3), new GeneratedEnemyUnit(-2,15,99,12,2), new GeneratedEnemyUnit(7,-16,24,3,2), new GeneratedEnemyUnit(0,15,60,28,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "e3494c4b8d5498be32d2520352ed8fb0fd29e78963fbe0f81bf4aad019b378b6");
        }

        private static void Case_04858()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4858,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,7,35,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-9,70,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-5,19,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,19,6,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-17,84,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-2,76,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-10,77,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-13,75,34,3), new GeneratedEnemyUnit(-15,15,40,1,4), new GeneratedEnemyUnit(4,-8,57,1,1), new GeneratedEnemyUnit(20,-3,90,6,4), new GeneratedEnemyUnit(7,-18,46,12,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "f6e4e3ab89f82b6760edf516508529bc95a6189c3e29d7ce2c31d08f88542268");
        }

        private static void Case_04859()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4859,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,13,7,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-9,90,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-17,62,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-15,61,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,17,46,2,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "0274051865cd2788add92f7373874bf8c4703e7422ff99efced9ef406c73b876");
        }

    }
}
