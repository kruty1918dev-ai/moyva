using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard004
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_00720();
            Case_00721();
            Case_00722();
            Case_00723();
            Case_00724();
            Case_00725();
            Case_00726();
            Case_00727();
            Case_00728();
            Case_00729();
            Case_00730();
            Case_00731();
            Case_00732();
            Case_00733();
            Case_00734();
            Case_00735();
            Case_00736();
            Case_00737();
            Case_00738();
            Case_00739();
            Case_00740();
            Case_00741();
            Case_00742();
            Case_00743();
            Case_00744();
            Case_00745();
            Case_00746();
            Case_00747();
            Case_00748();
            Case_00749();
            Case_00750();
            Case_00751();
            Case_00752();
            Case_00753();
            Case_00754();
            Case_00755();
            Case_00756();
            Case_00757();
            Case_00758();
            Case_00759();
            Case_00760();
            Case_00761();
            Case_00762();
            Case_00763();
            Case_00764();
            Case_00765();
            Case_00766();
            Case_00767();
            Case_00768();
            Case_00769();
            Case_00770();
            Case_00771();
            Case_00772();
            Case_00773();
            Case_00774();
            Case_00775();
            Case_00776();
            Case_00777();
            Case_00778();
            Case_00779();
            Case_00780();
            Case_00781();
            Case_00782();
            Case_00783();
            Case_00784();
            Case_00785();
            Case_00786();
            Case_00787();
            Case_00788();
            Case_00789();
            Case_00790();
            Case_00791();
            Case_00792();
            Case_00793();
            Case_00794();
            Case_00795();
            Case_00796();
            Case_00797();
            Case_00798();
            Case_00799();
            Case_00800();
            Case_00801();
            Case_00802();
            Case_00803();
            Case_00804();
            Case_00805();
            Case_00806();
            Case_00807();
            Case_00808();
            Case_00809();
            Case_00810();
            Case_00811();
            Case_00812();
            Case_00813();
            Case_00814();
            Case_00815();
            Case_00816();
            Case_00817();
            Case_00818();
            Case_00819();
            Case_00820();
            Case_00821();
            Case_00822();
            Case_00823();
            Case_00824();
            Case_00825();
            Case_00826();
            Case_00827();
            Case_00828();
            Case_00829();
            Case_00830();
            Case_00831();
            Case_00832();
            Case_00833();
            Case_00834();
            Case_00835();
            Case_00836();
            Case_00837();
            Case_00838();
            Case_00839();
            Case_00840();
            Case_00841();
            Case_00842();
            Case_00843();
            Case_00844();
            Case_00845();
            Case_00846();
            Case_00847();
            Case_00848();
            Case_00849();
            Case_00850();
            Case_00851();
            Case_00852();
            Case_00853();
            Case_00854();
            Case_00855();
            Case_00856();
            Case_00857();
            Case_00858();
            Case_00859();
            Case_00860();
            Case_00861();
            Case_00862();
            Case_00863();
            Case_00864();
            Case_00865();
            Case_00866();
            Case_00867();
            Case_00868();
            Case_00869();
            Case_00870();
            Case_00871();
            Case_00872();
            Case_00873();
            Case_00874();
            Case_00875();
            Case_00876();
            Case_00877();
            Case_00878();
            Case_00879();
            Case_00880();
            Case_00881();
            Case_00882();
            Case_00883();
            Case_00884();
            Case_00885();
            Case_00886();
            Case_00887();
            Case_00888();
            Case_00889();
            Case_00890();
            Case_00891();
            Case_00892();
            Case_00893();
            Case_00894();
            Case_00895();
            Case_00896();
            Case_00897();
            Case_00898();
            Case_00899();
        }

        private static void Case_00720()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 720,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-6,11,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-11,58,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-13,86,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,12,26,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-15,46,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,8,98,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,11,75,1,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "f887c2a800ffd61310cf71ac2466c02ad5fdb5266f08a4ef64809d005678b7d2");
        }

        private static void Case_00721()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 721,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,9,17,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,3,55,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,10,36,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-17,49,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,11,57,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,15,50,29,2), new GeneratedEnemyUnit(-13,-15,36,19,3), new GeneratedEnemyUnit(20,17,95,28,2), new GeneratedEnemyUnit(-10,17,62,9,4), new GeneratedEnemyUnit(3,-3,34,8,4), new GeneratedEnemyUnit(11,-20,53,31,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "550e5a251586b3fdb3a302e42701f3eb6bb5b55d671e3a615b90170f45bbbd86");
        }

        private static void Case_00722()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 722,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,19,51,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,7,84,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-17,87,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,17,5,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-2,66,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,17,92,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,13,5,43,4), new GeneratedEnemyUnit(5,-11,89,24,4), new GeneratedEnemyUnit(-18,-14,45,5,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "fe72c7704ae474e70ee9cc906c1555b05a00681cfca186f35470be81f190e943");
        }

        private static void Case_00723()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 723,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,18,62,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,3,25,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,2,46,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,19,13,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-12,84,45,3), new GeneratedEnemyUnit(-5,-11,29,13,3), new GeneratedEnemyUnit(-9,-12,12,18,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "a78df384797557ac2356146dbb56f2eb325166d2fad4184e929d22a1509b7329");
        }

        private static void Case_00724()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 724,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,18,98,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-18,10,4,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "d51312509a0348e6b303610ebe425354df82573833fd8415f443092a7cbc9504");
        }

        private static void Case_00725()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 725,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-16,40,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-5,14,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-15,84,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-1,45,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-17,38,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,2,23,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,17,34,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,17,95,28,1), new GeneratedEnemyUnit(-4,-18,65,29,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7cf1d0c0af1b495c079d41e808456745fd83e4a5b012a907ee3d65fb574a4cd8");
        }

        private static void Case_00726()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 726,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,2,85,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-4,72,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-6,14,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-15,83,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,20,65,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-13,59,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,5,80,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-19,62,7,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3572f02db405c513e0ee91a7e415a41768743e83128fe31842ce864ab3f2f29e");
        }

        private static void Case_00727()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 727,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-6,30,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-7,22,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-19,45,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,6,72,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-20,87,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-18,80,4,1), new GeneratedEnemyUnit(-6,7,100,39,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "33c6f507afa9e38c997982a98c08f040ffb5fac1fad2a4b840b4d32f8eecbe5a");
        }

        private static void Case_00728()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 728,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-8,60,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,10,58,14,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "2f2fc097d960a64946a4a5e59b1e04b745143d2a1d9c125be6decd3cf28ab728");
        }

        private static void Case_00729()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 729,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,1,90,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-2,95,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,0,70,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,10,85,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-8,72,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,8,95,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,8,54,22,1), new GeneratedEnemyUnit(18,-13,30,21,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "208d419fe2cdb7f570bc81b41560fb46d727c53f994daba82e74262aeee40349");
        }

        private static void Case_00730()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 730,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,19,52,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,5,95,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-7,48,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-19,35,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-10,41,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,13,93,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-3,15,18,2), new GeneratedEnemyUnit(20,19,58,25,4), new GeneratedEnemyUnit(7,15,85,19,2), new GeneratedEnemyUnit(-11,5,9,41,3), new GeneratedEnemyUnit(20,11,58,21,1), new GeneratedEnemyUnit(19,1,21,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "7173d540455a1faad7f16f62b323d1fda40a7e6fca403ce9189c37899fd0c1a9");
        }

        private static void Case_00731()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 731,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,18,18,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-3,52,15,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "ff2e93922d2af687589ced92781b536c8f268d77e30c6fe728a3d3821b6e3355");
        }

        private static void Case_00732()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 732,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-16,20,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,17,82,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-1,74,16,4), new GeneratedEnemyUnit(15,-2,17,41,2), new GeneratedEnemyUnit(13,18,19,23,3), new GeneratedEnemyUnit(-7,2,71,42,4), new GeneratedEnemyUnit(-12,-14,29,28,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "5d05edec9e9cfa67f98d7f1bf5943724f7f2f1588f4d57a566682a25154bd804");
        }

        private static void Case_00733()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 733,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,7,22,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,10,74,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-11,69,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-19,29,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,13,96,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-1,85,29,3), new GeneratedEnemyUnit(-14,17,78,8,4), new GeneratedEnemyUnit(-17,8,18,26,2), new GeneratedEnemyUnit(5,9,59,6,3), new GeneratedEnemyUnit(-7,8,80,31,1), new GeneratedEnemyUnit(17,17,100,40,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "b8edaa0cc00dbbfc1cd87cd53e53d5b26724178da072474c2db83502a203667c");
        }

        private static void Case_00734()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 734,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,20,8,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-14,32,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,18,27,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-7,28,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-1,49,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,9,33,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,5,12,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-13,51,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-13,90,29,3), new GeneratedEnemyUnit(-16,-4,76,38,2), new GeneratedEnemyUnit(-2,18,56,45,1), new GeneratedEnemyUnit(-4,20,68,19,4), new GeneratedEnemyUnit(-19,11,59,49,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "c6bfc03f7c9858aaf77c255bf5a414016e2c1f8b515c0f6d89c54053e770921d");
        }

        private static void Case_00735()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 735,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-7,73,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,6,30,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,12,59,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,16,32,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-17,31,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-5,44,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-10,13,20,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "61f6e5314f68c921add7fe92415088fb4bbb0b64743f1b43beb859abf6dba8a2");
        }

        private static void Case_00736()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 736,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-19,95,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,13,32,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-10,64,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,1,97,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,17,80,8,2), new GeneratedEnemyUnit(-18,-2,51,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "0b8cf15d194231f3efbe496ccfaebf7e119ee16d959a9d46caabff924827b8bd");
        }

        private static void Case_00737()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 737,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,18,25,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-6,83,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-18,79,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,0,57,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-19,86,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-8,72,47,4), new GeneratedEnemyUnit(-6,14,93,23,1), new GeneratedEnemyUnit(-17,5,42,39,1), new GeneratedEnemyUnit(6,3,42,18,3), new GeneratedEnemyUnit(13,-13,51,36,3), new GeneratedEnemyUnit(11,15,63,17,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "6af1d202b9caa51432ed15ed932b49e305a5255f65df66e57d672618d2faf285");
        }

        private static void Case_00738()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 738,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-12,24,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-7,73,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-19,54,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-12,60,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,4,35,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,4,100,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,15,24,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,6,40,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-11,79,27,1), new GeneratedEnemyUnit(-12,14,23,8,4), new GeneratedEnemyUnit(18,10,77,25,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "eaeaa21de90feabbd6bab387028a0eba87fbc39a788c2ea52d3fe4aa805f2776");
        }

        private static void Case_00739()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 739,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-18,71,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,4,63,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,14,54,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,17,84,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,2,39,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-15,37,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,10,93,37,3), new GeneratedEnemyUnit(1,8,29,21,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "c217fd16ff68f0eb665bb437cf2557dc4fba7d89780d93ef05014ad895bf5cc5");
        }

        private static void Case_00740()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 740,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,1,18,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-18,52,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-20,45,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,5,77,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-16,71,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-7,20,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-3,66,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,3,41,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-12,68,8,3), new GeneratedEnemyUnit(-11,13,97,15,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e618fede583fa509751fe099fa6660a44bcbe0e1c35c77fdead4e9a47d1cc137");
        }

        private static void Case_00741()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 741,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-9,45,3,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "de262f67234a8455135d23d529fa29bb630b496f3338e70ce4f7246142138cb3");
        }

        private static void Case_00742()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 742,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-3,39,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-15,72,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-16,34,16,4), new GeneratedEnemyUnit(-17,8,21,50,2), new GeneratedEnemyUnit(3,9,41,50,1), new GeneratedEnemyUnit(-4,15,92,26,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "829870097fd74fc222e4ff99c97ac1aca11853f5b06a9e3c0f7e0b5feab01ce7");
        }

        private static void Case_00743()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 743,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,7,55,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-10,79,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,12,74,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-14,54,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,13,43,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-6,67,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-6,28,29,4), new GeneratedEnemyUnit(-11,-3,51,38,2), new GeneratedEnemyUnit(-20,-7,45,38,2), new GeneratedEnemyUnit(-15,-17,71,11,1), new GeneratedEnemyUnit(-13,6,18,37,1), new GeneratedEnemyUnit(-7,-20,18,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "d709d315303ace815691139bff5ab18b2c7673d09d8630a8fb246fd368caf6ad");
        }

        private static void Case_00744()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 744,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-1,62,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-9,92,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,3,34,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,5,90,47,4), new GeneratedEnemyUnit(-4,4,86,18,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "0a48435c9e6932b6563305d52ad5c184f79112c154fc3ee6f1b7781edde21005");
        }

        private static void Case_00745()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 745,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-10,11,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,7,67,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-9,20,46,2), new GeneratedEnemyUnit(-20,-13,44,21,3), new GeneratedEnemyUnit(18,17,14,9,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "07a11acea2d3e1bb2c8add8518d59050490fffbeb4ee9e73e4b8af5cea1e32d3");
        }

        private static void Case_00746()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 746,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-10,32,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,5,48,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,15,11,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-1,8,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-10,61,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,6,53,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,3,91,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-18,40,14,1), new GeneratedEnemyUnit(-6,-11,99,23,2), new GeneratedEnemyUnit(3,-13,32,11,2), new GeneratedEnemyUnit(-7,15,58,44,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "70cea94faaa5f8cafd13cfb1d4d4ec0b29d60adfe1f668bc8e1ee0c12292019b");
        }

        private static void Case_00747()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 747,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,7,77,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,8,26,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-12,11,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,2,99,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-14,65,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,12,60,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-18,59,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-2,97,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,12,74,23,3), new GeneratedEnemyUnit(2,19,70,21,3), new GeneratedEnemyUnit(19,13,36,44,2), new GeneratedEnemyUnit(-18,5,38,4,1), new GeneratedEnemyUnit(-16,3,29,36,1), new GeneratedEnemyUnit(3,-5,13,30,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "3675749a9d19f870596e78b0dfd8b7026a145035b0c4fcd6678ff0c18705c94c");
        }

        private static void Case_00748()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 748,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,11,87,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-5,87,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,17,58,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,12,68,32,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "9dd370ad02fc917c0bcb076aa9d4c1d8d7303ecf8d9c3b005e590e5f3730de86");
        }

        private static void Case_00749()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 749,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-10,97,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,13,38,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-10,45,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,17,31,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,10,95,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,16,16,18,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "e02c2f79b358ea4a8ea9914e2d5ce402cf9f7471301322898be4fd49e8774e99");
        }

        private static void Case_00750()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 750,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,10,96,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,6,72,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-16,15,28,3), new GeneratedEnemyUnit(6,-20,43,44,3), new GeneratedEnemyUnit(-18,13,26,5,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "233a5fd365cfea3af4085927ab0f911ba11f47d50d74f5cfc2060ad27b01a604");
        }

        private static void Case_00751()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 751,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-18,98,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-8,88,2,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "8902884ba7627e654b0a35f306acc610cd36ffd0f103faa7f7468c556b5a958d");
        }

        private static void Case_00752()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 752,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-2,92,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-20,43,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,11,100,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,7,47,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,20,92,13,1), new GeneratedEnemyUnit(-5,0,74,46,4), new GeneratedEnemyUnit(5,-18,82,5,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "b0cec2b05a5870f17ec4fa6788ce33c55497bcb289f79e38ee1e0d6e4945179b");
        }

        private static void Case_00753()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 753,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,3,27,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-19,24,4,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "708b6fd16cbd53e42ab69c634c14773cc8e78d41505a79585ac12ddb1274d71e");
        }

        private static void Case_00754()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 754,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-15,37,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-5,74,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,15,100,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-16,19,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-5,24,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,14,96,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-7,65,7,4), new GeneratedEnemyUnit(-2,9,15,22,4), new GeneratedEnemyUnit(-18,-10,74,41,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "13a640799966a852b67bf4ff4f00974362471bbea36caadf71025b7541c312b8");
        }

        private static void Case_00755()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 755,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,19,17,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-8,69,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-9,31,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,0,91,44,2), new GeneratedEnemyUnit(-2,-5,86,44,3), new GeneratedEnemyUnit(-4,-4,12,43,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "313526749c6ea40261ee8c9805f6878c3f73b526d0ebbdaa82ed49863ac24e3e");
        }

        private static void Case_00756()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 756,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-7,55,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-20,10,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,15,34,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-17,97,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,20,8,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,13,76,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,15,32,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,19,54,33,1), new GeneratedEnemyUnit(18,12,21,19,2), new GeneratedEnemyUnit(2,18,37,5,2), new GeneratedEnemyUnit(19,-18,13,5,4), new GeneratedEnemyUnit(-5,-12,35,7,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "1ea4b6d10e785052769cad78f9643fe00f7b2aeea04e87c58befc9bf6f7baed5");
        }

        private static void Case_00757()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 757,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-18,87,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-17,83,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-5,45,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-7,67,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,3,8,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-3,59,1,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "3c67634e59b075b4997991a833a9778d3ed6257b04a43b9de8d92eacaa90e772");
        }

        private static void Case_00758()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 758,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-16,20,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-6,87,16,4), new GeneratedEnemyUnit(15,14,6,46,1), new GeneratedEnemyUnit(6,1,83,23,3), new GeneratedEnemyUnit(-16,12,87,50,2), new GeneratedEnemyUnit(-13,-11,20,13,1), new GeneratedEnemyUnit(-6,-3,63,27,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "bf64d287a12d0993fe9924267def11d43feb5c804d7b7f4f66b97c1b5c9033b2");
        }

        private static void Case_00759()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 759,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,18,98,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,19,71,36,4), new GeneratedEnemyUnit(17,6,38,46,1), new GeneratedEnemyUnit(17,-9,82,39,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "837f24e6f712ed7748bd732cde3c33315a7f1a6115a92935f03d112cb95bfdad");
        }

        private static void Case_00760()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 760,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-11,64,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-20,71,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-20,36,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,15,40,40,3), new GeneratedEnemyUnit(-2,2,42,2,2), new GeneratedEnemyUnit(5,3,60,47,3), new GeneratedEnemyUnit(13,-11,46,3,3), new GeneratedEnemyUnit(2,-1,46,30,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "73cf1394092353321ed560d87e32980c0dc6772553ee68df7810f4285a5a1fb0");
        }

        private static void Case_00761()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 761,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-17,45,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,17,30,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,19,76,7,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "18cc7954f29249217bffeb881cb01905aa3748f5f43d2bc9bf5a87ac11aea87f");
        }

        private static void Case_00762()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 762,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,9,52,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,1,99,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-5,34,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-18,72,6,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "51f09294e43e5ea23bf2426d7e85e4885be2efd6df0d3125727581b98db946c1");
        }

        private static void Case_00763()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 763,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-16,8,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,1,70,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,16,47,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,7,11,18,1), new GeneratedEnemyUnit(-20,16,74,23,3), new GeneratedEnemyUnit(5,0,10,33,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "803ad583e4a79be70b83aea6ef0c070c2f0450745f44da3bde8c2abedf80d333");
        }

        private static void Case_00764()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 764,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,18,8,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-2,89,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,20,67,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,20,83,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,3,55,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-8,7,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,7,47,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,0,70,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,5,53,47,3), new GeneratedEnemyUnit(-4,-6,92,11,3), new GeneratedEnemyUnit(2,16,30,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "54ec598b04cb9ea2ae8b88789d7ecb6b997fcdba98148595a410796134480c97");
        }

        private static void Case_00765()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 765,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-4,60,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-18,95,35,2), new GeneratedEnemyUnit(6,0,49,42,3), new GeneratedEnemyUnit(13,-16,34,10,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "eb58f3ca03551133a8891a344a4d2490e0499d78c56dd3238b795adccc1c7fb9");
        }

        private static void Case_00766()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 766,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,16,87,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,11,96,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,12,91,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,16,89,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,10,75,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-7,12,27,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "454d94746e3fb18d9ff9283dc567b9c6841923bf7b76017edeb80cf53a8b61dd");
        }

        private static void Case_00767()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 767,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-5,59,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-14,76,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-6,61,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-18,67,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-7,87,6,1), new GeneratedEnemyUnit(-11,19,82,37,2), new GeneratedEnemyUnit(7,20,59,29,2), new GeneratedEnemyUnit(-3,-16,78,45,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "d1a84a363a56531fda421dae6468a53b3ac8428137cc85832b8ed1aa434a9a79");
        }

        private static void Case_00768()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 768,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-1,85,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-12,77,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,15,8,2,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c7adcb3903066bc8417bcace1c9bcff6f14791b63228e054874cb88c1fbaa391");
        }

        private static void Case_00769()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 769,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-12,67,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-7,41,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,10,26,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,15,36,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,13,30,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,3,50,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,7,84,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,5,34,43,1), new GeneratedEnemyUnit(-20,9,42,22,3), new GeneratedEnemyUnit(2,4,86,50,4), new GeneratedEnemyUnit(-8,-7,50,40,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "c77858c44ddc49106795397bc178eb76c63d081d9693fb8f8e617aeb300b5962");
        }

        private static void Case_00770()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 770,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,10,45,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,3,42,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-7,35,29,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "dec6aa798ecb976b447e89ec44634c53003f723a7a18b36d9b9ec8c28da221b7");
        }

        private static void Case_00771()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 771,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,3,26,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,7,52,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-19,49,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-1,60,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,19,24,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,15,99,16,1), new GeneratedEnemyUnit(14,-13,14,35,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "941f2658e7af6da7845410ef94ae02979e8d6bd69b39aa4365339821093c3752");
        }

        private static void Case_00772()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 772,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-18,18,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-6,25,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,10,29,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-11,44,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,1,17,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-17,87,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-13,95,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-11,83,7,2), new GeneratedEnemyUnit(-7,16,74,18,1), new GeneratedEnemyUnit(-20,14,24,20,3), new GeneratedEnemyUnit(5,0,73,12,4), new GeneratedEnemyUnit(14,12,10,41,4), new GeneratedEnemyUnit(-7,13,23,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "121a97d5407b90fc83170de55f5387eda16ee7dc4c35c222efd000cb43c4034d");
        }

        private static void Case_00773()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 773,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,18,21,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-3,91,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,17,13,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,13,96,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-5,92,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-15,57,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-20,81,10,4), new GeneratedEnemyUnit(-14,-3,99,2,1), new GeneratedEnemyUnit(16,-8,42,23,1), new GeneratedEnemyUnit(-2,-17,47,30,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "0e064aebef35780892ea727e98f0e6567f2486c0bcd7803a832fde40abb25d61");
        }

        private static void Case_00774()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 774,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,2,14,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-17,11,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-12,43,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-1,36,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-9,62,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,6,62,30,1), new GeneratedEnemyUnit(9,4,15,13,2), new GeneratedEnemyUnit(-7,-8,48,8,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "a35aa2aa1cbc5b37b67226e5841c9a2acae609ce5b753bd7f1b04ec5345a7187");
        }

        private static void Case_00775()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 775,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-9,6,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,9,92,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-6,42,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,19,53,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-14,42,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-18,46,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-17,6,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,14,11,14,2), new GeneratedEnemyUnit(12,9,70,1,1), new GeneratedEnemyUnit(2,20,8,26,1), new GeneratedEnemyUnit(-17,-8,59,42,2), new GeneratedEnemyUnit(12,-4,32,23,4), new GeneratedEnemyUnit(19,6,36,32,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "b3a5b9ef4ef806a47b7163d149432273d27746840f4a1f1ae676c08c70b3d31c");
        }

        private static void Case_00776()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 776,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-15,14,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,4,53,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-5,28,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,16,43,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-12,94,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,18,72,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,3,6,14,2), new GeneratedEnemyUnit(14,2,76,19,1), new GeneratedEnemyUnit(-15,16,44,37,2), new GeneratedEnemyUnit(-17,-7,86,21,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "eb499fc6f8fb4364b8acfe54da6339d1cb301aaab7664613442589c17cc5b6d5");
        }

        private static void Case_00777()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 777,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,0,33,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,1,37,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-13,23,5,1), new GeneratedEnemyUnit(16,16,82,5,1), new GeneratedEnemyUnit(16,7,46,50,4), new GeneratedEnemyUnit(3,-12,76,26,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "45a0dd2d13306cb82964d76e6078e15b91ed8202fa424583774d8aac1efb55bb");
        }

        private static void Case_00778()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 778,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-20,99,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,11,74,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,4,77,31,4), new GeneratedEnemyUnit(0,-9,24,10,2), new GeneratedEnemyUnit(-16,2,52,28,3), new GeneratedEnemyUnit(-10,-17,14,33,2), new GeneratedEnemyUnit(-6,0,78,41,2), new GeneratedEnemyUnit(-1,-1,31,38,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "01b1e7f550ca672133d9cefdc09320a82f9e4c8d8bfc55389a3dc7579244bb7b");
        }

        private static void Case_00779()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 779,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-16,34,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-1,32,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,1,7,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-3,89,4,2), new GeneratedEnemyUnit(-13,-20,23,27,2), new GeneratedEnemyUnit(-16,12,51,32,2), new GeneratedEnemyUnit(8,8,11,46,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "8ce729ed29418604e36e499410c49841d2e8569f8fb06e1ab2418349a79e3a4e");
        }

        private static void Case_00780()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 780,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,5,7,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-7,26,24,2), new GeneratedEnemyUnit(-1,-1,10,49,2), new GeneratedEnemyUnit(2,-13,44,49,3), new GeneratedEnemyUnit(13,-5,44,46,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b11095b187ee9310898246584f6c29f826547e529059e693b3a0a887c6b7c0a9");
        }

        private static void Case_00781()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 781,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,6,20,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-5,15,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,16,35,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,15,27,26,4), new GeneratedEnemyUnit(-17,0,25,3,2), new GeneratedEnemyUnit(-20,-6,40,13,4), new GeneratedEnemyUnit(-16,-18,63,22,2), new GeneratedEnemyUnit(-17,15,15,26,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "c908485ece8e4243726700ed935d21968c57468870e4d5eb445d203ac6b337ee");
        }

        private static void Case_00782()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 782,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,12,94,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-18,38,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,5,40,5,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "0f8857dfaa8480a4b7f89fb8cfcf8ca9573ea86b684620a5e153f11d70642b6e");
        }

        private static void Case_00783()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 783,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-17,25,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-18,6,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,4,68,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-14,48,33,4), new GeneratedEnemyUnit(-7,-2,78,15,3), new GeneratedEnemyUnit(-12,-16,53,42,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "207c21c3e007efbed432f52140a3f433e4a63509c616610cf370f4c7b62edeab");
        }

        private static void Case_00784()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 784,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,18,15,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,13,97,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,8,42,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-14,65,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-5,64,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-12,47,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-2,16,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-13,25,35,3), new GeneratedEnemyUnit(-5,13,10,40,3), new GeneratedEnemyUnit(10,12,7,45,4), new GeneratedEnemyUnit(-6,2,85,38,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b5cd9b673efb3f817a2fb86bb899c6de2966874e5cf9fd76439a89f1ecd405c1");
        }

        private static void Case_00785()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 785,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-20,86,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-3,66,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,12,6,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,19,84,19,2), new GeneratedEnemyUnit(7,-11,23,38,3), new GeneratedEnemyUnit(10,-11,49,26,1), new GeneratedEnemyUnit(2,17,9,35,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "96c194df72a68f43bdb753059285260505a5ab5edb7074128c62dac5c71dfad7");
        }

        private static void Case_00786()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 786,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-13,17,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-8,93,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,20,34,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-7,42,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-13,29,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,9,34,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,7,98,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,6,85,4,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "6e887d25f77b50094c85ac0ec6fb7069be56f81c4a18d4cdad5ccc0833574500");
        }

        private static void Case_00787()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 787,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-6,54,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,15,43,15,2), new GeneratedEnemyUnit(-10,13,34,24,4), new GeneratedEnemyUnit(-17,11,69,21,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "658ddc1cc141a9df6f9816fe8470e61b808d24e4878f16fe0a5202702dd4cf97");
        }

        private static void Case_00788()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 788,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,15,96,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-5,51,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-3,72,47,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "d98f5dba3615ab5fab781b470bed2bd692ebde43ccd1e842cddd8a0c5115963e");
        }

        private static void Case_00789()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 789,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,11,46,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-17,52,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-20,55,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,14,17,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-4,82,16,4), new GeneratedEnemyUnit(-5,-1,66,23,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "4cdb7fb2e1cd03ce9db404d6deb0bb2d27b908112cbdfc7410869f22b42a7b6f");
        }

        private static void Case_00790()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 790,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-6,48,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-5,64,4,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8e7f5a403f090c18702abb8a3a99992e87bdb63ce100fe8ade41cb880f895414");
        }

        private static void Case_00791()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 791,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-19,58,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-10,31,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-20,80,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,6,71,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-14,40,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-5,93,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-14,84,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-6,86,16,3), new GeneratedEnemyUnit(4,8,53,12,3), new GeneratedEnemyUnit(-5,2,90,19,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "64583e2c8fd729fb579d2f90c987d81fd5e2de7ef1377eab9614cbbc618f9f91");
        }

        private static void Case_00792()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 792,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-10,42,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,0,38,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-17,87,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,20,71,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,3,10,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,6,45,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-3,36,10,3), new GeneratedEnemyUnit(-2,14,26,18,1), new GeneratedEnemyUnit(-16,11,13,46,3), new GeneratedEnemyUnit(-19,-11,56,44,3), new GeneratedEnemyUnit(19,1,65,30,3), new GeneratedEnemyUnit(12,-19,63,13,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "34288112616410aa0bc64bfe63849e7326fb461a230713552ba56c680fba81ed");
        }

        private static void Case_00793()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 793,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-18,86,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,19,19,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,7,10,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,5,97,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-15,31,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,19,53,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-16,22,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-5,98,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,4,18,25,3), new GeneratedEnemyUnit(-2,2,13,40,1), new GeneratedEnemyUnit(-19,5,76,19,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "93baf61e81e33c353a052ba925799d37bffdd13f0296869af4804c380ba6923d");
        }

        private static void Case_00794()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 794,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,0,21,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-20,27,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,10,70,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,19,23,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,4,100,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,8,92,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-4,23,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,15,31,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,10,28,25,1), new GeneratedEnemyUnit(0,-19,5,14,3), new GeneratedEnemyUnit(19,3,35,49,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "14c15ae9e6668d935cc8fc39409bd1b8eb2bd41b0a99fc4aaabd4bc75eca6956");
        }

        private static void Case_00795()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 795,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-15,36,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-8,31,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-6,66,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-8,8,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-5,83,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,17,54,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,8,61,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,14,56,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-15,8,18,2), new GeneratedEnemyUnit(15,-16,80,16,4), new GeneratedEnemyUnit(-7,1,34,50,3), new GeneratedEnemyUnit(-5,-8,42,42,2), new GeneratedEnemyUnit(-9,-10,14,3,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "d48216580bbb0cab34737ac2298c9fcf2eba36999f207527132d55abed6df764");
        }

        private static void Case_00796()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 796,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-2,94,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,17,5,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,6,7,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-20,88,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,7,83,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-19,72,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,2,38,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-20,27,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,20,19,10,1), new GeneratedEnemyUnit(4,-3,65,20,4), new GeneratedEnemyUnit(-6,-18,39,25,4), new GeneratedEnemyUnit(14,-5,77,12,3), new GeneratedEnemyUnit(-16,14,94,26,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "1cb54422cca70f6096279897bd377a3b24b11a2cf679139a61a173b0903ce84b");
        }

        private static void Case_00797()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 797,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,13,66,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-9,77,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,3,95,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-2,86,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-9,25,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,16,5,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,15,63,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,0,88,29,4), new GeneratedEnemyUnit(6,-7,38,18,4), new GeneratedEnemyUnit(11,10,27,5,1), new GeneratedEnemyUnit(-20,-20,13,32,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "6c955c5277013a5c2d5634e6df141e7c12ba4ce545416c46cec4736921d83bb5");
        }

        private static void Case_00798()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 798,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-9,64,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-9,27,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-14,91,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,5,11,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,8,37,23,3), new GeneratedEnemyUnit(18,-17,15,49,1), new GeneratedEnemyUnit(18,-14,40,22,4), new GeneratedEnemyUnit(6,4,75,32,3), new GeneratedEnemyUnit(20,15,52,7,2), new GeneratedEnemyUnit(10,-10,37,24,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "80684ddc8efc913ab74b92dce0076c266808cb9584ebef7a28ac2f0bafc5f6ee");
        }

        private static void Case_00799()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 799,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-14,54,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,20,23,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,7,70,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,3,22,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-18,28,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,1,5,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-17,9,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,11,17,31,2), new GeneratedEnemyUnit(-16,-15,57,38,2), new GeneratedEnemyUnit(20,6,15,16,4), new GeneratedEnemyUnit(0,-19,26,3,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8f683913b0504b9f6d53f09e70d6dd0c27a7183cace7b6c6c3cfc04652322a79");
        }

        private static void Case_00800()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 800,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-12,33,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,5,12,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,3,33,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-6,53,42,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "21f9d092c88fd51c97dc54cedf67b84eca655125de0a434254e91ede479597ea");
        }

        private static void Case_00801()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 801,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,20,98,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-19,89,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-15,26,3,2), new GeneratedEnemyUnit(3,9,24,8,1), new GeneratedEnemyUnit(20,-10,97,4,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "7bca52848087dc947a7ac3bc042aea24e4f0752dbd22a9e15256d08236058e92");
        }

        private static void Case_00802()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 802,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,11,96,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,0,6,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,8,39,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-4,96,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,14,31,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-19,42,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-16,52,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,16,64,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-12,6,46,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "417e305fe9a597d67a275f827e48a8e61f062a280d5935e77fa48199aa474c3d");
        }

        private static void Case_00803()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 803,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-12,85,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,4,85,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,18,49,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-17,38,33,2), new GeneratedEnemyUnit(2,-5,19,40,3), new GeneratedEnemyUnit(6,-1,27,37,1), new GeneratedEnemyUnit(-9,-2,44,7,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "cddc1bdf1d16008e9a58feffc58986cb4af105ad76197e5836b84eb7d9bb527a");
        }

        private static void Case_00804()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 804,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,16,8,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,0,41,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,8,54,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,6,75,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-12,33,7,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "df102cd9eae09834cbcbd4f66c1d3d06d81306e91aa9e10fad40336d94985fb9");
        }

        private static void Case_00805()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 805,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,3,23,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,10,21,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,15,20,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,8,94,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,1,97,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,14,8,19,2), new GeneratedEnemyUnit(8,5,97,46,2), new GeneratedEnemyUnit(20,-16,96,44,1), new GeneratedEnemyUnit(-9,0,84,33,3), new GeneratedEnemyUnit(-10,-13,49,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "a3ab0f31634511e736117ef1f32d576cd5950aeef7b48f19f84c0c3f1dbf6c43");
        }

        private static void Case_00806()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 806,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-12,72,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,19,6,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-13,28,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,10,95,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,11,38,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,1,8,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-16,8,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-15,16,5,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "9a48cf0fa7b733a23095ca72aed4734826c8f714114a9eb1eb5f9d3b5d1d6d05");
        }

        private static void Case_00807()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 807,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,6,15,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,16,63,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,2,44,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-5,78,47,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "bd2ada65214691cdaa3969270f4d03f7a033df360517b1765bab5c71820565a8");
        }

        private static void Case_00808()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 808,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,13,46,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,9,79,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,3,37,7,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "137f508d29a4015690b5dd824777487e37cab22502cc88c516adfb089cc6704e");
        }

        private static void Case_00809()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 809,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,10,51,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,2,97,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,18,8,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,15,48,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-16,54,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,13,19,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-13,73,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-12,94,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,13,75,50,2), new GeneratedEnemyUnit(-5,-10,60,45,3), new GeneratedEnemyUnit(-11,10,33,4,2), new GeneratedEnemyUnit(14,-16,78,32,3), new GeneratedEnemyUnit(-15,13,45,16,1), new GeneratedEnemyUnit(14,-15,98,38,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "0025adddcb6a469beea8ceceafa52d7d1a3715b573e968bfabd41792e32bd248");
        }

        private static void Case_00810()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 810,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,9,40,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-7,64,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-19,37,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,2,11,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-12,40,5,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "057677f3e93844dce7061ee912e88c4498a4d09577809b6747fc1602fa9aa464");
        }

        private static void Case_00811()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 811,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-19,65,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,19,35,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-14,92,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,5,85,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,14,89,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,0,16,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,17,34,2,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "0e72f4d1227189a6ad0eabede4201100c7acd31444031d7ad26d689cfc508cab");
        }

        private static void Case_00812()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 812,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,13,57,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,10,54,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-3,34,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-3,90,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,10,97,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-18,34,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-12,40,2,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "e24ac398af0d10b4eeb65994f08872f7d3828afc23250a0a889cfd9c6f10aee3");
        }

        private static void Case_00813()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 813,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-10,55,3,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "e93915ef475c3ea30026d825f1116d1273312e227fa395354342c51248c0d7ea");
        }

        private static void Case_00814()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 814,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,19,39,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,3,73,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,16,28,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-20,27,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-2,9,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,2,97,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,13,81,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,2,69,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-19,73,9,3), new GeneratedEnemyUnit(2,6,73,22,2), new GeneratedEnemyUnit(-16,-14,93,28,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "8c7786fdfa52a9b1727843a17d1f63817b99b30382d7a88e1f0f7fabd8c57ac1");
        }

        private static void Case_00815()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 815,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,12,62,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,17,31,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,12,87,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,12,60,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-20,54,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-11,26,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,11,80,10,2), new GeneratedEnemyUnit(-2,-8,51,48,2), new GeneratedEnemyUnit(12,-11,30,26,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "347b7983523ea03c6748ecc61d3055f3179625c01047d76f0e3a5e9ce4487e88");
        }

        private static void Case_00816()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 816,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,0,94,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-14,12,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-1,22,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-17,91,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,1,62,3,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "8049f277f7de743fa5709532e4bd7434c9f5426ca12e6b25bc2b099f3779a3bc");
        }

        private static void Case_00817()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 817,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,9,79,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-4,20,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-7,34,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,7,91,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-19,74,11,1), new GeneratedEnemyUnit(6,0,36,8,4), new GeneratedEnemyUnit(1,-18,27,8,2), new GeneratedEnemyUnit(18,5,26,46,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "3f8de3520d2deafb1e3d2978c7960729dc0e4b8351098055d41ffd6d8c13f210");
        }

        private static void Case_00818()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 818,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,13,70,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,2,27,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-5,39,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-20,46,17,4), new GeneratedEnemyUnit(-18,18,98,39,2), new GeneratedEnemyUnit(-5,9,85,12,4), new GeneratedEnemyUnit(-4,19,67,44,1), new GeneratedEnemyUnit(10,-14,60,23,2), new GeneratedEnemyUnit(18,11,98,28,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "835b918ca40a03f8fb410bf6ebc29fd8e06efeb84e20c0063e6441c3cc9d25d5");
        }

        private static void Case_00819()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 819,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-2,85,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,9,76,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,18,76,18,2), new GeneratedEnemyUnit(10,14,5,46,1), new GeneratedEnemyUnit(19,0,66,26,4), new GeneratedEnemyUnit(4,2,30,15,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "3878795f617889da71109f631de2849eb0819b0420a05c03386120f53d231044");
        }

        private static void Case_00820()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 820,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-4,59,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,3,13,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,5,11,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,6,91,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-16,57,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,19,55,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-7,11,23,4), new GeneratedEnemyUnit(-14,4,62,22,2), new GeneratedEnemyUnit(-16,19,19,31,3), new GeneratedEnemyUnit(6,17,16,32,1), new GeneratedEnemyUnit(-1,-13,14,16,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "cd40a9c72fd849bec626deb663f22eeecbef7ecb8f177ac9312aff1a135adae1");
        }

        private static void Case_00821()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 821,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,2,55,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,0,42,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,20,39,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-7,50,44,1), new GeneratedEnemyUnit(17,-7,60,13,4), new GeneratedEnemyUnit(-1,13,66,42,4), new GeneratedEnemyUnit(-20,-16,55,16,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "7470343d258738a8cccbb860812d51eef72f12a841459a62a347d4438a4046db");
        }

        private static void Case_00822()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 822,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,13,56,3,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "e30f2227f281cd3af6d3798e12ee8d4b0f7bbf3bb8cf685c5a4e07d229e50381");
        }

        private static void Case_00823()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 823,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,0,73,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-4,85,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,20,38,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,3,71,14,3), new GeneratedEnemyUnit(17,-12,77,45,3), new GeneratedEnemyUnit(-17,1,65,1,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "870fdf44a1c2cc6f104afbbf1ca193780591f183b0a840b6cf93d581db500476");
        }

        private static void Case_00824()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 824,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,10,61,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,6,44,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-6,97,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,4,56,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-18,93,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,1,84,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-19,59,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-9,96,37,4), new GeneratedEnemyUnit(14,0,11,21,2), new GeneratedEnemyUnit(15,7,82,7,1), new GeneratedEnemyUnit(-6,0,58,40,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "998430cba4baaa9399b26dfe97e01a7eeaef660944c9d206e13fa064b46e4f6d");
        }

        private static void Case_00825()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 825,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,9,20,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-19,14,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,19,26,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-2,41,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-16,23,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,16,71,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,10,75,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-2,33,48,4), new GeneratedEnemyUnit(17,10,98,39,4), new GeneratedEnemyUnit(6,5,18,31,1), new GeneratedEnemyUnit(-4,-9,21,36,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "8e53999aa6fe293f2afa4578ca10b528993383686840e728674681eb7dc7ed42");
        }

        private static void Case_00826()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 826,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-13,83,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,1,71,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,8,43,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-15,56,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-20,53,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,20,52,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,3,76,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,16,13,32,1), new GeneratedEnemyUnit(-9,15,5,38,4), new GeneratedEnemyUnit(-15,11,16,9,2), new GeneratedEnemyUnit(-17,7,94,1,2), new GeneratedEnemyUnit(13,6,60,38,1), new GeneratedEnemyUnit(-11,-15,60,20,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "c19aa704bfb638abd881bff66788c6fc919728bcc768e05bc57ca2528a7004d2");
        }

        private static void Case_00827()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 827,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,6,23,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,17,11,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,11,84,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-8,91,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,9,72,13,3), new GeneratedEnemyUnit(0,13,79,24,3), new GeneratedEnemyUnit(-10,1,78,15,2), new GeneratedEnemyUnit(-9,13,67,39,2), new GeneratedEnemyUnit(20,6,8,2,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "c80be39c95a516bbe35d35418285b79c1ed2e8ef9a2824cac714c98d2900ae5c");
        }

        private static void Case_00828()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 828,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-20,97,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-5,12,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-11,17,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-9,61,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,9,65,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,19,86,2,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3d25c9c498cdd888c6be0f91aec835d045927a02149964e97ab4d58d7d5d55e5");
        }

        private static void Case_00829()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 829,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,11,13,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,4,58,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,2,98,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,15,7,4,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "ca9378b5507cca3c854b58dcf6814e01fe772ea106d4962d612748ff8dcdc35a");
        }

        private static void Case_00830()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 830,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-9,84,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-6,74,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-17,15,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,4,44,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-9,57,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-8,52,17,3), new GeneratedEnemyUnit(18,-15,41,10,1), new GeneratedEnemyUnit(6,13,57,10,4), new GeneratedEnemyUnit(12,1,68,31,3), new GeneratedEnemyUnit(-19,4,74,10,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "ae40e8ef68c9be3e83bda673cf024f91b6a5cb96c8f1dcbe09ed6d6755ecb858");
        }

        private static void Case_00831()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 831,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,2,40,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-11,48,25,1), new GeneratedEnemyUnit(-6,-10,81,17,3), new GeneratedEnemyUnit(-15,6,37,26,4), new GeneratedEnemyUnit(7,17,62,23,3), new GeneratedEnemyUnit(-18,-7,32,46,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "41275fb617d57cb677285f896207ad9cfe444e697d43207f8fc4215b45b40644");
        }

        private static void Case_00832()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 832,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,20,54,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,15,5,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,14,55,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,1,50,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,12,99,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,16,12,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,16,79,47,3), new GeneratedEnemyUnit(5,9,60,24,3), new GeneratedEnemyUnit(2,-5,73,7,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "13b13a25e8f53d8c83c289c5208e0809288fa4da562c77aaf38047dd41f1ecd7");
        }

        private static void Case_00833()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 833,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,1,79,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,5,14,6,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "ede8aee4dcb40710bd3cb806d960a1047cce4abda4d972992d3c059913196f71");
        }

        private static void Case_00834()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 834,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,20,67,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,12,45,7,3), new GeneratedEnemyUnit(9,5,93,40,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "ce38d08473b68be6647e78e0b9d28bb55d3c325c7d9a26153fe0f6675a545c7e");
        }

        private static void Case_00835()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 835,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-13,5,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,11,93,45,4), new GeneratedEnemyUnit(-12,14,62,18,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "f0df61ecc5a7c513fe87727a4bc4be3a3bb0dcb97d76d8995e3cf37f32b4fe28");
        }

        private static void Case_00836()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 836,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,11,11,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-11,98,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-9,8,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,18,56,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-15,80,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,18,47,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-6,39,11,4), new GeneratedEnemyUnit(-9,19,51,23,4), new GeneratedEnemyUnit(-14,19,51,44,2), new GeneratedEnemyUnit(-12,5,76,43,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "28aa9581785185f3c849055e1632d2a5d2f3a6064fd68e2327c8128ef8c2c4d4");
        }

        private static void Case_00837()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 837,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,4,99,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-6,98,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,15,82,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,16,55,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-13,12,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,13,56,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-16,38,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-12,67,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-13,19,35,1), new GeneratedEnemyUnit(13,-7,15,28,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "2df12b107b6a45d02efa58371465af564974d3b175098a3628eaecf753d42217");
        }

        private static void Case_00838()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 838,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,5,24,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-12,85,27,1), new GeneratedEnemyUnit(7,-2,94,13,2), new GeneratedEnemyUnit(-16,18,14,2,1), new GeneratedEnemyUnit(-4,-13,85,46,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "350b5db955ec4ba975507546b2a7eb98daa5c82984885df16f80a2e7de42d98d");
        }

        private static void Case_00839()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 839,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,2,64,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,2,14,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-16,92,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-19,21,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,11,12,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,5,10,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,14,100,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-8,88,23,2), new GeneratedEnemyUnit(-13,1,9,9,2), new GeneratedEnemyUnit(11,-18,51,32,4), new GeneratedEnemyUnit(3,-11,81,32,4), new GeneratedEnemyUnit(-17,-10,17,6,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "10eb42dd00bae8684a8aee4b24f788dc25168c8290c6e4ec9944884e16f6e447");
        }

        private static void Case_00840()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 840,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,4,50,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-17,34,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,5,29,26,4), new GeneratedEnemyUnit(-4,18,42,49,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "626c5c8885b0b3fbcb238d84564e3b5ba09fe51af38341acea4742b4ac58acc0");
        }

        private static void Case_00841()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 841,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-11,20,3,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "c8f71f5c67de155b592149415014756729f0b6eeef373982829076bfe5d3c7c1");
        }

        private static void Case_00842()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 842,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-6,78,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,3,62,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,16,45,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,15,32,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,4,79,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,14,30,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,8,79,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,7,97,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-2,48,10,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "33099426b54677f5d8926a65ce226f96cf29392c0d3cd4f709e77157f2045964");
        }

        private static void Case_00843()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 843,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-14,70,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-20,95,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,3,24,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,19,29,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-15,25,7,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "7e3ad55815c39ffa2f2e90df0a72a24df4e128c62a69b45982506d83e1345a00");
        }

        private static void Case_00844()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 844,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,18,26,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,8,55,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-4,19,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,20,62,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-8,15,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,9,68,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-11,41,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,0,47,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,6,25,44,2), new GeneratedEnemyUnit(3,18,21,46,1), new GeneratedEnemyUnit(-11,2,41,21,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "596af743c359d8a837f7ba2a3b3c5b94d6bd0fa67f019d2cfb1c00f3e6bc6c7d");
        }

        private static void Case_00845()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 845,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-9,88,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-14,69,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,3,60,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-4,32,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-10,28,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-12,38,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-18,77,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-3,7,35,4), new GeneratedEnemyUnit(7,-4,41,38,4), new GeneratedEnemyUnit(19,-8,14,6,1), new GeneratedEnemyUnit(-9,14,99,21,2), new GeneratedEnemyUnit(18,9,17,39,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "408d6adef4ba7ec4993d28ce7a3b3103e2ba38ccf62ac800324fe84e1c484ea3");
        }

        private static void Case_00846()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 846,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,13,37,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-8,67,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-3,36,22,2), new GeneratedEnemyUnit(2,9,26,39,1), new GeneratedEnemyUnit(-13,-1,29,33,2), new GeneratedEnemyUnit(-9,-10,7,18,4), new GeneratedEnemyUnit(1,9,68,7,3), new GeneratedEnemyUnit(8,-14,9,26,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "49413f3c2f0706ccfca82fd698983a5de3f3dff11dc57b4f8fe04303f2eb7b1c");
        }

        private static void Case_00847()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 847,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,20,64,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,19,50,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-16,70,49,1), new GeneratedEnemyUnit(-10,8,58,24,1), new GeneratedEnemyUnit(-19,-2,7,38,4), new GeneratedEnemyUnit(-8,15,39,3,1), new GeneratedEnemyUnit(3,5,73,6,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "e9c510ecc356f20ec39ae563ca28086ff3dc3d79a4fc3706ee7d53a9e9b9e75b");
        }

        private static void Case_00848()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 848,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,6,11,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-11,38,29,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "c88017a22a47e768095a715cdf0594bd025b9534977e2ecc015c125ca4b714a1");
        }

        private static void Case_00849()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 849,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,1,90,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-20,41,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-6,10,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-19,19,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-13,30,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-20,64,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-17,45,41,3), new GeneratedEnemyUnit(-3,-4,88,16,3), new GeneratedEnemyUnit(0,-3,70,32,4), new GeneratedEnemyUnit(2,-14,45,49,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "9351326fc71c459575cac972c96aa73f3f69da9ba4819faad20791b1b6aeb210");
        }

        private static void Case_00850()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 850,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-13,88,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-14,97,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,13,64,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-16,80,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-18,6,7,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "842ab0e349b405aee35b6857b120958f806d74975c791bbb9b74b08b2047817b");
        }

        private static void Case_00851()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 851,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-20,67,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-14,92,30,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "80da968884de9b1956fd2d96168c392fe7f834dc877263def6b6944a65237665");
        }

        private static void Case_00852()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 852,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-7,76,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-20,84,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,1,91,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-8,49,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,8,69,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-6,30,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,19,15,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,16,92,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-2,40,47,1), new GeneratedEnemyUnit(10,20,39,36,4), new GeneratedEnemyUnit(8,-15,7,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "b5d2774ba988f3aef2cb20ab0ba0456032d230e06fa21f79bb32295f66f3f6ff");
        }

        private static void Case_00853()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 853,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-4,40,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,18,8,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-5,95,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,13,20,49,2), new GeneratedEnemyUnit(-17,0,30,31,1), new GeneratedEnemyUnit(-14,13,45,9,2), new GeneratedEnemyUnit(10,12,18,7,2), new GeneratedEnemyUnit(13,-19,40,14,2), new GeneratedEnemyUnit(-13,2,100,42,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "d7841c0228448ca69aadcd4145a791a67038561ec07d902f81c6b51df43cdb54");
        }

        private static void Case_00854()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 854,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-3,87,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-16,74,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-6,100,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-19,5,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,3,25,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-15,89,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-11,96,8,2), new GeneratedEnemyUnit(1,17,25,24,4), new GeneratedEnemyUnit(4,0,34,47,3), new GeneratedEnemyUnit(2,19,70,15,4), new GeneratedEnemyUnit(19,-13,5,21,3), new GeneratedEnemyUnit(12,5,90,15,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "46827f6468ad0a37fb4c146fe304174b627b8cfbb3b1d6bca0ff9999fc13c9d3");
        }

        private static void Case_00855()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 855,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,0,10,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,16,84,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,3,14,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-14,31,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,11,29,2,3), new GeneratedEnemyUnit(4,11,38,6,2), new GeneratedEnemyUnit(-18,-13,8,40,2), new GeneratedEnemyUnit(-10,-16,5,7,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "5f58dfd1aa5d1f609c9c7a59214125aa20531d9947bd7f837c1a9e533c14a18d");
        }

        private static void Case_00856()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 856,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,10,53,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,18,58,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,12,64,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,10,5,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,7,21,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,11,30,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,1,6,38,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ebe1b9666a4b5903f150dc1f84239f7d687e87245c0b17b233fae500eea903b0");
        }

        private static void Case_00857()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 857,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,17,74,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,4,38,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,13,21,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,2,84,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,8,51,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,20,75,18,4), new GeneratedEnemyUnit(-15,-14,14,21,4), new GeneratedEnemyUnit(14,4,16,19,1), new GeneratedEnemyUnit(4,-8,47,30,4), new GeneratedEnemyUnit(20,-16,33,47,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "9bc893d8aa270255309779ffd921a4bcae92df7788224e295625e3534a3a94bf");
        }

        private static void Case_00858()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 858,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-7,84,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,5,89,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-8,33,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-3,98,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,5,63,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-18,45,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,5,11,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-14,5,19,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f3dc10d50b66bc84848779959bf9bf034a7748780c669693640bd4c26b2a5eb5");
        }

        private static void Case_00859()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 859,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,17,30,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,16,36,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,3,74,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,1,38,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,3,36,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,10,60,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-11,42,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,14,73,14,2), new GeneratedEnemyUnit(-18,-6,61,47,4), new GeneratedEnemyUnit(-7,20,65,43,2), new GeneratedEnemyUnit(7,-9,35,32,4), new GeneratedEnemyUnit(-4,-6,12,28,2), new GeneratedEnemyUnit(-16,13,66,40,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "a675376d85c9edbc5b513cd547e9763a92682aa586852333be8b87cbfccb4cd5");
        }

        private static void Case_00860()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 860,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,10,40,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-15,57,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,18,92,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-16,64,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,4,82,17,1), new GeneratedEnemyUnit(-15,-2,54,45,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "d10aee6db73b0c9de045b7f0c9039967ba83f393d08ed0fb2a5e27014303f96a");
        }

        private static void Case_00861()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 861,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-14,67,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-15,92,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-8,71,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-7,80,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,0,95,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,20,88,25,3), new GeneratedEnemyUnit(-1,5,100,22,3), new GeneratedEnemyUnit(3,-1,48,23,3), new GeneratedEnemyUnit(-1,-8,56,22,1), new GeneratedEnemyUnit(10,5,88,15,1), new GeneratedEnemyUnit(0,-1,51,25,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "8641af7677381d131d080354b18d36f2a674413c68b66ede7d4efcd90587167b");
        }

        private static void Case_00862()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 862,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-12,11,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-15,42,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,16,10,8,3), new GeneratedEnemyUnit(14,11,49,45,1), new GeneratedEnemyUnit(4,-8,92,12,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "677ac1b6cf57e0f041152390f456a113a11a9c80d122390b7e2b1978f26d556c");
        }

        private static void Case_00863()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 863,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-2,20,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,19,48,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-2,100,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,4,57,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-20,21,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,19,12,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-6,69,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-8,22,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-4,5,34,4), new GeneratedEnemyUnit(-6,16,82,26,4), new GeneratedEnemyUnit(14,-6,71,9,4), new GeneratedEnemyUnit(7,-10,26,13,3), new GeneratedEnemyUnit(18,-5,47,47,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "06ad819ea8088989121763abd69783eed89a7057477ce9780253da28547f1133");
        }

        private static void Case_00864()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 864,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-17,30,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,6,30,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,7,63,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-19,39,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-9,61,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,5,39,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,6,14,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,1,71,19,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "cad9056b9516bf3afb3e9d3fffa70d2c620378ff8aa6fbe369a79a02c815ded2");
        }

        private static void Case_00865()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 865,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-2,94,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,15,28,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-2,5,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-20,64,35,2), new GeneratedEnemyUnit(15,-18,11,36,1), new GeneratedEnemyUnit(6,10,87,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "419a40167fc3cd6d09988feaf84a955279835fbea1cf1005a0e359d49350a348");
        }

        private static void Case_00866()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 866,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,20,20,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-12,88,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,17,52,5,2), new GeneratedEnemyUnit(-1,-14,90,37,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "43484fd5876d4dc60c7a19aee2e5560a4f5644caf07cacbe49edffa6b956ca6f");
        }

        private static void Case_00867()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 867,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-6,44,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-8,89,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,8,23,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,5,14,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,4,67,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,20,48,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,8,63,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-6,70,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,16,22,18,3), new GeneratedEnemyUnit(-2,9,91,42,4), new GeneratedEnemyUnit(-7,11,96,48,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "0a63c67116937c543f8ed1e1d76b99fcd89b7bcd82c78de216bb596840f6a2ff");
        }

        private static void Case_00868()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 868,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-11,91,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-2,96,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-18,42,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-6,75,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-9,53,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-15,61,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-11,52,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-12,92,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-11,59,17,4), new GeneratedEnemyUnit(9,-1,80,9,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b5cc80605ef6a1f155a3ee459bafed796349c6230c20db7d881cc58ecadc335b");
        }

        private static void Case_00869()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 869,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-14,54,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,5,26,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,10,61,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-11,5,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,13,24,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,5,21,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,19,45,35,1), new GeneratedEnemyUnit(-4,-16,44,2,1), new GeneratedEnemyUnit(20,5,62,38,3), new GeneratedEnemyUnit(9,-19,11,12,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "1448a82c354108ba19e642bcbc3fe09f6f18d90c59c9c23922f5f92d8053857d");
        }

        private static void Case_00870()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 870,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-19,31,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,18,32,46,2), new GeneratedEnemyUnit(18,20,89,22,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "678c610bcb84ec963174230c05123ab4464f26d76d0839b4839edfbcc568c43a");
        }

        private static void Case_00871()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 871,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-12,94,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,2,16,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-2,95,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-14,51,26,1), new GeneratedEnemyUnit(10,10,77,22,3), new GeneratedEnemyUnit(-7,8,76,25,2), new GeneratedEnemyUnit(1,-13,79,30,1), new GeneratedEnemyUnit(1,5,83,33,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "863bf0a90d818dcad414f8f8a6fa4da3453e0501ccc497fdb96019092adb78b1");
        }

        private static void Case_00872()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 872,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-19,73,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-4,82,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-18,78,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,3,91,15,4), new GeneratedEnemyUnit(16,4,24,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "d8132a2c286e4a9ef4f122fd7876e2d9aa1fcd2fe7adcb4152c42885b7ff44ec");
        }

        private static void Case_00873()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 873,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,1,14,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,2,10,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-11,34,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-3,71,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-19,26,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,17,31,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-1,67,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-17,9,9,3), new GeneratedEnemyUnit(5,8,12,31,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "7c5d625f4958ba43f3099906f0973a57ddd7155162336a3ab6b857c7827487a4");
        }

        private static void Case_00874()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 874,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,2,62,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-18,86,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-20,64,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-14,61,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,18,22,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-19,82,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,5,54,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-17,21,46,1), new GeneratedEnemyUnit(-19,12,24,32,2), new GeneratedEnemyUnit(12,6,34,11,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "4784007e527c64d8da05c315c31d834923288800ede2dc28eaa54b1455bbed74");
        }

        private static void Case_00875()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 875,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-15,47,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,5,85,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,11,41,7,1), new GeneratedEnemyUnit(-8,16,39,26,4), new GeneratedEnemyUnit(10,-17,99,17,1), new GeneratedEnemyUnit(-9,3,41,21,2), new GeneratedEnemyUnit(1,15,10,15,2), new GeneratedEnemyUnit(-17,8,86,16,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "394f4f27c88faa6abd8e21c599d05faa9986b63c7f6c3124e4f334909f36fd06");
        }

        private static void Case_00876()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 876,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,6,61,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-16,35,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,4,59,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-18,40,5,4), new GeneratedEnemyUnit(-18,-6,92,44,4), new GeneratedEnemyUnit(-4,-9,71,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "b670c132639f6a5c3774adf9723b28116b7ac1e29e7e4b82784bd887a2bf9262");
        }

        private static void Case_00877()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 877,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-13,60,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-16,87,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-8,95,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,7,91,22,2), new GeneratedEnemyUnit(-17,-5,63,21,4), new GeneratedEnemyUnit(-17,15,5,34,3), new GeneratedEnemyUnit(14,0,46,37,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "cbfdd122e308460bd872ae147250ca856ba91414e472a98680e5ebb13c7d4129");
        }

        private static void Case_00878()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 878,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-1,24,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,20,76,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-12,42,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,2,8,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-14,30,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,19,12,25,2), new GeneratedEnemyUnit(4,-20,28,12,2), new GeneratedEnemyUnit(12,8,82,24,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "d9b9d4129976b0954cab499e53537ea2c05e1e7b8c55688951dce07cf676bbf8");
        }

        private static void Case_00879()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 879,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,11,71,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,2,77,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,12,25,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,15,71,6,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "47a65afacee7461ef132ae78cc4912c5aa9b1c5e66942c79f06f55c575ed8b57");
        }

        private static void Case_00880()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 880,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-16,56,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,12,15,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,19,96,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,15,12,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,12,95,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-8,79,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,6,64,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-15,8,32,3), new GeneratedEnemyUnit(2,9,28,29,2), new GeneratedEnemyUnit(16,10,90,25,3), new GeneratedEnemyUnit(9,3,9,41,4), new GeneratedEnemyUnit(-13,-13,89,47,1), new GeneratedEnemyUnit(9,6,90,36,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "918bbbb9664b26b5bd8ad6236b0fde5124729660403ebe655134d4107ec92ba5");
        }

        private static void Case_00881()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 881,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-5,29,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-16,51,1,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "975994f1531298a8e53d2921aa133a9264ace5006d91783f8d5df50c0fc96b23");
        }

        private static void Case_00882()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 882,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,9,34,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,10,8,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,17,61,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,5,36,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,13,14,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-3,14,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,15,24,23,2), new GeneratedEnemyUnit(11,-18,42,39,2), new GeneratedEnemyUnit(-5,1,77,42,2), new GeneratedEnemyUnit(6,5,80,10,4), new GeneratedEnemyUnit(-14,5,32,13,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "6719a3a297c9e67818045e2586cd27dcb71336ad4fcd9c9b06059ce6f8e7a4af");
        }

        private static void Case_00883()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 883,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,17,19,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,14,76,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-18,6,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,18,55,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-2,89,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-16,18,41,1), new GeneratedEnemyUnit(12,14,22,30,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a19e9d846574aee92e2b1eafcce5459f35c3b6994cc98649128afe5da352fa99");
        }

        private static void Case_00884()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 884,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,9,5,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,18,14,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,7,76,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-4,66,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-8,28,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-20,48,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-11,11,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-5,85,22,4), new GeneratedEnemyUnit(-14,-7,76,34,3), new GeneratedEnemyUnit(17,12,46,19,4), new GeneratedEnemyUnit(20,13,80,3,2), new GeneratedEnemyUnit(13,-15,94,41,2), new GeneratedEnemyUnit(10,1,65,10,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "5449a480e78b07f42fa59cf3e06f5c9a73d0db9dc8d4f57023dc764eac209392");
        }

        private static void Case_00885()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 885,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,16,47,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-16,59,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-17,33,13,4), new GeneratedEnemyUnit(20,1,78,42,3), new GeneratedEnemyUnit(11,6,81,16,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "6801883230fac963b3ea627d0f144765942c17729499d96dcec39a67aba5220a");
        }

        private static void Case_00886()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 886,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-6,66,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-13,73,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,1,47,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-15,53,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,20,48,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-3,52,42,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "5bff29335964ab0edca25678ac6034cffd4552bef79a37c2dff0eb719467714e");
        }

        private static void Case_00887()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 887,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,13,7,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-13,49,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-13,49,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,8,52,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-10,18,50,4), new GeneratedEnemyUnit(8,17,57,18,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "dd43a675cdbcae56736f0421d09841e0f5a6802c67ede5030e542257c00a94f7");
        }

        private static void Case_00888()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 888,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-5,28,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,1,42,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,13,55,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-1,94,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,3,25,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,17,87,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-11,46,29,4), new GeneratedEnemyUnit(5,5,25,29,1), new GeneratedEnemyUnit(-7,4,76,20,3), new GeneratedEnemyUnit(2,19,40,39,2), new GeneratedEnemyUnit(-19,11,14,25,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "786b272969df047a5181652176cac591a1db8049924475d02b39c8707bb696e5");
        }

        private static void Case_00889()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 889,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,20,84,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-16,63,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,9,64,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-8,80,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,8,22,4,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "675359250e62e0518b1c14f4e0ccd05279842bdc4e67e58b345d34d1c81083ee");
        }

        private static void Case_00890()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 890,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-18,74,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-4,86,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-15,58,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-15,82,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-15,90,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,10,47,34,4), new GeneratedEnemyUnit(2,1,10,3,1), new GeneratedEnemyUnit(20,-10,81,38,3), new GeneratedEnemyUnit(-2,16,13,33,4), new GeneratedEnemyUnit(16,1,5,29,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "198284d15e453cb796cc44c7ca231b688a28f99e71017175d6d61ccf0abc7bbf");
        }

        private static void Case_00891()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 891,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,9,92,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-4,75,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,2,58,23,3), new GeneratedEnemyUnit(4,4,9,2,3), new GeneratedEnemyUnit(4,-19,87,49,4), new GeneratedEnemyUnit(17,-9,91,19,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "39fd6288d23f3373bf418c7ffd6aae8b74248f201870a305207c7d2c6e6e2dfb");
        }

        private static void Case_00892()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 892,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-18,67,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-11,35,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "795b4200788b908d0195a54e71908e504e0365e87d39d36881571f6dc8fb40c1");
        }

        private static void Case_00893()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 893,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,13,45,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,3,28,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,19,16,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,4,50,2,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "7674a85f4bb9b5ace4383f61bf391bf478ad6045b55ae47addbaedfce59c5fb8");
        }

        private static void Case_00894()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 894,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,8,83,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-7,54,6,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "034b4f7857ae369746b90004df7a947b55245775d437957d42ad4c425a4e3586");
        }

        private static void Case_00895()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 895,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-12,47,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-13,19,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-8,36,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,18,85,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,0,50,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-16,32,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,6,63,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-12,35,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-20,94,15,3), new GeneratedEnemyUnit(18,1,20,29,1), new GeneratedEnemyUnit(7,-6,43,45,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "a6d02502d1379d1aa66804ccbeb7db006664f39b65b65b2e1277e28b2f8a77b1");
        }

        private static void Case_00896()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 896,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-10,87,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-17,41,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-7,45,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,3,89,50,1), new GeneratedEnemyUnit(-12,10,75,9,1), new GeneratedEnemyUnit(-11,14,6,21,4), new GeneratedEnemyUnit(20,20,12,12,4), new GeneratedEnemyUnit(-13,5,28,11,2), new GeneratedEnemyUnit(14,-7,40,24,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "62289938840cc4aaab20ca8840a51f8566304590921b3d36bc406f1926603717");
        }

        private static void Case_00897()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 897,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,4,78,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,3,9,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,8,89,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-2,89,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,1,65,5,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "b1c74775b9b0627942dabd68d8c84129c62a99aac04cceb33748d4b062ec370c");
        }

        private static void Case_00898()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 898,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,6,63,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,20,33,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-20,60,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-6,33,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-9,89,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,15,96,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-19,23,22,3), new GeneratedEnemyUnit(-7,-19,74,23,1), new GeneratedEnemyUnit(4,-15,24,47,4), new GeneratedEnemyUnit(15,4,32,37,1), new GeneratedEnemyUnit(-18,-12,42,5,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "7e5a2ada5684faa91dda24e8ef401d5e2cc8b2d81567ddf1cca87f5eef73c6d8");
        }

        private static void Case_00899()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 899,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,14,84,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-9,5,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,0,25,9,1), new GeneratedEnemyUnit(4,-13,5,49,2), new GeneratedEnemyUnit(18,5,46,44,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "5e6947530c79c24d533d2cb44e5bdb895f822bd96637a24399d1e3172d96a2a7");
        }

    }
}
