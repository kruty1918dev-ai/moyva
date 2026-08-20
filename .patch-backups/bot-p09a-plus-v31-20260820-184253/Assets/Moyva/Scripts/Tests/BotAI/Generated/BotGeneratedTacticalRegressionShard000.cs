using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard000
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_00000();
            Case_00001();
            Case_00002();
            Case_00003();
            Case_00004();
            Case_00005();
            Case_00006();
            Case_00007();
            Case_00008();
            Case_00009();
            Case_00010();
            Case_00011();
            Case_00012();
            Case_00013();
            Case_00014();
            Case_00015();
            Case_00016();
            Case_00017();
            Case_00018();
            Case_00019();
            Case_00020();
            Case_00021();
            Case_00022();
            Case_00023();
            Case_00024();
            Case_00025();
            Case_00026();
            Case_00027();
            Case_00028();
            Case_00029();
            Case_00030();
            Case_00031();
            Case_00032();
            Case_00033();
            Case_00034();
            Case_00035();
            Case_00036();
            Case_00037();
            Case_00038();
            Case_00039();
            Case_00040();
            Case_00041();
            Case_00042();
            Case_00043();
            Case_00044();
            Case_00045();
            Case_00046();
            Case_00047();
            Case_00048();
            Case_00049();
            Case_00050();
            Case_00051();
            Case_00052();
            Case_00053();
            Case_00054();
            Case_00055();
            Case_00056();
            Case_00057();
            Case_00058();
            Case_00059();
            Case_00060();
            Case_00061();
            Case_00062();
            Case_00063();
            Case_00064();
            Case_00065();
            Case_00066();
            Case_00067();
            Case_00068();
            Case_00069();
            Case_00070();
            Case_00071();
            Case_00072();
            Case_00073();
            Case_00074();
            Case_00075();
            Case_00076();
            Case_00077();
            Case_00078();
            Case_00079();
            Case_00080();
            Case_00081();
            Case_00082();
            Case_00083();
            Case_00084();
            Case_00085();
            Case_00086();
            Case_00087();
            Case_00088();
            Case_00089();
            Case_00090();
            Case_00091();
            Case_00092();
            Case_00093();
            Case_00094();
            Case_00095();
            Case_00096();
            Case_00097();
            Case_00098();
            Case_00099();
            Case_00100();
            Case_00101();
            Case_00102();
            Case_00103();
            Case_00104();
            Case_00105();
            Case_00106();
            Case_00107();
            Case_00108();
            Case_00109();
            Case_00110();
            Case_00111();
            Case_00112();
            Case_00113();
            Case_00114();
            Case_00115();
            Case_00116();
            Case_00117();
            Case_00118();
            Case_00119();
            Case_00120();
            Case_00121();
            Case_00122();
            Case_00123();
            Case_00124();
            Case_00125();
            Case_00126();
            Case_00127();
            Case_00128();
            Case_00129();
            Case_00130();
            Case_00131();
            Case_00132();
            Case_00133();
            Case_00134();
            Case_00135();
            Case_00136();
            Case_00137();
            Case_00138();
            Case_00139();
            Case_00140();
            Case_00141();
            Case_00142();
            Case_00143();
            Case_00144();
            Case_00145();
            Case_00146();
            Case_00147();
            Case_00148();
            Case_00149();
            Case_00150();
            Case_00151();
            Case_00152();
            Case_00153();
            Case_00154();
            Case_00155();
            Case_00156();
            Case_00157();
            Case_00158();
            Case_00159();
            Case_00160();
            Case_00161();
            Case_00162();
            Case_00163();
            Case_00164();
            Case_00165();
            Case_00166();
            Case_00167();
            Case_00168();
            Case_00169();
            Case_00170();
            Case_00171();
            Case_00172();
            Case_00173();
            Case_00174();
            Case_00175();
            Case_00176();
            Case_00177();
            Case_00178();
            Case_00179();
        }

        private static void Case_00000()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 0,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-7,45,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-6,51,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,5,62,24,3), new GeneratedEnemyUnit(-14,-6,98,45,1), new GeneratedEnemyUnit(14,-12,26,6,4), new GeneratedEnemyUnit(-7,17,13,14,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "ce46dadad422888b6f95d3c53f97948cc0d15c14c753825ffb93f9b98e718df4");
        }

        private static void Case_00001()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,11,63,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,1,20,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,17,69,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-20,56,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,7,68,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,9,5,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,6,91,1,4), new GeneratedEnemyUnit(14,-16,65,27,1), new GeneratedEnemyUnit(-2,-7,65,45,1), new GeneratedEnemyUnit(20,-11,44,30,3), new GeneratedEnemyUnit(-20,14,56,45,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "3e28f78e6a7bb402410cddd7c638885ca1497662bdfeb7645ed5217b0421f21c");
        }

        private static void Case_00002()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 2,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-13,60,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-20,19,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,12,27,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-15,26,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,18,41,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-15,75,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,11,32,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,13,45,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,10,70,44,1), new GeneratedEnemyUnit(-8,8,82,20,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "22dfb033a4abd20f074c3be815e6555a70262a236c9b1025a2689f7c54107b2f");
        }

        private static void Case_00003()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-5,29,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-10,26,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-2,21,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,11,60,7,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "d516bc38e35326cd3f700b60d17c11cf0fdeb2422a01222423b23c8c031e731e");
        }

        private static void Case_00004()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-15,61,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-3,5,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,3,72,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-9,54,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,7,36,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,18,28,47,1), new GeneratedEnemyUnit(6,20,75,3,3), new GeneratedEnemyUnit(2,-18,25,25,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "eeb8f461665ff6d7948a7a9370374a4742e7d1c18342e58ed1d77610a648845e");
        }

        private static void Case_00005()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-10,98,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,17,42,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-8,79,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-13,17,21,3), new GeneratedEnemyUnit(-12,-20,9,35,3), new GeneratedEnemyUnit(18,-2,37,15,4), new GeneratedEnemyUnit(-12,-12,89,7,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "b1720074acde91bcf3b8daaa37bd03e83d90de3c088aa8e0e7dfdad280d848dd");
        }

        private static void Case_00006()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 6,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-19,16,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-5,10,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-14,39,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-9,91,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,1,64,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,3,77,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-1,90,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,5,86,46,2), new GeneratedEnemyUnit(18,19,38,16,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "7c03837475bc62e79385ef29254b10ea5322386552b72885d583bfd893e2c75c");
        }

        private static void Case_00007()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 7,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,20,41,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,2,100,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-10,69,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-4,22,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,13,13,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-1,53,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-8,56,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-2,90,34,1), new GeneratedEnemyUnit(5,-3,37,6,1), new GeneratedEnemyUnit(10,-18,25,23,4), new GeneratedEnemyUnit(4,20,23,12,3), new GeneratedEnemyUnit(1,-4,66,39,2), new GeneratedEnemyUnit(14,-5,58,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "a1b8b29e7219331995a79772fcc6b801fc591f6614b86abcdcdd353381eb1e7e");
        }

        private static void Case_00008()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 8,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-7,18,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,3,46,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-14,26,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,16,9,13,4), new GeneratedEnemyUnit(9,17,83,36,3), new GeneratedEnemyUnit(-18,16,78,31,4), new GeneratedEnemyUnit(14,11,14,44,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "e88c2cf4175253f290e3f3feda84976a51b1e2cb425df034757cb20fac811302");
        }

        private static void Case_00009()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 9,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-15,87,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-11,94,24,1), new GeneratedEnemyUnit(-18,-15,49,13,2), new GeneratedEnemyUnit(5,-10,69,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "55bbd6b909ab6684c8f150575901883640122612986f82ae01654c4fc43a7aed");
        }

        private static void Case_00010()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 10,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-5,32,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,8,84,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-5,30,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,4,20,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,10,92,33,3), new GeneratedEnemyUnit(-3,-3,43,33,3), new GeneratedEnemyUnit(-1,18,80,24,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "4159cf61d6219506727edd59a341e3a3a68733bb11b811dc634101d71800c28c");
        }

        private static void Case_00011()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 11,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,11,50,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,7,14,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,0,35,39,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "6b6b833dab5b6c2180bcc24a4c1e4a41344769446fa684f8c144088cd1c15e31");
        }

        private static void Case_00012()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 12,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-16,97,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,15,34,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,2,9,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-19,61,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-17,40,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,13,38,19,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "ec5dcad5c09d5b1cb178076e9b66ee6f007702dab7cdcd58542e0080a122e719");
        }

        private static void Case_00013()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 13,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,10,88,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-19,98,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,19,30,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,10,18,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-19,51,50,3), new GeneratedEnemyUnit(1,0,38,22,2), new GeneratedEnemyUnit(18,1,32,40,4), new GeneratedEnemyUnit(13,-15,70,45,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "3ddba4b84cbc8a4d52c2d24c5908fdf58f1facd55695595982c3061076203159");
        }

        private static void Case_00014()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 14,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-7,96,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,15,48,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,8,67,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,1,14,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,10,64,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-12,47,5,4), new GeneratedEnemyUnit(2,3,82,47,1), new GeneratedEnemyUnit(4,9,88,13,4), new GeneratedEnemyUnit(0,-15,19,46,1), new GeneratedEnemyUnit(19,8,57,48,1), new GeneratedEnemyUnit(11,10,22,47,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "eb40bfcac2096d699f2ba4ef716ddfcf8a06871a48fa318c9a4313e43a18730e");
        }

        private static void Case_00015()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 15,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-6,29,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,8,69,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-14,37,38,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b4c53f3a0cc809a58d3024fd3823d952655cce25e85a38e536e50b2dc0795893");
        }

        private static void Case_00016()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 16,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-14,92,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,4,68,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,19,31,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-9,6,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ed67dc133dbfa35bbb0a4d5a430f10464f149c799ee21e433281c60d6adddd37");
        }

        private static void Case_00017()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 17,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,16,86,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,5,67,22,4), new GeneratedEnemyUnit(-8,18,36,12,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "370b77c66c351b0bf5052f971e1a412dfd51614d195487d206117b52e2dcab8b");
        }

        private static void Case_00018()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 18,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-3,40,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,5,87,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,4,27,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-17,58,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-2,34,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-12,8,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-14,28,9,1), new GeneratedEnemyUnit(-1,13,75,8,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "404fb5e5f6574de0d37e4eb710e0f26e5dc1150f6eea0fd7d822451e7f8a5d9a");
        }

        private static void Case_00019()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 19,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-10,99,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-6,10,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-14,70,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,16,52,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-19,82,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-7,39,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-5,57,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-11,68,5,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "cae8da2c6131264f3e6d6ab1d8c9e54112a2a5c9be985c22c4a2630e46a93454");
        }

        private static void Case_00020()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 20,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,6,44,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,15,53,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,1,8,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-7,48,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,13,89,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-7,86,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-6,47,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-18,29,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-20,66,37,3), new GeneratedEnemyUnit(-8,18,6,6,4), new GeneratedEnemyUnit(-5,-5,57,15,3), new GeneratedEnemyUnit(-11,10,72,23,2), new GeneratedEnemyUnit(-19,-15,83,33,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "441c4ff733d62552637cd612a8191417745557472124bc5e09eed3ff9e490cbc");
        }

        private static void Case_00021()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 21,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,18,27,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,14,89,37,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8b100213573bf34387d30476545e875bdf0ce495e32fc06d23b07b3c7312ee61");
        }

        private static void Case_00022()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 22,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-8,22,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-9,66,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-5,100,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-10,18,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,5,39,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-5,76,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-20,18,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-13,46,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,17,69,12,3), new GeneratedEnemyUnit(7,-17,72,31,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "6ffd93c050180afaaebbbd2a1f298b58eef43dab845d0d3f8b273a7f1e2e4b2f");
        }

        private static void Case_00023()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 23,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-20,100,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-6,38,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,11,15,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,11,42,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-17,99,48,3), new GeneratedEnemyUnit(9,-10,42,7,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9bf7793b0537749c5dd2bf6f9171397aacc624e70cf2c5c6ba34a1ac574df0f2");
        }

        private static void Case_00024()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 24,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-6,78,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,20,56,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-9,73,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,16,96,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,18,42,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,17,8,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,2,80,17,1), new GeneratedEnemyUnit(-10,-3,53,12,3), new GeneratedEnemyUnit(13,-10,98,2,2), new GeneratedEnemyUnit(-8,-20,31,40,3), new GeneratedEnemyUnit(-15,11,100,41,2), new GeneratedEnemyUnit(-19,-3,88,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "96abaf74fac933605a4db51c4fb537caee10f08e7234d07859d751133183d8b4");
        }

        private static void Case_00025()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 25,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,15,45,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-2,53,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,1,70,22,2), new GeneratedEnemyUnit(-4,-8,11,5,4), new GeneratedEnemyUnit(2,12,45,8,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "7945d1fe126ae31dc6ae8a26a354ece2cc3c3dc5d7be7697edbbaa837afe47df");
        }

        private static void Case_00026()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 26,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-1,87,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-13,37,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-4,46,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-15,61,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-5,99,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,18,13,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-13,48,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,4,48,44,3), new GeneratedEnemyUnit(4,15,11,30,1), new GeneratedEnemyUnit(6,-8,48,28,2), new GeneratedEnemyUnit(10,9,39,20,4), new GeneratedEnemyUnit(-18,9,62,44,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "e8cf47e10dfd5b9b11c1cf2275c299f1e87bd761d8bf5016167297eedd0e62b9");
        }

        private static void Case_00027()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 27,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,7,43,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-5,87,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-2,61,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,6,23,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-15,88,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-7,12,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,4,67,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,12,75,8,1), new GeneratedEnemyUnit(14,12,93,5,3), new GeneratedEnemyUnit(19,2,54,21,2), new GeneratedEnemyUnit(-8,-15,52,27,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "b9495d407c454a8f27204202e3a555ea792cff9e2a88e11fd5be42af41fa5748");
        }

        private static void Case_00028()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 28,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,11,41,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-13,54,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,8,49,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,3,33,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-5,96,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,15,12,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-1,23,4,4), new GeneratedEnemyUnit(-7,15,99,47,3), new GeneratedEnemyUnit(-17,8,43,29,3), new GeneratedEnemyUnit(-4,-20,35,10,3), new GeneratedEnemyUnit(-4,0,21,26,4), new GeneratedEnemyUnit(18,16,12,30,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "8a774555836f5d352dd4a216c23204f10042c1269bb87e2b685390cf1f2fb361");
        }

        private static void Case_00029()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 29,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-13,89,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,7,78,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,19,95,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,17,84,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-9,76,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,3,60,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,7,92,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,0,32,5,4), new GeneratedEnemyUnit(-5,6,87,35,4), new GeneratedEnemyUnit(4,-7,26,50,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "14d6a5f30c7a300ce14e8893ae3728d57c614b6077d6faf89f5805d9c5fabcd1");
        }

        private static void Case_00030()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 30,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,20,28,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,17,43,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-8,93,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-11,53,46,4), new GeneratedEnemyUnit(-1,6,56,6,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "96aeefc04a105da0f14330d16c814111de94393fba60e7ea91d1113cd0339dfc");
        }

        private static void Case_00031()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 31,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-7,32,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-7,7,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-9,25,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-5,85,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-6,41,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,12,51,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-15,10,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,0,9,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-18,6,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "9dc2384d00d7be6bbcdd0682db948f774faefc555900c868ba992de7a411a555");
        }

        private static void Case_00032()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 32,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,9,7,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-14,99,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,13,49,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-13,18,33,2), new GeneratedEnemyUnit(-17,-1,50,43,1), new GeneratedEnemyUnit(-11,-15,31,39,1), new GeneratedEnemyUnit(-19,-16,55,43,4), new GeneratedEnemyUnit(3,-16,71,17,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "7e117a3f41307e020b6797eeb6793944fb388ab9a4502bcd9a42f67541b955d5");
        }

        private static void Case_00033()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 33,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-9,68,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-18,20,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-20,73,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-16,79,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,6,17,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,5,67,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,9,7,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-6,81,1,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "86f755597e001ae24673d146e1e8f345c2d05fbe859cb1d1c5ffcbcdabdbff1a");
        }

        private static void Case_00034()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 34,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-13,72,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-17,90,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-1,31,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,19,46,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,20,22,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-11,20,19,2), new GeneratedEnemyUnit(-20,8,8,23,4), new GeneratedEnemyUnit(-15,-5,83,38,1), new GeneratedEnemyUnit(-11,-11,93,1,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "de86f1ba81d2d0a478ff7a7d8b320a2820a66b0ed06ee5694df9eb9a983a9df9");
        }

        private static void Case_00035()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 35,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,6,99,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,13,66,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,19,35,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-8,52,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,14,23,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,12,61,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,2,86,26,3), new GeneratedEnemyUnit(-18,10,73,38,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "5ad2824d009c1db8244a06e66250951bd9a0c01c92dbb42886c98a850904bfb9");
        }

        private static void Case_00036()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 36,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,13,17,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-4,6,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,5,90,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,12,36,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,1,45,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-14,34,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-3,70,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-17,64,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,8,26,25,4), new GeneratedEnemyUnit(16,-20,52,15,2), new GeneratedEnemyUnit(3,-10,50,31,1), new GeneratedEnemyUnit(-13,8,91,7,3), new GeneratedEnemyUnit(-17,-2,23,33,1), new GeneratedEnemyUnit(8,-7,46,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "5e664c0556530ce513701a9ac8363200c67f1c42a498803f7a45927a7778ed2b");
        }

        private static void Case_00037()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 37,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-18,98,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-17,26,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,16,96,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-11,66,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-17,65,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-19,62,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,20,77,47,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "b33717903fffb7daf03322e0269e91a300c8bbc3906b963b5ea8e7d5a17cb24e");
        }

        private static void Case_00038()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 38,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-1,69,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-18,8,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,15,93,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,12,55,28,1), new GeneratedEnemyUnit(-9,-7,30,50,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "0f114edf45fc9fe942584ef5b91d983fded605dd54f6cd55fb8fafe5ec7bec97");
        }

        private static void Case_00039()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 39,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-17,73,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,15,94,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,7,97,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-2,64,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,8,37,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-2,96,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,16,48,48,1), new GeneratedEnemyUnit(11,19,38,12,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "21ba86dc26a40698469c86534c05309bbbf875ead5bbcc57ac09a067d9a7bfce");
        }

        private static void Case_00040()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 40,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,3,23,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,5,8,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-7,81,50,3), new GeneratedEnemyUnit(-11,19,53,3,1), new GeneratedEnemyUnit(-17,-6,38,37,1), new GeneratedEnemyUnit(13,-3,82,33,4), new GeneratedEnemyUnit(6,-4,59,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "f1964558c600fa5f4914c48be4173b4366775c088fa2721b0b84d49d8c9d2240");
        }

        private static void Case_00041()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 41,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,19,56,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,20,72,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-14,49,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,8,10,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-10,23,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,10,31,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-19,58,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,18,14,4,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "e90490f2e16bc52ed06aed445173e4f490612420432ad41fb75872359321f503");
        }

        private static void Case_00042()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 42,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-8,11,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-7,95,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,4,44,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-7,45,43,3), new GeneratedEnemyUnit(-2,-15,96,24,1), new GeneratedEnemyUnit(-8,11,30,34,1), new GeneratedEnemyUnit(4,-12,56,21,2), new GeneratedEnemyUnit(6,12,91,2,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "5abc9a5c2d339505fea6a6f0d48ba8d80eb4d4060d22731b42d2a6b6c0378497");
        }

        private static void Case_00043()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 43,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,7,10,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,19,22,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-8,47,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-15,99,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-5,62,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-20,73,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-7,61,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-3,99,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "09edaea7521be710fcb5c48f1f35382dfd6ba7c39620d2241c3b72234445e3dc");
        }

        private static void Case_00044()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 44,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-20,37,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,16,52,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,15,91,6,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "f255294fd465388b356c49e91c5eb2d5df3c3107708e06e21155e08d5e9d0547");
        }

        private static void Case_00045()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 45,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,9,19,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,20,33,36,3), new GeneratedEnemyUnit(-8,5,96,14,3), new GeneratedEnemyUnit(5,-6,12,13,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "0cf3088cd6a9031a31ba4d2e3746c558d5096046cca1924e95aed6bb20d21ad4");
        }

        private static void Case_00046()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 46,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-5,15,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-13,25,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-11,65,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,6,14,8,4), new GeneratedEnemyUnit(-20,4,25,7,1), new GeneratedEnemyUnit(-7,-18,35,40,1), new GeneratedEnemyUnit(19,18,80,1,4), new GeneratedEnemyUnit(-10,1,91,43,4), new GeneratedEnemyUnit(7,17,26,39,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "40d083e08aa4c6505f71cf61dbd50a3ce837afc4c3e7048f6314b9b0f13abdac");
        }

        private static void Case_00047()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 47,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,14,5,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-10,26,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,10,75,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-11,26,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-6,48,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,14,46,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,7,48,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-19,80,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-10,33,13,3), new GeneratedEnemyUnit(3,18,75,33,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "0e4458038fbc661a16141ad723195c8ac91c8c59b541f344a7c2e9ccf294a28f");
        }

        private static void Case_00048()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 48,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,0,92,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,20,89,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,6,100,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-15,11,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-14,81,46,3), new GeneratedEnemyUnit(17,14,73,40,1), new GeneratedEnemyUnit(-20,-20,25,29,3), new GeneratedEnemyUnit(16,19,15,23,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "9ed66caef53cb80dc9a4b351ac89bff04fe56b82d26c0024b37198bc24acc6b7");
        }

        private static void Case_00049()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 49,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-2,84,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-15,38,26,4), new GeneratedEnemyUnit(0,-19,17,25,4), new GeneratedEnemyUnit(-15,5,17,9,4), new GeneratedEnemyUnit(-12,-15,81,15,2), new GeneratedEnemyUnit(-11,3,81,37,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "f425cac88aaec213e0123efe28678fa81fbe595fbadee224fe61a17d343de1f1");
        }

        private static void Case_00050()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 50,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,3,71,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,8,32,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,11,61,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-4,93,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,5,14,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-19,86,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-9,87,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,1,96,3,4), new GeneratedEnemyUnit(-3,9,42,25,2), new GeneratedEnemyUnit(1,9,8,34,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "ebb4e1dd59241bf0eee95a580a082481e4040dbc9e214d49ae21ec8a7f7854b0");
        }

        private static void Case_00051()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 51,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-14,99,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,13,45,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,7,79,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-1,61,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,10,93,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,2,9,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-6,73,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,5,42,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,16,24,24,4), new GeneratedEnemyUnit(-16,-2,38,5,3), new GeneratedEnemyUnit(9,-15,68,43,1), new GeneratedEnemyUnit(15,-2,99,48,2), new GeneratedEnemyUnit(-14,-16,94,20,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "cd6dc8914ab099041cc5853216b5faf1e9beb7d48b757ce2fae647bf98d30b60");
        }

        private static void Case_00052()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 52,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-7,73,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-13,48,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-12,42,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,16,34,46,1), new GeneratedEnemyUnit(-10,15,85,48,3), new GeneratedEnemyUnit(-13,7,51,28,4), new GeneratedEnemyUnit(15,-2,32,36,2), new GeneratedEnemyUnit(-4,14,79,13,1), new GeneratedEnemyUnit(0,6,71,47,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "af87ec47b60e5a69906414986d5ed4088e91cc96a10ffb2a4048fe167f9041d3");
        }

        private static void Case_00053()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 53,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,20,97,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,7,38,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-11,27,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,10,67,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,3,8,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,4,86,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-7,89,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,8,82,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,2,16,16,1), new GeneratedEnemyUnit(10,1,33,1,1), new GeneratedEnemyUnit(14,11,66,1,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "9900f87f66790d1a8f4c70bf376ff2792e3c2933792baf7444fbf67f10a69b4b");
        }

        private static void Case_00054()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 54,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,7,72,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,2,17,4,4), new GeneratedEnemyUnit(15,18,92,18,2), new GeneratedEnemyUnit(-10,-10,62,5,1), new GeneratedEnemyUnit(-17,0,18,48,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "8b554bf125d463f4ba8f8357e2a04074c59a7c73e0ef40a3dc4d1bc9b6ba9a1b");
        }

        private static void Case_00055()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 55,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,15,9,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,10,35,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,9,24,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,7,30,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-7,72,3,4), new GeneratedEnemyUnit(-5,16,74,25,3), new GeneratedEnemyUnit(-15,5,67,6,3), new GeneratedEnemyUnit(8,14,47,26,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "985a22094e992c544f3b984825f05ff92c81653249300760444d26eea68c7426");
        }

        private static void Case_00056()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 56,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,1,74,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-3,42,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,5,58,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,10,42,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,16,33,14,2), new GeneratedEnemyUnit(8,-13,56,33,2), new GeneratedEnemyUnit(-8,-4,98,3,1), new GeneratedEnemyUnit(-8,14,14,7,3), new GeneratedEnemyUnit(14,17,76,3,2), new GeneratedEnemyUnit(-6,-9,77,24,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "61c87ad5111d478e2266642175cea9a3b053a9bb97799ea402e2f4210dd5fa98");
        }

        private static void Case_00057()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 57,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-18,59,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,8,92,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,18,98,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,4,24,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-4,84,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,17,58,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-5,57,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-1,83,48,2), new GeneratedEnemyUnit(8,20,54,7,3), new GeneratedEnemyUnit(-16,9,61,21,1), new GeneratedEnemyUnit(13,-17,53,21,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "fc2e97358e471eeeabaf4c00a0a09fb09223e19a276e61da12cf686729615c85");
        }

        private static void Case_00058()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 58,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,14,61,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,12,9,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-3,22,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,18,83,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-5,97,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-2,83,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-10,75,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-9,36,42,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "b9eb7a1dc88f5c22053401f00caf3c0dcc16a7da733e46e15a3d1ede84e41af9");
        }

        private static void Case_00059()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 59,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-14,89,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-7,49,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-14,51,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,18,90,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,15,66,1,1), new GeneratedEnemyUnit(-18,-19,76,17,3), new GeneratedEnemyUnit(-20,10,58,26,4), new GeneratedEnemyUnit(-20,-6,39,16,1), new GeneratedEnemyUnit(5,3,18,40,1), new GeneratedEnemyUnit(-13,-8,91,29,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "8192acee3cf6b9cfa52d9c7875eb08a6f33a82a4962e6097043471933b2c45d2");
        }

        private static void Case_00060()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 60,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-16,66,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,11,27,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-12,62,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-17,90,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,13,88,39,4), new GeneratedEnemyUnit(-7,19,83,38,2), new GeneratedEnemyUnit(20,8,18,18,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "9be8c655141d3c4a4766d9f757ddaf43000f83f84a344051f0f69c75f012072d");
        }

        private static void Case_00061()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 61,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-14,93,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-11,9,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,11,55,17,3), new GeneratedEnemyUnit(9,-18,39,17,2), new GeneratedEnemyUnit(-16,6,83,38,2), new GeneratedEnemyUnit(-7,1,93,11,3), new GeneratedEnemyUnit(-7,14,8,22,2), new GeneratedEnemyUnit(-3,16,60,24,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "574638ae04d834a91303851849d07be58158ac25dfce277f2f151ce73817dc06");
        }

        private static void Case_00062()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 62,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-13,60,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-13,59,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-11,56,48,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "1699c97c9b806bd65e659b0e67490eb31cb87a31aed3fdc1535e5d685e1ca03d");
        }

        private static void Case_00063()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 63,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-9,14,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-3,12,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-15,21,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,9,73,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,9,53,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,3,67,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-8,87,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,0,32,12,1), new GeneratedEnemyUnit(-13,20,19,21,2), new GeneratedEnemyUnit(3,6,7,29,3), new GeneratedEnemyUnit(-14,-8,84,29,2), new GeneratedEnemyUnit(-14,20,5,10,2), new GeneratedEnemyUnit(-5,-11,85,21,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b09f252c802053c80b8113c607bb8f479ed7dae707158712a272d29772a0b0d1");
        }

        private static void Case_00064()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 64,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-2,23,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,10,20,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,19,7,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,13,100,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-1,11,45,2), new GeneratedEnemyUnit(-16,14,39,39,3), new GeneratedEnemyUnit(-2,-14,85,12,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "c131db5a057aeab9ada65a9a37de2a12983dd54c41be780f85694ac893954ff6");
        }

        private static void Case_00065()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 65,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-14,82,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-3,96,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-4,37,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-5,16,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,6,8,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-19,46,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,0,78,46,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "4f4acbb146c446cb4131a4c1fb3399e3579f81a1137fddda8f48eebf35f23031");
        }

        private static void Case_00066()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 66,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-15,71,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,3,8,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-8,62,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,0,41,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,5,46,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-15,90,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,6,41,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-8,32,10,4), new GeneratedEnemyUnit(9,-20,14,50,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "9c371e640d5640296fc51d8599a51a32556f28d89439462b7397ab12ba41064f");
        }

        private static void Case_00067()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 67,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,18,73,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,9,68,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-7,63,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,18,39,49,2), new GeneratedEnemyUnit(0,-6,81,29,1), new GeneratedEnemyUnit(-7,-7,60,14,1), new GeneratedEnemyUnit(-14,-15,33,14,1), new GeneratedEnemyUnit(-6,-1,60,34,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "acd14fc6f5b02058f27aa36151e1d8a829c870408e7e876fb3fafb2f39331a10");
        }

        private static void Case_00068()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 68,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-19,40,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,11,10,36,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "b231a356082a29c56f5abef6ecf3795dc7e726ee0907792bd2aec033c05f8345");
        }

        private static void Case_00069()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 69,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,13,30,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,7,97,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-10,62,36,1), new GeneratedEnemyUnit(5,-4,5,46,2), new GeneratedEnemyUnit(-5,1,87,36,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "07b5a02800e9cb1b0e657bf7b275b481ea4c7b4fbe8d3d4ce2906f7098b756ee");
        }

        private static void Case_00070()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 70,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-5,35,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-16,7,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-8,81,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,0,27,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-7,94,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,1,14,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,4,32,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,19,62,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-4,34,48,1), new GeneratedEnemyUnit(13,19,26,49,4), new GeneratedEnemyUnit(-5,11,34,41,1), new GeneratedEnemyUnit(-10,19,53,32,1), new GeneratedEnemyUnit(16,-19,19,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "4bd389dac0cb23d6271734969735cda5890ac871c10a9b51059d2d50b469de17");
        }

        private static void Case_00071()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 71,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,5,91,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-19,34,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,7,61,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-17,54,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-18,96,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-10,74,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-2,39,5,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "213c977b02e682c2973e6b44896530b40ab343961cae674628ee80a038d9c379");
        }

        private static void Case_00072()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 72,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,3,97,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-14,100,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-6,79,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-12,95,20,3), new GeneratedEnemyUnit(1,-12,12,44,1), new GeneratedEnemyUnit(-14,14,94,8,1), new GeneratedEnemyUnit(8,-14,46,46,2), new GeneratedEnemyUnit(-16,-3,60,14,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "00dda69562d396b7fad7ccef7448b8332ff832df3b8119cbf8ecbf24274ea753");
        }

        private static void Case_00073()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 73,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,7,16,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-15,82,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-7,63,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-18,41,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,14,67,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,9,29,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-10,36,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-5,60,31,1), new GeneratedEnemyUnit(6,-12,47,29,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "056600d3d3e8c33ac33981935823312fd8e32dafb22bbc2af4c5d9a93bc673a9");
        }

        private static void Case_00074()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 74,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,1,44,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,9,36,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-18,41,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,3,50,48,1), new GeneratedEnemyUnit(-12,-3,14,3,1), new GeneratedEnemyUnit(-5,7,82,5,3), new GeneratedEnemyUnit(18,8,51,38,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a3239b293f3de645c21dea4a7884fbf9a4aa0a1cb924f8b61a274448b8976ff2");
        }

        private static void Case_00075()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 75,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,20,75,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,11,82,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,8,96,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-3,38,41,1), new GeneratedEnemyUnit(11,16,34,49,4), new GeneratedEnemyUnit(-11,8,12,24,1), new GeneratedEnemyUnit(-10,-17,56,6,4), new GeneratedEnemyUnit(-17,-20,21,45,1), new GeneratedEnemyUnit(-5,-19,37,40,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "be249ee68a676e1df2a15420ed86876f4bd985d520448771e735442381550d39");
        }

        private static void Case_00076()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 76,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,8,42,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-8,50,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,7,49,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,4,61,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,20,22,20,3), new GeneratedEnemyUnit(20,4,62,36,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "2c9edcf3e04437c10f9688a139c0594745fe36902c2177f9708dcc01dec09b44");
        }

        private static void Case_00077()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 77,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,11,26,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-12,36,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-15,26,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "32f7ee3f6c9c792061cce3b87c3991158ddb14d96ce12336a0013955efa4c57d");
        }

        private static void Case_00078()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 78,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,3,79,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-20,84,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-11,22,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-19,25,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-3,47,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,20,65,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,12,29,31,2), new GeneratedEnemyUnit(10,-6,43,25,1), new GeneratedEnemyUnit(2,-7,65,16,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "cd7b55bbd65e98e81e58a7dec29373fe00868a39ecc72345eda5aac52d34d8a4");
        }

        private static void Case_00079()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 79,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,19,87,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-12,12,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,3,5,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,15,27,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-4,99,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,9,98,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-6,68,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-10,85,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-14,90,6,2), new GeneratedEnemyUnit(19,20,24,46,1), new GeneratedEnemyUnit(20,17,42,38,2), new GeneratedEnemyUnit(10,3,23,47,3), new GeneratedEnemyUnit(-9,-10,49,22,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "98ee7a953a82163f57d1aca69ca3d90c8270259e74e6d71ce8167d9df14d2915");
        }

        private static void Case_00080()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 80,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-12,28,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-11,56,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,11,13,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-7,72,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,0,58,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,9,18,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,17,93,33,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "4837cffdd0019280143dd9ec223caa35cfc4aa2c1d5349b2758fe423b8ee913a");
        }

        private static void Case_00081()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 81,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,3,37,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-18,99,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,9,37,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,17,23,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-17,38,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,0,31,39,3), new GeneratedEnemyUnit(-3,-1,83,42,3), new GeneratedEnemyUnit(-1,3,96,31,2), new GeneratedEnemyUnit(-12,-6,48,17,3), new GeneratedEnemyUnit(16,4,40,7,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "225595b2e7051c113a2b53345eb7e10526441ab3bce10b385263ae84b02d93bd");
        }

        private static void Case_00082()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 82,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-12,12,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,4,54,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,3,95,40,4), new GeneratedEnemyUnit(2,-19,59,6,3), new GeneratedEnemyUnit(-11,-10,7,9,2), new GeneratedEnemyUnit(-6,-2,11,44,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "5eae5940c613c1eb32828b062823b31ab652849e4329ddc3079659536609a61a");
        }

        private static void Case_00083()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 83,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,4,28,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-14,28,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,19,75,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-2,49,16,2), new GeneratedEnemyUnit(-17,-13,82,47,3), new GeneratedEnemyUnit(12,19,61,2,2), new GeneratedEnemyUnit(-20,-17,17,21,2), new GeneratedEnemyUnit(0,-16,100,27,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "ba95877d5a653b6a2f2c8c560a988cee4d3e5f69dd7ee4db2b9735f7db637222");
        }

        private static void Case_00084()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 84,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,6,78,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,10,57,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,11,90,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,10,43,15,3), new GeneratedEnemyUnit(-4,10,44,12,2), new GeneratedEnemyUnit(14,4,54,44,2), new GeneratedEnemyUnit(-3,-13,98,21,1), new GeneratedEnemyUnit(9,-12,6,5,3), new GeneratedEnemyUnit(-2,4,64,9,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d302e92c609597d20e7a43c4c58047f911064058b6dc1fd8e68fcab97fce8251");
        }

        private static void Case_00085()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 85,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,11,22,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-2,48,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,8,46,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,16,89,3,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "07906ae1fb32e8b3fbba231ab19059f97dd12cba6f984384db462d83a7c09fc5");
        }

        private static void Case_00086()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 86,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-18,77,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-16,82,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-7,27,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-17,76,2,2), new GeneratedEnemyUnit(5,1,38,24,2), new GeneratedEnemyUnit(-6,0,85,46,4), new GeneratedEnemyUnit(18,3,96,17,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "587051ffa284377be375fced6c19754686084fbe43d327d96ccd5735700423de");
        }

        private static void Case_00087()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 87,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-18,5,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,17,28,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,10,30,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,4,64,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,0,92,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,13,91,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-18,33,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-9,28,1,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "a84e1ef00ad5d344f8f7f9e1c0e393f6703751d38279b80d33c0421c1e6d9a28");
        }

        private static void Case_00088()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 88,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-2,37,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,6,98,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,0,15,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,16,45,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,19,61,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,7,84,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,17,50,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,16,61,41,1), new GeneratedEnemyUnit(0,14,5,11,3), new GeneratedEnemyUnit(-7,19,37,19,2), new GeneratedEnemyUnit(10,9,83,50,1), new GeneratedEnemyUnit(-12,-9,48,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "2afd83363d30be9c96732e36335b0c061e97caf29a259d6818b42814088def57");
        }

        private static void Case_00089()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 89,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,8,28,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-5,48,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,2,68,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-19,31,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-18,51,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,13,45,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,3,9,43,2), new GeneratedEnemyUnit(17,9,26,5,3), new GeneratedEnemyUnit(-4,6,33,11,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "60abac7c0e3cf9f4f9664ad43619a10b83d23ccdc5b3d1e801242d8864b2a4e6");
        }

        private static void Case_00090()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 90,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,18,98,6,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "79f21e51d463c55ae392b7336825af278208cfceb99b1dcffafdccc10f40afc1");
        }

        private static void Case_00091()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 91,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,18,49,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-9,5,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,6,71,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-1,61,49,1), new GeneratedEnemyUnit(1,10,88,20,2), new GeneratedEnemyUnit(0,8,40,6,2), new GeneratedEnemyUnit(2,-13,30,42,1), new GeneratedEnemyUnit(9,2,19,34,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "e493e2ee3d7204d71f9c47637f63d2cb3091ae5c9a38a5a0550372b8240a5ee7");
        }

        private static void Case_00092()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 92,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,7,74,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,0,57,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,20,53,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,7,14,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,12,51,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,11,75,9,1), new GeneratedEnemyUnit(-19,-20,74,37,3), new GeneratedEnemyUnit(-6,-3,21,41,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "adc86c981e6c035687c2ca8399dcaef094f7d51cc15beca8f8aeb17b1b345660");
        }

        private static void Case_00093()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 93,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,16,11,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-7,88,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,5,15,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,1,57,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,20,51,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,8,75,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,16,66,2,4), new GeneratedEnemyUnit(-4,12,72,46,2), new GeneratedEnemyUnit(8,1,75,49,1), new GeneratedEnemyUnit(18,-2,69,32,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "9007d58b7814c899c1a81aad9a697276ff9160a69af93d954a87f23b8c39309b");
        }

        private static void Case_00094()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 94,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-14,14,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-20,66,38,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "c05518437cf0076973c07b4cc7e6ca049ec4ef33b52db37480e75f2753e732c3");
        }

        private static void Case_00095()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 95,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-10,24,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-10,18,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-19,72,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,17,15,40,2), new GeneratedEnemyUnit(5,-1,20,5,1), new GeneratedEnemyUnit(-7,-2,33,5,1), new GeneratedEnemyUnit(-15,-2,15,2,2), new GeneratedEnemyUnit(19,-15,88,3,3), new GeneratedEnemyUnit(13,19,6,17,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b700d20b2fe415db07a9ffb09e470074788024a7fb943e1a30014ec056c595ae");
        }

        private static void Case_00096()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 96,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-7,77,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,1,53,13,3), new GeneratedEnemyUnit(10,-16,16,6,2), new GeneratedEnemyUnit(4,16,42,6,2), new GeneratedEnemyUnit(1,0,75,19,3), new GeneratedEnemyUnit(-20,-16,34,49,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "324dae14f067ab4ff5aa69acee98cce97cc3ae34332e96ff1823cf3ae5d0dfba");
        }

        private static void Case_00097()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 97,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-19,50,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,4,64,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-2,5,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,5,79,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-10,86,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,3,48,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-2,28,15,3), new GeneratedEnemyUnit(-5,3,83,44,4), new GeneratedEnemyUnit(5,16,52,32,2), new GeneratedEnemyUnit(0,12,38,9,3), new GeneratedEnemyUnit(20,16,36,10,4), new GeneratedEnemyUnit(15,-10,40,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "9cc1f90f30dccae1a1f3f2bf2c58216be00b0cbd27d9c79178fcdf73f6590ba0");
        }

        private static void Case_00098()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 98,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-7,77,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,3,10,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,6,46,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,11,76,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,14,95,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,9,25,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-2,67,34,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "886439bd2077aadfcafdcfb1a04080616e70e3253e9f8b8eb8f5327205f95eb5");
        }

        private static void Case_00099()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 99,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,18,94,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-7,94,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,5,29,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-13,8,24,3), new GeneratedEnemyUnit(6,-11,44,1,4), new GeneratedEnemyUnit(-3,-2,56,45,1), new GeneratedEnemyUnit(-9,15,40,40,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "e5bc420d8a08e21bf930898c5cc881232eb4523874e19b11354efea73da5a014");
        }

        private static void Case_00100()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 100,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-19,49,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-5,39,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,4,89,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,20,52,3,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "51053007f88609fc6b84854c5979fdb76bf7a79f56205f1243388ebc625113fb");
        }

        private static void Case_00101()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 101,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,5,68,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,12,29,2,4), new GeneratedEnemyUnit(-20,-12,22,19,1), new GeneratedEnemyUnit(-4,12,40,34,3), new GeneratedEnemyUnit(10,-19,6,45,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "c95a456cb5314a4efef77a8f1b308bac4e6552fb066c65c49aafee6b4686bc4b");
        }

        private static void Case_00102()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 102,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,11,6,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-18,30,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,18,58,6,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "7cba8f3f7b302737e21fab53500825641dff18cc92b1dd19d5401a5718109c72");
        }

        private static void Case_00103()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 103,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,19,95,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,1,29,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-11,91,38,1), new GeneratedEnemyUnit(13,15,13,2,1), new GeneratedEnemyUnit(-5,20,65,44,2), new GeneratedEnemyUnit(19,18,54,22,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "3745e0784ca97d6fee912b80fd55d4f8fbd9752a2746af24f80a7fe6b8a01724");
        }

        private static void Case_00104()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 104,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-3,79,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-16,12,20,2), new GeneratedEnemyUnit(-9,-10,46,23,4), new GeneratedEnemyUnit(5,6,27,2,1), new GeneratedEnemyUnit(-11,12,81,26,1), new GeneratedEnemyUnit(-11,-10,96,36,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "cbe7254e5e1bf94249fbe788040bb0e12e2f1d98c41095c85c018cc79c9c2138");
        }

        private static void Case_00105()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 105,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,18,89,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-13,22,47,1), new GeneratedEnemyUnit(15,-3,54,17,2), new GeneratedEnemyUnit(-11,20,87,25,3), new GeneratedEnemyUnit(-1,-20,35,10,4), new GeneratedEnemyUnit(1,-17,89,18,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "18949da36650fa3e7f4fbf60fd2a31a52ef5db41dd4db0b831d3aea50c8449a7");
        }

        private static void Case_00106()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 106,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,20,7,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-7,53,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-16,15,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,12,58,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-4,81,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,8,45,49,4), new GeneratedEnemyUnit(1,8,67,39,3), new GeneratedEnemyUnit(-14,-4,81,46,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "05a187d111af8da6dd486ec2c81043a42ecb864d7937817dd76c10e6d8a0cbe2");
        }

        private static void Case_00107()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 107,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,9,12,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,12,74,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,15,41,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,12,38,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,7,32,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,11,87,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-9,88,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-15,54,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,19,91,33,2), new GeneratedEnemyUnit(1,7,18,14,2), new GeneratedEnemyUnit(9,10,70,48,4), new GeneratedEnemyUnit(6,20,38,22,3), new GeneratedEnemyUnit(4,-14,65,31,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "22b867ec395d74496087eb201a3a45d5bfe852ec6cf2dbd211b9402b50e87bee");
        }

        private static void Case_00108()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 108,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,0,26,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,8,87,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-2,31,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,3,32,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-20,60,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,19,18,29,3), new GeneratedEnemyUnit(-20,-20,46,16,2), new GeneratedEnemyUnit(19,2,58,11,3), new GeneratedEnemyUnit(0,-13,53,24,2), new GeneratedEnemyUnit(3,-19,56,29,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "908ad685cff95c6079664b4490ea1812b92fce36f7528c963d463d5bf56cecba");
        }

        private static void Case_00109()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 109,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,16,84,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-13,47,5,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "353cb9097738ad5b300a19f0e41bbe0561a2ed0210fccd861f581db6285bce38");
        }

        private static void Case_00110()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 110,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-18,86,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,8,32,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,2,64,39,4), new GeneratedEnemyUnit(8,13,56,9,1), new GeneratedEnemyUnit(-5,-11,18,39,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "e01f321c448f949fc7a3780d05c8e4c5d5c9c207a82d53c50d8cdac6c2015d82");
        }

        private static void Case_00111()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 111,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,19,24,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,9,16,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,16,29,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,7,14,39,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "e6a542e82a6c8fb4fac106a09e9fe53dd4f4f00c5103dc7b3ae4155d4aa1aee8");
        }

        private static void Case_00112()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 112,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-1,9,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-8,93,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,10,31,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,7,20,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,12,92,47,1), new GeneratedEnemyUnit(-6,-2,97,28,2), new GeneratedEnemyUnit(9,-16,15,31,4), new GeneratedEnemyUnit(-5,19,69,43,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "4bd4a0c75957dd794be991ff0c3a6b1c6e231caeb19d18235bee9f25c712d0f2");
        }

        private static void Case_00113()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 113,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-7,61,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,9,5,17,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "1b21416f95492fc8424b1031bb0a41c730e539cdd712d303b9cf8b0e00dbcfaf");
        }

        private static void Case_00114()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 114,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,14,6,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,17,25,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-6,36,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,13,18,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,17,29,19,4), new GeneratedEnemyUnit(18,0,21,47,2), new GeneratedEnemyUnit(-6,11,25,30,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7efdb5d5d8a9d91f9dd3e8027e519d0fcf751d9571f75f5be5bf141f86e1f011");
        }

        private static void Case_00115()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 115,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-17,97,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-8,68,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-4,66,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-20,56,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,11,63,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,17,95,10,4), new GeneratedEnemyUnit(-12,11,73,6,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "818cdabee09592c2c45c06eb0d35c15bebcebb0a5b357965dae39be2e4f3ebae");
        }

        private static void Case_00116()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 116,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,3,73,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-3,21,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,19,68,7,1), new GeneratedEnemyUnit(-1,19,66,37,3), new GeneratedEnemyUnit(9,13,68,39,1), new GeneratedEnemyUnit(-10,-9,36,33,4), new GeneratedEnemyUnit(-16,-11,49,42,3), new GeneratedEnemyUnit(-4,5,33,18,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "1ab12d95c164169c3d78cdb7d0a25023f0563461d2f55bd96cd767609c5f2e5e");
        }

        private static void Case_00117()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 117,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-18,86,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,10,51,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,7,71,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-11,98,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-17,97,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,17,78,4,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "bc658242bb041e77f0b4549a583517cf77d6931df09007bee8614bee164534df");
        }

        private static void Case_00118()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 118,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-5,94,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,10,89,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,1,67,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,9,30,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,5,67,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,15,21,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-20,70,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,20,64,28,4), new GeneratedEnemyUnit(6,5,29,8,1), new GeneratedEnemyUnit(13,7,24,8,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "0918d954f6d8d84ef1e8b636e926ff642a5701f8a9bfb9033d449b42ada66d3b");
        }

        private static void Case_00119()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 119,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,15,82,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,20,67,41,2), new GeneratedEnemyUnit(19,20,10,2,4), new GeneratedEnemyUnit(-12,-20,66,37,2), new GeneratedEnemyUnit(-20,18,27,3,1), new GeneratedEnemyUnit(17,5,36,21,2), new GeneratedEnemyUnit(-3,-16,17,21,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "73964afaf960719ca6e6eaf84f94357970d975f000c6ca750398c06c6a420101");
        }

        private static void Case_00120()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 120,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-3,68,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,14,16,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,20,68,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,3,36,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,12,45,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-12,9,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-7,96,14,4), new GeneratedEnemyUnit(-8,-4,41,26,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "6831eecd703001774d9b2050b897d876dafed367c181ee051859110cecb35522");
        }

        private static void Case_00121()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 121,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,1,58,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,8,50,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,0,55,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-8,46,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,3,51,3,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "70fac7e594357d4434d94a9160d25ac1b52404196940a1b73127d560c3a32e32");
        }

        private static void Case_00122()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 122,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-14,10,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,12,35,27,3), new GeneratedEnemyUnit(-3,4,18,3,3), new GeneratedEnemyUnit(20,-1,35,29,2), new GeneratedEnemyUnit(-9,10,98,22,4), new GeneratedEnemyUnit(5,16,8,48,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "c02d6e065492218619d0ac05116f011fd5ec37e3bb328c057296c2466cddfd71");
        }

        private static void Case_00123()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 123,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-5,83,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,10,100,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-6,30,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,2,66,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,4,29,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-12,94,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-18,73,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,0,35,48,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "d87d0fa77069e24c36ff1208edb79bfdd8c34a460e3fe74aea1ec9fa19dedf63");
        }

        private static void Case_00124()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 124,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,20,16,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-18,92,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,13,14,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,18,39,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-20,90,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,17,71,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,9,87,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,10,58,30,3), new GeneratedEnemyUnit(5,13,82,46,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "22c67799eb452cd28a1f2c8229c77954ce8c572b212f14d075bec90065ee7239");
        }

        private static void Case_00125()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 125,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,20,30,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,4,98,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-9,64,1,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "8a0e8991898676ef236da0610ad7f3bb41fcfc0def4bda34d50a3d824631853c");
        }

        private static void Case_00126()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 126,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-20,92,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-20,62,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-15,27,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-8,84,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,11,86,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,9,30,34,2), new GeneratedEnemyUnit(-2,10,41,19,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "70cce8bc96fae441dc05072463564bfa816faa762a577ea3a217ae0e3ca40024");
        }

        private static void Case_00127()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 127,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,4,20,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,12,66,29,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "d5cef998c5b4c0676daa9c6909a3b5dc924d12575f30eb7c9d16c4daf3bcf70e");
        }

        private static void Case_00128()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 128,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,3,61,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,4,90,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,6,93,15,4), new GeneratedEnemyUnit(1,-5,36,41,4), new GeneratedEnemyUnit(-16,18,68,23,4), new GeneratedEnemyUnit(18,6,51,16,4), new GeneratedEnemyUnit(-6,-17,37,3,2), new GeneratedEnemyUnit(-18,5,49,12,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "d83b2dcae8445ae4c2c10b0708d64dbbdba43d042e0c68e8b2cda36464ab9cb6");
        }

        private static void Case_00129()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 129,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,12,87,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,1,7,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-9,50,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-14,84,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,17,54,5,1), new GeneratedEnemyUnit(-9,-20,23,32,2), new GeneratedEnemyUnit(9,1,56,8,4), new GeneratedEnemyUnit(-11,-16,8,8,3), new GeneratedEnemyUnit(10,-10,18,8,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "a43807d5f952e9c3cfdda26dc7de7ab09e1a70db601d46f9f44e55cf078fbd32");
        }

        private static void Case_00130()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 130,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-18,81,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,13,65,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,11,41,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-12,75,3,4), new GeneratedEnemyUnit(14,-14,42,17,3), new GeneratedEnemyUnit(-2,12,75,19,2), new GeneratedEnemyUnit(-13,-15,96,19,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "a6919dac220dd1aaaf8979b1c3206f21ad67caa53b94e2e323538f7d2be6a1d2");
        }

        private static void Case_00131()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 131,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-2,55,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-2,97,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-11,98,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,11,12,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,19,56,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,4,16,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-14,83,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,6,17,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-18,77,42,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "49d13dbb177442df43a1a58b62266ab9452bb828e943b6d9e7a1a3734dc51e2f");
        }

        private static void Case_00132()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 132,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-9,21,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-6,34,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,6,21,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-11,59,25,3), new GeneratedEnemyUnit(-6,-15,95,20,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "6dab6e74b7f68c54d97571a4153721c7cf56f7ca4550d494cc51293a977a0b3d");
        }

        private static void Case_00133()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 133,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,1,60,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,0,19,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,1,48,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-2,43,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-19,80,29,2), new GeneratedEnemyUnit(3,15,82,14,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "5dc32f63677614c946148780cf25ea3b221d789676c0560eecb3ebe7f9fc035d");
        }

        private static void Case_00134()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 134,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-9,96,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,2,75,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,12,7,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-16,69,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-5,83,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,8,51,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,16,84,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,8,39,4,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "2b8a85ae83199137495f84d92180a8f0e0765a1baabe6e10c93b8351c8031429");
        }

        private static void Case_00135()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 135,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-9,41,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-2,65,39,4), new GeneratedEnemyUnit(-8,-13,28,33,3), new GeneratedEnemyUnit(5,-11,82,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "7fd08c54ef6e430122bf424ddffd249eddf6ee457325a779e5a20906840a88f8");
        }

        private static void Case_00136()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 136,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-4,44,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-8,22,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-17,57,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-12,62,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-10,44,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-7,80,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,8,94,22,3), new GeneratedEnemyUnit(-10,19,80,49,3), new GeneratedEnemyUnit(19,5,47,14,3), new GeneratedEnemyUnit(-20,6,35,20,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "827b37c86a3b3ef5157f94e725250383bc5afb44f45b482b93fa657034a6a199");
        }

        private static void Case_00137()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 137,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-5,54,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-6,48,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-10,80,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,11,25,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,7,76,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-10,46,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,19,44,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-16,59,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,16,97,39,3), new GeneratedEnemyUnit(-1,12,33,7,1), new GeneratedEnemyUnit(17,18,7,26,4), new GeneratedEnemyUnit(18,17,73,3,2), new GeneratedEnemyUnit(-10,-3,53,14,4), new GeneratedEnemyUnit(-4,16,80,25,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "a07edc82c260b98e7f11fef642650551f9f1d70bdfcb63abce047f2231715acd");
        }

        private static void Case_00138()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 138,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-4,82,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-4,23,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,4,86,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-9,30,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,8,61,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-11,25,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,10,92,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-5,37,17,3), new GeneratedEnemyUnit(14,2,82,6,3), new GeneratedEnemyUnit(-13,-3,87,40,2), new GeneratedEnemyUnit(19,-4,97,4,1), new GeneratedEnemyUnit(-4,7,56,39,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "2805e2b81eecdb4da9c14644bf05da30bcc29fb9d402ea68b92d33b32ddf4b99");
        }

        private static void Case_00139()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 139,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-2,76,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-11,76,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-6,43,3,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "b98ec50d7159881653bc2329b86b556c6d4e050628a5bc7d24b793dc4176bf36");
        }

        private static void Case_00140()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 140,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-1,86,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,12,80,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,0,15,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,3,66,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-11,36,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-18,53,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,1,13,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-10,80,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,20,52,16,3), new GeneratedEnemyUnit(-12,-13,37,6,2), new GeneratedEnemyUnit(-6,-8,91,11,1), new GeneratedEnemyUnit(11,9,98,31,4), new GeneratedEnemyUnit(-6,5,46,33,3), new GeneratedEnemyUnit(-7,-10,43,9,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "0efd6990504fac3ea61b931a9a2e550a4ca9bc85b37f2447ebb191996842765a");
        }

        private static void Case_00141()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 141,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,13,37,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-14,35,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-13,8,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-3,19,24,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "5b860f687c0caccd14dc49652b395dae535dc4bd582f66da832f09d115a848cd");
        }

        private static void Case_00142()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 142,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,9,14,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,11,88,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-7,70,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,4,28,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,20,17,24,4), new GeneratedEnemyUnit(-19,10,65,7,1), new GeneratedEnemyUnit(-19,9,68,43,4), new GeneratedEnemyUnit(-3,2,58,45,3), new GeneratedEnemyUnit(-6,-15,29,14,4), new GeneratedEnemyUnit(-19,3,100,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "6ba370fe163c8aea1a416232dd5faea243409300f7c03a9ae0251ed613b49a5a");
        }

        private static void Case_00143()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 143,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-17,56,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,17,70,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,2,36,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,6,92,46,3), new GeneratedEnemyUnit(6,-4,44,49,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "66356a12896100ce25646a84bf28578b0efa054f705bbc0156331f438d4d64d9");
        }

        private static void Case_00144()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 144,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,0,6,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,12,96,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,5,28,23,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "5890d5868599147da3cb157cf75fa75f9e0c2eb56c59bdd54bce70be0751f10f");
        }

        private static void Case_00145()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 145,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,9,27,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,20,19,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-7,49,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-7,56,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,20,88,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,14,9,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-5,54,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-14,47,35,2), new GeneratedEnemyUnit(6,7,41,23,3), new GeneratedEnemyUnit(-10,19,89,15,2), new GeneratedEnemyUnit(-17,-12,76,16,4), new GeneratedEnemyUnit(6,13,99,13,3), new GeneratedEnemyUnit(-10,-20,53,41,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "d1400b19e755e633f10d16dbd8c7ebcd00325183bafa8e0a055bd18d3263c452");
        }

        private static void Case_00146()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 146,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-8,45,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,4,61,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-1,91,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,16,66,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-2,42,49,3), new GeneratedEnemyUnit(18,16,22,25,3), new GeneratedEnemyUnit(19,-16,35,8,3), new GeneratedEnemyUnit(-1,6,59,24,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "467650d7c301ae54bc99f4f9369dabff6b21c65e3a2b74c2d4d38f538fd29b2d");
        }

        private static void Case_00147()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 147,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-3,20,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,7,6,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-20,85,6,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "4cfb1d5191f8253e0fe8846e33ad0fed55f286542265983e3e6e65fbdd1e31ef");
        }

        private static void Case_00148()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 148,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-6,75,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,14,11,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,11,100,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,17,59,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-12,55,26,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "866d3e78345785fe927eae3bb1b3e880c5fce9743208d46a5068ac80478f4eb7");
        }

        private static void Case_00149()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 149,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-7,89,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-12,19,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,11,71,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-4,50,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,20,22,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "b78e5a03a0854615a88527f04219c9c58cc06ac2b28738fc228a5d09200b9fd3");
        }

        private static void Case_00150()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 150,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,14,31,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-9,45,34,3), new GeneratedEnemyUnit(1,-1,46,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "faab8f9fe5c494e24f6f13fab6a28b31d3a8987b41ce63398661377335676fa6");
        }

        private static void Case_00151()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 151,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,7,24,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-4,58,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-17,5,20,2), new GeneratedEnemyUnit(14,-12,24,17,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "c8f9e6b459c30de2abc16f559b222f8c47bd0b4adfce99b66625ee6b15a9ea8d");
        }

        private static void Case_00152()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 152,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-8,28,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,0,57,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-4,21,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-10,5,6,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "8359cded37a069e308e63fa2b72c6b6538c01df32d4d7790cb91814827abf2c1");
        }

        private static void Case_00153()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 153,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-18,95,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-17,92,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,18,79,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,18,87,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-12,29,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,19,67,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,1,56,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-16,54,36,1), new GeneratedEnemyUnit(-6,19,10,9,1), new GeneratedEnemyUnit(-7,12,85,44,2), new GeneratedEnemyUnit(-19,-8,75,30,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "9ae1d7eaf22b5661e613b12cb731cc62f31dcfb0f35ec88e1bd70bfaed77ee7c");
        }

        private static void Case_00154()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 154,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,2,64,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,16,85,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,13,26,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-2,48,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,0,83,49,4), new GeneratedEnemyUnit(6,-2,33,20,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "4ad5726e50ec51fdaf6224eb0e5c470724202b6ccf7fd250b131de054863c156");
        }

        private static void Case_00155()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 155,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-9,74,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,20,52,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-14,18,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-4,74,13,3), new GeneratedEnemyUnit(20,-1,80,44,2), new GeneratedEnemyUnit(-8,-17,10,50,1), new GeneratedEnemyUnit(-5,-12,28,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "8fd1f5e292dfd4dc3d68f70458211f903d6e5933945246e4ac4b5ee20a9257d5");
        }

        private static void Case_00156()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 156,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-5,36,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,16,28,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,8,24,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-12,56,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-7,22,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,18,99,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,8,51,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,7,11,28,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "cecd0748a74c845a6e5cc7b10cfd1a096d327ec51d7ca6fbef50d2bc8ce4b48e");
        }

        private static void Case_00157()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 157,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,8,31,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,6,43,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,3,38,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-3,13,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,5,38,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,16,15,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-6,67,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,2,39,30,4), new GeneratedEnemyUnit(5,19,31,18,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "bc10121461064a5b154eff2747a7383d51ec6c2d5750897dfdca8bd094f1b163");
        }

        private static void Case_00158()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 158,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-11,49,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,5,91,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,7,97,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,5,59,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-7,45,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,19,38,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-9,27,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-12,77,7,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "a96907c11f088878ba5a70d55dcf4105be57ed9278b12f344de0cb5b010d95b2");
        }

        private static void Case_00159()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 159,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,8,33,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-18,23,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,15,15,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-10,42,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-5,41,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-8,6,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,8,43,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,5,42,22,3), new GeneratedEnemyUnit(-5,-10,74,37,2), new GeneratedEnemyUnit(19,-14,55,14,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "71de2187572b0a50a68b892db7e65f833f872cb2f8552826592c39fc2fa7c4e2");
        }

        private static void Case_00160()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 160,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-4,15,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,10,31,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,9,10,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,5,95,12,2), new GeneratedEnemyUnit(-18,13,23,13,3), new GeneratedEnemyUnit(-16,-3,65,38,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "1e3494f2fbf7e3243e4b478ab00f8cff277eabb404dd4d7a1101f313821203b0");
        }

        private static void Case_00161()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 161,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,7,9,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,20,35,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-14,49,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,20,62,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-15,9,8,1), new GeneratedEnemyUnit(9,14,36,25,4), new GeneratedEnemyUnit(19,-19,56,46,2), new GeneratedEnemyUnit(10,-6,9,46,2), new GeneratedEnemyUnit(-19,0,91,44,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "9121a3f70c0c8e09395254ce506465eda02ddb4792ff277650a88f996748ee75");
        }

        private static void Case_00162()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 162,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-13,79,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-4,41,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,3,53,25,3), new GeneratedEnemyUnit(-11,11,21,3,1), new GeneratedEnemyUnit(19,12,94,8,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "1f9b69793fa63e60e0a56dcf9f4860cd157df86720e7f6c219e6ff87a57b752d");
        }

        private static void Case_00163()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 163,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-2,16,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,7,79,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-3,56,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,7,16,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,1,54,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,6,48,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,7,92,4,2), new GeneratedEnemyUnit(10,11,33,6,2), new GeneratedEnemyUnit(-15,-6,65,15,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "9ccac7f505d6ca26d802ba8c157d3ee20c9d5f50ee3aee1b0ce099a38a2b636c");
        }

        private static void Case_00164()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 164,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-13,45,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-8,85,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,0,45,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-10,38,10,1), new GeneratedEnemyUnit(-10,-4,92,23,2), new GeneratedEnemyUnit(-6,-1,77,48,2), new GeneratedEnemyUnit(18,-1,65,15,3), new GeneratedEnemyUnit(-20,-12,91,13,2), new GeneratedEnemyUnit(-5,-5,43,48,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "ac3e1f927a2a61b73e852bc65eed8f9fc1efed36e87e933781a43234b82f83e4");
        }

        private static void Case_00165()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 165,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,4,20,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-9,50,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,11,98,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,7,8,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-1,17,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-7,10,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-16,60,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,0,89,36,3), new GeneratedEnemyUnit(2,10,12,6,1), new GeneratedEnemyUnit(19,-18,83,28,2), new GeneratedEnemyUnit(-5,18,13,27,2), new GeneratedEnemyUnit(-7,7,88,43,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "04c917221590044dfa89afb91230ef7e798d0c179d5ee8c8214e4bc98e218d23");
        }

        private static void Case_00166()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 166,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,17,25,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-7,23,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-6,47,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,1,28,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,2,37,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,18,40,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,11,88,7,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "e7038d4fa4938ddb66e62746244bea2c4ec1704356686fefac655c69217f83ed");
        }

        private static void Case_00167()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 167,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-12,79,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,12,30,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-1,15,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-20,96,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-18,31,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-6,94,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,14,47,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-17,78,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,17,6,10,1), new GeneratedEnemyUnit(15,-11,84,38,4), new GeneratedEnemyUnit(-19,-10,77,39,3), new GeneratedEnemyUnit(-5,4,5,7,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "a9664bd8c253905ab3cbf3c31a26a15f600db564d17cc519061bc4510f5c3e7e");
        }

        private static void Case_00168()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 168,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-19,67,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,11,56,32,1), new GeneratedEnemyUnit(9,-4,78,39,4), new GeneratedEnemyUnit(-19,18,99,14,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "bb09e9d77e34e007df9299af2444a08a2e4f996ac02101a29eceaf29744b7532");
        }

        private static void Case_00169()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 169,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,15,14,4,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "82093b504d5de38afd6a56c7345bc90a6ab303e103635afb9204f4c89166b972");
        }

        private static void Case_00170()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 170,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,19,41,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,6,33,40,2), new GeneratedEnemyUnit(-10,-16,92,12,3), new GeneratedEnemyUnit(-15,11,56,4,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "4d3e27b0631b10c0b91a977c31ba193a6d1e133dd562c1d47699d88762dd273d");
        }

        private static void Case_00171()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 171,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-16,52,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,17,74,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-18,7,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,20,99,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-7,86,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-19,73,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-16,16,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,20,13,45,1), new GeneratedEnemyUnit(-11,-1,48,6,1), new GeneratedEnemyUnit(13,-2,93,26,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "7bde621d66205dce8813571bc0fe8f8c5a5ffb53c07ca6bbf4f77215f59dc978");
        }

        private static void Case_00172()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 172,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-13,100,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-3,46,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-15,42,48,1), new GeneratedEnemyUnit(-8,9,84,28,3), new GeneratedEnemyUnit(-9,-5,42,11,4), new GeneratedEnemyUnit(19,5,27,19,4), new GeneratedEnemyUnit(3,-6,78,41,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1db95088ccf637af183498a1d30eee9b90ca760a45f2e288f27c0652087fa0d3");
        }

        private static void Case_00173()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 173,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-16,24,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,17,52,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-17,57,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-3,32,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-6,35,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-3,18,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-1,96,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,13,12,35,1), new GeneratedEnemyUnit(0,13,35,50,2), new GeneratedEnemyUnit(1,-1,10,12,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "4a690787ab3abe3309670c11fe4d617735ee0d6ae49b02ead2d7459bc94425bc");
        }

        private static void Case_00174()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 174,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,1,65,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-9,25,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-4,83,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-14,38,3,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "35a86dbe6c01efd615f78a8d0944306b5896a1522e2ce4afbe51791d7b8db5ff");
        }

        private static void Case_00175()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 175,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-12,63,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,5,60,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-17,35,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,5,58,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-16,70,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,4,16,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-18,87,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,3,37,3,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "c692890e39544af98d0d28e16c8445eafc056777357641cb9a9e59a3f3e3093d");
        }

        private static void Case_00176()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 176,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,11,92,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,5,11,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-15,88,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-2,85,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,4,49,18,3), new GeneratedEnemyUnit(-15,9,90,47,3), new GeneratedEnemyUnit(-8,-20,63,21,2), new GeneratedEnemyUnit(10,15,88,12,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "a6afdbb5da532d09f562c43e1f7614743ecffa0fb69c4234dc6950009d55ba87");
        }

        private static void Case_00177()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 177,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-12,45,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,7,17,40,1), new GeneratedEnemyUnit(-20,-12,20,4,1), new GeneratedEnemyUnit(9,6,58,6,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "03b877ef24084e2479a3d7144c8e9e96327d91b219f7f196856e2d94ffb86860");
        }

        private static void Case_00178()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 178,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-9,61,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-2,35,2,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "8405c1739fbd932ea00ed82a033578ed4b5eded6d3855e69c926340d1a768628");
        }

        private static void Case_00179()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 179,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,13,74,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-6,45,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,8,5,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-7,66,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,15,56,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,8,68,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,0,20,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,16,56,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,19,93,46,4), new GeneratedEnemyUnit(-7,-18,53,33,3), new GeneratedEnemyUnit(19,0,54,18,1), new GeneratedEnemyUnit(4,-2,39,17,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "0189eaec5547833897574f2bccf59436c919a7c8a28dc0bba90f9b80c34c1118");
        }

    }
}
